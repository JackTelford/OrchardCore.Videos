"""The slides of the Content Definitions in Code video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the SEO module (Manifest.cs, Startup.cs, Migrations.cs, the socialmetasettings recipe and
SeoMetaPart.cs of OrchardCore.Modules/OrchardCore.Seo) and from the content type documentation, trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "ContentItem", "ContentItemAspectContext", "ContentPart", "IContentDefinitionManager", "IRecipeMigrator",
    "IServiceCollection", "MediaField", "MediaFieldSettings", "Migrations", "RecipesConstants", "SeoMetaPart",
    "SeoMetaPartDisplayDriver", "SeoMetaPartHandler", "SeoAspect", "StartupBase", "Startup", "Task", "DataMigration",
}

DEFINITIONS = [
    ("1", "Built in the admin", "The definition lives in the database of one site."),
    ("2", "Defined in code", "The module carries its types, parts and fields to every site."),
    ("3", "Data migration", "A C# class that alters definitions, one version at a time."),
    ("4", "Recipe", "A JSON file that a migration can run."),
]

MANIFEST = """
[assembly: Module(
    Name = "SEO",
    // ...
    Dependencies =
    [
        "OrchardCore.Contents",
        "OrchardCore.Recipes.Core",
        "OrchardCore.Media",
        "OrchardCore.Shortcodes",
    ]
)]
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        // ...
        services.AddContentPart<SeoMetaPart>()
            .UseDisplayDriver<SeoMetaPartDisplayDriver>()
            .AddHandler<SeoMetaPartHandler>();

        // ...
    }
}
"""

MIGRATION_CLASS = """
public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _recipeMigrator = recipeMigrator;
    }

    // CreateAsync and UpdateFrom1Async ...
}
"""

PRODUCT_TYPE = """
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    // Items of this type can be created in the admin.
    .Creatable()
    // Items can have drafts, and their versions are saved.
    .Draftable()
    .Versionable()
    // Permissions can be set for items of this type.
    .Securable()
    .WithPart("TitlePart", part => part
        .WithPosition("0")));
"""

DISPLAY_NAME = """
// Created as "ProductReview", the type is shown as "Product Review".
await _contentDefinitionManager.AlterTypeDefinitionAsync("ProductReview", type => type
    .Creatable());

// WithDisplayName chooses another label.
await _contentDefinitionManager.AlterTypeDefinitionAsync("ProductReview", type => type
    .WithDisplayName("Customer review"));
"""

SEO_PART = """
await _contentDefinitionManager.AlterPartDefinitionAsync("SeoMetaPart", builder => builder
    .Attachable()
    .WithDescription("Provides a part that allows SEO meta descriptions to be applied to a content item.")
    .WithField("DefaultSocialImage", field => field
        .OfType("MediaField")
        .WithDisplayName("Default social image")
        .WithSettings(new MediaFieldSettings { Multiple = false }))
    // ...
);
"""

NAMED_PART = """
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .WithPart("Product"));

await _contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
    .WithField("Price", field => field
        .OfType("NumericField")
        .WithDisplayName("Price")));
"""

RECIPE_CALL = """
await _recipeMigrator.ExecuteAsync("socialmetasettings.recipe.json", this);
"""

MODULE_FOLDER = """
OrchardCore.Seo/
    Manifest.cs
    Migrations.cs
    Startup.cs
    ...
    Migrations/
        socialmetasettings.recipe.json
"""

RECIPE = """
{
  "name": "ReplaceContentDefinition",
  "ContentTypes": [
    {
      "Name": "SocialMetaSettings",
      "DisplayName": "Social Meta Settings",
      "Settings": { "ContentTypeSettings": { "Stereotype": "CustomSettings" }, "FullTextAspectSettings": {} },
      "ContentTypePartDefinitionRecords": [
        { "PartName": "SocialMetaSettings", "Name": "SocialMetaSettings", "Settings": { /* ... */ } }
      ]
    }
  ],
  "ContentParts": [
    {
      "Name": "SocialMetaSettings",
      "Settings": {},
      "ContentPartFieldDefinitionRecords": [
        { "FieldName": "MediaField", "Name": "DefaultSocialImage", "Settings": { /* ... */ } }
      ]
    }
  ]
}
"""

RECIPE_STEPS = [
    ("1", "ContentDefinition", "Adds and updates types and parts."),
    ("2", "ReplaceContentDefinition", "Deletes each listed type and part, then creates it again."),
    ("3", "DeleteContentDefinition", "Removes types and parts by name."),
]

VERSIONS = """
public async Task<int> CreateAsync()
{
    await _contentDefinitionManager.AlterPartDefinitionAsync("SeoMetaPart", builder => builder
        .Attachable()
        // ...
    );

    await _recipeMigrator.ExecuteAsync("socialmetasettings.recipe.json", this);

    return 2;
}

public async Task<int> UpdateFrom1Async()
{
    await _recipeMigrator.ExecuteAsync($"socialmetasettings{RecipesConstants.RecipeExtension}", this);

    return 2;
}
"""

REMOVE = """
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .RemovePart("TitlePart"));

await _contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
    .RemoveField("Price"));

await _contentDefinitionManager.DeleteTypeDefinitionAsync("Product");
await _contentDefinitionManager.DeletePartDefinitionAsync("Product");
"""

PART_CLASS = """
public class SeoMetaPart : ContentPart
{
    // ...
    public string PageTitle { get; set; }

    // ...
    public string MetaDescription { get; set; }

    // ...
    public MediaField DefaultSocialImage { get; set; }

    // ...
    public MediaField OpenGraphImage { get; set; }

    // ...
    public MediaField TwitterImage { get; set; }

    // ...
}
"""

HANDLER = """
public override Task GetContentItemAspectAsync(ContentItemAspectContext context, SeoMetaPart part)
{
    return context.ForAsync<SeoAspect>(async aspect =>
    {
        if (part.DefaultSocialImage?.Paths?.Length > 0)
        {
            // ...
        }
    });
}
"""

GET_OR_CREATE = """
var part = contentItem.GetOrCreate<SeoMetaPart>();
var paths = part.DefaultSocialImage?.Paths;
"""

REGISTER = """
services.AddContentPart<SeoMetaPart>();
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Where a definition lives")
        bullets(image, DEFINITIONS, active={0, 1})
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two tools for definitions in code")
        bullets(image, DEFINITIONS, active={2, 3})
        return image

    if step_id == "class-1":
        image = slide_base(chapter_title, label, "1. The manifest lists the dependencies")
        draw_code(image, (96, 240, 1150, 780), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=24)
        note_cards(image, [
            ("Enabled with the module", "Its dependencies are enabled too."),
            ("Migrated first", "Their migrations run before yours."),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "class-2":
        image = slide_base(chapter_title, label, "2. Register the migration")
        draw_code(image, (96, 240, W - 96, 830), "Startup.cs", "csharp", STARTUP, types=TYPES, size=26,
                  highlight={5, 7, 8, 9})
        return image

    if step_id == "class-3":
        image = slide_base(chapter_title, label, "3. Inherit from DataMigration")
        draw_code(image, (96, 240, W - 96, 860), "Migrations.cs", "csharp", MIGRATION_CLASS, types=TYPES, size=25)
        return image

    if step_id in ("type-1", "type-2"):
        image = slide_base(chapter_title, label, "Create a type" if step_id == "type-1" else "Attach its parts")
        draw_code(image, (96, 240, W - 96, 760), "Migrations.cs", "csharp", PRODUCT_TYPE, types=TYPES, size=26,
                  highlight={1, 2, 3, 4, 5, 6, 7, 8} if step_id == "type-1" else {1, 9, 10})
        return image

    if step_id == "type-3":
        image = slide_base(chapter_title, label, "The display name of a type")
        draw_code(image, (96, 240, W - 96, 700), "Migrations.cs", "csharp", DISPLAY_NAME, types=TYPES, size=26)
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "Create a part and its fields")
        draw_code(image, (96, 240, W - 96, 760), "Migrations.cs", "csharp", SEO_PART, types=TYPES, size=24)
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "A part named like the type")
        draw_code(image, (96, 240, W - 96, 760), "Migrations.cs", "csharp", NAMED_PART, types=TYPES, size=26)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Run a recipe from a migration")
        draw_code(image, (96, 240, W - 96, 400), "Migrations.cs", "csharp", RECIPE_CALL, types=TYPES, size=26)
        draw_code(image, (96, 450, W - 96, 850), "The folder of the module", "bash", MODULE_FOLDER, size=26)
        return image

    if step_id in ("recipe-2", "recipe-3"):
        image = slide_base(chapter_title, label, "A recipe step" if step_id == "recipe-2" else "Give every type a DisplayName")
        draw_code(image, (96, 240, W - 96, 1030), "socialmetasettings.recipe.json (one step)", "json", RECIPE, size=22,
                  highlight=None if step_id == "recipe-2" else {6, 7, 8, 9, 10})
        return image

    if step_id == "recipe-4":
        image = slide_base(chapter_title, label, "Three steps of the Content Types feature")
        bullets(image, RECIPE_STEPS)
        return image

    if step_id in ("version-1", "version-2"):
        image = slide_base(chapter_title, label, "Versions" if step_id == "version-1" else "One method for each change")
        draw_code(image, (96, 240, W - 96, 960), "Migrations.cs", "csharp", VERSIONS, types=TYPES, size=24,
                  highlight={1, 10, 13, 17} if step_id == "version-1" else {8, 13, 14, 15, 16, 17, 18})
        return image

    if step_id == "version-3":
        image = slide_base(chapter_title, label, "Remove explicitly")
        draw_code(image, (96, 240, W - 96, 760), "Migrations.cs", "csharp", REMOVE, types=TYPES, size=26)
        return image

    if step_id == "cs-1":
        image = slide_base(chapter_title, label, "A class named like the part")
        draw_code(image, (96, 240, W - 96, 1030), "SeoMetaPart.cs", "csharp", PART_CLASS, types=TYPES, size=26)
        return image

    if step_id == "cs-2":
        image = slide_base(chapter_title, label, "Register it, then read it")
        draw_code(image, (96, 240, W - 96, 380), "Startup.cs", "csharp", REGISTER, types=TYPES, size=26)
        draw_code(image, (96, 420, W - 96, 850), "A part handler", "csharp", HANDLER, types=TYPES, size=23)
        draw_code(image, (96, 870, W - 96, 1030), "Anywhere else", "csharp", GET_OR_CREATE, types=TYPES, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
