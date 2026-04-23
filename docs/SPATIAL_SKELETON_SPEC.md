# SPATIAL_SKELETON_SPEC

本文档定义 Zongzu 空间骨骼的静态结构与契约。

它是 `LIVING_WORLD_DESIGN.md` Phase 1c 的实施规格。

它遵循 `STATIC_BACKEND_FIRST.md`：**先固定状态形状和契约，再填规则密度**。

它遵循 `SIMULATION_FIDELITY_MODEL.md`：**精度不均分**。

它遵循 `MODULE_BOUNDARIES §1`：所有权归 `WorldSettlements`。

它遵循 `MODERN_GAME_ENGINEERING_STANDARDS.md` §3 System Standards 和 §4 Unity Presentation Standards。

它是对 skill pack 全集（50+ references）的 Zongzu 项目化收敛。主要依据：

**空间与地理**
- `zongzu-ancient-china/references/china-ancient-sandbox-structure.md`
- `zongzu-ancient-china/references/china-map-prefecture-county-routes.md`
- `zongzu-ancient-china/references/ancient-sandbox.md`

**区域对照**
- `zongzu-ancient-china/references/jiangnan-water-network-county.md`
- `zongzu-ancient-china/references/north-china-road-county.md`
- `zongzu-ancient-china/references/southwest-mountain-borderland.md`
- `zongzu-ancient-china/references/border-garrison-fort-belt.md`

**节律 / 时间**
- `zongzu-ancient-china/references/simulation-calibration.md`
- `zongzu-ancient-china/references/agrarian-calendar-waterworks-hazards.md`
- `zongzu-ancient-china/references/festival-folkways-entertainment.md`

**基础设施与物流**
- `zongzu-ancient-china/references/public-works-granaries-canal-transport.md`
- `zongzu-ancient-china/references/household-tax-corvee.md`
- `zongzu-ancient-china/references/disaster-famine-relief-granaries.md`

**信息与公共面**
- `zongzu-ancient-china/references/information-messaging-documents.md`
- `zongzu-ancient-china/references/public-opinion-reputation-public-spaces.md`
- `zongzu-ancient-china/references/religion-temples-ritual-brokerage.md`
- `zongzu-ancient-china/references/local-culture-customs-regional-identity.md`

**皇权与地方摩擦**
- `zongzu-ancient-china/references/imperial-local-bargaining-governance-friction.md`
- `zongzu-ancient-china/references/imperial-power-bureaucracy-and-subjects.md`
- `zongzu-ancient-china/references/imperial-sovereignty-legitimacy-succession.md`
- `zongzu-ancient-china/references/court-eunuchs-outer-kin.md`

**隐性组织与灰色地带**
- `zongzu-ancient-china/references/secret-societies-brotherhoods.md`
- `zongzu-ancient-china/references/gentry-local-magnates.md`
- `zongzu-ancient-china/references/local-order-militia-banditry.md`
- `zongzu-ancient-china/references/lineage-private-force-retainers-conflict.md`

**冲突尺度**
- `zongzu-ancient-china/references/conflict-scale-ladder.md`
- `zongzu-ancient-china/references/small-clash-vs-major-campaign.md`
- `zongzu-ancient-china/references/force-families-differentiation.md`

**项目级 shell / design**
- `zongzu-game-design/references/rules-driven-living-world.md`
- `zongzu-game-design/references/pressure-chains-and-causality.md`
- `zongzu-game-design/references/vertical-slice-and-mvp-shaping.md`
- `zongzu-game-design/references/command-resolution-and-bounded-leverage.md`
- `zongzu-ui-shell/references/shell-principles.md`
- `zongzu-ui-shell/references/surface-grammar.md`
- `zongzu-ui-shell/references/density-and-focus.md`

---

## 第零章：核心决策

### 决策 A：节点分类 = **功能语义**，不是地理几何

节点不是"地图上的点"。节点是**社会压力的锚点**。每一类节点都有自己的语义职责，独立承担一种压力 / 报告 / 队列 / 冲突 / 公共信号。

### 决策 B：路线分类 = **社会功能**，不是路段

路线不是"从 A 到 B 的边"。路线是**社会流动的通道**：粮道承粮，税路承税，驿路承文，商路承货，举子路承举子，武路承兵。同一段地理走廊上可以同时叠加多条功能路线。

### 决策 C：水路是独立拓扑，不是路的一种

水路 / 陆路 是**两套并列拓扑**，在节点处交接（渡口 / 埠 / 闸 / 港是水陆交接节点）。

### 决策 D：季节 = **带**，不是枚举

季节不是 Spring/Summer/Autumn/Winter 四值或 12 月值。季节是一组**压力带**：农历节气 + 市集节律 + 漕运窗 + 徭役窗 + 洪险窗 + 信息节律。每个带独立推进。

### 决策 E：叠加层 (overlay) = **Query 面**，不是 state 面

tax_reach / grain_movement / bandit_risk / administrative_delay 这些叠加不是 `WorldSettlementsState` 的列，而是 **跨模块 query 合并产出**。`WorldSettlements` 只拥有几何 + 节点分类 + 路线分类 + 季节带；叠加由 `NarrativeProjection` / `PresentationReadModelBuilder` 合多模块 Query 合成。

### 决策 F：案头读序是契约，不是 UI 细节

skill pack 明示案头读序：**节点压力 → 路线传导 → 户/房/官牵连 → 公共可见面 → 可用干预**。这个读序会落入 `NarrativeProjection` 和 hall-docket 的 ordering 契约。

### 决策 G：骨骼必须"活"

空间骨骼不是**可读的 state**，是**会脉动的 state**。每一个字段都必须有推进函数（第十八章），每一个节点都必须能发出事件（第二十一章），每一个可见面都必须让隐压力显形（第二十章），整套系统必须通过活世界自检（第二十二章）。**不满足这条的 state 字段不该加入骨骼。**

### 决策 H：可见性与合法性是一等公民

skill pack 明示"not all organization is visible on the state map"（`secret-societies-brotherhoods.md`），也明示"tolerated gray zones often produce better gameplay than permanent total suppression"（`local-order-militia-banditry.md`）。

空间骨骼**不假设**所有节点和路线都是官府视角可见、合法的：

- **节点**有 `Visibility`（`StateVisible` / `LocalKnown` / `Covert`）
- **路线**有 `Legitimacy`（`Official` / `Tolerated` / `GrayZone` / `Illicit`）

同一条地理走廊上可同时存在"正当商路"和"私盐走私廊"（决策 B 的叠加在这里再延伸一层）。

### 决策 I：世界有三轴节律，不是一轴

活世界的节律不是只有"农历节气"：

1. **自然轴**（`AgrarianPhase` / `CanalWindow` / `FloodRisk`）—— 农耕与气候
2. **政府轴**（`CorveeWindow` / `MessageDelayBand` / `Route.ComplianceMode`）—— 县治与下达链
3. **皇权轴**（`ImperialBand.MourningInterruption` / `AmnestyWave` / `SuccessionUncertainty`）—— 朝廷节律

三轴**独立推进**，但**相互影响**（如国丧中止徭役、大赦软化合规模式）。

### 决策 J：案头读序双层

skill pack UI 端（`density-and-focus.md`）的 "current locus → immediate action → consequence context → background context" 与 skill pack 历史端（`china-ancient-sandbox-structure.md`）的 "节点压力 → 路线传导 → 户/房/官 → 公共面 → 干预" **不是两条竞争读序**，而是**外壳 × 内核**：

- **外壳（UI 读序）**：locus → action → consequence → background
- **内核（溯因链）**：进入 consequence context 后，按 **节点压力 → 路线传导 → 户房官 → 公共面** 展开

见第六章。

---

## 第一章：节点分类

### 1.1 `SettlementNodeKind` 枚举

基于 `china-ancient-sandbox-structure.md` "Recommended Node Grammar"。在 Zongzu 项目里收敛为：

```csharp
public enum SettlementNodeKind
{
    // 行政 / 官府
    PrefectureSeat = 10,        // 州府治所：州衙、府衙、上一级文移源头
    CountySeat = 20,            // 县治：县衙、科考报名、牢、官仓
    YamenPost = 30,             // 分司、巡检司、税课局等分设衙门
    RelayPost = 35,             // 驿站：北地旱路 / 文移中转（north-china-road-county）

    // 聚居
    MarketTown = 40,            // 镇：常市、客栈、牙行、夜话
    WalledTown = 45,            // 堡：有城垣的中型聚落，边郡常见
    Village = 50,               // 乡村聚落
    EstateCluster = 55,         // 庄园 / 庄田聚落（大族田庄）
    FrontierCamp = 58,          // 边地新屯 / 移民聚落（frontier-migration-settlement）

    // 宗族 / 教化
    LineageHall = 60,           // 祠堂：宗族议事、族谱、调解
    Academy = 65,               // 书院 / 学宫
    VillageSchool = 66,         // 村学 / 私塾

    // 宗教
    Temple = 70,                // 佛寺 / 道观 / 祠庙
    ShrineCourt = 71,           // 社稷、乡贤、神祠等小型祭祀点
    HillShrine = 72,            // 山祠：山地 / 偏远祭祀（southwest-mountain-borderland）

    // 经济 / 物流
    Granary = 80,               // 常平仓 / 义仓 / 官仓
    Depot = 81,                 // 转运仓 / 军需仓
    WellPost = 82,              // 井台 / 水站（north-china-road-county 北地旱县核心）
    Ferry = 85,                 // 渡口（水陆交接）
    Wharf = 86,                 // 埠 / 码头 / 货运栈
    CanalJunction = 87,         // 闸 / 船闸 / 漕运节点
    Bridge = 88,                // 桥（陆路过水）
    Ford = 89,                  // 浅滩 / 徒涉

    // 军事
    Garrison = 90,              // 戍 / 营 / 防
    Pass = 95,                  // 关隘 / 山关
    BorderWatch = 96,           // 墩台 / 烽堠

    // 隐性节点（决策 H：不是所有节点都是官府视角可见）
    CovertMeetPoint = 200,      // 香堂 / 盟会点 / 私议处（secret-societies-brotherhoods）
    SmugglingCache = 210,       // 私藏 / 走私窝点
}
```

### 1.1b 节点可见性（决策 H）

每个节点都带 `Visibility`：

```csharp
public enum NodeVisibility
{
    StateVisible = 1,   // 官府登记在案：县衙、仓、驿、关、书院、寺等
    LocalKnown = 2,     // 本地知情但官府不直接干预：祠堂、钱庄、大族庄园、庙外街市
    Covert = 3,         // 隐性：香堂、私盐窝、逃丁聚点
}
```

**不是节点自动决定**：`LineageHall` 在强宗族县可能 `LocalKnown`（县衙知但不插手），在新移民县可能 `StateVisible`（还未形成灰色默契）；`MarketTown` 中大多是 `StateVisible`，但其中的**局部口岸**可能 `LocalKnown`。Visibility 是节点**实例**的属性，不是 NodeKind 的属性。

`CovertMeetPoint` / `SmugglingCache` 是唯二**默认 Covert** 的 NodeKind（它们的语义就是"不可见"）。

### 1.2 每类节点的"earn its place"语义

skill pack 铁律："**A node earns its place by owning a pressure, a report, a queue, a conflict, or a public signal.**"

在 Zongzu 里，这意味着每类节点必须至少**承担一项**下列义务之一，否则不应加入枚举：

| 节点类 | 拥有的压力 / 队列 / 信号 | 默认 Visibility |
|---|---|---|
| `PrefectureSeat` | 上一级文移源头、考试上送、税催上压 | StateVisible |
| `CountySeat` | 县衙案牍积压、刑讼、官仓开关 | StateVisible |
| `YamenPost` | 分司差发、巡检报捕 | StateVisible |
| `RelayPost` | 驿丁、文移中转、差马 | StateVisible |
| `MarketTown` | 市集节律、客栈人流、牙行消息 | StateVisible |
| `Village` | 徭役征派、收成、佃户逃亡 | StateVisible |
| `EstateCluster` | 田租收取、庄客依附、大族显形 | LocalKnown（默认软化） |
| `FrontierCamp` | 屯垦、新移民摩擦、不稳 | StateVisible |
| `LineageHall` | 宗族议事、族人调解、祭祀 | LocalKnown |
| `Academy` | 科场流、师徒网、士议 | StateVisible |
| `Temple` | 节庆、布施、避乱、谣传 | StateVisible |
| `HillShrine` | 偏远香火、山地出入标记 | LocalKnown |
| `Granary` | 粮仓余缺、开仓赈济、盗劫目标 | StateVisible |
| `WellPost` | 井水、旱县生命线、集会点 | StateVisible |
| `Ferry` | 过渡队列、水陆信息交接、匪伏风险 | StateVisible |
| `Wharf` | 漕船装卸、埠头行业、脚夫劳资 | StateVisible |
| `CanalJunction` | 漕运窗口、闸工徭役、洪险 | StateVisible |
| `Pass` | 关禁、查缉、军事调度 | StateVisible |
| `BorderWatch` | 烽讯、边情、骚扰 | StateVisible |
| `CovertMeetPoint` | 香堂议事、盟会、私相联络 | Covert |
| `SmugglingCache` | 私盐、禁货、亡命藏匿 | Covert |

这是 **MVP 切片的最小集**（第一批实现）。其余节点（`PrefectureSeat`、`Depot`、`RelayPost`、`WellPost`、`HillShrine`、`FrontierCamp` 等）在后续 phase 打开；`CovertMeetPoint` / `SmugglingCache` 接口登记在 Phase 1c（能在 state 里存），实际使用在 Phase 4+ 与 `SocialMemoryAndRelations` / `OrderAndBanditry` 合作。

### 1.3 `SettlementTier` 保留

现有 `SettlementTier` 枚举（已在代码里）继续保留作为**行政等级**标签。`SettlementNodeKind` 是**功能语义**标签。两者正交：一个 `CountySeat` 节点的 `Tier` 通常是 "county"，但一个大型 `MarketTown` 也可以 `Tier="county-adjacent"`。

---

## 第二章：路线分类

### 2.1 `RouteKind` 枚举（社会功能路线）

基于 `china-ancient-sandbox-structure.md` "Recommended Route Grammar"，加 `china-map-prefecture-county-routes.md` 水陆区分：

```csharp
public enum RouteKind
{
    // 正当社会功能（skill pack 铁律：路线按功能分类）
    GrainRoute = 10,             // 粮道：漕粮 / 县运 / 赈粮
    TaxRoute = 11,               // 税路：银两、绢布、实物上解
    PetitionRoute = 12,          // 呈文路：鸣冤、上告、民诉
    OfficialDispatchRoute = 13,  // 文移路：牒、檄、榜示
    ExamTravelRoute = 14,        // 举子路：赴考、迎送
    MarketRoute = 15,            // 商路：牙行、客商、货运
    MilitaryMoveRoute = 16,      // 武路：调兵、粮饷军运
    EscortRoute = 17,            // 护运路：押解、保镖、赋押
    CorveeRoute = 18,            // 徭役调发路：征夫去堤、闸、官工 (household-tax-corvee)

    // 灰色 / 隐性功能（决策 H）
    SmugglingCorridor = 200,     // 走私廊：私盐、私茶、禁货
    FugitivePath = 210,          // 逃丁路：逃徭、逃债、避刑
}
```

### 2.2 `RouteMedium` 枚举（水陆拓扑，决策 C）

```csharp
public enum RouteMedium
{
    LandRoad = 1,       // 官道、驿路、县道、山径
    WaterRiver = 2,     // 河运
    WaterCanal = 3,     // 漕渠、人工运河
    MountainPath = 4,   // 山径、坡道（陆路子类，但季节感弱）
    FerryLink = 5,      // 渡口跨水链接（节点级，不是走廊级）
    BridgeCrossing = 6, // 桥跨水链接
    PassApproach = 7,   // 关隘接近段
    CartCorridor = 8,   // 北地车马廊（north-china-road-county：cart_friction）
    RidgeTrail = 9,     // 山脊路（southwest-mountain-borderland：ridge_route_risk）
}
```

**关键规则**：一段从 A 到 B 的物理走廊可以承载多条 `RouteKind`（粮道 + 商路 + 举子路同走一条官道），但每条 route 记录只属于一个 `RouteMedium`。**同一走廊上的正当路线与走私廊视为不同 route 记录**，各自独立 `RouteState`。

### 2.3 `RouteLegitimacy` 枚举（决策 H）

```csharp
public enum RouteLegitimacy
{
    Official = 1,    // 官方认可：粮道、文移路、驿路
    Tolerated = 2,   // 官方默许：大多数商路、护运路
    GrayZone = 3,    // 灰色地带：小规模偷运、逃丁走小径
    Illicit = 4,     // 明令禁止：私盐廊、禁货走私
}
```

### 2.4 `ComplianceMode` 枚举（决策 I 政府轴）

基于 `imperial-local-bargaining-governance-friction.md`。描述**这条下达链**的实际到达质量：

```csharp
public enum ComplianceMode
{
    StrictEnforcement = 1,   // 严格执行：新官到任、朝廷亲巡、大案压境
    PaperCompliance = 2,     // 纸面合规：默认态，公文有执行不彻底
    LocalBuffering = 3,      // 本地软化：士绅 / 宗族 / 牙行中介消解部分
    BrokerCapture = 4,       // 书吏捕获：实际执行已被衙役 / 牙行 / 族权挟持
}
```

仅对"下达型"`RouteKind` 有意义（`OfficialDispatchRoute` / `TaxRoute` / `CorveeRoute` / `MilitaryMoveRoute`）。其他 RouteKind 的 `ComplianceMode` 字段存在但**不读**（或设为 `PaperCompliance` 占位）。

### 2.5 `RouteState` 字段

```csharp
public sealed class RouteState
{
    public RouteId Id;
    public RouteKind Kind;                 // 社会功能
    public RouteMedium Medium;             // 水陆拓扑
    public RouteLegitimacy Legitimacy;     // 合法性（决策 H）
    public ComplianceMode ComplianceMode;  // 下达质量（决策 I；仅下达型路线有意义）
    public SettlementId Origin;
    public SettlementId Destination;
    public List<SettlementId> Waypoints = new();  // 途经节点
    public int TravelDaysBand;             // 见第四章：calibration 带
    public int Capacity;                   // 0-100：通行能力 / 运力
    public int Reliability;                // 0-100：可靠度（淤、塌、盗频）
    public int SeasonalVulnerability;      // 0-100：对季节的敏感度
    public string CurrentConstraintLabel = string.Empty; // 当前受阻原因
}
```

不在 `RouteState` 里的：
- `TaxReach` / `GrainMovement` / `BanditRisk` —— 决策 E：这些是 query 叠加，不是 state 字段。
- 实时 traffic 量 —— 由 `TradeAndIndustry` / `PopulationAndHouseholds` 在各自 state 里持有。

### 2.6 节点与路线的交接

`Ferry` / `Wharf` / `Bridge` / `Ford` / `CanalJunction` / `Pass` 是**水陆交接节点**。一条 `RouteMedium=LandRoad` 的 route 如果中途要过水，必须在 Waypoints 里经过一个交接节点，否则视为不可通。

这条约束在 Phase 1c 末端加**一条校验规则**（`WorldSettlementsValidator.ValidateRouteConnectivity`），不在 state 层强制。

**灰色路线例外**：`SmugglingCorridor` / `FugitivePath` 可以**不经过** `Ferry` / `Bridge`（私渡、趟水、夜潜）。Validator 对 `Legitimacy >= GrayZone` 的路线跳过交接检查。

---

## 第三章：季节带模型（决策 D）

### 3.1 `SeasonBand` 容器（WorldSettlements 拥有）

季节不是单一枚举值，而是**一组并行带**：

```csharp
public sealed class SeasonBand
{
    public GameDate AsOf;

    // 农事带（基于 agrarian-calendar-waterworks-hazards.md）
    public AgrarianPhase AgrarianPhase;           // Sowing / Transplant / Tending / Harvest / Slack
    public int LaborPinch;                         // 0-100：农忙度（决定徭役窗 / 劳动力可借度）
    public int HarvestWindowProgress;              // 0-100：秋收进度

    // 水控带（基于 public-works-granaries-canal-transport.md）
    public int WaterControlConfidence;             // 0-100：水利信心（堤、闸、渠修缮状态）
    public int EmbankmentStrain;                   // 0-100：堤压力
    public int FloodRisk;                          // 0-100：洪险

    // 漕运带（基于 jiangnan-water-network-county.md）
    public CanalWindow CanalWindow;                // Open / Limited / Closed（冬闭 / 春汛）

    // 市集带
    public int MarketCadencePulse;                 // 0-100：本旬市集热度

    // 徭役带（基于 household-tax-corvee.md）
    public CorveeWindow CorveeWindow;              // Quiet / Pressed / Emergency

    // 信息带（基于 information-messaging-documents.md）
    public int MessageDelayBand;                   // 0-100：本旬文移延迟

    // 皇权带（决策 I 第三轴，基于 imperial-sovereignty-legitimacy-succession.md）
    public ImperialBand Imperial = new();
}

public sealed class ImperialBand
{
    public int MourningInterruption;     // 0-100：国丧 / 君丧中止程度（抑市、停工、停宴）
    public int AmnestyWave;              // 0-100：大赦波（登基 / 立储 / 灾异后）
    public int SuccessionUncertainty;    // 0-100：储位不明 / 夺嫡 / 太子废立后的观望
    public int MandateConfidence;        // 0-100：天命可信度（长期变量，灾异、战乱下滑）
    public int CourtTimeDisruption;      // 0-100：朝廷节奏被打断（权相更替、阉祸、废立）
}

public enum AgrarianPhase { Slack, Sowing, Transplant, Tending, Harvest }
public enum CanalWindow { Closed, Limited, Open }
public enum CorveeWindow { Quiet, Pressed, Emergency }
```

**三轴交互规则**（决策 I）：

- `ImperialBand.MourningInterruption >= 60` 时，**强制** `CorveeWindow = Quiet`（国丧停役），即使农时到 Slack
- `ImperialBand.AmnestyWave >= 50` 时，**下达路线** `ComplianceMode` 至少降一级（`StrictEnforcement → PaperCompliance`；`PaperCompliance → LocalBuffering`）
- `ImperialBand.SuccessionUncertainty >= 70` 时，`MessageDelayBand` 至少翻倍（朝廷停摆，公文积压）

### 3.2 谁推进季节带？

- **`WorldSettlements` 在 month cadence** 推进 `AgrarianPhase` / `HarvestWindowProgress` / `CanalWindow` / `CorveeWindow` / `Imperial.MourningInterruption` / `Imperial.AmnestyWave` / `Imperial.SuccessionUncertainty` 衰减 —— 它们是纯历法 / 政策节奏函数。
- **`WorldSettlements` 在 xun cadence** 推进 `LaborPinch` / `MarketCadencePulse` / `MessageDelayBand` —— 它们是旬级脉动。
- `EmbankmentStrain` / `FloodRisk` / `WaterControlConfidence` —— 由 `WorldSettlements` 持有，但在 xun 读 `PopulationAndHouseholds` 的徭役执行情况后调整（读 query，不跨写）。
- `Imperial.MandateConfidence` / `Imperial.CourtTimeDisruption` —— **不由 `WorldSettlements` 主动推进**。它们通过订阅未来的 `WorldEvents` 模块（Phase 2+）的皇权事件（登基 / 驾崩 / 大赦 / 废立）被**外部驱动**。Phase 1c 只把字段和默认值放到位。

### 3.3 季节带区域分化

skill pack 明示"Regional packs should change how waterworks feel: canals and dikes in Jiangnan, wells and dry-road strain in north-China counties"（`north-china-road-county.md` / `southwest-mountain-borderland.md` / `border-garrison-fort-belt.md`）。

Phase 1c 加 **`SettlementEcoZone` 枚举**（决策 H/I 配套）：

```csharp
public enum SettlementEcoZone
{
    JiangnanWaterNetwork = 1,   // 水网、漕渠、渡埠
    NorthChinaDryRoad = 2,      // 旱路、井水、冬寒、车马
    MountainBorderland = 3,     // 山脊路、谷地市、混合祠
    BorderGarrisonBelt = 4,     // 堡线、驿站、烽堠
}
```

县治 state 上标一个 `EcoZone`，影响 `SeasonBand` 推进的**系数**（不影响字段形状）：

- `JiangnanWaterNetwork`：`FloodRisk` 汛季系数 × 1.4，`CanalWindow` 春开秋闭窗口清晰
- `NorthChinaDryRoad`：`FloodRisk` 系数 × 0.4，但新增"冬季冰封"影响 `Route.Reliability`（见 §18.5）；`CanalWindow` 永远 `Closed`
- `MountainBorderland`：`FloodRisk` 系数 × 0.6（山洪骤发），`Route.SeasonalVulnerability` 因 ridge/pass 较高
- `BorderGarrisonBelt`：`MessageDelayBand` 对 `Imperial.SuccessionUncertainty` 敏感度 × 1.5（边郡消息更易断绝）

**Phase 1c 的兰溪种子** = `JiangnanWaterNetwork`。其他三类 EcoZone 在第十二章以"朔州"对照种子登记接口，但**不实装**完整世界。

---

## 第四章：时间校准带（决策：带优先，不定死数）

基于 `simulation-calibration.md`。

### 4.1 `TravelDaysBand`（路线时间）

不是具体天数，而是**命名带**：

| 值 | 语义 | 大致范围（供叙事层翻译用） |
|---|---|---|
| 0 | `SameDay` | 同聚落、邻村，数时至当日 |
| 1 | `Short` | 县境内，1–3 日 |
| 2 | `Medium` | 县到邻县、县到府，3–7 日 |
| 3 | `Long` | 府到邻府，7–15 日 |
| 4 | `Regional` | 跨路跨道，15–40 日 |
| 5 | `Extreme` | 远路、季候封闭、灾毁，不定期 |

### 4.2 `MessageDelayBand`

同上结构，但**官文** / **私信** / **谣言** 各自一条：

- 官文带（有印有封，慢但有威信）
- 私信带（快但不认证）
- 谣言带（最快但脏）

MVP 只落官文带，`SeasonBand.MessageDelayBand` 即官文带；私信 / 谣言在 Phase 2+ 和 `PublicLifeAndRumor` 合作时加。

### 4.3 `MusterDelayBand` / `GranaryBufferBand`

属于 `ConflictAndForce` / `PopulationAndHouseholds` 各自的 state，但使用**同一套带编号 0-5**，保证跨模块叙事层翻译一致。

**这一条是契约**：带编号是**跨模块共享约定**，定义在 `Zongzu.Contracts/CalibrationBands.cs`。

---

## 第五章：叠加层 (overlay) 清单（决策 E：query 面）

叠加**不进 state**，由 `PresentationReadModelBuilder` 或 `NarrativeProjection` 读多模块 query 合出。

MVP 叠加清单（第一批）：

| 叠加名 | 数据来源 | 消费者 |
|---|---|---|
| `TaxReach` | WorldSettlements.Routes + PopulationAndHouseholds.Households + OfficeAndCareer | Desk sandbox |
| `GrainMovement` | WorldSettlements.Routes(GrainRoute) + SeasonBand.CanalWindow + TradeAndIndustry | Desk sandbox |
| `BanditRisk` | OrderAndBanditry + WorldSettlements.Routes.Reliability + SeasonBand.LaborPinch | Desk sandbox + notice tray |
| `FloodExposure` | SeasonBand.FloodRisk + WorldSettlements.Nodes(Ferry/Wharf/CanalJunction) | Notice tray |
| `AdministrativeDelay` | SeasonBand.MessageDelayBand + WorldSettlements.Routes(OfficialDispatchRoute).Reliability | Hall docket |
| `ExamAccess` | WorldSettlements.Routes(ExamTravelRoute) + Academy nodes | Ancestral hall |
| `LevyReach` | SeasonBand.CorveeWindow + PopulationAndHouseholds.Households | Desk sandbox |

每个叠加**需要一份只读的 builder 函数签名**（不在 Phase 1c 落实，但在 spec 里登记名字和依赖，防止后续加 state 列）。

---

## 第六章：案头读序契约（决策 F + 决策 J）

skill pack 有两条读序，一条来自 UI shell（`density-and-focus.md`），一条来自历史端（`china-ancient-sandbox-structure.md`）。它们是**外壳 × 内核**关系：

### 6.1 外壳：UI 读序（决策 J 外层）

每个 desk sandbox surface 上的信息组织顺序：

```
1. Current Locus        ← 当前最该被玩家关注的那一个节点 / 路线
2. Immediate Action     ← 与 Locus 关联的 1-3 个受限命令
3. Consequence Context  ← 若忽略 / 采纳，下月将发生什么（内核溯因链展开处）
4. Background Context   ← 本县 / 本月其他低优先级脉动
```

对应 `surface-grammar.md` 的 **Desk Sandbox** anchors：node field / route field / notice pins / one active local hotspot。

### 6.2 内核：溯因链（决策 F / J 内层）

当 Consequence Context 需要展开时，按历史端读序展开：

```
A. 节点压力      ← Current Locus 是什么压力在推
B. 路线传导      ← 压力通过哪条路线传到这里 / 从这里传出
C. 户/房/官牵连  ← 哪些 person / household / office 被卷入
D. 公共可见面    ← 这些压力已经在哪条 OpinionStream 上显形（§20）
```

**A-D 是 D 区（Consequence Context）内部的展开顺序，不是 UI 总读序**。D 展开的层深由 shell mode（case card vs docket tile）决定。

### 6.3 Query 支持

- `IWorldSettlementsQueries.GetCurrentLocus()` —— 回答外壳 1
- `IWorldSettlementsQueries.GetPressureAtNode(id)` / `GetPressureOnRoute(id)` —— 支持内核 A-B（Phase 1c+1 实现，见 §19）
- 其他模块 query 支持内核 C-D

### 6.4 契约登记

Phase 1c 把读序作为**常量**登记到 `Zongzu.Contracts`（不强推现有 projection 迁移）：

```csharp
public static class DeskSandboxReadOrder
{
    // 外壳
    public const int CurrentLocus = 10;
    public const int ImmediateAction = 20;
    public const int ConsequenceContext = 30;
    public const int BackgroundContext = 40;
}

public static class AncientSandboxCausalChain
{
    // 内核（在 ConsequenceContext 内部）
    public const int NodePressure = 10;
    public const int RouteTransmission = 20;
    public const int HouseholdAndClanImplication = 30;
    public const int PublicVisibleSurface = 40;
}
```

Phase 1c 之后新 notification 按此 ordering，Phase 1c 之前的现有 projection 不强迁。

---

## 第七章：`WorldSettlementsState` 新字段（Phase 1c 改动清单）

现有：
```csharp
public sealed class WorldSettlementsState
{
    public List<SettlementStateData> Settlements = new();
}
```

Phase 1c 扩展为：
```csharp
public sealed class WorldSettlementsState
{
    public List<SettlementStateData> Settlements = new();     // 扩展字段
    public List<RouteState> Routes = new();                    // 新增
    public SeasonBand CurrentSeason = new();                   // 新增
}

public sealed class SettlementStateData
{
    public SettlementId Id;
    public string Name;
    public SettlementTier Tier;
    public SettlementNodeKind NodeKind;               // 新增：功能语义
    public NodeVisibility Visibility;                 // 新增：官府 / 本地知 / 隐（决策 H）
    public SettlementEcoZone EcoZone;                 // 新增：区域生态（决策 I）
    public int Security;
    public int Prosperity;
    public int BaselineInstitutionCount;
    public List<SettlementId> NeighborIds = new();    // 新增：几何邻接（水陆通用）
    public SettlementId? ParentAdministrativeId;      // 新增：行政上级
    public List<OpinionStream> HostedOpinionStreams = new();  // 新增：本节点承载的意见流（§20）
}
```

Schema migration：`WorldSettlements` v1 → v2，新字段取默认：
- `NodeKind`: 根据 `Tier` 猜测默认（`Tier.CountySeat → CountySeat`，其他 → `Village`）
- `Visibility`: 默认 `StateVisible`
- `EcoZone`: 默认 `JiangnanWaterNetwork`（兰溪种子一致）
- `NeighborIds`: 空
- `ParentAdministrativeId`: null
- `HostedOpinionStreams`: 空
- `Routes`: 空
- `CurrentSeason`: `AgrarianPhase.Slack` / `Imperial` 全 0 / 其他字段 0

---

## 第八章：Query 契约扩展

`IWorldSettlementsQueries` 新增：

```csharp
public interface IWorldSettlementsQueries
{
    // 现有查询...

    // 新增（Phase 1c）
    IReadOnlyList<SettlementSnapshot> GetSettlementsByNodeKind(SettlementNodeKind kind);
    IReadOnlyList<SettlementSnapshot> GetSettlementsByVisibility(NodeVisibility visibility);
    IReadOnlyList<RouteSnapshot> GetRoutes();
    IReadOnlyList<RouteSnapshot> GetRoutesByKind(RouteKind kind);
    IReadOnlyList<RouteSnapshot> GetRoutesByLegitimacy(RouteLegitimacy legitimacy);
    IReadOnlyList<RouteSnapshot> GetRoutesTouching(SettlementId settlementId);
    SeasonBandSnapshot GetCurrentSeason();

    // 案头锚点（决策 J）
    LocusSnapshot? GetCurrentLocus();
}

public sealed record LocusSnapshot(
    SettlementId? PrimaryNode,
    RouteId? PrimaryRoute,
    string ReasonKey,           // "flood-risk-breached" / "canal-opening" / "corvee-peak" / ...
    int Intensity);             // 0-100：为什么是这个而不是别的
```

每个 `*Snapshot` 是值对象（与 `ClanSnapshot` 同风格），防止 state 内部引用外泄。

**`GetCurrentLocus()` 的评分逻辑**（Phase 1c 落实）：

以 `SeasonBand` + `Routes` + `Settlements` 为输入，计算候选 locus 的 Intensity：
- `FloodRisk >= 70` → 最相关的 `CanalJunction` / `Ferry` / `Wharf`，Intensity = FloodRisk
- `CanalWindow` 刚切换 → 对应 `CanalJunction`，Intensity = 60
- `CorveeWindow = Emergency` → `CountySeat`，Intensity = 70
- `Imperial.MourningInterruption >= 60` → `PrefectureSeat` / `CountySeat`，Intensity = MourningInterruption
- Route 有 `CurrentConstraintLabel` 非空 → 该 route，Intensity = (100 - Reliability)
- 兜底 → 最低 security 的 Village，Intensity = 20

取最高 Intensity 的候选作为 current locus。**评分函数纯、确定性、与月/xun 推进绑定**。

---

## 第九章：MVP 切片（Phase 1c 第一次落地范围）

**做**：
1. 新枚举定义在 `Zongzu.Contracts`：
   - `SettlementNodeKind`（含 `RelayPost` / `WellPost` / `HillShrine` / `FrontierCamp` / `CovertMeetPoint` / `SmugglingCache`）
   - `NodeVisibility` / `SettlementEcoZone`
   - `RouteKind`（含 `CorveeRoute` / `SmugglingCorridor` / `FugitivePath`）
   - `RouteMedium`（含 `CartCorridor` / `RidgeTrail`）
   - `RouteLegitimacy` / `ComplianceMode`
   - `AgrarianPhase` / `CanalWindow` / `CorveeWindow`
   - `OpinionStream`
   - `PressureKind`
2. 值对象：`RouteState` / `SeasonBand` / `ImperialBand` / `RouteSnapshot` / `SeasonBandSnapshot` / `LocusSnapshot` / `PublicSurfaceSignal`
3. Interface 签名（登记，大多 Phase 1c 不实装）：`IPressureFlowQueries` / `IOpinionResidueQueries` / `IConflictAnchorQueries` / `IImperialEventTestHarness`
4. `WorldSettlementsState` 扩展字段 + schema v1→v2 migration
5. `IWorldSettlementsQueries` 扩展（含 `GetCurrentLocus` / `GetSettlementsByVisibility` / `GetRoutesByLegitimacy`）
6. `WorldSettlementsModule.RunMonth` / `RunXun` 推进 `CurrentSeason` 的自然轴 + 政府轴带；皇权轴字段到位但自身不主动推进
7. 事件发出：至少发 `SeasonPhaseAdvanced` / `CanalWindowChanged` / `CorveeWindowChanged` / `ImperialRhythmChanged` / `RouteConstraintEmerged` / `FloodRiskThresholdBreached` / `SeasonalFestivalArrived`
8. `PublicSurfaceSignal` 自发：`CanalWindow` 转换 + `FloodRisk` 超阈的三流并发
9. `SimulationBootstrapper.SeedMinimalWorld` 兰溪种子（9 节点 + 5 路线，见第十二章 12.2/12.5）；全部 `EcoZone = JiangnanWaterNetwork`
10. `CalibrationBands.cs` / `DeskSandboxReadOrder.cs` / `AncientSandboxCausalChain.cs` / `WorldSettlementsEventNames.cs`
11. 测试：
    - WorldSettlements 单测：新字段序列化（含 `EcoZone=NorthChinaDryRoad` round-trip）+ migration + 三轴 season 推进
    - Integration：bootstrap 后 `GetRoutes().Count == 5`、`GetSettlementsByVisibility(Covert).Count == 1`、`GetRoutesByLegitimacy(Illicit).Count == 1`、`GetCurrentLocus()` 非空
    - **Liveness（§22）**：`LivingWorldLivenessTests` 必过（12 月三轴节律 + shell-facing readiness）

**不做**（留待后续 phase）：
- 叠加层 builder 实现（只登记名字）
- 案头读序的现有 projection 迁移（只登记契约常量）
- 其他三个 `EcoZone` 的完整种子世界（朔州对照只做 schema round-trip 测试，不实装世界）
- 私信带 / 谣言带（留给 `PublicLifeAndRumor` 合作）
- `OpinionResidue` / `ShameResidue` 实际值（归 `SocialMemoryAndRelations`）
- `PressureFlow` 具体衰减实现（Phase 1c+1）
- `ConflictAnchor` 具体判定（Phase 5/6）
- 动态节点生灭（Phase 2+）
- 节点人口 / 经济实际流动（属于其他模块）
- 玩家命令窗口（`PlayerInvestmentLevel` 使用，留 Phase 1d+）

---

## 第十章：与已落地模块的合约

| 模块 | 将来读 | 将来不写 |
|---|---|---|
| PopulationAndHouseholds | `SeasonBand.LaborPinch` / `CorveeWindow` 决定徭役可征 | `WorldSettlementsState` |
| TradeAndIndustry | `Routes(MarketRoute)` + `Routes(GrainRoute)` + `SeasonBand.CanalWindow` | 同上 |
| OrderAndBanditry | `Routes.Reliability` + `SeasonBand.LaborPinch` | 同上 |
| OfficeAndCareer | `Routes(OfficialDispatchRoute)` + `SeasonBand.MessageDelayBand` | 同上 |
| ConflictAndForce | `Routes(MilitaryMoveRoute)` + `Nodes(Pass/Garrison)` | 同上 |
| FamilyCore | `Nodes(LineageHall)` 定位 | 同上 |
| PersonRegistry | — | — |

所有消费者通过 **query** 读，遵守 `MODULE_BOUNDARIES`。

---

## 第十一章：anti-pattern 防撞护栏

防止未来退化的明确禁止：

- ❌ 不要给 `SettlementStateData` 加 `TaxReach` / `BanditRisk` 这类字段（叠加层归 query）
- ❌ 不要把水路建成 `RouteMedium.LandRoad` 的 subtype
- ❌ 不要用 `Season` 单一枚举覆盖全部带
- ❌ 不要在 `WorldSettlements` 写路线上的"当前流量"（那属于 `TradeAndIndustry` / `PopulationAndHouseholds`）
- ❌ 不要把 `SettlementNodeKind` 用作行政等级（有 `SettlementTier` 负责）
- ❌ 不要给 `RouteState` 加 `OwnerFamilyId` 之类的字段（路线不属于任何宗族）
- ❌ 不要让 `NarrativeProjection` 直接读 `SeasonBand` 然后生成文本——走 `IWorldSettlementsQueries.GetCurrentSeason()` query

---

*本文档定义空间骨骼的静态形状。规则密度（路线实际传导、季节实际影响、叠加实际数值）在 Phase 1c 之后逐项填充。*

---

## 第十二章：兰溪种子世界详图

Phase 1c 的兰溪不是"示例数据"，而是**用于检验骨骼的最小自洽世界**。它必须能同时承载第一至十一章每一条契约。

### 12.1 地理假设

兰溪是 `jiangnan-water-network-county.md` 描述的**江南水网县**：一条主河穿过县境，两条乡级支流，一个漕运通道，一个主渡口，一个市镇依埠而立。

### 12.2 节点表（9 个节点）

所有节点 `EcoZone = JiangnanWaterNetwork`。

| NodeId | Name | NodeKind | Tier | Visibility | Security | Prosperity | 语义职责 |
|---|---|---|---|---|---|---|---|
| `n.lx-county` | 兰溪县治 | `CountySeat` | `County` | StateVisible | 55 | 50 | 县衙案牍、刑讼、官仓开关 |
| `n.lx-market` | 埠头镇 | `MarketTown` | `MarketTown` | StateVisible | 45 | 58 | 市集节律、客栈、牙行 |
| `n.lx-zhang-hall` | 张氏祠堂 | `LineageHall` | `Village` | **LocalKnown** | 60 | 40 | 宗族议事、族人调解 |
| `n.lx-east-village` | 东溪村 | `Village` | `Village` | StateVisible | 40 | 35 | 徭役、收成、佃户 |
| `n.lx-south-village` | 南渡村 | `Village` | `Village` | StateVisible | 38 | 33 | 徭役、收成、佃户 |
| `n.lx-ferry` | 南渡津 | `Ferry` | `Village` | StateVisible | 42 | 40 | 过渡队列、水陆信息交接 |
| `n.lx-granary` | 常平仓 | `Granary` | `County` | StateVisible | 50 | 45 | 粮储、赈济开关、盗劫目标 |
| `n.lx-qingfeng-temple` | 清风庙 | `Temple` | `Village` | StateVisible | 48 | 30 | 节庆、谣传、避乱 |
| `n.lx-salt-cache` | 芦滩盐窝 | `SmugglingCache` | `Village` | **Covert** | 25 | 35 | 私盐、亡命藏匿 |

**为什么加芦滩盐窝**：证明 spec 决策 H 在种子世界里能实例化。该节点对 `GetSettlementsByVisibility(Covert)` 有返回，对 `GetSettlementsByNodeKind(Village)` 无返回。

### 12.3 行政父节点

- `n.lx-county` 是其余**合法节点**的 `ParentAdministrativeId`
- `n.lx-salt-cache.ParentAdministrativeId = null`（走私窝不挂行政上级）
- `n.lx-county.ParentAdministrativeId = null`（上一级 `PrefectureSeat` 不在 MVP 里实例化）

### 12.4 邻接图（几何 NeighborIds）

```
兰溪县治 ─── 埠头镇 ─── 南渡津 ═══ 南渡村
    │           │
    │           └─── 清风庙
    │
    ├─── 张氏祠堂
    ├─── 东溪村 ╌╌╌ 芦滩盐窝（私路）
    └─── 常平仓
```

- `───`：陆路邻接
- `═══`：含水路邻接（南渡津与南渡村隔河）
- `╌╌╌`：灰色路径邻接（只承载走私廊，不算官方认可邻接）

### 12.5 路线表（5 条 routes）

所有路线 `ComplianceMode` 默认 `PaperCompliance`，除非标注。

| RouteId | Kind | Medium | Legitimacy | Compliance | Origin | Destination | Waypoints | TravelDays | Capacity | Reliability | SeasonalVuln |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `r.lx-grain-east` | `GrainRoute` | `LandRoad` | Official | - | `n.lx-east-village` | `n.lx-granary` | [] | 0 | 50 | 70 | 30 |
| `r.lx-grain-south` | `GrainRoute` | `LandRoad`+`FerryLink` | Official | - | `n.lx-south-village` | `n.lx-granary` | [`n.lx-ferry`, `n.lx-market`] | 1 | 45 | 55 | 60 |
| `r.lx-market-main` | `MarketRoute` | `WaterRiver` | Tolerated | - | `n.lx-market` | `n.lx-county` | [] | 0 | 70 | 65 | 45 |
| `r.lx-dispatch-hub` | `OfficialDispatchRoute` | `LandRoad` | Official | **PaperCompliance** | `n.lx-county` | `n.lx-market` | [] | 0 | 40 | 75 | 25 |
| `r.lx-salt-smuggle` | `SmugglingCorridor` | `MountainPath` | **Illicit** | - | `n.lx-east-village` | `n.lx-salt-cache` | [] | 0 | 15 | 40 | 50 |

**为什么是这五条**：
- `r.lx-grain-east`：最短粮道，无水路交接，基准控制
- `r.lx-grain-south`：跨水粮道（含渡口 waypoint），检验决策 C（水陆拓扑）+ §2.6 交接约束
- `r.lx-market-main`：纯水路商路，检验水路独立拓扑
- `r.lx-dispatch-hub`：官文路，检验 `ComplianceMode` 与 `SeasonBand.MessageDelayBand` 读取
- `r.lx-salt-smuggle`：**走私廊**（决策 H）；检验 validator 对 `Illicit` 路线跳过 ferry/bridge 约束，检验 `GetRoutesByLegitimacy(Illicit)` 返回一条

### 12.6 初始 SeasonBand

以兰溪世界**三月（春耕起始）**开局：

| 字段 | 值 | 说明 |
|---|---|---|
| `AsOf` | `GameDate(1200, 3)` | 与 `CurrentDate` 对齐 |
| `AgrarianPhase` | `Sowing` | 春耕 |
| `LaborPinch` | 55 | 春耕初忙 |
| `HarvestWindowProgress` | 0 | 未到秋 |
| `WaterControlConfidence` | 60 | 堤工新修 |
| `EmbankmentStrain` | 30 | 春汛尚浅 |
| `FloodRisk` | 35 | 春汛临近 |
| `CanalWindow` | `Limited` | 春汛初开未稳 |
| `MarketCadencePulse` | 40 | 清明前后赶集 |
| `CorveeWindow` | `Quiet` | 春耕不征（典型） |
| `MessageDelayBand` | 1 | 春道未淤 |
| `Imperial.MourningInterruption` | 0 | 无国丧 |
| `Imperial.AmnestyWave` | 0 | 无大赦 |
| `Imperial.SuccessionUncertainty` | 0 | 朝廷稳定 |
| `Imperial.MandateConfidence` | 60 | 默认中等可信 |
| `Imperial.CourtTimeDisruption` | 0 | 常态 |

### 12.7 族人锚点（与 Phase 1a/1b 合流）

- heir（张远）作为 `n.lx-zhang-hall` 的"所在节点"不由 `WorldSettlements` 记录；`FamilyCore` 后续负责，Phase 1c 不动。
- `n.lx-zhang-hall` 仅作为 `LineageHall` 节点存在，不含人物数据。

### 12.8 朔州对照种子（登记但不实装）

为验证 `SettlementEcoZone` / `NodeKind` 能承载其他区域，第 12 章登记一个"朔州"对照世界的骨骼形状（不实装到 `SeedMinimalWorld`，仅在 spec 里展示 schema 能装下）：

| NodeId | NodeKind | EcoZone | 说明 |
|---|---|---|---|
| `n.sz-county` | `CountySeat` | NorthChinaDryRoad | 朔州县治 |
| `n.sz-relay` | `RelayPost` | NorthChinaDryRoad | 驿站 |
| `n.sz-well` | `WellPost` | NorthChinaDryRoad | 旱县井台 |
| `n.sz-pass` | `Pass` | BorderGarrisonBelt | 通塞外的关 |
| `n.sz-border-watch` | `BorderWatch` | BorderGarrisonBelt | 烽堠 |
| `n.sz-garrison` | `Garrison` | BorderGarrisonBelt | 戍营 |

路线示例：`r.sz-dispatch-urgent: OfficialDispatchRoute / CartCorridor / StrictEnforcement / n.sz-county → n.sz-relay → n.sz-pass`（边郡急文）

Phase 1c 不构造这些节点，但 schema 必须能序列化 / 反序列化这些值。测试：`SettlementStateData` 的 JSON round-trip 支持 `EcoZone = NorthChinaDryRoad` + `NodeKind = WellPost`。

---

## 第十三章：Schema 迁移与兼容

### 13.1 模块版本号

- `WorldSettlements` 模块当前 schema version = **1**
- Phase 1c 升到 **2**
- 注册到 `SaveMigrationPipeline`：`MigrateWorldSettlementsStateV1ToV2`

### 13.2 迁移函数规则

```csharp
private static ModuleStateEnvelope MigrateWorldSettlementsStateV1ToV2(ModuleStateEnvelope envelope)
{
    WorldSettlementsState oldState = Deserialize(envelope.Payload);

    WorldSettlementsState newState = new()
    {
        Settlements = oldState.Settlements.Select(s => new SettlementStateData
        {
            Id = s.Id,
            Name = s.Name,
            Tier = s.Tier,
            Security = s.Security,
            Prosperity = s.Prosperity,
            BaselineInstitutionCount = s.BaselineInstitutionCount,

            // 新字段：默认从 Tier 推断 NodeKind
            NodeKind = s.Tier switch
            {
                SettlementTier.Prefecture => SettlementNodeKind.PrefectureSeat,
                SettlementTier.County     => SettlementNodeKind.CountySeat,
                SettlementTier.MarketTown => SettlementNodeKind.MarketTown,
                _                         => SettlementNodeKind.Village,
            },
            NeighborIds = new(),                     // 空，后续再建
            ParentAdministrativeId = null,           // 空，后续再建
        }).ToList(),

        Routes = new(),                               // 空路线列表
        CurrentSeason = new SeasonBand               // 中性默认
        {
            AsOf = default,                          // 首次推进时被覆盖
            AgrarianPhase = AgrarianPhase.Slack,
            CanalWindow = CanalWindow.Limited,
            CorveeWindow = CorveeWindow.Quiet,
            // 其他字段默认 0
        },
    };

    return new ModuleStateEnvelope
    {
        ModuleKey = KnownModuleKeys.WorldSettlements,
        ModuleSchemaVersion = 2,
        Payload = Serialize(newState),
    };
}
```

### 13.3 迁移后旧测试保护

旧的 `WorldSettlements` 单测（3 项）必须继续通过：
- 旧存档 load → migrate → reload，`Settlements` 字段保持一致
- 不要求旧存档 migrate 后 Routes 非空（留白是合法）

### 13.4 与 Persistence 层合约

`SaveCompatibilityGuardrailTests.LoadM2_CanLoadLegacyM0M1SaveWhenNewModulesAreDisabled` 必须继续通过：即使旧 M0M1 存档里 `WorldSettlementsState` 是 v1，走 v1→v2 migration 后能被 LoadM2 接受。

---

## 第十四章：代码落地清单

Phase 1c 的精确改动矩阵。

### 14.1 `Zongzu.Contracts`

新增文件 `WorldSpatialTypes.cs`：
- `SettlementNodeKind` enum
- `RouteKind` enum
- `RouteMedium` enum
- `AgrarianPhase` enum
- `CanalWindow` enum
- `CorveeWindow` enum
- `RouteId`（readonly struct，含 `GuidValue`）
- `RouteSnapshot`（只读值对象）
- `SeasonBandSnapshot`（只读值对象）

新增文件 `CalibrationBands.cs`：
- `TravelDaysBand` / `MessageDelayBand` / `MusterDelayBand` / `GranaryBufferBand` 的共享常量（0–5 的命名带）
- 带名字到语义的映射字典

### 14.2 `Zongzu.Modules.WorldSettlements`

修改文件 `WorldSettlementsState.cs`：
- `SettlementStateData` 加 `NodeKind` / `NeighborIds` / `ParentAdministrativeId`
- 新类 `RouteState`
- 新类 `SeasonBand`
- `WorldSettlementsState` 加 `Routes` / `CurrentSeason`

修改 `WorldSettlementsModule.cs`：
- `ModuleSchemaVersion` 返回 2
- `CreateInitialState()` 保证 `CurrentSeason` / `Routes` 非 null
- `RunMonth`：推进 `AgrarianPhase` / `HarvestWindowProgress` / `CanalWindow` / `CorveeWindow`
- `RunXun`：推进 `LaborPinch` / `MarketCadencePulse` / `MessageDelayBand` / `EmbankmentStrain` / `FloodRisk`
- `RunMonth` / `RunXun` 更新 `CurrentSeason.AsOf = currentDate`

修改 `WorldSettlementsQueries.cs`：
- 加 `GetSettlementsByNodeKind(kind)`
- 加 `GetRoutes()` / `GetRoutesByKind(kind)` / `GetRoutesTouching(settlementId)`
- 加 `GetCurrentSeason()`

修改 `IWorldSettlementsQueries.cs`（接口）：同步新方法

新增 `WorldSettlementsValidator.cs`：
- `ValidateRouteConnectivity(state)`：路径含水路的 route 必须经过 Ferry/Bridge/Ford/CanalJunction

### 14.3 `Zongzu.Application`

修改 `SimulationBootstrapper.cs`：
- `CreateDefaultMigrationPipeline`：注册 `WorldSettlementsState` v1→v2 migration
- `SeedMinimalWorld`：按第 12 章表种子 8 个节点 + 4 条路线 + 春耕 SeasonBand

### 14.4 `tests/Zongzu.Modules.WorldSettlements.Tests`

新增测试方法：
- `CreateInitialState_ReturnsEmptyRoutesAndSlackSeason`
- `RunMonth_AdvancesAgrarianPhaseAcrossYear`（12 月轮回检查）
- `RunXun_PulsesLaborPinchAndMarketCadence`
- `Routes_CrossingWaterRequireFerryOrBridgeWaypoint`（validator 契约）
- `GetRoutesByKind_FiltersCorrectly`
- `GetRoutesTouching_IncludesOriginDestinationAndWaypoints`

### 14.5 `tests/Zongzu.Persistence.Tests`

新增 `WorldSettlementsMigrationTests.cs`（或并入现有 `SaveMigrationPipelineTests.cs`）：
- `WorldSettlementsStateV1_MigratesToV2_PreservesSettlements`
- `WorldSettlementsStateV1_MigratesToV2_InfersNodeKindFromTier`
- `WorldSettlementsStateV1_MigratesToV2_InitializesRoutesAndSeason`

### 14.6 `tests/Zongzu.Integration.Tests`

新增 `SpatialSkeletonIntegrationTests.cs`：
- `LanxiSeed_IncludesEightNodesAndFourRoutes`
- `LanxiSeed_GrainSouthRoute_HasFerryWaypoint`
- `LanxiSeed_InitialSeason_IsSowing_WithLimitedCanal`
- `AdvanceTwelveMonths_PreservesDeterministicReplayHash_WithSpatialFields`

### 14.7 **不改**

- `NarrativeProjection`：Phase 1c 不消费新字段
- `PresentationReadModelBuilder`：Phase 1c 不构建叠加层
- 其他领域模块：Phase 1c 不读 Routes / SeasonBand

这保证 Phase 1c 的变更**纯粹是加结构，不改行为**。

---

## 第十五章：测试计划（按契约验证）

每个契约须有一条测试守护。

| 契约 | 守护测试 | 位置 |
|---|---|---|
| 决策 A：NodeKind 独立于 Tier | `NodeKind_IsIndependentOf_Tier` | WorldSettlements.Tests |
| 决策 B：同走廊可多 RouteKind | `MultipleRouteKinds_CanShareSameGeometry` | WorldSettlements.Tests |
| 决策 C：水路独立拓扑 | `Routes_CrossingWaterRequireFerryOrBridgeWaypoint` | WorldSettlements.Tests |
| 决策 D：Season = 带 | `SeasonBand_HasParallelBands_NotSingleEnum` | WorldSettlements.Tests |
| 决策 E：叠加不入 state | `WorldSettlementsState_DoesNotContain_TaxReachFields`（反射扫描禁字段） | WorldSettlements.Tests |
| 决策 F/J：案头读序登记 | `DeskSandboxReadOrder_AndCausalChain_ConstantsAreDeclared` | Contracts.Tests |
| 决策 H：可见性 / 合法性 | `LanxiSeed_HasCovertNodeAndIllicitRoute` + `Validator_SkipsFerryCheck_ForIllicitRoutes` | WorldSettlements.Tests |
| 决策 I：三轴节律 | `ImperialRhythm_InteractsWith_CorveeAndDispatch` | WorldSettlements.Tests |
| 种子世界完整性 | `LanxiSeed_SatisfiesAllNodeKindEarningClauses`（9 节点 + 5 路线） | Integration.Tests |
| Migration 兼容 | v1→v2 多条 migration 测试 | Persistence.Tests |
| 确定性 | 12 个月 / 60 个月 replay hash | Integration.Tests |
| Liveness | 第 22 章三轴 + shell-facing 全部 | Integration.Tests |

---

## 第十六章：未来阶段接口预留

Phase 1c **只做骨骼**，但必须为后续阶段留接口，不留技术债。

### 16.1 叠加层 builder（Phase 1c+1）

在 `Zongzu.Presentation.Unity` 下登记一个空 `OverlayCatalog` 静态类，列出七个叠加名字（第五章清单），每项一个空 TODO builder：

```csharp
public static class OverlayCatalog
{
    public static IReadOnlyList<string> DeclaredOverlays =>
    [
        "TaxReach", "GrainMovement", "BanditRisk", "FloodExposure",
        "AdministrativeDelay", "ExamAccess", "LevyReach",
    ];

    // Builder 签名留空，实现延后
}
```

作用：**合约登记**。防止其他人误把叠加字段加进 state。

### 16.2 案头读序契约（Phase 1c）

在 `Zongzu.Contracts` 下加两组常量（§6.4 已展示，此处重申）：`DeskSandboxReadOrder`（外壳）+ `AncientSandboxCausalChain`（内核）。后续 `NarrativeProjection` 在构建 trace 时先按外壳分区，再在 ConsequenceContext 区内部按内核 append diffs。

### 16.3 区域 eco zone —— **已移至 Phase 1c 实装**

~~预留枚举~~ `SettlementEcoZone` 已在 §3.3 和 §7 落地为 `SettlementStateData.EcoZone` 字段，Phase 1c 即生效。兰溪种子 = `JiangnanWaterNetwork`；朔州对照仅做 schema round-trip，不实装世界。

### 16.4 多渠道信息带（与 `PublicLifeAndRumor` 合作）

`SeasonBand.MessageDelayBand` 目前只代表官文带。后续新增 `PrivateLetterBand` / `RumorBand`，由 `PublicLifeAndRumor` 通过 query 贡献。Phase 1c 的 `SeasonBand` 留注释标记这一点。

### 16.5 动态节点生灭

Phase 1c 的节点集合 **静态**。后续（如移民、毁坏、新建）支持在 `WorldSettlementsEventNames` 下加 `SettlementEstablished` / `SettlementAbandoned` / `NodeRoleShifted`。**暂不定义**，只在 spec 登记为未来契约。

---

## 第十七章：失败模式与判别方法

在 code review 和未来 PR 检查里用来识别退化：

### 17.1 节点层面

| 退化模式 | 判别 |
|---|---|
| "节点都是 Village" | Node distribution 单调，非 Village 节点数 < 3 |
| "NodeKind 变成装饰" | 某 NodeKind 不对应任何 query / projection 消费 |
| "行政等级被混入 NodeKind" | 发现 `CountySeat / PrefectureSeat` 类节点携带非官府语义字段 |

### 17.2 路线层面

| 退化模式 | 判别 |
|---|---|
| "路线几何化" | 某 RouteKind 的 routes 数 = 0 但 `TravelDaysBand` 被广泛用 |
| "水陆折叠" | 出现 `RouteMedium.LandRoad` 但 Waypoints 含 `Ferry` 节点且无 `FerryLink` route 条目 |
| "路线持久化流量" | `RouteState` 出现动词字段（`CurrentCargoLoad` 等） |

### 17.3 季节层面

| 退化模式 | 判别 |
|---|---|
| "季节变单一枚举" | `SeasonBand` 字段数 < 5 |
| "季节不推进" | 12 个月后 `AgrarianPhase` 仍为初值 |
| "带值无区间" | 某 band 字段出现 `> 100` 或 `< 0` 值 |

### 17.4 叠加层面

| 退化模式 | 判别 |
|---|---|
| "叠加进 state" | `WorldSettlementsState` 出现 `TaxReach` / `BanditRisk` 字段 |
| "叠加绕过 query" | 非 `IWorldSettlementsQueries` 调用方直接读 `SeasonBand` |

### 17.5 自动化

加一条单测 `WorldSettlementsState_FieldsDoNotContainForbiddenKeywords`，通过反射扫描字段名：禁止 `TaxReach` / `GrainMovement` / `BanditRisk` / `FloodExposure` / `AdministrativeDelay` / `ExamAccess` / `LevyReach` 出现在 state 类。

---

## 第十八章：脉动与窗口（Pulses & Windows）

**活世界铁律**：`SeasonBand` 不是可读字段，是**每拍都会变**的脉冲容器。本章定义每个字段的推进函数。所有函数必须**纯、确定性、幂等**。

### 18.1 `AgrarianPhase` 月级推进

```
Slack     → Sowing     (Month ∈ {2, 3})
Sowing    → Transplant (Month ∈ {4, 5})
Transplant → Tending   (Month ∈ {6, 7, 8})
Tending   → Harvest    (Month ∈ {9, 10})
Harvest   → Slack      (Month ∈ {11, 12, 1})
```

双季稻区将来通过 `SettlementEcoZone` 覆写（Phase 2+）；MVP 只落单季。

### 18.2 `CanalWindow` 月级状态机

```
Closed → Limited : Month = 2 且 FloodRisk < 70
Limited → Open   : Month = 4 且 EmbankmentStrain < 60 且 WaterControlConfidence > 50
Open → Limited   : Month = 9 或 FloodRisk ≥ 70
Limited → Closed : Month = 12 或 EmbankmentStrain ≥ 85
```

每次转换**必发 `CanalWindowChanged` 事件**（见第二十一章）。

### 18.3 `CorveeWindow` 月级状态机

```
Quiet → Pressed    : AgrarianPhase ∈ {Slack} 且 CanalWindow ≠ Closed
Pressed → Quiet    : AgrarianPhase ∈ {Sowing, Transplant, Harvest}（避让农忙）
任意 → Emergency   : EmbankmentStrain ≥ 80 或 FloodRisk ≥ 80（急修）
Emergency → Quiet  : 条件解除后隔 1 个月
```

每次转换**必发 `CorveeWindowChanged` 事件**。

### 18.4 xun 级脉冲（旬级）

| 字段 | 推进规则 |
|---|---|
| `LaborPinch` | `AgrarianPhase` 映射基值（Sowing=55 / Transplant=75 / Tending=40 / Harvest=85 / Slack=20），旬内 ±5 噪声 |
| `MarketCadencePulse` | 节令日历表命中 +20，月初/月中市日 +10，其余 base 30 |
| `MessageDelayBand` | `Route(OfficialDispatchRoute).Reliability` 最低值映射（70-100→1, 40-70→2, <40→3），叠加季节阻塞 +1 |
| `EmbankmentStrain` | 汛季（Month 5-8）每 xun +5；非汛 -3（堤自然恢复）；徭役 Emergency 时本 xun 内 -15 |
| `FloodRisk` | `EmbankmentStrain × 季节系数`（Month 6-7 系数 = 1.2，其他 = 0.6）|
| `WaterControlConfidence` | 慢变量，月级 ±3，`CorveeWindow = Emergency` 解除后 +10 |
| `HarvestWindowProgress` | 只在 `AgrarianPhase = Harvest` 期间 xun 级 +33（3 xun 完成收获） |

### 18.5 `Route.Reliability` 的季节耦合

Phase 1c 不做 route 自己的季节动态，但**登记契约**：

- 冬季（Month 11-2）：`RouteMedium = WaterRiver | WaterCanal` 的 route `Reliability *= 0.5`（结冰 / 枯水）
- 汛季（Month 6-8）：`Ferry / Ford / CanalJunction` 经过的 route `Reliability *= 0.7`
- 春泥（Month 3）：`RouteMedium = LandRoad | MountainPath` 的 route `Reliability *= 0.85`

MVP 把这些写进 `IWorldSettlementsQueries.GetRoutes()` 的**读时计算**（只读，不写 state），不把衰减持久化。这样 route 的 Reliability 是"活的"每次查询都反映当下季节。

---

## 第十九章：压力传导契约（PressureFlow）

**活世界铁律**：节点是**压力源**，路线是**压力介质**，其他模块是**压力消费者**。不定义这条管线，空间骨骼就是死的。

### 19.1 `PressureKind` 枚举（`Zongzu.Contracts`）

```csharp
public enum PressureKind
{
    GrainPressure = 10,        // 粮压：Granary 缺口 / Village 收成不足
    TaxPressure = 20,           // 税压：CountySeat 向 Village 征收
    CorveePressure = 30,        // 徭压：CountySeat 向 Village 调工
    BanditPressure = 40,        // 匪压：Route 受劫 / 村受扰
    FloodPressure = 50,         // 险压：CanalJunction / Ferry 临近溃决
    InformationLag = 60,        // 信压：延迟导致的决策失准
    MarriagePressure = 70,      // 婚压：Village / LineageHall 婚配困难
    EscortPressure = 80,        // 护压：Route 急需护卫
}
```

这些是**跨模块共享概念**，`WorldSettlements` 只登记枚举，不独占定义。

### 19.2 压力流签名（query 面，不入 state）

压力读取是**跨模块 query 合成**，定义在 `IPressureFlowQueries`（placeholder interface，Phase 1c+1 实现）：

```csharp
public interface IPressureFlowQueries
{
    // 节点当前承载的所有压力（多模块贡献合并）
    IReadOnlyList<PressureReading> GetPressureAtNode(SettlementId node);

    // 路线当前传导的所有压力（路线特性决定衰减）
    IReadOnlyList<PressureReading> GetPressureOnRoute(RouteId route);

    // 全图压力快照（供 desk sandbox overlay 用）
    PressureMapSnapshot GetPressureMap();
}

public sealed record PressureReading(
    PressureKind Kind,
    int Intensity,        // 0-100
    string SourceModuleKey,
    string? SourceEntityKey);
```

### 19.3 衰减规则（MVP 雏形）

传导衰减由 `RouteState` 三字段决定：

```
DeliveredIntensity = SourceIntensity
                   × (Reliability / 100)
                   × (1 - SeasonalVulnerability × SeasonPenalty / 100)
                   × CapacityFactor
```

其中 `CapacityFactor` = `1.0` 若路线容量足够，否则线性下降。

**Phase 1c 只落契约不落实现**：`IPressureFlowQueries` 签名定下，但具体 implementation 是 Phase 1c+1。Phase 1c 保证 state 能支持这条计算（`Reliability` / `SeasonalVulnerability` / `Capacity` 字段到位）。

### 19.4 节点压力发源的六个锚

| Kind | 源节点类 | 发源条件（示例） |
|---|---|---|
| `GrainPressure` | `Granary` | `GrainStock < 0.3 * Capacity` |
| `TaxPressure` | `CountySeat` | 年中 / 年末催税节点 |
| `CorveePressure` | `CountySeat` | `CorveeWindow ∈ {Pressed, Emergency}` |
| `BanditPressure` | `Village` / `Ferry` | `OrderAndBanditry` 模块判定 |
| `FloodPressure` | `CanalJunction` / `Ferry` / `Wharf` | `FloodRisk ≥ 60` |
| `InformationLag` | `CountySeat` | `MessageDelayBand ≥ 3` |

`WorldSettlements` 自己只发 `FloodPressure` / `InformationLag`（因为源头数据在 `SeasonBand` 里）。其他 `PressureKind` 由对应 domain 模块发。

---

## 第二十章：公共可见面契约（Public Surfaces）

**活世界铁律**：`skill pack` 明示"**公共生活之所以重要，是因为它让隐藏系统可见**"（`public-opinion-reputation-public-spaces.md`）。同一文件还铁律要求"**Notices, proclamations, and rumor should compete with each other instead of merging into one generic information value.**"

### 20.1 五条意见流（OpinionStream）

不是单一 public opinion 值，不是单一 rumor heat，而是**五条独立、相互竞争的流**：

```csharp
public enum OpinionStream
{
    NoticeBoard = 1,        // 榜示流：官方告示、律文、税摊、榜文
    MarketTalk = 2,         // 市井流：市集价格、牙行消息、商贩闲话
    TeahouseChat = 3,       // 茶话流：士人议论、读书人评判、公案讨论
    TempleWhisper = 4,      // 庙堂流：神迹、吉凶、避乱传言、灵验谣传
    HallPronouncement = 5,  // 族堂流：宗族表态、族议结果、调解裁决
}
```

每条流**独立 heat**，同一事件可在多条流上有**不同强度甚至方向相反**（县衙榜示处决某贪吏 → `NoticeBoard` 正面，但 `TeahouseChat` 质疑"替罪羊"，`HallPronouncement` 关联到某族叔嫌隙）。

### 20.2 流与节点的宿主关系

每个节点 `HostedOpinionStreams: List<OpinionStream>` 列举它承载哪几条流（节点不必承载所有流）：

| NodeKind | 默认承载流 |
|---|---|
| `CountySeat` / `YamenPost` / `RelayPost` | `NoticeBoard` |
| `MarketTown` / `Wharf` | `MarketTalk`, `TeahouseChat` |
| `Ferry` | `MarketTalk`（过渡者带话），`TempleWhisper` |
| `Temple` / `HillShrine` / `ShrineCourt` | `TempleWhisper` |
| `LineageHall` | `HallPronouncement` |
| `Academy` / `VillageSchool` | `TeahouseChat` |
| `Village` | `MarketTalk`（本村集）、`TempleWhisper`（附近庙） |
| `CovertMeetPoint` / `SmugglingCache` | （空 —— 隐节点不上公共面） |

### 20.3 可见信号 → notification 契约

每次可见面触发**必产出至少一条** `PublicSurfaceSignal`：

```csharp
public sealed record PublicSurfaceSignal(
    SettlementId NodeId,
    SettlementNodeKind NodeKind,
    OpinionStream Stream,               // 五条流之一
    PublicSurfaceCategory Category,     // Rumor / Ritual / Commerce / Petition / Alarm
    NotificationTier Tier,              // 既有 NotificationTier 枚举
    string HeadlineKey,                 // projection 层翻译
    int Sentiment,                      // -100 ~ +100：本条信号在本流上的褒贬
    IReadOnlyList<PressureKind> ExposedPressures);
```

**同一事件可产出多条 signal，分别挂到不同 Stream，Sentiment 各异**。这是实现"流竞争"的关键。

信号进入 `NarrativeProjection.NotificationProjectionContext`，由 `PresentationReadModelBuilder` 合入 notice tray。Tray 渲染时**按 Stream 分列**，不合并。

**Phase 1c `WorldSettlements` 必须自发两类信号**：

- `CanalWindow` 转换 → `{NodeKind=CanalJunction, Stream=NoticeBoard, Category=Alarm|Commerce, Sentiment=-40|+30, ExposedPressures=[FloodPressure]|[GrainPressure]}`
- `FloodRisk ≥ 70` → **三条并发**：
  - `{NodeKind=CanalJunction, Stream=NoticeBoard, Category=Alarm, Tier=Urgent, Sentiment=-50}` （官方通告）
  - `{NodeKind=Ferry, Stream=MarketTalk, Sentiment=-70}` （渡口恐慌）
  - `{NodeKind=Temple, Stream=TempleWhisper, Sentiment=-60}` （神怒传言）

### 20.4 跨月残留（Shame / Praise Residue）

基于 `public-opinion-reputation-public-spaces.md` "Public humiliation and public praise should matter because they change later compliance, revenge, and alliance choices"。

每条 `OpinionStream` 上的 heat 不当月清零，而是**跨月衰减**。这份 state **不归 `WorldSettlements`**，归 `SocialMemoryAndRelations`（Phase 4）。空间骨骼只**登记接口**：

```csharp
public interface IOpinionResidueQueries
{
    int GetStreamHeat(SettlementId node, OpinionStream stream);
    int GetShameResidue(SettlementId node);   // 全流合成的负面残留
    int GetPraiseResidue(SettlementId node);  // 全流合成的正面残留
}
```

Phase 1c 不实现，仅登记签名；`IWorldSettlementsQueries.GetCurrentLocus()` 将来会读 `GetShameResidue` 作为评分输入之一（目前使用常量 0）。

### 20.5 节令日历表（MVP 最小集）

兰溪世界**必须识别**的节令（`Zongzu.Contracts.SeasonalFestivals`）：

| 节令 | 大致月份 | 触发流 |
|---|---|---|
| `Qingming`（清明） | Month 3 旬 3 / Month 4 旬 1 | `TempleWhisper` + `HallPronouncement`（祭祖） |
| `Duanwu`（端午） | Month 5 | `TempleWhisper` + `MarketTalk`（赛龙舟 / 节市） |
| `Zhongyuan`（中元） | Month 7 | `TempleWhisper` + `HallPronouncement`（祭幽 / 祭祖） |
| `Qiushe`（秋社） | Month 8 | `TempleWhisper` + `MarketTalk`（丰前酬社 + 节市） |
| `Laba`（腊八） | Month 12 | `TempleWhisper` + `MarketTalk`（施粥 + 岁末集） |

节令命中时，对应流 heat +20；`MarketCadencePulse` +15。

### 20.6 "无可见流 = 无压力"红线

若一个压力**无法在任何 OpinionStream 上显形**，它就不是玩家可感知的压力。反向约束：新增的 `PressureKind` 必须登记至少一条 `OpinionStream` 的显形路径，否则 code review 应拒收。

隐节点（`CovertMeetPoint` / `SmugglingCache`）**故意不承载任何流** —— 这就是"covert"的定义。它们的存在靠 `GetSettlementsByVisibility(Covert)` 显形，不靠 OpinionStream。

---

## 第二十一章：WorldSettlements 领域事件名

**活世界铁律**：模块之间通过 `DomainEvent` 对话。`WorldSettlements` 必须有自己的"神经末梢"——它感知季节变化、路线受阻、节点告警时发出事件，其他模块订阅。

### 21.1 `WorldSettlementsEventNames`（MVP 最小集）

```csharp
public static class WorldSettlementsEventNames
{
    // 自然轴节律
    public const string SeasonPhaseAdvanced     = "WorldSettlements.SeasonPhaseAdvanced";
    public const string CanalWindowChanged      = "WorldSettlements.CanalWindowChanged";

    // 政府轴节律
    public const string CorveeWindowChanged     = "WorldSettlements.CorveeWindowChanged";
    public const string ComplianceModeShifted   = "WorldSettlements.ComplianceModeShifted";

    // 皇权轴节律（决策 I 第三轴）
    public const string ImperialRhythmChanged   = "WorldSettlements.ImperialRhythmChanged";  // 国丧 / 大赦 / 储位摇动 band 值跨阈

    // 路线动态
    public const string RouteConstraintEmerged  = "WorldSettlements.RouteConstraintEmerged";  // 淤 / 塌 / 汛 / 匪 / 雪封
    public const string RouteConstraintCleared  = "WorldSettlements.RouteConstraintCleared";

    // 节点告警
    public const string FloodRiskThresholdBreached   = "WorldSettlements.FloodRiskThresholdBreached";
    public const string EmbankmentStressAlert        = "WorldSettlements.EmbankmentStressAlert";

    // 可见性 / 合法性动态（决策 H）
    public const string NodeVisibilityDiscovered = "WorldSettlements.NodeVisibilityDiscovered";  // 隐节点被官府发现
    public const string IllicitRouteExposed      = "WorldSettlements.IllicitRouteExposed";       // 走私廊被查获

    // 力家族驻扎（空间骨骼只读其他模块 state，发事件方便下游订阅）
    public const string ForceStationChanged     = "WorldSettlements.ForceStationChanged";

    // 节令
    public const string SeasonalFestivalArrived = "WorldSettlements.SeasonalFestivalArrived";
}
```

### 21.2 事件 EntityKey 规范

| 事件 | EntityKey |
|---|---|
| `SeasonPhaseAdvanced` / `CanalWindowChanged` / `CorveeWindowChanged` | `"world"`（全图级） |
| `ImperialRhythmChanged` | `"imperial"`（朝廷轴级） |
| `ComplianceModeShifted` | `RouteId.Value` |
| `RouteConstraintEmerged` / `Cleared` | `RouteId.Value` |
| `IllicitRouteExposed` | `RouteId.Value` |
| `FloodRiskThresholdBreached` / `EmbankmentStressAlert` | `SettlementId.Value`（最先超阈的节点） |
| `NodeVisibilityDiscovered` | `SettlementId.Value` |
| `ForceStationChanged` | `SettlementId.Value` |
| `SeasonalFestivalArrived` | `festivalKey`（例如 `"qingming"`） |

### 21.3 订阅方预期（跨模块合约）

| 事件 | 预期订阅方 | 预期响应 |
|---|---|---|
| `SeasonPhaseAdvanced` | `PopulationAndHouseholds` | 调整 labor 分配 |
| `CanalWindowChanged` | `TradeAndIndustry` | 调整漕运 route 使用 |
| `CorveeWindowChanged` | `PopulationAndHouseholds` / `OfficeAndCareer` | 征徭役 / 发告示 |
| `ImperialRhythmChanged` | `OfficeAndCareer` / `NarrativeProjection` | 朝廷大事件上案头、中止日常 |
| `ComplianceModeShifted` | `OfficeAndCareer` / `NarrativeProjection` | 重算下达到达率 |
| `RouteConstraintEmerged` | `TradeAndIndustry` / `OrderAndBanditry` | 切换备选路线 / 增派护卫 |
| `IllicitRouteExposed` | `OrderAndBanditry` / `OfficeAndCareer` | 抓捕、悬赏、处分 |
| `NodeVisibilityDiscovered` | `OrderAndBanditry` | 巡检司介入 |
| `FloodRiskThresholdBreached` | `NarrativeProjection` | Urgent 通知上案头 |
| `ForceStationChanged` | `OrderAndBanditry` / `ConflictAndForce` | 调整 bandit 评估 / 护送能力 |
| `SeasonalFestivalArrived` | `PublicLifeAndRumor` / `NarrativeProjection` | 市集脉冲、谣传扩散 |

**Phase 1c 只要求 `WorldSettlements` 发这些事件**。订阅方的实际响应留给后续 phase。`ForceStationChanged` 的发源数据来自 `ConflictAndForce` 模块 query，`WorldSettlements` 只做事件中转（保证空间骨骼能成为 desk sandbox 的统一事件源）。

### 21.4 与已有事件命名对称

与 `FamilyCoreEventNames` / `PersonRegistryEventNames` / `DeathCauseEventNames` 的命名风格对齐：`<模块>.<过去式动词短语>`。

---

## 第二十二章：活世界自检清单（Liveness Checklist）

**怎么判断世界是"活的"而不是"摆着的"**。这章是 integration test 的契约起点，也是 code review 的 smell detector。

### 22.1 12 个月基线（MVP 必过）

对兰溪种子世界跑 12 个月，以下**必须发生**（integration 测试）：

**自然轴脉动**
| 事件 / 断言 | 最低次数或阈值 |
|---|---|
| `SeasonPhaseAdvanced` | 4 次（5 相位轮回） |
| `CanalWindowChanged` | 2 次（春开 / 秋闭，或类似） |
| `FloodRiskThresholdBreached` 或 `EmbankmentStressAlert` | ≥ 1 次（汛季必 stress） |
| `LaborPinch` 的 12 月均值差异 | 最高月 - 最低月 ≥ 40（必须有忙闲） |
| `MarketCadencePulse` 的旬级波动 | std > 10（必须脉动） |

**政府轴脉动**
| 事件 / 断言 | 最低次数或阈值 |
|---|---|
| `CorveeWindowChanged` | 1 次（至少切进 Pressed 一次） |
| `RouteConstraintEmerged` | 路线级约束至少出现 1 次 |
| `Route(OfficialDispatchRoute).GetRoutes()` 返回的 `Reliability` 方差 | > 0 |

**皇权轴脉动**（决策 I）
| 事件 / 断言 | 最低次数或阈值 |
|---|---|
| `Imperial.MourningInterruption` / `AmnestyWave` / `SuccessionUncertainty` 任一 band 出现非零 | Phase 1c 种子里由 test harness 在 Month 6 注入一次"驾崩"刺激；验证 `ImperialRhythmChanged` 发出、`CorveeWindow` 被强制 Quiet、`MessageDelayBand` 倍增 |
| `ImperialRhythmChanged` | ≥ 1 次（注入刺激后）|

**节令 / 可见面**
| 事件 / 断言 | 最低次数或阈值 |
|---|---|
| `SeasonalFestivalArrived` | 3 次（清明 + 端午 + 中元至少命中） |
| `PublicSurfaceSignal` 覆盖至少 3 条 `OpinionStream` | 证明流竞争不只挂 NoticeBoard |
| 同一 `FloodRiskThresholdBreached` 事件至少产出 2 条 signal（分别挂 NoticeBoard / MarketTalk / TempleWhisper 中至少 2 条） | 流竞争具体断言 |

**Shell-facing readiness**（决策 J）
| 断言 | 阈值 |
|---|---|
| 12 月内任意时刻 `GetCurrentLocus()` 返回非空 | 100% |
| `GetCurrentLocus().ReasonKey` 在整个 12 月内至少变化过 3 种不同值 | 证明 locus 随脉动切换 |
| Notice tray 至少承接过一条 `Tier=Urgent` 的 PublicSurfaceSignal | shell 有告警通道 |

### 22.2 60 个月基线

- 以上所有节律**必须重复**（determinism 守护）
- `CanalWindow` 必须经历完整 `Closed → Limited → Open → Limited → Closed` 至少 4 轮
- 任意 12 个月窗口内，至少有一次 `RouteConstraintEmerged`
- `Route(SmugglingCorridor)` 持续存在且 `Reliability` 在 60 个月内有方差（灰色通道也活）

### 22.3 "死世界"反面症状（code review checklist）

发现以下任一，**视为退化**：

| 症状 | 判据 |
|---|---|
| 脉动消失 | 跑 12 个月，任一 xun 级字段方差为 0 |
| 窗口卡死 | 跑 60 个月 `CanalWindow` 只有一个值 |
| 节点永不告警 | 跑 60 个月 `FloodRiskThresholdBreached` 为 0（兰溪是水网县） |
| 路线永不受阻 | 跑 60 个月 `RouteConstraintEmerged` 为 0 |
| 节令无显形 | `SeasonalFestivalArrived` 不带动任何 `MarketCadencePulse` 变化 |
| **单轴世界** | 60 月内 `ImperialBand` 任一字段从未被触动（空壳三轴就是退化到一轴） |
| **流合并** | 一次事件仅产出 1 条 signal 且只挂 `NoticeBoard`（流竞争失败） |
| **Locus 卡点** | 12 月内 `GetCurrentLocus().ReasonKey` 从未变化（locus 未跟随脉动） |
| 隐节点显形 | `GetSettlementsByVisibility(Covert)` 在 60 月内返回空（兰溪种子有芦滩盐窝） |
| 灰色路线消失 | `GetRoutesByLegitimacy(Illicit)` 在 60 月内返回空 |
| 事件无订阅 | `WorldSettlementsEventNames` 的事件从未被任何模块消费（随 Phase 1c+1 检查） |

### 22.4 自检测试文件

新增 `tests/Zongzu.Integration.Tests/LivingWorldLivenessTests.cs`，按 22.1 / 22.2 / 22.3 写断言。这是 Phase 1c 的**收官测试**：spec 的"活"字能不能落地，看这个文件跑不跑得过。

测试 harness 必须提供一个 `InjectImperialEvent(kind, month)` 辅助，让皇权轴断言可以在 Month 6 注入"驾崩"，否则皇权轴永远不会自己动（决策 I：皇权轴不由 `WorldSettlements` 主动推进）。

---

## 第二十三章：冲突阶梯的空间锚点（Conflict Scale Anchors）

基于 `conflict-scale-ladder.md` + `small-clash-vs-major-campaign.md` + `force-families-differentiation.md`。

**活世界铁律**：不是每次冲突都该升级到 campaign board。空间骨骼必须为五级冲突阶梯分别提供 anchor：

### 23.1 五级阶梯与 anchor 契约

| 阶梯 | skill pack 定义 | 空间 anchor（WorldSettlements 提供） |
|---|---|---|
| 1. Social-Pressure Layer | 威胁 / 可见准备 / 谣传 | 单节点 `hotspot` 标记（`SettlementStateData` 的 `Security`、`PublicSurfaceSignal` stream）|
| 2. Local Conflict Vignette | 单点 / 乡间 / 族间爆发 | **单节点 `SettlementId`** 作为 primary anchor |
| 3. Tactical-Lite Encounter | 河口 / 关前 / 庄前 | **节点 + 邻接 route** 组合（`SettlementId` + `RouteId`） |
| 4. Campaign Board | 跨县 / 跨路 / 粮线战 | **多节点 + 多路线 + 前线**（`List<SettlementId>` + `List<RouteId>` + front marker） |
| 5. Aftermath Return | 冲突落回日常 | **被触动节点的 `PublicSurfaceSignal` + 跨月 residue** |

### 23.2 各 NodeKind 可承载的冲突级别

| NodeKind | 可作为 anchor 的阶梯 |
|---|---|
| `Village` / `EstateCluster` / `MarketTown` | 1, 2 |
| `Ferry` / `Wharf` / `Bridge` / `Ford` | 1, 2, 3 |
| `Pass` / `BorderWatch` / `Garrison` / `Depot` | 1, 2, 3, 4（可做 campaign front）|
| `CountySeat` / `PrefectureSeat` | 1, 4（campaign 司令 / 后勤中心），**不承载** 2/3 |
| `LineageHall` / `Academy` / `Temple` | 1, 2（祠堂清议、庙前械斗）|
| `CovertMeetPoint` / `SmugglingCache` | 1, 2（隐性冲突，但不上公共面） |
| `RelayPost` / `WellPost` / `HillShrine` | 1, 2 |
| `FrontierCamp` | 1, 2, 3（移民冲突常为 tactical-lite） |

### 23.3 `ConflictAnchor` query（Phase 1c+N 实现，Phase 1c 登记）

```csharp
public sealed record ConflictAnchor(
    int Scale,                              // 1-5
    IReadOnlyList<SettlementId> Nodes,      // 至少 1 个
    IReadOnlyList<RouteId> Routes,          // tactical-lite+ 必须 >= 1
    string KindKey);                        // "escort-ambush" / "pass-defense" / ...

public interface IConflictAnchorQueries
{
    // 给定一个 ConflictAnchor，验证它在空间骨骼上的节点 / 路线组合是否合法
    bool IsAnchorValid(ConflictAnchor anchor);
}
```

**Phase 1c 不实现**，仅登记签名。Phase 6 `WarfareCampaign` / Phase 5 `ConflictAndForce` 实装。

### 23.4 "每次冲突都走 campaign board" 红线

基于 `small-clash-vs-major-campaign.md`："do not use the campaign board for every family clash, escort fight, raid, arrest, or market incident that should stay at a smaller scale"。

code review 检查：`ConflictAndForce` / `WarfareCampaign` 模块若把 scale 1-3 的事件挂到 campaign board anchor，**视为退化**。

---

## 第二十四章：未来接口预留

以下接口 / 枚举在 Phase 1c 登记签名，不实装，留给后续 phase 实装时无需改 spec：

### 24.1 `CulturalRegion`（基于 `local-culture-customs-regional-identity.md`）

```csharp
public enum CulturalRegion
{
    Unspecified = 0,
    JiangnanLiterati = 1,      // 江南士风：书香、清议、商学并行
    NorthPlainMartial = 2,     // 北地尚武：边土、马道、朴直
    SichuanBasinMixed = 3,     // 蜀地混融：茶盐、家族、山民
    HuguangFrontier = 4,       // 湖广新辟：移民、苗汉混居
    MinYueCoastal = 5,         // 闽粤海疆：商贾、海道、宗族
}
```

影响 `OpinionStream` 的 `TrustWeight`（外人入本区，本地流对其信号权重衰减）。Phase 1c 不用，Phase 4+ `SocialMemoryAndRelations` 合作时启用。

### 24.2 `PlayerInvestmentLevel`（基于 `command-resolution-and-bounded-leverage.md`）

```csharp
public sealed class RouteInvestment
{
    public RouteId RouteId;
    public int PlayerInvestmentLevel;  // 0-100：玩家对该路线的持续投入
    public string InvestmentKindKey;   // "护送加派" / "修桥" / "赈沿途" / "通关打点"
}
```

Phase 1c 登记但 `RouteState` 不含此字段（投入是玩家指令结果，不是 route 自身属性）。玩家动作窗口在 Phase 1d+ 落地。

### 24.3 `ForceStation`（基于 `force-families-differentiation.md`）

```csharp
public sealed class ForceStationSnapshot
{
    public SettlementId NodeId;
    public string ForceFamilyKey;   // "jiading" / "lineage_guard" / "escort_band" / ...
    public int Strength;             // 0-100
    public ComplianceMode AlignedWith;  // 对下达链的态度
}
```

只读 snapshot，不进 `WorldSettlementsState`。实际 state 归 `ConflictAndForce` / `FamilyCore` / `OrderAndBanditry`。空间骨骼仅在 Phase 1c 登记签名。

### 24.4 `ImperialEventInjection`（测试用，Phase 1c 实装）

```csharp
public enum ImperialEventKind
{
    EmperorMourning = 1,
    GrandAmnesty = 2,
    SuccessionCrisis = 3,
    CourtFactionOverturn = 4,
}

public interface IImperialEventTestHarness
{
    void Inject(ImperialEventKind kind, int intensity);
}
```

仅在 test harness 暴露；production build 不暴露。用于第 22 章皇权轴断言。

---

## 第二十五章：与其他已落地骨骼的关系

| 骨骼 | 文档 | 关系 |
|---|---|---|
| 人物身份骨骼 | `PERSON_OWNERSHIP_RULES.md` + Phase 1a/1b | 正交。人物不位于空间字段里；但叠加层和案头读序把人物牵连显形 |
| 族人领域字段 | `PERSON_OWNERSHIP_RULES.md` 方案 B | Phase 2 会把 FamilyCore 的 person state 迁入分布式模式；需要读 WorldSettlements 的节点才能定位（如 LineageHall） |
| 生计骨骼 | Phase 3（待） | PopulationAndHouseholds 的 `SettlementId` 已存在，Phase 3 会加生计类型；空间骨骼是其前置 |
| 记忆骨骼 | Phase 4（待） | SocialMemoryAndRelations 的事件记忆可绑定 `SettlementId` / `RouteId` 锚点，`OpinionStream` heat + ShameResidue 也归此模块；空间骨骼是其前置 |
| 冲突 / 战役骨骼 | Phase 6（待） | `Routes(MilitaryMoveRoute)` + `Nodes(Pass/Garrison/BorderWatch/Depot)` + 第 23 章冲突阶梯锚点是 WarfareCampaign 的前置 |
| 朝廷骨骼 | Phase 5+（待） | `ImperialBand` 由未来的 `CourtAndThrone` / `WorldEvents` 模块驱动；Phase 1c 仅定义字段与 harness |

**结论**：空间骨骼是 Phase 3 / 4 / 5 / 6 的共同前置，Phase 2（人物领域字段分布）与 Phase 1c 正交但互补。

---

## 第二十六章：术语表（与 skill pack 对齐）

| 中文 | 英文（spec） | 语义 |
|---|---|---|
| 节点 | Settlement Node | 地图上承载压力 / 队列 / 信号的点 |
| 节点分类 | NodeKind | 节点的**功能语义**标签（决策 A） |
| 行政等级 | Tier | 节点在国家行政层级上的位置 |
| 节点可见性 | NodeVisibility | StateVisible / LocalKnown / Covert（决策 H） |
| 生态区 | SettlementEcoZone | 江南水网 / 北地旱路 / 山地边 / 边郡堡带（决策 I） |
| 路线 | Route | 社会流动通道 |
| 路线分类 | RouteKind | 路线的社会功能标签 |
| 路线介质 | RouteMedium | 路线的物理形态 |
| 路线合法性 | RouteLegitimacy | Official / Tolerated / GrayZone / Illicit（决策 H） |
| 合规模式 | ComplianceMode | 下达路线的实际到达质量（决策 I） |
| 走廊 | Corridor | 同一地理轨迹上可叠加多条 Route |
| 案头 | Desk Sandbox | 玩家的月度决策界面 |
| 案头读序 | Desk Sandbox Read Order | locus → action → consequence → background（决策 J 外壳）|
| 溯因链 | Causal Chain | 节点 → 路线 → 户房官 → 公共面（决策 J 内核）|
| 当前焦点 | Current Locus | Desk sandbox 上当前最该关注的节点 / 路线 |
| 叠加 | Overlay | 跨模块 query 合成的地图压力视图（不入 state） |
| 带 | Band | 压力值的命名区间（决策 D） |
| 三轴节律 | Three-Axis Rhythm | 自然 / 政府 / 皇权（决策 I） |
| 皇权节律带 | ImperialBand | 国丧 / 大赦 / 储位 / 天命 / 朝局五带（决策 I） |
| 水陆交接 | Water-Land Interface | Ferry/Wharf/Bridge/Ford/CanalJunction |
| 漕运 | Grain-Canal Transport | `GrainRoute` × `WaterCanal` 的常见组合 |
| 走私廊 | SmugglingCorridor | `RouteKind.SmugglingCorridor`，灰色拓扑 |
| 逃丁路 | FugitivePath | `RouteKind.FugitivePath`，隐性个人流动 |
| 隐节点 | Covert Node | `CovertMeetPoint` / `SmugglingCache`，默认 Covert 可见性 |
| 举子路 | Exam Travel Route | `RouteKind.ExamTravelRoute` |
| 呈文路 | Petition Route | `RouteKind.PetitionRoute` |
| 徭役路 | Corvee Route | `RouteKind.CorveeRoute`（新增） |
| 节气 | Agrarian Phase | `AgrarianPhase` 枚举的五状态 |
| 漕运窗 | Canal Window | 漕运是否可通 |
| 徭役窗 | Corvee Window | 可征徭役的节奏 |
| 脉动 | Pulse | xun 级节律性波动 |
| 压力传导 | Pressure Flow | 节点发源 → 路线衰减 → 消费者感知 的跨模块 query 链 |
| 意见流 | OpinionStream | 榜示 / 市井 / 茶话 / 庙堂 / 族堂五条独立竞争流（§20） |
| 羞辱残留 | ShameResidue | 跨月负面意见残留，归 SocialMemoryAndRelations |
| 公共可见面 | Public Surface | 让隐藏压力显形的节点（通过 OpinionStream 承载） |
| 节令 | Seasonal Festival | 清明 / 端午 / 中元 / 秋社 / 腊八 |
| 冲突阶梯 | Conflict Scale Ladder | 五级冲突尺度（§23） |
| 冲突锚点 | Conflict Anchor | 冲突在空间骨骼上的 node / route 组合（§23）|
| 力家族 | Force Family | jiading / lineage_guard / escort_band / militia / yamen_force 等 |
| 神经末梢 | Domain Event | `WorldSettlementsEventNames` 下模块对外广播 |
| 活世界自检 | Liveness Check | 第 22 章 12/60 月节律断言 |

---

## 第二十七章：壳层对象锚点与 2.5D 深度契约（UI-Shell 收敛）

空间骨骼不是纯后端数据结构。它必须能在 Unity 壳层中**被玩家看见、触摸、解读**。本章把 backend spec 收敛到 `VISUAL_FORM_AND_INTERACTION.md` 和 `UI_AND_PRESENTATION.md` 的对象锚点系统。

### 27.1 从 NodeKind 到 Object Anchor 的映射

每个 `SettlementNodeKind` 在壳层中必须对应一个**物理对象**，不是浮动图标：

| NodeKind | Shell object anchor | 2.5D depth hint | Player touch result |
|----------|--------------------|-----------------|---------------------|
| `CountySeat` | 县衙模型（微缩，置于沙盘边缘） | 中景（desk sandbox 边缘） | 打开衙门案牍面板 |
| `MarketTown` | 市镇聚落块（带市集旗） | 中景 | 打开市集节律 + 牙行消息 |
| `Village` | 村舍群（更小，带炊烟动画） | 远景（desk sandbox 深处） | 打开村情（徭役、收成、佃户） |
| `LineageHall` | 祠堂建筑（玩家宗族为可交互主模型） | 近景（desk 近边缘） | 跳转祖堂表面 |
| `Granary` | 粮仓（圆顶 / 方仓） | 中景 | 打开粮储 + 赈济开关 |
| `Ferry` | 渡口（小船 + 码头） | 中景 | 打开过渡队列 + 水陆交接信息 |
| `Temple` | 庙宇（塔 / 殿） | 中景 | 打开节庆 + 布施 + 谣传 |
| `Academy` | 书院（门楼 + 讲堂） | 中景 | 打开科场流 + 师徒网 |
| `CovertMeetPoint` | **不直接显示**；通过 rumor 或情报间接显形 | — | 不直接触摸 |
| `SmugglingCache` | **不直接显示**；通过 `OrderAndBanditry` 情报或查获事件显形 | — | 不直接触摸 |

### 27.2 Route 的壳层表现

路线在 desk sandbox 上是**物理条带**，不是画线：

- `LandRoad` → 土路纹理条带，宽度和颜色随 `Reliability` 变化
- `WaterRiver` / `WaterCanal` → 水道纹理条带，冬季可能结冰变白
- `FerryLink` → 渡口处有船形桥接物
- `MountainPath` → 更窄、更高 z 的条带（山径感）

路线条带的颜色 = `Reliability` 映射：
- 70-100：土黄 / 水蓝（正常）
- 40-70：赭色（有风险）
- <40：暗红或断线（危险 / 不可通）

`SmugglingCorridor` / `FugitivePath` **不绘制为可见条带**。它们的存在通过节点间 rumor 连接或情报纸片间接表示。

### 27.3 前景 / 动作 / 背景三层分离（Desk Sandbox）

Desk sandbox 必须遵守 `VISUAL_FORM_AND_INTERACTION.md` 的表面语法三层：

**前景层（Foreground）**
- 位置：沙盘中央偏玩家侧（z 最近）
- 内容：`GetCurrentLocus()` 返回的节点 + 1–3 个关联命令标记
- 表现：节点被轻微抬高（pop-up），周围有 Focus Ring 光晕；命令标记是小型令牌（可拾取）

**动作层（Action）**
- 位置：沙盘中央
- 内容：Consequence Context 面板（展开的内核溯因链）
- 表现：打开的书卷或折叠面板，从 focus 节点延伸出来；显示节点压力 → 路线传导 → 户房官牵连

**背景层（Background）**
- 位置：沙盘深处（z 最远）+ 沙盘外墙面
- 内容： ambient 季节带、远处节点脉动、路线热纹
- 表现：季节带以天空色或墙面色渐变表示；远处节点有微弱呼吸动画

### 27.4 玩家触摸契约

玩家与 desk sandbox 的交互必须是**物理触摸**，不是鼠标悬停：

1. **单指触摸节点** → 节点轻微下沉 → 弹出 foreground 信息（当前焦点 + 可用命令）
2. **双指捏合** → 缩放沙盘（但缩放范围受限，不能变成平面地图）
3. **滑动路线** → 路线高亮，显示 route 详情（但不打开新屏幕）
4. **长按节点** → 打开 consequence context 书卷（动作层）
5. **拖动命令令牌到节点** → 触发对应命令（如把 "护送" 令牌拖到路线上）
6. **按压沙盘边缘的 seal** → 确认本月决策，结束玩家回合

### 27.5 2.5D 深度规范

Desk sandbox 不是 2D 地图。它是**有深度的微缩场景**：

- **z 分层**：前景（玩家侧）> 动作层 > 中景（主要节点）> 远景（边缘节点）> 背景墙
- **高度**：县衙 > 市镇 > 村庄（行政等级对应物理高度，暗示权力层级）
- **透视**：微缩透视（miniature diorama），不是正交投影。有轻微的俯视角度（约 30°）
- **比例**：节点间距离被压缩（兰溪 9 节点在 1m × 0.6m 沙盘内），但相对位置正确
- **材质**：沙盘底座 = 木质桌面 + 沙面纹理；节点 = 陶瓷 / 木质微缩建筑；路线 = 布条或画线

### 27.6 与 NarrativeProjection 的接缝

`NarrativeProjection` 产出的通知**必须映射到物理对象**：

- Urgent notice → 红色 notice pin 插到对应节点上
- Consequential notice → 白色 notice pin
- Background rumor → 淡色纸片贴到沙盘边缘
- 通知被玩家阅读后 → pin 变灰但不消失（留下阅读痕迹）

### 27.7 壳层反模式（禁止）

| 反模式 | 说明 | 检测方法 |
|--------|------|----------|
| 2D 平面地图 | 节点变成地图上的平面图标 | 检查是否有 z 分层 |
| 浮动 HUD | 信息框悬浮在空中，不锚定对象 | 检查每个 UI 元素是否有物理父对象 |
| 无限缩放 | 沙盘可缩放到失去比例感 | 检查缩放范围是否受限 |
|  omnipresent status bar | 顶部/底部有常驻状态条 | 检查是否有全局 HUD；信息应在对象上 |
| 点击即全屏 | 触摸节点后打开全屏新界面 | 检查信息是否在沙盘内展开 |
| 路线 = 纯线段 | 路线没有宽度、材质、季节变化 | 检查 route renderer 是否有 texture 和 width |

---

*本文档定义空间骨骼的静态形状与契约。规则密度（路线实际传导、季节实际影响、叠加实际数值、节点间压力耦合）在 Phase 1c 之后按 `LIVING_WORLD_DESIGN.md` 第四章规则密度填充顺序逐项扩展。*
