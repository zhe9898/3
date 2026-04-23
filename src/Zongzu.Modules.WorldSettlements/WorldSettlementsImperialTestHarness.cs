using Zongzu.Contracts;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.2 / §24.4 — Phase 1c implementation of
/// <see cref="IImperialEventTestHarness"/>. Mutates the owning module's
/// <see cref="SeasonBandData.Imperial"/> channels directly; the next
/// <c>RunMonth</c> pass detects the change and emits
/// <c>WorldSettlementsEventNames.ImperialRhythmChanged</c>.
///
/// <para><b>Test-only</b>. SPEC §24.4 rules that production builds must not
/// expose this. Phase 1c keeps it registered alongside the normal query so
/// that <c>LivingWorldLivenessTests</c> can drive the imperial axis; later
/// phases must gate registration behind a feature flag or move it to a
/// separate test-bootstrap module.</para>
/// </summary>
internal sealed class WorldSettlementsImperialTestHarness : IImperialEventTestHarness
{
    private readonly WorldSettlementsState _state;

    public WorldSettlementsImperialTestHarness(WorldSettlementsState state)
    {
        _state = state;
    }

    public void Inject(ImperialEventKind kind, int intensity)
    {
        SeasonBandAdvancer.InjectImperialPulse(_state.CurrentSeason.Imperial, kind, intensity);
    }
}
