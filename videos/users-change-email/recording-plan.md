# Recording plan: Change Email

How to record this video without deciding anything. Source references (`file:line`) are paths in
`OrchardCMS/OrchardCore` under `src/OrchardCore.Modules/OrchardCore.Users/` unless another path is given. A selector
written as `#search-box` stands for `scene.Page.Locator("#search-box")`.

The video is recorded signed in as the one account the recorder knows: user `admin`. The user whose email address
changes on camera is that account. Nothing in this video needs a second sign-in.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, `main`, run on `http://localhost:5010` |
| Data folder | An empty `App_Data` outside both repositories, set with `ORCHARD_APP_DATA` |
| Setup | Auto Setup, tenant `Default`, no setup screen and no sign-in on camera (the `orchardcore-tester` skill, `references/autosetup.md`, lists the settings) |
| Recipe | `Blog` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Database | `Sqlite` |
| Admin | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in `VIDEO_PASSWORD`) |
| Outbound calls | Blocked with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The module sends no email, so no mail server or stand-in is needed. |
| Sign-in | `Recorder setup` once, outside the capture, saves `state.json` (git-ignored) |

The Blog recipe enables the Users module (`OrchardCore.Users`) but not Users Change Email
(`OrchardCore.Users.ChangeEmail`). The feature must be **disabled** when clip `02-enable` starts, and its setting off.

The pages `/ChangeEmail` and `/ChangeEmailConfirmation` are not admin pages: they are drawn by the site theme (The
Blog Theme with this recipe). Outbound calls are blocked, so that theme's web fonts fall back to the browser's
fonts. That is expected.

### Features to enable before recording

None. Clip `02-enable` enables Users Change Email on camera.

### Settings to save before recording

None. Users Change Email has one setting, `AllowChangeEmail`, and it must be **off** (the default) before clip
`03-setting`. Do not change any setting under Settings, Security, User Login: the field "Allow user to change their
email address" stays off, which is what the first slide describes.

### Fake seed data (create outside the capture, once, before the first take)

The checks of clip `05-checks` need a second user who already holds an address. Create it from the admin, signed in
as `admin`:

1. Open `/Admin/Users/Index` and select **Add User** (`Views/UsersAdminListCreate.cshtml:7`).
2. Fill in **User name** `jamie`, **Email** `jamie.chen@example.com`, **Password** `<password>` (any password that
   passes the rules; use the same value as `VIDEO_PASSWORD` and never write it down), and **Password confirmation**
   with the same value (`Views/UserName.Edit.cshtml:4`, `Views/UserEmail.Edit.cshtml:4`, `Views/UserFields.Edit.cshtml:12-24`).
   Assign no role. Select **Save**.
3. Back on the Users list, the row of `jamie` carries an **Unconfirmed** badge (`Views/UserFields.cshtml:11`). Open the
   menu of that row, choose **Confirm email** (`Views/UserActionsMenu.cshtml:35`) and accept the dialog. The badge
   disappears. The row of `admin` has no badge, and neither row may have one when the take starts.

The Users list then shows exactly two users: `admin` with `admin@example.com`, and `jamie` with
`jamie.chen@example.com`. No other user, content item or setting is created.

## Commands

Run them in Git Bash, from the root of the `OrchardCore.Videos` checkout. `<repo>` is the path of that checkout and
`<bin>` is the folder the recorder builds into (`%TEMP%/ocvideo/bin/Release/net10.0`). The password is chosen outside
git and is never written in a file.

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/users-change-email/recorder
export VIDEO_ROOT=<repo>/videos/users-change-email VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup
```

The video has no `PrepareAsync`: the seed above is done by hand, so `Recorder prepare` is not used. After the seed and
the start snapshot, narrate and record:

```bash
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/users-change-email
<bin>/Recorder record 02-enable
<bin>/Recorder record 03-setting
<bin>/Recorder record 04-change
<bin>/Recorder record 05-checks
```

Run `Recorder setup` again before `04-change` and before `05-checks`, as the next section says.

## Snapshots, order and what each clip changes

After the seed is done, stop the site and copy `App_Data` and `state.json` aside as the **start** snapshot. Restore
that copy before every full take. The clips depend on each other, so record them in this order:

| Order | Clip | Starts from | What it changes |
|---|---|---|---|
| 1 | `02-enable` | The start snapshot: feature disabled, setting off | Enables Users Change Email (the tenant restarts) |
| 2 | `03-setting` | Feature enabled, `AllowChangeEmail` off | Saves `AllowChangeEmail` = on |
| 3 | `04-change` | Setting on, the email of `admin` is `admin@example.com` | The email of `admin` becomes `owner@example.com` |
| 4 | `05-checks` | Setting on, the email of `admin` is `owner@example.com` | Nothing: every submit is refused |

- Copy `App_Data` and `state.json` aside after clips 1, 2 and 3 as well (stop the site first, as for the start
  snapshot), so one clip can be taken again: restore the copy made before that clip. Clips `02-enable`,
  `03-setting` and `04-change` cannot be repeated without a restore, because each one changes the site (`04-change` also needs the field to show `admin@example.com` again). Clip `05-checks`
  can be repeated as it is.
- Run `Recorder setup` again right before `04-change`, and again right after it, before `05-checks`: changing an
  address may end the saved session (see `OPEN-QUESTIONS.md`), and a stale `state.json` opens the sign-in page
  instead of the Users list. The user name stays `admin`, so the same sign-in works after the change.

## Chapters made of slides and cards

| Step | What the slide or card shows |
|---|---|
| `intro` | The opening card: the logo, the title "Users Change Email" and the subtitle. No recording. |
| `why-1` | Slide "The Email field of a user": two cards, "Not editable by default" and "User Login setting". Drawn by `slides.py`. |
| `why-2` | Slide "Users Change Email": four check cards, "User menu", "Change Email page", "The signed-in user", "One site setting". Drawn by `slides.py`. |
| `summary-1` | Slide "Change Email, start to finish": four numbered cards from "Enable the feature" to "Deployment Plans" (the last one marked optional). Drawn by `slides.py`. |
| `outro` | The closing card with `docs.orchardcore.net`. No recording. |

## Clip `02-enable` (chapter 02, Enable the feature)

Starts on `/Admin/Features` (open it before `StartCaptureAsync`). The search box is empty and the full list of features
is shown. The sidebar shows **Tools**, **Features** as the current page; expand it before the capture if it is collapsed.

### `enable-1`

- Actions: `PauseAsync(2800)` so the narrator reaches "Let's search for change email". Then `TypeAsync` into
  `#search-box` the text `change email` (`delay: 70`). Then `PauseAsync(600)`. Then
  `HighlightAsync` the row of the feature with the caption `Users Change Email`; the row is `.list-group-item`
  filtered to the one that holds `#btn-enable-OrchardCore_Users_ChangeEmail`.
- Visible at the end of the narration: the list reduced to one row under the heading **Security**: "Users Change
  Email" with the id `OrchardCore.Users.ChangeEmail`, its description, the badge **Disabled** and the button
  **Enable**, with the green ring around the row.
- Source: search box and category heading `OrchardCore.Features/Views/Admin/Features.cshtml:27,88`; feature name and
  category `Manifest.cs:34-43`; row `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml`.

### `enable-2`

- Actions: `ClearHighlightsAsync()`, then `MoveToAsync` the button `#btn-enable-OrchardCore_Users_ChangeEmail` (a link,
  label **Enable**; the admin scripts turn it into a POST, and it opens no confirmation dialog). Enabling a feature
  restarts the tenant, so the page that follows the click can take several seconds, longer than this narration, and
  `ClickAndWaitForNavigationAsync` waits for it on camera. Cut that wait by putting the click itself inside the
  wrapper, with the cursor already on the button (`button` is that locator):

  ```csharp
  await scene.OffCameraAsync(async () =>
  {
      await scene.ClickAndWaitForNavigationAsync(button);
      await scene.Page.Locator("text=The feature Users Change Email was enabled.").WaitForAsync();
  });
  ```

  The click ripple falls inside the cut; the video jumps from the cursor over **Enable** to the reloaded page.
  Afterwards `HighlightAsync` the text "The feature Users Change Email was enabled.".
- Visible at the end of the narration: the Features page after the reload, scrolled to the top, with the message "The
  feature Users Change Email was enabled." at the top, ringed. The reload clears the search, so the row of the feature
  is back in the long list, below the first screen: the narration talks about the message only, so do not scroll to it.
- Source: label **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the POST action
  and the redirect `OrchardCore.Features/Controllers/AdminController.cs:225-256`; message
  `OrchardCore.Features/Controllers/AdminController.cs:317`.

## Clip `03-setting` (chapter 03, Turn on the setting)

Starts on `/Admin/Settings/userChangeEmail` (open it before `StartCaptureAsync`; the route is
`Settings/{groupId}` with the group `userChangeEmail`, `Drivers/ChangeEmailSettingsDisplayDriver.cs:13`). The sidebar
shows **Settings**, **Security**, **Change Email** as the current item; expand **Settings** and **Security**
before the capture if they are collapsed. The only control is one unchecked box and a **Save** button.

### `setting-1`

- Actions: `PauseAsync(3000)`. Then `HighlightAsync` the sidebar item **Change Email** with the caption `Settings,
  Security, Change Email`. Then `PauseAsync(2500)`, `ClearHighlightsAsync()`, and `HighlightAsync` the checkbox
  `input[type=checkbox][name$="AllowChangeEmail"]` (unchecked).
- Visible at the end of the narration: the settings page with the box **Allow the users to change their email**
  unchecked and ringed, and the **Save** button below it.
- Source: menu path `ChangeEmailAdminMenu.cs:26-34`; label `Views/ChangeEmailSettings.Edit.cshtml:8`; default off
  `Models/ChangeEmailSettings.cs`; the item and the page depend on the setting `Drivers/ChangeEmailUserMenuDisplayDriver.cs:20`
  and `Controllers/ChangeEmailController.cs:39`.

### `setting-2`

- Actions: `ClearHighlightsAsync()`, `ClickAsync` the checkbox `input[type=checkbox][name$="AllowChangeEmail"]`
  (it becomes checked), `PauseAsync(1500)`, then `ClickAndWaitForNavigationAsync` on `button.save` (label **Save**).
  The narration of this step is about 8 seconds: keep the pause so the checked box is on screen first. Saving this
  setting asks for no restart of the tenant: the only release request in the Settings module is in the General
  settings (`OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:140`), and the Change Email driver
  makes none, so the page that follows the click is quick and stays on camera. If a take still shows a long wait, cut
  it with the wrapper of `enable-2`.
- Visible at the end of the narration: the page after the save, with the box checked and the green message "Site
  settings updated successfully."
- Source: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message
  `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `setting-3`

- Actions: `HighlightAsync` the message "Site settings updated successfully.", `PauseAsync(3500)`, then
  `ClearHighlightsAsync()` and `HighlightAsync` the checked box. No navigation.
- Visible at the end of the narration: the page with the message (if it has faded, the checked box ringed is enough:
  the narration then talks about the checkbox) and the checked box.
- Source: as `setting-2`.

## Clip `04-change` (chapter 04, Change an email address)

Run `Recorder setup` first. Starts on `/Admin/Users/Index` (open it before `StartCaptureAsync`). The Users list shows
two rows: `admin` with `admin@example.com`, and `jamie` with `jamie.chen@example.com`; neither has a badge. The
top bar of the admin theme shows the user icon.

### `change-1`

- Actions: `PauseAsync(1500)`, `HighlightAsync` the row of `admin` (the text `admin` with its address), `PauseAsync(2500)`,
  `ClearHighlightsAsync()`, then `HighlightAsync` the user icon `button#navbarDropdown` with the caption `User menu`.
  Do not click it in this step.
- Visible at the end of the narration: the Users list with the user icon in the top bar ringed and captioned.
- Source: the row shows the user name and the address `Views/UserFields.cshtml:7`; the user menu button
  `Views/UserMenu.DetailAdmin.cshtml:7`; its icon `Views/UserMenuItems-Title.DetailAdmin.cshtml`; **Access Control**,
  **Users** `AdminMenu.cs:27-38`.

### `change-2`

- Actions: `ClearHighlightsAsync()`, `ClickAsync` `button#navbarDropdown` (`pauseAfter: 1200`). The menu opens. Then
  `MoveToAsync` the item `a.dropdown-item:has-text("Change Email")`, `PauseAsync(3000)` (the narrator reads the order
  of the items), and finally `ClickAndWaitForNavigationAsync` on that item. The page `/ChangeEmail` opens.
- Visible while the menu is open: the header "Signed in as" with `admin`, then **Profile**, **Change password**,
  **Change Email** (envelope icon) and **Log off**, in that order. Visible at the end of the narration: the Change
  Email page has opened.
- Source: order of the items `Drivers/UserMenuDisplayDriver.cs:37-47,62-63` (Profile 5, Change password 10, Log off
  100) and `Drivers/ChangeEmailUserMenuDisplayDriver.cs:22` (Change Email 20); labels `Views/UserMenuItems-SignedUser.cshtml:3`,
  `Views/UserMenuItems-ChangePassword.DetailAdmin.cshtml:11`, `Views/UserMenuItems-ChangeEmail.cshtml:3`,
  `Views/UserMenuItems-SignOut.cshtml:4`.

### `change-3`

- Actions: `PauseAsync(500)`, `HighlightAsync` the field `#Email` (it holds `admin@example.com`). `PauseAsync(3500)`,
  `ClearHighlightsAsync()`. Then clear the field with `scene.Page.Locator("#Email").FillAsync("")` and `TypeAsync`
  into `#Email` the text `owner@example.com` (`delay: 60`).
- Visible at the end of the narration: the page with the heading **Change Email**, the label **Email**, the field
  holding `owner@example.com`, and the **Submit** button below it.
- Source: `Views/ChangeEmail/Index.cshtml:7,11,12,16`; the field is filled with the current address
  `Controllers/ChangeEmailController.cs:44-46`.

### `change-4`

- Actions: `ClickAndWaitForNavigationAsync` on `form button.btn-primary` (label **Submit**). The page
  `/ChangeEmailConfirmation` opens. `PauseAsync(1500)`, then `HighlightAsync` the text "Thank you for changing your
  email.".
- Visible at the end of the narration: the page with the heading **Email changed** and the text "Thank you for
  changing your email.", ringed. No error message.
- Source: `Views/ChangeEmail/ChangeEmailConfirmation.cshtml:5,8`; the redirect after the save
  `Controllers/ChangeEmailController.cs:76`; the form holds the field **Email** only, no password and no second
  step `ViewModels/ChangeEmailViewModel.cs`.

### `change-5`

- Actions: `ClearHighlightsAsync()`, `NavigateAsync("/Admin/Users/Index")`, `PauseAsync(1500)`, then `HighlightAsync`
  the row of `admin`. No other action: the second sentence of the narration has no screen of its own.
- Visible at the end of the narration: the Users list with the row `admin` showing `owner@example.com` beside the
  user name, ringed, and the row `jamie` unchanged with `jamie.chen@example.com`. No **Unconfirmed** badge on either
  row.
- Source: `Views/UserFields.cshtml:7`; the sign-in field label **Username or email address**
  `Views/LoginFormCredentials.cshtml:7`, and that the user is found by user name or by address
  `Controllers/AccountController.cs:138` and `OrchardCore.Users.Core/Services/UserService.cs:284-290`.

## Clip `05-checks` (chapter 05, What the page checks)

Run `Recorder setup` first (see above). Starts on `/ChangeEmail` (open it before `StartCaptureAsync`): the field
holds `owner@example.com`, the current address of `admin`. No message is shown.

Before the capture, check what kind of field it is (in Git Bash set `MSYS_NO_PATHCONV=1` first): `Recorder probe /ChangeEmail "() => document.querySelector('#Email').type"`.
If it prints `email`, the browser would stop the submit of `owner` itself. In that case add, before
`StartCaptureAsync`, `await scene.Page.EvaluateAsync("document.querySelector('form.no-multisubmit').noValidate = true")`
so that the page answers. If it prints `text`, add nothing.

### `checks-1`

- Actions: `PauseAsync(3500)` (the narrator explains the check), then `ClickAndWaitForNavigationAsync` on
  `form button.btn-primary` (**Submit**) without changing the field. The page answers on the same address `/ChangeEmail`.
- Visible at the end of the narration: the page with the message "This email is already your current one." (it
  appears above the form and under the field), the field still holding `owner@example.com`.
- Source: `Controllers/ChangeEmailController.cs:65`.

### `checks-2`

- Actions: `FillAsync("")` on `#Email`, `TypeAsync` into `#Email` the text `jamie.chen@example.com` (`delay: 50`),
  `PauseAsync(800)`, then `ClickAndWaitForNavigationAsync` on `form button.btn-primary`.
- Visible at the end of the narration: the message "A user with the same email already exists." and the field
  holding `jamie.chen@example.com`.
- Source: `Controllers/ChangeEmailController.cs:69`.

### `checks-3`

- Actions: `FillAsync("")` on `#Email`, `TypeAsync` into `#Email` the text `owner` (no at sign, `delay: 80`),
  `PauseAsync(800)`, then `ClickAndWaitForNavigationAsync` on `form button.btn-primary`.
- Visible at the end of the narration: a validation message under the field and above the form, and the field holding
  `owner`. Its exact words are not in the narration: the source holds two candidates, "Invalid email address."
  (the attribute's own result, `OrchardCore.Infrastructure.Abstractions/Email/EmailAddressAttribute.cs:23`) and
  "Invalid Email." (the text declared on the property, `ViewModels/ChangeEmailViewModel.cs:8`). Read the screen; see
  `OPEN-QUESTIONS.md`. The message appears twice, in the summary above the form and under the field
  (`Views/ChangeEmail/Index.cshtml:9,13`).
- Source: a value without an at sign is invalid `OrchardCore.Infrastructure/Email/EmailAddressValidator.cs:12`;
  the address is not changed unless every check passes `Controllers/ChangeEmailController.cs:58-81`.

## Checks for private data

- Every address on screen ends in `@example.com`: `admin@example.com`, `owner@example.com`, `jamie.chen@example.com`.
  User names are `admin` and `jamie` only.
- The password is never typed on camera: the seed form is filled outside the capture, and the password fields
  (masked) never appear in a clip. Do not record the seed.
- The take starts on a fresh site, so the capture needs no mask script. OCR the finished video at one frame per second
  and at every scene change, then at 5 frames per second, and search for any address that does not end in
  `@example.com`, any host name other than `localhost`, and the password.
- The browser's address bar is not recorded, but `localhost:5010` can appear in a message: it is allowed.
