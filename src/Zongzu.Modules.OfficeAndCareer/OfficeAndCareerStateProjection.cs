using System;
using System.Collections.Generic;
using System.Linq;

namespace Zongzu.Modules.OfficeAndCareer;

public static class OfficeAndCareerStateProjection
{
    public static List<JurisdictionAuthorityState> BuildJurisdictions(IReadOnlyList<OfficeCareerState> people)
    {
        return people
            .Where(static person => person.HasAppointment)
            .GroupBy(static person => person.SettlementId)
            .Select(group =>
            {
                OfficeCareerState lead = group
                    .OrderByDescending(static person => person.AuthorityTier)
                    .ThenByDescending(static person => person.OfficeReputation)
                    .ThenBy(static person => person.PersonId.Value)
                    .First();
                int jurisdictionLeverage = Math.Clamp(
                    group.Max(static person => person.JurisdictionLeverage) + ((group.Count() - 1) * 4),
                    0,
                    100);
                int petitionPressure = Math.Clamp(
                    (int)Math.Round(group.Average(static person => person.PetitionPressure)),
                    0,
                    100);
                int petitionBacklog = Math.Clamp(
                    (int)Math.Round(group.Average(static person => person.PetitionBacklog)),
                    0,
                    100);

                return new JurisdictionAuthorityState
                {
                    SettlementId = group.Key,
                    LeadOfficialPersonId = lead.PersonId,
                    LeadOfficialName = lead.DisplayName,
                    LeadOfficeTitle = lead.OfficeTitle,
                    AuthorityTier = lead.AuthorityTier,
                    JurisdictionLeverage = jurisdictionLeverage,
                    PetitionPressure = petitionPressure,
                    PetitionBacklog = petitionBacklog,
                    CurrentAdministrativeTask = lead.CurrentAdministrativeTask,
                    LastPetitionOutcome = lead.LastPetitionOutcome,
                    LastAdministrativeTrace =
                        $"{lead.DisplayName}以{lead.OfficeTitle}主事，乡面杖力{jurisdictionLeverage}，词牍压{petitionPressure}，积案{petitionBacklog}，所办差遣为{lead.CurrentAdministrativeTask}。{lead.LastPetitionOutcome}",
                };
            })
            .OrderBy(static authority => authority.SettlementId.Value)
            .ToList();
    }

    public static OfficeAndCareerState UpgradeFromSchemaV1(OfficeAndCareerState state)
    {
        foreach (OfficeCareerState career in state.People)
        {
            UpgradeCareer(career);
        }

        state.People = state.People
            .OrderBy(static person => person.PersonId.Value)
            .ToList();
        state.Jurisdictions = BuildJurisdictions(state.People);
        return state;
    }

    public static OfficeAndCareerState UpgradeFromSchemaV2ToV3(OfficeAndCareerState state)
    {
        foreach (OfficeCareerState career in state.People)
        {
            UpgradeCareerV3(career);
        }

        state.People = state.People
            .OrderBy(static person => person.PersonId.Value)
            .ToList();
        state.Jurisdictions = BuildJurisdictions(state.People);
        return state;
    }

    private static void UpgradeCareer(OfficeCareerState career)
    {
        if (career.HasAppointment)
        {
            string inferredTask = InferAdministrativeTask(career);
            string taskFromExplanation = TryExtractAdministrativeTask(career.LastExplanation);
            string currentTask = HasPersistedAdministrativeTask(career)
                ? career.CurrentAdministrativeTask
                : taskFromExplanation;
            string normalizedTask = NormalizeAdministrativeTask(string.IsNullOrWhiteSpace(currentTask)
                ? inferredTask
                : currentTask);
            string petitionOutcome = HasPersistedPetitionOutcome(career)
                ? NormalizePetitionOutcome(career.LastPetitionOutcome)
                : ExtractPetitionOutcomeOrFallback(career, normalizedTask);

            career.CurrentAdministrativeTask = normalizedTask;
            career.ServiceMonths = Math.Max(career.ServiceMonths, InferRepresentativeServiceMonths(career));
            career.PromotionMomentum = Math.Max(career.PromotionMomentum, InferPromotionMomentum(career));
            career.DemotionPressure = Math.Max(career.DemotionPressure, InferDemotionPressure(career));
            career.AdministrativeTaskLoad = Math.Max(career.AdministrativeTaskLoad, InferAdministrativeTaskLoad(career));
            career.PetitionBacklog = Math.Max(career.PetitionBacklog, InferPetitionBacklog(career));
            career.LastPetitionOutcome = petitionOutcome;
            return;
        }

        career.ServiceMonths = Math.Max(career.ServiceMonths, 0);
        career.CurrentAdministrativeTask = string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)
            ? "候补听选"
            : NormalizeAdministrativeTask(career.CurrentAdministrativeTask);
        career.LastPetitionOutcome = string.IsNullOrWhiteSpace(career.LastPetitionOutcome)
            ? OfficeAndCareerDescriptors.FormatPetitionOutcome("Unavailable", "未得官身，词状不得入案。")
            : NormalizePetitionOutcome(career.LastPetitionOutcome);
        career.PetitionBacklog = Math.Max(career.PetitionBacklog, 0);
        career.AdministrativeTaskLoad = Math.Max(career.AdministrativeTaskLoad, 0);
        career.PromotionMomentum = Math.Max(career.PromotionMomentum, 0);
        career.DemotionPressure = Math.Max(career.DemotionPressure, 0);
    }

    private static void UpgradeCareerV3(OfficeCareerState career)
    {
        if (career.HasAppointment)
        {
            career.AppointmentPressure = Math.Max(career.AppointmentPressure, 48);
            career.ClerkDependence = Math.Max(
                career.ClerkDependence,
                Math.Clamp(14 + (career.AdministrativeTaskLoad / 2) + (career.PetitionBacklog / 5), 0, 100));
            return;
        }

        if (career.IsEligible)
        {
            if (string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask))
            {
                career.CurrentAdministrativeTask = string.Equals(career.LastOutcome, "听差", StringComparison.Ordinal)
                    ? "随案听差"
                    : "守选候阙";
            }

            career.AppointmentPressure = Math.Max(
                career.AppointmentPressure,
                Math.Clamp(18 + (career.OfficeReputation / 3) + (career.PromotionMomentum / 4), 0, 100));
            career.ClerkDependence = Math.Max(
                career.ClerkDependence,
                string.Equals(career.LastOutcome, "听差", StringComparison.Ordinal)
                    ? 24
                    : 10);
            return;
        }

        career.AppointmentPressure = Math.Max(career.AppointmentPressure, 0);
        career.ClerkDependence = Math.Max(career.ClerkDependence, 0);
    }

    private static string InferAdministrativeTask(OfficeCareerState career)
    {
        if (career.AuthorityTier >= 3 && career.PetitionPressure >= 55)
        {
            return "急牍覆核";
        }

        if (career.AuthorityTier >= 2 && career.PetitionPressure >= 45)
        {
            return "勘理词状";
        }

        if (career.AuthorityTier >= 2)
        {
            return "勾检户籍";
        }

        return career.PetitionPressure >= 45
            ? "誊录词牍"
            : "誊黄封牍";
    }

    private static string InferPetitionOutcome(OfficeCareerState career)
    {
        if (string.Equals(career.LastOutcome, "Granted", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Queued", "新任甫定，词牍先行收案。");
        }

        return career.PetitionPressure >= 55
            ? OfficeAndCareerDescriptors.FormatPetitionOutcome("Delayed", $"正在{career.CurrentAdministrativeTask}，其余词牍暂缓。")
            : OfficeAndCareerDescriptors.FormatPetitionOutcome("Triaged", $"正在{career.CurrentAdministrativeTask}，诸状分轻重收理。");
    }

    private static bool HasPersistedAdministrativeTask(OfficeCareerState career)
    {
        if (string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask))
        {
            return false;
        }

        return !string.Equals(career.CurrentAdministrativeTask, "候补听选", StringComparison.Ordinal);
    }

    private static bool HasPersistedPetitionOutcome(OfficeCareerState career)
    {
        if (string.IsNullOrWhiteSpace(career.LastPetitionOutcome))
        {
            return false;
        }

        return !string.Equals(career.LastPetitionOutcome, "未开案：暂无词牍。", StringComparison.Ordinal);
    }

    private static string ExtractPetitionOutcomeOrFallback(OfficeCareerState career, string normalizedTask)
    {
        string? extracted = TryExtractPetitionOutcome(career.LastExplanation);
        if (!string.IsNullOrWhiteSpace(extracted))
        {
            return NormalizePetitionOutcome(extracted);
        }

        OfficeCareerState inferredCareer = new()
        {
            LastOutcome = career.LastOutcome,
            PetitionPressure = career.PetitionPressure,
            CurrentAdministrativeTask = normalizedTask,
        };
        return InferPetitionOutcome(inferredCareer);
    }

    private static int InferRepresentativeServiceMonths(OfficeCareerState career)
    {
        if (!career.HasAppointment)
        {
            return 0;
        }

        return career.LastOutcome switch
        {
            "Granted" => 1,
            _ => Math.Max(1, career.AuthorityTier * 3),
        };
    }

    private static int InferPromotionMomentum(OfficeCareerState career)
    {
        if (TryExtractPromotionPressureLabel(career.LastExplanation) is string label)
        {
            return label switch
            {
                "promotion-ready" => 60,
                "rising" => 40,
                "steady" => 24,
                "thin" => 12,
                _ => 0,
            };
        }

        return Math.Clamp((career.OfficeReputation / 4) + (career.AuthorityTier * 4), 0, 100);
    }

    private static int InferDemotionPressure(OfficeCareerState career)
    {
        if (TryExtractDemotionPressureLabel(career.LastExplanation) is string label)
        {
            return label switch
            {
                "critical" => 70,
                "strained" => 50,
                "watched" => 30,
                "stable" => 0,
                _ => 0,
            };
        }

        return Math.Clamp((career.PetitionPressure / 4) - career.AuthorityTier, 0, 100);
    }

    private static int InferAdministrativeTaskLoad(OfficeCareerState career)
    {
        string taskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(career.CurrentAdministrativeTask, career.AuthorityTier);
        return taskTier switch
        {
            "crisis" => Math.Clamp(18 + career.AuthorityTier, 0, 100),
            "district" => Math.Clamp(12 + (career.AuthorityTier * 2), 0, 100),
            "registry" => Math.Clamp(11 + career.AuthorityTier, 0, 100),
            "clerical" => Math.Clamp(8 + Math.Max(career.AuthorityTier, 1), 0, 100),
            _ => Math.Clamp(8 + (career.PetitionPressure / 3) - career.AuthorityTier, 0, 100),
        };
    }

    private static int InferPetitionBacklog(OfficeCareerState career)
    {
        if (TryExtractBacklog(career.LastExplanation) is int backlog)
        {
            return backlog;
        }

        return Math.Clamp((career.PetitionPressure / 2) - (career.AuthorityTier * 2), 0, 100);
    }

    private static string TryExtractAdministrativeTask(string explanation)
    {
        if (string.IsNullOrWhiteSpace(explanation))
        {
            return string.Empty;
        }

        const string outcomeWhileMarker = " while ";
        int outcomeWhileIndex = explanation.LastIndexOf(outcomeWhileMarker, StringComparison.Ordinal);
        if (outcomeWhileIndex >= 0)
        {
            int taskStart = outcomeWhileIndex + outcomeWhileMarker.Length;
            int taskEnd = explanation.IndexOf('.', taskStart);
            if (taskEnd > taskStart)
            {
                return explanation[taskStart..taskEnd].Trim();
            }
        }

        const string firstTaskMarker = "task '";
        int firstTaskIndex = explanation.IndexOf(firstTaskMarker, StringComparison.Ordinal);
        if (firstTaskIndex >= 0)
        {
            int taskStart = firstTaskIndex + firstTaskMarker.Length;
            int taskEnd = explanation.IndexOf('\'', taskStart);
            if (taskEnd > taskStart)
            {
                return explanation[taskStart..taskEnd].Trim();
            }
        }

        return string.Empty;
    }

    private static string? TryExtractPetitionOutcome(string explanation)
    {
        if (string.IsNullOrWhiteSpace(explanation))
        {
            return null;
        }

        foreach (string category in new[] { "Cleared", "Triaged", "Delayed", "Stalled", "Queued", "Unavailable" })
        {
            int index = explanation.LastIndexOf($"{category}:", StringComparison.Ordinal);
            if (index >= 0)
            {
                return explanation[index..].Trim();
            }
        }

        return null;
    }

    private static int? TryExtractBacklog(string explanation)
    {
        const string marker = "backlog ";
        int index = explanation.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        index += marker.Length;
        int end = index;
        while (end < explanation.Length && char.IsDigit(explanation[end]))
        {
            end += 1;
        }

        return end > index && int.TryParse(explanation[index..end], out int backlog)
            ? backlog
            : null;
    }

    private static string? TryExtractPromotionPressureLabel(string explanation)
    {
        return TryExtractLabel(
            explanation,
            "promotion",
            "promotion pressure",
            "promotion momentum",
            new[] { "promotion-ready", "rising", "steady", "thin" });
    }

    private static string? TryExtractDemotionPressureLabel(string explanation)
    {
        return TryExtractLabel(
            explanation,
            "demotion",
            "demotion pressure",
            "demotion drag",
            new[] { "critical", "strained", "watched", "stable" });
    }

    private static string? TryExtractLabel(
        string explanation,
        string directPrefix,
        string suffixOne,
        string suffixTwo,
        IReadOnlyList<string> labels)
    {
        if (string.IsNullOrWhiteSpace(explanation))
        {
            return null;
        }

        foreach (string label in labels)
        {
            if (explanation.Contains($"{directPrefix} {label}", StringComparison.Ordinal)
                || explanation.Contains($"{label} {suffixOne}", StringComparison.Ordinal)
                || explanation.Contains($"{label} {suffixTwo}", StringComparison.Ordinal))
            {
                return label;
            }
        }

        return null;
    }

    private static string NormalizeAdministrativeTask(string task)
    {
        return task switch
        {
            "reviewing emergency petition ledgers" => "急牍覆核",
            "emergency petition review" => "急牍覆核",
            "mediating clan grievance dockets" => "勘解乡怨词牍",
            "grievance mediation docket" => "勘解乡怨词牍",
            "inspecting local watch levies" => "巡丁清点",
            "watch levy inspection" => "巡丁清点",
            "hearing district petitions" => "勘理词状",
            "district petition hearings" => "勘理词状",
            "reviewing county registers" => "勾检户籍",
            "county register review" => "勾检户籍",
            "clearing district petitions" => "誊录词牍",
            "petition copying desk" => "誊录词牍",
            "copying tax rolls and sealed filings" => "誊黄封牍",
            "sealed filing copy desk" => "誊黄封牍",
            "Awaiting posting" => "候补听选",
            _ => task,
        };
    }

    private static string NormalizePetitionOutcome(string outcome)
    {
        if (outcome.Contains(':', StringComparison.Ordinal))
        {
            return outcome;
        }

        if (outcome.Contains("Initial petitions queued", StringComparison.Ordinal)
            || outcome.Contains("新任甫定", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Queued", "新任甫定，词牍先行收案。");
        }

        if (outcome.Contains("cleared", StringComparison.OrdinalIgnoreCase)
            || outcome.Contains("已清", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Cleared", outcome.TrimEnd('.'));
        }

        if (outcome.Contains("triaged", StringComparison.OrdinalIgnoreCase)
            || outcome.Contains("分轻重", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Triaged", outcome.TrimEnd('.'));
        }

        if (outcome.Contains("delayed", StringComparison.OrdinalIgnoreCase)
            || outcome.Contains("暂缓", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Delayed", outcome.TrimEnd('.'));
        }

        if (outcome.Contains("stalled", StringComparison.OrdinalIgnoreCase)
            || outcome.Contains("壅滞", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Stalled", outcome.TrimEnd('.'));
        }

        if (outcome.Contains("No petitions", StringComparison.OrdinalIgnoreCase)
            || outcome.Contains("未得官身", StringComparison.Ordinal)
            || outcome.Contains("暂无词牍", StringComparison.Ordinal))
        {
            return OfficeAndCareerDescriptors.FormatPetitionOutcome("Unavailable", "未得官身，词状不得入案。");
        }

        return OfficeAndCareerDescriptors.FormatPetitionOutcome("Unknown", outcome.Trim());
    }
}
