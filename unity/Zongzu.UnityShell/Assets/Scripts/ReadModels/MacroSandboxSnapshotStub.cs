using System;
using System.Collections.Generic;

namespace Zongzu.UnityShell.ReadModels
{
    [Serializable]
    public sealed class MacroSandboxSnapshotStub
    {
        public string SnapshotId = "macro-bootstrap";
        public string TimeLabel = "皇祐三年 九月";
        public string RouteBandLabel = "两浙路总览";
        public string LeadPressure = "漕路微阻，州府递报转慢，县域冷热分化。";
        public List<MacroSandboxNodeStub> MajorNodes = new List<MacroSandboxNodeStub>();
        public List<MacroSandboxRouteStub> MajorRoutes = new List<MacroSandboxRouteStub>();
        public List<string> HotspotLabels = new List<string>();
    }

    [Serializable]
    public sealed class MacroSandboxNodeStub
    {
        public string Label = "";
        public string Kind = "";
        public string Heat = "";
        public string Note = "";
    }

    [Serializable]
    public sealed class MacroSandboxRouteStub
    {
        public string Label = "";
        public string Kind = "";
        public string Status = "";
        public string Pressure = "";
    }
}
