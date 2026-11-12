# Recording plan: Workflows in Depth

This plan is for the person who records the video. It follows the storyboard (`storyboard.json`) chapter by chapter.
A selector written as `[data-cy="..."]` stands for `scene.Page.Locator("[data-cy=\"...\"]")`. Every label comes from the
Orchard Core source, with the file and line it is written in. Paths start in `src/OrchardCore.Modules/OrchardCore.Workflows/`
unless they say otherwise; `Assets/designer/src/` holds the designer, and `Services/WorkflowsDesignerJSLocalizer.cs`
holds the texts the designer shows (its lines are written `Localizer:<line>`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, provisioned by Auto Setup with the **Blog** recipe (it enables Contents,
  Deployment and Recipes, which this video uses), tenant `Default`, site name **Orchard Core**, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in
  `VIDEO_PASSWORD`). Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. No external service
  is needed.
- **Feature to enable before recording**: **Workflows** (Tools, Features: search for `Workflows`, then press **Enable**
  on the row named `Workflows`; not Timers, HTTP or Session). Its dependencies, Liquid and Scripting, come with it
  (`Manifest.cs:12-18`, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`).
- **Settings**: none. The defaults of the Workflows module are used, and no clip changes a setting.
- **Seed data (create it outside the capture)**: one workflow, **Sample: count the coffees**, made with Tools,
  Deployments, **JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`). Paste the JSON below in the **JSON** editor
  and press **Import** (`OrchardCore.Deployment/Views/Import/Json.cshtml`). The page then shows
  **Recipe imported successfully!** (`OrchardCore.Deployment/Controllers/ImportController.cs:185`). Open Design,
  **Workflows** (`AdminMenu.cs:19`) and check that the list holds **Sample: count the coffees**. The import publishes the
  workflow (`Recipes/WorkflowTypeStep.cs`), which **Run…** needs.

```json
{
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "samplecountcoffees",
          "Name": "Sample: count the coffees",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "coffee",
              "TypeName": "number",
              "DefaultValue": 0,
              "Description": "How many items of the list are Coffee."
            },
            {
              "Name": "summary",
              "TypeName": "string",
              "Description": "The result, written in Liquid.",
              "IsOutput": true
            }
          ],
          "Activities": [
            {
              "ActivityId": "started",
              "Name": "StartedByWorkflowEvent",
              "X": 20,
              "Y": 40,
              "IsStart": true,
              "Properties": {}
            },
            {
              "ActivityId": "eachitem",
              "Name": "ForEachTask",
              "X": 320,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Each item"
                },
                "Enumerable": {
                  "Expression": "Tea, Coffee, Cocoa, Coffee",
                  "Syntax": "Literal"
                },
                "LoopVariableName": "item"
              }
            },
            {
              "ActivityId": "checkitem",
              "Name": "IfElseTask",
              "X": 620,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Check the item"
                },
                "Condition": {
                  "Expression": "property('item') == 'Coffee'",
                  "Syntax": "JavaScript"
                }
              }
            },
            {
              "ActivityId": "countcoffee",
              "Name": "SetVariableTask",
              "X": 920,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Count the coffee"
                },
                "VariableName": "coffee",
                "Value": {
                  "Expression": "variable('coffee') + 1",
                  "Syntax": "JavaScript"
                }
              }
            },
            {
              "ActivityId": "writesummary",
              "Name": "SetVariableTask",
              "X": 320,
              "Y": 320,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": "Write the summary"
                },
                "VariableName": "summary",
                "Value": {
                  "Expression": "Coffee items: {{ Workflow.Variables.coffee }}",
                  "Syntax": "Liquid"
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "started",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "eachitem"
            },
            {
              "SourceActivityId": "eachitem",
              "SourceOutcomeName": "Iterate",
              "DestinationActivityId": "checkitem"
            },
            {
              "SourceActivityId": "eachitem",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "writesummary"
            },
            {
              "SourceActivityId": "checkitem",
              "SourceOutcomeName": "True",
              "DestinationActivityId": "countcoffee"
            },
            {
              "SourceActivityId": "checkitem",
              "SourceOutcomeName": "False",
              "DestinationActivityId": "eachitem"
            },
            {
              "SourceActivityId": "countcoffee",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "eachitem"
            }
          ]
        }
      ]
    }
  ]
}
```

- **What the workflow does**: it starts with **Started By Workflow**, loops over the literal list
  `Tea, Coffee, Cocoa, Coffee` with **For Each** (looping variable `item`), tests each item with **If/Else**
  (`property('item') == 'Coffee'`), adds one to the `coffee` variable for each coffee (**Set Variable**, JavaScript),
  and, when the list ends, writes `Coffee items: 2` to the `summary` variable (**Set Variable**, Liquid). `summary` is an
  output variable, so the Run dialog reports it.
- **Sign-in**: sign in once outside the capture (`Recorder setup`), which saves `state.json` (never committed).
- **Snapshot**: when the seed is done and `state.json` is saved, stop the site and copy its `App_Data` folder aside.

## The order of the clips

Record them in this order: `01-designer`, `02-declare`, `03-canvas`, `04-run`. The first three only look at the
workflow and change nothing. **`04-run` changes the site**: it runs the workflow once, which adds one instance. To
take it again, restore the `App_Data` snapshot (or delete the instance from the workflow's **Instances** list) first.
The slide clips (`01-syntaxes`, `02-use`, `03-outcomes`, `04-loops`, `05-extend`) need no site and no order.

After each of the first three clips, check the designer toolbar: it must say **All changes saved** and **Discard draft**
must be disabled (`Localizer:166`, `Localizer:200`, `Assets/designer/src/App.vue:754`). If not, restore the snapshot
before the next take.

## How every designer clip starts (not recorded)

Do this before `StartCaptureAsync()` in `01-designer`, `02-declare`, `03-canvas` and `04-run`:

1. `scene.OpenAsync("/Admin/Workflows/Types")`.
2. Click the link whose text is `Sample: count the coffees` (`Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml`). The
   designer opens at `/Admin/Workflows/Types/Edit/<id>`. Wait until `[data-cy="activity-eachitem"]` and
   `[data-cy="activity-writesummary"]` are visible.
3. Click `[data-cy="toolbox-collapse"]` (**Hide the activities**, `Localizer:66`), so that the canvas has more room. The
   designer remembers a collapsed toolbox in the browser, and the button is hidden while the toolbox is collapsed. Each
   scene starts a new browser context from `state.json` (`Scene.StartAsync`), so the toolbox starts expanded; if the button's
   `aria-expanded` is `false`, skip the click.
4. Click `[data-cy="zoom-fit"]` (**Fit to content**, `Localizer:358`). All five activities must be visible, and none
   under the workflow panel on the right. The **Variables** tab of the workflow panel is the tab open by default
   (`Assets/designer/src/panel/WorkflowPanel.vue:59`).
5. Move the pointer to an empty part of the canvas.

## Chapter 00: opening card

`intro` is a card. Nothing to record.

## Chapter 01: Expressions

### `01-syntaxes` (slides)

- `syntaxes-1`: three cards, Literal, Liquid and JavaScript, each with one line on how its text is read
  (`Expressions/LiteralExpressionProvider.cs`, `Expressions/LiquidExpressionProvider.cs`,
  `Expressions/JavaScriptExpressionProvider.cs`).
- `syntaxes-2`: the stored condition of the workflow (`Expression` and `Syntax`) on the left, and the three examples of
  the If/Else editor on the right: Literal `true`, Liquid `{{ Workflow.Properties.Count > 0 }}`, JavaScript
  `input("Count") > 0` (`Views/Items/IfElseTask.Fields.Edit.cshtml:11-13`).

### `01-designer` (recording)

Starts on the designer (see above), with no activity panel open.

- `designer-1`: `DoubleClickAsync([data-cy="activity-eachitem"])` (the card titled **Each item**, whose type is
  **For Each Task**: `Activities/ForEachTask.cs:27`). Wait until `select[aria-label="Syntax of Items"]` is visible
  (`Views/WorkflowExpressionEditor.cshtml:46`; the label **Items** is in `Views/Items/ForEachTask.Fields.Edit.cshtml:7`),
  then `HighlightAsync` it with the caption `Literal`. When the narration ends, the activity panel is open under the
  canvas on the **Settings** tab (`Localizer:79`): the syntax select shows **Literal**, the text box next to it shows
  `Tea, Coffee, Cocoa, Coffee`, and **Looping Variable** below shows `item`
  (`Views/Items/ForEachTask.Fields.Edit.cshtml:19`).
- `designer-2`: `ClearHighlightsAsync()`, click `button[aria-label="Close the activity panel"]` (`Localizer:151`), then
  `DoubleClickAsync([data-cy="activity-checkitem"])` (**Check the item**, type **If Else Task**,
  `Activities/IfElseTask.cs:25`). Wait until `select[aria-label="Syntax of Condition"]` is visible
  (`Views/Items/IfElseTask.Fields.Edit.cshtml:7`) and `HighlightAsync` it with the caption `JavaScript`. When the
  narration ends: the select shows **JavaScript** and the text box shows `property('item') == 'Coffee'`.
- `designer-3`: `ClearHighlightsAsync()`, close the activity panel the same way, then
  `DoubleClickAsync([data-cy="activity-writesummary"])` (**Write the summary**, type **Set Variable Task**,
  `Activities/SetVariableTask.cs:30`). Wait until `select[aria-label="Syntax of Value"]` and `[data-expression-monaco]`
  are visible (`Views/WorkflowExpressionEditor.cshtml:30`, `Views/Items/SetVariableTask.Fields.Edit.cshtml:5-17`) and
  `HighlightAsync` the select with the caption `Liquid`. When the narration ends: **Variable** shows `summary`, the
  select shows **Liquid**, and the code editor shows `Coffee items: {{ Workflow.Variables.coffee }}`.
- `designer-4`: `ClearHighlightsAsync()`, click `[data-cy="activity-tab-data"]` (**Available data**, `Localizer:77`). Wait
  until `[data-cy="available-data"]` is visible, then `HighlightAsync` it. Do not click an expression: that would
  insert it in the setting. When the narration ends: the **Variables** group (with the count `2`) is selected, and lists
  `coffee` and `summary` with their JavaScript and Liquid expressions; the other groups (for example **Workflow**,
  **Global** and **Functions**) are listed beside it (`Assets/designer/src/available/AvailableData.vue`).

## Chapter 02: Variables

### `02-declare` (recording)

Starts on the designer (see above), with no activity panel open, on the **Variables** tab.

- `declare-1`: `HighlightAsync([data-cy="variables-tab"])` for about 3 seconds, `ClearHighlightsAsync()`, then
  `HighlightAsync` on the **Output** check box of the second variable,
  `scene.Page.Locator('[data-cy="variable"]').Nth(1).Locator('[data-cy="variable-is-output"]')`, with the caption
  `Output` (`Assets/designer/src/panel/VariablesTab.vue:358`, label `Localizer:104`). When the narration ends: two rows
  are visible. `coffee` has the type **Number** and the default `0`. `summary` has the type **Text**, no default, and
  **Output** checked; **Input** is unchecked on both.

### `02-use` (slides)

- `types-1`: the `Variables` JSON of the workflow on the left, and a table on the right: the type names of the designer
  (Text, Number, Yes or no, Date and time, Object, List, Content item, Any) next to the names a definition stores
  (`Variables/*VariableType.cs`, `OrchardCore.Contents/Workflows/Variables/ContentItemVariableType.cs:15`). **Content item**
  is in the designer's list because the Contents feature is enabled, and a line under the table says so.
- `access-1`: a JavaScript panel (`variable('coffee') + 1;` and `setVariable('coffee', 3);`), a Liquid panel
  (`Coffee items: {{ Workflow.Variables.coffee }}`), and a card for **Set Variable**.
- `access-2`: the script of the sample recipe (`setOutcome('Done');` and a value on the last line) next to its
  `OutputBindings` (`"Result": "discount"`, in a panel titled **Activity properties**: the bindings are a property of the activity), and a card for the **Outputs** tab and **Store in**
  (`Recipes/workflows-variables-samples.recipe.json`, `Localizer:116`, `Localizer:121`).

## Chapter 03: Branches

### `03-outcomes` (slides)

- `paths-1`: two cards, **If/Else** (True or False from its **Condition**) and **Script** (its own outcomes in
  **Available Outcomes**, `Views/Items/ScriptTask.Fields.Edit.cshtml:5`).
- `paths-2`: the state machine script of the Workflows documentation (`setOutcome(variable('state'));`) with two cards,
  **Available Outcomes** and `setOutcome`.

### `03-canvas` (recording)

Starts on the designer (see above), with no activity panel open.

- `canvas-1`: `HighlightAsync([data-cy="port-checkitem-True"], "True")`, wait 3 seconds, `ClearHighlightsAsync()`,
  `HighlightAsync([data-cy="port-checkitem-False"], "False")`. When the narration ends: the **Check the item** card is
  visible with its two outcome ports, **True** and **False**, each with a line to another activity
  (`Assets/designer/src/canvas/OutcomePort.vue:30`).
- `canvas-2`: `ClearHighlightsAsync()`, click `[data-cy="panel-tab-workflow"]` (**Workflow**, `Localizer:67`). Wait until
  `scene.Page.GetByLabel("Outcomes with several transitions")` (the select, found by its label whatever prefix the form adds
  to the field name; `select[name="BranchingMode"]` is the same field) is attached, call `ScrollIntoViewIfNeededAsync()` on it,
  then `HighlightAsync` it. Do not
  change it. When the narration ends: the label **Outcomes with several transitions** and its select, showing **Follow
  the first transition only**, are visible with their hint (`Views/WorkflowDesigner/WorkflowDesignerSettings.cshtml:56-62`).

## Chapter 04: Loops

### `04-loops` (slides)

- `loops-1`: three cards: **For Each** (Items, Looping Variable), **For Loop** (From, To, Step; To is not included) and
  **While Loop** (Condition) (`Views/Items/ForEachTask.Fields.Edit.cshtml`, `ForLoopTask.Fields.Edit.cshtml`,
  `WhileLoopTask.Fields.Edit.cshtml`).
- `loops-2`: three cards: **Iterate**, **Back to the loop** and **Done**.
- `loops-3`: `ForEachTask.ExecuteAsync`, trimmed (`Activities/ForEachTask.cs:94-117`).

### `04-run` (recording)

Starts on the designer (see above), with no activity panel open. This clip changes the site (one instance).

- `run-1`: `HighlightAsync([data-cy="port-eachitem-Iterate"], "Iterate")` and wait 2.5 seconds; `ClearHighlightsAsync()`;
  `HighlightAsync([data-cy="activity-checkitem"])` and wait 2.5 seconds; `ClearHighlightsAsync()`;
  `HighlightAsync([data-cy="activity-countcoffee"])` and wait 3 seconds; `ClearHighlightsAsync()`;
  `HighlightAsync([data-cy="port-eachitem-Done"], "Done")` and wait 2.5 seconds. When the narration ends: the canvas
  shows the five activities, the lines back into **Each item**, and the **Done** port highlighted.
- `run-2`: `ClearHighlightsAsync()`, click `[data-cy="toolbar-run"]` (**Run…**, `Localizer:286`). Wait until
  `[data-cy="run-dialog"]` (**Run the workflow**) is visible and `[data-cy="run-start"]` is enabled. Click
  `[data-cy="run-start"]` (**Run**, `Localizer:282`). Wait until `[data-cy="run-status"]` has the text `Finished` and
  `[data-cy="run-output-summary"]` is visible (`Assets/designer/src/draft/RunDialog.vue:253-274`). When the narration
  ends: the dialog shows **Result**, the instance status **Finished**, and the output `summary` with its value
  (expected: `Coffee items: 2`).
- `run-3`: `ClickAndWaitForNavigationAsync([data-cy="run-open-instance"])` (**Open the instance**, `Localizer:311`). Wait
  until `[data-cy="activity-eachitem"]` and `[data-cy="executed-badge"]` are visible (the viewer fits the workflow in its
  frame by itself, `Assets/designer/src/App.vue:589-595`), then `HighlightAsync` the badge of **Each item**:
  `scene.Page.Locator('[data-cy="activity-eachitem"] [data-cy="executed-badge"]')`. When the narration ends: the page of
  the instance is a read-only designer where the activities and lines that ran are highlighted, and the repeated
  activities carry a badge with the number of runs (expected: `×5` on **Each item**, `×4` on **Check the item**, `×2` on
  **Count the coffee**; `Assets/designer/src/canvas/ActivityNode.vue:244`).
- `run-4`: `ClearHighlightsAsync()`, then `HighlightAsync([data-cy="variables-tab"])`; after 3 seconds,
  `ClearHighlightsAsync()` and `HighlightAsync([data-cy="variable-row-summary"])`. When the narration ends: the
  **Variables** tab of the workflow panel lists `coffee` and `summary` with the values of the instance (expected `2` and
  `Coffee items: 2`; `Assets/designer/src/panel/VariablesTab.vue:236-245`).

## Chapter 05: Extending expressions

### `05-extend` (slides)

- `extend-1`: `UpperCaseExpressionProvider` and its registration, from the Workflows documentation ("Adding a Syntax").
- `extend-2`: `IfElseTask`, trimmed: the `Condition` property, and `ExecuteAsync`, which evaluates it with the expression
  manager and returns `True` or `False` (`Activities/IfElseTask.cs`, `OrchardCore/OrchardCore.Workflows.Abstractions/Services/IWorkflowExpressionManager.cs`).
- `extend-3`: `OutcomeMethodProvider` (`Scripting/OutcomeMethodProvider.cs`), created by the Script activity
  (`Activities/ScriptTask.cs:59`).
- `extend-4`: the `Jint.Options` configuration of the Scripting documentation (`MaxStatements`, `TimeoutInterval`), with
  the `using Jint;` that those two extension methods need (`Jint.ConstraintsOptionsExtensions`), and a
  card saying that a script that reaches a limit faults the workflow (`Scripting/JavaScriptWorkflowScriptEvaluator.cs`).
- `extend-5`: a script that fails (`input("Order").Total`), and two cards: the default behavior (logged, default value)
  and the workflow setting **Fault the workflow on script errors**
  (`Views/WorkflowDesigner/WorkflowDesignerSettings.cshtml:70`).

## Chapter 99: closing card

`outro` is a card. Nothing to record.

## Checks for private data

- The only account is `admin` with `admin@example.com`; the password is never typed on camera, and the sign-in is in
  `state.json`.
- The data is `Tea, Coffee, Cocoa, Coffee` and the names in the seed JSON above: no real person, company or address.
- Look at every contact sheet (`shots/<clip>.jpg`): no error toast, no **Unsaved changes** or **Not saved** message in the
  designer toolbar, and no other workflow in the list.
- Scan the finished video by OCR for `@` and for host names: only `example.com` and `localhost` may appear.
