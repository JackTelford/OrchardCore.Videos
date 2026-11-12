"""The slides of the Content Picker and User Picker Fields video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. This is an
admin video: the slides hold names and short sentences, never code.
"""
from brand import bullets, slide_base

WAYS = [
    ("1", "Bag Part", "Stores items inside their parent item."),
    ("2", "List Part", "Keeps items in a container, like the posts of a blog."),
    ("3", "Taxonomies", "Tags items with shared terms."),
    ("4", "Content Picker Field", "Links to items that already exist, many to many."),
]

USES = [
    ("1", "Product bundle", "Picks the products it contains."),
    ("2", "Blog post", "Picks related posts for further reading."),
    ("3", "Any content item", "Picks the user accounts connected to it."),
]

FIELDS = [
    ("1", "Content Picker Field", "Links to content items of the types you allow."),
    ("2", "User Picker Field", "Links to users. Needs the Users feature."),
    ("3", "Localization Set Content Picker Field", "Links to translations. Needs Content Localization."),
]

EDITORS = [
    ("1", "Standard", "The default. Searches the database by title."),
    ("2", "Lucene", "Lucene Content Picker feature: a Lucene editor with an Index setting."),
    ("3", "Elasticsearch", "Elasticsearch Content Picker feature: an Elasticsearch editor with an Index setting."),
]

GLANCE = [
    ("1", "Content Picker Field", "Items of the types you choose, many to many."),
    ("2", "User Picker Field", "Users, all of them or only those in chosen roles."),
    ("3", "References", "Each choice is stored as a reference. One choice or several."),
    ("4", "Editors", "Standard, Lucene or Elasticsearch choose where the search runs."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("relate-1", "relate-2"):
        image = slide_base(chapter_title, label, "Ways to relate content items")
        bullets(image, WAYS, active={0, 1, 2} if step_id == "relate-1" else {3})
        return image

    if step_id == "relate-3":
        image = slide_base(chapter_title, label, "Typical uses of the pickers")
        bullets(image, USES)
        return image

    if step_id == "relate-4":
        image = slide_base(chapter_title, label, "Three picker fields")
        bullets(image, FIELDS)
        return image

    if step_id == "index-1":
        image = slide_base(chapter_title, label, "Where the picker searches")
        bullets(image, EDITORS)
        return image

    if step_id in ("recap-1", "recap-2"):
        image = slide_base(chapter_title, label, "Content pickers at a glance")
        bullets(image, GLANCE, active={0, 1} if step_id == "recap-1" else {2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")
