using System;
using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private static void DispatchTradeShockEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (!IsTradeShockEvent(domainEvent.EventType) || !TryResolveClanId(domainEvent, out ClanId clanId))
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, clanId);
            EventPressureDelta delta = domainEvent.EventType switch
            {
                TradeAndIndustryEventNames.TradeProspered => new EventPressureDelta(Hope: 5, Trust: 3, Obligation: 1, Favor: 2, Type: MemoryType.Trust, Subtype: MemorySubtype.ProtectionFavor, Kind: "trade_prospered", CauseKey: "trade.prospered"),
                TradeAndIndustryEventNames.TradeLossOccurred => new EventPressureDelta(Fear: 3, Shame: 2, Anger: 1, Grudge: 1, Type: MemoryType.Fear, Subtype: MemorySubtype.MarketDebt, Kind: "trade_loss", CauseKey: "trade.loss"),
                TradeAndIndustryEventNames.TradeDebtDefaulted => new EventPressureDelta(Fear: 4, Shame: 5, Anger: 3, Grudge: 3, Type: MemoryType.Debt, Subtype: MemorySubtype.TradeBreach, Kind: "trade_debt_default", CauseKey: "trade.debt_default"),
                TradeAndIndustryEventNames.RouteBusinessBlocked => new EventPressureDelta(Fear: 4, Anger: 4, Shame: 1, Grudge: 2, Type: MemoryType.Grudge, Subtype: MemorySubtype.WealthGrudge, Kind: "route_blocked", CauseKey: "trade.route_blocked"),
                _ => default,
            };

            ApplyEventPressure(scope, clanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Shame);
        }
    }

    private static void DispatchViolentDeathEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries? familyQueries = null;
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != DeathCauseEventNames.DeathByViolence || !TryResolvePersonId(domainEvent, out PersonId personId))
            {
                continue;
            }

            familyQueries ??= scope.GetRequiredQuery<IFamilyCoreQueries>();
            FamilyPersonSnapshot? person = familyQueries.FindPerson(personId);
            if (person is null)
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, person.ClanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, person.ClanId);
            PersonPressureTemperingState tempering = GetOrCreatePersonTempering(scope.State, person.Id, person.ClanId);
            EventPressureDelta delta = new(Fear: 7, Grief: 8, Anger: 6, Shame: 2, Grudge: 5, Type: MemoryType.Grief, Subtype: MemorySubtype.ViolentDeath, Kind: $"violent_death:{person.Id.Value}", CauseKey: "death.violent");

            ApplyEventPressure(scope, person.ClanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Grief);
            tempering.Grief = ClampPressure(tempering.Grief + 8);
            tempering.Fear = ClampPressure(tempering.Fear + 4);
            tempering.Anger = ClampPressure(tempering.Anger + 4);
            tempering.LastPressureScore = Math.Max(tempering.LastPressureScore, 70);
            tempering.LastUpdated = scope.Context.CurrentDate;
            tempering.LastTrace = $"{person.GivenName}: violent death entered clan memory";
        }
    }

    private static void DispatchFamilyBranchEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (!IsFamilyBranchEvent(domainEvent.EventType) || !TryResolveClanId(domainEvent, out ClanId clanId))
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, clanId);
            EventPressureDelta delta = domainEvent.EventType switch
            {
                FamilyCoreEventNames.BranchSeparationApproved => new EventPressureDelta(Shame: 3, Anger: 4, Grief: 2, Grudge: 3, Type: MemoryType.Grudge, Subtype: MemorySubtype.BranchRift, Kind: "branch_separation", CauseKey: "family.branch_separation"),
                FamilyCoreEventNames.HeirSecurityWeakened => new EventPressureDelta(Fear: 4, Shame: 3, Grief: 2, Grudge: 2, Type: MemoryType.Fear, Subtype: MemorySubtype.MourningDebt, Kind: "heir_security_weakened", CauseKey: "family.heir_security"),
                _ => default,
            };

            ApplyEventPressure(scope, clanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Anger);
        }
    }

    private static void DispatchMarriageAllianceEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != FamilyCoreEventNames.MarriageAllianceArranged || !TryResolveClanId(domainEvent, out ClanId clanId))
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, clanId);
            EventPressureDelta delta = new(Hope: 5, Trust: 4, Obligation: 4, Fear: -1, Shame: -1, Favor: 3, Type: MemoryType.Favor, Subtype: MemorySubtype.MarriageObligation, Kind: "marriage_alliance", CauseKey: "family.marriage_alliance");
            ApplyEventPressure(scope, clanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Obligation);
        }
    }

    private static void DispatchExamEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries? familyQueries = null;
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (!IsExamEvent(domainEvent.EventType) || !TryResolvePersonId(domainEvent, out PersonId personId))
            {
                continue;
            }

            familyQueries ??= scope.GetRequiredQuery<IFamilyCoreQueries>();
            FamilyPersonSnapshot? person = familyQueries.FindPerson(personId);
            if (person is null)
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, person.ClanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, person.ClanId);
            PersonPressureTemperingState tempering = GetOrCreatePersonTempering(scope.State, person.Id, person.ClanId);
            EventPressureDelta delta = domainEvent.EventType switch
            {
                EducationAndExamsEventNames.ExamPassed => new EventPressureDelta(Hope: 7, Trust: 3, Obligation: 2, Shame: -2, Favor: 2, Type: MemoryType.Aspiration, Subtype: MemorySubtype.ExamHonor, Kind: $"exam_pass:{person.Id.Value}", CauseKey: "exam.pass"),
                EducationAndExamsEventNames.ExamFailed => new EventPressureDelta(Shame: 5, Grief: 2, Anger: 2, Hope: -2, Type: MemoryType.Shame, Subtype: MemorySubtype.ExamFailure, Kind: $"exam_fail:{person.Id.Value}", CauseKey: "exam.fail"),
                EducationAndExamsEventNames.StudyAbandoned => new EventPressureDelta(Shame: 6, Grief: 4, Anger: 3, Hope: -4, Type: MemoryType.Shame, Subtype: MemorySubtype.ExamFailure, Kind: $"study_abandoned:{person.Id.Value}", CauseKey: "exam.study_abandoned"),
                _ => default,
            };

            ApplyEventPressure(scope, person.ClanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Hope);
            tempering.Hope = ClampPressure(tempering.Hope + delta.Hope);
            tempering.Shame = ClampPressure(tempering.Shame + delta.Shame);
            tempering.Grief = ClampPressure(tempering.Grief + delta.Grief);
            tempering.Anger = ClampPressure(tempering.Anger + delta.Anger);
            tempering.Obligation = ClampPressure(tempering.Obligation + delta.Obligation);
            tempering.LastPressureScore = Math.Max(tempering.LastPressureScore, Math.Abs(delta.Shame) + Math.Abs(delta.Hope) + Math.Abs(delta.Grief));
            tempering.LastUpdated = scope.Context.CurrentDate;
            tempering.LastTrace = $"{person.GivenName}: {domainEvent.EventType} shaped study pressure";
        }
    }

    private static void DispatchChildLossClimateEventsCore(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries? familyQueries = null;
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != DeathCauseEventNames.DeathByIllness
                || !string.Equals(domainEvent.ModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
                || !TryResolvePersonId(domainEvent, out PersonId personId))
            {
                continue;
            }

            familyQueries ??= scope.GetRequiredQuery<IFamilyCoreQueries>();
            FamilyPersonSnapshot? person = familyQueries.FindPerson(personId);
            if (person is null)
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, person.ClanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, person.ClanId);
            EventPressureDelta delta = new(Fear: 4, Grief: 6, Shame: 1, Obligation: 2, Type: MemoryType.Grief, Subtype: MemorySubtype.MourningDebt, Kind: $"child_loss_climate:{person.Id.Value}", CauseKey: "death.child_loss");
            ApplyEventPressure(scope, person.ClanId, narrative, climate, delta, domainEvent.Summary, EmotionalPressureAxis.Grief);
        }
    }

    private static void ApplyEventPressure(
        ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope,
        ClanId clanId,
        ClanNarrativeState narrative,
        ClanEmotionalClimateState climate,
        EventPressureDelta delta,
        string summary,
        EmotionalPressureAxis axis)
    {
        int previousPressureBand = climate.LastPressureBand;

        narrative.FearPressure = ClampPressure(narrative.FearPressure + delta.Fear);
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + delta.Shame);
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + delta.Grudge + Math.Max(0, delta.Anger / 2));
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + delta.Favor + (delta.Trust / 2), -100, 100);

        climate.Fear = ClampPressure(climate.Fear + delta.Fear);
        climate.Shame = ClampPressure(climate.Shame + delta.Shame);
        climate.Grief = ClampPressure(climate.Grief + delta.Grief);
        climate.Anger = ClampPressure(climate.Anger + delta.Anger);
        climate.Obligation = ClampPressure(climate.Obligation + delta.Obligation);
        climate.Hope = ClampPressure(climate.Hope + delta.Hope);
        climate.Trust = ClampPressure(climate.Trust + delta.Trust);
        climate.Bitterness = ClampPressure(climate.Bitterness + Math.Max(0, delta.Grudge + delta.Anger) / 2);
        climate.Volatility = ClampPressure(climate.Volatility + Math.Max(0, delta.Fear + delta.Anger + delta.Shame) / 4);

        int pressureScore = ClampPressure(climate.Fear + climate.Shame + climate.Grief + climate.Anger - climate.Trust - (climate.Hope / 2));
        int pressureBand = ResolveBand(pressureScore);
        climate.LastPressureScore = Math.Max(climate.LastPressureScore, pressureScore);
        climate.LastPressureBand = Math.Max(climate.LastPressureBand, pressureBand);
        climate.LastUpdated = scope.Context.CurrentDate;
        climate.LastTrace = $"{delta.CauseKey}: {summary}";

        if (!string.IsNullOrEmpty(delta.Kind))
        {
            AddMemory(
                scope.State,
                clanId,
                delta.Kind,
                delta.Type,
                delta.Subtype,
                delta.CauseKey,
                delta.MonthlyDecay,
                Math.Max(10, pressureScore),
                true,
                string.IsNullOrWhiteSpace(summary) ? delta.CauseKey : summary,
                scope.Context);
        }

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{clanId.Value} clan emotional pressure shifted by {delta.CauseKey}.",
                clanId.Value.ToString(),
                new Dictionary<string, string>
                {
                    [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                    [DomainEventMetadataKeys.ClanId] = clanId.Value.ToString(),
                    [DomainEventMetadataKeys.EmotionalAxis] = axis.ToString(),
                    [DomainEventMetadataKeys.SocialPressureScore] = pressureScore.ToString(),
                    [DomainEventMetadataKeys.PressureBand] = pressureBand.ToString(),
                    [DomainEventMetadataKeys.SourceEventType] = delta.CauseKey,
                });
        }
    }

    private static bool TryResolveClanId(IDomainEvent domainEvent, out ClanId clanId)
    {
        if (int.TryParse(domainEvent.EntityKey, out int entityClanId))
        {
            clanId = new ClanId(entityClanId);
            return true;
        }

        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.ClanId, out string? clanIdText)
            && int.TryParse(clanIdText, out int metadataClanId))
        {
            clanId = new ClanId(metadataClanId);
            return true;
        }

        clanId = default;
        return false;
    }

    private static bool TryResolvePersonId(IDomainEvent domainEvent, out PersonId personId)
    {
        if (int.TryParse(domainEvent.EntityKey, out int entityPersonId))
        {
            personId = new PersonId(entityPersonId);
            return true;
        }

        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.PersonId, out string? personIdText)
            && int.TryParse(personIdText, out int metadataPersonId))
        {
            personId = new PersonId(metadataPersonId);
            return true;
        }

        personId = default;
        return false;
    }

    private static bool IsTradeShockEvent(string eventType)
    {
        return eventType is TradeAndIndustryEventNames.RouteBusinessBlocked
            or TradeAndIndustryEventNames.TradeLossOccurred
            or TradeAndIndustryEventNames.TradeDebtDefaulted
            or TradeAndIndustryEventNames.TradeProspered;
    }

    private static bool IsFamilyBranchEvent(string eventType)
    {
        return eventType is FamilyCoreEventNames.BranchSeparationApproved
            or FamilyCoreEventNames.HeirSecurityWeakened;
    }

    private static bool IsExamEvent(string eventType)
    {
        return eventType is EducationAndExamsEventNames.ExamPassed
            or EducationAndExamsEventNames.ExamFailed
            or EducationAndExamsEventNames.StudyAbandoned;
    }

    private readonly record struct EventPressureDelta(
        int Fear = 0,
        int Shame = 0,
        int Grief = 0,
        int Anger = 0,
        int Obligation = 0,
        int Hope = 0,
        int Trust = 0,
        int Grudge = 0,
        int Favor = 0,
        MemoryType Type = MemoryType.Unknown,
        MemorySubtype Subtype = MemorySubtype.Unknown,
        string Kind = "",
        string CauseKey = "",
        int MonthlyDecay = 2);
}
