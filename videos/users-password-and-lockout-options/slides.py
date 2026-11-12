"""The slides of the Password and Lockout Options video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IdentityOptions", "IServiceCollection", "StartupBase", "IShellConfiguration", "TimeSpan", "PasswordOptions",
         "LockoutOptions", "Startup", "WebApplication"}

PASSWORD_STARTUP = """
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 3;
            options.Password.RequiredLength = 6;
        });
    }
}
"""

LOCKOUT_STARTUP = """
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
        });
    }
}
"""

PROGRAM = """
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    .ConfigureServices(tenantServices =>
        tenantServices.Configure<IdentityOptions>(options =>
        {
            // The same Password and Lockout options.
            options.Lockout.MaxFailedAccessAttempts = 3;
        }));

// ...
"""

CONFIGURATION_STARTUP = """
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
            _shellConfiguration.GetSection("IdentityOptions").Bind(options));
    }
}
"""

APPSETTINGS = """
{
  "OrchardCore": {
    "IdentityOptions": {
      "Password": {
        "RequireDigit": false,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": false,
        "RequiredUniqueChars": 3,
        "RequiredLength": 6
      },
      "Lockout": {
        "AllowedForNewUsers": true,
        "DefaultLockoutTimeSpan": "00:05:00",
        "MaxFailedAccessAttempts": 3
      }
    }
  }
}
"""

OPTIONS = [
    ("1", "Password", "Character rules, RequiredUniqueChars and RequiredLength."),
    ("2", "Lockout", "MaxFailedAccessAttempts, DefaultLockoutTimeSpan and AllowedForNewUsers."),
    ("3", "Defaults", "Orchard Core sets neither, so the defaults of ASP.NET Core Identity apply."),
]

DEFAULTS = [
    ("6", "Required length", "A password has at least six characters."),
    ("check", "Character rules", "A digit, a lowercase letter, an uppercase letter and a non-alphanumeric character."),
    ("5", "Failed attempts", "Five failed attempts are allowed before an account is locked out."),
    ("5", "Lockout time", "A lockout lasts five minutes, and the users you create can be locked out."),
]

USES = [
    ("1", "Setup", "The Setup screen describes the password rules in a popover."),
    ("2", "Add User", "The Generate password button builds a password that follows the rules."),
    ("3", "Create, change, reset", "A password is checked against the rules wherever it is set."),
    ("4", "Sign-in", "The sign-in form counts failed attempts and enforces the lockout."),
]


def slide(step_id, chapter_title, label):
    if step_id == "options-1":
        image = slide_base(chapter_title, label, "The options of ASP.NET Core Identity")
        bullets(image, OPTIONS)
        return image

    if step_id == "options-2":
        image = slide_base(chapter_title, label, "The defaults")
        bullets(image, DEFAULTS)
        return image

    if step_id == "options-3":
        image = slide_base(chapter_title, label, "Where Orchard Core uses the options")
        bullets(image, USES)
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "Password options in ConfigureServices")
        draw_code(image, (96, 240, 1110, 1030), "Startup.cs", "csharp", PASSWORD_STARTUP, types=TYPES, size=24)
        note_cards(image, [
            ("One call", "Configure<IdentityOptions>"),
            ("Password", "One line for each rule"),
            ("An example", "Choose your own values"),
        ], (1150, 240, W - 96, 1030))
        return image

    if step_id == "code-2":
        image = slide_base(chapter_title, label, "Password options in ConfigureServices")
        draw_code(image, (96, 240, 1110, 1030), "Startup.cs", "csharp", PASSWORD_STARTUP, types=TYPES, size=24,
                  highlight={11, 12, 13, 14, 15, 16})
        note_cards(image, [
            ("Require options", "A character rule, on or off"),
            ("RequiredUniqueChars", "Different characters needed"),
            ("RequiredLength", "Minimum password length"),
        ], (1150, 240, W - 96, 1030))
        return image

    if step_id in ("lockout-1", "lockout-2"):
        image = slide_base(chapter_title, label, "Lockout options in ConfigureServices")
        draw_code(image, (96, 240, W - 96, 980), "Startup.cs", "csharp", LOCKOUT_STARTUP, types=TYPES, size=28,
                  highlight={11} if step_id == "lockout-2" else None)
        return image

    if step_id == "where-1":
        image = slide_base(chapter_title, label, "In the web project")
        draw_code(image, (96, 240, 1110, 880), "Program.cs", "csharp", PROGRAM, types=TYPES, size=26)
        note_cards(image, [
            ("Orchard Core builder", "ConfigureServices"),
            ("Runs for each tenant", "No module needed"),
        ], (1150, 240, W - 96, 1030))
        return image

    if step_id == "where-2":
        image = slide_base(chapter_title, label, "Read the options from configuration")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", CONFIGURATION_STARTUP, types=TYPES, size=22)
        return image

    if step_id == "where-3":
        image = slide_base(chapter_title, label, "The values in appsettings.json")
        draw_code(image, (96, 240, 1000, 1030), "appsettings.json", "json", APPSETTINGS, types=TYPES, size=26)
        note_cards(image, [
            ("Section", "OrchardCore, IdentityOptions"),
            ("Environment variables", "OrchardCore__IdentityOptions__"),
            ("Each tenant", "Its own configuration on top"),
        ], (1040, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
