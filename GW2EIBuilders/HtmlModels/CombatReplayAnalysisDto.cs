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
    public CombatReplayFightDemandDto FightDemand { get; set; } = new();
    public CombatReplayDamageOverlayDto DamageOverlay { get; set; } = new();
    public Dictionary<int, CombatReplayPlayerEvaluationDto> PlayerEvaluations { get; set; } = [];
    public List<CombatReplaySpecCapabilityDto> SpecCapabilities { get; set; } = [];
}

internal class CombatReplayDamageOverlayDto
{
    public int Lookback { get; set; }
    public int FullHeatDamage { get; set; }
    public List<CombatReplayDamageOverlayEntryDto> Entries { get; set; } = [];
}

internal class CombatReplayDamageOverlayEntryDto
{
    public int UniqueId { get; set; }
    public string TargetSide { get; set; } = "";
    public long[] DamageTaken { get; set; } = [];
    public Dictionary<int, long[][]> TopContributors { get; set; } = [];
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
    public long TotalPetMinionDamageAbsorbed { get; set; }
    public double PetMinionAbsorptionPercent { get; set; }
    public CombatReplayDefenseBurstBarrierDto BurstBarrier { get; set; } = new();
    public CombatReplayDefenseMitigationDto Mitigation { get; set; } = new();
    public CombatReplayDefenseSavedPlayersSummaryDto SavedPlayersSummary { get; set; } = new();
    public List<CombatReplayEventActorSummaryDto> TopBarrierProviders { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> TopPetMinionAbsorbers { get; set; } = [];
    public List<CombatReplayDefenseNegatedHitSummaryDto> NegatedHitSummaries { get; set; } = [];
}

internal class CombatReplayDefenseNegatedHitSummaryDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public int NegatedHitCount { get; set; }
    public double EstimatedPreventedDamage { get; set; }
    public int FallbackEstimateCount { get; set; }
    public List<CombatReplayEffectCountSummaryDto> ContributingEffects { get; set; } = [];
    public List<CombatReplayDefenseNegatedHitOccurrenceDto> Occurrences { get; set; } = [];
}

internal class CombatReplayEffectCountSummaryDto
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

internal class CombatReplayDefenseNegatedHitOccurrenceDto
{
    public long Time { get; set; }
    public string TimeLabel { get; set; } = "";
    public int ActorId { get; set; }
    public string PlayerName { get; set; } = "";
    public string EffectName { get; set; } = "";
    public string SkillName { get; set; } = "";
    public double EstimatedPreventedDamage { get; set; }
    public bool UsedFallbackEstimate { get; set; }
}

internal class CombatReplayDefenseSavedPlayersSummaryDto
{
    public int SavedCases { get; set; }
    public double TotalBarrierAbsorbed { get; set; }
    public int BarrierSavedCases { get; set; }
    public double TotalEstimatedDamageReduction { get; set; }
    public int DamageReductionSavedCases { get; set; }
    public double TotalEstimatedNegatedDamage { get; set; }
    public int NegatedDamageSavedCases { get; set; }
    public double AverageLowestHealthPercent { get; set; }
    public double LowestLowestHealthPercent { get; set; }
    public int BothSavedCases { get; set; }
    public int MultiSourceSavedCases { get; set; }
    public double TotalIncomingDamage { get; set; }
    public double TotalIncomingHealing { get; set; }
    public List<CombatReplayEventActorSummaryDto> TopDamageReductionEffects { get; set; } = [];
    public List<CombatReplayEventActorSummaryDto> TopNegatedDamageEffects { get; set; } = [];
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
    public int HealthPoolUsed { get; set; }
    public bool HealthPoolEstimated { get; set; }
    public double LowestHealthPercent { get; set; }
    public int LowestHealthEstimate { get; set; }
    public double BarrierAbsorbedToLowest { get; set; }
    public bool BarrierSavedPlayer { get; set; }
    public double EstimatedMitigationToLowest { get; set; }
    public double EstimatedMitigation { get; set; }
    public bool EstimatedMitigationSavedPlayer { get; set; }
    public List<string> EstimatedMitigationSavedEffects { get; set; } = [];
    public double EstimatedNegatedDamageToLowest { get; set; }
    public double EstimatedNegatedDamage { get; set; }
    public bool EstimatedNegatedDamageSavedPlayer { get; set; }
    public List<string> EstimatedNegatedDamageSavedEffects { get; set; } = [];
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

internal sealed class TrackedMitigationReduction
{
    public string Name { get; init; } = "";
    public long[] BuffIds { get; init; } = [];
    public double StrikeReduction { get; init; }
}

internal enum NegatedHitTrigger
{
    Blocked,
    Absorbed,
}

internal sealed class TrackedNegatedMitigationEffect
{
    public string Name { get; init; } = "";
    public long[] BuffIds { get; init; } = [];
    public NegatedHitTrigger Trigger { get; init; }
    public string SummaryKey { get; init; } = "";
    public string SummaryLabel { get; init; } = "";
}

internal static class CombatReplayMitigationDefinitions
{
    public static readonly IReadOnlyList<TrackedMitigationBuff> TrackedEffects =
    [
        new() { Name = "Protection", BuffIds = [Protection, ProtectionUnstrippable] },
        new() { Name = "Resolution", BuffIds = [Resolution, ResolutionUnstrippable] },
        new() { Name = "Aegis", BuffIds = [Aegis] },
        new() { Name = "Frost Aura", BuffIds = [FrostAura] },
        new() { Name = "Light Aura", BuffIds = [LightAura] },
        new() { Name = "Dark Aura", BuffIds = [DarkAura] },
        new() { Name = "Distortion", BuffIds = [DistortionBuff] },
        new() { Name = "Blur", BuffIds = [Blur] },
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

    public static readonly IReadOnlyList<TrackedNegatedMitigationEffect> NegatedEffects =
    [
        new() { Name = "Aegis", BuffIds = [Aegis], Trigger = NegatedHitTrigger.Blocked, SummaryKey = "aegis", SummaryLabel = "Aegis Blocks" },
        new() { Name = "Distortion", BuffIds = [DistortionBuff], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "distortion", SummaryLabel = "Distortion Negations" },
        new() { Name = "Blur", BuffIds = [Blur], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "blur", SummaryLabel = "Blur Negations" },
        new() { Name = "Determined", BuffIds = [Determined762, Determined785, Determined788, Determined895, Determined3892, Determined31450, Determined52271], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
        new() { Name = "Invulnerability", BuffIds = [Invulnerability757, Invulnerability56227, Invulnerability801], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
        new() { Name = "Spawn Protection", BuffIds = [SpawnProtection], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
        new() { Name = "Obsidian Flesh", BuffIds = [ObsidianFlesh], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
        new() { Name = "Renewed Focus", BuffIds = [RenewedFocus], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
        new() { Name = "Defy Pain", BuffIds = [DefyPainSoulbeastBuff], Trigger = NegatedHitTrigger.Absorbed, SummaryKey = "invulnerability", SummaryLabel = "Invulnerability / Absorb" },
    ];

    public static readonly IReadOnlyList<TrackedMitigationReduction> StrikeReductions =
    [
        new() { Name = "Protection", BuffIds = [Protection, ProtectionUnstrippable], StrikeReduction = 0.33 },
        new() { Name = "Frost Aura", BuffIds = [FrostAura], StrikeReduction = 0.10 },
        new() { Name = "Guard!", BuffIds = [GuardBuff], StrikeReduction = 0.33 },
    ];
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
    public int OffensiveProtocolObliterateHitCount { get; set; }
    public int OffensiveProtocolObliterateBarrierRemovedHitCount { get; set; }
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
    public int OffensiveProtocolObliterateHitCount { get; set; }
    public int OffensiveProtocolObliterateBarrierRemovedHitCount { get; set; }
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
    public string FitSummary { get; set; } = "";
    public string DemandFitSummary { get; set; } = "";
    public CombatReplayContributionConfidenceDto Confidence { get; set; } = new();
    public CombatReplayPlayerFightImpactDto FightImpact { get; set; } = new();
    public List<CombatReplayPlayerContributionLaneDto> Lanes { get; set; } = [];
    public List<CombatReplayPlayerEvaluationModifierDto> Modifiers { get; set; } = [];
    public List<string> EvidenceSnapshot { get; set; } = [];
    public string ContributionProfile { get; set; } = "";
    public string KeyContributionSummary { get; set; } = "";
    public List<CombatReplayPlayerRoleMixEntryDto> RoleMix { get; set; } = [];
    public List<CombatReplayPlayerEvaluationAreaDto> Areas { get; set; } = [];
}

internal class CombatReplayPlayerFightImpactDto
{
    public double Score { get; set; }
    public string Label { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public string ConfidenceLabel { get; set; } = "";
    public List<string> Caveats { get; set; } = [];
    public List<CombatReplayPlayerFightImpactLaneDto> Lanes { get; set; } = [];
}

internal class CombatReplayPlayerFightImpactLaneDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public double DemandScorePercent { get; set; }
    public string DemandLabel { get; set; } = "";
    public double DemandWeightPercent { get; set; }
    public double ImpactScore { get; set; }
    public string EvidenceLine { get; set; } = "";
}

internal class CombatReplaySpecCapabilityDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public List<int> PlayerIds { get; set; } = [];
    public int PlayerCount { get; set; }
    public double ActiveSharePercent { get; set; }
    public string FitSummary { get; set; } = "";
    public string DemandFitSummary { get; set; } = "";
    public string DependencySummary { get; set; } = "";
    public CombatReplaySpecFightCoverageDto FightCoverage { get; set; } = new();
    public List<CombatReplaySpecCapabilityLaneDto> Lanes { get; set; } = [];
    public List<string> EvidenceSnapshot { get; set; } = [];
}

internal class CombatReplaySpecFightCoverageDto
{
    public double Score { get; set; }
    public string Label { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Detail { get; set; } = "";
    public List<string> Caveats { get; set; } = [];
    public List<CombatReplaySpecFightCoverageLaneDto> Lanes { get; set; } = [];
}

internal class CombatReplaySpecFightCoverageLaneDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public double PerSlotEfficiency { get; set; }
    public int PlayersContributing { get; set; }
    public int PlayerCount { get; set; }
    public double DemandScorePercent { get; set; }
    public string DemandLabel { get; set; } = "";
    public double DemandWeightPercent { get; set; }
    public double CoverageScore { get; set; }
    public string EvidenceLine { get; set; } = "";
}

internal class CombatReplaySpecCapabilityLaneDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public double PerSlotEfficiency { get; set; }
    public int PlayersContributing { get; set; }
    public int PlayerCount { get; set; }
    public double TopContributorSharePercent { get; set; }
    public string RateBand { get; set; } = "";
    public string DependencyLabel { get; set; } = "";
    public string EvidenceLine { get; set; } = "";
    public bool IsInteractive { get; set; }
    public string DrilldownTitle { get; set; } = "";
    public string DrilldownSubtitle { get; set; } = "";
    public List<CombatReplayPlayerEvaluationDetailSectionDto> DetailSections { get; set; } = [];
}

internal class CombatReplayFightDemandDto
{
    public string Summary { get; set; } = "";
    public List<CombatReplayFightDemandLaneDto> Lanes { get; set; } = [];
}

internal class CombatReplayFightDemandLaneDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double DemandScorePercent { get; set; }
    public string DemandLabel { get; set; } = "";
    public double ResponseScorePercent { get; set; }
    public string ResponseLabel { get; set; } = "";
    public string ResponseTone { get; set; } = "";
    public string ResponseLine { get; set; } = "";
    public double WeightMultiplier { get; set; }
    public string EvidenceLine { get; set; } = "";
}

internal class CombatReplayContributionConfidenceDto
{
    public string Label { get; set; } = "";
    public string Detail { get; set; } = "";
    public List<string> Caveats { get; set; } = [];
}

internal class CombatReplayPlayerContributionLaneDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double StrengthPercent { get; set; }
    public double SharePercent { get; set; }
    public int WindowsHit { get; set; }
    public int WindowsTotal { get; set; }
    public string WindowLabel { get; set; } = "";
    public string RateBand { get; set; } = "";
    public string EvidenceLine { get; set; } = "";
    public bool IsInteractive { get; set; }
    public string DrilldownTitle { get; set; } = "";
    public string DrilldownSubtitle { get; set; } = "";
    public List<CombatReplayPlayerEvaluationDetailSectionDto> DetailSections { get; set; } = [];
    public List<CombatReplayPlayerLaneMetricDto> Metrics { get; set; } = [];
}

internal class CombatReplayPlayerLaneMetricDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public string Aggregation { get; set; } = "";
}

internal class CombatReplayPlayerEvaluationModifierDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Detail { get; set; } = "";
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
    public long LiveTargetDamage { get; set; }
    public int AgainstDownedDamage { get; set; }
    public int DownContribution { get; set; }
    public double EnemyDownContributionDamage { get; set; }
    public int EnemyDownWindowsHit { get; set; }
    public int EnemyDownWindowsTotal { get; set; }
    public double EnemyKillContributionDamage { get; set; }
    public int EnemyKillWindowsHit { get; set; }
    public int EnemyKillWindowsTotal { get; set; }
    public int FastEnemyKillWindowsHit { get; set; }
    public double AverageTopTargetContribution { get; set; }
    public double OffensiveConditionPressure { get; set; }
    public double ControlConditionPressure { get; set; }
    public int StripsTotal { get; set; }
    public int StripDownContribution { get; set; }
    public double StripDownContributionTime { get; set; }
    public long HealingTotal { get; set; }
    public long BarrierTotal { get; set; }
    public long PetMinionAbsorptionTotal { get; set; }
    public double AttributedNegatedDamageTotal { get; set; }
    public int CleansesTotal { get; set; }
    public int ResurrectsTotal { get; set; }
    public double ResurrectTime { get; set; }
    public int OffensiveBoonWindows { get; set; }
    public int DefensiveBoonWindows { get; set; }
    public int BoonContributionWindows { get; set; }
    public double OffensiveBoonSupport { get; set; }
    public double DefensiveBoonSupport { get; set; }
    public double DefensiveConditionPressure { get; set; }
    public int EffectiveCrowdControlCount { get; set; }
    public double EffectiveCrowdControlDuration { get; set; }
    public int CrowdControlDownContribution { get; set; }
    public double CrowdControlDurationDownContribution { get; set; }
    public int BurstContributionWindows { get; set; }
    public int BurstWindowsTotal { get; set; }
    public int ConversionContributionWindows { get; set; }
    public int ConversionWindowsTotal { get; set; }
    public int ControlContributionWindows { get; set; }
    public int ControlWindowsTotal { get; set; }
    public int RecoveryContributionWindows { get; set; }
    public int RecoveryWindowsTotal { get; set; }
    public int DefensiveSupportWindows { get; set; }
    public int DefensiveSupportWindowsTotal { get; set; }
    public int SquadRecoveryWindowsHelped { get; set; }
    public int SquadRecoveryWindowsTotal { get; set; }
    public double DownedHealingOnRecoveries { get; set; }
    public double RezCountOnRecoveries { get; set; }
    public double RezTimeOnRecoveries { get; set; }
    public int BoonWindowsTotal { get; set; }
    public bool HasPositioningData { get; set; }
    public int PositioningSamples { get; set; }
    public double InPositionRate { get; set; }
    public double TooFarRate { get; set; }
    public double OverextendedRate { get; set; }
    public double LateralRiskRate { get; set; }
    public int Downs { get; set; }
    public int Deaths { get; set; }
    public int Recoveries { get; set; }
    public double FightDurationSeconds { get; set; }
    public double ActiveSeconds { get; set; }
    public double CombatSeconds { get; set; }
    public int KeyWindowsHit { get; set; }
    public int KeyWindowsTotal { get; set; }
    public List<CombatReplayPlayerEvaluationDetailEntryDto> EffectiveCrowdControlSources { get; set; } = [];
    public List<CombatReplayPlayerEvaluationDetailEntryDto> ControlConditionSources { get; set; } = [];
    public Dictionary<long, double> OffensiveBoonSupportByBuff { get; set; } = [];
    public Dictionary<long, double> DefensiveBoonSupportByBuff { get; set; } = [];
    public Dictionary<string, double> AttributedNegatedDamageByEffect { get; set; } = [];
}

internal class CombatReplayPlayerEvaluationMaximums
{
    public long DamageTotal { get; set; }
    public long LiveTargetDamage { get; set; }
    public int AgainstDownedDamage { get; set; }
    public int DownContribution { get; set; }
    public double EnemyDownContributionDamage { get; set; }
    public double EnemyKillContributionDamage { get; set; }
    public double AverageTopTargetContribution { get; set; }
    public double OffensiveConditionPressure { get; set; }
    public double ControlConditionPressure { get; set; }
    public int StripsTotal { get; set; }
    public int StripDownContribution { get; set; }
    public long HealingTotal { get; set; }
    public long BarrierTotal { get; set; }
    public long PetMinionAbsorptionTotal { get; set; }
    public double AttributedNegatedDamageTotal { get; set; }
    public int CleansesTotal { get; set; }
    public int ResurrectsTotal { get; set; }
    public double TotalBoonSupport { get; set; }
    public double OffensiveBoonSupport { get; set; }
    public double DefensiveBoonSupport { get; set; }
    public double DefensiveConditionPressure { get; set; }
    public int EffectiveCrowdControlCount { get; set; }
    public double EffectiveCrowdControlDuration { get; set; }
    public int CrowdControlDownContribution { get; set; }
    public int BurstContributionWindows { get; set; }
    public int BurstWindowsTotal { get; set; }
    public int ConversionContributionWindows { get; set; }
    public int ConversionWindowsTotal { get; set; }
    public int ControlContributionWindows { get; set; }
    public int ControlWindowsTotal { get; set; }
    public int RecoveryContributionWindows { get; set; }
    public int RecoveryWindowsTotal { get; set; }
    public int DefensiveSupportWindows { get; set; }
    public int DefensiveSupportWindowsTotal { get; set; }
    public int OffensiveBoonWindows { get; set; }
    public int DefensiveBoonWindows { get; set; }
    public int BoonContributionWindows { get; set; }
    public int BoonWindowsTotal { get; set; }
    public int SquadRecoveryWindowsHelped { get; set; }
    public double DownedHealingOnRecoveries { get; set; }
    public double RezTimeOnRecoveries { get; set; }
    public int KeyWindowsHit { get; set; }
}

internal class CombatReplayPlayerEvaluationTotals
{
    public double PressureContribution { get; set; }
    public long LiveTargetDamage { get; set; }
    public double ConversionContribution { get; set; }
    public int AgainstDownedDamage { get; set; }
    public int StripDownContribution { get; set; }
    public int StripsTotal { get; set; }
    public int CrowdControlDownContribution { get; set; }
    public int EffectiveCrowdControlCount { get; set; }
    public double TotalBoonSupport { get; set; }
    public long HealingTotal { get; set; }
    public long BarrierTotal { get; set; }
    public long PetMinionAbsorptionTotal { get; set; }
    public double AttributedNegatedDamageTotal { get; set; }
    public int CleansesTotal { get; set; }
    public int RecoveryContributionWindows { get; set; }
    public double DefensiveConditionPressure { get; set; }
    public int DefensiveSupportWindows { get; set; }
    public int SquadRecoveryWindowsHelped { get; set; }
    public double RezTimeOnRecoveries { get; set; }
}

internal class CombatReplaySpecCapabilityAggregate
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public int PlayerCount { get; set; }
    public double ActiveSeconds { get; set; }
    public double FightDurationSeconds { get; set; }
    public CombatReplayPlayerEvaluationAggregate Aggregate { get; set; } = new();
    public List<CombatReplayPlayerEvaluationAggregate> Players { get; set; } = [];
}

internal class PlayerAttributedNegationSummary
{
    public double TotalAmount { get; set; }
    public Dictionary<string, double> AmountByEffect { get; set; } = [];
}

internal static class CombatReplayAnalysisBuilder
{
    private const string MysticRebukeSkillName = "Mystic Rebuke";
    private const string OffensiveProtocolObliterateSkillNamePrefix = "Offensive Protocol: Obliterate";
    private const int LookbackWindow = 3000;
    private const int DamageOverlayLookbackWindow = 1000;
    private const double DamageOverlayFullHeatPercentile = 0.95;
    private const int DamageOverlayTopContributorCount = 3;
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
    private static readonly HashSet<long> OffensiveProtocolObliterateSkillIds =
    [
        OffensiveProtocolObliterate,
        OffensiveProtocolObliterate2,
        OffensiveProtocolObliterate3,
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

    private readonly record struct DamageRecord(long Time, int TargetUniqueId, int AttackerUniqueId, long SkillId, int Damage, bool HasDowned, bool HasKilled);
    private readonly record struct HealingRecord(long Time, int AttackerUniqueId, int Healing);
    private readonly record struct BarrierRecord(long Time, int AttackerUniqueId, int Barrier);
    private readonly record struct CleanseRecord(long Time, int AttackerUniqueId);
    private readonly record struct StripRecord(long Time, int TargetUniqueId, int AttackerUniqueId);
    private readonly record struct EvaluationWindow(long Start, long End);
    private readonly record struct PlayerEventContributionSummary(double TotalAmount, int WindowsHit, int WindowsTotal, int FastWindowsHit);
    private readonly record struct PlayerRecoveryContributionSummary(int WindowsHit, int WindowsTotal, double DownedHealing, double RezCasts, double RezTime);
    private readonly record struct EvaluationBuildResult(
        Dictionary<int, CombatReplayPlayerEvaluationDto> PlayerEvaluations,
        List<CombatReplaySpecCapabilityDto> SpecCapabilities);
    private readonly record struct PlayerLaneSnapshot(
        string Key,
        string Label,
        double StrengthPercent,
        double SharePercent,
        int WindowsHit,
        int WindowsTotal,
        string WindowLabel,
        string RateBand,
        string EvidenceLine,
        bool IsInteractive,
        string DrilldownTitle,
        string DrilldownSubtitle,
        List<CombatReplayPlayerEvaluationDetailSectionDto> DetailSections,
        List<CombatReplayPlayerLaneMetricDto> Metrics);
    private readonly record struct SpecLaneSnapshot(
        string Key,
        string Label,
        double StrengthPercent,
        double SharePercent,
        double PerSlotEfficiency,
        int PlayersContributing,
        int PlayerCount,
        double TopContributorSharePercent,
        string RateBand,
        string DependencyLabel,
        string EvidenceLine,
        bool IsInteractive,
        string DrilldownTitle,
        string DrilldownSubtitle,
        List<CombatReplayPlayerEvaluationDetailSectionDto> DetailSections);
    private readonly record struct DownOutcomeInfo(string Outcome, long? TransitionTime);
    private readonly record struct ObliterateBarrierSummary(
        int HitCount,
        int BarrierRemovedHitCount);
    private readonly record struct DamageWindowSummary(
        int TotalDamageTaken,
        int StrikeDamageTaken,
        int MysticRebukeDamageTaken,
        int ConditionDamageTaken,
        int BarrierDamageTaken,
        int HitCount,
        int ContributorCount,
        int OffensiveProtocolObliterateHitCount,
        int OffensiveProtocolObliterateBarrierRemovedHitCount,
        IReadOnlyList<CombatReplayEventContributionDto> Conditions,
        IReadOnlyList<CombatReplayEventContributionDto> ConditionDamageBreakdown,
        IReadOnlyList<CombatReplayEventContributionDto> Contributors,
        IReadOnlyList<CombatReplayEventTimelineEntryDto> DamageTimeline);
    private readonly record struct TeamActorContext(
        IReadOnlyList<SingleActor> Attackers,
        IReadOnlyList<SingleActor> Targets,
        IReadOnlyDictionary<AgentItem, int> AttackerIdsByAgent,
        string Label);

    public static CombatReplayAnalysisDto? Build(ParsedEvtcLog log, Dictionary<long, SkillItem>? usedSkills = null)
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
        CombatReplayEventAnalysisDto eventAnalysis = BuildEventAnalysis(log, squadPlayers, hostileTargets);
        CombatReplayDefenseAnalysisDto defenseAnalysis = BuildDefenseAnalysis(log, squadPlayers, enemyAnalysis, times);
        CombatReplayFightDemandDto fightDemand = BuildFightDemand(squadAnalysis, enemyAnalysis, eventAnalysis, defenseAnalysis, threatAnalysis, times);
        CombatReplayDamageOverlayDto damageOverlay = BuildDamageOverlay(log, squadContext, enemyContext, times, snapshotCount, usedSkills);
        EvaluationBuildResult evaluationData = BuildEvaluationData(
            log,
            squadPlayers,
            hostileTargets,
            squadAnalysis,
            enemyAnalysis,
            positioningAnalysis,
            eventAnalysis,
            fightDemand,
            times);

        return new CombatReplayAnalysisDto
        {
            Lookback = LookbackWindow,
            HasHealingData = log.CombatData.HasEXTHealing,
            Times = times,
            Squad = squadAnalysis,
            Enemy = enemyAnalysis,
            ThreatBoons = threatAnalysis,
            Positioning = positioningAnalysis,
            Events = eventAnalysis,
            Defense = defenseAnalysis,
            FightDemand = fightDemand,
            DamageOverlay = damageOverlay,
            PlayerEvaluations = evaluationData.PlayerEvaluations,
            SpecCapabilities = evaluationData.SpecCapabilities,
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

    private static CombatReplayDamageOverlayDto BuildDamageOverlay(
        ParsedEvtcLog log,
        TeamActorContext squadContext,
        TeamActorContext enemyContext,
        long[] times,
        int snapshotCount,
        Dictionary<long, SkillItem>? usedSkills)
    {
        var result = new CombatReplayDamageOverlayDto
        {
            Lookback = DamageOverlayLookbackWindow,
        };

        AddDamageOverlayEntries(log, squadContext, "enemy", times, snapshotCount, result.Entries, usedSkills);
        AddDamageOverlayEntries(log, enemyContext, "squad", times, snapshotCount, result.Entries, usedSkills);
        result.FullHeatDamage = ComputeDamageOverlayFullHeatDamage(result.Entries);
        return result;
    }

    private static int ComputeDamageOverlayFullHeatDamage(IEnumerable<CombatReplayDamageOverlayEntryDto> entries)
    {
        List<long> damageSamples = [.. entries
            .SelectMany(entry => entry.DamageTaken)
            .Where(damage => damage > 0)
            .Order()];
        if (damageSamples.Count == 0)
        {
            return 1;
        }
        int percentileIndex = Math.Min(
            damageSamples.Count - 1,
            Math.Max(0, (int)Math.Floor((damageSamples.Count - 1) * DamageOverlayFullHeatPercentile)));
        long fullHeatDamage = Math.Max(1, damageSamples[percentileIndex]);
        return fullHeatDamage > int.MaxValue ? int.MaxValue : (int)fullHeatDamage;
    }

    private static void AddDamageOverlayEntries(
        ParsedEvtcLog log,
        TeamActorContext context,
        string targetSide,
        long[] times,
        int snapshotCount,
        List<CombatReplayDamageOverlayEntryDto> entries,
        Dictionary<long, SkillItem>? usedSkills)
    {
        List<DamageRecord> damageRecords = BuildDamageRecords(log, context, usedSkills);
        var timelines = new Dictionary<int, long[]>();
        var topContributorsByTarget = new Dictionary<int, Dictionary<int, long[][]>>();
        var damageIndexStart = 0;
        var damageIndexEnd = 0;

        for (var snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
        {
            long time = times[snapshotIndex];
            long windowStart = Math.Max(0, time - DamageOverlayLookbackWindow);
            var contributorDamageByTarget = new Dictionary<int, Dictionary<(int SourceId, long SkillId), long>>();

            while (damageIndexStart < damageRecords.Count && damageRecords[damageIndexStart].Time < windowStart)
            {
                damageIndexStart++;
            }
            while (damageIndexEnd < damageRecords.Count && damageRecords[damageIndexEnd].Time <= time)
            {
                damageIndexEnd++;
            }

            for (var index = damageIndexStart; index < damageIndexEnd; index++)
            {
                DamageRecord damage = damageRecords[index];
                if (damage.Damage <= 0)
                {
                    continue;
                }
                if (!timelines.TryGetValue(damage.TargetUniqueId, out long[]? timeline))
                {
                    timeline = new long[snapshotCount];
                    timelines[damage.TargetUniqueId] = timeline;
                }
                timeline[snapshotIndex] += damage.Damage;

                if (!contributorDamageByTarget.TryGetValue(damage.TargetUniqueId, out Dictionary<(int SourceId, long SkillId), long>? contributorDamage))
                {
                    contributorDamage = [];
                    contributorDamageByTarget[damage.TargetUniqueId] = contributorDamage;
                }
                var contributorKey = (damage.AttackerUniqueId, damage.SkillId);
                contributorDamage[contributorKey] = contributorDamage.TryGetValue(contributorKey, out long currentDamage)
                    ? currentDamage + damage.Damage
                    : damage.Damage;
            }

            foreach ((int targetUniqueId, Dictionary<(int SourceId, long SkillId), long> contributorDamage) in contributorDamageByTarget)
            {
                long[][] topContributors = contributorDamage
                    .OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key.SourceId)
                    .ThenBy(pair => pair.Key.SkillId)
                    .Take(DamageOverlayTopContributorCount)
                    .Select(pair => new long[] { pair.Key.SourceId, pair.Key.SkillId, pair.Value })
                    .ToArray();
                if (topContributors.Length == 0)
                {
                    continue;
                }
                if (!topContributorsByTarget.TryGetValue(targetUniqueId, out Dictionary<int, long[][]>? snapshots))
                {
                    snapshots = [];
                    topContributorsByTarget[targetUniqueId] = snapshots;
                }
                snapshots[snapshotIndex] = topContributors;
            }
        }

        foreach ((int uniqueId, long[] timeline) in timelines.OrderBy(pair => pair.Key))
        {
            entries.Add(new CombatReplayDamageOverlayEntryDto
            {
                UniqueId = uniqueId,
                TargetSide = targetSide,
                DamageTaken = timeline,
                TopContributors = topContributorsByTarget.GetValueOrDefault(uniqueId) ?? [],
            });
        }
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

    private static EvaluationBuildResult BuildEvaluationData(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets,
        CombatReplayTeamAnalysisDto squadAnalysis,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        CombatReplayPositioningAnalysisDto positioningAnalysis,
        CombatReplayEventAnalysisDto eventAnalysis,
        CombatReplayFightDemandDto fightDemand,
        IReadOnlyList<long> times)
    {
        List<EvaluationWindow> burstWindows = BuildBurstWindows(squadAnalysis, times);
        List<EvaluationWindow> conversionWindows = BuildConversionWindows(squadAnalysis, times, log.LogData.LogEnd);
        List<EvaluationWindow> defensiveResponseWindows = BuildBurstWindows(enemyAnalysis, times);
        List<EvaluationWindow> offensiveConditionWindows = MergeEvaluationWindows([.. burstWindows, .. conversionWindows]);
        List<EvaluationWindow> keyWindows = MergeEvaluationWindows([.. offensiveConditionWindows, .. defensiveResponseWindows]);
        Dictionary<int, PlayerEventContributionSummary> enemyDownContributions = BuildPlayerEventContributionSummaries(
            [.. eventAnalysis.Downs.Events.Where(evt => evt.IsEnemy)],
            downEvent => downEvent.Contributors,
            contributor => contributor.Amount);
        Dictionary<int, PlayerEventContributionSummary> enemyKillContributions = BuildPlayerEventContributionSummaries(
            [.. eventAnalysis.Kills.Events.Where(evt => evt.IsEnemy)],
            killEvent => killEvent.Contributors,
            contributor => contributor.Amount,
            killEvent => killEvent.OutcomeTime.HasValue && killEvent.OutcomeTime.Value - killEvent.WindowStart <= 5000);
        Dictionary<int, PlayerAttributedNegationSummary> attributedNegationContributions = BuildPlayerAttributedNegationSummaries(log, squadPlayers);
        Dictionary<int, PlayerRecoveryContributionSummary> squadRecoveryContributions = BuildPlayerRecoveryContributionSummaries(
            [.. eventAnalysis.Recovered.Events.Where(evt => !evt.IsEnemy && evt.UsesSupportView)]);
        var aggregates = new List<CombatReplayPlayerEvaluationAggregate>(squadPlayers.Count);
        foreach (SingleActor player in squadPlayers)
        {
            aggregates.Add(BuildPlayerEvaluationAggregate(
                log,
                player,
                squadPlayers,
                hostileTargets,
                squadAnalysis,
                enemyAnalysis,
                positioningAnalysis,
                burstWindows,
                conversionWindows,
                defensiveResponseWindows,
                offensiveConditionWindows,
                keyWindows,
                enemyDownContributions,
                enemyKillContributions,
                attributedNegationContributions,
                squadRecoveryContributions,
                times));
        }

        CombatReplayPlayerEvaluationMaximums maximums = BuildPlayerEvaluationMaximums(aggregates);
        CombatReplayPlayerEvaluationTotals totals = BuildPlayerEvaluationTotals(aggregates, log.CombatData.HasEXTHealing, log.CombatData.HasEXTBarrier);
        Dictionary<int, CombatReplayPlayerEvaluationDto> playerEvaluations = aggregates.ToDictionary(
            aggregate => aggregate.PlayerId,
            aggregate => BuildPlayerEvaluationDto(log, aggregate, maximums, totals, fightDemand, log.CombatData.HasEXTHealing, log.CombatData.HasEXTBarrier));
        List<CombatReplaySpecCapabilityDto> specCapabilities = BuildSpecCapabilities(
            log,
            squadPlayers,
            aggregates,
            totals,
            fightDemand,
            log.CombatData.HasEXTHealing,
            log.CombatData.HasEXTBarrier);
        return new EvaluationBuildResult(playerEvaluations, specCapabilities);
    }

    private static CombatReplayPlayerEvaluationAggregate BuildPlayerEvaluationAggregate(
        ParsedEvtcLog log,
        SingleActor player,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<SingleActor> hostileTargets,
        CombatReplayTeamAnalysisDto squadAnalysis,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        CombatReplayPositioningAnalysisDto positioningAnalysis,
        IReadOnlyList<EvaluationWindow> burstWindows,
        IReadOnlyList<EvaluationWindow> conversionWindows,
        IReadOnlyList<EvaluationWindow> defensiveResponseWindows,
        IReadOnlyList<EvaluationWindow> offensiveConditionWindows,
        IReadOnlyList<EvaluationWindow> keyWindows,
        IReadOnlyDictionary<int, PlayerEventContributionSummary> enemyDownContributions,
        IReadOnlyDictionary<int, PlayerEventContributionSummary> enemyKillContributions,
        IReadOnlyDictionary<int, PlayerAttributedNegationSummary> attributedNegationContributions,
        IReadOnlyDictionary<int, PlayerRecoveryContributionSummary> squadRecoveryContributions,
        IReadOnlyList<long> times)
    {
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline = squadAnalysis.Attackers.GetValueOrDefault(player.UniqueID);
        CombatReplayPositioningPlayerTimelineDto? positioningTimeline = positioningAnalysis.Players.GetValueOrDefault(player.UniqueID);
        SupportStatistics supportStats = player.GetToAllySupportStats(log, 0, log.LogData.LogEnd);
        OffensiveStatistics offensiveStats = player.GetOffensiveStats(null, log, 0, log.LogData.LogEnd);
        DefenseAllStatistics defenseStats = player.GetDefenseStats(log, 0, log.LogData.LogEnd);
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
        long wholeFightPetMinionAbsorption = ComputePetMinionAbsorptionTotal(log, player);

        List<CrowdControlEvent> effectiveCrowdControlEvents = GetEffectiveCrowdControlEvents(log, player, hostileTargets);
        int effectiveCount = effectiveCrowdControlEvents.Count;
        double effectiveDuration = Math.Round(effectiveCrowdControlEvents.Sum(crowdControlEvent => crowdControlEvent.Duration) / 1000.0, 1);
        Dictionary<EvaluationWindow, double> burstOffensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, burstWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> conversionOffensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, conversionWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> offensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, offensiveConditionWindows, OffensiveConditionBuffIds);
        Dictionary<EvaluationWindow, double> controlConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, conversionWindows, ControlConditionBuffIds);
        Dictionary<EvaluationWindow, double> defensiveConditionContribution = ComputeConditionContributionByWindow(log, player, hostileTargets, defensiveResponseWindows, DefensiveConditionBuffIds);
        Dictionary<long, double> controlConditionSourceContribution = ComputeConditionContributionByBuff(log, player, hostileTargets, conversionWindows, ControlConditionBuffIds);
        Dictionary<EvaluationWindow, double> offensiveBoonContribution = ComputeBoonSupportContributionByWindow(log, player, squadPlayers, offensiveConditionWindows, OffensiveSupportBoonIds);
        Dictionary<EvaluationWindow, double> defensiveBoonContribution = ComputeBoonSupportContributionByWindow(log, player, squadPlayers, defensiveResponseWindows, DefensiveSupportBoonIds);
        Dictionary<long, double> offensiveBoonContributionByBuff = ComputeBoonSupportContributionByBuff(log, player, squadPlayers, offensiveConditionWindows, OffensiveSupportBoonIds);
        Dictionary<long, double> defensiveBoonContributionByBuff = ComputeBoonSupportContributionByBuff(log, player, squadPlayers, defensiveResponseWindows, DefensiveSupportBoonIds);
        List<EvaluationWindow> boonWindows = MergeEvaluationWindows([.. offensiveConditionWindows, .. defensiveResponseWindows]);
        Dictionary<EvaluationWindow, double> mergedBoonContribution = ComputeBoonSupportContributionByWindow(
            log,
            player,
            squadPlayers,
            boonWindows,
            [.. OffensiveSupportBoonIds.Union(DefensiveSupportBoonIds)]);
        double offensiveBoonSupport = Math.Round(offensiveBoonContribution.Values.Sum(), 1);
        double defensiveBoonSupport = Math.Round(defensiveBoonContribution.Values.Sum(), 1);
        enemyDownContributions.TryGetValue(player.UniqueID, out PlayerEventContributionSummary enemyDownContribution);
        enemyKillContributions.TryGetValue(player.UniqueID, out PlayerEventContributionSummary enemyKillContribution);
        attributedNegationContributions.TryGetValue(player.UniqueID, out PlayerAttributedNegationSummary? attributedNegationContribution);
        squadRecoveryContributions.TryGetValue(player.UniqueID, out PlayerRecoveryContributionSummary squadRecoveryContribution);

        return new CombatReplayPlayerEvaluationAggregate
        {
            PlayerId = player.UniqueID,
            DamageTotal = wholeFightDamageToPlayers,
            LiveTargetDamage = Math.Max(wholeFightDamageToPlayers - offensiveStats.AgainstDownedDamage, 0),
            AgainstDownedDamage = offensiveStats.AgainstDownedDamage,
            DownContribution = offensiveStats.DownContribution,
            EnemyDownContributionDamage = Math.Round(enemyDownContribution.TotalAmount, 1),
            EnemyDownWindowsHit = enemyDownContribution.WindowsHit,
            EnemyDownWindowsTotal = enemyDownContribution.WindowsTotal,
            EnemyKillContributionDamage = Math.Round(enemyKillContribution.TotalAmount, 1),
            EnemyKillWindowsHit = enemyKillContribution.WindowsHit,
            EnemyKillWindowsTotal = enemyKillContribution.WindowsTotal,
            FastEnemyKillWindowsHit = enemyKillContribution.FastWindowsHit,
            AverageTopTargetContribution = ComputeAverageContribution(attackerTimeline?.TopTargetContribution, attackerTimeline?.Damage),
            OffensiveConditionPressure = Math.Round(offensiveConditionContribution.Values.Sum(), 1),
            ControlConditionPressure = Math.Round(controlConditionContribution.Values.Sum(), 1),
            StripsTotal = supportStats.BoonStripCount,
            StripDownContribution = supportStats.BoonStripDownContribution,
            StripDownContributionTime = supportStats.BoonStripDownContributionTime,
            HealingTotal = wholeFightHealing,
            BarrierTotal = wholeFightBarrier,
            PetMinionAbsorptionTotal = wholeFightPetMinionAbsorption,
            AttributedNegatedDamageTotal = Math.Round(attributedNegationContribution?.TotalAmount ?? 0.0, 1),
            CleansesTotal = supportStats.ConditionCleanseCount,
            ResurrectsTotal = supportStats.ResurrectCount,
            ResurrectTime = supportStats.ResurrectTime,
            OffensiveBoonWindows = offensiveBoonContribution.Count,
            DefensiveBoonWindows = defensiveBoonContribution.Count,
            BoonContributionWindows = mergedBoonContribution.Count,
            OffensiveBoonSupport = offensiveBoonSupport,
            DefensiveBoonSupport = defensiveBoonSupport,
            DefensiveConditionPressure = Math.Round(defensiveConditionContribution.Values.Sum(), 1),
            EffectiveCrowdControlCount = effectiveCount,
            EffectiveCrowdControlDuration = effectiveDuration,
            CrowdControlDownContribution = offensiveStats.AppliedCrowdControlDownContribution,
            CrowdControlDurationDownContribution = Math.Round(offensiveStats.AppliedCrowdControlDurationDownContribution / 1000.0, 1),
            BurstContributionWindows = CountBurstContributionWindows(attackerTimeline, burstWindows, times, burstOffensiveConditionContribution),
            BurstWindowsTotal = burstWindows.Count,
            ConversionContributionWindows = CountOffensiveConversionWindows(attackerTimeline, conversionWindows, times, conversionOffensiveConditionContribution),
            ConversionWindowsTotal = conversionWindows.Count,
            ControlContributionWindows = CountControlContributionWindows(attackerTimeline, conversionWindows, times, effectiveCrowdControlEvents, controlConditionContribution),
            ControlWindowsTotal = conversionWindows.Count,
            RecoveryContributionWindows = CountRecoveryContributionWindows(attackerTimeline, defensiveResponseWindows, times),
            RecoveryWindowsTotal = defensiveResponseWindows.Count,
            DefensiveSupportWindows = CountPreventionContributionWindows(attackerTimeline, defensiveResponseWindows, times, defensiveConditionContribution),
            DefensiveSupportWindowsTotal = defensiveResponseWindows.Count,
            SquadRecoveryWindowsHelped = squadRecoveryContribution.WindowsHit,
            SquadRecoveryWindowsTotal = squadRecoveryContribution.WindowsTotal,
            DownedHealingOnRecoveries = Math.Round(squadRecoveryContribution.DownedHealing, 1),
            RezCountOnRecoveries = Math.Round(squadRecoveryContribution.RezCasts, 1),
            RezTimeOnRecoveries = Math.Round(squadRecoveryContribution.RezTime, 1),
            BoonWindowsTotal = boonWindows.Count,
            HasPositioningData = positioningTimeline != null && positioningTimeline.Eligible.Any(sample => sample),
            PositioningSamples = CountEligibleSamples(positioningTimeline),
            InPositionRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.InPosition),
            TooFarRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.TooFar),
            OverextendedRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.Overextended),
            LateralRiskRate = ComputeEligibleRate(positioningTimeline, timeline => timeline.LateralRisk),
            Downs = defenseStats.DownCount,
            Deaths = defenseStats.DeadCount,
            Recoveries = BuildPlayerRecoveryCount(log, player),
            FightDurationSeconds = Math.Round((log.LogData.LogEnd - log.LogData.LogStart) / 1000.0, 1),
            ActiveSeconds = Math.Round(player.GetActiveDuration(log, 0, log.LogData.LogEnd) / 1000.0, 1),
            CombatSeconds = Math.Round(player.GetTimeSpentInCombat(log, 0, log.LogData.LogEnd) / 1000.0, 1),
            KeyWindowsHit = CountKeyContributionWindows(
                keyWindows,
                times,
                attackerTimeline,
                effectiveCrowdControlEvents,
                offensiveConditionContribution,
                controlConditionContribution,
                defensiveConditionContribution,
                offensiveBoonContribution,
                defensiveBoonContribution),
            KeyWindowsTotal = keyWindows.Count,
            EffectiveCrowdControlSources = BuildEffectiveCrowdControlSourceEntries(effectiveCrowdControlEvents),
            ControlConditionSources = BuildConditionSourceEntries(log, controlConditionSourceContribution),
            OffensiveBoonSupportByBuff = offensiveBoonContributionByBuff,
            DefensiveBoonSupportByBuff = defensiveBoonContributionByBuff,
            AttributedNegatedDamageByEffect = attributedNegationContribution?.AmountByEffect != null
                ? new Dictionary<string, double>(attributedNegationContribution.AmountByEffect, StringComparer.OrdinalIgnoreCase)
                : [],
        };
    }

    private static CombatReplayPlayerEvaluationDto BuildPlayerEvaluationDto(
        ParsedEvtcLog log,
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        CombatReplayFightDemandDto fightDemand,
        bool hasHealingData,
        bool hasBarrierData)
    {
        List<PlayerLaneSnapshot> laneSnapshots =
        [
            BuildPressureLaneSnapshot(aggregate, maximums, totals),
            BuildConversionLaneSnapshot(aggregate, maximums, totals),
            BuildStripLaneSnapshot(aggregate, maximums, totals),
            BuildControlLaneSnapshot(aggregate, maximums, totals),
            BuildBoonSupportLaneSnapshot(log, aggregate, maximums, totals),
            BuildRecoveryLaneSnapshot(aggregate, maximums, totals, hasHealingData, hasBarrierData),
            BuildPreventionLaneSnapshot(aggregate, maximums, totals, hasBarrierData),
            BuildRezLaneSnapshot(aggregate, maximums, totals),
        ];
        laneSnapshots = [.. laneSnapshots.OrderByDescending(lane => lane.StrengthPercent).ThenByDescending(lane => lane.SharePercent).ThenBy(lane => lane.Label)];
        CombatReplayContributionConfidenceDto confidence = BuildPlayerEvaluationConfidence(aggregate, hasHealingData, hasBarrierData);
        CombatReplayPlayerFightImpactDto fightImpact = BuildPlayerFightImpact(laneSnapshots, fightDemand, confidence, hasHealingData, hasBarrierData);
        string fitSummary = BuildPlayerFitSummary(aggregate, laneSnapshots, fightDemand, confidence);
        string demandFitSummary = BuildPlayerDemandFitSummary(aggregate, laneSnapshots, fightDemand, confidence);
        string contributionProfile = BuildLegacyContributionProfile(laneSnapshots);
        string keyContributionSummary = demandFitSummary;

        return new CombatReplayPlayerEvaluationDto
        {
            FitSummary = fitSummary,
            DemandFitSummary = demandFitSummary,
            Confidence = confidence,
            FightImpact = fightImpact,
            Lanes = [.. laneSnapshots.Select(snapshot => new CombatReplayPlayerContributionLaneDto
            {
                Key = snapshot.Key,
                Label = snapshot.Label,
                StrengthPercent = snapshot.StrengthPercent,
                SharePercent = snapshot.SharePercent,
                WindowsHit = snapshot.WindowsHit,
                WindowsTotal = snapshot.WindowsTotal,
                WindowLabel = snapshot.WindowLabel,
                RateBand = snapshot.RateBand,
                EvidenceLine = snapshot.EvidenceLine,
                IsInteractive = snapshot.IsInteractive,
                DrilldownTitle = snapshot.DrilldownTitle,
                DrilldownSubtitle = snapshot.DrilldownSubtitle,
                DetailSections = snapshot.DetailSections,
                Metrics = snapshot.Metrics,
            })],
            Modifiers = BuildPlayerModifiers(aggregate),
            EvidenceSnapshot = BuildEvidenceSnapshot(aggregate, laneSnapshots, hasHealingData, hasBarrierData),
            ContributionProfile = contributionProfile,
            KeyContributionSummary = keyContributionSummary,
            RoleMix = [],
            Areas = [],
        };
    }

    private static List<CombatReplaySpecCapabilityDto> BuildSpecCapabilities(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyList<CombatReplayPlayerEvaluationAggregate> playerAggregates,
        CombatReplayPlayerEvaluationTotals totals,
        CombatReplayFightDemandDto fightDemand,
        bool hasHealingData,
        bool hasBarrierData)
    {
        if (playerAggregates.Count == 0)
        {
            return [];
        }

        Dictionary<int, SingleActor> playersById = squadPlayers.ToDictionary(player => player.UniqueID);
        double totalActiveSeconds = Math.Max(playerAggregates.Sum(aggregate => aggregate.ActiveSeconds), 0.1);
        List<CombatReplaySpecCapabilityAggregate> specAggregates =
        [
            .. playerAggregates
                .GroupBy(aggregate => playersById[aggregate.PlayerId].Spec.ToString(), StringComparer.Ordinal)
                .Select(group =>
                {
                    List<CombatReplayPlayerEvaluationAggregate> specPlayers = [.. group];
                    SingleActor actor = playersById[group.First().PlayerId];
                    return new CombatReplaySpecCapabilityAggregate
                    {
                        Key = actor.Spec.ToString(),
                        Label = actor.Spec.ToString(),
                        Icon = actor.GetIcon(),
                        PlayerCount = specPlayers.Count,
                        ActiveSeconds = Math.Round(specPlayers.Sum(player => player.ActiveSeconds), 1),
                        FightDurationSeconds = specPlayers.Max(player => player.FightDurationSeconds),
                        Aggregate = BuildSpecAggregate(specPlayers),
                        Players = specPlayers,
                    };
                }),
        ];
        CombatReplayPlayerEvaluationMaximums maximums = BuildPlayerEvaluationMaximums([.. specAggregates.Select(spec => spec.Aggregate)]);
        Dictionary<string, double> perPlayerMaximums = BuildSpecPerPlayerMaximums(specAggregates, totals, hasHealingData, hasBarrierData);
        return [.. specAggregates
            .Select(spec => BuildSpecCapabilityDto(log, spec, maximums, totals, perPlayerMaximums, totalActiveSeconds, fightDemand, hasHealingData, hasBarrierData))
            .OrderByDescending(spec => spec.ActiveSharePercent)
            .ThenBy(spec => spec.Label, StringComparer.OrdinalIgnoreCase)];
    }

    private static CombatReplayPlayerEvaluationAggregate BuildSpecAggregate(IReadOnlyList<CombatReplayPlayerEvaluationAggregate> players)
    {
        return new CombatReplayPlayerEvaluationAggregate
        {
            DamageTotal = players.Sum(player => player.DamageTotal),
            LiveTargetDamage = players.Sum(player => player.LiveTargetDamage),
            AgainstDownedDamage = players.Sum(player => player.AgainstDownedDamage),
            DownContribution = players.Sum(player => player.DownContribution),
            EnemyDownContributionDamage = Math.Round(players.Sum(player => player.EnemyDownContributionDamage), 1),
            EnemyDownWindowsHit = players.Max(player => player.EnemyDownWindowsHit),
            EnemyDownWindowsTotal = players.Max(player => player.EnemyDownWindowsTotal),
            EnemyKillContributionDamage = Math.Round(players.Sum(player => player.EnemyKillContributionDamage), 1),
            EnemyKillWindowsHit = players.Max(player => player.EnemyKillWindowsHit),
            EnemyKillWindowsTotal = players.Max(player => player.EnemyKillWindowsTotal),
            FastEnemyKillWindowsHit = players.Max(player => player.FastEnemyKillWindowsHit),
            AverageTopTargetContribution = ComputeAverageContribution(
                [.. players.Select(player => player.AverageTopTargetContribution)],
                [.. players.Select(player => player.LiveTargetDamage)]),
            OffensiveConditionPressure = Math.Round(players.Sum(player => player.OffensiveConditionPressure), 1),
            ControlConditionPressure = Math.Round(players.Sum(player => player.ControlConditionPressure), 1),
            StripsTotal = players.Sum(player => player.StripsTotal),
            StripDownContribution = players.Sum(player => player.StripDownContribution),
            StripDownContributionTime = Math.Round(players.Sum(player => player.StripDownContributionTime), 1),
            HealingTotal = players.Sum(player => player.HealingTotal),
            BarrierTotal = players.Sum(player => player.BarrierTotal),
            PetMinionAbsorptionTotal = players.Sum(player => player.PetMinionAbsorptionTotal),
            AttributedNegatedDamageTotal = Math.Round(players.Sum(player => player.AttributedNegatedDamageTotal), 1),
            CleansesTotal = players.Sum(player => player.CleansesTotal),
            ResurrectsTotal = players.Sum(player => player.ResurrectsTotal),
            ResurrectTime = Math.Round(players.Sum(player => player.ResurrectTime), 1),
            OffensiveBoonWindows = players.Max(player => player.OffensiveBoonWindows),
            DefensiveBoonWindows = players.Max(player => player.DefensiveBoonWindows),
            BoonContributionWindows = players.Max(player => player.BoonContributionWindows),
            OffensiveBoonSupport = Math.Round(players.Sum(player => player.OffensiveBoonSupport), 1),
            DefensiveBoonSupport = Math.Round(players.Sum(player => player.DefensiveBoonSupport), 1),
            DefensiveConditionPressure = Math.Round(players.Sum(player => player.DefensiveConditionPressure), 1),
            EffectiveCrowdControlCount = players.Sum(player => player.EffectiveCrowdControlCount),
            EffectiveCrowdControlDuration = Math.Round(players.Sum(player => player.EffectiveCrowdControlDuration), 1),
            CrowdControlDownContribution = players.Sum(player => player.CrowdControlDownContribution),
            CrowdControlDurationDownContribution = Math.Round(players.Sum(player => player.CrowdControlDurationDownContribution), 1),
            BurstContributionWindows = players.Max(player => player.BurstContributionWindows),
            BurstWindowsTotal = players.Max(player => player.BurstWindowsTotal),
            ConversionContributionWindows = players.Max(player => player.ConversionContributionWindows),
            ConversionWindowsTotal = players.Max(player => player.ConversionWindowsTotal),
            ControlContributionWindows = players.Max(player => player.ControlContributionWindows),
            ControlWindowsTotal = players.Max(player => player.ControlWindowsTotal),
            RecoveryContributionWindows = players.Max(player => player.RecoveryContributionWindows),
            RecoveryWindowsTotal = players.Max(player => player.RecoveryWindowsTotal),
            DefensiveSupportWindows = players.Max(player => player.DefensiveSupportWindows),
            DefensiveSupportWindowsTotal = players.Max(player => player.DefensiveSupportWindowsTotal),
            SquadRecoveryWindowsHelped = players.Max(player => player.SquadRecoveryWindowsHelped),
            SquadRecoveryWindowsTotal = players.Max(player => player.SquadRecoveryWindowsTotal),
            DownedHealingOnRecoveries = Math.Round(players.Sum(player => player.DownedHealingOnRecoveries), 1),
            RezCountOnRecoveries = Math.Round(players.Sum(player => player.RezCountOnRecoveries), 1),
            RezTimeOnRecoveries = Math.Round(players.Sum(player => player.RezTimeOnRecoveries), 1),
            BoonWindowsTotal = players.Max(player => player.BoonWindowsTotal),
            HasPositioningData = players.Any(player => player.HasPositioningData),
            PositioningSamples = players.Sum(player => player.PositioningSamples),
            Downs = players.Sum(player => player.Downs),
            Deaths = players.Sum(player => player.Deaths),
            Recoveries = players.Sum(player => player.Recoveries),
            FightDurationSeconds = players.Max(player => player.FightDurationSeconds),
            ActiveSeconds = Math.Round(players.Sum(player => player.ActiveSeconds), 1),
            CombatSeconds = Math.Round(players.Sum(player => player.CombatSeconds), 1),
            KeyWindowsHit = players.Max(player => player.KeyWindowsHit),
            KeyWindowsTotal = players.Max(player => player.KeyWindowsTotal),
            OffensiveBoonSupportByBuff = MergeContributionDictionaries(players.Select(player => player.OffensiveBoonSupportByBuff)),
            DefensiveBoonSupportByBuff = MergeContributionDictionaries(players.Select(player => player.DefensiveBoonSupportByBuff)),
            AttributedNegatedDamageByEffect = MergeContributionDictionaries(players.Select(player => player.AttributedNegatedDamageByEffect)),
        };
    }

    private static CombatReplaySpecCapabilityDto BuildSpecCapabilityDto(
        ParsedEvtcLog log,
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double totalActiveSeconds,
        CombatReplayFightDemandDto fightDemand,
        bool hasHealingData,
        bool hasBarrierData)
    {
        double activeSharePercent = ComputePercent(spec.ActiveSeconds, totalActiveSeconds);
        List<SpecLaneSnapshot> laneSnapshots =
        [
            BuildSpecPressureLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent),
            BuildSpecConversionLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent),
            BuildSpecStripLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent),
            BuildSpecControlLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent),
            BuildSpecBoonSupportLaneSnapshot(log, spec, maximums, totals, perPlayerMaximums, activeSharePercent),
            BuildSpecRecoveryLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent, hasHealingData, hasBarrierData),
            BuildSpecPreventionLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent, hasBarrierData),
            BuildSpecRezLaneSnapshot(spec, maximums, totals, perPlayerMaximums, activeSharePercent),
        ];
        laneSnapshots = [.. laneSnapshots
            .OrderByDescending(lane => lane.StrengthPercent)
            .ThenByDescending(lane => lane.SharePercent)
            .ThenBy(lane => lane.Label, StringComparer.OrdinalIgnoreCase)];
        CombatReplaySpecFightCoverageDto fightCoverage = BuildSpecFightCoverage(laneSnapshots, fightDemand, hasHealingData, hasBarrierData);
        return new CombatReplaySpecCapabilityDto
        {
            Key = spec.Key,
            Label = spec.Label,
            Icon = spec.Icon,
            PlayerIds = [.. spec.Players.Select(player => player.PlayerId)],
            PlayerCount = spec.PlayerCount,
            ActiveSharePercent = activeSharePercent,
            FitSummary = BuildSpecFitSummary(laneSnapshots, fightDemand),
            DemandFitSummary = BuildSpecDemandFitSummary(laneSnapshots, fightDemand),
            DependencySummary = BuildSpecDependencySummary(spec, laneSnapshots),
            FightCoverage = fightCoverage,
            Lanes = [.. laneSnapshots.Select(snapshot => new CombatReplaySpecCapabilityLaneDto
            {
                Key = snapshot.Key,
                Label = snapshot.Label,
                StrengthPercent = snapshot.StrengthPercent,
                SharePercent = snapshot.SharePercent,
                PerSlotEfficiency = snapshot.PerSlotEfficiency,
                PlayersContributing = snapshot.PlayersContributing,
                PlayerCount = snapshot.PlayerCount,
                TopContributorSharePercent = snapshot.TopContributorSharePercent,
                RateBand = snapshot.RateBand,
                DependencyLabel = snapshot.DependencyLabel,
                EvidenceLine = snapshot.EvidenceLine,
                IsInteractive = snapshot.IsInteractive,
                DrilldownTitle = snapshot.DrilldownTitle,
                DrilldownSubtitle = snapshot.DrilldownSubtitle,
                DetailSections = snapshot.DetailSections,
            })],
            EvidenceSnapshot = BuildSpecEvidenceSnapshot(spec, laneSnapshots),
        };
    }

    private static SpecLaneSnapshot BuildSpecPressureLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildPressureLaneSnapshot(spec.Aggregate, maximums, totals);
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecPressureRawAmount(aggregate, totals),
            perPlayerMaximums.GetValueOrDefault("pressure"),
            $"{FormatWholeNumber(spec.Aggregate.LiveTargetDamage)} live-target damage and {FormatWholeNumber((long)Math.Round(spec.Aggregate.EnemyDownContributionDamage))} pre-down pressure made {spec.Label} visible before enemy downs.");
    }

    private static SpecLaneSnapshot BuildSpecConversionLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildConversionLaneSnapshot(spec.Aggregate, maximums, totals);
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecConversionRawAmount(aggregate, totals),
            perPlayerMaximums.GetValueOrDefault("conversion"),
            $"{FormatWholeNumber((long)Math.Round(spec.Aggregate.EnemyKillContributionDamage))} finish contribution and {FormatWholeNumber(spec.Aggregate.AgainstDownedDamage)} against-downed damage helped this spec close conversions.");
    }

    private static SpecLaneSnapshot BuildSpecStripLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildStripLaneSnapshot(spec.Aggregate, maximums, totals);
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecStripRawAmount(aggregate, totals),
            perPlayerMaximums.GetValueOrDefault("strip"),
            $"{FormatWholeNumber(spec.Aggregate.StripsTotal)} strips and {FormatWholeNumber(spec.Aggregate.StripDownContribution)} down-linked strips show where {spec.Label} cracked enemy boon cover.");
    }

    private static SpecLaneSnapshot BuildSpecControlLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildControlLaneSnapshot(spec.Aggregate, maximums, totals);
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecControlRawAmount(aggregate, totals),
            perPlayerMaximums.GetValueOrDefault("control"),
            $"{FormatWholeNumber(spec.Aggregate.EffectiveCrowdControlCount)} effective CC events and {FormatWholeNumber(spec.Aggregate.CrowdControlDownContribution)} CC-linked downs show the visible control footprint for {spec.Label}.");
    }

    private static SpecLaneSnapshot BuildSpecBoonSupportLaneSnapshot(
        ParsedEvtcLog log,
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildBoonSupportLaneSnapshot(log, spec.Aggregate, maximums, totals);
        string boonLean = spec.Aggregate.DefensiveBoonSupport >= spec.Aggregate.OffensiveBoonSupport ? "Defensive" : "Offensive";
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecBoonSupportRawAmount(aggregate),
            perPlayerMaximums.GetValueOrDefault("boonSupport"),
            $"{boonLean} boon coverage was most visible, with {FormatWholeNumber((long)Math.Round(spec.Aggregate.OffensiveBoonSupport + spec.Aggregate.DefensiveBoonSupport))} total boon-seconds in key windows.",
            true,
            $"{spec.Label} Boon Support Detail",
            "Shows offensive and defensive boon-seconds by boon for this spec in the fight's key windows. Stack boons stay labeled as stack-seconds in the breakdown.",
            BuildBoonSupportDetailSections(log, spec.Aggregate));
    }

    private static SpecLaneSnapshot BuildSpecRecoveryLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent,
        bool hasHealingData,
        bool hasBarrierData)
    {
        PlayerLaneSnapshot baseLane = BuildRecoveryLaneSnapshot(spec.Aggregate, maximums, totals, hasHealingData, hasBarrierData);
        var recoveryEvidenceParts = new List<string>
        {
            $"{FormatWholeNumber(spec.Aggregate.CleansesTotal)} cleanses",
        };
        if (hasHealingData)
        {
            recoveryEvidenceParts.Add($"{FormatWholeNumber(spec.Aggregate.HealingTotal)} healing");
        }
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecRecoveryRawAmount(aggregate, hasHealingData, hasBarrierData),
            perPlayerMaximums.GetValueOrDefault("recovery"),
            $"{string.Join(", ", recoveryEvidenceParts)} gave {spec.Label} a visible recovery footprint.");
    }

    private static SpecLaneSnapshot BuildSpecPreventionLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent,
        bool hasBarrierData)
    {
        PlayerLaneSnapshot baseLane = BuildPreventionLaneSnapshot(spec.Aggregate, maximums, totals, hasBarrierData);
        var preventionEvidenceParts = new List<string>();
        if (hasBarrierData)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber(spec.Aggregate.BarrierTotal)} barrier");
        }
        if (spec.Aggregate.AttributedNegatedDamageTotal > 0.0)
        {
            preventionEvidenceParts.Add($"{FormatOneDecimal(spec.Aggregate.AttributedNegatedDamageTotal)} negated damage");
        }
        if (spec.Aggregate.PetMinionAbsorptionTotal > 0)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber(spec.Aggregate.PetMinionAbsorptionTotal)} pet absorption");
        }
        if (spec.Aggregate.DefensiveConditionPressure > 0.0)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber((long)Math.Round(spec.Aggregate.DefensiveConditionPressure))} defensive condition pressure");
        }
        string preventionSummary = preventionEvidenceParts.Count > 0
            ? $"{string.Join(", ", preventionEvidenceParts)} gave {spec.Label} a visible prevention footprint."
            : $"{spec.Label} showed prevention value through defensive windows.";
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecPreventionRawAmount(aggregate, hasBarrierData),
            perPlayerMaximums.GetValueOrDefault("prevention"),
            preventionSummary);
    }

    private static SpecLaneSnapshot BuildSpecRezLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        IReadOnlyDictionary<string, double> perPlayerMaximums,
        double activeSharePercent)
    {
        PlayerLaneSnapshot baseLane = BuildRezLaneSnapshot(spec.Aggregate, maximums, totals);
        return BuildSpecLaneSnapshot(
            spec,
            baseLane,
            activeSharePercent,
            aggregate => GetSpecRezRawAmount(aggregate, totals),
            perPlayerMaximums.GetValueOrDefault("rez"),
            $"{BuildPluralizedLabel(spec.Aggregate.SquadRecoveryWindowsHelped, "successful recovery", "successful recoveries")} were supported with {FormatOneDecimal(spec.Aggregate.RezTimeOnRecoveries)}s of rez time from {spec.Label}.");
    }

    private static SpecLaneSnapshot BuildSpecLaneSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        PlayerLaneSnapshot baseLane,
        double activeSharePercent,
        Func<CombatReplayPlayerEvaluationAggregate, double> rawSelector,
        double averagePerPlayerMaximum,
        string evidenceLine,
        bool isInteractive = false,
        string? drilldownTitle = null,
        string? drilldownSubtitle = null,
        List<CombatReplayPlayerEvaluationDetailSectionDto>? detailSections = null)
    {
        double rawAmount = Math.Max(rawSelector(spec.Aggregate), 0.0);
        int playersContributing = CountPlayersContributing(spec.Players, rawSelector);
        double topContributorSharePercent = ComputeTopContributorSharePercent(spec.Players, rawSelector, rawAmount);
        double perSlotEfficiency = ComputeSpecPerSlotEfficiency(baseLane.SharePercent, activeSharePercent);
        double strengthPercent = ComputeSpecLaneStrength(rawAmount, averagePerPlayerMaximum, baseLane.SharePercent, perSlotEfficiency, playersContributing, spec.PlayerCount);
        string dependencyLabel = GetDependencyLabel(baseLane.SharePercent, playersContributing, spec.PlayerCount, topContributorSharePercent);
        return new SpecLaneSnapshot(
            baseLane.Key,
            baseLane.Label,
            strengthPercent,
            baseLane.SharePercent,
            perSlotEfficiency,
            playersContributing,
            spec.PlayerCount,
            topContributorSharePercent,
            GetRateBand(strengthPercent),
            dependencyLabel,
            evidenceLine,
            isInteractive,
            drilldownTitle ?? "",
            drilldownSubtitle ?? "",
            detailSections ?? []);
    }

    private static string BuildSpecFitSummary(
        IReadOnlyList<SpecLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand)
    {
        if (laneSnapshots.Count == 0)
        {
            return "Observed spec contribution was too thin to summarize.";
        }

        SpecLaneSnapshot primaryLane = laneSnapshots[0];
        SpecLaneSnapshot? secondaryLane = SelectSecondarySpecLane(laneSnapshots);
        var alignedLanes = laneSnapshots
            .Select(lane => new
            {
                Lane = lane,
                Score = lane.StrengthPercent * GetDemandScore(fightDemand, lane.Key),
                Demand = GetDemandScore(fightDemand, lane.Key),
            })
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Demand)
            .ToList();
        var alignedPrimary = alignedLanes[0];
        if (alignedPrimary.Demand >= 0.55 && alignedPrimary.Score >= primaryLane.StrengthPercent * 0.35)
        {
            SpecLaneSnapshot? alignedSecondary = alignedLanes.Count > 1 && alignedLanes[1].Score >= alignedLanes[0].Score * 0.60
                ? alignedLanes[1].Lane
                : null;
            string alignedSecondaryText = alignedSecondary != null ? $" + {alignedSecondary.Value.Label}" : "";
            return $"This spec fit the fight through {alignedPrimary.Lane.Label}{alignedSecondaryText}.";
        }

        string secondaryText = secondaryLane != null ? $" + {secondaryLane.Value.Label}" : "";
        return $"This spec contributed most through {primaryLane.Label}{secondaryText}.";
    }

    private static string BuildSpecDemandFitSummary(
        IReadOnlyList<SpecLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand)
    {
        if (laneSnapshots.Count == 0)
        {
            return "Fight demand was too thin to frame this spec cleanly.";
        }

        List<CombatReplayFightDemandLaneDto> demandedLanes = [.. fightDemand.Lanes.Where(lane => lane.DemandScorePercent >= 55.0)];
        if (demandedLanes.Count == 0)
        {
            return "Fight demand was too evenly distributed to center one spec capability heavily.";
        }

        double coverage = demandedLanes.Average(demandedLane =>
        {
            SpecLaneSnapshot matchingLane = laneSnapshots.FirstOrDefault(lane => lane.Key == demandedLane.Key);
            return string.IsNullOrEmpty(matchingLane.Key) ? 0.0 : matchingLane.StrengthPercent;
        });
        if (coverage >= 60.0)
        {
            return "This spec covered a large share of the fight's biggest needs.";
        }
        if (coverage >= 35.0)
        {
            return $"This spec covered some of the fight's biggest needs, especially {demandedLanes[0].Label}.";
        }
        string demandLean = string.Join(" + ", demandedLanes.Take(2).Select(lane => lane.Label));
        return $"This spec's visible value was more situational than central in this fight, which leaned more on {demandLean}.";
    }

    private static string BuildSpecDependencySummary(
        CombatReplaySpecCapabilityAggregate spec,
        IReadOnlyList<SpecLaneSnapshot> laneSnapshots)
    {
        if (spec.PlayerCount <= 1)
        {
            return "This spec was represented by one player in this fight.";
        }

        var dependencyCandidates = laneSnapshots
            .Select(lane => new
            {
                Lane = lane,
                Score = ComputeDependencyScore(lane.SharePercent, lane.PlayersContributing, lane.PlayerCount, lane.TopContributorSharePercent),
            })
            .OrderByDescending(entry => entry.Score)
            .ToList();
        if (dependencyCandidates.Count == 0 || dependencyCandidates[0].Score < 25.0)
        {
            return "Coverage was broad across the spec.";
        }
        if (dependencyCandidates[0].Score >= 65.0)
        {
            return $"{dependencyCandidates[0].Lane.Label} value was concentrated in one player.";
        }
        if (dependencyCandidates[0].Score >= 45.0)
        {
            return $"{dependencyCandidates[0].Lane.Label} coverage leaned heavily on a smaller subset of players.";
        }
        return $"{dependencyCandidates[0].Lane.Label} value had some concentration, but coverage stayed fairly broad.";
    }

    private static Dictionary<string, double> BuildSpecPerPlayerMaximums(
        IReadOnlyList<CombatReplaySpecCapabilityAggregate> specs,
        CombatReplayPlayerEvaluationTotals totals,
        bool hasHealingData,
        bool hasBarrierData)
    {
        double ComputeMaxAverage(Func<CombatReplayPlayerEvaluationAggregate, double> selector)
        {
            return specs.Count == 0
                ? 0.0
                : specs.Max(spec => selector(spec.Aggregate) / Math.Max(spec.PlayerCount, 1));
        }

        return new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["pressure"] = ComputeMaxAverage(aggregate => GetSpecPressureRawAmount(aggregate, totals)),
            ["conversion"] = ComputeMaxAverage(aggregate => GetSpecConversionRawAmount(aggregate, totals)),
            ["strip"] = ComputeMaxAverage(aggregate => GetSpecStripRawAmount(aggregate, totals)),
            ["control"] = ComputeMaxAverage(aggregate => GetSpecControlRawAmount(aggregate, totals)),
            ["boonSupport"] = ComputeMaxAverage(GetSpecBoonSupportRawAmount),
            ["recovery"] = ComputeMaxAverage(aggregate => GetSpecRecoveryRawAmount(aggregate, hasHealingData, hasBarrierData)),
            ["prevention"] = ComputeMaxAverage(aggregate => GetSpecPreventionRawAmount(aggregate, hasBarrierData)),
            ["rez"] = ComputeMaxAverage(aggregate => GetSpecRezRawAmount(aggregate, totals)),
        };
    }

    private static double GetSpecPressureRawAmount(CombatReplayPlayerEvaluationAggregate aggregate, CombatReplayPlayerEvaluationTotals totals)
    {
        return totals.PressureContribution > 0.0 ? aggregate.EnemyDownContributionDamage : aggregate.LiveTargetDamage;
    }

    private static double GetSpecConversionRawAmount(CombatReplayPlayerEvaluationAggregate aggregate, CombatReplayPlayerEvaluationTotals totals)
    {
        return totals.ConversionContribution > 0.0 ? aggregate.EnemyKillContributionDamage : aggregate.AgainstDownedDamage;
    }

    private static double GetSpecStripRawAmount(CombatReplayPlayerEvaluationAggregate aggregate, CombatReplayPlayerEvaluationTotals totals)
    {
        return totals.StripDownContribution > 0 ? aggregate.StripDownContribution : aggregate.StripsTotal;
    }

    private static double GetSpecControlRawAmount(CombatReplayPlayerEvaluationAggregate aggregate, CombatReplayPlayerEvaluationTotals totals)
    {
        return totals.CrowdControlDownContribution > 0 ? aggregate.CrowdControlDownContribution : aggregate.EffectiveCrowdControlCount;
    }

    private static double GetSpecBoonSupportRawAmount(CombatReplayPlayerEvaluationAggregate aggregate)
    {
        return aggregate.OffensiveBoonSupport + aggregate.DefensiveBoonSupport;
    }

    private static double GetSpecRecoveryRawAmount(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasHealingData,
        bool hasBarrierData)
    {
        return ComputeRecoveryContributionMagnitude(aggregate, hasHealingData, hasBarrierData);
    }

    private static double GetSpecPreventionRawAmount(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasBarrierData)
    {
        return ComputePreventionContributionMagnitude(aggregate, hasBarrierData);
    }

    private static double GetSpecRezRawAmount(CombatReplayPlayerEvaluationAggregate aggregate, CombatReplayPlayerEvaluationTotals totals)
    {
        return totals.SquadRecoveryWindowsHelped > 0 ? aggregate.SquadRecoveryWindowsHelped : aggregate.RezTimeOnRecoveries;
    }

    private static List<string> BuildSpecEvidenceSnapshot(
        CombatReplaySpecCapabilityAggregate spec,
        IReadOnlyList<SpecLaneSnapshot> laneSnapshots)
    {
        var evidence = new List<string>
        {
            $"{spec.PlayerCount} {(spec.PlayerCount == 1 ? "player" : "players")} on {spec.Label} covered {FormatOneDecimal(ComputePercent(spec.ActiveSeconds, Math.Max(spec.FightDurationSeconds * spec.PlayerCount, 0.1)))}% average active time each."
        };
        foreach (SpecLaneSnapshot lane in laneSnapshots.Take(2))
        {
            evidence.Add($"{lane.Label}: {FormatOneDecimal(lane.SharePercent)}% squad share at {FormatOneDecimal(lane.PerSlotEfficiency)}x per-slot efficiency.");
        }
        return evidence;
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

    private static CombatReplayFightDemandDto BuildFightDemand(
        CombatReplayTeamAnalysisDto squadAnalysis,
        CombatReplayTeamAnalysisDto enemyAnalysis,
        CombatReplayEventAnalysisDto eventAnalysis,
        CombatReplayDefenseAnalysisDto defenseAnalysis,
        CombatReplayThreatBoonAnalysisDto threatAnalysis,
        IReadOnlyList<long> times)
    {
        int squadBurstWindows = BuildBurstWindows(squadAnalysis, times).Count;
        int enemyBurstWindows = BuildBurstWindows(enemyAnalysis, times).Count;
        int enemyDowns = eventAnalysis.Downs.Events.Count(evt => evt.IsEnemy);
        int enemyKills = eventAnalysis.Kills.Events.Count(evt => evt.IsEnemy);
        int enemyRecoveries = eventAnalysis.Recovered.Events.Count(evt => evt.IsEnemy && !evt.UsesSupportView);
        int squadDowns = eventAnalysis.Downs.Events.Count(evt => !evt.IsEnemy);
        int squadRecoveries = eventAnalysis.Recovered.Events.Count(evt => !evt.IsEnemy && evt.UsesSupportView);
        int ccImpactedEnemyDowns = eventAnalysis.Downs.Events.Count(evt => evt.IsEnemy && evt.CcImpacted);
        int stripSyncedBursts = squadBurstWindows;
        double burstIntensity = 100.0 * Math.Clamp((squadBurstWindows + enemyBurstWindows) / 6.0, 0.0, 1.0);
        double conversionContest = 100.0 * Math.Clamp((enemyDowns + enemyKills + enemyRecoveries) / 8.0, 0.0, 1.0);
        double boonCrackNeed = 100.0 * Math.Clamp((stripSyncedBursts / 6.0) * 0.65 + (enemyRecoveries / Math.Max((double)enemyDowns, 1.0)) * 0.35, 0.0, 1.0);
        double controlNeed = 100.0 * Math.Clamp((ccImpactedEnemyDowns / Math.Max((double)enemyDowns, 1.0)) * 0.70 + (eventAnalysis.Downs.EnemySummary.HardCcDowns / Math.Max((double)enemyDowns, 1.0)) * 0.30, 0.0, 1.0);
        double defensiveLoad = 100.0 * Math.Clamp((enemyBurstWindows / 6.0) * 0.45 + (squadDowns / 6.0) * 0.35 + (defenseAnalysis.BurstBarrier.LowHealthSurvivorOccurrences / 8.0) * 0.20, 0.0, 1.0);
        double rescueNeed = 100.0 * Math.Clamp((squadDowns / 6.0) * 0.55 + (squadRecoveries / 4.0) * 0.45, 0.0, 1.0);
        double threatenedBoonNeed = 100.0 * Math.Clamp(defensiveLoad / 100.0 * 0.70 + burstIntensity / 100.0 * 0.30, 0.0, 1.0);
        double conditionPressureNeed = 100.0 * Math.Clamp((eventAnalysis.Downs.SquadSummary.ConditionImpactedDowns / Math.Max((double)squadDowns, 1.0)) * 0.65 + (squadRecoveries / Math.Max((double)squadDowns, 1.0)) * 0.35, 0.0, 1.0);
        double enemyKillRate = GetPercent(enemyKills, enemyDowns);
        double enemyRecoveryRate = GetPercent(enemyRecoveries, enemyDowns);
        double enemyRecoveryDeniedRate = enemyDowns > 0 ? Math.Max(0.0, 100.0 - enemyRecoveryRate) : 0.0;
        double enemyDownShare = GetPercent(enemyDowns, enemyDowns + squadDowns);
        double squadBurstShare = GetPercent(squadBurstWindows, squadBurstWindows + enemyBurstWindows);
        double squadRecoveryRate = GetPercent(squadRecoveries, squadDowns);
        double squadRezActivityRate = squadRecoveries > 0
            ? GetPercent(Math.Min(eventAnalysis.Recovered.SquadSummary.TotalRezCasts, squadRecoveries), squadRecoveries)
            : 0.0;
        double enemyBurstHeldRate = GetPercent(defenseAnalysis.BurstBarrier.BurstWindowsHeld, enemyBurstWindows);
        double burstBarrierResponse = Math.Clamp(defenseAnalysis.BurstBarrier.BurstBarrierAbsorptionPercent / 30.0, 0.0, 1.0) * 100.0;
        double lowHealthSaveResponse = enemyBurstWindows > 0
            ? Math.Clamp(defenseAnalysis.BurstBarrier.LowHealthSurvivorOccurrences / (double)enemyBurstWindows, 0.0, 1.0) * 100.0
            : 0.0;
        double conversionResponse = enemyKills > 0 || enemyRecoveries > 0
            ? enemyKillRate * 0.75 + enemyRecoveryDeniedRate * 0.25
            : enemyDowns > 0 ? enemyRecoveryDeniedRate * 0.50 : 0.0;
        double boonSupportResponse = ComputeThreatBoonResponseScore(threatAnalysis);
        double pressureResponse = squadBurstShare * 0.35 + enemyDownShare * 0.35 + conversionResponse * 0.30;
        double stripResponse = (stripSyncedBursts > 0 ? 100.0 : 0.0) * 0.45 + conversionResponse * 0.35 + squadBurstShare * 0.20;
        double controlResponse = controlNeed;
        double recoveryResponse = squadRecoveryRate * 0.80 + enemyBurstHeldRate * 0.20;
        double preventionResponse = enemyBurstHeldRate * 0.55 + burstBarrierResponse * 0.25 + lowHealthSaveResponse * 0.20;
        double rezResponse = squadRecoveryRate * 0.70 + squadRezActivityRate * 0.30;

        var lanes = new List<CombatReplayFightDemandLaneDto>
        {
            BuildFightDemandLane("pressure", "Pressure",
                burstIntensity * 0.45 + conversionContest * 0.35 + boonCrackNeed * 0.20,
                pressureResponse,
                $"{enemyDowns} enemy downs and {squadBurstWindows} strong squad burst windows made live-target pressure matter.",
                $"{enemyDownShare:0.#}% enemy-down share and {conversionResponse:0.#}% conversion response."),
            BuildFightDemandLane("conversion", "Conversion",
                conversionContest * 0.55 + burstIntensity * 0.25 + boonCrackNeed * 0.20,
                conversionResponse,
                $"{enemyKills} enemy kills and {enemyRecoveries} enemy recoveries kept finishes contested.",
                $"{enemyKillRate:0.#}% enemy down-to-kill rate with {enemyRecoveries} enemy recoveries allowed."),
            BuildFightDemandLane("strip", "Strip",
                boonCrackNeed * 0.60 + conversionContest * 0.25 + burstIntensity * 0.15,
                stripResponse,
                $"{stripSyncedBursts} synced strip bursts and {enemyRecoveries} enemy recoveries increased boon-crack value.",
                $"{stripSyncedBursts} synced strip burst windows with {conversionResponse:0.#}% conversion response."),
            BuildFightDemandLane("control", "Control",
                controlNeed * 0.60 + conversionContest * 0.25 + burstIntensity * 0.15,
                controlResponse,
                $"{ccImpactedEnemyDowns} enemy downs were visibly CC-impacted.",
                $"{ccImpactedEnemyDowns} of {enemyDowns} enemy downs were CC-impacted."),
            BuildFightDemandLane("boonSupport", "Boon Support",
                defensiveLoad * 0.50 + burstIntensity * 0.25 + threatenedBoonNeed * 0.25,
                boonSupportResponse,
                $"{enemyBurstWindows} enemy burst windows raised the value of offensive and defensive boon coverage.",
                $"{boonSupportResponse:0.#}% weighted threatened support-boon coverage."),
            BuildFightDemandLane("recovery", "Recovery",
                defensiveLoad * 0.40 + rescueNeed * 0.20 + conditionPressureNeed * 0.40,
                recoveryResponse,
                $"{squadDowns} squad downs and {eventAnalysis.Downs.SquadSummary.ConditionImpactedDowns} condition-impacted squad downs raised post-hit recovery demand.",
                $"{squadRecoveries} of {squadDowns} squad downs recovered."),
            BuildFightDemandLane("prevention", "Prevention",
                defensiveLoad * 0.60 + threatenedBoonNeed * 0.20 + burstIntensity * 0.10 + rescueNeed * 0.10,
                preventionResponse,
                $"{enemyBurstWindows} enemy burst windows and {defenseAnalysis.BurstBarrier.LowHealthSurvivorOccurrences} low-health survive moments raised damage-prevention demand.",
                $"{enemyBurstHeldRate:0.#}% enemy burst windows held without a squad down."),
            BuildFightDemandLane("rez", "Rez",
                rescueNeed * 0.70 + defensiveLoad * 0.20 + conversionContest * 0.10,
                rezResponse,
                $"{squadRecoveries} squad recoveries made downstate rescue materially relevant.",
                $"{squadRecoveryRate:0.#}% squad recovery rate with {eventAnalysis.Recovered.SquadSummary.TotalRezCasts} rez casts."),
        };
        lanes = [.. lanes.OrderByDescending(lane => lane.DemandScorePercent).ThenBy(lane => lane.Label)];
        return new CombatReplayFightDemandDto
        {
            Summary = lanes.Count > 0
                ? $"Top demands: {string.Join(", ", lanes.Take(3).Select(lane => lane.Label))}."
                : "Fight demand was not strong enough to rank.",
            Lanes = lanes,
        };
    }

    private static CombatReplayFightDemandLaneDto BuildFightDemandLane(
        string key,
        string label,
        double demandScorePercent,
        double responseScorePercent,
        string evidenceLine,
        string responseLine)
    {
        demandScorePercent = Math.Clamp(Math.Round(demandScorePercent, 1), 0.0, 100.0);
        responseScorePercent = Math.Clamp(Math.Round(responseScorePercent, 1), 0.0, 100.0);
        string demandLabel = GetDemandLabel(demandScorePercent);
        string responseLabel = GetDemandResponseLabel(demandScorePercent, responseScorePercent);
        double weightMultiplier = demandLabel switch
        {
            "Very High" => 1.30,
            "High" => 1.15,
            "Moderate" => 1.00,
            _ => 0.85,
        };
        return new CombatReplayFightDemandLaneDto
        {
            Key = key,
            Label = label,
            DemandScorePercent = demandScorePercent,
            DemandLabel = demandLabel,
            ResponseScorePercent = responseScorePercent,
            ResponseLabel = responseLabel,
            ResponseTone = GetDemandResponseTone(responseLabel),
            ResponseLine = responseLine,
            WeightMultiplier = weightMultiplier,
            EvidenceLine = evidenceLine,
        };
    }

    private static double ComputeThreatBoonResponseScore(CombatReplayThreatBoonAnalysisDto threatAnalysis)
    {
        if (threatAnalysis.Boons.Count == 0)
        {
            return 0.0;
        }

        var weights = new Dictionary<long, double>
        {
            [Stability] = 0.30,
            [Protection] = 0.20,
            [Resolution] = 0.15,
            [Resistance] = 0.15,
            [Aegis] = 0.10,
            [Quickness] = 0.10,
        };
        double weightedCoverage = 0.0;
        double totalWeight = 0.0;
        foreach (CombatReplayThreatBoonTimelineDto boon in threatAnalysis.Boons)
        {
            if (!weights.TryGetValue(boon.Id, out double weight))
            {
                continue;
            }

            weightedCoverage += boon.SummaryCoverage * weight;
            totalWeight += weight;
        }

        return totalWeight > 0.0 ? weightedCoverage / totalWeight : 0.0;
    }

    private static double GetPercent(double numerator, double denominator)
    {
        return denominator > 0.0
            ? Math.Clamp(numerator * 100.0 / denominator, 0.0, 100.0)
            : 0.0;
    }

    private static string GetDemandResponseLabel(double demandScorePercent, double responseScorePercent)
    {
        if (demandScorePercent < 30.0)
        {
            return "Low Signal";
        }
        double gap = demandScorePercent - responseScorePercent;
        return gap switch
        {
            <= 10.0 => "Met",
            <= 30.0 => "Contested",
            _ => "Gap",
        };
    }

    private static string GetDemandResponseTone(string responseLabel)
    {
        return responseLabel switch
        {
            "Met" => "met",
            "Contested" => "contested",
            "Gap" => "gap",
            _ => "neutral",
        };
    }

    private static CombatReplayPlayerLaneMetricDto BuildLaneMetric(string key, string label, double value, string unit, string aggregation = "sum")
    {
        return new CombatReplayPlayerLaneMetricDto
        {
            Key = key,
            Label = label,
            Value = Math.Round(value, 1),
            Unit = unit,
            Aggregation = aggregation,
        };
    }

    private static CombatReplayPlayerEvaluationMaximums BuildPlayerEvaluationMaximums(IReadOnlyList<CombatReplayPlayerEvaluationAggregate> aggregates)
    {
        if (aggregates.Count == 0)
        {
            return new CombatReplayPlayerEvaluationMaximums();
        }

        return new CombatReplayPlayerEvaluationMaximums
        {
            DamageTotal = aggregates.Max(aggregate => aggregate.DamageTotal),
            LiveTargetDamage = aggregates.Max(aggregate => aggregate.LiveTargetDamage),
            AgainstDownedDamage = aggregates.Max(aggregate => aggregate.AgainstDownedDamage),
            DownContribution = aggregates.Max(aggregate => aggregate.DownContribution),
            EnemyDownContributionDamage = aggregates.Max(aggregate => aggregate.EnemyDownContributionDamage),
            EnemyKillContributionDamage = aggregates.Max(aggregate => aggregate.EnemyKillContributionDamage),
            AverageTopTargetContribution = aggregates.Max(aggregate => aggregate.AverageTopTargetContribution),
            OffensiveConditionPressure = aggregates.Max(aggregate => aggregate.OffensiveConditionPressure),
            ControlConditionPressure = aggregates.Max(aggregate => aggregate.ControlConditionPressure),
            StripsTotal = aggregates.Max(aggregate => aggregate.StripsTotal),
            StripDownContribution = aggregates.Max(aggregate => aggregate.StripDownContribution),
            HealingTotal = aggregates.Max(aggregate => aggregate.HealingTotal),
            BarrierTotal = aggregates.Max(aggregate => aggregate.BarrierTotal),
            PetMinionAbsorptionTotal = aggregates.Max(aggregate => aggregate.PetMinionAbsorptionTotal),
            AttributedNegatedDamageTotal = aggregates.Max(aggregate => aggregate.AttributedNegatedDamageTotal),
            CleansesTotal = aggregates.Max(aggregate => aggregate.CleansesTotal),
            ResurrectsTotal = aggregates.Max(aggregate => aggregate.ResurrectsTotal),
            TotalBoonSupport = aggregates.Max(aggregate => aggregate.OffensiveBoonSupport + aggregate.DefensiveBoonSupport),
            OffensiveBoonSupport = aggregates.Max(aggregate => aggregate.OffensiveBoonSupport),
            DefensiveBoonSupport = aggregates.Max(aggregate => aggregate.DefensiveBoonSupport),
            DefensiveConditionPressure = aggregates.Max(aggregate => aggregate.DefensiveConditionPressure),
            EffectiveCrowdControlCount = aggregates.Max(aggregate => aggregate.EffectiveCrowdControlCount),
            EffectiveCrowdControlDuration = aggregates.Max(aggregate => aggregate.EffectiveCrowdControlDuration),
            CrowdControlDownContribution = aggregates.Max(aggregate => aggregate.CrowdControlDownContribution),
            BurstContributionWindows = aggregates.Max(aggregate => aggregate.BurstContributionWindows),
            ConversionContributionWindows = aggregates.Max(aggregate => aggregate.ConversionContributionWindows),
            ControlContributionWindows = aggregates.Max(aggregate => aggregate.ControlContributionWindows),
            RecoveryContributionWindows = aggregates.Max(aggregate => aggregate.RecoveryContributionWindows),
            RecoveryWindowsTotal = aggregates.Max(aggregate => aggregate.RecoveryWindowsTotal),
            DefensiveSupportWindows = aggregates.Max(aggregate => aggregate.DefensiveSupportWindows),
            OffensiveBoonWindows = aggregates.Max(aggregate => aggregate.OffensiveBoonWindows),
            DefensiveBoonWindows = aggregates.Max(aggregate => aggregate.DefensiveBoonWindows),
            BoonContributionWindows = aggregates.Max(aggregate => aggregate.BoonContributionWindows),
            SquadRecoveryWindowsHelped = aggregates.Max(aggregate => aggregate.SquadRecoveryWindowsHelped),
            DownedHealingOnRecoveries = aggregates.Max(aggregate => aggregate.DownedHealingOnRecoveries),
            RezTimeOnRecoveries = aggregates.Max(aggregate => aggregate.RezTimeOnRecoveries),
            KeyWindowsHit = aggregates.Max(aggregate => aggregate.KeyWindowsHit),
        };
    }

    private static CombatReplayPlayerEvaluationTotals BuildPlayerEvaluationTotals(
        IReadOnlyList<CombatReplayPlayerEvaluationAggregate> aggregates,
        bool hasHealingData,
        bool hasBarrierData)
    {
        return new CombatReplayPlayerEvaluationTotals
        {
            PressureContribution = aggregates.Sum(aggregate => aggregate.EnemyDownContributionDamage),
            LiveTargetDamage = aggregates.Sum(aggregate => aggregate.LiveTargetDamage),
            ConversionContribution = aggregates.Sum(aggregate => aggregate.EnemyKillContributionDamage),
            AgainstDownedDamage = aggregates.Sum(aggregate => aggregate.AgainstDownedDamage),
            StripDownContribution = aggregates.Sum(aggregate => aggregate.StripDownContribution),
            StripsTotal = aggregates.Sum(aggregate => aggregate.StripsTotal),
            CrowdControlDownContribution = aggregates.Sum(aggregate => aggregate.CrowdControlDownContribution),
            EffectiveCrowdControlCount = aggregates.Sum(aggregate => aggregate.EffectiveCrowdControlCount),
            TotalBoonSupport = Math.Round(aggregates.Sum(aggregate => aggregate.OffensiveBoonSupport + aggregate.DefensiveBoonSupport), 1),
            HealingTotal = hasHealingData ? aggregates.Sum(aggregate => aggregate.HealingTotal) : 0,
            BarrierTotal = hasBarrierData ? aggregates.Sum(aggregate => aggregate.BarrierTotal) : 0,
            PetMinionAbsorptionTotal = aggregates.Sum(aggregate => aggregate.PetMinionAbsorptionTotal),
            AttributedNegatedDamageTotal = Math.Round(aggregates.Sum(aggregate => aggregate.AttributedNegatedDamageTotal), 1),
            CleansesTotal = aggregates.Sum(aggregate => aggregate.CleansesTotal),
            RecoveryContributionWindows = aggregates.Sum(aggregate => aggregate.RecoveryContributionWindows),
            DefensiveConditionPressure = Math.Round(aggregates.Sum(aggregate => aggregate.DefensiveConditionPressure), 1),
            DefensiveSupportWindows = aggregates.Sum(aggregate => aggregate.DefensiveSupportWindows),
            SquadRecoveryWindowsHelped = aggregates.Sum(aggregate => aggregate.SquadRecoveryWindowsHelped),
            RezTimeOnRecoveries = Math.Round(aggregates.Sum(aggregate => aggregate.RezTimeOnRecoveries), 1),
        };
    }

    private static Dictionary<int, PlayerEventContributionSummary> BuildPlayerEventContributionSummaries<TEvent>(
        IReadOnlyList<TEvent> events,
        Func<TEvent, IReadOnlyList<CombatReplayEventContributionDto>> contributorSelector,
        Func<CombatReplayEventContributionDto, double> amountSelector,
        Func<TEvent, bool>? fastEventPredicate = null)
    {
        var totals = new Dictionary<int, (double Amount, int WindowsHit, int FastWindowsHit)>();
        foreach (TEvent evt in events)
        {
            var seenContributors = new HashSet<int>();
            bool isFastEvent = fastEventPredicate?.Invoke(evt) ?? false;
            foreach (CombatReplayEventContributionDto contributor in contributorSelector(evt))
            {
                if (!contributor.ActorId.HasValue)
                {
                    continue;
                }
                double amount = amountSelector(contributor);
                if (amount <= 0.0)
                {
                    continue;
                }

                int actorId = contributor.ActorId.Value;
                totals.TryGetValue(actorId, out var current);
                current.Amount += amount;
                if (seenContributors.Add(actorId))
                {
                    current.WindowsHit++;
                    if (isFastEvent)
                    {
                        current.FastWindowsHit++;
                    }
                }
                totals[actorId] = current;
            }
        }

        return totals.ToDictionary(
            pair => pair.Key,
            pair => new PlayerEventContributionSummary(
                Math.Round(pair.Value.Amount, 1),
                pair.Value.WindowsHit,
                events.Count,
                pair.Value.FastWindowsHit));
    }

    private static Dictionary<int, PlayerRecoveryContributionSummary> BuildPlayerRecoveryContributionSummaries(IReadOnlyList<CombatReplayRecoveredEventDto> events)
    {
        var totals = new Dictionary<int, (int WindowsHit, double DownedHealing, double RezCasts, double RezTime)>();
        foreach (CombatReplayRecoveredEventDto evt in events)
        {
            var seenContributors = new HashSet<int>();
            foreach (CombatReplayEventContributionDto contributor in evt.SupportContributors)
            {
                if (!contributor.ActorId.HasValue)
                {
                    continue;
                }

                double downedHealing = GetSupportDetailAmount(contributor, "Downed healing");
                double rezCasts = GetSupportDetailAmount(contributor, "Rez casts");
                double rezTime = GetSupportDetailAmount(contributor, "Rez time");
                if (downedHealing <= 0.0 && rezCasts <= 0.0 && rezTime <= 0.0)
                {
                    continue;
                }

                int actorId = contributor.ActorId.Value;
                totals.TryGetValue(actorId, out var current);
                current.DownedHealing += downedHealing;
                current.RezCasts += rezCasts;
                current.RezTime += rezTime;
                if (seenContributors.Add(actorId))
                {
                    current.WindowsHit++;
                }
                totals[actorId] = current;
            }
        }

        return totals.ToDictionary(
            pair => pair.Key,
            pair => new PlayerRecoveryContributionSummary(
                pair.Value.WindowsHit,
                events.Count,
                Math.Round(pair.Value.DownedHealing, 1),
                Math.Round(pair.Value.RezCasts, 1),
                Math.Round(pair.Value.RezTime, 1)));
    }

    private static int BuildPlayerRecoveryCount(ParsedEvtcLog log, SingleActor player)
    {
        return log.CombatData.GetDownEvents(player.AgentItem)
            .Count(downEvent => string.Equals(GetDownOutcomeInfo(log, player.AgentItem, downEvent.Time).Outcome, "Recovered", StringComparison.OrdinalIgnoreCase));
    }

    private static Dictionary<EvaluationWindow, double> ComputeBoonSupportContributionByWindow(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> boonIds)
    {
        var result = new Dictionary<EvaluationWindow, double>();
        foreach (EvaluationWindow window in windows)
        {
            double windowTotal = 0.0;
            foreach (SingleActor recipient in recipients)
            {
                if (recipient.UniqueID == provider.UniqueID)
                {
                    continue;
                }

                foreach (long boonId in boonIds)
                {
                    if (!log.Buffs.BuffsByIDs.ContainsKey(boonId))
                    {
                        continue;
                    }

                    foreach (AbstractBuffApplyEvent applyEvent in recipient.GetBuffApplyEventsOnByID(log, window.Start, window.End, boonId, provider))
                    {
                        switch (applyEvent)
                        {
                            case BuffApplyEvent buffApplyEvent when buffApplyEvent.AppliedDuration < int.MaxValue:
                                windowTotal += buffApplyEvent.AppliedDuration / 1000.0;
                                break;
                            case BuffExtensionEvent buffExtensionEvent:
                                windowTotal += buffExtensionEvent.ExtendedDuration / 1000.0;
                                break;
                        }
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

    private static Dictionary<long, double> ComputeBoonSupportContributionByBuff(
        ParsedEvtcLog log,
        SingleActor provider,
        IReadOnlyList<SingleActor> recipients,
        IReadOnlyList<EvaluationWindow> windows,
        IReadOnlyList<long> boonIds)
    {
        var result = boonIds.ToDictionary(boonId => boonId, _ => 0.0);
        foreach (EvaluationWindow window in windows)
        {
            foreach (SingleActor recipient in recipients)
            {
                if (recipient.UniqueID == provider.UniqueID)
                {
                    continue;
                }

                foreach (long boonId in boonIds)
                {
                    if (!log.Buffs.BuffsByIDs.ContainsKey(boonId))
                    {
                        continue;
                    }

                    foreach (AbstractBuffApplyEvent applyEvent in recipient.GetBuffApplyEventsOnByID(log, window.Start, window.End, boonId, provider))
                    {
                        switch (applyEvent)
                        {
                            case BuffApplyEvent buffApplyEvent when buffApplyEvent.AppliedDuration < int.MaxValue:
                                result[boonId] += buffApplyEvent.AppliedDuration / 1000.0;
                                break;
                            case BuffExtensionEvent buffExtensionEvent:
                                result[boonId] += buffExtensionEvent.ExtendedDuration / 1000.0;
                                break;
                        }
                    }
                }
            }
        }

        return result
            .Where(pair => pair.Value > 0.0)
            .ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 1));
    }

    private static Dictionary<long, double> MergeContributionDictionaries(IEnumerable<IReadOnlyDictionary<long, double>> dictionaries)
    {
        var result = new Dictionary<long, double>();
        foreach (IReadOnlyDictionary<long, double> dictionary in dictionaries)
        {
            foreach (KeyValuePair<long, double> pair in dictionary)
            {
                result[pair.Key] = result.TryGetValue(pair.Key, out double existing)
                    ? Math.Round(existing + pair.Value, 1)
                    : Math.Round(pair.Value, 1);
            }
        }
        return result;
    }

    private static Dictionary<string, double> MergeContributionDictionaries(IEnumerable<IReadOnlyDictionary<string, double>> dictionaries)
    {
        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (IReadOnlyDictionary<string, double> dictionary in dictionaries)
        {
            foreach (KeyValuePair<string, double> pair in dictionary)
            {
                result[pair.Key] = result.TryGetValue(pair.Key, out double existing)
                    ? Math.Round(existing + pair.Value, 1)
                    : Math.Round(pair.Value, 1);
            }
        }
        return result;
    }

    private static List<CombatReplayPlayerEvaluationDetailSectionDto> BuildBoonSupportDetailSections(
        ParsedEvtcLog log,
        CombatReplayPlayerEvaluationAggregate aggregate)
    {
        var sections = new List<CombatReplayPlayerEvaluationDetailSectionDto>();
        if (aggregate.OffensiveBoonSupportByBuff.Count > 0)
        {
            sections.Add(BuildDetailSection(
                "Offensive Boon Breakdown",
                BuildBoonSupportDetailEntries(log, aggregate.OffensiveBoonSupportByBuff)));
        }
        if (aggregate.DefensiveBoonSupportByBuff.Count > 0)
        {
            sections.Add(BuildDetailSection(
                "Defensive Boon Breakdown",
                BuildBoonSupportDetailEntries(log, aggregate.DefensiveBoonSupportByBuff)));
        }
        return sections;
    }

    private static IEnumerable<CombatReplayPlayerEvaluationDetailEntryDto> BuildBoonSupportDetailEntries(
        ParsedEvtcLog log,
        IReadOnlyDictionary<long, double> boonSupportByBuff)
    {
        return boonSupportByBuff
            .Where(pair => log.Buffs.BuffsByIDs.ContainsKey(pair.Key))
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => log.Buffs.BuffsByIDs[pair.Key].Name, StringComparer.OrdinalIgnoreCase)
            .Select(pair =>
            {
                Buff buff = log.Buffs.BuffsByIDs[pair.Key];
                string unit = buff.Type == Buff.BuffType.Intensity ? "stack-seconds" : "seconds";
                return BuildDetailEntry(buff.Name, FormatOneDecimal(pair.Value), unit);
            });
    }

    private static IEnumerable<CombatReplayPlayerEvaluationDetailEntryDto> BuildNamedContributionDetailEntries(
        IReadOnlyDictionary<string, double> valuesByName,
        string secondary)
    {
        return valuesByName
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => BuildDetailEntry(pair.Key, FormatOneDecimal(pair.Value), secondary));
    }

    private static int CountKeyContributionWindows(
        IReadOnlyList<EvaluationWindow> keyWindows,
        IReadOnlyList<long> times,
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<CrowdControlEvent> effectiveCrowdControlEvents,
        IReadOnlyDictionary<EvaluationWindow, double> offensiveConditionContribution,
        IReadOnlyDictionary<EvaluationWindow, double> controlConditionContribution,
        IReadOnlyDictionary<EvaluationWindow, double> defensiveConditionContribution,
        IReadOnlyDictionary<EvaluationWindow, double> offensiveBoonContribution,
        IReadOnlyDictionary<EvaluationWindow, double> defensiveBoonContribution)
    {
        return keyWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Damage, attackerTimeline?.Healing, attackerTimeline?.Barrier, attackerTimeline?.Cleanses, attackerTimeline?.Strips) ||
            effectiveCrowdControlEvents.Any(crowdControlEvent => crowdControlEvent.Time >= window.Start && crowdControlEvent.Time <= window.End) ||
            offensiveConditionContribution.ContainsKey(window) ||
            controlConditionContribution.ContainsKey(window) ||
            defensiveConditionContribution.ContainsKey(window) ||
            offensiveBoonContribution.ContainsKey(window) ||
            defensiveBoonContribution.ContainsKey(window));
    }

    private static PlayerLaneSnapshot BuildPressureLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(aggregate.LiveTargetDamage, maximums.LiveTargetDamage), 0.31),
            (NormalizeValue(aggregate.EnemyDownContributionDamage, maximums.EnemyDownContributionDamage), maximums.EnemyDownContributionDamage > 0.0 ? 0.31 : 0.0),
            (NormalizeValue(aggregate.DownContribution, maximums.DownContribution), maximums.DownContribution > 0 ? 0.14 : 0.0),
            (NormalizeValue(aggregate.AverageTopTargetContribution, maximums.AverageTopTargetContribution), 0.14),
            (NormalizeValue(aggregate.OffensiveConditionPressure, maximums.OffensiveConditionPressure), maximums.OffensiveConditionPressure > 0.0 ? 0.10 : 0.0));
        double sharePercent = totals.PressureContribution > 0.0
            ? aggregate.EnemyDownContributionDamage * 100.0 / totals.PressureContribution
            : ComputePercent(aggregate.LiveTargetDamage, totals.LiveTargetDamage);
        int windowsTotal = aggregate.EnemyDownWindowsTotal > 0 ? aggregate.EnemyDownWindowsTotal : aggregate.BurstWindowsTotal;
        int windowsHit = aggregate.EnemyDownWindowsTotal > 0 ? aggregate.EnemyDownWindowsHit : aggregate.BurstContributionWindows;
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Pressure Metrics",
            [
                BuildDetailEntry("Live-target damage", FormatWholeNumber(aggregate.LiveTargetDamage), $"{FormatWholeNumber(aggregate.DamageTotal)} total damage to enemy players"),
                BuildDetailEntry("Pre-down contribution", FormatWholeNumber((long)Math.Round(aggregate.EnemyDownContributionDamage)), BuildPluralizedLabel(aggregate.EnemyDownWindowsHit, "down window", "down windows")),
                BuildDetailEntry("Down contribution", FormatWholeNumber(aggregate.DownContribution), ""),
                BuildDetailEntry("Focus contribution", $"{FormatOneDecimal(aggregate.AverageTopTargetContribution)}%", ""),
                BuildDetailEntry("Offensive condition pressure", FormatWholeNumber((long)Math.Round(aggregate.OffensiveConditionPressure)), "")
            ])
        ];
        return new PlayerLaneSnapshot(
            "pressure",
            "Pressure",
            strengthPercent,
            sharePercent,
            windowsHit,
            windowsTotal,
            aggregate.EnemyDownWindowsTotal > 0 ? "down windows" : "burst windows",
            GetRateBand(strengthPercent),
            $"{FormatWholeNumber(aggregate.LiveTargetDamage)} live-target damage and {FormatWholeNumber((long)Math.Round(aggregate.EnemyDownContributionDamage))} pre-down pressure showed up before enemies fell.",
            true,
            "Pressure Detail",
            "Pressure highlights live-target damage and visible pre-down contribution before enemy downs landed.",
            detailSections,
            [
                BuildLaneMetric("liveTargetDamage", "Live-target damage", aggregate.LiveTargetDamage, "damage"),
                BuildLaneMetric("preDownDamage", "Pre-down contribution", aggregate.EnemyDownContributionDamage, "damage"),
                BuildLaneMetric("downContribution", "Down contribution", aggregate.DownContribution, "count")
            ]);
    }

    private static PlayerLaneSnapshot BuildConversionLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(aggregate.EnemyKillContributionDamage, maximums.EnemyKillContributionDamage), maximums.EnemyKillContributionDamage > 0.0 ? 0.46 : 0.0),
            (NormalizeValue(aggregate.AgainstDownedDamage, maximums.AgainstDownedDamage), maximums.AgainstDownedDamage > 0 ? 0.26 : 0.0),
            (NormalizeValue(aggregate.FastEnemyKillWindowsHit, Math.Max(1, maximums.EnemyKillContributionDamage > 0.0 ? aggregate.EnemyKillWindowsTotal : 1)), 0.12),
            (NormalizeValue(aggregate.EnemyKillWindowsHit, Math.Max(1, aggregate.EnemyKillWindowsTotal)), 0.16));
        double sharePercent = totals.ConversionContribution > 0.0
            ? aggregate.EnemyKillContributionDamage * 100.0 / totals.ConversionContribution
            : ComputePercent(aggregate.AgainstDownedDamage, totals.AgainstDownedDamage);
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Conversion Metrics",
            [
                BuildDetailEntry("Finish contribution", FormatWholeNumber((long)Math.Round(aggregate.EnemyKillContributionDamage)), BuildPluralizedLabel(aggregate.EnemyKillWindowsHit, "finish window", "finish windows")),
                BuildDetailEntry("Against-downed damage", FormatWholeNumber(aggregate.AgainstDownedDamage), ""),
                BuildDetailEntry("Fast conversions helped", BuildPluralizedLabel(aggregate.FastEnemyKillWindowsHit, "fast finish", "fast finishes"), ""),
                BuildDetailEntry("Conversion windows", BuildPluralizedLabel(aggregate.EnemyKillWindowsHit, "finish window", "finish windows"), $"{aggregate.EnemyKillWindowsTotal} total")
            ])
        ];
        return new PlayerLaneSnapshot(
            "conversion",
            "Conversion",
            strengthPercent,
            sharePercent,
            aggregate.EnemyKillWindowsHit,
            aggregate.EnemyKillWindowsTotal,
            "finish windows",
            GetRateBand(strengthPercent),
            $"{FormatWholeNumber((long)Math.Round(aggregate.EnemyKillContributionDamage))} finish contribution and {FormatWholeNumber(aggregate.AgainstDownedDamage)} against-downed damage helped secure kills.",
            true,
            "Conversion Detail",
            "Conversion tracks visible contribution after enemy downs, especially in successful finish windows.",
            detailSections,
            [
                BuildLaneMetric("finishContributionDamage", "Finish contribution", aggregate.EnemyKillContributionDamage, "damage"),
                BuildLaneMetric("againstDownedDamage", "Against-downed damage", aggregate.AgainstDownedDamage, "damage"),
                BuildLaneMetric("fastFinishWindowsHit", "Fast finishes helped", aggregate.FastEnemyKillWindowsHit, "count")
            ]);
    }

    private static PlayerLaneSnapshot BuildStripLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(aggregate.StripDownContribution, maximums.StripDownContribution), maximums.StripDownContribution > 0 ? 0.44 : 0.0),
            (NormalizeValue(aggregate.StripsTotal, maximums.StripsTotal), 0.34),
            (NormalizeValue(aggregate.ControlContributionWindows, maximums.ControlContributionWindows), 0.12),
            (NormalizeValue(aggregate.StripDownContributionTime, Math.Max(aggregate.StripDownContributionTime, 0.1)), aggregate.StripDownContributionTime > 0.0 ? 0.10 : 0.0));
        double sharePercent = totals.StripDownContribution > 0
            ? aggregate.StripDownContribution * 100.0 / totals.StripDownContribution
            : ComputePercent(aggregate.StripsTotal, totals.StripsTotal);
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Strip Metrics",
            [
                BuildDetailEntry("Timed strips", FormatWholeNumber(aggregate.StripsTotal), $"{FormatOneDecimal(aggregate.StripDownContributionTime)}s down-linked strip time"),
                BuildDetailEntry("Down-linked strips", FormatWholeNumber(aggregate.StripDownContribution), ""),
                BuildDetailEntry("Strip windows", BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows"), $"{aggregate.ControlWindowsTotal} total")
            ])
        ];
        return new PlayerLaneSnapshot(
            "strip",
            "Strip",
            strengthPercent,
            sharePercent,
            aggregate.ControlContributionWindows,
            aggregate.ControlWindowsTotal,
            "strip windows",
            GetRateBand(strengthPercent),
            $"{FormatWholeNumber(aggregate.StripsTotal)} strips and {FormatWholeNumber(aggregate.StripDownContribution)} down-linked strips cracked enemy boons in key exchanges.",
            true,
            "Strip Detail",
            "Strip highlights enemy boon removal, with extra weight on strips that fed downs.",
            detailSections,
            [
                BuildLaneMetric("stripsTotal", "Strips", aggregate.StripsTotal, "count"),
                BuildLaneMetric("stripDownContribution", "Down-linked strips", aggregate.StripDownContribution, "count"),
                BuildLaneMetric("stripDownContributionTime", "Down-linked strip time", aggregate.StripDownContributionTime, "seconds")
            ]);
    }

    private static PlayerLaneSnapshot BuildControlLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(aggregate.CrowdControlDownContribution, maximums.CrowdControlDownContribution), maximums.CrowdControlDownContribution > 0 ? 0.25 : 0.0),
            (NormalizeValue(aggregate.EffectiveCrowdControlCount, maximums.EffectiveCrowdControlCount), 0.20),
            (NormalizeValue(aggregate.EffectiveCrowdControlDuration, maximums.EffectiveCrowdControlDuration), maximums.EffectiveCrowdControlDuration > 0.0 ? 0.15 : 0.0),
            (NormalizeValue(aggregate.ControlConditionPressure, maximums.ControlConditionPressure), maximums.ControlConditionPressure > 0.0 ? 0.30 : 0.0),
            (NormalizeValue(aggregate.ControlContributionWindows, maximums.ControlContributionWindows), 0.10));
        double sharePercent = totals.CrowdControlDownContribution > 0
            ? aggregate.CrowdControlDownContribution * 100.0 / totals.CrowdControlDownContribution
            : ComputePercent(aggregate.EffectiveCrowdControlCount, totals.EffectiveCrowdControlCount);
        var detailSections = new List<CombatReplayPlayerEvaluationDetailSectionDto>
        {
            BuildDetailSection("Control Metrics",
            [
                BuildDetailEntry("Effective CC", BuildPluralizedLabel(aggregate.EffectiveCrowdControlCount, "effective CC event", "effective CC events"), $"{FormatOneDecimal(aggregate.EffectiveCrowdControlDuration)}s total control"),
                BuildDetailEntry("CC-linked downs", FormatWholeNumber(aggregate.CrowdControlDownContribution), $"{FormatOneDecimal(aggregate.CrowdControlDurationDownContribution)}s linked duration"),
                BuildDetailEntry("Control-condition pressure", FormatWholeNumber((long)Math.Round(aggregate.ControlConditionPressure)), ""),
                BuildDetailEntry("Control windows", BuildPluralizedLabel(aggregate.ControlContributionWindows, "control window", "control windows"), $"{aggregate.ControlWindowsTotal} total")
            ])
        };
        detailSections.AddRange(BuildControlTimingDetailSections(aggregate));
        return new PlayerLaneSnapshot(
            "control",
            "Control",
            strengthPercent,
            sharePercent,
            aggregate.ControlContributionWindows,
            aggregate.ControlWindowsTotal,
            "control windows",
            GetRateBand(strengthPercent),
            $"{FormatWholeNumber(aggregate.EffectiveCrowdControlCount)} effective CC events and {FormatWholeNumber(aggregate.CrowdControlDownContribution)} CC-linked downs visibly disrupted enemy play.",
            true,
            "Control Detail",
            "Control captures effective crowd control, control conditions, and visible CC-linked downs.",
            detailSections,
            [
                BuildLaneMetric("effectiveCrowdControlCount", "Effective CC", aggregate.EffectiveCrowdControlCount, "count"),
                BuildLaneMetric("effectiveCrowdControlDuration", "Effective CC duration", aggregate.EffectiveCrowdControlDuration, "seconds"),
                BuildLaneMetric("crowdControlDownContribution", "CC-linked downs", aggregate.CrowdControlDownContribution, "count")
            ]);
    }

    private static PlayerLaneSnapshot BuildBoonSupportLaneSnapshot(
        ParsedEvtcLog log,
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double totalBoonSupport = aggregate.OffensiveBoonSupport + aggregate.DefensiveBoonSupport;
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(totalBoonSupport, maximums.TotalBoonSupport), maximums.TotalBoonSupport > 0.0 ? 0.36 : 0.0),
            (NormalizeValue(aggregate.OffensiveBoonSupport, maximums.OffensiveBoonSupport), maximums.OffensiveBoonSupport > 0.0 ? 0.22 : 0.0),
            (NormalizeValue(aggregate.DefensiveBoonSupport, maximums.DefensiveBoonSupport), maximums.DefensiveBoonSupport > 0.0 ? 0.30 : 0.0),
            (NormalizeValue(aggregate.BoonContributionWindows, maximums.BoonContributionWindows), 0.12));
        double sharePercent = totals.TotalBoonSupport > 0.0
            ? totalBoonSupport * 100.0 / totals.TotalBoonSupport
            : 0.0;
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Boon Support Metrics",
            [
                BuildDetailEntry("Total boon-seconds", FormatWholeNumber((long)Math.Round(totalBoonSupport)), "stack boons stay labeled as stack-seconds in the boon breakdown"),
                BuildDetailEntry("Offensive boon-seconds", FormatWholeNumber((long)Math.Round(aggregate.OffensiveBoonSupport)), BuildPluralizedLabel(aggregate.OffensiveBoonWindows, "offensive boon window", "offensive boon windows")),
                BuildDetailEntry("Defensive boon-seconds", FormatWholeNumber((long)Math.Round(aggregate.DefensiveBoonSupport)), BuildPluralizedLabel(aggregate.DefensiveBoonWindows, "defensive boon window", "defensive boon windows"))
            ])
        ];
        detailSections.AddRange(BuildBoonSupportDetailSections(log, aggregate));
        string boonLean = aggregate.DefensiveBoonSupport >= aggregate.OffensiveBoonSupport ? "defensive" : "offensive";
        return new PlayerLaneSnapshot(
            "boonSupport",
            "Boon Support",
            strengthPercent,
            sharePercent,
            aggregate.BoonContributionWindows,
            aggregate.BoonWindowsTotal,
            "boon windows",
            GetRateBand(strengthPercent),
            $"{boonLean.Substring(0, 1).ToUpperInvariant()}{boonLean[1..]} boon coverage was most visible, with {FormatWholeNumber((long)Math.Round(totalBoonSupport))} total boon-seconds in key windows.",
            true,
            "Boon Support Detail",
            "Boon Support tracks offensive and defensive boon-seconds in the fight's key windows. Stack boons stay labeled as stack-seconds in the boon breakdown.",
            detailSections,
            [
                BuildLaneMetric("totalBoonSupport", "Total boon-seconds", totalBoonSupport, "boonSeconds"),
                BuildLaneMetric("offensiveBoonSupport", "Offensive boon-seconds", aggregate.OffensiveBoonSupport, "boonSeconds"),
                BuildLaneMetric("defensiveBoonSupport", "Defensive boon-seconds", aggregate.DefensiveBoonSupport, "boonSeconds")
            ]);
    }

    private static PlayerLaneSnapshot BuildRecoveryLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        bool hasHealingData,
        bool hasBarrierData)
    {
        double strengthPercent = ComputeWeightedScore(
            (hasHealingData ? NormalizeValue(aggregate.HealingTotal, maximums.HealingTotal) : 0.0, hasHealingData ? 0.50 : 0.0),
            (NormalizeValue(aggregate.CleansesTotal, maximums.CleansesTotal), 0.30),
            (NormalizeValue(aggregate.RecoveryContributionWindows, maximums.RecoveryContributionWindows), maximums.RecoveryContributionWindows > 0 ? 0.20 : 0.0));
        double sharePercent = ComputeRecoverySharePercent(aggregate, totals, hasHealingData, hasBarrierData);
        string healingValue = hasHealingData ? FormatWholeNumber(aggregate.HealingTotal) : "Unavailable";
        string healingSecondary = hasHealingData ? "" : "Missing healing extension data";
        string negationValue = aggregate.AttributedNegatedDamageTotal > 0.0 ? FormatOneDecimal(aggregate.AttributedNegatedDamageTotal) : "0";
        var recoveryEvidenceParts = new List<string>
        {
            $"{FormatWholeNumber(aggregate.CleansesTotal)} cleanses",
        };
        if (hasHealingData)
        {
            recoveryEvidenceParts.Add($"{FormatWholeNumber(aggregate.HealingTotal)} healing");
        }
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Recovery Metrics",
            [
                BuildDetailEntry("Cleanses", FormatWholeNumber(aggregate.CleansesTotal), ""),
                BuildDetailEntry("Healing", healingValue, healingSecondary),
                BuildDetailEntry("Response windows", BuildPluralizedLabel(aggregate.RecoveryContributionWindows, "response window", "response windows"), $"{aggregate.RecoveryWindowsTotal} total")
            ])
        ];
        return new PlayerLaneSnapshot(
            "recovery",
            "Recovery",
            strengthPercent,
            sharePercent,
            aggregate.RecoveryContributionWindows,
            aggregate.RecoveryWindowsTotal,
            "response windows",
            GetRateBand(strengthPercent),
            $"{string.Join(", ", recoveryEvidenceParts)} helped the squad stabilize under pressure.",
            true,
            "Recovery Detail",
            "Recovery captures healing, cleansing, and presence in defensive response windows after pressure landed.",
            detailSections,
            [
                BuildLaneMetric("cleansesTotal", "Cleanses", aggregate.CleansesTotal, "count"),
                BuildLaneMetric("healingTotal", "Healing", aggregate.HealingTotal, "healing")
            ]);
    }

    private static PlayerLaneSnapshot BuildPreventionLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals,
        bool hasBarrierData)
    {
        double strengthPercent = ComputeWeightedScore(
            (hasBarrierData ? NormalizeValue(aggregate.BarrierTotal, maximums.BarrierTotal) : 0.0, hasBarrierData ? 0.30 : 0.0),
            (NormalizeValue(aggregate.AttributedNegatedDamageTotal, maximums.AttributedNegatedDamageTotal), maximums.AttributedNegatedDamageTotal > 0.0 ? 0.30 : 0.0),
            (NormalizeValue(aggregate.PetMinionAbsorptionTotal, maximums.PetMinionAbsorptionTotal), maximums.PetMinionAbsorptionTotal > 0 ? 0.10 : 0.0),
            (NormalizeValue(aggregate.DefensiveConditionPressure, maximums.DefensiveConditionPressure), maximums.DefensiveConditionPressure > 0.0 ? 0.20 : 0.0),
            (NormalizeValue(aggregate.DefensiveSupportWindows, maximums.DefensiveSupportWindows), maximums.DefensiveSupportWindows > 0 ? 0.10 : 0.0));
        double sharePercent = ComputePreventionSharePercent(aggregate, totals, hasBarrierData);
        string barrierValue = hasBarrierData ? FormatWholeNumber(aggregate.BarrierTotal) : "Unavailable";
        string barrierSecondary = hasBarrierData ? "" : "Missing barrier extension data";
        var preventionEvidenceParts = new List<string>();
        if (hasBarrierData)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber(aggregate.BarrierTotal)} barrier");
        }
        if (aggregate.AttributedNegatedDamageTotal > 0.0)
        {
            preventionEvidenceParts.Add($"{FormatOneDecimal(aggregate.AttributedNegatedDamageTotal)} negated damage");
        }
        if (aggregate.PetMinionAbsorptionTotal > 0)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber(aggregate.PetMinionAbsorptionTotal)} pet absorption");
        }
        if (aggregate.DefensiveConditionPressure > 0.0)
        {
            preventionEvidenceParts.Add($"{FormatWholeNumber((long)Math.Round(aggregate.DefensiveConditionPressure))} defensive condition pressure");
        }
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Prevention Metrics",
            [
                BuildDetailEntry("Barrier", barrierValue, barrierSecondary),
                BuildDetailEntry("Negated damage", FormatOneDecimal(aggregate.AttributedNegatedDamageTotal), "Estimated prevented damage from attributed Aegis, Blind, Distortion, Blur, and tracked invulnerability-style effects"),
                BuildDetailEntry("Pet absorption", FormatWholeNumber(aggregate.PetMinionAbsorptionTotal), "Incoming damage taken by owned pets and minions"),
                BuildDetailEntry("Defensive condition pressure", FormatWholeNumber((long)Math.Round(aggregate.DefensiveConditionPressure)), ""),
                BuildDetailEntry("Prevention windows", BuildPluralizedLabel(aggregate.DefensiveSupportWindows, "prevention window", "prevention windows"), $"{aggregate.DefensiveSupportWindowsTotal} total")
            ])
        ];
        if (aggregate.AttributedNegatedDamageByEffect.Count > 0)
        {
            detailSections.Add(BuildDetailSection(
                "Attributed Negation Breakdown",
                BuildNamedContributionDetailEntries(aggregate.AttributedNegatedDamageByEffect, "estimated damage")));
        }
        string preventionSummary = preventionEvidenceParts.Count > 0
            ? $"{string.Join(", ", preventionEvidenceParts)} reduced incoming pressure before it became recovery work."
            : "Preventive value was limited in this fight.";
        return new PlayerLaneSnapshot(
            "prevention",
            "Prevention",
            strengthPercent,
            sharePercent,
            aggregate.DefensiveSupportWindows,
            aggregate.DefensiveSupportWindowsTotal,
            "prevention windows",
            GetRateBand(strengthPercent),
            preventionSummary,
            true,
            "Prevention Detail",
            "Prevention captures barrier, attributed negations, pet/minion diversion, defensive conditions, and presence in windows where incoming pressure was prevented or redirected.",
            detailSections,
            [
                BuildLaneMetric("barrierTotal", "Barrier", aggregate.BarrierTotal, "barrier"),
                BuildLaneMetric("negatedDamageTotal", "Negated damage", aggregate.AttributedNegatedDamageTotal, "damage"),
                BuildLaneMetric("petAbsorptionTotal", "Pet absorption", aggregate.PetMinionAbsorptionTotal, "damage")
            ]);
    }

    private static PlayerLaneSnapshot BuildRezLaneSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationMaximums maximums,
        CombatReplayPlayerEvaluationTotals totals)
    {
        double strengthPercent = ComputeWeightedScore(
            (NormalizeValue(aggregate.SquadRecoveryWindowsHelped, maximums.SquadRecoveryWindowsHelped), maximums.SquadRecoveryWindowsHelped > 0 ? 0.42 : 0.0),
            (NormalizeValue(aggregate.DownedHealingOnRecoveries, maximums.DownedHealingOnRecoveries), maximums.DownedHealingOnRecoveries > 0.0 ? 0.28 : 0.0),
            (NormalizeValue(aggregate.RezTimeOnRecoveries, maximums.RezTimeOnRecoveries), maximums.RezTimeOnRecoveries > 0.0 ? 0.18 : 0.0),
            (NormalizeValue(aggregate.RezCountOnRecoveries, Math.Max(aggregate.RezCountOnRecoveries, 1.0)), aggregate.RezCountOnRecoveries > 0.0 ? 0.12 : 0.0));
        double sharePercent = totals.SquadRecoveryWindowsHelped > 0
            ? aggregate.SquadRecoveryWindowsHelped * 100.0 / totals.SquadRecoveryWindowsHelped
            : ComputePercent(aggregate.RezTimeOnRecoveries, totals.RezTimeOnRecoveries);
        List<CombatReplayPlayerEvaluationDetailSectionDto> detailSections =
        [
            BuildDetailSection("Rez Metrics",
            [
                BuildDetailEntry("Successful recoveries helped", BuildPluralizedLabel(aggregate.SquadRecoveryWindowsHelped, "recovery", "recoveries"), $"{aggregate.SquadRecoveryWindowsTotal} total"),
                BuildDetailEntry("Downed healing", FormatWholeNumber((long)Math.Round(aggregate.DownedHealingOnRecoveries)), ""),
                BuildDetailEntry("Rez casts", FormatOneDecimal(aggregate.RezCountOnRecoveries), ""),
                BuildDetailEntry("Rez time", $"{FormatOneDecimal(aggregate.RezTimeOnRecoveries)}s", "")
            ])
        ];
        string recoveryText = BuildPluralizedLabel(aggregate.SquadRecoveryWindowsHelped, "successful recovery", "successful recoveries");
        string recoveryVerb = aggregate.SquadRecoveryWindowsHelped == 1 ? "was" : "were";
        return new PlayerLaneSnapshot(
            "rez",
            "Rez",
            strengthPercent,
            sharePercent,
            aggregate.SquadRecoveryWindowsHelped,
            aggregate.SquadRecoveryWindowsTotal,
            "recovery windows",
            GetRateBand(strengthPercent),
            $"{recoveryText} {recoveryVerb} supported with {FormatOneDecimal(aggregate.RezTimeOnRecoveries)}s of rez time and {FormatWholeNumber((long)Math.Round(aggregate.DownedHealingOnRecoveries))} downed healing.",
            true,
            "Rez Detail",
            "Rez focuses on downstate rescue in successful squad recoveries.",
            detailSections,
            [
                BuildLaneMetric("squadRecoveryWindowsHelped", "Recoveries helped", aggregate.SquadRecoveryWindowsHelped, "count"),
                BuildLaneMetric("rezTimeOnRecoveries", "Rez time", aggregate.RezTimeOnRecoveries, "seconds"),
                BuildLaneMetric("downedHealingOnRecoveries", "Downed healing", aggregate.DownedHealingOnRecoveries, "healing")
            ]);
    }

    private static CombatReplayContributionConfidenceDto BuildPlayerEvaluationConfidence(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var caveats = new List<string>
        {
            "Enemy difficulty is inferred, not directly measured",
            "Contribution is shared, not perfectly attributable",
        };
        bool incompleteCoverage = !hasHealingData || !hasBarrierData || !aggregate.HasPositioningData;
        bool smallSample = aggregate.KeyWindowsTotal < 3 || aggregate.FightDurationSeconds < 20.0;
        if (incompleteCoverage)
        {
            caveats.Insert(0, "Data coverage is incomplete");
        }
        if (smallSample)
        {
            caveats.Add("Small sample: conclusions may be noisy");
        }

        int degraders = (incompleteCoverage ? 1 : 0) + (smallSample ? 1 : 0);
        string label = degraders switch
        {
            >= 2 => "Low",
            1 => "Medium",
            _ => "High",
        };
        string detail = incompleteCoverage && smallSample
            ? "Profile confidence is limited by missing coverage and a small sample."
            : incompleteCoverage
                ? "Profile confidence is moderated by incomplete data coverage."
                : smallSample
                    ? "Profile confidence is moderated by a small sample."
                    : "Profile confidence is high for the visible data captured here.";
        return new CombatReplayContributionConfidenceDto
        {
            Label = label,
            Detail = detail,
            Caveats = caveats,
        };
    }

    private static CombatReplayPlayerFightImpactDto BuildPlayerFightImpact(
        IReadOnlyList<PlayerLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand,
        CombatReplayContributionConfidenceDto confidence,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var demandByKey = (fightDemand.Lanes ?? [])
            .Where(lane => !string.IsNullOrWhiteSpace(lane.Key))
            .GroupBy(lane => lane.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var weightedLanes = laneSnapshots
            .Select(lane =>
            {
                demandByKey.TryGetValue(lane.Key, out CombatReplayFightDemandLaneDto? demand);
                return new
                {
                    Lane = lane,
                    Demand = demand,
                    RawWeight = demand is null ? 0.0 : ComputeFightImpactDemandWeight(demand)
                };
            })
            .Where(entry => entry.RawWeight > 0.0 && entry.Demand is not null)
            .ToArray();
        double totalRawWeight = weightedLanes.Sum(entry => entry.RawWeight);
        if (weightedLanes.Length == 0 || totalRawWeight <= 0.0)
        {
            return new CombatReplayPlayerFightImpactDto
            {
                Label = "No Signal",
                Summary = "Fight impact could not be weighted because lane demand was unavailable.",
                Detail = "Raw lane strengths are still available, but demand-adjusted impact needs fight demand scores.",
                ConfidenceLabel = confidence.Label,
                Caveats = [.. confidence.Caveats],
            };
        }

        List<CombatReplayPlayerFightImpactLaneDto> lanes = [.. weightedLanes
            .Select(entry =>
            {
                PlayerLaneSnapshot lane = entry.Lane;
                CombatReplayFightDemandLaneDto demand = entry.Demand!;
                double demandWeightPercent = entry.RawWeight * 100.0 / totalRawWeight;
                double impactScore = lane.StrengthPercent * demandWeightPercent / 100.0;
                return new CombatReplayPlayerFightImpactLaneDto
                {
                    Key = lane.Key,
                    Label = lane.Label,
                    StrengthPercent = lane.StrengthPercent,
                    SharePercent = lane.SharePercent,
                    DemandScorePercent = demand.DemandScorePercent,
                    DemandLabel = demand.DemandLabel,
                    DemandWeightPercent = Math.Round(demandWeightPercent, 1),
                    ImpactScore = Math.Round(impactScore, 1),
                    EvidenceLine = $"{demand.DemandLabel} demand: {demand.EvidenceLine}",
                };
            })
            .OrderByDescending(lane => lane.ImpactScore)
            .ThenByDescending(lane => lane.DemandScorePercent)
            .ThenByDescending(lane => lane.StrengthPercent)
            .ThenBy(lane => lane.Label, StringComparer.OrdinalIgnoreCase)];
        double score = Math.Round(lanes.Sum(lane => lane.ImpactScore), 1);
        string label = GetFightImpactLabel(score);
        var caveats = new List<string>(confidence.Caveats);
        if (!hasHealingData && lanes.Any(lane => lane.Key == "recovery" && lane.DemandScorePercent >= 30.0))
        {
            caveats.Insert(0, "Recovery impact may be undercounted because healing extension data is missing");
        }
        if (!hasBarrierData && lanes.Any(lane => lane.Key == "prevention" && lane.DemandScorePercent >= 30.0))
        {
            caveats.Insert(0, "Prevention impact may be undercounted because barrier extension data is missing");
        }

        return new CombatReplayPlayerFightImpactDto
        {
            Score = score,
            Label = label,
            Summary = BuildPlayerFightImpactSummary(score, lanes),
            Detail = "Weights this player's raw lane strengths by replay-visible lane demand, then sums the weighted lane points. Raw lane scores are unchanged.",
            ConfidenceLabel = confidence.Label,
            Caveats = [.. caveats.Distinct(StringComparer.OrdinalIgnoreCase)],
            Lanes = lanes,
        };
    }

    private static CombatReplaySpecFightCoverageDto BuildSpecFightCoverage(
        IReadOnlyList<SpecLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var demandByKey = (fightDemand.Lanes ?? [])
            .Where(lane => !string.IsNullOrWhiteSpace(lane.Key))
            .GroupBy(lane => lane.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var weightedLanes = laneSnapshots
            .Select(lane =>
            {
                demandByKey.TryGetValue(lane.Key, out CombatReplayFightDemandLaneDto? demand);
                return new
                {
                    Lane = lane,
                    Demand = demand,
                    RawWeight = demand is null ? 0.0 : ComputeFightImpactDemandWeight(demand)
                };
            })
            .Where(entry => entry.RawWeight > 0.0 && entry.Demand is not null)
            .ToArray();
        double totalRawWeight = weightedLanes.Sum(entry => entry.RawWeight);
        if (weightedLanes.Length == 0 || totalRawWeight <= 0.0)
        {
            return new CombatReplaySpecFightCoverageDto
            {
                Label = "No Signal",
                Summary = "Spec fight coverage could not be weighted because lane demand was unavailable.",
                Detail = "Raw spec lane capability remains available, but demand-adjusted coverage needs fight demand scores.",
            };
        }

        List<CombatReplaySpecFightCoverageLaneDto> lanes = [.. weightedLanes
            .Select(entry =>
            {
                SpecLaneSnapshot lane = entry.Lane;
                CombatReplayFightDemandLaneDto demand = entry.Demand!;
                double demandWeightPercent = entry.RawWeight * 100.0 / totalRawWeight;
                double coverageScore = lane.StrengthPercent * demandWeightPercent / 100.0;
                return new CombatReplaySpecFightCoverageLaneDto
                {
                    Key = lane.Key,
                    Label = lane.Label,
                    StrengthPercent = lane.StrengthPercent,
                    SharePercent = lane.SharePercent,
                    PerSlotEfficiency = lane.PerSlotEfficiency,
                    PlayersContributing = lane.PlayersContributing,
                    PlayerCount = lane.PlayerCount,
                    DemandScorePercent = demand.DemandScorePercent,
                    DemandLabel = demand.DemandLabel,
                    DemandWeightPercent = Math.Round(demandWeightPercent, 1),
                    CoverageScore = Math.Round(coverageScore, 1),
                    EvidenceLine = $"{demand.DemandLabel} demand: {demand.EvidenceLine}",
                };
            })
            .OrderByDescending(lane => lane.CoverageScore)
            .ThenByDescending(lane => lane.DemandScorePercent)
            .ThenByDescending(lane => lane.StrengthPercent)
            .ThenBy(lane => lane.Label, StringComparer.OrdinalIgnoreCase)];
        double score = Math.Round(lanes.Sum(lane => lane.CoverageScore), 1);
        var caveats = new List<string>();
        if (!hasHealingData && lanes.Any(lane => lane.Key == "recovery" && lane.DemandScorePercent >= 30.0))
        {
            caveats.Add("Recovery coverage may be undercounted because healing extension data is missing");
        }
        if (!hasBarrierData && lanes.Any(lane => lane.Key == "prevention" && lane.DemandScorePercent >= 30.0))
        {
            caveats.Add("Prevention coverage may be undercounted because barrier extension data is missing");
        }

        return new CombatReplaySpecFightCoverageDto
        {
            Score = score,
            Label = GetSpecFightCoverageLabel(score),
            Summary = BuildSpecFightCoverageSummary(score, lanes),
            Detail = "Weights this spec's raw lane capability by replay-visible lane demand, then sums the weighted coverage points. Raw spec lane capability scores are unchanged.",
            Caveats = [.. caveats.Distinct(StringComparer.OrdinalIgnoreCase)],
            Lanes = lanes,
        };
    }

    private static string BuildSpecFightCoverageSummary(double score, IReadOnlyList<CombatReplaySpecFightCoverageLaneDto> lanes)
    {
        var topLanes = lanes
            .Where(lane => lane.CoverageScore > 0.0)
            .Take(3)
            .Select(lane => $"{lane.Label} {FormatOneDecimal(lane.CoverageScore)}")
            .ToArray();
        if (topLanes.Length == 0 || score <= 0.0)
        {
            return "No demand-adjusted spec coverage stood out in this fight.";
        }
        return $"Demand-adjusted spec coverage came through {string.Join(", ", topLanes)}.";
    }

    private static string GetSpecFightCoverageLabel(double score)
    {
        return score switch
        {
            >= 75.0 => "Huge spec coverage",
            >= 55.0 => "Major spec coverage",
            >= 30.0 => "Strong spec coverage",
            > 0.0 => "Focused spec coverage",
            _ => "No Signal",
        };
    }

    private static double ComputeFightImpactDemandWeight(CombatReplayFightDemandLaneDto demand)
    {
        double normalizedDemand = Math.Clamp(demand.DemandScorePercent / 100.0, 0.0, 1.0);
        return Math.Pow(normalizedDemand, 1.15) * Math.Max(demand.WeightMultiplier, 0.01);
    }

    private static string BuildPlayerFightImpactSummary(double score, IReadOnlyList<CombatReplayPlayerFightImpactLaneDto> lanes)
    {
        var topLanes = lanes
            .Where(lane => lane.ImpactScore > 0.0)
            .Take(3)
            .Select(lane => $"{lane.Label} {FormatOneDecimal(lane.ImpactScore)}")
            .ToArray();
        if (topLanes.Length == 0 || score <= 0.0)
        {
            return "No demand-adjusted contribution stood out in this fight.";
        }
        return $"Demand-adjusted value came through {string.Join(", ", topLanes)}.";
    }

    private static string GetFightImpactLabel(double score)
    {
        return score switch
        {
            >= 75.0 => "Huge fight share",
            >= 55.0 => "Major fight share",
            >= 30.0 => "Strong fight share",
            > 0.0 => "Focused fight share",
            _ => "No Signal",
        };
    }

    private static List<CombatReplayPlayerEvaluationModifierDto> BuildPlayerModifiers(CombatReplayPlayerEvaluationAggregate aggregate)
    {
        double activePercent = ComputePercent(aggregate.ActiveSeconds, aggregate.FightDurationSeconds);
        double engagedPercent = ComputePercent(aggregate.CombatSeconds, aggregate.FightDurationSeconds);
        return
        [
            new CombatReplayPlayerEvaluationModifierDto
            {
                Label = "Discipline",
                Value = aggregate.HasPositioningData ? $"{FormatOneDecimal(aggregate.InPositionRate)}% in position" : "No positioning samples",
                Detail = aggregate.HasPositioningData
                    ? $"{FormatOneDecimal(aggregate.TooFarRate)}% too far, {FormatOneDecimal(aggregate.OverextendedRate)}% overextended, {FormatOneDecimal(aggregate.LateralRiskRate)}% left/right exposed"
                    : "Commander-relative positioning could not be evaluated for this player.",
            },
            new CombatReplayPlayerEvaluationModifierDto
            {
                Label = "Survival",
                Value = $"{BuildPluralizedLabel(aggregate.Downs, "down", "downs")}, {BuildPluralizedLabel(aggregate.Deaths, "death", "deaths")}",
                Detail = $"{BuildPluralizedLabel(aggregate.Recoveries, "recovery", "recoveries")} after being downed during the fight.",
            },
            new CombatReplayPlayerEvaluationModifierDto
            {
                Label = "Participation",
                Value = $"{FormatOneDecimal(activePercent)}% active",
                Detail = $"{aggregate.KeyWindowsHit}/{aggregate.KeyWindowsTotal} key windows, {FormatOneDecimal(engagedPercent)}% engaged presence",
            },
        ];
    }

    private static List<string> BuildEvidenceSnapshot(
        CombatReplayPlayerEvaluationAggregate aggregate,
        IReadOnlyList<PlayerLaneSnapshot> laneSnapshots,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var evidence = new List<string>();
        foreach (PlayerLaneSnapshot lane in laneSnapshots.Take(2))
        {
            evidence.Add($"{lane.Label}: {FormatOneDecimal(lane.SharePercent)}% squad share across {lane.WindowsHit}/{lane.WindowsTotal} {lane.WindowLabel}.");
        }
        if (aggregate.HasPositioningData)
        {
            evidence.Add($"{FormatOneDecimal(aggregate.InPositionRate)}% in position across {BuildPluralizedLabel(aggregate.PositioningSamples, "sample", "samples")}.");
        }
        if (aggregate.Deaths == 0)
        {
            evidence.Add("No deaths recorded in this fight.");
        }
        else
        {
            evidence.Add($"{BuildPluralizedLabel(aggregate.Deaths, "death", "deaths")} recorded during the fight.");
        }
        if (!hasHealingData || !hasBarrierData)
        {
            evidence.Add("Recovery and Prevention reads are partially limited by missing extension data.");
        }
        return [.. evidence.Take(4)];
    }

    private static string BuildPlayerFitSummary(
        CombatReplayPlayerEvaluationAggregate aggregate,
        IReadOnlyList<PlayerLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand,
        CombatReplayContributionConfidenceDto confidence)
    {
        if (laneSnapshots.Count == 0)
        {
            return "Observed contribution was too thin to summarize.";
        }

        bool smallSample = aggregate.KeyWindowsTotal < 3 || aggregate.FightDurationSeconds < 20.0;
        string prefix = ComputePercent(aggregate.ActiveSeconds, aggregate.FightDurationSeconds) < 70.0 ? "When present, " : "";
        PlayerLaneSnapshot primaryLane = laneSnapshots[0];
        PlayerLaneSnapshot? secondaryLane = SelectSecondaryLane(laneSnapshots);
        var alignedLanes = laneSnapshots
            .Select(lane => new
            {
                Lane = lane,
                Score = lane.StrengthPercent * GetDemandScore(fightDemand, lane.Key),
                Demand = GetDemandScore(fightDemand, lane.Key),
            })
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Demand)
            .ToList();
        var alignedPrimary = alignedLanes[0];
        string secondaryText = secondaryLane != null ? $" + {secondaryLane.Value.Label}" : "";

        if (smallSample)
        {
            return $"{prefix}Observed contribution leaned {primaryLane.Label}{secondaryText}, but the sample is thin.";
        }

        if (alignedPrimary.Demand >= 0.55 && alignedPrimary.Score >= primaryLane.StrengthPercent * 0.35)
        {
            PlayerLaneSnapshot? alignedSecondary = alignedLanes.Count > 1 && alignedLanes[1].Score >= alignedLanes[0].Score * 0.60
                ? alignedLanes[1].Lane
                : null;
            string alignedSecondaryText = alignedSecondary != null ? $" + {alignedSecondary.Value.Label}" : "";
            return $"{prefix}Best fit through {alignedPrimary.Lane.Label}{alignedSecondaryText}.";
        }

        return $"{prefix}Most visible through {primaryLane.Label}{secondaryText}.";
    }

    private static string BuildPlayerDemandFitSummary(
        CombatReplayPlayerEvaluationAggregate aggregate,
        IReadOnlyList<PlayerLaneSnapshot> laneSnapshots,
        CombatReplayFightDemandDto fightDemand,
        CombatReplayContributionConfidenceDto confidence)
    {
        if (laneSnapshots.Count == 0)
        {
            return confidence.Detail;
        }

        List<CombatReplayFightDemandLaneDto> demandedLanes = [.. fightDemand.Lanes.Where(lane => lane.DemandScorePercent >= 55.0)];
        if (demandedLanes.Count == 0)
        {
            return "Fight demand was too evenly distributed to prioritize one lane heavily.";
        }

        double coverage = demandedLanes.Average(demandedLane =>
        {
            PlayerLaneSnapshot matchingLane = laneSnapshots.FirstOrDefault(lane => lane.Key == demandedLane.Key);
            return string.IsNullOrEmpty(matchingLane.Key) ? 0.0 : matchingLane.StrengthPercent;
        });
        string topDemandLabel = demandedLanes[0].Label;
        if (coverage >= 60.0)
        {
            return "Main contributions lined up well with the fight's biggest needs.";
        }
        if (coverage >= 35.0)
        {
            return $"Main contributions covered part of the fight's biggest needs, especially {topDemandLabel}.";
        }
        string demandLean = string.Join(" + ", demandedLanes.Take(2).Select(lane => lane.Label));
        return $"Visible contribution was more specialized than this fight's biggest demands, which leaned more on {demandLean}.";
    }

    private static string BuildLegacyContributionProfile(IReadOnlyList<PlayerLaneSnapshot> laneSnapshots)
    {
        if (laneSnapshots.Count == 0)
        {
            return "";
        }
        PlayerLaneSnapshot primary = laneSnapshots[0];
        PlayerLaneSnapshot? secondary = SelectSecondaryLane(laneSnapshots);
        return secondary != null ? $"{primary.Label} + {secondary.Value.Label}" : primary.Label;
    }

    private static PlayerLaneSnapshot? SelectSecondaryLane(IReadOnlyList<PlayerLaneSnapshot> laneSnapshots)
    {
        if (laneSnapshots.Count < 2)
        {
            return null;
        }
        PlayerLaneSnapshot primary = laneSnapshots[0];
        PlayerLaneSnapshot secondary = laneSnapshots[1];
        return secondary.StrengthPercent >= primary.StrengthPercent * 0.60 ? secondary : null;
    }

    private static SpecLaneSnapshot? SelectSecondarySpecLane(IReadOnlyList<SpecLaneSnapshot> laneSnapshots)
    {
        if (laneSnapshots.Count < 2)
        {
            return null;
        }

        SpecLaneSnapshot primary = laneSnapshots[0];
        SpecLaneSnapshot secondary = laneSnapshots[1];
        return secondary.StrengthPercent >= primary.StrengthPercent * 0.60 ? secondary : null;
    }

    private static string GetDemandLabel(double demandScorePercent)
    {
        return demandScorePercent switch
        {
            >= 75.0 => "Very High",
            >= 55.0 => "High",
            >= 30.0 => "Moderate",
            _ => "Low",
        };
    }

    private static double GetDemandScore(CombatReplayFightDemandDto fightDemand, string laneKey)
    {
        CombatReplayFightDemandLaneDto? lane = fightDemand.Lanes.FirstOrDefault(entry => entry.Key == laneKey);
        return lane != null ? lane.DemandScorePercent / 100.0 : 0.0;
    }

    private static int CountPlayersContributing(
        IReadOnlyList<CombatReplayPlayerEvaluationAggregate> players,
        Func<CombatReplayPlayerEvaluationAggregate, double> rawSelector)
    {
        return players.Count(player => rawSelector(player) > 0.0);
    }

    private static double ComputeTopContributorSharePercent(
        IReadOnlyList<CombatReplayPlayerEvaluationAggregate> players,
        Func<CombatReplayPlayerEvaluationAggregate, double> rawSelector,
        double totalRawAmount)
    {
        if (totalRawAmount <= 0.0 || players.Count == 0)
        {
            return 0.0;
        }

        double topAmount = players.Max(player => rawSelector(player));
        return Math.Round(Math.Clamp(topAmount * 100.0 / totalRawAmount, 0.0, 100.0), 1);
    }

    private static double ComputeSpecPerSlotEfficiency(double sharePercent, double activeSharePercent)
    {
        if (sharePercent <= 0.0)
        {
            return 0.0;
        }

        return Math.Round(Math.Clamp(sharePercent / Math.Max(activeSharePercent, 5.0), 0.0, 4.0), 1);
    }

    private static double ComputeSpecLaneStrength(
        double rawAmount,
        double averagePerPlayerMaximum,
        double sharePercent,
        double perSlotEfficiency,
        int playersContributing,
        int playerCount)
    {
        double averagePerPlayer = rawAmount / Math.Max(playerCount, 1);
        double averageStrength = NormalizeValue(averagePerPlayer, averagePerPlayerMaximum);
        double shareStrength = Math.Clamp(sharePercent / 35.0, 0.0, 1.0);
        double efficiencyStrength = Math.Clamp(perSlotEfficiency / 3.0, 0.0, 1.0);
        double contributorCoverage = playerCount > 0
            ? Math.Clamp(playersContributing / (double)playerCount, 0.0, 1.0)
            : 0.0;
        return Math.Round(100.0 * (
            0.50 * averageStrength +
            0.28 * efficiencyStrength +
            0.12 * shareStrength +
            0.10 * contributorCoverage), 1);
    }

    private static string GetDependencyLabel(
        double sharePercent,
        int playersContributing,
        int playerCount,
        double topContributorSharePercent)
    {
        double dependencyScore = ComputeDependencyScore(sharePercent, playersContributing, playerCount, topContributorSharePercent);
        return dependencyScore switch
        {
            >= 65.0 => "High dependency",
            >= 45.0 => "Medium dependency",
            >= 25.0 => "Low dependency",
            _ => "",
        };
    }

    private static double ComputeDependencyScore(
        double sharePercent,
        int playersContributing,
        int playerCount,
        double topContributorSharePercent)
    {
        if (sharePercent < 15.0 || playersContributing == 0 || playerCount == 0)
        {
            return 0.0;
        }

        double sharePressure = Math.Clamp(sharePercent / 40.0, 0.0, 1.0);
        double expectedShare = 100.0 / Math.Max(playersContributing, 1);
        double concentration = playersContributing <= 1
            ? 1.0
            : Math.Clamp((topContributorSharePercent - expectedShare) / Math.Max(100.0 - expectedShare, 1.0), 0.0, 1.0);
        double redundancyRisk = 1.0 - Math.Clamp(playersContributing / (double)playerCount, 0.0, 1.0);
        return Math.Round(100.0 * (
            0.50 * sharePressure +
            0.30 * concentration +
            0.20 * redundancyRisk), 1);
    }

    private static string GetRateBand(double strengthPercent)
    {
        return strengthPercent switch
        {
            >= 80.0 => "Very High",
            >= 55.0 => "High",
            >= 30.0 => "Medium",
            _ => "Low",
        };
    }

    private static double ComputeRecoverySharePercent(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationTotals totals,
        bool hasHealingData,
        bool hasBarrierData)
    {
        var weightedShares = new List<(double Share, double Weight)>();
        if (hasHealingData && totals.HealingTotal > 0)
        {
            weightedShares.Add((aggregate.HealingTotal * 100.0 / totals.HealingTotal, 0.60));
        }
        if (totals.CleansesTotal > 0)
        {
            weightedShares.Add((aggregate.CleansesTotal * 100.0 / totals.CleansesTotal, 0.25));
        }
        if (totals.RecoveryContributionWindows > 0)
        {
            weightedShares.Add((aggregate.RecoveryContributionWindows * 100.0 / totals.RecoveryContributionWindows, 0.15));
        }
        if (weightedShares.Count == 0)
        {
            return 0.0;
        }
        double weightTotal = weightedShares.Sum(entry => entry.Weight);
        return Math.Round(weightedShares.Sum(entry => entry.Share * entry.Weight) / Math.Max(weightTotal, 0.01), 1);
    }

    private static double ComputeRecoveryContributionMagnitude(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasHealingData,
        bool hasBarrierData)
    {
        double magnitude = aggregate.CleansesTotal * (hasHealingData ? 1.0 : 1.6) +
            aggregate.RecoveryContributionWindows * 3.0;
        if (hasHealingData)
        {
            magnitude += aggregate.HealingTotal / 2500.0;
        }
        return Math.Round(magnitude, 2);
    }

    private static double ComputePreventionSharePercent(
        CombatReplayPlayerEvaluationAggregate aggregate,
        CombatReplayPlayerEvaluationTotals totals,
        bool hasBarrierData)
    {
        var weightedShares = new List<(double Share, double Weight)>();
        if (hasBarrierData && totals.BarrierTotal > 0)
        {
            weightedShares.Add((aggregate.BarrierTotal * 100.0 / totals.BarrierTotal, 0.35));
        }
        if (totals.AttributedNegatedDamageTotal > 0.0)
        {
            weightedShares.Add((aggregate.AttributedNegatedDamageTotal * 100.0 / totals.AttributedNegatedDamageTotal, 0.30));
        }
        if (totals.PetMinionAbsorptionTotal > 0)
        {
            weightedShares.Add((aggregate.PetMinionAbsorptionTotal * 100.0 / totals.PetMinionAbsorptionTotal, 0.15));
        }
        if (totals.DefensiveConditionPressure > 0.0)
        {
            weightedShares.Add((aggregate.DefensiveConditionPressure * 100.0 / totals.DefensiveConditionPressure, 0.10));
        }
        if (totals.DefensiveSupportWindows > 0)
        {
            weightedShares.Add((aggregate.DefensiveSupportWindows * 100.0 / totals.DefensiveSupportWindows, 0.10));
        }
        if (weightedShares.Count == 0)
        {
            return 0.0;
        }
        double weightTotal = weightedShares.Sum(entry => entry.Weight);
        return Math.Round(weightedShares.Sum(entry => entry.Share * entry.Weight) / Math.Max(weightTotal, 0.01), 1);
    }

    private static double ComputePreventionContributionMagnitude(
        CombatReplayPlayerEvaluationAggregate aggregate,
        bool hasBarrierData)
    {
        double magnitude = aggregate.DefensiveSupportWindows * 3.0;
        if (hasBarrierData)
        {
            magnitude += aggregate.BarrierTotal / 2500.0;
        }
        if (aggregate.AttributedNegatedDamageTotal > 0.0)
        {
            magnitude += aggregate.AttributedNegatedDamageTotal / 2500.0;
        }
        if (aggregate.PetMinionAbsorptionTotal > 0)
        {
            magnitude += aggregate.PetMinionAbsorptionTotal / 4000.0;
        }
        if (aggregate.DefensiveConditionPressure > 0.0)
        {
            magnitude += aggregate.DefensiveConditionPressure / 120.0;
        }
        return Math.Round(magnitude, 2);
    }

    private static CombatReplayPlayerEvaluationDetailSectionDto BuildDetailSection(
        string label,
        IEnumerable<CombatReplayPlayerEvaluationDetailEntryDto> entries)
    {
        return new CombatReplayPlayerEvaluationDetailSectionDto
        {
            Label = label,
            Entries = [.. entries],
        };
    }

    private static CombatReplayPlayerEvaluationDetailEntryDto BuildDetailEntry(string label, string value, string secondary)
    {
        return new CombatReplayPlayerEvaluationDetailEntryDto
        {
            Label = label,
            Value = value,
            Secondary = secondary,
        };
    }

    private static double ComputePercent(double numerator, double denominator)
    {
        return denominator > 0.0 ? Math.Round(numerator * 100.0 / denominator, 1) : 0.0;
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
        long totalPetMinionDamageAbsorbed = 0;
        var topPetMinionAbsorberContributions = new List<(int? ActorId, string Name, string Icon, double Amount, long EventTime)>();

        foreach (SingleActor player in squadPlayers)
        {
            long playerPetMinionAbsorption = ComputePetMinionAbsorptionTotal(log, player);
            if (playerPetMinionAbsorption > 0)
            {
                totalPetMinionDamageAbsorbed += playerPetMinionAbsorption;
                topPetMinionAbsorberContributions.Add((
                    player.UniqueID,
                    player.Character,
                    player.GetIcon(),
                    playerPetMinionAbsorption,
                    log.LogData.LogEnd));
            }

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
        summary.TotalPetMinionDamageAbsorbed = totalPetMinionDamageAbsorbed;
        summary.BarrierAbsorptionPercent = totalDamageToSquad > 0
            ? Math.Round(barrierDamageAbsorbed * 100.0 / totalDamageToSquad, 1)
            : 0.0;
        summary.PetMinionAbsorptionPercent = totalDamageToSquad + totalPetMinionDamageAbsorbed > 0
            ? Math.Round(totalPetMinionDamageAbsorbed * 100.0 / (totalDamageToSquad + totalPetMinionDamageAbsorbed), 1)
            : 0.0;
        summary.TopPetMinionAbsorbers = BuildTopActorSummaries(topPetMinionAbsorberContributions);
        summary.NegatedHitSummaries = BuildNegatedHitSummaries(log, squadPlayers);

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
        summary.SavedPlayersSummary = BuildDefenseSavedPlayersSummary(log, summary.Mitigation);
        summary.TopBarrierProviders = BuildTopActorSummaries(topBarrierProviderContributions);
        return summary;
    }

    private static long ComputePetMinionAbsorptionTotal(ParsedEvtcLog log, SingleActor player)
    {
        long totalAbsorbed = 0;
        foreach (Minions minions in player.GetMinions(log))
        {
            foreach (HealthDamageEvent damageEvent in minions.GetDamageTakenEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd))
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

                totalAbsorbed += totalDamage;
            }
        }
        return totalAbsorbed;
    }

    private static List<CombatReplayDefenseNegatedHitSummaryDto> BuildNegatedHitSummaries(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        var landedDamageLookup = BuildLandedDamageEstimateLookup(log, squadPlayers);
        long start = log.LogData.LogStart;
        long end = log.LogData.LogEnd;
        var summaries = CombatReplayMitigationDefinitions.NegatedEffects
            .GroupBy(effect => effect.SummaryKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (Label: group.First().SummaryLabel, Count: 0, EstimatedDamage: 0.0, Fallbacks: 0, Effects: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase), Occurrences: new List<CombatReplayDefenseNegatedHitOccurrenceDto>()),
                StringComparer.OrdinalIgnoreCase);
        summaries["blind"] = (Label: "Blind Misses", Count: 0, EstimatedDamage: 0.0, Fallbacks: 0, Effects: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase), Occurrences: new List<CombatReplayDefenseNegatedHitOccurrenceDto>());

        foreach (SingleActor player in squadPlayers)
        {
            Dictionary<string, List<(long Start, long End)>> negatedEffectRanges = BuildNegatedEffectRanges(player, log, start, end);
            foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, start, end))
            {
                (string? effectName, string? summaryKey) = ClassifyNegatedHit(damageEvent, negatedEffectRanges, includeGenericAbsorbs: true);
                if (summaryKey == null)
                {
                    continue;
                }

                double estimatedDamage = EstimateNegatedDamage(damageEvent, landedDamageLookup, out bool usedFallback);
                (string Label, int Count, double EstimatedDamage, int Fallbacks, Dictionary<string, int> Effects, List<CombatReplayDefenseNegatedHitOccurrenceDto> Occurrences) current = summaries[summaryKey];
                if (!string.IsNullOrEmpty(effectName))
                {
                    current.Effects.TryGetValue(effectName, out int currentCount);
                    current.Effects[effectName] = currentCount + 1;
                }
                current.Occurrences.Add(new CombatReplayDefenseNegatedHitOccurrenceDto
                {
                    Time = damageEvent.Time,
                    TimeLabel = FormatTime(damageEvent.Time),
                    ActorId = player.UniqueID,
                    PlayerName = player.Character,
                    EffectName = string.IsNullOrWhiteSpace(effectName) ? "Unknown effect" : effectName,
                    SkillName = string.IsNullOrWhiteSpace(damageEvent.Skill.Name) ? $"Skill {damageEvent.SkillID}" : damageEvent.Skill.Name,
                    EstimatedPreventedDamage = Math.Round(estimatedDamage, 1),
                    UsedFallbackEstimate = usedFallback,
                });
                summaries[summaryKey] = (
                    current.Label,
                    current.Count + 1,
                    current.EstimatedDamage + estimatedDamage,
                    current.Fallbacks + (usedFallback ? 1 : 0),
                    current.Effects,
                    current.Occurrences);
            }
        }

        return [.. summaries
            .Where(entry => entry.Value.Count > 0)
            .Select(entry => new CombatReplayDefenseNegatedHitSummaryDto
            {
                Key = entry.Key,
                Label = entry.Value.Label,
                NegatedHitCount = entry.Value.Count,
                EstimatedPreventedDamage = Math.Round(entry.Value.EstimatedDamage, 1),
                FallbackEstimateCount = entry.Value.Fallbacks,
                ContributingEffects = [.. entry.Value.Effects
                    .OrderByDescending(effect => effect.Value)
                    .ThenBy(effect => effect.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(effect => new CombatReplayEffectCountSummaryDto
                    {
                        Name = effect.Key,
                        Count = effect.Value,
                    })],
                Occurrences = [.. entry.Value.Occurrences
                    .OrderBy(occurrence => occurrence.Time)
                    .ThenByDescending(occurrence => occurrence.EstimatedPreventedDamage)
                    .ThenBy(occurrence => occurrence.EffectName, StringComparer.OrdinalIgnoreCase)],
            })
            .OrderByDescending(entry => entry.EstimatedPreventedDamage)
            .ThenByDescending(entry => entry.NegatedHitCount)
            .ThenBy(entry => entry.Label, StringComparer.OrdinalIgnoreCase)];
    }

    private static Dictionary<int, PlayerAttributedNegationSummary> BuildPlayerAttributedNegationSummaries(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        var summaries = squadPlayers.ToDictionary(
            player => player.UniqueID,
            _ => new PlayerAttributedNegationSummary());
        IReadOnlyDictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> landedDamageLookup = BuildLandedDamageEstimateLookup(log, squadPlayers);
        long start = log.LogData.LogStart;
        long end = log.LogData.LogEnd;
        foreach (SingleActor recipient in squadPlayers)
        {
            Dictionary<string, List<(long Start, long End)>> negatedEffectRanges = BuildNegatedEffectRanges(recipient, log, start, end);
            foreach (HealthDamageEvent damageEvent in recipient.GetDamageTakenEvents(null, log, start, end))
            {
                List<(int ProviderId, string EffectName)> providers = GetAttributedNegationProviders(recipient, damageEvent, log, squadPlayers, negatedEffectRanges);
                if (providers.Count == 0)
                {
                    continue;
                }

                double estimatedDamage = EstimateNegatedDamage(damageEvent, landedDamageLookup, out _);
                double splitDamage = estimatedDamage / providers.Count;
                foreach ((int providerId, string effectName) in providers)
                {
                    if (!summaries.TryGetValue(providerId, out PlayerAttributedNegationSummary? summary))
                    {
                        continue;
                    }

                    summary.TotalAmount += splitDamage;
                    summary.AmountByEffect[effectName] = summary.AmountByEffect.TryGetValue(effectName, out double existing)
                        ? existing + splitDamage
                        : splitDamage;
                }
            }
        }

        foreach (PlayerAttributedNegationSummary summary in summaries.Values)
        {
            summary.TotalAmount = Math.Round(summary.TotalAmount, 1);
            foreach (string effectName in summary.AmountByEffect.Keys.ToList())
            {
                summary.AmountByEffect[effectName] = Math.Round(summary.AmountByEffect[effectName], 1);
            }
        }
        return summaries;
    }

    private static Dictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> BuildLandedDamageEstimateLookup(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        var lookup = new Dictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)>();
        foreach (SingleActor player in squadPlayers)
        {
            foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, log.LogData.LogStart, log.LogData.LogEnd))
            {
                if (!damageEvent.HasHit || damageEvent.HealthDamage <= 0)
                {
                    continue;
                }

                var key = (damageEvent.CreditedFrom, damageEvent.SkillID);
                if (lookup.TryGetValue(key, out (double TotalDamage, int Count) current))
                {
                    lookup[key] = (current.TotalDamage + damageEvent.HealthDamage, current.Count + 1);
                }
                else
                {
                    lookup[key] = (damageEvent.HealthDamage, 1);
                }
            }
        }
        return lookup;
    }

    private static List<(int ProviderId, string EffectName)> GetAttributedNegationProviders(
        SingleActor recipient,
        HealthDamageEvent damageEvent,
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        IReadOnlyDictionary<string, List<(long Start, long End)>> negatedEffectRanges)
    {
        var providers = new List<(int ProviderId, string EffectName)>();
        var seenProviders = new HashSet<int>();
        if (damageEvent.IsBlind)
        {
            foreach (SingleActor squadPlayer in squadPlayers)
            {
                if (damageEvent.CreditedFrom.GetBuffStatus(log, squadPlayer, Blind, Math.Max(log.LogData.LogStart, damageEvent.Time - 1), Math.Min(log.LogData.LogEnd, damageEvent.Time + 1)).Any(segment => segment.Value > 0)
                    && seenProviders.Add(squadPlayer.UniqueID))
                {
                    providers.Add((squadPlayer.UniqueID, "Blind"));
                }
            }
            return providers;
        }

        if (damageEvent.IsBlocked)
        {
            foreach (SingleActor squadPlayer in squadPlayers)
            {
                if (recipient.GetBuffStatus(log, squadPlayer, Aegis, Math.Max(log.LogData.LogStart, damageEvent.Time - 1), Math.Min(log.LogData.LogEnd, damageEvent.Time + 1)).Any(segment => segment.Value > 0)
                    && seenProviders.Add(squadPlayer.UniqueID))
                {
                    providers.Add((squadPlayer.UniqueID, "Aegis"));
                }
            }
            return providers;
        }

        if (!damageEvent.IsAbsorbed)
        {
            return providers;
        }

        (string? effectName, string? _) = ClassifyNegatedHit(damageEvent, negatedEffectRanges, includeGenericAbsorbs: false);
        if (string.IsNullOrWhiteSpace(effectName)
            || string.Equals(effectName, "Unmatched absorb", StringComparison.OrdinalIgnoreCase)
            || !seenProviders.Add(recipient.UniqueID))
        {
            return providers;
        }

        providers.Add((recipient.UniqueID, effectName));
        return providers;
    }

    private static Dictionary<string, List<(long Start, long End)>> BuildNegatedEffectRanges(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end)
    {
        var ranges = new Dictionary<string, List<(long Start, long End)>>(StringComparer.OrdinalIgnoreCase);
        foreach (TrackedNegatedMitigationEffect effect in CombatReplayMitigationDefinitions.NegatedEffects)
        {
            List<(long Start, long End)> effectRanges = GetMergedBuffPresenceRanges(player, log, effect.BuffIds, start, end);
            if (effectRanges.Count > 0)
            {
                ranges[effect.Name] = effectRanges;
            }
        }
        return ranges;
    }

    private static (string? EffectName, string? SummaryKey) ClassifyNegatedHit(
        HealthDamageEvent damageEvent,
        IReadOnlyDictionary<string, List<(long Start, long End)>> negatedEffectRanges,
        bool includeGenericAbsorbs)
    {
        if (damageEvent.IsBlind)
        {
            return ("Blind", "blind");
        }

        foreach (TrackedNegatedMitigationEffect effect in CombatReplayMitigationDefinitions.NegatedEffects)
        {
            if ((damageEvent.IsBlocked && effect.Trigger != NegatedHitTrigger.Blocked)
                || (damageEvent.IsAbsorbed && effect.Trigger != NegatedHitTrigger.Absorbed)
                || (!damageEvent.IsBlocked && !damageEvent.IsAbsorbed))
            {
                continue;
            }

            if (negatedEffectRanges.TryGetValue(effect.Name, out List<(long Start, long End)>? ranges)
                && HasMitigationBuffNearTime(ranges, damageEvent.Time))
            {
                return (effect.Name, effect.SummaryKey);
            }
        }

        if (includeGenericAbsorbs && damageEvent.IsAbsorbed)
        {
            return ("Unmatched absorb", "invulnerability");
        }

        return (null, null);
    }

    private static double EstimateNegatedDamage(
        HealthDamageEvent damageEvent,
        IReadOnlyDictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> landedDamageLookup,
        out bool usedFallback)
    {
        if (landedDamageLookup.TryGetValue((damageEvent.CreditedFrom, damageEvent.SkillID), out (double TotalDamage, int Count) landedStats)
            && landedStats.Count > 0
            && landedStats.TotalDamage > 0)
        {
            usedFallback = false;
            return Math.Round(landedStats.TotalDamage / landedStats.Count, 1);
        }

        usedFallback = true;
        return 50.0;
    }

    private static bool HasMitigationBuffNearTime(
        IReadOnlyList<(long Start, long End)> ranges,
        long time,
        long graceMilliseconds = 1)
    {
        return ranges.Any(range => time >= range.Start && time <= range.End + graceMilliseconds);
    }

    private static CombatReplayDefenseMitigationDto BuildDefenseMitigationAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers)
    {
        IReadOnlyDictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> landedDamageLookup = BuildLandedDamageEstimateLookup(log, squadPlayers);
        int[] thresholds = [10, 20, 25, 33, 50, 80, 99];
        var result = new CombatReplayDefenseMitigationDto
        {
            Thresholds =
            [
                .. thresholds.Select(threshold => BuildDefenseMitigationThresholdAnalysis(log, squadPlayers, threshold, landedDamageLookup))
            ],
        };
        return result;
    }

    private static CombatReplayDefenseMitigationThresholdDto BuildDefenseMitigationThresholdAnalysis(
        ParsedEvtcLog log,
        IReadOnlyList<SingleActor> squadPlayers,
        int thresholdPercent,
        IReadOnlyDictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> landedDamageLookup)
    {
        var mitigationEvents = new List<CombatReplayDefenseMitigationEventDto>();

        foreach (SingleActor player in squadPlayers)
        {
            Dictionary<string, List<(long Start, long End)>> negatedEffectRanges = BuildNegatedEffectRanges(player, log, log.LogData.LogStart, log.LogData.LogEnd);
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
                    (int healthPoolUsed, bool healthPoolEstimated) = GetMitigationHealthPool(player, log);
                    int lowestHealthEstimate = (int)Math.Round(healthPoolUsed * lowestHealthPercent / 100.0, 0);
                    double barrierAbsorbedToLowest = 0.0;
                    if (lowestHealthTime > mitigationWindowStart)
                    {
                        barrierAbsorbedToLowest = Math.Round(player
                            .GetDamageTakenEvents(null, log, mitigationWindowStart, lowestHealthTime + 1)
                            .Where(damageEvent => damageEvent.HasHit && damageEvent.ShieldDamage > 0)
                            .Sum(damageEvent => (double)damageEvent.ShieldDamage), 1);
                    }
                    bool barrierSavedPlayer = barrierAbsorbedToLowest > 0 && lowestHealthEstimate - barrierAbsorbedToLowest <= 0;
                    (double estimatedMitigationToLowest, List<string> estimatedMitigationSavedEffects) =
                        GetEstimatedStrikeMitigation(player, log, mitigationWindowStart, lowestHealthTime > mitigationWindowStart ? lowestHealthTime + 1 : mitigationWindowStart);
                    bool estimatedMitigationSavedPlayer = estimatedMitigationToLowest > 0 && lowestHealthEstimate - estimatedMitigationToLowest <= 0;
                    (double estimatedMitigation, _) = GetEstimatedStrikeMitigation(player, log, mitigationWindowStart, healthSegment.Start);
                    (double estimatedNegatedDamageToLowest, List<string> estimatedNegatedDamageSavedEffects) =
                        GetEstimatedNegatedMitigation(player, log, mitigationWindowStart, lowestHealthTime > mitigationWindowStart ? lowestHealthTime + 1 : mitigationWindowStart, landedDamageLookup, negatedEffectRanges);
                    bool estimatedNegatedDamageSavedPlayer = estimatedNegatedDamageToLowest > 0 && lowestHealthEstimate - estimatedNegatedDamageToLowest <= 0;
                    (double estimatedNegatedDamage, _) = GetEstimatedNegatedMitigation(player, log, mitigationWindowStart, healthSegment.Start, landedDamageLookup, negatedEffectRanges);
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
                        HealthPoolUsed = healthPoolUsed,
                        HealthPoolEstimated = healthPoolEstimated,
                        LowestHealthPercent = lowestHealthPercent,
                        LowestHealthEstimate = lowestHealthEstimate,
                        BarrierAbsorbedToLowest = barrierAbsorbedToLowest,
                        BarrierSavedPlayer = barrierSavedPlayer,
                        EstimatedMitigationToLowest = estimatedMitigationToLowest,
                        EstimatedMitigation = estimatedMitigation,
                        EstimatedMitigationSavedPlayer = estimatedMitigationSavedPlayer,
                        EstimatedMitigationSavedEffects = estimatedMitigationSavedEffects,
                        EstimatedNegatedDamageToLowest = estimatedNegatedDamageToLowest,
                        EstimatedNegatedDamage = estimatedNegatedDamage,
                        EstimatedNegatedDamageSavedPlayer = estimatedNegatedDamageSavedPlayer,
                        EstimatedNegatedDamageSavedEffects = estimatedNegatedDamageSavedEffects,
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

    private static CombatReplayDefenseSavedPlayersSummaryDto BuildDefenseSavedPlayersSummary(
        ParsedEvtcLog log,
        CombatReplayDefenseMitigationDto mitigation)
    {
        CombatReplayDefenseMitigationThresholdDto? threshold99 = mitigation.Thresholds.FirstOrDefault(threshold => threshold.ThresholdPercent == 99);
        if (threshold99 == null || threshold99.Events.Count == 0)
        {
            return new CombatReplayDefenseSavedPlayersSummaryDto();
        }

        List<CombatReplayDefenseMitigationEventDto> savedEvents = [.. threshold99.Events.Where(evt =>
            evt.BarrierSavedPlayer || evt.EstimatedMitigationSavedPlayer || evt.EstimatedNegatedDamageSavedPlayer)];
        if (savedEvents.Count == 0)
        {
            return new CombatReplayDefenseSavedPlayersSummaryDto();
        }

        int barrierSavedCases = savedEvents.Count(evt => evt.BarrierSavedPlayer);
        int damageReductionSavedCases = savedEvents.Count(evt => evt.EstimatedMitigationSavedPlayer);
        int negatedDamageSavedCases = savedEvents.Count(evt => evt.EstimatedNegatedDamageSavedPlayer);
        int bothSavedCases = savedEvents.Count(evt => evt.BarrierSavedPlayer && evt.EstimatedMitigationSavedPlayer);
        int multiSourceSavedCases = savedEvents.Count(evt =>
        {
            int sources = 0;
            if (evt.BarrierSavedPlayer)
            {
                sources++;
            }
            if (evt.EstimatedMitigationSavedPlayer)
            {
                sources++;
            }
            if (evt.EstimatedNegatedDamageSavedPlayer)
            {
                sources++;
            }
            return sources > 1;
        });

        var result = new CombatReplayDefenseSavedPlayersSummaryDto
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

        result.TopDamageReductionEffects = [.. savedEvents
            .Where(evt => evt.EstimatedMitigationSavedPlayer && evt.EstimatedMitigationSavedEffects.Count > 0)
            .SelectMany(evt => evt.EstimatedMitigationSavedEffects.Distinct(StringComparer.OrdinalIgnoreCase))
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                TrackedMitigationReduction? definition = CombatReplayMitigationDefinitions.StrikeReductions
                    .FirstOrDefault(effect => effect.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase));
                return new CombatReplayEventActorSummaryDto
                {
                    Name = group.Key,
                    Icon = definition != null ? GetMitigationEffectIcon(log, definition.BuffIds) : "",
                    Count = group.Count(),
                    Amount = group.Count(),
                };
            })
            .OrderByDescending(entry => entry.Count)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)];
        result.TopNegatedDamageEffects = [.. savedEvents
            .Where(evt => evt.EstimatedNegatedDamageSavedPlayer && evt.EstimatedNegatedDamageSavedEffects.Count > 0)
            .SelectMany(evt => evt.EstimatedNegatedDamageSavedEffects.Distinct(StringComparer.OrdinalIgnoreCase))
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                TrackedMitigationBuff? definition = CombatReplayMitigationDefinitions.TrackedEffects
                    .FirstOrDefault(effect => effect.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase));
                return new CombatReplayEventActorSummaryDto
                {
                    Name = group.Key,
                    Icon = definition != null ? GetMitigationEffectIcon(log, definition.BuffIds) : "",
                    Count = group.Count(),
                    Amount = group.Count(),
                };
            })
            .OrderByDescending(entry => entry.Count)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)];
        return result;
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

        foreach (TrackedMitigationBuff trackedBuff in CombatReplayMitigationDefinitions.TrackedEffects)
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

    private static (int HealthPoolUsed, bool Estimated) GetMitigationHealthPool(
        SingleActor player,
        ParsedEvtcLog log)
    {
        int actualHealthPool = player.GetHealth(log.CombatData);
        if (actualHealthPool > 0)
        {
            return (actualHealthPool, false);
        }

        int estimatedHealthPool = player.Spec switch
        {
            GW2EIEvtcParser.ParserHelper.Spec.Firebrand => 21000,
            GW2EIEvtcParser.ParserHelper.Spec.Troubadour => 19000,
            GW2EIEvtcParser.ParserHelper.Spec.Evoker => 17000,
            GW2EIEvtcParser.ParserHelper.Spec.Untamed => 18000,
            GW2EIEvtcParser.ParserHelper.Spec.Druid => 19000,
            GW2EIEvtcParser.ParserHelper.Spec.Berserker => 22000,
            GW2EIEvtcParser.ParserHelper.Spec.Dragonhunter => 19000,
            GW2EIEvtcParser.ParserHelper.Spec.Holosmith => 19000,
            GW2EIEvtcParser.ParserHelper.Spec.Amalgam => 19000,
            _ => CombatReplayDefenseMitigationEventDto.EstimatedPlayerMaxHealth,
        };
        return (estimatedHealthPool, true);
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

    private static (double EstimatedMitigation, List<string> UsedEffects) GetEstimatedStrikeMitigation(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end)
    {
        if (end <= start)
        {
            return (0.0, []);
        }

        Dictionary<string, List<(long Start, long End)>> reductionRanges = CombatReplayMitigationDefinitions.StrikeReductions
            .Select(effect => (Effect: effect, Ranges: GetMergedBuffPresenceRanges(player, log, effect.BuffIds, start, end)))
            .Where(entry => entry.Ranges.Count > 0)
            .ToDictionary(entry => entry.Effect.Name, entry => entry.Ranges, StringComparer.OrdinalIgnoreCase);
        if (reductionRanges.Count == 0)
        {
            return (0.0, []);
        }

        double preventedDamage = 0.0;
        var usedEffects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, start, end))
        {
            if (!damageEvent.HasHit || damageEvent.HealthDamage <= 0 || damageEvent.ConditionDamageBased(log))
            {
                continue;
            }

            List<TrackedMitigationReduction> activeEffects = [.. CombatReplayMitigationDefinitions.StrikeReductions
                .Where(effect => reductionRanges.TryGetValue(effect.Name, out List<(long Start, long End)>? ranges)
                    && ranges.Any(range => damageEvent.Time >= range.Start && damageEvent.Time < range.End))];
            if (activeEffects.Count == 0)
            {
                continue;
            }

            double combinedMultiplier = activeEffects.Aggregate(1.0, (current, effect) => current * (1.0 - effect.StrikeReduction));
            if (combinedMultiplier <= 0 || combinedMultiplier >= 1.0)
            {
                continue;
            }

            preventedDamage += damageEvent.HealthDamage * ((1.0 / combinedMultiplier) - 1.0);
            foreach (TrackedMitigationReduction effect in activeEffects)
            {
                usedEffects.Add(effect.Name);
            }
        }

        return (Math.Round(preventedDamage, 1), [.. usedEffects.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)]);
    }

    private static (double EstimatedNegatedDamage, List<string> UsedEffects) GetEstimatedNegatedMitigation(
        SingleActor player,
        ParsedEvtcLog log,
        long start,
        long end,
        IReadOnlyDictionary<(AgentItem Attacker, long SkillId), (double TotalDamage, int Count)> landedDamageLookup,
        IReadOnlyDictionary<string, List<(long Start, long End)>> negatedEffectRanges)
    {
        if (end <= start || negatedEffectRanges.Count == 0)
        {
            return (0.0, []);
        }

        double preventedDamage = 0.0;
        var usedEffects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (HealthDamageEvent damageEvent in player.GetDamageTakenEvents(null, log, start, end))
        {
            (string? effectName, string? _) = ClassifyNegatedHit(damageEvent, negatedEffectRanges, includeGenericAbsorbs: false);
            if (effectName == null)
            {
                continue;
            }

            preventedDamage += EstimateNegatedDamage(damageEvent, landedDamageLookup, out _);
            usedEffects.Add(effectName);
        }

        return (Math.Round(preventedDamage, 1), [.. usedEffects.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)]);
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
            List<HealthDamageEvent> trackedDamageEvents = BuildTrackedDamageEvents(log, actor);
            Dictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup = BuildPreviousDamageEventLookup(trackedDamageEvents);
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                yield return BuildDownEvent(log, actor, trackedDamageEvents, previousDamageEventLookup, downEvent, side, isEnemy);
            }
        }
    }

    private static CombatReplayDownEventDto BuildDownEvent(
        ParsedEvtcLog log,
        SingleActor actor,
        IReadOnlyList<HealthDamageEvent> trackedDamageEvents,
        IReadOnlyDictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup,
        DownEvent downEvent,
        string side,
        bool isEnemy)
    {
        DownOutcomeInfo outcomeInfo = GetDownOutcomeInfo(log, actor.AgentItem, downEvent.Time);
        long windowStart = Math.Max(log.LogData.LogStart, downEvent.Time - LookbackWindow);
        long conditionSnapshotTime = Math.Max(log.LogData.LogStart, downEvent.Time - 1);
        DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, trackedDamageEvents, previousDamageEventLookup, windowStart, downEvent.Time, conditionSnapshotTime);
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
            OffensiveProtocolObliterateHitCount = summary.OffensiveProtocolObliterateHitCount,
            OffensiveProtocolObliterateBarrierRemovedHitCount = summary.OffensiveProtocolObliterateBarrierRemovedHitCount,
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
            OffensiveProtocolObliterateHitCount = events.Sum(evt => evt.OffensiveProtocolObliterateHitCount),
            OffensiveProtocolObliterateBarrierRemovedHitCount = events.Sum(evt => evt.OffensiveProtocolObliterateBarrierRemovedHitCount),
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
            List<HealthDamageEvent> trackedDamageEvents = BuildTrackedDamageEvents(log, actor);
            Dictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup = BuildPreviousDamageEventLookup(trackedDamageEvents);
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                CombatReplayKillEventDto? killEvent = BuildKillEvent(log, actor, trackedDamageEvents, previousDamageEventLookup, downEvent, side, isEnemy);
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
        IReadOnlyList<HealthDamageEvent> trackedDamageEvents,
        IReadOnlyDictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup,
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
        DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, trackedDamageEvents, previousDamageEventLookup, downEvent.Time, killTime, conditionSnapshotTime);
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
            List<HealthDamageEvent> trackedDamageEvents = BuildTrackedDamageEvents(log, actor);
            Dictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup = BuildPreviousDamageEventLookup(trackedDamageEvents);
            foreach (DownEvent downEvent in log.CombatData.GetDownEvents(actor.AgentItem).OrderBy(evt => evt.Time))
            {
                CombatReplayRecoveredEventDto? recoveredEvent = BuildRecoveredEvent(log, actor, trackedDamageEvents, previousDamageEventLookup, squadPlayers, downEvent, side, isEnemy);
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
        IReadOnlyList<HealthDamageEvent> trackedDamageEvents,
        IReadOnlyDictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup,
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
            DamageWindowSummary summary = BuildDamageWindowSummary(log, actor, trackedDamageEvents, previousDamageEventLookup, downEvent.Time, recoveredTime, conditionSnapshotTime);
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
        IReadOnlyList<HealthDamageEvent> trackedDamageEvents,
        IReadOnlyDictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup,
        long windowStart,
        long windowEnd,
        long conditionSnapshotTime)
    {
        List<HealthDamageEvent> damageEvents = [.. trackedDamageEvents
            .Where(damageEvent => damageEvent.Time >= windowStart && damageEvent.Time <= windowEnd)];
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
            int totalDamage = damageEvent.HealthDamage;
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
                    && damageEvent.Skill.Name.Contains(MysticRebukeSkillName, StringComparison.OrdinalIgnoreCase))
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

        int totalDamageTaken = damageEvents.Sum(damageEvent => damageEvent.HealthDamage);
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
        ObliterateBarrierSummary obliterateSummary = BuildOffensiveProtocolObliterateSummary(damageEvents, previousDamageEventLookup);

        return new DamageWindowSummary(
            TotalDamageTaken: totalDamageTaken,
            StrikeDamageTaken: strikeDamageTaken,
            MysticRebukeDamageTaken: mysticRebukeDamageTaken,
            ConditionDamageTaken: conditionDamageTaken,
            BarrierDamageTaken: barrierDamageTaken,
            HitCount: damageEvents.Count,
            ContributorCount: contributorTotals.Count(pair => pair.Value > 0.0),
            OffensiveProtocolObliterateHitCount: obliterateSummary.HitCount,
            OffensiveProtocolObliterateBarrierRemovedHitCount: obliterateSummary.BarrierRemovedHitCount,
            Conditions: conditions,
            ConditionDamageBreakdown: conditionDamageBreakdown,
            Contributors: contributors,
            DamageTimeline: BuildDownDamageTimeline(log, damageEvents));
    }

    private static List<HealthDamageEvent> BuildTrackedDamageEvents(ParsedEvtcLog log, SingleActor actor)
    {
        return [.. actor.GetDamageTakenEvents(null, log)
            .Where(damageEvent => damageEvent.HasHit && (damageEvent.HealthDamage > 0 || damageEvent.ShieldDamage > 0))
            .OrderBy(damageEvent => damageEvent.Time)];
    }

    private static Dictionary<HealthDamageEvent, HealthDamageEvent?> BuildPreviousDamageEventLookup(IReadOnlyList<HealthDamageEvent> trackedDamageEvents)
    {
        var result = new Dictionary<HealthDamageEvent, HealthDamageEvent?>();
        HealthDamageEvent? previousDamageEvent = null;
        foreach (HealthDamageEvent damageEvent in trackedDamageEvents)
        {
            result[damageEvent] = previousDamageEvent;
            previousDamageEvent = damageEvent;
        }
        return result;
    }

    private static ObliterateBarrierSummary BuildOffensiveProtocolObliterateSummary(
        IReadOnlyList<HealthDamageEvent> damageEvents,
        IReadOnlyDictionary<HealthDamageEvent, HealthDamageEvent?> previousDamageEventLookup)
    {
        int hitCount = 0;
        int barrierRemovedHitCount = 0;
        foreach (HealthDamageEvent damageEvent in damageEvents)
        {
            if (!IsOffensiveProtocolObliterateHit(damageEvent))
            {
                continue;
            }

            hitCount++;
            if (previousDamageEventLookup.TryGetValue(damageEvent, out HealthDamageEvent? previousDamageEvent)
                && previousDamageEvent != null
                && previousDamageEvent.ShieldDamage > 0)
            {
                barrierRemovedHitCount++;
            }
        }

        return new ObliterateBarrierSummary(
            HitCount: hitCount,
            BarrierRemovedHitCount: barrierRemovedHitCount);
    }

    private static bool IsOffensiveProtocolObliterateHit(HealthDamageEvent damageEvent)
    {
        return OffensiveProtocolObliterateSkillIds.Contains(damageEvent.SkillID)
            || (!string.IsNullOrWhiteSpace(damageEvent.Skill.Name)
                && damageEvent.Skill.Name.StartsWith(OffensiveProtocolObliterateSkillNamePrefix, StringComparison.OrdinalIgnoreCase));
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
            int healthDamage = Math.Max(damageEvent.HealthDamage - damageEvent.ShieldDamage, 0);
            string value = damageEvent.ShieldDamage > 0
                ? $"{FormatWholeNumber(healthDamage)} health, {FormatWholeNumber(damageEvent.ShieldDamage)} barrier removed ({FormatWholeNumber(damageEvent.HealthDamage)} total)"
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
            int healthDamage = Math.Max(damageEvent.HealthDamage - damageEvent.ShieldDamage, 0);
            string value = damageEvent.ShieldDamage > 0
                ? $"{FormatWholeNumber(healthDamage)} health, {FormatWholeNumber(damageEvent.ShieldDamage)} barrier removed ({FormatWholeNumber(damageEvent.HealthDamage)} total)"
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

        return MergeEvaluationWindows(windows);
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

    private static int CountRecoveryContributionWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> defensiveResponseWindows,
        IReadOnlyList<long> times)
    {
        return CountTimelineContributionWindows(
            defensiveResponseWindows,
            times,
            attackerTimeline?.Healing,
            attackerTimeline?.Cleanses);
    }

    private static int CountPreventionContributionWindows(
        CombatReplayAnalysisAttackerTimelineDto? attackerTimeline,
        IReadOnlyList<EvaluationWindow> defensiveResponseWindows,
        IReadOnlyList<long> times,
        IReadOnlyDictionary<EvaluationWindow, double> conditionContribution)
    {
        return defensiveResponseWindows.Count(window =>
            HasTimelineContribution(times, window, attackerTimeline?.Barrier) ||
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

    private static List<DamageRecord> BuildDamageRecords(ParsedEvtcLog log, TeamActorContext context, Dictionary<long, SkillItem>? usedSkills = null)
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
                usedSkills?.TryAdd(damageEvent.SkillID, damageEvent.Skill);
                result.Add(new DamageRecord(damageEvent.Time, target.UniqueID, attackerUniqueId, damageEvent.SkillID, damageEvent.HealthDamage, damageEvent.HasDowned, damageEvent.HasKilled));
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
            if (TryGetEligiblePosition(attacker, log, time, out var position))
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
            if (TryGetEligiblePosition(target, log, time, out var position))
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
