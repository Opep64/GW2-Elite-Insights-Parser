using System.Text.Json;
using System.Globalization;
using System.Numerics;
using GW2EIBuilders.HtmlModels;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIBuilders;

public sealed class WvWAnalystBuilder
{
    private const float FightShapeMaxDistanceFromFight = 5000.0f;
    private const long ArcDpsGenericKnockbackPullSkillId = 23295;

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
        var mitigationSummary = BuildMitigationSummary(combatReplayAnalysis);
        var obliterate = BuildObliterateSummary(combatReplayAnalysis);
        var topBursts = BuildTopBursts(combatReplayAnalysis, squadPlayers, analysis => analysis.Squad);
        var enemyTopBursts = BuildTopBursts(combatReplayAnalysis, hostilePlayerTargets, analysis => analysis.Enemy);
        var enemyPlayers = BuildEnemyPlayerSummaries(log, mainPhase, hostilePlayerTargets, squadPlayers);
        var fightShape = BuildFightShapeDiagnostics(log, mainPhase, squadPlayers, hostilePlayerTargets, combatReplayAnalysis, outcome);
        var squadClasses = BuildSideClasses(squadPlayers, combatReplayAnalysis?.SpecCapabilities);
        var enemyClasses = BuildSideClasses(hostilePlayerTargets);

        return new WvWAnalystFightPayloadDto
        {
            Meta = new WvWAnalystMetaDto
            {
                SchemaVersion = "1.24.0",
                PayloadType = "wvw-analyst-fight",
                DetailLevel = "summary+players+boons+lane-metrics+player-fight-impact+spec-fight-coverage+player-boons+provided-boons+top-bursts+enemy-player-performance+enemy-top-bursts+defense-saves+mitigation-summary+negated-hits+shield-of-courage+obliterate+side-classes+fight-shape-diagnostics+enemy-movement-score+three-way-context",
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
                    Classes = squadClasses,
                    Totals = BuildSideTotals(summary.Squad),
                },
                Enemy = new WvWAnalystSideDto
                {
                    SideId = "enemy",
                    DisplayLabel = summary.Enemy.Label,
                    PlayerCount = summary.Enemy.PlayerCount,
                    Classes = enemyClasses,
                    Totals = BuildSideTotals(summary.Enemy),
                }
            },
            Outcome = outcome,
            Execution = BuildExecution(summary),
            CommanderSummary = commanderSummary,
            DefenseSaves = defenseSaves,
            MitigationSummary = mitigationSummary,
            Obliterate = obliterate,
            FightShape = fightShape,
            ThreatBoons = threatBoons,
            TopBursts = topBursts,
            EnemyTopBursts = enemyTopBursts,
            Players = players,
            EnemyPlayers = enemyPlayers,
        };
    }

    // Shared by the analyst payload and the EI Summary Moments marker. Keep the detector post-fight and conservative:
    // it identifies when a side can no longer meaningfully contest, not who performed well.
    internal static WvWAnalystFightShapeDto BuildFightShapeDiagnostics(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        WvWAnalystOutcomeDto outcome)
    {
        const long minimumElapsedMs = 10000;
        const long minimumCleanupMs = 10000;
        const long sustainMs = 8000;
        const long lateRecoverySampleMs = 3000;
        const double minimumConfidence = 0.65;
        const double minimumRelaxedSustainConfidence = 0.72;

        long phaseStart = phase.Start;
        long phaseEnd = phase.End;
        long durationMs = Math.Max(phase.DurationInMS, 0);
        if (combatReplayAnalysis is null ||
            combatReplayAnalysis.Times.Length == 0 ||
            durationMs < minimumElapsedMs + minimumCleanupMs ||
            squadPlayers.Count == 0 ||
            hostilePlayerTargets.Count == 0)
        {
            return BuildUnknownFightShape(durationMs, "Insufficient detailed replay data for cleanup diagnostics.");
        }

        IReadOnlyDictionary<int, long> squadKillTimes = BuildFightShapeKillTimes(combatReplayAnalysis.Events.Kills.Events, isEnemy: false);
        IReadOnlyDictionary<int, long> enemyKillTimes = BuildFightShapeKillTimes(combatReplayAnalysis.Events.Kills.Events, isEnemy: true);

        FightShapeCandidateDiagnostics? bestCandidate = null;
        foreach (long time in combatReplayAnalysis.Times)
        {
            if (time < phaseStart + minimumElapsedMs || time > phaseEnd - minimumCleanupMs)
            {
                continue;
            }

            WvWAnalystFightShapeSideStateDto squadState = BuildFightShapeSideState(log, squadPlayers, squadKillTimes, hostilePlayerTargets, time);
            WvWAnalystFightShapeSideStateDto enemyState = BuildFightShapeSideState(log, hostilePlayerTargets, enemyKillTimes, squadPlayers, time);
            if (!HasEnoughKnownActors(squadState) || !HasEnoughKnownActors(enemyState))
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = "unknown",
                        Confidence = 0.0,
                        FailureRank = 1,
                        FailureReason = BuildKnownActorsFailureReason(squadState, enemyState),
                        FailureDetail = BuildFightShapeCandidateDetail(squadState, enemyState),
                    });
                continue;
            }

            bool squadAdvantage = HasCleanupBodyAdvantage(squadState, enemyState);
            bool enemyAdvantage = HasCleanupBodyAdvantage(enemyState, squadState);
            string cleanupSide = squadAdvantage && !enemyAdvantage
                ? "squad"
                : enemyAdvantage && !squadAdvantage
                    ? "enemy"
                    : "unknown";
            if (cleanupSide == "unknown")
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = "unknown",
                        Confidence = 0.0,
                        FailureRank = 2,
                        FailureReason = squadAdvantage && enemyAdvantage ? "ambiguous_body_advantage" : "no_body_advantage",
                        FailureDetail = BuildFightShapeCandidateDetail(squadState, enemyState),
                    });
                continue;
            }

            bool cleanupMatchesFinalWinner = HasKnownWinner(outcome) && IsCleanupSideFinalWinner(cleanupSide, outcome);
            if (HasKnownWinner(outcome) && !cleanupMatchesFinalWinner)
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = cleanupSide,
                        Confidence = ComputeFightShapeBodyConfidence(
                            cleanupSide == "squad" ? squadState : enemyState,
                            cleanupSide == "squad" ? enemyState : squadState),
                        FailureRank = 4,
                        FailureReason = "cleanup_side_not_final_winner",
                        FailureDetail = BuildFightShapeCandidateDetail(squadState, enemyState),
                    });
                continue;
            }

            long sustainTime = Math.Min(time + sustainMs, phaseEnd);
            WvWAnalystFightShapeSideStateDto sustainedSquadState = BuildFightShapeSideState(log, squadPlayers, squadKillTimes, hostilePlayerTargets, sustainTime);
            WvWAnalystFightShapeSideStateDto sustainedEnemyState = BuildFightShapeSideState(log, hostilePlayerTargets, enemyKillTimes, squadPlayers, sustainTime);
            bool hasSustainedBodyAdvantage = cleanupSide == "squad"
                ? HasCleanupBodyAdvantage(sustainedSquadState, sustainedEnemyState)
                : HasCleanupBodyAdvantage(sustainedEnemyState, sustainedSquadState);

            WvWAnalystFightShapeEventSnapshotDto afterCleanup = BuildFightShapeEventSnapshot(combatReplayAnalysis.Events, time, phaseEnd, includeStart: false);
            (long squadDamageAfter, long enemyDamageAfter) = ComputeDamageSplit(log, squadPlayers, hostilePlayerTargets, time, phaseEnd);
            afterCleanup.SquadDamage = squadDamageAfter;
            afterCleanup.EnemyDamage = enemyDamageAfter;
            string loserSide = cleanupSide == "squad" ? "enemy" : "squad";
            int loserCounterDownsAfter = cleanupSide == "squad" ? afterCleanup.SquadMembersDowned : afterCleanup.EnemyPlayersDowned;
            int loserCounterKillsAfter = cleanupSide == "squad" ? afterCleanup.EnemyKillsSecured : afterCleanup.SquadKillsSecured;
            int winnerDownsAfter = cleanupSide == "squad" ? afterCleanup.EnemyPlayersDowned : afterCleanup.SquadMembersDowned;
            int winnerKillsAfter = cleanupSide == "squad" ? afterCleanup.SquadKillsSecured : afterCleanup.EnemyKillsSecured;
            int loserRecoveriesAfter = cleanupSide == "squad" ? afterCleanup.EnemyRecoveries : afterCleanup.SquadRecoveries;
            long winnerDamageAfter = cleanupSide == "squad" ? squadDamageAfter : enemyDamageAfter;
            long loserDamageAfter = cleanupSide == "squad" ? enemyDamageAfter : squadDamageAfter;
            double confidence = ComputeFightShapeConfidence(
                cleanupSide == "squad" ? squadState : enemyState,
                cleanupSide == "squad" ? enemyState : squadState,
                winnerDamageAfter,
                loserDamageAfter,
                loserCounterDownsAfter,
                loserCounterKillsAfter,
                phaseEnd - time);
            if (!HasNoLateComeback(cleanupSide, afterCleanup))
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = cleanupSide,
                        Confidence = confidence,
                        FailureRank = 5,
                        FailureReason = "late_comeback",
                        FailureDetail = $"{BuildFightShapeCandidateDetail(squadState, enemyState)}; {BuildFightShapeAfterDetail(afterCleanup)}",
                    });
                continue;
            }

            FightShapeLateRecoveryDiagnostics? lateRecovery = FindLateBodyRecovery(
                log,
                combatReplayAnalysis.Times,
                time + sustainMs,
                phaseEnd,
                lateRecoverySampleMs,
                cleanupSide,
                squadPlayers,
                hostilePlayerTargets,
                squadKillTimes,
                enemyKillTimes);
            if (lateRecovery is not null)
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = cleanupSide,
                        Confidence = confidence,
                        FailureRank = 5,
                        FailureReason = "late_body_recovery",
                        FailureDetail = $"{BuildFightShapeCandidateDetail(squadState, enemyState)}; {BuildFightShapeLateRecoveryDetail(lateRecovery)}",
                    });
                continue;
            }

            bool relaxedSustain = false;
            if (!hasSustainedBodyAdvantage)
            {
                if (cleanupMatchesFinalWinner && confidence >= minimumRelaxedSustainConfidence)
                {
                    relaxedSustain = true;
                }
                else
                {
                    bestCandidate = SelectBetterFightShapeCandidate(
                        bestCandidate,
                        new FightShapeCandidateDiagnostics
                        {
                            TimeMs = time,
                            CleanupSide = cleanupSide,
                            Confidence = confidence,
                            FailureRank = 3,
                            FailureReason = "no_sustained_body_advantage",
                            FailureDetail = $"{BuildFightShapeCandidateDetail(squadState, enemyState, sustainedSquadState, sustainedEnemyState)}; {BuildFightShapeAfterDetail(afterCleanup)}",
                        });
                    continue;
                }
            }

            if (confidence < minimumConfidence)
            {
                bestCandidate = SelectBetterFightShapeCandidate(
                    bestCandidate,
                    new FightShapeCandidateDiagnostics
                    {
                        TimeMs = time,
                        CleanupSide = cleanupSide,
                        Confidence = confidence,
                        FailureRank = 6,
                        FailureReason = "confidence_below_minimum",
                        FailureDetail = $"{BuildFightShapeCandidateDetail(squadState, enemyState)}; {BuildFightShapeAfterDetail(afterCleanup)}",
                    });
                continue;
            }

            WvWAnalystFightShapeEventSnapshotDto atCleanup = BuildFightShapeEventSnapshot(combatReplayAnalysis.Events, phaseStart, time, includeStart: true);
            (long squadDamageBefore, long enemyDamageBefore) = ComputeDamageSplit(log, squadPlayers, hostilePlayerTargets, phaseStart, time);
            atCleanup.SquadDamage = squadDamageBefore;
            atCleanup.EnemyDamage = enemyDamageBefore;

            List<string> rules =
            [
                "no_late_comeback"
            ];
            rules.Insert(0, relaxedSustain ? "relaxed_sustain_final_winner" : "sustained_body_advantage");
            if (winnerDamageAfter + loserDamageAfter > 0 && loserDamageAfter <= (winnerDamageAfter + loserDamageAfter) * 0.25)
            {
                rules.Add("pressure_collapse");
            }
            if (winnerDownsAfter + winnerKillsAfter >= 2)
            {
                rules.Add("cleanup_momentum");
            }
            if (winnerDownsAfter > 0 && loserRecoveriesAfter == 0)
            {
                rules.Add("recovery_collapse");
            }

            long cleanupDurationMs = Math.Max(phaseEnd - time, 0);
            return new WvWAnalystFightShapeDto
            {
                Available = true,
                DetectionLabel = "Cleanup candidate",
                CleanupSide = cleanupSide,
                LosingSide = loserSide,
                Confidence = confidence,
                CompetitiveEndTimeMs = time,
                CleanupStartTimeMs = time,
                CompetitiveDurationMs = Math.Max(time - phaseStart, 0),
                CleanupDurationMs = cleanupDurationMs,
                CleanupPercent = durationMs > 0 ? Math.Round(cleanupDurationMs * 100.0 / durationMs, 1) : 0.0,
                Rules = rules,
                BestCandidateTimeMs = time,
                BestCandidateCleanupSide = cleanupSide,
                BestCandidateConfidence = confidence,
                BestCandidateReason = relaxedSustain ? "accepted_relaxed_sustain" : "accepted",
                BestCandidateDetail = relaxedSustain
                    ? $"{BuildFightShapeCandidateDetail(squadState, enemyState, sustainedSquadState, sustainedEnemyState)}; {BuildFightShapeAfterDetail(afterCleanup)}"
                    : BuildFightShapeCandidateDetail(squadState, enemyState),
                AtCleanupStart = atCleanup,
                AfterCleanupStart = afterCleanup,
                SquadAtCleanupStart = squadState,
                EnemyAtCleanupStart = enemyState,
            };
        }

        return BuildUnknownFightShape(durationMs, "No conservative cleanup boundary detected.", bestCandidate);
    }

    private static IReadOnlyDictionary<int, long> BuildFightShapeKillTimes(
        IReadOnlyList<CombatReplayKillEventDto> killEvents,
        bool isEnemy)
    {
        return killEvents
            .Where(evt => evt.IsEnemy == isEnemy)
            .GroupBy(evt => evt.ActorId)
            .ToDictionary(
                group => group.Key,
                group => group.Min(evt => evt.OutcomeTime ?? evt.Time));
    }

    private static WvWAnalystFightShapeDto BuildUnknownFightShape(
        long durationMs,
        string detail,
        FightShapeCandidateDiagnostics? bestCandidate = null)
    {
        return new WvWAnalystFightShapeDto
        {
            Available = false,
            DetectionLabel = detail,
            CleanupSide = "unknown",
            LosingSide = "unknown",
            Confidence = 0.0,
            CompetitiveDurationMs = durationMs,
            CleanupDurationMs = 0,
            CleanupPercent = 0.0,
            Rules = ["diagnostic_unknown"],
            BestCandidateTimeMs = bestCandidate?.TimeMs,
            BestCandidateCleanupSide = bestCandidate?.CleanupSide ?? string.Empty,
            BestCandidateConfidence = bestCandidate?.Confidence ?? 0.0,
            BestCandidateReason = bestCandidate?.FailureReason ?? string.Empty,
            BestCandidateDetail = bestCandidate?.FailureDetail ?? string.Empty,
        };
    }

    private static FightShapeCandidateDiagnostics SelectBetterFightShapeCandidate(
        FightShapeCandidateDiagnostics? current,
        FightShapeCandidateDiagnostics candidate)
    {
        if (current is null)
        {
            return candidate;
        }
        if (candidate.FailureRank != current.FailureRank)
        {
            return candidate.FailureRank > current.FailureRank ? candidate : current;
        }
        if (Math.Abs(candidate.Confidence - current.Confidence) > double.Epsilon)
        {
            return candidate.Confidence > current.Confidence ? candidate : current;
        }
        return candidate.TimeMs < current.TimeMs ? candidate : current;
    }

    private static string BuildKnownActorsFailureReason(
        WvWAnalystFightShapeSideStateDto squadState,
        WvWAnalystFightShapeSideStateDto enemyState)
    {
        bool squadKnown = HasEnoughKnownActors(squadState);
        bool enemyKnown = HasEnoughKnownActors(enemyState);
        if (!squadKnown && !enemyKnown)
        {
            return "insufficient_known_both";
        }
        return squadKnown ? "insufficient_known_enemy" : "insufficient_known_squad";
    }

    private static string BuildFightShapeCandidateDetail(
        WvWAnalystFightShapeSideStateDto squadState,
        WvWAnalystFightShapeSideStateDto enemyState,
        WvWAnalystFightShapeSideStateDto? sustainedSquadState = null,
        WvWAnalystFightShapeSideStateDto? sustainedEnemyState = null)
    {
        string current = string.Format(
            CultureInfo.InvariantCulture,
            "current squad known {0}/{1}, active {2}, downed {3}, deadDc {4}, removed {5}, far {6}, unobserved {7}; enemy known {8}/{9}, active {10}, downed {11}, deadDc {12}, removed {13}, far {14}, unobserved {15}",
            squadState.Known,
            squadState.Total,
            squadState.Active,
            squadState.Downed,
            squadState.DeadOrDc,
            squadState.Removed,
            squadState.FarFromFight,
            squadState.Unobserved,
            enemyState.Known,
            enemyState.Total,
            enemyState.Active,
            enemyState.Downed,
            enemyState.DeadOrDc,
            enemyState.Removed,
            enemyState.FarFromFight,
            enemyState.Unobserved);
        if (sustainedSquadState is null || sustainedEnemyState is null)
        {
            return current;
        }
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}; sustained squad known {1}/{2}, active {3}, downed {4}, deadDc {5}, removed {6}, far {7}, unobserved {8}; enemy known {9}/{10}, active {11}, downed {12}, deadDc {13}, removed {14}, far {15}, unobserved {16}",
            current,
            sustainedSquadState.Known,
            sustainedSquadState.Total,
            sustainedSquadState.Active,
            sustainedSquadState.Downed,
            sustainedSquadState.DeadOrDc,
            sustainedSquadState.Removed,
            sustainedSquadState.FarFromFight,
            sustainedSquadState.Unobserved,
            sustainedEnemyState.Known,
            sustainedEnemyState.Total,
            sustainedEnemyState.Active,
            sustainedEnemyState.Downed,
            sustainedEnemyState.DeadOrDc,
            sustainedEnemyState.Removed,
            sustainedEnemyState.FarFromFight,
            sustainedEnemyState.Unobserved);
    }

    private static string BuildFightShapeAfterDetail(WvWAnalystFightShapeEventSnapshotDto afterCleanup)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "after downs S/E {0}/{1}, kills S/E {2}/{3}, recoveries S/E {4}/{5}, damage S/E {6}/{7}",
            afterCleanup.SquadMembersDowned,
            afterCleanup.EnemyPlayersDowned,
            afterCleanup.SquadKillsSecured,
            afterCleanup.EnemyKillsSecured,
            afterCleanup.SquadRecoveries,
            afterCleanup.EnemyRecoveries,
            afterCleanup.SquadDamage,
            afterCleanup.EnemyDamage);
    }

    private static string BuildFightShapeLateRecoveryDetail(FightShapeLateRecoveryDiagnostics recovery)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "late body recovery at {0}ms: squad active {1}, downed {2}, deadDc {3}, far {4}; enemy active {5}, downed {6}, deadDc {7}, far {8}",
            recovery.TimeMs,
            recovery.SquadState.Active,
            recovery.SquadState.Downed,
            recovery.SquadState.DeadOrDc,
            recovery.SquadState.FarFromFight,
            recovery.EnemyState.Active,
            recovery.EnemyState.Downed,
            recovery.EnemyState.DeadOrDc,
            recovery.EnemyState.FarFromFight);
    }

    private static WvWAnalystFightShapeSideStateDto BuildFightShapeSideState(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        IReadOnlyDictionary<int, long> killTimes,
        IReadOnlyList<SingleActor> opposingActors,
        long time)
    {
        var result = new WvWAnalystFightShapeSideStateDto
        {
            Total = actors.Count,
        };
        IReadOnlyList<Vector3> opposingPositions = BuildFightShapeActivePositions(log, opposingActors, time);
        foreach (SingleActor actor in actors)
        {
            if (actor.FirstAware > time)
            {
                continue;
            }

            if (time > actor.LastAware)
            {
                if (killTimes.TryGetValue(actor.UniqueID, out long killTime) && killTime <= time)
                {
                    result.Known++;
                    result.Removed++;
                    continue;
                }

                result.Unobserved++;
                continue;
            }

            bool isDead = actor.IsDead(log, time);
            bool isDc = actor.IsDC(log, time);
            bool isDowned = actor.IsDowned(log, time);
            result.Known++;
            if (isDead || isDc)
            {
                result.DeadOrDc++;
            }
            else if (isDowned)
            {
                result.Downed++;
            }
            else if (IsFarFromFight(log, actor, opposingPositions, time))
            {
                result.FarFromFight++;
            }
            else
            {
                result.Active++;
            }
        }
        return result;
    }

    private static IReadOnlyList<Vector3> BuildFightShapeActivePositions(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        long time)
    {
        var positions = new List<Vector3>();
        foreach (SingleActor actor in actors)
        {
            if (TryGetFightShapeActivePosition(actor, log, time, out Vector3 position))
            {
                positions.Add(position);
            }
        }
        return positions;
    }

    private static bool IsFarFromFight(
        ParsedEvtcLog log,
        SingleActor actor,
        IReadOnlyList<Vector3> opposingPositions,
        long time)
    {
        if (opposingPositions.Count == 0 ||
            !TryGetFightShapeActivePosition(actor, log, time, out Vector3 position))
        {
            return false;
        }

        return !opposingPositions.Any(opposingPosition => IsWithinFightShapeRange(position, opposingPosition, FightShapeMaxDistanceFromFight));
    }

    private static bool TryGetFightShapeActivePosition(SingleActor actor, ParsedEvtcLog log, long time, out Vector3 position)
    {
        position = default;
        if (time < actor.FirstAware ||
            time > actor.LastAware ||
            actor.IsDowned(log, time) ||
            actor.IsDead(log, time) ||
            actor.IsDC(log, time))
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

    private static bool IsWithinFightShapeRange(Vector3 left, Vector3 right, float range)
    {
        float dx = left.X - right.X;
        float dy = left.Y - right.Y;
        return dx * dx + dy * dy <= range * range;
    }

    private static bool HasEnoughKnownActors(WvWAnalystFightShapeSideStateDto state)
    {
        return state.Total <= 0 || state.Known >= Math.Max(1, (int)Math.Ceiling(state.Total * 0.70));
    }

    private static bool HasKnownWinner(WvWAnalystOutcomeDto outcome)
    {
        return string.Equals(outcome.WinnerSideId, "squad", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(outcome.WinnerSideId, "enemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCleanupSideFinalWinner(string cleanupSide, WvWAnalystOutcomeDto outcome)
    {
        return string.Equals(cleanupSide, outcome.WinnerSideId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasCleanupBodyAdvantage(WvWAnalystFightShapeSideStateDto leading, WvWAnalystFightShapeSideStateDto trailing)
    {
        if (leading.Active < 5 || trailing.Known == 0)
        {
            return false;
        }

        int activeGap = leading.Active - trailing.Active;
        double activeRatio = leading.Active / Math.Max((double)trailing.Active, 1.0);
        int trailingActiveLimit = Math.Max(3, (int)Math.Ceiling(trailing.Known * 0.35));
        return activeGap >= 4 &&
            activeRatio >= 1.75 &&
            trailing.Active <= trailingActiveLimit;
    }

    private static FightShapeLateRecoveryDiagnostics? FindLateBodyRecovery(
        ParsedEvtcLog log,
        IReadOnlyList<long> times,
        long start,
        long end,
        long sampleInterval,
        string cleanupSide,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        IReadOnlyDictionary<int, long> squadKillTimes,
        IReadOnlyDictionary<int, long> enemyKillTimes)
    {
        long nextSampleTime = start;
        foreach (long time in times)
        {
            if (time < nextSampleTime)
            {
                continue;
            }
            if (time > end)
            {
                break;
            }
            nextSampleTime = time + sampleInterval;

            WvWAnalystFightShapeSideStateDto squadState = BuildFightShapeSideState(log, squadPlayers, squadKillTimes, hostilePlayerTargets, time);
            WvWAnalystFightShapeSideStateDto enemyState = BuildFightShapeSideState(log, hostilePlayerTargets, enemyKillTimes, squadPlayers, time);
            WvWAnalystFightShapeSideStateDto leading = cleanupSide == "squad" ? squadState : enemyState;
            WvWAnalystFightShapeSideStateDto trailing = cleanupSide == "squad" ? enemyState : squadState;
            if (HasLateBodyRecovery(leading, trailing))
            {
                return new FightShapeLateRecoveryDiagnostics(time, squadState, enemyState);
            }
        }

        return null;
    }

    private static bool HasLateBodyRecovery(WvWAnalystFightShapeSideStateDto leading, WvWAnalystFightShapeSideStateDto trailing)
    {
        if (leading.Active <= 0 || trailing.Active < 5)
        {
            return false;
        }

        int activeGap = leading.Active - trailing.Active;
        double trailingRatio = trailing.Active / Math.Max((double)leading.Active, 1.0);
        return activeGap <= 3 || trailingRatio >= 0.70;
    }

    private static WvWAnalystFightShapeEventSnapshotDto BuildFightShapeEventSnapshot(
        CombatReplayEventAnalysisDto eventAnalysis,
        long start,
        long end,
        bool includeStart)
    {
        bool InWindow(long eventTime) => includeStart
            ? eventTime >= start && eventTime <= end
            : eventTime > start && eventTime <= end;

        var result = new WvWAnalystFightShapeEventSnapshotDto();
        foreach (CombatReplayDownEventDto downEvent in eventAnalysis.Downs.Events.Where(evt => InWindow(evt.Time)))
        {
            if (downEvent.IsEnemy)
            {
                result.EnemyPlayersDowned++;
            }
            else
            {
                result.SquadMembersDowned++;
            }
        }
        foreach (CombatReplayKillEventDto killEvent in eventAnalysis.Kills.Events.Where(evt => InWindow(evt.OutcomeTime ?? evt.Time)))
        {
            if (killEvent.IsEnemy)
            {
                result.SquadKillsSecured++;
            }
            else
            {
                result.EnemyKillsSecured++;
            }
        }
        foreach (CombatReplayRecoveredEventDto recoveryEvent in eventAnalysis.Recovered.Events.Where(evt => InWindow(evt.OutcomeTime ?? evt.Time)))
        {
            if (recoveryEvent.IsEnemy)
            {
                result.EnemyRecoveries++;
            }
            else
            {
                result.SquadRecoveries++;
            }
        }
        return result;
    }

    private static bool HasNoLateComeback(string cleanupSide, WvWAnalystFightShapeEventSnapshotDto afterCleanup)
    {
        int counterDowns;
        int counterKills;
        int winnerDowns;
        int winnerKills;
        if (cleanupSide == "squad")
        {
            counterDowns = afterCleanup.SquadMembersDowned;
            counterKills = afterCleanup.EnemyKillsSecured;
            winnerDowns = afterCleanup.EnemyPlayersDowned;
            winnerKills = afterCleanup.SquadKillsSecured;
        }
        else
        {
            counterDowns = afterCleanup.EnemyPlayersDowned;
            counterKills = afterCleanup.SquadKillsSecured;
            winnerDowns = afterCleanup.SquadMembersDowned;
            winnerKills = afterCleanup.EnemyKillsSecured;
        }

        if (counterKills >= 2 || counterDowns >= 4)
        {
            return false;
        }

        return counterKills == 0 ||
            winnerKills + winnerDowns >= Math.Max(2, counterKills + counterDowns);
    }

    private static (long SquadDamage, long EnemyDamage) ComputeDamageSplit(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        long start,
        long end)
    {
        long squadDamage = ComputeOutgoingDamage(log, squadPlayers, hostilePlayerTargets, start, end);
        long enemyDamage = ComputeOutgoingDamage(log, hostilePlayerTargets, squadPlayers, start, end);
        return (squadDamage, enemyDamage);
    }

    private static long ComputeOutgoingDamage(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> attackers,
        IReadOnlyList<SingleActor> targets,
        long start,
        long end)
    {
        long total = 0;
        foreach (SingleActor attacker in attackers)
        {
            foreach (SingleActor target in targets)
            {
                total += attacker.GetDamageStats(target, log, start, end).Damage;
            }
        }
        return total;
    }

    private static double ComputeFightShapeConfidence(
        WvWAnalystFightShapeSideStateDto leading,
        WvWAnalystFightShapeSideStateDto trailing,
        long winnerDamageAfter,
        long loserDamageAfter,
        int loserCounterDownsAfter,
        int loserCounterKillsAfter,
        long cleanupDurationMs)
    {
        double activeGapScore = Math.Clamp((leading.Active - trailing.Active) / Math.Max((double)leading.Known, 1.0), 0.0, 1.0);
        double trailingCollapseScore = 1.0 - Math.Clamp(trailing.Active / Math.Max((double)trailing.Known, 1.0), 0.0, 1.0);
        double bodyScore = activeGapScore * 0.60 + trailingCollapseScore * 0.40;
        double comebackScore = loserCounterKillsAfter == 0
            ? loserCounterDownsAfter == 0 ? 1.0 : 0.75
            : 0.25;
        double totalDamageAfter = winnerDamageAfter + loserDamageAfter;
        double pressureScore = totalDamageAfter > 0
            ? 1.0 - Math.Clamp(loserDamageAfter / totalDamageAfter, 0.0, 1.0)
            : 0.60;
        double durationScore = Math.Clamp(cleanupDurationMs / 20000.0, 0.0, 1.0);

        return Math.Round(bodyScore * 0.35 + comebackScore * 0.25 + pressureScore * 0.25 + durationScore * 0.15, 2);
    }

    private static double ComputeFightShapeBodyConfidence(
        WvWAnalystFightShapeSideStateDto leading,
        WvWAnalystFightShapeSideStateDto trailing)
    {
        double activeGapScore = Math.Clamp((leading.Active - trailing.Active) / Math.Max((double)leading.Known, 1.0), 0.0, 1.0);
        double trailingCollapseScore = 1.0 - Math.Clamp(trailing.Active / Math.Max((double)trailing.Known, 1.0), 0.0, 1.0);
        return Math.Round(activeGapScore * 0.60 + trailingCollapseScore * 0.40, 2);
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
            Corrupts = side.BoonCorrupts,
            CorruptPercent = ComputePercent(side.BoonCorrupts, side.BoonStrips),
            ReceivedCrowdControl = side.ReceivedCrowdControl,
            StripsPerMinute = side.StripsPerMinute,
            CorruptsPerMinute = side.CorruptsPerMinute,
            CleansesPerMinute = side.CleansesPerMinute,
        };
    }

    private static IReadOnlyList<WvWAnalystSideClassSummaryDto> BuildSideClasses(
        IReadOnlyList<SingleActor> actors,
        IReadOnlyList<CombatReplaySpecCapabilityDto>? specCapabilities = null)
    {
        var specCapabilityByLabel = (specCapabilities ?? Array.Empty<CombatReplaySpecCapabilityDto>())
            .Where(spec => !string.IsNullOrWhiteSpace(spec.Label))
            .GroupBy(spec => spec.Label, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return actors
            .Select(actor => new
            {
                ClassLabel = BuildClassLabel(actor.BaseSpec.ToString(), actor.Spec.ToString()),
                Icon = actor.GetIcon(),
            })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.ClassLabel))
            .GroupBy(entry => entry.ClassLabel, StringComparer.OrdinalIgnoreCase)
            .Select(group => new WvWAnalystSideClassSummaryDto
            {
                ClassLabel = group.First().ClassLabel,
                Icon = group.Select(entry => entry.Icon).FirstOrDefault(icon => !string.IsNullOrWhiteSpace(icon)) ?? string.Empty,
                Count = group.Count(),
                FightCoverage = BuildSpecFightCoverage(specCapabilityByLabel.GetValueOrDefault(group.First().ClassLabel)),
            })
            .OrderByDescending(entry => entry.Count)
            .ThenBy(entry => entry.ClassLabel, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static WvWAnalystSpecFightCoverageDto? BuildSpecFightCoverage(CombatReplaySpecCapabilityDto? spec)
    {
        CombatReplaySpecFightCoverageDto? coverage = spec?.FightCoverage;
        if (coverage is null || coverage.Score <= 0.0)
        {
            return null;
        }

        return new WvWAnalystSpecFightCoverageDto
        {
            Score = coverage.Score,
            Label = coverage.Label,
            Summary = coverage.Summary,
            Detail = coverage.Detail,
            Caveats = coverage.Caveats?.ToArray() ?? Array.Empty<string>(),
            Lanes = coverage.Lanes?
                .Where(lane => lane.CoverageScore > 0.0 || lane.StrengthPercent > 0.0)
                .OrderByDescending(lane => lane.CoverageScore)
                .ThenByDescending(lane => lane.DemandScorePercent)
                .ThenByDescending(lane => lane.StrengthPercent)
                .Select(lane => new WvWAnalystSpecFightCoverageLaneDto
                {
                    Key = lane.Key,
                    Label = lane.Label,
                    StrengthPercent = lane.StrengthPercent,
                    SharePercent = lane.SharePercent,
                    PerSlotEfficiency = lane.PerSlotEfficiency,
                    PlayersContributing = lane.PlayersContributing,
                    PlayerCount = lane.PlayerCount,
                    DemandScorePercent = lane.DemandScorePercent,
                    DemandLabel = lane.DemandLabel,
                    DemandWeightPercent = lane.DemandWeightPercent,
                    CoverageScore = lane.CoverageScore,
                    EvidenceLine = lane.EvidenceLine,
                })
                .ToArray()
                ?? Array.Empty<WvWAnalystSpecFightCoverageLaneDto>(),
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
                EnemyMovementScore = execution.Context.EnemyMovementScore,
                EnemyMovementScoreLabel = execution.Context.EnemyMovementScoreLabel,
                EnemyMovementScoreDetail = execution.Context.EnemyMovementScoreDetail,
                EnemyMovementCenterTightShare = execution.Context.EnemyMovementCenterTightShare,
                EnemyMovementAverageDistanceToCenter = execution.Context.EnemyMovementAverageDistanceToCenter,
                EnemyMovementSampleCount = execution.Context.EnemyMovementSampleCount,
                ThreeWayDetected = execution.Context.ThreeWayDetected,
                ThreeWayLabel = execution.Context.ThreeWayLabel,
                ThreeWayDetail = execution.Context.ThreeWayDetail,
                ThreeWayStartTimeMs = execution.Context.ThreeWayStartTimeMs,
                ThreeWaySecondEnemyPeakCount = execution.Context.ThreeWaySecondEnemyPeakCount,
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
                CrowdControlDataAvailable = execution.Outcome.CrowdControlDataAvailable,
                IncomingCrowdControl = execution.Outcome.IncomingCrowdControl,
                OutgoingCrowdControl = execution.Outcome.OutgoingCrowdControl,
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
        Dictionary<AgentItem, int> corruptsByAttacker = CombatReplayAnalysisBuilder.CountBoonCorruptsByAttacker(
            log,
            phase.Start,
            phase.End,
            squadPlayers,
            hostilePlayerTargets);

        return squadPlayers
            .Select(player => BuildPlayerSummary(log, phase, player, squadPlayers, hostilePlayerTargets, combatReplayAnalysis, corruptsByAttacker, player.UniqueID == commanderId))
            .OrderByDescending(player => player.IsCommander)
            .ThenBy(player => player.Group)
            .ThenBy(player => player.Character, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static WvWAnalystPlayerSummaryDto BuildPlayerSummary(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        CombatReplayAnalysisDto? combatReplayAnalysis,
        IReadOnlyDictionary<AgentItem, int> corruptsByAttacker,
        bool isCommander)
    {
        SupportStatistics support = player.GetToAllySupportStats(log, phase.Start, phase.End);
        DefenseAllStatistics defense = player.GetDefenseStats(log, phase.Start, phase.End);
        long damage = 0;
        long damageToDownedTargets = 0;
        int downs = 0;
        int kills = 0;
        int downContribution = 0;
        foreach (SingleActor target in hostilePlayerTargets)
        {
            DamageStatistics damageStats = player.GetDamageStats(target, log, phase.Start, phase.End);
            damage += damageStats.Damage;
            OffensiveStatistics offensive = player.GetOffensiveStats(target, log, phase.Start, phase.End);
            damageToDownedTargets += offensive.AgainstDownedDamage;
            downs += offensive.DownedCount;
            kills += offensive.KilledCount;
            downContribution += offensive.DownContribution;
        }

        CombatReplayPlayerEvaluationDto? evaluation = TryGetPlayerEvaluation(combatReplayAnalysis, player.UniqueID);
        CombatReplayPositioningPlayerTimelineDto? positioningTimeline = TryGetPositioningTimeline(combatReplayAnalysis, player.UniqueID);
        int corrupts = corruptsByAttacker.GetValueOrDefault(player.EnglobingAgentItem);
        var outgoingHealing = log.CombatData.HasEXTHealing
            ? player.EXTHealing.GetOutgoingHealStats(null, log, phase.Start, phase.End)
            : null;

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
            DamageToDownedTargets = damageToDownedTargets,
            Downs = downs,
            Kills = kills,
            DownContribution = downContribution,
            Strips = support.BoonStripCount,
            Corrupts = corrupts,
            CorruptPercent = ComputePercent(corrupts, support.BoonStripCount),
            OutgoingCleanses = support.ConditionCleanseCount,
            Healing = outgoingHealing?.Healing ?? 0,
            DownedHealing = outgoingHealing?.DownedHealing ?? 0,
            Barrier = log.CombatData.HasEXTBarrier ? player.EXTBarrier.GetOutgoingBarrierStats(null, log, phase.Start, phase.End).Barrier : 0,
            Resurrects = support.ResurrectCount,
            IllusionOfLifeRezzes = CountIllusionOfLifeRezzes(log, phase, player, squadPlayers),
            Deaths = defense.DeadCount,
            Recoveries = BuildPlayerRecoveryCount(log, player, phase),
            DamageTaken = defense.DamageTaken,
            PetDamageAbsorbed = (long)Math.Round(GetPlayerLaneMetricValue(evaluation, "petAbsorptionTotal")),
            DamageReflectedOnEnemy = GetPlayerReflectDamageOnEnemy(combatReplayAnalysis, player.UniqueID),
            MysticRebukeDamage = ComputeMysticRebukeDamage(log, phase, player, hostilePlayerTargets),
            Pulls = CountPulls(log, phase, player, hostilePlayerTargets),
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
            FightImpactScore = evaluation?.FightImpact.Score ?? 0.0,
            FightImpactLabel = evaluation?.FightImpact.Label ?? string.Empty,
            FightImpactSummary = evaluation?.FightImpact.Summary ?? string.Empty,
            FightImpactDetail = evaluation?.FightImpact.Detail ?? string.Empty,
            FightImpactConfidenceLabel = evaluation?.FightImpact.ConfidenceLabel ?? string.Empty,
            FightImpactCaveats = evaluation?.FightImpact.Caveats?.ToArray() ?? Array.Empty<string>(),
            FightImpactLanes = BuildPlayerFightImpactLanes(evaluation),
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

    private static IReadOnlyList<WvWAnalystEnemyPlayerSummaryDto> BuildEnemyPlayerSummaries(
        ParsedEvtcLog log,
        PhaseData phase,
        IReadOnlyList<SingleActor> hostilePlayerTargets,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        double durationSeconds = Math.Max(phase.DurationInMS / 1000.0, 1.0);
        double durationMinutes = durationSeconds / 60.0;
        Dictionary<AgentItem, int> corruptsByAttacker = CombatReplayAnalysisBuilder.CountBoonCorruptsByAttacker(
            log,
            phase.Start,
            phase.End,
            hostilePlayerTargets,
            squadPlayers);

        return hostilePlayerTargets
            .Select(enemy =>
            {
                long damage = 0;
                foreach (SingleActor target in squadPlayers)
                {
                    DamageStatistics damageStats = enemy.GetDamageStats(target, log, phase.Start, phase.End);
                    damage += damageStats.Damage;
                }

                SupportStatistics support = enemy.GetToAllySupportStats(log, phase.Start, phase.End);
                int strips = support.BoonStripCount;
                int corrupts = corruptsByAttacker.GetValueOrDefault(enemy.EnglobingAgentItem);

                return new WvWAnalystEnemyPlayerSummaryDto
                {
                    ActorId = enemy.UniqueID,
                    Profession = enemy.BaseSpec.ToString(),
                    EliteSpec = enemy.Spec.ToString(),
                    Icon = enemy.GetIcon(),
                    ActiveSeconds = Math.Round(enemy.GetActiveDuration(log, phase.Start, phase.End) / 1000.0, 1),
                    CombatSeconds = Math.Round(enemy.GetTimeSpentInCombat(log, phase.Start, phase.End) / 1000.0, 1),
                    Damage = damage,
                    Dps = Math.Round(damage / durationSeconds, 1),
                    Strips = strips,
                    Corrupts = corrupts,
                    CorruptPercent = ComputePercent(corrupts, strips),
                    StripsPerMinute = Math.Round(strips / durationMinutes, 1),
                    CorruptsPerMinute = Math.Round(corrupts / durationMinutes, 1),
                };
            })
            .OrderBy(enemy => BuildClassLabel(enemy.Profession, enemy.EliteSpec), StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(enemy => enemy.Dps)
            .ThenByDescending(enemy => enemy.StripsPerMinute)
            .ThenBy(enemy => enemy.ActorId)
            .ToArray();
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

    private static IReadOnlyList<WvWAnalystPlayerFightImpactLaneDto> BuildPlayerFightImpactLanes(CombatReplayPlayerEvaluationDto? evaluation)
    {
        if (evaluation?.FightImpact?.Lanes is null || evaluation.FightImpact.Lanes.Count == 0)
        {
            return Array.Empty<WvWAnalystPlayerFightImpactLaneDto>();
        }

        return evaluation.FightImpact.Lanes
            .Where(lane => lane.ImpactScore > 0.0 || lane.StrengthPercent > 0.0)
            .OrderByDescending(lane => lane.ImpactScore)
            .ThenByDescending(lane => lane.DemandScorePercent)
            .Take(4)
            .Select(lane => new WvWAnalystPlayerFightImpactLaneDto
            {
                Key = lane.Key,
                Label = lane.Label,
                StrengthPercent = lane.StrengthPercent,
                SharePercent = lane.SharePercent,
                DemandScorePercent = lane.DemandScorePercent,
                DemandLabel = lane.DemandLabel,
                DemandWeightPercent = lane.DemandWeightPercent,
                ImpactScore = lane.ImpactScore,
                EvidenceLine = lane.EvidenceLine,
                ContextLine = lane.ContextLine,
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

    private static WvWAnalystMitigationSummaryDto? BuildMitigationSummary(CombatReplayAnalysisDto? combatReplayAnalysis)
    {
        CombatReplayDefenseAnalysisDto? defense = combatReplayAnalysis?.Defense;
        CombatReplayDefenseSavedPlayersSummaryDto? savedPlayers = defense?.SavedPlayersSummary;
        if (defense is null || savedPlayers is null)
        {
            return null;
        }

        return new WvWAnalystMitigationSummaryDto
        {
            HasBarrierData = defense.HasBarrierData,
            BarrierCoverageMayBeIncomplete = defense.BarrierCoverageMayBeIncomplete,
            TotalDamageToSquad = defense.TotalDamageToSquad,
            HealthDamageToSquad = defense.HealthDamageToSquad,
            TotalBarrierAbsorbed = defense.BarrierDamageAbsorbed,
            BarrierAbsorptionPercent = defense.BarrierAbsorptionPercent,
            TotalPetMinionAbsorption = defense.TotalPetMinionDamageAbsorbed,
            PetMinionAbsorptionPercent = defense.PetMinionAbsorptionPercent,
            SavedCases = savedPlayers.SavedCases,
            BarrierSavedCases = savedPlayers.BarrierSavedCases,
            DamageReductionSavedCases = savedPlayers.DamageReductionSavedCases,
            NegatedDamageSavedCases = savedPlayers.NegatedDamageSavedCases,
            BothSavedCases = savedPlayers.BothSavedCases,
            MultiSourceSavedCases = savedPlayers.MultiSourceSavedCases,
            TotalBarrierAbsorbedInSaves = savedPlayers.TotalBarrierAbsorbed,
            TotalEstimatedDamageReduction = savedPlayers.TotalEstimatedDamageReduction,
            TotalEstimatedNegatedDamage = savedPlayers.TotalEstimatedNegatedDamage,
            AverageLowestHealthPercent = savedPlayers.AverageLowestHealthPercent,
            LowestLowestHealthPercent = savedPlayers.LowestLowestHealthPercent,
            TotalIncomingDamage = savedPlayers.TotalIncomingDamage,
            TotalIncomingHealing = savedPlayers.TotalIncomingHealing,
            BarrierOvercap = BuildBarrierOvercapSummary(defense.BarrierOvercap),
            Reflects = BuildReflectSummary(defense.Reflects),
            ShieldOfCourage = BuildShieldOfCourageSummary(defense.ShieldOfCourage),
            NegatedHitSummaries = defense.NegatedHitSummaries
                .Select(summary => new WvWAnalystNegatedHitSummaryDto
                {
                    Key = summary.Key,
                    Label = summary.Label,
                    NegatedHitCount = summary.NegatedHitCount,
                    EstimatedPreventedDamage = summary.EstimatedPreventedDamage,
                    FallbackEstimateCount = summary.FallbackEstimateCount,
                    ContributingEffects = summary.ContributingEffects
                        .Select(effect => new WvWAnalystEffectCountSummaryDto
                        {
                            Name = effect.Name,
                            Count = effect.Count,
                        })
                        .ToArray(),
                })
                .ToArray(),
        };
    }

    private static WvWAnalystShieldOfCourageSummaryDto? BuildShieldOfCourageSummary(CombatReplayDefenseShieldOfCourageDto? summary)
    {
        if (summary is null || !summary.Available)
        {
            return null;
        }

        return new WvWAnalystShieldOfCourageSummaryDto
        {
            Available = summary.Available,
            BlockedAttackCount = summary.BlockedAttackCount,
            EstimatedBlockedDamage = summary.EstimatedBlockedDamage,
            FallbackEstimateCount = summary.FallbackEstimateCount,
            MaxCoveredPlayers = summary.MaxCoveredPlayers,
            MaxCoveredPlayersTimeLabel = summary.MaxCoveredPlayersTimeLabel,
        };
    }

    private static WvWAnalystBarrierOvercapSummaryDto? BuildBarrierOvercapSummary(CombatReplayDefenseBarrierOvercapDto? summary)
    {
        if (summary is null || !summary.Available)
        {
            return null;
        }

        return new WvWAnalystBarrierOvercapSummaryDto
        {
            Available = summary.Available,
            RawBarrierEvaluated = summary.RawBarrierEvaluated,
            EstimatedOvercap = summary.EstimatedOvercap,
            OvercapPercentOfEvaluated = summary.OvercapPercentOfEvaluated,
            EvaluatedApplicationGroups = summary.EvaluatedApplicationGroups,
            OvercapApplicationGroups = summary.OvercapApplicationGroups,
            HighConfidenceGroups = summary.HighConfidenceGroups,
            EstimatedHealthPoolGroups = summary.EstimatedHealthPoolGroups,
            SkippedNoBarrierStateGroups = summary.SkippedNoBarrierStateGroups,
        };
    }

    private static WvWAnalystReflectSummaryDto? BuildReflectSummary(CombatReplayDefenseReflectAnalysisDto? summary)
    {
        if (summary is null || !summary.HasMissileData)
        {
            return null;
        }

        return new WvWAnalystReflectSummaryDto
        {
            HasMissileData = summary.HasMissileData,
            TotalReflectedProjectiles = summary.TotalReflectedProjectiles,
            TotalLandedHits = summary.TotalLandedHits,
            TotalLandedDamage = summary.TotalLandedDamage,
            TotalEstimatedMitigatedProjectiles = summary.TotalEstimatedMitigatedProjectiles,
            TotalEstimatedMitigatedDamage = summary.TotalEstimatedMitigatedDamage,
            TotalUnestimatedMitigatedProjectiles = summary.TotalUnestimatedMitigatedProjectiles,
            TotalDowns = summary.TotalDowns,
            TotalKills = summary.TotalKills,
            SquadToEnemy = BuildReflectSideSummary(summary.SquadToEnemy),
            EnemyToSquad = BuildReflectSideSummary(summary.EnemyToSquad),
        };
    }

    private static WvWAnalystReflectSideSummaryDto BuildReflectSideSummary(CombatReplayDefenseReflectSideDto? summary)
    {
        if (summary is null)
        {
            return new WvWAnalystReflectSideSummaryDto();
        }

        return new WvWAnalystReflectSideSummaryDto
        {
            ReflectedProjectiles = summary.ReflectedProjectiles,
            LandedHits = summary.LandedHits,
            LandedDamage = summary.LandedDamage,
            EstimatedMitigatedProjectiles = summary.EstimatedMitigatedProjectiles,
            EstimatedMitigatedDamage = summary.EstimatedMitigatedDamage,
            HighConfidenceMitigatedProjectiles = summary.HighConfidenceMitigatedProjectiles,
            HighConfidenceMitigatedDamage = summary.HighConfidenceMitigatedDamage,
            FallbackEstimatedMitigatedProjectiles = summary.FallbackEstimatedMitigatedProjectiles,
            FallbackEstimatedMitigatedDamage = summary.FallbackEstimatedMitigatedDamage,
            UnestimatedMitigatedProjectiles = summary.UnestimatedMitigatedProjectiles,
            DownEvents = summary.DownEvents,
            KillEvents = summary.KillEvents,
            MatchedDamageEvents = summary.MatchedDamageEvents,
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
        IReadOnlyList<SingleActor> actors,
        Func<CombatReplayAnalysisDto, CombatReplayTeamAnalysisDto> teamSelector)
    {
        if (combatReplayAnalysis is null || combatReplayAnalysis.Times.Length == 0)
        {
            return Array.Empty<WvWAnalystTopBurstDto>();
        }

        CombatReplayTeamAnalysisDto teamAnalysis = teamSelector(combatReplayAnalysis);
        if (teamAnalysis.TopBursts is null || teamAnalysis.TopBursts.Count == 0)
        {
            return Array.Empty<WvWAnalystTopBurstDto>();
        }

        var actorsById = actors.ToDictionary(actor => actor.UniqueID);

        return teamAnalysis.TopBursts
            .Select(burst => BuildTopBurst(combatReplayAnalysis, teamAnalysis, actorsById, burst))
            .Where(burst => burst is not null)
            .Select(burst => burst!)
            .ToArray();
    }

    private static WvWAnalystTopBurstDto? BuildTopBurst(
        CombatReplayAnalysisDto combatReplayAnalysis,
        CombatReplayTeamAnalysisDto teamAnalysis,
        IReadOnlyDictionary<int, SingleActor> actorsById,
        CombatReplayAnalysisBurstSummaryDto burst)
    {
        var snapshotIndex = Array.BinarySearch(combatReplayAnalysis.Times, burst.Time);
        if (snapshotIndex < 0 || snapshotIndex >= combatReplayAnalysis.Times.Length)
        {
            return null;
        }

        var topPressureActors = BuildTopBurstActorSummaries(
            actorsById,
            teamAnalysis.TopDamageActorIds,
            teamAnalysis.TopDamageValues,
            snapshotIndex);
        var topStripActors = BuildTopBurstActorSummaries(
            actorsById,
            teamAnalysis.TopStripActorIds,
            teamAnalysis.TopStripValues,
            teamAnalysis.TopStripCorruptValues,
            snapshotIndex);

        return new WvWAnalystTopBurstDto
        {
            Time = burst.Time,
            TimeLabel = FormatBurstTime(burst.Time),
            Damage = burst.Damage,
            Strips = burst.Strips,
            Corrupts = burst.Corrupts,
            CorruptPercent = ComputePercent(burst.Corrupts, burst.Strips),
            Downs = burst.Downs,
            Kills = burst.Kills,
            TopPressure = topPressureActors.FirstOrDefault() ?? new WvWAnalystTopBurstActorDto(),
            TopStrips = topStripActors.FirstOrDefault() ?? new WvWAnalystTopBurstActorDto(),
            TopPressureActors = topPressureActors,
            TopStripActors = topStripActors,
        };
    }

    private static string FormatBurstTime(long time)
    {
        return $"{(time / 1000.0).ToString("0.000", CultureInfo.InvariantCulture)}s";
    }

    private static double ComputePercent(double numerator, double denominator)
    {
        return denominator <= 0.0
            ? 0.0
            : Math.Round(numerator * 100.0 / denominator, 1);
    }

    private static IReadOnlyList<WvWAnalystTopBurstActorDto> BuildTopBurstActorSummaries<TValue>(
        IReadOnlyDictionary<int, SingleActor> actorsById,
        IReadOnlyList<int[]> actorIdsBySnapshot,
        IReadOnlyList<TValue[]> valuesBySnapshot,
        int snapshotIndex)
        where TValue : struct
        => BuildTopBurstActorSummaries(actorsById, actorIdsBySnapshot, valuesBySnapshot, null, snapshotIndex);

    private static IReadOnlyList<WvWAnalystTopBurstActorDto> BuildTopBurstActorSummaries<TValue>(
        IReadOnlyDictionary<int, SingleActor> actorsById,
        IReadOnlyList<int[]> actorIdsBySnapshot,
        IReadOnlyList<TValue[]> valuesBySnapshot,
        IReadOnlyList<int[]>? corruptValuesBySnapshot,
        int snapshotIndex)
        where TValue : struct
    {
        if (snapshotIndex < 0 ||
            snapshotIndex >= actorIdsBySnapshot.Count ||
            snapshotIndex >= valuesBySnapshot.Count ||
            (corruptValuesBySnapshot is not null && snapshotIndex >= corruptValuesBySnapshot.Count))
        {
            return Array.Empty<WvWAnalystTopBurstActorDto>();
        }

        var actorIds = actorIdsBySnapshot[snapshotIndex];
        var values = valuesBySnapshot[snapshotIndex];
        var corruptValues = corruptValuesBySnapshot?[snapshotIndex];
        if (actorIds is null || values is null || actorIds.Length == 0 || values.Length == 0)
        {
            return Array.Empty<WvWAnalystTopBurstActorDto>();
        }

        var limit = Math.Min(actorIds.Length, values.Length);
        var result = new List<WvWAnalystTopBurstActorDto>(limit);
        for (var index = 0; index < limit; index++)
        {
            var actorId = actorIds[index];
            double amount = Convert.ToDouble(values[index], CultureInfo.InvariantCulture);
            int corrupts = corruptValues is not null && index < corruptValues.Length ? corruptValues[index] : 0;
            if (!actorsById.TryGetValue(actorId, out var actor))
            {
                result.Add(new WvWAnalystTopBurstActorDto
                {
                    ActorId = actorId,
                    Amount = amount,
                    Corrupts = corrupts,
                    CorruptPercent = ComputePercent(corrupts, amount),
                });
                continue;
            }

            result.Add(new WvWAnalystTopBurstActorDto
            {
                ActorId = actorId,
                Account = actor.Account,
                Character = actor.Character,
                Profession = actor.BaseSpec.ToString(),
                EliteSpec = actor.Spec.ToString(),
                Icon = actor.GetIcon(),
                Amount = amount,
                Corrupts = corrupts,
                CorruptPercent = ComputePercent(corrupts, amount),
            });
        }

        return result;
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

    private static double GetPlayerLaneMetricValue(CombatReplayPlayerEvaluationDto? evaluation, string metricKey)
    {
        return evaluation?.Lanes?
            .SelectMany(lane => lane.Metrics ?? [])
            .FirstOrDefault(metric => string.Equals(metric.Key, metricKey, StringComparison.OrdinalIgnoreCase))
            ?.Value ?? 0.0;
    }

    private static double GetPlayerReflectDamageOnEnemy(CombatReplayAnalysisDto? combatReplayAnalysis, int playerId)
    {
        return combatReplayAnalysis?.Defense?.Reflects?.SquadToEnemy?.TopAttributedActors?
            .Where(actor => actor.ActorId == playerId)
            .Sum(actor => actor.Amount) ?? 0.0;
    }

    private static long ComputeMysticRebukeDamage(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        long total = 0;
        foreach (SingleActor target in hostilePlayerTargets)
        {
            foreach (HealthDamageEvent damageEvent in player.GetDamageEvents(target, log, phase.Start, phase.End))
            {
                if (damageEvent.HasHit &&
                    damageEvent.HealthDamage > 0 &&
                    damageEvent.Skill.Name.Contains("Mystic Rebuke", StringComparison.OrdinalIgnoreCase))
                {
                    total += damageEvent.HealthDamage;
                }
            }
        }
        return total;
    }

    private static int CountPulls(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        IReadOnlyList<SingleActor> hostilePlayerTargets)
    {
        return log.CombatData.GetCrowdControlData(ArcDpsGenericKnockbackPullSkillId)
            .Count(evt =>
                evt.Time >= phase.Start &&
                evt.Time <= phase.End &&
                IsAttributedToPlayer(evt.CreditedFrom, evt.From, player) &&
                hostilePlayerTargets.Any(target => evt.To.Is(target.AgentItem) || evt.To.Is(target.EnglobingAgentItem)));
    }

    private static int CountIllusionOfLifeRezzes(
        ParsedEvtcLog log,
        PhaseData phase,
        SingleActor player,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        int count = 0;
        foreach (SingleActor recoveredPlayer in squadPlayers.Where(candidate => candidate.UniqueID != player.UniqueID))
        {
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(recoveredPlayer.AgentItem))
            {
                if (downEvent.Time < phase.Start || downEvent.Time > phase.End)
                {
                    continue;
                }

                AliveEvent? nextAlive = log.CombatData.GetAliveEvents(recoveredPlayer.AgentItem).FirstOrDefault(evt => evt.Time >= downEvent.Time);
                DeadEvent? nextDead = log.CombatData.GetDeadEvents(recoveredPlayer.AgentItem).FirstOrDefault(evt => evt.Time >= downEvent.Time);
                if (nextAlive is null || (nextDead is not null && nextDead.Time <= nextAlive.Time))
                {
                    continue;
                }

                if (log.CombatData.GetBuffApplyDataByIDByDst(SkillIDs.IllusionOfLifeBuff, recoveredPlayer.AgentItem)
                    .Any(buffApply =>
                        buffApply.Time >= downEvent.Time &&
                        buffApply.Time <= nextAlive.Time &&
                        IsAttributedToPlayer(buffApply.CreditedBy, buffApply.By, player)))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool IsAttributedToPlayer(AgentItem creditedBy, AgentItem source, SingleActor player)
    {
        AgentItem provider = !creditedBy.IsUnknown ? creditedBy : source.GetFinalMaster();
        return provider.Is(player.AgentItem) ||
            provider.Is(player.EnglobingAgentItem) ||
            source.Is(player.AgentItem) ||
            source.Is(player.EnglobingAgentItem);
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
            WeightPercent = metric.WeightPercent,
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

    private static string BuildClassLabel(string? profession, string? eliteSpec)
    {
        string trimmedProfession = profession?.Trim() ?? string.Empty;
        string trimmedEliteSpec = eliteSpec?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(trimmedEliteSpec) &&
            !string.Equals(trimmedEliteSpec, trimmedProfession, StringComparison.OrdinalIgnoreCase))
        {
            return trimmedEliteSpec;
        }

        return trimmedProfession;
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

    private sealed class FightShapeCandidateDiagnostics
    {
        public long TimeMs { get; init; }
        public string CleanupSide { get; init; } = string.Empty;
        public double Confidence { get; init; }
        public int FailureRank { get; init; }
        public string FailureReason { get; init; } = string.Empty;
        public string FailureDetail { get; init; } = string.Empty;
    }

    private sealed record FightShapeLateRecoveryDiagnostics(
        long TimeMs,
        WvWAnalystFightShapeSideStateDto SquadState,
        WvWAnalystFightShapeSideStateDto EnemyState);
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
    public WvWAnalystMitigationSummaryDto? MitigationSummary { get; set; }
    public WvWAnalystObliterateSummaryDto? Obliterate { get; set; }
    public WvWAnalystFightShapeDto? FightShape { get; set; }
    public IReadOnlyList<WvWAnalystThreatBoonSummaryDto> ThreatBoons { get; set; } = Array.Empty<WvWAnalystThreatBoonSummaryDto>();
    public IReadOnlyList<WvWAnalystTopBurstDto> TopBursts { get; set; } = Array.Empty<WvWAnalystTopBurstDto>();
    public IReadOnlyList<WvWAnalystTopBurstDto> EnemyTopBursts { get; set; } = Array.Empty<WvWAnalystTopBurstDto>();
    public IReadOnlyList<WvWAnalystPlayerSummaryDto> Players { get; set; } = Array.Empty<WvWAnalystPlayerSummaryDto>();
    public IReadOnlyList<WvWAnalystEnemyPlayerSummaryDto> EnemyPlayers { get; set; } = Array.Empty<WvWAnalystEnemyPlayerSummaryDto>();
}

internal sealed class WvWAnalystFightShapeDto
{
    public bool Available { get; set; }
    public string DetectionLabel { get; set; } = string.Empty;
    public string CleanupSide { get; set; } = string.Empty;
    public string LosingSide { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public long? CompetitiveEndTimeMs { get; set; }
    public long? CleanupStartTimeMs { get; set; }
    public long CompetitiveDurationMs { get; set; }
    public long CleanupDurationMs { get; set; }
    public double CleanupPercent { get; set; }
    public IReadOnlyList<string> Rules { get; set; } = Array.Empty<string>();
    public long? BestCandidateTimeMs { get; set; }
    public string BestCandidateCleanupSide { get; set; } = string.Empty;
    public double BestCandidateConfidence { get; set; }
    public string BestCandidateReason { get; set; } = string.Empty;
    public string BestCandidateDetail { get; set; } = string.Empty;
    public WvWAnalystFightShapeEventSnapshotDto? AtCleanupStart { get; set; }
    public WvWAnalystFightShapeEventSnapshotDto? AfterCleanupStart { get; set; }
    public WvWAnalystFightShapeSideStateDto? SquadAtCleanupStart { get; set; }
    public WvWAnalystFightShapeSideStateDto? EnemyAtCleanupStart { get; set; }
}

internal sealed class WvWAnalystFightShapeEventSnapshotDto
{
    public int SquadMembersDowned { get; set; }
    public int EnemyPlayersDowned { get; set; }
    public int SquadKillsSecured { get; set; }
    public int EnemyKillsSecured { get; set; }
    public int SquadRecoveries { get; set; }
    public int EnemyRecoveries { get; set; }
    public long SquadDamage { get; set; }
    public long EnemyDamage { get; set; }
}

internal sealed class WvWAnalystFightShapeSideStateDto
{
    public int Total { get; set; }
    public int Known { get; set; }
    public int Active { get; set; }
    public int Downed { get; set; }
    public int DeadOrDc { get; set; }
    public int Removed { get; set; }
    public int FarFromFight { get; set; }
    public int Unobserved { get; set; }
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
    public IReadOnlyList<WvWAnalystSideClassSummaryDto> Classes { get; set; } = Array.Empty<WvWAnalystSideClassSummaryDto>();
    public WvWAnalystSideTotalsDto Totals { get; set; } = new();
}

internal sealed class WvWAnalystSideClassSummaryDto
{
    public string ClassLabel { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Count { get; set; }
    public WvWAnalystSpecFightCoverageDto? FightCoverage { get; set; }
}

internal sealed class WvWAnalystSpecFightCoverageDto
{
    public double Score { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public IReadOnlyList<string> Caveats { get; set; } = Array.Empty<string>();
    public IReadOnlyList<WvWAnalystSpecFightCoverageLaneDto> Lanes { get; set; } = Array.Empty<WvWAnalystSpecFightCoverageLaneDto>();
}

internal sealed class WvWAnalystSpecFightCoverageLaneDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public double PerSlotEfficiency { get; set; }
    public int PlayersContributing { get; set; }
    public int PlayerCount { get; set; }
    public double DemandScorePercent { get; set; }
    public string DemandLabel { get; set; } = string.Empty;
    public double DemandWeightPercent { get; set; }
    public double CoverageScore { get; set; }
    public string EvidenceLine { get; set; } = string.Empty;
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
    public long Corrupts { get; set; }
    public double CorruptPercent { get; set; }
    public int ReceivedCrowdControl { get; set; }
    public double StripsPerMinute { get; set; }
    public double CorruptsPerMinute { get; set; }
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
    public int? EnemyMovementScore { get; set; }
    public string EnemyMovementScoreLabel { get; set; } = string.Empty;
    public string EnemyMovementScoreDetail { get; set; } = string.Empty;
    public double? EnemyMovementCenterTightShare { get; set; }
    public double? EnemyMovementAverageDistanceToCenter { get; set; }
    public int EnemyMovementSampleCount { get; set; }
    public bool ThreeWayDetected { get; set; }
    public string ThreeWayLabel { get; set; } = string.Empty;
    public string ThreeWayDetail { get; set; } = string.Empty;
    public long? ThreeWayStartTimeMs { get; set; }
    public int ThreeWaySecondEnemyPeakCount { get; set; }
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
    public bool CrowdControlDataAvailable { get; set; }
    public int IncomingCrowdControl { get; set; }
    public int OutgoingCrowdControl { get; set; }
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
    public double WeightPercent { get; set; }
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

internal sealed class WvWAnalystMitigationSummaryDto
{
    public bool HasBarrierData { get; set; }
    public bool BarrierCoverageMayBeIncomplete { get; set; }
    public double TotalDamageToSquad { get; set; }
    public double HealthDamageToSquad { get; set; }
    public double TotalBarrierAbsorbed { get; set; }
    public double BarrierAbsorptionPercent { get; set; }
    public double TotalPetMinionAbsorption { get; set; }
    public double PetMinionAbsorptionPercent { get; set; }
    public int SavedCases { get; set; }
    public int BarrierSavedCases { get; set; }
    public int DamageReductionSavedCases { get; set; }
    public int NegatedDamageSavedCases { get; set; }
    public int BothSavedCases { get; set; }
    public int MultiSourceSavedCases { get; set; }
    public double TotalBarrierAbsorbedInSaves { get; set; }
    public double TotalEstimatedDamageReduction { get; set; }
    public double TotalEstimatedNegatedDamage { get; set; }
    public double AverageLowestHealthPercent { get; set; }
    public double LowestLowestHealthPercent { get; set; }
    public double TotalIncomingDamage { get; set; }
    public double TotalIncomingHealing { get; set; }
    public WvWAnalystBarrierOvercapSummaryDto? BarrierOvercap { get; set; }
    public WvWAnalystReflectSummaryDto? Reflects { get; set; }
    public WvWAnalystShieldOfCourageSummaryDto? ShieldOfCourage { get; set; }
    public IReadOnlyList<WvWAnalystNegatedHitSummaryDto> NegatedHitSummaries { get; set; } = Array.Empty<WvWAnalystNegatedHitSummaryDto>();
}

internal sealed class WvWAnalystShieldOfCourageSummaryDto
{
    public bool Available { get; set; }
    public int BlockedAttackCount { get; set; }
    public double EstimatedBlockedDamage { get; set; }
    public int FallbackEstimateCount { get; set; }
    public int MaxCoveredPlayers { get; set; }
    public string MaxCoveredPlayersTimeLabel { get; set; } = string.Empty;
}

internal sealed class WvWAnalystBarrierOvercapSummaryDto
{
    public bool Available { get; set; }
    public double RawBarrierEvaluated { get; set; }
    public double EstimatedOvercap { get; set; }
    public double OvercapPercentOfEvaluated { get; set; }
    public int EvaluatedApplicationGroups { get; set; }
    public int OvercapApplicationGroups { get; set; }
    public int HighConfidenceGroups { get; set; }
    public int EstimatedHealthPoolGroups { get; set; }
    public int SkippedNoBarrierStateGroups { get; set; }
}

internal sealed class WvWAnalystReflectSummaryDto
{
    public bool HasMissileData { get; set; }
    public int TotalReflectedProjectiles { get; set; }
    public int TotalLandedHits { get; set; }
    public double TotalLandedDamage { get; set; }
    public int TotalEstimatedMitigatedProjectiles { get; set; }
    public double TotalEstimatedMitigatedDamage { get; set; }
    public int TotalUnestimatedMitigatedProjectiles { get; set; }
    public int TotalDowns { get; set; }
    public int TotalKills { get; set; }
    public WvWAnalystReflectSideSummaryDto SquadToEnemy { get; set; } = new();
    public WvWAnalystReflectSideSummaryDto EnemyToSquad { get; set; } = new();
}

internal sealed class WvWAnalystReflectSideSummaryDto
{
    public int ReflectedProjectiles { get; set; }
    public int LandedHits { get; set; }
    public double LandedDamage { get; set; }
    public int EstimatedMitigatedProjectiles { get; set; }
    public double EstimatedMitigatedDamage { get; set; }
    public int HighConfidenceMitigatedProjectiles { get; set; }
    public double HighConfidenceMitigatedDamage { get; set; }
    public int FallbackEstimatedMitigatedProjectiles { get; set; }
    public double FallbackEstimatedMitigatedDamage { get; set; }
    public int UnestimatedMitigatedProjectiles { get; set; }
    public int DownEvents { get; set; }
    public int KillEvents { get; set; }
    public int MatchedDamageEvents { get; set; }
}

internal sealed class WvWAnalystNegatedHitSummaryDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int NegatedHitCount { get; set; }
    public double EstimatedPreventedDamage { get; set; }
    public int FallbackEstimateCount { get; set; }
    public IReadOnlyList<WvWAnalystEffectCountSummaryDto> ContributingEffects { get; set; } = Array.Empty<WvWAnalystEffectCountSummaryDto>();
}

internal sealed class WvWAnalystEffectCountSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
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
    public long DamageToDownedTargets { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public int DownContribution { get; set; }
    public int Strips { get; set; }
    public int Corrupts { get; set; }
    public double CorruptPercent { get; set; }
    public int OutgoingCleanses { get; set; }
    public long Healing { get; set; }
    public long DownedHealing { get; set; }
    public long Barrier { get; set; }
    public int Resurrects { get; set; }
    public int IllusionOfLifeRezzes { get; set; }
    public int Deaths { get; set; }
    public int Recoveries { get; set; }
    public long DamageTaken { get; set; }
    public long PetDamageAbsorbed { get; set; }
    public double DamageReflectedOnEnemy { get; set; }
    public long MysticRebukeDamage { get; set; }
    public int Pulls { get; set; }
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
    public double FightImpactScore { get; set; }
    public string FightImpactLabel { get; set; } = string.Empty;
    public string FightImpactSummary { get; set; } = string.Empty;
    public string FightImpactDetail { get; set; } = string.Empty;
    public string FightImpactConfidenceLabel { get; set; } = string.Empty;
    public IReadOnlyList<string> FightImpactCaveats { get; set; } = Array.Empty<string>();
    public IReadOnlyList<WvWAnalystPlayerFightImpactLaneDto> FightImpactLanes { get; set; } = Array.Empty<WvWAnalystPlayerFightImpactLaneDto>();
    public string EvaluationConfidenceLabel { get; set; } = string.Empty;
    public string EvaluationConfidenceDetail { get; set; } = string.Empty;
    public IReadOnlyList<string> EvaluationCaveats { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EvidenceSnapshot { get; set; } = Array.Empty<string>();
    public IReadOnlyList<WvWAnalystPlayerRoleMixEntryDto> RoleMix { get; set; } = Array.Empty<WvWAnalystPlayerRoleMixEntryDto>();
    public IReadOnlyList<WvWAnalystPlayerLaneSummaryDto> Lanes { get; set; } = Array.Empty<WvWAnalystPlayerLaneSummaryDto>();
    public IReadOnlyList<WvWAnalystPlayerThreatBoonSummaryDto> ThreatBoons { get; set; } = Array.Empty<WvWAnalystPlayerThreatBoonSummaryDto>();
    public IReadOnlyList<WvWAnalystPlayerProvidedBoonSummaryDto> ProvidedBoons { get; set; } = Array.Empty<WvWAnalystPlayerProvidedBoonSummaryDto>();
}

internal sealed class WvWAnalystEnemyPlayerSummaryDto
{
    public int ActorId { get; set; }
    public string Profession { get; set; } = string.Empty;
    public string EliteSpec { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double ActiveSeconds { get; set; }
    public double CombatSeconds { get; set; }
    public long Damage { get; set; }
    public double Dps { get; set; }
    public int Strips { get; set; }
    public int Corrupts { get; set; }
    public double CorruptPercent { get; set; }
    public double StripsPerMinute { get; set; }
    public double CorruptsPerMinute { get; set; }
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

internal sealed class WvWAnalystPlayerFightImpactLaneDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public double DemandScorePercent { get; set; }
    public string DemandLabel { get; set; } = string.Empty;
    public double DemandWeightPercent { get; set; }
    public double ImpactScore { get; set; }
    public string EvidenceLine { get; set; } = string.Empty;
    public string ContextLine { get; set; } = string.Empty;
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
    public int Corrupts { get; set; }
    public double CorruptPercent { get; set; }
    public int Downs { get; set; }
    public int Kills { get; set; }
    public WvWAnalystTopBurstActorDto TopPressure { get; set; } = new();
    public WvWAnalystTopBurstActorDto TopStrips { get; set; } = new();
    public IReadOnlyList<WvWAnalystTopBurstActorDto> TopPressureActors { get; set; } = Array.Empty<WvWAnalystTopBurstActorDto>();
    public IReadOnlyList<WvWAnalystTopBurstActorDto> TopStripActors { get; set; } = Array.Empty<WvWAnalystTopBurstActorDto>();
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
    public int Corrupts { get; set; }
    public double CorruptPercent { get; set; }
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
