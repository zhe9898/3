using System;
using System.Globalization;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class DomainEventEntityKeys
{
    public static bool TryGetSettlementId(IDomainEvent domainEvent, out SettlementId settlementId)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (int.TryParse(domainEvent.EntityKey, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            settlementId = new SettlementId(value);
            return true;
        }

        settlementId = default;
        return false;
    }
}
