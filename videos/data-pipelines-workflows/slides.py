"""The slides of the Data Pipelines and Workflows video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

STARTS = [
    ("1", "By hand", "Actions, Run now, in the list of pipelines."),
    ("2", "By an event", "A workflow starts it when a content item is published."),
    ("3", "At set times", "A timer event starts the workflow on a schedule."),
    ("4", "After a run", "A workflow reacts when the run ends."),
]

ACTIVITIES = [
    ("1", "Run Data Pipeline Task", "Queues a run of a published pipeline, and can wait for it to end."),
    ("2", "Data Pipeline Run Completed Event", "Starts a workflow when a run of a pipeline ends."),
]

RULES = [
    ("1", "The published version", "A draft of the pipeline never runs."),
    ("2", "In the background", "A run does not hold up the request that queued it."),
    ("3", "The access of the publisher", "It reads and writes what the user who published the pipeline may."),
    ("4", "An enabled pipeline", "A disabled pipeline, or one never published, does not run."),
]


def slide(step_id, chapter_title, label):
    if step_id == "concept-1":
        image = slide_base(chapter_title, label, "Run a pipeline by hand or from a workflow")
        bullets(image, STARTS)
        return image

    if step_id == "concept-2":
        image = slide_base(chapter_title, label, "Two activities in the Data category")
        bullets(image, ACTIVITIES)
        return image

    if step_id == "concept-3":
        image = slide_base(chapter_title, label, "What a run uses")
        bullets(image, RULES)
        return image

    raise SystemExit(f"No slide for {step_id}")
