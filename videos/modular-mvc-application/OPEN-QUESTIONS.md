# Open questions: modular-mvc-application

What was checked in the Orchard Core source at commit `92db4a83` and in `src/docs/guides/create-modular-application-mvc/README.md`,
and what a person must confirm on a running site. `OC:` is a checkout of `OrchardCMS/OrchardCore`.

## 1. Which templates to install, and with which command

- **Checked:** the guide installs the templates with `dotnet new install OrchardCore.ProjectTemplates@3.0.1-*` and
  calls that the latest stable release (`README.md:19-21`). `src/docs/getting-started/templates/README.md:12` and `:18`
  uses `@3.0.1` for the stable release and `@3.0.1-*` with the preview feed for the `main` branch. The repository itself
  builds `4.0.0-preview` (`OC:src/OrchardCore.Build/OrchardCore.Commons.props:5-6`), and the templates it contains
  target a single framework (`Framework` parameter of `.template.config.src/template.json`). The slide `create-1` and the
  narration show `dotnet new install OrchardCore.ProjectTemplates` with no version, so the video stays true when
  versions change.
- **To confirm:** that this command installs templates whose generated files match the table in `recording-plan.md`
  (the layout with `MvcSandbox`, `Startup` with empty methods, the view `Hello from <project name>`), and which .NET SDK
  they need. If the released templates generate other files, record with templates built from the checkout (`dotnet pack`
  of `OC:src/Templates/OrchardCore.ProjectTemplates`, then `dotnet new install <folder>/<package>.nupkg`, with the
  Orchard Core packages of the same build restored from a local feed) and say so in the README of the video.

## 2. HTTP only on port 5010

- **Checked:** the template's `Program.cs:24` calls `app.UseHttpsRedirection()`, and its launch profile binds
  `https://localhost:5001;http://localhost:5000` (`Properties/launchSettings.json:21`). The recorder creates its browser
  context without `IgnoreHTTPSErrors` (`scripts/recorder/Scene.cs:105-112`), so an HTTPS address with the development
  certificate would fail.
- **To confirm:** that `--no-launch-profile --urls http://localhost:5010` makes the application answer over HTTP without a
  redirect to HTTPS (this is ASP.NET Core behaviour, not something in the Orchard Core source). If the browser is
  redirected anyway, stop and ask the maintainer: either trust the development certificate on the recording machine
  and record over HTTPS, or remove the redirect from the recording copy only (the slides never show that line).

## 3. Pronunciations to confirm with `scripts/pronounce.py`

The storyboard's `spoken` map holds spellings that no one has listened to yet. The map is applied key by key, in the
order written, as plain text replacement (`spoken()` in `scripts/narrate.py`), so a longer key has to come before a key
that it contains (`AddMvc` and the project names before `Mvc`). Confirm each, and fix the map if a voice reads one
wrongly:

- `Orchard Core dot M.V.C. dot Web`, `Orchard Core dot M.V.C. dot Hello World`, `Orchard Core dot Application dot
  M.V.C. dot Targets`, `Orchard Core dot Module dot Targets`, `Orchard Core dot Project Templates` (the project and
  package names; the dots are spoken as "dot").
- `Program dot C.S.`, `Manifest dot C.S.` (file names).
- `Add Orchard Core`, `Use Orchard Core`, `Add M.V.C.`, `Configure Services`, `Map Area Controller Route`, `Startup Base`,
  `Home Controller` (identifiers).
- `dot net` for `dotnet` (as in "dotnet new" and "dotnet run"), and `M.V.C.` for the `Mvc` of the template names. The
  letters "MVC" written in capitals are left to the voice.
- Also listen to ".NET SDK", "Razor SDK" and "CMS".

## 4. The default address after the module's own route (step `home-2`)

- **Checked:** `OrchardCore.Mvc.Core/Startup.cs:62-76` maps, for every controller action that has an `area` route value, the
  pattern of `DefaultAreaControllerRouteMapper` (`/{area}/{controller}/{action}/{id?}`, with the placeholders replaced,
  `DefaultAreaControllerRouteMapper.cs:9`, `:19`); that mapping does not depend on the routes the module registers in its
  own `Configure` method. So the default address keeps answering after `MapAreaControllerRoute` adds a route with an empty
  pattern.
- **To confirm:** in state B, `/OrchardCore.Mvc.HelloWorld/Home/Index` answers with the same page as `/`. If it does not,
  delete the step `home-2` (and its entry in `recording-plan.md`) and the sentence "The default address still answers"
  is gone with it.

## 5. Claims read from the source, to see once on the running application

- **The layout of the web application is used by the views of the module.** The guide says so
  (`README.md:66`, the sentence after "Hello from OrchardCore.Mvc.HelloWorld"). The template has `_ViewStart.cshtml` and
  the layout in the web application only, and none in the module. Read from the source: the application is itself one
  of the modules (`ModularApplicationContext.cs:43`); `ShellViewFeatureProvider.cs:146-151` gives the precompiled views of
  every module the path `/Areas/<module id>/...`, and `:154-166` registers the views of the application a second time
  under their own paths, so `/_ViewStart.cshtml` and `/Views/Shared/_Layout.cshtml` exist at the root. A view of the
  module is at `/Areas/OrchardCore.Mvc.HelloWorld/Views/Home/Index.cshtml`, and ASP.NET Core looks for a
  `_ViewStart.cshtml` in each folder above a view, up to the root. That lookup is framework behaviour, not Orchard Core
  code; the clip `06-run` shows it. Not traced: in the Development environment, when a `refs` folder exists beside the
  binaries, a module referenced as a project does not get its precompiled views (`ShellViewFeatureProvider.cs:95-103`),
  and the views are compiled at run time instead. Note also that the sample module of the Orchard Core repository ships
  its own `Views/_ViewStart.cshtml` that sets the same layout
  (`OC:src/OrchardCore.Modules/OrchardCore.Mvc.HelloWorld/Views/_ViewStart.cshtml`), and the template module has none:
  if the text of the module appears without the navigation bar and the footer, `web-2` and `run-3` are wrong.
- **No Setup screen, one tenant, every feature of the referenced modules enabled.** Read from
  `OC:src/OrchardCore/OrchardCore/Shell/SingleShellSettingsManager.cs` (`AsRunning()`),
  `AllFeaturesShellDescriptorManager.cs` and `ServiceCollectionExtensions.cs:197`.
- **How the module is found.** `OrchardCore.Application.Targets.targets:8-27` adds one `ModuleNameAttribute` per referenced
  module (project or package) to the application's assembly, and `AssemblyAttributeModuleNamesProvider` reads them at
  startup. `OrchardCore.Module.Targets.targets:52-90` adds `ModuleMarkerAttribute` and embeds the files of the module.
- **Routes are registered once per tenant pipeline.** `ModularTenantRouterMiddleware.cs:40-55` builds the pipeline on the
  first request, and `ShellPipelineExtensions.cs:80-101` calls `Configure` of every `IStartup` in it. The guide says to
  restart the application after adding the route.
- **The area is the identifier of the module.** `Module.cs:85-90` (`ModuleInfo.Id ?? assemblyName`) and
  `ModularApplicationModelProvider.cs:61`. The guide says "the name of the module"; see `docIssues`.

## 6. The console slide (`run-1`)

The three lines of the console slide are copied from the guide (`README.md`, "Now listening on" and "Application
started"). The template's launch profile gives those two addresses (`launchSettings.json:21`), but a real run prints a few
more lines, and the recording uses port 5010. The slide is a trimmed illustration, not a capture.

## 7. The captions of the rings and the added space (clips `06-run` and `07-home`)

- **Checked:** `Scene.HighlightAsync` puts the caption 10 px under the ring, in a box with a 15 px font and 6 px of
  padding (`scripts/recorder/Scene.cs`, the `.demo-caption` style and `HighlightAsync`). The template's layout puts a
  line of text at about 70 px from the top (`.body-content { margin-top: 70px }`, `_Layout.cshtml:9-13`), followed by
  `<hr />` and the footer (`:31-34`). Without the script of `recording-plan.md`, a caption under the text would sit on
  the rule and the footer. The script adds 40 px above and 64 px below the text (the heading, in `07-home`: 64 px below).
- **To confirm:** on the first take of each clip, on a frame at the end of each step, that no caption touches the
  navigation bar, the text, the rule or the footer, and that the page still looks like the template's. If a caption does
  touch one, change the two margins in the script; do not change the narration. The numbers were worked out from the CSS
  and `Scene.cs`, and the page was not opened in a browser.

## 8. Behaviour of the dotnet command line, which is not in the Orchard Core source

- **Checked:** the guide uses these commands (`README.md:28`, `:32`, `:37`, `:42-44`, `:51`), and the template files
  agree with them (`shortName`, `sourceName` and `preferNameDirectory` in each `template.json`). The slides and the
  recording plan write paths with forward slashes, which the dotnet command line accepts on every system; the guide
  writes Windows backslashes, which a Bash or Zsh terminal would swallow.
- **To confirm when the projects are generated:** that `dotnet add OrchardCore.Mvc.Web reference OrchardCore.Mvc.HelloWorld`
  accepts a folder in place of a project file (the guide uses it that way), that `dotnet new sln -n OrchardCore.Mvc`
  followed by `dotnet sln add <folder>/<project>.csproj` works with the SDK on the recording machine (recent SDKs create
  a `.slnx` file by default; the video only says "a solution file"), and that `dotnet run ... --no-launch-profile --urls
  http://localhost:5010` forwards the address to the application.
