# TECH_STACK

## Locked stack for the current production line

The code, analyzer, module, system, Unity presentation, performance, and content-standard interpretation for this stack is defined in `MODERN_GAME_ENGINEERING_STANDARDS.md`.

### Engine / platform
- Engine host: **Unity LTS**
- Primary platform: **Windows desktop**
- Render pipeline: **URP**
- Shipping backend: **IL2CPP**

### Language and projects
- Authoritative simulation: **pure C# class libraries**
- Presentation: **Unity C#**
- Nullable reference types: enabled
- Implicit usings: disabled in core libraries
- Warnings as errors: enabled for kernel, contracts, scheduler, persistence, and simulation modules

### Serialization
- authored configs: JSON
- authoritative saves: MessagePack or equivalent compact deterministic format
- debug snapshots: JSON
- no runtime binary serializer shortcuts

### Testing
- NUnit for pure libraries
- Unity Test Framework for presentation/integration tests only
- headless replay runner for determinism and multi-year simulation tests

## Repository layout

```text
/
  AGENTS.md
  CHANGELOG.md
  PLANS.md
  /docs
  /src
    /Zongzu.Kernel
    /Zongzu.Contracts
    /Zongzu.Scheduler
    /Zongzu.Application
    /Zongzu.Persistence
    /Zongzu.Modules.WorldSettlements
    /Zongzu.Modules.FamilyCore
    /Zongzu.Modules.PopulationAndHouseholds
    /Zongzu.Modules.SocialMemoryAndRelations
    /Zongzu.Modules.EducationAndExams
    /Zongzu.Modules.TradeAndIndustry
    /Zongzu.Modules.OfficeAndCareer
    /Zongzu.Modules.OrderAndBanditry
    /Zongzu.Modules.ConflictAndForce
    /Zongzu.Modules.WarfareCampaign
    /Zongzu.Modules.NarrativeProjection
    /Zongzu.Presentation.Unity
  /unity
    /Zongzu.UnityShell
  /tests
    /Zongzu.Kernel.Tests
    /Zongzu.Persistence.Tests
    /Zongzu.Scheduler.Tests
    /Zongzu.Modules.WorldSettlements.Tests
    /Zongzu.Modules.FamilyCore.Tests
    /Zongzu.Modules.PopulationAndHouseholds.Tests
    /Zongzu.Modules.SocialMemoryAndRelations.Tests
    /Zongzu.Modules.EducationAndExams.Tests
    /Zongzu.Modules.TradeAndIndustry.Tests
    /Zongzu.Modules.OfficeAndCareer.Tests
    /Zongzu.Modules.OrderAndBanditry.Tests
    /Zongzu.Modules.ConflictAndForce.Tests
    /Zongzu.Modules.WarfareCampaign.Tests
    /Zongzu.Modules.NarrativeProjection.Tests
    /Zongzu.Integration.Tests
  /content
    /authoring
    /generated
  /tools
```

## Architectural stance
This is a **modular monolith**:
- one repository
- one coordinated save model
- one scheduler
- many internal modules with hard boundaries

It is **not**:
- one giant object graph with arbitrary writes
- runtime plugin hot-loading
- a microservice-style split
- a premature DLC/mod marketplace architecture

## Dependency rules
- kernel depends on nothing game-specific
- contracts depend on kernel
- scheduler depends on kernel + contracts
- modules depend on kernel + contracts + scheduler interfaces
- application depends on kernel + contracts + scheduler + module facades
- persistence depends on kernel + contracts + module schema contracts
- presentation depends on application + projections, never directly on module internals
- the Unity host shell lives under `/unity/Zongzu.UnityShell`; it is a presentation host and asset/scene workspace, not an authority layer

## Out-of-scope technologies for MVP
Do not introduce without explicit architecture approval:
- ECS/DOTS rewrite
- multiplayer
- runtime scripting language
- behavior-tree framework for all AI
- reflection-heavy dependency injection in core
- node-graph event systems as authority
