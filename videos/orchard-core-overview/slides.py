"""The slides of the Orchard Core overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import (ACCENT, BORDER, DIM, MUTED, PANEL, PANEL_DIM, TEXT, W, bullets, font, slide_base)

# Chapter 1: the framework and the CMS, stacked the way they are built.
STACK = [
    ("Orchard Core CMS", "Content management, an admin interface, themes, users and roles, search, and APIs."),
    ("Orchard Core Framework", "Modular, multi-tenant building blocks for your own applications."),
    ("ASP.NET Core", "The platform both are built on."),
]

# Chapter 5: recipes and deployment plans.
RECIPE = [
    ("1", "Enables features", "The modules the site uses."),
    ("2", "Defines content types", "Such as Article and Blog Post, with their parts and fields."),
    ("3", "Chooses the themes", "One for the site, one for the admin."),
    ("4", "Creates starting content", "A blog, an About page, and a menu."),
]

DEPLOYMENT = [
    ("1", "Deployment plan", "A list of steps that build a package."),
    ("2", "Package", "Content and configuration, ready to move to another site."),
    ("3", "Import or send", "Import it on a site, or send it to another site."),
]

# Chapter 6: one application, three tenants.
TENANTS = [
    ("Default", "example.com", "Blog recipe"),
    ("partner", "example.com/partner", "Agency recipe"),
    ("shop", "shop.example.com", "Blank site recipe"),
]

# Chapter 7: what you can build.
BUILD = [
    ("1", "Full CMS", "A theme and templates render your content. Recipes: Blog, Agency, Coming Soon."),
    ("2", "Decoupled CMS", "Your own ASP.NET Core pages render the content."),
    ("3", "Headless CMS", "Another application reads it through GraphQL or REST."),
    ("4", "Framework only", "Your own modular, multi-tenant application."),
]

# Chapter 8: where to go next.
NEXT = [
    ("1", "Modules Reference", "The built-in modules, grouped by area."),
    ("2", "Module pages", "How to use and extend each module."),
    ("3", "Glossary", "Content item, content type, tenant, layer, recipe, and more."),
    ("4", "Getting started", "Set up your first site and find your way around the admin."),
]


def stack(image, layers, active):
    """Boxes stacked from the top: each one is built on the one below it. active: the indexes shown bright."""
    draw = ImageDraw.Draw(image)
    heights = [220, 220, 170]
    y = 250
    for index, ((title, text), height) in enumerate(zip(layers, heights)):
        on = index in active
        draw.rounded_rectangle((96, y, W - 96, y + height), radius=16, fill=PANEL if on else PANEL_DIM,
                               outline=ACCENT if on else BORDER, width=2)
        draw.text((146, y + 66), title, font=font(48, "Bold"), fill=TEXT if on else DIM, anchor="lm")
        draw.text((146, y + 142 if height > 200 else y + 122), text, font=font(30), fill=MUTED if on else DIM, anchor="lm")
        y += height
        if index < len(layers) - 1:
            # A small arrow in the gap: the box above is built on the box below.
            draw.polygon([(W // 2 - 16, y + 38), (W // 2 + 16, y + 38), (W // 2, y + 10)], fill=ACCENT)
            y += 48


def tenants(image, tenants_, note_on):
    """One application holding three tenants, and a note about where they are managed."""
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((96, 250, W - 96, 800), radius=20, fill=PANEL_DIM, outline=BORDER, width=2)
    draw.text((146, 306), "One Orchard Core application", font=font(36, "SemiBold"), fill=MUTED, anchor="lm")
    gap = 30
    width = (W - 192 - 100 - 2 * gap) // 3
    x = 146
    for name, address, recipe in tenants_:
        draw.rounded_rectangle((x, 360, x + width, 740), radius=16, fill=PANEL, outline=ACCENT, width=2)
        draw.text((x + 36, 430), name, font=font(48, "Bold"), fill=TEXT, anchor="lm")
        draw.text((x + 36, 520), address, font=font(30), fill=MUTED, anchor="lm")
        draw.text((x + 36, 590), recipe, font=font(30), fill=MUTED, anchor="lm")
        draw.text((x + 36, 676), "Own content, users, theme", font=font(26), fill=DIM if not note_on else MUTED, anchor="lm")
        x += width + gap
    draw.rounded_rectangle((96, 840, W - 96, 960), radius=16, fill=PANEL if note_on else PANEL_DIM,
                           outline=ACCENT if note_on else BORDER, width=2)
    draw.text((146, 900), "Managed from the Default tenant: Multi-Tenancy > Tenants", font=font(36, "SemiBold"),
              fill=TEXT if note_on else DIM, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id.startswith("what-"):
        image = slide_base(chapter_title, label, "Two parts, built on ASP.NET Core")
        stack(image, STACK, {0, 1} if step_id == "what-1" else {1, 2} if step_id == "what-2" else {0})
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "A recipe sets up a site")
        bullets(image, RECIPE)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Deployment plans move a site")
        bullets(image, DEPLOYMENT)
        return image

    if step_id.startswith("tenants-"):
        image = slide_base(chapter_title, label, "Many sites, one application")
        tenants(image, TENANTS, step_id == "tenants-2")
        return image

    if step_id.startswith("build-"):
        image = slide_base(chapter_title, label, "What you can build")
        bullets(image, BUILD, active={0} if step_id == "build-1" else {1, 2} if step_id == "build-2" else {3})
        return image

    if step_id.startswith("next-"):
        image = slide_base(chapter_title, label, "Keep learning")
        ImageDraw.Draw(image).text((W - 96, 150), "docs.orchardcore.net", font=font(40, "SemiBold"), fill=ACCENT,
                                   anchor="rm")
        bullets(image, NEXT, active={0, 1} if step_id == "next-1" else {2} if step_id == "next-2" else {3})
        return image

    raise SystemExit(f"No slide for {step_id}")
