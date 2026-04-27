Result: in progress on 2026-04-19. Reframe player-facing shell, projection, surfaced office-state wording, and authoritative event/diff summaries away from modern dashboard / workflow jargon while keeping system/debug wording modern and clear.

## Goal
Make player-facing surfaces read less like a modern management panel and more like a Chinese-ancient hall / yamen / desk-sandbox / campaign-board simulation, without forcing faux-ancient wording onto system, debug, migration, or engineering-only surfaces.

## Scope in / out
### In
- Reword player-facing strings in:
  - `Zongzu.Modules.NarrativeProjection`
  - `Zongzu.Presentation.Unity`
- Reword player-facing authoritative summaries at source in:
  - module `scope.RecordDiff(...)`
  - module `scope.Emit(...)`
  - seeded player-facing descriptor fixtures used by tests/bootstrap paths
- Ancientize office task / petition / pressure wording wherever it leaks into read models, notifications, shell surfaces, or tests.
- Update tests that assert player-facing wording.
- Update UI/presentation and acceptance docs to state that player-facing shell language should avoid modern dashboard jargon.

### Out
- No authority-rule changes.
- No new commands, events, or module boundaries.
- No tactical warfare changes.
- No broad internal renaming of engineering-only types, files, or debug structures.
- No faux-ancient rewrite of debug, migration, schema, or developer-facing wording.

## Affected modules
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Modules.FamilyCore`
- `Zongzu.Modules.WorldSettlements`
- `Zongzu.Modules.PopulationAndHouseholds`
- `Zongzu.Modules.SocialMemoryAndRelations`
- `Zongzu.Modules.EducationAndExams`
- `Zongzu.Modules.TradeAndIndustry`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Presentation.Unity`
- tests:
  - `Zongzu.Modules.NarrativeProjection.Tests`
  - `Zongzu.Presentation.Unity.Tests`
  - `Zongzu.Integration.Tests`
- docs:
  - `UI_AND_PRESENTATION.md`
  - `ACCEPTANCE_TESTS.md`

## Save/schema impact
- No root schema bump.
- No module schema bump.
- No planned root or module schema impact.
- If existing surfaced state strings are too modern, prefer rewriting them over preserving temporary development wording.
- During active development wording churn, legacy stress-save checks may temporarily validate structural migration continuity rather than exact replay-hash parity when only surfaced player-facing state has been intentionally rewritten.

## Determinism risk
- Low.
- Risks:
  - brittle tests coupled to previous wording
  - accidental leakage of modern placeholder text through untranslated shell fields
  - player-visible English surviving in authoritative event/diff sources and bypassing shell-only normalization
- Controls:
  - keep changes limited to player-facing text paths
  - add/update assertions for translated shell/narrative outputs
  - prefer fixing player-visible wording at authoritative/projection source rather than relying only on read-only shell replacements

## Milestones
1. Replace modern player-facing notification titles and guidance text in `NarrativeProjection`.
2. Add display-layer ancientization for office and aftermath wording in the shell.
3. Rewrite player-facing module diff/event summaries so notices no longer depend on shell-only English cleanup.
4. Update tests to assert in-theme wording.
5. Update docs and verify build/tests.

## Progress update
- `NarrativeProjection` player-facing titles and guidance have been ancientized in UTF-8 Chinese.
- `OfficeAndCareer` petition outcome descriptors no longer preserve English category output on player-facing surfaces.
- Active warfare shell mappings now normalize lingering English fixture phrases like `Registrar`, `docket traffic`, `campaign board`, and escort-route prose before they reach the hall / desk / campaign-board read-only surfaces.
- Authoritative player-facing event/diff summaries are being moved to final in-world Chinese at source across family, settlement, household, order, conflict, trade, exam, office, and social-memory modules so player notices do not rely on shell-only cleanup.
- Presentation tests are being tightened to fail on those modern phrases rather than merely accepting any non-empty output.
- The wording rule is now explicitly two-lane: player-facing in-world surfaces ancientize; system/debug/migration surfaces remain modern and engineering-clear.

## Tests to add/update
- `NarrativeProjectionModuleTests`
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`

## Rollback / fallback plan
- If full wording replacement is too noisy, prioritize anything that reaches hall, desk, office, warfare, or notice-facing surfaces before purely engineering-only identifiers.
