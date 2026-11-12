"""The slides of the Content Definition Stores video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (the paths are in recording-plan.md) and trimmed with // ...
"""
from brand import W, draw_code, note_cards, slide_base

TYPES = {
    "ContentDefinitionRecord", "ContentTypeDefinitionRecord", "ContentPartDefinitionRecord",
    "ContentTypePartDefinitionRecord", "ContentPartFieldDefinitionRecord", "IList", "JsonObject", "Document",
    "Task", "IContentDefinitionStore", "DatabaseContentDefinitionStore", "FileContentDefinitionStore",
    "IDocumentManager", "IFileDocumentStore", "IServiceCollection", "StartupBase", "FileDocumentStore",
    "IFileDocumentStore", "IOptions", "ShellOptions", "ShellSettings", "Feature", "FileDocumentStoreAttribute",
}

RECORD = """
public class ContentDefinitionRecord : Document
{
    // ...
    public IList<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
    public IList<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
}

public class ContentTypeDefinitionRecord
{
    // ...
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public JsonObject Settings { get; set; }
    public IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
}

public class ContentPartDefinitionRecord
{
    // ...
    public string Name { get; set; }
    public JsonObject Settings { get; set; }
    public IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }
}
"""

STORE_INTERFACE = """
public interface IContentDefinitionStore
{
    // Loads the record for updating: it is not cached.
    Task<ContentDefinitionRecord> LoadContentDefinitionAsync();

    // Gets the cached record for sharing: it must not be updated.
    Task<ContentDefinitionRecord> GetContentDefinitionAsync();

    // Updates the store with the record, then the cache.
    Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord);
}
"""

DATABASE_STORE = """
public class DatabaseContentDefinitionStore : IContentDefinitionStore
{
    private readonly IDocumentManager<ContentDefinitionRecord> _documentManager;

    public DatabaseContentDefinitionStore(IDocumentManager<ContentDefinitionRecord> documentManager)
    {
        _documentManager = documentManager;
    }

    public Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        => _documentManager.GetOrCreateMutableAsync();

    public Task<ContentDefinitionRecord> GetContentDefinitionAsync()
        => _documentManager.GetOrCreateImmutableAsync();

    public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        => _documentManager.UpdateAsync(contentDefinitionRecord);
}
"""

MANIFEST = """
[assembly: Feature(
    Id = "OrchardCore.Contents.FileContentDefinition",
    Name = "File Content Definition",
    Description = "Stores Content Definition in a local file.",
    Dependencies = ["OrchardCore.Contents"],
    Category = "Content Management"
)]
"""

STARTUP = """
[Feature("OrchardCore.Contents.FileContentDefinition")]
public sealed class FileContentDefinitionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddFileContentDefinitionStore();
    }
}
"""

REGISTRATION = """
public static IServiceCollection AddContentManagement(this IServiceCollection services)
{
    // ...
    services.TryAddScoped<IContentDefinitionStore, DatabaseContentDefinitionStore>();
    // ...
    return services;
}

public static IServiceCollection AddFileContentDefinitionStore(this IServiceCollection services)
{
    services.RemoveAll<IContentDefinitionStore>();
    services.AddScoped<IContentDefinitionStore, FileContentDefinitionStore>();
    return services;
}
"""

FILE_STORE = """
public class FileContentDefinitionStore : IContentDefinitionStore
{
    private readonly IDocumentManager<IFileDocumentStore, ContentDefinitionRecord> _documentManager;

    // ...
    public Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        => _documentManager.GetOrCreateMutableAsync();
}
"""

RECORD_FILE = """
[FileDocumentStore(FileName = "ContentDefinition")]
public class ContentDefinitionRecord : Document
{
    // ...
}
"""

TENANT_PATH = """
// In the constructor
_tenantPath = Path.Combine(
    shellOptions.Value.ShellsApplicationDataPath,
    shellOptions.Value.ShellsContainerName,
    shellSettings.Name) + "/";

// In GetDocumentAsync
var filename = _tenantPath + typeName + ".json";
"""

FILE_SAMPLE = """
{
  "ContentTypeDefinitionRecords": [ // ...
    {
      "Name": "Event",
      "DisplayName": "Event",
      "Settings": {
        "ContentTypeSettings": {
          "Creatable": true,
          "Listable": true,
          "Draftable": true,
          "Versionable": true,
          "Securable": true,
          // ...
        }
        // ...
      },
      "ContentTypePartDefinitionRecords": [
        { "PartName": "Event", "Name": "Event", "Settings": {} }
      ]
    }
  ]
  // ...
}
"""

MISSING_FILE = """
// FileDocumentStore.GetDocumentAsync<T>()
var filename = _tenantPath + typeName + ".json";
if (!File.Exists(filename))
{
    return default;
}
"""

EMPTY_RECORD = """
// FileDocumentStore.GetOrCreateMutableAsync<T>()
var document = await GetDocumentAsync<T>()
    ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
    ?? new T();
"""

PROFILE = """
{
  "name": "FeatureProfiles",
  "FeatureProfiles": {
    "my-profile": {
      "FeatureRules": [
        {
          "Rule": "Exclude",
          "Expression": "OrchardCore.Contents.FileContentDefinition"
        }
        // ...
      ]
    }
  }
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "store-1":
        image = slide_base(chapter_title, label, "A content definition is one document")
        draw_code(image, (96, 240, W - 96, 1040), "Metadata/Records", "csharp", RECORD, types=TYPES, size=22)
        return image

    if step_id == "store-2":
        image = slide_base(chapter_title, label, "The manager goes through a store")
        draw_code(image, (96, 240, W - 96, 760), "IContentDefinitionStore.cs", "csharp", STORE_INTERFACE, types=TYPES, size=28)
        return image

    if step_id == "store-3":
        image = slide_base(chapter_title, label, "The database store is the default")
        draw_code(image, (96, 240, W - 96, 1000), "DatabaseContentDefinitionStore.cs", "csharp", DATABASE_STORE, types=TYPES, size=24)
        return image

    if step_id == "file-1":
        image = slide_base(chapter_title, label, "A feature that replaces the store")
        draw_code(image, (96, 240, W - 96, 600), "OrchardCore.Contents: Manifest.cs", "csharp", MANIFEST, types=TYPES, size=25)
        draw_code(image, (96, 630, W - 96, 1030), "OrchardCore.Contents: Startup.cs", "csharp", STARTUP, types=TYPES, size=25)
        return image

    if step_id == "file-2":
        image = slide_base(chapter_title, label, "The file store takes the place of the database store")
        draw_code(image, (96, 240, W - 96, 900), "ServiceCollectionExtensions.cs", "csharp", REGISTRATION, types=TYPES, size=26)
        return image

    if step_id == "file-3":
        image = slide_base(chapter_title, label, "One file for each tenant")
        draw_code(image, (96, 240, W - 96, 630), "FileContentDefinitionStore.cs", "csharp", FILE_STORE, types=TYPES, size=23)
        draw_code(image, (96, 660, 900, 1030), "ContentDefinitionRecord.cs", "csharp", RECORD_FILE, types=TYPES, size=23)
        draw_code(image, (930, 660, W - 96, 1030), "FileDocumentStore.cs", "csharp", TENANT_PATH, types=TYPES, size=23)
        return image

    if step_id == "file-4":
        image = slide_base(chapter_title, label, "App_Data/Sites/<tenant>/ContentDefinition.json")
        draw_code(image, (96, 240, 1150, 1040), "ContentDefinition.json", "json", FILE_SAMPLE, types=TYPES, size=22)
        note_cards(image, [
            ("Indented JSON", "The same record."),
            ("Saved after the commit", "Admin, migrations, recipes."),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "file-5":
        image = slide_base(chapter_title, label, "No file, no definitions")
        draw_code(image, (96, 240, W - 96, 600), "FileDocumentStore.cs", "csharp", MISSING_FILE, types=TYPES, size=26)
        draw_code(image, (96, 640, W - 96, 1010), "FileDocumentStore.cs", "csharp", EMPTY_RECORD, types=TYPES, size=26)
        return image

    if step_id == "profile-1":
        image = slide_base(chapter_title, label, "Exclude the feature with a feature profile")
        draw_code(image, (96, 240, W - 96, 1010), "FeatureProfiles recipe step", "json", PROFILE, types=TYPES, size=26)
        return image

    raise SystemExit(f"No slide for {step_id}")
