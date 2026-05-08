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
    private const int ExecutionSizeGapGracePlayers = 4;
    private const double ExecutionSizeGapScorePerPlayer = 1.5;
    private const double ExecutionSizeGapScoreCap = 15.0;
    private const double ExecutionMinimumHealingCoverage = 0.4;
    private const double ExecutionReliableHealingCoverage = 0.8;
    private const double ExecutionMinimumTrackedCleansePressurePerActivePlayerPerMinute = 15.0;
    private const int ExecutionBurstYieldMinimumTotalWindows = 6;
    private const int ExecutionBurstYieldMinimumOneSideWindows = 3;
    private const int ExecutionDownstateMinimumOpportunityDowns = 4;
    private const int ExecutionDownstateMinimumTimedEvents = 2;
    private const int ThreeWayMinimumSecondEnemyCount = 15;
    private const int ThreeWayMinimumCombinedEnemyCountWithoutColor = 65;
    private const long ThreeWayMinimumSustainMs = 8000;
    private const float ThreeWayMaxDistanceFromSquad = 5000.0f;
    private const float ThreeWaySandwichRange = 600.0f;
    private const float ThreeWaySandwichExitRange = 900.0f;
    private const long ThreeWaySandwichReseparationMs = 5000;
    private static readonly IReadOnlyList<long> OffensiveConditionBuffIds =
    [
        Vulnerability,
        Burning,
        Poison,
        Bleeding,
        Torment,
        Confusion,
    ];
    private static readonly IReadOnlyList<long> ControlConditionBuffIds =
    [
        Chilled,
        Crippled,
        Immobile,
        Fear,
        Taunt,
    ];
    private static readonly IReadOnlyList<long> DefensiveConditionBuffIds =
    [
        Weakness,
        Blind,
        Chilled,
    ];
    private static readonly IReadOnlyList<long> OffensiveSupportBoonIds =
    [
        Might,
        Fury,
        Quickness,
        Swiftness,
        Superspeed,
    ];
    private static readonly IReadOnlyList<long> DefensiveSupportBoonIds =
    [
        Stability,
        Protection,
        Resolution,
        Resistance,
        Aegis,
        Regeneration,
        Vigor,
        Swiftness,
        Superspeed,
    ];
    private static readonly IReadOnlyList<WvwSummaryExecutionBoonExpectation> ExecutionDefensiveBoonExpectations =
    [
        new(Stability, 0.30, 85.0),
        new(Protection, 0.25, 85.0),
        new(Resolution, 0.20, 85.0),
        new(Resistance, 0.15, 55.0),
        new(Regeneration, 0.10, 75.0),
        new(Aegis, 0.10, 30.0),
    ];
    private static readonly IReadOnlyList<WvwSummaryExecutionBoonExpectation> ExecutionOffensiveBoonExpectations =
    [
        new(Might, 0.35, 0.0, 18.0),
        new(Fury, 0.30, 85.0),
        new(Quickness, 0.35, 55.0),
    ];

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
    public WvwSummaryFightExecutionScoreDto FightExecutionScore { get; set; } = new();
    public List<WvwSummaryMetricRowDto> MetricRows { get; set; } = [];
    public List<WvwSummaryMomentDto> Moments { get; set; } = [];
    public WvwSummaryPlayerStandoutsDto PlayerStandouts { get; set; } = new();
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
        squad.BoonCorrupts = CombatReplayAnalysisBuilder.CountBoonCorrupts(log, phase.Start, phase.End, squadActors, hostilePlayerTargets);
        enemy.BoonCorrupts = CombatReplayAnalysisBuilder.CountBoonCorrupts(log, phase.Start, phase.End, hostilePlayerTargets, squadActors);
        double durationInMinutes = durationInSeconds / 60.0;
        squad.CorruptsPerMinute = Math.Round(squad.BoonCorrupts / durationInMinutes, 1);
        enemy.CorruptsPerMinute = Math.Round(enemy.BoonCorrupts / durationInMinutes, 1);
        var squadDownState = BuildDownStateSide(log, phase, squadActors);
        var enemyDownState = BuildDownStateSide(log, phase, hostilePlayerTargets);
        GW2EIBuilders.WvWAnalystFightShapeDto fightShape = GW2EIBuilders.WvWAnalystBuilder.BuildFightShapeDiagnostics(
            log,
            phase,
            squadActors,
            hostilePlayerTargets,
            combatReplayAnalysis,
            BuildFightShapeOutcome(squad, enemy));
        List<WvwSummaryMomentDto> moments = BuildMoments(log, phase, combatReplayAnalysis, squadActors, hostilePlayerTargets, squadDownState, enemyDownState, fightShape);

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
            FightExecutionScore = BuildFightExecutionScore(log, phase, combatReplayAnalysis, squadActors, friendlyActors, hostilePlayerTargets, squad, enemy, squadDownState, enemyDownState, fightShape),
            Squad = squad,
            Enemy = enemy,
            MetricRows = BuildMetricRows(durationInMilliseconds, squad, enemy, squadDownState, enemyDownState, friendlyActors.Count),
            Moments = moments,
            PlayerStandouts = BuildPlayerStandouts(log, phase, combatReplayAnalysis, moments, squadActors, hostilePlayerTargets),
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

    private static WvwSummaryFightExecutionScoreDto BuildFightExecutionScore(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> friendlyActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummarySideDto squad,
        WvwSummarySideDto enemy,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState,
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape)
    {
        var result = new WvwSummaryFightExecutionScoreDto
        {
            Context = BuildFightExecutionContext(log, phase, squadActors, hostilePlayerTargets, friendlyActors.Count, fightShape),
            Outcome = BuildFightExecutionOutcome(log, phase, squadActors, hostilePlayerTargets, squad, enemy, squadDownState, enemyDownState),
            Summary = "Raw execution is scored separately from context and outcome. A numbers-adjusted interpretation is shown beside it when the active player gap is material. Comp, terrain, opponent difficulty, and raw win/loss are intentionally excluded from the raw execution score.",
            Detail = "Uses only high-confidence inputs already present in the detailed WvW summary and combat replay. If a scored metric is unavailable, that metric is neutralized at 50 instead of guessed.",
        };

        if (combatReplayAnalysis == null || combatReplayAnalysis.Times.Length == 0)
        {
            const int totalMetricCount = 18;
            result.ScoreAvailable = false;
            result.Confidence = new WvwSummaryExecutionConfidenceDto
            {
                Label = "Low",
                AvailableMetricCount = 0,
                TotalMetricCount = totalMetricCount,
                Notes = ["Combat Replay is required for the execution score in this v1."],
            };
            result.Context.DataConfidenceLabel = result.Confidence.Label;
            result.Context.DataConfidenceDetail = $"0 of {totalMetricCount} scored metrics are available because Combat Replay is missing for this log.";
            result.Detail = "Combat Replay is required for the full execution scorecard. Context and outcome are still shown so the fight can be reviewed without inventing replay-based values.";
            return result;
        }

        double squadPlayers = Math.Max(squad.PlayerCount, 1);
        double enemyPlayers = Math.Max(enemy.PlayerCount, 1);
        List<long> squadDownTimes = GetPhaseDownTimes(log, squadActors, phase);
        List<long> enemyDownTimes = GetPhaseDownTimes(log, hostilePlayerTargets, phase);
        List<WvwSummaryExecutionWindow> squadBurstWindows = BuildPhaseBurstWindows(combatReplayAnalysis.Squad, combatReplayAnalysis.Times, phase.Start, phase.End, combatReplayAnalysis.Lookback);
        List<WvwSummaryExecutionWindow> enemyBurstWindows = BuildPhaseBurstWindows(combatReplayAnalysis.Enemy, combatReplayAnalysis.Times, phase.Start, phase.End, combatReplayAnalysis.Lookback);
        WvwSummaryPhasePositioningDto positioningMetrics = BuildPhasePositioningMetrics(combatReplayAnalysis, phase);
        WvwSummaryPhasePositioningDto pressurePositioningMetrics = BuildPhasePositioningMetrics(combatReplayAnalysis, phase, enemyBurstWindows);
        int squadBurstDownCount = CountEventsInsideWindows(squadBurstWindows, enemyDownTimes);
        int enemyBurstDownCount = CountEventsInsideWindows(enemyBurstWindows, squadDownTimes);

        const double cohesionInPositionWeight = 25.0;
        const double cohesionPressurePositioningWeight = 25.0;
        const double cohesionTooFarWeight = 20.0;
        const double cohesionLateralRiskWeight = 20.0;
        const double cohesionOverextendedWeight = 10.0;
        var cohesionMetrics = new List<WvwSummaryExecutionMetricDto>(5);
        if (positioningMetrics.HasData)
        {
            cohesionMetrics.Add(BuildInPositionExecutionMetric(positioningMetrics.InPositionRate, positioningMetrics.EvaluatedSamples, cohesionInPositionWeight));
            if (pressurePositioningMetrics.HasData)
            {
                cohesionMetrics.Add(BuildPressurePositioningExecutionMetric(
                    pressurePositioningMetrics.InPositionRate,
                    pressurePositioningMetrics.EvaluatedSamples,
                    enemyBurstWindows.Count,
                    cohesionPressurePositioningWeight));
            }
            else
            {
                string pressurePositioningNote = enemyBurstWindows.Count == 0
                    ? "Neutralized at 50: no enemy pressure windows were detected in this phase."
                    : "Neutralized at 50: no eligible commander-relative replay samples overlapped enemy pressure windows.";
                cohesionMetrics.Add(BuildNeutralizedExecutionMetric("Pressure positioning", pressurePositioningNote, cohesionPressurePositioningWeight));
            }
            cohesionMetrics.Add(BuildPositioningExecutionMetric("Too-far rate", positioningMetrics.TooFarRate, positioningMetrics.EvaluatedSamples, higherIsBetter: false, weightPercent: cohesionTooFarWeight));
            cohesionMetrics.Add(BuildPositioningExecutionMetric("Lateral-risk rate", positioningMetrics.LateralRiskRate, positioningMetrics.EvaluatedSamples, higherIsBetter: false, weightPercent: cohesionLateralRiskWeight));
            cohesionMetrics.Add(BuildPositioningExecutionMetric("Overextended rate", positioningMetrics.OverextendedRate, positioningMetrics.EvaluatedSamples, higherIsBetter: false, weightPercent: cohesionOverextendedWeight));
        }
        else
        {
            string positioningNote = combatReplayAnalysis.Positioning.HasCommander
                ? "Neutralized at 50: no eligible commander-relative replay samples fell inside this phase."
                : "Neutralized at 50: commander-relative positioning requires a detected squad commander.";
            cohesionMetrics.Add(BuildNeutralizedExecutionMetric("In-position rate", positioningNote, cohesionInPositionWeight));
            cohesionMetrics.Add(BuildNeutralizedExecutionMetric("Pressure positioning", positioningNote, cohesionPressurePositioningWeight));
            cohesionMetrics.Add(BuildNeutralizedExecutionMetric("Too-far rate", positioningNote, cohesionTooFarWeight));
            cohesionMetrics.Add(BuildNeutralizedExecutionMetric("Lateral-risk rate", positioningNote, cohesionLateralRiskWeight));
            cohesionMetrics.Add(BuildNeutralizedExecutionMetric("Overextended rate", positioningNote, cohesionOverextendedWeight));
        }
        string cohesionSummary = positioningMetrics.HasData
            ? $"{FormatDecimal(positioningMetrics.InPositionRate)}% in position, {FormatDecimal(positioningMetrics.TooFarRate)}% too far, {FormatDecimal(positioningMetrics.OverextendedRate)}% overextended, and {FormatDecimal(positioningMetrics.LateralRiskRate)}% lateral risk over {positioningMetrics.EvaluatedSamples.ToString("N0", CultureInfo.InvariantCulture)} eligible replay samples. {BuildPressurePositioningSummary(pressurePositioningMetrics, enemyBurstWindows.Count)}"
            : "Commander-relative positioning could not be scored for this phase, so the pillar was neutralized.";

        double squadDownsPerActivePlayer = enemyDownState.Downs / squadPlayers;
        double enemyDownsPerActivePlayer = squadDownState.Downs / enemyPlayers;
        double squadStripsPerActivePlayerPerMinute = squad.StripsPerMinute / squadPlayers;
        double enemyStripsPerActivePlayerPerMinute = enemy.StripsPerMinute / enemyPlayers;

        const double pressureDownsWeight = 35.0;
        const double pressureBurstYieldWeight = 25.0;
        const double pressureOffensiveBoonWeight = 20.0;
        const double pressureStripWeight = 20.0;
        var pressureMetrics = new List<WvwSummaryExecutionMetricDto>(4)
        {
            BuildRelativeExecutionMetric(
                "Downs per active player",
                squadDownsPerActivePlayer,
                enemyDownsPerActivePlayer,
                higherIsBetter: true,
                $"{FormatDecimal(squadDownsPerActivePlayer)} vs enemy {FormatDecimal(enemyDownsPerActivePlayer)} downs per active player",
                weightPercent: pressureDownsWeight),
            BuildRelativeExecutionMetric(
                "Strips per active player per minute",
                squadStripsPerActivePlayerPerMinute,
                enemyStripsPerActivePlayerPerMinute,
                higherIsBetter: true,
                $"{FormatDecimal(squadStripsPerActivePlayerPerMinute)} vs enemy {FormatDecimal(enemyStripsPerActivePlayerPerMinute)} strips per active player per minute",
                weightPercent: pressureStripWeight),
        };
        double squadBurstDownsPerTenActivePlayers = squadBurstDownCount * 10.0 / squadPlayers;
        double enemyBurstDownsPerTenActivePlayers = enemyBurstDownCount * 10.0 / enemyPlayers;
        int totalBurstWindowCount = squadBurstWindows.Count + enemyBurstWindows.Count;
        bool hasMeaningfulBurstYieldSample =
            totalBurstWindowCount >= ExecutionBurstYieldMinimumTotalWindows &&
            Math.Max(squadBurstWindows.Count, enemyBurstWindows.Count) >= ExecutionBurstYieldMinimumOneSideWindows;
        if (hasMeaningfulBurstYieldSample)
        {
            pressureMetrics.Insert(1,
                BuildRelativeExecutionMetric(
                    "Burst pressure yield",
                    squadBurstDownsPerTenActivePlayers,
                    enemyBurstDownsPerTenActivePlayers,
                    higherIsBetter: true,
                    $"{FormatDecimal(squadBurstDownsPerTenActivePlayers)} vs enemy {FormatDecimal(enemyBurstDownsPerTenActivePlayers)} burst-window downs per 10 active players",
                    $"{squadBurstDownCount} enemy downs inside {BuildPluralizedTag(squadBurstWindows.Count, "detected squad burst window", "detected squad burst windows")}; enemy created {enemyBurstDownCount} squad downs inside {BuildPluralizedTag(enemyBurstWindows.Count, "detected burst window", "detected burst windows")}.",
                    pressureBurstYieldWeight));
        }
        else
        {
            string burstSampleLabel = BuildPluralizedTag(totalBurstWindowCount, "detected burst window", "detected burst windows");
            string burstSampleVerb = totalBurstWindowCount == 1 ? "was" : "were";
            string burstNote = totalBurstWindowCount == 0
                ? "Neutralized at 50: no detected burst windows were produced in this phase."
                : $"Neutralized at 50: only {burstSampleLabel} {burstSampleVerb} found, too little to judge burst pressure yield without over-reading a tiny sample.";
            pressureMetrics.Insert(1, BuildNeutralizedExecutionMetric("Burst pressure yield", burstNote, pressureBurstYieldWeight));
        }
        if (TryComputePhaseTargetAdjustedThreatBoonCoverage(
            combatReplayAnalysis,
            phase,
            ExecutionOffensiveBoonExpectations,
            squadBurstWindows,
            out double offensiveBoonCoverage,
            out string offensiveBoonCoverageNote))
        {
            pressureMetrics.Insert(2,
                BuildBoonCoverageExecutionMetric(
                    "Offensive boon coverage",
                    offensiveBoonCoverage,
                    $"{FormatDecimal(offensiveBoonCoverage)}% target-adjusted offensive boon coverage",
                    $"{offensiveBoonCoverageNote} Might uses a practical average-stack target, Quickness uses a lower WvW uptime target, and Alacrity is intentionally excluded.",
                    pressureOffensiveBoonWeight));
        }
        else
        {
            string offensiveBoonNote = squadBurstWindows.Count == 0
                ? "Neutralized at 50: no detected squad pressure windows were produced in this phase."
                : "Neutralized at 50: no threatened squad boon samples overlapped detected squad pressure windows.";
            pressureMetrics.Insert(2, BuildNeutralizedExecutionMetric("Offensive boon coverage", offensiveBoonNote, pressureOffensiveBoonWeight));
        }
        string pressureSummary = $"{FormatDecimal(squadDownsPerActivePlayer)} downs per active player and {FormatDecimal(squadStripsPerActivePlayerPerMinute)} strips per active player per minute.";
        if (pressureMetrics[1].Available)
        {
            pressureSummary = $"{pressureSummary} Detected burst windows produced {FormatDecimal(squadBurstDownsPerTenActivePlayers)} enemy downs per 10 squad players vs {FormatDecimal(enemyBurstDownsPerTenActivePlayers)} squad downs per 10 enemy players.";
        }
        else
        {
            pressureSummary = $"{pressureSummary} Burst pressure yield was neutralized because the detected burst sample was too small.";
        }
        if (pressureMetrics[2].Available)
        {
            pressureSummary = $"{pressureSummary} Offensive boon coverage scored {FormatDecimal(offensiveBoonCoverage)}% target-adjusted during squad pressure windows.";
        }
        else
        {
            pressureSummary = $"{pressureSummary} Offensive boon coverage was neutralized because there were no usable squad pressure-window boon samples.";
        }

        const double downstateConversionRateWeight = 35.0;
        const double downstateKillTimeWeight = 15.0;
        const double downstateRecoveryRateWeight = 35.0;
        const double downstateRecoveryTimeWeight = 15.0;
        bool hasOffensiveDownOpportunity = enemyDownState.Downs >= ExecutionDownstateMinimumOpportunityDowns;
        bool hasDefensiveRecoveryOpportunity = squadDownState.Downs >= ExecutionDownstateMinimumOpportunityDowns;
        var downstateMetrics = new List<WvwSummaryExecutionMetricDto>(4);
        if (hasOffensiveDownOpportunity)
        {
            string enemyConversionComparison = squadDownState.Downs > 0
                ? $"{squadDownState.KillConversions}/{squadDownState.Downs}"
                : "no";
            downstateMetrics.Add(
                BuildRelativeExecutionMetric(
                    "Enemy down conversion rate",
                    enemyDownState.KillConversionRate,
                    squadDownState.KillConversionRate,
                    higherIsBetter: true,
                    $"{FormatDecimal(enemyDownState.KillConversionRate)}% vs enemy {FormatDecimal(squadDownState.KillConversionRate)}%",
                    $"{enemyDownState.KillConversions}/{enemyDownState.Downs} enemy downs converted; enemy converted {enemyConversionComparison} squad downs.",
                    downstateConversionRateWeight));
        }
        else
        {
            downstateMetrics.Add(BuildNeutralizedExecutionMetric(
                "Enemy down conversion rate",
                $"Neutralized at 50: the squad created only {BuildPluralizedTag(enemyDownState.Downs, "enemy down", "enemy downs")}, too little to judge offensive downstate control cleanly.",
                downstateConversionRateWeight));
        }
        bool hasKillTimeSample =
            hasOffensiveDownOpportunity &&
            hasDefensiveRecoveryOpportunity &&
            enemyDownState.KillConversions >= ExecutionDownstateMinimumTimedEvents &&
            squadDownState.KillConversions >= ExecutionDownstateMinimumTimedEvents &&
            enemyDownState.AverageKillTime.HasValue &&
            squadDownState.AverageKillTime.HasValue;
        if (hasKillTimeSample)
        {
            downstateMetrics.Add(
                BuildRelativeExecutionMetric(
                    "Enemy average down-to-kill time",
                    enemyDownState.AverageKillTime.GetValueOrDefault(),
                    squadDownState.AverageKillTime.GetValueOrDefault(),
                    higherIsBetter: false,
                    $"{FormatOptionalSeconds(enemyDownState.AverageKillTime)} vs enemy {FormatOptionalSeconds(squadDownState.AverageKillTime)}",
                    $"{enemyDownState.KillConversions} squad kill conversions timed; enemy had {squadDownState.KillConversions}.",
                    downstateKillTimeWeight));
        }
        else
        {
            string killTimeNote = !hasOffensiveDownOpportunity || !hasDefensiveRecoveryOpportunity
                ? "Neutralized at 50: down-to-kill timing needs enough downs from both sides to avoid over-reading a lopsided fight."
                : $"Neutralized at 50: down-to-kill timing needs at least {ExecutionDownstateMinimumTimedEvents} kill conversions from each side.";
            downstateMetrics.Add(BuildNeutralizedExecutionMetric(
                "Enemy average down-to-kill time",
                killTimeNote,
                downstateKillTimeWeight));
        }
        bool hasRecoveryComparison = hasDefensiveRecoveryOpportunity && enemyDownState.Downs > 0;
        if (hasRecoveryComparison)
        {
            downstateMetrics.Add(
                BuildRelativeExecutionMetric(
                    "Own recovery rate",
                    squadDownState.RezRate,
                    enemyDownState.RezRate,
                    higherIsBetter: true,
                    $"{FormatDecimal(squadDownState.RezRate)}% vs enemy {FormatDecimal(enemyDownState.RezRate)}%",
                    $"{squadDownState.Rezzes}/{squadDownState.Downs} squad downs recovered; enemy recovered {enemyDownState.Rezzes}/{enemyDownState.Downs}.",
                    downstateRecoveryRateWeight));
        }
        else
        {
            downstateMetrics.Add(BuildNeutralizedExecutionMetric(
                "Own recovery rate",
                hasDefensiveRecoveryOpportunity
                    ? "Neutralized at 50: the enemy had no downs, so there was no recovery comparison side."
                    : $"Neutralized at 50: the squad took only {BuildPluralizedTag(squadDownState.Downs, "squad down", "squad downs")}, too little to judge defensive recovery cleanly.",
                downstateRecoveryRateWeight));
        }
        bool hasRecoveryTimeSample =
            hasOffensiveDownOpportunity &&
            hasDefensiveRecoveryOpportunity &&
            squadDownState.Rezzes >= ExecutionDownstateMinimumTimedEvents &&
            enemyDownState.Rezzes >= ExecutionDownstateMinimumTimedEvents &&
            squadDownState.AverageRezTime.HasValue &&
            enemyDownState.AverageRezTime.HasValue;
        if (hasRecoveryTimeSample)
        {
            downstateMetrics.Add(
                BuildRelativeExecutionMetric(
                    "Own average down-to-recover time",
                    squadDownState.AverageRezTime.GetValueOrDefault(),
                    enemyDownState.AverageRezTime.GetValueOrDefault(),
                    higherIsBetter: false,
                    $"{FormatOptionalSeconds(squadDownState.AverageRezTime)} vs enemy {FormatOptionalSeconds(enemyDownState.AverageRezTime)}",
                    $"{squadDownState.Rezzes} squad recoveries timed; enemy had {enemyDownState.Rezzes}.",
                    downstateRecoveryTimeWeight));
        }
        else
        {
            string recoveryTimeNote = !hasOffensiveDownOpportunity || !hasDefensiveRecoveryOpportunity
                ? "Neutralized at 50: recovery timing needs enough downs from both sides to avoid over-reading a lopsided fight."
                : $"Neutralized at 50: recovery timing needs at least {ExecutionDownstateMinimumTimedEvents} recoveries from each side.";
            downstateMetrics.Add(BuildNeutralizedExecutionMetric(
                "Own average down-to-recover time",
                recoveryTimeNote,
                downstateRecoveryTimeWeight));
        }
        string downstateSummary = $"{BuildDownstateContextSummary(squadDownState, enemyDownState)} {FormatDecimal(enemyDownState.KillConversionRate)}% of enemy downs were converted and the squad recovered {FormatDecimal(squadDownState.RezRate)}% of its own downs.";
        if (downstateMetrics[1].Available && downstateMetrics[3].Available)
        {
            downstateSummary = $"{downstateSummary} Enemy downs were finished in {FormatOptionalSeconds(enemyDownState.AverageKillTime)} and squad recoveries resolved in {FormatOptionalSeconds(squadDownState.AverageRezTime)}.";
        }
        else
        {
            downstateSummary = $"{downstateSummary} Low-opportunity or one-sided timing comparisons were neutralized.";
        }

        int healAddonPlayerCount = GetHealingAddonPlayerCount(log, squadActors);
        double healAddonCoverage = squadActors.Count > 0 ? healAddonPlayerCount * 1.0 / squadActors.Count : 0.0;
        string healAddonCoverageLabel = $"{healAddonPlayerCount}/{squadActors.Count} squad players ({FormatDecimal(healAddonCoverage * 100.0)}%) had Healing Stats";
        (long squadHealthDamageTaken, long squadBarrierAbsorbed) = ComputePhaseSquadIncomingDamage(log, squadActors, phase);
        double squadNegatedDamage = ComputePhaseEstimatedNegatedDamage(combatReplayAnalysis.Defense, phase);
        long squadPetMinionAbsorption = ComputePhasePetMinionAbsorption(log, squadActors, phase);
        long squadHealingTotal = log.CombatData.HasEXTHealing
            ? squadActors.Sum(actor => actor.EXTHealing.GetOutgoingHealStats(null, log, phase.Start, phase.End).Healing)
            : 0;
        CombatReplayDefenseSavedPlayersSummaryDto savedPlayersSummary = BuildPhaseSavedPlayersSummary(combatReplayAnalysis.Defense, phase);

        var supportMetrics = new List<WvwSummaryExecutionMetricDto>(5);
        if (log.CombatData.HasEXTHealing && healAddonCoverage >= ExecutionMinimumHealingCoverage && squadHealthDamageTaken > 0)
        {
            double healingScore = CompareScore(squadHealingTotal, squadHealthDamageTaken);
            bool reliableHealingSample = healAddonCoverage >= ExecutionReliableHealingCoverage;
            string healingValue = $"{FormatWholeNumber(squadHealingTotal)} observed healing vs {FormatWholeNumber(squadHealthDamageTaken)} squad health damage";
            string healingNote = healAddonPlayerCount == squadActors.Count
                ? $"{healAddonCoverageLabel}."
                : $"{healAddonCoverageLabel}. Observed healing is likely understated because some squad healing is missing from the add-on sample.";
            if (reliableHealingSample || healingScore >= 50.0)
            {
                string confidenceNote = reliableHealingSample
                    ? $"{healingNote} Reliable enough to score both high and low outcomes."
                    : $"{healingNote} Partial Healing Stats coverage: this can give credit for strong observed healing, but low observed healing is neutralized instead of treated as proof of poor healing.";
                supportMetrics.Add(BuildFixedScoreExecutionMetric(
                    "Observed healing coverage",
                    healingValue,
                    confidenceNote,
                    healingScore));
            }
            else
            {
                supportMetrics.Add(BuildNeutralizedExecutionMetric(
                    "Observed healing coverage",
                    $"Neutralized at 50: {healingValue}. {healingNote} Partial Healing Stats coverage below {FormatDecimal(ExecutionReliableHealingCoverage * 100.0)}% is not allowed to penalize healing when observed healing is low."));
            }
        }
        else
        {
            string healingNote = !log.CombatData.HasEXTHealing
                ? $"Neutralized at 50: Healing Stats add-on data is unavailable for this log. {healAddonCoverageLabel}."
                : healAddonCoverage < ExecutionMinimumHealingCoverage
                    ? $"Neutralized at 50: observed healing coverage needs at least {FormatDecimal(ExecutionMinimumHealingCoverage * 100.0)}% squad Healing Stats coverage before it can even give positive credit; {healAddonCoverageLabel}."
                    : "Neutralized at 50: squad health damage was zero in this phase.";
            supportMetrics.Add(BuildNeutralizedExecutionMetric("Observed healing coverage", healingNote));
        }
        WvwSummaryExecutionMetricDto cleansePressureMetric = BuildCleansePressureExecutionMetric(log, phase, squadActors, squadPlayers, out string cleansePressureSummary);
        supportMetrics.Add(cleansePressureMetric);
        if (TryComputePhaseTargetAdjustedThreatBoonCoverage(
            combatReplayAnalysis,
            phase,
            ExecutionDefensiveBoonExpectations,
            windows: null,
            out double defensiveBoonCoverage,
            out string defensiveBoonCoverageNote))
        {
            supportMetrics.Add(
                BuildBoonCoverageExecutionMetric(
                    "Defensive boon coverage",
                    defensiveBoonCoverage,
                    $"{FormatDecimal(defensiveBoonCoverage)}% target-adjusted defensive boon coverage",
                    $"{defensiveBoonCoverageNote} Resistance, Regeneration, and Aegis use WvW-specific targets; Alacrity is intentionally excluded."));
        }
        else
        {
            supportMetrics.Add(BuildNeutralizedExecutionMetric(
                "Defensive boon coverage",
                "Neutralized at 50: the squad never had threatened replay samples for defensive boon coverage in this phase."));
        }
        WvwSummaryExecutionMetricDto preventionMetric = BuildPreventionExecutionMetric(
            squadBarrierAbsorbed,
            squadNegatedDamage,
            squadPetMinionAbsorption,
            squadHealthDamageTaken,
            savedPlayersSummary,
            out string preventionSummary);
        supportMetrics.Add(preventionMetric);
        if (savedPlayersSummary.SavedCases > 0 || squadDownState.Downs > 0)
        {
            supportMetrics.Add(
                BuildRelativeExecutionMetric(
                    "Saved-player balance",
                    savedPlayersSummary.SavedCases,
                    squadDownState.Downs,
                    higherIsBetter: true,
                    $"{savedPlayersSummary.SavedCases} saved cases vs {squadDownState.Downs} squad downs",
                    $"{savedPlayersSummary.BarrierSavedCases} barrier saves, {savedPlayersSummary.DamageReductionSavedCases} damage-reduction saves, and {savedPlayersSummary.NegatedDamageSavedCases} negated-damage saves were detected."));
        }
        else
        {
            supportMetrics.Add(BuildNeutralizedExecutionMetric(
                "Saved-player balance",
                "Neutralized at 50: this phase had no detected saved-player cases and no squad downs to compare against."));
        }
        string supportSummary = cleansePressureSummary;
        if (supportMetrics[0].Available)
        {
            supportSummary = $"{supportSummary} {FormatWholeNumber(squadHealingTotal)} observed healing from {healAddonCoverageLabel} covered against {FormatWholeNumber(squadHealthDamageTaken)} squad health damage.";
        }
        else
        {
            supportSummary = $"{supportSummary} Observed healing coverage was neutralized; {healAddonCoverageLabel}.";
        }
        if (supportMetrics[2].Available)
        {
            supportSummary = $"{supportSummary} Defensive boon coverage scored {FormatDecimal(defensiveBoonCoverage)}% target-adjusted during threatened moments.";
        }
        if (supportMetrics[3].Available)
        {
            supportSummary = $"{supportSummary} {preventionSummary}";
        }
        if (supportMetrics[4].Available)
        {
            supportSummary = $"{supportSummary} {savedPlayersSummary.SavedCases} saved cases were detected against {squadDownState.Downs} squad downs.";
        }

        result.Pillars =
        [
            BuildExecutionPillar(
                "cohesion-positioning",
                "Cohesion & Positioning",
                cohesionMetrics,
                cohesionSummary,
                "Uses eligible non-commander squad replay samples only. Enemy commander-relative positioning is not observable in current detailed WvW outputs. Pressure positioning scores in-position rate only during enemy pressure windows; the other positioning metrics score full-phase tag discipline and risk exposure."),
            BuildExecutionPillar(
                "pressure-burst",
                "Pressure & Burst",
                pressureMetrics,
                pressureSummary,
                "Compares phase-level downs per active player, burst-window down yield, target-adjusted offensive boon coverage during squad pressure windows, and strips per active player per minute against the enemy. Offensive boon coverage tracks Might, Fury, and Quickness; Might is scored against a practical stack target, Quickness uses a lower WvW uptime target, and Alacrity is intentionally excluded. Burst pressure yield is neutralized when there are too few detected burst windows to avoid over-reading a one-window sample."),
            BuildExecutionPillar(
                "downstate-control",
                "Downstate Control",
                downstateMetrics,
                downstateSummary,
                "Compares conversion, recovery, and the time spent in downstate after downs exist. Conversion and recovery rates carry more weight than timing. Low-opportunity and lopsided downstate samples are neutralized instead of treating a tiny denominator as a clean downstate contest."),
            BuildExecutionPillar(
                "support-mitigation",
                "Support & Mitigation",
                supportMetrics,
                supportSummary,
                $"Blends observed healing coverage, pressure-gated cleanse response, target-adjusted defensive boon coverage, prevention value, and saved-player mitigation. Observed healing coverage needs at least {FormatDecimal(ExecutionMinimumHealingCoverage * 100.0)}% squad Healing Stats coverage to give positive credit and at least {FormatDecimal(ExecutionReliableHealingCoverage * 100.0)}% to penalize low observed healing; below that reliable threshold, incomplete add-on data can lift the score but cannot drag it under neutral. Defensive boon coverage tracks Stability, Protection, Resolution, Resistance, and Aegis during threatened moments; Resistance uses a lower WvW target because keeping it near the easier boons requires meaningful tradeoffs. Prevention value uses barrier absorbed, estimated negated damage, and discounted pet/minion absorption as pre-hit mitigation signals rather than only counting saves. Cleanse response is self-scored only when tracked condition pressure reaches at least {FormatDecimal(ExecutionMinimumTrackedCleansePressurePerActivePlayerPerMinute)} condition-seconds per active player per minute. Each tracked condition, including Vulnerability, counts as present or absent rather than by stack count, and faster cleanses get more credit because they remove more remaining duration. Missing support inputs are neutralized at 50 instead of guessed."),
        ];

        result.ScoreAvailable = true;
        result.OverallScore = (int)Math.Round(result.Pillars.Average(pillar => pillar.Score));
        result.Grade = ScoreToGrade(result.OverallScore);
        result.NumbersAdjustment = BuildFightExecutionNumbersAdjustment(result.Pillars, squad.PlayerCount, enemy.PlayerCount, result.OverallScore);
        result.Confidence = BuildFightExecutionConfidence(result.Pillars);
        result.Context.DataConfidenceLabel = result.Confidence.Label;
        result.Context.DataConfidenceDetail = result.Confidence.AvailableMetricCount == result.Confidence.TotalMetricCount
            ? $"All {result.Confidence.TotalMetricCount} scored metrics were available for this phase."
            : $"{result.Confidence.AvailableMetricCount} of {result.Confidence.TotalMetricCount} scored metrics were available. Missing metrics were neutralized at 50 instead of guessed.";

        WvwSummaryExecutionPillarDto strongestPillar = result.Pillars
            .OrderByDescending(pillar => pillar.Score)
            .ThenBy(pillar => pillar.Label, StringComparer.OrdinalIgnoreCase)
            .First();
        WvwSummaryExecutionPillarDto weakestPillar = result.Pillars
            .OrderBy(pillar => pillar.Score)
            .ThenBy(pillar => pillar.Label, StringComparer.OrdinalIgnoreCase)
            .First();
        if (strongestPillar.Label != weakestPillar.Label || strongestPillar.Score != weakestPillar.Score)
        {
            result.StrongestPillarLabel = strongestPillar.Label;
            result.StrongestPillarSummary = strongestPillar.Summary;
            result.WeakestPillarLabel = weakestPillar.Label;
            result.WeakestPillarSummary = weakestPillar.Summary;
        }
        return result;
    }

    private static WvwSummaryExecutionContextDto BuildFightExecutionContext(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        int friendlyNonSquadCount,
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape)
    {
        var context = new WvwSummaryExecutionContextDto
        {
            SquadPlayerCount = squadActors.Count,
            EnemyPlayerCount = hostilePlayerTargets.Count,
            FriendlyNonSquadCount = friendlyNonSquadCount,
            PhaseDuration = ToDurationString(Math.Max(phase.DurationInMS, 1)),
        };
        PopulateEnemyFormationStyleContext(log, phase, hostilePlayerTargets, squadActors, fightShape, context);
        return context;
    }

    private static void PopulateEnemyFormationStyleContext(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        IReadOnlyList<SingleActor> squadActors,
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape,
        WvwSummaryExecutionContextDto context)
    {
        const long ignoreTailWindow = 10000;

        List<WvwSummaryFormationSnapshot> enemyFormationSnapshots = BuildGroupFormationSnapshots(log, phase, hostilePlayerTargets, squadActors);
        WvwSummaryThreeWayContext threeWayContext = BuildThreeWayContext(log, phase, squadActors, hostilePlayerTargets);
        context.ThreeWayDetected = threeWayContext.Detected;
        context.ThreeWayStartTimeMs = threeWayContext.StartTimeMs;
        context.ThreeWaySecondEnemyPeakCount = threeWayContext.SecondEnemyPeakCount;
        context.ThreeWayLabel = threeWayContext.Label;
        context.ThreeWayDetail = threeWayContext.Detail;
        int peakPlayers = enemyFormationSnapshots.Select(snapshot => snapshot.PlayerCount).DefaultIfEmpty(0).Max();
        int minimumEligibleCount = Math.Max(5, (int)Math.Ceiling(peakPlayers * 0.6));
        int eligibleSnapshotCount = 0;
        int organizedSnapshotCount = 0;
        int cloudSnapshotCount = 0;

        foreach (WvwSummaryFormationSnapshot snapshot in enemyFormationSnapshots)
        {
            if (snapshot.Time > phase.End - ignoreTailWindow || snapshot.PlayerCount < minimumEligibleCount)
            {
                continue;
            }

            eligibleSnapshotCount++;
            if (snapshot.ClusteredShare >= 0.7 && snapshot.CompactnessScore >= 0.55)
            {
                organizedSnapshotCount++;
            }
            else if (snapshot.ClusteredShare <= 0.4 || snapshot.CompactnessScore <= 0.2)
            {
                cloudSnapshotCount++;
            }
        }

        if (eligibleSnapshotCount == 0)
        {
            context.EnemyFormationStyleLabel = "Insufficient data";
            context.EnemyFormationStyleDetail = "Could not evaluate enough engaged enemy replay snapshots to judge whether the enemy moved as an organized group, cloud, or mixed formation.";
            context.EnemyMovementScoreLabel = "Insufficient data";
            context.EnemyMovementScoreDetail = "Could not evaluate enough engaged pre-cleanup enemy replay samples for a movement tightness score.";
            return;
        }

        double organizedRate = Math.Round(organizedSnapshotCount * 100.0 / eligibleSnapshotCount, 1);
        double cloudRate = Math.Round(cloudSnapshotCount * 100.0 / eligibleSnapshotCount, 1);
        context.EnemyFormationStyleLabel = organizedRate >= 55.0 && cloudRate <= 20.0
            ? "Organized"
            : cloudRate >= 40.0
                ? "Cloud"
                : "Mixed";
        context.EnemyFormationStyleDetail = $"{FormatDecimal(organizedRate)}% organized snapshots and {FormatDecimal(cloudRate)}% cloud snapshots across {eligibleSnapshotCount.ToString("N0", CultureInfo.InvariantCulture)} engaged replay samples. Ignores the last 10s and routed cleanup after the enemy falls below 60% of peak active count.";
        PopulateEnemyMovementScoreContext(phase, enemyFormationSnapshots, fightShape, context);
    }

    private static void PopulateEnemyMovementScoreContext(
        PhaseData phase,
        IReadOnlyList<WvwSummaryFormationSnapshot> enemyFormationSnapshots,
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape,
        WvwSummaryExecutionContextDto context)
    {
        const long ignoreTailWindow = 10000;

        int peakPlayers = enemyFormationSnapshots.Select(snapshot => snapshot.PlayerCount).DefaultIfEmpty(0).Max();
        int minimumEligibleCount = Math.Max(5, (int)Math.Ceiling(peakPlayers * 0.6));
        long evaluationEnd = phase.End - ignoreTailWindow;
        string stopReason = "the final 10s and routed cleanup below 60% of peak active enemies were ignored";
        if (fightShape?.Available == true &&
            fightShape.CleanupStartTimeMs is long cleanupStart &&
            string.Equals(fightShape.LosingSide, "enemy", StringComparison.OrdinalIgnoreCase))
        {
            evaluationEnd = Math.Min(evaluationEnd, cleanupStart);
            stopReason = $"evaluation stopped at the detected enemy cleanup boundary ({ToDurationString(Math.Max(0, cleanupStart - phase.Start))})";
        }

        List<WvwSummaryFormationSnapshot> eligibleSnapshots = enemyFormationSnapshots
            .Where(snapshot => snapshot.Time <= evaluationEnd && snapshot.PlayerCount >= minimumEligibleCount)
            .ToList();
        if (eligibleSnapshots.Count == 0)
        {
            context.EnemyMovementScoreLabel = "Insufficient data";
            context.EnemyMovementScoreDetail = "Could not evaluate enough engaged pre-cleanup enemy replay samples for a movement tightness score.";
            return;
        }

        double score = eligibleSnapshots.Average(ComputeEnemyMovementSnapshotScore);
        double centerTightShare = eligibleSnapshots.Average(snapshot => snapshot.CentroidTightShare) * 100.0;
        double averageDistanceToCenter = eligibleSnapshots.Average(snapshot => snapshot.AverageDistanceToCentroid);

        context.EnemyMovementScore = (int)Math.Round(score);
        context.EnemyMovementScoreLabel = GetEnemyMovementScoreLabel(score);
        context.EnemyMovementCenterTightShare = Math.Round(centerTightShare, 1);
        context.EnemyMovementAverageDistanceToCenter = Math.Round(averageDistanceToCenter);
        context.EnemyMovementSampleCount = eligibleSnapshots.Count;
        context.EnemyMovementScoreDetail = $"{context.EnemyMovementScoreLabel} movement: {FormatDecimal(centerTightShare)}% of active enemy positions were within 600 units of their center, averaging {FormatWholeNumber(averageDistanceToCenter)} units from center across {eligibleSnapshots.Count.ToString("N0", CultureInfo.InvariantCulture)} engaged pre-cleanup samples; {stopReason}.";
    }

    private static double ComputeEnemyMovementSnapshotScore(WvwSummaryFormationSnapshot snapshot)
    {
        const double eliteAverageDistance = 240.0;
        const double organizedAverageDistance = 600.0;
        const double scatteredAverageDistance = 1000.0;

        double averageDistanceScore;
        if (snapshot.AverageDistanceToCentroid <= eliteAverageDistance)
        {
            averageDistanceScore = 100.0;
        }
        else if (snapshot.AverageDistanceToCentroid <= organizedAverageDistance)
        {
            averageDistanceScore = 70.0 + (organizedAverageDistance - snapshot.AverageDistanceToCentroid) * 30.0 / (organizedAverageDistance - eliteAverageDistance);
        }
        else
        {
            averageDistanceScore = Math.Max(0.0, 70.0 - (snapshot.AverageDistanceToCentroid - organizedAverageDistance) * 70.0 / (scatteredAverageDistance - organizedAverageDistance));
        }

        return snapshot.CentroidTightShare * 100.0 * 0.6 + averageDistanceScore * 0.4;
    }

    private static string GetEnemyMovementScoreLabel(double score)
    {
        if (score >= 92.0)
        {
            return "Elite tight";
        }
        if (score >= 84.0)
        {
            return "Tight";
        }
        if (score >= 72.0)
        {
            return "Organized";
        }
        if (score >= 58.0)
        {
            return "Loose";
        }
        return "Scattered";
    }

    private static WvwSummaryThreeWayContext BuildThreeWayContext(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        List<WvwSummaryThreeWaySnapshot> snapshots = BuildThreeWaySnapshots(log, phase, squadActors, hostilePlayerTargets);
        List<WvwSummaryThreeWaySnapshot> sustainedSegment = GetFirstSustainedThreeWaySegment(snapshots);
        if (sustainedSegment.Count == 0)
        {
            return new WvwSummaryThreeWayContext(false, null, 0, string.Empty, string.Empty);
        }

        WvwSummaryThreeWaySnapshot first = sustainedSegment[0];
        int secondEnemyPeakCount = sustainedSegment.Max(snapshot => snapshot.SecondEnemyCount);
        double averageEnemyCenterSeparation = sustainedSegment.Average(snapshot => snapshot.EnemyCenterDistance);
        double averagePrimaryDistanceToSquad = sustainedSegment.Average(snapshot => snapshot.PrimaryDistanceToSquad);
        double averageSecondDistanceToSquad = sustainedSegment.Average(snapshot => snapshot.SecondaryDistanceToSquad);
        string detail = $"3-way detected from {ToDurationString(Math.Max(0, first.Time - phase.Start))}: two enemy teams each had at least {ThreeWayMinimumSecondEnemyCount} active visible players and at least {ThreeWayMinimumCombinedEnemyCountWithoutColor} combined active visible players for {FormatDecimal(ThreeWayMinimumSustainMs / 1000.0)}s. Enemy team centers averaged {FormatWholeNumber(averageEnemyCenterSeparation)} units apart, with team {FormatTeamId(first.PrimaryTeamId)} averaging {FormatWholeNumber(averagePrimaryDistanceToSquad)} units from our squad and team {FormatTeamId(first.SecondaryTeamId)} averaging {FormatWholeNumber(averageSecondDistanceToSquad)} units from our squad.";
        return new WvwSummaryThreeWayContext(true, first.Time, secondEnemyPeakCount, "3-way", detail);
    }

    private static List<WvwSummaryThreeWaySnapshot> BuildThreeWaySnapshots(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        const long sampleInterval = 1000;

        var snapshots = new List<WvwSummaryThreeWaySnapshot>();
        for (long time = phase.Start; time <= phase.End; time += sampleInterval)
        {
            List<Vector3> squadPositions = BuildEligiblePositions(log, squadActors, time);
            if (squadPositions.Count == 0)
            {
                continue;
            }

            Vector3 squadCentroid = ComputeCentroid(squadPositions);
            Dictionary<ulong, List<Vector3>> enemyPositionsByTeam = BuildEnemyPositionsByTeam(log, hostilePlayerTargets, time);
            List<WvwSummaryEnemyTeamSnapshot> enemyTeams = enemyPositionsByTeam
                .Where(pair => pair.Value.Count >= ThreeWayMinimumSecondEnemyCount)
                .Select(pair =>
                {
                    Vector3 centroid = ComputeCentroid(pair.Value);
                    return new WvwSummaryEnemyTeamSnapshot(
                        pair.Key,
                        pair.Value.Count,
                        centroid,
                        GetDistance2D(centroid, squadCentroid));
                })
                .Where(team => team.DistanceToSquad <= ThreeWayMaxDistanceFromSquad)
                .OrderByDescending(team => team.PlayerCount)
                .ThenBy(team => team.DistanceToSquad)
                .Take(2)
                .ToList();

            if (enemyTeams.Count < 2)
            {
                continue;
            }

            WvwSummaryEnemyTeamSnapshot primary = enemyTeams[0];
            WvwSummaryEnemyTeamSnapshot secondary = enemyTeams[1];
            if (!HasKnownDifferentWvWColors(primary.TeamId, secondary.TeamId) &&
                primary.PlayerCount + secondary.PlayerCount < ThreeWayMinimumCombinedEnemyCountWithoutColor)
            {
                continue;
            }

            snapshots.Add(new WvwSummaryThreeWaySnapshot(
                time,
                primary.TeamId,
                secondary.TeamId,
                primary.PlayerCount,
                secondary.PlayerCount,
                primary.DistanceToSquad,
                secondary.DistanceToSquad,
                GetDistance2D(primary.Centroid, secondary.Centroid)));
        }

        return snapshots;
    }

    private static List<Vector3> BuildEligiblePositions(ParsedEvtcLog log, IReadOnlyList<SingleActor> actors, long time)
    {
        var positions = new List<Vector3>();
        foreach (SingleActor actor in actors)
        {
            if (TryGetEligiblePosition(actor, log, time, out Vector3 position))
            {
                positions.Add(position);
            }
        }
        return positions;
    }

    private static Dictionary<ulong, List<Vector3>> BuildEnemyPositionsByTeam(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        long time)
    {
        var positionsByTeam = new Dictionary<ulong, List<Vector3>>();
        foreach (SingleActor actor in hostilePlayerTargets)
        {
            if (!TryGetEligiblePosition(actor, log, time, out Vector3 position))
            {
                continue;
            }

            ulong teamId = GetActorTeamId(actor, log, time);
            if (teamId == 0)
            {
                continue;
            }

            if (!positionsByTeam.TryGetValue(teamId, out List<Vector3>? positions))
            {
                positions = [];
                positionsByTeam[teamId] = positions;
            }
            positions.Add(position);
        }
        return positionsByTeam;
    }

    private static ulong GetActorTeamId(SingleActor actor, ParsedEvtcLog log, long time)
    {
        IReadOnlyList<TeamChangeEvent> teamChanges = log.CombatData.GetTeamChangeEvents(actor.EnglobingAgentItem);
        TeamChangeEvent? teamChange = teamChanges.LastOrDefault(evt => evt.Time <= time)
            ?? teamChanges.LastOrDefault(evt => evt.Time <= actor.LastAware)
            ?? teamChanges.LastOrDefault();
        if (teamChange is null)
        {
            return 0;
        }
        if (teamChange.TeamIDInto > 0)
        {
            return teamChange.TeamIDInto;
        }
        return teamChange.TeamIDComingFrom;
    }

    private static List<WvwSummaryThreeWaySnapshot> GetFirstSustainedThreeWaySegment(IReadOnlyList<WvwSummaryThreeWaySnapshot> snapshots)
    {
        var currentSegment = new List<WvwSummaryThreeWaySnapshot>();
        WvwSummaryThreeWaySnapshot? previousSnapshot = null;
        foreach (WvwSummaryThreeWaySnapshot snapshot in snapshots.OrderBy(snapshot => snapshot.Time))
        {
            bool sameSegment = previousSnapshot.HasValue &&
                snapshot.Time - previousSnapshot.Value.Time <= 1500 &&
                UsesSameEnemyTeams(previousSnapshot.Value, snapshot);
            if (!sameSegment)
            {
                currentSegment.Clear();
            }

            currentSegment.Add(snapshot);
            previousSnapshot = snapshot;
            if (currentSegment[^1].Time - currentSegment[0].Time >= ThreeWayMinimumSustainMs)
            {
                return [.. currentSegment];
            }
        }

        return [];
    }

    private static bool UsesSameEnemyTeams(WvwSummaryThreeWaySnapshot left, WvwSummaryThreeWaySnapshot right)
    {
        return (left.PrimaryTeamId == right.PrimaryTeamId && left.SecondaryTeamId == right.SecondaryTeamId) ||
            (left.PrimaryTeamId == right.SecondaryTeamId && left.SecondaryTeamId == right.PrimaryTeamId);
    }

    private static bool HasKnownDifferentWvWColors(ulong primaryTeamId, ulong secondaryTeamId)
    {
        // The parser has numeric arcDPS team IDs, but no local Red/Green/Blue color mapping here.
        // Keep the stricter combined-count threshold until a reliable color map is available.
        return false;
    }

    private static string FormatTeamId(ulong teamId)
    {
        return teamId.ToString(CultureInfo.InvariantCulture);
    }

    private static WvwSummaryExecutionOutcomeDto BuildFightExecutionOutcome(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummarySideDto squad,
        WvwSummarySideDto enemy,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState)
    {
        return new WvwSummaryExecutionOutcomeDto
        {
            SquadDowns = enemyDownState.Downs,
            EnemyDowns = squadDownState.Downs,
            SquadKills = enemyDownState.KillConversions,
            EnemyKills = squadDownState.KillConversions,
            SquadDeaths = squad.Deaths,
            EnemyDeaths = enemy.Deaths,
            EnemyDownConversionRate = enemyDownState.KillConversionRate,
            SquadRecoveryRate = squadDownState.RezRate,
            CrowdControlDataAvailable = log.CombatData.HasCrowdControlData,
            IncomingCrowdControl = squad.ReceivedCrowdControl,
            OutgoingCrowdControl = enemy.ReceivedCrowdControl,
            WipeLabel = InferWipeLabel(log, phase, squadActors, hostilePlayerTargets),
        };
    }

    private static WvwSummaryExecutionConfidenceDto BuildFightExecutionConfidence(IReadOnlyList<WvwSummaryExecutionPillarDto> pillars)
    {
        int totalMetricCount = pillars.Sum(pillar => pillar.MetricCount);
        int availableMetricCount = pillars.Sum(pillar => pillar.AvailableMetricCount);
        string label = availableMetricCount == totalMetricCount
            ? "High"
            : availableMetricCount >= Math.Ceiling(totalMetricCount * 0.5)
                ? "Reduced"
                : "Low";
        List<string> notes =
        [
            .. pillars
                .SelectMany(pillar => pillar.Metrics)
                .Where(metric => !metric.Available && !string.IsNullOrWhiteSpace(metric.Note))
                .Select(metric => metric.Note)
                .Distinct(StringComparer.Ordinal)
        ];

        return new WvwSummaryExecutionConfidenceDto
        {
            Label = label,
            AvailableMetricCount = availableMetricCount,
            TotalMetricCount = totalMetricCount,
            Notes = notes,
        };
    }

    private static WvwSummaryExecutionNumbersAdjustmentDto BuildFightExecutionNumbersAdjustment(
        IReadOnlyList<WvwSummaryExecutionPillarDto> pillars,
        int squadPlayerCount,
        int enemyPlayerCount,
        int rawScore)
    {
        int playerGap = squadPlayerCount - enemyPlayerCount;
        int absolutePlayerGap = Math.Abs(playerGap);
        int effectivePlayerGap = Math.Max(absolutePlayerGap - ExecutionSizeGapGracePlayers, 0);
        double fullWeightAdjustment = playerGap switch
        {
            > 0 => -Math.Min(effectivePlayerGap * ExecutionSizeGapScorePerPlayer, ExecutionSizeGapScoreCap),
            < 0 => Math.Min(effectivePlayerGap * ExecutionSizeGapScorePerPlayer, ExecutionSizeGapScoreCap),
            _ => 0.0,
        };
        bool isApplied = effectivePlayerGap > 0 && Math.Abs(fullWeightAdjustment) > 0.001;

        foreach (WvwSummaryExecutionPillarDto pillar in pillars)
        {
            double adjustmentWeight = GetExecutionPillarAdjustmentWeight(pillar.Key);
            int adjustedScore = Math.Clamp((int)Math.Round(pillar.Score + fullWeightAdjustment * adjustmentWeight), 0, 100);
            pillar.AdjustedScore = adjustedScore;
            pillar.AdjustedGrade = ScoreToGrade(adjustedScore);
            pillar.AdjustmentApplied = isApplied && adjustmentWeight > 0 && adjustedScore != pillar.Score;
            pillar.AdjustmentDetail = pillar.AdjustmentApplied
                ? $"Numbers-adjusted: {adjustedScore}/100 ({FormatSignedDecimal(adjustedScore - pillar.Score, 0)}) from the active player gap."
                : "";
        }

        int adjustedOverallScore = pillars.Count > 0 ? (int)Math.Round(pillars.Average(pillar => pillar.AdjustedScore)) : rawScore;
        string summary = !isApplied
            ? $"Numbers-adjusted read matches raw because the active player gap stayed within {ExecutionSizeGapGracePlayers} players."
            : playerGap < 0
                ? $"Squad was outnumbered by {absolutePlayerGap} active players, so the numbers-adjusted read softens outcome-heavy penalties."
                : $"Squad had {absolutePlayerGap} more active players, so the numbers-adjusted read trims credit from outcome-heavy pillars.";
        string detail = !isApplied
            ? $"No size-gap compensation was applied. Adjustment starts only after the first {ExecutionSizeGapGracePlayers} active players of gap."
            : $"Ignores the first {ExecutionSizeGapGracePlayers} active players of gap, then applies {FormatDecimal(ExecutionSizeGapScorePerPlayer)} score points per remaining player, capped at {FormatDecimal(ExecutionSizeGapScoreCap)} for a full-weight pillar. Cohesion & Positioning stays raw. Pressure & Burst and Support & Mitigation use half shift. Downstate Control uses full shift.";

        return new WvwSummaryExecutionNumbersAdjustmentDto
        {
            IsApplied = isApplied,
            RawScore = rawScore,
            AdjustedScore = adjustedOverallScore,
            AdjustedGrade = ScoreToGrade(adjustedOverallScore),
            PlayerGap = playerGap,
            AbsolutePlayerGap = absolutePlayerGap,
            EffectivePlayerGap = effectivePlayerGap,
            FullWeightAdjustment = Math.Round(fullWeightAdjustment, 1),
            Summary = summary,
            Detail = detail,
        };
    }

    private static WvwSummaryExecutionPillarDto BuildExecutionPillar(
        string key,
        string label,
        IReadOnlyList<WvwSummaryExecutionMetricDto> metrics,
        string summary,
        string detail)
    {
        List<WvwSummaryExecutionMetricDto> metricList = metrics.ToList();
        ApplyDefaultExecutionMetricWeights(metricList);
        double totalWeight = metricList.Sum(metric => metric.WeightPercent);
        int score = totalWeight > 0.0
            ? (int)Math.Round(metricList.Sum(metric => metric.Score * metric.WeightPercent) / totalWeight)
            : 50;
        int availableMetricCount = metricList.Count(metric => metric.Available);
        string detailSuffix = availableMetricCount == metricList.Count
            ? ""
            : $" {metricList.Count - availableMetricCount} metric{(metricList.Count - availableMetricCount == 1 ? "" : "s")} neutralized at 50 due to missing or low-signal comparison data.";
        return new WvwSummaryExecutionPillarDto
        {
            Key = key,
            Label = label,
            Score = score,
            Grade = ScoreToGrade(score),
            AdjustedScore = score,
            AdjustedGrade = ScoreToGrade(score),
            Summary = summary,
            Detail = detail + detailSuffix,
            AvailableMetricCount = availableMetricCount,
            MetricCount = metricList.Count,
            Metrics = metricList,
        };
    }

    private static void ApplyDefaultExecutionMetricWeights(IReadOnlyList<WvwSummaryExecutionMetricDto> metrics)
    {
        if (metrics.Count == 0 || metrics.Any(metric => metric.WeightPercent > 0.0))
        {
            return;
        }

        double equalWeight = Math.Round(100.0 / metrics.Count, 1);
        foreach (WvwSummaryExecutionMetricDto metric in metrics)
        {
            metric.WeightPercent = equalWeight;
        }
    }

    private static double GetExecutionPillarAdjustmentWeight(string key)
    {
        return key switch
        {
            "cohesion-positioning" => 0.0,
            "pressure-burst" => 0.5,
            "downstate-control" => 1.0,
            "support-mitigation" => 0.5,
            _ => 0.0,
        };
    }

    private static WvwSummaryExecutionMetricDto BuildRelativeExecutionMetric(
        string label,
        double squadValue,
        double enemyValue,
        bool higherIsBetter,
        string value,
        string note = "",
        double weightPercent = 0.0)
    {
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = value,
            Note = note,
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(CompareScore(squadValue, enemyValue, higherIsBetter)),
        };
    }

    private static WvwSummaryExecutionMetricDto BuildFixedScoreExecutionMetric(
        string label,
        string value,
        string note,
        double score,
        double weightPercent = 0.0)
    {
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = value,
            Note = note,
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(Math.Clamp(score, 0.0, 100.0)),
        };
    }

    private static WvwSummaryExecutionMetricDto BuildPositioningExecutionMetric(
        string label,
        double rate,
        long evaluatedSamples,
        bool higherIsBetter,
        double weightPercent = 0.0)
    {
        double favorableShare = higherIsBetter ? rate : 100.0 - rate;
        double unfavorableShare = higherIsBetter ? 100.0 - rate : rate;
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = $"{FormatDecimal(rate)}% over {evaluatedSamples.ToString("N0", CultureInfo.InvariantCulture)} eligible replay samples",
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(CompareScore(favorableShare, unfavorableShare)),
        };
    }

    private static WvwSummaryExecutionMetricDto BuildInPositionExecutionMetric(double rate, long evaluatedSamples, double weightPercent = 0.0)
    {
        double score = Math.Clamp((rate - 10.0) * (100.0 / 60.0), 0.0, 100.0);
        return new WvwSummaryExecutionMetricDto
        {
            Label = "In-position rate",
            Value = $"{FormatDecimal(rate)}% over {evaluatedSamples.ToString("N0", CultureInfo.InvariantCulture)} eligible replay samples",
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(score),
        };
    }

    private static WvwSummaryExecutionMetricDto BuildPressurePositioningExecutionMetric(
        double rate,
        long evaluatedSamples,
        int pressureWindowCount,
        double weightPercent)
    {
        double score = Math.Clamp((rate - 10.0) * (100.0 / 60.0), 0.0, 100.0);
        return new WvwSummaryExecutionMetricDto
        {
            Label = "Pressure positioning",
            Value = $"{FormatDecimal(rate)}% in position over {evaluatedSamples.ToString("N0", CultureInfo.InvariantCulture)} eligible pressure samples",
            Note = $"Measured only inside {BuildPluralizedTag(pressureWindowCount, "enemy pressure window", "enemy pressure windows")}.",
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(score),
        };
    }

    private static WvwSummaryExecutionMetricDto BuildNeutralizedExecutionMetric(string label, string note, double weightPercent = 0.0)
    {
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = "n/a",
            Note = note,
            Available = false,
            WeightPercent = weightPercent,
            Score = 50,
        };
    }

    private static WvwSummaryExecutionMetricDto BuildBoonCoverageExecutionMetric(
        string label,
        double coverage,
        string value,
        string note,
        double weightPercent = 0.0)
    {
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = value,
            Note = note,
            Available = true,
            WeightPercent = weightPercent,
            Score = (int)Math.Round(Math.Clamp(coverage, 0.0, 100.0)),
        };
    }

    private static string BuildPressurePositioningSummary(WvwSummaryPhasePositioningDto pressurePositioningMetrics, int pressureWindowCount)
    {
        if (!pressurePositioningMetrics.HasData)
        {
            return pressureWindowCount == 0
                ? "Pressure positioning was neutralized because no enemy pressure windows were detected."
                : "Pressure positioning was neutralized because no eligible positioning samples overlapped enemy pressure windows.";
        }

        return $"{FormatDecimal(pressurePositioningMetrics.InPositionRate)}% in position during {BuildPluralizedTag(pressureWindowCount, "enemy pressure window", "enemy pressure windows")} over {pressurePositioningMetrics.EvaluatedSamples.ToString("N0", CultureInfo.InvariantCulture)} pressure samples.";
    }

    private static string BuildDownstateContextSummary(WvwSummaryDownStateSideDto squadDownState, WvwSummaryDownStateSideDto enemyDownState)
    {
        bool hasOffensiveDownOpportunity = enemyDownState.Downs >= ExecutionDownstateMinimumOpportunityDowns;
        bool hasDefensiveRecoveryOpportunity = squadDownState.Downs >= ExecutionDownstateMinimumOpportunityDowns;
        string enemyDownLabel = BuildPluralizedTag(enemyDownState.Downs, "enemy down", "enemy downs");
        string squadDownLabel = BuildPluralizedTag(squadDownState.Downs, "squad down", "squad downs");
        if (!hasOffensiveDownOpportunity && !hasDefensiveRecoveryOpportunity)
        {
            return $"Low downstate sample: the squad created {enemyDownLabel} and took {squadDownLabel}.";
        }
        if (!hasOffensiveDownOpportunity)
        {
            return $"Heavy defensive load: the squad took {squadDownLabel} but created only {enemyDownLabel}.";
        }
        if (!hasDefensiveRecoveryOpportunity)
        {
            return $"Light defensive load: the squad created {enemyDownLabel} but took only {squadDownLabel}.";
        }

        int lowerDownCount = Math.Max(Math.Min(squadDownState.Downs, enemyDownState.Downs), 1);
        double downLoadRatio = Math.Max(squadDownState.Downs, enemyDownState.Downs) / (double)lowerDownCount;
        return downLoadRatio >= 2.5
            ? $"Lopsided downstate load: the squad created {enemyDownLabel} and took {squadDownLabel}."
            : $"Contestable downstate: the squad created {enemyDownLabel} and took {squadDownLabel}.";
    }

    private static WvwSummaryExecutionMetricDto BuildCleansePressureExecutionMetric(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        double squadPlayers,
        out string summary)
    {
        const string label = "Cleanse pressure response";

        double durationInMinutes = Math.Max(phase.DurationInMS / 60000.0, 1.0 / 60.0);
        double enduredConditionSeconds = ComputePhaseTrackedConditionPresenceSeconds(log, phase, squadActors);
        double removedConditionSeconds = ComputePhaseRemovedConditionSeconds(log, phase, squadActors);
        double potentialConditionSeconds = enduredConditionSeconds + removedConditionSeconds;
        double pressurePerActivePlayerPerMinute = potentialConditionSeconds / Math.Max(squadPlayers, 1.0) / durationInMinutes;

        if (pressurePerActivePlayerPerMinute < ExecutionMinimumTrackedCleansePressurePerActivePlayerPerMinute)
        {
            summary = $"{FormatDecimal(pressurePerActivePlayerPerMinute)} tracked condition-seconds per active player per minute; cleanse execution was not meaningfully tested.";
            return BuildNeutralizedExecutionMetric(
                label,
                $"Neutralized at 50: tracked condition pressure was only {FormatDecimal(pressurePerActivePlayerPerMinute)} condition-seconds per active player per minute, below the {FormatDecimal(ExecutionMinimumTrackedCleansePressurePerActivePlayerPerMinute)} threshold for a meaningful cleanse test.");
        }

        double preventedShare = potentialConditionSeconds > 0.0 ? Math.Round(removedConditionSeconds * 100.0 / potentialConditionSeconds, 1) : 0.0;
        double enduredShare = Math.Round(Math.Max(0.0, 100.0 - preventedShare), 1);
        summary = $"{FormatDecimal(pressurePerActivePlayerPerMinute)} tracked condition-seconds per active player per minute, with cleanses preventing {FormatDecimal(preventedShare)}%.";
        return BuildRelativeExecutionMetric(
            label,
            removedConditionSeconds,
            enduredConditionSeconds,
            higherIsBetter: true,
            $"{FormatDecimal(preventedShare)}% prevented vs {FormatDecimal(enduredShare)}% endured under {FormatDecimal(pressurePerActivePlayerPerMinute)} tracked condition-seconds per active player per minute",
            "Self-scored from tracked condition burden. Each tracked condition, including Vulnerability, counts as present or absent rather than by stack count, and allied cleanse removes remaining condition duration, so earlier cleanses score better because they prevent more seconds from being endured.");
    }

    private static WvwSummaryExecutionMetricDto BuildPreventionExecutionMetric(
        long barrierAbsorbed,
        double negatedDamage,
        long petMinionAbsorption,
        long squadHealthDamageTaken,
        CombatReplayDefenseSavedPlayersSummaryDto savedPlayersSummary,
        out string summary)
    {
        const string label = "Prevention value";

        double phaseBarrierAbsorbed = Math.Max(0.0, barrierAbsorbed);
        double phaseNegatedDamage = Math.Max(0.0, negatedDamage);
        double phasePetMinionAbsorption = Math.Max(0.0, petMinionAbsorption);
        double phaseHealthDamageTaken = Math.Max(0.0, squadHealthDamageTaken);
        double totalPrevention = phaseBarrierAbsorbed + phaseNegatedDamage + phasePetMinionAbsorption;

        summary = $"{FormatWholeNumber((long)Math.Round(phaseBarrierAbsorbed))} barrier absorbed, {FormatWholeNumber((long)Math.Round(phaseNegatedDamage))} estimated negated damage, and {FormatWholeNumber((long)Math.Round(phasePetMinionAbsorption))} pet/minion absorption reduced incoming pressure before recovery work.";
        if (totalPrevention <= 0.0 && phaseHealthDamageTaken <= 0.0)
        {
            summary = "Prevention value was neutralized because this phase had no tracked barrier, negation, pet/minion absorption, or squad health damage to compare.";
            return BuildNeutralizedExecutionMetric(label, summary);
        }

        double totalPressure = totalPrevention + phaseHealthDamageTaken;
        double preventionShare = totalPressure > 0.0 ? totalPrevention * 100.0 / totalPressure : 0.0;
        int score = ComputePreventionExecutionScore(preventionShare);
        string note = $"{FormatWholeNumber((long)Math.Round(phaseBarrierAbsorbed))} barrier absorbed + {FormatWholeNumber((long)Math.Round(phaseNegatedDamage))} estimated negated damage + {FormatWholeNumber((long)Math.Round(phasePetMinionAbsorption))} pet/minion absorption. Scores against realistic prevention-share bands rather than expecting prevention to match damage taken. {savedPlayersSummary.BarrierSavedCases} barrier saves and {savedPlayersSummary.NegatedDamageSavedCases} negated-damage saves were detected.";
        return new WvwSummaryExecutionMetricDto
        {
            Label = label,
            Value = $"{FormatDecimal(preventionShare)}% of total incoming pressure prevented ({FormatWholeNumber((long)Math.Round(totalPrevention))} prevention vs {FormatWholeNumber((long)Math.Round(phaseHealthDamageTaken))} squad health damage)",
            Note = note,
            Available = true,
            Score = score,
        };
    }

    private static (long HealthDamage, long BarrierAbsorbed) ComputePhaseSquadIncomingDamage(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadActors,
        PhaseData phase)
    {
        long healthDamage = 0;
        long barrierAbsorbed = 0;
        foreach (SingleActor actor in squadActors)
        {
            foreach (HealthDamageEvent damageEvent in actor.GetDamageTakenEvents(null, log, phase.Start, phase.End))
            {
                if (!damageEvent.HasHit || damageEvent.HealthDamage <= 0)
                {
                    continue;
                }

                healthDamage += Math.Max(damageEvent.HealthDamage - damageEvent.ShieldDamage, 0);
                barrierAbsorbed += damageEvent.ShieldDamage;
            }
        }
        return (healthDamage, barrierAbsorbed);
    }

    private static double ComputePhaseEstimatedNegatedDamage(
        CombatReplayDefenseAnalysisDto defense,
        PhaseData phase)
    {
        return Math.Round(defense.NegatedHitSummaries
            .SelectMany(summary => summary.Occurrences)
            .Where(occurrence => occurrence.Time >= phase.Start && occurrence.Time <= phase.End)
            .Sum(occurrence => occurrence.EstimatedPreventedDamage), 1);
    }

    private static long ComputePhasePetMinionAbsorption(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadActors,
        PhaseData phase)
    {
        long totalAbsorbed = 0;
        foreach (SingleActor actor in squadActors)
        {
            foreach (Minions minions in actor.GetMinions(log))
            {
                foreach (HealthDamageEvent damageEvent in minions.GetDamageTakenEvents(null, log, phase.Start, phase.End))
                {
                    if (damageEvent.HasHit && damageEvent.HealthDamage > 0)
                    {
                        totalAbsorbed += damageEvent.HealthDamage;
                    }
                }
            }
        }
        return totalAbsorbed;
    }

    private static CombatReplayDefenseSavedPlayersSummaryDto BuildPhaseSavedPlayersSummary(
        CombatReplayDefenseAnalysisDto defense,
        PhaseData phase)
    {
        CombatReplayDefenseMitigationThresholdDto? threshold99 = defense.Mitigation.Thresholds
            .FirstOrDefault(threshold => threshold.ThresholdPercent == 99);
        if (threshold99 == null || threshold99.Events.Count == 0)
        {
            return new CombatReplayDefenseSavedPlayersSummaryDto();
        }

        List<CombatReplayDefenseMitigationEventDto> savedEvents = [.. threshold99.Events.Where(evt =>
            evt.RecoveryTime >= phase.Start &&
            evt.Time <= phase.End &&
            (evt.BarrierSavedPlayer || evt.EstimatedMitigationSavedPlayer || evt.EstimatedNegatedDamageSavedPlayer))];
        if (savedEvents.Count == 0)
        {
            return new CombatReplayDefenseSavedPlayersSummaryDto();
        }

        int barrierSavedCases = savedEvents.Count(evt => evt.BarrierSavedPlayer);
        int damageReductionSavedCases = savedEvents.Count(evt => evt.EstimatedMitigationSavedPlayer);
        int negatedDamageSavedCases = savedEvents.Count(evt => evt.EstimatedNegatedDamageSavedPlayer);
        int bothSavedCases = savedEvents.Count(evt => evt.BarrierSavedPlayer && evt.EstimatedMitigationSavedPlayer);
        int multiSourceSavedCases = savedEvents.Count(evt =>
            (evt.BarrierSavedPlayer ? 1 : 0) +
            (evt.EstimatedMitigationSavedPlayer ? 1 : 0) +
            (evt.EstimatedNegatedDamageSavedPlayer ? 1 : 0) > 1);

        return new CombatReplayDefenseSavedPlayersSummaryDto
        {
            SavedCases = savedEvents.Count,
            TotalBarrierAbsorbed = Math.Round(savedEvents.Sum(evt => evt.BarrierAbsorbedToLowest), 1),
            BarrierSavedCases = barrierSavedCases,
            TotalEstimatedDamageReduction = Math.Round(savedEvents.Sum(evt => evt.EstimatedMitigationToLowest), 0),
            DamageReductionSavedCases = damageReductionSavedCases,
            TotalEstimatedNegatedDamage = Math.Round(savedEvents.Sum(evt => evt.EstimatedNegatedDamageToLowest), 0),
            NegatedDamageSavedCases = negatedDamageSavedCases,
            AverageLowestHealthPercent = Math.Round(savedEvents.Average(evt => evt.LowestHealthPercent), 1),
            LowestLowestHealthPercent = Math.Round(savedEvents.Min(evt => evt.LowestHealthPercent), 1),
            BothSavedCases = bothSavedCases,
            MultiSourceSavedCases = multiSourceSavedCases,
            TotalIncomingDamage = Math.Round(savedEvents.Sum(evt => evt.IncomingDamage), 1),
            TotalIncomingHealing = Math.Round(savedEvents.Sum(evt => evt.IncomingHealing), 1),
        };
    }

    private static int ComputePreventionExecutionScore(double preventionSharePercent)
    {
        double[] shareAnchors = [0.0, 10.0, 20.0, 30.0, 40.0, 45.0, 55.0];
        double[] scoreAnchors = [15.0, 35.0, 55.0, 72.0, 88.0, 95.0, 100.0];

        if (preventionSharePercent <= shareAnchors[0])
        {
            return (int)Math.Round(scoreAnchors[0]);
        }

        for (int i = 1; i < shareAnchors.Length; i++)
        {
            if (preventionSharePercent <= shareAnchors[i])
            {
                double lowerShare = shareAnchors[i - 1];
                double upperShare = shareAnchors[i];
                double lowerScore = scoreAnchors[i - 1];
                double upperScore = scoreAnchors[i];
                double progress = (preventionSharePercent - lowerShare) / Math.Max(upperShare - lowerShare, 0.0001);
                return (int)Math.Round(lowerScore + (upperScore - lowerScore) * progress);
            }
        }

        return (int)Math.Round(scoreAnchors[^1]);
    }

    private static double ComputePhaseTrackedConditionPresenceSeconds(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> actors)
    {
        double totalSeconds = 0.0;
        foreach (SingleActor actor in actors)
        {
            foreach (Buff condition in log.Buffs.BuffsByClassification[Buff.BuffClassification.Condition])
            {
                foreach (Segment segment in actor.GetBuffPresenceStatus(log, condition.ID, phase.Start, phase.End))
                {
                    if (segment.Value <= 0.0)
                    {
                        continue;
                    }

                    long segmentStart = Math.Max(segment.Start, phase.Start);
                    long segmentEnd = Math.Min(segment.End, phase.End);
                    if (segmentEnd <= segmentStart)
                    {
                        continue;
                    }

                    totalSeconds += (segmentEnd - segmentStart) / 1000.0;
                }
            }
        }

        return Math.Round(totalSeconds, 1);
    }

    private static double ComputePhaseRemovedConditionSeconds(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> actors)
    {
        var trackedConditionIds = new HashSet<long>(
            log.Buffs.BuffsByClassification[Buff.BuffClassification.Condition]
                .Select(condition => condition.ID));
        var squadAgentItems = new HashSet<AgentItem>(actors.Select(actor => actor.AgentItem.GetFinalMaster()));
        double totalSeconds = 0.0;

        foreach (SingleActor actor in actors)
        {
            foreach (BuffRemoveAllEvent removeEvent in log.CombatData.GetBuffRemoveAllDataByDst(actor.EnglobingAgentItem))
            {
                if (removeEvent.Time < phase.Start || removeEvent.Time > phase.End)
                {
                    continue;
                }
                if (removeEvent.Time < actor.FirstAware || removeEvent.Time > actor.LastAware)
                {
                    continue;
                }
                if (!removeEvent.ToFriendly || removeEvent.CreditedBy.IsUnknown || !trackedConditionIds.Contains(removeEvent.BuffID))
                {
                    continue;
                }
                if (!squadAgentItems.Contains(removeEvent.CreditedBy.GetFinalMaster()))
                {
                    continue;
                }

                totalSeconds += Math.Max(removeEvent.RemovedDuration, 0) / 1000.0;
            }
        }

        return Math.Round(totalSeconds, 1);
    }

    private static WvwSummaryPhasePositioningDto BuildPhasePositioningMetrics(
        CombatReplayAnalysisDto combatReplayAnalysis,
        PhaseData phase,
        IReadOnlyList<WvwSummaryExecutionWindow>? windows = null)
    {
        var result = new WvwSummaryPhasePositioningDto
        {
            CommanderAvailable = combatReplayAnalysis.Positioning.HasCommander,
        };
        if (!combatReplayAnalysis.Positioning.HasCommander || combatReplayAnalysis.Times.Length == 0 || (windows is not null && windows.Count == 0))
        {
            return result;
        }

        long totalEvaluatedSamples = 0;
        long totalInPositionSamples = 0;
        long totalTooFarSamples = 0;
        long totalOverextendedSamples = 0;
        long totalLateralRiskSamples = 0;

        foreach (CombatReplayPositioningPlayerTimelineDto playerTimeline in combatReplayAnalysis.Positioning.Players.Values)
        {
            int limit = Math.Min(combatReplayAnalysis.Times.Length, playerTimeline.Eligible.Length);
            for (int index = 0; index < limit; index++)
            {
                long time = combatReplayAnalysis.Times[index];
                if (time < phase.Start ||
                    time > phase.End ||
                    !IsInsideExecutionWindow(time, windows) ||
                    !playerTimeline.Eligible[index])
                {
                    continue;
                }

                totalEvaluatedSamples++;
                if (index < playerTimeline.InPosition.Length && playerTimeline.InPosition[index])
                {
                    totalInPositionSamples++;
                }
                if (index < playerTimeline.TooFar.Length && playerTimeline.TooFar[index])
                {
                    totalTooFarSamples++;
                }
                if (index < playerTimeline.Overextended.Length && playerTimeline.Overextended[index])
                {
                    totalOverextendedSamples++;
                }
                if (index < playerTimeline.LateralRisk.Length && playerTimeline.LateralRisk[index])
                {
                    totalLateralRiskSamples++;
                }
            }
        }

        if (totalEvaluatedSamples == 0)
        {
            return result;
        }

        result.EvaluatedSamples = totalEvaluatedSamples;
        result.InPositionRate = Math.Round(totalInPositionSamples * 100.0 / totalEvaluatedSamples, 1);
        result.TooFarRate = Math.Round(totalTooFarSamples * 100.0 / totalEvaluatedSamples, 1);
        result.OverextendedRate = Math.Round(totalOverextendedSamples * 100.0 / totalEvaluatedSamples, 1);
        result.LateralRiskRate = Math.Round(totalLateralRiskSamples * 100.0 / totalEvaluatedSamples, 1);
        return result;
    }

    private static bool IsInsideExecutionWindow(long time, IReadOnlyList<WvwSummaryExecutionWindow>? windows)
    {
        return windows is null || windows.Any(window => time >= window.Start && time <= window.End);
    }

    private static List<long> GetPhaseDownTimes(ParsedEvtcLog log, IReadOnlyList<SingleActor> actors, PhaseData phase)
    {
        return [.. actors
            .SelectMany(actor => log.CombatData.GetDownEvents(actor.AgentItem).Select(evt => evt.Time))
            .Where(time => time >= phase.Start && time <= phase.End)
            .OrderBy(time => time)];
    }

    private static List<WvwSummaryExecutionWindow> BuildPhaseBurstWindows(
        CombatReplayTeamAnalysisDto teamAnalysis,
        IReadOnlyList<long> times,
        long phaseStart,
        long phaseEnd,
        int lookback)
    {
        var result = new List<WvwSummaryExecutionWindow>();
        if (times.Count == 0)
        {
            return result;
        }

        const int burstWindowTail = 1000;
        int limit = Math.Min(times.Count, Math.Min(teamAnalysis.BurstStrength.Length, teamAnalysis.StripSynced.Length));
        int windowStart = -1;
        for (int index = 0; index < limit; index++)
        {
            long time = times[index];
            if (time < phaseStart || time > phaseEnd)
            {
                if (windowStart >= 0)
                {
                    result.Add(CreatePhaseBurstWindow(windowStart, index - 1, times, phaseStart, phaseEnd, lookback, burstWindowTail));
                    windowStart = -1;
                }
                continue;
            }

            bool qualified = teamAnalysis.BurstStrength[index] == "strong" && teamAnalysis.StripSynced[index];
            if (qualified && windowStart < 0)
            {
                windowStart = index;
            }
            else if (!qualified && windowStart >= 0)
            {
                result.Add(CreatePhaseBurstWindow(windowStart, index - 1, times, phaseStart, phaseEnd, lookback, burstWindowTail));
                windowStart = -1;
            }
        }

        if (windowStart >= 0)
        {
            result.Add(CreatePhaseBurstWindow(windowStart, limit - 1, times, phaseStart, phaseEnd, lookback, burstWindowTail));
        }
        return MergeExecutionWindows(result, burstWindowTail);
    }

    private static WvwSummaryExecutionWindow CreatePhaseBurstWindow(
        int startIndex,
        int endIndex,
        IReadOnlyList<long> times,
        long phaseStart,
        long phaseEnd,
        int lookback,
        int burstWindowTail)
    {
        long startTime = Math.Max(phaseStart, Math.Max(0, times[startIndex] - lookback));
        long endTime = Math.Min(phaseEnd, times[endIndex] + burstWindowTail);
        return new WvwSummaryExecutionWindow(startTime, endTime);
    }

    private static int CountWindowsContainingEvents(IReadOnlyList<WvwSummaryExecutionWindow> windows, IReadOnlyList<long> eventTimes)
    {
        if (windows.Count == 0 || eventTimes.Count == 0)
        {
            return 0;
        }

        int eventIndex = 0;
        int count = 0;
        foreach (WvwSummaryExecutionWindow window in windows.OrderBy(window => window.Start))
        {
            while (eventIndex < eventTimes.Count && eventTimes[eventIndex] < window.Start)
            {
                eventIndex++;
            }
            if (eventIndex < eventTimes.Count && eventTimes[eventIndex] <= window.End)
            {
                count++;
            }
        }
        return count;
    }

    private static int CountEventsInsideWindows(IReadOnlyList<WvwSummaryExecutionWindow> windows, IReadOnlyList<long> eventTimes)
    {
        if (windows.Count == 0 || eventTimes.Count == 0)
        {
            return 0;
        }

        int eventIndex = 0;
        int count = 0;
        foreach (WvwSummaryExecutionWindow window in windows.OrderBy(window => window.Start))
        {
            while (eventIndex < eventTimes.Count && eventTimes[eventIndex] < window.Start)
            {
                eventIndex++;
            }
            while (eventIndex < eventTimes.Count && eventTimes[eventIndex] <= window.End)
            {
                count++;
                eventIndex++;
            }
        }
        return count;
    }

    private static List<WvwSummaryExecutionWindow> MergeExecutionWindows(List<WvwSummaryExecutionWindow> windows, int burstWindowTail)
    {
        if (windows.Count == 0)
        {
            return windows;
        }

        List<WvwSummaryExecutionWindow> mergedWindows = [windows[0]];
        foreach (WvwSummaryExecutionWindow window in windows.OrderBy(window => window.Start).Skip(1))
        {
            WvwSummaryExecutionWindow previous = mergedWindows[^1];
            if (window.Start <= previous.End + burstWindowTail)
            {
                mergedWindows[^1] = new WvwSummaryExecutionWindow(previous.Start, Math.Max(previous.End, window.End));
            }
            else
            {
                mergedWindows.Add(window);
            }
        }

        return mergedWindows;
    }

    private static string InferWipeLabel(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        WvwSummarySideStateDto squadState = BuildSideState(log, squadActors, phase.End);
        WvwSummarySideStateDto enemyState = BuildSideState(log, hostilePlayerTargets, phase.End);
        bool squadWiped = squadState.Alive == 0 && squadState.Down == 0 && squadState.Dead > 0;
        bool enemyWiped = enemyState.Alive == 0 && enemyState.Down == 0 && enemyState.Dead > 0;

        if (squadWiped && enemyWiped)
        {
            return "Trade wipe inferred";
        }
        if (enemyWiped)
        {
            return "Enemy wipe inferred";
        }
        if (squadWiped)
        {
            return "Squad wipe inferred";
        }
        return "No wipe inferred";
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

    private static WvwSummaryPlayerStandoutsDto BuildPlayerStandouts(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<WvwSummaryMomentDto> moments,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        List<WvwSummaryStandoutCategoryDto> categories =
        [
            new WvwSummaryStandoutCategoryDto
            {
                Key = "fight",
                Label = "Fight Impact",
                Detail = "Most influential timed contributions across the fight's key moments.",
            },
            new WvwSummaryStandoutCategoryDto
            {
                Key = "offense",
                Label = "Offensive Impact",
                Detail = "Burst pressure and focus fire in successful swing windows.",
            },
            new WvwSummaryStandoutCategoryDto
            {
                Key = "control",
                Label = "Control Impact",
                Detail = "Timed strips and effective crowd control around conversions and swings.",
            },
            new WvwSummaryStandoutCategoryDto
            {
                Key = "support",
                Label = "Support Impact",
                Detail = "Stabilization under pressure, not raw output outside key windows.",
            },
            new WvwSummaryStandoutCategoryDto
            {
                Key = "hybrid",
                Label = "Hybrid Impact",
                Detail = "Combined multi-role value across the fight's highest-leverage windows.",
            },
        ];

        if (squadActors.Count == 0)
        {
            return new WvwSummaryPlayerStandoutsDto
            {
                Summary = "No squad players were active in this phase.",
                Categories = categories,
            };
        }

        List<WvwSummaryStandoutWindow> windows = BuildStandoutWindows(phase, moments);
        if (windows.Count == 0)
        {
            return new WvwSummaryPlayerStandoutsDto
            {
                Summary = "No fight-turning windows were available to score timed player impact in this phase.",
                Categories = categories,
            };
        }

        List<WvwSummaryPlayerStandoutAggregate> aggregates = BuildPlayerStandoutAggregates(log, combatReplayAnalysis, squadActors, hostilePlayerTargets, windows);
        if (aggregates.Count == 0)
        {
            return new WvwSummaryPlayerStandoutsDto
            {
                Summary = "No squad player contributions were available for standout scoring in this phase.",
                Categories = categories,
            };
        }

        WvwSummaryPlayerStandoutMetricMaximums maximums = BuildPlayerStandoutMetricMaximums(aggregates);
        List<WvwSummaryPlayerStandoutScoreDto> playerScores = aggregates
            .Select(aggregate => BuildPlayerStandoutScore(aggregate, maximums))
            .ToList();

        foreach (WvwSummaryStandoutCategoryDto category in categories)
        {
            category.Entries = BuildStandoutEntriesForCategory(category.Key, playerScores);
        }

        int populatedCategoryCount = categories.Count(category => category.Entries.Count > 0);
        string summary = populatedCategoryCount > 0
            ? $"Timed standout recognition is based on {windows.Count} key fight windows built from Moments, with players allowed to appear in multiple categories."
            : "No standout players were identified from the available key windows in this phase.";

        return new WvwSummaryPlayerStandoutsDto
        {
            Summary = summary,
            Categories = categories,
        };
    }

    private static List<WvwSummaryPlayerStandoutAggregate> BuildPlayerStandoutAggregates(
        ParsedEvtcLog log,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        IReadOnlyList<WvwSummaryStandoutWindow> windows)
    {
        var result = new List<WvwSummaryPlayerStandoutAggregate>(squadActors.Count);
        foreach (SingleActor actor in squadActors)
        {
            int playerIndex = GetFriendlyPlayerIndex(log, actor);
            if (playerIndex < 0)
            {
                continue;
            }

            var aggregate = new WvwSummaryPlayerStandoutAggregate
            {
                PlayerIndex = playerIndex,
                Name = actor.Character,
                Account = actor.Account,
                Profession = actor.Spec.ToString(),
                Icon = actor.GetIcon(),
            };

            foreach (WvwSummaryStandoutWindow window in windows)
            {
                WvwSummaryPlayerWindowContribution contribution = BuildPlayerWindowContribution(log, combatReplayAnalysis, actor, squadActors, hostilePlayerTargets, window);
                AccumulatePlayerWindowContribution(aggregate, contribution, window);
            }

            result.Add(aggregate);
        }

        return result;
    }

    private static WvwSummaryPlayerWindowContribution BuildPlayerWindowContribution(
        ParsedEvtcLog log,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        SingleActor actor,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummaryStandoutWindow window)
    {
        double windowDurationSeconds = Math.Max(1.0, (window.EndTime - window.StartTime) / 1000.0);
        long damageToPlayers = 0;
        int effectiveCrowdControlCount = 0;
        double effectiveCrowdControlDuration = 0.0;

        foreach (SingleActor target in hostilePlayerTargets)
        {
            DamageStatistics damage = actor.GetDamageStats(target, log, window.StartTime, window.EndTime);
            damageToPlayers += damage.Damage;

            foreach (CrowdControlEvent crowdControlEvent in actor.GetJustOutgoingActorCrowdControlEvents(target, log, window.StartTime, window.EndTime))
            {
                if (IsCrowdControlEffective(log, target, crowdControlEvent))
                {
                    effectiveCrowdControlCount++;
                    effectiveCrowdControlDuration += crowdControlEvent.Duration / 1000.0;
                }
            }
        }

        SupportStatistics support = actor.GetToAllySupportStats(log, window.StartTime, window.EndTime);
        double healing = log.CombatData.HasEXTHealing
            ? actor.EXTHealing.GetOutgoingHealStats(null, log, window.StartTime, window.EndTime).Healing
            : 0.0;
        double barrier = log.CombatData.HasEXTBarrier
            ? actor.EXTBarrier.GetOutgoingBarrierStats(null, log, window.StartTime, window.EndTime).Barrier
            : 0.0;
        double offensiveConditionPressure = ComputeConditionSupportForWindow(log, actor, hostilePlayerTargets, window.StartTime, window.EndTime, OffensiveConditionBuffIds);
        double controlConditionPressure = ComputeConditionSupportForWindow(log, actor, hostilePlayerTargets, window.StartTime, window.EndTime, ControlConditionBuffIds);
        double defensiveConditionPressure = ComputeConditionSupportForWindow(log, actor, hostilePlayerTargets, window.StartTime, window.EndTime, DefensiveConditionBuffIds);
        double offensiveBoonSupport = ComputeBoonSupportForWindow(log, actor, squadActors, window.StartTime, window.EndTime, OffensiveSupportBoonIds);
        double defensiveBoonSupport = ComputeBoonSupportForWindow(log, actor, squadActors, window.StartTime, window.EndTime, DefensiveSupportBoonIds);

        return new WvwSummaryPlayerWindowContribution
        {
            DamagePerSecond = damageToPlayers / windowDurationSeconds,
            OffensiveConditionPressurePerSecond = offensiveConditionPressure / windowDurationSeconds,
            StripPerSecond = support.BoonStripCount / windowDurationSeconds,
            ControlConditionPressurePerSecond = controlConditionPressure / windowDurationSeconds,
            HealingPerSecond = healing / windowDurationSeconds,
            BarrierPerSecond = barrier / windowDurationSeconds,
            CleansePerSecond = support.ConditionCleanseCount / windowDurationSeconds,
            OffensiveBoonSupportPerSecond = offensiveBoonSupport / windowDurationSeconds,
            DefensiveBoonSupportPerSecond = defensiveBoonSupport / windowDurationSeconds,
            DefensiveConditionPressurePerSecond = defensiveConditionPressure / windowDurationSeconds,
            ResurrectCount = support.ResurrectCount,
            EffectiveCrowdControlPerSecond = effectiveCrowdControlCount / windowDurationSeconds,
            EffectiveCrowdControlDurationPerSecond = effectiveCrowdControlDuration / windowDurationSeconds,
            TopTargetContribution = GetAverageWindowSeriesValue(combatReplayAnalysis?.Squad?.Attackers, combatReplayAnalysis?.Times, actor.UniqueID, timeline => timeline.TopTargetContribution, window.StartTime, window.EndTime),
            InPositionRate = GetAveragePlayerInPositionRate(combatReplayAnalysis?.Positioning, combatReplayAnalysis?.Times, actor.UniqueID, window.StartTime, window.EndTime),
            HasPositioningData = HasEligiblePositioningSamples(combatReplayAnalysis?.Positioning, combatReplayAnalysis?.Times, actor.UniqueID, window.StartTime, window.EndTime),
        };
    }

    private static void AccumulatePlayerWindowContribution(
        WvwSummaryPlayerStandoutAggregate aggregate,
        WvwSummaryPlayerWindowContribution contribution,
        WvwSummaryStandoutWindow window)
    {
        double offensiveWeight = window.Weight * window.OffenseWeight;
        double controlWeight = window.Weight * window.ControlWeight;
        double supportWeight = window.Weight * window.SupportWeight;
        double fightWeight = window.Weight * window.FightWeight;
        double disciplineWeight = window.Weight * window.DisciplineWeight;

        bool offensiveActive = offensiveWeight > 0.0 &&
            (contribution.DamagePerSecond > 0.0 || contribution.OffensiveConditionPressurePerSecond > 0.0 || contribution.TopTargetContribution > 0.0 || contribution.StripPerSecond > 0.0);
        bool controlActive = controlWeight > 0.0 &&
            (contribution.StripPerSecond > 0.0 || contribution.ControlConditionPressurePerSecond > 0.0 || contribution.EffectiveCrowdControlPerSecond > 0.0 || contribution.EffectiveCrowdControlDurationPerSecond > 0.0);
        bool supportActive = supportWeight > 0.0 &&
            (contribution.HealingPerSecond > 0.0 || contribution.BarrierPerSecond > 0.0 || contribution.CleansePerSecond > 0.0 || contribution.OffensiveBoonSupportPerSecond > 0.0 || contribution.DefensiveBoonSupportPerSecond > 0.0 || contribution.DefensiveConditionPressurePerSecond > 0.0 || contribution.ResurrectCount > 0);
        bool fightActive = fightWeight > 0.0 &&
            (contribution.DamagePerSecond > 0.0 || contribution.OffensiveConditionPressurePerSecond > 0.0 || contribution.StripPerSecond > 0.0 || contribution.ControlConditionPressurePerSecond > 0.0 || contribution.EffectiveCrowdControlPerSecond > 0.0 || contribution.HealingPerSecond > 0.0 || contribution.BarrierPerSecond > 0.0 || contribution.CleansePerSecond > 0.0 || contribution.OffensiveBoonSupportPerSecond > 0.0 || contribution.DefensiveBoonSupportPerSecond > 0.0 || contribution.DefensiveConditionPressurePerSecond > 0.0 || contribution.ResurrectCount > 0);

        aggregate.OffensiveDamage += offensiveWeight * contribution.DamagePerSecond;
        aggregate.OffensiveFocus += offensiveWeight * contribution.TopTargetContribution;
        aggregate.OffensiveStrips += offensiveWeight * contribution.StripPerSecond;
        aggregate.OffensiveConditions += offensiveWeight * contribution.OffensiveConditionPressurePerSecond;
        if (offensiveActive)
        {
            aggregate.OffensiveWindowWeight += window.Weight;
            aggregate.OffensiveActiveWindowCount++;
        }

        aggregate.ControlStrips += controlWeight * contribution.StripPerSecond;
        aggregate.ControlEffectiveCrowdControl += controlWeight * contribution.EffectiveCrowdControlPerSecond;
        aggregate.ControlCrowdControlDuration += controlWeight * contribution.EffectiveCrowdControlDurationPerSecond;
        aggregate.ControlConditions += controlWeight * contribution.ControlConditionPressurePerSecond;
        if (controlActive)
        {
            aggregate.ControlWindowWeight += window.Weight;
            aggregate.ControlActiveWindowCount++;
        }

        aggregate.SupportHealing += supportWeight * contribution.HealingPerSecond;
        aggregate.SupportBarrier += supportWeight * contribution.BarrierPerSecond;
        aggregate.SupportCleanses += supportWeight * contribution.CleansePerSecond;
        aggregate.SupportOffensiveBoons += supportWeight * contribution.OffensiveBoonSupportPerSecond;
        aggregate.SupportDefensiveBoons += supportWeight * contribution.DefensiveBoonSupportPerSecond;
        aggregate.SupportDefensiveConditions += supportWeight * contribution.DefensiveConditionPressurePerSecond;
        aggregate.SupportResurrects += supportWeight * contribution.ResurrectCount;
        if (supportActive)
        {
            aggregate.SupportWindowWeight += window.Weight;
            aggregate.SupportActiveWindowCount++;
        }

        aggregate.FightDamage += fightWeight * (contribution.DamagePerSecond + contribution.OffensiveConditionPressurePerSecond);
        aggregate.FightControl += fightWeight * (contribution.StripPerSecond + contribution.ControlConditionPressurePerSecond + contribution.EffectiveCrowdControlPerSecond + contribution.EffectiveCrowdControlDurationPerSecond);
        aggregate.FightSupport += fightWeight * (contribution.HealingPerSecond + contribution.BarrierPerSecond + contribution.CleansePerSecond + contribution.OffensiveBoonSupportPerSecond + contribution.DefensiveBoonSupportPerSecond + contribution.DefensiveConditionPressurePerSecond + contribution.ResurrectCount * 3.0);
        if (fightActive)
        {
            aggregate.FightWindowWeight += window.Weight;
            aggregate.FightActiveWindowCount++;
        }

        if (contribution.HasPositioningData)
        {
            aggregate.HasPositioningData = true;
            aggregate.PositioningContribution += disciplineWeight * contribution.InPositionRate;
            if (disciplineWeight > 0.0)
            {
                aggregate.PositioningWindowWeight += window.Weight;
            }
        }

        switch (window.SourceCategory)
        {
            case "burst-positive":
            case "burst-negative":
                if (fightActive)
                {
                    aggregate.FightBurstWindowCount++;
                }
                if (offensiveActive)
                {
                    aggregate.OffensiveBurstWindowCount++;
                }
                break;
            case "cluster-kill-positive":
            case "cluster-kill-negative":
            case "cluster-down-positive":
            case "cluster-down-negative":
            case "first-kill-positive":
            case "first-kill-negative":
            case "milestone-five-kills":
                if (fightActive)
                {
                    aggregate.FightConversionWindowCount++;
                }
                if (offensiveActive)
                {
                    aggregate.OffensiveConversionWindowCount++;
                }
                if (controlActive)
                {
                    aggregate.ControlConversionWindowCount++;
                }
                break;
            case "cluster-rez-positive":
            case "cluster-rez-negative":
                if (fightActive)
                {
                    aggregate.FightStabilizeWindowCount++;
                }
                if (supportActive)
                {
                    aggregate.SupportStabilizeWindowCount++;
                }
                break;
            case "stability-drop":
                if (fightActive)
                {
                    aggregate.FightPressureWindowCount++;
                }
                if (supportActive)
                {
                    aggregate.SupportPressureWindowCount++;
                }
                break;
            case "momentum-swing-positive":
            case "momentum-swing-negative":
                if (fightActive)
                {
                    aggregate.FightMomentumWindowCount++;
                }
                break;
            case "formation-break":
            case "enemy-shattered":
            case "enemy-formation-break":
                if (fightActive)
                {
                    aggregate.FightCollapseWindowCount++;
                }
                break;
        }
    }

    private static WvwSummaryPlayerStandoutMetricMaximums BuildPlayerStandoutMetricMaximums(IReadOnlyList<WvwSummaryPlayerStandoutAggregate> aggregates)
    {
        return new WvwSummaryPlayerStandoutMetricMaximums
        {
            OffensiveDamage = aggregates.Max(aggregate => aggregate.OffensiveDamage),
            OffensiveFocus = aggregates.Max(aggregate => aggregate.OffensiveFocus),
            OffensiveStrips = aggregates.Max(aggregate => aggregate.OffensiveStrips),
            OffensiveConditions = aggregates.Max(aggregate => aggregate.OffensiveConditions),
            OffensiveWindowWeight = aggregates.Max(aggregate => aggregate.OffensiveWindowWeight),
            ControlStrips = aggregates.Max(aggregate => aggregate.ControlStrips),
            ControlEffectiveCrowdControl = aggregates.Max(aggregate => aggregate.ControlEffectiveCrowdControl),
            ControlCrowdControlDuration = aggregates.Max(aggregate => aggregate.ControlCrowdControlDuration),
            ControlConditions = aggregates.Max(aggregate => aggregate.ControlConditions),
            ControlWindowWeight = aggregates.Max(aggregate => aggregate.ControlWindowWeight),
            SupportHealing = aggregates.Max(aggregate => aggregate.SupportHealing),
            SupportBarrier = aggregates.Max(aggregate => aggregate.SupportBarrier),
            SupportCleanses = aggregates.Max(aggregate => aggregate.SupportCleanses),
            SupportOffensiveBoons = aggregates.Max(aggregate => aggregate.SupportOffensiveBoons),
            SupportDefensiveBoons = aggregates.Max(aggregate => aggregate.SupportDefensiveBoons),
            SupportDefensiveConditions = aggregates.Max(aggregate => aggregate.SupportDefensiveConditions),
            SupportResurrects = aggregates.Max(aggregate => aggregate.SupportResurrects),
            SupportWindowWeight = aggregates.Max(aggregate => aggregate.SupportWindowWeight),
            FightDamage = aggregates.Max(aggregate => aggregate.FightDamage),
            FightControl = aggregates.Max(aggregate => aggregate.FightControl),
            FightSupport = aggregates.Max(aggregate => aggregate.FightSupport),
            FightWindowWeight = aggregates.Max(aggregate => aggregate.FightWindowWeight),
            PositioningContribution = aggregates.Max(aggregate => aggregate.PositioningContribution),
            PositioningWindowWeight = aggregates.Max(aggregate => aggregate.PositioningWindowWeight),
        };
    }

    private static WvwSummaryPlayerStandoutScoreDto BuildPlayerStandoutScore(
        WvwSummaryPlayerStandoutAggregate aggregate,
        WvwSummaryPlayerStandoutMetricMaximums maximums)
    {
        double offensiveScore = ComputeWeightedStandoutScore(
            (NormalizeStandoutMetric(aggregate.OffensiveDamage, maximums.OffensiveDamage), 0.40, maximums.OffensiveDamage > 0.0),
            (NormalizeStandoutMetric(aggregate.OffensiveFocus, maximums.OffensiveFocus), 0.20, maximums.OffensiveFocus > 0.0),
            (NormalizeStandoutMetric(aggregate.OffensiveConditions, maximums.OffensiveConditions), 0.12, maximums.OffensiveConditions > 0.0),
            (NormalizeStandoutMetric(aggregate.OffensiveStrips, maximums.OffensiveStrips), 0.16, maximums.OffensiveStrips > 0.0),
            (NormalizeStandoutMetric(aggregate.OffensiveWindowWeight, maximums.OffensiveWindowWeight), 0.12, maximums.OffensiveWindowWeight > 0.0));

        double controlScore = ComputeWeightedStandoutScore(
            (NormalizeStandoutMetric(aggregate.ControlStrips, maximums.ControlStrips), 0.32, maximums.ControlStrips > 0.0),
            (NormalizeStandoutMetric(aggregate.ControlEffectiveCrowdControl, maximums.ControlEffectiveCrowdControl), 0.28, maximums.ControlEffectiveCrowdControl > 0.0),
            (NormalizeStandoutMetric(aggregate.ControlCrowdControlDuration, maximums.ControlCrowdControlDuration), 0.14, maximums.ControlCrowdControlDuration > 0.0),
            (NormalizeStandoutMetric(aggregate.ControlConditions, maximums.ControlConditions), 0.16, maximums.ControlConditions > 0.0),
            (NormalizeStandoutMetric(aggregate.ControlWindowWeight, maximums.ControlWindowWeight), 0.10, maximums.ControlWindowWeight > 0.0));

        double supportScore = ComputeWeightedStandoutScore(
            (NormalizeStandoutMetric(aggregate.SupportHealing, maximums.SupportHealing), 0.24, maximums.SupportHealing > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportBarrier, maximums.SupportBarrier), 0.14, maximums.SupportBarrier > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportCleanses, maximums.SupportCleanses), 0.18, maximums.SupportCleanses > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportOffensiveBoons, maximums.SupportOffensiveBoons), 0.08, maximums.SupportOffensiveBoons > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportDefensiveBoons, maximums.SupportDefensiveBoons), 0.12, maximums.SupportDefensiveBoons > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportDefensiveConditions, maximums.SupportDefensiveConditions), 0.10, maximums.SupportDefensiveConditions > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportResurrects, maximums.SupportResurrects), 0.14, maximums.SupportResurrects > 0.0),
            (NormalizeStandoutMetric(aggregate.SupportWindowWeight, maximums.SupportWindowWeight), 0.10, maximums.SupportWindowWeight > 0.0));

        double disciplineScore = ComputeWeightedStandoutScore(
            (NormalizeStandoutMetric(aggregate.PositioningContribution, maximums.PositioningContribution), 0.75, aggregate.HasPositioningData && maximums.PositioningContribution > 0.0),
            (NormalizeStandoutMetric(aggregate.PositioningWindowWeight, maximums.PositioningWindowWeight), 0.25, aggregate.HasPositioningData && maximums.PositioningWindowWeight > 0.0));

        double fightImpactScore = ComputeWeightedStandoutScore(
            (NormalizeStandoutMetric(aggregate.FightDamage, maximums.FightDamage), 0.28, maximums.FightDamage > 0.0),
            (NormalizeStandoutMetric(aggregate.FightControl, maximums.FightControl), 0.24, maximums.FightControl > 0.0),
            (NormalizeStandoutMetric(aggregate.FightSupport, maximums.FightSupport), 0.28, maximums.FightSupport > 0.0),
            (NormalizeStandoutMetric(aggregate.FightWindowWeight, maximums.FightWindowWeight), 0.10, maximums.FightWindowWeight > 0.0),
            (disciplineScore / 100.0, 0.10, aggregate.HasPositioningData));

        double hybridImpactScore = ComputeHybridStandoutScore(offensiveScore, controlScore, supportScore, disciplineScore);
        List<(string Role, double Score)> rankedRoles =
        [
            ("Offense", offensiveScore),
            ("Control", controlScore),
            ("Support", supportScore),
            ("Discipline", disciplineScore),
        ];
        rankedRoles = [.. rankedRoles.OrderByDescending(role => role.Score)];

        return new WvwSummaryPlayerStandoutScoreDto
        {
            PlayerIndex = aggregate.PlayerIndex,
            Name = aggregate.Name,
            Account = aggregate.Account,
            Profession = aggregate.Profession,
            Icon = aggregate.Icon,
            FightImpactScore = fightImpactScore,
            OffensiveImpactScore = offensiveScore,
            ControlImpactScore = controlScore,
            SupportImpactScore = supportScore,
            HybridImpactScore = hybridImpactScore,
            PrimaryRole = rankedRoles[0].Role,
            SecondaryRole = rankedRoles[1].Score >= rankedRoles[0].Score * 0.55 ? rankedRoles[1].Role : "",
            OffensiveWindowCount = aggregate.OffensiveActiveWindowCount,
            ControlWindowCount = aggregate.ControlActiveWindowCount,
            SupportWindowCount = aggregate.SupportActiveWindowCount,
            FightWindowCount = aggregate.FightActiveWindowCount,
            FightBurstWindowCount = aggregate.FightBurstWindowCount,
            FightConversionWindowCount = aggregate.FightConversionWindowCount,
            FightStabilizeWindowCount = aggregate.FightStabilizeWindowCount,
            FightPressureWindowCount = aggregate.FightPressureWindowCount,
            FightMomentumWindowCount = aggregate.FightMomentumWindowCount,
            FightCollapseWindowCount = aggregate.FightCollapseWindowCount,
            OffensiveBurstWindowCount = aggregate.OffensiveBurstWindowCount,
            OffensiveConversionWindowCount = aggregate.OffensiveConversionWindowCount,
            ControlConversionWindowCount = aggregate.ControlConversionWindowCount,
            SupportStabilizeWindowCount = aggregate.SupportStabilizeWindowCount,
            SupportPressureWindowCount = aggregate.SupportPressureWindowCount,
            HasControlCrowdControl = aggregate.ControlEffectiveCrowdControl > 0.0 || aggregate.ControlCrowdControlDuration > 0.0,
            HasControlConditions = aggregate.ControlConditions > 0.0,
            HasHealingSupport = aggregate.SupportHealing > 0.0 || aggregate.SupportBarrier > 0.0,
            HasCleanseSupport = aggregate.SupportCleanses > 0.0,
            HasOffensiveBoonSupport = aggregate.SupportOffensiveBoons > 0.0,
            HasDefensiveBoonSupport = aggregate.SupportDefensiveBoons > 0.0,
            HasDefensiveConditions = aggregate.SupportDefensiveConditions > 0.0,
            HasResurrectSupport = aggregate.SupportResurrects > 0.0,
        };
    }

    private static List<WvwSummaryStandoutEntryDto> BuildStandoutEntriesForCategory(string categoryKey, IReadOnlyList<WvwSummaryPlayerStandoutScoreDto> playerScores)
    {
        Func<WvwSummaryPlayerStandoutScoreDto, double> scoreSelector = categoryKey switch
        {
            "fight" => player => player.FightImpactScore,
            "offense" => player => player.OffensiveImpactScore,
            "control" => player => player.ControlImpactScore,
            "support" => player => player.SupportImpactScore,
            "hybrid" => player => player.HybridImpactScore,
            _ => player => 0.0,
        };

        return [.. playerScores
            .Where(player => scoreSelector(player) >= 12.0)
            .OrderByDescending(scoreSelector)
            .ThenBy(player => player.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(player => BuildStandoutEntry(categoryKey, player))];
    }

    private static WvwSummaryStandoutEntryDto BuildStandoutEntry(string categoryKey, WvwSummaryPlayerStandoutScoreDto player)
    {
        string roleLabel = string.IsNullOrEmpty(player.SecondaryRole)
            ? player.PrimaryRole
            : $"{player.PrimaryRole} + {player.SecondaryRole}";
        string whyLine = categoryKey switch
        {
            "fight" => $"{roleLabel} influence across {Math.Max(player.FightWindowCount, 1)} key windows.",
            "offense" => $"Burst pressure and focus fire in {Math.Max(GetOffenseDisplayWindowCount(player), 1)} successful windows.",
            "control" => player.HasControlConditions && player.HasControlCrowdControl
                ? $"Timed strips, control conditions, and effective CC in {Math.Max(GetControlDisplayWindowCount(player), 1)} conversion windows."
                : player.HasControlCrowdControl
                ? $"Timed strips and effective CC in {Math.Max(GetControlDisplayWindowCount(player), 1)} conversion windows."
                : player.HasControlConditions
                ? $"Timed strips and control conditions in {Math.Max(GetControlDisplayWindowCount(player), 1)} conversion windows."
                : $"Timed strip pressure in {Math.Max(GetControlDisplayWindowCount(player), 1)} conversion windows.",
            "support" => BuildSupportStandoutDetail(player),
            "hybrid" => $"{roleLabel} value across {Math.Max(player.FightWindowCount, 1)} high-leverage windows.",
            _ => $"{roleLabel} standout contribution.",
        };

        return new WvwSummaryStandoutEntryDto
        {
            PlayerIndex = player.PlayerIndex,
            Name = player.Name,
            Account = player.Account,
            Profession = player.Profession,
            Icon = player.Icon,
            RoleLabel = roleLabel,
            WhyLine = whyLine,
            EvidenceTags = BuildStandoutEvidenceTags(categoryKey, player),
        };
    }

    private static List<string> BuildStandoutEvidenceTags(string categoryKey, WvwSummaryPlayerStandoutScoreDto player)
    {
        var tags = new List<string>(3)
        {
            BuildWindowCountTag(player.FightWindowCount),
        };

        switch (categoryKey)
        {
            case "fight":
                AddPrimaryMomentTag(tags, player);
                break;
            case "offense":
                if (player.OffensiveBurstWindowCount > 0)
                {
                    tags.Add(BuildPluralizedTag(player.OffensiveBurstWindowCount, "bomb window", "bomb windows"));
                }
                else if (player.OffensiveConversionWindowCount > 0)
                {
                    tags.Add(BuildPluralizedTag(player.OffensiveConversionWindowCount, "conversion window", "conversion windows"));
                }
                break;
            case "control":
                if (player.ControlConversionWindowCount > 0)
                {
                    tags.Add(BuildPluralizedTag(player.ControlConversionWindowCount, "conversion window", "conversion windows"));
                }
                if (player.HasControlCrowdControl)
                {
                    tags.Add("effective CC");
                }
                else if (player.HasControlConditions)
                {
                    tags.Add("control conditions");
                }
                break;
            case "support":
                if (player.SupportStabilizeWindowCount > 0)
                {
                    tags.Add(BuildPluralizedTag(player.SupportStabilizeWindowCount, "stabilize window", "stabilize windows"));
                }
                else if (player.SupportPressureWindowCount > 0)
                {
                    tags.Add(BuildPluralizedTag(player.SupportPressureWindowCount, "pressure window", "pressure windows"));
                }
                AddSupportTypeTag(tags, player);
                break;
            case "hybrid":
                tags.Add(player.SecondaryRole.Length > 0 ? $"{player.PrimaryRole} + {player.SecondaryRole}" : player.PrimaryRole);
                AddPrimaryMomentTag(tags, player);
                break;
        }

        return [.. tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)];
    }

    private static string BuildWindowCountTag(int fightWindowCount)
    {
        return BuildPluralizedTag(Math.Max(fightWindowCount, 1), "key window", "key windows");
    }

    private static string BuildPluralizedTag(int count, string singular, string plural)
    {
        return count == 1 ? $"1 {singular}" : $"{count} {plural}";
    }

    private static void AddPrimaryMomentTag(List<string> tags, WvwSummaryPlayerStandoutScoreDto player)
    {
        if (player.FightBurstWindowCount > 0)
        {
            tags.Add(BuildPluralizedTag(player.FightBurstWindowCount, "bomb window", "bomb windows"));
            return;
        }
        if (player.FightConversionWindowCount > 0)
        {
            tags.Add(BuildPluralizedTag(player.FightConversionWindowCount, "conversion window", "conversion windows"));
            return;
        }
        if (player.FightStabilizeWindowCount > 0)
        {
            tags.Add(BuildPluralizedTag(player.FightStabilizeWindowCount, "stabilize window", "stabilize windows"));
            return;
        }
        if (player.FightMomentumWindowCount > 0)
        {
            tags.Add(BuildPluralizedTag(player.FightMomentumWindowCount, "momentum swing", "momentum swings"));
            return;
        }
        if (player.FightCollapseWindowCount > 0)
        {
            tags.Add(BuildPluralizedTag(player.FightCollapseWindowCount, "collapse window", "collapse windows"));
        }
    }

    private static void AddSupportTypeTag(List<string> tags, WvwSummaryPlayerStandoutScoreDto player)
    {
        if (player.HasResurrectSupport)
        {
            tags.Add("rez impact");
            return;
        }
        if (player.HasDefensiveBoonSupport && player.HasDefensiveConditions)
        {
            tags.Add("boons + conditions");
            return;
        }
        if (player.HasDefensiveBoonSupport && player.HasCleanseSupport)
        {
            tags.Add("boons + cleanse");
            return;
        }
        if (player.HasDefensiveBoonSupport)
        {
            tags.Add("defensive boons");
            return;
        }
        if (player.HasOffensiveBoonSupport)
        {
            tags.Add("offensive boons");
            return;
        }
        if (player.HasHealingSupport && player.HasCleanseSupport)
        {
            tags.Add("heal + cleanse");
            return;
        }
        if (player.HasHealingSupport)
        {
            tags.Add("healing");
            return;
        }
        if (player.HasCleanseSupport)
        {
            tags.Add("cleanse impact");
            return;
        }
        if (player.HasDefensiveConditions)
        {
            tags.Add("defensive conditions");
        }
    }

    private static string BuildSupportStandoutDetail(WvwSummaryPlayerStandoutScoreDto player)
    {
        int supportWindowCount = Math.Max(GetSupportDisplayWindowCount(player), 1);
        if (player.HasDefensiveBoonSupport && player.HasHealingSupport && player.HasCleanseSupport)
        {
            return $"Defensive boons, healing, and cleanses across {supportWindowCount} pressure windows.";
        }
        if (player.HasDefensiveBoonSupport && player.HasCleanseSupport)
        {
            return $"Defensive boons and cleanses across {supportWindowCount} pressure windows.";
        }
        if (player.HasDefensiveBoonSupport && player.HasDefensiveConditions)
        {
            return $"Defensive boons and conditions across {supportWindowCount} pressure windows.";
        }
        if (player.HasDefensiveBoonSupport)
        {
            return $"Defensive boon support across {supportWindowCount} pressure windows.";
        }
        if (player.HasOffensiveBoonSupport)
        {
            return $"Timed boon support across {supportWindowCount} high-pressure windows.";
        }
        if (player.HasHealingSupport && player.HasCleanseSupport && player.HasResurrectSupport)
        {
            return $"Healing, cleanses, and rez support across {supportWindowCount} pressure windows.";
        }
        if (player.HasHealingSupport && player.HasCleanseSupport)
        {
            return $"Healing and cleanse support across {supportWindowCount} pressure windows.";
        }
        if (player.HasCleanseSupport && player.HasResurrectSupport)
        {
            return $"Cleanse and rez support across {supportWindowCount} pressure windows.";
        }
        if (player.HasHealingSupport)
        {
            return $"Timed healing support across {supportWindowCount} pressure windows.";
        }
        if (player.HasCleanseSupport)
        {
            return $"Timed cleanse support across {supportWindowCount} pressure windows.";
        }
        if (player.HasResurrectSupport)
        {
            return $"Timed rez support across {supportWindowCount} pressure windows.";
        }
        return $"{player.PrimaryRole} support across {supportWindowCount} pressure windows.";
    }

    private static int GetOffenseDisplayWindowCount(WvwSummaryPlayerStandoutScoreDto player)
    {
        if (player.OffensiveBurstWindowCount > 0)
        {
            return player.OffensiveBurstWindowCount;
        }
        if (player.OffensiveConversionWindowCount > 0)
        {
            return player.OffensiveConversionWindowCount;
        }
        return player.OffensiveWindowCount;
    }

    private static int GetControlDisplayWindowCount(WvwSummaryPlayerStandoutScoreDto player)
    {
        return player.ControlConversionWindowCount > 0
            ? player.ControlConversionWindowCount
            : player.ControlWindowCount;
    }

    private static int GetSupportDisplayWindowCount(WvwSummaryPlayerStandoutScoreDto player)
    {
        int supportContextWindowCount = player.SupportStabilizeWindowCount + player.SupportPressureWindowCount;
        return supportContextWindowCount > 0
            ? supportContextWindowCount
            : player.SupportWindowCount;
    }

    private static List<WvwSummaryStandoutWindow> BuildStandoutWindows(PhaseData phase, IReadOnlyList<WvwSummaryMomentDto> moments)
    {
        if (moments.Count == 0)
        {
            return [];
        }

        var windows = new List<WvwSummaryStandoutWindow>(moments.Count);
        foreach (WvwSummaryMomentDto moment in moments)
        {
            windows.Add(CreateStandoutWindow(phase, moment));
        }
        return windows;
    }

    private static WvwSummaryStandoutWindow CreateStandoutWindow(PhaseData phase, WvwSummaryMomentDto moment)
    {
        long startOffset = -3000;
        long endOffset = 4000;
        double weight = 1.0;
        double offenseWeight = moment.Tone == "positive" ? 0.8 : 0.1;
        double controlWeight = moment.Tone == "positive" ? 0.6 : 0.1;
        double supportWeight = moment.Tone == "negative" ? 0.8 : 0.2;
        double fightWeight = 1.0;
        double disciplineWeight = moment.Tone == "negative" ? 0.7 : 0.2;

        switch (moment.Category)
        {
            case "burst-positive":
                startOffset = -2000;
                endOffset = 3000;
                offenseWeight = 1.0;
                controlWeight = 0.75;
                supportWeight = 0.15;
                disciplineWeight = 0.15;
                break;
            case "burst-negative":
                startOffset = -2000;
                endOffset = 3000;
                offenseWeight = 0.1;
                controlWeight = 0.05;
                supportWeight = 1.0;
                disciplineWeight = 0.8;
                break;
            case "cluster-kill-positive":
                startOffset = -3000;
                endOffset = 4000;
                weight = 1.15;
                offenseWeight = 1.0;
                controlWeight = 0.8;
                supportWeight = 0.15;
                disciplineWeight = 0.1;
                break;
            case "cluster-kill-negative":
                startOffset = -3000;
                endOffset = 4000;
                weight = 1.15;
                offenseWeight = 0.05;
                controlWeight = 0.05;
                supportWeight = 1.0;
                disciplineWeight = 0.9;
                break;
            case "cluster-down-positive":
                startOffset = -2500;
                endOffset = 3500;
                offenseWeight = 0.95;
                controlWeight = 0.7;
                supportWeight = 0.15;
                disciplineWeight = 0.1;
                break;
            case "cluster-down-negative":
                startOffset = -2500;
                endOffset = 3500;
                offenseWeight = 0.05;
                controlWeight = 0.05;
                supportWeight = 0.95;
                disciplineWeight = 0.85;
                break;
            case "cluster-rez-positive":
                startOffset = -2500;
                endOffset = 4000;
                supportWeight = 1.0;
                offenseWeight = 0.1;
                controlWeight = 0.1;
                fightWeight = 0.9;
                disciplineWeight = 0.45;
                break;
            case "cluster-rez-negative":
                startOffset = -2500;
                endOffset = 4000;
                supportWeight = 0.75;
                offenseWeight = 0.3;
                controlWeight = 0.2;
                fightWeight = 0.9;
                disciplineWeight = 0.35;
                break;
            case "first-kill-positive":
                startOffset = -3000;
                endOffset = 3000;
                weight = 0.85;
                offenseWeight = 0.9;
                controlWeight = 0.65;
                supportWeight = 0.1;
                disciplineWeight = 0.1;
                break;
            case "first-kill-negative":
                startOffset = -3000;
                endOffset = 3000;
                weight = 0.85;
                offenseWeight = 0.05;
                controlWeight = 0.05;
                supportWeight = 0.9;
                disciplineWeight = 0.8;
                break;
            case "milestone-five-kills":
                startOffset = -4000;
                endOffset = 5000;
                weight = 1.1;
                offenseWeight = moment.Tone == "positive" ? 0.95 : 0.05;
                controlWeight = moment.Tone == "positive" ? 0.7 : 0.05;
                supportWeight = moment.Tone == "negative" ? 0.95 : 0.15;
                disciplineWeight = moment.Tone == "negative" ? 0.8 : 0.1;
                break;
            case "momentum-swing-positive":
                startOffset = -5000;
                endOffset = 8000;
                weight = 1.2;
                offenseWeight = 0.7;
                controlWeight = 0.55;
                supportWeight = 0.55;
                disciplineWeight = 0.35;
                break;
            case "momentum-swing-negative":
                startOffset = -5000;
                endOffset = 8000;
                weight = 1.2;
                offenseWeight = 0.1;
                controlWeight = 0.1;
                supportWeight = 1.0;
                disciplineWeight = 0.9;
                break;
            case "formation-break":
                startOffset = -4000;
                endOffset = 6000;
                weight = 1.05;
                offenseWeight = 0.0;
                controlWeight = 0.0;
                supportWeight = 0.55;
                disciplineWeight = 1.15;
                break;
            case "stability-drop":
                startOffset = -4000;
                endOffset = 6000;
                weight = 1.1;
                offenseWeight = 0.0;
                controlWeight = 0.05;
                supportWeight = 1.05;
                disciplineWeight = 1.0;
                break;
            case "enemy-shattered":
                startOffset = -5000;
                endOffset = 7000;
                weight = 1.25;
                offenseWeight = 1.1;
                controlWeight = 0.85;
                supportWeight = 0.15;
                disciplineWeight = 0.1;
                break;
            case "enemy-formation-break":
                startOffset = -4000;
                endOffset = 6000;
                weight = 1.1;
                offenseWeight = 1.0;
                controlWeight = 0.8;
                supportWeight = 0.15;
                disciplineWeight = 0.1;
                break;
        }

        return new WvwSummaryStandoutWindow
        {
            StartTime = Math.Max(phase.Start, moment.Time + startOffset),
            EndTime = Math.Min(phase.End, moment.Time + endOffset),
            SourceCategory = moment.Category,
            Weight = weight,
            OffenseWeight = offenseWeight,
            ControlWeight = controlWeight,
            SupportWeight = supportWeight,
            FightWeight = fightWeight,
            DisciplineWeight = disciplineWeight,
        };
    }

    private static double ComputeConditionSupportForWindow(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        long windowStart,
        long windowEnd,
        IReadOnlyList<long> buffIds)
    {
        double totalSeconds = 0.0;
        foreach (SingleActor recipient in recipients)
        {
            foreach (long buffId in buffIds)
            {
                foreach (AbstractBuffApplyEvent applyEvent in recipient.GetBuffApplyEventsOnByID(log, windowStart, windowEnd, buffId, provider))
                {
                    switch (applyEvent)
                    {
                        case BuffApplyEvent buffApplyEvent when buffApplyEvent.AppliedDuration < int.MaxValue:
                            totalSeconds += buffApplyEvent.AppliedDuration / 1000.0;
                            break;
                        case BuffExtensionEvent buffExtensionEvent:
                            totalSeconds += buffExtensionEvent.ExtendedDuration / 1000.0;
                            break;
                    }
                }
            }
        }

        return Math.Round(totalSeconds, 1);
    }

    private static double ComputeBoonSupportForWindow(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        long windowStart,
        long windowEnd,
        IReadOnlyList<long> buffIds)
    {
        double totalSeconds = 0.0;
        foreach (SingleActor recipient in recipients)
        {
            if (recipient.UniqueID == provider.UniqueID)
            {
                continue;
            }

            foreach (long buffId in buffIds)
            {
                if (!log.Buffs.BuffsByIDs.ContainsKey(buffId))
                {
                    continue;
                }

                foreach (AbstractBuffApplyEvent applyEvent in recipient.GetBuffApplyEventsOnByID(log, windowStart, windowEnd, buffId, provider))
                {
                    switch (applyEvent)
                    {
                        case BuffApplyEvent buffApplyEvent when buffApplyEvent.AppliedDuration < int.MaxValue:
                            totalSeconds += buffApplyEvent.AppliedDuration / 1000.0;
                            break;
                        case BuffExtensionEvent buffExtensionEvent:
                            totalSeconds += buffExtensionEvent.ExtendedDuration / 1000.0;
                            break;
                    }
                }
            }
        }

        return Math.Round(totalSeconds, 1);
    }

    private static bool TryComputePhaseTargetAdjustedThreatBoonCoverage(
        CombatReplayAnalysisDto combatReplayAnalysis,
        PhaseData phase,
        IReadOnlyList<WvwSummaryExecutionBoonExpectation> boonExpectations,
        IReadOnlyList<WvwSummaryExecutionWindow>? windows,
        out double coverage,
        out string note)
    {
        coverage = 0.0;
        note = "";
        if (combatReplayAnalysis.Times.Length == 0 || combatReplayAnalysis.ThreatBoons.Boons.Count == 0)
        {
            return false;
        }
        if (windows is { Count: 0 })
        {
            return false;
        }

        Dictionary<long, CombatReplayThreatBoonTimelineDto> trackedBoons = combatReplayAnalysis.ThreatBoons.Boons
            .GroupBy(boon => boon.Id)
            .ToDictionary(group => group.Key, group => group.First());
        if (trackedBoons.Count == 0)
        {
            return false;
        }

        bool restrictToWindows = windows != null;
        var boonDetails = new List<string>(boonExpectations.Count);
        double weightedCoverage = 0.0;
        double totalWeight = 0.0;
        foreach (WvwSummaryExecutionBoonExpectation expectation in boonExpectations)
        {
            if (!trackedBoons.TryGetValue(expectation.Id, out CombatReplayThreatBoonTimelineDto? boon))
            {
                continue;
            }

            double rawValueSum = 0.0;
            int threatenedSamples = 0;
            int sampleCount = Math.Min(combatReplayAnalysis.Times.Length, Math.Min(combatReplayAnalysis.ThreatBoons.ThreatenedPlayerCount.Length, boon.CurrentCoverage.Length));
            for (int index = 0; index < sampleCount; index++)
            {
                long time = combatReplayAnalysis.Times[index];
                if (time < phase.Start || time > phase.End)
                {
                    continue;
                }
                if (restrictToWindows && !IsInsideExecutionWindow(time, windows))
                {
                    continue;
                }

                int threatenedPlayerCount = combatReplayAnalysis.ThreatBoons.ThreatenedPlayerCount[index];
                if (threatenedPlayerCount <= 0)
                {
                    continue;
                }

                double rawValue = expectation.TargetStackCount > 0.0 && index < boon.CurrentAverageStacks.Length
                    ? boon.CurrentAverageStacks[index]
                    : boon.CurrentCoverage[index];
                rawValueSum += rawValue * threatenedPlayerCount;
                threatenedSamples += threatenedPlayerCount;
            }

            if (threatenedSamples > 0)
            {
                double averageRawValue = rawValueSum / threatenedSamples;
                double target = expectation.TargetStackCount > 0.0
                    ? expectation.TargetStackCount
                    : expectation.TargetCoveragePercent;
                if (target <= 0.0)
                {
                    continue;
                }

                double boonCoverage = Math.Clamp(averageRawValue * 100.0 / target, 0.0, 100.0);
                weightedCoverage += boonCoverage * expectation.Weight;
                totalWeight += expectation.Weight;
                string detail = expectation.TargetStackCount > 0.0
                    ? $"{boon.Name} {FormatDecimal(averageRawValue)}/{FormatDecimal(target)} average stacks"
                    : $"{boon.Name} {FormatDecimal(averageRawValue)}%/{FormatDecimal(target)}% target";
                boonDetails.Add(detail);
            }
        }

        if (totalWeight <= 0.0)
        {
            return false;
        }

        coverage = Math.Round(weightedCoverage / totalWeight, 1);
        string windowScope = restrictToWindows
            ? $"during {BuildPluralizedTag(windows!.Count, "detected squad pressure window", "detected squad pressure windows")}"
            : "during threatened moments";
        note = $"Target-adjusted and weighted by threatened squad players {windowScope}: {string.Join(", ", boonDetails)}.";
        return true;
    }

    private static double GetAverageWindowSeriesValue(
        IReadOnlyDictionary<int, CombatReplayAnalysisAttackerTimelineDto>? timelines,
        IReadOnlyList<long>? times,
        int actorId,
        Func<CombatReplayAnalysisAttackerTimelineDto, double[]> selector,
        long windowStart,
        long windowEnd)
    {
        if (timelines == null || times == null || !timelines.TryGetValue(actorId, out CombatReplayAnalysisAttackerTimelineDto? timeline))
        {
            return 0.0;
        }
        double[] series = selector(timeline);
        double total = 0.0;
        int count = 0;
        for (int index = 0; index < times.Count && index < series.Length; index++)
        {
            if (times[index] < windowStart || times[index] > windowEnd)
            {
                continue;
            }
            total += series[index];
            count++;
        }
        return count > 0 ? total / count : 0.0;
    }

    private static double GetAveragePlayerInPositionRate(
        CombatReplayPositioningAnalysisDto? positioning,
        IReadOnlyList<long>? times,
        int actorId,
        long windowStart,
        long windowEnd)
    {
        if (!HasEligiblePositioningSamples(positioning, times, actorId, windowStart, windowEnd) ||
            positioning == null ||
            !positioning.Players.TryGetValue(actorId, out CombatReplayPositioningPlayerTimelineDto? timeline))
        {
            return 0.0;
        }

        int eligibleCount = 0;
        int inPositionCount = 0;
        for (int index = 0; index < times!.Count && index < timeline.Eligible.Length && index < timeline.InPosition.Length; index++)
        {
            if (times[index] < windowStart || times[index] > windowEnd || !timeline.Eligible[index])
            {
                continue;
            }
            eligibleCount++;
            if (timeline.InPosition[index])
            {
                inPositionCount++;
            }
        }

        return eligibleCount > 0 ? inPositionCount * 100.0 / eligibleCount : 0.0;
    }

    private static bool HasEligiblePositioningSamples(
        CombatReplayPositioningAnalysisDto? positioning,
        IReadOnlyList<long>? times,
        int actorId,
        long windowStart,
        long windowEnd)
    {
        if (positioning == null || times == null || !positioning.Players.TryGetValue(actorId, out CombatReplayPositioningPlayerTimelineDto? timeline))
        {
            return false;
        }

        for (int index = 0; index < times.Count && index < timeline.Eligible.Length; index++)
        {
            if (times[index] >= windowStart && times[index] <= windowEnd && timeline.Eligible[index])
            {
                return true;
            }
        }
        return false;
    }

    private static double NormalizeStandoutMetric(double value, double maximum)
    {
        return maximum > 0.0 ? Math.Clamp(value / maximum, 0.0, 1.0) : 0.0;
    }

    private static double ComputeWeightedStandoutScore(params (double Value, double Weight, bool Available)[] inputs)
    {
        double totalWeight = 0.0;
        double totalValue = 0.0;
        foreach ((double value, double weight, bool available) in inputs)
        {
            if (!available || weight <= 0.0)
            {
                continue;
            }

            totalWeight += weight;
            totalValue += value * weight;
        }

        return totalWeight > 0.0 ? (totalValue / totalWeight) * 100.0 : 0.0;
    }

    private static double ComputeHybridStandoutScore(double offensiveScore, double controlScore, double supportScore, double disciplineScore)
    {
        double[] rankedScores =
        [
            offensiveScore,
            controlScore,
            supportScore,
            disciplineScore,
        ];
        Array.Sort(rankedScores);
        Array.Reverse(rankedScores);
        return rankedScores[0] * 0.20 + rankedScores[1] * 0.45 + rankedScores[2] * 0.35;
    }

    private static List<WvwSummaryMomentDto> BuildMoments(
        ParsedEvtcLog log,
        PhaseData phase,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState,
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape)
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
        if (TryBuildCompetitiveBoundaryCandidate(fightShape, out WvwSummaryMomentCandidate competitiveBoundary))
        {
            requiredCandidates.Add(competitiveBoundary);
        }

        List<WvwSummaryFormationSnapshot> enemyFormationSnapshots = BuildGroupFormationSnapshots(log, phase, hostilePlayerTargets, squadActors);
        requiredCandidates.AddRange(BuildThreeWaySandwichMomentCandidates(log, phase, squadActors, hostilePlayerTargets));
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
                string detail = $"Our Squad dealt {burst.Damage.ToString("N0", CultureInfo.InvariantCulture)} damage in 3s with {FormatStripCountWithCorrupts(burst.Strips, burst.Corrupts)} strips, causing {burst.Downs} downs and {burst.Kills} kills.";
                double score = burst.Damage / 2000.0 + burst.Strips * 0.5 + burst.Downs * 8 + burst.Kills * 10;
                optionalCandidates.Add(new WvwSummaryMomentCandidate(burst.Time, 0, label, detail, "positive", "burst-positive", score));
            }
        }

        if (combatReplayAnalysis?.Enemy != null && combatReplayAnalysis.Times.Length > 0)
        {
            foreach (CombatReplayAnalysisBurstSummaryDto burst in BuildPhaseTopBursts(combatReplayAnalysis.Enemy, combatReplayAnalysis.Times, phase.Start, phase.End).Take(3))
            {
                string label = burst.Downs > 0 || burst.Kills > 0 ? "Enemy bomb landed" : "Enemy pressure spike";
                string detail = $"Enemy Team dealt {burst.Damage.ToString("N0", CultureInfo.InvariantCulture)} damage in 3s with {FormatStripCountWithCorrupts(burst.Strips, burst.Corrupts)} strips, causing {burst.Downs} downs and {burst.Kills} kills.";
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

    private static GW2EIBuilders.WvWAnalystOutcomeDto BuildFightShapeOutcome(WvwSummarySideDto squad, WvwSummarySideDto enemy)
    {
        // Keep this winner/tiebreak logic aligned with WvWAnalystBuilder.BuildOutcome; the fight-shape detector
        // uses final winner only as a hindsight guard against marking reversible fights as cleanup.
        const string squadSideId = "squad";
        const string enemySideId = "enemy";
        string outcomeCode;
        string winnerSideId;
        string displayLabel;
        string decidedBy;

        if (squad.Kills != enemy.Kills)
        {
            bool squadWon = squad.Kills > enemy.Kills;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? squad.Label : enemy.Label;
            decidedBy = "kills";
        }
        else if (squad.Downs != enemy.Downs)
        {
            bool squadWon = squad.Downs > enemy.Downs;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? squad.Label : enemy.Label;
            decidedBy = "downs";
        }
        else if (squad.Deaths != enemy.Deaths)
        {
            bool squadWon = squad.Deaths < enemy.Deaths;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? squad.Label : enemy.Label;
            decidedBy = "deaths";
        }
        else if (squad.Damage != enemy.Damage)
        {
            bool squadWon = squad.Damage > enemy.Damage;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? squad.Label : enemy.Label;
            decidedBy = "damage";
        }
        else
        {
            outcomeCode = "draw";
            winnerSideId = string.Empty;
            displayLabel = "Draw";
            decidedBy = "none";
        }

        return new GW2EIBuilders.WvWAnalystOutcomeDto
        {
            OutcomeCode = outcomeCode,
            WinnerSideId = winnerSideId,
            DisplayLabel = displayLabel,
            DecidedBy = decidedBy,
            TieBreakOrder = ["kills", "downs", "deaths", "damage"],
        };
    }

    private static List<WvwSummaryMomentCandidate> BuildThreeWaySandwichMomentCandidates(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadActors,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        List<WvwSummaryThreeWaySnapshot> snapshots = BuildThreeWaySnapshots(log, phase, squadActors, hostilePlayerTargets);
        if (snapshots.Count == 0)
        {
            return [];
        }

        var candidates = new List<WvwSummaryMomentCandidate>();
        bool inSandwich = false;
        long? separatedSince = null;
        long? previousSnapshotTime = null;

        foreach (WvwSummaryThreeWaySnapshot snapshot in snapshots.OrderBy(snapshot => snapshot.Time))
        {
            if (previousSnapshotTime.HasValue && snapshot.Time - previousSnapshotTime.Value >= ThreeWaySandwichReseparationMs)
            {
                inSandwich = false;
                separatedSince = null;
            }

            bool sandwich = snapshot.PrimaryDistanceToSquad <= ThreeWaySandwichRange &&
                snapshot.SecondaryDistanceToSquad <= ThreeWaySandwichRange;
            if (sandwich)
            {
                if (!inSandwich)
                {
                    string detail = $"Both enemy team centers were within {FormatWholeNumber(ThreeWaySandwichRange)} units of our squad center at the same time: team {FormatTeamId(snapshot.PrimaryTeamId)} was {FormatWholeNumber(snapshot.PrimaryDistanceToSquad)} units away, team {FormatTeamId(snapshot.SecondaryTeamId)} was {FormatWholeNumber(snapshot.SecondaryDistanceToSquad)} units away, and the enemy centers were {FormatWholeNumber(snapshot.EnemyCenterDistance)} units apart. This marks a three-way sandwich/contact point; it may be enemy pressure on us or our squad moving into two already-engaged enemy groups.";
                    double score = 72.0 + Math.Max(0.0, ThreeWaySandwichRange - Math.Max(snapshot.PrimaryDistanceToSquad, snapshot.SecondaryDistanceToSquad)) / 20.0;
                    candidates.Add(new WvwSummaryMomentCandidate(
                        snapshot.Time,
                        0,
                        "Three-way sandwich",
                        detail,
                        "neutral",
                        "three-way-sandwich",
                        score));
                    inSandwich = true;
                }
                separatedSince = null;
            }
            else if (inSandwich &&
                (snapshot.PrimaryDistanceToSquad >= ThreeWaySandwichExitRange ||
                    snapshot.SecondaryDistanceToSquad >= ThreeWaySandwichExitRange))
            {
                separatedSince ??= snapshot.Time;
                if (snapshot.Time - separatedSince.Value >= ThreeWaySandwichReseparationMs)
                {
                    inSandwich = false;
                    separatedSince = null;
                }
            }
            else
            {
                separatedSince = null;
            }

            previousSnapshotTime = snapshot.Time;
        }

        return candidates;
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

    private static bool TryBuildCompetitiveBoundaryCandidate(
        GW2EIBuilders.WvWAnalystFightShapeDto? fightShape,
        out WvwSummaryMomentCandidate candidate)
    {
        candidate = default;
        if (fightShape?.Available != true || fightShape.CleanupStartTimeMs is not long cleanupStart)
        {
            return false;
        }

        bool squadCleanup = string.Equals(fightShape.CleanupSide, "squad", StringComparison.OrdinalIgnoreCase);
        string winningLabel = squadCleanup ? "Our Squad" : "Enemy Team";
        string losingLabel = squadCleanup ? "Enemy Team" : "Our Squad";
        string tone = squadCleanup ? "positive" : "negative";

        // Surface the same fight-shape detector used by WvWAnalyst as a first-class EI Moment.
        // The TSV/debug UI stays in WvWAnalyst, but this marker is useful in normal log review.
        candidate = new WvwSummaryMomentCandidate(
            cleanupStart,
            0,
            "Competitive phase ended",
            $"{winningLabel} held a sustained advantage and {losingLabel} did not meaningfully recover; later time is treated as cleanup.",
            tone,
            squadCleanup ? "competitive-boundary-positive" : "competitive-boundary-negative",
            90.0);
        return true;
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
                Corrupts = analysis.Corrupts[bestIndex],
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
            double centroidTightShare = participatingGroupPositions.Count(position => GetDistance2D(position, groupCentroid) <= enemyAllyRange) / (double)playerCount;
            double compactnessScore = Clamp01((740.0 - averageDistanceToCentroid) / 500.0);
            double snapshotScore = Math.Round((clusteredShare * 0.5 + compactnessScore * 0.5) * 100.0, 1);

            snapshots.Add(new WvwSummaryFormationSnapshot(
                time,
                playerCount,
                participatingOpposingPositions.Count,
                clusteredShare,
                compactnessScore,
                centroidTightShare,
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

    private static string FormatStripCountWithCorrupts(int strips, int corrupts)
    {
        return $"{strips.ToString("N0", CultureInfo.InvariantCulture)} ({corrupts.ToString("N0", CultureInfo.InvariantCulture)})";
    }

    private static string FormatStripCountWithCorrupts(double strips, double corrupts)
    {
        return $"{FormatDecimal(strips)} ({FormatDecimal(corrupts)})";
    }

    private static string FormatSignedDecimal(double value, int decimals = 1)
    {
        string format = decimals <= 0 ? "0" : $"0.{new string('0', decimals)}";
        string formatted = value.ToString(format, CultureInfo.InvariantCulture);
        return value > 0 ? $"+{formatted}" : formatted;
    }

    private static string FormatCountValue(double value)
    {
        return Math.Abs(value - Math.Round(value)) < 0.05
            ? Math.Round(value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatWholeNumber(double value)
    {
        return Math.Round(value).ToString("N0", CultureInfo.InvariantCulture);
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
        if (actor.TryGetCurrentInterpolatedPosition(log, time, out Vector3? interpolatedPosition))
        {
            position = interpolatedPosition.Value;
            return true;
        }
        if (actor.TryGetCurrentPosition(log, time, out Vector3? currentPosition))
        {
            position = currentPosition.Value;
            return true;
        }
        return false;
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

    private static List<WvwSummaryMetricRowDto> BuildMetricRows(
        long durationInMilliseconds,
        WvwSummarySideDto squad,
        WvwSummarySideDto enemy,
        WvwSummaryDownStateSideDto squadDownState,
        WvwSummaryDownStateSideDto enemyDownState,
        int friendlyPlayerCount)
    {
        string squadPlayersValue = friendlyPlayerCount > 0
            ? $"{squad.PlayerCount} (+{friendlyPlayerCount} friendlies)"
            : squad.PlayerCount.ToString();
        int squadDowns = enemyDownState.Downs;
        int enemyDowns = squadDownState.Downs;
        int squadKills = enemyDownState.KillConversions;
        int enemyKills = squadDownState.KillConversions;
        double squadDownKillConversionRate = squadDowns > 0 ? Math.Round(100.0 * squadKills / squadDowns, 1) : 0.0;
        double enemyDownKillConversionRate = enemyDowns > 0 ? Math.Round(100.0 * enemyKills / enemyDowns, 1) : 0.0;

        return
        [
            new WvwSummaryMetricRowDto("Fight Time", ToDurationString(durationInMilliseconds), ToDurationString(durationInMilliseconds), true),
            new WvwSummaryMetricRowDto("Players", squadPlayersValue, enemy.PlayerCount.ToString()),
            new WvwSummaryMetricRowDto("Outgoing Damage", squad.Damage.ToString(), enemy.Damage.ToString(), false, true),
            new WvwSummaryMetricRowDto("DPS", squad.Dps.ToString(CultureInfo.InvariantCulture), enemy.Dps.ToString(CultureInfo.InvariantCulture), false, true, 1),
            new WvwSummaryMetricRowDto("Downs", squadDowns.ToString(), enemyDowns.ToString()),
            new WvwSummaryMetricRowDto("Kills", squadKills.ToString(), enemyKills.ToString()),
            new WvwSummaryMetricRowDto("Down to Kill %", squadDownKillConversionRate.ToString(CultureInfo.InvariantCulture), enemyDownKillConversionRate.ToString(CultureInfo.InvariantCulture), false, false, 1, true),
            new WvwSummaryMetricRowDto("Boon Strips", FormatStripCountWithCorrupts(squad.BoonStrips, squad.BoonCorrupts), FormatStripCountWithCorrupts(enemy.BoonStrips, enemy.BoonCorrupts)),
            new WvwSummaryMetricRowDto("Strips / Min", FormatStripCountWithCorrupts(squad.StripsPerMinute, squad.CorruptsPerMinute), FormatStripCountWithCorrupts(enemy.StripsPerMinute, enemy.CorruptsPerMinute)),
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
        Dictionary<AgentItem, int> corruptsByAttacker = CombatReplayAnalysisBuilder.CountBoonCorruptsByAttacker(
            log,
            phase.Start,
            phase.End,
            squadActors,
            GetHostilePlayerTargets(log, phase));
        return BuildTopPlayers(
            squadActors,
            actor => GetFriendlyPlayerIndex(log, actor),
            actor => actor.GetToAllySupportStats(log, phase.Start, phase.End).BoonStripCount,
            actor => corruptsByAttacker.GetValueOrDefault(actor.EnglobingAgentItem));
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
        Dictionary<AgentItem, int> corruptsByAttacker = CombatReplayAnalysisBuilder.CountBoonCorruptsByAttacker(
            log,
            phase.Start,
            phase.End,
            hostilePlayerTargets,
            log.PlayerList
                .Where(player => !player.IsFakeActor && IsActiveInPhase(player, phase))
                .Cast<SingleActor>()
                .ToList());
        return BuildTopPlayers(
            hostilePlayerTargets,
            actor => GetTargetIndex(log, actor),
            actor => actor.GetToAllySupportStats(log, phase.Start, phase.End).BoonStripCount,
            actor => corruptsByAttacker.GetValueOrDefault(actor.EnglobingAgentItem));
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

    private static List<WvwSummaryTopPlayerDto> BuildTopPlayers(
        IReadOnlyList<SingleActor> actors,
        Func<SingleActor, int> indexGetter,
        Func<SingleActor, long> valueGetter,
        Func<SingleActor, long>? secondaryValueGetter = null)
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
                SecondaryAmount = secondaryValueGetter?.Invoke(actor) ?? 0,
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
        if (totalPlayers <= 0)
        {
            return "Healing Stats add-on coverage is unavailable because this phase has no active squad players.";
        }
        if (healAddonPlayers >= totalPlayers)
        {
            return $"Healing Stats add-on detected for all {totalPlayers} active squad players in this phase.";
        }
        if (healAddonPlayers > 0)
        {
            return $"Heal stats are incomplete: Healing Stats add-on detected for {healAddonPlayers} of {totalPlayers} active squad players in this phase.";
        }
        return $"Healing Stats add-on data is unavailable for active squad players in this phase.";
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
    public int BoonCorrupts { get; set; }
    public int Cleanses { get; set; }
    public int Resurrects { get; set; }
    public long DamageTaken { get; set; }
    public int Deaths { get; set; }
    public int ReceivedCrowdControl { get; set; }
    public double StripsPerMinute { get; set; }
    public double CorruptsPerMinute { get; set; }
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

internal class WvwSummaryFightExecutionScoreDto
{
    public bool ScoreAvailable { get; set; }
    public int OverallScore { get; set; }
    public string Grade { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public WvwSummaryExecutionContextDto Context { get; set; } = new();
    public WvwSummaryExecutionOutcomeDto Outcome { get; set; } = new();
    public WvwSummaryExecutionNumbersAdjustmentDto NumbersAdjustment { get; set; } = new();
    public WvwSummaryExecutionConfidenceDto Confidence { get; set; } = new();
    public List<WvwSummaryExecutionPillarDto> Pillars { get; set; } = [];
    public string StrongestPillarLabel { get; set; } = "";
    public string StrongestPillarSummary { get; set; } = "";
    public string WeakestPillarLabel { get; set; } = "";
    public string WeakestPillarSummary { get; set; } = "";
}

internal class WvwSummaryExecutionContextDto
{
    public int SquadPlayerCount { get; set; }
    public int EnemyPlayerCount { get; set; }
    public string EnemyFormationStyleLabel { get; set; } = "";
    public string EnemyFormationStyleDetail { get; set; } = "";
    public int? EnemyMovementScore { get; set; }
    public string EnemyMovementScoreLabel { get; set; } = "";
    public string EnemyMovementScoreDetail { get; set; } = "";
    public double? EnemyMovementCenterTightShare { get; set; }
    public double? EnemyMovementAverageDistanceToCenter { get; set; }
    public int EnemyMovementSampleCount { get; set; }
    public bool ThreeWayDetected { get; set; }
    public string ThreeWayLabel { get; set; } = "";
    public string ThreeWayDetail { get; set; } = "";
    public long? ThreeWayStartTimeMs { get; set; }
    public int ThreeWaySecondEnemyPeakCount { get; set; }
    public int FriendlyNonSquadCount { get; set; }
    public string PhaseDuration { get; set; } = "";
    public string DataConfidenceLabel { get; set; } = "";
    public string DataConfidenceDetail { get; set; } = "";
}

internal class WvwSummaryExecutionOutcomeDto
{
    public int SquadDowns { get; set; }
    public int EnemyDowns { get; set; }
    public int SquadKills { get; set; }
    public int EnemyKills { get; set; }
    public int SquadDeaths { get; set; }
    public int EnemyDeaths { get; set; }
    public double EnemyDownConversionRate { get; set; }
    public double SquadRecoveryRate { get; set; }
    public bool CrowdControlDataAvailable { get; set; }
    public int IncomingCrowdControl { get; set; }
    public int OutgoingCrowdControl { get; set; }
    public string WipeLabel { get; set; } = "";
}

internal class WvwSummaryExecutionConfidenceDto
{
    public string Label { get; set; } = "";
    public int AvailableMetricCount { get; set; }
    public int TotalMetricCount { get; set; }
    public List<string> Notes { get; set; } = [];
}

internal class WvwSummaryExecutionNumbersAdjustmentDto
{
    public bool IsApplied { get; set; }
    public int RawScore { get; set; }
    public int AdjustedScore { get; set; }
    public string AdjustedGrade { get; set; } = "";
    public int PlayerGap { get; set; }
    public int AbsolutePlayerGap { get; set; }
    public int EffectivePlayerGap { get; set; }
    public double FullWeightAdjustment { get; set; }
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
}

internal class WvwSummaryExecutionPillarDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public int Score { get; set; }
    public string Grade { get; set; } = "";
    public int AdjustedScore { get; set; }
    public string AdjustedGrade { get; set; } = "";
    public bool AdjustmentApplied { get; set; }
    public string AdjustmentDetail { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public int AvailableMetricCount { get; set; }
    public int MetricCount { get; set; }
    public List<WvwSummaryExecutionMetricDto> Metrics { get; set; } = [];
}

internal class WvwSummaryExecutionMetricDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Note { get; set; } = "";
    public bool Available { get; set; }
    public double WeightPercent { get; set; }
    public int Score { get; set; }
}

internal readonly record struct WvwSummaryExecutionBoonExpectation(
    long Id,
    double Weight,
    double TargetCoveragePercent,
    double TargetStackCount = 0.0);

internal class WvwSummaryPhasePositioningDto
{
    public bool CommanderAvailable { get; set; }
    public long EvaluatedSamples { get; set; }
    public double InPositionRate { get; set; }
    public double TooFarRate { get; set; }
    public double OverextendedRate { get; set; }
    public double LateralRiskRate { get; set; }
    public bool HasData => CommanderAvailable && EvaluatedSamples > 0;
}

internal readonly record struct WvwSummaryFormationSnapshot(
    long Time,
    int PlayerCount,
    int OpposingPlayerCount,
    double ClusteredShare,
    double CompactnessScore,
    double CentroidTightShare,
    double AverageDistanceToCentroid,
    double SnapshotScore,
    double DistanceToOpposingCentroid);

internal readonly record struct WvwSummaryThreeWayContext(
    bool Detected,
    long? StartTimeMs,
    int SecondEnemyPeakCount,
    string Label,
    string Detail);

internal readonly record struct WvwSummaryEnemyTeamSnapshot(
    ulong TeamId,
    int PlayerCount,
    Vector3 Centroid,
    double DistanceToSquad);

internal readonly record struct WvwSummaryThreeWaySnapshot(
    long Time,
    ulong PrimaryTeamId,
    ulong SecondaryTeamId,
    int PrimaryEnemyCount,
    int SecondEnemyCount,
    double PrimaryDistanceToSquad,
    double SecondaryDistanceToSquad,
    double EnemyCenterDistance);

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
    public long SecondaryAmount { get; set; }
    public int EffectiveAmount { get; set; }
    public double TotalDuration { get; set; }
    public List<WvwSummarySkillDetailDto> SkillDetails { get; set; } = [];
}

internal class WvwSummaryPlayerStandoutsDto
{
    public string Summary { get; set; } = "";
    public List<WvwSummaryStandoutCategoryDto> Categories { get; set; } = [];
    public bool HasEntries => Categories.Any(category => category.Entries.Count > 0);
}

internal class WvwSummaryStandoutCategoryDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Detail { get; set; } = "";
    public List<WvwSummaryStandoutEntryDto> Entries { get; set; } = [];
}

internal class WvwSummaryStandoutEntryDto
{
    public int PlayerIndex { get; set; }
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public string RoleLabel { get; set; } = "";
    public string WhyLine { get; set; } = "";
    public List<string> EvidenceTags { get; set; } = [];
}

internal class WvwSummaryPlayerStandoutAggregate
{
    public int PlayerIndex { get; set; }
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public double OffensiveDamage { get; set; }
    public double OffensiveFocus { get; set; }
    public double OffensiveStrips { get; set; }
    public double OffensiveConditions { get; set; }
    public double OffensiveWindowWeight { get; set; }
    public double ControlStrips { get; set; }
    public double ControlEffectiveCrowdControl { get; set; }
    public double ControlCrowdControlDuration { get; set; }
    public double ControlConditions { get; set; }
    public double ControlWindowWeight { get; set; }
    public double SupportHealing { get; set; }
    public double SupportBarrier { get; set; }
    public double SupportCleanses { get; set; }
    public double SupportOffensiveBoons { get; set; }
    public double SupportDefensiveBoons { get; set; }
    public double SupportDefensiveConditions { get; set; }
    public double SupportResurrects { get; set; }
    public double SupportWindowWeight { get; set; }
    public double FightDamage { get; set; }
    public double FightControl { get; set; }
    public double FightSupport { get; set; }
    public double FightWindowWeight { get; set; }
    public bool HasPositioningData { get; set; }
    public double PositioningContribution { get; set; }
    public double PositioningWindowWeight { get; set; }
    public int OffensiveActiveWindowCount { get; set; }
    public int ControlActiveWindowCount { get; set; }
    public int SupportActiveWindowCount { get; set; }
    public int FightActiveWindowCount { get; set; }
    public int FightBurstWindowCount { get; set; }
    public int FightConversionWindowCount { get; set; }
    public int FightStabilizeWindowCount { get; set; }
    public int FightPressureWindowCount { get; set; }
    public int FightMomentumWindowCount { get; set; }
    public int FightCollapseWindowCount { get; set; }
    public int OffensiveBurstWindowCount { get; set; }
    public int OffensiveConversionWindowCount { get; set; }
    public int ControlConversionWindowCount { get; set; }
    public int SupportStabilizeWindowCount { get; set; }
    public int SupportPressureWindowCount { get; set; }
}

internal class WvwSummaryPlayerStandoutMetricMaximums
{
    public double OffensiveDamage { get; set; }
    public double OffensiveFocus { get; set; }
    public double OffensiveStrips { get; set; }
    public double OffensiveConditions { get; set; }
    public double OffensiveWindowWeight { get; set; }
    public double ControlStrips { get; set; }
    public double ControlEffectiveCrowdControl { get; set; }
    public double ControlCrowdControlDuration { get; set; }
    public double ControlConditions { get; set; }
    public double ControlWindowWeight { get; set; }
    public double SupportHealing { get; set; }
    public double SupportBarrier { get; set; }
    public double SupportCleanses { get; set; }
    public double SupportOffensiveBoons { get; set; }
    public double SupportDefensiveBoons { get; set; }
    public double SupportDefensiveConditions { get; set; }
    public double SupportResurrects { get; set; }
    public double SupportWindowWeight { get; set; }
    public double FightDamage { get; set; }
    public double FightControl { get; set; }
    public double FightSupport { get; set; }
    public double FightWindowWeight { get; set; }
    public double PositioningContribution { get; set; }
    public double PositioningWindowWeight { get; set; }
}

internal class WvwSummaryPlayerStandoutScoreDto
{
    public int PlayerIndex { get; set; }
    public string Name { get; set; } = "";
    public string Account { get; set; } = "";
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public double FightImpactScore { get; set; }
    public double OffensiveImpactScore { get; set; }
    public double ControlImpactScore { get; set; }
    public double SupportImpactScore { get; set; }
    public double HybridImpactScore { get; set; }
    public string PrimaryRole { get; set; } = "";
    public string SecondaryRole { get; set; } = "";
    public int OffensiveWindowCount { get; set; }
    public int ControlWindowCount { get; set; }
    public int SupportWindowCount { get; set; }
    public int FightWindowCount { get; set; }
    public int FightBurstWindowCount { get; set; }
    public int FightConversionWindowCount { get; set; }
    public int FightStabilizeWindowCount { get; set; }
    public int FightPressureWindowCount { get; set; }
    public int FightMomentumWindowCount { get; set; }
    public int FightCollapseWindowCount { get; set; }
    public int OffensiveBurstWindowCount { get; set; }
    public int OffensiveConversionWindowCount { get; set; }
    public int ControlConversionWindowCount { get; set; }
    public int SupportStabilizeWindowCount { get; set; }
    public int SupportPressureWindowCount { get; set; }
    public bool HasControlCrowdControl { get; set; }
    public bool HasControlConditions { get; set; }
    public bool HasHealingSupport { get; set; }
    public bool HasCleanseSupport { get; set; }
    public bool HasOffensiveBoonSupport { get; set; }
    public bool HasDefensiveBoonSupport { get; set; }
    public bool HasDefensiveConditions { get; set; }
    public bool HasResurrectSupport { get; set; }
}

internal class WvwSummaryPlayerWindowContribution
{
    public double DamagePerSecond { get; set; }
    public double OffensiveConditionPressurePerSecond { get; set; }
    public double StripPerSecond { get; set; }
    public double ControlConditionPressurePerSecond { get; set; }
    public double HealingPerSecond { get; set; }
    public double BarrierPerSecond { get; set; }
    public double CleansePerSecond { get; set; }
    public double OffensiveBoonSupportPerSecond { get; set; }
    public double DefensiveBoonSupportPerSecond { get; set; }
    public double DefensiveConditionPressurePerSecond { get; set; }
    public double ResurrectCount { get; set; }
    public double EffectiveCrowdControlPerSecond { get; set; }
    public double EffectiveCrowdControlDurationPerSecond { get; set; }
    public double TopTargetContribution { get; set; }
    public double InPositionRate { get; set; }
    public bool HasPositioningData { get; set; }
}

internal class WvwSummaryStandoutWindow
{
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public string SourceCategory { get; set; } = "";
    public double Weight { get; set; }
    public double OffenseWeight { get; set; }
    public double ControlWeight { get; set; }
    public double SupportWeight { get; set; }
    public double FightWeight { get; set; }
    public double DisciplineWeight { get; set; }
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

internal readonly record struct WvwSummaryExecutionWindow(long Start, long End);
