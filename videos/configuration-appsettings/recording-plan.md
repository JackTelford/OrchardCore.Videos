# Recording plan: configuration-appsettings

A developer video. Almost everything in it is a slide, drawn by `slides.py` when the video is assembled: nothing to
record for those steps. One clip needs a browser: `04-list` (steps `list-1` and `list-2`), which shows a tenant that is
configured in the environment, before it exists, in the admin and on its setup screen. This plan is written for
someone who has not seen the storyboard: everything the recording needs is here.

Labels were read in the Orchard Core source at commit `92db4a83`. Record against a checkout of that commit; if the
checkout differs, compare each label of this plan with its `file:line` before recording.

## The site it needs

A throwaway local site, never a production site.

1. Build and run `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout on `http://localhost:5010`.
2. Give it its own `App_Data` outside both repositories, with `ORCHARD_APP_DATA=<site-data>/App_Data` (an absolute
   path, which `ShellOptionsSetup` combines with the content root: `src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:24`).
   Start from an empty folder.
3. Block outbound calls: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The site calls no external service.
4. Provision the Default tenant with Auto Setup, through environment variables (`src/docs/reference/modules/AutoSetup/README.md`).
   The Default tenant must be signed in (the clip starts on the admin), so this video is recorded with Auto Setup
   and with a `state.json`, unlike a video whose subject is the Setup screen of the Default tenant. The setup
   screen of this video belongs to a second tenant, `MyTenant`.

   | Variable | Value |
   | --- | --- |
   | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
   | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
   | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
   | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
   | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
   | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same as `VIDEO_PASSWORD`) |
   | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
   | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `SaaS` |

   The recipe is `SaaS`, not the `Blog` recipe of the specification: the `SaaS` recipe enables the `Tenants`
   feature (`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:28`, the `Blog` recipe does not), and the clip
   needs the Tenants page. The same `SaaS` recipe is the one `MyTenant` points to.
5. Pre-configure the tenant that the clip shows, with four more environment variables, set before the site starts
   (the configuration is read at startup). They are the same settings as the slide of step `pre-1`, written as
   environment variables:

   | Variable | Value |
   | --- | --- |
   | `OrchardCore__MyTenant__State` | `Uninitialized` |
   | `OrchardCore__MyTenant__RequestUrlPrefix` | `mytenant` |
   | `OrchardCore__MyTenant__DatabaseProvider` | `Sqlite` |
   | `OrchardCore__MyTenant__RecipeName` | `SaaS` |

   Do not set `ConnectionString` (Sqlite needs none), `Secret` or `Description`, and never submit the setup form of
   `MyTenant`: the clip must leave the site as it found it.
6. Sign in once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`,
   which saves `state.json` (git-ignored). Set `MSYS_NO_PATHCONV=1` in Git Bash.
7. Check before recording, with a browser that is signed in: `/Admin/Features` lists `Tenants` as enabled, and
   `/Admin/Tenants` lists two tenants, `Default` (state Running) and `MyTenant` (state Uninitialized, with a
   `Sqlite` and a `SaaS` badge). If `MyTenant` is missing, the variables were not set in the process that runs the
   site. The row must read `MyTenant` with exactly that case: the tenant name is the key of the section, which takes
   the case of the variable name as the process receives it (set the variables from a shell that keeps their case,
   and check the list).

No content, users or other data are seeded. The only fake data is the tenant name `MyTenant`, the address
`admin@example.com` and the site name `Orchard Core`.

## Order and snapshots

One clip, so there is nothing to order. The clip does not change the site (it opens pages and clicks links, and types
nothing). A second take needs no snapshot restore; copy `App_Data` aside after step 4 anyway, so a failed setup of
the Default tenant can be undone.

## Clip `04-list`

Starts on `/Admin`, signed in as `admin` (opened with `scene.OpenAsync("/Admin")`, not recorded). The admin menu
group `Multi-Tenancy` must be closed when the capture starts. The menu script restores the groups that were left open
in the browser, so a reload does not reset it: before `scene.StartCaptureAsync()`, if the link
`#left-nav a[href$="/Admin/Tenants"]` is visible, click the toggle `#left-nav #multitenancy button` once to close the
group, and check that the link is hidden. Then `scene.StartCaptureAsync()`.

Selectors below stand for `scene.Page.Locator(...)`. They were read from the views, not from a running site (see
`OPEN-QUESTIONS.md`): confirm each one with `Recorder probe` before the first take.

### Step `list-1`

Narration: the list under Multi-Tenancy, Tenants shows `MyTenant`, with its Sqlite, SaaS and Uninitialized badges.

Actions, shorter than the narration (about 14 seconds):

1. `scene.ClickAsync(...)` on the toggle of the group `Multi-Tenancy`: `#left-nav #multitenancy button`, only if the
   link `#left-nav a[href$="/Admin/Tenants"]` is not visible (a click on an open group would close it). The group
   opens and shows `Tenants` (`AdminMenu.cs:30-33`).
2. `scene.ClickAndWaitForNavigationAsync(...)` on the `Tenants` link: `#left-nav a[href$="/Admin/Tenants"]`.
3. Wait until the row is shown: `.list-group-item:has-text("MyTenant")`.
4. `scene.HighlightAsync(...)` with that row locator and no caption.

What must be visible when the narration ends: the page `Tenants` with two rows, `Default` (badge `Running`) and
`MyTenant`, which has the ring around it and shows the badges `Sqlite`, `SaaS` and `Uninitialized`. The menu shows
`Multi-Tenancy` open with `Tenants` as the current item. The `Default` row may also show a `Sqlite` badge, because
setup writes the database provider of the tenant into its own `appsettings.json` file
(`src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:160` and `:303`); that is expected. It shows no `SaaS` badge
unless the tenant's file holds a `RecipeName`. No error message, no `Remove` button (tenant removal is not allowed
unless configured).

Where the labels come from:

- `Multi-Tenancy`, `Tenants` (menu): `src/OrchardCore.Modules/OrchardCore.Tenants/AdminMenu.cs:30` and `:33`.
- Badge `Uninitialized`: `Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:12`, in the same module.
- Badge with the database provider (its tooltip is `Database provider`): `Views/ShellSettingsEntry.Database.SummaryAdmin.cshtml:8-9`.
- Badge with the recipe (its tooltip is `Recipe`): `Views/ShellSettingsEntry.Recipe.SummaryAdmin.cshtml:8-9`.
- The list and its row class: `Views/Admin/Index.cshtml:34-37` (default layout `List`,
  `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`).

### Step `list-2`

Narration: `Setup` opens the address of the tenant, and the screen does not ask for a recipe or a database.

Actions, shorter than the narration (about 14 seconds):

1. `scene.ClearHighlightsAsync()`.
2. `scene.ClickAndWaitForNavigationAsync(...)` on the `Setup` button of the `MyTenant` row: `a#btn-setup-MyTenant`
   (`Views/ManageTenantActionButtons.cshtml:24`, shown for an uninitialized tenant when the actions layout is
   `Buttons`, the default: `AdminListOptions.cs:36`). It goes to `http://localhost:5010/mytenant`. If the site's
   admin settings use the `Menu` actions layout, open the actions menu of the row first.
3. Wait until the heading `Setup` and the button `#SubmitButton` are shown.
4. While the narration says "the time zone", `scene.SelectAsync(...)` on `#SiteTimeZone` with the value
   `America/Los_Angeles`. The screen presets the time zone of the machine that runs the site
   (`SetupController.cs`, `Index`: `SiteTimeZone = _clock.GetSystemTimeZone().TimeZoneId`), not the one of Auto Setup,
   and no configuration key changes it: without this step the recording shows the time zone of the recorder's
   machine. Nothing is submitted, so the site does not change.
5. If `Finish Setup` is below the visible area, `scene.ScrollAsync(0, 300)` once. Type nothing else, and click
   nothing else.

What must be visible when the narration ends: the page with the heading `Setup` and the text `Please answer a few
questions to configure your site.`, the field `What is the name of your site?`, the field `Default Time Zone`, the
`Super User` group (`User Name`, `Email`, `Password`, `Password Confirmation`), and the button `Finish Setup`.
There is no `Recipe` picker, no `What type of database to use?`, no `Table Prefix`, no `Connection String`, and no
`Table Schema` field. The site name, user name, e-mail and password fields are empty, and `Default Time Zone` shows
the `America/Los_Angeles` entry. Nothing was typed, so no password or e-mail address appears.

Where the labels come from, in `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml`:

- `Setup` and the lead text: `:102-103`.
- `What is the name of your site?`: `:118`. `Default Time Zone`: `:159`. `Super User`: `:218`. `Finish Setup`: `:256`.
- The recipe picker is not drawn when `RecipeNamePreset` is true (`:124`, set from `RecipeName` by
  `Controllers/SetupController.cs:236`, `CopyShellSettingsValues`), and the whole database block is not drawn when
  `DatabaseConfigurationPreset` is true (`:165`), which is the case for `DatabaseProvider` `Sqlite`, a provider that
  needs no connection string (`SetupController.cs:241`, `HasPresetDatabaseConfiguration`).

## Slide steps

Nothing to record. `slides.py` draws each step, and the narration decides how long it stays.

| Step | The slide shows |
| --- | --- |
| `sections-1` | `IShellConfiguration` as declared in the source, and three cards: application-wide, tenant-specific, an `IConfiguration`. |
| `sections-2` | An `appsettings.json` excerpt with `Media`, `Azure` and `Email`, `Smtp` under `OrchardCore`, and three notes. |
| `sections-3` | Three cards: current names, deprecated names, a value defined under both. |
| `sections-4` | Two cards: modules opt in, and where to find examples. |
| `env-1` | Three environment variable examples, the first one bright, and a card on the double underscore. |
| `env-2` | The same panel with the Default and `MyTenant` examples bright, and a card on the tenant name. |
| `env-3` | Two cards: Azure App Settings, and pipelines. |
| `order-1` to `order-4` | Four numbered cards, the sources in load order; the card of the step is bright. |
| `order-5` | Three `ORCHARD_APP_DATA` examples (relative, absolute, fully qualified) and a card. |
| `pre-1` | The `MyTenant` section with `State`, `RequestUrlPrefix`, `DatabaseProvider`, `RecipeName`, and three notes. |
| `pre-2` | Two cards: a section with a State, a section without one. |
| `post-1` | The `Default` section with a `Media` section inside, and three notes. |
| `post-2` | Two cards: a tenant section shares its path with module sections; pick another name. |
| `file-1` | The tenant file under `App_Data/Sites/MyTenant`, with its keys at the root, and three notes. |
| `file-2` | Three cards: what setup writes in the tenant file, in `tenants.json`, and only what differs. |
| `db-1` | The shared database settings, with the provider, the connection string and the `Default` section bright. |
| `db-2` | The same panel with the `Tenants` section bright. |
| `db-3` | Two cards: providers that share a connection string, and SQLite. |
| `mod-1` | The `Startup` class of the Tenants module, trimmed. |
| `mod-2` | A section for `MyService`, its environment variable, and a note. |
| `mod-3` | The targets file that declares a configuration schema, and two cards. |

## Checks for private data

- The only e-mail address anywhere is `admin@example.com`, and it is not on screen in this clip: the Tenants list
  and the empty setup screen do not show it.
- The password is typed nowhere in the capture (the session comes from `state.json`). `<password>` stays a
  placeholder in this plan; the real one is chosen outside git.
- Check the frames of the contact sheet (`shots/04-list.jpg`): no address other than `localhost:5010` (the browser
  address bar is not in the capture), no real name, no token in the links.
- OCR the video at one frame per second and at every scene change, as the skill asks, and search for e-mail
  addresses and names other than `MyTenant`, `Default`, `admin` and `Orchard Core`.
- The time zone of the setup screen is set to `America/Los_Angeles` by the clip (step `list-2`); look at the contact
  sheet to make sure the recorder's own time zone is not shown.
- Do not commit `state.json`, the copy of `App_Data`, or a script holding real names.
