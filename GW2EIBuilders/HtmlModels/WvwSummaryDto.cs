using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using System;
using System.Globalization;
using Segment = GW2EIEvtcParser.EIData.GenericSegment<double>;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIBuilders.HtmlModels;

internal class WvwSummaryDto
{
    public string FightTime { get; set; } = "";
    public double FightTimeSeconds { get; set; }
    public string HealStatsNotice { get; set; } = "";
    public int HealAddonPlayerCount { get; set; }
    public int TotalSquadPlayers { get; set; }
    public bool HasHealingData { get; set; }
    public bool HasBarrierData { get; set; }
    public bool HasCrowdControlData { get; set; }
    public List<WvwSummaryMetricRowDto> MetricRows { get; set; } = [];
    public WvwSummarySideDto Squad { get; set; } = new();
    public WvwSummarySideDto Enemy { get; set; } = new();
    public List<WvwSummaryTopPlayerDto> TopDamagePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopStripPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopCleansePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopBarrierPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopHealingPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopCrowdControlPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopEnemyDamagePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopEnemyStripPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopEnemyCrowdControlPlayers { get; set; } = [];
    public List<WvwSummaryMetricRowDto> DownsOutcomeRows { get; set; } = [];
    public List<WvwSummaryMetricRowDto> DownedStateRows { get; set; } = [];
    public List<WvwSummaryDownDetailEntryDto> SquadMaximumVulnerabilityEntries { get; set; } = [];
    public List<WvwSummaryDownDetailEntryDto> EnemyMaximumVulnerabilityEntries { get; set; } = [];
    public List<WvwSummaryDownDetailEntryDto> SquadMaximumBurningEntries { get; set; } = [];
    public List<WvwSummaryDownDetailEntryDto> EnemyMaximumBurningEntries { get; set; } = [];

    public static WvwSummaryDto? Build(ParsedEvtcLog log, PhaseData phase)
    {
        if (log.LogData.Logic.ParseMode != LogLogic.ParseModeEnum.WvW || log.LogData.Logic.Extension != "detailed_wvw")
        {
            return null;
        }

        var squadActors = log.PlayerList
            .Where(player => !player.IsFakeActor && IsActiveInPhase(player, phase))
            .Cast<SingleActor>()
            .ToList();
        var hostilePlayerTargets = GetHostilePlayerTargets(log, phase);
        var hostileDamageTargets = GetHostileDamageTargets(phase);
        int healAddonPlayerCount = GetHealingAddonPlayerCount(log, squadActors);

        var durationInMilliseconds = Math.Max(phase.DurationInMS, 1);
        var durationInSeconds = durationInMilliseconds / 1000.0;

        var squad = BuildSide(log, phase, squadActors, hostileDamageTargets, hostilePlayerTargets, "My Squad");
        var enemy = BuildSide(log, phase, hostilePlayerTargets, squadActors, squadActors, "Enemy Team");
        var squadDownState = BuildDownStateSide(log, phase, squadActors);
        var enemyDownState = BuildDownStateSide(log, phase, hostilePlayerTargets);

        return new WvwSummaryDto
        {
            FightTime = ToDurationString(durationInMilliseconds),
            FightTimeSeconds = Math.Round(durationInSeconds, TimeDigit),
            TotalSquadPlayers = squadActors.Count,
            HealAddonPlayerCount = healAddonPlayerCount,
            HealStatsNotice = BuildHealStatsNotice(healAddonPlayerCount, squadActors.Count),
            HasHealingData = log.CombatData.HasEXTHealing,
            HasBarrierData = log.CombatData.HasEXTBarrier,
            HasCrowdControlData = log.CombatData.HasCrowdControlData,
            Squad = squad,
            Enemy = enemy,
            MetricRows = BuildMetricRows(durationInMilliseconds, squad, enemy),
            DownsOutcomeRows = BuildDownsOutcomeRows(squadDownState, enemyDownState),
            DownedStateRows = BuildDownedStateRows(squadDownState, enemyDownState),
            SquadMaximumVulnerabilityEntries = squadDownState.MaximumVulnerabilityEntries,
            EnemyMaximumVulnerabilityEntries = enemyDownState.MaximumVulnerabilityEntries,
            SquadMaximumBurningEntries = squadDownState.MaximumBurningEntries,
            EnemyMaximumBurningEntries = enemyDownState.MaximumBurningEntries,
            TopDamagePlayers = BuildTopDamagePlayers(log, squadActors, hostilePlayerTargets, phase),
            TopStripPlayers = BuildTopStripPlayers(log, squadActors, phase),
            TopCleansePlayers = BuildTopCleansePlayers(log, squadActors, phase),
            TopBarrierPlayers = BuildTopBarrierPlayers(log, squadActors, phase),
            TopHealingPlayers = BuildTopHealingPlayers(log, squadActors, phase),
            TopCrowdControlPlayers = BuildTopCrowdControlPlayers(log, squadActors, hostilePlayerTargets, actor => GetFriendlyPlayerIndex(log, actor), phase),
            TopEnemyDamagePlayers = BuildTopEnemyDamagePlayers(log, phase, hostilePlayerTargets, squadActors),
            TopEnemyStripPlayers = BuildTopEnemyStripPlayers(log, hostilePlayerTargets, phase),
            TopEnemyCrowdControlPlayers = BuildTopCrowdControlPlayers(log, hostilePlayerTargets, squadActors, actor => GetTargetIndex(log, actor), phase),
        };
    }

    private static bool IsActiveInPhase(SingleActor actor, PhaseData phase)
    {
        return actor.FirstAware < phase.End && actor.LastAware > phase.Start;
    }

    private static List<SingleActor> GetHostilePlayerTargets(ParsedEvtcLog log, PhaseData phase)
    {
        return phase.Targets.Keys
            .Where(target =>
                !target.IsFakeActor &&
                target.AgentItem.Type == AgentItem.AgentType.NonSquadPlayer &&
                !target.IsSpecies(TargetID.WorldVersusWorld) &&
                IsActiveInPhase(target, phase))
            .ToList();
    }

    private static List<SingleActor> GetHostileDamageTargets(PhaseData phase)
    {
        return phase.Targets.Keys
            .Where(target =>
                !target.IsFakeActor &&
                IsActiveInPhase(target, phase) &&
                (
                    target.AgentItem.Type == AgentItem.AgentType.NonSquadPlayer ||
                    target.IsSpecies(TargetID.WorldVersusWorld)
                ))
            .ToList();
    }

    private static WvwSummarySideDto BuildSide(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> actors,
        IReadOnlyList<SingleActor> damageTargets,
        IReadOnlyList<SingleActor> playerTargets,
        string label)
    {
        var result = new WvwSummarySideDto
        {
            Label = label,
            PlayerCount = actors.Count,
        };

        foreach (SingleActor actor in actors)
        {
            DefenseAllStatistics defense = actor.GetDefenseStats(log, phase.Start, phase.End);
            SupportStatistics support = actor.GetToAllySupportStats(log, phase.Start, phase.End);

            result.BoonStrips += support.BoonStripCount;
            result.Cleanses += defense.ConditionCleanses;
            result.Resurrects += support.ResurrectCount;
            result.DamageTaken += defense.DamageTaken;
            result.Deaths += defense.DeadCount;
            result.ReceivedCrowdControl += defense.ReceivedCrowdControl;

            foreach (SingleActor target in damageTargets)
            {
                DamageStatistics damage = actor.GetDamageStats(target, log, phase.Start, phase.End);
                result.Damage += damage.Damage;
            }

            foreach (SingleActor target in playerTargets)
            {
                OffensiveStatistics offensive = actor.GetOffensiveStats(target, log, phase.Start, phase.End);
                result.Downs += offensive.DownedCount;
                result.Kills += offensive.KilledCount;
            }
        }

        double durationInSeconds = Math.Max(phase.DurationInMS / 1000.0, 1.0);
        double durationInMinutes = durationInSeconds / 60.0;

        result.Dps = Math.Round(result.Damage / durationInSeconds, 1);
        result.Kills = Math.Min(result.Kills, result.Downs);
        result.DownKillConversionRate = result.Downs > 0 ? Math.Round(100.0 * result.Kills / result.Downs, 1) : 0.0;
        result.StripsPerMinute = Math.Round(result.BoonStrips / durationInMinutes, 1);
        result.CleansesPerMinute = Math.Round(result.Cleanses / durationInMinutes, 1);

        return result;
    }

    private static List<WvwSummaryMetricRowDto> BuildMetricRows(long durationInMilliseconds, WvwSummarySideDto squad, WvwSummarySideDto enemy)
    {
        return
        [
            new WvwSummaryMetricRowDto("Fight Time", ToDurationString(durationInMilliseconds), ToDurationString(durationInMilliseconds), true),
            new WvwSummaryMetricRowDto("Players", squad.PlayerCount.ToString(), enemy.PlayerCount.ToString()),
            new WvwSummaryMetricRowDto("Outgoing Damage", squad.Damage.ToString(), enemy.Damage.ToString(), false, true),
            new WvwSummaryMetricRowDto("DPS", squad.Dps.ToString(CultureInfo.InvariantCulture), enemy.Dps.ToString(CultureInfo.InvariantCulture), false, true, 1),
            new WvwSummaryMetricRowDto("Downs", squad.Downs.ToString(), enemy.Downs.ToString()),
            new WvwSummaryMetricRowDto("Kills", squad.Kills.ToString(), enemy.Kills.ToString()),
            new WvwSummaryMetricRowDto("Down to Kill %", squad.DownKillConversionRate.ToString(CultureInfo.InvariantCulture), enemy.DownKillConversionRate.ToString(CultureInfo.InvariantCulture), false, false, 1, true),
            new WvwSummaryMetricRowDto("Boon Strips", squad.BoonStrips.ToString(), enemy.BoonStrips.ToString()),
            new WvwSummaryMetricRowDto("Strips / Min", squad.StripsPerMinute.ToString(CultureInfo.InvariantCulture), enemy.StripsPerMinute.ToString(CultureInfo.InvariantCulture), false, false, 1),
            new WvwSummaryMetricRowDto("Cleanses", squad.Cleanses.ToString(), enemy.Cleanses.ToString()),
            new WvwSummaryMetricRowDto("Cleanses / Min", squad.CleansesPerMinute.ToString(CultureInfo.InvariantCulture), enemy.CleansesPerMinute.ToString(CultureInfo.InvariantCulture), false, false, 1),
            new WvwSummaryMetricRowDto("Rezzes", squad.Resurrects.ToString(), enemy.Resurrects.ToString()),
            new WvwSummaryMetricRowDto("Damage Taken", squad.DamageTaken.ToString(), enemy.DamageTaken.ToString(), false, true, 0, false, false),
            new WvwSummaryMetricRowDto("Deaths", squad.Deaths.ToString(), enemy.Deaths.ToString(), false, false, 0, false, false),
            new WvwSummaryMetricRowDto("Received CC", squad.ReceivedCrowdControl.ToString(), enemy.ReceivedCrowdControl.ToString(), false, false, 0, false, false),
        ];
    }

    private static List<WvwSummaryMetricRowDto> BuildDownsOutcomeRows(WvwSummaryDownStateSideDto squad, WvwSummaryDownStateSideDto enemy)
    {
        return
        [
            new WvwSummaryMetricRowDto("Downs", squad.Downs.ToString(), enemy.Downs.ToString()),
            new WvwSummaryMetricRowDto("Kill Conversions", squad.KillConversions.ToString(), enemy.KillConversions.ToString()),
            new WvwSummaryMetricRowDto("Downs Converted", FormatPercentage(squad.KillConversionRate, squad.Downs > 0), FormatPercentage(enemy.KillConversionRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Kill Time Avg", FormatDurationSeconds(squad.AverageKillTime), FormatDurationSeconds(enemy.AverageKillTime), false, false, 1, false, false),
            new WvwSummaryMetricRowDto("Kill Time Min", FormatDurationSeconds(squad.MinimumKillTime), FormatDurationSeconds(enemy.MinimumKillTime), false, false, 1, false, false),
            new WvwSummaryMetricRowDto("Kill Time Max", FormatDurationSeconds(squad.MaximumKillTime), FormatDurationSeconds(enemy.MaximumKillTime), false, false, 1, false, false),
            new WvwSummaryMetricRowDto("Rezzes", squad.Rezzes.ToString(), enemy.Rezzes.ToString()),
            new WvwSummaryMetricRowDto("Downs Rezzed", FormatPercentage(squad.RezRate, squad.Downs > 0), FormatPercentage(enemy.RezRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Rez Time Avg", FormatDurationSeconds(squad.AverageRezTime), FormatDurationSeconds(enemy.AverageRezTime), false, false, 1, false, false),
            new WvwSummaryMetricRowDto("Rez Time Min", FormatDurationSeconds(squad.MinimumRezTime), FormatDurationSeconds(enemy.MinimumRezTime), false, false, 1, false, false),
            new WvwSummaryMetricRowDto("Rez Time Max", FormatDurationSeconds(squad.MaximumRezTime), FormatDurationSeconds(enemy.MaximumRezTime), false, false, 1, false, false),
        ];
    }

    private static List<WvwSummaryMetricRowDto> BuildDownedStateRows(WvwSummaryDownStateSideDto squad, WvwSummaryDownStateSideDto enemy)
    {
        return
        [
            new WvwSummaryMetricRowDto("Downed with Poison", FormatPercentage(squad.PoisonRate, squad.Downs > 0), FormatPercentage(enemy.PoisonRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Downed with Immobile", FormatPercentage(squad.ImmobileRate, squad.Downs > 0), FormatPercentage(enemy.ImmobileRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Downed with Vulnerability", FormatPercentage(squad.VulnerabilityRate, squad.Downs > 0), FormatPercentage(enemy.VulnerabilityRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Vulnerability Avg Stacks", FormatStacks(squad.AverageVulnerabilityStacksOnAffectedDowns), FormatStacks(enemy.AverageVulnerabilityStacksOnAffectedDowns), false, false, 1),
            new WvwSummaryMetricRowDto("Vulnerability Min Stacks", FormatStackRangeValue(squad.MinimumVulnerabilityStacks, squad.Downs > 0), FormatStackRangeValue(enemy.MinimumVulnerabilityStacks, enemy.Downs > 0), false, false, 0, false, false),
            new WvwSummaryMetricRowDto("Vulnerability Max Stacks", FormatStackRangeValue(squad.MaximumVulnerabilityStacks, squad.Downs > 0), FormatStackRangeValue(enemy.MaximumVulnerabilityStacks, enemy.Downs > 0), false, false, 0, false, false),
            new WvwSummaryMetricRowDto("Downed with Burning", FormatPercentage(squad.BurningRate, squad.Downs > 0), FormatPercentage(enemy.BurningRate, enemy.Downs > 0), false, false, 1, true),
            new WvwSummaryMetricRowDto("Burning Avg Stacks", FormatStacks(squad.AverageBurningStacksOnAffectedDowns), FormatStacks(enemy.AverageBurningStacksOnAffectedDowns), false, false, 1),
            new WvwSummaryMetricRowDto("Burning Min Stacks", FormatStackRangeValue(squad.MinimumBurningStacks, squad.Downs > 0), FormatStackRangeValue(enemy.MinimumBurningStacks, enemy.Downs > 0), false, false, 0, false, false),
            new WvwSummaryMetricRowDto("Burning Max Stacks", FormatStackRangeValue(squad.MaximumBurningStacks, squad.Downs > 0), FormatStackRangeValue(enemy.MaximumBurningStacks, enemy.Downs > 0), false, false, 0, false, false),
        ];
    }

    private static WvwSummaryDownStateSideDto BuildDownStateSide(ParsedEvtcLog log, PhaseData phase, IReadOnlyList<SingleActor> actors)
    {
        var result = new WvwSummaryDownStateSideDto();
        int downsWithVulnerability = 0;
        long totalVulnerabilityStacksOnAffectedDowns = 0;
        int? minimumVulnerabilityStacks = null;
        int? maximumVulnerabilityStacks = null;
        int downsWithBurning = 0;
        long totalBurningStacksOnAffectedDowns = 0;
        int? minimumBurningStacks = null;
        int? maximumBurningStacks = null;
        foreach (SingleActor actor in actors)
        {
            var (_, downs, _, _) = actor.GetStatus(log);
            IReadOnlyList<AliveEvent> aliveEvents = log.CombatData.GetAliveEvents(actor.AgentItem);
            IReadOnlyList<DeadEvent> deadEvents = log.CombatData.GetDeadEvents(actor.AgentItem);

            foreach (Segment down in downs)
            {
                if (down.Start < phase.Start || down.Start > phase.End)
                {
                    continue;
                }

                result.Downs++;
                long buffCheckTime = Math.Max(log.LogData.LogStart, down.Start - ServerDelayConstant);

                int poisonStacks = GetBuffStacksAtTime(actor, log, Poison, buffCheckTime);
                if (poisonStacks > 0)
                {
                    result.DownsWithPoison++;
                }

                int immobileStacks = GetBuffStacksAtTime(actor, log, Immobile, buffCheckTime);
                if (immobileStacks > 0)
                {
                    result.DownsWithImmobile++;
                }

                int vulnerabilityStacks = GetBuffStacksAtTime(actor, log, Vulnerability, buffCheckTime);
                UpdateStackStats(vulnerabilityStacks, ref downsWithVulnerability, ref totalVulnerabilityStacksOnAffectedDowns, ref minimumVulnerabilityStacks, ref maximumVulnerabilityStacks);
                UpdateMaximumEntries(
                    result.MaximumVulnerabilityEntries,
                    vulnerabilityStacks,
                    maximumVulnerabilityStacks,
                    actor,
                    down.Start,
                    phase.Start);

                int burningStacks = GetBuffStacksAtTime(actor, log, Burning, buffCheckTime);
                UpdateStackStats(burningStacks, ref downsWithBurning, ref totalBurningStacksOnAffectedDowns, ref minimumBurningStacks, ref maximumBurningStacks);
                UpdateMaximumEntries(
                    result.MaximumBurningEntries,
                    burningStacks,
                    maximumBurningStacks,
                    actor,
                    down.Start,
                    phase.Start);

                if (down.End > phase.End)
                {
                    continue;
                }

                double downDurationSeconds = Math.Max(0, down.End - down.Start) / 1000.0;
                if (HasStatusEventAtTime(deadEvents, down.End))
                {
                    result.KillConversions++;
                    result.KillTimes.Add(downDurationSeconds);
                }
                else if (HasStatusEventAtTime(aliveEvents, down.End))
                {
                    result.Rezzes++;
                    result.RezTimes.Add(downDurationSeconds);
                }
            }
        }

        result.DownsWithVulnerability = downsWithVulnerability;
        result.TotalVulnerabilityStacksOnAffectedDowns = totalVulnerabilityStacksOnAffectedDowns;
        result.MinimumVulnerabilityStacks = minimumVulnerabilityStacks;
        result.MaximumVulnerabilityStacks = maximumVulnerabilityStacks;
        result.DownsWithBurning = downsWithBurning;
        result.TotalBurningStacksOnAffectedDowns = totalBurningStacksOnAffectedDowns;
        result.MinimumBurningStacks = minimumBurningStacks;
        result.MaximumBurningStacks = maximumBurningStacks;
        result.KillConversionRate = result.Downs > 0 ? Math.Round(100.0 * result.KillConversions / result.Downs, 1) : 0.0;
        result.RezRate = result.Downs > 0 ? Math.Round(100.0 * result.Rezzes / result.Downs, 1) : 0.0;
        result.PoisonRate = result.Downs > 0 ? Math.Round(100.0 * result.DownsWithPoison / result.Downs, 1) : 0.0;
        result.ImmobileRate = result.Downs > 0 ? Math.Round(100.0 * result.DownsWithImmobile / result.Downs, 1) : 0.0;
        result.VulnerabilityRate = result.Downs > 0 ? Math.Round(100.0 * result.DownsWithVulnerability / result.Downs, 1) : 0.0;
        result.BurningRate = result.Downs > 0 ? Math.Round(100.0 * result.DownsWithBurning / result.Downs, 1) : 0.0;
        result.AverageKillTime = GetAverage(result.KillTimes);
        result.MinimumKillTime = GetMinimum(result.KillTimes);
        result.MaximumKillTime = GetMaximum(result.KillTimes);
        result.AverageRezTime = GetAverage(result.RezTimes);
        result.MinimumRezTime = GetMinimum(result.RezTimes);
        result.MaximumRezTime = GetMaximum(result.RezTimes);
        result.AverageVulnerabilityStacksOnAffectedDowns = result.DownsWithVulnerability > 0 ? Math.Round((double)result.TotalVulnerabilityStacksOnAffectedDowns / result.DownsWithVulnerability, 1) : null;
        result.AverageBurningStacksOnAffectedDowns = result.DownsWithBurning > 0 ? Math.Round((double)result.TotalBurningStacksOnAffectedDowns / result.DownsWithBurning, 1) : null;

        return result;
    }

    private static void UpdateStackStats(int stacks, ref int affectedDowns, ref long totalStacksOnAffectedDowns, ref int? minimumStacks, ref int? maximumStacks)
    {
        if (stacks > 0)
        {
            affectedDowns++;
            totalStacksOnAffectedDowns += stacks;
        }

        minimumStacks = minimumStacks.HasValue ? Math.Min(minimumStacks.Value, stacks) : stacks;
        maximumStacks = maximumStacks.HasValue ? Math.Max(maximumStacks.Value, stacks) : stacks;
    }

    private static void UpdateMaximumEntries(
        List<WvwSummaryDownDetailEntryDto> entries,
        int stacks,
        int? currentMaximum,
        SingleActor actor,
        long downTime,
        long phaseStart)
    {
        if (!currentMaximum.HasValue || stacks <= 0)
        {
            return;
        }

        if (entries.Count == 0 || stacks > entries[0].StackCount)
        {
            entries.Clear();
        }

        if (stacks == currentMaximum.Value)
        {
            entries.Add(new WvwSummaryDownDetailEntryDto
            {
                Name = actor.Character,
                Account = actor.Account,
                Profession = actor.Spec.ToString(),
                Icon = actor.GetIcon(),
                UniqueId = actor.UniqueID,
                StackCount = stacks,
                Time = downTime,
                TimeLabel = ToDurationString(Math.Max(0, downTime - phaseStart)),
            });
        }
    }

    private static int GetBuffStacksAtTime(SingleActor actor, ParsedEvtcLog log, long buffId, long time)
    {
        return (int)Math.Max(0, Math.Round(actor.GetBuffStatus(log, buffId, time).Value));
    }

    private static bool HasStatusEventAtTime<T>(IReadOnlyList<T> statusEvents, long time) where T : StatusEvent
    {
        return statusEvents.Any(evt => Math.Abs(evt.Time - time) <= ServerDelayConstant);
    }

    private static double? GetAverage(IReadOnlyList<double> values)
    {
        return values.Count > 0 ? Math.Round(values.Average(), 1) : null;
    }

    private static double? GetMinimum(IReadOnlyList<double> values)
    {
        return values.Count > 0 ? Math.Round(values.Min(), 1) : null;
    }

    private static double? GetMaximum(IReadOnlyList<double> values)
    {
        return values.Count > 0 ? Math.Round(values.Max(), 1) : null;
    }

    private static string FormatPercentage(double value, bool hasDenominator)
    {
        return hasDenominator ? value.ToString(CultureInfo.InvariantCulture) : "N/A";
    }

    private static string FormatDurationSeconds(double? value)
    {
        return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "N/A";
    }

    private static string FormatStacks(double? value)
    {
        return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "N/A";
    }

    private static string FormatStackRangeValue(int? value, bool hasDowns)
    {
        return hasDowns && value.HasValue ? value.Value.ToString() : "N/A";
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopDamagePlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors, IReadOnlyList<SingleActor> hostilePlayerTargets, PhaseData phase)
    {
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor =>
            {
                long damageToPlayers = 0;
                foreach (SingleActor target in hostilePlayerTargets)
                {
                    DamageStatistics damage = actor.GetDamageStats(target, log, phase.Start, phase.End);
                    damageToPlayers += damage.Damage;
                }
                return damageToPlayers;
            });
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopStripPlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors, PhaseData phase)
    {
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor => actor.GetToAllySupportStats(log, phase.Start, phase.End).BoonStripCount);
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopCleansePlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors, PhaseData phase)
    {
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor => actor.GetToAllySupportStats(log, phase.Start, phase.End).ConditionCleanseCount);
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopBarrierPlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors, PhaseData phase)
    {
        if (!log.CombatData.HasEXTBarrier)
        {
            return [];
        }
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor => actor.EXTBarrier.GetOutgoingBarrierStats(null, log, phase.Start, phase.End).Barrier);
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopHealingPlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors, PhaseData phase)
    {
        if (!log.CombatData.HasEXTHealing)
        {
            return [];
        }
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor => actor.EXTHealing.GetOutgoingHealStats(null, log, phase.Start, phase.End).Healing);
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopEnemyDamagePlayers(ParsedEvtcLog log, PhaseData phase, IReadOnlyList<SingleActor> hostilePlayerTargets, IReadOnlyList<SingleActor> squadActors)
    {
        return BuildTopPlayers(
            hostilePlayerTargets,
            actor => GetTargetIndex(log, actor),
            actor =>
            {
                long damageToSquadPlayers = 0;
                foreach (SingleActor target in squadActors)
                {
                    DamageStatistics damage = actor.GetDamageStats(target, log, phase.Start, phase.End);
                    damageToSquadPlayers += damage.Damage;
                }
                return damageToSquadPlayers;
            });
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopEnemyStripPlayers(ParsedEvtcLog log, IReadOnlyList<SingleActor> hostilePlayerTargets, PhaseData phase)
    {
        return BuildTopPlayers(
            hostilePlayerTargets,
            actor => GetTargetIndex(log, actor),
            actor => actor.GetToAllySupportStats(log, phase.Start, phase.End).BoonStripCount);
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopCrowdControlPlayers(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        IReadOnlyList<SingleActor> targets,
        Func<SingleActor, int> indexGetter,
        PhaseData phase)
    {
        var result = new List<WvwSummaryTopPlayerDto>(Math.Min(5, actors.Count));
        foreach (SingleActor actor in actors)
        {
            int actorIndex = indexGetter(actor);
            if (actorIndex < 0)
            {
                continue;
            }

            var crowdControlEvents = new List<WvwSummaryCrowdControlEventInfo>();
            foreach (SingleActor target in targets)
            {
                foreach (CrowdControlEvent crowdControlEvent in actor.GetJustOutgoingActorCrowdControlEvents(target, log, phase.Start, phase.End))
                {
                    crowdControlEvents.Add(new WvwSummaryCrowdControlEventInfo
                    {
                        Event = crowdControlEvent,
                        Effective = IsCrowdControlEffective(log, target, crowdControlEvent),
                    });
                }
            }

            result.Add(new WvwSummaryTopPlayerDto
            {
                PlayerIndex = actorIndex,
                Name = actor.Character,
                Account = actor.Account,
                Profession = actor.Spec.ToString(),
                Icon = actor.GetIcon(),
                Amount = crowdControlEvents.Count,
                EffectiveAmount = crowdControlEvents.Count(ccEvent => ccEvent.Effective),
                TotalDuration = Math.Round(crowdControlEvents.Sum(ccEvent => ccEvent.Event.Duration) / 1000.0, TimeDigit),
                SkillDetails = BuildCrowdControlSkillDetails(crowdControlEvents),
            });
        }

        return result
            .Where(player => player.Amount > 0)
            .OrderByDescending(player => player.Amount)
            .ThenByDescending(player => player.EffectiveAmount)
            .ThenByDescending(player => player.TotalDuration)
            .ThenBy(player => player.Name, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private static bool IsCrowdControlEffective(ParsedEvtcLog log, SingleActor target, CrowdControlEvent crowdControlEvent)
    {
        long stabilityCheckTime = Math.Max(log.LogData.LogStart, crowdControlEvent.Time - ServerDelayConstant);
        return !target.HasBuff(log, GW2EIEvtcParser.SkillIDs.Stability, stabilityCheckTime);
    }

    private static List<WvwSummarySkillDetailDto> BuildCrowdControlSkillDetails(IReadOnlyList<WvwSummaryCrowdControlEventInfo> crowdControlEvents)
    {
        return crowdControlEvents
            .GroupBy(ccEvent => ccEvent.Event.SkillID)
            .Select(group =>
            {
                WvwSummaryCrowdControlEventInfo firstEvent = group.First();
                return new WvwSummarySkillDetailDto
                {
                    SkillId = firstEvent.Event.SkillID,
                    Name = firstEvent.Event.Skill.Name,
                    Icon = firstEvent.Event.Skill.Icon,
                    Count = group.Count(),
                    EffectiveCount = group.Count(ccEvent => ccEvent.Effective),
                    TotalDuration = Math.Round(group.Sum(ccEvent => ccEvent.Event.Duration) / 1000.0, TimeDigit),
                };
            })
            .OrderByDescending(detail => detail.Count)
            .ThenByDescending(detail => detail.EffectiveCount)
            .ThenByDescending(detail => detail.TotalDuration)
            .ThenBy(detail => detail.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<WvwSummaryTopPlayerDto> BuildTopPlayers(IReadOnlyList<SingleActor> actors, Func<SingleActor, int> indexGetter, Func<SingleActor, long> valueGetter)
    {
        var result = new List<WvwSummaryTopPlayerDto>(Math.Min(5, actors.Count));
        foreach (SingleActor actor in actors)
        {
            int actorIndex = indexGetter(actor);
            if (actorIndex < 0)
            {
                continue;
            }

            result.Add(new WvwSummaryTopPlayerDto
            {
                PlayerIndex = actorIndex,
                Name = actor.Character,
                Account = actor.Account,
                Profession = actor.Spec.ToString(),
                Icon = actor.GetIcon(),
                Amount = valueGetter(actor),
            });
        }

        return result
            .OrderByDescending(player => player.Amount)
            .ThenBy(player => player.Name, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private static int GetFriendlyPlayerIndex(ParsedEvtcLog log, SingleActor actor)
    {
        for (int i = 0; i < log.Friendlies.Count; i++)
        {
            if (log.Friendlies[i] == actor)
            {
                return i;
            }
        }
        return -1;
    }

    private static int GetTargetIndex(ParsedEvtcLog log, SingleActor actor)
    {
        return log.LogData.Logic.Targets.IndexOf(actor);
    }

    private static string BuildHealStatsNotice(int healAddonPlayers, int totalPlayers)
    {
        return $"Heal stats are incomplete: Healing Stats add-on detected for {healAddonPlayers} of {totalPlayers} players in this phase.";
    }

    private static int GetHealingAddonPlayerCount(ParsedEvtcLog log, IReadOnlyList<SingleActor> squadActors)
    {
        var runningHealingAddon = new HashSet<string>(StringComparer.Ordinal);
        ExtensionHandler? healingExtension = log.LogMetadata.UsedExtensions.FirstOrDefault(extension => extension.Name == "Healing Stats");
        if (healingExtension != null)
        {
            if (log.LogMetadata.PoV != null)
            {
                runningHealingAddon.Add(log.FindActor(log.LogMetadata.PoV).Character);
            }
            foreach (AgentItem agent in healingExtension.RunningExtension)
            {
                runningHealingAddon.Add(log.FindActor(agent).Character);
            }
        }
        return squadActors.Count(actor => runningHealingAddon.Contains(actor.Character));
    }
}

internal class WvwSummarySideDto
{
    public string Label { get; set; } = "";
    public int PlayerCount { get; set; }
    public long Damage { get; set; }
    public double Dps { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public double DownKillConversionRate { get; set; }
    public int BoonStrips { get; set; }
    public int Cleanses { get; set; }
    public int Resurrects { get; set; }
    public long DamageTaken { get; set; }
    public int Deaths { get; set; }
    public int ReceivedCrowdControl { get; set; }
    public double StripsPerMinute { get; set; }
    public double CleansesPerMinute { get; set; }
}

internal class WvwSummaryMetricRowDto(string label, string squadValue, string enemyValue, bool equalValues = false, bool integerWithSpaces = false, int decimals = 0, bool percentage = false, bool higherIsBetter = true)
{
    public string Label { get; set; } = label;
    public string SquadValue { get; set; } = squadValue;
    public string EnemyValue { get; set; } = enemyValue;
    public bool EqualValues { get; set; } = equalValues;
    public bool IntegerWithSpaces { get; set; } = integerWithSpaces;
    public int Decimals { get; set; } = decimals;
    public bool Percentage { get; set; } = percentage;
    public bool HigherIsBetter { get; set; } = higherIsBetter;
}

internal class WvwSummaryTopPlayerDto
{
    public int PlayerIndex { get; set; }
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public long Amount { get; set; }
    public int EffectiveAmount { get; set; }
    public double TotalDuration { get; set; }
    public List<WvwSummarySkillDetailDto> SkillDetails { get; set; } = [];
}

internal class WvwSummarySkillDetailDto
{
    public long SkillId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public int EffectiveCount { get; set; }
    public double TotalDuration { get; set; }
}

internal class WvwSummaryCrowdControlEventInfo
{
    public CrowdControlEvent Event { get; set; } = null!;
    public bool Effective { get; set; }
}

internal class WvwSummaryDownStateSideDto
{
    public int Downs { get; set; }
    public int KillConversions { get; set; }
    public int Rezzes { get; set; }
    public int DownsWithPoison { get; set; }
    public int DownsWithImmobile { get; set; }
    public int DownsWithVulnerability { get; set; }
    public int DownsWithBurning { get; set; }
    public long TotalVulnerabilityStacksOnAffectedDowns { get; set; }
    public long TotalBurningStacksOnAffectedDowns { get; set; }
    public int? MinimumVulnerabilityStacks { get; set; }
    public int? MaximumVulnerabilityStacks { get; set; }
    public int? MinimumBurningStacks { get; set; }
    public int? MaximumBurningStacks { get; set; }
    public double KillConversionRate { get; set; }
    public double RezRate { get; set; }
    public double PoisonRate { get; set; }
    public double ImmobileRate { get; set; }
    public double VulnerabilityRate { get; set; }
    public double BurningRate { get; set; }
    public double? AverageKillTime { get; set; }
    public double? MinimumKillTime { get; set; }
    public double? MaximumKillTime { get; set; }
    public double? AverageRezTime { get; set; }
    public double? MinimumRezTime { get; set; }
    public double? MaximumRezTime { get; set; }
    public double? AverageVulnerabilityStacksOnAffectedDowns { get; set; }
    public double? AverageBurningStacksOnAffectedDowns { get; set; }
    public List<double> KillTimes { get; } = [];
    public List<double> RezTimes { get; } = [];
    public List<WvwSummaryDownDetailEntryDto> MaximumVulnerabilityEntries { get; } = [];
    public List<WvwSummaryDownDetailEntryDto> MaximumBurningEntries { get; } = [];
}

internal class WvwSummaryDownDetailEntryDto
{
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public int UniqueId { get; set; }
    public int StackCount { get; set; }
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
}
