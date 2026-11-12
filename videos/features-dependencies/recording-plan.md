# Recording plan: features-dependencies

Four clips are recordings (`02-read`, `03-enable`, `04-disable`, `05-demand`); the opening card, the closing card and
the three slides are drawn by the scripts. All the recordings are on one page, **Tools > Features**
(`/Admin/Features`), with one fake pair of features as the example: **Users Registration** and **Email**.

In this plan a selector stands for `scene.Page.Locator(...)`. File references are relative to `src/` of the `OrchardCMS/OrchardCore`
checkout the recording uses (the commit the catalog was read from is in `catalog/features.json`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`, with its own `ORCHARD_APP_DATA` folder
  outside both repositories.
- **Setup**: Auto Setup, so no setup or sign-in form is recorded. Recipe **Blog**, site name **Orchard Core**, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`,
  chosen outside git, never written in this repository). The Default tenant only: no other tenant exists.
- **No email or SMTP configuration anywhere** (no configuration section, no environment variable, no default sender in
  the site settings). Why: when the Email feature is installed for the first time, its data migration enables **SMTP
  Email Provider** as well if the site settings hold a default sender, or the configuration holds a complete SMTP setup
  (a default sender with a host or a pickup directory)
  (`OrchardCore.Modules/OrchardCore.Email/Migrations/EmailMigrations.cs:19-37`,
  `OrchardCore/OrchardCore.Email.Abstractions/SmtpOptions.cs:37-46`). With a fresh site this does not
  happen, and the clips depend on it: otherwise the confirmation dialog of `disable-2` would list two dependents.
- **Outbound calls blocked**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Features to enable before recording**: none. The Blog recipe enables the features the clips need: Users, Roles,
  Settings. Do not enable or disable anything else, and do not change the Admin settings: the clips rely on the
  default admin list layout (**List**, with the row actions as buttons). The layout selector (two small icon buttons) is
  next to the filters; leave **List** selected. Use a browser profile with no stored layout choice (a recording signs in
  with `state.json`, which holds only the sign-in cookie).
- **Seed data**: none. Nothing is created outside the capture, and no content is needed.
- **Check before the first take** (open `/Admin/Features`, outside the capture):
  - Search `Users Registration`: one row, **Disabled**, with the badges **Email** and **Users**.
  - Search `email`: **Email** is **Disabled**, and so is **SMTP Email Provider**.
  - Search `Roles Core Services` after clearing **Hide on demand** (see `demand-1`): it is **Enabled**.
  - Then open `/Admin/Features` again as a new navigation (type the address; do not use the reload button, which can
    restore the values of the form): the search box and the filters are at their defaults.

## Order of the clips

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-read` | snapshot **S0** | No |
| 2 | `05-demand` | snapshot **S0** | No |
| 3 | `03-enable` | snapshot **S0** | **Yes**: enables Users Registration and Email |
| 4 | `04-disable` | snapshot **S1** | **Yes**: disables them again |

- **S0** is the site right after Auto Setup, with the `state.json` sign-in made: copy `ORCHARD_APP_DATA` and `state.json`
  aside before the first take.
- Record `02-read` and `05-demand` from S0, then `03-enable` from S0. Right after `03-enable`, stop the site and copy
  `ORCHARD_APP_DATA` aside again as **S1** (Users Registration and Email enabled). Record `04-disable` from S1.
- Another take of `02-read`, `05-demand` or `03-enable` starts from S0. Another take of `04-disable` starts from S1. To
  record `03-enable` and `04-disable` again from scratch, restore S0 and run `03-enable` first.
- The slides (`why-1`, `why-2`, `sum-1`) need no site.

## Clip `02-read` (chapter 02, Dependencies on the Features page)

Starts on `/Admin/Features`, opened before the capture with `OpenAsync`: the search box is empty, the **Visibility**
filter shows its default (both options selected), the list is at the top, and it is grouped by category.

- `read-1`: wait 2.5 s while the first sentence plays, then `TypeAsync` `Users Registration` into `#search-box`; the
  third sentence plays over the narrowed list. The search narrows the list on every key typed, over the name, the
  identifier and the description (`OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`,
  `OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts:87-113`, `149`).
  On screen when the narration ends: the list narrowed to one row, under the category heading **Security**: the name
  **Users Registration** followed by its identifier, the badges **Email** and **Users**, the three-dot button, the
  **Disabled** badge, the description, and an **Enable** button. Labels: **Tools** and **Features**
  (`OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`), the **Search** box (`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`), the category
  heading (`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:88`).
- `read-2`: at the start, `HighlightAsync` the name (`[data-filter-value*='OrchardCore.Users.Registration'] .fw-semibold`).
  After about 5 s (when the narration reaches the badges), `ClearHighlightsAsync` and `HighlightAsync` the two badges
  (`[data-filter-value*='OrchardCore.Users.Registration'] .direct-dependencies`). On screen at the end: a ring around
  the badges **Email** and **Users**. Labels: name and identifier (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`),
  description (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Description.SummaryAdmin.cshtml`), status badge
  (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:14-16`), dependency badges
  (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-18`). The order Email, Users is by name
  (`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:134-139`).
- `read-3`: `ClearHighlightsAsync`; `MoveToAsync` the badge **Email**
  (`[data-filter-value*='OrchardCore.Users.Registration'] .direct-dependencies .badge` with the text `Email`); wait until
  `.tooltip.show` is visible; hold. On screen at the end: the tooltip over the badge, with the description of the Email
  feature: "Provides the necessary infrastructure for configuring email settings." Source: tooltip =
  description of the dependency (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:15`), description text
  (`OrchardCore.Modules/OrchardCore.Email/Manifest.cs:8`).
- `read-4`: `ClickAsync` the three-dot button
  (`[data-filter-value*='OrchardCore.Users.Registration'] button.badge-show-more`); wait until
  `[data-filter-value*='OrchardCore.Users.Registration'] .transitive-dependencies` no longer has the class `d-none`;
  `HighlightAsync` that element. On screen at the end: after a vertical bar, the badges **Roles Core Services** and
  **Settings**, ringed; the three-dot button has disappeared. Labels: tooltip and hidden text **Show indirect
  dependencies** (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:33-39`); the script that shows the
  indirect dependencies and hides the button (`OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts`, `badgeShowMoreButtons`). The names come from
  the manifests: `OrchardCore.Modules/OrchardCore.Roles/Manifest.cs:20-25` (Roles Core Services), `OrchardCore.Modules/OrchardCore.Settings/Manifest.cs:4`.

## Clip `05-demand` (chapter 05, Features you do not switch by hand)

Starts on `/Admin/Features`, opened before the capture: defaults everywhere. The site is in the state S0.

- `demand-1`: wait 7 s while the first two sentences play. `ClickAsync` the button of the filter titled **Visibility**
  (`button[data-id='visibility-filter']`); the menu opens, with the header **Filter by visibility**, and the options
  **Hide on demand** and **Hide always enabled**, both ticked. Then `ClickAsync` the option **Hide on demand**
  (`.bootstrap-select.show .dropdown-menu .dropdown-item` with the text `Hide on demand`): its tick disappears. Leave the
  menu open. On screen at the end: the open menu, **Hide on demand** unticked, **Hide always enabled** ticked. Labels:
  `OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:64-67`.
- `demand-2`: press Escape to close the menu. `TypeAsync` `Roles Core Services` into `#search-box`. Then `MoveToAsync` the
  wrapper of the inactive button
  (`[data-filter-value*='OrchardCore.Roles.Core'] span[data-bs-toggle='tooltip']:has(> button[disabled])`: the row has other spans with a tooltip, the badges, so the selector must name the one that holds the inactive button) and wait until `.tooltip.show` is
  visible (this starts at about 7 s, when the narration reaches the button). On screen at the end: one row under
  **Security**: **Roles Core Services**, the badge **Enabled**, the description "Provides role core services." (no dependency badge), and a red
  **Disable** button that is inactive, with its tooltip "This feature is automatically enabled based on dependency and
  cannot be manually disabled." Labels: `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:14-21` (tooltip and button),
  `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:6-10` (badge), `OrchardCore.Modules/OrchardCore.Roles/Manifest.cs:19-25` (name,
  description). Users needs it: `OrchardCore.Modules/OrchardCore.Users/Manifest.cs:17`.
- `demand-3`: `ClickAsync` the **Visibility** button again, `ClickAsync` the option **Hide always enabled** (its tick
  disappears), press Escape. Click the search box, press Escape (the page empties the search box), and `TypeAsync`
  `OrchardCore.Features`. `MoveToAsync` the wrapper of the inactive button
  (`[data-filter-value*='OrchardCore.Features'] span[data-bs-toggle='tooltip']:has(> button[disabled])`, for the same reason) at about 10 s and wait until
  `.tooltip.show` is visible. On screen at the end: one row under **Infrastructure**: **Features**, its identifier, the dependency badge
  **Resources**, the badge **Enabled**, an inactive red **Disable** button, and its tooltip "This feature is always enabled and cannot be
  manually disabled." Labels: `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:30-35`, `OrchardCore.Modules/OrchardCore.Features/Manifest.cs:11-18`
  (name, category `Infrastructure`, always enabled, dependency
  `OrchardCore.Resources`, whose name is "Resources": `OrchardCore.Modules/OrchardCore.Resources/Manifest.cs:4`).

## Clip `03-enable` (chapter 03, Enable a feature)

Starts on `/Admin/Features`, opened before the capture: defaults everywhere. State S0 (Users Registration, Email and SMTP
Email Provider are all **Disabled**). This clip changes the site.

- `enable-1`: `TypeAsync` `email` into `#search-box`. After about 2.5 s, `HighlightAsync` the **Disabled** badge of the
  Email row (`[data-filter-value^='Email OrchardCore.Email '] .metadata .badge`). After about 5 s, `ClearHighlightsAsync`,
  scroll down (`ScrollAsync`, about 400 px, enough to show the whole **Users Registration** row under **Security**) and
  `HighlightAsync` its **Disabled** badge (`[data-filter-value*='OrchardCore.Users.Registration'] .metadata .badge`). The
  search matches several rows (Communication, Notifications, Security): they do not all fit on one screen. On screen at the
  end: the **Users Registration** row, with a ring around **Disabled**, and its **Enable** button. Labels:
  `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:14-16`; Email is "Email" (`OrchardCore.Modules/OrchardCore.Email/Manifest.cs:4`).
- `enable-2`: `ClearHighlightsAsync`; scroll so that the **Users Registration** row is in view if it is not. After about 2 s
  (when the narration says "Select Enable"), `ClickAndWaitForNavigationAsync` the **Enable** button of the row
  (`a[href$='/Features/OrchardCore.Users.Registration/Enable']`; it has no confirmation). Put the wait for the result inside
  `OffCameraAsync`: wait until the text "The feature Users Registration was enabled." is visible (the tenant restarts, so the
  page can take several seconds). On screen at the end: the Features page reloaded from the top (the search box is empty),
  with the success message "The feature Users Registration was enabled." Source: route and action
  (`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:226-251`), message (`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:313-318`); it names only the
  features passed to the action, that is the one selected, not the dependencies enabled with it. The button has no
  confirmation (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:38-41`).
- `enable-3`: `TypeAsync` `email` into `#search-box`. After about 3 s, `HighlightAsync` the **Enabled** badge of the Email row
  (`[data-filter-value^='Email OrchardCore.Email '] .metadata .badge`). After about 8 s, `ClearHighlightsAsync`, scroll
  down as in `enable-1`, and `HighlightAsync` the **Enabled** badge of the **Users Registration** row. On screen at the
  end: the Users Registration row **Enabled**, with a **Disable** button; the row of **SMTP Email Provider** stays
  **Disabled** (do not highlight it). Labels: `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:6-10`. The rule: an
  enable adds the dependencies that are not enabled and leaves the others as they are
  (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163-181`; the action forces it, `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:247`).

After this clip, stop the site and save snapshot **S1**.

## Clip `04-disable` (chapter 04, Disable a feature)

Starts on `/Admin/Features`, opened before the capture: defaults everywhere. State S1 (Users Registration and Email
**Enabled**, SMTP Email Provider **Disabled**). This clip changes the site.

- `disable-1`: after about 1.5 s, `TypeAsync` `email` into `#search-box`. After about 3.5 s, `MoveToAsync` and `ClickAsync` the
  **Disable** button of the Email row (`a[href$='/Features/OrchardCore.Email/Disable']`; the `.Smtp` row has a different
  address). Click it last, so the dialog opens at the end of the step. Wait until `#confirmRemoveModal` is visible. On
  screen at the end: the dialog over the dimmed list. Labels: **Disable** (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`).
- `disable-2`: hold the dialog open. At about 5 s, when the narration says "lists the dependent features",
  `HighlightAsync` the list in the dialog (`#confirmRemoveModal .modal-body ul`). On screen at the end: the dialog with the
  title **Disable Feature**; the text "Disabling the Email feature will also disable the following dependent features:",
  a bulleted list with one item, **Users Registration**, and "Continue?"; the buttons **Yes** and **No**. Source: title,
  buttons and message (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:45-53`); the dependent features are those enabled
  that depend on it, directly or not (`OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:55-65` (line 64),
  `OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:351-380`); the dialog's markup, where the message is HTML
  (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:15-49`). Without enabled dependents the message is
  "Are you sure you want to disable the {0} feature? Continue?" (line 45): not recorded.
- `disable-3`: `ClearHighlightsAsync`; after about 1.5 s (when the narration says "Select Yes"),
  `ClickAndWaitForNavigationAsync` the button **Yes** (`#modalOkButton`). Put the wait inside `OffCameraAsync`: wait until
  the text "The feature Email was disabled." is visible. Then `TypeAsync` `registration` into `#search-box` (before 4.5 s
  after the step started) and `HighlightAsync` the **Disabled** badge of the Users Registration row
  (`[data-filter-value*='OrchardCore.Users.Registration'] .metadata .badge`). On screen at the end: the **Users
  Registration** row, **Disabled**, with an **Enable** button, ringed badge; the message "The feature Email was disabled."
  stays above if the page still shows it. Source: message (`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:319-322`); the dependents are
  disabled with it (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:190-208`, forced by `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:214`).

## Slides

- `why-1`: slide "A feature can need other features": two cards, **Users Registration** (lets visitors sign up and confirm
  their email address) and **Email and Users** (the features it needs: its dependencies).
- `why-2`: slide "Both sides stay consistent": two cards, **Enable a feature** (its dependencies are enabled with it) and
  **Disable a feature** (the features that depend on it are disabled with it).
- `sum-1`: slide "Two cases to know": two cards, **On demand** (enabled while another feature needs it, disabled when
  none does) and **Always enabled** (stays enabled, and its Disable button is inactive). Source: the inactive buttons
  and their tooltips (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:14-35`);
  on demand features are dropped from the stored list at every update and added back when the shell loads because an
  enabled feature needs them (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:50-66`,
  `OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:74-88`).

## Checks for private data

- The Features page lists modules only: no person, company, address or email appears in the clips. The only identity of
  the site is the user `admin` (and `admin@example.com`, which never appears on these pages). Do not open the user menu or
  the profile during a take.
- Look at every contact sheet: no error page, no empty list, no stack trace, no other message than the ones named above.
- OCR the video at one frame per second and at every scene change, and search for the real names of the person who
  recorded, their machine name, their email addresses, and the host name of the site (it must be `localhost`).
- The password is never typed on camera (the sign-in is `state.json`), and no recording starts on a sign-in page.
