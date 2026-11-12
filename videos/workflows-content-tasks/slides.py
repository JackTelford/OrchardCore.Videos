"""The slides of the "Workflows: Create, Update and Publish Content Automatically" video. assemble.py calls slide() for
each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The C# panels
are excerpts of the Orchard Core source (OrchardCore.Contents, Workflows folder), trimmed with comments.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Task", "IEnumerable", "WorkflowExecutionContext", "ContentItem", "ContentEventContext", "Dictionary",
         "ContentsHandler", "ContentHandlerBase", "IContent", "ActivityContext", "ActivityExecutionResult", "Outcome",
         "InvalidOperationException", "VersionOptions", "JsonNode", "JsonMergeSettings", "MergeArrayHandling",
         "UpdateContentTask", "PublishContentTask", "ContentEventConstants", "ContentPublishedEvent",
         "ContentCreatedEvent", "CreateContentTask"}

# From OrchardCore.Contents/Workflows/Handlers/ContentsHandler.cs.
HANDLER = """
private Task<IEnumerable<WorkflowExecutionContext>> TriggerWorkflowEventAsync(string name, ContentItem contentItem)
{
    var contentEvent = new ContentEventContext()
    {
        Name = name,
        ContentType = contentItem.ContentType,
        ContentItemId = contentItem.ContentItemId,
        ContentItemVersionId = contentItem.ContentItemVersionId,
    };

    var input = new Dictionary<string, object>
    {
        { ContentEventConstants.ContentItemInputKey, contentItem },
        { ContentEventConstants.ContentEventInputKey, contentEvent },
    };

    return _workflowManager.TriggerEventAsync(name, input, correlationId: contentItem.ContentItemId);
}
"""

# The value typed in Content Properties of Create Content Task.
CREATE_PROPERTIES = """
{
    "DisplayText": "Announcement: {{ Workflow.Input.ContentItem.DisplayText }}",
    "TitlePart": {
        "Title": "Announcement: {{ Workflow.Input.ContentItem.DisplayText }}"
    }
}
"""

# From OrchardCore.Contents/Workflows/Activities/UpdateContentTask.cs, ExecuteAsync.
UPDATE_MERGE = """
contentItem = await ContentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
// ...
contentItem.Merge(JsonNode.Parse(contentProperties), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
// ...
await ContentManager.UpdateAsync(contentItem);
// ...
var result = await ContentManager.ValidateAsync(contentItem);
"""

UPDATE_OUTCOME = """
var contentItemId = (await GetContentItemIdAsync(workflowContext))
    ?? throw new InvalidOperationException(/* ... */);
// ...
if (result.Succeeded)
{
    if (!inlineEventOfSameContentItemId)
    {
        if (Publish)
        {
            await ContentManager.PublishAsync(contentItem);
        }
        else
        {
            await ContentManager.SaveDraftAsync(contentItem);
        }
    }

    // ...
    return Outcome("Done");
}
"""

# From OrchardCore.Contents/Workflows/Activities/ContentActivity.cs.
LOOKUP = """
protected virtual async Task<IContent> GetContentAsync(WorkflowExecutionContext workflowContext)
{
    IContent content;

    if (!string.IsNullOrWhiteSpace(Content.Expression))
    {
        // ...
    }
    else
    {
        // If no expression was provided, see if the content item was provided as an input or as a property.
        content = workflowContext.Input.GetValue<IContent>(ContentEventConstants.ContentItemInputKey)
            ?? workflowContext.Properties.GetValue<IContent>(ContentEventConstants.ContentItemInputKey);
    }

    if (content?.ContentItem?.ContentItemId != null)
    {
        return content;
    }

    return null;
}
"""

# From OrchardCore.Contents/Workflows/Activities/PublishContentTask.cs.
PUBLISH = """
public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    => Outcome(S["Published"], S["Noop"]);

public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
{
    var content = (await GetContentAsync(workflowContext))
        ?? throw new InvalidOperationException(/* ... */);

    if (!content.HasDraft())
    {
        return Outcome("Noop");
    }

    // ...
    await ContentManager.PublishAsync(contentItem);

    return Outcome("Published");
}
"""

# From CreateContentTask.cs and PublishContentTask.cs.
GUARDS = """
// CreateContentTask
if (InlineEvent.IsStart && InlineEvent.ContentType == ContentType)
{
    if (InlineEvent.Name == nameof(ContentCreatedEvent))
    {
        throw new InvalidOperationException(/* ... */);
    }

    // ...
}

// PublishContentTask
if (InlineEvent.IsStart && InlineEvent.ContentType == contentItem.ContentType && InlineEvent.Name == nameof(ContentPublishedEvent))
{
    throw new InvalidOperationException(/* ... */);
}
"""

RECIPE_STEP = """
{
  "name": "WorkflowType",
  "data": [ { "Name": "Announce published articles", "Activities": [ /* ... */ ], "Transitions": [ /* ... */ ] } ]
}
"""

RECIPE_ACTIVITY = """
{
  "ActivityId": "publishannouncement",
  "Name": "PublishContentTask",
  "X": 1080,
  "Y": 40,
  "IsStart": false,
  "Properties": {
    "Content": {
      "Expression": "property(\\"ContentItem\\")"
    }
  }
}
"""

RECIPE_TRANSITION = """
{
  "SourceActivityId": "updateannouncement",
  "SourceOutcomeName": "Done",
  "DestinationActivityId": "publishannouncement"
}
"""

EVENTS = [
    ("1", "Events start a workflow", "Created, Updated, Save Draft, Published, Unpublished, Deleted and Versioned"),
    ("2", "Tasks work on content items", "Create, Retrieve, Update, Publish, Unpublish and Delete"),
]

TASKS = [
    ("1", "Create Content Task", "Outcomes: Done, Failed"),
    ("2", "Update Content Task", "Outcomes: Done, Failed"),
    ("3", "Publish Content Task", "Outcomes: Published, Noop"),
    ("4", "Retrieve, Unpublish and Delete Content Task", "Outcomes: Retrieved. Unpublished, Noop. Deleted, Noop."),
]

CREATE_OUTCOMES = [
    ("check", "Done", "The item is valid. The task creates it, and stores it as the ContentItem property."),
    ("!", "Failed", "The item is not valid. The validation result becomes the last result."),
]

FAILURES = [
    ("1", "A guard, or a missing Content Item ID, throws", "By default, the workflow instance is faulted at that activity."),
    ("2", "Invalid content takes Failed", "Create Content Task and Update Content Task. Connect it to another activity."),
    ("3", "No draft to publish takes Noop", "Publish Content Task has nothing to do, and the workflow goes on."),
]


def slide(step_id, chapter_title, label):
    if step_id == "concepts-1":
        image = slide_base(chapter_title, label, "Content events and tasks")
        bullets(image, EVENTS, active={0})
        return image

    if step_id == "concepts-2":
        image = slide_base(chapter_title, label, "Every content event carries two inputs")
        draw_code(image, (96, 240, W - 96, 1000), "ContentsHandler.cs", "csharp", HANDLER, types=TYPES, size=22)
        return image

    if step_id == "concepts-3":
        image = slide_base(chapter_title, label, "Content tasks and their outcomes")
        bullets(image, TASKS)
        return image

    if step_id == "properties-1":
        image = slide_base(chapter_title, label, "Content Properties: Liquid, then JSON")
        draw_code(image, (96, 240, W - 96, 560), "Content Properties", "json", CREATE_PROPERTIES, types=TYPES, size=25)
        note_cards(image, [
            ("Liquid first", "Workflow.Input holds the ContentItem and the ContentEvent."),
            ("Then JSON", "The result is merged into an empty Announcement."),
        ], (96, 600, W - 96, 1030))
        return image

    if step_id == "properties-2":
        image = slide_base(chapter_title, label, "Create Content Task outcomes")
        bullets(image, CREATE_OUTCOMES)
        return image

    if step_id == "merge-1":
        image = slide_base(chapter_title, label, "Update Content Task: load, merge, save")
        draw_code(image, (96, 240, W - 96, 760), "UpdateContentTask.cs", "csharp", UPDATE_MERGE, types=TYPES, size=22)
        return image

    if step_id == "merge-2":
        image = slide_base(chapter_title, label, "Update Content Task: ID and Publish")
        draw_code(image, (96, 240, W - 96, 1030), "UpdateContentTask.cs", "csharp", UPDATE_OUTCOME, types=TYPES, size=24)
        return image

    if step_id == "lookup-1":
        image = slide_base(chapter_title, label, "Which content item does a task use?")
        draw_code(image, (96, 240, W - 96, 1030), "ContentActivity.cs", "csharp", LOOKUP, types=TYPES, size=22,
                  highlight={9, 10, 11, 12, 13, 14})
        return image

    if step_id == "lookup-2":
        image = slide_base(chapter_title, label, "Publish Content Task")
        draw_code(image, (96, 240, W - 96, 900), "PublishContentTask.cs", "csharp", PUBLISH, types=TYPES, size=22)
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "Guards against infinite loops")
        draw_code(image, (96, 240, W - 96, 840), "CreateContentTask.cs and PublishContentTask.cs", "csharp", GUARDS,
                  types=TYPES, size=22)
        return image

    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "Failures and outcomes")
        bullets(image, FAILURES)
        return image

    if step_id == "export-1":
        image = slide_base(chapter_title, label, "A workflow as a recipe step")
        draw_code(image, (96, 240, W - 96, 480), "Recipe.json", "json", RECIPE_STEP, size=24)
        draw_code(image, (96, 520, 1050, 1030), "An activity", "json", RECIPE_ACTIVITY, size=24)
        draw_code(image, (1090, 520, W - 96, 800), "A transition", "json", RECIPE_TRANSITION, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
