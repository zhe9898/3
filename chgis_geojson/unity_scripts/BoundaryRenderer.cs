using System.Collections.Generic;
using UnityEngine;

namespace CHGIS
{
    /// <summary>
    /// 政区边界渲染器
    /// 负责将 GeoFeature 的 Polygon/MultiPolygon 渲染为 LineRenderer
    /// </summary>
    [RequireComponent(typeof(CHGISDataManager))]
    public class BoundaryRenderer : MonoBehaviour
    {
        [Header("渲染设置")]
        public Material lineMaterial;
        public float lineWidth = 0.05f;
        public int lineVertexCount = 64;

        [Header("颜色配置")]
        public Color provinceColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        public Color prefectureColor = new Color(0.3f, 0.5f, 0.9f, 0.8f);
        public Color countyColor = new Color(0.4f, 0.7f, 0.4f, 0.6f);

        [Header("层级配置")]
        public bool showProvince = true;
        public bool showPrefecture = true;
        public bool showCounty = false;

        // 对象池
        private Queue<LineRenderer> linePool = new();
        private List<LineRenderer> activeLines = new();
        private Transform lineContainer;

        void Awake()
        {
            lineContainer = new GameObject("BoundaryLines").transform;
            lineContainer.SetParent(transform);
        }

        /// <summary>
        /// 渲染指定年份的边界
        /// </summary>
        public void RenderYear(int year)
        {
            ClearAllLines();

            var features = CHGISDataManager.Instance.GetFeaturesForYear(year);
            foreach (var feat in features)
            {
                if (!feat.IsPolygon) continue;
                if (!ShouldRender(feat)) continue;

                RenderFeature(feat);
            }
        }

        bool ShouldRender(GeoFeature feat)
        {
            return feat.Type switch
            {
                GeoFeatureType.PrefecturePolygon => showPrefecture,
                _ => false
            };
        }

        void RenderFeature(GeoFeature feat)
        {
            Color color = GetColorForFeature(feat);

            // 处理 Polygon
            foreach (var ring in feat.Polygons)
            {
                DrawLine(ring, color);
            }

            // 处理 MultiPolygon
            foreach (var poly in feat.MultiPolygons)
            {
                foreach (var ring in poly)
                {
                    DrawLine(ring, color);
                }
            }
        }

        Color GetColorForFeature(GeoFeature feat)
        {
            string level = feat.LevelRank;
            return level switch
            {
                "2" => provinceColor,   // 省
                "4" => prefectureColor, // 府
                "6" => countyColor,     // 县
                _ => prefectureColor
            };
        }

        void DrawLine(List<Vector3> points, Color color)
        {
            if (points.Count < 2) return;

            LineRenderer lr = GetLineFromPool();
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = lineMaterial != null ? lineMaterial : CreateDefaultMaterial();
            lr.gameObject.SetActive(true);
        }

        LineRenderer GetLineFromPool()
        {
            if (linePool.Count > 0)
            {
                var lr = linePool.Dequeue();
                activeLines.Add(lr);
                return lr;
            }

            GameObject go = new GameObject("BoundaryLine");
            go.transform.SetParent(lineContainer);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            activeLines.Add(line);
            return line;
        }

        void ClearAllLines()
        {
            foreach (var line in activeLines)
            {
                line.gameObject.SetActive(false);
                linePool.Enqueue(line);
            }
            activeLines.Clear();
        }

        Material CreateDefaultMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            return mat;
        }

        void OnDestroy()
        {
            foreach (var line in activeLines) if (line != null) Destroy(line.gameObject);
            while (linePool.Count > 0) { var line = linePool.Dequeue(); if (line != null) Destroy(line.gameObject); }
        }
    }
}
