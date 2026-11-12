# Recording plan: Azure Communication Services Email

This plan lets a person record the three browser clips of the video without deciding anything. In this file a selector
stands for `scene.Page.Locator(...)`. Every source reference is `path:line` in the Orchard Core checkout at commit
`92db4a83` (the folder `src/OrchardCore.Modules/` is shortened to `Modules/`, and `src/OrchardCore.Themes/` to `Themes/`; `OrchardCore/` is `src/OrchardCore/`).

The video is an admin video: its three recordings show the admin UI only. Azure Communication Services has no local
emulator, so the recording uses fake values, and **no message is ever sent** (the test page is filled in, and `Send` is
never clicked).

## The site it needs

- A local site: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at commit `92db4a83`, on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). Never record a production site.
- Auto Setup (see the `orchardcore-tester` skill of that repository, `references/autosetup.md`), without a setup form on
  camera. Values: tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database
  provider `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (a password chosen outside git,
  passed in `VIDEO_PASSWORD`; never write it in a file of the repository).
- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`) before
  starting the site, so that nothing can reach a mail provider even if `Send` is clicked by mistake.
- Sign the recorder in once, outside the capture: `Recorder setup` writes `state.json` (git-ignored).
- Starting state of the site (checked on `/Admin/Features` with the search box before the first take): the features
  **Azure Communication Services Email**, **Email**, **SMTP** and **Secrets** are all Disabled. The Blog recipe enables
  none of them (`Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). If SMTP were enabled, the email
  settings would show a third tab and the clips would not match their narration; if Secrets were enabled, the
  `Connection String` field would offer a drop-down (`User-specified value` / `Use a secret`) instead of a plain
  password field.
- External service and its stand-in: none. The Azure resource is replaced by a fake connection string that is typed
  into a password field (the capture shows dots) and is never used, since no message is sent.
- No `PrepareAsync` is needed: there is nothing to seed. The video creates its own state (features enabled, settings
  saved) on camera.

### The exact fake values

| Where | Value |
|---|---|
| `Sender email address` | `noreply@example.com` |
| `Connection String` | `endpoint=https://example-resource.communication.example.com/;accesskey=FAKE-KEY-NOT-A-REAL-KEY` (not a working connection string; shown only as dots) |
| `To` (test page) | `jamie.chen@example.com` |
| `Subject` (test page) | `Test message` |
| `Body` (test page) | `Hello from the test page.` |

## Order of the clips, and what each one changes

The clips depend on each other and are recorded in the order they play: `02-enable`, `03-configure`, `04-test`.

| Clip | Needs | Changes the site? | Snapshot |
|---|---|---|---|
| `02-enable` | A fresh Auto Setup site (snapshot A) | Yes: enables the Email and Azure Communication Services Email features | Take snapshot B after a take you keep |
| `03-configure` | Snapshot B | Yes: saves the Azure settings and the default email provider | Take snapshot C after a take you keep |
| `04-test` | Snapshot C | No: the test form is never submitted | None needed; it can be recorded again at any time |

Snapshot procedure: stop the site (by its process ID, never by image name), copy the `App_Data` folder and `state.json`
aside under the snapshot's name, start the site again. To record a clip again, stop the site, restore the snapshot that
clip needs (A for `02-enable`, B for `03-configure`, C for `04-test`), and start the site again. To record everything again,
restore A.

## General rules for every clip

- Open the first page of the clip before `StartCaptureAsync`, with the left admin menu groups collapsed (the menu
  restores the groups the user last left open: `Themes/TheAdmin/Views/NavigationItem-admin.cshtml`; reload the page
  after collapsing them if needed). Opening a group is part of the story, so it happens inside the steps.
- A group of the left menu (`Tools`, `Testing`, `Settings`, `Communication`) is a `button` that opens its items; an item
  with a page is a link. The menu is `nav#left-nav` (`Themes/TheAdmin/Views/Navigation-admin.cshtml:24`).
- Navigate only with `ClickAndWaitForNavigationAsync`, so the capture keeps running.
- A green success notification is a toast (`.toast.message-success`, `Themes/TheAdmin/Views/Message.cshtml`) in the top
  right corner. `Notifier` success messages hide themselves after 5 seconds (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`),
  so the click that causes one happens late in its step, as each step below says, and the toast is still visible when
  the narration ends. Check this on the contact sheet; if the toast is gone, record the clip again with a longer pause
  before the click.
- Keep the actions of each step shorter than its narration (the step ids below name the narration steps).
- The labels below are the ones in the source; a locator that matches text uses that exact text.

## Clip `02-enable` (chapter 02, "Enable the feature")

Starts on `/Admin` (the admin dashboard of the Blog site), signed in as `admin`, menu groups collapsed.

`enable-1`
- Actions: click the `Tools` group of the left menu (`#left-nav button` with the text `Tools`, `pauseAfter` 600). Click the
  link `Features` under it (`#left-nav a` with the exact text `Features`) with `ClickAndWaitForNavigationAsync`; the page
  `/Admin/Features` loads. Click the search box (`#search-box`, placeholder `Search`) and type
  `Azure Communication Services Email` (`TypeAsync`, delay 30). The list filters while typing.
- On screen when the narration ends: the Features page with the search box filled in and exactly one row left, named
  **Azure Communication Services Email** (the feature id `OrchardCore.Email.Azure` is printed beside the name by the page;
  that is expected), under the heading **Communication**, with the status **Disabled** and a button **Enable**.
- Wait condition: before reading the screen, wait until only one `[data-filter-value]` row is visible (the others carry
  the class `d-none`).
- Labels: `Tools`, `Features`: `Modules/OrchardCore.Features/AdminMenu.cs:26-27`. `Search`: `Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`.
  Search text matches the name, the id and the description of a feature: `Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`
  and `Modules/OrchardCore.Features/Assets/ts/features-index.ts` (the `input` handler). Feature name **Azure Communication Services Email**
  and category **Communication**: `Modules/OrchardCore.Email.Azure/Manifest.cs:4` and `:13`. The heading is the category:
  `Features.cshtml:88`.

`enable-2`
- Actions: pause 2000 ms (the first sentence, "Let's select Enable.", is spoken first). Click the `Enable` button of the
  visible row (`a[id^="btn-enable-"]`, text `Enable`) with `ClickAndWaitForNavigationAsync`. The site enables the feature
  and the Email feature it depends on, restarts the tenant and reloads the page. If the reload takes more than 3 seconds,
  run the wait for the reloaded page inside `OffCameraAsync`, with the condition "the element `.toast.message-success`
  contains the text `was enabled`".
- On screen when the narration ends: the Features page reloaded, with a green toast in the top right corner that reads
  **The feature Azure Communication Services Email was enabled.** (the message names only the feature that was selected, not the Email feature that is enabled with it).
  The toast must be visible (it hides 5 seconds after it appears).
- Labels: `Enable`: `Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. The enable action also enables
  the dependencies (`Force` is true): `Modules/OrchardCore.Features/Controllers/AdminController.cs:247`. The toast text:
  `Modules/OrchardCore.Features/Controllers/AdminController.cs:317` (the singular form, since one feature is passed to the message: the dependencies are enabled but not named). The dependency:
  `Modules/OrchardCore.Email.Azure/Manifest.cs:9-12`.

After this clip: take snapshot B.

## Clip `03-configure` (chapter 03, "Configure the provider")

Starts on `/Admin` (or the Features page), signed in, from snapshot B, menu groups collapsed.

`configure-1`
- Actions: click the `Settings` group of the left menu (`#left-nav button` with the text `Settings`), then the `Communication`
  group (`#left-nav button` with the text `Communication`), then the link `Email` (`#left-nav li.email a`) with
  `ClickAndWaitForNavigationAsync`. The page `/Admin/Settings/email` loads. Move the cursor over the two tabs, then highlight
  the yellow alert with `HighlightAsync` (no caption), and clear the highlight at the end of the step.
- On screen when the narration ends: the page titled **Settings** with a tab row showing **Providers** (active) and
  **Azure Communication Services**, and under the Providers tab a warning alert reading **No email providers are currently
  enabled.**
- Labels: `Settings`, `Communication`, `Email`: `Modules/OrchardCore.Email/AdminMenu.cs:38-40`. Page title `Settings`:
  `Modules/OrchardCore.Settings/Views/Admin/Index.cshtml:4`. Tab **Providers**: `Modules/OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:59`
  (`Content:1#Providers`). Tab **Azure Communication Services**: `Modules/OrchardCore.Email.Azure/Drivers/AzureEmailSettingsDisplayDriver.cs:66`
  (`Content:5#Azure Communication Services`). The tabs appear because two groups exist: `OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:37-40` and `:87-91`.
  The alert: `Modules/OrchardCore.Email/Views/EmailSettings.Edit.cshtml:11`.

`configure-2`
- Actions: click the tab **Azure Communication Services** (`ul.nav-tabs a.nav-link` with the exact text). Pause 3000 ms (the
  narration describes the checkbox first). Click the checkbox label `Enable Azure Provider`. Wait until `#Azure_Provider_Settings`
  has the class `show` (the collapse is open).
- On screen when the narration ends: the Azure Communication Services tab with the checkbox **Enable Azure Provider**
  checked, and two fields below it: **Sender email address** and **Connection String**, both empty.
- Labels: `Enable Azure Provider`, `Sender email address`, `Connection String`: `Modules/OrchardCore.Email.Azure/Views/AzureEmailSettings.Edit.cshtml:10`, `:17`, `:26`.
  The fields appear when the box is checked: the same file, lines 9 and 15.

`configure-3`
- Actions: move the cursor to the hint under **Sender email address** and highlight it for about 3 seconds (`HighlightAsync`,
  no caption), then `ClearHighlightsAsync`. Click the **Sender email address** input and type `noreply@example.com` (delay 30).
- On screen when the narration ends: the input holds `noreply@example.com`, and the hint **The default email address to use as a
  sender, unless the email sender is set.** is visible under it.
- Labels: the hint text and the label: `Modules/OrchardCore.Email.Azure/Views/AzureEmailSettings.Edit.cshtml:17` and `:21`. The provider uses this address only
  when the message has no sender: `Modules/OrchardCore.Email.Azure/Services/AzureEmailProviderBase.cs:122-124`.

`configure-4`
- Actions: click the **Connection String** input (a password field whose placeholder reads **Azure connection string**) and type the fake
  connection string of the table above (delay 25). The capture shows dots.
- On screen when the narration ends: the Connection String field is filled with dots only; the typed text is not readable.
- Labels: `Connection String` and the placeholder `Azure connection string`: `Modules/OrchardCore.Email.Azure/Views/AzureEmailSettings.Edit.cshtml:26` and `:28`.
  Without the Secrets feature the field is a password input: `OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:89-100`.

`configure-5`
- Actions: pause 2500 ms (the narration says both fields are required first). Click the `Save` button (`button.save`) with
  `ClickAndWaitForNavigationAsync`. Wait until `.toast.message-success` is visible.
- On screen when the narration ends: the page `/Admin/Settings/email` reloaded, with a green toast **Site settings updated successfully.**
  in the top right corner (it must still be visible).
- Labels: `Save`: `Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`. The toast: `Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.
  Both fields are required: `Modules/OrchardCore.Email.Azure/Drivers/AzureEmailSettingsDisplayDriver.cs:102` and `:125`
  (the required messages, which do not appear in this take because both fields are filled in).

`configure-6`
- Actions: if the Providers tab is shown after the reload, click the tab **Azure Communication Services** (if it is already
  active, leave it). Move the cursor to the **Connection String** field and highlight it (`HighlightAsync`, no caption).
- On screen when the narration ends: the tab Azure Communication Services with **Enable Azure Provider** checked, **Sender email address**
  holding `noreply@example.com`, and the **Connection String** field empty, its placeholder fully readable: **The value is securely stored.
  Enter a new value to update it, or leave it blank to keep it.** The field is not focused.
- Labels: the placeholder: `OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103`. A blank field keeps the stored value:
  `OrchardCore/OrchardCore.Secrets.Abstractions/SecretInputExtensions.cs:28-33`.

`configure-7`
- Actions: `ClearHighlightsAsync`, then click the tab **Providers**. Highlight the **Default provider** drop-down (`HighlightAsync`, no caption). Do not
  open the list.
- On screen when the narration ends: the Providers tab with **Default provider** showing **Azure Communication Services**.
- Labels: `Default provider`: `Modules/OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`. The option text is the provider display name:
  `Modules/OrchardCore.Email.Azure/Services/AzureEmailProvider.cs:21`. The default is set when none is set:
  `Modules/OrchardCore.Email.Azure/Drivers/AzureEmailSettingsDisplayDriver.cs:140-143`. That the drop-down shows the provider right after the first
  save is not verified on a running site (see `OPEN-QUESTIONS.md`).

After this clip: take snapshot C.

## Clip `04-test` (chapter 04, "Test the provider")

Starts on `/Admin/Settings/email` (the Providers tab), signed in, from snapshot C, menu groups collapsed.

`test-1`
- Actions: click the `Tools` group, then the `Testing` group (`#left-nav button` with the text `Testing`), then the link `Email Test`
  (`#left-nav li.emailtest a`) with `ClickAndWaitForNavigationAsync`. The page `/Admin/Email/Test` loads. Highlight the **Provider**
  drop-down (`HighlightAsync`, no caption).
- On screen when the narration ends: the test form with **Provider** showing **Azure Communication Services** (preselected) and
  the other fields empty.
- Labels: `Tools`, `Testing`, `Email Test`: `Modules/OrchardCore.Email/AdminMenu.cs:27-29`. The form: `Modules/OrchardCore.Email/Views/Admin/Test.cshtml`
  (`Provider` line 20; the first option `Select a provider` line 23). The preselected value is the default provider:
  `Modules/OrchardCore.Email/Controllers/AdminController.cs:56`. Only enabled providers are listed:
  `Modules/OrchardCore.Email/Controllers/AdminController.cs:139-151`.

`test-2`
- Actions: `ClearHighlightsAsync`. Click **To** (`#To`) and type `jamie.chen@example.com` (delay 25). Click **Subject** (`#Subject`) and type
  `Test message`. Click **Body** (`#Body`, the text area) and type `Hello from the test page.`
- On screen when the narration ends: **To**, **Subject** and **Body** filled with the values of the table; the optional fields
  still empty.
- Labels: `Provider`, `To`, `Subject`, `Body` are the required fields (red required marker and `[Required]`):
  `Modules/OrchardCore.Email/Views/Admin/Test.cshtml:20`, `:29`, `:70`, `:78`; `Modules/OrchardCore.Email/ViewModels/EmailTestViewModel.cs:9-28`.

`test-3`
- Actions: move the cursor over the empty inputs **Sender** (`#From`), **Cc** (`#Cc`), **Bcc** (`#Bcc`) and **Reply To** (`#ReplyTo`) in this
  order, 500 ms on each, and end on **Sender** with `HighlightAsync` (no caption).
- On screen when the narration ends: **Sender** highlighted, with its hint **The sender is optional, it is useful when the email author is different
  than the email submitter.** readable.
- Labels: `Sender`, `Cc`, `Bcc`, `Reply To`, and the hint: `Modules/OrchardCore.Email/Views/Admin/Test.cshtml:37`, `:41`, `:46`, `:54`, `:62`.
  An empty sender makes the provider use the sender email address of its settings:
  `Modules/OrchardCore.Email/Controllers/AdminController.cs:117-120` and `Modules/OrchardCore.Email.Azure/Services/AzureEmailProviderBase.cs:122-124`.

`test-4`
- Actions: `ClearHighlightsAsync`, scroll down (`ScrollAsync(0, 400)`) until the **Send** button is in view, move the cursor onto **Send** and
  highlight it (`HighlightAsync`, no caption). **Do not click Send.**
- On screen when the narration ends: the filled test form with the **Send** button highlighted; no success toast and no error message.
- Labels: `Send`: `Modules/OrchardCore.Email/Views/Admin/Test.cshtml:86`. The success message the narration names, **Message sent successfully.**:
  `Modules/OrchardCore.Email/Controllers/AdminController.cs:82`.

## Slides and cards (no recording)

The opening card `intro` and the closing card `outro` are drawn from the storyboard.

- `fit-1` (slide): the heading "Email goes through a provider" and three cards: the Email module, a provider, features that send email.
- `fit-2` (slide): the heading "Two providers" and three cards: Azure Communication Services, Default Azure Communication Services, and when the
  default one is listed.
- `fit-3` (slide): the heading "What you need" and three cards: an Azure resource, a sender email address, a connection string.
- `notes-1` (slide): the heading "Good to know" and three cards, the first one bright: the default provider.
- `notes-2` (slide): the same slide with the second card bright: when delivery fails.
- `notes-3` (slide): the same slide with the third card bright: the Secrets feature.

## Checks for private data

- Every value on screen is fake: the site name `Orchard Core`, the user `admin` (`admin@example.com`), the addresses on `example.com`, and the connection
  string, which is never readable (password field).
- The sign-in is never recorded: `state.json` holds it and stays out of git. No password, no real Azure resource name, no access key, no real
  mailbox, and no real domain other than `example.com` may appear in a frame.
- After recording, look at every contact sheet. Search the frames (OCR, one frame per second and at every scene change) for `@` addresses other
  than `example.com`, for `accesskey` and for `azure.com`.
- Do not commit `state.json`, a snapshot of `App_Data`, a mask script, audio or video files.
