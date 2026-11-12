"""The slides of the Content Items video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "Content item", "One piece of content: an article, a page, a blog."),
    ("2", "Content type", "The definition that every item of that kind follows."),
    ("3", "Parts and fields", "What the editor shows: title, permalink, body, subtitle."),
    ("4", "Settings", "Creatable, Listable and Draftable decide what editors can do."),
]

LIFE_CYCLE = [
    ("1", "Draft", "Saved by an editor. Visitors do not see it."),
    ("2", "Published", "The version that visitors see on the site."),
    ("3", "Unpublished", "Taken off the site. It stays in the list as a draft."),
    ("4", "Deleted", "Removed from the list and from the site."),
]

PERMISSIONS = [
    ("1", "Edit content for others", "Shows Edit and Save Draft."),
    ("2", "Publish or unpublish content for others", "Shows Publish and Unpublish."),
    ("3", "Delete content for others", "Shows Delete."),
    ("4", "Own content permissions", "Edit own content, Publish or unpublish own content, Delete own content."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "Content items and content types")
        bullets(image, IDEA, active={0, 1} if step_id == "idea-1" else {2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "The life cycle of a content item")
        bullets(image, LIFE_CYCLE)
        return image

    if step_id in ("idea-4", "idea-5"):
        image = slide_base(chapter_title, label, "Permissions decide the buttons you see")
        bullets(image, PERMISSIONS, active={0, 1, 2} if step_id == "idea-4" else {3})
        return image

    raise SystemExit(f"No slide for {step_id}")
