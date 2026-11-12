# Recording plan: Forms: Validate What Visitors Type

An admin video: it shows the admin screens and the front end of the site, and no code. The recorder drives a browser
only. In this plan a selector in backticks stands for `scene.Page.Locator(...)`, and a `file:line` points into the
Orchard Core source (`src/OrchardCore.Modules/...`) that the label or the selector comes from. The selectors are read
from the source: check each one once with `Recorder probe` before recording, and adjust it if the page differs.

Where a step says "wait for", the condition is the one to wait for before the next action; never wait a fixed time
longer than 1 s.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, on `http://localhost:5010`,
  with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). The recording needs no external
  service: nothing in this video sends an email or calls a provider. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead
  address anyway.
- **Setup**: Auto Setup, Blog recipe, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email
  `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git). No setup or sign-in
  form is recorded. This video is not about the Setup screen, so the site is provisioned before recording.
- **Features to enable before recording** (Tools, Features: `OrchardCore.Features/AdminMenu.cs:27`; search each name
  and enable it): **Forms**, **Workflows** and **HTTP Workflows Activities** (`OrchardCore.Forms/Manifest.cs`,
  `OrchardCore.Workflows/Manifest.cs:13-27`). Forms needs Widgets and Flows, which the Blog recipe enables. The
  Blog recipe gives the `Page` content type its Title, Autoroute (permalink from the title) and Flow parts
  (`TheBlogTheme/Recipes/blog.recipe.json:444-497`), and a Raw Html widget.
- **Settings**: none to change. Leave the Forms and Workflows settings as they are: the Anti Forgery Token and Save Form
  Location options of the Form widget stay checked, and both Form Location Key fields stay empty.
- **Sign-in**: `Recorder setup` once, outside the capture (`state.json` is git-ignored).

### The seed data (create it outside the capture, in this order)

All values are fake. Use the admin screens; the recorder may use `fetch` calls for the content items if it prefers.

1. **The Thank You page.** Open `/Admin/Contents/ContentTypes/Page/Create`.
   - Title: `Thank You`.
   - Add Widget (the `+` button with the title "Add Widget", `OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`),
     choose **Raw Html**. In its **Content** box type `<p>Thank you for reaching out. We will reply soon.</p>`.
   - Click **Publish**. The page is served at `/thank-you` (Autoroute builds the permalink from the title): open it
     once to check.
2. **The Contact Form page.** Open `/Admin/Contents/ContentTypes/Page/Create`.
   - Title: `Contact Form` (permalink `contact-form`).
   - Add Widget, choose **Form**. In the Form widget (`OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml`):
     Title `Contact`; **Action** empty for now (step 4 fills it in); **Method** `POST`; **EncType**
     `application/x-www-form-urlencoded`; **Anti Forgery Token** checked; **Save Form Location** checked.
   - Inside the Form widget, use its own Add Widget (`+`) button four times, in this order:
     1. **Input**: **HTML Name** `Name`; **Label option** `Standard`, **Label Text** `Name`; **Type** `Text`;
        **Placeholder Text** `Enter your name`; **Validation option** `None`.
     2. **Input**: **HTML Name** `Email`; **Label option** `Standard`, **Label Text** `Email`; **Type** `Email`;
        **Placeholder Text** `Enter your email address`; **Validation option** `None`.
     3. **Text Area**: **HTML Name** `Message`; **Label option** `Standard`, **Label Text** `Message`;
        **Placeholder Text** `Enter your message`; **Rows** `5`; **Validation option** `None`.
     4. **Button**: **Text** `Submit`; **Type** `Submit`.
     There is no Validation Summary widget yet: `messages-4` adds it on camera.
   - Click **Publish**.
3. **The workflow, only its start event.** Open Design, Workflows (`/Admin/Workflows/Types`), click
   **Create Workflow** (`OrchardCore.Workflows/Views/WorkflowType/Index.cshtml:25`).
   - Name: `Contact Form Validation`. Leave **More settings** alone. Click **Create and open the designer**
     (`Views/WorkflowType/Create.cshtml:131`).
   - The empty designer offers "Start with an event" (`Services/WorkflowsDesignerJSLocalizer.cs:334`): click the
     button of the **Http Request Event** (`[data-cy="start-picker-HttpRequestEvent"]`,
     `Assets/designer/src/canvas/StartPicker.vue:35`). The first event added becomes the start activity
     (`Services/WorkflowTypeDraftManager.cs:175`), and its settings open in the activity panel.
   - Set **HTTP Method** to `POST` (the list starts on `GET`, `Http/ViewModels/HttpRequestEventViewModel.cs:9`), press
     Tab. Leave **Validate Antiforgery Token** checked, **Token Life Span** at `0` and **Form Location Key** empty.
   - Wait until the **URL** field is not empty: the editor generates it when it opens
     (`Workflows/Assets/Scripts/workflow-url-generator.ts:37-39`). Read its value with a script
     (`document.querySelector('[data-workflow-url-text]').value`) rather than with the Copy button, since the
     clipboard of a headless browser is unreliable. The value is a path with a token, such as
     `/Workflows/HttpWorkflow/Invoke?token=...`.
   - Close the activity panel and wait for the toolbar to say "All changes saved". **Do not publish**: `rules-9`
     does. Leave the activities as they are: the workflow holds the start event only.
4. **The Action of the form.** Open the Contact Form page in the editor (Content, Content Items, **Edit**), type the
   path from step 3 into the Form widget's **Action** field, and click **Publish**.
   Until `rules-9` publishes the workflow, a submission of the form gets a "not found" answer: that is expected.

Then **stop the site and copy its `App_Data` folder and `state.json` aside**: this is the snapshot every full take
starts from.

### Fake data typed on camera (clip `04-try`)

Name `Jamie Chen`; email `jamie.chen@example.com`; message `I would like to know more about your services.`

## Order of the clips

Record in this order: `02-messages`, `03-rules`, `04-try`. The video plays them in the same order.

- `02-messages` changes the Contact Form page (Validation options, a Validation Summary widget).
- `03-rules` changes the workflow (activities, publication).
- `04-try` changes no setting (it only creates workflow instances), and needs both of the others: the Standard
  options and the summary from `02-messages`, and the published workflow from `03-rules`.
- **To record a clip again**, stop the site, restore the snapshot, start the site, then run `02-messages` and
  `03-rules` again (even to retake one of them) before `04-try`. `04-try` alone can be retaken at once.
- The slides (`why-1` to `why-3`) and the two cards need no site.

## Before each clip

`Recorder setup` once. Check that the browser is signed in as `admin`. Collapse the admin menu unless the clip's first
step says otherwise. A leftover toast, an open dialog or an error message on the first frame means the site is not in
the state the clip expects: restore the snapshot.

## Opening card and slides

- `intro` (card): the opening card, with the title of the video and its subtitle.
- `why-1` (slide): "Validation has two parts", two cards: a workflow checks, the form displays.
- `why-2` (slide): "Three ways to display errors", three cards: the Validation Summary widget, the Validation option
  of an input, the Validation widget.
- `why-3` (slide): "The way back to the form", three cards: Save Form Location, HTTP Redirect To Form Location, the
  error messages.
- `outro` (card): the closing card, subtitle `docs.orchardcore.net`.

## Clip `02-messages`

**Starts on:** the Contact Form page in the editor, opened from the list: Content, Content Items, the **Edit** button
of Contact Form (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml`), which adds a `returnUrl`. With
`OpenAsync`, that is `/Admin/Contents/ContentItems/<contentItemId>/Edit?returnUrl=%2FAdmin%2FContents%2FContentItems`
(`AdminController.cs:502`); read the content item id in the list. Do not open the editor without a `returnUrl`: a
publish then reloads the editor (`AdminController.cs:841-847`), where the **Action** field would show. Open it before
the capture. Before `StartCaptureAsync`: collapse the four widgets inside the Form widget (click
the `.widget-editor-btn-collapse` button of each header: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml`),
so the settings of the Form widget and the four collapsed headers fit on the screen, and scroll so the Form widget's
**Title** field is at the top. A widget is a `.widget.widget-editor` card with `data-content-type` set to `Form`,
`Input`, `TextArea` or `Button`.

The **Action** field is the first field of the Form widget settings (`FormPart.Fields.Edit.cshtml:5-11`) and holds the
path of the workflow with its token. It must never be on screen, and the helpers of the recorder scroll to what they
click or ring (`MoveToAsync` and `HighlightAsync` call `ScrollIntoViewIfNeededAsync`), so scrolling alone cannot keep it
out. Before `StartCaptureAsync`, and after the collapsing and the scroll above, hide it with a style rule:
`await scene.Page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = "div.ocat-wrapper:has(> div.ocat-end > input[name$='FormPart.Action']) { display: none !important; }" })`,
then check that `scene.Page.Locator("input[name$='FormPart.Action']")` is not visible (`IsVisibleAsync` returns
`false`) and that the **Method** field is the first setting under the Title and ID fields. If the Action field is
still visible, do not start the capture. The rule lasts until the page is left, which is after `messages-5`.

- `messages-1`: highlight (`HighlightAsync`, no caption) the **Save Form Location** checkbox of the Form widget
  (`input[type="checkbox"][name$="FormPart.SaveFormLocation"]`, label `Save Form Location`:
  `OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml:50`; the `[type="checkbox"]` part is needed because the
  editor also renders a hidden input of the same name for a checkbox, and a locator that matches two elements makes
  `HighlightAsync` skip the ring). It is checked. *On screen at the end of the narration:* the Form widget settings,
  Save Form Location checked and ringed, and no **Action** field (hidden). Clear the highlight at the end.
- `messages-2`: click the `.widget-editor-btn-expand` button in the header of the first Input card (**Name**); scroll
  it into view; `SelectAsync` the **Validation option** list of that card
  (`select[name$="FormElementValidationPart.ValidationOption"]`, first of the page; label `Validation option`,
  options `None` and `Standard`: `FormElementValidationPart.Fields.Edit.cshtml:6-12`) to `Standard`; then ring the
  list. *On screen at the end:* the Name card open, its Validation option showing **Standard**.
- `messages-3`: scroll down; expand the second Input card (**Email**) and `SelectAsync` its Validation option
  (the second `select[name$="ValidationOption"]`) to `Standard`; expand the Text Area card (**Message**) and do the
  same with the third list. Keep the actions under 8 s: type nothing, only select. *On screen at the end:* the
  Message card open with **Standard** selected, the Email card above it with **Standard**.
- `messages-4`: scroll up to the header of the Name card; move the cursor onto it (the buttons of a card header show on
  hover); click its `button[title="Insert Widget"]` (`ContentCard-FlowPart.Edit.cshtml:51`), then click the menu item
  `a.insert-widget[data-widget-type="ValidationSummary"]` (shown as **Validation Summary**). The new card appears
  above the Name card (the widget is inserted before the card whose button was clicked:
  `OrchardCore.Flows/Assets/ts/flows.edit.ts:96`). Wait for the new `.widget[data-content-type="ValidationSummary"]`
  card, scroll it into view, and ring its **Show Model Only Error** checkbox
  (`input[type="checkbox"][name$="ValidationSummaryPart.ModelOnly"]`, label `Show Model Only Error`:
  `ValidationSummaryPart.Fields.Edit.cshtml:8-10`; `[type="checkbox"]` for the same reason as in `messages-1`). Leave
  it **unchecked**: a checked box hides the errors of fields from the summary. *On screen at the end:* the Validation
  Summary card above the Name card, the checkbox unchecked and ringed.
- `messages-5`: `ClickAndWaitForNavigationAsync` on the **Publish** button
  (`button[name="submit.Publish"][value="submit.Publish"]`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`
  and `:19`). Both the plain button and the split button of an edit page opened with a `returnUrl` carry
  `name="submit.Publish"`, and the "and continue" item of the split button too (`:24`, value `submit.PublishAndContinue`),
  so the `value` part keeps the locator to one element. *On screen at the end:* the Content Items list that the editor
  was opened from, with the notification of the publication (`AdminController.cs:858`, a redirect to the `returnUrl`).
  The notification text is not narrated.

**Changes the site:** yes (the Contact Form page). Restore the snapshot before a new take.

## Clip `03-rules`

**Starts on:** the admin home (`/Admin`), with the **Design** menu open so **Workflows** is visible in it
(`OrchardCore.Workflows/AdminMenu.cs:18-22`). The designer is a single-page app: after every action below, wait for the
element named, never for a fixed time. Useful selectors of the designer
(`OrchardCore.Workflows/Assets/designer/src`): an activity card is `.wfd-node` (it has `data-node-id`); an outcome
port is `.wfd-port[data-outcome="Done"]` inside the card (the visible text is the outcome's name: Done, Valid or
Invalid; the dot of a port that leads nowhere is hollow and shows a `+` only while the cursor is on it:
`Assets/designer/src/styles/designer.css:1466-1481`); the menu that a click on such a port opens is
`[data-cy="quick-add"]`, its search box `[data-cy="quick-add-search"]`, and each activity in it
`[data-cy="quick-add-<ActivityName>"]` with the names below; the activity panel is `[data-cy="activity-panel"]`, its
close button `[data-cy="activity-panel-close"]`; the toolbar's Publish button is `[data-cy="toolbar-publish"]`.

To add an activity after an outcome, always: click the port, wait for `[data-cy="quick-add"]`, type the search text,
and **click the item** (`data-cy`), never press Enter: the first item of a search is not always the one wanted
(`Validate Form` also matches `Validate Form Field`). The activity is added to the right, connected to the outcome, and
the activity panel opens by itself when the activity has settings (`App.vue:262-287`, `:309-326`). Type into a
settings field with `TypeAsync` (delay 25 ms), then press Tab: the panel's form applies a change about 0.6 s after
the field loses focus (`ServerFormHost`, `autoApplyDelay` 600, in the built designer
`OrchardCore.Workflows/wwwroot/Scripts/Workflows/designer/workflows-designer.js`; the panel's **Apply** button,
`[data-cy="panel-apply"]`, applies at once and then reads **Saved!**). Wait for the card body to show the applied
value (see each step) rather than for the issue badge to go away: a badge (a number in a red badge) is shown only
while the settings are invalid. Then close the panel before the next port: the panel covers the bottom of the canvas.
Closing the panel applies a pending change as well.

| Activity | `data-cy` | Search text | Label on the card body |
|---|---|---|---|
| Bind Form Model State | `quick-add-BindModelStateTask` | `bind` | Bind Form Model State |
| Validate Form Field | `quick-add-ValidateFormFieldTask` | `validate form field` | Validate Form Field |
| Validate Form | `quick-add-ValidateFormTask` | `validate form` | Validate Form |
| HTTP Redirect To Form Location | `quick-add-HttpRedirectToFormLocationTask` | `redirect to form` | HTTP Redirect To Form Location |
| HTTP Redirect | `quick-add-HttpRedirectTask` | `redirect` | HTTP Redirect |

The list itself names an activity by its type name ("Bind Model State Task", "Validate Form Field Task"): the
narration names the title that the card shows once added. The toolbox on the left calls Bind "Bind Form to Model
State": the narration does not use that label, and the toolbox stays collapsed.

- `rules-1`: click **Workflows** in the Design menu (`ClickAndWaitForNavigationAsync`), then click the name link
  **Contact Form Validation** in the list (`OrchardCore.Workflows/Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml`;
  `ClickAndWaitForNavigationAsync`). Wait for one `.wfd-node` to be visible. Ring (`HighlightAsync`) the start card and
  its `POST` line. *On screen at the end:* the designer with the single start card (a **Start** badge, the text
  `POST` in its body, an outcome port **Done** with a hollow dot: it leads nowhere yet).
- `rules-2`: click the **Done** port of the start card; type `bind`; click `quick-add-BindModelStateTask`; wait for the
  new card; close the activity panel (leave its **Title** field empty). *On screen at the end:* the
  start card connected to a **Bind Form Model State** card, no panel.
- `rules-3`: click the **Done** port of the Bind card; type `validate form field`; click
  `quick-add-ValidateFormFieldTask`. In the panel, Settings tab: type `Name` in **Field Name**
  (`ValidateFormFieldTask.Fields.Edit.cshtml:8`), Tab; type `Name is required` in **Error Message**
  (`ValidateFormFieldTask.Fields.Edit.cshtml:16`), Tab; wait until the body of the new card reads "Validate Name"
  (`ValidateFormFieldTask.Fields.Design.cshtml:6`), which means both fields were applied. Leave the panel **open**.
  *On screen at the end:* the new card reading "Validate Name", the panel with Field Name `Name` and Error Message
  `Name is required`.
- `rules-4`: close the panel; ring the three ports **Done**, **Valid** and **Invalid** of the Validate Form Field card
  (one ring around `.wfd-node-ports` of that card is enough; none of the three leads anywhere yet). *On screen at the
  end:* the Validate Form Field card, connected to the Bind card, with its three ports ringed and no panel.
- `rules-5`: Email check: click the **Done** port of the Name check, add Validate Form Field, `Email` /
  `Email is required`, Tab, wait for the body "Validate Email", close the panel. Message check: click the **Done** port
  of the Email check, add Validate Form Field, `Message` / `Message is required`, Tab, wait for the body "Validate
  Message", close the panel. Use a typing delay of 25 ms and no extra pauses:
  the step must finish before its narration (about 15 s) does. *On screen at the end:* the chain Start, Bind, three
  checks, each connected to the next through **Done**, no panel (scroll the canvas with the wheel, or use
  **Fit to content**, if the last card is outside the view).
- `rules-6`: click the **Done** port of the Message check; type `validate form`; click `quick-add-ValidateFormTask`.
  This activity has no settings: no panel opens. *On screen at the end:* a **Validate Form** card at the end of the
  chain with two ports, **Valid** and **Invalid**, both with a hollow dot (neither leads anywhere yet).
- `rules-7`: click the **Invalid** port of Validate Form; type `redirect to form`; click
  `quick-add-HttpRedirectToFormLocationTask`; in the panel leave **Form Location Key** empty
  (`OrchardCore.Forms/Views/Items/HttpRedirectToFormLocationTask.Fields.Edit.cshtml:6`) and close it. *On screen at the
  end:* **Invalid** connected to an **HTTP Redirect To Form Location** card.
- `rules-8`: click the **Valid** port of Validate Form, so the menu is open on camera. Then run everything else of this
  step inside `scene.OffCameraAsync`: type `redirect`, click `quick-add-HttpRedirectTask`, wait for the panel, type
  `/thank-you` in **Location** (`OrchardCore.Workflows/Views/Items/HttpRedirectTask.Fields.Edit.cshtml:6-7`), Tab,
  wait until the body of the new card reads `/thank-you` (`HttpRedirectTask.Fields.Design.cshtml:7`), close the panel.
  This keeps the hint under that field, which names a template language, off the screen. *On screen at the end:* **Valid** connected to an **HTTP Redirect** card whose body reads
  `/thank-you`, no panel.
- `rules-9`: click the canvas control **Fit to content** (label from
  `Services/WorkflowsDesignerJSLocalizer.cs:358`; an icon-only button, `[data-cy="zoom-fit"]`, whose title and
  aria-label are `Fit to content`, found in the built designer `wwwroot/Scripts/Workflows/designer/workflows-designer.js`;
  the label shows only as a tooltip, so the viewer sees the icon being clicked), so the whole workflow is in view; then
  click **Publish** (`[data-cy="toolbar-publish"]`). No dialog should open: a publish with no warnings goes straight
  through (`App.vue:379-409`). If a dialog opens, it lists an issue: fix the issue (an activity whose settings were
  never applied) and record the step again. Wait for a toast whose text starts with "The workflow was published as
  version" (`WorkflowsDesignerJSLocalizer.cs:221`) and do not wait for a number: creating the workflow already saved
  version 1, with no activities (`Services/WorkflowTypeStore.cs:87`, `Services/WorkflowTypeVersionStore.cs:90-124`),
  so this first publish makes version 2. The toast is not read in the narration. *On screen at the end:* all eight cards in view, connected as Start, Bind, Name, Email and Message checks, Validate Form, with
  Invalid and Valid leading to the two redirects, and the toast.

**Changes the site:** yes (the workflow gets its activities and is published). Do not open the **Available data** tab
of any activity, and do not open the settings of **HTTP Redirect** on camera. Do not open the settings of the start
card either (its **URL** field holds the token of the workflow URL, see "Checks for private data"): a double click or
the card's settings button opens them, and the narration never needs them.

## Clip `04-try`

**Starts on:** the front end, `/contact-form`, opened with `OpenAsync` before the capture. Check that the form shows
the three fields with their labels, the Submit button and, above the Name field, the Validation Summary (empty, it
shows nothing). If the signed-in session adds anything of the admin to the page (a toolbar, an edit link), record this
clip in a context without `state.json`: visitors are not signed in.

A submission posts to the workflow, which answers with a redirect: always use `ClickAndWaitForNavigationAsync` for the
Submit button. Do not type anything into a field with a malformed value: the Email input has the type `email`, and a
browser can stop a malformed address before the form is sent.

- `try-1`: click **Submit** with the three fields empty; wait for the page to load at `/contact-form` and for
  `.validation-summary-errors` (the summary) to be visible. *On screen at the end:* the form, with the summary above
  the Name field listing `Name is required`, `Email is required` and `Message is required`.
- `try-2`: ring (`HighlightAsync`) the three `.field-validation-error` messages, under the Name, Email and Message
  fields. Clear the rings at the end. *On screen at the end:* the three messages ringed, each under its field.
- `try-3`: `TypeAsync` `Jamie Chen` into the Name field; click **Submit** (`ClickAndWaitForNavigationAsync`); wait for
  the summary. *On screen at the end:* the Name field still holding `Jamie Chen`, the summary listing only
  `Email is required` and `Message is required`, a message under Email and under Message and none under Name.
- `try-4`: `TypeAsync` `jamie.chen@example.com` into the Email field and `I would like to know more about your
  services.` into the Message field; click **Submit** (`ClickAndWaitForNavigationAsync`); wait for `/thank-you`.
  *On screen at the end:* the Thank You page with its paragraph, no error.

**Changes the site:** no setting; each submission starts a workflow instance.

## If the front end differs from the narration

The text of the error messages, and which fields keep their values, come from the workflow built in `03-rules`. If a
take shows an error message under a field that the narration does not describe, or a field that lost its value, do
not narrate around it: read `OPEN-QUESTIONS.md`, then fix the site or the take.

## Checks for private data

- The people, emails and messages of this video are invented: `Jamie Chen`, `jamie.chen@example.com`,
  `admin@example.com`, the Thank You and Contact Form texts. Nothing else is typed.
- The **Action** field of the Form widget holds the path of the workflow URL, which carries a token of the throwaway
  site (`/Workflows/HttpWorkflow/Invoke?token=...`). It must not appear in any frame: `02-messages` hides the field
  with a style rule before the capture, and `03-rules` never opens the settings of the start event. The OCR pass looks
  for the text `token=` and for `Invoke`; any hit means the take is not used.
- No password is typed on camera. The sign-in comes from `state.json` (never committed).
- OCR the video at one frame per second and at every scene change, as the skill asks, and look for any other email
  address, any host other than `localhost`, and the word "Liquid" (it must not appear in this video).
