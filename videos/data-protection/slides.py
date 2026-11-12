"""The slides of the Data Protection Keys video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code panels are trimmed excerpts of the documentation and of the Orchard Core source.
"""

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "DirectoryInfo", "ServiceCollection", "IServiceCollection", "IConfigureOptions", "KeyManagementOptions",
    "StartupBase", "BlobClient", "OrchardCoreConstants", "Path", "RedisXmlRepository",
    "RedisKeyManagementOptionsSetup", "RedisDataProtectionStartup", "FeatureAttribute",
}

# Chapter 1: why the keys matter.

PROTECTED = [
    ("1", "Authentication cookies", "Who is signed in."),
    ("2", "Anti-forgery tokens", "The protection of form posts."),
    ("3", "Saved credentials", "Such as an SMTP password, but not user passwords."),
    ("4", "Temporary data", "Protected in the same way."),
]

DEFAULT_KEYS = """
var directory = new DirectoryInfo(Path.Combine(
    options.Value.ShellsApplicationDataPath,
    options.Value.ShellsContainerName,
    settings.Name, "DataProtection-Keys"));

// By default, the key ring is stored in
// the tenant directory of the
// configured App_Data path.
var collection = new ServiceCollection()
    .AddDataProtection()
    .PersistKeysToFileSystem(directory)
    .SetApplicationName(settings.Name)
    // ...
    .Services;
"""

DEPENDS = [
    ("1", "Several servers", "Each server has its own keys: a cookie from one fails on another."),
    ("2", "Restarts", "A folder that does not persist loses the keys, and cookies and tokens fail."),
    ("3", "Stored data", "Secrets and saved credentials are encrypted with the keys."),
    ("4", "Backups", "A database backup alone cannot decrypt them."),
]

# Chapter 2: where the keys can live.

PLACES = [
    ("1", "Local folder", "The default. It needs no configuration."),
    ("2", "Azure Blob Storage", "Distributed Data Protection (Azure Storage)"),
    ("3", "Redis", "Distributed Data Protection (Redis)"),
]

STATES = [
    ("1", "No feature enabled", "The local folder of the tenant holds the key ring."),
    ("2", "Feature enabled and configured", "Azure Blob Storage or Redis holds the key ring."),
    ("3", "Feature enabled, not configured", "The local folder stays, and an entry goes to the log."),
]

# Chapter 4: Azure Blob Storage.

AZURE_SETTINGS = """
{
  "OrchardCore": {
    "DataProtection": {
      "Azure": {
        "ConnectionString": "<your-azure-storage-connection-string>",
        "ContainerName": "dataprotection",
        "BlobName": "",
        "CreateContainer": true
      }
    }
  }
}
"""

AZURE_OPTIONS = [
    ("1", "ConnectionString", "Required. The connection string of the storage account."),
    ("2", "ContainerName", "Default: dataprotection. Always lowercased. Accepts Liquid."),
    ("3", "BlobName", "Optional. Default: Sites/<tenant>/DataProtectionKeys.xml. Accepts Liquid."),
    ("4", "CreateContainer", "Default: true. Set it to false when the container exists."),
]

AZURE_LAYOUT = [
    ("1", "Container", "dataprotection by default, set by ContainerName."),
    ("2", "Folder", "Sites, then the name of the tenant."),
    ("3", "Blob", "DataProtectionKeys.xml: the key ring of the tenant."),
]

AZURE_BLOBS = """
dataprotection/Sites/Default/DataProtectionKeys.xml
dataprotection/Sites/Marketing/DataProtectionKeys.xml
"""

AZURE_TENANT_SETTINGS = """
{
  "OrchardCore": {
    "DataProtection": {
      "Azure": {
        "ConnectionString": "<your-azure-storage-connection-string>",
        "ContainerName": "{{ ShellSettings.Name }}-dataprotection",
        "BlobName": "{{ ShellSettings.Name }}DataProtectionKeys.xml",
        "CreateContainer": true
      }
    }
  }
}
"""

AZURE_STARTUP = """
public override int Order
    => OrchardCoreConstants.ConfigureOrder.AzureDataProtection;

public override void ConfigureServices(IServiceCollection services)
{
    // ...
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        services.RemoveAll<IConfigureOptions<KeyManagementOptions>>();

        services
            .AddDataProtection()
            .PersistKeysToAzureBlobStorage(sp =>
            {
                // ...
                return new BlobClient(
                    options.ConnectionString,
                    options.ContainerName,
                    options.BlobName);
            });
    }
}
"""

# Chapter 5: Redis.

REDIS_SETTINGS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "<your-redis-connection-string>",
      "InstancePrefix": "MyApp:",
      "AllowAdmin": true
    }
  }
}
"""

REDIS_KEY = """
{InstancePrefix}{TenantName}:DataProtection-Keys

MyApp:Default:DataProtection-Keys
"""

PERSISTENCE = [
    ("1", "Durable storage", "The key ring is not a cache file."),
    ("2", "AOF or RDB persistence", "With a backup strategy for the server."),
    ("3", "A warning in the log", "When persistence is not confirmed."),
    ("4", "AllowAdmin", "Needed for the feature to check persistence."),
]

REDIS_STARTUP = """
[Feature("OrchardCore.Redis.DataProtection")]
public sealed class RedisDataProtectionStartup : StartupBase
{
    public override void ConfigureServices(
        IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddTransient<
                IConfigureOptions<KeyManagementOptions>,
                RedisKeyManagementOptionsSetup>();
        }
    }
}
"""

REDIS_SETUP = """
public void Configure(KeyManagementOptions options)
{
    var redisKey =
        $"{redis.InstancePrefix}{_tenant}:DataProtection-Keys";
    // ...
    options.XmlRepository = new RedisXmlRepository(() =>
        {
            // ...
            return redis.Database;
        },
        redisKey);
}
"""

# Chapter 6: check the result.

LOG_ENTRIES = [
    ("!", "Azure: a critical entry", "No connection string was supplied for OrchardCore.DataProtection.Azure."),
    ("!", "Redis: an error entry", "'Redis' features are not active on tenant 'Default': ..."),
]

STORES = [
    ("1", "Azure Blob Storage", "dataprotection/Sites/Default/DataProtectionKeys.xml"),
    ("2", "Redis", "MyApp:Default:DataProtection-Keys"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What Data Protection protects")
        bullets(image, PROTECTED)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The default: a key ring for each tenant")
        draw_code(image, (96, 240, 900, 880), "ServiceCollectionExtensions.cs", "csharp", DEFAULT_KEYS, types=TYPES, size=22)
        note_cards(image, [
            ("Per tenant", "A key ring for each tenant."),
            ("On disk", "App_Data/Sites/Default/DataProtection-Keys"),
            ("No setup", "It works without configuration."),
        ], (940, 240, W - 96, 880))
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What depends on the keys")
        bullets(image, DEPENDS, active={0, 1})
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "What depends on the keys")
        bullets(image, DEPENDS, active={2, 3})
        return image

    if step_id == "places-1":
        image = slide_base(chapter_title, label, "Three places for the key ring")
        bullets(image, PLACES)
        return image

    if step_id == "places-2":
        image = slide_base(chapter_title, label, "Enabled is not the same as configured")
        bullets(image, STATES)
        return image

    if step_id == "azure-1":
        image = slide_base(chapter_title, label, "The Azure Blob Storage settings")
        draw_code(image, (96, 240, W - 96, 790), "appsettings.json", "json", AZURE_SETTINGS, size=28)
        return image

    if step_id == "azure-2":
        image = slide_base(chapter_title, label, "The four settings")
        bullets(image, AZURE_OPTIONS)
        return image

    if step_id == "azure-3":
        image = slide_base(chapter_title, label, "One container, a folder for each tenant")
        bullets(image, AZURE_LAYOUT)
        draw_code(image, (96, 800, W - 96, 1030), "Blobs of two tenants", "bash", AZURE_BLOBS, size=26)
        return image

    if step_id == "azure-4":
        image = slide_base(chapter_title, label, "A container for each tenant")
        draw_code(image, (96, 240, 1240, 790), "appsettings.json", "json", AZURE_TENANT_SETTINGS, size=22)
        note_cards(image, [
            ("Liquid templates", "in two settings only."),
            ("ShellSettings", "is available in the template."),
            ("Lowercased", "for the container name."),
        ], (1280, 240, W - 96, 880))
        return image

    if step_id == "azure-5":
        image = slide_base(chapter_title, label, "How the feature takes over")
        draw_code(image, (96, 240, 1010, 1000), "Startup.cs", "csharp", AZURE_STARTUP, types=TYPES, size=22)
        note_cards(image, [
            ("Runs after", "the default registration."),
            ("Removes", "the default key management setup."),
            ("Persists", "the keys to the blob."),
        ], (1050, 240, W - 96, 880))
        return image

    if step_id == "redis-1":
        image = slide_base(chapter_title, label, "The Redis settings")
        draw_code(image, (96, 240, 980, 700), "appsettings.json", "json", REDIS_SETTINGS, size=24)
        note_cards(image, [
            ("Configuration", "Required. The connection string."),
            ("InstancePrefix", "Optional. A prefix for every key."),
            ("AllowAdmin", "Optional. Allows admin commands."),
        ], (1020, 240, W - 96, 880))
        return image

    if step_id == "redis-2":
        image = slide_base(chapter_title, label, "One Redis key for each tenant")
        draw_code(image, (96, 240, W - 96, 520), "The key of a tenant", "bash", REDIS_KEY, size=30)
        note_cards(image, [
            ("InstancePrefix", "Optional. Joined as is: end it with a colon."),
            ("Tenant", "The name of the tenant, such as Default."),
        ], (96, 570, W - 96, 1000))
        return image

    if step_id == "redis-3":
        image = slide_base(chapter_title, label, "Keep the key ring durable")
        bullets(image, PERSISTENCE)
        return image

    if step_id == "redis-4":
        image = slide_base(chapter_title, label, "How the feature takes over")
        draw_code(image, (96, 240, 900, 1000), "Startup.cs", "csharp", REDIS_STARTUP, types=TYPES, size=22)
        draw_code(image, (940, 240, W - 96, 1000), "RedisKeyManagementOptionsSetup.cs", "csharp", REDIS_SETUP, types=TYPES, size=22)
        return image

    if step_id == "check-1":
        image = slide_base(chapter_title, label, "What the log says")
        bullets(image, LOG_ENTRIES)
        return image

    if step_id == "check-2":
        image = slide_base(chapter_title, label, "What the store holds")
        bullets(image, STORES)
        return image

    raise SystemExit(f"No slide for {step_id}")
