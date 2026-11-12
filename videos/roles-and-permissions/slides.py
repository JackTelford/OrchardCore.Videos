"""The slides of the Roles and Permissions video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "Permission", "One thing a person may do, such as editing content or managing users."),
    ("2", "Role", "A named set of permissions, such as Editor. You grant permissions to roles."),
    ("3", "User", "A person who signs in. A user can have several roles."),
]

SYSTEM_ROLES = [
    ("check", "Administrator", "Allowed to do everything. Its permissions cannot be changed."),
    ("check", "Anonymous", "Every visitor who is not signed in."),
    ("check", "Authenticated", "Everyone who is signed in, who also receives what Anonymous allows."),
    ("4", "Your own roles", "Create as many as you need. The Blog recipe adds Author, Contributor, Editor, Moderator."),
]

RECAP = [
    ("1", "Roles hold permissions", "Allow a permission on a role to grant it explicitly."),
    ("2", "Users hold roles", "A user receives the permissions of every role they have."),
    ("3", "Effective is the result", "It adds implied permissions and those of Anonymous and Authenticated."),
    ("4", "Security Critical", "Grant these permissions with extreme consideration."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("idea-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "Who can do what")
        bullets(image, IDEA, active={index - 1})
        return image

    if step_id.startswith("system-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "The system roles, and your own")
        bullets(image, SYSTEM_ROLES, active={0} if index == 1 else {1, 2} if index == 2 else {3})
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "To sum up")
        bullets(image, RECAP)
        return image

    raise SystemExit(f"No slide for {step_id}")
