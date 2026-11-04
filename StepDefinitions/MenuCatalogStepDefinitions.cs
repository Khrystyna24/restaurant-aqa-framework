using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>What guests see on the menu: browsing, sorting, searching and opening a dish. The dish "the kitchen
/// has added" is a Screenplay task and is remembered, so any step in any class can refer to "that dish".</summary>
[Binding]
public sealed class MenuCatalogStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Given("the restaurant has a location")]
    public Task GivenRestaurantHasALocation() => Actor.AttemptsToAsync(new ChooseAnExistingLocation());

    [Given("the kitchen has added a {dish_type} dish")]
    public Task GivenKitchenHasAddedADish(DishType type) => Actor.AttemptsToAsync(new AddDish(type));

    [When("a guest browses {dish_type} dishes")]
    public Task WhenGuestBrowsesDishes(DishType type) => Actor.AttemptsToAsync(new BrowseTheMenu(category: type));

    [When("a guest sorts the menu by {sort_order}")]
    public Task WhenGuestSortsTheMenu(DishSortOrder order) => Actor.AttemptsToAsync(new BrowseTheMenu(sortedBy: order));

    [When("a guest browses {dish_type} dishes sorted by {sort_order}")]
    public Task WhenGuestBrowsesDishesSorted(DishType type, DishSortOrder order) =>
        Actor.AttemptsToAsync(new BrowseTheMenu(category: type, sortedBy: order));

    [When("a guest searches the menu for that dish by name")]
    public Task WhenGuestSearchesForThatDish() =>
        Actor.AttemptsToAsync(new BrowseTheMenu(nameContains: Recall<DishResponse>().Name));

    [When("a guest opens that dish")]
    public Task WhenGuestOpensThatDish() => Actor.AttemptsToAsync(new OpenTheDish());

    [When("a guest opens a {item} that does not exist")]
    public Task WhenGuestOpensMissingItem(string item) => Actor.AttemptsToAsync(new OpenSomethingThatDoesNotExist(item));

    [Then("all the dishes shown are {dish_type} dishes")]
    public async Task ThenAllDishesAreOfType(DishType type)
    {
        var dishes = await Actor.AsksForAsync(new TheDishesShown());
        await Assert.That(dishes.Where(d => d.DishType != type.ToLabel()).Select(d => d.Name).ToList()).IsEmpty();
    }

    [Then("the dishes are listed {price_order}")]
    public async Task ThenDishesAreListedByPrice(SortDirection direction)
    {
        var prices = (await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Price).ToList();
        var expected = direction == SortDirection.Ascending ? prices.Order().ToList() : prices.OrderDescending().ToList();

        await Assert.That(prices.SequenceEqual(expected)).IsTrue().Because($"prices were [{string.Join(", ", prices)}]");
    }

    [Then("that dish appears in the results")]
    public async Task ThenThatDishAppearsInResults() =>
        await Assert.That((await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Id)).Contains(Recall<DishResponse>().Id);

    [Then("that dish does not appear in the results")]
    public async Task ThenThatDishDoesNotAppearInResults() =>
        await Assert.That((await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Id)).DoesNotContain(Recall<DishResponse>().Id);

    [Then("the dish is shown with:")]
    public async Task ThenDishIsShownWith(Fields expected)
    {
        foreach (var key in expected.Keys.ToList())
            if (expected[key] == "(sample dish name)")
                expected[key] = Recall<DishResponse>().Name;

        await Assert.That(expected.Mismatches(await Actor.AsksForAsync(new TheResponseJson()))).IsEmpty();
    }

    [When("a guest views the menu")]
    public Task WhenGuestViewsTheMenu() => Actor.AttemptsToAsync(new OpenTheMenuPage());

    [Then("that dish is visible on the menu")]
    public async Task ThenThatDishIsVisibleOnTheMenu() =>
        await Assert.That(await Actor.AsksForAsync(new TheDishOnTheMenu(Recall<DishResponse>().Name))).IsTrue();
}
