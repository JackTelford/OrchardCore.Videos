"""The slides of the Themes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MARGIN, MUTED, bullets, font, slide_base

LOOK = [
    ("1", "Layout", "Where the parts of a page go."),
    ("2", "Colors and fonts", "How every page is styled."),
    ("3", "Your content", "Stays in your site when the theme changes."),
]

TEMPLATES = [
    ("1", "Layout and styling", "Shared by every page of the site."),
    ("2", "Content templates", "A theme can draw a type of content in its own way."),
    ("3", "The Blog Theme", "Has templates for blog posts, categories, and tags."),
]

TWO_THEMES = [
    ("1", "Site theme", "Draws the pages that visitors see."),
    ("2", "Admin theme", "Draws the admin, and the sign-in page by default."),
    ("3", "Sign-in page", "Check Use site theme for login page in Settings, Security, User Login."),
]

RECIPES = [
    ("1", "Blog", "The Blog Theme"),
    ("2", "Agency", "The Agency Theme"),
    ("3", "Coming Soon", "The Coming Soon Theme"),
    ("4", "Software as a Service", "The Default Theme"),
]

PERMISSION = [
    ("check", "Themes page", "Only users with Apply a Theme can open it."),
    ("check", "Administrator", "The Administrator role has the permission by default."),
    ("check", "Other roles", "Grant it by editing the role under Access Control, Roles."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "A theme is the look of a site")
        bullets(image, LOOK)
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "Themes can include content templates")
        bullets(image, TEMPLATES)
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "Two themes for every site")
        bullets(image, TWO_THEMES)
        return image

    if step_id == "what-4":
        image = slide_base(chapter_title, label, "The setup recipe picks the Site theme")
        bullets(image, RECIPES)
        draw = ImageDraw.Draw(image)
        draw.text((MARGIN, 990), "Blank site and Headless site set no Site theme.", font=font(28), fill=MUTED, anchor="lm")
        return image

    if step_id == "who-1":
        image = slide_base(chapter_title, label, "The Apply a Theme permission")
        bullets(image, PERMISSION)
        return image

    raise SystemExit(f"No slide for {step_id}")
