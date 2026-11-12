"""The slides of the Alias part video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

IDS = [
    ("1", "Generated", "Orchard Core creates the ID when it creates the item."),
    ("2", "Different on each site", "The same menu can have another ID on each site."),
    ("3", "Hard to share", "Nothing that moves between sites can rely on it."),
]

ALIASES = [
    ("check", "A name you choose", "For example main-menu or contact-us."),
    ("check", "Stored with the item", "Recipes and deployments carry it to other sites."),
    ("check", "Not a web address", "The Autoroute part gives a page its address."),
]

RULES = [
    ("1", "One item per alias", "Unique across every content type of the site."),
    ("2", "A number when taken", "A generated alias that exists gets a number at the end."),
    ("3", "Up to 735 characters", "Longer generated aliases are cut to fit."),
    ("4", "Case ignored when finding", "Finding an item treats Support and support the same."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A content item ID can differ between sites")
        bullets(image, IDS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "An alias is a name you choose")
        bullets(image, ALIASES)
        return image

    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "The rules of an alias")
        bullets(image, RULES)
        return image

    raise SystemExit(f"No slide for {step_id}")
