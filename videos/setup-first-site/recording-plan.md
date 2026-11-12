# Recording plan: Setup: Install Your First Orchard Core Site

A person who reads only this file can record the video. Source paths are relative to a checkout of
`OrchardCMS/OrchardCore` at commit `92db4a837ddad07f4b35179df59ac78375017860`, the commit the narration was checked
against. Record with a build of that commit (or of a later one, and then check every label below on screen). A source
path that starts with `Views/`, `Controllers/`, `Assets/`, `Startup.cs` or `wwwroot/` and is not preceded by a folder is
in `src/OrchardCore.Modules/OrchardCore.Setup/`.

This video is the exception to the programme's usual site: **the site must not be provisioned by Auto Setup**, because
the Setup screen is what the video shows. The site has to be uninstalled when the first clip starts.

## The site it needs

| What | Value |
|---|---|
| Application | `src/OrchardCore.Cms.Web` of the checkout, built and run on `http://localhost:5010` (`VIDEO_BASE_URL`). It references the starter themes, so the Setup screen lists the setup recipes of Blog, Agency, Coming Soon, Blank and Headless, and Software as a Service. |
| Data folder | `ORCHARD_APP_DATA` pointing at an **empty folder** outside both repositories. Nothing else is configured: no tenant but Default. |
| Auto Setup | **Off.** Set no `OrchardCore__AutoSetup__*` variable and no `AutoSetup` section in a configuration file of the host. Auto Setup would install the site on the first request and the Setup screen would never appear. |
| Presets | **None.** Set no `DatabaseProvider`, `ConnectionString`, `TablePrefix` or `RecipeName` for the Default tenant, in a variable or a file. A preset removes a question from the screen (`Views/Setup/Index.cshtml:124`, `:165`, `:173`), and the video shows all of them. |
| Time zone of the machine | **UTC.** The Setup screen preselects the time zone of the server in the Default Time Zone list (`Controllers/SetupController.cs:73`), and the clips show the list before `zone-1` changes it. Set the time zone of the machine that runs the site to UTC, so the screen does not show where the recorder lives. On Windows, note the current zone with `tzutil /g`, run `tzutil /s "UTC"` in an administrator shell, start the site, and restore the noted zone with `tzutil /s "<noted zone>"` after the last take (.NET takes the zone from the operating system there). On Linux and macOS, start the site with `TZ=UTC`, or in a container. **UTC is not an entry of the list**: the list holds the time zones of places, built from the zone locations of the time zone database (`src/OrchardCore/OrchardCore/Modules/Services/Clock.cs:22-33` and `:47-56`, `DefaultTimeZoneSelectListProvider.cs:17-20`), and a server on UTC gets the id `Etc/UTC`. No entry matches, so the browser shows the first entry of the list, `(GMT+00) Africa/Abidjan (+00:00)`. That is the wanted result: it reveals nothing about the recorder, and the narration does not say which zone the list starts on. Never set the machine to the Pacific time zone (`America/Los_Angeles`): the list would then start on the zone the clip selects, and `zone-1` would show no change. |
| Language | The recorder sends `en-US`, and the labels below are the English ones. When the screen opens in another language, choose English in Change language, then record. |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address for the site process, as `references/recording.md` says. The Blog theme links Google Fonts from the browser (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`); the page works with or without them. |
| Recorder | `VIDEO_ROOT` is `videos/setup-first-site`, `VIDEO_USER_NAME` is `admin`, `VIDEO_PASSWORD` is a password chosen outside git. It must satisfy the rules the Setup screen lists when the Password field has focus (the green bar and every rule checked off), and it is typed into the Setup screen and the Log in page. Never write it in a file of the repository. |
| `state.json` | **None.** Delete `videos/setup-first-site/state.json` if it exists, and do not run `Recorder setup` or `Recorder prepare` for this video: there is no user to sign in before `03-finish`, and `04-signin` must start signed out (the recorder loads `state.json` when it exists). |

### Fake data

Only these values are typed, all outside the narration's control and all fake:

| Field | Value |
|---|---|
| What is the name of your site? | `Contoso Bakery` |
| Recipe | Blog |
| Default Time Zone | `America/Los_Angeles` |
| What type of database to use? | Sqlite (left as it is) |
| User Name | `admin` |
| Email | `admin@example.com` |
| Password and Password Confirmation | the value of `VIDEO_PASSWORD` |

There is no other seed data. The Blog recipe creates its own sample content (a blog, tags, a first post).

### Selectors used below

The Setup screen is a plain MVC view, so an `asp-for` property is the id of its field (`Views/Setup/Index.cshtml`).

| Element | Locator |
|---|---|
| Language list | `#culturesList` |
| Site name | `#SiteName` |
| Recipe button, and its items | `#recipeButton`, `#recipes .dropdown-item` (the Blog item has `data-recipe-name="Blog"`) |
| Default Time Zone | `#SiteTimeZone` |
| Database type | `#DatabaseProvider` |
| Super User section | `fieldset` (legend text `Super User`) |
| User Name, Email | `#UserName`, `#Email` |
| Password, Password Confirmation | `#Password`, `#PasswordConfirmation` |
| Eye buttons (**never click them**) | `#togglePassword`, `#togglePasswordConfirmation` |
| Finish Setup | `#SubmitButton` |
| Log in page fields | by label: `Username or email address`, `Password` (exact), and the button `Log in` |
| Admin dashboard cards | the card that contains the title: `scene.Page.Locator(".card", new() { HasText = "Create content" })`, and the same with `Customize theme` and `Manage users` |
| General settings fields | by label, exact: `scene.Page.GetByLabel("Site name", new() { Exact = true })`, `scene.Page.GetByLabel("Default Time Zone")` and `scene.Page.GetByLabel("Use framework CDN (Content Delivery Network)")` (the labels have a `for` attribute: `Views/Settings-Site.Edit.cshtml:16`, `:43`, `Views/Settings-Resources.Edit.cshtml:29`). Do not guess the ids of these fields: the settings editor builds them from the shape prefix. |

The scene methods take a locator, not a selector string. Where this plan writes a selector in a call, such as `HighlightAsync("#culturesList")`, it stands for `scene.HighlightAsync(scene.Page.Locator("#culturesList"))`; the same goes for `TypeAsync`, `SelectAsync` and `MoveToAsync`.

## Order of the clips, and what each one changes

Record in the order the clips play. Slide steps have no recording.

| Clip | Starts from | Changes the site? |
|---|---|---|
| `02-questions` | the uninstalled site | No. Nothing is submitted, so it can be recorded again at any time before `03-finish`. |
| `03-finish` | the uninstalled site | **Yes: it installs the site, and cannot be undone.** Record it once the take of `02-questions` is good. |
| `04-signin` | the site as `03-finish` left it, no `state.json` | No. It signs in and looks at a settings page without saving. |

Snapshots: the first snapshot is the empty `ORCHARD_APP_DATA` folder. Before a new take of `03-finish`, stop the site, empty
that folder, and start the site again (the screen must show the Setup form with the Blog recipe list). After a good
take of `03-finish`, stop the site and copy `ORCHARD_APP_DATA` aside as the second snapshot; restore it before a new take
of `04-signin`.

The setup runs inside one request, and the Blog recipe takes a while. The recorder's default page timeout is 20 seconds:
raise it for `03-finish` (`scene.Page.SetDefaultTimeout(300_000)`) before the click on Finish Setup.

## Chapter 01: What setup does (slides)

`01-idea` has three slides, drawn by `slides.py`.

- `idea-1`: the heading "An empty site asks first" and four cards (Open the address, Answer the questions, Select Finish Setup, Sign in to the admin); the first two are bright.
- `idea-2`: the same four cards; the last two are bright.
- `idea-3`: the heading "What Finish Setup does" and four cards: Checks the database, Runs the recipe, Saves your answers, Creates the super user. The facts come from `src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:183-243` (connection check, datastore, recipe), `src/OrchardCore.Modules/OrchardCore.Settings/Services/SetupEventHandler.cs:23-25` (site name, time zone and super user saved as settings) and `src/OrchardCore.Modules/OrchardCore.Users/Services/SetupEventHandler.cs:23-33` (the user, in the Administrator role).

## Chapter 02: The setup questions

### Clip `02-questions` (a recording)

**Starts on:** `/` of the uninstalled site, opened with `OpenAsync("/")` before `StartCaptureAsync()`. The screen shows
the heading **Setup**, the line "Please answer a few questions to configure your site.", an empty site name, the
Recipe button reading **Software as a Service**, a Default Time Zone list that reads `(GMT+00) Africa/Abidjan (+00:00)` (see the time zone row above), the database type **Sqlite**, and the **Super User**
section below. No Table Prefix, Connection String or Table Schema field is shown. Nothing is typed before the capture.

**Does not change the site.** Do not scroll during the clip. The page is expected to fit the 864-pixel viewport with Sqlite
selected. When the **Super User** legend is below the bottom edge after the page has loaded, do not scroll either: end the
cursor movement of `screen-1` on the last field that is on the screen, and tell the author (the narration says the form is on
one page, which stays true, but the ending frame of `screen-1` then differs from the description below).

- `screen-1`
  - Actions: move the cursor to the **Change language** list at the top right and put a ring on it (`HighlightAsync("#culturesList")`, no caption). After about five seconds clear the ring and move the cursor slowly down the form, from the site name field to the **Super User** legend. Do not open the language list: a headless browser draws no open list, and choosing a language reloads the page.
  - Visible when the narration ends: the whole Setup screen, with the cursor over the Super User section; the language list at the top right still shows the English entry.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:88` (Change language), `:102` (Setup), `:103` (the lead line). The language list reloads the page: `Assets/ts/setup.ts:66-77`.
- `name-1`
  - Actions: type `Contoso Bakery` into the site name field (`TypeAsync("#SiteName", "Contoso Bakery")`).
  - Visible when the narration ends: the field holds `Contoso Bakery` and the small line under it reads "This is used as the default title of your pages."
  - Labels from: `Views/Setup/Index.cshtml:118` (What is the name of your site?), `:121` (the hint).
- `recipe-1`
  - Actions: ring the Recipe button for about two seconds, click `#recipeButton`, and hold the opened list for about four seconds. It lists, in this order, Agency, Blank site, Blog, Coming Soon, Headless site and Software as a Service, each followed by a short description. "OpenApi Client Generation" must not be in the list. Click the **Blog** item (`#recipes .dropdown-item[data-recipe-name="Blog"]`).
  - Visible when the narration ends: the list is closed and the button reads **Blog**.
  - Labels from: `Views/Setup/Index.cshtml:130` (Recipe), `:142`, `:146-149` (the items, sorted by recipe name). The first selection: `Controllers/SetupController.cs:61`; only `src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:10` has the tag `default`. The names and descriptions: `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:3-4`, `TheAgencyTheme/Recipes/agency.recipe.json:3-4`, `TheAdmin/Recipes/blank.recipe.json:3-4`, `TheComingSoonTheme/Recipes/comingsoon.recipe.json:3-4`, `TheAdmin/Recipes/headless.recipe.json:3-4` and `TheTheme/Recipes/saas.recipe.json:3-4` (all under `src/OrchardCore.Themes/`). The hidden recipe: `Controllers/SetupController.cs:56-60`.
- `zone-1`
  - Actions: ring the Default Time Zone list, which reads `(GMT+00) Africa/Abidjan (+00:00)` at that moment, then `SelectAsync("#SiteTimeZone", "America/Los_Angeles")`.
  - Visible when the narration ends: the list shows the Los Angeles time zone (the text of the entry is `(GMT-08) America/Los_Angeles (-08:00)` in winter and ends with `(-07:00)` while daylight saving time applies). The small line under it reads "Determines the default time zone used when displaying and editing dates and times."
  - Labels from: `Views/Setup/Index.cshtml:159` (Default Time Zone), `:162` (the hint). The entries are `(GMT<offset>) <time zone id> (<offset>)`: `src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs`, `DefaultTimeZoneSelectListProvider.cs`.
- `db-1`
  - Actions: ring the database type list (`HighlightAsync("#DatabaseProvider")`). Do not change it: a server database shows a sample connection string, which stays off screen.
  - Visible when the narration ends: the list reads **Sqlite**; the line under it reads "The database is used to store the site's configuration and its contents."; Table Prefix, Connection String and Table Schema are not on the screen.
  - Labels from: `Views/Setup/Index.cshtml:176` (What type of database to use?), `:184` (the hint). The entries and their order (Sql Server, Sqlite, MySql, Postgres, with Sqlite selected and without a connection string): `src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:52-55`. The hidden fields: `Assets/ts/setup.ts:4-46`. The Sqlite file lives with the site's data: `src/docs/reference/modules/Data/README.md`, "SQLite options".

### Clip `02-databases` (slides)

- `db-2`: the heading "Databases that use a server" and four cards (Sql Server, MySql and Postgres; Connection String; Table Prefix; Table Schema); the first two are bright. The database must exist already: `src/docs/reference/modules/Data/README.md`, "Database providers", and `src/OrchardCore/OrchardCore.Data.YesSql/DbConnectionValidator.cs:91-108` (a connection that cannot open is "invalid").
- `db-3`: the same four cards; the last two are bright. Hints: `Views/Setup/Index.cshtml:192` (Table Prefix) and `:213` (Table Schema).
- `db-4`: the heading "When setup can't finish" and two cards with the messages "The provided connection string is invalid or server is unreachable." and "The provided database, table prefix and schema are already in use." (`src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:192-199`), shown by the Setup screen at the top (`Controllers/SetupController.cs:198-206`).
- `db-5`: the heading "Answers your host can preset" and three cards (Recipe; Database type; Database settings). Source: `Views/Setup/Index.cshtml:124` (the recipe), `:173` (the type), `:165` (the whole database block).

## Chapter 03: Super user and Finish Setup

### Clip `03-finish` (a recording)

**Starts on:** `/` of the uninstalled site, with the form already answered, **before the capture starts** (none of this is
recorded): `OpenAsync("/")`, type `Contoso Bakery` into `#SiteName`, click `#recipeButton` and the **Blog** item,
`SelectAsync("#SiteTimeZone", "America/Los_Angeles")`. The database type stays Sqlite and the Super User fields stay
empty. Set the page timeout to 300 seconds. When the Super User section and the Finish Setup button do not fit on the
screen, scroll so both are in view, then start the capture. Do not scroll during the clip.

**Changes the site:** the last two steps install it. Record this clip once, from the empty snapshot.

- `user-1`
  - Actions: ring the **Super User** legend and the line under it (`HighlightAsync` on the fieldset's `legend`, then on the `span` that follows it).
  - Visible when the narration ends: the legend **Super User** and the line "The super user has all the rights. It should be used only during Setup and for disaster recovery." with a ring around them; the four fields empty.
  - Labels from: `Views/Setup/Index.cshtml:218` (Super User), `:219` (the line).
- `user-2`
  - Actions: clear the ring; type `admin` into `#UserName`, then `admin@example.com` into `#Email`.
  - Visible when the narration ends: User Name reads `admin`, Email reads `admin@example.com`; no red message.
  - Labels from: `Views/Setup/Index.cshtml:222` (User Name), `:227` (Email). The characters a user name accepts: `Startup.cs:61` of the Setup module.
- `user-3`
  - Actions: click `#Password`; the list "Password requirements:" appears above the field. Then type the value of `VIDEO_PASSWORD`, so the rules turn green one by one.
  - Visible when the narration ends: the list above the Password field with every rule checked off in green, the bar under the field full and green, and the typed text shown as dots. Never click `#togglePassword`.
  - Labels from: `Views/Setup/Index.cshtml:234` (Password); the list and its entries: `Assets/ts/setup.ts:160-273` (the texts are in the script, `:180-207`); the bar: the password strength component built into `wwwroot/Scripts/setup/setup.js`.
- `user-4`
  - Actions: click `#PasswordConfirmation` (the list closes) and type the value of `VIDEO_PASSWORD`. Move the cursor to the eye button beside Password Confirmation and ring both eye buttons (`HighlightAsync("#togglePassword")`, `HighlightAsync("#togglePasswordConfirmation")`). **Do not click either button**: it would show the password on screen.
  - Visible when the narration ends: both password fields filled, shown as dots, with a ring around each eye button.
  - Labels from: `Views/Setup/Index.cshtml:244` (Password Confirmation), `:237` and `:247` (the eye buttons), `Assets/ts/setup.ts:287-303` (what they do).
- `finish-1`
  - Actions: clear the rings and move the cursor to **Finish Setup** (`MoveToAsync("#SubmitButton")`). Then click it inside `OffCameraAsync`, so the wait for the setup is cut out of the clip: `await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("#SubmitButton").ClickAsync()))`. The wait ends when the browser has loaded `/` again and the page no longer has `#SubmitButton`.
  - Visible when the narration ends: the home page of the new site (the Blog theme) is fully loaded. If the screen shows the Setup form again with a red message at the top, or an error page, discard the take, empty `ORCHARD_APP_DATA`, restart the site and record again.
  - Labels from: `Views/Setup/Index.cshtml:256` (Finish Setup). What happens: `Controllers/SetupController.cs:185-209` (the redirect to the home page), `src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:183-312`.
- `finish-2`
  - Actions: ring the site name in the header (`HighlightAsync("a.navbar-brand")`).
  - Visible when the narration ends: the header of the Blog theme with **Contoso Bakery** circled.
  - Labels from: `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27` (the site name is the brand in the header).

## Chapter 04: Sign in and check the result

### Clip `04-signin` (a recording)

**Starts on:** `/` of the installed site, signed out (no `state.json`), opened with `OpenAsync("/")` before the capture.
Restore the second snapshot first when the site was started again.

**Does not change the site.** Do not click Save on the settings page.

- `signin-1`
  - Actions: `NavigateAsync("/Admin")`. The site sends the browser to the Log in page. Ring the **Log in** heading with the caption `Your site address + /Admin` (`HighlightAsync` on the heading, with that caption), so the address shows on screen: the recording has no address bar.
  - Visible when the narration ends: the Log in page with the heading **Log in**, the fields Username or email address and Password empty, and the Log in button, with the caption on the heading.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Users/Views/Account/Login.cshtml:27` (Log in). The admin address: `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminOptions.cs:5` (the default prefix is `Admin`) and `src/OrchardCore.Modules/OrchardCore.Admin/Startup.cs:96`. The redirect to the login page: `AdminController` has `[Authorize]` (`src/OrchardCore.Modules/OrchardCore.Admin/Controllers/AdminController.cs:6`), and the login path is `Login` (`src/OrchardCore/OrchardCore.Users.Abstractions/UserOptions.cs:5`).
- `signin-2`
  - Actions: clear the ring. Type `admin` into the field labeled **Username or email address**, then the value of `VIDEO_PASSWORD` into the field labeled **Password** (exact label). Move the cursor to the **Log in** button, without clicking: the click belongs to `signin-3`, whose narration begins with "Select Log in." Never click the eye button on this page either.
  - Visible when the narration ends: the user name `admin` in the first field, dots in the second, the cursor on Log in.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Users/Views/LoginFormCredentials.cshtml:7` (Username or email address), `:13` (Password); `src/OrchardCore.Modules/OrchardCore.Users/Views/LoginForm.Edit.cshtml:9` (Log in).
- `signin-3`
  - Actions: click **Log in** as the step starts (`ClickAndWaitForNavigationAsync`; the narration begins with "Select Log in."). On the admin page, ring the first three cards (Create content, Customize theme, Manage users: the locators are in the table above) one after the other, then clear the rings.
  - Visible when the narration ends: the admin page at `/Admin`, with the message **Welcome to Orchard Core**, the line "Your site is ready. Get started with the actions below, or browse the menu to discover all its possibilities.", and the cards **Create content**, **Customize theme**, **Manage users** (its text reads "Add users and assign roles and permissions.") and **Learn more**, and the admin menu.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml:2-3`, `OrchardCore.Contents/Views/AdminDashboardContents.cshtml:4`, `OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4`, `OrchardCore.Users/Views/AdminDashboardUsers.cshtml:4` (the title) and `:5` (the line "Add users and assign roles and permissions.", which the narration paraphrases as "where you add users and assign roles"), `OrchardCore.Admin/Views/AdminDashboardDocumentation.cshtml:4`. These cards are what `/Admin` shows while the Admin Dashboard feature is off, which is the case after the Blog recipe (`blog.recipe.json` does not enable it).
- `settings-1`
  - Actions: in the admin menu, click **Settings** (expand it when it is collapsed), then **General** (`ClickAndWaitForNavigationAsync`). When the menu does not respond, `NavigateAsync("/Admin/Settings/general")`. Ring the **Site name** field and the **Default Time Zone** list (by label, as in the table above).
  - Visible when the narration ends: the General settings page; Site name reads `Contoso Bakery`; Default Time Zone shows the Los Angeles time zone; both ringed.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` (Settings), `:39` (General); `src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:16` (Site name), `:43` (Default Time Zone); the route `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`. The values come from `src/OrchardCore.Modules/OrchardCore.Settings/Services/SetupEventHandler.cs:23-25`.
- `settings-2`
  - Actions: clear the rings, then scroll down (`ScrollAsync`) until the section "Settings for scripts and stylesheets" is on the screen, and ring the checkbox **Use framework CDN (Content Delivery Network)** (by label, as in the table above). Do not change or save anything.
  - Visible when the narration ends: the checkbox is **not** selected and has a ring; its hint reads "Whether a framework CDN is used for registered scripts and stylesheets, such as jQuery, or their local version".
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Resources.Edit.cshtml:11` (the section), `:29` (the checkbox), `:30` (the hint). The setting is off by default: `src/OrchardCore.Modules/OrchardCore.Settings/SiteSettings.cs:29` (`bool`), and no setup recipe sets it. The recommendation, and the reason the setting is off (the site works when no internet connection is available): `src/docs/reference/modules/Setup/README.md`, "CDN disabled by default". The hint on screen says "or their local version"; the narration says "from its own files".

## Closing checks for private data

Run the checks of `SKILL.md` on the finished video (OCR at one frame per second, at every scene change, and at five
frames per second). The only names that may appear are `Contoso Bakery`, `admin`, `admin@example.com`, `localhost`
and the recipe's sample content. In particular:

- The value of `VIDEO_PASSWORD` must not appear in any frame: search the OCR text for it, and look at the frames of
  `user-3`, `user-4` and `signin-2` by eye. Both password fields and the Log in field show dots only.
- The Default Time Zone list must read `(GMT+00) Africa/Abidjan (+00:00)` before `zone-1` changes it (UTC is not an entry, so this is the first entry of the list), not the recorder's own zone: look at the frames of `screen-1` and `name-1` through `recipe-1` and the first frames of `zone-1`.
- No error page, no "Service Unavailable" page, and no red message on the Setup screen.
- No path of the recorder's machine, machine name or real address.
