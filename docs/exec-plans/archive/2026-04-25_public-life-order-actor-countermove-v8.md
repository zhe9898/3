# Public-Life Order Closure v8: Actor Countermove & Passive Back-Pressure

## Status

Complete - 2026-04-25

## Framing

This plan extends the v6/v7 rule-driven command / residue / social-memory / response loop. It is not an event-pool, not an event-centered design, and should not be described as an event-chain. `DomainEvent` remains a deterministic fact propagation tool after rules resolve.

v7 made response residue persist, soften, harden, and feed later owner-module command friction. v8 adds a bounded autonomous layer:

Month N response aftermath -> Month N+2 SocialMemory-owned response residue -> later monthly residue drift -> owner-module actor countermove / passive back-pressure -> projected readback -> later SocialMemory residue if the countermove leaves a trace.

## Goal

Let local actors react to visible public-life order residue even when the player does not immediately issue another command.

The slice focuses on `添雇巡丁` and `严缉路匪` after账:

- 巡丁 / 脚户 may quietly patch a route guarantee, or misread the pressure and harden the road tail.
- 县门 / 胥吏 may quietly land a delayed document, or keep dragging the docket.
- 族老 / 本户担保 may explain enough to cool shame, or avoid the public guarantee and leave it sour.

The player still has bounded response commands. Autonomous actor movement is small, deterministic, and read-model visible; it is not a substitute for player leverage.

## Scope

In scope:

- Owner-module monthly countermoves in `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore`.
- Countermoves read structured `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, `SourceClanId`, and `OriginDate`.
- Countermoves skip response memories created in the current month.
- Countermoves write only their owner module's existing pressure and response trace fields.
- Projection/readback shows the countermove through existing response receipts and governance / public-life / family surfaces.

Out of scope:

- New persisted fields.
- New managers, controllers, actor schedulers, or a general AI planner.
- UI, Application, or Unity rule computation.
- Parsing `DomainEvent.Summary`, memory summaries, receipt summaries, or `LastRefusalResponseSummary`.
- PersonRegistry expansion.

## Ownership

- `OrderAndBanditry` owns road-watch / runner / route-pressure countermoves and mutates only `SettlementDisorderState`.
- `OfficeAndCareer` owns county-yamen / clerk / document countermoves and mutates only `OfficeCareerState`, then rebuilds its own jurisdiction projection.
- `FamilyCore` owns elder explanation / household guarantee countermoves and mutates only `ClanStateData`.
- `SocialMemoryAndRelations` owns durable residue. It does not resolve commands or actor countermoves; it may later read owner response traces and create SocialMemory-owned residue.
- Application builds read models only. Unity shell copies projected fields only.

## Save And Schema Impact

Expected impact: no schema bump.

v8 reuses existing persisted fields introduced by v6:

- `LastRefusalResponseCommandCode`
- `LastRefusalResponseCommandLabel`
- `LastRefusalResponseSummary`
- `LastRefusalResponseOutcomeCode`
- `LastRefusalResponseTraceCode`
- `ResponseCarryoverMonths`

It may add new trace-code constants, but no new module state fields. If implementation requires new persisted state, this plan must be revised before code lands with schema bump, migration, roundtrip and legacy migration tests, plus updates to `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md`.

## Milestones

1. Add trace constants for actor countermove response traces.
2. Add structured SocialMemory response-residue readers for monthly owner-module countermoves.
3. Apply bounded owner-module countermoves:
   - Order: local watch self-settles, or runner/route misread hardens.
   - Office: yamen quietly lands document, or clerk delay continues.
   - Family: elders explain quietly, or avoid guarantee and leave shame.
4. Keep projection-only readback through existing response receipt surfaces.
5. Add tests:
   - repaired/contained residue can produce a soft countermove without player command
   - escalated/ignored residue can produce a hard countermove without player command
   - countermove mutates only owning module at monthly resolution
   - same-month response command still does not write SocialMemory
   - architecture tests guard summary parsing and forbidden boundary drift

## Acceptance Proof Targets

- v7 response residue can trigger a later bounded owner-module countermove without a player command.
- Countermoves are deterministic and skip current-month response memories.
- Order / Office / Family mutate only their own module state.
- SocialMemory is not mutated by owner countermove command-time or module-time logic; it only reads owner traces on its later monthly pass.
- Read models expose the countermove via existing response readback.
- No schema bump or migration is introduced.
- `dotnet build Zongzu.sln --no-restore`
- focused tests
- `dotnet test Zongzu.sln --no-build`

## Implementation Evidence

- Added owner trace constants in `src/Zongzu.Contracts/PublicLifeOrderResponseTypes.cs` for order, office, and family actor-countermove outcomes.
- Added `OrderAndBanditry` monthly actor countermove rules for route-watch self-settlement and runner/route hardening in `src/Zongzu.Modules.OrderAndBanditry/OrderAndBanditryModule/OrderAndBanditryModule.PublicLifeActorCountermove.cs`.
- Added `OfficeAndCareer` monthly actor countermove rules for quiet yamen document landing and continued clerk delay in `src/Zongzu.Modules.OfficeAndCareer/OfficeAndCareerModule/OfficeAndCareerModule.PublicLifeActorCountermove.cs`.
- Added `FamilyCore` monthly actor countermove rules for quiet elder explanation and guarantee avoidance in `src/Zongzu.Modules.FamilyCore/FamilyCoreModule.PublicLifeActorCountermove.cs`.
- Added focused integration proof in `tests/Zongzu.Integration.Tests/PublicLifeOrderActorCountermoveRuleDrivenTests.cs` for a repaired/contained soft path and an escalated hard path.
- Added architecture guard `Public_life_actor_countermoves_must_read_structured_social_memory_only` in `tests/Zongzu.Architecture.Tests/ProjectReferenceTests.cs`.
- Updated schema, boundary, simulation, relationship, UI, acceptance, skill-matrix, and alignment-audit docs to record that v8 reuses existing SocialMemory schema `3` plus v6 owner response trace fields and adds no migration.

## Validation Log

- `git diff --check` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter PublicLifeOrderActorCountermoveRuleDrivenTests` passed: 2 tests.
- Focused architecture guard passed: 5 tests.
- `dotnet test tests/Zongzu.Modules.FamilyCore.Tests/Zongzu.Modules.FamilyCore.Tests.csproj --no-build` passed: 22 tests.
- `dotnet test tests/Zongzu.Modules.OfficeAndCareer.Tests/Zongzu.Modules.OfficeAndCareer.Tests.csproj --no-build` passed: 41 tests.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build` passed: 27 tests.
- `dotnet test Zongzu.sln --no-build` passed.

## Save / Migration Result

No new persisted state was added in v8. `OrderAndBanditry` remains schema `9`, `OfficeAndCareer` remains schema `7`, `FamilyCore` remains schema `8`, and `SocialMemoryAndRelations` remains schema `3`. The actor countermove layer reuses existing v6 response trace fields and existing SocialMemory records, so no new migration or save roundtrip case is required beyond the existing v6/v7 persistence coverage and full-solution test pass.
