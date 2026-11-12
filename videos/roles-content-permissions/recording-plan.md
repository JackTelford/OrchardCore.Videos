# Recording plan: Content Permissions: By Content Type and by Owner

This plan is for the person who records the video. It follows `storyboard.json` step by step. File references are to the
Orchard Core source (`src/` of `OrchardCMS/OrchardCore`, commit `92db4a83`). In this plan a selector such as
`#search-box` stands for `scene.Page.Locator("#search-box")`. A key press is Playwright's locator method, for example
`scene.Page.Locator("#search-box").PressAsync("Escape")`; the scene API (`Scene.cs`) has no key helper of its own.

The video has three recorded clips by the admin user (`02-securable`, `03-role`, `04-editor`), one recorded clip by a
second user (`05-writer`), and two slide chapters (`01-concepts`, `06-recap`). Chapter cards and the opening and closing
cards need no recording.

## 1. The site it needs

- **A fresh site**, provisioned by Auto Setup as `references/recording.md` describes: tenant `Default`, recipe `Blog`,
  site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>`
  (the value of `VIDEO_PASSWORD`, chosen outside git). Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a
  dead address. The video is not about Setup, so Auto Setup is used and no setup screen is recorded.
- **Features**: none to enable. The Blog recipe enables Roles, Contents, Content Types, Users and Admin, which are the
  only features the video uses.
- **Settings**: none to change. Leave the admin list layout, the registration settings (users do not have to confirm
  their email) and the admin theme at their defaults.
- **External services**: none.
- **Locale and look**: the recorder's Chromium (1536x864, light, `en-US`). The admin menu is not used in this video,
  every clip opens its page by address before the capture starts.

### Fake data

| What | Value |
| --- | --- |
| Administrator | user name `admin`, email `admin@example.com` |
| Article made by the recipe | `About` (owner: the admin user), already in the site |
| Article to create in the seed | `Press Kit` (title only, published, owner: the admin user) |
| Role to create on camera | `Writer`, description `Writes and publishes articles.` |
| Second user (created after `03-role`) | user name `jamie.chen`, email `jamie.chen@example.com`, password `<password>` (the same value as the administrator's), role `Writer` |
| Article to create on camera | `Team Offsite Notes` (title only, published, owner: `jamie.chen`) |

Nothing else is created. No real name, address or number appears anywhere.

### Seed the site (outside the capture)

1. Run `Recorder setup` with `VIDEO_USER_NAME=admin`. This writes `state.json` (git-ignored).
2. Open `/Admin/Contents/ContentItems/Article`. The list holds one article, `About`. Choose **New Article**
   (`ContentsAdminListCreate.cshtml:14`), enter the title `Press Kit`, and choose **Publish**. The list now holds
   `Press Kit` and `About`.
3. Check the defaults the narration relies on. Stop and write down what differs (see `OPEN-QUESTIONS.md`) if any is false.
   - Open `/Admin/Roles`, then **Edit** for `Anonymous`, type `View all content` in **Search permissions**: the **Allow**
     checkbox (`[id="Checkbox.ViewContent"]`) is ticked. Do the same for `Authenticated`.
   - Open **Edit** for `Editor`, search `content for others`: **Allow** is ticked for `Edit content for others`,
     `Publish or unpublish content for others` and `Delete content for others`.
   - Open `/Admin/ContentTypes/Edit/Article`: **Securable** is not ticked. Do not save anything.
4. Stop the site and copy its `App_Data` folder and `state.json` aside as **snapshot S0**.

## 2. Order, dependencies and what changes the site

Record in this order. Each clip starts from the state the previous one left.

| Order | Clip | Starts from | Changes the site | Before a new take |
| --- | --- | --- | --- | --- |
| 1 | `02-securable` | S0 | Makes `Article` Securable | Restore S0 |
| 2 | `03-role` | S1 (after `02-securable`) | Creates the role `Writer` with three permissions | Restore S1 |
| 3 | `04-editor` | S2 (after `03-role`) | Nothing (it only reads the Editor role) | None |
| 4 | the writer's user and sign-in (section 3) | S2 | Creates the user `jamie.chen` | Restore S2 |
| 5 | `05-writer` | S3 (after section 3) | Creates the article `Team Offsite Notes` | Restore S3 |

- `03-role` depends on `02-securable`: the **Article Content Type - Article** group exists only after `Article` is
  Securable.
- `04-editor` depends on `02-securable` for the same reason, and can be recorded at any time after it.
- `05-writer` depends on `03-role` (the role `Writer` and its permissions), on the user `jamie.chen`, and on `Press Kit`.
- Take a snapshot (copy `App_Data` and `state.json`) after each step that changes the site: **S1** after `02-securable`, **S2** after
  `03-role`, **S3** after section 3 (it also holds `state-jamie.json`). S0 is the seeded site.
- Slide steps are drawn from `slides.py`; there is nothing to record for them.

## 3. Create the writer's user and sign-in (outside the capture, after `04-editor`)

1. As `admin`, open `/Admin/Users` and choose **Add User**
   (`OrchardCore.Modules/OrchardCore.Users/Views/UsersAdminListCreate.cshtml:7`).
2. Enter **User name** `jamie.chen` (`UserName.Edit.cshtml:4`), **Email** `jamie.chen@example.com`
   (`UserEmail.Edit.cshtml:4`), **Password** and **Password confirmation** `<password>` (`UserFields.Edit.cshtml:12`,
   `:23`). Under **Roles** tick `Writer` (`UserRoleFields.Edit.cshtml:13`). Choose **Save**
   (`UserSaveButtons.Edit.cshtml:2`).
3. Copy `state.json` to `state-admin.json`. Run `Recorder setup` with `VIDEO_USER_NAME=jamie.chen` and the same
   `VIDEO_PASSWORD`. Rename the new `state.json` to `state-jamie.json`, then copy `state-admin.json` back to
   `state.json`. All three files match `state*.json` in `.gitignore`; never commit them.
4. In `Scenes.cs`, the method of `05-writer` creates its scene with the path of `state-jamie.json`
   (`Path.Combine(Root, "state-jamie.json")`) instead of `StatePath`; the other clips keep `state.json`.
5. Take **snapshot S3** (`App_Data`, `state.json`, `state-jamie.json`). Create `state-jamie.json` only once the role
   `Writer` is final: if `03-role` is recorded again, restore S1, record it, and run steps 1 to 5 again, so that the sign-in
   file is made after the role's permissions are saved.

## 4. The clips

### `01-concepts` (slides, no recording)

- `concepts-1`: two cards, "Contents: every content type" and "One content type: that type only" (its text says the type
  must be Securable), both bright.
- `concepts-2`: four cards, "View and preview", "Edit and clone", "Publish or unpublish, and delete" and "List, and edit
  the owner".
- `concepts-3`: three cards, "Edit Article" (the articles you own), "Edit Article for others" (the articles that belong
  to someone else) and "The owner of an item" (the user who created it).
- `concepts-4`: three cards, "All content includes each type", "Publish includes edit" and "Edit for others includes
  more".

### `02-securable`

Start page: `/Admin/ContentTypes/List`, signed in as `admin` (`state.json`). Open it before `StartCaptureAsync`. The page
is **Content Types**, reached in the menu under **Design**, **Content Definition**
(`OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:22-26`); the menu itself is not shown.

- `securable-1`: type `Article` into `#search-box` (`TypeAsync`, delay 90). Wait until the list shows only the row
  **Article** (its technical name `Article` as a badge). Move the cursor to `a[role="btn-edit-Article"]` (**Edit**) and
  highlight the row with `HighlightAsync` on `.list-group-item:has(a[role="btn-edit-Article"])` (in the grid layout,
  `tr:has(a[role="btn-edit-Article"])`). On screen at the end: the **Content Types** page, one row, **Article**, ringed,
  the cursor on **Edit**. Labels: `ContentTypes/Views/Admin/List.cshtml:15`,
  `ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`.
- `securable-2`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(a[role="btn-edit-Article"])`. Wait until
  the title **Edit Content Type - Article** shows (`ContentTypes/Views/Admin/Edit.cshtml:63`). Click the label
  `label:text-is("Securable")` (`ContentTypes/Views/ContentTypeSettings.Edit.cshtml:69`) so the checkbox becomes
  ticked, and highlight `.form-check:has(label:text-is("Securable"))`. If the checkbox is below the fold, scroll first,
  and clear the highlights before scrolling. On screen at the end: the settings block with **Creatable**, **Listable**,
  **Draftable** and **Versionable** already ticked, **Securable** ticked and ringed, and its hint "Determines if this
  content type can have custom permissions."
- `securable-3`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(button.save)` (**Save**,
  `Edit.cshtml:215`). Wait until the **Content Types** list is back (`#btnCreate` is shown). Highlight `#btnCreate`
  (**Create new type**, `List.cshtml:20`). On screen at the end: the **Content Types** list, with the **Create new type**
  button ringed. The "Content type updated successfully." notice (`ContentTypes/Controllers/AdminController.cs:225`) is
  part of the page when it is still shown; the narration does not depend on it.

### `03-role`

Start page: `/Admin/Roles`, signed in as `admin`, after `02-securable`. The list holds `Administrator`, `Anonymous`,
`Authenticated`, `Author`, `Contributor`, `Editor` and `Moderator`. The menu path is **Access Control**, **Roles**
(`OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`).

- `role-1`: move to `a.create` (**Add Role**, `Roles/Views/Admin/Index.cshtml:31`), click it with
  `ClickAndWaitForNavigationAsync`. Wait until `#RoleName` is shown. On screen at the end: the **Create Role** page,
  empty **Name** and **Description** fields, and the **Create**, **Save & Configure** and **Cancel** buttons.
- `role-2`: `TypeAsync(#RoleName, "Writer")`, then `TypeAsync(#RoleDescription, "Writes and publishes articles.")`, then
  `ClickAndWaitForNavigationAsync(button.save-configure)` (**Save & Configure**, `Create.cshtml:32`). Wait until the title
  **Edit 'Writer' Role** shows (`Edit.cshtml:6`). On screen at the end: the **Edit 'Writer' Role** page, the
  **Description** field holding `Writes and publishes articles.`, and the Allow/Effective explanation box under it.
  Labels: `Create.cshtml:15`, `:23`.
- `role-3`: `HighlightAsync(div.bg-info-subtle)`, the box that explains **Allow** ("Permission is granted explicitly.") and
  **Effective** ("Permission is implied by a higher level permission, or explicitly granted.",
  `Roles/Views/Admin/_RolePermissions.cshtml:40-43`). On screen at the end: that box ringed, the **Search permissions** box
  under it.
- `role-4`: `ClearHighlightsAsync`, `TypeAsync(#search-box, "admin panel")` (placeholder **Search permissions**,
  `_RolePermissions.cshtml:49`). Wait until the only row left is **Access admin panel**
  (`tr[data-text="Access admin panel"]`, label from `OrchardCore.Admin.Abstractions/AdminPermissions.cs:8`). Click its
  **Allow** checkbox `[id="Checkbox.AccessAdminPanel"]`. On screen at the end: the **Admin** group with the row **Access
  admin panel**, **Allow** ticked, **Effective** not ticked.
- `role-5`: `PressAsync(#search-box, "Escape")` (this clears the box and shows every row), then
  `TypeAsync(#search-box, "Article")`. Wait until the only group left is **Article Content Type - Article** (the legend
  `fieldset.permissions-list legend`). Scroll until that legend is near the top of the page area, then highlight the legend.
  On screen at the end: the legend ringed and, under it, the rows, in this order: **Clone Article by others**, **Clone own
  Article**, **Delete Article**, **Delete Article for others**, **Edit Article**, **Edit Article for others**, **Edit the
  owner of a Article content item**, **List Article content items**, **Preview Article by others**, **Preview own
  Article**, **Publish or unpublish Article**, **Publish or unpublish Article for others**, **View Article by others**,
  **View own Article**. Every **Allow** box is empty. Source of the group name and the labels:
  `OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:14-26` and `:155`; the group is built in
  `Roles/Controllers/AdminController.cs:412`. The page may be taller than the viewport: the rows from **Edit Article** to
  **Publish or unpublish Article for others** must be visible.
- `role-6`: `ClearHighlightsAsync`. Click `[id="Checkbox.PublishOwn_Article"]` (row **Publish or unpublish Article**), then
  `[id="Checkbox.ListContent_Article"]` (row **List Article content items**). Highlight both rows
  (`tr[data-text="Publish or unpublish Article"]`, `tr[data-text="List Article content items"]`), then move the cursor to
  the row `tr[data-text="Publish or unpublish Article for others"]` without clicking. On screen at the end: **Allow** ticked
  on exactly those two rows of the group; the "for others" rows visible and empty.
- `role-7`: `ClearHighlightsAsync`. Click `.btn-group[data-action-group="oc-role-save-action"] .dropdown-toggle-split`
  (**Toggle Dropdown**), then click `.save-continue` (**and continue**, `Edit.cshtml:34-38`) inside
  `WaitForNavigationAsync`. Wait until the title **Edit 'Writer' Role** is back and `#search-box` is shown, then
  `TypeAsync(#search-box, "Article")` and scroll as in `role-5`. On screen at the end: the **Article Content Type - Article**
  group with **Allow** ticked on **Publish or unpublish Article** and **List Article content items**, and **Effective**
  ticked on **Edit Article**, **List Article content items**, **Publish or unpublish Article**, **View Article by others**
  and **View own Article** (the last two come from the roles Anonymous and Authenticated: if they are not ticked, see
  `OPEN-QUESTIONS.md`). The notice "Role updated successfully." (`Roles/Controllers/AdminController.cs:213`) may
  show at the top. The **Save** button group remembers the entry that was used last: after **and continue** has been
  picked, a later page load of the same browser profile shows **Save and continue** as the main button and **Save** in the
  menu, and `.save-continue` then submits **Save** (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts:13-24`,
  `:51-57`; the choice is kept in the browser's local storage, `Assets/js/adminPreferences.ts:19-43`). Every clip starts
  from `state.json`, which holds no such choice, so this does not matter unless a take reuses a browser profile; then clear
  the site data of the profile first.
- `role-8`: move the cursor to the **Effective** cell of **Edit Article**
  (`tr[data-text="Edit Article"] td:nth-child(3) .form-check`) and hold about 2.5 seconds with `PauseAsync(2500)`: the
  tooltip reads "This permission is implied by: Publish or unpublish Article" (`_RolePermissions.cshtml:98`). Then move
  to `tr[data-text="View Article by others"] td:nth-child(3) .form-check`; the tooltip reads "This permission is granted
  by role: Anonymous, Authenticated" (`_RolePermissions.cshtml:113`). On screen at the end: that second tooltip, next to
  the row **View Article by others** with **Effective** ticked and **Allow** empty. Keep the first hold under the first
  half of the narration of the step so the second tooltip is shown when the narration ends.

State after this clip: the role `Writer` exists with the permissions **Access admin panel**, **Publish or unpublish
Article** and **List Article content items**. Take snapshot S2 before `04-editor`.

### `04-editor`

Start page: `/Admin/Roles`, signed in as `admin`, after `03-role`.

- `editor-1`: move to the **Edit** button of the `Editor` row (`[data-filter-value="editor"] a.edit`,
  `Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`) and click it with `ClickAndWaitForNavigationAsync`. Wait until
  the title **Edit 'Editor' Role** shows, then `TypeAsync(#search-box, "Article")`. On screen at the end: the
  **Edit 'Editor' Role** page filtered to the **Article Content Type - Article** group.
- `editor-2`: scroll until the legend of the group is near the top, then highlight the **Effective** column of the group
  (`fieldset.permissions-list tbody td:nth-child(3)`, ringing the whole table works too). On screen at the end: the group's
  **Allow** column empty and the **Effective** column ticked on every row except **Edit the owner of a Article content
  item**. (The Editor role is allowed the Contents permissions `Publish or unpublish content for others`, `Edit content for
  others`, `Delete content for others`, `Preview content`, `Clone content` and `List content items`, from
  `OrchardCore.Modules/OrchardCore.Contents/Permissions.cs:53-62`.)
- `editor-3`: `ClearHighlightsAsync`, press **Escape** in `#search-box`, then `TypeAsync(#search-box, "content for others")`.
  Wait until the group **Contents** (with the hint "(OrchardCore.Contents)") shows. Highlight the row
  `tr[data-text="Edit content for others"]`. On screen at the end: the **Contents** group with the rows **Delete content for
  others**, **Edit content for others** and **Publish or unpublish content for others**, **Allow** ticked on each, and the
  row **Edit content for others** ringed. Labels: `OrchardCore.Contents.Core/CommonPermissions.cs:17-23`.

Do not save the Editor role: leave the page with the browser's own navigation or end the clip. Nothing in the site changes.

### `05-writer`

Section 3 must be done first. Create this scene with `state-jamie.json`. Start page:
`/Admin/Contents/ContentItems/Article` (route `Contents/ContentItems/{contentTypeId?}`,
`OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:67`). Open it before `StartCaptureAsync` and wait
until the list is shown. The list holds `Press Kit` and `About`.

- `writer-1`: highlight the **New Article** button (`a.btn.btn-secondary:has-text("New Article")`; the button reads `New
  {type}` when exactly one type can be created, `Contents/Views/ContentsAdminListCreate.cshtml:12-14`), then move the cursor
  over the two rows. On screen at the end: the list of the Article type, signed in as `jamie.chen`. The page opened by
  this address is titled **Article**, with the breadcrumb **Manage Content** above it (`Contents/Views/Admin/List.cshtml:17-25`,
  `:34-41`); it is not titled **Content Items**, which is only the name of the menu entry (`Contents/AdminMenu.cs:68`).
  The **New Article** button is ringed, and the rows `Press Kit` and `About` are visible, each with the author's name beside
  the title (`Contents/Views/ContentsMeta_SummaryAdmin.cshtml:15-18`). Their titles are plain text and have no **Edit**
  button; this is what `writer-4` talks about, so do not ring them here.
- `writer-2`: `ClickAndWaitForNavigationAsync(New Article)`. Wait until the **Title** field (`label` text **Title**,
  `OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml`) is shown. `TypeAsync` the title `Team Offsite Notes`
  into it. Highlight `button.publish` (**Publish**, `Contents/Views/Content.PublishButton.cshtml:19`), then
  `ClickAndWaitForNavigationAsync(button.publish)` to return to the list (the page opened with a return address, so
  publishing returns to the list, `Contents/Controllers/AdminController.cs`, `CreateInternalAsync`). Wait until the row
  `Team Offsite Notes` shows. On screen at the end: the list again, `Team Offsite Notes` in the first row, **Published**.
  Keep the actions under the narration: type fast (delay 40) if the step runs long.
- `writer-3`: `ClearHighlightsAsync`, then highlight the row `Team Offsite Notes` (its title link and its **Edit** button,
  `a.edit`). On screen at the end: that row ringed, showing the title as a link and the **Edit** button
  (`Contents/Views/ContentsTitle_SummaryAdmin.cshtml:8`, `ContentsButtonEdit_SummaryAdmin.cshtml:11-14`).
- `writer-4`: `ClearHighlightsAsync`, then highlight the rows `About` and `Press Kit`. On screen at the end: both rows ringed,
  titles as plain text (not links) and no **Edit** button. A green **View** button may show on the published rows; that is
  expected (`ContentsButtonEdit_SummaryAdmin.cshtml:16-19`).
- `writer-5`: `ClearHighlightsAsync`, move the cursor slowly over the three rows. No new highlight. On screen at the end: the
  three articles listed.

### `06-recap` (slides, no recording)

- `recap-1`: four cards; the first two ("Widen a role to every article", "Widen it to every content type") are bright.
- `recap-2`: the same four cards; the third, "Permissions only add access", is bright.
- `recap-3`: the same four cards; the fourth, "Visitors can read by default", is bright.

## 5. Checks for private data

- Every name on screen is one of the fake values above: `admin`, `jamie.chen`, `jamie.chen@example.com`,
  `admin@example.com`, the role `Writer`, the articles `Press Kit`, `About` and `Team Offsite Notes`. The password is never
  typed on camera: the sign-ins happen outside the capture, and the **Password** fields appear in no clip.
- After recording, OCR the video at one frame per second and at every scene change (ffmpeg `select='gt(scene,0.02)'`), then at 5
  frames per second, and search for real names, real email addresses, phone numbers and tokens, as `SKILL.md` section 6
  describes. The Blog recipe's `Lorem ipsum` text is the only filler text.
- Do not commit `state.json`, `state-admin.json`, `state-jamie.json`, any `App_Data` copy, or any mask script.
