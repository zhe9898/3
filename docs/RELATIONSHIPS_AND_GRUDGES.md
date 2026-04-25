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
- clan emotional climate: fear, shame, grief, anger, obligation, hope, trust, restraint, hardening, bitterness, volatility
- person pressure tempering for clan-linked adults

## How other modules interact
- family queries it for marriage and branch tension context
- trade queries it for trust and betrayal context
- office queries it for reputation obligations
- order/banditry queries it for coercion and fear pressure
- conflict queries it for retaliation pressure
- warfare queries it for commander loyalty and inter-clan obligation context

No other module owns these states.

## Pressure tempering kernel

Repeated pressure should not vanish when the notification is dismissed. `SocialMemoryAndRelations` persists the residue as clan climate and personal tempering:
- material pressure from household distress, debt, migration, and trade strain can become fear, shame, anger, bitterness, or hardening
- lineage pressure from branch tension, weak succession, mourning, care burden, funeral debt, and relief sanction can become grief, obligation, restraint, or grudge
- recovery pressure from support reserve, mediation, marriage alliance, exam honor, relief, and trust can become hope, trust, obligation, and restraint
- personality traits from `FamilyCore` shape how adults absorb pressure: prudence and sociability tend toward restraint, ambition can raise hope and volatility, loyalty can deepen obligation or hardening

This is not a mood event pool. Authoritative state changes first; emotional receipts are downstream social-memory facts. Later adult autonomy and command friction may read them through queries, but no module may directly edit another module's state to "make someone feel" a result.

The first active command consumer is `FamilyCoreCommandResolver`. It may read clan climate and adult person-tempering snapshots to adjust family command pressure profiles: bitterness, volatility, anger, grief, or shame can make mediation and lifecycle decisions harder; trust, restraint, obligation, and hope can soften backlash or strengthen reconciliation. Missing SocialMemory queries are neutral. The resolver still mutates only `FamilyCore`, and any later memory residue must return through normal monthly simulation or event handling.

Current public-life/order v3 note:
- public-life order leverage readback may mention likely obligation, shame, fear, trade, office, or ground-pressure residue, but it is projection-only unless `SocialMemoryAndRelations` owns and persists a memory or climate change through its normal event/query seams
- `OrderAndBanditry` command receipts and carryover can explain that a command has a social tail; they do not themselves create durable favor/grudge records outside order-owned state
- future durable order-to-memory residue must add explicit SocialMemory-owned schema, migration, event metadata, and tests rather than parsing command summary text

Current public-life/order v4 note:
- `SocialMemoryAndRelations` now owns a minimal durable public-order residue cut for accepted order interventions such as `添雇巡丁` and `严缉路匪`
- the residue is recorded as existing schema v3 memory, narrative, and climate state, with cause keys such as `order.public_life.fund_local_watch` and `order.public_life.suppress_banditry`
- `OrderAndBanditry` supplies structured aftermath through queries: recent command code/label, carryover, shielding, retaliation, coercion, implementation drag, and pressure fields; SocialMemory decides what obligation, fear, shame, favor, or grudge remains
- no module, projection, shell adapter, or Unity surface may create social residue by interpreting command prose or `DomainEvent.Summary`

Current public-life/order v6 response note:
- refused or partial `添雇巡丁` / `严缉路匪` residue may now receive bounded responses such as `补保巡丁`, `请族老解释`, `押文催县门`, `赔脚户误读`, `暂缓强压`, or `改走递报`
- the response authority trace belongs to the actual owning module: order repair to `OrderAndBanditry`, yamen/document repair to `OfficeAndCareer`, and elder explanation / home-household guarantee repair to `FamilyCore`
- `SocialMemoryAndRelations` does not resolve those commands; on the later monthly pass it reads only structured response aftermath (`Repaired`, `Contained`, `Escalated`, or `Ignored`) and converts that into owned shame, fear, favor, grudge, and obligation residue
- `Repaired` should soften shame/fear/grudge and strengthen favor or trust, `Contained` should hold the matter with obligation still visible, `Escalated` should deepen fear/shame/grudge, and `Ignored` should leave public shame or bitterness attached
- the module may write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` for this response residue; it must not write Order, Family, PublicLife, Governance, Population, or PersonRegistry state
- response residue must not parse `DomainEvent.Summary`, receipt summary text, or `LastInterventionSummary`

Current public-life/order v7 residue-decay note:
- response residue does not disappear after the first readback. `SocialMemoryAndRelations` may later soften repaired后账, carry contained obligation, or harden escalated / ignored后账 by adjusting existing memory weight, narrative pressure, and clan climate.
- a repaired memory should tend toward trust/favor and lower shame/fear/grudge; a contained memory should remain as obligation; escalated or ignored memory should tend toward fear, shame, bitterness, volatility, or grudge.
- later `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` command resolvers may read structured SocialMemory response cause keys and weights to model repeat friction, but they still mutate only their own module state.
- no resolver may parse social-memory summary prose, command receipt text, `LastRefusalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- this pass reuses SocialMemory schema `3`; no new memory namespace, person table, relationship table, or migration is introduced.

Current public-life/order v8 actor-countermove note:
- local actors can now respond to existing response residue even when the player does not immediately issue another command. Route-watch / runner movement belongs to `OrderAndBanditry`, yamen / clerk movement belongs to `OfficeAndCareer`, and elder / household-guarantee movement belongs to `FamilyCore`.
- these actor countermoves read structured SocialMemory residue (`CauseKey`, outcome marker, `Weight`, `State`, `SourceClanId`, `OriginDate`) and skip current-month memories; they do not parse memory summaries, receipt summaries, `LastInterventionSummary`, `LastRefusalResponseSummary`, or `DomainEvent.Summary`.
- any durable shame, fear, favor, grudge, or obligation after the actor countermove is still written only by `SocialMemoryAndRelations` through existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`. v8 adds no new relationship table, person table, memory namespace, schema bump, or migration.

Current public-life/order v13 home-household response note:
- v12 local responses such as `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` can now leave durable SocialMemory residue on the later monthly pass, but only after `PopulationAndHouseholds` has resolved the command and exposed structured aftermath.
- `SocialMemoryAndRelations` reads `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode` through queries and converts `Relieved`, `Contained`, `Strained`, or `Ignored` into existing memory/narrative/climate state.
- This pass still represents thin-chain structure, not thick household-status formulas: no household class ladder, yamen incentive model, or repeated-response ledger is introduced yet.
- The reader must not parse `LastLocalResponseSummary`, command receipt text, memory summary prose, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

Current public-life/order v14 home-household repeat-friction note:
- later local household responses may now read prior home-household SocialMemory residue as friction or support, but only through structured cause keys and weights.
- `PopulationAndHouseholds` may let relieved memories soften local cost, contained memories leave obligation drag, and strained / ignored memories harden later local costs. It still writes only household labor, debt, distress, migration, and local response trace fields.
- This is not a new relationship table, household target system, or thick social formula. Durable memory remains owned by `SocialMemoryAndRelations`; repeated-response command effects remain owned by `PopulationAndHouseholds`.
- No resolver may parse SocialMemory summary prose, `LastLocalResponseSummary`, command receipt text, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

Current public-life/order v15 common-household response texture note:
- later local household responses may now also read existing population-owned household pressure as command-time texture: debt pressure, labor capacity, distress, migration risk, dependents, laborers, and livelihood.
- This texture changes only the `PopulationAndHouseholds` local response cost / outcome path. It does not create durable memory, social obligation, shame, fear, favor, grudge, or clan climate by itself.
- `SocialMemoryAndRelations` still reads only structured local response aftermath on its own later monthly pass and must not parse `本户底色` prose, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

Current public-life/order v16 home-household response capacity note:
- `回应承受线` is a projection / Population command-time capacity cue, not durable social memory state.
- A debt-over-line or labor-floor household may make `凑钱赔脚户` or `遣少丁递信` unfit in the read model and may make a forced command resolve as `Strained`, but only `PopulationAndHouseholds` writes that local response trace at command time.
- `SocialMemoryAndRelations` still waits for its later monthly pass and reads only structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`; it must not parse `回应承受线`, `承受线代价`, `承受线读回`, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

Current public-life/order v17 home-household response tradeoff note:
- `取舍预判`, `预期收益`, `反噬尾巴`, and `外部后账` are projection / Population command-time readability cues, not durable social memory state.
- The forecast may explain why `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` feels different in play, but only structured local response aftermath can later become durable memory.
- `SocialMemoryAndRelations` still waits for its later monthly pass and reads only structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`; it must not parse `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

Current public-life/order v18 home-household short-term readback note:
- `短期后果：缓住项`, `短期后果：挤压项`, and `短期后果：仍欠外部后账` are projected receipt readback cues, not durable social memory state.
- The readback may explain what the household locally eased and what it squeezed, but only structured local response aftermath can later become durable memory.
- `SocialMemoryAndRelations` still waits for its later monthly pass and reads only structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`; it must not parse `短期后果`, `缓住项`, `挤压项`, `仍欠外部后账`, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

Current public-life/order v19 home-household follow-up affordance note:
- `续接提示`, `换招提示`, `冷却提示`, and `续接读回` are projected affordance cues, not durable social memory state.
- The follow-up hint may explain whether repeating or switching a household local response is risky, but only structured local response aftermath can later become durable memory.
- `SocialMemoryAndRelations` still waits for its later monthly pass and reads only structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`; it must not parse `续接提示`, `换招提示`, `冷却提示`, `续接读回`, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

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
