# Skill Pack External Calibration and Fast Lane

## Purpose

Tighten the Zongzu skill pack after the v196 court-policy branch so future long tasks can use external calibration without slowing every small edit into a full whole-skill pass.

This is skill/documentation governance only. It does not change runtime code, module state, scheduler behavior, projection behavior, Unity scenes, content assets, save schema, or migrations.

## Scope

- Add fast-lane guidance to the repo-tracked Zongzu skills and `microsoft-code-reference`.
- Add missing `Use This Skill When` sections where the skill body did not mirror the trigger metadata.
- Sync useful local reference material into the repo-tracked skill pack:
  - Northern Song source calibration.
  - Similar-game system calibration.
  - Similar-game map/UI calibration.
  - Expanded game-design, map/sandbox, sovereignty, and shell references.
- Update the skill rationalization matrix with external calibration sources for .NET testing, diagnostics, high-performance logging, collection choice, Unity profiling, Unity managed memory, object pooling, and Unity UI optimization.

## External Calibration

External sources calibrate implementation discipline, not product authority:

- Microsoft Learn unit testing best practices: fast, isolated, repeatable, self-checking, behavior-focused tests.
- Microsoft Learn `dotnet-counters`: first-level CPU, GC, allocation, memory, and exception-rate evidence before deeper performance investigation.
- Microsoft Learn high-performance logging: source-generated or cached-delegate logging for hot diagnostic paths; logs never become player receipts or rule input.
- Microsoft Learn collection selection: data structures should follow access pattern, ordering, memory, and concurrency needs.
- Unity Profiler: Unity shell performance claims require actual profiling evidence when frame behavior changes.
- Unity managed memory: avoid per-frame allocation churn and unmanaged shell object lifetimes that create GC/frame spikes.
- Unity object pooling and UI optimization: pool or virtualize repeated shell rows/markers when churn is expected or measured; never pool authoritative simulation state.

## Boundary Rules

- No new skill turns external sources into authority over Zongzu product truths.
- No skill suggests a full Court engine, event pool, world manager, UI rule layer, or Unity-owned simulation.
- No new persisted state, schema bump, migration, ledger, feature-pack membership change, or `PersonRegistry` expansion.
- Fast-lane mode is preferred for small docs, copy, projection, adapter, and validation checks.
- Whole-skill mode remains available for broad, ambiguous, cross-module, runtime, save/schema, cadence, or future-system work.

## Validation Plan

- Check skill structure and missing sections with local scripts.
- Check UTF-8 / replacement-character markers.
- Run `git diff --check`.
- No runtime/build/test validation is required because this pass changes only skill governance and documentation.

## Validation Results

- All 10 repo-tracked `SKILL.md` files now contain both `## Use This Skill When` and `## Fast Lane`.
- UTF-8 / replacement-character marker scan found `0` suspect markers in all 10 `SKILL.md` files.
- Repo-tracked skill files were mirrored to `C:\Users\Xy172\.codex\skills`; local/repo hashes match for every repo skill file.
- `git diff --check` passed.
- No runtime/build/test validation was run because no `src/`, `tests/`, `unity/`, save/schema, or content-runtime artifact changed.
