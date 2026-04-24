# PERSON_OWNERSHIP_RULES

本文档定义人物数据的所有权边界。

> **相关骨骼**：`SPATIAL_SKELETON_SPEC.md` 是空间骨骼；本文件是人物骨骼。两者正交：空间不含人，人物不含节点；两者通过 `SettlementId`（人物在某地）/ `PersonId`（节点事件牵连某人）在 projection 层交汇。

## 核心决策

**方案 B：Kernel 层 PersonRegistry + 各模块各自持有领域状态。**

没有任何一个模块是"人物总表"。
FamilyCore 只管宗族视角的人。
一个人的完整画像由投影层合并多模块 Query 产出。

## 为什么

- `MULTI_ROUTE_DESIGN_MATRIX.md`：人在路径间流动是核心游戏性
- `EXTENSIBILITY_MODEL.md`：加新模块不改旧模块
- `MODULE_BOUNDARIES.md`：每个模块只拥有自己领域的状态
- `SIMULATION_FIDELITY_MODEL.md`：升格/降格是跨模块行为

如果某个模块当人物总表，路径流动每次都要绕经那个模块，摩擦高了路径流动就薄了。

---

## PersonRegistry（Kernel 层）

### 定位

最薄的共享身份锚点。不含任何领域逻辑。
所有模块通过 `PersonId` 引用同一个人。

PersonRegistry 是 **identity-only**。它不是可变世界状态底座。如果一个字段回答的是"这个人在做什么/感受什么/能干什么/跟谁有关系"而不是"这个人存在吗、活着吗、多大了"，它就不属于这里，属于领域模块。见 `ARCHITECTURE.md`。

### 拥有的状态

```
PersonRecord:
  PersonId id
  string displayName
  GameDate birthDate
  Gender gender
  LifeStage lifeStage        // Infant / Child / Youth / Adult / Elder / Deceased
  bool isAlive
  FidelityRing fidelityRing  // Core / Local / Regional
```

### 不拥有

- 性格
- 能力
- 社会位置
- 健康细节
- 亲属关系
- 任何领域状态

### 职责

1. 分配 PersonId
2. 维护生命阶段（根据年龄自动推进）
3. 维护存活状态（接收 DeathRegistered 事件）
4. 维护精度环（接收升格/降格请求）
5. 提供 `IPersonRegistryQueries`：按 Id 查人、按精度环查人

### 时间契约

- `month`：年龄推进、生命阶段检查

### Schema

独立 schema namespace，极稳定，极少迁移。

---

## 各模块持有的人物领域状态

### FamilyCore

```
FamilyClanMembership:
  PersonId personId
  ClanId clanId
  BranchPosition branchPosition  // MainLineHeir / BranchHead / BranchMember / DependentKin / MarriedOut
  PersonId? spouseId
  List<PersonId> childrenIds
  PersonId? fatherId
  PersonId? motherId

FamilyCore 只跟踪**属于或曾经属于本族的人**的亲属关系。
不隶属于任何宗族的人（流民、外族官员、独立匪首等）的亲属关系不在 FamilyCore 中。
如果未来需要全局亲属图谱（例如跨族婚姻网络分析），应由投影层从多个族的 FamilyCore Query 合并产出，不应让 FamilyCore 变成全局亲属总表。

FamilyPersonality:
  PersonId personId
  int ambition       // 0-100
  int prudence       // 0-100
  int loyalty         // 0-100
  int sociability    // 0-100
```

FamilyCore 不拥有：学业、商贸、官职、武力、生计状态。
FamilyCore 不拥有：SocialPosition（这是投影层合并概念）。

### EducationAndExams

```
EducationPersonState:
  PersonId personId
  int literacy              // 该领域拥有识字能力
  int studyProgress
  int stress
  ExamTier currentTier
  int examAttempts
  ExamResult lastResult
  PostExamPath? fallbackPath
  bool hasTutor
  int tutorQuality
```

EducationAndExams 拥有 `literacy` 的权威值和成长规则。

### TradeAndIndustry

```
TradePersonState:
  PersonId personId
  int commercialSense       // 该领域拥有商业能力
  List<ShopId> ownedShops
  int debtLevel
  int profitMargin
```

TradeAndIndustry 拥有 `commercialSense` 的权威值和成长规则。

### OfficeAndCareer

```
OfficePersonState:
  PersonId personId
  InstitutionId? currentPost
  PostRank? rank
  int waitingMonths
  int evaluationPressure
  int patronageSupport
  OfficialStatus status     // Candidate / Attached / Appointed / Dismissed / Retired
```

### ConflictAndForce

```
ForcePersonState:
  PersonId personId
  int martialAbility        // 该领域拥有武力能力
  ForceGroupId? assignedGroup
  int combatExperience
  int injuryLevel
```

ConflictAndForce 拥有 `martialAbility` 的权威值和成长规则。

### PopulationAndHouseholds

```
HouseholdMembership:
  PersonId personId
  HouseholdId householdId
  LivelihoodType livelihood
  int healthResilience      // 该领域拥有体质
  HealthStatus health
  int illnessMonths
  PersonActivity activity   // Idle / Farming / Migrating / Laboring / ...
```

PopulationAndHouseholds 拥有 `healthResilience` 的权威值。
PopulationAndHouseholds 拥有 `HealthStatus` 和 `PersonActivity` 的权威值。
这些字段涵盖所有有家户归属的人，不限于族人。
没有家户归属的人（如远方摘要池中的休眠人物）不在此模块持有状态。

### SocialMemoryAndRelations

```
PersonSocialState:
  PersonId personId
  List<SocialMemoryEntry> memories

DormantStub:
  PersonId personId
  string lastKnownLocation
  string lastKnownRole
  List<MemoryId> activeMemoryIds
  GameDate lastSeen
  bool isEligibleForReemergence
```

### OrderAndBanditry

```
OutlawPersonState:
  PersonId personId
  OutlawBandId? bandId
  OutlawRole role           // Leader / Lieutenant / Member
  int notoriety
```

---

## 能力值所有权规则

能力值归**使用该能力做权威裁决的模块**所有。

| 能力 | 拥有模块 | 原因 |
|---|---|---|
| `literacy` | EducationAndExams | 识字影响学业和考试裁决 |
| `commercialSense` | TradeAndIndustry | 商业能力影响商贸裁决 |
| `martialAbility` | ConflictAndForce | 武力影响冲突裁决 |
| `healthResilience` | PopulationAndHouseholds | 体质影响疾病和生存裁决 |
| `ambition / prudence / loyalty / sociability` | FamilyCore | 性格影响宗族内自主行为 |

### 跨模块读取

其他模块通过 Query 读取能力值，不直接访问。

例：FamilyCore 做婚姻匹配时需要知道某人识字水平
→ 调用 `IEducationAndExamsQueries.GetLiteracy(personId)`
→ 不直接读 EducationAndExams 的状态

### 能力值变化

能力值只由拥有模块修改。跨模块影响通过事件：

```
事件驱动示例:

CampaignAftermathRegistered（WarfareCampaign 发出）
  → ConflictAndForce 听到 → martialAbility 可能提升（战场历练）
  → PopulationAndHouseholds 听到 → healthResilience 可能下降（伤病）

ExamPassed（EducationAndExams 发出）
  → literacy 已经由 EducationAndExams 自己提升了
  → FamilyCore 听到 → 宗族声望调整

MarriageAllianceArranged（FamilyCore 发出）
  → SocialMemoryAndRelations 听到 → 建立恩怨/义务记忆
```

**红线：任何模块不直接修改其他模块拥有的能力值。**

---

## 人物创建流程

### 新建一个人

```
1. 领域模块通过 IPersonRegistryCommands.Register 建立身份:
     registryCommands.Register(context, personId, displayName,
                               birthDate, gender, fidelityRing)
   → PersonRegistry 写入 PersonRecord 并发 PersonCreated
   → PersonId 由调用方通过 KernelIdAllocator.NextPerson 预先分配
   → 默认 fidelityRing 按调用方域（宗族新生 = Local，远方摘要 = Regional）

2. 需要挂状态的模块各自创建领域记录:
   FamilyCore.AddFamilyPersonState(personId, clanId, branchPosition)
   PopulationAndHouseholds.RegisterHouseholdMember(personId, householdId, livelihood)
   // 其他模块按需

3. 如果该人进入玩家视野:
   PersonRegistry.Promote(personId, FidelityRing.Local 或 Core)
   → 各模块按精度环决定填充多少数据
```

### 人物死亡

```
1. 拥有死亡原因的模块在本地裁决死亡后，先通过 IPersonRegistryCommands
   权威写入 PersonRegistry：
     registryCommands.MarkDeceased(context, personId)
   → PersonRegistry 置 isAlive = false, lifeStage = Deceased
   → PersonRegistry 发出 PersonDeceased

2. 同一模块再发出领域死因事件作为下游 flavor/记忆/清理信号:
   PopulationAndHouseholds → DeathByIllness
   ConflictAndForce → DeathByViolence
   FamilyCore → ClanMemberDied (老死/难产等族内可见死因)

3. PersonRegistry 事件监听器保留为兜底路径：若某模块未走 MarkDeceased
   而直接发出死因事件，也会在 HandleEvents 中消费并收敛成 PersonDeceased。
   已处理的人物（!isAlive）会被自然去重，不会重复发射。

4. 各模块各自清理:
   FamilyCore → 触发继承/丧服逻辑
   OfficeAndCareer → 清空任命
   SocialMemoryAndRelations → 恩怨跨代传递
   ConflictAndForce → 清空武力编制
```

**红线**：死亡不得只写领域模块本地镜像；必须经 `IPersonRegistryCommands.MarkDeceased`
进入 PersonRegistry，或退而通过死因事件让 PersonRegistry 消费。本地 `IsAlive = false`
仅作为过渡期的可读镜像，不构成权威。

### 人物升格（Regional → Local → Core）

```
触发条件（来自 SIMULATION_FIDELITY_MODEL.md）:
  - 嫁入玩家关系网
  - 债务/信用关系
  - 仇恨关联
  - 机构重要性
  - 法律/治安事件
  - 热点代表
  - 荐举/庇护
  - 灾难/匪患推入视野

流程:
1. 任意模块检测到升格条件 → 发出 PromotionRequested 事件
2. PersonRegistry 执行升格 → fidelityRing 变更
3. 各模块听到 FidelityRingChanged 事件 → 按新环级填充数据
```

### 人物降格（Core → Local → Regional → Dormant）

```
触发条件:
  - 离开可及范围
  - 不再承载活跃压力
  - 无命令/后果链依赖

流程:
1. 定期检查（月末）→ 无活跃压力的 Core/Local 人物标记候选
2. PersonRegistry 执行降格 → fidelityRing 变更
3. 各模块听到 → 精简数据
4. SocialMemoryAndRelations → 保留 DormantStub

重度降格保留:
  - PersonRecord 保留（永不删除 PersonId）
  - DormantStub 保留记忆存根
  - 可重新升格
```

---

## 投影层合并

壳面需要看到完整人物时，投影层查多个模块 Query：

```
PersonProjection:
  // 从 PersonRegistry
  id, name, age, lifeStage, gender, isAlive, fidelityRing

  // 从 FamilyCore
  clanName, branchPosition, personality

  // 从 PopulationAndHouseholds
  livelihood, health, activity

  // 从 EducationAndExams（如该人有学业状态）
  studyStatus, examTier, literacy

  // 从 TradeAndIndustry（如该人有商贸状态）
  businessSummary, commercialSense

  // 从 OfficeAndCareer（如该人有官场状态）
  officialStatus, postTitle, rank

  // 从 ConflictAndForce（如该人有武力状态）
  martialAbility, forceAssignment

  // 从 SocialMemoryAndRelations
  activeMemories, grudgeSummary

  // 合并计算（投影层产出，非权威状态）
  socialPositionLabel    // "长房次子，候补知县，兼营布铺"
  activityLabel          // "在任，兼顾族务"
```

`socialPositionLabel` 不是任何模块的权威状态，是投影层根据各模块状态拼出来的展示文本。

---

## 与现有文档的关系

| 本文档 | 相关文档 | 状态 |
|---|---|---|
| PersonRegistry 加入 Kernel 层 | `ARCHITECTURE.md` | ✅ 已同步 |
| PersonRegistry 加入权威模块列表 | `ARCHITECTURE.md` | ✅ 已同步 |
| PersonRegistry 加入模块边界 | `MODULE_BOUNDARIES.md` §0 | ✅ 已同步 |
| FamilyCore 移除通用人物字段 | `MODULE_BOUNDARIES.md` §2 | ✅ 已同步 |
| FamilyCore kinship 限定 clan-scoped | `MODULE_BOUNDARIES.md` §2 | ✅ 已同步 |
| PopulationAndHouseholds 补全人物所有权 | `MODULE_BOUNDARIES.md` §3 | ✅ 已同步 |
| 升格/降格执行者明确为 PersonRegistry | `SIMULATION_FIDELITY_MODEL.md` Module landing | ✅ 已同步 |
| PersonRegistry 是 Kernel 基础设施不需 feature pack | `EXTENSIBILITY_MODEL.md` | ✅ 已同步 |
| 新模块只需引用 PersonId，不改已有模块 | `EXTENSIBILITY_MODEL.md` | ✅ 一致 |
| PersonRegistry 是最稳定的静态结构之一 | `STATIC_BACKEND_FIRST.md` | ✅ 一致 |
| LIVING_WORLD_DESIGN §2.2 使用 PersonRegistry + 各模块分布 | `LIVING_WORLD_DESIGN.md` | ✅ 已同步 |
| 死亡事件：各模块报告领域死因，PersonRegistry 合并 | `MODULE_BOUNDARIES.md` §0/§2/§3/§9 | ✅ 已同步 |

---

## 反模式

- 不要让 FamilyCore 持有 SocialPosition 或 PersonActivity
- 不要让任何模块直接修改其他模块拥有的能力值
- 不要在 PersonRegistry 里放领域逻辑
- 不要在权威状态里存投影文本
- 不要删除 PersonId（降格 ≠ 删除）

## 一句话总结

**PersonRegistry 只回答"这个人存在吗、活着吗、多大了"。其他一切归各模块自己管。**
## Current projection-layer landing

`PersonDossiers` are the current shell-facing landing point for this document's
multi-module person projection rule. They may join identity, clan placement,
household livelihood / activity / health, education, clan trade footing, office
career, clan memory, and dormant social-memory stubs through queries only.

This does not create a person master table. Each source fact remains owned by
its module, and the dossier is rebuilt as a runtime-only read model for the
lineage surface / person inspector.
