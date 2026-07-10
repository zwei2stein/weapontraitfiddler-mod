using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public class DefGeneratorPatch
    {
        private static void Postfix(bool hotReload)
        {
            Log.Message("[WeaponTraitFiddler] generating weapon upgrade defs");
            
            WeaponTraitFiddlerMain.ImpliedWeaponUpgradeDefs.Clear();
            WeaponTraitFiddlerMain.MapTraitsToItems.Clear();

            foreach (var impliedWeaponUpgradeDef in ThingDefGenerator_WeaponUpgrades.ImpliedWeaponUpgradeDefs())
            {
                WeaponTraitFiddlerMain.ImpliedWeaponUpgradeDefs.Add(impliedWeaponUpgradeDef);

                var trait = impliedWeaponUpgradeDef.GetCompProperties<CompProperties_WeaponUpgrade>().trait;
                WeaponTraitFiddlerMain.MapTraitsToItems.Add(trait, impliedWeaponUpgradeDef);

                DefGenerator.AddImpliedDef(impliedWeaponUpgradeDef);
            }
            
            Log.Message("[WeaponTraitFiddler] " + WeaponTraitFiddlerMain.ImpliedWeaponUpgradeDefs.Count +
                        " unique upgrades generated from traits.");
        }
    }
}