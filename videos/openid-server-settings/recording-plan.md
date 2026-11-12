# Recording plan: OpenID Connect Server: Flows, Endpoints and Keys

For the person who records this video. Follow it as written: nothing here needs a decision. `OPEN-QUESTIONS.md` lists
what could not be checked without a running site; read it first, and note the answers when you find them.

In this plan a **selector** stands for `scene.Page.Locator(...)`, and a **label** for
`scene.Page.GetByLabel("<label>", new() { Exact = true })`: the recorder's methods take locators. File and line
references are in the Orchard Core source (the commit in `catalog/SOURCE.md`), under
`src/OrchardCore.Modules/` unless a path starts with `src/`.

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore` at the commit in `catalog/SOURCE.md`, run on `http://localhost:5010` |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup through environment variables (see `references/recording.md` and the `orchardcore-tester` skill): recipe **Blog**, tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`; `VIDEO_USER_NAME=admin`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video needs no mail, SMS or payment provider |
| External services | None, so no local stand-in |
| Features to enable before recording | None. Clip `02-enable` enables **OpenID Connect Authorization Server** on camera. Do not enable **OpenID Connect Secrets**, **OpenID Connect Client Integration** or **OpenID Connect Token Validation**: Secrets adds a section to the page this video shows |
| Settings to save before recording | None. The server settings must never have been saved before clip `03-endpoints`: the page shows its defaults only until the first save (`OrchardCore.OpenId/Services/OpenIdServerService.cs:61-80`) |
| Machine certificates | The machine that runs the site must hold **at least one certificate that has a private key and is not archived**, in any store the account can read, for example the ASP.NET Core HTTPS development certificate in the current user's personal store (check with `dotnet dev-certs https --check`; create it with `dotnet dev-certs https`). With none, the page changes: see the next section |
| Seed data | None. The video shows no content items, users or applications besides the Blog recipe's own content, which is never on screen |
| Admin culture | `en-US`, light color scheme (the recorder's defaults) |

### Certificates: the one privacy risk

The certificate drop-downs of the page (`Views/OpenIdServerSettings.Edit.cshtml:269-298` and `341-370`) list the
certificates **of the machine that runs the site**, with their friendly name, dates and subject. They can show real
names. They stay hidden until a store location **and** a store name are both chosen (`Assets/ts/openid-server-settings.ts:35-107`).

- Never choose a store name on camera, never open a certificate list, and never leave one chosen in a clip.
- Run the site under an account whose certificate stores hold nothing personal besides the development certificate named
  above, or on a machine of its own.
- The recorder opens the page with both store locations at **None**, where the lists cannot be seen.
- **With no usable certificate the page looks different.** The certificate list is drawn only when at least one
  certificate is available (`Views/OpenIdServerSettings.Edit.cshtml:267` and `:339`). Otherwise a yellow warning
  (**A certificate must be added to your server to set up the OpenID Connect module.** for encryption, **You need to add
  a certificate to your server for setting up OpenID Connect module.** for signing) takes its place, and the script
  returns early (`Assets/ts/openid-server-settings.ts:36` and `:73`), so the **Encryption Certificate Store Name** and
  **Signing Certificate Store Name** lists stay visible while the location is **None**. The clip `06-advanced` is
  recorded for the case with a certificate; if you see those warnings, install the development certificate and record
  again.

## 2. Snapshots, order, and which clips change the site

Record in this order. Each clip starts from the snapshot shown.

| Order | Clip | Starts from | Changes the site? | Leaves |
|---|---|---|---|---|
| 1 | `02-enable` | Snapshot **A** (Auto Setup done, nothing else) | **Yes**: enables the feature (and the features it depends on) | Snapshot **B** |
| 2 | `03-endpoints` | **B** | No | **B** |
| 3 | `04-flows` | **B** | No: it changes the form and reloads it without saving | **B** |
| 4 | `05-global` | **B** | No: the save fails validation, so nothing is stored | **B** |
| 5 | `06-advanced` | **B** | No: it changes the form and never saves | **B** |
| 6 | `07-save` | **B** | **Yes**: saves the server settings | Snapshot **C** |

1. After Auto Setup, stop the site, copy its `App_Data` folder (**A**), start it, and run `Recorder setup`.
2. Record `02-enable`. Stop the site and copy `App_Data` again (**B**). Start the site.
3. Record `03-endpoints`, `04-flows`, `05-global` and `06-advanced` in any order, as many times as needed: they
   leave the site in **B**. Confirm that before each take by opening **Settings, OpenID Connect, Authorization Server**:
   the seven endpoints and **Require logout confirmation** are selected, **Allow Authorization Code Flow** and **Allow
   Refresh Token Flow** are selected, and the other flows are cleared.
4. Record `07-save` last. To record it again, stop the site, restore **B**, and start it.
5. To record `02-enable` again, restore **A**.

No clip depends on another one's recording, only on the site state shown above. The video plays the clips in the
order of the table.

No `PrepareAsync` is needed: nothing is seeded.

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/openid-server-settings/recorder
export VIDEO_ROOT=<repo>/videos/openid-server-settings VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup
<bin>/Recorder record 02-enable
```

## 3. Names used below

The page is **Settings, OpenID Connect, Authorization Server**, at `/Admin/OpenId/ServerConfiguration`
(`OrchardCore.OpenId/Controllers/ServerConfigurationController.cs:49`). Its title is **Configure OpenID Connect server
settings** (`Views/ServerConfiguration/Index.cshtml:4`).

The checkboxes and lists of that page have these locators. The `data-openid-field` attributes are in
`Views/OpenIdServerSettings.Edit.cshtml`; the other fields are found by label. The element ids (probably
`OpenIdServerSettings_<Property>`) were not verified: do not rely on them.

| Name | Locator | Source line (`Views/OpenIdServerSettings.Edit.cshtml`) |
|---|---|---|
| token endpoint | selector `[data-openid-field="EnableTokenEndpoint"]` | 25 |
| authorization endpoint | selector `[data-openid-field="EnableAuthorizationEndpoint"]` | 35 |
| pushed authorization endpoint | label "Enable Pushed Authorization Endpoint" | 45 |
| end session endpoint | label "Enable End Session Endpoint" | 55 |
| logout confirmation | label "Require logout confirmation" | 65 |
| user info endpoint | label "Enable User Info Endpoint" | 75 |
| introspection endpoint | label "Enable Introspection Endpoint" | 85 |
| revocation endpoint | label "Enable Revocation Endpoint" | 95 |
| code flow | selector `[data-openid-field="AllowAuthorizationCodeFlow"]` | 113 |
| implicit flow | selector `[data-openid-field="AllowImplicitFlow"]` | 123 |
| hybrid flow | selector `[data-openid-field="AllowHybridFlow"]` | 133 |
| password flow | selector `[data-openid-field="AllowPasswordFlow"]` | 143 |
| refresh token flow | selector `[data-openid-field="AllowRefreshTokenFlow"]` | 153 |
| client credentials flow | selector `[data-openid-field="AllowClientCredentialsFlow"]` | 163 |
| proof key | label "Require Proof Key for Code Exchange" | 181 |
| pushed authorization requests | label "Require Pushed Authorization Requests" | 201 |
| authority | label "Authority" | 226 |
| encryption store location | selector `[data-openid-field="EncryptionCertificateStoreLocation"]` | 235 |
| encryption store name | selector `[data-openid-field="EncryptionCertificateStoreName"]` | 251 |
| signing store location | selector `[data-openid-field="SigningCertificateStoreLocation"]` | 307 |
| rolling refresh tokens | label "Disable Rolling Refresh Tokens" | 382 |
| token format | selector `[data-openid-field="AccessTokenFormat"]` | 391 |
| access token encryption | selector `[data-openid-field="DisableAccessTokenEncryption"]` | 407 |
| reference tokens | label "Use Reference Access Tokens" | 417 |
| Save | selector `.action-bar-bottom button.btn-primary` (text **Save**) | `Views/ServerConfiguration/Index.cshtml:17` |
| validation summary | selector `.validation-summary-errors` | `Views/ServerConfiguration/Index.cshtml:8` |

Two rules of the page that the clips rely on (`Assets/ts/openid-server-settings.ts:122-213`):

- A flow row is shown only while the endpoints it needs are selected. The rows start collapsed and expand when the
  page loads: **wait for the client credentials flow selector to be visible before `StartCaptureAsync()`**.
- Clearing **Enable Token Endpoint** also clears and hides the password, client credentials, authorization code,
  hybrid and refresh token flows. Selecting it again shows the rows of the password, client credentials,
  authorization code and hybrid flows, but the flows stay cleared, and the refresh token row stays hidden until one of
  the password, authorization code or hybrid flows is selected (`ts:175-189`). A reload of the page is the way back to
  the saved state.

Before every clip that opens the page, the starting step is the same: `scene.OpenAsync("/Admin/OpenId/ServerConfiguration")`,
then wait for the client credentials flow selector, then scroll to the top.

## 4. The clips

### Chapter 01: How the server is organized (slides, `01-idea`)

Nothing to record. The slides are drawn by `slides.py`.

- `idea-1`: slide "Orchard Core as an identity provider", three cards: a client application, your Orchard Core site, tokens.
- `idea-2`: slide "One settings page, three groups", three cards: Endpoints, Flows, Keys and token options.
- `idea-3`: slide "The server sets the limits", three cards: Server settings, Applications, Scopes.

### Chapter 02: Enable the server (`02-enable`)

Starts on `/Admin`, the dashboard, before the capture. Expand the **Tools** group of the admin menu first (opening it is
not part of the story). Starts from snapshot **A**. Changes the site.

#### `enable-1`

- Actions: click **Features** in the **Tools** group with `ClickAndWaitForNavigationAsync`. When the page is loaded,
  `TypeAsync` the text `OpenID` into the search box (selector `#search-box`, placeholder **Search**).
- On screen when the narration ends: the **Features** page, the search box holding `OpenID`, and a group headed
  **OpenID Connect** whose rows include **OpenID Connect Authorization Server**. **OpenID Connect Core Services** is not
  listed (it is hidden by the default filter **Hide on demand**).
- Labels: **Tools** `OrchardCore.Features/AdminMenu.cs:26`; **Features** `AdminMenu.cs:27`; **Search**
  `Views/Admin/Features.cshtml:27`; the group heading is the feature category, `Views/Admin/Features.cshtml:88`, and the
  category is `OrchardCore.OpenId/Manifest.cs:46`.

#### `enable-2`

- Actions: click **Enable** in the row of **OpenID Connect Authorization Server** with `ClickAndWaitForNavigationAsync`.
  Selector `#btn-enable-OrchardCore_OpenId_Server` (the id is `btn-enable-` plus the feature id with dots replaced by
  underscores, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). If the button is not found, run
  `Recorder probe /Admin/Features` and use the **Enable** link inside the row whose title is exactly **OpenID Connect
  Authorization Server**: the descriptions of other rows mention the same words, so match the title, not the row text.
  The site reloads its configuration: wait for the message below, with `OffCameraAsync`, up to 60 seconds.
- On screen when the narration ends: the **Features** page with the success message **The feature OpenID Connect
  Authorization Server was enabled.** and the row of **OpenID Connect Authorization Server** marked **Enabled**.
- Labels: **Enable** `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the message `OrchardCore.Features/Controllers/AdminController.cs:317`;
  the feature name and its dependencies on **OpenID Connect Core Services** and **OpenID Connect Management UI**
  `OrchardCore.OpenId/Manifest.cs:42-52`.

#### `enable-3`

- Actions: in the admin menu, click **Settings** to expand it, then the **OpenID Connect** item inside it; `HighlightAsync`
  the **Authorization Server** item. `ClearHighlightsAsync`. Click **Access Control**, then the **OpenID Connect** item
  inside **Access Control** (two items are called **OpenID Connect**: pick the one inside each group, never by the
  shared id `openid`); `HighlightAsync` **Applications**, then **Scopes**.
- On screen when the narration ends: both groups expanded, showing **Authorization Server** under **Settings, OpenID
  Connect**, and **Applications** and **Scopes** under **Access Control, OpenID Connect**.
- Labels: **Settings**, **OpenID Connect**, **Authorization Server** `OrchardCore.OpenId/ServerAdminMenu.cs:18-22`;
  **Access Control**, **Applications**, **Scopes** `OrchardCore.OpenId/ManagementAdminMenu.cs:18-27`.

After this clip, stop the site and save snapshot **B**.

### Chapter 03: Endpoints (`03-endpoints`)

Starts on `/Admin` (the dashboard), before the capture, with **Settings** and its **OpenID Connect** item expanded in the
admin menu. Starts from **B**. Does not change the site.

#### `endpoints-1`

- Actions: click **Authorization Server** in the menu with `ClickAndWaitForNavigationAsync`. Wait for the client
  credentials flow selector to be visible. `HighlightAsync` the first warning (selector `.alert.alert-warning`, first
  match) with no caption.
- On screen when the narration ends: the page titled **Configure OpenID Connect server settings**, the yellow warning
  **The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the
  process.** ringed, and the **Endpoints** heading below it. (The same warning is probably shown once, because the page
  renders only the content zone of the form, not the wrapper that would repeat it: see `OPEN-QUESTIONS.md`.)
- Labels: `OrchardCore.OpenId/Views/ServerConfiguration/Index.cshtml:4` and `:10-12`; **Endpoints** `Views/OpenIdServerSettings.Edit.cshtml:17`.

#### `endpoints-2`

- Actions: `ClearHighlightsAsync`. `ScrollAsync(0, 150)` (adjust so that the **Endpoints** heading is at the top of the
  page area and the first flows are visible below). `HighlightAsync` the token endpoint checkbox, then the code flow and
  the refresh token flow checkboxes, without caption.
- On screen when the narration ends: the seven endpoint checkboxes and **Require logout confirmation** selected, **Allow
  Authorization Code Flow** and **Allow Refresh Token Flow** selected, the other flows cleared. If anything else is selected or cleared, the site is not in
  snapshot **B**: restore it.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:25-95` and `:113-163`; the defaults come from
  `OrchardCore.OpenId/Services/OpenIdServerService.cs:61-80`.

#### `endpoints-3`

- Actions: `ClearHighlightsAsync`. `MoveToAsync` the address shown beside **Enable Token Endpoint** (selector
  `[data-openid-field="EnableTokenEndpoint"] ~ code`), then `HighlightAsync` the whole token endpoint row.
- On screen when the narration ends: **Enable Token Endpoint** selected, with the text **Enables the endpoint:** and its
  address beside it, ringed.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:25-26`.

#### `endpoints-4`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the authorization endpoint row, hold, `ClearHighlightsAsync`, then
  `HighlightAsync` the pushed authorization endpoint row.
- On screen when the narration ends: **Enable Pushed Authorization Endpoint** ringed, **Enable Authorization Endpoint**
  just above it.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:35` and `:45`; the dependency is the check at
  `OrchardCore.OpenId/Services/OpenIdServerService.cs:232-238`.

#### `endpoints-5`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the end session endpoint row, hold, `ClearHighlightsAsync`, then
  `HighlightAsync` the logout confirmation row including its hint.
- On screen when the narration ends: **Require logout confirmation** ringed, selected, with the hint **When unchecked,
  end-users are signed out without a confirmation prompt if a valid id_token_hint matching the current session is
  provided.** beside it.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:55` and `:65-66`; the behavior is
  `OrchardCore.OpenId/Controllers/AccessController.cs:343-359` and the confirmation page `Views/Access/Logout.cshtml`.

#### `endpoints-6`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the user info endpoint row, then the introspection endpoint row, then
  the revocation endpoint row, in that order, one second each.
- On screen when the narration ends: the last three endpoint rows ringed one after another; the last one, **Enable
  Revocation Endpoint**, is ringed at the end.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:75`, `:85`, `:95`; the user info rule is
  `OrchardCore.OpenId/Controllers/UserInfoController.cs:57-69`.

### Chapter 04: Flows (`04-flows`)

Starts on `/Admin/OpenId/ServerConfiguration`, opened before the capture as described in section 3, then scrolled so that
**Enable Token Endpoint** is at the top of the page area and **Allow Client Credentials Flow** is still visible (if both
do not fit, scroll so the endpoint is just visible at the top). Starts from **B**. Does not change the site.

#### `flows-1`

- Actions: `HighlightAsync` the **Flows** heading (selector: the `h3` whose text is **Flows**), hold two seconds.
- On screen when the narration ends: the **Flows** heading ringed, with the six flow checkboxes under it: **Allow
  Authorization Code Flow**, **Allow Implicit Flow**, **Allow Hybrid Flow**, **Allow Password Flow**, **Allow Refresh Token
  Flow**, **Allow Client Credentials Flow**.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:105-163`.

#### `flows-2`

- Actions: `ClearHighlightsAsync`. `ClickAsync` the token endpoint checkbox (it becomes cleared). Wait until the password
  flow row is hidden (`WaitForAsync` with state Hidden on the closest `.ocat-wrapper` of the password flow selector), up to
  5 seconds. `HighlightAsync` the implicit flow checkbox.
- On screen when the narration ends: **Enable Token Endpoint** cleared; under **Flows** only **Allow Implicit Flow**
  remains, ringed. The other five flow rows are hidden.
- Labels and rules: `Views/OpenIdServerSettings.Edit.cshtml:25`, `:123`; `Assets/ts/openid-server-settings.ts:122-213`.

#### `flows-3`

- Actions: `ClearHighlightsAsync`. `NavigateAsync("/Admin/OpenId/ServerConfiguration")` (the reload restores the saved
  state; do not save anything). Wait for the client credentials flow selector, then scroll so that the flows are
  visible. `HighlightAsync` the code flow checkbox.
- On screen when the narration ends: all six flow rows back, **Allow Authorization Code Flow** selected and ringed.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:113`.

#### `flows-4`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the implicit flow row, hold, `ClearHighlightsAsync`, then
  `HighlightAsync` the hybrid flow row.
- On screen when the narration ends: **Allow Hybrid Flow** ringed, cleared.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:123` and `:133`; the response types they allow are in
  `OrchardCore.OpenId/Configuration/OpenIdServerConfiguration.cs:141-178`.

#### `flows-5`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the password flow row, hold, `ClearHighlightsAsync`, then
  `HighlightAsync` the refresh token flow row.
- On screen when the narration ends: **Allow Refresh Token Flow** ringed, selected.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:143` and `:153`; the rule about the refresh token flow is
  `OrchardCore.OpenId/Services/OpenIdServerService.cs:189-206`; the password check is
  `OrchardCore.OpenId/Controllers/AccessController.cs:521-594`.

#### `flows-6`

- Actions: `ClearHighlightsAsync`. `HighlightAsync` the client credentials flow row.
- On screen when the narration ends: **Allow Client Credentials Flow** ringed, cleared.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:163`; the roles of the application are used in
  `OrchardCore.OpenId/Controllers/AccessController.cs:465-519`.

### Chapter 05: Requirements for every application (`05-global`)

Starts on `/Admin/OpenId/ServerConfiguration`, opened before the capture as described in section 3, then scrolled so that
the **Proof Key for Code Exchange** and **Pushed Authorization Requests** sections are visible. Starts from **B**. Does
not change the site: the save at the end fails validation.

#### `global-1`

- Actions: `HighlightAsync` the proof key checkbox with its hint, hold two seconds. Do not select it.
- On screen when the narration ends: the **Proof Key for Code Exchange** section, **Require Proof Key for Code
  Exchange** ringed, cleared, with the hint **A global setting that enforces PKCE for all registered clients, regardless
  of whether the 'Require PKCE' option is enabled for individual client applications.**
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:173-184`.

#### `global-2`

- Actions: `ClearHighlightsAsync`. `ClickAsync` the pushed authorization requests checkbox (selected). Scroll up
  (`ScrollAsync`) until the pushed authorization endpoint checkbox is visible, `ClickAsync` it (cleared). Scroll down to
  **Save** (`ScrollAsync`, if the button is not already visible), and `ClickAndWaitForNavigationAsync` it. The page
  comes back with an error. Keep the actions under the narration length; if they run over, drop the scrolling by
  scrolling once to a position that shows both checkboxes.
- On screen when the narration ends: the form again, with the validation summary at the top (see `global-3`).
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:45`, `:201`; **Save** `Views/ServerConfiguration/Index.cshtml:17`.

#### `global-3`

- Actions: `ScrollAsync` to the top of the page. `HighlightAsync` the validation summary, hold three seconds.
- On screen when the narration ends: the red validation summary with the message **The pushed authorization endpoint
  must be enabled when enforcing pushed authorization requests.**, ringed. Nothing is saved: reload the page after the
  take and check that **Require Pushed Authorization Requests** is cleared and **Enable Pushed Authorization Endpoint**
  is selected.
- Labels: the message `OrchardCore.OpenId/Services/OpenIdServerService.cs:242`; the validation summary
  `Views/ServerConfiguration/Index.cshtml:8`; the errors are added in `Controllers/ServerConfigurationController.cs:80-89`.

### Chapter 06: Keys and token options (`06-advanced`)

Starts on `/Admin/OpenId/ServerConfiguration`, opened before the capture as described in section 3, then scrolled so that
the **Advanced Options** heading is at the top of the page area. Starts from **B**. Does not change the site; nothing
is saved. **Never choose a store name, and never open a certificate list** (section 1).

#### `advanced-1`

- Actions: `HighlightAsync` the **Advanced Options** heading and the blue note under it, hold two seconds,
  `ClearHighlightsAsync`, then `HighlightAsync` the authority field and its hint. Type nothing into the field.
- On screen when the narration ends: **Authority** ringed, empty, with the hint **The base URL of the identity server
  (this site). If not provided, a default value will be automatically generated based on the site host.**
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:213-230`; the note is line 220.

#### `advanced-2`

- Actions: `ClearHighlightsAsync`. `ScrollAsync` so that **Encryption Certificate Store Location** and **Signing
  Certificate Store Location** are both visible. `HighlightAsync` the encryption store location, then the signing store
  location, each with the caption **None**.
- On screen when the narration ends: both store location lists showing **None**, and no other certificate field
  (the store name and certificate fields are hidden while the location is **None**, provided the machine holds a
  usable certificate: see section 1).
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:235-246`, `:307-318`; the generated certificates are
  `OrchardCore.OpenId/Services/OpenIdServerService.cs:284-352` (encryption), `:356-426` (signing) and `:571-624`
  (self-signed, valid for three months). A new certificate is made when none of the existing ones is valid for more
  than seven days (`:314`, `:386`); the keys are read when the server options are built (`Configuration/OpenIdServerConfiguration.cs:61-70`),
  which is when the site starts or reloads.

#### `advanced-3`

- Actions: `ClearHighlightsAsync`. `SelectAsync` the encryption store location with the value `CurrentUser`. The
  **Encryption Certificate Store Name** list appears: `HighlightAsync` it. Leave it at **None**.
- On screen when the narration ends: **Encryption Certificate Store Location** showing **CurrentUser**, and the
  **Encryption Certificate Store Name** list under it, ringed, showing **None**. No certificate list is visible.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:235`, `:251`; the rule is `Assets/ts/openid-server-settings.ts:35-107`.
  Only certificates that have a private key are listed: `OrchardCore.OpenId/Services/OpenIdServerService.cs:268`.

#### `advanced-4`

- Actions: `ClearHighlightsAsync`. `ScrollAsync` so that **Disable Rolling Refresh Tokens** is visible. `HighlightAsync`
  its row including the hint.
- On screen when the narration ends: **Disable Rolling Refresh Tokens** ringed, cleared, with the hint **When this option
  is enabled, refresh tokens are not marked as redeemed and can be reused until they expire.**
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:382-383`.

#### `advanced-5`

- Actions: `ClearHighlightsAsync`. `ScrollAsync` so that **Token Format** is visible. `HighlightAsync` the token format
  list (it shows **ASP.NET Core Data Protection**), hold two seconds, `ClearHighlightsAsync`, then `SelectAsync` the
  token format with the value `JsonWebToken` (the option **JSON Web Token (JWT)**).
- On screen when the narration ends: **Token Format** showing **JSON Web Token (JWT)**.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:389-396`; the option values come from
  `OrchardCore.OpenId/Settings/OpenIdServerSettings.cs:50-54`.

#### `advanced-6`

- Actions: `ScrollAsync` so that the last two checkboxes are visible. `HighlightAsync` the access token encryption
  checkbox (it is now enabled), hold, `ClearHighlightsAsync`, then `HighlightAsync` the reference tokens row.
- On screen when the narration ends: **Disable Access Token Encryption (only for JWT tokens)** enabled (not greyed),
  and **Use Reference Access Tokens** ringed, cleared, with its hint.
- Labels: `Views/OpenIdServerSettings.Edit.cshtml:407-418`; the rule is `Assets/ts/openid-server-settings.ts:109-120`.

### Chapter 07: Save the settings (`07-save`)

Starts on `/Admin/OpenId/ServerConfiguration`, opened before the capture as described in section 3, then scrolled so that
**Allow Client Credentials Flow** and the **Proof Key for Code Exchange** section are both visible. Starts from **B**.
**Changes the site**: leaves snapshot **C**.

#### `save-1`

- Actions: `ClickAsync` the client credentials flow checkbox, then `ClickAsync` the proof key checkbox. Scroll to
  **Save** if it is not visible, and `ClickAndWaitForNavigationAsync` it. Wait for the message below with
  `OffCameraAsync` (up to 60 seconds): the site reloads its configuration on save.
- On screen when the narration ends: the form after the save, with the success message. If the message is not yet
  visible, the wait was too short: record again.
- Labels: **Allow Client Credentials Flow** `Views/OpenIdServerSettings.Edit.cshtml:163`; **Require Proof Key for Code
  Exchange** `:181`; **Save** `Views/ServerConfiguration/Index.cshtml:17`.

#### `save-2`

- Actions: `ScrollAsync` to the top of the page. `HighlightAsync` the success message, hold two seconds,
  `ClearHighlightsAsync`, scroll down so that the flows and the proof key section are visible, and `HighlightAsync` the
  client credentials flow checkbox.
- On screen when the narration ends: the message **OpenID server configuration successfully updated.** was shown;
  **Allow Client Credentials Flow** is selected and ringed, with **Require Proof Key for Code Exchange** selected below
  it.
- Labels: the message `OrchardCore.OpenId/Controllers/ServerConfigurationController.cs:96`. The reload of the site is
  `:98`.

### Chapter 99: closing card (`99-outro`)

Nothing to record.

## 5. Checks for private data

Do these before the video is assembled.

- Look at the contact sheet of every clip (`review.py`). No error page, no stack trace, no empty list.
- Every clip: the only user name on screen is `admin` and the only email `admin@example.com`. The password never
  appears; the recorder types none on camera (the sign-in is in `state.json`, which is never committed).
- No certificate: no clip may show a certificate list, a friendly name, a subject, a thumbprint or a machine name. Search
  the OCR of `06-advanced` for `CN=`, `localhost`, `.local` and the name of the recording machine.
- The **Authority** field is empty in every frame. No host name appears in a field.
- `06-advanced` shows no yellow certificate warning and no **Store Name** list while a location is **None** (section 1).
- The address bar of the browser is not part of the capture; check that no frame shows a full URL.
- Do not commit `state.json`, a mask script, the audio, the clips or a copy of `App_Data`.
