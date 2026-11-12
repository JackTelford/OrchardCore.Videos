"""The slides of the Build a Product Catalog video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

NEEDS = [
    ("1", "A content type", "Defines the structure of a product: a name, a price, and a picture."),
    ("2", "A taxonomy", "Holds the categories, such as Backpacks and Tents."),
    ("3", "The Media Library", "Stores the pictures that the products show."),
    ("4", "Search", "An index lets visitors find a product with a few words."),
]

INDEX = [
    ("1", "The index", "Lists the words of your content."),
    ("2", "Each word", "Points to the content items that contain it."),
    ("3", "The search page", "Looks up the words that a visitor types in the index."),
]

STEPS = [
    ("1", "Create an index", "Search, Indexes: choose the content types to index."),
    ("2", "Choose the default index", "Settings, Search, Site Search."),
    ("3", "Allow visitors", "Grant the index permission to the Anonymous role."),
]

SEARCHING = [
    ("1", "Check the permission", "The visitor needs the permission to query the index."),
    ("2", "Look the words up", "The default index returns the matching content items."),
    ("3", "Show the results", "Each result shows the title, price, picture, and category."),
]

RECAP = [
    ("check", "Content Types", "The Product type and the Product Category type."),
    ("check", "Taxonomies", "The Product Categories taxonomy, with its terms."),
    ("check", "Media", "A product-photos folder, with web-friendly file names."),
    ("check", "Search", "An index, a default search index, and a permission."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("needs-1", "needs-2", "needs-3"):
        image = slide_base(chapter_title, label, "What a product catalog needs")
        active = {"needs-1": {0}, "needs-2": {1, 2}, "needs-3": {3}}[step_id]
        bullets(image, NEEDS, active=active)
        return image

    if step_id == "how-1":
        image = slide_base(chapter_title, label, "What an index does")
        bullets(image, INDEX)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "Three steps to a working search")
        bullets(image, STEPS)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "What happens when a visitor searches")
        bullets(image, SEARCHING)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "A product catalog in Orchard Core")
        bullets(image, RECAP)
        return image

    raise SystemExit(f"No slide for {step_id}")
