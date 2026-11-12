"""The slides of the "Write a Content Field With Its Editor and Settings" video.

assemble.py calls slide() for each step of a "slide" clip. Everything visual comes from brand.py, so the slides look like
those of every other Orchard Core video. The code is the code of the Boolean field of the Content Fields module of
Orchard Core (src/OrchardCore.Modules/OrchardCore.ContentFields), trimmed with // ... to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "BooleanField", "BooleanFieldSettings", "BooleanFieldDisplayDriver", "BooleanFieldSettingsDriver",
    "ContentField", "FieldSettings", "ContentFieldDisplayDriver", "ContentPartFieldDefinitionDisplayDriver",
    "IContentPartFieldDefinitionDisplayDriver", "IDisplayResult", "Task", "BuildFieldDisplayContext",
    "BuildFieldEditorContext", "UpdateFieldEditorContext", "DisplayBooleanFieldViewModel",
    "EditBooleanFieldViewModel", "ContentPart", "ContentPartFieldDefinition", "BuildEditorContext",
    "UpdatePartFieldEditorContext", "OrchardCoreConstants", "StartupBase", "IServiceCollection", "Startup",
    "ContentFieldOptionBuilder", "ContentFieldOptionBuilder", "Type", "HeaderSettingsViewModel",
    "TextFieldHeaderDisplaySettings", "TextFieldHeaderDisplaySettingsDriver", "TextField", "Feature",
    "ContentPartFieldDefinitionDisplayDriver",
}

WHAT = [
    ("1", "A class", "It inherits from ContentField. Its properties are the data."),
    ("2", "Saved as JSON", "Inside the content item, under the part and the name of the field."),
    ("3", "Listed in a part definition", "The part names the field, and picks its editor and display mode."),
    ("4", "Configured by settings", "Options such as a hint or a default value, kept in the definition."),
]

PIECES = [
    ("1", "The class", "A ContentField that holds the data, and a settings class."),
    ("2", "The display driver", "Display, Edit and UpdateAsync: the front end, the editor, the posted form."),
    ("3", "The views", "One Razor template for each shape."),
    ("4", "The settings driver", "Edits the options of the field, with its own view."),
]

FIELD_CLASS = """
public class BooleanField : ContentField
{
    public bool Value { get; set; }
}
"""

SETTINGS_CLASS = """
public class BooleanFieldSettings : FieldSettings
{
    public string Label { get; set; }

    public bool DefaultValue { get; set; }
}
"""

DRIVER_OUTLINE = """
public sealed class BooleanFieldDisplayDriver : ContentFieldDisplayDriver<BooleanField>
{
    public override IDisplayResult Display(BooleanField field, BuildFieldDisplayContext context)
    {
        // ...
    }

    public override IDisplayResult Edit(BooleanField field, BuildFieldEditorContext context)
    {
        // ...
    }

    public override async Task<IDisplayResult> UpdateAsync(BooleanField field, UpdateFieldEditorContext context)
    {
        // ...
    }
}
"""

DRIVER_DISPLAY = """
public override IDisplayResult Display(BooleanField field, BuildFieldDisplayContext context)
{
    return Initialize<DisplayBooleanFieldViewModel>(GetDisplayShapeType(context), model =>
    {
        model.Field = field;
        model.Part = context.ContentPart;
        model.PartFieldDefinition = context.PartFieldDefinition;
    })
    .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
    .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
}
"""

DRIVER_EDIT = """
public override IDisplayResult Edit(BooleanField field, BuildFieldEditorContext context)
{
    return Initialize<EditBooleanFieldViewModel>(GetEditorShapeType(context), model =>
    {
        model.Value = (context.IsNew == false) ?
            field.Value : context.PartFieldDefinition.GetSettings<BooleanFieldSettings>().DefaultValue;

        model.Field = field;
        model.Part = context.ContentPart;
        model.PartFieldDefinition = context.PartFieldDefinition;
    });
}
"""

DRIVER_UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(
    BooleanField field, UpdateFieldEditorContext context)
{
    await context.Updater.TryUpdateModelAsync(
        field, Prefix, f => f.Value);

    return Edit(field, context);
}
"""

BASE_UPDATE = """
Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;
// ...
var result = await UpdateAsync(field, updateFieldEditorContext);
// ...
if (result == null)
{
    return null;
}

contentPart.Apply(partFieldDefinition.Name, field);
"""

EDIT_VIEW_MODEL = """
public class EditBooleanFieldViewModel
{
    public bool Value { get; set; }
    public BooleanField Field { get; set; }
    public ContentPart Part { get; set; }
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
"""

DISPLAY_VIEW_MODEL = """
public class DisplayBooleanFieldViewModel
{
    public bool Value => Field.Value;
    public BooleanField Field { get; set; }
    public ContentPart Part { get; set; }
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
"""

EDIT_VIEW = """
@model OrchardCore.ContentFields.ViewModels.EditBooleanFieldViewModel
@{
    var settings = Model.PartFieldDefinition.GetSettings<BooleanFieldSettings>();
    // ...
}

<!-- ... -->
<div class="form-check">
    <input asp-for="Value" type="checkbox" class="form-check-input content-preview-select"
           checked="@Model.Value" />
    <label class="form-check-label" asp-for="Value">
        @(string.IsNullOrEmpty(settings.Label) ? localizedFieldName : settings.Label)
        <!-- ... -->
    </label>
</div>
<!-- ... -->
"""

DISPLAY_VIEW = """
@model OrchardCore.ContentFields.ViewModels.DisplayBooleanFieldViewModel
@using OrchardCore.Mvc.Utilities

@{
    var name = (Model.PartFieldDefinition.PartDefinition.Name + "-" + Model.PartFieldDefinition.Name).HtmlClassify();
}

<div class="field field-type-booleanfield field-name-@name">
    @Model.Value
</div>
"""

SETTINGS_EDIT = """
public sealed class BooleanFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<BooleanField>
{
    public override IDisplayResult Edit(
        ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<BooleanFieldSettings>("BooleanFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<BooleanFieldSettings>();

            model.Hint = settings.Hint;
            model.Label = settings.Label;
            model.DefaultValue = settings.DefaultValue;
        }).Location("Content");
    }

    // ...
}
"""

SETTINGS_UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(
    ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
{
    var model = new BooleanFieldSettings();

    await context.Updater.TryUpdateModelAsync(model, Prefix);

    context.Builder.WithSettings(model);

    return Edit(partFieldDefinition, context);
}
"""

SETTINGS_VIEW = """
@model OrchardCore.ContentFields.Settings.BooleanFieldSettings

<div class="ocat-wrapper">
    <label asp-for="Hint" class="ocat-label">@T["Hint"]</label>
    <!-- ... -->
</div>

<div class="ocat-wrapper">
    <label asp-for="Label" class="ocat-label">@T["Label"]</label>
    <!-- ... -->
</div>

<!-- ... -->
<input asp-for="DefaultValue" type="checkbox" class="form-check-input" />
<label class="form-check-label" asp-for="DefaultValue">@T["Default value is true"]</label>
<!-- ... -->
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...

        // Boolean Field
        services.AddContentField<BooleanField>()
            .UseDisplayDriver<BooleanFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();

        // ...
    }
}
"""

MANIFEST = """
[assembly: Feature(
    Id = "OrchardCore.ContentFields",
    Name = "Content Fields",
    // ...
    Dependencies = ["OrchardCore.ContentTypes", "OrchardCore.Shortcodes"]
)]
"""

FIELD_TYPES = """
// In the AdminController of the Content Types module:
_contentFieldTypes = contentOptions.Value.ContentFieldOptions.Select(cfo => cfo.Type);
"""

SWITCH_EDIT = """
@model OrchardCore.ContentFields.ViewModels.EditBooleanFieldViewModel
@{
    var settings = Model.PartFieldDefinition.GetSettings<BooleanFieldSettings>();
    // ...
}
<!-- ... -->
<div class="form-check form-switch">
    <input asp-for="Value" type="checkbox" class="form-check-input content-preview-select"
           checked="@Model.Value" />
    <label asp-for="Value" class="form-check-label">
        @(string.IsNullOrEmpty(settings.Label) ? localizedFieldName : settings.Label)
    </label>
    <!-- ... -->
</div>
"""

SWITCH_OPTION = """
@{
    string currentEditor = Model.Editor;
}
<option value="Switch" selected="@(currentEditor == "Switch")">@T["Switch"]</option>
"""

EDITOR_SHAPE_TYPE = """
// In ContentFieldDisplayDriver<TField>:
protected string GetEditorShapeType(string shapeType, ContentPartFieldDefinition partFieldDefinition)
{
    var editor = partFieldDefinition.Editor();
    return !string.IsNullOrEmpty(editor)
        ? shapeType + "__" + editor
        : shapeType;
}
"""

USE_DISPLAY_DRIVER = """
// UseDisplayDriver registers the driver for every display mode and every editor:
public static ContentFieldOptionBuilder UseDisplayDriver(
    this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType)
{
    return builder.ForDisplayMode(fieldDisplayDriverType)
        .ForEditor(fieldDisplayDriverType);
}
"""

HEADER_OPTION = """
@{
    string currentDisplayMode = Model.DisplayMode;
}
<option value="Header" selected="@(currentDisplayMode == "Header")">@T["Header"]</option>
"""

HEADER_SETTINGS = """
// TextFieldHeaderDisplaySettingsDriver
public override IDisplayResult Edit(
    ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
{
    return Initialize<HeaderSettingsViewModel>("TextFieldHeaderDisplaySettings_Edit", model =>
    {
        var settings = partFieldDefinition.GetSettings<TextFieldHeaderDisplaySettings>();

        model.Level = settings.Level;
    }).Location("DisplayMode");
}
"""


def panel(image, x0, y0, x1, title, language, code, size=24, highlight=None):
    """A code panel as high as its code needs. Returns its bottom, so the next panel can start below it."""
    lines = len(code.strip("\n").split("\n"))
    y1 = y0 + 74 + lines * int(size * 1.42) + 28
    draw_code(image, (x0, y0, x1, y1), title, language, code, types=TYPES, highlight=highlight, size=size)
    return y1


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "What a content field is")
        bullets(image, WHAT, active={0, 1} if step_id == "idea-1" else {2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Four pieces, then a registration")
        bullets(image, PIECES)
        return image

    if step_id in ("class-1", "class-2"):
        image = slide_base(chapter_title, label, "The Boolean field")
        first = step_id == "class-1"
        bottom = panel(image, 96, 240, W - 96, "BooleanField.cs", "csharp", FIELD_CLASS, size=32,
                       highlight=None if first else set())
        panel(image, 96, bottom + 30, W - 96, "BooleanFieldSettings.cs", "csharp", SETTINGS_CLASS, size=32,
              highlight=set() if first else None)
        return image

    if step_id == "driver-1":
        image = slide_base(chapter_title, label, "One display driver, three overrides")
        panel(image, 96, 240, W - 96, "BooleanFieldDisplayDriver.cs", "csharp", DRIVER_OUTLINE, size=24)
        return image

    if step_id == "driver-2":
        image = slide_base(chapter_title, label, "Display: the front end")
        bottom = panel(image, 96, 240, W - 96, "BooleanFieldDisplayDriver.cs", "csharp", DRIVER_DISPLAY, size=26)
        note_cards(image, [("The shape type", "BooleanField, or BooleanField_Display__ followed by the display mode")],
                   (96, bottom + 40, W - 96, 0))
        return image

    if step_id == "driver-3":
        image = slide_base(chapter_title, label, "Edit: the editor")
        bottom = panel(image, 96, 240, W - 96, "BooleanFieldDisplayDriver.cs", "csharp", DRIVER_EDIT, size=26)
        note_cards(image, [("The shape type", "BooleanField_Edit, or BooleanField_Edit__ followed by the editor")],
                   (96, bottom + 40, W - 96, 0))
        return image

    if step_id == "driver-4":
        image = slide_base(chapter_title, label, "UpdateAsync: the posted form")
        bottom = panel(image, 96, 240, W - 96, "BooleanFieldDisplayDriver.cs", "csharp", DRIVER_UPDATE, size=22)
        panel(image, 96, bottom + 20, W - 96, "ContentFieldDisplayDriver.cs", "csharp", BASE_UPDATE, size=22)
        return image

    if step_id == "views-1":
        image = slide_base(chapter_title, label, "A view model for each shape")
        bottom = panel(image, 96, 240, W - 96, "EditBooleanFieldViewModel.cs", "csharp", EDIT_VIEW_MODEL, size=26)
        panel(image, 96, bottom + 24, W - 96, "DisplayBooleanFieldViewModel.cs", "csharp", DISPLAY_VIEW_MODEL, size=26)
        return image

    if step_id == "views-2":
        image = slide_base(chapter_title, label, "The editor template")
        panel(image, 96, 240, W - 96, "BooleanField.Edit.cshtml", "html", EDIT_VIEW, size=24)
        return image

    if step_id == "views-3":
        image = slide_base(chapter_title, label, "The display template")
        bottom = panel(image, 96, 240, W - 96, "BooleanField.cshtml", "html", DISPLAY_VIEW, size=26)
        note_cards(image, [("Found by its name", "Views/BooleanField.cshtml renders the shape BooleanField")],
                   (96, bottom + 40, W - 96, 0))
        return image

    if step_id == "settings-1":
        image = slide_base(chapter_title, label, "The settings driver")
        panel(image, 96, 240, W - 96, "BooleanFieldSettingsDriver.cs", "csharp", SETTINGS_EDIT, size=24)
        return image

    if step_id == "settings-2":
        image = slide_base(chapter_title, label, "Saving the settings")
        bottom = panel(image, 96, 240, W - 96, "BooleanFieldSettingsDriver.cs", "csharp", SETTINGS_UPDATE, size=26)
        note_cards(image, [("Kept in the definition", "As BooleanFieldSettings, next to the editor and display mode")],
                   (96, bottom + 40, W - 96, 0))
        return image

    if step_id == "settings-3":
        image = slide_base(chapter_title, label, "The settings view")
        panel(image, 96, 240, W - 96, "BooleanFieldSettings.Edit.cshtml", "html", SETTINGS_VIEW, size=24)
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register the field in Startup")
        panel(image, 96, 240, W - 96, "Startup.cs", "csharp", STARTUP, size=26)
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "What registration changes")
        bottom = panel(image, 96, 240, W - 96, "Manifest.cs", "csharp", MANIFEST, size=26)
        panel(image, 96, bottom + 30, W - 96, "AdminController.cs", "csharp", FIELD_TYPES, size=26)
        return image

    if step_id == "editors-1":
        image = slide_base(chapter_title, label, "An editor is two templates")
        bottom = panel(image, 96, 240, W - 96, "BooleanField-Switch.Edit.cshtml", "html", SWITCH_EDIT, size=22)
        panel(image, 96, bottom + 24, W - 96, "BooleanField-Switch.Option.cshtml", "html", SWITCH_OPTION, size=22)
        return image

    if step_id == "editors-2":
        image = slide_base(chapter_title, label, "No change in the driver")
        bottom = panel(image, 96, 240, W - 96, "ContentFieldDisplayDriver.cs", "csharp", EDITOR_SHAPE_TYPE, size=26)
        panel(image, 96, bottom + 30, W - 96, "ContentFieldServiceCollectionExtensions.cs", "csharp",
              USE_DISPLAY_DRIVER, size=26)
        return image

    if step_id == "editors-3":
        image = slide_base(chapter_title, label, "Display modes, and their settings")
        bottom = panel(image, 96, 240, W - 96, "TextField-Header.DisplayOption.cshtml", "html", HEADER_OPTION, size=24)
        panel(image, 96, bottom + 24, W - 96, "TextFieldHeaderDisplaySettingsDriver.cs", "csharp", HEADER_SETTINGS,
              size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
