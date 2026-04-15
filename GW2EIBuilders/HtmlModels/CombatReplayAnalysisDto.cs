using System.Numerics;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIBuilders.HtmlModels;

internal class CombatReplayAnalysisDto
{
    public int Lookback { get; set; }
    public long[] Times { get; set; } = [];
    public List<CombatReplayAnalysisBurstSummaryDto> TopBursts { get; set; } = [];

    public long[] SquadDamage { get; set; } = [];
    public int[] SquadDowns { get; set; } = [];
    public int[] SquadDownsTotal { get; set; } = [];
    public int[] SquadKills { get; set; } = [];
    public int[] SquadKillsTotal { get; set; } = [];
    public string[] BurstStrength { get; set; } = [];

    public int[] SquadStrips { get; set; } = [];
    public int[] StripPeakGap { get; set; } = [];
    public bool[] StripSynced { get; set; } = [];

    public int[] SquadTopTargetIds { get; set; } = [];
    public double[] SquadTopTargetShare { get; set; } = [];
    public double[] SquadTopThreeTargetShare { get; set; } = [];
    public int[] SquadTopTargetContributors { get; set; } = [];
    public bool[] SquadFocused { get; set; } = [];

    public int[] SquadTargetSaturationCount { get; set; } = [];
    public int[] SquadTargetSaturationDisplayCount { get; set; } = [];
    public string[] SquadTargetSaturation { get; set; } = [];

    public Dictionary<int, CombatReplayAnalysisPlayerTimelineDto> Players { get; set; } = [];
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

internal class CombatReplayAnalysisPlayerTimelineDto
{
    public long[] Damage { get; set; } = [];
    public int[] Strips { get; set; } = [];
    public double[] TopTargetContribution { get; set; } = [];
    public int[] HostilesHit { get; set; } = [];
    public int[] NearbyHostiles { get; set; } = [];
}

internal class CombatReplayAnalysisTargetTimelineDto
{
    public long[] DamageTaken { get; set; } = [];
    public int[] StripsTaken { get; set; } = [];
    public int[] SquadAttackers { get; set; } = [];
    public int[] NearbyAllies { get; set; } = [];
    public int[][] TopAttackerIds { get; set; } = [];
    public long[][] TopAttackerDamage { get; set; } = [];
}

internal static class CombatReplayAnalysisBuilder
{
    private const int LookbackWindow = 3000;
    private const int BucketSize = 1000;
    private const float RangeThreshold = 1200.0f;

    private readonly record struct DamageRecord(long Time, int TargetUniqueId, int PlayerUniqueId, int Damage, bool HasDowned, bool HasKilled);
    private readonly record struct StripRecord(long Time, int TargetUniqueId, int PlayerUniqueId);

    public static CombatReplayAnalysisDto? Build(ParsedEvtcLog log)
    {
        if (!log.CanCombatReplay ||
            log.LogData.Logic.ParseMode != LogLogic.ParseModeEnum.WvW ||
            log.LogData.Logic.Extension != "detailed_wvw")
        {
            return null;
        }

        var squadPlayers = log.PlayerList.Where(player => !player.IsFakeActor).ToList();
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

        var squadPlayersByAgent = squadPlayers.ToDictionary(player => player.AgentItem.GetFinalMaster(), player => player.UniqueID);
        var hostileTargetsById = hostileTargets.ToDictionary(target => target.UniqueID);
        var squadPlayersById = squadPlayers.ToDictionary(player => player.UniqueID);
        var boonIDs = new HashSet<long>(log.Buffs.BuffsByClassification[Buff.BuffClassification.Boon].Select(buff => buff.ID));

        var pollingRate = ParserHelper.CombatReplayPollingRate;
        var times = BuildTimes(log.LogData.LogEnd, pollingRate);
        var snapshotCount = times.Length;

        var result = new CombatReplayAnalysisDto
        {
            Lookback = LookbackWindow,
            Times = times,
            SquadDamage = new long[snapshotCount],
            SquadDowns = new int[snapshotCount],
            SquadDownsTotal = new int[snapshotCount],
            SquadKills = new int[snapshotCount],
            SquadKillsTotal = new int[snapshotCount],
            BurstStrength = new string[snapshotCount],
            SquadStrips = new int[snapshotCount],
            StripPeakGap = new int[snapshotCount],
            StripSynced = new bool[snapshotCount],
            SquadTopTargetIds = new int[snapshotCount],
            SquadTopTargetShare = new double[snapshotCount],
            SquadTopThreeTargetShare = new double[snapshotCount],
            SquadTopTargetContributors = new int[snapshotCount],
            SquadFocused = new bool[snapshotCount],
            SquadTargetSaturationCount = new int[snapshotCount],
            SquadTargetSaturationDisplayCount = new int[snapshotCount],
            SquadTargetSaturation = new string[snapshotCount],
            Players = squadPlayers.ToDictionary(
                player => player.UniqueID,
                _ => new CombatReplayAnalysisPlayerTimelineDto
                {
                    Damage = new long[snapshotCount],
                    Strips = new int[snapshotCount],
                    TopTargetContribution = new double[snapshotCount],
                    HostilesHit = new int[snapshotCount],
                    NearbyHostiles = new int[snapshotCount],
                }),
            Targets = hostileTargets.ToDictionary(
                target => target.UniqueID,
                _ => new CombatReplayAnalysisTargetTimelineDto
                {
                    DamageTaken = new long[snapshotCount],
                    StripsTaken = new int[snapshotCount],
                    SquadAttackers = new int[snapshotCount],
                    NearbyAllies = new int[snapshotCount],
                    TopAttackerIds = new int[snapshotCount][],
                    TopAttackerDamage = new long[snapshotCount][],
                }),
        };

        var damageRecords = BuildDamageRecords(log, hostileTargets, squadPlayersByAgent);
        var stripRecords = BuildStripRecords(log, hostileTargets, squadPlayersByAgent, boonIDs);

        var damagePeakGaps = new int[snapshotCount];
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

            var damageByPlayer = new Dictionary<int, long>();
            var damageByTarget = new Dictionary<int, long>();
            var damageByTargetByPlayer = new Dictionary<int, Dictionary<int, long>>();
            var playerTargetsHit = new Dictionary<int, HashSet<int>>();
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

                damageByPlayer[damage.PlayerUniqueId] = damageByPlayer.GetValueOrDefault(damage.PlayerUniqueId) + damage.Damage;
                damageByTarget[damage.TargetUniqueId] = damageByTarget.GetValueOrDefault(damage.TargetUniqueId) + damage.Damage;

                if (!damageByTargetByPlayer.TryGetValue(damage.TargetUniqueId, out var playerDamageOnTarget))
                {
                    playerDamageOnTarget = [];
                    damageByTargetByPlayer[damage.TargetUniqueId] = playerDamageOnTarget;
                }
                playerDamageOnTarget[damage.PlayerUniqueId] = playerDamageOnTarget.GetValueOrDefault(damage.PlayerUniqueId) + damage.Damage;

                if (!playerTargetsHit.TryGetValue(damage.PlayerUniqueId, out var hitTargets))
                {
                    hitTargets = [];
                    playerTargetsHit[damage.PlayerUniqueId] = hitTargets;
                }
                hitTargets.Add(damage.TargetUniqueId);

                if (!targetAttackers.TryGetValue(damage.TargetUniqueId, out var attackers))
                {
                    attackers = [];
                    targetAttackers[damage.TargetUniqueId] = attackers;
                }
                attackers.Add(damage.PlayerUniqueId);

                var bucketIndex = ComputeBucketIndex(damage.Time, windowStart);
                damageBuckets[bucketIndex] += damage.Damage;
            }

            var stripByPlayer = new Dictionary<int, int>();
            var stripByTarget = new Dictionary<int, int>();
            var stripBuckets = new int[3];
            var stripCount = 0;

            for (var index = stripIndexStart; index < stripIndexEnd; index++)
            {
                var strip = stripRecords[index];
                stripCount++;
                stripByPlayer[strip.PlayerUniqueId] = stripByPlayer.GetValueOrDefault(strip.PlayerUniqueId) + 1;
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
            if (topTargetId != 0 && totalDamage > 0 && damageByTargetByPlayer.TryGetValue(topTargetId, out var contributors))
            {
                contributorCount = contributors.Count(pair => pair.Value >= totalDamage * 0.05);
            }

            var effectiveTargetCount = totalDamage > 0
                ? damageByTarget.Values.Count(value => value >= totalDamage * 0.02)
                : 0;

            result.SquadDamage[snapshotIndex] = totalDamage;
            result.SquadDowns[snapshotIndex] = downs;
            result.SquadDownsTotal[snapshotIndex] = totalDowns;
            result.SquadKills[snapshotIndex] = kills;
            result.SquadKillsTotal[snapshotIndex] = totalKills;
            result.SquadStrips[snapshotIndex] = stripCount;
            result.SquadTopTargetIds[snapshotIndex] = topTargetId;
            result.SquadTopTargetShare[snapshotIndex] = totalDamage > 0 ? Math.Round(topTargetDamage * 100.0 / totalDamage, 1) : 0;
            result.SquadTopThreeTargetShare[snapshotIndex] = totalDamage > 0 ? Math.Round(topThreeDamage * 100.0 / totalDamage, 1) : 0;
            result.SquadTopTargetContributors[snapshotIndex] = contributorCount;
            result.SquadFocused[snapshotIndex] = totalDamage > 0 && result.SquadTopTargetShare[snapshotIndex] >= 50.0 && contributorCount >= 3;
            result.SquadTargetSaturationCount[snapshotIndex] = effectiveTargetCount;
            result.SquadTargetSaturationDisplayCount[snapshotIndex] = Math.Min(effectiveTargetCount, 5);
            result.SquadTargetSaturation[snapshotIndex] = effectiveTargetCount switch
            {
                < 3 => "under-saturated",
                <= 5 => "optimal",
                _ => "over-spread",
            };

            var damagePeakBucket = GetPeakBucketIndex(damageBuckets);
            var stripPeakBucket = GetPeakBucketIndex(stripBuckets);
            damagePeakGaps[snapshotIndex] = Math.Abs(damagePeakBucket - stripPeakBucket) * BucketSize;
            result.StripPeakGap[snapshotIndex] = damagePeakGaps[snapshotIndex];

            foreach (var player in squadPlayers)
            {
                var timeline = result.Players[player.UniqueID];
                timeline.Damage[snapshotIndex] = damageByPlayer.GetValueOrDefault(player.UniqueID);
                timeline.Strips[snapshotIndex] = stripByPlayer.GetValueOrDefault(player.UniqueID);
                timeline.HostilesHit[snapshotIndex] = playerTargetsHit.TryGetValue(player.UniqueID, out var hitTargets) ? hitTargets.Count : 0;

                if (topTargetId != 0 &&
                    topTargetDamage > 0 &&
                    damageByTargetByPlayer.TryGetValue(topTargetId, out var playerContribution) &&
                    playerContribution.TryGetValue(player.UniqueID, out var contributedDamage))
                {
                    timeline.TopTargetContribution[snapshotIndex] = Math.Round(contributedDamage * 100.0 / topTargetDamage, 1);
                }
            }

            foreach (var target in hostileTargets)
            {
                var timeline = result.Targets[target.UniqueID];
                timeline.DamageTaken[snapshotIndex] = damageByTarget.GetValueOrDefault(target.UniqueID);
                timeline.StripsTaken[snapshotIndex] = stripByTarget.GetValueOrDefault(target.UniqueID);
                timeline.SquadAttackers[snapshotIndex] = targetAttackers.TryGetValue(target.UniqueID, out var attackers) ? attackers.Count : 0;

                if (damageByTargetByPlayer.TryGetValue(target.UniqueID, out var attackerDamage))
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

            PopulateRangeCounts(log, time, squadPlayersById, hostileTargetsById, result, snapshotIndex);
        }

        var burstLowThreshold = GetPercentile(result.SquadDamage, 0.25);
        var burstHighThreshold = GetPercentile(result.SquadDamage, 0.75);
        var stripSyncThreshold = GetPercentile(result.SquadStrips, 0.75);

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            var damage = result.SquadDamage[snapshotIndex];
            result.BurstStrength[snapshotIndex] = damage <= burstLowThreshold
                ? "weak"
                : damage >= burstHighThreshold
                    ? "strong"
                    : "normal";

            result.StripSynced[snapshotIndex] =
                result.SquadStrips[snapshotIndex] > 0 &&
                result.SquadStrips[snapshotIndex] >= stripSyncThreshold &&
                result.StripPeakGap[snapshotIndex] <= BucketSize;
        }

        result.TopBursts = BuildTopBursts(result);

        return result;
    }

    private static List<CombatReplayAnalysisBurstSummaryDto> BuildTopBursts(CombatReplayAnalysisDto analysis)
    {
        var candidates = new List<CombatReplayAnalysisBurstSummaryDto>();
        var index = 0;
        while (index < analysis.Times.Length)
        {
            if (analysis.BurstStrength[index] != "strong" || !analysis.StripSynced[index])
            {
                index++;
                continue;
            }

            var bestIndex = index;
            var nextIndex = index + 1;
            while (nextIndex < analysis.Times.Length &&
                analysis.BurstStrength[nextIndex] == "strong")
            {
                if (analysis.StripSynced[nextIndex] && IsBetterBurstSnapshot(analysis, nextIndex, bestIndex))
                {
                    bestIndex = nextIndex;
                }
                nextIndex++;
            }

            candidates.Add(new CombatReplayAnalysisBurstSummaryDto
            {
                Time = analysis.Times[bestIndex],
                Damage = analysis.SquadDamage[bestIndex],
                Strips = analysis.SquadStrips[bestIndex],
                Downs = analysis.SquadDowns[bestIndex],
                DownsTotal = analysis.SquadDownsTotal[bestIndex],
                Kills = analysis.SquadKills[bestIndex],
                KillsTotal = analysis.SquadKillsTotal[bestIndex],
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

    private static bool IsBetterBurstSnapshot(CombatReplayAnalysisDto analysis, int candidateIndex, int currentBestIndex)
    {
        if (analysis.SquadDamage[candidateIndex] != analysis.SquadDamage[currentBestIndex])
        {
            return analysis.SquadDamage[candidateIndex] > analysis.SquadDamage[currentBestIndex];
        }
        if (analysis.SquadStrips[candidateIndex] != analysis.SquadStrips[currentBestIndex])
        {
            return analysis.SquadStrips[candidateIndex] > analysis.SquadStrips[currentBestIndex];
        }
        if (analysis.SquadDowns[candidateIndex] != analysis.SquadDowns[currentBestIndex])
        {
            return analysis.SquadDowns[candidateIndex] > analysis.SquadDowns[currentBestIndex];
        }
        if (analysis.SquadKills[candidateIndex] != analysis.SquadKills[currentBestIndex])
        {
            return analysis.SquadKills[candidateIndex] > analysis.SquadKills[currentBestIndex];
        }
        return analysis.Times[candidateIndex] < analysis.Times[currentBestIndex];
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

    private static List<DamageRecord> BuildDamageRecords(ParsedEvtcLog log, IReadOnlyList<SingleActor> hostileTargets, IReadOnlyDictionary<AgentItem, int> squadPlayersByAgent)
    {
        var result = new List<DamageRecord>();
        foreach (var target in hostileTargets)
        {
            foreach (var damageEvent in target.GetDamageTakenEvents(null, log))
            {
                var contributesDamage = damageEvent.HasHit && damageEvent.HealthDamage > 0;
                if (!contributesDamage && !damageEvent.HasDowned && !damageEvent.HasKilled)
                {
                    continue;
                }
                if (!squadPlayersByAgent.TryGetValue(damageEvent.CreditedFrom, out var playerUniqueId))
                {
                    continue;
                }
                result.Add(new DamageRecord(damageEvent.Time, target.UniqueID, playerUniqueId, damageEvent.HealthDamage, damageEvent.HasDowned, damageEvent.HasKilled));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static List<StripRecord> BuildStripRecords(ParsedEvtcLog log, IReadOnlyList<SingleActor> hostileTargets, IReadOnlyDictionary<AgentItem, int> squadPlayersByAgent, IReadOnlySet<long> boonIDs)
    {
        var result = new List<StripRecord>();
        foreach (var target in hostileTargets)
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
                if (!squadPlayersByAgent.TryGetValue(stripEvent.CreditedBy, out var playerUniqueId))
                {
                    continue;
                }
                result.Add(new StripRecord(stripEvent.Time, target.UniqueID, playerUniqueId));
            }
        }
        result.Sort((left, right) => left.Time.CompareTo(right.Time));
        return result;
    }

    private static void PopulateRangeCounts(
        ParsedEvtcLog log,
        long time,
        IReadOnlyDictionary<int, Player> squadPlayersById,
        IReadOnlyDictionary<int, SingleActor> hostileTargetsById,
        CombatReplayAnalysisDto result,
        int snapshotIndex)
    {
        var squadPositions = new Dictionary<int, Vector3>();
        foreach (var player in squadPlayersById.Values)
        {
            if (time < player.FirstAware || time > player.LastAware)
            {
                continue;
            }
            if (TryGetPosition(player, log, time, out var position))
            {
                squadPositions[player.UniqueID] = position;
            }
        }

        var hostilePositions = new Dictionary<int, Vector3>();
        foreach (var target in hostileTargetsById.Values)
        {
            if (time < target.FirstAware || time > target.LastAware)
            {
                continue;
            }
            if (TryGetPosition(target, log, time, out var position))
            {
                hostilePositions[target.UniqueID] = position;
            }
        }

        foreach (var (playerUniqueId, playerPosition) in squadPositions)
        {
            result.Players[playerUniqueId].NearbyHostiles[snapshotIndex] = hostilePositions.Values.Count(targetPosition => IsWithinRange(playerPosition, targetPosition, RangeThreshold));
        }

        foreach (var (targetUniqueId, targetPosition) in hostilePositions)
        {
            result.Targets[targetUniqueId].NearbyAllies[snapshotIndex] = squadPositions.Values.Count(playerPosition => IsWithinRange(playerPosition, targetPosition, RangeThreshold));
        }
    }

    private static bool TryGetPosition(SingleActor actor, ParsedEvtcLog log, long time, out Vector3 position)
    {
        return actor.TryGetCurrentInterpolatedPosition(log, time, out position) || actor.TryGetCurrentPosition(log, time, out position);
    }

    private static bool IsWithinRange(Vector3 left, Vector3 right, float range)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return dx * dx + dy * dy <= range * range;
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
}
