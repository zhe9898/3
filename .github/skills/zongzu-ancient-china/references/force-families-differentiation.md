# Force Families Differentiation

Use this reference when the task is about distinguishing `jiabing`, `jiading`, `tuanlian`, official forces, rebel bands, border forces, or other Chinese-ancient armed categories.

## Core Rule

Do not differentiate forces only by weapons.

In this repo, force families should mainly differ by:
- who authorizes them
- who feeds them
- what scale they can sustain
- what social consequences they create
- what surfaces they belong on

## Recommended Force Families

### Household or Lineage Retainers

Examples:
- `jiabing`
- `jiading`
- hall guards
- estate guards

Traits:
- tied to a house, hall, estate, or lineage patron
- fast local response
- good for intimidation, escort, feud pressure, household defense
- weak legitimacy outside patron domain

### Escort and Protection Bands

Examples:
- hired escorts
- route guards
- protection followers

Traits:
- route-oriented
- moderate mobility
- tied to transport, merchants, petitions, grain, or travelers
- strong at keeping one line moving, weak at holding territory

### Militia or `tuanlian`

Traits:
- local mobilization
- tied to county defense, local elite sponsorship, or emergency response
- variable discipline
- stronger public visibility than private retainers

### Yamen Force

Traits:
- tied to office and procedure
- used for arrest, escort, suppression, protection of order
- stronger legal face, limited sustained field capacity

### Official or Regular Detachment

Traits:
- recognized command chain
- better campaign utility
- higher supply expectation
- stronger relation to merit, blame, and office politics

### Rebel or Disorder Band

Traits:
- unstable legitimacy
- opportunistic composition
- can be locally rooted or highly mobile
- strong shock and fear effects, weak administrative integration

### Border or Garrison Force

Traits:
- sustained by frontier or fort logic
- stronger position holding
- tied to pass, depot, wall, or alert-state pressure
- more likely to shape wider campaign posture

## Good Distinguishing Axes

- authorization source
- support source
- route mobility
- territorial hold ability
- discipline level
- public legitimacy
- social fallout

## Good Zongzu Abstractions

- `force_family`
- `authorization_source`
- `support_source`
- `mobility_band`
- `hold_capacity`
- `discipline_band`
- `public_legitimacy_band`
- `fallout_profile`

## Design Guidance

- `Jiabing` and `jiading` are useful because they connect lineage power to coercion without implying a state army.
- `Tuanlian` should feel different from private retainers because it is more openly local-public and emergency-facing.
- Official force should matter not only in battle but in blame, paperwork, and merit.
- Border force should matter because forts, depots, and pass logic change how the conflict reads.

## Suggested Module Mapping

- `FamilyCore`
- `OrderAndBanditry`
- `ConflictAndForce`
- `WarfareCampaign`
- `NarrativeProjection`
