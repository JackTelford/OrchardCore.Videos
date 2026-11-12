# Recording plan: secrets-manage

The recording plan of "Secrets: Store Passwords and API Keys Safely" (`storyboard.json`). It is for the person who
records the clips: it says what the site must hold before each clip, what to click and type, and what must be on screen
when each narration ends. A selector below stands for `scene.Page.Locator(...)`. File references (`file:line`) are in
the Orchard Core source at commit 92db4a83, under `src/OrchardCore.Modules` unless a path starts with another folder;
they tell where a label comes from, so a changed label can be found again.

Narrate first (`narrate.py`), because each step is held for the length of its narration. The narration times below
(about 2.5 words per second) only help to pace the actions: the recorder takes the real ones from `durations.json`.

## The site it needs

- A fresh site, with its default tenant only: `src/OrchardCore.Cms.Web` of an Orchard Core checkout, with its own `App_Data`
  folder outside both repositories (`ORCHARD_APP_DATA`), on `http://localhost:5010`, provisioned by Auto Setup: recipe **Blog**, site name
  **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, and the password in
  `VIDEO_PASSWORD` (written `<password>` everywhere in the repository, never a real one). Outbound calls are blocked
  with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address; nothing in this video sends a request to a mail server.
- The Blog recipe enables neither the Secrets feature, the Email features nor the Admin Dashboard
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:29-79`), so a fresh site is in the starting state for the
  first clip, except for the SMTP provider below.
- **Before recording anything**, outside the capture: in **Tools, Features** enable **SMTP Email Provider**
  (`OrchardCore.Email.Smtp`; it enables **Email** with it). Do **not** enable **Secrets**: the first clip does it on
  camera. Leave **Admin Dashboard** disabled.
- Sign the recorder in once (`Recorder setup`) to make `state.json` (git-ignored).
- No external service is involved, so no local stand-in is needed. The only store is **Database**: do not enable
  **Secrets - Azure Key Vault**. With one writable store the **Store** row of a secret is a read-only text, which the
  narration of `create-4` relies on (`OrchardCore.Secrets/Views/Admin/Edit.cshtml:39-60`).
- The Localization feature is not enabled, so no request culture is applied (`UseRequestLocalization` is called only by
  `OrchardCore.Localization/Startup.cs:62` and `OrchardCore.Setup/Startup.cs:87`). The server prints the dates of the badges
  with the culture of its own process, which on Windows is the regional format of the account that runs the site
  (`Index.cshtml:121-133`). The plan shows the `en-US` form, such as `11/2/2026`: set the regional format of that account to
  English (United States) before the first take, and check the form on screen. The date input of the editor follows the
  browser's language instead.

### Dates are relative to the recording day

The expiration badges depend on today's date, so never type a fixed calendar date. Call the recording day **D** and
compute every date when you record: **D + 12 days**, **D - 20 days**, **D + 365 days**. A date input takes the ISO
form: `await scene.Page.Locator("#ExpiresUtc").FillAsync("yyyy-MM-dd")`. The narration never speaks a date.

### The fake data

Every value is invented. The secret values are shown as dots (a password field) and are never read aloud.

| Secret | Type | Value | Description | Expiration date | Created |
| --- | --- | --- | --- | --- | --- |
| `Smtp.Password` | Text Secret | `fake-mail-value-1` | Password of the mail account that sends the site email | D + 365 | on camera, `03-create` |
| `Signing.Key` | RSA Key | generated, 2048 bits | Example RSA key pair | none | on camera, `04-types` |
| `Payments.ApiKey` | Text Secret | `fake-payments-value-1` | API key of the payment service | D + 12 | seed, outside the capture |
| `Analytics.ApiKey` | Text Secret | `fake-analytics-value-1` | API key of the analytics service | D - 20 | seed, outside the capture |
| `Newsletter.ApiKey` | Text Secret | `fake-newsletter-value-1` | API key of the newsletter service | none | seed, outside the capture |
| `Survey.ApiKey` | Text Secret | `fake-survey-value-1` | API key of the survey service | none | seed, outside the capture |

The SMTP settings entered in `08-settings`: sender `noreply@example.com`, host `smtp.example.com`, user name
`mailer@example.com`.

**Seeding the four `*.ApiKey` secrets** (after `04-types`, before `05-review`, outside the capture). Either use the form
(**Add Secret**, **Text Secret**, **Create**, fill in, **Save**), or post it from a page of the signed-in session: open
`/Admin/Secrets/Create?type=TextSecret`, read the value of `input[name="__RequestVerificationToken"]` and the `action`
of the form, then `fetch` that action with `method: "POST"`, `credentials: "same-origin"`, a
`application/x-www-form-urlencoded` body with `__RequestVerificationToken`, `SecretType=TextSecret`, `Name`,
`Store=Database`, `TextValue`, `Description` and `ExpiresUtc` (`yyyy-MM-dd`, left out when none), and check that
`/Admin/Secrets` lists the secret. The fields are those of `OrchardCore.Secrets/ViewModels/SecretEditViewModel.cs`, and
the action is `AdminController.CreatePost` (`OrchardCore.Secrets/Controllers/AdminController.cs:278-325`). Do not use the
Secrets recipe step: its array form takes a `Value` but cannot set a description or an expiration date
(`OrchardCore.Secrets/Recipes/SecretsRecipeStep.cs:150-182`), and its object form can set them in `SecretInfo` but reads the
value from the configuration or from an encrypted payload (`SecretsRecipeStep.cs:63-148`).

## Order, dependencies and snapshots

Record in the order the clips play, with the seed between `04-types` and `05-review`. Every clip is recorded from the
state the previous one left, which is also how the narration is written.

| Order | Clip | Needs | Changes the site |
| --- | --- | --- | --- |
| 1 | `02-enable` | Snapshot **A**: fresh site, SMTP provider enabled, Secrets off | Enables **Secrets** |
| 2 | `03-create` | Snapshot **B**: A + Secrets enabled, no secret | Creates `Smtp.Password` |
| 3 | `04-types` | Snapshot **C**: B + `Smtp.Password` | Creates `Signing.Key` (the X.509 form is not saved) |
| 4 | seed | Snapshot **C2**: C + `Signing.Key`, made after `04-types` | Creates the four `*.ApiKey` secrets; then make Snapshot **D** |
| 5 | `05-review` | D | Nothing |
| 6 | `06-update` | D | Edits `Payments.ApiKey`; then make Snapshot **E** |
| 7 | `07-delete` | E | Deletes `Analytics.ApiKey`, `Newsletter.ApiKey`, `Survey.ApiKey`; then make Snapshot **F** |
| 8 | `08-settings` | Snapshot **F**: E after `07-delete`, so `Smtp.Password`, `Payments.ApiKey` and `Signing.Key` remain and the SMTP settings are empty | Saves the SMTP settings |

A snapshot is a copy of the site's `App_Data` folder and of `state.json`, made with the site stopped. Make each snapshot
named in the "Needs" column when the clip before it is recorded, as soon as that take is kept: **B** after `02-enable`, **C**
after `03-create`, **C2** after `04-types`, **D** after the seed, **E** after `06-update` and **F** after `07-delete`.
**Before a new take of a clip, stop the site, restore the snapshot in the "Needs" column and start the site again.**
`05-review` leaves the site as it found it, so it can be recorded again without a restore. The seed dates are fixed at the
moment of seeding: if D changes (a take on another day), restore **C2** and seed again. The Use a secret list of `settings-4`
shows the Text Secrets of the site, so `08-settings` must start from F, not from E.

The slide chapters (`01-why`, `09-access`) and the cards are not recorded.

## Clips

Narration times are approximate. Toasts (the green messages at the top right) disappear 5 seconds after the page
loads (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`; `OrchardCore.Themes/TheAdmin/Views/Message.cshtml`),
so click the button that causes one at the very start of the step. Where a step says "toast", the message is on screen
only for the first seconds of the step.

Common locators:

| What | Selector | Source |
| --- | --- | --- |
| Sidebar | `#ta-left-sidebar`; menu entries by their exact text, for example `#ta-left-sidebar >> text="Tools"` | `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:76` |
| Toast | `.oc-notify-toast-container .toast` | `OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml` |
| Confirm dialog | `#confirmRemoveModal`, button `#modalOkButton` ("Ok"), button `#modalCancelButton` ("Cancel") | `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-33`, `Layout.cshtml:99` |
| A row of the Secrets list | `li.list-group-item:has(a:text-is("<name>"))` | `OrchardCore.Secrets/Views/Admin/Index.cshtml:86-100` |
| Edit, Move, Delete of a row | `a.btn-primary` ("Edit"), `a.btn-secondary` ("Move"), `a.btn-danger` ("Delete") inside the row | `Index.cshtml:156-158` |

The sidebar markup is generated by the theme (`NavigationItem-admin.cshtml`) and its ids are not fixed in the source:
find the entries once with `Recorder probe`, and expand a menu group by clicking its name.

### `02-enable` (chapter 02, Turn on the Secrets feature)

Starts on: `/Admin` (the admin home), with Snapshot A, nothing expanded on purpose. The capture starts there.

- `enable-1` (about 7 s). Click **Tools** in the sidebar, then **Features** (`ClickAndWaitForNavigationAsync`). On the
  Features page, `TypeAsync` "Secrets" into `#search-box`; the list filters while typing. On screen at the end: the
  Features page with **Secrets** in the search box and the rows whose name, id or description contains it
  (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`; such as **Secrets**, **Secrets - Azure Key Vault** and
  **OpenID Connect Secrets**), the **Secrets** row in the **Security** group showing the
  **Disabled** badge and an **Enable** button. Labels: **Tools** and **Features** `OrchardCore.Features/AdminMenu.cs:26-27`;
  search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`; group heading `Features.cshtml:88` (the feature's category,
  `Category = "Security"` in `OrchardCore.Secrets/Manifest.cs:13`).
- `enable-2` (about 6 s). Click **Enable** in the **Secrets** row only: `#btn-enable-OrchardCore_Secrets` (the id is
  `btn-enable-` plus the feature id with the dot replaced by an underscore, `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`;
  if `Recorder probe` shows another id, use `li.list-group-item:has(code:text-is("OrchardCore.Secrets")) a:text-is("Enable")`)
  (`ClickAndWaitForNavigationAsync`; enabling restarts the tenant, so the redirect to the Features page takes a moment). On
  screen at the end: the Features page again, with the toast "The feature Secrets was enabled." at the top right (if the
  redirect is slow, it appears only as the narration ends: record again with a longer pause before the next step).
  Labels: **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message
  `OrchardCore.Features/Controllers/AdminController.cs:317`.
- `enable-3` (about 6 s). In the sidebar make **Tools** open if it is not, click **Security** to open it, and hold on the
  three entries **Secrets**, **Migrate Secrets**, **Secret Stores** (move the cursor over each, `MoveToAsync`, no
  highlight needed). Then click **Secrets** (`ClickAndWaitForNavigationAsync`). On screen at the end: the Secrets page
  starting to load or loaded (the next step needs the list). Labels: `OrchardCore.Secrets/AdminMenu.cs:20-21` (**Tools**,
  **Security**), `:33` (**Secrets**), `:40` (**Migrate Secrets**), `:47` (**Secret Stores**).
- `enable-4` (about 4 s). No action beyond `HighlightAsync` on the **Add Secret** button (`button.create`) and then
  `ClearHighlightsAsync()`. On screen at the end: the page titled **Secrets**, with the search box, the **Add Secret**
  button and the message "Nothing here! There are no secrets stored. Click 'Add Secret' to create one." in a blue box; no
  warning box. Labels: `OrchardCore.Secrets/Views/Admin/Index.cshtml:13` (title), `:39` (**Add Secret**), `:179`
  (message).

### `03-create` (chapter 03, Create a text secret)

Starts on: `/Admin/Secrets` (empty list), with Snapshot B. Changes the site: creates `Smtp.Password`.

- `create-1` (about 7 s). Click **Add Secret** (`button.create`). The dialog **Select Secret Type** opens with three
  cards: **Text Secret**, **RSA Key**, **X.509 Certificate**, each with a description and a **Create** button, and a
  **Cancel** button at the bottom. Move the cursor slowly over the three cards. On screen at the end: the open dialog
  `#modalSecretTypes` showing the three cards. Labels: `Index.cshtml:39`, `:200` (**Select Secret Type**), `:210-214`,
  `:222`; card titles `OrchardCore.Secrets/Providers/TextSecretTypeProvider.cs:14` (**Text Secret**),
  `RsaKeySecretTypeProvider.cs:14` (**RSA Key**), `X509SecretTypeProvider.cs:14` (**X.509 Certificate**); the cards follow
  the order of the registrations in `OrchardCore.Secrets/Startup.cs:31-33`.
- `create-2` (about 12 s). `HighlightAsync` on the **Text Secret** card (`#modalSecretTypes .card:has(h5:text-is("Text Secret"))`)
  and keep it for the whole step. No click. On screen at the end: the dialog with the ring around **Text Secret** and its
  description "Stores a text value such as passwords, API keys, or connection strings." (`TextSecretTypeProvider.cs:16`).
- `create-3` (about 9 s). `ClearHighlightsAsync()`, click **Create** in the **Text Secret** card
  (`ClickAndWaitForNavigationAsync`; the form is `/Admin/Secrets/Create?type=TextSecret`). `TypeAsync(#Name, "Smtp.Password")`.
  On screen at the end: the page titled **New Text Secret** with **Secrets** as its breadcrumb, **Name** filled with
  `Smtp.Password`, and its hint "A unique name used by the settings that reference this secret, for instance Smtp.Password.
  It cannot be changed later." Labels: `Edit.cshtml:6` (title), `:24` (**Name**), `:28` (hint).
- `create-4` (about 11 s). `HighlightAsync` on the **Store** row (`#store-display`, a read-only text showing **Database**), then
  `ClearHighlightsAsync()`, then `TypeAsync(#TextValue, "fake-mail-value-1")` (dots appear) and `HighlightAsync` on `#TextValue`.
  On screen at the end: **Store** showing **Database**, **Value** filled with dots and ringed. Labels: **Store** `Edit.cshtml:42`,
  value `:55`; **Value** `Edit.cshtml:68`.
- `create-5` (about 13 s). `ClearHighlightsAsync()`. `TypeAsync(#Description, "Password of the mail account that sends the site email")`
  (use a short typing delay), then `scene.Page.Locator("#ExpiresUtc").FillAsync(<D + 365 as yyyy-MM-dd>)` and `HighlightAsync`
  on `#ExpiresUtc`; its hint reads "Optional. This is informational only and does not disable the secret. Use it for
  rotation reminders and alerts." On screen at the end: **Description** filled, **Expiration date** filled (shown as a
  date, for example `11/12/2027`) and ringed. Labels: **Description** `Edit.cshtml:195`, **Expiration date** `:204`, hint `:208`.
- `create-6` (about 11 s). `ClearHighlightsAsync()`. **Click Save immediately at the start of the step**
  (`button.save`, `ClickAndWaitForNavigationAsync`) so the toast is on screen during the first seconds. On screen at the
  end: the Secrets list with one row: `Smtp.Password` as a link, the badges **Text Secret**, **Database**, **Expires** with the
  date, and a last-updated badge, and the description under the name; the warning box is
  absent. The toast "Secret created successfully." shows at the start. Labels: **Save** `Edit.cshtml:213`; message
  `AdminController.cs:313`; badges `Index.cshtml:109-147`.

### `04-types` (chapter 04, RSA keys and certificates)

Starts on: `/Admin/Secrets` (one row), with Snapshot C. Changes the site: creates `Signing.Key`.

- `types-1` (about 12 s). Click **Add Secret**, then **Create** in the **RSA Key** card (`ClickAndWaitForNavigationAsync`).
  `TypeAsync(#Name, "Signing.Key")`. `HighlightAsync` on `#RsaKeySize` (the **Key size** list showing **2048 bits**; leave
  it unchanged and do not open it). `TypeAsync(#Description, "Example RSA key pair")`. Click **Save** at about 9 seconds
  (`ClickAndWaitForNavigationAsync`). On screen at the end: the Secrets list with two rows, sorted by name
  (`AdminController.cs:112-115`): `Signing.Key` (badge **RSA Key**, **Never expires**), then `Smtp.Password`. Labels: page title "New RSA Key" `Edit.cshtml:6`; **Key size** and
  its options `Edit.cshtml:84-89`; hint "A new RSA key pair is generated with this key size." `:92`.
- `types-2` (about 10 s). Click **Edit** in the `Signing.Key` row (`ClickAndWaitForNavigationAsync`) and `HighlightAsync`
  the blue box `.alert-info` that reads "The RSA key is stored securely. The private key cannot be displayed." On screen
  at the end: the page titled **Edit 'Signing.Key' secret**, the **Store** row (read-only **Database**, with the hint to
  use the **Move** action), the ringed blue box, then **Description** and **Expiration date**; there is no key field.
  Labels: `Edit.cshtml:12` (title), `:58` (Move hint), `:101` (blue box).
- `types-3` (about 14 s). Click **Cancel** (`a.cancel`, `ClickAndWaitForNavigationAsync`), click **Add Secret**, click
  **Create** in the **X.509 Certificate** card (`ClickAndWaitForNavigationAsync`). The form shows **Source** set to
  **Certificate store** with **Store location**, **Store name** and **Thumbprint**: `HighlightAsync` on `#X509Source`. Then
  `SelectAsync(#X509Source, "PEM-encoded certificate")`: the **Certificate** and **Private key** boxes replace the three
  store fields. Do not type anything and do not save; leave the page open. On screen at the end: the page titled **New
  X.509 Certificate** with **Source** set to **PEM-encoded certificate** and the two text boxes **Certificate** (placeholder
  `-----BEGIN CERTIFICATE-----`) and **Private key**, whose hint begins "Optional.". Labels: `Edit.cshtml:110-114`
  (**Source**, **Certificate store**, **PEM-encoded certificate**), `:123` (**Certificate**), `:131` (**Private key**), `:141`
  (hint), `:149-171` (**Store location**, **Store name**, **Thumbprint**).

After this clip, make Snapshot **C2**, seed the four `*.ApiKey` secrets (see the data above), and make Snapshot **D**.

### `05-review` (chapter 05, Review your secrets)

Starts on: `/Admin/Secrets` with Snapshot D: six rows in this order: `Analytics.ApiKey`, `Newsletter.ApiKey`, `Payments.ApiKey`,
`Signing.Key`, `Smtp.Password`, `Survey.ApiKey`. Changes nothing.

- `review-1` (about 11 s). `HighlightAsync` on the warning box `.secrets-expiration-warning` for the whole step. No click.
  On screen at the end: above the list, the yellow box "One secret has expired. One secret expires within 30 days." (no
  **Review secrets** link on this page), the ring around it, and the six rows with their name, description and badges.
  Labels: `Index.cshtml:16-19`; `OrchardCore.Secrets/Views/SecretsExpirationWarning.cshtml:4` and `:8`.
- `review-2` (about 10 s). `ClearHighlightsAsync()`. `HighlightAsync` on the **Expired** badge of `Analytics.ApiKey`
  (`span.text-bg-danger` in its row), then, after about 4 seconds, on the **Expires** badge of `Payments.ApiKey`
  (`span.text-bg-warning` in its row). On screen at the end: the **Expires** badge of `Payments.ApiKey` ringed, with the
  **Expired** badge of `Analytics.ApiKey` above it. The badge texts are `Expired <date>` and `Expires <date>`. Labels:
  `Index.cshtml:121` and `:127`.
- `review-3` (about 7 s). `ClearHighlightsAsync()`. `TypeAsync(#search-box, "payment")`, then
  `WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`. On screen at the end: the search box holding
  "payment" and a list with the single row `Payments.ApiKey`; the warning box is still shown with the same two sentences
  (it counts all secrets). Label: placeholder **Search** `Index.cshtml:33`. Searching runs when Enter is pressed
  (`OrchardCore.Resources/Assets/js/list-management.js:437-452`).
- `review-4` (about 10 s). `NavigateAsync("/Admin/Secrets")` (this clears the search). The two filters are bootstrap-select
  menus, not native lists, so do not use `SelectAsync`: click the button **Expiration** (`div.filter button.dropdown-toggle:has-text("Expiration")`),
  then the option **Expired** (`.dropdown-menu.show a:has(span.text:text-is("Expired"))`); the choice reloads the page by
  itself, so wrap the click in `WaitForNavigationAsync`. The list shows only `Analytics.ApiKey`. Hold one second, then
  `NavigateAsync("/Admin/Secrets")` again, click the button **Type**, and choose **RSA Key** (again inside
  `WaitForNavigationAsync`); the list shows only `Signing.Key`. Do not combine the two filters: they would leave an empty
  list. On screen at the end: the list filtered to `Signing.Key`, with the filter buttons **Expiration** and **Type** at the
  top right of the list header. Labels: `Index.cshtml:62-63` (**Expiration**, **Type**, and the headers "Filter by
  expiration", "Filter by type"); options `AdminController.cs:124-127` (**All**, **Expired**, **Expires within 30 days**,
  **Never expires**) and `:132-135` (**All types**, then **RSA Key**, **Text Secret**, **X.509 Certificate** by name).
- `review-5` (about 8 s). `NavigateAsync("/Admin")` and `HighlightAsync` on `.secrets-expiration-warning`. On screen at the
  end: the admin home page with the same yellow box, followed by a **Review secrets** link, ringed. Labels:
  `OrchardCore.Secrets/Views/SecretsExpirationWarning.cshtml:12`; the filter that adds it to the admin home
  `OrchardCore.Secrets/Filters/SecretExpirationWarningFilter.cs:40-46`.

### `06-update` (chapter 06, Replace a value)

Starts on: `/Admin/Secrets` with Snapshot D. Changes the site: edits `Payments.ApiKey`.

- `update-1` (about 14 s). Click **Edit** in the `Payments.ApiKey` row (`ClickAndWaitForNavigationAsync`). `HighlightAsync`
  on `#TextValue` for the rest of the step. On screen at the end: the page titled **Edit 'Payments.ApiKey' secret**, **Store**
  showing **Database**, **Value** empty with the placeholder "Enter a new value to replace the current one" and the hint
  "Leave empty to keep the current value. Stored values are never displayed.", ringed. Labels: `Edit.cshtml:12`, `:68-74`
  (placeholder and hint at `:70` and `:74`).
- `update-2` (about 9 s). `ClearHighlightsAsync()`. `TypeAsync(#TextValue, "fake-payments-value-2")` (dots),
  `scene.Page.Locator("#ExpiresUtc").FillAsync(<D + 365 as yyyy-MM-dd>)`, then click **Save** at about 5 seconds
  (`ClickAndWaitForNavigationAsync`). On screen at the end: the Secrets list where `Payments.ApiKey` now shows an **Expires**
  badge without the highlight, and the warning box reads only "One secret has expired." (the sentence about 30 days is
  gone). Labels: **Save** `Edit.cshtml:213`; message "Secret updated successfully." `AdminController.cs:423` (toast, gone
  by the end).

### `07-delete` (chapter 07, Delete secrets)

Starts on: `/Admin/Secrets` with Snapshot E (D after `06-update`). Changes the site: deletes three secrets.

- `delete-1` (about 13 s). Click **Delete** in the `Analytics.ApiKey` row (`a.btn-danger`). The dialog titled **Delete Secret** asks
  "Are you sure you want to delete the secret 'Analytics.ApiKey'?"; hold it for about 2 seconds, then click **Ok**
  (`#modalOkButton`; the page reloads). On screen at the end: the list without `Analytics.ApiKey` (five rows) and, because
  no secret is expired or about to expire, no warning box. Labels: `Index.cshtml:158` (**Delete**, title, message); **Ok** `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`;
  toast "Secret deleted successfully." `AdminController.cs:457` (gone by the end).
- `delete-2` (about 11 s). Tick the check boxes of `Newsletter.ApiKey` and `Survey.ApiKey`
  (`input[value="Database:Newsletter.ApiKey"]` and `input[value="Database:Survey.ApiKey"]`, `ClickAsync`). As soon as one
  box is ticked, the **Actions** button takes the place of the **Expiration** and **Type** filters, which are hidden while a
  selection is active, and the item count gives way to "<n> selected" (`OrchardCore.Resources/Assets/js/list-management.js:552-557`;
  `data-min-selected-items="1"` at `Index.cshtml:22`, read at `list-management.js:106`). Click **Actions**
  (`#bulk-action-menu-button`), then **Delete** in its menu
  (`.dropdown-item[data-action="Remove"]`). The dialog titled **Bulk Action** asks "Are you sure you want to delete these
  items?"; click **Ok** (`#modalOkButton`) **at about 6 seconds into the step**, so that the toast is on screen when the
  narration ends. On screen at the end: the toast "2 secrets were deleted." and the list reduced to `Payments.ApiKey`,
  `Signing.Key` and `Smtp.Password`. Labels: `Index.cshtml:51-57` (selection), `:67-69` (**Actions**);
  **Delete** `AdminController.cs:140`; dialog title and message `Index.cshtml:74`; message `AdminController.cs:208`.

### `08-settings` (chapter 08, Use a secret in settings)

Starts on: `/Admin` with Snapshot F (E after `07-delete`; `Smtp.Password` exists and the SMTP settings are empty). Changes the
site: saves the SMTP settings. The capture starts on the admin home.

The fields of the SMTP editor have ids that start with the prefix of the display driver, which is built from the model name
and the settings class name, so it includes `SmtpSettings`
(`OrchardCore.DisplayManagement/Entities/SectionDisplayDriverBase.cs:79-89`, `smtp-settings.ts:1-3`), so select them by the end
of the id: `[id$='_IsEnabled']`, `[id$='_DefaultSender']`, `[id$='_Host']` (not `[id$='Host']`, which also matches
`_ProxyHost`), `[id$='_RequireCredentials']`, `[id$='_UserName']`. A label click works as well, because each label points at
its field. The ids are not fixed in the source: confirm them with `Recorder probe`.

- `settings-1` (about 11 s). In the sidebar open **Settings**, then **Communication**, and click **Email**
  (`ClickAndWaitForNavigationAsync`; the page is `/Admin/Settings/email`). The settings page has the tabs **Providers**
  and **SMTP**; click **SMTP**. On screen at the end: the page titled **Settings** with the **SMTP** tab selected and the
  **Enable SMTP Provider** check box, unchecked. Labels: `OrchardCore.Email/AdminMenu.cs:38-40` (**Settings**,
  **Communication**, **Email**); tabs `OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:59` ("Providers") and
  `OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88` ("SMTP"); title `OrchardCore.Settings/Views/Admin/Index.cshtml:4`.
- `settings-2` (about 10 s). Click the label **Enable SMTP Provider** (`label:text-is("Enable SMTP Provider")`), `TypeAsync`
  `[id$='_DefaultSender']` (**Sender email address**) with `noreply@example.com`, `TypeAsync` `[id$='_Host']` (**Host name**) with
  `smtp.example.com` (the **Delivery method** stays **Network**, and the **Port number** keeps 25), click the label
  **Require credentials** (`label:text-is("Require credentials")`), and `TypeAsync` `[id$='_UserName']` (**User name**) with
  `mailer@example.com`. Leave **Use Windows authentication** unchecked. On screen at the end: the **Network delivery options** with the filled fields, **Require
  credentials** checked, and the rows **User name** and **Password** visible (scroll down by hand if the Password row is
  below the fold, with `ScrollAsync`; clear highlights first). Labels: `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10`
  (**Enable SMTP Provider**), `:17` (**Sender email address**), `:50` (**Host name**), `:120` (**Require credentials**), `:131`
  (**Use Windows authentication**), `:139` (**User name**), `:148` (**Password**).
- `settings-3` (about 12 s). `HighlightAsync` on the source list of the **Password** row (`select[data-secret-input-source]`),
  which shows **User-specified value** and a password box under it with the hint "The value is encrypted and stored with these
  settings." Open nothing yet. On screen at the end: the ringed list and the hint. Labels:
  `OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:26-27` (**User-specified value**, **Use a secret**), `:36` (hint).
- `settings-4` (about 12 s). `ClearHighlightsAsync()`. `SelectAsync(select[data-secret-input-source], "Use a secret")`: the password box
  is replaced by a list of secrets with two buttons, a reload button (title "Reload the secrets") and **New secret**. Then
  `SelectAsync([data-secret-input-secrets] select, "Smtp.Password")`. Then `HighlightAsync` on the **New secret** link
  (`a:has-text("New secret")`) and on `button[data-secret-input-reload]`. Do not click either button. On screen at the end:
  **Use a secret** selected, **Smtp.Password** selected in the list (the list holds **None** and the Text Secrets of the site,
  `Smtp.Password` and `Payments.ApiKey`, in the order the store returns them, not sorted: `SelectSecretViewComponent.cs:36-45`;
  `Signing.Key` is not offered because it is not a Text Secret), the two buttons ringed, and the hint "Create the
  secret in the new tab, then reload the secrets to select it." Labels: `SecretInput.cshtml:27`, `:52` (reload), `:59`
  (**New secret**), `:69` (hint); list `OrchardCore.Secrets/ViewComponents/SelectSecretViewComponent.cs:49` (**None**).
- `settings-5` (about 10 s). `ClearHighlightsAsync()`. **Click Save immediately** (`button.save`, `ClickAndWaitForNavigationAsync`). On
  screen at the end: the settings page reloaded after saving, with the toast "Site settings updated successfully." at the
  start and, once it is gone, the form. If the page reopens on the **Providers** tab, click **SMTP** so the **Password** row is
  visible with **Use a secret** and **Smtp.Password**. Labels: **Save** `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`;
  message `OrchardCore.Settings/Controllers/AdminController.cs:119`. The narration of this step says that saving with a secret
  removes a password stored in the settings (`OrchardCore/OrchardCore.Secrets.Abstractions/SecretInputExtensions.cs:54-55`); the
  hint about it (`SecretInput.cshtml:73`) is shown only when a value is stored, so it is not on screen in this recording.
- `settings-6` (about 9 s). No action. On screen at the end: the same page, **SMTP** tab, **Password** row with **Use a secret**
  and **Smtp.Password**.

## Slide steps

These are drawn by `slides.py` (`slide(step_id, chapter_title, label)`) and need no recording.

- `why-1`: the four cards "Values in many places", "Every copy can leak", "No inventory", "No reminders", all bright.
- `why-2`: the four cards of the Secrets module: "Encrypted", "Named", "Listed in one place", "Expiration warnings".
- `access-1`: "Two permissions": the cards "View secrets", "Manage secrets", "Administrators", "Other roles". The permission
  names come from `OrchardCore.Secrets/Permissions.cs:9` and `:12`; Manage secrets implies View secrets (`:12`); the
  administrator role gets both (`OrchardCore.Secrets/SecretsPermissionProvider.cs:20-21`); the roles page is
  **Access Control**, **Roles** (`OrchardCore.Roles/AdminMenu.cs:18-19`). The menu items **Migrate Secrets** and **Secret Stores** need
  Manage secrets (`AdminMenu.cs:40-52`).
- `access-2`: "Under Tools, Security": the cards **Secrets**, **Migrate Secrets**, **Secret Stores**, with the last two bright.

## Checks for private data

- The sign-in is `admin` / `admin@example.com`; the password is never typed on camera (the session comes from `state.json`).
- Every name, address and host on screen is invented and ends in `example.com` (`noreply@example.com`, `mailer@example.com`,
  `smtp.example.com`). The secret values are dots on screen; if one is ever visible in clear text, record the clip again.
- A fresh site has no real content. OCR the result at one frame per second and at every scene change, and search for anything
  that is not in the data above: a machine name, a user profile folder, a real email address, a key or a token.
- Check that the browser's address bar and the sign-in page are not in any frame, and that no clip shows the folder of
  `App_Data`.
