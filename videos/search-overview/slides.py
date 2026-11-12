"""The slides of the site search video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PIECES = [
    ("1", "An index", "Holds the searchable text of your content items, and follows their changes."),
    ("2", "A search provider", "Creates and queries indexes: Lucene, Elasticsearch or Azure AI Search."),
    ("3", "The Search feature", "Adds the search page and the Search Form widget."),
    ("4", "A permission", "Visitors need permission to query an index."),
]

ACTIONS = [
    ("1", "Synchronize", "Processes the content changes that the index has not seen."),
    ("2", "Reset", "Restarts the indexing of all content items, and keeps the existing entries."),
    ("3", "Rebuild", "Builds the index again from scratch, then indexes all content items."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("how-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "Four pieces behind site search")
        bullets(image, PIECES, active={0} if index == 1 else {1} if index == 2 else {2, 3})
        return image

    if step_id == "maintain-1":
        image = slide_base(chapter_title, label, "Three actions on an index")
        bullets(image, ACTIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")
