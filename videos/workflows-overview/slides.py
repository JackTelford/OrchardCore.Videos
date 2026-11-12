"""The slides of the Workflows overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEMS = [
    ("1", "Content is published", "Someone has to tell the editors, or the readers."),
    ("2", "A user registers", "Someone has to welcome them and check the account."),
    ("3", "A form is submitted", "Someone has to read it and answer."),
    ("4", "Done by hand, or in code", "Slow to repeat, and costly to write for each case."),
]

SOLUTIONS = [
    ("check", "Drawn in the admin", "Activities on a canvas, connected by arrows."),
    ("check", "Started by events", "Content, users, requests, timers and more."),
    ("check", "Run by Orchard Core", "Each time the event happens, the diagram runs."),
    ("check", "Every run is kept", "Each run can be opened and inspected afterwards."),
]

PARTS = [
    ("1", "Event", "Waits for something to happen. Only an event can start a workflow."),
    ("2", "Task", "Does something: shows a notification, sends an email, publishes content."),
    ("3", "Outcome", "An exit of an activity, such as Done."),
    ("4", "Transition", "The connection from an outcome to the next activity."),
]

RUN = [
    ("1", "The event happens", "For example, an article is published."),
    ("2", "An instance starts", "Orchard Core runs the workflow from its start activity."),
    ("3", "Transitions are followed", "Each task runs, then an outcome leads to the next activity."),
    ("4", "It finishes, or waits", "An event in the middle holds the instance until it happens."),
]

MORE = [
    ("1", "HTTP Workflows Activities", "Receive requests and call web services."),
    ("2", "Timer Workflows Activities", "Run a workflow on a schedule."),
    ("3", "Other modules", "Events and tasks for content, users, email, SMS, tenants and more."),
]

CONTROL = [
    ("1", "If Else Task", "Chooses a path from a condition."),
    ("2", "Loops", "For Each Task, For Loop Task and While Loop Task repeat part of a workflow."),
    ("3", "Variables", "Named values that every activity of an instance can read and write."),
]

PERMISSIONS = [
    ("1", "Manage workflows", "Build and manage workflows. Administrators have it by default."),
    ("2", "Execute workflows", "Run a workflow from the designer."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "When something happens on a site")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Workflows in Orchard Core")
        bullets(image, SOLUTIONS)
        return image

    if step_id.startswith("parts-"):
        index = int(step_id[-1])
        if index < 4:
            image = slide_base(chapter_title, label, "The parts of a workflow")
            bullets(image, PARTS, active={0} if index == 1 else {1} if index == 2 else {2, 3})
        else:
            image = slide_base(chapter_title, label, "From event to finish")
            bullets(image, RUN)
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "More events and tasks")
        bullets(image, MORE)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "Control and values")
        bullets(image, CONTROL)
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "Who can use workflows")
        bullets(image, PERMISSIONS)
        return image

    raise SystemExit("No slide for " + step_id)
