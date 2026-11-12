"""The slides of the Site Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

CONTENT = [
    ("1", "Site name", "What the site is called in the admin and in themes"),
    ("2", "Default Time Zone", "How dates and times are displayed and edited"),
    ("3", "Page size", "How many items an admin list shows on one page"),
    ("4", "Resources and caches", "How scripts, stylesheets, and caches behave"),
]

WHERE = [
    ("1", "Settings, General", "In the admin menu"),
    ("2", "Site, Resources, Cache", "The three tabs of the page"),
    ("3", "Other pages under Settings", "Added by other modules"),
]

WHO = [
    ("1", "Manage general settings", "The permission that opens Settings, General"),
    ("2", "Manage settings", "Includes Manage general settings"),
    ("3", "Administrator", "This role has both permissions by default"),
]


def slide(step_id, chapter_title, label):
    if step_id == "about-1":
        image = slide_base(chapter_title, label, "One set of settings for the whole site")
        bullets(image, CONTENT)
        return image

    if step_id == "about-2":
        image = slide_base(chapter_title, label, "Where to find them")
        bullets(image, WHERE)
        return image

    if step_id == "about-3":
        image = slide_base(chapter_title, label, "Who can change them")
        bullets(image, WHO)
        return image

    raise SystemExit(f"No slide for {step_id}")
