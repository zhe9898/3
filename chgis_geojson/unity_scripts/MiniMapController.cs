using UnityEngine;
using UnityEngine.UI;

namespace CHGIS
{
    /// <summary>
    /// 小地图控制器
    /// 正交相机俯瞰大地图，输出到 RenderTexture
    /// </summary>
    public class MiniMapController : MonoBehaviour
    {
        [Header("相机设置")]
        public Camera miniMapCamera;
        public RenderTexture renderTexture;

        [Header("UI")]
        public RawImage miniMapImage;
        public RectTransform playerMarker;

        [Header("同步设置")]
        public Camera mainCamera;
        public float cameraHeight = 200f;
        public float followSmoothness = 5f;

        [Header("边界框")]
        public RectTransform viewPortRect; // 显示大地图视口的矩形

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            SetupRenderTexture();
        }

        void SetupRenderTexture()
        {
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(512, 512, 16);
                renderTexture.Create();
            }

            if (miniMapCamera != null)
            {
                miniMapCamera.targetTexture = renderTexture;
                miniMapCamera.orthographic = true;
            }

            if (miniMapImage != null)
            {
                miniMapImage.texture = renderTexture;
            }
        }

        void LateUpdate()
        {
            if (mainCamera == null || miniMapCamera == null) return;

            // 小地图相机跟随大地图相机位置
            Vector3 targetPos = mainCamera.transform.position;
            targetPos.y = cameraHeight;

            miniMapCamera.transform.position = Vector3.Lerp(
                miniMapCamera.transform.position,
                targetPos,
                Time.deltaTime * followSmoothness
            );

            // 同步旋转（可选）
            // miniMapCamera.transform.rotation = Quaternion.Euler(90f, mainCamera.transform.eulerAngles.y, 0f);

            UpdateViewPortRect();
        }

        /// <summary>
        /// 更新视口矩形，显示大地图当前可见范围
        /// </summary>
        void UpdateViewPortRect()
        {
            if (viewPortRect == null) return;

            float orthoSize = mainCamera.orthographicSize;
            float aspect = mainCamera.aspect;

            // 计算视口在世界坐标中的大小
            float viewHeight = orthoSize * 2f;
            float viewWidth = viewHeight * aspect;

            // 转换为小地图 UI 坐标
            Vector2 miniMapSize = ((RectTransform)miniMapImage.transform).sizeDelta;
            float scaleX = miniMapSize.x / (miniMapCamera.orthographicSize * 2f * miniMapCamera.aspect);
            float scaleY = miniMapSize.y / (miniMapCamera.orthographicSize * 2f);

            viewPortRect.sizeDelta = new Vector2(viewWidth * scaleX, viewHeight * scaleY);
        }

        /// <summary>
        /// 点击小地图跳转
        /// </summary>
        public void OnMiniMapClick(Vector2 localPoint)
        {
            RectTransform rt = (RectTransform)miniMapImage.transform;
            Vector2 normalized = new Vector2(
                (localPoint.x / rt.sizeDelta.x) - 0.5f,
                (localPoint.y / rt.sizeDelta.y) - 0.5f
            );

            Vector3 worldPos = miniMapCamera.transform.position;
            worldPos.x += normalized.x * miniMapCamera.orthographicSize * 2f * miniMapCamera.aspect;
            worldPos.z += normalized.y * miniMapCamera.orthographicSize * 2f;
            worldPos.y = mainCamera.transform.position.y;

            mainCamera.transform.position = worldPos;
        }

        void OnDestroy()
        {
            if (renderTexture != null) renderTexture.Release();
        }
    }
}
