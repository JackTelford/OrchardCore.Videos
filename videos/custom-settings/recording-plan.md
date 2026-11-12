# Recording plan: custom-settings

How to record this video without deciding anything. Everything a person types is fake. In this file a selector stands for
`scene.Page.Locator(...)`. Orchard Core source references (`file:line`) are relative to `src/OrchardCore.Modules/` of
`OrchardCMS/OrchardCore` at the commit the content was written from, unless they start with `src/`.

The video is an admin video: the recording shows the admin UI only. No code, JSON, Liquid or configuration file is shown
on screen, and there is no front-end clip.

## The site it needs

- **Run**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with its own
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup. No setup or sign-in form is
  recorded. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup**: recipe `Blog`, tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`,
  email `admin@example.com`, password chosen outside git and passed in `VIDEO_PASSWORD` (written `<password>` here).
- **Sign-in**: `Recorder setup` once, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`; it saves `state.json`
  (git-ignored). The Setup screen is not part of this video.
- **Features**: the Blog recipe enables everything the clips use: Features, Content Types, Content Fields, Contents,
  Roles, Settings, Deployment and Custom Settings (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the
  `feature` step). The Agency, Blank and Headless recipes enable it as well, which is why the narration of `enable-1`
  says "If it is not enabled on your site". Clip `02-enable` has to enable Custom Settings on camera, so it is
  **disabled in the preparation** below.
- **External services**: none. No mail, SMS, payment or search provider is called.
- **Real data**: none on a fresh site, so no mask script (`VIDEO_INIT_SCRIPT`) is needed.

### Preparation (outside the capture)

Do this once, after Auto Setup and `Recorder setup`, with the browser pane or a `Recorder prepare` method:

1. Disable Custom Settings: open `/Admin/Features`, type `Custom Settings` in the search box, select **Disable** on the
   row of Custom Settings, and answer **Yes** in the dialog titled **Disable Feature**
   (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:44-53`). No other feature depends on it, so the
   dialog lists no dependent feature. Wait for the success message that the feature was disabled.
2. Create the deployment plan that clip `06-deploy` uses: open `/Admin/DeploymentPlan/Index`, select **Add Deployment
   Plan**, enter `Settings Export` in **Name**, select **Create**
   (`OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:25`, `.../Create.cshtml:12-20`). The plan has no steps.
3. Check that there is no content type named `AnnouncementBanner` (`/Admin/ContentTypes/List`) and that the **Editor**
   role has no permission with `Announcement Banner` in its name.
4. Take **snapshot A**: stop the site, copy its `App_Data` folder and `state.json` aside, start the site again.

### The exact fake data

| Where | Field | Value |
|---|---|---|
| Content type | Display Name | `Announcement Banner` (Technical Name `AnnouncementBanner`) |
| Field 1 | Display Name / Field Type | `Banner Text` / Text Field |
| Field 2 | Display Name / Field Type | `Show Banner` / Boolean Field |
| Content type | Creatable, Listable, Draftable, Versionable, Securable | cleared (they are checked on a fresh type) |
| Content type | Stereotype | `CustomSettings`, set in a second save |
| Settings section | Banner Text | `The spring sale starts on Friday.` |
| Settings section | Show Banner | checked |
| Role | Name | `Editor` (created by the Blog recipe) |
| Deployment plan | Name | `Settings Export` (created in the preparation) |

## Order of the clips, and what changes the site

The clips depend on each other and are recorded in the order they play. Slide clips (`01-idea`) are drawn by
`slides.py` and are not recorded.

| Order | Clip | Starts from | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|---|
| 1 | `02-enable` | snapshot A | Enables Custom Settings | A |
| 2 | `03-create` | after `02-enable` | Creates the content type `AnnouncementBanner`, with its two fields, its cleared options and its stereotype | B (taken after `02-enable`) |
| 3 | `04-edit` | after `03-create` | Saves the values of the section | C (taken after `03-create`) |
| 4 | `05-permission` | after `04-edit` | Grants the new permission to the Editor role | D (taken after `04-edit`) |
| 5 | `06-deploy` | after `05-permission` | Adds a step to the deployment plan | E (taken after `05-permission`) |

Take a snapshot after each clip. A full take restores snapshot A and records the five clips in order. To record one clip
again, restore the snapshot taken before it.

Every clip is opened on `/Admin` before the capture starts (`scene.OpenAsync("/Admin")`), so the first click of each
clip is a click in the admin menu. The viewport is 1536x864 CSS pixels as the specification says.

## Clips

### 02-enable: chapter 02, Enable the feature

Starts on: `/Admin`, the admin home page, with the admin menu groups collapsed.

`enable-1`
- Click **Tools** in the admin menu, then click **Features** (`ClickAndWaitForNavigationAsync`). Source:
  `OrchardCore.Features/AdminMenu.cs:26-27`.
- On the Features page, click the search box (placeholder **Search**, `#search-box`) and type `Custom Settings`
  (`TypeAsync`). Source: `OrchardCore.Features/Views/Admin/Features.cshtml:27`.
- Wait until the list shows only the row of **Custom Settings** (the category heading above it is **Settings**).
- On screen when the narration ends: the Features page, the search box holding `Custom Settings`, and one row, Custom
  Settings, whose status is **Disabled** and whose button is **Enable**.

`enable-2`
- Click the **Enable** button of the Custom Settings row (`a[id='btn-enable-OrchardCore_CustomSettings']`; the id is
  `btn-enable-` plus the feature id `OrchardCore.CustomSettings` with the dot changed to an underscore, check it with
  `Recorder probe`) with `ClickAndWaitForNavigationAsync`. The
  tenant restarts after the click: wait until the page `Features` is shown again with the message.
  Source: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:38-41`.
- On screen when the narration ends: the Features page with the success message **The feature Custom Settings was
  enabled.** (`OrchardCore.Features/Controllers/AdminController.cs:317`). If the message disappears before the narration
  ends, see `OPEN-QUESTIONS.md`.

### 03-create: chapter 03, Create the content type

Starts on: `/Admin`, with the admin menu groups collapsed.

`type-1`
- Click **Design**, then **Content Definition**, then **Content Types** (`ClickAndWaitForNavigationAsync` on the last
  one). Source: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`.
- On the Content Types page, click **Create new type** (`#btnCreate`, `ClickAndWaitForNavigationAsync`). Source:
  `OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`.
- If these four clicks take longer than the narration, expand **Design** and **Content Definition** before
  `StartCaptureAsync` and click only **Content Types** and **Create new type** on camera.
- On screen when the narration ends: the **New Content Type** page with the empty **Display Name** and **Technical
  Name** boxes and the **Create** button.

`type-2`
- Type `Announcement Banner` in **Display Name** (`TypeAsync`). The **Technical Name** box fills in as `AnnouncementBanner`
  while typing and when the box loses focus (`OrchardCore.ContentTypes/Views/Admin/Create.cshtml:13-27`). Wait until
  the Technical Name shows `AnnouncementBanner`.
- Move the cursor over the **Technical Name** box (`MoveToAsync`) for about one second so the viewer sees the value.
- Click **Create** (`ClickAndWaitForNavigationAsync`) at the end of the step's actions, so the next page loads before
  `type-3` begins. Source: `.../Create.cshtml:30`.
- On screen when the narration ends: the **Add Parts** page with the message **The "Announcement Banner" content type
  has been created.** (`OrchardCore.ContentTypes/Controllers/AdminController.cs:162-164`).

`type-3`
- Hold for about two seconds on the Add Parts page (text **Choose the Parts to add to this Content Type.**), then click
  **Cancel** (`ClickAndWaitForNavigationAsync`). Source: `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18,48`.
  Do not select any part.
- On screen when the narration ends: the **Edit Content Type - Announcement Banner** page (the type's editor).

`fields-1`
- Scroll down until the **Fields** heading is visible. With `HighlightAsync` ring the empty list under **Fields** for
  about two seconds, then `ClearHighlightsAsync`. Then click **Add Field** (`ClickAndWaitForNavigationAsync`). Source:
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:88,137`.
- On screen when the narration ends: the page **Add New Field To** followed by the name of the type's own part in quotes
  (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:20`; see `OPEN-QUESTIONS.md` for the exact title), with the
  boxes **Display Name** and **Technical Name** and the **Field Type** list.

`fields-2`
- Type `Banner Text` in **Display Name** (`TypeAsync`); wait until **Technical Name** shows `BannerText`.
- Click the radio button labeled **Text Field** in the **Field Type** list: `input[type='radio'][value='TextField']`.
  Do not locate it by the words "Text Field", because the label **Multi Text Field** contains them. The labels are the
  field type names split into words (`.../AddFieldTo.cshtml:62`) and the value of each radio button is the field type
  name (`.../AddFieldTo.cshtml:61`). The first radio button (checked by default, the first name in alphabetical order)
  is not Text Field, so click it explicitly.
- Click **Save**: `button[name='submit.Save'][value='Save']` (`.../AddFieldTo.cshtml:71`,
  `ClickAndWaitForNavigationAsync`). Do not use **Save & Configure**, which has the value `SaveAndConfigure`
  (`.../AddFieldTo.cshtml:72`).
- On screen when the narration ends: the type's editor again, with **Banner Text** in the list under **Fields** and the
  message **The field "Banner Text" has been added.** (`OrchardCore.ContentTypes/Controllers/AdminController.cs:768`).

`fields-3`
- Click **Add Field** (`ClickAndWaitForNavigationAsync`), type `Show Banner` in **Display Name**, wait until **Technical
  Name** shows `ShowBanner`, click the radio button labeled **Boolean Field**
  (`input[type='radio'][value='BooleanField']`; it is the one checked by default, and clicking it again is harmless),
  and click **Save** (`button[name='submit.Save'][value='Save']`, `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the type's editor with **Banner Text** and **Show Banner** under **Fields**, and the
  message **The field "Show Banner" has been added.**

`opts-1`
- Scroll up until the checkboxes **Creatable**, **Listable**, **Draftable**, **Versionable** and **Securable** are
  visible (they sit between **Description** and **Stereotype**). They are checked on a type that was just created
  (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:83-91`; labels in
  `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:11-70`). Do not set the stereotype yet.
- Click each of the five checkboxes once to clear it, top to bottom (`ClickAsync`, `pauseAfter: 500`). Allow about one
  second per click and make the actions end before the narration does.
- On screen when the narration ends: the five checkboxes, all cleared, and the **Stereotype** box still empty.

`opts-2`
- Click **Save** at the bottom of the page (`button.btn-success.save[value='Save']`, label **Save**,
  `ClickAndWaitForNavigationAsync`). Do not open its drop-down: the item inside it, **and continue**, is
  `button.save-continue`. Source: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:215-221`.
- On screen when the narration ends: the **Content Types** list with the message **Content type updated successfully.**
  (`OrchardCore.ContentTypes/Controllers/AdminController.cs:225-230`) and, further down or up the list, the row
  **Announcement Banner** without a **Create New Announcement Banner** button.

`stereo-1`
- In the row **Announcement Banner**, click **Edit**: `a[role='btn-edit-AnnouncementBanner']`
  (`ClickAndWaitForNavigationAsync`; every row has an **Edit** button, so do not locate it by its label alone). Source:
  `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`. The five checkboxes are shown
  because the type has no stereotype yet.
- Click the **Stereotype** box (label **Stereotype**, `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:77-79`)
  and type `CustomSettings` (`TypeAsync`). If the browser draws a suggestion list, leave it; the typed text must be
  exactly `CustomSettings`. Ring the box with `HighlightAsync`.
- On screen when the narration ends: the editor of Announcement Banner with `CustomSettings` in the **Stereotype** box,
  ringed.

`stereo-2`
- `ClearHighlightsAsync`, then click **Save** at the bottom of the page (`button.btn-success.save[value='Save']`,
  `ClickAndWaitForNavigationAsync`). Do not open its drop-down.
- On screen when the narration ends: the **Content Types** list with the message **Content type updated successfully.**

### 04-edit: chapter 04, Edit the settings

Starts on: `/Admin`, with the admin menu groups collapsed.

`edit-1`
- Click **Settings** in the admin menu. The group opens and lists **General**, **Debugging**, other sections that the
  enabled features add, and **Announcement Banner**. Source: `OrchardCore.Settings/AdminMenu.cs:36-45`,
  `OrchardCore.CustomSettings/AdminMenu.cs:44-52`.
- Ring the **Announcement Banner** item with `HighlightAsync` (caption: none) and hold.
- On screen when the narration ends: the open Settings group with **Announcement Banner** ringed.

`edit-2`
- `ClearHighlightsAsync`, click **Announcement Banner** in the menu (`ClickAndWaitForNavigationAsync`). The page is
  titled **Settings** and shows the boxes **Banner Text** and **Show Banner**.
- Type `The spring sale starts on Friday.` in **Banner Text** (`TypeAsync`), press Tab, click the **Show Banner**
  checkbox, and click **Save** (`primaryAction`, label **Save**, `ClickAndWaitForNavigationAsync`). Source:
  `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`.
- On screen when the narration ends: the Settings page for the section, reloaded, with the saved values in the two
  fields.

`edit-3`
- Ring the success message **Site settings updated successfully.** (`OrchardCore.Settings/Controllers/AdminController.cs:119`)
  with `HighlightAsync` for about three seconds, `ClearHighlightsAsync`, then ring **Banner Text** and **Show Banner**
  together or one after the other.
- On screen when the narration ends: the page with the message visible (see `OPEN-QUESTIONS.md` if it fades) and the
  saved text `The spring sale starts on Friday.` with **Show Banner** checked.

### 05-permission: chapter 05, Control who can edit it

Starts on: `/Admin`, with the admin menu groups collapsed.

`perm-1`
- Do not click anything: the narration explains the permission while the admin home page stays on screen.
- On screen when the narration ends: the admin home page, with the admin menu groups collapsed.

`perm-2`
- Click **Access Control** in the admin menu so its group opens and **Roles** is visible. Source:
  `OrchardCore.Roles/AdminMenu.cs:18-19`. If this takes longer than the narration, expand the group before
  `StartCaptureAsync`.
- Click **Roles** (`ClickAndWaitForNavigationAsync`). In the row of **Editor**, click **Edit**
  (`ClickAndWaitForNavigationAsync`). Source: `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`.
- On screen when the narration ends: the page **Edit 'Editor' Role** with the box **Search permissions** and the
  permission groups.

`perm-3`
- Type `Announcement Banner` in the box **Search permissions** (`TypeAsync`). Source:
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`.
- Wait until only one group is shown: the group titled **Custom Settings**, with `(OrchardCore.CustomSettings)` beside
  its title, and one row, **Manage Custom Settings - Announcement Banner**
  (`OrchardCore.CustomSettings/Permissions.cs:11-13,38`; the group title comes from
  `OrchardCore.Roles/Controllers/AdminController.cs:412-425`). Ring the row with `HighlightAsync`. Only one group
  matches because **Securable** was cleared in `opts-1`: a securable type adds its own content permissions, whose
  descriptions contain the display name too (`OrchardCore.Contents/Security/ContentTypePermissions.cs:18-20`). If a
  second group shows, the type is still securable: restore the snapshot taken before `03-create`.
- On screen when the narration ends: that row, ringed, with its **Allow** box still unchecked.

`perm-4`
- `ClearHighlightsAsync`, click the **Allow** checkbox in that row (`input[id='Checkbox.ManageCustomSettings_AnnouncementBanner']`),
  then click **Save** (`button.save[value='Save'][name='submit']`, `ClickAndWaitForNavigationAsync`; the item inside its
  drop-down, **and continue**, is `button.save-continue`). Source: `OrchardCore.Roles/Views/Admin/Edit.cshtml:33`.
- On screen when the narration ends: the Roles list with the message **Role updated successfully.**
  (`OrchardCore.Roles/Controllers/AdminController.cs:213`).

### 06-deploy: chapter 06, Move settings to another site

Starts on: `/Admin`, with the admin menu groups collapsed.

`deploy-1`
- Click **Tools**, then **Deployments**, then **Plans** (`ClickAndWaitForNavigationAsync` on the last one). Source:
  `OrchardCore.Deployment/AdminMenu.cs:18-20`.
- In the row of the plan `Settings Export`, click **Manage Steps** (`ClickAndWaitForNavigationAsync`). Source:
  `OrchardCore.Deployment/Views/DeploymentPlanEntry.Buttons.SummaryAdmin.cshtml:6`.
- If these clicks take longer than the narration, expand **Tools** and **Deployments** before `StartCaptureAsync`.
- On screen when the narration ends: the page titled **Deployment Plan** (it is the plan `Settings Export`, but the
  page does not show the plan's name), with the buttons **Execute** and **Add Step** and the text **Nothing here! There
  are no steps on this plan for the moment.** (`OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:12,23-31,57`).

`deploy-2`
- Click **Add Step**. The dialog **Available Steps** opens. Click its **Filter** box and type `Custom Settings`
  (`TypeAsync`). Wait until the card **Custom Settings** (text **Exports all or specified custom settings.**) is the only
  card shown. Click **Add** in that card (`ClickAndWaitForNavigationAsync`): scope the locator to the card,
  `.deployment-step-item` filtered by the heading `h4.card-title` with the exact text `Custom Settings`, then its
  `a` labeled **Add**, because the filtered-out cards stay in the page and every card has an **Add** button. Source:
  `OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:68,75`,
  `OrchardCore.Deployment/Views/DeploymentStep.Thumbnail.cshtml:8`,
  `OrchardCore.CustomSettings/Views/Items/CustomSettingsDeploymentStep.Fields.Thumbnail.cshtml:3-4`.
- On screen when the narration ends: the page titled **Custom Settings** (the step's editor) with the checkbox **Include
  all custom settings.** checked and a **Create** button.

`deploy-3`
- Ring the checkbox **Include all custom settings.** with `HighlightAsync` for about three seconds. Do not change it.
  Source: `OrchardCore.CustomSettings/Views/Items/CustomSettingsDeploymentStep.Fields.Edit.cshtml:13`; checked by
  default: `OrchardCore.CustomSettings/Deployment/CustomSettingsDeploymentStep.cs:17`.
- `ClearHighlightsAsync`, then click **Create** (`OrchardCore.Deployment/Views/Step/Create.cshtml:53`,
  `ClickAndWaitForNavigationAsync`) at the end of the actions.
- On screen when the narration ends: the page titled **Deployment Plan** with the message **Deployment plan step added
  successfully.** (`OrchardCore.Deployment/Controllers/StepController.cs:112`).

`deploy-4`
- Ring the step's row (title **Custom Settings**) and its green badge **All** with `HighlightAsync`
  (`OrchardCore.CustomSettings/Views/Items/CustomSettingsDeploymentStep.Fields.Summary.cshtml:8-12`).
- On screen when the narration ends: the step **Custom Settings** with the badge **All**, ringed.

## Slides

Slides are drawn by `slides.py`; nothing is recorded for them.

- `idea-1`: three cards, "Site-wide values", "Sections in the Settings menu", "Sections of your own".
- `idea-2`: three cards, "A content type", "The CustomSettings stereotype", "A section in the Settings menu".
- `idea-3`: three cards, "Fields become the form", "Saved with the site settings", "One set of values per section".
- `idea-4`: two cards, "A content type of its own" and "Not a type with content items".

## Checks for private data

- The site is a fresh Auto Setup site: the only user is `admin` with `admin@example.com`; no real name, email address,
  phone number, token or password exists on it. The password is never typed on camera (the sign-in is in `state.json`).
- Search the contact sheets and an OCR pass of the video for anything that is not fake: only `Orchard Core`,
  `admin@example.com`, `Announcement Banner`, `Banner Text`, `Show Banner`, `Settings Export` and the sample sentence
  should appear. No address bar is recorded.
- Do not commit `state.json`, a copy of `App_Data`, a mask script, an MP4 or an audio file.
