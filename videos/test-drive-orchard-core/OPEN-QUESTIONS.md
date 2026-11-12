# Open questions: Test Drive Orchard Core

What was checked in the Orchard Core source (the scanned tree at commit `92db4a83`, which holds `src/` and `src/docs`
only), and what a person must confirm on a running site or machine before the video is recorded or published. The
narration states only what the source or the documentation page shows; each item below is a place where a running
system is the last word.

## Pronunciation (the `spoken` map of `storyboard.json`)

No audio was generated. Check each spelling with `scripts/pronounce.py` before narrating, and fix the map where the
voice gets it wrong:

- Added entries: `dotnet` as "dot net", `OrchardCore.Cms.Web` as "Orchard Core dot C.M.S. dot Web",
  `OrchardCore.Application.Cms.Targets`, `OrchardCore.Modules`, `OrchardCore.Themes`, `OrchardCMS/OrchardCore` as
  "Orchard C.M.S. slash Orchard Core", `AddOrchardCms` as "Add Orchard C.M.S.", `AddSetupFeatures`, `UseOrchardCore`,
  `Program.cs` as "Program dot C.S.", `App_Data` as "App Data", `App_Data/Sites/Default` as "App Data, Sites, Default"
  (the commas should give a short pause, not a long one), `ORCHARD_APP_DATA` as "Orchard App Data", `OrchardCore.db` as
  "Orchard Core dot D.B.", `8080` as "eighty eighty", `Ctrl+F5` as "Control F5", `/Admin` as "slash Admin".
- Entries added by the review, for the same check: `src` as "S.R.C." (it could be read as one word; some people say
  "source" instead), `Sqlite` as "S.Q.L. light", `NuGet` as "New Get", `DotNest` as "Dot Nest". Check that "S.R.C." does
  not stop the sentence short (the full stops), and that the voice's own reading of `Sqlite`, `NuGet` and `DotNest` was
  not already right, in which case remove the entry.
- Words with no entry that need listening: `Auto Setup`, "cloud-hosted", "localhost", "CMS".
- `ASP.NET` is already spoken as "A.S.P. .NET" by `narrate.py`.

## What `dotnet run` does and prints

- `source-2` and `source-3` say that the command "restores the NuGet packages, builds the projects, and starts the site"
  and that "the console reports the addresses the site listens on". Checked in the source: the `Kestrel` launch profile
  (`src/OrchardCore.Cms.Web/Properties/launchSettings.json:18-25`) lists `https://localhost:5001;http://localhost:5000` and
  sets the Development environment, and `NLog.config` sends the `Microsoft.Hosting.Lifetime` events to the console. Not
  checked: that `dotnet run` picks that profile on every SDK, and the exact lines printed. Run it and confirm that the
  console shows the `http://localhost:5000` address.
- `launchBrowser` is `true` in the profile, but only an IDE honors it: the narration never says that a browser opens.
- Which .NET SDK the checkout needs. `src/OrchardCore.Build/TargetFrameworks.props:7` and `:11` set `net11.0`, and the scanned
  tree has no `global.json`. The narration says "Install the .NET SDK" without a version (versions are not allowed in
  narration). Confirm the SDK to install, and whether `contributing-code.md` ("Visual Studio 2022 (v17.8+)") still names a
  Visual Studio that can open the solution.
- Visual Studio (`source-3`): the documentation says to launch the solution (`contributing-code.md:22`), set
  `OrchardCore.Cms.Web` as the startup project and press Ctrl+F5; the narration says "open the solution". The solution
  file is not named in the narration: `contributing-code.md:22` says `OrchardCore.sln`, while
  `src/docs/reference/modules/Localize/README.md:500` uses `OrchardCore.slnx`, and the scanned tree has no file at its root.

## Statements that come from the documentation page only

These cannot be read from the Orchard Core source; they are taken from `src/docs/getting-started/test-drive-orchard-core.md`
(`ways-1` to `ways-3`) and should be checked on the services before publishing:

- Try Orchard Core: a demo site; the Agency recipe is suggested ("Ready to use company website" in
  `agency.recipe.json:4`) and the Blog recipe; the site is deleted the next Sunday. The slide shows `try.orchardcore.net`.
  The page says "two clicks"; the narration and the slide do not repeat that, because it cannot be checked here.
- DotNest: a hosting service for Orchard Core, cloud-hosted sites, not deleted periodically, usable in production.
  The page also says "for free" and names the company that operates it; the video says neither.
- Docker: `docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest`. The scanned tree has no
  `Dockerfile`. Run the command and confirm that the image serves the Setup screen at `http://localhost:8080` (`ways-3`
  says that `-p` publishes port 80 of the container on port 8080 of the computer, "so you open localhost on port 8080").

## The Setup screen (`setup-1` to `setup-5`)

Read from `Views/Setup/Index.cshtml`, `SetupController.cs` and the setup recipes; confirm on screen:

- The **Recipe** button starts on *Software as a Service* (the only setup recipe tagged `default`,
  `saas.recipe.json:10`, chosen at `SetupController.cs:61`), and the list holds *Agency*, *Blank site*, *Blog*,
  *Coming Soon*, *Headless site* and *Software as a Service*, in that order (`OrderBy(x => x.Name)` on the names
  `Agency`, `Blank`, `Blog`, `ComingSoon`, `Headless`, `SaaS`). The `OpenApiGenerationSetup` recipe is tagged `hidden`.
  This is why `setup-2` says "starts on Software as a Service".
- **Default Time Zone** starts on the system time zone (`SetupController.cs:73`, `Clock.cs:47-55`); the narration says so.
  The list holds the NodaTime time zone database, not the operating system's zones (`Clock.cs:22-30`), and its option
  values are the zone ids (`DefaultTimeZoneSelectListProvider.cs`), so the plan's value `America/Los_Angeles` exists on
  every machine. Not checked on a running site: what the option label looks like. The plan selects the zone before the
  capture, so `setup-3` says "starts as the time zone of your computer" over a list that shows `America/Los_Angeles`.
- With *Sqlite* selected, **Connection String**, **Table Prefix** and **Table Schema** are hidden by `setup.ts`; the
  narration only says that no database server is needed.
- The password rules come from the host's identity options (`Views/Setup/Index.cshtml:32-66`); the plan says "every rule
  that the *Password requirements:* box lists". That box is built by `Assets/ts/setup.ts:172-233` (header text `:207`,
  rules `:180-199`) when **Password** gets the focus, and removed on blur (`:270-272`). The view's `data-title="Password
  strength"` (`Index.cshtml:236`) is not shown: the compiled `setup.min.js` creates no Bootstrap popover. Not seen on
  screen: the height of the box and what it covers above the field (it is placed above the input by script,
  `setup.ts:224-229`), so the plan accepts that it covers **User Name** and **Email** in the last frame of `setup-4`.
  The narration of `setup-4` calls the box "the Password requirements that the screen lists": confirm that the title on
  screen reads *Password requirements:*.
  Confirm that the chosen `VIDEO_PASSWORD` passes, that the box shows every rule ticked after the final click on
  **Password**, and that Setup does not sign the account in (it only redirects to `/`, `SetupController.cs:209`, and
  neither `SetupService.cs` nor the setup event handlers of Users and Settings sign in), which `explore-2` relies on.
- How long **Finish Setup** takes, and that the redirect to `/` lands on the Blog home page.
- `setup-2` says "A recipe sets up a site with pre-configured features and settings". That paraphrases the hint under the
  **Recipe** button (`Views/Setup/Index.cshtml:155`). The description of Blog (`blog.recipe.json:4`) is read aloud as
  "provides a functional blog with CMS features".
- The plan's scroll distance in `setup-4` (250 pixels) and the pauses of `setup-2` and `setup-3` are estimates from the
  length of the narration and the layout of the view; check them on the contact sheet.
- `explore-2` says that the Setup screen advises using the super user only for setup and disaster recovery. That sentence is
  `Views/Setup/Index.cshtml:219`, in the **Super User** section (`setup-4` says only "the account that has all the
  rights", the first half of that sentence); confirm that it is visible in the capture of `setup-4`
  (the box of the password rules can cover it in the last frame, but it is on screen while the fields are typed).

## The home page and the admin (`explore-1` to `explore-3`)

- The Blog home page: heading *Blog*, post *Man must explore, and this is exploration at its greatest*, the menu *Home*
  and *About* (`blog.recipe.json`). Confirm what is in view at 1536 by 864 and the scroll distance of `explore-1`.
- `/Admin` redirects an anonymous visitor to `/Login` (`AdminController` is `[Authorize]`; the login path is `Login`),
  and the login form fields have the ids `LoginForm_UserName` and `LoginForm_Password` (the ids that
  `scripts/recorder/Program.cs` uses). The labels were read in `LoginFormCredentials.cshtml`.
- The welcome page and its four cards were read from `OrchardCore.Admin/Views/AdminDashboard.cshtml` and the three
  `AdminDashboard*.cshtml` views of Contents, Themes and Users, plus `AdminDashboardDocumentation.cshtml`. `TheAdmin`
  overrides none of them. Confirm that all four cards show for the `admin` account on a site set up with the Blog recipe.

## Behind the site (`host-1` to `host-3`, `data-1`, `data-2`)

- `data-1` draws `App_Data/tenants.json` and `App_Data/Sites/Default/OrchardCore.db`. Setup stores `OrchardCore.db` as the
  `DatabaseName` of a new Sqlite tenant (`SetupService.cs:178-180`); `yessql.db` is only the fallback for a tenant that has
  no `DatabaseName` (`SqliteHelper.cs:23-26`), such as a site set up before the setting existed. The folder is
  `<App_Data>/Sites/<tenant>` (`SqliteHelper.cs:51-52`, `ShellOptionsSetup.cs:28`); `tenants.json` is named in
  `ShellSettings.cs:11-12` and `OrchardCoreConstants.cs:9`, and the first request already writes it
  (`ShellHost.cs:428-437`). Look at the folder after `03-setup` and confirm both files and the file name.
- `data-2` says that deleting `App_Data` while the site is stopped brings the Setup screen back. Read in
  `ShellHost.cs:357-366` (a missing default tenant runs the setup), not run. Confirm it, and that `ORCHARD_APP_DATA`
  (`ShellOptionsSetup.cs:20-24`) is applied as the narration says.
- `host-2`: `AddSetupFeatures("OrchardCore.AutoSetup")` is in `OrchardCore.Cms.Web/Program.cs`; its summary says that it
  registers features "used to setup and configure the actual tenants"
  (`OrchardCoreBuilderExtensions.cs:50`). "Installs a site on the first request from settings in the configuration" is the
  first sentence of `src/docs/reference/modules/AutoSetup/README.md` and its configuration section; "does nothing without
  those settings" is `OrchardCore.AutoSetup/Startup.cs:95-98` (`ConfigurationExists`); "the Setup screen appears" is what
  the recording of `03-setup` shows. Not run: Auto Setup itself, which a separate video covers.
- `host-3`: the project reference to `OrchardCore.Application.Cms.Targets` is at `OrchardCore.Cms.Web.csproj:52`; the
  package references the modules (`OrchardCore.Application.Cms.Core.Targets`) and the front-end themes
  (`OrchardCore.Application.Cms.Targets.csproj:34-38`; `TheAdmin` comes with the core targets). The module named
  `OrchardCore.Themes` (under `src/OrchardCore.Modules`) and the folder `src/OrchardCore.Themes` share a name; the narration
  says "under src" to mean the folder.
- `data-1` says "Default is the name of the tenant, the site you set up". `Default` is the name of the first tenant
  (`ShellSettings.DefaultShellName`, and the folder `Sites/<tenant>`); the narration defines a tenant only as that site.
