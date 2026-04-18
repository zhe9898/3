# IMPLEMENTATION_PHASES

This plan assumes a single-developer, Codex-assisted workflow.

## Phase M0 - kernel and modular spine
Goal:
- repo skeleton
- kernel/contracts/scheduler/persistence shell
- module registration
- save root + feature manifest
- replay hash skeleton

Done when:
- empty world can advance 12 months deterministically
- save/load roundtrip works on empty/minimal world
- module registration tests pass

## Phase M1 - lineage and population substrate
Modules:
- WorldSettlements
- FamilyCore
- PopulationAndHouseholds
- SocialMemoryAndRelations

Done when:
- births/deaths/households/branches work
- commoner pressure exists
- relationship/memory/grudge basics work
- 10-year interactive loop playable
- 20-year headless run stable

## Phase M2 - lite pathways
Modules:
- EducationAndExams.Lite
- TradeAndIndustry.Lite
- NarrativeProjection
- Presentation shell first pass

Done when:
- exam and trade outcomes feed the monthly loop
- explanations render correctly
- spatial shell usable
- MVP core question answerable

## Phase M3 - optional local conflict lite
Modules:
- OrderAndBanditry.Lite
- ConflictAndForce.Lite

Implementation note:
- M3 may land incrementally so long as each slice stays additive and preserves M0-M2 stability
- `OrderAndBanditry.Lite` may ship before `ConflictAndForce.Lite`
- order pressure should integrate with trade and projection before richer force resolution is added
- the order-enabled M3 bridge path may remain available while the full local-conflict lite path enables both `OrderAndBanditry` and `ConflictAndForce`
- active M3 local-conflict work must not alter the active M2 bootstrap surface

Done when:
- local security pressure exists
- local conflict can happen and be explained
- no tactical micro introduced
- schedule still intact

## Phase P1 - governance and disorder
Modules:
- OfficeAndCareer
- OrderAndBanditry full

## Phase P2 - force depth
Modules:
- ConflictAndForce full

## Phase P3 - campaign sandbox
Modules:
- WarfareCampaign
- desk sandbox war overlay
- campaign aftermath projections

Implementation note:
- the first active `WarfareCampaign.Lite` slice may land before deeper war rules so long as it stays campaign-level, read-only in presentation, and downstream of `ConflictAndForce` / `OfficeAndCareer`
- current board-depth work already adds bounded front labels, command-fit wording, commander summaries, route descriptors, and thin application-routed campaign intent descriptors plus a built-in schema `1 -> 2 -> 3` migration, without introducing tactical micro

## Phase P4 - regional breadth and polish
- more regions
- more presentation states
- analytics and debug expansions

## Scope discipline
If behind schedule:
1. cut post-MVP packs first
2. cut optional M3 next
3. preserve M0-M2 foundations at all costs
