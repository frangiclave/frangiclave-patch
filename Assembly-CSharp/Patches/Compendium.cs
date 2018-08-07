using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using MonoMod;
using OrbCreationExtensions;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::Compendium")]
    public class patch_Compendium
    {
        private Dictionary<string, Ending> _endings = new Dictionary<string, Ending>();

        public void UpdateEndings(List<Hashtable> endingsData)
        {
            _endings = new Dictionary<string, Ending>();
            foreach (var endingData in endingsData)
            {
                _endings.Add(
                    endingData.GetString("id"), 
                    new Ending(
                        endingData.GetString("id"), 
                        endingData.GetString("title"), 
                        endingData.GetString("description"), 
                        endingData.GetString("image"), 
                        (EndingFlavour) Enum.Parse(typeof(EndingFlavour), endingData.GetString("flavour")), 
                        endingData.GetString("anim"), 
                        null));
            }
        }
        
        private extern Ending orig_GetEndingById(string endingFlag);
        
        public Ending GetEndingById(string endingFlag)
        {
            return _endings.ContainsKey(endingFlag) ? _endings[endingFlag] : orig_GetEndingById(endingFlag);
        }
    }
}