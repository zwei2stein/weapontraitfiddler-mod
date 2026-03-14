using Verse;

namespace WeaponTraitFiddler
{
    public class WeaponTraitFiddlerModSettings : ModSettings
    {
        public static int weaponTraitMaxCount = 3;
        public static bool weaponCanBeDamagedByOperation = true;
        public static bool requiresMachiningResearch = true;
        
        public static int personaWeaponsTraitMaxCount = 2;
        public static bool personaWeaponsUpgradable = true;
        public static bool personaWeaponsRequireShipComputerCoreResearch = true;

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref weaponTraitMaxCount, "weaponTraitMaxCount", 3);
            Scribe_Values.Look<bool>(ref requiresMachiningResearch, "requiresMachiningResearch", true);
            
            Scribe_Values.Look<bool>(ref personaWeaponsUpgradable, "personaWeaponsUpgradable", true);
            Scribe_Values.Look<bool>(ref personaWeaponsRequireShipComputerCoreResearch, "personaWeaponsRequireShipComputerCoreResearch", true);
            Scribe_Values.Look<int>(ref personaWeaponsTraitMaxCount, "personaWeaponsTraitMaxCount", 2);

            Scribe_Values.Look<bool>(ref weaponCanBeDamagedByOperation, "weaponCanBeDamagedByOperation", true);
            
            base.ExposeData();
        }
    }
}