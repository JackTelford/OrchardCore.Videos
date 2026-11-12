"""The slides of the Content Fields video about links, rich text, Markdown and YouTube videos.

assemble.py calls slide() for each step of a "slide" clip. Everything visual comes from brand.py, so the slides look like
those of every other Orchard Core video. This is an admin video: the slides name screens and fields, and show no code.
"""
from brand import bullets, slide_base

# The four fields, each with the need of a workshop page that it answers.
FIELDS = [
    ("1", "Link Field", "A sign-up link: an address, the link text and where it opens."),
    ("2", "Html Field", "A schedule: formatted text, written in a rich text editor."),
    ("3", "Markdown Field", "A list of things to bring: text written in Markdown."),
    ("4", "Youtube Field", "A teaser video: the address of a video, shown in a player."),
]

# Where the fields come from, and where a site enables them.
MODULES = [
    ("1", "Content Fields", "The Link Field, the Html Field and the Youtube Field."),
    ("2", "Markdown", "The Markdown Field."),
    ("3", "Tools, Features", "Enable both modules from the list of features."),
]

# What happens to Html and Markdown content.
SANITIZING = [
    ("check", "Sanitize Html", "On by default in the Html Field and the Markdown Field."),
    ("check", "Cleaned when displayed", "Unsafe markup, such as script tags, is removed."),
    ("check", "Stored as written", "What you type is saved unchanged."),
]

# Two more checks.
CHECKS = [
    ("check", "HTML inside Markdown", "By default, HTML typed in Markdown is shown as text."),
    ("check", "Link Field validation", "An address that isn't valid is rejected."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("fields-1", "fields-2", "fields-3"):
        image = slide_base(chapter_title, label, "One field for each need")
        active = {"fields-1": None, "fields-2": {0, 1}, "fields-3": {2, 3}}[step_id]
        bullets(image, FIELDS, active=active)
        return image
    if step_id == "fields-4":
        image = slide_base(chapter_title, label, "Two modules, one list of features")
        bullets(image, MODULES)
        return image
    if step_id == "safe-1":
        image = slide_base(chapter_title, label, "What happens to your content")
        bullets(image, SANITIZING)
        return image
    if step_id == "safe-2":
        image = slide_base(chapter_title, label, "Two more checks")
        bullets(image, CHECKS)
        return image
    raise SystemExit("No slide for " + step_id)
