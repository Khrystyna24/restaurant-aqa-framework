using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Api.Seeding;

/// <summary>
/// Collects teardown actions for records created during a test and runs them in reverse (LIFO)
/// order on dispose. A failed cleanup is logged, never thrown: Locations, Dishes and Tables all have
/// real DELETE endpoints, but every one of them requires a bearer token (401 without one), so a
/// caller that forgets to forward it will still see a "failed" cleanup here rather than a crash —
/// one bad teardown never fails the rest of the scope.
/// </summary>
public sealed class CleanupScope(Action<string>? log = null) : IAsyncDisposable
{
    private readonly List<Func<Task>> _actions = [];
    private readonly Action<string> _log = log ?? (message => Console.Error.WriteLine(message));

    public void TrackDelete(string label, Func<Task<IAPIResponse>> delete) => _actions.Add(async () =>
    {
        try
        {
            var response = await delete();
            if (!response.Ok)
            {
                var error = await response.ReadErrorMessageAsync();
                _log($"[CleanupScope] Best-effort cleanup of '{label}' returned {response.Status}: {error}");
            }
        }
        catch (Exception ex)
        {
            _log($"[CleanupScope] Best-effort cleanup of '{label}' threw: {ex.Message}");
        }
    });

    public async ValueTask DisposeAsync()
    {
        for (var i = _actions.Count - 1; i >= 0; i--)
            await _actions[i]();

        _actions.Clear();
    }
}
