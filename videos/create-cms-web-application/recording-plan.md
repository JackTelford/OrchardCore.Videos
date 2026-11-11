# Recording plan: Create a CMS Web Application With dotnet new

Read this page and nothing else to record the video. Source references (`file:line`) are paths in `OrchardCMS/OrchardCore`
at commit `92db4a83`, under `src/` unless a path says otherwise.

**What is recorded.** The recorder captures a browser, not a terminal. The commands (`dotnet new install`, `dotnet new occms`,
`dotnet run`), the files the template writes and the console output are all slides drawn by `slides.py`; nobody records them.
Only two clips are recordings: `05-setup` (the Setup screen, then the blog it creates) and `06-admin` (the first sign-in).

| Chapter | Clip | Kind | Steps |
| --- | --- | --- | --- |
| 00 | `00-intro` | card | `intro` |
| 01 | `01-ways` | slide | `ways-1`, `ways-2` |
| 02 | `02-install` | slide | `install-1`, `install-2`, `install-3` |
| 03 | `03-create` | slide | `create-1` to `create-6` |
| 04 | `04-run` | slide | `run-1`, `run-2` |
| 05 | `05-setup` | **recording** | `setup-1` to `setup-7` |
| 06 | `06-admin` | **recording** | `admin-1` to `admin-3` |
| 07 | `07-logging` | slide | `log-1`, `log-2`, `log-3`, `data-1`, `data-2` |
| 08 | `08-next` | slide | `next-1`, `next-2` |
| 99 | `99-outro` | card | `outro` |

## The site it needs

The video is about the project that `dotnet new occms` writes, so that project is the site. It is not the `OrchardCore.Cms.Web`
project of the Orchard Core repository, and it is not provisioned by Auto Setup: the Setup screen is what the video shows.

Work in a folder outside both repositories, called `<work>` below, with two empty folders in it: `site` and `app-data`.

1. Install the templates: `dotnet new install OrchardCore.ProjectTemplates`. If the label check below fails with this release,
   install the release or preview package whose screens match, with `@<version>` after the package ID.
2. In `<work>/site`, create the project: `dotnet new occms -n MySite`. Do not pass `--logger`: the default is NLog, which the
   slides show.
3. Restore and build while the network is open: `dotnet restore MySite/MySite.csproj`, then `dotnet build MySite/MySite.csproj`.
4. Start the site from `<work>/site` with these environment variables set for that process only, and note its process ID
   (to stop it later by ID, never by image name):

   | Variable | Value | Why |
   | --- | --- | --- |
   | `ASPNETCORE_URLS` | `http://localhost:5010` | One http address. The recorder's browser does not accept an untrusted development certificate, and with no https address the `app.UseHttpsRedirection()` line of `Program.cs` has nothing to redirect to. |
   | `ASPNETCORE_ENVIRONMENT` | `Development` | The value the launch settings give (`Properties/launchSettings.json`). |
   | `ORCHARD_APP_DATA` | `<work>/app-data` | A fresh data folder outside both repositories (`OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs:5`). |
   | `HTTP_PROXY`, `HTTPS_PROXY` | `http://127.0.0.1:9` | A dead address, so the site never calls out. Set after step 3. |

   Command: `dotnet run --project MySite/MySite.csproj --no-build --no-launch-profile`.
   Do not set any `OrchardCore__AutoSetup__...` variable (nor the deprecated `OrchardCore__OrchardCore_AutoSetup__...`, `OrchardCore.AutoSetup/Startup.cs:29`): without that configuration the site shows the Setup screen.
5. Wait until `http://localhost:5010/` shows the Setup screen (its heading is `Setup`).

The default tenant is the only tenant. Time zone, recipe, site name and the super user are all entered on camera, from the
table below. No feature is enabled and no setting is saved before recording: the Blog recipe enables what the front end
needs. There is no external service and there is no seed data.

### The values typed on the Setup screen

| Field (label) | Value |
| --- | --- |
| What is the name of your site? | `Orchard Core` |
| Recipe | `Blog` |
| Default Time Zone | `America/Los_Angeles` (selected outside the capture, see `05-setup`) |
| What type of database to use? | `Sqlite` (already selected) |
| User Name | `admin` |
| Email | `admin@example.com` |
| Password and Password Confirmation | the value of `VIDEO_PASSWORD`: a password chosen outside git, never written in any file of this repository. It must satisfy the rules the popover of the Password field lists while the field has focus (`Password requirements:`, then one line per rule, such as `Minimum length: 6`: the strings are in `OrchardCore.Setup/Assets/ts/setup.ts:180-207`; the rules come from the identity options of the site, `Index.cshtml:32-73`) |

### Label check, before recording

Open `http://localhost:5010/` in a browser and compare the screen with this list (read from the source at `92db4a83`). If one
label differs, do not record: write what the screen shows in `OPEN-QUESTIONS.md` and ask for the narration to be corrected.

- Setup screen: `Setup`, `Please answer a few questions to configure your site.`, `What is the name of your site?`, `Recipe`,
  `Default Time Zone`, `What type of database to use?`, `Super User`, `User Name`, `Email`, `Password`, `Password Confirmation`,
  `Finish Setup`. The popover that opens when the Password field gets focus has the heading `Password requirements:`
  (`setup.ts:207`). The Bootstrap popover titled `Password strength` (`Index.cshtml:236`) is never started by `setup.ts`, so it
  does not open; do not wait for it. Both password fields are masked: `SetupViewModel.cs` marks them `[DataType(DataType.Password)]`.
- The Recipe button first reads `Software as a Service`: it is the only recipe tagged `default`
  (`OrchardCore.Setup/Controllers/SetupController.cs:61`, `OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:10`).
  Its menu lists, in this order, `Agency`, `Blank site`, `Blog`, `Coming Soon`, `Headless site`, `Software as a Service`
  (sorted by recipe name, `Index.cshtml:146`; the display names are in each `*.recipe.json`).
- Database: `Sqlite` is selected. Its Table Prefix, Connection String and Table Schema fields are hidden
  (`OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53`, `OrchardCore.Setup/Assets/ts/setup.ts:4`).
- Sign-in page: `Log in`, `Username or email address`, `Password`. Admin: `Welcome to Orchard Core`.

If the packages of the installed release do not carry these labels, run `src/OrchardCore.Cms.Web` of an Orchard Core checkout
at `92db4a83` instead, with the same environment variables (it has `AddSetupFeatures("OrchardCore.AutoSetup")`, but Auto Setup
does nothing without configuration: `OrchardCore.AutoSetup/Startup.cs:95`). Its Setup screen is the same. Say so in
`OPEN-QUESTIONS.md`.

## Order, snapshots and state

- Narrate first (`narrate.py`), then record: the recorder holds each step for the length of its narration.
- Record `05-setup`, then `06-admin`. `06-admin` needs the site that `05-setup` creates.
- Build the recorder with this video's scenes. `videos/create-cms-web-application/recorder/Scenes.cs` is not part of the
  committed content: the person who records writes it from this plan, with two clip methods named `05-setup` and `06-admin`
  (one `scene.StepAsync("<step id>", ...)` per step, as `.agents/skills/orchardcore-video-producer/SKILL.md` describes). The
  video has no `PrepareAsync`, so do not run `Recorder prepare`.
- **Do not run `Recorder setup` or `Recorder probe`.** Both clips start signed out, in a new browser context. The `record`
  command does not need `state.json`: `Scene.StartAsync` uses it only when the file exists
  (`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:109`). A `state.json` left over from another video must
  be deleted, or `06-admin` would start signed in.
- Environment of the recorder: `VIDEO_ROOT` (this video's folder), `VIDEO_BASE_URL=http://localhost:5010`, `VIDEO_PASSWORD`.
- **Snapshot S0**, the state `05-setup` needs: `<work>/app-data` is empty and the site is running. `05-setup` changes the site (it
  creates it). To record it again: stop the site by its process ID, empty `<work>/app-data`, start the site again.
- **Snapshot S1**, the state `06-admin` needs: after `05-setup`, stop the site and copy `<work>/app-data` aside. `06-admin`
  does not change the site. To record it again: stop the site, restore the copy of S1 into `<work>/app-data`, start the site.

## Clip `05-setup`: the Setup screen

Starts on `/`, the Setup screen, from S0. Before `StartCaptureAsync`, outside the capture:

1. `scene.OpenAsync("/")`, and wait until `#SiteName` is visible.
2. Select the time zone with `scene.Page.SelectOptionAsync("#SiteTimeZone", "America/Los_Angeles")`, so the time zone of the
   machine never appears on camera.
3. Press nothing else: the name field, the recipe and the super user stay as the page loaded them.

Every `#id` below is the id that the field's `asp-for` produces (`OrchardCore.Setup/Views/Setup/Index.cshtml`).

- `setup-1`: no action. Move the cursor to the `Setup` heading (`Index.cshtml:102`). On screen at the end: the Setup screen
  with the heading `Setup`, the line `Please answer a few questions to configure your site.`, the empty
  `What is the name of your site?` field, the Recipe button reading `Software as a Service`, `Default Time Zone` and
  `What type of database to use?`.
- `setup-2`: `TypeAsync` into `#SiteName` the text `Orchard Core` (delay 60). On screen at the end: the field holds
  `Orchard Core`, with the hint `This is used as the default title of your pages.` under it (`Index.cshtml:118-121`).
- `setup-3`: click `#recipeButton` (it reads `Software as a Service`), pause 900 ms so the menu is seen, then click the menu entry
  `#recipes a.dropdown-item[data-recipe-name='Blog']` (its text starts with `Blog`; `Index.cshtml:141-148`). On screen at the
  end: the menu is closed and the button reads `Blog` (`OrchardCore.Setup/Assets/ts/setup.ts:59`), with the sentence
  `Recipes allow you to setup your site with additional pre-configured options, features and settings out of the box.` under it
  (`Index.cshtml:155`).
- `setup-4`: `HighlightAsync(#SiteTimeZone)`, pause 1500 ms, `ClearHighlightsAsync()`. Do not change the value. On screen at the
  end: `Default Time Zone` shows `America/Los_Angeles` (the option text is `(GMT<standard offset>) America/Los_Angeles (<offset now>)`,
  built in `OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25`; NodaTime formats both offsets and the second one depends on
  the date of the recording, so do not rely on their exact form, only on the zone name), with the hint
  `Determines the default time zone used when displaying and editing dates and times.` under it (`Index.cshtml:162`), and no
  highlight is open.
- `setup-5`: `HighlightAsync(#DatabaseProvider)`, pause 1500 ms, `ClearHighlightsAsync()`. Do not change the value. On screen at
  the end: `What type of database to use?` shows `Sqlite`, with the hint
  `The database is used to store the site's configuration and its contents.` under it (`Index.cshtml:184`), and with no Table
  Prefix, Connection String or Table Schema field under it; no highlight is open.
- `setup-6`: if the `Super User` section and the `Finish Setup` button are not both visible, scroll down until they are
  (`ScrollAsync`, until `#SubmitButton` is fully inside the viewport; no highlight is open). Then `TypeAsync`: `#UserName` `admin`; `#Email` `admin@example.com`; `#Password` the
  value of `VIDEO_PASSWORD`; `#PasswordConfirmation` the same value. Do not click the eye buttons (`#togglePassword`,
  `#togglePasswordConfirmation`): they show the typed value. The popover of the Password field exists only while that field
  has focus (`setup.ts:262-272`: it is created on focus and removed on blur), so typing the confirmation closes it. As the last
  action, `ClickAsync(#Password)` to give the field focus again: the popover opens above it, with the heading
  `Password requirements:` and one line per rule (`setup.ts:172-233`), each ticked once the typed value satisfies it
  (`setup.ts:235-251`), and it stays open while the narration of the sentence about the rules plays. Keep the typing under the
  narration (about 9 seconds). On screen at the end: the `Super User` section with its sentence (`Index.cshtml:218-219`), the
  four fields filled (the two password fields show dots), the open popover (it is drawn above the Password field and must not
  hide the `Finish Setup` button), and the button `Finish Setup`. The popover never shows the typed value. A strength indicator
  also belongs to the Password field (`strenght(passwordElement, options)`, `setup.ts:140`); it comes from the
  `@orchardcore/bloom` package, which is not in the source, so what it draws is listed in `OPEN-QUESTIONS.md`.
- `setup-7`: `MoveToAsync(#SubmitButton)` (`Finish Setup`, `Index.cshtml:256`) on camera, and pause 600 ms, so the cursor rests on the
  button while the narration says `Select Finish Setup`. Then, inside `scene.OffCameraAsync(...)`, click the button and wait
  with `WaitForNavigationAsync`, so the seconds of setup are cut out of the clip. The click submits the form, the setup runs,
  and the browser is redirected to `/` (`OrchardCore.Setup/Controllers/SetupController.cs:209`, `Redirect("~/")`). Make the wait
  end only on the new state: `.navbar-brand` is visible and its text is `Orchard Core` (the theme prints the site name there:
  `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`). Allow up to 120 seconds: the scene's default is 20 seconds
  (`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:123`, `SetDefaultTimeout(20_000)`) and
  `WaitForNavigationAsync` takes no timeout argument, so, before the click, call `scene.Page.SetDefaultNavigationTimeout(120_000)`,
  and wait for `.navbar-brand` with `WaitForAsync(new() { Timeout = 120_000 })`, then check its text. On screen at the end: the home
  page of the Blog recipe, with the site name `Orchard Core` in the navigation bar and the blog's post titled
  `Man must explore, and this is exploration at its greatest` (`TheBlogTheme/Recipes/blog.recipe.json:1002`). If the post title
  is below the first screen, scroll down once, slowly, before the narration ends.

## Clip `06-admin`: the first sign-in

Starts from S1, signed out. Before `StartCaptureAsync`: `scene.OpenAsync("/")` (the blog's home page). The scene uses no
`state.json`.

- `admin-1`: `NavigateAsync("/admin")`. The admin requires a user (`OrchardCore.Admin/Controllers/AdminController.cs:6`, `[Authorize]`),
  so the server answers with the sign-in page. On screen at the end: the sign-in page with the heading `Log in`
  (`OrchardCore.Users/Views/Account/Login.cshtml:27`) and the fields `Username or email address` and `Password`
  (`OrchardCore.Users/Views/LoginFormCredentials.cshtml:7,13`).
- `admin-2`: `TypeAsync` into `#LoginForm_UserName` the text `admin`, and into `#LoginForm_Password` the value of `VIDEO_PASSWORD`
  (both ids are the ones the recorder's own `setup` command uses). Then `ClickAndWaitForNavigationAsync` on the button named
  `Log in` (`OrchardCore.Users/Views/LoginForm.Edit.cshtml:9`), then wait until the page contains the text
  `Welcome to Orchard Core` (`WaitForAsync` on the locator `text=Welcome to Orchard Core`, up to 20 seconds). The sign-in sends
  the browser back to the page it came from, `/admin` (the `ReturnUrl` of the sign-in page). On screen at the end: the admin page
  with the notice `Welcome to Orchard Core`; the sign-in page is gone.
- `admin-3`: no action; keep the cursor still. On screen at the end: the admin, with the notice `Welcome to Orchard Core` and the
  sentence under it that begins `Your site is ready.` (`OrchardCore.Admin/Views/AdminDashboard.cshtml:2-3`), and the admin menu.

## Slides

These steps are drawn by `slides.py`; nothing is recorded. One line each.

- `ways-1`: four numbered cards, the steps of building a site by hand (project, CMS package, registration, default pages).
- `ways-2`: four check cards, what the template gives (one command, logging, launch settings, matching versions).
- `install-1`: the `dotnet new install OrchardCore.ProjectTemplates` command, and two cards (what you need, what you install).
- `install-2`: the same command with `@<version>`, and with `@<version>-*` and a preview NuGet source (the form the documentation shows), and two cards that explain them.
- `install-3`: four cards, the templates of the package with their short names (`occms`, `ocmvc`, `ocmodulecms`, `ocmodulemvc`, `octheme`).
- `create-1`: the `dotnet new occms -n MySite` command and two cards (the name option, the short name).
- `create-2`: the file tree of `MySite`, and a card for each of `MySite.csproj`, `NLog.config` and `launchSettings.json`.
- `create-3`: the project file with its two package references, and a card about their versions.
- `create-4`: three cards, the CMS targets package, the CMS Core targets package, and when to use the Core package.
- `create-5`: `Program.cs` with the three Orchard Core lines bright (`UseNLogHost`, `AddOrchardCms`, `UseOrchardCore`).
- `create-6`: three cards (`--logger`, `--orchard-version`, `--framework`) and the `--help` and `--logger none` commands.
- `run-1`: the `dotnet run` command and three cards (run it from the folder that holds `MySite`, the packages come from NuGet, the launch profile is named `MySite`).
- `run-2`: the `MySite` launch profile of `launchSettings.json` (the file also holds `iisSettings` and an `IIS Express` profile, shown as `// ...`), and the console lines with the two listening addresses.
- `log-1`: the targets and rules of `NLog.config`.
- `log-2`: the `Logging` section of `appsettings.json`, and a card for each of its three levels.
- `log-3`: three cards, what each `--logger` value adds (NLog, Serilog, None).
- `data-1`: the `App_Data` tree (`Sites/Default/yessql.db`, `Sites/Default/Media`, `logs`) and three cards (Sqlite file, Media, `ORCHARD_APP_DATA`).
- `data-2`: two cards, how to stop the server (Ctrl+C) and that running it again starts the same site.
- `next-1`: the `dotnet new` commands of a module and a theme, and two cards (next to the web application, reference them).
- `next-2`: the `AddSetupFeatures("OrchardCore.AutoSetup")` lines of `Program.cs`, and two cards (first request, configuration).

## Checks for private data

The site is a fresh site with fake values only (`admin`, `admin@example.com`, a password chosen outside git), so it needs no mask
script. After assembling, OCR the video at one frame per second and at every scene change, then at 5 frames per second, and
search the text for:

- the user name of the machine, folder paths of the machine (`C:\Users`, `/home`, `/Users`), and the name of the machine;
- the machine's own time zone (the capture must show `America/Los_Angeles` and no other zone);
- any email address other than `admin@example.com`, and any phone number or token;
- the password: the Password fields must show dots (both are masked by `[DataType(DataType.Password)]` in `SetupViewModel.cs`, and the sign-in field by `LoginViewModel.cs`), the eye buttons must stay unclicked, and the password-requirements popover must not show the typed value.

Look at every contact sheet (`review.py`) before assembling. The Setup screen of `setup-1` and the sign-in page must show no
error message, and the blog's home page must not be empty.
