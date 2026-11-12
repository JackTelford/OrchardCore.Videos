"""The slides of the Lists module video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "Content that belongs together", "A blog and its posts. A help center and its pages."),
    ("2", "A list is a content item", "It holds other content items. Both are edited like any other content."),
    ("3", "The List part", "A content type becomes a list when it has the List part."),
    ("4", "One list per item", "Each item remembers the list it was created in."),
]

WHERE = [
    ("check", "In the admin", "A page for the list, with a button to create its items."),
    ("check", "On the site", "Visitors see the published items of the list."),
    ("check", "The Lists feature", "You enable it in Tools, Features."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "A list is a container item")
        bullets(image, IDEA, active={0, 1} if step_id == "idea-1" else {2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Where you meet a list")
        bullets(image, WHERE)
        return image

    raise SystemExit(f"No slide for {step_id}")
