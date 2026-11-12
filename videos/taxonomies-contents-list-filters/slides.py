"""The slides of the "Taxonomies: Filter the Content Items List by Term" video. assemble.py calls slide() for each step
of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEM = [
    ("1", "Taxonomies categorize content", "The terms of a taxonomy group content items."),
    ("2", "The list grows", "The Content Items list holds more items as the site grows."),
    ("3", "One term, scattered items", "The items that share a term are spread through the list."),
]

FEATURE = [
    ("check", "One button per taxonomy", "Each taxonomy you choose adds a filter to the Content Items list."),
    ("check", "Filter by term", "The dropdown lists the terms of the taxonomy."),
    ("check", "Only matching items", "The list shows the content items that have the term."),
]

STEPS = [
    ("1", "Enable the feature", "Tools, Features"),
    ("2", "Choose the taxonomies", "Settings, Taxonomy Filters"),
    ("3", "Filter the list", "Content, Content Items"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The list grows with the site")
        bullets(image, PROBLEM)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Taxonomies Contents List Filters")
        bullets(image, FEATURE)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Three steps")
        bullets(image, STEPS)
        return image

    raise SystemExit(f"No slide for {step_id}")
