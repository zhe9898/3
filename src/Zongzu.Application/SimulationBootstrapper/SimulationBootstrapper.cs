using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Application;

public static partial class SimulationBootstrapper
{
    private readonly record struct SeedFixture(SettlementId SettlementId, ClanId ClanId, PersonId HeirId);
    private readonly record struct StressSliceTemplate(
        string SettlementName,
        string ClanName,
        string HeirName,
        string AcademyName,
        string MarketName,
        string RouteName,
        string TenantHouseholdName,
        string FreeHouseholdName,
        int Security,
        int Prosperity,
        int ClanPrestige,
        int ClanSupport,
        int CommonerDistress,
        int LaborSupply,
        int MigrationPressure,
        int MilitiaPotential,
        int GrudgePressure,
        int FearPressure,
        int StudyProgress,
        int ScholarStress,
        int ScholarlyReputation,
        int CashReserve,
        int GrainReserve,
        int Debt,
        int CommerceReputation,
        int ShopCount,
        int ManagerSkill,
        int MarketDemand,
        int MarketRisk,
        int RouteCapacity,
        int RouteRisk,
        int BanditThreat,
        int RoutePressure,
        int SuppressionDemand,
        int DisorderPressure,
        int GuardCount,
        int RetainerCount,
        int MilitiaCount,
        int EscortCount,
        int Readiness,
        int CommandCapacity,
        string InitialConflictTrace);

}
