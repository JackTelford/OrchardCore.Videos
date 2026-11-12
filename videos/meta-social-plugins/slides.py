"""The slides of the Meta social plugins video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, MARGIN, MUTED, PANEL, PANEL_DIM, TEXT, W, bullets, font, slide_base

IDEA = [
    ("1", "Social plugin", "A Like button, a Share button, a comments box, and more."),
    ("2", "Widget", "The feature provides a widget for seven of the plugins."),
    ("3", "Zone and layer", "The zone sets the place. The layer sets the pages."),
]

WIDGETS = [
    "Facebook Chat",
    "Facebook Comments",
    "Facebook Continue With",
    "Facebook Like",
    "Facebook Quote",
    "Facebook Save",
    "Facebook Share",
]

# The widgets this video adds.
SHOWN = {"Facebook Comments", "Facebook Like", "Facebook Share"}

RIGHTS = [
    ("1", "Body field", "Holds the template of the plugin."),
    ("2", "Manage Liquid templates", "Needed to edit or publish these widgets. It is security critical."),
    ("3", "Default roles", "Administrator and Editor have the permission."),
    ("4", "Without the permission", "The widgets can't be edited or published, whatever else the role can edit."),
]


def widget_names(image):
    """The names of the widgets in a grid of three columns; the ones the video adds are bright."""
    draw = ImageDraw.Draw(image)
    gap = 36
    width = (W - 2 * MARGIN - 2 * gap) // 3
    for index, name in enumerate(WIDGETS):
        row, column = divmod(index, 3)
        x = MARGIN + column * (width + gap)
        y = 250 + row * 150
        on = name in SHOWN
        draw.rounded_rectangle((x, y, x + width, y + 110), radius=16, fill=PANEL if on else PANEL_DIM,
                               outline=ACCENT if on else BORDER, width=2)
        draw.text((x + width // 2, y + 55), name, font=font(36, "Bold" if on else "SemiBold"),
                  fill=TEXT if on else MUTED, anchor="mm")
    draw.text((MARGIN, 740), "Bright: the three widgets of this video.", font=font(30), fill=MUTED, anchor="lm")
    draw.text((MARGIN, 800), "The feature and its settings say Meta. The widgets say Facebook.", font=font(30),
              fill=MUTED, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Social plugins as widgets")
        bullets(image, IDEA)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Seven widgets")
        widget_names(image)
        return image

    if step_id == "rights-1":
        image = slide_base(chapter_title, label, "Editing the Body field")
        bullets(image, RIGHTS, active={0, 1, 2})
        return image

    if step_id == "rights-2":
        image = slide_base(chapter_title, label, "Editing the Body field")
        bullets(image, RIGHTS, active={3})
        return image

    raise SystemExit(f"No slide for {step_id}")
