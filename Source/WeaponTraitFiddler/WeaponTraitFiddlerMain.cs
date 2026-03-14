using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WeaponTraitFiddler
{
    [StaticConstructorOnStartup]
    public static class WeaponTraitFiddlerMain
    {
        public static readonly IList<ThingDef> ImpliedWeaponUpgradeDefs = new List<ThingDef>();

        public static readonly IDictionary<WeaponTraitDef, ThingDef> MapTraitsToItems =
            new Dictionary<WeaponTraitDef, ThingDef>();

        static WeaponTraitFiddlerMain()
        {
            Log.Message("[WeaponTraitFiddler] loaded! " + ImpliedWeaponUpgradeDefs.Count +
                        " unique upgrades generated fron traits.");
        }
    }

    public class WeaponTraitFiddlerMod : Mod
    {
        
        private WeaponTraitFiddlerModSettings settings;
     
        public WeaponTraitFiddlerMod(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("WeaponTraitFiddler");
            harmony.PatchAll();
            
            this.settings = this.GetSettings<WeaponTraitFiddlerModSettings>();
        }
        
        public override string SettingsCategory()
        {
            return "WeaponTraitFiddlerModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();

            var gapWidth = 12f;

            listingStandard.Begin(inRect);

            listingStandard.GapLine();
            
            listingStandard.Label("WeaponTraitFiddlerSettings_UniqueWeapons_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;
            
            listingStandard.CheckboxLabeled(
                "WeaponTraitFiddlerSettings_requiresMachiningResearch".Translate(),
                ref WeaponTraitFiddlerModSettings.requiresMachiningResearch,
                "WeaponTraitFiddlerSettings_requiresMachiningResearch_tooltip".Translate());
            
            WeaponTraitFiddlerModSettings.weaponTraitMaxCount = Mathf.RoundToInt(listingStandard.SliderLabeled(
                "WeaponTraitFiddlerSettings_weaponTraitMaxCount".Translate() +
                WeaponTraitFiddlerModSettings.weaponTraitMaxCount
                , WeaponTraitFiddlerModSettings.weaponTraitMaxCount, 2f, 5f, 0.5f,
                "WeaponTraitFiddlerSettings_weaponTraitMaxCount_tooltip".Translate()));
            
            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;
            
            listingStandard.GapLine();
            
            listingStandard.Label("WeaponTraitFiddlerSettings_PersonaWeapons_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;
            
            listingStandard.CheckboxLabeled(
                "WeaponTraitFiddlerSettings_personaWeaponsUpgradable".Translate(),
                ref WeaponTraitFiddlerModSettings.personaWeaponsUpgradable,
                "WeaponTraitFiddlerSettings_personaWeaponsUpgradable_tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "WeaponTraitFiddlerSettings_personaWeaponsRequireShipComputerCoreResearch".Translate(),
                ref WeaponTraitFiddlerModSettings.personaWeaponsRequireShipComputerCoreResearch,
                "WeaponTraitFiddlerSettings_personaWeaponsRequireShipComputerCoreResearch_tooltip".Translate());
            
            WeaponTraitFiddlerModSettings.personaWeaponsTraitMaxCount = Mathf.RoundToInt(listingStandard.SliderLabeled(
                "WeaponTraitFiddlerSettings_personaWeaponsTraitMaxCount".Translate() +
                WeaponTraitFiddlerModSettings.personaWeaponsTraitMaxCount
                , WeaponTraitFiddlerModSettings.personaWeaponsTraitMaxCount, 1f, 4f, 0.5f,
                "WeaponTraitFiddlerSettings_personaWeaponsTraitMaxCount_tooltip".Translate()));
            
            
            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;
            
            listingStandard.GapLine();
            
            listingStandard.Label("WeaponTraitFiddlerSettings_General_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;
            
            listingStandard.CheckboxLabeled(
                "WeaponTraitFiddlerSettings_weaponCanBeDamagedByOperation".Translate(),
                ref WeaponTraitFiddlerModSettings.weaponCanBeDamagedByOperation,
                "WeaponTraitFiddlerSettings_weaponCanBeDamagedByOperation_tooltip".Translate());
            
            
            listingStandard.End();
            
            base.DoSettingsWindowContents(inRect);
        }

    }
}