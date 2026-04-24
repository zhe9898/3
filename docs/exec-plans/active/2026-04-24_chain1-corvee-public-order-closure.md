# Chain 1 Corvee Public Order Closure

Date: 2026-04-24

## Goal

Advance one playable closure chain from visible county/public-life pressure into bounded local-order intervention, module-owned resolution, receipt, residue, and next-month readback.

Chosen chain:
- Chain ID: `chain1-corvee-public-order-closure`
- Chain name: Corvee / public disorder -> local watch follow-through
- Player-facing question: `徭役、路情与县门街谈已经压到本户可见的地方；本户要添雇巡丁、严缉路匪、遣人议路，还是暂缓穷追，并承担下月的县署后账？`
- Why this chain now: current code already has real scheduler pressure, `OrderAndBanditry` state, `PublicLifeAndRumor` read models, public-life command affordances, receipts, and governance next-month readback. The missing closure point is that public-life order commands still resolve in `Zongzu.Application` instead of the owning `OrderAndBanditry` module.
- Current status before this pass: Spec-supported, Contract-supported, Implemented-thin, Missing module-owned command seam, Missing explicit closure acceptance.
- Target status after this pass: Playable-thin for the selected order/public-life closure, with full-chain balancing and deeper household tax formulas still deferred.

## Scope In

- Move public-life order command effects from `PlayerCommandService` into an `OrderAndBanditry` module-owned command resolver.
- Keep application code as a thin router that checks feature enablement, evaluates office reach through queries, and calls the owning module.
- Add/adjust tests proving command acceptance/refusal, module-owned receipt state, deterministic replay hash behavior, public-life shell visibility, and next-month governance readback.
- Add documentation notes for the closure status and no save/schema impact.

## Scope Out

- No new `WorldManager`, `PersonManager`, `CharacterManager`, or god controller.
- No `PersonRegistry` expansion.
- No UI or Unity authority logic.
- No new save namespace, module schema version, migration, or feature pack.
- No full historical tax/corvee formula implementation.
- No commit or push.

## Affected Modules

- `Zongzu.Modules.OrderAndBanditry`: owns local disorder state, public-life order command resolution, intervention receipt, and one-month carryover.
- `Zongzu.Application`: routes player intent only; evaluates office reach and refreshes replay hash.
- `Zongzu.Contracts`: read model/command names already exist; only minimal metadata changes if needed.
- `Zongzu.Presentation.Unity`: read-only shell should continue consuming projected affordances and receipts only.

## Touched Docs

- `docs/exec-plans/active/2026-04-24_chain1-corvee-public-order-closure.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/MODULE_BOUNDARIES.md`
- `docs/UI_AND_PRESENTATION.md`
- `docs/ACCEPTANCE_TESTS.md`
- `docs/GAME_DEVELOPMENT_ROADMAP.md`

## Query Impact

Existing query impact only:
- `IOrderAndBanditryQueries.GetSettlementDisorder()` continues to expose local disorder plus last intervention receipt/carryover.
- `IOfficeAndCareerQueries.GetRequiredJurisdiction()` remains the source for office reach modifiers.
- Presentation read models keep joining public-life, order, and office projections read-only.

No new query contract is expected.

## Command Impact

- `OrderAndBanditry` becomes the owner of public-life order command resolution for:
  - `EscortRoadReport`
  - `FundLocalWatch`
  - `SuppressBanditry`
  - `NegotiateWithOutlaws`
  - `TolerateDisorder`
- `PlayerCommandService` remains the router and result adapter.
- Disabled-pack and missing-settlement refusals remain explicit.

## DomainEvent Impact

No new event name is expected.

The chain continues to use existing pressure events:
- `WorldSettlements.CorveeWindowChanged`
- `OrderAndBanditry.DisorderSpike`
- downstream public-life reactions to disorder spikes

Command receipts remain owned state/read-model residue rather than new domain events in this thin pass.

## Read Model / Projection Impact

- Public-life affordances and receipts remain exposed through `PlayerCommandSurfaceSnapshot`.
- `GovernanceDocket` and governance lane readback continue to show order-linked office aftermath when next-month office processing reads order carryover.
- Hall and desk shell adapters stay read-only.

## Unity / Presentation Boundary Impact

No Unity authority changes. Presentation continues to display:
- public-life pressure on desk settlement nodes
- command affordances
- recent receipts
- governance next-month readback

Shell code must not call module state or resolve commands directly.

## Save / Schema Impact

No save/schema impact.

The selected chain uses existing `OrderAndBanditryState` fields:
- `LastInterventionCommandCode`
- `LastInterventionCommandLabel`
- `LastInterventionSummary`
- `LastInterventionOutcome`
- `InterventionCarryoverMonths`

No schema version bump or migration is required.

## Determinism Impact

Low deterministic-command impact.

Command resolution remains pure and deterministic for the same simulation state, office-reach projection, command name, and settlement id. Replay hash changes after accepted commands are expected and should match across identical command sequences.

## Closure Table

| Link | This pass |
| --- | --- |
| Pressure source | Corvee/tax/service and local disorder pressure already enters through world/order/public-life slices; the executable closure test uses governance-local-conflict bootstrap plus existing corvee/disorder tests as upstream proof. |
| Home-household pressure | The pressure is visible from the home-household seat as county-gate/street/route disorder and local watch burden rather than as an omniscient map control. |
| Owning module | `OrderAndBanditry` owns disorder, route pressure, local watch effects, crackdown backlash, negotiation leakage, tolerance residue, and intervention carryover. |
| Owned state | `SettlementDisorderState` fields listed above. |
| Cadence | Monthly player review/command; order state and office/public-life readback settle through monthly scheduler, with xun only as transitional/projection grouping where existing modules use it. |
| Trigger / threshold | Existing route/disorder/bandit/suppression pressure thresholds enable public-life affordances and receipts. |
| Propagation path | World/settlement pressure -> order/public-life projections -> player command -> `OrderAndBanditry` owned mutation -> next-month office/public-life readback. |
| Query seam | `IOrderAndBanditryQueries`, `IPublicLifeAndRumorQueries`, `IOfficeAndCareerQueries`. |
| Command seam | Thin application router calls `OrderAndBanditry` module-owned public-life order resolver. |
| DomainEvent seam | Existing disorder/corvee events remain; command receipt is state residue in this thin pass. |
| Projection/read model | `PresentationReadModelBundle.PlayerCommands`, `GovernanceSettlements`, `GovernanceFocus`, `GovernanceDocket`, hall docket. |
| Shell surface | Great hall governance docket and desk sandbox public-life settlement nodes. |
| Player leverage | Money/manpower for watch, coercive pressure, negotiation channel, restraint/tolerance, and office reach modifiers when available. |
| Relationship chain required | Local household/lineage/public-life reach plus optional office jurisdiction reach; no direct god control. |
| Who can resist/delay/reinterpret | Local disorder actors, outlaws, clerks/yamen workload, public rumor channels, and office implementation drag. |
| Resolution owner | `OrderAndBanditry`. |
| Receipt/refusal | `PlayerCommandResult` plus projected `PlayerCommandReceiptSnapshot`; refusals for disabled module, missing settlement, or unknown command. |
| Residue/memory | One-month `InterventionCarryoverMonths`, route shielding, retaliation risk, black-route pressure, office aftermath readback. |
| Next-month readback | Governance lane/docket and debug hotspot show office cleanup, backlog, and order-linked aftermath after the next advance. |
| Acceptance proof | Module unit tests, integration closure test, presentation test, architecture test, `git diff --check`, targeted dotnet tests. |
| Save/schema impact | No save/schema impact. |
| Determinism impact | Accepted command mutates only owned deterministic state and refreshes replay hash; identical command sequence must match. |
| Missing pieces | Full tax/corvee household formulas, household-specific cost payment, social-memory durable favor/shame entries, full Renzong chain balancing, UI scene polish. |

## Milestones

- [x] M1: Add module-owned public-life order command resolver in `OrderAndBanditry`.
- [x] M2: Thin `PlayerCommandService` routing so Application no longer owns order command effects.
- [x] M3: Add module tests for receipt/refusal/residue and reuse integration coverage for next-month readback.
- [x] M4: Verify presentation still sees read-only affordances/receipts without direct module access.
- [x] M5: Update docs with closure status and no save/schema impact.
- [x] M6: Run targeted validation.

## Tests To Run

- `git diff --check`
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`

## Validation Log

- `git diff --check`: passed; only existing line-ending warnings for `docs/ARCHITECTURE.md` and `docs/STATIC_BACKEND_FIRST.md`.
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 25 tests.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 90 tests.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 24 tests.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`: passed, 9 tests.

## Final Closure Status

Playable-thin for `chain1-corvee-public-order-closure`.

The player can see public-life/order pressure from the home-household shell projections, choose a bounded local-order intervention, receive an order-owned acceptance/refusal/receipt, and read next-month order-linked governance fallout.  This does not complete the full Renzong tax/corvee society chain; household-grade tax/corvee formulas, durable social-memory entries, and deeper cost payment remain future work.

## Rollback / Fallback Plan

If the command resolver refactor is too wide, keep the existing application-routed behavior but add a smaller module-local helper and document it as a transitional command seam. If integration tests reveal unrelated dirty-worktree drift, isolate this pass to the new module resolver, one closure test, and the ExecPlan.

## Open Questions

- Should the eventual permanent command seam live on `ModuleRunner<TState>` or a parallel command-handler interface?
- Should command receipts become structured domain events in a later full-chain pass, or remain state residue plus read model for order/public-life commands?
