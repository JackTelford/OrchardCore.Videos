"""The slides of the Taxonomies overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, MUTED, PANEL, TEXT, W, bullets, font, slide_base

PROBLEMS = [
    ("1", "Typed by hand", "Each editor writes the category in their own way."),
    ("2", "Named differently", "Travel, Travels and Trips become three categories."),
    ("3", "Hard to browse", "Visitors can't follow one topic across the site."),
]

BUILDING_BLOCKS = [
    ("1", "Taxonomy", "A managed list of terms, organized as a hierarchy."),
    ("2", "Term", "One entry of the list, such as Travel or Recipes."),
    ("3", "Taxonomy Field", "Attaches terms to content items, and lets editors pick them."),
]

MORE = [
    ("1", "Taxonomy Field settings", "Required, Unique and Leaves only. The Tags editor adds Open."),
    ("2", "Taxonomy Filters", "Settings, Taxonomy Filters picks the filters of the Content Items list."),
    ("3", "Manage taxonomies", "The permission to add, edit and delete terms. Administrator has it."),
]


def term_box(draw, box, title, caption=None):
    """A rounded box with a term (or taxonomy) name, and an optional caption under it."""
    x0, y0, x1, y1 = box
    draw.rounded_rectangle(box, radius=16, fill=PANEL, outline=ACCENT, width=2)
    middle = (y0 + y1) // 2
    if caption:
        draw.text((x0 + 32, middle - 22), title, font=font(42, "Bold"), fill=TEXT, anchor="lm")
        draw.text((x0 + 32, middle + 30), caption, font=font(28), fill=MUTED, anchor="lm")
    else:
        draw.text((x0 + 32, middle), title, font=font(42, "Bold"), fill=TEXT, anchor="lm")


def connect(draw, start, ends, middle_x):
    """Elbow lines from the right edge of a box to the left edge of each box in ends."""
    sx, sy = start
    draw.line((sx, sy, middle_x, sy), fill=ACCENT, width=4)
    ys = [y for _, y in ends]
    draw.line((middle_x, min(ys + [sy]), middle_x, max(ys + [sy])), fill=ACCENT, width=4)
    for ex, ey in ends:
        draw.line((middle_x, ey, ex, ey), fill=ACCENT, width=4)


def hierarchy(image):
    """A taxonomy with its terms: Categories, Travel, and Food with two child terms."""
    draw = ImageDraw.Draw(image)
    for x, label in ((96, "Taxonomy"), (736, "Terms"), (1336, "Child terms")):
        draw.text((x, 290), label.upper(), font=font(28, "Bold"), fill=MUTED, anchor="lm")
        draw.line((x, 320, x + 400, 320), fill=BORDER, width=2)

    root = (96, 560, 596, 700)
    travel = (736, 400, 1236, 510)
    food = (736, 750, 1236, 860)
    recipes = (1336, 640, 1736, 750)
    restaurants = (1336, 860, 1736, 970)

    term_box(draw, root, "Categories", "A taxonomy")
    term_box(draw, travel, "Travel")
    term_box(draw, food, "Food")
    term_box(draw, recipes, "Recipes")
    term_box(draw, restaurants, "Restaurants")

    connect(draw, (root[2], 630), [(travel[0], 455), (food[0], 805)], 666)
    connect(draw, (food[2], 805), [(recipes[0], 695), (restaurants[0], 915)], 1286)


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without a shared list")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A taxonomy is a managed vocabulary")
        hierarchy(image)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Three building blocks")
        bullets(image, BUILDING_BLOCKS, active={2})
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "More to explore")
        bullets(image, MORE, active={0})
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "More to explore")
        bullets(image, MORE, active={1})
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "More to explore")
        bullets(image, MORE, active={2})
        return image

    raise SystemExit("No slide for " + step_id)
