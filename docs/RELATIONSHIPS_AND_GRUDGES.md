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

Current public-life/order v20 owner-lane return guidance note:
- `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` are projected readback cues, not durable social memory state.
- The guidance may explain that household local response has reached its limit and that Order, Office, or Family lanes still own the repair work. It does not authorize `SocialMemoryAndRelations` to handle commands.
- `SocialMemoryAndRelations` still waits for its later monthly pass and reads only structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`; it must not parse owner-lane guidance prose, `LastLocalResponseSummary`, receipt text, or `DomainEvent.Summary`.

Current public-life/order v21 owner-lane surface readback note:
- Office/Governance and Family-facing copies of `外部后账归位` remain projected readback cues, not durable social memory state.
- The copy may tell the player that county-yamen / document / clerk drag or clan elder / guarantee face still belongs to the corresponding lane, but it does not let `SocialMemoryAndRelations` resolve those commands.
- `SocialMemoryAndRelations` still ignores `该走县门/文移 lane`, `该走族老/担保 lane`, `本户不能代修`, receipt prose, and `LastLocalResponseSummary`; durable shame/fear/favor/grudge/obligation residue still comes only from structured aftermath.

Current public-life/order v22 owner-lane handoff entry readback note:
- `承接入口` is projected player guidance, not durable memory state and not a SocialMemory command input.
- The copy may name existing owner-lane command labels such as `添雇巡丁`, `押文催县门`, or `请族老解释`, but durable shame/fear/favor/grudge/obligation still comes only from structured aftermath read by `SocialMemoryAndRelations`.
- `SocialMemoryAndRelations` must not parse `承接入口`, owner-lane guidance prose, receipt prose, `LastLocalResponseSummary`, or `DomainEvent.Summary`.

Current public-life/order v23 owner-lane receipt status readback note:
- `归口状态`, `已归口到巡丁/路匪 lane`, `已归口到县门/文移 lane`, `已归口到族老/担保 lane`, `归口不等于修好`, and `仍看 owner lane 下月读回` are projected readback cues, not durable social memory state.
- `已归口` does not mean "社会其他人接手"; it only says the after-account is now visible on the owning Order / Office / Family lane through existing structured response trace fields.
- `SocialMemoryAndRelations` must not parse `归口状态`, owner-lane status prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, or `DomainEvent.Summary`; durable shame/fear/favor/grudge/obligation still comes only from structured aftermath.

Current public-life/order v24 owner-lane outcome reading note:
- `归口后读法`, `已修复：先停本户加压`, `暂压留账：仍看本 lane 下月`, `恶化转硬：别让本户代扛`, and `放置未接：仍回 owner lane` are projected readback cues, not durable social memory state.
- The wording helps the player read the existing owner-lane outcome after归口; it does not make ordinary home-household response a universal repair lane and does not authorize `SocialMemoryAndRelations` to handle Order / Office / Family commands.
- `SocialMemoryAndRelations` must not parse `归口后读法`, owner-lane outcome prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, or `DomainEvent.Summary`; durable shame/fear/favor/grudge/obligation still comes only from structured aftermath codes and query snapshots.

Current public-life/order v25 owner-lane social-residue readback note:
- `社会余味读回`, `后账渐平`, `后账暂压留账`, `后账转硬`, `后账放置发酸`, and `不是本户再修` are projected readback cues over existing SocialMemory residue, not new durable social memory state by themselves.
- The wording helps the player see that the later `SocialMemoryAndRelations` monthly pass has begun to settle, hold, harden, or sour the owner-lane after-account. It does not make ordinary home-household response a universal repair lane and does not authorize Application, UI, Unity, Order, Office, Family, or Population code to write SocialMemory.
- Projection may read structured `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, and `OriginDate`; no reader may parse SocialMemory summary prose, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- Same-month SocialMemory de-duplication preserves distinct owner-lane response residues by structured `CauseKey` as well as memory kind, so an Order repair and a Family explanation with the same outcome kind can both remain visible without adding a new memory namespace or schema.

Current public-life/order v26 owner-lane social-residue follow-up guidance note:
- `余味冷却提示`, `余味续接提示`, `余味换招提示`, `继续降温`, `别回压本户`, and `不要从本户硬补` are projected reading cues over existing SocialMemory residue, not new durable social memory state by themselves.
- The wording helps the player read visible residue as cool down, light owner-lane continuation, owner-lane tactic switch, or waiting for a better entry. It does not make ordinary home-household response a universal follow-up lane and does not authorize Application, UI, Unity, Order, Office, Family, or Population code to write SocialMemory.
- Projection may read structured `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane outcome codes; no reader may parse SocialMemory summary prose, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v26 adds no new memory namespace, relationship table, ledger, schema bump, or migration.

Current public-life/order v27-v30 owner-lane closure audit note:
- `现有入口读法`, `建议冷却`, `可轻续`, `建议换招`, `等待承接口`, `后手收口读回`, `已收口`, `仍留账`, `转硬待换招`, `未接待承口`, and `闭环防回压` are projected reading cues, not social-memory facts by themselves.
- The wording helps the player avoid turning the home household into a universal follow-up line. Order, Office, and Family still own their commands and outcomes; SocialMemory still owns durable residue.
- Projection may read structured owner outcome codes and `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, and `OriginDate`; no reader may parse SocialMemory summary prose, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v27-v30 add no memory namespace, relationship table, follow-up ledger, stale-guidance ledger, schema bump, or migration.

Current backend event contract health v32 note:
- V32 diagnostic classifications for `DomainEvent` contract debt are not social memories, relationship edges, grudges, favors, shame, fear, or obligations.
- `SocialMemoryAndRelations` still writes durable residue only from structured aftermath and scoped events; it must not parse diagnostic classification labels, `DomainEvent.Summary`, receipt prose, or projection text.
- V32 adds no memory namespace, relationship table, residue ledger, schema bump, or migration.

Current backend event contract health v33 note:
- V33's no-unclassified gate is also diagnostic/test evidence only. It does not create social memories, relationship edges, grudges, favors, shame, fear, obligations, or a durable event-debt ledger.
- `SocialMemoryAndRelations` still must not parse diagnostic labels, `DomainEvent.Summary`, receipt prose, projection text, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- V33 adds no memory namespace, relationship table, residue ledger, schema bump, or migration.

Current backend event contract health v34 note:
- V34's owner/evidence backlinks are also diagnostic/test evidence only. They are not social memories, relationship edges, grudges, favors, shame, fear, obligations, or durable residue.
- `SocialMemoryAndRelations` still must not parse diagnostic owner labels, evidence backlinks, `DomainEvent.Summary`, receipt prose, projection text, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- V34 adds no memory namespace, relationship table, residue ledger, event-health ledger, schema bump, or migration.

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

## Backend canal-window v35 note

`WorldSettlements.CanalWindowChanged` now returns first to `TradeAndIndustry` and `OrderAndBanditry` owner lanes. This does not create immediate shame, favor, fear, obligation, or grudge records. If canal-window pressure later becomes durable social residue, `SocialMemoryAndRelations` must read structured owner aftermath in its own cadence and write its own existing memory state rather than parsing canal event summaries or trade/order receipt prose.

## Backend household-family burden v36 note

`FamilyCore` now receives a thin same-month signal when existing `PopulationAndHouseholds` household burden facts identify a sponsor clan. This is family pressure, not social memory: charity obligation, reserve drawdown, branch tension, and relief sanction pressure remain family-owned lifecycle pressure. It does not immediately create shame, fear, favor, obligation, or grudge records in `SocialMemoryAndRelations`.

If household-family burden later becomes durable social residue, `SocialMemoryAndRelations` must read structured Population/Family aftermath in its own cadence and write its own existing memory state. It must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, v36 lifecycle wording, or family receipt text.

## Backend office/yamen implementation drag v37 note

`OfficeAndCareer.PolicyImplemented` is an office-owned implementation receipt, not a social memory. It can say whether a policy window moved rapidly, dragged in the yamen docket, was captured by clerks, or stayed at paper compliance, but shame, fear, favor, obligation, and grudge residue still belong to a later `SocialMemoryAndRelations` pass if such a pass is explicitly added.

V37 adds no SocialMemory field, memory namespace, relationship table, residue ledger, schema bump, or migration. `SocialMemoryAndRelations` must not parse `PolicyImplemented` summaries, policy-window prose, diagnostic labels, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastLocalResponseSummary` to create residue.

## Backend office/yamen readback v38-v45 note

V38-V45 adds the explicit later-month SocialMemory side of the v37 office/yamen implementation readback. It is still thin: `SocialMemoryAndRelations` reads structured `JurisdictionAuthoritySnapshot` fields and writes existing memory/narrative/climate records with cause keys such as `office.policy_implementation.<settlement>.<category>`.

This residue is about durable shame/fear/favor/grudge/obligation after county-yamen implementation, paper landing, clerk capture, or document delay. It is not a same-month command result, not an event-pool reaction, not a new relationship table, and not a new memory namespace.

The reader must not parse `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, `LastLocalResponseSummary`, or projected text such as `县门执行读回` / `外部后账归位`. V38-V45 adds no SocialMemory schema bump or migration; it reuses schema `3` records.

## Backend office-lane closure v46-v52 note

V46-V52 adds Office-lane closure readback over that same structured residue surface. `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, `Office闭环防回压`, and `本户不再代修` are projected cues, not durable social-memory facts by themselves.

`SocialMemoryAndRelations` still owns only the later durable shame/fear/favor/grudge/obligation residue. It may be read by Application projections through `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, and `OriginDate`; no reader may parse Office-lane projection prose, SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, or `DomainEvent.Summary`.

The ordinary home-household line remains a low-power local response surface. It cannot repair county-yamen/document/clerk delay, route pressure, clan guarantee face, or social memory by itself. V46-V52 adds no memory namespace, relationship table, residue ledger, schema bump, or migration.

## Backend Family-lane closure v53-v60 note

V53-V60 adds Family-lane closure readback over the same structured residue surface. `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` are projected cues, not durable social-memory facts by themselves.

`SocialMemoryAndRelations` still owns only later durable shame/favor/grudge/obligation residue. Application projections may read `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, and `OriginDate` alongside structured Family snapshots, but no SocialMemory reader may parse Family-lane projection prose, SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.

`FamilyCore` owns clan elder explanation, household guarantee, lineage-house face, and sponsor-clan pressure. The ordinary home-household line remains a low-power local response surface; it cannot repair Family guarantee face or durable social memory by itself. V53-V60 adds no memory namespace, relationship table, Family closure ledger, guarantee ledger, schema bump, or migration.

## Backend Family relief choice v61-v68 note

V61-V68 adds `GrantClanRelief` as a FamilyCore-owned command. It may reduce pressure already stored in `FamilyCore`, but it is not itself a durable shame/favor/grudge/obligation record and does not write `SocialMemoryAndRelations` in the same command.

If the relief choice later leaves social residue, `SocialMemoryAndRelations` must read structured Family aftermath in its monthly cadence and write existing memory/narrative/climate records. It must not parse `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, `不是普通家户再扛`, receipt prose, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, SocialMemory summary prose, or `DomainEvent.Summary`.

The ordinary home-household line still cannot repair lineage-house face or durable social memory by itself. V61-V68 adds no memory namespace, relationship table, relief ledger, charity ledger, Family closure ledger, guarantee ledger, schema bump, or migration.

## Backend Force/Campaign/Regime owner-lane readback v69-v76 note

V69-V76 adds military owner-lane closure readback over existing campaign, force, office, and SocialMemory snapshots. `军务承接入口`, `Force承接读回`, `战后后账读回`, `军务后手收口读回`, `军务余味续接读回`, `军务闭环防回压`, `不是普通家户硬扛`, and `不是把军务后账误读成县门/Order后账` are projected cues, not durable social-memory facts by themselves.

`SocialMemoryAndRelations` still owns only later durable shame/fear/favor/grudge/obligation residue. It may read structured campaign/force aftermath and `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, and `OriginDate`, but no reader may parse military closure projection prose, SocialMemory summary prose, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.

The ordinary home-household line still cannot repair campaign aftermath, force readiness, military order, regime coordination, or durable social memory by itself. V69-V76 adds no memory namespace, relationship table, military closure ledger, owner-lane ledger, cooldown ledger, schema bump, or migration.

## Backend Warfare directive choice depth v77-v84 note

V77-V84 adds `军令选择读回` for existing WarfareCampaign commands. `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, and `归营止损选择` explain which military posture the player selected, but they are not durable shame, favor, fear, grudge, or obligation records by themselves.

`SocialMemoryAndRelations` must not parse `军令选择读回`, `WarfareCampaign拥有军令`, `军务选择不是县门文移代打`, receipt prose, `LastDirectiveTrace`, SocialMemory summary prose, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`. Later military social residue must still come from structured campaign/force aftermath in its own monthly cadence.

The ordinary home-household line still cannot repair military directives or campaign aftertaste. V77-V84 adds no memory namespace, relationship table, directive ledger, military closure ledger, owner-lane ledger, cooldown ledger, schema bump, or migration.
