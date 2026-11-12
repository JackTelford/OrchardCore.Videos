"""The slides of the Sitemap Indexes and the Sitemaps Cache video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHY = [
    ("1", "A size limit", "Search engines limit how large one sitemap can be."),
    ("2", "Several sitemaps", "Split by content type, or use Limit Items with Skip and Take."),
    ("3", "A sitemap of sitemaps", "The index lists the address of each sitemap it contains."),
    ("4", "One address", "A search engine needs only the address of the index."),
]

HOW = [
    ("1", "Built on request", "A sitemap is built the first time someone requests it."),
    ("2", "Stored as a file", "The file name is the sitemap name followed by an identifier."),
    ("3", "A changed identifier", "Saving a sitemap, or publishing content it lists, changes it."),
    ("4", "Old files stay", "An unused file stays on disk until it is purged or cleaned up."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "Why a sitemap index")
        bullets(image, WHY, active={0, 1} if step_id == "why-1" else {2, 3})
        return image

    if step_id in ("how-1", "how-2"):
        image = slide_base(chapter_title, label, "How the Sitemaps Cache works")
        bullets(image, HOW, active={0, 1} if step_id == "how-1" else {2, 3})
        return image

    raise SystemExit("No slide for " + step_id)
