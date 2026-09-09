using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    // This job is only activated manually by player interaction with item, pawn AI should never pick it up on its own
    public class WorkGiver_SalvageUpgrade : WorkGiver_Scanner
    {
        
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced) || pawn.Faction != Faction.OfPlayer || !pawn.RaceProps.Humanlike;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps thingWithComps))
                return null;
            
            if (!thingWithComps.TryGetComp<CompUniqueWeaponCompanion>(out var compUniqueWeaponCompanion))
                return null;
            
            if (compUniqueWeaponCompanion.traitToRemove == null)
                return null;
            
            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Crafting) || pawn.WorkTagIsDisabled(WorkTags.Crafting))
                return null;
            else if (!pawn.CanReach((LocalTargetInfo) (Thing) t, PathEndMode.ClosestTouch, Danger.Some))
                return null;
            
            var tableMachining = WeaponTraitFiddlerUtils.GetBestWorkplace(pawn);
            if (tableMachining == null)
                return null;
            
            var job = JobMaker.MakeJob(WeaponTraitFiddlerDefOf.WeaponTraitFiddler_SalvageUpgrade);
            job.targetA = (LocalTargetInfo) tableMachining;
            job.targetB = (LocalTargetInfo) thingWithComps;
            job.targetC = (LocalTargetInfo) tableMachining.Position;
            return job;
            
        }
        
    }
}