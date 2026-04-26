---
name: zongzu-pressure-chain
description: "Use when a task explicitly involves Zongzu cross-module pressure chains, especially Renzong thin/full chains, broad-to-local pressure propagation, court/frontier/disaster/office/order/trade/public-life causality, event topology, projection receipts, player response surfaces, or chain-completion tests. Do not use for isolated module refactors with no pressure propagation."
---

# Zongzu Pressure Chain

## Overview

Use this skill to move a Zongzu pressure chain from idea to topology, implementation, projection, and validation without turning it into an event pool or detached subsystem.

Pressure chains are the connective tissue of the game: a source pressure enters the world, modules translate it through owned state, events record facts, projections explain the result, and the player sees bounded response options.

Use it only when there is an actual chain. If inspection shows a local helper, internal refactor, or single-module behavior with no cross-module pressure movement, hand off to the narrower skill or ordinary code workflow.

## Current Repo Anchors

Use live code facts before accepting docs as current:
- `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` is the current thin-chain ledger; `RENZONG_PRESSURE_CHAIN_SPEC.md` remains the fuller target
- `MonthlyScheduler` performs bounded fresh-event drain before projection, so same-month chains must prove handler-emitted follow-ons appear through that path
- event contracts must align with module `PublishedEvents` and `ConsumedEvents`; emitted-but-unconsumed events need classification as projection-only receipts, dormant/future contracts, or bugs
- current v32-v34 event-contract health work is evidence, not a pressure chain by itself: classification, no-unclassified gate, and owner/evidence backlinks keep the graph honest without creating runtime authority
- notification scope helpers are read-side only; they may normalize existing trace traversal but must not author, rank, or drive chain consequences
- persistent high bands require edge detection, watermarks, recovery, recurring-demand cadence, or a documented debt item rather than repeated fresh escalation
- performance/complexity proof is part of chain proof: every broad-to-local chain should name fanout, affected locus, no-touch boundary, scheduler drain cap, and whether the path is edge-triggered or recurring demand
- current public-life/order closure is intentionally rule-driven rather than event-pool driven: commands mutate only their owning module's structured aftermath, later modules read through query seams when in scope, SocialMemory writes durable residue inside its own state, and projections/Unity copy the resulting readback
- v3-v11 leverage/cost/readback/response fields are projection or owner-module traces; v12 adds `PopulationAndHouseholds` schema `3` home-household local response state; v13-v18 reuse existing schema seams for SocialMemory echo, repeat friction, capacity, tradeoff, and short-term receipt readback
- current v19-v108 adds projection-only follow-up affordance hints, owner-lane return/status/outcome/residue/no-loop readback, Office/Family/Force/Warfare/Court readbacks, directive-choice and aftermath-docket readbacks, and thin-chain closeout audit language. Keep the proof framed as projected owner-state readback with architecture, integration, Unity/presentation, and no-summary-parsing evidence
- current v35-v100 thin-chain examples graduate one owner lane at a time: canal-window pressure returns to Trade/Order, household burden and relief return to Family, Office/yamen后手 remains in Office, force/campaign后账 remains in Force/Campaign, Warfare directives/aftermath remain in `WarfareCampaign`, and court-policy process remains Office/PublicLife readback. Each uses structured metadata/query snapshots, existing owner state or projection-only readback, off-scope/no-touch tests, and no schema/ledger/UI authority expansion
- current v101-v108 closeout is an audit, not a new pressure chain: it records the thin skeleton and preserves full-chain rule-density debt

## External Calibration Anchors

Use outside material to harden proof quality:
- .NET unit-test guidance maps to small pressure-chain tests: one source, one locus, one owner mutation or readback, and one no-touch assertion before broad acceptance proof.
- .NET diagnostics and `dotnet-counters` are useful when chain fanout, scheduler drain, allocation rate, exception rate, or long-run health is the real risk; they are not needed for projection-only wording changes.
- Performance guidance should translate into cardinality and complexity notes: affected settlements, households, offices, routes, events, notices, and projection rows touched per month.
- Unity and accessibility guidance apply after the read model exists: shell surfaces may visualize chain readback, but may not calculate pressure propagation or owner-lane status.

## Workflow

1. Pick the chain mode.

   Use thin-chain mode when the task is proving topology, metadata, a projection receipt, or one bounded response.
   Use full-chain mode only when formulas, recovery/decay, long-run behavior, player consequences, and balancing are explicitly in scope.

2. Read the chain context narrowly.

   Start with:
   - `docs/RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
   - `docs/MODULE_INTEGRATION_RULES.md`
   - `docs/MODULE_BOUNDARIES.md`
   - the active ExecPlan for the chain

   Add `docs/RENZONG_PRESSURE_CHAIN_SPEC.md`, `docs/HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`, `docs/SIMULATION.md`, `docs/PLAYER_SCOPE.md`, or shell docs only when the task actually touches those layers.

   Pair with `zongzu-game-design` for product shape and `zongzu-architecture-boundaries` for code ownership.

3. Define the chain ledger before editing.

   Every chain needs:
   - source pressure and historical carrier
   - affected settlements, clans, households, offices, routes, public venues, or force posture
   - owning module for each state mutation
   - upstream query inputs and off-scope boundaries
   - command path if the player can respond
   - domain events and whether they are edge facts, recurring demand, receipts, or projection-only notices
   - fanout/cardinality budget and whether propagation is one-to-one, one-to-many within a locus, or deliberately global
   - projection/read-model surface and shell destination
   - tests that prove both propagation and no-touch entities

4. Keep thin-chain and full-chain claims separate.

   A thin chain may prove:
   - module path
   - event metadata
   - scheduler order
   - one visible projection
   - one bounded response or no-response proof

   A full chain also needs:
   - historically grounded formulas or thresholds
   - recovery and decay behavior
   - long-run pressure-band behavior
   - player response consequences and backlash
   - diagnostics for saturation or dead ends

5. Inspect current code facts.

   Check:
   - module `PublishedEvents`, `ConsumedEvents`, `RegisterQueries`, `RunXun`, `RunMonth`, `HandleEvents`, and `HandleCommand`
   - query contracts in `Zongzu.Contracts`
   - feature-pack bootstrap path in `SimulationBootstrapper`
   - projection/read-model builder and shell adapters
   - tests under affected module, integration, architecture, and presentation projects
   - long-run diagnostics such as `TenYearSimulationHealthCheckTests` when saturation or dormant-event risk is in scope

6. Validate chain completion.

   Use `zongzu-simulation-validation` for deterministic proof.
   A chain is not complete until:
   - real scheduler execution covers the path
   - structured metadata identifies affected entities without parsing summaries
   - off-scope entities stay untouched
   - projection or receipt behavior is explicit
   - event observability debt is classified
   - fanout and performance risk are bounded by locus, cadence, cap, or test fixture size
   - docs and ExecPlan reflect thin vs full status

## Output Rules

- Do not start from a narrative event list.
- Do not use `DomainEvent.Summary` as rule input.
- Do not let broad pressure hit every settlement without an allocation rule.
- Do not add all-to-all chain propagation unless the model explicitly requires it and tests prove both touched and no-touch entities.
- Do not call a chain complete when only one module emits an event.
- Do not turn court, frontier, disaster, or regime pressure into a fixed scripted timeline.
- Do not describe the public-life/order refusal/response/owner-lane readback loop as an event-chain unless the implementation actually moves through emitted command-time events; the current path is owner-command aftermath -> query/readback seam -> SocialMemory, household-owned residue/projection, or owner-lane return projection.
- Do not call event-contract diagnostic classification itself a pressure chain; v32-v34 are graph evidence, v35-v100 are actual thin handoff/readback passes only where owner modules consume/mutate their own state or projections expose existing owner-state aftermath, and v101-v108 is a closeout audit rather than runtime propagation.
- Prefer source pressure -> owned state -> structured event -> read model -> bounded response.
- Prefer edge events, watermarks, and recurring-demand state over repeating the same high-band shock every month.
- Prefer explicit no-touch tests for unaffected settlements/modules.
- Mark future contracts honestly instead of pretending they are consumed today.
- Do not require full-chain evidence for a task that only claims thin-chain topology.
- Do not hide algorithmic complexity behind "pressure spreads"; name the cap, watermark, deterministic ordering, affected locus, and off-scope boundary.
