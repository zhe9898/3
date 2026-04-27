# Public-Life Order Leverage Closure v3

Date: 2026-04-24

## Chosen chain

- Chain ID: `chain1-public-life-order-leverage-v3`
- Chain name: Public-life road pressure to home-household local-watch leverage residue readback
- Player-facing question: The county gate and road surface are heating up; which home-household leverage can be spent here, what does it cost, and what will still echo through local relations, office handling, trade routes, or ground pressure next month?
- Why this chain now: v2 already proves public-life/order pressure, owner-module command resolution, receipt/refusal, governance docket readback, disabled-module fallback, and shell-only projection behavior. v3 should deepen the same closed lane by making household leverage, cost, and next-month residue readable without opening a new system.
- Current status: `Playable-thin` / `Missing-leverage-readback` / `Missing-residue-explanation` / `Missing-shell-field` / `Missing-v3-acceptance`.

## Skill orchestration

1. `zongzu-game-design`: keep the player in the home-household seat with bounded leverage rather than omnipotent order buttons.
2. `zongzu-architecture-boundaries`: preserve module ownership, the shared command seam, and projection-only UI/Unity surfaces.
3. `zongzu-pressure-chain`: keep the chain ledger explicit from pressure source through next-month readback.
4. `zongzu-ui-shell`: make leverage/cost/readback visible on hall, desk, public-life receipts, and governance docket projections.
5. `zongzu-ancient-china`: keep leverage language grounded in lineage face, yamen reach, watch labor, market/route exposure, mediation, and ground risk.
6. `zongzu-content-authoring`: triggered because player-facing projection wording and docs change.
7. `zongzu-unity-shell`: triggered because ViewModels/adapters/presentation tests change.
8. `zongzu-simulation-validation`: triggered because acceptance proof covers command resolution and next-month readback.
9. `zongzu-save-and-schema`: not escalated. No persisted state, schema version, migration, or manifest change is planned.

## Touched modules, docs, and tests

- Contracts/read models:
  - `PlayerCommandAffordanceSnapshot`
  - `PlayerCommandReceiptSnapshot`
  - `GovernanceDocketSnapshot`
- Owner module:
  - `OrderAndBanditry` remains the only resolver/mutator for public-life order commands.
- Read sources:
  - `PublicLifeAndRumor`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `SocialMemoryAndRelations` projections where already present, and `TradeAndIndustry` route/asset projections.
- Application projection:
  - `PresentationReadModelBuilder` composes leverage/cost/readback strings from existing snapshots only.
- Shell:
  - `Zongzu.Presentation.Unity.ViewModels`
  - `Zongzu.Presentation.Unity` adapters
  - Unity host mirrored shared command ViewModels under `unity/Zongzu.UnityShell`
  - `Zongzu.Presentation.Unity.Tests`
- Docs:
  - `MODULE_BOUNDARIES.md`
  - `MODULE_INTEGRATION_RULES.md`
  - `UI_AND_PRESENTATION.md`
  - `ACCEPTANCE_TESTS.md`
  - `GAME_DEVELOPMENT_ROADMAP.md`
  - `PLAYER_SCOPE.md`
  - `RELATIONSHIPS_AND_GRUDGES.md`
  - `SIMULATION.md`
  - `DATA_SCHEMA.md`

## Query / Command / DomainEvent impact

- Query impact: no new query interface. The projection uses existing read models from the presentation bundle.
- Command impact: no new command name. Existing public-life order commands continue through `PlayerCommandCatalog` -> `GameSimulation.IssueModuleCommand(...)` -> `OrderAndBanditryModule.HandleCommand(...)` -> `OrderAndBanditryCommandResolver`.
- DomainEvent impact: no new event. No rule input parses `DomainEvent.Summary`.

## Read-model / projection impact

- Add runtime-only read-model fields:
  - leverage summary
  - cost summary
  - readback summary
- Affordances explain which household leverage is being spent before the command.
- Receipts explain the cost or backlash of the accepted/refused handling.
- Governance docket carries recent receipt leverage/cost/readback into the next-month readback surface.

## Unity / presentation boundary impact

- Unity ViewModels gain display-only fields for leverage, cost, and readback.
- Adapters copy fields from read models only.
- No Unity-side rule evaluation, module state access, command resolution, or mutation is added.

## Save / schema impact

- No save/schema impact.
- No module state field is added.
- No module schema version changes.
- No root save version changes.
- No migrations or feature-manifest changes.
- `DATA_SCHEMA.md` will document the new fields as runtime-only read-model data.

## Determinism impact

- No scheduler order change.
- No random branch added.
- No persisted state shape change.
- Determinism risk is limited to existing accepted command mutation and replay hash refresh already covered by v2.

## Closure table

| Link | v3 decision |
| --- | --- |
| Pressure source | Public-life road heat, road-report lag, route pressure, disorder pressure, bandit threat, suppression demand |
| Owned order/public-life state | `PublicLifeAndRumor` owns venue/public heat; `OrderAndBanditry` owns disorder pressure and intervention receipt/carryover |
| Spatial shell visibility | Hall docket, desk settlement node, public-life affordances/receipts, governance docket |
| Household leverage explanation | Projection labels lineage face, yamen/document reach, cash/watch labor, trade route exposure, elder mediation, and tolerated ground risk from existing read models |
| Bounded command | Existing order verbs: `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, `TolerateDisorder` |
| Module-owned resolution | `OrderAndBanditry` resolver mutates only order-owned state |
| Receipt/refusal | Existing command result and player-command receipt; v3 adds projected leverage/cost/readback text |
| Relationship or obligation residue | No new SocialMemory state. v3 explains residue as order carryover, office aftermath, trade/route exposure, family face, and public-life pressure; durable social-memory mutation remains future owner-module work |
| Next-month readback | Governance docket and receipt projection carry readback text after one monthly advance |
| Acceptance proof | Module, integration, presentation Unity, architecture, and diff checks |

## Milestones

- [x] Confirm matrix, skills, docs, and clean initial worktree.
- [x] Inspect live command seam, order resolver, projections, Unity adapters, query seams, and tests.
- [x] Select v3 minimal closure without persisted state.
- [x] Add read-model contract fields and projection helper.
- [x] Thread leverage/cost/readback into affordances, receipts, governance docket, hall prompt, and Unity adapters.
- [x] Add/extend integration and presentation Unity proof; module ownership remains covered by existing owner tests and the required module test run.
- [x] Update docs.
- [x] Run targeted validation.

## Tests to run

- `git diff --check`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false`

## Validation log

- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false` - passed, 25/25.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore /p:UseSharedCompilation=false` - passed, 105/105.
- `git diff --check` - passed.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false` - passed, 14/14.
- `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-restore /p:UseSharedCompilation=false` - passed, 29/29.
