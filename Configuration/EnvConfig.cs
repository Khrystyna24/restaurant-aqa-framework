using dotenv.net;

namespace Restaurant_Aqa_Framework.Configuration;

/// <summary>
/// Loads configuration from a `.env` file once per process. Real environment
/// variables always win over `.env` values (overwriteExistingVars: false), so
/// CI can run with no `.env` file at all and still work.
/// </summary>
public static class EnvConfig
{
    private static readonly Lazy<bool> Loaded = new(() =>
    {
        DotEnv.Load(options: new DotEnvOptions(
            ignoreExceptions: true,
            probeForEnv: true,
            probeLevelsToSearch: 6,
            overwriteExistingVars: false,
            trimValues: true));
        return true;
    });

    public static void EnsureLoaded() => _ = Loaded.Value;
}
