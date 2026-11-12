"""The slides of the Content Localization video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, MARGIN, MUTED, ON_ACCENT, PANEL, TEXT, W, bullets, font, slide_base

# The three items of the diagram: title, subtitle, culture, language and status.
ITEMS = [
    ("About", "This is what I do.", "en-US", "English (United States)", "Published"),
    ("À propos", "Voici ce que je fais.", "fr-FR", "French (France)", "Published"),
    ("Acerca de", "Esto es lo que hago.", "es-ES", "Spanish (Spain)", "Draft"),
]

PART = [
    ("1", "Culture", "The culture code of the item, such as fr-FR for French (France)."),
    ("2", "Localization set", "An identifier shared by every translation of the same item."),
]

FLOW = [
    ("1", "Create the item", "It gets the default culture of the site."),
    ("2", "Create a translation", "The module copies it into another supported culture, as a draft."),
    ("3", "Translate and publish", "Each translation is edited and published on its own."),
]


def fit(draw, text, weight, size, max_width, minimum=21):
    """The largest font, from size down to minimum, in which text fits max_width."""
    while size > minimum and draw.textlength(text, font=font(size, weight)) > max_width:
        size -= 1
    return font(size, weight)


def items_diagram(image):
    """Three content items, one for each culture, linked by the Localization part."""
    draw = ImageDraw.Draw(image)
    gap = 48
    width = (W - 2 * MARGIN - 2 * gap) // 3
    top, bottom = 280, 640
    centers = []
    for index, (title, subtitle, culture, language, status) in enumerate(ITEMS):
        left = MARGIN + index * (width + gap)
        right = left + width
        centers.append((left + right) // 2)
        draw.rounded_rectangle((left, top, right, bottom), radius=16, fill=PANEL, outline=ACCENT, width=2)
        inner = width - 80
        # The culture, as a green pill, and the language under it.
        pill = font(26, "Bold")
        pill_width = int(draw.textlength(culture, font=pill)) + 40
        draw.rounded_rectangle((left + 40, top + 34, left + 40 + pill_width, top + 80), radius=23, fill=ACCENT)
        draw.text((left + 40 + pill_width / 2, top + 57), culture, font=pill, fill=ON_ACCENT, anchor="mm")
        draw.text((left + 40, top + 118), language, font=fit(draw, language, "Regular", 28, inner), fill=MUTED, anchor="lm")
        # The content of the item.
        draw.text((left + 40, top + 190), title, font=fit(draw, title, "Bold", 48, inner), fill=TEXT, anchor="lm")
        draw.text((left + 40, top + 248), subtitle, font=fit(draw, subtitle, "Regular", 28, inner), fill=MUTED, anchor="lm")
        # Its own publication status.
        draw.text((left + 40, top + 316), status, font=fit(draw, status, "SemiBold", 28, inner), fill=ACCENT, anchor="lm")
    # The link between the items.
    bar = 700
    for x in centers:
        draw.line((x, bottom, x, bar), fill=ACCENT, width=4)
    draw.line((centers[0], bar, centers[-1], bar), fill=ACCENT, width=4)
    label = "Linked by the Localization part"
    label_font = font(30, "SemiBold")
    label_width = int(draw.textlength(label, font=label_font)) + 80
    middle = (centers[0] + centers[-1]) // 2
    draw.rounded_rectangle((middle - label_width // 2, bar - 36, middle + label_width // 2, bar + 36), radius=36, fill=PANEL, outline=ACCENT, width=3)
    draw.text((middle, bar), label, font=label_font, fill=TEXT, anchor="mm")
    note = "Each item has its own content and its own publication status."
    draw.text((W // 2, 850), note, font=fit(draw, note, "Regular", 32, W - 2 * MARGIN), fill=MUTED, anchor="mm")


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "One content item for each language")
        items_diagram(image)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "The Localization part stores two values")
        bullets(image, PART)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "From one item to many")
        bullets(image, FLOW)
        return image

    raise SystemExit("No slide for " + step_id)
