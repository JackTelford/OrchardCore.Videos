"""The slides of the Orchard Core Recipes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

STEPS = [
    ("1", "Enable features", "Turns on the modules and themes that the site needs."),
    ("2", "Set the themes", "Chooses the theme of the admin and the theme of the site."),
    ("3", "Create roles", "Adds roles, such as Editor and Author."),
    ("4", "Define and add content", "Creates content types and the content items that use them."),
]

FILE = [
    ("1", "A file you keep", "Store it, share it, and run it on another site."),
    ("2", "The same steps each time", "Sites that use the same recipe start from the same configuration."),
    ("3", "Fewer clicks", "One run replaces many changes made by hand in the admin."),
]

IDENTITY = [
    ("1", "Display name", "The name shown in the lists of recipes."),
    ("2", "Description", "A sentence about what the recipe does."),
    ("3", "Tags", "Keywords, shown as badges on the Recipes page."),
]

PLACES = [
    ("1", "Setup screen", "Runs the recipe you choose when it creates the site."),
    ("2", "Tools, Recipes", "Runs a recipe that a module or a theme provides."),
    ("3", "Tools, Deployments", "Package Import and JSON Import run a recipe that you supply."),
]

FAILURE = [
    ("1", "The recipe stops", "The steps after the failing step do not run."),
    ("2", "An error message", "It names the recipe that failed."),
    ("3", "Earlier steps have run", "The steps before the failing step have already run."),
]

AGAIN = [
    ("1", "Every step runs again", "A recipe runs all of its steps each time."),
    ("2", "Content can be added again", "The widget sample adds a second widget."),
    ("3", "Run it again on purpose", "Do it only when you want the steps repeated."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "A recipe is a list of steps")
        bullets(image, STEPS)
        return image
    if step_id == "what-2":
        image = slide_base(chapter_title, label, "Why a recipe is a file")
        bullets(image, FILE)
        return image
    if step_id == "what-3":
        image = slide_base(chapter_title, label, "What you see of a recipe")
        bullets(image, IDENTITY)
        return image
    if step_id == "what-4":
        image = slide_base(chapter_title, label, "Places where recipes run")
        bullets(image, PLACES)
        return image
    if step_id == "care-1":
        image = slide_base(chapter_title, label, "When a step fails")
        bullets(image, FAILURE)
        return image
    if step_id == "care-2":
        image = slide_base(chapter_title, label, "Running a recipe again")
        bullets(image, AGAIN)
        return image
    raise SystemExit("No slide for " + step_id)
