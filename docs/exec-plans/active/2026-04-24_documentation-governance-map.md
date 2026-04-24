# Documentation Governance Map

Date: 2026-04-24

## Goal

梳理现有项目文档，把已经成体系的产品、架构、历史、UI、schema、验收和执行文档整理成可导航、可提交、可继续扩展的文档入口。

本任务不重写世界观，不收窄到 MVP，不改变任何模块权威规则。目标是让人和 Codex 都能更快回答：

- 哪个文档是权威入口？
- 哪个文档只是综合摘要或执行记录？
- 做某类任务前应该读哪些文档？
- 从文档承诺到实现闭环应该怎么走？

## Scope in

- 新增文档地图 / 文档权威层级说明。
- 更新 docs 入口读序。
- 将 roadmap 中的 job-based 文档索引指向新的文档地图。
- 记录本次梳理的 save/schema/determinism 影响。

## Scope out

- 不修改产品设计承诺。
- 不修改模块边界。
- 不修改 save schema。
- 不修改测试验收语义。
- 不移动或拆分大型既有文档。
- 不改代码。

## Affected modules

None.

Touched documentation:

- `docs/DOCUMENTATION_MAP.md`
- `docs/README.md`
- `docs/GAME_DEVELOPMENT_ROADMAP.md`
- `AGENTS.md`
- `docs/exec-plans/active/2026-04-24_documentation-governance-map.md`

Related existing uncommitted slice included in the final verification/commit:

- `docs/exec-plans/active/2026-04-24_person-dossier-depth-ladder.md`
- person dossier read-model, Unity-facing DTO, adapter, tests, and docs already covered by that ExecPlan.

## Query / Command / DomainEvent impact

None.

## Save/schema impact

None. This is documentation-only and does not add persisted state, module schema versions, migrations, feature packs, or read-model fields.

## Determinism risk

None. No runtime code, scheduler cadence, event ordering, RNG use, or simulation state is changed.

## Unity/presentation boundary impact

No Unity code or assets are changed. Presentation docs are only indexed and categorized.

## Milestones

- [x] Read relevant skill workflows.
- [x] Inspect current git status and docs inventory.
- [x] Identify doc authority tiers and task-based read paths.
- [x] Add documentation map.
- [x] Update docs entry points.
- [x] Run formatting/check commands.
- [ ] Commit full current change set.

## Tests to run

- `git diff --check`
- Documentation inventory/link sanity check for repo docs.

No dotnet test is required for this documentation-only task.

## Rollback / fallback plan

If the map proves too heavy, keep `docs/DOCUMENTATION_MAP.md` as the single new authority/navigation layer and revert only entry-point cross-links.

## Open questions

- Whether completed documentation ExecPlans should be immediately archived or remain active until a later docs closeout pass.

## Verification result

- `git diff --check`: passed.
- Changed-docs mention sanity check: passed for this task's entry-point docs.
- `dotnet test Zongzu.sln --no-restore /p:UseSharedCompilation=false`: passed.
