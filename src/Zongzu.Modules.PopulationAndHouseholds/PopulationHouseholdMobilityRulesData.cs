using System;
using System.Collections.Generic;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed record PopulationHouseholdMobilityRulesData(int FocusedMemberPromotionCap)
{
    public const int DefaultFocusedMemberPromotionCap = 2;
    public const int MaxFocusedMemberPromotionCap = 8;

    public static PopulationHouseholdMobilityRulesData Default { get; } =
        new(DefaultFocusedMemberPromotionCap);

    public PopulationHouseholdMobilityRulesValidationResult Validate()
    {
        if (FocusedMemberPromotionCap is < 0 or > MaxFocusedMemberPromotionCap)
        {
            return PopulationHouseholdMobilityRulesValidationResult.Invalid(
                $"focused_member_promotion_cap must be between 0 and {MaxFocusedMemberPromotionCap}.");
        }

        return PopulationHouseholdMobilityRulesValidationResult.Valid;
    }

    public int GetFocusedMemberPromotionCapOrDefault()
    {
        return Validate().IsValid
            ? FocusedMemberPromotionCap
            : DefaultFocusedMemberPromotionCap;
    }
}

public sealed record PopulationHouseholdMobilityRulesValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static PopulationHouseholdMobilityRulesValidationResult Valid { get; } =
        new(true, Array.Empty<string>());

    public static PopulationHouseholdMobilityRulesValidationResult Invalid(string error)
    {
        return new PopulationHouseholdMobilityRulesValidationResult(false, [error]);
    }
}
