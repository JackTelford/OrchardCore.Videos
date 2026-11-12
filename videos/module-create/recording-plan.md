# Recording plan: Create a Module

For the person who records the browser clips. Everything is decided here; `OPEN-QUESTIONS.md` lists what the
recorder must also confirm on the running site. Only two chapters are recordings: `05-enable` (Tools, Features) and
`06-run` (the page of the module). The other chapters are slides, drawn by `slides.py`; they need no site.

In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators.

## 1. The site it needs

Everything below is throwaway and lives outside this repository. Nothing of it is committed.

**Orchard Core.** A throwaway checkout of `OrchardCMS/OrchardCore` at commit `92db4a83` (the source every label of
this video was read from). The host is `src/OrchardCore.Cms.Web` of that checkout.

**The module.** Generated exactly as the slides show, in a work folder outside both repositories:

```bash
dotnet new install OrchardCore.ProjectTemplates
dotnet new ocmodulecms -n Greetings.OrchardCore
```

The generated project file is replaced below, so the version of the package references it holds does not matter. Then
make the edits the video shows (the manifest and the view), and the two it does not show (`Startup.cs` and
`HomeController.cs`), with these exact contents.

`Greetings.OrchardCore/Manifest.cs` (replaces the generated file):

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Greetings",
    Author = "Jamie Chen",
    Website = "https://example.com",
    Version = "1.0.0",
    Description = "Adds a greeting page to the site.",
    Category = "Samples"
)]
```

`Greetings.OrchardCore/Views/Home/Index.cshtml` (replaces the generated file, which holds the word "Template"):

```html
<h1>Hello from the Greetings module</h1>
```

`Greetings.OrchardCore/Startup.cs` (replaces the generated file; the generated one imports the namespaces of the
sample part, which does not exist without `--AddPart`, see question 1 of `OPEN-QUESTIONS.md`; this one is what the
slide of the Startup class shows, with its usings and namespace added):

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Greetings.OrchardCore;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "Greetings.OrchardCore",
            pattern: "Home/Index",
            defaults: new { controller = "Home", action = "Index" }
        );
    }
}
```

`Greetings.OrchardCore/Controllers/HomeController.cs` (replaces the generated file, whose `using` lines repeat the
implicit usings; this is what the slide of the controller shows, with its using and namespace added):

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Greetings.OrchardCore.Controllers;

public sealed class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
```

Do not add `Dependencies` to the manifest: the slide that shows it is an optional property, and the recording shows
a module with no dependency badge.

**Build the module inside the checkout.** The checkout is one commit of the repository, so no released `OrchardCore.*`
package matches it, and the package references of the generated project would load a second copy of Orchard Core next
to the checkout's projects. The checkout targets net11.0 (`src/OrchardCore.Build/TargetFrameworks.props:7,11`), so
install the .NET SDK of that version. Then:

1. Move the generated folder to `<checkout>/src/OrchardCore.Modules/Greetings.OrchardCore/`. The
   `Directory.Build.props` and `Directory.Build.targets` of `src/OrchardCore.Modules` import the Module Targets into
   every project under that folder, which is how `src/OrchardCore.Modules/OrchardCore.Features/` becomes a module.
2. Replace `Greetings.OrchardCore.csproj` with the following. It keeps the Razor SDK, takes the target framework
   from those `Directory.Build.props`, and refers to the projects the package references stood for (the other two
   package references of the generated file belong to the sample part):

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Razor">

     <PropertyGroup>
       <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
     </PropertyGroup>

     <ItemGroup>
       <FrameworkReference Include="Microsoft.AspNetCore.App" />
     </ItemGroup>

     <ItemGroup>
       <ProjectReference Include="..\..\OrchardCore\OrchardCore.DisplayManagement\OrchardCore.DisplayManagement.csproj" />
       <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
       <ProjectReference Include="..\..\OrchardCore\OrchardCore.ResourceManagement\OrchardCore.ResourceManagement.csproj" />
     </ItemGroup>

   </Project>
   ```

3. Reference the module from the host. This is the step the video shows on slides in the chapter "Install the
   module"; the host is the checkout's web project:

   ```bash
   dotnet add <checkout>/src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj reference <checkout>/src/OrchardCore.Modules/Greetings.OrchardCore/Greetings.OrchardCore.csproj
   ```

   The host's `Application.Targets` import records the module in the host's assembly at build time
   (`src/OrchardCore/OrchardCore.Application.Targets/OrchardCore.Application.Targets.targets`, target
   `ResolveModuleProjectReferences`), which is why the Features page lists it afterwards.

No clip shows the project file. What could not be run is listed in question 2 of `OPEN-QUESTIONS.md`.

**Run the host** on a free port with its own empty `App_Data` outside both repositories, provisioned by Auto Setup
(the host calls `AddSetupFeatures("OrchardCore.AutoSetup")` in `src/OrchardCore.Cms.Web/Program.cs`), so no setup or
sign-in form is ever recorded:

| Variable | Value |
|---|---|
| `ORCHARD_APP_DATA` | an empty folder outside both repositories |
| `ASPNETCORE_URLS` | `http://localhost:5010` (and `VIDEO_BASE_URL` for the recorder) |
| `ASPNETCORE_ENVIRONMENT` | `Development` (the environment of the host's own launch profiles; `appsettings.Development.json` only raises the log levels) |
| `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
| `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
| `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
| `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
| `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
| `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same as `VIDEO_PASSWORD`) |
| `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
| `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |
| `HTTP_PROXY`, `HTTPS_PROXY` | a dead address such as `http://127.0.0.1:9`, so the site never calls out |

Restore and the recorder's own build need the network, and they honour the two proxy variables. So build the host
first in a shell where those two variables are not set, then start it in a shell where they are set with the other
variables of the table. Switch the launch profile off, so the profile's own addresses (`Properties/launchSettings.json`
of the host) do not replace `ASPNETCORE_URLS`:

```bash
dotnet build <checkout>/src/OrchardCore.Cms.Web
dotnet run --project <checkout>/src/OrchardCore.Cms.Web --no-build --no-launch-profile
```

Run `dotnet add` (above), the recorder's build and `Recorder setup` in a shell without the proxy variables.

- Tenant: the `Default` tenant, the only one.
- Features to enable before recording: none by hand. The Blog recipe's features are enough. The feature
  `Greetings.OrchardCore` must be **disabled** when the recording starts.
- Settings and external services: none.
- Seed data: none. The only invented names are in the manifest above (Jamie Chen, `https://example.com`); they appear
  on slides, never in a recording.
- Sign in once outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`
  saves `state.json` (git-ignored). The recorder then opens pages already signed in.

## 2. Order and snapshots

1. Start the host with an empty `App_Data` and let Auto Setup finish (open `http://localhost:5010/` once and wait
   for the Blog home page). Run `Recorder setup`.
2. Stop the host. **Copy `App_Data` and `state.json` aside**: this is the snapshot "before 05". Start the host again.
3. Record `05-enable`. It changes the site: it enables the feature `Greetings.OrchardCore`.
4. Record `06-run`. It depends on 05 (the feature must be enabled) and changes nothing.

To record everything again, stop the host, restore the snapshot, start the host, and record 05 then 06. To record
only 06 again, leave the feature enabled. To record only 05 again, restore the snapshot first.

## 3. Clips

### `05-enable`: Tools, Features

Starts on `/Admin` (the dashboard), signed in. Before `StartCaptureAsync`, wait for `#left-nav` to be visible and
make sure the **Tools** group is collapsed: the admin menu restores the groups the user last left open, so if Tools
is open, click its button once to close it. Start the capture on the dashboard.

- `enable-1`: Click the button `#left-nav button` with the text `Tools` (pause 600 ms). Click
  `#left-nav ul[data-title="Tools"] a[href$="/Features"]` with `ClickAndWaitForNavigationAsync`. Wait until
  `#search-box` is visible and `#list-summary` reads "Showing V of T features" (the script of the page applies the
  filters when it loads, and the Visibility filter starts with "Hide on demand" and "Hide always enabled" selected, so
  V is lower than T: do not wait for V to equal T; `Assets/ts/features-index.ts:87-134,218`,
  `Views/Admin/Features.cshtml:65-66`). On screen when the narration ends:
  the Features page, titled Features in the breadcrumb, with the Search box, the Visibility and Status filters, and
  the list grouped by category headings. Labels: "Tools" and "Features" from
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the title from
  `Views/Admin/Features.cshtml:15`; the summary from `Views/Admin/Features.cshtml:57-59`; the group headings from
  `Views/Admin/Features.cshtml:88`.
- `enable-2`: `TypeAsync(#search-box, "Greetings", 70)`. Wait until `#list-summary` reads "Showing 1 of T features" (T is the total of the previous step).
  On screen: the Search box holding "Greetings", one group heading and one row (the filter runs on every key press,
  so no button is needed). The filter matches the name, the feature id and the description:
  `Drivers/FeatureEntryDisplayDriver.cs` (`data-filter-value`) and `Assets/ts/features-index.ts` (`applyFilters`).
  Label: "Search" from `Views/Admin/Features.cshtml:27`.
- `enable-3`: `HighlightAsync` on the group heading `.feature-group h3` with the text `Samples`, hold about 3 s, then
  `ClearHighlightsAsync` and `HighlightAsync` on the row `[data-filter-value*="Greetings.OrchardCore"]`. Clear the
  highlights before the step ends. On screen when the narration ends: the heading "Samples"; the row with the name
  "Greetings", the feature id `Greetings.OrchardCore` beside it, the description "Adds a greeting page to the site.",
  the badge "Disabled" and the "Enable" button. No dependency badge. Labels: name and id from
  `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; the badge from `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`;
  the button from `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the group from the manifest's `Category`.
- `enable-4`: `ClickAndWaitForNavigationAsync` on `[data-filter-value*="Greetings.OrchardCore"] a` with the text
  `Enable`. The link posts the form and the site redirects to the Features page; enabling restarts the tenant, so
  allow a few seconds and wait until a toast `.toast.message-success` is visible. On screen when the narration ends:
  the Features page loaded again (the Search box is empty and the list is back to its default filters) with the green message at the
  top right, which reads "The feature Greetings was enabled." The message hides itself after 5 seconds
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`): the narration of this step must still be
  playing while it is visible, so click as soon as the step starts: the toast shows for 5 seconds after the page has loaded, which covers the end of the narration. Source of the text:
  `Controllers/AdminController.cs:317` (question 5 of `OPEN-QUESTIONS.md`).
- `enable-5`: `TypeAsync(#search-box, "Greetings", 70)` again. On screen when the narration ends: one row with the
  badge "Enabled" (a green check) and a red "Disable" button. Do not click "Disable". Labels:
  `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` and `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`.
- `enable-6`: No action. Hold the same row on screen with `HighlightAsync` on its "Enabled" badge, and clear it
  before the step ends. On screen: the same row and badge. The statement narrated is about
  `Controllers/AdminController.cs` (`ExecuteAsync` acts on the current tenant's `IShellFeaturesManager` when no
  other tenant is given).

### `06-run`: the page of the module

Depends on `05-enable`. Starts on `/` (the Blog home page of the front end), signed in.

- `run-1`: No action for the first 6.5 seconds (the narration describes the Startup class and the route). Then
  `NavigateAsync("/Home/Index")`, which the narration announces in its last words. On screen when the narration ends:
  the page of the module, loaded. The route is mapped by the `Startup` class of the generated module (template
  `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module/Startup.cs:38-43`, with the
  area renamed to `Greetings.OrchardCore`), and the controller by `Controllers/HomeController.cs:12-14`. If the page is
  not loaded when the narration ends, start the navigation earlier.
- `run-2`: `HighlightAsync` on `h1` with the text `Hello from the Greetings module`; clear it before the step ends.
  On screen when the narration ends: the heading "Hello from the Greetings module" inside the layout of the active
  theme (its header and footer around it), not inside the admin layout. The layout comes from
  `src/OrchardCore/OrchardCore.DisplayManagement/Theming/ThemeViewStart.cs`; the heading from the view edit above.

## 4. Slides (no recording)

One line each; `slides.py` draws them.

- `what-1`, `what-2`, `what-3`: three cards (A project, A manifest, Features), the one the narration talks about
  bright. `what-4`: the three cards, all bright.
- `create-1`: the terminal panel with `dotnet new install OrchardCore.ProjectTemplates` bright. `create-2`: the same
  panel with `dotnet new ocmodulecms -n Greetings.OrchardCore` bright, and two cards about the template and `-n`.
- `create-3`: four cards, one for each part of the generated project. `create-4`: the project file, with the Razor
  SDK line and the `OrchardCore.Module.Targets` reference bright. `create-5`: `Startup.cs`, with the class line and
  the `Configure` method bright. `create-6`: `HomeController.cs` above the one-line `Index.cshtml`.
- `manifest-1` to `manifest-3`: `Manifest.cs` with the properties the narration names bright, and cards on the
  right. `manifest-4`: the manifest with the optional `Dependencies` line. `manifest-5`: the manifest again, with a
  card about the project file.
- `install-1`: the terminal command for the CMS web application template, and its package reference. `install-2`:
  `Program.cs` with `AddOrchardCms` and `UseOrchardCore` bright. `install-3`: the `ProjectReference` item and two
  cards. `install-4`: three cards (the module project, the web project builds, the site starts). `install-5`: the
  `dotnet run` command and two cards.
- `recipe-1`: a recipe with one `Feature` step enabling `Greetings.OrchardCore`, and two cards.
- `missing-1`: three cards, the first two bright (the package reference, the project reference). `missing-2`: the
  same cards, the third bright (build and run again).

## 5. Checks for private data

Before anything is published, OCR the two recordings at one frame per second and at every scene change:

- The recordings show only the admin menu, the Features page (Orchard Core's own features), a toast, and the front
  end of the Blog recipe. No real name, email address, phone number or token should appear; the only email address
  that exists is `admin@example.com`, and it is not displayed.
- The address bar is not part of the capture. No folder name, user name or machine name of the recorder's computer
  may appear anywhere (the slides show none).
- `<password>` is never typed in a clip: the session comes from `state.json`, which stays out of git.
- Delete the work folder, the checkout copy, the snapshot and `state.json` when the recording is accepted.
