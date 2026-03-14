using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace WeaponTraitFiddler
{
    public class CompWeaponUpgrade : ThingComp
    {
        CompProperties_WeaponUpgrade Props => (CompProperties_WeaponUpgrade)props;

        public override string CompInspectStringExtra()
        {
            if (Props.trait != null)
                return "WeaponTraitFiddler_UpgradeItemDescTrait".Translate() + ": " +
                       Props.trait.label.CapitalizeFirst();

            return base.CompInspectStringExtra();
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (Props.trait != null)
            {
                var trait = Props.trait;

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(trait.LabelCap.Colorize(ColorLibrary.Yellow));
                stringBuilder.AppendLine(":");
                stringBuilder.AppendLine(trait.description);
                
                if (!trait.statOffsets.NullOrEmpty<StatModifier>())
                {
                    stringBuilder.Append(trait.statOffsets
                        .Select<StatModifier, string>((Func<StatModifier, string>)(x =>
                            x.stat.LabelCap + " " +
                            x.stat.Worker.ValueToString(x.value, false, ToStringNumberSense.Offset)))
                        .ToLineList(" - "));
                    stringBuilder.AppendLine();
                }

                if (!trait.statFactors.NullOrEmpty<StatModifier>())
                {
                    stringBuilder.Append(trait.statFactors
                        .Select<StatModifier, string>((Func<StatModifier, string>)(x =>
                            x.stat.LabelCap + " " +
                            x.stat.Worker.ValueToString(x.value, false, ToStringNumberSense.Factor)))
                        .ToLineList(" - "));
                    stringBuilder.AppendLine();
                }

                if (!Mathf.Approximately(trait.burstShotCountMultiplier, 1f))
                    stringBuilder.AppendLine(
                        " - " + "StatsReport_BurstShotCountMultiplier".Translate() + " " +
                        trait.burstShotCountMultiplier.ToStringByStyle(ToStringStyle.PercentZero,
                            ToStringNumberSense.Factor));
                if (!Mathf.Approximately(trait.burstShotSpeedMultiplier, 1f))
                    stringBuilder.AppendLine(
                        " - " + "StatsReport_BurstShotSpeedMultiplier".Translate() + " " +
                        trait.burstShotSpeedMultiplier.ToStringByStyle(ToStringStyle.PercentZero,
                            ToStringNumberSense.Factor));
                if (!Mathf.Approximately(trait.additionalStoppingPower, 0.0f))
                    stringBuilder.AppendLine(
                        " - " + "StatsReport_AdditionalStoppingPower".Translate() + " " +
                        trait.additionalStoppingPower.ToStringByStyle(ToStringStyle.FloatOne,
                            ToStringNumberSense.Offset));
                
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon, (string)"WeaponTraitFiddler_UpgradeItemDescTrait".Translate(),
                    Props.trait.label.CapitalizeFirst(), stringBuilder.ToString(), 1104);
            }
        }
    }
}