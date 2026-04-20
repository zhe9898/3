# Unity 壳命名与目录约定

这份约定只管 **Unity 壳层**。

原则很简单：

- 磁盘路径、资源文件名、Prefab 文件名：尽量用 `ASCII/英文`
- 场景中的根节点、表面节点、玩家看到的标题：用 `中文`
- 不把“中文可读性”强塞进文件系统
- 不把“英文工程味”暴露给玩家壳层

## 目录约定

### UI

- `Assets/UI/Common`
- `Assets/UI/GreatHall`
- `Assets/UI/Lineage`
- `Assets/UI/MacroSandbox`
- `Assets/UI/DeskSandbox`
- `Assets/UI/NoticeTray`
- `Assets/UI/ConflictVignette`
- `Assets/UI/CampaignLite`

### Prefabs

- `Assets/Prefabs/Common`
- `Assets/Prefabs/GreatHall`
- `Assets/Prefabs/Lineage`
- `Assets/Prefabs/MacroSandbox`
- `Assets/Prefabs/DeskSandbox`
- `Assets/Prefabs/NoticeTray`
- `Assets/Prefabs/ConflictVignette`
- `Assets/Prefabs/CampaignLite`

### Art

- `Assets/Art/Common`
- `Assets/Art/GreatHall`
- `Assets/Art/Lineage`
- `Assets/Art/MacroSandbox`
- `Assets/Art/DeskSandbox`
- `Assets/Art/NoticeTray`
- `Assets/Art/ConflictVignette`
- `Assets/Art/CampaignLite`

### Read models

- `Assets/StreamingAssets/ReadModels`
- `Assets/StreamingAssets/ReadModels/Fixtures`

## 文件名前缀

建议按表面走前缀，不要一股脑都叫 `Panel`。

- `GH_` = Great Hall / 大堂
- `LS_` = Lineage Surface / 宗谱面
- `MS_` = Macro Sandbox / 宏观沙盘
- `DS_` = Desk Sandbox / 案头沙盘
- `NT_` = Notice Tray / 告示托盘
- `CV_` = Conflict Vignette / 冲突剪影
- `CB_` = Campaign Board / 战役板
- `SH_` = Shared / 通用壳层

示例：

- `GH_MainTable.prefab`
- `GH_LeadNoticePanel.prefab`
- `LS_BranchColumn.prefab`
- `MS_RouteBand.prefab`
- `DS_NodeMarker.prefab`
- `NT_NoticeCard.prefab`
- `CV_ResultStrip.prefab`
- `SH_PaperCard.mat`

## 场景层级命名

场景里优先中文，这样你进层级面板就能直接工作。

推荐根层级：

- `壳层根`
- `大堂表面`
- `宗谱面`
- `宏观沙盘`
- `案头沙盘`
- `告示托盘`
- `冲突剪影`

对象锚点继续用中文：

- `主焦点`
- `次焦点`
- `背景层`
- `州府节点带`
- `水陆干线`
- `县域入口钉`
- `告示堆`
- `桌面节点`
- `路线标记`
- `来信槽`

## 读模型命名

JSON 文件继续用英文文件名，内容用中文。

示例：

- `bootstrap-shell-readmodel.json`
- `great-hall-snapshot.json`
- `desk-sandbox-snapshot.json`
- `notice-tray-snapshot.json`

## 不建议的命名

不要这样：

- `Panel1`
- `TestUI`
- `Final_Final`
- `New Folder`
- `系统界面新新新`
- `Card`, `Card2`, `Card3`

也不要把文件名直接写成整句中文标题。

## 一句话规则

**文件系统求稳，场景层级求顺手，玩家可见文字求中文。**
