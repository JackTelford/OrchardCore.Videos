"""The slides of the Content Types overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

# The four building blocks of the content model, one card each.
MODEL = [
    ("1", "Content item", "One piece of content, such as a page or a blog post."),
    ("2", "Content type", "A kind of content, defined as a list of parts."),
    ("3", "Content part", "One capability: a title, a permalink, a body."),
    ("4", "Content field", "One kind of value: text, a number, a date, an image."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        active = {0}
    elif step_id == "idea-2":
        active = {1}
    elif step_id == "idea-3":
        active = {2}
    elif step_id == "idea-4":
        active = {3}
    elif step_id == "idea-5":
        active = {2, 3}
    else:
        raise SystemExit(f"No slide for {step_id}")

    image = slide_base(chapter_title, label, "From content item to field")
    bullets(image, MODEL, active=active)
    return image
