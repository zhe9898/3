# Player Seat, Day Cadence, And Gameplay Loop Docs

Date: 2026-04-24

## Goal

Integrate the current design discussion into the existing documentation set:

- The gameplay perspective is the home-household seat, not a single-person RPG identity and not clan-god control.
- The player continuously manages the household's livelihood, population, name, debt, kinship, and position; people are the entry points that feel and execute pressure, not the player's body.
- The target bottom authority atom is `day`; `xun` / early-mid-late month remains useful as almanac wording, UI grouping, projection summary, or a loose schedule-window label.
- The docs already support a complete living world structurally, but playable closure requires a concrete loop: visible pressure -> readable leverage -> bounded command -> module-owned resolution -> receipt/refusal/residue -> changed next-month read.

## Scope in

- Update product, gameplay, player-scope, cadence, shell, acceptance, and documentation-map language.
- Preserve the world-first, monthly-review, bounded-command doctrine.
- Mark this as a documentation/spec alignment pass, not a runtime scheduler migration.

## Scope out

- No C# runtime changes.
- No scheduler implementation changes.
- No `GameDate`, save schema, migration, or feature-manifest changes.
- No new command system implementation.
- No commit or push unless explicitly requested.

## Affected modules

None at runtime.

Touched docs:

- `AGENTS.md`
- `docs/README.md`
- `docs/PRODUCT_SCOPE.md`
- `docs/FULL_SYSTEM_SPEC.md`
- `docs/CODEX_MASTER_SPEC.md`
- `docs/RULES_DRIVEN_LIVING_WORLD.md`
- `docs/PLAYER_SCOPE.md`
- `docs/MVP.md`
- `docs/MVP_SCOPE.md`
- `docs/POST_MVP_SCOPE.md`
- `docs/VERSION_ALIGNMENT.md`
- `docs/IMPLEMENTATION_PHASES.md`
- `docs/ARCHITECTURE.md`
- `docs/ENGINEERING_RULES.md`
- `docs/STATIC_BACKEND_FIRST.md`
- `docs/MODERN_GAME_ENGINEERING_STANDARDS.md`
- `docs/SIMULATION.md`
- `docs/MODULE_CADENCE_MATRIX.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/LIVING_WORLD_DESIGN.md`
- `docs/DATA_SCHEMA.md`
- `docs/UI_AND_PRESENTATION.md`
- `docs/VISUAL_FORM_AND_INTERACTION.md`
- `docs/GAME_DEVELOPMENT_ROADMAP.md`
- `docs/DOCUMENTATION_MAP.md`
- `docs/ACCEPTANCE_TESTS.md`

## Query / Command / DomainEvent impact

None. This task names the gameplay-loop contract but does not add runtime commands, query contracts, or events.

## Save/schema impact

None in this pass. `DATA_SCHEMA.md` now states that persisted root time remains year/month in the current codebase and that persisted exact day support would require a future schema/version/migration task.

## Determinism impact

None for this docs-only change. A future code migration from the current short-band scheduler shape to day-level authority needs scheduler, replay-hash, and save/load tests before it is accepted.

## Unity/presentation boundary impact

Docs only. The shell contract is clarified: player-facing surfaces should present a household-seat decision room, person dossiers as emotional/contextual entry points, and day-level motion as summarized pressure unless urgent.

## Milestones

- [x] Read relevant Zongzu skills.
- [x] Inspect current docs for player/cadence/play-loop wording.
- [x] Update docs with unified doctrine.
- [x] Run `git diff --check`.
- [ ] Report changed files and follow-up implementation branch.

## Tests to run

- `git diff --check`
- Changed-docs mention sanity check if new links are added.

No dotnet test is required for this docs-only pass.

## Fallback notes

If the day-cadence migration proves too large for the current runtime, keep existing short-band scheduler hooks as a transitional implementation layer while docs mark the target doctrine and future implementation path.
