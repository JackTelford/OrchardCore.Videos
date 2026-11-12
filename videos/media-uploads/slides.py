"""The slides of the Media Uploads video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FEATURES = [
    ("1", "Media Slugify", "Clean names for the folders you create and the files you upload"),
    ("2", "Media TUS Uploads", "Large files sent in parts: pause, resume and recover"),
    ("3", "Media SignalR", "Every open Media Library shows the changes of the others"),
]

NAMES = [
    ("1", "What you type", "Images & docs   /   The team (2020).jpg"),
    ("2", "What Media Slugify stores", "images-docs   /   the-team-2020.jpg"),
    ("3", "The public address", "/media/images-docs/the-team-2020.jpg"),
    ("4", "Left as they are", "Names that exist before the feature is enabled, and renamed files"),
]

SERVERS = [
    ("1", "Resumable uploads", "All parts of a file reach the same server, or the servers share the folder and the Redis Cache feature"),
    ("2", "Live updates", "A backplane: SignalR Azure Backplane or SignalR Redis Backplane"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What each feature is for")
        bullets(image, FEATURES, active={0})
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "What each feature is for")
        bullets(image, FEATURES, active={1})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What each feature is for")
        bullets(image, FEATURES, active={2})
        return image

    if step_id == "names-1":
        image = slide_base(chapter_title, label, "From the name you type to the stored name")
        bullets(image, NAMES, active={0, 1})
        return image

    if step_id == "names-2":
        image = slide_base(chapter_title, label, "From the name you type to the stored name")
        bullets(image, NAMES, active={2, 3})
        return image

    if step_id == "servers-1":
        image = slide_base(chapter_title, label, "Running on several servers")
        bullets(image, SERVERS, active={0})
        return image

    if step_id == "servers-2":
        image = slide_base(chapter_title, label, "Running on several servers")
        bullets(image, SERVERS, active={1})
        return image

    raise SystemExit(f"No slide for {step_id}")
