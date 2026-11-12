"""The slides of the video "Write a Content Part With a Display Driver". assemble.py calls slide() for each step of a
"slide" clip.

The code is the code of the Title part, from OrchardCore.Title in the Orchard Core repository, trimmed with comments
that read "..." where lines are left out. Everything visual comes from brand.py.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "TitlePart", "TitlePartViewModel", "TitlePartSettings", "TitlePartOptions", "TitlePartDisplayDriver",
    "TitlePartHandler", "ContentPart", "ContentItem", "ContentPartDisplayDriver", "IDisplayResult",
    "BuildPartDisplayContext", "BuildPartEditorContext", "UpdatePartEditorContext", "IStringLocalizer",
    "IContentDefinitionManager", "DataMigration", "Migrations", "Startup", "StartupBase", "IServiceCollection",
    "OrchardCoreConstants", "Task",
}

PART = """
using OrchardCore.ContentManagement;

namespace OrchardCore.Title.Models;

public class TitlePart : ContentPart
{
    public string Title { get; set; }
}
"""

VIEW_MODEL = """
public class TitlePartViewModel
{
    public string Title { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public TitlePart TitlePart { get; set; }

    [BindNever]
    public TitlePartSettings Settings { get; set; }
}
"""

MIGRATION = """
public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    // ...

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("TitlePart", builder => builder
            .Attachable()
            .WithDescription("Provides a Title for your content item.")
            .WithDefaultPosition("0")
            );

        // ...
        return 2;
    }
}
"""

DRIVER = """
public sealed class TitlePartDisplayDriver : ContentPartDisplayDriver<TitlePart>
{
    internal readonly IStringLocalizer S;

    public TitlePartDisplayDriver(IStringLocalizer<TitlePartDisplayDriver> localizer)
    {
        S = localizer;
    }

    // ...
}
"""

DISPLAY = """
public override IDisplayResult Display(TitlePart titlePart, BuildPartDisplayContext context)
{
    var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();

    if (!settings.RenderTitle || string.IsNullOrWhiteSpace(titlePart.Title))
    {
        return null;
    }

    return Initialize<TitlePartViewModel, TitlePart>(GetDisplayShapeType(context), static (model, titlePart) =>
    {
        model.Title = titlePart.ContentItem.DisplayText;
        model.TitlePart = titlePart;
        model.ContentItem = titlePart.ContentItem;
    }, titlePart).Location(OrchardCoreConstants.DisplayType.Detail, "Header")
    .Location(OrchardCoreConstants.DisplayType.Summary, "Header");
}
"""

EDIT = """
public override IDisplayResult Edit(TitlePart titlePart, BuildPartEditorContext context)
{
    return Initialize<TitlePartViewModel>(GetEditorShapeType(context), model =>
    {
        model.Title = titlePart.ContentItem.DisplayText;
        model.TitlePart = titlePart;
        model.ContentItem = titlePart.ContentItem;
        model.Settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();
    });
}
"""

UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(TitlePart model, UpdatePartEditorContext context)
{
    await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.Title);
    var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();

    if (settings.Options == TitlePartOptions.EditableRequired && string.IsNullOrWhiteSpace(model.Title))
    {
        context.Updater.ModelState.AddModelError(Prefix, nameof(model.Title), S["A value is required for Title."]);
    }
    else
    {
        model.ContentItem.DisplayText = model.Title;
    }

    return Edit(model, context);
}
"""

DISPLAY_TEMPLATE = """
@model OrchardCore.Title.ViewModels.TitlePartViewModel

<h1>@Model.Title</h1>
"""

SUMMARY_TEMPLATE = """
@model OrchardCore.Title.ViewModels.TitlePartViewModel

<h2><a display-for="@Model.TitlePart.ContentItem">@Model.Title</a></h2>
"""

EDIT_TEMPLATE = """
@model TitlePartViewModel
@* ... *@

<div class="ocat-wrapper" asp-validation-class-for="Title">
    <label asp-for="Title" class="ocat-label @(isRequired ? "ocat-label-required" : "")">@T["Title"]</label>
    <div class="ocat-end">
        <input asp-for="Title" class="form-control content-preview-text content-caption-text" />
        @* other attributes of the input left out *@
        <span asp-validation-for="Title"></span>
        @* ... *@
    </div>
</div>
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Title Part
        services.AddContentPart<TitlePart>()
            .UseDisplayDriver<TitlePartDisplayDriver>()
            .AddHandler<TitlePartHandler>();

        // ...

        services.AddDataMigration<Migrations>();
    }
}
"""

WHAT = [
    ("1", "Data", "The properties of the part, saved with the content item"),
    ("2", "An editor", "The fields you fill in when you create or edit an item"),
    ("3", "A display", "What visitors see on the pages of the site"),
]

PIECES = [
    ("1", "A class", "Derives from ContentPart and holds the data"),
    ("2", "A display driver", "Builds the shapes that edit and display the part"),
    ("3", "Templates", "One Razor file for each shape"),
    ("4", "A registration and a definition", "Make the part available to content types"),
]

FLOW = [
    ("1", "The driver creates a shape", "An object with the data to display"),
    ("2", "Its location decides where it goes", "A zone of the page, such as Header or Content"),
    ("3", "A template renders the shape", "Found from the shape type, in the Views folder"),
]

NAMES = [
    ("1", "TitlePart", "Views/TitlePart.cshtml: the display"),
    ("2", "TitlePart_Summary", "Views/TitlePart.Summary.cshtml: the Summary display type"),
    ("3", "TitlePart_Edit", "Views/TitlePart.Edit.cshtml: the editor"),
]

FURTHER = [
    ("1", "Settings", "A settings driver gives the part options for each content type"),
    ("2", "Handlers", "A handler reacts to the events of the content item"),
    ("3", "Placement", "A placement file moves or hides a shape, without changing the driver"),
    ("4", "Alternates", "A theme replaces a template for one content type"),
]


def lines_with(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    return {i + 1 for i, line in enumerate(code.strip("\n").split("\n")) if any(n in line for n in needles)}


def span(code, first, last):
    """The 1-based numbers of the lines from the first line that holds `first` to the next one that holds `last`."""
    lines = code.strip("\n").split("\n")
    start = next(i for i, line in enumerate(lines) if first in line)
    end = next(i for i in range(start, len(lines)) if last in lines[i])
    return set(range(start + 1, end + 2))


def code_panel(image, label, name, code, size=24, highlight=None):
    draw_code(image, (96, 240, W - 96, 1030), name, "csharp", code, types=TYPES, size=size, highlight=highlight)
    return image


def slide(step_id, chapter_title, label):
    if step_id == "pieces-1":
        image = slide_base(chapter_title, label, "A content part")
        bullets(image, WHAT)
        return image

    if step_id == "pieces-2":
        image = slide_base(chapter_title, label, "The pieces of a part")
        bullets(image, PIECES)
        return image

    if step_id == "pieces-3":
        image = slide_base(chapter_title, label, "From driver to page")
        bullets(image, FLOW)
        return image

    if step_id == "class-1":
        image = slide_base(chapter_title, label, "The class of the part")
        draw_code(image, (96, 240, 1060, 700), "Models/TitlePart.cs", "csharp", PART, types=TYPES, size=26)
        note_cards(image, [
            ("Derives from", "ContentPart"),
            ("Public properties", "hold the data"),
            ("Saved with the item", "under the name of the part"),
        ], (1110, 240, W - 96, 1030))
        return image

    if step_id == "class-2":
        image = slide_base(chapter_title, label, "The view model")
        draw_code(image, (96, 240, W - 96, 830), "ViewModels/TitlePartViewModel.cs", "csharp", VIEW_MODEL,
                  types=TYPES, size=26, highlight=lines_with(VIEW_MODEL, "BindNever", "Title { get; set; }", "ContentItem", "TitlePart TitlePart", "Settings"))
        return image

    if step_id == "class-3":
        image = slide_base(chapter_title, label, "The definition, in a migration")
        draw_code(image, (96, 240, W - 96, 1010), "Migrations.cs", "csharp", MIGRATION, types=TYPES, size=26,
                  highlight=lines_with(MIGRATION, "AlterPartDefinitionAsync", "Attachable", "WithDescription"))
        return image

    if step_id == "driver-1":
        image = slide_base(chapter_title, label, "The display driver")
        draw_code(image, (96, 240, W - 96, 800), "Drivers/TitlePartDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=26,
                  highlight=lines_with(DRIVER, "class TitlePartDisplayDriver", "IStringLocalizer", "// ..."))
        return image

    if step_id == "driver-2":
        image = slide_base(chapter_title, label, "Display: create the shape")
        return code_panel(image, "Display", "Drivers/TitlePartDisplayDriver.cs", DISPLAY,
                          highlight={1} | span(DISPLAY, "return Initialize", "titlePart)."))

    if step_id == "driver-3":
        image = slide_base(chapter_title, label, "Display: name it, and place it")
        return code_panel(image, "Display", "Drivers/TitlePartDisplayDriver.cs", DISPLAY,
                          highlight=lines_with(DISPLAY, "GetDisplayShapeType", ".Location("))

    if step_id == "driver-4":
        image = slide_base(chapter_title, label, "Display: when nothing is displayed")
        return code_panel(image, "Display", "Drivers/TitlePartDisplayDriver.cs", DISPLAY,
                          highlight=span(DISPLAY, "var settings", "return null") | {8} | lines_with(DISPLAY, ".Location("))

    if step_id == "driver-5":
        image = slide_base(chapter_title, label, "Edit: the shape of the editor")
        draw_code(image, (96, 240, W - 96, 800), "Drivers/TitlePartDisplayDriver.cs", "csharp", EDIT, types=TYPES, size=26,
                  highlight=lines_with(EDIT, "GetEditorShapeType"))
        return image

    if step_id == "driver-6":
        image = slide_base(chapter_title, label, "UpdateAsync: read the form")
        return code_panel(image, "UpdateAsync", "Drivers/TitlePartDisplayDriver.cs", UPDATE,
                          highlight=lines_with(UPDATE, "UpdateAsync", "TryUpdateModelAsync", "Prefix, t =>"))

    if step_id == "driver-7":
        image = slide_base(chapter_title, label, "UpdateAsync: validate")
        return code_panel(image, "UpdateAsync", "Drivers/TitlePartDisplayDriver.cs", UPDATE,
                          highlight=lines_with(UPDATE, "EditableRequired", "AddModelError", "return Edit"))

    if step_id == "templates-1":
        image = slide_base(chapter_title, label, "Shape types and template files")
        bullets(image, NAMES)
        return image

    if step_id == "templates-2":
        image = slide_base(chapter_title, label, "The display and editor templates")
        draw_code(image, (96, 240, W - 96, 450), "Views/TitlePart.cshtml", "cshtml", DISPLAY_TEMPLATE, size=24)
        draw_code(image, (96, 490, W - 96, 1030), "Views/TitlePart.Edit.cshtml", "cshtml", EDIT_TEMPLATE, size=22,
                  highlight=lines_with(EDIT_TEMPLATE, "asp-for", "asp-validation-for"))
        return image

    if step_id == "templates-3":
        image = slide_base(chapter_title, label, "The Summary template")
        draw_code(image, (96, 240, W - 96, 450), "Views/TitlePart.cshtml", "cshtml", DISPLAY_TEMPLATE, size=24)
        draw_code(image, (96, 490, W - 96, 700), "Views/TitlePart.Summary.cshtml", "cshtml", SUMMARY_TEMPLATE, size=24)
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register the part")
        draw_code(image, (96, 240, W - 96, 860), "Startup.cs", "csharp", STARTUP, types=TYPES, size=26,
                  highlight=lines_with(STARTUP, "AddContentPart", "UseDisplayDriver", "AddHandler", "AddDataMigration"))
        return image

    if step_id == "further-1":
        image = slide_base(chapter_title, label, "Going further")
        bullets(image, FURTHER, active={0, 1})
        return image

    if step_id == "further-2":
        image = slide_base(chapter_title, label, "Going further")
        bullets(image, FURTHER, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")
