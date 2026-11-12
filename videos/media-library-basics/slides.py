"""The slides of the Media Library video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHY = [
    ("1", "Files in many places", "Photos, PDFs and downloads used by many pages."),
    ("2", "One library", "Files and folders managed in the admin, each file with its own public address."),
    ("3", "Used by content", "A media field keeps the path of a file, so one upload serves many pages."),
]

WHY_ACTIVE = {"why-1": {0}, "why-2": {1}, "why-3": {2}}

OPTIONAL = [
    ("1", "Media Slugify", "Clean names for the folders and files you add."),
    ("2", "Media TUS Uploads", "Uploads that can be paused, resumed and recovered."),
    ("3", "Media SignalR", "Open libraries stay in sync across tabs and users."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2", "why-3"):
        image = slide_base(chapter_title, label, "One library for the files of a site")
        bullets(image, WHY, active=WHY_ACTIVE[step_id])
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "Optional features of the Media module")
        bullets(image, OPTIONAL)
        return image

    raise SystemExit(f"No slide for {step_id}")
