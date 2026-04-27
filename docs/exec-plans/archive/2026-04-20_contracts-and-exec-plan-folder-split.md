## Goal
- split oversized code folders into clearer responsibility buckets without changing behavior
- keep module boundaries, namespaces, and verification flow aligned while improving scanability

## Scope in
- reorganize `src/Zongzu.Contracts` into concern-based subfolders with namespaces unchanged
- reorganize `src/Zongzu.Presentation.Unity` view models and adapters into surface-based subfolders with namespaces unchanged
- add lightweight structure notes for `Zongzu.Contracts`
- add lightweight structure notes for `Zongzu.Presentation.Unity`
- run solution verification after the path moves

## Scope out
- no authoritative logic changes
- no contract shape or namespace rewrites
- no schema/save version changes
- no relocation of the untracked root `My project/` Unity workspace in this pass
- no module-boundary redesign

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`

## Save/schema impact
- none

## Determinism risk
- none
- path and documentation organization only

## Milestones
1. add plan and folder-structure notes for `Zongzu.Contracts`
2. move contract files into concern-based subfolders
3. move presentation view models and adapters into surface-based subfolders
4. run build/test verification and record results

## Tests to add/update
- no new focused tests required
- keep full solution build and tests green after path moves

## Rollback / fallback plan
- if any path move causes compile, tooling, or navigation regressions, move files back while preserving their content
