using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    public class JobDriver_AddUpgrade : JobDriver
    {
        
        private const TargetIndex TableMachiningInd = TargetIndex.A;
        private const TargetIndex WeaponInd = TargetIndex.B;
        private const TargetIndex UpgradeInd = TargetIndex.C;
        
        protected Thing Weapon => this.job.GetTarget(WeaponInd).Thing;
        protected Thing Upgrade => this.job.GetTarget(UpgradeInd).Thing;
        protected Thing TableMachining => this.job.GetTarget(TableMachiningInd).Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool reservation = this.pawn.Reserve((LocalTargetInfo) (Thing) this.TableMachining, this.job, errorOnFailed: errorOnFailed) ;
            reservation = reservation && this.pawn.Reserve((LocalTargetInfo)this.Weapon, this.job, errorOnFailed: errorOnFailed);
            reservation = reservation && this.pawn.Reserve((LocalTargetInfo)this.Upgrade, this.job, errorOnFailed: errorOnFailed);
            return reservation;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var f = this;
            f.FailOnDespawnedNullOrForbidden<JobDriver_AddUpgrade>(TableMachiningInd);
            f.FailOnBurningImmobile<JobDriver_AddUpgrade>(TableMachiningInd);
            
            this.job.count = 2;
            
            yield return Toils_Reserve.Reserve(WeaponInd);
            yield return Toils_Reserve.Reserve(UpgradeInd);
            
            yield return Toils_Goto
                .GotoThing(UpgradeInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden<Toil>(UpgradeInd)
                .FailOnSomeonePhysicallyInteracting<Toil>(UpgradeInd);
            yield return Toils_Haul
                .StartCarryThing(UpgradeInd, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden<Toil>(UpgradeInd);
            
            yield return Toils_Goto.GotoThing(TableMachiningInd, PathEndMode.InteractionCell);
            
            yield return Toils_Haul.DropCarriedThing();
            
            yield return Toils_Goto
                .GotoThing(WeaponInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden<Toil>(WeaponInd)
                .FailOnSomeonePhysicallyInteracting<Toil>(WeaponInd);
            yield return Toils_Haul
                .StartCarryThing(WeaponInd, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden<Toil>(WeaponInd);
            
            yield return Toils_Goto.GotoThing(TableMachiningInd, PathEndMode.InteractionCell);
            
            yield return Toils_Haul.DropCarriedThing();
            
            var waitToil = Toils_General.Wait(600, TableMachiningInd)
                .PlaySustainerOrSound(SoundDefOf.Recipe_ButcherCorpseMechanoid)
                .FailOnDestroyedNullOrForbidden<Toil>(WeaponInd)
                .FailOnDestroyedNullOrForbidden<Toil>(TableMachiningInd)
                .FailOnCannotTouch<Toil>(TableMachiningInd, PathEndMode.InteractionCell)
                .WithProgressBarToilDelay(TableMachiningInd);
            waitToil.activeSkill = (Func<SkillDef>) (() => SkillDefOf.Crafting);
            yield return waitToil;
            var toil = ToilMaker.MakeToil(nameof (MakeNewToils));
            toil.initAction = () =>
            {
                WeaponTraitFiddlerUtils.ApplyScheduledUpgrade((ThingWithComps)Weapon, f.pawn);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            
            yield return toil;

            
            
        }
    }
}