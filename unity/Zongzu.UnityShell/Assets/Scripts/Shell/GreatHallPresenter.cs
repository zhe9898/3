using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zongzu.Presentation.Unity;

namespace Zongzu.UnityShell.Shell
{
    /// <summary>
    /// Sits in the hall and anchors simulation data onto physical objects.
    /// Uses TextMeshPro for ASCII/numbers and Unity UI.Text (system font)
    /// for Chinese text so no manual font-creation is required.
    /// </summary>
    public sealed class GreatHallPresenter : MonoBehaviour
    {
        [Header("案头历书 — 日期 (TextMeshPro)")]
        [SerializeField] private TextMeshPro almanacDateLabel;

        [Header("宗谱面 — 家族摘要 (UI.Text)")]
        [SerializeField] private Text ancestralTableSummaryLabel;

        [Header("告示托盘 — 通知状态")]
        [SerializeField] private SpriteRenderer noticeTrayRenderer;
        [SerializeField] private TextMeshPro noticeTrayCountLabel;
        [SerializeField] private Color noticeTrayEmpty = new Color(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color noticeTrayNormal = new Color(0.85f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color noticeTrayUrgent = new Color(0.75f, 0.15f, 0.15f, 1f);

        [Header("案头沙盘 — 聚落数 (TextMeshPro)")]
        [SerializeField] private TextMeshPro sandboxSettlementCountLabel;

        [Header("冲突剪影 — 军务概要 (UI.Text)")]
        [SerializeField] private Text conflictSummaryLabel;

        private void Start()
        {
            void SetupTMP(TextMeshPro tmp, float size, float boxW, float boxH)
            {
                if (tmp == null) return;
                tmp.fontSize = size;
                tmp.enableAutoSizing = false;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;

                RectTransform rt = tmp.rectTransform;
                if (rt != null) rt.sizeDelta = new Vector2(boxW, boxH);

                if (tmp.GetComponent<Billboard>() == null)
                    tmp.gameObject.AddComponent<Billboard>();
            }

            SetupTMP(almanacDateLabel, 3f, 4f, 2f);
            SetupTMP(sandboxSettlementCountLabel, 4f, 2f, 2f);
            SetupTMP(noticeTrayCountLabel, 2f, 2f, 2f);

            // 中文文本用系统字体 + UI.Text，自动创建 Canvas 子对象
            EnsureChineseText(ref ancestralTableSummaryLabel, "宗谱面文字", transform.Find("宗谱面"), 0.7f);
            EnsureChineseText(ref conflictSummaryLabel, "冲突剪影文字", transform.Find("冲突剪影"), 0.7f);
        }

        private void EnsureChineseText(ref Text textField, string objName, Transform parent, float fontScale)
        {
            if (textField != null) return;
            if (parent == null)
            {
                Debug.LogWarning($"[GreatHallPresenter] Parent not found for {objName}");
                return;
            }

            // 查找是否已有 UI.Text 子对象
            foreach (Transform child in parent)
            {
                var existing = child.GetComponent<Text>();
                if (existing != null)
                {
                    textField = existing;
                    return;
                }
            }

            // 创建 World-Space Canvas + UI.Text
            GameObject go = new GameObject(objName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, 0.5f, 0);
            go.transform.localScale = Vector3.one * fontScale;

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 10;
            canvas.referencePixelsPerUnit = 10f; // 越小文字越锐利

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 400);

            // 使用系统字体（Windows: 微软雅黑）
            Font font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 48);
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("SimSun", 48);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Text txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 48;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;

            textField = txt;

            if (go.GetComponent<Billboard>() == null)
                go.AddComponent<Billboard>();
        }

        public void Present(PresentationShellViewModel shell)
        {
            if (shell == null) return;

            var gh = shell.GreatHall;
            var ds = shell.DeskSandbox;
            var wf = shell.Warfare;

            if (almanacDateLabel != null)
                almanacDateLabel.text = gh.CurrentDateLabel;

            if (ancestralTableSummaryLabel != null)
            {
                // English placeholder; full Chinese text is still logged to Console by ZongzuClient.
                ancestralTableSummaryLabel.text =
                    $"Clan Zhang | Prestige 52 | Heir UNSETTLED\n" +
                    $"Support: 60 | Urgent: {gh.UrgentCount} | Conseq: {gh.ConsequentialCount}";
            }

            int totalNotices = gh.UrgentCount + gh.ConsequentialCount + gh.BackgroundCount;
            if (noticeTrayRenderer != null)
            {
                if (gh.UrgentCount > 0)
                    noticeTrayRenderer.color = noticeTrayUrgent;
                else if (totalNotices > 0)
                    noticeTrayRenderer.color = noticeTrayNormal;
                else
                    noticeTrayRenderer.color = noticeTrayEmpty;
            }
            if (noticeTrayCountLabel != null)
                noticeTrayCountLabel.text = totalNotices > 0 ? totalNotices.ToString() : "";

            int settlementCount = ds?.Settlements?.Count ?? 0;
            if (sandboxSettlementCountLabel != null)
                sandboxSettlementCountLabel.text = settlementCount.ToString();

            if (conflictSummaryLabel != null)
            {
                bool hasCampaign = !string.IsNullOrEmpty(wf?.Summary) && !wf.Summary.Contains("暂无");
                conflictSummaryLabel.text = hasCampaign ? "CAMPAIGN ACTIVE" : "No active campaigns.";
            }
        }
    }
}
