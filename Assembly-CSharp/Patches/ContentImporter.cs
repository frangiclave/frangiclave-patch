using System.Collections;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using OrbCreationExtensions;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{    
    [MonoModPatch("global::ContentImporter")]
    public class patch_ContentImporter
    {
        private extern void orig_PopulateCompendium(ICompendium compendium);

        public void PopulateCompendium(ICompendium compendium)
        {
            // Populate the compendium with the original content data mixed in with the modded data
            // The mix will be performed by GetContentItems
            orig_PopulateCompendium(compendium);
            
            // Handle the endings separately, since there is no built-in support for custom endings yet
            var modManager = Registry.Retrieve<ModManager>();
            var endings = modManager.GetContentForCategory("endings");
            var compendiumType = compendium.GetType();
            var updateEndingsMethod = compendiumType.GetMethod("UpdateEndings");
            if (updateEndingsMethod == null)
            {
                Logging.Error("Method 'AddEndings' not found on 'Compendium'");
                return;
            }
            updateEndingsMethod.Invoke(compendium, new object[] { endings });
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