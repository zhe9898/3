using System;

namespace Zongzu.Contracts;

public sealed class ModuleStateEnvelope
{
    public string ModuleKey { get; set; } = string.Empty;

    public int ModuleSchemaVersion { get; set; }

    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
