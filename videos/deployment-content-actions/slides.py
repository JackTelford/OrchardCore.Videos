"""The slides of the "Export Content From the List" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

ACTIONS = [
    ("1", "View Or Download Content As JSON", "Shows one item as a JSON document, or saves it as a file."),
    ("2", "Add Content To Deployment Plan", "Adds a Content Item step. The plan exports the item each time it runs."),
    ("3", "Export Content To Deployment Target", "Sends the selected items once, through a target such as File Download."),
    ("4", "Permissions", "Export Data. Adding to a plan also needs Manage deployment plans."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("actions-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "Three actions on the content items list")
        bullets(image, ACTIONS, active={index})
        return image

    raise SystemExit(f"No slide for {step_id}")
