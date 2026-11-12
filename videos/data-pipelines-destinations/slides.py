"""The slides of the Data Pipelines destinations video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT = [
    ("1", "Receives files", "The files that a Create a file step or a Zip files step produces."),
    ("2", "Delivers them", "To a person, a mailbox or a file server."),
    ("3", "One output, several destinations", "Each destination receives every file."),
]

FOUR = [
    ("1", "Share a download link", "A secure link for users of the site. Needs the Email feature."),
    ("2", "Send by email", "The files attached to one email. Needs the Email feature."),
    ("3", "Upload to an FTP server", "Needs the Data Pipelines - FTP feature."),
    ("4", "Upload to an SFTP server", "Needs the Data Pipelines - SFTP feature."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The last step of a pipeline")
        bullets(image, WHAT, active={0, 1, 2})
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Four destinations in this video")
        bullets(image, FOUR, active={0, 1, 2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
