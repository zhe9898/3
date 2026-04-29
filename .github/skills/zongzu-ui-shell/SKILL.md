---
name: zongzu-ui-shell
description: Use when working on Zongzu's spatialized shell, Unity presentation layer, shared ViewModel DTOs, great hall, ancestral hall, desk sandbox, notice tray, public-life surface, conflict vignette, campaign-lite board, object-anchored 2.5D presentation, information density, shell read order, or when a UI draft feels like a poster, dashboard, document, generic card wall, or rule-owning frontend instead of a playable living-world surface.
---

# Zongzu UI Shell

## Overview

Use this skill to keep Zongzu UI work aligned to the repo's real product fantasy:
- spatialized lineage simulation
- great hall / study as the monthly decision room
- lineage and ancestral surfaces for memory, branch weight, inheritance, and family pressure
- desk sandbox as a local-world board with nodes, routes, pressure, markers, and bounded commands
- notice tray as projection of structured state changes
- conflict and warfare surfaces as consequence, posture, supply, route, and aftermath, not detached tactics
- Unity shell and adapters as presentation, never authority

Use it to turn broad UI requests into a whole-shell pass that connects:
- surface purpose
- player job
- focal action
- object grammar and 2.5D depth
- read model / ViewModel ownership
- projection and wording lane
- information density
- adapter and Unity-shell implementation seams
- module or read-model source of every displayed fact

## Current Repo Anchors

The current repository has both a pure C# presentation adapter layer and a minimal Unity host root:
- Unity project root: `unity/Zongzu.UnityShell`
- projection/adapters: `src/Zongzu.Presentation.Unity`
- shared ViewModel DTOs: `src/Zongzu.Presentation.Unity.ViewModels`
- presentation tests: `tests/Zongzu.Presentation.Unity.Tests`
- player-command affordance and receipt surfaces now include projected `LeverageSummary`, `CostSummary`, and `ReadbackSummary`
- public-life/order v18 readback may include `PresentationReadModelBundle.SocialMemories`, `HouseholdSocialPressure`, governance recent-receipt summaries, and home-household local response receipt text
- current v19-v492 adds projection-only follow-up hints, owner-lane return/status/outcome/residue/no-loop readback, Office/Family/Force/Warfare/Court process fields, directive-choice readbacks, aftermath docket readbacks, court-policy local-response/SocialMemory/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt echo guard readbacks, social mobility / fidelity-ring / influence readbacks, regime-legitimacy readback, personnel-flow readiness/gate/future-lane preflight wording, commoner/social-position readback, `SocialPositionSourceModuleKeys`, scale-budget readback, household mobility dynamics explanation, v461-v468 closeout governance, v469-v476 household mobility owner-lane preflight, v485-v492 preflight closeout governance, and closeout/preflight audit wording; shell surfaces may show those hints only from projected affordance/readback fields
- current v35-v492 Trade/Order, Family, Office/yamen, Force/Campaign, Warfare, Court-policy, social mobility, regime, personnel-flow, social-position, and household-mobility results/preflights/closeouts are module/projection state or docs/tests governance; shell surfaces may display projected route, market, canal, family, office, force, campaign, aftermath, court-policy, fidelity-scale, mobility/influence, regime, readiness/gate/future-lane preflight readback, social-position source keys, scale-budget summaries, and household mobility explanation, but may not compute route exposure, sponsor targeting, relief success, Office closure, campaign aftermath, court-policy outcomes, SocialMemory residue meaning, public-reading echo, public follow-up cue, docket guard, suggested-action priority, suggested receipt meaning, receipt-docket consistency, public-life receipt echo, social mobility tier, regime legitimacy, personnel-flow target eligibility, social class/status ranking, household movement, route history, event-consumer status, owner-lane outcomes, future household mobility eligibility, or preflight closeout authority
- WCAG 2.2 and Xbox Accessibility Guidelines calibrate contrast, focus/read order, semantic labels, status announcements, and narration parity for shell surfaces; they do not turn the shell into a generic dashboard
- Unity UI performance guidance calibrates shell implementation only: split static/dynamic canvas work when needed, avoid per-frame layout/raycast churn, use precomputed ViewModels/projection contexts, and never scan long simulation histories from a visible surface

Unity Editor MCP / live editor automation is not assumed available unless explicitly configured. Most shell work should be proven first through read-model, adapter, and presentation tests.

## External Calibration Anchors

Use outside shell guidance only after preserving Zongzu's object-anchored surface grammar:
- WCAG/Xbox guidance maps to readable hall/desk/notice objects: contrast, non-color cues, focus order, semantic labels, state values, and narration parity.
- Unity UI optimization maps to bounded display counts, stable anchors, precomputed ViewModels, one-pass projection contexts, and avoiding repeated layout/raycast churn.
- Unity Profiler evidence is for real Unity implementation/frame risks; pure read-model or copy changes usually need adapter/presentation tests instead.
- .NET performance guidance applies to projection builders and adapters when a surface can grow across notices, households, routes, offices, or memories.

## Use This Skill When

- designing or reviewing great hall, ancestral hall, desk sandbox, notice tray, public-life surface, conflict vignette, campaign-lite board, or debug shell
- correcting a mockup that feels like a poster, brochure, dashboard, card wall, or article instead of a playable surface
- translating system state into hall objects, desk objects, notices, ledgers, seals, visitors, route markers, settlement nodes, receipts, and bounded commands
- touching `Zongzu.Presentation.Unity`, `Zongzu.Presentation.Unity.ViewModels`, Unity shell files, projection contexts, or presentation adapters
- aligning UI work with `docs/VISUAL_FORM_AND_INTERACTION.md`, `docs/UI_AND_PRESENTATION.md`, `docs/SPATIAL_SKELETON_SPEC.md`, `docs/WRITING_AND_COPY_GUIDELINES.md`, and `docs/ART_AND_AUDIO_ASSET_SOURCING.md`

## Fast Lane

For small presentation/readback edits, check source read-model, copy-only adapter behavior, authority boundary, density/read order, and the smallest presentation test. Use a full shell pass when a new surface, workflow, object grammar, ViewModel shape, long list, Unity implementation, accessibility claim, or performance risk is involved.

## Workflow

1. Read repo shell intent first.

   Start with:
   - `docs/VISUAL_FORM_AND_INTERACTION.md`
   - `docs/UI_AND_PRESENTATION.md`
   - `docs/SPATIAL_SKELETON_SPEC.md`
   - `docs/WRITING_AND_COPY_GUIDELINES.md`
   - `docs/ART_AND_AUDIO_ASSET_SOURCING.md`

   If implementation is involved, also read:
   - `docs/MODERN_GAME_ENGINEERING_STANDARDS.md`
   - `docs/TECH_STACK.md`
   - relevant files under `src/Zongzu.Presentation.Unity`
   - relevant files under `src/Zongzu.Presentation.Unity.ViewModels`
   - relevant tests under `tests/Zongzu.Presentation.Unity.Tests`
   - Unity shell assets only when the task actually touches the Unity project

2. Identify the surface and player job.

   Determine:
   - which surface this is
   - what the player is supposed to notice first
   - what the player can touch or command
   - what facts are read-only projections
   - what must remain background, inspectable, or debug-only
   - which read model, ViewModel, adapter, or projection context owns the displayed information

3. Load only the references you need.

   - Read [references/shell-principles.md](references/shell-principles.md) for the core shell thesis and anti-poster baseline.
   - Read [references/surface-grammar.md](references/surface-grammar.md) for per-surface layout and object grammar.
   - Read [references/density-and-focus.md](references/density-and-focus.md) for information hierarchy, focal action, and text-density correction.
   - Read [references/similar-game-map-ui-calibration.md](references/similar-game-map-ui-calibration.md) only when outside UI/game comparisons should calibrate map, shell, or spatial interaction structure.

4. Convert style talk into operational structure.

   A good result answers:
   - what this surface is for
   - what the player reads first
   - what the player can touch
   - what the main spatial anchor is
   - what consequence or pressure is being projected
   - what stays secondary, ambient, or debug-only
   - which module/read model owns the information
   - which adapter/ViewModel should carry the display shape
   - what test or screenshot/check should protect the behavior
   - whether repeated shell-side lookup should become a shared projection context or read-only snapshot helper instead of being re-derived in every adapter/test
   - whether the surface needs a bounded display count, one-pass read-model index, virtualized list, pooled Unity objects, or profiler check because it can grow across settlements, notices, households, or route markers

## Short Prompt Expansion

Treat short shell prompts as whole-surface prompts unless the user explicitly asks for a narrow label.

For prompts like `great hall`, `desk sandbox`, `hall surface`, `notice tray`, `2.5D shell`, `poster`, `dashboard`, `Unity shell`, `ViewModel`, or `campaign board`, default to:
- identify the primary surface
- identify player job and focal action
- identify foreground/action/background lanes
- identify object anchors and read order
- identify projection source and authority boundary
- identify what makes the draft too static, too equal-weight, too copy-heavy, too generic-card-like, or too rule-owning

## Output Rules

- Do not treat the first viewport like a marketing poster.
- Do not turn the shell into stacked generic cards.
- Do not turn presentation into a spreadsheet, raw text parser, or document page.
- Do not put authoritative rules, autonomy logic, scheduler logic, or state mutation into UI or Unity code.
- Do not scan all notifications, memories, households, or command receipts from every UI binding or Unity `Update`; build a projection context once and display the result.
- Do not let text volume outrun interaction value.
- Do not solve weak structure by adding more ornament.
- Do not make every surface a giant overview panel.
- Do not use campaign board as the answer for every conflict.
- Do not create Unity-only facts that cannot be traced to projection/read-model state.
- Do allow read-only snapshot or projection-context helpers when they only normalize traversal of an already-built payload, such as finding the current hall-docket lane item, and do not become a second composition layer.
- Do allow narrow notification scope helpers when they only answer "does this existing notification match this settlement/module?" and leave ranking, visibility, and UI policy at the caller.
- Do allow bounded lanes, virtualized/pooled rows, and stable object anchors when projection size grows, as long as selection policy remains traceable to read-model fields.
- Prefer room, desk, notice, ledger, seal, route, marker, tray, and visitor logic over abstract dashboard blocks.
- Prefer one strong focal action cluster over many equal buttons.
- Prefer visible object anchors over pure panel geometry.
- Prefer read models and ViewModels over direct module access.
- Use 2.5D depth to clarify ownership, reach, and consequence, not to add spectacle.

## Zongzu-Specific Guidance

- Great hall is the main monthly decision room, not a hero page.
- Ancestral hall is for lineage memory, branch weight, elder memory, and inheritance context, not a second dashboard.
- Desk sandbox is a local-world board with topology, routes, pressure, visibility, and reach, not a giant minimap or static infographic.
- Notice tray separates urgent, consequential, and background pressure cleanly.
- Public-life surfaces show reputation, rumor, visibility, and public pressure without becoming free-form prose.
- Public-life/order/court-policy/regime/social-mobility/social-position/household-mobility shell surfaces may explain household leverage, command cost, partial/refused landing, SocialMemory residue, household local response, v19-v492 follow-up/owner-lane/Office/Family/Force/Warfare/Court readback, policy local response, memory-pressure, public-reading echo, public follow-up cue, docket guard, suggested-action guard, suggested-receipt guard, receipt-docket consistency guard, public-life receipt echo guard, fidelity-scale/mobility influence, regime-legitimacy readback, personnel-flow readiness, owner-lane gate, future-lane preflight, social-position sources, scale-budget detail-vs-summary, household mobility dimensions, household mobility owner-lane preflight, and preflight closeout guidance, but only by displaying projected read-model fields.
- shell density should be bounded by role and locus: prioritize active household/settlement/office context, collapse distant pressure into summaries, keep lists capped or paged, and never scan simulation state per frame or per hover
- accessibility is part of shell correctness: important status must have text/narration equivalents, non-color cues, readable contrast, stable focus order, and no meaning hidden only in decorative spatial objects
- Conflict vignette should feel like aftermath and consequence.
- Campaign-lite board is route, front, posture, supply, and aftermath pressure; it is later and scale-gated.
- Debug panels may expose internals, but player-facing surfaces should turn state into readable consequence and bounded action.
- Strong Zongzu UI should read as a lived shell with desks, halls, reports, nodes, and pressure, not an illustrated poster with text pasted onto it.
