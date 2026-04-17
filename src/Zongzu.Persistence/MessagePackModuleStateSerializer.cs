using System;
using MessagePack;
using MessagePack.Resolvers;

namespace Zongzu.Persistence;

public sealed class MessagePackModuleStateSerializer
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    public byte[] Serialize(Type stateType, object state)
    {
        ArgumentNullException.ThrowIfNull(stateType);
        ArgumentNullException.ThrowIfNull(state);
        return MessagePackSerializer.Serialize(stateType, state, Options);
    }

    public object Deserialize(Type stateType, ReadOnlyMemory<byte> payload)
    {
        ArgumentNullException.ThrowIfNull(stateType);
        object? state = MessagePackSerializer.Deserialize(stateType, payload.ToArray(), Options);
        return state ?? throw new InvalidOperationException($"Unable to deserialize module state {stateType.Name}.");
    }
}
