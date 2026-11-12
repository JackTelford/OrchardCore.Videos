"""The slides of the "Write a Background Task" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code
panels are taken from the source of Orchard Core, trimmed to what the narration talks about.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, slide_base

TYPES = {
    "ResizedMediaCacheBackgroundTask", "ContentVersionPruningBackgroundTask", "IBackgroundTask", "ILogger",
    "IServiceProvider", "CancellationToken", "Task", "IOptions", "MediaOptions", "IResizedImageCache", "Exception",
    "BackgroundTask", "StartupBase", "IServiceCollection", "IDistributedLock", "ILocalLock", "ILocker",
    "BackgroundTaskSettings", "IBackgroundTaskEventHandler", "BackgroundTaskEventContext", "TaskFailureHandler",
    "IBackgroundTaskMonitor", "BackgroundTaskState", "BackgroundTaskRun", "BackgroundTaskStatus",
    "BackgroundTaskRunResult", "BackgroundTaskRunRequestResult", "BackgroundTaskTrigger", "ShellSettings", "MyService",
    "IReadOnlyList", "DateTime", "TimeSpan", "Startup",
}


def lines_of(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    lines = code.strip("\n").split("\n")
    return {number for number, line in enumerate(lines, 1) if any(needle in line for needle in needles)}


# 1. How background tasks run.

HOW = [
    ("1", "One background service", "It wakes up on every round and visits each running tenant."),
    ("2", "The tasks of a tenant", "The modules enabled for the tenant register them as IBackgroundTask."),
    ("3", "A scope for each run", "The tasks of a tenant run one after another; the next round waits for them."),
    ("4", "The Background Tasks feature", "Optional: the admin pages, and the settings stored in the database."),
]

# 2. Write a task: ResizedMediaCacheBackgroundTask, from OrchardCore.Media.

RESIZED = """
public sealed class ResizedMediaCacheBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;

    public ResizedMediaCacheBackgroundTask(ILogger<ResizedMediaCacheBackgroundTask> logger)
    {
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
        // ...
        var cache = serviceProvider.GetRequiredService<IResizedImageCache>();
        try
        {
            await cache.ClearStaleAsync(mediaOptions.ResizedCacheMaxStale.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean the resized media cache.");
        }
    }
}
"""

# 3. Describe the task: the attributes of two tasks of Orchard Core.

RESIZED_ATTRIBUTE = """
[BackgroundTask(
    Title = "Resized Media Cache Cleaner",
    Schedule = "0 0 * * *",
    Description = "Resized media cache cleanup.")]
public sealed class ResizedMediaCacheBackgroundTask : IBackgroundTask
{
    // ...
}
"""

PRUNING_ATTRIBUTE = """
[BackgroundTask(
    Schedule = "0 0 * * *",
    Title = "Content Version Pruning Background Task",
    Description = "Regularly deletes old non-latest, non-published content item versions.",
    Enable = false,
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class ContentVersionPruningBackgroundTask : IBackgroundTask
{
    // ...
}
"""

# 4. Register the task: the Startup class of OrchardCore.Media.

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...

        // Resized media and remote media caches cleanups.
        services.AddSingleton<IBackgroundTask, ResizedMediaCacheBackgroundTask>();
        services.AddSingleton<IBackgroundTask, RemoteMediaCacheBackgroundTask>();

        // ...
    }
}
"""

# 6. Atomic tasks and web farms: the lock of the background service.

LOCK = """
public static Task<(ILocker locker, bool locked)> TryAcquireBackgroundTaskLockAsync(
    this IDistributedLock distributedLock, BackgroundTaskSettings settings)
{
    if (distributedLock is ILocalLock || !settings.IsAtomic)
    {
        return Task.FromResult<(ILocker, bool)>((null, true));
    }

    return distributedLock.TryAcquireLockAsync(
        settings.Name + "_LOCK",
        TimeSpan.FromMilliseconds(settings.LockTimeout),
        TimeSpan.FromMilliseconds(settings.LockExpiration));
}
"""

IS_ATOMIC = """
public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;
"""

FARM = [
    ("1", "One service on every server", "Each server runs its own background service, with its own copy of the schedule."),
    ("2", "A scheduled task runs on every server", "With three servers, a task scheduled at midnight runs three times."),
    ("3", "The runs can overlap", "Unless the task is atomic, and a distributed lock is registered."),
]

LOCK_NEEDS = [
    ("1", "A distributed lock feature", "Such as Redis Lock. Without one, the lock settings are ignored."),
    ("2", "An expiration above the longest run", "Otherwise the lock can expire while the task is running."),
    ("3", "One server at a time", "An atomic task runs on every server, but never on two at once."),
]

# 7. Configure the service.

CONFIGURATION = """
{
  "OrchardCore": {
    "BackgroundService": {
      "ShellWarmup": false,
      "PollingTime": "00:01:00",
      "MinimumIdleTime": "00:00:10",
      "MaxRecentRuns": 10
    }
  }
}
"""

CONFIGURATION_NOTES = [
    "ShellWarmup: load every tenant at startup.",
    "PollingTime: the time from one round to the next.",
    "MinimumIdleTime: the least wait after a round.",
    "MaxRecentRuns: the runs kept for each task.",
]

# 8. Hook into every run.

HANDLER_INTERFACE = """
public interface IBackgroundTaskEventHandler
{
    Task ExecutingAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);
    Task ExecutedAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);
}
"""

EVENT_CONTEXT = """
public class BackgroundTaskEventContext
{
    // ...
    public string Name { get; }
    public string Tenant { get; }
    public IServiceProvider Services { get; }
    public Exception Exception { get; set; }
    public bool HasException => Exception != null;
}
"""

HANDLER = """
public sealed class TaskFailureHandler : IBackgroundTaskEventHandler
{
    public Task ExecutingAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task ExecutedAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken)
    {
        if (context.HasException)
        {
            // Report context.Exception for context.Name on context.Tenant.
        }

        return Task.CompletedTask;
    }
}
"""

HANDLER_REGISTRATION = """
services.AddScoped<IBackgroundTaskEventHandler, TaskFailureHandler>();
"""

# 9. Read the state from code: IBackgroundTaskMonitor and the documentation's example.

MONITOR = """
public interface IBackgroundTaskMonitor
{
    Task<IReadOnlyList<BackgroundTaskState>> GetStatesAsync(string tenant);
    Task<BackgroundTaskState> GetStateAsync(string tenant, string name);
    Task<BackgroundTaskRunRequestResult> RequestRunAsync(string tenant, string name);
}
"""

MONITOR_NOTES = [
    ("check", "Registered with the background service", "Not available in an application that does not register it."),
]

STATE = """
public class BackgroundTaskState
{
    public BackgroundTaskStatus Status { get; set; }
    public DateTime? NextStartTime { get; set; }
    public BackgroundTaskRun LastRun { get; set; }
    public long RunCount { get; set; }
    public long FailureCount { get; set; }
    // ...
}
"""

RUN = """
public sealed class BackgroundTaskRun
{
    public DateTime StartTime { get; init; }
    public TimeSpan Duration => EndTime - StartTime;
    public BackgroundTaskTrigger Trigger { get; init; }
    public BackgroundTaskRunResult Result { get; init; }
    public string ErrorMessage { get; init; }
    // ...
}
"""

MY_SERVICE = """
public sealed class MyService
{
    private readonly IBackgroundTaskMonitor _backgroundTaskMonitor;
    private readonly ShellSettings _shellSettings;

    public MyService(IBackgroundTaskMonitor backgroundTaskMonitor, ShellSettings shellSettings)
    {
        // ...
    }

    public async Task<bool> HasFailedAsync(string taskName)
    {
        var state = await _backgroundTaskMonitor.GetStateAsync(_shellSettings.Name, taskName);

        return state?.LastRun?.Result == BackgroundTaskRunResult.Failed;
    }

    public Task<BackgroundTaskRunRequestResult> RunNowAsync(string taskName)
        => _backgroundTaskMonitor.RequestRunAsync(_shellSettings.Name, taskName);
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("how-1", "how-2", "how-3"):
        image = slide_base(chapter_title, label, "The background service")
        active = {"how-1": {0, 1}, "how-2": {2}, "how-3": {3}}[step_id]
        bullets(image, HOW, active=active)
        return image

    if step_id in ("write-1", "write-2", "write-3"):
        headings = {
            "write-1": "1. Implement IBackgroundTask",
            "write-2": "2. Constructor and service provider",
            "write-3": "3. A task that catches its errors",
        }
        image = slide_base(chapter_title, label, headings[step_id])
        if step_id == "write-1":
            lit = lines_of(RESIZED, "public sealed class", "public async Task DoWorkAsync")
        elif step_id == "write-2":
            lit = lines_of(RESIZED, "private readonly ILogger", "public ResizedMediaCacheBackgroundTask(", "_logger = logger",
                           "GetRequiredService")
        else:
            lit = set(range(min(lines_of(RESIZED, "try")), max(lines_of(RESIZED, "LogError")) + 2))
        draw_code(image, (96, 236, W - 96, 1062), "ResizedMediaCacheBackgroundTask.cs", "csharp", RESIZED,
                  types=TYPES, highlight=lit, size=22)
        return image

    if step_id in ("attr-1", "attr-2"):
        heading = "BackgroundTask: Title and Description" if step_id == "attr-1" else "BackgroundTask: Schedule"
        image = slide_base(chapter_title, label, heading)
        lit = lines_of(RESIZED_ATTRIBUTE, "Title", "Description") if step_id == "attr-1" else lines_of(
            RESIZED_ATTRIBUTE, "Schedule")
        draw_code(image, (96, 240, W - 96, 610), "ResizedMediaCacheBackgroundTask.cs", "csharp", RESIZED_ATTRIBUTE,
                  types=TYPES, highlight=lit, size=26)
        if step_id == "attr-1":
            cards = [
                ("1", "Title and Description", "What the admin shows for the task."),
                ("2", "No Title", "Built from the class name: Resized Media Cache."),
            ]
            bullets(image, cards, top=640, active={0, 1})
        else:
            cards = [
                ("1", "Schedule", "A cron expression, read in the time zone of the site."),
                ("2", "The default", "Every five minutes in the attribute, every minute without it."),
            ]
            bullets(image, cards, top=640, active={0, 1})
        return image

    if step_id == "attr-3":
        image = slide_base(chapter_title, label, "BackgroundTask: Enable")
        draw_code(image, (96, 240, W - 96, 670), "ContentVersionPruningBackgroundTask.cs", "csharp", PRUNING_ATTRIBUTE,
                  types=TYPES, highlight=lines_of(PRUNING_ATTRIBUTE, "Enable", "Schedule"), size=22)
        cards = [
            ("1", "An invalid cron expression", "The task never runs on schedule."),
            ("2", "Enable = false", "No scheduled runs, but the task can still be run on demand."),
        ]
        bullets(image, cards, top=700, active={0, 1})
        return image

    if step_id == "attr-4":
        image = slide_base(chapter_title, label, "UsePipeline and the admin")
        cards = [
            ("1", "UsePipeline", "Builds the tenant pipeline before the run, so a task can generate URLs."),
            ("2", "Settings saved in the admin", "Replace the values of the attribute for that tenant."),
        ]
        bullets(image, cards)
        return image

    if step_id == "reg-1":
        image = slide_base(chapter_title, label, "Register it in the Startup class")
        draw_code(image, (96, 240, W - 96, 790), "Startup.cs", "csharp", STARTUP, types=TYPES,
                  highlight=lines_of(STARTUP, "AddSingleton"), size=26)
        bullets(image, [("check", "One line for each task", "It runs for every tenant where the feature is enabled.")], top=810)
        return image

    if step_id == "farm-1":
        image = slide_base(chapter_title, label, "Several servers, several services")
        bullets(image, FARM)
        return image

    if step_id == "farm-2":
        image = slide_base(chapter_title, label, "LockTimeout and LockExpiration")
        draw_code(image, (96, 240, W - 96, 790), "DistributedLockExtensions.cs", "csharp", LOCK, types=TYPES,
                  highlight=lines_of(LOCK, "ILocalLock", "TryAcquireLockAsync", "_LOCK", "FromMilliseconds"), size=22)
        draw_code(image, (96, 830, W - 96, 990), "BackgroundTaskSettings.cs", "csharp", IS_ATOMIC, types=TYPES, size=26)
        return image

    if step_id == "farm-3":
        image = slide_base(chapter_title, label, "What the lock needs")
        bullets(image, LOCK_NEEDS, active={0, 1})
        return image

    if step_id == "cfg-1":
        image = slide_base(chapter_title, label, "Configure the background service")
        draw_code(image, (96, 240, 960, 760), "Configuration", "json", CONFIGURATION, types=TYPES, size=26)
        draw = ImageDraw.Draw(image)
        y = 330
        for note in CONFIGURATION_NOTES:
            draw.ellipse((1010, y - 7, 1024, y + 7), fill=GREEN)
            draw.text((1046, y), note, font=font(28), fill=TEXT, anchor="lm")
            y += 96
        return image

    if step_id == "hook-1":
        image = slide_base(chapter_title, label, "IBackgroundTaskEventHandler")
        draw_code(image, (96, 240, W - 96, 520), "IBackgroundTaskEventHandler.cs", "csharp", HANDLER_INTERFACE,
                  types=TYPES, size=26)
        draw_code(image, (96, 560, W - 96, 1000), "BackgroundTaskEventContext.cs", "csharp", EVENT_CONTEXT,
                  types=TYPES, size=26)
        return image

    if step_id == "hook-2":
        image = slide_base(chapter_title, label, "A handler and its registration")
        draw_code(image, (96, 240, W - 96, 830), "TaskFailureHandler.cs", "csharp", HANDLER, types=TYPES, size=24)
        draw_code(image, (96, 870, W - 96, 1010), "Startup.cs", "csharp", HANDLER_REGISTRATION, types=TYPES, size=26)
        return image

    if step_id == "mon-1":
        image = slide_base(chapter_title, label, "IBackgroundTaskMonitor")
        draw_code(image, (96, 240, W - 96, 560), "IBackgroundTaskMonitor.cs", "csharp", MONITOR, types=TYPES, size=26)
        bullets(image, MONITOR_NOTES, top=620)
        return image

    if step_id == "mon-2":
        image = slide_base(chapter_title, label, "What the state holds")
        draw_code(image, (96, 240, 940, 800), "BackgroundTaskState.cs", "csharp", STATE, types=TYPES, size=24)
        draw_code(image, (980, 240, W - 96, 800), "BackgroundTaskRun.cs", "csharp", RUN, types=TYPES, size=24)
        return image

    if step_id == "mon-3":
        image = slide_base(chapter_title, label, "Check a task, or request a run")
        draw_code(image, (96, 240, W - 96, 1030), "MyService.cs", "csharp", MY_SERVICE, types=TYPES,
                  highlight=lines_of(MY_SERVICE, "GetStateAsync", "LastRun", "RequestRunAsync", "RunNowAsync"), size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")

