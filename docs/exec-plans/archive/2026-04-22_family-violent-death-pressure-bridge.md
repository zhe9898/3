# Family Violent Death Pressure Bridge

## Goal
Route external violent / warlike deaths that target a clan member into the same `FamilyCore` death-pressure profile used by ordinary lifecycle deaths, so notices and ancestral-hall guidance can say whether the clan has an adult successor or a severe succession gap.

This is still a bridge, not a funeral, vendetta, adoption, or inheritance-court system.

## Scope in
- consume `DeathByViolence` in `FamilyCore.HandleEvents`
- parse the event `EntityKey` as `PersonId`
- find the matching `FamilyPersonState` and owning clan
- reuse `ComputeDeathImpactProfile` / `ApplyDeathImpactProfile`
- clear a dead current heir and recompute heir security
- record a FamilyCore diff that carries the same pressure-band text as ordinary death
- let `NarrativeProjection` treat diff-only FamilyCore death pressure as ancestral-hall guidance
- add tests for adult-successor versus no-adult-successor violent heir death

## Scope out
- no new persisted fields
- no schema bump
- no new player command
- no recursive event-handling pass
- no complete mourning / burial / adoption / inheritance subsystem
- no SocialMemory blood-feud implementation in this step

## Affected modules
- `src/Zongzu.Modules.FamilyCore`
- `src/Zongzu.Modules.NarrativeProjection`
- `tests/Zongzu.Modules.FamilyCore.Tests`
- `tests/Zongzu.Modules.NarrativeProjection.Tests`
- `docs/ACCEPTANCE_TESTS.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/UI_AND_PRESENTATION.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- FamilyCore mutates only existing clan lifecycle pressure fields
- registered person death remains identity-owned by PersonRegistry; FamilyCore does not emit a second cause-specific death event from this handler

## Determinism risk
- low
- no RNG draws are added
- event processing is ordered by the stable event snapshot and deduplicated by `PersonId`
- pressure output reuses existing deterministic FamilyCore formulas

## Milestones
1. Add this ExecPlan and keep scope bounded.
2. Implement the `DeathByViolence` pressure bridge in FamilyCore.
3. Teach NarrativeProjection to produce death guidance from FamilyCore death-pressure diffs even when the cause event came from another module.
4. Add module tests for violent current-heir death with adult successor versus only young/no adult successor.
5. Update docs and run targeted plus solution-level validation.

## Verification
- targeted FamilyCore and NarrativeProjection module tests
- existing family lifecycle integration filter
- `dotnet test .\Zongzu.sln -c Debug --no-restore`
- `git diff --check`

## Verification result
- targeted FamilyCore filter passed: 2/2
- targeted NarrativeProjection filter passed: 3/3
- targeted family lifecycle integration filter passed: 4/4
- full solution validation passed
- `git diff --check` passed; Git reported existing LF-to-CRLF normalization warnings only
