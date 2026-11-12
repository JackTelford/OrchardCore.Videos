"""The slides of the Admin Culture Picker and Content-Language Header video. assemble.py calls slide() for each step
of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

CULTURE = [
    ("1", "A language and its region", "For example English for the United States."),
    ("2", "Listed in the settings", "The site supports the cultures that its settings list."),
]

PICKER = [
    ("1", "A drop-down in the top bar", "It lists the supported cultures of the site."),
    ("2", "The admin follows your choice", "Labels that have a translation, and the text direction."),
    ("3", "Admin pages only", "The front end keeps the culture the site gives it."),
]

HEADER_FEATURE = [
    ("1", "No screen, no settings", "Enabling the feature is all it takes."),
    ("2", "A Content-Language header", "It names the language of the responses of the site."),
]

NEEDS = [
    ("1", "The Localization feature", "The Features page enables it together with the picker."),
    ("2", "Two supported cultures", "With a single culture, the drop-down does not appear."),
    ("3", "Translations in PO files", "Labels change where the site has a translation."),
]

HEADER_VALUE = [
    ("1", "Added by the feature", "The Content Language Header feature turns it on."),
    ("2", "Describes the audience", "The language intended for the audience of the response."),
    ("3", "Carries a culture", "The culture the site uses to render the response."),
]

HEADER_EXAMPLES = [
    ("1", "A page in English", "The header names the English culture of the site."),
    ("2", "An admin page in français", "The header names the French culture chosen with the picker."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What is a culture?")
        bullets(image, CULTURE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Admin Culture Picker")
        bullets(image, PICKER, active={0, 1})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The Content Language Header")
        bullets(image, HEADER_FEATURE)
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "What the picker needs")
        bullets(image, NEEDS)
        return image

    if step_id == "header-1":
        image = slide_base(chapter_title, label, "The Content-Language header")
        bullets(image, HEADER_VALUE, active={1, 2})
        return image

    if step_id == "header-2":
        image = slide_base(chapter_title, label, "What the header carries")
        bullets(image, HEADER_EXAMPLES)
        return image

    raise SystemExit(f"No slide for {step_id}")
