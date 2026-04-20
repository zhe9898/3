# Search Source Routing

Use this file when the local pack is not enough and you need external historical grounding.

Do not start with broad web search if a source-specific lookup will do the job.
Prefer source-specific routing, then broader scholarly search, then community leads.

## Core rule

Choose the search source by question type first, not by convenience.

A good lookup pass usually answers:
- what kind of claim is being checked
- which source family is best suited to that claim
- what query form should be used
- what confidence label the result deserves

## Core source families

### 1. Historical lexicon and wording sanity

Primary sites:
- Ministry of Education Revised Mandarin Dictionary
- Chinese Text Project

Use for:
- kinship terms
- office titles
- social labels
- old word senses
- whether a term sounds too modern

### 2. Primary text corpus and normative language

Primary sites:
- Chinese Text Project
- other digitized classical-text collections when needed

Use for:
- seeing how a term appears in premodern texts
- checking ritual, legal, or official wording
- verifying whether an institution has textual attestation

Do not treat primary text wording alone as proof of lived practice.

### 3. Biographical and office-network lookup

Primary sites:
- China Biographical Database (CBDB)

Use for:
- official careers
- exam and office trajectories
- elite networks
- person-grounded examples for mechanics and narrative grounding

### 4. Historical geography and route grounding

Primary sites:
- CHGIS / China Historical GIS
- Chinese Text Project for historical place wording

Use for:
- counties and prefectures
- historical place-name variants
- regional hierarchy
- route plausibility
- river, ferry, pass, corridor, and frontier framing

### 5. Local gazetteers and local archives

Primary source family:
- digitized local gazetteers
- library or university gazetteer collections
- regional archive or local-history collections

Use for:
- county and prefecture detail
- local schools, shrines, markets, ferries, embankments, and products
- local custom, notable lineages, or religious geography
- public works, disasters, and administrative routines with place-specific detail

Use local gazetteers as region-thickening evidence, not as a universal China template.

### 6. Modern scholarship and institutional synthesis

Primary source family:
- university presses
- peer-reviewed scholarship
- academic databases
- research institutes

Use for:
- dynasty-specific institutions
- social practice beyond rule texts
- contested interpretation
- clean summaries when raw source material is too thin or too normative

### 7. Museum, archaeology, and material-culture collections

Primary source family:
- museum collections
- archaeology reports
- university material-culture projects

Use for:
- clothing
- housing
- artifacts
- craft tools
- burial practice
- ritual objects
- visually grounded descriptors for UI and content

### 8. Public-library and university digitization

Primary source family:
- national and university digital libraries
- scanned rare books or local-history collections
- open institutional repositories

Use for:
- hard-to-find local or dynasty materials
- local gazetteers not indexed elsewhere
- manuals, compendia, or collections relevant to a specific topic

### 9. Community sources only as leads

Use:
- Wikipedia
- fan wikis
- forums
- general explainers

only to discover search terms, variant spellings, or candidate sources.
Do not let them serve as sole authority for Zongzu-facing design or naming.

## Coverage map across the whole skill

Use this map when the user asks something broad and you need to know which external source family completes the local pack.

### Lineage, inheritance, lineage institutions, retainers, elite local power

Lead with:
- local skill pack
- dictionary for wording sanity
- modern scholarship for practice
- local gazetteers for region-specific lineage density or hall geography
- CBDB when named elite networks or office families matter

### Commoners, labor, debt, tenancy, household continuity, women, fertility, childhood

Lead with:
- local skill pack
- modern scholarship
- local gazetteers when locality matters
- primary text only as secondary support for term or norm language

### Education, literacy, schools, academies, exam funnel

Lead with:
- local skill pack
- modern scholarship
- local gazetteers for academies, schools, local study culture
- CBDB when office careers and degree paths need examples
- dictionary or primary text for title wording

### Office life, yamen workflow, official family entanglement, office conflict

Lead with:
- local skill pack
- CBDB for career patterns and people
- modern scholarship for practice
- local gazetteers for county-level office texture
- primary text for memorial or formal wording

### Imperial power, succession, imperial-local bargaining, court politics

Lead with:
- local skill pack
- modern scholarship
- CBDB for office careers around the court or dynasty transitions
- primary text for reign language, amnesties, and formal legitimacy wording

### Religion, temples, ritual brokerage, state cult, heterodoxy

Lead with:
- local skill pack
- primary text for ritual language
- local gazetteers for temple geography and local cult life
- modern scholarship for lived religion and social practice

### Law, litigation, punishment, petitions, shame, public discipline

Lead with:
- local skill pack
- modern scholarship
- primary text for code or procedural language
- local gazetteers for county-level legal incidents when locality matters

### Disaster, famine, granaries, canals, ferries, public works, infrastructure burden

Lead with:
- local skill pack
- local gazetteers
- historical geography
- modern scholarship
- primary text for official provisioning or relief wording

### City life, markets, workshops, artisans, transport, public space, festivals, rumor

Lead with:
- local skill pack
- local gazetteers
- modern scholarship
- material-culture collections when visual descriptors matter

### Regions, counties, prefectures, roads, ferries, water routes, mountain and frontier belts

Lead with:
- local region pack inside this skill
- CHGIS
- local gazetteers
- primary text for place-name wording

### Frontier, migration, mixed populations, border administration

Lead with:
- local skill pack
- CHGIS
- modern scholarship
- local gazetteers or regional histories

### Warfare, mobilization, grain routes, command culture, force families, rebellion, campaign flow

Lead with:
- local skill pack
- modern scholarship
- primary text for command or institutional language
- local gazetteers when the issue is regional military burden, fort belts, ferries, passes, or garrison geography

### Death, mourning, burial, legitimacy shock, funerary cost, memory after death

Lead with:
- local skill pack
- modern scholarship
- material-culture and archaeology collections
- dictionary or primary text for wording sanity
- local gazetteers when graves, temples, memorial sites, or local ritual practice matter

### Local culture, dialect feel, foodways, dress, performance style, place pride

Lead with:
- local skill pack
- local gazetteers
- modern scholarship
- material-culture collections
- dictionary for term sanity

## Default routing by question type

### 1. Terminology, kinship wording, office titles, lexical sanity

Lead sources:
- Ministry of Education Revised Mandarin Dictionary
- Chinese Text Project

Use when checking:
- kinship terms
- title wording
- office names
- social labels
- whether a term is too modern

Query pattern:
- Chinese term
- pinyin variant
- dynasty plus term when needed

Confidence label usually:
- `historical word sense`
- `primary text attestation`

### 2. People, careers, office trajectories, networked elites

Lead sources:
- China Biographical Database (CBDB)
- Chinese Text Project
- modern scholarship when practice matters more than biography

Use when checking:
- official careers
- degree pathways
- office holders
- patronage networks
- biographical examples for mechanics grounding

Query pattern:
- person name in Chinese
- office title
- dynasty
- region if relevant

Confidence label usually:
- `biographical lookup`
- `institutional summary`

### 3. Geography, counties, prefectures, routes, regional framing

Lead sources:
- CHGIS / China Historical GIS
- local region pack inside this skill
- local gazetteers
- Chinese Text Project for historical place wording

Use when checking:
- prefecture and county hierarchy
- route plausibility
- ferry and pass logic
- whether a place naming choice is plausible
- whether a region should feel riverine, dry-road, mountain, or frontier

Query pattern:
- place name in Chinese
- alternate historical place name
- dynasty
- prefecture/county tag

Confidence label usually:
- `geographic lookup`
- `regional framing`

### 4. Institutions, tax, law, exams, military structure, ritual order

Lead sources:
- local skill pack first
- modern scholarship second
- primary text third
- local gazetteers when implementation varies by locality

Use when checking:
- dynasty-sensitive institutions
- household registration systems
- tax and corvee practice
- legal procedure
- exam funnels
- military household or garrison structures

Do not rely on raw primary text alone when lived practice matters.

Confidence label usually:
- `institutional summary`
- `secondary synthesis`

### 5. Social practice, everyday life, gender, commoner experience, public life

Lead sources:
- local skill pack first
- modern scholarship second
- local gazetteers third
- public primary text only as supporting color

Use when checking:
- marriage practice versus legal rule
- widowhood, remarriage, household authority
- market life, artisans, transport labor
- festival rhythm, rumor, public shame
- commoner survival patterns

Confidence label usually:
- `secondary synthesis`
- `low-confidence lead` if only anecdotal or thinly sourced

### 6. Material culture, clothing, housing, food, craft, burial

Lead sources:
- local skill pack first
- museum, archaeology, and university collections second
- modern scholarship third
- dictionary and primary text only for term sanity

Use when checking:
- whether an object, clothing item, building form, or burial practice is plausible
- whether a visual or UI descriptor sounds too generic

Confidence label usually:
- `material-culture reference`
- `secondary synthesis`

### 7. Locality-thickening via gazetteers

Lead sources:
- local gazetteers
- local-history collections
- public-library digitization

Use when checking:
- county texture
- school, temple, market, ferry, bridge, gate, and embankment patterns
- local products and occupational texture
- lineage density or named local elite presence
- local ritual and public-space rhythm

Confidence label usually:
- `local gazetteer`
- `regional framing`

## Query construction rules

Build queries from most stable to most specific:
1. core Chinese term
2. pinyin or alternate romanization if useful
3. dynasty or period
4. region
5. institution or actor class

Examples:
- `zongzu yizhuang late-qing`
- `jiading local-feud ming-qing`
- `county-seat ferry jiangnan`
- `lijia local-practice`
- `junhu jianghu household-status`
- `county gazetteer academy`
- `prefecture gazetteer ferry embankment`

When a term looks unstable, search several forms:
- simplified and traditional
- official title and common label
- Chinese term and pinyin
- historical place-name variants

## Escalation order

Use this escalation order:
1. repo docs and code
2. local skill pack
3. source-specific historical lookup
4. local gazetteers or local archives when place-specific texture matters
5. modern scholarship
6. broad web search only to find better primary or scholarly sources
7. community material only as leads

## Output discipline after search

When search results affect design, text, or naming:
- name the source family
- state dynasty or period when possible
- state region when relevant
- label confidence plainly
- distinguish rule text from lived practice

Do not let a single unsourced web page override the local pack or the repo's own abstractions.
