"""The slides of the Elasticsearch video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
This is an admin video: the slides name screens and labels, and show no code.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

SERVER = [
    ("1", "A cloud service", "Elasticsearch offered as a hosted service."),
    ("2", "Your own machines", "Elasticsearch installed on-premises."),
    ("3", "Docker", "A container, for development and testing."),
    ("4", "Already running", "The Orchard Core module connects to the server. It does not install it."),
]

CONNECTION = [
    ("1", "Part of the site configuration", "Set by whoever hosts the site, not in the admin."),
    ("2", "A server address", "And credentials, if the server asks for them."),
    ("3", "Hidden until it is valid", "Without a valid address, Run Elasticsearch Query does not appear."),
]

INDEX = [
    ("1", "An index", "A copy of the content of the types you choose, kept on the Elasticsearch server."),
    ("2", "Kept current", "Orchard Core updates it when content changes."),
    ("3", "Used by search", "The search page of your site reads from the index."),
    ("4", "Next to Lucene", "Both modules can be enabled at the same time."),
]

TOOLS = [
    ("1", "Query Index and Index Info", "In the Actions menu of each Elasticsearch index."),
    ("2", "Run Elasticsearch Query", "Search, Queries: query any Elasticsearch index."),
    ("3", "Elasticsearch Index settings", "Include this element in the index, for a part or a field."),
    ("4", "Synchronize, Reset, Rebuild", "The Actions entries that every provider shares."),
]


def slide(step_id, chapter_title, label):
    if step_id == "fit-1":
        image = slide_base(chapter_title, label, "Elasticsearch runs on its own server")
        bullets(image, SERVER)
        return image

    if step_id == "fit-2":
        image = slide_base(chapter_title, label, "The site connects to it")
        bullets(image, CONNECTION)
        return image

    if step_id == "fit-3":
        image = slide_base(chapter_title, label, "Index, then search")
        bullets(image, INDEX)
        return image

    if step_id.startswith("tools-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "More Elasticsearch tools")
        bullets(image, TOOLS, active={index})
        return image

    raise SystemExit(f"No slide for {step_id}")
