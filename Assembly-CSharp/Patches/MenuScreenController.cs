using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using TMPro;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::MenuScreenController")]
    public class MenuScreenController
    {
        [MonoModIgnore]
        public TextMeshProUGUI VersionNumber;
        
        [MonoModIgnore]
        public MenuSubtitle Subtitle;
        
        private extern void orig_InitialiseServices();
        
        private void InitialiseServices()
        {
            // Load all mods and add the manager to the registry for easier access
            var registry = new Registry();
            var modManager = new ModManager();
            modManager.LoadAll();
            registry.Register(modManager);
            
            orig_InitialiseServices();
        }

        private extern void orig_UpdateAndShowMenu();

        private void UpdateAndShowMenu()
        {
            orig_UpdateAndShowMenu();
            
            // Change the version number and subtitle to indicate the game has been modded
            VersionNumber.text += " [M]";
            Subtitle.SetText(Subtitle.SubtitleText.text + " [M]");
        }
    }
}