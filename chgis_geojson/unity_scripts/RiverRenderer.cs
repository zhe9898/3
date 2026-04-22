using System.Collections.Generic;
using UnityEngine;

namespace CHGIS
{
    /// <summary>
    /// 河流渲染器
    /// 按 Strahler 河网分级控制线条粗细
    /// </summary>
    public class RiverRenderer : MonoBehaviour
    {
        [Header("渲染设置")]
        public Material lineMaterial;
        public float baseWidth = 0.02f;
        public float widthMultiplier = 0.01f;

        [Header("颜色")]
        public Color riverColor = new Color(0.2f, 0.4f, 0.8f, 0.7f);

        private Transform container;
        private Queue<LineRenderer> linePool = new();
        private List<LineRenderer> activeLines = new();

        void Awake()
        {
            container = new GameObject("Rivers").transform;
            container.SetParent(transform);
        }

        public void RenderYear(int year)
        {
            // 河流是静态的（1820年数据），不需要按年份过滤
            if (activeLines.Count > 0) return; // 已渲染过

            var rivers = CHGISDataManager.Instance.RiverFeatures;
            foreach (var river in rivers)
            {
                if (!river.IsLine) continue;
                DrawRiver(river);
            }

            Debug.Log($"[RiverRenderer] 渲染了 {rivers.Count} 条河流");
        }

        void DrawRiver(GeoFeature river)
        {
            // 获取 Strahler 分级（如果有）
            int strahler = 3; // 默认中等河流
            float width = baseWidth + strahler * widthMultiplier;

            LineRenderer lr = GetLineFromPool();
            lr.positionCount = river.Points.Count;
            lr.SetPositions(river.Points.ToArray());
            lr.startColor = riverColor;
            lr.endColor = riverColor;
            lr.startWidth = width;
            lr.endWidth = width * 0.7f; // 上游细下游粗
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

            GameObject go = new GameObject("RiverLine");
            go.transform.SetParent(container);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            activeLines.Add(line);
            return line;
        }

        Material CreateDefaultMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = riverColor;
            return mat;
        }

        void OnDestroy()
        {
            foreach (var line in activeLines) if (line != null) Destroy(line.gameObject);
            while (linePool.Count > 0) { var line = linePool.Dequeue(); if (line != null) Destroy(line.gameObject); }
        }
    }
}
