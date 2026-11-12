"""The slides of the Static Files per Tenant video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"FileProviderStartup", "StartupBase", "IServiceCollection", "ITenantFileProvider", "IStaticFileProvider",
         "IOptions", "ShellOptions", "ShellSettings", "TenantFileProvider", "IApplicationBuilder",
         "IEndpointRouteBuilder", "IServiceProvider", "StaticFileOptions", "MediaTypeNames", "HeaderNames", "Directory"}

WEB_ROOT = [
    ("1", "One web root", "The wwwroot folder of the application holds its static files."),
    ("2", "Served to every tenant", "A request for a file is answered from that one folder."),
    ("3", "Owned by the application", "A file there is not the file of one tenant."),
]

PROVIDER = [
    ("check", "A file provider for each tenant", "Registered for every tenant that enables the feature."),
    ("check", "A wwwroot folder for each tenant", "In its site folder, under App_Data, Sites."),
    ("check", "Served at the tenant's address", "Under its domain and URL prefix."),
    ("check", "Same names, different files", "Each tenant serves the file of its own folder."),
]

RECIPE = """
{
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.Tenants.FileProvider" ],
      "disable": []
    }
  ]
}
"""

TREE = """
App_Data/
  Sites/
    Maple/                 # URL prefix: bakery
      wwwroot/
        robots.txt         # /bakery/robots.txt
    Cedar/                 # URL prefix: garden
      wwwroot/
        robots.txt         # /garden/robots.txt
"""

SUBFOLDER = """
Sites/Maple/wwwroot/
  robots.txt               # /bakery/robots.txt
  images/
    logo.png               # /bakery/images/logo.png
"""

SERVICES = """
[Feature("OrchardCore.Tenants.FileProvider")]
public sealed class FileProviderStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITenantFileProvider>(serviceProvider =>
        {
            var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var contentRoot = GetContentRoot(shellOptions.Value, shellSettings);
            if (!Directory.Exists(contentRoot))
            {
                Directory.CreateDirectory(contentRoot);
            }
            return new TenantFileProvider(contentRoot);
        });

        services.AddSingleton<IStaticFileProvider>(serviceProvider =>
        {
            return serviceProvider.GetRequiredService<ITenantFileProvider>();
        });
    }
}

"""

PIPELINE = """
public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
{
    var tenantFileProvider = serviceProvider.GetRequiredService<ITenantFileProvider>();

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = tenantFileProvider,
        DefaultContentType = MediaTypeNames.Application.Octet,
        ServeUnknownFileTypes = true,

        // Cache the tenant static files for 30 days.
        OnPrepareResponse = ctx =>
        {
            // ... sets the Cache-Control header
        },
    });
}
"""

VIEW = """
<img src="~/images/logo.png" asp-append-version="true" alt="Maple Bakery" />
"""

RENDERED = """
<!-- Rendered for the tenant with the URL prefix bakery -->
<img src="/bakery/images/logo.png?v=..." alt="Maple Bakery" />
"""

PRECEDENCE = [
    ("1", "The application's wwwroot first", "In the CMS web project, before a tenant file for the same request path."),
    ("2", "robots.txt and the SEO module", "A file in the tenant folder takes precedence over the robots settings."),
]

REMOVAL = [
    ("1", "The site folder goes with the tenant", "wwwroot is inside it, so its files are removed too."),
    ("2", "Keep your own copy", "Copy the files to the server again when you set up a tenant."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Static files of the application")
        bullets(image, WEB_ROOT)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Static File Provider feature")
        bullets(image, PROVIDER)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Enable the feature in a recipe")
        draw_code(image, (96, 240, 1200, 700), "recipe.json", "json", RECIPE, size=26)
        note_cards(image, [
            ("name", "feature"),
            ("enable", "the features to turn on"),
            ("disable", "the features to turn off"),
        ], (1250, 240, W - 96, 900))
        return image

    if step_id == "files-1":
        image = slide_base(chapter_title, label, "The folder of each tenant")
        draw_code(image, (96, 240, 1160, 700), "The site folders", "bash", TREE, size=28)
        note_cards(image, [
            ("Named after", "the tenant, not its prefix"),
            ("Created by", "the feature, if missing"),
            ("Served at", "the tenant's domain and prefix"),
        ], (1210, 240, W - 96, 900))
        return image

    if step_id == "files-2":
        image = slide_base(chapter_title, label, "Files and subfolders")
        draw_code(image, (96, 240, 1160, 600), "The folder of a tenant", "bash", SUBFOLDER, size=28)
        note_cards(image, [
            ("No upload screen", "Copy the files to the server"),
            ("Same path", "under the tenant's address"),
        ], (1210, 240, W - 96, 900))
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "Registering the provider")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", SERVICES, types=TYPES, size=22)
        return image

    if step_id == "code-2":
        image = slide_base(chapter_title, label, "Serving the files")
        draw_code(image, (96, 240, W - 96, 920), "Startup.cs", "csharp", PIPELINE, types=TYPES, size=24)
        return image

    if step_id == "code-3":
        image = slide_base(chapter_title, label, "Using the files in a view")
        draw_code(image, (96, 240, W - 96, 520), "A view", "html", VIEW, size=30)
        draw_code(image, (96, 580, W - 96, 900), "The page of the Maple tenant", "html", RENDERED, size=30)
        return image

    if step_id == "know-1":
        image = slide_base(chapter_title, label, "Which file is served")
        bullets(image, PRECEDENCE)
        return image

    if step_id == "know-2":
        image = slide_base(chapter_title, label, "Removing a tenant")
        bullets(image, REMOVAL)
        return image

    raise SystemExit(f"No slide for {step_id}")
