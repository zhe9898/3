# CHGIS Unity 沙盘渲染系统

Unity C# 脚本集，用于渲染中国历史地理信息系统（CHGIS）V6 数据。

## 文件清单

| 脚本 | 功能 |
|------|------|
| `CHGISDataManager.cs` | 加载 GeoJSON/TopoJSON，按年份索引 |
| `BoundaryRenderer.cs` | 渲染政区边界（Polygon → LineRenderer） |
| `SettlementRenderer.cs` | 渲染治所点（Point → Sprite + TextMeshPro） |
| `RiverRenderer.cs` | 渲染河流（LineString，支持 Strahler 分级） |
| `TimeAxisController.cs` | 时间轴控制，年份切换管理 |
| `MiniMapController.cs` | 小地图（正交相机 + RenderTexture） |

## 快速开始

### 1. 数据准备

将 `chgis_geojson/` 下的数据复制到 Unity `StreamingAssets/CHGIS/` 目录：

```
Assets/
└── StreamingAssets/
    └── CHGIS/
        ├── ns_county_points.geojson
        ├── ns_prefecture_points.geojson
        ├── ns_prefecture_polygons.geojson
        └── v6_1820_coded_rvr_lin_utf.geojson
```

### 2. 场景设置

创建空物体 `CHGISRoot`，挂载以下组件：

```
CHGISRoot
├── CHGISDataManager          ← 数据管理器
├── BoundaryRenderer          ← 边界渲染器
├── SettlementRenderer        ← 治所渲染器
├── RiverRenderer             ← 河流渲染器
└── TimeAxisController        ← 时间轴控制器
    ├── BoundaryRenderer (引用)
    ├── SettlementRenderer (引用)
    └── RiverRenderer (引用)
```

### 3. 小地图设置

```
Canvas
└── MiniMapPanel
    ├── RawImage                ← MiniMapController.miniMapImage
    └── ViewPortRect (Image)    ← 视口指示框

MiniMapCamera (Camera)
├── orthographic = true
├── targetTexture = RenderTexture (512x512)
└── MiniMapController 脚本
```

### 4. 代码控制

```csharp
// 设置年份
TimeAxisController.Instance.SetYear(960);

// 跳转到朝代
TimeAxisController.Instance.JumpToDynasty(Dynasty.NorthernSong);

// 逐年播放
TimeAxisController.Instance.StepForward();
```

## 依赖

- **Newtonsoft.Json** (Unity Package Manager)
- **TextMeshPro** (Unity 内置)

## 数据来源

CHGIS V6, Harvard University & Fudan University
License: Free for academic research

## 坐标说明

- 输入: WGS84 (EPSG:4326), 经纬度
- 输出: Unity 世界坐标 (x=经度*scale, z=纬度*scale, y=0)
- 默认 scaleFactor: 0.001
