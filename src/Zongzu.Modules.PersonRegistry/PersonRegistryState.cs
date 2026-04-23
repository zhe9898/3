using System.Collections.Generic;
using Zongzu.Contracts;

namespace Zongzu.Modules.PersonRegistry;

/// <summary>
/// Kernel-layer person identity anchor state.
/// Identity-only by design — see <c>PERSON_OWNERSHIP_RULES.md</c>.
/// Do not add domain fields (health, skills, kinship, activity) here.
/// </summary>
public sealed class PersonRegistryState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.PersonRegistry;

    public List<PersonRecord> Persons { get; set; } = new();
}
