# MVP Closeout: Family Lifecycle Slice

## Goal
Close a visible MVP gap by turning `FamilyCore` from a lineage-conflict-only module into a thin but playable lineage-lifecycle module.

This slice should make the MVP feel more like:
- arranging a marriage tie before branch pressure hardens
- worrying about heir security and continuity
- seeing births, deaths, and mourning alter household and lineage pressure
- handling these pressures through bounded clan-head commands rather than omnipotent edits

## Scope in
- extend `FamilyCore` with thin lifecycle state for:
  - marriage alliance pressure
  - heir security
  - reproductive pressure
  - mourning load
- add bounded family commands for:
  - arranging marriage
  - designating heir policy
- let `FamilyCore` monthly simulation produce:
  - marriage-state resolution
  - births in thin form
  - deaths in thin form
  - heir destabilization and mourning pressure
- expose lifecycle state through:
  - family queries
  - read models
  - family council / hall presentation
  - narrative projection
- add migration from `FamilyCore` schema `2 -> 3`
- tighten MVP docs and acceptance to emphasize this slice as core MVP closure work

## Scope out
- no full kinship graph
- no full spouse/parent lineage editor
- no widow/remarriage subsystem beyond pressure wording
- no new module
- no office or warfare expansion
- no UI authority logic
- no attempt to finish all post-MVP family/social depth in one pass

## Affected modules
- `Zongzu.Modules.FamilyCore`
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Presentation.Unity`
- `Zongzu.Persistence`

## Save/schema impact
- `FamilyCore` schema bumps from `2` to `3`
- new `FamilyCore` fields must be documented in:
  - `docs/DATA_SCHEMA.md`
  - `docs/SCHEMA_NAMESPACE_RULES.md`
- default migration pipeline must include `FamilyCore 2 -> 3`
- migration should conservatively backfill lifecycle fields from existing clan/heir/person data

## Determinism risk
- births and deaths add kernel-random usage to `FamilyCore`
- risk is acceptable if:
  - iteration order stays stable
  - all random draws are deterministic
  - same seed + same commands preserve replay
- tests must explicitly cover repeat-run determinism after lifecycle events

## Milestones
1. Add the ExecPlan and codify MVP closeout scope.
2. Extend `FamilyCore` state and queries with lifecycle pressures and outcomes.
3. Implement monthly lifecycle simulation and bounded commands.
4. Project lifecycle state into read models, shell, and narrative.
5. Add migration, tests, and docs.
6. Verify focused tests, then full solution build/test.

## Tests to add/update
- `Zongzu.Modules.FamilyCore.Tests`
  - marriage command changes lifecycle pressure deterministically
  - monthly run can produce a birth when conditions are met
  - monthly run can produce a death/mourning/heir instability when conditions are met
- `Zongzu.Integration.Tests`
  - default M2 bootstrap surfaces lifecycle pressures without post-MVP modules
  - family command affordances/receipts include lifecycle commands on MVP paths
- `Zongzu.Persistence.Tests`
  - `FamilyCore` schema `2 -> 3` migration
  - save roundtrip preserves lifecycle state
- `Zongzu.Presentation.Unity.Tests`
  - family council / hall remain read-only while surfacing lifecycle state
- update acceptance docs for MVP closeout expectations

## Rollback / fallback plan
- if births/deaths introduce too much instability, keep the new state and commands but reduce monthly births/deaths to pressure-only outcomes for this pass
- if lifecycle presentation becomes noisy, keep state/query changes but collapse shell wording to one hall summary plus one family-council summary

## Current closeout note
- lifecycle events now require dedicated ancestral-hall-facing titles / guidance for `议亲定婚`, `门内添丁`, `门内举哀`, and `承祧未稳`
- great hall and family-council summaries should both carry lifecycle pressure wording so MVP family play is legible without opening post-MVP paths
- player-facing lifecycle receipts should now read as concrete家内处置 such as聘财、人情、入谱、护持, not only as thin generic state summaries
- lifecycle follow-up should now also cover post-birth and post-death handling through bounded family commands such as `拨粮护婴` and `议定丧次`, still without opening a separate household-authority subsystem
- lifecycle projection should now also tell the player what to look at next after birth, mourning, and heir-instability events, especially when traces point to襁褓照护, 口粮支应, 丧次祭次, or承祧名分
- hall and family-council lifecycle summaries should now also surface a read-only `眼下宜先...` style prompt derived from enabled lifecycle affordances, so MVP family play points toward the next bounded command without moving authority into UI

- the top lifecycle notice on the great hall should now reuse that same projected directional prompt in both lead-notice guidance and notification-center follow-up text, so hall, council, and notice tray no longer drift apart on what the player ought to handle next

## Open questions
- should MVP default wording lean on one marriage-alliance summary per clan or one per adult person
- how much infant loss should appear in MVP versus remain future depth
- whether heir designation should remain abstract or immediately bind to a concrete `PersonId`
