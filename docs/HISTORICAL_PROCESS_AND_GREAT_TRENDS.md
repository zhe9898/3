# HISTORICAL_PROCESS_AND_GREAT_TRENDS

This document defines how named historical figures, reforms, wars, disasters, policy debates, and dynasty-scale turns enter Zongzu's living society.

Read this together with:
- `PRODUCT_SCOPE.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `MODULE_INTEGRATION_RULES.md`

## Grounding Stack

Use this source ladder when turning history into simulation rules:
- repo product truths and module boundaries first
- local Zongzu skills for design discipline, historical framing, and anti-anachronism checks
- public primary / reference tools for people, places, titles, terms, and dates, such as CBDB, CHGIS, dictionaries, and source-text tools
- outside scholarship and educational references for larger institutional patterns

Every historically grounded trend should record:
- period and reign context
- region or administrative scope
- social stratum touched
- source confidence
- which part is historical claim, design abstraction, or deliberate counterfactual

Do not treat a single outside source as a complete ruleset.
Use external material to calibrate pressure, vocabulary, and institutional direction, then translate it back into Zongzu's deterministic module grammar.

## Core Rule

Zongzu is not an ahistorical blank sandbox.
It should contain historical process, historical pressure, and historically grounded figures.

But historical process must not become a rigid event rail.
It enters the simulation as:
- great-trend pressure
- named-person potential
- institutional windows
- policy packages
- factional alignments
- local implementation
- backlash and memory

The world should be able to ask:
- what pressure made this trend possible
- who carries the trend
- which institutions open or block it
- how it reaches local households
- who profits, bends, suffers, resists, or captures it
- what remains after the moment passes

## Changing History Is In Scope

Historical grounding does not mean the timeline is locked.

The player may eventually change history at major scale:
- alter a local policy's lived reputation
- shift a faction's county or regional base
- save, ruin, or redirect a household or lineage that would otherwise disappear
- help a reform land differently
- back a local strongman, office bloc, rebel coalition, or restoration project
- move from local protection into armed autonomy
- support or obstruct rebellion, polity formation, or dynastic consolidation
- participate in court-facing struggle, succession politics, usurpation, or regime repair when later systems support that scale

The rule is not "history cannot change."
The rule is "history cannot be changed by naked menu authority."

History changes through accumulated pressure, actor carriers, institutions, force, legitimacy, logistics, public belief, and memory.
If the player has enough reach and pays the cost, the player can become one of the people through whom history turns.

## Renzong-Era Scenario Anchors

The current Northern Song opening should read as Renzong-era society, not as a generic imperial-China skin.

**Default temporal setting:** The MVP opens in the **Northern Song Renzong reign (1022–1063)**. This is before Wang Anshi's New Policies (1069+) and before the *baojia* militia system. The social grammar is: examination culture mature but not yet dominant over all other paths; commercial life dense but not yet fully monetized; frontier strain constant but military reform still debated; local order maintained through yamen and informal arrangement rather than structured militia.

**Default player stratum:** A **middling landed lineage (zhuhu, roughly 三等户 to 二等户)** in a single county. High enough to matter—tax obligations, corvée liability, marriage networks, academy access, yamen contact—but not so high as to begin with direct office authority.

**Default region:** Either a **north-China road-county** (旱路县, dry-land, cart-corridor, well-post society) or a **Jiangnan water-network county** (江南水网县, canal-dependent, ferry-rich, market-town society). The current seed world uses Jiangnan (兰溪) for its richer route topology.

Grounded anchors:
- Renzong reign legitimacy is strong enough that imperial pressure should exist as moral, bureaucratic, fiscal, ritual, and appointment gravity, not as a player-owned button.
- Imperial gravity should be embodied in local carriers: edict scrolls, county-gate postings, yamen documents, tax / corvee language, relief or amnesty proclamations, mourning interruptions, appointment atmospheres, border dispatches, and public legitimacy talk.
- Scholar-official and examination culture is a major ladder of ambition and public criticism, but it is costly, selective, and tied to family resources, writing, informal patronage ties, office access, and reputation. The Tang-era formal recommendation network (公荐) was largely abolished by Song early dynastic practice; advancement flows through exam success, yamen attachment, and social connection rather than through a structured "recommendation system."
- Commercial and urban life is already dense enough for cash, shops, transport, credit, street talk, service work, and city poverty to matter outside elite lineages. The economy is partially monetized: tax is collected in grain (二税) but commercial activity generates cash flow, credit need, and price volatility.
- Frontier strain against Liao / Western Xia and the cost of military institutions should exist as fiscal, supply, recruitment, office, and public-legitimacy pressure even when the player is far from the border. The Northern Song military relies on professional forces (禁军 / 厢军) and frontier garrisons, not on a general militia levy.
- Qingli reform pressure can exist in or near the opening horizon through Fan Zhongyan, Ouyang Xiu, Han Qi, Fu Bi, remonstrance culture, frontier experience, administrative cleanup, education reform, land/tax anxiety, and anti-corruption pressure. Note: Qingli reform included discussion of local security measures, but the *baojia* system as formal militia-organization was a later Wang Anshi innovation.
- Wang Anshi's later New Policies must not be an opening default. In a Renzong start, Wang can exist as future potential, local administrative experience, writings, policy reputation, and accumulated fiscal-social pressure that may later find a Shenzong-era court window.
- **Inner-court pressure (eunuchs / imperial in-laws):** Present but indirect at county level. Eunuch influence and consort-family power operate through appointment networks, fiscal leakage, and information distortion reaching local yamen. They should not appear as direct player-facing characters in the MVP, but their presence may explain why some imperial edicts arrive late, some appointments seem inexplicable, or some tax demands feel disconnected from local capacity.
- **Household registration reality:** The zhuhu / kehu distinction is structural. A Renzong-era county contains both tax-bearing landowners and tenant farmers. The proportion varies by region and disaster history. This distinction shapes corvée liability, tax contact, and social mobility pathways.

Opening scenario rule:
do not spawn later institutions as if they already dominate the county.
Represent them first as pressure, debate, personnel reputation, fiscal stress, paperwork experiments, and public rumor.

## Great Trends Are Not Event Cards

Do not implement a major historical turn as a single timed notification.

Bad shape:
- "1069: New Policies event fires; all counties change."

Better shape:
- fiscal and military pressure accumulates
- reform-minded officials gain reputation and enemies
- court trust, emperor temperament, and faction heat create an opening
- policies enter through office, market, household, military, and public-life chains
- local actors implement, distort, slow, exploit, resist, or invoke those policies
- later reversals leave memories, debts, faction labels, altered institutions, and social resentment

The event is the visible crest.
The game should simulate the pressure wave beneath it.

## Trend Packet Shape

A great trend should be shaped as a deterministic packet before it becomes presentation text.

Recommended design grammar:
- `TrendPressure`: fiscal, military, demographic, market, educational, legitimacy, disaster, or disorder pressure.
- `ActorCarrier`: named or unnamed people who make the pressure legible through writings, office, reputation, faction, household interest, or local memory.
- `InstitutionalWindow`: emperor trust, appointment opening, office conflict, local vacancy, crisis, examination taste, frontier emergency, or disaster-relief need.
- `PolicyBundle`: loans, tax classification, school reform, militia / security grouping, corvee substitution, official purchasing, granary action, appointment rules, clerk supervision, or relief measures.
- `LocalImplementation`: how magistrates, clerks, lineages, merchants, schools, temples, households, and force holders actually carry it out.
- `CaptureAndResistance`: delay, paper compliance, local rent-seeking, elite obstruction, commoner evasion, factional denunciation, or rumor.
- `Residue`: memories, debts, shame, faction labels, altered expectations, grudges, precedent, and institutional scars.

These names are design grammar, not required schema names.
Do not add saved state for them until an owning module or feature pack is specified.

## Rule-Driven Translation

Historical process must become rules before it becomes runtime content.

A named person, reform, war, edict, famine, faction struggle, or court turn may enter play only when its local effects can be translated into:
- owned module state
- declared cadence
- deterministic pressure or lookup table
- cross-module query / command / domain-event boundary
- structured diff
- projection and cause trace
- bounded player intervention, if the player's influence circle can reach it
- residue in social memory, public legitimacy, office record, household condition, or market / military state

Do not create a hidden "history script" that directly changes many modules.
Do not let a year number fire a global stat modifier.
Do not let a famous name bypass the same rules that ordinary households, clerks, merchants, lineages, and soldiers must obey.
Do not block regime-scale counterfactuals just because they are large.
Large historical changes are valid when the required pressure chain, owning modules, influence costs, legitimacy tests, and save-compatible state exist.

Correct shape:

`historical pressure -> module-owned rule -> local implementation -> diff / event -> projection -> bounded response -> remembered residue`

For example, Wang Anshi's later New Policies should not be implemented as a single `1069NewPolicies` event.
They should be a bundle of rule pressures touching loans, tax survey, service substitution, purchasing, village security, schools, clerk supervision, court trust, local implementation drag, public backlash, and faction memory.

## Historical Figures As Carriers Of Pressure

Named historical figures should not be passive encyclopedia entries.
They are carriers of accumulated pressure.

A figure may carry:
- policy theory
- patronage network
- literary and moral reputation
- administrative experience
- factional enemies
- court access
- local implementation memory
- symbolic charge in public talk

Example direction:
- Fan Zhongyan can carry Qingli reform pressure, frontier experience, literati responsibility, and administrative-cleanup ambition.
- Ouyang Xiu can carry literary prestige, remonstrance culture, scholar-official public opinion, and exam / writing taste.
- Wang Anshi can carry long-accumulating fiscal, administrative, military, and social reform pressure before he ever becomes a policy package.
- Sima Guang can carry conservative institutional memory, moral opposition, and anti-reform legitimacy.

Named figures should be able to matter before and after their most famous year.
They should have a build-up, a window, a struggle, and an afterlife.

## Wang Anshi Pattern

Use Wang Anshi as the model for "great trend carried by a person."

He should not enter the game only as a later switch called `NewPolicies`.
The design grammar should support:
- early local official experience
- essays, memorials, reputation, and policy theory
- court awareness without immediate adoption
- fiscal exhaustion, frontier pressure, bureaucratic inefficiency, household debt, and land/tax strain as upstream pressure
- an emperor or court window that can finally amplify the person
- policy bundles that hit different modules differently
- local execution by clerks, magistrates, lineages, markets, and households
- backlash from conservative officials, local elites, affected households, implementation drag, and public rumor
- lasting faction labels and institutional memory even after reversal

This is what "a person carrying decades of the realm's wish" means in system terms.
The person is not only a character; the person is where many pressures become legible.

## Player And Great Trends

The player can be swept up by great trends.
The player can also help carry, bend, accelerate, delay, localize, distort, or resist a trend.

Player influence should depend on the current influence circle:
- household reach may decide whether a family borrows, studies, flees, resists, or takes advantage of a policy
- lineage reach may shelter clients, mobilize reputation, block implementation, provide students, or enforce compliance
- market reach may exploit price changes, credit schemes, transport demand, or official purchasing
- education reach may push a youth into a new factional or examination opportunity
- yamen reach may shape paperwork timing, petition handling, tax classification, or local enforcement
- public-life reach may spread legitimacy, shame, rumor, ritual interpretation, or moral opposition
- force reach may protect routes, fulfill military demand, suppress disorder, or create coercive backlash
- court reach, if later unlocked, may affect policy framing, appointments, remonstrance, factional alignment, or imperial attention

The player must not directly rewrite history by menu.
But the player may become one of the people through whom history reaches the county, the region, or eventually the throne-facing field.
At higher influence, the player may carry, bend, accelerate, delay, localize, distort, or resist a trend.
Each verb must resolve through a real leverage channel, not a free timeline-edit action.

## Counterfactual Elasticity

Historical process should have different elasticity at different scales.

High elasticity:
- local implementation details
- household outcomes
- who benefits or suffers locally
- local faction labels and reputations
- individual career trajectories below the highest historical anchors
- whether a policy looks merciful, predatory, delayed, captured, or effective in a given county

Medium elasticity:
- timing and strength of regional adoption
- rise of secondary figures
- local rebellion, petition surges, market shifts, and public legitimacy
- whether a reform trend gains extra support, stalls early, or mutates in local practice

Low elasticity by default:
- major dynasty framework at scenario start
- existence and broad historical direction of major named figures
- large interstate pressures such as Liao / Western Xia / Song frontier strain
- the broad reality that court politics, literati factions, fiscal strain, and military burden exist

Low elasticity does not mean impossible to alter.
It means protected at scenario start and expensive to move.
Alteration requires explicit late-game depth, high influence, accumulated legitimacy or coercive capacity, clear causal path, and save-compatible systems.

Valid high-scale counterfactuals may include:
- a regional rebellion becoming durable governance rather than being suppressed
- a military / office coalition shifting succession outcome
- a lineage or player-backed bloc becoming a decisive faction carrier
- a reform coalition surviving longer or failing earlier
- a local polity forming from prolonged disorder and legitimacy vacuum
- a throne-facing usurpation, restoration, or regime repair arc in a later imperial / dynasty-cycle pack

## Localizing History

A historical trend becomes playable only when it touches local surfaces.

A great trend should be translated into:
- household pressure
- lineage choice
- market movement
- exam or appointment pressure
- yamen paperwork
- public-life rumor or ritual legitimacy
- military supply or recruitment pressure
- gray-disorder opportunity or suppression
- memory and faction residue

If a trend cannot be localized, keep it as distant context until the required module depth exists.

## Policy-To-Module Translation

Use outside history as pressure material, then route it through existing module boundaries.

| Historical pressure | Local gameplay translation | Likely owning / projecting modules |
| --- | --- | --- |
| Qingli reform pressure | anti-corruption talk, exam / school adjustment pressure, militia debate, land and labor anxiety, local official evaluation | `OfficeAndCareer`, `EducationAndExams`, `WorldSettlements`, `PopulationAndHouseholds`, `PublicLifeAndRumor`, `SocialMemoryAndRelations` |
| Wang Anshi New Policies pressure | crop loans, tax survey, official purchasing, hired-service substitution, village security grouping, school / exam changes, clerk supervision | `OfficeAndCareer`, `TradeAndIndustry`, `PopulationAndHouseholds`, `WorldSettlements`, `OrderAndBanditry`, `EducationAndExams`, `PublicLifeAndRumor` |
| Imperial legitimacy and state ritual | edict visibility, ritual order, amnesty / relief framing, moral criticism, heaven-and-people legitimacy language | `PublicLifeAndRumor`, `OfficeAndCareer`, `NarrativeProjection`, `SocialMemoryAndRelations` |
| Frontier and military burden | recruitment pressure, grain transport, route exposure, veteran return, tax strain, campaign rumor, militia / security pressure | `ConflictAndForce`, `WarfareCampaign`, `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `PublicLifeAndRumor` |
| Commercialization and urban life | cash need, shop work, credit, transport demand, public poverty, opportunity outside lineage control | `TradeAndIndustry`, `PopulationAndHouseholds`, `WorldSettlements`, `PublicLifeAndRumor`, `OrderAndBanditry` |
| Scholar-official culture | study cost, exam hope, writing reputation, remonstrance, patronage, factional talk, failed-scholar side paths | `EducationAndExams`, `OfficeAndCareer`, `FamilyCore`, `SocialMemoryAndRelations`, `PublicLifeAndRumor` |
| Rebellion / polity formation | protection failure, rebel governance, office defection, grain-route control, legitimacy claim, recognition struggle | `OrderAndBanditry`, `ConflictAndForce`, `WarfareCampaign`, `OfficeAndCareer`, `WorldSettlements`, `PublicLifeAndRumor`, `SocialMemoryAndRelations` |
| Succession / usurpation / regime repair | court-time disruption, succession uncertainty, appointment fracture, faction coalition, ritual claim, force backing, amnesty or purge | future `CourtAndThrone` / `WorldEvents` pack, plus `OfficeAndCareer`, `WarfareCampaign`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, `NarrativeProjection` |

The table is intentionally cross-module.
No single "history module" should directly mutate all of these states.

## Implementation Depth Bands

Use depth bands so history can enter early without pretending the full court-and-empire simulation already exists.

Absent:
- trend is only background prose in design docs
- no runtime projection

Lite:
- trend appears as read-only pressure, notice guidance, and module-owned deltas
- no named-person command surface
- no player timeline editing

MVP-compatible:
- trend can affect local households, prices, education pressure, yamen workload, public talk, military burden, and social memory through existing modules
- player can respond only through current influence circles
- all outputs are projections of state changes

Full:
- named figures, factional networks, court windows, policy bundles, regional variation, and reversals have explicit owning modules / feature packs
- counterfactual changes require causal build-up, influence cost, and save-compatible state
- rebellion, polity formation, usurpation, restoration, and dynasty repair may become playable when the imperial / dynasty-cycle stack exists

## Anti-Patterns

Do not:
- freeze history into a deterministic year-by-year railroad
- allow free player timeline editing
- make named historical figures into static flavor text only
- make famous reforms into one-time stat modifiers
- let narrative text mutate authority state
- let UI own historical consequences
- import later institutions into an earlier start as defaults
- erase ordinary households under famous-person spectacle
- make court politics a detached minigame that stops touching households, markets, yamen, and public life

## Integration Model

Historical process must obey the same modular-monolith rules as everything else.

In the current architecture, it should usually enter through existing modules:
- `OfficeAndCareer`: appointments, remonstrance pressure, evaluation, jurisdiction leverage, factional exposure
- `EducationAndExams`: examination pressure, scholar reputation, school access, policy-writing prestige
- `WorldSettlements`: tax, road, granary, disaster, and local administrative environment
- `PopulationAndHouseholds`: debt, labor, migration, illness, household survival
- `TradeAndIndustry`: credit, prices, transport, government purchasing, commercial pressure
- `PublicLifeAndRumor`: edicts, public legitimacy, rumor, notices, temple and street interpretation
- `OrderAndBanditry`: policy failure, disorder, suppression, paper compliance, implementation drag
- `ConflictAndForce` / `WarfareCampaign`: military burden, frontier pressure, supply, mobilization, aftermath
- `SocialMemoryAndRelations`: faction labels, favors, shame, grudges, obligations, policy memories
- `NarrativeProjection`: visible historical notices and explanations only

A future historical-process pack may own high-level trend windows, named-figure pressure, and scenario chronology.
Until then, do not create hidden global state in UI or presentation code.

## External Source Notes

Current calibration notes:
- **CHGIS** (China Historical GIS) provides historical geography for administrative hierarchy (prefecture/county boundaries, place names, and temporal validity). Use CHGIS to discipline map nodes and administrative labels before turning them into `WorldSettlements` state.
- **CBDB** (China Biographical Database) provides person data, kinship, offices, postings, and social ties for named historical figures. Use CBDB to discipline named-figure carriers (birth/death dates, office sequences, kinship networks) before turning them into `PersonRegistry` or `SocialMemoryAndRelations` state.
- **Columbia Asia for Educators** provides Song society references for commercialization, city life, exam culture, and imperial legitimacy. Use Columbia for social-pressure calibration (market density, urban consumption, literacy rates, exam quotas).
- **Britannica** provides quick stable biographical summaries for date/person checks. Use Britannica for stable fact verification (reign dates, reform years, figure lifespans) but not for contested institutional claims.
- **Scholarly sources** (Cambridge, Journal of Chinese History, etc.) provide contested or structural claims such as military institutions, tax regimes, and social mobility patterns. Use scholarly sources when the claim is debated or requires structural evidence.

Specific source uses:
- Britannica frames [Renzong](https://www.britannica.com/biography/Renzong) as a strong humane ruler whose reign also saw factional divisions that shaped later reform disputes.
- Britannica's [Fan Zhongyan](https://www.britannica.com/biography/Fan-Zhongyan) entry supports Qingli reform as a ten-point reform program in 1043 touching corruption, unused land, landholding, local militia, labor services, examinations, and schools.
- Britannica's [Wang Anshi](https://www.britannica.com/biography/Wang-Anshi) entry supports treating the 1069-1076 New Policies as a later Shenzong-era package grounded in prior local official experience, fiscal policy, crop loans, land survey, government purchasing, village militia / baojia, hired-service substitution, education / exam change, and opposition.
- Britannica's [baojia](https://www.britannica.com/topic/baojia) entry supports treating baojia as a Song-era Wang Anshi-linked security / military measure, not an opening Renzong default everywhere.
- Columbia Asia for Educators supports Song [commercialization](https://afe.easia.columbia.edu/songdynasty-module/econ-rev-commercial.html), [city life](https://afe.easia.columbia.edu/songdynasty-module/cities-new.html), [exam-centered scholar-official culture](https://afe.easia.columbia.edu/songdynasty-module/confucian-scholar.html), and the emperor's [ritual-bureaucratic legitimacy](https://afe.easia.columbia.edu/cosmos/irc/emperor.htm) as major social pressures.
- Cambridge scholarship on [Song military institutions](https://www.cambridge.org/core/journals/journal-of-chinese-history/article/military-institutions-as-a-defining-feature-of-the-song-dynasty/D020A447BD8666C3304D7A315CB65DFD) supports treating military administration and frontier pressure as state structure, civil-official oversight, fiscal burden, and policy conflict rather than battlefield-only spectacle.

**Source calibration rule:** When using external sources, state what they calibrate and what they do not prove. A source should become a rule-chain constraint, not a lore dump. Do not convert unsourced memory into authoritative game rules.

These notes are not a citation database.
When implementing a concrete scenario, add a narrower source note in the ExecPlan for the exact person, policy, place, or institution being modeled.

## Done Criteria For A Historical Trend

A historical process is ready for implementation only if it can answer:
- what trend pressure exists before the famous moment
- which named or unnamed actors carry it
- which module owns each state change
- which query / command / event seam transmits it
- what local surface shows it first
- how the player can be swept up by it
- how the player can carry or bend it at the current reachable scale
- what backlash or residue remains
- what is historically fixed, elastic, or deliberately fictionalized
- how the result remains deterministic and save-compatible
