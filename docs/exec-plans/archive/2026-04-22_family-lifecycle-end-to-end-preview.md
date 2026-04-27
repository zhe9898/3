# Family Lifecycle End-to-End Preview

## Goal
Pin the current family lifecycle slice into one deterministic, viewable loop:
world death pressure -> notice / ancestral-hall guidance -> bounded family command -> receipt -> next-month readable state.

This is still MVP plumbing. It must not become a full funeral, adoption, inheritance court, or kinship-detail system.

## Scope in
- align `MvpFamilyLifecyclePreviewScenario` lifecycle command priority with the hall / family-council selector
- ensure family lifecycle command candidate selection respects `PersonRegistry` authority for alive and age checks
- add a fixed integration scenario for current-heir death with no adult successor
- prove `议定承祧` appears in family guidance before command resolution
- prove the command receipt writes only `FamilyCore` lifecycle state and does not select the deceased heir again
- prove the next month still carries readable family lifecycle state

## Scope out
- no new player command
- no new persisted field
- no schema bump
- no command handling refactor into modules yet
- no complete funeral, adoption, branch court, or inheritance workflow
- no UI-owned rule inference

## Affected modules
- `src/Zongzu.Application`
- `tests/Zongzu.Integration.Tests`
- `docs/ACCEPTANCE_TESTS.md`
- `docs/MODULE_INTEGRATION_RULES.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- command resolution now reads `PersonRegistry` queries for identity facts when available, falling back to local `FamilyCore` mirrors for transitional / test-only persons

## Determinism risk
- low
- no RNG draws are added
- the new candidate filter is query-derived and deterministic
- lifecycle preview ordering now matches the existing deterministic hall and family-council priority rules

## Milestones
1. Add this ExecPlan and keep the slice bounded.
2. Update family lifecycle command candidate selection to respect PersonRegistry alive / age facts.
3. Align MVP lifecycle preview selection with hall lifecycle priority.
4. Add the death -> notice -> `议定承祧` -> receipt -> next month integration test.
5. Update docs and run targeted plus solution-level validation.

## Verification
- targeted family lifecycle integration tests
- `dotnet test .\Zongzu.sln -c Debug --no-restore`
- `git diff --check`

## Verification result
- targeted integration filter passed: 4/4
- full solution validation passed
- `git diff --check` passed; Git reported existing LF-to-CRLF normalization warnings only
