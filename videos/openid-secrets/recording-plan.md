# Recording plan: openid-secrets

How to record the video without deciding anything. Read `OPEN-QUESTIONS.md` first: its first question must be answered
on a running site before any clip is recorded.

In this plan a selector stands for `scene.Page.Locator(...)`, and a source reference such as `OpenId/Views/X.cshtml:5`
is a line of the Orchard Core source at commit `92db4a83` (`src/OrchardCore.Modules/OrchardCore.<Module>/...`):

| Short name | Folder under `src/OrchardCore.Modules` |
| --- | --- |
| `OpenId/` | `OrchardCore.OpenId/` |
| `Secrets/` | `OrchardCore.Secrets/` |
| `Features/` | `OrchardCore.Features/` |
| `TheAdmin/` | `src/OrchardCore.Themes/TheAdmin/` |

Every label below is written as the code spells it. The admin menu and the toast markup come from `TheAdmin/Views`.

## The site it needs

- **Site**: a local Orchard Core site, `src/OrchardCore.Cms.Web` of a checkout at commit `92db4a83` or a later `main`,
  on `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). It is not a
  video about the Setup screen: record with a signed-in session (`state.json`, git-ignored).
- **Auto Setup** (environment variables, see the Auto Setup documentation page): `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`,
  `...SiteName=Orchard Core`, `...SiteTimeZone=America/Los_Angeles`, `...AdminUsername=admin`,
  `...AdminEmail=admin@example.com`, `...AdminPassword=<password>`, `...DatabaseProvider=Sqlite`,
  `...RecipeName=Blog`. The password is chosen outside git and also passed to the recorder as `VIDEO_PASSWORD`
  (`VIDEO_USER_NAME=admin`). Write nothing else down about it.
- **Tenant**: Default only. No second tenant, no external service. Block outbound calls with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address.
- **Features to enable before recording** (outside the capture, on **Tools > Features**): "OpenID Connect Authorization
  Server" and "Secrets". Do **not** enable "OpenID Connect Secrets": clip `02-enable` does it. Enabling the first one also
  enables "OpenID Connect Core Services" and "OpenID Connect Management UI", which stay enabled.
- **Settings**: none. Do not open or save **Settings > OpenID Connect > Authorization Server** before the take. If it was saved
  with a signing or encryption certificate selected, enabling "OpenID Connect Secrets" would create secrets
  (`OpenId/Secrets/Migrations.cs:34-87`) and the Secrets list would not start empty.
- **Seed data**: none. Both secrets are created on camera by clip `03-create`.
- **Starting state to check after the preparation**: **Tools > Security > Secrets** exists and says "Nothing here! There are
  no secrets stored. Click 'Add Secret' to create one."; **Settings > OpenID Connect > Authorization Server** exists; the
  Features page lists "OpenID Connect Secrets" with an Enable button.
- **Certificates on the recording machine**: the Authorization Server page builds its certificate drop-downs, under "Advanced
  Options", from every certificate with a private key in the machine's certificate stores
  (`OpenId/Drivers/OpenIdServerSettingsDisplayDriver.cs:74-89`). A certificate's friendly name, issuer and subject can hold
  a real name. Never open, focus or select these lists (Encryption Certificate Store Location, Encryption Certificate
  Store Name, Encryption Certificate, Signing Certificate Store Location, Signing Certificate Store Name, Signing
  Certificate). Closed, they show "None" (or a warning if the machine has no such certificate). Prefer a recording
  machine, or a user profile, with no personal certificate.

## Snapshots, and which clips change the site

Stop the site, copy its `App_Data` folder and `state.json` aside, and start it again.

| Snapshot | Taken | Restore it before |
| --- | --- | --- |
| S0 | after the preparation above | a full take, or `02-enable` alone |
| S1 | after `02-enable` | `03-create` alone |
| S2 | after `03-create` | `04-select` alone |

| Clip | Changes the site | Depends on |
| --- | --- | --- |
| `02-enable` | enables the feature "OpenID Connect Secrets" (the tenant reloads) | S0 |
| `03-create` | creates the secrets `ServerSigningKey` and `ServerEncryptionKey` in the Database store | S1 (the Secrets list must be empty) |
| `04-select` | saves the server settings with both secrets selected (the tenant reloads) | S2: the card "RSA Keys from Secrets" exists only after `02-enable`, and its lists offer only the secrets of `03-create` |

Record in the order they play: `02-enable`, `03-create`, `04-select`. The slides (`01-why`, `05-carry`) and the cards need
no recording. To record `03-create` again without restoring S1, delete the two secrets first (**Tools > Security >
Secrets**, Delete on each row). The recorder's scenes for the three clips (`recorder/Scenes.cs`, one method per clip, named
like the clips) are written when the video is recorded.

## General rules for the three clips

- Navigate only with `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`.
- Success messages are toasts that hide after 5 seconds (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`;
  `TheAdmin/Views/Message.cshtml`, `.toast.message-success` in `.oc-notify-toast-container`, top right). When a step says a
  message must be visible at the end of the narration, run the click so that the page has reloaded less than 4 seconds
  before the narration ends; add a `PauseAsync` before the click if a take shows the toast gone.
- The admin menu is the `ul#adminMenu` in `nav#left-nav` (`TheAdmin/Views/Navigation-admin.cshtml:9-26`). Top-level items are
  groups (`li.nav-group.has-items`): click the group's title to expand it, then the link. Confirm each menu selector with
  `Recorder probe` before the first take. If the menu cannot be driven reliably, `NavigateAsync` to the address given in the
  step while the narration says the path (the routes are fixed in the controllers named below).
- Clear highlights (`ClearHighlightsAsync`) before every scroll.
- Never type a password, a token or a key on camera. The only typed values are the secret names and descriptions below.

## Clip `02-enable`

Page it starts on: `/Admin` (the dashboard), signed in as `admin`, nothing expanded in the admin menu. Fixed address of the
Features page: `/Admin/Features` (`Features/Controllers/AdminController.cs:58`).

- `enable-1` (narration: Open Tools, Features, and search for OpenID.)
  - Actions: `ClickAsync` the menu group "Tools" (`#adminMenu` item whose title is "Tools"); `ClickAndWaitForNavigationAsync`
    the link "Features" under it (`Features/AdminMenu.cs:26-31`); wait for the heading "Features" of the page
    (`Features/Views/Admin/Features.cshtml:15`). `TypeAsync` into `#search-box` (placeholder "Search",
    `Features/Views/Admin/Features.cshtml:27`) the text `OpenID`. Wait until the group heading "OpenID Connect"
    (`h3`, `Features.cshtml:88`) is shown and the rows of the other categories are hidden.
  - Visible when the narration ends: the search box holding `OpenID`, and the "OpenID Connect" category with its feature
    rows, among them "OpenID Connect Secrets". (The rows named "OpenID Connect Core Services" are hidden by the default
    visibility filter, "Hide on demand".)
  - Labels: Tools, Features (`Features/AdminMenu.cs:26-27`); category "OpenID Connect" (`OpenId/Manifest.cs:55-63`, `Category`).
- `enable-2` (narration: the description, and the badges of the direct dependencies.)
  - Actions: `HighlightAsync` on the row `[data-filter-value*='OrchardCore.OpenId.Secrets']`
    (`Features/Drivers/FeatureEntryDisplayDriver.cs:20`), with no caption. `MoveToAsync` the badges in that row, one then the
    other, if time allows.
  - Visible when the narration ends: a green ring around the row of "OpenID Connect Secrets" (the name in bold with its
    identifier beside it, the description "Allows storing OpenID Connect signing and encryption keys as secrets.", the
    badges "OpenID Connect Authorization Server" and "Secrets", and the Enable button).
  - Labels: description `OpenId/Manifest.cs:57`; badges `Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-17` (the
    names of the direct dependencies: `OpenId/Manifest.cs:58-62`, the features named "OpenID Connect Authorization Server" in
    `OpenId/Manifest.cs:44` and "Secrets" in `Secrets/Manifest.cs:10`; `Features/Controllers/AdminController.cs:152`. The features
    they pull in, "OpenID Connect Core Services" and "OpenID Connect Management UI", are behind the ellipsis button (tooltip "Show
    indirect dependencies", `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:20-40`): do not click it);
    the row `Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`, `FeatureEntry.Description.SummaryAdmin.cshtml:8`.
- `enable-3` (narration: Select Enable. A message confirms that the feature was enabled, and its row shows the Enabled badge.)
  - Actions: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` the Enable button of that row,
    `#btn-enable-OrchardCore_OpenId_Secrets` (the id is built from the feature id,
    `Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; confirm it with `Recorder probe`). The tenant reloads, so wrap the
    wait for the redirect in `OffCameraAsync`, waiting for the toast text "The feature OpenID Connect Secrets was enabled."
    (`Features/Controllers/AdminController.cs:317`). Then `TypeAsync` `OpenID` into `#search-box` again, since the page has been
    loaded again, and wait for the row to be shown.
  - Visible when the narration ends: the success toast (top right) and, in the filtered list, the row of "OpenID Connect
    Secrets" with the "Enabled" badge and no Enable button.
  - Labels: Enable `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; Enabled `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

## Clip `03-create`

Page it starts on: `/Admin` (the dashboard), nothing expanded in the admin menu. Fixed addresses: Secrets list
`/Admin/Secrets/Index`, the editor `/Admin/Secrets/Create?type=RsaKeySecret`
(`Secrets/Controllers/AdminController.cs:19`, the route `Secrets/{action}/{name?}`).

- `create-1` (narration: Open Tools, Security, Secrets, and select Add Secret. The dialog lists the secret types. RSA Key stores
  cryptographic keys for signing, encryption, or verification, so select Create on its card.)
  - Actions: `ClickAsync` the menu group "Tools", then "Security", then `ClickAndWaitForNavigationAsync` the link "Secrets"
    (the item with id `secrets`, `Secrets/AdminMenu.cs:20-39`). Wait for the text "Nothing here!". `ClickAsync` the button
    "Add Secret" (`button.create`, `Secrets/Views/Admin/Index.cshtml:39`); wait for the dialog "Select Secret Type" (`#modalSecretTypes`,
    `Index.cshtml:200`) to be fully shown. `HighlightAsync` the card whose title (`h5.card-title`) is "RSA Key"; then, as the last
    action, `ClickAndWaitForNavigationAsync` the link "Create" of that card (`#modalSecretTypes .card:has(h5:text-is('RSA Key')) a`,
    `Index.cshtml:214`).
  - Visible when the narration ends: the editor page of the RSA Key (breadcrumb "New RSA Key" after "Secrets"), or, if the
    click lands late, the dialog with the highlighted RSA Key card and its Create button under the cursor. Both are fine.
  - Labels: Tools, Security, Secrets `Secrets/AdminMenu.cs:20-21,33`; Add Secret `Index.cshtml:39`; Select Secret Type
    `Index.cshtml:200`; card title "RSA Key" and text "Stores RSA cryptographic keys for signing, encryption, or verification."
    `Secrets/Providers/RsaKeySecretTypeProvider.cs:14,16`; Create `Index.cshtml:214`; the empty-list text `Index.cshtml:179`.
- `create-2` (narration: Enter a name... Let's call this one ServerSigningKey.)
  - Actions: `TypeAsync` into `#Name` the text `ServerSigningKey`; `HighlightAsync` the hint under the field.
  - Visible when the narration ends: the field "Name" holding `ServerSigningKey`, the hint "A unique name used by the settings
    that reference this secret, for instance Smtp.Password. It cannot be changed later." highlighted, and the "Store" row
    reading "Database" in plain text (with a single writable store the store is displayed, not chosen).
  - Labels: Name `Secrets/Views/Admin/Edit.cshtml:24`, hint `Edit.cshtml:28`; Store `Edit.cshtml:42,55`; title "New RSA Key"
    `Edit.cshtml:6` with `RsaKeySecretTypeProvider.cs:14`.
- `create-3` (narration: Key size ... leave it at 2048 bits. The description and the expiration date are optional ... Select Save.)
  - Actions: `ClearHighlightsAsync`; `HighlightAsync` the "Key size" list (`#RsaKeySize`, left on "2048 bits"; do not open it);
    `TypeAsync` into `#Description` the text `Signing key of the OpenID Connect server.`; `MoveToAsync` the "Expiration date"
    field (`#ExpiresUtc`) and leave it empty; `ClickAndWaitForNavigationAsync` the button "Save" (`button.save`,
    `Edit.cshtml:213`). Wait for the toast "Secret created successfully." (`Secrets/Controllers/AdminController.cs:313`).
  - Visible when the narration ends: the Secrets list with the row `ServerSigningKey` and the success toast.
  - Labels: Key size `Edit.cshtml:84`, "2048 bits" `Edit.cshtml:87`, the hint "A new RSA key pair is generated with this key size." `Edit.cshtml:92`;
    Description `Edit.cshtml:195`; Expiration date and its hint `Edit.cshtml:204,208`; Save `Edit.cshtml:213`.
- `create-4` (narration: Create a second RSA Key secret the same way, for the encryption key. Name it ServerEncryptionKey ... select Save.)
  - Actions, kept short (the narration lasts about 10 seconds): `ClickAsync` "Add Secret"; `ClickAndWaitForNavigationAsync` the
    "Create" link of the "RSA Key" card; `TypeAsync` into `#Name` the text `ServerEncryptionKey` (typing delay 40 ms);
    `ClickAndWaitForNavigationAsync` "Save". If a take overruns, shorten the typing delay, never skip a click.
  - Visible when the narration ends: the Secrets list with two rows, `ServerEncryptionKey` and `ServerSigningKey`, and the success toast
    "Secret created successfully." (hidden after 5 seconds: see the general rules).
  - Labels: as in `create-1` to `create-3`; the list order is by name (`AdminController.cs:113`).
- `create-5` (narration: The Secrets list shows both secrets with badges for their type ... Never expires.)
  - Actions: `ClearHighlightsAsync`; `HighlightAsync` the badges of the first row, "RSA Key", "Database" and "Never expires", then
    those of the second row.
  - Visible when the narration ends: both rows with the badges "RSA Key", "Database", "Never expires" and a badge with the time since
    the secret was updated; no expiration warning above the list.
  - Labels: type badge `Index.cshtml:109` with `RsaKeySecretTypeProvider.cs:14`; store badge `Index.cshtml:114` (the store is named
    "Database": `src/OrchardCore/OrchardCore.Secrets.Core/Services/DatabaseSecretStore.cs:37`); "Never expires" `Index.cshtml:139`.

## Clip `04-select`

Page it starts on: `/Admin` (the dashboard), nothing expanded in the admin menu. Fixed address of the server settings:
`/Admin/OpenId/ServerConfiguration` (`OpenId/Controllers/ServerConfigurationController.cs:49`).

- `select-1` (narration: Open Settings, OpenID Connect, Authorization Server, and scroll to the RSA Keys from Secrets card ...)
  - Actions: `ClickAsync` the menu group "Settings", then the item "OpenID Connect" (id `openid`), then
    `ClickAndWaitForNavigationAsync` the link "Authorization Server" (`OpenId/ServerAdminMenu.cs:18-22`). Wait for the breadcrumb
    "Configure OpenID Connect server settings" (`OpenId/Views/ServerConfiguration/Index.cshtml:4`). `ScrollAsync(0, ...)` in steps of
    about 600 pixels, without stopping on the "Advanced Options" certificate lists and without clicking in them, until the
    heading `h5` "RSA Keys from Secrets" and the whole card are on screen (the card is the last block of the page, above the Save
    button: `OpenId/Secrets/Drivers/OpenIdSecretSettingsDisplayDriver.cs:51`, `Content:3`). `HighlightAsync` the card.
  - Visible when the narration ends: the card "RSA Keys from Secrets" highlighted, with its text "Use RSA keys stored in Secrets instead
    of managed certificates. When set, these take precedence over the certificate store options.", and both lists "Signing Key
    Secret" and "Encryption Key Secret" showing "None".
  - Labels: Settings, OpenID Connect, Authorization Server `OpenId/ServerAdminMenu.cs:18-22`; card title and text
    `OpenId/Views/OpenIdSecretSettings.Edit.cshtml:5,8`; labels `OpenIdSecretSettings.Edit.cshtml:13,27`; "None" `Secrets/ViewComponents/SelectSecretViewComponent.cs:49`.
- `select-2` (narration: Signing Key Secret lists the RSA Key and X.509 Certificate secrets. Its hint asks you to pick one to use for signing JSON Web Tokens. Select ServerSigningKey.)
  - Actions: `ClearHighlightsAsync`; `SelectAsync` `#SigningKeySecretName` with the value `ServerSigningKey`; `HighlightAsync` the hint under it.
  - Visible when the narration ends: "Signing Key Secret" showing `ServerSigningKey`, and the hint "Select an RSA key or X.509 certificate
    secret to use for signing JWT tokens."
  - Labels: `OpenIdSecretSettings.Edit.cshtml:13,21`; the list contents are the secrets of the types RsaKeySecret and X509Secret
    (`OpenIdSecretSettings.Edit.cshtml:15`, `Secrets/ViewComponents/SelectSecretViewComponent.cs:36-45`).
- `select-3` (narration: Encryption Key Secret works the same way, and its hint asks you to pick one to use for encrypting access tokens. Select ServerEncryptionKey.)
  - Actions: `ClearHighlightsAsync`; `SelectAsync` `#EncryptionKeySecretName` with the value `ServerEncryptionKey`; `HighlightAsync` its hint.
  - Visible when the narration ends: both lists filled (`ServerSigningKey`, `ServerEncryptionKey`), and the hint "Select an RSA key or X.509
    certificate secret to use for encrypting access tokens."
  - Labels: `OpenIdSecretSettings.Edit.cshtml:27,35`.
- `select-4` (narration: Select Save. The page warns ... a message confirms ... The card keeps both secrets selected.)
  - Actions: `ClearHighlightsAsync`; `ScrollAsync` down to the "Save" button (`OpenId/Views/ServerConfiguration/Index.cshtml:17`);
    `ClickAndWaitForNavigationAsync` it. The tenant reloads: wrap the wait in `OffCameraAsync`, waiting for the toast text
    "OpenID server configuration successfully updated." (`OpenId/Controllers/ServerConfigurationController.cs:96`). The reloaded page opens at the top,
    where the warning "The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the
    process." (`ServerConfiguration/Index.cshtml:11`) is shown: hold there for about 2 seconds, with `HighlightAsync` on the warning, then
    `ClearHighlightsAsync` and `ScrollAsync` to the card, within the 5 seconds the toast stays visible, or accept that the toast has hidden
    by the time the card is shown (the narration then ends on the card).
  - Visible when the narration ends: the card "RSA Keys from Secrets" with `ServerSigningKey` and `ServerEncryptionKey` selected.
  - Labels: Save `ServerConfiguration/Index.cshtml:17`; the warning `ServerConfiguration/Index.cshtml:11`; the toast
    `ServerConfigurationController.cs:96`; the selected values come from `OpenId/Secrets/Drivers/OpenIdSecretSettingsDisplayDriver.cs:45-50`.

## Slides and cards (drawn by `slides.py` and the cards of the assembler, no recording)

- `intro` (opening card): the logo, the title "OpenID Connect Keys as Secrets" and the subtitle.
- `why-1`: the slide "The default: managed certificates", with the cards "Generated by the server" and "Saved as files" bright, and
  "Not shared between servers" dimmed.
- `why-2`: the same slide, with "Not shared between servers" bright and the other two dimmed.
- `why-3`: the slide "The feature: keys as secrets", with four check cards: "Encrypted", "Listed in one place", "Kept in a secret store",
  "Shared through the store".
- `carry-1`: the slide "A certificate that is already selected", with three numbered cards: a certificate is selected, an X.509
  Certificate secret, selected in the card.
- `carry-2`: the slide "What stays in use", with two cards: "Nothing selected" and "A secret that cannot be loaded".
- `outro` (closing card): the title and "docs.orchardcore.net".

## Checks for private data

- Before the take: no secret exists, the machine's personal certificates are not shown (see "Certificates on the recording machine").
- After the take, look at every contact sheet, then OCR the video at one frame per second, at every scene change and at five frames per
  second, and search the text for real names, email addresses (only `admin@example.com` may appear, in the user menu), host names, file paths of
  this machine, certificate subjects and thumbprints. Pay attention to the whole **Authorization Server** page, the "Advanced Options" part
  included.
- The secret values are never displayed: the RSA key pair stays in the database. Nothing else is typed on camera than the two names and the description above.
- The password is never typed on camera: the session comes from `state.json`.
