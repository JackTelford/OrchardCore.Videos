"""The slides of the "Workflows for Tenants" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, W, bullets, font, slide_base

# The four activities of the Tenant category, and the outcomes each one has.
ACTIVITIES = [
    ("1", "Create Tenant Task", "Adds a tenant. Outcomes: Done, Failed."),
    ("2", "Setup Tenant Task", "Sets up a tenant. Outcomes: Done, Failed."),
    ("3", "Disable Tenant Task", "Disables a tenant. Outcomes: Disabled, Failed."),
    ("4", "Enable Tenant Task", "Enables a tenant. Outcomes: Enabled, Failed."),
]

# What each activity checks before it works on a tenant.
CHECKS = [
    ("1", "Create Tenant Task", "A name no tenant uses, and an address of its own."),
    ("2", "Setup Tenant Task", "A tenant in the Uninitialized state."),
    ("3", "Disable Tenant Task", "A Running tenant, other than the Default one."),
    ("4", "Enable Tenant Task", "A Disabled tenant."),
]

# What happens when a task takes its Failed outcome.
FAILED = [
    ("1", "Failed leads nowhere by itself", "A task that can't do its job takes the Failed outcome."),
    ("2", "No connection, the workflow ends", "The instance still reads Finished."),
    ("3", "Connect Failed to a report", "An activity such as Log or Notify tells you what happened."),
]

# More ways to use the tasks.
MORE = [
    ("1", "Run it a second time", "Bluebird exists, so Create Tenant Task takes Failed."),
    ("2", "Switch a tenant off and on", "Disable Tenant Task and Enable Tenant Task need only a tenant name."),
    ("3", "Start it from any event", "The start event decides when Disable and Enable Tenant Task run."),
]


def footer(image, text):
    """A line under the cards."""
    draw = ImageDraw.Draw(image)
    draw.ellipse((96, 976, 112, 992), fill=GREEN)
    draw.text((132, 984), text, font=font(32, "SemiBold"), fill=GREEN, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id in ("activities-1", "activities-2"):
        image = slide_base(chapter_title, label, "Four activities in the Tenant category")
        bullets(image, ACTIVITIES, active={0, 1} if step_id == "activities-1" else {2, 3})
        return image

    if step_id in ("activities-3", "activities-4"):
        image = slide_base(chapter_title, label, "Each activity checks its tenant first")
        bullets(image, CHECKS, active={0, 1} if step_id == "activities-3" else {2, 3})
        footer(image, "All four work in the Default tenant, which manages the others.")
        return image

    if step_id == "wrap-1":
        image = slide_base(chapter_title, label, "When a task takes Failed")
        bullets(image, FAILED)
        return image

    if step_id == "wrap-2":
        image = slide_base(chapter_title, label, "More ways to use the tasks")
        bullets(image, MORE)
        return image

    raise SystemExit(f"No slide for {step_id}")
