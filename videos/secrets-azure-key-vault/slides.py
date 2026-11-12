"""The slides of the Secrets in Azure Key Vault video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"ISecretStore", "AzureKeyVaultSecretStore", "Secret", "SecretSaveOptions", "SecretInfo", "Task",
         "IEnumerable", "TextSecret", "SecretClient", "SecretClientSettings", "MyService", "WebApplication",
         "ISecretManager"}

STORE = """
public interface ISecretStore
{
    string Name { get; }
    bool IsReadOnly { get; }
    Task<Secret> GetSecretAsync(string name);
    Task SaveSecretAsync(string name, Secret secret, SecretSaveOptions options = null);
    Task RemoveSecretAsync(string name);
    Task<IEnumerable<SecretInfo>> GetSecretInfosAsync();
}

public class AzureKeyVaultSecretStore : ISecretStore
{
    public string Name => "AzureKeyVault";
    public bool IsReadOnly => false;
    // ...
}
"""

STORES = [
    ("1", "Secrets", "The database store, where secrets are kept by default."),
    ("2", "Secrets - Azure Key Vault", "A second store, named AzureKeyVault, with the secrets as vault entries."),
    ("3", "Names", "Settings and code refer to a secret by its name, whichever store keeps it."),
    ("4", "Existing secrets", "Enabling the feature moves nothing: they stay in the database."),
]

PROGRAM = """
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

#pragma warning disable SCME0002
builder.AddKeyedAzureClient<SecretClient, SecretClientSettings>(
    "SecretClient", "AzureClients:SecretClient");
#pragma warning restore SCME0002

builder.Services.AddOrchardCms();
// ...
"""

HOST_CONFIGURATION = """
{
  "AzureClients": {
    "SecretClient": {
      "VaultUri": "<the URI of your vault>",
      "Credential": {
        "CredentialSource": "ManagedIdentityCredential"
      }
    }
  },
  "OrchardCore": {
    "Secrets": {
      "AzureKeyVault": {
        "AzureClient": "SecretClient",
        "NamePrefix": "myapp"
      }
    }
  }
}
"""

CLIENT_SECRET = """
# Supplied by the environment, not by the settings file.
export AzureClients__SecretClient__Credential__ClientSecret=...
"""

ENTRIES = [
    ("1", "Entry name", "myapp-default-payment-apikey- and a 24-character hash; tags hold the exact values."),
    ("2", "Namespace", "Only entries with the configured prefix and tenant are listed."),
    ("3", "Not a boundary", "Code with the vault credentials can read every entry."),
    ("4", "Deletion", "A normal Key Vault delete: the module never purges."),
]

READ_WRITE = """
// Read from the vault only.
var apiKey = await _secretManager.GetSecretAsync<TextSecret>(
    "Payment.ApiKey", "AzureKeyVault");

// Save to the vault. Without a store name, the default store is used.
var secret = new TextSecret { Text = value };
await _secretManager.SaveSecretAsync("Payment.ApiKey", secret, "AzureKeyVault");
"""

SHARED_CLIENT = """
public MyService(
    [FromKeyedServices(AzureKeyVaultSecretStore.SecretClientServiceKey)] SecretClient client)
{
    _client = client;
}
"""

EMULATOR = """
{
  "AzureClients": {
    "SecretClient": {
      "VaultUri": "https://localhost:4997",
      "Credential": {
        "CredentialSource": "EnvironmentCredential",
        "DisableInstanceDiscovery": true
      },
      "Options": {
        "DisableChallengeResourceVerification": true
      }
    }
  }
}
"""

LEFT = (96, 240, 1110, 1030)
RIGHT = (1160, 240, W - 96, 1030)


def slide(step_id, chapter_title, label):
    if step_id == "store-1":
        image = slide_base(chapter_title, label, "A store behind ISecretStore")
        draw_code(image, (96, 240, W - 96, 940), "ISecretStore.cs, AzureKeyVaultSecretStore.cs", "csharp", STORE,
                  types=TYPES, size=26)
        return image

    if step_id == "store-2":
        image = slide_base(chapter_title, label, "Two stores, the same names")
        bullets(image, STORES, active={2, 3})
        return image

    if step_id == "client-1":
        image = slide_base(chapter_title, label, "The host registers the client")
        draw_code(image, (96, 240, 1250, 760), "Program.cs", "csharp", PROGRAM, types=TYPES, size=24)
        note_cards(image, [
            ("Service key", "SecretClient"),
            ("Configuration section", "AzureClients:SecretClient"),
        ], (1300, 240, W - 96, 1030))
        return image

    if step_id == "client-2":
        image = slide_base(chapter_title, label, "The client settings")
        draw_code(image, LEFT, "Configuration", "json", HOST_CONFIGURATION, types=TYPES,
                  highlight=set(range(2, 10)), size=24)
        note_cards(image, [
            ("Vault address", "VaultUri"),
            ("Credential", "A token credential, not an API key"),
            ("Managed identity", "Needs no secret in the configuration"),
        ], RIGHT)
        return image

    if step_id == "client-3":
        image = slide_base(chapter_title, label, "A client secret stays out of the settings file")
        draw_code(image, (96, 240, W - 96, 480), "Environment variable", "bash", CLIENT_SECRET, types=TYPES, size=26)
        note_cards(image, [
            ("A secure configuration source", "For example, an environment variable."),
            ("Not the settings file", "Keep the client secret out of appsettings."),
        ], (96, 540, W - 96, 1030))
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Select the client")
        draw_code(image, LEFT, "Configuration", "json", HOST_CONFIGURATION, types=TYPES,
                  highlight={10, 11, 12, 13, 15, 16, 17}, size=24)
        note_cards(image, [
            ("AzureClient", "The service key of the host"),
            ("An explicit error", "No name, no client, or the reserved key"),
        ], RIGHT)
        return image

    if step_id == "config-2":
        image = slide_base(chapter_title, label, "Name the application")
        draw_code(image, LEFT, "Configuration", "json", HOST_CONFIGURATION, types=TYPES,
                  highlight={10, 11, 12, 14, 15, 16, 17}, size=24)
        note_cards(image, [
            ("NamePrefix", "Defaults to oc"),
            ("One prefix per application", "When applications share a vault"),
        ], RIGHT)
        return image

    if step_id.startswith("names-"):
        image = slide_base(chapter_title, label, "What the vault holds")
        active = {"names-1": {0}, "names-2": {1}, "names-3": {2}, "names-4": {3}}[step_id]
        bullets(image, ENTRIES, active=active)
        return image

    if step_id in ("code-1", "code-2"):
        image = slide_base(chapter_title, label, "Name the store")
        draw_code(image, (96, 240, W - 96, 700), "MyService.cs", "csharp", READ_WRITE, types=TYPES,
                  highlight={1, 2, 3} if step_id == "code-1" else {5, 6, 7}, size=26)
        return image

    if step_id == "code-3":
        image = slide_base(chapter_title, label, "Share the client of the store")
        draw_code(image, (96, 240, W - 96, 640), "MyService.cs", "csharp", SHARED_CLIENT, types=TYPES, size=26)
        return image

    if step_id in ("local-1", "local-2"):
        image = slide_base(chapter_title, label, "An emulator for local development")
        draw_code(image, LEFT, "appsettings.Development.json", "json", EMULATOR, types=TYPES,
                  highlight=None if step_id == "local-1" else {7, 10}, size=24)
        if step_id == "local-1":
            notes = [
                ("A local address", "VaultUri points to localhost"),
                ("An environment credential", "Read from environment variables"),
                ("Two flags", "Needed by the emulator"),
            ]
        else:
            notes = [
                ("Development only", "Keep the flags in development settings"),
                ("The default is false", "Leave them so for production"),
                ("Emulator", "Local development and testing only"),
            ]
        note_cards(image, notes, RIGHT)
        return image

    raise SystemExit(f"No slide for {step_id}")
