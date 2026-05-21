using System;

internal sealed class TestRunnerSettings
{
    private TestRunnerSettings(string? filter, int shardIndex, int shardCount)
    {
        Filter = filter;
        ShardIndex = shardIndex;
        ShardCount = shardCount;
    }

    public string? Filter { get; }

    public int ShardIndex { get; }

    public int ShardCount { get; }

    public static TestRunnerSettings Create(string[] args)
    {
        string? filter = null;
        var shardIndex = TestShardDefaults.ShardIndex;
        var shardCount = TestShardDefaults.ShardCount;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--filter", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= args.Length)
                {
                    throw new InvalidOperationException("Missing value after --filter.");
                }

                filter ??= args[index];
                continue;
            }

            if (arg.StartsWith("--filter=", StringComparison.OrdinalIgnoreCase))
            {
                filter ??= arg.Substring("--filter=".Length);
                continue;
            }

            if (string.Equals(arg, "--shard", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= args.Length)
                {
                    throw new InvalidOperationException("Missing value after --shard.");
                }

                (shardIndex, shardCount) = ParseShard(args[index]);
                continue;
            }

            if (arg.StartsWith("--shard=", StringComparison.OrdinalIgnoreCase))
            {
                (shardIndex, shardCount) = ParseShard(arg.Substring("--shard=".Length));
                continue;
            }

            filter ??= arg;
        }

        if (shardCount <= 0)
        {
            throw new InvalidOperationException("Shard count must be greater than zero.");
        }

        if (shardIndex < 0 || shardIndex >= shardCount)
        {
            throw new InvalidOperationException("Shard index must be zero-based and less than shard count.");
        }

        return new TestRunnerSettings(filter, shardIndex, shardCount);
    }

    public bool Includes(string name, int ordinal)
    {
        if (!string.IsNullOrWhiteSpace(Filter) &&
            !name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ordinal % ShardCount == ShardIndex;
    }

    public string FormatNoTestsMessage()
    {
        var filterText = string.IsNullOrWhiteSpace(Filter) ? "<none>" : Filter;
        return $"No tests matched filter '{filterText}' in shard {ShardIndex}/{ShardCount}.";
    }

    private static (int ShardIndex, int ShardCount) ParseShard(string value)
    {
        var parts = value.Split('/');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out var shardIndex) ||
            !int.TryParse(parts[1], out var shardCount))
        {
            throw new InvalidOperationException("Shard value must use '<zero-based-index>/<count>' format.");
        }

        return (shardIndex, shardCount);
    }
}
