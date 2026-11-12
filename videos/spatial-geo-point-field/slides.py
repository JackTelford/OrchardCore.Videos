"""The slides of the Geo Point field video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PLACES = [
    ("1", "Shops, offices and venues", "Each one has a place."),
    ("2", "A Geo Point field", "Keeps that place on the content item itself."),
    ("3", "Two ways to set it", "Type the numbers, or pick a point on a map."),
]

NUMBERS = [
    ("1", "Latitude", "A number from minus 90 to 90."),
    ("2", "Longitude", "A number from minus 180 to 180."),
    ("3", "Empty by default", "Both can stay empty unless the field is required."),
]

EDITORS = [
    ("1", "Standard", "A Latitude box and a Longitude box."),
    ("2", "Leaflet editor", "The same boxes and a map. A click on the map fills them in."),
    ("3", "OpenStreetMap", "The map background loads from the internet."),
]

AFTER = [
    ("1", "Stored with the content item", "The latitude and the longitude, as numbers."),
    ("2", "Shown on the site", "The field's template prints it as text, with six decimal places."),
    ("3", "A Lucene index", "Include this element in the index, then search by area or by distance."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Content that has a place")
        bullets(image, PLACES)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Two numbers")
        bullets(image, NUMBERS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Two editors")
        bullets(image, EDITORS)
        return image

    if step_id == "after-1":
        image = slide_base(chapter_title, label, "What happens to the location")
        bullets(image, AFTER)
        return image

    raise SystemExit(f"No slide for {step_id}")
