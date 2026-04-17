using System;
using System.Security.Cryptography;

namespace Zongzu.Kernel;

public static class ReplayHashing
{
    public static string ComputeHex(ReadOnlySpan<byte> payload)
    {
        byte[] hash = SHA256.HashData(payload);
        return Convert.ToHexString(hash);
    }
}
