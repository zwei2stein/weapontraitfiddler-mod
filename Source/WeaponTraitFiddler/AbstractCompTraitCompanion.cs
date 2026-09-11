using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WeaponTraitFiddler
{
    [StaticConstructorOnStartup]
    public abstract class AbstractCompTraitCompanion : ThingComp
    {
        private static readonly Texture2D
            SalvageTraitTexture = ContentFinder<Texture2D>.Get("UI/Commands/TraitSalvage");

        private static readonly Texture2D AddTraitTexture = ContentFinder<Texture2D>.Get("UI/Commands/TraitAdd");

        public ThingDef traitToAdd;

        public WeaponTraitDef traitToRemove;

        protected abstract bool FeatureEnabled();

        protected abstract List<WeaponTraitDef> TraitsListForReading();

        public abstract bool CanAddTrait(WeaponTraitDef weaponTraitDef);

        public abstract void RemoveTrait(WeaponTraitDef weaponTraitDef);

        public abstract void AddTrait(WeaponTraitDef weaponTraitDef);

        protected abstract int MaxTraitCount();

        private void SalvageTraitMenu()
        {
            var options = new List<FloatMenuOption>();
            foreach (var weaponTrait in TraitsListForReading())
            {
                var option = new FloatMenuOption(weaponTrait.LabelCap,
                    () =>
                    {
                        //Log.Message("selected " + weaponTrait.label);
                        traitToRemove = weaponTrait;
                        traitToAdd = null;

                        if (DebugSettings.godMode)
                            WeaponTraitFiddlerUtils.SalvageScheduledUpgrade(parent, null);
                    });
                option.tooltip = new TipSignal(weaponTrait.description);
                options.Add(option);
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void AddTraitMenu()
        {
            var options = new List<FloatMenuOption>();

            foreach (var weaponUpgradeItemDef in WeaponTraitFiddlerMain.ImpliedWeaponUpgradeDefs)
            {
                var comp = weaponUpgradeItemDef.GetCompProperties<CompProperties_WeaponUpgrade>();
                if (comp == null) continue;
                if (!CanAddTrait(comp.trait)) continue;
                if (!parent.Map.listerThings.AnyThingWithDef(weaponUpgradeItemDef)) continue;

                //Log.Message("exists on map " + weaponUpgradeItemDef);

                var option = new FloatMenuOption(weaponUpgradeItemDef.LabelCap,
                    () =>
                    {
                        //Log.Message("selected " + weaponUpgradeItemDef.label);
                        traitToAdd = weaponUpgradeItemDef;
                        traitToRemove = null;

                        if (DebugSettings.godMode)
                            WeaponTraitFiddlerUtils.ApplyScheduledUpgrade(parent, null);
                    });
                option.tooltip = new TipSignal(weaponUpgradeItemDef.description);

                options.Add(option);
            }

            if (options.Empty())
            {
                var option = new FloatMenuOption(
                    "WeaponTraitFiddler_WeaponUpgradeNotAvailable".Translate(),
                    null
                );
                option.Disabled = true;
                options.Add(option);
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!FeatureEnabled())
                yield break;
            
            if (!DebugSettings.godMode)
                yield break;
            
            var traitCount = TraitsListForReading().Count;

            // DEBUG gizmo to instantly salvage trait without workbench/job
            if (traitCount > 0)
            {
                var actionSalvageTrait = new Command_Action();
                actionSalvageTrait.defaultLabel = "WeaponTraitFiddler_SalvageTrait_label".Translate();
                actionSalvageTrait.defaultDesc = "WeaponTraitFiddler_SalvageTrait_desc".Translate();
                actionSalvageTrait.icon = SalvageTraitTexture;
                actionSalvageTrait.action = () => SalvageTraitMenu();
                yield return actionSalvageTrait;
            }

            // DEBUG gizmo to instantly add trait without workbench/job
            if (traitCount < MaxTraitCount())
            {
                var actionAddTrait = new Command_Action();
                actionAddTrait.defaultLabel = "WeaponTraitFiddler_AddTrait_label".Translate();
                actionAddTrait.defaultDesc = "WeaponTraitFiddler_AddTrait_desc".Translate();
                actionAddTrait.icon = AddTraitTexture;
                actionAddTrait.action = () => AddTraitMenu();
                yield return actionAddTrait;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!FeatureEnabled()) yield break;
            
            foreach (var traitRemovalJobFloatMenuOption in CreateTraitRemovalJobFloatMenuOptions(selPawn, this))
                yield return traitRemovalJobFloatMenuOption;
            if (TraitsListForReading().Count < MaxTraitCount())
            {
                foreach (var traitAddJobFloatMenuOption in createTraitAddJobFloatMenuOptions(selPawn, this))
                    yield return traitAddJobFloatMenuOption;
            }
        }

        private FloatMenuOption AddTraitFailMenuOption(string reason)
        {
            return new FloatMenuOption(
                (string)("WeaponTraitFiddler_AddTrait_label".Translate() + ": " +
                        reason.CapitalizeFirst()), (Action)null);
        }

        private IEnumerable<FloatMenuOption> createTraitAddJobFloatMenuOptions(Pawn selPawn,
            AbstractCompTraitCompanion compUniqueWeaponCompanion)
        {
            if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Crafting) || selPawn.WorkTagIsDisabled(WorkTags.Crafting))
            {
                yield return AddTraitFailMenuOption(
                    "WillNever".Translate((NamedArgument)"Crafting".TranslateSimple().UncapitalizeFirst()));
                yield break;
            }

            if (!selPawn.CanReach((LocalTargetInfo)(Thing)compUniqueWeaponCompanion.parent,
                    PathEndMode.ClosestTouch, Danger.Some))
            {
                yield return AddTraitFailMenuOption("CannotReach".Translate());
                yield break;
            }

            var tableMachining = WeaponTraitFiddlerUtils.GetBestWorkplace(selPawn);
            if (tableMachining == null)
            {
                yield return AddTraitFailMenuOption(WeaponTraitFiddlerUtils.GetWorkplaceFailMessage());
                yield break;
            }

            foreach (var weaponUpgradeItemDef in WeaponTraitFiddlerMain.ImpliedWeaponUpgradeDefs)
            {
                var comp = weaponUpgradeItemDef.GetCompProperties<CompProperties_WeaponUpgrade>();
                if (comp == null) continue;
                if (!compUniqueWeaponCompanion.CanAddTrait(comp.trait)) continue;
                if (!parent.Map.listerThings.AnyThingWithDef(weaponUpgradeItemDef)) continue;

                var closestComponent = GenClosest.ClosestThing_Global_Reachable(
                    this.parent.Position,
                    this.parent.Map,
                    this.parent.Map.listerThings.ThingsMatching(ThingRequest.ForDef(weaponUpgradeItemDef)),
                    PathEndMode.ClosestTouch,
                    TraverseParms.For(TraverseMode.PassDoors));

                if (closestComponent == null ||
                    !selPawn.CanReach((LocalTargetInfo)closestComponent, PathEndMode.ClosestTouch, Danger.Some))
                {
                    yield return new FloatMenuOption(
                        (string)("WeaponTraitFiddler_AddTraitNameWeapon_label"
                            .Translate(weaponUpgradeItemDef.Named("TRAIT"), compUniqueWeaponCompanion.parent.Named("WEAPON"))
                            + ": " + "CannotReach".Translate().CapitalizeFirst()), (Action)null);
                    continue;
                }

                var job = JobMaker.MakeJob(WeaponTraitFiddlerDefOf.WeaponTraitFiddler_AddUpgrade);
                job.targetA = (LocalTargetInfo)tableMachining;
                job.targetB = (LocalTargetInfo)compUniqueWeaponCompanion.parent;
                job.targetC = (LocalTargetInfo)closestComponent;

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                        (string)"WeaponTraitFiddler_AddTraitNameWeapon_label"
                            .Translate(weaponUpgradeItemDef.Named("TRAIT"),
                                compUniqueWeaponCompanion.parent.Named("WEAPON")),
                        (Action)(() =>
                        {
                            compUniqueWeaponCompanion.traitToAdd = weaponUpgradeItemDef;
                            selPawn.jobs.TryTakeOrderedJob(job);
                        })),
                    selPawn,
                    (LocalTargetInfo)(Thing)compUniqueWeaponCompanion.parent);
            }
        }
        
        private FloatMenuOption RemoveTraitFailMenuOption(string reason)
        {
            return new FloatMenuOption(
                (string)("WeaponTraitFiddler_SalvageTrait_label".Translate() + ": " +
                         reason.CapitalizeFirst()), (Action)null);
        }

        private IEnumerable<FloatMenuOption> CreateTraitRemovalJobFloatMenuOptions(Pawn selPawn,
            AbstractCompTraitCompanion compUniqueWeaponCompanion)
        {
            JobFailReason.Clear();

            if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Crafting) || selPawn.WorkTagIsDisabled(WorkTags.Crafting))
            {
                yield return RemoveTraitFailMenuOption("WillNever".Translate((NamedArgument)"Crafting".TranslateSimple().UncapitalizeFirst()));
                yield break;
            }

            if (!selPawn.CanReach((LocalTargetInfo)(Thing)compUniqueWeaponCompanion.parent,
                    PathEndMode.ClosestTouch, Danger.Some))
            {
                yield return RemoveTraitFailMenuOption("CannotReach".Translate());
                yield break;
            }

            if (!HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, (Thing)compUniqueWeaponCompanion.parent, true))
            {
                var reason = JobFailReason.HaveReason
                    ? JobFailReason.Reason
                    : (string)"WeaponTraitFiddler_UnableToHaulWeapon".Translate();
                JobFailReason.Clear();

                yield return RemoveTraitFailMenuOption(reason.CapitalizeFirst());
                yield break;
            }

            var tableMachining = WeaponTraitFiddlerUtils.GetBestWorkplace(selPawn);
            if (tableMachining == null)
            {
                yield return RemoveTraitFailMenuOption(WeaponTraitFiddlerUtils.GetWorkplaceFailMessage());
                yield break;
            }

            var job = JobMaker.MakeJob(WeaponTraitFiddlerDefOf.WeaponTraitFiddler_SalvageUpgrade);
            job.targetA = (LocalTargetInfo)tableMachining;
            job.targetB = (LocalTargetInfo)compUniqueWeaponCompanion.parent;

            foreach (var traitToRemoveCandidate in compUniqueWeaponCompanion.TraitsListForReading())
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                        "WeaponTraitFiddler_SalvageTraitNameWeapon_label"
                            .Translate(traitToRemoveCandidate.Named("TRAIT"),
                                compUniqueWeaponCompanion.parent.Named("WEAPON")),
                        (Action)(() =>
                        {
                            compUniqueWeaponCompanion.traitToRemove = traitToRemoveCandidate;
                            compUniqueWeaponCompanion.traitToAdd = null;
                            selPawn.jobs.TryTakeOrderedJob(job);
                        })),
                    selPawn,
                    (LocalTargetInfo)(Thing)compUniqueWeaponCompanion.parent);
            }
        }
    }
}