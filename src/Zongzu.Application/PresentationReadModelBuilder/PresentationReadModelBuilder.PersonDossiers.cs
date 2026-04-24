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

        return registryPeople
            .Select(person =>
            {
                FamilyPersonSnapshot? familyPerson = familyPeopleById.TryGetValue(person.Id, out FamilyPersonSnapshot? member)
                    ? member
                    : familyQueries?.FindPerson(person.Id);
                PersonPressureTemperingSnapshot? tempering = socialQueries?.FindPersonTempering(person.Id);
                ClanId? clanId = familyPerson?.ClanId ?? tempering?.ClanId;
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
                    MemoryPressureSummary = BuildMemoryPressureSummary(tempering, narrative, memories),
                    CurrentStatusSummary = BuildCurrentStatusSummary(person, clan, familyPerson, tempering),
                    SourceModuleKeys = BuildPersonDossierSourceKeys(familyPerson, clan, tempering, narrative, memories),
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

    private static string BuildMemoryPressureSummary(
        PersonPressureTemperingSnapshot? tempering,
        ClanNarrativeSnapshot? narrative,
        IReadOnlyList<SocialMemoryEntrySnapshot> memories)
    {
        if (tempering is null && narrative is null && memories.Count == 0)
        {
            return "No social-memory pressure projection.";
        }

        List<string> parts = [];
        if (tempering is not null)
        {
            string axes = string.Join(", ", new[]
                {
                    ("fear", tempering.Fear),
                    ("shame", tempering.Shame),
                    ("grief", tempering.Grief),
                    ("anger", tempering.Anger),
                    ("obligation", tempering.Obligation),
                    ("hope", tempering.Hope),
                    ("trust", tempering.Trust),
                }
                .OrderByDescending(static axis => axis.Item2)
                .ThenBy(static axis => axis.Item1, StringComparer.Ordinal)
                .Take(2)
                .Select(static axis => $"{axis.Item1} {axis.Item2}"));
            parts.Add($"pressure {tempering.LastPressureScore}; {axes}");
        }

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

    private static string BuildCurrentStatusSummary(
        PersonRecord person,
        ClanSnapshot? clan,
        FamilyPersonSnapshot? familyPerson,
        PersonPressureTemperingSnapshot? tempering)
    {
        string life = person.IsAlive ? "Living" : "Deceased";
        string clanText = clan is null ? "no clan projection" : $"clan {clan.ClanName}";
        string branchText = familyPerson is null ? "no family placement" : BuildBranchPositionLabel(familyPerson.BranchPosition);
        string memoryText = tempering is null ? "no person tempering" : $"pressure {tempering.LastPressureScore}";
        return $"{life} {person.LifeStage}; {person.FidelityRing} ring; {clanText}; {branchText}; {memoryText}.";
    }

    private static IReadOnlyList<string> BuildPersonDossierSourceKeys(
        FamilyPersonSnapshot? familyPerson,
        ClanSnapshot? clan,
        PersonPressureTemperingSnapshot? tempering,
        ClanNarrativeSnapshot? narrative,
        IReadOnlyList<SocialMemoryEntrySnapshot> memories)
    {
        List<string> keys = [KnownModuleKeys.PersonRegistry];
        if (familyPerson is not null || clan is not null)
        {
            keys.Add(KnownModuleKeys.FamilyCore);
        }

        if (tempering is not null || narrative is not null || memories.Count > 0)
        {
            keys.Add(KnownModuleKeys.SocialMemoryAndRelations);
        }

        return keys.Distinct(StringComparer.Ordinal).ToArray();
    }

}
