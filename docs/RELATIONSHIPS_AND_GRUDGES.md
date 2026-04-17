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

## Design rule
Grudges must be able to persist without being forced to explode every time.
The world should support:
- private resentment
- false reconciliation
- delayed revenge
- generational inheritance of narrative
