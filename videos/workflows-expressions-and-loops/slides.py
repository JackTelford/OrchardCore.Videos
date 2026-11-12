"""The slides of the Workflows in Depth video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, MUTED, PANEL, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"IWorkflowExpressionProvider", "IWorkflowExpressionManager", "IGlobalMethodProvider", "IfElseTask",
         "OutcomeMethodProvider", "UpperCaseExpressionProvider", "ForEachTask", "WorkflowExpression",
         "WorkflowExecutionContext", "WorkflowExpressionEvaluationContext", "ActivityContext",
         "ActivityExecutionResult", "GlobalMethod", "LocalizationSource", "Task", "Options", "TimeSpan",
         "IList", "IEnumerable", "Action", "CultureInfo", "Type", "WorkflowExpressionSyntaxes", "TaskActivity",
         "Outcome", "IServiceCollection"}

SYNTAXES = [
    ("1", "Literal", "The text is the value itself, converted to the type the setting expects."),
    ("2", "Liquid", "A Liquid template, rendered with the data of the workflow."),
    ("3", "JavaScript", "An expression that the engine of the Scripting module evaluates."),
]

STORED = """
"Condition": {
  "Expression": "property('item') == 'Coffee'",
  "Syntax": "JavaScript"
}
"""

VARIABLES = """
"Variables": [
  {
    "Name": "coffee",
    "TypeName": "number",
    "DefaultValue": 0
  },
  {
    "Name": "summary",
    "TypeName": "string",
    "IsOutput": true
  }
]
"""

# What the designer calls a type, and the name a workflow definition stores.
TYPE_NAMES = [
    ("Text", "string"),
    ("Number", "number"),
    ("Yes or no", "boolean"),
    ("Date and time", "datetime"),
    ("Object", "object"),
    ("List", "array"),
    ("Content item", "contentItem"),
    ("Any", "any"),
]

READ_WRITE = """
// Read a variable, then write one.
variable('coffee') + 1;
setVariable('coffee', 3);
"""

LIQUID = """
Coffee items: {{ Workflow.Variables.coffee }}
"""

SCRIPT_RESULT = """
setOutcome('Done');
variable('total') >= 20 ? 0.1 : 0;
"""

OUTPUT_BINDINGS = """
"OutputBindings": {
  "Result": "discount"
}
"""

STATE_SCRIPT = """
// Available Outcomes: Draft, Review, Published
setOutcome(variable('state'));
"""

PATHS = [
    ("1", "If/Else", "Evaluates its Condition, and takes the True or False outcome."),
    ("2", "Script", "Lists its own outcomes in Available Outcomes, and calls setOutcome."),
]

LOOPS = [
    ("1", "For Each", "Items is the list. Looping Variable names the property of the current item."),
    ("2", "For Loop", "From, To and Step are numbers. To is not included."),
    ("3", "While Loop", "Condition: the loop goes on while it is true."),
]

LOOP_WIRING = [
    ("1", "Iterate", "Connect it to the first activity to repeat."),
    ("2", "Back to the loop", "Connect the last activity of the body to the loop itself."),
    ("3", "Done", "Connect it to the activity that follows the loop."),
]

FOR_EACH = """
public override async Task<ActivityExecutionResult> ExecuteAsync(
    WorkflowExecutionContext workflowContext, ActivityContext activityContext)
{
    var items = await EvaluateItemsAsync(workflowContext);
    var count = items.Count;

    if (Index < count)
    {
        var current = Current = items[Index];
        // ...
        workflowContext.Properties[LoopVariableName] = current;
        workflowContext.LastResult = current;
        Index++;

        return Outcome("Iterate");
    }
    else
    {
        Index = 0;
        return Outcome("Done");
    }
}
"""

PROVIDER = """
public sealed class UpperCaseExpressionProvider : IWorkflowExpressionProvider
{
    public static string Name => "UpperCase";

    public static LocalizationSource DisplayName { get; } =
        LocalizationSource.Create<UpperCaseExpressionProvider>("Upper case");

    public static string EditorLanguage => "plaintext";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression,
        WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
        => Task.FromResult((T)Convert.ChangeType(expression.Expression?.ToUpperInvariant(),
            typeof(T), CultureInfo.InvariantCulture));

    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => [];
}

services.AddWorkflowExpressionProvider<UpperCaseExpressionProvider>();
"""

IF_ELSE = """
public class IfElseTask : TaskActivity<IfElseTask>, IActivityDescription
{
    // ...

    public WorkflowExpression<bool> Condition
    {
        get => GetProperty(() => new WorkflowExpression<bool>());
        set => SetProperty(value);
    }

    // ...

    public override async Task<ActivityExecutionResult> ExecuteAsync(
        WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var result = await _expressionManager.EvaluateAsync(
            WorkflowExpressionSyntaxes.Resolve(Condition, LiquidCondition.Expression, Syntax),
            workflowContext);

        return Outcome(result ? "True" : "False");
    }
}
"""

OUTCOME_PROVIDER = """
// The Script activity creates one for each evaluation, with the list that collects the outcomes.
public class OutcomeMethodProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _setOutcomeMethod;

    public OutcomeMethodProvider(IList<string> outcomes)
    {
        _setOutcomeMethod = new GlobalMethod
        {
            Name = "setOutcome",
            Method = serviceProvider => (Action<string>)(name => outcomes.Add(name)),
        };
    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return new[] { _setOutcomeMethod };
    }
}
"""

JINT_OPTIONS = """
using Jint;

services.Configure<Jint.Options>(options =>
{
    options.MaxStatements(10_000);
    options.TimeoutInterval(TimeSpan.FromSeconds(5));
});
"""

SCRIPT_ERROR = """
// There is no Order input, so reading its Total fails.
input("Order").Total
"""


def _table(image, rows, x, y, title_left, title_right, split):
    """Two columns of text, with a header, drawn with the fonts and colors of the slides."""
    draw = ImageDraw.Draw(image)
    head, body = font(26, "Bold"), font(30)
    draw.text((x, y), title_left, font=head, fill=MUTED, anchor="lm")
    draw.text((x + split, y), title_right, font=head, fill=MUTED, anchor="lm")
    y += 30
    draw.line((x, y, x + split + 330, y), fill=ACCENT, width=3)
    for left, right in rows:
        y += 54
        draw.text((x, y), left, font=body, fill=TEXT, anchor="lm")
        draw.text((x + split, y), right, font=body, fill=ACCENT, anchor="lm")
        draw.line((x, y + 27, x + split + 330, y + 27), fill=BORDER, width=1)


def slide(step_id, chapter_title, label):
    if step_id == "syntaxes-1":
        image = slide_base(chapter_title, label, "Three syntaxes for an expression")
        bullets(image, SYNTAXES)
        return image

    if step_id == "syntaxes-2":
        image = slide_base(chapter_title, label, "An expression and its syntax")
        draw_code(image, (96, 240, 1000, 520), "Condition", "json", STORED, size=26)
        note_cards(image, [
            ("Literal", "true"),
            ("Liquid", "{{ Workflow.Properties.Count > 0 }}"),
            ("JavaScript", 'input("Count") > 0'),
        ], (1040, 240, W - 96, 900))
        return image

    if step_id == "types-1":
        image = slide_base(chapter_title, label, "Variable types")
        draw_code(image, (96, 240, 940, 800), "Variables", "json", VARIABLES, size=26)
        _table(image, TYPE_NAMES, 1000, 290, "In the designer", "Stored as", 400)
        ImageDraw.Draw(image).text((1000, 830), "Content item needs the Contents feature.", font=font(22), fill=MUTED, anchor="lm")
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "Reading and writing variables")
        draw_code(image, (96, 240, 940, 560), "JavaScript", "javascript", READ_WRITE, size=26)
        draw_code(image, (980, 240, W - 96, 560), "Liquid", "html", LIQUID, size=26)
        note_cards(image, [("Set Variable", "Stores the result of an expression in the variable you name.")],
                   (96, 620, W - 96, 800))
        return image

    if step_id == "access-2":
        image = slide_base(chapter_title, label, "Binding an output to a variable")
        draw_code(image, (96, 240, 940, 520), "Script", "javascript", SCRIPT_RESULT, size=26)
        draw_code(image, (980, 240, W - 96, 520), "Activity properties", "json", OUTPUT_BINDINGS, size=26)
        note_cards(image, [("Outputs tab", "Store in: the variable that receives the Result of the Script.")],
                   (96, 580, W - 96, 760))
        return image

    if step_id == "paths-1":
        image = slide_base(chapter_title, label, "Outcomes choose the path")
        bullets(image, PATHS)
        return image

    if step_id == "paths-2":
        image = slide_base(chapter_title, label, "A Script activity with its own outcomes")
        draw_code(image, (96, 240, 1100, 520), "Script", "javascript", STATE_SCRIPT, size=26)
        note_cards(image, [
            ("Available Outcomes", "A comma-separated list of names."),
            ("setOutcome", "Adds one of those names."),
        ], (1140, 240, W - 96, 700))
        return image

    if step_id == "loops-1":
        image = slide_base(chapter_title, label, "Three loops")
        bullets(image, LOOPS)
        return image

    if step_id == "loops-2":
        image = slide_base(chapter_title, label, "Closing the loop")
        bullets(image, LOOP_WIRING)
        return image

    if step_id == "loops-3":
        image = slide_base(chapter_title, label, "What a loop does on each pass")
        draw_code(image, (96, 240, W - 96, 1040), "ForEachTask.cs", "csharp", FOR_EACH, types=TYPES, size=22)
        return image

    if step_id == "extend-1":
        image = slide_base(chapter_title, label, "Add a syntax")
        draw_code(image, (96, 240, W - 96, 1040), "UpperCaseExpressionProvider.cs", "csharp", PROVIDER, types=TYPES, size=22)
        return image

    if step_id == "extend-2":
        image = slide_base(chapter_title, label, "Evaluate an expression in an activity")
        draw_code(image, (96, 240, W - 96, 1040), "IfElseTask.cs", "csharp", IF_ELSE, types=TYPES, size=23)
        return image

    if step_id == "extend-3":
        image = slide_base(chapter_title, label, "Where setOutcome comes from")
        draw_code(image, (96, 240, W - 96, 1000), "OutcomeMethodProvider.cs", "csharp", OUTCOME_PROVIDER, types=TYPES, size=24)
        return image

    if step_id == "extend-4":
        image = slide_base(chapter_title, label, "Limits for JavaScript")
        draw_code(image, (96, 240, W - 96, 600), "Startup.cs", "csharp", JINT_OPTIONS, types=TYPES, size=26)
        note_cards(image, [("A script that reaches a limit", "Faults the workflow at the activity that ran it.")],
                   (96, 660, W - 96, 840))
        return image

    if step_id == "extend-5":
        image = slide_base(chapter_title, label, "When a script fails")
        draw_code(image, (96, 240, W - 96, 480), "Expression", "javascript", SCRIPT_ERROR, size=26)
        note_cards(image, [
            ("By default", "The error is logged, and the expression returns its default value."),
            ("Fault the workflow on script errors", "A setting of the workflow that faults the instance instead."),
        ], (96, 540, W - 96, 980))
        return image

    raise SystemExit(f"No slide for {step_id}")
