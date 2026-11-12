"""The slides of the Decoupled CMS video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
the code of the decoupled CMS guide of the documentation, trimmed to what the narration talks
about, and the ContentPreviewFeature check used by the Orchard Core source.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IOrchardHelper", "ContentPreviewFeature", "ContentItem", "WebApplication"}

CONTEXT = [
    ("1", "Administration", "Orchard Core: content types, content items, and the tools of the editors."),
    ("2", "Front end", "Your Razor Pages or controllers: the routes, the markup, and the styling."),
    ("3", "Content", "Your pages load content items from Orchard Core, and render them as you choose."),
]

TERMS = [
    ("1", "Content type", "Defines the properties of your content, like a class."),
    ("2", "Content part", "A building block you add to a type, such as Title."),
    ("3", "Content item", "A versioned document based on a type, like an instance."),
]

SUMMARY = [
    ("1", "Add Orchard Core", "The package, AddOrchardCms and UseOrchardCore, and the Blank site recipe."),
    ("2", "Model the content", "A Blog Post type with the Title and MarkdownBody parts."),
    ("3", "Render it with your pages", "GetContentItemByIdAsync, or GetContentItemByHandleAsync with an alias."),
    ("4", "Preview it", "The Preview part points the preview at your own page."),
]

TERMINAL = """
# Create the project
dotnet new webapp -o OrchardSite
cd OrchardSite

# Add Orchard Core
dotnet add package OrchardCore.Application.Cms.Core.Targets
"""

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrchardCms();

var app = builder.Build();

app.UseStaticFiles();
app.UseOrchardCore();

app.Run();
"""

REMOVE = """
// Remove these lines of the template:
builder.Services.AddRazorPages();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
"""

# The page by identifier.
PAGE_BY_ID = """
@page "/blogpost/{id}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByIdAsync(Id);
}

<h1>@blogPost.DisplayText</h1>

@functions
{
    [FromRoute]
    public string Id { get; set; }
}
"""

ADDRESSES = """
# The edit page of the content item
/Admin/Contents/ContentItems/<id>/Edit

# Your page
/blogpost/<id>
"""

CONSOLE_LOG = """
<h1>@blogPost.DisplayText</h1>

@Orchard.ConsoleLog(blogPost)
"""

CONSOLE_OUTPUT = """
{
  "ContentItemId": "<id>",
  "ContentItemVersionId": "<version id>",
  "ContentType": "BlogPost",
  "DisplayText": "Winter Pruning Notes",
  // ...
  "Content": {
    "BlogPost": {},
    "MarkdownBodyPart": {
      "Markdown": "Prune apple trees ..."
    },
    "TitlePart": {
      "Title": "Winter Pruning Notes"
    }
  }
}
"""

# The page by identifier, with the body.
PAGE_WITH_BODY = """
@page "/blogpost/{id}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByIdAsync(Id);
}

<h1>@blogPost.DisplayText</h1>

<p>@await Orchard.MarkdownToHtmlAsync((string) blogPost.Content.MarkdownBodyPart.Markdown)</p>

@functions
{
    [FromRoute]
    public string Id { get; set; }
}
"""

# The page by alias.
PAGE_BY_ALIAS = """
@page "/blogpost/{slug}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByHandleAsync($"alias:{Slug}");
}

<h1>@blogPost.DisplayText</h1>

<p>@await Orchard.MarkdownToHtmlAsync((string) blogPost.Content.MarkdownBodyPart.Markdown)</p>

@functions
{
    [FromRoute]
    public string Slug { get; set; }
}
"""

PREVIEWING = """
@using OrchardCore.ContentPreview

@{
    var previewing = HttpContext.Features.Get<ContentPreviewFeature>()?.Previewing == true;
}

@if (previewing)
{
    <p>This is a preview of the edited content item.</p>
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "One web application")
        bullets(image, CONTEXT, active={0, 1} if step_id == "why-1" else {1, 2})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Three terms")
        bullets(image, TERMS)
        return image

    if step_id == "add-1":
        image = slide_base(chapter_title, label, "Add Orchard Core to a Razor Pages project")
        draw_code(image, (96, 240, 1180, 640), "Terminal", "bash", TERMINAL, types=TYPES, size=24)
        note_cards(image, [
            ("TheAdmin theme", "For the administration."),
            ("Recipes", "Blank site and Headless site."),
            ("No front-end theme", "You write the front end."),
        ], (1230, 240, W - 96, 960))
        return image

    if step_id == "add-2":
        image = slide_base(chapter_title, label, "The bare minimum Program.cs")
        draw_code(image, (96, 240, W - 96, 780), "Program.cs", "csharp", PROGRAM, types=TYPES, size=28, highlight={3, 7, 8})
        return image

    if step_id == "add-3":
        image = slide_base(chapter_title, label, "Remove what the template added")
        draw_code(image, (96, 240, 1250, 880), "Program.cs", "csharp", REMOVE, types=TYPES, size=24)
        note_cards(image, [
            ("Razor Pages", "Registered by Orchard Core."),
            ("Routing", "Set up for every tenant."),
        ], (1300, 240, W - 96, 700))
        return image

    if step_id == "render-1":
        image = slide_base(chapter_title, label, "Load a content item by its identifier")
        draw_code(image, (96, 240, W - 96, 900), "Pages/BlogPost.cshtml", "html", PAGE_BY_ID, types=TYPES, size=26,
                  highlight={1, 3, 6})
        return image

    if step_id == "render-2":
        image = slide_base(chapter_title, label, "Where the identifier comes from")
        draw_code(image, (96, 240, 1010, 820), "Pages/BlogPost.cshtml", "html", PAGE_BY_ID, types=TYPES, size=22,
                  highlight={1, 9})
        draw_code(image, (1050, 240, W - 96, 500), "Addresses", "bash", ADDRESSES, types=TYPES, size=22)
        note_cards(image, [("DisplayText", "The title of the content item.")], (1050, 540, W - 96, 760))
        return image

    if step_id == "render-3":
        image = slide_base(chapter_title, label, "Read the other properties")
        draw_code(image, (96, 240, 900, 500), "Pages/BlogPost.cshtml", "html", CONSOLE_LOG, types=TYPES, size=24)
        draw_code(image, (940, 240, W - 96, 840), "Browser console", "javascript", CONSOLE_OUTPUT, types=TYPES, size=23)
        note_cards(image, [
            ("Content", "A dynamic document."),
            ("In production", "ConsoleLog writes null."),
        ], (96, 540, 900, 960))
        return image

    if step_id == "render-4":
        image = slide_base(chapter_title, label, "Convert the Markdown to HTML")
        draw_code(image, (96, 240, W - 96, 920), "Pages/BlogPost.cshtml", "html", PAGE_WITH_BODY, types=TYPES, size=24,
                  highlight={11})
        return image

    if step_id == "alias-3":
        image = slide_base(chapter_title, label, "Load a content item by its alias")
        draw_code(image, (96, 240, W - 96, 920), "Pages/BlogPost.cshtml", "html", PAGE_BY_ALIAS, types=TYPES, size=24,
                  highlight={1, 6, 16})
        return image

    if step_id == "preview-4":
        image = slide_base(chapter_title, label, "Detect a preview")
        draw_code(image, (96, 240, W - 96, 760), "Pages/BlogPost.cshtml", "html", PREVIEWING, types=TYPES, size=26)
        return image

    if step_id == "sum-1":
        image = slide_base(chapter_title, label, "What you built")
        bullets(image, SUMMARY)
        return image

    raise SystemExit(f"No slide for {step_id}")
