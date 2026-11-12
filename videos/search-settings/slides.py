"""The slides of the Site Search Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

OVERVIEW = [
    ("1", "Search page", "Visitors enter terms at /search and read the results"),
    ("2", "Search index", "The page queries an index that a search provider builds"),
    ("3", "Site Search settings", "Settings, Search, Site Search: default index and placeholder"),
]

RESULTS = [
    ("1", "Summary", "Each result uses the Summary display of its content item"),
    ("2", "Paging", "Page size under Settings, General sets the results per page"),
    ("3", "Highlights", "Excerpts from the provider replace the summary when it returns them"),
]

ADDRESS = [
    ("1", "/search", "Queries the default index"),
    ("2", "/search/Pages", "Queries the index named Pages"),
    ("3", "An index name that does not exist", "Returns a not found error"),
]

WIDGET = [
    ("1", "Search Form widget", "Places a search box in a layer zone or on a page"),
    ("2", "Index Name", "Leave it empty to use the default index"),
    ("3", "Placeholder", "Required, and separate from the site setting"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("overview-"):
        image = slide_base(chapter_title, label, "The page, the index and the settings")
        bullets(image, OVERVIEW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("results-"):
        image = slide_base(chapter_title, label, "What the results page shows")
        bullets(image, RESULTS, active={int(step_id[-1]) - 1})
        return image
    if step_id == "address-1":
        image = slide_base(chapter_title, label, "The address decides the index")
        bullets(image, ADDRESS, active={0, 1, 2})
        return image
    if step_id == "address-2":
        image = slide_base(chapter_title, label, "The Search Form widget")
        bullets(image, WIDGET, active={0, 1, 2})
        return image
    raise SystemExit("No slide for " + step_id)
