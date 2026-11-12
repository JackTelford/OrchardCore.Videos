"""The slides of the "Lists: Ordering, Paging and Contained Types" video. assemble.py calls slide() for each step of
a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT = [
    ("1", "The list", "A content item with the List part, such as a Blog."),
    ("2", "The contained items", "Content items of the types you choose, such as Blog Posts."),
    ("3", "The content type", "It holds the List part settings that every list of that type shares."),
]

SETTINGS = [
    ("1", "Contained Content Types", "The content types the list accepts."),
    ("2", "Page Size", "The number of items on each page. It is 10 by default."),
    ("3", "Enable Ordering", "Arrange the items by dragging them."),
    ("4", "Display options", "Show full pager and Show container's header."),
]


def slide(step_id, chapter_title, label):
    if step_id == "settings-1":
        image = slide_base(chapter_title, label, "A list and the items it holds")
        bullets(image, WHAT)
        return image
    if step_id == "settings-2":
        image = slide_base(chapter_title, label, "The settings of the List part")
        bullets(image, SETTINGS, active={0, 1})
        return image
    if step_id == "settings-3":
        image = slide_base(chapter_title, label, "The settings of the List part")
        bullets(image, SETTINGS, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
