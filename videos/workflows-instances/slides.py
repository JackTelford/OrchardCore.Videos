"""The slides of the Workflow Instances video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

DEFINITION = [
    ("1", "Definition", "The activities of a workflow, and the connections between them."),
    ("2", "Event", "Something the workflow listens for happens, and starts a run."),
    ("3", "Instance", "One run, with its own values and its own history."),
]

IN_MOTION = [
    ("1", "Halted", "The instance waits for an event before it goes on."),
    ("2", "Running", "Idle, Starting, Resuming or Executing: a run in progress."),
]

STOPPED = [
    ("check", "Finished", "The instance reached its last activity."),
    ("2", "Faulted", "An activity failed. The instance can be retried."),
    ("3", "Aborted", "The instance was canceled."),
]


def slide(step_id, chapter_title, label):
    if step_id == "instance-1":
        image = slide_base(chapter_title, label, "One workflow, many instances")
        bullets(image, DEFINITION)
        return image

    if step_id == "status-1":
        image = slide_base(chapter_title, label, "Instances that have not ended")
        bullets(image, IN_MOTION)
        return image

    if step_id == "status-2":
        image = slide_base(chapter_title, label, "Instances that have stopped")
        bullets(image, STOPPED)
        return image

    raise SystemExit(f"No slide for {step_id}")
