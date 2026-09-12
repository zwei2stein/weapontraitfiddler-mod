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

            Scribe_Defs.Look(ref upgradeItemToAdd, "traitToAdd");
            Scribe_Defs.Look(ref upgradeItemToRemove, "traitToRemove");
        }
        
        protected override bool FeatureEnabled()
        {
            if (Prefs.DevMode && DebugSettings.godMode)
                return true;
            
            return !WeaponTraitFiddlerModSettings.requiresMachiningResearch
                   || WeaponTraitFiddlerDefOf.Machining.IsFinished;
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
            //sibling.Setup(false);

            // TODO: no real way to reclalculate after trait removal, may affect traits that give reloadable abilties
            // Self corrects with game save-load.
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