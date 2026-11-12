"""The slides of the Indexes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT = [
    ("1", "Your content", "Content items are stored in the database of the site."),
    ("2", "An index", "A copy of that content, arranged for fast searching."),
    ("3", "A search", "Looks in the index instead of reading every content item."),
]

WHO = [
    ("1", "Indexing module", "Lists, creates, edits, rebuilds and deletes indexes."),
    ("2", "Provider", "Stores the index: Lucene, Elasticsearch or Azure AI Search."),
    ("3", "Source", "Decides what an index holds, such as the content of the site."),
]

CURRENT = [
    ("1", "A content item changes", "The indexes that include its content type are updated."),
    ("2", "The change is logged", "The site records the change in a log of indexing tasks."),
    ("3", "Indexing Worker", "An optional background task that goes through the log."),
]

ACTIONS = [
    ("1", "Synchronize", "Applies the changes that the index has not processed."),
    ("2", "Reset", "Starts again from the beginning of the log. Entries stay."),
    ("3", "Rebuild", "Deletes the index, creates it again, then refills it from the log."),
]

PERMISSION = [
    ("check", "Manage Indexes", "Needed for the Indexes page, and to add, edit, delete and update indexes."),
    ("check", "Administrators", "Have the permission by default."),
    ("check", "Other roles", "Receive it when you grant the permission to the role."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "An index is a copy built for searching")
        bullets(image, WHAT)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Who does what")
        bullets(image, WHO)
        return image
    if step_id == "why-3":
        image = slide_base(chapter_title, label, "How an index stays up to date")
        bullets(image, CURRENT)
        return image
    if step_id == "diff-1":
        image = slide_base(chapter_title, label, "Three ways to update an index")
        bullets(image, ACTIONS, active={0})
        return image
    if step_id == "diff-2":
        image = slide_base(chapter_title, label, "Three ways to update an index")
        bullets(image, ACTIONS, active={1})
        return image
    if step_id == "diff-3":
        image = slide_base(chapter_title, label, "Three ways to update an index")
        bullets(image, ACTIONS, active={2})
        return image
    if step_id == "perm-1":
        image = slide_base(chapter_title, label, "Who can manage indexes")
        bullets(image, PERMISSION)
        return image
    raise SystemExit(f"No slide for {step_id}")
