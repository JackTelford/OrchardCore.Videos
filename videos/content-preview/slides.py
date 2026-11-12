"""The slides of the Content Preview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

CHECK_FIRST = [
    ("1", "The editor shows fields", "Title, Subtitle and body: not the finished page."),
    ("2", "Content Preview shows the page", "The item rendered as a page, before it is published."),
]

TWO_WAYS = [
    ("1", "Preview", "In the editor: the values you are typing, even before you save."),
    ("2", "Preview Draft", "In the content items list: the draft you saved. It works without the feature."),
]

PERMISSIONS = [
    ("1", "Preview content", "The permission that allows previewing."),
    ("2", "Edit content for others", "Includes Preview content."),
    ("3", "Publish or unpublish content for others", "Includes Preview content."),
    ("4", "Access Control, Roles", "Assign Preview content to other roles."),
]

LIMITS = [
    ("1", "Same browser, same address", "The editor and the preview window stay on one site."),
    ("2", "Keep the editor open", "The preview stops updating when the editor closes."),
    ("3", "Do not share preview links", "A link identifies temporary content that may be unpublished."),
]

ADD_THE_PART = [
    ("1", "Edit the content type", "Design, Content Definition, Content Types."),
    ("2", "Add Parts", "Choose Preview in the list of parts."),
]

HOW_THE_PART_WORKS = [
    ("1", "Preview part", "Added to the content type."),
    ("2", "Pattern", "An expression that returns the path of the page that displays the item."),
    ("3", "Preview button", "Loads that page from your site, with your unsaved values."),
]


def slide(step_id, chapter_title, label):
    if step_id == "ways-1":
        image = slide_base(chapter_title, label, "Check a change before you publish")
        bullets(image, CHECK_FIRST)
        return image

    if step_id == "ways-2":
        image = slide_base(chapter_title, label, "Two ways to preview")
        bullets(image, TWO_WAYS)
        return image

    if step_id == "who-1":
        image = slide_base(chapter_title, label, "Who can preview")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "Limits of live preview")
        bullets(image, LIMITS, active={0, 2})
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "Preview through a page of your site")
        bullets(image, ADD_THE_PART)
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "How the Preview part works")
        bullets(image, HOW_THE_PART_WORKS)
        return image

    raise SystemExit(f"No slide for {step_id}")
