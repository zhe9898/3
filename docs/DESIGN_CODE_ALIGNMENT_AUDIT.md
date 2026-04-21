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

---

## Step 1a 审计：跨模块事件薄链现状（截至 Phase 10 完成）

> 审计范围：§2.1–§2.10 骨骼全部落地之后，基于一次 120 月（10 年）长程健康检查（`TenYearSimulationHealthCheckTests`）产出的事件计数，按"publish / consume"两端对账。
>
> **审计只描述事实与通电缺口，不决定触发概率、不填数值公式。** 每条断链下列出的"维度入口"表示"将来 Step 2 规则密度填充时，这条链的强度应当由这些已有维度共同解释"；具体函数形状留待 Step 2 讨论。

### 事件发布 / 消费对账表

| 事件 | 发布模块 | 120 月发生次数 | 被权威 consume | 状态 |
|---|---|---:|---:|---|
| `WarfareCampaign.CampaignPressureRaised` | WarfareCampaign | 6 | 48（8 模块 × 6 次）| ✅ 活 |
| `WarfareCampaign.CampaignMobilized` | WarfareCampaign | 5 | 40（8 模块 × 5 次）| ✅ 活 |
| `WarfareCampaign.CampaignSupplyStrained` | WarfareCampaign | — | — | ✅ 有 consumer |
| `WarfareCampaign.CampaignAftermathRegistered` | WarfareCampaign | — | — | ✅ 有 consumer |
| `FamilyCore.ClanMemberDied` | FamilyCore | 0 | — | 🟡 链活但源头未触发（无人死亡） |
| `FamilyCore.BirthRegistered` | FamilyCore | 0 | — | 🟡 源头未触发 |
| `FamilyCore.HeirSecurityWeakened` | FamilyCore | — | 0 | 🔴 发而未消费 |
| `PersonRegistry.PersonDeceased` | PersonRegistry | 0 | — | 🟡 上游死亡链未通电 |
| `Population.DeathByIllness` | PopulationAndHouseholds | 0 | — | 🔴 `ConsumedEvents` 未登记此事件；源头规则未实现 |
| `Population.HouseholdDebtSpiked` | PopulationAndHouseholds | — | 0 | 🔴 发而未消费 |
| `Population.MigrationStarted` | PopulationAndHouseholds | — | 0 | 🔴 发而未消费 |
| `Population.LaborShortage` | PopulationAndHouseholds | — | 0 | 🔴 发而未消费 |
| `Population.LivelihoodCollapsed` | PopulationAndHouseholds | — | 0 | 🔴 发而未消费 |
| `ConflictAndForce.ConflictResolved` | ConflictAndForce | 371 | 0 | 🔴 **大量发而未消费** |
| `ConflictAndForce.CommanderWounded` | ConflictAndForce | 135 | 0 | 🔴 **大量发而未消费** |
| `ConflictAndForce.DeathByViolence` | ConflictAndForce | 0 | — | 🔴 源头规则未实现（冲突从不致死） |
| `Trade.RouteBusinessBlocked` | TradeAndIndustry | 484 | 0 | 🔴 **高频空转** |
| `Trade.TradeLossOccurred` | TradeAndIndustry | 361 | 0 | 🔴 **高频空转** |
| `Trade.TradeDebtDefaulted` | TradeAndIndustry | 279 | 0 | 🔴 **高频空转** |
| `Trade.TradeProspered` | TradeAndIndustry | — | 0 | 🟡 发而未消费 |
| `Order.BanditThreatRaised` | OrderAndBanditry | — | 0 | 🔴 发而未消费 |
| `Order.OutlawGroupFormed` | OrderAndBanditry | — | 0 | 🔴 发而未消费 |
| `Order.RouteUnsafeDueToBanditry` | OrderAndBanditry | — | 0 | 🔴 发而未消费 |
| `Order.BlackRoutePressureRaised` | OrderAndBanditry | — | 0 | 🔴 发而未消费 |
| `World.FloodRiskThresholdBreached` | WorldSettlements | 77 | 0 | 🔴 **气候灾害空转** |
| `World.RouteConstraintEmerged` | WorldSettlements | — | 0 | 🔴 发而未消费 |
| `World.CorveeWindowChanged` | WorldSettlements | — | 0 | 🔴 发而未消费 |
| `PublicLifeAndRumor.*`（5 类） | PublicLifeAndRumor | 共 555（其中 `PrefectureDispatchPressed` 378、`CountyGateCrowded` 100） | 0 | 🔴 **公共脉动整体空转** |
| `SocialMemory.GrudgeEscalated` | SocialMemoryAndRelations | — | 0 | 🟡 记忆事件整体无下游 |
| `Office.OfficeGranted` / `OfficeLost` / `OfficeTransfer` / `AuthorityChanged` | OfficeAndCareer | — | 0 | 🟡 官场事件无下游 |

**全局结构观察**：

1. **所有非 WarfareCampaign 的事件 consumer = 0**。整个系统目前只有一条真正的跨模块事件通路，就是"战役压力向下压"；其他 8 个模块的 `ConsumedEvents` 字段都只登记了 4 个 WarfareCampaign 事件，没有互相监听。
2. **`PublishedEvents` 白名单与实际消费脱节**：绝大多数模块的 `PublishedEvents` 清单注册给了白名单审计，但没有任何模块在自己的 `ConsumedEvents` 里登记它们。
3. **源头型死水**：`ClanMemberDied=0 / PersonDeceased=0 / DeathByIllness=0 / DeathByViolence=0 / BirthRegistered=0` —— 生死链完全没启动，连 publish 这一步都没发生。

---

### 通电缺口清单（按体检报告热度排序）

> 每条缺口记录：**发布端 → 理论消费端 → 维度入口 → 通电需要的最小骨架**。
> 维度入口 = 当这条链 Step 2 填规则时，强度函数可以吃进去的已有字段。**不在本审计里决定函数形状**。

#### 缺口 1：贸易冲击 → 宗族信用 / 社会记忆 / 人口迁徙（🔴 最高频，1124 次空转）

**发布端**：
- `TradeAndIndustry.RouteBusinessBlocked`（484）
- `TradeAndIndustry.TradeLossOccurred`（361）
- `TradeAndIndustry.TradeDebtDefaulted`（279）

**理论消费端**：
- `FamilyCore` —— 违约/损失需要进入家族压力（现有字段：`branchTension / inheritanceFriction / prestige / shame`）
- `SocialMemoryAndRelations` —— 违约应沉淀为针对性记忆（现有记忆容器：`ClanNarrativeState` / `EventMemoryState`）
- `PopulationAndHouseholds` —— 破产对家户的压力（现有：`MigrationPressure / LivelihoodPressure / DebtLoad`）

**可供 Step 2 组合的维度入口**：
- 违约方身份（哪户 / 属哪族 / 是否有功名）
- 债务规模 / 家户家底 ratio
- 两家既有关系（SocialMemory 当前 grudge / favor）
- 当地粮价波动（TradeAndIndustry 价格）
- 当季徭役与诉讼负担（Office `PetitionBacklog`）
- 聚落治安（Order `Security`）
- 季节带 / 灾害窗口（WorldSettlements 季节字段）

**通电最小骨架**（不写规则函数，只接 dispatch）：
- FamilyCore.`ConsumedEvents` += 3 条；`HandleEvents` 加 3 个分支，每个分支体只留 TODO trace。
- SocialMemoryAndRelations 同上。
- PopulationAndHouseholds 同上。
- 规则函数主体：**Step 2 填**。

---

#### 缺口 2：冲突伤亡 → 死亡链 / 血仇记忆（🔴 506 次空转，且堵住生死链）

**发布端**：
- `ConflictAndForce.ConflictResolved`（371）
- `ConflictAndForce.CommanderWounded`（135）

**理论消费端**：
- `ConflictAndForce` 自身 —— 依据"负伤 → 致死"的维度组合，向上发 `DeathByViolence`（这条通道契约已预留，源头从未触发）
- `FamilyCore` —— 消费 `DeathByViolence`（不是 CommanderWounded），按已有规则转 `ClanMemberDied` + `HeirSecurityWeakened` + `MourningLoad`
- `PersonRegistry` —— 消费 `DeathByViolence`，汇总为 `PersonDeceased`（Phase 1a 通道已就位，但从未被触发过）
- `SocialMemoryAndRelations` —— 消费 `DeathByViolence`，沉淀跨代血仇记忆（与普通疾病死亡区分）

**可供 Step 2 组合的维度入口**：
- 伤情严重度（`IncidentScale` / `IncidentOutcome`）
- 个人 LifeStage / 年龄（PersonRegistry）
- 家族照料能力（FamilyCore `prosperity / prestige`）
- 当地医疗 / 治安（WorldSettlements `Security` + OrderAndBanditry）
- 季节带 / 气候（WorldSettlements）
- 是否处于战后恢复期（WarfareCampaign `Phase == Aftermath`）
- 既有家族恩怨（SocialMemory grudge）

**通电最小骨架**：
- ConflictAndForce.`PublishedEvents` 保证已声明 `DeathByViolence`（已登记），其 `RunMonth` / `RunXun` 里加入占位致死分支（函数体 TODO，默认 no-op）。
- FamilyCore.`ConsumedEvents` += `DeathByViolence`；`HandleEvents` 加分支，先只记 trace，不改 state。
- SocialMemoryAndRelations.`ConsumedEvents` += `DeathByViolence`；同上。
- 规则函数主体：**Step 2 填**。

---

#### 缺口 3：世界脉动 / 公共舆论 → 经济 / 秩序 / 官署（🔴 555 + 77 次空转）

**发布端**：
- `PublicLifeAndRumor.PrefectureDispatchPressed`（378）、`CountyGateCrowded`（100）、`StreetTalkSurged` / `MarketBuzzRaised` / `RoadReportDelayed` 共 555
- `WorldSettlements.FloodRiskThresholdBreached`（77）
- `WorldSettlements.RouteConstraintEmerged / CorveeWindowChanged`（未统计次数）

**理论消费端**：
- `OrderAndBanditry` —— 水患 / 徭役 / 州牒压力应抬高匪患基线
- `TradeAndIndustry` —— 洪灾应影响路况与粮价，徭役应拉走劳动力
- `OfficeAndCareer` —— 州牒与县门拥堵应进入 `PetitionBacklog`
- `PopulationAndHouseholds` —— 徭役 / 洪灾应改变 `LaborPressure / LivelihoodPressure`

**可供 Step 2 组合的维度入口**：
- 季节带 / 灾害窗口
- 当地地形分类（WorldSettlements 节点分类）
- 聚落繁荣度 / 治安基线
- 官署权威层级（Office `AuthorityTier`）
- 既有路线压力（TradeAndIndustry / OrderAndBanditry）

**通电最小骨架**：同上三步走（ConsumedEvents 登记 + HandleEvents 占位 + trace-only）。

---

#### 缺口 4：家族压力顶格 → 分家 / 出走 / 婚姻折价（🟡 中优先级）

**现象**：体检报告显示 4 个 clan 十年后 `grudge=100 / shame=100 / branchTension=100 / inheritanceFriction=100`，全部顶格，但没有任何分家事件、没有人迁出、没有婚姻折价。

**发布端**：
- `FamilyCore.HeirSecurityWeakened`（已声明，未统计次数）
- **缺失事件**：`BranchSplit` / `ClanMigrationStarted` / `MarriageAlliancePressureCritical` —— 契约尚未预留。

**理论消费端**：
- `PopulationAndHouseholds` —— 分家转为实际户数增减
- `SocialMemoryAndRelations` —— 分家进入族叙事
- `PersonRegistry` —— 出走改变个人 Settlement 归属

**通电最小骨架**：
- `FamilyCoreEvents.cs` 补声明 `BranchSplit` 事件（contract 层只加字符串常量，不改规则）
- FamilyCore 的 RunMonth 里加占位触发点（体顶格时发事件，但**发不发、何时发由 Step 2 决定**；本审计不触发）
- 下游三模块 `ConsumedEvents` 登记 + `HandleEvents` 占位。

---

### 审计结论

1. **系统目前唯一活的跨模块链 = 战役链**。其余事件总量超过 2000 条 / 10 年，`consumed=0`。
2. **三条源头型死水**：`ClanMemberDied / PersonDeceased / DeathByIllness / DeathByViolence / BirthRegistered`，生死链完全未启动。
3. **通电的最小代价**：在不触碰任何规则数值的前提下，给 8 条高频 / 关键链各加 `ConsumedEvents` 登记 + `HandleEvents` 占位分支。规则函数体留 TODO，等待 Step 2。
4. **下一步不由审计决定**：审计交付此清单后，由你指定先通哪几条链，再按每条链逐个加薄骨架（每条链改动 ≤ 30 行，不含规则）。

---

### Step 1b 建议起点（供你拍板用）

按"高频空转 + 游戏体验拉力"排序，我会建议先通缺口 2（冲突伤亡 → 死亡链），理由：
- 它**同时解锁两条被堵住的源头**（`DeathByViolence` + `PersonDeceased`）
- 它是游戏层最能被玩家感知的"人死、家破、世仇"叙事骨架
- 改动面最小（ConflictAndForce 触发点 + FamilyCore / PersonRegistry / SocialMemory consume 各一个分支）
- 不涉及任何新 contract（`DeathByViolence` 契约已就位）

通电完做一次 10 年 re-run，看 `consumed > 0` 即视为通电成功；数值是否"好玩"属于 Step 2 范畴。

其他顺序你也可以选（缺口 1 量最大，缺口 3 面最宽）。
