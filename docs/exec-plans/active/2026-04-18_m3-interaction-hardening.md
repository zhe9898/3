## Goal
- harden M3 local-conflict module interaction so `ConflictAndForce.Lite` and `OrderAndBanditry.Lite` coordinate cleanly without leaking influence when the force response is not actually activated
- strengthen end-to-end and performance coverage for the local-conflict slice
- leave a clearer structural seam for post-MVP war / black-route expansion

## Scope in
- conflict/order interaction and execution-order refinement if needed for same-month coordination
- query-side response activation / support gating
- no-leak tests for calm or non-activated local-conflict states
- stronger local-conflict E2E and runtime-only diagnostics coverage
- docs updates for M3 interaction flow and post-MVP integration seams

## Scope out
- no new player commands
- no tactical combat or campaign board work
- no full black-market or warfare implementation
- no Unity scene or UI interaction expansion

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.OrderAndBanditry`
- integration and module tests
- M3 and post-MVP planning docs

## Save/schema impact
- prefer query-only and runtime-only changes first
- if authoritative `ConflictAndForce` state needs new persisted coordination fields, document and version the module explicitly
- do not change root schema unless unavoidable

## Determinism risk
- medium
- changing execution order or downstream gating can alter replay results
- mitigate by:
  - deterministic ordering only
  - replay and calm/no-leak assertions
  - full build/test verification

## Milestones
1. capture interaction-hardening scope in an ExecPlan
2. tighten conflict/order coordination and activation gating
3. add calm/no-leak and stronger E2E tests
4. extend runtime-only local-conflict diagnostics coverage
5. update docs, including post-MVP seam notes, and verify

## Tests to add/update
- `ConflictAndForceModuleTests`
- `OrderAndBanditryModuleTests`
- `M2LiteIntegrationTests`
- diagnostics/performance coverage for the M3 local-conflict slice

## Rollback / fallback plan
- if same-month coordination creates too much coupling, keep the current order and limit the change to activation-gated support only
- if per-module diagnostics add too much noise, keep them runtime-only and test-only rather than surfacing them in the saved debug bundle

## Open questions
- should same-month coordination be solved by execution order, by explicit activation metadata, or both
- how much of the future black-route / warfare seam belongs in `OrderAndBanditry` vs `TradeAndIndustry` vs `ConflictAndForce`

## Completion notes
- `ConflictAndForce.Lite` now executes before `OrderAndBanditry.Lite` in the shared M3 local-conflict path so same-month force posture can be read without direct cross-module mutation.
- `OrderAndBanditry.Lite` now only consumes conflict-side suppression support when the response posture is actually activated; calm standing force capacity no longer leaks into disorder suppression.
- runtime-only diagnostics now track per-module diff/event activity peaks for the local-conflict slice without expanding save compatibility surface.
- post-MVP seam notes were captured for warfare and black-route / black-market expansion so later feature packs extend the same force and disorder substrate rather than inventing a parallel model.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`

## Save compatibility notes
- this slice stayed query-only and runtime-only for the new conflict/order coordination metadata.
- no new persisted authoritative state was added for activation gating, so root and module save schema versions did not need to change.
