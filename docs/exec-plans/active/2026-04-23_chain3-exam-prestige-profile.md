# Chain 3 Exam Prestige Profile

## Goal

Thicken chain 3 by replacing the current flat exam-pass prestige bump with a credential-prestige profile owned by `FamilyCore`.

Current thin chain:

`EducationAndExams.ExamPassed -> FamilyCore.ClanPrestigeAdjusted`

This pass keeps the topology unchanged. `EducationAndExams` records the credential fact, while `FamilyCore` decides how that credential changes clan prestige and marriage value from event metadata plus family-owned state.

## Scope In / Out

In:
- keep `ExamPassed` as the upstream event
- add structured credential metadata to `ExamPassed`
- compute clan prestige and marriage-value deltas from exam tier, score, academy prestige, stress, current clan standing, kinship role, adult unmarried status, and marriage pressure
- emit `ClanPrestigeAdjusted` with structured cause/source/person/delta/profile metadata
- add focused handler tests for stronger credentials and off-scope clan protection
- update chain topology docs and acceptance notes

Out:
- no new event name
- no schema version bump
- no `ExamAttemptResolved` payload migration yet
- no `OfficeAndCareer` waiting list or appointment queue
- no `SocialMemoryAndRelations` favor/shame record
- no `PublicLifeAndRumor` exam-board or literati-public projection
- no failed-exam / study-abandoned downstream branch

## Affected Modules

- `EducationAndExams`: owns study progress, exam score, credential tier, and emits `ExamPassed`
- `FamilyCore`: owns clan prestige, marriage alliance value, and emits `ClanPrestigeAdjusted`
- `OfficeAndCareer`, `SocialMemoryAndRelations`, and `PublicLifeAndRumor`: remain full-chain TODOs for this pass

## Save/Schema Impact

None. The rule uses existing state fields and event metadata only.

## Determinism Risk

Low. `EducationAndExams` already computes the deterministic exam score during the monthly pass; this pass only carries that score downstream. `FamilyCore` uses no new random calls.

## Milestones

1. [done] Add structured credential metadata to `ExamPassed`.
2. [done] Replace flat `+5/+3` clan adjustment with a credential-prestige profile.
3. [done] Add structured metadata to `ClanPrestigeAdjusted` for exam-pass cause.
4. [done] Add focused handler tests for profile strength and off-scope protection.
5. [done] Update docs.
6. [done] Run integration and full test suites.

## Tests To Add/Update

- `EducationAndExamsModuleTests`:
  - `ExamPassed` carries credential metadata without parsing narrative text
- `ExamResultHandlerTests`:
  - stronger credential profile produces a stronger prestige/marriage delta
  - off-scope clans remain unchanged
  - `ClanPrestigeAdjusted` carries exam-pass profile metadata
- `ExamPrestigeChainTests`:
  - real scheduler drains `ExamPassed` into cause-tagged `ClanPrestigeAdjusted`

## Rollback / Fallback Plan

Revert `ApplyExamPassPrestige` to the flat `+5/+3` rule and remove metadata assertions. No schema migration is involved.

## Open Questions

- Later full chain should introduce `ExamAttemptResolved` once downstream modules need a typed pass/fail/abandon payload.
- Later full chain should decide how exam success enters `OfficeAndCareer` waiting lists, `SocialMemoryAndRelations` sponsor favor/shame, and `PublicLifeAndRumor` exam-board / teahouse discussion.
