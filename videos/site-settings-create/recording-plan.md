# Recording plan: Add Site Settings to Your Module

A developer video. Most of its steps are slides drawn by `slides.py` (nothing to record), and one clip is
recorded in a browser: `05-try`. The slides show the code of the **Debugging** group of the Settings module of
Orchard Core, so the recorded site needs no module of its own: the group is part of every Orchard Core site.

Selectors below stand for `scene.Page.Locator(...)`. `file:line` references are in the Orchard Core source
(`src/OrchardCore.Modules/OrchardCore.Settings/...` unless a path says otherwise).

## The site it needs

- **Orchard Core:** a checkout of `OrchardCMS/OrchardCore` that contains the Debugging group (any branch that has
  `OrchardCore.Settings/Drivers/DebugSettingsDisplayDriver.cs`). Run `src/OrchardCore.Cms.Web` on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). The `orchardcore-tester` skill of that repository describes Auto Setup.
- **Provisioning (Auto Setup, so no setup form or sign-in form is recorded).** This is not a video about the Setup
  screen. Environment variables of the web process:

  | Variable (`OrchardCore__AutoSetup__Tenants__0__...`) | Value |
  | --- | --- |
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD`, chosen outside git |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site calls no external service.
- **Sign-in:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json`
  (git-ignored). Never write the password in this folder.
- **Features to enable:** none. `OrchardCore.Settings` is always enabled (`Manifest.cs`, `IsAlwaysEnabled = true`).
- **Settings to start from:** a fresh site has the Debugging group in its default state, with **Write shape debug
  information** unticked (`DebugSettings.cs`: `WriteShapeDebugInformation` is `false` until saved). Check it before every take.
- **External services and stand-ins:** none.
- **Seed data:** none. The site holds only what the Blog recipe creates and the user `admin`.
- **Snapshot:** after the first start and `Recorder setup`, stop the site and copy its `App_Data` folder and
  `state.json` aside. Restore both before every take (see below).

## Order and dependencies

- Only one clip is recorded, so there is no order: `05-try` starts from the snapshot.
- `05-try` **changes the site**: it saves the Debugging group with the box ticked. Before a new take, stop the site,
  restore the snapshot, and start it again (or, with the site running, open the Debugging group, untick the box and
  select **Save**).
- The slide steps are drawn by `assemble.py` from `slides.py`; they need no site.

## Clip `05-try`

**Starts on:** `/Admin` (the dashboard), signed in as `admin`, opened with `scene.OpenAsync("/Admin")` before
`StartCaptureAsync()`.

**Before the capture:** the **Settings** group of the admin menu must be collapsed. The menu renders every group
collapsed and a script restores what was last left open (`src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`,
the comment above `isGroup`). If the Settings button has `aria-expanded="true"`, click it once, off camera.

Locators used in this clip:

| Name | Selector | Where the label comes from |
| --- | --- | --- |
| Settings group | `#adminMenu button.nav-group-label:has(span.title:text-is("Settings"))` | `AdminMenu.cs:36` (`S["Settings"]`); the button is drawn by `TheAdmin/Views/NavigationItem-admin.cshtml` and `NavigationItemLink-admin.cshtml` |
| Debugging entry | `#adminMenu a.item-label[href$="/Admin/Settings/debugging"]` | `AdminMenu.cs:46` (`S["Debugging"]`); the address comes from `Controllers/AdminController.cs:50` (`Settings/{groupId}`) and `DebugSettingsDisplayDriver.cs:14` (`GroupId = "debugging"`) |
| Box | `scene.Page.GetByLabel("Write shape debug information")` | `Views/DebugSettings.Edit.cshtml:7` |
| Hint | `.hint` inside the form, the text starting "Wrap rendered shapes in HTML comments" | `Views/DebugSettings.Edit.cshtml:9` |
| Save button | `button.save` (text **Save**) | `Views/SiteSettings.SaveButton.cshtml:2` |
| Message | `.toast.message-success` | `TheAdmin/Views/Message.cshtml:15`; text from `Controllers/AdminController.cs:119`; it closes by itself 5 seconds after the page loads (see `try-4`) |

`try-1`
- Actions: click the Settings group; wait until the Debugging entry is visible (the group expands with a short
  Bootstrap collapse animation); if the entry is below the fold of the 864 px viewport (many modules add entries under
  Settings), bring it into view first, by scrolling the menu (`scene.Page.Locator("#left-nav")`) or with
  `MoveToAsync` on the entry; then `HighlightAsync` on the Debugging entry with the caption `Debugging`.
- On screen when the narration ends: the admin menu with **Settings** expanded and its entries listed, among them
  **General** and **Debugging**, with a green ring around **Debugging**. The page is still the dashboard.
- Labels: `AdminMenu.cs:36`, `AdminMenu.cs:39` (**General**), `AdminMenu.cs:46` (**Debugging**).

`try-2`
- Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync` on the Debugging entry; wait until the box is
  visible; then `HighlightAsync` on the box with the caption `Write shape debug information`.
- On screen when the narration ends: the Debugging group page, with the page title **Settings** (the breadcrumb of
  `Views/Admin/Index.cshtml:3-5`), the unticked check box **Write shape debug information**, the hint under it, and
  the **Save** button; the green ring is around the check box. No other field is on the page: the only other driver
  that renders for every group is the one that adds the **Save** button (`Drivers/ButtonsSettingsDisplayDriver.cs:12-14`).
- Labels: `Views/DebugSettings.Edit.cshtml:7` and `:9`, `Views/SiteSettings.SaveButton.cshtml:2`,
  `Views/Admin/Index.cshtml:3-5` (breadcrumb title **Settings**).

`try-3`
- Actions: `ClearHighlightsAsync()`; click the box (it becomes ticked) and pause 900 ms; then `HighlightAsync` on
  the Save button with the caption `Save`. Do **not** click **Save** in this step: the message that Save produces
  closes by itself after 5 seconds, so the click belongs to `try-4`, where the narration talks about the message.
- On screen when the narration ends: the Debugging group page with the box ticked and the green ring around the
  **Save** button; nothing has been saved yet.
- Labels: `Views/SiteSettings.SaveButton.cshtml:2`. The button is added to every group by
  `Drivers/ButtonsSettingsDisplayDriver.cs:12-14`, which the narration mentions.

`try-4`
- Actions, in this order: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync` on the Save button (saving
  redirects to the same group, so the click must wait for the navigation: `Controllers/AdminController.cs:122`,
  `RedirectToAction(nameof(Index), new { groupId })`); wait until `.toast.message-success` is visible;
  `HighlightAsync` on it with no caption and `PauseAsync(2000)`; `ClearHighlightsAsync()`; then `HighlightAsync` on
  the ticked box, which stays until the narration ends.
- Timing: the success message is added with a dismissal time of 5000 ms (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-71`),
  `TheAdmin/Views/Message.cshtml:11-20` turns that into an automatic hide, and `TheAdmin/wwwroot/js/notify-messages/NotifyMessages.js`
  removes the element when it is hidden. The message is therefore on screen for about 5 seconds after the page
  loads, and the ring around it must be cleared before that (hence the 2 second pause). The narration of this step
  names the message after "Let's select Save." and the click, so the message is on screen while it is named.
  If a take shows the message gone before the narration names it, add a `PauseAsync` of 1 to 2 seconds before the
  click, and check it again.
- On screen when the narration ends: the Debugging group page with the check box **Write shape debug
  information** ticked and ringed. The message **Site settings updated successfully.** has closed by then; it was
  on screen, ringed, while the narration named it.
- Label: `Controllers/AdminController.cs:119` (`H["Site settings updated successfully."]`).

## The slide steps

Each slide is drawn by `slides.py`; the code is from the Settings module, trimmed with `// ...`.

- `how-1`: three cards: the site settings document of a tenant, its built-in values, and the properties bag.
- `how-2`: three cards: a section (a class), a group (a page under Settings), a display driver.
- `how-3`: four cards: settings class, editor, permission, menu entry.
- `class-1`: `DebugSettings.cs` and two notes (plain properties saved as JSON, a default constructor).
- `class-2`: `DebugSettingsViewModel.cs` and two notes (bound to the form, free to differ).
- `drv-1`: the head of `DebugSettingsDisplayDriver.cs` (`GroupId`, `SettingsGroupId`) and three notes.
- `drv-2`: `EditAsync` of `DebugSettingsDisplayDriver.cs` (`DebugSettingsDisplayDriver.cs:33-47`).
- `drv-3`: `UpdateAsync` of `DebugSettingsDisplayDriver.cs` (`DebugSettingsDisplayDriver.cs:49-70`).
- `drv-4`: a model error from `DefaultSiteSettingsDisplayDriver.cs`, the `ModelState.IsValid` check of
  `src/OrchardCore/OrchardCore.DisplayManagement/Entities/SectionDisplayDriverBase.cs`, and two notes.
- `view-1`: `DebugSettings.Edit.cshtml` and three notes.
- `reg-1`: the registrations of `Startup.cs` (`Startup.cs:37-52`) and two notes.
- `reg-2`: `SettingsPermissions.cs` (`src/OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8,18`), with
  `ManageDebuggingSettings` bright, and one note.
- `reg-3`: `Permissions.cs`, the permission provider.
- `reg-4`: `AdminMenu.cs` (`AdminMenu.cs:16-20` and `:29-56`), the route values and the Debugging entry.
- `use-1`: `ShapeRenderingOptionsConfiguration.cs` and three notes.
- `use-2`: the `RequestUpdate` call of the driver (`DebugSettingsDisplayDriver.cs:62-65`) and the
  `AddSignalOptionsChangeTokenSource` line of `Startup.cs:59`.
- `use-3`: the `PermittedSiteProperties` configuration and two notes (`SitePropertiesLiquidMapper.cs`).
- `use-4`: a recipe with a `Settings` step (`Recipes/SettingsStep.cs:119-121`) and `DeploymentStartup` (`Startup.cs:123-132`).

## Checks for private data

- The capture shows only the admin menu, the Debugging group and a message. The only account is `admin` with
  `admin@example.com`; the site name is **Orchard Core**.
- Search the contact sheet and an OCR of the video for the real password, any email address that is not
  `@example.com`, and any machine or user name (the title bar of the browser is not captured, but check the
  admin user menu).
- No file of this folder holds a password, a sign-in (`state.json`), or a copy of `App_Data`.
