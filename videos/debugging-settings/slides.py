"""The slides of the Debugging settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

SHAPES = [
    ("1", "Shape", "The data to display, and how to display it."),
    ("2", "Binding", "What draws a shape: usually a template, sometimes code."),
    ("3", "Tree", "Shapes sit inside other shapes, down to small fragments."),
]

SOURCES = [
    ("1", "A module", "Provides a default template for a shape."),
    ("2", "A theme", "Can replace that template with its own."),
    ("3", "The admin", "Templates created in the admin can draw shapes too."),
    ("?", "The question", "Which one drew this part of the page?"),
]

COMMENTS = [
    ("check", "Two comments per shape", "Written around each shape that Orchard Core renders."),
    ("check", "Hidden in the browser", "A browser does not display HTML comments."),
    ("check", "Visible in the page source", "They show which template drew each part."),
    ("check", "Off by default", "You turn it on under Settings, Debugging."),
]

PERMISSIONS = [
    ("1", "Manage debugging settings", "Gives access to Settings, Debugging."),
    ("2", "Manage settings", "Includes Manage debugging settings and Manage general settings."),
    ("3", "Administrator role", "Has these permissions by default."),
]

RESOURCES = [
    ("1", "On the General page", "Under Settings, General, in the section for scripts and stylesheets."),
    ("2", "From environment", "Disabled in Production, enabled otherwise."),
    ("3", "Enabled", "Scripts and stylesheets load their debuggable version."),
    ("4", "Disabled", "Scripts and stylesheets load their minified version."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A page is built from shapes")
        bullets(image, SHAPES)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Which template drew this part?")
        bullets(image, SOURCES)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Shape debug information")
        bullets(image, COMMENTS)
        return image

    if step_id == "perm-1":
        image = slide_base(chapter_title, label, "Who can open the Debugging page")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "rel-1":
        image = slide_base(chapter_title, label, "Resource Debug Mode")
        bullets(image, RESOURCES)
        return image

    raise SystemExit(f"No slide for {step_id}")
