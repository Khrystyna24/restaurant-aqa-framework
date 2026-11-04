# Working in the AQA framework

Guidance for extending these tests, by hand or with an AI assistant. The commands are in `README.md`; this file covers *how* to write things so they fit.

## Layout

| Folder | Holds |
|---|---|
| `Features/` | Gherkin, business language only |
| `StepDefinitions/` | Thin glue from steps to Screenplay tasks and questions; `StepArgumentTransformations.cs` holds the typed parameters |
| `Screenplay/` | `Actor`, its abilities (`BrowseTheWeb`, `CallTheApi`, `RememberThings`), `Tasks/` (things it does) and `Questions/` (things it finds out) |
| `Ui/` | Page objects and components — locators and UI actions only; used by tasks and questions, never by steps |
| `Api/` | One client per resource, DTOs, `Seeding/` (test data + cleanup); used by tasks and questions, never by steps |
| `Hooks/` | Browser lifecycle, per-scenario stage (`@ui`/`@api`), cleanup sweep, trace grouping |
| `Support/`, `Contexts/` | Shared helpers and per-scenario state |
| `Configuration/` | Settings read from `.env` / environment |

## Writing Gherkin

- **Declarative, business language.** Say what the user wants and sees, never how: no URLs, HTTP verbs, status codes, JSON, ids, selectors or button labels. "the visitor is told …", not "response status is 400".
- **Personas** (visitor, guest, customer, waiter, manager) instead of "the user" or "the actor".
- **One behaviour per scenario.** Group related scenarios under a `Rule:` named after the business rule; put shared `Given`s in `Background:` (feature or rule level).
- **Outlines** for the same behaviour over varied data; give each `Examples:` block a name and, where useful, a tag (`@names`, `@boundary`). Name bad inputs ("an invalid first name (too long)") and keep the concrete value in code (`Support/InvalidInputs.cs`).
- **Data tables** for entities and expected facts (`| detail | value |`), **doc strings** only for genuinely multi-line text.
- **Never write data the rule does not care about** — generate it in the step (fresh email, valid password).
- Tags: `@api` / `@ui` pick the stage and are required; `@smoke`, `@negative`, `@validation`, `@data-dependent` describe the scenario; `@known-defect` marks a failing bug on purpose; `@wip` marks a spec without step definitions yet.
- Check before finishing: `npx gherkin-lint -c .gherkin-lintrc "Features/*.feature"`.

## Writing step definitions

- Derive from `StepDefinitionBase`. A step has **one collaborator, the actor**: `await Actor.AttemptsToAsync(new AddDish(type))` to do something, `await Actor.AsksForAsync(new TheDishesShown())` to find something out, then assert. A step **never touches an API client or a page object** and holds **no fields of its own**.
- Keep them **thin**: parse arguments, perform a task, ask a question, assert. No business rules, no locators, no raw URLs, no set-up logic — that belongs in a task.
- **Cucumber expressions** over regex. Turn repeated vocabulary into a named transformation (`[StepArgumentTransformation(Name = "dish_type", Regex = "(…)")]`) — the regex needs its own capture group.
- The same sentence can mean different things per stage: use `[Scope(Tag = "ui")]` / `"api"` rather than branching inside a step.
- **Share state through the actor's memory**, never through private fields, statics or `ScenarioContext.Current`: `Remember(location)` in one step, `Recall<LocationResponse>()` in a step of *any other* class. The memory (`IRemember`/`RememberThings`) is Reqnroll's `ScenarioContext` used through its typed `Set`/`Get`/`TryGetValue`, one per scenario, so parallel scenarios stay apart. Remember as many of a kind as the scenario needs: the latest is "the current" one, a name fetches a specific one (`new SetUpLocation(name: "Hilltop")`, then `actor.Recall<LocationResponse>("Hilltop")`), and `RecallAll<T>()` returns them all.
- Assert in `Then` steps only, with a `.Because(...)` that includes the response body or page state.
- The meaning of an outcome ("sign-in is refused") maps to protocol details in exactly one step class (`OutcomeStepDefinitions`).

## Screenplay, page objects, locators

- An `Actor` holds abilities; a step asks for the one it needs (`actor.AbilityTo<IBrowseTheWeb>()`). A `@ui` actor can also call the API through the browser context.
- **Tasks** (`ITask`) are what the actor does, through the API or the UI: `SetUpLocation`, `AddDish`, `BrowseTheMenu`, `SignInThroughTheService`, `SignInThroughTheLoginForm`, `SignUp`. They recall what they need from memory (`AddDish` finds the location), leave the outcome as the scenario's last response, remember what they create and schedule its removal. A set-up task throws if it fails, so a broken Given stops the scenario there; only a scenario that tests a refusal opts out (`mustSucceed: false`).
- **Questions** (`IQuestion<T>`) are how the actor finds something out: `TheOutcome` (status and error of the last request), `TheDishesShown`, `TheSignUpFormMessages`, `TheSignedInUser`. UI questions do their own waiting, because an answer is a plain value — there is no retrying `Expect` once it has been returned.
- Add a task when two steps would repeat the same action, a question when two steps would read the same thing.
- Page objects expose locators and small actions; components model reusable widgets. They never assert.
- Prefer, in order: role + accessible name, label, test id, then CSS. Avoid positional and text-fragile selectors; use `Exact = true` when names overlap.
- No sleeps. Playwright auto-waits; use `Expect(...)` assertions, which retry.
- Every `@ui` step is wrapped in a trace group named after the Gherkin step; a failed one is marked inside it. Open traces with `npx playwright show-trace`.

## Test data and the shared environment

- Everything the suite creates is prefixed `AQA-Sample-` and removed again. Tasks register what they create with the scenario's single `CleanupScope` (kept in the actor's memory); one hook in `ScenarioStageHooks` disposes it before the stage's request context is closed — a new task needs nothing more than `TrackDelete`.
- A sweep removes leftovers before and after every run. Do not rely on any existing record's content — the environment is shared.
- GET/PUT/DELETE calls retry on a transient 500; POST never does. Do not add blanket retries to hide a real failure.
- Parallelism defaults to the CPU count, capped at 4 (`MAX_PARALLEL_TESTS`). Seed per scenario so scenarios stay independent.

## A failing test is information

Do not `@ignore` a real bug to get green. A scenario that fails because the application is wrong stays red with `@known-defect` and a comment saying what is expected; one that cannot run yet is `@wip`. Fix the test only when the test is wrong.

## Working cost-efficiently with an AI assistant

- **Give it the goal and the nearest example**, not the whole repo: "add a Rule to `Features/MenuCatalog.feature` following the existing Outline; reuse steps from `MenuCatalogStepDefinitions.cs`". It will follow the pattern instead of inventing one.
- **Read before writing, but narrowly.** Point at the files that matter; ask it to search for an existing step before creating a new one.
- **Run the smallest thing that proves the change**: build first, then one tag or filter (`--treenode-filter "/*/*/*/*[Category=smoke]"`), not the full suite against the live environment.
- **Paste the failing step and the first error lines**, not the whole log. For UI failures read the aria snapshot in the error or open the trace before asking for a fix.
- **Plan multi-file changes first**, then implement in small commits; the hooks (`.husky`) check format, Gherkin and commit messages for you, so do not spend prompts on style.
- **Start a fresh conversation per task** and keep instructions short and specific; long histories are re-read on every turn.
- **Use a smaller model for mechanical work** (renames, table edits, more Examples rows) and a stronger one for design (new page object, new ability, flaky-test diagnosis).
- **Verify, don't trust.** Check that a generated scenario is declarative (no payloads, ids or status codes) and that its steps are not duplicates of existing ones.
