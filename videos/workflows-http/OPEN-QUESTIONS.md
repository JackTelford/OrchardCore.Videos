# Open questions: HTTP Workflows

Everything the narration states was read in the Orchard Core source at commit `92db4a83` (paths relative to
`src/OrchardCore.Modules/OrchardCore.Workflows`, unless another module is named). The source was read, not run: nothing
here was tried on a running site. A person recording the video confirms the items below on a running site, and tells the
author if one is wrong.

## To confirm on a running site

1. **The seed imports as expected.** The JSON in `recording-plan.md` follows the shape of
   `Recipes/workflows-variables-samples.recipe.json`. Confirm that JSON Import (Tools > Deployments > JSON Import)
   accepts it, that the list shows **Stock check** as enabled, that the HTTP Request event has a URL after the import
   (`Recipes/WorkflowTypeStep.cs:55-64`, which needs `TokenLifeSpan` in the properties), and that the Script, HTTP Request
   task and two HTTP Response activities show the settings the narration reads.
2. **The JSON Import field.** `OrchardCore.Deployment/Views/Import/Json.cshtml:15-17` is a text area, and
   `OrchardCore.Deployment/Assets/ts/import-json.ts:3` turns it into a CodeMirror 5 editor. `recording-plan.md` sets its
   value with `document.querySelector('.CodeMirror').CodeMirror.setValue(json)`. Confirm that the value reaches the
   server when **Import** is clicked (CodeMirror copies it back to the text area when the form is submitted).
3. **The canvas.** The seed places the five activities in two rows (x 40 to 940, y 40 and 330). Confirm they all show
   without scrolling once the activities pane is hidden, and that the node headers read "Http Request Event", "Script
   Task", "Http Request Task" and "Http Response Task" (the display texts of the activities), with the design titles
   "Handle Incoming HTTP Request", "Script", "HTTP Request" and "HTTP Response" in the node bodies.
4. **The activity panel.** Confirm that, for `recv-3`, `call-1` and `call-4`, the fields below the first ones can be
   reached by scrolling the panel (or by pinning it), and that opening an editor and closing it without touching a field
   leaves **Discard draft** disabled. Confirm that the Content, Body and Headers fields (code editors) can be read
   without being focused.
5. **Six activities.** `acts-1` says the HTTP category lists six activities. The catalog and `Http/Startup.cs:44-49`
   register six, and the search matches the display text and the category name (`Assets/designer/src/toolbox/filter.ts:39`).
   The Forms feature adds a seventh activity to the HTTP category (`OrchardCore.Forms/Activities/HttpRedirectToFormLocationTask.cs:30`);
   the Blog recipe does not enable Forms, so the count is 6 on the site of the plan. Confirm the count badge reads 6, that
   no activity of another module matches "HTTP", and that the cards come in the order the plan lists (sorted by display text).
6. **The HTTP Request task reaches the stand-in.** With `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address and
   `NO_PROXY=localhost,127.0.0.1`, confirm that the task calls `http://localhost:5080/A-100.json` (the server binds
   `127.0.0.1`; `localhost` may resolve to `::1` first) and that the 200 and 404 outcomes are followed.
7. **The body variable.** The 200 reply returns `{{ Workflow.Variables.stockBody }}`, filled by the Body output of the HTTP
   Request task through `OutputBindings`, as the Reply task of the samples recipe does with its variable. This avoids
   reading the last result of the task in Liquid: that value is an anonymous object (`Http/Activities/HttpRequestTask.cs:208-214`),
   and the source read here registers only `Input`, `Output`, `Properties` and `Variables` of the workflow context for
   Liquid (`Startup.cs:50-56`), so `{{ Workflow.LastResult.Body }}` was not relied on. Confirm that the reply body is the
   JSON of the stock API, and that the **Outputs** tab shows `stockBody` selected for **Response body**. The narration of
   `call-3` also says the last result holds `Body`, `Headers`, `StatusCode`, `ReasonPhrase` and `IsSuccessStatusCode`; that
   list is the object assigned at `HttpRequestTask.cs:208-214`, and `lastResult().Body` is what the designer's Available
   data offers for it in a script.
8. **`readBody()` in the Script.** `Http/Scripting/HttpMethodsProvider.cs:69-74,119-125` reads the request stream to the
   end, once. Confirm that `JSON.parse(readBody())` gives the product code for a JSON POST made by the Run dialog.
9. **The Run dialog.** Confirm the labels and the flow of `run-1` to `run-3`: **Run…**, **Run the workflow**, **Query
   string parameters** with no parameter, **JSON or text**, **Run**, **Result**, **Response**, **Instance**, **Open the
   instance**. The dialog sends its request to a URL it generates itself, valid for one day
   (`Assets/designer/src/api/designerApi.ts:67-68`), not to the URL of the event. Confirm that the 404 run ends with the
   instance **Finished**, and that **Response** shows the red badge for 404 with the message as its body. The reply sets a
   content type and writes a body, so the status code pages of the Diagnostics feature (`OrchardCore.Diagnostics/DiagnosticsStartupFilter.cs:29`)
   should not replace it; a reply with no body and no content type would be replaced by the error page.
10. **The stand-in's content type.** `python -m http.server` answers `A-100.json` with a JSON content type on the usual
    installs. The reply of the workflow sets its own content type, so only the body matters.
11. **The designer's draft.** `06-run` assumes there is no draft. Confirm that the clips before it leave none.
12. **Import on another site.** `more-3` says the import generates the URL of each HTTP Request event. The recipe step does
    so for an event whose properties hold `TokenLifeSpan` (`Recipes/WorkflowTypeStep.cs:55-64`), which the editor writes
    when it applies the event's settings. An event that was never edited may have none; opening its editor then
    generates the URL (`Assets/Scripts/workflow-url-generator.ts:37`). Confirm with an export of the Stock check workflow
    imported on a second, empty site.

## Checked in the source, deliberately not narrated

- **Methods.** The HTTP Request event's editor offers GET, POST, PUT, PATCH, DELETE and OPTIONS
  (`Http/ViewModels/HttpRequestEventViewModel.cs:7-14`), but the controller action that receives the call accepts only
  GET, POST, PUT and PATCH (`Http/Controllers/HttpWorkflowController.cs:77`). A request whose method differs from the
  event's does not start the workflow, and still answers 202 (`Http/Activities/HttpRequestEvent.cs:67-74`,
  `Services/WorkflowManager.cs:556-563`). The narration names no list of methods.
- **The HTTP Request task's method hint** lists OPTIONS and HEAD, while its select offers GET, POST, PUT, PATCH and DELETE
  (`Views/Items/HttpRequestTask.Fields.Edit.cshtml:14-24`).
- **Headers of an HTTP Response** are split on every colon, and a header whose value holds a colon is dropped
  (`Http/Activities/HttpResponseTask.cs:104-117`).
- **Liquid output encoding.** The URL and Headers of the HTTP Request task, and the Headers and Content Type of the HTTP
  Response, are URL-encoded when they hold Liquid output. The body of the request and the content of the response are
  evaluated with no encoder (`HttpRequestTask.cs:180-194`, `HttpResponseTask.cs:76-78`), and the Liquid evaluator renders
  with `NullEncoder.Default` when it receives none (`Expressions/LiquidWorkflowExpressionEvaluator.cs:76`), so the JSON
  quotes of the reply are not HTML-encoded.
- **202 Accepted.** `read-3` says the caller of the HTTP Request event's URL receives 202 when no activity writes a
  response (`Http/Controllers/HttpWorkflowController.cs:279-292`). The same holds for a Signal link. The other case is a
  form post whose location was saved by the HTTP Request event (**Form Location Key**), where the caller is redirected
  (`HttpWorkflowController.cs:286-289`). The video does not cover form locations. For a workflow started by Filter
  Incoming HTTP Request, the controller action answers, so the narration does not say 202 there.
- **Signals with a correlation id.** The token of `signalUrl` and `signal_url` targets the instance that made it when
  that instance has no correlation id, and every workflow with that correlation id otherwise
  (`Http/Scripting/SignalMethodProvider.cs:23`, `Http/Liquid/SignalUrlFilter.cs:36-40`, `Http/Controllers/HttpWorkflowController.cs:226-273`).
  The narration says "the workflow that made the link, if it waits on a Signal with the same name", which is exact for a
  workflow without a correlation id (the case of this video). The exact path of the signal and invoke URLs
  (`/workflows/Trigger?token=...` or a path with the token in it) was not established from the routes, so no slide shows one.
- **Filter Incoming HTTP Request.** The route entries treat a blank Controller, Action or Area as "any"
  (`Http/Services/WorkflowRouteEntriesOfT.cs:47-51`), but the activity's own check compares every stored route value
  (`Http/Activities/HttpRequestFilterEvent.cs:42-56,65-85,101-108`): a blank field is stored as null, and `RouteMatches`
  requires the current request to hold that route key with an equal value, so an action without an area may not match
  at all. It is not established that a blank field matches, so the narration says "to match" and no example leaves one
  blank. The editor offers GET, POST, PUT, DELETE and OPTIONS here (`Http/ViewModels/HttpRequestFilterEventViewModel.cs:14`).

## Tracker and specification notes

- **The tracker's docs path.** The entry lists `src/docs/reference/modules/Scripting/README.md` only, which holds the
  script functions. The activities are documented in `src/docs/reference/modules/Workflows/README.md`. `embeddedIn` and
  the README link follow the tracker; `video.json` links both pages. A person who adds the video marker to the docs
  decides whether it also goes in the Workflows page.
- **Spoken spellings to confirm with `scripts/pronounce.py`** before narrating: "signal U.R.L." (for `signalUrl` and
  `signal_url`), "HTTP context", "response write", "is success status code", "reason phrase", "deserialize request
  data", "set outcome", "set property" and "read body"; and the plain words "URL", "JSON", "Liquid" and "webhook". The
  narration also says "Run…" (written with the ellipsis of the label) and "202 Accepted".
