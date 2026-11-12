"""The slides of the Html and Markdown video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# Chapter 01: what the two body parts are.
PARTS = [
    ("1", "HtmlBody", "Stores HTML. The Html module provides it."),
    ("2", "MarkdownBody", "Stores Markdown, plain text with simple marks. The Markdown module provides it."),
    ("3", "Both are parts of a content type", "Every item of the type gets the body editor of the part."),
]

EDITORS = [
    ("1", "HtmlBody: Standard", "A text box with line numbers for the HTML source."),
    ("2", "HtmlBody: Wysiwyg editor", "Formatted text with a toolbar."),
    ("3", "HtmlBody: Trumbowyg editor, Monaco editor", "A toolbar you configure, and a source code editor."),
    ("4", "MarkdownBody: Standard, Wysiwyg editor", "A plain text box, or a toolbar with a preview."),
]

FLOW = [
    ("1", "You write", "Formatted text or HTML in HtmlBody. Markdown in MarkdownBody."),
    ("2", "The site stores it", "Without changing it."),
    ("3", "A visitor opens the page", "Markdown becomes HTML, and shortcodes are replaced by their output."),
    ("4", "Sanitize Html", "Removes unsafe markup from the result. On by default."),
]

# Chapter 06: good to know.
CHOICES = [
    ("1", "Formatted text with a toolbar", "HtmlBody with the Wysiwyg editor."),
    ("2", "HTML source", "HtmlBody with the Standard editor or the Monaco editor."),
    ("3", "Plain text with simple marks", "MarkdownBody, with the Standard editor or the Wysiwyg editor."),
]

SANITIZE = [
    ("check", "On by default", "Removes unsafe markup, such as script tags, from the page."),
    ("check", "A setting of each body part", "You set it on every content type that has the part."),
    ("!", "Turn it off with care", "Only for content you fully trust."),
]

TEXT_RULES = [
    ("1", "HTML in a Markdown body", "Appears as text on the page by default."),
    ("2", "Shortcodes", "Work in both bodies. The Wysiwyg editors have a button to insert them."),
]

FIELD = [
    ("1", "A field, not a part", "You add it to a content type, next to your other fields."),
    ("2", "The same editors", "Standard and Wysiwyg editor."),
    ("3", "The same output", "Converted to HTML, shortcodes replaced, then sanitized."),
]


def slide(step_id, chapter_title, label):
    if step_id == "kinds-1":
        image = slide_base(chapter_title, label, "Two body parts")
        bullets(image, PARTS, active={0, 1})
        return image
    if step_id == "kinds-2":
        image = slide_base(chapter_title, label, "Editors")
        bullets(image, EDITORS)
        return image
    if step_id == "kinds-3":
        image = slide_base(chapter_title, label, "From the editor to the page")
        bullets(image, FLOW)
        return image
    if step_id == "good-1":
        image = slide_base(chapter_title, label, "Which body fits your writers")
        bullets(image, CHOICES)
        return image
    if step_id == "good-2":
        image = slide_base(chapter_title, label, "Sanitize Html")
        bullets(image, SANITIZE)
        return image
    if step_id == "good-3":
        image = slide_base(chapter_title, label, "Text and shortcodes")
        bullets(image, TEXT_RULES)
        return image
    if step_id == "good-4":
        image = slide_base(chapter_title, label, "The Markdown field")
        bullets(image, FIELD)
        return image
    raise SystemExit(f"No slide for {step_id}")
