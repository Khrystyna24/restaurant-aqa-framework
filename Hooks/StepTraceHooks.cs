using Reqnroll.BoDi;
using Restaurant_Aqa_Framework.Contexts;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Hooks;

/// <summary>
/// Wraps every Gherkin step of a `@ui` scenario in a Playwright trace group titled like the step in
/// the feature file ("When the actor opens the menu page"), so the trace viewer's action list reads
/// as the scenario does and every click/request/API call is nested under the step that caused it.
/// `@api` scenarios have no browser context (a standalone `IAPIRequestContext` has no tracing),
/// hence the `ui` tag filter. `[AfterStep]` also runs for a failed step, so groups are always closed.
/// </summary>
[Binding]
public class StepTraceHooks(IObjectContainer container, ScenarioContext scenarioContext)
{
    [BeforeStep("ui")]
    public async Task BeginStepGroup()
    {
        var stage = (UiStageContext)container.Resolve<IStageContext>();
        var step = scenarioContext.StepContext.StepInfo;
        await stage.BrowserContext.Tracing.GroupAsync($"{step.StepDefinitionType} {step.Text}");
    }

    [AfterStep("ui")]
    public async Task EndStepGroup()
    {
        var stage = (UiStageContext)container.Resolve<IStageContext>();

        // A group's title can't change once opened, so a failed step gets a ❌ marker (with the first
        // line of the error) nested inside it, right where the failure happened.
        if (scenarioContext.TestError is { } error)
            await stage.BrowserContext.TraceError($"Step failed: {FirstLine(error.Message)}");

        await stage.BrowserContext.Tracing.GroupEndAsync();
    }

    private static string FirstLine(string message)
    {
        var line = message.Split('\n', 2)[0].Trim();
        return line.Length > 200 ? line[..200] + "…" : line;
    }
}
