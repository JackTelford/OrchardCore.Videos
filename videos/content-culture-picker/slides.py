"""The slides of the Content Culture Picker video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

MENU = [
    ("1", "A menu in the navigation bar", "It appears on the site, in a theme that shows the navigation bar."),
    ("2", "The button shows the current language", "Each language is written in its own language."),
    ("3", "The menu lists the other languages", "Choosing one opens the page in that language."),
    ("4", "A language is a culture", "Orchard Core names the languages of a site cultures."),
]

RELIES_ON = [
    ("1", "Two or more supported cultures", "With a single culture, the menu does not appear."),
    ("2", "Translated pages", "The versions of a page are linked by the Localization part."),
    ("3", "A path for each page", "Autoroute gives a page the path that the picker looks up."),
    ("4", "A theme with the navigation bar", "The Default Theme shows it."),
]

LANDS = [
    ("1", "The same page in that language", "When the page has a version in the chosen culture."),
    ("2", "The homepage in that language", "When Homepage redirect is on, and the page has no version."),
    ("3", "The page the visitor is on", "When neither exists, the visitor stays where they are."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "The language menu")
        bullets(image, MENU)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What the picker relies on")
        bullets(image, RELIES_ON)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Where the visitor lands")
        bullets(image, LANDS)
        return image

    raise SystemExit(f"No slide for {step_id}")
