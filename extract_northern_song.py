#!/usr/bin/env python3
"""
从 CHGIS V6 Time Series 数据中提取北宋时期 (960-1127) 的数据切片
"""

import json
from pathlib import Path

# 北宋起止年份
NORTHERN_SONG_START = 960
NORTHERN_SONG_END = 1127

def extract_northern_song_features(geojson_path: Path) -> list[dict]:
    """提取北宋时期有效的地理要素"""
    with open(geojson_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    features = data.get('features', [])
    ns_features = []
    
    for feat in features:
        props = feat.get('properties', {})
        beg_yr = props.get('BEG_YR')
        end_yr = props.get('END_YR')
        
        # 跳过无效记录
        if beg_yr is None or end_yr is None:
            continue
        
        # 筛选条件：记录在北宋期间内存在
        # 即 BEG_YR <= 1127 且 END_YR >= 960
        if beg_yr <= NORTHERN_SONG_END and end_yr >= NORTHERN_SONG_START:
            ns_features.append(feat)
    
    return ns_features


def create_ns_geojson(input_path: Path, output_dir: Path, data_type: str):
    """创建北宋数据 GeoJSON"""
    features = extract_northern_song_features(input_path)
    
    if not features:
        print(f"  [警告] {input_path.name} 中未找到北宋数据")
        return None
    
    output_path = output_dir / f"ns_{data_type}.geojson"
    
    geojson = {
        "type": "FeatureCollection",
        "name": f"CHGIS_V6_Northern_Song_{data_type}",
        "description": f"北宋时期 (960-1127) {data_type}数据，提取自 CHGIS V6 Time Series",
        "source": "CHGIS V6, Harvard University & Fudan University",
        "time_range": {"start": 960, "end": 1127},
        "features": features
    }
    
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(geojson, f, ensure_ascii=False, separators=(',', ':'))
    
    size = output_path.stat().st_size / 1024
    print(f"  完成: {output_path.name} ({len(features)} 条记录, {size:.1f} KB)")
    return output_path


def main():
    base_dir = Path("/root/.openclaw/workspace/chgis_geojson")
    output_dir = base_dir / "northern_song"
    output_dir.mkdir(exist_ok=True)
    
    # 数据源映射
    sources = {
        "county_points": base_dir / "time_series" / "v6_time_cnty_pts_utf_wgs84.geojson",
        "prefecture_points": base_dir / "time_series" / "v6_time_pref_pts_utf_wgs84.geojson",
        "prefecture_polygons": base_dir / "time_series" / "v6_time_pref_pgn_utf_wgs84.geojson",
    }
    
    print("=" * 60)
    print("提取北宋时期数据 (960-1127)")
    print("=" * 60)
    print(f"输出目录: {output_dir}")
    print()
    
    results = {}
    for data_type, src_path in sources.items():
        if not src_path.exists():
            print(f"[跳过] 源文件不存在: {src_path}")
            continue
        
        print(f"[{data_type}]")
        result = create_ns_geojson(src_path, output_dir, data_type)
        if result:
            results[data_type] = str(result)
    
    # 生成统计信息
    print("\n" + "=" * 60)
    print("北宋数据提取完成")
    print("=" * 60)
    
    for data_type, path in results.items():
        p = Path(path)
        with open(p, 'r', encoding='utf-8') as f:
            data = json.load(f)
        count = len(data['features'])
        print(f"  {data_type}: {count} 条记录")
    
    # 生成 README
    readme = output_dir / "README.txt"
    with open(readme, 'w', encoding='utf-8') as f:
        f.write("北宋时期历史地理数据 (960-1127 CE)\n")
        f.write("=" * 50 + "\n\n")
        f.write("数据来源: CHGIS V6 Time Series\n")
        f.write("提取规则: BEG_YR <= 1127 且 END_YR >= 960\n\n")
        f.write("文件清单:\n")
        for data_type, path in results.items():
            p = Path(path)
            with open(p, 'r', encoding='utf-8') as ff:
                data = json.load(ff)
            f.write(f"  - {p.name}: {len(data['features'])} 条记录\n")
    
    print(f"\nREADME 已生成: {readme}")


if __name__ == "__main__":
    main()
