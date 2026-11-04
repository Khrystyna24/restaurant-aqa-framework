using dotenv.net.Utilities;
using TUnit.Core;
using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<Restaurant_Aqa_Framework.Configuration.ConfiguredParallelLimit>]

namespace Restaurant_Aqa_Framework.Configuration;

/// <summary>
/// How many tests TUnit may run at once: <c>MAX_PARALLEL_TESTS</c> if set, otherwise the machine's
/// logical processor count capped at <see cref="DefaultCap"/>. The cap exists because every run
/// shares one small "develop" backend, which starts answering 500s when a many-core machine fires a
/// burst of concurrent writes at it. Resolved lazily, once.
/// </summary>
public static class ParallelismConfig
{
    private const int DefaultCap = 4;

    private static readonly Lazy<int> Value = new(Resolve);

    public static int MaxParallelism => Value.Value;

    private static int Resolve()
    {
        EnvConfig.EnsureLoaded();

        return EnvReader.TryGetIntValue("MAX_PARALLEL_TESTS", out var configured) && configured > 0
            ? configured
            : Math.Min(Environment.ProcessorCount, DefaultCap);
    }
}

/// <summary>
/// Feeds <see cref="ParallelismConfig.MaxParallelism"/> into TUnit's own concurrency
/// control via the assembly-level <see cref="ParallelLimiterAttribute{T}"/> above —
/// this is the real knob TUnit's scheduler reads, not just an inert config value.
/// </summary>
public sealed class ConfiguredParallelLimit : IParallelLimit
{
    public int Limit => ParallelismConfig.MaxParallelism;
}
