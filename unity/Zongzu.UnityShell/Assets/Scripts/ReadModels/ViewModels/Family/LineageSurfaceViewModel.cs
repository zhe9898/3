using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class LineageSurfaceViewModel
{
	public IReadOnlyList<ClanTileViewModel> Clans { get; set; } = Array.Empty<ClanTileViewModel>();

	public IReadOnlyList<PersonDossierViewModel> PersonDossiers { get; set; } = Array.Empty<PersonDossierViewModel>();

	public PersonInspectorViewModel? FocusedPerson { get; set; }
}
}
