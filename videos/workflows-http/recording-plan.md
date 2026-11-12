# Recording plan: HTTP Workflows

How to record `workflows-http` without deciding anything. Selectors stand for `scene.Page.Locator(...)`; `GetByLabel`
and `GetByRole` are the matching `scene.Page` methods. Orchard Core paths are those of a checkout of
`OrchardCMS/OrchardCore`, at the commit the catalog was read from (`92db4a83`) or later; line numbers are of that commit.
A path that starts with `Views/`, `Services/`, `Http/`, `Assets/`, `Recipes/` or `Activities/` is under
`src/OrchardCore.Modules/OrchardCore.Workflows/`; one that starts with another module name (`OrchardCore.Features/`,
`OrchardCore.Deployment/`) is under `src/OrchardCore.Modules/`; `OrchardCore.Workflows.Abstractions/` is under
`src/OrchardCore/`; the Blog recipe is under `src/OrchardCore.Themes/`.

The video has four recordings (`02-activities`, `03-receive`, `05-call`, `06-run`). Everything else is slides. All four
recordings use one workflow, **Stock check**, which is seeded outside the capture. No clip changes a setting on camera:
the narration only opens and reads settings, so no code editor (Monaco, CodeMirror) is ever typed into.

## The site it needs

- **Host**: `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at
  an empty folder outside both repositories.
- **Auto Setup**: recipe Blog, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email
  `admin@example.com`, password chosen outside git and passed in `VIDEO_PASSWORD` (write `<password>` anywhere it has
  to be named). Default tenant, no URL prefix. No setup or sign-in form is recorded; the recording signs in once with
  `Recorder setup`, which writes the git-ignored `state.json`.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` asks, **and**
  `NO_PROXY=localhost,127.0.0.1`, so the HTTP Request task can reach the local stand-in below and nothing else.
- **Features to enable** (Tools > Features, search `Workflows`): **Workflows**, then **HTTP Workflows Activities**
  (`OrchardCore.Workflows.Http`, which depends on Workflows). Use each row's **Enable** button, or select both rows and
  choose **Bulk Actions** > **Enable**. The **Deployment** feature, needed for the import below, is enabled by the Blog
  recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Menu path: `OrchardCore.Features/AdminMenu.cs:26-27`.
- **Settings**: none.
- **Fake data**: the only data is the product code `A-100` ("Garden trowel") and the unknown code `Z-999`.

### External service and its local stand-in

The workflow calls a stock API. The stand-in is the Python standard library's file server, run from a throwaway folder
**outside both repositories** (for example `%TEMP%/stock-api`), bound to the loopback address:

1. Create `A-100.json` in that folder, with this content and nothing else:

   ```
   { "sku": "A-100", "name": "Garden trowel", "inStock": 12 }
   ```

2. From that folder, run `python -m http.server 5080 --bind 127.0.0.1` and leave it running while recording.

`http://localhost:5080/A-100.json` answers 200 with that JSON (content type `application/json`). Any other file name,
such as `Z-999.json`, answers 404. Stop the server and delete the folder afterwards.

### Seed data (outside the capture)

Import the workflow **Stock check** with Tools > Deployments > **JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`):
paste the JSON below into the field labeled **JSON** (`OrchardCore.Deployment/Views/Import/Json.cshtml:15`), and click
**Import**. The page turns that text area into a CodeMirror editor (`OrchardCore.Deployment/Assets/ts/import-json.ts:3`),
so do not type the JSON: set it with `document.querySelector('.CodeMirror').CodeMirror.setValue(json)` from
`scene.Page.EvaluateAsync`, then click **Import**. The page answers "Recipe imported successfully!"
(`OrchardCore.Deployment/Controllers/ImportController.cs:185`).

`TokenLifeSpan` is present on the HTTP Request event on purpose: the `WorkflowType` recipe step generates the event's
URL on import only when that property exists (`OrchardCore.Workflows/Recipes/WorkflowTypeStep.cs:55-64`). The activities
have no title of their own, so the canvas shows their default titles. The reply of the 200 branch reads the response
body from the variable `stockBody`, which the HTTP Request task fills through its `OutputBindings` (the Body output),
the way the Reply task of the samples recipe reads its variable
(`OrchardCore.Workflows/Recipes/workflows-variables-samples.recipe.json`).

```json
{
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "stockcheck",
          "Name": "Stock check",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            {
              "Name": "stockBody",
              "TypeName": "string",
              "Description": "The body of the response of the stock API, bound to the Body output of the HTTP Request task."
            }
          ],
          "Activities": [
            {
              "ActivityId": "stockrequested",
              "Name": "HttpRequestEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": {
                "HttpMethod": "POST",
                "ValidateAntiforgeryToken": false,
                "TokenLifeSpan": 0
              }
            },
            {
              "ActivityId": "readproduct",
              "Name": "ScriptTask",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "AvailableOutcomes": [
                  "Done"
                ],
                "Script": {
                  "Expression": "// The body of the request, parsed as JSON.\nconst request = JSON.parse(readBody());\n\n// Keep the product code for the activities that follow.\nsetProperty('sku', request.sku);\nsetOutcome('Done');"
                }
              }
            },
            {
              "ActivityId": "callstockapi",
              "Name": "HttpRequestTask",
              "X": 720,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "Url": {
                  "Expression": "http://localhost:5080/{{ Workflow.Properties.sku }}.json"
                },
                "HttpMethod": "GET",
                "HttpResponseCodes": "200, 404",
                "OutputBindings": {
                  "Body": "stockBody"
                }
              }
            },
            {
              "ActivityId": "replyfound",
              "Name": "HttpResponseTask",
              "X": 550,
              "Y": 330,
              "IsStart": false,
              "Properties": {
                "HttpStatusCode": 200,
                "ContentType": {
                  "Expression": "application/json"
                },
                "Content": {
                  "Expression": "{{ Workflow.Variables.stockBody }}"
                }
              }
            },
            {
              "ActivityId": "replynotfound",
              "Name": "HttpResponseTask",
              "X": 890,
              "Y": 330,
              "IsStart": false,
              "Properties": {
                "HttpStatusCode": 404,
                "ContentType": {
                  "Expression": "text/plain"
                },
                "Content": {
                  "Expression": "No product has this product code."
                }
              }
            }
          ],
          "Transitions": [
            {
              "SourceActivityId": "stockrequested",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "readproduct"
            },
            {
              "SourceActivityId": "readproduct",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "callstockapi"
            },
            {
              "SourceActivityId": "callstockapi",
              "SourceOutcomeName": "200",
              "DestinationActivityId": "replyfound"
            },
            {
              "SourceActivityId": "callstockapi",
              "SourceOutcomeName": "404",
              "DestinationActivityId": "replynotfound"
            }
          ]
        }
      ]
    }
  ]
}
```

After the import, open Design > Workflows (`OrchardCore.Workflows/AdminMenu.cs:18-19`): the list holds **Stock check**,
enabled. Open it once, check that five activities are on the canvas (a **Start** badge on the first), and leave the
page without changing anything. Check the stock API from a browser tab: `http://localhost:5080/A-100.json` shows the
JSON above.

## The state to save

After the seed, stop the site, copy its `App_Data` folder and `state.json` aside, and start the site again. To record
again, stop the site, restore the copy, and start it again.

## The order of the clips, and what they change

| Clip | Depends on | Changes the site |
|---|---|---|
| `02-activities` | the seed | no |
| `03-receive` | the seed | no, as long as no field is edited. If the event's URL were empty, the editor would generate one and leave a draft (`Assets/Scripts/workflow-url-generator.ts:37`); the seed avoids that. |
| `05-call` | the seed | no, same condition |
| `06-run` | the seed, the stock API running | **yes**: each run adds a workflow instance |

Record in the order `02-activities`, `03-receive`, `05-call`, `06-run`. Before `06-run`, check the toolbar: **Discard
draft** must be disabled (no draft). If it is enabled, restore the saved copy. The clips do not depend on each other, so
any one can be recorded alone from the saved copy.

## Common start of the designer clips (not recorded)

Every designer clip starts like this, before `StartCaptureAsync()`:

1. `scene.OpenAsync("/Admin/Workflows/Types")`, then click the link **Stock check** (its name is the link text of the
   row, `Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`) with a navigation wait.
2. Wait for `[data-cy='designer-toolbar']` and for five `.wfd-node` elements.
3. Clip `02-activities` keeps the activities pane open. The other designer clips hide it first, to give the canvas the
   width: click `[data-cy='toolbox-collapse']` (label **Hide the activities**,
   `Services/WorkflowsDesignerJSLocalizer.cs:66`).
4. The activity panel is closed. Nodes are found by their activity ids, `[data-cy='activity-stockrequested']`,
   `[data-cy='activity-readproduct']`, `[data-cy='activity-callstockapi']`, `[data-cy='activity-replyfound']` and
   `[data-cy='activity-replynotfound']` (`Assets/designer/src/canvas/ActivityNode.vue`, `data-cy` of the node).

To open an activity's settings, double-click its node, or click the settings button at its top right
(`[data-cy='node-edit']`, label **Settings (or double-click the activity)**). The panel opens over the bottom of the
canvas. To close it, click `[data-cy='activity-panel-close']` (label **Close the activity panel**,
`WorkflowsDesignerJSLocalizer.cs:151`). Never click inside a field: a field is applied when it loses focus, which would
leave a draft. Scroll the panel with `scene.ScrollAsync` when a field is below the visible part.

## Clip `02-activities`

**Starts on**: the designer of Stock check, activities pane open (heading **Activities**, `WorkflowsDesignerJSLocalizer.cs:63`),
search box empty, **All** selected.

- `acts-1`: click the search box `[data-cy='toolbox-search']` (placeholder **Search activities**,
  `WorkflowsDesignerJSLocalizer.cs:46`) and type `HTTP` (`scene.TypeAsync`). The list filters by display text and
  category name. Highlight `[data-cy='toolbox-category-HTTP']`. **On screen at the end of the narration**: one
  category, **HTTP**, open, with a count badge of 6 and six cards, in this order (the toolbox sorts by the activity's
  display text, `Services/WorkflowDesignerModelBuilder.cs:153`: Http Redirect Task, Http Request Event, Http Request
  Filter Event, Http Request Task, Http Response Task, Signal Event): HTTP Redirect, HTTP Request (event), Filter
  Incoming HTTP Request, HTTP Request (task), HTTP Response, Signal. The count is 6 because the Forms feature is not
  enabled: its HTTP Redirect To Form Location task would join the category
  (`OrchardCore.Forms/Activities/HttpRedirectToFormLocationTask.cs:30`). Labels: category
  `OrchardCore.Workflows.Abstractions/Activities/ActivityCategoryLocalizationSources.cs:26`; cards
  `Views/Items/*.Fields.Thumbnail.cshtml:1` of `HttpRequestEvent`, `HttpRequestFilterEvent`, `HttpRedirectTask`,
  `HttpRequestTask`, `HttpResponseTask` and `SignalEvent`.
- `acts-2`: clear highlights; click the label **Events** (`scene.Page.Locator("label[for='wfd-kind-events']")`;
  `WorkflowsDesignerJSLocalizer.cs:49`). **On screen**: three cards, HTTP Request, Filter Incoming HTTP Request and
  Signal. Highlight the three cards in turn: `[data-cy='toolbox-activity-HttpRequestEvent']`,
  `[data-cy='toolbox-activity-HttpRequestFilterEvent']`, `[data-cy='toolbox-activity-SignalEvent']`
  (`Assets/designer/src/toolbox/ActivityToolbox.vue:102`; the key of a card is the name of the activity).
- `acts-3`: click the label **Tasks** (`label[for='wfd-kind-tasks']`; `WorkflowsDesignerJSLocalizer.cs:50`). **On
  screen**: three cards, HTTP Redirect, HTTP Request and HTTP Response. Highlight them in turn: `[data-cy='toolbox-activity-HttpRedirectTask']`,
  `[data-cy='toolbox-activity-HttpRequestTask']`, `[data-cy='toolbox-activity-HttpResponseTask']`.

## Clip `03-receive`

**Starts on**: the designer of Stock check, activities pane hidden, activity panel closed, the five activities visible
without scrolling (if not, click the canvas control **Fit to content**, `GetByRole(AriaRole.Button, new() { Name = "Fit to
content" })`, `Services/WorkflowsDesignerJSLocalizer.cs:358`, before the capture).

- `recv-1`: highlight, one after the other and about every three seconds, `[data-cy='activity-stockrequested']`,
  `[data-cy='activity-readproduct']`, `[data-cy='activity-callstockapi']`, then both `[data-cy='activity-replyfound']`
  and `[data-cy='activity-replynotfound']`; clear the highlights at the end. **On screen**: the whole workflow, the
  first activity with the **Start** badge (`WorkflowsDesignerJSLocalizer.cs:329`), the node headers "Http Request
  Event", "Script Task", "Http Request Task" and "Http Response Task" (the header is the activity's display text:
  `Http/Activities/HttpRequestEvent.cs:30`, `Http/Activities/HttpRequestTask.cs:109`,
  `Http/Activities/HttpResponseTask.cs:39`, `Activities/ScriptTask.cs:23`), and the body of the first node reading
  "Handle Incoming HTTP Request" with the method `POST` (`Views/Items/HttpRequestEvent.Fields.Design.cshtml:4`).
- `recv-2`: double-click `[data-cy='activity-stockrequested']`. Wait until the URL field `[data-workflow-url-text]`
  holds a value. Highlight, in this order, the select `GetByLabel("HTTP Method")` (value POST), the URL group
  (`[data-workflow-url-text]` with its base address in front), the **Regenerate** button `#generate-url-button`, and the
  warning `GetByText("Anyone that invokes this URL will trigger this workflow. Keep it in a safe place.")`. **Do not click Regenerate or Copy.** **On screen**: the panel with **HTTP Method** POST, **URL**
  filled with a long generated address, the buttons **Regenerate** and **Copy**, and the hint "Anyone that invokes
  this URL will trigger this workflow. Keep it in a safe place." Labels:
  `Views/Items/HttpRequestEvent.Fields.Edit.cshtml:8,17,25,26,28`.
- `recv-3`: scroll the panel down with `scene.ScrollAsync`, and highlight `GetByLabel("Validate Antiforgery Token")`
  (unchecked), then `GetByLabel("Token Life Span")` (value 0), then the hint `GetByText("Number of days to expire the token. 0 for the token to never expire")`. **On screen**: **Validate Antiforgery Token** unchecked with its hint about webhooks,
  **Token Life Span** 0 with its hints. Labels: `Views/Items/HttpRequestEvent.Fields.Edit.cshtml:36,37,43,46,47`. At the
  end, close the panel with `[data-cy='activity-panel-close']`.

## Clip `05-call`

**Starts on**: the designer of Stock check, activities pane hidden, activity panel closed.

- `call-1`: double-click `[data-cy='activity-callstockapi']`. Highlight, in turn, `GetByLabel("URL", new() { Exact = true })`
  (it reads `http://localhost:5080/`, a Liquid expression, then `.json`), `GetByLabel("HTTP Method")` (GET),
  `GetByLabel("Content Type")`, then scroll and highlight the **Body** and **Headers** fields (both empty). **On screen**:
  the settings of the HTTP Request task, with **URL**, **HTTP Method**, **Content Type**, **Body** and **Headers**
  visible at some point of the narration, and **URL** with its Liquid expression visible at the end of the first
  sentence. Labels: `Views/Items/HttpRequestTask.Fields.Edit.cshtml:5,9,14,29,38,45,49,54,58`.
- `call-2`: scroll to `GetByLabel("HTTP Response Codes")` (value `200, 404`) and highlight it with its hint
  `GetByText("A comma-separated list of HTTP response status codes to handle as activity outcomes.")`. Then close the panel
  (`[data-cy='activity-panel-close']`) and highlight the three ports of the node:
  `[data-cy='activity-callstockapi'] [data-outcome='200']`, `[data-outcome='404']` and
  `[data-outcome='UnhandledHttpStatus']`. **On screen at the end**: the HTTP Request node with the ports **200 OK**
  and **404 Not Found** (each connected to an HTTP Response) and **Unhandled Http Status** (not connected). Labels:
  `Views/Items/HttpRequestTask.Fields.Edit.cshtml:64-68`; port names `Http/Activities/HttpRequestTask.cs:153-171`
  (the status texts are the dictionary at the top of that file).
- `call-3`: double-click `[data-cy='activity-callstockapi']` again, then click `[data-cy='activity-tab-outputs']` (tab
  **Outputs**, `WorkflowsDesignerJSLocalizer.cs:116`). Highlight `[data-cy='output-Body']`, then
  `[data-cy='output-StatusCode']` and `[data-cy='output-Response']`. **On screen**: the **Outputs** tab with three
  outputs, "Response body", "Status code" and "Response" (`Http/Activities/HttpRequestTask.cs:247-249`). Each has a
  **Store in** list (`WorkflowsDesignerJSLocalizer.cs:121`, the list is `[data-cy='output-binding']`); the first reads
  `stockBody`, the two others "Don't store" (`:122`). Do not open the lists. Close the panel at the end.
- `call-4`: double-click `[data-cy='activity-replyfound']`; highlight `GetByLabel("HTTP Status Code")` (200),
  `GetByLabel("Content Type")` (application/json) and the **Content** field (`{{ Workflow.Variables.stockBody }}`), then
  scroll and highlight **Headers** (empty). Close the panel, then double-click `[data-cy='activity-replynotfound']` and
  highlight **HTTP Status Code** (404), **Content Type** (text/plain) and **Content** ("No product has this product
  code."). **On screen at the end**: the settings of the second HTTP Response, 404. Labels:
  `Views/Items/HttpResponseTask.Fields.Edit.cshtml:5,14,31,40`. Keep the actions shorter than the narration: if they
  run long, drop the highlight of **Headers**.

## Clip `06-run`

**Starts on**: the designer of Stock check, activities pane hidden, activity panel closed, **Discard draft** disabled.
The stock API must be running on port 5080.

- `run-1`: click `[data-cy='toolbar-run']` (button **Run…**, `WorkflowsDesignerJSLocalizer.cs:286`) and wait for
  `[data-cy='run-dialog']` (title **Run the workflow**, line 288). The dialog says "Sends a POST request to the
  workflow, as a client would." (line 295), lists **Query string parameters** with "The workflow doesn't read any
  parameter by name." (lines 296-297), and offers a **Body** with the choices **Form fields** and **JSON or text** (lines
  304 and 305); **JSON or text** is selected and its **Content type** is `application/json`. Click the body text area
  `[data-cy='run-body']` and type `{ "sku": "A-100" }` with `scene.TypeAsync`. **On screen at the end**: the dialog
  with the JSON typed in the body. The text area is a plain text area, not a code editor.
- `run-2`: click `[data-cy='run-start']` (button **Run**, line 282). Wait until `[data-cy='run-response-status']`
  reads `200` and `[data-cy='run-status']` reads `Finished`. **On screen at the end**: the **Result** section (line
  308) with **Response** 200 in a green badge, the body `{ "sku": "A-100", "name": "Garden trowel", "inStock": 12 }`,
  **Instance** `Finished`, and the button **Open the instance** (line 311). Do not click it. Highlight
  `[data-cy='run-response-status']`, then `[data-cy='run-open-instance']`.
- `run-3`: the first action of the step empties the body: `FillAsync("")` on `[data-cy='run-body']`. Then type
  `{ "sku": "Z-999" }` with `scene.TypeAsync` and click `[data-cy='run-start']` again. Wait until
  `[data-cy='run-response-status']` reads `404`. **On screen at the end**: **Response** 404 in a red badge with the body
  "No product has this product code.", and **Instance** `Finished`. The actions must end before the narration does; if
  they run long, shorten the typing delay.

## Slides (no recording)

- `why-1`: the four cards of "What the HTTP activities do", the first two bright (HTTP Request event, Filter Incoming HTTP Request).
- `why-2`: the same four cards, the last two bright (HTTP Response and HTTP Redirect, HTTP Request task).
- `read-1`: the script of the Stock check workflow in a code panel (read the body, set the property, set the outcome).
- `read-2`: the four other functions in a code panel: `queryString`, `requestForm`, `deserializeRequestData`, `httpContext`.
- `read-3`: `responseWrite` in a code panel, with three cards on the ways to answer and what happens when none runs.
- `signal-1`: the Liquid content with the signal URL filter and the script with `signalUrl`, with two cards (Signal event, Name: Approved).
- `signal-2`: the same code, with three cards on the link: protected token, a path on the site, the same name.
- `more-1`: four cards for the fields of Filter Incoming HTTP Request: HTTP Method, Controller, Action, Area.
- `more-2`: two cards for the fields of HTTP Redirect: Location, Permanent.
- `more-3`: three cards for moving a workflow: Export, Import, URLs.

The opening card (`intro`) and the closing card (`outro`) are drawn by the assembler.

## Checks for private data

- Every address on screen is `localhost`, `example.com` or the site's own generated URL; no real name, email address or
  token of a real service appears. The only email is `admin@example.com`, and these clips do not show it.
- The generated event URL of clip `03-receive` holds a token protected by the throwaway site's Data Protection keys.
  Delete the site's `App_Data` after recording, so the token opens nothing.
- The password is never typed on camera: the clips start signed in through `state.json`.
- OCR the video at one frame per second and at every scene change, as the skill's step 6 asks, and search for the
  token text, `@`, and any name other than the product `Garden trowel`.
