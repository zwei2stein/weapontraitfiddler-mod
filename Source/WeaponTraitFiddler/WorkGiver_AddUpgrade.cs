using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    public class WorkGiver_AddUpgrade : WorkGiver_Scanner
    {

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced) && (pawn.Faction != Faction.OfPlayer) && (!pawn.RaceProps.Humanlike);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps thingWithComps))
                return null;

            if (!thingWithComps.TryGetComp<CompUniqueWeaponCompanion>(out var compUniqueWeaponCompanion))
                return null;

            if (compUniqueWeaponCompanion.traitToAdd == null)
                return null;

            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Crafting) || pawn.WorkTagIsDisabled(WorkTags.Crafting))
                return null;
            else if (!pawn.CanReach((LocalTargetInfo)(Thing)t, PathEndMode.ClosestTouch, Danger.Some))
                return null;

            var tableMachining = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForDef(WeaponTraitFiddlerUtils.GetWorkplaceThingDef()),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn, Danger.Some),
                validator: (Predicate<Thing>)(thing =>
                    !thing.IsForbidden(pawn) && pawn.CanReserve((LocalTargetInfo)thing)));
            if (tableMachining == null)
                return null;
            
            var closestComponent = GenClosest.ClosestThing_Global_Reachable(
                compUniqueWeaponCompanion.parent.Position,
                compUniqueWeaponCompanion.parent.Map,
                compUniqueWeaponCompanion.parent.Map.listerThings.ThingsMatching(ThingRequest.ForDef(compUniqueWeaponCompanion.traitToAdd)),
                PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.PassDoors));
            if (closestComponent == null)
                return null;

            var job = JobMaker.MakeJob(WeaponTraitFiddlerDefOf.WeaponTraitFiddler_AddUpgrade);
            job.targetA = (LocalTargetInfo)tableMachining;
            job.targetQueueB.Add((LocalTargetInfo) closestComponent);
            job.targetQueueB.Add((LocalTargetInfo) compUniqueWeaponCompanion.parent);
            job.targetC = (LocalTargetInfo)tableMachining.Position;
            return job;

        }

    }
}