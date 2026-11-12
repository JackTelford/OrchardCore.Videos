"""The slides of the Rules video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOW = [
    ("1", "Widgets belong to a layer", "Each widget is assigned to one layer."),
    ("2", "A layer has one rule", "The rule decides when the layer is active."),
    ("3", "The site checks the rule", "Each time it displays a page of the site."),
]

LIST = [
    ("1", "A list of conditions", "A rule holds as many conditions as you add."),
    ("2", "Every condition must be true", "One false condition makes the whole rule false."),
    ("3", "No conditions, no widgets", "A rule without conditions is never true."),
]

KINDS = [
    ("1", "About the page", "Homepage, Url, Content type, Culture"),
    ("2", "About the visitor", "Is authenticated, Is anonymous, Role"),
    ("3", "Fixed or scripted", "Boolean, Javascript"),
    ("4", "Groups of conditions", "All, Any"),
]

OPERATIONS = [
    ("1", "Equals", "Does not equal"),
    ("2", "Starts with", "Does not start with"),
    ("3", "Ends with", "Does not end with"),
    ("4", "Contains", "Does not contain"),
]

KNOW = [
    ("1", "Edit, delete and reorder", "Edit and Delete on each row. Drag a row to reorder it, or into a group that has conditions."),
    ("2", "Content type tests pages, not widgets", "It tests the content that the page shows in full."),
    ("3", "Manage layers permission", "Administrators have it by default."),
    ("4", "No screen of its own", "Layers shows the rule editor, and enabling Layers enables Rules."),
]

# The card of KINDS and KNOW that each step talks about.
ACTIVE = {"kinds-1": 0, "kinds-2": 1, "kinds-3": 2, "kinds-4": 3,
          "know-1": 0, "know-2": 1, "know-3": 2, "know-4": 3}


def slide(step_id, chapter_title, label):
    if step_id == "rule-1":
        image = slide_base(chapter_title, label, "A layer shows its widgets when its rule is true")
        bullets(image, HOW)
        return image
    if step_id == "rule-2":
        image = slide_base(chapter_title, label, "A rule is a list of conditions")
        bullets(image, LIST)
        return image
    if step_id in ("kinds-1", "kinds-2", "kinds-3", "kinds-4"):
        image = slide_base(chapter_title, label, "The conditions of a rule")
        bullets(image, KINDS, active={ACTIVE[step_id]})
        return image
    if step_id == "kinds-5":
        image = slide_base(chapter_title, label, "Operations that compare text")
        bullets(image, OPERATIONS)
        return image
    if step_id in ("know-1", "know-2", "know-3", "know-4"):
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, KNOW, active={ACTIVE[step_id]})
        return image
    raise SystemExit(f"No slide for {step_id}")
