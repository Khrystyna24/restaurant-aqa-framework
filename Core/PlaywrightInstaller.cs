using System.Runtime.InteropServices;
using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Core
{
    /// <summary>
    /// Ensures Playwright browsers are installed. Thread-safe, process-safe.
    /// Always calls 'playwright install' - it's idempotent and fast if already installed.
    /// </summary>
    public static class PlaywrightInstaller
    {
        private static readonly Lock Lock = new();
        private static Task? _installTask;
        private static bool _completed;
        private static Exception? _exception;

        private const string MutexName = "Global\\playwright-dotnet-install";
        private static readonly HashSet<string> ValidBrowsers = ["chromium", "firefox", "webkit"];

        public static Task EnsureInstalledAsync(
            IEnumerable<string>? browserTypes = null,
            bool withDeps = false)
        {
            if (_completed)
            {
                return Task.CompletedTask;
            }

            if (_exception is not null)
            {
                return Task.FromException(_exception);
            }

            var existing = Volatile.Read(ref _installTask);
            if (existing is not null)
            {
                return existing;
            }

            lock (Lock)
            {
                if (_completed) return Task.CompletedTask;
                if (_exception is not null) return Task.FromException(_exception);
                if (_installTask is not null) return _installTask;

                var types = Normalize(browserTypes);

                _installTask = Task.Run(() =>
                {
                    try
                    {
                        InstallWithMutex(types, withDeps);
                        _completed = true;
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                        throw;
                    }
                });

                return _installTask;
            }
        }

        public static Task EnsureInstalledAsync(string browserType, bool withDeps = false)
            => EnsureInstalledAsync([browserType], withDeps);

        private static string[] Normalize(IEnumerable<string>? types)
        {
            var result = (types ?? ["chromium"])
                .Select(t => t?.Trim().ToLowerInvariant() ?? "")
                .Where(ValidBrowsers.Contains)
                .Distinct()
                .ToArray();

            return result.Length > 0
                ? result
                : throw new ArgumentException($"Invalid browser types. Valid: {string.Join(", ", ValidBrowsers)}");
        }

        private static void InstallWithMutex(string[] types, bool withDeps)
        {
            using var mutex = new Mutex(false, MutexName);
            var acquired = false;

            try
            {
                try
                {
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(30));
                }
                catch (AbandonedMutexException)
                {
                    acquired = true;
                }

                if (!acquired)
                    throw new TimeoutException("Timeout waiting for browser installation mutex.");

                // Remove old browsers first (safe: only old versions, not the ones we're installing)
                CleanupOldBrowsers(types);

                // Install required browsers if missing
                var args = BuildArgs(types, withDeps);
                var exitCode = Program.Main(args);

                if (exitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Playwright install failed for [{string.Join(", ", types)}]. Exit code: {exitCode}");
                }
            }
            finally
            {
                if (acquired)
                {
                    try { mutex.ReleaseMutex(); }
                    catch { /* ignore */ }
                }
            }
        }

        /// <summary>
        /// Deletes old browser directories for the current types list.
        /// </summary>
        private static void CleanupOldBrowsers(string[] currentTypes)
        {
            var basePath = GetBrowsersPath();
            if (!Directory.Exists(basePath)) return;

            var required = currentTypes
                .Select(t => t.ToLowerInvariant())
                .ToHashSet();

            foreach (var browser in required)
            {
                var versions = Directory.GetDirectories(basePath, $"{browser}-*")
                    .Select(path =>
                    {
                        var name = Path.GetFileName(path);
                        var suffix = name[(browser.Length + 1)..];

                        return int.TryParse(suffix, out var v)
                            ? (Path: path, Version: v)
                            : (Path: null, Version: -1);
                    })
                    .Where(x => x.Path != null)
                    .OrderByDescending(x => x.Version)
                    .ToList();

                // Keep newest, delete the rest
                foreach (var old in versions.Skip(1))
                {
                    TryDelete(old.Path!);
                }
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
                // ignore deletion errors
            }
        }

        private static string[] BuildArgs(string[] types, bool withDeps)
        {
            var args = new List<string> { "install" };

            if (withDeps && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                args.Add("--with-deps");

            args.AddRange(types);
            return [.. args];
        }

        /// <summary>
        /// Reset for testing.
        /// </summary>
        public static void Reset()
        {
            lock (Lock)
            {
                _installTask = null;
                _completed = false;
                _exception = null;
            }
        }

        /// <summary>
        /// Get browsers base path (for diagnostics).
        /// </summary>
        public static string GetBrowsersPath()
        {
            return Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ms-playwright");
        }
    }
}
