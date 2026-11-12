"""The slides of the "Secrets: Store Passwords and API Keys Safely" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The video is
for administrators: the slides hold no code.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEMS = [
    ("1", "Values in many places", "Configuration files, settings pages, and shared notes."),
    ("2", "Every copy can leak", "Each place that holds a key is one more place to protect."),
    ("3", "No inventory", "Nothing lists the keys that a site uses."),
    ("4", "No reminders", "Nobody is told which keys are about to expire."),
]

SOLUTIONS = [
    ("check", "Encrypted", "Each value is encrypted before it is saved."),
    ("check", "Named", "Settings refer to a secret by its name."),
    ("check", "Listed in one place", "The list shows the type, the store, and the expiration."),
    ("check", "Expiration warnings", "Secrets that expired, or expire soon, are highlighted."),
]

PERMISSIONS = [
    ("1", "View secrets", "Opens the Secrets list. Values are never shown."),
    ("2", "Manage secrets", "Adds, edits, moves, and deletes secrets. Includes View secrets."),
    ("check", "Administrators", "Have both permissions."),
    ("check", "Other roles", "Get them under Access Control, Roles."),
]

PAGES = [
    ("1", "Secrets", "The list: add, review, edit, and delete secrets."),
    ("2", "Migrate Secrets", "Moves credentials that settings already hold into secrets."),
    ("3", "Secret Stores", "Moves secrets from one store to another."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without a central place for secrets")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Secrets module")
        bullets(image, SOLUTIONS)
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "Two permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "access-2":
        image = slide_base(chapter_title, label, "Under Tools, Security")
        bullets(image, PAGES, active={1, 2})
        return image

    raise SystemExit(f"No slide for {step_id}")
