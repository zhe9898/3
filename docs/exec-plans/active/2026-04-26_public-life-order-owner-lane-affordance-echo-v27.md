# Public-Life Order Closure v27: Owner-Lane Follow-Up Affordance Echo

## Goal
- Carry v26 `余味冷却提示` / `余味续接提示` / `余味换招提示` into the existing owner-lane command affordance surfaces as a projection echo.
- Make the player see the next usable owner-lane surface without treating the text as routing, validity, or command outcome authority.
- This remains a rule-driven command / aftermath / SocialMemory / readback loop. It is not an event pool, not a new command system, not a hidden recommendation ledger, and not a thick yamen / runner / family formula pass.

## Scope In
- Projection wording such as `现有入口读法`, `建议冷却：先不加压`, `可轻续：仍走本 lane`, `建议换招：别回压本户`, and `等待承接口：本户不能硬补`.
- Apply the echo to existing Order / Office / Family affordances that already carry owner-lane guidance.
- Tests that the echo appears on at least Order plus Office or Family affordances from structured owner outcome + SocialMemory residue.

## Scope Out
- No new command names, command routing, command queue, follow-up ledger, owner-lane ledger, cooldown ledger, or target field.
- No enable/disable decisions based on the echo.
- No Application command-result calculation; no UI or Unity inference.
- No parsing of receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, or SocialMemory summary prose.

## Save / Schema Impact
- Target impact: none.
- v27 adds runtime projection/readback text only. If new persisted state appears necessary, stop and create a schema/migration plan instead.

## Milestones
1. Reuse the v26 structured residue helper output.
2. Add owner-lane affordance echo text without changing command availability.
3. Extend integration, architecture, and Unity copy-only tests.
4. Update docs and acceptance criteria with no schema impact.

## Evidence Checklist
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture / Unity presentation tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] docs updated
- [x] schema/migration impact explicitly documented as none
