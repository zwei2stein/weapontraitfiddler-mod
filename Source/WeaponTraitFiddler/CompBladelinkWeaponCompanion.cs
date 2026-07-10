using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    public class CompBladelinkWeaponCompanion : AbstractCompTraitCompanion
    {
        
        CompProperties_BladelinkWeaponCompanion Props => (CompProperties_BladelinkWeaponCompanion)props;
        
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Defs.Look(ref traitToAdd, "traitToAdd");
            Scribe_Defs.Look(ref traitToRemove, "traitToRemove");
        }

        protected override bool FeatureEnabled()
        {
            return WeaponTraitFiddlerModSettings.personaWeaponsUpgradable 
                   && (!WeaponTraitFiddlerModSettings.personaWeaponsRequireShipComputerCoreResearch || WeaponTraitFiddlerDefOf.ShipComputerCore.IsFinished)
                   && (!WeaponTraitFiddlerModSettings.requiresMachiningResearch || WeaponTraitFiddlerDefOf.Machining.IsFinished);
        }

        protected override List<WeaponTraitDef> TraitsListForReading()
        {
            if (!parent.TryGetComp<CompBladelinkWeapon>(out var sibling))
                return new List<WeaponTraitDef>();

            return sibling.TraitsListForReading;
        }

        public override bool CanAddTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompBladelinkWeapon>(out var sibling))
                return false;

            //private sibling.CanAddTrait(weaponTraitDef);
            //copy implementation:
            
            if (weaponTraitDef.weaponCategory != WeaponCategoryDefOf.BladeLink)
                return false;
            if (!sibling.TraitsListForReading.NullOrEmpty<WeaponTraitDef>())
            {
                for (int index = 0; index < sibling.TraitsListForReading.Count; ++index)
                {
                    if (weaponTraitDef.Overlaps(sibling.TraitsListForReading[index]))
                        return false;
                }
            }
            return true;
        }

        public override void RemoveTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompBladelinkWeapon>(out var sibling))
                return;
            
            sibling.TraitsListForReading.Remove(weaponTraitDef);
        }

        public override void AddTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompBladelinkWeapon>(out var sibling))
                return;
            
            sibling.TraitsListForReading.Add(weaponTraitDef);
        }

        protected override int MaxTraitCount()
        {
            return WeaponTraitFiddlerModSettings.personaWeaponsTraitMaxCount;
        }
    }
}