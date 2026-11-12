"""The slides of the "Write a Workflow Activity" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
the source of the Notify Task of the Workflows module (src/OrchardCore.Modules/OrchardCore.Workflows), trimmed with
// ... and with long lines broken, plus the pieces of other modules that the narration names.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "NotifyTask", "TaskActivity", "IActivityDescription", "LocalizationSource", "ActivityCategoryLocalizationSources",
    "LocalizedString", "NotifyType", "WorkflowExpression", "Outcome", "WorkflowExecutionContext", "ActivityContext",
    "ActivityExecutionResult", "LocalizedHtmlString", "INotifier", "IWorkflowExpressionEvaluator",
    "IStringLocalizerFactory", "IStringLocalizer", "HtmlEncoder", "NotifyTaskViewModel", "NotifyTaskDisplayDriver",
    "ActivityDisplayDriver", "Startup", "StartupBase", "IServiceCollection", "IfElseTask", "IfElseTaskViewModel",
    "WorkflowExpressionInput", "IDisplayResult", "UpdateEditorContext", "DataPipelineRunCompletedEvent",
    "EventActivity", "DataPipelineRun", "Dictionary", "ActivityViewModel", "Task", "ContentPublishedEvent",
    "ContentPublishedEventDisplayDriver", "TimerEvent", "TimerEventDisplayDriver", "ActivityRegistration",
    "IEnumerable", "ModelBinding",
}

CONCEPTS = [
    ("1", "Task", "Does some work, then continues with an outcome."),
    ("2", "Event", "Waits for something to happen, and can start a workflow."),
    ("3", "Outcome", "A named exit of an activity, such as Done."),
    ("4", "Transition", "Connects an outcome to the next activity."),
]

PARTS = [
    ("1", "The activity class", "Its names, settings, outcomes and the work it does."),
    ("2", "A display driver and a view model", "Connect the settings to the editor."),
    ("3", "Three views", "Thumbnail, Design and Edit."),
    ("4", "One registration", "A call in the Startup of your module."),
]

SHAPES = [
    ("1", "Thumbnail", "NotifyTask.Fields.Thumbnail.cshtml: the card in the Activities pane"),
    ("2", "Design", "NotifyTask.Fields.Design.cshtml: the body of the activity on the canvas"),
    ("3", "Edit", "NotifyTask.Fields.Edit.cshtml: the settings of the activity"),
]

CLASS_NAMES = """
public class NotifyTask : TaskActivity<NotifyTask>, IActivityDescription
{
    // ...

    public static LocalizationSource DisplayName { get; } =
        LocalizationSource.Create<NotifyTask>("Notify Task");

    public static LocalizationSource CategoryName =>
        ActivityCategoryLocalizationSources.UI;

    public override LocalizedString DisplayText =>
        _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category =>
        _localizerFactory.Localize(CategoryName);

    // ...
}
"""

CLASS_CONSTRUCTOR = """
public class NotifyTask : TaskActivity<NotifyTask>, IActivityDescription
{
    // ...

    public NotifyTask(
        INotifier notifier,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<NotifyTask> localizer,
        HtmlEncoder htmlEncoder)
    {
        // ...
    }

    // ...
}
"""

CLASS_SETTINGS = """
public class NotifyTask : TaskActivity<NotifyTask>, IActivityDescription
{
    // ...

    public NotifyType NotificationType
    {
        get => GetProperty<NotifyType>();
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Message
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    // ...
}
"""

CLASS_WORK = """
public class NotifyTask : TaskActivity<NotifyTask>, IActivityDescription
{
    // ...

    public override IEnumerable<Outcome> GetPossibleOutcomes(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = await _expressionEvaluator.EvaluateAsync(Message, workflowContext, _htmlEncoder);

        // The notification message can contain HTML by design
        await _notifier.AddAsync(NotificationType, new LocalizedHtmlString(nameof(NotifyTask), message));

        return Outcome("Done");
    }
}
"""

VIEW_MODEL = """
public class NotifyTaskViewModel
{
    public NotifyType NotificationType { get; set; }

    [Required]
    public string Message { get; set; }
}
"""

DRIVER = """
public sealed class NotifyTaskDisplayDriver
    : ActivityDisplayDriver<NotifyTask, NotifyTaskViewModel>
{
    protected override void EditActivity(
        NotifyTask activity, NotifyTaskViewModel model)
    {
        model.NotificationType = activity.NotificationType;
        model.Message = activity.Message.Expression;
    }

    protected override void UpdateActivity(
        NotifyTaskViewModel model, NotifyTask activity)
    {
        activity.NotificationType = model.NotificationType;
        activity.Message = new WorkflowExpression<string>(model.Message);
    }
}
"""

THUMBNAIL_VIEW = """
<h4 class="card-title"><i class="fa-solid fa-circle-info" aria-hidden="true"></i>@T["Notify"]</h4>
<p>@T["Display a notification."]</p>
"""

DESIGN_VIEW = """
@model ActivityViewModel<NotifyTask>
@using OrchardCore.DisplayManagement.Notify
@{
    string className = "info-circle";

    // ...
}
<header>
    <h4>
        <i class="fa-solid fa-@className" aria-hidden="true"></i>
        @Model.Activity.GetTitleOrDefault(() => T["{0} Notification", Model.Activity.NotificationType])
    </h4>
</header>
<em>&quot;@Model.Activity.Message&quot;</em>
"""

EDIT_VIEW = """
@using OrchardCore.DisplayManagement.Notify
@using OrchardCore.Workflows.ViewModels
@model NotifyTaskViewModel

<div class="ocat-wrapper" asp-validation-class-for="NotificationType">
    <label asp-for="NotificationType" class="ocat-label">@T["Notification Type"]</label>
    <div class="ocat-end">
        <select asp-for="NotificationType" class="form-select" asp-items="Html.GetEnumSelectList<NotifyType>()"></select>
        <span asp-validation-for="NotificationType"></span>
        <span class="hint">@T["The notification type."]</span>
    </div>
</div>

<div class="ocat-wrapper" asp-validation-class-for="Message">
    <label asp-for="Message" class="ocat-label ocat-label-required">@T["Message"]</label>
    <div class="ocat-end">
        <input type="text" asp-for="Message" class="form-control code" />
        <span asp-validation-for="Message"></span>
        <span class="hint">@T["The message to show. With Liquid support."]</span>
    </div>
</div>
"""

EXPRESSION_VIEW_MODEL = """
public class IfElseTaskViewModel
{
    public WorkflowExpressionInput Condition { get; set; } = new();
}
"""

EXPRESSION_DRIVER = """
public override async Task<IDisplayResult> UpdateAsync(IfElseTask activity, UpdateEditorContext context)
{
    // ...

    activity.Condition = _expressionValidator.Validate<bool>(
        model.Condition, context.Updater.ModelState, Prefix, nameof(model.Condition),
        new() { Label = S["Condition"], Required = true });

    // ...

    return Edit(activity, context);
}
"""

EXPRESSION_VIEW = """
@{
    var conditionEditor = await Factory.CreateWorkflowExpressionEditorAsync(Html, m => m.Condition, editor =>
    {
        editor.Label = T["Condition"].Value;
        editor.Hint = T["An expression that evaluates to true or false."].Value;
        editor.Required = true;
        editor.Multiline = false;
        editor.Examples[WorkflowExpressionSyntaxes.Literal] = "true";
        editor.Examples[WorkflowExpressionSyntaxes.JavaScript] = "input(\\"Count\\") > 0";
        editor.Examples[WorkflowExpressionSyntaxes.Liquid] = "{{ Workflow.Properties.Count > 0 }}";
    });
}
@await DisplayAsync(conditionEditor)
"""

REGISTER_STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-bell");

        // ...
    }
}
"""

REGISTER_MANIFEST = """
[assembly: Feature(
    Id = "OrchardCore.Workflows.Timers",
    Name = "Timer Workflows Activities",
    Description = "Provides timer-based services and activities.",
    Dependencies = ["OrchardCore.Workflows"],
    Category = "Workflows"
)]
"""

REGISTER_REQUIRE = """
[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<ContentPublishedEvent, ContentPublishedEventDisplayDriver>();
        // ...
    }
}
"""

EVENT_CLASS = """
public sealed class DataPipelineRunCompletedEvent : EventActivity, IActivityDescription
{
    // ...

    public override string Name => nameof(DataPipelineRunCompletedEvent);

    // ...

    public override IEnumerable<Outcome> GetPossibleOutcomes(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Done"]);
    }

    public override ActivityExecutionResult Resume(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        // ...

        return Outcome("Done");
    }
}
"""

EVENT_TRIGGER = """
public async Task CompletedAsync(DataPipelineRun run)
{
    var input = new Dictionary<string, object> { [RunDataPipelineTask.RunKey] = ToDictionary(run) };

    // ...

    await _workflowManager.TriggerEventAsync(nameof(DataPipelineRunCompletedEvent), input, correlationId: run.PipelineId);
}
"""

LEFT, RIGHT, TOP = 96, W - 96, 240


def height(code, size):
    """The height of a code panel: the window bar, the lines, and the bottom margin."""
    lines = len(code.strip("\n").split("\n"))
    return 74 + lines * int(size * 1.42) + 34


def panel(image, top, title, language, code, size, left=LEFT, right=RIGHT, highlight=None):
    """Draws a code panel at top, sized to its code, and returns the y where the next panel can start."""
    bottom = top + height(code, size)
    draw_code(image, (left, top, right, bottom), title, language, code, types=TYPES, size=size, highlight=highlight)
    return bottom + 30


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "Tasks, events and outcomes")
        bullets(image, CONCEPTS, active={0, 1} if step_id == "idea-1" else {2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What a task with settings needs")
        bullets(image, PARTS)
        return image

    if step_id == "class-1":
        image = slide_base(chapter_title, label, "1. Names and category")
        panel(image, TOP, "NotifyTask.cs", "csharp", CLASS_NAMES, 26)
        return image

    if step_id == "class-2":
        image = slide_base(chapter_title, label, "2. Services from the constructor")
        panel(image, TOP, "NotifyTask.cs", "csharp", CLASS_CONSTRUCTOR, 26)
        return image

    if step_id == "class-3":
        image = slide_base(chapter_title, label, "3. Settings")
        panel(image, TOP, "NotifyTask.cs", "csharp", CLASS_SETTINGS, 26)
        return image

    if step_id == "class-4":
        image = slide_base(chapter_title, label, "4. Outcomes and work")
        panel(image, TOP, "NotifyTask.cs", "csharp", CLASS_WORK, 24)
        return image

    if step_id == "driver-1":
        image = slide_base(chapter_title, label, "The display driver and its view model")
        panel(image, TOP, "NotifyTaskDisplayDriver.cs", "csharp", DRIVER, 22, left=LEFT, right=1080)
        panel(image, TOP, "NotifyTaskViewModel.cs", "csharp", VIEW_MODEL, 22, left=1110, right=RIGHT)
        return image

    if step_id == "shapes-1":
        image = slide_base(chapter_title, label, "Three views, named after the class")
        bullets(image, SHAPES)
        return image

    if step_id in ("views-1", "views-2"):
        first = step_id == "views-1"
        heading = "The Thumbnail view" if first else "The Design view"
        image = slide_base(chapter_title, label, heading)
        next_top = panel(image, TOP, "NotifyTask.Fields.Thumbnail.cshtml", "cshtml", THUMBNAIL_VIEW, 22,
                         highlight=None if first else set())
        panel(image, next_top, "NotifyTask.Fields.Design.cshtml", "cshtml", DESIGN_VIEW, 22,
              highlight=set() if first else None)
        return image

    if step_id == "views-3":
        image = slide_base(chapter_title, label, "The Edit view")
        panel(image, TOP, "NotifyTask.Fields.Edit.cshtml", "cshtml", EDIT_VIEW, 22)
        return image

    if step_id == "expr-1":
        image = slide_base(chapter_title, label, "An expression in the view model and the driver")
        next_top = panel(image, TOP, "IfElseTaskViewModel.cs", "csharp", EXPRESSION_VIEW_MODEL, 22)
        panel(image, next_top, "IfElseTaskDisplayDriver.cs", "csharp", EXPRESSION_DRIVER, 22)
        return image

    if step_id == "expr-2":
        image = slide_base(chapter_title, label, "The expression editor in the Edit view")
        panel(image, TOP, "IfElseTask.Fields.Edit.cshtml", "cshtml", EXPRESSION_VIEW, 22)
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Registering the activity")
        next_top = panel(image, TOP, "Startup.cs", "csharp", REGISTER_STARTUP, 24)
        note_cards(image, [
            ("The icon is optional", "A Font Awesome class; without it, the category's icon, or a generic one."),
        ], (LEFT, next_top + 10, RIGHT, next_top + 200))
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "Where the registration goes")
        next_top = panel(image, TOP, "Manifest.cs", "csharp", REGISTER_MANIFEST, 22)
        panel(image, next_top, "Startup.cs", "csharp", REGISTER_REQUIRE, 22)
        return image

    if step_id == "event-1":
        image = slide_base(chapter_title, label, "An event activity")
        panel(image, TOP, "DataPipelineRunCompletedEvent.cs", "csharp", EVENT_CLASS, 22)
        return image

    if step_id == "event-2":
        image = slide_base(chapter_title, label, "Triggering the event")
        next_top = panel(image, TOP, "WorkflowDataPipelineRunHandler.cs", "csharp", EVENT_TRIGGER, 22)
        note_cards(image, [
            ("Starts and resumes", "Starts the workflows that begin with the event, and resumes those that wait for it."),
        ], (LEFT, next_top + 20, RIGHT, next_top + 200))
        return image

    raise SystemExit(f"No slide for {step_id}")
