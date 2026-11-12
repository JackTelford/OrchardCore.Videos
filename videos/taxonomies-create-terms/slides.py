"""The slides of the "Create a Taxonomy and Organize Its Terms" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, MARGIN, MUTED, ON_ACCENT, PANEL, TEXT, W, bullets, font, slide_base

CONCEPT = [
    ("1", "Taxonomy", "A content item that holds a list of terms, such as Topics."),
    ("2", "Terms", "The entries of the list. A term can contain other terms."),
    ("3", "Term content type", "Every term is a content item of one content type that you choose."),
]

STORAGE = [
    ("1", "Terms live inside the taxonomy", "They are not listed as content items of their own."),
    ("2", "Changes go to a draft", "Adding, editing and deleting terms changes a draft of the taxonomy."),
    ("3", "Publishing applies them", "The published taxonomy changes only when you publish it."),
]

BEFORE = [
    ("1", "The Taxonomies feature", "Enable Taxonomies in Tools, Features."),
    ("2", "The Manage taxonomies permission", "It allows adding, editing and deleting terms. Administrators have it."),
]

CONTENT_TYPE = [
    ("1", "Any content type without a stereotype", "Special-purpose types, such as widgets, have one and are left out of the list."),
    ("2", "Topic", "The content type of the terms in this video. It has only a title."),
]

# The taxonomy built in the video: (term, [terms inside it]).
TREE = [
    ("Gardening", ["Vegetables", "Herbs"]),
    ("Cooking", ["Baking", "Barbecue"]),
]

BOX_W, BOX_H = 320, 96
COLUMN_STEP = 440
LEAF_GAP = 34
GROUP_GAP = 90


def tree_slide(chapter_title, label):
    image = slide_base(chapter_title, label, "The Topics taxonomy")
    draw = ImageDraw.Draw(image)

    columns = [MARGIN + 40, MARGIN + 40 + COLUMN_STEP, MARGIN + 40 + 2 * COLUMN_STEP]
    captions = ["Taxonomy", "Terms", "Terms inside a term"]
    for x, caption in zip(columns, captions):
        draw.text((x + BOX_W // 2, 270), caption, font=font(28, "SemiBold"), fill=MUTED, anchor="mm")

    def box(x, top, text, fill, outline, color):
        draw.rounded_rectangle((x, top, x + BOX_W, top + BOX_H), radius=16, fill=fill, outline=outline, width=2)
        draw.text((x + BOX_W // 2, top + BOX_H // 2), text, font=font(36, "Bold"), fill=color, anchor="mm")

    def line(points):
        draw.line(points, fill=ACCENT, width=4, joint="curve")

    y = 320
    parents = []
    for name, children in TREE:
        centers = []
        for child in children:
            box(columns[2], y, child, PANEL, ACCENT, TEXT)
            centers.append(y + BOX_H // 2)
            y += BOX_H + LEAF_GAP
        y += GROUP_GAP - LEAF_GAP
        parent_center = (centers[0] + centers[-1]) // 2
        parents.append(parent_center)
        box(columns[1], parent_center - BOX_H // 2, name, PANEL, ACCENT, TEXT)
        bar_x = columns[1] + BOX_W + 60
        line([(columns[1] + BOX_W, parent_center), (bar_x, parent_center)])
        line([(bar_x, centers[0]), (bar_x, centers[-1])])
        for center in centers:
            line([(bar_x, center), (columns[2], center)])

    root_center = (parents[0] + parents[-1]) // 2
    box(columns[0], root_center - BOX_H // 2, "Topics", ACCENT, ACCENT, ON_ACCENT)
    bar_x = columns[0] + BOX_W + 60
    line([(columns[0] + BOX_W, root_center), (bar_x, root_center)])
    line([(bar_x, parents[0]), (bar_x, parents[-1])])
    for center in parents:
        line([(bar_x, center), (columns[1], center)])

    # A note on the right, in the space the tree leaves free.
    note_x = columns[2] + BOX_W + 80
    draw.rounded_rectangle((note_x, 320, W - MARGIN, 560), radius=16, fill=PANEL, outline=BORDER, width=2)
    draw.text((note_x + 32, 380), "Every term is a Topic:", font=font(30, "SemiBold"), fill=TEXT, anchor="lm")
    draw.text((note_x + 32, 440), "a content item with", font=font(28), fill=MUTED, anchor="lm")
    draw.text((note_x + 32, 486), "only a title.", font=font(28), fill=MUTED, anchor="lm")
    return image


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A taxonomy is a tree of terms")
        bullets(image, CONCEPT)
        return image

    if step_id == "why-2":
        return tree_slide(chapter_title, label)

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Where the terms are kept")
        bullets(image, STORAGE)
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "Before you start")
        bullets(image, BEFORE)
        return image

    if step_id == "why-5":
        image = slide_base(chapter_title, label, "A content type for the terms")
        bullets(image, CONTENT_TYPE)
        return image

    raise SystemExit(f"No slide for {step_id}")
