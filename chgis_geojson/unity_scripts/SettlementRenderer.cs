using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CHGIS
{
    /// <summary>
    /// 治所点渲染器
    /// 将 GeoFeature 的点数据渲染为带标签的 Sprite/Mesh
    /// </summary>
    [RequireComponent(typeof(CHGISDataManager))]
    public class SettlementRenderer : MonoBehaviour
    {
        [Header("预制体")]
        public GameObject settlementPrefab; // 包含 SpriteRenderer + TextMeshPro
        public GameObject labelPrefab;      // 仅 TextMeshPro

        [Header("显示设置")]
        public float pointScale = 0.05f;
        public float labelOffsetY = 0.1f;
        public bool showLabels = true;
        public int labelMinZoom = 5; // 最小缩放级别才显示标签

        [Header("层级筛选")]
        public bool showProvincialCapital = true;   // 省会
        public bool showPrefecturalCapital = true;  // 府城
        public bool showCountyCapital = true;       // 县城
        public bool showTown = false;               // 乡镇

        [Header("颜色")]
        public Color provincialColor = Color.red;
        public Color prefecturalColor = new Color(1f, 0.6f, 0f);
        public Color countyColor = new Color(0.3f, 0.8f, 0.3f);
        public Color townColor = Color.gray;

        private Transform container;
        private Queue<GameObject> objectPool = new();
        private List<GameObject> activeObjects = new();
        private Camera mainCamera;

        void Awake()
        {
            container = new GameObject("Settlements").transform;
            container.SetParent(transform);
            mainCamera = Camera.main;
        }

        public void RenderYear(int year)
        {
            ClearAll();

            var features = CHGISDataManager.Instance.GetFeaturesForYear(year);
            foreach (var feat in features)
            {
                if (!feat.IsPoint) continue;
                if (!ShouldShow(feat)) continue;

                CreateSettlement(feat);
            }
        }

        bool ShouldShow(GeoFeature feat)
        {
            string type = feat.AdminType;
            return type switch
            {
                "省" => showProvincialCapital,
                "府" => showPrefecturalCapital,
                "州" => showPrefecturalCapital,
                "县" => showCountyCapital,
                "镇" or "乡" => showTown,
                _ => true
            };
        }

        void CreateSettlement(GeoFeature feat)
        {
            Vector3 pos = feat.Points[0];

            GameObject go = GetFromPool();
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * pointScale;
            go.name = $"Settlement_{feat.NameChinese}";

            // 设置颜色
            SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = GetColor(feat);

            // 设置标签
            TextMeshPro label = go.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.text = feat.NameChinese;
                label.gameObject.SetActive(showLabels);
                label.transform.localPosition = Vector3.up * labelOffsetY;
            }

            go.SetActive(true);
            activeObjects.Add(go);
        }

        Color GetColor(GeoFeature feat)
        {
            return feat.AdminType switch
            {
                "省" => provincialColor,
                "府" or "州" => prefecturalColor,
                "县" => countyColor,
                _ => townColor
            };
        }

        GameObject GetFromPool()
        {
            if (objectPool.Count > 0)
            {
                var go = objectPool.Dequeue();
                return go;
            }

            if (settlementPrefab != null)
            {
                return Instantiate(settlementPrefab, container);
            }

            // 默认创建
            GameObject go = new GameObject("Settlement");
            go.transform.SetParent(container);
            GameObject sprite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sprite.GetComponent<Collider>());
            sprite.transform.SetParent(go.transform);
            sprite.transform.localScale = Vector3.one * 0.5f;

            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.AddComponent<TextMeshPro>();

            return go;
        }

        void ClearAll()
        {
            foreach (var go in activeObjects)
            {
                go.SetActive(false);
                objectPool.Enqueue(go);
            }
            activeObjects.Clear();
        }

        void OnDestroy()
        {
            foreach (var go in activeObjects) if (go != null) Destroy(go);
            while (objectPool.Count > 0) { var go = objectPool.Dequeue(); if (go != null) Destroy(go); }
        }
    }
}
