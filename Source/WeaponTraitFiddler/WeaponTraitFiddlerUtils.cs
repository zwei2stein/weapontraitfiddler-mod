using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
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
            "XER_MediumTableMachining",
            // vanillaexpanded.gravship:
            "VGE_CompactMachiningTable"
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
        
        public static bool ModifyCarriedThingDrawPosAtTable(ref Vector3 drawPos, ref bool flip, TargetIndex tableInd, JobDriver driver)
        {
            var pawn = driver.pawn;
            if (pawn.pather.Moving)
                return false;

            if (pawn.carryTracker.CarriedThing == null)
                return false;

            var placeCell = driver.job.GetTarget(tableInd).Cell;
            if (!placeCell.IsValid || !placeCell.AdjacentToCardinal(pawn.Position))
                return false;

            drawPos = new Vector3(placeCell.x + 0.5f, drawPos.y, placeCell.z + 0.5f);
            return true;
        }

        public static string GetWorkplaceFailMessage()
        {
            if (WeaponTraitFiddlerModSettings.requiresMachiningResearch)
            {
                return "WeaponTraitFiddler_NoMachiningTable".Translate();
            }
            else
            {
                return "WeaponTraitFiddler_NoCraftingSpot".Translate();
            }
        }

        public static IEnumerable<AbstractCompTraitCompanion> GetComps(ThingWithComps weapon)
        {
            var compUniqueWeaponCompanion = weapon.TryGetComp<CompUniqueWeaponCompanion>();
            if (compUniqueWeaponCompanion != null)
                yield return compUniqueWeaponCompanion;

            var compBladelinkWeaponCompanion = weapon.TryGetComp<CompBladelinkWeaponCompanion>();
            if (compBladelinkWeaponCompanion != null)
                yield return compBladelinkWeaponCompanion;

        }
        
        public static void ApplyScheduledUpgrade(ThingWithComps weapon, Pawn actor, Thing upgradeItem = null)
        {
            foreach (var comp in GetComps(weapon))
            {
                if (comp.upgradeItemToAdd == null) continue;

                var closestComponent = upgradeItem;
                if (closestComponent == null || closestComponent.Destroyed || !closestComponent.Spawned
                    || closestComponent.def != comp.upgradeItemToAdd)
                {
                    closestComponent = GenClosest.ClosestThing_Global_Reachable(
                        weapon.PositionHeld,
                        weapon.MapHeld,
                        weapon.MapHeld.listerThings.ThingsMatching(ThingRequest.ForDef(comp.upgradeItemToAdd)),
                        PathEndMode.OnCell,
                        TraverseParms.For(TraverseMode.PassDoors));
                }

                if (closestComponent == null)
                {
                    comp.upgradeItemToAdd = null;
                    continue;
                }

                var upgrade = closestComponent.def.GetCompProperties<CompProperties_WeaponUpgrade>();
                if (upgrade != null && comp.CanAddTrait(upgrade.trait))
                {
                    comp.AddTrait(upgrade.trait);
                    closestComponent.SplitOff(1).Destroy();
                    comp.upgradeItemToAdd = null;
                    ProcessPawnActor(weapon, actor);
                }
                else
                {
                    comp.upgradeItemToAdd = null;
                }
            }
        }

        public static void SalvageScheduledUpgrade(ThingWithComps weapon, Pawn actor)
        {
            foreach (var comp in GetComps(weapon))
            {
                if (comp.upgradeItemToRemove == null) continue;

                var salvagedWeaponUpgrade = ThingMaker.MakeThing(
                    WeaponTraitFiddlerMain.MapTraitsToItems[comp.upgradeItemToRemove]);
                GenPlace.TryPlaceThing(
                    salvagedWeaponUpgrade,
                    weapon.PositionHeld,
                    weapon.MapHeld,
                    ThingPlaceMode.Near);

                comp.RemoveTrait(comp.upgradeItemToRemove);
                comp.upgradeItemToRemove = null;

                ProcessPawnActor(weapon, actor);
            }
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