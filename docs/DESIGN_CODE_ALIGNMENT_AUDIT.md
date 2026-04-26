# DESIGN_CODE_ALIGNMENT_AUDIT

## v33 delta - 2026-04-26

- Backend event-contract health v33 turns the v32 classification table into a hard ten-year diagnostic gate: emitted-but-unconsumed and declared-but-not-emitted `DomainEvent` contract debt may no longer remain `Unclassified` in the current health run.
- V33 adds a debt collector plus `AssertNoUnclassifiedEventContractDebt` in integration diagnostics, and proves the helper rejects synthetic unclassified debt before the health report can be used as evidence.
- V33 remains diagnostic/test evidence only. It adds no gameplay rule, event-pool authority, command surface, projection wording, persisted state, schema bump, migration, manager/controller layer, Application/UI/Unity authority, or `PersonRegistry` expansion.

## v32 delta - 2026-04-26

- Backend event-contract health v32 adds explicit ten-year diagnostic classifications for emitted-but-unconsumed and declared-but-not-emitted `DomainEvent` contract debt.
- v32 treats `DomainEvent` as a deterministic fact-propagation tool after module rules resolve. It is not an event-pool design, not a new pressure-chain body, and not a new gameplay rule layer.
- v32 adds no persisted state, schema bump, migration, command surface, projection wording, manager/controller layer, Application/UI/Unity authority, or `PersonRegistry` expansion.
- Validation evidence is focused on integration diagnostics: current known contract debt has classifications and diagnostic event keys no longer double-prefix module-owned event names.

## v31 delta - 2026-04-26

- Public-life/order closure v31 is an operational merge/cleanup pass: it lands the already validated v20-v30 owner-lane closure arc on `main` and removes merged topic branches after validation.
- v31 adds no gameplay rules, projection wording, commands, event topology, persisted state, schema bump, migration, ledger, manager/controller layer, or `PersonRegistry` expansion.
- Validation remains the same evidence lane as v30: build, focused integration / architecture / Unity presentation tests, `git diff --check`, and full no-build solution tests after mainline merge.

## v30 delta - 2026-04-26

- Public-life/order closure now includes v30 audit lock: v20-v30 are documented and tested as projection/readback guidance over structured owner-lane and SocialMemory fields, not a new command system, event pool, ledger, or thick household/yamen/order formula.
- v27-v29 add the missing middle layers: `现有入口读法` affordance echo, `后手收口读回` owner-lane receipt closure, and `闭环防回压` stale/no-loop guard.
- v30 adds no persisted state, schema bump, migration, command target shape, command queue, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, SocialMemory ledger, household target field, manager/controller layer, or `PersonRegistry` expansion.

## v26 delta - 2026-04-26

- Public-life/order closure now includes v26 owner-lane social-residue follow-up guidance: projected `余味冷却提示`, `余味续接提示`, and `余味换招提示` text can explain whether visible `社会余味读回` should cool down, lightly continue in the owner lane, switch owner-lane tactic, or wait for a better owner-lane entry.
- v26 remains projection/readback guidance only. It reads existing structured `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane response command/outcome fields; it does not parse SocialMemory summary prose, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v26 adds no persisted state, schema bump, migration, command system, follow-up ledger, SocialMemory ledger, owner-lane ledger, outcome ledger, cooldown ledger, household target field, or `PersonRegistry` expansion. Ordinary home-household response remains a low-power local response surface, not a universal repair or follow-up line.

## v25 delta - 2026-04-26

- Public-life/order closure now includes v25 owner-lane social-residue readback: projected `社会余味读回` text can show `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸` after the later SocialMemory monthly pass has made owner-lane response residue visible.
- v25 remains projection/readback guidance only. It reads existing structured `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane response command/outcome fields; it does not parse SocialMemory summary prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v25 adds no persisted state, schema bump, migration, command system, owner-lane ledger, outcome ledger, cooldown ledger, household target field, or `PersonRegistry` expansion. Ordinary home-household response remains a low-power local response surface, not a universal repair line.

## v24 delta - 2026-04-26

- Public-life/order closure now includes v24 owner-lane outcome reading guidance: projected `归口后读法` text explains how to read existing owner-lane outcomes such as `已修复：先停本户加压`, `暂压留账：仍看本 lane 下月`, `恶化转硬：别让本户代扛`, and `放置未接：仍回 owner lane`.
- v24 remains projection/readback guidance only. It reads existing `LastRefusalResponseOutcomeCode` values from Order / Office / Family snapshots and adds no persisted state, schema bump, migration, outcome formula, owner-lane ledger, receipt-status ledger, cooldown ledger, household target field, or `PersonRegistry` expansion.
- Ordinary home-household response remains a low-power local response surface, not a universal repair line. Unity and shell adapters copy DTO fields only; they do not compute归口结果, query modules, resolve outcomes, or write SocialMemory.

## v23 delta - 2026-04-26

- Public-life/order closure now includes v23 owner-lane receipt status readback: projected `归口状态` text says when an external after-account has already returned to Order, Office, or Family through existing structured owner response traces.
- v23 remains projection/readback guidance only. It adds no persisted state, no schema bump, no migration, no owner-lane ledger, no receipt-status ledger, no cooldown ledger, no household target field, and no `PersonRegistry` expansion.
- `已归口` is not "社会其他人接手" and not automatic repair. Application only reads `SettlementDisorderSnapshot`, `JurisdictionAuthoritySnapshot`, or `ClanSnapshot` structured response fields; Unity and shell adapters copy DTO fields only.

## v22 delta - 2026-04-26

- Public-life/order closure now includes v22 owner-lane handoff entry readback: the projected `外部后账归位` guidance also names `承接入口` labels for existing Order, Office, and Family affordances.
- v22 remains projection/readback guidance only. It adds no new commands, no command queue, no persisted state, no schema bump, no migration, no owner-lane ledger, no cooldown ledger, no household target field, and no `PersonRegistry` expansion.
- Application still does not compute command outcomes or execute the handoff; it only appends lane-specific entry labels from existing structured household response fields. Unity and shell adapters copy DTO fields only.

## v21 delta - 2026-04-26

- Public-life/order closure now includes v21 owner-lane return surface readback: the v20 `外部后账归位` guidance is copied into Office/Governance and Family-facing surfaces from existing `HouseholdPressureSnapshot.LastLocalResponse*` structure.
- v21 remains projection/readback guidance only. It adds no persisted state, no schema bump, no migration, no owner-lane ledger, no cooldown ledger, no household target field, and no `PersonRegistry` expansion.
- `PopulationAndHouseholds` still owns only the low-power home-household response trace; ordinary household response is not a universal repair line. `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, and later `SocialMemoryAndRelations` keep their owner lanes.
- Unity and shell adapters still copy DTO fields only; they do not compute owner-lane validity, query modules, resolve outcomes, or write SocialMemory.

## 当前对齐快照（2026-04-25）

本文是当前文档/代码对齐页，不再内嵌早期历史审计全文。旧的 `codex/mvp-default-path-closeout @ 1164241` 基线已经被后续模块和测试进度覆盖；如需考古，用 git 历史追溯，比在当前规范里保留过期失败清单更清楚。

当前实现事实：
- 仓库当前主线是模块化单体：`Zongzu.Application` 组合模块，`Zongzu.Scheduler` 负责确定性月度/xun 调度，模块通过 Query / Command / DomainEvent 协作。
- `PersonRegistry` 已作为 identity-only 模块存在；`FamilyCore` 死亡事件已转为 `ClanMemberDied`，并由 registry 汇总为人物死亡事实。
- `PublicLifeAndRumor`、`OfficeAndCareer`、`ConflictAndForce`、`WarfareCampaign` 等 lite/后续切片已按独立模块和独立 state namespace 接入。
- `MonthlyScheduler` 已有 prepare phase、三旬推进、月末推进、有限事件 drain、projection-last 顺序。
- `Zongzu.Presentation.Unity.ViewModels` 与 `Zongzu.Presentation.Unity` 是只读 presentation seam；Unity host shell 已存在于 `unity/Zongzu.UnityShell`，但权威模拟仍在 `src/`，且模拟项目不得引用 Unity API。
- 当前 SDK 基线以 `global.json` 和项目文件为准，现为 .NET SDK `10.0.202` 线。
- 当前分支已落地 public-life/order v3-v30 闭环：v20-v26 完成 `外部后账归位` / `承接入口` / `归口状态` / `归口后读法` / `社会余味读回` / `余味冷却提示` / `余味续接提示` / `余味换招提示`，v27 把 `现有入口读法` 投到既有 owner-lane affordance，v28 把 `后手收口读回` 投到 owner-lane receipt，v29 用 `闭环防回压` 防旧提示回压本户，v30 锁定审计证据；整段仍不新增 schema、command queue、owner-lane ledger、receipt-status ledger、outcome ledger、follow-up ledger、SocialMemory ledger 或 thick household rule loop。
- 当前 V32 后端健康检查把 DomainEvent contract debt 显式分类为 projection-only receipt / future contract / dormant seeded path / acceptance-test gap / alignment bug / unclassified debt；这是诊断证据，不是事件池、命令系统、schema 变化或新规则。
- `FamilyCore`、`OfficeAndCareer`、`OrderAndBanditry`、`WarfareCampaign` 的已迁移命令路径通过 module-owned `HandleCommand(...)` seam 解析；`PlayerCommandService` / `PlayerCommandCatalog` 只负责 catalog lookup、feature fallback 与路由。
- `PresentationReadModelBundle.SocialMemories`、command `LeverageSummary` / `CostSummary` / `ReadbackSummary`、governance receipt readback 都是 read-model/projection surface，Unity shell 只复制这些字段。

本轮文档对齐已修正：
- `AGENTS.md` 的 Unity 根目录说明已更新为“存在 Unity host shell，但不默认可用 Unity Editor MCP”。
- `TECH_STACK.md` 的项目布局与依赖规则已更新为当前 source/test/project 拓扑。
- 早期历史审计正文已从本页移除，避免把已解决缺口误读为当前问题。
- 根 `README.md` 与 `release-artifacts.yml` 的 release artifact 输出路径已从旧 `net8.0` 对齐到当前 `net10.0`。
- `CODEX_SKILL_RATIONALIZATION_MATRIX.md` 和本地 Zongzu skill 当前锚点已对齐到 public-life/order v30、Order schema `9`、Office schema `7`、Family schema `8`、Population schema `3`、SocialMemory schema `3` response-residue drift / actor-countermove back-pressure / ordinary-household readback/play-surface、home-household local response、home-household social-memory readback/repeat-friction、common-household response texture、home-household response capacity、home-household response tradeoff forecast、home-household short-term consequence readback、home-household follow-up affordance readback、external after-account owner-lane return guidance、owner-lane surface readback、owner-lane handoff entry readback、owner-lane receipt status readback、owner-lane outcome reading guidance、owner-lane social-residue readback、owner-lane social-residue follow-up guidance、owner-lane affordance echo、owner-lane receipt closure、owner-lane no-loop guard、minimum playable response affordance/readback、module-owned command seam、projection-only Unity/read-model boundary。

## 当前判断

文档和代码总体对齐：模块边界、调度顺序、只读 presentation seam、Unity host shell 定位、PersonRegistry 基线、死亡事件通道、lite 模块接入方式、public-life/order v3-v30 闭环、Order schema `9`、Office schema `7`、Family schema `8`、Population schema `3` 和 SocialMemory schema `3` response-residue drift / actor-countermove back-pressure / ordinary-household readback/play-surface / home-household local response / home-household social-memory readback/repeat-friction / common-household response texture / home-household response capacity / home-household response tradeoff forecast / home-household short-term consequence readback / home-household follow-up affordance readback / external after-account owner-lane return guidance / owner-lane surface readback / owner-lane handoff entry readback / owner-lane receipt status readback / owner-lane outcome reading guidance / owner-lane social-residue readback / owner-lane social-residue follow-up guidance / owner-lane affordance echo / owner-lane receipt closure / owner-lane no-loop guard / minimum playable response readback，已经和当前代码事实一致。

仍应视为真实后续债务：
- 仁宗 pressure-chain 1-9 当前是 thin topology proof，不等同于完整社会公式或完整历史模拟；完整链条债务以 `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` 和各 chain profile 为准。
- 长跑健康检查中的压力饱和、事件 observability 分类、未消费/未来契约事件归类，仍是诊断与平衡债务，不是当前架构失败。
- pressure-chain 事件完整性需要继续按 `MODULE_INTEGRATION_RULES.md` 分类：projection-only receipt、future contract、dormant source、alignment bug。
- `docs/exec-plans/active/` 仍包含许多历史活动计划。永久契约应落在 authority docs；ExecPlan 只作为执行记录，不应单独当成已完成规范。
