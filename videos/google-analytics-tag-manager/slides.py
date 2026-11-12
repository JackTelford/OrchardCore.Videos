"""The slides of the Google Analytics and Google Tag Manager video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FEATURES = [
    ("1", "Google Analytics", "Takes the tracking ID of your Google Analytics account."),
    ("2", "Google Tag Manager", "Takes the container ID. You choose the tags in your Google account."),
    ("3", "A script on each public page", "Added to the head of the page, and loaded from Google."),
    ("4", "Either or both", "Each has its own settings page. No saved ID, no script."),
]

LEFT_OUT = [
    ("1", "Pages of the admin", "The scripts are never added to the admin."),
    ("2", "Responses that are not full pages", "Only a full page carries a script."),
    ("3", "No ID saved", "A feature adds nothing until its ID is saved."),
    ("4", "No consent to track", "The visitor's consent does not allow tracking."),
]

STOPPING = [
    ("check", "Disable the feature", "Under Tools, Features, to stop its script."),
    ("check", "The ID is required", "The ID field cannot be saved empty."),
]

PERMISSIONS = [
    ("check", "Manage Google Analytics settings", "Needed to see and change the Google Analytics settings."),
    ("check", "Manage Google Tag Manager settings", "Needed to see and change the Google Tag Manager settings."),
    ("check", "Administrator role", "Has both permissions by default."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2", "why-3"):
        image = slide_base(chapter_title, label, "Two Google features")
        active = {"why-1": {0, 1}, "why-2": {1, 2}, "why-3": {3}}[step_id]
        bullets(image, FEATURES, active=active)
        return image

    if step_id in ("limits-1", "limits-2"):
        image = slide_base(chapter_title, label, "Four cases leave a script out")
        bullets(image, LEFT_OUT, active={0, 1} if step_id == "limits-1" else {2, 3})
        return image

    if step_id == "limits-3":
        image = slide_base(chapter_title, label, "Stopping a script")
        bullets(image, STOPPING)
        return image

    if step_id == "limits-4":
        image = slide_base(chapter_title, label, "Permissions")
        bullets(image, PERMISSIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")
