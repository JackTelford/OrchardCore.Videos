"""The slides of the "Send an Email When Content Is Published" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The video is
for administrators, so the slides hold plain words only.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEM = [
    ("1", "An article is published", "The editors want to know when it happens."),
    ("2", "Someone writes the email", "Each message is written and sent by hand."),
    ("3", "A workflow does it instead", "The site sends the same email every time."),
]

ACTIVITIES = [
    ("1", "Event", "Waits for something to happen, such as content being published."),
    ("2", "Task", "Does something, such as sending an email."),
    ("3", "Outcome", "How an activity ends, such as Done. You connect it to the next activity."),
]

OURS = [
    ("1", "Content Published Event", "Starts the workflow when content is published."),
    ("2", "Done", "The outcome of the event, connected to the Email Task."),
    ("3", "Email Task", "Sends the email to the editors."),
]

NEEDS = [
    ("check", "Workflows", "Adds Workflows under Design, where you build workflows."),
    ("check", "Email", "Adds the email settings and, with Workflows, the Email Task activity."),
    ("check", "SMTP Email Provider", "Sends the messages to a mail server."),
    ("check", "Contents", "Enabled on this site. Adds the content events when Workflows is enabled."),
]

NEXT = [
    ("1", "Content Types", "Select several types, or none for every type."),
    ("2", "Other events", "Content Created Event, Content Updated Event, Content Unpublished Event."),
    ("3", "Recipients, Cc and Bcc", "Each takes a comma-separated list of addresses."),
    ("4", "Values from the content", "The Available data tab lists the title and other values to use."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "From publishing to a message")
        bullets(image, PROBLEM)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Activities: events and tasks")
        bullets(image, ACTIVITIES)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Our workflow")
        bullets(image, OURS)
        return image

    if step_id == "needs-1":
        image = slide_base(chapter_title, label, "What the site needs")
        bullets(image, NEEDS)
        return image

    if step_id in ("next-1", "next-2", "next-3", "next-4"):
        image = slide_base(chapter_title, label, "Make it yours")
        bullets(image, NEXT, active={int(step_id[-1]) - 1})
        return image

    raise SystemExit(f"No slide for {step_id}")
