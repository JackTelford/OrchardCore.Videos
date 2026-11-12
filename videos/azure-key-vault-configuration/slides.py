"""The slides of the Azure Key Vault configuration video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code
panels hold code of the Orchard Core source (src/OrchardCore/OrchardCore.Configuration.KeyVault) and of its
documentation page (src/docs/reference/modules/KeyVault.Azure/README.md), trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"AzureKeyVaultSecretManager", "KeyVaultSecretManager", "KeyVaultSecret", "ConfigurationManager",
         "TokenCredential", "IConfigurationBuilder", "IConfiguration", "DefaultAzureCredential", "WebApplication",
         "IHost", "Program", "Startup", "Host"}

# Where settings are kept (why-1, why-2).
KEPT = [
    ("1", "appsettings.json files", "Read from files that sit next to the application."),
    ("2", "Environment variables", "Set on every machine that runs the site."),
    ("3", "Azure Key Vault", "Kept in a vault, outside the application and its files."),
]

# The provider and the Secrets module (why-3).
TWO_VAULT_USES = [
    ("1", "Azure Key Vault configuration",
     "A library in the host. It adds the secrets of a vault to the configuration, with no screen."),
    ("2", "Secrets - Azure Key Vault",
     "A feature of the Secrets module. Secrets are created in the admin and kept in a vault."),
]

INSTALL = """
dotnet add package OrchardCore.Configuration.KeyVault
"""

PROGRAM_MINIMAL = """
using OrchardCore.Configuration.KeyVault.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adds the vault after the sources the host already has.
builder.Configuration.AddOrchardCoreAzureKeyVault();

builder.Services
    .AddOrchardCms();

// ...
"""

PROGRAM_GENERIC_HOST = """
using OrchardCore.Configuration.KeyVault.Extensions;

public class Program
{
    // ...

    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddOrchardCoreAzureKeyVault()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .Build();
}
"""

THREE_TARGETS = [
    ("IHostBuilder", "The Generic Host"),
    ("IWebHostBuilder", "The web host"),
    ("ConfigurationManager", "builder.Configuration"),
]

SETTINGS = """
{
  "OrchardCore": {
    "KeyVault": {
      "Azure": {
        "KeyVaultName": "example-vault",
        "VaultURI": "",
        "ReloadInterval": 300
      }
    }
  }
}
"""

ADDRESS_NOTES = [
    ("KeyVaultName", "The name of the vault."),
    ("VaultURI", "Its full address. It takes precedence if both are set."),
]

RELOAD_NOTES = [
    ("ReloadInterval", "Seconds to wait between polls of the vault."),
    ("Empty", "Reloading is disabled."),
]

WHERE_READ = [
    ("1", "Sources registered before the call", "On builder.Configuration: appsettings.json, environment variables, user secrets and others."),
    ("2", "The application configuration only", "The settings are not read from the configuration of a tenant."),
    ("3", "Neither a name nor an address", "The call throws an exception that names the missing setting."),
]

ORDER = [
    ("1", "Sources registered before the call", "The project's appsettings.json files and the other sources of the host."),
    ("2", "Azure Key Vault", "Added at the call. Its values replace the same keys of the project's appsettings.json."),
]

SIGNATURE = """
public static ConfigurationManager AddOrchardCoreAzureKeyVault(
    this ConfigurationManager manager, TokenCredential tokenCredential = null)
{
    // ...
}
"""

DEFAULT_CREDENTIAL = """
private static void AddOrchardCoreAzureKeyVault(
    this IConfigurationBuilder builder, IConfiguration configuration, TokenCredential tokenCredential)
{
    // ...
    tokenCredential ??= new DefaultAzureCredential(includeInteractiveCredentials: true);
    // ...
    builder.AddAzureKeyVault(keyVaultEndpointUri, tokenCredential, configOptions);
}
"""

CREDENTIAL_NOTES = [
    ("1", "Another credential", "Pass a TokenCredential to the method."),
    ("2", "Access", "The identity must be allowed to read the secrets of the vault."),
    ("3", "Development", "Sign in from Visual Studio or Visual Studio Code, as the documentation describes."),
]

SECRET_MANAGER = """
public class AzureKeyVaultSecretManager : KeyVaultSecretManager
{
    public override string GetKey(KeyVaultSecret secret) =>
        secret.Name.Replace("---", "_").Replace("--", ":");
}
"""

HYPHENS = [
    ("--", "Two hyphens", "Become a colon, the separator of configuration keys."),
    ("---", "Three hyphens", "Become an underscore."),
]

EXAMPLES = [
    ("1", "OrchardCore--Shells--Database--ConnectionString", "Becomes OrchardCore:Shells:Database:ConnectionString"),
    ("2", "Part---Name", "Becomes Part_Name"),
    ("3", "Part-Name", "Stays Part-Name"),
]

SCOPES = [
    ("1", "OrchardCore--Media--Azure--ConnectionString", "Every tenant: OrchardCore:Media:Azure:ConnectionString"),
    ("2", "OrchardCore--MyTenant--Media--Azure--ConnectionString",
     "MyTenant only: OrchardCore:MyTenant:Media:Azure:ConnectionString"),
]

CHECKLIST = [
    ("1", "Create the secrets", "In the vault, with hyphens in their names."),
    ("2", "Add the provider", "Reference the package, and call the method in the host."),
    ("3", "Point it at the vault", "Set KeyVaultName or VaultURI."),
    ("4", "Allow the identity", "The identity that runs the site must be able to read the secrets."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "Where settings are kept")
        bullets(image, KEPT, active={0, 1} if step_id == "why-1" else {2})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Two ways to use a vault")
        bullets(image, TWO_VAULT_USES)
        return image

    if step_id == "host-1":
        image = slide_base(chapter_title, label, "Reference the package, then call the method")
        draw_code(image, (96, 240, W - 96, 380), "Terminal", "bash", INSTALL, size=28)
        draw_code(image, (96, 420, W - 96, 960), "Program.cs", "csharp", PROGRAM_MINIMAL, types=TYPES, size=28)
        return image

    if step_id == "host-2":
        image = slide_base(chapter_title, label, "The same method, three targets")
        draw_code(image, (96, 240, 1290, 800), "Program.cs", "csharp", PROGRAM_GENERIC_HOST, types=TYPES, size=22)
        note_cards(image, THREE_TARGETS, (1330, 240, W - 96, 1030))
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Point to the vault")
        draw_code(image, (96, 240, 1000, 790), "appsettings.json", "json", SETTINGS, highlight={5, 6}, size=28)
        note_cards(image, ADDRESS_NOTES, (1040, 300, W - 96, 1030))
        return image

    if step_id == "config-2":
        image = slide_base(chapter_title, label, "Where the settings are read from")
        bullets(image, WHERE_READ)
        return image

    if step_id == "config-3":
        image = slide_base(chapter_title, label, "Reload the values")
        draw_code(image, (96, 240, 1000, 790), "appsettings.json", "json", SETTINGS, highlight={7}, size=28)
        note_cards(image, RELOAD_NOTES, (1040, 300, W - 96, 1030))
        return image

    if step_id == "config-4":
        image = slide_base(chapter_title, label, "Order of the sources")
        bullets(image, ORDER)
        return image

    if step_id == "auth-1":
        image = slide_base(chapter_title, label, "The default credential")
        draw_code(image, (96, 240, W - 96, 520), "AzureKeyVaultConfigurationExtension.cs", "csharp", SIGNATURE,
                  types=TYPES, size=26)
        draw_code(image, (96, 560, W - 96, 1030), "AzureKeyVaultConfigurationExtension.cs", "csharp",
                  DEFAULT_CREDENTIAL, types=TYPES, highlight={5}, size=24)
        return image

    if step_id == "auth-2":
        image = slide_base(chapter_title, label, "Another credential, and access")
        bullets(image, CREDENTIAL_NOTES)
        return image

    if step_id == "names-1":
        image = slide_base(chapter_title, label, "From secret names to configuration keys")
        draw_code(image, (96, 240, W - 96, 560), "AzureKeyVaultSecretManager.cs", "csharp", SECRET_MANAGER,
                  types=TYPES, highlight={3, 4}, size=28)
        bullets(image, HYPHENS, top=610)
        return image

    if step_id == "names-2":
        image = slide_base(chapter_title, label, "Examples")
        bullets(image, EXAMPLES)
        return image

    if step_id == "names-3":
        image = slide_base(chapter_title, label, "For every tenant, or for one")
        bullets(image, SCOPES)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "The steps, in the order you do them")
        bullets(image, CHECKLIST)
        return image

    raise SystemExit(f"No slide for {step_id}")
