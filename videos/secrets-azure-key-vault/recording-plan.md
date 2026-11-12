# Recording plan: secrets-azure-key-vault

The recording plan of "Secrets in Azure Key Vault" (`storyboard.json`). It is for the person who records the clip: it
says what the site and its stand-in for the vault must hold, what to click and type, and what must be on screen when each
narration ends. A selector below stands for `scene.Page.Locator(...)`. File references (`file:line`) are in the Orchard
Core source at commit 92db4a83, under `src/OrchardCore.Modules` unless a path starts with another folder; they tell where
a label comes from, so a changed label can be found again. A bare `README.md:<n>` is `src/docs/reference/modules/Secrets/README.md`,
a bare `AdminController.cs` is `OrchardCore.Secrets/Controllers/AdminController.cs`, a bare `Index.cshtml` or `Edit.cshtml` is in
`OrchardCore.Secrets/Views/Admin/`, a bare `AzureKeyVaultSecretStore.cs` or `Manifest.cs` is in `OrchardCore.Secrets.AzureKeyVault/`.

Narrate first (`narrate.py`), because each step is held for the length of its narration. The narration times below
(about 2.5 words per second) only help to pace the actions: the recorder takes the real ones from `durations.json`.

Only one chapter is recorded: `04-create` (chapter 04). The other chapters are slides and cards. The video shows no code
in a browser and no Azure portal: the vault is never on screen.

## The site it needs

This video needs a vault, which is an external service. A real vault would show a real Azure account, so the site talks to
a local **Azure Key Vault emulator** instead, which the Secrets documentation describes in its section "Local development
with the Azure Key Vault Emulator" (`src/docs/reference/modules/Secrets/README.md:374-424`). Everything below is off camera.

### The host is not the stock web project

The stock `src/OrchardCore.Cms.Web` registers no `SecretClient`, and the feature only resolves a client that the host
registered (`OrchardCore.Secrets.AzureKeyVault/Startup.cs:18-24`). So run a **throwaway host project**, outside both
repositories and outside the Orchard Core solution (delete it after recording):

- Its project file mirrors `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`: the same imports of
  `OrchardCore.Application.Cms.Core.Targets` and `OrchardCore.Application.Targets`, and the project reference to
  `OrchardCore.Application.Cms.Targets`, without NLog and OpenTelemetry. Fix the relative paths for its folder. The Secrets
  and Secrets - Azure Key Vault modules come with that reference (`OrchardCore.Application.Cms.Core.Targets.csproj:115-116`).
- Its `Program.cs` is the one of `src/OrchardCore.Cms.Web/Program.cs` (`AddOrchardCms().AddSetupFeatures("OrchardCore.AutoSetup")`,
  `UseStaticFiles`, `UseOrchardCore`) plus the registration of a keyed `SecretClient` under the key `SecretClient`, built by
  hand so that the emulator's certificate is accepted **in this process only** and never added to the trust store of the
  machine (`references/recording.md`). The registration is a sketch, to be confirmed on a running site (see
  `OPEN-QUESTIONS.md`, question 1):

  ```csharp
  using Azure.Core.Pipeline;
  using Azure.Identity;
  using Azure.Security.KeyVault.Secrets;

  builder.Services.AddKeyedSingleton<SecretClient>("SecretClient", (_, _) =>
  {
      var handler = new HttpClientHandler
      {
          UseProxy = false, // the recording site sets a dead proxy on purpose; the vault is local
          ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
              string.Equals(certificate?.GetCertHashString(), builder.Configuration["Emulator:CertificateThumbprint"],
                  StringComparison.OrdinalIgnoreCase),
      };
      var transport = new HttpClientTransport(new HttpClient(handler));
      var credential = new EnvironmentCredential(new EnvironmentCredentialOptions
      {
          Transport = transport,
          DisableInstanceDiscovery = true,
      });
      return new SecretClient(new Uri(builder.Configuration["Emulator:VaultUri"]), credential,
          new SecretClientOptions { Transport = transport, DisableChallengeResourceVerification = true });
  });
  ```

  It differs on purpose from the registration on the slides, which is the one of the documentation
  (`AddKeyedAzureClient`, `README.md:319-327`): a slide shows what a production host does, the stand-in only has to reach
  an emulator whose certificate is not trusted.
- The feature is configured by the host's environment, as the slides describe (`OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStoreOptionsSetup.cs:18-22`):
  `OrchardCore__Secrets__AzureKeyVault__AzureClient=SecretClient` and `OrchardCore__Secrets__AzureKeyVault__NamePrefix=myapp`.
  Set `Emulator__VaultUri` to the emulator's address (`https://localhost:<port>`), `Emulator__CertificateThumbprint` to the
  thumbprint of its certificate, and the emulator's credential variables in the host's process, with the values the
  documentation lists for a standalone emulator (`README.md:407-416`; they are public placeholders, not real credentials).
  `AZURE_AUTHORITY_HOST` points to the emulator, as the documentation says (`README.md:418`).

### The Orchard site

- A fresh site with its default tenant only, from the throwaway host, with its own `App_Data` folder outside both
  repositories (`ORCHARD_APP_DATA`), on `http://localhost:5010`, provisioned by Auto Setup through environment variables
  (`OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`,
  `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`,
  `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`; the password is the one in `VIDEO_PASSWORD`, never written in the
  repository). `HTTP_PROXY` and `HTTPS_PROXY` point to a dead address, and `NO_PROXY=localhost,127.0.0.1`.
- Outside the capture, with the recorder signed in once (`Recorder setup`, which makes the git-ignored `state.json`):
  1. In **Tools, Features**, enable **Secrets**. Do **not** enable **Secrets - Azure Key Vault**: the clip does it on camera.
  2. In **Tools, Security, Secrets**, create the one secret of the table below, in the **Database** store. With one writable
     store the **Store** row is read-only text (`OrchardCore.Secrets/Views/Admin/Edit.cshtml:39-60`), so just fill in
     **Name**, **Value** and **Description**, and select **Save**.
  3. Stop the site and copy its `App_Data` folder and `state.json` aside: this is **Snapshot A**.

### The fake data

| Secret | Type | Store | Value | Description | Created |
| --- | --- | --- | --- | --- | --- |
| `Smtp.Password` | Text Secret | Database | `fake-mail-value-1` | Password of the mail account that sends the site email | seed, outside the capture |
| `Payment.ApiKey` | Text Secret | AzureKeyVault | `fake-payment-value-1` | none | on camera, `create-2` |

No expiration date on either secret. In particular, never set a past expiration date on a secret of the vault: the module
passes the date to the vault entry (`AzureKeyVaultSecretStore.cs:106`), and the clip does not need to know what the vault
does with it. The secret values are typed into password fields and are never read aloud.

### The emulator

- Run the emulator as the documentation describes (`README.md:374-424`) and its own setup page, which the documentation
  links: in Docker, with the container's port 4997 exposed on a **fixed** host port, **without persistence**, and with its
  certificate generated into a throwaway folder. Do **not** install that certificate in the trust store: read its
  thumbprint (`Emulator__CertificateThumbprint`) and let the throwaway host pin it. If pinning does not work, see
  `OPEN-QUESTIONS.md`, question 1.
- The emulator holds the vault in memory: **every take starts with a fresh emulator container**, because restoring
  `App_Data` does not reset the vault, and the form rejects a name that exists in any store (`OrchardCore.Secrets/Controllers/AdminController.cs:295-299`,
  "A secret with this name already exists.").
- Create secrets through Orchard only, never directly in the emulator: the store needs its ownership tags and ignores other
  entries (`README.md:424`; `AzureKeyVaultSecretStore.cs:149-155`).
- Remove the container, the certificate folder and the throwaway host when the recording is finished.

### Checks before every take

1. The emulator answers on its fixed port, and the host starts without an error in its log.
2. Rehearse once from Snapshot A: enable **Secrets - Azure Key Vault**, open **Tools, Security, Secrets**, and check that the
   page loads. The page lists the secrets of every store without catching a failure, so an unreachable emulator or a wrong
   `AzureClient` name makes the whole page fail (`AdminController.cs:67`, `AzureKeyVaultSecretStore.cs:142-176`,
   `OrchardCore.Secrets.AzureKeyVault/Startup.cs:14-23`). Then create one throwaway secret in the **AzureKeyVault** store, check
   that it appears in the list with its store badge and that its Edit page opens: the store keeps its ownership data in tags,
   and ignores or rejects an entry whose tags the emulator did not keep (`AzureKeyVaultSecretStore.cs:149-155,185-191`).
   Then restore Snapshot A and restart the emulator container, so the clip finds the vault empty.
   The misconfiguration is not limited to the Secrets pages: the secret manager builds every store, and the
   expiration filter of the module asks for the secret manager on every MVC request of the tenant
   (`OrchardCore.Secrets/Startup.cs:28`, `OrchardCore.Secrets/Filters/SecretExpirationWarningFilter.cs:24-35`), so a failing
   store can make the Features page fail too; the way back is to stop the host and restore Snapshot A (`OPEN-QUESTIONS.md`, question 19).
3. The browser's culture is `en-US`.

## Order, dependencies and snapshots

One clip, so one order.

| Order | Clip | Needs | Changes the site |
| --- | --- | --- | --- |
| 1 | `04-create` | Snapshot **A** (Secrets enabled, `Smtp.Password` in the Database store, Key Vault feature disabled) and a fresh, empty emulator | Enables **Secrets - Azure Key Vault**; creates `Payment.ApiKey` in the vault |

**Before a new take:** stop the host, restore Snapshot A (`App_Data` and `state.json`), start a **new emulator container**,
start the host, and run the checks above. The slide chapters (`01-store`, `02-client`, `03-config`, `05-names`, `06-code`,
`07-local`) and the opening and closing cards are not recorded.

## Clip

Narration times are approximate. Toasts (the green messages at the top right) disappear 5 seconds after the page loads
(`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), so click the button that causes one at the
very start of the step. Where a step says "toast", the message is on screen only for the first seconds of the step.

Common locators:

| What | Selector | Source |
| --- | --- | --- |
| Sidebar | `#ta-left-sidebar`; entries by their exact text, for example `#ta-left-sidebar >> text="Tools"` | `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:76` |
| Toast | `.oc-notify-toast-container .toast` | `OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml` |
| Features search box | `#search-box` | `OrchardCore.Features/Views/Admin/Features.cshtml:27` |
| Row of a feature | `li.list-group-item:has(code:text-is("OrchardCore.Secrets.AzureKeyVault"))` | `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` |
| Enable button | `#btn-enable-OrchardCore_Secrets_AzureKeyVault`, otherwise `a.btn-primary:text-is("Enable")` inside the row | `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |
| Add Secret | `button.create` | `OrchardCore.Secrets/Views/Admin/Index.cshtml:39` |
| Create under Text Secret | `#modalSecretTypes .card:has(h5:text-is("Text Secret")) a:text-is("Create")` | `Index.cshtml:200-214` |
| Fields of the form | `#Name`, `#Store` (a drop-down list), `#TextValue`, `button.save` | `OrchardCore.Secrets/Views/Admin/Edit.cshtml:26,46,70,213` |
| A row of the Secrets list | `li.list-group-item:has(a:text-is("<name>"))` | `Index.cshtml:86-100` |
| Store badge of a row | `.badge:has(.fa-database)` inside the row | `Index.cshtml:113-115` |
| Edit of a row | `a.btn-primary:text-is("Edit")` inside the row | `Index.cshtml:156` |

The sidebar markup is generated by the theme and its ids are not fixed in the source: find the entries once with
`Recorder probe`, and expand a menu group by clicking its name. The sidebar has two entries named **Security**: one under
**Tools**, which holds the Secrets pages (`OrchardCore.Secrets/AdminMenu.cs:19-21`), and one under **Settings**, which holds
Security Headers and User Login (`OrchardCore.Security/AdminMenu.cs:26-27`, `OrchardCore.Users/AdminMenu.cs:40-41`). Scope the
locator of **Security** to the **Tools** group, because a plain text locator can match both. The id of the Enable button depends on
how the markup helper writes the feature id: confirm it with `Recorder probe` (`OPEN-QUESTIONS.md`, question 4).

The Features list is drawn in its List layout, which a fresh site and admin user have by default (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`);
the row locator above does not match the Grid layout.

### `04-create` (chapter 04, Create a secret in the vault)

Starts on: `/Admin` (the admin home), with Snapshot A and a fresh emulator. Nothing expanded on purpose. The capture starts
there.

- `enable-1` (about 10 s). Click **Tools** in the sidebar, then **Features** (`ClickAndWaitForNavigationAsync`). On the
  Features page, `TypeAsync` "Key Vault" into `#search-box`; the list filters while typing. On screen at the end: the Features page
  with "Key Vault" in the search box and one row, **Secrets - Azure Key Vault** `OrchardCore.Secrets.AzureKeyVault`, in the
  **Security** group, with the **Disabled** badge and an **Enable** button. Labels: **Tools** and **Features** `OrchardCore.Features/AdminMenu.cs:26-27`;
  search `Features.cshtml:27`; feature name `OrchardCore.Secrets.AzureKeyVault/Manifest.cs:10`; group heading from the feature's
  category `Manifest.cs:13`; search text is the name, the id and the description (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`).
- `enable-2` (about 6 s). **First action of the step:** click **Enable** in the **Secrets - Azure Key Vault** row
  (`ClickAndWaitForNavigationAsync`; enabling restarts the tenant, so the redirect to the Features page takes a moment).
  On screen during the first seconds: the Features page with the toast "The feature Secrets - Azure Key Vault was enabled."
  (`OrchardCore.Features/Controllers/AdminController.cs:317`). For "The Secrets pages are under Tools, Security", click
  **Security** in the **Tools** group of the sidebar to expand it. On screen at the end: the sidebar with **Security** expanded,
  showing **Secrets**, **Migrate Secrets** and **Secret Stores** (`OrchardCore.Secrets/AdminMenu.cs:33-53`); no other module adds
  an entry under **Tools**, **Security**.
- `create-1` (about 9 s). Click **Secrets** in the sidebar (`ClickAndWaitForNavigationAsync`): the list shows `Smtp.Password`
  with the **Database** badge. Click **Add Secret** (`ClickAsync`): the dialog **Select Secret Type** opens. Click **Create**
  under **Text Secret** (`ClickAndWaitForNavigationAsync`). On screen at the end: the page **New Text Secret** with
  the fields **Name**, **Store** (a drop-down list, preselected to the first store), **Value**, **Description**, **Expiration date**;
  draw a green ring around **Store** (`HighlightAsync(#Store)`), and clear it before the next step. The **Store** list appears
  only when two writable stores exist (`Edit.cshtml:39-47`). Labels: `Index.cshtml:39,200,214`; **Text Secret**
  `OrchardCore.Secrets/Providers/TextSecretTypeProvider.cs:14`; the page title `Edit.cshtml:6`.
- `create-2` (about 6 s). `TypeAsync(#Name, "Payment.ApiKey")`, `TypeAsync(#TextValue, "fake-payment-value-1")` (a password
  field, shown as dots), `SelectAsync(#Store, "AzureKeyVault")`, then `ClickAndWaitForNavigationAsync(button.save)` as the last
  action. Type quickly (a delay of 30 ms per character) to keep the actions shorter than the narration. On screen at the
  end: the form with **AzureKeyVault** selected, or already the list after the save, whichever the timing gives. The option text
  is the store's name (`OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStore.cs:66`). Labels: **Name** `Edit.cshtml:24`,
  **Store** `Edit.cshtml:42`, **Value** `Edit.cshtml:68`, **Save** `Edit.cshtml:213`.
- `create-3` (about 14 s). The Secrets list is on screen with the toast "Secret created successfully." (`AdminController.cs:313`)
  during its first seconds. Two rows, sorted by name: `Payment.ApiKey` (badges **Text Secret**, **AzureKeyVault**, **Never expires**,
  and a last-updated badge when the emulator returns an update time, `Index.cshtml:143`) and `Smtp.Password` (**Database**). `HighlightAsync` the store badge of `Payment.ApiKey`, then
  `ClearHighlightsAsync`, then `HighlightAsync` the store badge of `Smtp.Password`. On screen at the end: the list with the
  ring around the **Database** badge. If the save is still running at the start of this step, the pause only lengthens the step.
  Labels: `Index.cshtml:113-115,139`.
- `create-4` (about 10 s). `ClearHighlightsAsync`, then click **Edit** in the row of `Payment.ApiKey`
  (`ClickAndWaitForNavigationAsync`). The page **Edit 'Payment.ApiKey' secret** shows **Store** as plain text, **AzureKeyVault**, with the
  hint "To keep this secret in another store, use the Move action." Draw a green ring around the hint
  (`span.hint:has(a:text-is("Move"))`). On screen at the end: that page with the ring on the hint and an empty **Value** field.
  Do **not** select **Save**, and do not follow the **Move** link. Labels: `Edit.cshtml:12,56-59`; **Value** placeholder and hint
  `Edit.cshtml:70,74`.

## The slides

Not recorded; `slides.py` draws them. One line each:

- `store-1`: the `ISecretStore` interface and the `AzureKeyVaultSecretStore` class declaration, with its `Name` and `IsReadOnly`.
- `store-2`: four cards (the database store, the Key Vault store, names, existing secrets), the last two bright.
- `client-1`: a `Program.cs` that registers the keyed `SecretClient` as the documentation shows, with cards for the service key and the configuration section.
- `client-2`: the configuration with the `AzureClients` section bright, and cards for the vault address, the credential and the managed identity.
- `client-3`: an environment variable for a client secret, with two cards.
- `config-1`: the same configuration with the `OrchardCore` section bright and `AzureClient`, and two cards.
- `config-2`: the same configuration with `NamePrefix`, and two cards.
- `names-1` to `names-4`: four cards (entry name, namespace, not a boundary, deletion), one bright at a time.
- `code-1`, `code-2`: `GetSecretAsync` and `SaveSecretAsync` with the store name, one half bright at a time.
- `code-3`: a constructor that takes the shared `SecretClient` with `FromKeyedServices`.
- `local-1`, `local-2`: the configuration for an emulator (`https://localhost:4997`, an environment credential, the two flags), with cards; the two flags bright in `local-2`.

## Checks for private data

- The vault is a local emulator: no Azure account, subscription, vault address or portal is ever on screen. The slides show
  `<the URI of your vault>` and a localhost address only.
- Every secret name and value is invented (`Payment.ApiKey`, `fake-payment-value-1`). Values are in password fields.
- `admin@example.com` is the only email; the password never appears on screen.
- No error page: the host log shows no `OptionsValidationException` and no certificate error. OCR the recorded clip `04-create`
  for `AZURE_`, a GUID, `https://` and the word "Error" (the slides show `https://localhost:4997` on purpose).
- The emulator's placeholder credentials are in the host's environment only, never in a file of the repository.
