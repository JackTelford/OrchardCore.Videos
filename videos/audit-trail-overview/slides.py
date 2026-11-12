"""The slides of the Audit Trail overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

QUESTIONS = [
    ("1", "Who", "Which user made the change"),
    ("2", "What", "Which item changed, and in which version"),
    ("3", "When", "The date and time of the change"),
    ("4", "Why", "An optional comment from the editor"),
]

EVENT_FACTS = [
    ("1", "Category", "The area the event belongs to, such as Content"),
    ("2", "Event name", "What happened, such as Published"),
    ("3", "User", "The user who caused the event"),
    ("4", "Time", "When the event was recorded"),
]

CATEGORIES = [
    ("check", "Content", "Added by the Contents module: Created, Saved, Published, Removed, Restored"),
    ("check", "User", "Added by the Users Audit Trail feature: Logged in, Created, Updated, and more"),
]

ACCESS = [
    ("1", "View audit trail", "Opens the log under Tools, Audit Trail"),
    ("2", "Manage audit trail settings", "Changes what is recorded in Settings, Audit Trail"),
    ("3", "Manage settings", "Also needed to open Settings, Audit Trail"),
    ("check", "Administrator", "The Administrator role has all of them"),
]

COMMENTS = [
    ("1", "A content part", "Add it to a content type to get a Comment box in the editor"),
    ("2", "Saved with the event", "The comment is stored with the event it belongs to"),
    ("3", "Shown in the log", "It appears in the row, and in Content Item Info"),
]

MORE = [
    ("1", "User events", "The Users Audit Trail feature records events such as Logged in"),
    ("2", "Client IP address", "The Events tab has an Allow client IP address box"),
    ("3", "Personal data", "Depending on your legislation, an IP address can be personal data"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Questions a log can answer")
        bullets(image, QUESTIONS, active={0, 1, 2})
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "What an event holds")
        bullets(image, EVENT_FACTS)
        return image
    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Categories of events")
        bullets(image, CATEGORIES)
        return image
    if step_id == "notes-1":
        image = slide_base(chapter_title, label, "Who can use it")
        bullets(image, ACCESS)
        return image
    if step_id == "notes-2":
        image = slide_base(chapter_title, label, "Comments on changes")
        bullets(image, COMMENTS)
        return image
    if step_id == "notes-3":
        image = slide_base(chapter_title, label, "More to know")
        bullets(image, MORE)
        return image
    raise SystemExit(f"No slide for {step_id}")
