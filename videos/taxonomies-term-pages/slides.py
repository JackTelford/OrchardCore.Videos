"""The slides of the "Taxonomies: Term Pages" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "One page for each term", "Visitors browse the content of a site by category or by tag."),
    ("2", "An address of its own: /categories/travel", "The permalink of the taxonomy, then the permalink of the term."),
    ("3", "A list of its content", "The items that have the term selected in a taxonomy field."),
]

NEEDS = [
    ("1", "Route contained items", "Checked on the taxonomy, with the Autoroute feature enabled, so each term gets an address."),
    ("2", "A published taxonomy", "A term has a page once the published taxonomy contains it."),
    ("3", "Manage contained item routes", "Checked on the type of the terms, so each term uses its own Permalink."),
    ("4", "A taxonomy field", "The items that have the term selected are listed on its page."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2", "idea-3"):
        image = slide_base(chapter_title, label, "A page for each term")
        bullets(image, IDEA, active={int(step_id[-1]) - 1})
        return image
    if step_id in ("needs-1", "needs-2", "needs-3", "needs-4"):
        image = slide_base(chapter_title, label, "Four things to check")
        bullets(image, NEEDS, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
