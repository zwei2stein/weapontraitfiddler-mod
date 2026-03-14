using RimWorld;
using Verse;

namespace WeaponTraitFiddler
{
    public class CompProperties_WeaponUpgrade : CompProperties
    {
        public WeaponTraitDef trait;

        public CompProperties_WeaponUpgrade()
        {
            compClass = typeof(CompWeaponUpgrade);
        }
    }


}
        
    
