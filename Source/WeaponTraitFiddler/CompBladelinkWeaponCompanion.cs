using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    public class CompBladelinkWeaponCompanion : AbstractCompTraitCompanion
    {
        CompProperties_BladelinkWeaponCompanion Props => (CompProperties_BladelinkWeaponCompanion)props;

        // Reflect into the private CompBladelinkWeapon.CanAddTrait(WeaponTraitDef)
        // TODO: When rimworld upgrades, see if it is still private.
        private static readonly Func<CompBladelinkWeapon, WeaponTraitDef, bool> VanillaCanAddTrait =
            AccessTools.MethodDelegate<Func<CompBladelinkWeapon, WeaponTraitDef, bool>>(
                AccessTools.Method(typeof(CompBladelinkWeapon), "CanAddTrait", new[] { typeof(WeaponTraitDef) }));

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

            return VanillaCanAddTrait(sibling, weaponTraitDef);
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