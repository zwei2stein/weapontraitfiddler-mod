using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    [StaticConstructorOnStartup]
    public class CompUniqueWeaponCompanion : AbstractCompTraitCompanion
    {

        public CompProperties_UniqueWeaponCompanion Props => (CompProperties_UniqueWeaponCompanion)props;
        
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Defs.Look(ref traitToAdd, "traitToAdd");
            Scribe_Defs.Look(ref traitToRemove, "traitToRemove");
        }
        
        protected override bool FeatureEnabled()
        {
            return !WeaponTraitFiddlerModSettings.requiresMachiningResearch
                   || WeaponTraitFiddlerDefOf.Machining.IsFinished
                   || DebugSettings.godMode;
        }

        protected override List<WeaponTraitDef> TraitsListForReading()
        {
            if (!parent.TryGetComp<CompUniqueWeapon>(out var sibling))
                return new List<WeaponTraitDef>();

            return sibling.TraitsListForReading;
        }

        public override bool CanAddTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompUniqueWeapon>(out var sibling))
                return false;

            return sibling.CanAddTrait(weaponTraitDef);
        }

        public override void RemoveTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompUniqueWeapon>(out var sibling))
                return;

            sibling.TraitsListForReading.Remove(weaponTraitDef);
        }

        public override void AddTrait(WeaponTraitDef weaponTraitDef)
        {
            if (!parent.TryGetComp<CompUniqueWeapon>(out var sibling))
                return;
            
            sibling.AddTrait(weaponTraitDef);
            sibling.Setup(false);
        }

        protected override int MaxTraitCount()
        {
            return WeaponTraitFiddlerModSettings.weaponTraitMaxCount;
        }

    }
}