"""The slides of the Liquid Part and Liquid Field video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the OrchardCore.Liquid module (and the Content Types documentation) and trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"ShapeTableBuilder", "ShapeDisplayContext", "ContentItem", "FluidValue", "ObjectValue", "ILiquidTemplateManager",
         "ContentItemAspectContext", "LiquidPart", "BodyAspect", "StringGraphType", "Permission", "LocalizationSource",
         "PermissionStereotype", "HashSet", "StringComparer", "CommonPermissions", "Permissions", "Task", "ValueTask",
         "IEnumerable"}

IDEA = [
    ("1", "Liquid part", "The body of a content item, written in Liquid."),
    ("2", "Liquid field", "A named block inside a part, written in Liquid."),
    ("3", "Stored as submitted", "The source is saved unchanged, and runs when the content is displayed."),
    ("4", "Other body types", "HtmlBodyPart, HtmlField, MarkdownBodyPart and MarkdownField keep it as text."),
]

CODE_MIGRATION = """
// The Liquid part is attached by name.
await _contentDefinitionManager.AlterTypeDefinitionAsync("Notice", type => type
    .WithPart("TitlePart")
    .WithPart("LiquidPart")
    .WithPart("Notice")
    .Creatable()
    .Listable()
);

// The fields of a type live in the part named after it.
await _contentDefinitionManager.AlterPartDefinitionAsync("Notice", part => part
    .WithField("ClosingNote", field => field
        .OfType("LiquidField")
        .WithDisplayName("Closing Note")
    )
);
"""

# LiquidShapes.cs: lines 1 to 7 describe the shapes, lines 11 to 19 render the stored source.
LIQUID_SHAPES = """
public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
{
    builder.Describe("LiquidPart").OnProcessing(BuildViewModelAsync);
    builder.Describe("LiquidPart_Summary").OnProcessing(BuildViewModelAsync);
    builder.Describe("LiquidField").OnProcessing(BuildViewModelAsync);
    // ...
}

// ...

private async Task<string> RenderAsync(ShapeDisplayContext shapeDisplayContext, string liquid, ContentItem contentItem)
{
    var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
    return await liquidTemplateManager.RenderStringAsync(liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
        new Dictionary<string, FluidValue>()
        {
            ["ContentItem"] = ObjectValue.Create(contentItem),
        });
}
"""

VIEW_PART = """
@model OrchardCore.Liquid.ViewModels.LiquidPartViewModel
@Html.Raw(Model.Html)
"""

VIEW_SUMMARY = """
@using OrchardCore.Mvc.Utilities
@model OrchardCore.Liquid.ViewModels.LiquidPartViewModel
@Html.Raw(Model.Html.RemoveTags(true).Ellipsize(200))
"""

VIEW_FIELD = """
@using OrchardCore.Mvc.Utilities
@model OrchardCore.Liquid.ViewModels.LiquidFieldViewModel

@{
    var name = (Model.PartFieldDefinition.PartDefinition.Name + "-" + Model.PartFieldDefinition.Name).HtmlClassify();
}

<div class="field field-type-liquidfield field-name-@name">
    @Html.Raw(Model.Html)
</div>
"""

ASPECT = """
public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LiquidPart part)
{
    return context.ForAsync<BodyAspect>(async bodyAspect =>
    {
        // ...
        var result = await _liquidTemplateManager.RenderHtmlContentAsync(part.Liquid, _htmlEncoder, model,
            new Dictionary<string, FluidValue>() { ["ContentItem"] = ObjectValue.Create(model.ContentItem) });

        bodyAspect.Body = result;
        // ...
    });
}
"""

GRAPHQL = """
Field<StringGraphType>("liquid")
    .Description(S["the rendered Liquid content"])
    .ResolveLockedAsync(RenderLiquidAsync);
"""

PERMISSION = """
public static readonly Permission ManageLiquidTemplates = new(
    "ManageLiquidTemplates",
    LocalizationSource.Create<Permissions>("Manage Liquid templates"),
    isSecurityCritical: true);

// ... _allPermissions holds ManageLiquidTemplates.
public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
[
    new PermissionStereotype
    {
        Name = OrchardCoreConstants.Roles.Administrator,
        Permissions = _allPermissions,
    },
    new PermissionStereotype
    {
        Name = OrchardCoreConstants.Roles.Editor,
        Permissions = _allPermissions,
    },
];
"""

HANDLER = """
private static readonly HashSet<string> s_mutationPermissions = new(StringComparer.Ordinal)
{
    CommonPermissions.EditContent.Name,
    CommonPermissions.EditOwnContent.Name,
    CommonPermissions.PublishContent.Name,
    CommonPermissions.PublishOwnContent.Name,
};

// ...
if (!await authorizationService.AuthorizeAsync(
    context.User, Permissions.ManageLiquidTemplates))
{
    context.Fail();
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "Liquid inside content")
        bullets(image, IDEA, active={0, 1} if step_id == "idea-1" else {2, 3})
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "Attach the part and the field in a migration")
        draw_code(image, (96, 240, W - 96, 975), "Migrations.cs", "csharp", CODE_MIGRATION, types=TYPES, size=28)
        return image

    if step_id in ("render-1", "render-2"):
        image = slide_base(chapter_title, label, "Liquid runs when the shape is processed")
        # render-1 follows the shapes that are described, render-2 the rendering: the shape as Model, ContentItem.
        highlight = set(range(1, 8)) if step_id == "render-1" else set(range(11, 20))
        draw_code(image, (96, 240, W - 96, 990), "LiquidShapes.cs", "csharp", LIQUID_SHAPES, types=TYPES,
                  highlight=highlight, size=24)
        return image

    if step_id == "render-3":
        image = slide_base(chapter_title, label, "The views write the Html")
        draw_code(image, (96, 240, 940, 440), "LiquidPart.cshtml", "html", VIEW_PART, types=TYPES, size=22)
        draw_code(image, (980, 240, W - 96, 440), "LiquidPart.Summary.cshtml", "html", VIEW_SUMMARY, types=TYPES, size=22)
        draw_code(image, (96, 480, W - 96, 960), "LiquidField.cshtml", "html", VIEW_FIELD, types=TYPES, size=24)
        return image

    if step_id == "render-4":
        image = slide_base(chapter_title, label, "Other consumers of the same source")
        draw_code(image, (96, 240, W - 96, 745), "LiquidPartHandler.cs", "csharp", ASPECT, types=TYPES, size=24)
        draw_code(image, (96, 785, W - 96, 995), "LiquidFieldQueryObjectType.cs", "csharp", GRAPHQL, types=TYPES, size=24)
        return image

    if step_id == "safe-1":
        image = slide_base(chapter_title, label, "A security-critical permission")
        draw_code(image, (96, 240, W - 96, 985), "Permissions.cs", "csharp", PERMISSION, types=TYPES, size=24)
        return image

    if step_id == "safe-2":
        image = slide_base(chapter_title, label, "It protects the whole content item")
        draw_code(image, (96, 240, 1300, 800), "LiquidContentAuthorizationHandler.cs", "csharp", HANDLER, types=TYPES, size=22)
        note_cards(image, [
            ("Edit and publish", "both need it."),
            ("Whole item", "is refused, not a part."),
            ("Nested content", "in Flow or Bag counts."),
        ], (1340, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
