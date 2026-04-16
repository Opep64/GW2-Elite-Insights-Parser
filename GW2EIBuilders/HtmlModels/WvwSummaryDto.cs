using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.LogLogic;
using GW2EIEvtcParser.ParsedData;
using System;
using System.Globalization;
using static GW2EIEvtcParser.ParserHelper;
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
    public List<WvwSummaryMetricRowDto> MetricRows { get; set; } = [];
    public WvwSummarySideDto Squad { get; set; } = new();
    public WvwSummarySideDto Enemy { get; set; } = new();
    public List<WvwSummaryTopPlayerDto> TopDamagePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopStripPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopCleansePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopBarrierPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopHealingPlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopEnemyDamagePlayers { get; set; } = [];
    public List<WvwSummaryTopPlayerDto> TopEnemyStripPlayers { get; set; } = [];

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

        return new WvwSummaryDto
        {
            FightTime = ToDurationString(durationInMilliseconds),
            FightTimeSeconds = Math.Round(durationInSeconds, TimeDigit),
            TotalSquadPlayers = squadActors.Count,
            HealAddonPlayerCount = healAddonPlayerCount,
            HealStatsNotice = BuildHealStatsNotice(healAddonPlayerCount, squadActors.Count),
            HasHealingData = log.CombatData.HasEXTHealing,
            HasBarrierData = log.CombatData.HasEXTBarrier,
            Squad = squad,
            Enemy = enemy,
            MetricRows = BuildMetricRows(durationInMilliseconds, squad, enemy),
            TopDamagePlayers = BuildTopDamagePlayers(log, squadActors, hostilePlayerTargets, phase),
            TopStripPlayers = BuildTopStripPlayers(log, squadActors, phase),
            TopCleansePlayers = BuildTopCleansePlayers(log, squadActors, phase),
            TopBarrierPlayers = BuildTopBarrierPlayers(log, squadActors, phase),
            TopHealingPlayers = BuildTopHealingPlayers(log, squadActors, phase),
            TopEnemyDamagePlayers = BuildTopEnemyDamagePlayers(log, phase, hostilePlayerTargets, squadActors),
            TopEnemyStripPlayers = BuildTopEnemyStripPlayers(log, hostilePlayerTargets, phase),
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
    public string Profession { get; set; } = "";
    public string Icon { get; set; } = "";
    public long Amount { get; set; }
}
