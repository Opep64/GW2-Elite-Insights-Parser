using System.Numerics;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SkillIDs;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIBuilders.HtmlModels;

internal class CombatReplayAnalysisDto
{
    public int Lookback { get; set; }
    public long[] Times { get; set; } = [];
    public CombatReplayTeamAnalysisDto Squad { get; set; } = new();
    public CombatReplayTeamAnalysisDto Enemy { get; set; } = new();
    public CombatReplayThreatBoonAnalysisDto ThreatBoons { get; set; } = new();
    public CombatReplayPositioningAnalysisDto Positioning { get; set; } = new();
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
    public int[] Strips { get; set; } = [];
    public int[] StripPeakGap { get; set; } = [];
    public bool[] StripSynced { get; set; } = [];
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

internal static class CombatReplayAnalysisBuilder
{
    private const int LookbackWindow = 3000;
    private const int BucketSize = 1000;
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

    private readonly record struct DamageRecord(long Time, int TargetUniqueId, int AttackerUniqueId, int Damage, bool HasDowned, bool HasKilled);
    private readonly record struct StripRecord(long Time, int TargetUniqueId, int AttackerUniqueId);
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
            "My Squad");
        var enemyContext = BuildContext(
            hostileTargets,
            squadPlayers,
            "Enemy Team");
        var squadAnalysis = BuildTeamAnalysis(log, squadContext, boonIDs, times, snapshotCount);
        Player? commander = log.PlayerList.FirstOrDefault(player => !player.IsFakeActor && player.IsCommander(log));

        return new CombatReplayAnalysisDto
        {
            Lookback = LookbackWindow,
            Times = times,
            Squad = squadAnalysis,
            Enemy = BuildTeamAnalysis(log, enemyContext, boonIDs, times, snapshotCount),
            ThreatBoons = BuildThreatBoonAnalysis(log, squadPlayers, times, pollingRate, squadAnalysis),
            Positioning = BuildPositioningAnalysis(log, squadPlayers, hostileTargets, commander, times),
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
            Strips = new int[snapshotCount],
            StripPeakGap = new int[snapshotCount],
            StripSynced = new bool[snapshotCount],
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
        var stripRecords = BuildStripRecords(log, context, boonIDs);

        var damageIndexStart = 0;
        var damageIndexEnd = 0;
        var cumulativeDamageIndex = 0;
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
            result.Strips[snapshotIndex] = stripCount;
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
            Label = "My Squad",
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
