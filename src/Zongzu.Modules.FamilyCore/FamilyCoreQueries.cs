using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

internal sealed class FamilyCoreQueries : IFamilyCoreQueries
{
    private readonly FamilyCoreState _state;
    private readonly QueryRegistry _queries;

    public FamilyCoreQueries(FamilyCoreState state, QueryRegistry queries)
    {
        _state = state;
        _queries = queries;
    }

    private bool IsPersonAlive(FamilyPersonState person)
    {
        IPersonRegistryQueries registry = _queries.GetRequired<IPersonRegistryQueries>();
        if (registry.TryGetPerson(person.Id, out PersonRecord record))
        {
            return record.IsAlive;
        }

        return person.IsAlive;
    }

    public ClanSnapshot GetRequiredClan(ClanId clanId)
    {
        ClanStateData clan = _state.Clans.Single(clan => clan.Id == clanId);
        return Clone(_state, clan);
    }

    public IReadOnlyList<ClanSnapshot> GetClans()
    {
        return _state.Clans
            .OrderBy(static clan => clan.Id.Value)
            .Select(clan => Clone(_state, clan))
            .ToArray();
    }

    public FamilyPersonSnapshot? FindPerson(PersonId personId)
    {
        FamilyPersonState? person = _state.People.SingleOrDefault(person => person.Id == personId);
        return person is null ? null : ClonePerson(person);
    }

    public IReadOnlyList<FamilyPersonSnapshot> GetClanMembers(ClanId clanId)
    {
        return _state.People
            .Where(person => person.ClanId == clanId)
            .OrderBy(static person => person.Id.Value)
            .Select(ClonePerson)
            .ToArray();
    }

    private FamilyPersonSnapshot ClonePerson(FamilyPersonState person)
    {
        return new FamilyPersonSnapshot
        {
            Id = person.Id,
            ClanId = person.ClanId,
            GivenName = person.GivenName,
            AgeMonths = person.AgeMonths,
            IsAlive = IsPersonAlive(person),
            BranchPosition = person.BranchPosition,
            SpouseId = person.SpouseId,
            FatherId = person.FatherId,
            MotherId = person.MotherId,
            ChildrenIds = person.ChildrenIds.ToArray(),
            Ambition = person.Ambition,
            Prudence = person.Prudence,
            Loyalty = person.Loyalty,
            Sociability = person.Sociability,
        };
    }

    private ClanSnapshot Clone(FamilyCoreState state, ClanStateData clan)
    {
        int infantCount = state.People.Count(person =>
            person.ClanId == clan.Id
            && IsPersonAlive(person)
            && person.AgeMonths <= FamilyCoreModule.InfantAgeMonths);

        return new ClanSnapshot
        {
            Id = clan.Id,
            ClanName = clan.ClanName,
            HomeSettlementId = clan.HomeSettlementId,
            Prestige = clan.Prestige,
            SupportReserve = clan.SupportReserve,
            HeirPersonId = clan.HeirPersonId,
            BranchTension = clan.BranchTension,
            InheritancePressure = clan.InheritancePressure,
            SeparationPressure = clan.SeparationPressure,
            MediationMomentum = clan.MediationMomentum,
            BranchFavorPressure = clan.BranchFavorPressure,
            ReliefSanctionPressure = clan.ReliefSanctionPressure,
            MarriageAlliancePressure = clan.MarriageAlliancePressure,
            MarriageAllianceValue = clan.MarriageAllianceValue,
            HeirSecurity = clan.HeirSecurity,
            ReproductivePressure = clan.ReproductivePressure,
            MourningLoad = clan.MourningLoad,
            CareLoad = clan.CareLoad,
            FuneralDebt = clan.FuneralDebt,
            RemedyConfidence = clan.RemedyConfidence,
            CharityObligation = clan.CharityObligation,
            InfantCount = infantCount,
            LastConflictCommandCode = clan.LastConflictCommandCode,
            LastConflictCommandLabel = clan.LastConflictCommandLabel,
            LastConflictOutcome = clan.LastConflictOutcome,
            LastConflictTrace = clan.LastConflictTrace,
            LastRefusalResponseCommandCode = clan.LastRefusalResponseCommandCode,
            LastRefusalResponseCommandLabel = clan.LastRefusalResponseCommandLabel,
            LastRefusalResponseSummary = clan.LastRefusalResponseSummary,
            LastRefusalResponseOutcomeCode = clan.LastRefusalResponseOutcomeCode,
            LastRefusalResponseTraceCode = clan.LastRefusalResponseTraceCode,
            ResponseCarryoverMonths = clan.ResponseCarryoverMonths,
            LastLifecycleCommandCode = clan.LastLifecycleCommandCode,
            LastLifecycleCommandLabel = clan.LastLifecycleCommandLabel,
            LastLifecycleOutcome = clan.LastLifecycleOutcome,
            LastLifecycleTrace = clan.LastLifecycleTrace,
        };
    }
}
