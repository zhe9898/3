using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Zongzu.Presentation.Unity;

namespace Zongzu.UnityShell.Shell
{
    /// <summary>
    /// Thin HTTP client that pulls the authoritative simulation's great-hall
    /// projection from the local Host process. Shell is a downstream
    /// projection only: this component never mutates authority. It only fetches
    /// shell-safe read models and logs them. Future work will feed the view
    /// model into the hall object anchors (notice tray, ancestral shrine,
    /// ledger, seals, almanac).
    /// </summary>
    public sealed class ZongzuClient : MonoBehaviour
    {
        [SerializeField] private string backendBaseUrl = "http://localhost:5173";
        [SerializeField] private bool fetchOnStart = true;

        private void Start()
        {
            if (fetchOnStart)
            {
                StartCoroutine(FetchHall());
            }
        }

        [ContextMenu("Fetch /hall")]
        public void FetchHallNow()
        {
            StartCoroutine(FetchHall());
        }

        [ContextMenu("Post /advance-month")]
        public void AdvanceOneMonth()
        {
            StartCoroutine(AdvanceMonth());
        }

        private System.Collections.IEnumerator FetchHall()
        {
            using UnityWebRequest request = UnityWebRequest.Get($"{backendBaseUrl}/hall");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Zongzu/HALL] fetch failed: {request.error}");
                yield break;
            }

            LogShell(request.downloadHandler.text, tag: "HALL");
        }

        private System.Collections.IEnumerator AdvanceMonth()
        {
            using UnityWebRequest request = UnityWebRequest.PostWwwForm(
                $"{backendBaseUrl}/advance-month", string.Empty);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Zongzu/ADV] post failed: {request.error}");
                yield break;
            }

            LogShell(request.downloadHandler.text, tag: "ADV");
        }

        private static void LogShell(string json, string tag)
        {
            try
            {
                PresentationShellViewModel shell =
                    JsonConvert.DeserializeObject<PresentationShellViewModel>(json);

                if (shell == null)
                {
                    Debug.LogWarning($"[Zongzu/{tag}] deserialized to null");
                    return;
                }

                GreatHallDashboardViewModel gh = shell.GreatHall;
                Debug.Log(
                    $"[Zongzu/{tag}] {gh.CurrentDateLabel} | " +
                    $"urgent={gh.UrgentCount} conseq={gh.ConsequentialCount} bg={gh.BackgroundCount} | " +
                    $"family: {gh.FamilySummary}");

                if (shell.NotificationCenter != null && shell.NotificationCenter.Items != null)
                {
                    Debug.Log(
                        $"[Zongzu/{tag}] notifications={shell.NotificationCenter.Items.Count} " +
                        $"| settlements={shell.DeskSandbox?.Settlements?.Count ?? 0}");
                }

                // 将数据锚到大堂物件上
                GreatHallPresenter presenter =
                    UnityEngine.Object.FindFirstObjectByType<GreatHallPresenter>();
                if (presenter != null)
                {
                    presenter.Present(shell);
                    Debug.Log($"[Zongzu/{tag}] GreatHallPresenter updated.");
                }
                else
                {
                    Debug.LogWarning(
                        $"[Zongzu/{tag}] GreatHallPresenter not found in scene. " +
                        "Create one and hang it under the hall root.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Zongzu/{tag}] deserialize threw: {ex.Message}");
            }
        }
    }
}
