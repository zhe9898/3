# Family Death Guidance Projection

## Goal
Project the existing death-succession pressure profile into notice and ancestral-hall guidance so the player can read whether the next pressure is mourning order, succession designation, or branch containment.

This remains a projection slice. It must not become a full funeral system, inheritance workflow, adoption system, or UI-owned command resolver.

## Scope in
- append the `FamilyCore` death pressure summary to death `WorldDiff` entries
- let `NarrativeProjection` distinguish severe succession gaps from adult-successor deaths in `WhatNext`
- give infant/illness death and heir appointment/succession events dedicated family-facing projection handling
- let great hall, family council, and notification center choose the lead lifecycle prompt from existing family command affordances
- prefer `议定承祧` when the current death trace leaves no heir / a severe `承祧缺口3阶`
- prefer `议定丧次` when the heir death has an adult successor and mourning remains the lead pressure
- add tests for notice guidance and shell prompt alignment

## Scope out
- no new persisted fields
- no schema bump
- no new player command
- no complete mourning, funeral, inheritance, adoption, or branch-court system
- no UI mutation or authority rules
- no numeric rebalance beyond the already existing death-impact profile

## Affected modules
- `src/Zongzu.Modules.FamilyCore`
- `src/Zongzu.Modules.NarrativeProjection`
- `src/Zongzu.Application`
- `src/Zongzu.Presentation.Unity`
- `tests/Zongzu.Modules.NarrativeProjection.Tests`
- `tests/Zongzu.Presentation.Unity.Tests`
- `tests/Zongzu.Integration.Tests`
- `docs/ACCEPTANCE_TESTS.md`
- `docs/UI_AND_PRESENTATION.md`
- `docs/MODULE_INTEGRATION_RULES.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- changed projection text only

## Determinism risk
- low
- no RNG draws are added
- command prompts are selected from deterministic read-model affordance ordering
- notice guidance is derived from deterministic `WorldDiff` / `DomainEvent` traces

## Milestones
1. Add this ExecPlan and lock the projection-only boundary.
2. Carry death pressure summaries into traceable death diffs.
3. Enhance family death notice `WhatNext` for adult-successor versus severe succession-gap cases.
4. Align great hall / family council / notification center prompts to the same lifecycle affordance priority.
5. Update acceptance and projection docs.
6. Run targeted build and tests.

## Verification
- `dotnet build .\src\Zongzu.Application\Zongzu.Application.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Modules.NarrativeProjection.Tests\Zongzu.Modules.NarrativeProjection.Tests.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -c Debug`
- targeted integration lifecycle guidance tests
- scoped `git diff --check`

## Verification result
- application build passed with 0 warnings and 0 errors
- narrative projection tests passed: 10/10
- presentation Unity tests passed: 20/20
- FamilyCore tests passed: 14/14
- targeted lifecycle integration tests passed: 3/3
- scoped `git diff --check` passed; only existing line-ending normalization warnings were reported
