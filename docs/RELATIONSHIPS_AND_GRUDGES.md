# RELATIONSHIPS_AND_GRUDGES

This document defines the dedicated social memory module.

## Module owner
All relationship edges, memories, obligation records, fear, shame, and grudge pressure belong to:
`SocialMemoryAndRelations`

This avoids spreading social-state mutation across many modules.

## Two ledgers
### Public ledger
- known alliances
- visible apologies
- public insults
- compensation
- formal obligations

### Private ledger
- private shame
- concealed fear
- hidden resentment
- uncertain blame
- personal debt of honor

## Major grudge families
- blood grievance
- property grievance
- reputation grievance
- authority grievance

## Core state ideas
- relationship edges between people
- memory records about events
- clan narrative summaries that outlive individuals
- escalation, restraint, and reconciliation pressure
- dormant social-memory stubs for actors who leave the dense local horizon without becoming socially irrelevant

## How other modules interact
- family queries it for marriage and branch tension context
- trade queries it for trust and betrayal context
- office queries it for reputation obligations
- order/banditry queries it for coercion and fear pressure
- conflict queries it for retaliation pressure
- warfare queries it for commander loyalty and inter-clan obligation context

No other module owns these states.

## Typical event reactions
- `MarriageArranged` may reduce some old tensions and create new obligations
- `TradeDebtDefaulted` may create shame and property grievance
- `ExamPassed` may create pride, envy, patronage debt, or status pressure
- `ConflictResolved` may create fear, blood grievance, or public humiliation
- `CampaignLost` may create blame narratives or hero cults

## Resolution pathways
Grievances may move through:
- revenge
- formal complaint
- compensation
- mediation
- marriage-based reconciliation
- suppression without true resolution
- fading after death or time
- clan narrative preservation

## Dormant stubs and delayed return

Not every important actor should remain a dense local presence forever.
Some should leave the core ring while still remaining active inside social memory.

Typical cases:
- defeated officials sent into remote service or exile
- kin who marry out, migrate, or fall into distant hardship
- affines and old friends who leave local visibility but remain emotionally or politically relevant
- patrons, clients, and retainers who lose position without losing narrative weight
- disgraced brokers pushed out of county visibility
- branch actors expelled, married out, or displaced
- feud-linked figures who leave but remain narratively charged

For these cases, `SocialMemoryAndRelations` should preserve a dormant stub rather than treating the actor as erased.

A dormant stub should preserve:
- identity anchor
- key relationship edges
- shame / fear / resentment residue
- patronage or faction residue
- outstanding obligations
- the narrative summary by which others still remember them
- hooks that may reactivate them later

This supports outcomes such as:
- false disappearance
- delayed revenge
- delayed aid or recall through kin and friendship ties
- reluctant reconciliation after years away
- frontier-hardening and return
- old faction residue returning through new office openings

The social rule is:

**people may leave the player's dense horizon without leaving the society's memory.**

## Design rule
Grudges must be able to persist without being forced to explode every time.
The world should support:
- private resentment
- false reconciliation
- delayed revenge
- generational inheritance of narrative
