using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    public static class WeaponTraitFiddlerUtils
    {
        
        private static readonly List<string> WORKPLACE_DEFS_MODED = new List<string>
        {
            // Mlie.TinyWorkbenchs:
            "TWB_TableMachiningMini", 
            // Xercaine.Furniture.Small:
            "XER_SmallTableMachining",
            "XER_MediumTableMachining"
        };
        private static readonly List<ThingDef> moddedWorkplaceDefs = new List<ThingDef>();

        static WeaponTraitFiddlerUtils()
        {
            // Pre-cache modded workplace ThingDefs ONCE at startup
            foreach (var thingDefName in WORKPLACE_DEFS_MODED)
            {
                var def = DefDatabase<ThingDef>.GetNamed(thingDefName, false);
                if (def != null)
                    moddedWorkplaceDefs.Add(def);
            }
        }
        
        public static IEnumerable<ThingDef> GetWorkplaceThingDef()
        {
            yield return WeaponTraitFiddlerDefOf.TableMachining;

            foreach (var thingDef in moddedWorkplaceDefs)
                yield return thingDef;
            
            if (!WeaponTraitFiddlerModSettings.requiresMachiningResearch)
                yield return WeaponTraitFiddlerDefOf.CraftingSpot;
        }

        public static Thing GetBestWorkplace(Pawn selPawn)
        {
            if (selPawn == null) return null;
            
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            List<ThingDef> workplaceDefs = new List<ThingDef>(GetWorkplaceThingDef());
            
            //Log.Message("Workplacedefs: " + workplaceDefs.ToString());
            
            Predicate<Thing> validator = (thing) => workplaceDefs.Contains(thing.def)
                                                    && !thing.IsForbidden(selPawn)
                                                    && selPawn.CanReserve((LocalTargetInfo)thing);
            
            return GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map,
                    request,
                    PathEndMode.InteractionCell,
                    TraverseParms.For(selPawn, Danger.Some),
                    validator: validator);

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
                closestComponent.SplitOff(1).Destroy();
            }

            comp.traitToAdd = null;

            ProcessPawnActor(weapon, actor);

        }

        public static void SalvageScheduledUpgrade(ThingWithComps weapon, Pawn actor)
        {
            AbstractCompTraitCompanion comp = GetComp(weapon);

            if (comp.traitToRemove == null) return;

            var salvagedWeaponUpgrade = ThingMaker.MakeThing(
                WeaponTraitFiddlerMain.MapTraitsToItems[comp.traitToRemove]);
            GenPlace.TryPlaceThing(
                salvagedWeaponUpgrade,
                weapon.Position,
                weapon.Map,
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