"""The slides of the Configuration video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IShellConfiguration", "IConfiguration", "StartupBase", "IServiceCollection", "TenantsOptions", "Startup"}

# Chapter 1: sections.

INTERFACE = """
namespace OrchardCore.Environment.Shell.Configuration;

public interface IShellConfiguration : IConfiguration;
"""

IDEA = [
    ("1", "Application-wide", "One ASP.NET Core configuration, shared by every tenant."),
    ("2", "Tenant-specific", "Each tenant adds its own settings on top of the shared ones."),
    ("3", "An IConfiguration", "Read sections and values, or bind options, as in any ASP.NET Core code."),
]

SECTIONS = """
{
  "OrchardCore": {
    "Media": {
      "MaxFileSize": 30000000,
      "Azure": {
        "ContainerName": "somecontainer"
      }
    },
    "Email": {
      "Smtp": {
        "Host": "localhost",
        "Port": 25
      }
    }
  }
}
"""

SECTION_NOTES = [
    ("One root section", "OrchardCore holds it all."),
    ("Nested by service", "Media, Email, Search, ..."),
    ("Then by provider", "Azure, Smtp, Elasticsearch, ..."),
]

NAMES = [
    ("1", "Current names", "Nested by service: Media:Azure, Email:Smtp, Search:Elasticsearch."),
    ("2", "Deprecated names", "Prefixed and flat: OrchardCore_Media_Azure, OrchardCore_Email_Smtp. Still read."),
    ("3", "Defined under both", "The current name wins, key by key. An array replaces the other array."),
]

OPT_IN = [
    ("1", "Modules opt in", "Only a module that reads a section is configured by it."),
    ("2", "Look for examples", "The appsettings.json of OrchardCore.Cms.Web lists many sections as comments."),
]

# Chapter 2: environment variables.

ENVIRONMENT = """
# Every tenant
OrchardCore__Media__MaxFileSize=10000000

# The Default tenant only
OrchardCore__Default__Media__MaxFileSize=10000000

# The tenant named MyTenant only
OrchardCore__MyTenant__Media__MaxFileSize=10000000
"""

DEPLOYMENT = [
    ("1", "Azure App Settings", "Reach the site as environment variables, on Windows and on Linux."),
    ("2", "Pipelines", "Transform the appsettings.json file with values from pipeline variables."),
]

# Chapter 3: sources.

SOURCES = [
    ("1", "The host project", "appsettings.json, appsettings.{Environment}.json, then user secrets in Development."),
    ("2", "The App_Data folder", "App_Data/appsettings.json, then App_Data/appsettings.{Environment}.json."),
    ("3", "Environment variables, then command line", "Loaded by ASP.NET Core, and applied again here, after the App_Data files."),
    ("4", "The tenant", "Its OrchardCore:{Tenant} section, then App_Data/Sites/{tenant}/appsettings.json."),
]

APP_DATA = """
# Relative to the application folder
ORCHARD_APP_DATA=./App_Data

# Absolute
ORCHARD_APP_DATA=/path/from/root

# Fully qualified, on Windows
ORCHARD_APP_DATA=D:\\Path\\To\\App_Data
"""

# Chapter 4: tenants.

PRE_TENANT = """
{
  "OrchardCore": {
    "MyTenant": {
      "State": "Uninitialized",
      "RequestUrlPrefix": "mytenant",
      "DatabaseProvider": "Sqlite",
      "RecipeName": "SaaS"
    }
  }
}
"""

PRE_NOTES = [
    ("State", "Uninitialized: not set up."),
    ("URL prefix", "Routes /mytenant to the tenant."),
    ("Provider, recipe", "Used when it is set up."),
]

STATE_RULE = [
    ("1", "With a valid State", "A section under OrchardCore with a valid State is a tenant, listed before it is set up."),
    ("2", "Without a State", "A section named for an existing tenant only changes its configuration."),
]

POST_TENANT = """
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

POST_NOTES = [
    ("Named for the tenant", "OrchardCore:Default."),
    ("No State", "The tenant already exists."),
    ("Same keys", "Media:MaxFileSize, as anywhere."),
]

SHARED_PATH = [
    ("!", "Same path", "OrchardCore:MyTenant sits next to OrchardCore:Media and OrchardCore:Email."),
    ("check", "Pick another name", "Do not name a tenant Media, Email or any other module section."),
]

TENANT_FILE = """
{
  "Media": {
    "MaxFileSize": 10000000
  }
}
"""

FILE_NOTES = [
    ("At the root", "No OrchardCore section."),
    ("One file only", "No file per environment."),
    ("One folder per tenant", "Sites, then the tenant name."),
]

WRITTEN = [
    ("1", "App_Data/Sites/MyTenant/appsettings.json", "Configuration keys that differ from the application configuration."),
    ("2", "App_Data/tenants.json", "Tenant settings such as State, RequestUrlHost and RequestUrlPrefix."),
    ("3", "Only what differs", "A value equal to the shared configuration is not saved, in either file."),
]

# Chapter 5: one database.

SHARED_DATABASE = """
{
  "OrchardCore": {
    "ConnectionString": "...",
    "DatabaseProvider": "SqlConnection",
    "Tenants": {
      "RequireTablePrefix": true,
      "TablePrefixPattern": "{{ ShellSettings.Name }}",
      "SchemaPattern": "dbo"
    },
    "Default": {
      "State": "Uninitialized",
      "TablePrefix": "Default"
    }
  }
}
"""

DATABASE_NOTES_1 = [
    ("Set once", "At the root of OrchardCore."),
    ("Shared by all", "No per-tenant database settings."),
    ("Default tenant", "Its own table prefix."),
]

DATABASE_NOTES_2 = [
    ("Prefix, schema", "Generated for new tenants."),
    ("Valid names", "Letters, digits, underscores."),
    ("Prefix required", "When a tenant is created."),
]

PROVIDERS = [
    ("check", "SQL Server, MySQL, PostgreSQL", "Use the shared connection string, with table prefixes and schemas."),
    ("2", "SQLite", "Keeps a database file for each tenant: the shared connection string is not used."),
]

# Chapter 6: your own module.

STARTUP = """
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.Configure<TenantsOptions>(_shellConfiguration.GetSectionCompat("Tenants", "OrchardCore_Tenants"));
    }
}
"""

OWN_SECTION = """
{
  "OrchardCore": {
    "MyService": {
      "MyProvider": {
        "MySetting": "value"
      }
    }
  }
}
"""

OWN_VARIABLE = """
OrchardCore__MyService__MyProvider__MySetting=value
"""

SCHEMA_TARGETS = """
<Project>
  <ItemGroup>
    <JsonSchemaSegment Include="$(MSBuildThisFileDirectory)..\\ConfigurationSchema.json" FilePathPattern="appsettings\\..*json" />
  </ItemGroup>
</Project>
"""

SCHEMA = [
    ("1", "ConfigurationSchema.json", "Describes your sections under the OrchardCore property, at the root of the package."),
    ("2", "IntelliSense and validation", "Visual Studio suggests names and values in appsettings files, and flags deprecated names."),
]


def slide(step_id, chapter_title, label):
    if step_id == "sections-1":
        image = slide_base(chapter_title, label, "IShellConfiguration")
        draw_code(image, (96, 240, W - 96, 450), "IShellConfiguration.cs", "csharp", INTERFACE, types=TYPES, size=28)
        bullets(image, IDEA, top=490)
        return image

    if step_id == "sections-2":
        image = slide_base(chapter_title, label, "Everything sits under OrchardCore")
        draw_code(image, (96, 240, 1180, 900), "appsettings.json", "json", SECTIONS, types=TYPES, size=26)
        note_cards(image, SECTION_NOTES, (1220, 240, W - 96, 900))
        return image

    if step_id == "sections-3":
        image = slide_base(chapter_title, label, "Current and deprecated section names")
        bullets(image, NAMES)
        return image

    if step_id == "sections-4":
        image = slide_base(chapter_title, label, "Not every module reads a section")
        bullets(image, OPT_IN)
        return image

    if step_id in ("env-1", "env-2"):
        image = slide_base(chapter_title, label, "Environment variables")
        highlight = {1, 2} if step_id == "env-1" else {4, 5, 7, 8}
        draw_code(image, (96, 240, W - 96, 720), "Environment variables", "bash", ENVIRONMENT, types=TYPES,
                  highlight=highlight, size=30)
        if step_id == "env-1":
            note = ("Each __ stands for a colon", "OrchardCore__Media__MaxFileSize is the path OrchardCore:Media:MaxFileSize.")
        else:
            note = ("The tenant name follows OrchardCore", "OrchardCore:Default:Media:MaxFileSize, OrchardCore:MyTenant:Media:MaxFileSize.")
        note_cards(image, [note], (96, 780, W - 96, 1030))
        return image

    if step_id == "env-3":
        image = slide_base(chapter_title, label, "Configuration in deployment")
        bullets(image, DEPLOYMENT)
        return image

    if step_id == "order-5":
        image = slide_base(chapter_title, label, "Moving the App_Data folder")
        draw_code(image, (96, 240, W - 96, 760), "Environment variable", "bash", APP_DATA, types=TYPES, size=30)
        note_cards(image, [("Created when missing", "Orchard Core creates the folder if it does not exist.")], (96, 800, W - 96, 1030))
        return image

    if step_id.startswith("order-"):
        image = slide_base(chapter_title, label, "Later sources override earlier ones")
        bullets(image, SOURCES, active={int(step_id[-1]) - 1})
        return image

    if step_id == "pre-1":
        image = slide_base(chapter_title, label, "Configure a tenant before it exists")
        draw_code(image, (96, 240, 1180, 760), "appsettings.json", "json", PRE_TENANT, types=TYPES, size=27)
        note_cards(image, PRE_NOTES, (1220, 240, W - 96, 900))
        return image

    if step_id == "pre-2":
        image = slide_base(chapter_title, label, "A State makes a section a tenant")
        bullets(image, STATE_RULE)
        return image

    if step_id == "post-1":
        image = slide_base(chapter_title, label, "Configure a tenant that exists")
        draw_code(image, (96, 240, 1180, 760), "appsettings.json", "json", POST_TENANT, types=TYPES, size=27)
        note_cards(image, POST_NOTES, (1220, 240, W - 96, 900))
        return image

    if step_id == "post-2":
        image = slide_base(chapter_title, label, "Avoid the names of module sections")
        bullets(image, SHARED_PATH)
        return image

    if step_id == "file-1":
        image = slide_base(chapter_title, label, "The configuration file of a tenant")
        draw_code(image, (96, 240, 1180, 700), "App_Data/Sites/MyTenant/appsettings.json", "json", TENANT_FILE,
                  types=TYPES, size=28)
        note_cards(image, FILE_NOTES, (1220, 240, W - 96, 900))
        return image

    if step_id == "file-2":
        image = slide_base(chapter_title, label, "What setup writes")
        bullets(image, WRITTEN)
        return image

    if step_id in ("db-1", "db-2"):
        image = slide_base(chapter_title, label, "The same database for every tenant")
        highlight = {3, 4, 10, 11, 12, 13} if step_id == "db-1" else {5, 6, 7, 8, 9}
        draw_code(image, (96, 240, 1196, 900), "appsettings.json", "json", SHARED_DATABASE, types=TYPES,
                  highlight=highlight, size=25)
        note_cards(image, DATABASE_NOTES_1 if step_id == "db-1" else DATABASE_NOTES_2, (1236, 240, W - 96, 900))
        return image

    if step_id == "db-3":
        image = slide_base(chapter_title, label, "Which providers share a connection string")
        bullets(image, PROVIDERS)
        return image

    if step_id == "mod-1":
        image = slide_base(chapter_title, label, "Read a section in your module")
        draw_code(image, (96, 240, W - 96, 900), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24)
        return image

    if step_id == "mod-2":
        image = slide_base(chapter_title, label, "Name your own sections")
        draw_code(image, (96, 240, 900, 760), "appsettings.json", "json", OWN_SECTION, types=TYPES, size=26)
        draw_code(image, (940, 240, W - 96, 420), "Environment variable", "bash", OWN_VARIABLE, types=TYPES, size=22)
        note_cards(image, [("Short names", "Nesting keeps variable names short.")], (940, 470, W - 96, 700))
        return image

    if step_id == "mod-3":
        image = slide_base(chapter_title, label, "Describe your sections")
        draw_code(image, (96, 240, W - 96, 520), "buildTransitive/{PackageId}.targets", "xml", SCHEMA_TARGETS,
                  types=TYPES, size=22)
        bullets(image, SCHEMA, top=570)
        return image

    raise SystemExit(f"No slide for {step_id}")
