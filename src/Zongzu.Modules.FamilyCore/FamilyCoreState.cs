using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.FamilyCore;

    public List<ClanStateData> Clans { get; set; } = new();

    public List<FamilyPersonState> People { get; set; } = new();
}

public sealed class ClanStateData
{
    public ClanId Id { get; set; }

    public string ClanName { get; set; } = string.Empty;

    public SettlementId HomeSettlementId { get; set; }

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public PersonId? HeirPersonId { get; set; }
}

public sealed class FamilyPersonState
{
    public PersonId Id { get; set; }

    public ClanId ClanId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public int AgeMonths { get; set; }

    public bool IsAlive { get; set; }
}
