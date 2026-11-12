"""The slides of the Deployment plans video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

IDEA = [
    ("1", "A named list of steps", "You create a plan and fill it with steps, in the order you choose."),
    ("2", "Most steps add data", "Content, content definitions, settings, or a file."),
    ("3", "Features provide the steps", "A step is available when the feature that provides it is enabled."),
    ("4", "Executing builds a package", "The steps run from top to bottom and produce one package."),
]

PACKAGE = [
    ("1", "A recipe", "Steps such as Content add their data to it, in the plan's order."),
    ("2", "Your files", "Each Custom File step adds its file at the path you entered."),
    ("3", "A zip named after the plan", "Spaces and punctuation are removed: Spring Launch becomes SpringLaunch."),
]

ACCESS = [
    ("1", "Manage deployment plans", "Create, edit, and delete plans and their steps, and reorder steps."),
    ("2", "Export Data", "Execute a plan to produce its package."),
    ("3", "Administrators", "Have both permissions. The Plans page needs both."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("plan-1", "plan-2"):
        image = slide_base(chapter_title, label, "A deployment plan")
        bullets(image, IDEA, active=(0, 1) if step_id == "plan-1" else (2, 3))
        return image

    if step_id == "package-1":
        image = slide_base(chapter_title, label, "What the package holds")
        bullets(image, PACKAGE)
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "Who can use plans")
        bullets(image, ACCESS)
        return image

    raise SystemExit("No slide for " + step_id)
