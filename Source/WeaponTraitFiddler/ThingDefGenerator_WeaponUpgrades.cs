using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    
    public class WeaponUpgradeExtension : DefModExtension
    {
        public TechLevel? techLevel;
        public string texPathSuffix;
        public ThingCategoryDef thingCategory;
    }
    
    public static class ThingDefGenerator_WeaponUpgrades
    {
        private const string Tag = "WeaponTraitFiddler_WeaponUpgrade";
        private const int BaseMarketValue = 100;
        private const TechLevel DefaultTechLevel = TechLevel.Industrial;

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

                var ext = weaponTraitDef.GetModExtension<WeaponUpgradeExtension>();

                if (ext == null)
                {
                    Log.Warning("[Weapon Trait Fiddler] No WeaponUpgradeExtension configured for trait " +
                                weaponTraitDef.defName + "), using defaults.");
                    def.techLevel = DefaultTechLevel;
                    def.thingCategories = new List<ThingCategoryDef> { WeaponTraitFiddlerDefOf.WeaponTraitFiddler_WeaponUpgrades };
                    def.graphicData = new GraphicData
                    {
                        graphicClass = typeof(Graphic_Single),
                        texPath = $"Things/Item/Special/WeaponTraitFiddler_WeaponUpgrade"
                    };
                }
                else
                {
                    var thingCategory = ext.thingCategory ?? WeaponTraitFiddlerDefOf.WeaponTraitFiddler_WeaponUpgrades;
                    def.thingCategories = new List<ThingCategoryDef> { thingCategory };

                    var texPathSuffix = "";
                    if (!string.IsNullOrEmpty(ext.texPathSuffix))
                    {
                        texPathSuffix = ext.texPathSuffix;
                    }
                    else
                    {
                        Log.Warning("[Weapon Trait Fiddler] No texture configured for trait " +
                                    weaponTraitDef.defName + " (category " + weaponTraitDef.weaponCategory.defName +
                                    "), using default icon.");
                    }

                    def.techLevel = ext.techLevel ?? DefaultTechLevel;
                    if (ext.techLevel == null)
                    {
                        Log.Warning("[Weapon Trait Fiddler] No tech level configured for trait " +
                                    weaponTraitDef.defName + " (category " + weaponTraitDef.weaponCategory.defName +
                                    "), using " + DefaultTechLevel + ".");
                    }

                    def.graphicData = new GraphicData
                    {
                        graphicClass = typeof(Graphic_Single),
                        texPath = $"Things/Item/Special/WeaponTraitFiddler_WeaponUpgrade{texPathSuffix}"
                    };
                }

                def.useHitPoints = true;
                def.selectable = true;
                def.thingSetMakerTags = new List<string> { Tag };
                def.stackLimit = 5;
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
                def.comps.Add(new CompProperties_WeaponUpgrade { trait = weaponTraitDef });

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
                def.label = "WeaponTraitFiddler_WeaponUpgrade_Label".Translate(weaponTraitDef.Named("TRAIT"));

                yield return def;
            }
        }
    }
}