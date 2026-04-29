# MODULE_INTEGRATION_RULES

This document defines exactly how modules may interact.

## Allowed integration channels

### 1. Query
Read-only access to projections or query services.
Use for:
- security pressure lookups
- school availability
- route conditions
- force pool summaries
- grudge pressure summaries

### 2. Command
Intent routed to the owning module.
Use for:
- arrange marriage
- fund study
- expand trade route
- suppress banditry
- mobilize militia
- start campaign

Commands do not guarantee success.
They trigger module-owned resolution.

### 3. DomainEvent
Structured “something happened” outputs.
Use for:
- exam passed
- caravan raided
- branch split
- bandit group formed
- campaign lost

Subscribers may update **their own** state only.

## Forbidden integration
- direct object references to foreign mutable state
- module A changing module B’s private collections
- UI writing into module state
- text templates causing authority changes
- ad hoc global singleton service with cross-module write access

## Event handling rules
- event queues are deterministic
- event ordering is documented by scheduler phase
- handlers must be side-effect limited to owned state
- if a handler needs additional data, it must query published projections
- the current scheduler now performs a deterministic bounded post-simulation event drain before `NarrativeProjection` runs
- each drain round processes only fresh events emitted since the previous watermark, preserving stable module order and preventing replay of the whole month event list
- handler-emitted follow-on events may trigger downstream handlers in the same month only within the bounded drain cap; if a chain cannot finish within the cap, the remaining pressure must be carried as traceable state rather than hidden recursion

## Pressure-chain topology freeze

Current Renzong thin chains are indexed in `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`.

When changing one of those chains:
- update the index in the same change as code or spec edits
- keep the chain's pressure locus explicit before any local mutation
- keep broad pressures such as frontier, court, regime, disaster, or imperial rhythm from fanning out to every jurisdiction unless the rule explicitly says it is realm-wide
- document whether a same-month follow-on uses the bounded scheduler drain or deliberately carries pressure into the next month
- preserve the distinction between a thin topology proof and the full social chain

### Chain 9 v253-v260 regime legitimacy readback integration note

Allowed data path:
- `WorldSettlements.RegimeLegitimacyShifted` structured mandate pressure -> `OfficeAndCareer.OfficeDefected` office-owned defection mutation and metadata -> `PublicLifeAndRumor` matching-settlement public interpretation -> Application `RegimeOfficeReadbackSummary` / governance projection -> Unity copy-only ViewModels

The first-layer readback may show `天命摇动读回`, `去就风险读回`, `官身承压姿态`, `公议向背读法`, `仍由Office/PublicLife分读`, `不是本户替朝廷修合法性`, and `不是UI判定归附成败`.

Forbidden:
- parsing `DomainEvent.Summary`, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`
- treating the readback as a full regime engine, court engine, faction AI, event pool, regime-recognition ledger, legitimacy ledger, defection ledger, owner-lane ledger, cooldown ledger, or policy/court process ledger
- adding a Court module, schema field, migration, save-manifest change, Application rule layer, UI rule layer, Unity authority, `PersonRegistry` expansion, manager/god-controller path, or same-month SocialMemory durable residue

### Chain 9 v261-v268 regime legitimacy readback closeout integration note

V261-V268 adds no new integration channel. It audits the existing v253-v260 data path and records that Chain 9 is closed only as first-layer readback:
- `WorldSettlements.RegimeLegitimacyShifted` structured pressure
- `OfficeAndCareer.OfficeDefected` owner mutation and metadata
- `PublicLifeAndRumor` matching-settlement public interpretation
- Application governance/read-model projection
- Unity copy-only ViewModels

Forbidden:
- treating the closeout as a full regime-recognition system, public-allegiance simulation, faction AI, dynasty-cycle model, Court module, event pool, ledger, schema field, migration, or new scheduler phase
- letting Application/UI/Unity calculate defection success, legitimacy repair, public allegiance, owner-lane status, or SocialMemory residue
- parsing `DomainEvent.Summary`, receipt prose, projection prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`

### Chain 8 v197-v204 first rule-density closeout audit integration note

The v109-v196 first rule-density closeout audit v197-v204 is integration governance only. It documents that Chain 8 has a closed first-layer readback branch, but it adds no new integration channel and no production rule.

Allowed data path:
- already-implemented `WorldSettlements` court agenda facts -> `OfficeAndCareer` policy/local-response owner lane -> `PublicLifeAndRumor` public interpretation -> later `SocialMemoryAndRelations` `office.policy_local_response...` residue -> Application projection/readback -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`
- treating the audit as a new scheduler phase, court process state, appointment slate, dispatch arrival, downstream household/market/public consequence rule, or policy-dispatch completion claim
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt-echo/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating court-policy receipt/docket/public-life readback as durable cooldown, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v189-v196 public-life receipt echo integration note

Court-policy public-life receipt echo is projection-only integration over existing local-response/public-reading inputs. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured cause key and current public-life scalars to show `公议回执回声防误读`, `街面只读已投影的政策公议后手`, and `公议不把回执读成新政令` in public-life command `LeverageSummary` / `ReadbackSummary`.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` -> Application public-life command readback -> Unity copy-only command ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`
- changing command routing, command availability, policy success, cooldown, receipt state, docket state, or owner-lane state
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt-echo/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the public-life echo or receipt guard as a durable cooldown account, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v181-v188 receipt-docket consistency guard integration note

Court-policy receipt-docket consistency guard is projection-only integration over existing local-response/public-follow-up inputs. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured cause key and current public-life scalars to show `回执案牍一致防误读`, `回执只回收已投影的政策公议后手`, and `案牍不把回执读成新政策结果` in governance no-loop / docket guidance readbacks.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` -> Application governance/docket readback -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`
- changing command routing, command availability, policy success, cooldown, receipt state, docket state, or owner-lane state
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the docket guard or receipt guard as a durable cooldown account, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v173-v180 suggested receipt guard integration note

Court-policy suggested receipt guard is projection-only integration over existing local-response/public-follow-up inputs. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured cause key and current public-life scalars to show `建议回执防误读`, `只回收已投影的政策公议后手`, and `回执不是新政策结果` in command receipt `ReadbackSummary`.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` -> Application command receipt readback -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, affordance prose, or `DomainEvent.Summary`
- changing command routing, command availability, policy success, cooldown, receipt state, or owner-lane state
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the receipt guard as a durable cooldown account, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v165-v172 suggested action guard integration note

Court-policy suggested action guard is projection-only integration over the existing follow-up docket guard. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read structured guard eligibility and the already-selected projected affordance to show `建议动作防误读` and `只承接已投影的政策公议后手` in `SuggestedCommandPrompt` / docket `GuidanceSummary`.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` + already-selected `PlayerCommandAffordanceSnapshot` -> Application read model / governance docket prompt -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, affordance prose, or `DomainEvent.Summary`
- changing `SelectPrimaryGovernanceAffordance` priority, command availability, policy success, cooldown, or owner-lane state
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/suggested-action/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the prompt as a durable cooldown account, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v157-v164 follow-up docket guard integration note

Court-policy follow-up docket guard is projection-only integration over existing public follow-up cue inputs. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured outcome code and current public-life scalars to show `政策后手案牍防误读`, `公议后手只作案牍提示`, `不是Order后账`, `不是Office成败`, and `仍等Office/PublicLife/SocialMemory分读` on governance/docket no-loop guard readbacks.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` -> Application read model / governance docket guard -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, or `DomainEvent.Summary`
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/docket/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the guard as a durable cooldown account, an Order after-account, Office success/failure, or home-household debt

### Chain 8 v149-v156 public follow-up cue integration note

Court-policy public follow-up cue is projection-only integration over existing public-reading echo inputs. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured outcome code and current public-life scalars to show `政策公议后手提示`, `公议冷却提示`, `公议轻续提示`, `公议换招提示`, and `下一步仍看榜示/递报承口` on governance and public-life command readbacks.

Allowed data path:
- existing SocialMemory projection + `OfficePolicyLocalResponseResidueCause.OutcomeCode` + `SettlementPublicLifeSnapshot` -> Application read model / command readback -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, or `DomainEvent.Summary`
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading/public-follow-up/cooldown ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating the cue as a durable cooldown account, an Order after-account, Office-only success, or home-household debt

### Chain 8 v141-v148 public-reading echo integration note

Court-policy public-reading echo is projection-only integration over existing SocialMemory and current Office/PublicLife snapshots. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured cause/type/weight and current public-life scalars to show `政策公议旧读回`, `公议旧账回声`, and `下一次榜示/递报旧读法` on governance and public-life command readbacks.

Allowed data path:
- existing SocialMemory projection + `JurisdictionAuthoritySnapshot` + `SettlementPublicLifeSnapshot` -> Application read model / command readback -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, or `DomainEvent.Summary`
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure/public-reading ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating public-reading echo as Order debt, Office-only success, or home-household debt

### Chain 8 v133-v140 memory-pressure readback integration note

Court-policy memory-pressure readback is projection-only integration over existing SocialMemory and current Office/PublicLife snapshots. `SocialMemoryAndRelations` has already written `office.policy_local_response...` residue in a later monthly pass; Application may read the structured cause/type/weight and current policy-window/public-life scalars to show `政策旧账回压读回`, `旧文移余味`, `下一次政策窗口读法`, and `公议旧读法续压`.

Allowed data path:
- existing SocialMemory projection + `JurisdictionAuthoritySnapshot` + `SettlementPublicLifeSnapshot` -> Application read model -> Unity copy-only ViewModels

Forbidden:
- parsing memory summaries, receipt prose, public-life prose, or `DomainEvent.Summary`
- adding a Court module, event pool, dispatch/policy/court-process/memory-pressure ledger, schema field, migration, Application rule layer, UI rule layer, Unity authority, or `PersonRegistry` expansion
- treating old residue as Order debt or home-household debt

### Chain 8 v125-v132 social-memory echo integration note

Court-policy local response residue is a later-month SocialMemory integration, not a new event-pool path or ledger. `OfficeAndCareer` writes structured command aftermath; `SocialMemoryAndRelations` later reads `JurisdictionAuthoritySnapshot` response fields and may write an `office.policy_local_response...` memory. The same structured aftermath must not also be recorded as `order.public_life.response...`, and it must not be read as home-household debt.

Allowed data path:
- `OfficeAndCareer` query snapshot -> `SocialMemoryAndRelations` monthly pass -> existing SocialMemory memory/narrative/climate records
- existing SocialMemory projection -> Application read model -> Unity copy-only ViewModels

Forbidden data path:
- parsing `DomainEvent.Summary`, command receipt prose, public-life notice/dispatch prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`
- Application, UI, or Unity calculating whether the policy response succeeded
- adding a Court module, dispatch/policy/court-process/owner-lane/cooldown/social-memory ledger, or global manager/controller to coordinate the echo

## Projection rules
- projections are read models
- projections may be cached
- projections are rebuilt from authoritative state
- projections are not a backdoor write channel
- read-model snapshots may expose read-only traversal helpers such as lane/item lookup when they only normalize visible data access; they must not grow a second composition or ordering layer beyond the already-built projection payload
- `PersonDossiers` are a presentation-layer join over `PersonRegistry`, `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, and optional `SocialMemoryAndRelations` queries. They are not a person master table, do not expand `PersonRegistry`, do not own commands/events, and must be rebuilt from source projections each read.

## Historical-process integration rules

Historical people, reforms, wars, policies, and great trends must use the same Query / Command / DomainEvent channels as ordinary simulation.

They must not enter as:
- UI-side writes
- narrative-triggered authority changes
- hidden global script state
- one-off year triggers that bypass module ownership

They should enter as:
- upstream pressure in the owning modules
- named-person or faction pressure exposed through queries
- policy windows represented as commands, events, or module-owned state
- structured diffs that describe local implementation and backlash
- projection-only notices that explain why a trend is visible now

The player may carry or bend a great trend only through valid influence channels: household, lineage, market, education, yamen, public-life, force, office, or later court-facing seams.
That influence must still resolve through module-owned state and deterministic command/event handling.
At later scale, the same rule applies to rebellion, polity formation, succession struggle, usurpation, restoration, and dynasty repair: no direct timeline rewrite, but earned counterfactual history is allowed when modules own the pressure, force, legitimacy, logistics, and memory state.

## Integration review checklist
Before approving a cross-module change:
- Who owns the state being changed?
- Could this be an event instead of a direct write?
- Could this be a query instead of a reference?
- Is the foreign state only being read through projections?
- Is save/schema impact documented?
- Does this keep the feature pack additive?

## Canonical bad example
“ExamPassed directly increments Clan.Prestige and creates OfficeRank.”

Why bad:
- exam module is mutating family and office internals

Correct approach:
- `EducationAndExams` emits `ExamPassed`
- `FamilyCore` handles the event to update its own prestige state
- `OfficeAndCareer` handles the event to open or advance office eligibility

## Current command-routing seam

`ModuleRunner<TState>` now exposes a module-owned command seam through `HandleCommand(ModuleCommandHandlingScope<TState>)`. `GameSimulation.IssueModuleCommand(...)` builds the normal query registry, delegates the request to the owning module, and refreshes the replay hash only after an accepted mutation.

`PlayerCommandService` is now dispatch glue over a shared player-command catalog: it resolves existing `PlayerCommandRequest` names to module/surface metadata, reports disabled-module rejection, and does not own consequence formulas for the migrated command slices.

Partial closure note (2026-04-24): the public-life order verbs `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, and `TolerateDisorder` now route through the shared module command seam into `OrderAndBanditryModule.HandleCommand(...)`, so their state changes, receipts, refusals, and one-month carryover are owned by `OrderAndBanditry`. `PlayerCommandService` resolves catalog metadata, reports disabled-module rejection, and delegates through `GameSimulation.IssueModuleCommand(...)`; it must not own order-rule effects.

Until the general seam exists, any command route not yet moved into its owning module remains transitional.  Application services may route, gather query-derived modifiers, and adapt results, but new closure work should keep authority effects in the owning module rather than adding new Application-owned rule layers.  UI and projection code must remain read-only.

## Current M2-lite integration notes
- Renzong chain-1 is currently a **real scheduler thin slice with a first household-profile thickening**, not the full tax/corvee society chain: `WorldSettlements.TaxSeasonOpened` drains through `PopulationAndHouseholds.HouseholdDebtSpiked` and settlement-scoped `OfficeAndCareer.YamenOverloaded` into matching-settlement `PublicLifeAndRumor` street-talk heat. `PopulationAndHouseholds` now converts tax season into debt pressure through existing multi-dimensional household state: livelihood exposure, land visibility, grain/cash buffer, labor/dependency load, debt/distress fragility, and interaction terms. The household handler accepts settlement scope when present and preserves the existing symbolic global thin signal until `WorldSettlements` emits precise settlement tax events; Office must carry the resolved settlement and official metadata forward so public-life does not fan one yamen receipt out to unrelated settlements. The full chain still needs formal household-grade/tax-kind formulas, client/tenant rent cascade, market cash squeeze, long memory, precise settlement tax events, and richer jurisdiction capacity formulas.
- Renzong chain-2 is currently a **real scheduler thin slice with a first household-subsistence thickening**, not the full grain/famine economy chain: `WorldSettlements.SeasonPhaseAdvanced(Harvest)` drains through `TradeAndIndustry.GrainPriceSpike` into `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`, with off-scope settlement protection. `TradeAndIndustry` now carries structured grain-market metadata (`oldPrice`, `currentPrice`, `priceDelta`, `supply`, `demand`), while `PopulationAndHouseholds` decides household distress from a multi-dimensional profile: price pressure, grain-store buffer, livelihood market dependency, labor/dependency load, existing debt/distress fragility, and interaction terms. The full chain still needs formal yield ratio, disaster inputs, granary security, route risk, migration/death pressure, order-route insecurity, and long public/memory residue.
- `EducationAndExams.Lite` currently reads only `WorldSettlements`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `EducationAndExams.Lite` owns study progress, tutor quality, exam attempts, outcomes, and explanation text; it does not write family prestige or office state directly
- `EducationAndExams.Lite` emits `ExamPassed` / `ExamFailed` / `StudyAbandoned` with `EntityKey = personId` (added 2026-04-23). `ExamPassed` now carries credential metadata (`examTier`, score, study progress, academy prestige, tutor quality, clan support, favor/shame, stress) so downstream handlers do not parse `Summary`.
- `FamilyCore` now **consumes** `ExamPassed` via `HandleEvents` (thin slice + first credential-prestige thickening, 2026-04-23): looks up the person's clan, computes `Prestige` / `MarriageAllianceValue` deltas from event metadata plus family-owned clan/person state, and emits `ClanPrestigeAdjusted` with structured cause/source/person/delta/profile metadata.
- Downstream modules must treat `ClanPrestigeAdjusted` metadata as cause-specific: exam-pass adjustments are structured; warfare, marriage, or other prestige causes must add equivalent metadata before consumers depend on them.
- `FamilyCore` does **not** yet consume `ExamFailed` or `StudyAbandoned`; `OfficeAndCareer` does **not** yet consume `ExamPassed`; `SocialMemoryAndRelations` now records exam pass/fail/study-abandonment as scoped aspiration/shame pressure and memory residue, but the full office waiting-list / public-life exam projection chain remains deferred.
- `SocialMemoryAndRelations` now owns the pressure-tempering kernel (schema `3`): xun/month passes combine family lineage pressure, household distress/debt/migration, optional trade debt/reputation, mourning/care/funeral pressure, support, mediation, and FamilyCore personality traits into clan climate and person tempering ledgers. It emits only terminal social-memory receipts after owned state changes and does not feed its own receipts back into authority recursion.
- Social-memory event handlers must stay scoped. Trade shock and family branch events target `ClanId`; exam and death events target `PersonId` and resolve clan through `IFamilyCoreQueries`; settlement or campaign events must carry settlement/entity metadata before memory handlers may mutate. Unscoped legacy events remain readable by projections but must not create broad social-memory writes.
- `OfficeAndCareer` now **consumes** `WorldSettlementsEventNames.ImperialRhythmChanged` for the chain-4 thin slice + first amnesty execution thickening (2026-04-23): it reads `IWorldSettlementsQueries.GetCurrentSeason().Imperial.AmnestyWave`, emits one settlement-scoped `OfficeAndCareer.AmnestyApplied` per jurisdiction only when the wave is newly above threshold, carries structured execution metadata (`AmnestyWave`, authority tier, jurisdiction leverage, clerk dependence, petition backlog, administrative task load), and persists `LastAppliedAmnestyWave` to avoid repeating the same amnesty when other imperial axes change.
- `OrderAndBanditry` now **consumes** `OfficeAndCareer.AmnestyApplied` for the same chain-4 slice: it mutates only the matching settlement's `DisorderPressure` through an order-owned amnesty-disorder profile using the office metadata plus local disorder soil (`DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, `RoutePressure`, suppression buffers), then emits `OrderAndBanditry.DisorderSpike` with cause/profile metadata when the settlement crosses the public-spike threshold. `OrderAndBanditry.DisorderLevelChanged` remains the fuller periodic summary event for later expansion.
- Renzong chain-5 is currently a **real scheduler thin slice with a first frontier-supply household-profile thickening**, not the full frontier/war economy chain: `WorldSettlements.FrontierStrainEscalated` drains through `OfficeAndCareer.OfficialSupplyRequisition` into `PopulationAndHouseholds.HouseholdBurdenIncreased` when household distress crosses the receipt threshold. `OfficeAndCareer` now converts the frontier event into structured supply-execution metadata (`FrontierPressure`, severity, quota pressure, docket pressure, clerk distortion, authority buffer) and leaves office-owned task/backlog pressure on the matching jurisdiction. `PopulationAndHouseholds` then computes household burden from that metadata plus existing household dimensions: livelihood exposure, grain/tool/shelter buffer, labor/dependency load, debt/distress fragility, migration pressure, and interaction terms. The full chain still needs frontier sector allocation, WarfareCampaign mobilization, ConflictAndForce readiness, TradeAndIndustry market diversion, formal quota/cash formulas, public-life military-burden projection, and long memory residue.
- `WorldSettlements.FrontierStrainEscalated` is settlement-scoped in this slice: `EntityKey` must be a `SettlementId`, not a global `frontier` token. `OfficeAndCareer` must emit `OfficialSupplyRequisition` only for a matching jurisdiction and must not fan one frontier signal out to all jurisdictions.
- `WorldSettlements` schema `8` persists `LastDeclaredFrontierStrainBand` as a module-owned watermark so the same active frontier band is not re-declared every month. Future multi-frontier work should replace this thin-slice scalar with per-sector/per-settlement watermarks before adding simultaneous fronts or recurring quota cadence.
- `PopulationAndHouseholds.HouseholdBurdenIncreased` is a downstream receipt event for threshold-crossing household burden; it must be declared in `PublishedEvents` and must carry structured cause/source/settlement/profile metadata rather than asking later modules to parse narrative summaries.
- Renzong chain-6 is currently a **real scheduler thin slice with a first disaster-disorder profile thickening**, not the full disaster-relief society chain: `WorldSettlements.DisasterDeclared` drains through `OrderAndBanditry.DisorderSpike` into `PublicLifeAndRumor` street-talk heat, with off-scope settlement protection. `OrderAndBanditry` now computes disorder delta from disaster metadata plus local order soil (`DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, route rupture / retaliation / implementation drag) and suppression buffers (`SuppressionRelief`, `RouteShielding`, `ResponseActivationLevel`, `AdministrativeSuppressionWindow`), then emits `DisorderSpike` with profile metadata when the threshold is crossed.
- `WorldSettlements.DisasterDeclared` must carry rule-readable metadata (`cause`, `disasterKind`, `severity`, `floodRisk`, `embankmentStrain`) and `OrderAndBanditry` must use that metadata rather than parsing `Summary`; the current slice implements flood only, and the current thickening still remains inside Order-owned state.
- `WorldSettlements` schema `7` persists `LastDeclaredFloodDisasterBand` as a module-owned watermark so the same active flood band is not re-declared every month. Future multi-locus disasters should replace this thin-slice scalar with a per-disaster/per-settlement watermark before adding drought, locust, epidemic, or simultaneous flood fronts.
- `OrderAndBanditry.DisorderSpike` now carries cause/profile metadata (`corvee`, `amnesty`, or `disaster`) so `PublicLifeAndRumor` can project cause-appropriate guidance without hard-coding every disorder spike as corvee pressure. The full chain-6 still needs relief decisions, household subsistence / migration, market panic, route insecurity, SocialMemory residue, and public legitimacy.
- Renzong chain-7 is currently a **real scheduler thin slice with a first clerk-capture profile thickening**, not the full official-clerk-execution chain: `OfficeAndCareer.ClerkCaptureDeepened` is emitted only on a newly crossed settlement clerk-capture edge and drains into scoped `PublicLifeAndRumor` heat. `OfficeAndCareer` now emits structured clerk-capture profile metadata (`ClerkDependence`, `PetitionBacklog`, `AdministrativeTaskLoad`, `PetitionPressure`, authority / leverage buffer, capture pressure components). `PublicLifeAndRumor` uses that metadata to scale street-talk heat while preserving the legacy +12 fallback for older/simple events. `OfficeAndCareer` schema `7` persists `ActiveClerkCaptureSettlementIds` plus later refusal-response trace fields as module-owned office state; repeated high clerk dependence must not re-emit until the condition clears, and off-scope settlements must stay untouched.
- Renzong chain-8 is currently a **real scheduler thin slice with a first policy-window profile thickening**, not the full court-agenda/policy-dispatch chain: `WorldSettlements.CourtAgendaPressureAccumulated` remains court/global input, but `OfficeAndCareer` must allocate it to one selected court-facing jurisdiction before emitting `PolicyWindowOpened`. Selection is office-owned and uses mandate deficit, authority tier, jurisdiction leverage, petition signal, administrative task load, clerk dependence, and petition backlog; the emitted event carries those profile components as metadata. The handler must not fan one court pressure event into all known jurisdictions, and high local drag can prevent a window even under low mandate confidence.
- V37 extends chain-8 one step further inside the same owner lane: `OfficeAndCareer.PolicyWindowOpened` drains into `OfficeAndCareer.PolicyImplemented` through `OfficeAndCareer`'s own event handler. The implementation outcome uses structured policy-window metadata plus existing office/yamen fields, mutates only `OfficeAndCareer` state, and must not parse `DomainEvent.Summary`, receipt prose, or public-life/home-household response summaries.
- Renzong chain-9 is currently a **real scheduler thin slice with a first defection-risk profile thickening**, not the full regime-recognition/compliance chain: `WorldSettlements.RegimeLegitimacyShifted` updates office-owned `OfficialDefectionRisk`, then `OfficeAndCareer.OfficeDefected` is emitted only as a receipt after one highest-risk appointed official actually loses appointment eligibility. The risk profile uses mandate deficit, demotion pressure, clerk pressure, petition pressure, reputation strain, and authority/reputation buffer; a low mandate signal alone must not defect a well-buffered official. Downstream household, market, public-life, memory, and force reactions remain deferred until their owning modules receive a fuller regime-pressure contract.
- Chain-8 / chain-9 imperial pressure must be explicitly seeded or moved by an imperial/court owner. A default `WorldSettlements` state uses neutral `MandateConfidence = 70`; an uninitialized world must not generate court crisis or regime defection by accident.
- `TradeAndIndustry.Lite` currently reads only `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `TradeAndIndustry.Lite` owns clan trade cash/debt state, market pressure, route pressure, outcomes, and explanation text; it does not write household or clan internals directly
- `PublicLifeAndRumor.Lite` now reads `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `OrderAndBanditry`, optional `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces only
- `PublicLifeAndRumor.Lite` owns settlement public pulse only: street-talk heat, market bustle, notice visibility, road-report lag, prefecture-dispatch pressure, public legitimacy, dominant-venue wording, monthly cadence / crowd-mix wording, venue-channel competition metrics, and channel-line wording for notice / street talk / road report / prefecture pressure / contention
- `PublicLifeAndRumor.Lite` may now also compress office-owned `AdministrativeTaskLoad` and `ClerkDependence` into day-facing short-band county-gate heat, notice drift, and prefecture-pressure bias through the existing office query seam; month-end readable diffs and events remain public-life-owned and month-bound
- `WorldSettlements` now owns settlement tier / node rank at schema `2`; presentation and public-life projections must not invent county / market / village rank in UI-only code
- M2 and later manifests may enable `PublicLifeAndRumor` as an additive county-public-life layer without changing ownership of household, office, trade, force, or clan state
- great-hall and desk-sandbox public-life summaries must be rebuilt from `IPublicLifeAndRumorQueries` through the presentation bundle only; UI remains read-only
- monthly cadence labels such as fair days, county-gate docket pressure, or road-report bustle must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel descriptors such as documentary weight, market-rumor flow, verification cost, and courier risk must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel wording such as what the posted notice claims, what street talk says, how road reports differ, and how prefecture dispatch presses downward must also remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- bounded public-life responses may surface as read-only command affordances / receipts on hall or desk nodes, but command resolution must still route through `OfficeAndCareer`, `OrderAndBanditry`, or `FamilyCore` rather than `PublicLifeAndRumor`
- both M2-lite modules emit deterministic domain events and keep outcome explanations derived from queryable state plus kernel RNG only
- `NarrativeProjection` currently reads only the shared `WorldDiff` and `DomainEvent` streams plus its own saved history; it does not emit authority events or write foreign module state
- `NarrativeProjection` may use `FamilyCore` death pressure phrases carried in death diffs, such as `承祧缺口1阶` or `承祧缺口3阶`, to shape family-facing next-step guidance; this remains projection text and must not become a hidden funeral, inheritance, or command-resolution lane
- when a `DeathByViolence` source event targets a clan `PersonId`, `NarrativeProjection` may pull the matching `FamilyCore` death-pressure diff into the notice trace and may also create an ancestral-hall diff notice; both remain downstream projection and do not cause authority state changes
- bounded narrative-history trimming may preserve the latest notification per source module before trimming older overflow, so cross-module visibility stays readable without creating a second authority channel
- the current first-pass presentation shell consumes a read-model bundle only; it does not reference simulation modules directly and does not resolve commands or authority rules inside UI code
- the application-layer presentation bundle may also compose `HouseholdSocialPressureSnapshot` and `PlayerInfluenceFootprintSnapshot` from existing household, family, trade, education, public-life, order, office, and warfare projections; this is runtime-only visibility, not a stored route tag, player class, module key, or authority shortcut
- that influence footprint must distinguish the player's anchor household from observed household pressure: the anchor can carry local agency summaries, while outside households stay readable but not directly commandable unless a real social touchpoint exists

## Family-conflict vertical slice notes
- `FamilyCore` now owns lineage-conflict pressure, mediation momentum, branch-favor pressure, relief-sanction pressure, and last family-command receipts inside the family namespace
- `FamilyCore` schema `8` also owns marriage-alliance pressure/value, heir security, reproductive pressure, mourning load, care load, funeral debt, remedy confidence, charity obligation, clan-scoped spouse/parent/child links, last lifecycle-command receipts, and public-life refusal-response trace fields inside the same namespace
- `FamilyCore` may use `PersonRegistry` command interfaces only for identity-shaped writes when birth, marriage-in spouse creation, or death requires a canonical person anchor; all lineage facts and lifecycle pressures remain `FamilyCore`-owned
- `FamilyCore` may consume `DeathByViolence` from conflict / order / warfare producers only as a lineage-pressure bridge: it parses the event `EntityKey` as `PersonId`, reads identity facts through `PersonRegistry`, updates only clan-owned mourning / inheritance / branch / heir-security pressures, and must not emit a second cause-specific death event from that handler
- family command routing now uses the shared module command seam: `PlayerCommandService` selects the `FamilyCore` module, while `FamilyCoreCommandResolver` owns family consequence formulas and mutates only `FamilyCore` state
- `FamilyCoreCommandResolver` may read `PersonRegistry` queries for identity-only facts such as alive / age when choosing candidates; it must not mutate `PersonRegistry` except through already authorized birth/death identity paths
- `FamilyCoreCommandResolver` may read `SocialMemoryAndRelations` clan climate and adult pressure-tempering snapshots as deterministic command-friction inputs; missing SocialMemory queries remain neutral
- `SocialMemoryAndRelations` may read those family-conflict fields through queries only; it may not be written by the player-command service or by the family command resolver
- bounded family intents such as branch favor, formal apology, branch separation, relief suspension, elder mediation, marriage arrangement, heir designation, newborn care, and mourning order must route into `FamilyCore` through the shared module command seam
- the family-council shell now reads clan conflict summaries, clan narratives, family affordances, and family receipts from read models only
- built-in default loaders now migrate legacy `FamilyCore` schemas through current schema `7` without changing enabled-module or envelope-key sets

## Current observability and migration notes
- diagnostics harness reports and presentation debug snapshots now align on the same runtime-only metrics: diff entries, domain events, notifications, and save payload bytes
- diagnostics harness now also supports multi-seed long-run sweeps plus explicit budget evaluation, still as runtime-only reporting
- diagnostics harness now also records per-module diff/event activity peaks for local-conflict slices as runtime-only reporting
- diagnostics harness now also records runtime-only local-conflict interaction pressure such as activated responses, supported order settlements, and high suppression-demand settlements
- diagnostics harness may now also surface top hotspot settlements by joining `OrderAndBanditry` and `ConflictAndForce` state after simulation; those hotspot summaries remain runtime-only
- those runtime-only observability summaries may now also include order-owned intervention-carryover counts plus shielding-vs-backlash dominance and hotspot fields, still without creating any write path back into authority state
- those observability summaries are derived after authority simulation and never become a backdoor write channel
- save loading now passes through an explicit migration seam with registrable root/module migration hooks, same-version pass-through, and explicit failure when no path is registered

## M3 local-conflict transition notes
- `OrderAndBanditry.Lite` now owns settlement disorder pressure, route insecurity pressure, suppression demand, explanation text, plus black-route pressure / coercion / paper-compliance / implementation-drag / route-shielding / retaliation-risk / suppression-window summaries inside its own namespace
- `OrderAndBanditry.Lite` reads `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, optional `TradeAndIndustry`, optional `OfficeAndCareer`, and optional `ConflictAndForce` projections only
- `OrderAndBanditry.Lite` converts activated local guard, escort, readiness, and militia posture into slower disorder escalation and lower suppression demand through queries only
- `OrderAndBanditry.Lite` also collapses local-force response into two order-owned handoff signals: route shielding when patrol or escort pressure really protects traffic, and retaliation risk when suppression invites backlash
- `OrderAndBanditry.Lite` now also owns explicit public-life intervention receipts for bounded order verbs on the same settlement lane: command code/label plus short summary/outcome text
- `OrderAndBanditry.Lite` now also owns a one-month intervention follow-through window on that same lane; recent `催护一路` / `添雇巡丁` / `严缉路匪` / `遣人议路` / `暂缓穷追` effects must decay inside order-owned state rather than via UI timers or direct trade writes
- `OrderAndBanditry.Lite` now owns the corresponding public-life order command resolver for `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, and `TolerateDisorder`; the application layer supplies only the settlement id, command name/label, and query-derived office-reach modifiers
- `OrderAndBanditry.Lite` may now also read office-owned jurisdiction leverage, clerk dependence, administrative task load, and petition pressure as bounded administrative-reach inputs; it still may not write office state
- those bounded order verbs may now scale their immediate effect against the same office query seam when governance-lite is enabled; when office is disabled or no jurisdiction is present, the order lane stays on its neutral local baseline
- those same public-life order affordances may now also project a read-only execution outlook from the office query seam so hall or desk surfaces can tell whether a command is likely to land as a full push or only half-arrive; UI still does not resolve authority
- `OfficeAndCareer.Lite` may now also read recent order-intervention carryover plus black-route pressure summaries through order queries only; it converts that aftermath into office-owned task load, petition backlog, petition pressure, clerk dependence, and leverage drift on the next monthly pass, without writing any order state back
- those same public-life order receipts may now also surface a read-only office-aftermath execution summary when the next-month jurisdiction trace still carries the same order command label; the receipt remains a projection and does not create a new write lane
- `SocialMemoryAndRelations` may now read the same query-visible public-life order aftermath during its next monthly pass and persist owner-owned residue such as watch obligation, protection favor, crackdown fear, public shame, or grudge pressure inside social-memory state only
- runtime-only observability and hotspot summaries may now also surface that same order-linked office aftermath as read-only administrative task/backlog context, but those fields remain derived after simulation and never become authority state
- the application-layer presentation bundle may now also project a read-only settlement governance lane by joining public-life, order, and office snapshots for the same settlement; this derived summary remains outside all module-owned authority state
- that same governance lane may also nominate one current public-life lead affordance as a read-only next-step prompt, but the prompt must be selected from the existing player-command projections rather than a second hidden command-resolution path
- that same governance lane may now also carry a read-only day-facing public-momentum summary derived from current public-life pressure plus office task-load / clerk-drag projections; it remains projection-only and does not create governance-owned cadence state
- the application-layer presentation bundle may also derive one lead governance focus from those settlement governance lanes so hall surfaces can consume a single monthly docket headline without re-sorting inside UI
- that same application-layer bundle may also derive one read-only governance docket from the selected governance focus plus same-settlement notifications, but governance focus remains primary and notifications only decorate why-now / what-next context after authority has already resolved
- that same read-only governance docket may also pull one same-settlement recent handling receipt from the existing command projections, so hall surfaces can read why-now / what-was-done / what-next without inventing a second command ledger
- that same governance docket may also derive a read-only current phase such as `待即刻应对`, `已下处置`, or `案后收束` from governance-lane pressure plus existing receipts/notifications; this phase remains an application join and does not become module-owned workflow state
- the application layer may also derive one read-only `HallDocketStack` from family lifecycle, governance, and warfare-aftercare projections, with one lead item plus secondary items; the stack remains a projection-only ordering seam and may not become a hidden authority scheduler
- each `HallDocketStack` item may also expose neutral ordering/provenance fields such as ordering summary plus source projection/module keys, but those fields must remain shell-agnostic and may not hard-code hall object grammar, paper types, trays, seals, or other presentation objects into shared contracts
- `OrderAndBanditry.Lite` may now also read trade-owned gray-route ledgers after trade executes in the same month, but only through `IBlackRouteLedgerQueries`
- `TradeAndIndustry.Lite` now reads `OrderAndBanditry` projections when enabled and converts that pressure into route risk plus gray-route ledger updates through queries only
- `TradeAndIndustry.Lite` now distinguishes order-owned route shielding from retaliation risk instead of treating all response pressure as the same trade-side penalty
- `TradeAndIndustry.Lite` may react to recent order-intervention follow-through only by reading `IOrderAndBanditryQueries`; it must not persist or decrement foreign carryover state itself
- `TradeAndIndustry.Lite` now also owns active-route blockage / seizure mirrors and route-constraint traces inside its own namespace
- `ConflictAndForce.Lite` now owns settlement guard, retainer, militia, escort, readiness, command-capacity, and local conflict trace state inside its own namespace
- `ConflictAndForce.Lite` now also owns persistent campaign-fatigue, escort-strain, and campaign-fallout trace state inside the same namespace
- `ConflictAndForce.Lite` reads `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `OrderAndBanditry`, optional `OfficeAndCareer`, and optional `TradeAndIndustry` projections only
- `ConflictAndForce.Lite` now runs before `OrderAndBanditry.Lite` inside the active local-conflict slice so same-month response activation can feed order support without direct writes
- `ConflictAndForce.Lite` now persists explicit response activation/support state instead of relying on trace-text inference alone
- `ConflictAndForce.Lite` may now also consume settlement-targeted warfare aftermath events, but only to update its own fatigue / escort-strain / readiness fallout state
- warfare aftermath fallout now persists in `ConflictAndForce.Lite` for later monthly recovery; it does not write back into `WarfareCampaign`, `OrderAndBanditry`, or office state
- calm or standing-but-untriggered `ConflictAndForce.Lite` posture must not leak support, escort relief, or militia relief into `OrderAndBanditry` until the response is actually activated by local-conflict pressure
- `ConflictAndForce.Lite` emits deterministic readiness and local-conflict events while still updating only its own state
- event observability must classify each emitted-but-unconsumed or declared-but-not-yet-emitted event as one of: projection-only receipt, future contract, dormant source, or alignment bug; a pressure-chain slice is not complete until that classification is documented or tested
- `OrderAndBanditry.Lite` remains available through an order-enabled M3 bridge bootstrap path
- the current public-life order lane already supports `催护一路`, `添雇巡丁`, `严缉路匪`, `遣人议路`, and `暂缓穷追` through thin `PlayerCommandService` routing into the `OrderAndBanditry` resolver; later work should deepen consequences through the same lane rather than invent a second order surface
- `ConflictAndForce.Lite` is available through a conflict-enabled M3 local-conflict bootstrap path and remains absent from active M2 manifests

## Playable closure v2 integration note - 2026-04-24
- `chain1-public-life-order-v2` proves the public-life order lane as pressure -> order-owned state -> projection -> shell affordance -> bounded command -> order-owned receipt/refusal -> next-month governance/order readback.
- Application routing may still pass the existing public-life order command into `OrderAndBanditryModule.HandlePublicLifeCommand`, but it must not compute the order outcome itself; office reach is supplied as query-derived modifiers only.
- Projection joins may show office aftermath tied to the prior order receipt, but no projection, notification, or shell adapter may parse `DomainEvent.Summary` or mutate module state.
- When `OrderAndBanditry` is disabled, public-life order commands must return a refusal and leave save/module projection state unchanged.

## Playable closure v3 leverage note - 2026-04-24
- `chain1-public-life-order-leverage-v3` deepens the same lane with read-only household leverage, cost, and next-month readback text on affordances, receipts, and the governance docket.
- The projection may read existing family prestige/support, social-memory clan narrative, office reach, trade-route exposure, public-life heat, and order pressure snapshots from the presentation bundle. These joins remain runtime-only and do not create a new query interface, command, domain event, module state, or save field.
- The leverage text explains what the home-household is spending, such as lineage face, yamen/document reach, cash/watch labor, mediation, trade exposure, or tolerated ground risk. It is not an authority formula and must not be parsed by later modules.

## Playable closure v4 social-memory residue note - 2026-04-25
- `public-life-order-social-memory-residue-v4` chooses a rule-driven query-visible aftermath seam rather than a command-time event path: `OrderAndBanditry` stores the accepted public-life order receipt/carryover, and `SocialMemoryAndRelations` reads that structured state on the following monthly pass.
- The SocialMemory pass must use fields such as `LastInterventionCommandCode`, `InterventionCarryoverMonths`, `RouteShielding`, `RetaliationRisk`, `BlackRoutePressure`, `CoercionRisk`, and `ImplementationDrag`; it must not parse `DomainEvent.Summary` or command receipt prose.
- The write boundary is strict: Order writes order state, SocialMemory writes social-memory state, and Application/UI/Unity only join read models for `社会记忆读回`.
- Because the residue uses existing SocialMemory schema v3 records, this pass does not add a new module state field, migration, command, feature pack, `PersonRegistry` field, or manager layer.

## Playable closure v5 refusal-residue note - 2026-04-25
- `public-life-order-refusal-residue-v5` extends the same rule-driven loop to partial and refused `添雇巡丁` / `严缉路匪`: Order resolution writes structured outcome/refusal/partial/trace codes, not just receipt prose.
- `OrderAndBanditry` is the only owner of refusal / partial authority classification. Application may pass office-reach modifiers from queries, but it may not compute `县门未落地`, `地方拖延`, or refusal causes.
- `SocialMemoryAndRelations` must consume `LastInterventionOutcomeCode`, `LastInterventionRefusalCode`, `LastInterventionPartialCode`, `RefusalCarryoverMonths`, and pressure fields through `IOrderAndBanditryQueries`; it must not parse `LastInterventionSummary`, `LastInterventionOutcome`, or `DomainEvent.Summary`.
- Governance, public-life, family, and Unity shell readback may display refusal residue only through `PresentationReadModelBundle.SocialMemories`, command receipts, and governance summaries.
- Save impact is limited to `OrderAndBanditry` schema `7 -> 8`; SocialMemory residue continues to use schema `3`.

## Playable closure v6 refusal-response note - 2026-04-25
- `public-life-order-refusal-response-v6` completes the post-refusal account as a rule-driven response loop, not an event-centered or event-pool design: Month N refusal/partial residue is projected in Month N+1, a bounded response affordance is selected from the read model, the owning module resolves the response, and Month N+2 `SocialMemoryAndRelations` reads structured aftermath.
- Response command ownership is split by actual authority: `OrderAndBanditry` owns `补保巡丁`, `赔脚户误读`, and `暂缓强压`; `OfficeAndCareer` owns `押文催县门` and `改走递报`; `FamilyCore` owns `请族老解释`.
- Response aftermath must be one of structured outcome codes `Repaired`, `Contained`, `Escalated`, or `Ignored`, with module-owned trace codes. No Application service, projection, UI, or Unity adapter may compute whether the response worked.
- `SocialMemoryAndRelations` reads only structured response fields such as `LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, and `ResponseCarryoverMonths`; it writes only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Governance lanes, public-life receipts, family surfaces, and Unity shell objects may show whether the后账 was repaired, contained, worsened, or left alone only through projected readbacks. Unity must copy projection fields and must not query modules.
- Save impact is module-local: `OrderAndBanditry` `8 -> 9`, `OfficeAndCareer` `6 -> 7`, and `FamilyCore` `7 -> 8`; `SocialMemoryAndRelations` remains schema `3`.
- `public-life-order-residue-decay-friction-v7` extends the same rule-driven response loop after Month N+2: `SocialMemoryAndRelations` may soften repaired residue, carry contained obligation, or harden escalated/ignored residue by adjusting existing memory weight, clan narrative pressure, and clan emotional climate only.
- Later `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` commands may read structured `SocialMemoryEntrySnapshot` response cause keys and weights as bounded repeat-friction inputs, then mutate only their own owner state. They must not parse social-memory summary prose, receipt prose, or `DomainEvent.Summary`.
- v7 adds no persisted fields and no migration. It reuses SocialMemory schema `3` and the v6 owner response trace fields.
- `public-life-order-actor-countermove-v8` adds a later monthly passive back-pressure layer: `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` may read structured SocialMemory response memories and local clan scope, skip current-month memories, and resolve small deterministic actor countermoves inside their own monthly rules.
- Order actor movement owns route-watch / runner / road-tail outcomes such as `巡丁自补保` or `脚户误读反噬`; Office actor movement owns yamen / clerk / document outcomes such as `县门自补落地` or `胥吏续拖`; Family actor movement owns elder explanation / household guarantee outcomes such as `族老自解释` or `族老避羞`.
- v8 countermoves mutate only the owning module's pressure and existing response trace fields. `SocialMemoryAndRelations` does not resolve those countermoves; it may later read the structured owner aftermath through queries and write only SocialMemory-owned residue.
- v8 adds no persisted fields and no migration. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- `public-life-order-ordinary-household-readback-v10` adds a projection-only ordinary-household readback layer. Application may join `HouseholdPressureSnapshot` with structured Order / Office / Family public-life aftermath to build `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue`, but it must not mutate `PopulationAndHouseholds`, compute response outcomes, or create a new household command owner.
- Desk Sandbox / Unity may copy that household readback from `PresentationReadModelBundle.HouseholdSocialPressures` only. This readback can explain night-road fear, runner/watch misunderstanding, labor/debt/migration strain, and yamen delay, while command resolution remains owner-module and SocialMemory residue remains SocialMemory-owned.
- v10 adds runtime read-model constants only and no persisted fields, schema bump, or migration.
- `public-life-order-ordinary-household-play-surface-v11` may enrich existing public-life response affordances / receipts with a selected ordinary-household stake from `PresentationReadModelBundle.HouseholdSocialPressures`. This is read-model enrichment only: it may expose costed choice and next-readback text, but it must not add a `HouseholdId` command target, compute response effectiveness, mutate `PopulationAndHouseholds`, or write SocialMemory.
- Unity may display those enriched `PlayerCommandAffordanceSnapshot` / `PlayerCommandReceiptSnapshot` fields only. It must not query modules, rescore households, or decide repaired / contained / escalated / ignored outcomes.
- v11 adds no persisted fields, command request shape change, schema bump, or migration.
- `public-life-order-home-household-local-response-v12` adds a narrow population-owned local response surface: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` route through the shared player-command catalog into `PopulationAndHouseholdsModule.HandleCommand(...)`.
- v12 command-time mutation is limited to `PopulationAndHouseholds` household labor, debt, distress, migration, and `LastLocalResponse*` / `LocalResponseCarryoverMonths` fields. It must not mutate `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `SocialMemoryAndRelations`, `PublicLifeAndRumor`, or `PersonRegistry`.
- v12 projections may expose those home-household affordances and receipts when v5/v10/v11 residue is visible, but projection/UI/Unity code must not compute outcome effectiveness and must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- v12 save impact is module-local: `PopulationAndHouseholds` schema `2 -> 3`; Order remains schema `9`, Office schema `7`, Family schema `8`, and SocialMemory schema `3`.
- `public-life-order-home-household-social-memory-v13` lets `SocialMemoryAndRelations` read the structured v12 local-response aftermath on the later monthly pass and convert `Relieved`, `Contained`, `Strained`, or `Ignored` into SocialMemory-owned residue.
- v13 SocialMemory writes are limited to existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`; the reader must not parse `LastLocalResponseSummary`, receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- v13 projection may append the resulting SocialMemory readback to home-household local response receipts, but Application / UI / Unity still do not compute outcome effectiveness or write social memory. v13 adds no persisted state, schema bump, or migration.
- `public-life-order-home-household-repeat-friction-v14` lets `PopulationAndHouseholds` read structured v13 SocialMemory residue when resolving later local household responses. `Relieved` can supply small local support, `Contained` can leave obligation drag, and `Strained` / `Ignored` can add debt or labor drag.
- v14 command-time mutation remains limited to `PopulationAndHouseholds`; `SocialMemoryAndRelations` is read through queries only and is not mutated by the command resolver.
- v14 projections may display the existing SocialMemory hint on local response affordances / receipts, but Application / UI / Unity must not compute the command outcome. v14 adds no persisted state, schema bump, or migration.
- `public-life-order-common-household-response-texture-v15` adds a small population-owned texture profile to the same local household response lane. The resolver reads only existing household pressure fields (`DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, `Livelihood`) and writes only population-owned household cost/outcome trace.
- v15 Application projections may display `本户底色` hints from `HouseholdPressureSnapshot` so the player can distinguish debt-heavy compensation, labor-thin night restriction / road messaging, distress-heavy face pressure, and migration-prone avoidance. These hints are read-model text only; UI and Unity must not query modules or compute final response effectiveness.
- v15 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-response-capacity-v16` adds a projected `回应承受线` to those same three local household responses. Application may derive the affordance enabled/unfit hint from existing `HouseholdPressureSnapshot` fields, but it still does not compute final outcomes or mutate state.
- v16 command-time capacity consequences remain population-owned: `PopulationAndHouseholds` may mark over-line debt or floor-level labor as a strained local aftermath and may append a capacity summary tail, while `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, and `PersonRegistry` remain untouched at command time.
- v16 Unity / shell behavior is copy-only for projected availability, cost, readback, and receipt fields. v16 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-response-tradeoff-v17` adds projected `取舍预判` to those same three local household responses. Application may derive expected benefit, recoil tail, and external-afteraccount boundary from existing `HouseholdPressureSnapshot` fields, but it still does not compute final outcomes or mutate state.
- v17 command-time tradeoff summaries remain population-owned: `PopulationAndHouseholds` may append command summary tails for the local tradeoff, while `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, and `PersonRegistry` remain untouched at command time.
- v17 Unity / shell behavior is copy-only for projected `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, and receipt fields. v17 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-short-term-readback-v18` adds projected receipt-side short-term consequence readback after a local household response resolves. Application may derive `缓住项`, `挤压项`, and `仍欠外部后账` from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v18 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` continues to read only structured aftermath on the later monthly pass and must not parse `短期后果`, `缓住项`, `挤压项`, `仍欠外部后账`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v18 Unity / shell behavior is copy-only for projected receipt fields. v18 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-follow-up-affordance-v19` adds projected follow-up hints to local response affordances after a prior local response. Application may derive repeat/switch/cooldown hints from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v19 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` must not parse `续接提示`, `换招提示`, `冷却提示`, `续接读回`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v19 Unity / shell behavior is copy-only for projected affordance fields. v19 adds no persisted state, command request shape change, schema bump, cooldown ledger, repeated-response counter, or migration.
- `public-life-order-owner-lane-return-guidance-v20` adds projected `外部后账归位` guidance to local response affordances and receipts after a v12-v19 home-household response. Application may join this guidance from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v20 explicitly returns external after-accounts to owner lanes: `OrderAndBanditry` for 巡丁/路匪/route pressure repair, `OfficeAndCareer` for 县门/文移/胥吏续拖, `FamilyCore` for 族老解释/本户担保/宗房脸面, and `SocialMemoryAndRelations` for later durable shame/fear/favor/grudge/obligation residue.
- v20 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` must not parse `外部后账归位`, `该走巡丁`, `该走县门`, `该走族老`, `本户不能代修`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v20 Unity / shell behavior is copy-only for projected fields. v20 adds no persisted state, command request shape change, schema bump, cooldown ledger, owner-lane ledger, household target field, or migration.
- `public-life-order-owner-lane-return-surface-readback-v21` carries the same owner-lane return projection into Office/Governance and Family-facing surfaces. Application may join it from existing household read models, but it must not parse receipts, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v21 keeps ownership unchanged: Order repairs road-watch / route-pressure, Office handles county-yamen / document / clerk drag, Family handles elder explanation / guarantee face, and SocialMemory writes durable residue only on its later structured read.
- v21 Unity / shell behavior remains copy-only for projected fields. v21 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, household target field, or new SocialMemory field.
- `public-life-order-owner-lane-handoff-entry-readback-v22` adds projected `承接入口` text to the same owner-lane guidance. It may name existing command labels such as `添雇巡丁`, `押文催县门`, or `请族老解释`, but it does not create a new command system, command queue, recommendation ledger, or routing layer.
- v22 keeps the same no-summary-parsing rule: Application may derive the guidance from existing `HouseholdPressureSnapshot` structured local-response fields only; SocialMemory must not parse `承接入口`, receipt prose, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v22 Unity / shell behavior remains copy-only for projected fields. v22 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-receipt-status-readback-v23` adds projected `归口状态` text when the relevant owner lane already has structured public-life refusal-response fields. This is owner-lane归口 status, not "社会其他人接手" and not automatic repair.
- v23 may join existing `HouseholdPressureSnapshot.LastLocalResponse*` with `SettlementDisorderSnapshot.LastRefusalResponse*`, `JurisdictionAuthoritySnapshot.LastRefusalResponse*`, or `ClanSnapshot.LastRefusalResponse*`. It must not parse `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v23 Unity / shell behavior remains copy-only for projected fields. v23 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-outcome-reading-guidance-v24` adds projected `归口后读法` text from existing owner-lane outcome codes. It may render `已修复：先停本户加压`, `暂压留账：仍看本 lane 下月`, `恶化转硬：别让本户代扛`, or `放置未接：仍回 owner lane`, but it does not calculate the outcome.
- v24 may join existing `HouseholdPressureSnapshot.LastLocalResponse*` with structured owner snapshots and `LastRefusalResponseOutcomeCode` only. It must not parse `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v24 Unity / shell behavior remains copy-only for projected fields. v24 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-social-residue-readback-v25` adds projected `社会余味读回` text after existing SocialMemory response residue is visible. It may render `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸` from structured outcome codes and the matching active SocialMemory response cause key, but it does not calculate residue or mutate SocialMemory.
- v25 may join existing `HouseholdPressureSnapshot.LastLocalResponse*`, owner-lane `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and `SocialMemoryEntrySnapshot.CauseKey` / `State` / `Weight` / `OriginDate` only. It must not parse SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- `SocialMemoryAndRelations` remains the only writer of durable social residue. Its same-month memory de-duplication now includes `CauseKey`, so distinct owner-lane after-accounts with the same memory kind stay separate without creating a new ledger or namespace.
- v25 Unity / shell behavior remains copy-only for projected fields. v25 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-social-residue-followup-v26` adds projected `余味冷却提示`, `余味续接提示`, and `余味换招提示` text after existing v25 `社会余味读回` is visible. It may tell the player to cool down, lightly continue in the owner lane, switch owner-lane tactic, or wait for a better owner-lane entry, but it does not create a new follow-up command system.
- v26 may join existing `HouseholdPressureSnapshot.LastLocalResponse*`, owner-lane `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and `SocialMemoryEntrySnapshot.CauseKey` / `State` / `Weight` / `OriginDate` only. It must not parse `余味冷却提示`, `余味续接提示`, `余味换招提示`, SocialMemory summary prose, owner-lane guidance prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v26 Unity / shell behavior remains copy-only for projected fields. v26 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-affordance-echo-v27` adds projected `现有入口读法` text to existing owner-lane affordance readback. It may say `建议冷却`, `可轻续`, `建议换招`, or `等待承接口`, but command availability and command routing still come from the existing owner modules and catalog.
- `public-life-order-owner-followup-receipt-closure-v28` adds projected `后手收口读回` text to owner-lane receipts from existing outcome codes and matching SocialMemory cause keys. It must not parse receipt prose, `LastRefusalResponseSummary`, SocialMemory summary prose, or `DomainEvent.Summary`.
- `public-life-order-owner-lane-no-loop-guard-v29` adds projected `闭环防回压` text so old guidance remains readback and does not point back to the home household. It adds no stale-guidance ledger, cooldown ledger, repeated-response counter, or command target.
- `public-life-order-owner-lane-v30-closure-audit` records v20-v30 as a complete projection/readback closure arc. v27-v30 Unity / shell behavior remains copy-only for projected fields and adds no persisted state, command request shape change, schema bump, migration, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, SocialMemory ledger, household target field, command queue, or new SocialMemory field.
- `backend-event-contract-health-v32` classifies ten-year diagnostic DomainEvent contract debt as `ProjectionOnlyReceipt`, `FutureContract`, `DormantSeededPath`, `AcceptanceTestGap`, `AlignmentBug`, or `Unclassified`. This is diagnostic/readback evidence only: it does not add event-pool authority, module rules, persisted state, migration, command surfaces, or projection wording.
- v32 diagnostics may normalize event keys for readability, but they must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or projection text. Any unclassified event contract debt must be documented before being used as proof that a pressure-chain slice is healthy or broken.
- `backend-event-contract-health-no-unclassified-gate-v33` makes that last rule executable: the ten-year diagnostic run now fails if current emitted-without-authority-consumer or declared-but-not-emitted debt remains `Unclassified`.
- V33 still does not add a module handler, event pool, scheduler phase, command surface, projection wording, persisted state, schema bump, or migration. It reads only structured event contract declarations and runtime diagnostic counts.
- `backend-event-contract-health-evidence-backlinks-v34` adds `owner=<module>` and `evidence=<doc/test backlink>` to diagnostic classification readback. This is still developer evidence only: the owner lane comes from the structured event key/module prefix, not from summary prose, and it does not create a handler, event pool, scheduler phase, command surface, projection wording, persisted state, schema bump, migration, event ledger, or owner-lane registry.
- `backend-canal-window-trade-order-thin-chain-v35` adds real owner-module consumers for `WorldSettlements.CanalWindowChanged`. `WorldSettlements` still owns the canal-window fact; `TradeAndIndustry` owns market/route/black-route ledger adjustments; `OrderAndBanditry` owns route pressure, black-route pressure, suppression demand, and route-shielding adjustments. The handlers use `IWorldSettlementsQueries` and structured canal-window metadata, never `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- V35 adds no command surface, event pool, scheduler phase, projection authority, persisted state, schema bump, migration, canal ledger, owner-lane ledger, cooldown ledger, household target field, UI/Unity rule path, or SocialMemory write path.
- `backend-household-family-burden-thin-chain-v36` adds a thin owner handoff from `PopulationAndHouseholds` household burden facts to `FamilyCore` sponsor-clan pressure. `FamilyCore` reads the household through `IPopulationAndHouseholdsQueries`, resolves `SponsorClanId`, and writes only existing family-owned pressure fields.
- V36 same-month handling remains a scheduler event-drain path, not an event pool or new command system. It must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or household local-response prose.
- V36 adds no command surface, projection authority, persisted state, schema bump, migration, relief ledger, sponsor-lane ledger, household target field, UI/Unity rule path, or SocialMemory write path.

## Backend office/yamen readback v38-v45 integration note
- V38-V45 is the readback spine after v37 `OfficeAndCareer.PolicyImplemented`, not a new command system, event pool, thick county-yamen formula, or clerk AI.
- `PublicLifeAndRumor` consumes `OfficeAndCareer.PolicyImplemented` and settlement-scoped `OfficeAndCareer.OfficeDefected` through structured event metadata and updates existing public-life readback fields only.
- Governance and player-command read models may show `县门执行读回`, `县门/文移后手`, `官员摇摆读回`, route-map guidance, and residue-health guidance from structured snapshots. They must not parse `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, or `LastLocalResponseSummary` as rule input.
- `SocialMemoryAndRelations` writes office/yamen residue only in its later monthly pass, reading `JurisdictionAuthoritySnapshot.PetitionOutcomeCategory`, `AdministrativeTaskLoad`, `ClerkDependence`, `PetitionBacklog`, `PetitionPressure`, `JurisdictionLeverage`, and `AuthorityTier`. Same-month response/implementation handling must not mutate SocialMemory.
- Unity copies projected DTO fields only. It does not query modules, inspect event metadata, compute implementation effectiveness, infer owner lanes, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, policy ledger, yamen workflow state, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Backend office-lane closure v46-v52 integration note
- V46-V52 is projection/readback closure over the existing Office lane after v38-v45. It is not a new command system, event pool, thick county-yamen formula, clerk AI, or persisted closure ledger.
- Governance, docket, office, and receipt read models may show `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, and `Office闭环防回压` through `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary`.
- The readback may read structured `JurisdictionAuthoritySnapshot` fields, existing `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`. It must not parse `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- `OfficeAndCareer` remains the owner of county-yamen / document / clerk follow-through. Ordinary home-household responses remain local `PopulationAndHouseholds` pressure relief and are not a universal Office repair lane.
- `SocialMemoryAndRelations` may only write durable Office residue in its later monthly pass from structured aftermath. It must not treat Office closure projection text as a command or residue source.
- Unity copies projected DTO fields only. It does not query modules, compute closure, infer owner lanes, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, policy ledger, yamen workflow state, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, or save-manifest change.

## Backend Family-lane closure v53-v60 integration note
- V53-V60 is projection/readback closure over the existing Family lane after public-life/order owner-lane guidance has pointed clan elder explanation, household guarantee, lineage-house face, and sponsor-clan pressure back to `FamilyCore`. It is not a new command system, event pool, thick clan economy, elder AI, branch-faction model, guarantee formula, or persisted closure ledger.
- Public-life, family-facing, governance, docket, and receipt read models may show `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` through runtime Family closure fields.
- The readback may read structured `ClanSnapshot`, `HouseholdPressureSnapshot`, `SponsorClanId`, existing `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`. It must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or projected Family prose.
- `FamilyCore` remains the owner of clan elder explanation, household guarantee, lineage-house face, and sponsor-clan pressure. Ordinary home-household responses remain local `PopulationAndHouseholds` relief/strain and are not a universal Family repair lane.
- `SocialMemoryAndRelations` may only write durable shame/favor/grudge/obligation residue in its later monthly pass from structured aftermath. It must not treat Family closure projection text as a command or residue source.
- Unity copies projected DTO fields only. It does not query modules, compute closure, infer guarantee success, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, Family closure ledger, guarantee ledger, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, or save-manifest change.

## Backend Family relief choice v61-v68 integration note
- V61-V68 adds one bounded FamilyCore command, `GrantClanRelief`, after Family closure readback makes sponsor-clan pressure legible. It is a rule-driven command/readback addition, not an event pool, thick clan economy, elder AI, branch-faction model, guarantee formula, or persisted relief ledger.
- `FamilyCore` resolves the command against existing `CharityObligation`, `SupportReserve`, `BranchTension`, `BranchFavorPressure`, `ReliefSanctionPressure`, and `MediationMomentum` fields. It writes only `FamilyCore` state and existing conflict receipt fields.
- `Application` routes/catalogs and projects Family-facing affordance/readback copy. It must not compute command results, parse receipt prose, choose hidden household targets, or maintain relief/owner-lane state.
- `PopulationAndHouseholds` is not mutated by `GrantClanRelief`; ordinary household local response remains low-power relief/strain and not a universal fix line. `SocialMemoryAndRelations` is not written during the same command and may only handle later durable residue from structured aftermath.
- Unity copies projected DTO fields only. It does not query modules, compute relief outcome, infer sponsor targeting, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, relief ledger, charity ledger, guarantee ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Governance-lite notes
- `OfficeAndCareer.Lite` now owns office appointments, authority tier, candidate waiting pressure, clerk dependence, service progression, administrative tasks, petition backlog/outcomes, jurisdiction leverage, petition pressure, jurisdiction task load, and explanation text inside its own namespace
- `OfficeAndCareer.Lite` now reads `EducationAndExams`, `SocialMemoryAndRelations`, and optional `OrderAndBanditry` projections only
- the new governance-lite bootstrap path enables `OfficeAndCareer` without mutating the stable M2 or M3 manifests
- bounded office intents such as petition review, administrative leverage, county notice, and road dispatch now route through `OfficeAndCareerCommandResolver`; they may not write family, trade, order, or force state directly
- governance-lite now treats local-exam success as entry into a bounded office funnel rather than direct appointment: recommendation, waiting for openings, and attached yamen service stay office-owned and deterministic
- the lighter office v2.1 slice now exposes petition-outcome category, administrative-task tier, promotion/demotion labels, and authority-trajectory wording as derived read-model/query fields only; it does not add new office-owned save fields
- office influence stays bounded: downstream modules may read leverage or petition pressure, but only `OfficeAndCareer` mutates office appointments and jurisdiction authority
- governance-lite loads now include built-in `OfficeAndCareer` `1 -> 2 -> 3` migration for legacy office saves
- legacy office migration now reconstructs missing service/task/petition details first, then backfills queue pressure and clerk dependence conservatively for schema `3`

## Campaign-lite integration notes
- `WarfareCampaign.Lite` is now active only through the dedicated campaign-enabled bootstrap/load path; stable M2/M3/governance-lite paths remain warfare-free
- `WarfareCampaign.Lite` owns campaign boards, campaign aftermath summaries, and mobilization-signal snapshots inside its own namespace
- `WarfareCampaign.Lite` reads `ConflictAndForce`, `WorldSettlements`, and optional `OfficeAndCareer` projections only
- `WarfareCampaign.Lite` does not write force posture, office leverage, or settlement baselines back into upstream modules
- campaign read models now flow through `IWarfareCampaignQueries`, using local-force posture plus office coordination as upstream signals rather than inventing a second force-ownership model
- the active warfare-lite slice currently emits bounded campaign events only: mobilization, front-pressure escalation, supply strain, and aftermath registration
- warfare-lite events now also carry settlement-targeting metadata so downstream handlers can update their own state without parsing narrative strings
- current warfare-lite contracts already carry order-support, office-authority-tier, administrative-leverage, petition-backlog, mobilization-window, command-fit, route-flow, and office-coordination precursor fields so later campaign depth can stay query-first
- the current board-depth refinement persists commander summaries plus bounded route descriptors inside `WarfareCampaign` rather than synthesizing them in UI-only code
- `WarfareCampaignCommandResolver` now stages `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` into `WarfareCampaign`-owned directive state only
- the current player-command vertical slice may expose those same warfare directives as read-only affordances and receipts in presentation, but authoritative resolution stays in the module command seam and writes stay inside `WarfareCampaign`
- current warfare-lite state now persists active directive code/label/summary and last directive trace inside the warfare namespace instead of inventing a cross-module command ledger
- built-in `WarfareCampaign` migration now upgrades schema `1 -> 2 -> 3 -> 4` by backfilling labels, route descriptors, directive descriptors, and campaign phasing plus aftermath dockets without changing enabled-module or envelope-key sets
- current warfare-lite aftermath now propagates into `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` through the event-handling seam only
- those downstream modules update only their own prosperity, livelihood, prestige, ledger, pressure, petition, or memory state; none of them write back into `WarfareCampaign`

## Post-MVP preflight seam notes
- black-route depth now has explicit preflight query seams: pressure snapshots stay aligned with `OrderAndBanditry`, while gray-route / illicit ledger snapshots stay aligned with `TradeAndIndustry`
- the first active authority slice now persists paper-compliance, implementation-drag, route-shielding, retaliation-risk, administrative suppression-window, escalation-band, seizure-risk, and diversion-band fields inside those two owned modules only
- no standalone `BlackRoute` module key or save namespace should be introduced; future black-route migrations must stay inside the `OrderAndBanditry` and `TradeAndIndustry` module envelopes

## Backend Force/Campaign/Regime owner-lane readback v69-v76 integration note
- V69-V76 is projection/readback closure over existing warfare-lite and force/regime snapshots. It is not a new command system, event pool, event-chain body, thick campaign AI, force economy, Office override, Order repair path, or persisted closure ledger.
- Governance, owner-lane docket, warfare command affordances, and campaign receipts may show `军务承接入口`, `Force承接读回`, `战后后账读回`, `军务后手收口读回`, `军务余味续接读回`, `军务闭环防回压`, `不是普通家户硬扛`, and `不是把军务后账误读成县门/Order后账`.
- The readback may read structured `CampaignMobilizationSignalSnapshot`, `CampaignFrontSnapshot`, `JurisdictionAuthoritySnapshot`, `ClanSnapshot`, and `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` / `OriginDate`. It must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, campaign prose, or projected readback text.
- `ConflictAndForce`, `WarfareCampaign`, and `OfficeAndCareer` keep their state ownership. `PopulationAndHouseholds` remains a local household response lane, and `SocialMemoryAndRelations` remains the later durable residue owner.
- Unity copies projected DTO fields only. It does not query modules, compute closure, infer owner lanes, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Backend Warfare directive choice depth v77-v84 integration note
- V77-V84 keeps the existing WarfareCampaign command seam. `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` are still resolved by `WarfareCampaignCommandResolver` and write only existing directive state.
- The new readback wording may show `军令选择读回`, `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, `归营止损选择`, `WarfareCampaign拥有军令`, and `军务选择不是县门文移代打`. These are command/readback cues, not an event pool, directive ledger, or yamen/Order substitute.
- Application projections may compose directive-choice readback with v69-v76 military closure guidance from structured campaign snapshots. They must not compute military success, force readiness, official paperwork outcomes, household repair, durable residue, or owner-lane closure.
- `ConflictAndForce`, `OfficeAndCareer`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` are not mutated by the command. Later durable residue must still come from structured aftermath in `SocialMemoryAndRelations`, never from readback prose.
- Unity copies the projected command/receipt fields only. It does not query modules, execute from prose, infer owner lanes, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, directive ledger, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Backend Warfare aftermath docket readback v85-v92 integration note
- V85-V92 reuses the existing `WarfareCampaign` aftermath docket seam. `AftermathDocketSnapshot.Merits`, `Blames`, `ReliefNeeds`, and `RouteRepairs` are structured campaign-owned read data, not a new command surface, event pool, or post-battle ledger.
- Application projections may show `战后案卷读回`, `记功簿读回`, `劾责状读回`, `抚恤簿读回`, `清路札读回`, `WarfareCampaign拥有战后案卷`, `战后案卷不是县门/Order代算`, `不是普通家户补战后`, and `军务案卷防回压` from those lists only.
- `OfficeAndCareer`, `OrderAndBanditry`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` are not mutated by the readback. Later durable residue must still come from structured aftermath in `SocialMemoryAndRelations`, never from docket/readback prose.
- Unity copies or renders projected aftermath docket snapshots only. It does not derive docket contents from notifications, event traces, settlement stats, receipt prose, or `DocketSummary`.
- Save/schema result: no persisted state, schema bump, migration, aftermath ledger, relief ledger, route-repair ledger, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Backend court-policy process readback v93-v100 integration note
- V93-V100 reuses the existing thin court-policy path: `WorldSettlements.CourtAgendaPressureAccumulated` feeds `OfficeAndCareer.PolicyWindowOpened`, which resolves as `OfficeAndCareer.PolicyImplemented`, then `PublicLifeAndRumor` projects notice, dispatch, and public interpretation. This is a rule-driven command / aftermath / social-memory / readback loop, not an event pool or notification-driven design body.
- Application projections may show `朝议压力读回`, `政策窗口读回`, `文移到达读回`, `县门执行承接读回`, `公议读法读回`, `Court后手不直写地方`, `Office/PublicLife分读`, `不是本户也不是县门独吞朝廷后账`, and `Court-policy防回压` from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` fields only.
- `OfficeAndCareer` owns the policy window and yamen implementation result; `PublicLifeAndRumor` owns the street/public interpretation; `PopulationAndHouseholds` remains ordinary local household response only; `SocialMemoryAndRelations` remains later durable residue only.
- The projection must not parse `DomainEvent.Summary`, receipt prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- Unity copies projected court-policy fields only. It does not query modules, compute policy outcome, infer ownership from prose, maintain dispatch/policy/owner-lane ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, court module, dispatch ledger, policy closure ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Thin-chain closeout audit v101-v108 integration note
- V101-V108 adds no integration channel. It audits the existing Query / Command / DomainEvent / projection seams through v100 and records that the thin-chain skeleton is closed as topology evidence, not as full rule-density.
- "Thin-chain closed" means each live chain has a documented source pressure, owner module, scheduler drain or delayed monthly residue path, structured metadata/query seam, repetition guard, off-scope boundary where needed, and readback surface. It does not mean the full historical or social formula is implemented.
- No module may use this closeout as permission to parse `DomainEvent.Summary`, receipt prose, projected readback strings, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No Application/UI/Unity code may convert the closeout into an owner-lane ledger, cooldown ledger, dispatch ledger, relief ledger, aftermath ledger, household target field, or second command/result layer.

## Backend court-policy process thickening v109-v116 integration note
- V109-V116 is Chain 8 first rule-density work over the existing `WorldSettlements.CourtAgendaPressureAccumulated -> OfficeAndCareer.PolicyWindowOpened -> OfficeAndCareer.PolicyImplemented -> PublicLifeAndRumor` path. This remains a rule-driven command / aftermath / social-memory / readback loop, not an event pool or notification-driven design body.
- `WorldSettlements` supplies court agenda / mandate pressure; `OfficeAndCareer` allocates policy windows and resolves county/yamen implementation posture; `PublicLifeAndRumor` reads structured `PolicyImplemented` metadata into notice, dispatch, and public interpretation; `SocialMemoryAndRelations` may only write later durable residue from structured aftermath.
- Application projections may show `政策语气读回`, `文移指向读回`, `县门承接姿态`, `公议承压读法`, `朝廷后手仍不直写地方`, and `不是本户硬扛朝廷后账` from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` fields only.
- The integration must not parse `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or projected court-policy prose.
- Unity copies projected court-policy thickening fields only. Save/schema result: no persisted state, schema bump, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.

## Backend court-policy local response v117-v124 integration note
- V117-V124 adds bounded local response affordances to Chain 8 without changing the owner lanes. The same path remains `CourtAgendaPressureAccumulated -> PolicyWindowOpened -> PolicyImplemented -> PublicLifeAndRumor`.
- `PressCountyYamenDocument` and `RedirectRoadReport` are reused as `OfficeAndCareer` document/report continuations when structured office scalar pressure shows policy-process strain. `PostCountyNotice` and `DispatchRoadReport` remain public-life surfaces for public interpretation and notice/report visibility.
- Application projections may show `政策回应入口`, `文移续接选择`, `县门轻催`, `递报改道`, `公议降温只读回`, and `不是本户硬扛朝廷后账`, but must not compute policy success or choose outcomes from prose.
- The integration must not parse `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or projected court-policy prose.
- Unity copies projected command/readback fields only. Save/schema result: no persisted state, schema bump, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, household target field, or save-manifest change.
## Backend social mobility fidelity ring v213-v244 integration note
- V213-V244 is a rule-driven social/person movement readback loop, not an event-pool body and not a full society engine.
- `PopulationAndHouseholds` mutates only its own household livelihood, membership activity, and existing labor/marriage/migration pools. It may use `IPersonRegistryCommands.ChangeFidelityRing` to request a bounded focus-ring change for hot household members.
- `PersonRegistry` changes only `FidelityRing` and emits structured `FidelityRingChanged` metadata; it does not store population, household, livelihood, office, clan, memory, or relationship state.
- Application projections may assemble `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, and person dossier readbacks from structured queries only. They must not parse `DomainEvent.Summary`, receipt prose, movement readback text, or population summaries as authority.
- Unity copies projected fields only. Save/schema result: no persisted state, schema bump, migration, social-mobility ledger, movement ledger, focus ledger, projection cache, or save-manifest change.

## Social mobility fidelity ring closeout v245-v252 integration note
- V245-V252 adds no new integration channel. It audits the existing v213-v244 Query / Command / DomainEvent / projection path and records that the branch is closed as first-layer substrate, not as a complete society engine.
- No downstream module may treat `FidelityRingChanged`, `SettlementMobilitySnapshot`, `FidelityScaleSnapshot`, person dossier movement text, or population pool summaries as a ledger or as permission to parse prose.
- Later SocialMemory movement residue, dormant-stub demotion, migration economy, relief/class mobility, or force/office mobility hooks must consume structured owner snapshots or events and declare their own owner lane, schema impact, tests, and no-touch boundaries.
- Application and Unity remain read/projection/copy surfaces only. Save/schema result: no persisted state, schema bump, migration, movement ledger, social-mobility ledger, focus ledger, owner-lane ledger, scheduler ledger, projection cache, or save-manifest change.

## Social mobility scale-budget guard v269-v276 integration note
- V269-V276 adds no integration channel. It records that the v213-v252 fidelity substrate must scale by ring and pressure rather than by all-world person ticks.
- Future mobility consumers must choose one of four surfaces before mutating or projecting: named detail for the player household / close orbit, selective detail for the player influence footprint or active pressure, structured pools for active chain regions, and pressure summaries for distant society.
- `FidelityRingChanged`, population pools, settlement mobility summaries, and person dossier movement/fidelity readbacks are not ledgers. Later readers must not parse their prose or use them as hidden scheduler state.
- Any future expansion beyond this guard must declare owner module, hot path, expected cardinality, deterministic cap/order, no-touch boundary, schema impact, and validation lane. Save/schema result for v269-v276: no persisted state, schema bump, migration, movement/social/focus/scheduler ledger, projection cache, or save-manifest change.

## Social mobility influence readback v277-v284 integration note
- V277-V284 adds read-model fields only. `InfluenceFootprintReadbackSummary` and `ScaleBudgetReadbackSummary` explain the existing fidelity/mobility substrate without introducing a new command or event channel.
- Application may compose these strings from `PersonDossierSnapshot`, `HouseholdPressureSnapshot`, population pool snapshots, and existing `FidelityRing` values after owner modules resolve. It must not parse person dossier text, settlement mobility text, notification text, receipt prose, or `DomainEvent.Summary`.
- Unity may copy these fields into great hall, desk, and lineage surfaces only. It must not query simulation modules, compute promotion/demotion, rank movement targets, maintain ledgers, or write SocialMemory.
- Save/schema result: no persisted state, schema bump, migration, movement/social/focus/scheduler ledger, projection cache, or save-manifest change.

## Social mobility boundary closeout v285-v292 integration note
- V285-V292 adds no integration channel. It audits v213-v284 as a bounded mobility/personnel-flow substrate, not a complete social engine.
- Later modules may read structured population pools, fidelity-ring facts, settlement mobility snapshots, and person dossier snapshots as inputs only when their own owner lane, cadence, fanout, no-touch boundary, schema impact, and tests are declared.
- No downstream reader may parse `InfluenceFootprintReadbackSummary`, `ScaleBudgetReadbackSummary`, movement prose, notification prose, receipt prose, or `DomainEvent.Summary` to drive rules.
- Future personnel-flow commands must resolve in an owner module and must not be calculated by Application, UI, Unity, or a global manager.
- Save/schema result: no persisted state, schema bump, migration, movement/social/focus/scheduler/command/personnel ledger, projection cache, or save-manifest change.

## Personnel command preflight v293-v300 integration note
- V293-V300 adds no integration channel and no command route. It is a preflight gate for future personnel-flow commands.
- A future movement / migration / return / assignment / placement command must enter through `PlayerCommandCatalog` only after an owning module resolver exists. Application may route the command but must not compute outcome, target ranking, or movement success.
- `PopulationAndHouseholds` can own household migration-pressure responses; `FamilyCore` can own kin/lineage mediation; `OfficeAndCareer` can own office/document personnel channels; `WarfareCampaign` can own campaign manpower posture; `PersonRegistry` remains identity/fidelity only.
- No downstream reader may parse command labels, readback strings, person dossier prose, settlement mobility text, notification prose, receipt prose, or `DomainEvent.Summary` to drive a personnel rule.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler ledger, projection cache, or save-manifest change.

## Personnel flow command readiness v301-v308 integration note
- V301-V308 adds `PersonnelFlowReadinessSummary` to existing player-command affordance and receipt projections. It is a readback field, not an integration channel.
- The field is populated only for existing `PopulationAndHouseholds` local response command surfaces: `RestrictNightTravel`, `PoolRunnerCompensation`, and `SendHouseholdRoadMessage`.
- Application may assemble the readback from structured household snapshots and command identity. It must not parse readback prose, choose a person target, rank movement candidates, or calculate movement success.
- Unity receives the field only through the shared command ViewModel copy path.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler ledger, projection cache, or save-manifest change.

## Personnel flow surface echo v309-v316 integration note
- V309-V316 adds `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary` as a runtime surface echo over existing affordance/receipt personnel-flow readiness fields.
- The echo is assembled only from structured `PersonnelFlowReadinessSummary` fields on player-command affordances and receipts. It must not parse `ReadbackSummary`, receipt prose, notification text, mobility text, or `DomainEvent.Summary`.
- Great Hall mobility readback may append the command-surface echo. This is presentation of an already projected field, not a new integration channel or movement-resolution path.
- No module may consume this echo as authority, ledger membership, movement permission, or durable SocialMemory residue.

## Personnel flow readiness closeout v317-v324 integration note
- V317-V324 adds no integration channel. It closes v293-v316 as a first personnel-flow command-readiness layer only.
- The closed layer consists of preflight gates, existing `PopulationAndHouseholds` local-response readiness readback, a command-surface echo, and Great Hall display of projected fields.
- Future personnel-flow command work still must enter through a named owner module and must not use the v293-v316 readbacks as authority, ledger membership, movement permission, or durable SocialMemory residue.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/closeout ledger, projection cache, or save-manifest change.

## Personnel flow owner-lane gate v325-v332 integration note
- V325-V332 adds `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` as a runtime projected gate, not an integration channel.
- The gate is assembled only from structured command affordance/receipt metadata and the presence of projected personnel-flow readiness fields. It must not parse `ReadbackSummary`, receipt prose, notification text, mobility text, or `DomainEvent.Summary`.
- The current readable lane is `PopulationAndHouseholds`; `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` are named as future owner-lane plans only.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/owner-lane-gate ledger, projection cache, or save-manifest change.

## Personnel flow desk gate echo v333-v340 integration note
- V333-V340 adds no integration channel. Desk Sandbox displays `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` only when structured local public-life command affordances/receipts already carry `PersonnelFlowReadinessSummary`.
- The desk echo must use command-surface enumeration and must not parse `ReadbackSummary`, receipt prose, notification text, mobility text, public-life lines, or `DomainEvent.Summary`.
- The echo is presentation-only and must not be consumed as authority, ledger membership, movement permission, or durable SocialMemory residue.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/desk-gate ledger, projection cache, or save-manifest change.

## Personnel flow desk gate containment v341-v348 integration note
- V341-V348 adds no integration channel. A global `PersonnelFlowOwnerLaneGateSummary` is not settlement-local authority by itself.
- Desk Sandbox must keep the gate tied to structured local public-life `PersonnelFlowReadinessSummary` affordances or receipts for the same settlement.
- Quiet or distant settlements keep mobility summaries and do not inherit another settlement's local gate echo.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/desk-gate-containment ledger, projection cache, or save-manifest change.

## Personnel flow gate closeout v349-v356 integration note
- V349-V356 adds no integration channel. It closes the current personnel-flow gate visibility layer as readback only.
- The closed layer is `PersonnelFlowReadinessSummary` -> `PersonnelFlowOwnerLaneGateSummary` -> Great Hall readback -> Desk Sandbox local echo -> containment against quiet/distant node leakage.
- Future integration for Family, Office, Warfare, or broader migration/personnel movement must open a separate owner-lane plan and cannot reuse this closeout as authority.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/owner-lane-gate ledger, projection cache, or save-manifest change.

## Personnel flow future owner-lane preflight v357-v364 integration note
- V357-V364 adds no integration channel. It is a gate for future integration work only.
- Future Family/Office/Warfare personnel-flow paths must arrive as module-owned command/rule contracts with declared target scope, no-touch boundary, hot path, cardinality, deterministic ordering/cap, cadence, schema impact, and validation.
- Application may later route a declared command, but it may not infer personnel movement from the current gate, command prose, receipt prose, notification text, mobility text, public-life lines, or `DomainEvent.Summary`.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/future-owner-lane ledger, projection cache, or save-manifest change.

## Personnel flow future lane surface v365-v372 integration note
- V365-V372 adds a runtime surface readback field, not an integration channel.
- `PlayerCommandSurfaceSnapshot.PersonnelFlowFutureOwnerLanePreflightSummary` is assembled only from existing structured personnel-flow readiness affordance/receipt fields and counts.
- Great Hall may display this preflight so the player can see that Family/Office/Warfare personnel-flow lanes still require owner-module plans; no module may consume it as authority, ledger membership, movement permission, command eligibility, or durable SocialMemory residue.
- Application, UI, and Unity must not infer personnel movement from the preflight, command prose, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary`.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/future-lane-surface ledger, projection cache, or save-manifest change.

## Personnel flow future lane closeout v373-v380 integration note
- V373-V380 adds no integration channel. It closes the future-lane preflight/surface layer as evidence only.
- The closed layer is `future owner-lane contract` -> `PersonnelFlowFutureOwnerLanePreflightSummary` -> Great Hall display. It is not a command route, resolver, event consumer, ledger, or movement permission.
- Future Family/Office/Warfare personnel-flow integration must open a separate owner-lane plan and cannot consume this closeout, the surface text, command prose, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, command/movement/personnel/assignment/focus/scheduler/future-lane-closeout ledger, projection cache, or save-manifest change.

## Commoner social position preflight v381-v388 integration note
- V381-V388 adds no integration channel. It is a gate for future commoner / social-position mobility work only.
- Current readable carriers remain the existing owner surfaces: `PopulationAndHouseholds` livelihood/activity/pools, `PersonRegistry` identity/FidelityRing, and adjacent projections from education, trade, office, family, public life, and social memory.
- Future commoner status drift must open one owner lane with declared pressure carrier, target scope, hot path, cardinality, deterministic order/cap, cadence, schema impact, projection fields, and validation.
- Application, UI, and Unity must not infer promotion, demotion, zhuhu/kehu conversion, office service, trade attachment, class movement, or durable residue from person dossier labels, mobility text, public-life lines, docs text, receipt prose, notification prose, or `DomainEvent.Summary`.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/focus/scheduler ledger, projection cache, or save-manifest change.

## Commoner social position readback v389-v396 integration note
- V389-V396 adds a runtime projection field, not a new integration channel.
- `SocialPositionReadbackSummary` is assembled from existing structured person-dossier owner snapshots and copied to Unity-facing ViewModels. No module consumes it as authority.
- The readback may name `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `SocialMemoryAndRelations`, and `PersonRegistry` ownership, but it is display guidance only.
- Future commoner status drift still requires a separate owner-lane plan; no reader may parse the readback, `SocialPositionLabel`, mobility text, public-life lines, receipt prose, notification prose, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/focus/scheduler/readback ledger, projection cache, or save-manifest change.

## Social position owner-lane keys v397-v404 integration note
- V397-V404 adds a runtime projection field, not a new integration channel.
- `SocialPositionSourceModuleKeys` is assembled from existing structured person-dossier owner snapshots and copied to Unity-facing ViewModels. It is the structured reader path for the v389-v396 readback.
- Future surfaces may use the key list to show provenance or filter display, but no module may consume it as command authority, class movement, promotion/demotion, zhuhu/kehu conversion, durable residue, or route eligibility.
- Future commoner status drift still requires a separate owner-lane plan; no reader may parse the key list's sibling prose fields, mobility text, public-life lines, receipt prose, notification prose, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/source-key ledger, projection cache, or save-manifest change.

## Social position readback closeout v405-v412 integration note
- V405-V412 adds no integration channel. It closes the preflight/readback/source-key layer as evidence only.
- The closed layer is `future owner-lane contract` -> `SocialPositionReadbackSummary` -> `SocialPositionSourceModuleKeys` -> Unity copy-only display. It is not a command route, resolver, event consumer, ledger, or movement/class permission.
- Future commoner status integration must open a separate owner-lane plan and cannot consume this closeout, social-position prose, source-key display, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/closeout ledger, projection cache, or save-manifest change.

## Social position scale budget v413-v420 integration note
- V413-V420 adds a runtime projection field, not a new integration channel.
- `SocialPositionScaleBudgetReadbackSummary` is assembled from existing `FidelityRing` and structured source keys and copied to Unity-facing ViewModels.
- No module may consume the field as command authority, class movement, promotion/demotion, zhuhu/kehu conversion, precision-band mutation, durable residue, or route eligibility.
- Future commoner status integration still requires a separate owner-lane plan; no reader may parse the scale-budget text, social-position prose, source-key display, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/scale-budget ledger, projection cache, or save-manifest change.

## Social position regional scale guard v421-v428 integration note
- V421-V428 adds no integration channel and no production event or command path.
- The guard proves a registry-only `FidelityRing.Regional` dossier reads as regional summary from existing projection logic.
- No module may consume regional scale-budget text as command authority, class movement, precision mutation, durable residue, route eligibility, or target selection.
- Future commoner status integration still requires a separate owner-lane plan; no reader may parse the scale-budget text, social-position prose, source-key display, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/scale-budget ledger, projection cache, or save-manifest change.

## Social position scale closeout v429-v436 integration note
- V429-V436 adds no integration channel. It closes the preflight/readback/source-key/scale-budget/regional-guard layer as evidence only.
- The closed layer is display and validation substrate, not a command route, event consumer, resolver, ledger, selection rule, or movement/class permission.
- Future commoner status integration must open a separate owner-lane plan and cannot consume closeout prose, social-position text, source-key display, scale-budget text, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/personnel/movement/source-key/scale-budget/closeout ledger, projection cache, or save-manifest change.

## Commoner status owner-lane preflight v437-v444 integration note
- V437-V444 adds no integration channel, command, event, resolver, or consumer.
- Future commoner status integration should start in `PopulationAndHouseholds` unless a later ExecPlan explicitly proves another owner lane. The future lane must declare state, cadence, target scope, no-touch boundary, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation.
- `PersonRegistry`, Application, UI, Unity, docs text, and projection prose must not be consumed as status authority.
- Save/schema result: no persisted state, schema bump, migration, class/social-position/commoner-status/personnel/movement/owner-lane/preflight ledger, projection cache, or save-manifest change.

## Fidelity scale budget preflight v445-v452 integration note
- V445-V452 adds no integration channel, command, event, resolver, consumer, scheduler pass, or selector.
- Future detail promotion must enter through one declared owner lane and must name whether it targets named close-orbit actors, local households, active-region pools, or distant summaries.
- No module may consume `FidelityRing`, scale-budget prose, social-position prose, source-key display, receipt prose, notification text, mobility text, public-life lines, docs text, or `DomainEvent.Summary` as movement/status authority.
- Save/schema result: no persisted state, schema bump, migration, scale-budget/fidelity-budget/selector/class/social-position/commoner-status/personnel/movement ledger, projection cache, or save-manifest change.

## Household mobility dynamics explanation v453-v460 integration note

- V453-V460 adds runtime projection fields, not an integration channel, command, event consumer, resolver, scheduler pass, or selector.
- `MobilityDynamicsDimensionKeys` are selected from existing household social-pressure signals with deterministic ordering and a bounded cap. No module may consume this list as status authority, movement permission, fidelity mutation, or durable residue.
- The explanation may be displayed by Desk Sandbox as projected context for a settlement's local household pressure, but no reader may parse it, sibling mobility text, social-position prose, receipts, notification text, public-life lines, docs text, or `DomainEvent.Summary`.
- Future commoner status or migration depth still requires a separate owner-lane plan with state, cadence, target scope, hot path, touched counts, deterministic cap/order, schema impact, projection fields, and validation.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/commoner-status/class/selector ledger, projection cache, or save-manifest change.

## Household mobility dynamics closeout v461-v468 integration note

- V461-V468 is integration governance over the v453-v460 explanation fields. It adds no new Query, Command, DomainEvent, handler, scheduler pass, resolver, or selector.
- The closed layer may be read as first-layer explanation evidence only: household-pressure signals -> bounded dimension keys -> read-model summary -> Desk Sandbox copy.
- No module may consume `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, mobility prose, social-position prose, receipt text, notification text, docs text, or `DomainEvent.Summary` as movement permission, status authority, fidelity mutation, route history, target selection, or durable residue.
- Future household movement/status depth still requires a separate owner-lane plan with state, cadence, target scope, hot path, touched counts, deterministic cap/order, schema impact, projection fields, and validation.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/movement/route-history/status/class/selector/closeout ledger, projection cache, or save-manifest change.

## Household mobility owner-lane preflight v469-v476 integration note

- V469-V476 adds no integration channel, command, event, resolver, consumer, scheduler pass, selector, or route-history reader.
- Future household mobility integration should start in `PopulationAndHouseholds` unless a later ExecPlan explicitly proves another owner lane. The future lane must declare owned state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation.
- No module may consume `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, scale-budget prose, social-position prose, source-key display, receipt text, notification text, docs text, public-life lines, or `DomainEvent.Summary` as movement permission, status authority, route history, target selection, durable residue, or fidelity mutation.
- Application may route and assemble after an owner exists, but it must not calculate movement success, status drift, target eligibility, route eligibility, or durable SocialMemory residue.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/owner-lane/preflight/movement/route-history/status/class/selector ledger, projection cache, or save-manifest change.

## Household mobility preflight closeout v485-v492 integration note

- V485-V492 adds no integration channel, command, event, resolver, consumer, scheduler pass, selector, route-history reader, or SocialMemory residue path.
- The closed layer is docs/tests evidence only: household-mobility explanation -> owner-lane preflight gate -> closeout guard. It is not a command route, event consumer, resolver, ledger, selection rule, or movement permission.
- Future household mobility integration must open a separate owner-lane plan and cannot consume this closeout, `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, scale-budget prose, social-position prose, source-key display, receipt text, notification text, public-life lines, docs text, or `DomainEvent.Summary` as authority.
- Application, UI, and Unity must not infer movement success, route eligibility, status drift, target selection, fidelity promotion, or durable SocialMemory residue from the closeout.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/owner-lane/preflight/closeout/movement/route-history/status/class/selector ledger, projection cache, or save-manifest change.

## Household mobility runtime rules-data readiness v501-v508 integration note

- V501-V508 adds no Query, Command, DomainEvent, handler, scheduler pass, resolver, selector, route-history reader, rules-data loader, or SocialMemory residue path.
- The integration map says the later first runtime rule should be consumed only by `PopulationAndHouseholds`, using existing household/pool/settlement snapshots and already-owned event metadata. Application may route and assemble only after an owner rule exists.
- Future rules-data may hold thresholds, weights, regional/era modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities, but it must be owner-consumed and validated. It is not a runtime plugin system, arbitrary script surface, runtime assembly load, or reflection-heavy rule loader.
- No module may consume `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, projection prose, receipt text, public-life lines, docs text, or `DomainEvent.Summary` as movement authority.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/runtime-rule/readiness/rules-data/movement/route-history/status/class/selector ledger, projection cache, rules-data loader, or save-manifest change.

## Household mobility rules-data contract preflight v509-v516 integration note

- V509-V516 adds no Query, Command, DomainEvent, handler, scheduler pass, resolver, selector, route-history reader, rules-data loader, validator implementation, or SocialMemory residue path.
- The future contract requires stable ids, schema/version, deterministic ordering, default fallback, readable validation errors, owner-consumed use only, no UI/Application authority, and no arbitrary script/plugin execution.
- Future rules-data categories are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities.
- Because the repo has no reusable runtime rules-data/content/config pattern, this pass does not add a default file or loader. A later owner implementation must add validation before any runtime use.
- No module may consume config, validation text, fallback text, `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, projection prose, receipt text, public-life lines, docs text, or `DomainEvent.Summary` as movement authority.
- Save/schema result: no persisted state, schema bump, migration, household-mobility/runtime-rule/rules-data/contract/movement/route-history/status/class/selector ledger, projection cache, rules-data loader, validator, config file, or save-manifest change.
