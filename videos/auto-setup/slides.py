"""The slides of the Auto Setup video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The helpers
below only measure: they stop with an error when a text or a code panel would run past its box.
"""
from PIL import Image, ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {"WebApplication", "OrchardCoreBuilder", "IServiceCollection"}

MARGIN = 96
FULL = W - 2 * MARGIN

# The panels of code and text.

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

var app = builder.Build();

// ...
app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
"""

TEMPLATE_CALL = """
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");
"""

SECTION = """
{
  "OrchardCore": {
    "AutoSetup": {
      "Tenants": [
        {
          "ShellName": "Default",
          "SiteName": "Lakeside Journal",
          "SiteTimeZone": "America/Los_Angeles",
          "AdminUsername": "admin",
          "AdminEmail": "admin@example.com",
          "AdminPassword": "<password>",
          "DatabaseProvider": "Sqlite",
          "RecipeName": "Blog"
        }
      ]
    }
  }
}
"""

ENVIRONMENT = """
export OrchardCore__AutoSetup__Tenants__0__ShellName="Default"
export OrchardCore__AutoSetup__Tenants__0__SiteName="Lakeside Journal"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone="America/Los_Angeles"
export OrchardCore__AutoSetup__Tenants__0__AdminUsername="admin"
export OrchardCore__AutoSetup__Tenants__0__AdminEmail="admin@example.com"
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="<password>"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider="Sqlite"
export OrchardCore__AutoSetup__Tenants__0__RecipeName="Blog"
"""

USER_SECRETS = """
cd src/OrchardCore.Cms.Web
dotnet user-secrets init
dotnet user-secrets set "OrchardCore:AutoSetup:Tenants:0:ShellName" "Default"
dotnet user-secrets set "OrchardCore:AutoSetup:Tenants:0:SiteName" "Lakeside Journal"
dotnet user-secrets set "OrchardCore:AutoSetup:Tenants:0:AdminPassword" "<password>"
# ...
"""

TENANTS = """
"Tenants": [
  {
    "ShellName": "Default",
    // ...
  },
  {
    "ShellName": "Harbor",
    "SiteName": "Harbor Notes",
    "SiteTimeZone": "America/Los_Angeles",
    "AdminUsername": "harboradmin",
    "AdminEmail": "harbor@example.com",
    "AdminPassword": "<password>",
    "DatabaseProvider": "Sqlite",
    "RecipeName": "Blog",
    "RequestUrlPrefix": "harbor"
  }
]
"""

PATH = """
{
  "OrchardCore": {
    "AutoSetup": {
      "AutoSetupPath": "/autosetup",
      "Tenants": [
        // ...
      ]
    }
  }
}
"""

VALIDATION_LOG = """
AutoSetup did not start, configuration has following errors: The AdminPassword field is required.
"""

SUCCESS_LOG = """
The AutoSetup successfully provisioned the site 'Lakeside Journal'.
"""

FAILURE_RESPONSE = """
503 Service Unavailable
The AutoSetup failed installing the site.
"""

FAILURE_LOG = """
The AutoSetup failed installing the site 'Lakeside Journal' with errors: ...
"""

LOCK_PROGRAM = """
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.Redis.Lock", "OrchardCore.AutoSetup");
"""

LOCK_ENVIRONMENT = """
export OrchardCore__Redis__Configuration="redis.example.com:6379"
"""

LOCK_OPTIONS = """
export OrchardCore__AutoSetup__LockOptions__LockTimeout=10000
export OrchardCore__AutoSetup__LockOptions__LockExpiration=10000
"""


# Measuring helpers.

_ruler = ImageDraw.Draw(Image.new("RGB", (1, 1)))


def _fail(message):
    raise SystemExit(f"Slide overflow: {message}")


def code_height(text, size):
    """The height of a code panel that holds text: the window bar, the lines, and a bottom margin."""
    return 74 + len(text.strip("\n").split("\n")) * int(size * 1.42) + 24


def code(image, x0, y0, x1, y1, title, language, text, size, highlight=None):
    """draw_code, after checking that the longest line fits the width and the lines fit the height. y1 None: the
    panel is as tall as its lines. Returns the bottom of the panel."""
    if y1 is None:
        y1 = y0 + code_height(text, size)
    widest = max(_ruler.textlength(line, font=mono(size)) for line in text.strip("\n").split("\n"))
    if widest > (x1 - x0) - 60:
        _fail(f"{title}: a line of {widest:.0f} px does not fit in {x1 - x0 - 60} px")
    if code_height(text, size) > y1 - y0:
        _fail(f"{title}: {code_height(text, size)} px of lines do not fit in {y1 - y0} px")
    if y1 > 1040:
        _fail(f"{title}: the panel ends at {y1}")
    draw_code(image, (x0, y0, x1, y1), title, language, text, types=TYPES, highlight=highlight, size=size)
    return y1


def notes(image, cards, x0, y0, x1):
    """note_cards, after checking that the texts fit the cards and the cards fit the slide."""
    for title, text in cards:
        for value, weight, size in ((title, "Bold", 34), (text, "Regular", 30)):
            width = _ruler.textlength(value, font=font(size, weight))
            if width > (x1 - x0) - 80:
                _fail(f"card {title!r}: {value!r} is {width:.0f} px wide, the card allows {x1 - x0 - 80}")
    if y0 + (len(cards) - 1) * 220 + 180 > 1040:
        _fail(f"the cards end at {y0 + (len(cards) - 1) * 220 + 180}")
    note_cards(image, cards, (x0, y0, x1, 0))


def cards(image, items, top=250):
    """bullets, after checking that the texts fit the cards and the cards fit the slide."""
    for _icon, title, text in items:
        for value, weight, size in ((title, "Bold", 36), (text, "Regular", 28)):
            width = _ruler.textlength(value, font=font(size, weight))
            if width > W - MARGIN - (MARGIN + 140) - 40:
                _fail(f"card {title!r}: {value!r} is {width:.0f} px wide")
    if top + (len(items) - 1) * 176 + 150 > 1040:
        _fail(f"the cards end at {top + (len(items) - 1) * 176 + 150}")
    bullets(image, items, top=top)


def slide(step_id, chapter_title, label):
    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register Auto Setup in the host")
        code(image, MARGIN, 240, W - MARGIN, None, "Program.cs", "csharp", PROGRAM, 28, highlight={3, 4, 5})
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "Where the call is, and where it is not")
        code(image, MARGIN, 240, 1100, None, "Program.cs", "csharp", TEMPLATE_CALL, 26, highlight={3})
        notes(image, [
            ("OrchardCore.Cms.Web", "Registers the module."),
            ("The CMS template", "Does not register it."),
            ("The Features page", "Has no Enable button for it."),
        ], 1150, 240, W - MARGIN)
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "The AutoSetup section")
        code(image, MARGIN, 240, 1130, None, "appsettings.json", "json", SECTION, 26, highlight={3, 4, 6})
        notes(image, [
            ("OrchardCore:AutoSetup", "The configuration section."),
            ("Tenants", "One entry per site."),
            ("Default", "Exactly one entry is required."),
        ], 1180, 240, W - MARGIN)
        return image

    if step_id == "config-2":
        image = slide_base(chapter_title, label, "Setup screen to configuration")
        cards(image, [
            ("1", "What is the name of your site?", "SiteName"),
            ("2", "Super User, User Name", "AdminUsername"),
            ("3", "Super User, Email", "AdminEmail"),
            ("4", "Super User, Password", "AdminPassword"),
        ])
        return image

    if step_id == "config-3":
        image = slide_base(chapter_title, label, "Recipe and time zone")
        cards(image, [
            ("1", "Recipe", "RecipeName: the name of a setup recipe, such as Blog, Agency, or SaaS."),
            ("2", "The name, not the display name", "SaaS is shown as Software as a Service on the Setup screen."),
            ("3", "Default Time Zone", "SiteTimeZone: a time zone identifier, such as America/Los_Angeles."),
        ])
        return image

    if step_id == "config-4":
        image = slide_base(chapter_title, label, "The database")
        cards(image, [
            ("1", "DatabaseProvider", "Sqlite, SqlConnection (SQL Server), MySql, or Postgres."),
            ("2", "DatabaseConnectionString", "Required by every provider except Sqlite."),
            ("3", "DatabaseTablePrefix", "Optional. Lets several sites share one database."),
            ("4", "DatabaseSchema", "Optional. The schema, for providers that support one."),
        ])
        return image

    if step_id == "secrets-1":
        image = slide_base(chapter_title, label, "Environment variables")
        bottom = code(image, MARGIN, 240, W - MARGIN, None, "Environment variables", "bash", ENVIRONMENT, 26)
        notes(image, [("Double underscore", "Separates each level of the section.")], MARGIN, bottom + 50, 930)
        notes(image, [("A number", "Is the index of the Tenants item.")], 990, bottom + 50, W - MARGIN)
        return image

    if step_id == "secrets-2":
        image = slide_base(chapter_title, label, "User secrets")
        bottom = code(image, MARGIN, 240, W - MARGIN, None, "Terminal", "bash", USER_SECRETS, 26)
        notes(image, [("Colons", "Separate the levels of the key.")], MARGIN, bottom + 50, 930)
        notes(image, [("Local development", "User secrets are for your machine.")], 990, bottom + 50, W - MARGIN)
        return image

    if step_id == "tenants-1":
        image = slide_base(chapter_title, label, "More than one tenant")
        code(image, MARGIN, 240, 820, None, "appsettings.json", "json", TENANTS, 26, highlight={6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16})
        notes(image, [
            ("Default first", "Its installation creates the other tenants."),
            ("RequestUrlHost", "Routes requests by host name."),
            ("RequestUrlPrefix", "Routes requests by URL prefix."),
        ], 870, 240, W - MARGIN)
        return image

    if step_id == "tenants-2":
        image = slide_base(chapter_title, label, "AutoSetupPath")
        code(image, MARGIN, 240, 1000, None, "appsettings.json", "json", PATH, 26, highlight={4})
        notes(image, [
            ("Default tenant", "/autosetup"),
            ("Tenant with the prefix harbor", "/harbor/autosetup"),
            ("AutoSetupPath empty", "Any first request installs the tenant."),
        ], 1050, 240, W - MARGIN)
        return image

    if step_id == "check-1":
        image = slide_base(chapter_title, label, "Validation")
        bottom = code(image, MARGIN, 240, W - MARGIN, None, "Log", "html", VALIDATION_LOG, 22)
        cards(image, [
            ("1", "One Default tenant", "Exactly one ShellName is Default. A ShellName uses letters, digits, and underscores."),
            ("2", "Required keys", "SiteName, SiteTimeZone, AdminUsername, AdminEmail, AdminPassword, DatabaseProvider, RecipeName."),
            ("3", "DatabaseConnectionString", "Required when the database provider uses a connection string."),
        ], top=bottom + 40)
        return image

    if step_id == "check-2":
        image = slide_base(chapter_title, label, "Success and failure")
        y = code(image, MARGIN, 240, W - MARGIN, None, "Information log, when the installation succeeds", "html", SUCCESS_LOG, 24)
        y = code(image, MARGIN, y + 30, W - MARGIN, None, "Response, when it fails", "html", FAILURE_RESPONSE, 24)
        code(image, MARGIN, y + 30, W - MARGIN, None, "Error log, when it fails", "html", FAILURE_LOG, 24)
        return image

    if step_id == "lock-1":
        image = slide_base(chapter_title, label, "Several instances, one database")
        y = code(image, MARGIN, 240, W - MARGIN, None, "Program.cs", "csharp", LOCK_PROGRAM, 28)
        code(image, MARGIN, y + 40, W - MARGIN, None, "Environment variables", "bash", LOCK_ENVIRONMENT, 28)
        return image

    if step_id == "lock-2":
        image = slide_base(chapter_title, label, "Lock options")
        bottom = code(image, MARGIN, 240, W - MARGIN, None, "Environment variables", "bash", LOCK_OPTIONS, 26)
        notes(image, [("Milliseconds", "Both values are in milliseconds.")], MARGIN, bottom + 50, 640)
        notes(image, [("Both or neither", "Set both above zero, or neither.")], 690, bottom + 50, 1240)
        notes(image, [("Distributed lock", "Default: 60 seconds each.")], 1290, bottom + 50, W - MARGIN)
        return image

    raise SystemExit(f"No slide for {step_id}")
