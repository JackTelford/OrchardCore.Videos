# Recording plan: Test Drive Orchard Core

Read only this page to record the video. `Orchard Core source` below means a checkout of `OrchardCMS/OrchardCore` on
the `main` branch; every `src/...` path is relative to it. A selector written as `#SiteName` stands for
`scene.Page.Locator("#SiteName")`.

Two chapters are recordings; the others are slides that `slides.py` draws, so they need no capture.

| Chapter | Clip | Kind | Starts on | Changes the site |
| --- | --- | --- | --- | --- |
| 00 | `00-intro` | card | | no |
| 01 | `01-ways` | slides | | no |
| 02 | `02-source` | slides | | no |
| 03 | `03-setup` | recording | `/` (the Setup screen) | **yes: installs the site** |
| 04 | `04-explore` | recording | `/` (the home page) | no durable change |
| 05 | `05-host` | slides | | no |
| 06 | `06-data` | slides | | no |
| 99 | `99-outro` | card | | no |

## The site it needs

**This video's subject is the Setup screen and an uninstalled site. Record it without Auto Setup and without
`state.json`.** Do not run `Recorder setup` (it signs in to a site that is already installed) and do not run
`Recorder prepare` (`Scenes.cs` has no `PrepareAsync`: nothing is seeded). The recorder opens the site anonymously
(`Scene.StartAsync` passes no storage state when `state.json` does not exist), and `04-explore` signs in on camera.

1. Run the web application the way the video shows it, from the Orchard Core source:

   ```bash
   cd <Orchard Core source>/src/OrchardCore.Cms.Web
   export ORCHARD_APP_DATA=<an empty folder outside both repositories, absolute path>
   dotnet run
   ```

   The `Kestrel` launch profile (`src/OrchardCore.Cms.Web/Properties/launchSettings.json:18-25`) listens on
   `http://localhost:5000` and `https://localhost:5001`. Record on the http address:
   `export VIDEO_BASE_URL=http://localhost:5000`, so the address in the narration is the address recorded.
   `ORCHARD_APP_DATA` is read by `src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:20-24`.
2. **Make sure Auto Setup cannot run**, or the Setup screen never appears. It runs only when an `AutoSetup`
   configuration section exists (`src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:95-98`). Check that:
   - the `AutoSetup` example in `src/OrchardCore.Cms.Web/appsettings.json` (about lines 249 to 275) is still commented out;
   - `dotnet user-secrets list --project src/OrchardCore.Cms.Web` lists no `OrchardCore` secret (it prints a message
     such as *No secrets configured for this application.*; the project has a user secrets id,
     `OrchardCore.Cms.Web.csproj:15`);
   - no environment variable has a name that contains `OrchardCore` (in Git Bash: `env | grep -i orchardcore`), so no
     `AutoSetup` section and no tenant setting (recipe, database provider) is preset: a preset hides the **Recipe** list or
     the database fields.
3. **Check the first request.** Opening `http://localhost:5000/` in a normal browser must show the screen titled
   **Setup** with the sentence *Please answer a few questions to configure your site.* (`Views/Setup/Index.cshtml:102-103`
   of `src/OrchardCore.Modules/OrchardCore.Setup`). If it shows a site, the folder of `ORCHARD_APP_DATA` was not empty.
4. Outbound calls: the server makes none during setup. The browser loads the web fonts of the Blog theme from a public
   font service (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`); they carry no user data, and the page
   falls back to system fonts when offline.
5. The browser language is `en-US` (the recorder sets it), so the Setup screen is in English even when the machine is not.

### The fake data

Everything is typed on camera, except the time zone, which is selected before the capture starts.

| Field | Value |
| --- | --- |
| What is the name of your site? | `Test Drive` |
| Recipe | `Blog` |
| Default Time Zone | `America/Los_Angeles`, selected **before** the capture starts (the list starts on the time zone of the recording machine, which must not appear) |
| What type of database to use? | `Sqlite` (the default, left as it is) |
| User Name | `admin` |
| Email | `admin@example.com` |
| Password and Password Confirmation | `<password>`: the value of `VIDEO_PASSWORD`, chosen outside git; it must satisfy every rule that the *Password requirements:* box lists (see `setup-4`) |

`export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>` before recording, with the password chosen outside git; `Scenes.cs`
reads both. Never click the eye buttons `#togglePassword` and `#togglePasswordConfirmation` on the
Setup screen, nor `#togglePassword` on the login page: the password must stay as dots in the capture.

Narrate before recording (`scripts/narrate.py`): the recorder holds each step for the length of its narration.

## Order and state

1. Start the site on the empty `ORCHARD_APP_DATA` folder. This empty folder is the snapshot for `03-setup`.
2. Record `03-setup`. It installs the site.
3. Stop the site with Ctrl+C in the terminal that runs `dotnet run`, and check that no `OrchardCore.Cms.Web` process is
   left (by its process id, never by image name): a process left behind keeps `OrchardCore.db` open. Then copy the
   `ORCHARD_APP_DATA` folder aside as `after-setup`, outside git (`.gitignore` also ignores `App_Data*/`). Start the site again.
4. Record `04-explore`. It depends on `03-setup`: it needs the installed site and the account `admin`.

To record `03-setup` again, stop the site as in step 3, empty the `ORCHARD_APP_DATA` folder, and start the site. To record
`04-explore` again, stop the site as in step 3, restore `after-setup`, and start the site.

## Clip `03-setup`: the Setup screen

Starts on `/`. Before `StartCaptureAsync`: `OpenAsync("/")`; then
`SelectOptionAsync("America/Los_Angeles")` on `#SiteTimeZone`; check that **What is the name of your site?** is empty,
the **Recipe** button reads *Software as a Service*, and the page is in English. Then `StartCaptureAsync()`.

Every label below is read from `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml` unless another
file is named.

### `setup-1`

Actions: `HighlightAsync` on the heading block, `scene.Page.Locator("div.bg-light")` (no caption), and nothing else.
Visible when the narration ends: the heading **Setup** and the sentence *Please answer a few questions to configure your
site.* with a green ring; the form below it is empty. Label source: lines 102 and 103. The ring also surrounds the
**Change language** list at the right of the block (lines 80 to 100, label at 88): it shows because the Setup supports more
than one UI language (`src/OrchardCore.Modules/OrchardCore.Setup/Startup.cs:17-84`), and its entries can include the
installed UI language of the recording machine (`Startup.cs:17`, `:77`). Leave it alone; the page is in English because the
browser asks for `en-US`.

### `setup-2`

Actions, in this order:

1. `ClearHighlightsAsync()`, then `TypeAsync(#SiteName, "Test Drive")`, then `PauseAsync(5000)`: the narration says what a
   recipe is while the list is closed.
2. `ClickAsync(#recipeButton)`: the list opens with six entries, in this order: *Agency*, *Blank site*, *Blog*,
   *Coming Soon*, *Headless site*, *Software as a Service*. Wait until `#recipes .dropdown-menu` is visible, then
   `PauseAsync(2500)`.
3. `ClickAsync(#recipes .dropdown-item[data-recipe-name='Blog'])`. Wait until `#recipeButton` reads *Blog*.

Visible when the narration ends: **What is the name of your site?** holds *Test Drive*, and the **Recipe** button reads
*Blog* with its list closed. Label sources: lines 118 (site name), 130 and 142 (Recipe, `recipeButton`), 148 (list
entries, with the recipe names and descriptions of `src/OrchardCore.Themes/*/Recipes/*.recipe.json`: `agency` 3, `blank` 3,
`blog` 3, `comingsoon` 3, `headless` 3, `saas` 3). The narration's definition of a recipe paraphrases line 155 (*Recipes
allow you to setup your site with additional pre-configured options, features and settings out of the box.*), and its
description of Blog is `blog.recipe.json:4` (*Provides a functional Blog with CMS features.*). The list starts on
*Software as a Service* because it is the only setup recipe tagged `default` (`SetupController.cs:61`,
`saas.recipe.json:10`).

### `setup-3`

Actions, in this order:

1. `ClearHighlightsAsync()`, then `HighlightAsync(#SiteTimeZone)` (no caption), then `PauseAsync(4500)`: the first sentence
   of the narration is about this list.
2. `ClearHighlightsAsync()`, then `HighlightAsync(#DatabaseProvider)`: it shows *Sqlite*; **Connection String**, **Table
   Prefix** and **Table Schema** are hidden by the page script (`Assets/ts/setup.ts`, `toggleConnectionStringAndPrefix`).

Visible when the narration ends: the **What type of database to use?** list, ringed and showing *Sqlite*, with no
connection string, table prefix or table schema field below it; the **Default Time Zone** list above it shows
*America/Los_Angeles*. The narration says that the list starts as the time zone of the computer, which describes the
default; the plan changed it before the capture so that the time zone of the machine does not appear. Label sources: lines
159 (**Default Time Zone**) and 176 (database question). The time zone list starts on the system time zone
(`SetupController.cs:73`, `SiteTimeZone = _clock.GetSystemTimeZone().TimeZoneId`). *Sqlite* is the default provider
(`src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53`, `isDefault: true`, no connection string).

### `setup-4`

Actions, in this order:

1. `ClearHighlightsAsync()`, then `ScrollAsync(0, 250)`, always: the form is taller than the 864-pixel viewport. After the
   scroll, the **Super User** legend and the **Finish Setup** button must both be in view; if the button is cut off on the
   contact sheet, use `ScrollAsync(0, 330)` for the next take.
2. `TypeAsync(#UserName, "admin")`, `TypeAsync(#Email, "admin@example.com")`,
   `TypeAsync(#Password, <VIDEO_PASSWORD>)`, `TypeAsync(#PasswordConfirmation, <VIDEO_PASSWORD>)`.
3. `ClickAsync(#Password)`, then `PauseAsync(2500)` and nothing else. The page script builds a box above the **Password**
   field each time the field gets the focus, and removes it when the field loses the focus (`Assets/ts/setup.ts:262-272`),
   so the box shows while the password is typed, closes when **Password Confirmation** is clicked, and returns here, with
   every rule ticked, while the narration says *the Password requirements that the screen lists*. The box sits directly
   above the field and can cover **User Name** and **Email**: that is expected.

Visible when the narration ends: the box titled *Password requirements:* above the **Password** field, listing
*Minimum length: N* and the other rules of the site's password options, each one with a check mark; the **Password** and
**Password Confirmation** fields shown as dots; the **Finish Setup** button; no validation message in red. Typed earlier in
the step, and possibly hidden by the box: **User Name** *admin*, **Email** *admin@example.com*, under **Super User** and its
sentence *The super user has all the rights. It should be used only during Setup and for disaster recovery.* (the narration
of `setup-4` says "the account that has all the rights", and the narration of `explore-2` refers to the rest of the
sentence). Label sources: lines 218 and 219 (**Super User** and its sentence), 222 (**User Name**), 227 (**Email**), 234
(**Password**), 244 (**Password Confirmation**); the box is not in the view: its title *Password requirements:* is the text
at `Assets/ts/setup.ts:207` and its lines are at `:180-199` (the compiled `wwwroot/Scripts/setup/setup.min.js` has the same
texts). `Views/Setup/Index.cshtml:236` also holds a *Password strength* title (`data-title`), but nothing in the page script
creates a Bootstrap popover from it, so it is not shown.

### `setup-5`

Actions: `HighlightAsync(#SubmitButton)`, `PauseAsync(1000)`, `ClearHighlightsAsync()`, then click **Finish Setup** and cut
the wait, because the recipe runs inside the request and takes from several seconds to a minute:

```csharp
scene.Page.SetDefaultNavigationTimeout(180000);   // the recipe can take longer than the 20 seconds of the scene's default timeout
await scene.MoveToAsync(scene.Page.Locator("#SubmitButton"));
await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#SubmitButton")));
scene.Page.SetDefaultNavigationTimeout(20000);
```

The condition of the wait is the navigation to `/` (`SetupController.cs:209`, `Redirect("~/")`), which completes only
when the recipe has finished, so nothing waits on camera. If the page shows a red message instead of the home page,
discard the take (see the checks at the end).

Visible when the narration ends: the home page of the Blog recipe: the navigation bar with the brand *Test Drive*, the
menu entries *Home* and *About*, and the large heading *Blog*. Label source: line 256 (**Finish Setup**, `SubmitButton`);
`TheBlogTheme/Views/Layout.liquid:27` (brand), `blog.recipe.json:834` and `:843` (*Home*, *About*), `:976` (the
description of the blog).

## Clip `04-explore`: the site and the admin

Starts on `/`, anonymous (no `state.json`). Before `StartCaptureAsync`: `OpenAsync("/")`. Check that the home page of the
Blog recipe shows, and that no cookie of an earlier take is kept (a new browser context is used). Then
`StartCaptureAsync()`.

### `explore-1`

Actions, in this order: `HighlightAsync(a.navbar-brand)` (no caption); `PauseAsync(2000)`; `ClearHighlightsAsync()`;
`HighlightAsync(header.masthead h1)`; `PauseAsync(2000)`; `ClearHighlightsAsync()`; then `ScrollAsync(0, 380)` so
that the post below the heading is in view.

Visible when the narration ends: the page scrolled so that the bottom of the heading block and the post preview are in
view: the title *Man must explore, and this is exploration at its greatest*, its subtitle *Problems look mighty small
from 150 miles up*, and the line *Posted by admin on* followed by the date. Label sources:
`TheBlogTheme/Views/Content-Blog.liquid:14-16` (heading and subheading), `Content-BlogPost.Summary.liquid:3-6` (title and
subtitle), `blog.recipe.json:1002` and `:1022`.

### `explore-2`

Actions, in this order: `NavigateAsync("/Admin")`: the site redirects to the login page (`/Login?ReturnUrl=...`) because
nobody is signed in (`src/OrchardCore.Modules/OrchardCore.Admin/Controllers/AdminController.cs`, `[Authorize]`; the login
path is `Login`, `src/OrchardCore/OrchardCore.Users.Abstractions/UserOptions.cs:5`); wait until `#LoginForm_UserName` is visible;
`TypeAsync(#LoginForm_UserName, "admin")`; `TypeAsync(#LoginForm_Password, <VIDEO_PASSWORD>)`. Do not submit yet.

Visible when the narration ends: the page titled **Log in** with **Username or email address** holding *admin* and
**Password** shown as dots, and the **Log in** button below. Label sources:
`src/OrchardCore.Modules/OrchardCore.Users/Views/Account/Login.cshtml:27` (heading), `Views/LoginFormCredentials.cshtml:7`
(**Username or email address**) and `:13` (**Password**), `Views/LoginForm.Edit.cshtml:9` (**Log in**); the admin address
is the default `AdminUrlPrefix`, `Admin` (`OrchardCore.Admin.Abstractions/AdminOptions.cs:5`).

### `explore-3`

Actions, in this order: click the **Log in** button with `ClickAndWaitForNavigationAsync(form.auth-form button[type=submit])`;
the browser returns to `/Admin`. Wait until the heading *Welcome to Orchard Core* (`div.alert.alert-primary h4`) is
visible. Then, one after the other, `HighlightAsync` on the four cards, each located as `.card` filtered by its title
(*Create content*, *Customize theme*, *Manage users*, *Learn more*), with `PauseAsync(1500)` after each one and
`ClearHighlightsAsync()` between them (not after the last).

Visible when the narration ends: the admin welcome page with the alert *Welcome to Orchard Core* (and *Your site is
ready. Get started with the actions below, or browse the menu to discover all its possibilities.*) and the four cards;
the last one, **Learn more**, is ringed. Do not click **Documentation**: it opens another site in a new tab. Label
sources: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml:2-3`;
`OrchardCore.Contents/Views/AdminDashboardContents.cshtml:4` (**Create content**, button *Go to content*);
`OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4` (**Customize theme**, button *Manage themes*);
`OrchardCore.Users/Views/AdminDashboardUsers.cshtml:4` (**Manage users**, button *Go to users*);
`OrchardCore.Admin/Views/AdminDashboardDocumentation.cshtml:4` (**Learn more**, button *Documentation*).

## The slides

They are drawn by `slides.py`; nothing is recorded. Each step shows:

- `ways-1`: the four ways as cards, the first one bright: Try Orchard Core, with its address `try.orchardcore.net`.
- `ways-2`: the same cards, DotNest bright.
- `ways-3`: the Docker command in a panel (`docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest`,
  from `src/docs/getting-started/test-drive-orchard-core.md`), with two cards explaining `--name` and `-p`.
- `ways-4`: the four cards, The source code bright.
- `source-1`: the commands of the clone, lines 1 to 3, bright, with the cards Install (.NET SDK) and Clone
  (OrchardCMS/OrchardCore).
- `source-2`: the same panel, `cd src/OrchardCore.Cms.Web` and `dotnet run` bright, with the cards Folder and Command.
- `source-3`: the same panel, with the cards Browser (`http://localhost:5000`) and Visual Studio (Startup project, then
  Ctrl+F5).
- `host-1`: `Program.cs` of `src/OrchardCore.Cms.Web`, trimmed, with the `AddOrchardCms` and `UseOrchardCore` lines bright and
  a card for each.
- `host-2`: the same panel, with the `AddSetupFeatures` line bright and two cards: `AddSetupFeatures` (makes Auto Setup
  available) and Auto Setup (installs a site from settings).
- `host-3`: four cards for the folders `OrchardCore.Cms.Web`, `OrchardCore.Modules`, `OrchardCore.Themes` and `docs`.
- `data-1`: the tree `App_Data/tenants.json`, `App_Data/Sites/Default/OrchardCore.db` in a panel, with the cards Default
  and Sqlite (the database provider that the Setup screen selects by default). The slide shows the default location
  (`App_Data` in `OrchardCore.Cms.Web`); with `ORCHARD_APP_DATA` set, the folder it names takes the place of `App_Data`.
  **Before publishing, look at the `ORCHARD_APP_DATA` folder after `03-setup` and confirm that both files exist at these
  paths**; if not, change the slide.
- `data-2`: two cards: to start over (stop the site, delete App_Data, run it again) and to use another folder (the
  `ORCHARD_APP_DATA` environment variable).

## Checks for private data

Do these before the video is assembled, and again on the result (the skill's OCR pass at one frame per second and at
every scene change):

- The only names that may appear are `Test Drive`, `admin`, `admin@example.com` and the sample content of the Blog
  recipe. Search the OCR text for the Windows or macOS user name of the recording machine, its host name, any folder
  path (`C:\`, `/Users/`, `/home/`), and any email address other than `@example.com`.
- The password never appears: the fields show dots, no eye button was clicked, and the OCR text does not contain the value
  of `VIDEO_PASSWORD`.
- The time zone shown on the Setup screen is `America/Los_Angeles`, not the time zone of the machine.
- Discard a take that shows an error page: the site runs in the Development environment (the launch profile sets it), so an
  error page lists file paths of the machine.
- The slides show only `http://localhost:5000`, `try.orchardcore.net` and the name of the public repository
  `OrchardCMS/OrchardCore`; the `git clone` address is not drawn.
- Delete the `ORCHARD_APP_DATA` folder and its copy `after-setup` when the recording is done: they hold the account
  and the password hash. Never commit them, nor a `state.json`.
