using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC 搂20.3 鈥?derives <see cref="PublicSurfaceSignal"/>s
/// from authoritative state transitions. Signals are <b>not</b> authored
/// prose drawn from an event pool; they are the forced, rule-driven
/// shape that a state change takes when it becomes publicly visible.
///
/// <para><b>Stream competition</b> (SPEC 搂20.1): the same underlying
/// transition can produce multiple signals, one per <see cref="OpinionStream"/>
/// it reaches, with different <see cref="PublicSurfaceSignal.Sentiment"/>
/// and possibly different <see cref="PublicSurfaceSignal.Category"/>. That
/// is the definition of "streams compete" 鈥?a canal opening reads positive
/// on <see cref="OpinionStream.NoticeBoard"/>, skeptical on
/// <see cref="OpinionStream.MarketTalk"/>.</para>
///
/// <para><b>Covert nodes emit nothing</b> (SPEC 搂20.6): callers must pass
/// only nodes whose <see cref="SettlementStateData.Visibility"/> is not
/// <see cref="NodeVisibility.Covert"/>. Covertness is expressed by absence.</para>
///
/// <para><b>Phase 1c minimum set</b> (SPEC 搂9 step 8):</para>
/// <list type="bullet">
///   <item><see cref="EmitCanalWindowChanged"/> 鈥?one signal per transition
///         shape (open/closed/limited-narrowed), kind depends on direction.</item>
///   <item><see cref="EmitFloodRiskBreach"/> 鈥?three concurrent signals on
///         NoticeBoard / MarketTalk / TempleWhisper.</item>
/// </list>
/// </summary>
internal static class PublicSurfaceSignalEmitter
{
    private const int FloodRiskUrgentThreshold = 70;

    /// <summary>
    /// SPEC 搂20.3 canal-transition pattern. Emits either:
    /// <list type="bullet">
    ///   <item>Opening (Closed/Limited 鈫?Open) 鈥?<see cref="OpinionStream.NoticeBoard"/>
    ///         <see cref="PublicSurfaceCategory.Commerce"/> +30 with
    ///         <see cref="PressureKind.GrainPressure"/> eased.</item>
    ///   <item>Closing (Open/Limited 鈫?Closed) 鈥?<see cref="OpinionStream.NoticeBoard"/>
    ///         <see cref="PublicSurfaceCategory.Alarm"/> -40 exposing
    ///         <see cref="PressureKind.FloodPressure"/>.</item>
    ///   <item>Narrowing (Open 鈫?Limited) 鈥?single Alarm -20.</item>
    /// </list>
    /// </summary>
    public static void EmitCanalWindowChanged(
        List<PublicSurfaceSignal> buffer,
        SettlementStateData canalJunction,
        CanalWindow from,
        CanalWindow to)
    {
        if (canalJunction.Visibility == NodeVisibility.Covert)
        {
            return; // SPEC 搂20.6: covert nodes host no streams.
        }

        bool opening = to == CanalWindow.Open && from != CanalWindow.Open;
        bool closing = to == CanalWindow.Closed && from != CanalWindow.Closed;

        if (opening)
        {
            buffer.Add(new PublicSurfaceSignal(
                NodeId: canalJunction.Id,
                NodeKind: canalJunction.NodeKind,
                Stream: OpinionStream.NoticeBoard,
                Category: PublicSurfaceCategory.Commerce,
                Tier: NotificationTier.Consequential,
                HeadlineKey: "canal-opening",
                Sentiment: +30,
                ExposedPressures: new[] { PressureKind.GrainPressure }));
            return;
        }

        if (closing)
        {
            buffer.Add(new PublicSurfaceSignal(
                NodeId: canalJunction.Id,
                NodeKind: canalJunction.NodeKind,
                Stream: OpinionStream.NoticeBoard,
                Category: PublicSurfaceCategory.Alarm,
                Tier: NotificationTier.Consequential,
                HeadlineKey: "canal-closing",
                Sentiment: -40,
                ExposedPressures: new[] { PressureKind.FloodPressure }));
            return;
        }

        // Narrowing (Open 鈫?Limited) or widening (Closed 鈫?Limited) 鈥?soft notice.
        buffer.Add(new PublicSurfaceSignal(
            NodeId: canalJunction.Id,
            NodeKind: canalJunction.NodeKind,
            Stream: OpinionStream.NoticeBoard,
            Category: PublicSurfaceCategory.Alarm,
            Tier: NotificationTier.Consequential,
            HeadlineKey: "canal-limited",
            Sentiment: -20,
            ExposedPressures: new[] { PressureKind.GrainPressure }));
    }

    /// <summary>
    /// SPEC 搂20.3 flood three-stream concurrent breach. When
    /// <c>FloodRisk &gt;= 70</c>, the same hazard surfaces on three streams
    /// with different sentiment and category 鈥?this is the canonical
    /// stream-competition demonstration required by SPEC 搂22.1.
    ///
    /// <list type="bullet">
    ///   <item>NoticeBoard 鈥?official Alarm, Urgent tier, -50 sentiment.</item>
    ///   <item>MarketTalk 鈥?ferry panic, -70 sentiment.</item>
    ///   <item>TempleWhisper 鈥?divine-anger rumor, -60 sentiment.</item>
    /// </list>
    ///
    /// <para>Callers pass up to three nodes; nulls are skipped so a world
    /// missing (say) a Temple still emits the available streams rather than
    /// silencing the breach entirely.</para>
    /// </summary>
    public static void EmitFloodRiskBreach(
        List<PublicSurfaceSignal> buffer,
        int floodRisk,
        SettlementStateData? canalJunction,
        SettlementStateData? ferry,
        SettlementStateData? temple)
    {
        if (floodRisk < FloodRiskUrgentThreshold)
        {
            return;
        }

        IReadOnlyList<PressureKind> exposed = new[] { PressureKind.FloodPressure };

        if (canalJunction is not null && canalJunction.Visibility != NodeVisibility.Covert)
        {
            buffer.Add(new PublicSurfaceSignal(
                NodeId: canalJunction.Id,
                NodeKind: canalJunction.NodeKind,
                Stream: OpinionStream.NoticeBoard,
                Category: PublicSurfaceCategory.Alarm,
                Tier: NotificationTier.Urgent,
                HeadlineKey: "flood-risk-official",
                Sentiment: -50,
                ExposedPressures: exposed));
        }

        if (ferry is not null && ferry.Visibility != NodeVisibility.Covert)
        {
            buffer.Add(new PublicSurfaceSignal(
                NodeId: ferry.Id,
                NodeKind: ferry.NodeKind,
                Stream: OpinionStream.MarketTalk,
                Category: PublicSurfaceCategory.Alarm,
                Tier: NotificationTier.Consequential,
                HeadlineKey: "flood-risk-ferry-panic",
                Sentiment: -70,
                ExposedPressures: exposed));
        }

        if (temple is not null && temple.Visibility != NodeVisibility.Covert)
        {
            buffer.Add(new PublicSurfaceSignal(
                NodeId: temple.Id,
                NodeKind: temple.NodeKind,
                Stream: OpinionStream.TempleWhisper,
                Category: PublicSurfaceCategory.Rumor,
                Tier: NotificationTier.Consequential,
                HeadlineKey: "flood-risk-divine-anger",
                Sentiment: -60,
                ExposedPressures: exposed));
        }
    }
}
