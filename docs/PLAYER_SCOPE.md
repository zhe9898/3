# PLAYER_SCOPE

The player is a clan head, not a god.

For the broader design framing of bounded leverage, limited reach, and multi-generational consequence, see `RULES_DRIVEN_LIVING_WORLD.md`.

## Player resources of influence
The player primarily acts through:
- money
- grain and material support
- prestige
- favors / obligations
- clan authority
- office leverage if available
- force resources if available

## Player command philosophy
Commands are intents.
They resolve through:
- target autonomy
- institutional constraints
- relationship pressure
- available resources
- risk
- current world state

## Command categories
### Family
- arrange marriage
- favor branch or heir
- allocate support
- restrain feud
- endorse split or reconciliation

### Education
- fund study
- hire tutor
- stop study
- prioritize candidate

### Trade
- invest
- cut losses
- appoint manager
- open or close route

### Order / banditry later
- fund local security
- suppress
- negotiate in limited cases
- tolerate at cost

### Force / war later
- mobilize militia
- hire guards
- choose commander
- set campaign objective and stance

## Current first command vertical slice
The current first playable command slice stays thin and bounded.

Enabled through application-routed services only:
- stable M2 and later paths may expose family-council commands such as branch favor, formal apology, branch separation, relief suspension, and elder mediation through `FamilyCore`
- stable M2 and later paths may also expose thin family-lifecycle commands such as `议亲定婚`, `议定承祧`, `拨粮护婴`, and `议定丧次` through `FamilyCore`
- governance-lite may expose petition review and administrative-leverage commands through the office surface
- governance-lite office play should feel like candidate waiting, attached yamen service, recommendation, and formal appointment pressure rather than a one-click exam-to-office ladder
- campaign-enabled warfare may expose plan drafting, mobilization, supply-line protection, and barracks withdrawal through the campaign board surface

Rules:
- these commands still resolve inside application/module code, not in UI
- family commands may write only `FamilyCore`-owned lineage-conflict state directly; downstream memory or narrative changes still happen through later monthly simulation and projection
- family lifecycle commands may still write only `FamilyCore`-owned marriage/heir/mourning state directly; births, deaths, and later public effects still emerge through later monthly simulation and projection
- disabled office or warfare paths must not leak their commands into the shell
- same-month handling is allowed only for explicitly bounded command windows such as office review or campaign directive updates

## What the player cannot do
- rewrite world rules
- force adult compliance absolutely
- issue direct stat edits through UI
- micro units in battle
- bypass command resolution

## Success condition for player scope
When the player wins, it should feel like:
- good leverage
- good preparation
- good judgment
- good timing

not:
- omnipotent click authority
