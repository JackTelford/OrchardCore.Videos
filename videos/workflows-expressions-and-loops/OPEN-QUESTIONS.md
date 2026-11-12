# Open questions: Workflows in Depth

Everything in the narration was read from the Orchard Core source (the `OrchardCore.Workflows` and `OrchardCore.Scripting`
modules) and from the documentation pages of the tracker entry. No site was run while writing the video, so the points
below are what a person must confirm on a running site before recording. None of them changes the narration unless the
check fails.

## The seeded workflow runs as the narration says

What was checked in the source: `ForEachTask`, `IfElseTask`, `SetVariableTask`, `WorkflowManager`, the `Literal`, `Liquid`
and `JavaScript` expression providers, `WorkflowTypeStep` (the import) and `WorkflowDesignerController.Run`. From them, the
seed in `recording-plan.md` should import, publish, and run to the status **Finished** from **Run…**.

To confirm on a site:

1. Tools, Deployments, **JSON Import** accepts the JSON of the plan as it is (a recipe with only a `steps` list) and shows
   **Recipe imported successfully!**
2. **Run…** in the designer opens the dialog with the text **The workflow has no input variables.**, and the **Run** button
   gives the status **Finished** and the output `summary`.
3. `summary` is `Coffee items: 2`. The number comes from a `double` rendered by Liquid; if it shows another form (for
   example `2.0`), change the Liquid expression of **Write the summary** in the seed and the slide `access-1`, which shows
   `Coffee items: {{ Workflow.Variables.coffee }}`.
4. The comparison `property('item') == 'Coffee'` is true for the two `Coffee` items. The loop variable is stored by
   `ForEachTask` as a .NET string (`Activities/ForEachTask.cs:105`), which Jint should compare as a JavaScript string. If
   the count is not 2, use `String(property('item')) == 'Coffee'` in the seed and in the slide `syntaxes-2`.
5. The instance page shows the badges `×5` (**Each item**: four iterations and the last pass that takes **Done**), `×4`
   (**Check the item**) and `×2` (**Count the coffee**), and the **Variables** tab shows `2` and `Coffee items: 2`. The
   counts are the records of the journal (`Controllers/WorkflowDesignerController.cs:347`), which is on by default
   (`OrchardCore:Workflows:Journal:Enabled`). The narration says only that a badge shows how many times the repeated
   activities ran.

## The canvas

The X and Y values of the seed place the five activities on two rows. The routes of the lines are drawn by the designer
(`Assets/designer/src/canvas/geometry.ts`), which was read but not run. Open the workflow once and check that:

- after **Fit to content** all five activities are visible, and the text of the cards is readable at the size of the video;
- the lines back into **Each item** (from **Count the coffee** and from the **False** outcome of **Check the item**) do not
  cross another card. If they do, change `X` and `Y` in the seed (not by dragging, which changes the draft) and import again;
- the activity panel, when it is open, does not cover the card of the next activity to double-click. The plan closes the
  panel with the **Close the activity panel** button before each double-click to avoid that.

## Selectors that were derived, not seen

- `select[aria-label="Syntax of Items"]`, `"Syntax of Condition"` and `"Syntax of Value"` come from
  `Views/WorkflowExpressionEditor.cshtml:30,46` (`T["Syntax of {0}", Model.Label]`).
- `select[name="BranchingMode"]` comes from `asp-for="BranchingMode"` in `Views/WorkflowDesigner/WorkflowDesignerSettings.cshtml:58`.
  The settings form is loaded by the designer, and it may add a prefix to the field names; if so, select it by its label
  **Outcomes with several transitions** instead.
- `button[aria-label="Close the activity panel"]` comes from the designer texts (`Services/WorkflowsDesignerJSLocalizer.cs:151`).
- The `data-cy` values come from the `.vue` sources of the designer. The built script served to the browser
  (`wwwroot/Scripts/Workflows/designer/workflows-designer.js`) was searched for each of them, and has them all:
  `toolbox-collapse`, `zoom-fit` (the fit control is not in the `.vue` sources, only in the built script),
  `activity-tab-`, `available-data`, `variables-tab`, `variable-is-output`, `port-`, `panel-tab-`, `toolbar-run`,
  `run-dialog`, `run-start`, `run-status`, `run-output-`, `run-open-instance`, `variable-row-` and `executed-badge`. A page
  was not run, so a name that is built at run time (`activity-tab-data`, `port-<id>-<outcome>`, `panel-tab-workflow`) is
  derived from the template, not seen.

## Pronunciation

`storyboard.json` has a `spoken` map: `IWorkflowExpressionProvider`, `IWorkflowExpressionManager`,
`AddWorkflowExpressionProvider`, `Workflow.Variables`, `setVariable`, `setOutcome` and `If/Else`. Check each with
`scripts/pronounce.py` before narrating. Also listen to: `Liquid`, `JavaScript`, `Looping Variable`, and the title
**Check the item**.

## Code on the slides

- `extend-1` (`UpperCaseExpressionProvider`), `extend-4` (`Jint.Options`, `MaxStatements`, `TimeoutInterval`) and `extend-5`
  (`input("Order").Total`) come from the documentation pages, not from compiled code. `Jint` is an external package that is
  not in the source tree that was read. The API documentation shipped in the Jint packages (4.8 to 4.17) lists
  `MaxStatements(Jint.Options, int)` and `TimeoutInterval(Jint.Options, TimeSpan)` as extension methods of
  `Jint.ConstraintsOptionsExtensions`, so the slide `extend-4` starts with `using Jint;` (the Scripting documentation omits
  it). The Jint version that Orchard Core pins is not in the tree that was read. Compile `extend-1` and `extend-4` once in a host project of the Orchard Core version used.
- The narration says that a script that reaches a limit faults the workflow at that activity. It rests on the Scripting
  documentation and on `IsStoppedEvaluation` in `Scripting/JavaScriptWorkflowScriptEvaluator.cs`, which lets those
  exceptions reach the workflow manager. To see it, set `MaxStatements(10)` in a host, run a **Script** with a long loop,
  and check that the instance is **Faulted** at that activity.
- The narration says that the other built-in activities do not have `setOutcome`. A search of the modules of the source
  tree found `OutcomeMethodProvider` used only in `Activities/ScriptTask.cs:59`. A module outside the tree could do
  otherwise.
- The narration of `syntaxes-2` says that a JavaScript result must already have the type the setting expects. It rests on
  `JavaScriptWorkflowScriptEvaluator.EvaluateAsync`, which casts the result with `(T)` (no conversion), and on the catch
  that logs a result of the wrong type and returns the default value. To see it, set the condition of **Check the item**
  to the JavaScript `1` in a copy of the workflow, run it, and check that the instance takes **False** and the page of the
  instance shows a script error. (Literal and Liquid results are converted: `LiteralExpressionProvider.TryConvert`,
  `LiquidWorkflowExpressionEvaluator.ConvertValue`.)
- The narration of `types-1` says that a script logs a failed `setVariable()` by default. It rests on the same catch
  (`WorkflowVariables.Set` throws `WorkflowVariableException` before it changes the variable) and on the Workflows
  documentation ("Reading and Writing Variables"). To see it, run a **Script** with `setVariable('coffee', 'abc')` and check
  that the instance finishes, `coffee` keeps its value, and the log has the error.

## Run… without Usable as an activity

The Workflows documentation describes a workflow that starts with **Started By Workflow** as "usable as an activity". The
seed does not check **Usable as an activity** (`IsActivity` is false), and `WorkflowDesignerController.RunOf` looks only
at the name of the start activity, so **Run…** should still be offered and should run it. Confirm on the site that the
button is in the toolbar and that the dialog reports the output `summary`.
