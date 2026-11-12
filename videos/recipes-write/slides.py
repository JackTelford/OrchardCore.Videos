"""The slides of the "Write an Orchard Core Recipe" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"DataMigration", "IRecipeMigrator", "Task"}

TREE = """
Bakery.Starter/
  Manifest.cs
  Bakery.Starter.csproj
  Recipes/
    bakery-pages.recipe.json
    Snippets/
      welcome.html
"""

STEPS_SHAPE = """
"steps": [
  { "name": "feature", "enable": [ "OrchardCore.Html" ] },
  { "name": "themes", "site": "TheBlogTheme" },
  // ...
]
"""

HEADER = """
{
  "name": "BakeryPages",
  "displayName": "Bakery Pages",
  "description": "Creates the Bakery Page content type and a home page.",
  "author": "Greenfield Bakery",
  "website": "https://example.com",
  "version": "1.0.0",
  "tags": [ "bakery", "content" ],
  // ...
}
"""

COMMENTS = """
{
  "name": "BakeryPages",
  // A comment explains a step where it appears.
  "tags": [ "bakery", "content", ],
  // ...
}
"""

VARIABLES = """
"variables": {
  "homePageId": "[js:uuid()]",
  "timestamp": "[js:new Date().toISOString()]"
},
"""

VARIABLE_USE = """
"ContentItemId": "[js:variables('homePageId')]"
"""

SCRIPTS = """
"ContentItemId": "[js:uuid()]",
"Html": "[file:text('Snippets/welcome.html')]",
"Text": "[abc:uuid()]" // Not evaluated: stored as abc:uuid()
"""

HOST = """
"variables": {
  "adminUrlPrefix": "[js:configuration('Admin:AdminUrlPrefix', 'Admin')]"
},
// ...
"Author": "[js:parameters('AdminUsername')]"
"""

FILE_USE = """
"HtmlBodyPart": { "Html": "[file:text('Snippets/welcome.html')]" }
"""

WELCOME = """
<p>Fresh bread and pastries, baked every morning.</p>
"""

FEATURE = """
{
  "name": "feature",
  "enable": [
    "OrchardCore.Contents",
    "OrchardCore.Title",
    "OrchardCore.Html",
    "OrchardCore.HomeRoute",
    "TheBlogTheme"
    // ...
  ],
  "disable": []
}
"""

THEMES = """
{
  "name": "themes",
  "admin": "TheAdmin",
  "site": "TheBlogTheme"
}
"""

SETTINGS = """
{
  "name": "settings",
  "SiteName": "Greenfield Bakery",
  "HomeRoute": {
    "Action": "Display",
    "Controller": "Item",
    "Area": "OrchardCore.Contents",
    "ContentItemId": "[js:variables('homePageId')]"
  }
}
"""

DEFINITION_TYPE = """
{
  "name": "ContentDefinition",
  "ContentTypes": [
    {
      "Name": "BakeryPage",
      "DisplayName": "Bakery Page",
      "ContentTypePartDefinitionRecords": [
        { "PartName": "TitlePart", "Name": "TitlePart" },
        { "PartName": "HtmlBodyPart", "Name": "HtmlBodyPart" },
        { "PartName": "BakeryPage", "Name": "BakeryPage" }
      ]
    }
  ],
  // ...
}
"""

DEFINITION_PART = """
"ContentParts": [
  {
    "Name": "BakeryPage",
    "ContentPartFieldDefinitionRecords": [
      {
        "FieldName": "TextField",
        "Name": "OpeningHours",
        "Settings": {
          "ContentPartFieldSettings": { "DisplayName": "Opening Hours" }
        }
      }
    ]
  }
]
"""

CONTENT = """
{
  "name": "content",
  "data": [
    {
      "ContentItemId": "[js:variables('homePageId')]",
      "ContentType": "BakeryPage",
      "DisplayText": "Welcome to Greenfield Bakery",
      "Latest": true,
      "Published": true,
      // ...
    }
  ]
}
"""

CONTENT_PARTS = """
{
  // ...
  "Owner": "[js:parameters('AdminUserId')]",
  "Author": "[js:parameters('AdminUsername')]",
  "TitlePart": { "Title": "Welcome to Greenfield Bakery" },
  "HtmlBodyPart": { "Html": "[file:text('Snippets/welcome.html')]" },
  "BakeryPage": { "OpeningHours": { "Text": "Tuesday to Sunday, 7 am to 3 pm" } }
}
"""

RELOAD = """
"steps": [
  // ... the steps that write the settings
  { "name": "ReloadTenant" }
]
"""

RECIPES_STEP = """
{
  "name": "recipes",
  "Values": [
    { "name": "BakeryPages" }
  ]
}
"""

MIGRATION = """
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync("migration.recipe.json", this);
        return 1;
    }
}
"""

REGISTER = """
public override void ConfigureServices(IServiceCollection services)
    => services.AddDataMigration<Migrations>();
"""

WHAT_A_RECIPE_HOLDS = [
    ("1", "Properties", "The name, the display name, the description, the tags."),
    ("2", "Variables", "Values that you define once and use in any step."),
    ("3", "Steps", "Objects that Orchard Core runs in order, first to last."),
]

EVALUATION = [
    ("1", "First read", "The first step that reads a variable evaluates it."),
    ("2", "Kept", "Later reads return the same value."),
    ("3", "One identifier", "Two steps can point at the same content item."),
]

OTHER_STEPS = [
    ("1", "Roles", "Creates user roles and assigns their permissions."),
    ("2", "Templates", "Defines or updates Liquid templates."),
    ("3", "Layers", "Defines layer rules for conditional widget placement."),
]


def slide(step_id, chapter_title, label):
    # 1. What a recipe is.
    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Where a recipe lives")
        draw_code(image, (96, 240, 1000, 760), "A module project", "bash", TREE, types=TYPES, size=28)
        note_cards(image, [
            ("File name", "Ends in .recipe.json."),
            ("Folder", "Recipes, in a module or a theme."),
            ("Subfolders", "Hold files that steps read."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "What a recipe holds")
        bullets(image, WHAT_A_RECIPE_HOLDS)
        return image

    if step_id == "recipe-3":
        image = slide_base(chapter_title, label, "A step is an object with a name")
        draw_code(image, (96, 240, 1150, 560), "bakery-pages.recipe.json", "json", STEPS_SHAPE, size=26)
        note_cards(image, [
            ("name", "Picks the step."),
            ("Other properties", "Are the settings of that step."),
            ("Unknown name", "The step is skipped."),
        ], (1200, 240, W - 96, 1030))
        return image

    # 2. The recipe header.
    if step_id == "header-1":
        image = slide_base(chapter_title, label, "Name, display name and description")
        draw_code(image, (96, 240, W - 96, 740), "bakery-pages.recipe.json", "json", HEADER, highlight={2, 3, 4}, size=28)
        return image

    if step_id == "header-2":
        image = slide_base(chapter_title, label, "Tags and information")
        draw_code(image, (96, 240, W - 96, 740), "bakery-pages.recipe.json", "json", HEADER, highlight={5, 6, 7, 8}, size=28)
        return image

    if step_id == "header-3":
        image = slide_base(chapter_title, label, "Setup recipes and the Recipes page")
        note_cards(image, [
            ("issetuprecipe: true", "Offered on the setup screen. Not listed on Tools > Recipes."),
            ("issetuprecipe left out", "Listed on Tools > Recipes, to run on an existing site."),
            ("Tag: hidden", "Left out of Tools > Recipes."),
        ], (96, 260, W - 96, 1030))
        return image

    if step_id == "header-4":
        image = slide_base(chapter_title, label, "Comments and trailing commas")
        draw_code(image, (96, 240, W - 96, 620), "example.recipe.json", "json", COMMENTS, size=28)
        return image

    # 3. Variables and scripts.
    if step_id == "vars-1":
        image = slide_base(chapter_title, label, "Scripts")
        draw_code(image, (96, 240, 1150, 560), "Properties of a step", "json", SCRIPTS, size=26)
        note_cards(image, [
            ("[ ]", "Square brackets mark a script."),
            ("js:", "Runs JavaScript."),
            ("file:", "Reads a file."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "vars-2":
        image = slide_base(chapter_title, label, "Variables")
        draw_code(image, (96, 240, W - 96, 560), "bakery-pages.recipe.json", "json", VARIABLES, size=28)
        draw_code(image, (96, 620, W - 96, 800), "A step that reads a variable", "json", VARIABLE_USE, size=28)
        return image

    if step_id == "vars-3":
        image = slide_base(chapter_title, label, "Evaluated once, on first read")
        bullets(image, EVALUATION)
        return image

    if step_id == "vars-4":
        image = slide_base(chapter_title, label, "Configuration and parameters")
        draw_code(image, (96, 240, W - 96, 620), "example.recipe.json", "json", HOST, size=28)
        return image

    if step_id == "vars-5":
        image = slide_base(chapter_title, label, "Text from a file")
        draw_code(image, (96, 240, W - 96, 460), "bakery-pages.recipe.json", "json", FILE_USE, size=28)
        draw_code(image, (96, 520, W - 96, 740), "Snippets/welcome.html", "html", WELCOME, size=28)
        note_cards(image, [("Steps only", "A variable cannot read a file.")], (96, 800, W - 96, 1030))
        return image

    # 4. Features, themes and settings.
    if step_id == "steps-1":
        image = slide_base(chapter_title, label, "The feature step")
        draw_code(image, (96, 240, W - 96, 840), "bakery-pages.recipe.json", "json", FEATURE, size=28)
        return image

    if step_id == "steps-2":
        image = slide_base(chapter_title, label, "The themes step")
        draw_code(image, (96, 240, 1000, 620), "bakery-pages.recipe.json", "json", THEMES, size=28)
        note_cards(image, [
            ("Enable first", "A theme is a feature too."),
            ("Missing or empty", "Leaves that theme as it is."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "steps-3":
        image = slide_base(chapter_title, label, "The settings step")
        draw_code(image, (96, 240, 1000, 760), "bakery-pages.recipe.json", "json", SETTINGS, size=26)
        note_cards(image, [
            ("Core settings", "SiteName, TimeZoneId, HomeRoute, and more."),
            ("Any other property", "Is stored under its own name."),
        ], (1050, 240, W - 96, 1030))
        return image

    # 5. Content types and content.
    if step_id == "content-1":
        image = slide_base(chapter_title, label, "The ContentDefinition step: a type")
        draw_code(image, (96, 240, W - 96, 940), "bakery-pages.recipe.json", "json", DEFINITION_TYPE, size=28)
        return image

    if step_id == "content-2":
        image = slide_base(chapter_title, label, "The ContentDefinition step: a part")
        draw_code(image, (96, 240, W - 96, 900), "bakery-pages.recipe.json", "json", DEFINITION_PART, size=28)
        return image

    if step_id == "content-alter":
        image = slide_base(chapter_title, label, "A type or part that already exists")
        note_cards(image, [
            ("Altered", "The step adds the parts and fields you list."),
            ("Settings merged", "The settings of a part or a field merge with the existing ones."),
            ("Type settings", "Merge only when the type has a DisplayName."),
        ], (96, 260, W - 96, 1030))
        return image

    if step_id == "content-3":
        image = slide_base(chapter_title, label, "The content step")
        draw_code(image, (96, 240, W - 96, 860), "bakery-pages.recipe.json", "json", CONTENT, size=28)
        return image

    if step_id == "content-4":
        image = slide_base(chapter_title, label, "Data of each part")
        draw_code(image, (96, 240, W - 96, 700), "bakery-pages.recipe.json", "json", CONTENT_PARTS, size=26)
        return image

    # 7. More steps and reuse.
    if step_id == "more-1":
        image = slide_base(chapter_title, label, "More steps")
        bullets(image, OTHER_STEPS)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "The ReloadTenant step")
        draw_code(image, (96, 240, 1000, 600), "bakery-pages.recipe.json", "json", RELOAD, size=26)
        note_cards(image, [
            ("Reloads the tenant", "Settings read at startup take effect."),
            ("Place it after", "The steps that write those settings."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "reuse-1":
        image = slide_base(chapter_title, label, "The recipes step")
        draw_code(image, (96, 240, 1000, 640), "greenfield-site.recipe.json", "json", RECIPES_STEP, size=28)
        note_cards(image, [
            ("Runs by name", "The name property of a recipe."),
            ("Runs in place", "Right after the step, before the next."),
            ("Unknown name", "The step reports an error."),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "reuse-2":
        image = slide_base(chapter_title, label, "Recipes in data migrations")
        draw_code(image, (96, 236, W - 96, 876), "Migrations.cs", "csharp", MIGRATION, types=TYPES, size=22)
        draw_code(image, (96, 890, W - 96, 1030), "Startup.cs", "csharp", REGISTER, types={"IServiceCollection", "Migrations"}, size=22)
        return image

    if step_id == "reuse-3":
        image = slide_base(chapter_title, label, "When the items are imported")
        note_cards(image, [
            ("Recipe run by a data migration", "The import waits until all migrations have completed."),
            ("Recipe run from Tools, Recipes", "The items are imported immediately."),
        ], (96, 260, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
