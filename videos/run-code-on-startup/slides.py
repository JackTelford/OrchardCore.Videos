"""The slides of the "Run Code When a Tenant Starts" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the documentation page of the guide and from the Orchard Core source, trimmed with // ... .
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"IModularTenantEvents", "ModularTenantEvents", "ILogger", "MyStartupTaskService", "Task", "StartupBase",
         "IServiceCollection", "ShellRemovingContext", "AutomaticDataMigrations",
         "PreloadSiteSettingsTenantEventHandler", "MediaBlobContainerTenantEvents", "TempDirCleanerService",
         "BlobContainerClient", "RequestFailedException", "Exception", "Startup"}

# src/OrchardCore/OrchardCore.Abstractions/Modules/IModularTenantEvents.cs
INTERFACE = """
using OrchardCore.Environment.Shell.Removing;

namespace OrchardCore.Modules;

public interface IModularTenantEvents
{
    Task ActivatingAsync();
    Task ActivatedAsync();
    Task TerminatingAsync();
    Task TerminatedAsync();
    Task RemovingAsync(ShellRemovingContext context);
}
"""

# src/docs/guides/run-code-on-startup/README.md
HANDLER = """
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

public class MyStartupTaskService : ModularTenantEvents
{
    private readonly ILogger<MyStartupTaskService> _logger;

    public MyStartupTaskService(ILogger<MyStartupTaskService> logger)
    {
        _logger = logger;
    }

    public override Task ActivatingAsync()
    {
        _logger.LogInformation("A tenant has been activated.");

        return Task.CompletedTask;
    }
}
"""

# The registration line is the one of the documentation page; the class is the shape of the Startup class of a module.
STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IModularTenantEvents, MyStartupTaskService>();
    }
}
"""

# src/OrchardCore/OrchardCore.Data/Migration/AutomaticDataMigrations.cs
MIGRATIONS = """
public sealed class AutomaticDataMigrations : ModularTenantEvents
{
    // ...
    public override Task ActivatingAsync()
    {
        // ...
        return dataMigrationManager.UpdateAllFeaturesAsync();
    }
}
"""

# src/OrchardCore.Modules/OrchardCore.Settings/Services/PreloadSiteSettingsTenantEventHandler.cs
PRELOAD = """
internal sealed class PreloadSiteSettingsTenantEventHandler : ModularTenantEvents
{
    // ...
    public override async Task ActivatedAsync()
    {
        // ...
        _ = await _siteService.GetSiteSettingsAsync();
    }
}
"""

# src/OrchardCore.Modules/OrchardCore.Media.Azure/Services/MediaBlobContainerTenantEvents.cs
REMOVING = """
public override async Task RemovingAsync(ShellRemovingContext context)
{
    // ...
    var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
    if (_options.RemoveContainer)
    {
        try
        {
            var response = await blobContainer.DeleteIfExistsAsync();
            // ...
        }
        catch (RequestFailedException ex)
        {
            // ...
            context.ErrorMessage = S["Failed to remove the Azure Media Storage Container '{0}'.", _options.ContainerName];
            context.Error = ex;
        }
    }
}
"""

# src/OrchardCore.Modules/OrchardCore.Media/Services/TempDirCleanerService.cs
CLEANER = """
public override async Task ActivatedAsync()
{
    if (_shellSettings.IsRunning())
    {
        try
        {
            // ...
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while cleaning temporary media folder.");
        }
    }
}
"""

# What the console of a terminal shows after the first request (the documentation page of the guide).
CONSOLE = """
info: MyStartupTaskService[0]
      A tenant has been activated.
"""

WHAT_RUNS = [
    ("1", "Startup class", "Registers the services and the middleware of a module for each tenant."),
    ("2", "Tenant event handler", "Runs your own code at key moments in the life of a tenant."),
]

OWN_HANDLERS = [
    ("1", "Run data migrations", "Updates the features whose data needs an update."),
    ("2", "Preload the site settings", "So that they can be read synchronously afterwards."),
    ("3", "Create a storage container", "For the media files of a tenant that are kept in Azure."),
    ("4", "Empty a temporary folder", "The one used by media fields."),
]

LAZY = [
    ("1", "The application starts", "By default, no handler runs."),
    ("2", "A tenant handles its first request", "The tenant activates, and its handlers run."),
    ("3", "The request is processed", "It starts once every handler has completed."),
]

ORDER = [
    ("1", "ActivatingAsync", "In the order of registration, which follows the dependencies of the modules."),
    ("2", "ActivatedAsync", "In the reverse order, after every ActivatingAsync has completed."),
    ("3", "TerminatingAsync", "In the order of registration."),
    ("4", "TerminatedAsync and RemovingAsync", "In the reverse order."),
]

AGAIN = [
    ("1", "A second request", "Nothing is logged: the tenant is already activated."),
    ("2", "A feature is enabled or disabled", "The tenant is built again, and activates on its next request."),
    ("3", "The tenant is reloaded", "The same: the handlers run again on the next request."),
]

RELEASED = [
    ("1", "The tenant is released", "For example, when a feature is enabled or the tenant is reloaded."),
    ("2", "The work already using it finishes", "Requests that are running are not cut."),
    ("3", "The handlers are called", "TerminatingAsync in the order of registration, TerminatedAsync in reverse."),
]


def slide(step_id, chapter_title, label):
    if step_id == "when-1":
        image = slide_base(chapter_title, label, "A Startup class and a tenant event handler")
        bullets(image, WHAT_RUNS)
        return image

    if step_id == "when-2":
        image = slide_base(chapter_title, label, "What the handlers of Orchard Core do")
        bullets(image, OWN_HANDLERS)
        return image

    if step_id == "when-3":
        image = slide_base(chapter_title, label, "Tenants are loaded lazily")
        bullets(image, LAZY)
        return image

    if step_id == "write-1":
        image = slide_base(chapter_title, label, "The IModularTenantEvents interface")
        draw_code(image, (96, 240, 1150, 900), "IModularTenantEvents.cs", "csharp", INTERFACE, types=TYPES, size=28)
        note_cards(image, [
            ("Starts", "ActivatingAsync, ActivatedAsync"),
            ("Released", "TerminatingAsync, TerminatedAsync"),
            ("Removed", "RemovingAsync"),
        ], (1190, 240, W - 96, 1030))
        return image

    if step_id == "write-2":
        image = slide_base(chapter_title, label, "Inherit from ModularTenantEvents")
        draw_code(image, (96, 240, W - 96, 1030), "MyStartupTaskService.cs", "csharp", HANDLER, types=TYPES, size=26,
                  highlight={5, 14, 15, 16, 17, 18, 19, 20})
        return image

    if step_id == "write-3":
        image = slide_base(chapter_title, label, "Services come from the tenant")
        draw_code(image, (96, 240, W - 96, 1030), "MyStartupTaskService.cs", "csharp", HANDLER, types=TYPES, size=26,
                  highlight={5, 6, 7, 8, 9, 10, 11, 12, 20})
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register it in the Startup class")
        draw_code(image, (96, 240, W - 96, 770), "Startup.cs", "csharp", STARTUP, types=TYPES, size=30)
        draw = ImageDraw.Draw(image)
        notes = [
            "The service type is IModularTenantEvents.",
            "Every handler registered this way is called.",
        ]
        y = 850
        for note in notes:
            draw.ellipse((106, y - 7, 120, y + 7), fill=GREEN)
            draw.text((142, y), note, font=font(34), fill=TEXT, anchor="lm")
            y += 76
        return image

    if step_id in ("order-1", "order-2"):
        image = slide_base(chapter_title, label, "The order of the events")
        bullets(image, ORDER, active={0} if step_id == "order-1" else {1, 2, 3})
        return image

    if step_id == "order-3":
        image = slide_base(chapter_title, label, "Both are used by Orchard Core")
        draw_code(image, (96, 240, W - 96, 630), "AutomaticDataMigrations.cs", "csharp", MIGRATIONS, types=TYPES, size=24)
        draw_code(image, (96, 650, W - 96, 1030), "PreloadSiteSettingsTenantEventHandler.cs", "csharp", PRELOAD,
                  types=TYPES, size=24)
        return image

    if step_id == "run-1":
        image = slide_base(chapter_title, label, "The first request activates the tenant")
        draw_code(image, (96, 240, W - 96, 480), "Console", "bash", CONSOLE, types=TYPES, size=32)
        note_cards(image, [
            ("Category", "The class of the handler: MyStartupTaskService."),
            ("Message", "The text that the handler logged."),
        ], (96, 540, W - 96, 1030))
        return image

    if step_id == "run-2":
        image = slide_base(chapter_title, label, "The handlers run again when the tenant is built again")
        bullets(image, AGAIN)
        return image

    if step_id == "stop-1":
        image = slide_base(chapter_title, label, "Terminating a released tenant")
        bullets(image, RELEASED)
        return image

    if step_id in ("remove-1", "remove-2"):
        image = slide_base(chapter_title, label, "Removing a tenant")
        draw_code(image, (96, 240, W - 96, 1030), "MediaBlobContainerTenantEvents.cs", "csharp", REMOVING, types=TYPES,
                  size=24, highlight={1, 4, 5, 9} if step_id == "remove-1" else {1, 12, 13, 14, 15, 16, 17})
        return image

    if step_id in ("care-1", "care-2"):
        image = slide_base(chapter_title, label, "Errors and the state of the tenant")
        draw_code(image, (96, 240, W - 96, 1030), "TempDirCleanerService.cs", "csharp", CLEANER, types=TYPES, size=30,
                  highlight={5, 6, 7, 8, 9, 10, 11, 12} if step_id == "care-1" else {1, 3, 4, 13})
        return image

    raise SystemExit(f"No slide for {step_id}")
