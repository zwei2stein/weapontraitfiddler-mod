using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    // This job is only activated manually by player interaction with item, pawn AI should never pick it up on its own
    public class JobDriver_SalvageUpgrade : JobDriver
    {

        private const TargetIndex TableMachiningInd = TargetIndex.A;
        private const TargetIndex WeaponInd = TargetIndex.B;

        protected Thing Weapon => this.job.GetTarget(WeaponInd).Thing;

        protected Thing TableMachining => this.job.GetTarget(TableMachiningInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve((LocalTargetInfo)(Thing)this.TableMachining, this.job,
                       errorOnFailed: errorOnFailed)
                   && this.pawn.Reserve((LocalTargetInfo)this.Weapon, this.job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var f = this;
            f.FailOnDespawnedNullOrForbidden<JobDriver_SalvageUpgrade>(TableMachiningInd);
            f.FailOnBurningImmobile<JobDriver_SalvageUpgrade>(TableMachiningInd);

            this.job.count = 1;
            yield return Toils_Reserve.Reserve(WeaponInd);
            yield return Toils_Goto
                .GotoThing(WeaponInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden<Toil>(WeaponInd)
                .FailOnSomeonePhysicallyInteracting<Toil>(WeaponInd);
            yield return Toils_Haul
                .StartCarryThing(WeaponInd, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden<Toil>(WeaponInd);
            yield return Toils_Goto.GotoThing(TableMachiningInd, PathEndMode.InteractionCell);

            var waitToil = Toils_General.Wait(600, TableMachiningInd)
                .PlaySustainerOrSound(SoundDefOf.Recipe_ButcherCorpseMechanoid)
                .FailOnDestroyedNullOrForbidden<Toil>(WeaponInd)
                .FailOnDestroyedNullOrForbidden<Toil>(TableMachiningInd)
                .FailOnCannotTouch<Toil>(TableMachiningInd, PathEndMode.InteractionCell)
                .WithProgressBarToilDelay(TableMachiningInd);
            waitToil.activeSkill = (Func<SkillDef>)(() => SkillDefOf.Crafting);
            yield return waitToil;

            var toil = ToilMaker.MakeToil(nameof(MakeNewToils));
            toil.initAction = () =>
            {
                WeaponTraitFiddlerUtils.SalvageScheduledUpgrade((ThingWithComps)Weapon, f.pawn);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            yield return toil;

            yield return Toils_Haul.DropCarriedThing();

        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool flip)
        {
            return WeaponTraitFiddlerUtils.ModifyCarriedThingDrawPosAtTable(ref drawPos, ref flip, TableMachiningInd,
                this);
        }

    }

}