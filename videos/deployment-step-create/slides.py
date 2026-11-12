"""The slides of the Write a Deployment Step video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code
panels show the All Features deployment step of the Features module, the Custom File step and the Roles startup of
Orchard Core, trimmed with // ... to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "DeploymentStep", "LocalizationSource", "CommonLocalizationSources", "FeaturesLocalizationSources",
    "AllFeaturesDeploymentStep", "AllFeaturesDeploymentSource", "AllFeaturesDeploymentStepDriver",
    "AllFeaturesDeploymentStepViewModel", "DeploymentSourceBase", "DeploymentPlanResult", "JsonObject",
    "IModuleService", "DisplayDriver", "IDisplayResult", "BuildDisplayContext", "BuildEditorContext",
    "UpdateEditorContext", "Task", "StartupBase", "IServiceCollection", "CustomFileDeploymentStep",
    "CustomFileDeploymentSource", "RecipeFileDeploymentStep", "RecipeFileDeploymentStepDriver",
    "AllRolesDeploymentSource", "AllRolesDeploymentStep", "AllRolesDeploymentStepDriver", "IDeploymentTargetProvider",
    "DeploymentTarget", "RouteValueDictionary", "NamedRecipeStepHandler", "FeatureStep", "IShellFeaturesManager",
    "FileDownloadDeploymentTargetProvider", "IEnumerable", "Startup", "DeploymentStartup", "ShapeViewModel",
}

# What a plan, the sources and a package are.
FLOW = [
    ("1", "Deployment plan", "An ordered list of deployment steps, saved with a name."),
    ("2", "Deployment sources", "Each step goes to every source, which adds recipe steps and files."),
    ("3", "Package", "A Recipe.json file, and the files that the steps added."),
]

PARTS = [
    ("1", "Step", "A class that holds the settings saved in the plan."),
    ("2", "Source", "Adds recipe steps and files to the package."),
    ("3", "Display driver", "The card, the summary and the editor in the admin."),
    ("4", "Registration", "One call that registers the three together."),
]

STABLE = [
    ("1", "Type and assembly name", "Saved in each plan, to load the step again."),
    ("2", "Class name", "The Type of the step in the deployment recipe step."),
    ("3", "Renamed or moved class", "Existing plans show the step as unavailable."),
]

DISABLED = [
    ("1", "Plans keep the data", "The step is stored as it was saved."),
    ("2", "A warning in the plan", "The step is shown as unavailable."),
    ("3", "Delete only", "The plan offers a Delete button and no Edit button."),
]

STEP = """
using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

public class AllFeaturesDeploymentStep : DeploymentStep
{
    public override string Name => "AllFeatures";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Infrastructure;

    [JsonIgnore]
    public override LocalizationSource Title => FeaturesLocalizationSources.AllFeatures;

    public bool IgnoreDisabledFeatures { get; set; }
}
"""

STEP_TITLE = """
public class AllFeaturesDeploymentStep : DeploymentStep
{
    // ...

    [JsonIgnore]
    public override LocalizationSource Title => FeaturesLocalizationSources.AllFeatures;
}
"""

LOCALIZATION_SOURCES = """
using OrchardCore.Localization;

namespace OrchardCore.Features;

internal static class FeaturesLocalizationSources
{
    public static readonly LocalizationSource AllFeatures =
        LocalizationSource.Create("All Features", typeof(FeaturesLocalizationSources));
}
"""

SOURCE = """
public sealed class AllFeaturesDeploymentSource
    : DeploymentSourceBase<AllFeaturesDeploymentStep>
{
    // ...

    protected override async Task ProcessAsync(AllFeaturesDeploymentStep step, DeploymentPlanResult result)
    {
        // ...
    }
}
"""

PROCESS = """
protected override async Task ProcessAsync(AllFeaturesDeploymentStep step, DeploymentPlanResult result)
{
    var features = await _moduleService.GetAvailableFeaturesAsync();
    var featureStep = new JsonObject
    {
        ["name"] = "Feature",
        ["enable"] = JNode.FromObject(
            features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()),
    };

    if (!step.IgnoreDisabledFeatures)
    {
        featureStep.Add("disable", JNode.FromObject(
            features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()));
    }

    result.Steps.Add(featureStep);
}
"""

CUSTOM_FILE_SOURCE = """
public sealed class CustomFileDeploymentSource
    : DeploymentSourceBase<CustomFileDeploymentStep>
{
    protected override Task ProcessAsync(CustomFileDeploymentStep step, DeploymentPlanResult result)
        => result.FileBuilder.SetFileAsync(step.FileName, Encoding.UTF8.GetBytes(step.FileContent));
}
"""

CONSTRUCTOR = """
private readonly IModuleService _moduleService;

public AllFeaturesDeploymentSource(IModuleService moduleService)
{
    _moduleService = moduleService;
}
"""

DRIVER_DISPLAY = """
public sealed class AllFeaturesDeploymentStepDriver
    : DisplayDriver<DeploymentStep, AllFeaturesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(
        AllFeaturesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllFeaturesDeploymentStep_Fields_Summary", step)
                    .Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllFeaturesDeploymentStep_Fields_Thumbnail", step)
                    .Location("Thumbnail", "Content")
            );
    }

    // ...
}
"""

DRIVER_EDIT = """
// ...

public override IDisplayResult Edit(
    AllFeaturesDeploymentStep step, BuildEditorContext context)
{
    return Initialize<AllFeaturesDeploymentStepViewModel>(
        "AllFeaturesDeploymentStep_Fields_Edit", model =>
        {
            model.IgnoreDisabledFeatures = step.IgnoreDisabledFeatures;
        }).Location("Content");
}

public override async Task<IDisplayResult> UpdateAsync(
    AllFeaturesDeploymentStep step, UpdateEditorContext context)
{
    await context.Updater.TryUpdateModelAsync(
        step, Prefix, x => x.IgnoreDisabledFeatures);

    return Edit(step, context);
}
"""

DRIVER_VALIDATE = """
public override async Task<IDisplayResult> UpdateAsync(
    CustomFileDeploymentStep step, UpdateEditorContext context)
{
    await context.Updater.TryUpdateModelAsync(
        step, Prefix, x => x.FileName, x => x.FileContent);

    if (!string.IsNullOrWhiteSpace(step.FileName))
    {
        if (step.FileName.StartsWith('/') || Path.IsPathFullyQualified(step.FileName))
        {
            context.Updater.ModelState.AddModelError(
                Prefix, nameof(step.FileName), S["A virtual path is required."]);
        }

        // ...
    }

    return Edit(step, context);
}
"""

THUMBNAIL_VIEW = """
@model dynamic

<h4 class="card-title">@T["All Features"]</h4>
<p>@T["Exports the state of all features."]</p>
"""

SUMMARY_VIEW = """
@model dynamic

<h5>@T["All Features"]</h5>

<span class="hint">@T["Adds all features and their state to the plan. ..."]</span>
"""

VIEW_MODEL = """
public class AllFeaturesDeploymentStepViewModel
{
    public bool IgnoreDisabledFeatures { get; set; }
}
"""

EDIT_VIEW = """
@model AllFeaturesDeploymentStepViewModel

<div class="ocat-wrapper" asp-validation-class-for="IgnoreDisabledFeatures">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input type="checkbox" class="form-check-input" asp-for="IgnoreDisabledFeatures" />
            <label class="form-check-label" asp-for="IgnoreDisabledFeatures">@T["Ignore disabled features"]</label>
        </div>
        <span class="hint">@T["Check if the disabled features have to be ignored."]</span>
    </div>
</div>
"""

FEATURES_STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddDeployment<AllFeaturesDeploymentSource, AllFeaturesDeploymentStep, AllFeaturesDeploymentStepDriver>();
    }
}
"""

ROLES_STARTUP = """
[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllRolesDeploymentSource, AllRolesDeploymentStep, AllRolesDeploymentStepDriver>();
    }
}
"""

RECIPE = """
{
  "name": "",
  "displayName": "",
  // ...
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.Admin",
        // ...
      ],
      "disable": [
        "OrchardCore.Tenants",
        // ...
      ]
    }
  ]
}
"""

FEATURE_STEP = """
public sealed class FeatureStep : NamedRecipeStepHandler
{
    // ...

    public FeatureStep(IShellFeaturesManager shellFeaturesManager)
        : base("Feature")
    {
        _shellFeaturesManager = shellFeaturesManager;
    }

    // ...
}
"""

TARGET_PROVIDER = """
public class FileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
{
    // ...

    public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
    {
        return Task.FromResult<IEnumerable<DeploymentTarget>>(
            new[] {
                new DeploymentTarget(
                    name: S["File Download"],
                    description: S["Download a deployment plan locally."],
                    route: new RouteValueDictionary(new
                    {
                        area = "OrchardCore.Deployment",
                        controller = "ExportFile",
                        action = "Execute",
                    })
                ),
            }
        );
    }
}
"""


def lines(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    return {number for number, text in enumerate(code.strip("\n").split("\n"), start=1)
            if any(needle in text for needle in needles)}


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "From a plan to a package")
        bullets(image, FLOW)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "The parts of a deployment step")
        bullets(image, PARTS, active={0, 1, 2})
        return image

    if step_id == "step-1":
        image = slide_base(chapter_title, label, "The step model")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentStep.cs", "csharp", STEP, types=TYPES, size=26,
                  highlight=lines(STEP, "public class", "Name =>", "IgnoreDisabledFeatures", "{", "}"))
        return image

    if step_id == "step-2":
        image = slide_base(chapter_title, label, "Category")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentStep.cs", "csharp", STEP, types=TYPES, size=26,
                  highlight=lines(STEP, "public class", "Category =>", "{", "}"))
        return image

    if step_id == "step-3":
        image = slide_base(chapter_title, label, "Title")
        draw_code(image, (96, 240, W - 96, 560), "AllFeaturesDeploymentStep.cs", "csharp", STEP_TITLE, types=TYPES, size=24)
        draw_code(image, (96, 590, W - 96, 1030), "FeaturesLocalizationSources.cs", "csharp", LOCALIZATION_SOURCES,
                  types=TYPES, size=24)
        return image

    if step_id == "step-4":
        image = slide_base(chapter_title, label, "JsonIgnore on Category and Title")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentStep.cs", "csharp", STEP, types=TYPES, size=26,
                  highlight=lines(STEP, "JsonIgnore]", "Category =>", "Title =>", "{", "}"))
        return image

    if step_id == "step-5":
        image = slide_base(chapter_title, label, "Names that must stay stable")
        bullets(image, STABLE)
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "The source")
        draw_code(image, (96, 240, W - 96, 760), "AllFeaturesDeploymentSource.cs", "csharp", SOURCE, types=TYPES, size=26,
                  highlight=lines(SOURCE, "public sealed class", "DeploymentSourceBase<", "ProcessAsync"))
        return image

    if step_id == "source-2":
        image = slide_base(chapter_title, label, "Add a recipe step")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentSource.cs", "csharp", PROCESS, types=TYPES, size=24)
        return image

    if step_id == "source-3":
        image = slide_base(chapter_title, label, "Add a file")
        draw_code(image, (96, 240, W - 96, 560), "CustomFileDeploymentSource.cs", "csharp", CUSTOM_FILE_SOURCE,
                  types=TYPES, size=24, highlight=lines(CUSTOM_FILE_SOURCE, "SetFileAsync"))
        draw_code(image, (96, 600, W - 96, 960), "AllFeaturesDeploymentSource.cs", "csharp", CONSTRUCTOR, types=TYPES,
                  size=26)
        return image

    if step_id == "editor-1":
        image = slide_base(chapter_title, label, "The display driver: the Summary and the Thumbnail")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentStepDriver.cs", "csharp", DRIVER_DISPLAY,
                  types=TYPES, size=24)
        return image

    if step_id == "editor-2":
        image = slide_base(chapter_title, label, "The display driver: the editor")
        draw_code(image, (96, 240, W - 96, 1030), "AllFeaturesDeploymentStepDriver.cs", "csharp", DRIVER_EDIT,
                  types=TYPES, size=24)
        return image

    if step_id == "editor-3":
        image = slide_base(chapter_title, label, "Validate the input")
        draw_code(image, (96, 240, W - 96, 1030), "CustomFileDeploymentStepDriver.cs", "csharp", DRIVER_VALIDATE,
                  types=TYPES, size=24, highlight=lines(DRIVER_VALIDATE, "AddModelError", "Prefix, nameof", "S[\"A virtual"))
        return image

    if step_id == "editor-4":
        image = slide_base(chapter_title, label, "One Razor view for each shape")
        draw_code(image, (96, 240, W - 96, 560), "Views/Items/AllFeaturesDeploymentStep.Fields.Thumbnail.cshtml", "cshtml",
                  THUMBNAIL_VIEW, types=TYPES, size=26)
        draw_code(image, (96, 600, W - 96, 960), "Views/Items/AllFeaturesDeploymentStep.Fields.Summary.cshtml", "cshtml",
                  SUMMARY_VIEW, types=TYPES, size=26)
        return image

    if step_id == "editor-5":
        image = slide_base(chapter_title, label, "The edit view and its view model")
        draw_code(image, (96, 240, W - 96, 520), "AllFeaturesDeploymentStepViewModel.cs", "csharp", VIEW_MODEL,
                  types=TYPES, size=26)
        draw_code(image, (96, 560, W - 96, 1030), "Views/Items/AllFeaturesDeploymentStep.Fields.Edit.cshtml", "cshtml",
                  EDIT_VIEW, types=TYPES, size=24)
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register the three together")
        draw_code(image, (96, 240, W - 96, 700), "Startup.cs of the Features module", "csharp", FEATURES_STARTUP,
                  types=TYPES, size=24, highlight=lines(FEATURES_STARTUP, "AddDeployment"))
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "Only when Deployment is enabled")
        draw_code(image, (96, 240, W - 96, 700), "Startup.cs of the Roles module", "csharp", ROLES_STARTUP, types=TYPES,
                  size=24, highlight=lines(ROLES_STARTUP, "RequireFeatures", "AddDeployment"))
        return image

    if step_id == "register-3":
        image = slide_base(chapter_title, label, "When the feature is disabled")
        bullets(image, DISABLED)
        return image

    if step_id == "package-1":
        image = slide_base(chapter_title, label, "Recipe.json in the package")
        draw_code(image, (96, 240, W - 96, 1030), "Recipe.json", "json", RECIPE, types=TYPES, size=26,
                  highlight=set(range(5, 18)))
        return image

    if step_id == "package-2":
        image = slide_base(chapter_title, label, "The recipe step that imports it")
        draw_code(image, (96, 240, W - 96, 900), "FeatureStep.cs", "csharp", FEATURE_STEP, types=TYPES, size=26,
                  highlight=lines(FEATURE_STEP, "public sealed class", "base("))
        return image

    if step_id == "package-3":
        image = slide_base(chapter_title, label, "Other destinations")
        draw_code(image, (96, 240, W - 96, 1030), "FileDownloadDeploymentTargetProvider.cs", "csharp", TARGET_PROVIDER,
                  types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
