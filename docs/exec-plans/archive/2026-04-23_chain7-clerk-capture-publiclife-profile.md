# Chain 7 Clerk Capture Public-Life Profile

> Date: 2026-04-23
> Status: Implemented in branch `codex/thin-chain-topology-index`
> Scope: first rule thickening on top of the existing chain-7 thin slice

## Goal

Keep the proven scheduler path:

```
OfficeAndCareer.ClerkCaptureDeepened
  -> PublicLifeAndRumor street-talk heat
```

Replace the purely fixed public-life response with an office-owned clerk-capture profile that downstream projection can read without parsing text.

## Boundary

- `OfficeAndCareer` owns clerk dependence, petition backlog, administrative task load, jurisdiction leverage, and the repeated-edge watermark.
- `PublicLifeAndRumor` owns public heat and trace projection.
- The profile is metadata on the domain event; it does not create a second rule layer in application services.
- No household, market, or memory effects are implemented in this pass.

## Implemented Milestones

1. `OfficeAndCareer.ClerkCaptureDeepened` now carries:
   - clerk dependence
   - petition backlog
   - administrative task load
   - petition pressure
   - authority tier
   - jurisdiction leverage
   - capture pressure components
   - authority buffer
2. `PublicLifeAndRumor` scales street-talk heat from the profile metadata.
3. Older/simple `ClerkCaptureDeepened` events without profile metadata still use the legacy `+12` heat fallback.
4. Existing settlement scoping and `ActiveClerkCaptureSettlementIds` watermark behavior are preserved.

## Tests

- `Chain789OfficePressureHandlerTests` verifies clerk-capture profile metadata on the Office event.
- `Chain7ClerkCaptureTests` verifies profile-scaled public heat, legacy fallback, clamping, off-scope protection, and invalid-key no-op.
- `OfficeCourtRegimePressureChainTests` verifies the real scheduler drains clerk capture into scoped public life with profile metadata and no repeated emission.

## Save / Schema

- No module save schema changes.
- New `DomainEventMetadataKeys` entries are event contract metadata only.

## Determinism

- The profile is deterministic and uses only jurisdiction state.
- Public heat scaling is deterministic and settlement-scoped.
- No random draw was introduced.

## Still Not Done

- Official evaluation cadence.
- Memorial attacks and censorial pressure.
- Household petition delay state.
- Trade dispute delay and commercial confidence.
- Clerk factions and runner networks.
- Recommended clerk / shiye intervention.
- SocialMemory stigma, favor, shame, and long residue.
