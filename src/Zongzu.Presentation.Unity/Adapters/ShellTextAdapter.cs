using System;
using System.Linq;

namespace Zongzu.Presentation.Unity;

internal static class ShellTextAdapter
{
	internal static string CombineDistinct(params string[] values)
	{
		return string.Join(" ", values
			.Where(value => !string.IsNullOrWhiteSpace(value))
			.Distinct(StringComparer.Ordinal));
	}

	internal static string AppendDistinct(string primary, string suffix)
	{
		if (string.IsNullOrWhiteSpace(primary))
		{
			return suffix ?? string.Empty;
		}

		if (string.IsNullOrWhiteSpace(suffix) || primary.Contains(suffix, StringComparison.Ordinal))
		{
			return primary;
		}

		return primary + " " + suffix;
	}
}
