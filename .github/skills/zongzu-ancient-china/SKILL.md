---
name: zongzu-ancient-china
description: Use when working on Zongzu's historical grounding and translating premodern Chinese society, imperial-local politics, regional cultures, lineages, commoner lives, warfare structures, and dynastic pressure into a living-world clan simulation, desk sandbox, conflict vignette, tactical-lite encounter, campaign board, force-family differentiation, wargame flow, narrative labels, schemas, calibration bands, or anti-anachronism review. Especially useful when converting history into spatialized gameplay rather than passive background lore.
---

# Zongzu Ancient China

## Overview

This skill helps Codex ground Zongzu's design and implementation work in historically plausible premodern Chinese contexts without pretending the game is a literal documentary simulation.
Use it to turn history into game abstractions, desk-sandbox surfaces, conflict vignettes, tactical-lite encounter logic, campaign-board logic, map nodes, readable labels, power and faction grammars, and anti-anachronism checks before they leak into code, docs, narrative text, UI, or tests.

## Use This Skill When

- adding or reviewing mechanics tied to clans, households, lineage power, commoner survival, trade, service, office, exams, literacy, schools, influence, prestige, factional pull, imperial-local bargaining, official family entanglement, tax, local order, banditry, rebellion, polity formation, dynasty rise or collapse, warfare, ritual, local custom, naming, or social status
- writing or revising Chinese-facing strings, English labels, glossary entries, descriptors, notifications, narrative beats, flavor text, or data dictionaries
- evaluating whether a proposal sounds too modern, too generic-fantasy, or too flattened across dynasties
- mapping historical practices into module boundaries, save schemas, acceptance tests, or product docs
- converting history into ancient-sandbox play, map nodes, desk-board interactions, local conflict vignettes, tactical-lite encounters, warfare-lite or conflict-lite presentation, or spatial UI language
- choosing between multiple historical framings and needing a disciplined way to state assumptions and confidence

## Workflow

1. Read project intent first.

   Start with the repo's own product and architecture docs before importing outside history. Zongzu's game truth wins over raw historical mimicry.

2. Fix the grounding frame.

   Determine, and state when relevant:
   - period or dynasty
   - region
   - social stratum
   - source type
   - confidence level

   If the task does not specify these, do not silently invent certainty. Use "premodern/imperial China inspired" or state a provisional assumption.

3. Load only the references you need.

   - Read [references/source-ladder.md](references/source-ladder.md) for trusted source order and lookup strategy. Do not jump to general external search when the local skill pack already covers the topic.
   - Read [references/search-source-routing.md](references/search-source-routing.md) when the local pack is not enough and you need to choose the right external search source rather than doing broad web search first.
   - Read [references/full-skill-orchestration.md](references/full-skill-orchestration.md) when the user wants the whole skill strung together, when the prompt is broad, or when the topic clearly spans multiple layers of society at once.
   - Read [references/era-grounding.md](references/era-grounding.md) for anti-anachronism rules and confidence labeling.
   - Read [references/domain-checklists.md](references/domain-checklists.md) for domain-specific checks.
   - Read [references/first-class-system-topics.md](references/first-class-system-topics.md) when the user names any broad major theme and wants it treated as a real system topic rather than a lore leaf.
   - Read [references/deep-dive-lenses.md](references/deep-dive-lenses.md) when the user wants the skill deepened, when a design feels flat, or when you need a stronger pass across institutions, classes, public life, state reach, material flows, and failure modes.
   - Read [references/topic-combination-matrix.md](references/topic-combination-matrix.md) when you need to combine multiple files into a thicker China-ancient social slice instead of answering from one topic in isolation.
   - Read [references/late-imperial-pack.md](references/late-imperial-pack.md) when the task needs a stable late-imperial baseline for county society, yamen reach, gentry mediation, exam funnels, grain and silver pressure, and local order without pretending every dynasty is the same.
   - Read [references/jiangnan-water-network-county.md](references/jiangnan-water-network-county.md) when the task needs a strong Chinese regional feel grounded in water routes, market towns, ferries, embankments, dock life, and river-linked county society.
- Read [references/local-culture-customs-regional-identity.md](references/local-culture-customs-regional-identity.md) when the task is about local custom, dialect feel, foodways, dress norms, performance style, place pride, regional stigma, outsider friction, or how counties feel culturally different beyond terrain alone.
- Read [references/simulation-calibration.md](references/simulation-calibration.md) when the task needs believable timing, travel bands, message delay, market cadence, levy response, settlement scale, or other calibration ranges instead of fake precision.
- Read [references/influence-power-factional-pull.md](references/influence-power-factional-pull.md) when the task is about influence, prestige, office leverage, lineage standing, local magnates, factional pull, who can block whom, who can move paperwork, or how power should travel without becoming one abstract score.
   - Read [references/lineage-institutions-corporate-power.md](references/lineage-institutions-corporate-power.md) when the task is specifically about lineage institutions such as ancestral halls, lineage rules, corporate property, lineage schools, charity, mediation, discipline, or organized clan power.
   - Read [references/lineage-inheritance.md](references/lineage-inheritance.md) when the task is about family, lineage, branch households, succession, adoption, widowhood, or inheritance pressure.
   - Read [references/lineage-commoner-relations.md](references/lineage-commoner-relations.md) when the task is about how lineages interact with tenants, clients, retainers, dependents, servants, or ordinary households.
   - Read [references/lineage-private-force-retainers-conflict.md](references/lineage-private-force-retainers-conflict.md) when the task is about `jiading`, private retainers, estate guards, clan intimidation, feud escalation, or family-to-family armed conflict below formal war.
    - Read [references/marriage-gender-household-power.md](references/marriage-gender-household-power.md) when the task is about marriage, concubinage, widowhood, household authority, women's work, domestic hierarchy, or intra-household power.
    - Read [references/fertility-demography-infant-mortality.md](references/fertility-demography-infant-mortality.md) when the task is about childbirth, fertility pressure, infant survival, dependency waves, heir security, or household reproduction over time.
    - Read [references/women-life-cycle-gendered-experience.md](references/women-life-cycle-gendered-experience.md) when the task is about girlhood, marriage transfer, daughter-in-law pressure, childbearing risk, widowhood, remarriage, concubinage, or elder female authority.
    - Read [references/disease-lifespan-death.md](references/disease-lifespan-death.md) when the task is about illness, injury, disability, aging, death, funerals, epidemics, or household care burden.
    - Read [references/death-succession-mourning-political-shock.md](references/death-succession-mourning-political-shock.md) when the task is about death aftermath across household, lineage, office, command, mourning, succession, or regime shock.
    - Read [references/childhood-generations.md](references/childhood-generations.md) when the task is about childhood, upbringing, apprenticeship, sibling order, elder care, or intergenerational turnover.
    - Read [references/commoner-livelihoods.md](references/commoner-livelihoods.md) when the task is about peasants, tenants, laborers, artisans, peddlers, boatmen, migrants, servants, debt, or everyday survival.
    - Read [references/property-contract-debt-credit.md](references/property-contract-debt-credit.md) when the task is about landholding, house rights, dowry transfer, tenancy contracts, loans, credit, collateral, or debt dependency.
    - Read [references/material-life-clothing-food-housing-travel.md](references/material-life-clothing-food-housing-travel.md) when the task is about everyday material life such as clothing, food, housing, fuel, utensils, storage, ordinary travel, or the visible standard of living.
    - Read [references/agrarian-calendar-waterworks-hazards.md](references/agrarian-calendar-waterworks-hazards.md) when the task is about farming rhythms, seasonal labor peaks, irrigation, wells, canals, embankments, flood control, drought, or agricultural hazards.
    - Read [references/public-works-granaries-canal-transport.md](references/public-works-granaries-canal-transport.md) when the task is about embankments, canals, ferries, depots, granaries, official transport, dredging, or infrastructure as a political-social force.
    - Read [references/crafts-guilds-workshops.md](references/crafts-guilds-workshops.md) when the task is about craft production, workshops, apprentices, tools, guild-like coordination, town industries, or repair economies.
    - Read [references/status-groups-military-craft-households.md](references/status-groups-military-craft-households.md) when the task is about military households, craft households, hereditary service groups, status-bound obligation, or constrained mobility.
    - Read [references/medicine-healing-burial.md](references/medicine-healing-burial.md) when the task is about healers, remedies, care networks, funerals, burial, mourning, grave maintenance, or death as a social and economic process.
    - Read [references/festival-folkways-entertainment.md](references/festival-folkways-entertainment.md) when the task is about festivals, temple fairs, seasonal customs, storytelling, opera, taverns, teahouses, amusements, leisure, or recurring rhythms of ordinary life.
   - Read [references/multi-route-play.md](references/multi-route-play.md) when the task is about widening the game beyond family-only play into social routes, livelihood routes, service routes, market routes, governance routes, or mixed progression paths.
   - Read [references/route-families.md](references/route-families.md) when the task is about the game's top-level social layers such as social governance, lineage management, commoner household management, bandit management, commercial management, official promotion, or imperial governance, while keeping them dynamic rather than fixed lanes.
   - Read [references/religion-temples-ritual-brokerage.md](references/religion-temples-ritual-brokerage.md) when the task is about temples, shrines, ritual authority, vows, festivals, monks, priests, spirit legitimacy, or religious brokerage.
   - Read [references/religion-state-and-popular-practice.md](references/religion-state-and-popular-practice.md) when the task is about the relation between state cult, official recognition, local temple practice, household ritual, heterodoxy fear, or religion as part of governance.
   - Read [references/gentry-local-magnates.md](references/gentry-local-magnates.md) when the task is about gentry, local magnates, strongmen, estate blocs, patronage webs, or elite local dominance.
   - Read [references/household-tax-corvee.md](references/household-tax-corvee.md) when the task is about household registration, land, tax, labor service, transport burden, or extraction pressure.
    - Read [references/exams-offices-clerks.md](references/exams-offices-clerks.md) when the task is about study, exams, credentials, office access, clerical service, petitions, or yamen workflow.
    - Read [references/education-literacy-schools-academies.md](references/education-literacy-schools-academies.md) when the task is about literacy, lineage schools, village schooling, private tutors, academies, copying culture, book access, practical document competence, or educational mobility beyond exam rank.
    - Read [references/official-private-interest-family-entanglement.md](references/official-private-interest-family-entanglement.md) when the task is about officials' private motives, household needs, natal or affinal kin pull, gift obligation, clerk capture, patronage, or how office life tangles with family and reputation.
    - Read [references/office-conflict-faction-yamen-struggle.md](references/office-conflict-faction-yamen-struggle.md) when the task is about office rivalry, factional conflict, memorial attack, yamen infighting, docket control, clerk blocs, or how official conflict distorts governance.
    - Read [references/imperial-power-bureaucracy-and-subjects.md](references/imperial-power-bureaucracy-and-subjects.md) when the task is about emperor, court, magistrates, clerks, runners, ordinary subjects, townspeople, petition chains, or how state power is actually felt by commoners and urban residents.
    - Read [references/imperial-sovereignty-legitimacy-succession.md](references/imperial-sovereignty-legitimacy-succession.md) when the task is about emperor-centered authority, dynastic legitimacy, mandate language, reign continuity, succession clarity, amnesties, or the symbolic-political force of `huangquan`.
    - Read [references/imperial-local-bargaining-governance-friction.md](references/imperial-local-bargaining-governance-friction.md) when the task is about center-local bargaining, soft compliance, paper obedience, implementation drag, local buffering, or how imperial pressure is negotiated before it reaches the county.
    - Read [references/court-eunuchs-outer-kin.md](references/court-eunuchs-outer-kin.md) when the task is about courts, inner-palace influence, eunuchs, outer-kin, favorites, succession circles, or court capture.
   - Read [references/law-litigation-punishment.md](references/law-litigation-punishment.md) when the task is about accusations, lawsuits, mediation, punishment, prisons, yamen adjudication, or legal fear.
   - Read [references/body-desire-taboo.md](references/body-desire-taboo.md) when the task is about sexuality, scandal, vice, bodily shame, fertility anxiety, illicit intimacy, or taboo.
   - Read [references/local-order-militia-banditry.md](references/local-order-militia-banditry.md) when the task is about local security, militia activation, escorts, runners, clan shielding, or bandit pressure.
   - Read [references/military-governors-garrisons.md](references/military-governors-garrisons.md) when the task is about military governors, garrisons, regional commands, warlord-like autonomy, army settlements, or militarized provinces.
   - Read [references/border-garrison-fort-belt.md](references/border-garrison-fort-belt.md) when the task is about border zones, fort lines, passes, garrison depots, courier chains, military household camps, raid alarms, or frontier public life under alert.
   - Read [references/secret-societies-brotherhoods.md](references/secret-societies-brotherhoods.md) when the task is about sworn brotherhoods, secret societies, underground aid networks, smuggling rings, or covert organization.
   - Read [references/rebellion-dynasty-cycle.md](references/rebellion-dynasty-cycle.md) when the task is about bandit-to-rebel escalation, family militarization, regime fracture, founding a dynasty, dynastic consolidation, or dynastic collapse.
   - Read [references/disaster-famine-relief-granaries.md](references/disaster-famine-relief-granaries.md) when the task is about flood, drought, famine, relief, granaries, epidemics, displacement, or social breakdown under disaster.
   - Read [references/city-market-town-urban-life.md](references/city-market-town-urban-life.md) when the task is about county seats, prefectural cities, wards, markets, inns, workshops, urban consumption, or town life.
   - Read [references/frontier-migration-settlement.md](references/frontier-migration-settlement.md) when the task is about migration, colonization, frontier garrisons, resettlement, mixed populations, refugees, or borderland opportunity and risk.
   - Read [references/human-motives-social-psychology.md](references/human-motives-social-psychology.md) when the task is about motives, shame, honor, fear, grief, aspiration, rumor, trust, reciprocity, opportunism, crowd behavior, or believable human response under pressure.
   - Read [references/information-messaging-documents.md](references/information-messaging-documents.md) when the task is about rumor, letters, messengers, market news, yamen paperwork, proclamations, notices, or information delay.
   - Read [references/public-opinion-reputation-public-spaces.md](references/public-opinion-reputation-public-spaces.md) when the task is about reputation, street talk, crowd mood, tea-house politics, market-square pressure, temple-fair visibility, shame display, or public legitimacy.
   - Read [references/warfare-mobilization-supply-merit.md](references/warfare-mobilization-supply-merit.md) when the task is about warfare-lite, levies, supply lines, grain routes, campaign boards, or military merit.
   - Read [references/military-thought-and-command-culture.md](references/military-thought-and-command-culture.md) when the task is about bingfa, command style, timing, deception, terrain sense, intelligence, morale handling, or how military thought should influence a Chinese-ancient warfare layer without turning it into unit micro.
   - Read [references/conflict-scale-ladder.md](references/conflict-scale-ladder.md) when the task is about battle, combat, force types, local clashes, conflict presentation, skirmish resolution, or when the design should distinguish sandboxes, vignettes, tactical-lite encounters, and campaign boards.
   - Read [references/china-ancient-sandbox-structure.md](references/china-ancient-sandbox-structure.md) when the task is specifically about Chinese-ancient sandbox structure, layer composition, node grammar, route grammar, or desk read order.
   - Read [references/wargame-simulation-flow.md](references/wargame-simulation-flow.md) when the task is specifically about `bingqi tuiyan`, warfare flow, command sequence, resolution order, or step-by-step campaign logic.
   - Read [references/force-families-differentiation.md](references/force-families-differentiation.md) when the task is about distinguishing `jiabing`, `jiading`, `tuanlian`, yamen force, official detachments, rebel bands, or border/garrison force.
   - Read [references/small-clash-vs-major-campaign.md](references/small-clash-vs-major-campaign.md) when the task is about dividing local conflict from large campaign play or deciding whether a fight should stay local or escalate upward.
   - Read [references/ancient-sandbox.md](references/ancient-sandbox.md) when the task is about ancient desk-sandbox structure, social-pressure boards, settlement surfaces, node layers, or player verbs on the local map.
   - Read [references/campaign-board.md](references/campaign-board.md) when the task is about bingqi, warfare-lite boards, fronts, command posture, route pressure, or campaign-state transitions.
   - Read [references/china-map-prefecture-county-routes.md](references/china-map-prefecture-county-routes.md) when the task is about Chinese ancient map hierarchy, prefecture and county nodes, roads, ferries, canals, river links, passes, or map labeling.
   - Read [references/game-translation.md](references/game-translation.md) when translating history into playable systems, maps, desk sandboxes, or warfare-lite surfaces.
   - Read [references/module-mapping.md](references/module-mapping.md) when turning history into Zongzu module design.

3a. Treat matching prompts as auto-deep-linked by default.

   If the user gives a compressed prompt, a broad topic label, or a single domain phrase:
   - `皇权与地方博弈`
   - `官员私心与家族`
   - `宗族制度`
   - `中国古代教育`

   do not wait for them to manually name every adjacent topic.
   Do not stop at the primary topic page unless the user explicitly asks for a narrow definition, glossary, or terminology-only answer.

   Default to this pass:
   - use [references/first-class-system-topics.md](references/first-class-system-topics.md) to treat the topic as a first-class system topic
   - use [references/full-skill-orchestration.md](references/full-skill-orchestration.md) to build the whole-chain pass when the prompt is broad or compressed
   - use [references/domain-checklists.md](references/domain-checklists.md) to identify the primary section
   - use [references/deep-dive-lenses.md](references/deep-dive-lenses.md) to thicken the answer across institution, actor ladder, public surface, pressure, and failure
   - use [references/topic-combination-matrix.md](references/topic-combination-matrix.md) to pull in adjacent files
   - use [references/game-translation.md](references/game-translation.md) to translate the result into mechanics, boards, labels, or surfaces
   - use [references/module-mapping.md](references/module-mapping.md) to anchor the answer back into repo modules
   - keep walking the chain until the answer connects upstream pressure, local actors, visible surfaces, downstream consequences, and project-module ownership

   In other words: one short user sentence should usually be enough to trigger the full pass and a deeper linked read, not just a single-file summary.

3b. Treat broad world prompts as whole-skill prompts by default.

   If the user says things like:
   - `串起来`
   - `整套来`
   - `中国古代活社会`
   - `整个 skill 连起来`

   do not answer from one topic page only.

   Default to a linked pass across:
   - era and region
   - household and lineage
   - material life and debt
   - education, religion, and office
   - private force, office conflict, and disorder
   - public life, rumor, and visibility
   - death, succession, and legitimacy
   - module and UI ownership

3c. Default to chain-completion instead of topic-isolation.

   Once the skill is triggered, the model should normally keep deepening until it can answer all of these in one connected pass:
   - where the pressure starts
   - who carries it
   - how it becomes visible on the desk, map, hall, market, or conflict surface
   - what it changes next in household, office, force, legitimacy, or public life
   - which gameplay surface or module should own the abstraction

   Only stay narrow when the user explicitly asks for a narrow answer.

4. Convert history into game abstractions.

   Prefer durable institutional patterns over brittle trivia. A good result usually answers:
   - what social force is being modeled
   - which module should own it
   - what the player sees and touches in the shell
   - what the intentional abstraction is
   - what should remain flavor only

5. Surface uncertainty instead of bluffing.

   If a historical claim is era-sensitive, contested, region-bound, or based mostly on normative texts rather than practice, say so clearly.

## Output Rules

- Do not present cross-dynastic patterns as universal facts.
- Distinguish prescriptive institutions from lived practice.
- Treat offices, degree titles, tax regimes, military structures, marriage rules, and inheritance rules as highly era-sensitive.
- When the era is unspecified, prefer an explicitly labeled baseline such as `late imperial inspired` instead of bluffing exact dynasty certainty.
- Do not translate `shimin` or urban populations into modern citizenship by default. Prefer townspeople, urban residents, market populations, or subjects unless the design explicitly wants modernized framing.
- Prefer English plus pinyin or a Chinese term in docs when precision matters, for example `xiangshi` or `zongzu`.
- Do not let all meaningful agency collapse into elite lineage agency; commoner routes, clientage, labor, service, trade, migration, and coercion should remain visible.
- Do not harden the society into fixed route trees or class tracks; people and households should be able to drift, stack roles, switch position, and get pushed by pressure.
- Do not treat banditry, rebellion, or dynasty change as sudden scripted spectacle with no social buildup; show the pressure ladder.
- Do not flatten religion, law, marriage, urban life, and disaster into mere flavor. If they matter, they should alter pressure, legitimacy, mobility, or survival.
- Do not model people as pure rational maximizers. Face, fear, fatigue, grief, hope, reciprocity, imitation, habit, rumor, and opportunism should visibly shape action.
- Do not treat age, illness, death, desire, and information delay as cosmetic. They should alter attachment, decision speed, labor, and legitimacy.
- Do not let death end at burial. It should be able to open succession disputes, mourning interruptions, office vacancies, command shock, and dynastic or local legitimacy tests.
- Do not reduce births, child loss, widowhood, contract debt, hereditary service status, or public works burden to hidden background math. They should bend planning, mobility, resentment, and continuity.
- Do not treat local custom, dialect, and regional identity as cosmetic reskins. They should alter trust, outsider friction, public behavior, recruitment tone, and customary legitimacy.
- Do not collapse education into exam rank alone. Literacy, copying, account keeping, petition handling, schooling cost, and academy prestige should alter competence, status reproduction, and mobility.
- Do not treat lineage force as a generic army. Private retainers, hall guards, escorts, feud violence, and estate coercion should stay socially embedded and politically ambiguous.
- Do not force every conflict into a campaign board. Pressure, local clashes, tactical-lite encounters, and campaign-level warfare should remain distinguishable layers.
- Do not treat festivals, leisure, public talk, and crowd-visible space as decorative backdrop only. They should shape rhythm, attention, rumor, solidarity, panic, or reputation.
- Do not treat clothing, food, housing, travel, craft work, water control, healing, or burial as decorative set dressing. They should visibly affect seasonal rhythm, dignity, cost, mobility, storage, vulnerability, and memory.
- Do not reduce `huangquan` to paperwork alone. Emperor-centered rule should sometimes operate through legitimacy, succession, ritual center, amnesty tone, and symbolic confidence even when local reach is imperfect.
- Do not model emperor-to-county relation as a pure command line. Bargaining, buffering, selective enforcement, paper compliance, and local reinterpretation should remain visible.
- Do not model officials as disembodied public offices. Household need, kin pull, gift obligation, patronage, clerk dependence, and reputational cover should be allowed to bend conduct.
- Do not flatten office conflict into clean ideology or pure corruption. Procedure, accusation, clerk blocs, docket control, patron backing, and reputational war should all matter.
- Do not flatten influence into one prestige bar or flatten power into static factions. Distinguish lineage standing, favor debt, office authority, wealth pull, coercive weight, public legitimacy, and information reach.
- When the prompt is broad, produce a linked chain rather than isolated sub-answers. The answer should show how power, household pressure, material life, conflict, and public visibility connect.
- When asked to "deepen" a topic, do not stop at one file. Pull in adjacent topics and apply multiple lenses such as institutions, actor ladders, material flows, public surfaces, temporal rhythms, and failure modes.
- Treat all major topics in this skill as first-class system topics by default. Do not leave law, religion, education, office conflict, lineage coercion, local culture, influence, or warfare as disconnected leaves if the user is asking at system scale.
- For code and schemas, model the stable gameplay abstraction first and keep era-specific wording in descriptors, narrative projections, or data.
- When asked for maps, sandboxes, or warfare surfaces, prefer node, route, pressure, and posture abstractions over literal historical maps or tactical grids unless the repo explicitly asks for more detail.
- If gameplay intentionally departs from history, preserve the gameplay goal and mark the historical simplification as deliberate.

## Zongzu-Specific Guidance

- The authoritative simulation is not a textbook. It should be historically grounded, not enslaved to one dynasty's minutiae.
- Use history to sharpen social pressure, institutional constraint, lineage memory, and plausible pathways of advancement.
- Avoid "generic East Asian flavor" shortcuts. Pick concrete social mechanisms instead: household registration, lineage leverage, yamen pressure, patronage, exam bottlenecks, grain and silver constraints, escort and militia capacity, ritual obligations, commoner dependency, petty trade, hired labor, and mobility pressure.
- Use regional ecology to avoid flattening China into one landform. Water-network counties, dry inland counties, market corridors, and frontier belts should not feel interchangeable.
- Use local culture to avoid flattening China into one voice. Dialect feel, customary legitimacy, foodways, local pride, temple style, and schooling tone should help one county feel socially different from another.
- For this project, history should become playable surfaces: a great hall, lineage surface, desk sandbox, conflict vignette, and later campaign board, not a lore dump.
- A stronger ancient-China game is not just "family tree plus officials." It should leave room for tenants, artisans, traders, boatmen, clerks, escorts, migrants, and shadow survival routes to matter.
- The long arc can extend from household survival to lineage consolidation to bandit pressure, rebellion, regime formation, dynastic legitimacy, and dynastic decline.
- The social field can explicitly include governance, lineage management, household survival, bandit and shadow power, commerce, official service, and imperial rule, but these should behave like overlapping positions in a living society rather than sealed routes.
- A stronger lineage layer should also allow private coercion: retainers, `jiading`, hall guards, escort clients, feud escalation, and estate intimidation without turning every clan into a mini-state.
- A living world also needs layered elites and intermediaries: gentry, local magnates, court insiders, military strongmen, sworn groups, temple brokers, and frightened crowds.
- A living world also needs death to keep working after it happens: heirs fight, halls go quiet, offices stall, armies hesitate, and legitimacy can thin or thicken around a loss.
- A living world also reproduces itself unevenly: births, infant loss, widowhood, remarriage, dowry movement, debt, and heir anxiety should keep changing the field.
- A living world should also breathe in public: fairs, markets, temple gatherings, tea-house talk, performances, seasonal customs, posted notices, and the shame or prestige of being seen.
- A living world also needs material thickness: clothing wear, food stores, fuel strain, roof leaks, workshop smoke, embankment repair, ordinary travel burden, healer access, and funeral cost should all be able to matter.
- A stronger county layer also needs contracts, granaries, canal routes, repair summons, military or craft household obligations, and other binding structures that make state reach feel material.
- A stronger county layer also needs cultural reproduction: schoolrooms, copied texts, academy prestige, local phrasing, practical literacy, and arguments over who can afford to study.
- A stronger imperial layer is not only ministries and magistrates. It is also succession, reign continuity, dynastic naming, mercy and punishment tone, mandate confidence, and the felt distance between throne and county.
- A stronger political layer also needs imperial-local bargaining: quotas softened on the way down, memorials bent upward, local elites buffering pressure, and symbolic obedience masking selective compliance.
- A stronger office layer should include private entanglement: natal kin, affinal obligations, debt, gift exchange, household cost, clerk dependence, and the constant blur between office duty and family strategy.
- A stronger office layer should also include internal struggle: memorial attack, procedural freezing, clerk splits, faction heat, blocked promotions, and quietly poisoned administration.
- A stronger conflict stack should move from desk pressure to conflict vignette to tactical-lite encounter to campaign board only when scale and consequence justify the jump.
- A stronger whole-skill answer should be able to run from throne to county to hall to household to market to grave and back again without dropping the chain.
- Ancient sandbox here means spatialized political-social pressure, not a detached grand strategy map and not a tactical battlefield game.
- Warfare-lite and conflict-lite should read like command, mobilization, route, supply, morale, and aftermath pressure on a desk board, not like unit micro.
- Do not inject authoritative historical rules into UI-only code.
- If a historical grounding choice implies a new subsystem, boundary, or save namespace, update the repo docs in parallel.

## Example Triggers

- "Use $zongzu-ancient-china to check whether this lineage mechanic feels too modern."
- "Use $zongzu-ancient-china to rename these office and exam descriptors."
- "Use $zongzu-ancient-china to review this warfare note for anachronisms."
- "Use $zongzu-ancient-china to ground our clan memory system in plausible social practice."
- "Use $zongzu-ancient-china to turn this historical idea into a desk-sandbox map mechanic."
- "Use $zongzu-ancient-china to design a warfare-lite campaign board that still feels Chinese-ancient rather than generic wargame."
- "Use $zongzu-ancient-china to widen this from family-only play into commoner and market routes."
- "Use $zongzu-ancient-china to add ordinary households, service work, and mobility pressure to this county sandbox."
- "Use $zongzu-ancient-china to turn bandit pressure into a rebellion and dynasty-cycle design."
- "Use $zongzu-ancient-china to sketch the arc from family power to founding and losing a dynasty."
- "Use $zongzu-ancient-china to define the dynamic social layers behind family, commoner, commerce, bandit, official, and emperor play."
- "Use $zongzu-ancient-china to make this county feel like Jiangnan rather than a generic map."
- "Use $zongzu-ancient-china to contrast a north-China road county with a water-network county."
- "Use $zongzu-ancient-china to calibrate rumor, levy, and travel timing without fake precision."
- "Use $zongzu-ancient-china to string the whole skill together for a living county world."
- "Use $zongzu-ancient-china to deepen the clan system beyond inheritance into halls, rules, schools, and corporate power."
- "Use $zongzu-ancient-china to make religion feel like governance and public life rather than decoration."
- "Use $zongzu-ancient-china to give our warfare layer a Chinese-ancient command culture instead of generic tactics."
- "Use $zongzu-ancient-china to connect emperor, magistrates, clerks, commoners, and townspeople in one state-society chain."
- "Use $zongzu-ancient-china to deeply expand this topic across institutions, classes, public life, and failure modes."
- "Use $zongzu-ancient-china to deepen everyday life through clothing, food, housing, travel, and visible standards of living."
- "Use $zongzu-ancient-china to turn agrarian calendar, waterworks, workshops, healing, and burial into county pressures instead of background flavor."
- "Use $zongzu-ancient-china to split battle into local conflict vignette, tactical-lite encounter, and campaign board."
- "Use $zongzu-ancient-china to define Chinese-ancient sandbox structure."
- "Use $zongzu-ancient-china to define a wargame simulation flow."
- "Use $zongzu-ancient-china to distinguish `jiabing`, `jiading`, `tuanlian`, official force, rebel force, and border force."
- "Use $zongzu-ancient-china to separate small clashes from major campaigns."
- "Use $zongzu-ancient-china to show how one death ripples through inheritance, mourning, office vacancy, command loss, and public legitimacy."
- "Use $zongzu-ancient-china to deepen `huangquan` through succession, mandate confidence, reign change, and symbolic-political pressure."
- "Use $zongzu-ancient-china to model childbirth, infant loss, widowhood, dowry, and heir pressure as part of household continuity."
- "Use $zongzu-ancient-china to turn debt contracts, granaries, canal works, and military-household obligations into county-level gameplay pressure."
- "Use $zongzu-ancient-china to make this county feel culturally local through custom, dialect, schooling, and place pride rather than generic regional art."
- "Use $zongzu-ancient-china to deepen literacy, lineage schooling, academies, and practical document competence beyond the exam funnel."
- "Use $zongzu-ancient-china to map how imperial pressure is bargained, softened, or distorted before it reaches the county."
- "Use $zongzu-ancient-china to show how an official's family, gifts, clerks, and private needs bend public duty."
- "`家丁私兵与家族冲突`"
- "`官场冲突`"
- "`皇权与地方博弈`"
- "`官员私心与家族`"
- "`地方文化与教育`"
- "`中国古代县域社会`"

## References

- [references/source-ladder.md](references/source-ladder.md)
- [references/search-source-routing.md](references/search-source-routing.md)
- [references/full-skill-orchestration.md](references/full-skill-orchestration.md)
- [references/era-grounding.md](references/era-grounding.md)
- [references/domain-checklists.md](references/domain-checklists.md)
- [references/first-class-system-topics.md](references/first-class-system-topics.md)
- [references/deep-dive-lenses.md](references/deep-dive-lenses.md)
- [references/topic-combination-matrix.md](references/topic-combination-matrix.md)
- [references/late-imperial-pack.md](references/late-imperial-pack.md)
- [references/jiangnan-water-network-county.md](references/jiangnan-water-network-county.md)
- [references/local-culture-customs-regional-identity.md](references/local-culture-customs-regional-identity.md)
- [references/simulation-calibration.md](references/simulation-calibration.md)
- [references/influence-power-factional-pull.md](references/influence-power-factional-pull.md)
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
- [references/agrarian-calendar-waterworks-hazards.md](references/agrarian-calendar-waterworks-hazards.md)
- [references/public-works-granaries-canal-transport.md](references/public-works-granaries-canal-transport.md)
- [references/crafts-guilds-workshops.md](references/crafts-guilds-workshops.md)
- [references/status-groups-military-craft-households.md](references/status-groups-military-craft-households.md)
- [references/medicine-healing-burial.md](references/medicine-healing-burial.md)
- [references/festival-folkways-entertainment.md](references/festival-folkways-entertainment.md)
- [references/multi-route-play.md](references/multi-route-play.md)
- [references/route-families.md](references/route-families.md)
- [references/religion-temples-ritual-brokerage.md](references/religion-temples-ritual-brokerage.md)
- [references/religion-state-and-popular-practice.md](references/religion-state-and-popular-practice.md)
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
- [references/body-desire-taboo.md](references/body-desire-taboo.md)
- [references/local-order-militia-banditry.md](references/local-order-militia-banditry.md)
- [references/military-governors-garrisons.md](references/military-governors-garrisons.md)
- [references/border-garrison-fort-belt.md](references/border-garrison-fort-belt.md)
- [references/secret-societies-brotherhoods.md](references/secret-societies-brotherhoods.md)
- [references/rebellion-dynasty-cycle.md](references/rebellion-dynasty-cycle.md)
- [references/disaster-famine-relief-granaries.md](references/disaster-famine-relief-granaries.md)
- [references/city-market-town-urban-life.md](references/city-market-town-urban-life.md)
- [references/frontier-migration-settlement.md](references/frontier-migration-settlement.md)
- [references/human-motives-social-psychology.md](references/human-motives-social-psychology.md)
- [references/information-messaging-documents.md](references/information-messaging-documents.md)
- [references/public-opinion-reputation-public-spaces.md](references/public-opinion-reputation-public-spaces.md)
- [references/warfare-mobilization-supply-merit.md](references/warfare-mobilization-supply-merit.md)
- [references/military-thought-and-command-culture.md](references/military-thought-and-command-culture.md)
- [references/conflict-scale-ladder.md](references/conflict-scale-ladder.md)
- [references/china-ancient-sandbox-structure.md](references/china-ancient-sandbox-structure.md)
- [references/wargame-simulation-flow.md](references/wargame-simulation-flow.md)
- [references/force-families-differentiation.md](references/force-families-differentiation.md)
- [references/small-clash-vs-major-campaign.md](references/small-clash-vs-major-campaign.md)
- [references/ancient-sandbox.md](references/ancient-sandbox.md)
- [references/campaign-board.md](references/campaign-board.md)
- [references/china-map-prefecture-county-routes.md](references/china-map-prefecture-county-routes.md)
- [references/game-translation.md](references/game-translation.md)
- [references/module-mapping.md](references/module-mapping.md)
