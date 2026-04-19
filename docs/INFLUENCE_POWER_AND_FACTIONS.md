# INFLUENCE_POWER_AND_FACTIONS

This document defines how influence, power, factional pull, and social force should work in Zongzu.

Read this together with:
- `RULES_DRIVEN_LIVING_WORLD.md`
- `PLAYER_SCOPE.md`
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `RELATIONSHIPS_AND_GRUDGES.md`
- `MODULE_BOUNDARIES.md`

## Core rule

Influence is not one mana bar.
Power is not one faction list.

Zongzu should model influence as a layered social field:
- some forms are intimate and kin-based
- some are institutional
- some are economic
- some are coercive
- some are public and reputational
- some are hidden and conditional

What matters is not only who has power, but:
- what kind of power they have
- where it reaches
- what it costs to use
- what converts into what
- what decays, lingers, or backfires

## Influence forms

At minimum, the design should recognize these influence families.

### 1. Lineage prestige

Power rooted in:
- ancestral standing
- branch legitimacy
- ritual continuity
- remembered status

It helps with:
- marriage attractiveness
- internal compliance
- mediation weight
- public face

It weakens through:
- disgrace
- branch fracture
- visible decline
- failed protection

### 2. Favor and obligation

Power rooted in:
- help given
- debts carried
- rescue
- patronage
- protection

It helps with:
- quiet cooperation
- introductions
- delayed repayment
- non-market access

It weakens through:
- time
- humiliation
- unmet reciprocation
- changed circumstance

### 3. Office authority

Power rooted in:
- appointment
- jurisdiction
- seal-bearing legitimacy
- procedural control

It helps with:
- petition handling
- document movement
- timing pressure
- law-backed constraint

It weakens through:
- transfer
- dismissal
- faction loss
- court/local mistrust

### 4. Wealth and credit power

Power rooted in:
- liquidity
- route access
- warehouse control
- lending capacity
- payroll and relief ability

It helps with:
- sustaining households
- buying time
- sponsoring study
- stabilizing dependents
- acquiring influence indirectly

It weakens through:
- price shocks
- route disruption
- debt spirals
- confiscation

### 5. Coercive power

Power rooted in:
- guards
- retainers
- militia access
- local force readiness
- intimidation credibility

It helps with:
- protection
- deterrence
- escorting value
- forcing near-term outcomes

It weakens through:
- suppression
- desertion
- cost
- legitimacy loss

### 6. Public legitimacy and moral face

Power rooted in:
- visible fairness
- charity
- repair sponsorship
- ritual correctness
- public steadiness

It helps with:
- broader compliance
- community tolerance
- softer petition pressure
- easier coalition-building

It weakens through:
- cruelty
- hypocrisy
- scandal
- inability to protect dependents

### 7. Informational reach

Power rooted in:
- clerks
- brokers
- kin letters
- market rumor
- temple and street talk
- messenger speed

It helps with:
- early warning
- leverage timing
- selective concealment
- coordinated response

It weakens through:
- isolation
- censorship
- degraded routes
- reputation collapse

## Power blocs and faction families

Zongzu should avoid a simplistic "red faction vs blue faction" model.
Instead, power should emerge through overlapping blocs:

- household and branch blocs
- affinal blocs
- lineage alliances
- office and yamen blocs
- local notable / strongman blocs
- market and guild blocs
- temple / ritual mediation blocs
- shadow / outlaw blocs
- militia / garrison / military blocs
- court-centered and dynasty-facing blocs

These are not always formal factions.
Many are temporary alignments held together by:
- need
- fear
- debt
- marriage
- procedure
- shared enemies

## Faction logic

Factional pull should behave like this:

- alignment is partial, not absolute
- blocs overlap
- loyalty can be bought, earned, inherited, feared, or merely borrowed
- the same actor may belong to several blocs with conflicting incentives
- blocs should persist long enough to matter, but not as eternal frozen teams

Good faction logic produces situations like:
- a branch is kin-loyal but debt-bound elsewhere
- an office actor depends on clan backing and yamen survival at once
- a merchant house is publicly neutral while quietly financing one side
- a temple mediator softens one feud while strengthening a local notable's standing

## Player-side leverage and limits

The player should feel influence through bounded levers, not direct stat editing.

The player's main influence resources remain:
- money
- grain and material support
- prestige
- favors and obligation
- clan authority
- office leverage when present
- force resources when present

These should map into influence use like this:
- money and grain buy time, shelter, repair, staffing, and relief
- prestige changes willingness, marriage value, and public reading
- favors open doors that money alone cannot open
- clan authority can discipline, allocate, endorse, and suppress within a limited social radius
- office leverage can delay, approve, classify, summon, or redirect
- force can deter or coerce, but at social and legitimacy cost

## Module ownership expectations

Influence must stay modular.

### `FamilyCore`
Owns or projects:
- clan authority
- branch support and favoritism pressure
- lineage prestige projection
- inheritance-backed internal leverage

### `SocialMemoryAndRelations`
Owns:
- favor
- obligation
- shame
- fear
- dependency
- grudge pressure

### `OfficeAndCareer`
Owns or projects:
- office authority
- administrative leverage
- petition handling pressure
- authority trajectory

### `TradeAndIndustry`
Owns or projects:
- credit exposure
- liquidity and debt
- route dependence
- commercial bargaining strength

### `WorldSettlements`
Owns or projects:
- institution availability
- route condition
- settlement-level pressure
- baseline local power environment

### `OrderAndBanditry`
Owns or projects:
- local order support
- coercive pressure environment
- public-order legitimacy strain

### `ConflictAndForce`
Owns or projects:
- organized force readiness
- violent deterrence potential
- escalation risk when force becomes active

### `NarrativeProjection`
Does not own power.
It only renders:
- summaries
- notices
- labels
- visible traces

## System rules

- influence should be partially convertible, but never free to convert
- prestige may help into marriage, office access, or mediation, but not with universal certainty
- wealth may become patronage or protection, but may also attract predation
- office authority may solve one bottleneck while generating another enemy
- coercive power should produce faster outcomes and slower damage
- hidden influence should matter, but players still need enough readable traces to act intelligently

## Shell and readability

Influence and factional pull should appear in the shell through:
- family council weight
- branch compliance or resentment
- petition pressure
- office-facing summaries
- marriage negotiation value
- market trust or reluctance
- notice severity and public confidence
- conflict vignette framing

The shell should let the player infer:
- who can move what
- who is blocked
- who is protected
- who is losing standing
- which bloc is gaining confidence

## Anti-patterns

Do not reduce power to:
- one abstract "influence" number
- static color-coded factions
- free player authority points
- permanent loyalty flags
- universal bribery mechanics

Do not let:
- prestige replace all other power types
- office rank automatically dominate all local realities
- money buy outcomes without mediation, delay, or backlash
- force become a clean shortcut with no social consequences

## Codex review questions

When adding or deepening influence systems, Codex should ask:

- what kind of influence is this
- who actually holds it
- where does it reach
- what does it cost to use
- what weakens it
- what other influence families can it convert into
- which module owns its authoritative state
- how does the shell show it without pretending the player is omniscient
- how does it create politics instead of just another resource bar
