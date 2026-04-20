# Lineage and Inheritance

Use this reference when the task is about family, lineage, branch households, succession, heirs, adoption, widowhood, marriage leverage, or inheritance disputes.

## What To Distinguish

- household versus lineage: co-residence is not the same as descent or corporate lineage power
- branch household versus main branch: do not flatten every kin group into one undifferentiated clan
- ritual legitimacy versus legal or practical possession
- heir security versus property control
- marriage alliance value versus household labor need
- widowhood, remarriage, guardianship, adoption, and collateral succession as separate pressures

## Good Zongzu Abstractions

- `household_strength`
- `branch_legitimacy`
- `heir_security`
- `inheritance_pressure`
- `ritual_burden`
- `lineage_obligation`
- `marriage_alliance_value`

These should usually be modeled as pressures, claims, and obligations rather than as rigid legal rules copied from a single dynasty.

## Desk Sandbox Signals

- lineage branch weight shown in the hall or lineage surface
- heir instability warnings before succession breaks
- ancestor hall pressure and ritual debt markers
- branch grievance hotspots on the desk board
- event hooks such as heir dispute, adoption bargain, widow guardianship, or branch settlement

## Design Guidance

- Do not assume primogeniture, equal partition, and adoption logic are stable across all eras and regions.
- If the user needs a concrete rule, state the assumed period and confidence level.
- For gameplay, favor intelligible pressure loops over reproducing edge-case legal minutiae.
- A good lineage mechanic should create memory, obligation, rivalry, and prestige pressure, not just move a property number.

## Suggested Module Mapping

- `FamilyCore`
- `PopulationAndHouseholds`
- `SocialMemoryAndRelations`
- `NarrativeProjection`
