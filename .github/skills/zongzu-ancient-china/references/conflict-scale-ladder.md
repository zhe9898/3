# Conflict Scale Ladder

Use this reference when the task is about combat, battle, local clashes, force types, skirmish resolution, warfare layering, or when the user pushes back against making everything a campaign board.

## Core Rule

Not every conflict in Zongzu should escalate into a campaign board.

Conflict should resolve at the lowest scale that still preserves:
- social meaning
- administrative consequence
- route and settlement relevance
- readable aftermath

## Preferred Scale Ladder

### 1. Social-Pressure Layer

Use when conflict is still mostly threat, fear, or visible preparation.

Examples:
- feud threat
- escort hesitation
- yamen intimidation
- branch standoff
- road harassment
- militia mustering rumor

Best surfaces:
- desk sandbox
- lineage surface
- notices
- route pressure
- hotspot overlays

### 2. Local Conflict Vignette

Use when force appears openly but the clash is still local, bounded, and socially embedded.

Examples:
- family-on-family clash
- escort ambush
- market riot suppression
- yamen arrest gone wrong
- bandit raid on one route
- hall guard versus petition crowd

Best surfaces:
- conflict result vignette
- node incident panel
- short encounter summary
- local fallout notices

This is usually the right layer for Zongzu's first battle-like presentation.

### 3. Tactical-Lite Encounter Resolution

Use when the clash needs some force distinction, posture, terrain, and morale, but should still avoid unit micro.

Examples:
- river crossing fight
- escort column engagement
- pass defense
- punitive sweep
- market-town siege scare
- fortified estate assault

Keep it about:
- force family
- posture
- terrain edge
- cohesion
- rout risk
- civilian exposure

Do not turn it into per-soldier or per-squad micromanagement.

### 4. Campaign Board

Use only when the conflict spans multiple settlements, routes, or administrative regions.

Examples:
- county-spanning suppression
- multi-route rebel pressure
- grain-line war
- front stabilization
- garrison redeployment
- dynastic or regional campaign

This is where mobilization timing, depots, ferries, grain, command fit, and aftermath pressure dominate.

### 5. Aftermath Return

Every conflict should fall back into the living world.

Push results back into:
- households
- lineage standing
- office pressure
- order and banditry
- trade and route confidence
- public opinion
- legitimacy

## Force Families For This Repo

Prefer broad families over huge unit catalogs:
- `household_retainer`
- `lineage_guard`
- `escort_band`
- `militia_or_tuanlian`
- `yamen_force`
- `regular_detachment`
- `bandit_or_rebel_band`
- `garrison_force`

These are better than long weapon rosters for MVP because they connect to society and module ownership.

## Good Zongzu Abstractions

- `engagement_scale`
- `force_family`
- `force_posture`
- `terrain_edge`
- `cohesion_state`
- `rout_risk`
- `civilian_exposure`
- `aftermath_spread`

## Design Guidance

- Most violence in this repo should read as socially situated conflict, not isolated sport combat.
- The same family or office problem may appear first as pressure, then vignette, then encounter, and only later as campaign pressure.
- Local clashes should usually show who was affected, what route or node was stressed, and what changed afterward.
- Tactical-lite is useful, but it should stay subordinate to the larger living-world loop.
- Campaign boards are powerful, but they should not swallow family conflict, office violence, escort work, feud clashes, or county incidents that deserve a closer surface.

## Suggested Module Mapping

- `ConflictAndForce` for local conflict and tactical-lite encounter ownership
- `WarfareCampaign` for multi-settlement campaign pressure
- `WorldSettlements` for node and route stress
- `NarrativeProjection` for conflict-result wording and aftermath explanation
- `Presentation.Unity` for conflict vignettes, node incident panels, and campaign surfaces
