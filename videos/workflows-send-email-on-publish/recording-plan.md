# Recording plan: Workflows, send an email when content is published

For the person who records the video. Read it from top to bottom; nothing here needs a decision. A selector below stands
for `scene.Page.Locator(...)`. "OC:" is a path in the Orchard Core source (the commit named in `catalog/features.json`),
where the label on screen comes from. Several selectors could not be checked without a running site: they are marked
(confirm) and listed in `OPEN-QUESTIONS.md`.

## 1. The site it needs

- Orchard Core `src/OrchardCore.Cms.Web` of the commit in `catalog/features.json`, on `http://localhost:5010`, with its
  own `ORCHARD_APP_DATA` folder outside both repositories, as `references/recording.md` of the skill describes.
- Provisioned by Auto Setup (no setup form and no login form on camera): recipe **Blog**, site name **Orchard Core**,
  time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, database SQLite. The password is chosen
  outside git and passed in `VIDEO_PASSWORD`; write it nowhere.
- The site is a fresh Blog site. The Blog recipe enables Contents but neither Workflows, Email nor SMTP Email Provider,
  and it creates the content types used here, among them **Article** (creatable; its parts are Title, Autoroute, HTML
  body and Article, and none of its fields is required: OC `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).
  The recipe makes three types creatable (Article, Blog and Page), so the **New** control of Content Items is a drop-down.
- Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address as the skill says. This does not block the mail: SMTP does not use
  an HTTP proxy, and the mail goes to `localhost`.
- Use no `VIDEO_INIT_SCRIPT`: the site is made from scratch, so no real name can appear.

### External service: a local test mail server

The workflow sends a real SMTP message, so the video needs a server that catches it. The tracker plans smtp4dev in Docker:

```bash
docker run --rm -p 3000:80 -p 2525:25 rnwood/smtp4dev
```

- Its web page is `http://localhost:3000` (the "mail page"); its SMTP port on this computer is `2525`.
- The container keeps the messages in memory. Stop it with Ctrl+C or `docker stop`, and start it again for a take that
  must begin with an empty inbox (clips `02-setup` and `07-try`).
- The command, the ports and the labels of its web page are not part of Orchard Core and were not checked: see
  `OPEN-QUESTIONS.md`. The narration never names the product or its labels (it says "the test mail server").
- Remove the container afterwards.

### Seed, outside the capture

Do this once on the fresh site, signed in as `admin`, before the first clip. Nothing here is recorded.

1. **Tools > Features** (OC: `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs`, search box `#search-box`): search
   `workflows` and click **Enable** on **Workflows** (`#btn-enable-OrchardCore_Workflows`); it enables Liquid and
   Scripting with it if they are not. Search `smtp` and click **Enable** on **SMTP Email Provider**
   (`#btn-enable-OrchardCore_Email_Smtp`); it enables **Email** with it (`Manifest.cs` of OrchardCore.Email.Smtp lists the
   dependency, and the Enable action forces dependencies on). Search `email` and check that **Email** and **SMTP Email
   Provider** both read Enabled. The Email Task activity exists only when Email and Workflows are both enabled
   (OC: `OrchardCore.Modules/OrchardCore.Email/Workflows/Startup.cs`).
2. **Settings > Communication > Email** (`/Admin/Settings/email`). On the **SMTP** tab:
   - **Enable SMTP Provider**: checked.
   - **Sender email address**: `noreply@example.com`.
   - **Delivery method**: Network.
   - **Host name**: `localhost`.
   - **Port number**: `2525`.
   - **Auto Select Encryption method**: unchecked. **Encryption method to use**: None. **Require credentials**:
     unchecked. **Ignore Invalid SSL Certificate**: unchecked.

   On a site that never saved these settings the form already shows them so (the editor copies `SmtpSettings`, whose
   defaults are false and None: OC `Drivers/SmtpSettingsDisplayDriver.cs:81-82`); only check that they read so.
   Click **Save** (OC `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). The page reloads with the message "Site settings updated successfully." Saving also makes SMTP the
   default provider when none is set (OC: `Drivers/SmtpSettingsDisplayDriver.cs:195`); open the **Providers** tab and
   check that **Default provider** reads "Simple Mail Transfer Protocol (SMTP)".
   (The same values can be applied with the `Settings` recipe step and its `SmtpSettings` section, as the SMTP
   module's documentation shows.)
3. **Tools > Testing > Email Test**: choose the provider, **To** `editors@example.com`, **Subject** `Test`, **Body**
   `Test`, click **Send**. The message "Message sent successfully." appears, and the test mail server shows the message.
   If it does not, fix the settings before recording anything. Then restart the test mail server so its inbox is empty.
4. **Design > Workflows** must list no workflow: the list reads "Nothing here! There are no workflow types for the
   moment." Delete any workflow made while trying things out.
5. Check that **Content > Content Items** has a **New** drop-down with **Article** in it (the top-level **New** menu of
   the admin is off by default, so the video uses the drop-down of the list).
6. Stop the site and copy its `App_Data` folder and `state.json` aside. This is **snapshot A**.

## 2. Order of the clips, and what changes the site

The clips depend on each other and are recorded in the order of the video. Take a copy of `App_Data` after each clip
that changes the site, to start a new take from the state the previous clip left.

| Clip | Starts from | Changes the site? | Snapshot to start a new take from |
|---|---|---|---|
| `02-setup` | A | No (read only) | A |
| `03-create` | A | Yes: creates the workflow, empty | A |
| `04-event` | after `03-create` | Yes: adds the event, with its content type | after `03-create` |
| `05-email` | after `04-event` | Yes: adds the Email Task and its settings | after `04-event` |
| `06-publish` | after `05-email` | Yes: publishes version 1 | after `05-email` |
| `07-try` | after `06-publish` | Yes: publishes an Article and starts one instance | after `06-publish` |

Restart the test mail server before `02-setup` and before `07-try`, so the inbox is empty. A take of `06-publish` or
`07-try` needs the published workflow state of the previous snapshot, not a fresh one. `02-setup` can be recorded at any
time after the seed.

For every clip, create the scene as the skill's `Scenes.cs` does (`NewSceneAsync`), do everything before
`StartCaptureAsync()` off camera, and keep the actions of a step shorter than its narration.

### The two pages of a clip that shows the mail

Clips `02-setup` and `07-try` end on the mail page. `OpenAsync` and `NavigateAsync` only open pages of the Orchard site
(they prepend the base URL), so create the mail page by hand before the capture:

```csharp
var mail = await scene.Context.NewPageAsync();
await mail.GotoAsync("http://localhost:3000", new() { WaitUntil = WaitUntilState.NetworkIdle });
await scene.OpenAsync("/Admin/...");          // the Orchard page of the clip; scene.Page stays the Orchard page
await scene.Page.BringToFrontAsync();
// ... later, in the step: await scene.SwitchToAsync(mail);
```

## 3. The clips

### Clip `02-setup` (chapter 02, What the site needs)

Starts on: `/Admin/Settings/email` (Settings > Communication > Email), opened with `OpenAsync`, the **Providers** tab
showing, the admin menu expanded on **Settings** and **Communication** so **Email** is visible. The mail page is open in
a second tab, with an empty inbox.

- `setup-1`
  - Actions: highlight the **Email** entry of the admin menu under **Communication**; click the **SMTP** tab; if the
    **Port number** is below the bottom of the page, scroll down 200 px with the pointer over the form; highlight, one
    after the other, **Enable SMTP Provider**, **Sender email address**, **Host name** and **Port number**. Do not
    change anything.
  - On screen at the end: the **SMTP** tab with **Enable SMTP Provider** checked, **Sender email address**
    `noreply@example.com`, **Delivery method** Network, **Host name** `localhost`, **Port number** `2525`.
  - Labels: OC `OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:39-40` (Communication, Email),
    `OrchardCore.Modules/OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88` (the SMTP tab; confirm the tab
    text), `OrchardCore.Modules/OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10,17,26,50,59`.
- `setup-2`
  - Actions: `await scene.SwitchToAsync(mail);` and hold.
  - On screen at the end: the mail page with no message in its list.
  - Labels: none from Orchard Core. This screen belongs to the test mail server, and the narration names nothing on it.

### Clip `03-create` (chapter 03, Create the workflow)

Starts on: `/Admin/Workflows/Types` (Design > Workflows), opened with `OpenAsync`, the admin menu expanded on **Design** so
**Workflows** is visible and highlighted as the current page. The list shows "Nothing here! There are no workflow types
for the moment."

- `create-1`
  - Actions: highlight the **Workflows** entry under **Design** in the admin menu; then highlight and click **Create
    Workflow** with `ClickAndWaitForNavigationAsync` (the button is `a.create`).
  - On screen at the end: the Create Workflow page with the **Name** field focused and **More settings** closed.
  - Labels: OC `OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18-19` (Design, Workflows),
    `Views/WorkflowType/Index.cshtml:25` (Create Workflow), `Views/WorkflowType/Index.cshtml:34` (the empty list).
- `create-2`
  - Actions: first highlight **More settings** (`summary` with that text; leave it closed), then clear the highlight;
    `TypeAsync` in the **Name** field (`input#Name`: the view model has no prefix) the text `Notify the editors when an article is
    published`, at the moment the narration says "Let's type"; `ClickAndWaitForNavigationAsync` on **Create and open
    the designer** (`button.save`); wait for `[data-cy="start-picker"]` to be visible.
  - On screen at the end: the designer, empty, with "Start with an event" on the canvas and the name of the workflow
    in the toolbar.
  - Labels: OC `Views/WorkflowType/Create.cshtml:21` (Name), `:25` (the hint that suggests the name), `:30` (More
    settings), `:34` (its hint: the defaults suit most workflows), `:131` (Create and open the designer).

### Clip `04-event` (chapter 04, Start with an event)

Starts on: the designer of the workflow made by `03-create`. Open it before the capture: `/Admin/Workflows/Types`, then
click the name of the workflow (a plain link; its **Edit** button does the same); wait for `[data-cy="start-picker"]`.
No activity is selected, the Activities list is expanded on the left.

- `event-1`
  - Actions: highlight the **Activities** title of the left pane (`.wfd-toolbox-title`), clear it, then highlight the
    "Start with an event" block (`[data-cy="start-picker"]`).
  - On screen at the end: the empty canvas with "Start with an event" and its buttons, the first one **Content
    Published Event**.
  - Labels: OC `Services/WorkflowsDesignerJSLocalizer.cs:63` (Activities), `:334-335` (Start with an event and its hint),
    `Assets/designer/src/canvas/StartPicker.vue:13` (Content Published Event is the first suggestion).
- `event-2`
  - Actions: click **Content Published Event** (`[data-cy="start-picker-ContentPublishedEvent"]`); wait for
    `[data-cy="start-badge"]` and for `[data-cy="activity-panel"]` to be visible.
  - On screen at the end: one activity on the canvas, headed "Content Published Event", with the green **Start** badge;
    the body of its card reads "Content Published" (the heading of the activity's own design view) and, under it,
    "Any content type"; the activity panel open below it, with **Title** and **Content Types**. The narration names the
    header, "Content Published Event", and the badge.
  - Labels: OC `OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22`,
    `OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Design.cshtml:4,6`,
    `...Fields.Edit.cshtml:5`, `Services/WorkflowsDesignerJSLocalizer.cs:329` (Start). The first event added becomes the
    start activity: OC `Services/WorkflowTypeDraftManager.cs:174-175`.
- `event-3`
  - Actions: click the **Content Types** picker (it shows "Any content type"; the widget is a searchable multi-select
    drawn by the bootstrap-select library, confirm the selectors); type `Article` in its search box ("Search content
    types"); click the option **Article**; click an empty part of the panel (or press Escape) to close the list; wait
    for the button to read **Saved!** or for the card on the canvas to read "Article" (the form applies a change about
    0.6 s after it; the picker fires a native `change` itself: OC `OrchardCore.Contents/Assets/ts/content-type-picker.ts:19`).
    If neither has happened after about 1.5 s, click **Apply** (`[data-cy="panel-apply"]`), which sits at the foot of the
    panel and applies the same change.
  - On screen at the end: the picker shows the tag **Article**, and the activity card on the canvas reads "Article"
    instead of "Any content type".
  - Labels: OC `OrchardCore.Modules/OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml:16,20`.
    The type's display name "Article" comes from the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
    `"DisplayName": "Article"`).

### Clip `05-email` (chapter 05, Add the email)

Starts on: the designer of the same workflow, opened as in `04-event`. It holds one activity, **Content Published
Event** (Start badge, card "Article"), with its **Done** outcome unconnected. Click an empty part of the canvas so no
activity is selected and the activity panel is closed. The canvas must have room to the right of the event.

- `email-1`
  - Actions: click the **Done** port of the event (`[data-outcome="Done"]` inside the activity; a click, not a drag);
    wait for `[data-cy="quick-add"]`; `TypeAsync` `email` in `[data-cy="quick-add-search"]`; highlight the row
    **Email Task** (`[data-cy="quick-add-EmailTask"]`: the key is the activity name, `TaskActivity<T>.Name` in
    `OrchardCore.Workflows.Abstractions/Activities/TaskActivity.cs:8`), which reads "Email Task" and its category
    "Messaging" (`ActivityCategoryLocalizationSources.cs:29`); click it.
  - On screen at the end: the menu titled "Add the next activity, after Done", with `email` in its search box and the
    **Email Task** row just clicked; the Email Task starts to appear on the canvas. Click the row as the narration says
    "choose Email Task", not before.
  - Labels: OC `Services/WorkflowsDesignerJSLocalizer.cs:333` (Add the next activity, after {0}), `:46` (Search
    activities), `OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentActivity.cs:60` (Done),
    `OrchardCore.Modules/OrchardCore.Email/Workflows/Activities/EmailTask.cs:35,37` (Email Task, Messaging).
- `email-2`
  - Actions: wait for the Email Task to appear to the right of the event and for `[data-cy="activity-panel"]`; drag the
    top edge of the panel (`[data-cy="activity-panel-resizer"]`, a separator) up with `DragAsync` until the panel takes
    about 60 % of the height of the page (target: the pointer at about `y = 330` CSS pixels), so that **Recipients**,
    **Subject** and **Format** can be shown with one or two scrolls.
  - On screen at the end: two activities joined by an arrow from **Done**; the panel enlarged, showing the **Title**
    and **From** fields of the Email Task.
  - Labels: OC `OrchardCore.Modules/OrchardCore.Email/Views/Items/EmailTask.Fields.Edit.cshtml:7` (From).
- `email-3`
  - Actions: highlight, one after the other, the **From**, **Sender** and **ReplyTo** fields (`input[id$="AuthorExpression"]`,
    `input[id$="SenderExpression"]` and `input[id$="ReplyToExpression"]`; leave them empty, and highlight the inputs, not
    the labels: the label of **ReplyTo** is bound to the **Sender** input in the view); scroll the panel body (pointer over
    the form, `ScrollAsync` by about 200 px) so **Recipients** is visible; `TypeAsync` in **Recipients** (`input[id$="RecipientsExpression"]`: the
    display driver prefixes the ids, as the comment of `Assets/ts/email-task.ts:5-8` says) the text `editors@example.com`; press Tab.
  - On screen at the end: **From**, **Sender** and **ReplyTo** empty above, **Recipients** reading `editors@example.com`.
  - Labels: `EmailTask.Fields.Edit.cshtml:7,16,25` (From, Sender, ReplyTo), `:11` (the hint "Leave blank to use the configured
    email address."), `:34` (Recipients), `:38` (the hint: comma-separated list). Do not read or show the "With Liquid support" hints in the
    narration; they are on screen and that is fine.
- `email-4`
  - Actions: scroll so **Subject** and **Format** are visible; `TypeAsync` in **Subject**
    (`input[id$="SubjectExpression"]`) the text `An article was published`; press Tab; `SelectAsync` on
    **Format** (`select[id$="BodyFormat"]`) the option **Text**.
  - On screen at the end: **Subject** filled, **Format** reading Text, **HTML Body** gone and **Text Body** showing.
  - Labels: `EmailTask.Fields.Edit.cshtml:61` (Subject), `:70` (Format), `:74` (Text), `:82` (Text Body). The script
    that hides the unused body is `OrchardCore.Modules/OrchardCore.Email/Assets/ts/email-task.ts:20-31`.
- `email-5`
  - Actions: scroll so **Text Body** is visible; click inside its editor (it is a CodeMirror editor bound to the text
    area, so click `#textBodyDiv .CodeMirror` or `#textBodyDiv .cm-content`, confirm) and type the text with
    `scene.Page.Keyboard.TypeAsync("An article was published on the site. Open the admin to review it.")`; click
    **Apply** (`[data-cy="panel-apply"]`), which is always in view: its bar is `position: sticky; bottom: 0` at the
    foot of the panel (`wwwroot/Styles/workflows-designer.css`, `.ocd-form-actions`), so no scroll is needed; wait until
    the button reads **Saved!**. The button keeps **Saved!** for about 1.8 seconds (`SAVED_FEEDBACK_DURATION` in the
    designer bundle, `wwwroot/Scripts/Workflows/designer/workflows-designer.js`), so click **Apply** last in the step,
    close to the moment the narration says "Saved!", and keep the pointer on it.
  - On screen at the end: **Text Body** with the message, and the **Apply** button showing **Saved!** (green), which
    lasts only about 1.8 seconds: the end of the narration must fall inside that time. A change made in a field is
    applied by itself 0.6 s after it (`autoApplyDelay`), so **Saved!** may already have shown once; clicking **Apply**
    shows it again.
  - Labels: `EmailTask.Fields.Edit.cshtml:82`; the **Apply** and **Saved!** texts: `Services/WorkflowsDesignerJSLocalizer.cs:132,134`.
    The form also holds a "When it fails" section after the fields (`Views/ActivityRetryPolicy.Edit.cshtml:7`): leave it.
- `email-6`
  - Actions: close the panel with its close button (**Close the activity panel**, `[data-cy="activity-panel-close"]`);
    highlight the Email Task on the canvas, then its two ports **Done** and **Failed** one after the other
    (`[data-outcome="Done"]`, `[data-outcome="Failed"]` inside the Email Task).
  - On screen at the end: the two activities on the canvas; the header of the Email Task card reads "Email Task", its
    body reads "Send Email" and, under it, the subject in quotes; its ports are **Done** and **Failed**, neither
    connected.
  - Labels: OC `EmailTask.cs:139` (Done, Failed), `EmailTask.Fields.Design.cshtml:6-7`,
    `Services/WorkflowsDesignerJSLocalizer.cs:151` (Close the activity panel).

### Clip `06-publish` (chapter 06, Publish the workflow)

Starts on: the designer of the workflow, opened as above. The canvas shows both activities; nothing selected; the toolbar
reads "All changes saved" and the **Publish** button is enabled (a draft exists). Open the **Issues** tab of the panel on
the right once, before the capture, and check it reads "No issues found."; otherwise fix the cause (a warning would make
the designer ask before publishing).

- `publish-1`
  - Actions: highlight the save status in the toolbar ("All changes saved"); highlight **Publish**
    (`[data-cy="toolbar-publish"]`); click it.
  - On screen at the end: the toolbar after the click, with the toast appearing.
  - Labels: OC `Services/WorkflowsDesignerJSLocalizer.cs:166` (All changes saved), `:190` (Publish).
- `publish-2`
  - Actions: wait for the toast "The workflow was published as version 1." and for the badge `[data-cy="published-version"]`
    reading "Version 1" next to the name; highlight the badge.
  - On screen at the end: the badge "Version 1" and, if it has not faded, the toast.
  - Labels: `Services/WorkflowsDesignerJSLocalizer.cs:221` (the toast), `:218` (Version {0}).
- `publish-3`
  - Actions: none that change anything; move the pointer to the badge and hold. (The **Discard draft** and **Publish**
    buttons are disabled now that there is no draft.)
  - On screen at the end: the same view as `publish-2`.
  - Labels: the narration repeats the designer's own text for the toolbar's Versions button: OC
    `Services/WorkflowsDesignerJSLocalizer.cs:213` ("Each publish creates a version.") and the documentation, section
    "Drafts and Publishing" of the Workflows module ("A draft doesn't run: the workflow keeps running its published
    definition until you publish the draft").

### Clip `07-try` (chapter 07, Try it)

Starts on: `/Admin/Contents/ContentItems` (Content > Content Items), opened with `OpenAsync`, the admin menu expanded on
**Content**. The mail page is open in a second tab with an empty inbox (restart the test mail server first).

- `try-1`
  - Actions: click the **New** drop-down (`#new-dropdown`) and then **Article** in its list with
    `ClickAndWaitForNavigationAsync`.
  - On screen at the end: the editor of a new Article, with the **Title** field focused.
  - Labels: OC `OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68` (Content, Content Items),
    `Views/ContentsAdminListCreate.cshtml:20` (New), `Title`: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`.
- `try-2`
  - Actions: `TypeAsync` in **Title** the text `Spring garden tips`; leave every other field empty; highlight the
    **Save Draft** button while the narration says it, then the **Publish** button; click **Publish** (`button.publish`)
    with `ClickAndWaitForNavigationAsync` at "Click Publish", the last words of the narration.
  - On screen at the end: the list of content items after the redirect, with the new Article at the top.
  - Labels: OC `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` (Publish) and
    `Views/Content.SaveDraftButton.cshtml:19` (Save Draft): the **New** link carries a `returnUrl`, so both are the
    split buttons of the second branch of each view (the plain buttons of line 7 show only without a `returnUrl`).
- `try-3`
  - Actions: highlight the green notification.
  - On screen at the end: the notification "Your Article has been published." (OC `Controllers/AdminController.cs:473`).
- `try-4`
  - Actions: `NavigateAsync("/Admin/Workflows/Types")` (this is Design > Workflows); `ClickAndWaitForNavigationAsync` on
    **Instances** (`[data-cy="all-instances"]`); highlight the status badge of the instance and the **Finished** counter.
  - On screen at the end: the Instances page, with the workflow `Notify the editors when an article is published`, one
    instance with the badge **Finished**, and the counter **Finished** at 1.
  - Labels: OC `Views/WorkflowType/Index.cshtml:24` (Instances), `Views/Workflow/Index.cshtml:43` (Finished),
    `OrchardCore.Workflows.Abstractions/Helpers/ActivityExtensions.cs:55` (the status text).
- `try-5`
  - Actions: `await scene.SwitchToAsync(mail);` click the message in the list so its content shows (confirm the
    selector); highlight the sender, the recipient and the subject. Do not open the headers or the raw source.
  - On screen at the end: the mail page showing one message from `noreply@example.com` to `editors@example.com`,
    subject `An article was published`, body `An article was published on the site. Open the admin to review it.`

## 4. Cards and slides

- `00-intro` (card): the opening card, title "Send an Email When Content Is Published".
- `01-idea` (slides): `idea-1` shows three cards, the problem (an article is published, someone writes the email, a
  workflow does it instead); `idea-2` shows Event, Task and Outcome, all bright; `idea-3` shows Content Published
  Event, Done and Email Task.
- `02-needs` (slide): `needs-1` shows the four things the site needs: Workflows, Email, SMTP Email Provider, Contents.
- `08-next` (slides): the same four cards in each step, one bright at a time: `next-1` Content Types, `next-2` Other
  events, `next-3` Recipients, Cc and Bcc, `next-4` Values from the content.
- `99-outro` (card): the closing card, subtitle `docs.orchardcore.net`.

## 5. Checks for private data

- Every address on screen is `@example.com`: `noreply@example.com`, `editors@example.com`. The user `admin` has the email
  `admin@example.com`.
- The mail page shows the message list and one message. Do not open its headers or raw source: they can show the name of
  this computer. Look at the first frames of `setup-2` and `try-5` for anything else it shows (a version number is fine,
  a machine name is not).
- The recording shows the page only, not the browser's tabs or address bar. The admin pages show no real name.
- No password appears anywhere: the SMTP settings need none, and the login is not recorded.
- The Content Items list shows the sample content of the Blog recipe and the Article made here; check that none of it
  carries a real name.
- Before assembling, look at every contact sheet (`review.py`): the frame at the end of each step must show what its
  narration says, with no error message, no empty list where the narration names an item, and no leftover workflow from a
  failed take. After assembling, run the OCR scan of the skill (one frame per second, at every scene change, and five
  frames per second) and search for `@` addresses other than `example.com`, and for the name of this computer.
