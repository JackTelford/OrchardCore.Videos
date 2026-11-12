"""The slides of the web farm video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"WebApplication", "OrchardCoreBuilder", "IBackgroundTask", "IServiceProvider", "CancellationToken",
         "ContentIndexingService", "ContentIndexingBackgroundTask", "Task", "BackgroundTask"}

STATE = [
    ("1", "Tenant settings", "tenants.json and each tenant's appsettings.json are files in App_Data."),
    ("2", "Tenant state", "Each server builds and keeps its own copy of every tenant in memory."),
    ("3", "Lucene indexes", "An index and its position in the task log are files on one server's disk."),
    ("4", "Background tasks", "A scheduled task runs on every server, on its own copy of the schedule."),
]

CONTAINER = [
    ("1", "tenants.json", "At the root of the container: the settings of every tenant."),
    ("2", "appsettings.json", "Optional, at the root: the global tenant configuration."),
    ("3", "Sites/Northwind/appsettings.json", "One file for each tenant, in a folder named after it."),
]

HOW = [
    ("1", "A tenant changes", "An instance reloads, releases, creates or removes a tenant."),
    ("2", "An identifier is stored", "It writes a changed identifier for that tenant to the distributed cache."),
    ("3", "Every instance checks", "Each instance reads the identifiers about once a second."),
    ("4", "The local copy follows", "The instance reloads, releases, loads or removes its own copy."),
]

INDEXING = [
    ("1", "Indexing tasks", "Content changes are recorded in the database that every instance shares."),
    ("2", "Last task processed", "Elasticsearch keeps it in the index. Lucene keeps it in lucene.status.json."),
    ("3", "Catching up", "An instance reads the tasks recorded after its position, and updates its index."),
]

CHECKLIST = [
    ("check", "Shared database", "Every instance uses the same database."),
    ("check", "Shared tenant settings", "Azure Shells Configuration, or Database Shells Configuration."),
    ("check", "Distributed cache and tenants", "Redis Cache with the Distributed Tenants feature."),
    ("check", "Index worker", "The Indexing Worker feature, with Redis Lock for atomic tasks."),
]

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);
// ...
builder.Services
    .AddOrchardCms()
    .AddAzureShellsConfiguration();
"""

SHELLS_SETTINGS = """
{
  "OrchardCore": {
    "Shells": {
      "Azure": {
        "ConnectionString": "<Azure Storage connection string>",
        "ContainerName": "hostcontainer",
        "BasePath": "some/base/path"
      }
    }
  }
}
"""

DATABASE_PROGRAM = """
// Or keep the same settings in a database:
builder.Services
    .AddOrchardCms()
    .AddDatabaseShellsConfiguration();
"""

REDIS_SETTINGS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "localhost:6379"
    }
  }
}
"""

WORKER = """
[BackgroundTask(
    Title = "Indexes Content Updater",
    Schedule = "* * * * *",
    // ...
    LockTimeout = 1000,
    LockExpiration = 300000)]
public sealed class ContentIndexingBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var indexingService = serviceProvider.GetRequiredService<ContentIndexingService>();

        return indexingService.ProcessRecordsForAllIndexesAsync();
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2", "why-3"):
        image = slide_base(chapter_title, label, "State that stays on one server")
        active = {"why-1": None, "why-2": {0, 1}, "why-3": {2, 3}}[step_id]
        bullets(image, STATE, active=active)
        return image

    if step_id == "shared-1":
        image = slide_base(chapter_title, label, "Settings in an Azure Blob Storage container")
        bullets(image, CONTAINER)
        return image

    if step_id == "shared-2":
        image = slide_base(chapter_title, label, "Add the provider to the host")
        draw_code(image, (96, 240, W - 96, 520), "Program.cs", "csharp", PROGRAM, types=TYPES, size=26)
        draw_code(image, (96, 550, W - 96, 1030), "appsettings.json", "json", SHELLS_SETTINGS, types=TYPES, size=24)
        return image

    if step_id == "shared-3":
        image = slide_base(chapter_title, label, "Before the first start")
        draw_code(image, (96, 240, 1010, 640), "Program.cs", "csharp", DATABASE_PROGRAM, types=TYPES, size=24)
        note_cards(image, [
            ("Create the container first", "Orchard Core does not create it."),
            ("Keep it private", "It holds the settings of every tenant."),
            ("MigrateFromFiles", "Copies App_Data files the container lacks."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "redis-1":
        image = slide_base(chapter_title, label, "A distributed cache shared by the farm")
        draw_code(image, (96, 240, 1010, 640), "appsettings.json", "json", REDIS_SETTINGS, types=TYPES, size=26)
        note_cards(image, [
            ("Same on every server", "Every instance uses one Redis server."),
            ("A valid string is required", "Otherwise Redis features stay inactive."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id in ("how-1", "how-2"):
        image = slide_base(chapter_title, label, "How the instances stay in step")
        bullets(image, HOW, active={0, 1} if step_id == "how-1" else {2, 3})
        return image

    if step_id in ("log-1", "log-2", "log-3"):
        image = slide_base(chapter_title, label, "Indexing in a web farm")
        bullets(image, INDEXING, active={int(step_id[-1]) - 1})
        return image

    if step_id in ("worker-1", "worker-2"):
        image = slide_base(chapter_title, label, "The Indexing Worker feature")
        highlight = {1, 2, 3, 7, 13} if step_id == "worker-1" else {1, 5, 6}
        draw_code(image, (96, 240, W - 96, 960), "ContentIndexingBackgroundTask.cs", "csharp", WORKER,
                  types=TYPES, size=28, highlight=highlight)
        return image

    if step_id == "list-1":
        image = slide_base(chapter_title, label, "A web farm checklist")
        bullets(image, CHECKLIST)
        return image

    raise SystemExit(f"No slide for {step_id}")
