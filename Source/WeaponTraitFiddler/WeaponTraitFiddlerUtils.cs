using RimWorld;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    public static class WeaponTraitFiddlerUtils
    {

        public static ThingDef GetWorkplaceThingDef()
        {
            if (WeaponTraitFiddlerModSettings.requiresMachiningResearch)
            {
                return WeaponTraitFiddlerDefOf.TableMachining;
            }
            else
            {
                return WeaponTraitFiddlerDefOf.CraftingSpot;
            }
        }

        public static string GetWorkplaceFailMessage()
        {
            if (WeaponTraitFiddlerModSettings.requiresMachiningResearch)
            {
                return "WeaponTraitFiddler_NoMachiningTable";
            }
            else
            {
                return "WeaponTraitFiddler_NoCraftingSpot";
            }
        }

        public static AbstractCompTraitCompanion GetComp(ThingWithComps weapon)
        {
            var compUniqueWeaponCompanion = weapon.TryGetComp<CompUniqueWeaponCompanion>();
            if (compUniqueWeaponCompanion != null)
                return compUniqueWeaponCompanion;
            
            var compBladelinkWeaponCompanion = weapon.TryGetComp<CompBladelinkWeaponCompanion>();
            if (compBladelinkWeaponCompanion != null)
                return compBladelinkWeaponCompanion;
            
            return null;
        }
        
        public static void ApplyScheduledUpgrade(ThingWithComps weapon, Pawn actor)
        {
            var comp = GetComp(weapon);
            
            if (comp.traitToAdd == null) return;

            var closestComponent = GenClosest.ClosestThing_Global_Reachable(
                weapon.Position,
                weapon.Map,
                weapon.Map.listerThings.ThingsMatching(ThingRequest.ForDef(comp.traitToAdd)),
                PathEndMode.OnCell,
                TraverseParms.For(TraverseMode.PassDoors));

            var upgrade = closestComponent.def.GetCompProperties<CompProperties_WeaponUpgrade>();
            if (comp.CanAddTrait(upgrade.trait))
            {
                comp.AddTrait(upgrade.trait);
                closestComponent.Destroy();
            }

            comp.traitToAdd = null;

            ProcessPawnActor(weapon, actor);

        }

        public static void SalvageScheduledUpgrade(ThingWithComps weapon, Pawn actor)
        {
            AbstractCompTraitCompanion comp = GetComp(weapon);

            if (comp.traitToRemove == null) return;

            var thing = ThingMaker.MakeThing(
                WeaponTraitFiddlerMain.MapTraitsToItems[comp.traitToRemove]);
            GenPlace.TryPlaceThing(
                thing,
                actor.Position,
                actor.Map,
                ThingPlaceMode.Near);
            
            comp.RemoveTrait(comp.traitToRemove);

            comp.traitToRemove = null;

            ProcessPawnActor(weapon, actor);
            
        }
        
        private static void ProcessPawnActor(ThingWithComps weapon, Pawn actor)
        {
            if (actor != null)
            {
                actor.skills.Learn(SkillDefOf.Crafting, 100);

                if (WeaponTraitFiddlerModSettings.weaponCanBeDamagedByOperation)
                {
                    float failP = 1f - actor.skills.GetSkill(SkillDefOf.Crafting).Level / 20f;
                    if (Rand.Chance(failP))
                    {
                        var damage = GenMath.RoundRandom( (20f - actor.skills.GetSkill(SkillDefOf.Crafting).Level));
                        weapon.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damage));
                        
                        Messages.Message(
                            "WeaponTraitFiddler_pawnMessedUp".Translate((NamedArgument)actor.LabelShort,
                                    (NamedArgument)weapon.LabelCapNoCount, actor.Named("PAWN"), weapon.Named("WEAPON"))
                                .CapitalizeFirst(), (LookTargets) (Thing) actor, MessageTypeDefOf.NegativeEvent);
                    }
                }
                
            }
            else
            {
                if (WeaponTraitFiddlerModSettings.weaponCanBeDamagedByOperation)
                    weapon.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
            }
        }
    }
}