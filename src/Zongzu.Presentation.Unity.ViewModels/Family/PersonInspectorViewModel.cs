using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class PersonInspectorViewModel
{
	public string ObjectAnchorLabel { get; set; } = string.Empty;

	public string TabletLabel { get; set; } = string.Empty;

	public string PortraitScrollLine { get; set; } = string.Empty;

	public string KinshipThreadLine { get; set; } = string.Empty;

	public string LivelihoodThreadLine { get; set; } = string.Empty;

	public string EducationThreadLine { get; set; } = string.Empty;

	public string OfficeThreadLine { get; set; } = string.Empty;

	public string MemoryThreadLine { get; set; } = string.Empty;

	public string StatusLedgerLine { get; set; } = string.Empty;

	public PersonDossierViewModel Dossier { get; set; } = new PersonDossierViewModel();

	public IReadOnlyList<string> SourceModuleKeys { get; set; } = Array.Empty<string>();
}
}
