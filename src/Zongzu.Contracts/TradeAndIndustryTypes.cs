using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// Phase 5 商贸骨骼 — <c>LIVING_WORLD_DESIGN §2.5</c>。七类物品的粗粒分类。
/// </summary>
public enum GoodsCategory
{
    Unknown = 0,
    Grain = 1,
    Salt = 2,
    Cloth = 3,
    Iron = 4,
    Tea = 5,
    Timber = 6,
    Luxury = 7,
}

/// <summary>
/// 某镇场上某类物品的供需与价位只读投影。
/// Phase 5 只承载 <see cref="GoodsCategory.Grain"/> 的真实链路；其他类别先立住容器。
/// </summary>
public sealed record MarketGoodsSnapshot
{
    public SettlementId SettlementId { get; init; }

    public GoodsCategory Goods { get; init; } = GoodsCategory.Unknown;

    public int Supply { get; init; }

    public int Demand { get; init; }

    public int BasePrice { get; init; }

    public int CurrentPrice { get; init; }
}
