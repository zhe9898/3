using UnityEngine;

namespace Zongzu.UnityShell.Shell
{
    public sealed class ShellSurfaceRoot : MonoBehaviour
    {
        [SerializeField] private ShellSurfaceId surfaceId = ShellSurfaceId.GreatHall;
        [SerializeField] private string displayLabel = "大堂";
        [SerializeField] private Transform focalAnchor;
        [SerializeField] private Transform supportingLaneAnchor;
        [SerializeField] private Transform backgroundLaneAnchor;

        public ShellSurfaceId SurfaceId => surfaceId;

        public string DisplayLabel => displayLabel;

        public Transform FocalAnchor => focalAnchor;

        public Transform SupportingLaneAnchor => supportingLaneAnchor;

        public Transform BackgroundLaneAnchor => backgroundLaneAnchor;
    }
}
