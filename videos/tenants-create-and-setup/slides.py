"""The slides of the "Tenants: Create a Tenant and Set It Up" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

STAGES = [
    ("1", "Create", "The Default tenant saves the name, the address and the presets."),
    ("2", "Uninitialized", "The tenant is saved, and its setup has not run."),
    ("3", "Set up", "The Setup screen prepares the database and runs the recipe."),
    ("4", "Running", "The tenant answers requests as a site of its own."),
]

NEEDS = [
    ("check", "The Tenants feature", "Enabled on the Default tenant, the only tenant that can use it."),
    ("check", "The Manage tenants permission", "Granted to the Administrator role."),
    ("check", "The Multi-Tenancy menu", "Tenants is listed under it in the admin menu of the Default tenant."),
]


def slide(step_id, chapter_title, label):
    if step_id == "stages-1":
        image = slide_base(chapter_title, label, "Two stages")
        bullets(image, STAGES, active={0, 1})
        return image

    if step_id == "stages-2":
        image = slide_base(chapter_title, label, "Two stages")
        bullets(image, STAGES, active={2, 3})
        return image

    if step_id == "stages-3":
        image = slide_base(chapter_title, label, "What you need")
        bullets(image, NEEDS)
        return image

    raise SystemExit(f"No slide for {step_id}")
