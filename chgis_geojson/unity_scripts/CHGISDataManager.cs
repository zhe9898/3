using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CHGIS
{
    /// <summary>
    /// CHGIS V6 数据管理器
    /// 负责加载 GeoJSON/TopoJSON 并按年份索引
    /// </summary>
    public class CHGISDataManager : MonoBehaviour
    {
        public static CHGISDataManager Instance { get; private set; }

        [Header("数据文件")]
        [Tooltip("StreamingAssets 下的相对路径")]
        public string countyPointsPath = "CHGIS/ns_county_points.geojson";
        public string prefecturePointsPath = "CHGIS/ns_prefecture_points.geojson";
        public string prefecturePolygonsPath = "CHGIS/ns_prefecture_polygons.geojson";
        public string riverPath = "CHGIS/v6_1820_coded_rvr_lin_utf.geojson";

        [Header("坐标转换")]
        public float scaleFactor = 0.001f;
        public Vector3 offset = Vector3.zero;

        // 按年份索引的数据缓存
        public Dictionary<int, List<GeoFeature>> FeaturesByYear { get; private set; } = new();
        public List<GeoFeature> AllFeatures { get; private set; } = new();
        public List<GeoFeature> RiverFeatures { get; private set; } = new();

        public int MinYear { get; private set; } = int.MaxValue;
        public int MaxYear { get; private set; } = int.MinValue;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            LoadAllData();
        }

        public void LoadAllData()
        {
            LoadGeoJSON(countyPointsPath, GeoFeatureType.CountyPoint);
            LoadGeoJSON(prefecturePointsPath, GeoFeatureType.PrefecturePoint);
            LoadGeoJSON(prefecturePolygonsPath, GeoFeatureType.PrefecturePolygon);
            LoadGeoJSON(riverPath, GeoFeatureType.River);

            BuildYearIndex();
            Debug.Log($"[CHGIS] 加载完成: {AllFeatures.Count} 条政区记录, {RiverFeatures.Count} 条河流");
            Debug.Log($"[CHGIS] 年份范围: {MinYear} ~ {MaxYear}");
        }

        void LoadGeoJSON(string relativePath, GeoFeatureType featureType)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[CHGIS] 文件不存在: {fullPath}");
                return;
            }

            string json = File.ReadAllText(fullPath);
            JObject root = JObject.Parse(json);
            JArray features = (JArray)root["features"];

            foreach (JToken f in features)
            {
                var feature = ParseFeature(f, featureType);
                if (feature == null) continue;

                if (featureType == GeoFeatureType.River)
                    RiverFeatures.Add(feature);
                else
                    AllFeatures.Add(feature);
            }
        }

        GeoFeature ParseFeature(JToken token, GeoFeatureType type)
        {
            JObject props = (JObject)token["properties"];
            if (props == null) return null;

            int begYr = props["BEG_YR"]?.Value<int>() ?? -9999;
            int endYr = props["END_YR"]?.Value<int>() ?? 9999;

            var feature = new GeoFeature
            {
                Type = type,
                NameChinese = props["NAME_CH"]?.Value<string>() ?? "",
                NamePinyin = props["NAME_PY"]?.Value<string>() ?? "",
                AdminType = props["TYPE_CH"]?.Value<string>() ?? "",
                BeginYear = begYr,
                EndYear = endYr,
                PresentLocation = props["PRES_LOC"]?.Value<string>() ?? "",
                BeginChangeType = props["BEG_CHG_TY"]?.Value<string>() ?? "",
                EndChangeType = props["END_CHG_TY"]?.Value<string>() ?? "",
                SysID = props["SYS_ID"]?.Value<int>() ?? 0,
                LevelRank = props["LEV_RANK"]?.Value<string>() ?? ""
            };

            // 解析几何
            JObject geom = (JObject)token["geometry"];
            if (geom != null)
            {
                string geomType = geom["type"]?.Value<string>();
                JArray coords = (JArray)geom["coordinates"];

                switch (geomType)
                {
                    case "Point":
                        feature.Points.Add(ParseCoordinate(coords));
                        break;
                    case "LineString":
                        feature.Points = ParseLineString(coords);
                        break;
                    case "Polygon":
                        feature.Polygons = ParsePolygon(coords);
                        break;
                    case "MultiPolygon":
                        feature.Polygons = ParseMultiPolygon(coords);
                        break;
                }
            }

            return feature;
        }

        Vector3 ParseCoordinate(JArray coord)
        {
            float lon = coord[0].Value<float>();
            float lat = coord[1].Value<float>();
            // WGS84 → Unity 坐标: x=经度, z=纬度, y=高度(后续DEM)
            return new Vector3(
                lon * scaleFactor + offset.x,
                0 + offset.y,
                lat * scaleFactor + offset.z
            );
        }

        List<Vector3> ParseLineString(JArray coords)
        {
            List<Vector3> points = new();
            foreach (JToken c in coords)
                points.Add(ParseCoordinate((JArray)c));
            return points;
        }

        List<List<Vector3>> ParsePolygon(JArray coords)
        {
            List<List<Vector3>> rings = new();
            foreach (JToken ring in coords)
                rings.Add(ParseLineString((JArray)ring));
            return rings;
        }

        List<List<List<Vector3>>> ParseMultiPolygon(JArray coords)
        {
            List<List<List<Vector3>>> polys = new();
            foreach (JToken poly in coords)
                polys.Add(ParsePolygon((JArray)poly));
            return polys;
        }

        void BuildYearIndex()
        {
            FeaturesByYear.Clear();
            MinYear = int.MaxValue;
            MaxYear = int.MinValue;

            foreach (var feat in AllFeatures)
            {
                MinYear = Math.Min(MinYear, feat.BeginYear);
                MaxYear = Math.Max(MaxYear, feat.EndYear);

                // 为该记录存在的每一年建立索引
                for (int y = feat.BeginYear; y <= feat.EndYear; y++)
                {
                    if (!FeaturesByYear.ContainsKey(y))
                        FeaturesByYear[y] = new List<GeoFeature>();
                    FeaturesByYear[y].Add(feat);
                }
            }
        }

        /// <summary>
        /// 获取指定年份的所有有效政区
        /// </summary>
        public List<GeoFeature> GetFeaturesForYear(int year)
        {
            if (FeaturesByYear.TryGetValue(year, out var list))
                return list;
            return new List<GeoFeature>();
        }

        /// <summary>
        /// 检查某记录在指定年份是否存在
        /// </summary>
        public bool IsFeatureActive(GeoFeature feat, int year)
        {
            return feat.BeginYear <= year && year <= feat.EndYear;
        }
    }

    public enum GeoFeatureType
    {
        CountyPoint,
        PrefecturePoint,
        PrefecturePolygon,
        River
    }

    [Serializable]
    public class GeoFeature
    {
        public GeoFeatureType Type;
        public string NameChinese;
        public string NamePinyin;
        public string AdminType;
        public int BeginYear;
        public int EndYear;
        public string PresentLocation;
        public string BeginChangeType;
        public string EndChangeType;
        public int SysID;
        public string LevelRank;

        // 几何数据
        public List<Vector3> Points = new();                    // Point / LineString
        public List<List<Vector3>> Polygons = new();            // Polygon (外环+内环)
        public List<List<List<Vector3>>> MultiPolygons = new(); // MultiPolygon

        public bool IsPolygon => Polygons.Count > 0 || MultiPolygons.Count > 0;
        public bool IsPoint => Points.Count == 1 && !IsPolygon;
        public bool IsLine => Points.Count > 1 && !IsPolygon;
    }
}
