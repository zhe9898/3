using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Persistence.Tests;

public sealed partial class SaveMigrationPipelineTests
{
    private sealed class TestMigrationModuleState : IModuleStateDescriptor
    {
        public string ModuleKey => "Test.Migration";
    }

    private static LegacyWarfareCampaignStateV1 CreateLegacyWarfareCampaignStateV1(WarfareCampaignState currentState)
    {
        return new LegacyWarfareCampaignStateV1
        {
            Campaigns = currentState.Campaigns.Select(static campaign => new LegacyCampaignFrontStateV1
            {
                CampaignId = campaign.CampaignId,
                AnchorSettlementId = campaign.AnchorSettlementId,
                AnchorSettlementName = campaign.AnchorSettlementName,
                CampaignName = campaign.CampaignName,
                IsActive = campaign.IsActive,
                ObjectiveSummary = campaign.ObjectiveSummary,
                MobilizedForceCount = campaign.MobilizedForceCount,
                FrontPressure = campaign.FrontPressure,
                SupplyState = campaign.SupplyState,
                MoraleState = campaign.MoraleState,
                MobilizationWindowLabel = campaign.MobilizationWindowLabel,
                SupplyLineSummary = campaign.SupplyLineSummary,
                OfficeCoordinationTrace = campaign.OfficeCoordinationTrace,
                SourceTrace = campaign.SourceTrace,
                LastAftermathSummary = campaign.LastAftermathSummary,
            }).ToList(),
            MobilizationSignals = currentState.MobilizationSignals.Select(static signal => new LegacyCampaignMobilizationSignalStateV1
            {
                SettlementId = signal.SettlementId,
                SettlementName = signal.SettlementName,
                ResponseActivationLevel = signal.ResponseActivationLevel,
                CommandCapacity = signal.CommandCapacity,
                Readiness = signal.Readiness,
                AvailableForceCount = signal.AvailableForceCount,
                OrderSupportLevel = signal.OrderSupportLevel,
                OfficeAuthorityTier = signal.OfficeAuthorityTier,
                AdministrativeLeverage = signal.AdministrativeLeverage,
                PetitionBacklog = signal.PetitionBacklog,
                MobilizationWindowLabel = signal.MobilizationWindowLabel,
                OfficeCoordinationTrace = signal.OfficeCoordinationTrace,
                SourceTrace = signal.SourceTrace,
            }).ToList(),
        };
    }

    public sealed class LegacyOfficeAndCareerStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.OfficeAndCareer;

        public List<LegacyOfficeCareerStateV1> People { get; set; } = new();

        public List<LegacyJurisdictionAuthorityStateV1> Jurisdictions { get; set; } = new();
    }

    public sealed class LegacyFamilyCoreStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.FamilyCore;

        public List<LegacyClanStateDataV1> Clans { get; set; } = new();

        public List<LegacyFamilyPersonStateV1> People { get; set; } = new();
    }

    public sealed class LegacyConflictAndForceStateV2 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.ConflictAndForce;

        public List<LegacySettlementForceStateV2> Settlements { get; set; } = new();
    }

    public sealed class LegacyWarfareCampaignStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.WarfareCampaign;

        public List<LegacyCampaignFrontStateV1> Campaigns { get; set; } = new();

        public List<LegacyCampaignMobilizationSignalStateV1> MobilizationSignals { get; set; } = new();
    }

    public sealed class LegacyOfficeCareerStateV1
    {
        public PersonId PersonId { get; set; }

        public ClanId ClanId { get; set; }

        public SettlementId SettlementId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public bool IsEligible { get; set; }

        public bool HasAppointment { get; set; }

        public string OfficeTitle { get; set; } = "Unappointed";

        public int AuthorityTier { get; set; }

        public int JurisdictionLeverage { get; set; }

        public int PetitionPressure { get; set; }

        public int OfficeReputation { get; set; }

        public string LastOutcome { get; set; } = string.Empty;

        public string LastExplanation { get; set; } = string.Empty;
    }

    public sealed class LegacyClanStateDataV1
    {
        public ClanId Id { get; set; }

        public string ClanName { get; set; } = string.Empty;

        public SettlementId HomeSettlementId { get; set; }

        public int Prestige { get; set; }

        public int SupportReserve { get; set; }

        public PersonId? HeirPersonId { get; set; }
    }

    public sealed class LegacyFamilyPersonStateV1
    {
        public PersonId Id { get; set; }

        public ClanId ClanId { get; set; }

        public string GivenName { get; set; } = string.Empty;

        public int AgeMonths { get; set; }

        public bool IsAlive { get; set; }
    }

    public sealed class LegacyJurisdictionAuthorityStateV1
    {
        public SettlementId SettlementId { get; set; }

        public PersonId? LeadOfficialPersonId { get; set; }

        public string LeadOfficialName { get; set; } = string.Empty;

        public string LeadOfficeTitle { get; set; } = string.Empty;

        public int AuthorityTier { get; set; }

        public int JurisdictionLeverage { get; set; }

        public int PetitionPressure { get; set; }

        public string LastAdministrativeTrace { get; set; } = string.Empty;
    }

    public sealed class LegacySettlementForceStateV2
    {
        public SettlementId SettlementId { get; set; }

        public int GuardCount { get; set; }

        public int RetainerCount { get; set; }

        public int MilitiaCount { get; set; }

        public int EscortCount { get; set; }

        public int Readiness { get; set; }

        public int CommandCapacity { get; set; }

        public int ResponseActivationLevel { get; set; }

        public int OrderSupportLevel { get; set; }

        public bool IsResponseActivated { get; set; }

        public bool HasActiveConflict { get; set; }

        public string LastConflictTrace { get; set; } = string.Empty;
    }

    public sealed class LegacyCampaignFrontStateV1
    {
        public CampaignId CampaignId { get; set; }

        public SettlementId AnchorSettlementId { get; set; }

        public string AnchorSettlementName { get; set; } = string.Empty;

        public string CampaignName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string ObjectiveSummary { get; set; } = string.Empty;

        public int MobilizedForceCount { get; set; }

        public int FrontPressure { get; set; }

        public int SupplyState { get; set; }

        public int MoraleState { get; set; }

        public string MobilizationWindowLabel { get; set; } = string.Empty;

        public string SupplyLineSummary { get; set; } = string.Empty;

        public string OfficeCoordinationTrace { get; set; } = string.Empty;

        public string SourceTrace { get; set; } = string.Empty;

        public string LastAftermathSummary { get; set; } = string.Empty;
    }

    public sealed class LegacyCampaignMobilizationSignalStateV1
    {
        public SettlementId SettlementId { get; set; }

        public string SettlementName { get; set; } = string.Empty;

        public int ResponseActivationLevel { get; set; }

        public int CommandCapacity { get; set; }

        public int Readiness { get; set; }

        public int AvailableForceCount { get; set; }

        public int OrderSupportLevel { get; set; }

        public int OfficeAuthorityTier { get; set; }

        public int AdministrativeLeverage { get; set; }

        public int PetitionBacklog { get; set; }

        public string MobilizationWindowLabel { get; set; } = string.Empty;

        public string OfficeCoordinationTrace { get; set; } = string.Empty;

        public string SourceTrace { get; set; } = string.Empty;
    }

    private sealed class TestMigrationModuleRunner : ModuleRunner<TestMigrationModuleState>
    {
        public override string ModuleKey => "Test.Migration";

        public override int ModuleSchemaVersion => 2;

        public override SimulationPhase Phase => SimulationPhase.Prepare;

        public override int ExecutionOrder => 1;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override TestMigrationModuleState CreateInitialState()
        {
            return new TestMigrationModuleState();
        }

        public override void RunMonth(ModuleExecutionScope<TestMigrationModuleState> scope)
        {
        }
    }

    private sealed class TestNamedModuleState : IModuleStateDescriptor
    {
        public string ModuleKey { get; init; } = string.Empty;
    }

    private sealed class TestNamedModuleRunner : ModuleRunner<TestNamedModuleState>
    {
        private readonly string _moduleKey;
        private readonly int _moduleSchemaVersion;

        public TestNamedModuleRunner(string moduleKey, int moduleSchemaVersion)
        {
            _moduleKey = moduleKey;
            _moduleSchemaVersion = moduleSchemaVersion;
        }

        public override string ModuleKey => _moduleKey;

        public override int ModuleSchemaVersion => _moduleSchemaVersion;

        public override SimulationPhase Phase => SimulationPhase.Prepare;

        public override int ExecutionOrder => 1;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override TestNamedModuleState CreateInitialState()
        {
            return new TestNamedModuleState
            {
                ModuleKey = _moduleKey,
            };
        }

        public override void RunMonth(ModuleExecutionScope<TestNamedModuleState> scope)
        {
        }
    }
}
