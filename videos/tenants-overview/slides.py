"""The slides of the Tenants overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, GREEN, MARGIN, MUTED, PANEL, TEXT, W, bullets, font, slide_base

SITE = [
    ("1", "One application", "A single deployment hosts every site."),
    ("2", "Many tenants", "Each tenant is a separate website."),
    ("3", "Kept apart", "Own content, themes and users, in its own database or tables."),
]

DEFAULT_TENANT = [
    ("1", "The first tenant", "It is the first tenant you set up."),
    ("2", "Manages the others", "The Tenants pages exist on the Default tenant alone."),
    ("3", "Cannot be disabled", "The Default tenant has no Disable button."),
]

# (what the request asks for, the tenant that answers, why)
ROUTES = [
    ("example.com", "Default", "No Hostname, no Url Prefix"),
    ("example.com/bakery", "Bakery", "Url Prefix: bakery"),
    ("studio.example.com", "Studio", "Hostname: studio.example.com"),
]

STATES = [
    ("1", "Uninitialized", "The tenant exists, but it is not set up."),
    ("2", "Running", "The tenant is set up and answers requests."),
    ("3", "Disabled", "The tenant is set up but stopped. Enable starts it again."),
]

FEATURES = [
    ("1", "Tenants", "Manage the tenants of the application from the admin."),
    ("2", "Tenant Feature Profiles", "Limit the features that each tenant can use."),
    ("3", "Distributed Tenants", "Keep tenant states in sync. Needs a distributed cache and a stateless configuration."),
    ("4", "Static File Provider", "Serve static files from a folder of each tenant."),
]

PERMISSIONS = [
    ("1", "Manage tenants", "Gives access to the Tenants pages."),
    ("2", "Manage tenant feature profiles", "Gives access to the Feature Profiles pages."),
    ("check", "The Administrator role", "Has both permissions by default."),
]


def routes(image):
    """Three requests, an arrow, and the tenant that answers each one."""
    draw = ImageDraw.Draw(image)
    y = 250
    for address, tenant, reason in ROUTES:
        draw.rounded_rectangle((MARGIN, y, 880, y + 150), radius=16, fill=PANEL, outline=BORDER, width=2)
        draw.text((MARGIN + 40, y + 75), address, font=font(40, "SemiBold"), fill=TEXT, anchor="lm")

        # The arrow between the request and the tenant.
        draw.line((912, y + 75, 1000, y + 75), fill=GREEN, width=6)
        draw.polygon([(1000, y + 52), (1040, y + 75), (1000, y + 98)], fill=GREEN)

        draw.rounded_rectangle((1072, y, W - MARGIN, y + 150), radius=16, fill=PANEL, outline=ACCENT, width=2)
        draw.text((1112, y + 50), tenant, font=font(40, "Bold"), fill=TEXT, anchor="lm")
        draw.text((1112, y + 102), reason, font=font(28), fill=MUTED, anchor="lm")
        y += 176


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "One application, many sites")
        bullets(image, SITE)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "The Default tenant")
        bullets(image, DEFAULT_TENANT)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "How a request finds its tenant")
        routes(image)
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "The state of a tenant")
        bullets(image, STATES)
        return image

    if step_id.startswith("module-"):
        index = int(step_id[-1])
        if index < 4:
            image = slide_base(chapter_title, label, "The features of the Tenants module")
            bullets(image, FEATURES, active={0} if index == 1 else {1, 2} if index == 2 else {3})
        else:
            image = slide_base(chapter_title, label, "Who can manage tenants")
            bullets(image, PERMISSIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")
