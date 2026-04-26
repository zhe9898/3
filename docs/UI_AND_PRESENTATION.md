# UI_AND_PRESENTATION

This document defines information hierarchy, screens, and interaction responsibilities.

## Information hierarchy
### Tier 1: urgent
Needs immediate player attention:
- death, inheritance, core marriage, legal emergency, critical conflict, campaign collapse

### Tier 2: consequential
Important soon:
- branch tension
- exam result
- trade route loss
- rising outlaw pressure
- local security decline

### Tier 3: background
Useful but non-blocking:
- rumors
- local market drift
- distant kin information
- ambient reports

## Core screens / surfaces

These are not "screens" in the modern UI sense. They are **spatial surfaces** anchored to physical objects in the shell.

| # | Surface | Primary object anchor | What the player touches |
|---|---------|----------------------|------------------------|
| 1 | Great hall / main hall | Notice tray + visitor cushion | Notices, visitor, seal box, almanac |
| 2 | Lineage surface | Ancestral tablet cluster | Tablets, branch ledgers, memorial pile, heir marker |
| 3 | Person inspector | Individual portrait scroll | Scroll to unfurl; lineage relation thread |
| 4 | Household/clan inspector | Household ledger book | Ledger pages, property tokens, debt tally |
| 5 | Macro sandbox / regional board | Desk surface (sand table) | Route strips, county seal, spillover markers |
| 6 | Desk sandbox settlement view | Desk surface (local sand table) | Settlement discs, route threads, focus cluster, notice pin |
| 7 | Ledgers, memorials, route reports | Open book / scroll on desk | Page turns, wax seals, route annotations |
| 8 | Notice tray / notification center | Physical tray or box on great hall table | Pick up notice, read, press seal to acknowledge |
| 9 | Conflict result vignette | Aftermath scroll + casualty tally | Unroll scroll, examine tally, route thread to perpetrator |
| 10 | Campaign board surface later | War overlay on desk sandbox | Front markers, supply-line threads, directive tokens |

### Surface interaction contract
Every surface must obey:
1. **Touch before read** — The player touches a physical object; the information appears in response.
2. **No floating HUD** — Information lives on or near the object that carries it. No omnipresent status bar.
3. **Seal = commit** — Player decisions are confirmed by pressing a seal (lineage seal, office seal), not by clicking "OK."
4. **Thread = connection** — Relationships between entities are shown as physical threads or route lines, not as abstract relationship matrices.
5. **Lane discipline** — Foreground / action / background lanes apply to every surface (see `VISUAL_FORM_AND_INTERACTION.md`).

### Time interaction contract
- the normal playable rhythm is monthly review, monthly interpretation, and monthly bounded command
- day-level state may appear as route heat, market bustle, illness trend, messenger delay, public-life drift, hotspot motion, or xun-labeled almanac summaries, but not as daily turns or three mandatory player turns
- the shell should not expose `advance shangxun`, `advance zhongxun`, and `advance xiaxun` as the main progression buttons
- urgent red-band items may interrupt the month only when the authoritative projection marks them as time-sensitive or irreversible
- interrupt windows should expose only the narrow response justified by the crisis, then return the player to the monthly shell

### Immersion protection rules
- do not solve ordinary play with a wall of sliders
- do not fill the shell with profession labels, career tags, or route badges as if they were the player's permanent identity
- preserve the player as a home-household seat: people are emotional, tactical, and legal entry points, while the continuing play perspective is the household/branch seat
- person surfaces should show how a person feels, carries, or executes pressure for the household; they should not imply that the player has become that person
- do not make every day receipt or xun-labeled summary a separate report
- carry information through physical objects first: rice jar, account book, medicine packet, debt note, study text, road marker, temple gate, county gate, notice tray, seal box
- carry imperial pressure through objects too: edict scroll, appointment notice, amnesty proclamation, mourning cloth, tax / corvee writ, border dispatch, county-gate posting, and yamen docket seal
- monthly review should foreground the pressure closest to the player's current position, not every available metric
- results should echo through later months as receipts, residue, obligation, rumor, debt, trust, shame, or changed reach; avoid immediate abstract rewards such as `+5 credit` as the primary feedback
- a playable shell slice is incomplete until a visible pressure can lead to a readable leverage surface, a bounded command, a module-owned receipt or refusal, and a changed next-month read
- commoner-facing pressure should feel like household life under strain; elite-facing pressure should feel like requests, reputation, obligation, scrutiny, and backlash, not free omnipotent control
- imperial-facing pressure should feel like moral gravity, paperwork arrival, ritual interruption, appointment confidence, fiscal extraction, mercy / punishment tone, and distant court uncertainty reaching the county through mediated channels, not like an emperor-control button

## UI architecture rules
- use view models or read models
- presentation reads projections/query services
- presentation sends commands
- presentation does not hold authoritative logic
- presentation follows `MODERN_GAME_ENGINEERING_STANDARDS.md` §4 Unity Presentation Standards

## Wording lanes
- detailed lane rules, ownership, and authoring workflow live in `WRITING_AND_COPY_GUIDELINES.md`
- player-facing in-world surfaces should use theme-appropriate wording for the setting rather than modern product-dashboard language
- for the current baseline, county, office, and campaign wording should default to a Northern Song-inspired tone: hall, yamen, memorial, route, grain line, gate, ferry, and posted notice rather than generic fantasy bureaucracy
- bootstrap and fixture-fed place / household labels should enter the player-facing shell already grounded as county, market, wharf, ferry, academy, and household names from the same Northern Song-inspired register; do not rely on late-stage replacement of English placeholder seeds
- system actions and general usability labels may stay modern and plain when clarity matters, for example save/load, confirm/cancel, sorting, filtering, and settings
- development and diagnostics surfaces should stay modern, explicit, and engineering-friendly; do not rewrite debug, migration, schema, payload, or inspector language into faux-historical prose
- do not mix the two lanes on one surface: a hall / desk / warfare board should not read like a dashboard, and a debug inspector should not read like a memorial

## Current first-pass implementation note
- the current repository implements a first-pass shell as view-model composition rather than final Unity scenes
- `Zongzu.Presentation.Unity` consumes a read-model bundle exported by application code
- family, exam, trade, settlement, governance-office, warfare-campaign, and notice panels are composed from projections only
- the shell should treat the map stack as sandboxes at multiple scales rather than a modern flat map: a macro sandbox for route / prefecture / regional pressure and a desk sandbox for county-scale lived pressure
- great hall and desk sandbox may now also surface county-public-life summaries such as street talk, county-gate crowding, market bustle, road-report lag, and prefecture pressure from read models only
- the current shell now also carries a first thin player-command layer through read-only affordances and receipts for office and warfare slices; it may show what can be ordered and what was last acknowledged, but command resolution still lives outside UI
- the current shell now also carries a read-only family-council surface, exposing lineage-conflict summaries, clan-memory wording, family command affordances, and recent receipts without adding authority UI
- the lineage surface now also carries read-only `PersonDossiers` and a focused person-inspector object so a lineage tablet / portrait scroll can answer "who is this person?" from projected identity, clan placement, temperament, household livelihood / activity / health, education, clan trade footing, office position, and social-memory context without letting UI infer inheritance, marriage, death, office authority, or command results
- the shell may pass a transient `FocusedPersonId` selection into first-pass composition; this only chooses one existing dossier for the portrait scroll and must fall back safely when the requested person is absent
- stable M2 and later paths may now surface bounded family commands such as elder mediation, branch favor, apology, relief suspension, and branch separation when `FamilyCore` projects them
- the same family-council surface may now also expose thin marriage / heir / mourning lifecycle wording such as `议亲定婚`, `拨粮护婴`, `议定承祧`, `议定丧次`, `承祧未稳`, and `门内举哀`, but those remain `FamilyCore` read models rather than UI-owned rules
- the great hall family line may now also summarize whether a clan is pressed by婚议, 承祧, or 举哀, and that wording must still come from family read models rather than UI-side rule inference
- family lifecycle receipts may now read as household dispositions such as聘财轻重, 入谱定名, and 丧服护持 instead of generic success text, but those phrases still come from `FamilyCore` state/read models rather than UI-owned logic
- great hall and family-council lifecycle summaries may now also carry a read-only directional prompt such as `眼下宜先议定承祧` or `眼下宜先议定丧次`; the shell may choose which bounded family command to highlight from projected affordances, but it still may not resolve or mutate family state inside UI code
- when the lead notice itself is a family-lifecycle notice, the great hall lead-notice guidance and notification-center `WhatNext` text should align to the same read-only directional prompt already chosen from projected lifecycle affordances
- family death notices may now distinguish adult-successor deaths from severe承祧 gaps: the former should lead with丧次 / 祭次 and stabilizing the new承祧, while the latter should lead with议定承祧 and压住房支后议; this is notice / ancestral-hall guidance only, not a full funeral workflow
- violent / warlike deaths that target a clan member may surface both the source conflict notice and the FamilyCore ancestral-hall pressure notice, but both must read from projected traces and must not let UI infer death, succession, or funeral authority on its own
- the MVP lifecycle preview should preserve the same shell-facing loop: death pressure appears as notice / family-council guidance, `议定承祧` or `议定丧次` remains a projected affordance, and command receipts return to the hall without UI mutating authority state
- the read-only office surface now exposes current appointment, current administrative task, petition backlog, petition outcome category, and promotion/demotion pressure wording without introducing any authority controls
- the read-only office surface may now also expose bounded command affordances such as petition review and administrative leverage only when governance-lite is enabled
- the read-only public-life surface now exposes county-public-life summaries on the hall and settlement nodes only; it does not resolve notices, rumors, or county pressure inside UI code
- the read-only public-life surface may now also expose monthly cadence and crowd-mix wording such as fair days, docket-choked county gates, or road-report bustle on the hall and settlement nodes, but that wording still comes from `PublicLifeAndRumor` read models only
- the read-only public-life surface may now also expose venue-channel summaries such as榜示分量、市语流势、查验周折、递报险数 on settlement nodes, but those numbers and summaries still come from `PublicLifeAndRumor` read models only
- the read-only public-life surface may now also expose who says what:榜文如何写、街谈如何传、路报如何失真、州牒如何压下来, and that contention wording must still come from `PublicLifeAndRumor` read models only
- hall / desk settlement nodes may now show bounded public-life affordances and receipts such as `张榜晓谕`, `遣吏催报`, `催护一路`, and `请族老出面`, but those remain read-only UI projections; public-life order verbs resolve through `OrderAndBanditry` while other command lanes must likewise route to their owning module/domain service
- the current playable-thin public-life order closure is complete only when the shell can show pressure, expose a bounded order affordance, receive an order-owned receipt/refusal, and read the next-month governance/order aftermath back from projections; merely showing a public-life read model is not sufficient
- the v3 public-life order shell also carries read-only leverage / cost / readback fields on command affordances, command receipts, hall prompts, and governance docket summaries, so the player can see which household channel is being spent and what may echo next month without Unity resolving authority
- the v4 public-life order shell may also display `社会记忆读回` when `SocialMemoryAndRelations` has persisted public-order residue such as `添雇巡丁` obligation/favor or `严缉路匪` fear/grudge; the shell still copies projected text only and must not create or repair social memory
- the v5 public-life order shell may also display projected refusal / partial readback such as `县门未落地`, `地方拖延`, `后账仍在`, and SocialMemory refusal residue for `添雇巡丁` / `严缉路匪`; shell adapters still copy projection fields only and must not query Order/SocialMemory modules directly
- the v6 public-life order shell may also display projected response affordances and readback for `补保巡丁`, `请族老解释`, `押文催县门`, `赔脚户误读`, `暂缓强压`, and `改走递报`; public-life, governance, and family surfaces should show whether the后账 was repaired, temporarily contained, worsened, or left aside, plus projected shame/fear/favor/grudge/obligation readback
- the v7 public-life order shell may also display later SocialMemory response-residue wording such as `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸`; these are projected SocialMemory readbacks and may not be recomputed in UI or Unity
- the v8 public-life order shell may also display projected actor-countermove readback such as `巡丁自补保`, `脚户误读反噬`, `县门自补落地`, `胥吏续拖`, `族老自解释`, or `族老避羞`; these labels come from owner-module response traces copied through read models, not from shell-side rule evaluation
- the v9 hardening rule for public-life order playability is that residue readback must still lead to bounded, visible response choices with projected availability, leverage, cost, execution, and next-readback text; a shell surface that only exposes hidden command identifiers is not a playable response surface
- the v10 ordinary-household rule is that Desk Sandbox settlement pressure may show how public-life order residue lands on commoner households through night-road fear, runner/watch misunderstanding, labor/debt/migration strain, and yamen delay; this must be copied from `HouseholdSocialPressureSnapshot` projections and must not make Unity query modules or compute response effectiveness
- the v11 ordinary-household play-surface rule is that those same projected household stakes may appear on public-life response affordances and receipts as costed choice text: which household is visibly carrying the后账, what the player is spending, what tradeoff remains, and which owner module resolves it. The shell still copies `PlayerCommandAffordanceSnapshot` / `PlayerCommandReceiptSnapshot` fields only.
- the v12 home-household local response rule is that the same public-life surface may now show `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` as low-power household-seat affordances when projected residue is visible. These choices should read as local household life under strain: night-road avoidance, cash/favor payment to porters, and sending a young household runner for clearer road news.
- v12 receipts may show `本户已缓`, `本户暂压`, `本户吃紧`, or `本户放置`, plus household labor/debt/distress/migration readback. They must not imply that the household repaired county order, the yamen docket, elder shame, or durable social memory by itself.
- v17 local response affordances and receipts may also show `取舍预判`, `预期收益`, `反噬尾巴`, and `外部后账` so the player can distinguish migration avoidance, runner compensation, and road-message choices. These strings must be copied from read models; shell code cannot compute whether the choice works.
- v18 local response receipts may also show `短期后果：缓住项`, `短期后果：挤压项`, and `短期后果：仍欠外部后账` so the player can see what the household locally eased, what got squeezed, and which county/order/family/social-memory after-account remains outside household authority. These strings must be copied from read models; shell code cannot compute whether the choice worked.
- v19 local response affordances may also show `续接提示`, `换招提示`, `冷却提示`, and `续接读回` so the player can see whether repeating or switching a home-household move is light follow-up, risky overpressure, or better cooled down. These strings must be copied from read models; shell code cannot compute final effectiveness or maintain a hidden cooldown ledger.
- v20 local response affordances and receipts may also show `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` so the player can see that home-household response has reached its limit and the remaining repair belongs to Order, Office, Family, or later SocialMemory lanes. Governance lanes / dockets may copy projected county-yamen / document / clerk-delay after-account wording, and family-facing surfaces may copy projected elder-explanation / guarantee wording, but shell code cannot compute owner-lane validity.
- v21 Office/Governance and Family-facing surfaces may now carry that owner-lane return guidance directly on their projected command affordances / docket text: the player should read "本户这头只能缓到这里；催县门/文移 or 请族老解释 must return to the owning lane." The shell still copies fields and must not calculate owner-lane validity.
- v22 owner-lane surfaces may also show projected `承接入口` text naming existing affordance labels such as `添雇巡丁`, `押文催县门`, or `请族老解释`. These labels are plain readback cues, not shell-side routing, ranking, queueing, or validity computation.
- v23 owner-lane surfaces may also show projected `归口状态` text such as `已归口到巡丁/路匪 lane`, `已归口到县门/文移 lane`, or `已归口到族老/担保 lane` when the owning module already has a structured response trace. `归口不等于修好`; shell code must still show this as readback, not as automatic repair or "社会其他人接手".
- v24 owner-lane surfaces may also show projected `归口后读法` text such as `已修复：先停本户加压`, `暂压留账：仍看本 lane 下月`, `恶化转硬：别让本户代扛`, or `放置未接：仍回 owner lane`. Shell code must copy this from DTO fields only and must not compute the owner-lane outcome.
- v25 owner-lane surfaces may also show projected `社会余味读回` text such as `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸` once the later SocialMemory monthly pass has visible residue. Shell code must copy this from DTO fields only and must not compute social residue, query modules, or write SocialMemory.
- v26 owner-lane surfaces may also show projected `余味冷却提示`, `余味续接提示`, or `余味换招提示` text once v25 `社会余味读回` is visible. Shell code must copy this from DTO fields only and must not compute follow-up validity, query modules, or write SocialMemory.
- v27-v30 owner-lane surfaces may also show projected `现有入口读法`, `后手收口读回`, and `闭环防回压` text. Shell code must copy this from DTO fields only and must not compute follow-up validity, query modules, write SocialMemory, maintain a ledger, or hide a household target.
- v46-v52 Office-lane surfaces may also show projected `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, and `Office闭环防回压` text in governance lanes, dockets, Office jurisdiction surfaces, desk settlement nodes, and command receipts. Shell code must copy these from DTO fields only and must not compute Office closure, query `OfficeAndCareer`, parse receipt/event prose, write SocialMemory, maintain any ledger, or hide a household target.
- v53-v60 Family-lane surfaces may also show projected `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` text in public-life, family-facing, governance/docket, desk settlement, and command receipt surfaces. Shell code must copy these from DTO fields only and must not compute Family closure, infer guarantee success, query `FamilyCore`, parse receipt/event prose, write SocialMemory, maintain any ledger, or hide a household target.
- governance lanes / dockets may show whether县门补落地, 胥吏拖延 remains, or the后账 is still present only from projection fields; family surfaces may show whether族老解释缓羞面 or household guarantee remains owed only from projection fields
- no UI, Unity adapter, shell object, or presentation surface may compute whether a public-life refusal response is effective; response outcome comes from the owning module and social residue comes from `SocialMemoryAndRelations`
- the read-only warfare surface now exposes campaign boards, mobilization windows, front labels, command-fit labels, commander summaries, active directive label/summary/trace, bounded route summaries, supply-line summaries, office coordination traces, and aftermath summaries without introducing any authority controls
- the read-only warfare surface may now also expose bounded command affordances such as plan drafting, mobilization, supply-line protection, and barracks withdrawal only when the campaign-enabled path is active
- the read-only warfare surface now also derives board-environment labels, board-surface labels, marker summaries, and atmosphere summaries from front pressure, supply state, morale state, active directive, and route posture so the campaign sandbox no longer reads like one static board
- the read-only warfare surface now also derives regional-profile labels and regional backdrop summaries from existing settlement security/prosperity plus local route naming signals, so a water-linked market county, a hill-route pass, and a flat inland county do not read like the same board
- the read-only warfare surface now also surfaces aftermath-docket summaries for the hall, settlement nodes, and campaign board, so `记功簿`, `劾责状`, `抚恤簿`, and `清路札` can appear as projections only
- player-facing hall / desk / office / warfare wording should avoid modern dashboard or workflow jargon; use hall, yamen, memorial, route, grain-line, petition, and campaign-board language instead
- player-facing public-life wording should read like lived county society: market lanes, county gates, posted notices, ferries, inns, and road reports rather than analytics or dashboard prose
- warfare-lite wording should read like a Chinese-ancient desk-sandbox board: `军务沙盘`, `前线`, `粮道`, `军心`, `号令`, with directives such as `发檄点兵` and `催督粮道`
- player-facing authoritative summaries should be authored in final in-world language at source where practical; do not rely on shell-only replacement passes for module diffs, domain-event summaries, or surfaced state explanations that will later feed notices
- if legacy or fixture-fed English phrases still enter the warfare shell, normalize them at the read-only presentation boundary before they reach hall, desk, or campaign-board surfaces; do not preserve `Registrar`, `campaign board`, `docket traffic`, or escort-logistics prose on player-facing panels
- development-facing debug panels are also composed from read-only projections, now grouped for faster scanning into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings`, and they should keep modern engineering wording
- authoritative simulation state stays inside application/module layers
- command affordances and receipts in the shell are rebuilt from current authoritative state only; they do not authorize, queue, or resolve commands inside UI code
- governance-lite office read models remain optional: when `OfficeAndCareer` is disabled, the shell must surface a read-only "no office projection" equivalent rather than synthesizing office data
- public-life/order read models remain optional by feature pack: when `OrderAndBanditry` is absent, public-life order affordances/receipts must be empty or refused through command routing; the shell must not synthesize order state or command outcomes
- `chain1-public-life-order-v2` treats the desk settlement node and great hall as readback surfaces only: they can show pressure, affordances, receipts, and next-month governance aftermath, but they cannot infer inheritance, death, marriage, office authority, or order command results
- `chain1-public-life-order-leverage-v3` keeps that same rule for leverage/cost/readback: shell adapters copy projected strings only and must fall back to empty text when the projection is absent
- `public-life-order-social-memory-residue-v4` keeps the same projection-only shell rule for durable residue: Unity adapters consume `PresentationReadModelBundle.SocialMemories`, player-command receipts, and governance summaries only; they must not query modules or write SocialMemory state
- `public-life-order-refusal-residue-v5` keeps the same projection-only shell rule for refusal and partial residue: Unity may show projected command receipt and governance readback fields, but it cannot infer missing residue, repair social memory, or compute refusal causes
- `public-life-order-refusal-response-v6` keeps the same projection-only shell rule for response aftermath: Unity may copy projected affordance / receipt / governance / family readback fields, but it cannot query `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, or `SocialMemoryAndRelations`, and it cannot compute `Repaired`, `Contained`, `Escalated`, or `Ignored`
- `public-life-order-residue-decay-friction-v7` keeps the same projection-only shell rule for later response residue: Unity may copy projected social-memory readback and recent command receipts, but it cannot compute whether a residue softened, hardened, or made a later command harder
- `public-life-order-actor-countermove-v8` keeps the same projection-only shell rule for passive back-pressure: Unity may copy projected actor-countermove receipt/readback fields, but it cannot decide whether local actors self-settle, continue delay, escalate, or avoid guarantee
- `public-life-order-ordinary-household-readback-v10` keeps the same projection-only shell rule for commoner pressure: Unity may copy `HouseholdSocialPressures` into settlement pressure readback, but it cannot synthesize household order residue, mutate `PopulationAndHouseholds`, or create a new household command owner
- `public-life-order-ordinary-household-play-surface-v11` keeps the same projection-only shell rule for playable response choices: Unity may display ordinary-household names and stakes copied from public-life command affordances / receipts, but it cannot compute response availability, effectiveness, owner outcome, or durable social residue.
- `public-life-order-home-household-local-response-v12` keeps Unity projection-only even though `PopulationAndHouseholds` now owns three local commands. Unity may display projected home-household affordances and receipts only; it cannot query `PopulationAndHouseholds`, select hidden household targets, recompute `本户已缓` / `本户暂压` / `本户吃紧`, or write local response traces.
- `public-life-order-home-household-social-memory-v13` keeps the shell projection-only for the later social-memory readback: Unity may display home-household receipts that already include `SocialMemoryAndRelations` readback, but it cannot compute shame/fear/favor/grudge/obligation residue, parse `LastLocalResponseSummary`, or write SocialMemory state.
- `public-life-order-home-household-repeat-friction-v14` keeps the shell projection-only for repeat local response friction: Unity may display projected `旧账记忆` / `社会记忆读回` hints on local response affordances and receipts, but it cannot compute whether those memories soften or harden the next local command.
- `public-life-order-common-household-response-texture-v15` keeps the shell projection-only for ordinary household texture: Unity may display projected `本户底色` hints such as debt-heavy, labor-thin, distress-heavy, or migration-prone pressure, but it cannot compute local response cost, select a hidden household target, or resolve whether `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` succeeds.
- `public-life-order-home-household-response-capacity-v16` keeps the shell projection-only for `回应承受线`: Unity may display projected bearable / risky / unfit availability, cost, and readback text for `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信`, but it cannot compute final outcome, query `PopulationAndHouseholds`, write SocialMemory, or invent a hidden household target.
- `public-life-order-home-household-response-tradeoff-v17` keeps the shell projection-only for `取舍预判`: Unity may display projected expected benefit, recoil tail, external-afteraccount boundary, and readback text for `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信`, but it cannot compute final outcome, query `PopulationAndHouseholds`, write SocialMemory, or invent a hidden household target.
- `public-life-order-home-household-short-term-readback-v18` keeps the shell projection-only for `短期后果`: Unity may display projected `缓住项`, `挤压项`, and `仍欠外部后账` receipt text for `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信`, but it cannot compute final outcome, query `PopulationAndHouseholds`, write SocialMemory, or invent a hidden household target.
- `public-life-order-home-household-follow-up-affordance-v19` keeps the shell projection-only for `续接提示`: Unity may display projected repeat/switch/cooldown hints on `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` affordances, but it cannot compute final outcome, query `PopulationAndHouseholds`, write SocialMemory, maintain a cooldown ledger, or invent a hidden household target.
- `public-life-order-owner-lane-return-guidance-v20` keeps the shell projection-only for owner-lane return guidance: Unity may display projected `外部后账归位` fields on public-life affordances/receipts and copied governance/family surfaces, but it cannot compute owner lanes, query modules, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- `public-life-order-owner-lane-return-surface-readback-v21` keeps the shell projection-only while making those copied governance/family fields explicit: Unity may show the Office/Governance `该走县门/文移 lane` and Family `该走族老/担保 lane` readback already present in DTOs, but it cannot compute owner lanes, query modules, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- `public-life-order-owner-lane-handoff-entry-readback-v22` keeps the shell projection-only while adding `承接入口` labels. Unity may show the text already present in DTOs, but it cannot navigate, rank, or execute commands from that prose, compute owner lanes, query modules, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- `public-life-order-owner-lane-receipt-status-readback-v23` keeps the shell projection-only while adding `归口状态` readback. Unity may show the text already present in DTOs, but it cannot compute归口, query Order/Office/Family modules, infer whether the account is repaired, write SocialMemory, maintain an owner-lane or receipt-status ledger, or invent a hidden household target.
- `public-life-order-owner-lane-outcome-reading-guidance-v24` keeps the shell projection-only while adding `归口后读法` readback. Unity may show the text already present in DTOs, but it cannot compute owner-lane outcome meaning, query Order/Office/Family modules, write SocialMemory, maintain an outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- `public-life-order-owner-lane-social-residue-readback-v25` keeps the shell projection-only while adding `社会余味读回` readback. Unity may show the text already present in DTOs, but it cannot compute residue, query SocialMemory or owner modules, write SocialMemory, maintain a SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- `public-life-order-owner-lane-social-residue-followup-v26` keeps the shell projection-only while adding `余味冷却提示` / `余味续接提示` / `余味换招提示` readback. Unity may show the text already present in DTOs, but it cannot compute follow-up validity, query SocialMemory or owner modules, write SocialMemory, maintain a follow-up / SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- `public-life-order-owner-lane-v30-closure-audit` keeps the shell projection-only while adding v27-v29 `现有入口读法`, `后手收口读回`, and `闭环防回压` readback. Unity may show the text already present in DTOs, but it cannot compute closure, query SocialMemory or owner modules, write SocialMemory, maintain a stale-guidance / follow-up / SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- warfare-lite read models remain optional: when `WarfareCampaign` is disabled, the shell must surface `暂无军务沙盘投影。` rather than synthesizing campaign data

## Explainability rule
Every major visible outcome needs:
- what happened
- affected actors
- why it happened
- what the player can do next

## Debug UI minimum
Development builds must expose:
- current seed
- current date
- replay hash
- enabled feature packs
- diff traces
- module state inspectors
- current interaction-pressure summary
- pressure-distribution summary
- runtime scale summary
- payload-summary headline
- named hotspot summary for active local-conflict runs
- top module payload footprint summary
- bootstrap vs save-load origin and any runtime migration steps
- migration consistency summary/warnings
- warning/invariant list
- campaign traces when warfare pack is enabled

Current repository note:
- the first-pass shell now carries a read-only debug panel whose `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings` sections reorganize the same runtime-only diagnostics into scan-friendly developer buckets
- latest-month debug traces, pressure/scale summaries, hotspot summaries, payload headlines/footprints, and migration summaries are runtime diagnostics only and are not part of save compatibility
- v32 event-contract health classifications, the v33 no-unclassified gate, and the v34 owner/evidence backlinks may appear only as developer diagnostics. Player-facing UI and Unity shell surfaces must not compute authority or owner lanes from those diagnostic labels, and must not parse `DomainEvent.Summary`.
- v35 canal-window Trade/Order handling is backend authority only. UI/Unity may display projected market/order state after the modules update, but must not compute canal exposure, route pressure deltas, or owner-lane validity itself.
- v36 household-family burden handling is backend authority only. UI/Unity may display projected FamilyCore lifecycle, charity obligation, support reserve, branch tension, or relief pressure readback after read models expose it, but must not compute sponsor-clan targeting, query `PopulationAndHouseholds` or `FamilyCore`, parse event summaries, or write SocialMemory.
- v37 office/yamen implementation drag is backend authority only. UI/Unity may display projected `OfficeAndCareer` task, backlog, clerk-dependence, petition-outcome, or `PolicyImplemented` readback after read models expose it, but must not compute implementation outcome, query `OfficeAndCareer`, parse event summaries, maintain a policy/yamen ledger, or write SocialMemory.
- v38-v45 office/yamen readback spine is projection authority only. UI/Unity may display projected `OfficeImplementationReadbackSummary`, `OfficeNextStepReadbackSummary`, `RegimeOfficeReadbackSummary`, `CanalRouteReadbackSummary`, and `ResidueHealthSummary` after Application read models expose them, but must not compute implementation effectiveness, infer owner lanes, query modules, parse `DomainEvent.Summary` / receipt prose / `LastPetitionOutcome`, maintain a policy/yamen/owner-lane ledger, or write SocialMemory.
- v46-v52 Office-lane closure is projection authority only. UI/Unity may display projected `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary` after Application read models expose them, but must not compute Office closure, infer owner-lane validity, query modules, parse `DomainEvent.Summary` / receipt prose / `LastPetitionOutcome` / `LastExplanation` / `LastRefusalResponseSummary`, maintain a policy/yamen/owner-lane/receipt/outcome/follow-up ledger, or write SocialMemory.
- v53-v60 Family-lane closure is projection authority only. UI/Unity may display projected `FamilyLaneEntryReadbackSummary`, `FamilyElderExplanationReadbackSummary`, `FamilyGuaranteeReadbackSummary`, `FamilyHouseFaceReadbackSummary`, `FamilyLaneReceiptClosureSummary`, `FamilyLaneResidueFollowUpSummary`, and `FamilyLaneNoLoopGuardSummary` after Application read models expose them, but must not compute Family closure, infer guarantee success, query modules, parse `DomainEvent.Summary` / receipt prose / `LastLocalResponseSummary` / `LastRefusalResponseSummary`, maintain a Family closure / owner-lane / receipt / outcome / follow-up ledger, or write SocialMemory.
- v61-v68 Family relief choice remains projection/display only outside `FamilyCore`. UI/Unity may display the `GrantClanRelief` affordance/receipt and projected `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛` copy after Application read models expose it, but must not compute relief outcome, support cost, sponsor targeting, Family closure, guarantee success, SocialMemory residue, or any relief/owner-lane ledger.
- v69-v76 Force/Campaign/Regime owner-lane readback remains projection/display only. UI/Unity may display projected `WarfareLaneEntryReadbackSummary`, `ForceReadinessReadbackSummary`, `CampaignAftermathReadbackSummary`, `WarfareLaneReceiptClosureSummary`, `WarfareLaneResidueFollowUpSummary`, and `WarfareLaneNoLoopGuardSummary` after Application read models expose them, but must not compute force readiness, campaign closure, military ownership, SocialMemory residue, yamen/Order fallback, or any force/campaign/owner-lane ledger.
- a minimal Unity host shell now also lives at `/unity/Zongzu.UnityShell`; treat it as the scene/asset workspace for hand-built UI, while authoritative simulation and read-model composition remain in `src/`
