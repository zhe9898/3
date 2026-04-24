# LIVING_WORLD_DESIGN

本文档是 MVP 之后的活世界规划。
它不是单独的模块愿望清单，而是从 MVP 薄骨架走向完整活社会模拟的结构补全路线。

它遵循 `STATIC_BACKEND_FIRST.md` 的原则：**先稳定状态形状和契约，再填充规则密度。**

它遵循 `SIMULATION_FIDELITY_MODEL.md` 的三环精度模型：**核心环 1:1 agent，本地环混合模拟，区域环摘要压力。**

它也遵循 `RULES_DRIVEN_LIVING_WORLD.md` 与 `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md` 的补正规则：
- 活世界不是事件池，所有结果都必须能回到规则链。
- 玩家不是开局上帝，但长期可以通过规则链改变历史。
- 历史大势、皇权、改革、叛乱、建制、篡位、复辟、王朝修复都必须先变成模块拥有的压力、窗口、执行、反噬和记忆。
- MVP 不做政权级玩法，但静态结构和边界不能把后期路线堵死。
- 仁宗朝只是开局制度场和压力场，不是锁死的历史轨道；后续历史必须能被规则、人物、地方执行、偶然压力和玩家影响圈推向合理的反事实。

它不重复已有文档，只补全从 MVP 薄骨架到活世界之间缺失的**静态结构**。

前置阅读：
- `PRODUCT_SCOPE.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `STATIC_BACKEND_FIRST.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `MODULE_CADENCE_MATRIX.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `PERSON_OWNERSHIP_RULES.md`
- `SPATIAL_SKELETON_SPEC.md`（Phase 1c 的实施规格）
- `FULL_SYSTEM_SPEC.md`
- `POST_MVP_SCOPE.md`

---

## 第零章：补正后的总路线

活世界结构补全分四层推进：

1. **地方生活骨骼**
   - 空间、人物、家户、生计、宗族、记忆、商贸、科举、公共生活。
   - 目标是让北宋仁宗朝地方社会自己呼吸，而不是只让玩家家族动。

2. **地方权力和失序骨骼**
   - 衙门、胥吏、地方治理、匪患、灰色路线、私力、治安、冲突。
   - 目标是让命令、文书、钱粮、路线、保护、恐惧和舆论互相传导。

3. **战役和上层压力骨骼**
   - 武力编制、战役沙盘、军需、边防、皇权节律、朝廷公文、大赦、国丧、储位摇动。
   - 目标是让上层世界以压力进入地方，而不是一开始做 detached grand strategy。

4. **历史大势和王朝循环骨骼**
   - 历史人物、改革窗口、政策包、派系、叛乱、建制、继承斗争、篡位、复辟、王朝修复。
   - 目标是允许玩家长期改变历史，但必须通过规则链、影响圈、合法性、武力、钱粮、官府、舆论和记忆。

核心路线：

`静态容器 -> 查询/事件/命令契约 -> 一条薄规则链 -> 只读投影 -> 有界玩家介入 -> 记忆和后果 -> 更深规则密度`

不允许的路线：

`写一个历史事件 -> 全局改数值 -> 弹通知 -> 玩家点按钮改天下`

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
| 公共生活无场所/渠道 | 榜文、街谈、庙会、茶肆、州牒无法成为压力传导面 |
| 玩家影响圈无投影 | 玩家会被误读成固定职业或默认族长 |
| 历史大势无趋势包 | 王安石、庆历、新政、边防压力会退化成年份事件 |
| 皇权节律无上层骨骼 | 国丧、大赦、储位、朝廷停摆只能当背景文案 |
| 叛乱/建制无阶梯 | 匪患会直接跳成新皇帝，或永远只是匪患 |

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

时间契约：day 路况短漂移 / seasonal 收成结算和灾荒判定；xun 只作为历法读法或投影分组

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

### 2.11 PublicLifeAndRumor：公共生活骨骼

公共生活不是通知皮肤，而是地方社会把压力变成可见舆论、羞辱、合法性和行动窗口的地方。

补全 `PublicVenueState`：
- `venueId` / `settlementId`
- `VenueType`：CountyGate / MarketStreet / TempleCourt / TeaHouse / FerryLanding / NoticeWall / SchoolGate / YamenOuterCourt
- `visibility` / `crowdHeat` / `elitePresence` / `commonerPresence` / `officialAttention`

补全 `PublicChannelState`：
- `ChannelType`：OfficialNotice / StreetTalk / TempleWhisper / RoadReport / PrefectureDispatch / MarketRumor
- `heat` / `credibility` / `spreadRadius` / `contention`
- `sourceModuleKey` / `causeKey`

新增 `PublicLegitimacySignal`（只读投影或模块拥有状态，按实现时边界决定）：
- `settlementId`
- `targetKind`：Person / Clan / Office / Policy / ForceGroup / RebelBand
- `legitimacyDelta` / `shameDelta` / `fearDelta` / `attentionBand`

时间契约：
- day：街谈、路报、榜文热度短漂移
- xun：只作为早 / 中 / 晚月的可读汇总标签，不作为底层权威格
- month：整合公共面 residue，供 `NarrativeProjection` 和 hall / desk 读

薄链：
`死亡 / 饥荒 / 匪患 / 政策执行 -> 公共渠道升温 -> 声誉/羞辱/合法性投影 -> 玩家可用公共面杠杆`

---

### 2.12 PlayerInfluenceFootprint：影响圈投影骨骼

影响圈不是玩家职业，不是路线系统，不是权威 state。
它是应用层或投影层对现有模块 Query 的只读合成。

补全 `InfluenceReachSnapshot`：
- `layerKey`：Household / Lineage / Market / Education / Yamen / PublicLife / Disorder / Force / Campaign / Imperial
- `visibility`：Absent / WatchOnly / Indirect / Commandable
- `availableLevers`：Money / Grain / Prestige / Favor / Office / Force / PublicFace / Information
- `frictionSummary`
- `sourceProjectionKeys`

补全 `PlayerInfluenceFootprintSnapshot`：
- `ownHousehold`
- `observedHouseholds`
- `lineageReach`
- `marketReach`
- `educationReach`
- `yamenReach`
- `publicLifeReach`
- `disorderReach`
- `forceReach`
- `imperialReach`

边界：
- 不进权威存档，除非未来明确有玩家身份/职位模块拥有它。
- 不产生新命令。
- 只解释玩家当前能看见、能碰到、碰不到什么。

薄链：
`模块 read model -> 应用层合成影响圈 -> shell 显示可见/可介入层 -> 玩家选择已有 command`

---

### 2.13 HistoricalProcess：历史大势骨骼

历史大势先作为设计语法和 feature-pack 边界存在，不急着建隐藏全局模块。
当某条历史大势要运行时，必须落到拥有模块。

趋势包形状：
- `TrendPressure`：财政 / 军事 / 市场 / 教育 / 合法性 / 灾害 / 失序
- `ActorCarrier`：人物、派系、官署、宗族、商路、军伍、寺院或公共空间
- `InstitutionalWindow`：皇帝信任、官缺、边防急迫、灾荒赈济、科举风向、奏议窗口
- `PolicyBundle`：青苗、税籍、募役、保甲、学校、采购、漕运、赈济、考课、巡检
- `LocalImplementation`：县令、胥吏、族老、商户、寺院、匪团、军伍如何执行或变形
- `CaptureAndResistance`：拖延、纸面服从、截留、抵制、上诉、谣言、派系攻击
- `Residue`：政策记忆、恩怨、羞辱、债、派系标签、制度伤痕

推荐未来状态形状（仅在有 owner 时建立）：
- `HistoricalTrendState`：trendId / eraPack / pressureBand / windowState / activePolicyBundles / residueKeys
- `HistoricalActorState`：actorId / personId? / factionKey / reputation / writings / officeReach / enemies
- `PolicyImplementationState`：policyId / settlementId / implementationQuality / captureRisk / publicLegitimacy / backlash

边界：
- 当前先通过 `OfficeAndCareer`、`WorldSettlements`、`PopulationAndHouseholds`、`TradeAndIndustry`、`PublicLifeAndRumor`、`OrderAndBanditry`、`WarfareCampaign`、`SocialMemoryAndRelations` 落地。
- 不允许 `NarrativeProjection` 或 UI 按年份直接改状态。
- 不允许一个 `1069NewPolicies` 事件全局改数值。

薄链：
`财政/边防压力积累 -> 人物/派系承载 -> 朝廷或官署窗口 -> 地方政策执行 -> 家户/市场/衙门/公共面变化 -> 反噬和记忆`

---

### 2.14 CourtAndThrone / WorldEvents：皇权与王朝循环骨骼（P5）

皇权不是 MVP 直接玩法，但它必须是后期活世界的一条上层压力轴。
它既不是单纯文书，也不是全能按钮。

玩家首先接触到的不是"皇帝本人"，而是皇权下行后的可见物件和地方反应：
- 诏书 / 敕牒 / 官牒到县
- 县门榜示和衙门案牍
- 国丧导致的停宴、停役、停婚、停市或礼仪变调
- 大赦导致的诉讼、罪责、匪患、旧怨、复出机会变化
- 任命、考课、弹劾、调任带来的官署节奏变化
- 税役、赈济、军需、边报压到家户、宗族、市场和公共生活

MVP 只允许这些成为 watch-only / projection pressure。
P5 以后才允许玩家通过官府、派系、文书、军伍、钱粮、舆论、仪礼和信息链条触碰皇权本身。
不允许出现无条件的皇帝按钮、改诏按钮、改元按钮或自由时间线编辑。

补全未来 `ImperialBand`：
- `MourningInterruption`：国丧 / 帝王死亡导致的事务中止
- `AmnestyWave`：登基、大赦、灾异修复后的赦令波
- `SuccessionUncertainty`：储位不明、废立、夺嫡、观望
- `MandateConfidence`：天命可信度，受灾异、战败、乱象、修复行为影响
- `CourtTimeDisruption`：朝廷节奏被权相更替、党争、废立、内廷干预打断

补全未来 `RegimeCycleState`：
- `dynasticFatigue`
- `regionalFracture`
- `recognitionPressure`
- `rebelGovernancePressure`
- `successionCrisisBand`
- `usurpationRisk`
- `restorationMomentum`

补全未来 `ClaimantOrRegimeActorState`：
- `actorId`
- `claimType`：Restoration / Usurpation / RebelPolity / LoyalistDefense / RegionalAutonomy
- `forceBacking`
- `grainBacking`
- `officeBacking`
- `publicLegitimacy`
- `ritualClaim`
- `recognitionMap`

补全未来 `CourtProcessState`：
- `courtDocketId`
- `agendaPressure`：Fiscal / Frontier / DisasterRelief / Appointment / Examination / Law / Ritual / Succession / Reform
- `memorialQueue`
- `debateHeat`
- `emperorAttention`
- `chancellorOrCouncilBacking`
- `censorPressure`
- `appointmentSlate`
- `policyWindow`
- `courtTimeDisruption`
- `localDispatchTargets`

朝会 / 廷议 / 奏议不是过场动画。它是把财政、边防、灾异、任命、科举、礼仪、继承和改革压力排序、压缩、派发、拖延或扭曲的流程。
一次朝会不应直接改全世界数值，而应产生：
- 任命名单、考课压力、贬谪 / 调任窗口
- 政策口径、赈济口径、税粮口径、军需口径
- 派系标签、弹劾风险、官员观望或站队
- 下发到州县后的执行质量、纸面服从、胥吏捕获、地方缓冲

补全未来 `OfficialActorState`：
- `actorId`
- `personId`
- `credentialBand`：DegreeHolder / Recommended / ClerkAttached / Appointed / Retired / Dismissed
- `currentPostId`
- `patronTies`
- `factionHeat`
- `clerkDependence`
- `familyPull`
- `privateObligation`
- `evaluationPressure`
- `memorialAttackRisk`
- `administrativeCapacity`

官员不是一枚官职棋子。官员要同时承受朝廷语言、任命制度、胥吏执行、家族私求、地方舆论、考课、弹劾、调任和财政边防压力。
官员线应该能回答：
- 他是否有名义权力？
- 他的书吏 / 衙役是否听话？
- 他的家族或姻亲是否拖他下水？
- 他是否欠恩主、怕弹劾、怕调任、怕地方反噬？
- 他执行的是朝廷原意、地方缓冲，还是被胥吏和豪强截走后的变形版本？

补全未来 `RegimeAuthorityState`：
- `regimeId`
- `legitimacyClaim`：DynasticContinuity / Restoration / Usurpation / RebelMandate / RegionalProtection / MilitaryNecessity
- `recognitionMap`
- `administrativeContinuity`
- `taxReach`
- `appointmentReach`
- `grainRouteReach`
- `forceBacking`
- `ritualClaim`
- `publicBelief`
- `officeDefection`
- `localCompliance`
- `frontierOrForeignPressure`

政权不是一个朝代名。政权在游戏里应表现为谁能发文、谁能任官、谁能收税粮、谁能维持漕运和军需、谁被地方官承认、谁能举行有说服力的礼仪、谁能让普通家户相信明天还按这套规矩活。
叛乱、建制、篡位、复辟、王朝修复都必须先经过保护失败、武力背书、粮道控制、官员倒向、公共合法性、仪礼名分、地方服从和记忆残留，不能一键改国号。

时间契约：
- seasonal / annual：王朝疲劳、边防和财政趋势
- month：国丧、大赦、储位、朝廷停摆对地方的传导
- command：后期玩家通过官府、军伍、派系、钱粮、舆论、继承条件介入

薄链：
`灾荒/战败/财政疲劳/继承不稳 -> 天命和朝廷节奏下滑 -> 地方执行变形 -> 叛乱/建制/继承斗争窗口 -> 玩家高阶影响圈介入 -> 政权级后果`

补充薄链：
- `奏议/朝会压力 -> 朝廷议程排序 -> 任命/政策窗口 -> 官署执行质量 -> 家户/市场/公共面变化 -> 弹劾、感恩、怨恨或地方缓冲`
- `政权合法性 -> 官员站队 -> 税粮/军需/舆论/礼仪承认 -> 地方服从或纸面服从 -> 叛乱、复辟、修复或新残留`
- `官员私家压力 -> 胥吏依赖和恩主关系 -> 案牍流向 -> 诉讼、税契、救济、征发、治安执行 -> 个人清名、家族声望和地方记忆`

边界：
- 不在 MVP 开启。
- 不做 detached grand strategy。
- 不做自由时间线编辑器。
- 必须通过既有模块压力和未来明确 owner 的状态进入存档。

## 第三章：实施顺序

**Phase 1a** ✅：PersonRegistry 骨架（Kernel 层身份锚点、`ClanMemberDied → PersonDeceased` 汇总、Scheduler Phase 0 hook）
**Phase 1b** ✅：PersonRegistry 接入所有 bootstrap、manifest 启用、seed heir 双写、金丝雀 integration 测试
**Phase 1c**：空间骨骼（WorldSettlements 节点分类 + 功能路线 + 水陆双拓扑 + 季节带 + 叠加 query 契约）—— 详见 `SPATIAL_SKELETON_SPEC.md`
**Phase 2**：各模块人物领域状态（按 `PERSON_OWNERSHIP_RULES.md` 分布到各模块；同时迁出 FamilyCore 的冗余年龄/存活字段，改读 PersonRegistry）
**Phase 3** ✅：生计骨骼（PopulationAndHouseholds 生计类型 + 摘要池 + `DeathByIllness` 通道）
**Phase 4** ✅：记忆骨骼（SocialMemoryAndRelations 事件记忆）
**Phase 5** ✅：商贸骨骼（TradeAndIndustry 商品 + 价格）
**Phase 6** ✅：科举 + 官场 + 匪患 + 武力 + 战役骨骼（含 ConflictAndForce 的 `DeathByViolence` 通道）；分解为 Phase 6/7/8/9/10 四骨骼依次落地（EducationAndExams / OfficeAndCareer / OrderAndBanditry / ConflictAndForce / WarfareCampaign）
**Phase 11**：公共生活骨骼补齐（PublicLifeAndRumor 场所/渠道/公共合法性信号，与 hall / desk / notice 只读投影对齐）
**Phase 12**：影响圈投影补齐（PlayerInfluenceFootprint / InfluenceReach 只读合成，确认玩家不是固定职业、默认族长或上帝）
**Phase 13**：历史大势骨骼预留（HistoricalProcess 先以趋势包语法、ExecPlan source note、模块映射表存在，不新增隐藏全局状态）
**Phase 14 / P5**：皇权、官员、朝会、政权与王朝循环骨骼（未来 `CourtAndThrone` / `WorldEvents` 或等价 pack，处理皇权节律、朝会议程、奏议 / 任命流程、官员站队、叛乱建制、继承斗争、篡位、复辟、王朝修复）

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
9. **公共合法性链**：Office / Family / Order / Warfare / Policy -> PublicLifeAndRumor -> SocialMemoryAndRelations -> NarrativeProjection
10. **影响圈链**：模块 read model -> Application 合成 -> shell 可见/可介入状态 -> 既有 command
11. **官员执行链**：OfficeAndCareer -> OfficialActor / clerk dependence -> petition / tax / relief / enforcement -> local residue
12. **朝会议程链**：CourtProcess -> memorial queue -> appointment / policy window -> local dispatch -> implementation drag
13. **皇权节律链**：CourtAndThrone / WorldEvents -> ImperialBand -> Office / World / PublicLife / Warfare -> 地方压力
14. **历史大势链**：TrendPressure -> ActorCarrier -> InstitutionalWindow -> PolicyBundle -> LocalImplementation -> CaptureAndResistance -> Residue
15. **政权承认链**：RegimeAuthority -> recognition / appointment / tax / grain / force / ritual -> local compliance / defection
16. **叛乱建制链**：ProtectionFailure -> BanditConcentration -> ArmedAutonomy -> RebelGovernance -> LegitimacyClaim -> PolityFormation
17. **王朝循环链**：DynasticFatigue -> SuccessionCrisis / RegionalFracture -> Usurpation / Restoration / RegimeRepair -> 新 residue

---

## 第五章：补正后的切分纪律

活世界补全必须遵守以下切分：

- **先投影，后命令**：能看到社会链，不等于能直接命令社会链。
- **先地方，后天下**：MVP 和 M2/M3 先证明地方生活、家户、宗族、市场、公共面能自转；皇权和王朝循环后期接入。
- **先压力，后剧情**：历史人物和改革先作为压力、窗口、派系、执行变形和记忆存在，不能先写成大事件。
- **先 owner，后 schema**：没有明确拥有模块，不新增保存状态。
- **先薄链，后密度**：每个骨骼必须先有一条可解释薄链，再逐渐加公式、权重、AI 行为和文本。
- **玩家可改史，但不裸改史**：后期可以谋权、叛乱、建制、篡位、复辟、修复王朝，但必须经过影响圈、资源、武力、官府、舆论、合法性、物流和记忆。

最终目标不是把模块堆满，而是让每月都能回答：
- 哪条社会链在动？
- 这条链从哪个模块压力开始？
- 谁承载了压力？
- 玩家当前够不够得着？
- 不管会怎样？
- 介入后留下什么账？

---

*本文档定义静态骨骼和补全路线。规则密度在骨骼稳定后逐链填充。*
