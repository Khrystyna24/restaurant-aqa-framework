using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Registers a visitor with the registration service directly (no browser) and remembers the details, so a
/// later step can sign in with them. The outcome is the scenario's last response.</summary>
public sealed class RegisterThroughTheService(RegisterRequest visitor) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        actor.Remember(visitor);
        await actor.TheLastResponse().CaptureAsync(await new AuthApi(actor.UsingTheApi()).RegisterAsync(visitor));
    }
}
