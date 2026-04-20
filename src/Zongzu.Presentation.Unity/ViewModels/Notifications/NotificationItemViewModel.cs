namespace Zongzu.Presentation.Unity;

public sealed class NotificationItemViewModel
{
	public string Title { get; set; } = string.Empty;

	public string Summary { get; set; } = string.Empty;

	public string WhyItHappened { get; set; } = string.Empty;

	public string WhatNext { get; set; } = string.Empty;

	public string TierLabel { get; set; } = string.Empty;

	public string SurfaceLabel { get; set; } = string.Empty;

	public string SourceModuleKey { get; set; } = string.Empty;

	public int TraceCount { get; set; }
}
