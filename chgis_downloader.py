#!/usr/bin/env python3
"""
CHGIS V6 自动下载 + 转换脚本
下载中国历史地理信息系统 V6 数据，并将 Shapefile 转为 GeoJSON

用法:
    python3 chgis_downloader.py [--download-dir DIR] [--output-dir DIR] [--skip-download] [--skip-convert]

依赖:
    - Python 3.7+
    - GDAL/OGR (ogr2ogr 命令行工具)
    - 网络连接

数据源: Harvard Dataverse CHGIS Repository
https://dataverse.harvard.edu/dataverse/chgis_v6
"""

import argparse
import json
import os
import shutil
import subprocess
import sys
import urllib.request
import zipfile
from pathlib import Path
from urllib.error import HTTPError

# ============================================================
# 数据集配置 - CHGIS V6 文件清单
# ============================================================

# Dataverse 基础下载 URL
DATAVERSE_DOWNLOAD_URL = "https://dataverse.harvard.edu/api/access/datafile/"

# 数据集分组定义
DATASETS = {
    "time_series": {
        "name": "时间序列数据 (221 BCE - 1911 CE)",
        "files": [
            {"id": 3048165, "name": "v6_time_cnty_pts_utf_wgs84.zip", "desc": "县级治所点时间序列"},
            {"id": 2970286, "name": "v6_time_pref_pts_utf_wgs84.zip", "desc": "府级治所点时间序列"},
            {"id": 2966510, "name": "v6_time_pref_pgn_utf_wgs84.zip", "desc": "府级边界时间序列"},
        ]
    },
    "time_slice_1820": {
        "name": "1820年截面数据",
        "files": [
            {"id": 2966719, "name": "v6_1820_cnty_pts_utf.zip", "desc": "1820县级治所点"},
            {"id": 2966725, "name": "v6_1820_pref_pts_utf.zip", "desc": "1820府级治所点"},
            {"id": 2966723, "name": "v6_1820_prov_pts_utf.zip", "desc": "1820省级治所点"},
            {"id": 2966724, "name": "v6_1820_twn_pts_utf.zip", "desc": "1820乡镇点"},
            {"id": 2966717, "name": "v6_1820_pref_pgn_utf.zip", "desc": "1820府级边界"},
            {"id": 2966720, "name": "v6_1820_prov_pgn_utf.zip", "desc": "1820省级边界"},
            {"id": 2966718, "name": "v6_1820_lks_pgn_utf.zip", "desc": "1820湖泊"},
            {"id": 2966716, "name": "v6_1820_coded_rvr_lin_utf.zip", "desc": "1820编码河流"},
        ]
    },
    "time_slice_1911": {
        "name": "1911年截面数据",
        "files": [
            {"id": 2966694, "name": "v6_1911_cnty_pts_utf.zip", "desc": "1911县级治所点"},
            {"id": 2966691, "name": "v6_1911_pref_pts_utf.zip", "desc": "1911府级治所点"},
            {"id": 2966690, "name": "v6_1911_prov_pts_utf.zip", "desc": "1911省级治所点"},
            {"id": 2966687, "name": "v6_1911_twn_pts_utf.zip", "desc": "1911乡镇点"},
            {"id": 2966692, "name": "v6_1911_cnty_pgn_utf.zip", "desc": "1911县级边界"},
            {"id": 2966689, "name": "v6_1911_pref_pgn_utf.zip", "desc": "1911府级边界"},
            {"id": 2966688, "name": "v6_1911_prov_pgn_utf.zip", "desc": "1911省级边界"},
        ]
    },
    "time_slice_1990": {
        "name": "1990年 CITAS 数据",
        "files": [
            {"id": 2966747, "name": "v6_citas90_cnty_pgn_gbk.zip", "desc": "1990县级边界"},
            {"id": 2966748, "name": "v6_citas90_pref_pgn_gbk.zip", "desc": "1990府级边界"},
            {"id": 2966746, "name": "v6_citas90_prov_pgn_gbk.zip", "desc": "1990省级边界"},
        ]
    },
    "dem": {
        "name": "数字高程模型 (DEM)",
        "files": [
            {"id": 3359166, "name": "DEM_QGIS-3_REVISED.zip", "desc": "DEM地形数据(QGIS版)"},
        ]
    }
}

# ============================================================
# 工具函数
# ============================================================

def check_ogr2ogr():
    """检查 ogr2ogr 是否可用"""
    if shutil.which("ogr2ogr") is None:
        print("错误: 未找到 ogr2ogr 命令。")
        print("请安装 GDAL/OGR:")
        print("  Ubuntu/Debian: sudo apt-get install gdal-bin")
        print("  macOS: brew install gdal")
        print("  Windows: 从 https://www.gisinternals.com 下载")
        return False
    return True


def download_file(file_id: int, filename: str, download_dir: Path, timeout: int = 120) -> Path:
    """从 Dataverse 下载单个文件"""
    url = f"{DATAVERSE_DOWNLOAD_URL}{file_id}"
    dest_path = download_dir / filename
    
    if dest_path.exists():
        print(f"  [跳过] {filename} 已存在")
        return dest_path
    
    print(f"  下载: {filename} (ID: {file_id})")
    print(f"    URL: {url}")
    
    # 添加请求头模拟浏览器，避免 403
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Accept": "application/zip,application/octet-stream,*/*",
    }
    
    req = urllib.request.Request(url, headers=headers)
    
    try:
        # 使用 opener 处理重定向和 cookie
        opener = urllib.request.build_opener(
            urllib.request.HTTPRedirectHandler(),
            urllib.request.HTTPCookieProcessor()
        )
        
        with opener.open(req, timeout=timeout) as response:
            with open(dest_path, 'wb') as f:
                shutil.copyfileobj(response, f)
        
        size = dest_path.stat().st_size
        print(f"    完成: {size / 1024:.1f} KB")
        return dest_path
    except HTTPError as e:
        print(f"    错误: HTTP {e.code} - {e.reason}")
        if dest_path.exists():
            dest_path.unlink()
        raise
    except Exception as e:
        print(f"    错误: {e}")
        if dest_path.exists():
            dest_path.unlink()
        raise


def unzip_file(zip_path: Path, extract_dir: Path) -> list[Path]:
    """解压 zip 文件，返回解压出的文件列表"""
    extracted = []
    with zipfile.ZipFile(zip_path, 'r') as zf:
        for member in zf.namelist():
            # 跳过 macOS 的 __MACOSX 目录
            if member.startswith('__MACOSX'):
                continue
            zf.extract(member, extract_dir)
            extracted.append(extract_dir / member)
    return extracted


def find_shapefiles(directory: Path) -> list[Path]:
    """查找目录下的所有 .shp 文件"""
    return sorted(directory.glob("**/*.shp"))


def convert_shapefile_to_geojson(shp_path: Path, output_dir: Path, encoding: str = "UTF-8") -> Path:
    """使用 ogr2ogr 将 Shapefile 转为 GeoJSON"""
    geojson_name = shp_path.stem + ".geojson"
    output_path = output_dir / geojson_name
    
    if output_path.exists():
        print(f"    [跳过] {geojson_name} 已存在")
        return output_path
    
    print(f"    转换: {shp_path.name} -> {geojson_name}")
    
    # 检测源坐标系：如果是投影坐标系（如西安80），需要重投影
    # 不要强制指定 -s_srs，让 ogr2ogr 自动读取 .prj 文件
    cmd = [
        "ogr2ogr",
        "-f", "GeoJSON",
        "-lco", "COORDINATE_PRECISION=6",
        "-t_srs", "EPSG:4326",  # 目标坐标系 WGS84
        str(output_path),
        str(shp_path),
    ]
    
    # 对 GBK 编码文件添加编码参数
    if "gbk" in shp_path.name.lower():
        cmd.extend(["--config", "SHAPE_ENCODING", "GBK"])
    
    try:
        subprocess.run(cmd, check=True, capture_output=True, text=True)
        size = output_path.stat().st_size
        print(f"      完成: {size / 1024:.1f} KB")
        return output_path
    except subprocess.CalledProcessError as e:
        print(f"      错误: ogr2ogr 失败")
        print(f"      {e.stderr}")
        raise


def generate_metadata(download_dir: Path, output_dir: Path, converted_files: dict):
    """生成数据元数据索引文件"""
    metadata = {
        "source": "CHGIS V6 (China Historical Geographic Information System)",
        "publisher": "Harvard University & Fudan University",
        "license": "Free for academic research, no commercial use",
        "citation": "CHGIS, Version: 6. (c) Fairbank Center for Chinese Studies of Harvard University and the Center for Historical Geographical Studies at Fudan University, 2016.",
        "download_url": "https://dataverse.harvard.edu/dataverse/chgis_v6",
        "converted_files": converted_files,
    }
    
    meta_path = output_dir / "metadata.json"
    with open(meta_path, 'w', encoding='utf-8') as f:
        json.dump(metadata, f, ensure_ascii=False, indent=2)
    
    print(f"\n元数据已保存: {meta_path}")
    return meta_path


# ============================================================
# 主流程
# ============================================================

def main():
    parser = argparse.ArgumentParser(description="CHGIS V6 自动下载与转换工具")
    parser.add_argument("--download-dir", default="./chgis_raw", help="原始数据下载目录")
    parser.add_argument("--output-dir", default="./chgis_geojson", help="GeoJSON 输出目录")
    parser.add_argument("--skip-download", action="store_true", help="跳过下载步骤")
    parser.add_argument("--skip-convert", action="store_true", help="跳过转换步骤")
    parser.add_argument("--only", choices=list(DATASETS.keys()), help="仅处理指定数据集")
    args = parser.parse_args()
    
    download_dir = Path(args.download_dir).resolve()
    output_dir = Path(args.output_dir).resolve()
    
    download_dir.mkdir(parents=True, exist_ok=True)
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print("=" * 60)
    print("CHGIS V6 自动下载 + GeoJSON 转换")
    print("=" * 60)
    print(f"下载目录: {download_dir}")
    print(f"输出目录: {output_dir}")
    print()
    
    # 检查 ogr2ogr
    if not args.skip_convert and not check_ogr2ogr():
        sys.exit(1)
    
    converted_files = {}
    failed_downloads = []
    failed_conversions = []
    
    # 选择要处理的数据集
    datasets_to_process = {args.only: DATASETS[args.only]} if args.only else DATASETS
    
    # ===== 步骤 1: 下载 =====
    if not args.skip_download:
        print("【步骤 1】下载原始数据")
        print("-" * 60)
        
        for group_key, group_info in datasets_to_process.items():
            print(f"\n[{group_info['name']}]")
            for file_info in group_info["files"]:
                try:
                    download_file(file_info["id"], file_info["name"], download_dir)
                except Exception as e:
                    print(f"    下载失败: {e}")
                    failed_downloads.append(file_info["name"])
        
        print(f"\n下载完成。失败: {len(failed_downloads)} 个")
        if failed_downloads:
            print(f"  失败文件: {', '.join(failed_downloads)}")
    else:
        print("【步骤 1】跳过下载")
    
    # ===== 步骤 2: 解压与转换 =====
    if not args.skip_convert:
        print("\n【步骤 2】解压并转换为 GeoJSON")
        print("-" * 60)
        
        for group_key, group_info in datasets_to_process.items():
            print(f"\n[{group_info['name']}]")
            
            group_output_dir = output_dir / group_key
            group_output_dir.mkdir(exist_ok=True)
            
            for file_info in group_info["files"]:
                zip_path = download_dir / file_info["name"]
                
                if not zip_path.exists():
                    print(f"  [跳过] {file_info['name']} 未找到")
                    continue
                
                # 解压
                extract_dir = download_dir / file_info["name"].replace(".zip", "")
                if not extract_dir.exists():
                    print(f"  解压: {file_info['name']}")
                    try:
                        unzip_file(zip_path, extract_dir)
                    except Exception as e:
                        print(f"    解压失败: {e}")
                        failed_conversions.append(file_info["name"])
                        continue
                
                # 查找并转换 Shapefile
                shp_files = find_shapefiles(extract_dir)
                if not shp_files:
                    print(f"  [警告] {file_info['name']} 中未找到 .shp 文件")
                    continue
                
                for shp_path in shp_files:
                    try:
                        geojson_path = convert_shapefile_to_geojson(shp_path, group_output_dir)
                        rel_path = geojson_path.relative_to(output_dir)
                        converted_files.setdefault(group_key, []).append({
                            "source": file_info["name"],
                            "shp": shp_path.name,
                            "geojson": str(rel_path),
                            "desc": file_info["desc"],
                        })
                    except Exception as e:
                        print(f"    转换失败: {e}")
                        failed_conversions.append(f"{file_info['name']} / {shp_path.name}")
        
        print(f"\n转换完成。失败: {len(failed_conversions)} 个")
        if failed_conversions:
            print(f"  失败项: {', '.join(failed_conversions)}")
    else:
        print("【步骤 2】跳过转换")
    
    # ===== 步骤 3: 生成元数据 =====
    print("\n【步骤 3】生成元数据索引")
    print("-" * 60)
    generate_metadata(download_dir, output_dir, converted_files)
    
    # ===== 完成摘要 =====
    print("\n" + "=" * 60)
    print("处理完成!")
    print("=" * 60)
    print(f"原始数据: {download_dir}")
    print(f"GeoJSON 输出: {output_dir}")
    
    # 统计
    total_geojson = sum(len(files) for files in converted_files.values())
    print(f"\n总计转换: {total_geojson} 个 GeoJSON 文件")
    for group_key, files in converted_files.items():
        print(f"  {group_key}: {len(files)} 个文件")
    
    if failed_downloads or failed_conversions:
        print(f"\n注意: {len(failed_downloads)} 个下载失败, {len(failed_conversions)} 个转换失败")
        print("请检查网络连接或手动下载失败文件。")


if __name__ == "__main__":
    main()
