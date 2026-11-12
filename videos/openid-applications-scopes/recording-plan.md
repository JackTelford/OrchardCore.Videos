# Recording plan: OpenID Connect Applications and Scopes

How to record this video without deciding anything. The narration is in `storyboard.json`; narrate before you record
(`narrate.py`), since the recorder holds each step for the length of its narration.

Source paths are relative to `src/OrchardCore.Modules/OrchardCore.OpenId/` (written `OpenId/...`) in
`OrchardCMS/OrchardCore`, except where another module folder is named (the modules sit side by side in `src/OrchardCore.Modules/`). Line numbers are those of commit `92db4a83`. A
selector written below stands for `scene.Page.Locator(...)`. Admin URLs assume the default admin prefix `Admin`.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, built from `main` (labels verified at `92db4a83`), on `http://localhost:5010` |
| Provisioning | Auto Setup through environment variables (see `references/autosetup.md` of the `orchardcore-tester` skill). This video is not about Setup, so it is recorded with Auto Setup and a `state.json` |
| Recipe, tenant | Blog recipe, tenant `Default` |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles` |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`; never written in this repository) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no external service |
| External services | None. The video never sends a token request or a sign-in to a client, and never opens the consent page |
| Browser | The recorder's standard Chromium, 1536x864 CSS pixels, light color scheme, `en-US` |
| Admin lists | Leave the admin settings as Auto Setup leaves them: the lists use the List layout and the row actions use the Buttons layout, which shows **Edit** and **Delete** as buttons on each row (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). If the site was set to the Menu layout, the two buttons are under an **Actions** menu and the clips cannot be recorded as written. |

### Before recording (outside the capture)

Do these once, in this order, signed in as `admin` (`Recorder setup` creates `state.json`).

1. **Enable the feature.** Tools > Features (`OrchardCore.Features/AdminMenu.cs:26-27`). Search for
   `OpenID Connect Authorization Server`, select it, and use Bulk Actions > Enable
   (`OrchardCore.Features/Views/Admin/Features.cshtml:34-38`). Its dependencies, **OpenID Connect Management UI** and
   **OpenID Connect Core Services**, are enabled with it (`OpenId/Manifest.cs`, the `Dependencies` of the Server and
   Management features). Do not enable Token Validation, Client Integration or Secrets. Enabling the Server feature
   creates the four default scopes (`OpenId/Startup.cs:127`, `OpenId/Migrations/DefaultScopesMigration.cs`).
2. **Save the server settings.** Settings > OpenID Connect > Authorization Server
   (`OpenId/ServerAdminMenu.cs:22`, page title "Configure OpenID Connect server settings"). Before the first save the
   page shows the defaults of `OpenId/Services/OpenIdServerService.cs:72-83` (the second migration of the Server
   feature changes the settings only when they were saved before: `OpenId/Migrations/PushedAuthorizationRequestsMigration.cs:85-94`).
   Do not rely on that: before you select Save, make sure that these are checked, and check the one that is not:

   | Section | Control | State to save |
   |---|---|---|
   | Endpoints | **Enable Token Endpoint** | checked |
   | Endpoints | **Enable Authorization Endpoint** | checked |
   | Endpoints | **Enable End Session Endpoint** | checked (it is what makes **Allow End Session Endpoint** appear on the application form: `OpenId/Views/Application/Create.cshtml:24`) |
   | Flows | **Allow Authorization Code Flow** | checked |
   | Flows | **Allow Refresh Token Flow** | checked |
   | Flows | **Allow Client Credentials Flow** | check it |

   Leave every other control as the page shows it. The flow checkboxes sit in sections that the page's script reveals
   only while the Token endpoint is checked (and, for Allow Authorization Code Flow, the Authorization endpoint too):
   `OpenId/wwwroot/Scripts/openid-settings/openid-server-settings.js`. Select Save. The success message is "OpenID
   server configuration successfully updated." and the tenant restarts: wait until `/Admin` answers again, then sign
   in again with `Recorder setup` if the session was lost.
   This is what makes the application form offer exactly three flows: Allow Authorization Code Flow, Allow Client
   Credentials Flow and Allow Refresh Token Flow, plus Allow End Session Endpoint (the form hides the flows that the
   server settings do not allow: `OpenId/wwwroot/Scripts/openid-application-editor/application-create.js`).
3. **Check the starting state.** Access Control > OpenID Connect > Scopes lists Email, Phone, Profile and Roles
   (and nothing else). Access Control > OpenID Connect > Applications shows "Nothing here! There are no applications
   at the moment." Open an application form (New application) and check that the **Client Credentials Roles** list
   (visible after checking Allow Client Credentials Flow) contains **Editor**, which the Blog recipe creates
   (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:88-110`). Cancel without saving.
4. **Seed one application**, so that the first view of the list is not empty and the Search and Delete steps have
   something to act on. Access Control > OpenID Connect > Applications > New application, then:

   | Field | Value |
   |---|---|
   | Display Name | `Legacy Prototype` |
   | Type | `Public client` |
   | Client Id | `legacy-prototype` |
   | Allow Authorization Code Flow | checked |
   | Redirect Uris | `https://legacy.example.com/signin-oidc` |
   | Allowed Scopes | none |

   Select Save. The list shows `Legacy Prototype`.
5. **Snapshot S0.** Stop the site (by its process ID), copy its `ORCHARD_APP_DATA` folder and `state.json` aside, and
   start the site again.

No other data is needed. The fake values of the whole video are: `Legacy Prototype`, `legacy-prototype`,
`Inventory Sync`, `inventory-sync`, `Storefront Web`, `storefront-web`, `https://storefront.example.com/signin-oidc`,
`https://storefront.example.com/signout-callback-oidc`, `https://legacy.example.com/signin-oidc`, `Inventory`,
`inventory`, `inventory-api`. The Client Secret of Inventory Sync is generated by the form's own button: nobody types
a secret, and nothing in this repository holds one.

## Clips, order and the state they change

The clips depend on each other and play in the order they are recorded in.

| Order | Clip | Starts from | Page it starts on | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|---|---|
| 1 | `02-service` | S0 | `/Admin` (dashboard) | Creates the application Inventory Sync | S0 |
| 2 | `03-web` | after `02-service` | `/Admin/OpenId/Application` | Creates the application Storefront Web | S1 (copy aside after `02-service`) |
| 3 | `04-manage` | after `03-web` | `/Admin/OpenId/Application` | Deletes Legacy Prototype | S2 (copy aside after `03-web`) |
| 4 | `05-scopes` | after `04-manage` | `/Admin/OpenId/Application` | Creates the scope Inventory and allows it on Inventory Sync | S3 (copy aside after `04-manage`) |

To record one clip again, stop the site, restore the snapshot named in the last column (the `App_Data` folder and
`state.json` together), start the site, and record that clip and the ones after it. `01-concepts` (slides) and the
two cards have nothing to record.

Open each clip's starting page before `StartCaptureAsync()`. Keep the admin menu as it is: the clips that start from
the menu click it on camera.

The recorder calls used below are those of `scripts/recorder/Scene.cs`: `ClickAsync`, `TypeAsync(locator, text)`,
`SelectAsync(locator, "Public client")` (it takes the option's text or its value), `HighlightAsync(locator)` (no
caption), `ClearHighlightsAsync()`, `ScrollAsync(0, pixels)` and `ClickAndWaitForNavigationAsync(locator)`. A click that
loads another page (Save, a menu item, Edit, Cancel, Clear, the search button, the **Ok** button of the dialog) uses
`ClickAndWaitForNavigationAsync`; the **Delete** button only opens the dialog, so it uses `ClickAsync`. "Scroll until X
is in view" means `ScrollAsync(0, 300)` repeated until the element's `BoundingBoxAsync()` lies inside the 864-pixel
viewport.

## Chapter 00: opening card (`00-intro`)

- `intro`: card. Nothing to record: the card shows the logo, the video title "OpenID Connect Applications and
  Scopes" and the subtitle "Register the programs that use your site as an identity provider".

## Chapter 01: Applications and scopes (`01-concepts`, slides)

Nothing to record. `slides.py` draws them.

- `what-1`: slide "What you register": three cards (Application, Scope, Allowed Scopes), the first one bright.
- `what-2`: the same slide with the second and third cards bright.
- `what-3`: slide "Where they are managed": three cards with check marks (OpenID Connect Management UI, OpenID
  Connect Authorization Server, Access Control, OpenID Connect).

## Chapter 02: Register a confidential application (`02-service`)

Starts on `/Admin`, the admin dashboard, with the admin menu visible.

### `service-1`

- Actions: click the admin menu item **Access Control**, then **OpenID Connect**, then **Applications**, waiting for
  the navigation after the last click (`ClickAndWaitForNavigationAsync`). Wait until the row `Legacy Prototype` is
  shown.
  Move the cursor onto the **New application** button (`a.create`) and stop there.
- Visible at the end: the page titled **Applications**, the search box, the **New application** button, and one row,
  `Legacy Prototype`, with its **Edit** and **Delete** buttons.
- Labels: Access Control, OpenID Connect, Applications: `OpenId/ManagementAdminMenu.cs:18-22`. Page title and
  buttons: `OpenId/Views/Application/Index.cshtml:7,15,26`; row buttons:
  `OpenId/Views/OpenIdApplicationEntry.Buttons.SummaryAdmin.cshtml`.

### `service-2`

- Actions: click **New application** (`a.create`) and wait for the form (`#DisplayName` shown). Type `Inventory Sync`
  into `#DisplayName` (`TypeAsync`). Move the cursor to the **Type** list (`#Type`). Do not change it: the list
  starts on **Confidential client**.
- Visible at the end: the page titled **Create a new application**; Display Name `Inventory Sync`; Type
  **Confidential client**; the hint under Type ("Confidential applications MUST send their client secret...").
- Labels: `OpenId/Views/Application/Create.cshtml:10` (title), `:26` (Display Name), `:34` (Type), `:37` (Confidential
  client), `:40` (hint).

### `service-3`

- Actions: type `inventory-sync` into `#ClientId`. Click the shuffle button beside the Client Secret field
  (`#generateClientSecret`, tooltip "Click here to generate new value"). Never click the eye button
  (`#toggleClientSecret`) or the copy buttons.
- Visible at the end: Client Id `inventory-sync`; the Client Secret field filled with masked dots (it is a password
  field, so the value is never shown).
- Labels: `OpenId/Views/Application/Create.cshtml:45` (Client Id), `:57` (Client Secret), `:63` (generate button
  tooltip).

### `service-4`

- Actions: scroll down until the **Flows** heading and the flow checkboxes are in view. Click the checkbox
  `#AllowClientCredentialsFlow`.
- Visible at the end: the **Flows** heading; **Allow Authorization Code Flow** (unchecked); **Allow Client
  Credentials Flow** (checked); **Allow Refresh Token Flow** (shown grayed out, because no flow that uses refresh
  tokens is checked). No Implicit, Hybrid or Password flow is listed. The "Recommended Parameters" hint opens under
  the checked flow; leave it.
- Labels: `OpenId/Views/Application/Create.cshtml:71` (Flows), `:79` (Allow Authorization Code Flow), `:123` (Allow
  Client Credentials Flow), `:134` (Allow Refresh Token Flow).

### `service-5`

- Actions: scroll down to the end of the form, so that **Allowed Scopes** and **Client Credentials Roles** are both in
  view. Click the checkbox of the role labeled `Editor` (`#RoleGroup` > the `Editor` label).
- Visible at the end: **Client Credentials Roles** with `Editor` checked and the other roles unchecked; **Allowed
  Scopes** above it, with its four checkboxes (`email`, `profile`, `phone`, `roles`; the form does not fix their
  order, `OpenId/Controllers/ApplicationController.cs:153`) all unchecked.
- Labels: `OpenId/Views/Application/Create.cshtml:235` (Client Credentials Roles), `:221` (Allowed Scopes). The role
  names come from the site's roles (`OpenId/Controllers/ApplicationController.cs:137-143`).

### `service-6`

- Actions: draw a highlight ring around the Allowed Scopes group (`#ScopeGroup`, no caption) and hold for two
  seconds; `ClearHighlightsAsync()`; click **Save** (`button.save`) and wait for the navigation.
- Visible at the end: the **Applications** list with two rows, `Inventory Sync` and `Legacy Prototype` (sorted by
  display name).
- Labels: `OpenId/Views/Application/Create.cshtml:248` (Save).

## Chapter 03: Register a public application (`03-web`)

Starts on `/Admin/OpenId/Application` (the Applications list with the two applications).

### `web-1`

- Actions: click **New application** (`a.create`) and wait for the form. Type `Storefront Web` into `#DisplayName`.
  Choose **Public client** in `#Type` (`SelectAsync(#Type, "Public client")`). Wait until the Client Secret row
  (`#ClientSecretWrapper`) has finished collapsing.
- Visible at the end: Display Name `Storefront Web`; Type **Public client**; no Client Secret row; the **Flows**
  heading with **Allow Client Credentials Flow** grayed out. If the whole block does not fit in the viewport, scroll
  down by the least amount that shows the Type list and the grayed-out flow together.
- Labels: `OpenId/Views/Application/Create.cshtml:38` (Public client), `:57` (Client Secret), `:123`.

### `web-2`

- Actions: type `storefront-web` into `#ClientId`. Click `#AllowAuthorizationCodeFlow`.
- Visible at the end: Client Id `storefront-web`; **Allow Authorization Code Flow** checked; **Allow Refresh Token
  Flow** enabled (no longer grayed out). The redirect section starts to open below the flows, and the "Recommended
  Parameters" hint opens under the checked flow (the checkbox carries `data-bs-toggle="collapse"`): leave it, the
  narration does not read it.
- Labels: `OpenId/Views/Application/Create.cshtml:45`, `:79`, `:134`.

### `web-3`

- Actions: scroll until the redirect section is in view. Type `https://storefront.example.com/signin-oidc` into
  `#RedirectUris`. Click `#AllowLogoutEndpoint` and wait until `#PostLogoutRedirectUris` is shown. Type
  `https://storefront.example.com/signout-callback-oidc` into it.
- Visible at the end: **Redirect Uris** with the first address; **Allow End Session Endpoint** checked;
  **Post-Logout Redirect Uris** with the second address.
- Labels: `OpenId/Views/Application/Create.cshtml:143` (Redirect Uris), `:157` (Allow End Session Endpoint), `:164`
  (Post-Logout Redirect Uris).

### `web-4`

- Actions: scroll until **Require Proof Key for Code Exchange** and **Require Pushed Authorization Requests** are in
  view. Click `#RequireProofKeyForCodeExchange`.
- Visible at the end: **Require Proof Key for Code Exchange** checked; **Require Pushed Authorization Requests**
  unchecked; both hints ("Ensure that the client application and OAuth or OIDC library being used supports PKCE
  before enabling this option." and the same for PAR) readable.
- Labels: `OpenId/Views/Application/Create.cshtml:179,182,191,194`.

### `web-5`

- Actions: scroll until the **Consent type** list and its three explanatory bullets are in view. Move the cursor to
  `#ConsentType` and draw a highlight ring around it (no caption). Do not change the list.
- Visible at the end: **Consent type** showing **Explicit consent**, and the hint with its three bullets (explicit,
  implicit, external) fully readable.
- Labels: `OpenId/Views/Application/Create.cshtml:200-212`.

### `web-6`

- Actions: `ClearHighlightsAsync()`. Scroll until **Allowed Scopes** is in view. Click the labels `email` and
  `profile` (the checkboxes `#ScopeEntries_N__Selected` whose label text is `email` and `profile`; the form does not
  fix the order of the checkboxes, so find them by label, never by index). Hold for two seconds. Click
  **Save** (`button.save`) and wait for the navigation.
- Visible at the end: the **Applications** list with three rows: `Inventory Sync`, `Legacy Prototype`,
  `Storefront Web`.
- Labels: `OpenId/Views/Application/Create.cshtml:221-232` (Allowed Scopes; each label is the scope's name),
  `:248` (Save).

## Chapter 04: Manage applications (`04-manage`)

Starts on `/Admin/OpenId/Application` (three applications).

### `manage-1`

- Actions: click the search box (`#search-text`) and type `Legacy`. Click the search button (the `button[type=submit]`
  with the title "Search" in the same form) and wait for the navigation. Hold two seconds on the filtered list. Click
  **Clear** (`a` with the text `Clear`) and wait for the navigation.
- Visible at the end: the full list again (`Inventory Sync`, `Legacy Prototype`, `Storefront Web`), the search box
  empty, no **Clear** button.
- Labels: `OpenId/Views/Application/Index.cshtml:15-26`; the search matches the display name
  (`OpenId/Controllers/ApplicationController.cs:91`).

### `manage-2`

- Actions: in the row `Inventory Sync`, click **Edit** and wait for the form (`#ClientSecret` shown). Draw a
  highlight ring around `#ClientSecret` (no caption). Click nothing else in the form.
- Visible at the end: the page titled **Edit an application**; Display Name `Inventory Sync`; the **Client Secret**
  field empty, with the gray placeholder "enter a new value to replace the existing secret"; **Allow Introspection
  Endpoint** and **Allow Revocation Endpoint** below it.
- Labels: `OpenId/Views/Application/Edit.cshtml:10` (title), `:59,62` (Client Secret, placeholder), `:75` (Allow
  Introspection Endpoint), `:84` (Allow Revocation Endpoint). These two checkboxes are only on the Edit page.

### `manage-3`

- Actions: `ClearHighlightsAsync()`. Click **Cancel** (`a.cancel`) and wait for the navigation. In the row
  `Legacy Prototype`, click **Delete** (`a.delete`). Wait until the confirmation dialog (`#confirmRemoveModal`) is
  shown and hold two seconds. Click its **Ok** button (`#modalOkButton`) and wait for the navigation.
- Visible at the end: the **Applications** list with two rows, `Inventory Sync` and `Storefront Web`.
- Labels: the dialog is built from `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`: title "Delete", message
  "Are you sure you want to remove this element?", buttons "Ok" and "Cancel". Row buttons:
  `OpenId/Views/OpenIdApplicationEntry.Buttons.SummaryAdmin.cshtml`.

## Chapter 05: Scopes (`05-scopes`)

Starts on `/Admin/OpenId/Application` (two applications).

### `scopes-1`

- Actions: click the admin menu item **Scopes** under **Access Control > OpenID Connect** (if that section is
  collapsed, click **Access Control** and **OpenID Connect** first) and wait for the navigation.
- Visible at the end: the page titled **Scopes**; four rows sorted by display name: `Email`, `Phone`, `Profile`,
  `Roles`. Each row shows a badge with a key icon and its name (`email`, `phone`, `profile`, `roles`), and a
  description line under the display name.
- Labels: `OpenId/ManagementAdminMenu.cs:27` (Scopes); `OpenId/Views/Scope/Index.cshtml:7,26`; the rows:
  `OpenId/Drivers/OpenIdScopeDisplayDriver.cs`, `OpenId/Views/OpenIdScopeEntry.DefaultTags.SummaryAdmin.cshtml`,
  `OpenId/Views/OpenIdScopeEntry.Description.SummaryAdmin.cshtml`; the four scopes:
  `OpenId/Migrations/DefaultScopesMigration.cs:22,34,45,57`.

### `scopes-2`

- Actions: click **New scope** (`a.create`) and wait for the form. Type `Inventory` into `#DisplayName`, `inventory`
  into `#Name`, and `Reads and updates the stock levels of the shop.` into `#Description`.
- Visible at the end: the page titled **Create a new scope**; Display Name `Inventory`; Name `inventory`; the
  description filled in.
- Labels: `OpenId/Views/Scope/Index.cshtml:26` (New scope), `OpenId/Views/Scope/Create.cshtml:6` (title), `:15`
  (Display Name), `:23` (Name), `:31` (Description).

### `scopes-3`

- Actions: type `inventory-api` into `#Resources`. Draw a highlight ring around the hint under the field (the
  `span.hint` that follows `#Resources`, no caption).
- Visible at the end: the label **Additional resources (API audiences)** with `inventory-api` in the field, and the
  hint under it fully readable.
- Labels: `OpenId/Views/Scope/Create.cshtml:39` (label), `:42` (hint).

### `scopes-4`

- Actions: `ClearHighlightsAsync()`. Click **Save** (`button.save`) and wait for the navigation.
- Visible at the end: the **Scopes** list with five rows: `Email`, `Inventory`, `Phone`, `Profile`, `Roles`. The
  `Inventory` row shows the badge `inventory` and the description typed in `scopes-2`.
- Labels: `OpenId/Views/Scope/Create.cshtml:48` (Save).

### `scopes-5`

- Actions: click the admin menu item **Applications** under **Access Control > OpenID Connect** and wait for the
  navigation. In the row `Inventory Sync`, click **Edit** and wait for the form. Scroll until **Allowed Scopes** is
  in view. Click the label `inventory` (find the checkbox by its label, as in `web-6`, never by index).
- Visible at the end: **Allowed Scopes** with `inventory` checked and `email`, `profile`, `phone` and `roles`
  unchecked.
- Labels: `OpenId/ManagementAdminMenu.cs:22` (Applications); `OpenId/Views/Application/Edit.cshtml:241` (Allowed
  Scopes).

### `scopes-6`

- Actions: click **Save** (`button.save`) and wait for the navigation.
- Visible at the end: the **Applications** list with `Inventory Sync` and `Storefront Web`.
- Labels: `OpenId/Views/Application/Edit.cshtml:268` (Save).

## Chapter 99: closing card (`99-outro`)

- `outro`: card. Nothing to record: the logo, the title and the subtitle "docs.orchardcore.net".

## Checks for private data

Before assembling, look at every contact sheet (`review.py`), then OCR the finished video as the skill describes.

- Names: only `admin` (the signed-in user), `Legacy Prototype`, `Inventory Sync`, `Storefront Web`, `Inventory`.
- Addresses: only `admin@example.com` and addresses under `example.com`. No other email, host, phone number or token.
- The Client Secret is generated by the button and never shown: no frame may show the eye button's revealed value.
  If one does (the field shows letters instead of dots), record `service-3` again.
- No frame shows an error message, a validation message or an empty list (the seeded application keeps the first
  list from being empty).
- No frame shows a password, the contents of `state.json`, or a real path of the machine.
- The site name is `Orchard Core`.
