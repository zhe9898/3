# MVP_SCOPE

This document defines the first shippable vertical slice.

For the living-world structure that this slice implements, see `LIVING_WORLD_DESIGN.md`.
For the development route and phase ownership, see `GAME_DEVELOPMENT_ROADMAP.md`.
For the rules-driven thesis, see `RULES_DRIVEN_LIVING_WORLD.md`.
For modern game-engineering standards (code, module, system, Unity, content), see `MODERN_GAME_ENGINEERING_STANDARDS.md`.

## MVP question
The MVP exists to answer one question:

**Can a living local Northern-Song society run month by month, with bounded player intervention, readable consequences, and a spatial shell that feels like acting from one situated household / lineage / local-institution position rather than as an omnipotent world owner?**

If the answer is yes, the MVP is successful.

## MVP feature packs

### Mandatory kernel/infrastructure
- `Kernel`
- `Contracts`
- `Scheduler`
- `Application`
- `Persistence`
- `NarrativeProjection`
- `Presentation.Unity` shell

### Mandatory gameplay modules
- `WorldSettlements`
- `FamilyCore`
- `PopulationAndHouseholds`
- `SocialMemoryAndRelations`
- `EducationAndExams.Lite`
- `TradeAndIndustry.Lite`

Default MVP bootstrap/load paths should keep this mandatory set plus `NarrativeProjection` only, and should not silently enable optional public-life, conflict, governance, or warfare packs.

### Conditional MVP-plus modules
Include only if they fit schedule without harming the above:
- `OrderAndBanditry.Lite`
- `ConflictAndForce.Lite`

### Explicitly not in MVP
- `OfficeAndCareer` full
- `WarfareCampaign`
- multi-region strategic play
- imperial / dynasty-cycle command play
- rebellion-to-polity formation
- succession struggle, usurpation, restoration, or dynasty repair
- full outlaw faction play
- advanced institution politics
- free-roam exploration
- tactical battle maps

These exclusions are scope cuts, not product denials.
The long-run design may still allow history-changing play at larger scale when later packs provide the necessary rule chains.

## MVP player-facing pillars
1. monthly review and command
2. family tree and branch management
3. marriage, birth, death, inheritance pressure
4. education/exam pressure in light form
5. local trade/economy in light form
6. memories, grudges, and visible consequences
7. read-only household social-pressure and player influence-footprint projection, so the shell can show which social layers are moving without turning them into jobs, skill trees, or direct player buttons
8. spatial shell:
   - great hall / study
   - ancestral hall / lineage surface
   - desk sandbox
   - conflict vignette if conflict pack is enabled

## MVP time UX contract

The MVP should feel monthly to the player even though the authoritative simulation may use three internal xun pulses.

Required:
- one normal player-facing review per month
- one normal bounded command window per month
- xun movement summarized as trend, pressure, cause trace, and urgency
- interrupt windows only for red-band or irreversible items such as death, flight, violence, office seizure, route collapse, or disaster impact

Forbidden for MVP:
- three routine player turns per month
- separate `advance shangxun / advance zhongxun / advance xiaxun` buttons as the main loop
- showing every xun receipt as equal-weight notification spam
- letting interrupts become the default play rhythm

The intended feel is:
`month begins -> world breathes internally -> urgent exceptions may surface -> month-end review -> bounded player command -> next month carries residue`.

## MVP immersion contract

The MVP should protect immersion by making pressure legible through objects and delayed consequences.

Required:
- household pressure should be readable through object-like anchors such as grain, account/debt, medicine, study, road, gate, and notice surfaces
- monthly review should foreground the one or few pressures closest to the player's current position
- command results should return as receipts and next-month residue rather than instant abstract score bumps
- commoner starts should feel like managing rice, illness, debt, labor, travel, study cost, kin help, and institutional contact under pressure
- elite starts should feel like handling requests, reputation, obligations, scrutiny, factional resentment, public face, and backlash
- imperial pressure in MVP should be watch-only and object-mediated: posted notices, yamen docket pressure, tax / corvee language, relief or amnesty rumor, border dispatch, and appointment atmosphere may appear, but the player does not command the court

Forbidden for MVP:
- primary interaction built around many exposed sliders
- full-screen profession / career labels as the player's identity model
- showing every xun pulse as a separate report
- using bare `+5 credit`, `+10 reputation`, or similar abstract score text as the main player-facing consequence
- emperor buttons, edict editing, throne commands, or direct court control

## MVP command inventory
The following bounded commands must be available in the MVP shell. They are not omnipotent; each resolves against autonomy, institution, logistics, and risk.

### Family / clan commands
| Command | Preconditions | Cost / Risk | Typical receipt |
|---------|--------------|-------------|-----------------|
| `议定承祧` (heir designation) | Adult death with unsettled succession; eligible heir exists | Prestige, branch tension | `入谱定名` or `承祧未稳` |
| `议定丧次` (funeral order) | Death in household | Cash, labor, public face | `丧服护持` |
| `议亲定婚` (arrange marriage) | Eligible unmarried adult; match available | Bride-price / dowry, branch relations | `聘财轻重` |
| `拨粮护婴` (allocate grain for infant) | Birth in household; household grain below threshold | Grain stock, labor | `入谱定名` |
| ` elder mediation` (branch dispute) | Branch tension above threshold | Prestige, time | Tension reduced or deferred |
| `branch favor` (allocate support) | Surplus grain or cash; distressed branch | Resources | Obligation created or deepened |

### Trade / household commands
| Command | Preconditions | Cost / Risk | Typical receipt |
|---------|--------------|-------------|-----------------|
| ` Guarantee debt` | Trusted party requests backing; player credit sufficient | Credit exposure, future liability | `保状留底` |
| `Fund study` | Exam aspirant in household; academy access exists | Cash, household labor | `束脩已纳` or `学业不继` |
| `Escort route` | Trade route unsafe; escort capacity available | Labor, cash, casualty risk | `一路平安` or `折损报告` |
| `Invest in estate/shop` | Available cash; settlement node with capacity | Cash, time | Profit/loss receipt next season |

### Public / local commands
| Command | Preconditions | Cost / Risk | Typical receipt |
|---------|--------------|-------------|-----------------|
| `Petition yamen` | Grievance or request; document capacity | Public face, time, possible dismissal | `县准` / `县驳` / `悬案` |
| `Recommend someone` | Known person with merit; player reputation sufficient | Reputation, future obligation | `荐书已递` |
| `Endure / defer` | Any pressure situation | Opportunity cost, possible escalation | `暂压` (pressure deferred, not resolved) |

### What is NOT an MVP command
- Direct appointment or dismissal of officials
- Military campaign planning
- Tax-rate editing
- Free land redistribution
- Direct household command (other households are autonomous)
- Dynasty-level edict issuance

## MVP pressure-chain examples
These walkthroughs show what a player should experience in one month, proving the "world moves first, player responds second" design.

### Example 1: Death → 承祧 gap → notice → command → receipt
1. **上旬 pulse:** Elderly head of household falls ill; household strain rises.
2. **中旬 pulse:** Illness worsens; branch tension begins to surface.
3. **下旬 pulse:** Death. `FamilyCore` emits `DeathEvent` → triggers inheritance check → finds no confirmed heir.
4. **Month-end diff:** Death recorded; heir status = `UNSETTLED`; branch tension +15; mourning obligation registered.
5. **Projection:** Great hall notice tray receives urgent notice: `张公殁，承祧未定，房支浮动`. Ancestral hall memorial pile adds one marker.
6. **Player review:** Player sees `眼下宜先议定承祧` directional prompt.
7. **Player command:** Issues `议定承祧`, selecting eligible heir from branch ledger.
8. **Command resolution:** `FamilyCore` validates heir eligibility (age, gender, branch rules), updates heir status, emits `HeirConfirmedEvent`.
9. **Receipt:** `入谱定名` — heir recorded in lineage register.
10. **Next month:** Branch tension begins decay; mourning rites continue; lineage surface shows settled heir.

### Example 2: Harvest failure → debt pressure → trade route risk
1. **Seasonal band:** Drought in summer → autumn harvest `HarvestWindowProgress` stalls at 30%.
2. **上旬 pulse:** Household grain consumption exceeds safe threshold.
3. **中旬 pulse:** Household seeks credit; `TradeAndIndustry` records debt obligation.
4. **下旬 pulse:** Debt servicer demands repayment; route to market unsafe due to bandit hotspot.
5. **Month-end diff:** Household debt +20; grain stock critical; route `r.lx-grain-south` reliability drops to 35.
6. **Projection:** Desk sandbox shows `南渡津` node in red; great hall notice: `粮道吃紧，佃户浮动`.
7. **Player options:**
   - `Escort route` — spend cash and labor to secure grain movement
   - `Guarantee debt` — leverage lineage credit for household
   - `Endure` — accept risk of tenant flight next month
8. **If player escorts:** `TradeAndIndustry` resolves escort against route reliability → if successful, grain arrives; if failure, casualty + deeper debt.

### Example 3: Exam failure → social drift → household return
1. **Seasonal band:** Autumn exam season arrives.
2. **上旬 pulse:** Aspirant travels to county seat; `EducationAndExams` resolves attempt.
3. **中旬 pulse:** Result: failure. Aspirant returns home.
4. **下旬 pulse:** Household reabsorbs labor; aspirant mood = `withdrawn`.
5. **Month-end diff:** Exam attempt recorded; household labor +1; study investment sunk.
6. **Projection:** Ancestral hall shows `科场未第`; household ledger shows sunk cost.
7. **Player options:**
   - `Fund study` for next cycle — expensive, uncertain
   - Redirect aspirant to trade/accounting — immediate labor gain, but exam path closed
   - `Endure` — aspirant may drift away or re-attempt autonomously

## MVP slice boundary lock
The following items are **explicitly inside** the MVP slice and must be playable:
- [ ] One complete family lifecycle (birth → marriage → death → succession → next generation)
- [ ] One exam cycle (study → attempt → pass or fail → consequence)
- [ ] One trade cycle (invest → operate → profit or loss → debt or expansion)
- [ ] One pressure-chain walkthrough from world pulse to player command to receipt to next-month state
- [ ] One grudge formation and visible consequence (insult → memory → later retaliation or mediation)
- [ ] Spatial shell with all four surfaces (great hall, ancestral hall, desk sandbox, macro sandbox)
- [ ] Save/load roundtrip preserving at least 5 years of play

The following items are **explicitly outside** the MVP slice and must not block ship:
- Multi-region strategic play
- Full office module
- Full conflict module
- Campaign warfare
- Dynasty-cycle mechanics
- Tactical battle maps
- Free-roam exploration
- CJK font asset in Unity (English placeholder acceptable for MVP)

## MVP ignored tests (intentional non-goals)
These are things the MVP deliberately does not solve. They are not bugs or cut corners; they are scope boundaries.

1. **No cross-region play** — The MVP is single-county. Multi-region travel exists as route pressure and rumor only.
2. **No full office career** — Office exists as projection and petition surface only. Appointment, promotion, demotion are out of scope.
3. **No tactical combat** — Conflict is abstract (injury, raid, casualty marker). No unit micro, no battle map.
4. **No player-as-emperor** — Imperial pressure arrives through notices and yamen documents only. No edict issuance.
5. **No free market manipulation** — The player can invest, guarantee, and escort, but cannot set prices, create routes, or abolish taxes.
6. **No full CJK text rendering** — Unity TMP CJK font asset is post-MVP. English placeholders or system-font UI.Text are acceptable.
7. **No multiplayer or networked play** — Deterministic single-player only.

**Scope-cut framing:** These exclusions are temporal scope cuts for the first slice, not design declarations that these topics are unimportant. Law, religion, local culture, office conflict, lineage coercion, material life, and warfare remain first-class system topics in the product architecture and must re-enter as soon as module depth permits.

## MVP scope by pathway

### Family / clan
Must be robust and complete enough to carry the game.

### Commoner / household base
Must exist as the labor, migration, pressure, and support layer.
The MVP may project household drift such as holding steady, sliding into debt, entering the market, seeking lineage protection, preparing to migrate, contacting yamen paperwork, or being pushed toward disorder. These are derived pressure readings, not stored career rails and not a separate player route system.

### Exams
Must exist in light form:
- study state
- academy availability
- exam attempts
- pass/fail outcomes
- explanation traces

### Trade
Must exist in light form:
- estates and shops
- local routes
- cash/grain/debt pressure
- local profit/loss explanation

### Office
Only indirect effects through institutions and titles if needed.
No full office module yet.

### Outlaw / banditry
Only if the lite pack fits:
- local security pressure
- raid/disorder risk
- some actors falling into outlaw/bandit states
- no fully managed outlaw ecosystem

## MVP quality bar
The MVP must already provide:
- deterministic 20-year headless run
- playable 10-year interactive run
- valid save/load roundtrip
- no impossible kinship states
- readable cause traces for major outcomes
- readable social-pressure and influence-footprint summaries that do not imply the player is always a clan head or can directly command every household
- no event-pool core loop
- no module boundary violations

## MVP cut rules
Cut any feature that does not directly strengthen:
- lineage pressure
- commoner/household pressure
- local institutions
- trade/exam upward mobility
- grudge persistence
- bounded command play
- readable explanation
