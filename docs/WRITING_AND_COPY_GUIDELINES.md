# WRITING_AND_COPY_GUIDELINES

This document defines how player-facing and developer-facing text should be authored for Zongzu.

The goal is to stop every writing task from feeling like a fresh style problem.
Writers and implementers should choose a copy lane first, then write within that lane's rules.

## Goals

- keep player text clear before making it ornate
- keep in-world wording grounded without drifting into faux-classical fog
- separate system clarity, simulation explanation, and flavor writing
- make shell, notice, conflict, and debug text feel like parts of one product
- reduce late-stage rewriting caused by mixed tone or mixed ownership

## Non-goals

- do not turn every sentence into literary prose
- do not make debug or usability text sound historical
- do not force one wording lane onto every surface
- do not treat flavor as a substitute for explainability

## Core rules

1. Choose the lane before writing the line.
2. One surface should primarily read in one lane.
3. Clarity outranks flourish on every interactive surface.
4. Player-facing language should sound grounded, not antique-for-antique's-sake.
5. Developer-facing language should stay modern, explicit, and searchable.
6. When practical, authoritative summaries should be authored close to the module or projection that owns their meaning, not patched at the very end in UI.

## Lane model

### Lane 1. System usability copy

Use for:
- save/load
- confirm/cancel
- sort/filter
- settings
- keybinds
- accessibility
- menu chrome

Tone:
- plain
- modern
- short
- unambiguous

Should sound like:
- `Save`
- `Load`
- `Confirm separation`
- `Sort by branch pressure`

Should not sound like:
- memorial prose
- archaic commands
- historical roleplay text

### Lane 2. In-world operational shell copy

Use for:
- great hall
- ancestral hall
- desk sandbox
- office surface
- warfare surface
- ledgers and reports

Tone:
- grounded
- concise
- institution-aware
- readable in one pass

Preferred vocabulary for the current baseline:
- hall
- ancestral hall
- yamen
- memorial
- petition
- route
- grain line
- gate
- ferry
- wharf
- posted notice
- market lane
- escort
- county report

Avoid on these surfaces:
- dashboard
- workflow
- ticket
- backlog
- registrar
- analytics
- logistics console

Rule of thumb:
- use concrete social nouns and place nouns
- avoid inflated literary verbs
- avoid generic fantasy bureaucracy

### Lane 3. Authoritative event and outcome summaries

Use for:
- module diff summaries
- event headlines
- consequence summaries
- receipts for bounded commands
- explainability text

These lines must answer:
- what happened
- who it affected
- why it happened
- what the player can do next, if anything

Tone:
- direct
- consequence-led
- grounded
- slightly in-world, but never vague

Formula:
- actor or place
- event
- cause or pressure
- next visible consequence

Example shape:
- `North Wharf households lost grain boats after ferry delays; market pressure will rise next month.`

### Lane 4. Public-life and notice writing

Use for:
- notice tray
- rumors
- market talk
- road reports
- posted notices
- ambient county summaries

Tone:
- social
- slightly more flavorful
- observant rather than omniscient

This lane may be more expressive than Lane 3, but it still must stay readable.
Do not let rumor writing become purple prose.

### Lane 5. Conflict and vignette writing

Use for:
- injury reports
- feud results
- funerals after violence
- raid aftermath
- military pressure summaries
- campaign-board incidents

Tone:
- sharper
- more visual
- still concise

Rule:
- show pressure, damage, and aftermath
- do not drift into tactics-game commentary unless the surface is explicitly tactical-lite

### Lane 6. Character, ceremony, and long-form narrative

Use for:
- visitor scenes
- letters
- ceremony text
- family council speech
- rare longer event writing

Tone:
- strongest flavor
- most characterful
- still bounded by clarity

This is the only lane that should regularly carry stronger rhetorical texture.
Even here, avoid unreadable faux-classical imitation.

### Lane 7. Debug and engineering copy

Use for:
- debug panels
- migration summaries
- payload notes
- inspectors
- schema or replay text

Tone:
- modern
- exact
- engineering-friendly

Never historicalize this lane.

## Surface matrix

### Great hall

Primary lane:
- Lane 2

Secondary lanes:
- Lane 3 for outcome summaries
- Lane 4 for public-life spillover

### Ancestral hall / lineage surface

Primary lane:
- Lane 2

Secondary lanes:
- Lane 3 for inheritance and branch outcomes
- Lane 6 for rare ceremonial or memorial text

### Desk sandbox

Primary lane:
- Lane 2

Secondary lanes:
- Lane 4 for market, gate, ferry, and street pulse
- Lane 5 for local conflict pressure

### Notice tray

Primary lanes:
- Lane 3
- Lane 4

### Conflict vignette

Primary lane:
- Lane 5

Secondary lane:
- Lane 3 when the vignette must explain system consequence

### Campaign board

Primary lane:
- Lane 2 for board labels

Secondary lanes:
- Lane 5 for incidents and aftermath
- Lane 3 for command receipts and consequence summaries

### Debug panel

Primary lane:
- Lane 7 only

## Length discipline

Use these as defaults unless a surface needs something else.

- button or small action: 1 to 3 words
- tab or panel label: 1 to 4 words
- node or marker summary: 3 to 8 words
- notice headline: 4 to 12 words
- notice body: 1 to 3 short sentences
- conflict vignette summary: 1 short paragraph
- debug summary: as explicit as needed

If a player must reread a line to understand the action, shorten it.

## Ownership rules

### Module-owned wording

Prefer module or projection ownership for:
- authoritative change summaries
- receipts
- simulation explanation text
- pressure labels tied to domain meaning

### NarrativeProjection-owned wording

Own:
- grouping into urgent, consequential, and background
- notice composition
- rumor/public-life packaging
- explanation trails spanning multiple events

### Presentation-owned wording

Own:
- stable shell labels
- surface headings
- menu and usability copy
- final normalization of legacy English placeholders at the boundary

Presentation should not invent authority facts.

## Authoring workflow

1. Identify the surface.
2. Choose the lane.
3. Write the plain version first.
4. Add in-world grounding only after the plain version is clear.
5. Check whether the line names actor, place, and consequence clearly enough.
6. Check whether the line belongs at module, projection, or presentation level.

## Fast checks

Before accepting a line, ask:

- could a player understand this on first read
- does it sound like the right surface
- is it too modern for an in-world lane
- is it too flowery for an interactive lane
- did a debug phrase leak into a hall or desk surface
- did faux-historical phrasing leak into a debug or system surface

## Anti-patterns

- writing every surface in one pseudo-historical voice
- leaving player-facing lines as English engineering placeholders
- making notices poetic but uninformative
- making debug text theatrical
- using shell-only replacement to fix deeply wrong module wording
- turning every event into a lore paragraph

## Minimal templates

### Outcome summary template

`[Actor or place] + [change] + [cause] + [next consequence].`

### Public-life summary template

`[Visible place] + [crowd, rumor, or market condition] + [pressure signal].`

### Conflict vignette template

`[Clash or injury] + [immediate result] + [aftermath on family, county, or route].`

### Receipt template

`[Command or intervention] + [acknowledged state] + [when to expect further change].`

## Current baseline

Until replaced by a later explicit decision, player-facing in-world wording should stay aligned with the repository's Northern Song-inspired baseline and its existing shell vocabulary.

This means:
- grounded county and office language
- no dynasty-agnostic fantasy tone
- no modern product-dashboard leakage on hall, desk, office, or warfare surfaces

## Implementation note

When a new feature introduces player-facing text, its spec or PR should declare:
- which lane it belongs to
- which surface owns it
- whether wording is authoritative, projected, or purely presentational

If that cannot be answered, the feature is under-specified from a writing perspective.
