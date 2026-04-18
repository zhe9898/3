# CONFLICT_AND_FORCE

This document defines the force line from local violence to post-MVP campaign play.

## Position
This project does not use traditional tactical combat as its core.
Instead it uses:
- **MVP abstract conflict resolution**
- **post-MVP campaign sandbox projection**

Both are integrated into the monthly lineage simulation.

## Module split
### `ConflictAndForce`
Owns:
- guards, retainers, clan militia, escorts
- local force readiness
- local conflict outcomes
- persistent local campaign fatigue and escort strain
- authority/mobilization/command/supply basics

### `WarfareCampaign`
Owns:
- campaign board
- mobilized war formations
- routes, fronts, supply lines
- commander fit, morale, pursuit, campaign aftermath

This split is mandatory for extensibility.

## MVP conflict
Supported:
- street violence
- debt enforcement violence
- caravan/road raid
- estate/water/field dispute escalation
- casualty/escort/retainer incident reports

Player acts through:
- restrain
- compensate
- report
- hire guards/escorts
- mobilize limited local force
- prepare or delay retaliation

### MVP local conflict resolution inputs
- available force pools
- readiness
- command capacity
- legitimacy and local support
- relationship/grudge pressure
- order pressure in the settlement
- traits of participants

### MVP outputs
- injury/death/disability
- asset loss
- prestige/legal fallout
- grudge escalation or restraint
- follow-up player decisions

### M3 lite coordination note
- `ConflictAndForce.Lite` may refresh force posture before `OrderAndBanditry.Lite` reads same-month support
- that support now flows through explicit response activation/support state owned by `ConflictAndForce`
- guards, militia, or escorts should matter only when they are actually activated into response posture
- calm or standing-but-untriggered posture should remain visible but should not leak suppression power into other modules
- warfare aftermath may now feed back into `ConflictAndForce.Lite` as owned fatigue / escort-strain fallout, which then drags later readiness and recovery without writing into foreign modules

## Post-MVP campaign sandbox
This is the rare large-scale extension, not the main loop.

### Four gates before battle
1. **Authority** — what force may legally or practically be called
2. **Mobilization** — what troops can actually be gathered
3. **Command** — how much the leader can effectively handle
4. **Supply** — what can be sustained

### Determining troop scale
Do not hardcode “rank X = 5000 troops”.
Use layered limits.

#### A. Authority
`AuthorityTier`
- 0 no formal authority, only private/clan force
- 1 local watch/temporary village power
- 2 county-scale access
- 3 prefectural/regional access
- 4 major operational access
- 5 theater-level access

#### B. Mobilization pools
- official troops
- private retainers
- clan militia
- allied contingent
- mercenaries

#### C. Clan influence factors
- clan prestige
- clan wealth
- clan land power
- martial tradition

These affect response rate, retention, local support, and logistics.

#### D. Effective command
Use a command cap rather than hard lock.

Concept:
```text
CommandCap =
  Base
  + CommandAbility
  + Prestige
  + MilitaryExperience
  + StaffSupport
  + TraitAdjustment
```

If mobilized troops exceed command cap, excess causes:
- discipline loss
- delay
- fragmentation
- higher casualties
- worse execution

#### E. Supply cap
Deployable troops are further constrained by supply.

Concept:
```text
DeployableTroops = min(TotalMobilizable, SupplyCap)
EffectiveTroops  = min(DeployableTroops, CommandCap)
```

## Personality and strategy
The player gives high-level intent.
The commander executes through personality.

### Strategy dimensions
- objective: repel / destroy / escort / delay / hold / relieve / break supply
- stance: cautious / steady / aggressive / desperate
- method: frontal / feint / flank / night raid / split / concentrate / hold route
- loss tolerance: low / medium / severe

### Personality axes
- cautious <-> rash
- decisive <-> hesitant
- disciplined <-> lax
- severe <-> merciful
- suspicious <-> trusting

### Execution concepts
```text
ExecutionQuality =
  StrategyFit
  + CommanderAbility
  + StaffSupport
  + IntelligenceQuality
  - Stress
  - Fatigue
  - SupplyPressure
```

```text
DeviationRisk =
  PersonalityConflict
  + DisciplineProblems
  + OverCommandPenalty
  + Fatigue
  + MoraleInstability
```

## Force types
Keep force categories limited and legible.

Recommended categories:
- retainers / guards
- spear-shield infantry
- archers / crossbow
- light cavalry / riders
- clan militia / levies
- logistics train / engineers (campaign scope)

Fields per force type:
- melee
- ranged
- shock
- defense
- mobility
- discipline
- supply use
- terrain tags

## Logistics
Logistics is not one bar; it is:
- food
- pay/maintenance
- transport capacity
- route safety / local support

Concept:
```text
SupplyRatio = ExpectedSupply / OperationalNeed
```

Supply states:
- abundant
- stable
- strained
- breaking
- collapsed

Supply pressure influences:
- strategy choices
- morale
- desertion
- disease/attrition
- retreat pressure

## Campaign phases
1. muster
2. scouting
3. deployment
4. main engagement
5. collapse/pursuit
6. aftermath

## What matters after war
The most important outputs are:
- deaths and disability
- honors and blame
- branch power shifts
- clan prestige changes
- debt and supply collapse
- route and settlement devastation
- new feuds and fear
- official promotion or punishment
- exhausted guards, escorts, militia, and command capacity that must recover locally over later months
- commoner distress, migration pressure, and local livelihood cracks that feed back into the living settlement

## Anti-patterns
Do not implement:
- unit-level tactics micro
- fully detached battle game
- battle maps as the dominant content form
- war that ignores clan/world consequences
