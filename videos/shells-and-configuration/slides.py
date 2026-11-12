"""The slides of the Shells and Configuration video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"StartupBase", "IShellConfiguration", "IServiceCollection", "MyServiceOptions", "ResourceOptions",
         "IServiceProvider", "WebApplication", "WebApplicationBuilder", "OrchardCoreBuilder"}

# What a tenant is made of.
SETTINGS_AND_CONFIGURATION = [
    ("1", "Settings", "The state, the URL prefix and the URL host of the tenant."),
    ("2", "Configuration", "The settings of its modules, read through IShellConfiguration."),
]

APP_DATA_TREE = """
App_Data/
  tenants.json
  Sites/
    Default/
      appsettings.json
    Acme/
      appsettings.json
"""

TENANTS_ARE_FOUND = [
    ("1", "tenants.json", "Every entry of the file is a tenant."),
    ("2", "A section with a State", "In the configuration, for example OrchardCore:Acme."),
]

APP_DATA_VARIABLE = """
# A path relative to the application:
ORCHARD_APP_DATA=./App_Data

# Or an absolute path:
ORCHARD_APP_DATA=/path/from/root
"""

# The layers of the configuration of a tenant.
LAYERS = [
    ("1", "Host application", "appsettings.json, user secrets, environment variables, command line"),
    ("2", "Files in App_Data", "appsettings.json and the file for the environment; environment variables still win"),
    ("3", "Section named for the tenant", "OrchardCore:Acme, taken from any source above"),
    ("4", "File of the tenant", "App_Data/Sites/Acme/appsettings.json, without an OrchardCore section"),
]

PREPARE = """
{
  "OrchardCore": {
    "Acme": {
      "State": "Uninitialized",
      "RequestUrlPrefix": "acme",
      "Category": "Demo",
      "Description": "The Acme sample site.",
      "DatabaseProvider": "Sqlite"
    }
  }
}
"""

OVERRIDE = """
{
  "OrchardCore": {
    "Default": {
      "Media": {
        "MaxFileSize": 10000000
      }
    }
  }
}
"""

OVERRIDE_VARIABLES = """
# Every tenant
OrchardCore__Media__MaxFileSize=10000000

# The Default tenant
OrchardCore__Default__Media__MaxFileSize=10000000

# The tenant named Acme
OrchardCore__Acme__Media__MaxFileSize=10000000
"""

SAME_PATH = [
    ("1", "A tenant section", "OrchardCore:Acme"),
    ("2", "A module section", "OrchardCore:Media"),
    ("3", "The same path", "Avoid tenant names such as Media or Email."),
]

SHARED_DATABASE = """
{
  "OrchardCore": {
    "ConnectionString": "...",
    "DatabaseProvider": "SqlConnection",
    // ...
    "Default": {
      "State": "Uninitialized",
      "TablePrefix": "Default"
    }
  }
}
"""

TENANTS_OPTIONS = """
{
  "OrchardCore": {
    "Tenants": {
      "RequireTablePrefix": true,
      "TablePrefixPattern": "{{ ShellSettings.Name }}",
      "SchemaPattern": "dbo"
    }
  }
}
"""

SHARED_VALUES = [
    ("check", "Not copied into tenants", "A tenant keeps only the values that differ from the shared ones."),
    ("check", "One place to change", "Moving to another environment means editing the shared values."),
]

MODULE_STARTUP = """
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MyServiceOptions>(
            _shellConfiguration.GetSection("MyService"));
    }
}
"""

MODULE_SECTION = """
{
  "OrchardCore": {
    "MyService": {
      "MyProvider": {
        "MySetting": "..."
      }
    }
  }
}
"""

MODULE_VARIABLE = """
OrchardCore__MyService__MyProvider__MySetting=...
"""

MODULE_CODE = """
builder.Services
    .AddOrchardCms()
    .ConfigureServices((tenantServices, serviceProvider) =>
    {
        var shellConfiguration =
            serviceProvider.GetRequiredService<IShellConfiguration>();

        tenantServices.PostConfigure<ResourceOptions>(options =>
        {
            options.UseCdn = shellConfiguration.GetValue<bool>("Resources:UseCdn");
        });
    });
"""

# The shared stores.
STORES = [
    ("1", "App_Data", "The default. It suits most sites."),
    ("2", "Azure Blob Storage", "tenants.json and the tenant files, in a container."),
    ("3", "Database", "The settings and the configurations, as documents."),
]

AZURE_PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    .AddAzureShellsConfiguration();
"""

AZURE_OPTIONS = """
{
  "OrchardCore": {
    "Shells": {
      "Azure": {
        "ConnectionString": "",
        "ContainerName": "hostcontainer",
        "BasePath": "some/base/path",
        "MigrateFromFiles": true
      }
    }
  }
}
"""

AZURE_CONTAINER = """
tenants.json
appsettings.json
appsettings.{Environment}.json
Sites/
  Default/
    appsettings.json
  Acme/
    appsettings.json
"""

DATABASE_PROGRAM = """
builder.Services
    .AddOrchardCms()
    .AddDatabaseShellsConfiguration();
"""

DATABASE_OPTIONS = """
{
  "OrchardCore": {
    "Shells": {
      "Database": {
        "DatabaseProvider": "SqlConnection",
        "ConnectionString": "",
        "TablePrefix": "",
        "MigrateFromFiles": true
      }
    }
  }
}
"""

ENVIRONMENT_CHECK = """
var orchardCore = builder.Services.AddOrchardCms();

if (!builder.Environment.IsDevelopment())
{
    orchardCore.AddDatabaseShellsConfiguration();
}
"""

STORE_USE = [
    ("check", "Meant for", "Hosts that share tenants while they run."),
    ("check", "Not meant for", "Sharing configuration between development and production."),
]


def caption(image, text, y):
    ImageDraw.Draw(image).text((96, y), text, font=font(30), fill=MUTED, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id == "shell-1":
        image = slide_base(chapter_title, label, "A tenant is a shell")
        bullets(image, SETTINGS_AND_CONFIGURATION)
        return image

    if step_id == "shell-2":
        image = slide_base(chapter_title, label, "By default, both are files")
        draw_code(image, (96, 240, 1060, 760), "App_Data", "bash", APP_DATA_TREE, size=30)
        note_cards(image, [
            ("tenants.json", "The settings of every tenant."),
            ("Sites/Acme/appsettings.json", "The configuration of one tenant."),
        ], (1110, 300, W - 96, 760))
        return image

    if step_id == "shell-3":
        image = slide_base(chapter_title, label, "Where Orchard Core finds tenants")
        bullets(image, TENANTS_ARE_FOUND)
        return image

    if step_id == "shell-4":
        image = slide_base(chapter_title, label, "Moving the App_Data folder")
        draw_code(image, (96, 240, W - 96, 640), "Environment variable", "bash", APP_DATA_VARIABLE, size=30)
        return image

    if step_id.startswith("layers-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "The layers of the configuration of a tenant")
        bullets(image, LAYERS, active={index})
        caption(image, "The last layer wins.", 1000)
        return image

    if step_id == "prepare-1":
        image = slide_base(chapter_title, label, "Prepare a tenant in the configuration")
        draw_code(image, (96, 240, 960, 760), "appsettings.json", "json", PREPARE, size=26)
        note_cards(image, [
            ("State", "Makes the section a tenant."),
            ("Uninitialized", "The tenant waits for its setup."),
            ("Other values", "Presets for the setup."),
        ], (1010, 260, W - 96, 1030))
        return image

    if step_id == "override-1":
        image = slide_base(chapter_title, label, "Change a tenant that exists")
        draw_code(image, (96, 240, 960, 700), "appsettings.json", "json", OVERRIDE, size=26)
        note_cards(image, [
            ("No State", "The tenant already exists."),
            ("Only Default", "Other tenants keep the default."),
        ], (1010, 280, W - 96, 1030))
        return image

    if step_id == "override-2":
        image = slide_base(chapter_title, label, "The same layout in environment variables")
        draw_code(image, (96, 240, W - 96, 700), "Environment variables", "bash", OVERRIDE_VARIABLES, size=30)
        caption(image, "A double underscore separates each level.", 780)
        return image

    if step_id == "override-3":
        image = slide_base(chapter_title, label, "Tenant names and section names")
        bullets(image, SAME_PATH)
        return image

    if step_id == "shared-1":
        image = slide_base(chapter_title, label, "One database for every tenant")
        draw_code(image, (96, 240, 960, 760), "appsettings.json", "json", SHARED_DATABASE, size=26)
        note_cards(image, [
            ("Root of OrchardCore", "Shared by every tenant."),
            ("Default:TablePrefix", "Keeps the Default tables apart."),
        ], (1010, 280, W - 96, 1030))
        return image

    if step_id == "shared-2":
        image = slide_base(chapter_title, label, "Table prefixes and schemas")
        draw_code(image, (96, 240, 960, 700), "appsettings.json", "json", TENANTS_OPTIONS, size=24)
        note_cards(image, [
            ("RequireTablePrefix", "A prefix when a tenant is created."),
            ("TablePrefixPattern", "Generated by a Fluid template."),
            ("SchemaPattern", "The same, for the schema."),
        ], (1010, 260, W - 96, 1030))
        return image

    if step_id == "shared-3":
        image = slide_base(chapter_title, label, "Shared values stay in one place")
        bullets(image, SHARED_VALUES)
        return image

    if step_id == "modules-1":
        image = slide_base(chapter_title, label, "1. Read a section in a module")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", MODULE_STARTUP, types=TYPES, size=28)
        return image

    if step_id == "modules-2":
        image = slide_base(chapter_title, label, "2. Name and nest the section")
        draw_code(image, (96, 240, W - 96, 700), "appsettings.json", "json", MODULE_SECTION, size=28)
        draw_code(image, (96, 740, W - 96, 960), "Environment variable", "bash", MODULE_VARIABLE, size=28)
        return image

    if step_id == "modules-3":
        image = slide_base(chapter_title, label, "3. Or set options in code")
        draw_code(image, (96, 240, W - 96, 800), "Program.cs", "csharp", MODULE_CODE, types=TYPES, size=26)
        caption(image, "ConfigureServices runs for each tenant.", 880)
        return image

    if step_id == "stores-1":
        image = slide_base(chapter_title, label, "Where the files can live")
        bullets(image, STORES)
        return image

    if step_id == "stores-2":
        image = slide_base(chapter_title, label, "Azure Blob Storage: register the provider")
        draw_code(image, (96, 240, W - 96, 660), "Program.cs", "csharp", AZURE_PROGRAM, types=TYPES, size=30)
        caption(image, "The provider comes in the OrchardCore.Shells.Azure package.", 740)
        return image

    if step_id == "stores-3":
        image = slide_base(chapter_title, label, "Azure Blob Storage: configure it")
        draw_code(image, (96, 240, 1010, 800), "appsettings.json", "json", AZURE_OPTIONS, size=26,
                  highlight={5, 6, 7})
        note_cards(image, [
            ("ConnectionString", "Your storage account."),
            ("ContainerName", "It must exist."),
            ("BasePath", "Optional folder."),
        ], (1060, 260, W - 96, 1030))
        return image

    if step_id == "stores-4":
        image = slide_base(chapter_title, label, "Azure Blob Storage: the container")
        draw_code(image, (96, 240, 1060, 800), "Container", "bash", AZURE_CONTAINER, size=28)
        note_cards(image, [
            ("tenants.json", "The settings of every tenant."),
            ("Root appsettings.json", "And the one for the environment."),
            ("Sites folder", "One appsettings.json per tenant."),
        ], (1110, 260, W - 96, 1030))
        return image

    if step_id == "stores-5":
        image = slide_base(chapter_title, label, "Database: register and configure it")
        draw_code(image, (96, 240, W - 96, 470), "Program.cs", "csharp", DATABASE_PROGRAM, types=TYPES, size=28)
        draw_code(image, (96, 500, W - 96, 1030), "appsettings.json", "json", DATABASE_OPTIONS, size=26)
        return image

    if step_id == "stores-6":
        image = slide_base(chapter_title, label, "The options of the providers")
        draw_code(image, (96, 240, 940, 820), "appsettings.json (Azure)", "json", AZURE_OPTIONS, size=22,
                  highlight={8})
        draw_code(image, (980, 240, W - 96, 820), "appsettings.json (Database)", "json", DATABASE_OPTIONS, size=22,
                  highlight={8})
        caption(image, "Read from the host configuration, never from the store.", 900)
        return image

    if step_id == "stores-7":
        image = slide_base(chapter_title, label, "Skip a provider in development")
        draw_code(image, (96, 240, W - 96, 580), "Program.cs", "csharp", ENVIRONMENT_CHECK, types=TYPES, size=28)
        bullets(image, STORE_USE, top=640)
        return image

    raise SystemExit(f"No slide for {step_id}")
