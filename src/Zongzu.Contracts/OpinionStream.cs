namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §20.1 — not one "public opinion" value, but five
/// independent, competing streams. The same event may appear on multiple
/// streams with different Sentiment — a county execution of a corrupt clerk
/// may read positive on <see cref="NoticeBoard"/>, skeptical on
/// <see cref="TeahouseChat"/>, and tied to a family feud on
/// <see cref="HallPronouncement"/>.
///
/// Covert nodes (<see cref="SettlementNodeKind.CovertMeetPoint"/>,
/// <see cref="SettlementNodeKind.SmugglingCache"/>) deliberately host no
/// streams — that is the definition of covert.
/// </summary>
public enum OpinionStream
{
    Unknown = 0,

    /// <summary>Official notices, proclamations, posted edicts. County seats, yamen, relay posts.</summary>
    NoticeBoard = 1,

    /// <summary>Market prices, broker gossip, peddler news. Market towns, wharves, ferries.</summary>
    MarketTalk = 2,

    /// <summary>Literati debate, exam chatter, moral verdicts. Academies, tea houses in market towns.</summary>
    TeahouseChat = 3,

    /// <summary>Omens, miracles, vow-talk, flight-from-disorder rumor. Temples, shrines.</summary>
    TempleWhisper = 4,

    /// <summary>Clan verdicts, mediation outcomes, public lineage stance. Ancestral halls.</summary>
    HallPronouncement = 5,
}
