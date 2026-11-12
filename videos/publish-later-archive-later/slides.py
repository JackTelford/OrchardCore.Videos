"""The slides of the Publish Later and Archive Later video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA_1 = [
    ("1", "Publish Later", "A draft is published at the date and time you choose."),
    ("2", "Archive Later", "A published item is unpublished at the date and time you choose."),
]

IDEA_2 = [
    ("1", "A content part", "Attach Publish Later or Archive Later to the content types that need it."),
    ("2", "A date and time", "Editors enter it in the content editor, next to the other actions."),
    ("3", "A background task", "It checks every minute for items whose time has arrived."),
    ("4", "A change of state", "The item is published or unpublished by itself."),
]

TIME_ZONES = [
    ("1", "The site", "Settings, General, Default Time Zone sets the zone for the times you enter."),
    ("2", "The user", "With the User Time Zone feature enabled, a zone set for a user takes precedence."),
    ("3", "Stored as UTC", "A later change of zone changes how a time is shown, not when it happens."),
]

TIMING = [
    ("1", "Who sees the controls", "Only people who may publish the item."),
    ("2", "When the change happens", "At the first run of the task after the time, never before it."),
    ("3", "Where it runs", "Only while the site is running. Due items wait until it runs again."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Two scheduled changes")
        bullets(image, IDEA_1)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "One part and one task for each")
        bullets(image, IDEA_2)
        return image

    if step_id == "know-1":
        image = slide_base(chapter_title, label, "Which time zone applies")
        bullets(image, TIME_ZONES)
        return image

    if step_id == "know-2":
        image = slide_base(chapter_title, label, "Permissions and timing")
        bullets(image, TIMING)
        return image

    raise SystemExit("No slide for " + step_id)
