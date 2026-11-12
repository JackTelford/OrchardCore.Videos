"""The slides of the "Write a Recipe Step" video. assemble.py calls slide() for each step of a "slide" clip.

Every code panel is code from the source of OrchardCMS/OrchardCore, or an example from its documentation, trimmed
with // ... The values of the recipe heads on the slides are written for the slides.
Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "IRecipeStepHandler", "NamedRecipeStepHandler", "RecipeExecutionContext", "JsonObject", "IList", "Task",
    "ThemesStep", "ThemeStepModel", "ISiteThemeService", "IAdminThemeService", "StartupBase", "IServiceCollection",
    "IServiceProvider", "TranslationsStep", "RecipesStep", "InternalStep", "IStringLocalizer", "IRecipeHarvester",
    "IEnumerable", "RecipeDescriptor", "ShortcodeTemplate", "RecipeExecutionException", "RecipeStepResult",
    "Exception", "StringComparison", "StringComparer", "IHtmlSanitizerService", "ShortcodeTemplatesManager",
    "IFeatureInfo", "RecipeEntry", "RecipeResult", "ExceptionDispatchInfo", "TImplementation",
}

LEFT, RIGHT = 96, W - 96

INTERFACE = """
public interface IRecipeStepHandler
{
    /// <summary>
    /// Processes a recipe step.
    /// </summary>
    Task ExecuteAsync(RecipeExecutionContext context);
}
"""

CONTEXT = """
public class RecipeExecutionContext
{
    public string Name { get; set; }

    public JsonObject Step { get; set; }

    public IList<string> Errors { get; } = [];

    // ...
}
"""

NAMED = """
public abstract class NamedRecipeStepHandler : IRecipeStepHandler
{
    protected readonly string StepName;

    protected NamedRecipeStepHandler(string stepName)
    {
        StepName = stepName;
    }

    public Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HandleAsync(context);
    }

    protected abstract Task HandleAsync(RecipeExecutionContext context);
}
"""

THEMES_CLASS = """
using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
// ...

namespace OrchardCore.Themes.Recipes;

public sealed class ThemesStep : NamedRecipeStepHandler
{
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public ThemesStep(
        ISiteThemeService siteThemeService,
        IAdminThemeService adminThemeService)
        : base("Themes")
    {
        _adminThemeService = adminThemeService;
        _siteThemeService = siteThemeService;
    }

    // ...
}
"""

THEMES_HANDLE = """
protected override async Task HandleAsync(RecipeExecutionContext context)
{
    var model = context.Step.ToObject<ThemeStepModel>();

    if (!string.IsNullOrEmpty(model.Site))
    {
        await _siteThemeService.SetSiteThemeAsync(model.Site);
    }

    if (!string.IsNullOrEmpty(model.Admin))
    {
        await _adminThemeService.SetAdminThemeAsync(model.Admin);
    }
}
"""

THEMES_MODEL = """
public sealed class ThemeStepModel
{
    public string Site { get; set; }
    public string Admin { get; set; }
}
"""

THEMES_JSON = """
{
  "name": "themes",
  "admin": "TheAdmin",
  "site": "MyCustomTheme"
}
"""

RECIPE_FILE = """
{
  "name": "MyRecipe",
  "displayName": "My recipe",
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.Admin", "MyCustomTheme" ],
      "disable": []
    },
    {
      "name": "themes",
      "admin": "TheAdmin",
      "site": "MyCustomTheme"
    }
  ]
}
"""

RECIPE_USE = """
{
  "name": "MyRecipe",
  "displayName": "My recipe",
  "description": "Sets the themes of the site.",
  "tags": [ "themes" ],
  "steps": [
    {
      "name": "themes",
      "admin": "TheAdmin",
      "site": "MyCustomTheme"
    }
  ]
}
"""

SCRIPTS = """
{
  "variables": {
    "homeId": "[js:uuid()]"
  },
  "steps": [
    {
      "name": "settings",
      "HomeRoute": {
        "Action": "Display",
        "Controller": "Item",
        "Area": "OrchardCore.Contents",
        "ContentItemId": "[js:variables('homeId')]"
      }
    }
  ]
}
"""

SHORTCODES = """
protected override async Task HandleAsync(RecipeExecutionContext context)
{
    if (context.Step.TryGetPropertyValue("ShortcodeTemplates", out var jsonNode) && jsonNode is JsonObject templates)
    {
        foreach (var property in templates)
        {
            var name = property.Key;
            var value = property.Value.ToObject<ShortcodeTemplate>();

            value.Usage = _htmlSanitizerService.Sanitize(value.Usage);

            await _templatesManager.UpdateShortcodeTemplateAsync(name, value);
        }
    }
}
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Recipes;
// ...

namespace OrchardCore.Themes;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddRecipeExecutionStep<ThemesStep>();
        // ...
    }
}
"""

ADD_STEP = """
public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
    where TImplementation : class, IRecipeStepHandler
{
    services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());

    return services;
}
"""

EXECUTOR_LOOP = """
var recipeStepHandlers = scope.ServiceProvider.GetServices<IRecipeStepHandler>();
// ...
foreach (var recipeStepHandler in recipeStepHandlers)
{
    await recipeStepHandler.ExecuteAsync(recipeStep);
}
"""

RECIPES_LIST = """
var recipes = recipeCollections.SelectMany(x => x)
    .Where(r => !r.IsSetupRecipe &&
        (r.Tags == null || !r.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase)) &&
        features.Any(f => r.BasePath != null && f.Extension?.SubPath != null &&
            r.BasePath.Contains(f.Extension.SubPath, StringComparison.OrdinalIgnoreCase)));
"""

RECIPES_STEP = """
protected override async Task HandleAsync(RecipeExecutionContext context)
{
    // ...
    foreach (var recipe in step.Values)
    {
        if (!recipes.TryGetValue(recipe.Name, out var value))
        {
            context.Errors.Add(S["No recipe named '{0}' was found.", recipe.Name]);

            continue;
        }

        innerRecipes.Add(value);
    }
    // ...
}
"""

EXECUTOR_ERRORS = """
await ExecuteStepAsync(recipeStep, cancellationToken);

if (recipeStep.Errors.Count > 0)
{
    stepResult.IsSuccessful = false;
    stepResult.Errors = recipeStep.Errors.ToArray();
}
// ...
if (!stepResult.IsSuccessful)
{
    throw new RecipeExecutionException(stepResult);
}
"""

NOTIFY = """
catch (RecipeExecutionException e)
{
    _logger.LogError(e, "Unable to import a recipe file.");

    await _notifier.ErrorAsync(H[
        "The recipe '{0}' failed to run due to the following errors: {1}",
        recipe.DisplayName, string.Join(' ', e.StepResult.Errors)]);
}
"""

EXECUTOR_CATCH = """
catch (Exception e)
{
    stepResult.IsSuccessful = false;
    stepResult.Errors = [S["Unexpected error occurred while executing the '{0}' step.", stepResult.StepName]];
    // ...
}
"""

TRANSLATIONS = """
public sealed class TranslationsStep : IRecipeStepHandler
{
    // ...

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (string.Equals(context.Name, "Translations", StringComparison.OrdinalIgnoreCase))
        {
            await HandleNewFormatAsync(context);
        }
        else if (string.Equals(context.Name, "DynamicDataTranslations", StringComparison.OrdinalIgnoreCase))
        {
            await HandleLegacyFormatAsync(context);
        }
    }

    // ...
}
"""

CONCEPT_FLOW = [
    ("1", "Read the steps in order", "The executor goes through the steps list of the recipe."),
    ("2", "Evaluate the script expressions", "The expressions in a step's values are replaced by their results."),
    ("3", "Hand the step to every handler", "Each registered step handler receives the same context."),
    ("4", "The matching handler acts", "A handler does nothing when the step's name is not its own."),
]

CONCEPT_WHERE = [
    ("1", "The Recipes page", "Tools, Recipes runs the recipes of modules and themes."),
    ("2", "Site setup", "A setup recipe runs when a tenant is set up."),
    ("3", "A deployment import", "Importing a package runs the recipe inside it."),
    ("4", "A data migration", "A migration runs a recipe file of its module."),
]

RECAP = [
    ("1", "Derive from NamedRecipeStepHandler", "Pass the name of the step to the constructor."),
    ("2", "Read the step with ToObject", "Properties of the model match those of the step."),
    ("3", "Report problems in Errors", "Localized messages for the person who runs the recipe."),
    ("4", "Register with AddRecipeExecutionStep", "One line in the Startup class of the module."),
]


def slide(step_id, chapter_title, label):
    if step_id == "concept-1":
        image = slide_base(chapter_title, label, "A recipe is a list of steps")
        draw_code(image, (LEFT, 240, 1180, 930), "Recipes/MyRecipe.recipe.json", "json", RECIPE_FILE, types=TYPES, size=24)
        note_cards(image, [
            ("name", "Which step it is."),
            ("Other properties", "The data of the step."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "concept-2":
        image = slide_base(chapter_title, label, "How Orchard Core runs a step")
        bullets(image, CONCEPT_FLOW)
        return image

    if step_id == "concept-3":
        image = slide_base(chapter_title, label, "Where the same handlers run")
        bullets(image, CONCEPT_WHERE)
        return image

    if step_id == "handler-1":
        image = slide_base(chapter_title, label, "The step handler interface")
        draw_code(image, (LEFT, 240, 980, 660), "IRecipeStepHandler.cs", "csharp", INTERFACE, types=TYPES, size=22)
        draw_code(image, (1020, 240, RIGHT, 660), "RecipeExecutionContext.cs", "csharp", CONTEXT, types=TYPES, size=22)
        note_cards(image, [("Called for every step", "Whatever the name of the step is.")], (LEFT, 710, 980, 0))
        note_cards(image, [("Decides by name", "Acts only on the steps it owns.")], (1020, 710, RIGHT, 0))
        return image

    if step_id == "handler-2":
        image = slide_base(chapter_title, label, "Named Recipe Step Handler")
        draw_code(image, (LEFT, 240, 1270, 990), "NamedRecipeStepHandler.cs", "csharp", NAMED, types=TYPES, size=22)
        note_cards(image, [
            ("Constructor", "Takes the name of the step."),
            ("ExecuteAsync", "Compares it, ignoring case."),
            ("HandleAsync", "Your code, for a match only."),
        ], (1300, 240, RIGHT, 0))
        return image

    if step_id == "handler-3":
        image = slide_base(chapter_title, label, "A complete example: the Themes step")
        draw_code(image, (LEFT, 240, 1180, 1036), "ThemesStep.cs", "csharp", THEMES_CLASS, types=TYPES, size=22,
                  highlight={8, 10, 11, 13, 14, 15, 16})
        note_cards(image, [
            ("The name", "Passed to the base constructor."),
            ("The services", "Injected by the constructor."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "handler-4":
        image = slide_base(chapter_title, label, "Read the step with ToObject")
        draw_code(image, (LEFT, 240, 1130, 780), "ThemesStep.cs", "csharp", THEMES_HANDLE, types=TYPES, size=22)
        draw_code(image, (1160, 240, RIGHT, 520), "ThemeStepModel", "csharp", THEMES_MODEL, types=TYPES, size=22)
        draw_code(image, (1160, 560, RIGHT, 820), "The step in a recipe", "json", THEMES_JSON, types=TYPES, size=22)
        note_cards(image, [
            ("Names match", "The match ignores case."),
        ], (LEFT, 820, 1130, 0))
        return image

    if step_id == "read-1":
        image = slide_base(chapter_title, label, "Script expressions are evaluated first")
        draw_code(image, (LEFT, 240, 1180, 880), "A recipe file", "json", SCRIPTS, types=TYPES, size=24)
        note_cards(image, [
            ("In the recipe file", "A script expression."),
            ("In the context", "The result of the expression."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "read-2":
        image = slide_base(chapter_title, label, "Read the JSON node yourself")
        draw_code(image, (LEFT, 240, RIGHT, 790), "ShortcodeTemplateStep.cs", "csharp", SHORTCODES, types=TYPES, size=22)
        note_cards(image, [
            ("TryGetPropertyValue", "Reads one property of the step as a JSON node."),
        ], (LEFT, 830, RIGHT, 0))
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register the handler")
        draw_code(image, (LEFT, 240, 1180, 860), "Startup.cs", "csharp", STARTUP, types=TYPES, size=23,
                  highlight={13})
        note_cards(image, [
            ("In your module", "Registers the handler."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "What the registration does")
        draw_code(image, (LEFT, 240, RIGHT, 640), "ServiceCollectionExtensions.cs", "csharp", ADD_STEP, types=TYPES, size=25)
        note_cards(image, [
            ("Scoped", "The constructor can take any service of the tenant."),
        ], (LEFT, 700, RIGHT, 0))
        return image

    if step_id == "register-3":
        image = slide_base(chapter_title, label, "A step that no handler claims is skipped")
        draw_code(image, (LEFT, 240, RIGHT, 560), "RecipeExecutor.cs", "csharp", EXECUTOR_LOOP, types=TYPES, size=25)
        note_cards(image, [
            ("No Feature attribute", "The Startup class belongs to the feature of the module."),
            ("Feature disabled", "The handler is not registered, and no error is reported."),
        ], (LEFT, 600, RIGHT, 0))
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "A recipe file that uses the step")
        draw_code(image, (LEFT, 240, 1180, 790), "Recipes/MyRecipe.recipe.json", "json", RECIPE_USE, types=TYPES, size=25)
        note_cards(image, [
            ("Recipes folder", "In a module or a theme."),
            ("Top-level properties", "Name and describe the recipe."),
            ("steps", "One object for each step."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "The Recipes page")
        draw_code(image, (LEFT, 240, RIGHT, 590), "AdminController.cs", "csharp", RECIPES_LIST, types=TYPES, size=24)
        note_cards(image, [
            ("Not listed", "Setup recipes, and recipes tagged hidden."),
            ("Run", "Executes the recipe, then shows a notification."),
        ], (LEFT, 630, RIGHT, 0))
        return image

    if step_id == "errors-1":
        image = slide_base(chapter_title, label, "Report a problem")
        draw_code(image, (LEFT, 240, 1230, 820), "RecipesStep.cs", "csharp", RECIPES_STEP, types=TYPES, size=22,
                  highlight={6, 7, 8, 9, 10, 11})
        note_cards(image, [
            ("Errors", "A list of messages on the context."),
            ("S", "The string localizer."),
        ], (1260, 240, RIGHT, 0))
        return image

    if step_id == "errors-2":
        image = slide_base(chapter_title, label, "A step with errors stops the recipe")
        draw_code(image, (LEFT, 240, 1180, 790), "RecipeExecutor.cs", "csharp", EXECUTOR_ERRORS, types=TYPES, size=24)
        note_cards(image, [
            ("Failed", "The step result is not successful."),
            ("Stopped", "The steps after it do not run."),
        ], (1220, 240, RIGHT, 0))
        return image

    if step_id == "errors-3":
        image = slide_base(chapter_title, label, "What the person who runs the recipe sees")
        draw_code(image, (LEFT, 240, RIGHT, 600), "AdminController.cs", "csharp", NOTIFY, types=TYPES, size=24)
        note_cards(image, [
            ("Your messages", "Joined into one notification."),
            ("Write them", "Localized, and without sensitive data."),
        ], (LEFT, 630, RIGHT, 0))
        return image

    if step_id == "errors-4":
        image = slide_base(chapter_title, label, "A thrown exception")
        draw_code(image, (LEFT, 240, RIGHT, 540), "RecipeExecutor.cs", "csharp", EXECUTOR_CATCH, types=TYPES, size=24)
        note_cards(image, [
            ("In the notification", "A general message that names the step."),
            ("In the log", "The exception, with its details."),
        ], (LEFT, 580, RIGHT, 0))
        return image

    if step_id == "several-1":
        image = slide_base(chapter_title, label, "More than one step name")
        draw_code(image, (LEFT, 240, RIGHT, 960), "TranslationsStep.cs", "csharp", TRANSLATIONS, types=TYPES, size=24)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "Writing a recipe step")
        bullets(image, RECAP)
        return image

    if step_id == "recap-2":
        image = slide_base(chapter_title, label, "Then use it")
        draw_code(image, (LEFT, 240, 1180, 790), "Recipes/MyRecipe.recipe.json", "json", RECIPE_USE, types=TYPES, size=25,
                  highlight={7, 8, 9, 10, 11})
        note_cards(image, [
            ("In the recipe", "Spell the name of the step."),
            ("Deployment steps", "Covered in a separate video."),
        ], (1220, 240, RIGHT, 0))
        return image

    raise SystemExit(f"No slide for {step_id}")
