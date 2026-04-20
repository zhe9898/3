using System;
using System.Collections.Generic;

namespace Zongzu.UnityShell.ReadModels
{
    [Serializable]
    public sealed class ReadModelSnapshotStub
    {
        public string SnapshotId = "bootstrap";
        public string DateLabel = "1200-03";
        public string LeadNotice = "请用导出的堂上首条告示替换这里。";
        public List<string> UrgentItems = new();
        public List<string> ConsequentialItems = new();
        public List<string> BackgroundItems = new();
    }
}
