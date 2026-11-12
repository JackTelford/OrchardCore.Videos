# Open questions: Create a Theme

What was read from the source and the documentation, but not run, and what a person must confirm on a running site or
a clean machine before the video is recorded or published. Orchard Core source commit `92db4a837d`.

## Pronunciation (new spellings, not yet heard)

`storyboard.json` has a `spoken` map. Each entry is a new spelling that `scripts/pronounce.py` must confirm before
narrating (the content was written without running any script that produces audio):

- `dotnet` spoken as "dot net"; `octheme` as "O.C. theme"; `AddLiquid` as "Add Liquid".
- `MyTheme.OrchardCore` as "My Theme dot Orchard Core"; `MyTheme` as "My Theme".
- `Manifest.cs` as "Manifest dot C.S."; `Layout.liquid` as "Layout dot liquid"; `Layout.cshtml` as
  "Layout dot C.S.H.T.M.L."; `Theme.png` as "Theme dot P.N.G."; `wwwroot` as "W.W.W. root".
- `render_body`, `render_section` and `page_title` as the same words without the underscore.
- Words left to the voice, to listen for: "SDK" (read as S.D.K.?), "jQuery", "Razor", "Liquid".

## Commands on the slides

They come from `getting-started/theme.md` and `getting-started/templates/README.md`, adapted. None was run.

- `dotnet new install OrchardCore.ProjectTemplates`: the documentation pins a version (`@...`); the slide leaves it
  out, so that it stays true. Confirm on a clean machine that it installs a stable release, and that
  `dotnet new list` then shows `occms`, `ocmvc`, `ocmodulecms`, `ocmodulemvc` and `octheme` (the five templates under
  `OrchardCore.ProjectTemplates/content`; the slide groups them as web applications, modules and theme).
- `dotnet new octheme -n "MyTheme.OrchardCore"`: from `OrchardCore.Templates.Theme/.template.config.src/template.json`
  (`shortName` `octheme`, `preferNameDirectory` true, `sourceName` replaced in file names and contents). Confirm that it
  creates the folder `MyTheme.OrchardCore` holding `MyTheme.OrchardCore.csproj`, `Manifest.cs` and
  `Views/Layout.liquid`, and nothing else, with `Name` and `Description` of the manifest set to the name given.
- `--AddLiquid false`: the template excludes `Views/Layout.liquid` (`template.json`, `AddLiquid`, default true). The
  template has three options (`Framework`, `AddLiquid`, `OrchardVersion`), so `create-4` says that AddLiquid is one of
  them. Confirm the spelling of the option on the command line, and in `dotnet new octheme --help`.
- `dotnet sln add ...` and `dotnet add ... reference ...` with the paths shown, run from the folder that holds the
  solution, with a web application named `Cms.Web` (the name `getting-started/README.md` uses). `theme.md` runs them
  from inside the project folders, with the names `OrchardCore.Cms.Web` and `OrchardCore.Themes` of the source
  repository; the slide shows the form for a solution of the Getting started page.

## Claims read from the build files, not built

- The Theme Targets package marks the project as a theme (`ModuleType` `Theme`) and embeds views and static files in
  the assembly (`OrchardCore.Theme.Targets.props`, `OrchardCore.Module.Targets.props` and `.targets`).
- A theme with `.cshtml` views and a project that does not use the Razor SDK fails with error OC2001
  (`OrchardCore.Module.Targets.targets`, target `EnsureRazorSdk`). Confirm by building a generated theme with the SDK
  changed to `Microsoft.NET.Sdk` and a `Layout.cshtml` added.
- `Layout.cshtml` used in place of `Layout.liquid` in a generated theme (next-1). The template does not generate a
  `Views/_ViewImports.cshtml`, and the members a layout uses (`RenderBodyAsync`, `RenderTitleSegments`, `Site`,
  `Orchard`) belong to `OrchardCore.DisplayManagement.Razor.RazorPage<TModel>` (`RazorPage.cs`), which the built-in Razor
  themes select with `@inherits` in their `Views/_ViewImports.cshtml` (`SafeMode`, `TheTheme`, `TheAdmin`). No default
  imports are added elsewhere in the source. Confirm by adding `Layout.cshtml` and that one-line `_ViewImports.cshtml`
  to a generated theme (and removing `Layout.liquid`), then loading the front end. A layout that uses the
  `<resources>` tags also needs the `@addTagHelper` lines of `TheTheme/Views/_ViewImports.cshtml`: confirm that too
  before saying more than the narration says. The narration of next-1 says only that the built-in Razor themes keep such
  a file in Views and what it does for the layout; it does not claim that a generated theme fails without it.
- The identifier of the theme is the name of the assembly, which is the project name (`Module.cs`: the logical name is
  the `Id` of the manifest, else the assembly name; the manifest sets no `Id`). Confirm that `MyTheme.OrchardCore` is
  what the thumbnail URL and `SetCurrentTheme` use (`/MyTheme.OrchardCore/Theme.png` answers with the picture).
- Files of `wwwroot` are served under the identifier of the theme (manifest-3 and manifest-4): read from
  `ThemeEntry-Thumbnail.SummaryAdmin.cshtml:6`, `ModuleEmbeddedStaticFileProvider.cs:38-47` (the first segment of the
  URL is the module id, the rest is the path under `wwwroot`) and `OrchardCore.Module.Targets.props:24` (every file of
  the project is embedded, except the patterns of `ModuleDefaultAssetExcludes`, such as `*.props`, `*.targets`, `*.cs`
  and `*.resx`, which is why `manifest-4` says "files" and does not claim every file). Confirm with a file other than
  `Theme.png`, for example `wwwroot/css/site.css` at `/MyTheme.OrchardCore/css/site.css`.
- The layout of the template requests jQuery from a CDN (`use_cdn:"true"`, `Layout.liquid`), so the browser that
  records `apply-5` fetches a script from a host outside this machine. Confirm that the front end loads with or without
  a network, and that the wait for the load state `NetworkIdle` ends.

## On the running site (clip `05-apply`)

- The labels were read at commit `92db4a837d`, not from the release that viewers install. Confirm on the recording
  build, and on a release, that these read the same: `Design`, `Themes`, `Make Current`, `Enable`,
  `This is the current Site theme`, `MyTheme was enabled`, `MyTheme was set as the default Site theme`.
- Both success messages survive the redirect that follows `Make Current`, although enabling the feature restarts the
  tenant (`AdminController.cs:135-148`). They last 5 seconds (`NotifierExtensions.cs:71`). Confirm that both appear, in
  this order, and how long the page takes to come back.
- The narration says that the current themes are listed first (`AdminController.cs:92`, `OrderByDescending`) and that
  the card of MyTheme reads `This is the current Site theme`. Confirm on screen, with the admin theme also current.
- The narration of `apply-5` says only that the pages are rendered by the layout of MyTheme, which has no stylesheet of
  its own. Confirm what the Blog home page looks like in that layout (the content as plain markup? no menu? any
  error). If it does not render, change the step; do not describe an appearance that was not seen.
- `hidden` and `Admin` tags (manifest-2): read from `AdminController.cs:67-72` and `:231-234`, the only code that reads
  the tags of a theme manifest (the narration says that the two have a meaning on the Themes page, and no more).
  Confirm that a theme tagged `hidden` is absent from the page, and that the tag `Starter` is shown on the card.
- The card of a theme that is not enabled shows `Make Current` and `Enable`
  (`ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:15-16`), and `Enable` turns the theme on without making it
  current (`AdminController.cs`, action `Enable`). Confirm before recording `apply-3`.
- The thumbnail: confirm that the green `Theme.png` of the recording-plan shows as the picture of the card, and does not
  look like the card of another theme.
