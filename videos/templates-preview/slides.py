"""The slides of the Template preview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the Templates module of Orchard Core and trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, slide_base

TYPES = {"IActionResult", "IShapeBindingResolver", "ShapeBinding", "Task", "Template", "TemplateViewModel",
         "TemplatesDocument"}

WHY = [
    ("1", "A name", "Name a template after a shape or one of its alternates, such as Content__BlogPost."),
    ("2", "Saved", "Once saved, it is used instead of a template file with the same name."),
    ("3", "Previewed", "The Preview button renders a page with the template as typed, and stores nothing."),
]

LIMITS = [
    ("1", "Permissions", "Manage templates and Manage Liquid templates. Administrator and Editor have both."),
    ("2", "Pages", "A home page that is a content item, or any published content item with an Autoroute address."),
    ("3", "Shapes", "A preview template applies to every shape of that name, unless a more specific template matches first."),
    ("4", "Admin templates", "The Edit Admin Template page has no Preview button."),
]

# template-edit.ts: the editor tab.
EDITOR = """
const sendFormData = () => {
    const formData = {
        Name: nameInput.value,
        Content: editor.getValue(),
        // ...
    };
    localStorage.setItem(
        "OrchardCore.templates",
        JSON.stringify(toQueryString(formData)));
};

editor.getModel()?.onDidChangeContent(() => {
    sendFormData();
});
"""

# template-preview.ts: the preview tab.
LISTENER = """
window.addEventListener("storage", (ev) => {
    if (ev.key === "OrchardCore.templates:not-connected") {
        // ...
    } else if (ev.key === "OrchardCore.templates") {
        // ...
        previewEventTimer = setTimeout(
            () => renderPreview(ev.newValue as string), 150);
    }
});
"""

# PreviewController.cs
RENDER = """
[HttpPost]
public async Task<IActionResult> Render()
{
    // ...
    var name = Request.Form["Name"];
    var content = Request.Form["Content"];

    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
    {
        HttpContext.Items["OrchardCore.PreviewTemplate"] =
            new TemplateViewModel { Name = name, Content = content };
    }

    // The home route, or the Autoroute address in Handle, gives the contentItem.
    // ...
    var model = await _contentItemDisplayManager.BuildDisplayAsync(
        contentItem, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.Detail);

    return View(model);
}
"""

# PreviewTemplatesProvider.cs
PROVIDER = """
if (httpContext.Items.TryGetValue("OrchardCore.PreviewTemplate", out var model))
{
    // ...
    var template = new Template { Content = viewModel.Content };
    templatesDocument.Templates.Add(viewModel.Name, template);
}
"""

# TemplatesShapeBindingResolver.cs
RESOLVER = """
public Task<ShapeBinding> GetShapeBindingAsync(string shapeType)
{
    // ...
    if (_localTemplates?.Templates?.TryGetValue(shapeType, out var localTemplate) == true)
    {
        return Task.FromResult(BuildShapeBinding(shapeType, localTemplate));
    }

    return GetShapeBindingAsyncCore(shapeType);
}
"""


def panel_height(code, size):
    """The height of a code panel for the lines of code: the window bar, the lines and a bottom margin."""
    return 74 + len(code.strip("\n").split("\n")) * int(size * 1.42) + 28


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "Templates are matched by name")
        bullets(image, WHY, active={0, 1} if step_id == "why-1" else {2})
        return image

    if step_id == "how-1":
        image = slide_base(chapter_title, label, "1. The editor tells the preview tab")
        middle = W // 2
        draw_code(image, (96, 240, middle - 12, 240 + panel_height(EDITOR, 22)), "template-edit.ts: the editor tab",
                  "javascript", EDITOR, types=TYPES, size=22)
        draw_code(image, (middle + 12, 240, W - 96, 240 + panel_height(LISTENER, 22)),
                  "template-preview.ts: the preview tab", "javascript", LISTENER, types=TYPES, size=22)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "2. The Render action builds the page")
        draw_code(image, (96, 240, W - 96, 240 + panel_height(RENDER, 23)), "PreviewController.cs", "csharp", RENDER,
                  types=TYPES, size=23)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "3. The preview template is checked first")
        top = 240 + panel_height(PROVIDER, 23)
        draw_code(image, (96, 240, W - 96, top), "PreviewTemplatesProvider.cs", "csharp", PROVIDER, types=TYPES, size=23)
        draw_code(image, (96, top + 24, W - 96, top + 24 + panel_height(RESOLVER, 23)),
                  "TemplatesShapeBindingResolver.cs", "csharp", RESOLVER, types=TYPES, size=23)
        return image

    if step_id in ("limits-1", "limits-2", "limits-3", "limits-4"):
        image = slide_base(chapter_title, label, "What to know about the preview")
        bullets(image, LIMITS, active={int(step_id[-1]) - 1})
        return image

    raise SystemExit(f"No slide for {step_id}")
