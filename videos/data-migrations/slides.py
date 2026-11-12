"""The slides of the Data Migrations video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (the Forms, Menu and Alias modules) and from the documentation of data migrations,
trimmed with // ... to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MARGIN, W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {
    "DataMigration", "IContentDefinitionManager", "IRecipeMigrator", "ISession", "ProductIndex", "MapIndex",
    "Migrations", "IndexMigrations", "Startup", "IndexingStartup", "StartupBase", "IServiceCollection", "ContentItem",
    "ContentItemIndex", "Task", "Feature",
}

# Code panels.

CLASS_CREATE = """
public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        // ...

        return 1;
    }
}
"""

CLASS_UPDATE = """
public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        // ...

        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        // ...

        return 2;
    }
}
"""

FORMS = """
public async Task<int> CreateAsync()
{
    await _contentDefinitionManager.AlterTypeDefinitionAsync("Form", type => type
        // ...
        .WithPart("FormPart", part => part.WithPosition("2"))
        // ...
        .Stereotype("Widget"));

    // ...

    return 5;
}

public async Task<int> UpdateFrom1Async()
{
    // ...

    return 2;
}

// ... UpdateFrom2Async to UpdateFrom4Async
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace Products;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
    }
}
"""

FEATURE_STARTUP = """
[Feature("Products.Indexing")]
public sealed class IndexingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<IndexMigrations>();
    }
}
"""

INJECTION = """
public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly ISession _session;

    public Migrations(
        IRecipeMigrator recipeMigrator,
        ISession session)
    {
        _recipeMigrator = recipeMigrator;
        _session = session;
    }

    // ...
}
"""

CONTENT_CREATE = """
public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            .WithPart("TitlePart")
        );

        return 1;
    }
}
"""

CONTENT_OPTIONS = """
public async Task<int> UpdateFrom1Async()
{
    await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
        .Creatable()
        .Listable()
        .Draftable()
        .Versionable()
        .Securable()
    );

    return 2;
}
"""

INDEX_CLASS = """
public class ProductIndex : MapIndex
{
    public string Name { get; set; }
    public string Category { get; set; }
}
"""

INDEX_TABLE = """
public async Task<int> UpdateFrom2Async()
{
    await SchemaBuilder.CreateMapIndexTableAsync<ProductIndex>(table => table
        .Column<string>("Name", column => column.WithLength(100))
        .Column<string>("Category", column => column.WithLength(50))
    );

    return 3;
}
"""

INDEX_ALTER = """
public async Task<int> UpdateFrom3Async()
{
    await SchemaBuilder.AlterIndexTableAsync<ProductIndex>(table => table
        .CreateIndex("IDX_ProductIndex_Category", "Category")
    );

    return 4;
}
"""

RECIPE = """
public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> UpdateFrom4Async()
    {
        await _recipeMigrator.ExecuteAsync("sample-products.recipe.json", this);

        return 5;
    }
}
"""

TREE = """
Products/
  Migrations/
    sample-products.recipe.json
  Migrations.cs
  Products.csproj
  Startup.cs
"""

CONTENT_UPDATE = """
public async Task<int> UpdateFrom3Async()
{
    var menus = await _session
        .Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Menu")
        .ListAsync();

    foreach (var menu in menus)
    {
        // ...

        await _session.SaveAsync(menu);
    }

    return 4;
}
"""

# Cards.

WHY = [
    ("1", "Data comes first", "A module needs content types, index tables or starter content before it works."),
    ("2", "Every tenant differs", "Each tenant has its own data, and enables features at different times."),
    ("3", "Numbered steps", "A migration is a class of steps, and each tenant records the version it reached."),
    ("4", "Run in order", "When a tenant starts, Orchard Core runs the steps that are missing."),
]

STEPS = [
    ("1", "Names", "Create or CreateAsync, and UpdateFrom plus a number, with or without Async."),
    ("2", "Return type", "int or Task<int>: the version the step leaves."),
    ("3", "Anything else", "Another name or return type is not a step, and Orchard Core ignores it."),
]

TENANTS = [
    ("1", "No version stored", "CreateAsync runs, then the updates that continue from its number."),
    ("2", "Version 1 stored", "UpdateFrom1Async runs, then the steps after it."),
    ("3", "Last version stored", "Nothing runs, and the tenant starts as it is."),
]

KEEP = [
    ("1", "Keep the numbers", "Changing a number can disrupt the migrations of other tenants."),
    ("2", "Keep earlier steps", "A step that ran on a tenant never runs again for that tenant."),
    ("3", "Add a step", "Use the next UpdateFrom method for each change."),
]

RECORD = [
    ("1", "One record per tenant", "It lists each migration class by its full name, with its version."),
    ("2", "Rename the class", "Or change its namespace, and Orchard Core finds no version for it."),
    ("3", "CreateAsync runs again", "The class is treated as a migration that never ran."),
]

FAILURE = [
    ("1", "The error is logged", "The message names the migration, its feature and its version."),
    ("2", "The tenant starts", "A step that throws doesn't stop the tenant from starting."),
    ("3", "Read the log", "Check it when a table or content type you expect is missing."),
]

# Drawing, with checks that nothing overflows.

CODE_LEFT, CODE_RIGHT = MARGIN, W - MARGIN


def check_width(text, text_font, limit, what):
    if text_font.getlength(text) > limit:
        raise SystemExit(f"{what} is too wide ({int(text_font.getlength(text))} px, at most {int(limit)}): {text!r}")


def heading(chapter_title, label, text):
    check_width(text, font(52, "Bold"), W - 2 * MARGIN, "The heading")
    return slide_base(chapter_title, label, text)


def code(image, box, title, language, source, size=24, highlight=None):
    """A code panel that fits its content: the height shrinks to the code, and the code must fit the panel."""
    x0, y0, x1, y1 = box
    lines = source.strip("\n").split("\n")
    line_height = int(size * 1.42)
    for line in lines:
        check_width(line, mono(size), x1 - x0 - 30 - 20, f"A line of {title}")
    needed = 74 + len(lines) * line_height + 14
    if needed > y1 - y0:
        raise SystemExit(f"{title} has {len(lines)} lines at {size} px: it needs {needed} px, the panel has {y1 - y0}")
    draw_code(image, (x0, y0, x1, y0 + needed), title, language, source, types=TYPES, highlight=highlight, size=size)
    return y0 + needed


def cards(image, items, active=None):
    for _, title, text in items:
        check_width(title, font(36, "Bold"), W - 2 * MARGIN - 140 - 30, "A card title")
        check_width(text, font(28), W - 2 * MARGIN - 140 - 30, "A card text")
    bullets(image, items, active=active)


def notes(image, items, box):
    x0, _, x1, _ = box
    for title, text in items:
        check_width(title, font(34, "Bold"), x1 - x0 - 80, "A note title")
        check_width(text, font(30), x1 - x0 - 80, "A note text")
    note_cards(image, items, box)


NOTES_BOX = (1290, 240, CODE_RIGHT, 1030)


def side_by_side(image, title, source, size, side_notes, highlight=None):
    code(image, (CODE_LEFT, 240, 1250, 1030), title, "csharp", source, size=size, highlight=highlight)
    notes(image, side_notes, NOTES_BOX)


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = heading(chapter_title, label, "Why a module needs migrations")
        cards(image, WHY, active={0, 1} if step_id == "why-1" else {2, 3})
        return image

    if step_id == "class-1":
        image = heading(chapter_title, label, "1. Inherit from DataMigration, add CreateAsync")
        side_by_side(image, "Migrations.cs", CLASS_CREATE, 26, [
            ("Base class", "DataMigration"),
            ("First step", "CreateAsync"),
            ("It returns", "The version reached"),
        ])
        return image

    if step_id == "class-2":
        image = heading(chapter_title, label, "2. Add UpdateFrom steps")
        side_by_side(image, "Migrations.cs", CLASS_UPDATE, 26, [
            ("Name", "UpdateFrom + a version"),
            ("Runs when", "The stored version is 1"),
            ("It returns", "The version it leaves"),
        ], highlight={1, 2, 10, 11, 12, 13, 14, 15, 16})
        return image

    if step_id == "class-3":
        image = heading(chapter_title, label, "3. Which methods are steps")
        cards(image, STEPS)
        return image

    if step_id == "class-4":
        image = heading(chapter_title, label, "4. What runs on each tenant")
        cards(image, TENANTS)
        return image

    if step_id == "class-5":
        image = heading(chapter_title, label, "5. CreateAsync can return a higher number")
        side_by_side(image, "Migrations.cs (Forms module)", FORMS, 22, [
            ("No version", "CreateAsync returns 5."),
            ("Stored version 2", "Updates 2, 3 and 4 run."),
            ("Stored version 5", "Nothing runs."),
        ], highlight={1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12})
        return image

    if step_id == "reg-1":
        image = heading(chapter_title, label, "1. Register it in a Startup class")
        code(image, (CODE_LEFT, 240, CODE_RIGHT, 1030), "Startup.cs", "csharp", STARTUP, size=26,
             highlight={2, 9, 10, 11, 12})
        return image

    if step_id == "reg-2":
        image = heading(chapter_title, label, "2. Tie it to a feature")
        side_by_side(image, "IndexingStartup.cs", FEATURE_STARTUP, 24, [
            ("Its feature", "The one of the Startup."),
            ("It runs only", "Where that feature is on."),
            ("Register it", "In one Startup class."),
        ])
        return image

    if step_id == "reg-3":
        image = heading(chapter_title, label, "3. Ask for services in the constructor")
        code(image, (CODE_LEFT, 240, CODE_RIGHT, 1030), "Migrations.cs (Menu module)", "csharp", INJECTION, size=26)
        return image

    if step_id == "cd-1":
        image = heading(chapter_title, label, "Create a content type")
        code(image, (CODE_LEFT, 240, CODE_RIGHT, 1030), "Migrations.cs", "csharp", CONTENT_CREATE, size=26,
             highlight={10, 11, 12, 13, 14, 15, 16, 17})
        return image

    if step_id == "cd-2":
        image = heading(chapter_title, label, "Set the options of the type")
        side_by_side(image, "Migrations.cs", CONTENT_OPTIONS, 24, [
            ("Creatable, Listable", "Create and list items."),
            ("Draftable, Versionable", "Drafts and versions."),
            ("Securable", "Custom permissions."),
        ])
        return image

    if step_id == "idx-1":
        image = heading(chapter_title, label, "Create the table of a map index")
        bottom = code(image, (CODE_LEFT, 240, CODE_RIGHT, 1030), "ProductIndex.cs", "csharp", INDEX_CLASS, size=26)
        code(image, (CODE_LEFT, bottom + 30, CODE_RIGHT, 1030), "Migrations.cs", "csharp", INDEX_TABLE, size=26)
        return image

    if step_id == "idx-2":
        image = heading(chapter_title, label, "Add a database index")
        side_by_side(image, "Migrations.cs", INDEX_ALTER, 24, [
            ("Alters a table", "AlterIndexTableAsync"),
            ("CreateIndex", "A name, then columns."),
            ("The effect", "Faster fetches by Category."),
        ])
        return image

    if step_id == "rec-1":
        image = heading(chapter_title, label, "Run a recipe from a step")
        code(image, (CODE_LEFT, 240, CODE_RIGHT, 1030), "Migrations.cs", "csharp", RECIPE, size=26,
             highlight={10, 11, 12, 13, 14, 15})
        return image

    if step_id == "rec-2":
        image = heading(chapter_title, label, "Where the recipe file goes")
        side_by_side(image, "The module's folder", TREE, 26, [
            ("Folder", "Migrations, in the module."),
            ("File name", "The ExecuteAsync argument."),
            ("Use it for", "Changes that are data."),
        ])
        return image

    if step_id == "upd-1":
        image = heading(chapter_title, label, "Change content that already exists")
        side_by_side(image, "Migrations.cs (Menu module)", CONTENT_UPDATE, 24, [
            ("1. Query", "Items of one content type."),
            ("2. Change", "Each content item."),
            ("3. Save", "With the session."),
        ])
        return image

    if step_id == "rules-1":
        image = heading(chapter_title, label, "Keep released steps as they are")
        cards(image, KEEP)
        return image

    if step_id == "rules-2":
        image = heading(chapter_title, label, "The record that tracks the versions")
        cards(image, RECORD)
        return image

    if step_id == "rules-3":
        image = heading(chapter_title, label, "When a step fails")
        cards(image, FAILURE)
        return image

    raise SystemExit(f"No slide for {step_id}")
