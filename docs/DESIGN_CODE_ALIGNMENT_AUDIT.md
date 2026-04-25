# DESIGN_CODE_ALIGNMENT_AUDIT

## 当前对齐快照（2026-04-25）

本文是当前文档/代码对齐页，不再内嵌早期历史审计全文。旧的 `codex/mvp-default-path-closeout @ 1164241` 基线已经被后续模块和测试进度覆盖；如需考古，用 git 历史追溯，比在当前规范里保留过期失败清单更清楚。

当前实现事实：
- 仓库当前主线是模块化单体：`Zongzu.Application` 组合模块，`Zongzu.Scheduler` 负责确定性月度/xun 调度，模块通过 Query / Command / DomainEvent 协作。
- `PersonRegistry` 已作为 identity-only 模块存在；`FamilyCore` 死亡事件已转为 `ClanMemberDied`，并由 registry 汇总为人物死亡事实。
- `PublicLifeAndRumor`、`OfficeAndCareer`、`ConflictAndForce`、`WarfareCampaign` 等 lite/后续切片已按独立模块和独立 state namespace 接入。
- `MonthlyScheduler` 已有 prepare phase、三旬推进、月末推进、有限事件 drain、projection-last 顺序。
- `Zongzu.Presentation.Unity.ViewModels` 与 `Zongzu.Presentation.Unity` 是只读 presentation seam；Unity host shell 已存在于 `unity/Zongzu.UnityShell`，但权威模拟仍在 `src/`，且模拟项目不得引用 Unity API。
- 当前 SDK 基线以 `global.json` 和项目文件为准，现为 .NET SDK `10.0.202` 线。
- 当前分支已落地 public-life/order v3/v4/v5/v6/v7/v8/v9/v10 闭环：v3 为 runtime-only leverage/cost/readback projection，v4 为 SocialMemory schema `3` 既有集合内的 durable residue，v5 为 `OrderAndBanditry` schema `8` 的 accepted/partial/refused structured trace 与 refusal carryover，v6 为 refusal/partial residue 的 bounded response surface，并在 `OrderAndBanditry` schema `9`、`OfficeAndCareer` schema `7`、`FamilyCore` schema `8` 中保存 owner-owned response trace，v7 为 SocialMemory-owned response-residue decay / hardening 与后续 owner-module repeat friction，v8 为 owner-module actor countermove / passive back-pressure，v9 为 full soft/hard path proof 与 minimum playable response affordance/readback acceptance，v10 为普通家户 public-life/order 后账 readback，读取既有 SocialMemory snapshots / owner response trace / household pressure projections，未新增 schema bump。
- `FamilyCore`、`OfficeAndCareer`、`OrderAndBanditry`、`WarfareCampaign` 的已迁移命令路径通过 module-owned `HandleCommand(...)` seam 解析；`PlayerCommandService` / `PlayerCommandCatalog` 只负责 catalog lookup、feature fallback 与路由。
- `PresentationReadModelBundle.SocialMemories`、command `LeverageSummary` / `CostSummary` / `ReadbackSummary`、governance receipt readback 都是 read-model/projection surface，Unity shell 只复制这些字段。

本轮文档对齐已修正：
- `AGENTS.md` 的 Unity 根目录说明已更新为“存在 Unity host shell，但不默认可用 Unity Editor MCP”。
- `TECH_STACK.md` 的项目布局与依赖规则已更新为当前 source/test/project 拓扑。
- 早期历史审计正文已从本页移除，避免把已解决缺口误读为当前问题。
- 根 `README.md` 与 `release-artifacts.yml` 的 release artifact 输出路径已从旧 `net8.0` 对齐到当前 `net10.0`。
- `CODEX_SKILL_RATIONALIZATION_MATRIX.md` 和本地 Zongzu skill 当前锚点已对齐到 public-life/order v10、Order schema `9`、Office schema `7`、Family schema `8`、Population schema `2`、SocialMemory schema `3` response-residue drift / actor-countermove back-pressure / ordinary-household readback、minimum playable response affordance/readback、module-owned command seam、projection-only Unity/read-model boundary。

## 当前判断

文档和代码总体对齐：模块边界、调度顺序、只读 presentation seam、Unity host shell 定位、PersonRegistry 基线、死亡事件通道、lite 模块接入方式、public-life/order v3/v4/v5/v6/v7/v8/v9/v10 闭环、Order schema `9`、Office schema `7`、Family schema `8`、Population schema `2` 和 SocialMemory schema `3` response-residue drift / actor-countermove back-pressure / ordinary-household readback / minimum playable response readback，已经和当前代码事实一致。

仍应视为真实后续债务：
- 仁宗 pressure-chain 1-9 当前是 thin topology proof，不等同于完整社会公式或完整历史模拟；完整链条债务以 `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` 和各 chain profile 为准。
- 长跑健康检查中的压力饱和、事件 observability 分类、未消费/未来契约事件归类，仍是诊断与平衡债务，不是当前架构失败。
- pressure-chain 事件完整性需要继续按 `MODULE_INTEGRATION_RULES.md` 分类：projection-only receipt、future contract、dormant source、alignment bug。
- `docs/exec-plans/active/` 仍包含许多历史活动计划。永久契约应落在 authority docs；ExecPlan 只作为执行记录，不应单独当成已完成规范。
