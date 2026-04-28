# Personnel Command Preflight V293-V300

## Goal

Add the next small guard after v285-v292: before Zongzu adds any player-facing command that influences personnel movement, migration, assignment, return, or settlement placement, the command must be owner-laned and scale-budgeted.

This pass is a preflight audit. It does not add a new command. It documents the rule that future personnel-flow commands are bounded influence intents, not direct player control over people and not Application/UI/Unity-calculated movement.

## Scope In

- Document future personnel-command gates:
  - command owner module must be named before implementation;
  - target scope must be household, clan, settlement, office, route, or campaign-lane specific;
  - hot path, expected cardinality, deterministic cap/order, no-touch boundary, schema impact, and validation lane must be declared;
  - player-facing text must keep the difference between "influence pressure" and "move this person because UI selected them."
- Record current supported seams:
  - `PopulationAndHouseholds` already owns household livelihood/activity/migration pressure and local household response commands.
  - `PersonRegistry` owns identity and existing `FidelityRing` assignment only; `ChangeFidelityRing` is not a movement command.
  - `FamilyCore`, `OfficeAndCareer`, `WarfareCampaign`, and future owner lanes may request personnel effects only through their own command/rule contracts.
  - Application routes commands; UI/Unity copy command affordances and receipts only.
- Add an architecture guard proving the current code has no direct personnel command, no movement command ledger, no Application-owned personnel rule path, and no `PersonRegistry` domain expansion.

## Scope Out

- No production rule change.
- No new player command.
- No direct move-person / transfer-person / summon-person command.
- No complete personnel-flow system.
- No migration economy, route placement engine, office appointment pipeline, campaign manpower assignment, or household resettlement formula.
- No durable SocialMemory movement residue.
- No new persisted state, schema version, root save version, migration, manifest change, projection cache, or serialized module payload change.
- No command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, or cooldown ledger.
- No Application command outcome calculation.
- No UI/Unity authority.
- No prose parsing from command text, readback fields, person dossier text, settlement mobility text, receipt prose, notification text, or `DomainEvent.Summary`.

## Affected Modules

- `docs/*`: topology, design audit, player scope, boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation, acceptance, and skill alignment notes.
- `tests/Zongzu.Architecture.Tests`: preflight guard.

No runtime module, command catalog, scheduler, projection, ViewModel, Unity adapter, persistence, schema, or manifest code is planned.

## Save / Schema Impact

Target impact: none.

This pass is docs/tests only. If implementation requires persisted state, stop and split a schema/migration plan before coding.

## Determinism / Performance Risk

No runtime determinism or performance risk is expected because this pass changes no scheduler, module, command, projection, save, or Unity implementation code.

The future risk being guarded is high: personnel commands can easily become all-world scans, UI-owned movement, hidden assignment ledgers, or `PersonRegistry` domain expansion. Future implementation must declare cardinality and deterministic caps before any rule code lands.

## Milestones

1. Create this ExecPlan and confirm the no-schema target.
2. Update docs with v293-v300 personnel-command preflight gates.
3. Add architecture guard for no direct personnel command drift.
4. Run focused architecture/build/diff/full validation.
5. Archive this plan with validation evidence after merge readiness.

## Tests To Add / Update

- Architecture guard proving v293-v300 is docs/tests governance only, has explicit future command gates, preserves current owner lanes, and forbids direct move/transfer/summon/assign personnel commands, ledgers, manager paths, summary parsing, schema drift, and Application/UI/Unity authority.

## Rollback / Fallback

If the guard becomes too broad, narrow it to the stable invariant: future personnel commands must be owner-laned and bounded; `PersonRegistry` remains identity/fidelity only; Application routes only; UI/Unity copy only; no schema or runtime command is added in this pass.

## Implementation Evidence

- Added v293-v300 preflight wording to topology, player scope, design audit, module boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation docs, acceptance tests, and skill rationalization matrix.
- Added `Personnel_command_preflight_v293_v300_must_block_direct_personnel_command_drift` to architecture tests.
- Confirmed current command seams remain unchanged: `PopulationAndHouseholds` owns local household response commands, `PersonRegistry.ChangeFidelityRing` remains identity/fidelity only, and Application routes through `IssueModuleCommand`.
- No production module, command catalog, scheduler, projection, ViewModel, Unity adapter, persistence, schema, or manifest code changed.
- Save/schema result: none. The pass is docs/tests only.

## Validation Evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "FullyQualifiedName~Personnel_command_preflight_v293_v300"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed: 0 failed, full solution green.

## Closure

Completed and ready to archive. V293-V300 is a personnel-command preflight only. It adds no runtime authority, schema, migration, direct movement command, command ledger, or complete personnel-flow system.
