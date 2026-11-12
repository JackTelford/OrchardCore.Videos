"""The slides of the Timer Workflows video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "Something happens", "A content item is published, or a user is created."),
    ("2", "The clock strikes", "The Timer Event runs the workflow when its schedule comes due."),
    ("3", "A wait in the middle", "The Timer Event holds a workflow until its next scheduled time."),
]

FIELDS = [
    ("1", "Minute and hour", "The time of day."),
    ("2", "Day of the month and month", "The date."),
    ("3", "Day of the week", "The weekday."),
    ("*", "A star", "Matches every value of its field."),
]

EXAMPLES = [
    ("1", "*/5 * * * *", "Every five minutes: the default."),
    ("2", "0 * * * *", "Every hour, on the hour."),
    ("3", "0 8 * * *", "Every day at 8:00 (UTC by default)."),
    ("4", "0 9 1 * *", "The first day of each month at 9:00."),
]

GOOD_TO_KNOW = [
    ("1", "Checked once a minute", "The Timed Workflow Starter background task looks for due timers."),
    ("2", "The site must run", "By default, timers resume after a restart with the first request."),
    ("3", "One instance for each run", "Check Delete successfully completed workflows, then publish."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("how-1", "how-2"):
        image = slide_base(chapter_title, label, "The clock as an event")
        bullets(image, IDEA, active={0, 1} if step_id == "how-1" else {2})
        return image
    if step_id == "how-3":
        image = slide_base(chapter_title, label, "A CRON expression has five fields")
        bullets(image, FIELDS)
        return image
    if step_id == "how-4":
        image = slide_base(chapter_title, label, "Four schedules")
        bullets(image, EXAMPLES)
        return image
    if step_id.startswith("know-"):
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, GOOD_TO_KNOW, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit("No slide for " + step_id)
