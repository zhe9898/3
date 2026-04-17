using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class ClanSnapshot
{
    public ClanId Id { get; set; }

    public string ClanName { get; set; } = string.Empty;

    public SettlementId HomeSettlementId { get; set; }

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public PersonId? HeirPersonId { get; set; }
}

public interface IFamilyCoreQueries
{
    ClanSnapshot GetRequiredClan(ClanId clanId);

    IReadOnlyList<ClanSnapshot> GetClans();
}
