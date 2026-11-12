# Recording plan: Secrets: Move Existing Credentials and Change Stores

Audience: admin. The browser shows the admin UI only. Terminal commands, configuration and code stay out of the
capture; they appear here because this file is for the recorder. In this file a selector stands for
`scene.Page.Locator(...)`. Every label below was read from the Orchard Core source (paths are relative to the
`OrchardCMS/OrchardCore` checkout: `src/OrchardCore.Modules/` for the modules, `src/OrchardCore/` for
`OrchardCore.Secrets.Abstractions`, `OrchardCore.Secrets.Core` and `OrchardCore.Email.Abstractions`). Selectors that depend on the admin
theme's markup (tabs, breadcrumbs, the admin menu) are marked "probe": confirm them with `Recorder probe` before
recording, and prefer the visible text over a CSS class.

## The site it needs

- **Source**: a checkout of `OrchardCMS/OrchardCore` at commit `92db4a83`, the source every label below was read from,
  built from `src/OrchardCore.Cms.Web`. If you build another commit, compare the labels of the pages listed below first.
- **Host**: this video needs two writable secret stores, `Database` and `AzureKeyVault`
  (`OrchardCore.Secrets.Core/Services/DatabaseSecretStore.cs:37`, `OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStore.cs:66`).
  The Key Vault store resolves a client that the host registers, so run a throwaway host project outside both
  repositories: a copy of `OrchardCore.Cms.Web` whose `Program.cs` registers the keyed client named `SecretClient`
  from the `AzureClients:SecretClient` configuration section, exactly as the "Azure Key Vault Store" section of
  `src/docs/reference/modules/Secrets/README.md` shows (with the usings shown there). The store looks up that
  client when it is built (`OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStore.cs:35-41`, `Startup.cs`), so on a host that
  does not register it any page that builds the secret manager, such as the Secrets pages, is expected to fail: never
  enable the Azure Key Vault feature on the plain `Cms.Web`.
- **External service and its stand-in**: the vault is the local Azure Key Vault Emulator of the same documentation
  page ("Local development with the Azure Key Vault Emulator"), run in Docker with container port `4997` on host port
  `4997`, persistence off. Configure the throwaway host as that page shows for the emulator (`VaultUri`
  `https://localhost:4997`, `CredentialSource` `EnvironmentCredential`, `DisableInstanceDiscovery` true,
  `DisableChallengeResourceVerification` true, `AzureClient` `SecretClient`, `NamePrefix` `localdev`), and set the
  emulator's published placeholder credential variables in the process that starts the host. Set `NO_PROXY` to
  `localhost,127.0.0.1` next to the dead `HTTP_PROXY`/`HTTPS_PROXY`, so only the emulator stays reachable. The emulator
  must be running for every clip and for the seed: besides the Secrets and Secret Stores pages, the SMTP and GitHub
  Authentication settings pages list the secrets of every store (`OrchardCore.Secrets/ViewComponents/SelectSecretViewComponent.cs:32`,
  rendered by `Views/Shared/SecretInput.cshtml:41` even while `User-specified value` is selected). How the
  emulator's certificate is trusted is an open question (see OPEN-QUESTIONS.md). Remove the container and the host
  project afterwards.
- **Provisioning**: Auto Setup through environment variables (see the `orchardcore-tester` skill of the Orchard Core
  repository), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), tenant `Default`, site
  name `Orchard Core`, recipe Blog, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password
  `<password>` (passed in `VIDEO_PASSWORD`, chosen outside git). Block outbound calls with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address. No sign-in screen is recorded; save `state.json` with `Recorder setup`.
- **Window**: the standard 1536x864 viewport. Expand Tools, then Security, in the admin menu before
  `StartCaptureAsync` in every clip that clicks a link of that group, so Secrets, Migrate Secrets and Secret Stores are
  visible (probe the menu markup).

### Features to enable before recording

In Tools, Features (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, button `Enable`), search
for each name and enable it. Each enable restarts the tenant, so wait for the page to reload.

1. `Secrets`
2. `Secrets - Azure Key Vault` (only on the throwaway host, with the emulator running)
3. `SMTP Email Provider` (also enables Email)
4. `GitHub Authentication` (also enables its external authentication dependency)

### Seed data (create it outside the capture)

All values are fake. The password fields are never readable on screen.

1. **SMTP** at `/Admin/Settings/email`, tab `SMTP` (group id `email`, `OrchardCore.Email.Abstractions/EmailSettings.cs:8`;
   labels in `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml`):
   - tick `Enable SMTP Provider` (line 10);
   - `Sender email address` (line 17): `noreply@example.com`;
   - `Delivery method` (line 26): `Network`;
   - `Host name` (line 50): `smtp.example.com`; `Port number` (line 59): `25`;
   - tick `Require credentials` (line 120); `User name` (line 139): `mailer`;
   - `Password` (line 148): leave the selector on `User-specified value` and type a made-up value such as
     `demo-mail-value` in the password box;
   - click `Save` (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`).
2. **GitHub Authentication** at `/Admin/Settings/OrchardCore.GitHub.Authentication` (the group id is the feature id,
   `OrchardCore.GitHub/GithubConstants.cs:7`; the menu path is Settings, Security, Authentication, GitHub,
   `AdminMenuGitHubLogin.cs`). Labels in `OrchardCore.GitHub/Views/GitHubAuthenticationSettings.Edit.cshtml`:
   - `Client ID` (line 20): `demo-client-id`;
   - `Client secret` (line 29): leave the selector on `User-specified value` and type a made-up value such as
     `demo-github-value`;
   - `CallbackPath` (line 37): type `/signin-github`, the value its placeholder shows; it matches the pattern of
     `OrchardCore.GitHub/ViewModels/GitHubAuthenticationSettingsViewModel.cs`;
   - click `Save`. Saving requests a tenant release (`Drivers/GitHubAuthenticationSettingsDisplayDriver.cs:92`), so wait
     for the page to reload.
3. **Preconditions to check, not to record**:
   - Tools, Security, Secrets shows "Nothing here! There are no secrets stored. Click 'Add Secret' to create one."
     (`OrchardCore.Secrets/Views/Admin/Index.cshtml:179`, empty list message; the Blog recipe,
     `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, enables no Secrets feature and creates no secret);
   - Tools, Security, Migrate Secrets lists exactly two rows, SMTP Password and GitHub Authentication Client secret,
     and shows the `Destination store` list (two writable stores);
   - Tools, Security, Secret Stores lists `Database` and `AzureKeyVault` under `Source store`;
   - the emulator is running and empty (a fresh container).

Then stop the site, copy its `App_Data` folder and `state.json` aside (the snapshot), and start the site again. The
snapshot is only complete together with an empty emulator: restore both before every full take.

## Order of the clips, and what each one changes

| Order | Clip | Starts on | Changes the site |
|---|---|---|---|
| 1 | `01-before` | `/Admin/Settings/email`, tab SMTP | No |
| 2 | `02-migrate` | `/Admin` | Yes: both credentials become secrets in `Database`, both settings reference them, the tenant restarts |
| 3 | `03-check` | `/Admin` | No (read only: never click Save) |
| 4 | `05-move-all` | `/Admin` | Yes: both secrets move from `Database` to `AzureKeyVault` |
| 5 | `06-move-one` | `/Admin` | Yes: `Smtp.Password` moves back to `Database` |

The slide clips (`01-settings`, `01-stores`, `04-reasons`, `07-retire`) need no recording. Clips 2 to 5 depend on each
other in this order: `03-check` needs `02-migrate` done, `05-move-all` needs both secrets in `Database`, `06-move-one`
needs both secrets in `AzureKeyVault`. To record one clip again, restore the snapshot (site and emulator) and play the
earlier clips off camera (the same actions, without the capture) until the site is in the state the clip expects.
`01-before` has to be recorded from the snapshot, before `02-migrate`, because it needs the password still kept in the
settings.

## Chapter 00: opening card

`intro`: the opening card. Nothing to record.

## Chapter 01: Credentials, secrets, and stores

- `tools-1` (slide, clip `01-settings`): three cards, "In the settings", "As a secret" and "Migrate Secrets", with the
  heading "Where a credential can be kept".
- `before-1` (clip `01-before`)
  - Starts on: `/Admin/Settings/email`. Before `StartCaptureAsync`: click the `SMTP` tab (probe the tab markup, the
    tab comes from `Content:5#SMTP` in `OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs`), and scroll so the
    `Password` row is in the middle of the viewport.
  - Actions: `HighlightAsync` the selector `[data-secret-input-source]` of the `Password` row with the caption
    `User-specified value`; hold; `ClearHighlightsAsync`.
  - Visible when the narration ends: the `Password` label, the selector showing `User-specified value`, the password box
    with its placeholder "The value is securely stored. Enter a new value to update it, or leave it blank to keep it.",
    and the hint "The value is encrypted and stored with these settings."
  - Labels from: `OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:26` (`User-specified value`), `:31` (placeholder),
    `:36` (hint); `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:148` (`Password`).
- `tools-2` (slide, clip `01-stores`): three cards, "Database", "AzureKeyVault" and "Secret Stores", with the heading
  "Where a secret is kept".

## Chapter 02: Migrate Secrets (clip `02-migrate`)

Starts on `/Admin`, with Tools, Security expanded. Nothing is typed in this clip.

- `open-1`
  - Actions: `HighlightAsync` the menu link `Migrate Secrets` (probe: link text `Migrate Secrets`), then
    `ClickAndWaitForNavigationAsync` on it. The page is `/Admin/Secrets/Migration/Index`.
  - Visible when the narration ends: the title `Migrate Secrets`, the paragraph "These credentials are kept, encrypted,
    in the settings of the features that use them. Select the ones to move to secrets. The settings then reference the new
    secrets, and no longer keep the values.", the two rows, and the `Destination store` list above them.
  - Labels from: `OrchardCore.Secrets/AdminMenu.cs:40` (`Migrate Secrets`), `Views/Migration/Index.cshtml:8` (title),
    `:51` (paragraph).
- `list-1`
  - Actions: `HighlightAsync` the group badge and the credential name of the SMTP row (selector
    `.list-group-item:has-text("SMTP")`, label `.form-check-label`), then of the GitHub Authentication row; then
    `HighlightAsync` the `Secret name` box of the first row (selector `.list-group-item:has-text("SMTP") input[type=text]`),
    which shows `Smtp.Password`. The other row shows `GitHub.ClientSecret`. Clear the highlights at the end.
  - Visible when the narration ends: both rows with their badges, `Password` and `Client secret`, and the `Secret name`
    boxes with the suggested names.
  - Labels from: `Views/SecretMigrationItem.Edit.cshtml:9` (group badge), `:16` (`Secret name`);
    `OrchardCore.Email.Smtp/Drivers/SmtpSecretMigrationDisplayDriver.cs:61-63` (`SMTP`, `Password`,
    `Smtp.Password` from `Services/SmtpOptionsConfiguration.cs:15`);
    `OrchardCore.GitHub/Drivers/GitHubSecretMigrationDisplayDriver.cs:61-63` (`GitHub Authentication`, `Client secret`,
    `GitHub.ClientSecret` from `GithubConstants.cs:12`).
- `store-1`
  - Actions: `HighlightAsync` the `Destination store` label and list (selector `select#Store`), then `SelectAsync` with the
    value `Database`. Do not change anything else.
  - Visible when the narration ends: `Destination store` with `Database` selected and the hint "The secret store that
    receives the new secrets."
  - Labels from: `Views/Migration/Index.cshtml:58` (label), `:62` (hint). The list is only rendered when more than one
    writable store exists (`:55`).
- `pick-1`
  - Actions: `ClickAsync` the checkbox of the SMTP row (selector `.list-group-item:has-text("SMTP") input[type=checkbox]`),
    then the checkbox of the GitHub Authentication row (selector
    `.list-group-item:has-text("GitHub Authentication") input[type=checkbox]`); then `HighlightAsync` the button `Move to Secrets`
    (selector `button:text-is("Move to Secrets")`) without clicking it.
  - Visible when the narration ends: both checkboxes ticked, `Move to Secrets` highlighted.
  - Labels from: `Views/Migration/Index.cshtml:80` (`Move to Secrets`).
- `done-1`
  - Actions: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on `Move to Secrets`. The tenant is released
    after the save (`Controllers/MigrationController.cs:116`), so the next page can load slowly: if it takes more than
    about three seconds, wait for the alert with `OffCameraAsync` on the condition "the alert `.alert-info` containing
    `Nothing to migrate!` is visible" (up to 30 seconds), and record from there.
  - Visible when the narration ends: the success message "2 credentials were moved to secrets." and, below it, the info
    alert "Nothing to migrate! No setting keeps a credential outside of the Secrets module."
  - Labels from: `Controllers/MigrationController.cs:118` (message, plural form), `Views/Migration/Index.cshtml:43` (alert).
    The page after the save is the redirect to `Index` (`:129`).

## Chapter 03: Check the result (clip `03-check`)

Starts on `/Admin`, with Tools, Security expanded. Do not click `Save` anywhere in this clip.

- `check-1`
  - Actions: `ClickAndWaitForNavigationAsync` on the menu link `Secrets` (probe: link text `Secrets`; the page is
    `/Admin/Secrets/Index`); `HighlightAsync` the badges `Text Secret` and `Database` of the first row; hold.
  - Visible when the narration ends: two rows, `GitHub.ClientSecret` and `Smtp.Password`, each with the badges
    `Text Secret`, `Database` and `Never expires`, and the descriptions "GitHub Authentication: Client secret" and
    "SMTP: Password" under the names.
  - Labels from: `OrchardCore.Secrets/AdminMenu.cs:33`; `Views/Admin/Index.cshtml:109` (type badge), `:113` (store badge),
    `:139` (`Never expires`), `:166` (description); `Providers/TextSecretTypeProvider.cs:14` (`Text Secret`). The
    description is set to the display name by `OrchardCore.Secrets.Abstractions/SecretMigrationExtensions.cs:79`.
- `check-2`
  - Actions: `NavigateAsync("/Admin/Settings/email")`; click the `SMTP` tab (probe); `ScrollAsync` until the `Password` row
    is in the middle of the viewport; `HighlightAsync` the selector `[data-secret-input-source]` with the caption
    `Use a secret`.
  - Visible when the narration ends: the `Password` row with the selector on `Use a secret`, the list of secrets below it
    with `Smtp.Password` selected, the reload button (tooltip `Reload the secrets`) and the `New secret` link, and the
    hint "Create the secret in the new tab, then reload the secrets to select it."
  - Labels from: `OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:27` (`Use a secret`), `:52` and `:59`
    (`Reload the secrets`, `New secret`), `:69` (hint), `ViewComponents/SelectSecretViewComponent.cs:36-45` (the list);
    the selector shows `Use a secret` because the settings now hold a secret name
    (`OrchardCore.Secrets.Abstractions/SecretInputViewModel.cs`, `Create`).

## Chapter 04: When a credential stays

- `reasons-1` (slide, clip `04-reasons`): four cards, "The name is missing", "The name is in use", "The value does not
  decrypt" and "The store does not save it", each with the message of the screen
  (`Views/Migration/Index.cshtml:22-32`, the `switch` on `result.Error`; the second message shows the example name
  `'Smtp.Password'` where the screen shows the name of the secret).

## Chapter 05: Secret Stores (clip `05-move-all`)

Starts on `/Admin`, with Tools, Security expanded. State expected: two secrets in `Database`, none in `AzureKeyVault`.

- `stores-1`
  - Actions: `HighlightAsync` the menu link `Secret Stores`, `ClickAndWaitForNavigationAsync` on it (the page is
    `/Admin/Secrets/Stores/Index`); `HighlightAsync` the `Source store` list, then the hint under it.
  - Visible when the narration ends: the title `Secret Stores`, `Source store` set to `Database`, the hint "Total active
    secrets in the selected store: 2", and below it the heading "Move all active secrets from this store to".
  - Labels from: `OrchardCore.Secrets/AdminMenu.cs:47`; `Views/Store/Index.cshtml:19` (title), `:29` (`Source store`),
    `:40` (hint), `:55` (heading).
- `dest-1`
  - Actions: `ClearHighlightsAsync`; `SelectAsync` on `select#DestinationStore` with the value `AzureKeyVault` (the
    first option is "Select a destination"); `ClickAsync` the checkbox `input#Confirm`; `HighlightAsync` its label.
  - Visible when the narration ends: `Destination store` set to `AzureKeyVault`, the hint "A move verifies the destination
    before deleting the source, and keeps the name of each secret. Existing secrets in the destination are never
    overwritten, and secret values stay on the server.", and the ticked checkbox "I confirm this move, and the deletion
    from the source store once the destination is verified."
  - Labels from: `Views/Store/Index.cshtml:72` (`Destination store`), `:75` (`Select a destination`), `:82` (hint), `:90` (checkbox label).
- `moved-1`
  - Actions: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on `button:text-is("Move")` (the primary button
    under the checkbox; the POST returns the same page with results). The emulator answers in about a second per secret;
    if it takes longer, wait with `OffCameraAsync` on the condition "the list `.list-group` under the heading `Results`
    is visible".
  - Visible when the narration ends: the page after the move, with `Results` in view.
  - Labels from: `Views/Store/Index.cshtml:98` (`Move`).
- `result-1`
  - Actions: `HighlightAsync` the `Results` heading and the two `Moved` badges; then `HighlightAsync` the hint under
    `Source store`.
  - Visible when the narration ends: `Results` with `GitHub.ClientSecret` and `Smtp.Password`, each with a green badge
    `Moved`, and "Total active secrets in the selected store: 0" under `Source store`. The move form is gone, because
    `Database` keeps no active secret (`Views/Store/Index.cshtml:7`).
  - Labels from: `Views/Store/Index.cshtml:112` (`Results`), `:122` (`Moved`), `:40` (hint).
- `source-1`
  - Actions: `ClearHighlightsAsync`; `WaitForNavigationAsync` around `SelectAsync` on `select#source-store` with the value
    `AzureKeyVault` (the list submits its form on change, `Views/Store/Index.cshtml:31` and the script at `:143`); `HighlightAsync` the hint.
  - Visible when the narration ends: `Source store` set to `AzureKeyVault` and the hint "Total active secrets in the
    selected store: 2". The move form appears again, with `Database` as the only destination. Do not submit it.

## Chapter 06: Move one secret (clip `06-move-one`)

Starts on `/Admin`, with Tools, Security expanded. State expected: both secrets in `AzureKeyVault`.

- `one-1`
  - Actions: `ClickAndWaitForNavigationAsync` on the menu link `Secrets`; `HighlightAsync` the `AzureKeyVault` badge of the
    first row; `MoveToAsync` the `Move` button of the `Smtp.Password` row (selector
    `li.list-group-item:has-text("Smtp.Password") a:text-is("Move")`), without clicking.
  - Visible when the narration ends: both rows with the badge `AzureKeyVault`, and the cursor on `Move`, between `Edit`
    and `Delete`. The narration says "a badge ... names the store": the word `Store` is only the tooltip of the badge
    (`title="Store"`), so the narration does not say it.
  - Labels from: `Views/Admin/Index.cshtml:113` (store badge, tooltip `Store`), `:156` to `:158` (`Edit`, `Move`, `Delete`).
- `one-2`
  - Actions: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on that `Move` button; `HighlightAsync` the page title
    in the header (probe), then the heading under the form.
  - Visible when the narration ends: the title "Move 'Smtp.Password' secret", the heading "Move the secret from the
    'AzureKeyVault' store to", the list `Destination store` with "Select a destination" and `Database`, and no
    `Source store` list.
  - Labels from: `Views/Store/Index.cshtml:13` (title), `:55` (heading).
- `one-3`
  - Actions: `ClearHighlightsAsync`; `SelectAsync` on `select#DestinationStore` with the value `Database`; `ClickAsync` the
    checkbox `input#Confirm`; `ClickAndWaitForNavigationAsync` on `button:text-is("Move")`. Use short `pauseAfter` values so
    the actions end before the narration does.
  - Visible when the narration ends: the page after the move, with `Results` in view.
- `one-4`
  - Actions: `HighlightAsync` the `Moved` badge of the result; hold about three seconds; `ClearHighlightsAsync`;
    `ClickAndWaitForNavigationAsync` on the breadcrumb link `Secrets` above the title (probe; fall back to the menu link
    `Secrets`).
  - Visible when the narration ends: the Secrets list, with `Smtp.Password` badged `Database` and `GitHub.ClientSecret`
    badged `AzureKeyVault`.
  - Labels from: `Views/Store/Index.cshtml:122` (`Moved`), `:14` (breadcrumb `Secrets`).

## Chapter 07: Before you retire a store

- `care-1` (slide, clip `07-retire`): four cards with the heading "Before you retire a store"; the first two are bright.
- `care-2` (slide, clip `07-retire`): the same slide, the last two cards bright.

## Chapter 99: closing card

`outro`: the closing card. Nothing to record.

## Checks for private data

- The only account on screen is `admin`; the only addresses are `admin@example.com`, `noreply@example.com` and the host
  `smtp.example.com`. No password box is ever filled on camera, and none is readable.
- Look at every contact sheet: the Secrets list, the Settings pages and the Secret Stores page must show only the fake
  names above (`Smtp.Password`, `GitHub.ClientSecret`, `demo-client-id`). The capture has no address bar, so `localhost`
  does not appear; if the emulator's address appears in an error message, record the clip again.
- OCR the video at one frame per second and at every scene change, and search for anything outside `example.com`,
  `orchardcore.net` and the fake names; never publish a frame with an error message or a stack trace.
- Do not commit `state.json`, the `App_Data` snapshot, the emulator's data, the throwaway host project (it holds the
  emulator's placeholder credentials) or the password.
