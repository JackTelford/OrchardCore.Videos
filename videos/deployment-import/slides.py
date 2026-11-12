"""The slides of the Import: Package and JSON Import video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

IDEA = [
    ("1", "Recipe", "A list of steps that change the site."),
    ("2", "Package", "A zip file with the recipe and the files its steps need."),
    ("3", "Order", "The steps run in the order of the recipe, each by a feature that is enabled."),
    ("4", "Restart", "When the last step has run, the site restarts with the next request."),
]

COMPARE = [
    ("1", "Package Import", "A zip file with the files its steps need, or a single recipe file."),
    ("2", "JSON Import", "The text of a recipe in an editor. No other file travels with it."),
]

FAIL = [
    ("1", "The import stops", "The steps after the failing step do not run."),
    ("2", "Earlier steps have run", "Check the site before you import again."),
    ("3", "Package Import shows the errors", "The import failed with the following errors, then what the step reported."),
]

PERMISSIONS = [
    ("1", "Import Data", "Runs a recipe. Marked Security Critical in the permissions of a role."),
    ("2", "Administrators", "Only administrators have Import Data by default."),
    ("3", "The menu", "Package Import and JSON Import appear only for users who have Import Data."),
    ("4", "Trusted sources", "Grant Import Data to trusted roles, and import only what you trust."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("idea-"):
        image = slide_base(chapter_title, label, "What an import does")
        active = {"idea-1": {0}, "idea-2": {1}, "idea-3": {2, 3}}[step_id]
        bullets(image, IDEA, active=active)
        return image

    if step_id == "compare-1":
        image = slide_base(chapter_title, label, "Package Import or JSON Import")
        bullets(image, COMPARE)
        return image

    if step_id == "fail-1":
        image = slide_base(chapter_title, label, "When a step fails")
        bullets(image, FAIL)
        return image

    if step_id.startswith("perm-"):
        image = slide_base(chapter_title, label, "Who can import")
        bullets(image, PERMISSIONS, active={0, 1} if step_id == "perm-1" else {2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")
