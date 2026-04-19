## Goal
- tighten the documented `xun` bridge from `OfficeAndCareer` into `PublicLifeAndRumor`
- let county-gate public pulse feel hotter when the local yamen is overloaded, without moving office ownership or breaking the month-end review shell

## Scope in
- add a bounded `xun`-only office-surface adjustment inside `PublicLifeAndRumor`
- read only from the existing `IOfficeAndCareerQueries` jurisdiction snapshot
- use office-owned `AdministrativeTaskLoad` and `ClerkDependence` as the main short-band pressure inputs
- let hot yamen surfaces bias notice visibility, prefecture dispatch pressure, road-report lag, and public legitimacy during `xun`
- add focused module coverage for hot-vs-calm yamen xun behavior
- update integration / acceptance notes

## Scope out
- no schema bump
- no new public-life or office commands
- no new office-owned state
- no month-end rewrite of public-life wording ownership
- no UI or hall-shell changes

## Affected modules
- `Zongzu.Modules.PublicLifeAndRumor`
- `Zongzu.Modules.PublicLifeAndRumor.Tests`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- the new behavior is xun-only deterministic projection drift over existing office/public-life state

## Determinism risk
- low
- the bridge stays arithmetic and query-first
- `PublicLifeAndRumor` still does not write office state
- month-end readable output remains public-life-owned

## Milestones
1. add a bounded office-surface xun adjustment in `PublicLifeAndRumor`
2. keep month-end public-life pulse ownership and readable output unchanged
3. add focused tests for hot-vs-calm yamen surfaces
4. run targeted and full verification

## Tests to add/update
- `Zongzu.Modules.PublicLifeAndRumor.Tests`
- full solution build / test verification

## Rollback / fallback plan
- if office heat over-dominates xun public pulse, keep the bridge but narrow it to `xiaxun` only
- if month-end readability starts drifting, keep the xun bridge and remove any month-path dependence on the same fields

## Open questions
- whether a later slice should let `PublicLifeAndRumor` compress office pressure into trend-only read models for hall/desk without changing month-owned wording
- whether `PublicLifeAndRumor` should later distinguish petition-heavy county seats from market towns more sharply at `xun`

## Result notes
- `PublicLifeAndRumor` now has a bounded xun-only office-surface bridge over the existing jurisdiction query seam
- the bridge reads office-owned `AdministrativeTaskLoad`, `ClerkDependence`, and bounded petition context, then folds that into county-gate heat only during `xun`
- `shangxun` now lets a hot yamen slightly thicken notice drift
- `zhongxun` now lets paper queue drag leak into street talk and public-legitimacy softening
- `xiaxun` now lets a genuinely overloaded yamen push notice visibility, prefecture pressure, and route-report drag harder before month-end review
- month-end public-life pulse, readable diffs, and event emission remain unchanged in ownership and cadence
- focused tests now prove:
  - hot and calm yamen surfaces separate during `xiaxun`
  - the same task-load / clerk-dependence difference does not silently rewrite the month-end public pulse

## Verification
- `dotnet test .\tests\Zongzu.Modules.PublicLifeAndRumor.Tests\Zongzu.Modules.PublicLifeAndRumor.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
