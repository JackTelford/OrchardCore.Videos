"""The slides of the Error pages video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"HttpErrorShapeViewModel", "HttpErrorAlternatesFactory", "HttpErrorAlternatesCacheKey", "HttpStatusCode",
         "ShapeViewModel", "IStatusCodePagesFeature"}

FLOW = [
    ("1", "An error status", "A request ends with a status such as 404."),
    ("2", "The Error route", "The request runs again as /Error/404."),
    ("3", "The HttpError shape", "The Error action builds a shape named HttpError."),
    ("4", "A template", "The most specific template for the status draws the page."),
]

VIEW_MODEL = """
public HttpErrorShapeViewModel(int? code)
{
    // ...
    Metadata.Type = ShapeType; // "HttpError"

    if (code.HasValue && Enum.TryParse<HttpStatusCode>(Code.ToString(), out var httpStatusCode))
    {
        Metadata.Alternates.AddRange(HttpErrorAlternatesFactory.GetAlternates(code.Value, httpStatusCode));
    }
}
"""

ALTERNATES = """
public static string[] GetAlternates(int code, HttpStatusCode statusCode)
{
    var key = new HttpErrorAlternatesCacheKey(code, statusCode);
    return s_cache.GetOrAdd(key, static k =>
    [
        $"HttpError__{k.Code}",
        $"HttpError__{k.StatusCode}"
    ]);
}
"""

MODULE_VIEWS = """
HttpError__BadRequest         HttpError-BadRequest.cshtml
HttpError__Unauthorized       HttpError-Unauthorized.cshtml
HttpError__Forbidden          HttpError-Forbidden.cshtml
HttpError__NotFound           HttpError-NotFound.cshtml
HttpError__TooManyRequests    HttpError-TooManyRequests.cshtml
HttpError                     HttpError.cshtml
"""

THEME_VIEW = """
<h1>@T["We could not find that page."]</h1>
<p>
    <a href="~/">@T["Back to the home page"]</a>
</p>
"""

MODULE_NOT_FOUND = """
<h1>@T["The page could not be found."]</h1>
"""

LIQUID_VIEW = """
<h1>{{ "We could not find that page." | t }}</h1>
<p>{{ "Check the address and try again." | t }}</p>
"""

FALLBACK_VIEW = """
@model OrchardCore.Diagnostics.ViewModels.HttpErrorShapeViewModel

<h1>@T["An error occurred while executing this request."]</h1>
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Templates",
      "Templates": {
        "HttpError__NotFound": {
          "Description": "The Not Found page",
          "Content": "<h2 class=\\"text-danger\\">{{ \\"We could not find that page.\\" | t }}</h2>"
        }
      }
    }
  ]
}
"""

EXTENSION_FILTER = """
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 400)
    {
        if (_contentTypeProvider.TryGetContentType(context.Request.Path.Value, out var contentType))
        {
            var statusCodePagesFeature = context.Features.Get<IStatusCodePagesFeature>();
            if (statusCodePagesFeature != null)
            {
                statusCodePagesFeature.Enabled = false;
            }
        }
    }
});
"""

EXCEPTION_HANDLER = """
if (!_hostEnvironment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
"""

ERROR_ROUTE = """
routes.MapAreaControllerRoute(
    name: "Diagnostics.Error",
    areaName: "OrchardCore.Diagnostics",
    pattern: "Error/{status?}",
    defaults: new { controller = "Diagnostics", action = "Error" }
);
"""


def slide(step_id, chapter_title, label):
    if step_id == "flow-1":
        image = slide_base(chapter_title, label, "From a status to a page")
        bullets(image, FLOW, active={0, 1})
        return image

    if step_id == "flow-2":
        image = slide_base(chapter_title, label, "The shape and its alternates")
        draw_code(image, (96, 240, W - 96, 650), "HttpErrorShapeViewModel.cs", "csharp", VIEW_MODEL, types=TYPES, size=22)
        draw_code(image, (96, 670, W - 96, 1040), "HttpErrorAlternatesFactory.cs", "csharp", ALTERNATES, types=TYPES, size=22)
        return image

    if step_id == "flow-3":
        image = slide_base(chapter_title, label, "The views of the module")
        draw_code(image, (96, 240, 1110, 600), "OrchardCore.Diagnostics: shape and view", "html", MODULE_VIEWS, size=22)
        note_cards(image, [
            ("Statuses with a view", "400, 401, 403, 404 and 429"),
            ("Any other status", "Uses the HttpError view"),
            ("A status of your own", "HttpError__MethodNotAllowed"),
        ], (1150, 240, W - 96, 1010))
        return image

    if step_id == "theme-1":
        image = slide_base(chapter_title, label, "A view in the theme")
        draw_code(image, (96, 240, 1110, 520), "Theme: Views/HttpError-NotFound.cshtml", "cshtml", THEME_VIEW, types=TYPES, size=24)
        draw_code(image, (96, 560, 1110, 740), "Module: Views/HttpError-NotFound.cshtml", "cshtml", MODULE_NOT_FOUND, types=TYPES, size=24)
        note_cards(image, [
            ("Shape name", "HttpError__NotFound"),
            ("File name", "HttpError-NotFound.cshtml"),
            ("Folder", "Views, in the theme"),
        ], (1150, 240, W - 96, 1010))
        return image

    if step_id == "theme-2":
        image = slide_base(chapter_title, label, "The same page in Liquid")
        draw_code(image, (96, 240, 1110, 470), "Theme: Views/HttpError-NotFound.liquid", "html", LIQUID_VIEW, size=24)
        note_cards(image, [
            ("File name", "HttpError-NotFound.liquid"),
            ("The t filter", "Localizes the text"),
            ("Folder", "Views, in the theme"),
        ], (1150, 240, W - 96, 1010))
        return image

    if step_id == "theme-3":
        image = slide_base(chapter_title, label, "The fallback view")
        draw_code(image, (96, 240, 1110, 500), "Views/HttpError.cshtml", "cshtml", FALLBACK_VIEW, types=TYPES, size=24)
        note_cards(image, [
            ("Code", "The status code, or null"),
            ("HttpStatusCode", "The same status, or null"),
        ], (1150, 240, W - 96, 1010))
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Templates in a recipe")
        draw_code(image, (96, 240, W - 96, 770), "recipe.json", "json", RECIPE, size=24)
        bullets(image, [
            ("check", "All Templates deployment step", "Exports the templates of a site as this recipe step."),
        ], top=830)
        return image

    if step_id == "rank-1":
        image = slide_base(chapter_title, label, "Which name is tried first")
        bullets(image, [
            ("1", "The name of the status", "HttpError__NotFound"),
            ("2", "The number of the status", "HttpError__404, reached only when nothing has the name"),
            ("3", "The fallback", "HttpError"),
        ])
        return image

    if step_id == "rank-2":
        image = slide_base(chapter_title, label, "Which source wins for a name")
        bullets(image, [
            ("1", "A template from the admin", "Design, Templates"),
            ("2", "A view in the active theme", "Views/HttpError-NotFound.cshtml"),
            ("3", "A view in the module", "OrchardCore.Diagnostics, Views/HttpError-NotFound.cshtml"),
        ])
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "No page for a file path")
        draw_code(image, (96, 240, W - 96, 870), "DiagnosticsStartupFilter.cs", "csharp", EXTENSION_FILTER, types=TYPES, size=24)
        bullets(image, [
            ("check", "A known file extension", "A missing image or style sheet keeps its plain status."),
        ], top=895)
        return image

    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "Unhandled exceptions")
        draw_code(image, (96, 240, 1110, 540), "DiagnosticsStartupFilter.cs", "csharp", EXCEPTION_HANDLER, types=TYPES, size=24)
        draw_code(image, (96, 570, 1110, 880), "Startup.cs", "csharp", ERROR_ROUTE, types=TYPES, size=24)
        note_cards(image, [
            ("Other environments", "Exceptions go to /Error"),
            ("Development", "No exception handler"),
        ], (1150, 240, W - 96, 1010))
        return image

    raise SystemExit(f"No slide for {step_id}")
