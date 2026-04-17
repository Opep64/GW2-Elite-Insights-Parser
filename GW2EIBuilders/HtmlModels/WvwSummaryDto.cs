using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using System;
using System.Globalization;
using System.Numerics;
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
    public int FriendlyPlayerCount { get; set; }
    public double EffectiveAlliedPlayerCount { get; set; }
    public bool HasHealingData { get; set; }
    public bool HasBarrierData { get; set; }
    public bool HasCrowdControlData { get; set; }
    public WvwSummaryGradeDto SquadGrade { get; set; } = new();
    public WvwSummaryOppositionEstimateDto OppositionEstimate { get; set; } = new();
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
    public List<WvwSummaryDownEventEntryDto> SquadDownEntries { get; set; } = [];
    public List<WvwSummaryDownEventEntryDto> SquadKillConversionEntries { get; set; } = [];
    public List<WvwSummaryDownEventEntryDto> SquadRezEntries { get; set; } = [];
    public List<WvwSummaryDownEventEntryDto> EnemyDownEntries { get; set; } = [];
    public List<WvwSummaryDownEventEntryDto> EnemyKillConversionEntries { get; set; } = [];
    public List<WvwSummaryDownEventEntryDto> EnemyRezEntries { get; set; } = [];

    public static WvwSummaryDto? Build(ParsedEvtcLog log, PhaseData phase, CombatReplayAnalysisDto? combatReplayAnalysis = null)
    {
        if (log.LogData.Logic.ParseMode != LogLogic.ParseModeEnum.WvW || log.LogData.Logic.Extension != "detailed_wvw")
        {
            return null;
        }

        var squadActors = log.PlayerList
            .Where(player => !player.IsFakeActor && IsActiveInPhase(player, phase))
            .Cast<SingleActor>()
            .ToList();
        var friendlyActors = GetFriendlyPlayerActors(log, phase);
        var hostilePlayerTargets = GetHostilePlayerTargets(log, phase);
        var hostileDamageTargets = GetHostileDamageTargets(phase);
        int healAddonPlayerCount = GetHealingAddonPlayerCount(log, squadActors);
        double effectiveAlliedPlayerCount = squadActors.Count + friendlyActors.Count / 3.0;

        var durationInMilliseconds = Math.Max(phase.DurationInMS, 1);
        var durationInSeconds = durationInMilliseconds / 1000.0;

        var squad = BuildSide(log, phase, squadActors, hostileDamageTargets, hostilePlayerTargets, "Our Squad");
        var enemy = BuildSide(log, phase, hostilePlayerTargets, squadActors, squadActors, "Enemy Team");
        var squadDownState = BuildDownStateSide(log, phase, squadActors);
        var enemyDownState = BuildDownStateSide(log, phase, hostilePlayerTargets);

        return new WvwSummaryDto
        {
            FightTime = ToDurationString(durationInMilliseconds),
            FightTimeSeconds = Math.Round(durationInSeconds, TimeDigit),
            TotalSquadPlayers = squadActors.Count,
            FriendlyPlayerCount = friendlyActors.Count,
            EffectiveAlliedPlayerCount = Math.Round(effectiveAlliedPlayerCount, 1),
            HealAddonPlayerCount = healAddonPlayerCount,
            HealStatsNotice = BuildHealStatsNotice(healAddonPlayerCount, squadActors.Count),
            HasHealingData = log.CombatData.HasEXTHealing,
            HasBarrierData = log.CombatData.HasEXTBarrier,
            HasCrowdControlData = log.CombatData.HasCrowdControlData,
            SquadGrade = BuildSquadGrade(log, phase, combatReplayAnalysis, squadActors, hostilePlayerTargets, squad, enemy, squadDownState, enemyDownState, effectiveAlliedPlayerCount, friendlyActors.Count),
            OppositionEstimate = BuildOppositionEstimate(log, phase, combatReplayAnalysis, squadActors, hostilePlayerTargets, squad, enemy, squadDownState, enemyDownState, effectiveAlliedPlayerCount),
            Squad = squad,
            Enemy = enemy,
            MetricRows = BuildMetricRows(durationInMilliseconds, squad, enemy, friendlyActors.Count),
            DownsOutcomeRows = BuildDownsOutcomeRows(squadDownState, enemyDownState),
            DownedStateRows = BuildDownedStateRows(squadDownState, enemyDownState),
            SquadMaximumVulnerabilityEntries = squadDownState.MaximumVulnerabilityEntries,
            EnemyMaximumVulnerabilityEntries = enemyDownState.MaximumVulnerabilityEntries,
            SquadMaximumBurningEntries = squadDownState.MaximumBurningEntries,
            EnemyMaximumBurningEntries = enemyDownState.MaximumBurningEntries,
            SquadDownEntries = squadDownState.DownEntries.OrderBy(entry => entry.Time).ToList(),
            SquadKillConversionEntries = squadDownState.KillConversionEntries.OrderBy(entry => entry.Time).ToList(),
            SquadRezEntries = squadDownState.RezEntries.OrderBy(entry => entry.Time).ToList(),
            EnemyDownEntries = enemyDownState.DownEntries.OrderBy(entry => entry.Time).ToList(),
            EnemyKillConversionEntries = enemyDownState.KillConversionEntries.OrderBy(entry => entry.Time).ToList(),
            EnemyRezEntries = enemyDownState.RezEntries.OrderBy(entry => entry.Time).ToList(),
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

    private static WvwSummaryGradeDto BuildSquadGrade(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummarySideDto squad,
        WvwSummarySideDto enemy,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState,
        double effectiveAlliedPlayerCount,
        int friendlyPlayerCount)
    {
        double squadPlayers = Math.Max(squad.PlayerCount, 1);
        double enemyPlayers = Math.Max(enemy.PlayerCount, 1);
        WvwSummaryCohesionEstimateDto cohesion = BuildGroupCohesionEstimate(log, phase, squadActors, hostilePlayerTargets);
        WvwSummaryBurstMetricsDto squadBurst = BuildBurstMetrics(phase, combatReplayAnalysis, combatReplayAnalysis?.Squad, squad.PlayerCount);
        WvwSummaryBurstMetricsDto enemyBurst = BuildBurstMetrics(phase, combatReplayAnalysis, combatReplayAnalysis?.Enemy, enemy.PlayerCount);

        var weightedSubGrades = new List<(WvwSummarySubGradeDto SubGrade, double Weight)>
        {
            (
                new WvwSummarySubGradeDto
                {
                    Label = "Cohesion",
                    Score = cohesion.Score,
                    Grade = ScoreToGrade(cohesion.Score),
                    Detail = $"{cohesion.StyleLabel}: {cohesion.Detail}",
                },
                0.20
            ),
            (
                BuildSubGrade(
                    "Offensive Execution",
                    [
                        CompareScore(squad.Damage / squadPlayers, enemy.Damage / enemyPlayers),
                        CompareScore(squad.Dps / squadPlayers, enemy.Dps / enemyPlayers),
                        CompareScore(squad.StripsPerMinute / squadPlayers, enemy.StripsPerMinute / enemyPlayers),
                        CompareScore(squad.Downs / squadPlayers, enemy.Downs / enemyPlayers),
                    ],
                    $"{FormatDecimal(squad.Dps / squadPlayers)} DPS/player, {FormatDecimal(squad.StripsPerMinute / squadPlayers)} strips/min/player, {FormatDecimal(squad.Downs / squadPlayers)} downs/player."),
                0.25
            ),
            (
                BuildBurstSubGrade("Burst", squadBurst, enemyBurst),
                0.20
            ),
            (
                BuildSubGrade(
                    "Resilience",
                    [
                        CompareScore(squad.Deaths / squadPlayers, enemy.Deaths / enemyPlayers, higherIsBetter: false),
                        CompareScore(squad.DamageTaken / squadPlayers, enemy.DamageTaken / enemyPlayers, higherIsBetter: false),
                        CompareScore(squad.ReceivedCrowdControl / squadPlayers, enemy.ReceivedCrowdControl / enemyPlayers, higherIsBetter: false),
                    ],
                    $"{FormatDecimal(squad.Deaths / squadPlayers)} deaths/player, {FormatDecimal(squad.DamageTaken / squadPlayers)} damage taken/player, {FormatDecimal(squad.ReceivedCrowdControl / squadPlayers)} received CC/player."),
                0.15
            ),
            (
                BuildSubGrade(
                    "Kill Conversion",
                    [
                        CompareScore(squad.Kills / squadPlayers, enemy.Kills / enemyPlayers),
                        CompareScore(enemyDownState.KillConversionRate, squadDownState.KillConversionRate),
                        CompareScore(enemyDownState.RezRate, squadDownState.RezRate, higherIsBetter: false),
                        CompareNullableLowerIsBetterScore(enemyDownState.AverageKillTime, squadDownState.AverageKillTime),
                    ],
                    $"{FormatDecimal(enemyDownState.KillConversionRate)}% of enemy downs finished, {FormatOptionalSeconds(enemyDownState.AverageKillTime)} average kill time, {FormatDecimal(enemyDownState.RezRate)}% of enemy downs recovered."),
                0.20
            ),
        };

        var subGrades = weightedSubGrades.Select(entry => entry.SubGrade).ToList();
        int baseScore = (int)Math.Round(weightedSubGrades.Sum(entry => entry.SubGrade.Score * entry.Weight));
        int countAdjustment = ComputeSideCountAdjustment(effectiveAlliedPlayerCount, enemy.PlayerCount);
        int overallScore = Math.Clamp(baseScore + countAdjustment, 0, 100);

        return new WvwSummaryGradeDto
        {
            BaseScore = baseScore,
            CountAdjustment = countAdjustment,
            Score = overallScore,
            Grade = ScoreToGrade(overallScore),
            Summary = BuildSquadGradeSummary(overallScore, subGrades),
            Detail = "Compares this phase against the enemy team using cohesion, offensive execution, replay burst windows, resilience, and kill conversion signals drawn from the Summary tab and Combat Replay analysis.",
            CountSummary = BuildPlayerCountSummary(squad.PlayerCount, friendlyPlayerCount, effectiveAlliedPlayerCount, enemy.PlayerCount, countAdjustment),
            SubGrades = subGrades,
        };
    }

    private static WvwSummaryOppositionEstimateDto BuildOppositionEstimate(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummarySideDto squad,
        WvwSummarySideDto enemy,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState,
        double effectiveAlliedPlayerCount)
    {
        double squadPlayers = Math.Max(squad.PlayerCount, 1);
        double enemyPlayers = Math.Max(enemy.PlayerCount, 1);
        WvwSummaryCohesionEstimateDto cohesion = BuildGroupCohesionEstimate(log, phase, hostilePlayerTargets, squadActors);
        WvwSummaryBurstMetricsDto squadBurst = BuildBurstMetrics(phase, combatReplayAnalysis, combatReplayAnalysis?.Squad, squad.PlayerCount);
        WvwSummaryBurstMetricsDto enemyBurst = BuildBurstMetrics(phase, combatReplayAnalysis, combatReplayAnalysis?.Enemy, enemy.PlayerCount);

        var weightedSubGrades = new List<(WvwSummarySubGradeDto SubGrade, double Weight)>
        {
            (
                new WvwSummarySubGradeDto
                {
                    Label = "Cohesion",
                    Score = cohesion.Score,
                    Grade = ScoreToGrade(cohesion.Score),
                    Detail = $"{cohesion.StyleLabel}: {cohesion.Detail}",
                },
                0.20
            ),
            (
                BuildSubGrade(
                    "Offensive Execution",
                    [
                        CompareScore(enemy.Damage / enemyPlayers, squad.Damage / squadPlayers),
                        CompareScore(enemy.Dps / enemyPlayers, squad.Dps / squadPlayers),
                        CompareScore(enemy.StripsPerMinute / enemyPlayers, squad.StripsPerMinute / squadPlayers),
                        CompareScore(enemy.Downs / enemyPlayers, squad.Downs / squadPlayers),
                    ],
                    $"{FormatDecimal(enemy.Dps / enemyPlayers)} DPS/player, {FormatDecimal(enemy.StripsPerMinute / enemyPlayers)} strips/min/player, {FormatDecimal(enemy.Downs / enemyPlayers)} downs/player."),
                0.25
            ),
            (
                BuildBurstSubGrade("Burst", enemyBurst, squadBurst),
                0.20
            ),
            (
                BuildSubGrade(
                    "Resilience",
                    [
                        CompareScore(enemy.Deaths / enemyPlayers, squad.Deaths / squadPlayers, higherIsBetter: false),
                        CompareScore(enemy.DamageTaken / enemyPlayers, squad.DamageTaken / squadPlayers, higherIsBetter: false),
                        CompareScore(enemy.ReceivedCrowdControl / enemyPlayers, squad.ReceivedCrowdControl / squadPlayers, higherIsBetter: false),
                    ],
                    $"{FormatDecimal(enemy.Deaths / enemyPlayers)} deaths/player, {FormatDecimal(enemy.DamageTaken / enemyPlayers)} damage taken/player, {FormatDecimal(enemy.ReceivedCrowdControl / enemyPlayers)} received CC/player."),
                0.15
            ),
            (
                BuildSubGrade(
                    "Kill Conversion",
                    [
                        CompareScore(enemy.Kills / enemyPlayers, squad.Kills / squadPlayers),
                        CompareScore(squadDownState.KillConversionRate, enemyDownState.KillConversionRate),
                        CompareScore(squadDownState.RezRate, enemyDownState.RezRate, higherIsBetter: false),
                        CompareNullableLowerIsBetterScore(squadDownState.AverageKillTime, enemyDownState.AverageKillTime),
                    ],
                    $"{FormatDecimal(squadDownState.KillConversionRate)}% of squad downs finished, {FormatOptionalSeconds(squadDownState.AverageKillTime)} average kill time, {FormatDecimal(squadDownState.RezRate)}% of squad downs recovered."),
                0.20
            ),
        };

        var subGrades = weightedSubGrades.Select(entry => entry.SubGrade).ToList();
        int baseScore = (int)Math.Round(weightedSubGrades.Sum(entry => entry.SubGrade.Score * entry.Weight));
        int countAdjustment = ComputeSideCountAdjustment(enemy.PlayerCount, effectiveAlliedPlayerCount);
        int overallScore = Math.Clamp(baseScore + countAdjustment, 0, 100);

        return new WvwSummaryOppositionEstimateDto
        {
            BaseScore = baseScore,
            CountAdjustment = countAdjustment,
            Score = overallScore,
            Grade = ScoreToGrade(overallScore),
            Summary = BuildOppositionEstimateSummary(overallScore, cohesion.StyleLabel, subGrades),
            Detail = "Uses observed enemy damage, strip, replay burst windows, downs, conversion, survival, and replay-based formation behavior. Enemy healing and cleanses are not directly visible, so sustain is inferred from outcomes rather than measured outright.",
            FormationLabel = cohesion.StyleLabel,
            FormationDetail = cohesion.Detail,
            SubGrades = subGrades,
        };
    }

    private static WvwSummarySubGradeDto BuildSubGrade(string label, IReadOnlyList<double> scores, string detail)
    {
        int score = scores.Count > 0 ? (int)Math.Round(scores.Average()) : 50;
        return new WvwSummarySubGradeDto
        {
            Label = label,
            Score = score,
            Grade = ScoreToGrade(score),
            Detail = detail,
        };
    }

    private static WvwSummarySubGradeDto BuildBurstSubGrade(string label, WvwSummaryBurstMetricsDto sideBurst, WvwSummaryBurstMetricsDto opposingBurst)
    {
        if (!sideBurst.DataAvailable && !opposingBurst.DataAvailable)
        {
            return new WvwSummarySubGradeDto
            {
                Label = label,
                Score = 50,
                Grade = ScoreToGrade(50),
                Detail = "Combat Replay burst data is unavailable for this phase.",
            };
        }

        if (!sideBurst.HasBurstData && !opposingBurst.HasBurstData)
        {
            return new WvwSummarySubGradeDto
            {
                Label = label,
                Score = 50,
                Grade = ScoreToGrade(50),
                Detail = "No strong synced burst windows were detected in this phase.",
            };
        }

        return BuildSubGrade(
            label,
            [
                CompareScore(sideBurst.SyncedBurstsPerMinute, opposingBurst.SyncedBurstsPerMinute),
                CompareScore(sideBurst.TopBurstDamagePerPlayer, opposingBurst.TopBurstDamagePerPlayer),
                CompareScore(sideBurst.AverageBurstDamagePerPlayer, opposingBurst.AverageBurstDamagePerPlayer),
                CompareScore(sideBurst.AverageBurstStripsPerPlayer, opposingBurst.AverageBurstStripsPerPlayer),
            ],
            sideBurst.HasBurstData
                ? $"{sideBurst.TopBursts.Count} strong synced bursts, {FormatDecimal(sideBurst.SyncedBurstsPerMinute)} bursts/min, {FormatDecimal(sideBurst.TopBurstDamagePerPlayer)} top burst damage/player, {FormatDecimal(sideBurst.AverageBurstStripsPerPlayer)} top-5 avg strips/player."
                : "No strong synced burst windows were detected in this phase.");
    }

    private static double CompareScore(double squadValue, double enemyValue, bool higherIsBetter = true)
    {
        if (Math.Abs(squadValue) < 0.0001 && Math.Abs(enemyValue) < 0.0001)
        {
            return 50.0;
        }

        double favorableValue = higherIsBetter ? squadValue : enemyValue;
        double unfavorableValue = higherIsBetter ? enemyValue : squadValue;
        double denominator = Math.Abs(favorableValue) + Math.Abs(unfavorableValue);
        if (denominator < 0.0001)
        {
            return 50.0;
        }

        double normalizedAdvantage = (favorableValue - unfavorableValue) / denominator;
        return Math.Clamp(50.0 + normalizedAdvantage * 40.0, 10.0, 90.0);
    }

    private static string BuildSquadGradeSummary(int overallScore, IReadOnlyList<WvwSummarySubGradeDto> subGrades)
    {
        if (subGrades.Count == 0)
        {
            return "This phase did not have enough data to grade.";
        }

        WvwSummarySubGradeDto strongest = subGrades.MaxBy(subGrade => subGrade.Score)!;
        WvwSummarySubGradeDto weakest = subGrades.MinBy(subGrade => subGrade.Score)!;

        if (overallScore >= 74)
        {
            return $"The squad won most exchanges this phase. Strongest area: {strongest.Label}.";
        }
        if (overallScore >= 58)
        {
            return $"The squad held a positive edge overall, led by {strongest.Label}, with {weakest.Label} lagging slightly behind.";
        }
        if (overallScore >= 42)
        {
            return $"This phase was fairly even. {strongest.Label} stood out most, while {weakest.Label} was the biggest drag on the score.";
        }
        if (overallScore >= 30)
        {
            return $"The squad struggled to keep pace this phase, especially in {weakest.Label}.";
        }
        return $"The squad was heavily pressured this phase. {weakest.Label} fell furthest behind the enemy.";
    }

    private static string BuildOppositionEstimateSummary(int overallScore, string cohesionStyle, IReadOnlyList<WvwSummarySubGradeDto> subGrades)
    {
        if (subGrades.Count == 0)
        {
            return "Not enough observed enemy data was available to estimate opposition skill.";
        }

        WvwSummarySubGradeDto strongest = subGrades.MaxBy(subGrade => subGrade.Score)!;
        if (overallScore >= 74)
        {
            return $"The opposition looked very sharp in this phase. Strongest signal: {strongest.Label}. Formation read: {cohesionStyle}.";
        }
        if (overallScore >= 58)
        {
            return $"The opposition looked competent and coordinated overall, with {strongest.Label} standing out. Formation read: {cohesionStyle}.";
        }
        if (overallScore >= 42)
        {
            return $"The opposition looked mixed rather than dominant. Strongest signal: {strongest.Label}. Formation read: {cohesionStyle}.";
        }
        return $"The opposition did not show many strong execution signals in this phase. Formation read: {cohesionStyle}.";
    }

    private static int ComputeSideCountAdjustment(double sidePlayers, double otherPlayers)
    {
        double playerGap = otherPlayers - sidePlayers;
        double excessGap = Math.Abs(playerGap) - 5.0;
        if (excessGap <= 0.0)
        {
            return 0;
        }

        double maxPlayers = Math.Max(Math.Max(sidePlayers, otherPlayers), 1.0);
        double scaledGap = excessGap * 0.85 + (excessGap * excessGap) / 18.0 + (Math.Abs(playerGap) / maxPlayers) * 6.0;
        int adjustment = (int)Math.Round(Math.Clamp(scaledGap, 0.0, 18.0));
        return playerGap > 0.0 ? adjustment : -adjustment;
    }

    private static string BuildPlayerCountSummary(int squadPlayers, int friendlyPlayers, double effectiveAlliedPlayerCount, int enemyPlayers, int countAdjustment)
    {
        string alliedSummary = friendlyPlayers > 0
            ? $"{squadPlayers} squad + {friendlyPlayers} friendlies ({FormatCountValue(effectiveAlliedPlayerCount)} effective)"
            : $"{squadPlayers} squad";

        if (Math.Abs(effectiveAlliedPlayerCount - enemyPlayers) <= 5.0)
        {
            return $"Even numbers from squad view: {alliedSummary} vs {enemyPlayers} enemy. No count adjustment.";
        }
        if (countAdjustment > 0)
        {
            return $"Outnumbered from squad view: {alliedSummary} vs {enemyPlayers} enemy. +{countAdjustment} score context.";
        }
        if (countAdjustment < 0)
        {
            return $"Superior numbers from squad view: {alliedSummary} vs {enemyPlayers} enemy. {countAdjustment} score context.";
        }
        return $"Even numbers from squad view: {alliedSummary} vs {enemyPlayers} enemy. No count adjustment.";
    }

    private static WvwSummaryBurstMetricsDto BuildBurstMetrics(
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        CombatReplayTeamAnalysisDto? teamAnalysis,
        int playerCount)
    {
        if (combatReplayAnalysis == null || teamAnalysis == null || combatReplayAnalysis.Times.Length == 0)
        {
            return new WvwSummaryBurstMetricsDto();
        }

        List<CombatReplayAnalysisBurstSummaryDto> topBursts = BuildPhaseTopBursts(teamAnalysis, combatReplayAnalysis.Times, phase.Start, phase.End);
        if (topBursts.Count == 0)
        {
            return new WvwSummaryBurstMetricsDto
            {
                DataAvailable = true,
            };
        }

        double normalizedPlayerCount = Math.Max(playerCount, 1);
        double durationInMinutes = Math.Max(phase.DurationInMS / 60000.0, 1.0 / 60.0);

        return new WvwSummaryBurstMetricsDto
        {
            DataAvailable = true,
            TopBursts = topBursts,
            SyncedBurstsPerMinute = Math.Round(topBursts.Count / durationInMinutes, 1),
            TopBurstDamagePerPlayer = Math.Round(topBursts.Max(burst => burst.Damage) / normalizedPlayerCount, 1),
            AverageBurstDamagePerPlayer = Math.Round(topBursts.Average(burst => burst.Damage) / normalizedPlayerCount, 1),
            AverageBurstStripsPerPlayer = Math.Round(topBursts.Average(burst => burst.Strips) / normalizedPlayerCount, 1),
        };
    }

    private static List<CombatReplayAnalysisBurstSummaryDto> BuildPhaseTopBursts(
        CombatReplayTeamAnalysisDto analysis,
        IReadOnlyList<long> times,
        long phaseStart,
        long phaseEnd)
    {
        var candidates = new List<CombatReplayAnalysisBurstSummaryDto>();
        var index = 0;
        while (index < times.Count)
        {
            if (times[index] < phaseStart)
            {
                index++;
                continue;
            }
            if (times[index] > phaseEnd)
            {
                break;
            }
            if (analysis.BurstStrength[index] != "strong" || !analysis.StripSynced[index])
            {
                index++;
                continue;
            }

            var bestIndex = index;
            var nextIndex = index + 1;
            while (nextIndex < times.Count &&
                times[nextIndex] <= phaseEnd &&
                analysis.BurstStrength[nextIndex] == "strong")
            {
                if (analysis.StripSynced[nextIndex] && IsBetterBurstSnapshotForSummary(analysis, nextIndex, bestIndex, times))
                {
                    bestIndex = nextIndex;
                }
                nextIndex++;
            }

            candidates.Add(new CombatReplayAnalysisBurstSummaryDto
            {
                Time = times[bestIndex],
                Damage = analysis.Damage[bestIndex],
                Strips = analysis.Strips[bestIndex],
                Downs = analysis.Downs[bestIndex],
                DownsTotal = analysis.DownsTotal[bestIndex],
                Kills = analysis.Kills[bestIndex],
                KillsTotal = analysis.KillsTotal[bestIndex],
            });
            index = nextIndex;
        }

        return [.. candidates
            .OrderByDescending(burst => burst.Damage)
            .ThenByDescending(burst => burst.Strips)
            .ThenByDescending(burst => burst.Downs)
            .ThenByDescending(burst => burst.Kills)
            .ThenBy(burst => burst.Time)
            .Take(5)];
    }

    private static bool IsBetterBurstSnapshotForSummary(CombatReplayTeamAnalysisDto analysis, int candidateIndex, int currentBestIndex, IReadOnlyList<long> times)
    {
        if (analysis.Damage[candidateIndex] != analysis.Damage[currentBestIndex])
        {
            return analysis.Damage[candidateIndex] > analysis.Damage[currentBestIndex];
        }
        if (analysis.Strips[candidateIndex] != analysis.Strips[currentBestIndex])
        {
            return analysis.Strips[candidateIndex] > analysis.Strips[currentBestIndex];
        }
        if (analysis.Downs[candidateIndex] != analysis.Downs[currentBestIndex])
        {
            return analysis.Downs[candidateIndex] > analysis.Downs[currentBestIndex];
        }
        if (analysis.Kills[candidateIndex] != analysis.Kills[currentBestIndex])
        {
            return analysis.Kills[candidateIndex] > analysis.Kills[currentBestIndex];
        }
        return times[candidateIndex] < times[currentBestIndex];
    }

    private static WvwSummaryCohesionEstimateDto BuildGroupCohesionEstimate(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> groupActors,
        IReadOnlyList<SingleActor> opposingActors)
    {
        const long sampleInterval = 1000;
        const long ignoreTailWindow = 10000;
        const float enemyAllyRange = 600.0f;
        const float engageRange = 1600.0f;
        const float maxDistanceFromFight = 5000.0f;

        var snapshots = new List<WvwSummaryEnemySnapshot>();
        for (long time = phase.Start; time <= phase.End; time += sampleInterval)
        {
            var groupPositions = new List<Vector3>();
            foreach (SingleActor enemy in groupActors)
            {
                if (TryGetEligiblePosition(enemy, log, time, out Vector3 position))
                {
                    groupPositions.Add(position);
                }
            }

            var opposingPositions = new List<Vector3>();
            foreach (SingleActor squadActor in opposingActors)
            {
                if (TryGetEligiblePosition(squadActor, log, time, out Vector3 position))
                {
                    opposingPositions.Add(position);
                }
            }

            if (groupPositions.Count == 0 || opposingPositions.Count == 0)
            {
                continue;
            }

            bool engaged = AreGroupsEngaged(groupPositions, opposingPositions, engageRange);
            if (!engaged)
            {
                continue;
            }

            List<Vector3> participatingGroupPositions = FilterPositionsNearFight(groupPositions, opposingPositions, maxDistanceFromFight);
            List<Vector3> participatingOpposingPositions = FilterPositionsNearFight(opposingPositions, groupPositions, maxDistanceFromFight);
            if (participatingGroupPositions.Count == 0 || participatingOpposingPositions.Count == 0)
            {
                continue;
            }

            snapshots.Add(new WvwSummaryEnemySnapshot(time, participatingGroupPositions, true));
        }

        int peakEligibleEnemies = snapshots.Where(snapshot => snapshot.Engaged).Select(snapshot => snapshot.EnemyPositions.Count).DefaultIfEmpty(0).Max();
        int minimumEligibleCount = Math.Max(5, (int)Math.Ceiling(peakEligibleEnemies * 0.6));

        var cohesionScores = new List<double>();
        int organizedSnapshots = 0;
        int cloudSnapshots = 0;

        foreach (WvwSummaryEnemySnapshot snapshot in snapshots)
        {
            if (!snapshot.Engaged ||
                snapshot.Time > phase.End - ignoreTailWindow ||
                snapshot.EnemyPositions.Count < minimumEligibleCount)
            {
                continue;
            }

            int playerCount = snapshot.EnemyPositions.Count;
            int neighborThreshold = Math.Max(2, Math.Min(5, (int)Math.Round(playerCount * 0.25)));
            Vector3 centroid = ComputeCentroid(snapshot.EnemyPositions);
            int clusteredPlayers = 0;
            double totalDistanceToCentroid = 0.0;

            for (int enemyIndex = 0; enemyIndex < snapshot.EnemyPositions.Count; enemyIndex++)
            {
                Vector3 enemyPosition = snapshot.EnemyPositions[enemyIndex];
                int nearbyAllies = 0;
                for (int otherIndex = 0; otherIndex < snapshot.EnemyPositions.Count; otherIndex++)
                {
                    if (enemyIndex == otherIndex)
                    {
                        continue;
                    }
                    if (IsWithinRange(enemyPosition, snapshot.EnemyPositions[otherIndex], enemyAllyRange))
                    {
                        nearbyAllies++;
                    }
                }
                if (nearbyAllies >= neighborThreshold)
                {
                    clusteredPlayers++;
                }
                totalDistanceToCentroid += GetDistance2D(enemyPosition, centroid);
            }

            double clusteredShare = clusteredPlayers / (double)playerCount;
            double averageDistanceToCentroid = totalDistanceToCentroid / playerCount;
            double compactnessScore = Clamp01((900.0 - averageDistanceToCentroid) / 500.0);
            double snapshotScore = Math.Round((clusteredShare * 0.7 + compactnessScore * 0.3) * 100.0, 1);
            cohesionScores.Add(snapshotScore);

            if (clusteredShare >= 0.7 && compactnessScore >= 0.55)
            {
                organizedSnapshots++;
            }
            else if (clusteredShare <= 0.4 || compactnessScore <= 0.2)
            {
                cloudSnapshots++;
            }
        }

        if (cohesionScores.Count == 0)
        {
            return new WvwSummaryCohesionEstimateDto
            {
                Score = 50,
                StyleLabel = "insufficient replay data",
                Detail = "Could not evaluate enough engaged enemy snapshots to judge whether the opposition moved as an organized group or a cloud.",
            };
        }

        int score = (int)Math.Round(cohesionScores.Average());
        double organizedRate = Math.Round(organizedSnapshots * 100.0 / cohesionScores.Count, 1);
        double cloudRate = Math.Round(cloudSnapshots * 100.0 / cohesionScores.Count, 1);
        string styleLabel = organizedRate >= 55.0 && cloudRate <= 20.0
            ? "organized group"
            : cloudRate >= 40.0
                ? "cloud"
                : "mixed formation";

        return new WvwSummaryCohesionEstimateDto
        {
            Score = score,
            StyleLabel = styleLabel,
            Detail = $"{FormatDecimal(organizedRate)}% organized snapshots, {FormatDecimal(cloudRate)}% cloud snapshots across {cohesionScores.Count} engaged replay samples. Ignores the last 10s and routed cleanup after the enemy falls below 60% of peak active count.",
        };
    }

    private static double CompareNullableLowerIsBetterScore(double? squadValue, double? enemyValue)
    {
        if (!squadValue.HasValue && !enemyValue.HasValue)
        {
            return 50.0;
        }
        if (!squadValue.HasValue)
        {
            return 60.0;
        }
        if (!enemyValue.HasValue)
        {
            return 40.0;
        }
        return CompareScore(squadValue.Value, enemyValue.Value, higherIsBetter: false);
    }

    private static string ScoreToGrade(int score)
    {
        return score switch
        {
            >= 90 => "A",
            >= 82 => "A-",
            >= 74 => "B+",
            >= 66 => "B",
            >= 58 => "B-",
            >= 50 => "C+",
            >= 42 => "C",
            >= 34 => "D",
            _ => "F",
        };
    }

    private static string FormatDecimal(double value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatCountValue(double value)
    {
        return Math.Abs(value - Math.Round(value)) < 0.05
            ? Math.Round(value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatOptionalSeconds(double? value)
    {
        return value.HasValue ? $"{value.Value.ToString("0.0", CultureInfo.InvariantCulture)}s" : "n/a";
    }

    private static bool TryGetEligiblePosition(SingleActor actor, ParsedEvtcLog log, long time, out Vector3 position)
    {
        position = default;
        if (time < actor.FirstAware || time > actor.LastAware || actor.IsDowned(log, time) || actor.IsDead(log, time) || actor.IsDC(log, time))
        {
            return false;
        }
        return actor.TryGetCurrentInterpolatedPosition(log, time, out position) || actor.TryGetCurrentPosition(log, time, out position);
    }

    private static bool AreGroupsEngaged(IReadOnlyList<Vector3> enemyPositions, IReadOnlyList<Vector3> squadPositions, float engageRange)
    {
        foreach (Vector3 enemyPosition in enemyPositions)
        {
            foreach (Vector3 squadPosition in squadPositions)
            {
                if (IsWithinRange(enemyPosition, squadPosition, engageRange))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static List<Vector3> FilterPositionsNearFight(IReadOnlyList<Vector3> groupPositions, IReadOnlyList<Vector3> opposingPositions, float maxDistanceFromFight)
    {
        var filteredPositions = new List<Vector3>();
        foreach (Vector3 groupPosition in groupPositions)
        {
            bool nearFight = opposingPositions.Any(opposingPosition => IsWithinRange(groupPosition, opposingPosition, maxDistanceFromFight));
            if (nearFight)
            {
                filteredPositions.Add(groupPosition);
            }
        }
        return filteredPositions;
    }

    private static Vector3 ComputeCentroid(IReadOnlyList<Vector3> positions)
    {
        if (positions.Count == 0)
        {
            return default;
        }

        float x = 0;
        float y = 0;
        float z = 0;
        foreach (Vector3 position in positions)
        {
            x += position.X;
            y += position.Y;
            z += position.Z;
        }
        float divisor = positions.Count;
        return new Vector3(x / divisor, y / divisor, z / divisor);
    }

    private static bool IsWithinRange(Vector3 left, Vector3 right, float range)
    {
        float dx = left.X - right.X;
        float dy = left.Y - right.Y;
        return dx * dx + dy * dy <= range * range;
    }

    private static float GetDistance2D(Vector3 left, Vector3 right)
    {
        float dx = left.X - right.X;
        float dy = left.Y - right.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static double Clamp01(double value)
    {
        return Math.Clamp(value, 0.0, 1.0);
    }

    private static bool IsActiveInPhase(SingleActor actor, PhaseData phase)
    {
        return actor.FirstAware < phase.End && actor.LastAware > phase.Start;
    }

    private static List<SingleActor> GetFriendlyPlayerActors(ParsedEvtcLog log, PhaseData phase)
    {
        return log.LogData.Logic.NonSquadFriendlies
            .Where(actor =>
                !actor.IsFakeActor &&
                actor.AgentItem.Type == AgentItem.AgentType.NonSquadPlayer &&
                IsActiveInPhase(actor, phase))
            .ToList();
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

    private static List<WvwSummaryMetricRowDto> BuildMetricRows(long durationInMilliseconds, WvwSummarySideDto squad, WvwSummarySideDto enemy, int friendlyPlayerCount)
    {
        string squadPlayersValue = friendlyPlayerCount > 0
            ? $"{squad.PlayerCount} (+{friendlyPlayerCount} friendlies)"
            : squad.PlayerCount.ToString();

        return
        [
            new WvwSummaryMetricRowDto("Fight Time", ToDurationString(durationInMilliseconds), ToDurationString(durationInMilliseconds), true),
            new WvwSummaryMetricRowDto("Players", squadPlayersValue, enemy.PlayerCount.ToString()),
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
                result.DownEntries.Add(BuildDownEventEntry(actor, down.Start, phase.Start, null));
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
                    result.KillConversionEntries.Add(BuildDownEventEntry(
                        actor,
                        down.End,
                        phase.Start,
                        $"Downed at {ToDurationString(Math.Max(0, down.Start - phase.Start))}"));
                }
                else if (HasStatusEventAtTime(aliveEvents, down.End))
                {
                    result.Rezzes++;
                    result.RezTimes.Add(downDurationSeconds);
                    result.RezEntries.Add(BuildDownEventEntry(
                        actor,
                        down.End,
                        phase.Start,
                        $"Downed at {ToDurationString(Math.Max(0, down.Start - phase.Start))}"));
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

    private static WvwSummaryDownEventEntryDto BuildDownEventEntry(SingleActor actor, long eventTime, long phaseStart, string? detailLabel)
    {
        return new WvwSummaryDownEventEntryDto
        {
            Name = actor.Character,
            Account = actor.Account,
            Profession = actor.Spec.ToString(),
            Icon = actor.GetIcon(),
            Time = eventTime,
            TimeLabel = ToDurationString(Math.Max(0, eventTime - phaseStart)),
            DetailLabel = detailLabel ?? "",
        };
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

internal class WvwSummaryGradeDto
{
    public int BaseScore { get; set; }
    public int CountAdjustment { get; set; }
    public int Score { get; set; }
    public string Grade { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public string CountSummary { get; set; } = "";
    public List<WvwSummarySubGradeDto> SubGrades { get; set; } = [];
}

internal class WvwSummaryOppositionEstimateDto
{
    public int BaseScore { get; set; }
    public int CountAdjustment { get; set; }
    public int Score { get; set; }
    public string Grade { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public string FormationLabel { get; set; } = "";
    public string FormationDetail { get; set; } = "";
    public List<WvwSummarySubGradeDto> SubGrades { get; set; } = [];
}

internal class WvwSummarySubGradeDto
{
    public string Label { get; set; } = "";
    public int Score { get; set; }
    public string Grade { get; set; } = "";
    public string Detail { get; set; } = "";
}

internal class WvwSummaryCohesionEstimateDto
{
    public int Score { get; set; }
    public string StyleLabel { get; set; } = "";
    public string Detail { get; set; } = "";
}

internal class WvwSummaryBurstMetricsDto
{
    public bool DataAvailable { get; set; }
    public List<CombatReplayAnalysisBurstSummaryDto> TopBursts { get; set; } = [];
    public double SyncedBurstsPerMinute { get; set; }
    public double TopBurstDamagePerPlayer { get; set; }
    public double AverageBurstDamagePerPlayer { get; set; }
    public double AverageBurstStripsPerPlayer { get; set; }
    public bool HasBurstData => TopBursts.Count > 0;
}

internal record WvwSummaryEnemySnapshot(long Time, List<Vector3> EnemyPositions, bool Engaged);

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
    public List<WvwSummaryDownEventEntryDto> DownEntries { get; } = [];
    public List<WvwSummaryDownEventEntryDto> KillConversionEntries { get; } = [];
    public List<WvwSummaryDownEventEntryDto> RezEntries { get; } = [];
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

internal class WvwSummaryDownEventEntryDto
{
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public string DetailLabel { get; set; } = "";
}
