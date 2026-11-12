"""The slides of the Deployment overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHY = [
    ("1", "Built on one site", "Content types, settings, and pages take shape on a development copy."),
    ("2", "Needed on another site", "A staging or live site has to end up with the same result."),
    ("3", "Repeated by hand", "Each change entered again takes time, and a missed one makes the sites differ."),
]

STAGES = [
    ("1", "Build a plan", "An ordered list of steps, kept on the source site."),
    ("2", "Execute the plan", "The steps produce a package, which is a zip file."),
    ("3", "Import the package", "The destination site runs it."),
]

PACKAGE = [
    ("1", "A zip file", "One file that you can store, move, or upload to another site."),
    ("2", "A recipe", "A list of steps that a site can run."),
    ("3", "Run in order", "The destination site runs the steps when you import the package."),
]

PERMISSIONS = [
    ("1", "Manage deployment plans", "Create, edit, and delete plans and their steps. The Plans page also needs Export Data."),
    ("2", "Export Data", "Run a plan to produce a package."),
    ("3", "Import Data", "Use Package Import and JSON Import."),
]

CRITICAL = [
    ("!", "Security Critical badge", "Shown beside Import Data when you edit a role."),
    ("1", "A recipe changes the site", "Settings, features, content definitions, and content."),
    ("2", "Grant it with care", "Give Import Data only to roles you trust."),
    ("3", "Import with care", "Use only packages and recipes from sources you trust."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Moving work between sites by hand")
        bullets(image, WHY)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Three stages")
        bullets(image, STAGES)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What a package holds")
        bullets(image, PACKAGE)
        return image

    if step_id == "perm-1":
        image = slide_base(chapter_title, label, "Three permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "perm-2":
        image = slide_base(chapter_title, label, "Import Data is Security Critical")
        bullets(image, CRITICAL)
        return image

    raise SystemExit(f"No slide for {step_id}")
