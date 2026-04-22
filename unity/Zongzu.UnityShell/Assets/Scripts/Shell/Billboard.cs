using UnityEngine;

namespace Zongzu.UnityShell.Shell
{
    /// <summary>
    /// Keeps the GameObject facing the main camera so world-space text
    /// is always readable regardless of camera angle.
    /// </summary>
    public sealed class Billboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main == null) return;
            transform.forward = Camera.main.transform.forward;
        }
    }
}
