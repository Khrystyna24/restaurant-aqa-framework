using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Configuration;

namespace Restaurant_Aqa_Framework.Api;

public sealed class AuthApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> RegisterAsync(RegisterRequest request) =>
        Api.PostAsync(Url("/register"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(),
        });

    public Task<IAPIResponse> SignInAsync(SignInRequest request) =>
        Api.PostAsync(Url("/signin"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(),
        });

    /// <summary>Signs in as the configured standing Waiter and returns its id token, for seeding/cleanup
    /// calls that need a bearer token. Throws if the account isn't configured or sign-in fails.</summary>
    public async Task<string> SignInAsWaiterAsync()
    {
        var (username, password) = Settings.Current.RequireWaiter();
        var response = await SignInAsync(new SignInRequest(username, password));
        if (!response.Ok)
            throw new InvalidOperationException(
                $"Waiter sign-in failed with {response.Status}: {await response.ReadErrorMessageAsync()}");

        return (await response.ReadAsAsync<SignInResponse>()).IdToken;
    }
}
