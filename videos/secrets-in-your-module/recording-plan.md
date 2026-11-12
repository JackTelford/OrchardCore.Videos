# Recording plan: Secrets in Your Module

Audience: developer. Most of the video is slides (`slides.py`). Two clips are browser recordings: the Password field of
the SMTP email settings, which is edited with the secret input that the video teaches, first with the Secrets feature
disabled (`01-without`), then enabled (`01-with`). Nothing else is recorded. Every `src/...` path below is a path in
`OrchardCMS/OrchardCore` at commit `92db4a83`, the revision the content was written from. In this file a selector
stands for `scene.Page.Locator(...)`.

## The site it needs

- **Source:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at commit `92db4a83` or a descendant of
  it. Run it on `http://localhost:5010` (`VIDEO_BASE_URL`). Auto Setup is enabled in its `Program.cs`
  (`AddSetupFeatures("OrchardCore.AutoSetup")`).
- **Data folder:** `ORCHARD_APP_DATA` points to an empty folder outside both repositories
  (`src/OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs:5`).
- **Auto Setup** (environment variables, no setup or login form is recorded):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **No outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`). The
  SMTP settings are only saved, never used: no mail is sent.
- **Sign-in:** `Recorder setup` signs in as `admin` with `VIDEO_PASSWORD=<password>` and writes `state.json`
  (git-ignored). Run it once, as soon as Auto Setup has finished and before the features are enabled, so that
  `state.json` exists when snapshot S1 is taken. Never write the password in a file of the repository.
- **Tenant, theme, language:** the Default tenant, the admin theme TheAdmin, light color scheme, `en-US`.
- **Features enabled before the first take** (the Blog recipe enables neither of them,
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`): in **Tools > Features**
  (`src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`), type `Email` in the box with the placeholder
  **Search** (it filters the list on the page), click **Enable** in the row named exactly **Email** (not
  **Azure Communication Services Email** or **Email Notifications**); then type `SMTP`, and click **Enable** in the row
  of **SMTP Email Provider** (`OrchardCore.Email/Manifest.cs`, `OrchardCore.Email.Smtp/Manifest.cs`). If a row shows
  **Disable** instead of **Enable**, the feature is already on: leave it. The button is
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. Both rows then show
  **Disable**. The **Secrets** feature stays disabled for now.
- **Seed data (S1), created by hand, outside the capture.** Open **Settings > Communication > Email**
  (`/Admin/Settings/email`, `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:38-40`), click the tab **SMTP**,
  and set exactly:

  | Label | Value |
  |---|---|
  | Enable SMTP Provider | checked |
  | Sender email address | `noreply@example.com` |
  | Delivery method | Network |
  | Host name | `smtp.example.com` |
  | Port number | `25` |
  | Require credentials | checked |
  | Use Windows authentication | not checked |
  | User name | `orchard-mailer` |
  | Password | empty |

  Click **Save**. The notice **Site settings updated successfully.** appears
  (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`). The other labels come from
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10-148`. Nothing else is configured.
- **External services:** none. There is no mail server: `smtp.example.com` is never contacted.

## Order of the clips and snapshots

1. With `state.json` written (see **Sign-in**), stop the site, copy its `App_Data` folder and `state.json` aside as
   **S1** (SMTP enabled and configured, no password saved, **Secrets** disabled). Start the site again.
2. Record `01-without`. It changes the site: it saves a protected password in the SMTP settings.
3. Off camera, enable **Secrets** (below), and create the secret **Smtp.Password** (below). Stop the site and copy
   `App_Data` and `state.json` aside as **S2**. Start the site again.
4. Record `01-with`. It changes the site: it saves a reference to the secret and removes the stored password.

`01-with` depends on `01-without`: S2 must come from a take of `01-without` that was kept, because the hint
"The value currently stored with these settings is removed once a secret is selected." appears only when a protected
value is stored. To record `01-without` again, restore S1, and repeat steps 3 and 4 afterwards. To record `01-with`
again, restore S2.

**Enable Secrets (off camera, between the two clips):** **Tools > Features**, type `Secrets` in the box with the
placeholder **Search**, click **Enable** in the row named exactly **Secrets** (`OrchardCore.Secrets/Manifest.cs`: Name "Secrets"; not
**Secrets - Azure Key Vault** or **OpenID Connect Secrets**).

**Create the secret (off camera, between the two clips):** **Tools > Security > Secrets**
(`src/OrchardCore.Modules/OrchardCore.Secrets/AdminMenu.cs:33`), click **Add Secret**
(`Views/Admin/Index.cshtml:39`), and in the dialog **Select Secret Type** click **Create** in the card **Text Secret**
(`Views/Admin/Index.cshtml:200,214`, `Providers/TextSecretTypeProvider.cs:14`). Fill in **Name** `Smtp.Password`,
**Value** `sample-secret-value`, **Description** `SMTP password of the sample site`, and click **Save**
(`Views/Admin/Edit.cshtml:24,68,195,213`). The list **Secrets** then shows one entry.

## Clip `01-without` (chapter 01, Secrets disabled)

- **Starts on:** `scene.OpenAsync("/Admin/Settings/email")`, signed in as `admin` (not recorded). The tab **Providers**
  is active. Expand **Settings** and **Communication** in the admin menu before `StartCaptureAsync()`, so that
  **Email** is highlighted in the menu when the capture starts. (Opening the page by its address usually expands the
  branch of the current page; if it does not, click **Settings**, then **Communication**.)
- **Site state:** S1.
- **Changes the site:** yes (saves a protected password). Restore S1 before another take.
- Selectors used: the tab `a[role='tab']:text-is('SMTP')`; the password input `input[type='password'][id$='_Password']`
  (its id ends with `_Password`; the secret input renders a password input with this id when the Secrets feature is
  disabled, `src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96-97`); the save
  button `button.save` (label **Save**, `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`).

`editor-1`
- Actions: click the tab `a[role='tab']:text-is('SMTP')` (the tabs are named by the placement `Content:5#SMTP`,
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88`, rendered by
  `src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:14-60`). Wait until the tab has the class
  `active`, and until the label `label[for$='_Password']` is visible (the Network delivery options open when the
  page loads, `OrchardCore.Email.Smtp/Assets/ts/smtp-settings.ts`; allow a second for the animation). Scroll down
  with `ScrollAsync(0, 200)` repeatedly until that label is entirely inside the viewport (its bounding box ends above
  864 CSS pixels), then stop. Highlight the password input with `HighlightAsync`, without a caption.
- Visible when the narration ends: the **SMTP** tab selected; the rows **User name** (`orchard-mailer`) and
  **Password**; the Password row holds an empty password input with the placeholder **Enter a new value** and the hint
  "The password for authentication."; a green ring around the input. No drop-down list in that row.
- Labels from: `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:139,148,151`;
  `OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:104`.

`editor-2`
- Actions: `ClearHighlightsAsync()`; `TypeAsync` into `input[type='password'][id$='_Password']` the text
  `not-a-real-password` (a made-up value, drawn as dots); click **Save** with `ClickAndWaitForNavigationAsync`; wait
  until the text **Site settings updated successfully.** is visible; click the tab `a[role='tab']:text-is('SMTP')`
  again (the page reloads on its first tab, **Providers**, unless the admin remembers the tab: click only if the tab
  is not `active`); scroll down with the same stop condition as in `editor-1`.
- Visible when the narration ends: the **SMTP** tab selected, the Password row with an empty password input whose
  placeholder now reads **The value is securely stored. Enter a new value to update it, or leave it blank to keep it.**
- Labels from: `SecretInputTagHelper.cs:103`; `Views/SiteSettings.SaveButton.cshtml:2`;
  `OrchardCore.Settings/Controllers/AdminController.cs:119`.

## Clip `01-with` (chapter 01, Secrets enabled)

- **Starts on:** `scene.OpenAsync("/Admin/Settings/email")`, signed in as `admin` (not recorded), on site state S2.
  Expand **Settings** and **Communication** in the menu before `StartCaptureAsync()`, as in `01-without`.
- **Changes the site:** yes (saves a reference to `Smtp.Password`, removes the stored password). Restore S2 before
  another take.
- Selectors used: the tab `a[role='tab']:text-is('SMTP')`; the source list `select[data-secret-input-source]`
  (options **User-specified value** and **Use a secret**,
  `src/OrchardCore.Modules/OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:25-27`); the list of secrets
  `[data-secret-input-secrets] select` (first option **None**, then **Smtp.Password**,
  `OrchardCore.Secrets/ViewComponents/SelectSecretViewComponent.cs:49`); the reload button
  `button[data-secret-input-reload]` (an icon with no text and the title **Reload the secrets**, `SecretInput.cshtml:52`);
  the link `a:has-text('New secret')` (`SecretInput.cshtml:58-59`); the save button `button.save`.

`editor-3`
- Actions: click the tab `a[role='tab']:text-is('SMTP')`; wait until it is `active` and until the label
  `label[for$='_Password']` is visible; scroll down with `ScrollAsync(0, 200)` repeatedly until the label `label[for$='_Password']` is entirely inside the viewport (its
  bounding box ends above 864 CSS pixels), then stop; highlight `select[data-secret-input-source]` with
  `HighlightAsync`, without a caption.
- Visible when the narration ends: the Password row shows a drop-down list with **User-specified value** selected, a
  password input under it with the placeholder **The value is securely stored. Enter a new value to update it, or
  leave it blank to keep it.**, and the hint **The value is encrypted and stored with these settings.**; a green ring
  around the list.
- Labels from: `SecretInput.cshtml:26,31,36`.

`editor-4`
- Actions: `ClearHighlightsAsync()`; `SelectAsync(select[data-secret-input-source], "Use a secret")` (a headless
  browser draws no open list: the click shows a ripple and the value changes); wait until
  `[data-secret-input-secrets] select` is visible; then `HighlightAsync` the button `button[data-secret-input-reload]`
  and, after a short pause (`PauseAsync(700)`), the link `a:has-text('New secret')`, without captions. Do not click
  **New secret** (it opens another tab).
- Visible when the narration ends: the list of secrets showing **None**, the reload button and **New secret** beside it
  (the second one ringed), the hint **Create the secret in the new tab, then reload the secrets to select it.** and the
  hint **The value currently stored with these settings is removed once a secret is selected.** The password input is
  hidden.
- Labels from: `SecretInput.cshtml:27,52,58-59,69,73`; `SelectSecretViewComponent.cs:49` (the option **None**).

`editor-5`
- Actions: `ClearHighlightsAsync()`; `SelectAsync([data-secret-input-secrets] select, "Smtp.Password")`; click **Save**
  with `ClickAndWaitForNavigationAsync`; wait until **Site settings updated successfully.** is visible; click the SMTP
  tab if it is not `active`; scroll down with the same stop condition as in `editor-3`.
- Visible when the narration ends: the Password row shows **Use a secret** selected, the list of secrets with
  **Smtp.Password** selected, the reload button and **New secret**, and the hint about the stored value is gone (no
  value is stored any more).
- Labels from: `SecretInput.cshtml:27,52,58-59`; `OrchardCore.Settings/Controllers/AdminController.cs:119`.

## Slide steps

The slides are drawn by `slides.py` from the documentation of the Secrets module (the MyService example) and the SMTP
provider of Orchard Core. Nothing is recorded for them.

- `pieces-1`: four cards: the editor model, the secret-input tag helper, GetSecretValueAsync, SecretMigration.
- `pieces-2`: the manifest of the SMTP email provider (it depends on Email, not on Secrets), and three cards: a
  reference, no dependency, detected at runtime.
- `settings-1`: the settings class with the protected value and the name of the secret.
- `settings-2`: the constants for the Data Protection purpose and the suggested secret name, and two cards.
- `driver-1`: the editor model of the settings, and the members of SecretInputViewModel.
- `driver-2`: the display driver and its Edit method calling SecretInputViewModel.Create.
- `driver-3`: the UpdateAsync method of the display driver, with the SecretInputUpdateContext.
- `driver-4`: the `Succeeded` check that stores the result, and three cards about the outcomes.
- `driver-5`: the Startup class that registers the site display driver.
- `view-1`: the `_ViewImports.cshtml` tag helper line, and the editor row with the secret-input tag helper.
- `view-2`: two cards: the Secrets feature disabled, enabled.
- `runtime-1`: the GetSecretValueAsync call, and two cards (the secret first, otherwise the protected value).
- `runtime-2`: the same call with the logger argument highlighted, and two cards (null when no value is available,
  the optional logger receives the reason, never the value).
- `runtime-3`: the code of the SMTP settings driver that requests an options update when the settings changed, and the
  registration of the signal in its Startup (`AddSignalOptionsChangeTokenSource<SmtpOptions>()`).
- `migrate-1`: the SecretMigration display driver class and the SecretsStartup class with RequireFeatures.
- `migrate-2`: BuildPrefix and EditAsync of the migration driver.
- `migrate-3`: UpdateAsync of the migration driver calling MoveToSecretAsync.
- `other-1`: the WebhookPart content part, and three cards about credentials outside site settings.

## Cards

- `intro`: the opening card, "Secrets in Your Module", subtitle "Offer secret inputs in your settings".
- `outro`: the closing card, subtitle "docs.orchardcore.net".

## Checks for private data

- The only values typed on camera are `not-a-real-password` (shown as dots) and the choice of the secret
  `Smtp.Password`; the value `sample-secret-value` is typed off camera and never displayed.
- Every address is `@example.com` or `example.com`: `noreply@example.com`, `smtp.example.com`, and the admin
  `admin@example.com`. The user name is `orchard-mailer`.
- The password of the admin account is never typed on camera; the recording starts signed in (`state.json`).
- Look at every contact sheet (`review.py`) before assembling: no error message, no empty list, no admin password, no
  real name or address. OCR the video once at one frame per second and at every scene change, and search for `@` and
  for digits that look like a phone number or a key.
- `state.json`, the copies of `App_Data` (S1, S2) and any mask script stay out of git.
