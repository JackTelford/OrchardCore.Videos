"""The slides of the Build a Company Website With the Agency Theme video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

RECIPE = [
    ("1", "Choose the recipe", "On the Setup screen, open Recipe and choose Agency."),
    ("2", "Finish Setup", "The site uses the Agency theme, and the landing page is its home page."),
    ("3", "A main menu is included", "The Main Menu links to the Services, Portfolio, About, Team and Contact sections."),
]

STRUCTURE = [
    ("1", "One landing page", "One content item holds the items of its Bag parts."),
    ("2", "Bag parts from the Flows module", "Services, Portfolio, About, Team and Clients are Bag parts."),
    ("3", "One type per Bag part", "In this recipe, each Bag part holds items of a single type."),
]

PARTS = [
    ("1", "Services", "Service items: a title, a description and an icon."),
    ("2", "Portfolio", "Project items: a title, a description, a category and an image."),
    ("3", "About", "Milestone items: a date, a title, a description and an image."),
    ("4", "Team and Clients", "Team Member items with a picture, Client items with a logo."),
]

PLAN = [
    ("1", "Edit the landing page", "Add and change the items inside its Bag parts."),
    ("2", "Add a contact form", "Enable the Forms module and build the form on its own page."),
    ("3", "Extend the menu", "Add a menu item that links to the form page."),
]

RECAP = [
    ("check", "Sections", "Bag parts hold the items of Services, Portfolio, About, Team and Clients."),
    ("check", "Contact form", "A Form widget on a page holds the Input, Text Area and Button widgets."),
    ("check", "Menu", "A Content Menu Item links the Main Menu to the form page."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Ready to use company website")
        bullets(image, RECIPE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A landing page with Bag parts")
        bullets(image, STRUCTURE)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What each Bag part holds")
        bullets(image, PARTS)
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "Three changes to the site")
        bullets(image, PLAN)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "Three pieces, each changed on its own")
        bullets(image, RECAP)
        return image

    raise SystemExit(f"No slide for {step_id}")
