using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule
{
    private static void DispatchPolicyWindowImplementationEvents(
        ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        bool anyCareerChanged = false;
        foreach (IDomainEvent domainEvent in scope.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyWindowOpened)
            .OrderBy(static e => e.EntityKey, StringComparer.Ordinal))
        {
            SettlementId? settlementId = ResolvePolicyWindowSettlement(domainEvent);
            if (!settlementId.HasValue)
            {
                continue;
            }

            List<OfficeCareerState> appointedOfficials = scope.State.People
                .Where(p => p.HasAppointment && p.SettlementId == settlementId.Value)
                .OrderBy(static p => p.PersonId.Value)
                .ToList();
            if (appointedOfficials.Count == 0)
            {
                continue;
            }

            JurisdictionAuthorityState jurisdiction = ResolvePolicyImplementationJurisdiction(
                scope.State,
                settlementId.Value,
                appointedOfficials);
            OfficeCareerState lead = appointedOfficials
                .OrderByDescending(static p => p.AuthorityTier)
                .ThenByDescending(static p => p.OfficeReputation)
                .ThenBy(static p => p.PersonId.Value)
                .First();
            PolicyImplementationProfile profile = ComputePolicyImplementationProfile(
                domainEvent,
                lead,
                jurisdiction);

            foreach (OfficeCareerState career in appointedOfficials)
            {
                ApplyPolicyImplementationProfile(career, profile);
            }

            scope.Emit(
                OfficeAndCareerEventNames.PolicyImplemented,
                BuildPolicyImplementationEventText(jurisdiction, profile),
                settlementId.Value.Value.ToString(),
                BuildPolicyImplementationMetadata(domainEvent, jurisdiction, lead, profile));
            scope.RecordDiff(
                BuildPolicyImplementationDiffText(jurisdiction, profile),
                settlementId.Value.Value.ToString());
            anyCareerChanged = true;
        }

        if (!anyCareerChanged)
        {
            return;
        }

        scope.State.People = scope.State.People
            .OrderBy(static person => person.PersonId.Value)
            .ToList();
        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);
        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(scope.State);
    }

    private static SettlementId? ResolvePolicyWindowSettlement(IDomainEvent domainEvent)
    {
        SettlementId? settlementId = ResolveSettlementMetadata(domainEvent);
        if (settlementId.HasValue)
        {
            return settlementId;
        }

        return int.TryParse(domainEvent.EntityKey, out int entitySettlementId)
            ? new SettlementId(entitySettlementId)
            : null;
    }

    private static JurisdictionAuthorityState ResolvePolicyImplementationJurisdiction(
        OfficeAndCareerState state,
        SettlementId settlementId,
        IReadOnlyList<OfficeCareerState> appointedOfficials)
    {
        JurisdictionAuthorityState? existing = state.Jurisdictions
            .FirstOrDefault(j => j.SettlementId == settlementId);
        if (existing is not null)
        {
            return existing;
        }

        return OfficeAndCareerStateProjection.BuildJurisdictions(appointedOfficials)
            .First(static j => true);
    }

    private static PolicyImplementationProfile ComputePolicyImplementationProfile(
        IDomainEvent sourceEvent,
        OfficeCareerState lead,
        JurisdictionAuthorityState jurisdiction)
    {
        int windowPressure = ReadMetadataInt(
            sourceEvent,
            DomainEventMetadataKeys.PolicyWindowPressure,
            ReadMetadataInt(sourceEvent, DomainEventMetadataKeys.PressureScore, 60));
        int mandateDeficit = ReadMetadataInt(sourceEvent, DomainEventMetadataKeys.PolicyWindowMandateDeficit, 0);
        int authorityTier = Math.Max(lead.AuthorityTier, jurisdiction.AuthorityTier);
        int leverage = Math.Max(lead.JurisdictionLeverage, jurisdiction.JurisdictionLeverage);
        int clerk = Math.Max(lead.ClerkDependence, jurisdiction.ClerkDependence);
        int backlog = Math.Max(lead.PetitionBacklog, jurisdiction.PetitionBacklog);
        int taskLoad = Math.Max(lead.AdministrativeTaskLoad, jurisdiction.AdministrativeTaskLoad);
        int petition = Math.Max(lead.PetitionPressure, jurisdiction.PetitionPressure);
        int metadataAdministrativeDrag = ReadMetadataInt(
            sourceEvent,
            DomainEventMetadataKeys.PolicyWindowAdministrativeDrag,
            0);
        int metadataClerkDrag = ReadMetadataInt(
            sourceEvent,
            DomainEventMetadataKeys.PolicyWindowClerkDrag,
            0);
        int metadataBacklogDrag = ReadMetadataInt(
            sourceEvent,
            DomainEventMetadataKeys.PolicyWindowBacklogDrag,
            0);

        int docketDrag = Math.Clamp(
            (taskLoad / 2) + (backlog / 3) + (petition / 4) + metadataAdministrativeDrag + metadataBacklogDrag,
            0,
            100);
        int clerkCapture = Math.Clamp(
            (clerk / 2) + (backlog / 4) + (taskLoad / 6) + (metadataClerkDrag * 2)
            - ((authorityTier * 3) + (leverage / 8)),
            0,
            100);
        int localBuffer = Math.Clamp(
            (authorityTier * 8) + (leverage / 3) + (Math.Max(0, lead.OfficeReputation - 40) / 8),
            0,
            80);
        int paperCompliance = Math.Clamp(
            (windowPressure / 2) + (authorityTier * 5) + (Math.Max(0, lead.OfficeReputation) / 20)
            - (clerk / 5) - (backlog / 8),
            0,
            100);
        int implementationScore = Math.Clamp(
            windowPressure + localBuffer + (mandateDeficit / 2) - docketDrag - (clerk / 2),
            0,
            100);

        string outcome = clerkCapture >= 48 && clerk >= 55
            ? DomainEventMetadataValues.PolicyImplementationCaptured
            : implementationScore < 42
                ? DomainEventMetadataValues.PolicyImplementationDragged
                : implementationScore < 62 && paperCompliance >= 35
                    ? DomainEventMetadataValues.PolicyImplementationPaperCompliance
                    : DomainEventMetadataValues.PolicyImplementationRapid;

        return new PolicyImplementationProfile(
            outcome,
            implementationScore,
            windowPressure,
            docketDrag,
            clerkCapture,
            localBuffer,
            paperCompliance,
            authorityTier,
            leverage,
            clerk,
            backlog,
            petition,
            taskLoad);
    }

    private static void ApplyPolicyImplementationProfile(
        OfficeCareerState career,
        PolicyImplementationProfile profile)
    {
        PolicyImplementationMutation mutation = ResolvePolicyImplementationMutation(profile);

        career.CurrentAdministrativeTask = SelectAdministrativeTask(
            career.CurrentAdministrativeTask,
            mutation.TaskName,
            Math.Max(career.AuthorityTier, 1));
        career.AdministrativeTaskLoad = Math.Clamp(
            career.AdministrativeTaskLoad + mutation.AdministrativeTaskLoadDelta,
            0,
            100);
        career.PetitionBacklog = Math.Clamp(
            career.PetitionBacklog + mutation.PetitionBacklogDelta,
            0,
            100);
        career.PetitionPressure = Math.Clamp(
            career.PetitionPressure + mutation.PetitionPressureDelta,
            0,
            100);
        career.ClerkDependence = Math.Clamp(
            career.ClerkDependence + mutation.ClerkDependenceDelta,
            0,
            100);
        career.JurisdictionLeverage = Math.Clamp(
            career.JurisdictionLeverage + mutation.JurisdictionLeverageDelta,
            0,
            100);
        career.PromotionMomentum = Math.Clamp(
            career.PromotionMomentum + mutation.PromotionMomentumDelta,
            0,
            100);
        career.DemotionPressure = Math.Clamp(
            career.DemotionPressure + mutation.DemotionPressureDelta,
            0,
            100);
        career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(
            mutation.OutcomeCategory,
            BuildPolicyImplementationOutcomeDetail(profile));
        career.LastExplanation =
            $"{career.LastExplanation} Policy implementation {profile.Outcome}: score {profile.ImplementationScore}, docket drag {profile.DocketDrag}, clerk capture {profile.ClerkCapture}.";
    }

    private static PolicyImplementationMutation ResolvePolicyImplementationMutation(
        PolicyImplementationProfile profile)
    {
        return profile.Outcome switch
        {
            DomainEventMetadataValues.PolicyImplementationCaptured => new PolicyImplementationMutation(
                "clerk-held policy docket",
                "Stalled",
                8,
                7,
                4,
                6,
                -4,
                -1,
                4),
            DomainEventMetadataValues.PolicyImplementationDragged => new PolicyImplementationMutation(
                "county yamen follow-up",
                "Delayed",
                6,
                5,
                3,
                3,
                -2,
                -1,
                2),
            DomainEventMetadataValues.PolicyImplementationPaperCompliance => new PolicyImplementationMutation(
                "posted paper compliance",
                "Triaged",
                3,
                1,
                1,
                1,
                0,
                1,
                1),
            _ => new PolicyImplementationMutation(
                "urgent policy review",
                "Granted",
                1,
                -2,
                -1,
                0,
                1,
                2,
                0),
        };
    }

    private static Dictionary<string, string> BuildPolicyImplementationMetadata(
        IDomainEvent sourceEvent,
        JurisdictionAuthorityState jurisdiction,
        OfficeCareerState lead,
        PolicyImplementationProfile profile)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseCourt,
            [DomainEventMetadataKeys.SourceEventType] = sourceEvent.EventType,
            [DomainEventMetadataKeys.SettlementId] = jurisdiction.SettlementId.Value.ToString(),
            [DomainEventMetadataKeys.PersonId] = lead.PersonId.Value.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationOutcome] = profile.Outcome,
            [DomainEventMetadataKeys.PolicyImplementationScore] = profile.ImplementationScore.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationWindowPressure] = profile.WindowPressure.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationDocketDrag] = profile.DocketDrag.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationClerkCapture] = profile.ClerkCapture.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationLocalBuffer] = profile.LocalBuffer.ToString(),
            [DomainEventMetadataKeys.PolicyImplementationPaperCompliance] = profile.PaperCompliance.ToString(),
            [DomainEventMetadataKeys.AuthorityTier] = profile.AuthorityTier.ToString(),
            [DomainEventMetadataKeys.JurisdictionLeverage] = profile.JurisdictionLeverage.ToString(),
            [DomainEventMetadataKeys.ClerkDependence] = profile.ClerkDependence.ToString(),
            [DomainEventMetadataKeys.PetitionBacklog] = profile.PetitionBacklog.ToString(),
            [DomainEventMetadataKeys.PetitionPressure] = profile.PetitionPressure.ToString(),
            [DomainEventMetadataKeys.AdministrativeTaskLoad] = profile.AdministrativeTaskLoad.ToString(),
        };
    }

    private static string BuildPolicyImplementationEventText(
        JurisdictionAuthorityState jurisdiction,
        PolicyImplementationProfile profile)
    {
        return profile.Outcome switch
        {
            DomainEventMetadataValues.PolicyImplementationCaptured =>
                $"{jurisdiction.LeadOfficeTitle} policy implementation was captured by clerks; score {profile.ImplementationScore}.",
            DomainEventMetadataValues.PolicyImplementationDragged =>
                $"{jurisdiction.LeadOfficeTitle} policy implementation dragged in the yamen docket; score {profile.ImplementationScore}.",
            DomainEventMetadataValues.PolicyImplementationPaperCompliance =>
                $"{jurisdiction.LeadOfficeTitle} policy implementation reached paper compliance; score {profile.ImplementationScore}.",
            _ =>
                $"{jurisdiction.LeadOfficeTitle} policy implementation moved quickly; score {profile.ImplementationScore}.",
        };
    }

    private static string BuildPolicyImplementationDiffText(
        JurisdictionAuthorityState jurisdiction,
        PolicyImplementationProfile profile)
    {
        return
            $"{jurisdiction.LeadOfficeTitle} policy implementation {profile.Outcome}: docket drag {profile.DocketDrag}, clerk capture {profile.ClerkCapture}, local buffer {profile.LocalBuffer}.";
    }

    private static string BuildPolicyImplementationOutcomeDetail(PolicyImplementationProfile profile)
    {
        return profile.Outcome switch
        {
            DomainEventMetadataValues.PolicyImplementationCaptured =>
                $"Clerks held the docket; capture {profile.ClerkCapture}, paper compliance {profile.PaperCompliance}.",
            DomainEventMetadataValues.PolicyImplementationDragged =>
                $"Yamen follow-up still drags; docket drag {profile.DocketDrag}, score {profile.ImplementationScore}.",
            DomainEventMetadataValues.PolicyImplementationPaperCompliance =>
                $"The notice reached paper compliance, but implementation remains thin; paper {profile.PaperCompliance}.",
            _ =>
                $"Urgent review cleared the local docket; buffer {profile.LocalBuffer}, score {profile.ImplementationScore}.",
        };
    }

    private readonly record struct PolicyImplementationProfile(
        string Outcome,
        int ImplementationScore,
        int WindowPressure,
        int DocketDrag,
        int ClerkCapture,
        int LocalBuffer,
        int PaperCompliance,
        int AuthorityTier,
        int JurisdictionLeverage,
        int ClerkDependence,
        int PetitionBacklog,
        int PetitionPressure,
        int AdministrativeTaskLoad);

    private readonly record struct PolicyImplementationMutation(
        string TaskName,
        string OutcomeCategory,
        int AdministrativeTaskLoadDelta,
        int PetitionBacklogDelta,
        int PetitionPressureDelta,
        int ClerkDependenceDelta,
        int JurisdictionLeverageDelta,
        int PromotionMomentumDelta,
        int DemotionPressureDelta);
}
