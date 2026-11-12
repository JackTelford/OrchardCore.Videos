"""The slides of the home page video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOME_ROUTE = ("1", "Home Route", "Opens the page you chose at the address of the site.")
AUTOROUTE = ("2", "Autoroute", "Gives content items a permalink and a Set as homepage checkbox.")

ADDRESS = ("1", "The address of the site", "A visitor types it, with nothing after it.")
REMEMBERS = ("2", "Home Route", "Sends every visit to that address to the page you chose.")

NEEDS = [
    ("1", "A content type with the Autoroute part", "Article is one example."),
    ("2", "Show homepage options", "A setting of the Autoroute part, checked on the content type."),
    ("3", "Set homepage", "A permission of a role. The Administrator role has it by default."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "What the home page is")
        bullets(image, [ADDRESS, REMEMBERS], active={0, 1})
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Two features, one choice")
        bullets(image, [HOME_ROUTE, AUTOROUTE], active={1})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What the checkbox needs")
        bullets(image, NEEDS)
        return image

    raise SystemExit(f"No slide for {step_id}")
