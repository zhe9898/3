# IMPLEMENTATION_PHASES

This plan assumes a single-developer, Codex-assisted workflow.
For the phase-by-phase index, cross-document map, and ultra-fine implementation route, see `GAME_DEVELOPMENT_ROADMAP.md`.

## Phase M0 - kernel and modular spine
Goal:
- repo skeleton
- kernel/contracts/scheduler/persistence shell
- cadence-aware scheduler foundation for `day / month / seasonal`, with `xun` kept to calendar/projection grouping unless a transitional implementation hook still uses the old name
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

## Phase M2 - influence and player agency
Modules:
- EducationAndExams.Lite
- TradeAndIndustry.Lite
- NarrativeProjection
- bounded command layer (family / trade / public)

Done when:
- exam and trade outcomes feed the monthly loop
- bounded commands are validated, resolved, and receipted
- influence footprint is readable in the shell
- player can issue at least 6 distinct bounded commands

Implementation note:
- M2 is about **player agency**, not shell polish
- the shell may still be view-model composition rather than final Unity scenes
- M2 proves the loop: world pulse -> diff -> projection -> bounded command -> receipt -> next-month state

## Phase M3 - MVP shell
Modules:
- Presentation shell integration
- Great hall / ancestral hall / desk sandbox / macro sandbox
- Notice tray, visitor system, seal confirmation

Done when:
- spatial shell is playable in Unity (not just backend tests)
- at least one complete pressure chain can be walked end-to-end in the shell
- the shell feels like "sitting in the hall, hearing the world arrive"
- A good Zongzu slice proves that structured diffs, projections, and bounded commands already make the shell feel alive

### M3 shell surface checklist (must all be present)
| Surface | Minimum viable state | Object anchors present |
|---------|----------------------|------------------------|
| Great hall / study | Monthly notice tray + visitor slot + current date | Almanac, visitor cushion, notice tray, seal box |
| Ancestral hall / lineage | Family tree readable + heir marker + memorial pile | Ancestral tablets, branch ledgers, heir marker, memorial pile |
| Desk sandbox | 3+ settlement nodes + 2+ routes visible + focus cluster | Settlement discs, route threads, focus cluster, notice pin |
| Macro sandbox | County-entry seal + route strips + seasonal band | Route strips, county seal, calendar strip, spillover markers |
| Conflict vignette | If conflict pack on: injury/raid marker + aftermath scroll | Aftermath scroll, casualty tally |

### M3 pressure-chain proof gate
M3 is not "done" until at least one complete pressure chain can be walked end-to-end **in the Unity shell**:
1. World pulse creates pressure (e.g., drought → grain spike)
2. Diff detection surfaces the change
3. Projection builds readable notice (e.g., `粮道吃紧`)
4. Player reviews and issues bounded command (e.g., `EscortRoute`)
5. Command resolves deterministically
6. Receipt surfaces in next-month projection (e.g., `一路平安` or `折损报告`)
7. Next month carries the consequence into new world state

### M3 design review criteria
Before M3 is declared complete, verify:
- [ ] No "dashboard" vocabulary remains in player-facing surfaces
- [ ] Every surface follows foreground / action / background lane grammar
- [ ] Object anchors are physical objects, not floating text panels
- [ ] Player commands are bounded (no god-buttons)
- [ ] Explanations render as cause chains, not event cards
- [ ] Chinese text uses historically grounded terms (yamen, memorial, route, grain-line) not modern product jargon
- [ ] CJK font fallback is acceptable (English placeholder or system font); full TMP CJK asset is post-MVP

Implementation note:
- the explicit default MVP bootstrap stops at mandatory M0-M3 packs plus `NarrativeProjection`
- later public-life, conflict, governance, and warfare slices must remain on separately named non-MVP paths

## Phase P0 - optional local conflict lite (was M3)
Modules:
- OrderAndBanditry.Lite
- ConflictAndForce.Lite

Implementation note:
- P0 may land incrementally so long as each slice stays additive and preserves M0-M3 stability
- `OrderAndBanditry.Lite` may ship before `ConflictAndForce.Lite`
- order pressure should integrate with trade and projection before richer force resolution is added
- the current P0 authority slice now also persists black-route pressure, paper compliance, implementation drag, route shielding, retaliation risk, and intervention receipts in `OrderAndBanditry` plus gray-route / illicit ledgers in `TradeAndIndustry`
- the current trade slice now also mirrors blockage / seizure pressure onto active routes so route-owned state can explain which road is actually being squeezed
- the order-enabled P0 bridge path may remain available while the full local-conflict lite path enables both `OrderAndBanditry` and `ConflictAndForce`
- active P0 local-conflict work must not alter the active M3 bootstrap surface

Done when:
- local security pressure exists
- black-route pressure and gray-route ledgers stay split across order/trade ownership
- local conflict can happen and be explained
- no tactical micro introduced
- schedule still intact

## Phase P1 - deep society
Modules:
- OfficeAndCareer
- OrderAndBanditry full
- PublicLifeAndRumor depth
- SocialMemoryAndRelations depth

Implementation note:
- the first black-route authority slice is already in place before full order-pack work; later P1 work should extend those same fields and envelopes rather than invent a detached outlaw namespace
- P1 deepens the social layers that M0-M3 only sketched: law, religion, local culture, office conflict, lineage coercion, material life, festivals, and public talk become first-class system topics with owning modules and visible pressure chains

## Phase P2 - county/court
Modules:
- ConflictAndForce full
- OfficeAndCareer depth
- Court-facing surfaces

Implementation note:
- P2 adds county/court-scale play: magistrate appointments, clerk blocs, docket control, patron backing, reputational war, and the blur between office duty and family strategy
- force depth (retainers, militia, escorts, command capacity) is part of P2, not the whole of P2

## Phase P3 - conflict/war
Modules:
- WarfareCampaign
- desk sandbox war overlay
- campaign aftermath projections
- conflict scale ladder full (pressure -> local clash -> tactical-lite -> campaign)

Implementation note:
- the first active `WarfareCampaign.Lite` slice may land before deeper war rules so long as it stays campaign-level, read-only in presentation, and downstream of `ConflictAndForce` / `OfficeAndCareer`
- current board-depth work already adds bounded front labels, command-fit wording, commander summaries, route descriptors, and module-owned campaign intent descriptors plus a built-in schema migration path, without introducing tactical micro
- P3 owns the full conflict/war stack, not just campaign boards

## Phase P4 - historical trends
- historical process packs
- named-figure pressure carriers
- reform pressure (Qingli, Wang Anshi-style)
- regional breadth and cultural differentiation
- more presentation states
- analytics and debug expansions

Implementation note:
- P4 moves from "more content" to "historical momentum": great trends enter as pressure carriers with build-up, window, struggle, and afterlife
- regional breadth is part of P4, not the whole of P4

## Phase P5 - dynasty-cycle agency
Modules / packs:
- future `CourtAndThrone` or `WorldEvents` pack if split out
- deeper `OfficeAndCareer`
- deeper `OrderAndBanditry`
- deeper `ConflictAndForce`
- deeper `WarfareCampaign`
- public legitimacy and memory projections
- dynasty-cycle spatial receipts (grain, routes, army readiness, office fracture, faction backing, succession pressure)

Done when:
- imperial rhythm can affect office, order, public life, military burden, and local households without direct cross-module writes
- rebellion can escalate toward provisional governance or polity formation only through owned pressure chains
- succession struggle, usurpation, restoration, or regime repair can be represented as bounded, deterministic, save-compatible rule chains
- player regime-scale agency still resolves through force, grain, logistics, office access, public legitimacy, information reach, faction memory, and succession conditions
- no detached grand-strategy layer or timeline-editor command surface is introduced
- dynasty-cycle play produces spatial receipts: the sandbox shows legitimacy strain, office fracture, and succession uncertainty as visible terrain/node changes, not just title changes

## Ignored tests (intentional non-goals by phase)
These are not "cut features"; they are explicit scope boundaries that must not be chased during a phase.

| Phase | Ignored (deliberately out) |
|-------|---------------------------|
| M0 | Multi-region play, CJK font asset, tactical combat, office career |
| M1 | Full trade rules, exam season mechanics, conflict resolution, campaign warfare |
| M2 | Cross-region strategy, full office module, dynasty-cycle mechanics, free-roam exploration |
| M3 | Tactical battle maps, full outlaw ecosystem, multi-county order networks |
| P1+ | Timeline editing, player-as-emperor, detached grand-strategy layer |

## Scope discipline
If behind schedule:
1. cut post-MVP packs first
2. cut optional M3 next
3. preserve M0-M2 foundations at all costs

Command window count target by phase:
- M1: 3–4 family lifecycle commands (death, marriage, birth support)
- M2: 6–8 commands (add trade/household + public/local)
- M3: 8–10 commands (add conflict/order bounded actions)
- P1+: expand through office, campaign, and regional commands without losing bounded-leverage principle
