using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IReadOnlyList<PersonDossierSnapshot> BuildPersonDossiers(
        FeatureManifest manifest,
        QueryRegistry queries)
    {
        if (!manifest.IsEnabled(KnownModuleKeys.PersonRegistry))
        {
            return [];
        }

        IPersonRegistryQueries registryQueries = queries.GetRequired<IPersonRegistryQueries>();
        IFamilyCoreQueries? familyQueries = manifest.IsEnabled(KnownModuleKeys.FamilyCore)
            ? queries.GetRequired<IFamilyCoreQueries>()
            : null;
        ISocialMemoryAndRelationsQueries? socialQueries = manifest.IsEnabled(KnownModuleKeys.SocialMemoryAndRelations)
            ? queries.GetRequired<ISocialMemoryAndRelationsQueries>()
            : null;
        IPopulationAndHouseholdsQueries? populationQueries = manifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds)
            ? queries.GetRequired<IPopulationAndHouseholdsQueries>()
            : null;
        IEducationAndExamsQueries? educationQueries = manifest.IsEnabled(KnownModuleKeys.EducationAndExams)
            ? queries.GetRequired<IEducationAndExamsQueries>()
            : null;
        ITradeAndIndustryQueries? tradeQueries = manifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)
            ? queries.GetRequired<ITradeAndIndustryQueries>()
            : null;
        IOfficeAndCareerQueries? officeQueries = manifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)
            ? queries.GetRequired<IOfficeAndCareerQueries>()
            : null;

        IReadOnlyList<PersonRecord> registryPeople = registryQueries.GetAllPersons();
        Dictionary<PersonId, PersonRecord> registryById = registryPeople
            .GroupBy(static person => person.Id)
            .ToDictionary(static group => group.Key, static group => group.First());

        IReadOnlyList<ClanSnapshot> clans = familyQueries?.GetClans() ?? [];
        Dictionary<ClanId, ClanSnapshot> clansById = clans.ToDictionary(static clan => clan.Id);

        Dictionary<PersonId, FamilyPersonSnapshot> familyPeopleById = [];
        if (familyQueries is not null)
        {
            foreach (ClanSnapshot clan in clans.OrderBy(static clan => clan.Id.Value))
            {
                foreach (FamilyPersonSnapshot member in familyQueries.GetClanMembers(clan.Id)
                             .OrderBy(static member => member.Id.Value))
                {
                    familyPeopleById.TryAdd(member.Id, member);
                }
            }
        }

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = socialQueries is null
            ? []
            : socialQueries.GetClanNarratives().ToDictionary(static narrative => narrative.ClanId);
        Dictionary<PersonId, DormantStubSnapshot> dormantStubsByPerson = socialQueries is null
            ? []
            : socialQueries.GetDormantStubs()
                .GroupBy(static stub => stub.PersonId)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<MemoryId, SocialMemoryEntrySnapshot> memoriesById = socialQueries is null
            ? []
            : socialQueries.GetMemories()
                .GroupBy(static memory => memory.Id)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<HouseholdId, HouseholdPressureSnapshot> householdsById = populationQueries is null
            ? []
            : populationQueries.GetHouseholds()
                .GroupBy(static household => household.Id)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<PersonId, HouseholdMembershipSnapshot> membershipsByPerson = populationQueries is null
            ? []
            : populationQueries.GetMemberships()
                .GroupBy(static membership => membership.PersonId)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<PersonId, EducationCandidateSnapshot> educationByPerson = educationQueries is null
            ? []
            : educationQueries.GetCandidates()
                .GroupBy(static candidate => candidate.PersonId)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<ClanId, ClanTradeSnapshot> tradesByClan = tradeQueries is null
            ? []
            : tradeQueries.GetClanTrades()
                .GroupBy(static trade => trade.ClanId)
                .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<PersonId, OfficeCareerSnapshot> officeByPerson = officeQueries is null
            ? []
            : officeQueries.GetCareers()
                .GroupBy(static career => career.PersonId)
                .ToDictionary(static group => group.Key, static group => group.First());

        return registryPeople
            .Select(person =>
            {
                FamilyPersonSnapshot? familyPerson = familyPeopleById.TryGetValue(person.Id, out FamilyPersonSnapshot? member)
                    ? member
                    : familyQueries?.FindPerson(person.Id);
                ClanId? clanId = familyPerson?.ClanId;
                ClanSnapshot? clan = clanId.HasValue && clansById.TryGetValue(clanId.Value, out ClanSnapshot? clanSnapshot)
                    ? clanSnapshot
                    : null;
                ClanNarrativeSnapshot? narrative = clanId.HasValue &&
                    narrativesByClan.TryGetValue(clanId.Value, out ClanNarrativeSnapshot? clanNarrative)
                        ? clanNarrative
                        : null;
                IReadOnlyList<SocialMemoryEntrySnapshot> memories = clanId.HasValue && socialQueries is not null
                    ? socialQueries.GetMemoriesByClan(clanId.Value)
                    : [];
                HouseholdMembershipSnapshot? membership = membershipsByPerson.TryGetValue(person.Id, out HouseholdMembershipSnapshot? personMembership)
                    ? personMembership
                    : null;
                HouseholdPressureSnapshot? household = membership is not null &&
                    householdsById.TryGetValue(membership.HouseholdId, out HouseholdPressureSnapshot? householdSnapshot)
                        ? householdSnapshot
                        : null;
                EducationCandidateSnapshot? education = educationByPerson.TryGetValue(person.Id, out EducationCandidateSnapshot? educationCandidate)
                    ? educationCandidate
                    : null;
                ClanTradeSnapshot? trade = clanId.HasValue &&
                    tradesByClan.TryGetValue(clanId.Value, out ClanTradeSnapshot? clanTrade)
                        ? clanTrade
                        : null;
                OfficeCareerSnapshot? office = officeByPerson.TryGetValue(person.Id, out OfficeCareerSnapshot? officeCareer)
                    ? officeCareer
                    : null;
                DormantStubSnapshot? dormantStub = dormantStubsByPerson.TryGetValue(person.Id, out DormantStubSnapshot? stub)
                    ? stub
                    : null;

                return new PersonDossierSnapshot
                {
                    PersonId = person.Id,
                    DisplayName = person.DisplayName,
                    LifeStage = person.LifeStage,
                    Gender = person.Gender,
                    IsAlive = person.IsAlive,
                    FidelityRing = person.FidelityRing,
                    ClanId = clanId,
                    ClanName = clan?.ClanName ?? string.Empty,
                    BranchPositionLabel = BuildBranchPositionLabel(familyPerson?.BranchPosition),
                    KinshipSummary = BuildKinshipSummary(familyPerson, registryById),
                    TemperamentSummary = BuildTemperamentSummary(familyPerson),
                    HouseholdId = membership?.HouseholdId,
                    HouseholdName = household?.HouseholdName ?? string.Empty,
                    LivelihoodSummary = BuildLivelihoodSummary(membership, household),
                    HealthSummary = BuildHealthSummary(membership),
                    ActivitySummary = BuildActivitySummary(membership, household),
                    MovementReadbackSummary = BuildPersonMovementReadbackSummary(membership, household),
                    FidelityRingReadbackSummary = BuildPersonFidelityRingReadbackSummary(person, membership, household),
                    EducationSummary = BuildEducationSummary(education),
                    TradeSummary = BuildTradeSummary(trade),
                    OfficeSummary = BuildOfficeSummary(office),
                    MemoryPressureSummary = BuildMemoryPressureSummary(narrative, memories),
                    DormantMemorySummary = BuildDormantMemorySummary(dormantStub, memoriesById),
                    SocialPositionLabel = BuildSocialPositionLabel(familyPerson, membership, education, trade, office, dormantStub),
                    CurrentStatusSummary = BuildCurrentStatusSummary(
                        person,
                        clan,
                        familyPerson,
                        membership,
                        education,
                        office,
                        narrative,
                        memories,
                        dormantStub),
                    SourceModuleKeys = BuildPersonDossierSourceKeys(
                        familyPerson,
                        clan,
                        membership,
                        education,
                        trade,
                        office,
                        narrative,
                        memories,
                        dormantStub),
                };
            })
            .OrderBy(static dossier => dossier.IsAlive ? 0 : 1)
            .ThenBy(static dossier => FidelityRingRank(dossier.FidelityRing))
            .ThenBy(static dossier => string.IsNullOrWhiteSpace(dossier.ClanName) ? 1 : 0)
            .ThenBy(static dossier => dossier.ClanName, StringComparer.Ordinal)
            .ThenBy(static dossier => dossier.DisplayName, StringComparer.Ordinal)
            .ThenBy(static dossier => dossier.PersonId.Value)
            .ToArray();
    }

    private static int FidelityRingRank(FidelityRing ring)
    {
        return ring switch
        {
            FidelityRing.Core => 0,
            FidelityRing.Local => 1,
            FidelityRing.Regional => 2,
            _ => 3,
        };
    }

    private static string BuildBranchPositionLabel(BranchPosition? branchPosition)
    {
        return branchPosition switch
        {
            BranchPosition.MainLineHeir => "Main-line heir",
            BranchPosition.BranchHead => "Branch head",
            BranchPosition.BranchMember => "Branch member",
            BranchPosition.DependentKin => "Dependent kin",
            BranchPosition.MarriedOut => "Married-out kin",
            BranchPosition.Unknown => "Unplaced clan member",
            _ => "No family branch projection.",
        };
    }

    private static string BuildKinshipSummary(
        FamilyPersonSnapshot? familyPerson,
        IReadOnlyDictionary<PersonId, PersonRecord> registryById)
    {
        if (familyPerson is null)
        {
            return "No clan kinship projection.";
        }

        string father = FormatKinName("father", familyPerson.FatherId, registryById);
        string mother = FormatKinName("mother", familyPerson.MotherId, registryById);
        string spouse = FormatKinName("spouse", familyPerson.SpouseId, registryById);
        string children = familyPerson.ChildrenIds.Count == 0
            ? "children 0"
            : $"children {familyPerson.ChildrenIds.Count}";

        return string.Join("; ", DistinctNonEmpty(father, mother, spouse, children));
    }

    private static string FormatKinName(
        string label,
        PersonId? personId,
        IReadOnlyDictionary<PersonId, PersonRecord> registryById)
    {
        if (!personId.HasValue)
        {
            return string.Empty;
        }

        return registryById.TryGetValue(personId.Value, out PersonRecord? person) &&
            !string.IsNullOrWhiteSpace(person.DisplayName)
                ? $"{label} {person.DisplayName}"
                : $"{label} #{personId.Value.Value}";
    }

    private static string BuildTemperamentSummary(FamilyPersonSnapshot? familyPerson)
    {
        if (familyPerson is null)
        {
            return "No family temperament projection.";
        }

        return $"ambition {familyPerson.Ambition}, prudence {familyPerson.Prudence}, " +
               $"loyalty {familyPerson.Loyalty}, sociability {familyPerson.Sociability}";
    }

    private static string BuildLivelihoodSummary(
        HouseholdMembershipSnapshot? membership,
        HouseholdPressureSnapshot? household)
    {
        if (membership is null)
        {
            return "No household livelihood projection.";
        }

        string householdText = household is null
            ? $"household #{membership.HouseholdId.Value}"
            : household.HouseholdName;
        string pressureText = household is null
            ? "household pressure unknown"
            : $"distress {household.Distress}, debt {household.DebtPressure}, migration risk {household.MigrationRisk}";
        return $"{householdText}; livelihood {membership.Livelihood}; {pressureText}";
    }

    private static string BuildHealthSummary(HouseholdMembershipSnapshot? membership)
    {
        if (membership is null)
        {
            return "No household health projection.";
        }

        string illness = membership.IllnessMonths <= 0
            ? "no active illness months"
            : $"illness months {membership.IllnessMonths}";
        return $"health {membership.Health}; resilience {membership.HealthResilience}; {illness}";
    }

    private static string BuildActivitySummary(
        HouseholdMembershipSnapshot? membership,
        HouseholdPressureSnapshot? household)
    {
        if (membership is null)
        {
            return "No household activity projection.";
        }

        string capacity = household is null
            ? string.Empty
            : $"labor {household.LaborCapacity}; dependents {household.DependentCount}";
        return string.Join("; ", DistinctNonEmpty($"activity {membership.Activity}", capacity));
    }

    private static string BuildPersonMovementReadbackSummary(
        HouseholdMembershipSnapshot? membership,
        HouseholdPressureSnapshot? household)
    {
        if (membership is null || household is null)
        {
            return "No population movement readback.";
        }

        if (membership.Activity == PersonActivity.Migrating || household.IsMigrating)
        {
            return $"{household.HouseholdName}迁徙风险{household.MigrationRisk}，此人作为近处人物读迁徙活动；远处同类仍由流徙池承接。";
        }

        return $"{household.HouseholdName}当前活动{membership.Activity}，生计{membership.Livelihood}；这是人口模块读回，不是UI推导。";
    }

    private static string BuildPersonFidelityRingReadbackSummary(
        PersonRecord person,
        HouseholdMembershipSnapshot? membership,
        HouseholdPressureSnapshot? household)
    {
        string ringText = person.FidelityRing switch
        {
            FidelityRing.Core => "核心近处",
            FidelityRing.Local => "地方近处",
            FidelityRing.Regional => "远处汇总",
            _ => "未明精度",
        };

        if (household is not null && (household.IsMigrating || household.MigrationRisk >= 80))
        {
            return $"{ringText}：迁徙压力可把少量人物拉进近处读回，但不把天下逐人硬算。";
        }

        if (membership is null)
        {
            return $"{ringText}：当前只有身份登记，未接家户活动。";
        }

        return $"{ringText}：人物精度来自PersonRegistry，家户活动来自PopulationAndHouseholds。";
    }

    private static string BuildEducationSummary(EducationCandidateSnapshot? education)
    {
        if (education is null)
        {
            return "No education projection.";
        }

        string status = education.HasPassedLocalExam
            ? "local exam passed"
            : education.IsStudying ? "studying" : "not currently studying";
        string tutor = education.HasTutor ? "has tutor" : "no tutor";
        return $"{status}; tier {education.CurrentTier}; progress {education.StudyProgress}; stress {education.Stress}; {tutor}";
    }

    private static string BuildTradeSummary(ClanTradeSnapshot? trade)
    {
        if (trade is null)
        {
            return "No clan trade projection.";
        }

        string outcome = string.IsNullOrWhiteSpace(trade.LastOutcome)
            ? "no recent trade outcome"
            : $"last outcome {trade.LastOutcome}";
        return $"clan trade cash {trade.CashReserve}, grain {trade.GrainReserve}, debt {trade.Debt}; shops {trade.ShopCount}; {outcome}";
    }

    private static string BuildOfficeSummary(OfficeCareerSnapshot? office)
    {
        if (office is null)
        {
            return "No office projection.";
        }

        string status = office.HasAppointment
            ? $"appointed {office.OfficeTitle}"
            : office.IsEligible ? "eligible for office path" : "office-listed without appointment";
        string task = string.IsNullOrWhiteSpace(office.CurrentAdministrativeTask)
            ? "no current administrative task"
            : $"task {office.CurrentAdministrativeTask}";
        return $"{status}; authority {office.AuthorityTier}; petitions {office.PetitionPressure}/{office.PetitionBacklog}; {task}";
    }

    private static string BuildMemoryPressureSummary(
        ClanNarrativeSnapshot? narrative,
        IReadOnlyList<SocialMemoryEntrySnapshot> memories)
    {
        if (narrative is null && memories.Count == 0)
        {
            return "No social-memory pressure projection.";
        }

        List<string> parts = [];
        if (narrative is not null)
        {
            parts.Add($"clan memory count {narrative.MemoryCount}; grudge {narrative.GrudgePressure}; fear {narrative.FearPressure}");
        }
        else if (memories.Count > 0)
        {
            parts.Add($"clan memory count {memories.Count}");
        }

        return string.Join("; ", parts);
    }

    private static string BuildDormantMemorySummary(
        DormantStubSnapshot? dormantStub,
        IReadOnlyDictionary<MemoryId, SocialMemoryEntrySnapshot> memoriesById)
    {
        if (dormantStub is null)
        {
            return "No dormant social-memory stub.";
        }

        string role = string.IsNullOrWhiteSpace(dormantStub.LastKnownRole)
            ? "last role unknown"
            : $"last role {dormantStub.LastKnownRole}";
        string rememberedCauses = string.Join(
            ", ",
            dormantStub.ActiveMemoryIds
                .Select(memoryId => memoriesById.TryGetValue(memoryId, out SocialMemoryEntrySnapshot? memory)
                    ? memory.CauseKey
                    : memoryId.Value.ToString())
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Take(3));
        string hooks = string.IsNullOrWhiteSpace(rememberedCauses)
            ? $"active memory hooks {dormantStub.ActiveMemoryIds.Count}"
            : $"active memory hooks {dormantStub.ActiveMemoryIds.Count}: {rememberedCauses}";
        string reemergence = dormantStub.IsEligibleForReemergence
            ? "eligible for reemergence"
            : "not eligible for reemergence";
        return $"{role}; last seen {dormantStub.LastSeen}; {hooks}; {reemergence}";
    }

    private static string BuildSocialPositionLabel(
        FamilyPersonSnapshot? familyPerson,
        HouseholdMembershipSnapshot? membership,
        EducationCandidateSnapshot? education,
        ClanTradeSnapshot? trade,
        OfficeCareerSnapshot? office,
        DormantStubSnapshot? dormantStub)
    {
        string branch = familyPerson is null ? string.Empty : BuildBranchPositionLabel(familyPerson.BranchPosition);
        string officeText = office is null
            ? string.Empty
            : office.HasAppointment ? office.OfficeTitle : office.IsEligible ? "office candidate" : string.Empty;
        string study = education is null
            ? string.Empty
            : education.HasPassedLocalExam ? "local-exam passer" : education.IsStudying ? "student" : string.Empty;
        string livelihood = membership is null ? string.Empty : membership.Livelihood.ToString();
        string tradeText = trade is null || trade.ShopCount <= 0 ? string.Empty : $"clan shops {trade.ShopCount}";
        string dormant = dormantStub is null || string.IsNullOrWhiteSpace(dormantStub.LastKnownRole)
            ? string.Empty
            : dormantStub.LastKnownRole;

        string label = string.Join(", ", DistinctNonEmpty(branch, officeText, study, livelihood, tradeText, dormant));
        return string.IsNullOrWhiteSpace(label) ? "Registry-only person." : label;
    }

    private static string BuildCurrentStatusSummary(
        PersonRecord person,
        ClanSnapshot? clan,
        FamilyPersonSnapshot? familyPerson,
        HouseholdMembershipSnapshot? membership,
        EducationCandidateSnapshot? education,
        OfficeCareerSnapshot? office,
        ClanNarrativeSnapshot? narrative,
        IReadOnlyList<SocialMemoryEntrySnapshot> memories,
        DormantStubSnapshot? dormantStub)
    {
        string life = person.IsAlive ? "Living" : "Deceased";
        string clanText = clan is null ? "no clan projection" : $"clan {clan.ClanName}";
        string branchText = familyPerson is null ? "no family placement" : BuildBranchPositionLabel(familyPerson.BranchPosition);
        string householdText = membership is null ? "no household projection" : $"household {membership.HouseholdId.Value}";
        string educationText = education is null
            ? "no education projection"
            : education.HasPassedLocalExam ? "local exam passed" : education.IsStudying ? "studying" : "education record";
        string officeText = office is null
            ? "no office projection"
            : office.HasAppointment ? $"office {office.OfficeTitle}" : office.IsEligible ? "office eligible" : "office record";
        string memoryText = narrative is null && memories.Count == 0
            ? "no social-memory projection"
            : $"social memory entries {narrative?.MemoryCount ?? memories.Count}";
        string dormantText = dormantStub is null ? string.Empty : "dormant memory stub present";
        return string.Join(
            "; ",
            DistinctNonEmpty(
                $"{life} {person.LifeStage}",
                $"{person.FidelityRing} ring",
                clanText,
                branchText,
                householdText,
                educationText,
                officeText,
                memoryText,
                dormantText)) + ".";
    }

    private static IReadOnlyList<string> BuildPersonDossierSourceKeys(
        FamilyPersonSnapshot? familyPerson,
        ClanSnapshot? clan,
        HouseholdMembershipSnapshot? membership,
        EducationCandidateSnapshot? education,
        ClanTradeSnapshot? trade,
        OfficeCareerSnapshot? office,
        ClanNarrativeSnapshot? narrative,
        IReadOnlyList<SocialMemoryEntrySnapshot> memories,
        DormantStubSnapshot? dormantStub)
    {
        List<string> keys = [KnownModuleKeys.PersonRegistry];
        if (familyPerson is not null || clan is not null)
        {
            keys.Add(KnownModuleKeys.FamilyCore);
        }

        if (membership is not null)
        {
            keys.Add(KnownModuleKeys.PopulationAndHouseholds);
        }

        if (education is not null)
        {
            keys.Add(KnownModuleKeys.EducationAndExams);
        }

        if (trade is not null)
        {
            keys.Add(KnownModuleKeys.TradeAndIndustry);
        }

        if (office is not null)
        {
            keys.Add(KnownModuleKeys.OfficeAndCareer);
        }

        if (narrative is not null || memories.Count > 0 || dormantStub is not null)
        {
            keys.Add(KnownModuleKeys.SocialMemoryAndRelations);
        }

        return keys.Distinct(StringComparer.Ordinal).ToArray();
    }

}
