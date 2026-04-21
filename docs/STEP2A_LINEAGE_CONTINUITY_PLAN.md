# STEP 2-A: Lineage Continuity Plan（家族传承活环计划）

本文承接 `DESIGN_CODE_ALIGNMENT_AUDIT.md` Step 2 靶位分类章节，专攻 Step 2-A（"源头代码已就位 / 不存在，需真实规则 + 人口底料"）。

## 更正审计 🅰️：PersonRegistry Kernel 已建成

审计文档 🅰️ 把 PersonRegistry kernel 列为"不存在"。实证核对后：

- `src/Zongzu.Modules.PersonRegistry/` 已是一等模块，`Phase=Prepare, ExecutionOrder=0`
- `RunMonth` 已在每月开头按 `current - birthDate` 重算 `AgeMonths` 并刷 `LifeStage`
- `IPersonRegistryCommands.Register / MarkDeceased` 已就位；FamilyCore 新生儿已走 `Register`、死亡已走 `MarkDeceased`
- `DeathCauseEventNames.ClanMemberDied / DeathByIllness / DeathByViolence` 已有契约常量；PersonRegistry 已消费这三条并 Emit `PersonDeceased`

**因此 Step 2-A 真正的缺口不在 kernel 本身**，而在：**家族传承活环的六个关键环节，在 headless 10 年沙盘里全断电。**

---

## 家族传承活环现状巡检

### 巡检方法
10 年体检沙盘 (`TenYearSimulationHealthCheckTests`) `Seed=20260421, Months=120`，收盘时：
- `PersonRegistry.total=4, living=4, deceased=0`
- FamilyCore 4 clan 各 1 人，全 Adult；`heirSecurity=55` / `branchTension=100` / `marriage=0` 全 10 年不变

### 七环诊断

| 环节 | 代码落点 | 状态 |
|---|---|---|
| **生（birth）** | `FamilyCoreModule.TryResolveClanBirth` L475 | 🔴 断电：门槛 `MarriageAllianceValue>=55 && ReproductivePressure>=52`，seed 值 0 且沙盘没有上涨通道 |
| **养（infant/child）** | 无 | 🔴 缺失：没有抚育成本、没有夭折风险、child 阶段完全安全 |
| **成（冠礼/成年）** | PersonRegistry 按月刷 LifeStage Youth→Adult | 🟡 静默：LifeStage 变化不触发任何事件、不激活任何压力 |
| **婚（联姻）** | `MarriageAllianceArranged` 存在（10 年发 4 次，consumers=0） | 🟡 孤立：联姻事件无下游消费，也不写 spouse 关系；`MarriageAlliancePressure/Value` 字段有但涨落源不清 |
| **育（heir 指派）** | `clan.HeirPersonId` + `wasHeir` 判断已就位 | 🔴 断电：沙盘里 `HeirPersonId` 始终 null → `ComputeHeirSecurity` 走"无 heir"分支本应返回 18，但 seed 手设 55 绕过 |
| **继（承祧/收养）** | 无 | 🔴 缺失：heir 死了 `HeirPersonId` 清空后不再重算；无过继/收养/旁支递补规则 |
| **老（elder）** | `ElderAgeMonths=55*12` 定义但无消费 | 🔴 静默：进入 elder 不触发 `MourningLoad` 前兆、不加 `CareLoad`、不降劳动 |
| **亡（death）** | `TryResolveClanDeath` L525；悬崖 `DeathAgeMonths=72*12` | 🔴 不活：必须满 72 才死；10 年沙盘 4 个 Adult 全活 |

### 观察：不是"规则没写"，是"压力链死板 + 人口底料不够"

- 老死是**悬崖**（72 必死，71 必活）而非风险带
- seed 每 clan 仅 1 人全 Adult，没有跨代梯队
- 婚/育/继三环**互相锁死**：不婚 → 不育 → 无 heir → heir 死了没人接 → `HeirPersonId` 长期 null
- 文件 `FamilyPersonState.AgeMonths` 是 mirror 字段，与 Registry 的 `BirthDate` 双源头并存（技术债但非本 step 阻塞）

---

## 设计锚点（来自 skill `zongzu-ancient-china`）

### `fertility-demography-infant-mortality`
> 人口不是平滑增长计；应改 **劳动视野 / 照料负担 / 承祧稳度 / 悲伤与恐惧 / 婚期 / 宗族续航**。
> 分开建模：**生育机会 vs 童子成活**、**育龄成人 vs 幼小依赖**、**母体风险 vs 婴儿风险**、**单次生育 vs 持续续航信心**。

### `lineage-inheritance`
> 区分：**户 vs 宗**、**支房 vs 主房**、**礼法合法性 vs 实际占有**、**承祧稳度 vs 财产控制**、**联姻价值 vs 户内劳动**。
> **孀居、再醮、监护、过继、旁支递补是分离的压力**——不要塌成同一条规则。

### `disease-lifespan-death`
> 死亡不是血条见底；要改 **劳动 / 债 / 照料 / 婚期 / 承祧 / 恐惧** 六面。
> 老龄化在能改变风险承受度和户内规划时才有价值。

### `simulation-calibration`
> **年龄带 / 概率用"believable bands"**，不拍确切数。
> 带的例子：`infant_survival_band` / `elder_decline_band` / `heir_security_band`。

### `childhood-generations`
> **童子 / 少年 / 学徒 / 老年 / 承祧视野** 是不同环；不能"成年开始，承祧结束"。

---

## Step 2-A 工作面分解（A0a–A7）

### 第零刀：五条社会化照护链（A0a → A0b → A0c → A0d，独立 commit）

医疗/养生不是单一字段，是 **五条并行的社会化照护链**——横贯 A1 老死、A5 婴儿夭折、A4 婚议、A6 生育。

#### **A0a — 链 ① 家内照料 + 链 ② 郎中药铺**

**新字段**：
- `WorldSettlements.SettlementStateData.HealerAccess` —— band `None / Itinerant / Local / Renowned`
- `FamilyCore.ClanStateData.CareLoad`（0-100，长期照料负担）
- `FamilyCore.ClanStateData.FuneralDebt`（0-100，葬事后拖累）
- `FamilyCore.ClanStateData.RemedyConfidence`（0-100）

**机制**：
- 慢性老人 / 重病婴儿 → `CareLoad` 累积；葬事 → `FuneralDebt` 涨一跳 3-6 月衰
- clan 花 `SupportReserve` "问医" → `RemedyConfidence +`、本人风险权重 **降一档但不归零**
- `HealerAccess = None` 的聚落，花钱也没用

**回路**：
- `CareLoad ≥ 50` 阻塞婚议（A4）、压分房（A7）、降 `ReproductivePressure`
- `FuneralDebt` 高时降 `SupportReserve`、推迟生育（A6）

**skill 铁律**：治疗**只改风险权重，不保证成功**（`medicine-healing-burial`）

#### **A0b — 链 ③ 寺观 / 巫祝 / 民间疗法**

**新字段**：
- `WorldSettlements.SettlementStateData.TempleHealingPresence` —— band
- `SocialMemoryAndRelations` memory kind 扩 `vow_obligation`

**机制（平行通道，不是替代医生）**：
- 信任寺观的家 → 不找郎中 → **延误**（A1/A5 风险权重反涨）**但** `ShamePressure / FearPressure` 下沉（安抚）
- 病愈欠还愿债 → `vow_obligation` 未还会转回 `ShamePressure`
- **玩法矛盾**：信仰救心，不救命

#### **A0c — 链 ④ 官府 / 义仓 / 赈济（疫灾驱动，平时 dormant）**

**新字段**：
- `WorldSettlements.SettlementStateData.GranaryTrust`（0-100）
- `WorldSettlements.SettlementStateData.ReliefReach` —— band

**新契约事件**：
- `EpidemicOutbreak`（WorldSettlements 发）
- `ReliefDelivered` / `ReliefWithheld`（WorldSettlements 发）

**机制**：
- 疫灾时 `GranaryTrust` 决定 clan 是否求赈；`ReliefReach` 决定实到
- 实到 → clan `favor_incurred`（记官府管用）
- 不到 → clan `shame + grudge`，走街谈（补 Step 2-C 的 `StreetTalkSurged` 源头之一）

**skill 铁律**：relief is political as well as humanitarian（`disaster-famine-relief-granaries`）

#### **A0d — 链 ⑤ 宗族救济（挑选性，非普惠）**

**新字段**：
- `FamilyCore.ClanStateData.CharityObligation`（0-100）

**新 memory kind**：
- `SocialMemoryAndRelations.favor_incurred`（被救）
- `SocialMemoryAndRelations.shame_exclusion`（被弃）

**机制（关键：挑选性）**：
- 房支近 + 门望高 + 历史 favor 正 → 被救 → 记 `favor_incurred`
- 房支远 + 素有过节 + 门望低 → 被弃 → 记 `shame_exclusion` + 跨代 `grudge`
- 救济动作降 `SupportReserve`、涨 `prestige`（公信）
- 弃一人降 `CharityObligation`、涨远支 `branchTension`

**20-30 年回响**：memory 跨代累积，未来某支起事、分房、外嫁谈判时权重来自这里——**宗族不是温情，是有成本的秩序**。

**skill 铁律**：charity obligation can turn into debt dependency（`lineage-institutions-corporate-power` 失败态）

---

### 第一刀：让时间轴活起来（A2 → A1 → A3 → A4，独立 commit）

#### **A1 — 老死风险带**
替换 `DeathAgeMonths=72*12` 悬崖为 **年龄带 × 多维护养** 的渐进风险模型。

**维度入口（来自 skill + 已有 Query + A0 新供给）**：
- `age_band`: Infant / Child / Youth / Adult / Elder-Early（55–65）/ Elder-Late（65–80）/ Venerable（80+）
- `clan.SupportReserve`（养护能力）
- `clan.MourningLoad`（已在重负的户更脆弱）
- `clan.CareLoad`（A0 新供给，长期照料拖累）
- `clan.RemedyConfidence`（A0 新供给，愿意求医的信心）
- `settlement.Security`（动乱期病弱更易死）
- `settlement.HealerAccess`（A0 新供给，有无医者）
- 季节（寒冬老人高风险）
- 是否处于战后恢复期

**不做的事**：
- 不引入新随机数发生器——复用 `scope.Context` 已有的 deterministic source
- 不拍"死亡概率=多少%"；用 **"本月该 clan elder 中风险权重最高的那个** 作为候选"——纯确定性选位
- 不加新契约事件——仍发 `ClanMemberDied`

**判定形状**（草图，无实际阈值，留 Step 2-A 实作确认）：
```
foreach elder in clan.LivingPersons where age >= 55:
    risk = AgeBandBaseRisk(age_band)
         + FragilityFromSupport(clan.SupportReserve)
         + FragilityFromMourning(clan.MourningLoad)
         + FragilityFromSettlement(settlement, season)
    accumulate to clan.FragilityLedger[personId]
    when ledger >= threshold → this person dies this month
```
这是**累积压力**而非单月骰子——复合维度模型，没有随机性。

**验证**：
- 10 年沙盘从"0 死亡"变为"可见的代际更替"
- 先死的是最老的人、养护最差的户、最动荡的地
- `PersonDeceased` / `ClanMemberDied` 两条事件真实发射

#### **A2 — Seed 人口跨代化**
改 `SeedM3StressWorld`，每 clan 种 **4–6 人跨代梯队**，不要再只发 1 个 Adult。

**梯队形状（初始态，believable band）**：
- 1× Elder-Early（55–65）—— 真的能走完 A1 老死带
- 1–2× Adult（25–45）—— 工作面主力
- 0–1× Youth（15–22）—— 承祧候选人
- 0–1× Child（5–12）—— dependency wave
- 0–1× Infant（0–2）—— 婴儿死亡链有靶子

**并同时写好**：
- 每 clan 指定初始 `HeirPersonId`（通常是最年长在世成年男性）
- 已婚夫妇的 spouse 关系（为 A4 婚议链预热）
- 初始 `MarriageAllianceValue` 设为一个真实非零基线（clan prestige 派生，不拍数字）

**验证**：
- 开局就有 20+ 人
- 10 年后能看到 Infant → Child、Youth → Adult、Adult → Elder 的自然迁移
- A1 的风险带有足够 elder 可供触发

#### **A3 — Heir 自动指派 / 递补**
每月 Phase 2 末段 FamilyCore 重算 `HeirPersonId`：
- 选在世 + Adult 或 Elder-Early + 男性 + 最年长者（近 primogeniture）
- `HeirPersonId` 变化时发新契约事件 `HeirAppointed` / `HeirSuccessionOccurred`
- heir 死亡后**立即**递补（不等到下月），减少 HeirSecurity 的人为凹陷

**新契约事件**（`FamilyCoreEventNames` 追加）：
```csharp
public const string HeirAppointed = "HeirAppointed";
public const string HeirSuccessionOccurred = "HeirSuccessionOccurred";
```

**维度入口保留给 Step 2-A 后续**：
- 过继（adopting from a branch）—— skill 明言是单独压力，本 step 不做，留 A7+
- 旁支递补（collateral succession）—— 当 clan 无合格男嗣时才触发

**验证**：
- A2 开局每 clan 有 heir
- A1 打死 heir 后，下月有新 heir
- `HeirAppointed` / `HeirSuccessionOccurred` 发射并被 SocialMemory / NarrativeProjection 消费

#### **A4 — 婚议链通电**
让 `MarriageAllianceArranged` 有真实消费端 + 真实驱动。

**驱动**（在 FamilyCore 已有字段上激活）：
- `MarriageAlliancePressure` 月节拍增长维度：clan 有适婚 Youth（15–22）+ 邻近 clan 有适龄异性 + 门望差距在带内
- 达阈值 → Emit `MarriageAllianceArranged`，写双向 spouse 关系

**消费**（在 A4 补齐）：
- FamilyCore 消费自己的 `MarriageAllianceArranged`：把 `MarriageAllianceValue` 涨一跳、开启生育窗口
- SocialMemory 消费：记 `favor` + 跨 clan 关系网
- Population 消费（Step 2-A 后续）：把新媳妇加入新户

**本 step 不做**：
- 不做"联姻政治操作"——那是玩家命令层
- 不做聘礼 / 债务——Property/Contract 模块未建

**验证**：
- 10 年沙盘联姻事件从 4 次 / 0 消费 → 真实往上走、下游 SocialMemory narrative 有婚议痕迹

---

### 第二刀：生育—夭折配对（A5 + A6，独立 commit）

#### **A5 — 婴幼儿夭折**
Infant（<2）/ Child（<12）阶段引入 **低基线 + 压力加权** 的 `DeathByIllness` 通道。

**维度入口**：
- `age_band`: Infant 最高 / Child 次之 / Youth 接近 0
- 季节（春瘟、冬寒）
- settlement 疫疠度（Order / PublicLife）
- clan.SupportReserve（穷户夭折率高）
- settlement.HealerAccess（A0 新供给）
- clan.RemedyConfidence（A0 新供给）
- clan.CareLoad（A0 新供给，照料已重时新婴儿最脆弱）

**设计锚点**（`fertility-demography-infant-mortality`）：
> **分开建模母体风险 vs 婴儿风险；婴儿死亡不应塌成单一 sadness 变量，应产出 fear/debt/ritual/inheritance anxiety。**

故 A5 婴儿夭折除发 `DeathByIllness` 外，还要触发：
- `clan.MourningLoad` 加重（已有）
- `clan.ReproductivePressure` 加重（再育焦虑）
- SocialMemory 添 child_loss_grief memory（新 memory kind）

**新契约事件**：无（`DeathByIllness` 已在契约）

**验证**：
- 10 年沙盘 Infant/Child 有真实减员
- `DeathByIllness` 从 0 → 真实出现
- SocialMemory child_loss 记忆出现

#### **A6 — 生育链解卡**
审计并放宽 `TryResolveClanBirth` 门槛。当前规则：`MarriageAllianceValue >= 55 && ReproductivePressure >= 52 && MourningLoad < 18 && AdultCount > 0 && InfantCount == 0`。

**问题**：四重合取 + 两个 seed=0 的字段 → 永不触发。

**改造方向**：
- 不拍新数字；改为**"已婚成年夫妇 × 无近 12 月婴儿 × SupportReserve 可撑"** 的新形状
- 依赖 A4 完成后 `MarriageAllianceValue` 已有真实基线
- 依赖 A2 的 spouse 关系已种好

**验证**：
- 10 年沙盘首次出现真实新生儿
- 婴儿数随 A5 夭折真实波动

---

### 第三刀：成年仪式 + 分房候选（A7，独立 commit）

#### **A7 — CameOfAge + 分房前置**
- LifeStage Youth → Adult 触发 `CameOfAge`（新契约事件）
- 成年男性 > 2 且无分房议 → 按月慢慢堆 `SeparationPressure`
- 为 Step 2-B 玩家命令 `PermitBranchSeparation` 铺好真实触发条件

**新契约事件**：
```csharp
public const string CameOfAge = "CameOfAge";
```

**消费端（本 step 最低）**：
- FamilyCore 自己消费 CameOfAge 以堆 SeparationPressure / 开启婚议候选
- SocialMemory 记一个轻量 narrative 条目

**验证**：
- A2 seed 的 Youth 在 10 年内普遍成年
- `SeparationPressure` 在多子家族真实上涨到 ≥55
- 沙盘虽然还打不出 `BranchSeparationApproved`（Step 2-B），但前置信号齐了

---

## 推进与 commit 顺序

| 阶段 | 代号 | commit 粒度 | 验证点 |
|---|---|---|---|
| 1 | A2 | Seed 跨代化 + seed 五链初值 | 开局 20+ 人，跨代分布；五链字段就位 |
| 2 | A0a | 链①家内照料 + 链②郎中药铺 | `CareLoad`/`FuneralDebt`/`RemedyConfidence`/`HealerAccess` 真实涨落 |
| 3 | A0b | 链③寺观民间（延误/安抚平行通道） | `vow_obligation` memory 出现；寺观派别家延误权重真实 |
| 4 | A0c | 链④官仓赈济（疫灾驱动） | `EpidemicOutbreak` / `ReliefDelivered` / `ReliefWithheld` 出现 |
| 5 | A0d | 链⑤宗族救济（挑选性） | `favor_incurred` / `shame_exclusion` memory 跨代累积 |
| 6 | A1 | 老死风险带（吃 A0a–d 全部维度） | 10 年沙盘见死亡发生，风险依维度加权 |
| 7 | A3 | Heir 自动指派 / 递补 | `HeirSecurity` 随 heir 生死真实波动 |
| 8 | A4 | 婚议链通电（吃 CareLoad 阻塞） | 婚事有 SocialMemory 痕迹 |
| 9 | A5 | 婴幼儿夭折（吃 A0a–d） | Infant/Child 死亡率带 |
| 10 | A6 | 生育链解卡 | 新生儿真实出现 |
| 11 | A7 | CameOfAge + 分房前置 | SeparationPressure 真涨 |

每一步都要：
1. 改代码
2. build + full test 通过（不能破坏任何既有 state 测试）
3. 重跑 `TenYearSimulationHealthCheckTests` 观察报告的**行为变化**
4. 独立 commit

## 反模式清单（开工前自检）

- ❌ 拍数字（"70 岁死亡率 30%"）—— 违 `simulation-calibration`
- ❌ 把老死塌成一个悬崖阈值 —— 违 `disease-lifespan-death`
- ❌ 让 heir 死等于 clan 终结 —— 违 `lineage-inheritance`（忽略过继 / 旁支 / 收养）
- ❌ 把婴儿死亡塌成"sadness -= 1" —— 违 `fertility-demography-infant-mortality`
- ❌ 在 A1–A4 阶段试图做过继 / 玩家操作 —— 超出本 step 范围
- ❌ 引入新随机数发生器 —— 必须用 `scope.Context` 已有 deterministic source
- ❌ 在 FamilyCoreModule 之外的模块改 clan state —— 违模块边界
- ❌ 把治疗做成"付钱 → 治好"的交易 —— 违 `medicine-healing-burial`（治疗不保证成功）
- ❌ 让宗族救济变成自动普惠 —— 违 `lineage-institutions-corporate-power`（救济是挑选性的）
- ❌ 让寺观变成"第二家医院" —— 违 `religion-temples-ritual-brokerage`（要保留延误/安抚张力）
- ❌ 让 `HealerAccess` / `GranaryTrust` / `TempleHealingPresence` 变成同质 0-100 条 —— 违 `simulation-calibration`（每 band 有不同语义）

## 跨 step 衔接

- **A1 做完 → Step 2-A 死亡链全部通电** → `HeirSecurityWeakened` 自然触发（Step 1c 体检里的 dormant 之一）
- **A3 做完 → Step 2-A 承祧链通电** → `HeirAppointed` 新契约（不在 Step 1c dormant 清单）
- **A7 做完 → Step 2-B 玩家分房的触发前置齐备** → 等 shell UI 补完命令通道

---

状态：**计划文档**，不含任何代码变更。动手前需用户确认。
