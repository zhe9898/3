---
name: zongzu-ancient-china
description: Use when working on Zongzu's source-calibrated historical grounding and translating premodern Chinese society, especially Northern Song/Renzong living society, imperial-local politics, regional cultures, lineages, commoner lives, markets, exams, yamen contact, public life, warfare structures, reform pressure, imperial legitimacy, player-earned counterfactual history, map/geography calibration, named historical carriers, or dynasty-cycle pressure into a living-world simulation, desk sandbox, conflict vignette, campaign board, narrative labels, schemas, calibration bands, or anti-anachronism review.
---

# Zongzu Ancient China

## Overview

Use this skill to ground Zongzu design and implementation in historically plausible premodern Chinese contexts without pretending the game is a literal documentary simulation.
History is rules material, not a cage: named people, reforms, routes, institutions, and dates are pressure carriers, source anchors, and windows of possibility; they do not fire gameplay outcomes by themselves.

For current mainline work, assume a Northern Song / Renzong-era opening unless the repo or user says otherwise. Keep architecture open to later divergence, reform pressure, usurpation, restoration, dynasty repair, and player-earned counterfactual history through rule chains rather than free timeline editing.

Use history to sharpen:
- household, lineage, commoner, yamen, market, public-life, court, conflict, and warfare pressure
- spatialized surfaces such as great hall, lineage surface, desk sandbox, conflict vignette, and campaign board
- readable labels, source notes, regional topology, calibration bands, and anti-anachronism checks
- module ownership, save/schema impact, event metadata, projections, and acceptance tests

## Current Repo Anchors

For Renzong work, distinguish historical/design plausibility from current implementation:
- `RENZONG_PRESSURE_CHAIN_SPEC.md` is the fuller design target; `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` is the live topology ledger
- historical pressure becomes code only through module-owned state, structured events, metadata, tests, and projection/read-model surfaces
- current public-life/order and global thin-chain readback phrases such as `县门未落地`, `地方拖延`, `后账仍在`, `社会记忆读回`, `外部后账归位`, `承接入口`, `归口后读法`, `闭环防回压`, `Office承接入口`, `Family救济选择读回`, `军令选择读回`, `战后案卷读回`, `朝议压力读回`, `政策回应余味续接读回`, `政策旧账回压读回`, `政策公议旧读回`, and `Court-policy防回压` are projection carriers for local yamen friction, route/order repair, clan elder explanation, household cost, military/campaign aftermath, court-policy process, public interpretation, owner-lane return, remembered residue, and anti-loop interpretation; they must not become fixed historical event triggers
- current v35-v196 language should be treated as historically plausible pressure-carrier wording, not universal formula proof: canal-window friction can carry trade/order pressure, household burden can carry sponsor-clan pressure, Office/yamen后手 can carry document/clerk delay readback, force/campaign wording can carry mobilization and aftermath pressure, and court-policy wording can carry court-to-local process, old public reading, docket/receipt anti-misread, and remembered county-document residue without making every Song county identical
- current shell and Unity surfaces may show court, frontier, disaster, office, public-life, and campaign pressure as objects/notices/boards, but they must not resolve historical authority in UI code
- historical fidelity must stay scale-aware: use dense named actors only where pressure, player reach, or source confidence justifies it; use county/route/office pressure summaries elsewhere
- if a historical correction changes a label or descriptor only, use `zongzu-content-authoring`; if it changes a chain, use `zongzu-pressure-chain`; if it changes persistence, use `zongzu-save-and-schema`

## External Calibration Anchors

Use outside history and implementation material as calibration with confidence bands:
- Historical sources calibrate period, region, institution, route, office, and social practice; they do not by themselves prove a module formula, save field, or scripted event.
- CHGIS-style geography can discipline map/topology/provenance; it does not prove travel cost, market intensity, disaster exposure, or player visibility without a Zongzu rule path.
- CBDB-style person data can discipline named carriers, offices, kinship, postings, and networks; it does not require full-fidelity named agents everywhere.
- Performance and fidelity budgets matter to history: dense named actors belong near focus rings, player reach, pressure chains, or high-confidence carriers; distant society may remain summarized until promoted.
- Accessibility and shell guidance matter to historical presentation: period language should remain readable, narratable, and traceable to projected state.

## Use This Skill When

- adding or reviewing mechanics tied to clans, households, lineage power, commoner survival, trade, service, office, exams, literacy, schools, influence, prestige, factional pull, imperial-local bargaining, official family entanglement, tax, local order, banditry, rebellion, polity formation, dynasty rise or collapse, warfare, ritual, local custom, naming, or social status
- grounding Northern Song/Renzong-era society, Qingli-style reform pressure, Wang Anshi-style long reform aspiration, frontier pressure, imperial legitimacy, court rhythm, or famous historical carriers
- deciding how a player can plausibly bend history, seize power, restore a regime, found a polity, repair a dynasty, or get swallowed by backlash
- writing or revising Chinese-facing strings, English labels, glossary entries, descriptors, notifications, narrative beats, flavor text, or data dictionaries
- evaluating whether a proposal sounds too modern, generic-fantasy, or flattened across dynasties and regions
- mapping historical practices into module boundaries, save schemas, acceptance tests, product docs, or Unity shell surfaces
- choosing between historical framings and needing assumptions, confidence, period, region, and gameplay abstraction stated clearly
- checking a historical claim, map node, named person, reform timing, title, office, or institution against external sources before it becomes a rule or player-facing label

## Fast Lane

For quick historical checks, state period, region, confidence, gameplay abstraction, and what the source can or cannot prove. Use a full historical pass when the claim changes mechanics, topology, named carriers, map nodes, pressure chains, player-facing labels at scale, or future scenario direction.

## Workflow

1. Read project intent before importing outside history.

   Zongzu's product truth wins over raw mimicry. Start with the relevant repo docs:
   - `docs/PRODUCT_SCOPE.md`
   - `docs/SOCIAL_STRATA_AND_PATHWAYS.md`
   - `docs/HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
   - `docs/INFLUENCE_POWER_AND_FACTIONS.md`
   - `docs/CONFLICT_AND_FORCE.md`
   - `docs/MODULE_BOUNDARIES.md`
   - `docs/MODULE_INTEGRATION_RULES.md`
   - `docs/SPATIAL_SKELETON_SPEC.md`

   For Renzong and thin-chain work, also read:
   - `docs/RENZONG_PRESSURE_CHAIN_SPEC.md`
   - `docs/RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`

2. Fix the grounding frame.

   Determine, and state when relevant:
   - period or dynasty
   - region
   - social stratum
   - source type
   - confidence level
   - temporal validity: exact year, reign band, dynasty band, or broad premodern abstraction
   - abstraction level: documentary fact, scholarly interpretation, gameplay simplification, or fictionalized descriptor
   - gameplay locus: person, household, lineage, settlement, route, office, market, temple, court, campaign, realm, or background flavor
   - fidelity budget: dense agent, named stub, household/pool, settlement summary, route pressure, or realm climate

   If the task does not specify these, do not silently invent certainty. Use "premodern/imperial China inspired" or state a provisional assumption.

3. Run source calibration when external correction is requested or the claim will become code, schema, topology, or acceptance criteria.

   Use this chain:
   - repo docs and local skill references
   - [references/northern-song-source-calibration.md](references/northern-song-source-calibration.md) for Northern Song/Renzong, Qingli, Wang Anshi-style reform pressure, Song society, and source-family routing
   - [references/source-ladder.md](references/source-ladder.md)
   - [references/search-source-routing.md](references/search-source-routing.md)
   - [references/simulation-calibration.md](references/simulation-calibration.md) when timing, travel, message delay, or scale bands matter
   - authoritative source families such as CHGIS-style historical geography for map/topology, CBDB-style person data for named carriers and networks, Columbia Asia for Educators-style Song society references for social/economic baseline, Britannica-style stable biography checks for quick date/person summaries, and scholarship-facing sources for debated institutions
   - translation back into Zongzu module ownership, state pressure, projection, and player leverage

   State what a source calibrates and what it does not prove. Do not project one scale onto another: a biography database, GIS placename table, urban scroll, county gazetteer, or survey article each proves different things.

4. Load only the references you need.

   Use the catalog below to avoid broad context loading. Pull adjacent references when the prompt spans multiple layers, such as household + office + public life + conflict.

5. Translate history into playable structure.

   A useful result answers:
   - what historical/social mechanism matters
   - what module owns the stable gameplay abstraction
   - what state, pressure, command, event, metadata, projection, or surface should carry it
   - what period/region/source confidence applies
   - what player leverage is plausible
   - what actors can block, distort, bargain, misreport, exploit, or remember it
   - what becomes visible in hall, desk, notice, public-life surface, conflict vignette, or campaign board
   - what is intentionally simplified and why

## Short Prompt Expansion

Treat broad or compressed prompts as whole-skill prompts unless the user explicitly asks for a glossary definition.

For prompts like `北宋仁宗`, `庆历`, `王安石`, `皇权`, `改史`, `谋权篡位`, `复辟`, `家丁私兵与家族冲突`, `官场冲突`, `皇权与地方博弈`, `官员私心与家族`, `地方文化与教育`, `中国古代县域社会`, `史料校正`, `CHGIS`, `CBDB`, `地图`, `沙盘`, or `外部资料校正`, default to:
- set period, region, stratum, source type, and confidence
- connect court, county, household, lineage, market, public life, and force where relevant
- identify pressure locus and first affected actors
- identify player reach and backlash
- identify shell surface and module contract
- identify source uncertainty instead of overfitting false precision

## Output Rules

- Do not use history as passive lore if it should create pressure, constraint, legitimacy, cost, route friction, or public visibility.
- Do not make named historical events into fixed gameplay triggers.
- Do not create a fantasy timeline editor; large historical changes require accumulated leverage, institutions, resources, force, legitimacy, and backlash.
- Do not flatten China into generic East Asian flavor.
- Do not flatten dynasties and regions into one county texture.
- Do not model every historically interesting person, office, county, or route at full fidelity unless a pressure chain or player reach needs it.
- Do not let a capital, frontier, water-network county, inland road county, or market town prove facts for every place.
- Do not treat lineage force as a generic army.
- Do not turn every clash into a campaign board.
- Do not inject authoritative historical rules into UI-only code.
- Do not parse narrative prose as rule input.
- Do not make historical flavor override deterministic simulation, save schema, module ownership, or player comprehension.
- Prefer source-calibrated pressure carriers over lore dumps.
- Prefer node, route, posture, supply, rumor, yamen, temple, market, and lineage mechanisms over abstract map decoration.
- Prefer confidence bands and scenario assumptions over fake precision.
- Prefer source-calibrated abstraction by scale: named carriers for focal chains, summary pressure for distant institutions, and promotion hooks for later re-entry.

## Zongzu-Specific Guidance

- The authoritative simulation is not a textbook. It should be historically grounded, not enslaved to one dynasty's minutiae.
- Use history to sharpen social pressure, institutional constraint, lineage memory, and plausible pathways of advancement.
- Northern Song/Renzong is the current default opening; later divergence stays possible through rule chains.
- Qingli-like reform pressure and Wang Anshi-like reform ambition are historical-process carriers that gather fiscal, military, moral, educational, and bureaucratic tensions already present in the world.
- CHGIS-style geography should discipline topology/provenance, not prove travel cost, market intensity, disaster exposure, or player visibility by itself.
- CBDB-style person data should discipline named carriers, offices, kinship, postings, and social ties, not force fixed cutscenes.
- A stronger ancient-China game is not just "family tree plus officials." Tenants, artisans, traders, boatmen, clerks, escorts, migrants, temple brokers, and shadow survival routes should matter where in scope.
- Ancient sandbox here means spatialized political-social pressure, not a detached grand strategy map.
- Warfare-lite should read like command, mobilization, route, supply, morale, and aftermath pressure on a desk board, not unit micro.
- If a historical grounding choice implies a new subsystem, boundary, or save namespace, update repo docs in parallel.

## Reference Catalog

Source and orchestration:
- [references/northern-song-source-calibration.md](references/northern-song-source-calibration.md)
- [references/source-ladder.md](references/source-ladder.md)
- [references/search-source-routing.md](references/search-source-routing.md)
- [references/full-skill-orchestration.md](references/full-skill-orchestration.md)
- [references/era-grounding.md](references/era-grounding.md)
- [references/domain-checklists.md](references/domain-checklists.md)
- [references/first-class-system-topics.md](references/first-class-system-topics.md)
- [references/deep-dive-lenses.md](references/deep-dive-lenses.md)
- [references/topic-combination-matrix.md](references/topic-combination-matrix.md)
- [references/simulation-calibration.md](references/simulation-calibration.md)

Household, lineage, material life:
- [references/lineage-institutions-corporate-power.md](references/lineage-institutions-corporate-power.md)
- [references/lineage-inheritance.md](references/lineage-inheritance.md)
- [references/lineage-commoner-relations.md](references/lineage-commoner-relations.md)
- [references/lineage-private-force-retainers-conflict.md](references/lineage-private-force-retainers-conflict.md)
- [references/marriage-gender-household-power.md](references/marriage-gender-household-power.md)
- [references/fertility-demography-infant-mortality.md](references/fertility-demography-infant-mortality.md)
- [references/women-life-cycle-gendered-experience.md](references/women-life-cycle-gendered-experience.md)
- [references/disease-lifespan-death.md](references/disease-lifespan-death.md)
- [references/death-succession-mourning-political-shock.md](references/death-succession-mourning-political-shock.md)
- [references/childhood-generations.md](references/childhood-generations.md)
- [references/commoner-livelihoods.md](references/commoner-livelihoods.md)
- [references/property-contract-debt-credit.md](references/property-contract-debt-credit.md)
- [references/material-life-clothing-food-housing-travel.md](references/material-life-clothing-food-housing-travel.md)
- [references/medicine-healing-burial.md](references/medicine-healing-burial.md)
- [references/human-motives-social-psychology.md](references/human-motives-social-psychology.md)

County society, culture, work, public life:
- [references/late-imperial-pack.md](references/late-imperial-pack.md)
- [references/jiangnan-water-network-county.md](references/jiangnan-water-network-county.md)
- [references/north-china-road-county.md](references/north-china-road-county.md)
- [references/southwest-mountain-borderland.md](references/southwest-mountain-borderland.md)
- [references/local-culture-customs-regional-identity.md](references/local-culture-customs-regional-identity.md)
- [references/agrarian-calendar-waterworks-hazards.md](references/agrarian-calendar-waterworks-hazards.md)
- [references/public-works-granaries-canal-transport.md](references/public-works-granaries-canal-transport.md)
- [references/crafts-guilds-workshops.md](references/crafts-guilds-workshops.md)
- [references/status-groups-military-craft-households.md](references/status-groups-military-craft-households.md)
- [references/festival-folkways-entertainment.md](references/festival-folkways-entertainment.md)
- [references/religion-temples-ritual-brokerage.md](references/religion-temples-ritual-brokerage.md)
- [references/religion-state-and-popular-practice.md](references/religion-state-and-popular-practice.md)
- [references/public-opinion-reputation-public-spaces.md](references/public-opinion-reputation-public-spaces.md)
- [references/information-messaging-documents.md](references/information-messaging-documents.md)
- [references/body-desire-taboo.md](references/body-desire-taboo.md)

Power, education, office, law, imperial pressure:
- [references/influence-power-factional-pull.md](references/influence-power-factional-pull.md)
- [references/gentry-local-magnates.md](references/gentry-local-magnates.md)
- [references/household-tax-corvee.md](references/household-tax-corvee.md)
- [references/exams-offices-clerks.md](references/exams-offices-clerks.md)
- [references/education-literacy-schools-academies.md](references/education-literacy-schools-academies.md)
- [references/official-private-interest-family-entanglement.md](references/official-private-interest-family-entanglement.md)
- [references/office-conflict-faction-yamen-struggle.md](references/office-conflict-faction-yamen-struggle.md)
- [references/imperial-power-bureaucracy-and-subjects.md](references/imperial-power-bureaucracy-and-subjects.md)
- [references/imperial-sovereignty-legitimacy-succession.md](references/imperial-sovereignty-legitimacy-succession.md)
- [references/imperial-local-bargaining-governance-friction.md](references/imperial-local-bargaining-governance-friction.md)
- [references/court-eunuchs-outer-kin.md](references/court-eunuchs-outer-kin.md)
- [references/law-litigation-punishment.md](references/law-litigation-punishment.md)

Routes, conflict, warfare, dynasty-scale:
- [references/local-order-militia-banditry.md](references/local-order-militia-banditry.md)
- [references/military-governors-garrisons.md](references/military-governors-garrisons.md)
- [references/border-garrison-fort-belt.md](references/border-garrison-fort-belt.md)
- [references/secret-societies-brotherhoods.md](references/secret-societies-brotherhoods.md)
- [references/rebellion-dynasty-cycle.md](references/rebellion-dynasty-cycle.md)
- [references/disaster-famine-relief-granaries.md](references/disaster-famine-relief-granaries.md)
- [references/city-market-town-urban-life.md](references/city-market-town-urban-life.md)
- [references/frontier-migration-settlement.md](references/frontier-migration-settlement.md)
- [references/warfare-mobilization-supply-merit.md](references/warfare-mobilization-supply-merit.md)
- [references/military-thought-and-command-culture.md](references/military-thought-and-command-culture.md)
- [references/conflict-scale-ladder.md](references/conflict-scale-ladder.md)
- [references/wargame-simulation-flow.md](references/wargame-simulation-flow.md)
- [references/force-families-differentiation.md](references/force-families-differentiation.md)
- [references/small-clash-vs-major-campaign.md](references/small-clash-vs-major-campaign.md)
- [references/campaign-board.md](references/campaign-board.md)

Game translation and map/sandbox:
- [references/multi-route-play.md](references/multi-route-play.md)
- [references/route-families.md](references/route-families.md)
- [references/china-ancient-sandbox-structure.md](references/china-ancient-sandbox-structure.md)
- [references/ancient-sandbox.md](references/ancient-sandbox.md)
- [references/china-map-prefecture-county-routes.md](references/china-map-prefecture-county-routes.md)
- [references/game-translation.md](references/game-translation.md)
- [references/module-mapping.md](references/module-mapping.md)
