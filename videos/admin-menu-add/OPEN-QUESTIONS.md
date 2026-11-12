# Open questions: Add Admin Menu Entries to Your Module

Every statement of the narration was read from the Orchard Core source (commit 92db4a83) or from the documentation
page `src/docs/guides/add-admin-menu/README.md`. Nothing was run, so these points need a person with a running site.

## 1. Does a module made with the template build as is?

The documentation page tells the reader to run `dotnet new ocmodulecms -n MyModule`. In
`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module/Startup.cs` the `using` lines for
the `Drivers`, `Handlers`, `Models` and `Settings` namespaces are outside the `#if (AddPart)` blocks, while
`.template.config.src/template.json` excludes the files of those namespaces unless `AddPart` is true.

- **Checked:** the two files only.
- **To confirm:** create a module with the default options and build it. If it fails, the guide's first steps need
  `--AddPart true`, or the template's `Startup.cs` needs the usings inside the conditional block.
- **Effect on the video:** none. The narration does not show the command ("a separate video covers creating a module"),
  and `recording-plan.md` builds the module from its own files.

## 2. Does the code of the video compile as shown?

The narration says the module references the `OrchardCore.Admin` package, as the guide does. The types used by the code
come from two assemblies: `INavigationProvider`, `NavigationBuilder` and `PrefixPosition` from
`src/OrchardCore/OrchardCore.Navigation.Core`, and the `[Admin]` attribute from
`src/OrchardCore/OrchardCore.Admin.Abstractions`. `src/OrchardCore.Modules/OrchardCore.Admin/OrchardCore.Admin.csproj`
references both projects (and `OrchardCore.Navigation.Core` references `OrchardCore.Admin.Abstractions`), so a package
reference to `OrchardCore.Admin` reaches them. Neither is the only way in: `OrchardCore.Navigation.Core` alone gives
both, which is why the narration says the module references the package rather than that it needs it.

- **Checked:** the project references, and that the code of the slides uses only those types plus `Microsoft.AspNetCore.Mvc`,
  `Microsoft.Extensions.Localization`, `Microsoft.Extensions.DependencyInjection` and `OrchardCore.Modules`.
- **To confirm:** that a module referencing only the `OrchardCore.Admin` package from NuGet compiles the code of the
  video (the package's dependency list was not available), and that `dotnet build` of the scratch copy raises no error
  or warning for the module. The `.editorconfig` and `Directory.Build.props` files of the checkout were not in the
  source copy that was read, so it is not known whether unnecessary `using` lines (the guide has `using System;` and
  `using System.Threading.Tasks;`) are reported. The module project sets `ImplicitUsings` to `disable` so that every
  `using` of the slides is needed; if the build still complains, fix the project, never the code on the slides. The
  `ocmodulecms` template that the guide uses has `ImplicitUsings` on, so this choice is only for the scratch copy.

## 3. The place of My Root View among the top-level items

The narration says My Root View is above Tools and Settings. That comes from the order of positions
(`FlatPositionComparer.cs`: a text position such as `am-My Root View` sorts before `end.50` and `end.100`, the
positions of Tools and Settings in `OrchardCore.Settings/AdminMenu.cs`). The exact slot among the other top-level items
was not worked out.

- **To confirm (`admin-3`):** on the Blog site, My Root View is listed above Tools and Settings, and the narration still
  matches what is on screen.

## 4. The spoken forms in `storyboard.json`

The `spoken` map adds spellings that no person has heard yet: `INavigationProvider`, `AdminNavigationProvider`,
`AddNavigationProvider`, `BuildNavigationAsync`, `BuildAsync`, `PrefixPosition`, `IStringLocalizer`, `AdminMenu`,
`DemoNav`, `ChildOne`, `ChildTwo`, `MyModule` and `OrchardCore.Admin` ("Orchard Core dot Admin").

- **To confirm:** listen to each with `scripts/pronounce.py` before narrating. Also check that the voice reads "admin"
  (the name of the menu, step `provider-2`) and "end" and "start" (step `position-3`) as the words.

## 5. The recording of `05-admin`

- **Toast.** The success toast "The feature MyModule was enabled." hides itself after 5 seconds
  (`NotifierExtensions.cs`: `SuccessAsync` uses 5000 ms). The plan relies on the pointer over the toast pausing the timer,
  which is Bootstrap's behavior and was not checked in this theme. If the toast is gone when the narration of `admin-2`
  ends, the click on Enable has to happen later in the step.
- **Restart.** Enabling the feature restarts the tenant; how long the redirect takes on the recording machine decides
  whether `OffCameraAsync` is needed.
- **The pages.** `View One` and `View Two` are rendered by the admin layout with no title or breadcrumb, since the views
  hold only a paragraph. Confirm that the contact sheet shows the layout, the left menu and the selected item.
- **The menu.** At 1536 pixels the sidebar is expected to be expanded (not compact). Confirm that a click on
  `My Root View` (a button, because it has no page of its own) opens it and closes Tools, as the focused behavior
  describes (`sidebar.ts`, `NavigationItemLink-admin.cshtml`).
