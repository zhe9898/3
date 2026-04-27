# Playable Closure Chain Implementation v2 - chain1-public-life-order-v2

Date: 2026-04-24

## Chosen chain

- Chain ID: `chain1-public-life-order-v2`
- Chain name: Public-life road pressure to local watch follow-through
- Player-facing question: The county gate and road surface are heating up; should the home-household spend limited local standing on watchmen, escort, suppression, negotiation, or restraint, knowing the county office may amplify or drag the result next month?
- Why this chain now: It already has real module state, read-model projection, public-life affordances, command routing, module-owned receipt/carryover, and governance readback. v2 should harden the playable proof instead of inventing a broader system.
- Current status: Playable-thin, with v2 acceptance hardening for refusal/fallback, shell-only projection behavior, and architecture guardrails.

## Skill orchestration

1. `zongzu-game-design`: selected a living-world, monthly-review chain with bounded household leverage and next-month readback, not a standalone button.
2. `zongzu-architecture-boundaries`: kept ownership in `OrderAndBanditry`; `Application` routes/adapts; projections and Unity shell stay read-only.
3. `zongzu-pressure-chain`: required because the chain crosses public-life, order, office/governance projection, event/cadence, and acceptance proof.
4. `zongzu-ui-shell`: required because visibility lands in great hall, desk sandbox, public-life node affordances, receipts, and governance docket.
5. `zongzu-ancient-china`: required because this is county-road, yamen, local watch, suppression, and public-opinion pressure in Northern Song semantics.
6. `zongzu-content-authoring`: required for docs and player-facing wording references; no runtime copy is changed in this pass.
7. `zongzu-unity-shell`: required because presentation Unity tests and shell behavior are part of the acceptance proof.
8. `zongzu-simulation-validation`: required because v2 proves scheduler/readback, deterministic command result, optional-module fallback, and architecture invariants.
9. `zongzu-save-and-schema`: not escalated for implementation. No persisted module state, schema version, migration, manifest, or serialized read-history shape changes are planned. Record: no save/schema impact.

## Touched modules, docs, and tests

- Owner module: `Zongzu.Modules.OrderAndBanditry`
- Neighbor read sources: `PublicLifeAndRumor`, `OfficeAndCareer`, `WorldSettlements`, `NarrativeProjection`
- Application routing: `PlayerCommandService` public-life order route, read-model builder
- Shell surfaces: `FirstPassPresentationShell`, desk sandbox settlement nodes, public-life affordances/receipts, governance docket
- Tests:
  - `tests/Zongzu.Modules.OrderAndBanditry.Tests`
  - `tests/Zongzu.Integration.Tests`
  - `tests/Zongzu.Presentation.Unity.Tests`
  - `tests/Zongzu.Architecture.Tests`
- Docs:
  - `docs/MODULE_BOUNDARIES.md`
  - `docs/MODULE_INTEGRATION_RULES.md`
  - `docs/UI_AND_PRESENTATION.md`
  - `docs/ACCEPTANCE_TESTS.md`
  - `docs/GAME_DEVELOPMENT_ROADMAP.md`

## Query / Command / DomainEvent impact

- Query impact: no new query contract. Existing `IOrderAndBanditryQueries.GetSettlementDisorder()` and `SettlementDisorderSnapshot` expose pressure, receipt, outcome, and carryover state to read models.
- Command impact: no new command contract. Existing public-life command names route through `PlayerCommandService.IssueIntent` to `OrderAndBanditryModule.HandlePublicLifeCommand`.
- DomainEvent impact: no new event. Existing upstream pressure events feed disorder/public-life pressure; command receipt/carryover remains owned state rather than a parsed event summary.

## Read-model / projection impact

- Existing projection path:
  - `PresentationReadModelBuilder` reads `SettlementDisorderSnapshot`, `SettlementPublicLifeSnapshot`, and `JurisdictionAuthoritySnapshot`.
  - `BuildPublicLifeAffordances` exposes order choices only from projected public-life/order pressure.
  - `BuildOrderPublicLifeReceipt` surfaces module-owned receipt state.
  - Governance readback uses order carryover and office aftermath state after the next month.
- v2 impact: strengthen tests that the shell consumes projection/read-model state only and tolerates missing public-life/order projection without crashing.

## Unity / presentation boundary impact

- Unity remains host shell / adapter / ViewModel binding.
- No Unity-side command resolution, inheritance/death/marriage/office derivation, or module state mutation.
- Presentation tests will assert public-life affordances/receipts are copied from read models and missing projections fall back safely.

## Save / schema impact

- No save/schema impact.
- No persisted field additions.
- No module schema version change.
- No root save version change.
- No migration or manifest change.
- Existing receipt/carryover state is already in `OrderAndBanditryState` and covered by current schema.

## Determinism impact

- Determinism impact is bounded to existing command mutation and replay hash refresh.
- No scheduler order change.
- No random branch added.
- v2 tests will reuse deterministic bootstraps and compare control/intervention outcomes after the same month advance.

## Closure table

| Link | v2 decision |
| --- | --- |
| Pressure source | Public-life street heat, road-report lag, route pressure, bandit threat, suppression demand |
| Home-household pressure | The household sees roads and county gate pressure through public-life nodes and governance docket |
| Owning module | `OrderAndBanditry` owns disorder pressure, route pressure, local order intervention receipt, carryover |
| Owned state | `SettlementDisorderState` pressure fields plus `LastIntervention*` and `InterventionCarryoverMonths` |
| Cadence | Month review / command rhythm; existing scheduler remains month authority with xun as projection/cadence band only |
| Trigger | Public-life order affordance selected from projected pressure |
| Propagation path | World/settlement pressure and disorder events -> public-life projection -> order command -> order state -> next-month office/governance aftermath |
| Query seam | `IOrderAndBanditryQueries`, `IPublicLifeAndRumorQueries`, `IOfficeAndCareerQueries` |
| Command seam | `PlayerCommandService.IssueIntent` routes existing public-life order command to owner module |
| DomainEvent seam | Existing upstream event drain remains; command resolution does not parse `DomainEvent.Summary` |
| Projection | `PresentationReadModelBuilder` emits affordances, receipts, governance lanes, docket, debug hotspots |
| Shell surface | Great hall governance summary, desk sandbox settlement node, public-life affordances/receipts, governance docket |
| Player leverage | Escort road report, fund local watch, suppress banditry, negotiate with outlaws, tolerate disorder |
| Relationship chain | Household standing and office reach shape execution; local watch/suppression creates office cleanup or drag next month |
| Resistance or delay | Administrative reach profile can improve or clog command impact; refusal occurs for missing settlement, unknown command, disabled module |
| Resolution owner | `OrderAndBanditryModule.HandlePublicLifeCommand` |
| Receipt/refusal | `PlayerCommandResult` and `PlayerCommandReceiptSnapshot` show accepted/refused and outcome summaries |
| Residue/memory | `LastIntervention*`, `InterventionCarryoverMonths`, order administrative aftermath, debug hotspot |
| Next-month readback | Governance lane/focus/docket and office career pressure read back order aftermath after one month |
| Acceptance proof | Module, integration, presentation Unity, and architecture tests |
| Save-schema impact | No save/schema impact |
| Determinism impact | Existing replay hash refresh only; no new nondeterministic path |
| Missing pieces | A future general command bus can replace direct application state access; richer kin/favor cost is later scope |

## Milestone checklist

- [x] Confirm matrix and active skill set.
- [x] Read core docs and inspect live code facts.
- [x] Select chain and status.
- [x] Add v2 acceptance tests for disabled optional module fallback.
- [x] Add v2 presentation shell fallback/projection-only test.
- [x] Add v2 architecture guardrails.
- [x] Update docs with v2 playable closure status.
- [x] Run targeted validation.

## Tests to run

- `git diff --check`
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`

## Validation log

- `git diff --check`: passed.
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 25 tests.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 91 tests.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 25 tests.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 13 tests.

## Final status

- Current chain status: Playable-thin with v2 acceptance hardening.
- No save/schema impact.
- No scheduler or deterministic random changes.
- Runtime code path remains the existing owner-module resolver; v2 adds proof and docs rather than new authoritative state.

## Fallback notes

- If runtime code is already sufficient, v2 is allowed to be acceptance hardening plus docs, because the chain's current weakness is proof density rather than missing state.
- Do not add save/schema work unless a persisted state shape changes.
- Do not add a WorldManager, PersonManager, CharacterManager, UI rule layer, or second application rule layer.
