"""The slides of the "Compare and Restore Content Versions" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

RECORDED = [
    ("1", "An event for each change", "Created, Saved, Published, Unpublished, Removed, Cloned, Restored."),
    ("2", "A full copy of the item", "The content item as it was when the event happened."),
    ("3", "The user and the time", "Who triggered the event, and when it happened."),
]

COPY = [
    ("1", "View", "Open the item as it was, in a read-only editor."),
    ("2", "Compare", "See what changed since the previous recorded copy."),
    ("3", "Restore", "Bring a version or a deleted item back as a draft."),
    ("4", "The published page stays", "It changes only when you publish the restored draft."),
]

LIMITS = [
    ("1", "Chosen content types", "Only the types selected in the Audit Trail settings are recorded."),
    ("2", "Retention period", "Events older than the period are removed: ten days by default, unless trimming is disabled."),
    ("3", "Two restorable events", "Restore appears on Published and Removed events, except for the latest version."),
    ("4", "Permissions", "View audit trail opens the list. Edit permission views a version, publish permission restores it."),
]


def slide(step_id, chapter_title, label):
    if step_id == "versions-1":
        image = slide_base(chapter_title, label, "What the Audit Trail records")
        bullets(image, RECORDED)
        return image

    if step_id == "versions-2":
        image = slide_base(chapter_title, label, "What you can do with a copy")
        bullets(image, COPY)
        return image

    if step_id == "versions-3":
        image = slide_base(chapter_title, label, "What to know before you rely on it")
        bullets(image, LIMITS, active={0, 1, 2})
        return image

    if step_id == "versions-4":
        image = slide_base(chapter_title, label, "What to know before you rely on it")
        bullets(image, LIMITS, active={3})
        return image

    raise SystemExit(f"No slide for {step_id}")
