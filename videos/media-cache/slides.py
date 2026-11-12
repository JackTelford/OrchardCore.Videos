"""The slides of the Media Cache video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. This is an
admin video: the slides show ideas as cards and hold no code.
"""
from brand import bullets, slide_base

# Chapter 1: how the cache works.

STORE = [
    ("1", "Files live in a remote store", "Azure Blob Storage or Amazon S3 keeps the media files."),
    ("2", "Your site serves them", "Visitors request the files from your Orchard Core site."),
    ("3", "The first request makes a copy", "The site downloads the file and keeps a copy on the web server."),
]

USES = [
    ("check", "Later requests", "The site answers them from the local copy."),
    ("check", "Image resizing", "Resized images are made from the local copy."),
    ("check", "Uploads", "The media library copies the file as soon as it is uploaded."),
    ("check", "Deletes and moves", "Deleting or moving a file removes its old copy."),
]

PURGE = [
    ("1", "Free disk space", "Copies stay on the web server until something removes them."),
    ("2", "Fetch fresh copies", "After a purge, each file is downloaded again when it is requested."),
    ("3", "In front of a content delivery network", "Purge once the CDN has fetched the files it needs."),
]

# Chapter 5: the two caches and the tasks that clean them.

CACHES = [
    ("1", "Remote file cache", "Full-size copies of the files in the remote store. Purge All empties it."),
    ("2", "Resized image cache", "Resized versions of images. Purge All leaves it alone."),
]

TASKS = [
    ("1", "Remote Media Cache Cleaner", "Cleans the cache of remote files."),
    ("2", "Resized Media Cache Cleaner", "Cleans the cache of resized images."),
]

SCHEDULE = [
    ("1", "Once a day by default", "Both tasks run on a schedule."),
    ("2", "A time limit sets what is removed", "The host configuration sets it. Without it, the tasks do nothing."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A remote store and a local copy")
        bullets(image, STORE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "How the copy is used and kept")
        bullets(image, USES)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "When to purge the cache")
        bullets(image, PURGE)
        return image

    if step_id == "clean-1":
        image = slide_base(chapter_title, label, "Two caches")
        bullets(image, CACHES)
        return image

    if step_id == "clean-2":
        image = slide_base(chapter_title, label, "Two cleaners, listed in Tools, Background Tasks")
        bullets(image, TASKS)
        return image

    if step_id == "clean-3":
        image = slide_base(chapter_title, label, "Schedule and time limit")
        bullets(image, SCHEDULE)
        return image

    raise SystemExit(f"No slide for {step_id}")
