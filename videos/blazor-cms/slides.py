"""The slides of the Blazor video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the guide (src/docs/guides/create-blazor-cms/README.md) and from the Orchard Core source, and trimmed.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"ContentItem", "ISite", "BodyAspect", "VersionOptions", "Task", "IContentHandleManager", "IContentManager",
         "ISiteService", "MarkupString", "WebApplication"}

IDEA_1 = [
    ("1", "Orchard Core", "Editors create, edit and publish content items in the admin."),
    ("2", "Blazor", "Components turn the content items into pages."),
    ("3", "One application", "Components read content through Orchard Core services."),
]

IDEA_2 = [
    ("1", "Razor class library", "The App component, the router, the layout and the pages."),
    ("2", "Web project", "References the library, and maps it into the pipeline of every tenant."),
    ("3", "Program.cs", "Registers the Razor components, and maps the App component."),
]

SETUP = [
    ("1", "Setup", "The recipe is chosen: enter a site name and the super user, then Finish Setup."),
    ("2", "Sign in", "Use the super user at the URL prefix of the tenant."),
    ("3", "JSON Import", "Tools, Deployments, JSON Import adds the Markdown Page type and an item."),
]

PREFIX_SOURCES = [
    ("1", "HttpContext", "Request.PathBase holds the URL prefix of the tenant."),
    ("2", "ShellScope", "ShellScope.Context.Settings.RequestUrlPrefix is the prefix itself."),
    ("3", "ISiteService", "Site.BaseUrl is the Base url setting. It is empty unless it is set."),
]

CONNECTION = [
    ("1", "Developer tools", "Open the network tab of the browser."),
    ("2", "WebSocket", "Filter on WebSocket connections, and refresh the page."),
    ("3", "Test", "Select it: each click sends a message over the connection."),
]

LIBRARY_COMMANDS = """
dotnet new razorclasslib -o OCBlazorLib
dotnet sln add ./OCBlazorLib
dotnet add ./BlazorCms/BlazorCms.csproj reference ./OCBlazorLib/OCBlazorLib.csproj
"""

LIBRARY_PROJECT = """
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
"""

APP = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <base href="/" />
    <link rel="stylesheet" href="OrchardCore.Resources/Styles/bootstrap.min.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
"""

ROUTES = """
<Router AppAssembly="typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
"""

MAIN_LAYOUT = """
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
"""

NAV_MENU = """
<nav class="flex-column">
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="home">Home</NavLink>
    </div>
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="content/orchard-core">
            Orchard Core
        </NavLink>
    </div>
</nav>
"""

HOME = """
@page "/home"

<PageTitle>Home</PageTitle>

<h1>Hello, Orchard!</h1>
"""

PROGRAM = """
using OCBlazorLib;

builder.Services
    .AddOrchardCms()
    // Orchard Specific Pipeline
    .ConfigureServices(services =>
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
    })
    .Configure((app, routes, services) =>
    {
        app.UseStaticFiles();
        app.UseAntiforgery();
        routes.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
    });
"""

CONTENT_HEAD = """
@page "/content/{alias}"
@using OrchardCore.ContentManagement
@using OrchardCore.ContentManagement.Models
@using OrchardCore.Settings
@inject IContentHandleManager handleManager
@inject IContentManager contentManager
@inject ISiteService SiteService

@code
{
    [Parameter]
    public string Alias { get; set; }

    protected ContentItem ContentItem { get; set; }
    protected string Markup { get; set; }
    protected ISite Site { get; set; }
    // ...
}
"""

CONTENT_LOAD = """
protected override async Task OnParametersSetAsync()
{
    string id = await handleManager.GetContentItemIdAsync($"alias:{Alias}");
    ContentItem = await contentManager.GetAsync(id, VersionOptions.Published);
    var bodyAspect = await contentManager.PopulateAspectAsync<BodyAspect>(ContentItem);
    Markup = bodyAspect.Body.ToString();
}
"""

CONTENT_MARKUP = """
@if (ContentItem == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <PageTitle>@ContentItem.DisplayText - @Site.SiteName</PageTitle>

    <h1>@ContentItem.DisplayText</h1>
    @((MarkupString)Markup)
}
"""

STREAM_ATTRIBUTE = """
@attribute [StreamRendering]
"""

STREAM_INIT = """
protected override async Task OnInitializedAsync()
{
    Site = await SiteService.GetSiteSettingsAsync();
    await Task.Delay(500); // mimics a long-running task
}
"""

MIDDLEWARE = """
if (!string.IsNullOrEmpty(shellContext.Settings.RequestUrlPrefix))
{
    var prefix = shellContext.Settings.RequestPathBase;
    httpContext.Request.PathBase += prefix;
    // ...
}
"""

APP_BASE = """
@inject NavigationManager NavManager

<head>
    @* ... *@
    <base href=@_baseUrl />
    <HeadOutlet />
</head>

@code
{
    protected string _baseUrl = "/";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _baseUrl = NavManager.BaseUri;
    }
}
"""

BUTTON = """
<h3>Interactivity test</h3>

<button class="btn-primary" @onclick="Clicked">Test</button>
<hr />
@_interactiveFeedback

@code {

    protected string? _interactiveFeedback;

    protected void Clicked()
    {
        _interactiveFeedback = $"Clicked! [{DateTime.Now.ToLocalTime()}]";
    }
}
"""

USAGE = """
<OCBlazorLib.Components.InteractiveButton @rendermode="RenderMode.InteractiveServer" />
"""

REGISTRATION = """
services.AddRazorComponents()
    .AddInteractiveServerComponents();
// ...
routes.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
"""


def notes(image, lines, y):
    """Short statements under a code panel, each with a green dot."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((100, y - 7, 114, y + 7), fill=GREEN)
        draw.text((136, y), line, font=font(30), fill=TEXT, anchor="lm")
        y += 60


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Content in Orchard Core, pages in Blazor")
        bullets(image, IDEA_1)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Where each part lives")
        bullets(image, IDEA_2)
        return image

    if step_id == "lib-1":
        image = slide_base(chapter_title, label, "1. Create the library, and reference it")
        draw_code(image, (96, 240, W - 96, 470), "Terminal", "bash", LIBRARY_COMMANDS, size=26)
        draw_code(image, (96, 510, W - 96, 740), "OCBlazorLib.csproj", "xml", LIBRARY_PROJECT, size=26)
        notes(image, ["Razor components in Orchard Core modules are not supported.",
                      "Use a library, and reference it from the web project."], 810)
        return image

    if step_id == "lib-2":
        image = slide_base(chapter_title, label, "2. The App component and the router")
        draw_code(image, (96, 240, W - 96, 728), "App.razor", "html", APP, types=TYPES, size=22)
        draw_code(image, (96, 752, W - 96, 1030), "Routes.razor", "html", ROUTES, types=TYPES, size=22)
        return image

    if step_id == "lib-3":
        image = slide_base(chapter_title, label, "3. Layout, navigation and the Home page")
        draw_code(image, (96, 240, 840, 730), "MainLayout.razor", "html", MAIN_LAYOUT, types=TYPES, size=22)
        draw_code(image, (880, 240, W - 96, 730), "NavMenu.razor", "html", NAV_MENU, types=TYPES, size=22)
        draw_code(image, (96, 760, 840, 1000), "Home.razor", "html", HOME, types=TYPES, size=22)
        note_cards(image, [("Why /home?", "Orchard Core also listens on the root path.")], (880, 790, W - 96, 1000))
        return image

    if step_id == "serve-1":
        image = slide_base(chapter_title, label, "Program.cs: the Orchard Specific Pipeline")
        draw_code(image, (96, 240, 1110, 900), "Program.cs", "csharp", PROGRAM, types=TYPES,
                  highlight={5, 6, 7, 8, 9, 10}, size=24)
        note_cards(image, [("Template", "Leaves a commented block."),
                           ("ConfigureServices", "Registers Razor components.")], (1150, 280, W - 96, 1000))
        return image

    if step_id == "serve-2":
        image = slide_base(chapter_title, label, "Program.cs: the Orchard Specific Pipeline")
        draw_code(image, (96, 240, 1110, 900), "Program.cs", "csharp", PROGRAM, types=TYPES,
                  highlight={11, 12, 13, 14, 15, 16, 17}, size=24)
        note_cards(image, [("Configure", "Static files, anti-forgery, App."),
                           ("For each tenant", "Both callbacks run per tenant.")], (1150, 280, W - 96, 1000))
        return image

    if step_id == "page-1":
        image = slide_base(chapter_title, label, "The Content page: route and services")
        draw_code(image, (96, 240, W - 96, 960), "Content.razor", "html", CONTENT_HEAD, types=TYPES, size=25)
        notes(image, ["One package reference: OrchardCore.ContentManagement.",
                      "It brings IContentManager, IContentHandleManager and ISiteService."], 1000)
        return image

    if step_id == "page-2":
        image = slide_base(chapter_title, label, "From an alias to HTML")
        draw_code(image, (96, 240, W - 96, 580), "Content.razor", "csharp", CONTENT_LOAD, types=TYPES, size=26)
        notes(image, ["The prefix alias: plus the alias gives the content item ID.",
                      "An unknown alias gives a null item.",
                      "PopulateAspectAsync throws for a null item, so guard against it."], 650)
        return image

    if step_id == "page-3":
        image = slide_base(chapter_title, label, "The markup, and sanitized HTML")
        draw_code(image, (96, 240, W - 96, 740), "Content.razor", "html", CONTENT_MARKUP, types=TYPES, size=26)
        notes(image, ["MarkupString writes the HTML as it is.",
                      "Markdown body part: Sanitize Html is on by default."], 810)
        return image

    if step_id == "page-4":
        image = slide_base(chapter_title, label, "Stream rendering")
        draw_code(image, (96, 240, W - 96, 390), "Content.razor", "html", STREAM_ATTRIBUTE, size=26)
        draw_code(image, (96, 430, W - 96, 710), "Content.razor", "csharp", STREAM_INIT, types=TYPES, size=26)
        notes(image, ["Loading... is sent first.", "The content streams in when the work finishes."], 780)
        return image

    if step_id == "setup-1":
        image = slide_base(chapter_title, label, "Set up the tenant")
        bullets(image, SETUP)
        return image

    if step_id == "base-1":
        image = slide_base(chapter_title, label, "The prefix of a tenant is in the path base")
        draw_code(image, (96, 240, 1200, 580), "ModularTenantRouterMiddleware.cs", "csharp", MIDDLEWARE, size=24)
        notes(image, ["The default tenant: the base tag is /.",
                      "FirstTenant: the base tag must be /tenant01/.",
                      "Relative links resolve from the base tag."], 650)
        return image

    if step_id == "base-2":
        image = slide_base(chapter_title, label, "Set the base tag from NavigationManager")
        draw_code(image, (96, 240, W - 96, 960), "App.razor", "html", APP_BASE, types=TYPES, size=24)
        return image

    if step_id == "base-3":
        image = slide_base(chapter_title, label, "Other ways to read the prefix")
        bullets(image, PREFIX_SOURCES)
        return image

    if step_id == "inter-1":
        image = slide_base(chapter_title, label, "A component with an event handler")
        draw_code(image, (96, 240, W - 96, 880), "InteractiveButton.razor", "html", BUTTON, types=TYPES, size=26)
        return image

    if step_id == "inter-2":
        image = slide_base(chapter_title, label, "Use the render mode")
        draw_code(image, (96, 240, W - 96, 400), "Interactive.razor", "html", USAGE, types=TYPES, size=26)
        draw_code(image, (96, 440, W - 96, 740), "Program.cs", "csharp", REGISTRATION, types=TYPES, size=26)
        notes(image, ["The render mode makes the component interactive.",
                      "Program.cs registers the services and the render mode."], 810)
        return image

    if step_id == "inter-4":
        image = slide_base(chapter_title, label, "Check the connection")
        bullets(image, CONNECTION)
        return image

    raise SystemExit(f"No slide for {step_id}")
