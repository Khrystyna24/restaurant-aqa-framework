using System.Globalization;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>A restaurant location as the manager maintains it and a guest sees it. The location is a Screenplay task's
/// work and lives in the actor's memory, so steps in other classes (adding a dish, looking at the menu) can use the
/// very same one. The backend stores average occupancy as a percentage string, so 70 reads back as "70%".</summary>
[Binding]
public sealed class LocationStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Given("a restaurant location with:")]
    public Task GivenRestaurantLocationWith(Fields details)
    {
        var seeded = SampleDataFactory.CreateLocationRequest();
        return Actor.AttemptsToAsync(new SetUpLocation(seeded with
        {
            Description = details.GetValueOrDefault("description", seeded.Description),
            TotalCapacity = details.TryGetValue("capacity", out var capacity) ? int.Parse(capacity, CultureInfo.InvariantCulture) : seeded.TotalCapacity,
            AverageOccupancy = details.GetValueOrDefault("average occupancy", seeded.AverageOccupancy),
        }));
    }

    [When("a guest looks up the location")]
    public Task WhenGuestLooksUpTheLocation() => Actor.AttemptsToAsync(new LookUpTheLocation());

    [When("a guest opens the location's menu")]
    public Task WhenGuestOpensTheLocationsMenu() => Actor.AttemptsToAsync(new OpenTheLocationsMenu());

    [When("a guest opens the menu of the {string} location")]
    public Task WhenGuestOpensTheMenuOfNamedLocation(string location) => Actor.AttemptsToAsync(new OpenTheLocationsMenu(location));

    [When("the manager updates the location's average occupancy to {int}%")]
    public Task WhenManagerUpdatesOccupancy(int occupancy) =>
        Actor.AttemptsToAsync(new ChangeLocation(l => l with { AverageOccupancy = occupancy.ToString(CultureInfo.InvariantCulture) }));

    [When("the manager describes the location as:")]
    public Task WhenManagerDescribesTheLocation(string description) =>
        Actor.AttemptsToAsync(new ChangeLocation(l => l with { Description = description }));

    [Then("the location shows:")]
    public async Task ThenLocationShows(Fields expected) =>
        await Assert.That(expected.Mismatches(await Actor.AsksForAsync(new TheResponseJson()))).IsEmpty();

    [Then("the location shows an average occupancy of {int}%")]
    public async Task ThenLocationShowsOccupancy(int occupancy) =>
        await Assert.That((await Actor.AsksForAsync(new TheResponseAs<LocationResponse>())).AverageOccupancy).IsEqualTo($"{occupancy}%");

    [Then("the location is described as:")]
    public async Task ThenLocationIsDescribedAs(string expected) =>
        await Assert.That(Normalize((await Actor.AsksForAsync(new TheResponseAs<LocationResponse>())).Description)).IsEqualTo(Normalize(expected));

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
}
