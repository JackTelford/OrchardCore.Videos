# Recording plan: Forms and Workflows: Process Each Submission

A developer video: slides with code from the Orchard Core source, and five browser recordings. Read this file top to
bottom; nothing else is needed to record it. Source paths are relative to the root of `OrchardCMS/OrchardCore`
(`src/OrchardCore.Modules/...`); a selector below stands for `scene.Page.Locator(...)`. Open questions about what the
source could not settle are in `OPEN-QUESTIONS.md`: read them before the first take.

The video has 28 narrated steps: 2 cards (`intro`, `outro`), 8 slide steps, and 18 steps of 5 recorded clips.

| Clip | Kind | Chapter | Steps |
|---|---|---|---|
| `00-intro` | card | opening | `intro` |
| `01-how` | slide | From a form to a workflow | `how-1` to `how-4` |
| `02-start` | clip | Start the workflow | `start-1` to `start-4` |
| `03-validate` | clip | Validate the fields | `validate-1` to `validate-5` |
| `04-values` | slide | Handle a valid submission | `values-1` |
| `04-valid` | clip | Handle a valid submission | `valid-1` to `valid-3` |
| `05-connect` | clip | Connect the form | `connect-1` to `connect-3` |
| `06-try` | clip | Try it | `try-1` to `try-3` |
| `07-state` | slide | Behind the redirect | `state-1` to `state-3` |
| `99-outro` | card | closing | `outro` |

## The site it needs

A fresh throwaway site, never a copy of a real one, so no mask script is needed.

- **Provisioning.** Run `src/OrchardCore.Cms.Web` on `http://localhost:5010` with its own `ORCHARD_APP_DATA` folder
  outside both repositories, provisioned by Auto Setup through environment variables (no setup screen, no sign-in
  form is recorded): `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
  `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
  `...__AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `...__DatabaseProvider=Sqlite`,
  `...__RecipeName=Blog`. The Blog recipe creates the `Page` content type, which has a title, a permalink (Autoroute)
  and the Flow part that holds widgets, and it enables the Flows and Widgets features.
- **Outbound calls.** Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site calls no provider. Mail goes
  to the local SMTP catcher below, which a proxy setting does not affect.
- **Sign-in.** `Recorder setup` signs in as `admin` with `<password>` and saves `state.json` (git-ignored).
- **Features to enable before recording** (outside the capture, in the admin under Tools, Features; search for each
  name): `Forms`, `Workflows`, `HTTP Workflows Activities`, `Email`, `SMTP Email Provider`. Forms needs the Widgets
  and Flows features, which the Blog recipe already enabled. Names: `OrchardCore.Forms/Manifest.cs:12`,
  `OrchardCore.Workflows/Manifest.cs:14,22`, `OrchardCore.Email/Manifest.cs:4`, `OrchardCore.Email.Smtp/Manifest.cs:4`.
- **External service and its stand-in.** The Email Task needs a working email provider, or it takes its Failed
  outcome and the visitor is not redirected to the thank-you page. Run a local SMTP catcher, such as smtp4dev in
  Docker (`docker run --rm -d --name smtp4dev -p 3000:80 -p 2525:25 rnwood/smtp4dev`), and remove the container
  after the last take. It is never shown on screen.
- **Email settings** (outside the capture, Settings, Communication, Email): check `Enable SMTP Provider`; `Sender email
  address` = `noreply@example.com`; `Delivery method` = `Network`; `Host name` = `localhost`; `Port number` = `2525`;
  uncheck `Auto Select Encryption method` and choose the first option of `Encryption method to use` (its text is
  `None - Connect to server using insecure connection.`); leave `Require credentials` unchecked. When the page has a `Default provider` list, choose the SMTP provider. Save. Labels:
  `OrchardCore.Email/AdminMenu.cs:38-40`, `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10-60`,
  `OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`.

### The seed data

Create it in the admin, outside the capture, and publish it. Fake data only.

1. **Page `Thank You`** (Content, Content Items, then create a `Page`). Title `Thank You`; the permalink the page
   proposes is `thank-you` (the Blog recipe's pattern is the slugified title: `blog.recipe.json` in
   `OrchardCore.Themes/TheBlogTheme/Recipes`, content type `Page`). In its Flow part, `Add Widget` (the `+` button),
   choose `Paragraph`, and type `Thank you for reaching out. We will get back to you soon.` Publish.
2. **Page `Contact`.** Title `Contact`; the permalink is `contact`. In its Flow part, `Add Widget`, choose `Form`
   (`OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`). The names in the list come from the type names with a space
   before each capital (`ContentDefinitionManagerExtensions.cs:12`, `StringExtensions.cs:30`), so the widgets are
   `Form`, `Validation Summary`, `Input`, `Text Area` and `Button`. Settings of the Form widget:
   - Title `Contact`. `Action` stays **empty** (the `05-connect` clip fills it). `Method` = `POST`; `EncType` stays at
     its first value; `Anti Forgery Token` and `Save Form Location` stay checked
     (`OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml:6-50`).
   - Inside the Form widget, in its own Flow part, add these widgets in this order with `Add Widget`:
     1. `Validation Summary`, nothing to change.
     2. `Input`: `HTML Name` = `Name`; `Label option` = `Standard`; `Label Text` = `Name`; `Type` = `Text`;
        `Placeholder Text` = `Enter your name`; `Validation option` = `None`.
     3. `Input`: `HTML Name` = `Email`; `Label option` = `Standard`; `Label Text` = `Email`; `Type` = `Email`;
        `Placeholder Text` = `Enter your email address`; `Validation option` = `None`.
     4. `Text Area`: `HTML Name` = `Message`; `Label option` = `Standard`; `Label Text` = `Message`;
        `Placeholder Text` = `Enter your message`; `Rows` = `4`; `Validation option` = `None`.
     5. `Button`: `HTML Name` = `Send`; `Text` = `Submit`; `Type` = `Submit`.
   - `HTML Name` is required on every input-like widget, the button included
     (`OrchardCore.Forms/Drivers/FormInputElementPartDisplayDriver.cs:35-45`). Labels:
     `OrchardCore.Forms/Views/Items/InputPart.Fields.Edit.cshtml`, `TextAreaPart.Fields.Edit.cshtml`,
     `ButtonPart.Fields.Edit.cshtml`, `FormElementLabelPart.Fields.Edit.cshtml`,
     `FormElementValidationPart.Fields.Edit.cshtml`.
   - Publish the page. Check `http://localhost:5010/contact` shows the form (a Name field, an Email field, a Message
     box and a Submit button) and no errors.
3. The site has no workflow yet. The video creates the only one.

Snapshot **S1** now: stop the site, copy its `ORCHARD_APP_DATA` folder aside with `state.json`, start the site again.

## Order of the clips and the state they leave

The clips share one site and run in this order, each one starting from what the previous one left. This is also the
order of the video.

| Order | Clip | Needs | Changes the site | Snapshot to take afterwards |
|---|---|---|---|---|
| 1 | `02-start` | S1 | Creates the workflow `Contact Form Workflow` with its Http Request Event, still a draft | S2 |
| 2 | `03-validate` | S2 | Adds four activities to the draft | S3 |
| 3 | `04-valid` | S3 | Adds two activities and **publishes** the workflow | S4 |
| 4 | `05-connect` | S4 and the workflow URL | Saves the Action on the Contact page and publishes it | S5 |
| 5 | `06-try` | S5 | Creates two workflow instances; sends one email | none |

To record one clip again, stop the site, restore the snapshot it needs (the folder and `state.json`), start the site,
and record it. To record everything again, restore S1.

**Saving before a snapshot.** The designer saves the draft by itself after every change. Before a clip that changes
the workflow ends, and before the site is stopped for a snapshot, wait until the toolbar says `All changes saved`
(`WorkflowsDesignerJSLocalizer.cs:166`). The draft is the only state of `02-start` and `03-validate`: a snapshot taken
while the toolbar says `Saving…` or `Unsaved changes` loses the last change.

**The workflow URL.** `02-start` shows it in step `start-3`; `05-connect` pastes it. The recorder reads it in
`start-3` with `InputValueAsync` on `[data-workflow-url-text]` and writes it to a scratch file outside the repository
(`%TEMP%/ocvideo/forms-workflows/workflow-url.txt`), which `05-connect` reads. When the file is missing (a snapshot
was restored), open the designer of the workflow outside the capture, double-click the `Http Request Event` activity,
and read the same box. The URL belongs to the throwaway site: it never goes into git.

**Closing the activity panel.** Adding an activity that has settings opens the activity panel over the bottom of the
canvas, which can cover the ports of the activities. Before using a port, close the panel with its `Close the
activity panel` button (`WorkflowsDesignerJSLocalizer.cs:151`), unless a step says otherwise. Closing the panel applies
the changes typed in it (`Assets/designer/src/panel/ActivityPanel.vue`, `close`), and so does selecting another
activity and `Publish` (`App.vue`, `publish`); press Tab after every typed value as well, since the source of the form
host that decides when a value is applied is not in the tree (see `OPEN-QUESTIONS.md`).

**Adding an activity to a port.** Every activity after the first is added the same way. Click the port of the
outcome (`[data-cy^="port-"][data-outcome="Done"]` inside the activity's `.wfd-node`), type the search in
`[data-cy="quick-add-search"]`, and click the result `[data-cy="quick-add-<Name>"]` (the activity's type name, given
in each step). The list shows the full activity name, such as `Validate Form Field Task`, and its category
(`WorkflowsDesignerJSLocalizer.cs:333`, `Assets/designer/src/canvas/QuickAddMenu.vue`). Click the result, not Enter:
the first match of a search is not always the activity wanted. The new activity is placed to the right of its
source and is already connected.

## Clip `02-start`

Starts on `/Admin` (the dashboard). Narration of the chapter: 25, 33, 33 and 29 words. Keep every step's actions
shorter than its narration.

### `start-1`

1. Click `Design` in the admin menu, then `Workflows` (it opens `/Admin/Workflows/Types`, which says there are no
   workflow types for the moment). Use `ClickAndWaitForNavigationAsync`.
2. Click `Create Workflow` (`btn` link, `Index.cshtml:25`), waiting for the navigation.
3. Type `Contact Form Workflow` into the `Name` box (`TypeAsync`, delay 25).
4. Click `Create and open the designer` (`Create.cshtml:131`), waiting for the navigation.

On screen when the narration ends: the designer of `Contact Form Workflow`, an empty canvas, with the heading `Start
with an event`. Labels: `OrchardCore.Workflows/AdminMenu.cs:18-19`, `Views/WorkflowType/Index.cshtml:25`,
`Views/WorkflowType/Create.cshtml:21,131`, `WorkflowsDesignerJSLocalizer.cs:334`.

### `start-2`

Click `[data-cy="start-picker-HttpRequestEvent"]`, whose text is `Http Request Event`
(`OrchardCore.Workflows/Http/Activities/HttpRequestEvent.cs:30`, `Assets/designer/src/canvas/StartPicker.vue`).
Wait for `[data-cy="activity-panel"]` to be visible.

On screen: one activity on the canvas, headed `Http Request Event`, with the green `Start` badge
(`WorkflowsDesignerJSLocalizer.cs:329`; the first event added is the start activity,
`Services/WorkflowTypeDraftManager.cs:175`), and the activity panel open at the bottom showing the settings.

### `start-3`

1. In the panel, choose `POST` in the `HTTP Method` list (`SelectAsync`), then press Tab.
2. Wait until `[data-workflow-url-text]` has a non-empty value (`WaitForFunctionAsync` on its `value`): the page
   fills it by itself when the editor opens (`Assets/Scripts/workflow-url-generator.ts`).
3. Highlight the `URL` box and the warning under it (`HighlightAsync`, no caption).
4. Read the URL and save it to the scratch file (see above). Do not read it aloud or narrate its shape.

On screen: `HTTP Method` = `POST`, the `URL` box filled, the warning `Anyone that invokes this URL will trigger this
workflow. Keep it in a safe place.` Labels: `Views/Items/HttpRequestEvent.Fields.Edit.cshtml:8,17,28`.

### `start-4`

Scroll the panel body down (`ScrollAsync`) and highlight, one after the other, the checked `Validate Antiforgery
Token` box and the empty `Form Location Key` box. Do not change either. Clear the highlights.

On screen: the checked `Validate Antiforgery Token` box and the empty `Form Location Key` box. Labels:
`HttpRequestEvent.Fields.Edit.cshtml:36,52`. Then close the activity panel and let the canvas settle (outside the
narration's last second).

## Clip `03-validate`

Starts on the designer of `Contact Form Workflow`, opened outside the capture at `/Admin/Workflows/Types/Edit/<id>`
(the workflow's id is in the address bar after `02-start`; the list at `/Admin/Workflows/Types` links to it). The
canvas shows the `Http Request Event` activity, with its panel closed.

### `validate-1`

1. Click the `Done` port of the `Http Request Event` activity. A menu titled `Add the next activity, after Done`
   opens.
2. Type `bind` in the search box, then click `[data-cy="quick-add-BindModelStateTask"]` (`Bind Model State Task`,
   category `Validation`).

On screen: two connected activities; the second one headed `Bind Model State Task`, with its `Done` port. The panel
that opens has only a `Title` box and the retry settings: leave them. Labels:
`OrchardCore.Forms/Workflows/Activities/BindModelStateTask.cs:31`, `WorkflowsDesignerJSLocalizer.cs:333`.

### `validate-2`

1. Close the panel. Click the `Done` port of `Bind Model State Task`.
2. Type `validate form field`, then click `[data-cy="quick-add-ValidateFormFieldTask"]`
   (`Validate Form Field Task`).
3. In the panel, `TypeAsync` into `Field Name`: `Name`; press Tab. Into `Error Message`: `Name is required`; press Tab.

On screen: the panel with `Field Name` = `Name` and `Error Message` = `Name is required`, and the new activity on the
canvas showing `Validate Name` and its three ports `Done`, `Valid` and `Invalid`. An activity left with an empty field
shows an error badge and blocks Publish, so fill both boxes. Labels: `ValidateFormFieldTask.cs:32`,
`Views/Items/ValidateFormFieldTask.Fields.Edit.cshtml:5,14`.

### `validate-3`

1. Close the panel. Click the `Done` port of the first `Validate Form Field Task` (not `Valid` or `Invalid`).
2. Search `validate form field`, click `[data-cy="quick-add-ValidateFormFieldTask"]`.
3. `Field Name` = `Email`, Tab, `Error Message` = `Email is required`, Tab.

On screen: two `Validate Form Field Task` activities in a row, the second showing `Validate Email`, the panel with
its two values. If the chain no longer fits, use the canvas control `Fit to content` once the step's narration has
ended (`WorkflowsDesignerJSLocalizer.cs`, key `FitToContent`).

### `validate-4`

1. Close the panel. Click the `Done` port of the second `Validate Form Field Task`.
2. Search `validate form task`, then click `[data-cy="quick-add-ValidateFormTask"]` (`Validate Form Task`). The list
   also shows `Validate Form Field Task` first, so do not press Enter.

On screen: a `Validate Form Task` activity with the ports `Valid` and `Invalid`; no panel opens, since it has no
settings (`ValidateFormTask.cs:27,35`).

### `validate-5`

1. Click the `Invalid` port of `Validate Form Task`.
2. Search `form location`, click `[data-cy="quick-add-HttpRedirectToFormLocationTask"]`
   (`Http Redirect To Form Location Task`, category `HTTP`). Leave `Form Location Key` empty.

On screen: the whole chain: Http Request Event, Bind Model State Task, two Validate Form Field Tasks, Validate Form
Task, and `Http Redirect To Form Location Task` connected from `Invalid`; the `Valid` port unconnected. Use `Fit to
content` if needed. Labels: `OrchardCore.Forms/Activities/HttpRedirectToFormLocationTask.cs:28`,
`Views/Items/HttpRedirectToFormLocationTask.Fields.Edit.cshtml:6`.

## Clip `04-valid`

Starts on the designer with the chain of `03-validate`, panel closed.

### `valid-1`

1. Click the `Valid` port of `Validate Form Task`.
2. Search `email`, click `[data-cy="quick-add-EmailTask"]` (`Email Task`, category `Messaging`).
3. In the panel: `Recipients` = `owner@example.com`, Tab. `Subject` = `Contact form submitted by {{ Request.Form.Name
   }}` typed with `TypeAsync` (the text has two braces on each side; type it as one string), Tab.
4. In the `HTML Body` editor, type `<p>Message: {{ Request.Form.Message }}</p>`. If the editor closes the braces by
   itself and doubles them, set the text through the editor's own API instead (see `OPEN-QUESTIONS.md`).

On screen: the panel with `Recipients`, `Subject` and `HTML Body` filled, and the `Email Task` on the canvas showing
the subject. Leave `From`, `Sender` and `Format` as they are. Labels: `OrchardCore.Email/Workflows/Activities/
EmailTask.cs:35`, `OrchardCore.Email/Views/Items/EmailTask.Fields.Edit.cshtml:34,61,90`.

### `valid-2`

1. Close the panel. Click the `Done` port of `Email Task`.
2. Search `http redirect`, then click `[data-cy="quick-add-HttpRedirectTask"]` (`Http Redirect Task`). Do not press
   Enter: `Http Redirect To Form Location Task` also matches.
3. `Location` = `/thank-you`, Tab. Leave `Permanent` unchecked.
4. Highlight the `Failed` port of `Email Task`, which has no connection, with `HighlightAsync`; clear it at the end.

On screen: the `Http Redirect Task` showing `/thank-you`, connected from `Done`, and the unconnected `Failed` port of
`Email Task` highlighted. Labels: `OrchardCore.Workflows/Http/Activities/HttpRedirectTask.cs:32`,
`Views/Items/HttpRedirectTask.Fields.Edit.cshtml:5,17`.

### `valid-3`

1. Close the panel. Click `[data-cy="toolbar-publish"]` (`Publish`, `WorkflowsDesignerJSLocalizer.cs:190`).
2. A dialog `Publish the workflow?` appears only when the draft has warnings; there should be none. If it appears,
   find the cause (an activity that can't be reached from the start) and fix the draft, rather than publishing
   anyway.

On screen: the toolbar after publishing, with the published version badge, the confirmation toast `The workflow was
published as version 1.` (`PublishedVersion`, `WorkflowsDesignerJSLocalizer.cs:221`; the shorter `The workflow was
published.` of `:197` shows when the server sends no version, `App.vue`, `doPublish`) and the whole workflow on the
canvas.

## Clip `05-connect`

Starts on `/Admin/Contents/ContentItems`, which lists the pages `Contact` and `Thank You`.

### `connect-1`

1. Click the `Contact` page title (a link), waiting for the navigation to its editor.
2. Scroll to the Form widget until its `Action` box is in view (`ScrollAsync`), then highlight the empty `Action` box.
   The card is headed `Contact Form`: the title, then the type name (`ContentCard-FlowPart.Edit.cshtml:38` in
   `OrchardCore.Flows/Views`). Inside the card, use the first match of
   `GetByLabel("Action", new() { Exact = true })`, that is `.First`: the `Input` and `Text Area` widgets inside the
   Form widget each have a list labeled `Action` too, for their visibility
   (`OrchardCore.Forms/Views/FormInputElementVisibility.Edit.cshtml:8`), so the label alone matches several boxes.
   The Form widget's own `Action` comes first, since its part is placed before its Flow part
   (`OrchardCore.Forms/Migrations.cs`, positions 2 and 3). Scope the `Method` list and the two check boxes of
   `connect-2` to the card as well.

On screen: the page editor with the Form widget's `Action` box, empty, highlighted. Labels: `OrchardCore.Contents/
AdminMenu.cs:68`, `OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml:6`.

### `connect-2`

1. Fill the `Action` box with the workflow URL from the scratch file using `FillAsync` (the token is long: do not type
   it character by character), then press Tab.
2. Highlight, one after the other, the `Method` list (`POST`) and the checked `Anti Forgery Token` and `Save Form
   Location` boxes. Clear the highlights.

On screen: `Action` filled, `Method` = `POST`, the two boxes checked. Labels: `FormPart.Fields.Edit.cshtml:6,14,40,50`.

### `connect-3`

Scroll to the bottom of the editor and click `Publish` (`button[name="submit.Publish"]`,
`OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`), waiting for the navigation.

On screen: the Content Items list after publishing, with the `Contact` page listed as published. The editor was opened
from that list with a `returnUrl` (the title link is built by `ContentsTitle_SummaryAdmin.cshtml` in
`OrchardCore.Contents/Views`), and `Publish` goes back to it (`AdminController.cs`, `EditInternalAsync`).

## Clip `06-try`

Starts on `http://localhost:5010/contact` (the front end), opened before the capture starts.

### `try-1`

1. Click the `Message` box and type `Can you send me your price list?` (`TypeAsync`).
2. Click `Submit` (`ClickAndWaitForNavigationAsync`). The post is answered with a redirect back to `/contact`.

On screen: `/contact` again, with the validation summary listing `Name is required` and `Email is required`, and the
message still in the `Message` box. The error texts come from the two Validate Form Field activities of
`03-validate`. Source of the behaviour: `OrchardCore.Forms/Filters/ExportModelStateAttribute.cs`,
`ImportModelStateAttribute.cs`, `Views/Items/TextAreaPart.cshtml`.

### `try-2`

1. `TypeAsync` `Jamie Chen` into `Name`, and `jamie.chen@example.com` into `Email`. The message from `try-1` is
   still there.
2. Click `Submit`, waiting for the navigation. Wait for the address to end with `/thank-you`.

On screen: the `Thank You` page with the paragraph `Thank you for reaching out. We will get back to you soon.` The
SMTP catcher receives the email; it is not shown.

### `try-3`

1. `NavigateAsync("/Admin/Workflows/Types")`, then click `Instances` at the top right of the list (it lists the
   instances of every workflow, newest first; `Views/WorkflowType/Index.cshtml:24`, `Workflow/Index.cshtml`).
2. Click the instance id of the first row (the link whose text is the id; the link before it, the name of the
   workflow, opens the designer instead; `Views/WorkflowEntry.Fields.SummaryAdmin.cshtml`), waiting for the
   navigation. The page shows the workflow in a read-only designer, with the activities and connections the instance
   ran highlighted. Its `Workflow` tab holds the id, the creation time and the status; the `State` tab holds the JSON.
3. In the workflow panel on the right, click the `Journal` tab (`WorkflowsDesignerJSLocalizer.cs:272`).

On screen: the instance page with the executed path highlighted, and the `Journal` tab listing the activities that ran
with a status such as `Completed` and their outcomes. The newest instance is the valid post of `try-2`: its journal
contains the `Email Task`.

## The slides

One line each; `slides.py` draws them.

- `how-1`: four cards, `Form widget`, `Http Request Event`, `Validation tasks` and `A redirect`; the first two are
  bright.
- `how-2`: the form element of the Form widget, from `Form.Wrapper.cshtml`: the antiforgery token and the hidden field
  with the path of the page.
- `how-3`: the `Invoke` action of the HTTP workflow controller, trimmed.
- `how-4`: `GetWorkflowActionResult`, the three possible answers.
- `values-1`: the Liquid of the email subject and body, and the JavaScript `requestForm("Name")`.
- `state-1`: the three filters registered for MVC, and `ExportModelStateAttribute`.
- `state-2`: `ImportModelStateAttribute`.
- `state-3`: three cards about posting the form to an action of your own.

## Checks for private data

- Only `admin@example.com`, `owner@example.com`, `noreply@example.com`, `jamie.chen@example.com`, the fake name `Jamie
  Chen` and the texts above appear on screen. The sign-in page is never recorded.
- The workflow URL on screen (steps `start-3` and `connect-2`) holds a token of the throwaway site: discard the site
  and its `App_Data` after the last take, and never commit the scratch file.
- No password is typed on camera, and `<password>` never appears in a file of the repository.
- OCR the finished video at one frame per second and at every scene change, as the skill asks, and search for any
  address other than the ones listed, and for the machine's user name in a path or a title bar.
