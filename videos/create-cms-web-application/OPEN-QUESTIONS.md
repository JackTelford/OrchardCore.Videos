# Open questions

What the author could not verify from the source at `92db4a83` or from the documentation. The narration claims no more than the
source and the documentation show. Where a slide or a recording step relies on something else, it is listed here, and a person
confirms it on a machine that runs the project, before recording, then corrects `storyboard.json`, `slides.py` or
`recording-plan.md` if the answer differs.

## What the author checked

- Every label of the Setup screen, the sign-in page and the admin welcome notice, in the views (`recording-plan.md` gives the
  file and line of each).
- The template: `template.json`, `dotnetcli.host.json`, `Program.cs`, the project file, `appsettings.json`, `NLog.config` and
  `launchSettings.json` of `OrchardCore.Templates.Cms.Web`, and the package references of the two CMS targets packages.
- The recipes of the CMS targets package (names, display names, the `default` tag) and the database providers of the Setup screen.

## To confirm on a running machine

1. **The install command without a version.** The slide and the narration say that the package ID installs the templates and that
   `@` and a version number select a release. `dotnet new install OrchardCore.ProjectTemplates` with no version installing the
   latest stable release is behavior of the .NET SDK, not of the Orchard source. The documentation shows a command with a
   version, and the guide's command ends with a version pattern followed by `-*`, which the templates page uses only with the
   preview source: which one installs the latest stable release? The slide `install-2` shows the preview command in the form
   the templates page gives (`@<version>-*` with `--nuget-source`); confirm that this form installs the preview templates, and
   what `<version>` means for a viewer (the page uses the number of the next release).
2. **The console text (slide `run-2`).** The two `Now listening on` lines and the `Application started` line come from the guide.
   The template configures NLog, whose rules send the hosting lifetime events to the console and to the log file
   (`NLog.config`), so the real console lines may carry a prefix such as a time stamp. The slide shows `...` for it. If the text
   differs, change `CONSOLE` in `slides.py`. Also confirm that `dotnet run` uses the launch profile of the project (both
   addresses, the environment `Development`). `Properties/launchSettings.json` lists an `IIS Express` profile first and the `MySite`
   profile second (`OrchardCore.Templates.Cms.Web/Properties/launchSettings.json:11,18`); that `dotnet run` skips the first one
   and uses the first profile whose `commandName` is `Project` is behavior of the .NET SDK, not of the Orchard source.
3. **The development certificate.** On a machine where the ASP.NET Core development certificate is not trusted, the browser warns
   at the https address. The documentation does not mention `dotnet dev-certs`, so the narration only says to open the https
   address. If the warning appears for a new viewer, add a sentence.
4. **The recording address.** The plan runs the site on `http://localhost:5010` with no https address, so that
   `app.UseHttpsRedirection()` has no port to redirect to. That is ASP.NET Core behavior, not Orchard source. Confirm that the
   Setup screen loads on that address without a redirect.
5. **The home page after setup.** The plan expects the navigation bar to show the site name (`Layout.liquid` of the Blog theme)
   and the post `Man must explore, and this is exploration at its greatest` (`blog.recipe.json`). The theme's layout also asks
   Google Fonts for fonts, and the recipe names image files (`home-bg.jpg`, `post-bg.jpg`): confirm what the page looks like
   when it is recorded, and whether anything is missing.
6. **How long the setup takes.** The documentation says a few seconds. The plan allows up to 120 seconds off camera, which needs
   an explicit timeout in the scene: the scene's default is 20 seconds (`Scene.cs:123`). Confirm on the machine how long the Blog
   recipe takes the first time.
7. **The admin landing page.** The Blog recipe does not enable `OrchardCore.AdminDashboard`, so `/admin` renders the view of the
   Admin module (`Welcome to Orchard Core`). Confirm that this is what the admin shows after the first sign-in.
8. **`--logger none` in lower case.** The documentation shows the choices in lower case (`nlog`, `serilog`, `none`); the template's
   choices are `NLog`, `Serilog` and `None`. `dotnet new` is expected to accept any case, and the template's own usage examples use the lower case form
   (`OrchardCore.Templates.Cms.Web/.template.config.src/dotnetcli.host.json:19`, `--logger none`). Confirm with
   `dotnet new occms --help` and by creating a project with `--logger none` (slide `create-6`, narration of `create-6` and `log-3`).
9. **The labels of the installed release.** The labels were read from the source at `92db4a83`; the project the recorder runs
    uses the packages of the installed templates. The label check in `recording-plan.md` compares them before recording.
10. **What the Password field draws.** `setup.ts` opens a popover headed `Password requirements:` when the Password field gets
    focus and removes it on blur (`OrchardCore.Setup/Assets/ts/setup.ts:172-272`), and starts a strength indicator with
    `strenght(passwordElement, options)` (`setup.ts:140`), imported from `@orchardcore/bloom/components/password-strength`.
    That package is not in the source at `92db4a83`, so what the indicator looks like, and where it sits, is not verified.
    Confirm that the popover is not hidden by the indicator or by the page, and that the narration sentence about the rules
    matches what is on screen. The Bootstrap popover titled `Password strength` in `Index.cshtml:236` is not started by `setup.ts`;
    confirm that it does not open.
11. **The text of the time zone option.** The option reads `(GMT<standard offset>) America/Los_Angeles (<offset now>)`
    (`OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25`). NodaTime, which is not in the source, formats the offsets, and the
    second one depends on the date. Confirm what the select shows on the day of the recording. The narration only says
    `Default Time Zone`.
12. **Where `App_Data` is created.** The narration and the slides say only that the data of the site is in `App_Data`, not where the
    folder is. The source puts it under the content root (`OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:28`). Whether the
    content root is the folder of `MySite.csproj` when the project runs with `dotnet run --project MySite/MySite.csproj` from the
    folder above depends on the .NET SDK, so the narration does not claim it. Confirm where `App_Data` appears after the first
    start; if it is inside `MySite`, a sentence can say so.

## Pronunciation, to confirm with `scripts/pronounce.py` (it writes audio, so the author did not run it)

`storyboard.json` has these `spoken` entries; each is a guess until a person has listened to it:

| Written | Spoken as |
| --- | --- |
| `Program.cs` | `Program dot C.S.` |
| `dotnet` | `dot net` |
| `occms` | `O.C.C.M.S.` |
| `ocmodulecms` | `O.C. module C.M.S.` |
| `octheme` | `O.C. theme` |
| `NLog` | `N Log` |
| `NuGet` | `New Get` |
| `Sqlite` | `S.Q. Lite` |
| `OrchardCore.ProjectTemplates` | `Orchard Core Project Templates` |
| `ORCHARD_APP_DATA` | `Orchard App Data` |
| `App_Data` | `App Data` |
| `MySite` | `My Site` |
| `/admin` | `slash admin` |

Also check the words with no entry: `Serilog`, `https`, `http`, `MVC`, `CMS` and `SDK`; the spaced API names `Use N Log Host`,
`Add Orchard CMS` and `Use Orchard Core`; and "Control and C", which the narration says for the key combination.
