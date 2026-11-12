"""The slides of the media profiles video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

SIZES = [
    ("1", "Banner", "Wide, across the top of a page."),
    ("2", "Thumbnail", "Small, in a list or a card."),
    ("3", "Original", "The file you uploaded, at its own size."),
    ("4", "Sizes written by hand", "Repeating them in every page makes them hard to keep the same."),
]

PROFILE = [
    ("check", "A name", "Such as banner or thumbnail."),
    ("check", "Image settings", "Such as width, height, resize mode, format and quality."),
    ("check", "Asked for by name", "A theme asks for the profile, and Orchard Core makes the image."),
    ("check", "The original stays", "The file in the Media Library is not changed."),
]

MODES = [
    ("1", "Max", "Fits the whole picture inside the size and keeps its proportions."),
    ("2", "Crop", "A large picture fills the size; what falls outside is cut away."),
    ("3", "Pad", "Fits the whole picture and fills the empty space with a color."),
    ("4", "Stretch", "Forces the exact size, so the picture can distort."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "One picture, several sizes")
        bullets(image, SIZES)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A media profile")
        bullets(image, PROFILE)
        return image

    if step_id == "modes-1":
        image = slide_base(chapter_title, label, "Resize Mode")
        bullets(image, MODES, active={0, 1})
        return image

    if step_id == "modes-2":
        image = slide_base(chapter_title, label, "Resize Mode")
        bullets(image, MODES, active={2, 3})
        return image

    raise SystemExit("No slide for " + step_id)
