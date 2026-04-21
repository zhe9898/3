# LIVING_WORLD_DESIGN

本文档是 MVP 之后的活世界规划。

它遵循 `STATIC_BACKEND_FIRST.md` 的原则：**先稳定状态形状和契约，再填充规则密度。**

它遵循 `SIMULATION_FIDELITY_MODEL.md` 的三环精度模型：**核心环 1:1 agent，本地环混合模拟，区域环摘要压力。**

它不重复已有文档，只补全从 MVP 薄骨架到活世界之间缺失的**静态结构**。

前置阅读：
- `STATIC_BACKEND_FIRST.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `MODULE_CADENCE_MATRIX.md`
- `PLAYER_SCOPE.md`
- `PERSON_OWNERSHIP_RULES.md`
- `SPATIAL_SKELETON_SPEC.md`（Phase 1c 的实施规格）
- `FULL_SYSTEM_SPEC.md`

---

## 第一章：差距在静态结构，不在规则密度

MVP 的规则很薄，但瓶颈不是"公式不够复杂"，而是**静态容器缺失**：

| 缺失结构 | 影响 |
|---|---|
| 聚落无节点类型 | 渡口和书院不可区分 |
| 无路线拓扑 | 商路/粮道/匪压无空间锚点 |
| 无季节日历 | 安全/繁荣只能随机漂移 |
| 无 PersonRegistry，人物数据散落或全堆 FamilyCore | 路径流动受阻，模块边界模糊（已决策方案 B，见 `PERSON_OWNERSHIP_RULES.md`）|
| 无生计类型 | 佃户和手艺人不可区分 |
| 无商品/价格结构 | 粮价无法存在 |
| 恩怨无事件锚定 | 无法记录"因为什么" |
| 科举无分层 | 学业是线性进度条 |
| 无衙门/胥吏结构 | 官场只有声明 |
| 无匪团实体 | 匪压只是数字 |
| 无武力编制和交战裁决 | 冲突只是压力条 |

**下一步是补全这些静态容器，不是写规则公式。**

---

## 第二章：静态结构补全

每个补全遵循 `STATIC_BACKEND_FIRST.md` 模式：
1. 状态形状（ID/引用/列表/聚合桶）
2. 时间契约（旬/月/季节谁跑什么）
3. 命令/差异/事件/投影契约
4. 一条薄规则链证明结构可工作
5. 规则密度后续填充

---

### 2.1 WorldSettlements：空间骨骼

补全 `SettlementStateData`：
- `SettlementType`：CountySeat / MarketTown / EstateClusters / VillageCluster / Ferry / Academy / Granary / Pass / Garrison / Temple
- `RegionProfile`：WaterNetwork / InlandRoad / BorderFort / Mountain
- `floodExposure` / `droughtExposure`（区域固有）
- `currentWeather`：Normal / Drought / Flood / Locust / Epidemic
- `harvestForecast`（秋季结算）
- `embankmentCondition`（水利状态）

新增 `RouteStateData`：
- `RouteType`：MainRoad / LocalRoad / RiverRoute / CanalRoute / FerryLink / MountainPath / PassApproach
- `fromId` / `toId` / `condition` / `safety` / `isBlocked`

新增 `SeasonalCalendar`（静态工具类）：
- Season / AgrarianPhase / isFloodWindow / isDroughtWindow / isExamSeason / isTaxSeason / isFestivalWindow

时间契约：xun 路况短漂移 / seasonal 收成结算和灾荒判定

薄链：夏季 + floodExposure 高 + embankment 差 -> Flood -> harvestForecast 降

---

### 2.2 PersonRegistry + 各模块人物状态（三环分层）

人物数据所有权遵循 `PERSON_OWNERSHIP_RULES.md`（方案 B）。

#### PersonRegistry（Kernel 层，新增）

最薄的共享身份锚点，不含领域逻辑：
- PersonId / displayName / birthDate / gender / lifeStage / isAlive / fidelityRing

时间契约：month（年龄推进、生命阶段检查）

#### 各模块各自持有的人物领域状态

| 模块 | 拥有的人物数据 | 拥有的能力值 |
|---|---|---|
| FamilyCore | 族谱位置、房支归属、婚姻、亲属引用、性格（ambition/prudence/loyalty/sociability） | — |
| EducationAndExams | 学业状态、考试层级、导师 | literacy |
| TradeAndIndustry | 商铺、债务、利润 | commercialSense |
| OfficeAndCareer | 官职、候缺、考课 | — |
| ConflictAndForce | 武力编制、战斗经验、伤情 | martialAbility |
| PopulationAndHouseholds | 家户归属、生计类型、健康、活动 | healthResilience |
| SocialMemoryAndRelations | 恩怨记忆、休眠存根 | — |
| OrderAndBanditry | 匪团归属、恶名 | — |

能力值只由拥有模块修改，跨模块影响通过事件驱动。

#### 三环分层

- **核心环**：PersonRegistry 完整记录 + 各模块完整填充
- **本地环**：PersonRegistry 完整记录 + 各模块部分填充
- **区域环**：不持有个体，通过摘要池（见 2.3）

升格/降格由 PersonRegistry 执行，各模块按 FidelityRingChanged 事件填充或精简数据。

#### socialPositionLabel 是投影产物

"长房次子，候补知县，兼营布铺" 不是任何模块的权威状态，而是投影层合并各模块 Query 后产出的展示文本。

薄链：PersonRegistry 年龄推进 -> 生命阶段变更 -> 各模块按需响应

---

### 2.3 PopulationAndHouseholds：生计骨骼

补全 `PopulationHouseholdState`：
- `LivelihoodType`：Smallholder / Tenant / HiredLabor / Artisan / PettyTrader / Boatman / DomesticServant / YamenRunner / SeasonalMigrant / Vagrant
- `landHolding` / `grainStore` / `toolCondition` / `shelterQuality`
- `dependentCount` / `laborerCount`

新增摘要池容器（`SIMULATION_FIDELITY_MODEL.md` 已声明，这里定义形状）：
- `LaborPoolEntry`：settlementId / availableLabor / laborDemand / seasonalSurplus / wageLevel
- `MarriagePoolEntry`：settlementId / eligibleMales / eligibleFemales / matchDifficulty
- `MigrationPoolEntry`：settlementId / outflowPressure / inflowPressure / floatingPopulation

薄链：LivelihoodType 影响 Distress 计算基线

---

### 2.4 SocialMemoryAndRelations：事件记忆骨骼

新增 `SocialMemoryEntry`（替代纯压力条漂移）：
- MemoryType：Favor / Grudge / Shame / Fear / Debt / Patronage
- MemorySubtype：BloodGrudge / WealthGrudge / HonorGrudge / PowerGrudge / ...
- source / target（PersonId 或 ClanId）
- originDate / causeKey（结构化原因键）
- weight / monthlyDecay / isPublic / state（Active / Dormant / Resolved）

新增 `DormantStub`（重度降格人物记忆存根）：
- personId / lastKnownLocation / lastKnownRole / activeMemoryIds / lastSeen / isEligibleForReemergence

薄链：一个赈济/拒绝赈济事件产生一条记忆条目

---

### 2.5 TradeAndIndustry：商贸骨骼

新增 `GoodsCategory`：Grain / Salt / Cloth / Iron / Tea / Timber / Luxury

新增 `MarketGoodsEntry`：settlementId / goods / supply / demand / basePrice / currentPrice

新增 `TradeRouteState`：routeId / primaryGoods / throughput / riskPremium

薄链：收成 -> 粮食供给 -> 粮价

---

### 2.6 EducationAndExams：科举骨骼

新增 `ExamTier`：CountyExam / PrefecturalExam / MetropolitanExam

补全 `EducationPersonState`：currentTier / examAttempts / lastResult / fallbackPath（ContinueStudy / TeachVillage / TurnToTrade / BecomeClerk / Drift）

薄链：季节考试窗口 + 通过/落第分流

---

### 2.7 OfficeAndCareer：衙门骨骼

新增 `OfficialPostState`：postId / location / rank / currentHolder / vacancyMonths / petitionBacklog / clerkDependence / evaluationPressure

新增 `WaitingListEntry`：personId / qualificationTier / waitingMonths / patronageSupport

---

### 2.8 OrderAndBanditry：匪患骨骼

新增 `OutlawBandState`：id / bandName / baseSettlementId / strength / grainReserve / cohesion / legitimacy / concentration（Scattered / Roaming / RouteHolding / TerritoryHolding / RebelGovernance）/ controlledRoutes

---

### 2.9 ConflictAndForce：武力骨骼

新增 `ForceGroupState`：id / family（HouseholdRetainer / EscortBand / Militia / YamenForce / OfficialDetachment / RebelBand / GarrisonForce）/ owner / location / strength / readiness / morale / discipline / fatigue

新增 `ConflictIncidentState`：id / scale（SocialPressure / LocalVignette / TacticalLite / CampaignBoard）/ location / routeId / attackers / defenders / outcome / causeKey / date

---

### 2.10 WarfareCampaign：战役骨骼 ✅

补全 `CampaignState`：phase（8步 `CampaignPhase` 枚举：Proposed / Mobilizing / Marshalled / Engaged / Stalemate / Decisive / Withdrawing / Aftermath）/ directive / committedForces / contestedRoutes / supplyStretch / commandFit / civilianExposure

新增 `AftermathDocket`：merits / blames / reliefNeeds / routeRepairs / docketSummary

> **Phase 10 已落地**（schema 3→4，投影 `WarfareCampaignStateProjection.BuildCampaignPhasingAndAftermath` 在 RunMonth 尾部统一推导相位与善后案卷；`IWarfareCampaignQueries.GetAftermathDockets` 暴露只读快照）。

---

## 第三章：实施顺序

**Phase 1a** ✅：PersonRegistry 骨架（Kernel 层身份锚点、`ClanMemberDied → PersonDeceased` 汇总、Scheduler Phase 0 hook）
**Phase 1b** ✅：PersonRegistry 接入所有 bootstrap、manifest 启用、seed heir 双写、金丝雀 integration 测试
**Phase 1c**：空间骨骼（WorldSettlements 节点分类 + 功能路线 + 水陆双拓扑 + 季节带 + 叠加 query 契约）—— 详见 `SPATIAL_SKELETON_SPEC.md`
**Phase 2**：各模块人物领域状态（按 `PERSON_OWNERSHIP_RULES.md` 分布到各模块；同时迁出 FamilyCore 的冗余年龄/存活字段，改读 PersonRegistry）
**Phase 3** ✅：生计骨骼（PopulationAndHouseholds 生计类型 + 摘要池 + `DeathByIllness` 通道）
**Phase 4** ✅：记忆骨骼（SocialMemoryAndRelations 事件记忆）
**Phase 5** ✅：商贸骨骼（TradeAndIndustry 商品 + 价格）
**Phase 6** ✅：科举 + 官场 + 匪患 + 武力 + 战役骨骼（含 ConflictAndForce 的 `DeathByViolence` 通道）；分解为 Phase 6/7/8/9/10 四骨骼依次落地（EducationAndExams / OfficeAndCareer / OrderAndBanditry / ConflictAndForce / WarfareCampaign）

每个 Phase：枚举 -> 状态容器 -> Schema 迁移 -> Query 契约 -> 一条薄链。

人物相关的所有结构变更均遵循 `PERSON_OWNERSHIP_RULES.md`。

> **Phase 1a/1b 已落地状态**（截至 commit 后审计）：`Zongzu.Modules.PersonRegistry` 作为 Kernel 层新模块存在，identity-only（`PersonId/DisplayName/BirthDate/Gender/LifeStage/IsAlive/FidelityRing`）；`FamilyCoreEventNames.DeathRegistered` 已重命名为 `ClanMemberDied`，entity key 改为 PersonId；PersonRegistry 消费 `ClanMemberDied / DeathByIllness / DeathByViolence` 后汇总发出规范的 `PersonDeceased`；`MonthlyScheduler` 新增 Phase 0 pre-month pass；所有 bootstrap 场景 manifest 启用 PersonRegistry，seed heir 以 `FidelityRing.Core` 双写入 registry；包括一个金丝雀 integration 测试 `ClanMemberDied_IsConsolidated_Into_PersonDeceased_ByPersonRegistry` 守护这条链。

---

## 第四章：规则密度填充顺序

结构稳定后，按压力链逐条填充：

1. **生计链**：季节 -> 天气 -> 收成 -> 粮价 -> 家户 -> 宗族 -> 恩怨 -> 投影
2. **婚姻链**：FamilyCore -> SocialMemoryAndRelations
3. **死亡链**：FamilyCore -> 全模块
4. **科举链**：EducationAndExams -> OfficeAndCareer
5. **匪患链**：PopulationAndHouseholds -> OrderAndBanditry -> ConflictAndForce
6. **冲突链**：ConflictAndForce -> 全模块善后
7. **战役链**：WarfareCampaign 8 步
8. **信息链**：PublicLifeAndRumor 渠道模型

---

*本文档定义静态骨骼。规则密度在骨骼稳定后逐链填充。*
