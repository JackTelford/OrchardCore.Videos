# Recording plan: scripting-javascript

The video has one recorded clip, `03-workflow`, and slides everywhere else. The slides are drawn by `slides.py` (nothing
to record). Paths below are relative to the Orchard Core checkout (`OrchardCMS/OrchardCore`) unless they start with
`videos/`. In this file a selector such as `[data-cy='activity-readquery']` stands for
`scene.Page.Locator("[data-cy='activity-readquery']")`.

## The site it needs

- A local, throwaway site: `src/OrchardCore.Cms.Web` of an `OrchardCore` checkout, on `http://localhost:5010`, with its
  own `ORCHARD_APP_DATA` folder outside both repositories.
- Auto Setup (no setup or sign-in form is recorded): the **Blog** recipe, site name **Orchard Core**, time zone
  `America/Los_Angeles`, tenant `Default`, user `admin`, email `admin@example.com`, password `<password>` (the recorder
  reads it from `VIDEO_PASSWORD`; it is chosen outside git and never written in this repository).
- Outbound calls blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. Nothing in this video calls an external
  service, so there is no local stand-in to run.
- Features: **Workflows** (`OrchardCore.Workflows`) and **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`).
  `src/OrchardCore.Modules/OrchardCore.Workflows/Manifest.cs:13-26`. The recipe below enables both with its `feature`
  step, so nothing needs enabling by hand. If the recipe reports an error for its `WorkflowType` step, enable both
  features first under **Tools**, **Features**, and run the recipe again (see `OPEN-QUESTIONS.md`).
- Settings: none to change.
- Seed data (made outside the capture, with fake data that ships in the Orchard Core source): the recipe
  **Workflow Variables Samples** (`src/OrchardCore.Modules/OrchardCore.Workflows/Recipes/workflows-variables-samples.recipe.json:3`).
  1. Sign in as `admin`, open **Tools**, **Recipes** (`src/OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:18-19`).
  2. In the list, find **Workflow Variables Samples**, click its **Run** button, and click **Yes** in the confirmation
     (`src/OrchardCore.Modules/OrchardCore.Recipes/Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:15,18`).
  3. Wait for the notification "The recipe 'Workflow Variables Samples' has been run successfully."
     (`src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:142`).
  4. Open `/Admin/Workflows/Types`. It must list three workflows: **Sample: order total**,
     **Sample: format a greeting** and **Sample: greet through another workflow** (recipe lines 26 and following).
     They hold no personal data.
- Sign in once outside the capture: `Recorder setup` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`) saves
  `state.json`, which is git-ignored.
- Copy the site's `App_Data` folder and `state.json` aside now: that is the snapshot to restore.

## Order, dependencies and snapshots

- Only `03-workflow` is recorded. It depends on the seed data above and on nothing else.
- The designer saves its draft as it goes, so a take can leave a draft behind (more so if a node is dragged or a field is
  edited). Stop the site, restore the snapshot, and start it again before every new take.
- The narration comes first: run `narrate.py` before recording, so the recorder holds each step for its narration.

## Clip `03-workflow` (chapter 03, "Scripts in a workflow")

Starts on `/Admin/Workflows/Types`, the **Workflows** list under **Design**, **Workflows**
(`src/OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18-19`,
`src/OrchardCore.Modules/OrchardCore.Workflows/Controllers/WorkflowTypeController.cs:87`).

Before `StartCaptureAsync` (not recorded), prepare the activity panel once, so the Script field is fully visible:

1. `scene.OpenAsync("/Admin/Workflows/Types")`, then click the link **Sample: order total**
   (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Sample: order total", Exact = true })`) and wait until
   `[data-cy='activity-readquery']` is visible.
2. Double-click `[data-cy='activity-readquery']` and wait until `[data-cy='activity-panel']` is visible.
3. Focus `[data-cy='activity-panel-resizer']` (its label is "Resize the activity panel") and press `Shift+ArrowUp` twice.
   The panel grows by 128 pixels and the designer remembers the height for this browser
   (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/panel/ActivityPanel.vue:370-378,398-407`).
4. `scene.OpenAsync("/Admin/Workflows/Types")` again, then `StartCaptureAsync()`.

Do not pin the panel. Check the contact sheet of `workflow-2`: the whole script must be visible (see
`OPEN-QUESTIONS.md` if it is not).

### `workflow-1`

- Actions: `ClickAndWaitForNavigationAsync` on the link **Sample: order total** (same locator as above). Then wait until
  `[data-cy='activity-readquery']` and `[data-cy='activity-computetotal']` are both visible.
- On screen when the narration ends: the designer with the canvas of the workflow and its activities, among them
  **Read the query string** and **Compute the total**. The activity panel is closed.
- Labels: the link text comes from the workflow's name (recipe line 26,
  `src/OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`); the activity
  titles come from the recipe (lines 82-90 and 100-108).

### `workflow-2`

- Actions: `DoubleClickAsync` on `[data-cy='activity-readquery']`. Wait until `[data-cy='activity-panel']` is visible and
  `[data-cy='panel-activity-form'] .monaco-editor` is visible. Do not type anything.
- On screen when the narration ends: the activity panel titled **Read the query string**, with the small text
  **Script Task**, the **Settings** tab selected, the **Available Outcomes** field holding `Done`, and the **Script**
  editor showing the script from `if (queryString('customer'))` down to `setOutcome('Done');`.
- Labels: "Script Task" `src/OrchardCore.Modules/OrchardCore.Workflows/Activities/ScriptTask.cs:23`; "Available
  Outcomes" and "Script" `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/ScriptTask.Fields.Edit.cshtml:5,14`;
  "Settings" `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:79`; the script is
  in the recipe (lines 82-97).

### `workflow-3`

- Actions: `DoubleClickAsync` on `[data-cy='activity-computetotal']`. Wait until `[data-cy='panel-activity-title']`
  contains the text "Compute the total" and `[data-cy='panel-activity-form']` contains the label **Value**. Then
  `HighlightAsync(scene.Page.GetByLabel("Syntax of Value"))`, with no caption.
- On screen when the narration ends: the panel titled **Compute the total**, with the small text **Set Variable Task**,
  the **Variable** field holding `total`, and the **Value** field with the syntax list set to **JavaScript**, ringed in
  green, next to the expression `variable('quantity') * variable('unitPrice')`.
- Labels: "Set Variable Task" `.../Activities/SetVariableTask.cs:30`; "Variable" and "Value"
  `.../Views/Items/SetVariableTask.Fields.Edit.cshtml:5,17`; the syntax list is labeled "Syntax of Value"
  (`.../Views/WorkflowExpressionEditor.cshtml:30,46`) and offers "Literal", "Liquid" and "JavaScript"
  (`.../Expressions/LiteralExpressionProvider.cs:28`, `LiquidExpressionProvider.cs:31`, `JavaScriptExpressionProvider.cs:21`);
  the expression is in the recipe (lines 100-112).

### `workflow-4`

- Actions: `ClearHighlightsAsync()`, then click `[data-cy='activity-tab-data']` (the tab **Available data**), wait until
  `[data-cy='available-data']` is visible, then click `[data-cy='available-group-functions']` (the group **Functions**).
  Do not scroll the list.
- On screen when the narration ends: the **Available data** tab of the **Compute the total** panel, with the group
  **Functions** selected and the start of its list visible (entries such as `workflowId()`, `setProperty("name", value)`
  and `setVariable("name", value)`).
- Labels: "Available data" `.../Services/WorkflowsDesignerJSLocalizer.cs:77`; "Functions" `...:86`; the entries
  `src/OrchardCore.Modules/OrchardCore.Workflows/Services/DefaultWorkflowGlobalValueProvider.cs:66-74`.

## The slides (nothing to record)

Each slide step is drawn by `slides.py`. The code of the panels comes from these files of `OrchardCMS/OrchardCore`,
trimmed with `// ...`.

- `run-1`: four cards, the shape of a script (`js: new Date().toISOString()`), the js engine, the file engine, and
  "Always enabled" (`src/OrchardCore.Modules/OrchardCore.Scripting/Manifest.cs:10`).
- `run-2`: four cards, where scripts are evaluated: recipes, workflows, layer rules, external logins
  (`src/OrchardCore/OrchardCore.Recipes.Core/Services/RecipeExecutor.cs:237-300`,
  `src/OrchardCore.Modules/OrchardCore.Workflows/Scripting/JavaScriptWorkflowScriptEvaluator.cs`,
  `src/OrchardCore.Modules/OrchardCore.Rules/Services/JavascriptConditionEvaluator.cs`,
  `src/OrchardCore.Modules/OrchardCore.Users/Handlers/ScriptExternalLoginEventHandler.cs`).
- `recipe-1`: a recipe step with `[js: uuid()]` and `[file:text('Snippets/footer.html')]`, the lines that carry the
  scripts bright (`src/OrchardCore.Themes/TheAgencyTheme/Recipes/agency.recipe.json:848-872`).
- `recipe-2`: a recipe with a `variables` section and the three recipe methods `variables`, `parameters` and
  `configuration` (`agency.recipe.json:14,104,858`; `src/docs/reference/modules/Scripting/README.md:292` for
  `configuration`).
- `methods-1`: three cards of the methods that modules register (`Scripting/Startup.cs`, `Providers/*.cs`,
  `OrchardCore.Infrastructure/Scripting/CommonGeneratorMethods.cs`,
  `OrchardCore.Infrastructure/Entities/Scripting/IdGeneratorMethod.cs`, `OrchardCore.Contents/Scripting/ContentMethodsProvider.cs`,
  `OrchardCore.Layers/Services/DefaultLayersMethodProvider.cs`, `OrchardCore.Queries/QueryGlobalMethodProvider.cs`).
- `methods-2`: three cards of the methods scoped to one evaluation: recipe, workflow, Script Task
  (`OrchardCore.Recipes.Core/*MethodProvider.cs`, `OrchardCore.Workflows/Scripting/WorkflowMethodsProvider.cs:92`,
  `OutcomeMethodProvider.cs`).
- `add-1`: the code of `IdGeneratorMethod`
  (`src/OrchardCore/OrchardCore.Infrastructure/Entities/Scripting/IdGeneratorMethod.cs:6-22`).
- `add-2`: a `Startup` class that registers `MyGlobalMethodProvider` as a singleton (the line of
  `src/docs/reference/modules/Scripting/README.md:60`), and three sentences about the singleton, the lazy factory and
  its services (`JavaScriptEngine.cs:243-290`).
- `add-3`: the `newContentItem` method of `ContentMethodsProvider` with `Method` and `AsyncMethod`
  (`src/OrchardCore.Modules/OrchardCore.Contents/Scripting/ContentMethodsProvider.cs:17-26`), those lines bright.
- `eval-1`: a method that calls `IScriptingManager.EvaluateAsync` with `js: new Date().toISOString()`, and three
  sentences about the scope (`src/OrchardCore/OrchardCore.Infrastructure/Scripting/DefaultScriptingManager.cs:77-107`).
- `eval-2`: the scoped `ContextMethodProvider` and the `EvaluateAsync` call of the Users module
  (`ScriptExternalLoginEventHandler.cs:15,48,111-126`).
- `limits-1`: the `services.Configure<Jint.Options>` snippet and three cards
  (`src/docs/reference/modules/Scripting/README.md:81-86`).
- `limits-2`: three cards: a script that throws, a script stopped by a limit, and the setting
  **Fault the workflow on script errors**
  (`src/OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowDesigner/WorkflowDesignerSettings.cshtml:70`,
  `JavaScriptWorkflowScriptEvaluator.cs:50-76`).
- `limits-3`: two cards: the stack overflow guard (`OrchardCore.Scripting.JavaScript/ServiceCollectionExtensions.cs:49`)
  and the engine pool (`JavaScriptEngineOptions.cs:37`).

## Checks for private data

- Nothing in this video comes from a real site: the workflows come from the recipe in the source, the user is `admin`
  with `admin@example.com`, and the password is never typed or shown.
- After recording, look at the contact sheet of `03-workflow`: no email other than `admin@example.com`, no real host
  name (the address bar is not recorded), no error message in the designer. The canvas shows the activity **Order requested**
  with the text `GET` (`.../Http/Views/Items/HttpRequestEvent.Fields.Design.cshtml`), but no URL and no token: the panel
  of that activity, which would show its URL and token, is never opened.
- OCR the finished video once per second and at every scene change, as the skill asks, and search for names, emails,
  phone numbers and tokens.
