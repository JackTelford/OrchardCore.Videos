# Recording plan: X (Twitter): Sign In With X and Post From Workflows

How to record this video without deciding anything. The narration is in `storyboard.json`; the recorder holds each step
for the length of its narration, so narrate first (`narrate.py`), then record.

Conventions of this plan:

- A selector in backticks stands for `scene.Page.Locator(...)`. `GetByLabel`, `GetByText` and `GetByRole` are the
  Playwright methods of the same name on `scene.Page`.
- "Source" lines are paths relative to `src/` of the Orchard Core checkout, at commit `92db4a83`, with the line where
  the label is written. The video describes that code. Record on a build of the same commit.
- Fake values only. The four keys, the callback path and the workflow text below are invented. The password of the
  `admin` user is never typed on camera: the recorder signs in outside the capture, with the password passed in
  `VIDEO_PASSWORD`. In this plan it is written `<password>`.
- Nothing in this video may reach X. The site is started with `HTTP_PROXY` and `HTTPS_PROXY` set to an address that does
  not answer (for example `http://127.0.0.1:9`). The X (Twitter) button of the login page is never clicked, the
  workflow is never run, and no Blog Post is published while the workflow of the last clip is published.

## The site it needs

| | |
|---|---|
| Build | `src/OrchardCore.Cms.Web` of Orchard Core at the commit above, run on `http://localhost:5010` |
| Data folder | A new empty folder outside both repositories, set in `ORCHARD_APP_DATA` |
| Provisioning | Auto Setup, Default tenant: site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git) |
| Language | en-US, light color scheme (the recorder's defaults) |
| Outbound calls | Blocked with a dead `HTTP_PROXY` / `HTTPS_PROXY`, as above |
| Recorder | `VIDEO_BASE_URL=http://localhost:5010`, `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`; `Recorder setup` signs in once and writes `state.json` (git-ignored) |

Features to enable before recording (outside the capture, as `admin`, on `/Admin/Features`):

- **Workflows** (Search `Workflows`, then Enable the row named exactly "Workflows" of the category "Workflows"; the search
  also lists "HTTP Workflows Activities", "Timer Workflows Activities" and "Session Workflows Activities", which stay
  disabled). The Blog recipe does not enable it, and the last clip needs it. Enabling it also enables Liquid and
  Scripting, its dependencies (`OrchardCore.Modules/OrchardCore.Workflows/Manifest.cs:16`). The X (Twitter) task only
  exists when this feature is enabled (`OrchardCore.Modules/OrchardCore.Twitter/Workflows/Startup.cs:10`).

Features that must stay disabled:

- **Secrets**. With it disabled, the two secret fields of the X (Twitter) settings are plain password fields, which
  is what the narration of chapter 03 shows. The Blog recipe leaves it disabled; do not enable it.
- Do not enable any other feature. The two X (Twitter) features are enabled on camera, in chapter 02.

Settings that must keep their defaults:

- Settings > Security > User Login, tab "External Login", the option "Use external provider for login." must stay
  cleared (its default). Checked, and with X as the only provider, the login page would send the visitor to X at once
  (label: `OrchardCore.Modules/OrchardCore.Users/Views/ExternalLoginSettings.Edit.cshtml:15`; menu:
  `OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:42`; behavior:
  `OrchardCore.Modules/OrchardCore.Users/Services/ExternalLoginFormEvents.cs:38`).

Seed data to create outside the capture: none. The Blog recipe provides the **Blog Post** content type that chapter 07
picks (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:188`, display name "Blog Post").

External services and local stand-ins: none. X is never contacted, so nothing stands in for it.

## Order of the clips, and what each one changes

Record in this order. Each clip starts from the state the previous one left, and every clip except `05-login` changes
the site. Take a copy of the data folder (and `state.json`) at each snapshot; to take a clip again, stop the site,
restore the snapshot named in the last column, and start the site.

| Order | Clip | Needs | Changes the site | Restore before a retake |
|---|---|---|---|---|
| 1 | `02-features` | Workflows enabled | Enables Sign in with X (Twitter), X (Twitter) Integration and External Authentication | Snapshot A: taken after Workflows is enabled and before anything else |
| 2 | `03-keys` | `02-features` | Saves the four fake keys | Snapshot B: taken after `02-features` |
| 3 | `04-signin` | `02-features` | Saves the callback path | Snapshot C: taken after `03-keys` |
| 4 | `05-login` | `03-keys` and `04-signin` | Nothing | Snapshot D: taken after `04-signin` (the clip changes nothing, so any take can follow another) |
| 5 | `07-workflow` | `02-features` and Workflows | Creates and publishes one workflow | Snapshot D |

The slide chapters (`01-why`, `06-first`) need no site and can be recorded at any time.

The login button appears only when all four keys are saved (the integration settings are validated before the sign-in
scheme is added: `OrchardCore.Modules/OrchardCore.Twitter/Signin/Configuration/TwitterOptionsConfiguration.cs:113-123`,
`OrchardCore.Modules/OrchardCore.Twitter/Services/TwitterSettingsService.cs:53`). That is why `05-login` needs `03-keys`.

## Slide steps

Drawn by `slides.py`; nothing is recorded.

- `why-1`: two cards, "X (Twitter) Integration" and "Sign in with X (Twitter)", with what each one does.
- `why-2`: four cards, the API key, API secret key, Access Token and Access Token Secret, under the heading "From the Keys and tokens tab of your X app". The first and third read "Marked as required on the page." (the required mark of `TwitterSettings.Edit.cshtml:20` and `:37`); the other two read "Stored encrypted in the settings."
- `first-1`: the four outcomes of a return from X; the first one ("The X account is already linked") is bright.
- `first-2`: the same four cards; the last three are bright. The third card says that the settings can create the account without asking: that is the case when "Do not ask username", "Do not ask email address" and "Do not create local password" are all selected (`OrchardCore.Modules/OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:201-205`; labels in `OrchardCore.Modules/OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:34`, `:45`, `:56`). The fourth is the case "Disable new external registration" (`:23` of the same view).
- `first-3`: two cards, "External Logins" and "Add another service to log in.".

The labels on these slides come from `OrchardCore.Modules/OrchardCore.Users/Views/ExternalAuthentications/LinkExternalLogin.cshtml:8`
("Link your account."), `RegisterExternalLogin.cshtml:9` ("Create a new account."),
`OrchardCore.Modules/OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:182` ("New registrations are disabled for this site."),
`OrchardCore.Modules/OrchardCore.Users/Views/UserMenuItems-ExternalLogins.cshtml:3` ("External Logins") and
`OrchardCore.Modules/OrchardCore.Users/Views/ExternalAuthentications/ExternalLogins.cshtml:42` ("Add another service to log in.").

## Clip `02-features`

Starts on `/Admin` (the dashboard), opened before the capture. All groups of the admin menu collapsed.
The clip enables the two X (Twitter) features, so it changes the site.

### `features-1`

Actions:

1. Click the "Tools" group of the admin menu (`#adminMenu`, the item with the text "Tools"), then click the "Features"
   link (`#adminMenu a[href$="/Admin/Features"]`) with `ClickAndWaitForNavigationAsync`.
2. Click the search box (`#search-box`) and type `Twitter` (delay 60).

Visible when the narration ends: the Features page with the search box holding `Twitter`, and only the category
heading "X (Twitter)" with two rows: "X (Twitter) Integration" and "Sign in with X (Twitter)", both with an "Enable"
button.

Source: `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26` ("Tools"), `:27` ("Features");
`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` (the "Search" box), `:88` (the category
heading); `OrchardCore.Modules/OrchardCore.Twitter/Manifest.cs:14` and `:21` (the names of the two features), `:15` and `:22`
(the category "X (Twitter)").

### `features-2`

Actions:

1. Highlight the dependency tags of the "Sign in with X (Twitter)" row
   (`.list-group-item` with the text "Sign in with X (Twitter)", then `.direct-dependencies` inside it) with no caption.
   The tags read "External Authentication" and "X (Twitter) Integration".
2. Wait 1500 ms, `ClearHighlightsAsync()`, then click the "Enable" link of that row
   (`a[href*="/Features/OrchardCore.Twitter.Signin/Enable"]`) with `ClickAndWaitForNavigationAsync`.

Visible when the narration ends: the Features page after the redirect, with the green notification "The feature Sign
in with X (Twitter) was enabled." at the top. The search box is empty again and all features are listed.

Source: `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:16` (dependency
tags), `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` ("Enable"),
`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317` (the notification, which names only the
feature that was clicked).

### `features-3`

Actions:

1. Click the search box and type `Twitter` (delay 40). Wait until the two rows are the only ones listed.
2. Highlight the "Enabled" badges of the two rows (`.list-group-item` with the text "X (Twitter)", then the text
   "Enabled"), with no caption.
3. Wait 1200 ms and `ClearHighlightsAsync()`. Then click the "Settings" group of the admin menu, and click the
   "Integrations" group inside it, so that "X (Twitter)" is listed under "Integrations". Do not click "X (Twitter)".

Visible when the narration ends: the two rows, each with the "Enabled" badge and a "Disable" button, and the admin
menu open on Settings > Integrations > X (Twitter).

Source: `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` ("Enabled");
`OrchardCore.Modules/OrchardCore.Twitter/AdminMenu.cs:25` ("Settings"), `:26` ("Integrations"), `:27` ("X (Twitter)").
The menu items exist only once the features are enabled.

## Clip `03-keys`

Starts on `/Admin`, opened before the capture. The clip saves the four fake keys, so it changes the site.

Fake values to type, exactly:

| Field | Value |
|---|---|
| API key | `example-api-key` |
| API secret key | `example-api-secret-key` |
| Access Token | `example-access-token` |
| Access Token Secret | `example-access-token-secret` |

### `keys-1`

Actions:

1. Click the "Settings" group of the admin menu, the "Integrations" group, then the "X (Twitter)" link
   (`#adminMenu a[href$="/Settings/OrchardCore.Twitter"]`) with `ClickAndWaitForNavigationAsync`.
2. Highlight the yellow warning (`.alert.alert-warning`) without a caption.

Visible when the narration ends: the settings page with the heading "X (Twitter) Integration Settings" and the warning
"The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the
process." highlighted. The four fields are empty.

Source: `OrchardCore.Modules/OrchardCore.Twitter/AdminMenu.cs:25-27` (the menu path);
`OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSettings.Edit.cshtml:7` (the warning), `:15` (the heading).

### `keys-2`

Actions:

1. `ClearHighlightsAsync()`.
2. `TypeAsync` into `input[name$=".APIKey"]` the value `example-api-key` (delay 45).
3. `TypeAsync` into `input[name$=".AccessToken"]` the value `example-access-token` (delay 45).
4. Highlight the two labels "API key" and "Access Token" (`label` with the text, exact), which carry the required mark.

Visible when the narration ends: the two plain fields filled, "API key" and "Access Token" with their required marks.

Source: `OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSettings.Edit.cshtml:20` ("API key", required), `:37` ("Access Token", required).

### `keys-3`

Actions:

1. `ClearHighlightsAsync()`.
2. `TypeAsync` into `input[type="password"][name$=".ConsumerSecret.Value"]` the value `example-api-secret-key` (delay 45).
3. `TypeAsync` into `input[type="password"][name$=".AccessTokenSecret.Value"]` the value `example-access-token-secret` (delay 45).

Visible when the narration ends: the two secret fields show dots; the four fields are filled. No clear text of the two
secrets is on screen.

Source: `OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSettings.Edit.cshtml:29` ("API secret key"), `:46` ("Access Token Secret");
`OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103-104` (without the Secrets feature
the field is a password input; the placeholder is "Enter a new value").

### `keys-4`

Actions:

1. Click "Save" (`button.save`) with `ClickAndWaitForNavigationAsync`. Saving restarts the tenant, so the page can take a
   few seconds: run the wait with `OffCameraAsync`, its condition being the text "Site settings updated successfully."
   visible.
2. Highlight the two secret fields (the same two inputs as in `keys-3`) without a caption.

Visible when the narration ends: the green notification "Site settings updated successfully." and the settings page
again. The API key and Access Token fields hold their values. The two secret fields are empty and show the placeholder
"The value is securely stored. Enter a new value to update it, or leave it blank to keep it." (the placeholder can be
cut off by the width of the field; that is acceptable).

Source: `OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` ("Save");
`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` ("Site settings updated successfully.");
`OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103` (the placeholder once a value is kept).

### `keys-5`

Actions: none. `ClearHighlightsAsync()`, then `HoldAsync` for the rest of the narration. The cursor rests beside the
"API secret key" field.

Visible when the narration ends: the same page as at the end of `keys-4`, with no highlight.

Source: `OrchardCore.Modules/OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:26-27` (the options "User-specified value"
and "Use a secret" that the field offers when the Secrets feature is enabled).

## Clip `04-signin`

Starts on `/Admin`, opened before the capture. The clip saves the callback path, so it changes the site.

### `signin-1`

Actions:

1. Click the "Settings" group of the admin menu, then "Security", then "Authentication", then the "Sign In with X (Twitter)" link
   (`#adminMenu a[href$="/Settings/OrchardCore.Twitter.Signin"]`) with `ClickAndWaitForNavigationAsync`.
   Click each group only if it is collapsed.

Visible when the narration ends: the page with the heading "Sign in with X (Twitter) Settings", the same restart
warning, the field "CallbackPath" (empty, with the placeholder `/signin-x`) and the checkbox "Store tokens".

Source: `OrchardCore.Modules/OrchardCore.Twitter/AdminMenuSignin.cs:25-28` ("Settings", "Security", "Authentication",
"Sign In with X (Twitter)"); `OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSigninSettings.Edit.cshtml:15` (the
heading), `:20` ("CallbackPath"), `:22` (the placeholder), `:32` ("Store tokens").

### `signin-2`

Actions:

1. `TypeAsync` into `input[name$=".CallbackPath"]` the value `/signin-x` (delay 70).
2. Highlight the hint under the field (`.hint` that follows it) without a caption.

Visible when the narration ends: "CallbackPath" holding `/signin-x`, and its hint "The request path within the
application's base path where the user-agent will be returned. The middleware will process this request when it
arrives." highlighted.

Typing the path is required: when the field is empty the page does not set a path itself, so the screen and the
behavior only agree when the path is written (see OPEN-QUESTIONS.md, question 2).

Source: `OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSigninSettings.Edit.cshtml:20-24`.

### `signin-3`

Actions:

1. `ClearHighlightsAsync()`. Highlight the "Store tokens" checkbox and its hint ("Store external authentication tokens for
   reuse"); leave the checkbox cleared.
2. Wait 1500 ms, `ClearHighlightsAsync()`, then click "Save" (`button.save`) with `ClickAndWaitForNavigationAsync`; run
   the wait with `OffCameraAsync`, its condition being the text "Site settings updated successfully." visible. A plain
   form posts the value of the field on Save, so the field needs no Tab.

Visible when the narration ends: the green notification "Site settings updated successfully.", "CallbackPath" holding
`/signin-x`, and "Store tokens" cleared.

Source: `OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSigninSettings.Edit.cshtml:32-33`;
`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.

## Clip `05-login`

Starts on `/Admin`, opened before the capture. It changes nothing. It needs the four keys and the callback path saved.

### `login-1`

Actions:

1. `NavigateAsync("/Login")`. The page can be opened while signed in as `admin`: it does not redirect
   (`OrchardCore.Modules/OrchardCore.Users/Controllers/AccountController.cs:75`). If it does redirect on the build you use,
   record this clip with a browser context that has no saved sign-in.
2. Highlight the section titled "Use another service to log in" (`h1` with that text, and the button under it) without a caption.

Visible when the narration ends: two columns. On the left, "Log in" with the empty user name and password fields and the
"Log in" button. On the right, "Use another service to log in" with one button, "X (Twitter)". Nothing is typed in the
login form.

Source: `OrchardCore.Modules/OrchardCore.Users/Views/Account/Login.cshtml:27` ("Log in"), `:38` ("Use another service to
log in"), `:43` (the button, whose text is the provider's display name);
`OrchardCore.Modules/OrchardCore.Twitter/Signin/Configuration/TwitterOptionsConfiguration.cs:73` ("X (Twitter)").

### `login-2`

Actions:

1. `MoveToAsync` the "X (Twitter)" button (`button[name="provider"][value="Twitter"]`; if the value differs, the button whose text is
   exactly "X (Twitter)"). Do not click it.
2. `HoldAsync` for the rest of the narration. The cursor rests on the button.

Visible when the narration ends: the same page, with the cursor on the "X (Twitter)" button and no page change.

Never click the button: it would start the sign-in with X.

## Clip `07-workflow`

Starts on `/Admin`, opened before the capture. The clip creates and publishes a workflow, so it changes the site.
Do not publish any content while the workflow is published. Fake values:

| What | Value |
|---|---|
| Name of the workflow | `Announce Blog Posts on X` |
| Text of the post | `Our blog has a post to read: https://example.com/blog` |

The text has no braces and no template markup. Do not open the "Available data" tab of the activity panel.

### `flow-1`

Actions:

1. Click the "Design" group of the admin menu, then the "Workflows" link (`#adminMenu a[href$="/Admin/Workflows/Types"]`) with `ClickAndWaitForNavigationAsync`.
2. Click "Create Workflow" (`a.create`) with `ClickAndWaitForNavigationAsync`.
3. `TypeAsync` into the "Name" field (`GetByLabel("Name", Exact)`) the value `Announce Blog Posts on X` (delay 40).
4. Click "Create and open the designer" (`button.save`) with `ClickAndWaitForNavigationAsync`. Wait until the designer has loaded:
   the section `[data-cy="start-picker"]` is visible.

Visible when the narration ends: the designer with the toolbar showing "Workflows / Announce Blog Posts on X", an empty
canvas, and the section "Start with an event" with its event buttons.

Keep the menu clicks short (pauseAfter 300): the four steps must fit in the narration.

Source: `OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18` ("Design"), `:19` ("Workflows");
`OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowType/Index.cshtml:25` ("Create Workflow");
`OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowType/Create.cshtml:21` ("Name"), `:131` ("Create and open the designer");
`OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:334` ("Start with an event").

### `flow-2`

Actions:

1. Highlight the section `[data-cy="start-picker"]` without a caption, wait 1500 ms, `ClearHighlightsAsync()`.
2. Click `[data-cy="start-picker-ContentPublishedEvent"]` (the button "Content Published Event").

Visible when the narration ends: one activity on the canvas, "Content Published Event", with the green "Start" badge
and an outcome button "Done". The activity panel opens by itself at the bottom of the canvas, because the activity has
settings (`OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:273-275`); leave it open.

Source: `OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/StartPicker.vue:13` (the first event offered);
`OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22` ("Content Published Event");
`OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:174` (the first event becomes the start
activity); `OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:329` ("Start").

### `flow-3`

Actions:

1. In the activity panel (`[data-cy="activity-panel"]`), the setting "Content Types" shows a picker with the placeholder
   "Any content type". Click its button (`[data-cy="activity-panel"] .bootstrap-select button.dropdown-toggle`).
2. In the list that opens (`.dropdown-menu.show`), click the entry "Blog Post". Press Escape to close the list.
3. Click the title of the panel (`[data-cy="panel-activity-title"]`) so that the field loses the focus and the setting is applied.

Visible when the narration ends: the "Content Types" picker showing the tag "Blog Post", and the hint "Select any
content types to filter on.".

Source: `OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Edit.cshtml:5` ("Content Types"),
`:9` (the hint); `OrchardCore.Modules/OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml:16`
("Any content type"); `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:188-189` ("Blog Post").

### `flow-4`

Actions:

1. Close the activity panel (`[data-cy="activity-panel-close"]`) so that the whole activity is visible.
2. Hover the outcome button "Done" of the activity (`button.wfd-port[data-outcome="Done"]`) until a plus is drawn on its
   dot (the plus appears on hover, on an outcome that is not connected:
   `OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/styles/designer.css:1475-1476`), then click it once
   (no drag). The menu "Add the next activity, after Done" opens (`[data-cy="quick-add"]`).
3. `TypeAsync` into its search box (`[data-cy="quick-add-search"]`) the value `Twitter` (delay 70).
4. Highlight the entry "Update X (Twitter) Status Task" with its category "Social"
   (`[data-cy="quick-add"] [role="option"]` with the text "Update X (Twitter) Status Task"), wait 1200 ms,
   `ClearHighlightsAsync()`, then click that entry.

Visible when the narration ends: two activities side by side on the canvas, "Content Published Event" and "Update X
(Twitter) Status Task", joined by a connection from "Done". The header of a card reads the name of the activity
(`ActivityNode.vue:234`); the body of the task card repeats it as "Update X (Twitter) Status"
(`OrchardCore.Modules/OrchardCore.Twitter/Views/Items/UpdateTwitterStatusTask.Fields.Design.cshtml:8`). The task has the
outcomes "Done" and "Failed". The activity panel opens for the task (`App.vue:273-275`). The task shows an issue count
(`[data-cy="issue-badge"]`), because its text is empty.

Source: `OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:333` ("Add the next activity, after {0}");
`OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/OutcomePort.vue:28-30`;
`OrchardCore.Modules/OrchardCore.Twitter/Workflows/Activities/UpdateTwitterStatusTask.cs:31` ("Update X (Twitter) Status Task"),
`:49` ("Done", "Failed"); `OrchardCore.Modules/OrchardCore.Twitter/TwitterLocalizationSources.cs:14` ("Social").

### `flow-5`

Actions:

1. In the panel of the task, the setting "Template for updating X (Twitter) status" is shown. Highlight its label
   (`GetByLabel("Template for updating X (Twitter) status")`) without a caption, wait 1200 ms, `ClearHighlightsAsync()`.
2. `TypeAsync` into that field the value `Our blog has a post to read: https://example.com/blog` (delay 30).
3. Press Tab once so that the field loses the focus and the setting is applied. Wait until the issue count has left
   the task (`[data-cy="issue-badge"]` is no longer visible) and the toolbar says "All changes saved".

Visible when the narration ends: the field holding the text, its hint "The status text of the post.", and no issue
badge on the task. The toolbar shows "All changes saved".

Source: `OrchardCore.Modules/OrchardCore.Twitter/Views/Items/UpdateTwitterStatusTask.Fields.Edit.cshtml:6` (label, required),
`:10` (hint); `OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:418` (the settings of an
activity are checked when it is saved); `OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:166` ("All changes saved").

### `flow-6`

Actions:

1. Close the activity panel (`[data-cy="activity-panel-close"]`).
2. Highlight the two outcome buttons of the task (`button.wfd-port` inside the task, with the texts "Done" and "Failed")
   without a caption. Hold.

Visible when the narration ends: the task with the outcome buttons "Done" and "Failed" highlighted. Both are unconnected.

Source: `OrchardCore.Modules/OrchardCore.Twitter/Workflows/Activities/UpdateTwitterStatusTask.cs:49` ("Done", "Failed"),
`:60-62` (Done when the response is a success, Failed otherwise).

### `flow-7`

Actions:

1. `ClearHighlightsAsync()`. Click "Publish" (`[data-cy="toolbar-publish"]`).
2. Wait until the toast "The workflow was published as version 1." (or "The workflow was published.") is visible.

Visible when the narration ends: the toast, and the toolbar with the badge of the published version. Nothing else is
done: no content is published and the workflow is not run.

Source: `OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:190` ("Publish"), `:221` (the toast);
`OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:757` (the button).

## Opening and closing cards

`intro` and `outro` are cards drawn from the storyboard; nothing is recorded.

## Checks for private data

Before assembling, look at every contact sheet, then OCR the video (one frame per second and at every scene change) and
search it for:

- any text that is not the fake values of this plan: the user is `admin`, the email (if shown anywhere) ends in `@example.com`,
  the four keys start with `example-`, the only address is `https://example.com/blog`;
- the password: it must appear nowhere (the recorder signs in outside the capture; the login form is shown empty);
- a real X account name, a real token, a real domain other than `example.com`, `orchardcore.net`, `docs.orchardcore.net`;
- the address bar or a local path: the recording shows the page only;
- an error page: the settings must save, the login page must show the button, the workflow must publish.
