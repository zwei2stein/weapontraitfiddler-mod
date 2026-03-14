using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    [DefOf]
    public static class WeaponTraitFiddlerDefOf
    {
        [MayRequireOdyssey]
        public static ThingCategoryDef WeaponTraitFiddler_WeaponUpgrades;
        
        public static ResearchProjectDef Machining;
        
        public static ResearchProjectDef ShipComputerCore; //machine persuasion
        
        public static ThingDef TableMachining;
        
        public static ThingDef CraftingSpot;

        public static JobDef WeaponTraitFiddler_SalvageUpgrade;
        
        public static JobDef WeaponTraitFiddler_AddUpgrade;

        static WeaponTraitFiddlerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WeaponTraitFiddlerDefOf));
        }
    }
}