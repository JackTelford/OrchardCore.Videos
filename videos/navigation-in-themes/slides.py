"""The slides of the "Navigation in Themes" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code
comes from the Orchard Core documentation and source (see the notes next to each constant) and is trimmed.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "ActionResult", "BreadcrumbContext", "BreadcrumbItem", "ContentItem", "ExampleBreadcrumbProvider",
    "IBreadcrumbProvider", "LinkMenuItemPart", "Pager", "PagerParameters", "PagerSlim", "TagBuilder", "Task", "ValueTask",
}

# Where the views of a theme go, and how a template is named after a shape. Names from the Navigation and Menu modules.
TREE = """
MyTheme/
  Views/
    Menu.cshtml                  # every menu
    Menu-MainMenu.cshtml         # only the menu titled Main Menu
    Breadcrumb.cshtml            # every breadcrumb trail
    Breadcrumb-MenuEdit.cshtml   # only the trail named MenuEdit
    Pager.cshtml                 # every full pager
    Pager-MainBlog.cshtml        # only the pager with the PagerId MainBlog
"""

SHAPE_AND_TEMPLATE = [
    ("1", "Shape", "A named piece of output, such as a menu, a trail or a pager."),
    ("2", "Template", "A file in Views, named after the shape. A longer name is more specific."),
]

SHAPES = [
    ("1", "Menu", "From the Menu module: Menu, MenuItem and MenuItemLink."),
    ("2", "Breadcrumb", "From the Navigation module: Breadcrumb and BreadcrumbItem."),
    ("3", "Pager", "From the Navigation module: Pager, PagerSlim and Pager_Link."),
]

# TheTheme/Views/_ViewImports.cshtml and TheTheme/Views/Layout.cshtml.
RAZOR_MENU = """
@* In _ViewImports.cshtml *@
@addTagHelper *, OrchardCore.Menu

@* In Layout.cshtml *@
<menu alias="alias:main-menu"
      cache-id="main-menu"
      cache-fixed-duration="00:05:00"
      cache-tag="alias:main-menu"
      cache-context="user.roles" />
"""

# TheBlogTheme/Views/Layout.liquid, with the arguments on their own lines.
LIQUID_MENU = """
{% shape "menu",
    alias: "alias:main-menu",
    cache_id: "main-menu",
    cache_fixed_duration: "00:05:00",
    cache_tag: "alias:main-menu",
    cache_context: "user.roles" %}
"""

RENDER_CARDS = [
    ("1", "alias", "Selects the menu by the alias of its content item."),
    ("2", "cache tag", "The alias is also the cache tag: publishing the menu drops the cached copy."),
]

# TheTheme/Views/Menu.cshtml.
MENU_TEMPLATE = """
@{
    TagBuilder tag = Tag(Model, "ul");
    tag.AddCssClass("navbar-nav");

    foreach (var item in Model.Items)
    {
        tag.InnerHtml.AppendHtml(await DisplayAsync(item));
    }
}

@tag
"""

# TheTheme/Views/MenuItem.cshtml.
MENU_ITEM_TEMPLATE = """
@{
    TagBuilder tag = Tag(Model, "li");
    tag.AddCssClass("nav-item");

    Model.Metadata.Alternates.Clear();
    Model.Classes.Clear();
    Model.Metadata.Type = "MenuItemLink";

    tag.InnerHtml.AppendHtml(await DisplayAsync(Model));

    // ... the sub items, in a nested list
}

@tag
"""

# TheTheme/Views/MenuItemLink-LinkMenuItem.cshtml.
MENU_ITEM_LINK_TEMPLATE = """
@using OrchardCore.ContentManagement
@{
    ContentItem contentItem = Model.ContentItem;
    if (!contentItem.TryGet<LinkMenuItemPart>(out var linkMenuItemPart))
    {
        return;
    }

    TagBuilder tag = Tag(Model, "a");

    var url = linkMenuItemPart.Url;
    // ... a path that starts with / is resolved with Url.Content

    tag.Attributes["href"] = url;
    tag.InnerHtml.Append(contentItem.DisplayText);
}
@tag
"""

ALTERNATES = [
    ("1", "Menu-MainMenu.cshtml", "Only the menu titled Main Menu."),
    ("2", "MenuItem-level-1.cshtml", "Only level 1, the first level below the top, which is level 0."),
    ("3", "MenuItemLink-HtmlMenuItem.cshtml", "Only HTML menu items."),
    ("4", "MenuItemLink-MainMenu-level-1.cshtml", "One menu and one level."),
]

# src/docs/reference/modules/Navigation/README.md
VIEW_IMPORTS = """
@addTagHelper *, OrchardCore.Navigation.Core
"""

TRAIL_VIEW = """
<zone Name="Title">
    <breadcrumb name="RecordsEdit" title="@T["Edit {0}", record.Name]">
        <breadcrumb-item id="Records" action="Index" controller="Record"
                         area="My.Module"
                         permission="@MyPermissions.ManageRecords.Name">
            @T["Records"]
        </breadcrumb-item>
    </breadcrumb>
</zone>
"""

# src/docs/reference/modules/Navigation/README.md
PROVIDER = """
public sealed class ExampleBreadcrumbProvider : IBreadcrumbProvider
{
    public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context)
    {
        if (context.Name.StartsWith("Example", StringComparison.Ordinal)
            && context.Name != "Examples")
        {
            context.Items.Insert(0, new BreadcrumbItem
            {
                Id = "Examples",
                Text = "Examples",
                Url = "~/examples",
            });
        }

        return ValueTask.CompletedTask;
    }
}
"""

REGISTER = """
services.AddBreadcrumbProvider<ExampleBreadcrumbProvider>();
"""

TRAIL_TEMPLATES = [
    ("1", "Breadcrumb.cshtml", "Every trail: the list of nodes and the heading."),
    ("2", "Breadcrumb.DetailAdmin.cshtml", "Only the trails of the admin."),
    ("3", "Breadcrumb.Detail.cshtml", "Only the trails of the front end."),
    ("4", "Breadcrumb-MenuEdit.cshtml", "Only the trail named MenuEdit."),
]

# OrchardCore.Users, Controllers/AdminController.cs.
PAGER_CONTROLLER = """
public async Task<ActionResult> Index(PagerParameters pagerParameters)
{
    // ... users is the query of the list
    var pager = new Pager(pagerParameters, _pagerOptions);

    var count = await users.CountAsync();

    var results = await users
        .Skip(pager.GetStartIndex())
        .Take(pager.PageSize)
        .ListAsync();

    var pagerShape = await _shapeFactory.PagerAsync(pager, count);

    // ... pass pagerShape to the view model
}
"""

# OrchardCore.Lists, Views/ListPart.cshtml.
PAGER_RAZOR = """
@await DisplayAsync(Model.Pager)
"""

# TheBlogTheme/Views/Blog-ListPart.liquid.
PAGER_LIQUID = """
{% assign previousText = "← Newer Posts" | t %}
{% assign nextText = "Older Posts →" | t %}
{% assign previousClass = "previous" | t: note: "CSS class of the pager link to the newer posts, not displayed." %}
{% assign nextClass = "next" | t: note: "CSS class of the pager link to the older posts, not displayed." %}

{% shape_pager Model.Pager previous_text: previousText, next_text: nextText,
    previous_class: previousClass, next_class: nextClass %}

{{ Model.Pager | shape_render }}
"""

# OrchardCore.Lists, Drivers/ListPartDisplayDriver.cs.
PAGER_SLIM = """
var pagerSlim = new PagerSlim(pagerParameters.Before, pagerParameters.After, pageSize);
model.Pager = await _shapeFactory.PagerSlimAsync(pagerSlim);
"""

# TheTheme/Views/Pager.cshtml.
PAGER_TEMPLATE = """
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Links";
}

<nav aria-label="@T["Listing pages"]">
    @await DisplayAsync(Model)
</nav>
"""

# TheTheme/Views/Pager_Next.cshtml.
PAGER_NEXT_TEMPLATE = """
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "Pager_Link";
    // ... Model.Value is a Font Awesome icon
}

@await DisplayAsync(Model)
"""

# TheTheme/Views/Pager_Link.cshtml.
PAGER_LINK_TEMPLATE = """
@{
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "ActionLink";
    Model.Classes.Add("page-link");
    // ... the list item gets the page-item class
}

@await DisplayAsync(Model)
"""

PAGE_SIZE = [
    ("1", "Settings, General", "Page size selection: Allow users to change the page size on listing pages."),
    ("2", "Pager_PageSizeSelector", "The Navigation module renders plain markup for the Items per page selector."),
    ("3", "Your theme", "Add a Pager_PageSizeSelector template to style it, as The Admin Theme does."),
]


def slide(step_id, chapter_title, label):
    if step_id == "shapes-1":
        image = slide_base(chapter_title, label, "A template for each shape")
        draw_code(image, (96, 240, W - 96, 640), "Views", "bash", TREE, types=TYPES, size=28)
        bullets(image, SHAPE_AND_TEMPLATE, top=700)
        return image

    if step_id == "shapes-2":
        image = slide_base(chapter_title, label, "The shapes of navigation")
        bullets(image, SHAPES)
        return image

    if step_id == "render-1":
        image = slide_base(chapter_title, label, "Render a menu by its alias")
        draw_code(image, (96, 240, 936, 660), "Layout.cshtml", "html", RAZOR_MENU, types=TYPES, size=26)
        draw_code(image, (984, 240, W - 96, 660), "Layout.liquid", "html", LIQUID_MENU, types=TYPES, size=26,
                  highlight=set())
        bullets(image, RENDER_CARDS, top=700, active={0})
        return image

    if step_id == "render-2":
        image = slide_base(chapter_title, label, "Render a menu by its alias")
        draw_code(image, (96, 240, 936, 660), "Layout.cshtml", "html", RAZOR_MENU, types=TYPES, size=26,
                  highlight={8})
        draw_code(image, (984, 240, W - 96, 660), "Layout.liquid", "html", LIQUID_MENU, types=TYPES, size=26)
        bullets(image, RENDER_CARDS, top=700, active={1})
        return image

    if step_id == "templates-1":
        image = slide_base(chapter_title, label, "The Menu template")
        draw_code(image, (96, 240, 1250, 800), "Menu.cshtml", "html", MENU_TEMPLATE, types=TYPES, size=28)
        note_cards(image, [
            ("Model.Items", "The first level of items."),
            ("DisplayAsync", "Draws each item in turn."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "templates-2":
        image = slide_base(chapter_title, label, "The MenuItem template")
        draw_code(image, (96, 240, 1250, 840), "MenuItem.cshtml", "html", MENU_ITEM_TEMPLATE, types=TYPES, size=26)
        note_cards(image, [
            ("Metadata.Type", "Shows it as a MenuItemLink."),
            ("Alternates", "Removes the MenuItem ones."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "templates-3":
        image = slide_base(chapter_title, label, "A template for each type of menu item")
        draw_code(image, (96, 240, 1250, 940), "MenuItemLink-LinkMenuItem.cshtml", "html", MENU_ITEM_LINK_TEMPLATE,
                  types=TYPES, size=26)
        note_cards(image, [
            ("Link, content, HTML", "One template for each."),
            ("The address", "Checked before it runs."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "templates-4":
        image = slide_base(chapter_title, label, "Alternates of a menu")
        bullets(image, ALTERNATES)
        return image

    if step_id == "trail-1":
        image = slide_base(chapter_title, label, "Declare the trail in a view")
        draw_code(image, (96, 240, 1250, 370), "_ViewImports.cshtml", "html", VIEW_IMPORTS, types=TYPES, size=26)
        draw_code(image, (96, 410, 1250, 880), "Edit.cshtml", "html", TRAIL_VIEW, types=TYPES, size=26,
                  highlight={1, 2, 8, 9})
        note_cards(image, [
            ("name", "Stable, in PascalCase."),
            ("title", "The current page."),
            ("Title zone", "The admin theme renders it."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "trail-2":
        image = slide_base(chapter_title, label, "Declare the trail in a view")
        draw_code(image, (96, 240, 1250, 370), "_ViewImports.cshtml", "html", VIEW_IMPORTS, types=TYPES, size=26,
                  highlight=set())
        draw_code(image, (96, 410, 1250, 880), "Edit.cshtml", "html", TRAIL_VIEW, types=TYPES, size=26,
                  highlight={3, 4, 5, 6, 7})
        note_cards(image, [
            ("action", "Or url, for a link."),
            ("permission", "Plain text without it."),
            ("Current page", "Always last, no link."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "provider-1":
        image = slide_base(chapter_title, label, "Change a trail with a provider")
        draw_code(image, (96, 240, 1200, 900), "ExampleBreadcrumbProvider.cs", "csharp", PROVIDER, types=TYPES, size=23)
        draw_code(image, (96, 920, W - 96, 1040), "Startup.cs", "csharp", REGISTER, types=TYPES, size=24)
        note_cards(image, [
            ("Items", "The ancestors: add or remove."),
            ("Name", "Check it before editing."),
        ], (1230, 300, W - 96, 900))
        return image

    if step_id == "trailtheme-1":
        image = slide_base(chapter_title, label, "Templates of a trail")
        bullets(image, TRAIL_TEMPLATES)
        return image

    if step_id == "pager-1":
        image = slide_base(chapter_title, label, "Create the pager shape")
        draw_code(image, (96, 240, 1250, 900), "AdminController.cs", "csharp", PAGER_CONTROLLER, types=TYPES, size=26)
        note_cards(image, [
            ("PagerParameters", "Page and page size."),
            ("PagerOptions", "The paging values of the site."),
            ("PagerAsync", "Creates the Pager shape."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "pager-2":
        image = slide_base(chapter_title, label, "Display the pager")
        draw_code(image, (96, 240, W - 96, 360), "ListPart.cshtml", "html", PAGER_RAZOR, types=TYPES, size=26)
        draw_code(image, (96, 385, W - 96, 805), "Blog-ListPart.liquid", "html", PAGER_LIQUID, types=TYPES, size=26)
        draw_code(image, (96, 830, W - 96, 990), "ListPartDisplayDriver.cs", "csharp", PAGER_SLIM, types=TYPES, size=26)
        return image

    if step_id == "pagertheme-1":
        image = slide_base(chapter_title, label, "Templates of the parts of a pager")
        draw_code(image, (96, 240, 760, 600), "Pager.cshtml", "html", PAGER_TEMPLATE, types=TYPES, size=24)
        draw_code(image, (800, 240, W - 96, 600), "Pager_Next.cshtml", "html", PAGER_NEXT_TEMPLATE, types=TYPES,
                  size=24)
        draw_code(image, (96, 640, W - 96, 1010), "Pager_Link.cshtml", "html", PAGER_LINK_TEMPLATE, types=TYPES,
                  size=24)
        return image

    if step_id == "pagertheme-2":
        image = slide_base(chapter_title, label, "Page size selection")
        bullets(image, PAGE_SIZE)
        return image

    raise SystemExit(f"No slide for {step_id}")
