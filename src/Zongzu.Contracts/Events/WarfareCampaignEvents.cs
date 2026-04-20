using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class WarfareCampaignEventNames
{
    public const string CampaignMobilized = "CampaignMobilized";

    public const string CampaignPressureRaised = "CampaignPressureRaised";

    public const string CampaignSupplyStrained = "CampaignSupplyStrained";

    public const string CampaignAftermathRegistered = "CampaignAftermathRegistered";
}

public sealed class WarfareCampaignEventBundle
{
    public SettlementId SettlementId { get; set; }

    public bool CampaignMobilized { get; set; }

    public bool CampaignPressureRaised { get; set; }

    public bool CampaignSupplyStrained { get; set; }

    public bool CampaignAftermathRegistered { get; set; }
}

public static class WarfareCampaignEventBundler
{
    public static IReadOnlyList<WarfareCampaignEventBundle> Build(IReadOnlyList<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        Dictionary<SettlementId, WarfareCampaignEventBundle> bundles = new();
        foreach (IDomainEvent domainEvent in events)
        {
            if (!string.Equals(domainEvent.ModuleKey, KnownModuleKeys.WarfareCampaign, StringComparison.Ordinal)
                || !DomainEventEntityKeys.TryGetSettlementId(domainEvent, out SettlementId settlementId)
                || !IsRecognized(domainEvent.EventType))
            {
                continue;
            }

            if (!bundles.TryGetValue(settlementId, out WarfareCampaignEventBundle? bundle))
            {
                bundle = new WarfareCampaignEventBundle
                {
                    SettlementId = settlementId,
                };
                bundles[settlementId] = bundle;
            }

            switch (domainEvent.EventType)
            {
                case WarfareCampaignEventNames.CampaignMobilized:
                    bundle.CampaignMobilized = true;
                    break;
                case WarfareCampaignEventNames.CampaignPressureRaised:
                    bundle.CampaignPressureRaised = true;
                    break;
                case WarfareCampaignEventNames.CampaignSupplyStrained:
                    bundle.CampaignSupplyStrained = true;
                    break;
                case WarfareCampaignEventNames.CampaignAftermathRegistered:
                    bundle.CampaignAftermathRegistered = true;
                    break;
            }
        }

        return bundles.Values
            .OrderBy(static bundle => bundle.SettlementId.Value)
            .ToArray();
    }

    private static bool IsRecognized(string eventType)
    {
        return eventType is
            WarfareCampaignEventNames.CampaignMobilized or
            WarfareCampaignEventNames.CampaignPressureRaised or
            WarfareCampaignEventNames.CampaignSupplyStrained or
            WarfareCampaignEventNames.CampaignAftermathRegistered;
    }
}
