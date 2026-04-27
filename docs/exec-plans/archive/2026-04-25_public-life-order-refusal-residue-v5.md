# Public-Life Order Closure v5: Rule-Driven Refusal Trace And SocialMemory Refusal Residue

## Goal
Close the refusal / partial-landing side of the public-life order loop without turning the slice into an event-chain or event-pool design.

This pass is a rule-driven command / aftermath / social-memory readback loop:

`Month N public-life/order command -> OrderAndBanditry-owned accepted / partial / refused resolution -> structured outcome/refusal/partial trace -> Month N+1 SocialMemoryAndRelations-owned residue -> family / public-life / governance readback -> shell visibility -> save / migration / acceptance proof`

DomainEvents remain fact propagation tools when needed. They are not the design body of this work, and `DomainEvent.Summary` is never rule input.

## Priority Cut
Primary verbs:
- `FundLocalWatch` / `添雇巡丁`
- `SuppressBanditry` / `严缉路匪`

Priority refusal / partial carriers:
- county-yamen drag or non-landing
- local watch / foot-runner misreading or evasion
- crackdown backlash from coerced ground actors
- public guarantee failure that leaves shame, fear, favor, grudge, or obligation residue

## Module Ownership
- `OrderAndBanditry` owns command-time authority resolution, structured outcome code, refusal code, partial code, trace code, carryover, and order pressure mutation.
- `SocialMemoryAndRelations` owns all durable residue in `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- `Application` and read models may join projected order aftermath and social memory readback, but may not calculate refusal causes or write residue.
- `Zongzu.Presentation.Unity` and Unity shell code may copy projected fields only. They must not query modules directly, infer missing residue, or repair state.

## Save And Schema Impact
This pass adds persisted Order-owned trace fields to `SettlementDisorderState`, so `OrderAndBanditry` schema moves from `7` to `8`.

Planned fields:
- `LastInterventionOutcomeCode`
- `LastInterventionRefusalCode`
- `LastInterventionPartialCode`
- `LastInterventionTraceCode`
- `RefusalCarryoverMonths`

Migration `7 -> 8` must:
- backfill null strings to empty strings
- infer legacy non-empty intervention receipts as accepted outcome trace
- clamp intervention and refusal carryover to one month
- preserve module key and avoid adding any new module envelope

`SocialMemoryAndRelations` schema remains `3`; v5 adds new memory kind constants and cause keys only, then writes existing memory/narrative/climate records.

## Implementation Milestones
1. Add Order outcome/refusal/partial constants, persisted fields, query snapshot fields, schema version bump, migration, and save tests.
2. Deepen `OrderAndBanditryModule.HandlePublicLifeCommand` so known commands can resolve accepted, partial, or refused inside Order rules. Application may pass office reach modifiers only.
3. Extend SocialMemory public-life order residue profiles to read structured Order outcome/refusal/partial fields, not summaries, and write only SocialMemory state.
4. Extend read-model summaries so public-life receipts and governance lanes/dockets can show refused or partial landing, local drag, and social-memory refusal residue.
5. Extend presentation tests proving shell receipt/readback fields are copied from projections only.
6. Update schema, boundary, integration, simulation, UI, and acceptance docs with v5 evidence and run validation.

## Test Targets
- `OrderAndBanditry` accepted / partial / refused command paths mutate only Order-owned state at command time.
- Same-month command does not mutate SocialMemory.
- Month N refused or partial `添雇巡丁` / `严缉路匪` produces Month N+1 durable SocialMemory residue.
- SocialMemory consumes structured query aftermath, not `DomainEvent.Summary` or receipt prose.
- Read models expose refusal / partial residue through `PresentationReadModelBundle.SocialMemories`.
- Governance, public-life, and family-facing readback include refusal residue.
- Unity shell displays projected refusal readback only.
- Save/load preserves Order refusal trace and SocialMemory residue.
- Legacy Order schema `7 -> 8` migration backfills the new structured trace fields.
- Architecture tests guard summary parsing, Application/UI/Unity social-memory writes, forbidden manager/god-controller names, and `PersonRegistry` expansion.

## Determinism Notes
- Outcome/refusal/partial classification is a pure function of Order state plus query-derived office-reach modifiers.
- SocialMemory owner-clan selection remains deterministic: prestige, support reserve, clan name, id.
- No random draw, narrative text parsing, or command-time event drain is required for the residue rule.
- Scheduler order remains intentional: SocialMemory execution order `400` reads prior-month Order trace before Order execution order `700` clears the carryover in the same monthly pass.

## Evidence Log
- Implemented `OrderAndBanditry` schema `8` structured public-life order trace fields: `LastInterventionOutcomeCode`, `LastInterventionRefusalCode`, `LastInterventionPartialCode`, `LastInterventionTraceCode`, and `RefusalCarryoverMonths`.
- Added same-namespace `OrderAndBanditry` migration `7 -> 8`, backfilling legacy non-empty intervention receipts as accepted follow-through and clamping carryover windows.
- Extended `添雇巡丁` / `严缉路匪` command resolution so Order owns accepted / partial / refused classification, while Application passes only query-derived office-reach modifiers.
- Extended `SocialMemoryAndRelations` public-life order residue to consume structured outcome/refusal/partial fields through `IOrderAndBanditryQueries`, never receipt summary text, and write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Extended read models so public-life receipts and governance lanes/dockets can expose projected `县门未落地`, `地方拖延`, `后账仍在`, and `社会记忆读回` text.
- Extended Unity presentation tests to prove the shell copies projected refusal readback text only.
- Updated schema, namespace, boundary, integration, simulation, UI, and acceptance docs for v5.

Validation:
- `dotnet build Zongzu.sln --no-restore` passed.
- Focused Order tests passed: `dotnet test tests/Zongzu.Modules.OrderAndBanditry.Tests/Zongzu.Modules.OrderAndBanditry.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeCommand"`.
- Focused SocialMemory tests passed: `dotnet test tests/Zongzu.Modules.SocialMemoryAndRelations.Tests/Zongzu.Modules.SocialMemoryAndRelations.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrder"`.
- Focused integration tests passed: `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderSocialMemoryResidueRuleDrivenTests"`.
- Focused persistence tests passed: `dotnet test tests/Zongzu.Persistence.Tests/Zongzu.Persistence.Tests.csproj --no-build --filter "FullyQualifiedName~OrderAndBanditry|FullyQualifiedName~PublicLifeOrder|FullyQualifiedName~OrderRefusal"`.
- Focused architecture tests passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Social_memory|FullyQualifiedName~forbidden_manager"`.
- Focused Unity presentation test passed: `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~SocialMemoryOrderReadback"`.
- Full suite passed: `dotnet test Zongzu.sln --no-build`.
