# Recording plan: A Modular ASP.NET Core MVC Application With Orchard Core

Read this file and `OPEN-QUESTIONS.md`, and nothing else, to record the video. A selector below stands for
`scene.Page.Locator(...)`. Paths of the Orchard Core source (`OC:`) are relative to a checkout of
`OrchardCMS/OrchardCore` at commit `92db4a83`.

## What is recorded

The video has 9 chapters. Only two clips are browser recordings: `06-run` (steps `run-2` and `run-3`) and `07-home`
(steps `home-1` and `home-2`). The terminal commands and the code are slides, drawn by `slides.py` from the storyboard:
there is nothing to record for them (see "The slide steps" at the end).

## The site it needs

This is **not** a CMS site, and the usual recipe does not apply:

- **No Auto Setup, no tenant, no recipe, no time zone, no features to enable, no settings, no database.** The
  application is the plain MVC application of the templates; it runs one tenant with every feature of its module
  enabled (`OC:src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:197`), so there is no Setup
  screen.
- **No sign-in.** Do not run `Recorder setup` (it opens `/Login`, which this application does not have), do not create
  `state.json`, do not set `VIDEO_USER_NAME` or `VIDEO_PASSWORD`, and do not run `Recorder prepare` (the video's
  `Scenes.cs` has no `PrepareAsync`).
- **No external service, no stand-in, no seed data.** The only text on the screens is the template's: `MvcSandbox`,
  `Home`, `Hello from OrchardCore.Mvc.HelloWorld`, `Hello World` and `© 2015 - MvcSandbox`.
- **Internet access for the browser.** The layout loads Bootstrap from `cdn.jsdelivr.net`
  (`OC:src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Mvc.Web/Views/Shared/_Layout.cshtml:8`
  and `:37`). Without it the page is unstyled and the clip must be recorded again. The application itself makes no
  outbound call, so do not set `HTTP_PROXY` or `HTTPS_PROXY` for this video.

### Generate the projects (outside the capture)

In an empty folder outside both repositories, named `OrchardCore.Mvc`:

```bash
mkdir OrchardCore.Mvc && cd OrchardCore.Mvc
dotnet new install OrchardCore.ProjectTemplates      # the templates, as on the slide (see OPEN-QUESTIONS.md, question 1)
dotnet new ocmvc -n OrchardCore.Mvc.Web
dotnet new ocmodulemvc -n OrchardCore.Mvc.HelloWorld
dotnet add OrchardCore.Mvc.Web reference OrchardCore.Mvc.HelloWorld
```

Use the generated projects as they are, not the sample projects of the Orchard Core repository: the sample's view and
`Startup` are different. Before going on, check the generated files against what the video shows. If one differs, stop
and write the difference in `OPEN-QUESTIONS.md`; do not change the narration or the slides.

In the last column, a bare file name is under `OC:src/Templates/OrchardCore.ProjectTemplates/content/<template folder>/`.

| File | Must contain | Source of the template |
| --- | --- | --- |
| `OrchardCore.Mvc.Web/Program.cs` | `.AddOrchardCore()`, `.AddMvc()`, `app.UseOrchardCore();` | `OC:src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Mvc.Web/Program.cs:4-5`, `:28` |
| `OrchardCore.Mvc.Web/OrchardCore.Mvc.Web.csproj` | `Sdk="Microsoft.NET.Sdk.Web"`, a package reference to `OrchardCore.Application.Mvc.Targets` | `OrchardCore.Templates.Mvc.Web.csproj:1` and `:19` |
| `OrchardCore.Mvc.Web/_ViewStart.cshtml` | `Layout = "~/Views/Shared/_Layout.cshtml";` | `OrchardCore.Templates.Mvc.Web/_ViewStart.cshtml:2` |
| `OrchardCore.Mvc.Web/Views/Shared/_Layout.cshtml` | a dark navigation bar with the brand `MvcSandbox` and a `Home` link, `@RenderBody()`, then `<hr />` and a footer `© 2015 - MvcSandbox` | `_Layout.cshtml:16-18`, `:24`, `:29-34` |
| `OrchardCore.Mvc.HelloWorld/OrchardCore.Mvc.HelloWorld.csproj` | `Sdk="Microsoft.NET.Sdk.Razor"`, `AddRazorSupportForMvc`, a package reference to `OrchardCore.Module.Targets` | `OrchardCore.Templates.Mvc.Module.csproj:1`, `:5`, `:15` |
| `OrchardCore.Mvc.HelloWorld/Manifest.cs` | `[assembly: Module(...)]` with `Name`, `Author`, `Website`, `Version`, `Description`, `Category` | `OrchardCore.Templates.Mvc.Module/Manifest.cs:3-10` |
| `OrchardCore.Mvc.HelloWorld/Controllers/HomeController.cs` | `HomeController` with an `Index` action that returns `View()` | `OrchardCore.Templates.Mvc.Module/Controllers/HomeController.cs` |
| `OrchardCore.Mvc.HelloWorld/Startup.cs` | `Startup : StartupBase` with empty `ConfigureServices` and `Configure` | `OrchardCore.Templates.Mvc.Module/Startup.cs:10`, `:14` |
| `OrchardCore.Mvc.HelloWorld/Views/Home/Index.cshtml` | one line: `Hello from OrchardCore.Mvc.HelloWorld` (the template's text, `Hello from OrchardCore.Templates.Mvc.Module`, with the template's name replaced by the project name) | `OrchardCore.Templates.Mvc.Module/Views/Home/Index.cshtml:1`, `.template.config.src/template.json:12` (`sourceName`) |

### Run the application

Set `ORCHARD_APP_DATA` to an empty folder outside both repositories, as `references/recording.md` says (the application
may write its data there), then, from the `OrchardCore.Mvc` folder:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project OrchardCore.Mvc.Web/OrchardCore.Mvc.Web.csproj --no-launch-profile --urls http://localhost:5010
```

and for the recorder:

```bash
export VIDEO_ROOT=<repo>/videos/modular-mvc-application VIDEO_BASE_URL=http://localhost:5010
```

The application is reached over HTTP only, on port 5010. The template calls `UseHttpsRedirection`
(`Program.cs:24`) and its launch profile binds an HTTPS address (`Properties/launchSettings.json:21`), and the recorder
does not accept the development certificate. `--no-launch-profile --urls http://...` avoids both (see
`OPEN-QUESTIONS.md`, question 2). Wait for the console line `Application started.`. Stop the application by its
process ID, never by image name, before every rebuild: it locks the DLLs.

## The two states of the code, and the order of the clips

The two clips show two states of the same projects. There is no data to restore, only code.

- **State A**: the projects as generated, with the project reference. `06-run` is recorded in this state.
- **State B**: state A with the two edits below. `07-home` is recorded in this state.

Order:

1. Generate the projects. Copy the whole `OrchardCore.Mvc` folder aside as `state-A` (without `bin` and `obj`).
2. Run the application (state A). Narrate, then record `06-run`.
3. Stop the application. Apply the edits below. Copy the folder aside as `state-B`. Run the application again, so it
   builds and starts with the edits.
4. Record `07-home`.

To record `06-run` again, stop the application, restore `state-A`, run it. To record `07-home` again, restore
`state-B`. The clips do not depend on each other in any other way.

**The edits (state A to state B)**, the same as on the slides `route-2` and `route-3`:

1. In `OrchardCore.Mvc.HelloWorld/Startup.cs`, put this inside the method `Configure`, which is empty:

   ```csharp
   routes.MapAreaControllerRoute(
       name: "Home",
       areaName: "OrchardCore.Mvc.HelloWorld",
       pattern: "",
       defaults: new { controller = "Home", action = "Index" }
   );
   ```

2. In `OrchardCore.Mvc.HelloWorld/Views/Home/Index.cshtml`, replace the whole content with one line:
   `<h1>Hello World</h1>`.

## Both clips: the page that is captured

The recorder's browser has no address bar, so the viewer never sees an address. Each step puts the address on the
screen as the caption of a green ring (`HighlightAsync(locator, caption)`).

Two things about the page need a small script, run after the page loads and before any ring is drawn. It changes only
the space around the text of the module, not the template's files.

- The text of the module's view is a bare text node inside `div.body-content`, with no element of its own to ring.
- `HighlightAsync` draws its caption 10 px under the ring, in a box about 32 px high (`Scene.cs`, `HighlightAsync`).
  On the template's page the rule (`<hr />`) and the footer follow the text by about 16 px, so a caption under the text
  would cover the footer, and the caption under the navigation bar would cover the text. The script therefore also
  adds space above and below the text. The numbers come from the CSS of the layout and of `Scene.cs`, not from a run:
  see `OPEN-QUESTIONS.md`, question 7.

For `06-run`, once per page load:

```csharp
await scene.Page.EvaluateAsync(@"() => {
  const container = document.querySelector('div.body-content');
  const text = [...container.childNodes].find(n => n.nodeType === 3 && n.textContent.includes('Hello from OrchardCore.Mvc.HelloWorld'));
  const span = document.createElement('span');
  span.id = 'module-text';
  span.style.display = 'inline-block';
  span.style.margin = '40px 0 64px';
  container.insertBefore(span, text);
  span.appendChild(text);
}");
```

For `07-home`, once per page load (the view is an `h1` there, which is already an element):

```csharp
await scene.Page.EvaluateAsync(@"() => {
  const heading = document.querySelector('div.body-content h1');
  heading.style.display = 'inline-block';
  heading.style.marginBottom = '64px';
}");
```

## Clip `06-run` (state A)

Starts on a blank page. Before `StartCaptureAsync`, warm the application up (the first request builds the pipeline of
the tenant and is slow), then leave the page:

```csharp
await scene.OpenAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");   // not recorded
await scene.Page.GotoAsync("about:blank");
await scene.StartCaptureAsync();
```

- `run-2`: "Let's browse to the address of the module: its identifier, the controller, and the action. The page says Hello from
  OrchardCore.Mvc.HelloWorld."
  - Actions: `await scene.NavigateAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");`, run the `06-run` script above, then
    `await scene.HighlightAsync(scene.Page.Locator("#module-text"), "/OrchardCore.Mvc.HelloWorld/Home/Index");`.
  - On screen when the narration ends: the page with the dark navigation bar (`MvcSandbox`, `Home`), the text `Hello from
    OrchardCore.Mvc.HelloWorld` inside a green ring whose caption is `/OrchardCore.Mvc.HelloWorld/Home/Index`, and the
    rule and the footer `© 2015 - MvcSandbox` below the caption, not covered by it. No error page, no unstyled page.
    If the text appears without the navigation bar and the footer, stop: the narration of `web-2` and `run-3` (the views of
    the module use the layout of the web application) is then wrong, and the case goes to `OPEN-QUESTIONS.md` (question 5).
  - Labels from: `Index.cshtml:1`, `_Layout.cshtml:18`, `:24`, `:33` (paths in the table above). The address is the
    default route: `OC:src/OrchardCore/OrchardCore.Mvc.Core/Routing/DefaultAreaControllerRouteMapper.cs:9`.
- `run-3`: "The navigation bar and the footer come from the layout of the web application. The text between them comes
  from the view of the module."
  - Actions, in this order, with `PauseAsync` between them so the whole sequence is shorter than the narration:
    `await scene.ClearHighlightsAsync();`, ring `scene.Page.Locator("nav.navbar")` with the caption `Layout of the web
    application`, `PauseAsync(2200)`, ring `scene.Page.Locator("footer")` with the same caption, `PauseAsync(2200)`,
    `await scene.ClearHighlightsAsync();`, ring `scene.Page.Locator("#module-text")` with the caption `View of the
    module`. Do not scroll. The caption of the first ring sits just under the navigation bar, above the text of the
    module (the space the script adds keeps it clear of the text).
  - On screen when the narration ends: the same page, with a single ring around the module's text and the caption `View
    of the module`, and the rule and the footer below the caption, not covered by it.
  - Labels from: `_Layout.cshtml:16`, `:32-34`; `Index.cshtml:1`.

## Clip `07-home` (state B)

Starts on a blank page, warmed up the same way:

```csharp
await scene.OpenAsync("/");   // not recorded
await scene.Page.GotoAsync("about:blank");
await scene.StartCaptureAsync();
```

This clip does not need the `06-run` script: the module's view is now an `h1`, which is already an element. It uses the
`07-home` script above after each page load.

- `home-1`: "After a restart, let's browse to the root address. The home page shows Hello World, with the layout of the
  web application around it."
  - Actions: `await scene.NavigateAsync("/");`, run the `07-home` script above, then
    `await scene.HighlightAsync(scene.Page.Locator("div.body-content h1"), "/");`.
  - On screen when the narration ends: the dark navigation bar, the heading `Hello World` inside a green ring with the
    caption `/`, and the rule and the footer below the caption, not covered by it. The home page is served by the route
    of state B: `Startup.cs`, `MapAreaControllerRoute` with the pattern `""`.
  - Labels from: the `h1` of the edit above; `_Layout.cshtml:18`, `:33`.
- `home-2`: "The default address still answers as well. The route of the module adds to the default route, and does not
  replace it."
  - Actions: `await scene.ClearHighlightsAsync();`, `await scene.NavigateAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");`,
    run the `07-home` script above, then
    `await scene.HighlightAsync(scene.Page.Locator("div.body-content h1"), "/OrchardCore.Mvc.HelloWorld/Home/Index");`.
  - On screen when the narration ends: the same page as in `home-1` (the heading `Hello World` in the layout), with the
    caption `/OrchardCore.Mvc.HelloWorld/Home/Index`. If this address answers with an error instead, stop: the narration
    of `home-2` is then wrong, and the case goes to `OPEN-QUESTIONS.md` (question 4).
  - The claim comes from the source, not from a run: every action of a module keeps its default route
    (`OC:src/OrchardCore/OrchardCore.Mvc.Core/Startup.cs:62-76`).

## The slide steps

No recording. The slide of each step, for the checks of the recording session and the review:

- `build-1`: three cards: the web application, the module, a project reference.
- `build-2`: three cards: what the package of the web application brings (a modular application, ASP.NET Core MVC, no
  CMS modules).
- `build-3`: three cards: the sample in the Orchard Core repository, the .NET SDK, a text editor and a terminal.
- `create-1`: the command that installs the templates, and the names of the two templates.
- `create-2`: the command that creates the web application, with the template, short name and project name.
- `create-3`: the command that creates the module, with the template, short name and project name.
- `create-4`: the command that adds the project reference.
- `create-5`: the optional solution commands.
- `web-1`: `Program.cs`, with `AddOrchardCore`, `AddMvc` and `UseOrchardCore` bright.
- `web-2`: the project file of the web application and `_ViewStart.cshtml`.
- `mod-1`: the project file of the module.
- `mod-2`: `Manifest.cs`.
- `mod-3`: `HomeController.cs` and `Index.cshtml`.
- `mod-4`: the `Startup` class with its two empty methods.
- `find-1`, `find-2`, `find-3`: the same three cards, one bright at a time: the module builds, the web application
  builds, the application starts.
- `run-1`: the `dotnet run` command and its console output.
- `route-1`: the default route pattern and what its parts are.
- `route-2`: the `Startup` class with its `Configure` method bright, which holds the `MapAreaControllerRoute` call.
- `route-3`: `Index.cshtml` with the heading.
- `route-4`: three cards: why a restart is needed.

## Checks for private data

- The frames hold only the template's text: no person, no email address, no company, no credential, no token.
- The tab title and the address are not captured. The only addresses on screen are the captions typed above, which are
  paths of this application.
- No path of the recording machine appears (the console is a slide, drawn from text, not captured).
- Run the OCR check of the skill on both clips anyway, and look at both contact sheets: the frame at the end of each
  step must match the "On screen" line above.
- Do not commit `state-A`, `state-B`, the `OrchardCore.Mvc` folder, `ORCHARD_APP_DATA`, or any frame.
