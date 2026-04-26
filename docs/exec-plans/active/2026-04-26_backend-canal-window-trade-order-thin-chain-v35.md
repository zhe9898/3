# Backend Canal Window Chain v35: Trade / Order Owner-Lane Hookup

## Goal
- Graduate `WorldSettlements.CanalWindowChanged` from diagnostic future-contract debt into a thin, real owner-module handoff.
- Keep the design as a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` is only deterministic fact propagation after owner rules resolve; it is not an event-pool core loop.
- Let canal-window changes press the existing `TradeAndIndustry` route/market lane and the existing `OrderAndBanditry` route-pressure lane without introducing thick canal economy, yamen formulas, route AI, or household target ledgers.

## Scope In
- Add structured canal-window metadata (`canalWindowBefore`, `canalWindowAfter`) to `WorldSettlements.CanalWindowChanged`.
- Add `CanalWindowChanged` to `TradeAndIndustry` and `OrderAndBanditry` consumed event declarations.
- In `TradeAndIndustry`, use `IWorldSettlementsQueries` and existing route/market/black-route ledger fields to adjust only trade-owned state for water/canal-exposed settlements.
- In `OrderAndBanditry`, use `IWorldSettlementsQueries` and existing route/order pressure fields to adjust only order-owned state for water/canal-exposed settlements.
- Update event-contract health diagnostics so `CanalWindowChanged` is no longer classified as emitted-without-authority-consumer debt.

## Scope Out
- No new command system, event pool, owner-lane ledger, cooldown ledger, canal ledger, household target field, route AI, yamen formula, or UI-owned rule.
- No new persisted state, root save-version bump, module schema bump, migration, save-manifest change, or save roundtrip behavior.
- No Application/UI/Unity authority; no Unity module queries; no SocialMemory write path.
- No parsing of `DomainEvent.Summary`, receipt prose, report prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

## Affected Modules
- `WorldSettlements`: still owns canal-window state and emits the structured fact.
- `TradeAndIndustry`: owns market risk, route risk, blocked shipments, seizure risk, and black-route ledger adjustments caused by canal-window pressure.
- `OrderAndBanditry`: owns local route pressure, black-route pressure, suppression demand, and route-shielding adjustments caused by canal-window pressure.
- `tests`: focused module tests plus architecture and event-health diagnostics.
- `docs`: topology, boundaries, schema/no-migration, simulation, UI/presentation, acceptance evidence.

## Save / Schema Impact
- V35 has no persisted state impact. `TradeAndIndustry` remains schema `4`; `OrderAndBanditry` remains schema `9`; `WorldSettlements` remains schema `8`.
- The new metadata constants are runtime event payload keys only. They are not saved fields, ledgers, indexes, or module envelopes.
- If implementation requires persisting canal-window aftermath, owner-lane state, diagnostic state, or projected readback cache, stop and convert this into a schema/migration plan before code changes.

## Determinism Risk
- Low. The handler targets are selected from existing deterministic `WorldSettlements` query snapshots and sorted state lists.
- The handlers use existing state fields and fixed deltas by structured `CanalWindow` enum names. They do not use wall-clock time, random choices, summary text, UI state, or external services.

## Milestones
1. Add this ExecPlan with no-schema target.
2. Add structured canal-window metadata on the WorldSettlements event.
3. Add Trade and Order owner-lane consumers that mutate only their own existing fields.
4. Add focused tests for Trade and Order canal-window handling, off-scope negative assertions, and metadata/prose separation.
5. Update event-contract health diagnostics to remove `CanalWindowChanged` from future debt.
6. Update docs and run validation.
7. Commit and push `codex/backend-canal-window-chain-v35`.

## Tests To Add / Update
- `CanalWindowChanged_Closed_AdjustsTradeOwnedWaterRouteStateOnly`
- `CanalWindowChanged_Closed_ReturnsRoutePressureToOrderOwnedLane`
- `Canal_window_owner_lane_handlers_must_use_structured_world_reads`
- `EventContractHealth_CanalWindowChangedHasTradeAndOrderAuthorityConsumers`
- Existing ten-year health report must no longer list `WorldSettlements.CanalWindowChanged` as emitted-without-authority-consumer debt.

## Rollback / Fallback Plan
- If the thin deltas make long-run pressure too noisy, keep the consumed-event declarations and reduce the profile deltas while preserving owner-lane tests.
- If water/canal exposure cannot be selected from `IWorldSettlementsQueries`, stop before adding a persisted target list and document the required schema/migration impact.
- If diagnostics still see `CanalWindowChanged` as unconsumed, treat it as an integration ordering bug, not as a reason to reclassify it as future debt.

## Evidence Checklist
- [x] ExecPlan created
- [x] structured canal-window metadata added
- [x] Trade owner-lane handler added
- [x] Order owner-lane handler added
- [x] focused Trade test passed
- [x] focused Order test passed
- [x] focused architecture test passed
- [x] focused event-health test passed
- [x] docs updated
- [x] no schema/migration impact documented in docs
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture / Unity presentation tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [ ] commit and push

## Validation Evidence
- `dotnet test tests\Zongzu.Modules.TradeAndIndustry.Tests\Zongzu.Modules.TradeAndIndustry.Tests.csproj --no-restore --filter CanalWindow` passed 1 test.
- `dotnet test tests\Zongzu.Modules.OrderAndBanditry.Tests\Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore --filter CanalWindow` passed 1 test.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter Canal_window_owner_lane_handlers_must_use_structured_world_reads` passed 1 test.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter EventContractHealth_CanalWindowChangedHasTradeAndOrderAuthorityConsumers` passed 1 test.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Modules.TradeAndIndustry.Tests\Zongzu.Modules.TradeAndIndustry.Tests.csproj --no-build --filter CanalWindow` passed 1 test.
- `dotnet test tests\Zongzu.Modules.OrderAndBanditry.Tests\Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-build --filter CanalWindow` passed 1 test.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Canal_window_owner_lane_handlers_must_use_structured_world_reads|Event_contract_health|summary|forbidden_manager|PersonRegistry|Presentation_Unity"` passed 9 tests.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "EventContractHealth|CampaignEnabledStressSandbox_TenYearHealthReport"` passed 6 tests; the 120-month report shows `WorldSettlements.CanalWindowChanged: emitted=41, authorityConsumed=82, consumers=[OrderAndBanditry,TradeAndIndustry]`.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 31 tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed; integration suite count is now 130 tests.
