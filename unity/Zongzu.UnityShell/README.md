# Zongzu Unity 壳

这是 Zongzu 的本地 Unity 表现壳工程。

## 用途

- 在 Unity 里手搓可玩的 UI 壳层
- 让壳层与权威 `net8.0` 模拟库保持分离
- 让 Unity 侧对齐 `大堂`、`祠堂/宗谱面`、`宏观沙盘`、`案头沙盘`、`告示托盘`、`冲突剪影` 这些核心表面

## 当前定位

- Unity 版本：`6000.3.13f1`
- 渲染管线：`URP`
- 目标平台：`Windows desktop`
- 出包后端：`IL2CPP`

## 重要约束

不要让 Unity 场景变成权威逻辑层。

- 权威模拟仍然留在 `src/`
- Unity 壳只消费导出的读模型、投影、或 shell-safe DTO 快照
- 如果后面要加直接桥接，把它当成显式兼容任务，不要偷着跨层互引

## 第一次打开

编辑器已经装好，但第一次交互式打开时，Unity 可能会要求你先登录或激活许可。

编辑器路径：

- `C:\Users\Xy172\Unity\Hub\Editor\6000.3.13f1\Editor\Unity.exe`

工程路径：

- `E:\zongzu_codex_spec_modular_rebuilt\unity\Zongzu.UnityShell`

## 建议优先工作的目录

- `Assets/Scenes`
- `Assets/Scripts/Shell`
- `Assets/Scripts/ReadModels`
- `Assets/UI`
- `Assets/Prefabs`
- `Assets/Art`
- `Assets/StreamingAssets/ReadModels`

如果你准备先做大地图沙盘，先看：

- `Assets/UI/MacroSandbox`
- `Assets/Prefabs/MacroSandbox`
- `Assets/Art/MacroSandbox`
- `Assets/StreamingAssets/ReadModels/macro-sandbox-snapshot.json`

## 命名约定

Unity 壳层的目录、文件名前缀、中文场景层级规则见：

- `ASSET_AND_SCENE_NAMING.md`
