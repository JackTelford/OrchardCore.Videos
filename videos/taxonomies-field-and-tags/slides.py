"""The slides of the Taxonomy Field video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

TAXONOMY = [
    ("1", "Taxonomy", "A managed vocabulary, such as Categories or Tags."),
    ("2", "Terms", "The entries of a taxonomy, organized as a hierarchy."),
    ("3", "Taxonomy Field", "Attached to a content type, it lets editors pick terms."),
]

EDITORS = [
    ("1", "Standard editor", "Every term is shown as a checkbox or a radio button."),
    ("2", "Tags editor", "A box where you type to search, and can create tags."),
]

SETTINGS = [
    ("1", "One taxonomy", "Each field draws its terms from the taxonomy you choose."),
    ("2", "Rules for the selection", "Required, Unique and Leaves only."),
    ("3", "A display mode", "Standard or Tags: how the chosen terms appear on the page."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A taxonomy and its terms")
        bullets(image, TAXONOMY)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Two editors for the same field")
        bullets(image, EDITORS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What you choose for each field")
        bullets(image, SETTINGS)
        return image

    raise SystemExit("No slide for " + step_id)
