"""The slides of the content items list video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

ROW = [
    ("1", "Title", "The display text of the item, the name you see in the list."),
    ("2", "Type", "The content type of the item, such as Article, Page or Blog."),
    ("3", "Status", "Published, Draft, or both when a published item has unpublished changes."),
    ("4", "Last modified", "The time of the last modification, and the author."),
]

CONTROLS = [
    ("1", "Search", "Words in the title of an item, with AND, NOT, parentheses and quotes."),
    ("2", "Content Type", "Keeps the items of one content type."),
    ("3", "Show", "Keeps the items with one status, such as Published or Unpublished."),
    ("4", "Sort", "Orders by modification, creation or publication date, or by Title."),
]

ACTIONS = [
    ("1", "Publish Now", "Publishes every selected item."),
    ("2", "Unpublish", "Takes every selected item off the public site."),
    ("3", "Delete", "Removes every selected item from the list."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "What each row tells you")
        bullets(image, ROW)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Four ways to narrow and order the list")
        bullets(image, CONTROLS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Commands for many items")
        bullets(image, ACTIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")
