# Recording plan: Add Admin Menu Entries to Your Module

The video has six slide clips and one recording (`05-admin`). The recorder is only needed for `05-admin`; the slides are
drawn by `slides.py`. Everything below is meant to be followed without deciding anything. A selector in this plan stands
for `scene.Page.Locator(...)`.

## The site it needs

The subject is a module that the video writes, so the site is an Orchard Core checkout plus that module. Use a
scratch copy of `OrchardCMS/OrchardCore` (the commit the video documents, or the branch being recorded), never a working
copy, and delete it afterwards.

1. **Module.** Create the folder `src/OrchardCore.Modules/MyModule/` in the scratch copy, with the files under
   "The module's files" below. The project name and assembly name must be exactly `MyModule`: the menu items point at
   the area `MyModule`.
2. **Host.** In `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`, add to the group that holds the other project
   references:

   ```xml
   <ProjectReference Include="..\OrchardCore.Modules\MyModule\MyModule.csproj" />
   ```

   `OrchardCore.Cms.Web` already enables Auto Setup in its `Program.cs`, so no setup form is recorded. (A throwaway host
   built from the project templates also works, but the templates add Auto Setup only when
   `.AddSetupFeatures("OrchardCore.AutoSetup")` follows `.AddOrchardCms()`, and the package versions have to match the
   checkout. The scratch copy avoids both.)
3. **Run it** on `http://localhost:5010`, with its own data folder outside both repositories, and no outbound calls:

   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   export ORCHARD_APP_DATA=<an empty folder outside both repositories>
   export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
   export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
   export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
   export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
   export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
   export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
   export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
   export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
   export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
   dotnet run --project src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj --urls http://localhost:5010
   ```

   `VIDEO_PASSWORD` is chosen outside git, and the same value is used by `Recorder setup`. Write `<password>` in any
   note; never a real or sample password. Open `http://localhost:5010/` once so Auto Setup runs (the site name is
   **Orchard Core**, the user is `admin`, the email is `admin@example.com`).
4. **Features.** Enable nothing else. `MyModule` is built into the site but stays **disabled**: the clip enables it on
   camera. The Blog recipe's own sample content is not shown by this video.
5. **Admin settings.** Leave them at their defaults. The menu behavior is **Focused** (the default of "Navigation menu
   behavior"): only the section of the current page is open, and opening a section closes the others. The sidebar is
   not compact at 1536 pixels wide. If the left menu is not visible at the start of the clip, stop and fix that first.
6. **Sign in once, outside the capture**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`. It writes
   `state.json` (never committed).

There is no seed data and no mask script: the site is made from scratch, and the only names on screen are `admin`,
`admin@example.com` and the module's.

### The module's files

All names are fake and the file contents are exactly what the slides show. Create them as text.

`MyModule.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Navigation.Core\OrchardCore.Navigation.Core.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Admin.Abstractions\OrchardCore.Admin.Abstractions.csproj" />
  </ItemGroup>

</Project>
```

This is the pattern of `src/OrchardCore.Modules/OrchardCore.Cors/OrchardCore.Cors.csproj`. The video's text says the
module references the `OrchardCore.Admin` package, which is what a module outside the repository does; inside the
repository the three project references above stand for it (`OrchardCore.Admin.csproj` references the same two
projects, `OrchardCore.Navigation.Core` and `OrchardCore.Admin.Abstractions`).

`ImplicitUsings` is set to `disable` on purpose: `src/OrchardCore.Build/OrchardCore.Commons.props` turns it on for every
project of the checkout, and the slides show every `using` line, including `using System;` and
`using System.Threading.Tasks;`. With implicit usings on, those two lines are redundant, and a build that reports
unnecessary usings as errors could refuse the code of the guide (this was not checked: see `OPEN-QUESTIONS.md`,
question 2). With the setting off, each `using` of the slides is needed, so the code is expected to compile as shown.
Nobody has run the build: do it before recording. (The `ocmodulecms` template that the guide uses keeps
`ImplicitUsings` on; the guide's readers are not affected by this choice.)

`Manifest.cs`:

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "MyModule",
    Author = "Example Author",
    Website = "https://example.com",
    Version = "0.0.1",
    Description = "Adds menu items to the admin menu.",
    Category = "Content Management"
)]
```

`Startup.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
    }
}
```

`AdminMenu.cs` (from the documentation page `src/docs/guides/add-admin-menu/README.md`):

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        // We want to add our menus to the "admin" menu only.
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["My Root View"], S["My Root View"].PrefixPosition(), rootView => rootView
                .Add(S["Child One"], S["Child One"].PrefixPosition(), childOne => childOne
                    .Action("ChildOne", "DemoNav", new { area = "MyModule" }))
                .Add(S["Child Two"], S["Child Two"].PrefixPosition(), childTwo => childTwo
                    .Action("ChildTwo", "DemoNav", new { area = "MyModule" })));

        return ValueTask.CompletedTask;
    }
}
```

`Controllers/DemoNavController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace MyModule.Controllers;

[Admin]
public sealed class DemoNavController : Controller
{
    public ActionResult ChildOne()
    {
        return View();
    }

    public ActionResult ChildTwo()
    {
        return View();
    }
}
```

`Views/DemoNav/ChildOne.cshtml` holds `<p>View One</p>`, and `Views/DemoNav/ChildTwo.cshtml` holds `<p>View Two</p>`.

Build once (`dotnet build src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`) and fix any error before recording; the
video shows this code as it is, so the module must compile without changes. The addresses of the two pages are
`/Admin/MyModule/DemoNav/ChildOne` and `/Admin/MyModule/DemoNav/ChildTwo`: once the snapshot of "Order of the clips"
exists, enable the module in a trial run, check both addresses in the browser (not captured), then restore the
snapshot.

The feature is named after the module's manifest: `Name = "MyModule"` is what the Features page lists, what the search
box matches and what the toast prints ("The feature MyModule was enabled."), and the feature id is the assembly name
`MyModule`. A different `Name` would change the words on screen and the narration of `admin-2` would no longer match
(`FeaturesProvider.cs` builds a module's only feature from `extensionInfo.Id` and `manifestInfo.Name`; the toast text is
`AdminController.cs:317`).

## Order of the clips, and what changes the site

Only `05-admin` is recorded. It **changes the site**: it enables the feature `MyModule`.

1. Run the site once so that Auto Setup completes, run `Recorder setup`, and stop the site.
2. Copy the data folder (`ORCHARD_APP_DATA`) and `state.json` aside. This is the snapshot: module built in, module
   disabled, nobody has used the admin menu.
3. Start the site on the snapshot, and record `05-admin`.
4. For another take, stop the site, restore the snapshot (data folder and `state.json`), and start it again. Never
   record from a site where `MyModule` is already enabled: `admin-2` has no Enable button then.

The menu's open sections are kept in the browser (session storage and the admin preferences), not on the site, so a
restored `state.json` from `Recorder setup` is enough to start every take with a closed menu.

## Slide clips

The slides carry no recording. Each line says what the slide shows when its narration ends.

- `build-1`: the heading "A root item with two children" and three cards: My Root View (top-level, no page of its
  own), Child One and Child Two (each opens an action of the DemoNav controller).
- `build-2`: the heading "What the module needs" and three cards: a site, a module, and the OrchardCore.Admin package.
- `pages-1`: the controller `DemoNavController.cs` with the `[Admin]` line and the class line bright, and two cards,
  "Admin theme" and "Access admin panel".
- `pages-2`: the same controller with the two actions bright, and two cards with the addresses
  `/Admin/MyModule/DemoNav/ChildOne` and `/Admin/MyModule/DemoNav/ChildTwo`.
- `pages-3`: the two views side by side, `View One` and `View Two`, with the line saying the files are in the
  `Views/DemoNav` folder.
- `provider-1`: `AdminMenu.cs` with the usings, the class that implements `INavigationProvider`, and the constructor
  that takes the localizer.
- `provider-2`: the method `BuildNavigationAsync` with the check of the menu name bright.
- `provider-3`: the same method with the `builder` chain of three `Add` calls bright.
- `provider-4`: the same method with the root `Add` line and the two `Action` lines bright.
- `position-1`: the four position cards of "The order of positions", with "Numbers" and "before and after" bright.
- `position-2`: the same cards, with "PrefixPosition()" bright.
- `position-3`: the same cards, with "start and end" bright (Tools is end.50, Settings is end.100).
- `register-1`: `Startup.cs` with the line `services.AddNavigationProvider<AdminMenu>();` bright.
- `beyond-1`: the `AdminMenu` of the Background Tasks module, with the class line (`AdminNavigationProvider`) and the
  `BuildAsync` line bright.
- `beyond-2`: the same code, with the three lines that add Background Tasks under Tools bright.
- `beyond-3`: the same code, with the `.Permission(...)` line bright.

## Clip `05-admin`

**Starts on** `/Admin` (the admin home), opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync`. The left
menu is visible and every section is closed, as the menu shows nothing open at the admin root. The browser is signed in
as `admin` through `state.json`. `MyModule` is disabled.

The menu items of this clip are in the sidebar `#left-nav`. A top-level item that has children and no page of its own
(Tools, My Root View) is a `button.nav-group-label`; an item that has a page is an `a.item-label`. Match the exact text
of the label, not a part of it.

- `admin-1` (narration: "In the admin, open Tools, then Features, ..."):
  1. Click the button `#left-nav button.nav-group-label` whose text is exactly `Tools`. The section opens and shows the
     items under Tools. Pause 700 ms.
  2. `ClickAndWaitForNavigationAsync` on the link `#left-nav a.item-label` whose text is exactly `Features`. Wait for
     `#search-box` to be visible.
  
  On screen when the narration ends: the Features page (title "Features"), the search box (placeholder "Search"), the
  summary "Showing N of M features" (N can be smaller than M, because the page starts with the filters "Hide on demand" and
  "Hide always enabled" on: `Features.cshtml:65` and `:66`, applied by `features-index.ts` when the page loads), and
  the list grouped by category; in the menu, Tools is open and Features is selected. Labels: `Tools` in `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:32` and
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`, `Features` in
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:27`, the page title in
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:5`; the button or link kind in
  `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24`.

- `admin-2` (narration: "Search for MyModule, and select Enable. A message confirms ..."):
  1. `TypeAsync` into `#search-box` the text `MyModule`. Wait until `#list-summary` reads "Showing 1 of" followed by the
     total (the list is then reduced to the one row, named MyModule, with the id `MyModule` beside the name and the
     badge "Disabled"). Pause 600 ms.
  2. `ClickAndWaitForNavigationAsync` on `#btn-enable-MyModule` (the link labeled `Enable` in the row of MyModule; the
     id is `btn-enable-` plus the feature id). The link is posted by script, so use the navigation variant. Enabling a
     feature restarts the tenant, which can take several seconds: wrap the wait for the redirected page in
     `scene.OffCameraAsync`, with the condition "the toast `.toast.message-success` containing `The feature MyModule was
     enabled.` is visible", so the pause is cut from the video.
  3. Move the cursor onto the toast (`MoveToAsync`). The toast hides itself 5 seconds after the page loads; the pointer
     over it should pause that timer (confirm on the contact sheet). If the toast is gone when the narration ends,
     start the click later in the step and shorten the pauses until the toast is visible at the end.

  On screen when the narration ends: the Features page after the redirect, with the green toast "The feature MyModule
  was enabled." at the top right, and the admin menu with the new top-level item My Root View among the items (the
  menu is closed except for Tools, which holds the current page). Labels: `Enable` in
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, the message in
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`, the search box in
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`, the summary in
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:56`.

- `admin-3` (narration: "My Root View is in the admin menu, above Tools and Settings. ..."):
  1. If the toast is still on screen, leave it; it hides itself.
  2. `HighlightAsync` on the top-level item whose text is exactly `My Root View` (the `button.nav-group-label` in
     `#left-nav`), with no caption. Pause 800 ms. The item must be above Tools and Settings, which sort last (`end.50` and
     `end.100`).
  3. `ClearHighlightsAsync`, then click that button. The menu opens it and, being focused, closes Tools. The two
     children, `Child One` and `Child Two`, appear under it.

  On screen when the narration ends: My Root View open, with Child One and Child Two under it, above the collapsed
  Tools and Settings. Labels: `My Root View`, `Child One` and `Child Two` in the `AdminMenu.cs` of the module (the
  file above); the position order in `src/OrchardCore/OrchardCore.DisplayManagement/Zones/FlatPositionComparer.cs:7`,
  and `end.50` and `end.100` in `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:32` and `:36`; a parent with
  no page toggles its children in `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24`. The
  root has no `Action`, so `NavigationManager.cs:185-188` gives it the address `#`. `MenuItem.LinkToFirstChild` is true
  by default (`MenuItem.cs:19`), but no `.cs`, `.cshtml`, `.liquid` or `.ts` file of the checkout reads it to link a
  parent to its first child: `NavigationHelper.cs:67` only copies it to the shape and `NavigationManager.cs:103` and
  `:127` only merge it. If the root renders as a link to Child One on the running site, the narration of `provider-4`
  and `admin-3` is wrong: stop and report it.

- `admin-4` (narration: "Select Child One. The page opens in the admin theme with the paragraph View One, ..."):
  1. `ClickAndWaitForNavigationAsync` on the link `#left-nav a.item-label` whose text is exactly `Child One`.
  2. Wait for the paragraph `p` whose text is `View One` to be visible. Move the cursor away from the menu
     (`MoveToAsync` to the paragraph).

  On screen when the narration ends: the admin layout (the admin theme, with the left menu and the top bar) at
  `/Admin/MyModule/DemoNav/ChildOne`, the text "View One" in the content area, and in the menu My Root View open with
  Child One selected (highlighted as the current item). Labels: `Child One` in the module's `AdminMenu.cs`; the
  paragraph in `Views/DemoNav/ChildOne.cshtml`; the selection in
  `src/OrchardCore/OrchardCore.Navigation.Core/NavigationHelper.cs:91` and the `current` class in
  `src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml:63`.

- `admin-5` (narration: "Select Child Two, and the second view opens the same way, ..."):
  1. `ClickAndWaitForNavigationAsync` on the link `#left-nav a.item-label` whose text is exactly `Child Two`.
  2. Wait for the paragraph `p` whose text is `View Two` to be visible.

  On screen when the narration ends: the admin layout at `/Admin/MyModule/DemoNav/ChildTwo`, the text "View Two", and
  Child Two selected under My Root View. Labels: `Child Two` in the module's `AdminMenu.cs`; the paragraph in
  `Views/DemoNav/ChildTwo.cshtml`.

Keep every step's actions shorter than its narration; the pauses above are the part to shorten if a step is too long.

## Checks for private data

- The only account is `admin` with `admin@example.com`. No password appears on screen: do not open the user menu.
- The data folder, `state.json` and the scratch copy of the checkout are not in either repository.
- The password is read from `VIDEO_PASSWORD` and is never written in a file, a note or a command line saved in history.
- Look at every contact sheet for the module's author or website: the Features page does not show them, but if it does,
  they must read `Example Author` and `https://example.com`.
- OCR the finished video as the skill describes (one frame per second, at every scene change, and five frames per
  second) and search for real names, emails, phone numbers and tokens before publishing.
