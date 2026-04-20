using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §20.3 — the category dimension of a
/// <see cref="PublicSurfaceSignal"/>.
///
/// Separate from <see cref="OpinionStream"/> (which stream heard it) and from
/// <see cref="NotificationTier"/> (how loud it is). Category answers <i>what
/// kind of public event</i>: an alarm, a rumor, a market posting, a ritual
/// notice, or a petition surfacing.
/// </summary>
public enum PublicSurfaceCategory
{
    Unknown = 0,

    /// <summary>Unofficial talk and gossip.</summary>
    Rumor = 1,

    /// <summary>Temple rites, festival notice, ancestral-hall pronouncement.</summary>
    Ritual = 2,

    /// <summary>Market price, goods arrival, ferry schedule, broker posting.</summary>
    Commerce = 3,

    /// <summary>Citizen grievance surfacing — yamen complaint, hall appeal.</summary>
    Petition = 4,

    /// <summary>Urgent public warning — flood crest, bandit sighting, canal closure.</summary>
    Alarm = 5,
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §20.3 — a public-surface signal emitted when a
/// living-world event becomes visible on one of the five
/// <see cref="OpinionStream"/> channels.
///
/// <b>The same underlying event may produce multiple signals</b>, one per
/// stream it reaches, with different <see cref="Sentiment"/> and potentially
/// different <see cref="Category"/> on each. That is the definition of
/// "stream competition" in SPEC §20.1 — a county execution reads positive on
/// <see cref="OpinionStream.NoticeBoard"/>, skeptical on
/// <see cref="OpinionStream.TeahouseChat"/>, feud-laden on
/// <see cref="OpinionStream.HallPronouncement"/>.
///
/// <para>Covert nodes (<see cref="SettlementNodeKind.CovertMeetPoint"/>,
/// <see cref="SettlementNodeKind.SmugglingCache"/>) deliberately emit no
/// signals — covertness is expressed by absence, not by a flag on the
/// signal.</para>
///
/// <para>Phase 1c emitters (SPEC §9 step 8): <c>WorldSettlements</c> must
/// self-emit at minimum two patterns — <c>CanalWindow</c> transitions, and
/// <c>FloodRisk &gt;= 70</c> three-stream concurrent breach.</para>
///
/// <para><c>HeadlineKey</c> is a projection-layer translation key, not
/// player-facing prose — authority keeps the fact, shell renders the words
/// (ENGINEERING_RULES §10).</para>
/// </summary>
/// <param name="NodeId">The settlement node hosting the signal.</param>
/// <param name="NodeKind">The node's functional kind (cached to avoid extra lookup).</param>
/// <param name="Stream">Which of the five opinion streams this signal lives on.</param>
/// <param name="Category">Event category — alarm, rumor, commerce, petition, ritual.</param>
/// <param name="Tier">Urgency in the notice tray (reuses <see cref="NotificationTier"/>).</param>
/// <param name="HeadlineKey">Projection translation key; never rendered directly.</param>
/// <param name="Sentiment">-100..+100 — this stream's valence on this signal.</param>
/// <param name="ExposedPressures">
/// Which <see cref="PressureKind"/>s became visible through this signal. Empty
/// if the signal is pure ritual / rumor without authoritative pressure exposure.
/// </param>
public sealed record PublicSurfaceSignal(
    SettlementId NodeId,
    SettlementNodeKind NodeKind,
    OpinionStream Stream,
    PublicSurfaceCategory Category,
    NotificationTier Tier,
    string HeadlineKey,
    int Sentiment,
    IReadOnlyList<PressureKind> ExposedPressures);
