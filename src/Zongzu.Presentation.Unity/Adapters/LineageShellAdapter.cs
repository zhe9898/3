using System;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class LineageShellAdapter
{
	internal static LineageSurfaceViewModel BuildLineage(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		return new LineageSurfaceViewModel
		{
			Clans = bundle.Clans
				.OrderBy(clan => clan.ClanName, StringComparer.Ordinal)
				.Select(clan => new ClanTileViewModel
				{
					ClanName = clan.ClanName,
					Prestige = clan.Prestige,
					SupportReserve = clan.SupportReserve,
					StatusText = clan.HeirPersonId.HasValue ? "承祧之人已入谱。" : "宗房暂未举出承祧人。"
				})
				.ToArray()
		};
	}
}
