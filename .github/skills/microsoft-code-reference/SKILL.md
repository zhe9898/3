---
name: microsoft-code-reference
description: Use when working on Zongzu's C# / .NET 10 codebase, Unity-facing presentation adapters, shared ViewModel DTOs, MessagePack persistence, or NUnit tests. Covers API lookups, implementation patterns, error repair, dependency rules, determinism, state ownership, save/schema versioning, scheduler cadence, and testing conventions for this modular-monolith simulation project.
---

# Microsoft Code Reference (Zongzu Local)

## Overview

This skill provides C# / .NET 10 coding guidance aligned to the Zongzu repo's actual architecture, engineering rules, and module boundaries.
It is a local complement to the remote `microsoft-code-reference` MCP skill.
Use it for quick lookup of project-specific patterns without re-reading the full docs.

## Use This Skill When

- writing or reviewing C# code in any Zongzu project
- fixing build errors, test failures, or determinism bugs
- adding new modules, commands, events, queries, projections, or migrations
- checking dependency direction, state ownership, scheduler behavior, or integration rules
- verifying save/schema compatibility
- touching Unity-facing adapters or shared ViewModel DTOs

## Fast Lane

For small C# fixes, use this skill as a quick guard: check dependency direction, deterministic iteration, schema impact, and the smallest relevant test. Use a full pass only when the change touches scheduler cadence, command resolution, save/migration, cross-module events, projection builders, or Unity-facing adapters.

## Locked Stack

| Layer | Technology |
|---|---|
| Engine host | Unity LTS |
| Platform | Windows desktop |
| Render pipeline | URP |
| Shipping backend | IL2CPP |
| Authoritative simulation | Pure C# class libraries (`net10.0`, C# 14) |
| Presentation adapters | `Zongzu.Presentation.Unity` |
| Shared Unity DTOs | `Zongzu.Presentation.Unity.ViewModels` targets `net10.0` but locks `LangVersion` to `9.0` for Unity sync |
| Save serialization | MessagePack |
| Config / debug serialization | JSON |
| Testing | NUnit for pure libraries; Unity Test Framework for Unity presentation |
| Nullable reference types | Enabled |
| Implicit usings | Disabled in core libraries |
| Warnings as errors | Enabled for kernel, contracts, scheduler, persistence, and simulation modules |

## Dependency Rules

```text
Kernel -> nothing game-specific
Contracts -> Kernel
Scheduler -> Kernel + Contracts
Modules -> Kernel + Contracts
Application -> Kernel + Contracts + Scheduler + module facades
Persistence -> Kernel + Contracts + module schema contracts
Presentation.Unity.ViewModels -> Contracts only; Unity-compatible DTOs
Presentation.Unity -> Contracts + ViewModels; adapters/projection text only
Host -> Application + presentation adapters for preview/diagnostics
Unity Shell -> presentation host, not authority layer
```

Presentation and Unity code may format, adapt, and display projections. They may not contain authoritative game rules or mutate module state.

## File Size Rules

- Authority / application / persistence / projection files: target under roughly 400 logical lines.
- Non-generated files over roughly 600 logical lines: split by responsibility unless there is a documented reason.
- Do not mix domain rules, command routing, persistence/migration, projection/shell wording, diagnostics/debug, and external IO in one file.

## Determinism Rules

- All randomness goes through `DeterministicRandom`; never use `System.Random`, `Random.Shared`, Unity random, or wall-clock time in authority paths.
- Keep authoritative iteration order stable; prefer sorted `List<T>` patterns over unordered collection side effects.
- Avoid floating-point drift in authoritative economy and pressure rules; use integer or fixed-point style values.
- No file, network, process, or Unity API calls inside xun, month, seasonal, event-handling, or diff-generation paths.
- Choose collections by access pattern and cardinality: prefer simple arrays/lists for stable ordered passes, dictionaries for keyed lookup, queues for FIFO drains, and avoid unordered iteration when authority ordering matters.

## State Ownership

- Every module owns its own state namespace.
- Only the owning module may mutate that namespace.
- Other modules read through public queries, projections, or structured event receipts only.
- No direct cross-module state mutation.

## Integration Channels

| Channel | Use |
|---|---|
| **Query** | Read-only projection access |
| **Command** | Player or application intent routed to the owning module |
| **DomainEvent** | Deterministic event emission; handlers update their own state only |

Forbidden: module A mutating module B state, UI writing authority state, or narrative/templates causing gameplay effects.

## Application Layer

- Application services validate and route intent; they must not become a second rule engine.
- Command handling must not directly mutate foreign module state.
- If a command needs domain consequence logic, move it into the owning module.
- When a feature claims an end-to-end pressure chain, test the real scheduler path rather than manually feeding downstream events only.

## Typed IDs

```csharp
public readonly record struct PersonId(int Value);
public readonly record struct HouseholdId(int Value);
public readonly record struct ClanId(int Value);
public readonly record struct SettlementId(int Value);
public readonly record struct InstitutionId(int Value);
public readonly record struct MemoryId(int Value);
public readonly record struct RelationshipEdgeId(int Value);
public readonly record struct ForceGroupId(int Value);
public readonly record struct CampaignId(int Value);
public readonly record struct NotificationId(int Value);
```

IDs are unique within save scope, never recycled, and serialized as primitive integers.

## Core Types

```csharp
public readonly record struct GameDate(int Year, int Month);

public sealed record ModuleStateEnvelope<TState>
{
    public string ModuleKey { get; init; }
    public int ModuleSchemaVersion { get; init; }
    public TState State { get; init; }
}
```

## Save Structure

```json
{
  "rootSchemaVersion": 1,
  "featureManifest": { "FamilyCore": "full", "EducationAndExams": "lite" },
  "kernelState": { },
  "modules": {
    "ModuleName": { "moduleSchemaVersion": 1, "state": { } }
  }
}
```

## Current Module Schema Versions

Verify against module `ModuleSchemaVersion` before changing migrations.

| Module | Schema Version |
|---|---:|
| PersonRegistry | 1 |
| WorldSettlements | 8 |
| FamilyCore | 8 |
| PopulationAndHouseholds | 3 |
| SocialMemoryAndRelations | 3 |
| EducationAndExams | 2 |
| TradeAndIndustry | 4 |
| PublicLifeAndRumor | 4 |
| OfficeAndCareer | 7 |
| NarrativeProjection | 1 |
| OrderAndBanditry | 9 |
| ConflictAndForce | 4 |
| WarfareCampaign | 4 |

## Current Zongzu Chain Anchors

- The implemented thin-chain skeleton is closed through v108 as topology/readback/ownership evidence, not full-chain rule-density completion.
- The current mainline is through v476: v109-v204 closes the first Chain 8 court-policy rule-density branch; v213-v292 implements and closes the first social mobility / fidelity-ring substrate and readback layer; v253-v268 closes Chain 9 regime-legitimacy first readback; v293-v380 closes personnel-flow readiness/gate/future-lane preflight without opening direct movement lanes; v381-v452 closes commoner/social-position readback, owner-source keys, scale-budget readback, regional guard, and fidelity scale-budget preflight; v453-v460 adds household mobility dynamics explanation over existing signals; v461-v468 closes that layer as docs/tests governance only; v469-v476 adds household mobility owner-lane preflight.
- v35-v476 use existing owner state, runtime projection fields, or docs/tests preflight evidence for Trade/Order, Family, Office, PublicLife, SocialMemory, Force/Campaign, Warfare, Court-policy, social mobility, regime readback, personnel-flow readiness/gate/future-lane preflight, commoner/social-position readback/source keys, fidelity scale-budget evidence, and household mobility dynamics explanation/closeout/preflight.
- v101-v108, v197-v204, v245-v252, v261-v268, v285-v292, v293-v300, v317-v324, v349-v356, v373-v380, v405-v412, v429-v436, v437-v444, v445-v452, v461-v468, and v469-v476 are documentation plus architecture-test closeout/preflight evidence. v213-v244 uses existing `PopulationAndHouseholds` and `PersonRegistry.FidelityRing` state only; v277-v284, v389-v404, v413-v428, and v453-v460 add runtime read-model/ViewModel fields only; v301-v316, v325-v348, and v357-v372 add runtime projection/ViewModel readback only. These passes add no schema bump, migration, ledger, manager/controller layer, UI/Unity authority, social-class engine, mobility engine, route-history model, or `PersonRegistry` domain expansion.
- For performance work, name hot path, cardinality, complexity, allocation risk, deterministic ordering/cap, cache owner/invalidation, save/schema impact, and validation lane before changing algorithms. Current projection builders may build one-pass dictionaries over already-built snapshots; scheduler hot paths and Unity frame loops must not rely on repeated global scans.
- For diagnostics, prefer structured, source-generated or cached logging patterns on hot paths; use counters or focused benchmarks before claiming allocation, GC, CPU, exception-rate, or scheduler-throughput improvement. Choose `Dictionary`, `HashSet`, `Queue`, `List`, or sorted collections by access pattern rather than habit.

## Migration Rules

- Root schema migrations are explicit and rare.
- Module schema migrations are explicit and local.
- Adding a module requires default-state migration behavior for old saves.
- Removing or disabling a module requires a documented retention/cleanup policy.
- Migration preparation must not mutate the source save root.
- Migrations run through `SaveMigrationPipeline` with chained steps.

## Scheduler Cadence

| Band | When | What |
|---|---|---|
| **Xun** | 3 per month (`上旬` / `中旬` / `下旬`) | World's internal breathing: food, labor, petty trade, rumor, illness, debt |
| **Month** | 1 per month | Player review shell: consolidated diffs, family review, policy choices |
| **Season / year** | Slower | Harvest, major exams, tax climate, war fatigue, legitimacy shifts |

Monthly flow: prepare -> 3 xun pulses -> month-end authority pass -> bounded event drain -> projection -> player review and commands.
Same-month follow-on events are allowed only through `MonthlyScheduler`'s bounded deterministic drain; do not hand-roll recursive dispatch.

## Module Registration Pattern

A module should register:
- module key and schema version
- feature flag / pack mode
- scheduler phase and cadence participation
- accepted commands
- published and consumed event names
- query registrations
- projection builders or read model outputs
- migration hooks

## Testing Conventions

| Test Type | Purpose |
|---|---|
| Deterministic replay | Same seed + same input = same output |
| Save roundtrip | Save -> load -> compare |
| Invariant tests | No impossible family, household, or authority states |
| Feature manifest compatibility | Enabled/disabled modules handled correctly |
| Module boundary tests | No cross-module state leakage |
| Scheduler cadence tests | Prepare + 3 xun + month-end + bounded event drain without ownership violation |
| Headless diagnostics | Long-run health, bounded notification growth, save payload size |
| ViewModel roundtrip | Unity-facing DTOs serialize cleanly and stay rule-free |

## Common Pitfalls

1. Float determinism: use integer/fixed-point style values for authoritative economy.
2. Iteration order: keep authoritative loops stable.
3. Wrong RNG: always use `DeterministicRandom`.
4. Cross-module write: event handlers update only their owning state.
5. UI authority leak: presentation must not contain game rule logic.
6. Fat files: split when mixing domain rules, routing, persistence, and projection.
7. Blocking IO: no file/network/process IO in authority hot paths.
8. Summary parsing: never parse `DomainEvent.Summary` as rule input; use metadata, entity keys, or queryable state.

## Code Style

- Prefer value objects over primitive obsession.
- Make side effects explicit.
- Do not bury game rules in UI, config loaders, text adapters, or debug views.
- Authoritative state stores facts, pressures, and references; shell prose belongs downstream.
- Avoid stringly typed glue and ad-hoc key concatenation when a typed ID, enum, or metadata key exists.
- Do not grow god classes or vague utility sinks.
