"""The slides of the "Data Pipelines: Schedule Runs and Read the History" video. assemble.py calls slide() for each
step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

RUNS = [
    ("1", "Published version", "A run executes the published version of a pipeline, never its draft."),
    ("2", "In the background", "A run is queued and executes in the background, so no page waits for it."),
    ("3", "Access of the publisher", "A run reads and writes data with the access of the user who published the pipeline."),
    ("4", "One run at a time", "Only one run of a pipeline executes at a time. The others wait in the queue."),
]

SCHEDULE = [
    ("1", "Timer Event", "Starts the workflow on a schedule, written as a CRON expression."),
    ("2", "Run Data Pipeline Task", "Queues a run of the published version of a pipeline."),
    ("3", "Run History", "Lists every run, whatever started it."),
    ("4", "Features", "Workflows and Timer Workflows Activities, besides Data Pipelines."),
]

KEEP = [
    ("1", "Cancel a run", "A queued run is cancelled at once. A running run stops at its next batch."),
    ("2", "Permission", "Run data pipelines allows starting and cancelling runs."),
    ("3", "Retention", "By default, runs are deleted 30 days after they end."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("runs-1", "runs-2"):
        image = slide_base(chapter_title, label, "What a run is")
        bullets(image, RUNS, active={0, 1} if step_id == "runs-1" else {2, 3})
        return image

    if step_id == "schedule-1":
        image = slide_base(chapter_title, label, "A schedule is a workflow")
        bullets(image, SCHEDULE)
        return image

    if step_id == "keep-1":
        image = slide_base(chapter_title, label, "Cancel runs, keep the history")
        bullets(image, KEEP)
        return image

    raise SystemExit("No slide for " + step_id)
