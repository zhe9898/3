# Northern Song Living Society Footprint

## Goal
Expose the first read-only "living society" footprint for MVP planning: household social pressure plus player influence reach across household, lineage, market, education, yamen/document contact, public life, disorder, and force.

This is a projection slice. It must not become a player-facing route system, career tree, class picker, new authority module, or saved global actor state.

Design directive fused into the specs:
Zongzu should read as a Northern Song Renzong-era living society simulation. The world moves through households, lineages, markets, education, yamen/documents, local governance, military and border pressure, temples/public life, gray disorder, and imperial/court pressure. The player touches only the part their current influence circle can reach.

## Grounding
- local skill pass: `zongzu-game-design` for pressure-chain and bounded-leverage discipline
- local skill pass: `zongzu-ancient-china` for multi-position society, commoner livelihoods, influence circles, and anti-anachronism checks
- external grounding:
  - Columbia Asia for Educators, Song commercial revolution: https://afe.easia.columbia.edu/songdynasty-module/econ-rev-commercial.html
  - Columbia Asia for Educators, Song cities and public commerce: https://afe.easia.columbia.edu/songdynasty-module/cities-new.html
  - Columbia Asia for Educators, scholar-official and exam society: https://afe.easia.columbia.edu/songdynasty-module/confucian-scholar.html
  - Cambridge Journal of Chinese History, Song military institutions: https://www.cambridge.org/core/journals/journal-of-chinese-history/article/military-institutions-as-a-defining-feature-of-the-song-dynasty/D020A447BD8666C3304D7A315CB65DFD
  - Britannica, Renzong: https://www.britannica.com/biography/Renzong
  - Britannica, baojia: https://www.britannica.com/topic/baojia
  - Britannica, Song dynasty government and Northern Song consolidation: https://www.britannica.com/place/China/The-Song-dynasty
  - Britannica, Wang Anshi and later New Policies chronology: https://www.britannica.com/biography/Wang-Anshi
  - Columbia Asia for Educators, emperor, state cult, and bureaucratic interdependence: https://afe.easia.columbia.edu/cosmos/irc/emperor.htm
  - Columbia Asia for Educators, official temples and state ritual at county/prefecture levels: https://afe.easia.columbia.edu/cosmos/irc/temples.htm
  - Encyclopedia.com, Renzong-era Qingli reform pressure and later Wang Anshi continuity: https://www.encyclopedia.com/history/news-wires-white-papers-and-books/song-political-reforms
- latest historical-process supplement:
  - added a source ladder and confidence discipline: period, region, stratum, source confidence, historical claim vs design abstraction vs counterfactual
  - added Renzong-era anchors so the opening can carry imperial legitimacy, scholar-official culture, commercialization, frontier burden, Qingli reform pressure, and later Wang Anshi potential without importing Shenzong-era New Policies as a default
  - shaped great trends as packets: pressure, actor carrier, institutional window, policy bundle, local implementation, capture / resistance, and residue
  - mapped outside-history material back to existing module boundaries rather than adding a hidden global history system
- latest rules-driven supplement:
  - expanded `RULES_DRIVEN_LIVING_WORLD.md` with an explicit rule contract: owning module, cadence, inputs, deterministic resolution, outputs, player leverage, and cause trace
  - defined the rule stack: state, pressure, resolution, transfer, visibility, intervention, and memory
  - added rule-chain examples for heir death, commoner debt slide, frontier pressure, and historical reform
  - clarified that historical process must translate into module-owned rules before it becomes runtime content
- correction after full-doc search:
  - restored the intended high-scale player agency: the player may eventually change history through rebellion, polity formation, succession struggle, usurpation, restoration, or dynasty repair
  - clarified that "not a timeline editor" means no naked menu rewrite, not "history cannot change"
  - connected regime-scale agency to existing multi-route, rebellion/dynasty-cycle, imperial sovereignty, force, office, public-life, and memory rules
- whole-doc linkage pass:
  - updated the main synthesis / entry docs so `FULL_SYSTEM_SPEC`, `CODEX_MASTER_SPEC`, and `README` all mention earned history-change rather than only local pressure
  - updated delivery docs so `IMPLEMENTATION_PHASES`, `EXTENSIBILITY_MODEL`, `POST_MVP_SCOPE`, and `ACCEPTANCE_TESTS` all carry an imperial / dynasty-cycle pack line
  - updated `MVP_SCOPE` to mark regime-scale command play as out of MVP by scope, not out of product vision
- living-world structure roadmap pass:
  - updated `LIVING_WORLD_DESIGN.md` so the static-structure roadmap includes public life, influence footprint, historical trend packets, imperial rhythm, rebellion / polity formation, and dynasty-cycle scaffolding
  - aligned its implementation order with the broader M0-M3 / P1-P5 roadmap while keeping MVP regime-scale play out of scope
  - added rule-density chains for public legitimacy, influence reach, imperial rhythm, historical trends, rebellion-to-polity formation, and dynasty-cycle transformation
- ultra-fine implementation roadmap pass:
  - added `GAME_DEVELOPMENT_ROADMAP.md` as the cross-document implementation index and master map
  - included a Mermaid index graph, phase route table, per-phase ultra-fine implementation steps, pressure-chain build order, and documentation index by job
  - linked the roadmap from `README.md`, `CODEX_MASTER_SPEC.md`, and `IMPLEMENTATION_PHASES.md`

## Scope in
- add contracts for household social-pressure signals and influence-reach snapshots
- let the application presentation builder compose those snapshots from existing module queries and read-model bundles
- split household reach into the player's anchor household and observed household pressure
- keep observed household pressure watch-only; direct outside-household stat commands remain out of scope
- allow the anchor household to carry local-agency summaries without pretending that formal household command surfaces are already implemented
- show lineage / office / public-life / warfare commandability only when existing command affordances exist
- update docs to clarify that "multi-route" language is an architectural pathway lens, not a player route system
- fuse the living-society / influence-circle directive into product, social-strata, player-scope, and multi-route docs
- restore imperial / court pressure as a tenth living-society layer, grounded as distant Renzong-era pressure rather than opening emperor control
- add a historical-process / great-trends doctrine: famous figures, reforms, wars, and policy turns enter as pressure, named-person potential, institutional windows, local implementation, backlash, and memory rather than fixed event rails
- state that the player can be swept up by great trends and can locally carry, bend, accelerate, delay, distort, or resist them through valid influence circles
- add depth bands for historical content: absent, lite, MVP-compatible, full
- add policy-to-module translation for Qingli pressure, Wang Anshi pressure, imperial legitimacy, frontier burden, commercialization, and scholar-official culture
- add a rules-driven acceptance checklist for mechanics, scenario features, historical process, and UI prompts
- require historical-process runtime content to pass through the same rule chain as ordinary households and local institutions
- correct docs that could imply a locked timeline by explicitly allowing earned regime-scale counterfactuals in later packs
- connect the regime-scale doctrine into entry, implementation, extensibility, MVP exclusion, and acceptance-test docs
- bring `LIVING_WORLD_DESIGN.md` into alignment with the living-society, rules-driven, historical-process, and regime-scale agency docs
- add a top-level implementation roadmap / index map so later work has one obvious route into the docs
- add integration coverage for the read-only projection and pack-boundary behavior

## Scope out
- no new authoritative module
- no schema bump
- no saved route tag or `CurrentRoute` field
- no direct household command layer
- no full commoner career system
- no full yamen, temple, market, or military system expansion
- no UI-owned command resolution

## Affected modules
- `src/Zongzu.Contracts`
- `src/Zongzu.Application`
- `tests/Zongzu.Integration.Tests`
- `docs/MVP_SCOPE.md`
- `docs/PRODUCT_SCOPE.md`
- `docs/SOCIAL_STRATA_AND_PATHWAYS.md`
- `docs/PLAYER_SCOPE.md`
- `docs/DATA_SCHEMA.md`
- `docs/MULTI_ROUTE_DESIGN_MATRIX.md`
- `docs/INFLUENCE_POWER_AND_FACTIONS.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/MODULE_BOUNDARIES.md`
- `docs/LIVING_WORLD_DESIGN.md`
- `docs/GAME_DEVELOPMENT_ROADMAP.md`
- `docs/HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
- `docs/README.md`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no module schema bump
- new contracts are runtime presentation read models only
- older saves remain governed by feature manifest and module-envelope compatibility

## Determinism risk
- low
- no RNG draws are added
- projection ordering is deterministic by settlement and household id
- commandability is derived from existing affordance projections

## Milestones
1. Add living-society read-model contracts.
2. Compose household social pressure and influence footprint in `PresentationReadModelBuilder`.
3. Split household reach into anchor-household local agency and observed-household pressure, while routing formal commandability through existing affordances.
4. Add integration coverage.
5. Update docs and this ExecPlan.
6. Run targeted integration tests and a broader M2 presentation regression.

## Verification
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~M2Bundle_SurfacesLivingSocietyPressureAndInfluenceFootprint"`
- broader `M2Bundle` integration test filter
- scoped text search for leftover route-system naming in new contracts / builder / tests

## Verification result
- targeted living-society integration test passed: 1/1
- broader `M2Bundle` integration filter passed: 2/2
- full `Zongzu.Integration.Tests` project passed: 70/70
- anchor-household refinement passed: `OwnHousehold` carries local agency and `ObservedHouseholds` remains non-commandable
- scoped naming search found no leftover `HouseholdRoute*` / `SocialRoute*` / `PrimaryRoute*` symbols in the new code path
