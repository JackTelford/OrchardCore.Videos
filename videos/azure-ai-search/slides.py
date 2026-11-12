"""The slides of the Azure AI Search video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT_IT_DOES = [
    ("1", "Creates indexes", "Index profiles in Orchard Core create the indexes in the search service."),
    ("2", "Sends content", "Content items are sent to the indexes when they change."),
    ("3", "Answers searches", "With the Search module, the results come from the service."),
]

WHAT_YOU_NEED = [
    ("1", "A search service", "An Azure AI Search service in Azure, which holds the indexes."),
    ("2", "Its endpoint", "The address of the service. You enter it in Endpoint."),
    ("3", "A way to sign in", "A key issued by the service, or an identity of the host."),
]

AUTHENTICATION_TYPES = [
    ("1", "Default", "Uses the Azure credentials available to the host."),
    ("2", "Managed Identity", "Uses the identity of the host. Identity Client Id picks a user-assigned one."),
    ("3", "API Key", "Uses a key issued by the search service."),
]

INDEX_NAME = [
    ("1", "default", "The name of your tenant, in lowercase."),
    ("2", "blog-posts", "The Index Name you choose."),
    ("3", "default-blog-posts", "The name of the index in the search service."),
]

INDEX_CONTENT = [
    ("1", "Content item id", "Every document carries the id of its content item."),
    ("2", "Full-text field", "Built by default from the display text and the body of the item."),
    ("3", "Included elements", "Parts and fields that are checked in Azure AI Search Index Settings."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "What the module does")
        bullets(image, WHAT_IT_DOES)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What you need from Azure")
        bullets(image, WHAT_YOU_NEED)
        return image

    if step_id == "auth-1":
        image = slide_base(chapter_title, label, "Three authentication types")
        bullets(image, AUTHENTICATION_TYPES)
        return image

    if step_id == "name-1":
        image = slide_base(chapter_title, label, "How an index is named in the service")
        bullets(image, INDEX_NAME)
        return image

    if step_id == "holds-1":
        image = slide_base(chapter_title, label, "What goes into an index")
        bullets(image, INDEX_CONTENT, active={0, 1})
        return image

    if step_id == "holds-2":
        image = slide_base(chapter_title, label, "What goes into an index")
        bullets(image, INDEX_CONTENT, active={2})
        return image

    raise SystemExit(f"No slide for {step_id}")
