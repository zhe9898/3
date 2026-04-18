using System;

namespace Zongzu.Modules.ConflictAndForce;

public static class ConflictAndForceResponseStateCalculator
{
    public const int MinimumResponseActivationLevel = 3;

    public static void Refresh(SettlementForceState settlement)
    {
        ArgumentNullException.ThrowIfNull(settlement);

        int activationLevel = ComputeResponseActivationLevel(settlement);
        bool isResponseActivated = settlement.HasActiveConflict && activationLevel >= MinimumResponseActivationLevel;

        settlement.ResponseActivationLevel = activationLevel;
        settlement.IsResponseActivated = isResponseActivated;
        settlement.OrderSupportLevel = isResponseActivated
            ? ComputeOrderSupportLevel(settlement, activationLevel)
            : 0;
    }

    public static int ComputeResponseActivationLevel(SettlementForceState settlement)
    {
        ArgumentNullException.ThrowIfNull(settlement);

        int activation = 0;

        if (settlement.MilitiaCount >= 20)
        {
            activation += 3;
        }
        else if (settlement.MilitiaCount >= 10)
        {
            activation += 1;
        }

        if (settlement.EscortCount >= 8)
        {
            activation += 2;
        }
        else if (settlement.EscortCount >= 4)
        {
            activation += 1;
        }

        if (settlement.GuardCount >= 16)
        {
            activation += 1;
        }

        if (settlement.Readiness >= 45)
        {
            activation += 2;
        }
        else if (settlement.Readiness >= 30)
        {
            activation += 1;
        }

        if (settlement.CommandCapacity >= 35)
        {
            activation += 1;
        }

        return Math.Clamp(activation, 0, 10);
    }

    public static int ComputeOrderSupportLevel(SettlementForceState settlement, int activationLevel)
    {
        ArgumentNullException.ThrowIfNull(settlement);

        if (!settlement.HasActiveConflict || activationLevel < MinimumResponseActivationLevel)
        {
            return 0;
        }

        int support =
            activationLevel
            + (settlement.GuardCount / 8)
            + (settlement.EscortCount / 5)
            + (settlement.Readiness / 20)
            + (settlement.CommandCapacity / 25);

        return Math.Clamp(support, 0, 12);
    }

    public static bool InferLegacyHasActiveConflict(string trace)
    {
        ArgumentNullException.ThrowIfNull(trace);

        return trace.Contains("contained", StringComparison.OrdinalIgnoreCase)
            || trace.Contains("clash", StringComparison.OrdinalIgnoreCase)
            || trace.Contains("violence", StringComparison.OrdinalIgnoreCase)
            || trace.Contains("wounded", StringComparison.OrdinalIgnoreCase)
            || trace.Contains("conflict", StringComparison.OrdinalIgnoreCase);
    }
}
