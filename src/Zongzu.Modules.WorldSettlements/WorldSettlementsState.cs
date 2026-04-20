using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

public sealed class WorldSettlementsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.WorldSettlements;

    public List<SettlementStateData> Settlements { get; set; } = new();
}

public sealed class SettlementStateData
{
    public SettlementId Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SettlementTier Tier { get; set; }

    public int Security { get; set; }

    public int Prosperity { get; set; }

    public int BaselineInstitutionCount { get; set; }
}
