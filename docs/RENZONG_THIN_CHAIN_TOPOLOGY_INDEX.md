# RENZONG_THIN_CHAIN_TOPOLOGY_INDEX

This document freezes the current Renzong pressure-chain thin-slice topology.

It does not replace `RENZONG_PRESSURE_CHAIN_SPEC.md`. The spec remains the fuller design target. This index records what is actually wired as a thin live chain today, which scope each chain is allowed to touch, what keeps it from repeating, and which tests prove the current slice.

Use this file before adding rule density. If a future change deepens a chain, update this index in the same PR.

## Reading Rule

Each thin chain must answer:
- source pressure and owning module
- event path across modules
- pressure locus: global, court, regime, settlement, route, household, clan, person, office, or campaign
- same-month drain or explicit delayed-month behavior
- watermark, edge rule, or cadence that prevents accidental repetition
- off-scope boundary, when the pressure has a concrete locus
- downstream receipt or projection surface
- full-chain debt that remains intentionally unimplemented

The index protects the rule that Zongzu is rules-driven, not event-pool driven. `DomainEvent` records and transfers resolved facts; it is not the source of gameplay by itself.

## Topology Map

```mermaid
flowchart LR
    WS["WorldSettlements"] -->|"TaxSeasonOpened"| POP["PopulationAndHouseholds"]
    POP -->|"HouseholdDebtSpiked"| OFFICE["OfficeAndCareer"]
    OFFICE -->|"YamenOverloaded"| PUBLIC["PublicLifeAndRumor"]

    WS -->|"SeasonPhaseAdvanced(Harvest)"| TRADE["TradeAndIndustry"]
    TRADE -->|"GrainPriceSpike"| POP

    EDU["EducationAndExams"] -->|"ExamPassed"| FAMILY["FamilyCore"]

    WS -->|"ImperialRhythmChanged(amnesty axis)"| OFFICE
    OFFICE -->|"AmnestyApplied"| ORDER["OrderAndBanditry"]
    ORDER -->|"DisorderSpike"| PUBLIC

    WS -->|"FrontierStrainEscalated"| OFFICE
    OFFICE -->|"OfficialSupplyRequisition"| POP

    WS -->|"DisasterDeclared"| ORDER
    ORDER -->|"DisorderSpike"| PUBLIC

    OFFICE -->|"ClerkCaptureDeepened"| PUBLIC

    WS -->|"CourtAgendaPressureAccumulated"| OFFICE
    OFFICE -->|"PolicyWindowOpened"| OFFICE

    WS -->|"RegimeLegitimacyShifted"| OFFICE
    OFFICE -->|"OfficeDefected"| OFFICE
```

## Thin-Chain Ledger

| Chain | Current thin path | Locus | Same-month? | Repetition guard | Receipt / projection | Current proof |
|---|---|---|---|---|---|---|
| 1. Tax/corvee household-yamen-public | `WorldSettlements.TaxSeasonOpened -> HouseholdDebtSpiked -> OfficeAndCareer.YamenOverloaded -> PublicLife heat` | symbolic tax-season source today; handler accepts settlement scope and mutates household debt by household-owned exposure profile | yes, bounded scheduler drain | scheduler fresh-event watermark; full precise tax locus still deferred | `HouseholdDebtSpiked` with tax-profile metadata, then `YamenOverloaded` plus public-life street-talk heat | `RenzongPressureChainTests.Chain1_RealMonthlyScheduler_DrainsTaxSeasonIntoYamenAndPublicLife`; `TaxSeasonBurdenHandlerTests` |
| 2. Harvest/grain household pressure | `SeasonPhaseAdvanced(Harvest) -> TradeAndIndustry.GrainPriceSpike -> HouseholdSubsistencePressureChanged` | settlement market / household | yes, bounded scheduler drain | harvest phase edge plus local market event; off-scope household assertion | `GrainPriceSpike` carries grain-market metadata; `HouseholdSubsistencePressureChanged` carries household subsistence-profile metadata | `RenzongPressureChainTests.Chain2_RealMonthlyScheduler_DrainsHarvestPriceIntoLocalHouseholdPressure`; `GrainPriceSubsistenceHandlerTests` |
| 3. Exam prestige | `EducationAndExams.ExamPassed -> FamilyCore.ClanPrestigeAdjusted` | person to clan | yes, bounded scheduler drain | exam cadence and one exam result | `ExamPassed` carries credential metadata; `ClanPrestigeAdjusted` carries exam-prestige profile metadata | `ExamPrestigeChainTests.ExamPass_ThinChain_RealScheduler_DrainsIntoClanPrestige`; `ExamResultHandlerTests` |
| 4. Imperial amnesty disorder | `ImperialRhythmChanged(amnesty axis) -> OfficeAndCareer.AmnestyApplied -> OrderAndBanditry.DisorderSpike` | imperial signal allocated to settlement jurisdiction | yes, bounded scheduler drain | office-owned `LastAppliedAmnestyWave`; handler must respect amnesty axis rather than any imperial change | `AmnestyApplied` carries office execution metadata; `DisorderSpike` carries amnesty-disorder profile metadata | `ImperialAmnestyDisorderChainTests.ImperialAmnesty_ThinChain_RealScheduler_DrainsIntoDisorderSpike`; `AmnestyDispatchHandlerTests`; `AmnestyDisorderHandlerTests` |
| 5. Frontier supply household burden | `WorldSettlements.FrontierStrainEscalated -> OfficeAndCareer.OfficialSupplyRequisition -> PopulationAndHouseholds.HouseholdBurdenIncreased` | settlement | yes, bounded scheduler drain | world-owned `LastDeclaredFrontierStrainBand`; current scalar is thin-slice only | `OfficialSupplyRequisition` carries office execution profile metadata; `HouseholdBurdenIncreased` carries household burden-profile metadata | `FrontierSupplyHouseholdChainTests` plus office/population handler tests |
| 6. Disaster disorder public life | `WorldSettlements.DisasterDeclared -> OrderAndBanditry.DisorderSpike -> PublicLife heat` | settlement disaster | yes, bounded scheduler drain | world-owned `LastDeclaredFloodDisasterBand`; metadata-only rule handling | `DisorderSpike` carries disaster-disorder profile metadata; PublicLife projects cause-aware heat | `DisasterDisorderPublicLifeChainTests` plus disaster handler tests |
| 7. Clerk capture public life | `OfficeAndCareer.ClerkCaptureDeepened -> PublicLife heat` | settlement jurisdiction | yes, bounded scheduler drain | office-owned `ActiveClerkCaptureSettlementIds`; clears only when condition falls | `ClerkCaptureDeepened` carries office profile metadata; PublicLife heat scales from that profile | `OfficeCourtRegimePressureChainTests.Chain7_RealScheduler_ClerkCaptureDrainsIntoScopedPublicLifeAndDoesNotRepeat` plus office/public-life handler tests |
| 8. Court agenda policy window | `WorldSettlements.CourtAgendaPressureAccumulated -> OfficeAndCareer.PolicyWindowOpened` | court/global source allocated to one jurisdiction | yes, bounded scheduler handling | allocation rule opens exactly one selected court-facing jurisdiction in the thin slice; local drag can absorb the signal | `PolicyWindowOpened` carries office-owned policy-window profile metadata | `OfficeCourtRegimePressureChainTests.Chain8_RealScheduler_CourtAgendaOpensOnlyOneCourtFacingWindow`; `Chain789OfficePressureHandlerTests` |
| 9. Regime legitimacy office defection | `WorldSettlements.RegimeLegitimacyShifted -> OfficeAndCareer.OfficeDefected` | regime/global source allocated to one highest-risk official | yes, bounded scheduler handling | risk allocation chooses one official; default `MandateConfidence = 70` prevents unseeded crisis | office-owned defection receipt after appointment mutation | `OfficeCourtRegimePressureChainTests.Chain9_RealScheduler_RegimePressureDefectsOnlyOneHighRiskOfficial` |

## Freeze Rules

1. Thin slice does not mean full chain. Every row above is a proof of topology and boundary, not the full social formula.
2. A broad pressure must choose a first local locus before mutating local state. Frontier, court, regime, disaster, and imperial rhythm may start wide, but they may not touch every household or jurisdiction by accident.
3. Persistent pressure requires an edge, watermark, processed band, or explicit recurring-demand cadence. A high current value alone is not a new escalation event.
4. Same-month follow-on effects must pass through the bounded scheduler drain and process only fresh events. If a future link cannot finish inside the cap, carry traceable state into the next month.
5. Every emitted event must be listed in `PublishedEvents`; every handled event must be listed in `ConsumedEvents`; every cross-module event name must come from `Zongzu.Contracts`.
6. Concrete-locus handlers must filter before mutation and tests must include a comparable off-scope negative case.
7. Rule density must consume existing owner-state dimensions before inventing a second rule layer. For household pressure, prefer `Livelihood`, land, grain, labor, dependents, debt, distress, and migration fields owned by `PopulationAndHouseholds`.
8. Generic downstream events must either carry cause metadata or keep projection wording cause-neutral.
9. Projection may explain why-now and what-next, but it may not become a second authority layer.
10. Application services may route commands and compose read models, but they may not absorb chain rules while the owning modules are still being shaped.

## Full-Chain Debt

The thin topology leaves these fuller branches intentionally unimplemented:

- Chain 1: precise zhuhu / kehu household grade, tax kind, tenant/client rent cascade, cash squeeze into markets, long memory, precise jurisdiction payloads. The current handler already uses a multi-dimensional proxy profile from existing household state (`Livelihood`, `LandHolding`, `GrainStore`, `LaborCapacity`, `DependentCount`, `DebtPressure`, `Distress`) but it is not yet a full tax/corvee society formula.
- Chain 2: formal yield ratio, granary security, route risk, disaster relief, migration, death pressure, famine narrative residue. The current handler already uses a first household-owned subsistence profile from grain price/current supply-demand metadata plus existing household dimensions (`Livelihood`, `GrainStore`, `LaborCapacity`, `DependentCount`, `DebtPressure`, `Distress`, `MigrationRisk`) but it is not yet a full grain/famine society formula.
- Chain 3: office waiting list, recommendation, favor/shame memory, public-life exam projection, failure and study-abandon branches. The current handler already uses a first credential-prestige profile from exam metadata (`ExamTier`, score, academy prestige, stress) plus existing clan/person state (`Prestige`, `MarriageAlliancePressure`, heir/branch role, adult unmarried status), but it is not yet the full education-office-public-life chain.
- Chain 4: mourning, succession, appointment rhythm, dispatch wording, public legitimacy, and non-amnesty imperial axes. The current handler already uses a first amnesty-disorder profile from office execution metadata (`AmnestyWave`, authority tier, jurisdiction leverage, clerk dependence, petition backlog, administrative task load) plus order-owned local state (`DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, `RoutePressure`, suppression buffers), but it is not yet the full imperial-local political chain.
- Chain 5: frontier sectors, WarfareCampaign mobilization, ConflictAndForce readiness, market diversion, formal quota formulas, public-life military burden, and long memory residue. The current handler already uses a first office-to-household profile from frontier metadata plus jurisdiction pressure (`AuthorityTier`, leverage, clerk dependence, backlog, administrative task load) and household-owned dimensions (`Livelihood`, `LandHolding`, `GrainStore`, `ToolCondition`, `ShelterQuality`, `LaborCapacity`, `DependentCount`, `LaborerCount`, `DebtPressure`, `Distress`, `MigrationRisk`), but it is not yet the full frontier/war economy chain.
- Chain 6: relief decisions, household subsistence/migration, market panic, route insecurity, SocialMemory disaster residue, public legitimacy. The current handler already uses a first order-owned disaster-disorder profile from disaster metadata (`severity`, `floodRisk`, `embankmentStrain`) plus local order soil (`DisorderPressure`, `BanditThreat`, `RoutePressure`, `BlackRoutePressure`, `CoercionRisk`, `RetaliationRisk`, `ImplementationDrag`) and suppression buffers, but it is not yet the full disaster-relief society chain.
- Chain 7: official evaluation, memorial attacks, petition delay, trade dispute delay, clerk factions, recommended clerk / shiye intervention. The current handler already uses a first office-to-public profile from `ClerkDependence`, `PetitionBacklog`, `AdministrativeTaskLoad`, `PetitionPressure`, authority tier, and jurisdiction leverage, but it is not yet the full official-clerk-execution chain.
- Chain 8: court process state, policy wording, appointment slate, dispatch arrival, local implementation drag, policy capture, downstream household/market/public consequences. The current handler already uses a first office-owned policy-window profile from mandate deficit, authority tier, jurisdiction leverage, petition signal, administrative task load, clerk dependence, and petition backlog, but it is not yet the full court-agenda/policy-dispatch chain.
- Chain 9: regime recognition, grain-route control, household compliance, public legitimacy, ritual claim, force backing, rebellion-to-polity and dynasty-cycle consequences.

## Next Thickening Priority

After this freeze, deepen rule density in this order unless an explicit ExecPlan says otherwise:

1. household tax/grain burden and family lifecycle pressure, because these make ordinary households and lineages breathe before higher politics becomes playable;
2. office/yamen implementation drag, because court and frontier pressure must arrive through documents, clerks, and local bargaining rather than direct imperial commands;
3. public-life and map projection, because the player must see where pressure landed before issuing bounded commands;
4. force/campaign and regime depth, only after local burden, legitimacy, and office execution are readable.
