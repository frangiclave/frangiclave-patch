using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using OrbCreationExtensions;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::ContentImporter")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ContentImporter : global::ContentImporter
    {
        private extern void orig_PopulateRecipeList(ArrayList importedRecipes);

        public new void PopulateRecipeList(ArrayList importedRecipes)
        {
            // Set up a dictionary to hold the map property for each recipe
            // This has to be done before because the original function will delete the ID first otherwise,
            // making it impossible to re-associate the property with the recipe.
            var recipeMaps = new Dictionary<string, string>();
            for (var i = 0; i < importedRecipes.Count; i++)
            {
                var recipeData = importedRecipes.GetHashtable(i);
                var id = recipeData["id"].ToString();
                if (!recipeData.ContainsKey("map")) continue;
                recipeMaps[id] = recipeData["map"].ToString();
                recipeMaps.Remove("map");
            }

            // Import all the recipes
            orig_PopulateRecipeList(importedRecipes);

            // Add the map IDs
            foreach (var recipe in Recipes)
            {
                if (!recipeMaps.ContainsKey(recipe.Id)) continue;
                var moddedRecipe = (Recipe) recipe;
                moddedRecipe.MapId = recipeMaps[recipe.Id];
            }
        }

        private extern void orig_PopulateCompendium(ICompendium compendium);

        public new void PopulateCompendium(ICompendium compendium)
        {
            // Populate the compendium with the original content data mixed in with the modded data
            // The mix will be performed by GetContentItems
            orig_PopulateCompendium(compendium);

            // Handle the endings and maps separately, since there is no built-in support for custom endings yet
            var modManager = Registry.Retrieve<ModManager>();
            var moddedCompendium = (Compendium) compendium;
            moddedCompendium.UpdateEndings(modManager.GetContentForCategory("endings"));
            moddedCompendium.UpdateMaps(modManager.GetContentForCategory("maps"));
        }

        private extern ArrayList orig_GetContentItems(string contentOfType);

        private ArrayList GetContentItems(string contentOfType)
        {
            var modManager = Registry.Retrieve<ModManager>();
            var items = orig_GetContentItems(contentOfType);
            var moddedItems = modManager.GetContentForCategory(contentOfType);
            foreach (var moddedItem in moddedItems)
            {
                var moddedItemId = moddedItem.GetString("id");
                Hashtable originalItem = null;
                var parents = new Dictionary<string, Hashtable>();
                var parentsOrder = moddedItem.GetArrayList("extends") ?? new ArrayList();
                foreach (Hashtable item in items)
                {
                    // Check if this item is overwriting an existing item (this will consider only the first matching
                    // item - normally, there should only be one)
                    var itemId = item.GetString("id");
                    if (itemId == moddedItemId && originalItem == null)
                    {
                        originalItem = item;
                    }

                    // Collect all the parents of this modded item so that the full item can be built
                    if (parentsOrder.Contains(itemId))
                    {
                        parents[itemId] = item;
                    }
                }

                // Build the new item, first by copying its parents, then by applying its own specificities
                // If the new item should override an older one, replace that one too
                var newItem = new Hashtable();
                foreach (string parent in parentsOrder)
                {
                    if (!parents.ContainsKey(parent))
                    {
                        Logging.Error($"Unknown parent '{parent}' for '{moddedItemId}', skipping parent");
                        continue;
                    }
                    newItem.AddHashtable(parents[parent], false);
                }
                newItem.AddHashtable(moddedItem, true);
                if (originalItem != null)
                {
                    originalItem.Clear();
                    originalItem.AddHashtable(newItem, true);
                }
                else
                {
                    items.Add(newItem);
                }
            }
            return items;
        }
    }
}
