using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class NotificationCenterViewModel
{
	public IReadOnlyList<NotificationItemViewModel> Items { get; set; } = Array.Empty<NotificationItemViewModel>();
}
