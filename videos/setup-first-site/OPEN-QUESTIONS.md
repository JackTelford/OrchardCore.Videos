# Open questions: Setup: Install Your First Orchard Core Site

The narration states only what the source and the documentation show. The source was read at commit
`92db4a837ddad07f4b35179df59ac78375017860` and no site was run. A person must confirm each point below on a running
site before the video is recorded, and tell the author when the screen differs.

## What the narration says that a running site must confirm

| Step | What the narration says | What was checked | What to confirm on screen |
|---|---|---|---|
| `recipe-1` | Software as a Service is selected when the screen opens. | `Controllers/SetupController.cs:61` selects the first recipe whose tags contain `default`. Only `saas.recipe.json` has that tag; Blog has `default` only as a category. | The Recipe button reads "Software as a Service" when the screen opens, and the list shows the descriptions. If another recipe is selected first, the build has other recipes: change the step. |
| `recipe-1` | The list can be opened and has descriptions. | `Views/Setup/Index.cshtml:141-150` renders a dropdown with a display name and a description per recipe. The list is expected to hold Agency, Blank site, Blog, Coming Soon, Headless site and Software as a Service (the "OpenApi Client Generation" recipe is tagged `hidden`). | The dropdown opens in the headless browser, with all six entries. |
| `screen-1` | At the top right, Change language reloads the screen in another language. | `Views/Setup/Index.cshtml:79-101` shows the list when more than one UI culture is supported; `Assets/ts/setup.ts:66-77` navigates to the URL of the chosen culture. | The list is on the screen, with an English entry selected. |
| `zone-1` | Default Time Zone sets how dates and times are shown and edited; Los Angeles can be chosen from the list. The narration does not say which zone the list starts on. | `Controllers/SetupController.cs:73` preselects the time zone of the server, but the list holds only the time zones of places (`Clock.cs:22-33`), and a server on UTC gets the id `Etc/UTC`, which is not in it (checked by listing the zone locations of the time zone database with NodaTime). No entry matches, so the browser is expected to show the first entry, `(GMT+00) Africa/Abidjan (+00:00)`. The package version of NodaTime that Orchard Core pins was not available, so the first entry was checked with another 3.x release. | With the time zone of the machine set to UTC (`tzutil /s "UTC"` on Windows, `TZ=UTC` on Linux and macOS), the list reads `(GMT+00) Africa/Abidjan (+00:00)` before the clip changes it, and the entry with `America/Los_Angeles` can be chosen. Tell the author if the screen shows another entry. |
| `db-1` | Sqlite is selected and needs no database server or connection string; Orchard Core keeps the database as a file in the data folder of the site. | Sqlite is the default provider without connection string, table prefix or schema (`OrchardCoreBuilderExtensions.cs:53`, `Assets/ts/setup.ts:4-46`); the file is under the shells data folder (`SqliteHelper.cs:51-52`, and `src/docs/reference/modules/Data/README.md`, "SQLite options": the database file of each tenant is in that tenant's shell data directory). | Sqlite is selected, and no Table Prefix, Connection String or Table Schema field is shown. |
| `db-2`, `db-3` | Server databases need a Connection String and an existing database; Table Prefix and Table Schema are optional. | The fields appear for those providers (`OrchardCoreBuilderExtensions.cs:52,54,55`); only Connection String is required (`Views/Setup/Index.cshtml:198-203`); the "create the database first" rule is in `src/docs/reference/modules/Data/README.md`, and a connection that cannot be opened is reported as invalid (`DbConnectionValidator.cs:91-108`). | Not recorded on screen. A person with SQL Server, MySql or Postgres can confirm that setup fails with the "invalid" message when the database does not exist. |
| `db-4` | The two messages on the slide are shown at the top of the screen. | `SetupService.cs:192-199`, `SetupController.cs:198-206`, and the validation summary at `Views/Setup/Index.cshtml:106`. | Optional: provoke the second message by setting up a second site on the same Sqlite file or database and prefix, and read the text. |
| `user-3` | A list shows the password rules and checks each one off; a bar shows the progress. | The list is built on focus in `Assets/ts/setup.ts:160-273`; the bar is in the bundled `wwwroot/Scripts/setup/setup.js` (password strength component). The rules themselves come from the default identity options; no module of the repository changes them, so the narration gives no numbers. | The list appears above the field on focus, entries turn green as the password meets them, the bar fills. Tell the author the rules if a number should be said. |
| `finish-1` | Orchard Core checks the database, builds the site, and then opens it. | `SetupService.cs:183-312` and the redirect at `SetupController.cs:209`. The request stays open until the recipe has run. | How long the wait is, and that the browser lands on the home page. The plan cuts the wait out of the clip. |
| `finish-2` | The name you entered is in the header of the front end. | `TheBlogTheme/Views/Layout.liquid:27` prints `Site.SiteName` as the brand. | The header shows "Contoso Bakery". |
| `signin-1` | Adding Admin to the address leads to a login first. | `AdminController` has `[Authorize]`, the login path is `Login`, and the default admin prefix is `Admin`. | `/Admin` redirects to the Log in page. |
| `signin-3` | A welcome message and cards for content, theme and users; Manage users is where you add users and assign roles. | `OrchardCore.Admin/Views/AdminDashboard.cshtml` and the three dashboard card views; they are what `/Admin` shows while the Admin Dashboard feature is off. | The Blog recipe leaves that feature off, and the page shows the four cards. |
| `settings-1` | Setup saves the site name and the time zone as settings; under Settings, General, Site name and Default Time Zone hold them. | `OrchardCore.Settings/AdminMenu.cs`, `Services/SetupEventHandler.cs:23-25`, `Views/Settings-Site.Edit.cshtml`. | The menu entries exist as Settings and General in the redesigned admin menu, and the page shows the saved values. |
| `settings-2` | Use framework CDN is not selected after setup, so the site loads its scripts and stylesheets from its own files, which works without an internet connection; the documentation recommends selecting it after setup. | `SiteSettings.UseCdn` is a plain `bool`, and no setup recipe sets it. The reason and the recommendation are in `src/docs/reference/modules/Setup/README.md`, "CDN disabled by default". | The checkbox is unchecked, and its hint mentions the local version of the scripts and stylesheets. |

## Pronunciation

No word was added to `spoken`, and no audio was generated. A person should listen to the voice on each of these with
`scripts/pronounce.py` before narrating (`db-1`, `db-2`, `name-1`, `settings-2`), and confirm any spelling before it goes
into `storyboard.json`. The written form stays in the captions and on the slides.

| Written in the narration | Spellings to try when the voice misreads it |
|---|---|
| `Sqlite` (`db-1`) | `S.Q.L. lite`, `sequel lite` |
| `Sql Server` (`db-2`) | `S.Q.L. Server`, `sequel Server` |
| `MySql` (`db-2`) | `My S.Q.L.`, `My sequel` |
| `Postgres` (`db-2`) | `Post gres`, `Postgres` as written |
| `jQuery` (`settings-2`) | `j Query` |
| `Contoso` (`name-1`) | as written |

## Not checked

- The bar and the list of rules of the Password field, in a browser: the source was read, not run.
- That the Los Angeles entry is found by the value `America/Los_Angeles` in the headless browser's `SelectAsync`
  (the option values are the time zone ids: `Views/Setup/Index.cshtml:160`, `DefaultTimeZoneSelectListProvider.cs:18-19`).
- That the Setup screen fits the 864-pixel viewport without scrolling when Sqlite is selected: the page was not rendered.
- That the first entry of the Default Time Zone list in the recorded build is `(GMT+00) Africa/Abidjan (+00:00)`, as in the NodaTime release used for the check described under `zone-1`.
