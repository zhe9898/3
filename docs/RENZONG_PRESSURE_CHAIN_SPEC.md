# RENZONG_PRESSURE_CHAIN_SPEC v0.3

北宋仁宗朝开局（1022–1063）活社会模拟的跨模块压力链规格。

本文档不是历史教科书，也不是职业树。它是把北宋地方社会的**结构性张力**翻译成模块拥有的状态变化、跨模块事件流、玩家可介入的命令窗口，以及忽略后的延迟后果。

当前已经落地的薄链拓扑、scope 边界、水位/edge 规则、receipt、测试证明和完整链条债务，见 `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`。本文档仍是完整设计目标；拓扑索引记录的是现在代码已经证明的薄切片。

## 设计约束（不可违反）

1. **不是职业树**。人在社会中的位置由压力推动，不是由 UI 选择决定。一个佃农之子可以读书、经商、当兵、落草、当胥吏，也可以一辈子种地——取决于压力链打开什么，不是玩家点击"选这个职业"。

2. **不是锁死历史事件**。仁宗朝提供初始压力校准（科举成熟且强可见但不是唯一晋身路径、商业高度货币化但普通家户仍被粮役租佃债人情信用实物储备共同束缚、无保甲、禁军/厢军边防压力、庆历改革在 horizon 上），但不强制范仲淹改革、不强制王安石出现、不强制任何历史事件按年份发生。历史大势作为压力载体存在，世界通过规则链决定它们是否落地、何时落地、以什么形式落地。

3. **玩家不是默认宗族族长，也不是默认皇帝**。玩家是一个 situated social actor，开局是中等地主户（三等户至二等户），有变化的影响圈。玩家可以是家户家长、宗族分支长老、地方商人、书院赞助人、胥吏后台——取决于压力链如何打开和收缩，不是固定标签。

---

## 事件名状态总表

### 命名规则（v0.3 起生效）

- **新事件一律前缀**：`Module.EventName`
- **旧事件不迁**：已有无前缀常量保持兼容，字符串值不变
- **原则**：压力是 state，爆点才是 event

### 状态图例

| 图例 | 含义 |
|---|---|
| **ExistingPublic** | 已在 `Zongzu.Contracts` 公开定义 |
| **ExistingInternal** | 模块内部字符串，v0.3 起迁入 `Zongzu.Contracts` |
| **NewPrefixed** | 新增，前缀风格，已入 `Zongzu.Contracts` |
| **ReusesExisting** | 不复用/不新建，直接引用已有常量 |
| **MergedToEnum** | 不单独成事件，合并为 payload enum |
| **Deleted** | 状态字段/投影，不作为 DomainEvent |

### 总表

| 事件名 | 归属模块 | 状态 | 说明 |
|---|---|---|---|
| `HouseholdDebtSpiked` | `PopulationAndHouseholds` | **ExistingPublic** | `PopulationEventNames` 已定义（无前缀，兼容） |
| `MigrationStarted` | `PopulationAndHouseholds` | **ExistingPublic** | `PopulationEventNames` 已定义（无前缀，兼容） |
| `LaborShortage` | `PopulationAndHouseholds` | **ExistingPublic** | `PopulationEventNames` 已定义（无前缀，兼容） |
| `ClanPrestigeAdjusted` | `FamilyCore` | **ExistingPublic** | `FamilyCoreEventNames` 已定义（无前缀，兼容） |
| `DeathByIllness` | `PersonRegistry` | **ExistingPublic** | `DeathCauseEventNames` 已定义（无前缀，兼容） |
| `StudyAbandoned` | `EducationAndExams` | **ExistingInternal** | 原模块内部字符串，已迁入 `EducationAndExamsEventNames` |
| `ExamPassed` | `EducationAndExams` | **ExistingInternal** | 原模块内部字符串，已迁入 `EducationAndExamsEventNames` |
| `ExamFailed` | `EducationAndExams` | **ExistingInternal** | 原模块内部字符串，已迁入 `EducationAndExamsEventNames` |
| `TutorSecured` | `EducationAndExams` | **ExistingInternal** | 原模块内部字符串，已迁入 `EducationAndExamsEventNames` |
| `OfficeGranted` | `OfficeAndCareer` | **ExistingInternal** | 原模块内部字符串，已迁入 `OfficeAndCareerEventNames`；复用替代 `OfficeGranted` |
| `OfficeLost` | `OfficeAndCareer` | **ExistingInternal** | 原模块内部字符串，已迁入 `OfficeAndCareerEventNames`；复用替代 `OfficeLost` |
| `OfficeTransfer` | `OfficeAndCareer` | **ExistingInternal** | 原模块内部字符串，已迁入 `OfficeAndCareerEventNames` |
| `AuthorityChanged` | `OfficeAndCareer` | **ExistingInternal** | 原模块内部字符串，已迁入 `OfficeAndCareerEventNames` |
| `BanditThreatRaised` | `OrderAndBanditry` | **ExistingInternal** | 原模块内部字符串，已迁入 `OrderAndBanditryEventNames` |
| `OutlawGroupFormed` | `OrderAndBanditry` | **ExistingInternal** | 原模块内部字符串，已迁入 `OrderAndBanditryEventNames` |
| `SuppressionSucceeded` | `OrderAndBanditry` | **ExistingInternal** | 原模块内部字符串，已迁入 `OrderAndBanditryEventNames` |
| `RouteUnsafeDueToBanditry` | `OrderAndBanditry` | **ExistingInternal** | 原模块内部字符串，已迁入 `OrderAndBanditryEventNames` |
| `BlackRoutePressureRaised` | `OrderAndBanditry` | **ExistingInternal** | 原模块内部字符串，已迁入 `OrderAndBanditryEventNames` |
| `TradeProspered` | `TradeAndIndustry` | **ExistingInternal** | 原模块内部字符串，已迁入 `TradeAndIndustryEventNames` |
| `TradeLossOccurred` | `TradeAndIndustry` | **ExistingInternal** | 原模块内部字符串，已迁入 `TradeAndIndustryEventNames` |
| `TradeDebtDefaulted` | `TradeAndIndustry` | **ExistingInternal** | 原模块内部字符串，已迁入 `TradeAndIndustryEventNames` |
| `RouteBusinessBlocked` | `TradeAndIndustry` | **ExistingInternal** | 原模块内部字符串，已迁入 `TradeAndIndustryEventNames` |
| `ConflictResolved` | `ConflictAndForce` | **ExistingInternal** | 原模块内部字符串，已迁入 `ConflictAndForceEventNames` |
| `CommanderWounded` | `ConflictAndForce` | **ExistingInternal** | 原模块内部字符串，已迁入 `ConflictAndForceEventNames` |
| `ForceReadinessChanged` | `ConflictAndForce` | **ExistingInternal** | 原模块内部字符串，已迁入 `ConflictAndForceEventNames` |
| `MilitiaMobilized` | `ConflictAndForce` | **ExistingInternal** | 原模块内部字符串，已迁入 `ConflictAndForceEventNames` |
| `GrudgeEscalated` | `SocialMemoryAndRelations` | **ExistingInternal** | 原模块内部字符串，已迁入 `SocialMemoryAndRelationsEventNames` |
| `GrudgeSoftened` | `SocialMemoryAndRelations` | **ExistingInternal** | 原模块内部字符串，已迁入 `SocialMemoryAndRelationsEventNames` |
| `FavorIncurred` | `SocialMemoryAndRelations` | **ExistingInternal** | 原模块内部字符串，已迁入 `SocialMemoryAndRelationsEventNames` |
| `ClanNarrativeUpdated` | `SocialMemoryAndRelations` | **ExistingInternal** | 原模块内部字符串，已迁入 `SocialMemoryAndRelationsEventNames` |
| `ImperialRhythmChanged` | `WorldSettlements` | **ExistingPublic** | `WorldSettlementsEventNames` 已定义；**复用替代 `WorldSettlements.ImperialRhythmChanged`** |
| `ReliefDelivered` | `WorldSettlements` | **ExistingPublic** | `WorldSettlementsEventNames` 已定义；**复用替代 `WorldSettlements.ReliefDelivered`** |
| `ReliefWithheld` | `WorldSettlements` | **ExistingPublic** | `WorldSettlementsEventNames` 已定义；**复用替代 `WorldSettlements.ReliefWithheld`** |
| `CampaignAftermathRegistered` | `WarfareCampaign` | **ExistingPublic** | `WarfareCampaignEventNames` 已定义；**复用替代 `CampaignAftermath`** |
| `EducationAndExams.ExamAttemptResolved` | `EducationAndExams` | **NewPrefixed** | 新建 |
| `WorldSettlements.ExamSeasonOpened` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.GrainPriceSpike` | `TradeAndIndustry` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.GrainPriceSpikeRisk` | `TradeAndIndustry` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.MarketPanicRisk` | `TradeAndIndustry` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.MarketDiversion` | `TradeAndIndustry` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.OfficialPurchasingPressure` | `TradeAndIndustry` | **NewPrefixed** | 新建 |
| `TradeAndIndustry.GrainRouteBlocked` | `TradeAndIndustry` | **NewPrefixed** | 新建（P5+） |
| `TradeAndIndustry.GrainRouteReopened` | `TradeAndIndustry` | **NewPrefixed** | 新建（P5+） |
| `WorldSettlements.TaxSeasonOpened` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.ExamSeasonOpened` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.CourtMourning` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.FrontierSupplyDemand` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.FrontierStrainEscalated` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.DisasterDeclared` | `WorldSettlements` | **NewPrefixed** | 新建 |
| `WorldSettlements.CourtAgendaPressureAccumulated` | `WorldSettlements` | **NewPrefixed** | 新建（P5+） |
| `WorldSettlements.RegimeLegitimacyShifted` | `WorldSettlements` | **NewPrefixed** | 新建（P5+） |
| `WorldSettlements.GrainRouteControlDisputed` | `WorldSettlements` | **NewPrefixed** | 新建（P5+） |
| `WorldSettlements.RitualClaimStaged` | `WorldSettlements` | **NewPrefixed** | 新建（P5+） |
| `OfficeAndCareer.YamenOverloaded` | `OfficeAndCareer` | **NewPrefixed** | 新建；payload 带 `taskKind` |
| `OfficeAndCareer.AppointmentQueuePressure` | `OfficeAndCareer` | **NewPrefixed** | 新建 |
| `OfficeAndCareer.AmnestyApplied` | `OfficeAndCareer` | **NewPrefixed** | 新建 |
| `OfficeAndCareer.OfficialSupplyRequisition` | `OfficeAndCareer` | **NewPrefixed** | 新建 |
| `OfficeAndCareer.MemorialAttackReceived` | `OfficeAndCareer` | **NewPrefixed** | 新建 |
| `OfficeAndCareer.ClerkCaptureDeepened` | `OfficeAndCareer` | **NewPrefixed** | 新建 |
| `OfficeAndCareer.PolicyImplemented` | `OfficeAndCareer` | **NewPrefixed** | 新建；替代 `RapidImplementation` / `ImplementationDrag` / `ImplementationCaptured` / `ImplementationDistorted`，payload 带 `outcome = Rapid | Dragged | Captured | PaperCompliance` |
| `OfficeAndCareer.PolicyWindowOpened` | `OfficeAndCareer` | **NewPrefixed** | 新建（P5+） |
| `OfficeAndCareer.AppointmentSlateProposed` | `OfficeAndCareer` | **NewPrefixed** | 新建（P5+） |
| `OfficeAndCareer.PolicyWordingDrafted` | `OfficeAndCareer` | **NewPrefixed** | 新建（P5+） |
| `OfficeAndCareer.DispatchArrived` | `OfficeAndCareer` | **NewPrefixed** | 新建（P5+） |
| `OfficeAndCareer.EvaluationCompleted` | `OfficeAndCareer` | **NewPrefixed** | 新建；替代 `EvaluationCycleTriggered` |
| `OrderAndBanditry.DisorderSpike` | `OrderAndBanditry` | **NewPrefixed** | 新建 |
| `OrderAndBanditry.BanditRecruitmentOpportunity` | `OrderAndBanditry` | **NewPrefixed** | 新建 |
| `OrderAndBanditry.BanditHotspotActivated` | `OrderAndBanditry` | **NewPrefixed** | 新建 |
| `OrderAndBanditry.DisorderLevelChanged` | `OrderAndBanditry` | **NewPrefixed** | 新建；替代 `DisorderRiskAdjusted` |
| `OrderAndBanditry.RouteInsecuritySpike` | `OrderAndBanditry` | **NewPrefixed** | 新建 |
| `PopulationAndHouseholds.TenantFlightTriggered` | `PopulationAndHouseholds` | **NewPrefixed** | 新建；替代 `PopulationAndHouseholds.TenantFlightTriggered` |
| `PopulationAndHouseholds.HouseholdSubsistencePressureChanged` | `PopulationAndHouseholds` | **NewPrefixed** | 新建；合并 `HouseholdSubsistenceStrain` + `HouseholdSubsistenceCrisis`，payload 带 `band` enum |
| `PopulationAndHouseholds.HouseholdLaborReabsorbed` | `PopulationAndHouseholds` | **NewPrefixed** | 新建 |
| `PopulationAndHouseholds.LaborDraftPressure` | `PopulationAndHouseholds` | **NewPrefixed** | 新建 |
| `PopulationAndHouseholds.HouseholdBurdenIncreased` | `PopulationAndHouseholds` | **NewPrefixed** | 新建 |
| `PopulationAndHouseholds.HouseholdWarLoss` | `PopulationAndHouseholds` | **NewPrefixed** | 新建 |
| `PopulationAndHouseholds.HouseholdComplianceShifted` | `PopulationAndHouseholds` | **NewPrefixed** | 新建；合并 `HouseholdResistance` + `HouseholdCompliance`，payload 带 `direction` enum |
| `PopulationAndHouseholds.HouseholdRegimeTransition` | `PopulationAndHouseholds` | **NewPrefixed** | 新建（P5+） |
| `FamilyCore.RetainerMobilized` | `FamilyCore` | **NewPrefixed** | 新建 |
| `ConflictAndForce.ForceReadinessDemand` | `ConflictAndForce` | **NewPrefixed** | 新建 |
| `ConflictAndForce.ForceFatigueIncreased` | `ConflictAndForce` | **NewPrefixed** | 新建 |
| `WarfareCampaign.MobilizationWindowOpened` | `WarfareCampaign` | **NewPrefixed** | 新建 |
| `WarfareCampaign.CampaignCommitted` | `WarfareCampaign` | **NewPrefixed** | 新建 |
| `WarfareCampaign.RouteRepairNeed` | `WarfareCampaign` | **NewPrefixed** | 新建 |
| `EvaluationPressure` | `OfficeAndCareer` | **Deleted** | 状态字段，不是事件。下游通过 Query 读取 |
| `ClerkCaptureRisk` | `OfficeAndCareer` | **Deleted** | 状态字段/风险投影，不是事件。实现后发 `OfficeAndCareer.ClerkCaptureDeepened` |
| `WaitingListFrustration` | `OfficeAndCareer` | **Deleted** | 状态字段。`PublicLifeAndRumor` 通过 Query 投影 |
| `OfficialDefectionRisk` | `OfficeAndCareer` | **Deleted** | 状态字段/风险投影，不是事件。实现后发 `OfficeAndCareer.OfficeDefected` |
| `HouseholdPetitionFrustration` | — | **Deleted** | `PublicLifeAndRumor` 投影，不作为 DomainEvent |
| `PetitionBacklogExceeded` | `OfficeAndCareer` | **MergedToEnum** | 合并进 `OfficeAndCareer.YamenOverloaded`，payload 带 `taskKind = petition-backlog` |
| `OfficeGranted` | `OfficeAndCareer` | **ReusesExisting** | 合并进 `OfficeGranted`，payload 带 `postId, location, rank` |
| `OfficeLost` | `OfficeAndCareer` | **ReusesExisting** | 合并进 `OfficeLost`，payload 带 `reason: transfer` |
| `WorldSettlements.ImperialRhythmChanged` | `WorldSettlements` | **ReusesExisting** | 复用 `ImperialRhythmChanged`，payload 带 `bandKind` |
| `WorldSettlements.ReliefDelivered` | `WorldSettlements` | **ReusesExisting** | 复用 `ReliefDelivered` |
| `WorldSettlements.ReliefWithheld` | `WorldSettlements` | **ReusesExisting** | 复用 `ReliefWithheld` |
| `CampaignAftermath` | `WarfareCampaign` | **ReusesExisting** | 复用 `CampaignAftermathRegistered` |
| `OfficeAndCareer.PolicyImplemented` | `OfficeAndCareer` | **MergedToEnum** | 合并 `RapidImplementation` / `ImplementationDrag` / `ImplementationCaptured` / `ImplementationDistorted`；payload `outcome = Rapid | Dragged | Captured | PaperCompliance` |
| `PopulationAndHouseholds.HouseholdSubsistencePressureChanged` | `PopulationAndHouseholds` | **MergedToEnum** | 合并进 `HouseholdSubsistencePressureChanged`，payload `band = Strain` |
| `PopulationAndHouseholds.HouseholdSubsistencePressureChanged` | `PopulationAndHouseholds` | **MergedToEnum** | 合并进 `HouseholdSubsistencePressureChanged`，payload `band = Crisis` |
| `HouseholdResistance` | `PopulationAndHouseholds` | **MergedToEnum** | 合并进 `PopulationAndHouseholds.HouseholdComplianceShifted`，payload `direction = Resistance` |
| `HouseholdCompliance` | `PopulationAndHouseholds` | **MergedToEnum** | 合并进 `PopulationAndHouseholds.HouseholdComplianceShifted`，payload `direction = Compliance` |
| `DisorderRiskAdjusted` | `OrderAndBanditry` | **MergedToEnum** | 改为 `DisorderLevelChanged`，定期 summary 事件 |
| `EvaluationCycleTriggered` | `OfficeAndCareer` | **Deleted** | 内部 cadence 信号，不发事件。完成后发 `EvaluationCompleted` |
| `PublicLifeAndRumor.PublicLegitimacyShifted` | `PublicLifeAndRumor` | **NewPrefixed** | 保留为事件：`PublicLifeAndRumor.PublicLegitimacyShifted`（但 P5+） |


---

## 核心压力链

每条链按以下格式定义：
- **Trigger**：什么模块的什么状态变化触发此链
- **Event Flow**：模块间事件流（事件名、发送方、接收方、关键 payload）
- **Player Window**：玩家在哪里用什么命令介入
- **Ignore Consequence**：玩家什么都不做，下月/下季度会怎样
- **Backlash / Memory**：此链产生的长期残留（恩怨、派系标签、合法性伤痕）
- **Module Boundary Check**：谁拥有什么状态，谁不能越界写

---

### 链1：税役-家户-衙门-公共生活

> **实现状态：真实 scheduler 薄切片（M2-lite）+ 链一第一层规则加厚。已落 `TaxSeasonOpened -> HouseholdDebtSpiked -> OfficeAndCareer.YamenOverloaded -> PublicLife heat`，并有真实 `MonthlyScheduler` drain 测试；`PopulationAndHouseholds` 现在用现有多维家户画像（生计、土地、粮储、劳力、依附人口、债压、民困）计算税季债务增量并写入结构化 metadata。完整版的正式户等/税种/额度公式、客户租压、佃户逃散、税季现金挤压到市场、SocialMemory 年度残留、精确 settlement-scoped 衙门 payload 仍未实现。**

**Trigger**：`WorldSettlements` 季节推进进入 `TaxSeason` 或 `CorveeWindow`。

**Event Flow**：

```
WorldSettlements (monthly, seasonal band)
  └── WorldSettlements.TaxSeasonOpened { settlementId, taxKind (二税/身丁钱/杂变), quotaBase, seasonalAdjustment }
      ├──→ PopulationAndHouseholds
      │     计算每个 zhuhu 的 taxablePressure = quotaBase * householdGrade * seasonalAdjustment
      │     计算每个 kehu 的 rentPressure (间接，通过 landlord 传递)
      │     更新 household.livelihoodPressure
      │     若 pressure > threshold:
      │       发出 HouseholdDebtSpiked { householdId, causeKey: "tax-season", pressureDelta }
      │       或发出 PopulationAndHouseholds.TenantFlightTriggered { householdId, landlordClanId }
      │
      ├──→ TradeAndIndustry
      │     税役季节 cash demand 上升 → 市场 cashNeed 增加
      │     若 granarySecurity 低:
      │       发出 TradeAndIndustry.GrainPriceSpikeRisk { settlementId, causeKey: "tax-season-cash-squeeze" }
      │
      └──→ OfficeAndCareer
            tax collection 进入 yamen workload
            更新 petitionBacklog（欠税纠纷、减免请求）
            若 backlog > capacity:
              发出 OfficeAndCareer.YamenOverloaded { settlementId, taskKind: "tax-collection", delayMonths }

PopulationAndHouseholds (xun pulse)
  └── HouseholdDebtSpiked
      ├──→ FamilyCore（通过 query，非事件直接写）
      │     若 household 属于玩家宗族:
      │       clan.SupportReserve 下降压力
      │       可能触发 branch-favor 请求
      │
      └──→ SocialMemoryAndRelations
            记忆条目: DebtOfHonorCreated { source: "tax-pressure", targetHouseholdId, weight }

TradeAndIndustry (monthly)
  └── TradeAndIndustry.GrainPriceSpikeRisk
      └──→ 若风险实现:
            发出 TradeDebtDefaulted { traderId, causeKey: "grain-price-spike" }
            或发出 CaravanDelayed { routeId, causeKey: "cash-shortage" }

OfficeAndCareer (monthly)
  └── OfficeAndCareer.YamenOverloaded
      ├──→ 若 office 有 jurisdiction:
      │     更新 evaluationPressure（状态字段，不发出跨模块事件）
      │
      └──→ PublicLifeAndRumor（通过 query 读取 yamen 状态）
            更新 county-gate heat: "衙门口挤满了欠税和请减的人"
            更新 streetTalk: "今年的二税又提前了"
```

**Player Window**：
- `Endure` — 承受税役压力，下月债务累积
- `PetitionYamen` — 请求减免或延期（消耗 publicFace，可能成功/驳回/悬案）
- `GuaranteeDebt` — 为宗族成员或信任对象担保债务（消耗 credit，创造未来义务）
- `FundLocalWatch` — 若县衙因税役征发导致治安空虚，可资助巡丁（消耗 cash，提升局部 order）

**Ignore Consequence**：
- 下月：debt 累积，tenant flight risk 上升，若 granary 低则粮价可能上涨
- 下季度：若 debt 持续，migration 压力出现，market cashNeed 继续紧缩
- 下年：若 yamen backlog 未解，evaluation pressure 可能影响本地官员任命/调任

**Backlash / Memory**：
- `SocialMemoryAndRelations` 记录：谁担保了、谁拒绝了、谁在衙门口被驳回了
- `PublicLifeAndRumor` 残留："某年某县税重" 成为年度叙事标签，影响后续 petition 成功率
- 若玩家宗族成员因欠税被衙役追索，`FamilyCore` branch tension 上升

**Module Boundary Check**：
- `WorldSettlements` 拥有季节带和税役窗口，不拥有家户债务
- `PopulationAndHouseholds` 拥有家户生计压力和债务，不直接写 `TradeAndIndustry` 价格
- `TradeAndIndustry` 拥有市场价格和贸易债务，通过事件通知下游
- `OfficeAndCareer` 拥有衙门 workload 和官员 evaluation，不直接写家户状态
- `PublicLifeAndRumor` 只读查询其他模块状态，不拥有权威状态

---

### 链2：粮价-市场-家户-贸易

> **实现状态：真实 scheduler 薄切片（M2-lite）+ 链二第一层规则加厚。已落 `SeasonPhaseAdvanced(Harvest) -> TradeAndIndustry.GrainPriceSpike -> PopulationAndHouseholds.HouseholdSubsistencePressureChanged`，并有真实 `MonthlyScheduler` drain 测试和 off-scope 聚落负例。`TradeAndIndustry` 现在随 `GrainPriceSpike` 写入粮价/供需 metadata；`PopulationAndHouseholds` 用现有多维家户画像（生计、粮储、劳力、依附人口、债压、民困、迁徙风险）计算生计压力增量并写入结构化 metadata。完整版的 `HarvestPressureChanged/yieldRatio`、灾荒输入、granarySecurity/routeRisk、迁徙/病亡、OrderAndBanditry 路险、SocialMemory 与 PublicLife 长期饥荒叙事仍未实现。**

**Trigger**：`WorldSettlements` 秋收结算（`HarvestPressureChanged`）或灾荒判定（`Drought`/`Flood`）。

**Event Flow**：

```
WorldSettlements (seasonal)
  └── HarvestPressureChanged { settlementId, yieldRatio (0.0–1.5+), causeKey }
      └──→ TradeAndIndustry
            更新 MarketGoodsEntry[Grain].supply = baseSupply * yieldRatio
            重新计算 currentPrice = basePrice * f(supply/demand, routeRisk, granarySecurity)
            若 priceDelta > threshold:
              发出 TradeAndIndustry.GrainPriceSpike { settlementId, oldPrice, newPrice, priceDelta, supply, demand, causeKey }

TradeAndIndustry (monthly)
  └── TradeAndIndustry.GrainPriceSpike
      ├──→ PopulationAndHouseholds
      │     ✅ FIRST THICKENING DONE：每个 household 按 household-owned profile 重新计算 subsistence pressure
      │     画像维度：price pressure、grain-store buffer、livelihood market dependency、
      │              labor/dependency load、debt/distress fragility、interaction terms
      │     若 Distress 跨过 60：
      │       发出 PopulationAndHouseholds.HouseholdSubsistencePressureChanged { householdId, delta, profile metadata }
      │     若 strainLevel >= Critical:
      │       发出 MigrationStarted { householdId, direction: "toward_market_town" }
      │
      ├──→ FamilyCore（通过 query）
      │     若 strain 涉及宗族成员:
      │       clan.CareLoad 上升
      │       可能触发 SupportNewbornCare 或拨粮请求
      │
      └──→ OrderAndBanditry（通过 query 读取 route 状态）
            粮道吃紧 → 护送需求上升
            若 route safety 低:
              发出 OrderAndBanditry.RouteInsecuritySpike { routeId, causeKey: "grain-scarcity" }

PopulationAndHouseholds (xun pulse)
  └── PopulationAndHouseholds.HouseholdSubsistencePressureChanged
      └──→ 持续 strain:
            更新 healthResilience（慢性下降）
            若 healthResilience < threshold + random:
              发出 DeathByIllness { personId, causeKey: "malnutrition-related" }
              （PersonRegistry 合并为 PersonDeceased）

OrderAndBanditry (monthly)
  └── OrderAndBanditry.RouteInsecuritySpike
      └──→ 若 insecurity 持续:
            更新 settlementDisorder
            可能发出 OrderAndBanditry.BanditHotspotActivated { settlementId }
```

**Player Window**：
- `EscortRoute` — 花钱雇人护送粮道（消耗 cash + labor，风险：折损报告）
- `InvestInEstate` — 投资农田/店铺缓冲粮价波动（消耗 cash，长期回报）
- `GuaranteeDebt` — 为受粮价冲击的农户担保（消耗 credit）
- `PetitionYamen` — 请求开仓赈济（依赖 publicFace 和衙门 capacity）

**Ignore Consequence**：
- 下月：subsistence strain 累积，healthResilience 下降，migration 出现
- 下季度：若 strain 持续，death risk 上升，tenant flight 触发 landlord 收入下降
- 下年：若 bandit hotspot 激活，route 长期不安全，市场 cashNeed 持续紧缩

**Backlash / Memory**：
- 饥荒年的死亡在 `SocialMemoryAndRelations` 留下 `Favor` 或 `Grudge`（谁赈了、谁没赈）
- `PublicLifeAndRumor`："某年大旱，某族长见死不救" 成为长期 stigma
- 若玩家 `EscortRoute` 失败（折损），`ConflictAndForce` 记录 casualty，影响宗族 prestige

**Module Boundary Check**：
- `WorldSettlements` 拥有收成/灾荒，不拥有价格
- `TradeAndIndustry` 拥有价格，通过事件传播
- `PopulationAndHouseholds` 拥有家户健康和生存，不直接写市场价格
- `OrderAndBanditry` 拥有 route security，读取 trade 的 route 状态但不写价格

---

### 链3：科举-教育-家户-官员-公共生活

> **实现状态：真实 scheduler 薄切片（M2-lite）+ 链三第一层规则加厚。已落 `ExamPassed -> FamilyCore.ClanPrestigeAdjusted`，并有真实 `MonthlyScheduler` drain 测试。`EducationAndExams` 现在随 `ExamPassed` 写入考阶、分数、学业、塾望、塾师、宗房支持、恩义/羞压、心气劳迫 metadata；`FamilyCore` 用 credential metadata + 本族 person/clan 状态计算门望与婚议价值增量并写入结构化 metadata。完整版的 `ExamAttemptResolved`、OfficeAndCareer waiting list、SocialMemory Favor/Shame、PublicLife 放榜/士论投影、失败与停学旁路仍未实现。**

**Trigger**：`WorldSettlements` 进入 `ExamSeason`，或 `EducationAndExams` 检测到 eligible aspirant。

**Event Flow**：

```
WorldSettlements (seasonal)
  └── WorldSettlements.ExamSeasonOpened { settlementId, examTier, quota }
      └──→ EducationAndExams
            遍历 eligible aspirants（studyProgress + age + academyAccess）
            计算 passProbability = f(studyProgress, tutorQuality, stress, academyPrestige, quota, competition)
            对每个 aspirant 发出 EducationAndExams.ExamAttemptResolved { personId, examTier, result, passProbability }
            ⏳ 当前未消费 ExamSeasonOpened；EducationAndExams 自行检查考试窗口 (month 3/9)
            ⏳ 当前未发出 ExamAttemptResolved；直接发 ExamPassed / ExamFailed / StudyAbandoned

EducationAndExams (monthly)
  └── EducationAndExams.ExamAttemptResolved { result: Pass }
      ├──→ FamilyCore（通过事件，非直接写）✅ THIN-SLICE DONE
      │     ✅ FIRST THICKENING DONE：按 credential-prestige profile 更新 clan.Prestige / MarriageAllianceValue
      │     画像维度：examTier、score、academyPrestige、stress、
      │              clan standing、heir/branch role、adult unmarried status、marriage pressure
      │     发出 ClanPrestigeAdjusted { clanId, causeKey: "exam-pass", delta, profile metadata }
      │
      ├──→ OfficeAndCareer（通过事件）⏳ NOT IMPLEMENTED
      │     创建 WaitingListEntry { personId, qualificationTier: examTier, waitingMonths: 0 }
      │     若 queue 长:
      │       发出 OfficeAndCareer.AppointmentQueuePressure { settlementId, tier: examTier, queueLength }
      │
      └──→ SocialMemoryAndRelations ⏳ NOT IMPLEMENTED
            记忆: FavorIncurred（赞助人-考生关系加深）
            若考生来自平民家户: 记录 "寒门出贵子" narrative

EducationAndExams (monthly)
  └── EducationAndExams.ExamAttemptResolved { result: Fail }
      ├──→ FamilyCore（通过事件）⏳ NOT IMPLEMENTED
      │     若 study investment 大:
      │       发出 PopulationAndHouseholds.HouseholdLaborReabsorbed { personId, householdId }
      │       clan.SupportReserve 可能因沉没成本而紧张
      │
      ├──→ PopulationAndHouseholds（通过事件）⏳ NOT IMPLEMENTED
      │     更新 person.activity = Idle / Farming / Laboring
      │     若连续失败 >= 3:
      │       发出 StudyAbandoned { personId, fallbackPath }
      │
      └──→ SocialMemoryAndRelations ⏳ NOT IMPLEMENTED
            记忆: Shame 或 Resignation（取决于家庭期望和社会压力）

OfficeAndCareer (monthly)
  └── OfficeAndCareer.AppointmentQueuePressure ⏳ NOT IMPLEMENTED
      └──→ 若 patronageSupport 足够 + vacancy 出现:
            发出 OfficeGranted { personId, postId, location, rank }
            否则:
            更新 waitingMonths（状态字段，PublicLifeAndRumor 通过 Query 投影）

PublicLifeAndRumor (monthly, 通过 query)
  └── 读取 exam results + appointment news ⏳ NOT IMPLEMENTED
      更新 notice wall: 放榜
      更新 street talk: "今年县学又中了几个"
      若 waiting list 长: 更新 rumor: "候补的人排到了衙门口"
```

**Player Window**：
- `FundStudy` — 资助宗族成员读书（消耗 cash，提升 studyProgress，风险：学业不继/束脩已纳）
- `RecommendSomeone` — 向官员推荐有才能的人（消耗 reputation，创造 future obligation）
- `PetitionYamen` — 请求改善县学条件（publicFace 消耗，长期影响 academyAccess）

**Ignore Consequence**：
- 下月：失败考生回归家户劳动力，成功考生进入候补队列
- 下季度：若候补队列长，frustration 累积，可能影响 faction heat 或 public legitimacy
- 下年：若无人资助学业，宗族科举竞争力下降，长期 prestige 受影响

**Backlash / Memory**：
- 连续资助但连续失败：`FamilyCore` 内部 branch tension（"把钱都砸在读书上了"）
- 成功任命后：赞助人与被赞助人之间的 `Favor` 深度绑定，未来可能要求回报
- `PublicLifeAndRumor`：县学的"中举率"成为年度公共叙事，影响后续投资意愿

**Module Boundary Check**：
- `EducationAndExams` 拥有考试结果，不直接写家族 prestige 或官职
- `FamilyCore` 收到事件后更新自己的 prestige
- `OfficeAndCareer` 收到事件后更新自己的 waiting list
- `PublicLifeAndRumor` 只读查询，不拥有考试状态

---

### 链4：皇权-任命-地方-公共生活

> **实现状态：薄切片 + 链4第一层规则加厚（M2-lite）。已落 `ImperialRhythmChanged -> OfficeAndCareer.AmnestyApplied -> OrderAndBanditry.DisorderSpike`；`AmnestyApplied` 已携带衙门执行 metadata，`OrderAndBanditry` 已用本地治安土壤 + 衙门执行上下文计算失序 delta。完整版的任命暂停、国丧、边防军需、公共生活诏令投影仍未实现。**

**Trigger**：`WorldSettlements` 的 `ImperialBand` 更新（大赦、国丧、储位摇动、边防急报）。

**Event Flow**：

```
WorldSettlements (seasonal / month)
  └── WorldSettlements.ImperialRhythmChanged { bandKind, severity, durationMonths }
      ├──→ OfficeAndCareer
      │     若 AmnestyWave:
      │       释放部分在押人员，更新 yamen docket
      │       发出 OfficeAndCareer.AmnestyApplied { settlementId, amnestyWave, authorityTier, jurisdictionLeverage, clerkDependence, petitionBacklog, administrativeTaskLoad }
      │     若 MourningInterruption:
      │       暂停 appointment / evaluation / marriage / public festivities
      │       发出 WorldSettlements.CourtMourning { settlementId, duration, affectedProcesses }
      │     若 FrontierEmergency:
      │       更新 military supply demand，影响 corvée / tax quota
      │       发出 WorldSettlements.FrontierSupplyDemand { settlementId, grainQuota, laborQuota }
      │
      ├──→ PopulationAndHouseholds
      │     若 WorldSettlements.FrontierSupplyDemand:
      │       重新计算 corvéeLoad
      │       若 laborQuota > available:
      │         发出 PopulationAndHouseholds.LaborDraftPressure { settlementId, shortage }
      │
      └──→ PublicLifeAndRumor
            更新 notice wall: 诏书/敕牒内容摘要
            更新 street talk: "皇上大赦了" / "边报又紧了"
            若 MourningInterruption:
              更新 temple / market activity 为 quiet

OfficeAndCareer (monthly)
  └── OfficeAndCareer.AmnestyApplied
      └──→ OrderAndBanditry（通过事件）
            OrderAndBanditry 处理实际治安后果：释放人员中惯犯的再犯风险、失序压力上升
            ✅ FIRST THICKENING DONE：不再固定 +10；读取 AmnestyApplied metadata 与本地 order state，
               按 releasePressure / docketPressure / clerkHandlingPressure / localDisorderSoil / authorityBuffer / suppressionBuffer 计算 delta；
               若本地官威和镇压缓冲足够，delta 可为 0，不强制每次大赦都变成失序
            ✅ THIN-SLICE DONE：跨 50 阈值时复用 OrderAndBanditry.DisorderSpike { settlementId }
            ⏳ FULL CHAIN TODO：定期 summary 事件仍应使用 OrderAndBanditry.DisorderLevelChanged { settlementId, oldBand, newBand }
      注意：OfficeAndCareer 只处理文书和命令（发布赦令、更新案牍），不直接处理治安执行

OfficeAndCareer (monthly)
  └── WorldSettlements.FrontierSupplyDemand
      └──→ TradeAndIndustry（通过 query）
            军需采购文书/命令发出
            若 local supply 不足:
              发出 OfficialPurchasingPressure { settlementId, goods: grain, urgency }
      注意：OfficeAndCareer 处理采购命令和文书，实际物资调配和路线风险由 TradeAndIndustry 和 OrderAndBanditry 处理

PopulationAndHouseholds (monthly)
  └── PopulationAndHouseholds.LaborDraftPressure
      └──→ 若 pressure 持续:
            household.laborerCount 下降
            发出 LaborShortage { settlementId, sector: "agriculture" }
            若 draft 强制且家户抗拒:
              发出 PopulationAndHouseholds.HouseholdComplianceShifted { householdId, causeKey: "labor-draft", direction: "Resistance" }

PublicLifeAndRumor (monthly, 通过 query)
  └── 读取所有 imperial band 变化
      更新 public legitimacy:
      - 若 amnesty 慷慨: legitimacy + modest
      - 若 frontier demand 过重且无 relief: legitimacy -
      - 若 mourning 过长影响生计: public frustration +
```

**Player Window**：
- `Endure` — 承受征兵/征粮压力
- `PetitionYamen` — 请求减免军需征发（依赖 publicFace，成功率受 imperial band 影响）
- `EscortRoadReport` — 若边报导致粮道紧张，可护送军粮（消耗 labor，有风险，但有报酬）
- `FundLocalWatch` — 若大赦后治安恶化，资助巡丁

**Ignore Consequence**：
- 下月：corvée / tax 压力直接落地，家户劳动力减少
- 下季度：若 frontier emergency 持续，trade route 转向军需，民用市场紧张
- 下年：若 legitimacy 持续下降，public life 中出现"天命"议论，为后期不稳定埋下种子

**Backlash / Memory**：
- 军需征发中的强制抓丁：`SocialMemoryAndRelations` 记录 Fear / Grudge
- 大赦释放的重犯若再犯：`PublicLifeAndRumor` 记录 "朝廷赦错了人"
- 长期 mourning 影响婚嫁和市场：`FamilyCore` 婚议延迟，`TradeAndIndustry` 市场冷淡

**Module Boundary Check**：
- `WorldSettlements` 拥有 `ImperialBand`，不直接写家户或衙门状态
- `OfficeAndCareer` 拥有衙门对皇权的响应，不直接写家户
- `PopulationAndHouseholds` 拥有劳动力和征兵响应
- `PublicLifeAndRumor` 只读查询，传播舆论但不改变权威状态

---

### 链5：边防-军需-家户-市场-衙门

> **实现状态：薄切片（M2-lite）+ 第一层规则加厚。已落 `WorldSettlements.FrontierStrainEscalated -> OfficeAndCareer.OfficialSupplyRequisition -> PopulationAndHouseholds.HouseholdBurdenIncreased` 的真实 scheduler 链；`OfficeAndCareer` 已把边防压力转成辖区侧军需执行 profile，`PopulationAndHouseholds` 已按家户生计/储备/劳力/债压/迁徙风险计算负担 profile。完整版的边防 sector、WarfareCampaign mobilization、ConflictAndForce readiness、TradeAndIndustry market diversion、正式配额/现金公式、公共生活军需投影和长期记忆残留仍未实现。**

**Trigger**：`WorldSettlements` 长期 `FrontierEmergency` + `WarfareCampaign` 若启用。

**Event Flow**：

```
WorldSettlements (seasonal)
  └── WorldSettlements.FrontierStrainEscalated { borderSector, threatLevel, durationForecast }
      ├──→ WarfareCampaign（若启用）
      │     更新 mobilization signals
      │     发出 WarfareCampaign.MobilizationWindowOpened { sector, grainNeed, laborNeed, forceNeed }
      │
      ├──→ OfficeAndCareer
      │     更新 military supply quota / docket pressure / clerk distortion risk
      │     发出 OfficeAndCareer.OfficialSupplyRequisition { settlementId, supplyPressure, quotaPressure, docketPressure, clerkDistortionPressure, authorityBuffer }
      │
      └──→ ConflictAndForce（若启用）
            更新 readiness demand
            发出 ConflictAndForce.ForceReadinessDemand { settlementId, requiredGuardCount }

WarfareCampaign（若启用）
  └── WarfareCampaign.MobilizationWindowOpened
      └──→ 若 campaign 启动:
            发出 WarfareCampaign.CampaignCommitted { campaignId, sector, committedForces }
            后续发出 WarfareCampaign.CampaignAftermathRegistered { campaignId, casualties, merits, blames }

OfficeAndCareer (monthly)
  └── OfficeAndCareer.OfficialSupplyRequisition
      ├──→ PopulationAndHouseholds
      │     强制征粮/征役
      │     当前薄切片 + 加厚：按 livelihood exposure、grain/tool/shelter buffer、labor/dependent load、debt/distress fragility、migration pressure 计算家户负担
      │     发出 HouseholdBurdenIncreased { householdId, kind: "military-supply", distressDelta, debtDelta, laborDrop, migrationDelta, profileMetadata }
      │
      ├──→ TradeAndIndustry
      │     军需采购优先
      │     若 market supply 转向军需:
      │       发出 TradeAndIndustry.MarketDiversion { settlementId, goods: grain, diversionRate }
      │
      └──→ PublicLifeAndRumor
            更新 street talk: "边报又紧了，衙门口在征粮"

ConflictAndForce（若启用）
  └── ConflictAndForce.ForceReadinessDemand
      └──→ FamilyCore（通过 query 读取 retainer 状态）
            若 clan 有 retainers:
              可选择 mobilize retainers for escort / local guard
              发出 FamilyCore.RetainerMobilized { clanId, count, purpose }

WarfareCampaign（若启用，monthly）
  └── WarfareCampaign.CampaignAftermathRegistered
      ├──→ ConflictAndForce
      │     更新 fatigue / escort-strain
      │     发出 ConflictAndForce.ForceFatigueIncreased { settlementId, duration }
      │
      ├──→ PopulationAndHouseholds
      │     阵亡者家庭进入 mourning / debt
      │     发出 PopulationAndHouseholds.HouseholdWarLoss { householdId, deceasedCount }
      │
      ├──→ TradeAndIndustry
      │     route repair cost
      │     发出 WarfareCampaign.RouteRepairNeed { routeId, costEstimate }
      │
      └──→ SocialMemoryAndRelations
            记录 honors / blames
            若 blames > merits: 记录 Grudge（"朝廷打输了，还征了我们的粮"）
```

**Thin-slice constraints (implemented)**：
- 当前 `FrontierStrainEscalated` 使用 `EntityKey = settlementId`，不是 `frontier` 这类全局 token；`OfficeAndCareer` 只对匹配 jurisdiction 发 `OfficialSupplyRequisition`。
- `WorldSettlements.LastDeclaredFrontierStrainBand` 是 schema `8` 的模块内水位，边防压力持续高位时不重复发同一档升级事件；未来若改成周期军需，应另起 `FrontierSupplyDemand` 或配额 cadence，不复用升级事件。
- `OfficeAndCareer.OfficialSupplyRequisition` 当前携带第一层军需执行 profile：`FrontierPressure` / `Severity` 进入辖区侧 `SupplyPressure`、`QuotaPressure`、`DocketPressure`、`ClerkDistortionPressure`、`AuthorityBuffer`，并只在本模块内留下 office task / backlog / authority pressure，不直接改家户。
- `PopulationAndHouseholds.HouseholdBurdenIncreased` 只作为家户负担跨阈值 receipt；当前携带第一层 household burden profile metadata，证明军需压力会被家户生计、储备、劳力、债压、迁徙风险重新解释，但不代表完整军需征发公式完成。

**Player Window**：
- `ProtectSupplyLine` — 保护军粮路线（消耗 force / cash，若 warfare 启用）
- `EscortRoadReport` — 护送边报/军粮（消耗 labor，有 casualty 风险）
- `FundLocalWatch` — 维持本地治安（因征兵导致本地 guard 空虚）
- `PetitionYamen` — 请求减免征发（publicFace 消耗）

**Ignore Consequence**：
- 下月：军需征发强制落地，家户劳动力和粮食减少
- 下季度：若 campaign 失败，fatigue 持续，本地 guard 空虚，bandit risk 上升
- 下年：war loss 家庭的 debt 和 mourning 压力可能传导到宗族和婚姻市场

**Backlash / Memory**：
- 军需征发中的强制行为：`SocialMemoryAndRelations` 记录 Fear / Grudge（对衙门、对朝廷）
- campaign 失败后的 blame：`PublicLifeAndRumor` 传播"朝廷无能"
- 阵亡者家庭的长期影响：`FamilyCore` 继承压力，可能触发 heir succession crisis

**Module Boundary Check**：
- `WarfareCampaign` 拥有战役状态，通过事件传播 aftermath
- `ConflictAndForce` 拥有 force posture，不直接写家户
- `OfficeAndCareer` 拥有征发命令，不直接写市场价格
- `TradeAndIndustry` 拥有市场转向，通过事件通知下游

---

### 链6：灾荒-赈济-家户-市场-匪患-秩序

> **实现状态：真实 scheduler 薄切片（M2-lite）+ 第一层规则加厚。已落 `WorldSettlements.DisasterDeclared -> OrderAndBanditry.DisorderSpike -> PublicLife heat`，并有真实 `MonthlyScheduler` drain 测试、off-scope 聚落负例、metadata-only 规则测试和重复灾荒宣告水位测试。`OrderAndBanditry` 已将固定 severity delta 改为灾害失序 profile：读取 `severity/floodRisk/embankmentStrain`，再叠本地失序土壤、路面裂口和镇压缓冲。完整版的赈济决策、市场恐慌、家户生存/迁徙、路险、疫病、SocialMemory 灾后记忆、PublicLife 合法性仍未实现。**

**Trigger**：`WorldSettlements` 灾荒判定（`Drought` / `Flood` / `Locust` / `Epidemic`）。

**Event Flow**：

```
WorldSettlements (seasonal)
  └── WorldSettlements.DisasterDeclared { settlementId, disasterKind, severity, durationEstimate }
      ✅ THIN-SLICE DONE：当前仅 flood，payload 通过 DomainEvent.Metadata 携带
         { cause: disaster, disasterKind: flood, severity, floodRisk, embankmentStrain }
      ✅ THIN-SLICE DONE：WorldSettlements schema 7 用 LastDeclaredFloodDisasterBand 防止同一活动水患每月重复宣告
      ├──→ PopulationAndHouseholds
      │     ⏳ FULL CHAIN TODO
      │     更新 harvestForecast = 0（或严重减损）
      │     更新 illness risk（epidemic 时）
      │     发出 PopulationAndHouseholds.HouseholdSubsistencePressureChanged { settlementId, severity }
      │
      ├──→ TradeAndIndustry
      │     ⏳ FULL CHAIN TODO
      │     粮价预测暴涨
      │     发出 TradeAndIndustry.MarketPanicRisk { settlementId, goods: grain }
      │
      └──→ PublicLifeAndRumor
            ⏳ FULL CHAIN TODO：直接灾异/赈济公告投影尚未落地；当前薄切片通过 DisorderSpike 间接加热街谈
            更新 notice: "灾异示警" / "开仓赈济"
            更新 street talk: "今年颗粒无收"
            若 epidemic: 更新 temple activity（祈福、避疫）

PopulationAndHouseholds (xun pulse)
  └── PopulationAndHouseholds.HouseholdSubsistencePressureChanged
      └──→ 持续 crisis:
            发出 MigrationStarted { householdId, direction: "toward_county_seat" }
            或发出 TenantFlight { householdId, landlordClanId }
            若 illness risk 实现:
              发出 DeathByIllness { personId, causeKey: "epidemic" }

TradeAndIndustry (monthly)
  └── TradeAndIndustry.MarketPanicRisk
      └──→ 若 panic 实现:
            价格暴涨
            发出 TradeAndIndustry.GrainPriceSpike { settlementId, multiplier }
            若 price > affordability threshold:
              发出 TradeDebtDefaulted { traderId, causeKey: "disaster-panic" }

OrderAndBanditry (monthly)
  └── ✅ THIN-SLICE + FIRST THICKENING DONE：消费 WorldSettlements.DisasterDeclared
      - 只按 EntityKey 修改指定 settlement
      - 只读 Metadata，不解析 Summary
      - 失序 delta = 灾害压力(severity/floodRisk/embankmentStrain)
                   + 本地失序土壤(disorder/bandit/black-route/coercion)
                   + 路面裂口(route/retaliation/implementation drag)
                   - 镇压缓冲(suppression relief/route shielding/response/admin window)
      - 跨 50 阈值时发 OrderAndBanditry.DisorderSpike
      - DisorderSpike 继续携带 cause/source/severity/disaster-disorder profile metadata 供 PublicLife 投影
  └── ⏳ FULL CHAIN TODO：读取 PopulationAndHouseholds migration + TradeAndIndustry route risk
      若 refugee influx > capacity:
        更新 settlementDisorder
        发出 OrderAndBanditry.DisorderSpike { settlementId, causeKey: "refugee-influx" }
      若 grain scarcity + guard 空虚:
        发出 OrderAndBanditry.BanditRecruitmentOpportunity { settlementId }

OfficeAndCareer (monthly)
  └── 读取 WorldSettlements.DisasterDeclared + PopulationAndHouseholds.HouseholdSubsistencePressureChanged
      若 granarySecurity 足够:
        发出 WorldSettlements.ReliefDelivered { settlementId, scope, grainAmount }
        WorldSettlements.ReliefDelivered → PopulationAndHouseholds（缓解 crisis）
      若 granarySecurity 不足:
        发出 WorldSettlements.ReliefWithheld { settlementId, reason: "granary-empty" }
        WorldSettlements.ReliefWithheld → PublicLifeAndRumor（"衙门口说没粮了"）
        WorldSettlements.ReliefWithheld → SocialMemoryAndRelations（Grudge: "官府见死不救"）
      注意：OfficeAndCareer 处理赈济命令和案牍，实际分发执行和秩序维护由 PopulationAndHouseholds 和 OrderAndBanditry 处理

PublicLifeAndRumor (monthly, 通过 query)
  └── 读取所有灾荒相关状态
      更新 public legitimacy:
      - 若 relief 及时充足: legitimacy + significant
      - 若 relief 失败或延迟: legitimacy - significant
      - 若 epidemic + 无医: fear + significant
```

**Player Window**：
- `GuaranteeDebt` — 为受灾户担保购粮（消耗 credit）
- `FundLocalWatch` — 维持秩序（防止灾民骚乱）
- `PetitionYamen` — 请求开仓赈济（成功率依赖 granary + publicFace）
- `EscortRoute` — 若外地有粮，护送粮队进入灾区（消耗 labor + cash，风险高但回报高）
- `InvestInEstate` — 长期来看，投资水利（降低 future flood/drought risk）

**Ignore Consequence**：
- 下月：migration 开始，disorder 上升，若 granary 空则死亡率上升
- 下季度：若 disorder 持续，bandit recruitment 实现，route 长期不安全
- 下年：灾后土地可能荒芜，tenant 流失， landlord 收入下降，可能引发 landlord → kehu 转换

**Backlash / Memory**：
- 赈济成功：`SocialMemoryAndRelations` 记录 Favor（"某年某族长开仓"）
- 赈济失败：`SocialMemoryAndRelations` 记录 Grudge + Shame（"官府见死不救"）
- 疫后：`SocialMemoryAndRelations` 长期 Fear（"那年瘟疫"）
- `PublicLifeAndRumor`：灾异叙事可能成为年度甚至十年标签，影响后续 legitimacy 和 petition 成功率

**Module Boundary Check**：
- `WorldSettlements` 拥有灾荒判定，不直接写家户
- `PopulationAndHouseholds` 拥有家户生存，通过事件传播危机
- `TradeAndIndustry` 拥有市场恐慌和价格
- `OfficeAndCareer` 拥有赈济决策，不直接写家户状态（通过事件 `WorldSettlements.ReliefDelivered`）
- `OrderAndBanditry` 拥有 disorder 和 bandit，读取人口迁移状态但不写家户
- `PublicLifeAndRumor` 只读查询

---

## 玩家影响圈动态规格

### 影响圈不是职业，是投影

`PlayerInfluenceFootprintSnapshot` 是应用层对现有模块 Query 的只读合成。它不进权威存档，不产生新命令，只解释玩家当前能看见什么、能碰到什么、碰不到什么。

### 影响圈七层

| Layer | 可见性等级 | 什么决定它 | 什么时候收缩 |
|---|---|---|---|
| **Household** | Commandable | 玩家锚定家户 | 永不收缩（锚定） |
| **Lineage** | Commandable / Indirect | 宗族 membership + prestige | 分支分裂、 prestige 崩溃 |
| **Market** | Commandable / WatchOnly | 贸易资产、信用、路线控制 | 破产、路线丧失 |
| **Education** | Indirect / WatchOnly | 资助记录、 academy 关系 | 资金枯竭、 academy 关闭 |
| **Yamen** | Indirect / WatchOnly | petition 历史、 office 关系 | 官员调任、弹劾、派系失势 |
| **PublicLife** | WatchOnly | public face、 reputation | 丑闻、 shame 曝光 |
| **Disorder / Force** | WatchOnly / Absent | 武力资产、秩序接触 | 镇压、武力解散 |
| **Imperial** | Absent / WatchOnly | 后期才解锁 | MVP 只能是 watch-only |

### 影响圈变化规则

**扩张条件**：
- `Market`：成功投资店铺/路线，信用良好
- `Education`：资助学业成功，与 academy 建立关系
- `Yamen`：petition 成功，与官员建立 patronage
- `PublicLife`：public face 提升，赞助公共工程
- `Disorder / Force`：拥有 retainers，参与 escort

**收缩条件**：
- `Market`：破产、债务违约、路线被截
- `Education`：资助失败、 academy 排斥
- `Yamen`：petition 失败、官员被贬、派系失势
- `PublicLife`：丑闻、 shame 曝光、公共工程失败
- `Lineage`：分支分裂、 prestige 崩溃、继承危机未解决

**关键规则**：影响圈变化是**被动的**，不是玩家主动"切换职业"。它是世界对玩家行为的反馈。

### 命令可用性

命令是否出现在玩家面前，由影响圈 + 世界状态共同决定：

```
CommandAvailable = 
  InfluenceLayer.Contains(command.requiredLayer) 
  && WorldState.Satisfies(command.preconditions)
  && !PlayerState.BlockedBy(command.blockingConditions)
```

例如：
- `DesignateHeirPolicy` 要求 `Lineage` 层 Commandable + 有死亡事件 + 有 eligible candidate
- `PetitionYamen` 要求 `Yamen` 层至少 Indirect + 有 grievance + publicFace > threshold
- `EscortRoute` 要求 `Market` 或 `Disorder` 层 + 有 unsafe route + 有可用 labor/force

---

## 模块边界澄清

### OfficeAndCareer vs OrderAndBanditry vs PublicLifeAndRumor

这三个模块最容易模糊。明确边界：

| 概念 | 拥有模块 | 为什么 |
|---|---|---|
| 官员任命、考课、候缺、派系 | OfficeAndCareer | 官职是人的社会位置 |
| 衙门 workload、petition backlog、 clerk 依赖 | OfficeAndCareer | 衙门是官署运营 |
| 逮捕、惩罚、suppress、black-route | OrderAndBanditry | 治安是执行行为 |
|  disorder 压力、bandit 实体、route shielding | OrderAndBanditry | 失序是社会状态 |
| 榜文内容、街谈、路报、州牒 | PublicLifeAndRumor | 舆论是信息传播，但 PublicLifeAndRumor 不拥有外部模块的权威状态；它只拥有公共渠道热度、合法性语言和 crowd mood 等自己的投影状态 |
| 公共合法性、shame、crowd mood | PublicLifeAndRumor | legitimacy 是集体认知，由 PublicLifeAndRumor 从各模块查询后合成；它不直接写家户债务或官员 rank |

**禁止**：
- `OfficeAndCareer` 不能直接创建 bandit 实体
- `OrderAndBanditry` 不能直接任命官员
- `PublicLifeAndRumor` 不能直接改变家户债务或官员 rank

### 皇权与地方

`WorldSettlements` 拥有 `ImperialBand`（季节/年度节律），但：
- 不拥有圣旨文本（那是 `PublicLifeAndRumor` 的投影）
- 不拥有官员任命（那是 `OfficeAndCareer`）
- 不拥有税役征收（那是 `PopulationAndHouseholds` + `OfficeAndCareer`）

皇权→地方的传导必须是：**`WorldSettlements` 发事件 → `OfficeAndCareer` 响应 → `PopulationAndHouseholds` / `TradeAndIndustry` 承受 → `PublicLifeAndRumor` 传播**。

不允许 `WorldSettlements` 直接修改家户状态。

### 市场与家户

`TradeAndIndustry` 拥有价格、店铺、路线、债务。
`PopulationAndHouseholds` 拥有生计、消费、健康、迁移。

边界：
- 价格变化通过事件通知家户
- 家户的 cash need 通过查询影响市场
- `TradeAndIndustry` 不直接写家户的 healthResilience
- `PopulationAndHouseholds` 不直接写市场价格

---

---

### 链7：官员-胥吏-案牍-地方执行

**Trigger**：`OfficeAndCareer` 中官员到任、考课周期到达、或 petition backlog 超过 threshold。

**Event Flow**：

```
OfficeAndCareer (monthly)
  └── OfficeGranted { personId, postId, location, rank, credentialBand }
      └──→ OfficeAndCareer 内部状态更新
            post.currentHolder = personId
            post.vacancyMonths = 0
            post.clerkDependence = 初始值（高，新官不熟悉本地案牍）
            若 clerkDependence > threshold:
              记录 clerkCaptureRisk（状态字段，不发出事件）

OfficeAndCareer (monthly, 考课周期)
  └── 考课周期到达（内部 cadence，不发事件）
      ├──→ OfficeAndCareer 内部
      │     计算 evaluationScore = f(petitionBacklog, taxCollectionRate, disorderLevel, publicLegitimacy)
      │     若 evaluationScore < threshold:
      │       更新 evaluationPressure（状态字段）
      │     若 memorialAttackRisk > threshold:
      │       发出 OfficeAndCareer.MemorialAttackReceived { officialId, attackerFaction, accusationKey }
      │
      └──→ SocialMemoryAndRelations（通过事件）
            记录 evaluation 结果作为 reputation 调整依据
            若 evaluation 优秀: FavorIncurred（与 patron 关系加深）
            若 evaluation 差 + memorial attack: Grudge（与攻击者结怨）

OfficeAndCareer (xun pulse)
  └── OfficeAndCareer.YamenOverloaded { postId, taskKind: "petition-backlog", count, capacity }
      └──→ OfficeAndCareer 内部
            更新 clerkDependence（案牍积压 → 胥吏权力上升）
            若 clerkDependence > captureThreshold:
              发出 OfficeAndCareer.ClerkCaptureDeepened { postId, clerkFactionId, capturedProcesses }
            发出 OfficeAndCareer.YamenOverloaded { settlementId, taskKind: "petition-backlog", delayMonths }

OfficeAndCareer (monthly)
  └── OfficeAndCareer.ClerkCaptureDeepened { postId, clerkFactionId, capturedProcesses }
      ├──→ PopulationAndHouseholds（通过查询读取案牍延迟）
      │     家户感受到 petition 处理缓慢
      │     若 delay > threshold:
      │       记录 householdPetitionDelay（状态字段，PublicLifeAndRumor 通过 Query 投影）
      │
      ├──→ TradeAndIndustry（通过查询）
      │     市场纠纷案牍延迟 → 商人信心下降
      │
      └──→ PublicLifeAndRumor（通过查询）
            更新 street talk: "新来的县太爷被书吏架空了"
            更新 notice wall: 案牍积压公告

OfficeAndCareer (monthly)
  └── OfficeAndCareer.OfficialSupplyRequisition { settlementId, grainQuota, cashQuota }
      └──→ 官员-胥吏-执行链
            OfficeAndCareer 发出征发命令文书
            实际执行由 clerk/runner 完成
            若 clerk capture 深:
              征发可能被 reinterpreted（多征、少征、选择性执行）
              发出 OfficeAndCareer.PolicyImplemented { settlementId, intendedQuota, actualQuota, distortionKind }
            OfficeAndCareer.PolicyImplemented → PopulationAndHouseholds（实际负担与文书不符）
            OfficeAndCareer.PolicyImplemented → PublicLifeAndRumor（"衙役又来多收了"）
```

**Player Window**：
- `PetitionYamen` — 向衙门提交 petition（消耗 publicFace，成功率受 clerk capture 和 backlog 影响）
- `RecommendSomeone` — 推荐可靠胥吏或师爷给官员（消耗 reputation，可降低 clerk capture risk）
- `DeployAdministrativeLeverage` — 若玩家有 office reach，可加速案牍处理（消耗 office authority）
- `PostCountyNotice` — 在衙门口张贴公告（消耗 publicFace，影响 public legitimacy）

**Ignore Consequence**：
- 下月：clerk capture 持续加深，petition 处理更慢，家户 frustration 上升
- 下季度：若 evaluation 持续差，官员可能被调任/贬谪，clerk faction 更稳固
- 下年：案牍积压导致 market 纠纷无法解决，商人信心下降，可能影响 trade route 安全

**Backlash / Memory**：
- `SocialMemoryAndRelations`：谁帮了新官、谁架空了新官、谁在案牍中吃了亏
- `PublicLifeAndRumor`："某县太爷被书吏玩得团团转" 成为长期 stigma
- 若玩家推荐的人确实降低了 clerk capture：玩家与官员之间形成 patronage 纽带

**Module Boundary Check**：
- `OfficeAndCareer` 拥有官员、任命、考课、案牍、 clerk dependence
- `OfficeAndCareer` 发出征发命令文书，但不直接执行（执行由 clerk/runner 完成，其效果通过 `OfficeAndCareer.PolicyImplemented` 事件传播）
- `PopulationAndHouseholds` 承受实际征发负担，不直接写官员状态
- `PublicLifeAndRumor` 只读查询，传播舆论

---

### 链8：朝会/奏议-任命/政策窗口-地方传导

**Trigger**：`WorldSettlements` 的 `ImperialBand` 积累到阈值，或 `OfficeAndCareer` 的 senior official 积累足够 faction backing。

**Event Flow**：

```
WorldSettlements (seasonal / annual)
  └── WorldSettlements.CourtAgendaPressureAccumulated { agendaKind: Fiscal / Frontier / DisasterRelief / Appointment / Examination / Law / Ritual / Succession / Reform, pressureBand }
      └──→ OfficeAndCareer（高层官员）
            更新 court-facing officials 的 faction heat
            若 pressureBand > windowThreshold:
              发出 OfficeAndCareer.PolicyWindowOpened { agendaKind, windowDurationMonths, sponsoringFaction }

OfficeAndCareer (monthly, court-facing)
  └── OfficeAndCareer.PolicyWindowOpened { agendaKind, windowDurationMonths, sponsoringFaction }
      ├──→ OfficeAndCareer 内部
      │     高层官员可推动 appointment slate 或 policy wording
      │     发出 OfficeAndCareer.AppointmentSlateProposed { slateId, appointments[], factionAlignment }
      │     发出 OfficeAndCareer.PolicyWordingDrafted { policyId, agendaKind, wording, expectedImpact }
      │
      └──→ SocialMemoryAndRelations
            记录 faction alignment 变化
            支持方: FavorIncurred
            反对方: GrudgeEscalated

OfficeAndCareer (monthly)
  └── OfficeAndCareer.AppointmentSlateProposed { appointments[] }
      └──→ 任命 cascade
            每个 appointment 产生 OfficeGranted { personId, postId, location, rank }
            若 appointment 涉及地方官员调任:
              原地方官员的 post 变为 vacant
              发出 OfficeLost { postId, previousHolderId, reason: "transfer" }
              OfficeLost → PopulationAndHouseholds（本地居民感受到官员更替）
              OfficeLost → PublicLifeAndRumor（"新县太爷要来了"）

OfficeAndCareer (monthly)
  └── OfficeAndCareer.PolicyWordingDrafted { policyId, agendaKind, expectedImpact }
      └──→ 地方传导
            政策 wording 通过 dispatch 到达 county
            发出 OfficeAndCareer.DispatchArrived { settlementId, policyId, wording, urgency }
            OfficeAndCareer.DispatchArrived → OfficeAndCareer（地方官员收到命令）
            OfficeAndCareer.DispatchArrived → PublicLifeAndRumor（榜文张贴）

OfficeAndCareer (monthly, 地方)
  └── OfficeAndCareer.DispatchArrived { policyId, wording, urgency }
      └──→ 地方官员响应
            若 urgency 高 + clerk capture 低:
              快速执行，发出 OfficeAndCareer.PolicyImplemented { settlementId, policyId }
            若 urgency 低 或 clerk capture 高:
              拖延/缓冲/纸面服从，发出 OfficeAndCareer.PolicyImplemented { settlementId, policyId, delayMonths }
            若 local elite 反对:
              选择性执行/截留，发出 OfficeAndCareer.PolicyImplemented { settlementId, policyId, capturingActor }

PopulationAndHouseholds (monthly)
      └── OfficeAndCareer.PolicyImplemented
      └──→ 家户感受政策实际效果
            outcome = Rapid：负担快速落地，可能引发 resistance
            outcome = Dragged：负担延迟，家户不确定未来
            outcome = Captured：负担被 elite 转嫁给弱势家户
```

**Player Window**：
- 若玩家有 court reach（后期）：参与 faction coalition，影响 policy wording（消耗 faction credit，高风险）
- 若玩家有 yamen reach：向地方官员 petition，请求缓冲政策执行（消耗 publicFace）
- 若玩家有 public-life reach：在街谈中传播对政策的不满或支持（消耗 reputation，影响 public legitimacy）
- `Endure` — 承受政策落地压力

**Ignore Consequence**：
- 下月：policy 通过 dispatch 到达地方，地方官员开始执行/拖延
- 下季度：若 policy 涉及 tax/grain/corvée，家户负担变化；若涉及 exam/school，教育投资变化
- 下年：faction alignment 变化影响后续 appointment；policy implementation 质量影响 public legitimacy

**Backlash / Memory**：
- `SocialMemoryAndRelations`：谁支持了政策、谁反对了、谁在执行中吃了亏
- `PublicLifeAndRumor`：政策口碑成为年度叙事（"某年新政害民" / "某年赈济及时"）
- 若政策失败：`FamilyCore` 和 `PopulationAndHouseholds` 长期压力；`OrderAndBanditry` disorder 上升

**Module Boundary Check**：
- `WorldSettlements` 拥有 `ImperialBand` 和 agenda pressure 积累
- `OfficeAndCareer` 拥有 court process、appointment、policy wording
- `OfficeAndCareer` 地方层拥有 implementation 决策，但不直接写家户状态
- `PopulationAndHouseholds` 承受实际政策效果
- `PublicLifeAndRumor` 传播舆论

---

### 链9：政权承认-税粮/官员站队/粮道/仪礼-地方服从

**Trigger**： regime legitimacy 下降到临界值，或 rebellion/polity formation 压力积累到阈值。（P5 以后才 fully playable，但静态结构需在 MVP 预留）。

**Event Flow**：

```
WorldSettlements (annual / seasonal, P5+)
  └── WorldSettlements.RegimeLegitimacyShifted { regimeId, legitimacyDelta, causeKey }
      └──→ OfficeAndCareer
            更新官员站队倾向（loyalty / defection /观望）
            更新 defectionRisk（状态字段，不发出事件）
            若 defection 达到 threshold:
              发出 OfficeAndCareer.OfficeDefected { postId, officialId, newRegimeId }

OfficeAndCareer (monthly, P5+)
  └── OfficeAndCareer.OfficeDefected { postId, officialId, newRegimeId }
      ├──→ PopulationAndHouseholds
      │     该辖区的 tax/corvée 归属变化
      │     家户感受到"换天了"
      │     发出 PopulationAndHouseholds.HouseholdRegimeTransition { householdId, oldRegimeId, newRegimeId }
      │
      ├──→ TradeAndIndustry
      │     该辖区的 route control / tax collection 归属变化
      │     发出 TradeAndIndustry.RouteControlShifted { routeId, oldController, newController }
      │
      └──→ PublicLifeAndRumor
            更新 notice: "新朝已立" / "某官已降"
            更新 street talk: "朝廷完了，咱们听谁的"
            更新 public legitimacy: 基于新 regime 的 ritual claim 和 force backing

WorldSettlements (annual / seasonal, P5+)
  └── WorldSettlements.GrainRouteControlDisputed { routeId, claimants[] }
      └──→ TradeAndIndustry
            粮道控制归属影响 grain transport
            若 route 被 rebel 控制:
              发出 TradeAndIndustry.GrainRouteBlocked { routeId, controller, reason: "rebel-tax" }
            若 route 被 loyalist 恢复:
              发出 TradeAndIndustry.GrainRouteReopened { routeId, restorer }

TradeAndIndustry (monthly, P5+)
  └── TradeAndIndustry.GrainRouteBlocked { routeId, controller }
      └──→ PopulationAndHouseholds
            粮道阻断 → 粮价飙升 → 家户危机
            （回到链2的粮价-市场-家户逻辑）

WorldSettlements (annual / seasonal, P5+)
  └── WorldSettlements.RitualClaimStaged { regimeId, ritualKind, location, legitimacyDelta }
      └──→ PublicLifeAndRumor
            仪礼事件影响 public belief
            若 ritual 成功: public legitimacy + significant
            若 ritual 失败/被扰: public legitimacy - significant，Shame 扩散
            发出 PublicLifeAndRumor.PublicLegitimacyShifted { settlementId, regimeId, delta }

PublicLifeAndRumor (monthly, P5+)
  └── PublicLifeAndRumor.PublicLegitimacyShifted
      └──→ PopulationAndHouseholds
            家户的 compliance / resistance 倾向变化
            若 legitimacy 极低:
              发出 PopulationAndHouseholds.HouseholdComplianceShifted { householdId, causeKey: "regime-illegitimate", direction: "Resistance" }
            若 legitimacy 高:
              发出 PopulationAndHouseholds.HouseholdComplianceShifted { householdId, causeKey: "regime-legitimate", direction: "Compliance" }
```

**Player Window**（P5+）：
- 若玩家支持 loyalist：资助 loyalist force、护送 loyalist grain route、在公共生活中传播 loyalist legitimacy
- 若玩家支持 rebel：资助 rebel force、封锁 loyalist grain route、在公共生活中传播 rebel ritual claim
- 若玩家观望：`Endure`，观察哪方胜算更大
- 所有介入都消耗 real resources（force / grain / cash / reputation）并产生 backlash

**Ignore Consequence**：
- 下月：regime legitimacy 继续漂移，官员继续站队
- 下季度：grain route control 决定粮价和家户生存
- 下年：regime 可能稳定、分裂、或被推翻；player 的选择决定其在新 regime 中的位置

**Backlash / Memory**：
- `SocialMemoryAndRelations`：谁忠于旧朝、谁投靠新朝、谁在乱世中见死不救
- `PublicLifeAndRumor`：regime change 叙事成为世代记忆
- `FamilyCore`：regime change 中的死亡、财产丧失、婚姻破裂

**Module Boundary Check**：
- `WorldSettlements` 拥有 regime-level 状态（`RegimeCycleState`、`ImperialBand`）
- `OfficeAndCareer` 拥有官员站队，不直接写 regime state
- `TradeAndIndustry` 拥有粮道经济，不直接写 regime state
- `PopulationAndHouseholds` 拥有家户服从/抵抗，不直接写 regime state
- `PublicLifeAndRumor` 拥有舆论和合法性投影
- **核心规则**：regime change 不是一键改国号，而是通过 tax reach、appointment reach、grain route reach、force backing、ritual claim、public belief 的渐进变化实现

---

## 实施优先级

按 `STATIC_BACKEND_FIRST.md` 原则，先稳定静态结构，再填充规则密度：

1. **P0**：确保上述9条链的**事件名**和**模块边界**在代码中已有定义或已规划 ✅
2. **P1**：为每条链写**一条薄规则链测试**（如：TaxSeason → HouseholdDebtSpiked → PublicLifeHeated）
   - ✅ 链1 薄切片（税役-家户-衙门-公共生活）— `Chain1_TaxSeasonOpens_DebtsSpike_YamenOverloads_PublicLifeReacts` + `Chain1_RealMonthlyScheduler_DrainsTaxSeasonIntoYamenAndPublicLife`
   - ✅ 链1 第一层规则加厚 — `TaxSeasonBurdenHandlerTests` 覆盖多维家户画像、结构化 tax-profile metadata、settlement scope 负例、symbolic global thin 信号兼容
   - ⏳ 链1 完整版（正式户等/税种/额度公式、客户租压/佃户逃散、税季现金挤压市场、SocialMemory/年度公共残留、精确 jurisdiction payload）
   - ✅ 链2 薄切片（粮价-市场-家户）— `Chain2_HarvestPhase_GrainPriceSpike_SubsistencePressureChanged` + `Chain2_RealMonthlyScheduler_DrainsHarvestPriceIntoLocalHouseholdPressure`
   - ✅ 链2 第一层规则加厚 — `GrainPriceSubsistenceHandlerTests` 覆盖粮价/供需 metadata、多维家户生计压力画像、结构化 subsistence-profile metadata、settlement scope 负例
   - ⏳ 链2 完整版（yieldRatio/灾荒、granarySecurity/routeRisk、家户粮仓/生计类型、迁徙/病亡、路险、SocialMemory/PublicLife 饥荒叙事）
   - ✅ 链3 薄切片（ExamPassed → ClanPrestigeAdjusted）— `ExamPrestigeChainTests.cs`
   - ✅ 链3 第一层规则加厚 — `ExamResultHandlerTests` 覆盖 credential metadata、多维宗族声望画像、结构化 exam-prestige metadata、off-scope clan 负例
   - ⏳ 链3 完整版（OfficeAndCareer waiting list / SocialMemory Favor-Shame / PublicLife 放榜投影）
   - ✅ 链4 薄切片 + 第一层规则加厚（ImperialRhythmChanged → AmnestyApplied(metadata) → amnesty-disorder profile → DisorderSpike）— `ImperialAmnestyDisorderChainTests.cs` + `AmnestyDispatchHandlerTests.cs` + `AmnestyDisorderHandlerTests.cs`
   - ✅ 链6 薄切片 + 第一层规则加厚（DisasterDeclared → disaster-disorder profile → DisorderSpike → PublicLife）— `DisasterDisorderPublicLifeChainTests.cs` + `DisasterDisorderHandlerTests.cs` + metadata-only / repeated-declaration tests
   - ⏳ 链6 完整版（赈济决策、市场恐慌、家户生存/迁徙、路险、疫病、SocialMemory 灾后记忆、PublicLife 合法性）
   - ✅ 链5 薄切片 + 第一层规则加厚（FrontierStrainEscalated → OfficialSupplyRequisition(profile metadata) → household burden profile → HouseholdBurdenIncreased）— `FrontierSupplyHouseholdChainTests.cs` + `FrontierSupplyHandlerTests.cs` + `OfficialSupplyBurdenHandlerTests.cs`
   - ⏳ 链5 完整版（WarfareCampaign mobilization、ConflictAndForce readiness、TradeAndIndustry market diversion）
3. **P2**：补充事件 handler 的**空实现**（先存在接口，再填充逻辑）
   - ✅ 链1 thin handler 已落并加厚（`ApplyTaxSeasonPressure` 读取多维 household profile；`DispatchPopulationDebtEvents`, `HandleEvents(YamenOverloaded)`）
   - ✅ 链2 thin handler 已落并加厚（`ApplyHarvestPricePulse` 写入粮价/供需 metadata；`ApplyGrainPriceSubsistencePressure` 读取多维 household profile）
   - ✅ 链3 thin handler 已落并加厚（`ExamPassed` 写入 credential metadata；`ApplyExamPassPrestige` 读取 credential metadata 与 family-owned profile）
4. **P3**：填充**压力公式和阈值**
5. **P4**：连接**Unity 壳层投影**

当前 Contracts 常量状态：
- ✅ `WorldSettlementsEventNames`（15+ 个常量，含 `TaxSeasonOpened`, `SeasonPhaseAdvanced`, `DisasterDeclared` 等）
- ✅ `PopulationEventNames`（13 个常量，含 `HouseholdDebtSpiked`, `HouseholdSubsistencePressureChanged` 等）
- ✅ `TradeAndIndustryEventNames`（11 个常量，含 `GrainPriceSpike`, `GrainPriceSpikeRisk` 等）
- ✅ `OfficeAndCareerEventNames`（16+ 个常量，含 `YamenOverloaded`, `PolicyImplemented` 等）
- ✅ `OrderAndBanditryEventNames`, `ConflictAndForceEventNames`, `EducationAndExamsEventNames`, `SocialMemoryAndRelationsEventNames`

---

## 与现有文档的关系

本文档不替代已有文档，而是把已有文档中的**概念性压力链**翻译成**可执行的模块间事件流**：
- `SOCIAL_STRATA_AND_PATHWAYS.md` → 本文档的9条链提供了具体的跨模块连接
- `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md` → 本文档的链4和链5提供了皇权/边防压力的具体落地方式
- `MODULE_BOUNDARIES.md` → 本文档的"模块边界澄清"节补充了容易模糊的三模块边界
- `PLAYER_SCOPE.md` → 本文档的"玩家影响圈动态规格"节提供了影响圈的计算公式
- `LIVING_WORLD_DESIGN.md` → 本文档的静态容器已在代码中大部分实现，本文档补全了动态连接

---

## 版本记录

### v0.3 (2026-04-21) — Contract Preflight 完成
- **命名风格**：所有新增跨模块 DomainEvent 统一前缀 `Module.EventName`
- **旧事件兼容**：已有无前缀常量（`HouseholdDebtSpiked`, `ClanPrestigeAdjusted` 等）字符串值不变
- **新建 Contracts 类**：`EducationAndExamsEventNames`, `OfficeAndCareerEventNames`, `OrderAndBanditryEventNames`, `TradeAndIndustryEventNames`, `ConflictAndForceEventNames`, `SocialMemoryAndRelationsEventNames`
- **扩展现有 Contracts 类**：`PopulationEventNames`, `FamilyCoreEventNames`, `WorldSettlementsEventNames`, `WarfareCampaignEventNames`, `PublicLifeAndRumorEventNames`
- **删除误标事件（状态字段）**：`EvaluationPressure`, `ClerkCaptureRisk`, `WaitingListFrustration`, `OfficialDefectionRisk`, `HouseholdPetitionFrustration`, `EvaluationCycleTriggered`
- **合并为枚举**：`RapidImplementation`/`ImplementationDrag`/`ImplementationCaptured`/`ImplementationDistorted` → `PolicyImplemented` + `outcome = Rapid | Dragged | Captured | PaperCompliance`；`HouseholdSubsistenceStrain`/`Crisis` → `HouseholdSubsistencePressureChanged` + `band` enum；`HouseholdResistance`/`Compliance` → `HouseholdComplianceShifted` + `direction` enum
- **复用现有名称**：`ImperialBandChanged` → `ImperialRhythmChanged`；`ReliefDistributed` → `ReliefDelivered`；`ReliefFailed` → `ReliefWithheld`；`CampaignAftermath` → `CampaignAftermathRegistered`；`OfficialArrived` → `OfficeGranted`；`PostVacated` → `OfficeLost`
- **模块引用迁移**：6 个模块的内部裸字符串全部迁入 `Zongzu.Contracts`

### v0.4 (2026-04-23) Chain 7/8/9 thin-slice hardening
- Chain 7 is implemented only as a real-scheduler thin slice: `OfficeAndCareer.ClerkCaptureDeepened -> PublicLifeAndRumor` with settlement scope, structured metadata, off-scope protection, and module-owned repeated-edge suppression through `ActiveClerkCaptureSettlementIds`.
- Chain 8 is implemented only as a real-scheduler thin slice: `WorldSettlements.CourtAgendaPressureAccumulated -> OfficeAndCareer.PolicyWindowOpened`. The current rule allocates one court/global pressure event to one selected court-facing jurisdiction; it must not open all jurisdictions at once.
- Chain 9 is implemented only as a real-scheduler thin slice: `WorldSettlements.RegimeLegitimacyShifted -> OfficeAndCareer.OfficeDefected`. `OfficeDefected` is a receipt after office-owned state mutation, not a standalone event-pool outcome; only the highest-risk appointed official defects in the current slice.
- Default `MandateConfidence` is neutral (`70`). Court / regime pressure must be explicitly seeded or moved by an imperial/court owner; an uninitialized world must not behave like a regime crisis.
- The full versions still need court-process state, policy dispatch, faction / public-life / household compliance propagation, market and force consequences, memory residue, and future imperial/dynasty-cycle ownership.

### v0.2 (2026-04-21)
- 新增 3 条链（official-clerk-execution, court-agenda-dispatch, regime-recognition-compliance）
- 校准仁宗朝语言（exam visibility, commercial monetization）
- 添加事件名状态总表（48→76 个名称）
- 澄清 `PublicLifeAndRumor` / `OfficeAndCareer` / `OrderAndBanditry` 三模块边界

### v0.1 (2026-04-20)
- 初稿：6 条跨模块压力链 + 玩家影响圈规格
