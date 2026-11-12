"""The slides of the Feeds video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

READERS = [
    ("1", "A feed", "A page made for programs, listing the newest entries of a site."),
    ("2", "A feed reader", "A program that checks the feeds a person follows and shows their entries."),
]

FEATURES = [
    ("1", "Feeds", "Builds the feed in the RSS format and serves it. Publishes nothing by itself."),
    ("2", "Lists", "Provides the lists whose items fill a feed, with feed settings for each list."),
]

CONTENT = [
    ("1", "The list", "Its title, its address and its description."),
    ("2", "Its items", "Published items only, newest first by creation date, up to a chosen number."),
]

LINK = [
    ("1", "A link in the page", "The page of the list tells browsers and feed readers where its feed is."),
    ("2", "Controlled by the list", "Its feed settings decide whether the link exists, and where it points."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Following a site without visiting it")
        bullets(image, READERS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Two features work together")
        bullets(image, FEATURES)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What the feed of a list holds")
        bullets(image, CONTENT)
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "How readers find a feed")
        bullets(image, LINK)
        return image

    raise SystemExit("No slide for " + step_id)
