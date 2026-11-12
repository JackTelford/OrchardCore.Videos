"""The slides of the Cultures video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import BORDER, GREEN, MARGIN, MUTED, PANEL, TEXT, W, bullets, font, slide_base

CULTURE = [
    ("1", "A language", "French, German or Spanish: each one is a culture."),
    ("2", "A language with a region", "French as used in Canada is a culture of its own."),
    ("3", "A parent culture", "French is the parent of French as used in Canada."),
    ("4", "One culture per request", "Each request to your site is served in one culture."),
]

LIST = [
    ("1", "Supported cultures", "The cultures your site can be served in."),
    ("2", "Default culture", "Used when nothing asks for another culture."),
    ("3", "Starting point", "A site starts with the culture of the system it runs on."),
]

ASKING = [
    ("1", "The language of the browser", "A browser set to French asks for French."),
    ("2", "The culture parameter", "Added to the address of a page, it asks for a culture."),
]

SEEING = [
    ("1", "One request", "The parameter applies to that request only."),
    ("2", "A sample French file", "It translates the labels of the Cultures page."),
    ("3", "Without the parameter", "The language of the browser decides again."),
]

BUILDS = [
    ("1", "Content Localization", "Manages localized versions of a content item."),
    ("2", "Content Culture Picker", "Offers the supported cultures on the front end."),
    ("3", "Data Localization", "Translates texts the site stores, per culture."),
    ("4", "Admin Culture Picker", "Offers the supported cultures in the admin."),
]

AROUND = [
    ("1", "Manage supported culture", "The Administrator role has this permission by default."),
    ("2", "Culture settings", "A deployment plan step that exports the culture settings."),
    ("3", "Recipes", "The settings step of a recipe can set the cultures."),
]


def address_bar(image):
    """An address with the culture parameter highlighted, and what it means."""
    draw = ImageDraw.Draw(image)
    top, bottom = 650, 760
    draw.rounded_rectangle((MARGIN, top, W - MARGIN, bottom), radius=16, fill=PANEL, outline=BORDER, width=2)
    page = "example.com/Admin/Settings/localization"
    parameter = "?culture=fr"
    x = MARGIN + 40
    middle = (top + bottom) // 2
    draw.text((x, middle), page, font=font(38), fill=TEXT, anchor="lm")
    x += int(font(38).getlength(page))
    draw.text((x, middle), parameter, font=font(38, "Bold"), fill=GREEN, anchor="lm")
    draw.text((MARGIN, 830), "fr is the name of the French culture.", font=font(30), fill=MUTED, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What a culture is")
        bullets(image, CULTURE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Supported and default")
        bullets(image, LIST)
        return image

    if step_id == "ask-1":
        image = slide_base(chapter_title, label, "Asking for a culture")
        bullets(image, ASKING)
        address_bar(image)
        return image

    if step_id == "ask-2":
        image = slide_base(chapter_title, label, "Seeing it on this site")
        bullets(image, SEEING)
        return image

    if step_id == "next-1":
        image = slide_base(chapter_title, label, "What builds on the list")
        bullets(image, BUILDS, active={0, 1})
        return image

    if step_id == "next-2":
        image = slide_base(chapter_title, label, "What builds on the list")
        bullets(image, BUILDS, active={2, 3})
        return image

    if step_id == "next-3":
        image = slide_base(chapter_title, label, "Around the Cultures page")
        bullets(image, AROUND)
        return image

    raise SystemExit(f"No slide for {step_id}")
