using System.Numerics;
using System.Globalization;
using System.Text.RegularExpressions;
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
    public CombatReplayDefenseAnalysisDto Defense { get; set; } = new();
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

internal class CombatReplayDefenseAnalysisDto
{
    public bool HasBarrierData { get; set; }
    public bool BarrierCoverageMayBeIncomplete { get; set; }
    public long TotalBarrierGranted { get; set; }
    public long InitialBarrierOnSquad { get; set; }
    public long TotalBarrierAvailable { get; set; }
    public long TotalDamageToSquad { get; set; }
    public long HealthDamageToSquad { get; set; }
    public long BarrierDamageAbsorbed { get; set; }
    public double BarrierAbsorptionPercent { get; set; }
    public CombatReplayDefenseBurstBarrierDto BurstBarrier { get; set; } = new();
    public CombatReplayDefenseMitigationDto Mitigation { get; set; } = new();
    public List<CombatReplayEventActorSummaryDto> TopBarrierProviders { get; set; } = [];
}

internal class CombatReplayDefenseBurstBarrierDto
{
    public int EnemyBurstWindows { get; set; }
    public int BurstWindowsWithBarrierAbsorbed { get; set; }
    public int BurstWindowsWithSquadDown { get; set; }
    public int BurstWindowsHeld { get; set; }
    public int LowHealthSurvivorOccurrences { get; set; }
    public int LowHealthSurvivorPlayers { get; set; }
    public double TotalBurstDamageToSquad { get; set; }
    public double TotalBurstBarrierAbsorbed { get; set; }
    public double BurstBarrierAbsorptionPercent { get; set; }
    public double AverageBurstDamageToSquad { get; set; }
    public double AverageBurstBarrierAbsorbed { get; set; }
    public double HeldBurstBarrierShare { get; set; }
    public double DownedBurstBarrierShare { get; set; }
    public List<CombatReplayDefenseBurstSurvivorEventDto> LowHealthSurvivorEvents { get; set; } = [];
    public List<string> Takeaways { get; set; } = [];
}

internal class CombatReplayDefenseBurstSurvivorEventDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public int ActorId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public double LowestHealthPercent { get; set; }
}

internal class CombatReplayDefenseMitigationDto
{
    public List<CombatReplayDefenseMitigationThresholdDto> Thresholds { get; set; } = [];
}

internal class CombatReplayDefenseMitigationThresholdDto
{
    public int ThresholdPercent { get; set; }
    public int Count { get; set; }
    public List<CombatReplayDefenseMitigationEventDto> Events { get; set; } = [];
}

internal class CombatReplayDefenseMitigationEventDto
{
    public const int EstimatedPlayerMaxHealth = 20000;
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public long PreviousFullHealthTime { get; set; }
    public string PreviousFullHealthTimeLabel { get; set; } = "";
    public long LowestHealthTime { get; set; }
    public string LowestHealthTimeLabel { get; set; } = "";
    public long RecoveryTime { get; set; }
    public string RecoveryTimeLabel { get; set; } = "";
    public int ActorId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public double LowestHealthPercent { get; set; }
    public int LowestHealthEstimate { get; set; }
    public double BarrierAbsorbedToLowest { get; set; }
    public bool BarrierSavedPlayer { get; set; }
    public double ProtectionEstimatedMitigation { get; set; }
    public double IncomingDamage { get; set; }
    public double IncomingHealing { get; set; }
    public List<CombatReplayMitigationEffectDto> Effects { get; set; } = [];
}

internal class CombatReplayMitigationEffectDto
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public double Seconds { get; set; }
}

internal sealed class TrackedMitigationBuff
{
    public string Name { get; init; } = "";
    public long[] BuffIds { get; init; } = [];
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
    public CombatReplayDownAnalysisDto Downs { get; set; } = new();
    public CombatReplayKillAnalysisDto Kills { get; set; } = new();
    public CombatReplayRecoveredAnalysisDto Recovered { get; set; } = new();
}

internal class CombatReplayDownAnalysisDto
{
    public CombatReplayDownSummaryDto CombinedSummary { get; set; } = new();
    public CombatReplayDownSummaryDto SquadSummary { get; set; } = new();
    public CombatReplayDownSummaryDto EnemySummary { get; set; } = new();
    public List<CombatReplayDownEventDto> Events { get; set; } = [];
}

internal class CombatReplayDownSummaryDto
{
    public int SquadDowns { get; set; }
    public int EnemyDowns { get; set; }
    public int CcImpactedDowns { get; set; }
    public int ConditionImpactedDowns { get; set; }
    public int MysticRebukeDowns { get; set; }
    public int HardCcDowns { get; set; }
    public int SoftCcDowns { get; set; }
    public int BothCcDowns { get; set; }
    public int BurningDowns { get; set; }
    public int ConditionMajorityDowns { get; set; }
    public int MysticRebukeHeavyDowns { get; set; }
    public double TotalDamage { get; set; }
    public double TotalStrikeDamage { get; set; }
    public double TotalConditionDamage { get; set; }
    public double TotalBurningDamage { get; set; }
    public double TotalMysticRebukeDamage { get; set; }
    public double AverageMysticRebukeDamage { get; set; }
    public double AverageEffectiveSoftCcSeconds { get; set; }
    public List<CombatReplayEventActorSummaryDto> TopContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> MysticRebukeContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> ConditionContributors { get; set; } = [];
    public List<CombatReplayEventSummaryEntryDto> TopConditions { get; set; } = [];
    public List<CombatReplayEventSummaryEntryDto> TopHardCcSources { get; set; } = [];
    public List<CombatReplayEventSummaryEntryDto> TopSoftCcSources { get; set; } = [];
    public List<string> Takeaways { get; set; } = [];
}

internal class CombatReplayKillAnalysisDto
{
    public CombatReplayKillSummaryDto CombinedSummary { get; set; } = new();
    public CombatReplayKillSummaryDto SquadSummary { get; set; } = new();
    public CombatReplayKillSummaryDto EnemySummary { get; set; } = new();
    public List<CombatReplayKillEventDto> Events { get; set; } = [];
}

internal class CombatReplayKillSummaryDto
{
    public int SquadKills { get; set; }
    public int EnemyKills { get; set; }
    public int ConditionImpactedKills { get; set; }
    public int MysticRebukeKills { get; set; }
    public int BurningKills { get; set; }
    public int ConditionMajorityKills { get; set; }
    public int MysticRebukeHeavyKills { get; set; }
    public double TotalDamage { get; set; }
    public double TotalStrikeDamage { get; set; }
    public double TotalConditionDamage { get; set; }
    public double TotalBurningDamage { get; set; }
    public double TotalMysticRebukeDamage { get; set; }
    public double TotalBarrierDamage { get; set; }
    public double AverageKillTimeSeconds { get; set; }
    public double AverageMysticRebukeDamage { get; set; }
    public List<CombatReplayEventActorSummaryDto> FinishContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> MysticRebukeContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> ConditionContributors { get; set; } = [];
    public List<CombatReplayEventSummaryEntryDto> TopConditions { get; set; } = [];
    public List<string> Takeaways { get; set; } = [];
}

internal class CombatReplayRecoveredAnalysisDto
{
    public CombatReplayRecoveredSquadSummaryDto SquadSummary { get; set; } = new();
    public CombatReplayRecoveredEnemySummaryDto EnemySummary { get; set; } = new();
    public List<CombatReplayRecoveredEventDto> Events { get; set; } = [];
}

internal class CombatReplayRecoveredSquadSummaryDto
{
    public int RecoveredCount { get; set; }
    public double AverageRecoverTimeSeconds { get; set; }
    public int TotalDownedHealing { get; set; }
    public int TotalHealingEvents { get; set; }
    public int TotalRezCasts { get; set; }
    public double TotalRezCastDurationSeconds { get; set; }
    public List<CombatReplayEventActorSummaryDto> SupportContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> HealingContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> RezContributors { get; set; } = [];
    public List<string> Takeaways { get; set; } = [];
}

internal class CombatReplayRecoveredEnemySummaryDto
{
    public int RecoveredCount { get; set; }
    public int ConditionImpactedRecoveries { get; set; }
    public int MysticRebukeRecoveries { get; set; }
    public int BurningRecoveries { get; set; }
    public int ConditionMajorityRecoveries { get; set; }
    public int MysticRebukeHeavyRecoveries { get; set; }
    public double TotalDamage { get; set; }
    public double TotalStrikeDamage { get; set; }
    public double TotalConditionDamage { get; set; }
    public double TotalBurningDamage { get; set; }
    public double TotalMysticRebukeDamage { get; set; }
    public double TotalBarrierDamage { get; set; }
    public double AverageRecoverTimeSeconds { get; set; }
    public double AverageMysticRebukeDamage { get; set; }
    public List<CombatReplayEventActorSummaryDto> PressureContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> MysticRebukeContributors { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> ConditionContributors { get; set; } = [];
    public List<CombatReplayEventSummaryEntryDto> TopConditions { get; set; } = [];
    public List<string> Takeaways { get; set; } = [];
}

internal class CombatReplayDownEventDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public long WindowStart { get; set; }
    public string WindowStartLabel { get; set; } = "";
    public long ConditionSnapshotTime { get; set; }
    public string ConditionSnapshotTimeLabel { get; set; } = "";
    public int ActorId { get; set; }
    public string ActorName { get; set; } = "";
    public string ActorIcon { get; set; } = "";
    public string Side { get; set; } = "";
    public bool IsEnemy { get; set; }
    public string Outcome { get; set; } = "";
    public long? OutcomeTime { get; set; }
    public string OutcomeDurationLabel { get; set; } = "";
    public int TotalDamageTaken { get; set; }
    public int StrikeDamageTaken { get; set; }
    public int MysticRebukeDamageTaken { get; set; }
    public int ConditionDamageTaken { get; set; }
    public int BarrierDamageTaken { get; set; }
    public int HitCount { get; set; }
    public int ContributorCount { get; set; }
    public bool CcImpacted { get; set; }
    public int CcImpactCount { get; set; }
    public int HardCcImpactCount { get; set; }
    public List<CombatReplayEventContributionDto> Conditions { get; set; } = [];
    public List<CombatReplayEventContributionDto> ConditionDamageBreakdown { get; set; } = [];
    public List<CombatReplayEventTimelineEntryDto> CrowdControlEffects { get; set; } = [];
    public List<CombatReplayEventContributionDto> Contributors { get; set; } = [];
    public List<CombatReplayEventTimelineEntryDto> DamageTimeline { get; set; } = [];
}

internal class CombatReplayKillEventDto : CombatReplayDownEventDto
{
}

internal class CombatReplayRecoveredEventDto : CombatReplayDownEventDto
{
    public bool UsesSupportView { get; set; }
    public int TotalDownedHealing { get; set; }
    public int DownedHealingEventCount { get; set; }
    public int RezCastCount { get; set; }
    public double RezCastDurationSeconds { get; set; }
    public int SupportContributorCount { get; set; }
    public List<CombatReplayEventContributionDto> SupportContributors { get; set; } = [];
    public List<CombatReplayEventTimelineEntryDto> SupportTimeline { get; set; } = [];
}

internal class CombatReplayConditionConversionAnalysisDto
{
    public int TotalEvents { get; set; }
    public int ConvertedEvents { get; set; }
    public int ConditionNecessaryEvents { get; set; }
    public double TotalBurningDamage { get; set; }
    public double TotalConditionDamage { get; set; }
    public double TotalVulnerabilityBonusDamage { get; set; }
    public double TotalAttributedValue { get; set; }
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

internal class CombatReplayEventSummaryEntryDto
{
    public long? BuffId { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public double Amount { get; set; }
}

internal class CombatReplayEventTimelineEntryDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Secondary { get; set; } = "";
    public bool IsHardCc { get; set; }
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
    public bool ConditionDamageNecessary { get; set; }
    public double TotalConditionDamage { get; set; }
    public double BurningDamage { get; set; }
    public double VulnerabilityBonusDamage { get; set; }
    public double TotalAttributedValue { get; set; }
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
    private const string MysticRebukeSkillName = "Mystic Rebuke";
    private const int LookbackWindow = 3000;
    private const int RecoveryRezAttributionWindow = 500;
    private const int BucketSize = 1000;
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
    private static readonly IReadOnlyList<long> ConditionConversionDamageBuffIds =
    [
        Burning,
        Poison,
        Bleeding,
        Torment,
        Confusion,
    ];
    private static readonly IReadOnlyList<long> DownContextConditionBuffIds =
    [
        Vulnerability,
        Burning,
        Poison,
        Bleeding,
        Torment,
        Confusion,
        Chilled,
        Crippled,
        Immobile,
        Fear,
        Taunt,
        Weakness,
        Blind,
    ];

    private readonly record struct DamageRecord(long Time, int TargetUniqueId, int AttackerUniqueId, int Damage, bool HasDowned, bool HasKilled);
    private readonly record struct HealingRecord(long Time, int AttackerUniqueId, int Healing);
    private readonly record struct BarrierRecord(long Time, int AttackerUniqueId, int Barrier);
    private readonly record struct CleanseRecord(long Time, int AttackerUniqueId);
    private readonly record struct StripRecord(long Time, int TargetUniqueId, int AttackerUniqueId);
    private readonly record struct EvaluationWindow(long Start, long End);
    private readonly record struct DownOutcomeInfo(string Outcome, long? TransitionTime);
    private readonly record struct DamageWindowSummary(
        int TotalDamageTaken,
        int StrikeDamageTaken,
        int MysticRebukeDamageTaken,
        int ConditionDamageTaken,
        int BarrierDamageTaken,
        int HitCount,
        int ContributorCount,
        IReadOnlyList<CombatReplayEventContributionDto> Conditions,
        IReadOnlyList<CombatReplayEventContributionDto> ConditionDamageBreakdown,
        IReadOnlyList<CombatReplayEventContributionDto> Contributors,
        IReadOnlyList<CombatReplayEventTimelineEntryDto> DamageTimeline);
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
            Defense = BuildDefenseAnalysis(log, squadPlayers, enemyAnalysis, times),
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
            Downs = BuildDownAnalysis(log, squadPlayers, hostileTargets),
            Kills = BuildKillAnalysis(log, squadPlayers, hostileTargets),
            Recovered = BuildRecoveredAnalysis(log, squadPlayers, hostileTargets),
        };
    }

    private static CombatReplayDefenseAnalysisDto BuildDefenseAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        IReadOnlyList<long> times)
    {
        var summary = new CombatReplayDefenseAnalysisDto
        {
            HasBarrierData = log.CombatData.HasEXTBarrier,
            BarrierCoverageMayBeIncomplete = log.CombatData.HasEXTBarrier,
        };

        long totalDamageToSquad = 0;
        long healthDamageToSquad = 0;
        long barrierDamageAbsorbed = 0;

        foreach (SingleActor player in squadPlayers)
        {
            foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd))
            {
                if (!damageEvent.HasHit)
                {
                    continue;
                }

                int totalDamage = damageEvent.HealthDamage;
                if (totalDamage <= 0)
                {
                    continue;
                }

                totalDamageToSquad += totalDamage;
                healthDamageToSquad += Math.Max(damageEvent.HealthDamage - damageEvent.ShieldDamage, 0);
                barrierDamageAbsorbed += damageEvent.ShieldDamage;
            }
        }

        summary.TotalDamageToSquad = totalDamageToSquad;
        summary.HealthDamageToSquad = healthDamageToSquad;
        summary.BarrierDamageAbsorbed = barrierDamageAbsorbed;
        summary.BarrierAbsorptionPercent = totalDamageToSquad > 0
            ? Math.Round(barrierDamageAbsorbed * 100.0 / totalDamageToSquad, 1)
            : 0.0;

        if (!log.CombatData.HasEXTBarrier)
        {
            return summary;
        }

        var topBarrierProviderContributions = new List<(int? ActorId, string Name, string Icon, double Amount, long EventTime)>();
        long totalBarrierGranted = 0;
        long initialBarrierOnSquad = 0;

        foreach (SingleActor player in squadPlayers)
        {
            long barrierAtStart = Math.Max(GetApproximateCurrentBarrier(player, log, Math.Max(log.LogData.LogStart, player.FirstAware)), 0);
            initialBarrierOnSquad += barrierAtStart;
        }

        foreach (SingleActor provider in squadPlayers)
        {
            long providerBarrier = provider.EXTBarrier.GetOutgoingBarrierStats(null, log, log.LogData.LogStart, log.LogData.LogEnd).Barrier;
            if (providerBarrier <= 0)
            {
                continue;
            }

            totalBarrierGranted += providerBarrier;
            topBarrierProviderContributions.Add((
                provider.UniqueID,
                provider.Character,
                provider.GetIcon(),
                providerBarrier,
                log.LogData.LogEnd));
        }

        summary.TotalBarrierGranted = totalBarrierGranted;
        summary.InitialBarrierOnSquad = initialBarrierOnSquad;
        summary.TotalBarrierAvailable = totalBarrierGranted + initialBarrierOnSquad;
        summary.BurstBarrier = BuildDefenseBurstBarrierAnalysis(log, squadPlayers, enemyAnalysis, times);
        summary.Mitigation = BuildDefenseMitigationAnalysis(log, squadPlayers);
        summary.TopBarrierProviders = BuildTopActorSummaries(topBarrierProviderContributions);
        return summary;
    }

    private static CombatReplayDefenseMitigationDto BuildDefenseMitigationAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        int[] thresholds = [10, 20, 25, 33, 50, 80];
        var result = new CombatReplayDefenseMitigationDto
        {
            Thresholds =
            [
                .. thresholds.Select(threshold => BuildDefenseMitigationThresholdAnalysis(log, squadPlayers, threshold))
            ],
        };
        return result;
    }

    private static CombatReplayDefenseMitigationThresholdDto BuildDefenseMitigationThresholdAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        int thresholdPercent)
    {
        var mitigationEvents = new List<CombatReplayDefenseMitigationEventDto>();

        foreach (SingleActor player in squadPlayers)
        {
            IReadOnlyList<Segment> healthUpdates = player.GetHealthUpdates(log);
            if (healthUpdates.Count == 0)
            {
                continue;
            }

            IReadOnlyList<DownEvent> downEvents = log.CombatData.GetDownEvents(player.AgentItem);
            IReadOnlyList<DeadEvent> deadEvents = log.CombatData.GetDeadEvents(player.AgentItem);
            int downIndex = 0;
            int deadIndex = 0;
            bool mitigationActive = false;
            long mitigationStartTime = 0;
            long previousFullHealthTime = 0;
            long mitigationPreviousFullHealthTime = 0;

            foreach (Segment healthSegment in healthUpdates.OrderBy(segment => segment.Start))
            {
                while (downIndex < downEvents.Count && downEvents[downIndex].Time <= healthSegment.Start)
                {
                    if (mitigationActive)
                    {
                        mitigationActive = false;
                    }
                    downIndex++;
                }

                while (deadIndex < deadEvents.Count && deadEvents[deadIndex].Time <= healthSegment.Start)
                {
                    if (mitigationActive)
                    {
                        mitigationActive = false;
                    }
                    deadIndex++;
                }

                bool aliveAndUpright =
                    !player.IsDowned(log, healthSegment.Start) &&
                    !player.IsDead(log, healthSegment.Start) &&
                    !player.IsDC(log, healthSegment.Start);

                if (aliveAndUpright && healthSegment.Value >= 100.0)
                {
                    previousFullHealthTime = healthSegment.Start;
                }

                if (!mitigationActive)
                {
                    if (
                        healthSegment.Value <= thresholdPercent &&
                        aliveAndUpright
                    )
                    {
                        mitigationActive = true;
                        mitigationStartTime = healthSegment.Start;
                        mitigationPreviousFullHealthTime = previousFullHealthTime;
                        if (mitigationPreviousFullHealthTime == 0)
                        {
                            mitigationPreviousFullHealthTime = FindPreviousFullHealthTime(player, log, mitigationStartTime);
                        }
                    }
                    continue;
                }

                if (aliveAndUpright && healthSegment.Value >= 100.0)
                {
                    long mitigationWindowStart = mitigationPreviousFullHealthTime > 0 ? mitigationPreviousFullHealthTime : mitigationStartTime;
                    double mitigationWindowStartHealthPercent = GetSafePercent(player.GetCurrentHealthPercent(log, mitigationWindowStart));
                    (long lowestHealthTime, double lowestHealthPercent) = GetLowestHealthPoint(player, log, mitigationWindowStart, healthSegment.Start, mitigationWindowStartHealthPercent);
                    int lowestHealthEstimate = (int)Math.Round(CombatReplayDefenseMitigationEventDto.EstimatedPlayerMaxHealth * lowestHealthPercent / 100.0, 0);
                    double barrierAbsorbedToLowest = 0.0;
                    if (lowestHealthTime > mitigationWindowStart)
                    {
                        barrierAbsorbedToLowest = Math.Round(player
                            .GetDamageTakenEvents(null, log, mitigationWindowStart, lowestHealthTime + 1)
                            .Where(damageEvent => damageEvent.HasHit && damageEvent.ShieldDamage > 0)
                            .Sum(damageEvent => (double)damageEvent.ShieldDamage), 1);
                    }
                    bool barrierSavedPlayer = barrierAbsorbedToLowest > 0 && lowestHealthEstimate - barrierAbsorbedToLowest <= 0;
                    double protectionEstimatedMitigation = GetEstimatedProtectionMitigation(player, log, mitigationWindowStart, healthSegment.Start);
                    double incomingDamage = Math.Round(player
                        .GetDamageTakenEvents(null, log, mitigationWindowStart, healthSegment.Start)
                        .Where(damageEvent => damageEvent.HasHit && (damageEvent.HealthDamage > 0 || damageEvent.ShieldDamage > 0))
                        .Sum(damageEvent => (double)damageEvent.HealthDamage), 1);
                    double incomingHealing = log.CombatData.HasEXTHealing
                        ? Math.Round(player.EXTHealing
                            .GetIncomingHealEvents(null, log, mitigationWindowStart, healthSegment.Start)
                            .Where(healingEvent => healingEvent.HealingDone > 0)
                            .Sum(healingEvent => (double)healingEvent.HealingDone), 1)
                        : 0.0;
                    mitigationEvents.Add(new CombatReplayDefenseMitigationEventDto
                    {
                        Time = mitigationStartTime,
                        TimeLabel = FormatTime(mitigationStartTime),
                        PreviousFullHealthTime = mitigationPreviousFullHealthTime,
                        PreviousFullHealthTimeLabel = mitigationPreviousFullHealthTime > 0 ? FormatTime(mitigationPreviousFullHealthTime) : "",
                        LowestHealthTime = lowestHealthTime,
                        LowestHealthTimeLabel = FormatTime(lowestHealthTime),
                        RecoveryTime = healthSegment.Start,
                        RecoveryTimeLabel = FormatTime(healthSegment.Start),
                        ActorId = player.UniqueID,
                        Name = player.Character,
                        Icon = player.GetIcon(),
                        LowestHealthPercent = lowestHealthPercent,
                        LowestHealthEstimate = lowestHealthEstimate,
                        BarrierAbsorbedToLowest = barrierAbsorbedToLowest,
                        BarrierSavedPlayer = barrierSavedPlayer,
                        ProtectionEstimatedMitigation = protectionEstimatedMitigation,
                        IncomingDamage = incomingDamage,
                        IncomingHealing = incomingHealing,
                        Effects = BuildMitigationEffects(log, player, mitigationWindowStart, healthSegment.Start),
                    });
                    mitigationActive = false;
                }
            }
        }

        mitigationEvents = [.. mitigationEvents.OrderBy(evt => evt.Time).ThenBy(evt => evt.Name, StringComparer.OrdinalIgnoreCase)];
        return new CombatReplayDefenseMitigationThresholdDto
        {
            ThresholdPercent = thresholdPercent,
            Count = mitigationEvents.Count,
            Events = mitigationEvents,
        };
    }

    private static long FindPreviousFullHealthTime(SingleActor player, ParsedEvtcLog log, long mitigationStartTime)
    {
        long lowerBound = Math.Max(player.FirstAware, log.LogData.LogStart);
        if (mitigationStartTime < lowerBound)
        {
            return 0;
        }

        const long coarseStep = 100;
        const long fineStep = 10;
        long coarseCandidate = 0;
        for (long probe = mitigationStartTime; probe >= lowerBound; probe -= coarseStep)
        {
            if (IsAliveUprightAt(player, log, probe) && GetSafePercent(player.GetCurrentHealthPercent(log, probe)) >= 100.0)
            {
                coarseCandidate = probe;
                break;
            }
        }

        if (coarseCandidate == 0)
        {
            if (IsAliveUprightAt(player, log, lowerBound) && GetSafePercent(player.GetCurrentHealthPercent(log, lowerBound)) >= 100.0)
            {
                return lowerBound;
            }
            return 0;
        }

        long refinedCandidate = coarseCandidate;
        long refineEnd = Math.Min(mitigationStartTime, coarseCandidate + coarseStep - 1);
        for (long probe = coarseCandidate; probe <= refineEnd; probe += fineStep)
        {
            if (IsAliveUprightAt(player, log, probe) && GetSafePercent(player.GetCurrentHealthPercent(log, probe)) >= 100.0)
            {
                refinedCandidate = probe;
            }
        }

        long preciseStart = Math.Max(coarseCandidate, refinedCandidate - fineStep);
        long preciseEnd = Math.Min(mitigationStartTime, refinedCandidate + fineStep);
        for (long probe = preciseStart; probe <= preciseEnd; probe++)
        {
            if (IsAliveUprightAt(player, log, probe) && GetSafePercent(player.GetCurrentHealthPercent(log, probe)) >= 100.0)
            {
                refinedCandidate = probe;
            }
        }

        return refinedCandidate;
    }

    private static bool IsAliveUprightAt(SingleActor player, ParsedEvtcLog log, long time)
    {
        return !player.IsDowned(log, time) && !player.IsDead(log, time) && !player.IsDC(log, time);
    }

    private static List<CombatReplayMitigationEffectDto> BuildMitigationEffects(
        ParsedEvtcLog log,
        SingleActor player,
        long start,
        long end)
    {
        if (end <= start)
        {
            return [];
        }

        var effects = new List<CombatReplayMitigationEffectDto>();
        IReadOnlyList<TrackedMitigationBuff> trackedMitigationBuffs =
        [
            new() { Name = "Protection", BuffIds = [Protection, ProtectionUnstrippable] },
            new() { Name = "Resolution", BuffIds = [Resolution, ResolutionUnstrippable] },
            new() { Name = "Aegis", BuffIds = [Aegis] },
            new() { Name = "Frost Aura", BuffIds = [FrostAura] },
            new() { Name = "Light Aura", BuffIds = [LightAura] },
            new() { Name = "Dark Aura", BuffIds = [DarkAura] },
            new() { Name = "Distortion / Blur", BuffIds = [Blur] },
            new() { Name = "Determined", BuffIds = [Determined762, Determined785, Determined788, Determined895, Determined3892, Determined31450, Determined52271] },
            new() { Name = "Invulnerability", BuffIds = [Invulnerability757, Invulnerability56227, Invulnerability801] },
            new() { Name = "Spawn Protection", BuffIds = [SpawnProtection] },
            new() { Name = "Strength in Numbers", BuffIds = [StrengthinNumbers] },
            new() { Name = "Obsidian Flesh", BuffIds = [ObsidianFlesh] },
            new() { Name = "Rebound", BuffIds = [Rebound] },
            new() { Name = "Spectrum Shield", BuffIds = [SpectrumShieldBuff] },
            new() { Name = "Barrier Signet", BuffIds = [BarrierSignet, BarrierSignetJDrive] },
            new() { Name = "Renewed Focus", BuffIds = [RenewedFocus] },
            new() { Name = "Death Shroud", BuffIds = [DeathShroud] },
            new() { Name = "Signet of Stone", BuffIds = [SignetOfStoneActive] },
            new() { Name = "Guard!", BuffIds = [GuardBuff] },
            new() { Name = "Dolyak Stance", BuffIds = [DolyakStanceBuff] },
            new() { Name = "Defy Pain", BuffIds = [DefyPainSoulbeastBuff] },
            new() { Name = "Infuse Light", BuffIds = [InfuseLight] },
            new() { Name = "Vengeful Hammers", BuffIds = [VengefulHammersBuff] },
            new() { Name = "Urn of Saint Viktor", BuffIds = [UrnOfSaintViktorBuff] },
            new() { Name = "Shielding Hands", BuffIds = [ShieldingHandsBuff] },
            new() { Name = "Shadow Shroud", BuffIds = [ShadowShroud] },
        ];

        double barrierSeconds = GetBarrierPresenceSeconds(player, log, start, end);
        if (barrierSeconds > 0)
        {
            effects.Add(new CombatReplayMitigationEffectDto
            {
                Name = "Barrier",
                Icon = log.SkillData.Get(BarrierBurst).Icon,
                Seconds = barrierSeconds,
            });
        }

        double dodgeSeconds = GetDodgeSeconds(player, log, start, end);
        if (dodgeSeconds > 0)
        {
            effects.Add(new CombatReplayMitigationEffectDto
            {
                Name = "Dodge",
                Icon = log.SkillData.Get(log.SkillData.DodgeID).Icon,
                Seconds = dodgeSeconds,
            });
        }

        foreach (TrackedMitigationBuff trackedBuff in trackedMitigationBuffs)
        {
            double seconds = GetBuffPresenceSeconds(player, log, trackedBuff.BuffIds, start, end);
            if (seconds <= 0)
            {
                continue;
            }

            effects.Add(new CombatReplayMitigationEffectDto
            {
                Name = trackedBuff.Name,
                Icon = GetMitigationEffectIcon(log, trackedBuff.BuffIds),
                Seconds = seconds,
            });
        }

        return [.. effects
            .OrderByDescending(effect => effect.Seconds)
            .ThenBy(effect => effect.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static string GetMitigationEffectIcon(ParsedEvtcLog log, IReadOnlyList<long> buffIds)
    {
        foreach (long buffId in buffIds)
        {
            if (log.Buffs.BuffsByIDs.TryGetValue(buffId, out Buff? buff))
            {
                if (!string.IsNullOrEmpty(buff.Link))
                {
                    return buff.Link;
                }
            }
            SkillItem skill = log.SkillData.Get(buffId);
            if (!string.IsNullOrEmpty(skill.Icon))
            {
                return skill.Icon;
            }
        }
        return "";
    }

    private static double GetBuffPresenceSeconds(
        SingleActor player,
        ParsedEvtcLog log,
        IReadOnlyList<long> buffIds,
        long start,
        long end)
    {
        List<(long Start, long End)> ranges = GetMergedBuffPresenceRanges(player, log, buffIds, start, end);
        if (ranges.Count == 0)
        {
            return 0.0;
        }
        double milliseconds = ranges.Sum(range => (double)(range.End - range.Start));
        return Math.Round(milliseconds / 1000.0, 1);
    }

    private static List<(long Start, long End)> GetMergedBuffPresenceRanges(
        SingleActor player,
        ParsedEvtcLog log,
        IReadOnlyList<long> buffIds,
        long start,
        long end)
    {
        var ranges = new List<(long Start, long End)>();
        foreach (long buffId in buffIds)
        {
            if (!log.Buffs.BuffsByIDs.ContainsKey(buffId))
            {
                continue;
            }
            foreach (Segment segment in player.GetBuffStatus(log, buffId, start, end))
            {
                if (segment.Value <= 0)
                {
                    continue;
                }

                long overlapStart = Math.Max(start, segment.Start);
                long overlapEnd = Math.Min(end, segment.End);
                if (overlapEnd > overlapStart)
                {
                    ranges.Add((overlapStart, overlapEnd));
                }
            }
        }

        if (ranges.Count == 0)
        {
            return [];
        }

        ranges.Sort((left, right) => left.Start != right.Start ? left.Start.CompareTo(right.Start) : left.End.CompareTo(right.End));
        var mergedRanges = new List<(long Start, long End)> { ranges[0] };
        for (int i = 1; i < ranges.Count; i++)
        {
            (long rangeStart, long rangeEnd) = ranges[i];
            (long currentStart, long currentEnd) = mergedRanges[^1];
            if (rangeStart <= currentEnd)
            {
                mergedRanges[^1] = (currentStart, Math.Max(currentEnd, rangeEnd));
            }
            else
            {
                mergedRanges.Add((rangeStart, rangeEnd));
            }
        }

        return mergedRanges;
    }

    private static double GetEstimatedProtectionMitigation(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end)
    {
        if (end <= start)
        {
            return 0.0;
        }

        List<(long Start, long End)> protectionRanges = GetMergedBuffPresenceRanges(player, log, [Protection, ProtectionUnstrippable], start, end);
        if (protectionRanges.Count == 0)
        {
            return 0.0;
        }

        double preventedDamage = 0.0;
        foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, start, end))
        {
            if (!damageEvent.HasHit || damageEvent.HealthDamage <= 0 || damageEvent.ConditionDamageBased(log))
            {
                continue;
            }
            if (!protectionRanges.Any(range => damageEvent.Time >= range.Start && damageEvent.Time < range.End))
            {
                continue;
            }

            preventedDamage += damageEvent.HealthDamage * (0.33 / 0.67);
        }

        return Math.Round(preventedDamage, 1);
    }

    private static double GetBarrierPresenceSeconds(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end)
    {
        const long sampleStep = 100;
        double milliseconds = 0.0;
        for (long sampleTime = start; sampleTime < end; sampleTime += sampleStep)
        {
            long nextTime = Math.Min(end, sampleTime + sampleStep);
            if (GetSafePercent(player.GetCurrentBarrierPercent(log, sampleTime)) > 0)
            {
                milliseconds += nextTime - sampleTime;
            }
        }
        return Math.Round(milliseconds / 1000.0, 1);
    }

    private static double GetDodgeSeconds(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end)
    {
        double milliseconds = 0.0;
        foreach (CastEvent castEvent in player.GetCastEvents(log, start, end).Where(castEvent => castEvent.Skill.IsDodge(log.SkillData)))
        {
            long overlapStart = Math.Max(start, castEvent.Time);
            long overlapEnd = Math.Min(end, castEvent.EndTime);
            if (overlapEnd > overlapStart)
            {
                milliseconds += overlapEnd - overlapStart;
            }
        }
        return Math.Round(milliseconds / 1000.0, 1);
    }

    private static CombatReplayDefenseBurstBarrierDto BuildDefenseBurstBarrierAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        IReadOnlyList<long> times)
    {
        var summary = new CombatReplayDefenseBurstBarrierDto();
        List<EvaluationWindow> burstWindows = BuildBurstWindows(enemyAnalysis, times);
        summary.EnemyBurstWindows = burstWindows.Count;
        if (burstWindows.Count == 0)
        {
            return summary;
        }

        var heldWindowShares = new List<double>();
        var downedWindowShares = new List<double>();
        var lowHealthSurvivorRows = new List<(SingleActor Player, long Time, double LowestHealthPercent)>();

        foreach (EvaluationWindow window in burstWindows)
        {
            long windowDamage = 0;
            long windowBarrierAbsorbed = 0;
            bool hadSquadDown = false;

            foreach (SingleActor player in squadPlayers)
            {
                foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, window.Start, window.End))
                {
                    if (!damageEvent.HasHit || damageEvent.HealthDamage <= 0)
                    {
                        continue;
                    }

                    windowDamage += damageEvent.HealthDamage;
                    windowBarrierAbsorbed += damageEvent.ShieldDamage;
                }

                if (!hadSquadDown && log.CombatData.GetDownEvents(player.AgentItem).Any(evt => evt.Time >= window.Start && evt.Time <= window.End))
                {
                    hadSquadDown = true;
                }
            }

            if (windowBarrierAbsorbed > 0)
            {
                summary.BurstWindowsWithBarrierAbsorbed++;
            }
            if (hadSquadDown)
            {
                summary.BurstWindowsWithSquadDown++;
            }
            else
            {
                summary.BurstWindowsHeld++;
            }

            summary.TotalBurstDamageToSquad += windowDamage;
            summary.TotalBurstBarrierAbsorbed += windowBarrierAbsorbed;

            double windowShare = windowDamage > 0 ? windowBarrierAbsorbed * 100.0 / windowDamage : 0.0;
            if (hadSquadDown)
            {
                downedWindowShares.Add(windowShare);
            }
            else
            {
                heldWindowShares.Add(windowShare);
            }

            if (windowShare > 0)
            {
                foreach (SingleActor player in squadPlayers)
                {
                    double healthPercentStart = GetSafePercent(player.GetCurrentHealthPercent(log, window.Start));
                    (long lowestHealthTime, double lowestHealthPercent) = GetLowestHealthPoint(player, log, window.Start, window.End, healthPercentStart);
                    if (lowestHealthPercent > 0 && lowestHealthPercent <= windowShare && !player.IsDowned(log, window.Start, window.End))
                    {
                        lowHealthSurvivorRows.Add((player, lowestHealthTime, lowestHealthPercent));
                    }
                }
            }
        }

        summary.TotalBurstDamageToSquad = Math.Round(summary.TotalBurstDamageToSquad, 1);
        summary.TotalBurstBarrierAbsorbed = Math.Round(summary.TotalBurstBarrierAbsorbed, 1);
        summary.BurstBarrierAbsorptionPercent = summary.TotalBurstDamageToSquad > 0
            ? Math.Round(summary.TotalBurstBarrierAbsorbed * 100.0 / summary.TotalBurstDamageToSquad, 1)
            : 0.0;
        summary.AverageBurstDamageToSquad = Math.Round(summary.TotalBurstDamageToSquad / burstWindows.Count, 1);
        summary.AverageBurstBarrierAbsorbed = Math.Round(summary.TotalBurstBarrierAbsorbed / burstWindows.Count, 1);
        summary.HeldBurstBarrierShare = heldWindowShares.Count > 0 ? Math.Round(heldWindowShares.Average(), 1) : 0.0;
        summary.DownedBurstBarrierShare = downedWindowShares.Count > 0 ? Math.Round(downedWindowShares.Average(), 1) : 0.0;
        List<(SingleActor Player, long Time, double LowestHealthPercent)> uniqueLowHealthSurvivorRows = [.. lowHealthSurvivorRows
            .GroupBy(row => (row.Player.UniqueID, row.Time))
            .Select(group => group
                .OrderBy(row => row.LowestHealthPercent)
                .First())
            .OrderBy(row => row.Time)
            .ThenBy(row => row.LowestHealthPercent)];
        summary.LowHealthSurvivorOccurrences = uniqueLowHealthSurvivorRows.Count;
        summary.LowHealthSurvivorPlayers = uniqueLowHealthSurvivorRows.Select(row => row.Player.UniqueID).Distinct().Count();
        summary.LowHealthSurvivorEvents = [.. uniqueLowHealthSurvivorRows
            .Select(row => new CombatReplayDefenseBurstSurvivorEventDto
            {
                Time = row.Time,
                TimeLabel = FormatTime(row.Time),
                ActorId = row.Player.UniqueID,
                Name = row.Player.Character,
                Icon = row.Player.GetIcon(),
                LowestHealthPercent = Math.Round(row.LowestHealthPercent, 1),
            })];
        summary.Takeaways = BuildDefenseBurstBarrierTakeaways(summary);
        return summary;
    }

    private static CombatReplayDownAnalysisDto BuildDownAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        var events = new List<CombatReplayDownEventDto>();
        events.AddRange(BuildDownEvents(log, squadPlayers, "Squad", false));
        events.AddRange(BuildDownEvents(log, hostileTargets, "Enemy", true));
        events.Sort((left, right) => left.Time.CompareTo(right.Time));
        return new CombatReplayDownAnalysisDto
        {
            CombinedSummary = BuildDownSummary(events),
            SquadSummary = BuildDownSummary([.. events.Where(evt => !evt.IsEnemy)]),
            EnemySummary = BuildDownSummary([.. events.Where(evt => evt.IsEnemy)]),
            Events = events,
        };
    }

    private static IEnumerable<CombatReplayDownEventDto> BuildDownEvents(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        string side,
        bool isEnemy)
    {
        foreach (SingleActor actor in actors)
        {
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                yield return BuildDownEvent(log, actor, downEvent, side, isEnemy);
            }
        }
    }

    private static CombatReplayDownEventDto BuildDownEvent(
        ParsedEvtcLog log,
        SingleActor actor,
        DownEvent downEvent,
        string side,
        bool isEnemy)
    {
        DownOutcomeInfo outcomeInfo = GetDownOutcomeInfo(log, actor.AgentItem, downEvent.Time);
        long windowStart = Math.Max(log.LogData.LogStart, downEvent.Time - LookbackWindow);
        long conditionSnapshotTime = Math.Max(log.LogData.LogStart, downEvent.Time - 1);
        DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, windowStart, downEvent.Time, conditionSnapshotTime);
        (List<CombatReplayEventTimelineEntryDto> crowdControlEffects, int hardCcImpactCount) = BuildDownCrowdControlEffects(log, actor, windowStart, downEvent.Time, isEnemy);

        return new CombatReplayDownEventDto
        {
            Time = downEvent.Time,
            TimeLabel = FormatTime(downEvent.Time),
            WindowStart = windowStart,
            WindowStartLabel = FormatTime(windowStart),
            ConditionSnapshotTime = conditionSnapshotTime,
            ConditionSnapshotTimeLabel = FormatTime(conditionSnapshotTime),
            ActorId = actor.UniqueID,
            ActorName = actor.Character,
            ActorIcon = actor.GetIcon(),
            Side = side,
            IsEnemy = isEnemy,
            Outcome = outcomeInfo.Outcome,
            OutcomeTime = outcomeInfo.TransitionTime,
            OutcomeDurationLabel = outcomeInfo.TransitionTime.HasValue
                ? FormatDuration(outcomeInfo.TransitionTime.Value - downEvent.Time)
                : "",
            TotalDamageTaken = summary.TotalDamageTaken,
            StrikeDamageTaken = summary.StrikeDamageTaken,
            MysticRebukeDamageTaken = summary.MysticRebukeDamageTaken,
            ConditionDamageTaken = summary.ConditionDamageTaken,
            BarrierDamageTaken = summary.BarrierDamageTaken,
            HitCount = summary.HitCount,
            ContributorCount = summary.ContributorCount,
            CcImpacted = crowdControlEffects.Count > 0,
            CcImpactCount = crowdControlEffects.Count,
            HardCcImpactCount = hardCcImpactCount,
            Conditions = [.. summary.Conditions],
            ConditionDamageBreakdown = [.. summary.ConditionDamageBreakdown],
            CrowdControlEffects = crowdControlEffects,
            Contributors = [.. summary.Contributors],
            DamageTimeline = [.. summary.DamageTimeline],
        };
    }

    private static CombatReplayDownSummaryDto BuildDownSummary(IReadOnlyList<CombatReplayDownEventDto> events)
    {
        var summary = new CombatReplayDownSummaryDto
        {
            SquadDowns = events.Count(evt => !evt.IsEnemy),
            EnemyDowns = events.Count(evt => evt.IsEnemy),
            CcImpactedDowns = events.Count(evt => evt.CcImpacted),
            ConditionImpactedDowns = events.Count(evt => evt.ConditionDamageTaken > 0),
            MysticRebukeDowns = events.Count(evt => evt.MysticRebukeDamageTaken > 0),
            HardCcDowns = events.Count(evt => evt.HardCcImpactCount > 0),
            SoftCcDowns = events.Count(evt => evt.CrowdControlEffects.Any(effect => !effect.IsHardCc)),
            BurningDowns = events.Count(evt => evt.ConditionDamageBreakdown.Any(entry => entry.BuffId == Burning && entry.Amount > 0)),
            ConditionMajorityDowns = events.Count(evt => evt.ConditionDamageTaken > evt.StrikeDamageTaken),
            MysticRebukeHeavyDowns = events.Count(evt => evt.StrikeDamageTaken > 0 && evt.MysticRebukeDamageTaken >= evt.StrikeDamageTaken * 0.10),
            TotalDamage = Math.Round(events.Sum(evt => (double)evt.TotalDamageTaken), 1),
            TotalStrikeDamage = Math.Round(events.Sum(evt => (double)evt.StrikeDamageTaken), 1),
            TotalConditionDamage = Math.Round(events.Sum(evt => (double)evt.ConditionDamageTaken), 1),
            TotalMysticRebukeDamage = Math.Round(events.Sum(evt => (double)evt.MysticRebukeDamageTaken), 1),
        };
        summary.BothCcDowns = events.Count(evt => evt.HardCcImpactCount > 0 && evt.CrowdControlEffects.Any(effect => !effect.IsHardCc));
        summary.TotalBurningDamage = Math.Round(events.Sum(evt =>
            evt.ConditionDamageBreakdown
                .Where(entry => entry.BuffId == Burning)
                .Sum(entry => (double)entry.Amount)), 1);
        summary.AverageMysticRebukeDamage = summary.MysticRebukeDowns > 0
            ? Math.Round(summary.TotalMysticRebukeDamage / summary.MysticRebukeDowns, 1)
            : 0.0;

        List<CombatReplayDownEventDto> squadCcEvents = [.. events.Where(evt => !evt.IsEnemy && evt.CrowdControlEffects.Any(effect => !effect.IsHardCc))];
        summary.AverageEffectiveSoftCcSeconds = squadCcEvents.Count > 0
            ? Math.Round(squadCcEvents
                .SelectMany(evt => evt.CrowdControlEffects.Where(effect => !effect.IsHardCc))
                .Select(ParseEffectiveCcSeconds)
                .Where(seconds => seconds > 0.0)
                .DefaultIfEmpty(0.0)
                .Average(), 1)
            : 0.0;

        summary.TopContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    contributor.Amount,
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.MysticRebukeContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Mystic Rebuke"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.ConditionContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Condition"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.TopConditions = BuildTopSummaryEntries(events.SelectMany(evt =>
            evt.ConditionDamageBreakdown.Select(entry => (entry.Name, entry.Icon, entry.Amount, GetEventSummaryKey(evt)))));
        summary.TopHardCcSources = BuildTopSummaryEntries(events.SelectMany(evt =>
            evt.CrowdControlEffects
                .Where(effect => effect.IsHardCc)
                .Select(effect => (effect.Label, "", 0.0, GetEventSummaryKey(evt)))));
        summary.TopSoftCcSources = BuildTopSummaryEntries(events.SelectMany(evt =>
            evt.CrowdControlEffects
                .Where(effect => !effect.IsHardCc)
                .Select(effect => (effect.Label, "", ParseEffectiveCcSeconds(effect), GetEventSummaryKey(evt)))));
        summary.Takeaways = BuildDownSummaryTakeaways(summary);
        return summary;
    }

    private static CombatReplayKillAnalysisDto BuildKillAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        var events = new List<CombatReplayKillEventDto>();
        events.AddRange(BuildKillEvents(log, squadPlayers, "Squad", false));
        events.AddRange(BuildKillEvents(log, hostileTargets, "Enemy", true));
        events.Sort((left, right) => left.Time.CompareTo(right.Time));
        return new CombatReplayKillAnalysisDto
        {
            CombinedSummary = BuildKillSummary(events),
            SquadSummary = BuildKillSummary([.. events.Where(evt => !evt.IsEnemy)]),
            EnemySummary = BuildKillSummary([.. events.Where(evt => evt.IsEnemy)]),
            Events = events,
        };
    }

    private static IEnumerable<CombatReplayKillEventDto> BuildKillEvents(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        string side,
        bool isEnemy)
    {
        foreach (SingleActor actor in actors)
        {
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                CombatReplayKillEventDto? killEvent = BuildKillEvent(log, actor, downEvent, side, isEnemy);
                if (killEvent != null)
                {
                    yield return killEvent;
                }
            }
        }
    }

    private static CombatReplayKillEventDto? BuildKillEvent(
        ParsedEvtcLog log,
        SingleActor actor,
        DownEvent downEvent,
        string side,
        bool isEnemy)
    {
        DownOutcomeInfo outcomeInfo = GetDownOutcomeInfo(log, actor.AgentItem, downEvent.Time);
        if (!string.Equals(outcomeInfo.Outcome, "Killed", StringComparison.OrdinalIgnoreCase)
            || !outcomeInfo.TransitionTime.HasValue)
        {
            return null;
        }

        long killTime = outcomeInfo.TransitionTime.Value;
        long conditionSnapshotTime = Math.Max(log.LogData.LogStart, killTime - 1);
        DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, downEvent.Time, killTime, conditionSnapshotTime);
        return new CombatReplayKillEventDto
        {
            Time = killTime,
            TimeLabel = FormatTime(killTime),
            WindowStart = downEvent.Time,
            WindowStartLabel = FormatTime(downEvent.Time),
            ConditionSnapshotTime = conditionSnapshotTime,
            ConditionSnapshotTimeLabel = FormatTime(conditionSnapshotTime),
            ActorId = actor.UniqueID,
            ActorName = actor.Character,
            ActorIcon = actor.GetIcon(),
            Side = side,
            IsEnemy = isEnemy,
            Outcome = "Killed",
            OutcomeTime = killTime,
            OutcomeDurationLabel = FormatDuration(killTime - downEvent.Time),
            TotalDamageTaken = summary.TotalDamageTaken,
            StrikeDamageTaken = summary.StrikeDamageTaken,
            MysticRebukeDamageTaken = summary.MysticRebukeDamageTaken,
            ConditionDamageTaken = summary.ConditionDamageTaken,
            BarrierDamageTaken = summary.BarrierDamageTaken,
            HitCount = summary.HitCount,
            ContributorCount = summary.ContributorCount,
            Conditions = [.. summary.Conditions],
            ConditionDamageBreakdown = [.. summary.ConditionDamageBreakdown],
            Contributors = [.. summary.Contributors],
            DamageTimeline = [.. summary.DamageTimeline],
        };
    }

    private static CombatReplayKillSummaryDto BuildKillSummary(IReadOnlyList<CombatReplayKillEventDto> events)
    {
        var summary = new CombatReplayKillSummaryDto
        {
            SquadKills = events.Count(evt => !evt.IsEnemy),
            EnemyKills = events.Count(evt => evt.IsEnemy),
            ConditionImpactedKills = events.Count(evt => evt.ConditionDamageTaken > 0),
            MysticRebukeKills = events.Count(evt => evt.MysticRebukeDamageTaken > 0),
            BurningKills = events.Count(evt => evt.ConditionDamageBreakdown.Any(entry => entry.BuffId == Burning && entry.Amount > 0)),
            ConditionMajorityKills = events.Count(evt => evt.ConditionDamageTaken > evt.StrikeDamageTaken),
            MysticRebukeHeavyKills = events.Count(evt => evt.StrikeDamageTaken > 0 && evt.MysticRebukeDamageTaken >= evt.StrikeDamageTaken * 0.10),
            TotalDamage = Math.Round(events.Sum(evt => (double)evt.TotalDamageTaken), 1),
            TotalStrikeDamage = Math.Round(events.Sum(evt => (double)evt.StrikeDamageTaken), 1),
            TotalConditionDamage = Math.Round(events.Sum(evt => (double)evt.ConditionDamageTaken), 1),
            TotalMysticRebukeDamage = Math.Round(events.Sum(evt => (double)evt.MysticRebukeDamageTaken), 1),
            TotalBarrierDamage = Math.Round(events.Sum(evt => (double)evt.BarrierDamageTaken), 1),
        };
        summary.TotalBurningDamage = Math.Round(events.Sum(evt =>
            evt.ConditionDamageBreakdown
                .Where(entry => entry.BuffId == Burning)
                .Sum(entry => (double)entry.Amount)), 1);
        summary.AverageKillTimeSeconds = events.Count > 0
            ? Math.Round(events.Average(evt => (evt.Time - evt.WindowStart) / 1000.0), 1)
            : 0.0;
        summary.AverageMysticRebukeDamage = summary.MysticRebukeKills > 0
            ? Math.Round(summary.TotalMysticRebukeDamage / summary.MysticRebukeKills, 1)
            : 0.0;
        summary.FinishContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    contributor.Amount,
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.MysticRebukeContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Mystic Rebuke"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.ConditionContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Condition"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.TopConditions = BuildTopSummaryEntries(events.SelectMany(evt =>
            evt.ConditionDamageBreakdown.Select(entry => (entry.Name, entry.Icon, entry.Amount, GetEventSummaryKey(evt)))));
        summary.Takeaways = BuildKillSummaryTakeaways(summary);
        return summary;
    }

    private static CombatReplayRecoveredAnalysisDto BuildRecoveredAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets)
    {
        var events = new List<CombatReplayRecoveredEventDto>();
        events.AddRange(BuildRecoveredEvents(log, squadPlayers, squadPlayers, "Squad", false));
        events.AddRange(BuildRecoveredEvents(log, hostileTargets, squadPlayers, "Enemy", true));
        events.Sort((left, right) => left.Time.CompareTo(right.Time));
        return new CombatReplayRecoveredAnalysisDto
        {
            SquadSummary = BuildRecoveredSquadSummary([.. events.Where(evt => !evt.IsEnemy && evt.UsesSupportView)]),
            EnemySummary = BuildRecoveredEnemySummary([.. events.Where(evt => evt.IsEnemy && !evt.UsesSupportView)]),
            Events = events,
        };
    }

    private static IEnumerable<CombatReplayRecoveredEventDto> BuildRecoveredEvents(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> actors,
        IReadOnlyList<SingleActor> squadPlayers,
        string side,
        bool isEnemy)
    {
        foreach (SingleActor actor in actors)
        {
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                CombatReplayRecoveredEventDto? recoveredEvent = BuildRecoveredEvent(log, actor, squadPlayers, downEvent, side, isEnemy);
                if (recoveredEvent != null)
                {
                    yield return recoveredEvent;
                }
            }
        }
    }

    private static CombatReplayRecoveredEventDto? BuildRecoveredEvent(
        ParsedEvtcLog log,
        SingleActor actor,
        IReadOnlyList<SingleActor> squadPlayers,
        DownEvent downEvent,
        string side,
        bool isEnemy)
    {
        DownOutcomeInfo outcomeInfo = GetDownOutcomeInfo(log, actor.AgentItem, downEvent.Time);
        if (!string.Equals(outcomeInfo.Outcome, "Recovered", StringComparison.OrdinalIgnoreCase)
            || !outcomeInfo.TransitionTime.HasValue)
        {
            return null;
        }

        long recoveredTime = outcomeInfo.TransitionTime.Value;
        long conditionSnapshotTime = Math.Max(log.LogData.LogStart, recoveredTime - 1);
        if (isEnemy)
        {
            DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, downEvent.Time, recoveredTime, conditionSnapshotTime);
            return new CombatReplayRecoveredEventDto
            {
                Time = recoveredTime,
                TimeLabel = FormatTime(recoveredTime),
                WindowStart = downEvent.Time,
                WindowStartLabel = FormatTime(downEvent.Time),
                ConditionSnapshotTime = conditionSnapshotTime,
                ConditionSnapshotTimeLabel = FormatTime(conditionSnapshotTime),
                ActorId = actor.UniqueID,
                ActorName = actor.Character,
                ActorIcon = actor.GetIcon(),
                Side = side,
                IsEnemy = true,
                Outcome = "Recovered",
                OutcomeTime = recoveredTime,
                OutcomeDurationLabel = FormatDuration(recoveredTime - downEvent.Time),
                TotalDamageTaken = summary.TotalDamageTaken,
                StrikeDamageTaken = summary.StrikeDamageTaken,
                MysticRebukeDamageTaken = summary.MysticRebukeDamageTaken,
                ConditionDamageTaken = summary.ConditionDamageTaken,
                BarrierDamageTaken = summary.BarrierDamageTaken,
                HitCount = summary.HitCount,
                ContributorCount = summary.ContributorCount,
                Conditions = [.. summary.Conditions],
                ConditionDamageBreakdown = [.. summary.ConditionDamageBreakdown],
                Contributors = [.. summary.Contributors],
                DamageTimeline = [.. summary.DamageTimeline],
            };
        }

        RecoverySupportSummary supportSummary = BuildRecoverySupportSummary(log, actor, squadPlayers, downEvent.Time, recoveredTime);
        return new CombatReplayRecoveredEventDto
        {
            Time = recoveredTime,
            TimeLabel = FormatTime(recoveredTime),
            WindowStart = downEvent.Time,
            WindowStartLabel = FormatTime(downEvent.Time),
            ConditionSnapshotTime = conditionSnapshotTime,
            ConditionSnapshotTimeLabel = FormatTime(conditionSnapshotTime),
            ActorId = actor.UniqueID,
            ActorName = actor.Character,
            ActorIcon = actor.GetIcon(),
            Side = side,
            IsEnemy = false,
            Outcome = "Recovered",
            OutcomeTime = recoveredTime,
            OutcomeDurationLabel = FormatDuration(recoveredTime - downEvent.Time),
            UsesSupportView = true,
            TotalDownedHealing = supportSummary.TotalDownedHealing,
            DownedHealingEventCount = supportSummary.DownedHealingEventCount,
            RezCastCount = supportSummary.RezCastCount,
            RezCastDurationSeconds = supportSummary.RezCastDurationSeconds,
            SupportContributorCount = supportSummary.SupportContributorCount,
            SupportContributors = [.. supportSummary.SupportContributors],
            SupportTimeline = [.. supportSummary.SupportTimeline],
        };
    }

    private static CombatReplayRecoveredSquadSummaryDto BuildRecoveredSquadSummary(IReadOnlyList<CombatReplayRecoveredEventDto> events)
    {
        var summary = new CombatReplayRecoveredSquadSummaryDto
        {
            RecoveredCount = events.Count,
            AverageRecoverTimeSeconds = events.Count > 0
                ? Math.Round(events.Average(evt => (evt.Time - evt.WindowStart) / 1000.0), 1)
                : 0.0,
            TotalDownedHealing = events.Sum(evt => evt.TotalDownedHealing),
            TotalHealingEvents = events.Sum(evt => evt.DownedHealingEventCount),
            TotalRezCasts = events.Sum(evt => evt.RezCastCount),
            TotalRezCastDurationSeconds = Math.Round(events.Sum(evt => evt.RezCastDurationSeconds), 1),
        };
        summary.SupportContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.SupportContributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetSupportDetailAmount(contributor, "Downed healing") + GetSupportDetailAmount(contributor, "Rez casts"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.HealingContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.SupportContributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetSupportDetailAmount(contributor, "Downed healing"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.RezContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.SupportContributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetSupportDetailAmount(contributor, "Rez casts"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.Takeaways = BuildRecoveredSquadSummaryTakeaways(summary);
        return summary;
    }

    private static CombatReplayRecoveredEnemySummaryDto BuildRecoveredEnemySummary(IReadOnlyList<CombatReplayRecoveredEventDto> events)
    {
        var summary = new CombatReplayRecoveredEnemySummaryDto
        {
            RecoveredCount = events.Count,
            ConditionImpactedRecoveries = events.Count(evt => evt.ConditionDamageTaken > 0),
            MysticRebukeRecoveries = events.Count(evt => evt.MysticRebukeDamageTaken > 0),
            BurningRecoveries = events.Count(evt => evt.ConditionDamageBreakdown.Any(entry => entry.BuffId == Burning && entry.Amount > 0)),
            ConditionMajorityRecoveries = events.Count(evt => evt.ConditionDamageTaken > evt.StrikeDamageTaken),
            MysticRebukeHeavyRecoveries = events.Count(evt => evt.StrikeDamageTaken > 0 && evt.MysticRebukeDamageTaken >= evt.StrikeDamageTaken * 0.10),
            TotalDamage = Math.Round(events.Sum(evt => (double)evt.TotalDamageTaken), 1),
            TotalStrikeDamage = Math.Round(events.Sum(evt => (double)evt.StrikeDamageTaken), 1),
            TotalConditionDamage = Math.Round(events.Sum(evt => (double)evt.ConditionDamageTaken), 1),
            TotalMysticRebukeDamage = Math.Round(events.Sum(evt => (double)evt.MysticRebukeDamageTaken), 1),
            TotalBarrierDamage = Math.Round(events.Sum(evt => (double)evt.BarrierDamageTaken), 1),
            AverageRecoverTimeSeconds = events.Count > 0
                ? Math.Round(events.Average(evt => (evt.Time - evt.WindowStart) / 1000.0), 1)
                : 0.0,
        };
        summary.TotalBurningDamage = Math.Round(events.Sum(evt =>
            evt.ConditionDamageBreakdown
                .Where(entry => entry.BuffId == Burning)
                .Sum(entry => (double)entry.Amount)), 1);
        summary.AverageMysticRebukeDamage = summary.MysticRebukeRecoveries > 0
            ? Math.Round(summary.TotalMysticRebukeDamage / summary.MysticRebukeRecoveries, 1)
            : 0.0;
        summary.PressureContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    contributor.Amount,
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.MysticRebukeContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Mystic Rebuke"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.ConditionContributors = BuildTopActorSummaries(events.SelectMany(evt =>
            evt.Contributors
                .Where(contributor => contributor.ActorId != null)
                .Select(contributor => (
                    contributor.ActorId,
                    contributor.Name,
                    contributor.Icon,
                    GetContributionAmount(contributor, "Condition"),
                    evt.Time))
                .Where(entry => entry.Item4 > 0.0)));
        summary.TopConditions = BuildTopSummaryEntries(events.SelectMany(evt =>
            evt.ConditionDamageBreakdown.Select(entry => (entry.Name, entry.Icon, entry.Amount, GetEventSummaryKey(evt)))));
        summary.Takeaways = BuildRecoveredEnemySummaryTakeaways(summary);
        return summary;
    }

    private static string GetDownOutcome(ParsedEvtcLog log, AgentItem agent, long downTime)
    {
        return GetDownOutcomeInfo(log, agent, downTime).Outcome;
    }

    private static DownOutcomeInfo GetDownOutcomeInfo(ParsedEvtcLog log, AgentItem agent, long downTime)
    {
        DeadEvent? nextDead = log.CombatData.GetDeadEvents(agent).FirstOrDefault(evt => evt.Time >= downTime);
        AliveEvent? nextAlive = log.CombatData.GetAliveEvents(agent).FirstOrDefault(evt => evt.Time >= downTime);
        if (nextDead != null && (nextAlive == null || nextDead.Time <= nextAlive.Time))
        {
            return new DownOutcomeInfo("Killed", nextDead.Time);
        }
        if (nextAlive != null)
        {
            return new DownOutcomeInfo("Recovered", nextAlive.Time);
        }
        return new DownOutcomeInfo("Unresolved", null);
    }

    private readonly record struct RecoverySupportSummary(
        int TotalDownedHealing,
        int DownedHealingEventCount,
        int RezCastCount,
        double RezCastDurationSeconds,
        int SupportContributorCount,
        IReadOnlyList<CombatReplayEventContributionDto> SupportContributors,
        IReadOnlyList<CombatReplayEventTimelineEntryDto> SupportTimeline);

    private static RecoverySupportSummary BuildRecoverySupportSummary(
        ParsedEvtcLog log,
        SingleActor recoveredPlayer,
        IReadOnlyList<SingleActor> squadPlayers,
        long downTime,
        long recoveredTime)
    {
        List<EXTHealingEvent> healingEvents = log.CombatData.HasEXTHealing
            ? [.. recoveredPlayer.EXTHealing.GetIncomingHealEvents(null, log, downTime, recoveredTime)
                .Where(healingEvent => healingEvent.AgainstDowned && healingEvent.HealingDone > 0)
                .OrderBy(healingEvent => healingEvent.Time)]
            : [];
        int totalDownedHealing = healingEvents.Sum(healingEvent => healingEvent.HealingDone);
        var healingByProvider = new Dictionary<AgentItem, double>();
        foreach (EXTHealingEvent healingEvent in healingEvents)
        {
            AgentItem provider = !healingEvent.CreditedFrom.IsUnknown
                ? healingEvent.CreditedFrom
                : healingEvent.From;
            healingByProvider[provider] = healingByProvider.TryGetValue(provider, out double existingHealing)
                ? existingHealing + healingEvent.HealingDone
                : healingEvent.HealingDone;
        }

        List<AnimatedCastEvent> rezCastEvents = [.. squadPlayers
            .Where(player => player.UniqueID != recoveredPlayer.UniqueID)
            .SelectMany(player => player.GetAnimatedCastEvents(log, downTime, recoveredTime)
                .Where(cast =>
                    (cast.SkillID == Resurrect || cast.SkillID == Resurrect2)
                    && cast.ActualDuration > 0
                    && cast.Time <= recoveredTime
                    && cast.EndTime >= recoveredTime - RecoveryRezAttributionWindow))
            .OrderBy(cast => cast.Time)];
        var rezCountByProvider = new Dictionary<AgentItem, double>();
        var rezDurationByProvider = new Dictionary<AgentItem, double>();
        foreach (AnimatedCastEvent rezCast in rezCastEvents)
        {
            rezCountByProvider[rezCast.Caster] = rezCountByProvider.TryGetValue(rezCast.Caster, out double existingRezCount)
                ? existingRezCount + 1
                : 1;
            rezDurationByProvider[rezCast.Caster] = rezDurationByProvider.TryGetValue(rezCast.Caster, out double existingRezDuration)
                ? existingRezDuration + rezCast.ActualDuration / 1000.0
                : rezCast.ActualDuration / 1000.0;
        }

        List<CombatReplayEventContributionDto> supportContributors = BuildRecoverySupportContributors(
            log,
            healingByProvider,
            rezCountByProvider,
            rezDurationByProvider);
        List<CombatReplayEventTimelineEntryDto> supportTimeline = BuildRecoverySupportTimeline(log, healingEvents, rezCastEvents);
        return new RecoverySupportSummary(
            TotalDownedHealing: totalDownedHealing,
            DownedHealingEventCount: healingEvents.Count,
            RezCastCount: rezCastEvents.Count,
            RezCastDurationSeconds: Math.Round(rezCastEvents.Sum(cast => cast.ActualDuration) / 1000.0, 1),
            SupportContributorCount: healingByProvider.Keys.Union(rezCountByProvider.Keys).Count(),
            SupportContributors: supportContributors,
            SupportTimeline: supportTimeline);
    }

    private static DamageWindowSummary BuildDamageWindowSummary(
        ParsedEvtcLog log,
        SingleActor actor,
        long windowStart,
        long windowEnd,
        long conditionSnapshotTime)
    {
        List<HealthDamageEvent> damageEvents = [.. actor.GetDamageTakenEvents(null, log, windowStart, windowEnd)
            .Where(damageEvent => damageEvent.HasHit && (damageEvent.HealthDamage > 0 || damageEvent.ShieldDamage > 0))
            .OrderBy(damageEvent => damageEvent.Time)];
        var contributorTotals = new Dictionary<AgentItem, double>();
        var contributorStrikeTotals = new Dictionary<AgentItem, double>();
        var contributorConditionTotals = new Dictionary<AgentItem, double>();
        var contributorMysticRebukeTotals = new Dictionary<AgentItem, double>();
        var conditionDamageBySkill = new Dictionary<long, CombatReplayEventSummaryEntryDto>();
        int strikeDamageTaken = 0;
        int mysticRebukeDamageTaken = 0;
        int conditionDamageTaken = 0;

        foreach (HealthDamageEvent damageEvent in damageEvents)
        {
            int totalDamage = damageEvent.HealthDamage + damageEvent.ShieldDamage;
            if (totalDamage <= 0)
            {
                continue;
            }

            AgentItem source = !damageEvent.CreditedFrom.IsUnknown
                ? damageEvent.CreditedFrom
                : damageEvent.From;

            if (damageEvent.ConditionDamageBased(log))
            {
                conditionDamageTaken += damageEvent.HealthDamage;
                contributorConditionTotals[source] = contributorConditionTotals.TryGetValue(source, out double existingConditionAmount)
                    ? existingConditionAmount + damageEvent.HealthDamage
                    : damageEvent.HealthDamage;
                long conditionKey = damageEvent.SkillID;
                if (!conditionDamageBySkill.TryGetValue(conditionKey, out CombatReplayEventSummaryEntryDto? conditionSummary))
                {
                    conditionSummary = new CombatReplayEventSummaryEntryDto
                    {
                        BuffId = conditionKey,
                        Name = NormalizeConditionDamageName(damageEvent.Skill.Name),
                        Icon = damageEvent.Skill.Icon,
                    };
                    conditionDamageBySkill[conditionKey] = conditionSummary;
                }
                conditionSummary.Amount = Math.Round(conditionSummary.Amount + damageEvent.HealthDamage, 1);
                conditionSummary.Count++;
            }
            else
            {
                strikeDamageTaken += damageEvent.HealthDamage;
                contributorStrikeTotals[source] = contributorStrikeTotals.TryGetValue(source, out double existingStrikeAmount)
                    ? existingStrikeAmount + damageEvent.HealthDamage
                    : damageEvent.HealthDamage;
                if (!string.IsNullOrWhiteSpace(damageEvent.Skill.Name)
                    && damageEvent.Skill.Name.IndexOf(MysticRebukeSkillName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mysticRebukeDamageTaken += totalDamage;
                    contributorMysticRebukeTotals[source] = contributorMysticRebukeTotals.TryGetValue(source, out double existingMysticRebukeAmount)
                        ? existingMysticRebukeAmount + totalDamage
                        : totalDamage;
                }
            }
            contributorTotals[source] = contributorTotals.TryGetValue(source, out double existingAmount)
                ? existingAmount + totalDamage
                : totalDamage;
        }

        int totalDamageTaken = damageEvents.Sum(damageEvent => damageEvent.HealthDamage + damageEvent.ShieldDamage);
        int barrierDamageTaken = damageEvents.Sum(damageEvent => damageEvent.ShieldDamage);
        List<CombatReplayEventContributionDto> conditions = BuildDownConditionList(log, actor, conditionSnapshotTime);
        List<CombatReplayEventContributionDto> conditionDamageBreakdown = [.. conditionDamageBySkill.Values
            .Where(entry => entry.Amount > 0.0)
            .OrderByDescending(entry => entry.Amount)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Select(entry => new CombatReplayEventContributionDto
            {
                BuffId = entry.BuffId,
                Name = entry.Name,
                Icon = entry.Icon,
                Amount = entry.Amount,
                Percent = conditionDamageTaken > 0
                    ? Math.Round(entry.Amount * 100.0 / conditionDamageTaken, 1)
                    : 0.0,
            })];
        List<CombatReplayEventContributionDto> contributors = BuildTopActorContributionList(
            log,
            contributorTotals,
            contributorStrikeTotals,
            contributorConditionTotals,
            contributorMysticRebukeTotals,
            totalDamageTaken);

        return new DamageWindowSummary(
            TotalDamageTaken: totalDamageTaken,
            StrikeDamageTaken: strikeDamageTaken,
            MysticRebukeDamageTaken: mysticRebukeDamageTaken,
            ConditionDamageTaken: conditionDamageTaken,
            BarrierDamageTaken: barrierDamageTaken,
            HitCount: damageEvents.Count,
            ContributorCount: contributorTotals.Count(pair => pair.Value > 0.0),
            Conditions: conditions,
            ConditionDamageBreakdown: conditionDamageBreakdown,
            Contributors: contributors,
            DamageTimeline: BuildDownDamageTimeline(log, damageEvents));
    }

    private static (List<CombatReplayEventTimelineEntryDto> Effects, int HardCcCount) BuildDownCrowdControlEffects(
        ParsedEvtcLog log,
        SingleActor actor,
        long start,
        long end,
        bool isEnemy)
    {
        var crowdControlEffects = new List<CombatReplayEventTimelineEntryDto>();
        int hardCcCount = 0;
        List<Segment> resistanceSegments = isEnemy
            ? []
            : [.. actor.GetBuffStatus(log, Resistance, start, end).Where(segment => segment.Value > 0)];

        List<CombatReplayEventTimelineEntryDto> hardCrowdControlEffects = [.. actor.GetIncomingCrowdControlEvents(null, log, start, end)
            .GroupBy(crowdControlEvent => crowdControlEvent.SkillID)
            .Select(group =>
            {
                CrowdControlEvent firstEvent = group.First();
                int count = group.Count();
                double totalDuration = Math.Round(group.Sum(crowdControlEvent => crowdControlEvent.Duration) / 1000.0, 1);
                return new CombatReplayEventTimelineEntryDto
                {
                    Time = firstEvent.Time,
                    TimeLabel = FormatTime(firstEvent.Time),
                    Label = firstEvent.Skill.Name,
                    Value = BuildPluralizedLabel(count, "hard CC event", "hard CC events"),
                    Secondary = $"{FormatOneDecimal(totalDuration)}s total control",
                    IsHardCc = true,
                };
            })];
        hardCcCount = hardCrowdControlEffects.Count;
        crowdControlEffects.AddRange(hardCrowdControlEffects);

        foreach (long buffId in ControlConditionBuffIds)
        {
            if (!log.Buffs.BuffsByIDs.TryGetValue(buffId, out Buff? buff))
            {
                continue;
            }

            IReadOnlyList<Segment> segments = actor.GetBuffStatus(log, buffId, start, end);
            List<Segment> activeSegments = [.. segments.Where(segment => segment.Value > 0)];
            if (activeSegments.Count == 0)
            {
                continue;
            }

            double maxStacks = Math.Round(activeSegments.Max(segment => segment.Value), 1);
            long activeMilliseconds = activeSegments
                .Sum(segment => Math.Max(0, Math.Min(segment.End, end) - Math.Max(segment.Start, start)));
            long resistedMilliseconds = resistanceSegments.Count == 0
                ? 0
                : SumSegmentOverlap(activeSegments, resistanceSegments, start, end);
            long effectiveMilliseconds = Math.Max(0, activeMilliseconds - resistedMilliseconds);
            if (!isEnemy && effectiveMilliseconds == 0)
            {
                continue;
            }

            double effectiveSeconds = Math.Round(effectiveMilliseconds / 1000.0, 1);
            double resistedSeconds = Math.Round(resistedMilliseconds / 1000.0, 1);
            crowdControlEffects.Add(new CombatReplayEventTimelineEntryDto
            {
                Time = activeSegments[0].Start,
                TimeLabel = FormatTime(activeSegments[0].Start),
                Label = buff.Name,
                Value = maxStacks > 1.0
                    ? $"Up to {FormatOneDecimal(maxStacks)} stacks"
                    : "Present in window",
                Secondary = isEnemy
                    ? $"{FormatOneDecimal(effectiveSeconds)}s active"
                    : resistedMilliseconds > 0
                        ? $"{FormatOneDecimal(effectiveSeconds)}s effective ({FormatOneDecimal(resistedSeconds)}s resisted)"
                        : $"{FormatOneDecimal(effectiveSeconds)}s effective",
                IsHardCc = false,
            });
        }

        crowdControlEffects.Sort((left, right) => left.Time.CompareTo(right.Time));
        return (crowdControlEffects, hardCcCount);
    }

    private static long SumSegmentOverlap(
        IReadOnlyList<Segment> primarySegments,
        IReadOnlyList<Segment> overlapSegments,
        long start,
        long end)
    {
        long overlap = 0;
        int overlapIndex = 0;
        foreach (Segment primarySegment in primarySegments)
        {
            long primaryStart = Math.Max(primarySegment.Start, start);
            long primaryEnd = Math.Min(primarySegment.End, end);
            if (primaryEnd <= primaryStart)
            {
                continue;
            }

            while (overlapIndex < overlapSegments.Count && overlapSegments[overlapIndex].End <= primaryStart)
            {
                overlapIndex++;
            }

            int currentOverlapIndex = overlapIndex;
            while (currentOverlapIndex < overlapSegments.Count)
            {
                Segment overlapSegment = overlapSegments[currentOverlapIndex];
                if (overlapSegment.Start >= primaryEnd)
                {
                    break;
                }

                long overlapStart = Math.Max(primaryStart, Math.Max(overlapSegment.Start, start));
                long overlapEnd = Math.Min(primaryEnd, Math.Min(overlapSegment.End, end));
                if (overlapEnd > overlapStart)
                {
                    overlap += overlapEnd - overlapStart;
                }
                currentOverlapIndex++;
            }
        }
        return overlap;
    }

    private static List<CombatReplayEventContributionDto> BuildRecoverySupportContributors(
        ParsedEvtcLog log,
        IReadOnlyDictionary<AgentItem, double> healingByProvider,
        IReadOnlyDictionary<AgentItem, double> rezCountByProvider,
        IReadOnlyDictionary<AgentItem, double> rezDurationByProvider,
        int maxActors = 6)
    {
        List<AgentItem> orderedAgents = [.. healingByProvider.Keys
            .Union(rezCountByProvider.Keys)
            .OrderByDescending(agent => healingByProvider.TryGetValue(agent, out double healing) ? healing : 0.0)
            .ThenByDescending(agent => rezCountByProvider.TryGetValue(agent, out double rezCount) ? rezCount : 0.0)
            .ThenByDescending(agent => rezDurationByProvider.TryGetValue(agent, out double rezDuration) ? rezDuration : 0.0)
            .ThenBy(agent => GetActorName(log, agent), StringComparer.OrdinalIgnoreCase)];
        if (orderedAgents.Count == 0)
        {
            return [];
        }

        List<AgentItem> visibleAgents = [.. orderedAgents.Take(maxActors)];
        var result = new List<CombatReplayEventContributionDto>(visibleAgents.Count + 1);
        foreach (AgentItem agent in visibleAgents)
        {
            SingleActor? actor = FindActor(log, agent);
            double healing = Math.Round(healingByProvider.TryGetValue(agent, out double healingAmount) ? healingAmount : 0.0, 1);
            double rezCount = Math.Round(rezCountByProvider.TryGetValue(agent, out double rezCountAmount) ? rezCountAmount : 0.0, 1);
            double rezDuration = Math.Round(rezDurationByProvider.TryGetValue(agent, out double rezDurationAmount) ? rezDurationAmount : 0.0, 1);
            result.Add(new CombatReplayEventContributionDto
            {
                ActorId = actor?.UniqueID,
                Name = actor?.Character ?? GetActorName(log, agent),
                Icon = actor?.GetIcon() ?? "",
                Amount = healing,
                Details =
                [
                    new CombatReplayEventContributionDto
                    {
                        Name = "Downed healing",
                        Amount = healing,
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Rez casts",
                        Amount = rezCount,
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Rez time",
                        Amount = rezDuration,
                    },
                ],
            });
        }

        if (orderedAgents.Count > visibleAgents.Count)
        {
            IEnumerable<AgentItem> otherAgents = orderedAgents.Skip(visibleAgents.Count);
            result.Add(new CombatReplayEventContributionDto
            {
                Name = "Other",
                Amount = Math.Round(otherAgents.Sum(agent => healingByProvider.TryGetValue(agent, out double healing) ? healing : 0.0), 1),
                Details =
                [
                    new CombatReplayEventContributionDto
                    {
                        Name = "Downed healing",
                        Amount = Math.Round(otherAgents.Sum(agent => healingByProvider.TryGetValue(agent, out double healing) ? healing : 0.0), 1),
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Rez casts",
                        Amount = Math.Round(otherAgents.Sum(agent => rezCountByProvider.TryGetValue(agent, out double rezCount) ? rezCount : 0.0), 1),
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Rez time",
                        Amount = Math.Round(otherAgents.Sum(agent => rezDurationByProvider.TryGetValue(agent, out double rezDuration) ? rezDuration : 0.0), 1),
                    },
                ],
            });
        }

        return result;
    }

    private static List<CombatReplayEventTimelineEntryDto> BuildRecoverySupportTimeline(
        ParsedEvtcLog log,
        IReadOnlyList<EXTHealingEvent> healingEvents,
        IReadOnlyList<AnimatedCastEvent> rezCastEvents)
    {
        var timeline = new List<CombatReplayEventTimelineEntryDto>(healingEvents.Count + rezCastEvents.Count);
        timeline.AddRange(healingEvents.Select(healingEvent =>
        {
            AgentItem provider = !healingEvent.CreditedFrom.IsUnknown
                ? healingEvent.CreditedFrom
                : healingEvent.From;
            return new CombatReplayEventTimelineEntryDto
            {
                Time = healingEvent.Time,
                TimeLabel = FormatTime(healingEvent.Time),
                Label = healingEvent.Skill.Name,
                Value = $"{FormatWholeNumber(healingEvent.HealingDone)} healing",
                Secondary = provider.IsUnknown ? "Downed healing" : $"{GetActorName(log, provider)} | Downed healing",
            };
        }));
        timeline.AddRange(rezCastEvents.Select(rezCast => new CombatReplayEventTimelineEntryDto
        {
            Time = rezCast.Time,
            TimeLabel = FormatTime(rezCast.Time),
            Label = rezCast.Skill.Name,
            Value = $"{FormatDuration(rezCast.ActualDuration)} rez cast",
            Secondary = GetActorName(log, rezCast.Caster),
        }));
        timeline.Sort((left, right) => left.Time.CompareTo(right.Time));
        return timeline;
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
        result.ConditionNecessaryEvents = events.Count(evt => evt.ConditionDamageNecessary);
        result.TotalBurningDamage = Math.Round(events.Sum(evt => evt.BurningDamage), 1);
        result.TotalConditionDamage = Math.Round(events.Sum(evt => evt.TotalConditionDamage), 1);
        result.TotalVulnerabilityBonusDamage = Math.Round(events.Sum(evt => evt.VulnerabilityBonusDamage), 1);
        result.TotalAttributedValue = Math.Round(events.Sum(evt => evt.TotalAttributedValue), 1);
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
        List<SingleActor> squadPlayers = [.. log.PlayerList.Where(player => !player.IsFakeActor).Cast<SingleActor>()];
        int healthAtWindowStart = target.GetCurrentHealth(log, windowStart);
        if (healthAtWindowStart <= 0)
        {
            healthAtWindowStart = GetApproximateCurrentHealth(target, log, windowStart);
        }
        int incomingHealing = log.CombatData.HasEXTHealing
            ? target.EXTHealing.GetIncomingHealEvents(null, log, windowStart, downEvent.Time)
                .Where(healingEvent => !healingEvent.AgainstDowned)
                .Sum(healingEvent => healingEvent.HealingDone)
            : 0;
        var buffBreakdowns = new List<CombatReplayEventContributionDto>(ConditionConversionDamageBuffIds.Count);
        var providerTotals = new Dictionary<AgentItem, double>();
        var providerConditionTotals = new Dictionary<AgentItem, Dictionary<long, double>>();
        var providerVulnerabilityTotals = new Dictionary<AgentItem, double>();
        int nonConditionDamage = 0;

        foreach (HealthDamageEvent damageEvent in target.GetHitDamageTakenEvents(null, log, windowStart, downEvent.Time, ParserHelper.DamageType.All))
        {
            int amount = damageEvent.HealthDamage;
            if (amount <= 0)
            {
                continue;
            }

            if (!damageEvent.ConditionDamageBased(log))
            {
                nonConditionDamage += amount;
                continue;
            }
            if (!ConditionConversionDamageBuffIds.Contains(damageEvent.SkillID))
            {
                continue;
            }

            AgentItem provider = damageEvent.CreditedFrom;
            providerTotals[provider] = providerTotals.TryGetValue(provider, out double existingProviderDamage)
                ? existingProviderDamage + amount
                : amount;
            if (!providerConditionTotals.TryGetValue(provider, out Dictionary<long, double>? conditionTotals))
            {
                conditionTotals = [];
                providerConditionTotals[provider] = conditionTotals;
            }
            conditionTotals[damageEvent.SkillID] = conditionTotals.TryGetValue(damageEvent.SkillID, out double existingConditionDamage)
                ? existingConditionDamage + amount
                : amount;
        }

        foreach (HealthDamageEvent damageEvent in target.GetHitDamageTakenEvents(null, log, windowStart, downEvent.Time, ParserHelper.DamageType.StrikeAndCondition))
        {
            if (damageEvent.HealthDamage <= 0 || damageEvent.IsLifeLeech)
            {
                continue;
            }

            double totalVulnerabilityStacks = Math.Max(0.0, target.GetBuffStatus(log, Vulnerability, damageEvent.Time).Value);
            if (totalVulnerabilityStacks <= 0.0)
            {
                continue;
            }

            double vulnerabilityBonus = damageEvent.HealthDamage * totalVulnerabilityStacks / (100.0 + totalVulnerabilityStacks);
            foreach (SingleActor squadPlayer in squadPlayers)
            {
                double providerVulnerabilityStacks = Math.Max(0.0, target.GetBuffStatus(log, squadPlayer, Vulnerability, damageEvent.Time).Value);
                if (providerVulnerabilityStacks <= 0.0)
                {
                    continue;
                }

                double providerBonus = vulnerabilityBonus * providerVulnerabilityStacks / totalVulnerabilityStacks;
                if (providerBonus <= 0.0)
                {
                    continue;
                }

                AgentItem provider = squadPlayer.AgentItem;
                providerTotals[provider] = providerTotals.TryGetValue(provider, out double existingProviderValue)
                    ? existingProviderValue + providerBonus
                    : providerBonus;
                providerVulnerabilityTotals[provider] = providerVulnerabilityTotals.TryGetValue(provider, out double existingVulnerabilityBonus)
                    ? existingVulnerabilityBonus + providerBonus
                    : providerBonus;
            }
        }

        foreach (long buffId in ConditionConversionDamageBuffIds)
        {
            if (!log.Buffs.BuffsByIDs.TryGetValue(buffId, out Buff? buff))
            {
                continue;
            }

            double totalAmount = Math.Round(providerConditionTotals.Values
                .Where(conditionTotals => conditionTotals.TryGetValue(buffId, out double amount) && amount > 0.0)
                .Sum(conditionTotals => conditionTotals[buffId]), 1);
            if (totalAmount <= 0.0)
            {
                continue;
            }

            buffBreakdowns.Add(new CombatReplayEventContributionDto
            {
                BuffId = buffId,
                Name = buff.Name,
                Icon = buff.Link,
                Amount = totalAmount,
            });
        }

        double totalConditionDamage = Math.Round(buffBreakdowns.Sum(entry => entry.Amount), 1);
        if (totalConditionDamage <= 0.0)
        {
            return null;
        }

        bool conditionDamageNecessary = healthAtWindowStart > 0 && nonConditionDamage < healthAtWindowStart + incomingHealing;
        double vulnerabilityBonusDamage = Math.Round(providerVulnerabilityTotals.Values.Sum(), 1);
        double totalAttributedValue = Math.Round(totalConditionDamage + vulnerabilityBonusDamage, 1);
        foreach (CombatReplayEventContributionDto entry in buffBreakdowns)
        {
            entry.Percent = Math.Round(entry.Amount * 100.0 / totalConditionDamage, 1);
        }

        List<CombatReplayEventContributionDto> providers = BuildMeaningfulConditionProviders(
            log,
            providerTotals,
            providerConditionTotals,
            providerVulnerabilityTotals,
            totalAttributedValue);
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
            ConditionDamageNecessary = conditionDamageNecessary,
            TotalConditionDamage = totalConditionDamage,
            BurningDamage = Math.Round(burningBreakdown?.Amount ?? 0.0, 1),
            VulnerabilityBonusDamage = vulnerabilityBonusDamage,
            TotalAttributedValue = totalAttributedValue,
            TopConditionName = topCondition.Name,
            TopConditionIcon = topCondition.Icon,
            TopContributorSummary = BuildCompactContributorSummary(providers),
            Conditions = buffBreakdowns,
            Providers = providers,
        };
    }

    private static List<CombatReplayEventContributionDto> BuildMeaningfulConditionProviders(
        ParsedEvtcLog log,
        IReadOnlyDictionary<AgentItem, double> providerTotals,
        IReadOnlyDictionary<AgentItem, Dictionary<long, double>> providerConditionTotals,
        IReadOnlyDictionary<AgentItem, double> providerVulnerabilityTotals,
        double totalAttributedValue)
    {
        List<CombatReplayEventContributionDto> providers = BuildMeaningfulActorContributionList(log, providerTotals, totalAttributedValue);
        Dictionary<int, AgentItem> agentsById = providerTotals.Keys
            .Select(agent => (Agent: agent, Actor: FindActor(log, agent)))
            .Where(entry => entry.Actor != null)
            .ToDictionary(entry => entry.Actor!.UniqueID, entry => entry.Agent);
        foreach (CombatReplayEventContributionDto provider in providers)
        {
            AgentItem? providerAgent = null;
            if (provider.ActorId != null && agentsById.TryGetValue(provider.ActorId.Value, out AgentItem? agent))
            {
                providerAgent = agent;
            }
            else if (!string.Equals(provider.Name, "Other", StringComparison.OrdinalIgnoreCase))
            {
                providerAgent = providerTotals.Keys.FirstOrDefault(agentItem => string.Equals(GetActorName(log, agentItem), provider.Name, StringComparison.OrdinalIgnoreCase));
            }
            if (providerAgent == null)
            {
                continue;
            }

            var details = new List<CombatReplayEventContributionDto>();
            if (providerConditionTotals.TryGetValue(providerAgent, out Dictionary<long, double>? conditionTotals))
            {
                details.AddRange(ConditionConversionDamageBuffIds
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
                    }));
            }

            if (providerVulnerabilityTotals.TryGetValue(providerAgent, out double vulnerabilityAmount) && vulnerabilityAmount > 0.0)
            {
                string vulnerabilityIcon = log.Buffs.BuffsByIDs.TryGetValue(Vulnerability, out Buff? vulnerabilityBuff) ? vulnerabilityBuff.Link : "";
                details.Add(new CombatReplayEventContributionDto
                {
                    BuffId = Vulnerability,
                    Name = "Vulnerability bonus",
                    Icon = vulnerabilityIcon,
                    Amount = Math.Round(vulnerabilityAmount, 1),
                    Percent = provider.Amount > 0.0 ? Math.Round(vulnerabilityAmount * 100.0 / provider.Amount, 1) : 0.0,
                });
            }

            provider.Details = [.. details];
        }
        return providers;
    }

    private static List<CombatReplayEventContributionDto> BuildDownConditionList(
        ParsedEvtcLog log,
        SingleActor actor,
        long time)
    {
        var conditions = new List<CombatReplayEventContributionDto>();
        foreach (long buffId in DownContextConditionBuffIds)
        {
            if (!log.Buffs.BuffsByIDs.TryGetValue(buffId, out Buff? buff))
            {
                continue;
            }

            int stacks = GetBuffStacksAtTime(actor, log, buffId, time);
            if (stacks <= 0)
            {
                continue;
            }

            conditions.Add(new CombatReplayEventContributionDto
            {
                BuffId = buffId,
                Name = buff.Name,
                Icon = buff.Link,
                Amount = stacks,
            });
        }

        return [.. conditions
            .OrderByDescending(condition => condition.Amount)
            .ThenBy(condition => condition.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static List<CombatReplayEventContributionDto> BuildTopActorContributionList(
        ParsedEvtcLog log,
        IReadOnlyDictionary<AgentItem, double> contributorTotals,
        IReadOnlyDictionary<AgentItem, double> contributorStrikeTotals,
        IReadOnlyDictionary<AgentItem, double> contributorConditionTotals,
        IReadOnlyDictionary<AgentItem, double> contributorMysticRebukeTotals,
        double totalAmount,
        int maxActors = 6)
    {
        if (totalAmount <= 0.0 || contributorTotals.Count == 0)
        {
            return [];
        }

        List<(AgentItem Agent, double Amount)> orderedContributors = [.. contributorTotals
            .Where(pair => pair.Value > 0.0)
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetActorName(log, pair.Key), StringComparer.OrdinalIgnoreCase)
            .Select(pair => (pair.Key, pair.Value))];
        if (orderedContributors.Count == 0)
        {
            return [];
        }

        List<(AgentItem Agent, double Amount)> visibleContributors = [.. orderedContributors.Take(maxActors)];
        var result = new List<CombatReplayEventContributionDto>(visibleContributors.Count + 1);
        foreach ((AgentItem agent, double amount) in visibleContributors)
        {
            SingleActor? actor = FindActor(log, agent);
            double strikeAmount = Math.Round(contributorStrikeTotals.TryGetValue(agent, out double strikeTotal) ? strikeTotal : 0.0, 1);
            double conditionAmount = Math.Round(contributorConditionTotals.TryGetValue(agent, out double conditionTotal) ? conditionTotal : 0.0, 1);
            double mysticRebukeAmount = Math.Round(contributorMysticRebukeTotals.TryGetValue(agent, out double mysticRebukeTotal) ? mysticRebukeTotal : 0.0, 1);
            result.Add(new CombatReplayEventContributionDto
            {
                ActorId = actor?.UniqueID,
                Name = actor?.Character ?? GetActorName(log, agent),
                Icon = actor?.GetIcon() ?? "",
                Amount = Math.Round(amount, 1),
                Percent = Math.Round(amount * 100.0 / totalAmount, 1),
                Details =
                [
                    new CombatReplayEventContributionDto
                    {
                        Name = "Strike",
                        Amount = strikeAmount,
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Condition",
                        Amount = conditionAmount,
                    },
                    new CombatReplayEventContributionDto
                    {
                        Name = "Mystic Rebuke",
                        Amount = mysticRebukeAmount,
                    },
                ],
            });
        }

        double remainingAmount = Math.Round(totalAmount - visibleContributors.Sum(pair => pair.Amount), 1);
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

    private static List<CombatReplayEventTimelineEntryDto> BuildDownDamageTimeline(
        ParsedEvtcLog log,
        IReadOnlyList<HealthDamageEvent> damageEvents)
    {
        return [.. damageEvents.Select(damageEvent =>
        {
            string value = damageEvent.ShieldDamage > 0
                ? $"{FormatWholeNumber(damageEvent.HealthDamage)} health, {FormatWholeNumber(damageEvent.ShieldDamage)} barrier"
                : $"{FormatWholeNumber(damageEvent.HealthDamage)} health";
            AgentItem sourceAgent = !damageEvent.CreditedFrom.IsUnknown
                ? damageEvent.CreditedFrom
                : damageEvent.From;
            string source = sourceAgent.IsUnknown ? "" : GetActorName(log, sourceAgent);
            string damageType = damageEvent.ConditionDamageBased(log) ? "Condition" : "Strike";
            string secondary = string.IsNullOrWhiteSpace(source)
                ? damageType
                : $"{source} | {damageType}";
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

    private static double GetContributionAmount(CombatReplayEventContributionDto contribution, string detailName)
    {
        return Math.Round((double)(contribution.Details.FirstOrDefault(detail =>
            string.Equals(detail.Name, detailName, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0.0), 1);
    }

    private static double GetSupportDetailAmount(CombatReplayEventContributionDto contribution, string detailName)
    {
        return Math.Round((double)(contribution.Details.FirstOrDefault(detail =>
            string.Equals(detail.Name, detailName, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0.0), 1);
    }

    private static string GetEventSummaryKey(CombatReplayDownEventDto evt)
    {
        return $"{evt.Time}-{evt.ActorId}-{evt.Side}";
    }

    private static string NormalizeConditionDamageName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "";
        }

        int separatorIndex = name.IndexOf('-');
        if (separatorIndex > 0 && name.Take(separatorIndex).All(char.IsDigit))
        {
            return name[(separatorIndex + 1)..].Trim();
        }
        return name.Trim();
    }

    private static double ParseEffectiveCcSeconds(CombatReplayEventTimelineEntryDto effect)
    {
        if (string.IsNullOrWhiteSpace(effect.Secondary))
        {
            return 0.0;
        }

        Match match = Regex.Match(effect.Secondary, @"(?<seconds>\d+(?:\.\d+)?)s\s+(effective|active)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return 0.0;
        }
        if (!double.TryParse(match.Groups["seconds"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
        {
            return 0.0;
        }
        return Math.Round(seconds, 1);
    }

    private static List<CombatReplayEventSummaryEntryDto> BuildTopSummaryEntries(
        IEnumerable<(string Name, string Icon, double Amount, string EventKey)> entries,
        int maxEntries = 5)
    {
        return [.. entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .GroupBy(entry => entry.Name)
            .Select(group => new CombatReplayEventSummaryEntryDto
            {
                Name = group.Key,
                Icon = group.First().Icon,
                Count = group.Select(entry => entry.EventKey).Distinct(StringComparer.Ordinal).Count(),
                Amount = Math.Round(group.Sum(entry => entry.Amount), 1),
            })
            .OrderByDescending(entry => entry.Amount)
            .ThenByDescending(entry => entry.Count)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Take(maxEntries)];
    }

    private static List<string> BuildDownSummaryTakeaways(CombatReplayDownSummaryDto summary)
    {
        var takeaways = new List<string>();
        int totalDowns = summary.SquadDowns + summary.EnemyDowns;
        if (totalDowns == 0)
        {
            return takeaways;
        }

        if (summary.MysticRebukeDowns > 0)
        {
            takeaways.Add($"Mystic Rebuke showed up in {summary.MysticRebukeDowns} of {totalDowns} downs ({FormatWholeNumber((long)Math.Round(summary.MysticRebukeDowns * 100.0 / totalDowns))}%) for {FormatWholeNumber((long)Math.Round(summary.TotalMysticRebukeDamage))} total damage.");
        }

        if (summary.ConditionImpactedDowns > 0)
        {
            CombatReplayEventSummaryEntryDto? topCondition = summary.TopConditions.FirstOrDefault();
            string topConditionText = topCondition != null
                ? $" Top damaging condition: {topCondition.Name} ({FormatWholeNumber((long)Math.Round(topCondition.Amount))})."
                : "";
            takeaways.Add($"Condition damage appeared in {summary.ConditionImpactedDowns} downs, with Burning contributing {FormatWholeNumber((long)Math.Round(summary.TotalBurningDamage))} across the fight.{topConditionText}");
        }

        if (summary.CcImpactedDowns > 0)
        {
            takeaways.Add($"CC affected {summary.CcImpactedDowns} downs, with hard CC on {summary.HardCcDowns} and soft CC on {summary.SoftCcDowns}.");
        }

        if (summary.ConditionMajorityDowns > 0)
        {
            takeaways.Add($"Conditions outweighed strike damage in {summary.ConditionMajorityDowns} down windows.");
        }

        return takeaways.Take(4).ToList();
    }

    private static List<string> BuildKillSummaryTakeaways(CombatReplayKillSummaryDto summary)
    {
        var takeaways = new List<string>();
        int totalKills = summary.SquadKills + summary.EnemyKills;
        if (totalKills == 0)
        {
            return takeaways;
        }

        takeaways.Add($"Average down-to-kill time was {FormatOneDecimal(summary.AverageKillTimeSeconds)}s across {totalKills} kill windows.");

        if (summary.MysticRebukeKills > 0)
        {
            takeaways.Add($"Mystic Rebuke appeared in {summary.MysticRebukeKills} kill windows for {FormatWholeNumber((long)Math.Round(summary.TotalMysticRebukeDamage))} total damage.");
        }

        if (summary.ConditionImpactedKills > 0)
        {
            CombatReplayEventSummaryEntryDto? topCondition = summary.TopConditions.FirstOrDefault();
            string topConditionText = topCondition != null
                ? $" Top kill condition: {topCondition.Name} ({FormatWholeNumber((long)Math.Round(topCondition.Amount))})."
                : "";
            takeaways.Add($"Condition damage appeared in {summary.ConditionImpactedKills} kill windows, with Burning contributing {FormatWholeNumber((long)Math.Round(summary.TotalBurningDamage))}.{topConditionText}");
        }

        if (summary.ConditionMajorityKills > 0)
        {
            takeaways.Add($"Conditions outweighed strike damage in {summary.ConditionMajorityKills} kill windows.");
        }

        return takeaways.Take(4).ToList();
    }

    private static List<string> BuildRecoveredSquadSummaryTakeaways(CombatReplayRecoveredSquadSummaryDto summary)
    {
        var takeaways = new List<string>();
        if (summary.RecoveredCount == 0)
        {
            return takeaways;
        }

        takeaways.Add($"Average squad down-to-recover time was {FormatOneDecimal(summary.AverageRecoverTimeSeconds)}s across {summary.RecoveredCount} recoveries.");
        if (summary.TotalDownedHealing > 0)
        {
            takeaways.Add($"Squad recoveries received {FormatWholeNumber(summary.TotalDownedHealing)} downed healing across {summary.TotalHealingEvents} healing events.");
        }
        if (summary.TotalRezCasts > 0)
        {
            takeaways.Add($"{summary.TotalRezCasts} rez casts contributed {FormatOneDecimal(summary.TotalRezCastDurationSeconds)}s of total rez time.");
        }
        return takeaways.Take(4).ToList();
    }

    private static List<string> BuildRecoveredEnemySummaryTakeaways(CombatReplayRecoveredEnemySummaryDto summary)
    {
        var takeaways = new List<string>();
        if (summary.RecoveredCount == 0)
        {
            return takeaways;
        }

        takeaways.Add($"Enemy downs that recovered lasted {FormatOneDecimal(summary.AverageRecoverTimeSeconds)}s on average.");
        if (summary.MysticRebukeRecoveries > 0)
        {
            takeaways.Add($"Mystic Rebuke appeared in {summary.MysticRebukeRecoveries} enemy recoveries for {FormatWholeNumber((long)Math.Round(summary.TotalMysticRebukeDamage))} total damage.");
        }
        if (summary.ConditionImpactedRecoveries > 0)
        {
            CombatReplayEventSummaryEntryDto? topCondition = summary.TopConditions.FirstOrDefault();
            string topConditionText = topCondition != null
                ? $" Top pressure condition: {topCondition.Name} ({FormatWholeNumber((long)Math.Round(topCondition.Amount))})."
                : "";
            takeaways.Add($"Condition damage appeared in {summary.ConditionImpactedRecoveries} enemy recoveries, with Burning contributing {FormatWholeNumber((long)Math.Round(summary.TotalBurningDamage))}.{topConditionText}");
        }
        return takeaways.Take(4).ToList();
    }

    private static List<string> BuildDefenseBurstBarrierTakeaways(CombatReplayDefenseBurstBarrierDto summary)
    {
        var takeaways = new List<string>();
        if (summary.EnemyBurstWindows == 0)
        {
            return takeaways;
        }

        takeaways.Add($"Barrier absorbed damage in {summary.BurstWindowsWithBarrierAbsorbed} of {summary.EnemyBurstWindows} tracked enemy burst windows, for {FormatOneDecimal(summary.BurstBarrierAbsorptionPercent)}% burst absorption overall.");

        if (summary.BurstWindowsHeld > 0 || summary.BurstWindowsWithSquadDown > 0)
        {
            takeaways.Add($"{summary.BurstWindowsHeld} burst windows were held without a squad down, while {summary.BurstWindowsWithSquadDown} produced at least one squad down.");
        }

        if (summary.BurstWindowsHeld > 0 && summary.BurstWindowsWithSquadDown > 0)
        {
            takeaways.Add($"Held bursts averaged {FormatOneDecimal(summary.HeldBurstBarrierShare)}% barrier absorption versus {FormatOneDecimal(summary.DownedBurstBarrierShare)}% in bursts that still caused downs.");
        }

        if (summary.LowHealthSurvivorOccurrences > 0)
        {
            takeaways.Add($"{summary.LowHealthSurvivorOccurrences} burst-window survival moments fell below that burst's own barrier absorption percentage without becoming downs.");
        }

        return takeaways.Take(4).ToList();
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

    private static (long Time, double Percent) GetLowestHealthPoint(
        SingleActor actor,
        ParsedEvtcLog log,
        long start,
        long end,
        double fallbackPercent)
    {
        double minimumPercent = fallbackPercent;
        long minimumTime = start;
        foreach (Segment healthSegment in actor.GetHealthUpdates(log))
        {
            if (healthSegment.Start < start || healthSegment.Start > end)
            {
                continue;
            }
            if (healthSegment.Value < minimumPercent)
            {
                minimumPercent = healthSegment.Value;
                minimumTime = healthSegment.Start;
            }
        }
        return (minimumTime, Math.Round(minimumPercent, 1));
    }

    private static int GetApproximateCurrentHealth(SingleActor actor, ParsedEvtcLog log, long time)
    {
        int currentHealth = actor.GetCurrentHealth(log, time);
        return Math.Max(currentHealth, 0);
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

    private static string FormatDuration(long duration)
    {
        return $"{Math.Max(0, duration) / 1000.0:0.000}s";
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
