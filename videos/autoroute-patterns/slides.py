"""The slides of the Autoroute patterns video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"DataMigration", "IContentDefinitionManager", "AutoroutePartSettings", "Task"}

IDEA = [
    ("1", "A Liquid pattern per content type", "A setting of the Autoroute part of the type."),
    ("2", "Rendered while the path is empty", "On create and update. A path that is already set is kept."),
    ("3", "Unique", "A path that is taken gets a dash and a number."),
    ("4", "Valid characters", "A permalink has no spaces, double slashes, or characters such as ? : # and a dot."),
]

# Liquid has no lexer in brand.py: the html lexer shows it as plain text.
PATTERNS = """
{{ ContentItem | display_text | slugify }}
{{ Model.ContentItem | display_text | slugify }}

{{ ContentItem | container | display_text | slugify }}/{{ ContentItem | display_text | slugify }}

{{ ContentItem.Content.Product.Color.Text | slugify }}
"""

MIGRATION = """
using OrchardCore.Autoroute.Models;
// ...

public sealed class Migrations : DataMigration
{
    // ... _contentDefinitionManager is an IContentDefinitionManager.

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            .WithPart("TitlePart", part => part.WithPosition("1"))
            .WithPart("AutoroutePart", part => part
                .WithPosition("2")
                .WithSettings(new AutoroutePartSettings
                {
                    Pattern = "{{ ContentItem | display_text | slugify }}",
                })));

        return 1;
    }
}
"""

RECIPE = """
{
  "name": "ContentDefinition",
  "ContentTypes": [
    {
      "Name": "Article",
      // ...
      "ContentTypePartDefinitionRecords": [
        {
          "PartName": "AutoroutePart",
          "Name": "AutoroutePart",
          "Settings": {
            "AutoroutePartSettings": {
              "AllowCustomPath": true,
              "Pattern": "{{ Model.ContentItem | display_text | slugify }}"
            }
          }
        }
      ]
    }
  ]
}
"""

LIQUID_LOOKUP = """
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}

{% assign my_content = Content.Slug["my-blog/my-blog-post"] %}
"""

RAZOR_LOOKUP = """
// In a Razor view, Orchard is an IOrchardHelper.
var published = await Orchard.GetContentItemBySlugAsync("my-blog/my-blog-post");
var mostRecent = await Orchard.GetContentItemBySlugAsync("my-blog/my-blog-post", true);
"""


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2", "idea-3"):
        image = slide_base(chapter_title, label, "From a Liquid expression to a path")
        active = {"idea-1": {0}, "idea-2": {1}, "idea-3": {2, 3}}[step_id]
        bullets(image, IDEA, active=active)
        return image

    if step_id in ("use-1", "use-2", "use-3"):
        image = slide_base(chapter_title, label, "Three patterns")
        highlight = {"use-1": {1, 2}, "use-2": {4}, "use-3": {6}}[step_id]
        draw_code(image, (96, 240, W - 96, 590), "Pattern", "html", PATTERNS, highlight=highlight, size=26)
        notes = {
            "use-1": [
                ("display_text", "A filter of the Contents module: the display text of an item."),
                ("slugify", "A filter of the Liquid module: lowercase, with dashes for spaces and most punctuation."),
            ],
            "use-2": [
                ("container", "A filter of the Lists module: the list that holds the item."),
                ("Outside a list", "The filter returns the item itself."),
            ],
            "use-3": [
                ("Content", "The parts of the item, then their fields, then their values."),
                ("Product, Color, Text", "A Text field named Color on the Product type."),
            ],
        }[step_id]
        note_cards(image, notes, (96, 620, W - 96, 1040))
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "A pattern in a migration")
        draw_code(image, (96, 240, W - 96, 1050), "Migrations.cs", "csharp", MIGRATION, types=TYPES,
                  highlight={10, 12, 13, 14, 15, 16, 17}, size=24)
        return image

    if step_id == "code-2":
        image = slide_base(chapter_title, label, "A pattern in a recipe")
        draw_code(image, (96, 240, W - 96, 1050), "recipe.json", "json", RECIPE, size=24)
        return image

    if step_id == "lookup-1":
        image = slide_base(chapter_title, label, "Load content by its path in Liquid")
        draw_code(image, (96, 240, W - 96, 560), "Liquid", "html", LIQUID_LOOKUP, size=28)
        note_cards(image, [
            ("Content[...]", "The handle is slug:, then the path."),
            ("Content.Slug[...]", "Takes the path alone."),
        ], (96, 620, W - 96, 1040))
        return image

    if step_id == "lookup-2":
        image = slide_base(chapter_title, label, "Load content by its path in Razor")
        draw_code(image, (96, 240, W - 96, 560), "MyView.cshtml", "csharp", RAZOR_LOOKUP, size=28)
        note_cards(image, [
            ("GetContentItemBySlugAsync", "A method of IOrchardHelper, from the Autoroute module."),
            ("Second argument", "false loads the published version, true the most recent one. The default is false."),
        ], (96, 620, W - 96, 1040))
        return image

    raise SystemExit(f"No slide for {step_id}")
