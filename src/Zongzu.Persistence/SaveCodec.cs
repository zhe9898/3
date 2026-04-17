using System;
using MessagePack;
using MessagePack.Resolvers;
using Zongzu.Contracts;

namespace Zongzu.Persistence;

public sealed class SaveCodec
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    public byte[] Encode(SaveRoot saveRoot)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);
        return MessagePackSerializer.Serialize(saveRoot, Options);
    }

    public SaveRoot Decode(ReadOnlyMemory<byte> payload)
    {
        SaveRoot? saveRoot = MessagePackSerializer.Deserialize<SaveRoot>(payload.ToArray(), Options);
        return saveRoot ?? throw new InvalidOperationException("Unable to decode save root.");
    }
}
