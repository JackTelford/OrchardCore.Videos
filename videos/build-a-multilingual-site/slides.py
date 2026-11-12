"""The slides of the "Build a Multilingual Site" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PLAN = [
    ("check", "Two cultures", "English is the default culture, and French is the second one."),
    ("check", "One translated page", "The About page gets a French copy, titled À propos."),
    ("check", "A language menu", "Visitors switch between the two from the navigation bar."),
]

FEATURES = [
    ("1", "Localization", "Defines the cultures the site supports, and the default one."),
    ("2", "Content Localization", "Gives a content item one translation for each culture."),
    ("3", "Content Culture Picker", "Provides a language menu for the navigation bar of the site."),
]

RULES = [
    ("1", "The page has a translation", "The visitor goes to the translation in the chosen language."),
    ("2", "No translation, Homepage redirect on", "The picker tries the home page in the chosen language."),
    ("3", "Anything else", "The visitor stays on the current page."),
]


def slide(step_id, chapter_title, label):
    if step_id == "plan-1":
        image = slide_base(chapter_title, label, "A site in two languages")
        bullets(image, PLAN)
        return image

    if step_id.startswith("idea-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "Three features, three jobs")
        bullets(image, FEATURES, active={index})
        return image

    if step_id.startswith("rules-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "Three rules for the picker")
        bullets(image, RULES, active={index})
        return image

    raise SystemExit(f"No slide for {step_id}")
