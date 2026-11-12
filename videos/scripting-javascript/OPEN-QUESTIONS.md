# Open questions: scripting-javascript

What was read from the source, and what a person must still confirm on a running site or with the voice.

## The voice

`narrate.py` replaces the keys of `"spoken"` as plain substrings, so each key is a whole phrase or identifier. Check each
spelling with `scripts/pronounce.py` before narrating:

- `uuid` is spoken as "U.U.I.D." (a guess; it could be read as one word).
- "The js prefix" is spoken as "The J.S. prefix". The step `run-1` is the only one that says "js".
- `IGlobalMethodProvider` is spoken as "I Global Method Provider" and `IScriptingManager` as "I Scripting Manager".
- Not in the `"spoken"` map, so check how the voice reads them: "Jint" (steps `run-1`, `limits-1`), "gzip", and the
  camel-case names `queryString`, `setVariable`, `setOutcome`, `isAnonymous`, `executeQuery`, `newContentItemAsync`,
  `EnginePoolSize`, `MaxStatements`, `TimeoutInterval` and `LimitMemory`.

## The recorded clip `03-workflow` (the designer cannot be run from the source)

- Whether the **Script** field of the Script Task is fully visible in the activity panel after the two `Shift+ArrowUp`
  presses of the preparation. The panel opens at 40% of the designer's height
  (`Assets/designer/src/styles/designer.css:197`) and the editor has a minimum height of 400 pixels
  (`Views/Items/ScriptTask.Fields.Edit.cshtml:16`). If the script is cut off in the contact sheet of `workflow-2`, press
  `Shift+ArrowUp` a third time in the preparation and check that **Compute the total** (`workflow-3`) can still be
  reached above the panel.
- Whether the designer keeps the panel's height from the preparation to the capture. It is stored as a per-browser
  preference (`Assets/designer/src/ui/preferences.ts`), but the storage and its key come from a shared helper that is
  not part of the source read here.
- Whether the **Script** editor renders its text (the Monaco editor loads from `~/OrchardCore.Resources/Scripts/monaco-esm/`)
  before the narration of `workflow-2` is far along. The wait for `.monaco-editor` should cover it.
- Whether the small text with the activity type (**Script Task**, **Set Variable Task**) shows in the panel header. It is
  hidden when the panel is narrower than 48 rem (`designer.css:287-291`), which a 1536 pixel viewport should not trigger.
- Whether the **Functions** list of `workflow-4` starts with `workflowId()`, `setProperty(...)` and `setVariable(...)` at
  the top of the visible part, as the order of `DefaultWorkflowGlobalValueProvider` suggests. The step ends on the
  start of the list and the narration does not name an entry, so the exact entries do not matter.
- Whether opening an activity marks its form as changed (the first load of the code editor may), which would apply it and
  leave a draft of the workflow. The plan restores the snapshot before every take, so a draft does not matter for the
  next one, but a person should check that no change is visible on the canvas.
- Whether running **Workflow Variables Samples** works in a single run, since its `feature` step enables the Workflows
  features in the same run that imports the workflows. If its `WorkflowType` step fails, enable **Workflows** and
  **HTTP Workflows Activities** under **Tools**, **Features** first, and run the recipe again.
- The recipe appears on **Tools**, **Recipes** in the group of the module it belongs to. Use the search box of that page
  to find it.

## Statements read from the source and never executed

- "A script stopped by a limit faults the workflow": read in `JavaScriptWorkflowScriptEvaluator.IsStoppedEvaluation`
  (the exceptions are not caught there) and the `catch` of `WorkflowManager.cs:717-775`. A workflow with a retry policy on
  the activity retries it first. A person can confirm it with `options.MaxStatements(10)` and a Script Task that loops.
- "In a workflow, a script that throws or returns the wrong type falls back to a default, and the error is logged":
  `JavaScriptWorkflowScriptEvaluator.cs:50-57`. In a recipe nothing catches the error, so the step fails; the video does
  not say otherwise.
- "A string with no known prefix comes back unchanged": `DefaultScriptingManager.cs:50-58`.
- "External logins can run a script": `ScriptExternalLoginEventHandler.cs`, switched on by the settings
  "Use a script to set user roles, user claims and user properties based on external provider claims"
  (`OrchardCore.Users/Views/ExternalLoginSettings.Edit.cshtml:26`) and by `UseScriptToGenerateUsername`. Not shown on screen.
- "A layer rule has a Javascript condition": the label comes from
  `OrchardCore.Rules/Views/Items/JavascriptCondition.Fields.Thumbnail.cshtml:3`. The layer rule screen is not shown.
- "Each tenant also reuses a small pool of engines, sized by EnginePoolSize": `JavaScriptEngine.cs:81-87`,
  `JavaScriptEngineOptions.cs:37`.
- "Scripts are cached by their text": `JavaScriptEngine.cs:214-218` (the prepared script is cached in `IMemoryCache` under
  a key made of the script). The Users module's own comment says the same (`ScriptExternalLoginEventHandler.cs:12-14`).

## Not said in the video, on purpose

- `file:base64`: `FilesScriptEngine.cs:50-53` calls `memoryStream.WriteTo(fileStream)`, which copies from the empty memory
  stream into the file's read stream, so it cannot return the file's content. The video only names the `file` prefix as
  reading a text file.
- The recipe expression `[js: ...]` ends at the last `]`: `RecipeExecutor` removes every leading `[` and trailing `]`
  of the value (`value.Trim('[', ']')`, line 287), so a script that itself ends with `]` loses it. The slides do not use one.
