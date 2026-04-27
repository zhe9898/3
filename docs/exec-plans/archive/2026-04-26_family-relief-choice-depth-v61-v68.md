# Family Relief Choice Depth v61-v68

## Goal

Continue after v53-v60 by turning the already-readable Family lane pressure into one thin, bounded FamilyCore-owned relief choice.

This is the next topology priority after v36 sponsor-clan pressure and v53-v60 Family lane closure readback: when household burden has reached `FamilyCore`, the player should see and issue a narrow Family choice to spend lineage support or leave sanction pressure behind. It remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` is still only a deterministic fact propagation tool, not the design body.

The player-facing read should become:

> This is not an ordinary household repair. If a sponsor clan is carrying charity obligation, support reserve strain, branch tension, and relief sanction pressure, the Family lane can choose whether to open lineage relief. The cost and aftertaste stay in `FamilyCore` and later `SocialMemoryAndRelations`, not in UI or Population.

## Version Slices

- v61: Add this ExecPlan and freeze the no-schema target.
- v62: Add a `FamilyCore` command for opening lineage relief from existing clan pressure fields.
- v63: Resolve the command only inside `FamilyCore`, mutating existing `CharityObligation`, `SupportReserve`, `BranchTension`, `ReliefSanctionPressure`, `MediationMomentum`, and conflict receipt fields.
- v64: Project a Family-facing affordance/readback that says `Family救济选择读回`, `宗房余力读回`, `接济义务读回`, and `不是普通家户再扛`.
- v65: Ensure receipts surface the Family choice without parsing receipt prose or creating a relief ledger.
- v66: Keep Unity/shell copy-only through existing command affordance/receipt fields.
- v67: Add focused module/integration/architecture tests.
- v68: Update docs, validation evidence, and push the branch.

## Scope In

- Add a new bounded player command routed to `FamilyCore`, tentatively `GrantClanRelief`.
- Use existing persisted `FamilyCore` fields only:
  - `CharityObligation`
  - `SupportReserve`
  - `BranchTension`
  - `ReliefSanctionPressure`
  - `MediationMomentum`
  - existing conflict command/trace/outcome receipt fields
- Add Family-facing affordance copy that explains:
  - the command spends宗房余力;
  - it reduces接济义务 / 制裁压力 / 房支争力 only if `FamilyCore` resolves it;
  - ordinary `PopulationAndHouseholds` local response remains low-power local easing/strain;
  - later durable residue remains `SocialMemoryAndRelations`.
- Add focused tests proving command-time mutation is only `FamilyCore`, no SocialMemory same-month writes, and no new schema/migration.

## Scope Out

- No new persisted state.
- No schema bump, root save version bump, migration, save manifest change, or persisted projection cache.
- No relief ledger, charity ledger, guarantee ledger, Family closure ledger, owner-lane ledger, cooldown ledger, command queue, household target field, or repeated-response counter.
- No thick clan economy, clan elder AI, branch-faction model, full guarantee formula, or full welfare/relief subsystem.
- No Application command-result authority. Application may route/catalog/project only.
- No UI/Unity calculation of relief outcome, support cost, sponsor targeting, Family closure, guarantee success, or social residue.
- No UI/Unity writes to `SocialMemoryAndRelations`.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, SocialMemory summary prose, or Family projection prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

## Affected Modules

- `FamilyCore`: owns and resolves the new relief choice against existing clan pressure fields.
- `Zongzu.Contracts`: adds a command name constant only; no persisted DTO/state shape.
- `Zongzu.Application`: adds catalog route and projected affordance/readback text only.
- `PopulationAndHouseholds`: remains owner of ordinary household distress/debt/labor/migration and local response traces; no mutation from this command.
- `SocialMemoryAndRelations`: remains later-month durable residue owner; no same-month mutation from this command.
- `Zongzu.Presentation.Unity` / Unity shell: existing command DTOs/adapters copy projected fields only.
- Tests/docs: update focused command, architecture, schema/no-migration, UI, simulation, and acceptance evidence.

## Save / Schema Impact

No persisted shape changes are planned.

- `FamilyCore` remains at schema `8`.
- `PopulationAndHouseholds` remains at schema `3`.
- `SocialMemoryAndRelations` remains at schema `3`.
- No root save version change.
- No migration.
- No save manifest update.

The command mutates existing `FamilyCore` persisted fields through the existing module command seam. If implementation requires a new relief ledger, charity ledger, household target, sponsor-lane ledger, SocialMemory field, persisted receipt cache, or projection cache, stop before adding it and create a schema/migration plan.

## Determinism Risk

Low. The command reads a selected clan from the existing command target, resolves fixed formulas over existing integer fields, and writes only `FamilyCore` state. It must not depend on random choices, wall-clock time, UI text, DomainEvent summaries, receipt prose, or SocialMemory summary prose.

## Milestones

1. Create this ExecPlan.
2. Add `GrantClanRelief` to command contracts/catalog and route it to `FamilyCore`.
3. Implement the `FamilyCore` resolver profile using existing fields only.
4. Add Family-facing affordance/readback projection.
5. Add focused module, integration, architecture, and presentation-relevant tests.
6. Update docs with v61-v68 scope and no-schema evidence.
7. Run build, focused tests, `git diff --check`, and full solution tests.
8. Commit and push `codex/family-relief-choice-v61-v68`.

## Tests To Add / Update

- `GrantClanRelief` reduces existing Family pressure and spends only existing `FamilyCore` support reserve.
- Command-time relief choice mutates `FamilyCore` only; `PopulationAndHouseholds` and `SocialMemoryAndRelations` remain untouched.
- Family-facing affordance includes `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛`.
- Command catalog / presentation affordances remain aligned.
- Architecture guard: no new schema/migration, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no summary parsing, no relief/charity/owner-lane ledger.

## Rollback / Fallback Plan

If a useful Family relief choice cannot be expressed through existing `FamilyCore` fields, stop and leave v60 as the stable readback baseline. Do not add a hidden ledger or move relief authority into Application/UI/Unity/Population.

## Open Questions

- None blocking. Default implementation should be a narrow sibling to the existing `SuspendClanRelief` command: same Family surface, opposite relief posture, existing state only.

## Evidence Checklist

- [x] ExecPlan created
- [x] command contract/catalog added
- [x] FamilyCore command resolver added
- [x] Family-facing affordance/readback added
- [x] focused tests added
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore` (passes)
- [x] focused tests (FamilyCore, Integration, Unity presentation pass)
- [x] `git diff --check` (passes)
- [x] `dotnet test Zongzu.sln --no-build` (passes)
- [x] commit `c065104` (`Add family relief choice command v61-v68`)
- [x] push `origin/codex/family-relief-choice-v61-v68`
