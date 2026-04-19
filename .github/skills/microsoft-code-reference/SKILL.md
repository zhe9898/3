---
name: microsoft-code-reference
description: Use when working on Zongzu's C# / .NET 8 codebase. Covers API lookups, implementation patterns, code samples, error repair, dependency rules, determinism, state ownership, save/schema, scheduler cadence, and testing conventions specific to this modular-monolith simulation project.
---

# Microsoft Code Reference (Zongzu Local)

## Overview

This skill provides C# / .NET 8 coding guidance aligned to the Zongzu repo's actual architecture, engineering rules, and module boundaries.
It is a local complement to the remote `microsoft-code-reference` MCP skill.
Use it for quick lookup of project-specific patterns without re-reading the full docs.

## Use This Skill When

- writing or reviewing C# code in any Zongzu project
- fixing build errors, test failures, or determinism bugs
- adding new modules, commands, events, queries, or migrations
- checking dependency direction, state ownership, or integration rules
- verifying save/schema compatibility

## Locked Stack

| Layer | Technology |
|---|---|
| Engine host | Unity LTS |
| Platform | Windows desktop |
| Render pipeline | URP |
| Shipping backend | IL2CPP |
| Authoritative simulation | Pure C# class libraries (.NET 8) |
| Presentation | Unity C# |
| Save serialization | MessagePack |
| Config / debug serialization | JSON |
| Testing | NUnit (pure libs), Unity Test Framework (presentation) |
| Nullable reference types | Enabled |
| Implicit usings | Disabled in core libraries |
| Warnings as errors | Enabled for kernel, contracts, scheduler, persistence, simulation modules |

## Dependency Rules

```
Kernel → nothing game-specific
Contracts → Kernel
Scheduler → Kernel + Contracts
Modules → Kernel + Contracts + Scheduler interfaces
Application → Kernel + Contracts + Scheduler + Module facades
Persistence → Kernel + Contracts + Module schema contracts
Presentation → Application + Projections (never module internals)
Unity Shell → presentation host, not authority layer
```

## File Size Rules

- Authority / application / persistence / projection files: target < ~400 logical lines
- Non-generated files > ~600 logical lines: split is the default expectation
- Do not mix in one file: domain rules, command routing, persistence/migration, projection/shell wording, diagnostics/debug, external IO

## Determinism Rules

- All randomness through `DeterministicRandom` — never `System.Random` or `Random.Shared`
- Stable iteration order — use `List<T>`, avoid `HashSet<T>` / `Dictionary<K,V>` for authoritative iteration
- No wall-clock authority
- No unordered collection side effects
- Authoritative economy: integer or fixed-point, avoid float drift

## State Ownership

- Every module owns its own state namespace
- Only the owning module may mutate that namespace
- Other modules read through public projections / queries only
- No direct cross-module state mutation

## Integration Channels

| Channel | Use |
|---|---|
| **Query** | Read-only projection access |
| **Command** | Player or application intent routed to owning module |
| **DomainEvent** | Deterministic event emission; handlers update own state only |

**Forbidden**: module A mutating module B state, UI writing authority state, narrative templates causing gameplay effects.

## Application Layer

- Application services validate and route intent — they must not become a second rule engine
- Command handling must not directly mutate foreign module state
- If a command needs domain consequence logic, move it into the owning module

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

IDs are unique within save scope, never recycled, serialized as primitive integers.

## Core Types

```csharp
public readonly record struct GameDate(int Year, int Month);

public sealed record ModuleStateEnvelope<TState> {
    string ModuleKey;
    int ModuleSchemaVersion;
    TState State;
}
```

## Save Structure

```json
{
  "rootSchemaVersion": 1,
  "featureManifest": { "FamilyCore": "full", "EducationAndExams": "lite", ... },
  "kernelState": { },
  "modules": {
    "ModuleName": { "moduleSchemaVersion": N, "state": { } }
  }
}
```

## Current Module Schema Versions

| Module | Schema Version |
|---|---|
| WorldSettlements | 2 |
| FamilyCore | 3 |
| PopulationAndHouseholds | 1 |
| SocialMemoryAndRelations | 1 |
| EducationAndExams | 1 |
| TradeAndIndustry | 3 |
| PublicLifeAndRumor | 4 |
| OfficeAndCareer | 3 |
| NarrativeProjection | 1 |
| OrderAndBanditry | 6 |
| ConflictAndForce | 3 |
| WarfareCampaign | 3 |

## Migration Rules

- Root schema migrations: explicit and rare
- Module schema migrations: explicit and local
- Adding a module: provide default-state migration for old saves
- Removing/disabling: document retention/cleanup policy
- Migration preparation must not mutate source save root
- Migrations run through `SaveMigrationPipeline` with chained steps

## Scheduler Cadence

| Band | When | What |
|---|---|---|
| **旬** (xun) | 3× per month (上旬/中旬/下旬) | World's internal breathing: food, labor, petty trade, rumor, illness, debt |
| **月** (month) | 1× per month | Player review shell: consolidated diffs, family review, policy choices |
| **季/年** (seasonal) | Slower | Harvest, major exams, tax climate, war fatigue, legitimacy shifts |

Monthly flow: prepare → 3 xun pulses → month-end consolidation → event handling → diff aggregation → narrative projection → player commands.

## Module Registration Pattern

A module should register:
- module key + schema version
- required feature flag/pack
- scheduler phase participation (xun / month / seasonal)
- supported commands
- emitted event types
- projection builders
- migration hooks

## Testing Conventions

| Test Type | Purpose |
|---|---|
| Deterministic replay | Same seed + same input = same output |
| Save roundtrip | Save → load → compare |
| Invariant tests | No impossible family states, household membership holds |
| Feature manifest compatibility | Enabled/disabled modules handled correctly |
| Module boundary tests | No cross-module state leakage |
| Scheduler cadence tests | 3 xun + month-end without ownership violation |
| Headless diagnostics | 120-month run, bounded notification growth, save payload size |

## Common Pitfalls

1. **Float determinism** — use integer/fixed-point for authoritative economy
2. **Iteration order** — `List<T>` for authoritative loops, not `HashSet`/`Dictionary`
3. **Wrong RNG** — always `DeterministicRandom`, never `System.Random`
4. **Cross-module write** — check that event handlers only update own state
5. **UI authority leak** — presentation must not contain game rule logic
6. **Fat files** — split when mixing domain rules + routing + persistence + projection
7. **Blocking IO** — no file/network IO in xun/month/seasonal/event-handling/diff paths

## Code Style

- Prefer value objects over primitive obsession
- Side effects must be explicit
- Do not bury game rules in UI or config loaders
- Authoritative state stores facts/pressures/references; shell prose belongs downstream
- Avoid stringly-typed glue and ad-hoc key concatenation
- Do not grow god classes or vague utility sinks
