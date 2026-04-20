# DESIGN_CODE_ALIGNMENT_AUDIT

本文档记录截至 `codex/mvp-default-path-closeout @ 1164241` 时，设计文档（MODULE_BOUNDARIES / PERSON_OWNERSHIP_RULES / SIMULATION 等）与 `src/` 代码之间的一致性差距。

审计目的：
- 确认哪些设计决策已经落地
- 确认哪些设计决策只活在文档里，尚未进入代码
- 为下一阶段实施（LIVING_WORLD_DESIGN Phase 1a 及之后）确定真实基线

审计不做任何代码改动。它只产出差异清单。

---

## 一致的地方 ✅

| 项 | 设计 | 代码 | 状态 |
|---|---|---|---|
| 模块化单体 + IModuleRunner + Phase/ExecutionOrder | MODULE_BOUNDARIES + ENGINEERING_RULES | `MonthlyScheduler.OrderModules` 按 Phase → ExecutionOrder → ModuleKey 排序 | ✅ |
| xun / month 双层节拍 | SIMULATION.md | `MonthlyScheduler.AdvanceOneMonth` 先跑三次 xun，再跑 month，再处理月末事件 | ✅ |
| 模块拥有各自 state，不直接写别的模块 | MODULE_BOUNDARIES | 各 `*State.cs` 独立，跨模块走 Query/Event | ✅ |
| WorldSettlements 拥有聚落 / 路况 / 机构基线 | MODULE_BOUNDARIES §1 | `WorldSettlementsState` + `WorldSettlementsModule` | ✅ |
| PublicLifeAndRumor 作为新模块接入 | 设计第 9 轮补入 §11 | `Zongzu.Modules.PublicLifeAndRumor` 项目 + 6 tests pass | ✅ |
| FamilyCore 拥有宗族压力字段（MourningLoad / HeirSecurity / MarriageAlliancePressure 等） | MODULE_BOUNDARIES §2 | `ClanStateData` 包含全部命名字段 | ✅ |
| NarrativeProjection 作为只读投影层 | ARCHITECTURE | `NarrativeProjectionModule` 只读多模块 Query | ✅ |

---

## 不一致的地方 🔲

### 🅰️ PersonRegistry Kernel 模块根本不存在 —— 高优先级

- **设计**：`MODULE_BOUNDARIES §0` 与 `PERSON_OWNERSHIP_RULES` 要求 Kernel 层存在一个 identity-only 的 `PersonRegistry`，拥有 `PersonId / displayName / birthDate / gender / lifeStage / isAlive / FidelityRing`，提供 `IPersonRegistryQueries`。
- **代码**：
  - 不存在 `src/Zongzu.Kernel.PersonRegistry/` 或任何 `PersonRegistry` 类
  - 没有 `FidelityRing` 枚举
  - 没有 `IPersonRegistryQueries` 接口
  - `PersonId` 目前只是 `Zongzu.Kernel` 里的值对象，没有 registry 托管它
- **影响**：
  - 多路径人物流动无基础锚点（`MULTI_ROUTE_DESIGN_MATRIX` 的前置条件）
  - `LIVING_WORLD_DESIGN` Phase 1a 的工作面
  - Scheduler Phase 0 的"年龄推进 / 生命阶段检查"目前没有执行者

### 🅱️ 死亡事件模型仍是旧版 —— 高优先级

- **设计**：
  - 领域模块发出**因果特定**事件（`ClanMemberDied` / `DeathByIllness` / `DeathByViolence`）
  - `PersonRegistry` 消费这些事件并汇总成 `PersonDeceased`
- **代码**：
  - `FamilyCoreEventNames` 只有单个 `DeathRegistered`
  - `FamilyCoreModule.TryResolveClanDeath` 自己同时判定"谁死"和"谁老了该死"，然后发单一 `DeathRegistered` —— FamilyCore 在承担 PersonRegistry 应负的年龄淘汰职责
  - 没有 `DeathByIllness` / `DeathByViolence` / `PersonDeceased` 事件名定义
- **影响**：
  - PopulationAndHouseholds 若要上 `health` 导致死亡、ConflictAndForce 若要上战斗致死，没有契约事件通道
  - FamilyCore 目前越界（既决定"这个人因为老了该死"又决定"这是本族之死"）

### 🅲 FamilyCore 的人物状态字段不全 —— 中优先级

- **设计**（PERSON_OWNERSHIP_RULES）：FamilyCore 的 `FamilyPersonState` / `FamilyClanMembership` 应该有
  - `branchPosition` (MainLineHeir / BranchHead / BranchMember / DependentKin / MarriedOut)
  - `spouseId / childrenIds / fatherId / motherId`（clan-scoped 亲属）
  - `FamilyPersonality` (ambition / prudence / loyalty / sociability)
- **代码**：`FamilyPersonState` 目前只有 `Id / ClanId / GivenName / AgeMonths / IsAlive`。
- **影响**：
  - 宗族叙事（继嗣 / 房支 / 性格）目前靠 clan 聚合字段粗粒度代表，没有人物粒度
  - 性格尚无落点，无法驱动"同样的命令，不同的人做，结果不同"

### 🅳 PopulationAndHouseholds 的人物领域状态未显化 —— 中优先级

- **设计**：PopulationAndHouseholds 拥有 `healthResilience / health / activity` 等户内个人状态。
- **代码**：当前 state 以 `Household` 粒度为主，没有 person-level 健康 / 活跃度结构。
- **影响**：
  - xun Phase B 里"某人病情加重"尚无具体数据落点
  - 未来的病逝 → `DeathByIllness` 通道还没有源头

### 🅴 其他模块的 person-level 能力字段未显化 —— 低优先级（设计允许延后）

- 设计表明：
  - `EducationAndExams.literacy`
  - `TradeAndIndustry.commercialSense`
  - `ConflictAndForce.martialAbility`
- **代码**：这些模块当前以 clan/settlement/region 聚合为主，尚无 per-PersonId 能力字段。
- **评估**：这符合 `STATIC_BACKEND_FIRST` 的"先结构后规则"哲学 —— 不是违规，只是尚未展开。一旦 PersonRegistry 到位，再分模块加 person 能力即可。

### 🅵 Scheduler Phase 0 定义与实现差一截 —— 低优先级

- **设计**：SIMULATION.md Phase 0 要求 PersonRegistry 执行年龄推进与生命阶段检查。
- **代码**：`MonthlyScheduler.AdvanceOneMonth` 没有任何 "Phase 0 / pre-month" 前置步骤，直接进入 xun 循环。
- **评估**：这与 🅰️ 是同一件事的两面 —— 没有 PersonRegistry 模块，自然没有 Phase 0 hook。等 🅰️ 落地时一起补。

### 🅶 FamilyCore 非族人亲属数据需要确认未被误写入 —— 低优先级

- **设计**：FamilyCore 只跟踪"属于或曾属于本族的人"的 kinship，不做全局亲属表。
- **代码**：`FamilyCoreState.People` 目前只包含 `ClanId` 必填的条目 —— 看起来已经 clan-scoped，但需要在 👆 🅲 补 `spouse/parents/children` 字段时重申这条规则并加单测保护（防止有人写一个"别族人加进 People 列表"的 PR 就过）。

---

## 差距分级汇总

| 等级 | 项 | 状态 |
|---|---|---|
| 🔴→✅ | 🅰️ PersonRegistry 不存在 | **已解决 (Phase 1a+1b)** |
| 🔴→✅ | 🅱️ 死亡事件模型旧 | **已解决 (Phase 1a)** — `DeathRegistered` 重命名为 `ClanMemberDied`，entity key=PersonId，PersonRegistry 汇总为 `PersonDeceased` |
| 🟡 | 🅲 FamilyPersonState 字段不全 | Phase 2 |
| 🟡 | 🅳 Population person-level 健康字段 | Phase 3（含 `DeathByIllness` 通道） |
| 🟢 | 🅴 其他模块能力字段 | Phase 2 / Phase 6 分模块 |
| 🟢→✅ | 🅵 Scheduler Phase 0 hook | **已解决 (Phase 1a)** — `MonthlyScheduler` 在三次 xun 前跑 `SimulationPhase.Prepare` |
| 🟢 | 🅶 FamilyCore clan-scoped 单测 | 做 🅲 时顺手加 |

---

## Phase 1a/1b 完成情况（补录）

**落地内容：**
- 新增 `src/Zongzu.Modules.PersonRegistry/`（Kernel 层，identity-only）
- 新增 `src/Zongzu.Contracts/PersonRegistryTypes.cs`：`LifeStage` / `FidelityRing` / `PersonGender` / `PersonRecord` / `IPersonRegistryQueries` / `PersonRegistryEventNames` / `DeathCauseEventNames`
- `KnownModuleKeys.PersonRegistry` 加入，按字母序排在 `PopulationAndHouseholds` 之前
- `FamilyCoreEventNames.DeathRegistered` → `ClanMemberDied`（指向 `DeathCauseEventNames.ClanMemberDied`，单一真源）；emit 时 entity key 改为 PersonId
- `MonthlyScheduler.AdvanceOneMonth` 新增 Phase 0 pre-month pass（`SimulationPhase.Prepare` 模块 month cadence 先跑）
- `SimulationBootstrapper`：7 个 `CreateXxxModules` 列表注入 `new PersonRegistryModule()`；`CreateM0M1Manifest` 启用 PersonRegistry；`SeedMinimalWorld` 把 heir 以 `FidelityRing.Core` 双写入 registry
- `NarrativeProjection` / `NotificationProjectionContext` 5 处事件名同步跟随
- 测试套件：新增 `Zongzu.Modules.PersonRegistry.Tests`（6 tests）+ `PersonRegistryIntegrationTests`（2 金丝雀 tests，其中一个端到端验证 `ClanMemberDied → PersonDeceased`）
- 17 个测试项目全绿，174+ tests pass

**当前代码基线（Phase 1b 后）：**
| 契约 | 状态 |
|---|---|
| PersonRegistry Kernel 层模块存在，identity-only | ✅ |
| Phase 0 age progression 在 xun 前跑 | ✅ |
| FamilyCore 发 `ClanMemberDied`（PersonId entity key） | ✅ |
| PersonRegistry 汇总出规范 `PersonDeceased` | ✅（金丝雀守护） |
| 所有 bootstrap 场景启用 PersonRegistry | ✅ |
| Save/load roundtrip 确定性 | ✅ |
| `DeathByIllness` / `DeathByViolence` 通道 | 🔲 契约已预留，源头模块尚未 emit（Phase 3 / Phase 6） |
| FamilyCore 迁出冗余 `AgeMonths/IsAlive`，改读 Registry | 🔲 Phase 2 |
| 各模块 person 能力字段 | 🔲 Phase 2+ |
