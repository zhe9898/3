using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private const int SocialAdultAgeMonths = 16 * 12;

    private static SocialPressureInput BuildPressureInput(
        ClanSnapshot clan,
        IReadOnlyCollection<HouseholdPressureSnapshot> sponsoredHouseholds,
        ClanTradeSnapshot? trade)
    {
        int averageDistress = sponsoredHouseholds.Count == 0
            ? 0
            : sponsoredHouseholds.Sum(static household => household.Distress) / sponsoredHouseholds.Count;
        int averageMigrationRisk = sponsoredHouseholds.Count == 0
            ? 0
            : sponsoredHouseholds.Sum(static household => household.MigrationRisk) / sponsoredHouseholds.Count;
        int householdDebt = sponsoredHouseholds.Count == 0
            ? 0
            : sponsoredHouseholds.Sum(static household => household.DebtPressure) / sponsoredHouseholds.Count;
        int tradeDebt = trade is null ? 0 : Math.Clamp(trade.Debt / 3, 0, 100);
        int tradeReputationLoss = trade is null ? 0 : Math.Clamp(55 - trade.CommerceReputation, 0, 100);

        return new SocialPressureInput(
            averageDistress,
            averageMigrationRisk,
            sponsoredHouseholds.Any(static household => household.IsMigrating),
            householdDebt,
            tradeDebt,
            tradeReputationLoss,
            Math.Clamp(clan.BranchTension + clan.InheritancePressure + clan.SeparationPressure, 0, 300) / 3,
            clan.Prestige < 50 ? 50 - clan.Prestige : 0,
            clan.SupportReserve,
            clan.MediationMomentum,
            clan.ReliefSanctionPressure,
            clan.BranchFavorPressure,
            clan.MourningLoad,
            clan.CareLoad,
            clan.FuneralDebt,
            clan.CharityObligation,
            clan.HeirSecurity);
    }

    private static TraitAverages BuildTraitAverages(IReadOnlyCollection<FamilyPersonSnapshot> members)
    {
        FamilyPersonSnapshot[] livingAdults = members
            .Where(static member => member.IsAlive && member.AgeMonths >= SocialAdultAgeMonths)
            .OrderBy(static member => member.Id.Value)
            .ToArray();

        if (livingAdults.Length == 0)
        {
            return new TraitAverages(50, 50, 50, 50);
        }

        return new TraitAverages(
            livingAdults.Sum(static member => member.Ambition) / livingAdults.Length,
            livingAdults.Sum(static member => member.Prudence) / livingAdults.Length,
            livingAdults.Sum(static member => member.Loyalty) / livingAdults.Length,
            livingAdults.Sum(static member => member.Sociability) / livingAdults.Length);
    }

    private static void ApplyXunTemperingPulse(
        SimulationXun currentXun,
        SocialPressureInput input,
        TraitAverages traits,
        ClanEmotionalClimateState climate,
        GameDate currentDate)
    {
        int pressure = ComputeCompositePressure(input);
        switch (currentXun)
        {
            case SimulationXun.Shangxun:
                climate.Fear = ClampPressure(climate.Fear + (pressure >= 55 ? 1 : pressure < 35 ? -1 : 0) + (input.HasMigratingHousehold ? 1 : 0));
                climate.Anger = ClampPressure(climate.Anger + (input.LineageTension >= 60 ? 1 : input.LineageTension < 35 ? -1 : 0));
                break;
            case SimulationXun.Zhongxun:
                climate.Shame = ClampPressure(climate.Shame + (input.PrestigeDeficit >= 8 ? 1 : -1) + (input.TradeReputationLoss >= 35 ? 1 : 0));
                climate.Trust = ClampPressure(climate.Trust + (input.SupportReserve >= 65 ? 1 : input.SupportReserve < 38 ? -1 : 0));
                climate.Obligation = ClampPressure(climate.Obligation + (input.CharityObligation >= 45 ? 1 : 0) + (input.SupportReserve >= 65 ? 1 : 0));
                break;
            case SimulationXun.Xiaxun:
                climate.Hardening = ClampPressure(climate.Hardening + (pressure >= 60 ? 1 : 0) + (traits.Prudence >= 65 ? 1 : 0));
                climate.Restraint = ClampPressure(climate.Restraint + (traits.Prudence >= 60 ? 1 : 0) + (input.MediationMomentum >= 50 ? 1 : 0) - (climate.Anger >= 70 ? 1 : 0));
                climate.Bitterness = ClampPressure(climate.Bitterness + (pressure >= 70 && climate.Trust < 45 ? 1 : 0) - (climate.Trust >= 60 ? 1 : 0));
                climate.Volatility = ClampPressure(climate.Volatility + (climate.Fear + climate.Anger >= 120 ? 1 : 0) - (climate.Restraint >= 55 ? 1 : 0));
                break;
        }

        climate.LastPressureScore = pressure;
        climate.LastUpdated = currentDate;
        climate.LastTrace = BuildPressureTrace(input, pressure);
    }

    private static TemperingOutcome ApplyMonthlyTempering(
        SocialPressureInput input,
        TraitAverages traits,
        ClanEmotionalClimateState climate,
        GameDate currentDate)
    {
        int previousPressureBand = climate.LastPressureBand;
        int previousTemperingBand = climate.LastTemperingBand;
        int pressure = ComputeCompositePressure(input);

        climate.Fear = ClampPressure(Decay(climate.Fear, 1) + ScaleOver(input.AverageDistress, 55, 3) + ScaleOver(input.AverageMigrationRisk, 60, 2) + ScaleOver(input.MourningLoad, 30, 2));
        climate.Shame = ClampPressure(Decay(climate.Shame, 1) + ScaleOver(input.PrestigeDeficit, 8, 3) + ScaleOver(input.ReliefSanctionPressure, 35, 2) + ScaleOver(input.HouseholdDebtPressure, 70, 1) + ScaleOver(input.TradeReputationLoss, 30, 2));
        climate.Grief = ClampPressure(Decay(climate.Grief, 1) + ScaleOver(input.MourningLoad, 15, 3) + ScaleOver(input.FuneralDebt, 30, 2) + ScaleOver(input.CareLoad, 70, 1));
        climate.Anger = ClampPressure(Decay(climate.Anger, 1) + ScaleOver(input.LineageTension, 55, 3) + ScaleOver(input.ReliefSanctionPressure, 45, 2) + ScaleOver(input.TradeDebtPressure, 60, 1) - ScaleOver(input.MediationMomentum, 45, 2));
        climate.Obligation = ClampPressure(Decay(climate.Obligation, 1) + ScaleOver(input.CharityObligation, 35, 2) + ScaleOver(input.SupportReserve, 65, 2) + ScaleOver(input.MediationMomentum, 50, 1));
        climate.Hope = ClampPressure(Decay(climate.Hope, 1) + ScaleOver(input.SupportReserve, 58, 2) + ScaleOver(input.HeirSecurity, 55, 1) - ScaleOver(input.AverageDistress, 75, 2) - ScaleOver(input.MourningLoad, 55, 1));
        climate.Trust = ClampPressure(Decay(climate.Trust, 1) + ScaleOver(input.SupportReserve, 65, 3) + ScaleOver(input.MediationMomentum, 45, 2) - ScaleOver(input.ReliefSanctionPressure, 50, 2) - ScaleOver(input.ShameSoil, 65, 1));

        climate.Hardening = ClampPressure(climate.Hardening + ScaleOver(pressure, 45, 3) + ScaleOver(traits.Prudence, 65, 1) + ScaleOver(traits.Loyalty, 70, 1));
        climate.Restraint = ClampPressure(Decay(climate.Restraint, 1) + ScaleOver(traits.Prudence, 58, 2) + ScaleOver(traits.Sociability, 62, 1) + ScaleOver(input.MediationMomentum, 45, 2) - ScaleOver(climate.Anger, 70, 2));
        climate.Bitterness = ClampPressure(Decay(climate.Bitterness, 1) + ScaleOver(climate.Anger + climate.Shame + input.LineageTension, 150, 3) + ScaleOver(input.ReliefSanctionPressure, 55, 1) - ScaleOver(climate.Trust, 60, 2));
        climate.Volatility = ClampPressure(Decay(climate.Volatility, 1) + ScaleOver(climate.Fear + climate.Anger + climate.Shame, 150, 3) + (traits.Prudence < 40 ? 1 : 0) - ScaleOver(climate.Restraint, 55, 2));

        int pressureBand = ResolveBand(pressure);
        int temperingScore = ComputeTemperingScore(climate);
        int temperingBand = ResolveBand(temperingScore);
        climate.LastPressureScore = pressure;
        climate.LastPressureBand = pressureBand;
        climate.LastTemperingBand = temperingBand;
        climate.LastUpdated = currentDate;
        climate.LastTrace = BuildPressureTrace(input, pressure);

        return new TemperingOutcome(
            pressure,
            pressureBand,
            temperingScore,
            temperingBand,
            pressureBand > previousPressureBand,
            temperingBand > previousTemperingBand);
    }

    private static void ApplyPersonPressureTempering(
        SocialMemoryAndRelationsState state,
        ClanSnapshot clan,
        IReadOnlyCollection<FamilyPersonSnapshot> members,
        ClanEmotionalClimateState climate,
        SocialPressureInput input,
        GameDate currentDate)
    {
        foreach (FamilyPersonSnapshot person in members
                     .Where(static member => member.IsAlive && member.AgeMonths >= SocialAdultAgeMonths)
                     .OrderBy(static member => member.Id.Value))
        {
            PersonPressureTemperingState tempering = GetOrCreatePersonTempering(state, person.Id, clan.Id);
            int personalPressure = Math.Clamp(
                climate.LastPressureScore
                + ((person.Ambition - 50) / 6)
                - ((person.Prudence - 50) / 8)
                + (clan.HeirPersonId == person.Id && input.HeirSecurity < 45 ? 8 : 0),
                0,
                100);

            tempering.ClanId = clan.Id;
            tempering.Fear = ClampPressure(Decay(tempering.Fear, 1) + ScaleOver(climate.Fear, 45, 2) - ScaleOver(person.Prudence, 70, 1));
            tempering.Shame = ClampPressure(Decay(tempering.Shame, 1) + ScaleOver(climate.Shame, 45, 2) + ScaleOver(person.Ambition, 70, 1) - ScaleOver(person.Sociability, 70, 1));
            tempering.Grief = ClampPressure(Decay(tempering.Grief, 1) + ScaleOver(climate.Grief, 45, 2));
            tempering.Anger = ClampPressure(Decay(tempering.Anger, 1) + ScaleOver(climate.Anger, 45, 2) + ScaleOver(person.Ambition, 75, 1) - ScaleOver(person.Prudence, 60, 2));
            tempering.Obligation = ClampPressure(Decay(tempering.Obligation, 1) + ScaleOver(climate.Obligation, 45, 2) + ScaleOver(person.Loyalty, 60, 2) - ScaleOver(tempering.Bitterness, 70, 1));
            tempering.Hope = ClampPressure(Decay(tempering.Hope, 1) + ScaleOver(climate.Hope, 45, 2) + ScaleOver(person.Ambition, 60, 1) - ScaleOver(tempering.Fear, 75, 1));
            tempering.Trust = ClampPressure(Decay(tempering.Trust, 1) + ScaleOver(climate.Trust, 45, 2) + ScaleOver(person.Sociability, 60, 1) + ScaleOver(person.Loyalty, 70, 1) - ScaleOver(tempering.Shame, 75, 1));
            tempering.Hardening = ClampPressure(tempering.Hardening + ScaleOver(personalPressure, 45, 3) + ScaleOver(person.Prudence, 65, 1));
            tempering.Restraint = ClampPressure(Decay(tempering.Restraint, 1) + ScaleOver(person.Prudence, 55, 2) + ScaleOver(person.Sociability, 65, 1) + ScaleOver(tempering.Trust, 55, 1) - ScaleOver(tempering.Anger, 72, 2));
            tempering.Bitterness = ClampPressure(Decay(tempering.Bitterness, 1) + ScaleOver(tempering.Anger + tempering.Shame + tempering.Fear, 130, 2) + (person.Loyalty < 35 ? 1 : 0) - ScaleOver(tempering.Trust, 60, 2));
            tempering.Volatility = ClampPressure(Decay(tempering.Volatility, 1) + ScaleOver(tempering.Anger + tempering.Fear, 120, 2) + (person.Ambition >= 75 ? 1 : 0) - ScaleOver(person.Prudence, 65, 2));
            tempering.LastPressureScore = personalPressure;
            tempering.LastUpdated = currentDate;
            tempering.LastTrace = $"{person.GivenName}: pressure {personalPressure}, prudence {person.Prudence}, loyalty {person.Loyalty}, ambition {person.Ambition}";
        }

        state.PersonTemperings = state.PersonTemperings
            .OrderBy(static entry => entry.PersonId.Value)
            .ToList();
    }

    private static ClanEmotionalClimateState GetOrCreateClimate(SocialMemoryAndRelationsState state, ClanId clanId)
    {
        ClanEmotionalClimateState? climate = state.ClanEmotionalClimates.SingleOrDefault(existing => existing.ClanId == clanId);
        if (climate is not null)
        {
            return climate;
        }

        climate = new ClanEmotionalClimateState { ClanId = clanId };
        state.ClanEmotionalClimates.Add(climate);
        state.ClanEmotionalClimates = state.ClanEmotionalClimates.OrderBy(static entry => entry.ClanId.Value).ToList();
        return climate;
    }

    private static PersonPressureTemperingState GetOrCreatePersonTempering(SocialMemoryAndRelationsState state, PersonId personId, ClanId clanId)
    {
        PersonPressureTemperingState? tempering = state.PersonTemperings.SingleOrDefault(existing => existing.PersonId == personId);
        if (tempering is not null)
        {
            return tempering;
        }

        tempering = new PersonPressureTemperingState { PersonId = personId, ClanId = clanId };
        state.PersonTemperings.Add(tempering);
        return tempering;
    }

    private static int ComputeCompositePressure(SocialPressureInput input)
    {
        int raw =
            (input.AverageDistress / 2)
            + (input.AverageMigrationRisk / 4)
            + (input.HouseholdDebtPressure / 5)
            + (input.TradeDebtPressure / 5)
            + (input.TradeReputationLoss / 4)
            + (input.LineageTension / 3)
            + (input.PrestigeDeficit * 2)
            + (input.MourningLoad / 3)
            + (input.CareLoad / 5)
            + (input.FuneralDebt / 4)
            + (input.ReliefSanctionPressure / 4)
            + (input.BranchFavorPressure / 5)
            - (input.SupportReserve / 5)
            - (input.MediationMomentum / 5)
            - (input.HeirSecurity / 8);

        return Math.Clamp(raw, 0, 100);
    }

    private static IReadOnlyDictionary<string, string> BuildTemperingMetadata(
        ClanId clanId,
        EmotionalPressureAxis axis,
        TemperingOutcome outcome)
    {
        return new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.Cause] = axis == EmotionalPressureAxis.Unknown
                ? DomainEventMetadataValues.CausePressureTempering
                : DomainEventMetadataValues.CauseSocialPressure,
            [DomainEventMetadataKeys.ClanId] = clanId.Value.ToString(),
            [DomainEventMetadataKeys.EmotionalAxis] = axis.ToString(),
            [DomainEventMetadataKeys.SocialPressureScore] = outcome.PressureScore.ToString(),
            [DomainEventMetadataKeys.TemperingScore] = outcome.TemperingScore.ToString(),
            [DomainEventMetadataKeys.PressureBand] = outcome.PressureBand.ToString(),
            [DomainEventMetadataKeys.TemperingBand] = outcome.TemperingBand.ToString(),
        };
    }

    private static int ComputeTemperingScore(ClanEmotionalClimateState climate)
    {
        return Math.Clamp(
            (climate.Hardening + climate.Restraint + climate.Bitterness + climate.Volatility) / 2,
            0,
            100);
    }

    private static string BuildPressureTrace(SocialPressureInput input, int pressure)
    {
        return $"pressure {pressure}: distress {input.AverageDistress}, migration {input.AverageMigrationRisk}, lineage {input.LineageTension}, mourning {input.MourningLoad}, tradeDebt {input.TradeDebtPressure}, support {input.SupportReserve}, mediation {input.MediationMomentum}";
    }

    private static int ResolveBand(int value)
    {
        return value switch
        {
            >= 80 => 3,
            >= 60 => 2,
            >= 40 => 1,
            _ => 0,
        };
    }

    private static int ScaleOver(int value, int threshold, int maxDelta)
    {
        if (value < threshold)
        {
            return 0;
        }

        return Math.Clamp(1 + ((value - threshold) / 20), 1, maxDelta);
    }

    private static int Decay(int value, int amount)
    {
        return Math.Max(0, value - amount);
    }

    private static int ClampPressure(int value)
    {
        return Math.Clamp(value, 0, 100);
    }

    private readonly record struct SocialPressureInput(
        int AverageDistress,
        int AverageMigrationRisk,
        bool HasMigratingHousehold,
        int HouseholdDebtPressure,
        int TradeDebtPressure,
        int TradeReputationLoss,
        int LineageTension,
        int PrestigeDeficit,
        int SupportReserve,
        int MediationMomentum,
        int ReliefSanctionPressure,
        int BranchFavorPressure,
        int MourningLoad,
        int CareLoad,
        int FuneralDebt,
        int CharityObligation,
        int HeirSecurity)
    {
        public int ShameSoil => Math.Clamp(PrestigeDeficit + ReliefSanctionPressure + BranchFavorPressure + TradeReputationLoss, 0, 100);
    }

    private readonly record struct TraitAverages(int Ambition, int Prudence, int Loyalty, int Sociability);

    private readonly record struct TemperingOutcome(
        int PressureScore,
        int PressureBand,
        int TemperingScore,
        int TemperingBand,
        bool PressureBandRose,
        bool TemperingBandRose);
}
