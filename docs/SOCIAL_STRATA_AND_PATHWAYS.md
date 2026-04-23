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
