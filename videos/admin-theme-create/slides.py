"""The slides of the "Create an Orchard Core admin theme" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Theme", "Controller", "IActionResult", "MyCustomAdminController"}

HOW = [
    ("1", "Two current themes", "A site theme renders the pages visitors see, an admin theme renders the admin."),
    ("2", "The Admin attribute", "It marks a request as admin: the request renders with the admin theme."),
    ("3", "A base theme", "Templates of your theme replace the same templates of TheAdmin."),
]

COMMANDS = """
# In an empty folder named MyAdminTheme
dotnet new octheme

# In the folder that holds your solution and MySite
dotnet sln add MyAdminTheme/MyAdminTheme.csproj
dotnet add MySite/MySite.csproj reference MyAdminTheme/MyAdminTheme.csproj
"""

MANIFEST = """
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyAdminTheme",
    Author = "My name",
    Website = "https://example.com",
    Version = "0.0.1",
    Description = "My Orchard Core Admin theme.",
    Tags = new [] { "admin" },
    BaseTheme = "TheAdmin"
)]
"""

TEMPLATE_FILES = """
MyAdminTheme/
  Manifest.cs
  MyAdminTheme.csproj
  Views/
    Layout.liquid    # delete this file
"""

VIEW_IMPORTS = """
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>

@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
"""

BRANDING = """
<style asp-src="~/MyAdminTheme/Styles/theme.css"
       depends-on="TheAdminLayout,bootstrap,the-admin"
       at="Head"></style>

<a class="ta-navbar-brand navbar-brand"
   asp-route-area="OrchardCore.Admin"
   asp-controller="Admin" asp-action="Index">
    <span>@Site.SiteName</span>
</a>
"""

THEME_FILES = """
MyAdminTheme/
  Views/
    AdminBranding.cshtml
    _ViewImports.cshtml
  wwwroot/
    Styles/
      theme.css
"""

OCAT = [
    ("1", "ocat-wrapper", "Wraps a field: its label, its input and its hint."),
    ("2", "ocat-label", "The label of the field."),
    ("3", "ocat-end", "Holds the input and its hint."),
    ("4", "ocat-limited-wrapper, ocat-limited", "Fields of limited width: dropdowns and numbers."),
]

CSS = """
.ocat-wrapper {
    --ocat-gutter-x: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    margin-right: calc(-0.5 * var(--ocat-gutter-x));
    margin-left: calc(-0.5 * var(--ocat-gutter-x));
    margin-bottom: 1rem;
}

.ocat-label {
    flex: 0 0 auto;
    width: 25%;
    /* ... */
    text-align: end;
}

.ocat-end {
    flex: 0 0 auto;
    width: 75%;
}

/* ... */
"""

MORE = [
    ("1", "Templates of TheAdmin", "Layout, Breadcrumb, Pager and Message are among them."),
    ("2", "Placement", "A placement file in your theme changes where parts of admin pages go."),
]

HINT = """
<button type="button"
        class="btn btn-link p-0 align-baseline"
        data-bs-toggle="tooltip"
        data-bs-placement="top"
        title="@Model.Hint"
        aria-label="@Model.Hint">
    <i class="fa-regular fa-circle-question" aria-hidden="true"></i>
</button>
"""

CONTROLLER = """
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

[Admin]
public sealed class MyCustomAdminController : Controller
{
    // All actions in this controller will render using the Admin Theme
    public IActionResult Index()
    {
        return View();
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("how-1", "how-2", "how-3"):
        image = slide_base(chapter_title, label, "Two themes, one base")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image

    if step_id == "create-1":
        image = slide_base(chapter_title, label, "1. Create the project")
        draw_code(image, (96, 240, W - 96, 700), "Terminal", "bash", COMMANDS, size=28)
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "2. Add the admin tag and the base theme")
        draw_code(image, (96, 240, 1150, 830), "Manifest.cs", "csharp", MANIFEST, types=TYPES, highlight={9, 10}, size=27)
        note_cards(image, [
            ("Tags", "Offers it as an admin theme."),
            ("BaseTheme", "TheAdmin supplies the rest."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "3. Delete the Layout of the template")
        draw_code(image, (96, 240, 1150, 660), "The theme project", "bash", TEMPLATE_FILES, highlight={5}, size=28)
        note_cards(image, [
            ("Layout.liquid", "Would replace the admin layout."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "template-1":
        image = slide_base(chapter_title, label, "A view imports file")
        draw_code(image, (96, 240, W - 96, 640), "Views/_ViewImports.cshtml", "cshtml", VIEW_IMPORTS, size=28)
        return image

    if step_id == "template-2":
        image = slide_base(chapter_title, label, "Override AdminBranding")
        draw_code(image, (96, 240, W - 96, 720), "Views/AdminBranding.cshtml", "cshtml", BRANDING,
                  highlight={5, 6, 7, 8, 9}, size=28)
        return image

    if step_id == "template-3":
        image = slide_base(chapter_title, label, "Load your stylesheet")
        draw_code(image, (96, 240, W - 96, 632), "Views/AdminBranding.cshtml", "cshtml", BRANDING,
                  highlight={1, 2, 3}, size=24)
        draw_code(image, (96, 665, 900, 1040), "The theme project", "bash", THEME_FILES, size=24)
        note_cards(image, [
            ("Rendered first", "The layout renders branding early."),
        ], (960, 665, W - 96, 1040))
        return image

    if step_id == "template-4":
        image = slide_base(chapter_title, label, "Order the stylesheet after the admin styles")
        draw_code(image, (96, 240, 1150, 720), "Views/AdminBranding.cshtml", "cshtml", BRANDING,
                  highlight={2}, size=28)
        note_cards(image, [
            ("depends-on", "The styles of the admin layout."),
            ("Your file", "Loads after them, so it can override."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id in ("forms-1", "forms-2"):
        image = slide_base(chapter_title, label, "The ocat classes")
        bullets(image, OCAT, active={0, 1, 2} if step_id == "forms-1" else {3})
        return image

    if step_id == "forms-3":
        image = slide_base(chapter_title, label, "A horizontal form layout")
        draw_code(image, (96, 240, W - 96, 1030), "Styles/theme.css", "html", CSS, size=23)
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "What else a theme can replace")
        bullets(image, MORE)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "Views/Hint.Admin.cshtml")
        draw_code(image, (96, 240, W - 96, 700), "Views/Hint.Admin.cshtml", "cshtml", HINT, size=28)
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "Pages of your own modules")
        draw_code(image, (96, 240, W - 96, 800), "MyCustomAdminController.cs", "csharp", CONTROLLER,
                  types=TYPES, size=28)
        return image

    raise SystemExit(f"No slide for {step_id}")
