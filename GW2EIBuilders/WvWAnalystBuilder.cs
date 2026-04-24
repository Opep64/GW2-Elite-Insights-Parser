using System.Text.Json;
using System.Globalization;
using GW2EIBuilders.HtmlModels;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIBuilders;

public sealed class WvWAnalystBuilder
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions IndentedSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly WvWAnalystFightPayloadDto? _payload;

    public bool HasPayload => _payload is not null;

    public WvWAnalystBuilder(ParsedEvtcLog log, Version parserVersion, string sourceFileName)
    {
        _payload = Build(log, parserVersion, sourceFileName);
    }

    public void CreateJSON(Stream stream, bool indent)
    {
        if (_payload is null)
        {
            throw new InvalidDataException("No WvW analyst payload is available for this log.");
        }

        JsonSerializer.Serialize(stream, _payload, indent ? IndentedSerializerOptions : DefaultSerializerOptions);
    }

    private static WvWAnalystFightPayloadDto? Build(ParsedEvtcLog log, Version parserVersion, string sourceFileName)
    {
        var combatReplayAnalysis = CombatReplayAnalysisBuilder.Build(log);
        var mainPhase = log.LogData.GetMainPhase(log);
        var summary = WvwSummaryDto.Build(log, mainPhase, combatReplayAnalysis);
        if (summary is null)
        {
            return null;
        }

        string encounterLabel = log.LogData.LogName;
        string mapLabel = ExtractMapLabel(encounterLabel);
        var squadPlayers = log.PlayerList.Where(player => !player.IsFakeActor).Cast<SingleActor>().ToList();
        var hostilePlayerTargets = log.LogData.Logic.Targets
            .Where(target =>
                !target.IsFakeActor &&
                target.AgentItem.Type == AgentItem.AgentType.NonSquadPlayer &&
                !target.IsSpecies(TargetID.WorldVersusWorld))
            .ToList();
        Player? commander = log.PlayerList.FirstOrDefault(player => !player.IsFakeActor && player.IsCommander(log));
        var outcome = BuildOutcome(summary);
        var players = BuildPlayerSummaries(log, mainPhase, squadPlayers, hostilePlayerTargets, combatReplayAnalysis, commander?.UniqueID ?? 0);
        var commanderSummary = BuildCommanderSummary(summary, combatReplayAnalysis, players, commander?.UniqueID ?? 0);
        var threatBoons = BuildThreatBoons(combatReplayAnalysis);
        var defenseSaves = BuildDefenseSaves(combatReplayAnalysis);
        var obliterate = BuildObliterateSummary(combatReplayAnalysis);
        var topBursts = BuildTopBursts(combatReplayAnalysis, squadPlayers);

        return new WvWAnalystFightPayloadDto
        {
            Meta = new WvWAnalystMetaDto
            {
                SchemaVersion = "1.8.0",
                PayloadType = "wvw-analyst-fight",
                DetailLevel = "summary+players+boons+lane-metrics+player-boons+provided-boons+top-bursts+defense-saves+obliterate",
                GeneratedAtUtc = DateTime.UtcNow.ToString("O"),
                ParserVersion = parserVersion.ToString(),
            },
            Source = new WvWAnalystSourceDto
            {
                SourceFileName = sourceFileName,
                SourceFileSha256 = string.Empty,
                LogGuid = string.Empty,
            },
            Fight = new WvWAnalystFightDto
            {
                FightId = $"{log.LogMetadata.DateStartStd}|{encounterLabel}",
                Mode = "wvw_detailed",
                MapCode = Slugify(mapLabel),
                MapLabel = mapLabel,
                EncounterLabel = encounterLabel,
                StartTimeUtc = log.LogMetadata.DateStartStd,
                EndTimeUtc = log.LogMetadata.DateEndStd,
                DurationMs = mainPhase.DurationInMS,
            },
            Availability = new WvWAnalystAvailabilityDto
            {
                CombatReplay = combatReplayAnalysis is not null,
                HealingStats = log.CombatData.HasEXTHealing,
                BarrierStats = log.CombatData.HasEXTBarrier,
                CrowdControlStats = log.CombatData.HasCrowdControlData,
                CommanderDetected = commander is not null,
            },
            Sides = new WvWAnalystSideCollectionDto
            {
                Squad = new WvWAnalystSideDto
                {
                    SideId = "squad",
                    DisplayLabel = summary.Squad.Label,
                    PlayerCount = summary.Squad.PlayerCount,
                    FriendlyNonSquadCount = summary.FriendlyPlayerCount,
                    EffectiveAlliedPlayerCount = summary.EffectiveAlliedPlayerCount,
                    Commander = BuildCommander(commander),
                    Totals = BuildSideTotals(summary.Squad),
                },
                Enemy = new WvWAnalystSideDto
                {
                    SideId = "enemy",
                    DisplayLabel = summary.Enemy.Label,
                    PlayerCount = summary.Enemy.PlayerCount,
                    Totals = BuildSideTotals(summary.Enemy),
                }
            },
            Outcome = outcome,
            Execution = BuildExecution(summary),
            CommanderSummary = commanderSummary,
            DefenseSaves = defenseSaves,
            Obliterate = obliterate,
            ThreatBoons = threatBoons,
            TopBursts = topBursts,
            Players = players,
        };
    }

    private static WvWAnalystSideTotalsDto BuildSideTotals(WvwSummarySideDto side)
    {
        return new WvWAnalystSideTotalsDto
        {
            Dps = side.Dps,
            Downs = side.Downs,
            Kills = side.Kills,
            DownKillConversionRate = side.DownKillConversionRate,
            Cleanses = side.Cleanses,
            Resurrects = side.Resurrects,
            Deaths = side.Deaths,
            Damage = side.Damage,
            DamageTaken = side.DamageTaken,
            Strips = side.BoonStrips,
            ReceivedCrowdControl = side.ReceivedCrowdControl,
            StripsPerMinute = side.StripsPerMinute,
            CleansesPerMinute = side.CleansesPerMinute,
        };
    }

    private static WvWAnalystExecutionDto BuildExecution(WvwSummaryDto summary)
    {
        var execution = summary.FightExecutionScore;
        return new WvWAnalystExecutionDto
        {
            ScoreAvailable = execution.ScoreAvailable,
            OverallScore = execution.ScoreAvailable ? execution.OverallScore : null,
            Grade = execution.Grade,
            Summary = execution.Summary,
            Detail = execution.Detail,
            StrongestPillarLabel = execution.StrongestPillarLabel,
            StrongestPillarSummary = execution.StrongestPillarSummary,
            WeakestPillarLabel = execution.WeakestPillarLabel,
            WeakestPillarSummary = execution.WeakestPillarSummary,
            Confidence = new WvWAnalystExecutionConfidenceDto
            {
                Label = execution.Confidence.Label,
                AvailableMetricCount = execution.Confidence.AvailableMetricCount,
                TotalMetricCount = execution.Confidence.TotalMetricCount,
                Notes = execution.Confidence.Notes.ToArray(),
            },
            Context = new WvWAnalystExecutionContextDto
            {
                SquadPlayerCount = execution.Context.SquadPlayerCount,
                EnemyPlayerCount = execution.Context.EnemyPlayerCount,
                FriendlyNonSquadCount = execution.Context.FriendlyNonSquadCount,
                PhaseDurationLabel = execution.Context.PhaseDuration,
                EnemyFormationStyleCode = Slugify(execution.Context.EnemyFormationStyleLabel),
                EnemyFormationStyleLabel = execution.Context.EnemyFormationStyleLabel,
                EnemyFormationStyleDetail = execution.Context.EnemyFormationStyleDetail,
                DataConfidenceLabel = execution.Context.DataConfidenceLabel,
                DataConfidenceDetail = execution.Context.DataConfidenceDetail,
            },
            Outcome = new WvWAnalystExecutionOutcomeDto
            {
                SquadDowns = execution.Outcome.SquadDowns,
                EnemyDowns = execution.Outcome.EnemyDowns,
                SquadKills = execution.Outcome.SquadKills,
                EnemyKills = execution.Outcome.EnemyKills,
                SquadDeaths = execution.Outcome.SquadDeaths,
                EnemyDeaths = execution.Outcome.EnemyDeaths,
                EnemyDownConversionRate = execution.Outcome.EnemyDownConversionRate,
                SquadRecoveryRate = execution.Outcome.SquadRecoveryRate,
                WipeLabel = execution.Outcome.WipeLabel,
            },
            Pillars = execution.Pillars.Select(BuildExecutionPillar).ToArray()
        };
    }

    private static IReadOnlyList<WvWAnalystPlayerSummaryDto> BuildPlayerSummaries(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        int commanderId)
    {
        return squadPlayers
            .Select(player => BuildPlayerSummary(log, phase, player, hostilePlayerTargets, combatReplayAnalysis, player.UniqueID == commanderId))
            .OrderByDescending(player => player.IsCommander)
            .ThenBy(player => player.Group)
            .ThenBy(player => player.Character, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static WvWAnalystPlayerSummaryDto BuildPlayerSummary(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        bool isCommander)
    {
        SupportStatistics support = player.GetToAllySupportStats(log, phase.Start, phase.End);
        DefenseAllStatistics defense = player.GetDefenseStats(log, phase.Start, phase.End);
        long damage = 0;
        int downs = 0;
        int kills = 0;
        foreach (SingleActor target in hostilePlayerTargets)
        {
            DamageStatistics damageStats = player.GetDamageStats(target, log, phase.Start, phase.End);
            damage += damageStats.Damage;
            OffensiveStatistics offensive = player.GetOffensiveStats(target, log, phase.Start, phase.End);
            downs += offensive.DownedCount;
            kills += offensive.KilledCount;
        }

        CombatReplayPlayerEvaluationDto? evaluation = TryGetPlayerEvaluation(combatReplayAnalysis, player.UniqueID);
        CombatReplayPositioningPlayerTimelineDto? positioningTimeline = TryGetPositioningTimeline(combatReplayAnalysis, player.UniqueID);

        return new WvWAnalystPlayerSummaryDto
        {
            ActorId = player.UniqueID,
            Account = player.Account,
            Character = player.Character,
            Profession = player.BaseSpec.ToString(),
            EliteSpec = player.Spec.ToString(),
            Icon = player.GetIcon(),
            Group = player.Group,
            IsCommander = isCommander,
            ActiveSeconds = Math.Round(player.GetActiveDuration(log, phase.Start, phase.End) / 1000.0, 1),
            CombatSeconds = Math.Round(player.GetTimeSpentInCombat(log, phase.Start, phase.End) / 1000.0, 1),
            Damage = damage,
            Downs = downs,
            Kills = Math.Min(kills, downs),
            Strips = support.BoonStripCount,
            OutgoingCleanses = support.ConditionCleanseCount,
            Healing = log.CombatData.HasEXTHealing ? player.EXTHealing.GetOutgoingHealStats(null, log, phase.Start, phase.End).Healing : 0,
            Barrier = log.CombatData.HasEXTBarrier ? player.EXTBarrier.GetOutgoingBarrierStats(null, log, phase.Start, phase.End).Barrier : 0,
            Resurrects = support.ResurrectCount,
            Deaths = defense.DeadCount,
            Recoveries = BuildPlayerRecoveryCount(log, player, phase),
            DamageTaken = defense.DamageTaken,
            ReceivedCrowdControl = defense.ReceivedCrowdControl,
            HasPositioningData = positioningTimeline is not null && CountEligibleSamples(positioningTimeline) > 0,
            PositioningSamples = CountEligibleSamples(positioningTimeline),
            InPositionRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.InPosition),
            TooFarRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.TooFar),
            OverextendedRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.Overextended),
            LateralRiskRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.LateralRisk),
            FitSummary = evaluation?.FitSummary ?? string.Empty,
            DemandFitSummary = evaluation?.DemandFitSummary ?? string.Empty,
            ContributionProfile = evaluation?.ContributionProfile ?? string.Empty,
            KeyContributionSummary = evaluation?.KeyContributionSummary ?? string.Empty,
            EvaluationConfidenceLabel = evaluation?.Confidence.Label ?? string.Empty,
            EvaluationConfidenceDetail = evaluation?.Confidence.Detail ?? string.Empty,
            EvaluationCaveats = evaluation?.Confidence.Caveats?.ToArray() ?? Array.Empty<string>(),
            EvidenceSnapshot = evaluation?.EvidenceSnapshot?.Take(4).ToArray() ?? Array.Empty<string>(),
            RoleMix = BuildRoleMix(evaluation),
            Lanes = BuildPlayerLanes(evaluation),
            ThreatBoons = BuildPlayerThreatBoons(combatReplayAnalysis, player.UniqueID),
            ProvidedBoons = BuildPlayerProvidedBoons(log, phase, player, combatReplayAnalysis),
        };
    }

    private static WvWAnalystCommanderSummaryDto BuildCommanderSummary(
        WvwSummaryDto summary,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<WvWAnalystPlayerSummaryDto> players,
        int commanderId)
    {
        if (commanderId == 0)
        {
            return new WvWAnalystCommanderSummaryDto();
        }

        WvWAnalystPlayerSummaryDto? commanderPlayer = players.FirstOrDefault(player => player.ActorId == commanderId);
        WvwSummaryExecutionPillarDto? cohesionPillar = summary.FightExecutionScore.Pillars.FirstOrDefault(
            pillar => string.Equals(pillar.Key, "cohesion-positioning", StringComparison.OrdinalIgnoreCase))
            ?? summary.FightExecutionScore.Pillars.FirstOrDefault(
                pillar => pillar.Label.Contains("Cohesion", StringComparison.OrdinalIgnoreCase));

        return new WvWAnalystCommanderSummaryDto
        {
            ActorId = commanderId,
            Available = commanderPlayer is not null,
            SquadPositioningSamples = combatReplayAnalysis?.Positioning.SummaryEvaluatedSamples ?? 0,
            SquadInPositionRate = combatReplayAnalysis?.Positioning.SummaryInPositionRate ?? 0,
            SquadTooFarRate = combatReplayAnalysis?.Positioning.SummaryTooFarRate ?? 0,
            SquadOverextendedRate = combatReplayAnalysis?.Positioning.SummaryOverextendedRate ?? 0,
            SquadLateralRiskRate = combatReplayAnalysis?.Positioning.SummaryLateralRiskRate ?? 0,
            CohesionPillarScore = cohesionPillar?.Score,
            CohesionPillarSummary = cohesionPillar?.Summary ?? string.Empty,
            FitSummary = commanderPlayer?.FitSummary ?? string.Empty,
            DemandFitSummary = commanderPlayer?.DemandFitSummary ?? string.Empty,
            ContributionProfile = commanderPlayer?.ContributionProfile ?? string.Empty,
            KeyContributionSummary = commanderPlayer?.KeyContributionSummary ?? string.Empty,
            EvaluationConfidenceLabel = commanderPlayer?.EvaluationConfidenceLabel ?? string.Empty,
            EvaluationConfidenceDetail = commanderPlayer?.EvaluationConfidenceDetail ?? string.Empty,
            EvaluationCaveats = commanderPlayer?.EvaluationCaveats ?? Array.Empty<string>(),
        };
    }

    private static IReadOnlyList<WvWAnalystPlayerRoleMixEntryDto> BuildRoleMix(CombatReplayPlayerEvaluationDto? evaluation)
    {
        if (evaluation?.RoleMix is null || evaluation.RoleMix.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerRoleMixEntryDto>();
        }

        return evaluation.RoleMix
            .Where(entry => entry.Percent > 0.0)
            .OrderByDescending(entry => entry.Percent)
            .Take(3)
            .Select(entry => new WvWAnalystPlayerRoleMixEntryDto
            {
                Label = entry.Label,
                Percent = entry.Percent,
            })
            .ToArray();
    }

    private static IReadOnlyList<WvWAnalystPlayerLaneSummaryDto> BuildPlayerLanes(CombatReplayPlayerEvaluationDto? evaluation)
    {
        if (evaluation?.Lanes is null || evaluation.Lanes.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerLaneSummaryDto>();
        }

        return evaluation.Lanes
            .Where(lane => lane.StrengthPercent > 0.0 || lane.SharePercent > 0.0)
            .OrderByDescending(lane => lane.StrengthPercent)
            .ThenByDescending(lane => lane.SharePercent)
            .Take(4)
            .Select(lane => new WvWAnalystPlayerLaneSummaryDto
            {
                Key = lane.Key,
                Label = lane.Label,
                StrengthPercent = lane.StrengthPercent,
                SharePercent = lane.SharePercent,
                WindowsHit = lane.WindowsHit,
                WindowsTotal = lane.WindowsTotal,
                WindowLabel = lane.WindowLabel,
                RateBand = lane.RateBand,
                EvidenceLine = lane.EvidenceLine,
                Metrics = lane.Metrics
                    .Where(metric => !string.IsNullOrWhiteSpace(metric.Label))
                    .Select(metric => new WvWAnalystPlayerLaneMetricDto
                    {
                        Key = metric.Key,
                        Label = metric.Label,
                        Value = metric.Value,
                        Unit = metric.Unit,
                        Aggregation = metric.Aggregation,
                    })
                    .ToArray(),
            })
            .ToArray();
    }

    private static IReadOnlyList<WvWAnalystThreatBoonSummaryDto> BuildThreatBoons(CombatReplayAnalysisDto? combatReplayAnalysis)
    {
        if (combatReplayAnalysis?.ThreatBoons?.Boons is null || combatReplayAnalysis.ThreatBoons.Boons.Count == 0)
        {
            return Array.Empty<WvWAnalystThreatBoonSummaryDto>();
        }

        return combatReplayAnalysis.ThreatBoons.Boons
            .Where(boon => boon.SummaryCoverage > 0.0 || boon.SummaryAverageStacks > 0.0 || boon.SummaryOverapplication > 0.0)
            .OrderByDescending(boon => boon.SummaryCoverage)
            .ThenByDescending(boon => boon.SummaryAverageStacks)
            .Select(boon => new WvWAnalystThreatBoonSummaryDto
            {
                Id = boon.Id,
                Name = boon.Name,
                Icon = boon.Icon,
                StackBased = boon.StackBased,
                TracksOverapplication = boon.TracksOverapplication,
                Coverage = boon.SummaryCoverage,
                AverageStacks = boon.SummaryAverageStacks,
                Overapplication = boon.SummaryOverapplication,
            })
            .ToArray();
    }

    private static WvWAnalystDefenseSaveSummaryDto? BuildDefenseSaves(CombatReplayAnalysisDto? combatReplayAnalysis)
    {
        CombatReplayDefenseSavedPlayersSummaryDto? summary = combatReplayAnalysis?.Defense?.SavedPlayersSummary;
        if (summary is null)
        {
            return null;
        }

        return new WvWAnalystDefenseSaveSummaryDto
        {
            SavedCases = summary.SavedCases,
            BarrierSavedCases = summary.BarrierSavedCases,
            DamageReductionSavedCases = summary.DamageReductionSavedCases,
            BothSavedCases = summary.BothSavedCases,
            TotalBarrierAbsorbed = summary.TotalBarrierAbsorbed,
            TotalEstimatedDamageReduction = summary.TotalEstimatedDamageReduction,
            AverageLowestHealthPercent = summary.AverageLowestHealthPercent,
            LowestLowestHealthPercent = summary.LowestLowestHealthPercent,
            TotalIncomingDamage = summary.TotalIncomingDamage,
            TotalIncomingHealing = summary.TotalIncomingHealing,
        };
    }

    private static WvWAnalystObliterateSummaryDto? BuildObliterateSummary(CombatReplayAnalysisDto? combatReplayAnalysis)
    {
        CombatReplayDownSummaryDto? summary = combatReplayAnalysis?.Events?.Downs?.CombinedSummary;
        if (summary is null)
        {
            return null;
        }

        return new WvWAnalystObliterateSummaryDto
        {
            HitCount = summary.OffensiveProtocolObliterateHitCount,
            BarrierRemovedHitCount = summary.OffensiveProtocolObliterateBarrierRemovedHitCount,
        };
    }

    private static IReadOnlyList<WvWAnalystTopBurstDto> BuildTopBursts(
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        if (combatReplayAnalysis?.Squad?.TopBursts is null ||
            combatReplayAnalysis.Squad.TopBursts.Count == 0 ||
            combatReplayAnalysis.Times.Length == 0)
        {
            return Array.Empty<WvWAnalystTopBurstDto>();
        }

        var playersById = squadPlayers.ToDictionary(player => player.UniqueID);

        return combatReplayAnalysis.Squad.TopBursts
            .Select(burst => BuildTopBurst(combatReplayAnalysis, playersById, burst))
            .Where(burst => burst is not null)
            .Select(burst => burst!)
            .ToArray();
    }

    private static WvWAnalystTopBurstDto? BuildTopBurst(
        CombatReplayAnalysisDto combatReplayAnalysis,
        IReadOnlyDictionary<int, SingleActor> playersById,
        CombatReplayAnalysisBurstSummaryDto burst)
    {
        var snapshotIndex = Array.BinarySearch(combatReplayAnalysis.Times, burst.Time);
        if (snapshotIndex < 0 || snapshotIndex >= combatReplayAnalysis.Times.Length)
        {
            return null;
        }

        return new WvWAnalystTopBurstDto
        {
            Time = burst.Time,
            TimeLabel = FormatBurstTime(burst.Time),
            Damage = burst.Damage,
            Strips = burst.Strips,
            Downs = burst.Downs,
            Kills = burst.Kills,
            TopPressure = BuildTopBurstActorSummary(
                playersById,
                combatReplayAnalysis.Squad.TopDamageActorIds,
                combatReplayAnalysis.Squad.TopDamageValues,
                snapshotIndex),
            TopStrips = BuildTopBurstActorSummary(
                playersById,
                combatReplayAnalysis.Squad.TopStripActorIds,
                combatReplayAnalysis.Squad.TopStripValues,
                snapshotIndex),
        };
    }

    private static string FormatBurstTime(long time)
    {
        return $"{(time / 1000.0).ToString("0.000", CultureInfo.InvariantCulture)}s";
    }

    private static WvWAnalystTopBurstActorDto BuildTopBurstActorSummary<TValue>(
        IReadOnlyDictionary<int, SingleActor> playersById,
        IReadOnlyList<int[]> actorIdsBySnapshot,
        IReadOnlyList<TValue[]> valuesBySnapshot,
        int snapshotIndex)
        where TValue : struct
    {
        if (snapshotIndex < 0 ||
            snapshotIndex >= actorIdsBySnapshot.Count ||
            snapshotIndex >= valuesBySnapshot.Count)
        {
            return new WvWAnalystTopBurstActorDto();
        }

        var actorIds = actorIdsBySnapshot[snapshotIndex];
        var values = valuesBySnapshot[snapshotIndex];
        if (actorIds is null || values is null || actorIds.Length == 0 || values.Length == 0)
        {
            return new WvWAnalystTopBurstActorDto();
        }

        var actorId = actorIds[0];
        if (!playersById.TryGetValue(actorId, out var player))
        {
            return new WvWAnalystTopBurstActorDto
            {
                ActorId = actorId,
                Amount = Convert.ToDouble(values[0], CultureInfo.InvariantCulture),
            };
        }

        return new WvWAnalystTopBurstActorDto
        {
            ActorId = actorId,
            Account = player.Account,
            Character = player.Character,
            Profession = player.BaseSpec.ToString(),
            EliteSpec = player.Spec.ToString(),
            Icon = player.GetIcon(),
            Amount = Convert.ToDouble(values[0], CultureInfo.InvariantCulture),
        };
    }

    private static IReadOnlyList<WvWAnalystPlayerThreatBoonSummaryDto> BuildPlayerThreatBoons(
        CombatReplayAnalysisDto? combatReplayAnalysis,
        int playerId)
    {
        if (combatReplayAnalysis?.ThreatBoons?.Players is null ||
            !combatReplayAnalysis.ThreatBoons.Players.TryGetValue(playerId, out CombatReplayThreatPlayerTimelineDto? playerTimeline) ||
            playerTimeline?.Boons is null ||
            playerTimeline.Boons.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerThreatBoonSummaryDto>();
        }

        bool[] threatenedTimeline = playerTimeline.Threatened ?? Array.Empty<bool>();
        int threatenedSamples = threatenedTimeline.Count(sample => sample);
        if (threatenedSamples <= 0)
        {
            return Array.Empty<WvWAnalystPlayerThreatBoonSummaryDto>();
        }

        return playerTimeline.Boons
            .Select(boon =>
            {
                int activeThreatSamples = 0;
                double threatStackTotal = 0.0;
                int overappliedThreatSamples = 0;
                int sampleCount = Math.Min(threatenedTimeline.Length, boon.CurrentStacks.Length);

                for (int index = 0; index < sampleCount; index++)
                {
                    if (!threatenedTimeline[index])
                    {
                        continue;
                    }

                    int stacks = boon.CurrentStacks[index];
                    threatStackTotal += stacks;
                    if (stacks > 0)
                    {
                        activeThreatSamples++;
                    }

                    if (boon.TracksOverapplication && stacks >= boon.OverapplicationThreshold)
                    {
                        overappliedThreatSamples++;
                    }
                }

                double coverage = threatenedSamples > 0
                    ? Math.Round(activeThreatSamples * 100.0 / threatenedSamples, 1)
                    : 0.0;
                double averageStacks = threatenedSamples > 0
                    ? Math.Round(threatStackTotal / threatenedSamples, 1)
                    : 0.0;
                double overapplication = boon.TracksOverapplication && threatenedSamples > 0
                    ? Math.Round(overappliedThreatSamples * 100.0 / threatenedSamples, 1)
                    : 0.0;

                return new WvWAnalystPlayerThreatBoonSummaryDto
                {
                    Id = boon.Id,
                    Name = boon.Name,
                    Icon = boon.Icon,
                    StackBased = boon.StackBased,
                    TracksOverapplication = boon.TracksOverapplication,
                    ThreatenedSamples = threatenedSamples,
                    ActiveThreatSamples = activeThreatSamples,
                    ThreatStackTotal = threatStackTotal,
                    OverappliedThreatSamples = overappliedThreatSamples,
                    Coverage = coverage,
                    AverageStacks = averageStacks,
                    Overapplication = overapplication,
                };
            })
            .Where(boon => boon.ActiveThreatSamples > 0 || boon.OverappliedThreatSamples > 0 || boon.ThreatStackTotal > 0.0)
            .OrderByDescending(boon => boon.Coverage)
            .ThenByDescending(boon => boon.AverageStacks)
            .ToArray();
    }

    private static IReadOnlyList<WvWAnalystPlayerProvidedBoonSummaryDto> BuildPlayerProvidedBoons(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        CombatReplayAnalysisDto? combatReplayAnalysis)
    {
        HashSet<long>? trackedBoonIds = combatReplayAnalysis?.ThreatBoons?.Boons?
            .Select(boon => boon.Id)
            .ToHashSet();

        if (trackedBoonIds is null || trackedBoonIds.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerProvidedBoonSummaryDto>();
        }

        IReadOnlyDictionary<long, BuffStatistics> squadBuffs = player.GetBuffs(ParserHelper.BuffEnum.Squad, log, phase.Start, phase.End);
        if (squadBuffs.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerProvidedBoonSummaryDto>();
        }

        return combatReplayAnalysis!.ThreatBoons.Boons
            .Where(boon => trackedBoonIds.Contains(boon.Id))
            .Select(boon =>
            {
                if (!squadBuffs.TryGetValue(boon.Id, out BuffStatistics? stats))
                {
                    return null;
                }

                if (stats.Generation <= 0.0 && stats.GenerationPresence <= 0.0 && stats.Overstack <= 0.0)
                {
                    return null;
                }

                return new WvWAnalystPlayerProvidedBoonSummaryDto
                {
                    Id = boon.Id,
                    Name = boon.Name,
                    Icon = boon.Icon,
                    StackBased = boon.StackBased,
                    Generation = stats.Generation,
                    GenerationPresence = stats.GenerationPresence,
                    Overstack = stats.Overstack,
                };
            })
            .Where(boon => boon is not null)
            .Select(boon => boon!)
            .OrderByDescending(boon => boon.Generation)
            .ThenByDescending(boon => boon.GenerationPresence)
            .ToArray();
    }

    private static CombatReplayPlayerEvaluationDto? TryGetPlayerEvaluation(CombatReplayAnalysisDto? analysis, int playerId)
    {
        if (analysis?.PlayerEvaluations is null)
        {
            return null;
        }

        return analysis.PlayerEvaluations.TryGetValue(playerId, out CombatReplayPlayerEvaluationDto? evaluation)
            ? evaluation
            : null;
    }

    private static CombatReplayPositioningPlayerTimelineDto? TryGetPositioningTimeline(CombatReplayAnalysisDto? analysis, int playerId)
    {
        if (analysis?.Positioning?.Players is null)
        {
            return null;
        }

        return analysis.Positioning.Players.TryGetValue(playerId, out CombatReplayPositioningPlayerTimelineDto? timeline)
            ? timeline
            : null;
    }

    private static int CountEligibleSamples(CombatReplayPositioningPlayerTimelineDto? timeline)
    {
        return timeline?.Eligible.Count(sample => sample) ?? 0;
    }

    private static double ComputeEligibleRate(
        CombatReplayPositioningPlayerTimelineDto? timeline,
        Func<CombatReplayPositioningPlayerTimelineDto, bool[]> selector)
    {
        if (timeline is null)
        {
            return 0.0;
        }

        int eligibleSamples = CountEligibleSamples(timeline);
        if (eligibleSamples == 0)
        {
            return 0.0;
        }

        bool[] values = selector(timeline);
        int activeSamples = 0;
        for (int index = 0; index < timeline.Eligible.Length && index < values.Length; index++)
        {
            if (timeline.Eligible[index] && values[index])
            {
                activeSamples++;
            }
        }

        return Math.Round(activeSamples * 100.0 / eligibleSamples, 1);
    }

    private static int BuildPlayerRecoveryCount(ParsedEvtcLog log, SingleActor player, PhaseData phase)
    {
        return log.CombatData.GetDownEvents(player.AgentItem)
            .Count(downEvent =>
                downEvent.Time >= phase.Start &&
                downEvent.Time <= phase.End &&
                string.Equals(GetDownOutcome(log, player.AgentItem, downEvent.Time), "Recovered", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetDownOutcome(ParsedEvtcLog log, AgentItem agent, long downTime)
    {
        DeadEvent? nextDead = log.CombatData.GetDeadEvents(agent).FirstOrDefault(evt => evt.Time >= downTime);
        AliveEvent? nextAlive = log.CombatData.GetAliveEvents(agent).FirstOrDefault(evt => evt.Time >= downTime);
        if (nextDead != null && (nextAlive == null || nextDead.Time <= nextAlive.Time))
        {
            return "Killed";
        }

        if (nextAlive != null)
        {
            return "Recovered";
        }

        return "Unresolved";
    }

    private static WvWAnalystExecutionPillarDto BuildExecutionPillar(WvwSummaryExecutionPillarDto pillar)
    {
        return new WvWAnalystExecutionPillarDto
        {
            PillarId = pillar.Key,
            Label = pillar.Label,
            Score = pillar.Score,
            Grade = pillar.Grade,
            AdjustedScore = pillar.AdjustedScore,
            AdjustedGrade = pillar.AdjustedGrade,
            AdjustmentApplied = pillar.AdjustmentApplied,
            AdjustmentDetail = pillar.AdjustmentDetail,
            Summary = pillar.Summary,
            Detail = pillar.Detail,
            AvailableMetricCount = pillar.AvailableMetricCount,
            MetricCount = pillar.MetricCount,
            Metrics = pillar.Metrics.Select(BuildExecutionMetric).ToArray(),
        };
    }

    private static WvWAnalystExecutionMetricDto BuildExecutionMetric(WvwSummaryExecutionMetricDto metric)
    {
        return new WvWAnalystExecutionMetricDto
        {
            MetricId = Slugify(metric.Label),
            Label = metric.Label,
            Unit = string.Empty,
            HigherIsBetter = true,
            Value = metric.Value,
            Note = metric.Note,
            Available = metric.Available,
            Neutralized = !metric.Available,
            Score = metric.Score,
            Values = new WvWAnalystExecutionMetricValueDto(),
        };
    }

    private static WvWAnalystCommanderDto BuildCommander(Player? commander)
    {
        if (commander is null)
        {
            return new WvWAnalystCommanderDto();
        }

        return new WvWAnalystCommanderDto
        {
            Account = commander.Account,
            Character = commander.Character,
            Profession = commander.BaseSpec.ToString(),
            EliteSpec = commander.Spec.ToString(),
        };
    }

    private static WvWAnalystOutcomeDto BuildOutcome(WvwSummaryDto summary)
    {
        const string squadSideId = "squad";
        const string enemySideId = "enemy";
        string outcomeCode;
        string winnerSideId;
        string displayLabel;
        string decidedBy;

        if (summary.Squad.Kills != summary.Enemy.Kills)
        {
            bool squadWon = summary.Squad.Kills > summary.Enemy.Kills;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? summary.Squad.Label : summary.Enemy.Label;
            decidedBy = "kills";
        }
        else if (summary.Squad.Downs != summary.Enemy.Downs)
        {
            bool squadWon = summary.Squad.Downs > summary.Enemy.Downs;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? summary.Squad.Label : summary.Enemy.Label;
            decidedBy = "downs";
        }
        else if (summary.Squad.Deaths != summary.Enemy.Deaths)
        {
            bool squadWon = summary.Squad.Deaths < summary.Enemy.Deaths;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? summary.Squad.Label : summary.Enemy.Label;
            decidedBy = "deaths";
        }
        else if (summary.Squad.Damage != summary.Enemy.Damage)
        {
            bool squadWon = summary.Squad.Damage > summary.Enemy.Damage;
            outcomeCode = squadWon ? squadSideId : enemySideId;
            winnerSideId = outcomeCode;
            displayLabel = squadWon ? summary.Squad.Label : summary.Enemy.Label;
            decidedBy = "damage";
        }
        else
        {
            outcomeCode = "draw";
            winnerSideId = string.Empty;
            displayLabel = "Draw";
            decidedBy = "none";
        }

        return new WvWAnalystOutcomeDto
        {
            OutcomeCode = outcomeCode,
            WinnerSideId = winnerSideId,
            DisplayLabel = displayLabel,
            DecidedBy = decidedBy,
            TieBreakOrder = ["kills", "downs", "deaths", "damage"],
        };
    }

    private static string ExtractMapLabel(string encounterLabel)
    {
        int separatorIndex = encounterLabel.IndexOf(" - ", StringComparison.Ordinal);
        return separatorIndex >= 0 && separatorIndex + 3 < encounterLabel.Length
            ? encounterLabel[(separatorIndex + 3)..]
            : encounterLabel;
    }

    private static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[Math.Min(value.Length, 48)];
        int index = 0;
        bool previousDash = false;

        foreach (char character in value)
        {
            if (index >= buffer.Length)
            {
                break;
            }

            if (char.IsLetterOrDigit(character))
            {
                buffer[index++] = char.ToLowerInvariant(character);
                previousDash = false;
                continue;
            }

            if (previousDash || index == 0)
            {
                continue;
            }

            buffer[index++] = '-';
            previousDash = true;
        }

        while (index > 0 && buffer[index - 1] == '-')
        {
            index--;
        }

        return new string(buffer[..index]);
    }
}

internal sealed class WvWAnalystFightPayloadDto
{
    public WvWAnalystMetaDto Meta { get; set; } = new();
    public WvWAnalystSourceDto Source { get; set; } = new();
    public WvWAnalystFightDto Fight { get; set; } = new();
    public WvWAnalystAvailabilityDto Availability { get; set; } = new();
    public WvWAnalystSideCollectionDto Sides { get; set; } = new();
    public WvWAnalystOutcomeDto Outcome { get; set; } = new();
    public WvWAnalystExecutionDto Execution { get; set; } = new();
    public WvWAnalystCommanderSummaryDto CommanderSummary { get; set; } = new();
    public WvWAnalystDefenseSaveSummaryDto? DefenseSaves { get; set; }
    public WvWAnalystObliterateSummaryDto? Obliterate { get; set; }
    public IReadOnlyList<WvWAnalystThreatBoonSummaryDto> ThreatBoons { get; set; } = Array.Empty<WvWAnalystThreatBoonSummaryDto>();
    public IReadOnlyList<WvWAnalystTopBurstDto> TopBursts { get; set; } = Array.Empty<WvWAnalystTopBurstDto>();
    public IReadOnlyList<WvWAnalystPlayerSummaryDto> Players { get; set; } = Array.Empty<WvWAnalystPlayerSummaryDto>();
}

internal sealed class WvWAnalystMetaDto
{
    public string SchemaVersion { get; set; } = string.Empty;
    public string PayloadType { get; set; } = string.Empty;
    public string DetailLevel { get; set; } = string.Empty;
    public string GeneratedAtUtc { get; set; } = string.Empty;
    public string ParserVersion { get; set; } = string.Empty;
}

internal sealed class WvWAnalystSourceDto
{
    public string SourceFileSha256 { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public string LogGuid { get; set; } = string.Empty;
}

internal sealed class WvWAnalystFightDto
{
    public string FightId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string MapCode { get; set; } = string.Empty;
    public string MapLabel { get; set; } = string.Empty;
    public string EncounterLabel { get; set; } = string.Empty;
    public string StartTimeUtc { get; set; } = string.Empty;
    public string EndTimeUtc { get; set; } = string.Empty;
    public long? DurationMs { get; set; }
}

internal sealed class WvWAnalystAvailabilityDto
{
    public bool CombatReplay { get; set; }
    public bool HealingStats { get; set; }
    public bool BarrierStats { get; set; }
    public bool CrowdControlStats { get; set; }
    public bool CommanderDetected { get; set; }
}

internal sealed class WvWAnalystSideCollectionDto
{
    public WvWAnalystSideDto Squad { get; set; } = new();
    public WvWAnalystSideDto Enemy { get; set; } = new();
}

internal sealed class WvWAnalystSideDto
{
    public string SideId { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int FriendlyNonSquadCount { get; set; }
    public double EffectiveAlliedPlayerCount { get; set; }
    public WvWAnalystCommanderDto Commander { get; set; } = new();
    public WvWAnalystSideTotalsDto Totals { get; set; } = new();
}

internal sealed class WvWAnalystCommanderDto
{
    public string Account { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public string EliteSpec { get; set; } = string.Empty;
}

internal sealed class WvWAnalystSideTotalsDto
{
    public double Dps { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public double DownKillConversionRate { get; set; }
    public int Cleanses { get; set; }
    public int Resurrects { get; set; }
    public int Deaths { get; set; }
    public long Damage { get; set; }
    public long DamageTaken { get; set; }
    public long Strips { get; set; }
    public int ReceivedCrowdControl { get; set; }
    public double StripsPerMinute { get; set; }
    public double CleansesPerMinute { get; set; }
}

internal sealed class WvWAnalystOutcomeDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string WinnerSideId { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public string DecidedBy { get; set; } = string.Empty;
    public IReadOnlyList<string> TieBreakOrder { get; set; } = Array.Empty<string>();
}

internal sealed class WvWAnalystExecutionDto
{
    public bool ScoreAvailable { get; set; }
    public int? OverallScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string StrongestPillarLabel { get; set; } = string.Empty;
    public string StrongestPillarSummary { get; set; } = string.Empty;
    public string WeakestPillarLabel { get; set; } = string.Empty;
    public string WeakestPillarSummary { get; set; } = string.Empty;
    public WvWAnalystExecutionConfidenceDto Confidence { get; set; } = new();
    public WvWAnalystExecutionContextDto Context { get; set; } = new();
    public WvWAnalystExecutionOutcomeDto Outcome { get; set; } = new();
    public IReadOnlyList<WvWAnalystExecutionPillarDto> Pillars { get; set; } = Array.Empty<WvWAnalystExecutionPillarDto>();
}

internal sealed class WvWAnalystExecutionConfidenceDto
{
    public string Label { get; set; } = string.Empty;
    public int AvailableMetricCount { get; set; }
    public int TotalMetricCount { get; set; }
    public IReadOnlyList<string> Notes { get; set; } = Array.Empty<string>();
}

internal sealed class WvWAnalystExecutionContextDto
{
    public int SquadPlayerCount { get; set; }
    public int EnemyPlayerCount { get; set; }
    public int FriendlyNonSquadCount { get; set; }
    public string PhaseDurationLabel { get; set; } = string.Empty;
    public string EnemyFormationStyleCode { get; set; } = string.Empty;
    public string EnemyFormationStyleLabel { get; set; } = string.Empty;
    public string EnemyFormationStyleDetail { get; set; } = string.Empty;
    public string DataConfidenceLabel { get; set; } = string.Empty;
    public string DataConfidenceDetail { get; set; } = string.Empty;
}

internal sealed class WvWAnalystExecutionOutcomeDto
{
    public int SquadDowns { get; set; }
    public int EnemyDowns { get; set; }
    public int SquadKills { get; set; }
    public int EnemyKills { get; set; }
    public int SquadDeaths { get; set; }
    public int EnemyDeaths { get; set; }
    public double EnemyDownConversionRate { get; set; }
    public double SquadRecoveryRate { get; set; }
    public string WipeLabel { get; set; } = string.Empty;
}

internal sealed class WvWAnalystExecutionPillarDto
{
    public string PillarId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Grade { get; set; } = string.Empty;
    public int AdjustedScore { get; set; }
    public string AdjustedGrade { get; set; } = string.Empty;
    public bool AdjustmentApplied { get; set; }
    public string AdjustmentDetail { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int AvailableMetricCount { get; set; }
    public int MetricCount { get; set; }
    public IReadOnlyList<WvWAnalystExecutionMetricDto> Metrics { get; set; } = Array.Empty<WvWAnalystExecutionMetricDto>();
}

internal sealed class WvWAnalystExecutionMetricDto
{
    public string MetricId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public bool HigherIsBetter { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool Available { get; set; }
    public bool Neutralized { get; set; }
    public int Score { get; set; }
    public WvWAnalystExecutionMetricValueDto Values { get; set; } = new();
}

internal sealed class WvWAnalystExecutionMetricValueDto
{
    public double? Squad { get; set; }
    public double? Enemy { get; set; }
}

internal sealed class WvWAnalystCommanderSummaryDto
{
    public int ActorId { get; set; }
    public bool Available { get; set; }
    public long SquadPositioningSamples { get; set; }
    public double SquadInPositionRate { get; set; }
    public double SquadTooFarRate { get; set; }
    public double SquadOverextendedRate { get; set; }
    public double SquadLateralRiskRate { get; set; }
    public int? CohesionPillarScore { get; set; }
    public string CohesionPillarSummary { get; set; } = string.Empty;
    public string FitSummary { get; set; } = string.Empty;
    public string DemandFitSummary { get; set; } = string.Empty;
    public string ContributionProfile { get; set; } = string.Empty;
    public string KeyContributionSummary { get; set; } = string.Empty;
    public string EvaluationConfidenceLabel { get; set; } = string.Empty;
    public string EvaluationConfidenceDetail { get; set; } = string.Empty;
    public IReadOnlyList<string> EvaluationCaveats { get; set; } = Array.Empty<string>();
}

internal sealed class WvWAnalystDefenseSaveSummaryDto
{
    public int SavedCases { get; set; }
    public int BarrierSavedCases { get; set; }
    public int DamageReductionSavedCases { get; set; }
    public int BothSavedCases { get; set; }
    public double TotalBarrierAbsorbed { get; set; }
    public double TotalEstimatedDamageReduction { get; set; }
    public double AverageLowestHealthPercent { get; set; }
    public double LowestLowestHealthPercent { get; set; }
    public double TotalIncomingDamage { get; set; }
    public double TotalIncomingHealing { get; set; }
}

internal sealed class WvWAnalystObliterateSummaryDto
{
    public int HitCount { get; set; }
    public int BarrierRemovedHitCount { get; set; }
}

internal sealed class WvWAnalystPlayerSummaryDto
{
    public int ActorId { get; set; }
    public string Account { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public string EliteSpec { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Group { get; set; }
    public bool IsCommander { get; set; }
    public double ActiveSeconds { get; set; }
    public double CombatSeconds { get; set; }
    public long Damage { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public int Strips { get; set; }
    public int OutgoingCleanses { get; set; }
    public long Healing { get; set; }
    public long Barrier { get; set; }
    public int Resurrects { get; set; }
    public int Deaths { get; set; }
    public int Recoveries { get; set; }
    public long DamageTaken { get; set; }
    public int ReceivedCrowdControl { get; set; }
    public bool HasPositioningData { get; set; }
    public int PositioningSamples { get; set; }
    public double InPositionRate { get; set; }
    public double TooFarRate { get; set; }
    public double OverextendedRate { get; set; }
    public double LateralRiskRate { get; set; }
    public string FitSummary { get; set; } = string.Empty;
    public string DemandFitSummary { get; set; } = string.Empty;
    public string ContributionProfile { get; set; } = string.Empty;
    public string KeyContributionSummary { get; set; } = string.Empty;
    public string EvaluationConfidenceLabel { get; set; } = string.Empty;
    public string EvaluationConfidenceDetail { get; set; } = string.Empty;
    public IReadOnlyList<string> EvaluationCaveats { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EvidenceSnapshot { get; set; } = Array.Empty<string>();
    public IReadOnlyList<WvWAnalystPlayerRoleMixEntryDto> RoleMix { get; set; } = Array.Empty<WvWAnalystPlayerRoleMixEntryDto>();
    public IReadOnlyList<WvWAnalystPlayerLaneSummaryDto> Lanes { get; set; } = Array.Empty<WvWAnalystPlayerLaneSummaryDto>();
    public IReadOnlyList<WvWAnalystPlayerThreatBoonSummaryDto> ThreatBoons { get; set; } = Array.Empty<WvWAnalystPlayerThreatBoonSummaryDto>();
    public IReadOnlyList<WvWAnalystPlayerProvidedBoonSummaryDto> ProvidedBoons { get; set; } = Array.Empty<WvWAnalystPlayerProvidedBoonSummaryDto>();
}

internal sealed class WvWAnalystPlayerRoleMixEntryDto
{
    public string Label { get; set; } = string.Empty;
    public double Percent { get; set; }
}

internal sealed class WvWAnalystPlayerLaneSummaryDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public int WindowsHit { get; set; }
    public int WindowsTotal { get; set; }
    public string WindowLabel { get; set; } = string.Empty;
    public string RateBand { get; set; } = string.Empty;
    public string EvidenceLine { get; set; } = string.Empty;
    public IReadOnlyList<WvWAnalystPlayerLaneMetricDto> Metrics { get; set; } = Array.Empty<WvWAnalystPlayerLaneMetricDto>();
}

internal sealed class WvWAnalystPlayerLaneMetricDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Aggregation { get; set; } = string.Empty;
}

internal sealed class WvWAnalystThreatBoonSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool StackBased { get; set; }
    public bool TracksOverapplication { get; set; }
    public double Coverage { get; set; }
    public double AverageStacks { get; set; }
    public double Overapplication { get; set; }
}

internal sealed class WvWAnalystTopBurstDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = string.Empty;
    public long Damage { get; set; }
    public int Strips { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public WvWAnalystTopBurstActorDto TopPressure { get; set; } = new();
    public WvWAnalystTopBurstActorDto TopStrips { get; set; } = new();
}

internal sealed class WvWAnalystTopBurstActorDto
{
    public int ActorId { get; set; }
    public string Account { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public string EliteSpec { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Amount { get; set; }
}

internal sealed class WvWAnalystPlayerThreatBoonSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool StackBased { get; set; }
    public bool TracksOverapplication { get; set; }
    public int ThreatenedSamples { get; set; }
    public int ActiveThreatSamples { get; set; }
    public double ThreatStackTotal { get; set; }
    public int OverappliedThreatSamples { get; set; }
    public double Coverage { get; set; }
    public double AverageStacks { get; set; }
    public double Overapplication { get; set; }
}

internal sealed class WvWAnalystPlayerProvidedBoonSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool StackBased { get; set; }
    public double Generation { get; set; }
    public double GenerationPresence { get; set; }
    public double Overstack { get; set; }
}
