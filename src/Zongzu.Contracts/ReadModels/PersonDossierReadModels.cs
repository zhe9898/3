using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// Runtime-only dossier assembled from distributed person facts.
/// This is a presentation read model, not authoritative state and not a save
/// namespace. Person identity still belongs to PersonRegistry; lineage,
/// temperament, and social-memory facts still belong to their owning modules.
/// </summary>
public sealed record PersonDossierSnapshot
{
    public PersonId PersonId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public LifeStage LifeStage { get; init; }

    public PersonGender Gender { get; init; } = PersonGender.Unspecified;

    public bool IsAlive { get; init; }

    public FidelityRing FidelityRing { get; init; } = FidelityRing.Local;

    public ClanId? ClanId { get; init; }

    public string ClanName { get; init; } = string.Empty;

    public string BranchPositionLabel { get; init; } = string.Empty;

    public string KinshipSummary { get; init; } = string.Empty;

    public string TemperamentSummary { get; init; } = string.Empty;

    public string MemoryPressureSummary { get; init; } = string.Empty;

    public string CurrentStatusSummary { get; init; } = string.Empty;

    public IReadOnlyList<string> SourceModuleKeys { get; init; } = [];
}
