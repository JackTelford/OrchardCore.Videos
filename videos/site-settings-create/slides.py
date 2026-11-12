"""The slides of the "Add Site Settings to Your Module" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py. The code is the Debugging group of the Settings module of Orchard Core,
trimmed to what the narration talks about ("// ..." marks what is omitted).
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {
    "DebugSettings", "DebugSettingsViewModel", "DebugSettingsDisplayDriver", "SiteDisplayDriver", "IDisplayResult",
    "ISite", "BuildEditorContext", "UpdateEditorContext", "SettingsPermissions", "Task", "Permission",
    "LocalizationSource", "IPermissionProvider", "Permissions", "PermissionStereotype", "IServiceCollection",
    "AdminMenu", "AdminNavigationProvider", "RouteValueDictionary", "NavigationBuilder", "ValueTask",
    "ShapeRenderingOptions", "ShapeRenderingOptionsConfiguration", "IConfigureOptions", "ISiteService",
    "DeploymentStartup", "StartupBase", "IEnumerable", "OrchardCoreConstants",
}

HOW_1 = [
    ("1", "Site settings", "One document for each tenant, read through ISiteService."),
    ("2", "Built-in values", "The site name, the time zone, the page size and more."),
    ("3", "Properties", "A bag where each module saves a section of its own."),
]

HOW_2 = [
    ("1", "A section", "A plain class, saved under the name of the class."),
    ("2", "A group", "A page under Settings. Its address ends with the group identifier."),
    ("3", "A display driver", "Builds the editor of the group and updates the section."),
]

HOW_3 = [
    ("1", "A settings class", "Plain properties, saved in the site settings."),
    ("2", "An editor", "A view model, a view and a display driver."),
    ("3", "A permission", "Who may open and change the group."),
    ("4", "A menu entry", "A link under Settings that opens the group."),
]

SETTINGS = """
namespace OrchardCore.Settings;

public sealed class DebugSettings
{
    public bool WriteShapeDebugInformation { get; set; }
}
"""

VIEW_MODEL = """
namespace OrchardCore.Settings.ViewModels;

public class DebugSettingsViewModel
{
    public bool WriteShapeDebugInformation { get; set; }
}
"""

DRIVER_HEADER = """
public sealed class DebugSettingsDisplayDriver
    : SiteDisplayDriver<DebugSettings>
{
    public const string GroupId = "debugging";

    protected override string SettingsGroupId
        => GroupId;

    // ...
}
"""

DRIVER_EDIT = """
public override async Task<IDisplayResult> EditAsync(ISite site, DebugSettings settings, BuildEditorContext context)
{
    if (!await _authorizationService.AuthorizeAsync(
        _httpContextAccessor.HttpContext?.User,
        SettingsPermissions.ManageDebuggingSettings))
    {
        return null;
    }

    return Initialize<DebugSettingsViewModel>("DebugSettings_Edit", model =>
    {
        model.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
    }).Location("Content:5")
    .OnGroup(SettingsGroupId);
}
"""

DRIVER_UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(ISite site, DebugSettings settings, UpdateEditorContext context)
{
    if (!await _authorizationService.AuthorizeAsync(
        _httpContextAccessor.HttpContext?.User,
        SettingsPermissions.ManageDebuggingSettings))
    {
        return null;
    }

    var model = new DebugSettingsViewModel();

    await context.Updater.TryUpdateModelAsync(model, Prefix);

    // ...

    settings.WriteShapeDebugInformation = model.WriteShapeDebugInformation;

    return await EditAsync(site, settings, context);
}
"""

MODEL_ERROR = """
if (model.PageSize.Value < 1)
{
    context.Updater.ModelState.AddModelError(Prefix, nameof(model.PageSize), S["The page size must be greater than zero."]);
}
"""

SAVE_IF_VALID = """
if (context.Updater.ModelState.IsValid)
{
    model.Properties[PropertyName] = JObject.FromObject(section);
}
"""

VIEW = """
@model OrchardCore.Settings.ViewModels.DebugSettingsViewModel

<div class="ocat-wrapper" asp-validation-class-for="WriteShapeDebugInformation">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input asp-for="WriteShapeDebugInformation" class="form-check-input" type="checkbox" />
            <label asp-for="WriteShapeDebugInformation" class="form-check-label">@T["Write shape debug information"]</label>
        </div>
        @* ... *@
    </div>
</div>
"""

STARTUP = """
public override void ConfigureServices(IServiceCollection services)
{
    services.AddPermissionProvider<Permissions>();

    // ...

    services.AddSiteDisplayDriver<DebugSettingsDisplayDriver>();

    // ...

    services.AddSiteSettingsPermission(DebugSettingsDisplayDriver.GroupId, SettingsPermissions.ManageDebuggingSettings);
    services.AddNavigationProvider<AdminMenu>();
}
"""

PERMISSION = """
public static class SettingsPermissions
{
    public static readonly Permission ManageSettings = new(
        "ManageSettings",
        LocalizationSource.Create("Manage settings", typeof(SettingsPermissions)));

    public static readonly Permission ManageDebuggingSettings = new(
        "ManageDebuggingSettings",
        LocalizationSource.Create("Manage debugging settings", typeof(SettingsPermissions)),
        [ManageSettings]);

    // ...
}
"""

PROVIDER = """
public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SettingsPermissions.ManageSettings,
        SettingsPermissions.ManageGeneralSettings,
        SettingsPermissions.ManageDebuggingSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
"""

MENU = """
private static readonly RouteValueDictionary s_debuggingRouteValues = new()
{
    { "area", "OrchardCore.Settings" },
    { "groupId", DebugSettingsDisplayDriver.GroupId },
};

protected override ValueTask BuildAsync(NavigationBuilder builder)
{
    builder
        // ...
        .Add(S["Settings"], "end.100", settings => settings
            // ...
            .Add(S["Debugging"], "after", debugging => debugging
                // ...
                .Action("Index", "Admin", s_debuggingRouteValues)
                .Permission(SettingsPermissions.ManageDebuggingSettings)
                .LocalNav()
            )
        , priority: 1);

    return ValueTask.CompletedTask;
}
"""

READ = """
public sealed class ShapeRenderingOptionsConfiguration : IConfigureOptions<ShapeRenderingOptions>
{
    private readonly ISiteService _siteService;

    // ...

    public void Configure(ShapeRenderingOptions options)
    {
        var settings = _siteService.GetSettings<DebugSettings>();

        options.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
    }
}
"""

REQUEST_UPDATE = """
if (settings.WriteShapeDebugInformation != model.WriteShapeDebugInformation)
{
    _optionsUpdateNotifier.RequestUpdate<ShapeRenderingOptions>();
}
"""

CHANGE_TOKEN = """
public override void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddSignalOptionsChangeTokenSource<ShapeRenderingOptions>();

    // ...
}
"""

CONFIGURATION = """
{
  "OrchardCore": {
    "Liquid": {
      "PermittedSiteProperties": [
        "DebugSettings"
      ]
    }
  }
}
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Settings",
      "DebugSettings": { "WriteShapeDebugInformation": true }
    }
  ]
}
"""

DEPLOYMENT = """
[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<DebugSettings, DeploymentStartup>(
            S => S["Debugging settings"],
            S => S["Exports the debugging settings."]);
    }
}
"""


def dotted(image, x, y, lines, step=60):
    """Lines of text with a green dot, under or next to a code panel."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((x, y - 7, x + 14, y + 7), fill=GREEN)
        draw.text((x + 36, y), line, font=font(30), fill=TEXT, anchor="lm")
        y += step


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "One document for the whole site")
        bullets(image, HOW_1)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "A section becomes a group")
        bullets(image, HOW_2)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "What your module provides")
        bullets(image, HOW_3)
        return image

    if step_id == "class-1":
        image = slide_base(chapter_title, label, "1. The settings class")
        draw_code(image, (96, 240, 1100, 620), "DebugSettings.cs", "csharp", SETTINGS, types=TYPES, size=28)
        note_cards(image, [
            ("Plain properties", "Saved as JSON in the site settings."),
            ("A default constructor", "Used when nothing is saved."),
        ], (1150, 240, W - 96, 700))
        return image

    if step_id == "class-2":
        image = slide_base(chapter_title, label, "2. The view model")
        draw_code(image, (96, 240, 1100, 620), "DebugSettingsViewModel.cs", "csharp", VIEW_MODEL, types=TYPES, size=28)
        note_cards(image, [
            ("Bound to the form", "The posted values go into it."),
            ("Free to differ", "It can hold extras, like a list."),
        ], (1150, 240, W - 96, 700))
        return image

    if step_id == "drv-1":
        image = slide_base(chapter_title, label, "3. The display driver")
        draw_code(image, (96, 240, 1180, 720), "DebugSettingsDisplayDriver.cs", "csharp", DRIVER_HEADER, types=TYPES, size=26)
        note_cards(image, [
            ("SiteDisplayDriver", "Reads and writes your section."),
            ("SettingsGroupId", "Names the group of this driver."),
            ("Other groups", "Are left to other drivers."),
        ], (1230, 240, W - 96, 900))
        return image

    if step_id == "drv-2":
        image = slide_base(chapter_title, label, "3. The edit method")
        draw_code(image, (96, 240, W - 96, 900), "DebugSettingsDisplayDriver.cs", "csharp", DRIVER_EDIT, types=TYPES, size=24)
        return image

    if step_id == "drv-3":
        image = slide_base(chapter_title, label, "3. The update method")
        draw_code(image, (96, 240, W - 96, 1000), "DebugSettingsDisplayDriver.cs", "csharp", DRIVER_UPDATE, types=TYPES, size=24)
        return image

    if step_id == "drv-4":
        image = slide_base(chapter_title, label, "3. Reject a value")
        draw_code(image, (96, 240, W - 96, 490), "DefaultSiteSettingsDisplayDriver.cs", "csharp", MODEL_ERROR, types=TYPES, size=24)
        draw_code(image, (96, 530, W - 96, 780), "SectionDisplayDriverBase.cs", "csharp", SAVE_IF_VALID, types=TYPES, size=24)
        dotted(image, 110, 850, [
            "A model error: the section is not saved.",
            "The form is shown again, with the message.",
        ])
        return image

    if step_id == "view-1":
        image = slide_base(chapter_title, label, "4. The view")
        draw_code(image, (96, 240, W - 96, 740), "DebugSettings.Edit.cshtml", "cshtml", VIEW, types=TYPES, size=22)
        dotted(image, 110, 810, [
            "The shape type DebugSettings_Edit is the file DebugSettings.Edit.cshtml.",
            "asp-for binds the input to the view model.",
            "T translates the label.",
        ])
        return image

    if step_id == "reg-1":
        image = slide_base(chapter_title, label, "5. Register the driver and the permission")
        draw_code(image, (96, 240, W - 96, 800), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24)
        dotted(image, 110, 870, [
            "AddSiteDisplayDriver: the driver builds the editor.",
            "AddSiteSettingsPermission: who may open the group.",
        ])
        return image

    if step_id == "reg-2":
        image = slide_base(chapter_title, label, "6. The permission")
        draw_code(image, (96, 240, W - 96, 830), "SettingsPermissions.cs", "csharp", PERMISSION, types=TYPES,
                  highlight={7, 8, 9, 10}, size=26)
        dotted(image, 110, 900, ["The last argument lists the permissions that imply this one."])
        return image

    if step_id == "reg-3":
        image = slide_base(chapter_title, label, "6. The permission provider")
        draw_code(image, (96, 240, W - 96, 1020), "Permissions.cs", "csharp", PROVIDER, types=TYPES, size=23)
        return image

    if step_id == "reg-4":
        image = slide_base(chapter_title, label, "7. The menu entry")
        draw_code(image, (96, 240, W - 96, 1020), "AdminMenu.cs", "csharp", MENU, types=TYPES, size=22)
        return image

    if step_id == "use-1":
        image = slide_base(chapter_title, label, "Read the settings in code")
        draw_code(image, (96, 240, W - 96, 790), "ShapeRenderingOptionsConfiguration.cs", "csharp", READ, types=TYPES, size=24)
        dotted(image, 110, 860, [
            "GetSettings: synchronous code.",
            "GetSettingsAsync: asynchronous code.",
            "Nothing saved: an instance with default values.",
        ])
        return image

    if step_id == "use-2":
        image = slide_base(chapter_title, label, "Update the options that use a group")
        draw_code(image, (96, 240, W - 96, 480), "DebugSettingsDisplayDriver.cs", "csharp", REQUEST_UPDATE, types=TYPES, size=26)
        draw_code(image, (96, 520, W - 96, 930), "Startup.cs", "csharp", CHANGE_TOKEN, types=TYPES, size=26)
        return image

    if step_id == "use-3":
        image = slide_base(chapter_title, label, "Read the settings in Liquid")
        draw_code(image, (96, 240, 1000, 700), "appsettings.json", "json", CONFIGURATION, types=TYPES, size=26)
        note_cards(image, [
            ("Site", "The site settings in Liquid."),
            ("Properties", "Empty unless a name is listed."),
        ], (1050, 240, W - 96, 700))
        return image

    if step_id == "use-4":
        image = slide_base(chapter_title, label, "Recipes and deployment")
        draw_code(image, (96, 240, W - 96, 590), "recipe.json", "json", RECIPE, types=TYPES, size=22)
        draw_code(image, (96, 625, W - 96, 1035), "Startup.cs", "csharp", DEPLOYMENT, types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
