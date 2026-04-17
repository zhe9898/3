using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.OrderAndBanditry;

    public List<SettlementDisorderState> Settlements { get; set; } = new();
}

public sealed class SettlementDisorderState
{
    public SettlementId SettlementId { get; set; }

    public int BanditThreat { get; set; }

    public int RoutePressure { get; set; }

    public int SuppressionDemand { get; set; }

    public int DisorderPressure { get; set; }

    public string LastPressureReason { get; set; } = string.Empty;
}
