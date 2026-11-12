"""The slides of the SEO Meta video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PLACES = [
    ("1", "Social Meta Settings", "Defaults for every page that shows a content item."),
    ("2", "SeoMeta part", "Values for one page, edited in the SEO tab of the editor."),
]

FORMATS = [
    ("1", "Open Graph", "A format for the preview of a shared link."),
    ("2", "Twitter Card", "A second format for the same purpose."),
]

TITLE_AND_ADDRESS = [
    ("1", "Page Title", "Replaces the whole browser title, without the site name."),
    ("2", "Meta Description", "The page value, then Default Meta Description."),
    ("3", "Canonical", "The page value, then the address of the page itself."),
]

SOCIAL_TEXT = [
    ("1", "Open Graph and Twitter title", "Its own field, then Page Title, then the title of the content item."),
    ("2", "Open Graph and Twitter description", "Its own field, then the page's Meta Description, then Default Open Graph Description or Default Twitter Description."),
]

PICTURES = [
    ("1", "Open graph image or Twitter image", "Set on the page."),
    ("2", "Default social image", "Set on the page."),
    ("3", "Open graph image or Twitter Image", "Set in Social Meta Settings."),
    ("4", "Default social image", "Set in Social Meta Settings."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Two places for the values")
        bullets(image, PLACES, active={0})
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two places for the values")
        bullets(image, PLACES, active={1})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Two formats for shared links")
        bullets(image, FORMATS)
        return image

    if step_id == "sources-1":
        image = slide_base(chapter_title, label, "Title, description and address")
        bullets(image, TITLE_AND_ADDRESS)
        return image

    if step_id == "sources-2":
        image = slide_base(chapter_title, label, "Titles and descriptions for sharing")
        bullets(image, SOCIAL_TEXT)
        return image

    if step_id == "sources-3":
        image = slide_base(chapter_title, label, "Pictures, in the order they are used")
        bullets(image, PICTURES)
        return image

    raise SystemExit(f"No slide for {step_id}")
