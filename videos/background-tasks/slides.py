"""The slides of the Background Tasks video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WORK = [
    ("1", "Clean up", "Resized image caches and abandoned file uploads."),
    ("2", "Publish", "Content items whose scheduled publish date has arrived."),
    ("3", "Remove old data", "Audit trail events, workflow instances and old content versions."),
    ("4", "Keep indexes current", "Modified content items are updated in the search indexes."),
]

MODULE = [
    ("check", "See what ran", "The last run, its outcome and the next scheduled run of every task."),
    ("check", "Control the schedule", "Change a schedule, and enable or disable a task."),
    ("check", "Run on demand", "The Run now action queues a task right away, whatever its schedule."),
    ("check", "Restricted access", "The Manage background tasks permission, held by the Administrator role."),
]

FAILURE = [
    ("1", "Last run badge", "Turns red, with how long ago the run started and how long it took."),
    ("2", "Failed runs badge", "Counts the failed runs, for example 1 of 3."),
    ("3", "Error message", "Appears under the description of the task in the list."),
    ("4", "Last error", "The details page adds the full error and its stack trace."),
]

MEMORY = [
    ("1", "Kept in memory", "Each server keeps the runs it executed since the application started."),
    ("2", "Cleared on restart", "A restart clears the runs. The settings stay."),
    ("3", "Session affinity", "In a web farm, a page shows the runs of the server that rendered it."),
    ("4", "Run now", "Runs the task only on the server that handled the request."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Work a site does on its own")
        bullets(image, WORK)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Background Tasks module")
        bullets(image, MODULE)
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "When a run fails")
        bullets(image, FAILURE)
        return image

    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "Memory, restarts and web farms")
        bullets(image, MEMORY)
        return image

    raise SystemExit(f"No slide for {step_id}")
