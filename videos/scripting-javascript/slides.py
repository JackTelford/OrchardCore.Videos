"""The slides of the "Scripting: JavaScript in Workflows and Recipes" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source and trimmed with // ... (see recording-plan.md for the file of each panel).
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {
    "IGlobalMethodProvider", "GlobalMethod", "IIdGenerator", "IdGeneratorMethod", "IEnumerable", "Startup", "StartupBase",
    "IServiceCollection", "MyGlobalMethodProvider", "ContentMethodsProvider", "Func", "Task", "IContent", "IScriptingManager",
    "CancellationToken", "ContextMethodProvider", "Options", "TimeSpan", "IServiceProvider",
}

PREFIXES = [
    ("1", "A prefix, a colon and the code", "js: new Date().toISOString()"),
    ("2", "js: the JavaScript engine", "Evaluates JavaScript with Jint."),
    ("3", "file: the file engine", "file:text('Snippets/footer.html') reads the text of a file."),
    ("check", "Always enabled", "The Scripting module has no feature to turn on."),
]

PLACES = [
    ("1", "Recipes", "Any text value of a step, such as [js: uuid()]."),
    ("2", "Workflows", "The Script Task, and every expression set to the JavaScript syntax."),
    ("3", "Layer rules", "The Javascript condition, which returns true or false."),
    ("4", "External logins", "The Users module can run a script when someone signs in."),
]

RECIPE_VALUES = """
{
  "name": "content",
  "Data": [
    {
      // A script is a text value between square brackets.
      "ContentItemId": "[js: uuid()]",
      "ContentType": "RawHtml",
      "DisplayText": "Footer",
      // ...
      "RawHtml": {
        "Content": {
          // The file prefix loads the text of a file next to the recipe.
          "Html": "[file:text('Snippets/footer.html')]"
        }
      }
    }
  ]
}
"""

RECIPE_METHODS = """
{
  // Computed the first time a script reads it, then reused by every step.
  "variables": {
    "landingContentItemId": "[js:uuid()]"
  },
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: variables('landingContentItemId')]",
          "Owner": "[js: parameters('AdminUserId')]",
          "DisplayText": "[js: configuration('Admin:AdminUrlPrefix', 'Admin')]"
          // ...
        }
      ]
    }
  ]
}
"""

REGISTERED = [
    ("check", "Always available", "log, protect, encrypt, decrypt, base64, html, gzip, uuid"),
    ("1", "Contents", "newContentItem, createContentItem, updateContentItem, deleteContentItem"),
    ("2", "Layers and Queries", "isHomepage, isAnonymous, isAuthenticated, isInRole, url, culture, executeQuery"),
]

SCOPED = [
    ("1", "Recipe", "variables, parameters, configuration"),
    ("2", "Workflow", "workflow, workflowId, input, output, property, setProperty, variable, setVariable, lastResult"),
    ("3", "Script Task", "setOutcome"),
]

ID_GENERATOR = """
public class IdGeneratorMethod : IGlobalMethodProvider
{
    private static readonly GlobalMethod s_uuid = new()
    {
        Name = "uuid",
        Method = serviceProvider => () =>
        {
            var idGenerator = serviceProvider.GetRequiredService<IIdGenerator>();
            return idGenerator.GenerateUniqueId();
        },
    };

    public IEnumerable<GlobalMethod> GetMethods()
    {
        yield return s_uuid;
    }
}
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGlobalMethodProvider, MyGlobalMethodProvider>();
    }
}
"""

STARTUP_NOTES = [
    "One provider for the tenant, registered when the module's feature is enabled.",
    "The factory runs when a script first reads the name, not when the engine starts.",
    "It receives the services of the evaluation that reads it.",
]

CONTENT_METHODS = """
public sealed class ContentMethodsProvider : IGlobalMethodProvider
{
    // ...
    public ContentMethodsProvider()
    {
        _newContentItemMethod = new GlobalMethod
        {
            Name = "newContentItem",
            Method = serviceProvider => (Func<string, IContent>)((contentType) =>
                NewContentItemAsync(serviceProvider, contentType).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<string, Task<IContent>>)(contentType =>
                NewContentItemAsync(serviceProvider, contentType)),
        };

        // ...
    }
}
"""

EVALUATE = """
public async Task<object> GetTimestampAsync(
    IScriptingManager scriptingManager,
    CancellationToken cancellationToken)
{
    // The prefix selects the engine. A string with no known prefix comes back unchanged.
    return await scriptingManager.EvaluateAsync(
        "js: new Date().toISOString()", null, null, null, cancellationToken);
}
"""

EVALUATE_NOTES = [
    "The registered global methods are added to the scope.",
    "The manager releases the scope in a finally block, even when the script throws.",
    "Called from an engine directly, dispose the scope yourself.",
]

SCOPED_PROVIDER = """
// ScriptExternalLoginEventHandler.cs, Users module
private const string ContextMethodName = "externalLoginContext";

dynamic evaluationResult = await _scriptingManager.EvaluateAsync(
    script, null, null, [CreateContextMethodProvider(context)]);

private sealed class ContextMethodProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _globalMethod;
    public ContextMethodProvider(string serializedContext)
    {
        _globalMethod = new GlobalMethod
        {
            Name = ContextMethodName,
            Method = _ => (Func<string>)(() => serializedContext),
        };
    }

    public IEnumerable<GlobalMethod> GetMethods() => [_globalMethod];
}
"""

JINT_OPTIONS = """
services.Configure<Jint.Options>(options =>
{
    options.MaxStatements(10_000);
    options.TimeoutInterval(TimeSpan.FromSeconds(5));
});
"""

FAILURES = [
    ("1", "A script throws, or returns the wrong type", "In a workflow, a default value is used and the error is logged."),
    ("2", "A limit stops the script, or it is cancelled", "The workflow faults at the activity that was running it."),
    ("3", "Fault the workflow on script errors", "A workflow setting: any script error faults the instance."),
]

SAFEGUARDS = [
    ("check", "Stack overflow guard", "Unbounded recursion raises a JavaScript error that the script can catch."),
    ("check", "Engine pool", "Each tenant reuses idle engines. EnginePoolSize sets how many; 0 turns reuse off."),
]


def lines_of(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    lines = code.strip("\n").split("\n")
    return {number for number, line in enumerate(lines, 1) if any(needle in line for needle in needles)}


def notes(image, texts, y, step=64):
    """Short sentences under a code panel, each with a green dot."""
    draw = ImageDraw.Draw(image)
    for text in texts:
        draw.ellipse((100, y - 7, 114, y + 7), fill=GREEN)
        draw.text((136, y), text, font=font(30), fill=TEXT, anchor="lm")
        y += step


def slide(step_id, chapter_title, label):
    if step_id == "run-1":
        image = slide_base(chapter_title, label, "A script is a prefix and some code")
        bullets(image, PREFIXES)
        return image

    if step_id == "run-2":
        image = slide_base(chapter_title, label, "Where scripts are evaluated")
        bullets(image, PLACES)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Scripts in a recipe")
        draw_code(image, (96, 240, W - 96, 1030), "recipe.json", "json", RECIPE_VALUES, types=TYPES, size=26,
                  highlight=lines_of(RECIPE_VALUES, "A script is", "[js: uuid()]", "The file prefix", "[file:text"))
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Variables and the recipe methods")
        draw_code(image, (96, 240, W - 96, 1030), "recipe.json", "json", RECIPE_METHODS, types=TYPES, size=26)
        return image

    if step_id == "methods-1":
        image = slide_base(chapter_title, label, "Methods that modules register")
        bullets(image, REGISTERED)
        return image

    if step_id == "methods-2":
        image = slide_base(chapter_title, label, "Methods scoped to one evaluation")
        bullets(image, SCOPED)
        return image

    if step_id == "add-1":
        image = slide_base(chapter_title, label, "1. Implement IGlobalMethodProvider")
        draw_code(image, (96, 240, W - 96, 1030), "IdGeneratorMethod.cs", "csharp", ID_GENERATOR, types=TYPES, size=28)
        return image

    if step_id == "add-2":
        image = slide_base(chapter_title, label, "2. Register it in your module's Startup")
        draw_code(image, (96, 240, W - 96, 620), "Startup.cs", "csharp", STARTUP, types=TYPES, size=28)
        notes(image, STARTUP_NOTES, 720, step=84)
        return image

    if step_id == "add-3":
        image = slide_base(chapter_title, label, "3. Add an asynchronous variant")
        draw_code(image, (96, 240, W - 96, 1030), "ContentMethodsProvider.cs", "csharp", CONTENT_METHODS, types=TYPES, size=26,
                  highlight=lines_of(CONTENT_METHODS, 'Name = "newContentItem"', "Method = serviceProvider", "AsyncMethod = serviceProvider",
                                     "(Func<string, Task<IContent>>)", "(Func<string, IContent>)", "NewContentItemAsync(serviceProvider, contentType)"))
        return image

    if step_id == "eval-1":
        image = slide_base(chapter_title, label, "Evaluate a script from C#")
        draw_code(image, (96, 240, W - 96, 720), "ScriptRunner.cs", "csharp", EVALUATE, types=TYPES, size=28)
        notes(image, EVALUATE_NOTES, 810, step=84)
        return image

    if step_id == "eval-2":
        image = slide_base(chapter_title, label, "Pass data with a scoped method provider")
        draw_code(image, (96, 240, W - 96, 1030), "ScriptExternalLoginEventHandler.cs", "csharp", SCOPED_PROVIDER, types=TYPES, size=23)
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "Limit what a script can do")
        draw_code(image, (96, 240, 1010, 640), "Startup.cs", "csharp", JINT_OPTIONS, types=TYPES, size=26)
        note_cards(image, [
            ("No limit by default", "Set one when others write scripts."),
            ("One options instance", "It serves every engine of the tenant."),
            ("More helpers", "LimitMemory and CancellationToken."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "When a script fails")
        bullets(image, FAILURES)
        return image

    if step_id == "limits-3":
        image = slide_base(chapter_title, label, "Safeguards of the JavaScript engine")
        bullets(image, SAFEGUARDS)
        return image

    raise SystemExit(f"No slide for {step_id}")
