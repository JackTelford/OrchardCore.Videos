"""The slides of the "Workflows and the Database Session" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from OrchardCMS/OrchardCore and trimmed with // ... to what the narration talks about.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {
    "ISession", "IUpdateModelAccessor", "IUpdateModel", "IDocumentStore", "IStore", "IServiceCollection", "IServiceProvider",
    "IActivityDescription", "IList", "List", "Stack", "ShellScope", "Task", "ActivityExecutionResult", "ActivityRecord",
    "ActivityContext", "WorkflowExecutionContext", "WorkflowStore", "Workflow", "Outcome", "TaskActivity", "StartupBase",
    "ActivityDisplayDriver", "CommitTransactionTask", "CommitTransactionTaskDisplayDriver", "CommitTransactionTaskViewModel",
    "SessionStartup", "ForkTask", "JoinTask", "JoinMode", "NullModelUpdater", "ControllerModelUpdater", "Controller",
    "ActionExecutingContext", "ModelStateDictionary", "JObject", "IEnumerable",
}

SCOPE_CARDS = [
    ("1", "One scope", "A workflow runs in the shell scope of the request or background task that triggers it."),
    ("2", "One session", "Every activity of the run receives the same ISession from that scope."),
    ("3", "One commit", "The shell scope commits the session when it ends, and cancels it on an exception."),
]

SESSION_SCOPE = """
var session = store.CreateSession();
// ...
if (sp == shellScope?.ServiceProvider)
{
    shellScope
        .RegisterBeforeDispose(scope =>
        {
            return scope.ServiceProvider
                .GetRequiredService<IDocumentStore>()
                .CommitAsync();
        })
        .AddExceptionHandler((scope, e) =>
        {
            return scope.ServiceProvider
                .GetRequiredService<IDocumentStore>()
                .CancelAsync();
        });
}
"""

PERSIST = """
private async Task PersistAsync(WorkflowExecutionContext workflowContext)
{
    // ...
    workflowContext.Workflow.State = JObject.FromObject(state, _jsonSerializerOptions);
    await _workflowStore.SaveAsync(workflowContext.Workflow);
    // ...
}
"""

STORE_SAVE = """
public async Task SaveAsync(Workflow workflow)
{
    var isNew = workflow.Id == 0;
    await _session.SaveAsync(workflow);
    // ...
}
"""

COMMIT_TASK = """
public class CommitTransactionTask : TaskActivity<CommitTransactionTask>, IActivityDescription
{
    // ...
    public override IEnumerable<Outcome> GetPossibleOutcomes(/* ... */)
        => Outcome(S["Done"], S["Valid"], S["Invalid"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            return Outcome("Done", "Invalid");
        }

        await _session.SaveChangesAsync(workflowContext.CancellationToken);

        return Outcome("Done", "Valid");
    }
}
"""

ACCESSOR = """
public IUpdateModel ModelUpdater
{
    get
    {
        var updateModel = _httpContextAccessor.HttpContext.Items[s_key] as IUpdateModel;
        return updateModel ?? new NullModelUpdater();
    }
    // ...
}
"""

FILTER = """
public void OnActionExecuting(ActionExecutingContext context)
{
    var controller = context.Controller as Controller;
    if (controller != null)
    {
        // ...
        modelBinderAccessor.ModelUpdater = new ControllerModelUpdater(controller);
    }
}
"""

STARTUP = """
[Feature("OrchardCore.Workflows.Session")]
public sealed class SessionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<CommitTransactionTask, CommitTransactionTaskDisplayDriver>(
            activity => activity.Icon = "fa-solid fa-database");
    }
}
"""

DRIVER = """
public sealed class CommitTransactionTaskDisplayDriver
    : ActivityDisplayDriver<CommitTransactionTask, CommitTransactionTaskViewModel>
{
}
"""

VIEW_MODEL = """
public class CommitTransactionTaskViewModel;
"""

FORK_TASK = """
public class ForkTask : TaskActivity<ForkTask>, IActivityDescription
{
    // ...
    public IList<string> Forks
    {
        get => GetProperty(() => new List<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(/* ... */)
        => Forks.SelectMany(f => Outcome(S[f]));

    public override ActivityExecutionResult Execute(/* ... */)
    {
        return Outcome(Forks);
    }
}
"""

SCHEDULER = """
var scheduled = new Stack<ActivityRecord>();
// ...
scheduled.Push(activity);

while (scheduled.Count > 0)
{
    activity = scheduled.Pop();
    // ...
    result = await activityContext.Activity.ExecuteAsync(workflowContext, activityContext);
    // ...
    foreach (var outcome in outcomes)
    {
        // ...
        scheduled.Push(destinationActivity);
    }
}
"""

JOIN_TASK = """
switch (Mode)
{
    case JoinMode.WaitAll:
        done = inboundTransitions.All(x => branches.Contains(GetTransitionKey(x)));
        break;
    case JoinMode.WaitAny:
        done = inboundTransitions.Any(x => branches.Contains(GetTransitionKey(x)));
        if (done)
        {
            // Remove any inbound blocking activities.
            // ...
        }
        break;
}

if (done)
{
    return Outcome("Joined");
}

return Noop();
"""

PLACE_CARDS = [
    ("1", "After a WaitAll Join", "Every branch has saved its changes to the session, and one commit writes them."),
    ("2", "Invalid", "The task skips its commit and cancels nothing when the model state is invalid."),
]


def lines_with(code, *markers):
    """The 1-based numbers of the lines of a code panel (as draw_code strips it) that hold one of the markers."""
    return {number for number, line in enumerate(code.strip("\n").split("\n"), 1) if any(marker in line for marker in markers)}


def panel(image, x0, y0, x1, title, code, size, highlight=None):
    """A code panel just tall enough for its lines (draw_code starts them 74 px below the top, one line every
    1.42 times the size). Returns the y of its bottom edge."""
    count = len(code.strip("\n").split("\n"))
    y1 = y0 + 74 + (count - 1) * int(size * 1.42) + int(size * 1.3) + 22
    draw_code(image, (x0, y0, x1, y1), title, "csharp", code, types=TYPES, size=size, highlight=highlight)
    return y1


def slide(step_id, chapter_title, label):
    if step_id == "session-1":
        image = slide_base(chapter_title, label, "One scope, one session")
        bullets(image, SCOPE_CARDS, active={0, 1})
        return image

    if step_id == "session-2":
        image = slide_base(chapter_title, label, "The shell scope commits the session")
        panel(image, 96, 240, 1200, "OrchardCoreBuilderExtensions.cs", SESSION_SCOPE, 25)
        note_cards(image, [
            ("Scope ends", "CommitAsync saves the session."),
            ("Exception", "CancelAsync discards it."),
        ], (1240, 300, W - 96, 1000))
        return image

    if step_id == "session-3":
        image = slide_base(chapter_title, label, "The instance is saved, not committed")
        bottom = panel(image, 96, 240, W - 96, "WorkflowManager.cs", PERSIST, 25)
        panel(image, 96, bottom + 30, W - 96, "WorkflowStore.cs", STORE_SAVE, 25)
        ImageDraw.Draw(image).text((96, 985), "No SaveChangesAsync here: the shell scope commits.", font=font(32), fill=MUTED, anchor="lm")
        return image

    if step_id in ("commit-1", "commit-2"):
        image = slide_base(chapter_title, label, "The Commit Transaction task")
        if step_id == "commit-1":
            # The if, its braces and the return of the Invalid outcome: four lines.
            start = min(lines_with(COMMIT_TASK, "if (!_updateModelAccessor"))
            highlight = set(range(start, start + 4))
        else:
            highlight = lines_with(COMMIT_TASK, "SaveChangesAsync", 'return Outcome("Done", "Valid")')
        panel(image, 96, 240, W - 96, "CommitTransactionTask.cs", COMMIT_TASK, 25, highlight=highlight)
        return image

    if step_id == "commit-3":
        image = slide_base(chapter_title, label, "Where the model state comes from")
        bottom = panel(image, 96, 240, W - 96, "ModelBinderAccessorFilter.cs", FILTER, 22)
        panel(image, 96, bottom + 24, W - 96, "LocalModelBinderAccessor.cs", ACCESSOR, 22)
        ImageDraw.Draw(image).text((96, 1025), "NullModelUpdater: an empty ModelState, which is always valid.", font=font(30), fill=MUTED, anchor="lm")
        return image

    if step_id == "commit-4":
        image = slide_base(chapter_title, label, "Registering the task")
        bottom = panel(image, 96, 240, W - 96, "Startup.cs", STARTUP, 22)
        bottom = panel(image, 96, bottom + 24, W - 96, "CommitTransactionTaskDisplayDriver.cs", DRIVER, 22)
        panel(image, 96, bottom + 24, W - 96, "CommitTransactionTaskViewModel.cs", VIEW_MODEL, 22)
        return image

    if step_id == "fork-1":
        image = slide_base(chapter_title, label, "Fork: one outcome for each name")
        panel(image, 96, 240, W - 96, "ForkTask.cs", FORK_TASK, 25)
        return image

    if step_id == "fork-2":
        image = slide_base(chapter_title, label, "The branches run in one loop")
        panel(image, 96, 240, W - 96, "WorkflowManager.cs", SCHEDULER, 25)
        return image

    if step_id == "fork-3":
        image = slide_base(chapter_title, label, "Join: WaitAll or WaitAny")
        panel(image, 96, 240, 1260, "JoinTask.cs", JOIN_TASK, 23)
        note_cards(image, [
            ("WaitAll", "The default."),
            ("WaitAny", "Any one branch."),
            ("Joined", "The outcome."),
        ], (1300, 300, W - 96, 1000))
        return image

    if step_id in ("where-1", "where-2"):
        image = slide_base(chapter_title, label, "Placing the task")
        bullets(image, PLACE_CARDS, active={0} if step_id == "where-1" else {1})
        return image

    raise SystemExit(f"No slide for {step_id}")
