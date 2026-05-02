# SOCIAL_STRATA_AND_PATHWAYS

This document defines the main social positions and how people move between them.

For the stronger multi-route design table covering route families, verbs, pressures, modules, surfaces, and transitions, see `MULTI_ROUTE_DESIGN_MATRIX.md`.

## Core rule

These are not isolated job trees.
They are social positions within the same world.

The design target is a living Northern Song society, not a career-selection screen.
Every social position in this document is described as a **pressure chain**, not a static label:
- **Pressure start**: what social force opens the chain
- **Actor carrier**: who carries that pressure (named or unnamed)
- **Visibility**: how it appears on hall, desk, ancestral-hall, or ledger surfaces
- **Consequence**: what module state it changes
- **Module ownership**: which module owns the authoritative state

## Pressure drift, not route choice

People and households move because pressure pushes them.

Relevant pressure includes:
- livelihood and subsistence
- debt and credit
- illness and labor loss
- grain price and market access
- study cost and exam failure
- marriage, mourning, and funeral burden
- tax, corvée, and document contact
- road safety and local order
- protection, patronage, and public face

Example:
> A tenant's son who once studied, failed, and now faces his father's illness debt should not become a single "career class." The same person may drift toward shop accounting, village teaching, contract writing, debt flight, lineage dependence, service, petition work, or gray survival depending on which pressure chain opens first.

### Historical baseline: household registration (zhuhu / kehu)

The Northern Song household-registration system distinguishes **two fundamental categories** that shape all social pressure:

- **主户 (zhuhu)** — households with permanent property (land). They are the tax-and-corvée base, ranked into five grades (一等至五等) by property and household size. Upper grades bear heavier corvée; lower grades are still liable for basic tax but lighter duty. The government actively encouraged tenants to become zhuhu through land reclamation and relief policies.
- **客户 (kehu)** — households without permanent property, mainly tenant farmers and laborers. They do not pay the main land tax (二税) and are exempt from formal corvée duty, but they live under rent pressure and lineage dependence. By the early Northern Song kehu may have comprised roughly 30–40% of the rural population.

This distinction is not a player "class choice." It is a structural pressure layer:
- A zhuhu household faces tax, corvée, and document-contact pressure directly.
- A kehu household faces rent, lineage dependence, and downward-mobility risk, with fewer formal protections but also fewer formal obligations.
- Conversion between the two happens through land acquisition (kehu → zhuhu) or land loss (zhuhu → kehu), not through a UI toggle.

**Anachronism guard:** The *baojia* 保甲 system (formal militia-organization by household grouping) was introduced by Wang Anshi in 1069 under Emperor Shenzong. It must **not** appear as a default feature in a Renzong-era (1022–1063) opening. Local security in the Renzong period relies on yamen runners, informal escort arrangements, and county-level order response—not on a structured militia roster.

This is why route-facing language must stay architectural.
The player should see social pressure and social drift, not a rigid route label.

---

## 1. Clan / family

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Marriage need, heir insecurity, death, branch tension, prestige competition, support burden |
| **Actor carrier** | Household head, branch elder, matchmaker, widow, disgruntled kin |
| **Visibility** | Ancestral hall tablets, branch ledgers, memorial pile, heir marker, visitor slot |
| **Consequence** | Genealogy update, property redistribution, alliance formation, grudge registration |
| **Module ownership** | `FamilyCore` owns genealogy, marriage, inheritance, mourning; `SocialMemoryAndRelations` owns grudges and reputation |

The player’s primary layer. A person can remain within family authority while also participating in exams, trade, office, force, or outlaw paths.

### Death: the cross-cutting pressure event

Death does not end at burial. It opens multiple pressure chains simultaneously:
1. **Succession**: `FamilyCore` resolves heir designation; branch tension may spike if unsettled
2. **Mourning**: household labor loss, exam study interruption, office duty suspension, marriage delay
3. **Property**: redistribution triggers branch disputes; `SocialMemoryAndRelations` records favor or resentment
4. **Public face**: funeral scale affects lineage prestige; insufficient mourning draws shame
5. **Office shock**: if the deceased held office or clerk position, `OfficeAndCareer` feels vacancy pressure
6. **Military command**: if the deceased led escort or lineage force, `ConflictAndForce` feels command-gap pressure

---

## 2. Commoners / households

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Grain price, rent, tax demand, corvée call, illness, disaster, debt, labor shortage |
| **Actor carrier** | Household head, tenant farmer, laborer, widow, migrant, servant |
| **Visibility** | Desk sandbox village nodes (color = strain), household ledger, subsistence strain overlay |
| **Consequence** | Debt accumulation, migration drift, household dissolution, riot/disorder risk, kehu→zhuhu conversion or reverse |
| **Module ownership** | `PopulationAndHouseholds` owns livelihood, debt, labor, migration; `TradeAndIndustry` owns market price; `WorldSettlements` owns disaster and corvée windows |

The social base. Includes tenant farmers, laborers, shop hands, petty traders, servants, migrants, and able-bodied men subject to corvée and informal escort duty.

**Prescriptive vs. lived practice gap:** The household-registration system prescribes zhuhu/kehu categories, but actual practice includes unregistered migration, false household splitting to evade tax, unreported deaths to preserve corvée exemptions, and elite households sheltering clients as fictive kin.

### Current implementation preflight: v381-v388

V381-V388 records commoner social-position mobility as a future owner-lane problem, not as a complete class engine.

Current code already has commoner pressure carriers: `PopulationAndHouseholds` owns livelihood, labor/activity pressure, distress, and migration/mobility pools; `EducationAndExams` owns study/exam paths; `TradeAndIndustry` owns shop, route, debt, and market attachment; `OfficeAndCareer` owns yamen/document/office contact; `FamilyCore` owns lineage support, marriage, inheritance, and branch pressure; `PublicLifeAndRumor` owns public visibility; `SocialMemoryAndRelations` owns durable reputation, shame, debt, fear, favor, and grudges.

Future class/status drift must pick one owner lane and state its hot path, cardinality, deterministic cap/order, target scope, no-touch boundary, schema impact, cadence, projection fields, and validation. It must not become a UI promotion button, an Application resolver, a `PersonRegistry` social-class table, or a prose parser.

### Current implementation readback: v389-v396

V389-V396 adds `SocialPositionReadbackSummary` to person dossiers as a projection-only explanation of why a visible person reads socially situated.

The field may name owner carriers such as FamilyCore kin position, PopulationAndHouseholds livelihood/activity, EducationAndExams study/exam status, TradeAndIndustry trade attachment, OfficeAndCareer document/office posture, and SocialMemoryAndRelations memory pressure. `PersonRegistry` still contributes only identity and `FidelityRing`.

This readback does not promote/demote people, resolve zhuhu/kehu conversion, create office-service or trade-attachment routes, write durable residue, or make UI/Unity a class authority.

---

## 3. Exams

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Family investment capacity, local school access, tutor quality, study time, stress, rank competition |
| **Actor carrier** | Aspirant, household head funding study, academy master, exam grader, patron |
| **Visibility** | Academy node heat, ancestral hall study-marker, exam-result notice, household ledger tuition entry |
| **Consequence** | Pass/fail, prestige change, marriage value change, household labor reallocation, later office candidacy |
| **Module ownership** | `EducationAndExams` owns study state, attempt resolution, outcomes; `FamilyCore` owns funding pressure; `SocialMemoryAndRelations` owns reputation |

Education is **not** just exam rank. The full pressure chain includes:
- **Literacy and copying**: practical document competence (account keeping, petition handling, contract writing)
- **Schooling cost**: academy fees, tutor fees, book/paper/ink costs, travel to exam site
- **Academy prestige**: which academy the aspirant attends affects grader bias and patron network access
- **Study withdrawal**: failure or funding collapse returns aspirant to household labor or trade

---

## 4. Trade

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Capital shortage, route unsafe, market volatile, trust network fracture, debt, logistics failure |
| **Actor carrier** | Merchant, shop keeper, peddler, boatman, creditor, debtor, broker (牙行) |
| **Visibility** | Desk sandbox market-town node heat, route reliability color, trade ledger, debt tally |
| **Consequence** | Profit/loss, credit expansion or collapse, alliance or feud, route abandonment, shop closure |
| **Module ownership** | `TradeAndIndustry` owns shops, routes, credit, prices; `WorldSettlements` owns route conditions; `OrderAndBanditry` owns route safety |

---

## 5. Office

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Exam success, yamen attachment, patron backing, vacancy opening, evaluation pressure, memorial attack |
| **Actor carrier** | Official, clerk (书吏), runner (衙役), patron, faction rival, remonstrance officer |
| **Visibility** | Yamen node on desk sandbox, petition backlog notice, appointment scroll, faction heat marker |
| **Consequence** | Appointment, demotion, exile, dismissal, faction label, family prestige shift, clerk-bloc capture |
| **Module ownership** | `OfficeAndCareer` owns appointments, evaluations, petitions; `SocialMemoryAndRelations` owns faction labels and grudges |

**Officials are not disembodied offices.** Office pressure is entangled with private life:
- **Household need**: official salary often insufficient; kin expect support, dowry help, funeral contributions
- **Kin pull**: natal and affinal kin demand favors, appointments, protection, or marriage leverage
- **Gift obligation**: seasonal and life-event gift exchange creates debt networks
- **Patronage**: rising officials need patrons; patrons expect loyalty, information, and future reciprocity
- **Clerk dependence (书吏捕获)**: clerks control actual paperwork; an official without clerk trust cannot implement policy
- **Reputational cover**: public face (清名) matters for promotion; scandal destroys career faster than incompetence
- **Office conflict**: memorial attack (弹章), procedural freezing, clerk splits, patron backing shifts, reputational war

Office pressure must also distinguish **title**, **credential**, **post**, and **actual reach**:
- a degree-holder may have prestige without office
- a recommended candidate may have hope without jurisdiction
- an attached yamen actor may move documents without formal rank
- a formal official may hold the seal yet be captured by clerks, debt, patrons, family pull, or faction fear
- a retired or dismissed official may still carry local face, enemies, and document literacy

The office chain should therefore read as:
`study / recommendation / patronage -> waiting or yamen attachment -> appointment or clerk service -> docket control -> local implementation -> evaluation, memorial attack, promotion, demotion, family consequence`

### 5a. Court process and official gaze

Court-facing play is not just "high-level office".
It adds a separate pressure ladder:
- memorials and accusations enter a queue
- court agenda is shaped by fiscal, frontier, ritual, disaster, appointment, exam, law, succession, or reform pressure
- senior backing, censor pressure, emperor attention, and faction heat change whether a matter moves, stalls, or turns poisonous
- the result becomes appointment slate, policy window, punishment tone, relief wording, military urgency, or court-time disruption
- local yamen and households see the result only after documents, posts, clerks, routes, public talk, and local buffering translate it

A future court process surface should show agenda pressure, memorial queue, appointment and policy windows, faction heat, and dispatch targets.
It should not become a cutscene that directly overwrites household, market, or county state.

---

## 6. Law / litigation

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Accusation, property dispute, injury, murder, tax evasion charge, heresy suspicion |
| **Actor carrier** | Plaintiff, defendant, yamen magistrate, clerk, runner, mediator, lineage elder, witness |
| **Visibility** | Yamen docket notice, legal document scroll, prisoner marker, mediation session visitor |
| **Consequence** | Judgment, punishment, compensation, grudge registration, lineage shame, public legitimacy shift |
| **Module ownership** | `OrderAndBanditry` owns accusation and punishment; `SocialMemoryAndRelations` owns shame and grudge; `PublicLifeAndRumor` owns street talk about the case |

Law is a **first-class system topic**, not a sub-topic of order. The yamen is where state power is most directly felt by commoners: tax disputes, land lawsuits, criminal accusations, and petition handling all produce pressure that alters household survival, lineage reputation, and public legitimacy.

---

## 7. Religion / temples / ritual

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Festival obligation, funeral ritual, oath/vow, heterodoxy fear, temple-fair economy, rain prayer, ancestral neglect guilt |
| **Actor carrier** | Monk, priest, spirit medium, temple manager, lineage ritual officer, pilgrim, donor |
| **Visibility** | Temple node on desk sandbox, festival calendar marker, ancestral-hall incense state, public-life rumor about omens |
| **Consequence** | Legitimacy gain/loss, community solidarity or panic, economic stimulus (fair days), lineage shame if rituals neglected |
| **Module ownership** | `PublicLifeAndRumor` owns festival heat, rumor, legitimacy language; `FamilyCore` owns ancestral ritual; `SocialMemoryAndRelations` owns community reputation |

Religion is **not decorative**. Temples are public spaces where rumor spreads, legitimacy is negotiated, and community solidarity is formed or fractured. Festival days alter market cadence, labor availability, and public attention. Omens and heterodoxy fears can trigger panic, migration, or official intervention.

---

## 8. Local culture / custom / regional identity

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Outsider arrival, dialect barrier, customary dispute, place-pride competition, regional stigma, schooling tone difference |
| **Actor carrier** | Local elder, migrant, merchant from another region, matchmaker, schoolmaster, temple keeper |
| **Visibility** | Settlement node regional accent marker, market-town custom flag, public-life rumor about "outsiders" |
| **Consequence** | Trust friction, marriage barrier, trade suspicion, recruitment bias, customary legitimacy advantage or penalty |
| **Module ownership** | `WorldSettlements` owns regional eco-zone; `PublicLifeAndRumor` owns outsider friction and rumor; `SocialMemoryAndRelations` owns trust networks |

Local culture alters **trust**, **outsider friction**, **public behavior**, **recruitment tone**, and **customary legitimacy**. A Jiangnan water-network county and a north-China dry-road county should feel socially different beyond terrain: schooling tone, market customs, temple styles, and lineage practices vary.

---

## 9. Material life

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Clothing wear, food shortage, housing decay, fuel strain, travel cost, workshop tool breakage, water-source failure, healer access, funeral expense |
| **Actor carrier** | Household member, artisan, peddler, healer, coffin maker, builder, ferryman |
| **Visibility** | Household ledger expense entries, village-node prosperity color, market-town craft smoke, well-post water-level marker |
| **Consequence** | Dignity loss, mobility restriction, labor reduction, debt, household dissolution, public shame |
| **Module ownership** | `PopulationAndHouseholds` owns household consumption and storage; `TradeAndIndustry` owns market supply; `WorldSettlements` owns waterworks and infrastructure |

Material life is **not decorative set dressing**. Clothing wear affects public face and marriage value. Food shortage triggers debt or migration. Housing decay increases illness risk. Fuel strain reduces winter survival. Workshop tool breakage halts production. Water-source failure destroys agriculture. Healer access determines mortality. Funeral expense can bankrupt a household.

---

## 10. Lineage private force

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Feud escalation, estate protection need, escort demand, bandit threat, rival lineage provocation, debt enforcement |
| **Actor carrier** | Jiading (家丁, household retainer), hall guard, estate guard, escort leader, feud enforcer |
| **Visibility** | Lineage-hall guard marker, estate-cluster security color, escort token on route, casualty tally on conflict vignette |
| **Consequence** | Force posture change, feud temperature rise, county-yamen attention, reputation shift (honor or stain), death/injury |
| **Module ownership** | `ConflictAndForce` owns force posture and resolution; `FamilyCore` owns retainer employment; `OrderAndBanditry` owns county response; `SocialMemoryAndRelations` owns feud memory |

**Lineage force is not a generic army.** Private retainers, hall guards, escorts, feud violence, and estate coercion are socially embedded and politically ambiguous:
- A lineage with retainers may be respected as protector or feared as local bully
- Escort service creates obligation networks across counties
- Feud violence stains both parties; third-party mediation is often required
- County yamen may tolerate private force as local order supplement or suppress it as threat

---

## 11. Outlaw / banditry

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Famine, debt collapse, expulsion, war damage, failed upward mobility, revenge pressure, security breakdown |
| **Actor carrier** | Bandit leader, recruited refugee, former soldier, debt fugitive, revenge seeker, gray-route broker |
| **Visibility** | Route bandit-risk overlay (red corridor), black-route marker, wanted notice on yamen node, refugee cluster on desk sandbox |
| **Consequence** | Route closure, protection demand, recruitment or suppression pressure, long-term stain, fear, retaliation |
| **Module ownership** | `OrderAndBanditry` owns disorder and suppression; `ConflictAndForce` owns violent resolution; `TradeAndIndustry` owns gray-route economics; `SocialMemoryAndRelations` owns fear and stigma |

This is not a "cool faction mode". It emerges from living-world pressure accumulation. The pressure ladder:
1. Livelihood strain (`PopulationAndHouseholds`)
2. Debt or tax flight (`TradeAndIndustry`, `PopulationAndHouseholds`)
3. Route insecurity (`WorldSettlements`, `OrderAndBanditry`)
4. Gray survival or bandit recruitment (`OrderAndBanditry`)
5. Local conflict or suppression (`ConflictAndForce`)
6. Social memory and stigma (`SocialMemoryAndRelations`)

---

## 12. Imperial / court pressure

| Chain element | Detail |
|--------------|--------|
| **Pressure start** | Reign legitimacy, edict, amnesty, appointment, fiscal demand, military budget, court agenda, reform debate, succession uncertainty, regime recognition |
| **Actor carrier** | Emperor, court faction, censor, remonstrance officer, senior official, claimant, loyalist, outer-kin, reformer, conservative, local office-holder |
| **Visibility** | Imperial edict scroll on macro sandbox edge, appointment notice, court docket marker, reform rumor in public-life stream, legitimacy pillar on realm sandbox, recognition or defection signal |
| **Consequence** | Tax climate shift, exam policy change, corvée window adjustment, appointment cascade, faction label, public legitimacy swing, office side-taking, regime compliance or paper compliance |
| **Module ownership** | `WorldSettlements` owns `ImperialBand` and season bands; `OfficeAndCareer` owns appointment pressure; `PublicLifeAndRumor` owns rumor and legitimacy; `NarrativeProjection` owns edict projection |

**Huangquan (皇权)** is not just paperwork. It operates through:
- **Legitimacy**: the moral and symbolic force of imperial rule, felt even in distant counties
- **Succession**: clarity or uncertainty about the throne alters official behavior and public confidence
- **Ritual center**: court ritual tone sets the moral frame for local ceremonies and legal judgments
- **Amnesty tone**: the generosity or severity of amnesties shapes local expectations of mercy
- **Symbolic confidence**: disasters and military defeats erode "mandate" confidence even without direct policy change

**Emperor-to-county is not a pure command line.** It is mediated through:
- **Bargaining**: quotas are often negotiated between center and locality
- **Buffering**: local elites and clerks soften or delay implementation
- **Selective enforcement**: some edicts are enforced vigorously, others ignored
- **Paper compliance**: documents show compliance while actual practice differs
- **Local reinterpretation**: county magistrates adapt central policy to local conditions

Player-facing carriers:
- edict scrolls and sealed documents
- county-gate postings
- yamen docket pressure
- appointment notices and evaluation rumors
- tax, corvee, relief, and military supply language
- amnesty proclamations and punishment tone
- mourning cloth, ritual interruption, and public silence around imperial death
- border dispatches that change prices, escort demand, office urgency, and public fear

This makes huangquan feel present without making the player a hidden emperor.

**Polity / regime pressure** is broader than the throne label.
A regime is the currently persuasive bundle of:
- who can issue documents
- who can appoint and dismiss office-holders
- who can collect tax, grain, labor, and military supply
- who controls or protects key routes
- who can stage ritual legitimacy and public mercy / punishment
- which officials, clerks, local elites, soldiers, temples, markets, and households recognize the order as real enough to obey

This allows later play to model rebellion, provisional governance, usurpation, restoration, regional autonomy, or dynasty repair without turning them into one-step events.
The route should read:
`protection failure / fiscal fatigue / succession uncertainty -> armed or office coalition -> document and grain-route reach -> public legitimacy and ritual claim -> recognition, compliance, defection, or collapse`

Renzong-era start conditions calibrate the first pressures only.
They should not force the Qingli reforms, later Wang Anshi reforms, faction outcomes, succession consequences, military crises, or regime futures to occur exactly as known history.
The game should protect plausibility by using institutional constraints and pressure chains, while still allowing people, regions, shocks, and player leverage to push the world into earned counterfactual history.

---

## Influence dimensions

Player influence is **not** a single prestige bar. It resolves through seven distinct dimensions, each with different reach and friction:

| Dimension | Reach | Friction | Shell visibility |
|-----------|-------|----------|------------------|
| **Lineage standing** | Household → clan → affinal network | Branch tension, elder rivalry, gender limits | Ancestral hall prestige marker, branch ledger |
| **Favor debt (人情)** | Personal network | Reciprocity expectation, memory decay | Social-memory web, obligation tally |
| **Office authority** | Jurisdiction → clerk network → county | Clerk capture, paper compliance, evaluation risk | Yamen node heat, appointment scroll |
| **Wealth pull** | Market → credit → investment | Price volatility, debt exposure, envy/predation | Trade ledger, estate tokens |
| **Coercive weight** | Retainers → escorts → local force | County suppression, feud retaliation, stain | Guard markers, casualty tallies |
| **Public legitimacy** | Reputation → rumor → crowd mood | Shame, slander, rumor distortion | Public-life stream heat, notice-board sentiment |
| **Information reach** | Letters → rumor → spy network | Delay, distortion, information cost | Message-delay band, courier route threads |

Commands fail when the player lacks the required dimension. A prestige-rich but cash-poor lineage cannot fund study. An office-holder without clerk trust cannot implement policy. A wealthy merchant without lineage standing cannot arrange favorable marriage.

---

## Return paths matter

These transformations should not be read as one-way disposal.

Especially for office, coercive, and feud-linked actors, the world should support:
- demotion out of the core ring
- summary-level survival in a remote or exile pool
- social-memory persistence
- later re-entry through merit, amnesty, patronage, shortage, or factional change

The same logic should also apply to:
- kin who leave the household core
- affines who drift into distant counties
- trusted friends or clients who lose local visibility
- branch members who disappear from daily life but remain narratively and politically relevant

This is how a living society keeps old grudges, obligations, and reputations active across years.

---

## Design rule

These transitions should be produced by world pressure and module logic, not by hard-coded career trees.

Design principle:
- lineage is not the whole world
- commoners are not background population
- exams are family investment and failure pathways, not a promotion button
- markets are cash, credit, labor, and road pressure, not a profit table
- yamen contact is paperwork, tax contract, petition, service, intermediary, and public face pressure, not a quest NPC
- **law is a first-class pressure chain**, not a sub-topic of order
- **religion shapes legitimacy and community**, not decorative backdrop
- **local culture alters trust and recruitment**, not cosmetic reskin
- **material life affects dignity and survival**, not set dressing
- **lineage force is socially embedded coercion**, not a generic army
- **imperial pressure is bargained and buffered**, not a direct command line

## V213-V244 First Social-Mobility Layer

- Social position now has a first executable drift layer in `PopulationAndHouseholds`: severe household pressure can push tenant or smallholder households toward seasonal migration, hired labor, or vagrancy, while recovery can move hired labor back toward smallholding when land, grain, labor, and settlement conditions support it.
- Person movement is still bounded by fidelity budget. A few hot household members can be pulled into `Local` readback through `PersonRegistry.ChangeFidelityRing`, but regional society remains represented through labor, marriage, and migration pools.
- This is not a full social-class engine, faction AI, or per-person world simulation. It is the first readable loop for "near detail, far summary" over existing population and registry fields.

### Current implementation source keys: v397-v404

- `SocialPositionSourceModuleKeys` now exposes which owner-lane snapshots support a person dossier's social-position readback.
- `PersonRegistry` appears only as the identity / `FidelityRing` anchor. Household livelihood, study/exam, trade attachment, office posture, family placement, and social-memory pressure remain in their owner modules.
- This list is the future-safe reader path. Do not parse `SocialPositionLabel`, `SocialPositionReadbackSummary`, receipt prose, notification prose, or docs text to decide which lane contributed.

### Current closeout: v405-v412

- V381-V404 is closed only as a first readback layer for commoner / social-position legibility.
- Current code can show what made a nearby person's social position readable; it cannot promote, demote, convert zhuhu/kehu status, assign office service, attach a trade route, or simulate every commoner as an individual career actor.
- Future status drift still needs a single owner lane, target scope, cadence, deterministic cap/order, schema impact, and validation before implementation.

### Current scale-budget readback: v413-v420

- `SocialPositionScaleBudgetReadbackSummary` states the detail rule directly on person dossiers: near people can read owner-lane detail, distant society remains pooled summary.
- The readback is built from `FidelityRing` and structured social-position source keys. It does not move anyone between rings or add a hidden class/status route.
- Future commoner status depth still needs an owner lane before it changes state, precision, or durable residue.

### Current regional guard: v421-v428

- A `FidelityRing.Regional` person dossier must read as regional summary, not close/local detail.
- This protects the far-summary side of the scale budget: regional society remains pooled and pressure-carried unless a later owner lane explicitly raises precision with deterministic cap/order, schema impact, and validation.
- This guard adds no new social class, commoner route, office-service lane, or per-person world simulation.

### Current closeout: v429-v436

- V381-V428 is closed only as first-layer social-position visibility: future-lane requirements, readback, structured source keys, scale-budget wording, and regional summary guard.
- Current code can explain why a nearby dossier has social-position context and why regional society remains summary-first. It still cannot promote, demote, convert zhuhu/kehu status, assign office service, attach trade work, or simulate every commoner as an individual career actor.
- Future status depth still needs a single owner lane, target scope, cadence, deterministic cap/order, schema impact, and validation before implementation.

### Current owner-lane preflight: v437-v444

- Future first commoner status rules should start from `PopulationAndHouseholds` because household livelihood, activity, distress, debt, migration pressure, labor, land, grain, and pool thickness already live there.
- `PersonRegistry` remains identity / `FidelityRing` only. It should not become the place where commoner class, route, service, or trade status is stored.
- Future status depth still needs a specific owner state, cadence, deterministic target selection, schema plan, and player-facing projection before implementation.

### Current scale-budget preflight: v445-v452

- Commoner and personnel depth follows the rule: near detail, far summary. Close household and pressure-hit actors may be readable; distant society stays summarized through pools and settlement pressure.
- Future status depth must say whether it touches named close-orbit actors, local households, active-region pools, or distant summaries before it changes fidelity or state.
- No future route may use `PersonRegistry`, UI selection, or prose parsing as a class/status authority path.

### Current household dynamics explanation: v453-v460

- Existing household mobility dynamics now have a first structured explanation readback over household social-pressure signals.
- The explanation can name debt/subsistence, labor, mobility, family, office, market, public-life, or order pressure dimensions when those signals are already present.
- This does not add a class ladder, commoner status route, zhuhu/kehu conversion, office-service lane, trade attachment lane, durable residue, selector, or global per-person simulation.
- `PopulationAndHouseholds` remains the owner of household dynamics; `PersonRegistry` remains identity / `FidelityRing` only; distant society remains pooled summary until a later owner-lane plan changes state or fidelity.

### Current household dynamics closeout: v461-v468

- The v453-v460 explanation layer is closed only as first-layer readback evidence.
- It does not add migration economy, route history, class/status drift, zhuhu/kehu conversion, office-service lane, trade attachment lane, durable residue, selector, direct movement command, or all-world per-person simulation.
- Future commoner status or household movement depth still starts with an owner-lane plan, target scope, hot path, deterministic cap/order, cadence, schema impact, projection fields, and validation.

### Current household mobility owner-lane preflight: v469-v476

- Future household mobility rule depth should start from `PopulationAndHouseholds` unless a later ExecPlan proves another owner lane, because household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers already live there.
- The scale rule is still near detail, far summary: player-near and pressure-hit households can become richer local readbacks; distant society remains settlement/pool pressure summary until a bounded owner-lane rule promotes detail.
- A future implementation must declare owner state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation before changing runtime behavior.
- This preflight does not add migration economy, route history, class/status drift, zhuhu/kehu conversion, office-service lane, trade attachment lane, durable residue, selector, direct movement command, `PersonRegistry` expansion, or UI/Unity authority.

### Current household mobility preflight closeout: v485-v492

- V469-V476 is closed only as future-rule gate evidence. The codebase still has no household movement command, route-history model, migration economy, selector, or class/status drift rule.
- `PopulationAndHouseholds` remains the default first owner lane for later household mobility depth; `PersonRegistry` remains identity / `FidelityRing` only.
- Future household mobility implementation still needs one owner-lane plan naming owned state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation.

### Current household mobility runtime/rules-data readiness: v501-v508

- V501-V508 moves the gate one step forward by documenting a first runtime rule readiness map and hardcoded extraction map. It still adds no household movement, route-history, migration economy, selector, class/status drift, or per-person world simulation.
- The first future rule should start from `PopulationAndHouseholds` and read existing household livelihood/activity/distress/debt/labor/grain/land/migration-pressure and pool carriers. These are social-pressure carriers, not a universal social-class ladder.
- Target scope remains scale-budgeted: player-near households, pressure-hit local households, active-region pools, and distant summaries. Quiet households, off-scope settlements, distant pooled society, `PersonRegistry`, Application, UI, and Unity stay no-touch.
- Thresholds, weights, regional/era assumptions, recovery/decay rates, fanout caps, tie-break priorities, and pool limits should move over time into owner-consumed authored rules-data, but not into a runtime plugin marketplace or prose-owned authority.
- The closeout preserves near detail, far summary: player-near and pressure-hit households can become richer only through future bounded owner rules; distant society remains summarized by pools and settlement pressure.

### Current household mobility rules-data contract preflight: v509-v516

- V509-V516 defines a future rules-data contract for household mobility without adding movement, route history, migration economy, selector state, class/status drift, or per-person world simulation.
- The contract keeps social drift pressure-carried rather than class-tree driven: stable ids, schema/version, deterministic ordering, default fallback, readable validation errors, and owner-consumed use only.
- Future data categories are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities.
- The current repo has no reusable runtime rules-data/content/config pattern, so this pass remains docs/tests-only and adds no loader or default file.
- `PopulationAndHouseholds` remains the later owner lane; Application, UI, Unity, prose, and `PersonRegistry` remain outside household movement authority.

### Current household mobility default rules-data skeleton gate: v517-v524

- V517-V524 defines the default skeleton shape for future household mobility rules-data without adding a file, loader, validator implementation, movement rule, route history, selector, class/status drift, or per-person world simulation.
- The future skeleton is data-only and owner-consumed: `ruleSetId`, `schemaVersion`, `ownerModule`, `defaultFallbackPolicy`, `parameterGroups`, `validationResult`, and deterministic declaration order.
- Parameter groups remain social-pressure tuning categories, not class ladders: threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and tie-break priorities.
- The skeleton does not enter save and cannot be read by Application, UI, Unity, prose, or `PersonRegistry` as movement authority.

### Current household mobility first hardcoded rule extraction: v525-v532

- V525-V532 extracts the focused member promotion fanout cap from a naked module constant into `PopulationHouseholdMobilityRulesData`.
- The default remains two regional members per pressure-hit household, preserving the existing near-detail promotion behavior and person-id tie-break order.
- This is still not class/status drift, zhuhu/kehu conversion, migration economy, route history, direct movement, durable residue, or all-world per-person simulation.
- `PopulationAndHouseholds` is the only consumer. Application, UI, Unity, prose, and `PersonRegistry` do not read this rule data as authority.

### Current household mobility first runtime rule: v533-v540

- V533-V540 adds the first tiny monthly runtime rule in `PopulationAndHouseholds`, using existing household livelihood, distress, debt, labor, grain, land, migration risk, and `MigrationPools` outflow pressure.
- The social effect is deliberately narrow: the highest-pressure active local pool may nudge at most two pressure-hit households by one existing migration-risk point.
- This is not status-class conversion, household relocation, route history, migration economy, zhuhu/kehu engine, durable social residue, or all-world person simulation.
- Quiet households, lower-priority active pools, off-scope settlements, distant pooled society, `PersonRegistry`, Application, UI, and Unity remain no-touch authority surfaces.

### Current household mobility first runtime rule closeout: v541-v548

- V541-V548 closes the first runtime rule as bounded household-pressure evidence only.
- It does not add a second runtime rule, status/class drift, zhuhu/kehu conversion, relocation, route history, migration economy, durable social residue, or full-population person simulation.
- The current social meaning remains a local pressure nudge over existing household distress/debt/labor/grain/land/livelihood/migration signals.
- Any future household movement, deeper regional variation, or persistent history must return with a new owner-lane plan and schema decision before implementation.

### Current household mobility runtime rule health evidence: v549-v556

- V549-V556 records health/readiness evidence for the first monthly household mobility runtime rule; it does not add another social-status formula or runtime rule.
- The existing social interpretation remains bounded: one active local pool may nudge capped pressure-hit households, while quiet households, lower-priority pools, off-scope settlements, and distant pooled society remain no-touch.
- The next social gate before expansion is evidence of touched counts, deterministic cap/order, same-seed replay, no-touch summaries, and pressure-band interpretation; this is not long-run saturation tuning.
- It adds no relocation, route history, migration economy, zhuhu/kehu conversion, durable social residue, class/status engine, `PersonRegistry` expansion, Application/UI/Unity authority, or schema change.

### Current household mobility runtime widening gate: v557-v564

- V557-V564 keeps household mobility social interpretation at the current pressure-nudge scale; it does not widen fanout or add a second runtime rule.
- Any future social expansion must state whether pressure bands reflect intended local distress, missing recovery, missing allocation, or projection debt before changing recovery/decay formulas.
- The gate requires touched household/pool/settlement counts and quiet/off-scope/distant-summary no-touch proof before richer household movement or regional detail can land.
- It adds no relocation, route history, migration economy, zhuhu/kehu conversion, durable social residue, class/status engine, touch-count state, diagnostic state, performance cache, `PersonRegistry` expansion, Application/UI/Unity authority, or schema change.

### Current household mobility runtime touch-count proof: v565-v572

- V565-V572 turns the widening-gate requirement into focused test evidence over the current first runtime rule; it does not widen fanout or add a second rule.
- The owner test proves the current default fixture touches exactly two eligible households in one selected active pool while leaving the lower-priority candidate, quiet household, and lower-priority active pool untouched.
- Social interpretation stays at pressure-nudge scale: no relocation, route history, migration economy, zhuhu/kehu conversion, durable social residue, class/status engine, persisted touch-count state, diagnostic state, performance cache, `PersonRegistry` expansion, Application/UI/Unity authority, or schema change.

### Current household mobility rules-data fallback matrix: v573-v580

- V573-V580 adds fallback evidence for malformed household mobility runtime rules-data; it does not introduce a runtime loader or content file.
- Malformed threshold/cap/delta values fall back to defaults, and an owner run with malformed runtime rules-data must match the default run signature.
- Social interpretation remains owner-state pressure readback only: no relocation, route history, migration economy, zhuhu/kehu conversion, durable social residue, class/status engine, persisted touch-count state, diagnostic state, performance cache, `PersonRegistry` expansion, Application/UI/Unity authority, runtime plugin marketplace, or schema change.
### Current household mobility runtime threshold no-touch proof: v581-v588

V581-V588 records that the first household mobility runtime rule can be blocked by `monthly_runtime_active_pool_outflow_threshold` without touching households or pools. The no-touch proof stays inside `PopulationAndHouseholds`, compares against a zero-risk-delta baseline, and adds no social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime zero-cap no-touch proof: v589-v596

V589-V596 records that settlement and household fanout caps can block the first household mobility runtime rule without touching households or pools. The no-touch proof stays inside `PopulationAndHouseholds`, compares against a zero-risk-delta baseline, and adds no social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime zero-risk-delta no-touch proof: v597-v604

V597-V604 records that zero runtime risk delta blocks the first household mobility runtime rule without touching households or pools. The no-touch proof stays inside `PopulationAndHouseholds`, compares against a cap-blocked no-touch baseline, and adds no social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime candidate-filter no-touch proof: v605-v612

V605-V612 records that candidate filters block the first household mobility runtime rule from touching already-migrating/high-risk households or households below the candidate migration-risk floor. The proof stays inside `PopulationAndHouseholds` and adds no social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime tie-break no-touch proof: v613-v620

V613-V620 records that equal-score household candidates are resolved by deterministic household-id ordering in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows the lower household id consumes the cap-one touch, and keeps the tied higher household id no-touch without adding a social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime pool tie-break no-touch proof: v621-v628

V621-V628 records that equal-outflow active pools are resolved by deterministic settlement-id ordering in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows the lower settlement id consumes the cap-one pool touch, and keeps the tied higher settlement id no-touch without adding a social class/status ladder, route history, movement command, or `PersonRegistry` expansion.
### Current household mobility runtime score-ordering no-touch proof: v629-v636

V629-V636 records that higher household runtime score outranks lower household id in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows the higher-score candidate consumes the cap-one touch, and keeps the lower household id no-touch without adding a social class/status ladder, route history, movement command, or `PersonRegistry` expansion.

### Current household mobility runtime pool-priority no-touch proof: v637-v644

V637-V644 records that active-pool priority is applied before cross-pool household score comparison in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows the higher-outflow pool consumes the cap-one settlement pass, and keeps a higher-scoring household in the lower-priority pool no-touch without adding a social class/status ladder, route history, movement command, or `PersonRegistry` expansion.

### Current household mobility runtime per-pool cap no-touch proof: v645-v652

V645-V652 records that household cap application is scoped inside each selected active pool in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows two selected pools each receive one deterministic household touch under cap one, and keeps lower-score households in each pool no-touch without adding a social class/status ladder, route history, movement command, global household cap, or `PersonRegistry` expansion.

### Current household mobility runtime threshold-event no-touch proof: v653-v660

V653-V660 records that the existing `MigrationStarted` threshold event remains owner-scoped in the first household mobility runtime rule. The proof stays inside `PopulationAndHouseholds`, shows only the selected crossing household emits the structured event with existing metadata, and keeps unselected/off-cap households from emitting threshold events or receiving household mobility pressure diffs without adding a social class/status ladder, route history, movement command, new event type, event router, or `PersonRegistry` expansion.

### Current household mobility runtime event-metadata no-prose proof: v661-v668

V661-V668 records that the first runtime rule's threshold event is socially readable through structured owner metadata, not event prose. The proof stays inside `PopulationAndHouseholds`, derives cause, settlement, and household identity from `Metadata`, and keeps `Summary` downstream without adding a social class/status ladder, route history, movement command, prose parser, event router, or `PersonRegistry` expansion.

### Current household mobility runtime event-metadata replay proof: v669-v676

V669-V676 records that the first runtime rule's threshold-event metadata remains stable across same-seed owner runs. The proof stays inside `PopulationAndHouseholds`, compares event type, entity key, cause, settlement id, household id, and downstream summary as test evidence only, and adds no social class/status ladder, route history, movement command, replay state, event router, or `PersonRegistry` expansion.

### Current household mobility runtime threshold extraction: v677-v684

V677-V684 records that one remaining hardcoded event threshold in the first household mobility runtime rule has been moved into owner-consumed rules-data. The social meaning remains a pressure-threshold receipt for a selected near-detail household; it does not become household relocation, zhuhu/kehu conversion, class/status movement, route history, or a full social mobility ladder.

Default rules-data keeps the threshold at 80, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime candidate-floor extraction: v685-v692

V685-V692 records that the first household mobility runtime rule's low-risk candidate floor has been moved into owner-consumed rules-data. The social meaning remains bounded: low migration-pressure households are still summary/no-touch under default rules-data, and near-detail pressure remains limited to eligible selected households.

Default rules-data keeps the candidate floor at 55, malformed floor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime score-weight extraction: v693-v700

V693-V700 records that the first household mobility runtime rule's migration-risk score weight has been moved into owner-consumed rules-data. The social meaning remains bounded: score weighting orders already-eligible near-detail households inside the selected pool, not distant society, class/status movement, or household relocation.

Default rules-data keeps the migration-risk score weight at 4, malformed weight input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime labor-floor extraction: v701-v708

V701-V708 records that the first household mobility runtime rule's labor-capacity pressure floor has been moved into owner-consumed rules-data. The social meaning remains bounded: labor strain contributes to ordering already-eligible near-detail households inside the selected pool, not distant society, class/status movement, labor-market simulation, or household relocation.

Default rules-data keeps the labor-capacity pressure floor at 60, malformed floor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime grain-floor extraction: v709-v716

V709-V716 records that the first household mobility runtime rule's grain-store pressure floor has been moved into owner-consumed rules-data. The social meaning remains bounded: grain shortage contributes to ordering already-eligible near-detail households inside the selected pool, not distant society, class/status movement, grain-market simulation, or household relocation.

Default rules-data keeps the grain-store pressure floor at 25, malformed floor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime land-floor extraction: v717-v724

V717-V724 records that the first household mobility runtime rule's land-holding pressure floor has been moved into owner-consumed rules-data. The social meaning remains bounded: weak landholding contributes to ordering already-eligible near-detail households inside the selected pool, not distant society, zhuhu/kehu conversion, class/status movement, land-market simulation, or household relocation.

Default rules-data keeps the land-holding pressure floor at 20, malformed floor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime grain-divisor extraction: v725-v732

V725-V732 records that the first household mobility runtime rule's grain-store pressure divisor has been moved into owner-consumed rules-data. The social meaning remains bounded: grain shortage still contributes to ordering already-eligible near-detail households inside the selected pool, not distant society, grain-market simulation, or household relocation.

Default rules-data keeps the grain-store pressure divisor at 2, malformed divisor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime land-divisor extraction: v733-v740

V733-V740 records that the first household mobility runtime rule's land-holding pressure divisor has been moved into owner-consumed rules-data. The social meaning remains bounded: thin holdings still contribute to ordering already-eligible near-detail households inside the selected pool, not distant society, land-market simulation, household relocation, or social-class promotion/demotion.

Default rules-data keeps the land-holding pressure divisor at 2, malformed divisor input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime candidate-ceiling extraction: v741-v748

V741-V748 records that the first household mobility runtime rule's high-risk candidate ceiling has been moved into owner-consumed rules-data. The social meaning remains bounded: households already at the default migration-risk ceiling are not re-selected for the small monthly pressure nudge, and this does not become movement, relocation, route history, or a social-status engine.

Default rules-data keeps the candidate migration-risk ceiling at 80, malformed ceiling input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime distress-trigger extraction: v749-v756

V749-V756 records that the first household mobility runtime rule's distress trigger threshold has been moved into owner-consumed rules-data. The social meaning remains bounded: distress can qualify an already-windowed household for the small monthly pressure nudge, but this is not a full poverty, migration, status, or relocation economy.

Default rules-data keeps the distress trigger threshold at 60, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime debt-trigger extraction: v757-v764

V757-V764 records that the first household mobility runtime rule's debt-pressure trigger threshold has been moved into owner-consumed rules-data. The social meaning remains bounded: debt pressure can qualify an already-windowed household for the small monthly pressure nudge, but this is not a credit market, migration economy, status engine, or relocation command.

Default rules-data keeps the debt-pressure trigger threshold at 60, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime labor-trigger extraction: v765-v772

V765-V772 records that the first household mobility runtime rule's labor-capacity trigger ceiling has been moved into owner-consumed rules-data. The social meaning remains bounded: thin labor capacity can qualify an already-windowed household for the small monthly pressure nudge, but this is not a labor market, migration economy, status engine, or relocation command.

Default rules-data keeps the labor-capacity trigger ceiling at 45, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime grain-trigger extraction: v773-v780

V773-V780 records that the first household mobility runtime rule's grain-store trigger floor has been moved into owner-consumed rules-data. The social meaning remains bounded: thin grain stores can qualify an already-windowed household for the small monthly pressure nudge, but this is not a grain economy, famine system, status engine, or relocation command.

Default rules-data keeps the grain-store trigger floor at 25, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.

### Current household mobility runtime land-trigger extraction: v781-v788

V781-V788 records that the first household mobility runtime rule's land-holding trigger floor has been moved into owner-consumed rules-data. The social meaning remains bounded: thin land holding can qualify an already-windowed household for the small monthly pressure nudge, but this is not a land economy, tenancy/status engine, or relocation command.

Default rules-data keeps the land-holding trigger floor at 15, malformed threshold input falls back to that default, and Application/UI/Unity may not read the parameter to infer movement, choose targets, raise detail, or calculate household outcomes.
