# Warfare, Mobilization, Supply, and Merit

Use this reference when the task is about warfare-lite design, levies, requisition, grain routes, logistics, campaign boards, command pressure, or military merit.

## What To Distinguish

- legal authority to call force versus actual mobilizable manpower
- command title versus command fitness
- mustering versus sustaining a campaign
- food and transport capacity versus nominal troop count
- visible achievement versus political recognition
- merit reward versus blame, exhaustion, and social cost

## Good Zongzu Abstractions

- `authority_tier`
- `mobilization_pool`
- `command_fit`
- `supply_state`
- `grain_route_security`
- `campaign_fatigue`
- `aftermath_pressure`
- `merit_claim`

These abstractions are better than tactical unit detail for this repo because they connect warfare to households, offices, settlements, and memory.
They are not a reason to erase smaller conflict layers such as local clashes, escort engagements, or bounded conflict vignettes.

## Campaign Board Signals

- fronts, routes, depots, crossings, and support regions
- mobilization windows and delayed muster pressure
- grain-line risk and transport exposure
- posture or morale markers instead of unit micro
- aftermath markers for casualties, fatigue, and resettlement strain

## Player Levers

- accelerate muster
- protect grain lines
- conserve force
- push pursuit
- rotate exhausted units
- emphasize visible merit
- trade prestige for logistics reliability

## Deepening Notes

- Pair this file with [military-thought-and-command-culture.md](military-thought-and-command-culture.md) when the user wants `bingfa` rather than only logistics and manpower.
- Use merit as a political and narrative object, not only a post-battle reward track.
- Let campaign aftermath spill into households, towns, lineages, and office careers.

## Design Guidance

- Do not reduce military merit to flat experience points.
- Merit should reflect visible achievement, recognition, blame shifting, casualties, lineage prestige, and future office opportunity.
- Supply and transport should usually be more decisive than nominal headcount.
- For this project, warfare should read as command, mobilization, route, grain, morale, and aftermath pressure on a campaign board rather than a tactical battlefield simulator.
- Use campaign logic only when the scale really is supra-local. Smaller violent events may belong to `ConflictAndForce` as local incidents or tactical-lite encounters.

## Suggested Module Mapping

- `ConflictAndForce`
- `WarfareCampaign`
- `OfficeAndCareer`
- `WorldSettlements`
- `NarrativeProjection`
