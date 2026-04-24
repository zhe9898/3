# DESIGN_CODE_ALIGNMENT_AUDIT

## 当前对齐快照（2026-04-24）

本文是当前文档/代码对齐页，不再内嵌早期历史审计全文。旧的 `codex/mvp-default-path-closeout @ 1164241` 基线已经被后续模块和测试进度覆盖；如需考古，用 git 历史追溯，比在当前规范里保留过期失败清单更清楚。

当前实现事实：
- 仓库当前主线是模块化单体：`Zongzu.Application` 组合模块，`Zongzu.Scheduler` 负责确定性月度/xun 调度，模块通过 Query / Command / DomainEvent 协作。
- `PersonRegistry` 已作为 identity-only 模块存在；`FamilyCore` 死亡事件已转为 `ClanMemberDied`，并由 registry 汇总为人物死亡事实。
- `PublicLifeAndRumor`、`OfficeAndCareer`、`ConflictAndForce`、`WarfareCampaign` 等 lite/后续切片已按独立模块和独立 state namespace 接入。
- `MonthlyScheduler` 已有 prepare phase、三旬推进、月末推进、有限事件 drain、projection-last 顺序。
- `Zongzu.Presentation.Unity.ViewModels` 与 `Zongzu.Presentation.Unity` 是只读 presentation seam；Unity host shell 已存在于 `unity/Zongzu.UnityShell`，但权威模拟仍在 `src/`，且模拟项目不得引用 Unity API。
- 当前 SDK 基线以 `global.json` 和项目文件为准，现为 .NET SDK `10.0.202` 线。

本轮文档对齐已修正：
- `AGENTS.md` 的 Unity 根目录说明已更新为“存在 Unity host shell，但不默认可用 Unity Editor MCP”。
- `TECH_STACK.md` 的项目布局与依赖规则已更新为当前 source/test/project 拓扑。
- 早期历史审计正文已从本页移除，避免把已解决缺口误读为当前问题。

## 当前判断

文档和代码总体对齐：模块边界、调度顺序、只读 presentation seam、Unity host shell 定位、PersonRegistry 基线、死亡事件通道、lite 模块接入方式，已经和当前代码事实一致。

仍应视为真实后续债务：
- 仁宗 pressure-chain 1-9 当前是 thin topology proof，不等同于完整社会公式或完整历史模拟；完整链条债务以 `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` 和各 chain profile 为准。
- 长跑健康检查中的压力饱和、事件 observability 分类、未消费/未来契约事件归类，仍是诊断与平衡债务，不是当前架构失败。
- pressure-chain 事件完整性需要继续按 `MODULE_INTEGRATION_RULES.md` 分类：projection-only receipt、future contract、dormant source、alignment bug。
- `docs/exec-plans/active/2026-04-24_notification-scope-read-helpers.md` 当前仍是未跟踪提案文件；除非后续提交实现，否则不应当被当成已完成规范。
