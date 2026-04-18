using System.Numerics;
using System.Globalization;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SkillIDs;
using static GW2EIEvtcParser.SpeciesIDs;
using Segment = GW2EIEvtcParser.EIData.GenericSegment<double>;

namespace GW2EIBuilders.HtmlModels;

internal class CombatReplayAnalysisDto
{
    public int Lookback { get; set; }
    public bool HasHealingData { get; set; }
    public long[] Times { get; set; } = [];
    public CombatReplayTeamAnalysisDto Squad { get; set; } = new();
    public CombatReplayTeamAnalysisDto Enemy { get; set; } = new();
    public CombatReplayThreatBoonAnalysisDto ThreatBoons { get; set; } = new();
    public CombatReplayPositioningAnalysisDto Positioning { get; set; } = new();
    public CombatReplayEventAnalysisDto Events { get; set; } = new();
    public Dictionary<int, CombatReplayPlayerEvaluationDto> PlayerEvaluations { get; set; } = [];
}

internal class CombatReplayTeamAnalysisDto
{
    public string Label { get; set; } = "";
    public List<CombatReplayAnalysisBurstSummaryDto> TopBursts { get; set; } = [];
    public long[] Damage { get; set; } = [];
    public int[] Downs { get; set; } = [];
    public int[] DownsTotal { get; set; } = [];
    public int[] Kills { get; set; } = [];
    public int[] KillsTotal { get; set; } = [];
    public string[] BurstStrength { get; set; } = [];
    public int[][] TopDamageActorIds { get; set; } = [];
    public long[][] TopDamageValues { get; set; } = [];
    public int[] Strips { get; set; } = [];
    public int[] StripPeakGap { get; set; } = [];
    public bool[] StripSynced { get; set; } = [];
    public int[][] TopStripActorIds { get; set; } = [];
    public int[][] TopStripValues { get; set; } = [];
    public int[] TopTargetIds { get; set; } = [];
    public double[] TopTargetShare { get; set; } = [];
    public double[] TopThreeTargetShare { get; set; } = [];
    public int[] TopTargetContributors { get; set; } = [];
    public bool[] Focused { get; set; } = [];
    public int[] TargetSaturationCount { get; set; } = [];
    public string[] TargetSaturation { get; set; } = [];
    public Dictionary<int, CombatReplayAnalysisAttackerTimelineDto> Attackers { get; set; } = [];
    public Dictionary<int, CombatReplayAnalysisTargetTimelineDto> Targets { get; set; } = [];
}

internal class CombatReplayAnalysisBurstSummaryDto
{
    public long Time { get; set; }
    public long Damage { get; set; }
    public int Strips { get; set; }
    public int Downs { get; set; }
    public int DownsTotal { get; set; }
    public int Kills { get; set; }
    public int KillsTotal { get; set; }
}

internal class CombatReplayAnalysisAttackerTimelineDto
{
    public long[] Damage { get; set; } = [];
    public long[] Healing { get; set; } = [];
    public long[] Barrier { get; set; } = [];
    public int[] Cleanses { get; set; } = [];
    public int[] Strips { get; set; } = [];
    public double[] TopTargetContribution { get; set; } = [];
    public int[] TargetsHit { get; set; } = [];
    public int[] NearbyTargets { get; set; } = [];
}

internal class CombatReplayAnalysisTargetTimelineDto
{
    public long[] DamageTaken { get; set; } = [];
    public int[] StripsTaken { get; set; } = [];
    public int[] Attackers { get; set; } = [];
    public int[] NearbyAllies { get; set; } = [];
    public int[][] TopAttackerIds { get; set; } = [];
    public long[][] TopAttackerDamage { get; set; } = [];
}

internal class CombatReplayThreatBoonAnalysisDto
{
    public string Label { get; set; } = "";
    public int ThreatRange { get; set; }
    public int[] ThreatenedPlayerCount { get; set; } = [];
    public List<CombatReplayThreatBoonTimelineDto> Boons { get; set; } = [];
    public Dictionary<int, CombatReplayThreatPlayerTimelineDto> Players { get; set; } = [];
}

internal class CombatReplayThreatBoonTimelineDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public bool StackBased { get; set; }
    public bool TracksOverapplication { get; set; }
    public int OverapplicationThreshold { get; set; }
    public double[] CurrentCoverage { get; set; } = [];
    public double[] RunningCoverage { get; set; } = [];
    public double[] CurrentAverageStacks { get; set; } = [];
    public double[] CurrentOverapplication { get; set; } = [];
    public double[] RunningOverapplication { get; set; } = [];
    public double SummaryCoverage { get; set; }
    public double SummaryAverageStacks { get; set; }
    public double SummaryOverapplication { get; set; }
}

internal class CombatReplayThreatPlayerTimelineDto
{
    public int[] NearbyEnemies { get; set; } = [];
    public bool[] Threatened { get; set; } = [];
    public long[] RunningThreatTime { get; set; } = [];
    public List<CombatReplayThreatPlayerBoonTimelineDto> Boons { get; set; } = [];
}

internal class CombatReplayThreatPlayerBoonTimelineDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public bool StackBased { get; set; }
    public bool TracksOverapplication { get; set; }
    public int OverapplicationThreshold { get; set; }
    public int[] CurrentStacks { get; set; } = [];
    public double[] RunningCoverage { get; set; } = [];
    public double[] RunningOverapplication { get; set; } = [];
}

internal class CombatReplayPositioningAnalysisDto
{
    public bool HasCommander { get; set; }
    public int CommanderId { get; set; }
    public string CommanderName { get; set; } = "";
    public int DesiredCommanderDistance { get; set; }
    public int MingledCommanderDistance { get; set; }
    public int IgnoreCommanderDistance { get; set; }
    public int EngageRange { get; set; }
    public int MingledRange { get; set; }
    public int MingledEnemyThreshold { get; set; }
    public int EnemyCountThreshold { get; set; }
    public int OverextendedPlayerThreshold { get; set; }
    public int[] EngagedEnemyCount { get; set; } = [];
    public int[] EnemiesNearCommanderCount { get; set; } = [];
    public bool[] Mingled { get; set; } = [];
    public int[] EligiblePlayerCount { get; set; } = [];
    public int[] InPositionCount { get; set; } = [];
    public int[] OutOfPositionCount { get; set; } = [];
    public int[] TooFarCount { get; set; } = [];
    public int[] OverextendedCount { get; set; } = [];
    public int[] LateralRiskCount { get; set; } = [];
    public double[] InPositionRate { get; set; } = [];
    public double SummaryInPositionRate { get; set; }
    public double SummaryTooFarRate { get; set; }
    public double SummaryOverextendedRate { get; set; }
    public double SummaryLateralRiskRate { get; set; }
    public long SummaryEvaluatedSamples { get; set; }
    public Dictionary<int, CombatReplayPositioningPlayerTimelineDto> Players { get; set; } = [];
}

internal class CombatReplayPositioningPlayerTimelineDto
{
    public bool[] Eligible { get; set; } = [];
    public bool[] InPosition { get; set; } = [];
    public bool[] TooFar { get; set; } = [];
    public bool[] Overextended { get; set; } = [];
    public bool[] LateralRisk { get; set; } = [];
    public int[] DistanceToCommander { get; set; } = [];
    public int[] EnemiesCloserThanCommander { get; set; } = [];
    public int[] EnemiesAheadOfCommander { get; set; } = [];
    public double[] RunningInPositionRate { get; set; } = [];
    public double[] RunningTooFarRate { get; set; } = [];
    public double[] RunningOverextendedRate { get; set; } = [];
    public double[] RunningLateralRiskRate { get; set; } = [];
}

internal class CombatReplayEventAnalysisDto
{
    public CombatReplayBarrierSaveAnalysisDto BarrierSaves { get; set; } = new();
    public CombatReplayConditionConversionAnalysisDto ConditionConversions { get; set; } = new();
}

internal class CombatReplayBarrierSaveAnalysisDto
{
    public bool Available { get; set; }
    public int TotalEvents { get; set; }
    public List<CombatReplayEventActorSummaryDto> SavedPlayers { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> Providers { get; set; } = [];
    public List<CombatReplayBarrierSaveEventDto> Events { get; set; } = [];
}

internal class CombatReplayConditionConversionAnalysisDto
{
    public int TotalEvents { get; set; }
    public int ConvertedEvents { get; set; }
    public double TotalBurningPressure { get; set; }
    public double TotalPressure { get; set; }
    public List<CombatReplayEventActorSummaryDto> Providers { get; set; } = [];
    public List<CombatReplayConditionConversionEventDto> Events { get; set; } = [];
}

internal class CombatReplayEventActorSummaryDto
{
    public int? ActorId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public double Amount { get; set; }
}

internal class CombatReplayEventContributionDto
{
    public int? ActorId { get; set; }
    public long? BuffId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public double Amount { get; set; }
    public double Percent { get; set; }
    public List<CombatReplayEventContributionDto> Details { get; set; } = [];
}

internal class CombatReplayEventTimelineEntryDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Secondary { get; set; } = "";
}

internal class CombatReplayBarrierSaveEventDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public int SavedPlayerId { get; set; }
    public string SavedPlayerName { get; set; } = "";
    public string SavedPlayerIcon { get; set; } = "";
    public int TotalBarrier { get; set; }
    public int BarrierAbsorbed { get; set; }
    public double LowestHealthPercent { get; set; }
    public int ApproxHealthStart { get; set; }
    public int ApproxBarrierStart { get; set; }
    public double HealthPercentStart { get; set; }
    public double BarrierPercentStart { get; set; }
    public string ProviderSummary { get; set; } = "";
    public List<CombatReplayEventContributionDto> Providers { get; set; } = [];
    public List<CombatReplayEventTimelineEntryDto> IncomingDamage { get; set; } = [];
}

internal class CombatReplayConditionConversionEventDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public int TargetId { get; set; }
    public string TargetName { get; set; } = "";
    public string TargetIcon { get; set; } = "";
    public string Outcome { get; set; } = "";
    public long? ConversionTime { get; set; }
    public string ConversionTimeLabel { get; set; } = "";
    public double TotalPressure { get; set; }
    public double BurningPressure { get; set; }
    public string TopConditionName { get; set; } = "";
    public string TopConditionIcon { get; set; } = "";
    public string TopContributorSummary { get; set; } = "";
    public List<CombatReplayEventContributionDto> Conditions { get; set; } = [];
    public List<CombatReplayEventContributionDto> Providers { get; set; } = [];
}

internal class CombatReplayPlayerEvaluationDto
{
    public string ContributionProfile { get; set; } = "";
    public string KeyContributionSummary { get; set; } = "";
    public List<CombatReplayPlayerRoleMixEntryDto> RoleMix { get; set; } = [];
    public List<CombatReplayPlayerEvaluationAreaDto> Areas { get; set; } = [];
}

internal class CombatReplayPlayerRoleMixEntryDto
{
    public string Label { get; set; } = "";
    public double Percent { get; set; }
}

internal class CombatReplayPlayerEvaluationAreaDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Detail { get; set; } = "";
    public bool IsInteractive { get; set; }
    public string DrilldownTitle { get; set; } = "";
    public string DrilldownSubtitle { get; set; } = "";
    public List<CombatReplayPlayerEvaluationDetailSectionDto> DetailSections { get; set; } = [];
}

internal class CombatReplayPlayerEvaluationDetailSectionDto
{
    public string Label { get; set; } = "";
    public List<CombatReplayPlayerEvaluationDetailEntryDto> Entries { get; set; } = [];
}

internal class CombatReplayPlayerEvaluationDetailEntryDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Secondary { get; set; } = "";
}

internal class CombatReplayPlayerEvaluationAggregate
{
    public int PlayerId { get; set; }
    public long DamageTotal { get; set; }
    public double AverageTopTargetContribution { get; set; }
    public double OffensiveConditionPressure { get; set; }
    public double ControlConditionPressure { get; set; }
    public int StripsTotal { get; set; }
    public long HealingTotal { get; set; }
    public long BarrierTotal { get; set; }
    public int CleansesTotal { get; set; }
    public int ResurrectsTotal { get; set; }
    public double OffensiveBoonSupport { get; set; }
    public double DefensiveBoonSupport { get; set; }
    public double DefensiveConditionPressure { get; set; }
    public int EffectiveCrowdControlCount { get; set; }
    public double EffectiveCrowdControlDuration { get; set; }
    public int BurstContributionWindows { get; set; }
    public int ConversionContributionWindows { get; set; }
    public int ControlContributionWindows { get; set; }
    public int DefensiveSupportWindows { get; set; }
    public bool HasPositioningData { get; set; }
    public int PositioningSamples { get; set; }
    public double InPositionRate { get; set; }
    public double TooFarRate { get; set; }
    public double OverextendedRate { get; set; }
    public double LateralRiskRate { get; set; }
    public List<CombatReplayPlayerEvaluationDetailEntryDto> EffectiveCrowdControlSources { get; set; } = [];
    public List<CombatReplayPlayerEvaluationDetailEntryDto> ControlConditionSources { get; set; } = [];
}

internal class CombatReplayPlayerEvaluationMaximums
{
    public long DamageTotal { get; set; }
    public double AverageTopTargetContribution { get; set; }
    public double OffensiveConditionPressure { get; set; }
    public double ControlConditionPressure { get; set; }
    public int StripsTotal { get; set; }
    public long HealingTotal { get; set; }
    public long BarrierTotal { get; set; }
    public int CleansesTotal { get; set; }
    public int ResurrectsTotal { get; set; }
    public double OffensiveBoonSupport { get; set; }
    public double DefensiveBoonSupport { get; set; }
    public double DefensiveConditionPressure { get; set; }
    public int EffectiveCrowdControlCount { get; set; }
    public double EffectiveCrowdControlDuration { get; set; }
    public int BurstContributionWindows { get; set; }
    public int ConversionContributionWindows { get; set; }
    public int ControlContributionWindows { get; set; }
    public int DefensiveSupportWindows { get; set; }
}

internal static class CombatReplayAnalysisBuilder
{
    private const int LookbackWindow = 3000;
    private const int BucketSize = 1000;
    private const int SaveEventMergeWindow = 500;
    private const int SaveEventLookaheadWindow = 1500;
    private const double MeaningfulContributionThreshold = 0.10;
    private const float RangeThreshold = 1200.0f;
    private static readonly PositioningCriteria PositioningSettings = new(
        DesiredCommanderDistance: 240.0f,
        MingledCommanderDistance: 180.0f,
        IgnoreCommanderDistance: 3000.0f,
        EngageRange: 1200.0f,
        MingledRange: 100.0f,
        MingledEnemyThreshold: 5,
        EnemyCountThreshold: 5,
        OverextendedPlayerThreshold: 5);
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
    private static readonly IReadOnlyList<long> DefensiveConditionBuffIds =
    [
        Weakness,
        Blind,
        Chilled,
    ];
    private static readonly IReadOnlyList<long> ConditionConversionDisplayBuffIds =
    [
        Burning,
        Poison,
        Bleeding,
        Torment,
        Confusion,
        Vulnerability,
    ];

    private readonly record struct DamageRecord(long Time, int TargetUniqueId, int AttackerUniqueId, int Damage, bool HasDowned, bool HasKilled);
    private readonly record struct HealingRecord(long Time, int AttackerUniqueId, int Healing);
    private readonly record struct BarrierRecord(long Time, int AttackerUniqueId, int Barrier);
    private readonly record struct CleanseRecord(long Time, int AttackerUniqueId);
    private readonly record struct StripRecord(long Time, int TargetUniqueId, int AttackerUniqueId);
    private readonly record struct EvaluationWindow(long Start, long End);
    private readonly record struct BarrierSaveCandidate(
        long StartTime,
        long EndTime,
        SingleActor SavedPlayer,
        int TotalBarrier,
        int BarrierAbsorbed,
        int ApproxHealthStart,
        int ApproxBarrierStart,
        double HealthPercentStart,
        double BarrierPercentStart,
        double LowestHealthPercent,
        IReadOnlyList<CombatReplayEventContributionDto> Providers,
        IReadOnlyList<CombatReplayEventTimelineEntryDto> IncomingDamage);
    private readonly record struct TeamActorContext(
        IReadOnlyList<SingleActor> Attackers,
        IReadOnlyList<SingleActor> Targets,
        IReadOnlyDictionary<AgentItem, int> AttackerIdsByAgent,
        string Label);

    public static CombatReplayAnalysisDto? Build(ParsedEvtcLog log)
    {
        if (!log.CanCombatReplay ||
            log.LogData.Logic.ParseMode != LogLogic.ParseModeEnum.WvW ||
            log.LogData.Logic.Extension != "detailed_wvw")
        {
            return null;
        }

        var squadPlayers = log.PlayerList.Where(player => !player.IsFakeActor).Cast<SingleActor>().ToList();
        var hostileTargets = log.LogData.Logic.Targets
            .Where(target =>
                !target.IsFakeActor &&
                target.AgentItem.Type == AgentItem.AgentType.NonSquadPlayer &&
                !target.IsSpecies(TargetID.WorldVersusWorld))
            .ToList();
        if (squadPlayers.Count == 0 || hostileTargets.Count == 0)
        {
            return null;
        }

        var pollingRate = ParserHelper.CombatReplayPollingRate;
        var times = BuildTimes(log.LogData.LogEnd, pollingRate);
        var snapshotCount = times.Length;
        var boonIDs = new HashSet<long>(log.Buffs.BuffsByClassification[Buff.BuffClassification.Boon].Select(buff => buff.ID));

        var squadContext = BuildContext(
            squadPlayers,
            hostileTargets,
            "Our Squad");
        var enemyContext = BuildContext(
            hostileTargets,
            squadPlayers,
            "Enemy Team");
        var squadAnalysis = BuildTeamAnalysis(log, squadContext, boonIDs, times, snapshotCount);
        var enemyAnalysis = BuildTeamAnalysis(log, enemyContext, boonIDs, times, snapshotCount);
        Player? commander = log.PlayerList.FirstOrDefault(player => !player.IsFakeActor && player.IsCommander(log));
        var threatAnalysis = BuildThreatBoonAnalysis(log, squadPlayers, times, pollingRate, squadAnalysis);
        var positioningAnalysis = BuildPositioningAnalysis(log, squadPlayers, hostileTargets, commander, times);

        return new CombatReplayAnalysisDto
        {
            Lookback = LookbackWindow,
            HasHealingData = log.CombatData.HasEXTHealing,
            Times = times,
            Squad = squadAnalysis,
            Enemy = enemyAnalysis,
            ThreatBoons = threatAnalysis,
            Positioning = positioningAnalysis,
            Events = BuildEventAnalysis(log, squadPlayers, hostileTargets),
            PlayerEvaluations = BuildPlayerEvaluations(log, squadPlayers, hostileTargets, squadAnalysis, enemyAnalysis, positioningAnalysis, times),
        };
    }

    private static TeamActorContext BuildContext(IReadOnlyList<SingleActor> attackers, IReadOnlyList<SingleActor> targets, string label)
    {
        return new TeamActorContext(
            attackers,
            targets,
            attackers.ToDictionary(actor => actor.AgentItem.GetFinalMaster(), actor => actor.UniqueID),
            label);
    }

    private static CombatReplayTeamAnalysisDto BuildTeamAnalysis(
        ParsedEvtcLog log,
        TeamActorContext context,
        IReadOnlySet<long> boonIDs,
        long[] times,
        int snapshotCount)
    {
        var result = new CombatReplayTeamAnalysisDto
        {
            Label = context.Label,
            Damage = new long[snapshotCount],
            Downs = new int[snapshotCount],
            DownsTotal = new int[snapshotCount],
            Kills = new int[snapshotCount],
            KillsTotal = new int[snapshotCount],
            BurstStrength = new string[snapshotCount],
            TopDamageActorIds = new int[snapshotCount][],
            TopDamageValues = new long[snapshotCount][],
            Strips = new int[snapshotCount],
            StripPeakGap = new int[snapshotCount],
            StripSynced = new bool[snapshotCount],
            TopStripActorIds = new int[snapshotCount][],
            TopStripValues = new int[snapshotCount][],
            TopTargetIds = new int[snapshotCount],
            TopTargetShare = new double[snapshotCount],
            TopThreeTargetShare = new double[snapshotCount],
            TopTargetContributors = new int[snapshotCount],
            Focused = new bool[snapshotCount],
            TargetSaturationCount = new int[snapshotCount],
            TargetSaturation = new string[snapshotCount],
            Attackers = context.Attackers.ToDictionary(
                attacker => attacker.UniqueID,
                _ => new CombatReplayAnalysisAttackerTimelineDto
                {
                    Damage = new long[snapshotCount],
                    Healing = new long[snapshotCount],
                    Barrier = new long[snapshotCount],
                    Cleanses = new int[snapshotCount],
                    Strips = new int[snapshotCount],
                    TopTargetContribution = new double[snapshotCount],
                    TargetsHit = new int[snapshotCount],
                    NearbyTargets = new int[snapshotCount],
                }),
            Targets = context.Targets.ToDictionary(
                target => target.UniqueID,
                _ => new CombatReplayAnalysisTargetTimelineDto
                {
                    DamageTaken = new long[snapshotCount],
                    StripsTaken = new int[snapshotCount],
                    Attackers = new int[snapshotCount],
                    NearbyAllies = new int[snapshotCount],
                    TopAttackerIds = new int[snapshotCount][],
                    TopAttackerDamage = new long[snapshotCount][],
                }),
        };

        var damageRecords = BuildDamageRecords(log, context);
        var healingRecords = BuildHealingRecords(log, context);
        var barrierRecords = BuildBarrierRecords(log, context);
        var cleanseRecords = BuildCleanseRecords(log, context);
        var stripRecords = BuildStripRecords(log, context, boonIDs);

        var damageIndexStart = 0;
        var damageIndexEnd = 0;
        var cumulativeDamageIndex = 0;
        var healingIndexStart = 0;
        var healingIndexEnd = 0;
        var barrierIndexStart = 0;
        var barrierIndexEnd = 0;
        var cleanseIndexStart = 0;
        var cleanseIndexEnd = 0;
        var stripIndexStart = 0;
        var stripIndexEnd = 0;
        var totalDowns = 0;
        var totalKills = 0;

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            var time = times[snapshotIndex];
            var windowStart = Math.Max(0, time - LookbackWindow);

            while (damageIndexStart < damageRecords.Count && damageRecords[damageIndexStart].Time < windowStart)
            {
                damageIndexStart++;
            }
            while (damageIndexEnd < damageRecords.Count && damageRecords[damageIndexEnd].Time <= time)
            {
                damageIndexEnd++;
            }
            while (cumulativeDamageIndex < damageIndexEnd)
            {
                var damage = damageRecords[cumulativeDamageIndex];
                if (damage.HasDowned)
                {
                    totalDowns++;
                }
                if (damage.HasKilled)
                {
                    totalKills++;
                }
                cumulativeDamageIndex++;
            }
            while (healingIndexStart < healingRecords.Count && healingRecords[healingIndexStart].Time < windowStart)
            {
                healingIndexStart++;
            }
            while (healingIndexEnd < healingRecords.Count && healingRecords[healingIndexEnd].Time <= time)
            {
                healingIndexEnd++;
            }
            while (barrierIndexStart < barrierRecords.Count && barrierRecords[barrierIndexStart].Time < windowStart)
            {
                barrierIndexStart++;
            }
            while (barrierIndexEnd < barrierRecords.Count && barrierRecords[barrierIndexEnd].Time <= time)
            {
                barrierIndexEnd++;
            }
            while (cleanseIndexStart < cleanseRecords.Count && cleanseRecords[cleanseIndexStart].Time < windowStart)
            {
                cleanseIndexStart++;
            }
            while (cleanseIndexEnd < cleanseRecords.Count && cleanseRecords[cleanseIndexEnd].Time <= time)
            {
                cleanseIndexEnd++;
            }
            while (stripIndexStart < stripRecords.Count && stripRecords[stripIndexStart].Time < windowStart)
            {
                stripIndexStart++;
            }
            while (stripIndexEnd < stripRecords.Count && stripRecords[stripIndexEnd].Time <= time)
            {
                stripIndexEnd++;
            }

            var damageByAttacker = new Dictionary<int, long>();
            var damageByTarget = new Dictionary<int, long>();
            var damageByTargetByAttacker = new Dictionary<int, Dictionary<int, long>>();
            var attackerTargetsHit = new Dictionary<int, HashSet<int>>();
            var targetAttackers = new Dictionary<int, HashSet<int>>();
            var damageBuckets = new long[3];
            long totalDamage = 0;
            var downs = 0;
            var kills = 0;

            for (var index = damageIndexStart; index < damageIndexEnd; index++)
            {
                var damage = damageRecords[index];
                totalDamage += damage.Damage;
                if (damage.HasDowned)
                {
                    downs++;
                }
                if (damage.HasKilled)
                {
                    kills++;
                }

                damageByAttacker[damage.AttackerUniqueId] = damageByAttacker.GetValueOrDefault(damage.AttackerUniqueId) + damage.Damage;
                damageByTarget[damage.TargetUniqueId] = damageByTarget.GetValueOrDefault(damage.TargetUniqueId) + damage.Damage;

                if (!damageByTargetByAttacker.TryGetValue(damage.TargetUniqueId, out var attackerDamageOnTarget))
                {
                    attackerDamageOnTarget = [];
                    damageByTargetByAttacker[damage.TargetUniqueId] = attackerDamageOnTarget;
                }
                attackerDamageOnTarget[damage.AttackerUniqueId] = attackerDamageOnTarget.GetValueOrDefault(damage.AttackerUniqueId) + damage.Damage;

                if (!attackerTargetsHit.TryGetValue(damage.AttackerUniqueId, out var hitTargets))
                {
                    hitTargets = [];
                    attackerTargetsHit[damage.AttackerUniqueId] = hitTargets;
                }
                hitTargets.Add(damage.TargetUniqueId);

                if (!targetAttackers.TryGetValue(damage.TargetUniqueId, out var attackers))
                {
                    attackers = [];
                    targetAttackers[damage.TargetUniqueId] = attackers;
                }
                attackers.Add(damage.AttackerUniqueId);

                var bucketIndex = ComputeBucketIndex(damage.Time, windowStart);
                damageBuckets[bucketIndex] += damage.Damage;
            }

            var stripByAttacker = new Dictionary<int, int>();
            var stripByTarget = new Dictionary<int, int>();
            var stripBuckets = new int[3];
            var stripCount = 0;
            var healingByAttacker = new Dictionary<int, long>();
            var barrierByAttacker = new Dictionary<int, long>();
            var cleanseByAttacker = new Dictionary<int, int>();

            for (var index = healingIndexStart; index < healingIndexEnd; index++)
            {
                var healing = healingRecords[index];
                healingByAttacker[healing.AttackerUniqueId] = healingByAttacker.GetValueOrDefault(healing.AttackerUniqueId) + healing.Healing;
            }
            for (var index = barrierIndexStart; index < barrierIndexEnd; index++)
            {
                var barrier = barrierRecords[index];
                barrierByAttacker[barrier.AttackerUniqueId] = barrierByAttacker.GetValueOrDefault(barrier.AttackerUniqueId) + barrier.Barrier;
            }
            for (var index = cleanseIndexStart; index < cleanseIndexEnd; index++)
            {
                var cleanse = cleanseRecords[index];
                cleanseByAttacker[cleanse.AttackerUniqueId] = cleanseByAttacker.GetValueOrDefault(cleanse.AttackerUniqueId) + 1;
            }

            for (var index = stripIndexStart; index < stripIndexEnd; index++)
            {
                var strip = stripRecords[index];
                stripCount++;
                stripByAttacker[strip.AttackerUniqueId] = stripByAttacker.GetValueOrDefault(strip.AttackerUniqueId) + 1;
                stripByTarget[strip.TargetUniqueId] = stripByTarget.GetValueOrDefault(strip.TargetUniqueId) + 1;
                var bucketIndex = ComputeBucketIndex(strip.Time, windowStart);
                stripBuckets[bucketIndex]++;
            }

            var topTargetId = 0;
            long topTargetDamage = 0;
            long topThreeDamage = 0;
            if (damageByTarget.Count > 0)
            {
                var orderedTargets = damageByTarget.OrderByDescending(pair => pair.Value).ToList();
                topTargetId = orderedTargets[0].Key;
                topTargetDamage = orderedTargets[0].Value;
                topThreeDamage = orderedTargets.Take(3).Sum(pair => pair.Value);
            }

            var contributorCount = 0;
            if (topTargetId != 0 && totalDamage > 0 && damageByTargetByAttacker.TryGetValue(topTargetId, out var contributors))
            {
                contributorCount = contributors.Count(pair => pair.Value >= totalDamage * 0.05);
            }

            var effectiveTargetCount = totalDamage > 0
                ? damageByTarget.Values.Count(value => value >= totalDamage * 0.02)
                : 0;

            result.Damage[snapshotIndex] = totalDamage;
            result.Downs[snapshotIndex] = downs;
            result.DownsTotal[snapshotIndex] = totalDowns;
            result.Kills[snapshotIndex] = kills;
            result.KillsTotal[snapshotIndex] = totalKills;
            var topDamageAttackers = damageByAttacker
                .OrderByDescending(pair => pair.Value)
                .Take(5)
                .ToArray();
            result.TopDamageActorIds[snapshotIndex] = [.. topDamageAttackers.Select(pair => pair.Key)];
            result.TopDamageValues[snapshotIndex] = [.. topDamageAttackers.Select(pair => pair.Value)];
            result.Strips[snapshotIndex] = stripCount;
            var topStripAttackers = stripByAttacker
                .OrderByDescending(pair => pair.Value)
                .Take(5)
                .ToArray();
            result.TopStripActorIds[snapshotIndex] = [.. topStripAttackers.Select(pair => pair.Key)];
            result.TopStripValues[snapshotIndex] = [.. topStripAttackers.Select(pair => pair.Value)];
            result.TopTargetIds[snapshotIndex] = topTargetId;
            result.TopTargetShare[snapshotIndex] = totalDamage > 0 ? Math.Round(topTargetDamage * 100.0 / totalDamage, 1) : 0;
            result.TopThreeTargetShare[snapshotIndex] = totalDamage > 0 ? Math.Round(topThreeDamage * 100.0 / totalDamage, 1) : 0;
            result.TopTargetContributors[snapshotIndex] = contributorCount;
            result.Focused[snapshotIndex] = totalDamage > 0 && result.TopTargetShare[snapshotIndex] >= 50.0 && contributorCount >= 3;
            result.TargetSaturationCount[snapshotIndex] = effectiveTargetCount;
            result.TargetSaturation[snapshotIndex] = effectiveTargetCount switch
            {
                < 3 => "under-saturated",
                <= 5 => "optimal",
                _ => "over-spread",
            };

            var damagePeakBucket = GetPeakBucketIndex(damageBuckets);
            var stripPeakBucket = GetPeakBucketIndex(stripBuckets);
            result.StripPeakGap[snapshotIndex] = Math.Abs(damagePeakBucket - stripPeakBucket) * BucketSize;

            foreach (var attacker in context.Attackers)
            {
                var timeline = result.Attackers[attacker.UniqueID];
                timeline.Damage[snapshotIndex] = damageByAttacker.GetValueOrDefault(attacker.UniqueID);
                timeline.Healing[snapshotIndex] = healingByAttacker.GetValueOrDefault(attacker.UniqueID);
                timeline.Barrier[snapshotIndex] = barrierByAttacker.GetValueOrDefault(attacker.UniqueID);
                timeline.Cleanses[snapshotIndex] = cleanseByAttacker.GetValueOrDefault(attacker.UniqueID);
                timeline.Strips[snapshotIndex] = stripByAttacker.GetValueOrDefault(attacker.UniqueID);
                timeline.TargetsHit[snapshotIndex] = attackerTargetsHit.TryGetValue(attacker.UniqueID, out var hitTargets) ? hitTargets.Count : 0;

                if (topTargetId != 0 &&
                    topTargetDamage > 0 &&
                    damageByTargetByAttacker.TryGetValue(topTargetId, out var attackerContribution) &&
                    attackerContribution.TryGetValue(attacker.UniqueID, out var contributedDamage))
                {
                    timeline.TopTargetContribution[snapshotIndex] = Math.Round(contributedDamage * 100.0 / topTargetDamage, 1);
                }
            }

            foreach (var target in context.Targets)
            {
                var timeline = result.Targets[target.UniqueID];
                timeline.DamageTaken[snapshotIndex] = damageByTarget.GetValueOrDefault(target.UniqueID);
                timeline.StripsTaken[snapshotIndex] = stripByTarget.GetValueOrDefault(target.UniqueID);
                timeline.Attackers[snapshotIndex] = targetAttackers.TryGetValue(target.UniqueID, out var attackers) ? attackers.Count : 0;

                if (damageByTargetByAttacker.TryGetValue(target.UniqueID, out var attackerDamage))
                {
                    var topAttackers = attackerDamage
                        .OrderByDescending(pair => pair.Value)
                        .Take(3)
                        .ToArray();
                    timeline.TopAttackerIds[snapshotIndex] = [.. topAttackers.Select(pair => pair.Key)];
                    timeline.TopAttackerDamage[snapshotIndex] = [.. topAttackers.Select(pair => pair.Value)];
                }
                else
                {
                    timeline.TopAttackerIds[snapshotIndex] = [];
                    timeline.TopAttackerDamage[snapshotIndex] = [];
                }
            }

            PopulateRangeCounts(log, time, context, result, snapshotIndex);
        }

        var burstLowThreshold = GetPercentile(result.Damage, 0.25);
        var burstHighThreshold = GetPercentile(result.Damage, 0.75);
        var stripSyncThreshold = GetPercentile(result.Strips, 0.75);

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            var damage = result.Damage[snapshotIndex];
            result.BurstStrength[snapshotIndex] = damage <= burstLowThreshold
                ? "weak"
                : damage >= burstHighThreshold
                    ? "strong"
                    : "normal";

            result.StripSynced[snapshotIndex] =
                result.Strips[snapshotIndex] > 0 &&
                result.Strips[snapshotIndex] >= stripSyncThreshold &&
                result.StripPeakGap[snapshotIndex] <= BucketSize;
        }

        result.TopBursts = BuildTopBursts(result, times);
        return result;
    }

    private static CombatReplayThreatBoonAnalysisDto BuildThreatBoonAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        long[] times,
        int pollingRate,
        CombatReplayTeamAnalysisDto squadAnalysis)
    {
        var boonDefinitions = new[]
        {
            CreateThreatBoonDefinition(log, Stability, true, true, 10),
            CreateThreatBoonDefinition(log, Protection, false),
            CreateThreatBoonDefinition(log, Resolution, false),
            CreateThreatBoonDefinition(log, Resistance, false),
            CreateThreatBoonDefinition(log, Aegis, false),
            CreateThreatBoonDefinition(log, Might, true),
            CreateThreatBoonDefinition(log, Fury, false),
            CreateThreatBoonDefinition(log, Quickness, false),
        };

        var snapshotCount = times.Length;
        var result = new CombatReplayThreatBoonAnalysisDto
        {
            Label = "Our Squad",
            ThreatRange = (int)RangeThreshold,
            ThreatenedPlayerCount = new int[snapshotCount],
            Boons = [.. boonDefinitions.Select(definition => new CombatReplayThreatBoonTimelineDto
            {
                Id = definition.Id,
                Name = definition.Name,
                Icon = definition.Icon,
                StackBased = definition.StackBased,
                TracksOverapplication = definition.TracksOverapplication,
                OverapplicationThreshold = definition.OverapplicationThreshold,
                CurrentCoverage = new double[snapshotCount],
                RunningCoverage = new double[snapshotCount],
                CurrentAverageStacks = new double[snapshotCount],
                CurrentOverapplication = new double[snapshotCount],
                RunningOverapplication = new double[snapshotCount],
            })],
            Players = squadPlayers.ToDictionary(
                player => player.UniqueID,
                player => new CombatReplayThreatPlayerTimelineDto
                {
                    NearbyEnemies = squadAnalysis.Attackers[player.UniqueID].NearbyTargets,
                    Threatened = new bool[snapshotCount],
                    RunningThreatTime = new long[snapshotCount],
                    Boons = [.. boonDefinitions.Select(definition => new CombatReplayThreatPlayerBoonTimelineDto
                    {
                        Id = definition.Id,
                        Name = definition.Name,
                        Icon = definition.Icon,
                        StackBased = definition.StackBased,
                        TracksOverapplication = definition.TracksOverapplication,
                        OverapplicationThreshold = definition.OverapplicationThreshold,
                        CurrentStacks = new int[snapshotCount],
                        RunningCoverage = new double[snapshotCount],
                        RunningOverapplication = new double[snapshotCount],
                    })],
                }),
        };

        var playerThreatSamples = squadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var playerThreatTime = squadPlayers.ToDictionary(player => player.UniqueID, _ => 0L);
        var playerActiveThreatSamples = squadPlayers.ToDictionary(
            player => player.UniqueID,
            _ => boonDefinitions.ToDictionary(definition => definition.Id, _ => 0));
        var playerOverappliedThreatSamples = squadPlayers.ToDictionary(
            player => player.UniqueID,
            _ => boonDefinitions.ToDictionary(definition => definition.Id, _ => 0));
        var squadThreatSamples = 0;
        var squadActiveThreatSamples = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);
        var squadThreatStackSums = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);
        var squadOverappliedThreatSamples = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            var threatenedNow = 0;
            var activeNow = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);
            var stackSumsNow = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);
            var overappliedNow = boonDefinitions.ToDictionary(definition => definition.Id, _ => 0);

            foreach (var player in squadPlayers)
            {
                var timeline = result.Players[player.UniqueID];
                var nearbyEnemies = timeline.NearbyEnemies[snapshotIndex];
                var isThreatened = nearbyEnemies > 0;
                timeline.Threatened[snapshotIndex] = isThreatened;

                if (isThreatened)
                {
                    threatenedNow++;
                    squadThreatSamples++;
                    playerThreatSamples[player.UniqueID]++;
                    playerThreatTime[player.UniqueID] += pollingRate;
                }

                timeline.RunningThreatTime[snapshotIndex] = playerThreatTime[player.UniqueID];

                for (var boonIndex = 0; boonIndex < boonDefinitions.Length; boonIndex++)
                {
                    var definition = boonDefinitions[boonIndex];
                    var stacks = GetBuffStacksAtTime(player, log, definition.Id, times[snapshotIndex]);
                    var playerBoon = timeline.Boons[boonIndex];
                    playerBoon.CurrentStacks[snapshotIndex] = stacks;

                    if (isThreatened && stacks > 0)
                    {
                        activeNow[definition.Id]++;
                        stackSumsNow[definition.Id] += stacks;
                        squadActiveThreatSamples[definition.Id]++;
                        playerActiveThreatSamples[player.UniqueID][definition.Id]++;
                    }
                    if (isThreatened)
                    {
                        squadThreatStackSums[definition.Id] += stacks;
                        if (definition.TracksOverapplication && stacks >= definition.OverapplicationThreshold)
                        {
                            overappliedNow[definition.Id]++;
                            squadOverappliedThreatSamples[definition.Id]++;
                            playerOverappliedThreatSamples[player.UniqueID][definition.Id]++;
                        }
                    }

                    playerBoon.RunningCoverage[snapshotIndex] = playerThreatSamples[player.UniqueID] > 0
                        ? Math.Round(playerActiveThreatSamples[player.UniqueID][definition.Id] * 100.0 / playerThreatSamples[player.UniqueID], 1)
                        : 0;
                    playerBoon.RunningOverapplication[snapshotIndex] =
                        definition.TracksOverapplication && playerThreatSamples[player.UniqueID] > 0
                            ? Math.Round(playerOverappliedThreatSamples[player.UniqueID][definition.Id] * 100.0 / playerThreatSamples[player.UniqueID], 1)
                            : 0;
                }
            }

            result.ThreatenedPlayerCount[snapshotIndex] = threatenedNow;
            for (var boonIndex = 0; boonIndex < boonDefinitions.Length; boonIndex++)
            {
                var definition = boonDefinitions[boonIndex];
                var boon = result.Boons[boonIndex];
                boon.CurrentCoverage[snapshotIndex] = threatenedNow > 0
                    ? Math.Round(activeNow[definition.Id] * 100.0 / threatenedNow, 1)
                    : 0;
                boon.RunningCoverage[snapshotIndex] = squadThreatSamples > 0
                    ? Math.Round(squadActiveThreatSamples[definition.Id] * 100.0 / squadThreatSamples, 1)
                    : 0;
                boon.CurrentAverageStacks[snapshotIndex] = threatenedNow > 0
                    ? Math.Round(stackSumsNow[definition.Id] * 1.0 / threatenedNow, 1)
                    : 0;
                boon.CurrentOverapplication[snapshotIndex] =
                    definition.TracksOverapplication && threatenedNow > 0
                        ? Math.Round(overappliedNow[definition.Id] * 100.0 / threatenedNow, 1)
                        : 0;
                boon.RunningOverapplication[snapshotIndex] =
                    definition.TracksOverapplication && squadThreatSamples > 0
                        ? Math.Round(squadOverappliedThreatSamples[definition.Id] * 100.0 / squadThreatSamples, 1)
                        : 0;
            }
        }

        foreach (var boon in result.Boons)
        {
            boon.SummaryCoverage = squadThreatSamples > 0
                ? Math.Round(squadActiveThreatSamples[boon.Id] * 100.0 / squadThreatSamples, 1)
                : 0;
            boon.SummaryAverageStacks = squadThreatSamples > 0
                ? Math.Round(squadThreatStackSums[boon.Id] * 1.0 / squadThreatSamples, 1)
                : 0;
            boon.SummaryOverapplication =
                boon.TracksOverapplication && squadThreatSamples > 0
                    ? Math.Round(squadOverappliedThreatSamples[boon.Id] * 100.0 / squadThreatSamples, 1)
                    : 0;
        }

        return result;
    }

    private static CombatReplayPositioningAnalysisDto BuildPositioningAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets,
        Player? commander,
        long[] times)
    {
        var snapshotCount = times.Length;
        var result = new CombatReplayPositioningAnalysisDto
        {
            HasCommander = commander != null,
            CommanderId = commander?.UniqueID ?? 0,
            CommanderName = commander?.Character ?? "",
            DesiredCommanderDistance = (int)PositioningSettings.DesiredCommanderDistance,
            MingledCommanderDistance = (int)PositioningSettings.MingledCommanderDistance,
            IgnoreCommanderDistance = (int)PositioningSettings.IgnoreCommanderDistance,
            EngageRange = (int)PositioningSettings.EngageRange,
            MingledRange = (int)PositioningSettings.MingledRange,
            MingledEnemyThreshold = PositioningSettings.MingledEnemyThreshold,
            EnemyCountThreshold = PositioningSettings.EnemyCountThreshold,
            OverextendedPlayerThreshold = PositioningSettings.OverextendedPlayerThreshold,
            EngagedEnemyCount = new int[snapshotCount],
            EnemiesNearCommanderCount = new int[snapshotCount],
            Mingled = new bool[snapshotCount],
            EligiblePlayerCount = new int[snapshotCount],
            InPositionCount = new int[snapshotCount],
            OutOfPositionCount = new int[snapshotCount],
            TooFarCount = new int[snapshotCount],
            OverextendedCount = new int[snapshotCount],
            LateralRiskCount = new int[snapshotCount],
            InPositionRate = new double[snapshotCount],
            Players = squadPlayers.ToDictionary(
                player => player.UniqueID,
                _ => new CombatReplayPositioningPlayerTimelineDto
                {
                    Eligible = new bool[snapshotCount],
                    InPosition = new bool[snapshotCount],
                    TooFar = new bool[snapshotCount],
                    Overextended = new bool[snapshotCount],
                    LateralRisk = new bool[snapshotCount],
                    DistanceToCommander = new int[snapshotCount],
                    EnemiesCloserThanCommander = new int[snapshotCount],
                    EnemiesAheadOfCommander = new int[snapshotCount],
                    RunningInPositionRate = new double[snapshotCount],
                    RunningTooFarRate = new double[snapshotCount],
                    RunningOverextendedRate = new double[snapshotCount],
                    RunningLateralRiskRate = new double[snapshotCount],
                }),
        };

        if (commander == null)
        {
            return result;
        }

        var nonCommanderSquadPlayers = squadPlayers.Where(player => player.UniqueID != commander.UniqueID).ToList();
        var playerEvaluatedSamples = nonCommanderSquadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var playerInPositionSamples = nonCommanderSquadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var playerTooFarSamples = nonCommanderSquadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var playerOverextendedSamples = nonCommanderSquadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var playerLateralRiskSamples = nonCommanderSquadPlayers.ToDictionary(player => player.UniqueID, _ => 0);
        var totalEvaluatedSamples = 0L;
        var totalInPositionSamples = 0L;
        var totalTooFarSamples = 0L;
        var totalOverextendedSamples = 0L;
        var totalLateralRiskSamples = 0L;

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            var time = times[snapshotIndex];
            if (!TryGetEligiblePosition(commander, log, time, out var commanderPosition))
            {
                continue;
            }

            var engagedEnemies = new List<Vector3>();
            foreach (var enemy in hostileTargets)
            {
                if (!TryGetEligiblePosition(enemy, log, time, out var enemyPosition))
                {
                    continue;
                }
                if (IsWithinRange(commanderPosition, enemyPosition, PositioningSettings.EngageRange))
                {
                    engagedEnemies.Add(enemyPosition);
                }
            }
            var enemiesNearCommander = engagedEnemies.Count(enemyPosition => IsWithinRange(commanderPosition, enemyPosition, PositioningSettings.MingledRange));
            var mingled = enemiesNearCommander > PositioningSettings.MingledEnemyThreshold;
            var desiredCommanderDistance = mingled ? PositioningSettings.MingledCommanderDistance : PositioningSettings.DesiredCommanderDistance;

            result.EngagedEnemyCount[snapshotIndex] = engagedEnemies.Count;
            result.EnemiesNearCommanderCount[snapshotIndex] = enemiesNearCommander;
            result.Mingled[snapshotIndex] = mingled;

            if (engagedEnemies.Count == 0)
            {
                continue;
            }

            var playerStates = new List<PositioningPlayerSnapshotState>(nonCommanderSquadPlayers.Count);
            foreach (var player in nonCommanderSquadPlayers)
            {
                var timeline = result.Players[player.UniqueID];
                if (!TryGetEligiblePosition(player, log, time, out var playerPosition))
                {
                    UpdateRunningPositioningRates(player.UniqueID, snapshotIndex, result, playerEvaluatedSamples, playerInPositionSamples, playerTooFarSamples, playerOverextendedSamples, playerLateralRiskSamples);
                    continue;
                }

                var distanceToCommander = (int)Math.Round(GetDistance2D(playerPosition, commanderPosition));
                timeline.DistanceToCommander[snapshotIndex] = distanceToCommander;
                if (distanceToCommander > PositioningSettings.IgnoreCommanderDistance)
                {
                    UpdateRunningPositioningRates(player.UniqueID, snapshotIndex, result, playerEvaluatedSamples, playerInPositionSamples, playerTooFarSamples, playerOverextendedSamples, playerLateralRiskSamples);
                    continue;
                }

                var enemiesCloserThanCommander = engagedEnemies.Count(enemyPosition => GetDistance2D(playerPosition, enemyPosition) < GetDistance2D(commanderPosition, enemyPosition));
                var enemiesAheadOfCommander = mingled ? 0 : engagedEnemies.Count(enemyPosition => IsPlayerAheadOfCommander(commanderPosition, playerPosition, enemyPosition));

                timeline.Eligible[snapshotIndex] = true;
                timeline.EnemiesCloserThanCommander[snapshotIndex] = enemiesCloserThanCommander;
                timeline.EnemiesAheadOfCommander[snapshotIndex] = enemiesAheadOfCommander;

                playerStates.Add(new PositioningPlayerSnapshotState(
                    PlayerId: player.UniqueID,
                    TooFar: distanceToCommander > desiredCommanderDistance,
                    Overextended: !mingled && enemiesAheadOfCommander > 0,
                    LateralRisk: !mingled && enemiesCloserThanCommander > PositioningSettings.EnemyCountThreshold));
            }

            var overextendedPlayers = playerStates.Count(state => state.Overextended);
            var effectiveOverextendedPlayers = mingled || overextendedPlayers >= PositioningSettings.OverextendedPlayerThreshold
                ? new HashSet<int>()
                : playerStates.Where(state => state.Overextended).Select(state => state.PlayerId).ToHashSet();

            foreach (var state in playerStates)
            {
                var timeline = result.Players[state.PlayerId];
                var effectiveOverextended = effectiveOverextendedPlayers.Contains(state.PlayerId);
                var outOfPosition = state.TooFar || state.LateralRisk || effectiveOverextended;

                timeline.TooFar[snapshotIndex] = state.TooFar;
                timeline.Overextended[snapshotIndex] = effectiveOverextended;
                timeline.LateralRisk[snapshotIndex] = state.LateralRisk;
                timeline.InPosition[snapshotIndex] = !outOfPosition;

                result.EligiblePlayerCount[snapshotIndex]++;
                if (timeline.InPosition[snapshotIndex])
                {
                    result.InPositionCount[snapshotIndex]++;
                }
                else
                {
                    result.OutOfPositionCount[snapshotIndex]++;
                }
                if (state.TooFar)
                {
                    result.TooFarCount[snapshotIndex]++;
                }
                if (effectiveOverextended)
                {
                    result.OverextendedCount[snapshotIndex]++;
                }
                if (state.LateralRisk)
                {
                    result.LateralRiskCount[snapshotIndex]++;
                }

                playerEvaluatedSamples[state.PlayerId]++;
                totalEvaluatedSamples++;
                if (timeline.InPosition[snapshotIndex])
                {
                    playerInPositionSamples[state.PlayerId]++;
                    totalInPositionSamples++;
                }
                if (state.TooFar)
                {
                    playerTooFarSamples[state.PlayerId]++;
                    totalTooFarSamples++;
                }
                if (effectiveOverextended)
                {
                    playerOverextendedSamples[state.PlayerId]++;
                    totalOverextendedSamples++;
                }
                if (state.LateralRisk)
                {
                    playerLateralRiskSamples[state.PlayerId]++;
                    totalLateralRiskSamples++;
                }
            }

            result.InPositionRate[snapshotIndex] = result.EligiblePlayerCount[snapshotIndex] > 0
                ? Math.Round(result.InPositionCount[snapshotIndex] * 100.0 / result.EligiblePlayerCount[snapshotIndex], 1)
                : 0;

            foreach (var player in nonCommanderSquadPlayers)
            {
                UpdateRunningPositioningRates(player.UniqueID, snapshotIndex, result, playerEvaluatedSamples, playerInPositionSamples, playerTooFarSamples, playerOverextendedSamples, playerLateralRiskSamples);
            }
        }

        result.SummaryEvaluatedSamples = totalEvaluatedSamples;
        result.SummaryInPositionRate = totalEvaluatedSamples > 0 ? Math.Round(totalInPositionSamples * 100.0 / totalEvaluatedSamples, 1) : 0;
        result.SummaryTooFarRate = totalEvaluatedSamples > 0 ? Math.Round(totalTooFarSamples * 100.0 / totalEvaluatedSamples, 1) : 0;
        result.SummaryOverextendedRate = totalEvaluatedSamples > 0 ? Math.Round(totalOverextendedSamples * 100.0 / totalEvaluatedSamples, 1) : 0;
        result.SummaryLateralRiskRate = totalEvaluatedSamples > 0 ? Math.Round(totalLateralRiskSamples * 100.0 / totalEvaluatedSamples, 1) : 0;
        return result;
    }

    private static ThreatBoonDefinition CreateThreatBoonDefinition(
        ParsedEvtcLog log,
        long boonId,
        bool stackBased,
        bool tracksOverapplication = false,
        int overapplicationThreshold = 0)
    {
        if (log.Buffs.BuffsByIDs.TryGetValue(boonId, out Buff? buff))
        {
            return new ThreatBoonDefinition(boonId, buff.Name, buff.Link, stackBased, tracksOverapplication, overapplicationThreshold);
        }
        return new ThreatBoonDefinition(boonId, boonId.ToString(), "", stackBased, tracksOverapplication, overapplicationThreshold);
    }

    private static List<CombatReplayAnalysisBurstSummaryDto> BuildTopBursts(CombatReplayTeamAnalysisDto analysis, IReadOnlyList<long> times)
    {
        var candidates = new List<CombatReplayAnalysisBurstSummaryDto>();
        var index = 0;
        while (index < times.Count)
        {
            if (analysis.BurstStrength[index] != "strong" || !analysis.StripSynced[index])
            {
                index++;
                continue;
            }

            var bestIndex = index;
            var nextIndex = index + 1;
            while (nextIndex < times.Count &&
                analysis.BurstStrength[nextIndex] == "strong")
            {
                if (analysis.StripSynced[nextIndex] && IsBetterBurstSnapshot(analysis, nextIndex, bestIndex, times))
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

    private static bool IsBetterBurstSnapshot(CombatReplayTeamAnalysisDto analysis, int candidateIndex, int currentBestIndex, IReadOnlyList<long> times)
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

    private static Dictionary<int, CombatReplayPlayerEvaluationDto> BuildPlayerEvaluations(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets,
        CombatReplayTeamAnalysisDto squadAnalysis,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        CombatReplayPositioningAnalysisDto positioningAnalysis,
        IReadOnlyList<long> times)
    {
        var aggregates = new List<CombatReplayPlayerEvaluationAggregate>(squadPlayers.Count);
        foreach (SingleActor player in squadPlayers)
        {
            aggregates.Add(BuildPlayerEvaluationAggregate(log, player, squadPlayers, hostileTargets, squadAnalysis, enemyAnalysis, positioningAnalysis, times));
        }

        CombatReplayPlayerEvaluationMaximums maximums = new()
        {
            DamageTotal = aggregates.Max(aggregate => aggregate.DamageTotal),
            AverageTopTargetContribution = aggregates.Max(aggregate => aggregate.AverageTopTargetContribution),
            OffensiveConditionPressure = aggregates.Max(aggregate => aggregate.OffensiveConditionPressure),
            ControlConditionPressure = aggregates.Max(aggregate => aggregate.ControlConditionPressure),
            StripsTotal = aggregates.Max(aggregate => aggregate.StripsTotal),
            HealingTotal = aggregates.Max(aggregate => aggregate.HealingTotal),
            BarrierTotal = aggregates.Max(aggregate => aggregate.BarrierTotal),
            CleansesTotal = aggregates.Max(aggregate => aggregate.CleansesTotal),
            ResurrectsTotal = aggregates.Max(aggregate => aggregate.ResurrectsTotal),
            OffensiveBoonSupport = aggregates.Max(aggregate => aggregate.OffensiveBoonSupport),
            DefensiveBoonSupport = aggregates.Max(aggregate => aggregate.DefensiveBoonSupport),
            DefensiveConditionPressure = aggregates.Max(aggregate => aggregate.DefensiveConditionPressure),
            EffectiveCrowdControlCount = aggregates.Max(aggregate => aggregate.EffectiveCrowdControlCount),
            EffectiveCrowdControlDuration = aggregates.Max(aggregate => aggregate.EffectiveCrowdControlDuration),
            BurstContributionWindows = aggregates.Max(aggregate => aggregate.BurstContributionWindows),
            ConversionContributionWindows = aggregates.Max(aggregate => aggregate.ConversionContributionWindows),
            ControlContributionWindows = aggregates.Max(aggregate => aggregate.ControlContributionWindows),
            DefensiveSupportWindows = aggregates.Max(aggregate => aggregate.DefensiveSupportWindows),
        };

        return aggregates.ToDictionary(
            aggregate => aggregate.PlayerId,
            aggregate => BuildPlayerEvaluationDto(aggregate, maximums, log.CombatData.HasEXTHealing, log.CombatData.HasEXTBarrier));
    }

    private static CombatReplayPlayerEvaluationAggregate BuildPlayerEvaluationAggregate(
        ParsedEvtcLog log,
        SingleActor player,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets,
        CombatReplayTeamAnalysisDto squadAnalysis,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        CombatReplayPositioningAnalysisDto positioningAnalysis,
        IReadOnlyList<long> times)
    {
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline = squadAnalysis.Attackers.GetValueOrDefault(player.UniqueID);
        CombatReplayPositioningPlayerTimelineDto? positioningTimeline = positioningAnalysis.Players.GetValueOrDefault(player.UniqueID);
        SupportStatistics supportStats = player.GetToAllySupportStats(log, 0, log.LogData.LogEnd);
        List<EvaluationWindow> burstWindows = BuildBurstWindows(squadAnalysis, times);
        List<EvaluationWindow> conversionWindows = BuildConversionWindows(squadAnalysis, times, log.LogData.LogEnd);
        List<EvaluationWindow> defensiveResponseWindows = BuildBurstWindows(enemyAnalysis, times);
        List<EvaluationWindow> offensiveConditionWindows = MergeEvaluationWindows([.. burstWindows, .. conversionWindows]);
        long wholeFightDamageToPlayers = 0;
        foreach (SingleActor target in hostileTargets)
        {
            wholeFightDamageToPlayers += player.GetDamageStats(target, log, 0, log.LogData.LogEnd).Damage;
        }

        long wholeFightHealing = log.CombatData.HasEXTHealing
            ? player.EXTHealing.GetOutgoingHealStats(null, log, 0, log.LogData.LogEnd).Healing
            : 0;
        long wholeFightBarrier = log.CombatData.HasEXTBarrier
            ? player.EXTBarrier.GetOutgoingBarrierStats(null, log, 0, log.LogData.LogEnd).Barrier
            : 0;

        List<CrowdControlEvent> effectiveCrowdControlEvents = GetEffectiveCrowdControlEvents(log, player, hostileTargets);
        int effectiveCount = effectiveCrowdControlEvents.Count;
        double effectiveDuration = Math.Round(effectiveCrowdControlEvents.Sum(crowdControlEvent => crowdControlEvent.Duration) / 1000.0, 1);
        Dictionary<EvaluationWindow, double> burstOffensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, burstWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> conversionOffensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, conversionWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> offensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, offensiveConditionWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> controlConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, conversionWindows, ControlConditionBuffIds);
        Dictionary<EvaluationWindow, double> defensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, defensiveResponseWindows, DefensiveConditionBuffIds);
        Dictionary<long, double> controlConditionSourceContribution = ComputeConditionContributionByBuff(log, player, hostileTargets, conversionWindows, ControlConditionBuffIds);
        double offensiveBoonSupport = ComputeBoonSupportByWindow(log, player, squadPlayers, offensiveConditionWindows, OffensiveSupportBoonIds);
        double defensiveBoonSupport = ComputeBoonSupportByWindow(log, player, squadPlayers, defensiveResponseWindows, DefensiveSupportBoonIds);

        return new CombatReplayPlayerEvaluationAggregate
        {
            PlayerId = player.UniqueID,
            DamageTotal = wholeFightDamageToPlayers,
            AverageTopTargetContribution = ComputeAverageContribution(attackerTimeline?.TopTargetContribution, attackerTimeline?.Damage),
            OffensiveConditionPressure = Math.Round(offensiveConditionContribution.Values.Sum(), 1),
            ControlConditionPressure = Math.Round(controlConditionContribution.Values.Sum(), 1),
            StripsTotal = supportStats.BoonStripCount,
            HealingTotal = wholeFightHealing,
            BarrierTotal = wholeFightBarrier,
            CleansesTotal = supportStats.ConditionCleanseCount,
            ResurrectsTotal = supportStats.ResurrectCount,
            OffensiveBoonSupport = offensiveBoonSupport,
            DefensiveBoonSupport = defensiveBoonSupport,
            DefensiveConditionPressure = Math.Round(defensiveConditionContribution.Values.Sum(), 1),
            EffectiveCrowdControlCount = effectiveCount,
            EffectiveCrowdControlDuration = effectiveDuration,
            BurstContributionWindows = CountBurstContributionWindows(attackerTimeline, burstWindows, times, burstOffensiveConditionContribution),
            ConversionContributionWindows = CountOffensiveConversionWindows(attackerTimeline, conversionWindows, times, conversionOffensiveConditionContribution),
            ControlContributionWindows = CountControlContributionWindows(attackerTimeline, conversionWindows, times, effectiveCrowdControlEvents, controlConditionContribution),
            DefensiveSupportWindows = CountDefensiveSupportWindows(attackerTimeline, defensiveResponseWindows, times, defensiveConditionContribution),
            HasPositioningData = positioningTimeline != null && positioningTimeline.Eligible.Any(sample => sample),
            PositioningSamples = CountEligibleSamples(positioningTimeline),
            InPositionRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.InPosition),
            TooFarRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.TooFar),
            OverextendedRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.Overextended),
            LateralRiskRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.LateralRisk),
            EffectiveCrowdControlSources = BuildEffectiveCrowdControlSourceEntries(effectiveCrowdControlEvents),
            ControlConditionSources = BuildConditionSourceEntries(log, controlConditionSourceContribution),
        };
    }

    private static CombatReplayPlayerEvaluationDto BuildPlayerEvaluationDto(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        bool hasHealingData,
        bool hasBarrierData)
    {
        double offenseScore = ComputeWeightedScore(
            (NormalizeValue(aggregate.DamageTotal, maximums.DamageTotal), 0.45),
            (NormalizeValue(aggregate.AverageTopTargetContribution, maximums.AverageTopTargetContribution), 0.20),
            (NormalizeValue(aggregate.OffensiveConditionPressure, maximums.OffensiveConditionPressure), maximums.OffensiveConditionPressure > 0.0 ? 0.12 : 0.0),
            (NormalizeValue(aggregate.BurstContributionWindows, maximums.BurstContributionWindows), 0.13),
            (NormalizeValue(aggregate.ConversionContributionWindows, maximums.ConversionContributionWindows), 0.10));

        double controlScore = ComputeWeightedScore(
            (NormalizeValue(aggregate.StripsTotal, maximums.StripsTotal), 0.32),
            (NormalizeValue(aggregate.EffectiveCrowdControlCount, maximums.EffectiveCrowdControlCount), 0.26),
            (NormalizeValue(aggregate.EffectiveCrowdControlDuration, maximums.EffectiveCrowdControlDuration), 0.16),
            (NormalizeValue(aggregate.ControlConditionPressure, maximums.ControlConditionPressure), maximums.ControlConditionPressure > 0.0 ? 0.14 : 0.0),
            (NormalizeValue(aggregate.ControlContributionWindows, maximums.ControlContributionWindows), 0.12));

        double supportScore = ComputeWeightedScore(
            (hasHealingData ? NormalizeValue(aggregate.HealingTotal, maximums.HealingTotal) : 0.0, hasHealingData ? 0.22 : 0.0),
            (hasBarrierData ? NormalizeValue(aggregate.BarrierTotal, maximums.BarrierTotal) : 0.0, hasBarrierData ? 0.14 : 0.0),
            (NormalizeValue(aggregate.CleansesTotal, maximums.CleansesTotal), hasHealingData || hasBarrierData ? 0.16 : 0.28),
            (NormalizeValue(aggregate.OffensiveBoonSupport, maximums.OffensiveBoonSupport), maximums.OffensiveBoonSupport > 0.0 ? 0.08 : 0.0),
            (NormalizeValue(aggregate.DefensiveBoonSupport, maximums.DefensiveBoonSupport), maximums.DefensiveBoonSupport > 0.0 ? 0.12 : 0.0),
            (NormalizeValue(aggregate.DefensiveConditionPressure, maximums.DefensiveConditionPressure), maximums.DefensiveConditionPressure > 0.0 ? 0.10 : 0.0),
            (NormalizeValue(aggregate.DefensiveSupportWindows, maximums.DefensiveSupportWindows), hasHealingData || hasBarrierData ? 0.10 : 0.20),
            (NormalizeValue(aggregate.ResurrectsTotal, maximums.ResurrectsTotal), hasHealingData || hasBarrierData ? 0.08 : 0.16));

        double positioningScore = aggregate.HasPositioningData
            ? Math.Clamp(
                aggregate.InPositionRate * 0.55 +
                (100.0 - aggregate.TooFarRate) * 0.15 +
                (100.0 - aggregate.OverextendedRate) * 0.15 +
                (100.0 - aggregate.LateralRiskRate) * 0.15,
                0.0,
                100.0)
            : 0.0;

        List<(string Label, double Score)> rankedRoles =
        [
            ("Offense", offenseScore),
            ("Control", controlScore),
            ("Support", supportScore),
        ];
        rankedRoles = [.. rankedRoles.OrderByDescending(role => role.Score)];
        double roleScoreTotal = rankedRoles.Sum(role => role.Score);
        string contributionProfile = rankedRoles[1].Score >= rankedRoles[0].Score * 0.6
            ? $"{rankedRoles[0].Label} + {rankedRoles[1].Label}"
            : rankedRoles[0].Label;
        List<CombatReplayPlayerEvaluationDetailSectionDto> controlTimingDetailSections = BuildControlTimingDetailSections(aggregate);

        return new CombatReplayPlayerEvaluationDto
        {
            ContributionProfile = contributionProfile,
            KeyContributionSummary = BuildPlayerContributionSummary(aggregate, rankedRoles[0].Label, rankedRoles[1].Label),
            RoleMix =
            [
                new CombatReplayPlayerRoleMixEntryDto
                {
                    Label = "Offense",
                    Percent = roleScoreTotal > 0.0 ? Math.Round(offenseScore * 100.0 / roleScoreTotal, 1) : 0.0,
                },
                new CombatReplayPlayerRoleMixEntryDto
                {
                    Label = "Control",
                    Percent = roleScoreTotal > 0.0 ? Math.Round(controlScore * 100.0 / roleScoreTotal, 1) : 0.0,
                },
                new CombatReplayPlayerRoleMixEntryDto
                {
                    Label = "Support",
                    Percent = roleScoreTotal > 0.0 ? Math.Round(supportScore * 100.0 / roleScoreTotal, 1) : 0.0,
                },
            ],
            Areas =
            [
                new CombatReplayPlayerEvaluationAreaDto
                {
                    Label = "Offensive Presence",
                    Value = BuildPluralizedLabel(aggregate.BurstContributionWindows, "burst window", "burst windows"),
                    Detail = $"{FormatWholeNumber(aggregate.DamageTotal)} whole-fight damage to enemy players, {FormatOneDecimal(aggregate.AverageTopTargetContribution)}% average top-target contribution, {FormatWholeNumber((long)Math.Round(aggregate.OffensiveConditionPressure))} offensive condition pressure in key windows, impact in {BuildPluralizedLabel(aggregate.ConversionContributionWindows, "conversion window", "conversion windows")}",
                },
                new CombatReplayPlayerEvaluationAreaDto
                {
                    Label = "Control Timing",
                    Value = BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows"),
                    Detail = $"{FormatWholeNumber(aggregate.StripsTotal)} strips, {FormatWholeNumber(aggregate.EffectiveCrowdControlCount)} effective CC, {FormatOneDecimal(aggregate.EffectiveCrowdControlDuration)}s total control, {FormatWholeNumber((long)Math.Round(aggregate.ControlConditionPressure))} control condition pressure",
                    IsInteractive = controlTimingDetailSections.Count > 0,
                    DrilldownTitle = "Control Timing Detail",
                    DrilldownSubtitle = "Shows the effective crowd control skills and control-condition types that fed this player's Control Timing result when that source data is available.",
                    DetailSections = controlTimingDetailSections,
                },
                new CombatReplayPlayerEvaluationAreaDto
                {
                    Label = "Support Under Pressure",
                    Value = BuildPluralizedLabel(aggregate.DefensiveSupportWindows, "response window", "response windows"),
                    Detail = BuildSupportDetail(aggregate, hasHealingData, hasBarrierData),
                },
                new CombatReplayPlayerEvaluationAreaDto
                {
                    Label = "Positioning Context",
                    Value = aggregate.HasPositioningData ? $"{FormatOneDecimal(aggregate.InPositionRate)}% in position" : "No positioning samples",
                    Detail = aggregate.HasPositioningData
                        ? $"{FormatOneDecimal(aggregate.TooFarRate)}% too far, {FormatOneDecimal(aggregate.OverextendedRate)}% overextended, {FormatOneDecimal(aggregate.LateralRiskRate)}% left/right exposed"
                        : "Commander-relative positioning could not be evaluated for this player.",
                },
            ],
        };
    }

    private static List<CombatReplayPlayerEvaluationDetailSectionDto> BuildControlTimingDetailSections(CombatReplayPlayerEvaluationAggregate aggregate)
    {
        var sections = new List<CombatReplayPlayerEvaluationDetailSectionDto>();
        if (aggregate.EffectiveCrowdControlSources.Count > 0)
        {
            sections.Add(new CombatReplayPlayerEvaluationDetailSectionDto
            {
                Label = "Effective Crowd Control Sources",
                Entries = aggregate.EffectiveCrowdControlSources,
            });
        }
        if (aggregate.ControlConditionSources.Count > 0)
        {
            sections.Add(new CombatReplayPlayerEvaluationDetailSectionDto
            {
                Label = "Control Condition Sources",
                Entries = aggregate.ControlConditionSources,
            });
        }
        return sections;
    }

    private static string BuildPlayerContributionSummary(
        CombatReplayPlayerEvaluationAggregate aggregate,
        string primaryRole,
        string secondaryRole)
    {
        return primaryRole switch
        {
            "Offense" when aggregate.ConversionContributionWindows > 0 => $"Most active in {BuildPluralizedLabel(aggregate.ConversionContributionWindows, "conversion window", "conversion windows")}, with pressure that carried into finishes.",
            "Offense" => $"Most active in {BuildPluralizedLabel(Math.Max(aggregate.BurstContributionWindows, 1), "burst window", "burst windows")}, with steady pressure on focused targets.",
            "Control" when aggregate.ControlContributionWindows > 0 && aggregate.ControlConditionPressure > 0
                => $"Most visible through timed strips, crowd control, and control conditions in {BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows")}.",
            "Control" when aggregate.ControlContributionWindows > 0 && aggregate.EffectiveCrowdControlCount > 0
                => $"Most visible through timed strips and effective crowd control in {BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows")}.",
            "Control" when aggregate.ControlContributionWindows > 0
                => $"Most visible through timed strips in {BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows")}.",
            "Control" => "Most visible through smaller control contributions across the fight's key exchanges.",
            "Support" when aggregate.DefensiveBoonSupport > 0 && aggregate.DefensiveConditionPressure > 0
                => $"Most visible in {BuildPluralizedLabel(Math.Max(aggregate.DefensiveSupportWindows, 1), "defensive response window", "defensive response windows")}, with defensive boon support and defensive conditions helping the squad recover pressure.",
            "Support" when aggregate.DefensiveBoonSupport > 0
                => $"Most visible in {BuildPluralizedLabel(Math.Max(aggregate.DefensiveSupportWindows, 1), "defensive response window", "defensive response windows")}, with defensive boon support helping the squad recover pressure.",
            "Support" when aggregate.DefensiveConditionPressure > 0
                => $"Most visible in {BuildPluralizedLabel(Math.Max(aggregate.DefensiveSupportWindows, 1), "defensive response window", "defensive response windows")}, with defensive conditions helping the squad absorb pressure.",
            "Support" => $"Most visible in {BuildPluralizedLabel(Math.Max(aggregate.DefensiveSupportWindows, 1), "defensive response window", "defensive response windows")}, helping the squad recover pressure.",
            _ when !string.IsNullOrEmpty(secondaryRole) => $"Observed contribution profile leans {primaryRole.ToLowerInvariant()} with {secondaryRole.ToLowerInvariant()} support around the fight's key exchanges.",
            _ => $"Observed contribution profile leans {primaryRole.ToLowerInvariant()} around the fight's key exchanges.",
        };
    }

    private static string BuildSupportDetail(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var parts = new List<string>();
        if (hasHealingData)
        {
            parts.Add($"{FormatWholeNumber(aggregate.HealingTotal)} healing");
        }
        else
        {
            parts.Add("healing unavailable in this log");
        }

        if (hasBarrierData)
        {
            parts.Add($"{FormatWholeNumber(aggregate.BarrierTotal)} barrier");
        }

        parts.Add($"{FormatWholeNumber(aggregate.CleansesTotal)} cleanses");
        if (aggregate.OffensiveBoonSupport > 0)
        {
            parts.Add($"{FormatWholeNumber((long)Math.Round(aggregate.OffensiveBoonSupport))} offensive boon-seconds");
        }
        if (aggregate.DefensiveBoonSupport > 0)
        {
            parts.Add($"{FormatWholeNumber((long)Math.Round(aggregate.DefensiveBoonSupport))} defensive boon-seconds");
        }
        if (aggregate.DefensiveConditionPressure > 0)
        {
            parts.Add($"{FormatWholeNumber((long)Math.Round(aggregate.DefensiveConditionPressure))} defensive condition pressure");
        }
        parts.Add($"{FormatWholeNumber(aggregate.ResurrectsTotal)} rez");
        return string.Join(", ", parts);
    }

    private static CombatReplayEventAnalysisDto BuildEventAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        return new CombatReplayEventAnalysisDto
        {
            BarrierSaves = BuildBarrierSaveAnalysis(log, squadPlayers),
            ConditionConversions = BuildConditionConversionAnalysis(log, hostileTargets),
        };
    }

    private static CombatReplayBarrierSaveAnalysisDto BuildBarrierSaveAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        var result = new CombatReplayBarrierSaveAnalysisDto
        {
            Available = log.CombatData.HasEXTBarrier,
        };
        if (!log.CombatData.HasEXTBarrier)
        {
            return result;
        }

        var candidateEvents = new List<BarrierSaveCandidate>();
        foreach (SingleActor player in squadPlayers)
        {
            candidateEvents.AddRange(BuildBarrierSaveCandidates(log, player));
        }

        List<CombatReplayBarrierSaveEventDto> events =
        [
            .. candidateEvents
                .OrderBy(candidate => candidate.StartTime)
                .Select(candidate => new CombatReplayBarrierSaveEventDto
                {
                    Time = candidate.StartTime,
                    TimeLabel = FormatTime(candidate.StartTime),
                    SavedPlayerId = candidate.SavedPlayer.UniqueID,
                    SavedPlayerName = candidate.SavedPlayer.Character,
                    SavedPlayerIcon = candidate.SavedPlayer.GetIcon(),
                    TotalBarrier = candidate.TotalBarrier,
                    BarrierAbsorbed = candidate.BarrierAbsorbed,
                    LowestHealthPercent = candidate.LowestHealthPercent,
                    ApproxHealthStart = candidate.ApproxHealthStart,
                    ApproxBarrierStart = candidate.ApproxBarrierStart,
                    HealthPercentStart = candidate.HealthPercentStart,
                    BarrierPercentStart = candidate.BarrierPercentStart,
                    ProviderSummary = BuildCompactContributorSummary(candidate.Providers),
                    Providers = [.. candidate.Providers],
                    IncomingDamage = [.. candidate.IncomingDamage],
                })
        ];
        result.Events = events;
        result.TotalEvents = events.Count;
        result.SavedPlayers = [.. events
            .GroupBy(evt => evt.SavedPlayerId)
            .Select(group => new CombatReplayEventActorSummaryDto
            {
                ActorId = group.Key,
                Name = group.First().SavedPlayerName,
                Icon = group.First().SavedPlayerIcon,
                Count = group.Count(),
                Amount = group.Sum(evt => evt.BarrierAbsorbed),
            })
            .OrderByDescending(entry => entry.Count)
            .ThenByDescending(entry => entry.Amount)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)];
        result.Providers = BuildTopActorSummaries(events.SelectMany(
            evt => evt.Providers.Select(provider => (provider.ActorId, provider.Name, provider.Icon, provider.Amount, evt.Time))));
        return result;
    }

    private static IReadOnlyList<BarrierSaveCandidate> BuildBarrierSaveCandidates(
        ParsedEvtcLog log,
        SingleActor player)
    {
        IReadOnlyList<EXTBarrierEvent> incomingBarrierEvents = player.EXTBarrier.GetIncomingBarrierEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd);
        if (incomingBarrierEvents.Count == 0)
        {
            return [];
        }

        var candidates = new List<BarrierSaveCandidate>();
        int index = 0;
        while (index < incomingBarrierEvents.Count)
        {
            EXTBarrierEvent firstEvent = incomingBarrierEvents[index];
            long clusterStart = firstEvent.Time;
            long clusterEnd = clusterStart;
            var clusterEvents = new List<EXTBarrierEvent> { firstEvent };
            int nextIndex = index + 1;
            while (nextIndex < incomingBarrierEvents.Count && incomingBarrierEvents[nextIndex].Time - clusterEnd <= SaveEventMergeWindow)
            {
                EXTBarrierEvent nextEvent = incomingBarrierEvents[nextIndex];
                clusterEvents.Add(nextEvent);
                clusterEnd = nextEvent.Time;
                nextIndex++;
            }

            BarrierSaveCandidate? candidate = TryBuildBarrierSaveCandidate(log, player, clusterStart, clusterEnd, clusterEvents);
            if (candidate != null)
            {
                candidates.Add(candidate.Value);
            }
            index = nextIndex;
        }

        return candidates;
    }

    private static BarrierSaveCandidate? TryBuildBarrierSaveCandidate(
        ParsedEvtcLog log,
        SingleActor player,
        long clusterStart,
        long clusterEnd,
        IReadOnlyList<EXTBarrierEvent> clusterEvents)
    {
        long lookaheadEnd = Math.Min(log.LogData.LogEnd, clusterEnd + SaveEventLookaheadWindow);
        long snapshotTime = Math.Max(log.LogData.LogStart, clusterStart - 1);
        IReadOnlyList<HealthDamageEvent> incomingDamageEvents = player.GetDamageTakenEvents(null, log, clusterStart, lookaheadEnd)
            .Where(damageEvent => damageEvent.HasHit && (damageEvent.HealthDamage > 0 || damageEvent.ShieldDamage > 0))
            .OrderBy(damageEvent => damageEvent.Time)
            .ToList();
        if (!incomingDamageEvents.Any())
        {
            return null;
        }
        if (player.IsDowned(log, clusterStart, lookaheadEnd))
        {
            return null;
        }

        int clusterBarrier = clusterEvents.Sum(barrierEvent => barrierEvent.BarrierGiven);
        if (clusterBarrier <= 0)
        {
            return null;
        }

        double healthPercentStart = GetSafePercent(player.GetCurrentHealthPercent(log, snapshotTime));
        double barrierPercentStart = GetSafePercent(player.GetCurrentBarrierPercent(log, snapshotTime));
        int approxHealthStart = GetApproximateCurrentHealth(player, log, snapshotTime);
        int approxBarrierStart = GetApproximateCurrentBarrier(player, log, snapshotTime);
        if (approxHealthStart <= 0)
        {
            return null;
        }

        int barrierAbsorbed = incomingDamageEvents.Sum(damageEvent => damageEvent.ShieldDamage);
        if (barrierAbsorbed <= 0)
        {
            return null;
        }

        int cumulativeThreat = incomingDamageEvents.Sum(damageEvent => damageEvent.HealthDamage + damageEvent.ShieldDamage);
        int lethalWithoutCluster = approxHealthStart + approxBarrierStart;
        int lethalWithCluster = approxHealthStart + approxBarrierStart + clusterBarrier;
        if (cumulativeThreat <= lethalWithoutCluster || cumulativeThreat > lethalWithCluster)
        {
            return null;
        }

        double lowestHealthPercent = GetLowestHealthPercent(player, log, clusterStart, lookaheadEnd, healthPercentStart);
        List<CombatReplayEventContributionDto> providers = BuildMeaningfulBarrierProviders(log, clusterEvents);
        if (providers.Count == 0)
        {
            return null;
        }

        return new BarrierSaveCandidate(
            StartTime: clusterStart,
            EndTime: lookaheadEnd,
            SavedPlayer: player,
            TotalBarrier: clusterBarrier,
            BarrierAbsorbed: barrierAbsorbed,
            ApproxHealthStart: approxHealthStart,
            ApproxBarrierStart: approxBarrierStart,
            HealthPercentStart: healthPercentStart,
            BarrierPercentStart: barrierPercentStart,
            LowestHealthPercent: lowestHealthPercent,
            Providers: providers,
            IncomingDamage: BuildIncomingDamageTimeline(log, incomingDamageEvents));
    }

    private static CombatReplayConditionConversionAnalysisDto BuildConditionConversionAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        var result = new CombatReplayConditionConversionAnalysisDto();
        var events = new List<CombatReplayConditionConversionEventDto>();
        foreach (SingleActor target in hostileTargets)
        {
            IReadOnlyList<DownEvent> downEvents = log.CombatData.GetDownEvents(target.AgentItem);
            foreach (DownEvent downEvent in downEvents.OrderBy(evt => evt.Time))
            {
                CombatReplayConditionConversionEventDto? conversionEvent = BuildConditionConversionEvent(log, target, downEvent);
                if (conversionEvent != null)
                {
                    events.Add(conversionEvent);
                }
            }
        }

        events.Sort((left, right) => left.Time.CompareTo(right.Time));
        result.Events = events;
        result.TotalEvents = events.Count;
        result.ConvertedEvents = events.Count(evt => evt.Outcome == "Converted");
        result.TotalBurningPressure = Math.Round(events.Sum(evt => evt.BurningPressure), 1);
        result.TotalPressure = Math.Round(events.Sum(evt => evt.TotalPressure), 1);
        result.Providers = BuildTopActorSummaries(events.SelectMany(
            evt => evt.Providers.Select(provider => (provider.ActorId, provider.Name, provider.Icon, provider.Amount, evt.Time))));
        return result;
    }

    private static CombatReplayConditionConversionEventDto? BuildConditionConversionEvent(
        ParsedEvtcLog log,
        SingleActor target,
        DownEvent downEvent)
    {
        long windowStart = Math.Max(log.LogData.LogStart, downEvent.Time - LookbackWindow);
        var buffBreakdowns = new List<CombatReplayEventContributionDto>(ConditionConversionDisplayBuffIds.Count);
        var providerTotals = new Dictionary<SingleActor, double>();
        var providerConditionTotals = new Dictionary<SingleActor, Dictionary<long, double>>();
        IReadOnlyDictionary<long, BuffVolumeByActorStatistics> activeBuffVolumes = target.GetActiveBuffVolumesDictionary(log, windowStart, downEvent.Time);
        foreach (long buffId in ConditionConversionDisplayBuffIds)
        {
            if (!log.Buffs.BuffsByIDs.TryGetValue(buffId, out Buff? buff))
            {
                continue;
            }

            if (!activeBuffVolumes.TryGetValue(buffId, out BuffVolumeByActorStatistics? buffStats))
            {
                continue;
            }

            double totalAmount = Math.Round(buffStats.IncomingBy.Values.Sum(), 1);
            if (totalAmount <= 0.0)
            {
                continue;
            }

            foreach ((SingleActor provider, double amount) in buffStats.IncomingBy)
            {
                if (amount <= 0.0)
                {
                    continue;
                }
                providerTotals[provider] = providerTotals.TryGetValue(provider, out double existingAmount) ? existingAmount + amount : amount;
                if (!providerConditionTotals.TryGetValue(provider, out Dictionary<long, double>? conditionTotals))
                {
                    conditionTotals = [];
                    providerConditionTotals[provider] = conditionTotals;
                }
                conditionTotals[buffId] = conditionTotals.TryGetValue(buffId, out double existingConditionAmount) ? existingConditionAmount + amount : amount;
            }

            buffBreakdowns.Add(new CombatReplayEventContributionDto
            {
                BuffId = buffId,
                Name = buff.Name,
                Icon = buff.Link,
                Amount = totalAmount,
            });
        }

        double totalPressure = Math.Round(buffBreakdowns.Sum(entry => entry.Amount), 1);
        if (totalPressure <= 0.0)
        {
            return null;
        }

        foreach (CombatReplayEventContributionDto entry in buffBreakdowns)
        {
            entry.Percent = Math.Round(entry.Amount * 100.0 / totalPressure, 1);
        }

        List<CombatReplayEventContributionDto> providers = BuildMeaningfulConditionProviders(log, providerTotals, providerConditionTotals, totalPressure);
        CombatReplayEventContributionDto? burningBreakdown = buffBreakdowns.FirstOrDefault(entry => entry.BuffId == Burning);
        CombatReplayEventContributionDto topCondition = buffBreakdowns
            .OrderByDescending(entry => entry.Amount)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .First();
        string outcome = "Down only";
        long? conversionTime = null;
        DeadEvent? nextDead = log.CombatData.GetDeadEvents(target.AgentItem).FirstOrDefault(evt => evt.Time >= downEvent.Time);
        AliveEvent? nextAlive = log.CombatData.GetAliveEvents(target.AgentItem).FirstOrDefault(evt => evt.Time >= downEvent.Time);
        if (nextDead != null && (nextAlive == null || nextDead.Time <= nextAlive.Time))
        {
            outcome = "Converted";
            conversionTime = nextDead.Time;
        }
        else if (nextAlive != null)
        {
            outcome = "Recovered";
        }

        return new CombatReplayConditionConversionEventDto
        {
            Time = downEvent.Time,
            TimeLabel = FormatTime(downEvent.Time),
            TargetId = target.UniqueID,
            TargetName = target.Character,
            TargetIcon = target.GetIcon(),
            Outcome = outcome,
            ConversionTime = conversionTime,
            ConversionTimeLabel = conversionTime.HasValue ? FormatTime(conversionTime.Value) : "",
            TotalPressure = totalPressure,
            BurningPressure = Math.Round(burningBreakdown?.Amount ?? 0.0, 1),
            TopConditionName = topCondition.Name,
            TopConditionIcon = topCondition.Icon,
            TopContributorSummary = BuildCompactContributorSummary(providers),
            Conditions = buffBreakdowns,
            Providers = providers,
        };
    }

    private static List<CombatReplayEventContributionDto> BuildMeaningfulBarrierProviders(
        ParsedEvtcLog log,
        IReadOnlyList<EXTBarrierEvent> clusterEvents)
    {
        double totalBarrier = clusterEvents.Sum(barrierEvent => barrierEvent.BarrierGiven);
        Dictionary<AgentItem, double> providerTotals = clusterEvents
            .GroupBy(barrierEvent => barrierEvent.CreditedFrom)
            .ToDictionary(group => group.Key, group => (double)group.Sum(barrierEvent => barrierEvent.BarrierGiven));
        return BuildMeaningfulActorContributionList(log, providerTotals, totalBarrier);
    }

    private static List<CombatReplayEventContributionDto> BuildMeaningfulConditionProviders(
        ParsedEvtcLog log,
        IReadOnlyDictionary<SingleActor, double> providerTotals,
        IReadOnlyDictionary<SingleActor, Dictionary<long, double>> providerConditionTotals,
        double totalPressure)
    {
        var actorTotals = providerTotals.ToDictionary(pair => pair.Key.AgentItem, pair => pair.Value);
        List<CombatReplayEventContributionDto> providers = BuildMeaningfulActorContributionList(log, actorTotals, totalPressure);
        Dictionary<int, SingleActor> actorsById = providerTotals.Keys.ToDictionary(actor => actor.UniqueID, actor => actor);
        foreach (CombatReplayEventContributionDto provider in providers)
        {
            if (provider.ActorId == null || !actorsById.TryGetValue(provider.ActorId.Value, out SingleActor? actor))
            {
                continue;
            }
            if (!providerConditionTotals.TryGetValue(actor, out Dictionary<long, double>? conditionTotals))
            {
                continue;
            }

            provider.Details = [.. ConditionConversionDisplayBuffIds
                .Where(buffId => conditionTotals.TryGetValue(buffId, out double amount) && amount > 0.0)
                .Select(buffId =>
                {
                    Buff buff = log.Buffs.BuffsByIDs[buffId];
                    double amount = Math.Round(conditionTotals[buffId], 1);
                    return new CombatReplayEventContributionDto
                    {
                        BuffId = buffId,
                        Name = buff.Name,
                        Icon = buff.Link,
                        Amount = amount,
                        Percent = provider.Amount > 0.0 ? Math.Round(amount * 100.0 / provider.Amount, 1) : 0.0,
                    };
                })];
        }
        return providers;
    }

    private static List<CombatReplayEventContributionDto> BuildMeaningfulActorContributionList(
        ParsedEvtcLog log,
        IReadOnlyDictionary<AgentItem, double> providerTotals,
        double totalAmount)
    {
        if (totalAmount <= 0.0 || providerTotals.Count == 0)
        {
            return [];
        }

        List<(AgentItem Agent, double Amount)> orderedProviders = [.. providerTotals
            .Where(pair => pair.Value > 0.0)
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetActorName(log, pair.Key), StringComparer.OrdinalIgnoreCase)
            .Select(pair => (pair.Key, pair.Value))];
        if (orderedProviders.Count == 0)
        {
            return [];
        }

        List<(AgentItem Agent, double Amount)> meaningfulProviders = [.. orderedProviders.Where(pair => pair.Amount / totalAmount >= MeaningfulContributionThreshold)];
        if (meaningfulProviders.Count == 0)
        {
            meaningfulProviders.Add(orderedProviders[0]);
        }

        var result = new List<CombatReplayEventContributionDto>(meaningfulProviders.Count + 1);
        foreach ((AgentItem agent, double amount) in meaningfulProviders)
        {
            SingleActor? actor = FindActor(log, agent);
            result.Add(new CombatReplayEventContributionDto
            {
                ActorId = actor?.UniqueID,
                Name = actor?.Character ?? GetActorName(log, agent),
                Icon = actor?.GetIcon() ?? "",
                Amount = Math.Round(amount, 1),
                Percent = Math.Round(amount * 100.0 / totalAmount, 1),
            });
        }

        double remainingAmount = Math.Round(totalAmount - meaningfulProviders.Sum(pair => pair.Amount), 1);
        if (remainingAmount > 0.0)
        {
            result.Add(new CombatReplayEventContributionDto
            {
                Name = "Other",
                Amount = remainingAmount,
                Percent = Math.Round(remainingAmount * 100.0 / totalAmount, 1),
            });
        }
        return result;
    }

    private static List<CombatReplayEventTimelineEntryDto> BuildIncomingDamageTimeline(
        ParsedEvtcLog log,
        IReadOnlyList<HealthDamageEvent> damageEvents)
    {
        return [.. damageEvents.Select(damageEvent =>
        {
            string value = damageEvent.ShieldDamage > 0
                ? $"{FormatWholeNumber(damageEvent.HealthDamage)} health, {FormatWholeNumber(damageEvent.ShieldDamage)} barrier"
                : $"{FormatWholeNumber(damageEvent.HealthDamage)} health";
            string secondary = "";
            if (!damageEvent.CreditedFrom.IsUnknown)
            {
                secondary = GetActorName(log, damageEvent.CreditedFrom);
            }
            return new CombatReplayEventTimelineEntryDto
            {
                Time = damageEvent.Time,
                TimeLabel = FormatTime(damageEvent.Time),
                Label = damageEvent.Skill.Name,
                Value = value,
                Secondary = secondary,
            };
        })];
    }

    private static List<CombatReplayEventActorSummaryDto> BuildTopActorSummaries(
        IEnumerable<(int? ActorId, string Name, string Icon, double Amount, long EventTime)> contributions)
    {
        return [.. contributions
            .Where(entry => entry.ActorId != null)
            .GroupBy(entry => entry.ActorId)
            .Select(group => new CombatReplayEventActorSummaryDto
            {
                ActorId = group.Key,
                Name = group.First().Name,
                Icon = group.First().Icon,
                Count = group.Count(),
                Amount = Math.Round(group.Sum(entry => entry.Amount), 1),
            })
            .OrderByDescending(entry => entry.Amount)
            .ThenByDescending(entry => entry.Count)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Take(5)];
    }

    private static string BuildCompactContributorSummary(IReadOnlyList<CombatReplayEventContributionDto> contributions)
    {
        List<CombatReplayEventContributionDto> displayContributions =
        [
            .. contributions.Where(entry =>
                !string.IsNullOrWhiteSpace(entry.Name) &&
                !string.Equals(entry.Name, "Other", StringComparison.OrdinalIgnoreCase))
        ];
        if (displayContributions.Count == 0)
        {
            return "No contributors";
        }
        CombatReplayEventContributionDto topContributor = displayContributions[0];
        return displayContributions.Count == 1
            ? topContributor.Name
            : $"{topContributor.Name} +{displayContributions.Count - 1}";
    }

    private static SingleActor? FindActor(ParsedEvtcLog log, AgentItem agent)
    {
        try
        {
            return log.FindActor(agent);
        }
        catch
        {
            return null;
        }
    }

    private static string GetActorName(ParsedEvtcLog log, AgentItem agent)
    {
        return FindActor(log, agent)?.Character ?? agent.Name;
    }

    private static double GetLowestHealthPercent(
        SingleActor actor,
        ParsedEvtcLog log,
        long start,
        long end,
        double defaultPercent)
    {
        double minimumPercent = defaultPercent;
        foreach (Segment healthSegment in actor.GetHealthUpdates(log))
        {
            if (healthSegment.Start < start || healthSegment.Start > end)
            {
                continue;
            }
            minimumPercent = Math.Min(minimumPercent, healthSegment.Value);
        }
        return Math.Round(minimumPercent, 1);
    }

    private static int GetApproximateCurrentHealth(SingleActor actor, ParsedEvtcLog log, long time)
    {
        double currentHealthPercent = GetSafePercent(actor.GetCurrentHealthPercent(log, time));
        int maxHealth = actor.GetHealth(log.CombatData);
        return maxHealth > 0 ? (int)Math.Round(maxHealth * currentHealthPercent / 100.0, 0) : -1;
    }

    private static int GetApproximateCurrentBarrier(SingleActor actor, ParsedEvtcLog log, long time)
    {
        double currentBarrierPercent = GetSafePercent(actor.GetCurrentBarrierPercent(log, time));
        int maxHealth = actor.GetHealth(log.CombatData);
        return maxHealth > 0 ? (int)Math.Round(maxHealth * currentBarrierPercent / 100.0, 0) : 0;
    }

    private static double GetSafePercent(double value)
    {
        return value >= 0.0 ? Math.Round(value, 1) : 0.0;
    }

    private static double ComputeBoonSupportByWindow(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> trackedBoonIds)
    {
        double total = 0.0;
        foreach (EvaluationWindow window in windows)
        {
            foreach (SingleActor recipient in squadPlayers)
            {
                if (recipient.UniqueID == provider.UniqueID)
                {
                    continue;
                }

                foreach (long boonId in trackedBoonIds)
                {
                    if (!log.Buffs.BuffsByIDs.TryGetValue(boonId, out Buff? buff))
                    {
                        continue;
                    }

                    foreach (AbstractBuffApplyEvent applyEvent in recipient.GetBuffApplyEventsOnByID(log, window.Start, window.End, boonId, provider))
                    {
                        switch (applyEvent)
                        {
                            case BuffApplyEvent buffApplyEvent when buffApplyEvent.AppliedDuration < int.MaxValue:
                                total += buffApplyEvent.AppliedDuration / 1000.0;
                                break;
                            case BuffExtensionEvent buffExtensionEvent:
                                total += buffExtensionEvent.ExtendedDuration / 1000.0;
                                break;
                        }
                    }
                }
            }
        }

        return Math.Round(total, 1);
    }

    private static List<CrowdControlEvent> GetEffectiveCrowdControlEvents(
        ParsedEvtcLog log,
        SingleActor actor,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        var effectiveEvents = new List<CrowdControlEvent>();

        foreach (SingleActor target in hostileTargets)
        {
            foreach (CrowdControlEvent crowdControlEvent in actor.GetJustOutgoingActorCrowdControlEvents(target, log, log.LogData.LogStart, log.LogData.LogEnd))
            {
                if (!IsCrowdControlEffective(log, target, crowdControlEvent))
                {
                    continue;
                }

                effectiveEvents.Add(crowdControlEvent);
            }
        }

        return effectiveEvents;
    }

    private static bool IsCrowdControlEffective(ParsedEvtcLog log, SingleActor target, CrowdControlEvent crowdControlEvent)
    {
        long stabilityCheckTime = Math.Max(log.LogData.LogStart, crowdControlEvent.Time - ParserHelper.ServerDelayConstant);
        return !target.HasBuff(log, Stability, stabilityCheckTime);
    }

    private static List<EvaluationWindow> BuildBurstWindows(CombatReplayTeamAnalysisDto analysis, IReadOnlyList<long> times)
    {
        var windows = new List<EvaluationWindow>();
        if (times.Count == 0)
        {
            return windows;
        }

        int limit = Math.Min(times.Count, Math.Min(analysis.BurstStrength.Length, analysis.StripSynced.Length));
        int windowStart = -1;
        for (int index = 0; index < limit; index++)
        {
            bool qualified = analysis.BurstStrength[index] == "strong" && analysis.StripSynced[index];
            if (qualified && windowStart < 0)
            {
                windowStart = index;
            }
            else if (!qualified && windowStart >= 0)
            {
                windows.Add(CreateEvaluationWindow(windowStart, index - 1, times, times[^1]));
                windowStart = -1;
            }
        }

        if (windowStart >= 0)
        {
            windows.Add(CreateEvaluationWindow(windowStart, limit - 1, times, times[^1]));
        }

        return windows;
    }

    private static List<EvaluationWindow> BuildConversionWindows(
        CombatReplayTeamAnalysisDto squadAnalysis,
        IReadOnlyList<long> times,
        long fightEnd)
    {
        var rawWindows = new List<EvaluationWindow>();
        if (times.Count == 0)
        {
            return rawWindows;
        }

        int previousDownsTotal = 0;
        int previousKillsTotal = 0;
        int limit = Math.Min(times.Count, Math.Min(squadAnalysis.DownsTotal.Length, squadAnalysis.KillsTotal.Length));
        for (int index = 0; index < limit; index++)
        {
            bool conversionAdvanced = squadAnalysis.DownsTotal[index] > previousDownsTotal || squadAnalysis.KillsTotal[index] > previousKillsTotal;
            if (conversionAdvanced)
            {
                long anchorTime = times[index];
                rawWindows.Add(new EvaluationWindow(
                    Math.Max(0, anchorTime - LookbackWindow),
                    Math.Min(fightEnd, anchorTime + BucketSize)));
            }
            previousDownsTotal = squadAnalysis.DownsTotal[index];
            previousKillsTotal = squadAnalysis.KillsTotal[index];
        }

        return MergeEvaluationWindows(rawWindows);
    }

    private static EvaluationWindow CreateEvaluationWindow(int startIndex, int endIndex, IReadOnlyList<long> times, long fightEnd)
    {
        long startTime = Math.Max(0, times[startIndex] - LookbackWindow);
        long endTime = Math.Min(fightEnd, times[endIndex] + BucketSize);
        return new EvaluationWindow(startTime, endTime);
    }

    private static List<EvaluationWindow> MergeEvaluationWindows(List<EvaluationWindow> windows)
    {
        if (windows.Count == 0)
        {
            return windows;
        }

        List<EvaluationWindow> mergedWindows = [windows[0]];
        foreach (EvaluationWindow window in windows.OrderBy(window => window.Start).Skip(1))
        {
            EvaluationWindow previous = mergedWindows[^1];
            if (window.Start <= previous.End + BucketSize)
            {
                mergedWindows[^1] = new EvaluationWindow(previous.Start, Math.Max(previous.End, window.End));
            }
            else
            {
                mergedWindows.Add(window);
            }
        }

        return mergedWindows;
    }

    private static int CountBurstContributionWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> burstWindows,
        IReadOnlyList<long> times,
        IReadOnlyDictionary<EvaluationWindow, double> conditionContribution)
    {
        return burstWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Damage, attackerTimeline?.Strips) ||
            conditionContribution.ContainsKey(window));
    }

    private static int CountOffensiveConversionWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> conversionWindows,
        IReadOnlyList<long> times,
        IReadOnlyDictionary<EvaluationWindow, double> conditionContribution)
    {
        return conversionWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Damage) ||
            conditionContribution.ContainsKey(window));
    }

    private static int CountControlContributionWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> conversionWindows,
        IReadOnlyList<long> times,
        IReadOnlyList<CrowdControlEvent> effectiveCrowdControlEvents,
        IReadOnlyDictionary<EvaluationWindow, double> conditionContribution)
    {
        return conversionWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Strips) ||
            effectiveCrowdControlEvents.Any(crowdControlEvent => crowdControlEvent.Time >= window.Start && crowdControlEvent.Time <= window.End) ||
            conditionContribution.ContainsKey(window));
    }

    private static int CountDefensiveSupportWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> defensiveResponseWindows,
        IReadOnlyList<long> times,
        IReadOnlyDictionary<EvaluationWindow, double> conditionContribution)
    {
        return defensiveResponseWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Healing, attackerTimeline?.Barrier, attackerTimeline?.Cleanses) ||
            conditionContribution.ContainsKey(window));
    }

    private static int CountTimelineContributionWindows(
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> times,
        params Array?[] series)
    {
        return windows.Count(window => series.Any(values => HasTimelineContribution(times, window, values)));
    }

    private static Dictionary<EvaluationWindow, double> ComputeConditionContributionByWindow(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> buffIds)
    {
        var result = new Dictionary<EvaluationWindow, double>();
        foreach (EvaluationWindow window in windows)
        {
            double windowTotal = 0.0;
            foreach (SingleActor recipient in recipients)
            {
                IReadOnlyDictionary<long, BuffVolumeByActorStatistics> activeBuffVolumes = recipient.GetActiveBuffVolumesDictionary(log, window.Start, window.End);
                foreach (long buffId in buffIds)
                {
                    if (activeBuffVolumes.TryGetValue(buffId, out BuffVolumeByActorStatistics? stats) &&
                        stats.IncomingBy.TryGetValue(provider, out double amount))
                    {
                        windowTotal += amount;
                    }
                }
            }

            if (windowTotal > 0.0)
            {
                result[window] = Math.Round(windowTotal, 1);
            }
        }

        return result;
    }

    private static Dictionary<long, double> ComputeConditionContributionByBuff(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> buffIds)
    {
        var result = buffIds.ToDictionary(buffId => buffId, _ => 0.0);
        foreach (EvaluationWindow window in windows)
        {
            foreach (SingleActor recipient in recipients)
            {
                IReadOnlyDictionary<long, BuffVolumeByActorStatistics> activeBuffVolumes = recipient.GetActiveBuffVolumesDictionary(log, window.Start, window.End);
                foreach (long buffId in buffIds)
                {
                    if (activeBuffVolumes.TryGetValue(buffId, out BuffVolumeByActorStatistics? stats) &&
                        stats.IncomingBy.TryGetValue(provider, out double amount))
                    {
                        result[buffId] += amount;
                    }
                }
            }
        }
        return result
            .Where(pair => pair.Value > 0.0)
            .ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 1));
    }

    private static List<CombatReplayPlayerEvaluationDetailEntryDto> BuildEffectiveCrowdControlSourceEntries(
        IReadOnlyList<CrowdControlEvent> effectiveCrowdControlEvents)
    {
        return [.. effectiveCrowdControlEvents
            .GroupBy(crowdControlEvent => crowdControlEvent.SkillID)
            .Select(group =>
            {
                CrowdControlEvent firstEvent = group.First();
                int count = group.Count();
                double durationSeconds = Math.Round(group.Sum(crowdControlEvent => crowdControlEvent.Duration) / 1000.0, 1);
                return new CombatReplayPlayerEvaluationDetailEntryDto
                {
                    Label = firstEvent.Skill.Name,
                    Value = BuildPluralizedLabel(count, "effective CC event", "effective CC events"),
                    Secondary = $"{FormatOneDecimal(durationSeconds)}s total control",
                };
            })
            .OrderByDescending(entry => ParseLeadingNumericValue(entry.Secondary))
            .ThenByDescending(entry => ParseLeadingNumericValue(entry.Value))
            .ThenBy(entry => entry.Label)];
    }

    private static List<CombatReplayPlayerEvaluationDetailEntryDto> BuildConditionSourceEntries(
        ParsedEvtcLog log,
        IReadOnlyDictionary<long, double> sourceContributionByBuff)
    {
        return [.. sourceContributionByBuff
            .Select(pair =>
            {
                string label = log.Buffs.BuffsByIDs.TryGetValue(pair.Key, out Buff? buff) ? buff.Name : $"Buff {pair.Key}";
                return new CombatReplayPlayerEvaluationDetailEntryDto
                {
                    Label = label,
                    Value = $"{FormatWholeNumber((long)Math.Round(pair.Value))} pressure",
                };
            })
            .OrderByDescending(entry => ParseLeadingNumericValue(entry.Value))
            .ThenBy(entry => entry.Label)];
    }

    private static double ParseLeadingNumericValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0.0;
        }
        string numericPortion = new string(value.TakeWhile(character => char.IsDigit(character) || character == '.' || character == ',').ToArray()).Replace(",", "");
        return double.TryParse(numericPortion, out double parsedValue) ? parsedValue : 0.0;
    }

    private static bool HasTimelineContribution(IReadOnlyList<long> times, EvaluationWindow window, Array? values)
    {
        if (values == null)
        {
            return false;
        }

        int limit = Math.Min(times.Count, values.Length);
        for (int index = 0; index < limit; index++)
        {
            if (times[index] < window.Start || times[index] > window.End)
            {
                continue;
            }

            switch (values)
            {
                case long[] longSeries when longSeries[index] > 0:
                    return true;
                case int[] intSeries when intSeries[index] > 0:
                    return true;
            }
        }

        return false;
    }

    private static bool HasTimelineContribution(IReadOnlyList<long> times, EvaluationWindow window, params Array?[] values)
    {
        return values.Any(series => HasTimelineContribution(times, window, series));
    }

    private static int CountEligibleSamples(CombatReplayPositioningPlayerTimelineDto? positioningTimeline)
    {
        return positioningTimeline?.Eligible.Count(sample => sample) ?? 0;
    }

    private static double ComputeEligibleRate(
        CombatReplayPositioningPlayerTimelineDto? positioningTimeline,
        Func<CombatReplayPositioningPlayerTimelineDto, bool[]> selector)
    {
        if (positioningTimeline == null)
        {
            return 0.0;
        }

        bool[] selectedSeries = selector(positioningTimeline);
        int eligibleCount = 0;
        int selectedCount = 0;
        for (int index = 0; index < positioningTimeline.Eligible.Length && index < selectedSeries.Length; index++)
        {
            if (!positioningTimeline.Eligible[index])
            {
                continue;
            }

            eligibleCount++;
            if (selectedSeries[index])
            {
                selectedCount++;
            }
        }

        return eligibleCount > 0 ? Math.Round(selectedCount * 100.0 / eligibleCount, 1) : 0.0;
    }

    private static double ComputeAverageContribution(double[]? values, long[]? weights)
    {
        if (values == null || values.Length == 0)
        {
            return 0.0;
        }

        if (weights == null || weights.Length == 0)
        {
            double average = values.Where(value => value > 0.0).DefaultIfEmpty(0.0).Average();
            return Math.Round(average, 1);
        }

        double totalWeight = 0.0;
        double totalValue = 0.0;
        for (int index = 0; index < values.Length && index < weights.Length; index++)
        {
            if (weights[index] <= 0)
            {
                continue;
            }

            totalWeight += weights[index];
            totalValue += values[index] * weights[index];
        }

        return totalWeight > 0.0 ? Math.Round(totalValue / totalWeight, 1) : 0.0;
    }

    private static double NormalizeValue(long value, long maximum)
    {
        return maximum > 0 ? Math.Clamp(value / (double)maximum, 0.0, 1.0) : 0.0;
    }

    private static double NormalizeValue(int value, int maximum)
    {
        return maximum > 0 ? Math.Clamp(value / (double)maximum, 0.0, 1.0) : 0.0;
    }

    private static double NormalizeValue(double value, double maximum)
    {
        return maximum > 0.0 ? Math.Clamp(value / maximum, 0.0, 1.0) : 0.0;
    }

    private static double ComputeWeightedScore(params (double Value, double Weight)[] inputs)
    {
        double totalWeight = 0.0;
        double totalValue = 0.0;
        foreach ((double value, double weight) in inputs)
        {
            if (weight <= 0.0)
            {
                continue;
            }
            totalWeight += weight;
            totalValue += value * weight;
        }

        return totalWeight > 0.0 ? (totalValue / totalWeight) * 100.0 : 0.0;
    }

    private static string BuildPluralizedLabel(int count, string singular, string plural)
    {
        return count == 1 ? $"1 {singular}" : $"{count} {plural}";
    }

    private static string FormatWholeNumber(long value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatWholeNumber(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatOneDecimal(double value)
    {
        return Math.Round(value, 1).ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatTime(long time)
    {
        return $"{(time / 1000.0).ToString("0.000", CultureInfo.InvariantCulture)}s";
    }

    private static long[] BuildTimes(long fightEnd, int pollingRate)
    {
        var times = new List<long>();
        for (long time = 0; time <= fightEnd; time += pollingRate)
        {
            times.Add(time);
        }
        if (times.Count == 0 || times[^1] != fightEnd)
        {
            times.Add(fightEnd);
        }
        return [.. times];
    }

    private static List<DamageRecord> BuildDamageRecords(ParsedEvtcLog log, TeamActorContext context)
    {
        var result = new List<DamageRecord>();
        foreach (var target in context.Targets)
        {
            foreach (var damageEvent in target.GetDamageTakenEvents(null, log))
            {
                var contributesDamage = damageEvent.HasHit && damageEvent.HealthDamage > 0;
                if (!contributesDamage && !damageEvent.HasDowned && !damageEvent.HasKilled)
                {
                    continue;
                }
                if (!context.AttackerIdsByAgent.TryGetValue(damageEvent.CreditedFrom, out var attackerUniqueId))
                {
                    continue;
                }
                result.Add(new DamageRecord(damageEvent.Time, target.UniqueID, attackerUniqueId, damageEvent.HealthDamage, damageEvent.HasDowned, damageEvent.HasKilled));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static List<HealingRecord> BuildHealingRecords(ParsedEvtcLog log, TeamActorContext context)
    {
        var result = new List<HealingRecord>();
        if (!log.CombatData.HasEXTHealing)
        {
            return result;
        }
        foreach (var attacker in context.Attackers)
        {
            foreach (var healingEvent in attacker.EXTHealing.GetOutgoingHealEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd))
            {
                if (!context.AttackerIdsByAgent.ContainsKey(healingEvent.To.GetFinalMaster()))
                {
                    continue;
                }
                result.Add(new HealingRecord(healingEvent.Time, attacker.UniqueID, healingEvent.HealingDone));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static List<BarrierRecord> BuildBarrierRecords(ParsedEvtcLog log, TeamActorContext context)
    {
        var result = new List<BarrierRecord>();
        if (!log.CombatData.HasEXTBarrier)
        {
            return result;
        }
        foreach (var attacker in context.Attackers)
        {
            foreach (var barrierEvent in attacker.EXTBarrier.GetOutgoingBarrierEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd))
            {
                if (!context.AttackerIdsByAgent.ContainsKey(barrierEvent.To.GetFinalMaster()))
                {
                    continue;
                }
                result.Add(new BarrierRecord(barrierEvent.Time, attacker.UniqueID, barrierEvent.BarrierGiven));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static List<CleanseRecord> BuildCleanseRecords(ParsedEvtcLog log, TeamActorContext context)
    {
        var result = new List<CleanseRecord>();
        var conditionIds = new HashSet<long>(log.Buffs.BuffsByClassification[Buff.BuffClassification.Condition].Select(buff => buff.ID));
        foreach (var cleanedActor in context.Attackers)
        {
            foreach (var removeEvent in log.CombatData.GetBuffRemoveAllDataByDst(cleanedActor.EnglobingAgentItem))
            {
                if (removeEvent.Time < cleanedActor.FirstAware || removeEvent.Time > cleanedActor.LastAware)
                {
                    continue;
                }
                if (!conditionIds.Contains(removeEvent.BuffID) || removeEvent.CreditedBy.IsUnknown || !removeEvent.ToFriendly)
                {
                    continue;
                }
                if (!context.AttackerIdsByAgent.TryGetValue(removeEvent.CreditedBy, out var attackerUniqueId))
                {
                    continue;
                }
                result.Add(new CleanseRecord(removeEvent.Time, attackerUniqueId));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static List<StripRecord> BuildStripRecords(ParsedEvtcLog log, TeamActorContext context, IReadOnlySet<long> boonIDs)
    {
        var result = new List<StripRecord>();
        foreach (var target in context.Targets)
        {
            foreach (var stripEvent in log.CombatData.GetBuffRemoveAllDataByDst(target.EnglobingAgentItem))
            {
                if (stripEvent.Time < target.FirstAware || stripEvent.Time > target.LastAware)
                {
                    continue;
                }
                if (!boonIDs.Contains(stripEvent.BuffID) || stripEvent.CreditedBy.IsUnknown)
                {
                    continue;
                }
                if (!context.AttackerIdsByAgent.TryGetValue(stripEvent.CreditedBy, out var attackerUniqueId))
                {
                    continue;
                }
                result.Add(new StripRecord(stripEvent.Time, target.UniqueID, attackerUniqueId));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static void PopulateRangeCounts(
        ParsedEvtcLog log,
        long time,
        TeamActorContext context,
        CombatReplayTeamAnalysisDto result,
        int snapshotIndex)
    {
        var attackerPositions = new Dictionary<int, Vector3>();
        foreach (var attacker in context.Attackers)
        {
            if (time < attacker.FirstAware || time > attacker.LastAware)
            {
                continue;
            }
            if (TryGetPosition(attacker, log, time, out var position))
            {
                attackerPositions[attacker.UniqueID] = position;
            }
        }

        var targetPositions = new Dictionary<int, Vector3>();
        foreach (var target in context.Targets)
        {
            if (time < target.FirstAware || time > target.LastAware)
            {
                continue;
            }
            if (TryGetPosition(target, log, time, out var position))
            {
                targetPositions[target.UniqueID] = position;
            }
        }

        foreach (var (attackerUniqueId, attackerPosition) in attackerPositions)
        {
            result.Attackers[attackerUniqueId].NearbyTargets[snapshotIndex] = targetPositions.Values.Count(targetPosition => IsWithinRange(attackerPosition, targetPosition, RangeThreshold));
        }

        foreach (var (targetUniqueId, targetPosition) in targetPositions)
        {
            result.Targets[targetUniqueId].NearbyAllies[snapshotIndex] = attackerPositions.Values.Count(attackerPosition => IsWithinRange(attackerPosition, targetPosition, RangeThreshold));
        }
    }

    private static bool TryGetPosition(SingleActor actor, ParsedEvtcLog log, long time, out Vector3 position)
    {
        return actor.TryGetCurrentInterpolatedPosition(log, time, out position) || actor.TryGetCurrentPosition(log, time, out position);
    }

    private static int GetBuffStacksAtTime(SingleActor actor, ParsedEvtcLog log, long buffId, long time)
    {
        return (int)Math.Max(0, Math.Round(actor.GetBuffStatus(log, buffId, time).Value));
    }

    private static bool TryGetEligiblePosition(SingleActor actor, ParsedEvtcLog log, long time, out Vector3 position)
    {
        position = default;
        if (time < actor.FirstAware || time > actor.LastAware || actor.IsDowned(log, time) || actor.IsDead(log, time) || actor.IsDC(log, time))
        {
            return false;
        }
        return TryGetPosition(actor, log, time, out position);
    }

    private static bool IsWithinRange(Vector3 left, Vector3 right, float range)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return dx * dx + dy * dy <= range * range;
    }

    private static float GetDistance2D(Vector3 left, Vector3 right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static bool IsPlayerAheadOfCommander(Vector3 commanderPosition, Vector3 playerPosition, Vector3 enemyPosition)
    {
        var commanderToPlayerX = playerPosition.X - commanderPosition.X;
        var commanderToPlayerY = playerPosition.Y - commanderPosition.Y;
        var commanderToEnemyX = enemyPosition.X - commanderPosition.X;
        var commanderToEnemyY = enemyPosition.Y - commanderPosition.Y;
        return commanderToPlayerX * commanderToEnemyX + commanderToPlayerY * commanderToEnemyY > 0;
    }

    private static void UpdateRunningPositioningRates(
        int playerId,
        int snapshotIndex,
        CombatReplayPositioningAnalysisDto result,
        IReadOnlyDictionary<int, int> evaluatedSamples,
        IReadOnlyDictionary<int, int> inPositionSamples,
        IReadOnlyDictionary<int, int> tooFarSamples,
        IReadOnlyDictionary<int, int> overextendedSamples,
        IReadOnlyDictionary<int, int> lateralRiskSamples)
    {
        var timeline = result.Players[playerId];
        var denominator = evaluatedSamples[playerId];
        timeline.RunningInPositionRate[snapshotIndex] = denominator > 0 ? Math.Round(inPositionSamples[playerId] * 100.0 / denominator, 1) : 0;
        timeline.RunningTooFarRate[snapshotIndex] = denominator > 0 ? Math.Round(tooFarSamples[playerId] * 100.0 / denominator, 1) : 0;
        timeline.RunningOverextendedRate[snapshotIndex] = denominator > 0 ? Math.Round(overextendedSamples[playerId] * 100.0 / denominator, 1) : 0;
        timeline.RunningLateralRiskRate[snapshotIndex] = denominator > 0 ? Math.Round(lateralRiskSamples[playerId] * 100.0 / denominator, 1) : 0;
    }

    private static int ComputeBucketIndex(long time, long windowStart)
    {
        return Math.Clamp((int)((time - windowStart) / BucketSize), 0, 2);
    }

    private static int GetPeakBucketIndex(IReadOnlyList<long> values)
    {
        var peakIndex = 0;
        var peakValue = values[0];
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index] > peakValue)
            {
                peakValue = values[index];
                peakIndex = index;
            }
        }
        return peakIndex;
    }

    private static int GetPeakBucketIndex(IReadOnlyList<int> values)
    {
        var peakIndex = 0;
        var peakValue = values[0];
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index] > peakValue)
            {
                peakValue = values[index];
                peakIndex = index;
            }
        }
        return peakIndex;
    }

    private static long GetPercentile(IReadOnlyList<long> values, double percentile)
    {
        if (values.Count == 0)
        {
            return 0;
        }
        var ordered = values.OrderBy(value => value).ToArray();
        var index = Math.Clamp((int)Math.Floor((ordered.Length - 1) * percentile), 0, ordered.Length - 1);
        return ordered[index];
    }

    private static int GetPercentile(IReadOnlyList<int> values, double percentile)
    {
        if (values.Count == 0)
        {
            return 0;
        }
        var ordered = values.OrderBy(value => value).ToArray();
        var index = Math.Clamp((int)Math.Floor((ordered.Length - 1) * percentile), 0, ordered.Length - 1);
        return ordered[index];
    }

    private readonly record struct ThreatBoonDefinition(
        long Id,
        string Name,
        string Icon,
        bool StackBased,
        bool TracksOverapplication,
        int OverapplicationThreshold);

    private readonly record struct PositioningCriteria(
        float DesiredCommanderDistance,
        float MingledCommanderDistance,
        float IgnoreCommanderDistance,
        float EngageRange,
        float MingledRange,
        int MingledEnemyThreshold,
        int EnemyCountThreshold,
        int OverextendedPlayerThreshold);

    private readonly record struct PositioningPlayerSnapshotState(
        int PlayerId,
        bool TooFar,
        bool Overextended,
        bool LateralRisk);
}
