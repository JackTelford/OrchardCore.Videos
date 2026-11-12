"""The slides of the security headers video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FLOW = [
    ("1", "The browser asks", "A visitor opens a page of your site."),
    ("2", "The site answers", "The response holds the page and its headers."),
    ("3", "The browser follows", "Security headers say what the page may load, use and share."),
]

HEADERS = [
    ("1", "Content Security Policy", "Where a page may load scripts, styles, images and more from."),
    ("2", "X-Content-Type-Options", "Trust the file type a response declares, do not guess it."),
    ("3", "Permissions Policy", "Which browser features a page may use, such as the camera."),
    ("4", "Referrer Policy", "What a link reveals about the page that contains it."),
]

DEFAULTS = [
    ("check", "X-Content-Type-Options", "Always sent, as nosniff. The settings have no field for it."),
    ("check", "Referrer Policy", "Always sent, as No referrer until you choose another value."),
    ("if", "Content Security Policy", "Sent only after you give a row or a checkbox a value."),
    ("if", "Permissions Policy", "Sent only after you choose a feature other than None."),
]

GOOD_TO_KNOW = [
    ("1", "Permissions", "The page takes Manage Security Headers Settings and Manage settings. Administrator has both."),
    ("2", "Set by a developer", "Configured values take precedence over the values you save."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Headers travel with every response")
        bullets(image, FLOW)
        return image

    if step_id in ("why-2", "why-3"):
        image = slide_base(chapter_title, label, "The headers of the Security module")
        bullets(image, HEADERS, active={0, 1} if step_id == "why-2" else {2, 3})
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "What the site sends by default")
        bullets(image, DEFAULTS)
        return image

    if step_id in ("know-1", "know-2"):
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, GOOD_TO_KNOW, active={0} if step_id == "know-1" else {1})
        return image

    raise SystemExit("No slide for " + step_id)
