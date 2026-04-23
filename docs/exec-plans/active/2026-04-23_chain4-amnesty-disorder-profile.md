# Chain 4 First Thickening: Amnesty Disorder Profile

## Goal

Thicken the already-wired chain-4 thin path without expanding it into the full imperial/court system:

`ImperialRhythmChanged` -> `OfficeAndCareer.AmnestyApplied(metadata)` -> `OrderAndBanditry` amnesty-disorder profile -> `DisorderSpike`

The purpose is to replace the fixed disorder bump with a rules-owned local profile while keeping the same scheduler-backed topology.

## Scope

In scope:
- `OfficeAndCareer.AmnestyApplied` carries structured execution facts: `AmnestyWave`, authority tier, jurisdiction leverage, clerk dependence, petition backlog, and administrative task load.
- `OrderAndBanditry` computes the disorder delta from those facts plus local order-owned state: `DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, `RoutePressure`, and suppression buffers.
- Strong local authority / suppression can absorb a limited amnesty without forcing a disorder bump.
- `DisorderSpike` carries amnesty cause/profile metadata when the public-spike threshold is crossed.
- Focused tests prove missing metadata does not fall back to `Summary` parsing.
- Real scheduler test proves the same-month drain still crosses into `DisorderSpike` under a seeded local-yamen pressure state.

Out of scope:
- no `CourtAndThrone` module
- no mourning/succession branch
- no appointment-pause branch
- no public legitimacy formula
- no `DisorderLevelChanged` periodic summary implementation
- no direct household, market, or public-life mutation from `OfficeAndCareer`

## Ownership

| Module | Owns |
| --- | --- |
| `WorldSettlements` | imperial rhythm source state and `AmnestyWave` |
| `OfficeAndCareer` | local yamen execution receipt and office-context metadata |
| `OrderAndBanditry` | local disorder consequences and threshold receipt |
| `PublicLifeAndRumor` | downstream projection only through `DisorderSpike` metadata |

## Determinism

- The handler uses only event metadata and order-owned state.
- Missing `AmnestyWave` metadata is a no-op.
- The rule does not parse `IDomainEvent.Summary`.
- The profile can resolve to zero; a broad imperial mercy wave does not automatically mean every county becomes less orderly.
- Same-month propagation still goes through bounded scheduler drain.
- Off-scope settlements remain untouched because `EntityKey` is settlement-scoped.

## Save Compatibility

No schema bump. The change adds event metadata and handler formula logic only.

## Milestones

- [x] Add amnesty execution metadata keys.
- [x] Emit `AmnestyApplied` metadata from `OfficeAndCareer`.
- [x] Replace fixed Order `+10` with an amnesty-disorder profile.
- [x] Emit profile metadata on amnesty-origin `DisorderSpike`.
- [x] Update focused Office/Order tests.
- [x] Update real scheduler chain-4 test.
- [x] Update topology/spec/integration docs.

## Validation

- `dotnet test tests/Zongzu.Modules.OfficeAndCareer.Tests/Zongzu.Modules.OfficeAndCareer.Tests.csproj --no-restore --filter AmnestyDispatchHandlerTests -p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore --filter AmnestyDisorderHandlerTests -p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter ImperialAmnestyDisorderChainTests -p:UseSharedCompilation=false`
