using System;

namespace Zongzu.Persistence;

public sealed class SaveMigrationException : InvalidOperationException
{
    public SaveMigrationException(string message)
        : base(message)
    {
    }
}
