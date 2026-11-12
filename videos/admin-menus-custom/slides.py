"""The slides of the Admin Menus video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# The two ideas, as the Admin Menu module names them.
IDEAS = [
    ("1", "Admin menu", "A named set of nodes. You can have several, and switch each one on or off."),
    ("2", "Admin node", "One entry of a menu. A node can hold other nodes."),
    ("3", "Merged into the admin menu", "Enabled menus appear next to the standard entries. Only the admin changes."),
]

# The kinds of node the Add Node dialog offers, with the names it shows.
NODE_KINDS = [
    ("1", "Link Admin Node", "Opens a page of the admin, or any other address."),
    ("2", "Placeholder Admin Node", "A menu item that holds other nodes."),
    ("3", "Content Types", "One link for each content type you choose. Comes with the Contents module."),
    ("4", "Lists", "One link for each item that holds a list, such as each blog. Comes with the Lists module."),
]

# Two ways to move menus to another site.
REUSE = [
    ("1", "Deployment plan", "The Admin Menus step exports every admin menu."),
    ("2", "Recipe", "A recipe can create menus when a site is set up, as the Blog recipe does."),
]


def slide(step_id, chapter_title, label):
    if step_id == "ideas-1":
        image = slide_base(chapter_title, label, "Two ideas")
        bullets(image, IDEAS)
        return image

    if step_id == "ideas-2":
        image = slide_base(chapter_title, label, "Four kinds of node")
        bullets(image, NODE_KINDS)
        return image

    if step_id == "reuse-1":
        image = slide_base(chapter_title, label, "Reuse your menus on other sites")
        bullets(image, REUSE)
        return image

    raise SystemExit("No slide for " + step_id)
