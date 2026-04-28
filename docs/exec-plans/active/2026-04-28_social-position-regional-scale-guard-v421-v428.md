# Social Position Regional Scale Guard V421-V428

## Intent

V421-V428 closes a narrow evidence gap after v413-v420: the social-position scale-budget readback must prove the far-summary side, not only close/local person detail.

This is a guard pass for the existing rule: near people may surface owner-lane detail, while regional / distant society remains pooled summary.

## Baseline

- `main` at `45574c9 Add social position scale budget readback`.
- V413-V420 added `PersonDossierSnapshot.SocialPositionScaleBudgetReadbackSummary`.
- Existing production code already maps `FidelityRing.Regional` to `regional summary`.

## Scope

- Add a focused integration test with a registry-only `FidelityRing.Regional` person.
- Add docs and architecture guard evidence that regional readback remains summary-only.
- Keep production rule code unchanged unless the test exposes a real mismatch.

## Non-Goals

- No fidelity-ring mutation.
- No precision-band state.
- No class / social-position / movement / scale-budget ledger.
- No full commoner mobility engine.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity authority.
- No parsing of `DomainEvent.Summary`, person dossier prose, social-position text, source-key display, notification prose, receipt prose, mobility text, public-life lines, or docs text.

## Target Schema / Migration

Target schema/migration impact: none.

This pass is tests/docs guard only. It adds no production persisted state, schema version, root save version, migration, feature manifest entry, save manifest entry, module namespace, serialized projection cache, or module payload.

## Validation Plan

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "Name=RegistryOnlyBootstrap_BuildsRegionalScaleBudget_ForDistantPerson"`
- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Milestones

- [x] Add regional/distant scale-budget integration test.
- [x] Add docs and architecture guard evidence.
- [x] Run final validation lane.

## Completion Evidence

- Added `RegistryOnlyBootstrap_BuildsRegionalScaleBudget_ForDistantPerson` to prove a registry-only `FidelityRing.Regional` dossier reads `regional summary`, `registry-only source`, and distant pooled summary.
- Added architecture/docs guard evidence for v421-v428 so the far-summary branch cannot be mistaken for precision mutation, a hidden person selector, or a class engine.
- Production rule code unchanged; this pass is tests/docs only.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, module namespace, projection cache, or `PersonRegistry` authority field changed.

Validation completed on 2026-04-28:

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "Name=RegistryOnlyBootstrap_BuildsRegionalScaleBudget_ForDistantPerson"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Social_position_regional_scale_guard_v421_v428_must_keep_far_summary_without_new_authority"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`
