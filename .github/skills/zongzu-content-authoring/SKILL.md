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
- current public-life/order readback wording is downstream projection text; examples include `社会记忆读回`, `县门未落地`, `地方拖延`, `后账仍在`, `续接提示`, `换招提示`, `冷却提示`, `续接读回`, `外部后账归位`, `承接入口`, `归口状态`, `归口后读法`, `社会余味读回`, `现有入口读法`, `后手收口读回`, and `闭环防回压`
- generated Unity art/content under `unity/Zongzu.UnityShell/Assets/Art/Generated` needs source/provenance manifests and `.meta` discipline, but it must not become simulation authority
- authored content and generated assets need cardinality discipline: large tables, descriptor banks, image sets, or localization surfaces should declare stable IDs, provenance, validation path, and whether the runtime reads them as rules-data or presentation-only content

Player-facing text should enter as source/module/read-model wording when practical; do not rely on Unity-only replacement passes to make authoritative summaries setting-appropriate. Debug/migration/diagnostic wording may stay modern and explicit.

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
- Do not infer accepted, partial, refused, repeated, switched, or cooled-down outcomes from prose; use structured owner-module trace fields, household snapshots, and SocialMemory read models.
- Do not let content packs bypass module ownership, schema, manifest, or migration rules.
- Do not use a large descriptor table as a disguised event pool; if code reads it, it needs stable IDs, validation, bounds, and an owner.
- Do not make narrative notifications the cause of state changes.
- Prefer descriptors that expose pressure, actor incentives, place, office, memory, and consequence.
- Prefer historically grounded language over modern administrative or genre shorthand.
- Keep debug copy and player-facing copy separate when possible.
- Do not turn a copy-only task into a schema or simulation task unless the artifact is read by code.
