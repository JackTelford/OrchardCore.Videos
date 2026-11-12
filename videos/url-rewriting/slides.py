"""The slides of the URL Rewriting video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEMS = [
    ("1", "A page is renamed", "Its old address stays in bookmarks, search results and links on other sites."),
    ("2", "A folder of files moves", "Pages keep pointing to the old location of images and downloads."),
]

KINDS = [
    ("R", "Redirect", "Sends the visitor to another address. The address in the browser changes."),
    ("R", "Use it when a page is renamed or moved", "/about-us leads to /about, and old links and bookmarks keep working."),
    ("W", "Rewrite", "Answers with the content of another address. The address in the browser stays."),
    ("W", "Use it to serve content from a hidden location", "/img/photo.jpg is served from /media/photo.jpg."),
]

ORDER = [
    ("1", "A list, from the top down", "The first rule in the list is checked first."),
    ("2", "Stop Evaluation of Subsequent Rules", "A Rewrite rule can end the check when it matches."),
    ("3", "Admin addresses are left alone", "Addresses under the admin prefix, /Admin by default, are never changed."),
]

PATTERNS = [
    ("^", "The start of the address", "The pattern must match from the first character."),
    ("$", "The end of the address", "The pattern must match up to the last character."),
    ("( )", "Parentheses capture text", "(.*) captures everything that follows /img/."),
    ("$1", "$1 puts the captured text back", "In the substitution, /media/$1 inserts what follows /img/."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "When an address stops working")
        bullets(image, PROBLEMS)
        return image

    if step_id in ("kinds-1", "kinds-2"):
        image = slide_base(chapter_title, label, "Two kinds of rule")
        bullets(image, KINDS, active={0, 1} if step_id == "kinds-1" else {2, 3})
        return image

    if step_id == "order-1":
        image = slide_base(chapter_title, label, "How the rules are checked")
        bullets(image, ORDER)
        return image

    if step_id == "pattern-1":
        image = slide_base(chapter_title, label, "Patterns are regular expressions")
        bullets(image, PATTERNS)
        return image

    raise SystemExit(f"No slide for {step_id}")
