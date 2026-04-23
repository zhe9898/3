# Chain 4 Thin Slice: ImperialRhythmChanged -> AmnestyApplied -> DisorderSpike

## Goal

Prove the smallest scheduler-backed chain for imperial amnesty pressure:

`WorldSettlements.ImperialRhythmChanged` -> `OfficeAndCareer.AmnestyApplied` -> `OrderAndBanditry.DisorderSpike`

This is not the full chain-4 implementation. It covers only the grand-amnesty branch and reuses `DisorderSpike` as the threshold burst event. `DisorderLevelChanged`, mourning interruption, appointment pause, frontier supply pressure, and public-life edict projection remain later work.

## Module Boundaries

| Module | Owns | Thin-slice behavior |
| --- | --- | --- |
| WorldSettlements | `ImperialBand` / `AmnestyWave` | Emits global `ImperialRhythmChanged` when the imperial axis crosses a band. |
| OfficeAndCareer | Yamen command / docket response | Reads `IWorldSettlementsQueries.GetCurrentSeason().Imperial.AmnestyWave`; emits settlement-scoped `AmnestyApplied` per jurisdiction. |
| OrderAndBanditry | Local disorder state | Consumes `AmnestyApplied`, mutates only the matching settlement, emits `DisorderSpike` on threshold crossing. |
| NarrativeProjection | Read-only notice wording | Projects `DisorderSpike` with cause-neutral guidance until event metadata exists. |

## Scope Rules

- `ImperialRhythmChanged` is global and uses `EntityKey = "imperial"`.
- `AmnestyApplied` is settlement-scoped and uses `EntityKey = settlementId`.
- `DisorderSpike` is settlement-scoped and uses `EntityKey = settlementId`.
- `OfficeAndCareer.LastAppliedAmnestyWave` is persisted de-duplication state so non-amnesty imperial-axis changes do not replay the same amnesty while `AmnestyWave` remains high.

## Save Compatibility

- `OfficeAndCareer` schema moves from `4` to `5`.
- Built-in migration `4 -> 5` initializes `LastAppliedAmnestyWave = 0`.
- Root schema is unchanged.

## Tests

- `AmnestyDispatchHandlerTests`
  - contract declaration
  - high amnesty emits per jurisdiction
  - low amnesty no-op and resets de-duplication
  - already-applied amnesty does not repeat on another imperial rhythm change
- `AmnestyDisorderHandlerTests`
  - settlement-scoped mutation
  - off-scope negative assertion
  - invalid entity key no-op
  - threshold crossing emits `DisorderSpike`
- `ImperialAmnestyDisorderChainTests`
  - real scheduler drains the chain end to end.

## Remaining Design Debt

- Add structured metadata or typed payload to `IDomainEvent` so `ImperialRhythmChanged` can carry `bandKind`, and `DisorderSpike` can carry `causeKey`.
- Implement full chain-4 branches for mourning, appointment pause, frontier supply, public-life edict projection, and `DisorderLevelChanged`.
