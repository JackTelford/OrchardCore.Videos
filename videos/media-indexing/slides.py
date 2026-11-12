"""The slides of the media indexing video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# What a search index holds.
HOLDS = [
    ("1", "Content item text", "The title and body of a content item go into the search index."),
    ("2", "Attached file text", "The words inside an attached PDF, Word or text file do not."),
]

# The three features that read files.
FEATURES = [
    ("check", "PDF Media Indexing", "Reads PDF files: .pdf"),
    ("check", "Text Media Indexing", "Reads text and Markdown files: .txt and .md"),
    ("check", "OpenXML Media Indexing", "Reads Word and PowerPoint files: .docx and .pptx"),
]

# What has to be in place.
CONDITIONS = [
    ("1", "A Media Field", "The file is attached to a content item through a Media Field."),
    ("2", "The field in the index", "The field has the setting Include this element in the index."),
    ("3", "An index of the content type", "A search provider indexes the content type: this site uses Lucene."),
    ("!", "Not indexed", "Files that only sit in the Media Library, with no Media Field."),
]

# Limits of the file formats.
LIMITS = [
    ("1", "Other formats", "Word files in the doc format and Excel workbooks can be attached, but no feature reads them."),
    ("2", "Text and Markdown", "The site must allow .txt and .md files in its Media configuration before they can be attached."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "What a search index holds")
        bullets(image, HOLDS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Three features read the files")
        bullets(image, FEATURES)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What has to be in place")
        bullets(image, CONDITIONS)
        return image

    if step_id == "notes-1":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, LIMITS, active={0})
        return image

    if step_id == "notes-2":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, LIMITS, active={1})
        return image

    raise SystemExit(f"No slide for {step_id}")
