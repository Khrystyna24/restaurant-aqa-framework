using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Dtos.Tables;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>The manager sets up a location, its dishes and its tables. The work is done by Screenplay tasks, which
/// remember what they created (and remove it afterwards), so steps in other classes can carry on with the same
/// location, dish or table — or with a named one when a scenario uses several.</summary>
[Binding]
public sealed class SeedingStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Given("the manager has set up a new location")]
    [When("the manager sets up a new location")]
    public Task WhenManagerSetsUpALocation() => Actor.AttemptsToAsync(new SetUpLocation());

    [Given("a restaurant location called {string}")]
    public Task GivenRestaurantLocationCalled(string name) => Actor.AttemptsToAsync(new SetUpLocation(name: name));

    /// <summary>Everything else about the location is valid, so the named problem is the only possible cause.</summary>
    [When("the manager tries to set up a location with {location_problem}")]
    public Task WhenManagerTriesToSetUpAnInvalidLocation(LocationProblem problem)
    {
        var valid = SampleDataFactory.CreateLocationRequest();
        var request = problem switch
        {
            LocationProblem.EmptyAddress => valid with { Address = "" },
            LocationProblem.NegativeCapacity => valid with { TotalCapacity = -5 },
            _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, null),
        };

        return Actor.AttemptsToAsync(new SetUpLocation(request, mustSucceed: false));
    }

    [Then("the location is created")]
    [Then("the dish is created")]
    [Then("the table is created")]
    public async Task ThenItIsCreated()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(201).Because($"response body was: {outcome.Body}");
    }

    [When("the manager adds a dish to the location")]
    public Task WhenManagerAddsADish() => Actor.AttemptsToAsync(new AddDish());

    [When("the manager adds a {dish_type} dish to the location")]
    public Task WhenManagerAddsADishOfType(DishType type) => Actor.AttemptsToAsync(new AddDish(type));

    [When("the manager adds a {dish_type} dish to the {string} location")]
    public Task WhenManagerAddsADishToNamedLocation(DishType type, string location) =>
        Actor.AttemptsToAsync(new AddDish(type, location));

    [Then("the dish is listed in the {dish_type} category")]
    public async Task ThenDishIsListedInCategory(DishType type) =>
        await Assert.That((await Actor.AsksForAsync(new TheStoredDish())).DishType).IsEqualTo(type.ToLabel());

    [When("the manager adds a table to the location")]
    public Task WhenManagerAddsATable() => Actor.AttemptsToAsync(new AddTable());

    [When("the manager adds a table for {int} guests to the location")]
    public Task WhenManagerAddsATableFor(int guests) => Actor.AttemptsToAsync(new AddTable(guests));

    [Then("the table seats {int} guests")]
    public async Task ThenTableSeats(int guests) =>
        await Assert.That(Recall<TableResponse>().Capacity).IsEqualTo(guests);
}
