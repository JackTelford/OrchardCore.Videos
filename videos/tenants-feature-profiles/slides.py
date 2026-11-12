"""The slides of the Tenant Feature Profiles video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import MARGIN, W, bullets, font, slide_base

# The text of a card starts 140 px right of the card's left edge and must stop before its right edge.
MAX_TEXT_WIDTH = W - 2 * MARGIN - 140 - 40

WITHOUT = [
    ("1", "The same features for every tenant", "The Features page of each tenant lists them."),
    ("2", "Enabled by the tenant's administrator", "Its administrator decides what to turn on."),
]

WITH = [
    ("1", "A named list of rules", "Created on the Default tenant."),
    ("2", "Assigned to a tenant", "Its rules decide which features the tenant is offered."),
    ("3", "Shared by tenants", "Several tenants can use the same profile."),
    ("4", "Optional", "A tenant without a profile is not restricted."),
]

RULE_PARTS = [
    ("1", "Rule", "Exclude makes features unavailable. Include makes them available."),
    ("2", "Expression", "Matched against feature IDs, such as OrchardCore.Workflows."),
    ("3", "Wildcards", "An asterisk matches any characters, as in OrchardCore.Email.*"),
]

RULE_ORDER = [
    ("1", "Exclude OrchardCore.Email.*", "Every Email provider becomes unavailable."),
    ("2", "Include OrchardCore.Email.Smtp", "SMTP Email Provider is available again."),
    ("!", "In the reverse order", "The Exclude rule comes last, so SMTP Email Provider stays unavailable."),
    ("check", "No rule matches", "The feature stays available."),
]

DEPENDENCIES = [
    ("1", "Exclude OrchardCore.Workflows", "The Workflows feature is unavailable."),
    ("2", "Features that depend on it", "HTTP, Timer and Session Workflows Activities are unavailable too."),
    ("check", "Every other feature", "Stays available."),
]


def _check(items):
    """Raises when a card's text would run past the right edge of the card."""
    for _, title, text in items:
        for value, size, weight in ((title, 36, "Bold"), (text, 28, "Regular")):
            if font(size, weight).getlength(value) > MAX_TEXT_WIDTH:
                raise SystemExit(f"Text too wide for a card: {value!r}")
    return items


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without feature profiles")
        bullets(image, _check(WITHOUT))
        return image

    if step_id in ("why-2", "why-3"):
        image = slide_base(chapter_title, label, "With feature profiles")
        bullets(image, _check(WITH), active={0, 1} if step_id == "why-2" else {2, 3})
        return image

    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "Each rule has two parts")
        bullets(image, _check(RULE_PARTS))
        return image

    if step_id == "rules-2":
        image = slide_base(chapter_title, label, "The last matching rule decides")
        bullets(image, _check(RULE_ORDER))
        return image

    if step_id == "rules-3":
        image = slide_base(chapter_title, label, "Dependencies follow the exclusion")
        bullets(image, _check(DEPENDENCIES))
        return image

    raise SystemExit(f"No slide for {step_id}")
