# Recording plan: OpenID Connect Token Validation

Read this file and nothing else to record the video. Every label below was read in the Orchard Core source at the
commit named in `catalog/SOURCE.md`; the file and line follow each one. A selector in this file stands for
`scene.Page.Locator(...)`. The video is a developer video: only two chapters are recordings (`03-enable` and
`04-settings`); the other chapters are slides and cards that `assemble.py` draws from `slides.py`.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Setup | Auto Setup through environment variables, so no setup form or sign-in form is recorded (see `references/recording.md` and the `orchardcore-tester` skill) |
| Recipe | Blog |
| Tenant | `Default` (the only tenant; no second tenant is recorded) |
| Site name | Orchard Core |
| Time zone | `America/Los_Angeles` |
| Administrator | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The remote server of the video, `idp.example.com`, is never contacted: the Token Validation page checks the form of the values only (`Services/OpenIdValidationService.cs`, `ValidateSettingsAsync`) |
| Sign-in | `Recorder setup` signs in once and writes `state.json` (git-ignored); run it before taking snapshot A, so the snapshot holds a session that stays valid after a restore. The sign-in form is never recorded |
| External services | none |

### Features to enable before recording (outside the capture)

Open `/Admin/Features` and enable **OpenID Connect Authorization Server** (Tools > Features, feature id
`OrchardCore.OpenId.Server`). Its dependencies, OpenID Connect Management UI and OpenID Connect Core Services, are
enabled with it.

**Leave OpenID Connect Token Validation (`OrchardCore.OpenId.Validation`) disabled.** Clip `03-enable` enables it on
camera.

Do not open or save Settings > OpenID Connect > Token Validation, and do not change the Authorization Server
settings, before the take. Clip `04-settings` relies on the page opening with no stored validation settings: with the
Authorization Server feature enabled and nothing stored, the page opens with the current tenant name, `Default`, in
Authorization server tenant (`Services/OpenIdValidationService.cs:51-66`).

### Seed data

None. The values typed during the capture are fake:

| Field | Value |
|---|---|
| Authority | `https://idp.example.com` |
| Audience | `inventory-api` |

## The order of the clips, and what they change

Record in the order the chapters play, because the second clip needs the feature the first one enables.

| Order | Clip | Starts on | Changes the site | Depends on |
|---|---|---|---|---|
| 1 | `03-enable` | `/Admin/Features` | Enables the Token Validation feature (OpenID Connect Core Services is already enabled by the Authorization Server) | the seed above |
| 2 | `04-settings` | `/Admin` | Saves Token Validation settings: Authority `https://idp.example.com`, Audience `inventory-api` | `03-enable` |

Take two snapshots of the site (stop it, copy `App_Data` and `state.json` aside, start it again):

- **Snapshot A**, after the seed and before `03-enable`. Restore it before every take of `03-enable`.
- **Snapshot B**, after `03-enable` and before `04-settings`. Restore it before every take of `04-settings`.

To record everything again, restore snapshot A, record `03-enable`, stop the site, take snapshot B again (or restore
the copy), and record `04-settings`.

## Chapter 00: opening card (`00-intro`)

Step `intro`: card. The opening card shows the logo over the title "OpenID Connect Token Validation" and the subtitle
"Protect your APIs with access tokens". Nothing is recorded.

## Chapter 01: How an API checks a token (`01-flow`, slides)

Nothing is recorded.

- `flow-1`: slide "Four parts of an API call": four cards (Client, API scheme, Token handler, Your endpoint), the first
  two bright.
- `flow-2`: slide "The API scheme forwards the request": the trimmed `ApiAuthenticationHandler` code.
- `flow-3`: slide "The feature chooses what it forwards to": `ApiAuthorizationOptions` and the two `Configure` methods
  of the validation configuration.

## Chapter 02: Who issues the tokens (`02-issuer`, slides)

Nothing is recorded.

- `issuer-1`: slide "Who issues the tokens": three cards (Authorization server tenant, Authority, Exactly one), all bright.
- `issuer-2`: the same slide, the Authorization server tenant card bright.
- `issuer-3`: the same slide, the Authority card bright.
- `issuer-4`: slide "Another tenant as the issuer": three note cards about the scope, the tenant prefix, and reference access tokens.

## Chapter 03: Enable the feature (`03-enable`, recording)

Method name: `03-enable`. Restore snapshot A first. Start page: `/Admin/Features` (`scene.OpenAsync("/Admin/Features")`,
not recorded). Leave the list layout and the filters at their defaults. Then `StartCaptureAsync()`.

Where the labels come from:

- Tools > Features: `OrchardCore.Features/AdminMenu.cs:26-27`.
- The search field, `#search-box`, placeholder "Search": `OrchardCore.Features/Views/Admin/Features.cshtml:27`.
- The category heading "OpenID Connect": `Features.cshtml:88` (the feature's `Category`, `OrchardCore.OpenId/Manifest.cs:70`).
- The feature name "OpenID Connect Token Validation": `OrchardCore.OpenId/Manifest.cs:68`.
- The feature names "OpenID Connect Core Services" and "OpenID Connect Authorization Server" (narrated in `enable-2` and
  `settings-2`): `OrchardCore.OpenId/Manifest.cs:13` and `:44`. Core Services depends on nothing and is
  `EnabledByDependencyOnly` (`Manifest.cs:16`), so it is hidden by the default "Hide on demand" filter.
- The permission "View and edit the OpenID Connect validation settings" (narrated in `enable-2`):
  `OrchardCore.OpenId.Core/OpenIdPermissions.cs:20-21`; the Administrator role holds it by default
  (`OrchardCore.OpenId/Permissions.cs:68-75`).
- The button "Enable" and the status "Enabled": `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`,
  `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

Each feature row carries a `data-filter-value` attribute that holds its name, id and description
(`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs`), which the search matches. The row of the video is
`[data-filter-value*="OpenID Connect Token Validation"]` narrowed to the row whose name is exactly that: the
description of OpenID Connect Authorization Server (`Manifest.cs:45`) mentions the phrase too, so a search for "token
validation" shows two rows.

- `enable-1`:
  1. `TypeAsync(#search-box, "token validation")`. The list filters while typing (the `input` event).
  2. Wait until the row named "OpenID Connect Token Validation" is visible.
  3. `HighlightAsync` the Token Validation row only (not the Authorization Server row).
  4. On screen when the narration ends: the search field holding "token validation", the "OpenID Connect" heading, the
     highlighted row "OpenID Connect Token Validation" with the status Disabled and a blue Enable button. The row of
     OpenID Connect Authorization Server (Enabled) may be shown under it.
- `enable-2`:
  1. `ClearHighlightsAsync()`.
  2. `ClickAndWaitForNavigationAsync` on the Enable button (`a.btn` with the text "Enable") of the Token Validation
     row. The page reloads on the same list (`GetNextUrl`, `OrchardCore.Features/Controllers/AdminController.cs:287`).
     The success message reads "The feature OpenID Connect Token Validation was enabled."
     (`AdminController.cs:313-317`; only the requested feature is listed, since Core Services is already enabled).
  3. The tenant restarts on the first request after the redirect, so wait, with a timeout of 30 seconds, until `#search-box`
     is visible. Then `TypeAsync(#search-box, "token validation")` again, because the reload clears the search.
  4. `HighlightAsync` the Token Validation row.
  5. On screen when the narration ends: the Token Validation row with the status Enabled and a red Disable button (do
     not click it), under the "OpenID Connect" heading.

## Chapter 04: Configure the validation (`04-settings`, recording)

Method name: `04-settings`. Restore snapshot B first (the Token Validation feature is enabled, nothing is stored).
Start page: `/Admin` (`scene.OpenAsync("/Admin")`, not recorded), with the admin menu visible and its Settings entry
collapsed. Then `StartCaptureAsync()`.

The viewport is 1536x864 CSS pixels and this page carries two or three warnings and long hints. Before every
`HighlightAsync` or click in this clip, check that the element is inside the viewport; if it is not, call
`scene.ScrollAsync(0, y)` to bring it in. Highlights are fixed-position: call `ClearHighlightsAsync()` before any scroll,
and highlight again after it. After a save the page loads at the top again.

Where the labels come from, all in `OrchardCore.OpenId`:

- Settings > OpenID Connect > Token Validation: `ValidationAdminMenu.cs:18-20`. The page is also reachable at
  `/Admin/OpenId/ValidationConfiguration` (`Controllers/ValidationConfigurationController.cs:49`).
- The page title (breadcrumb) "Configure OpenID Connect validation settings": `Views/ValidationConfiguration/Index.cshtml:4`.
- The warning "The website might be restarted upon saving the settings, ...": `Index.cshtml:11`.
- The warning about a separate tenant, "To be able to validate tokens issued by a separate tenant, ...":
  `Views/OpenIdValidationSettings.Edit.cshtml:7`.
- Authorization server tenant (`input.openid-validation-tenant`): `OpenIdValidationSettings.Edit.cshtml:15`.
- Authority (`#AuthorityField`, `input.openid-validation-authority`): `OpenIdValidationSettings.Edit.cshtml:22-25`.
- OpenID Connect Metadata Address (no id; the wrapper that holds that label): `OpenIdValidationSettings.Edit.cshtml:32`.
- Audience (`#AudienceField`, `input.openid-validation-audience`): `OpenIdValidationSettings.Edit.cshtml:40-43`.
- Disable token type validation (`#DisableTokenTypeValidationField`): `OpenIdValidationSettings.Edit.cshtml:53`.
- The button "Save": `Index.cshtml:17`.
- The feature name "OpenID Connect Authorization Server" and the narrated "Authorization Server" (`settings-2`,
  `settings-6`): `Manifest.cs:44`.
- The error "An audience must be set when configuring the authority.": `Services/OpenIdValidationService.cs:152`.
- The message "OpenID validation configuration successfully updated.": `Controllers/ValidationConfigurationController.cs:96`.

The script `Assets/ts/openid-validation-settings.ts` hides Authority, Audience and Disable token type validation
while Authorization server tenant holds text, and shows them when it is empty. It listens for the `change` event,
which a text field raises when it loses focus: press Tab after editing the tenant field.

- `settings-1`:
  1. `ClickAsync` the admin menu entry "Settings" (it expands), then the entry "OpenID Connect", then
     `ClickAndWaitForNavigationAsync` on the entry "Token Validation". If the menu entries cannot be driven reliably,
     call `scene.NavigateAsync("/Admin/OpenId/ValidationConfiguration")` instead of the three clicks.
  2. Wait until the title "Configure OpenID Connect validation settings" is visible.
  3. `HighlightAsync` the first warning, the one that begins "The website might be restarted".
  4. On screen when the narration ends: the page title, the highlighted warning about a restart, the field Authorization
     server tenant. The same warning can appear twice on the page (once in the page, once from the editor wrapper,
     `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml`); highlight the first.
- `settings-2`:
  1. `ClearHighlightsAsync()`, then `HighlightAsync(input.openid-validation-tenant)`.
  2. On screen when the narration ends: Authorization server tenant holding `Default`, highlighted, with the field
     OpenID Connect Metadata Address under it and the Save button. Authority, Audience and Disable token type validation
     are not shown (the script hides them while the tenant field holds text).
- `settings-3`:
  1. `ClearHighlightsAsync()`. `ClickAsync(input.openid-validation-tenant)`, press `Control+A`, then `Backspace`, then
     `Tab`.
  2. Wait until `#AuthorityField` is visible.
  3. `HighlightAsync` `#AuthorityField`, `#AudienceField` and `#DisableTokenTypeValidationField`, in that order,
     400 ms apart, then the Metadata Address wrapper (the field that was already shown; the script never hides it).
  4. On screen when the narration ends: the empty Authorization server tenant field and four highlighted fields:
     Authority, Audience, the checkbox Disable token type validation, and OpenID Connect Metadata Address.
- `settings-4`:
  1. `ClearHighlightsAsync()` (the highlights are fixed-position: clear them before anything scrolls).
  2. `TypeAsync(input.openid-validation-authority, "https://idp.example.com")`, then `Tab`.
  3. Leave Audience empty. `ClickAndWaitForNavigationAsync` on the button "Save".
  4. Wait until the text "An audience must be set when configuring the authority." is visible.
  5. On screen when the narration ends: the page at the top, with the error message "An audience must be set when
     configuring the authority." in the summary above the form, and Authority still holding the address.
- `settings-5`:
  1. `TypeAsync(input.openid-validation-audience, "inventory-api")`, then `Tab`.
  2. `ClickAndWaitForNavigationAsync` on the button "Save".
  3. Wait, with a timeout of 30 seconds (the tenant is released and starts again), until the text
     "OpenID validation configuration successfully updated." is visible. Wait for that text, not for a fixed time.
  4. On screen when the narration ends: the message "OpenID validation configuration successfully updated.", and under
     it the form with Authority `https://idp.example.com/` and Audience `inventory-api`.
- `settings-6`:
  1. `HighlightAsync` `#AuthorityField`, then `#AudienceField`.
  2. Hold. No other action.
  3. On screen when the narration ends: the saved form with Authority and Audience highlighted.
  4. `ClearHighlightsAsync()` before the clip ends.

  The narration says that this site also runs the Authorization Server and that its issuer and keys are applied after
  the authority. This follows `Configuration/OpenIdValidationConfiguration.cs:100-187`: no `return` follows the block
  that applies an Authority, and the block below it reads the Authorization Server of the same tenant, which is enabled
  here with valid default settings (`Services/OpenIdServerService.cs:60-83`). Do not call an endpoint with a token on this
  site to "prove" the remote server: by that code it is not accepted here. See `OPEN-QUESTIONS.md`.

The clip leaves the site with a remote Authority saved. Restore snapshot B before another take.

## Chapter 05: Protect your own endpoint (`05-endpoint`, slides)

Nothing is recorded.

- `endpoint-1`: slide "Name the API scheme": the trimmed `ContentApiController` of OrchardCore.Demo, lines 1 to 4 bright.
- `endpoint-2`: slide "Check the permission in the action": the same code, the permission check and `ChallengeOrForbid` bright (the action decides, since the class also carries `AllowAnonymous`).
- `endpoint-3`: slide "What the caller receives": three note cards: no valid token (challenged), valid token without the
  permission (forbidden), valid token with the permission (the action carries on).

## Chapter 06: Set it from a recipe (`06-recipe`, slides)

Nothing is recorded.

- `recipe-1`: slide "The validation settings step": a recipe that enables the features and sets Tenant, and a second
  step for a remote server with Authority and Audience.
- `recipe-2`: slide "A recipe is not validated on save": three note cards (the page validates, the recipe step does
  not, invalid values register no handler).

## Chapter 99: closing card (`99-outro`)

Step `outro`: card. The closing card shows the logo over the title and the subtitle "docs.orchardcore.net". Nothing is
recorded.

## Checks for private data

- The only user on the site is `admin` / `admin@example.com`. The password is typed outside the capture and never
  appears in a clip. The sign-in form is not recorded.
- The only server address typed is `https://idp.example.com`; the only audience is `inventory-api`.
- The recorded pages show no tenant other than `Default`, no real host name, no token and no key.
- OCR the final video at one frame per second and at every scene change (see `SKILL.md`, "Check for private data") and
  search for any email address other than `@example.com`, any host name other than `localhost` and `example.com`, and
  the password.
- Remove the site's `App_Data` and the snapshots after the last take; do not commit them.
