"""The slides of the Content Permissions video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

LEVELS = [
    ("1", "Contents: every content type", "For example, Edit content for others."),
    ("2", "One content type: that type only", "For example, Edit Article for others. The type must be Securable."),
]

ACTIONS = [
    ("1", "View and preview", "Read the items of a type, and preview their drafts."),
    ("2", "Edit and clone", "Change an item, or make a copy of it."),
    ("3", "Publish or unpublish, and delete", "Put an item live, take it down, or remove it."),
    ("4", "List, and edit the owner", "Open the list of a type. Edit the owner of an item."),
]

OWNERS = [
    ("1", "Edit Article", "Applies to the articles you own."),
    ("2", "Edit Article for others", "Applies to the articles that belong to someone else."),
    ("3", "The owner of an item", "The user who created the content item."),
]

INCLUDES = [
    ("1", "All content includes each type", "Edit content for others includes Edit Article for others."),
    ("2", "Publish includes edit", "Publish or unpublish Article includes Edit Article."),
    ("3", "Edit for others includes more", "View, preview, and clone, for others and for your own."),
]

REMEMBER = [
    ("1", "Widen a role to every article", "Allow Edit Article for others."),
    ("2", "Widen it to every content type", "Allow Edit content for others, in the Contents group."),
    ("3", "Permissions only add access", "A user gets what any of their roles allows. Nothing takes it away."),
    ("4", "Visitors can read by default", "Anonymous holds View all content, which covers every type."),
]


def slide(step_id, chapter_title, label):
    if step_id == "concepts-1":
        image = slide_base(chapter_title, label, "Two levels of content permissions")
        bullets(image, LEVELS)
        return image

    if step_id == "concepts-2":
        image = slide_base(chapter_title, label, "What you can allow for a type")
        bullets(image, ACTIONS)
        return image

    if step_id == "concepts-3":
        image = slide_base(chapter_title, label, "Your own items, and other people's")
        bullets(image, OWNERS)
        return image

    if step_id == "concepts-4":
        image = slide_base(chapter_title, label, "A higher permission includes lower ones")
        bullets(image, INCLUDES)
        return image

    if step_id.startswith("recap-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "What to remember")
        bullets(image, REMEMBER, active={0, 1} if index == 1 else ({2} if index == 2 else {3}))
        return image

    raise SystemExit(f"No slide for {step_id}")
