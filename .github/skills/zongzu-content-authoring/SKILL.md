---
name: zongzu-content-authoring
description: "Use when authoring, reviewing, or organizing Zongzu content artifacts such as historical carriers, Chinese narrative copy, authored configs, data tables, settlement/clan/office/rumor/warfare descriptors, projection wording, content packs, localization-facing text, or content validation while keeping content from becoming authority rules. Do not use for pure historical research unless a content artifact changes."
---

# Zongzu Content Authoring

## Overview

Use this skill to keep Zongzu content historically grounded, system-readable, and downstream from authority.

Content should give the living world texture and legibility. It should not secretly become an event pool, hidden rules engine, or detached scripted timeline.

Use it when content is being written, edited, structured, or validated. For conceptual history or anti-anachronism review without a content artifact, use `zongzu-ancient-china`.

## Current Repo Anchors

Current content-bearing surfaces include:
- docs and specs under `docs/`
- authored/generated content under `content/`
- player-facing module summaries, event summaries, projection copy, and ViewModel labels in `src/`
- Unity shell labels/assets under `unity/Zongzu.UnityShell` only when presentation assets are actually touched
- current public-life/order and global thin-chain readback wording is downstream projection text; examples include `社会记忆读回`, `县门未落地`, `地方拖延`, `后账仍在`, `续接提示`, `外部后账归位`, `承接入口`, `归口后读法`, `社会余味读回`, `后手收口读回`, `闭环防回压`, `Office承接入口`, `Family救济选择读回`, `不是普通家户再扛`, `军令选择读回`, `战后案卷读回`, `朝议压力读回`, `政策局面读回`, `政策回应选择读回`, `政策回应余味续接读回`, `政策旧账回压读回`, `政策公议旧读回`, `政策公议后手提示`, `政策后手案牍防误读`, `建议动作防误读`, `建议回执防误读`, `回执案牍一致防误读`, `公议回执回声防误读`, `PublicLife只读街面解释`, `县门承接仍归OfficeAndCareer`, `Court-policy防回压`, `天命摇动读回`, `去就风险读回`, `公议向背读法`, `精度环`, `影响足迹读回`, `流动池读回`, `人员命令预检`, and `本户不再代修`
- current personnel-flow wording such as `人员流动预备读回`, `人员流动命令预备汇总`, `人员流动归口门槛`, `人员流动未来归口预检`, `FamilyCore未来归口`, `OfficeAndCareer未来归口`, and `WarfareCampaign未来归口` is projected readiness/gate/preflight wording only; it must not become a direct move, transfer, summon, assignment, migration, office-service, kin-transfer, or manpower command
- current v381-v452 social-position wording such as `社会位置读回`, `社会位置来源模块`, `社会位置精度预算读回`, `近处细读、远处汇总`, `区域汇总`, `住户/客户`, and `普通民户身份归口预检` is projected readback/preflight wording over structured owner-module snapshots; it must not become prose-owned rank, a universal class ladder, a hidden status ledger, a `PersonRegistry` expansion, or direct promote/demote control
- current v453-v460 household mobility dynamics wording such as `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` is runtime projection over existing household pressure signals; v461-v468 closes it as docs/tests governance only; v469-v476 adds owner-lane preflight for future rule depth. These words must not become prose-owned class/status authority, a household-mobility ledger, a route history store, or UI/Unity movement logic
- current canal/route/order wording such as `漕渠窗口`, `商路读数`, `巡丁`, `私路`, `护路`, and `路面压力` should remain projection or diagnostic wording unless a module-owned rule explicitly reads structured data for it
- generated Unity art/content under `unity/Zongzu.UnityShell/Assets/Art/Generated` needs source/provenance manifests and `.meta` discipline, but it must not become simulation authority
- authored content and generated assets need cardinality discipline: large tables, descriptor banks, image sets, or localization surfaces should declare stable IDs, provenance, validation path, and whether the runtime reads them as rules-data or presentation-only content
- content scale is a performance and design issue: authored tables should avoid hidden formulas in prose, unbounded tag combinations, locale-length breakage, duplicate semantic IDs, and per-frame text parsing
- historical flavor should be decomposed into stable keys, tags, bands, and projection wording so localization and source calibration stay downstream of module authority

Player-facing text should enter as source/module/read-model wording when practical; do not rely on Unity-only replacement passes to make authoritative summaries setting-appropriate. Debug/migration/diagnostic wording may stay modern and explicit.

## External Calibration Anchors

Use external material to improve content operations, not to make prose authoritative:
- Unity asset metadata and project-organization guidance calibrate generated art/content handling: keep `.meta` files, provenance, naming, and folder intent stable.
- WCAG/Xbox accessibility guidance calibrates player-facing copy and labels: important meaning needs contrast, non-color cues, semantic labels, and narration-friendly summaries.
- Historical source calibration should record period, region, source confidence, and abstraction level when a descriptor becomes a pressure carrier or player-facing label.
- Performance guidance applies to content scale: large descriptor banks should parse once, validate deterministically, expose stable IDs, and avoid becoming an unbounded runtime event pool.

## Use This Skill When

- authoring or reviewing projection copy, Chinese labels, descriptors, notices, glossary text, or localization-facing strings
- changing authored config/data, content packs, generated text inventories, generated art manifests, or Unity-facing labels/assets
- deciding whether a content table is presentation-only or rules-data read by code
- preserving UTF-8 Chinese text, provenance, stable IDs, validation paths, and load/cardinality expectations
- content wording describes implemented pressure chains, court-policy readbacks, public-life rumors, offices, settlements, clans, campaigns, or routes

## Fast Lane

For copy-only edits, check UTF-8 preservation, authority boundary, historical tone, and surface fit without inventing runtime tests. Use a full content pass when code reads the artifact, content affects rules-data, generated assets/manifests change, or descriptor banks can grow large.

## Workflow

1. Identify the content artifact.

   Confirm what is changing:
   - authored config/data
   - projection or notification copy
   - descriptor text
   - content pack structure
   - localization-facing text
   - content validation rules

   If no artifact is changing, answer through the narrower design or historical skill instead.

2. Read the content frame needed.

   Start with:
   - `docs/WRITING_AND_COPY_GUIDELINES.md`
   - `docs/DATA_SCHEMA.md`
   - the active ExecPlan for the content surface

   Add product, social-strata, visual, UI, asset, or static-backend docs only when the artifact touches those concerns.

   Pair with `zongzu-ancient-china` for historical grounding, `zongzu-game-design` for playable pressure, `zongzu-pressure-chain` when copy describes an implemented chain, and `zongzu-unity-shell` when the artifact is a Unity-facing label/asset/binding.

3. Classify the content.

   Decide whether it is:
   - authored configuration that modules read deterministically
   - narrative/projection wording for facts that already happened
   - historical carrier or pressure source
   - clan, settlement, office, venue, route, or campaign descriptor
   - UI label/copy
   - debug/diagnostic wording
   - future content-pack material
   - generated asset or generated-text inventory that needs provenance and promotion rules

4. Name the authority boundary.

   For every content item, identify:
   - which module, projection, or adapter reads it
   - whether it can affect simulation or only presentation
   - what schema or validation protects it
   - what happens if the content is absent or malformed
   - whether it needs localization or Chinese text preservation
   - expected count/size and whether it can be loaded, indexed, or filtered without turning into a runtime event pool

5. Keep historical content playable.

   Historical carriers should become:
   - pressure windows
   - institutional incentives
   - social constraints
   - route or office conditions
   - rumor/public-life visibility
   - player-readable consequences

   They should not become:
   - fixed cutscene timelines
   - omniscient event cards
   - guaranteed outcomes
   - modern labels pasted onto premodern society

6. Validate content.

   Use structured validation where available.
   If code reads the content, add or update tests.
   If wording changes player-facing interpretation, check it against UI shell surface purpose and historical grounding.
   For copy-only edits with no parser, schema, or code reader, do not invent tests; use a focused wording/encoding review.

## Output Rules

- Preserve existing Chinese text exactly unless explicitly editing the wording.
- Use UTF-8 and treat mojibake as a real bug.
- Do not put authoritative rules in prose.
- Do not infer accepted, partial, refused, repeated, switched, cooled-down, Office-closed, Family-guaranteed, relief-granted, military, aftermath, court-policy, public-reading, memory-pressure, regime-legitimacy, social-mobility, influence, fidelity-scale, social-position/status, personnel-flow readiness, owner-lane gates, future-lane preflight, or personnel-command outcomes from prose; use structured owner-module trace fields, household snapshots, jurisdiction snapshots, FamilyCore snapshots, WarfareCampaign snapshots, Office/PublicLife snapshots, Population/PersonRegistry snapshots, and SocialMemory read models.
- Do not let content packs bypass module ownership, schema, manifest, or migration rules.
- Do not use a large descriptor table as a disguised event pool; if code reads it, it needs stable IDs, validation, bounds, and an owner.
- Do not make localization, generated-text inventories, or asset manifests grow without provenance, expected count, load path, and validation ownership.
- Do not make narrative notifications the cause of state changes.
- Prefer descriptors that expose pressure, actor incentives, place, office, memory, and consequence.
- Prefer historically grounded language over modern administrative or genre shorthand.
- Keep debug copy and player-facing copy separate when possible.
- Do not turn a copy-only task into a schema or simulation task unless the artifact is read by code.
