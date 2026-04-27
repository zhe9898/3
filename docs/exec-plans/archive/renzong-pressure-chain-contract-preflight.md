# Renzong 压力链 Contract Preflight

> 目标：在填充公式之前，逐个确认 `RENZONG_PRESSURE_CHAIN_SPEC.md` v0.2 中所有事件名的归属、类型（事件 vs 状态字段 vs 投影）、与现有代码的关系、以及是否需要公开到 `Zongzu.Contracts`。
> 日期：2026-04-21

---

## 1. 执行摘要

- **评审总量**：76 个名称（事件名/状态名/投影名）
- **已有常量**：6 个在 `Zongzu.Contracts` 中已有公开常量；约 20+ 个在模块内部以字符串字面量存在但未公开
- **与现有名称冲突/相似**：8 组需决策复用还是新建
- **状态字段误标为事件**：至少 4 个
- **建议用枚举替代的事件三元组**：1 组（Implementation outcome）
- **P5+ 专属（regime 链）**：约 12 个，MVP 阶段可 deferred

**核心决策待做**：
1. 命名风格统一：前缀（`Module.Name`）还是无前缀？
2. `EvaluationPressure` 等 4 个名称是状态字段，spec 中误标为事件——是否保留为事件或改为查询字段？
3. `ReliefDelivered`/`ReliefWithheld` 已存在，spec 的 `ReliefDistributed`/`ReliefFailed` 是否合并？
4. 6 个模块的事件名字面量从未公开到 Contracts，是否同步迁移？

---

## 2. 现有事件名清单（按模块，已公开到 Contracts）

| 模块 | 类名 | 数量 | 命名风格 | 关键名称 |
|---|---|---|---|---|
| `WorldSettlements` | `WorldSettlementsEventNames` | 15 | **前缀** `WorldSettlements.Xxx` | `SeasonPhaseAdvanced`, `ImperialRhythmChanged`, `RouteConstraintEmerged`, `ReliefDelivered`, `ReliefWithheld` |
| `FamilyCore` | `FamilyCoreEventNames` | 12 | 无前缀 | `ClanPrestigeAdjusted`, `MarriageAllianceArranged`, `BranchSeparationApproved` |
| `PopulationAndHouseholds` | `PopulationEventNames` | 5 | 无前缀 | `HouseholdDebtSpiked`, `MigrationStarted`, `LaborShortage`, `LivelihoodCollapsed` |
| `WarfareCampaign` | `WarfareCampaignEventNames` | 4 | 无前缀 | `CampaignMobilized`, `CampaignAftermathRegistered` |
| `PersonRegistry` | `PersonRegistryEventNames` + `DeathCauseEventNames` | 6 | 无前缀 | `PersonDeceased`, `DeathByIllness`, `DeathByViolence` |
| `PublicLifeAndRumor` | `PublicLifeAndRumorEventNames` | 5 | 无前缀 | `StreetTalkSurged`, `CountyGateCrowded`, `MarketBuzzRaised` |

**从未公开到 Contracts 的模块内部事件名**：

| 模块 | 当前位置 | 事件名 |
|---|---|---|
| `EducationAndExams` | 模块内部字符串 | `ExamPassed`, `ExamFailed`, `StudyAbandoned`, `TutorSecured` |
| `OfficeAndCareer` | 模块内部字符串 | `OfficeGranted`, `OfficeLost`, `OfficeTransfer`, `AuthorityChanged` |
| `OrderAndBanditry` | 模块内部字符串 | `BanditThreatRaised`, `OutlawGroupFormed`, `SuppressionSucceeded`, `RouteUnsafeDueToBanditry`, `BlackRoutePressureRaised` |
| `TradeAndIndustry` | 模块内部字符串 | `TradeProspered`, `TradeLossOccurred`, `TradeDebtDefaulted`, `RouteBusinessBlocked` |
| `ConflictAndForce` | 模块内部字符串 | `ConflictResolved`, `CommanderWounded`, `ForceReadinessChanged`, `MilitiaMobilized` |
| `SocialMemoryAndRelations` | 模块内部字符串 | `GrudgeEscalated`, `GrudgeSoftened`, `FavorIncurred`, `ClanNarrativeUpdated` |

---

## 3. 逐条评审（76 个名称）

### 3.1 已有常量（Existing）— 6 个 ✅

| # | 名称 | 归属模块 | 类型 | 现有位置 | 行动 |
|---|---|---|---|---|---|
| 1 | `HouseholdDebtSpiked` | `PopulationAndHouseholds` | DomainEvent | `PopulationEventNames` | 无变更，直接引用 |
| 2 | `MigrationStarted` | `PopulationAndHouseholds` | DomainEvent | `PopulationEventNames` | 无变更，直接引用 |
| 3 | `LaborShortage` | `PopulationAndHouseholds` | DomainEvent | `PopulationEventNames` | 无变更，直接引用 |
| 4 | `ClanPrestigeAdjusted` | `FamilyCore` | DomainEvent | `FamilyCoreEventNames` | 无变更，直接引用 |
| 5 | `DeathByIllness` | `PersonRegistry` | DomainEvent | `DeathCauseEventNames` | 无变更，直接引用 |
| 6 | `StudyAbandoned` | `EducationAndExams` | DomainEvent | 模块内部字符串 | **需迁移到 Contracts** |

> **注**：`StudyAbandoned` 在 spec 中标记为 Existing，但它目前只在 `EducationAndExams` 模块内部作为字符串字面量存在，未进入 `Zongzu.Contracts`。建议新建 `EducationAndExamsEventNames` 类并迁入。

---

### 3.2 与现有名称冲突/相似 — 8 组 ⚠️

| # | Spec 名称 | 现有相似名称 | 归属模块 | 建议 |
|---|---|---|---|---|
| 7 | `ImperialBandChanged` | `ImperialRhythmChanged` (`WorldSettlementsEventNames`) | `WorldSettlements` | **复用 `ImperialRhythmChanged`**。Spec 中语义完全一致（大赦/国丧/储位摇动/边防急报）。若未来需要区分 "band 内容变化" vs "rhythm 节拍变化"，可在 payload 中加 `bandKind` 字段，不必新建事件名。 |
| 8 | `ReliefDistributed` | `ReliefDelivered` (`WorldSettlementsEventNames`) | `WorldSettlements` | **复用 `ReliefDelivered`**。Spec 的 "赈济分发" 与现有 "救济送达" 语义相同。`OfficeAndCareer` 发出命令，实际 relief 事件应由 `WorldSettlements` 或 `PopulationAndHouseholds` 发出；`OfficeAndCareer` 不应直接发 relief 事件（边界问题）。 |
| 9 | `ReliefFailed` | `ReliefWithheld` (`WorldSettlementsEventNames`) | `WorldSettlements` | **复用 `ReliefWithheld`**。同理，"赈济失败" 与 "救济扣留/未能送达" 语义一致。 |
| 10 | `CampaignAftermath` | `CampaignAftermathRegistered` (`WarfareCampaignEventNames`) | `WarfareCampaign` | **复用 `CampaignAftermathRegistered`**。Spec 中的 `CampaignAftermath` 就是战役善后登记，完全对应。 |
| 11 | `OfficialArrived` | `OfficeGranted` (`OfficeAndCareer` 内部) | `OfficeAndCareer` | **复用 `OfficeGranted`**。"官员到任" 就是 "被授予官职"。到任后的 clerk-dependence 初始化是 `OfficeAndCareer` 内部状态转移，不需要单独事件。若需要通知下游，复用 `OfficeGranted` 并在 payload 中增加 `isNewArrival` 标志。 |
| 12 | `PostVacated` | `OfficeLost` / `OfficeTransfer` (`OfficeAndCareer` 内部) | `OfficeAndCareer` | **复用 `OfficeLost`**。调任导致的职位空缺是 `OfficeLost` 的一种原因。若需要区分原因，在 payload 中加 `reason: "transfer"`。 |
| 13 | `GrainRouteBlocked` | `RouteBusinessBlocked` (`TradeAndIndustry` 内部) / `RouteConstraintEmerged` (`WorldSettlementsEventNames`) | `TradeAndIndustry` | **新建 `GrainRouteBlocked`**，但注意语义区分。`RouteBusinessBlocked` 是商业路线受阻（商队层面），`RouteConstraintEmerged` 是世界层路线约束（洪水/战乱）。`GrainRouteBlocked` 是政权层粮道被封锁（P5+），属于不同抽象层。建议新建，前缀风格：`TradeAndIndustry.GrainRouteBlocked`。 |
| 14 | `GrainRouteReopened` | `RouteConstraintCleared` (`WorldSettlementsEventNames`) | `TradeAndIndustry` | **新建 `GrainRouteReopened`**。同理，与 `RouteConstraintCleared` 区分抽象层。前缀风格：`TradeAndIndustry.GrainRouteReopened`。 |

---

### 3.3 状态字段误标为事件 — 4 个 🔴

| # | Spec 名称 | 实际代码状态 | 正确归类 | 建议 |
|---|---|---|---|---|
| 15 | `EvaluationPressure` | `OfficeAndCareerState` 有 `int EvaluationPressure { get; set; }` | **状态字段** | 从事件列表中移除。若需要下游感知，应通过 `EvaluationScoreLow` 事件（新名）或让下游通过 Query 读取。不应把内部计分压力直接暴露为跨模块事件。 |
| 16 | `ClerkCaptureRisk` | 无代码，但语义是 "风险等级" | **状态字段 / 投影** | 不是事件。`clerkDependence > threshold` 是一种状态条件，应作为 `OfficeAndCareer` 内部状态字段。若需要通知下游，用状态变化事件如 `ClerkCaptureDeepened`（当风险实现为深度俘获时）。 |
| 17 | `WaitingListFrustration` | 无代码，语义是 "候补队列挫折感" | **状态字段** | 不是事件。`waitingMonths` 是 `OfficeAndCareer` 内部状态。挫折感应由 `PublicLifeAndRumor` 通过 Query 读取 waiting list 长度后自行投影为 narrative。若必须跨模块传播，建议用 `WaitingListEntryAged` 事件（每 xun 发出一次，携带 `personId, waitingMonths`）。 |
| 18 | `OfficialDefectionRisk` | 无代码，语义是 "叛变风险" | **状态字段 / 投影** | 不是事件。风险是一种概率/倾向，应由 `OfficeAndCareer` 内部维护。当风险实现为实际叛变时，才发出 `OfficeDefected` 事件。 |

---

### 3.4 建议用枚举替代 — 1 组 🟡

| # | Spec 名称 | 归属模块 | 建议 |
|---|---|---|---|
| 19 | `RapidImplementation` | `OfficeAndCareer` | **不建议作为独立事件名**。Policy implementation 的结果应是一个枚举值：`ImplementationOutcome { Rapid, Dragged, Captured }`。单一事件 `PolicyImplemented` 携带 `outcome: ImplementationOutcome` 和 `delayMonths`/`capturingActor` 等字段。这样更紧凑，且避免三个几乎相同的事件处理逻辑。 |
| 20 | `ImplementationDrag` | `OfficeAndCareer` | 同上，合并为 `PolicyImplemented` + outcome enum |
| 21 | `ImplementationCaptured` | `OfficeAndCareer` | 同上，合并为 `PolicyImplemented` + outcome enum |

---

### 3.5 投影 vs 事件 — 3 个 🟡

| # | Spec 名称 | 归属模块 | 建议 |
|---|---|---|---|
| 22 | `HouseholdPetitionFrustration` | `PublicLifeAndRumor`（投影）或 `PopulationAndHouseholds`（状态） | **不作为 DomainEvent**。这是 `PublicLifeAndRumor` 通过 Query 读取 `OfficeAndCareer` 的 petition backlog 后合成的公共情绪投影。`PopulationAndHouseholds` 也不应发此事件——家户的 frustration 通过 `HouseholdResistance` 或 `HouseholdBurdenIncreased` 表达。 |
| 23 | `PublicLegitimacyShifted` | `PublicLifeAndRumor` | **不作为 DomainEvent 存档**。`PublicLifeAndRumor` 拥有 `PublicLegitimacySignal` 作为其自身状态。合法性变化是 `PublicLifeAndRumor` 内部状态转移，通过 Query 对外提供投影。但若其他模块需要**确定性**地响应合法性变化（如 `PopulationAndHouseholds` 的 compliance 计算），可以发出事件。建议：**保留为事件**，但明确由 `PublicLifeAndRumor` 发出，作为 "下游模块可以订阅的公开信号"。 |
| 24 | `HouseholdCompliance` / `HouseholdResistance` | `PopulationAndHouseholds` | **状态字段，不是事件**。Compliance/resistance 是家户的行为倾向或状态标签（如 `ComplianceStance { Compliant, Resistant, Evasive }`）。当 stance 变化时，可发出 `HouseholdStanceChanged` 事件（若需要跨模块传播）。Spec 中的两个名称建议合并为一个事件名 `HouseholdStanceChanged`，携带 `newStance` 字段。 |

---

### 3.6 P5+ 专属（Regime 链）— 12 个 ⏳

以下事件在 MVP 阶段不激活，但静态结构需预留。建议**先定义事件名常量，handler 留空实现**。

| # | 名称 | 归属模块 | 行动 |
|---|---|---|---|
| 25 | `RegimeLegitimacyShifted` | `WorldSettlements` | 新建常量，前缀风格 |
| 26 | `OfficeDefected` | `OfficeAndCareer` | 新建常量 |
| 27 | `HouseholdRegimeTransition` | `PopulationAndHouseholds` | 新建常量 |
| 28 | `RouteControlShifted` | `TradeAndIndustry` | 新建常量 |
| 29 | `GrainRouteControlDisputed` | `WorldSettlements` | 新建常量 |
| 30 | `RitualClaimStaged` | `WorldSettlements` | 新建常量 |
| 31 | `GrainRouteBlocked` | `TradeAndIndustry` | 新建常量（已在 3.2 讨论） |
| 32 | `GrainRouteReopened` | `TradeAndIndustry` | 新建常量（已在 3.2 讨论） |

> 注意：`OfficialDefectionRisk` 已归入 3.3（状态字段），不应作为事件。

---

### 3.7 其余 Proposed 事件名 — 逐个确认

以下事件名经评审后确认为**合法的 DomainEvent**，需要新建 `Zongzu.Contracts` 常量。

#### 链1：税役-家户-衙门

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 33 | `TaxSeasonOpened` | `WorldSettlements` | 季节节律事件。前缀风格：`WorldSettlements.TaxSeasonOpened`。与 `CorveeWindowChanged` 并列。 |
| 34 | `YamenOverloaded` | `OfficeAndCareer` | Workload 超限事件。需新建 `OfficeAndCareerEventNames`。 |
| 35 | `TenantFlightRisk` | `PopulationAndHouseholds` | 佃户逃离风险实现时发出。注意："风险"一词易混淆——此事件应在 flight 实际发生或迫在眉睫时发出，而不是概率更新。建议改名为 `TenantFlightTriggered` 以避免与状态字段混淆。 |

#### 链2：粮价-市场-家户

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 36 | `GrainPriceSpike` | `TradeAndIndustry` | 价格暴涨事件。需新建 `TradeAndIndustryEventNames`。 |
| 37 | `GrainPriceSpikeRisk` | `TradeAndIndustry` | 风险预警事件。注意与 `GrainPriceSpike` 的关系：risk 是预警（可能不实现），spike 是实现。保留两者，但确保 handler 区分。 |
| 38 | `MarketPanicRisk` | `TradeAndIndustry` | 市场恐慌风险。同上，预警类事件。 |
| 39 | `OfficialPurchasingPressure` | `TradeAndIndustry` | 官方采购压力。由 `OfficeAndCareer` 的军需命令触发，`TradeAndIndustry` 响应后发出。 |
| 40 | `HouseholdSubsistenceStrain` | `PopulationAndHouseholds` | 家户生存压力。 strain < critical。 |
| 41 | `HouseholdSubsistenceCrisis` | `PopulationAndHouseholds` | 家户生存危机。 strain >= critical。建议合并为 `HouseholdSubsistenceStrained`，携带 `severity: { Strain, Crisis }` 枚举，减少事件名数量。 |

#### 链3：科举-教育-官员

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 42 | `ExamSeasonOpened` | `WorldSettlements` | 季节节律。前缀风格：`WorldSettlements.ExamSeasonOpened`。 |
| 43 | `ExamAttemptResolved` | `EducationAndExams` | 考试结果事件。携带 `result: { Pass, Fail }`。一个事件名覆盖两种结果，通过 payload 区分。 |
| 44 | `AppointmentQueuePressure` | `OfficeAndCareer` | 候补队列压力。当 queue length > threshold 时发出。 |
| 45 | `AppointmentMade` | `OfficeAndCareer` | 任命完成。与 `OfficeGranted` 的关系：`OfficeGranted` 是 "被授予官职"（个人层面），`AppointmentMade` 是 "完成任命到具体岗位"（岗位层面）。两者可同时存在：先 `OfficeGranted`，后 `AppointmentMade`。或合并：`OfficeGranted` 增加 `postId` payload。建议**合并到 `OfficeGranted`**——任命就是授官。 |
| 46 | `HouseholdLaborReabsorbed` | `PopulationAndHouseholds` | 考生回归劳动力。合法事件。 |

#### 链4：皇权-地方

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 47 | `AmnestyApplied` | `OfficeAndCareer` | 大赦执行。注意：`OfficeAndCareer` 发出命令，`OrderAndBanditry` 执行。事件由 `OrderAndBanditry` 在 actually processed 后发出更合理？当前 spec 中 `OfficeAndCareer` 发出。建议：**由 `OrderAndBanditry` 发出**，因为治安后果（释放、再犯风险）是 Order 的权威状态。`OfficeAndCareer` 可发 `AmnestyOrdered`（命令层面），`OrderAndBanditry` 响应后发 `AmnestyApplied`（执行层面）。但为减少事件名，可以只保留一个：由执行模块 `OrderAndBanditry` 发出 `AmnestyApplied`。 |
| 48 | `CourtMourning` | `WorldSettlements` | 国丧宣布。`WorldSettlements` 拥有 `ImperialBand`，由其发出。前缀风格。 |
| 49 | `FrontierSupplyDemand` | `WorldSettlements` | 边防军需需求。`WorldSettlements` 发出，作为 imperial band 的一种。前缀风格。 |
| 50 | `OfficialSupplyRequisition` | `OfficeAndCareer` | 官方征发命令。由 `OfficeAndCareer` 发出。 |
| 51 | `LaborDraftPressure` | `PopulationAndHouseholds` | 征兵压力。当 draft quota > available 时发出。 |
| 52 | `HouseholdResistance` | `PopulationAndHouseholds` | 家户抵抗。见 3.5，建议改为 `HouseholdStanceChanged` + `stance: Resistant`。 |
| 53 | `HouseholdBurdenIncreased` | `PopulationAndHouseholds` | 负担增加。合法事件。 |

#### 链5：边防-军需

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 54 | `FrontierStrainEscalated` | `WorldSettlements` | 边防紧张升级。前缀风格。 |
| 55 | `MobilizationWindowOpened` | `WarfareCampaign` | 动员窗口。`WarfareCampaign` 发出。 |
| 56 | `CampaignCommitted` | `WarfareCampaign` | 战役承诺/启动。`WarfareCampaign` 发出。 |
| 57 | `ForceReadinessDemand` | `ConflictAndForce` | 武力战备需求。`ConflictAndForce` 发出。 |
| 58 | `RetainerMobilized` | `FamilyCore` | 家丁动员。由 `FamilyCore` 发出（因为 retainer 属于 clan）。注意：`ConflictAndForce` 查询 `FamilyCore` 的 retainer 状态后，家族自行决定是否动员。 |
| 59 | `ForceFatigueIncreased` | `ConflictAndForce` | 武力疲劳增加。合法事件。 |
| 60 | `HouseholdWarLoss` | `PopulationAndHouseholds` | 战亡家庭损失。合法事件。 |
| 61 | `RouteRepairNeed` | `WarfareCampaign` | 路线修复需求。合法事件。虽然 `WarfareCampaign` 有 `RouteRepairs` 状态字段，但 "需要修复" 是一个事件（触发后续资源调配）。 |

#### 链6：灾荒-赈济

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 62 | `DisasterDeclared` | `WorldSettlements` | 灾荒宣布。前缀风格。 |
| 63 | `DisorderSpike` | `OrderAndBanditry` | 失序激增。合法事件。与 `BanditThreatRaised` 区分：`DisorderSpike` 是广义社会失序（灾民骚乱等），`BanditThreatRaised` 是匪患威胁。 |
| 64 | `BanditRecruitmentOpportunity` | `OrderAndBanditry` | 匪患招募机会。合法事件。 |
| 65 | `BanditHotspotActivated` | `OrderAndBanditry` | 匪患热点激活。合法事件。与 `OutlawGroupFormed` 的关系：`OutlawGroupFormed` 是匪帮实体创建，`BanditHotspotActivated` 是区域风险状态变化。可以共存。 |
| 66 | `DisorderRiskAdjusted` | `OrderAndBanditry` | 失序风险调整。注意：这是一个**状态更新**，不是瞬态事件。建议改为 `DisorderLevelChanged` 或作为状态字段。若必须事件化，保留但明确其是 periodic summary 事件（每月/每旬发出一次当前 level）。 |
| 67 | `RouteInsecuritySpike` | `OrderAndBanditry` | 路线不安全激增。合法事件。 |

#### 链7：官员-胥吏

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 68 | `EvaluationCycleTriggered` | `OfficeAndCareer` | 考课周期触发。注意：这是一个**内部节奏信号**，不是跨模块事件。`OfficeAndCareer` 的 cadence 是 monthly，考课周期由其内部 scheduler 管理。建议**不作为公开事件**。若需要让 `SocialMemoryAndRelations` 记录考课结果，可在考课完成后发 `EvaluationCompleted` 事件（携带 score）。 |
| 69 | `MemorialAttackReceived` | `OfficeAndCareer` | 弹劾/奏章攻击收到。合法事件。 |
| 70 | `PetitionBacklogExceeded` | `OfficeAndCareer` | 诉状积压超限。合法事件。注意：这是 workload 的一种，可与 `YamenOverloaded` 合并？`YamenOverloaded` 是广义衙门超载，`PetitionBacklogExceeded` 是具体 petition 队列超限。建议保留两者，`YamenOverloaded` 携带 `taskKind: "petition-backlog"` 即可覆盖此场景。 |
| 71 | `ClerkCaptureDeepened` | `OfficeAndCareer` | 胥吏俘获加深。合法事件。这是状态变化事件（clerk dependence 越过新阈值）。 |
| 72 | `ImplementationDistorted` | `OfficeAndCareer` | 执行扭曲。见 3.4，建议合并到 `PolicyImplemented` + outcome enum。若保留独立事件，应明确是 `OfficeAndCareer` 对执行结果的记录（query 下游状态后发出）。 |

#### 链8：朝会-政策

| # | 名称 | 归属模块 | 备注 |
|---|---|---|---|
| 73 | `CourtAgendaPressureAccumulated` | `WorldSettlements` | 朝廷议程压力积累。`WorldSettlements` 拥有 agenda 积累状态。前缀风格。注意：这是**状态 summary 事件**（每月/每季发出当前积累值），不是瞬态事件。 |
| 74 | `PolicyWindowOpened` | `OfficeAndCareer` | 政策窗口打开。P5 以后才激活。合法事件。 |
| 75 | `AppointmentSlateProposed` | `OfficeAndCareer` | 任命名单提议。P5 以后。合法事件。 |
| 76 | `PolicyWordingDrafted` | `OfficeAndCareer` | 政策措辞草案。P5 以后。合法事件。注意：这是高阶 court 内容，MVP 可能只有空 handler。 |
| 77 | `DispatchArrived` | `OfficeAndCareer` | 敕牒到达地方。合法事件。由 `OfficeAndCareer` 高层发出，地方层接收。 |

---

## 4. 命名风格决策

### 4.1 现状

| 模块 | 风格 | 示例 |
|---|---|---|
| `WorldSettlements` | **前缀** `Module.Name` | `WorldSettlements.SeasonPhaseAdvanced` |
| 其他所有模块 | **无前缀** | `ExamPassed`, `OfficeGranted` |

### 4.2 问题

- 前缀风格消除了跨模块碰撞风险，但 `WorldSettlements` 是唯一使用前缀的模块。
- 无前缀风格简洁，但随着事件名增加（尤其是 P5+ 的 regime 链），碰撞风险上升。
- `TradeAndIndustry` 的 `RouteBusinessBlocked` 与 `WorldSettlements` 的 `RouteConstraintEmerged` 已经在语义上接近，如果未来再加 `GrainRouteBlocked`，无前缀风格会让消费者难以区分来源。

### 4.3 建议

**选项 A：统一前缀风格（推荐）**
- 所有新事件名采用 `Module.EventName` 格式
- 现有无前缀常量保持不变（向后兼容）
- 新常量类示例：`OfficeAndCareerEventNames.OfficeGranted = "OfficeAndCareer.OfficeGranted"`

**选项 B：保持现状，按模块习惯**
- `WorldSettlements` 继续前缀
- 其他模块继续无前缀
- 风险：未来碰撞由代码审查捕获

**选项 C：统一无前缀，但加命名空间保护**
- 通过 C# namespace + class name 提供保护（`OfficeAndCareerEventNames.OfficeGranted`）
- 字符串值本身无前缀
- 当前实际就是这样做的（除了 WorldSettlements 的字符串值也加了前缀）

**推荐选项 A**，理由：
1. 事件名字符串是跨模块通信的契约，类名命名空间保护在运行时无效（字符串比较）
2. P5+ regime 链将引入大量新事件，碰撞风险不可接受
3. `WorldSettlements` 已经证明前缀风格可行
4. 逐步迁移成本低：新事件用前缀，旧事件保持不变

---

## 5. 新建 Contracts 类规划

### 5.1 必须新建（支撑 Renzong 压力链）

| 类名 | 模块 | 事件数量 | 前缀风格 | 备注 |
|---|---|---|---|---|
| `EducationAndExamsEventNames` | `EducationAndExams` | 4+ | 是 | 迁入现有 `ExamPassed`, `ExamFailed`, `StudyAbandoned`, `TutorSecured`；新增 `ExamAttemptResolved`, `ExamSeasonOpened` |
| `OfficeAndCareerEventNames` | `OfficeAndCareer` | 8+ | 是 | 迁入现有 `OfficeGranted`, `OfficeLost`, `OfficeTransfer`, `AuthorityChanged`；新增 `YamenOverloaded`, `AppointmentQueuePressure`, `AmnestyApplied`, `OfficialSupplyRequisition`, `MemorialAttackReceived`, `ClerkCaptureDeepened`, `PolicyWindowOpened`, `AppointmentSlateProposed`, `PolicyWordingDrafted`, `DispatchArrived` 等 |
| `OrderAndBanditryEventNames` | `OrderAndBanditry` | 7+ | 是 | 迁入现有 `BanditThreatRaised`, `OutlawGroupFormed`, `SuppressionSucceeded`, `RouteUnsafeDueToBanditry`, `BlackRoutePressureRaised`；新增 `DisorderSpike`, `BanditRecruitmentOpportunity`, `BanditHotspotActivated`, `DisorderRiskAdjusted`/`DisorderLevelChanged`, `RouteInsecuritySpike` |
| `TradeAndIndustryEventNames` | `TradeAndIndustry` | 6+ | 是 | 迁入现有 `TradeProspered`, `TradeLossOccurred`, `TradeDebtDefaulted`, `RouteBusinessBlocked`；新增 `GrainPriceSpike`, `GrainPriceSpikeRisk`, `MarketPanicRisk`, `OfficialPurchasingPressure`, `MarketDiversion`, `GrainRouteBlocked`, `GrainRouteReopened` |
| `ConflictAndForceEventNames` | `ConflictAndForce` | 4+ | 是 | 迁入现有 `ConflictResolved`, `CommanderWounded`, `ForceReadinessChanged`, `MilitiaMobilized`；新增 `ForceReadinessDemand`, `ForceFatigueIncreased` |
| `PopulationEventNames` 扩展 | `PopulationAndHouseholds` | 已有 5 个 | 是（新名）/否（旧名） | 新增 `TenantFlightTriggered`, `HouseholdSubsistenceStrained`, `HouseholdLaborReabsorbed`, `LaborDraftPressure`, `HouseholdBurdenIncreased`, `HouseholdWarLoss`, `HouseholdStanceChanged`, `HouseholdRegimeTransition` |
| `FamilyCoreEventNames` 扩展 | `FamilyCore` | 已有 12 个 | 否（保持） | 新增 `RetainerMobilized` |
| `WorldSettlementsEventNames` 扩展 | `WorldSettlements` | 已有 15 个 | 是（保持） | 新增 `TaxSeasonOpened`, `ExamSeasonOpened`, `CourtMourning`, `FrontierSupplyDemand`, `FrontierStrainEscalated`, `DisasterDeclared`, `CourtAgendaPressureAccumulated`, `RegimeLegitimacyShifted`, `GrainRouteControlDisputed`, `RitualClaimStaged` |
| `SocialMemoryAndRelationsEventNames` | `SocialMemoryAndRelations` | 4 | 是 | 迁入现有 `GrudgeEscalated`, `GrudgeSoftened`, `FavorIncurred`, `ClanNarrativeUpdated` |

### 5.2 建议合并/删除的事件名（减少总数）

| 原 Spec 名称 | 操作 | 替代方案 |
|---|---|---|
| `EvaluationPressure` | **删除**（从事件列表移除） | 状态字段，不跨模块传播。下游通过 Query 读取 `OfficeAndCareerState.EvaluationPressure` |
| `ClerkCaptureRisk` | **删除** | 状态字段。当 risk 实现时发 `ClerkCaptureDeepened` |
| `WaitingListFrustration` | **删除** | 状态字段。发 `WaitingListEntryAged`（若需要）或让 `PublicLifeAndRumor` 自行投影 |
| `OfficialDefectionRisk` | **删除** | 状态字段。当 defection 实现时发 `OfficeDefected` |
| `HouseholdSubsistenceStrain` + `HouseholdSubsistenceCrisis` | **合并** | `HouseholdSubsistenceStrained` + `severity: { Strain, Crisis }` |
| `RapidImplementation` + `ImplementationDrag` + `ImplementationCaptured` | **合并** | `PolicyImplemented` + `ImplementationOutcome { Rapid, Dragged, Captured }` |
| `HouseholdResistance` + `HouseholdCompliance` | **合并** | `HouseholdStanceChanged` + `ComplianceStance { Compliant, Resistant, Evasive }` |
| `AppointmentMade` | **合并到 `OfficeGranted`** | `OfficeGranted` 增加 `postId, location, rank` payload |
| `YamenOverloaded` + `PetitionBacklogExceeded` | **合并** | `YamenOverloaded` 增加 `taskKind: "petition-backlog"` payload |
| `ImperialBandChanged` | **复用 `ImperialRhythmChanged`** | 增加 `bandKind` payload |
| `ReliefDistributed` | **复用 `ReliefDelivered`** | 无变更 |
| `ReliefFailed` | **复用 `ReliefWithheld`** | 无变更 |
| `CampaignAftermath` | **复用 `CampaignAftermathRegistered`** | 无变更 |
| `OfficialArrived` | **复用 `OfficeGranted`** | 增加 `isNewArrival` payload |
| `PostVacated` | **复用 `OfficeLost`** | 增加 `reason: "transfer"` payload |
| `EvaluationCycleTriggered` | **删除** | 内部节奏，不发事件。考课完成后发 `EvaluationCompleted`（若需要） |
| `HouseholdPetitionFrustration` | **删除** | `PublicLifeAndRumor` 投影，不作为事件 |
| `DisorderRiskAdjusted` | **改为状态字段或 summary 事件** | `DisorderLevelChanged`（每月/每旬 summary） |

**合并后净新增事件名估算**：
- 原始 Proposed：约 68 个
- 删除/合并后：约 **45 个** 真正需要的新事件常量
- 加上 20+ 个需要迁移到 Contracts 的现有内部字符串
- 总计需要 Contracts 覆盖约 **65 个** 事件名

---

## 6. 发现的问题清单

### 6.1 🔴 模块边界违规（当前代码）

| 问题 | 位置 | 说明 |
|---|---|---|
| `SocialMemoryAndRelations` 内部重复定义贸易事件字符串 | `SocialMemoryAndRelationsModule.cs:49-55` | 私有类 `TradeShockEventTypes` 重新定义了 `RouteBusinessBlocked` 等 4 个字符串，与 `TradeAndIndustry` 的事件名字面量相同。虽然字符串值相同可以工作，但这是隐性耦合。建议：`SocialMemoryAndRelations` 应通过 `Zongzu.Contracts` 引用 `TradeAndIndustryEventNames`（待新建）。 |
| `OfficeAndCareer` 和 `OrderAndBanditry` 引用 `PublicLifeAndRumorEventNames` | 两模块的 `ConsumedEventNames` | 这**不是**违规，因为 `PublicLifeAndRumorEventNames` 在 `Zongzu.Contracts` 中。但 `PublicLifeAndRumorEventNames` 使用无前缀风格，未来如果其他模块也有 `StreetTalkSurged` 字符串字面量，会发生碰撞。 |

### 6.2 🟡 事件名语义重叠

| 问题 | 涉及事件 | 建议 |
|---|---|---|
| `ReliefDelivered` vs `ReliefWithheld` 的归属 | 当前在 `WorldSettlementsEventNames` | 赈济/救济的决策者是 `OfficeAndCareer`，执行者是 `PopulationAndHouseholds` 或 `OrderAndBanditry`。`WorldSettlements` 拥有 `ReliefDelivered`/`ReliefWithheld` 略显越界。建议：将这两个事件迁移到 `PopulationEventNames`（由 Population 模块在 actually distributed 后发出），或保持现状但更新边界文档。 |
| `TradeAndIndustry` 与 `SocialMemoryAndRelations` 的贸易事件消费 | `TradeDebtDefaulted` 等 | `SocialMemoryAndRelations` 消费贸易事件来创建 clan narrative。这是合法的跨模块事件流，但需确保 `TradeAndIndustry` 确实发布这些事件（当前是的）。 |

### 6.3 🟡 状态字段与事件混淆

- `OfficeAndCareerState` 中的 `EvaluationPressure`, `ClerkDependence`, `PetitionBacklog` 等是内部状态
- Spec 中把它们的一些变化标为事件，会导致过度的事件化
- 原则：**只有当变化需要跨模块确定性响应时，才升级为事件**。如果只是让 `PublicLifeAndRumor` 做 narrative 投影，用 Query 即可。

---

## 7. 推荐行动项

### 7.1 立即执行（P0）

1. **决策命名风格**：在 `docs/RENZONG_PRESSURE_CHAIN_SPEC.md` 中确认统一前缀风格
2. **创建 6 个新的 Contracts 常量类**：`EducationAndExamsEventNames`, `OfficeAndCareerEventNames`, `OrderAndBanditryEventNames`, `TradeAndIndustryEventNames`, `ConflictAndForceEventNames`, `SocialMemoryAndRelationsEventNames`
3. **迁移现有内部字符串**：将 20+ 个模块内部的裸字符串事件名迁入对应 Contracts 类
4. **更新 Spec v0.3**：
   - 删除 `EvaluationPressure`, `ClerkCaptureRisk`, `WaitingListFrustration`, `OfficialDefectionRisk`, `HouseholdPetitionFrustration` 从事件列表
   - 合并 `HouseholdSubsistenceStrain`+`Crisis`, `RapidImplementation`+`Drag`+`Captured`, `HouseholdResistance`+`Compliance`
   - 标注复用关系：`ImperialBandChanged`→`ImperialRhythmChanged`, `ReliefDistributed`→`ReliefDelivered`, 等
5. **修复 `SocialMemoryAndRelations` 的 `TradeShockEventTypes`**：改为引用 `Zongzu.Contracts.TradeAndIndustryEventNames`（新建后）

### 7.2 短期执行（P1）

6. 为每个链写**一条薄规则链测试**（验证事件流连通性，不验证公式）
7. 补充事件 handler 的**空实现**（接口存在，body 为 `// TODO: Step 3`）
8. 更新 `MODULE_BOUNDARIES.md` 和 `DATA_SCHEMA.md` 中的事件归属表

### 7.3 中期执行（P2）

9. 填充压力公式和阈值（Step 3 of 7）
10. 连接 Unity 壳层投影（Step 7 of 7）

---

## 8. 附录：事件名 → 模块归属速查表（去重合并后）

| 事件名 | 归属模块 | 类型 | 状态 |
|---|---|---|---|
| `WorldSettlements.SeasonPhaseAdvanced` | `WorldSettlements` | DomainEvent | 已有 |
| `WorldSettlements.TaxSeasonOpened` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.ExamSeasonOpened` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.ImperialRhythmChanged` | `WorldSettlements` | DomainEvent | 已有（复用原 `ImperialBandChanged`） |
| `WorldSettlements.CourtMourning` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.FrontierSupplyDemand` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.FrontierStrainEscalated` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.DisasterDeclared` | `WorldSettlements` | DomainEvent | 新建 |
| `WorldSettlements.CourtAgendaPressureAccumulated` | `WorldSettlements` | DomainEvent | 新建（P5+） |
| `WorldSettlements.RegimeLegitimacyShifted` | `WorldSettlements` | DomainEvent | 新建（P5+） |
| `WorldSettlements.GrainRouteControlDisputed` | `WorldSettlements` | DomainEvent | 新建（P5+） |
| `WorldSettlements.RitualClaimStaged` | `WorldSettlements` | DomainEvent | 新建（P5+） |
| `WorldSettlements.RouteConstraintEmerged` | `WorldSettlements` | DomainEvent | 已有 |
| `WorldSettlements.RouteConstraintCleared` | `WorldSettlements` | DomainEvent | 已有 |
| `WorldSettlements.FloodRiskThresholdBreached` | `WorldSettlements` | DomainEvent | 已有 |
| `WorldSettlements.ReliefDelivered` | `WorldSettlements` | DomainEvent | 已有 |
| `WorldSettlements.ReliefWithheld` | `WorldSettlements` | DomainEvent | 已有 |
| `PopulationAndHouseholds.HouseholdDebtSpiked` | `PopulationAndHouseholds` | DomainEvent | 已有 |
| `PopulationAndHouseholds.MigrationStarted` | `PopulationAndHouseholds` | DomainEvent | 已有 |
| `PopulationAndHouseholds.LaborShortage` | `PopulationAndHouseholds` | DomainEvent | 已有 |
| `PopulationAndHouseholds.LivelihoodCollapsed` | `PopulationAndHouseholds` | DomainEvent | 已有 |
| `PopulationAndHouseholds.TenantFlightTriggered` | `PopulationAndHouseholds` | DomainEvent | 新建（原 `TenantFlightRisk`） |
| `PopulationAndHouseholds.HouseholdSubsistenceStrained` | `PopulationAndHouseholds` | DomainEvent | 新建（合并 Strain+Crisis） |
| `PopulationAndHouseholds.HouseholdLaborReabsorbed` | `PopulationAndHouseholds` | DomainEvent | 新建 |
| `PopulationAndHouseholds.LaborDraftPressure` | `PopulationAndHouseholds` | DomainEvent | 新建 |
| `PopulationAndHouseholds.HouseholdBurdenIncreased` | `PopulationAndHouseholds` | DomainEvent | 新建 |
| `PopulationAndHouseholds.HouseholdWarLoss` | `PopulationAndHouseholds` | DomainEvent | 新建 |
| `PopulationAndHouseholds.HouseholdStanceChanged` | `PopulationAndHouseholds` | DomainEvent | 新建（合并 Resistance+Compliance） |
| `PopulationAndHouseholds.HouseholdRegimeTransition` | `PopulationAndHouseholds` | DomainEvent | 新建（P5+） |
| `PopulationAndHouseholds.DeathByIllness` | `PersonRegistry` | DomainEvent | 已有 |
| `TradeAndIndustry.GrainPriceSpike` | `TradeAndIndustry` | DomainEvent | 新建 |
| `TradeAndIndustry.GrainPriceSpikeRisk` | `TradeAndIndustry` | DomainEvent | 新建 |
| `TradeAndIndustry.MarketPanicRisk` | `TradeAndIndustry` | DomainEvent | 新建 |
| `TradeAndIndustry.OfficialPurchasingPressure` | `TradeAndIndustry` | DomainEvent | 新建 |
| `TradeAndIndustry.MarketDiversion` | `TradeAndIndustry` | DomainEvent | 新建 |
| `TradeAndIndustry.TradeProspered` | `TradeAndIndustry` | DomainEvent | 已有（迁移到 Contracts） |
| `TradeAndIndustry.TradeLossOccurred` | `TradeAndIndustry` | DomainEvent | 已有（迁移到 Contracts） |
| `TradeAndIndustry.TradeDebtDefaulted` | `TradeAndIndustry` | DomainEvent | 已有（迁移到 Contracts） |
| `TradeAndIndustry.RouteBusinessBlocked` | `TradeAndIndustry` | DomainEvent | 已有（迁移到 Contracts） |
| `TradeAndIndustry.GrainRouteBlocked` | `TradeAndIndustry` | DomainEvent | 新建（P5+） |
| `TradeAndIndustry.GrainRouteReopened` | `TradeAndIndustry` | DomainEvent | 新建（P5+） |
| `OfficeAndCareer.OfficeGranted` | `OfficeAndCareer` | DomainEvent | 已有（迁移到 Contracts，合并 `OfficialArrived`） |
| `OfficeAndCareer.OfficeLost` | `OfficeAndCareer` | DomainEvent | 已有（迁移到 Contracts，合并 `PostVacated`） |
| `OfficeAndCareer.OfficeTransfer` | `OfficeAndCareer` | DomainEvent | 已有（迁移到 Contracts） |
| `OfficeAndCareer.AuthorityChanged` | `OfficeAndCareer` | DomainEvent | 已有（迁移到 Contracts） |
| `OfficeAndCareer.YamenOverloaded` | `OfficeAndCareer` | DomainEvent | 新建（合并 `PetitionBacklogExceeded`） |
| `OfficeAndCareer.AppointmentQueuePressure` | `OfficeAndCareer` | DomainEvent | 新建 |
| `OfficeAndCareer.AmnestyApplied` | `OfficeAndCareer` | DomainEvent | 新建 |
| `OfficeAndCareer.OfficialSupplyRequisition` | `OfficeAndCareer` | DomainEvent | 新建 |
| `OfficeAndCareer.MemorialAttackReceived` | `OfficeAndCareer` | DomainEvent | 新建 |
| `OfficeAndCareer.ClerkCaptureDeepened` | `OfficeAndCareer` | DomainEvent | 新建 |
| `OfficeAndCareer.PolicyImplemented` | `OfficeAndCareer` | DomainEvent | 新建（合并 Rapid/Drag/Captured） |
| `OfficeAndCareer.PolicyWindowOpened` | `OfficeAndCareer` | DomainEvent | 新建（P5+） |
| `OfficeAndCareer.AppointmentSlateProposed` | `OfficeAndCareer` | DomainEvent | 新建（P5+） |
| `OfficeAndCareer.PolicyWordingDrafted` | `OfficeAndCareer` | DomainEvent | 新建（P5+） |
| `OfficeAndCareer.DispatchArrived` | `OfficeAndCareer` | DomainEvent | 新建（P5+） |
| `OfficeAndCareer.EvaluationCompleted` | `OfficeAndCareer` | DomainEvent | 新建（替代 `EvaluationCycleTriggered`） |
| `EducationAndExams.ExamPassed` | `EducationAndExams` | DomainEvent | 已有（迁移到 Contracts） |
| `EducationAndExams.ExamFailed` | `EducationAndExams` | DomainEvent | 已有（迁移到 Contracts） |
| `EducationAndExams.StudyAbandoned` | `EducationAndExams` | DomainEvent | 已有（迁移到 Contracts） |
| `EducationAndExams.TutorSecured` | `EducationAndExams` | DomainEvent | 已有（迁移到 Contracts） |
| `EducationAndExams.ExamAttemptResolved` | `EducationAndExams` | DomainEvent | 新建 |
| `OrderAndBanditry.BanditThreatRaised` | `OrderAndBanditry` | DomainEvent | 已有（迁移到 Contracts） |
| `OrderAndBanditry.OutlawGroupFormed` | `OrderAndBanditry` | DomainEvent | 已有（迁移到 Contracts） |
| `OrderAndBanditry.SuppressionSucceeded` | `OrderAndBanditry` | DomainEvent | 已有（迁移到 Contracts） |
| `OrderAndBanditry.RouteUnsafeDueToBanditry` | `OrderAndBanditry` | DomainEvent | 已有（迁移到 Contracts） |
| `OrderAndBanditry.BlackRoutePressureRaised` | `OrderAndBanditry` | DomainEvent | 已有（迁移到 Contracts） |
| `OrderAndBanditry.DisorderSpike` | `OrderAndBanditry` | DomainEvent | 新建 |
| `OrderAndBanditry.BanditRecruitmentOpportunity` | `OrderAndBanditry` | DomainEvent | 新建 |
| `OrderAndBanditry.BanditHotspotActivated` | `OrderAndBanditry` | DomainEvent | 新建 |
| `OrderAndBanditry.DisorderLevelChanged` | `OrderAndBanditry` | DomainEvent | 新建（替代 `DisorderRiskAdjusted`） |
| `OrderAndBanditry.RouteInsecuritySpike` | `OrderAndBanditry` | DomainEvent | 新建 |
| `ConflictAndForce.ConflictResolved` | `ConflictAndForce` | DomainEvent | 已有（迁移到 Contracts） |
| `ConflictAndForce.CommanderWounded` | `ConflictAndForce` | DomainEvent | 已有（迁移到 Contracts） |
| `ConflictAndForce.ForceReadinessChanged` | `ConflictAndForce` | DomainEvent | 已有（迁移到 Contracts） |
| `ConflictAndForce.MilitiaMobilized` | `ConflictAndForce` | DomainEvent | 已有（迁移到 Contracts） |
| `ConflictAndForce.ForceReadinessDemand` | `ConflictAndForce` | DomainEvent | 新建 |
| `ConflictAndForce.ForceFatigueIncreased` | `ConflictAndForce` | DomainEvent | 新建 |
| `FamilyCore.ClanPrestigeAdjusted` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.MarriageAllianceArranged` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.BirthRegistered` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.BranchSeparationApproved` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.HeirSecurityWeakened` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.HeirAppointed` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.HeirSuccessionOccurred` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.CameOfAge` | `FamilyCore` | DomainEvent | 已有 |
| `FamilyCore.RetainerMobilized` | `FamilyCore` | DomainEvent | 新建 |
| `WarfareCampaign.CampaignMobilized` | `WarfareCampaign` | DomainEvent | 已有 |
| `WarfareCampaign.CampaignPressureRaised` | `WarfareCampaign` | DomainEvent | 已有 |
| `WarfareCampaign.CampaignSupplyStrained` | `WarfareCampaign` | DomainEvent | 已有 |
| `WarfareCampaign.CampaignAftermathRegistered` | `WarfareCampaign` | DomainEvent | 已有 |
| `WarfareCampaign.MobilizationWindowOpened` | `WarfareCampaign` | DomainEvent | 新建 |
| `WarfareCampaign.CampaignCommitted` | `WarfareCampaign` | DomainEvent | 新建 |
| `WarfareCampaign.RouteRepairNeed` | `WarfareCampaign` | DomainEvent | 新建 |
| `PublicLifeAndRumor.StreetTalkSurged` | `PublicLifeAndRumor` | DomainEvent | 已有 |
| `PublicLifeAndRumor.CountyGateCrowded` | `PublicLifeAndRumor` | DomainEvent | 已有 |
| `PublicLifeAndRumor.MarketBuzzRaised` | `PublicLifeAndRumor` | DomainEvent | 已有 |
| `PublicLifeAndRumor.RoadReportDelayed` | `PublicLifeAndRumor` | DomainEvent | 已有 |
| `PublicLifeAndRumor.PrefectureDispatchPressed` | `PublicLifeAndRumor` | DomainEvent | 已有 |
| `SocialMemoryAndRelations.GrudgeEscalated` | `SocialMemoryAndRelations` | DomainEvent | 已有（迁移到 Contracts） |
| `SocialMemoryAndRelations.GrudgeSoftened` | `SocialMemoryAndRelations` | DomainEvent | 已有（迁移到 Contracts） |
| `SocialMemoryAndRelations.FavorIncurred` | `SocialMemoryAndRelations` | DomainEvent | 已有（迁移到 Contracts） |
| `SocialMemoryAndRelations.ClanNarrativeUpdated` | `SocialMemoryAndRelations` | DomainEvent | 已有（迁移到 Contracts） |
| `PersonRegistry.PersonDeceased` | `PersonRegistry` | DomainEvent | 已有 |
| `PersonRegistry.FidelityRingChanged` | `PersonRegistry` | DomainEvent | 已有 |
| `PersonRegistry.DeathByViolence` | `PersonRegistry` | DomainEvent | 已有 |

---

## 9. 结论

经过逐条评审，原始 spec 中的 76 个名称经过合并、删除、复用后，实际需要的**跨模块 DomainEvent 常量**约为 **65 个**（含已有和新建）。其中：

- **6 个已有公开常量**，可直接使用
- **约 20 个已有模块内部字符串**，需迁移到 `Zongzu.Contracts`
- **约 39 个真正新建的事件常量**
- **4 个状态字段误标为事件**，应从事件列表删除
- **3 组事件建议合并为枚举**，减少事件名数量
- **8 个与现有名称相似**，建议复用现有名称并扩展 payload

**下一步**：待命名风格和合并决策确认后，即可开始批量创建 Contracts 常量类和迁移现有字符串。
