"""The slides of the Lists in Templates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
Liquid is drawn with the "html" language of draw_code, since brand.py has no Liquid lexer.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"ListPart", "ContainedPart", "ContentPart", "ListPartViewModel", "ContentItem", "ContentTypeDefinition",
         "BuildPartDisplayContext", "IEnumerable"}

LIST_PART = """
public class ListPart : ContentPart;
"""

CONTAINED_PART = """
public class ContainedPart : ContentPart
{
    // The content item id of the list owning this item.
    public string ListContentItemId { get; set; }

    // The content type of the list owning this item.
    public string ListContentType { get; set; }

    // The order of this item in the list.
    public int Order { get; set; }
}
"""

VIEW_MODEL = """
public class ListPartViewModel
{
    // ...
    public ListPart ListPart { get; set; }
    public IEnumerable<ContentItem> ContentItems { get; set; }
    public IEnumerable<ContentTypeDefinition> ContainedContentTypeDefinitions { get; set; }
    public BuildPartDisplayContext Context { get; set; }
    public dynamic Pager { get; set; }
    // ...
}
"""

LIST_PART_TEMPLATE = """
{% for item in Model.ContentItems %}
    {{ item | shape_build_display: "Summary" | shape_render }}
{% endfor %}

<!-- ... -->

{% shape_pager Model.Pager previous_text: previousText, next_text: nextText,
    previous_class: previousClass, next_class: nextClass %}

{{ Model.Pager | shape_render }}
"""

CONTAINER_USE = """
{{ Model.ContentItem | container | display_text }}
<!-- Prints the title of the blog: Blog -->
"""

AUTOROUTE_PATTERN = """
<!-- The Autoroute pattern of the BlogPost type, in the Blog recipe. -->
{{ Model.ContentItem | container | display_text | slugify }}/{{ Model.ContentItem | display_text | slugify }}
"""

ITEMS_USE = """
{% assign posts = Model.ContentItem | list_items %}

{{ Model.ContentItem | list_count }}

<!-- The content item id of a list, as a string, works too. -->
{{ "4fq8m2xe7v1jt9k3n5d0w6ypb2" | list_count }}
"""

POST_TEMPLATE = """
<!-- ... the header and the content of the post ... -->
{% assign blog = Model.ContentItem | container %}

<h3>More in
    <a href="{{ blog | display_url }}">{{ blog | display_text }}</a>
</h3>
<p>{{ blog | list_count }} published posts</p>

{% assign posts = blog | list_items %}
<ul>
{% for post in posts %}
    <li>
        <a href="{{ post | display_url }}">{{ post | display_text }}</a>
    </li>
{% endfor %}
</ul>
"""

RAZOR = """
@model OrchardCore.Lists.ViewModels.ListPartViewModel
@{
    var listId = Model.ListPart.ContentItem.ContentItemId;

    // Published items only.
    var posts = await Orchard.QueryListItemsAsync(listId);
    var count = await Orchard.QueryListItemsCountAsync(listId);

    // A predicate on the content item index replaces that default.
    var withDrafts = await Orchard.QueryListItemsAsync(listId, x => x.Latest);
}
"""

FILTERS = [
    ("1", "container", "From a content item to the list that holds it."),
    ("2", "list_items", "From a list to its published content items."),
    ("3", "list_count", "From a list to the number of its published items."),
]

CONTAINER_RESULTS = [
    ("1", "The item is not in a list", "container returns the item itself."),
    ("2", "The list is not published", "container returns the item itself."),
    ("3", "The input is not a content item", 'container raises an exception: "A Content Item was expected"'),
]

ITEMS_RESULTS = [
    ("check", "Published items only", "An item that exists only as a draft is left out."),
    ("check", "The whole list", "The filters do not page: every published item is counted or returned."),
    ("check", "list_items sorts by Order", "The order stored in the ContainedPart of each item."),
    ("check", "A content item or its id", "The id of the list works as a string."),
]

WHERE = [
    ("1", "Content-BlogPost.liquid", "A file in the Views folder of a theme."),
    ("2", "Content__BlogPost", "A template under Design, Templates. It takes priority over the file, so it holds all of it."),
]


def notes(image, lines, y, step=62):
    """Short notes under a code panel: a green dot and a line of text."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((100, y - 7, 114, y + 7), fill=GREEN)
        draw.text((136, y), line, font=font(32), fill=TEXT, anchor="lm")
        y += step


def slide(step_id, chapter_title, label):
    if step_id == "store-1":
        image = slide_base(chapter_title, label, "Items know their list")
        draw_code(image, (96, 240, W - 96, 420), "ListPart.cs", "csharp", LIST_PART, types=TYPES, size=28)
        draw_code(image, (96, 460, W - 96, 1030), "ContainedPart.cs", "csharp", CONTAINED_PART, types=TYPES, size=28)
        return image

    if step_id == "store-2":
        image = slide_base(chapter_title, label, "Three filters in the Lists module")
        bullets(image, FILTERS)
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "The ListPart shape")
        draw_code(image, (96, 240, W - 96, 760), "ListPartViewModel.cs", "csharp", VIEW_MODEL, types=TYPES, size=28,
                  highlight={5, 8})
        notes(image, ["ContentItems: one page of published items.", "Pager: the links to the other pages."], 850)
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "Blog-ListPart.liquid")
        draw_code(image, (96, 240, 1290, 1030), "Blog-ListPart.liquid", "html", LIST_PART_TEMPLATE, types=TYPES, size=24)
        note_cards(image, [
            ("Applies to", "the Blog content type."),
            ("The loop", "Model.ContentItems"),
            ("The pager", "Model.Pager"),
        ], (1330, 300, W - 96, 1030))
        return image

    if step_id == "container-1":
        image = slide_base(chapter_title, label, "container: from an item to its list")
        draw_code(image, (96, 240, W - 96, 460), "Content__BlogPost", "html", CONTAINER_USE, types=TYPES, size=28)
        draw_code(image, (96, 500, W - 96, 760), "Autoroute pattern", "html", AUTOROUTE_PATTERN, types=TYPES, size=24)
        return image

    if step_id == "container-2":
        image = slide_base(chapter_title, label, "What container returns")
        bullets(image, CONTAINER_RESULTS)
        return image

    if step_id == "items-1":
        image = slide_base(chapter_title, label, "list_items and list_count")
        draw_code(image, (96, 240, W - 96, 800), "Content__Blog", "html", ITEMS_USE, types=TYPES, size=28)
        return image

    if step_id == "items-2":
        image = slide_base(chapter_title, label, "What the two filters return")
        bullets(image, ITEMS_RESULTS)
        return image

    if step_id in ("template-1", "template-2"):
        image = slide_base(chapter_title, label, "Content__BlogPost")
        highlight = {2, 4, 5, 6, 7} if step_id == "template-1" else {9, 10, 11, 12, 13, 14, 15, 16}
        draw_code(image, (96, 240, W - 96, 930), "Content-BlogPost.liquid", "html", POST_TEMPLATE, types=TYPES, size=26,
                  highlight=highlight)
        return image

    if step_id == "template-3":
        image = slide_base(chapter_title, label, "Two places for the template")
        bullets(image, WHERE)
        return image

    if step_id in ("razor-1", "razor-2"):
        image = slide_base(chapter_title, label, "The Orchard helper in Razor")
        highlight = {3, 5, 6, 7} if step_id == "razor-1" else {9, 10}
        draw_code(image, (96, 240, W - 96, 800), "ListPart.cshtml", "csharp", RAZOR, types=TYPES, size=28,
                  highlight=highlight)
        if step_id == "razor-1":
            notes(image, ["Both take the content item id of the list.", "By default, they use published items only."], 880)
        else:
            notes(image, ["x => x.Latest: the Latest property of the index also matches drafts."], 880)
        return image

    raise SystemExit(f"No slide for {step_id}")
