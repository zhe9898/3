namespace Zongzu.Presentation.Unity
{
public sealed class DebugModuleInspectorViewModel
{
	public string ModuleKey { get; set; } = string.Empty;

	public int SchemaVersion { get; set; }

	public int PayloadBytes { get; set; }
}
}
