# EXTENSIBILITY_MODEL

This document defines how the project grows without becoming strongly coupled.

## Goal
Support additive growth through:
- new modules
- richer feature packs
- deeper projections
- more content

without:
- rewriting the kernel
- inflating core entity types
- bypassing module ownership

## Extensibility strategy
Use **feature packs** inside one modular monolith.

A feature pack:
- enables one or more modules
- registers commands, queries, events, schemas, migrations, tests, and projections
- does not bypass scheduler or persistence rules

## Feature pack examples

### MVP packs
- `CoreWorldPack`
- `LineagePack`
- `PopulationPack`
- `SocialMemoryPack`
- `ExamLitePack`
- `TradeLitePack`
- `SpatialShellPack`
- optional `LocalConflictLitePack`

Note: `PersonRegistry` is Kernel-layer infrastructure, always present, and does not require a feature pack. See `PERSON_OWNERSHIP_RULES.md`.

### Post-MVP packs
- `OfficePack`
- `OrderBanditryPack`
- `ForcePack`
- `WarfareCampaignPack`
- `RegionalExpansionPack`

## Module registration contract
A module should register:
- module key
- module schema version
- required feature flag/pack
- scheduler phase participation
- supported commands
- emitted event types
- projection builders
- migration hooks

## When to add a new module instead of extending an old one
Create a new module if the feature:
- owns a clearly separate state family
- has independent lifecycle or pacing
- would otherwise bloat `Person`, `Clan`, or `Settlement` core state
- introduces new commands/events that are not just detail of an existing module
- could be disabled in some release lines

Extend an existing module if the feature:
- is a detail of the same owned state family
- uses the same commands/events and save namespace
- would be impossible to understand as separate ownership

## Feature manifest
Each save root stores an enabled-feature manifest.
Example:
- `FamilyCore = true`
- `EducationAndExams = lite`
- `TradeAndIndustry = lite`
- `OrderAndBanditry = false`
- `WarfareCampaign = false`

This enables:
- content gating
- migration decisions
- test profile generation

## Pack compatibility rules
A pack must declare:
- required upstream packs
- incompatible packs, if any
- migration policy if added to an old save
- acceptance tests it adds

## Anti-patterns
Do not:
- dynamically load arbitrary runtime assemblies as feature packs
- let packs mutate scheduler order ad hoc
- let packs write into foreign module save blobs
- create “temporary pack hacks” that bypass boundaries

## Extensibility success test
A feature is well-extended only if:
- it has a clear module owner
- it can be enabled/disabled in configuration or release planning
- it uses only queries, commands, and events across boundaries
- it preserves save compatibility discipline
