# Zongzu Copilot Instructions

先读最相关的本地 skill，不要一次性吞完整仓库。

## 优先读取
- 历史/社会/北宋/宗族/地方秩序：`.github/skills/zongzu-ancient-china/SKILL.md`
- 规则/模拟/主循环/压力链/MVP：`.github/skills/zongzu-game-design/SKILL.md`
- 大堂/案头/沙盘/UI 壳：`.github/skills/zongzu-ui-shell/SKILL.md`

只有在需要时，再读这些总文档：
- `docs/FULL_SYSTEM_SPEC.md`
- `docs/ARCHITECTURE.md`
- `docs/ENGINEERING_RULES.md`

## 项目铁律
- 这是一个北宋启发的、多路线、规则驱动的中国古代活社会模拟。
- 世界先自运行，玩家后介入；玩家只有有限干预，不是上帝控制。
- 不要把项目做成事件池游戏、纯宗族经理、海报式 UI、或平台软件味很重的工具界面。

## 后端架构
- 后端是模块化单体：一个调度器、一个存档根、多个权威模块。
- 模块只改自己的 state；跨模块只走 Query / Command / DomainEvent。
- Application 保持薄，只做编排和路由，不要长成第二套规则引擎。
- 先稳定结构和合同，再加深规则：ownership、cadence、save、migration、projection 先立住。
- 使用 `xun / month / seasonal` 节拍：月是 review 壳，旬是生活脉冲。

## 代码要求
- 高内聚、低耦合；一个文件只承载一个主要职责。
- 非生成代码文件超过 400 行就检查是否职责混杂，超过 600 行默认拆分。
- authority hot path 禁止阻塞文件 IO、阻塞网络 IO、隐藏时钟、弱胶水层。
- 不要把 debug/internal data、stack trace、绝对路径、未来信息泄露到玩家可见层。

## UI 与投影
- UI / read model / wording 都是权威模拟的下游投影，不能决定权威结果。
- 地图和壳优先做物件锚定的 2.5D 大堂 / 案头 / 沙盘，不做平铺 dashboard。
