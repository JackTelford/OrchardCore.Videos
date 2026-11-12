"""The slides of the Mini Profiler video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"MiniProfilerOptions", "RenderPosition", "MiniProfiler"}

RECORDS = [
    ("1", "Request timings", "How long each request that the tenant handles takes."),
    ("2", "MVC filters and views", "Timed as steps of the request."),
    ("3", "Rendered shapes", "Steps named Shape, followed by the shape type."),
    ("4", "YesSql commands", "Each database command, with its duration."),
]

CARE = [
    ("1", "A specific problem", "Use it to investigate a specific performance problem."),
    ("2", "Added work", "Profiling adds work to every request of an enabled tenant."),
    ("3", "Exposed details", "Results can show route names, SQL statements and timings."),
    ("4", "Limited access", "It should not be broadly available on a production site."),
]

BADGES = [
    ("1", "Hover a badge", "Its tooltip tells what it stands for."),
    ("!", "Duplicate", "The same command text already ran earlier in the request."),
    ("!", "Error", "The command threw an exception."),
]

GAPS = [
    ("1", "<n> ms without queries", "How long the request ran without any query."),
    ("2", "Mostly in <step> (<n> ms)", "The step that spent most of that time."),
    ("!", "Red exclamation mark", "Steps and requests that run duplicate or failing commands."),
]

CONTROLS = [
    ("1", "Widget permissions", "Decide whether the widget is added to a page."),
    ("2", "ResultsAuthorize or ResultsAuthorizeAsync", "Protect the result endpoints. The module sets none."),
    ("3", "ShouldProfile", "Decides whether a request is recorded at all."),
]

MISSING = [
    ("1", "The feature", "Mini Profiler is enabled for the current tenant."),
    ("2", "The permission", "Your role has the front end or back end permission."),
    ("3", "The Footer zone", "The page uses an Orchard layout that renders it."),
    ("4", "Expired results", "Results stay 30 minutes in memory, unshared between instances."),
]

OPTIONS = """
using StackExchange.Profiling;

builder.Services
    .AddOrchardCms()
    .ConfigureServices(tenantServices =>
        tenantServices.Configure<MiniProfilerOptions>(options =>
        {
            options.IgnoredPaths.Add("/health");
            options.PopupRenderPosition = RenderPosition.Right;
        }));
"""

STEP = """
using StackExchange.Profiling;

// ...
using (MiniProfiler.Current?.Step("Load recommendations"))
{
    await recommendationService.LoadAsync();
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "What the Mini Profiler records")
        bullets(image, RECORDS)
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "Use it with care")
        bullets(image, CARE)
        return image

    if step_id == "flags-1":
        image = slide_base(chapter_title, label, "Badges in the queries view")
        bullets(image, BADGES)
        return image

    if step_id == "flags-2":
        image = slide_base(chapter_title, label, "Between the queries")
        bullets(image, GAPS)
        return image

    if step_id == "conf-1":
        image = slide_base(chapter_title, label, "Configure the options in code")
        draw_code(image, (96, 240, 1140, 740), "Program.cs", "csharp", OPTIONS, types=TYPES, size=24)
        note_cards(image, [
            ("IgnoredPaths", "Skips paths such as /health."),
            ("PopupRenderPosition", "Moves the widget."),
            ("Not from configuration", "No section binds the options."),
        ], (1180, 240, W - 96, 1030))
        return image

    if step_id == "conf-2":
        image = slide_base(chapter_title, label, "Three separate controls")
        bullets(image, CONTROLS)
        return image

    if step_id == "custom-1":
        image = slide_base(chapter_title, label, "Add a step to the profile")
        draw_code(image, (96, 240, 1140, 740), "MyService.cs", "csharp", STEP, types=TYPES, size=24)
        note_cards(image, [
            ("Step", "Adds a named step."),
            ("MiniProfiler.Current", "Is null when not profiled."),
        ], (1180, 240, W - 96, 1030))
        return image

    if step_id == "missing-1":
        image = slide_base(chapter_title, label, "When something is missing")
        bullets(image, MISSING)
        return image

    raise SystemExit(f"No slide for {step_id}")
