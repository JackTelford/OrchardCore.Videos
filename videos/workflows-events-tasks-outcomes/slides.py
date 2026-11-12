"""The slides of the "Workflows: Events, Tasks and Outcomes" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

BLOCKS = [
    ("1", "Event", "Waits for something to happen. As the start activity, it starts the workflow."),
    ("2", "Task", "Does something: shows a notification, writes to the log, or sends an email."),
    ("3", "Outcome", "How an activity ended, such as Done. Some activities have several outcomes."),
    ("4", "Connection", "Links an outcome to the next activity. An activity no connection reaches never runs."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("blocks-"):
        index = int(step_id.split("-")[-1]) - 1
        image = slide_base(chapter_title, label, "Events, tasks and outcomes")
        bullets(image, BLOCKS, active={index})
        return image

    raise SystemExit(f"No slide for {step_id}")
