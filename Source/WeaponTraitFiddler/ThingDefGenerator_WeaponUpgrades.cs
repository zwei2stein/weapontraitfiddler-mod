using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    public static class ThingDefGenerator_WeaponUpgrades
    {
        private const string Tag = "WeaponTraitFiddler_WeaponUpgrade";
        private const int BaseMarketValue = 100;

        private static readonly List<string> TexturedWeaponCategory = new List<string>
        {
            "Sighted", "BurstFire", "BeamWeapon", "BulletFiring", "PelletFiring", "Bow", "PulseCharge", "Scoped",
            "Attachable", "LowStoppingPower", "Ranged", "Pistol", "Rifle", "Shotgun", "Gun", "BladeLink"
        };

        private static readonly List<String> TL_Neolithic = new List<string> { "Bow" };
        
        private static readonly List<String> TL_Ultra = new List<string> { "BladeLink" };
        
        private static readonly List<String> TL_Industrial = new List<string> { "Sighted", "BurstFire", "BeamWeapon", "BulletFiring", "PelletFiring", "PulseCharge", "Scoped",
            "Attachable", "LowStoppingPower", "Ranged", "Pistol", "Rifle", "Shotgun", "Gun" };

        public static IEnumerable<ThingDef> ImpliedWeaponUpgradeDefs(bool hotReload = false)
        {
            foreach (var weaponTraitDef in DefDatabase<WeaponTraitDef>.AllDefsListForReading)
            {
                var defName = "WeaponTraitFiddler_WeaponUpgrade_" + weaponTraitDef.defName;

                var def = hotReload
                    ? DefDatabase<ThingDef>.GetNamed(defName, false) ?? new ThingDef()
                    : new ThingDef();

                def.defName = defName;

                def.resourceReadoutPriority = ResourceCountPriority.Uncounted;
                def.drawerType = DrawerType.MapMeshOnly;
                def.category = ThingCategory.Item;
                def.thingClass = typeof(ThingWithComps);

                def.thingCategories = new List<ThingCategoryDef>
                    { WeaponTraitFiddlerDefOf.WeaponTraitFiddler_WeaponUpgrades };
                
                var texPathSuffix = "";
                if (TexturedWeaponCategory.Contains(weaponTraitDef.weaponCategory.defName))
                    texPathSuffix = "_" + weaponTraitDef.weaponCategory.defName;
                else
                    Log.Message("[Weapon Trait Fiddler] Weapon category without item icon: " + weaponTraitDef.weaponCategory.defName + ", using default.");

                if (TL_Neolithic.Contains(weaponTraitDef.weaponCategory.defName))
                    def.techLevel = TechLevel.Neolithic;
                else if (TL_Ultra.Contains(weaponTraitDef.weaponCategory.defName))
                    def.techLevel = TechLevel.Ultra;
                else if (TL_Industrial.Contains(weaponTraitDef.weaponCategory.defName))
                    def.techLevel = TechLevel.Industrial;
                else
                {
                    Log.Message("[Weapon Trait Fiddler] Weapon category without tech level definition: " + weaponTraitDef.weaponCategory.defName + ", using Industrial.");
                    def.techLevel = TechLevel.Industrial;
                }

                def.graphicData = new GraphicData
                {
                    graphicClass = typeof(Graphic_Single),
                    texPath = $"Things/Item/Special/WeaponTraitFiddler_WeaponUpgrade{texPathSuffix}"
                };

                def.useHitPoints = true;
                def.selectable = true;
                def.thingSetMakerTags = new List<string> { Tag };
                def.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
                def.SetStatBaseValue(StatDefOf.Flammability, 0.5f);
                def.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);

                var marketValueFactor = weaponTraitDef.statFactors.GetStatFactorFromList(StatDefOf.MarketValue);
                var marketValue = weaponTraitDef.statOffsets.GetStatFactorFromList(StatDefOf.MarketValue);
                def.SetStatBaseValue(StatDefOf.MarketValue, marketValue + marketValueFactor * BaseMarketValue);

                def.SetStatBaseValue(StatDefOf.Mass, 0.03f);
                def.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);

                def.altitudeLayer = AltitudeLayer.Item;
                def.comps.Add(new CompProperties_Forbiddable());
                def.comps.Add(new CompProperties_WeaponUpgrade
                {
                    trait = weaponTraitDef
                });

                def.tickerType = TickerType.Never;
                def.alwaysHaulable = true;
                def.rotatable = false;
                def.pathCost = 14;
                def.drawGUIOverlay = true;
                def.modContentPack = weaponTraitDef.modContentPack;
                def.tradeTags = new List<string> { Tag, "ExoticMisc" };

                def.description =
                    "WeaponTraitFiddler_WeaponUpgrade_Desc".Translate(
                        weaponTraitDef.Named("TRAIT"), weaponTraitDef.weaponCategory.defName.Named("CATEGORY")) + "\n\n" +
                    weaponTraitDef.LabelCap.Colorize(ColorLibrary.Yellow) + ":\n" + weaponTraitDef.description;
                def.label = "WeaponTraitFiddler_WeaponUpgrade_Label".Translate(
                    weaponTraitDef.Named("TRAIT"));

                yield return def;
            }
        }
    }
}