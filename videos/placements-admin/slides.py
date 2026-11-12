"""The slides of the Placements module video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IPlacementStore", "PlacementsDocument", "Task", "IServiceCollection", "StartupBase",
         "FileContentDefinitionStartup", "DatabasePlacementsStore", "FilePlacementsStore", "Feature"}

PLACEMENT_FILE = """
{
  "MediaField_Edit": [
    {
      "place": "-",
      "differentiator": "BlogPost-Image"
    }
  ]
}
"""

PLACEMENT_NOTES = [
    ("Shape type", "Each property is one."),
    ("Rules", "Its value is an array."),
    ("Where", "In a module or a theme."),
]

WHY_MODULE = [
    ("check", "Edited in the admin", "Design, Placements, with a code editor for the rules."),
    ("check", "Saved with the site", "In the database by default, or in a file."),
    ("check", "The same format", "A shape type, and an array of rules."),
]

PRECEDENCE = [
    ("1", "Location", "A place in a rule of this module replaces the one from a theme or a module."),
    ("2", "Alternates", "The alternates of its rules are added to the existing ones."),
    ("3", "Wrappers", "The wrappers of its rules are added to the existing ones."),
]

STORE_INTERFACE = """
public interface IPlacementStore
{
    Task<PlacementsDocument> LoadPlacementsAsync();

    Task<PlacementsDocument> GetPlacementsAsync();

    Task SavePlacementsAsync(PlacementsDocument placementsDocument);
}
"""

STORE_DATABASE = """
public override void ConfigureServices(IServiceCollection services)
{
    // ...
    services.TryAddScoped<IPlacementStore, DatabasePlacementsStore>();
    // ...
}
"""

STORE_FILE = """
[Feature("OrchardCore.Placements.FileStorage")]
public class FileContentDefinitionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IPlacementStore>();
        services.AddScoped<IPlacementStore, FilePlacementsStore>();
    }
}
"""

STORE_DOCUMENT = """
{
  "Identifier": "...",
  "Placements": {
    "MediaField_Edit": [
      {
        "place": "-",
        "differentiator": "BlogPost-Image"
      }
    ]
  }
}
"""

STORE_PATH = """
App_Data/Sites/Default/PlacementsDocument.json
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Placements",
      "Placements": {
        "MediaField_Edit": [
          {
            "place": "-",
            "differentiator": "BlogPost-Image"
          }
        ]
      }
    }
  ]
}
"""

RECIPE_NOTES = [
    ("Step name", "Placements."),
    ("One property", "per shape type."),
    ("Executing it", "replaces those rules."),
]

MATCHING = [
    ("1", "Every matching rule applies", "In this module, from the first rule to the last."),
    ("2", "The last place wins", "A place in a later rule replaces the place of an earlier one."),
    ("3", "Alternates and wrappers add up", "Each matching rule contributes its own."),
]

DEPLOYMENT = [
    ("1", "Deployment module", "Enable it to add the All Placements step to a plan."),
    ("2", "All Placements step", "Adds all placements to the plan."),
    ("3", "Recipe format", "The export is a Placements step, as shown before."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A placement file belongs to the project")
        draw_code(image, (96, 240, 1010, 760), "placement.json", "json", PLACEMENT_FILE, types=TYPES, size=28)
        note_cards(image, PLACEMENT_NOTES, (1070, 300, W - 96, 1030))
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Placements module")
        bullets(image, WHY_MODULE)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The admin rule takes precedence")
        bullets(image, PRECEDENCE)
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "Several rules for one shape type")
        bullets(image, MATCHING)
        return image

    if step_id == "store-1":
        image = slide_base(chapter_title, label, "Placements are stored by a service")
        draw_code(image, (96, 240, W - 96, 650), "IPlacementStore.cs", "csharp", STORE_INTERFACE, types=TYPES, size=24)
        draw_code(image, (96, 690, W - 96, 1010), "Startup.cs", "csharp", STORE_DATABASE, types=TYPES, size=24)
        return image

    if step_id == "store-2":
        image = slide_base(chapter_title, label, "Placements file storage")
        draw_code(image, (96, 240, 1040, 700), "Startup.cs", "csharp", STORE_FILE, types=TYPES, size=22)
        draw_code(image, (1080, 240, W - 96, 700), "PlacementsDocument.json", "json", STORE_DOCUMENT, types=TYPES, size=22)
        draw_code(image, (96, 760, W - 96, 900), "The file of the Default tenant", "bash", STORE_PATH, types=TYPES, size=28)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The Placements recipe step")
        draw_code(image, (96, 240, 1100, 860), "recipe.json", "json", RECIPE, types=TYPES, size=24)
        note_cards(image, RECIPE_NOTES, (1160, 300, W - 96, 1030))
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Export with a deployment plan")
        bullets(image, DEPLOYMENT)
        return image

    raise SystemExit(f"No slide for {step_id}")
