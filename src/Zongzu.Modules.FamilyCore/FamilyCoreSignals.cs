using System.Collections.Generic;

namespace Zongzu.Modules.FamilyCore;

internal readonly record struct FamilyMonthSignals(
    IReadOnlyList<FamilyPersonAge> LivingPeople,
    FamilyPersonAge? LivingHeir,
    int AdultCount,
    int ChildCount,
    int ElderCount,
    int InfantCount);

internal readonly record struct FamilyPersonAge(FamilyPersonState Person, int AgeMonths);
