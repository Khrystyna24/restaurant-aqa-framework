using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;

namespace Restaurant_Aqa_Framework.StepDefinitions;

[Binding]
public sealed class SmokeStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [When("a guest asks for the {information}")]
    public Task WhenGuestAsksFor(string information) => Actor.AttemptsToAsync(new AskForInformation(information));

    [Then("every dish has a name, a price and a type")]
    public async Task ThenEveryDishIsComplete()
    {
        var dishes = await Actor.AsksForAsync(new TheDishesShown());
        var incomplete = dishes
            .Where(d => string.IsNullOrWhiteSpace(d.Name) || d.Price <= 0 || string.IsNullOrWhiteSpace(d.DishType))
            .Select(d => d.Id)
            .ToList();

        await Assert.That(incomplete).IsEmpty();
    }

    [When("a guest opens the home page")]
    public Task WhenGuestOpensHomePage() => Actor.AttemptsToAsync(new OpenTheHomePage());

    [Then("the home page is shown")]
    public async Task ThenHomePageIsShown() =>
        await Assert.That(await Actor.AsksForAsync(new TheHomePageLoaded())).IsTrue();
}
