# VISUAL_FORM_AND_INTERACTION

This document defines the experiential shell.

For modern game-engineering standards (Unity, performance, content), see `MODERN_GAME_ENGINEERING_STANDARDS.md`.
For the map and sandbox direction, see `MAP_AND_SANDBOX_DIRECTION.md`.
For the spatial skeleton backend spec, see `SPATIAL_SKELETON_SPEC.md`.

## Form statement
The game should not feel like:
- a raw spreadsheet
- a pure text parser
- a card battler
- a free-roam open world
- a tactical battlefield game

It should feel like:
**a household-side or lineage-side actor seated in lived space, receiving people, letters, pressure, grief, celebration, and consequence.**

## Core visual thesis
Use **spatialized living-society simulation**:
- systemic authority remains data-driven
- player experience is room/object/visitor anchored
- presentation is stronger than a bare status panel, but cheaper than open-world production

The shell must not imply permanent elite insulation.
It should still read correctly if the player's house is strained, diminished, debt-ridden, socially slipping toward ordinary survival, or starting from a poorer position.

## Immersion thesis

Immersion comes from pressure attached to objects, people, and delayed consequence.
It should not come from exposing more controls.

Avoid:
- many sliders as the primary interaction grammar
- full-screen profession labels or career tags
- three routine xun reports per month
- instant abstract feedback such as `+5 credit` as the main result language

Prefer:
- rice jar for food stress
- account book and debt note for cash, rent, and credit
- medicine packet for illness pressure
- study text for education cost and child future
- road marker, ferry marker, and route thread for travel or trade risk
- temple gate and county gate for public help, petition, and institutional contact
- edict scroll, appointment notice, amnesty proclamation, mourning cloth, tax / corvee writ, border dispatch, and yamen docket seal for imperial pressure reaching local life
- monthly review that shows only the pressure closest to the player's current position
- next-month echo through receipt, rumor, obligation, grudge, trust, debt, shame, or altered reach

Imperial pressure should be visible as arrival, interruption, delay, and local interpretation.
The player may see a sealed scroll arrive, a county-gate notice pasted up, a mourning marker dimming the hall, a docket seal slowing petitions, or a border dispatch heating the sandbox.
The shell must not imply the player is directly operating the emperor unless a later court-facing pack has earned that reach through office, faction, legitimacy, force, and information chains.

## Primary spatial anchors
### A. Great hall / study
Main monthly surface and decision room.

### B. Ancestral hall / lineage surface
Family tree, branch memory, ancestor weight, heir context.

**Object anchors on this surface:**
- Ancestral tablet cluster (sealed/unsealed states indicate active mourning)
- Branch ledger scrolls (prestige, tension, outstanding obligations)
- Memorial pile (recent deaths, pending funeral rites, 承祧 gaps)
- Visitor slot (matchmaker, branch elder, petitioning relative)
- Heir marker (unsettled / contested / confirmed)

### C. Macro sandbox
Regional route-pressure board:
- roads and waterways as physical strips on the desk surface
- prefecture and county bands as labeled bands
- grain and petition flow as moving tokens or route-heat coloring
- flood, bandit, and military spillover as edge glow or spill markers
- county-entry pins as push-pins or wax seals

This should still feel like a sand table on a desk, not a detached national map.

**Object anchors:**
- Route strips (physical bands with directional grain-flow markers)
- County-entry seal (wax seal indicating current administrative posture)
- Spillover markers (small incident tokens that slide from macro into desk sandbox)
- Calendar strip (current month plus internal xun/trend indicator aligned to agricultural phase; not three routine player-turn buttons)

### D. Desk sandbox
Local-world board:
- estates
- roads
- markets
- academies
- offices
- security hotspots
- later campaign overlay

**Object anchors:**
- Settlement nodes (physical discs or blocks with current-pressure color)
- Route threads (physical string or drawn line connecting nodes)
- Focus-action cluster (the 1–3 most urgent loci pushed to foreground)
- Notice pin (red/white pin for urgent notices; migrates from desk to great-hall tray)
- Ledger book (open/closed state indicates pending household decisions)
- Seal box (player's available authority: lineage seal, office seal if applicable)

### F. Realm / world sandbox (天下图)
Dynasty-scale pressure board:
- imperial edict reach and delay
- frontier garrison posture
- historical-trend pressure fronts
- dynasty-cycle legitimacy markers
- major grain-route corridors
- region-entry pins (路 / 道)

**Object anchors:**
- Imperial edict scrolls (stacked on sandbox edge, wax seal color = urgency)
- Frontier garrison markers (tower pieces along northern/western edge)
- Legitimacy pillar (central marker whose height/color reflects `MandateConfidence`)
- Succession uncertainty token (crown piece that wobbles or shifts when `SuccessionUncertainty` rises)
- Historical-trend front bands (colored strips advancing across the board)
- Courier route threads (thin threads showing message-delay bands)

This should still feel like the largest sand table in the hall, not a detached strategy map.

### G. Conflict vignette
Short visualized presentation for:
- injury
- raids
- retaliation
- funerals
- wounded return
- later campaign reports

**Object anchors:**
- Aftermath scroll (unrolled/re-rolled indicates read/unread status)
- Casualty marker (small wooden tally for clan losses)
- Retaliation thread (physical thread connecting victim to perpetrator on desk sandbox)
- Damage debris (broken-building token on affected sandbox node)
- Repair token (hammer/wood token placed on damaged node when repair is underway)

## Taskful map principle

Every map surface in the shell must be **taskful**, not decorative. A map is worth opening only when it exposes at least one of:
- **Current pressure objective**: what the player is being asked to respond to
- **Visible route cost**: how much time, grain, or labor a movement requires
- **Information reach**: what the player can actually know from this position
- **Travel/message delay**: how long news or people take to arrive
- **Risk band**: where danger is concentrated
- **Modifier/event**: what seasonal or disaster condition is active
- **Next drill-down locus**: where to look closer

Maps that only show terrain without stakes, route decisions, or pressure interpretation are empty overmaps and must not be built.

## Fog, uncertainty, and partial knowledge

The player's view of the world is bounded by **influence footprint and information reach**:
- **Fog of distance**: settlements beyond the player's information network show only rumor-grade summaries
- **Stale reports**: distant regions display last-known state, not current state; the older the report, the more uncertain the marker
- **Partial overlays**: when the player lacks clerk access, administrative overlays (tax, paperwork) show only rough bands, not precise values
- **Rumor distortion**: public-life surfaces may show contradictory signals; the player must infer which streams are reliable

The shell must preserve a physical **"you are here / your reach ends here"** marker. The player may study the realm, but commands still resolve through influence footprint, route access, office access, public visibility, and message delay.

## Moving physical markers

When flow matters, the shell should show moving or placed physical markers on the sand table:
- **Caravan / grain cart**: moving tokens on trade routes showing grain flow direction
- **Courier / messenger**: small runner tokens on dispatch routes showing message delay
- **Tax packet**: sealed-bundle tokens on tax routes showing collection timing
- **Refugee band**: cluster tokens moving toward safer counties
- **Military column**: column tokens on military move routes showing campaign posture
- **Rumor slip**: floating paper tokens drifting between public-life nodes
- **Storm / flood front**: translucent overlay bands advancing across the board
- **Repair crew**: worker tokens placed on damaged nodes showing repair progress

These markers are **read-only projections** of module state. They do not resolve rules; they make flow visible.

## Time visibility principle

The shell should make sub-month life visible without making sub-month clicking mandatory.

- monthly review remains the foreground decision rhythm
- xun pulse appears as marker motion, route heat, public-life drift, illness trend, and pressure accumulation
- the calendar strip may show where the month currently sits, but it should not ask the player to play three separate turns by default
- urgent interrupts are foregrounded only when a projected red-band threshold demands a response
- after an interrupt is handled, the shell returns to the monthly review / command structure

## Surface grammar: foreground / action / background lanes

Every shell surface follows the same three-lane grammar to prevent dashboard creep:

| Lane | Position | Content | Player relationship |
|------|----------|---------|---------------------|
| **Foreground** | Closest to player eye / center of desk | 1 current locus + 1–3 immediate bounded actions | Must act or explicitly defer |
| **Action** | Mid-ground, reachable but not blocking | Consequence context: what happens if action taken or ignored | Read, interpret, then return to foreground |
| **Background** | Periphery / wall / distant surface | Ambient motion: market bustle, road heat, rumor drift, seasonal change | Absorbs at glance; no required interaction |

This grammar applies to:
- Great hall (foreground = notice tray + visitor; action = opened ledger; background = ambient room state)
- Desk sandbox (foreground = focus-action cluster; action = node consequence panel; background = route heat and seasonal band)
- Ancestral hall (foreground = memorial pile / heir marker; action = branch ledger detail; background = ancestor tablet glow / incense state)

## Interaction language
The player acts through:
- receiving visitors
- opening ledgers and books
- reading letters/reports
- touching desk-sandbox nodes
- unfolding lineage surfaces
- issuing bounded commands
- pressing seals (lineage seal to confirm family decisions; office seal if holding title)

## MVP requirements
Must already include:
- one great hall/study shell
- one lineage surface
- one macro sandbox
- one desk sandbox
- one conflict vignette surface if local conflict pack is on

Does not need:
- free-roam mansion walking
- explorable city traversal
- cutscene-heavy production
- tactical battlefields

## Post-MVP extension
Additively extend:
- room states
- ceremonies
- seasonal changes
- more visitors
- campaign board overlay
- richer ambient life

Do not replace the shell with a different product fantasy.
