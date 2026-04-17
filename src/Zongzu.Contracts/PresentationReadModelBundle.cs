using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class PresentationReadModelBundle
{
    public GameDate CurrentDate { get; set; }

    public string ReplayHash { get; set; } = string.Empty;

    public IReadOnlyList<ClanSnapshot> Clans { get; set; } = [];

    public IReadOnlyList<SettlementSnapshot> Settlements { get; set; } = [];

    public IReadOnlyList<PopulationSettlementSnapshot> PopulationSettlements { get; set; } = [];

    public IReadOnlyList<EducationCandidateSnapshot> EducationCandidates { get; set; } = [];

    public IReadOnlyList<AcademySnapshot> Academies { get; set; } = [];

    public IReadOnlyList<ClanTradeSnapshot> ClanTrades { get; set; } = [];

    public IReadOnlyList<MarketSnapshot> Markets { get; set; } = [];

    public IReadOnlyList<TradeRouteSnapshot> TradeRoutes { get; set; } = [];

    public IReadOnlyList<NarrativeNotificationSnapshot> Notifications { get; set; } = [];

    public PresentationDebugSnapshot Debug { get; set; } = new();
}
