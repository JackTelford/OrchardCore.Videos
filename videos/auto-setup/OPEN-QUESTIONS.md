# Open questions: Auto Setup

What the narration states was read from the source of Orchard Core (the Auto Setup module, the Setup module and the
setup service). The points below were not run on a site. A person must confirm each one on a running site before the
clips are recorded, and change the storyboard if the site behaves differently.

## Behavior to confirm on a running site

1. **The Setup screen appears when Auto Setup does not start.** `check-1` says that Auto Setup "logs the errors and does
   not start". The code adds the middleware only when the configuration exists and validates
   (`src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:92` to `:121`); the Setup screen is then served by the Setup
   module (`src/OrchardCore.Modules/OrchardCore.Setup/Startup.cs:89`). The narration does not say that the Setup screen
   appears. Check with a configuration that lacks `AdminPassword`: the log should show `AutoSetup did not start,
   configuration has following errors: The AdminPassword field is required.`, and `/` should show the Setup screen.
2. **`AutoSetupPath`.** `tenants-2` says that only requests to that path start the installation. The code maps the
   middleware with `MapWhen` on the path (`Startup.cs:119`), so other URLs reach the regular pipeline. The match is by
   path segments, so `/autosetup/anything` matches too (`src/OrchardCore/OrchardCore.Abstractions/Routing/PathStringExtensions.cs:7`).
   Check that `/` shows the Setup screen and `/autosetup` installs the site and redirects to `/`.
3. **A second tenant.** `tenants-1` and `tenants-2` come from the code (`AutoSetupMiddleware.cs:133` to `:143`,
   `AutoSetupService.cs:59` to `:80`) and the documentation, and are not recorded. The other tenants are created only
   inside the request that installs the Default tenant. After that request, the Default tenant is no longer
   uninitialized, so `Startup.cs:92` adds no middleware, and an entry added to `Tenants` later is never created. The
   narration says "When Auto Setup installs the Default tenant, it also creates a tenant for every other entry" and
   does not say what happens to an entry added later. Check, with the tenant `Harbor` (`RequestUrlPrefix` `harbor`) of
   the `tenants-1` slide, that `/harbor` installs it on its first request after the Default tenant was installed, that
   with an `AutoSetupPath` the tenant installs on `/harbor/autosetup`, and that an entry added after the Default
   tenant was installed is ignored.
4. **The failure response.** `check-2` says `503 Service Unavailable` with the text `The AutoSetup failed installing the
   site.` (`AutoSetupMiddleware.cs:150` to `:152`) and a log line that lists the errors (`AutoSetupService.cs:54`).
   Check it with a password that the site rejects, or an unreachable `DatabaseConnectionString` with the Postgres
   provider (`SetupService.cs:184` to `:201` turns the failed connection into an error). Not narrated, and to find out:
   whether the next request tries the installation again. A failed installation resets the shell state
   (`SetupService.cs:98` to `:100`), but the database may keep the tables that were created before the failure; the
   next attempt could then answer `The provided database, table prefix and schema are already in use.`
5. **A recipe name that matches no setup recipe.** Not narrated. `AutoSetupService.cs:84` looks the recipe up with
   `SingleOrDefault` on its name and passes `null` on when none matches. From the source: `RecipeExecutor.ExecuteAsync`
   reads `recipeDescriptor.Name` first (`src/OrchardCore/OrchardCore.Recipes.Core/Services/RecipeExecutor.cs:44`), which
   throws for `null`; `SetupService.cs:256` to `:261` catches it and reports `Unexpected error occurred while importing
   the recipe.`, so the response would be the 503 of item 4. Check what a site does with `RecipeName` set to a display
   name such as `Software as a Service`, so that the video can say what it does.
6. **The lock.** `lock-1` and `lock-2` come from the code (`Extensions/DistributedLockExtensions.cs:24` to `:51`,
   `AutoSetupOptions.cs:53`) and the documentation, and no Redis server was used. The documentation shows `LockTimeout`
   and `LockExpiration` as two optional values; the code rejects a `LockOptions` section in which either value is zero
   or missing, so the narration says to set both. The default of 60 seconds applies only to a distributed lock: with the
   local lock, no timeout and no expiration are used (`DistributedLockExtensions.cs:27` to `:31`). Confirm with a Redis
   server (for example in a container) that the Redis Lock feature, registered as a setup feature, is the lock that Auto
   Setup uses.
7. **Redis Lock as a setup feature.** The documentation registers `OrchardCore.Redis.Lock` with `AddSetupFeatures`. The
   Redis feature only replaces the lock when the Redis configuration is valid (`src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs:92` to `:101`).
   The Redis Lock feature depends on `OrchardCore.Redis` (`src/OrchardCore.Modules/OrchardCore.Redis/Manifest.cs:37`);
   confirm that registering only the lock feature as a setup feature also brings in the dependency, as the
   documentation implies.
8. **Several instances with separate data.** `lock-1` says the instances take the lock one at a time, so the site is
   installed once. The check that another instance already installed the site reads the shell settings from the shared
   store (`AutoSetupMiddleware.cs:114` to `:127`). With the settings in a separate data folder per instance, the second
   instance may find no settings, run the setup, and meet `The provided database, table prefix and schema are already in
   use.` (`SetupService.cs:198`). Not narrated; confirm what the documentation's use case does (instances that share
   the database and the data folder, or a shared shell configuration).
9. **The Features page.** `register-2` says the Features page has no Enable button for Auto Setup. The code hides the
   Enable button and the row's checkbox for it (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:151`,
   `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:38`, `Models/FeatureEntry.cs:40`); look at the page of a running site
   to see how Auto Setup is listed. The `Enable` and `Features` POST actions of the same controller do not repeat the
   check (`AdminController.cs:160` to `:255`), so the narration claims only what the page offers, not what the server
   would refuse.
10. **The log level.** `check-2` says that an Information level log entry names the site. The code writes it only when
    Information is enabled (`AutoSetupService.cs:40` to `:43`). `src/OrchardCore.Cms.Web/appsettings.json` sets `Default`
    to `Warning`, and the NLog rule reads that value, so a site in Production does not write it; `appsettings.Development.json`
    sets `Debug`, and the entry goes to the log file, not to the console (`src/OrchardCore.Cms.Web/NLog.config`). The
    validation and failure lines are errors and are written at any level. Confirm both in the log file of the recording.
11. **A 503 that is not a failure.** While an installation runs, the shell state is `Initializing` and other requests
    get `503 Service Unavailable` (`SetupService.cs:131`); a request that waited for the lock and finds the tenant
    installed also gets a 503 with `The requested tenant is not initialized.` (`AutoSetupMiddleware.cs:117` to `:126`).
    The narration says only that a failed installation answers 503. Not narrated.

## Details for the recorder

12. **The password rules.** The recording plan asks for a `VIDEO_PASSWORD` that meets the site's rules. The rules are not
    set by Orchard Core's code (no override of the password options was found), so they are the ones of ASP.NET Core
    Identity; read the list in the popover of the Password field of the Setup screen before choosing the password.
13. **The Setup screen's selects.** The plan selects the value `America/Los_Angeles` in `#SiteTimeZone` (the option values
    are time zone identifiers: `src/OrchardCore/OrchardCore/Modules/Services/DefaultTimeZoneSelectListProvider.cs:18`) and the option
    text `Postgres` in `#DatabaseProvider`. Confirm both exist. The option text of a time zone is built as
    `(GMT{StandardOffset}) {TimeZoneId} ({WallOffset})` (`src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25`), so
    the Los Angeles entry should contain `America/Los_Angeles`; confirm the exact text on screen.
14. **The elements `#recipeButton`, `#SiteTimeZone`, `#DatabaseProvider`, `#UserName`, `#Email`, `#Password`,
    `#PasswordConfirmation`** are the ids that the tag helpers generate from the view model
    (`src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml`, `ViewModels/SetupViewModel.cs`); confirm them
    with `Recorder probe`, on the site without Auto Setup configuration only. A probe on the site configured for
    `05-first-request` installs it. `Recorder probe` also needs a `state.json` file to exist (see the first section of
    `recording-plan.md`).
15. **The admin.** The plan opens `/Admin/Settings/general` directly. The narration says `Settings`, `General`, which are
    the labels of the menu (`src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36`, `:39`). No selector of the admin
    menu was checked. The labels `Site name` and `Default Time Zone` are in `Settings-Site.Edit.cshtml:16` and `:43`;
    confirm that `GetByLabel` finds them, and finds only one element each.
16. **The duration of the installation.** The Blog recipe is installed during the first request. The plan sets a 180-second
    navigation timeout and cuts the wait with `OffCameraAsync`; confirm that the cut leaves no blank frame, and that the
    redirect ends in a network-idle page. The Blog theme loads fonts from a Google address in the browser
    (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12` and `:13`), which the proxy variables of the site do
    not cover; confirm that the page still reaches network idle on the recording machine.

## Pronunciation, for a person to check with `pronounce.py`

The storyboard's `spoken` map has ten entries, none of them confirmed with the voice: `Program.cs` (Program dot C.S.),
`appsettings.json` (app settings dot J.S.O.N.), `RequestUrlHost` (Request U.R.L. Host) and `RequestUrlPrefix` (Request
U.R.L. Prefix). Also listen for words the voice may misread: `Sqlite`, `SqlConnection`, `MySql`, `Postgres`,
`AddSetupFeatures`, `OrchardCore.AutoSetup`, `OrchardCore.Cms.Web`, `AutoSetupPath`, `/autosetup`, `/harbor/autosetup`,
`America/Los_Angeles`, `ShellName`, and the config keys of `config-2` to `config-4`. The entries for `OrchardCore.AutoSetup`, `OrchardCore.Cms.Web`, `America/Los_Angeles`, `/harbor/autosetup`, `/autosetup` and `OrchardCore` write the dots and slashes as they are said (the longer keys come first, because the replacement is by substring in order). Also listen for `SaaS` in `config-3` (it may be read as one word, "sass"; "S.a.a.S." is the alternative). Add an entry to `spoken` for each one
that is misread, after a person confirmed the spelling.
