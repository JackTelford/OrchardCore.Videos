# Recording plan: User Registration

How to record `users-registration` without deciding anything. The narration is in `storyboard.json`; this file says what
is on the site before the first take, in which order the clips are recorded, and what happens in every clip.

Source references use two short names, both relative to the root of the `OrchardCMS/OrchardCore` repository at commit
`92db4a83`:

- `U` is `src/OrchardCore.Modules/OrchardCore.Users`
- `F` is `src/OrchardCore.Modules/OrchardCore.Features`

In this plan a selector stands for `scene.Page.Locator(...)`. A label is quoted as the code spells it, with the file and
line it comes from.

## 1. The site it needs

### The application

- `src/OrchardCore.Cms.Web` of an Orchard Core checkout at commit `92db4a83` or the same code on a later commit of
  `main`, on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`).
- Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls a service on the internet. The only
  outbound connection the story needs is SMTP to the local inbox below, which these two variables do not affect.
- Stop the site before copying its `App_Data` (snapshots, section 2).

### Auto Setup (no setup screen, no login form on camera)

Provision the `Default` tenant with Auto Setup through environment variables, so the first request installs the site:

| Variable | Value |
|---|---|
| `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
| `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
| `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
| `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
| `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
| `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | the value of `VIDEO_PASSWORD` |
| `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
| `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

`VIDEO_PASSWORD` is chosen outside git. The Blog recipe enables `OrchardCore.Users`, `OrchardCore.Features`,
`OrchardCore.Settings` and the `TheAdmin` and `TheBlogTheme` themes, and nothing else this video needs.

### Passwords of the recording

- `VIDEO_PASSWORD`: the password of `admin` (`Recorder setup`, and Auto Setup above). Never written in git.
- `VIDEO_VISITOR_PASSWORD`: the password the three visitors type. Chosen outside git. It has to satisfy the default
  password rules of ASP.NET Core Identity (at least six characters, with a digit, a lower-case letter, an upper-case
  letter and a symbol), because the Registration feature changes none of them. It is typed in password fields, which
  show dots.
- In every step below, "the visitor password" means the value of `VIDEO_VISITOR_PASSWORD`.

### Features to enable before the first take (outside the capture)

- Enable **SMTP Email Provider** at Tools, Features. Its dependency, **Email**, is enabled with it.
- Do **not** enable Users Registration: clip `02-enable` enables it on camera.
- Do not enable any feature of a login provider (Google Authentication, GitHub Authentication, Meta Login), External
  Authentication, Rate Limits or ReCaptcha. The story shows a Registration settings page with three checkboxes and no
  tab strip.

### Settings to save before the first take (outside the capture)

At Settings, Communication, Email (`/Admin/Settings/email`), on the SMTP tab:

| Tab | Field label | Value |
|---|---|---|
| SMTP | Enable SMTP Provider | selected |
| SMTP | Sender email address | `noreply@example.com` |
| SMTP | Delivery method | Network |
| SMTP | Host name | `localhost` |
| SMTP | Port number | `2525` |
| SMTP | Auto Select Encryption method | cleared |
| SMTP | Encryption method to use | the option "None - Connect to server using insecure connection." |
| SMTP | Require credentials | cleared |

Select Save once. The Providers tab has no Default provider drop-down before this save: it shows only the message
"No email providers are currently enabled." (`src/OrchardCore.Modules/OrchardCore.Email/Views/EmailSettings.Edit.cshtml:11`),
because the drop-down lists only enabled providers (`EmailSettingsDisplayDriver.cs:90`) and the SMTP provider is enabled
by this save (`SmtpProviderOptionsConfigurations.cs:33`). The same save makes SMTP the default provider, because it is
the only one (`SmtpSettingsDisplayDriver.cs:193-196`). After the save, open the Providers tab and check that "Default
provider" reads "Simple Mail Transfer Protocol (SMTP)". Then open Tools, Testing, Email Test, send a message to
`admin@example.com`, and check that it reaches the inbox below. Restart the inbox container afterwards, so that the
inbox is empty. Labels: `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml` (the SMTP tab,
`Content:5#SMTP` in `SmtpSettingsDisplayDriver.cs:88`; the option text of the encryption method, line 101),
`src/OrchardCore.Modules/OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20` (Default provider, tab `Providers`,
`EmailSettingsDisplayDriver.cs:59`), and `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Services/SmtpEmailProvider.cs:20`
(the provider's display name).

### External service: a local inbox

smtp4dev in Docker catches the confirmation message. Start it before the first take, and remove it after the last one:

```bash
docker run --rm -d --name ocvideo-inbox -p 3000:80 -p 2525:25 rnwood/smtp4dev
# its web page: http://localhost:3000 (VIDEO_INBOX_URL), its SMTP port: 2525
docker rm -f ocvideo-inbox
```

The image name, the ports and the behaviour of its web page are not verified here: see `OPEN-QUESTIONS.md`.

### Users that exist at the start

Only `admin` (`admin@example.com`). The three visitors register on camera. No other data is seeded.

| Visitor | Username | Email address | Registers in clip |
|---|---|---|---|
| Jamie | `jamie.chen` | `jamie.chen@example.com` | `04-register` |
| Morgan | `morgan.lee` | `morgan.lee@example.com` | `05-verify-visitor` |
| Riley | `riley.park` | `riley.park@example.com` | `06-approve-visitor` |

### Signed in or not

- **Admin clips** (`02-enable`, `03-settings`, `05-verify-settings`, `06-approve-settings`, `06-approve-admin`) use
  `state.json`, saved by `Recorder setup` with the user `admin`.
- **Visitor clips** (`04-register`, `05-verify-visitor`, `06-approve-visitor`) must be anonymous. `NewSceneAsync` loads
  `state.json` whenever it exists, so the admin would be signed in while "registering". Start these three scenes with a
  storage-state path that does not exist, for example `Path.Combine(Root, "anonymous.json")`, which is never created.
  Every scene starts with a new browser context, so each visitor clip starts signed out.

## 2. Order, dependencies and snapshots

Record the clips in this order, which is also the order they play in. Every clip starts from the state the previous one
left.

| # | Clip | Needs | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | the seed above | Enables Users Registration |
| 2 | `03-settings` | 1 | Nothing: the page is never saved |
| 3 | `04-register` | 1 | Creates `jamie.chen`, who stays signed in only in that scene |
| 4 | `05-verify-settings` | 1 | Saves Users must verify their email address |
| 5 | `05-verify-visitor` | 4 | Creates `morgan.lee`, then confirms the address |
| 6 | `06-approve-settings` | 4 | Clears the email option, saves Users must be approved before they can log in |
| 7 | `06-approve-visitor` | 6 | Creates `riley.park`, disabled |
| 8 | `06-approve-admin` | 7 | Enables `riley.park` |

`01-idea` and `07-external` are slides: they need no site. The opening and closing cards need nothing either.

**Snapshots.** Stop the site and copy its `App_Data` folder and `state.json` aside before every clip that changes the
site (clips 1, 3, 4, 5, 6, 7 and 8). To record a clip again, stop the site, restore the snapshot taken before it, and
start the site. Clip 2 changes nothing, so it starts from the snapshot taken before clip 3. After recording again a
clip that changes the site, take the snapshots of the later clips again. For a full take, restore the snapshot taken
before clip 1 (the seed, with Users Registration disabled).

The inbox must hold no message when clip 5 starts: restart the container (`docker rm -f ocvideo-inbox`, then the
`docker run` of section 1) after the seed, and again after clip 4 if it holds any.

## 3. Rules for every clip

- Clear highlights (`scene.ClearHighlightsAsync()`) at the start of a step that scrolls or navigates.
- Navigate only with `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`.
- Keep the actions of a step shorter than its narration. The words of each step are in `storyboard.json`.
- Type with `scene.TypeAsync(locator, text, delay)`. Use `delay: 40` for an email address and `delay: 25` where a step
  fills a whole form.
- After a form is submitted, wait for the condition named in the step before the narration ends.
- Enabling a feature restarts the tenant, so the first page after it can load slowly. If a pause shows, record the
  clip again after opening the pages of the story once.

## 4. The clips

### `01-idea` (slides)

- `idea-1`: the slide "Without registration", with two cards: "Administrators add every account" and "Visitors cannot
  sign up".
- `idea-2`: the slide "With Users Registration", with three cards: "A Register page", "A link on the login page" and
  "A settings page".

### `02-enable` (admin)

Starts on `/Admin`, the dashboard, signed in as `admin`, with the admin menu showing its top-level entries. Open the page
before `StartCaptureAsync`.

- `enable-1`
  - Actions: click "Tools" in the admin menu; click "Features" with `ClickAndWaitForNavigationAsync` (the page is
    `/Admin/Features`); type `Users Registration` into the search box `#search-box` (delay 70).
  - Visible when the narration ends: the Features page, the search box holding "Users Registration", and the list
    reduced to the row "Users Registration" (category Security) with its blue Enable button
    `#btn-enable-OrchardCore_Users_Registration`.
  - Wait: the row is visible.
  - Labels: "Tools" and "Features" `F/AdminMenu.cs:26-27`; the box `F/Views/Admin/Features.cshtml:27`; the feature name
    `U/Manifest.cs:47`.
- `enable-2`
  - Actions: click `#btn-enable-OrchardCore_Users_Registration` with `ClickAndWaitForNavigationAsync`; the page reloads
    on `/Admin/Features`. Type `Users Registration` into `#search-box` again (delay 20). Highlight the dependency
    badges of the row, `.direct-dependencies` inside the `.list-group-item` that holds
    `#btn-disable-OrchardCore_Users_Registration`.
  - Wait: the notification "The feature Users Registration was enabled." is visible
    (`text=The feature Users Registration was enabled.`).
  - Visible when the narration ends: the green notification, and the row "Users Registration" with a red Disable
    button, and the badges "Users" and "Email" highlighted.
  - Labels: the button `F/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the notification `F/Controllers/AdminController.cs:317`;
    the badges `F/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`, from the dependencies declared in `U/Manifest.cs:49-53`.
- `enable-3`
  - Actions: click "Settings", then "Communication", then "Email" in the admin menu, the last with
    `ClickAndWaitForNavigationAsync` (the page is `/Admin/Settings/email`; if a menu click does not open the page, call
    `NavigateAsync("/Admin/Settings/email")`). The first tab is the one that opens: if it is the "SMTP" tab, click the tab
    "Providers" (`a.nav-link` with that text). Highlight the drop-down `select[name$="DefaultProvider"]`.
  - Visible when the narration ends: the Email settings page on its "Providers" tab, with the drop-down labelled
    "Default provider" showing "Simple Mail Transfer Protocol (SMTP)", highlighted.
  - Labels: "Settings", "Communication", "Email" `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:39-40`; "Default
    provider" `src/OrchardCore.Modules/OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`.

### `03-settings` (admin)

Starts on `/Admin` signed in as `admin`, after clip 1. Users Registration is enabled, so "Registration" is in the menu.
Nothing is saved in this clip: it ends on a page with all three checkboxes cleared.

- `settings-1`
  - Actions: click "Settings", then "Security", then "Registration" in the admin menu, the last with
    `ClickAndWaitForNavigationAsync` (the page is `/Admin/Settings/userRegistration`). Highlight the three checkboxes
    together: `input[type=checkbox][name$="UsersMustValidateEmail"]`, `input[type=checkbox][name$="UsersAreModerated"]`
    and `input[type=checkbox][name$="UseSiteTheme"]`.
  - Visible when the narration ends: the Registration settings page with three cleared checkboxes, highlighted, and a
    Save button.
  - Labels: "Settings", "Security", "Registration" `U/RegistrationAdminMenu.cs:26-28`.
- `settings-2`
  - Actions: clear the highlights; click `input[type=checkbox][name$="UsersMustValidateEmail"]` with
    `scene.ClickAsync`; highlight its label.
  - Visible when the narration ends: the checkbox "Users must verify their email address" selected, with the yellow
    warning above it: "The users whose email address has not been confirmed will not be able to login or reset their
    password."
  - Labels: `U/Views/RegistrationSettings.Edit.cshtml:23` (the label) and `:14` (the warning).
- `settings-3`
  - Actions: clear the highlights; click the first checkbox again to clear it (the warning folds away); highlight the
    label of `input[type=checkbox][name$="UsersAreModerated"]`. Do not select it.
  - Visible when the narration ends: the first checkbox cleared, and the label "Users must be approved before they can
    log in" highlighted.
  - Labels: `U/Views/RegistrationSettings.Edit.cshtml:33`.
- `settings-4`
  - Actions: clear the highlights; highlight the label and the hint of `input[type=checkbox][name$="UseSiteTheme"]`.
    Do not select it, and do not select Save.
  - Visible when the narration ends: the label "Use site theme for registration page" and its hint "Requires an active
    site theme." highlighted, every checkbox cleared.
  - Labels: `U/Views/RegistrationSettings.Edit.cshtml:43-44`.

### `04-register` (visitor, anonymous)

Open `/Login` before `StartCaptureAsync`, in a scene with no sign-in. The page shows the "Log in" form with the link
"Register as a new user" under the Log in button. The scene must not be signed in as `admin`.

- `register-1`
  - Actions: highlight the link `a:has-text("Register as a new user")`; click it with
    `ClickAndWaitForNavigationAsync` (the page is `/Register`).
  - Visible when the narration ends: the Register page, with the heading "Register a new account" and four empty
    fields.
  - Labels: "Log in" `U/Views/LoginForm.Edit.cshtml:9`; "Register as a new user" `U/Views/LoginFormRegisterUser.cshtml:2`;
    "Register a new account" `U/Views/RegisterUserForm.Edit.cshtml:3`.
- `register-2`
  - Actions: type into `input[name$="UserName"]` the text `jamie.chen`; into `input[name$="Email"]` the text
    `jamie.chen@example.com`; into `input[name$="Password"]:not([name$="ConfirmPassword"])` the visitor password; into
    `input[name$="ConfirmPassword"]` the visitor password again. Delay 25.
  - Visible when the narration ends: the form with "Username" `jamie.chen`, "Email address" `jamie.chen@example.com` and
    both password fields filled with dots.
  - Labels: "Username", "Email address", "Password", "Confirm password" `U/Views/RegisterUserFormIdentifier.cshtml:4,10,16,22`.
- `register-3`
  - Actions: click the Register button, `button[type=submit]` with the text "Register", with
    `ClickAndWaitForNavigationAsync`.
  - Wait: the page is the site's home page (the path is `/`).
  - Visible when the narration ends: the home page of the Blog site, without an error.
  - Labels: "Register" `U/Views/RegisterUserForm.Edit.cshtml:16`. The redirect to the home page is
    `U/Controllers/RegistrationController.cs:81` and `:93-100`.
    The layout of TheBlogTheme has no user menu (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), so the page
    does not show that the visitor is signed in.

### `05-verify-settings` (admin)

Open `/Admin/Settings/userRegistration` before `StartCaptureAsync`, signed in as `admin`. All three checkboxes are
cleared.

- `verify-1`
  - Actions: click `input[type=checkbox][name$="UsersMustValidateEmail"]` with `scene.ClickAsync` (the warning appears);
    click the Save button `button.save` with `WaitForNavigationAsync`.
  - Wait: the notification "Site settings updated successfully." is visible.
  - Visible when the narration ends: the notification, with the first checkbox selected.
  - Labels: `U/Views/RegistrationSettings.Edit.cshtml:23`; "Save" `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`;
    the notification `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `05-verify-visitor` (visitor, anonymous, two browser tabs)

Open `/Register` before `StartCaptureAsync`, in a scene with no sign-in. The inbox at `VIDEO_INBOX_URL` holds no message.

- `verify-2`
  - Actions: fill the form with `morgan.lee`, `morgan.lee@example.com` and the visitor password twice, as in
    `register-2` (delay 25); click the Register button with `ClickAndWaitForNavigationAsync`.
  - Wait: the heading "Email confirmation sent" is visible.
  - Visible when the narration ends: the page with the heading "Email confirmation sent" and the sentence "An email has
    been sent to you. Please click on the link it contains in order to have access on this site."
  - Labels: `U/Views/EmailConfirmation/ConfirmEmailSent.cshtml:13` and `:29`. The sent page is the result of
    `U/Handlers/EmailConfirmationLoginFormEvent.cs:31-46`, run from `U/Controllers/RegistrationController.cs:71-79`.
  - Heading: it reads "Email confirmation sent" because registering sets no cookie of the Email confirmation scheme. That
    cookie is set only when a visitor logs in with the right password before the address is confirmed
    (`U/Controllers/AccountController.cs:155-158`), and then the same page reads "Confirm your email" with a "Send
    confirmation email" button (`U/Views/EmailConfirmation/ConfirmEmailSent.cshtml:9,23`). Do not log in as `morgan.lee`
    in this scene before the step.
  - Theme: this page belongs to the Email confirmation controller, which `U/Services/UsersThemeSelector.cs:41-75` has no
    case for, so the page appears in the site theme (TheBlogTheme), not in the login layout of the admin theme that the
    Register page uses. Make sure the heading and the sentence are visible without scrolling.
- `verify-3`
  - Actions: open the inbox in a second tab of the same context, then capture it:
    `var inbox = await scene.Page.Context.NewPageAsync(); await inbox.GotoAsync(inboxUrl); await scene.SwitchToAsync(inbox);`.
    Click the one message in the list, the one whose subject is "Confirm your account", so that its body is shown.
  - Visible when the narration ends: the inbox with a single message from `noreply@example.com` to
    `morgan.lee@example.com`, subject "Confirm your account", and its body open: "Dear morgan.lee," followed by "Please
    click here to confirm your account." and the sentence about how long the link stays valid.
  - Labels: the subject `U/Services/UserEmailService.cs:63`; the body `U/Views/TemplateUserConfirmEmail.cshtml:13,16,18`.
- `verify-4`
  - Actions: click the link "click here" in the message body. If the inbox opens it in a new tab, capture that tab with
    `scene.SwitchToAsync(tab)`. If the inbox does not let the click through, read the link's address with a script
    (off camera, with `scene.OffCameraAsync`), open it with `inbox.GotoAsync(address)` and keep capturing. The address
    is on `http://localhost:5010`.
  - Wait: the heading "Email confirmation" is visible.
  - Visible when the narration ends: the page with the heading "Email confirmation", the sentence "Thank you for
    confirming your email." and the link "Home".
  - Labels: `U/Views/EmailConfirmation/ConfirmEmail.cshtml:5,8`.
  - Theme: as in `verify-2`, this page appears in the site theme (TheBlogTheme).

### `06-approve-settings` (admin)

Open `/Admin/Settings/userRegistration` before `StartCaptureAsync`, signed in as `admin`. The first checkbox is selected
(clip `05-verify-settings` saved it).

- `approve-1`
  - Actions: click `input[type=checkbox][name$="UsersMustValidateEmail"]` to clear it; click
    `input[type=checkbox][name$="UsersAreModerated"]` to select it; click the Save button `button.save` with
    `WaitForNavigationAsync`. The yellow warning of the first checkbox appears when it is cleared, because the box folds
    its warning out on every click; leave it.
  - Wait: the notification "Site settings updated successfully." is visible.
  - Visible when the narration ends: the notification, the first checkbox cleared, and "Users must be approved before
    they can log in" selected.
  - Labels: `U/Views/RegistrationSettings.Edit.cshtml:23,33`.

### `06-approve-visitor` (visitor, anonymous)

Open `/Register` before `StartCaptureAsync`, in a scene with no sign-in.

- `approve-2`
  - Actions: fill the form with `riley.park`, `riley.park@example.com` and the visitor password twice (delay 25); click
    the Register button with `ClickAndWaitForNavigationAsync` (the page is `/RegistrationPending`).
  - Wait: the heading "User Registration Pending" is visible.
  - Visible when the narration ends: the heading "User Registration Pending" and the sentence "Your user account has been
    created but has to be approved before it can be used."
  - Labels: `U/Views/Registration/RegistrationPending.cshtml:7,9`. The redirect is `U/Handlers/UserModerationLoginFormEvent.cs:42-45`.
  - Theme: this page belongs to the Registration controller, so with "Use site theme for registration page" cleared it
    appears in the login layout of the admin theme (`U/Services/UsersThemeSelector.cs:67-69,86`).

### `06-approve-admin` (admin)

Starts on `/Admin`, the dashboard, signed in as `admin`, with the admin menu showing its top-level entries. Open the
page before `StartCaptureAsync`. The Users list, once opened, shows `admin`, `jamie.chen`, `morgan.lee` and `riley.park`.

- `approve-3`
  - Actions: click "Access Control" in the admin menu; click "Users" with `ClickAndWaitForNavigationAsync` (the page is
    `/Admin/Users/Index`); highlight the badge "Disabled" in the row of `riley.park` (the `.list-group-item` whose text
    holds `riley.park`); click the "Actions" button of that row (`button.actions`, text "Actions"); highlight the item
    "Enable" of the open menu.
  - Wait: the row of `riley.park` is visible.
  - Visible when the narration ends: the Users list, the row of `riley.park` with the red badge "Disabled", its Actions
    menu open, and the item "Enable" highlighted.
  - Labels: "Users" under "Access Control" `U/AdminMenu.cs:27,30`; "Disabled" `U/Views/UserFields.cshtml:23`; "Actions"
    `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml` (the default Buttons layout); "Enable"
    `U/Views/UserActionsMenu.cshtml:24`.
- `approve-4`
  - Actions: click "Enable" of the open menu; the dialog "Enable user" asks "Are you sure you want to enable this user?";
    click its button `#modalOkButton` (text "Ok") with `WaitForNavigationAsync`.
  - Wait: the notification "User account was successfully enabled." is visible.
  - Visible when the narration ends: the Users list with the notification, and the row of `riley.park` without the
    "Disabled" badge.
  - Labels: the dialog title and message `U/Views/UserActionsMenu.cshtml:24`; "Ok" `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`;
    the notification `U/Controllers/AdminController.cs:580`.

### `07-external` (slides)

- `external-1`: the slide "Registering with a login provider", with four cards: "Google Authentication", "GitHub
  Authentication", "Meta Login" and "External Authentication tab".
- `external-2`: the slide "External registration options", with four cards: "Disable new external registration", "Do not
  ask username", "Do not ask email address" and "Do not create local password".
- `external-3`: the slide "Providers follow the same rules", with three cards: "Same options", "Own credentials" and
  "One page per provider".

## 5. Checks for private data

- Every email address on screen ends in `@example.com`: `admin@example.com`, `noreply@example.com`,
  `jamie.chen@example.com`, `morgan.lee@example.com`, `riley.park@example.com`. The visitors are invented.
- No password is ever on screen: the password fields show dots, and no step displays `VIDEO_PASSWORD` or
  `VIDEO_VISITOR_PASSWORD`.
- The inbox must show only the one message of clip `05-verify-visitor`, with no other sender, no host name of this
  machine, and no raw header view (the raw view holds the confirmation address with its token). Show the message body
  only.
- The Docker container, the data folder and the user name of the person recording are not on screen.
- OCR the assembled video at one frame per second and at every scene change, and search the text for `@` addresses
  other than `example.com`, for digits that look like phone numbers, and for the user name of the person recording.
