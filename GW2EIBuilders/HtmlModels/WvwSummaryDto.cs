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
    public List<WvwSummaryMomentDto> Moments { get; set; } = [];
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
        List<WvwSummaryMomentDto> moments = BuildMoments(log, phase, combatReplayAnalysis, squadActors, hostilePlayerTargets, squadDownState, enemyDownState);

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
            Moments = moments,
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

    private static List<WvwSummaryMomentDto> BuildMoments(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState)
    {
        Player? commander = log.PlayerList.FirstOrDefault(player => !player.IsFakeActor && player.IsCommander(log));
        var requiredCandidates = new List<WvwSummaryMomentCandidate>();
        var optionalCandidates = new List<WvwSummaryMomentCandidate>();

        if (TryBuildFirstEventCandidate(enemyDownState.KillConversionEntries, "First enemy kill", entry => $"Our Squad secured the first kill on {entry.Name}. {entry.DetailLabel}".Trim(), "positive", "first-kill-positive", 48.0, out WvwSummaryMomentCandidate firstEnemyKill))
        {
            requiredCandidates.Add(firstEnemyKill);
        }
        if (TryBuildFirstEventCandidate(squadDownState.KillConversionEntries, "First squad kill", entry => $"Enemy Team secured the first kill on {entry.Name}. {entry.DetailLabel}".Trim(), "negative", "first-kill-negative", 48.0, out WvwSummaryMomentCandidate firstSquadKill))
        {
            requiredCandidates.Add(firstSquadKill);
        }
        if (TryBuildKillMilestoneCandidate(enemyDownState.KillConversionEntries, squadDownState.KillConversionEntries, out WvwSummaryMomentCandidate firstToFiveKills))
        {
            requiredCandidates.Add(firstToFiveKills);
        }

        List<WvwSummaryFormationSnapshot> enemyFormationSnapshots = BuildGroupFormationSnapshots(log, phase, hostilePlayerTargets, squadActors);
        foreach (WvwSummaryMomentCandidate momentumSwing in BuildMomentumSwingCandidates(log, phase, squadActors, hostilePlayerTargets, commander))
        {
            optionalCandidates.Add(momentumSwing);
        }
        AddClusterCandidates(optionalCandidates, enemyDownState.DownEntries, "Enemy downs spiked", count => $"Our Squad caused {count} enemy downs inside 3 seconds.", "positive", "cluster-down-positive", 3.0, 22.0);
        AddClusterCandidates(optionalCandidates, squadDownState.DownEntries, "Squad downs spiked", count => $"Enemy Team caused {count} squad downs inside 3 seconds.", "negative", "cluster-down-negative", 3.0, 22.0);
        AddClusterCandidates(optionalCandidates, enemyDownState.KillConversionEntries, "Kill chain started", count => $"Our Squad converted {count} enemy kills inside 3 seconds.", "positive", "cluster-kill-positive", 3.0, 26.0);
        AddClusterCandidates(optionalCandidates, squadDownState.KillConversionEntries, "Enemy kill chain started", count => $"Enemy Team converted {count} squad kills inside 3 seconds.", "negative", "cluster-kill-negative", 3.0, 26.0);
        AddClusterCandidates(optionalCandidates, squadDownState.RezEntries, "Rez swing stabilized squad", count => $"Our Squad completed {count} rezzes inside 3 seconds.", "positive", "cluster-rez-positive", 3.0, 18.0);
        AddClusterCandidates(optionalCandidates, enemyDownState.RezEntries, "Enemy rez swing stabilized", count => $"Enemy Team completed {count} rezzes inside 3 seconds.", "negative", "cluster-rez-negative", 3.0, 18.0);

        if (TryBuildEnemyFormationBreakCandidate(phase, enemyFormationSnapshots, out WvwSummaryMomentCandidate enemyFormationBreak))
        {
            optionalCandidates.Add(enemyFormationBreak);
        }
        if (TryBuildEnemyShatteredCandidate(phase, enemyFormationSnapshots, enemyDownState.KillConversionEntries, out WvwSummaryMomentCandidate enemyShattered))
        {
            optionalCandidates.Add(enemyShattered);
        }

        if (combatReplayAnalysis?.Squad != null && combatReplayAnalysis.Times.Length > 0)
        {
            foreach (CombatReplayAnalysisBurstSummaryDto burst in BuildPhaseTopBursts(combatReplayAnalysis.Squad, combatReplayAnalysis.Times, phase.Start, phase.End).Take(3))
            {
                string label = burst.Downs > 0 || burst.Kills > 0 ? "Bomb landed" : "Pressure spike";
                string detail = $"Our Squad dealt {burst.Damage.ToString("N0", CultureInfo.InvariantCulture)} damage in 3s with {burst.Strips} strips, causing {burst.Downs} downs and {burst.Kills} kills.";
                double score = burst.Damage / 2000.0 + burst.Strips * 0.5 + burst.Downs * 8 + burst.Kills * 10;
                optionalCandidates.Add(new WvwSummaryMomentCandidate(burst.Time, 0, label, detail, "positive", "burst-positive", score));
            }
        }

        if (combatReplayAnalysis?.Enemy != null && combatReplayAnalysis.Times.Length > 0)
        {
            foreach (CombatReplayAnalysisBurstSummaryDto burst in BuildPhaseTopBursts(combatReplayAnalysis.Enemy, combatReplayAnalysis.Times, phase.Start, phase.End).Take(3))
            {
                string label = burst.Downs > 0 || burst.Kills > 0 ? "Enemy bomb landed" : "Enemy pressure spike";
                string detail = $"Enemy Team dealt {burst.Damage.ToString("N0", CultureInfo.InvariantCulture)} damage in 3s with {burst.Strips} strips, causing {burst.Downs} downs and {burst.Kills} kills.";
                double score = burst.Damage / 2000.0 + burst.Strips * 0.5 + burst.Downs * 8 + burst.Kills * 10;
                optionalCandidates.Add(new WvwSummaryMomentCandidate(burst.Time, 0, label, detail, "negative", "burst-negative", score));
            }
        }

        if (combatReplayAnalysis != null)
        {
            if (TryBuildFormationBreakCandidate(phase, combatReplayAnalysis, out WvwSummaryMomentCandidate formationBreak))
            {
                optionalCandidates.Add(formationBreak);
            }
            if (TryBuildStabilityDropCandidate(phase, combatReplayAnalysis, squadDownState.DownEntries, out WvwSummaryMomentCandidate stabilityDrop))
            {
                optionalCandidates.Add(stabilityDrop);
            }
        }

        var selected = new List<WvwSummaryMomentCandidate>();
        foreach (WvwSummaryMomentCandidate candidate in requiredCandidates.OrderBy(candidate => candidate.Time))
        {
            if (!selected.Any(existing => existing.Category == candidate.Category && existing.Time == candidate.Time))
            {
                selected.Add(candidate);
            }
        }

        const int maxMoments = 25;
        const long dedupeWindow = 4000;
        foreach (WvwSummaryMomentCandidate candidate in optionalCandidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Time))
        {
            if (selected.Count >= maxMoments)
            {
                break;
            }
            bool overlaps = selected.Any(existing => Math.Abs(existing.Time - candidate.Time) <= dedupeWindow);
            if (!overlaps)
            {
                selected.Add(candidate);
            }
        }

        return [.. selected
            .OrderBy(candidate => candidate.Time)
            .Select(candidate =>
            {
                WvwSummarySideStateDto squadState = BuildSideState(log, squadActors, candidate.Time, commander, applyCommanderRange: true);
                WvwSummarySideStateDto enemyState = BuildSideState(log, hostilePlayerTargets, candidate.Time);
                return new WvwSummaryMomentDto
                {
                    Time = candidate.Time,
                    RelativeTime = Math.Max(0, candidate.Time - phase.Start),
                    TimeLabel = ToDurationString(Math.Max(0, candidate.Time - phase.Start)),
                    UniqueId = candidate.UniqueId,
                    Label = candidate.Label,
                    Detail = candidate.Detail,
                    SquadAlive = squadState.Alive,
                    EnemyAlive = enemyState.Alive,
                    StateSummary = $"Our Squad {FormatSideState(squadState)} | Enemy Team {FormatSideState(enemyState)}",
                    Category = candidate.Category,
                    Tone = candidate.Tone,
                };
            })];
    }

    private static List<WvwSummaryMomentCandidate> BuildMomentumSwingCandidates(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        SingleActor? commander)
    {
        const long sampleInterval = 1000;
        const int sustainedWindowSeconds = 10;
        const double minimumSwingDelta = 0.01;
        const double directionalEpsilon = 0.005;
        const int maxOpposingSteps = 2;
        const int maxMomentumSwings = 4;

        var snapshots = new List<WvwSummaryMomentumSnapshot>();
        for (long time = phase.Start; time <= phase.End; time += sampleInterval)
        {
            WvwSummarySideStateDto squadState = BuildSideState(log, squadActors, time, commander, applyCommanderRange: true);
            WvwSummarySideStateDto enemyState = BuildSideState(log, hostilePlayerTargets, time);
            int squadAlive = squadState.Alive;
            int enemyAlive = enemyState.Alive;
            int totalAlive = squadAlive + enemyAlive;
            double odds = totalAlive > 0 ? squadAlive / (double)totalAlive : 0.5;
            snapshots.Add(new WvwSummaryMomentumSnapshot(time, squadAlive, enemyAlive, odds));
        }

        if (snapshots.Count <= sustainedWindowSeconds * 2)
        {
            return [];
        }

        var rawCandidates = new List<WvwSummaryMomentCandidate>();
        for (int pivotIndex = sustainedWindowSeconds; pivotIndex + sustainedWindowSeconds < snapshots.Count; pivotIndex++)
        {
            WvwSummaryMomentumSnapshot previousSnapshot = snapshots[pivotIndex - sustainedWindowSeconds];
            WvwSummaryMomentumSnapshot pivotSnapshot = snapshots[pivotIndex];
            WvwSummaryMomentumSnapshot nextSnapshot = snapshots[pivotIndex + sustainedWindowSeconds];
            double previousDelta = pivotSnapshot.Odds - previousSnapshot.Odds;
            double nextDelta = nextSnapshot.Odds - pivotSnapshot.Odds;
            if (Math.Abs(previousDelta) < minimumSwingDelta || Math.Abs(nextDelta) < minimumSwingDelta)
            {
                continue;
            }

            int previousDirection = Math.Sign(previousDelta);
            int nextDirection = Math.Sign(nextDelta);
            if (previousDirection == 0 || nextDirection == 0 || previousDirection == nextDirection)
            {
                continue;
            }

            if (CountOpposingSteps(snapshots, pivotIndex - sustainedWindowSeconds, pivotIndex, previousDirection, directionalEpsilon) > maxOpposingSteps ||
                CountOpposingSteps(snapshots, pivotIndex, pivotIndex + sustainedWindowSeconds, nextDirection, directionalEpsilon) > maxOpposingSteps)
            {
                continue;
            }

            int previousOddsPercent = (int)Math.Round(previousSnapshot.Odds * 100.0);
            int nextOddsPercent = (int)Math.Round(nextSnapshot.Odds * 100.0);
            bool swungToSquad = nextDirection > 0;
            string label = swungToSquad ? "Momentum swung to Our Squad" : "Momentum swung to Enemy Team";
            string detail = $"Alive-based win odds reversed from {previousOddsPercent}% to {nextOddsPercent}% over 10s and sustained the new direction.";
            string tone = swungToSquad ? "positive" : "negative";
            string category = swungToSquad ? "momentum-swing-positive" : "momentum-swing-negative";
            double score = (Math.Abs(previousDelta) + Math.Abs(nextDelta)) * 100.0;
            rawCandidates.Add(new WvwSummaryMomentCandidate(pivotSnapshot.Time, 0, label, detail, tone, category, score));
        }

        if (rawCandidates.Count == 0)
        {
            return [];
        }

        var selected = new List<WvwSummaryMomentCandidate>();
        foreach (WvwSummaryMomentCandidate candidate in rawCandidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Time))
        {
            if (selected.Count >= maxMomentumSwings)
            {
                break;
            }

            if (!selected.Any(existing => Math.Abs(existing.Time - candidate.Time) < sustainedWindowSeconds * sampleInterval))
            {
                selected.Add(candidate);
            }
        }

        return [.. selected.OrderBy(candidate => candidate.Time)];
    }

    private static int CountOpposingSteps(
        IReadOnlyList<WvwSummaryMomentumSnapshot> snapshots,
        int startIndex,
        int endIndex,
        int direction,
        double epsilon)
    {
        int opposingSteps = 0;
        for (int index = startIndex + 1; index <= endIndex; index++)
        {
            double delta = snapshots[index].Odds - snapshots[index - 1].Odds;
            if ((direction > 0 && delta <= -epsilon) || (direction < 0 && delta >= epsilon))
            {
                opposingSteps++;
            }
        }
        return opposingSteps;
    }

    private static bool TryBuildFirstEventCandidate(
        IReadOnlyList<WvwSummaryDownEventEntryDto> entries,
        string label,
        Func<WvwSummaryDownEventEntryDto, string> detailBuilder,
        string tone,
        string category,
        double score,
        out WvwSummaryMomentCandidate candidate)
    {
        candidate = default;
        WvwSummaryDownEventEntryDto? entry = entries.OrderBy(current => current.Time).FirstOrDefault();
        if (entry == null)
        {
            return false;
        }

        candidate = new WvwSummaryMomentCandidate(entry.Time, entry.UniqueId, label, detailBuilder(entry), tone, category, score);
        return true;
    }

    private static bool TryBuildKillMilestoneCandidate(
        IReadOnlyList<WvwSummaryDownEventEntryDto> enemyKillEntries,
        IReadOnlyList<WvwSummaryDownEventEntryDto> squadKillEntries,
        out WvwSummaryMomentCandidate candidate)
    {
        candidate = default;

        WvwSummaryDownEventEntryDto? squadMilestone = enemyKillEntries.OrderBy(entry => entry.Time).Skip(4).FirstOrDefault();
        WvwSummaryDownEventEntryDto? enemyMilestone = squadKillEntries.OrderBy(entry => entry.Time).Skip(4).FirstOrDefault();
        if (squadMilestone == null && enemyMilestone == null)
        {
            return false;
        }

        bool squadReachedFirst = squadMilestone != null &&
            (enemyMilestone == null || squadMilestone.Time <= enemyMilestone.Time);
        if (squadReachedFirst)
        {
            candidate = new WvwSummaryMomentCandidate(
                squadMilestone!.Time,
                squadMilestone.UniqueId,
                "Reached 5 kills first",
                "Our Squad was the first team to reach 5 kills.",
                "positive",
                "milestone-five-kills",
                50.0);
            return true;
        }

        candidate = new WvwSummaryMomentCandidate(
            enemyMilestone!.Time,
            enemyMilestone.UniqueId,
            "Enemy reached 5 kills first",
            "Enemy Team was the first team to reach 5 kills.",
            "negative",
            "milestone-five-kills",
            50.0);
        return true;
    }

    private static void AddClusterCandidates(
        List<WvwSummaryMomentCandidate> candidates,
        IReadOnlyList<WvwSummaryDownEventEntryDto> entries,
        string label,
        Func<int, string> detailBuilder,
        string tone,
        string category,
        double windowSeconds,
        double baseScore)
    {
        foreach (WvwSummaryMomentCandidate candidate in BuildClusterCandidates(entries, label, detailBuilder, tone, category, windowSeconds, baseScore))
        {
            candidates.Add(candidate);
        }
    }

    private static List<WvwSummaryMomentCandidate> BuildClusterCandidates(
        IReadOnlyList<WvwSummaryDownEventEntryDto> entries,
        string label,
        Func<int, string> detailBuilder,
        string tone,
        string category,
        double windowSeconds,
        double baseScore)
    {
        if (entries.Count < 2)
        {
            return [];
        }

        List<WvwSummaryDownEventEntryDto> orderedEntries = [.. entries.OrderBy(entry => entry.Time)];
        long window = (long)Math.Round(windowSeconds * 1000.0);
        var rawCandidates = new List<WvwSummaryClusterWindowCandidate>();

        int start = 0;
        for (int end = 0; end < orderedEntries.Count; end++)
        {
            while (orderedEntries[end].Time - orderedEntries[start].Time > window)
            {
                start++;
            }

            int count = end - start + 1;
            if (count >= 2)
            {
                rawCandidates.Add(new WvwSummaryClusterWindowCandidate(
                    orderedEntries[start].Time,
                    orderedEntries[end].Time,
                    orderedEntries[end].UniqueId,
                    count,
                    baseScore + count * 8.0));
            }
        }

        if (rawCandidates.Count == 0)
        {
            return [];
        }

        var selected = new List<WvwSummaryClusterWindowCandidate>();
        foreach (WvwSummaryClusterWindowCandidate candidate in rawCandidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.StartTime))
        {
            if (selected.Count >= 3)
            {
                break;
            }

            bool overlaps = selected.Any(existing =>
                candidate.StartTime <= existing.EndTime &&
                candidate.EndTime >= existing.StartTime);
            if (!overlaps)
            {
                selected.Add(candidate);
            }
        }

        return [.. selected
            .OrderBy(candidate => candidate.StartTime)
            .Select(candidate => new WvwSummaryMomentCandidate(
                candidate.StartTime,
                candidate.UniqueId,
                label,
                detailBuilder(candidate.Count),
                tone,
                category,
                candidate.Score))];
    }

    private static bool TryBuildFormationBreakCandidate(PhaseData phase, CombatReplayAnalysisDto combatReplayAnalysis, out WvwSummaryMomentCandidate candidate)
    {
        candidate = default;
        CombatReplayPositioningAnalysisDto positioning = combatReplayAnalysis.Positioning;
        if (!positioning.HasCommander || combatReplayAnalysis.Times.Length == 0)
        {
            return false;
        }

        int bestIndex = -1;
        double bestScore = 0.0;
        for (int index = 0; index < combatReplayAnalysis.Times.Length; index++)
        {
            long time = combatReplayAnalysis.Times[index];
            if (time < phase.Start || time > phase.End || positioning.Mingled[index] || positioning.EligiblePlayerCount[index] < 5)
            {
                continue;
            }

            double inPositionRate = positioning.InPositionRate[index];
            int overextended = positioning.OverextendedCount[index];
            int lateralRisk = positioning.LateralRiskCount[index];
            int tooFar = positioning.TooFarCount[index];
            double score = overextended * 4.0 + lateralRisk * 2.0 + tooFar * 1.5 + Math.Max(0, 70.0 - inPositionRate) / 4.0;
            if (overextended < 2 && inPositionRate > 60.0)
            {
                continue;
            }
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = index;
            }
        }

        if (bestIndex < 0)
        {
            return false;
        }

        candidate = new WvwSummaryMomentCandidate(
            combatReplayAnalysis.Times[bestIndex],
            0,
            "Formation broke",
            $"Only {FormatDecimal(positioning.InPositionRate[bestIndex])}% of eligible players were in position, with {positioning.OverextendedCount[bestIndex]} overextended and {positioning.TooFarCount[bestIndex]} too far from tag.",
            "negative",
            "formation-break",
            bestScore);
        return true;
    }

    private static bool TryBuildStabilityDropCandidate(
        PhaseData phase,
        CombatReplayAnalysisDto combatReplayAnalysis,
        IReadOnlyList<WvwSummaryDownEventEntryDto> squadDownEntries,
        out WvwSummaryMomentCandidate candidate)
    {
        candidate = default;
        CombatReplayThreatBoonTimelineDto? stability = combatReplayAnalysis.ThreatBoons.Boons.FirstOrDefault(boon => boon.Id == Stability);
        if (stability == null || combatReplayAnalysis.Times.Length == 0)
        {
            return false;
        }

        int bestIndex = -1;
        double bestScore = 0.0;
        for (int index = 1; index < combatReplayAnalysis.Times.Length; index++)
        {
            long time = combatReplayAnalysis.Times[index];
            if (time < phase.Start || time > phase.End)
            {
                continue;
            }

            int threatened = combatReplayAnalysis.ThreatBoons.ThreatenedPlayerCount[index];
            double coverage = stability.CurrentCoverage[index];
            double previousCoverage = stability.CurrentCoverage[index - 1];
            if (threatened < 5 || coverage > 45.0 || previousCoverage <= coverage)
            {
                continue;
            }

            int nearbyDowns = squadDownEntries.Count(entry => Math.Abs(entry.Time - time) <= 5000);
            double score = (45.0 - coverage) + (previousCoverage - coverage) + threatened + nearbyDowns * 6.0;
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = index;
            }
        }

        if (bestIndex < 0)
        {
            return false;
        }

        long bestTime = combatReplayAnalysis.Times[bestIndex];
        int bestThreatened = combatReplayAnalysis.ThreatBoons.ThreatenedPlayerCount[bestIndex];
        double bestCoverage = stability.CurrentCoverage[bestIndex];
        string downSuffix = squadDownEntries.Any(entry => Math.Abs(entry.Time - bestTime) <= 5000)
            ? " Enemy pressure followed quickly."
            : "";
        candidate = new WvwSummaryMomentCandidate(
            bestTime,
            0,
            "Stability coverage dropped",
            $"Stability coverage fell to {FormatDecimal(bestCoverage)}% across {bestThreatened} threatened squad players.{downSuffix}",
            "negative",
            "stability-drop",
            bestScore);
        return true;
    }

    private static WvwSummarySideStateDto BuildSideState(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        long time,
        SingleActor? commander = null,
        bool applyCommanderRange = false)
    {
        const float runbackRange = 3000.0f;
        var state = new WvwSummarySideStateDto();
        Vector3 commanderPosition = default;
        bool hasCommanderPosition = applyCommanderRange &&
            commander != null &&
            TryGetEligiblePosition(commander, log, time, out commanderPosition);
        foreach (SingleActor actor in actors)
        {
            if (time < actor.FirstAware || time > actor.LastAware || actor.IsDC(log, time))
            {
                continue;
            }

            if (actor.IsDead(log, time))
            {
                state.Dead++;
            }
            else if (actor.IsDowned(log, time))
            {
                state.Down++;
            }
            else
            {
                if (hasCommanderPosition &&
                    actor.UniqueID != commander!.UniqueID &&
                    TryGetEligiblePosition(actor, log, time, out Vector3 actorPosition) &&
                    !IsWithinRange(actorPosition, commanderPosition, runbackRange))
                {
                    state.Runback++;
                }
                else
                {
                    state.Alive++;
                }
            }
        }

        return state;
    }

    private static string FormatSideState(WvwSummarySideStateDto state)
    {
        string runbackSuffix = state.Runback > 0 ? $" / {state.Runback} runback" : "";
        return $"{state.Alive} alive / {state.Down} down / {state.Dead} downed{runbackSuffix}";
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
        const long ignoreTailWindow = 10000;
        List<WvwSummaryFormationSnapshot> snapshots = BuildGroupFormationSnapshots(log, phase, groupActors, opposingActors);
        int peakEligibleEnemies = snapshots.Select(snapshot => snapshot.PlayerCount).DefaultIfEmpty(0).Max();
        int minimumEligibleCount = Math.Max(5, (int)Math.Ceiling(peakEligibleEnemies * 0.6));

        var cohesionScores = new List<double>();
        int organizedSnapshots = 0;
        int cloudSnapshots = 0;

        foreach (WvwSummaryFormationSnapshot snapshot in snapshots)
        {
            if (snapshot.Time > phase.End - ignoreTailWindow ||
                snapshot.PlayerCount < minimumEligibleCount)
            {
                continue;
            }

            cohesionScores.Add(snapshot.SnapshotScore);

            if (snapshot.ClusteredShare >= 0.7 && snapshot.CompactnessScore >= 0.55)
            {
                organizedSnapshots++;
            }
            else if (snapshot.ClusteredShare <= 0.4 || snapshot.CompactnessScore <= 0.2)
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

    private static List<WvwSummaryFormationSnapshot> BuildGroupFormationSnapshots(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> groupActors,
        IReadOnlyList<SingleActor> opposingActors)
    {
        const long sampleInterval = 1000;
        const float enemyAllyRange = 600.0f;
        const float engageRange = 1600.0f;
        const float maxDistanceFromFight = 5000.0f;

        var snapshots = new List<WvwSummaryFormationSnapshot>();
        for (long time = phase.Start; time <= phase.End; time += sampleInterval)
        {
            var groupPositions = new List<Vector3>();
            foreach (SingleActor actor in groupActors)
            {
                if (TryGetEligiblePosition(actor, log, time, out Vector3 position))
                {
                    groupPositions.Add(position);
                }
            }

            var opposingPositions = new List<Vector3>();
            foreach (SingleActor actor in opposingActors)
            {
                if (TryGetEligiblePosition(actor, log, time, out Vector3 position))
                {
                    opposingPositions.Add(position);
                }
            }

            if (groupPositions.Count == 0 || opposingPositions.Count == 0)
            {
                continue;
            }

            if (!AreGroupsEngaged(groupPositions, opposingPositions, engageRange))
            {
                continue;
            }

            List<Vector3> participatingGroupPositions = FilterPositionsNearFight(groupPositions, opposingPositions, maxDistanceFromFight);
            List<Vector3> participatingOpposingPositions = FilterPositionsNearFight(opposingPositions, groupPositions, maxDistanceFromFight);
            if (participatingGroupPositions.Count == 0 || participatingOpposingPositions.Count == 0)
            {
                continue;
            }

            int playerCount = participatingGroupPositions.Count;
            int neighborThreshold = Math.Max(2, Math.Min(5, (int)Math.Round(playerCount * 0.25)));
            Vector3 groupCentroid = ComputeCentroid(participatingGroupPositions);
            Vector3 opposingCentroid = ComputeCentroid(participatingOpposingPositions);
            int clusteredPlayers = 0;
            double totalDistanceToCentroid = 0.0;

            for (int groupIndex = 0; groupIndex < participatingGroupPositions.Count; groupIndex++)
            {
                Vector3 groupPosition = participatingGroupPositions[groupIndex];
                int nearbyAllies = 0;
                for (int otherIndex = 0; otherIndex < participatingGroupPositions.Count; otherIndex++)
                {
                    if (groupIndex == otherIndex)
                    {
                        continue;
                    }
                    if (IsWithinRange(groupPosition, participatingGroupPositions[otherIndex], enemyAllyRange))
                    {
                        nearbyAllies++;
                    }
                }
                if (nearbyAllies >= neighborThreshold)
                {
                    clusteredPlayers++;
                }
                totalDistanceToCentroid += GetDistance2D(groupPosition, groupCentroid);
            }

            double clusteredShare = clusteredPlayers / (double)playerCount;
            double averageDistanceToCentroid = totalDistanceToCentroid / playerCount;
            double compactnessScore = Clamp01((900.0 - averageDistanceToCentroid) / 500.0);
            double snapshotScore = Math.Round((clusteredShare * 0.7 + compactnessScore * 0.3) * 100.0, 1);

            snapshots.Add(new WvwSummaryFormationSnapshot(
                time,
                playerCount,
                participatingOpposingPositions.Count,
                clusteredShare,
                compactnessScore,
                averageDistanceToCentroid,
                snapshotScore,
                GetDistance2D(groupCentroid, opposingCentroid)));
        }

        return snapshots;
    }

    private static bool TryBuildEnemyFormationBreakCandidate(
        PhaseData phase,
        IReadOnlyList<WvwSummaryFormationSnapshot> snapshots,
        out WvwSummaryMomentCandidate candidate)
    {
        const long ignoreTailWindow = 10000;
        candidate = default;
        if (snapshots.Count == 0)
        {
            return false;
        }

        int peakPlayers = snapshots.Select(snapshot => snapshot.PlayerCount).DefaultIfEmpty(0).Max();
        int minimumEligibleCount = Math.Max(5, (int)Math.Ceiling(peakPlayers * 0.6));
        int bestIndex = -1;
        double bestScore = 0.0;
        WvwSummaryFormationSnapshot? previousSnapshot = null;

        for (int index = 0; index < snapshots.Count; index++)
        {
            WvwSummaryFormationSnapshot snapshot = snapshots[index];
            if (snapshot.Time > phase.End - ignoreTailWindow || snapshot.PlayerCount < minimumEligibleCount)
            {
                continue;
            }

            double scoreDrop = previousSnapshot.HasValue ? Math.Max(0.0, previousSnapshot.Value.SnapshotScore - snapshot.SnapshotScore) : 0.0;
            double clusteredDrop = previousSnapshot.HasValue ? Math.Max(0.0, previousSnapshot.Value.ClusteredShare - snapshot.ClusteredShare) : 0.0;
            double spreadGain = previousSnapshot.HasValue ? Math.Max(0.0, snapshot.AverageDistanceToCentroid - previousSnapshot.Value.AverageDistanceToCentroid) : 0.0;
            bool collapsed = snapshot.ClusteredShare <= 0.45 ||
                snapshot.CompactnessScore <= 0.25 ||
                (snapshot.SnapshotScore <= 48.0 && scoreDrop >= 15.0);
            if (collapsed)
            {
                double score = (100.0 - snapshot.SnapshotScore) +
                    scoreDrop * 2.0 +
                    clusteredDrop * 45.0 +
                    spreadGain / 20.0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = index;
                }
            }

            previousSnapshot = snapshot;
        }

        if (bestIndex < 0)
        {
            return false;
        }

        WvwSummaryFormationSnapshot bestSnapshot = snapshots[bestIndex];
        candidate = new WvwSummaryMomentCandidate(
            bestSnapshot.Time,
            0,
            "Enemy formation broke",
            $"Enemy cohesion dropped to {FormatDecimal(bestSnapshot.SnapshotScore)} with {bestSnapshot.PlayerCount} players still in fight and {FormatDecimal(bestSnapshot.ClusteredShare * 100.0)}% moving in a tight group.",
            "positive",
            "enemy-formation-break",
            bestScore);
        return true;
    }

    private static bool TryBuildEnemyShatteredCandidate(
        PhaseData phase,
        IReadOnlyList<WvwSummaryFormationSnapshot> snapshots,
        IReadOnlyList<WvwSummaryDownEventEntryDto> enemyKillEntries,
        out WvwSummaryMomentCandidate candidate)
    {
        const long ignoreTailWindow = 5000;
        const long nearbyKillWindow = 5000;
        candidate = default;
        if (snapshots.Count < 2)
        {
            return false;
        }

        int peakPlayers = snapshots.Select(snapshot => snapshot.PlayerCount).DefaultIfEmpty(0).Max();
        int shatteredCountThreshold = Math.Max(4, (int)Math.Floor(peakPlayers * 0.6));
        int bestIndex = -1;
        double bestScore = 0.0;

        for (int index = 0; index < snapshots.Count; index++)
        {
            WvwSummaryFormationSnapshot snapshot = snapshots[index];
            if (snapshot.Time > phase.End - ignoreTailWindow || snapshot.PlayerCount > shatteredCountThreshold)
            {
                continue;
            }

            double retreatGain = 0.0;
            int retreatingSamples = 0;
            for (int nextIndex = index + 1; nextIndex < snapshots.Count && nextIndex <= index + 3; nextIndex++)
            {
                WvwSummaryFormationSnapshot nextSnapshot = snapshots[nextIndex];
                if (nextSnapshot.Time - snapshot.Time > 3000)
                {
                    break;
                }

                double distanceGain = nextSnapshot.DistanceToOpposingCentroid - snapshot.DistanceToOpposingCentroid;
                if (distanceGain > 150.0)
                {
                    retreatingSamples++;
                }
                retreatGain = Math.Max(retreatGain, distanceGain);
            }

            bool shattered = snapshot.SnapshotScore <= 40.0 &&
                snapshot.ClusteredShare <= 0.35 &&
                retreatingSamples > 0 &&
                retreatGain >= 250.0;
            if (!shattered)
            {
                continue;
            }

            int nearbyKills = enemyKillEntries.Count(entry => Math.Abs(entry.Time - snapshot.Time) <= nearbyKillWindow);
            double score = (peakPlayers - snapshot.PlayerCount) * 5.0 +
                (100.0 - snapshot.SnapshotScore) +
                retreatGain / 25.0 +
                nearbyKills * 6.0;
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = index;
            }
        }

        if (bestIndex < 0)
        {
            return false;
        }

        WvwSummaryFormationSnapshot bestSnapshot = snapshots[bestIndex];
        int nearbyBestKills = enemyKillEntries.Count(entry => Math.Abs(entry.Time - bestSnapshot.Time) <= nearbyKillWindow);
        string killSuffix = nearbyBestKills > 0 ? $" Our Squad secured {nearbyBestKills} nearby kills." : "";
        candidate = new WvwSummaryMomentCandidate(
            bestSnapshot.Time,
            0,
            "Enemy shattered",
            $"Enemy Team fell to {bestSnapshot.PlayerCount} players in fight from a peak of {peakPlayers} and was actively pulling away from combat.{killSuffix}",
            "positive",
            "enemy-shattered",
            bestScore);
        return true;
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
            UniqueId = actor.UniqueID,
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

internal class WvwSummaryMomentDto
{
    public long Time { get; set; }
    public long RelativeTime { get; set; }
    public string TimeLabel { get; set; } = "";
    public int UniqueId { get; set; }
    public string Label { get; set; } = "";
    public string Detail { get; set; } = "";
    public int SquadAlive { get; set; }
    public int EnemyAlive { get; set; }
    public string StateSummary { get; set; } = "";
    public string Category { get; set; } = "";
    public string Tone { get; set; } = "";
}

internal class WvwSummarySideStateDto
{
    public int Alive { get; set; }
    public int Down { get; set; }
    public int Dead { get; set; }
    public int Runback { get; set; }
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

internal readonly record struct WvwSummaryFormationSnapshot(
    long Time,
    int PlayerCount,
    int OpposingPlayerCount,
    double ClusteredShare,
    double CompactnessScore,
    double AverageDistanceToCentroid,
    double SnapshotScore,
    double DistanceToOpposingCentroid);

internal readonly record struct WvwSummaryMomentumSnapshot(
    long Time,
    int SquadAlive,
    int EnemyAlive,
    double Odds);

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
    public int UniqueId { get; set; }
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public string DetailLabel { get; set; } = "";
}

internal readonly record struct WvwSummaryMomentCandidate(
    long Time,
    int UniqueId,
    string Label,
    string Detail,
    string Tone,
    string Category,
    double Score);

internal readonly record struct WvwSummaryClusterWindowCandidate(
    long StartTime,
    long EndTime,
    int UniqueId,
    int Count,
    double Score);
