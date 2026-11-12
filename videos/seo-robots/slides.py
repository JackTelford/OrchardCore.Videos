"""The slides of the Robots video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, MARGIN, MUTED, PANEL, TEXT, W, bullets, font, slide_base

WHAT = [
    ("1", "Web robots", "Search engines send programs to visit and read your pages."),
    ("2", "A robots.txt file", "It sits at the root of your site and gives the robots directives."),
    ("3", "Which parts to visit", "The directives say which parts of the site the robots may access."),
]

GENERATED = [
    ("1", "The SEO feature", "Provides the Robots settings."),
    ("2", "The Robots settings", "Decide what the file contains."),
    ("3", "The robots.txt file", "Built from the settings, so you do not edit a file by hand."),
]

SETTINGS = [
    ("1", "Add a rule to allow all web robots.", "A rule that addresses every robot."),
    ("2", "Add a rule to disallow web robots to the admin routes.", "Asks robots to stay out of your admin path."),
    ("3", "Additional rules", "A text area for rules of your own."),
]

SITEMAPS = [
    ("1", "Include all sitemaps.", "A checkbox that the Sitemaps feature adds to the Robots page."),
    ("2", "Base url", "Set in Settings, General. The Sitemap lines are built from it."),
    ("3", "Sitemap lines", "After you save, they list enabled sitemap indexes and the enabled sitemaps outside them."),
]

PHYSICAL = [
    ("1", "The site serves that file", "A robots.txt file among the static files of the site replaces the generated one."),
    ("2", "The Robots page can show a warning", "The settings below have no effect until the file is removed."),
]

WARNING = [
    "A physical robots.txt file is found for the current site.",
    "Until removed, the settings below will have no effect.",
]


def warning_panel(image):
    """The warning of the Robots page, in a panel under the cards."""
    draw = ImageDraw.Draw(image)
    left, top, right, bottom = MARGIN, 680, W - MARGIN, 960
    draw.rounded_rectangle((left, top, right, bottom), radius=16, fill=PANEL, outline=ACCENT, width=2)
    draw.text((left + 40, top + 52), "The warning the Robots page can show", font=font(28, "SemiBold"), fill=MUTED, anchor="lm")
    y = top + 130
    for line in WARNING:
        draw.text((left + 40, y), line, font=font(36), fill=TEXT, anchor="lm")
        y += 60


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What a robots.txt file does")
        bullets(image, WHAT)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Orchard Core generates the file")
        bullets(image, GENERATED)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The Robots settings")
        bullets(image, SETTINGS)
        return image

    if step_id in ("sitemaps-1", "sitemaps-2"):
        image = slide_base(chapter_title, label, "Sitemaps in the file")
        bullets(image, SITEMAPS, active=(0,) if step_id == "sitemaps-1" else (1, 2))
        return image

    if step_id == "file-1":
        image = slide_base(chapter_title, label, "A physical robots.txt file takes precedence")
        bullets(image, PHYSICAL)
        warning_panel(image)
        return image

    raise SystemExit("No slide for " + step_id)
