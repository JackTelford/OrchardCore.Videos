# Recording plan: Auto Setup

Two chapters of this video are recordings: `01-setup-screen` (the Setup screen of a site that is not installed) and
`05-first-request` (a site installing itself on its first request). Everything else is slides, drawn by `slides.py`.
This plan is for the person at the recorder: it holds the commands, selectors and `scene` calls that the narration and
the slides do not. In this file a selector stands for `scene.Page.Locator(...)`.

**Read this first.** The subject of this video is the Setup screen and Auto Setup itself, so the site is **not
provisioned with Auto Setup before the recording**, and there is **no `state.json`**:

- Do not run `Recorder setup`. It requests `/Login`, and on a site with Auto Setup configured and an empty data folder
  any request installs the site, which is the very thing `05-first-request` records.
- Delete `state.json` from the video folder if one exists. Both clips run signed out; `05-first-request` signs in on
  camera.
- Do not use a readiness probe that sends an HTTP request. Wait for the line `Now listening on: http://localhost:5010`
  in the console of the site (the NLog console target of `src/OrchardCore.Cms.Web/NLog.config` prints the
  `Microsoft.Hosting.Lifetime` events).
- Do not run `Recorder probe` against the site configured for `05-first-request` before the take: it requests a page,
  and the first request installs the site. Probe the Setup screen only on the site without Auto Setup configuration
  (`Recorder probe` passes `state.json` to the browser without checking that the file exists, `Program.cs:77` of the
  recorder; if you have none, keep a file outside git that holds `{"cookies": [], "origins": []}`). The recorder's
  `record` command sends no request of its own: only the clip's own calls do (`Program.cs:33` to `:36`).

## The site it needs

A checkout of `OrchardCMS/OrchardCore` (the branch that matches the documentation being shown), run from
`src/OrchardCore.Cms.Web`. Its `Program.cs` registers `OrchardCore.AutoSetup` as a setup feature, so the feature needs no
other action (`src/OrchardCore.Cms.Web/Program.cs:9`).

| | Clip `01-setup-screen` | Clip `05-first-request` |
|---|---|---|
| Data folder | an empty folder outside both repositories (`ORCHARD_APP_DATA`) | another empty folder, or the same one after deleting everything in it |
| Auto Setup configuration | **none**, anywhere | the environment variables below |
| Port | 5010 (`VIDEO_BASE_URL=http://localhost:5010`) | the same |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address | the same |
| Signed in | no | no (the clip signs in) |

Start the site once, without building again, in the Development environment, for example from the Orchard Core
checkout:

```bash
export ORCHARD_APP_DATA=<an empty folder outside the repositories>
export ASPNETCORE_ENVIRONMENT=Development
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
dotnet build -c Release src/OrchardCore.Cms.Web
dotnet run --no-build --no-launch-profile -c Release --project src/OrchardCore.Cms.Web --urls http://localhost:5010
```

The environment is stated, not chosen. `--no-launch-profile` keeps the `Kestrel` profile of
`src/OrchardCore.Cms.Web/Properties/launchSettings.json` (which sets `launchBrowser`, `https://localhost:5001` and the
same Development environment) from being applied. Development matters for two reasons: the user secrets of the
machine are read (hence the check below), and `appsettings.Development.json` sets the log level to `Debug`, so the
Information level lines of Auto Setup are written to the log file. The console only shows the lines of
`Microsoft.Hosting.Lifetime`; the Auto Setup lines go to the log file, in the `logs` folder of the data folder
(`ORCHARD_APP_DATA`, `src/OrchardCore/OrchardCore.Logging.NLog/HostBuilderExtensions.cs:25` to `:31`).

### No Auto Setup configuration (clip `01-setup-screen`)

The Setup screen only appears when Auto Setup finds no configuration (`src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:92` to `:97`).
Check all three places before starting the site:

1. No variable whose name contains `AutoSetup` (any case) is set in the shell. The module also reads the legacy section
   `OrchardCore_AutoSetup` (`Startup.cs:29`, `:67`), which as a variable is `OrchardCore__OrchardCore_AutoSetup__...`, and
   the shell configuration of a tenant can hold the section under the tenant's own name, so a prefix check misses them.
2. `dotnet user-secrets list --project src/OrchardCore.Cms.Web` prints no key that contains `AutoSetup`.
   `OrchardCore.Cms.Web` shares a `UserSecretsId` (`OrchardCore.Cms.Web.csproj:15`), so a secret set while working on
   another video is read here.
3. No `AutoSetup` section is active in `src/OrchardCore.Cms.Web/appsettings.json` or an `appsettings.*.json` file. The
   example in `appsettings.json` is commented out (lines 249 to 282) and must stay so.

### Auto Setup configuration (clip `05-first-request`)

Set these variables in the shell that starts the site, with the fake values below. `VIDEO_PASSWORD` is the password
chosen outside git (the same one the recorder reads); it must meet the password rules that the Password field of the
Setup screen lists, or the installation fails with `The AutoSetup failed installing the site.`
(`src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:125`, `src/OrchardCore.Modules/OrchardCore.Users/Services/SetupEventHandler.cs:33`).

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName="Default"
export OrchardCore__AutoSetup__Tenants__0__SiteName="Lakeside Journal"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone="America/Los_Angeles"
export OrchardCore__AutoSetup__Tenants__0__AdminUsername="admin"
export OrchardCore__AutoSetup__Tenants__0__AdminEmail="admin@example.com"
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider="Sqlite"
export OrchardCore__AutoSetup__Tenants__0__RecipeName="Blog"
```

`AutoSetupPath` stays unset, so any first request installs the site. No other tenant is configured, and the Blog
recipe creates all the content: there is no seed data to create outside the capture. User secrets and
`appsettings.json` must not hold a conflicting `AutoSetup` section.

## Order of the clips

| Order | Clip | Site before | Site after |
|---|---|---|---|
| 1 | `01-setup-screen` | started without Auto Setup configuration, data folder empty, not requested yet | the Default tenant is still not installed (its settings file exists, nothing else); the form is never submitted |
| 2 | `05-first-request` | **stop the site, empty the data folder**, start it with the variables above, and do not request it | installed, and signed in as `admin` |

The clips do not depend on each other, but they need different start-up configurations, so record `01-setup-screen`
first. `01-setup-screen` can be recorded again on the same run. **`05-first-request` changes the site and cannot be
recorded again on the same data:** for each take, stop the site, delete everything in the data folder, and start it
again. Never copy a site's data folder into the repository.

## Clip `01-setup-screen`

Starts on: `/` of the site without configuration, which shows the Setup screen. Before `StartCaptureAsync`:

```csharp
await scene.OpenAsync("/");
await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");   // not recorded
await scene.StartCaptureAsync();
```

The time zone is chosen off camera so the screen does not show the time zone of the recording machine. Do not type in
any field, do not click `Finish Setup`, and do not click the eye buttons.

- `setup-1`: Actions: highlight, one after another while the narration names them, the site name field (the label
  `What is the name of your site?`), the Recipe drop-down (`#recipeButton`), and the time zone select (`#SiteTimeZone`,
  label `Default Time Zone`), with `scene.HighlightAsync(...)` and `scene.MoveToAsync(...)`; call `ClearHighlightsAsync()`
  at the end. On screen when the narration ends: the top of the Setup screen, with the heading `Setup`, the three fields
  empty or at their defaults, and the time zone select highlighted. Labels: `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:102`
  (`Setup`), `:118` (`What is the name of your site?`), `:130` (`Recipe`), `:159` (`Default Time Zone`).
- `setup-2`: Actions: `await scene.ScrollAsync(0, 300)` until the database row is in view, highlight `#DatabaseProvider`
  (label `What type of database to use?`), then `await scene.SelectAsync(scene.Page.Locator("#DatabaseProvider"), "Postgres")`,
  which shows the three extra fields (the Sqlite provider, the default, shows none of them), and highlight `Table Prefix`,
  `Connection String` and `Table Schema` in turn, the order of the narration and of the page from top to bottom.
  Clear the highlights before scrolling again. On screen when the narration ends: the database select showing
  `Postgres`, with the field `Table Prefix` to its right in the same row, and below them `Connection String` and
  `Table Schema` (`Index.cshtml:172` to `:214`). Labels: `Index.cshtml:176`
  (`What type of database to use?`), `:189` (`Table Prefix`), `:198` (`Connection String`), `:210` (`Table Schema`);
  the fields appear for providers that have them (`Sql Server`, `MySql` and `Postgres` have a connection string and a
  table prefix; `Sqlite` has neither, and `Table Schema` follows the table prefix): `src/OrchardCore.Modules/OrchardCore.Setup/Assets/ts/setup.ts:4`,
  `Index.cshtml:188` and `:209`;
  the provider names (`Sql Server`, `Sqlite`, `MySql`, `Postgres`): `src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:52`.
- `setup-3`: Actions: `await scene.ScrollAsync(0, 500)` until the `Super User` block is in view, highlight the
  legend `Super User`, then `User Name` (`#UserName`), `Email` (`#Email`), `Password` (`#Password`) and
  `Password Confirmation` (`#PasswordConfirmation`) in turn. Nothing is typed. On screen when the narration ends: the
  Super User block with the four fields empty, and the `Finish Setup` button below it. Labels: `Index.cshtml:218`
  (`Super User`), `:222` (`User Name`), `:227` (`Email`), `:234` (`Password`), `:244` (`Password Confirmation`), `:256`
  (`Finish Setup`).

## Clip `05-first-request`

Starts on: the scene's own blank page. Do **not** call `scene.OpenAsync`: the first request to the site has to happen
during the capture. The installation of the Blog recipe can take longer than the scene's 20-second default, so raise the
navigation timeout for this clip first.

```csharp
await using var scene = await NewSceneAsync(browser, directory, durations);
scene.Page.SetDefaultNavigationTimeout(180_000);
await scene.StartCaptureAsync();
```

- `first-1`: Actions: let the first words of the narration play over the blank page (`await scene.HoldAsync(3)`), then
  request the home page and cut the wait out of the clip with `await scene.OffCameraAsync(() => scene.NavigateAsync("/"))`.
  `NavigateAsync` waits for the network to be idle, which is after the redirect that ends the installation; the cut
  takes the whole installation out of the video. If the first frame after the cut is still blank, add
  `await scene.PauseAsync(500)` after it. This request is the one that installs the site, so it must be the first request
  the site ever gets. On screen when the narration ends: the home page of the Blog site, with `Lakeside Journal` in the
  navigation bar (`.navbar-brand`). Source: `src/OrchardCore.Modules/OrchardCore.AutoSetup/AutoSetupMiddleware.cs:95` to
  `:155` (the installation, and the redirect to the root of the site at line 145), the site name in the bar:
  `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`.
- `first-2`: Actions: `await scene.HighlightAsync(scene.Page.Locator(".navbar-brand"))` on the site name, then move
  the cursor down over the first blog content with `scene.MoveToAsync(...)`; clear the highlight before the step ends.
  On screen when the narration ends: the home page with `Lakeside Journal` in the navigation bar and blog content
  below it, with no error page and no `Service Unavailable` text. The name comes from the `SiteName` value
  (`src/OrchardCore.Modules/OrchardCore.Settings/Services/SetupEventHandler.cs:23`).
- `first-3`: Actions: `await scene.NavigateAsync("/Login")`, then `await scene.TypeAsync(scene.Page.Locator("#LoginForm_UserName"), "admin")`,
  then `await scene.TypeAsync(scene.Page.Locator("#LoginForm_Password"), Scenes.Password)` (the field masks what is typed:
  never click `#togglePassword`), then `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form.auth-form button[type=submit]"))`.
  On screen when the narration ends: the page that follows the sign-in (the home page or the admin), with the user
  signed in and no password visible. Labels: `Log in`, `Username or email address`, `Password`:
  `src/OrchardCore.Modules/OrchardCore.Users/Views/Account/Login.cshtml:27`, `.../Views/LoginFormCredentials.cshtml:7` and `:13`, `.../Views/LoginForm.Edit.cshtml:9`.
  The selectors `#LoginForm_UserName` and `#LoginForm_Password` are the recorder's own (`Program.cs` of the recorder, `SetupAsync`).
- `first-4`: Actions: `await scene.NavigateAsync("/Admin/Settings/general")`, then highlight the `Site name` field
  (`scene.Page.GetByLabel("Site name")`) and the `Default Time Zone` select (`scene.Page.GetByLabel("Default Time Zone")`)
  in turn; clear the highlights at the end. On screen when the narration ends: the `Settings` page of the admin
  (breadcrumb `Settings`) with `Site name` showing `Lakeside Journal`, and `Default Time Zone` showing the entry for
  `America/Los_Angeles`, highlighted. The menu path in the narration is `Settings`, `General`
  (`src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` to `:41`); the page route is `Settings/{groupId}` with
  the group `general` (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`,
  `Drivers/DefaultSiteSettingsDisplayDriver.cs:14`). The labels: `Settings-Site.Edit.cshtml:16` (`Site name`) and `:43` (`Default Time Zone`).

## Slides

Each slide step is drawn by `slides.py`; nothing is recorded for it.

- `register-1`: the Program.cs of a host, with the three lines that register `OrchardCore.AutoSetup` highlighted.
- `register-2`: the one call to add (highlighted), next to three cards: the Orchard Core web project registers it, the
  CMS template does not, and the Features page has no Enable button for it.
- `config-1`: the AutoSetup section of `appsettings.json` for the Default tenant, with three cards (section, Tenants, Default).
- `config-2`: four cards that map the Setup screen labels to `SiteName`, `AdminUsername`, `AdminEmail` and `AdminPassword`.
- `config-3`: three cards on `RecipeName` (the recipe's name, not its display name) and `SiteTimeZone`.
- `config-4`: four cards on `DatabaseProvider`, `DatabaseConnectionString`, `DatabaseTablePrefix` and `DatabaseSchema`.
- `secrets-1`: the settings as environment variables, with two cards (double underscore, the item number).
- `secrets-2`: the `dotnet user-secrets` commands, with two cards (colons, local development).
- `tenants-1`: the Tenants list with a second tenant (the entry highlighted), with three cards (Default first, host, prefix).
- `tenants-2`: `AutoSetupPath` in the section, with three cards (the path of the Default tenant, of a prefixed tenant, and the empty default).
- `check-1`: the Error log line of a configuration that fails validation, and three cards on what is validated.
- `check-2`: the Information log line of a successful installation, the 503 response of a failed one, and its Error log line.
- `lock-1`: `Program.cs` with `OrchardCore.Redis.Lock` registered next to `OrchardCore.AutoSetup`, and the Redis configuration variable.
- `lock-2`: the two lock option variables, with three cards (milliseconds, both or neither, the default).

## Checks for private data

- The only names on screen are `Lakeside Journal`, the user `admin` and `admin@example.com`; there is no other data.
- The password is typed into a masked field and is never shown: OCR the video for the value of `VIDEO_PASSWORD` and for
  the word `password` in a field, at one frame per second, at every scene change and at five frames per second.
- The Setup screen shows the default culture of the machine in `Change language` and, unless chosen off camera, its
  time zone; the plan selects `America/Los_Angeles` before the capture, and the language list shows only language names.
- The console of the site, the folder of the site's data and the user secrets of the machine never appear.
- No `state.json`, no data folder and no audio file is committed.
