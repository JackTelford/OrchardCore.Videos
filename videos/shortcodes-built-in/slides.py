"""The slides of the shortcodes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The video
is for editors and site owners: the slides show what is typed into the text of a page, never code.
"""
from brand import bullets, slide_base

# A step shows one slide. Each list holds the cards of that slide: (icon, title, text).

WHAT = [
    ("1", "You type a shortcode", "A word in square brackets, such as [image], goes into the text of a page."),
    ("2", "The site replaces it", "When it displays the page, the shortcode becomes a picture, an address or text."),
    ("3", "The editor keeps it", "What you edit and store is the shortcode, not its result."),
]

PARTS = [
    ("1", "A name", "The word in the opening tag: image, asset_url or locale."),
    ("2", "Options", "Optional settings after the name, such as width."),
    ("3", "Content", "What sits between the opening and the closing tag, such as a file name."),
    ("4", "Example", '[image width="480"]home-bg.jpg[/image]'),
]

BUILT_IN = [
    ("1", "image", "Shows a picture from the media library. Comes with the Media module."),
    ("2", "asset_url", "Gives the address of a media library file. Comes with the Media module."),
    ("3", "locale", "Shows its content for a matching language. Needs the Localization module."),
]

OPTIONS = [
    ("1", "alt and class", "Alternative text for the picture, and a style class."),
    ("2", "width and height", "A resized copy, in the sizes the site accepts."),
    ("3", "mode", "How the copy fits the size, for example crop or stretch."),
    ("4", "format and quality", "Another file format, and the quality of a JPEG or WebP copy."),
]

WHERE = [
    ("1", "HTML body and HTML fields", "Shortcodes in their text are replaced when the page is displayed."),
    ("2", "Markdown body and Markdown fields", "The Markdown editor with a toolbar has Insert Shortcode and Insert Image."),
    ("3", "In the editor", "A shortcode stays plain text. Only the displayed page shows the result."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Text that depends on the site")
        bullets(image, WHAT)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "How a shortcode is written")
        bullets(image, PARTS)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The built-in shortcodes")
        bullets(image, BUILT_IN)
        return image

    if step_id == "which-1":
        image = slide_base(chapter_title, label, "Options of the image shortcode")
        bullets(image, OPTIONS)
        return image

    if step_id == "where-1":
        image = slide_base(chapter_title, label, "Where shortcodes work")
        bullets(image, WHERE, active={0, 1})
        return image

    if step_id == "where-2":
        image = slide_base(chapter_title, label, "Where shortcodes work")
        bullets(image, WHERE, active={2})
        return image

    raise SystemExit(f"No slide for {step_id}")
