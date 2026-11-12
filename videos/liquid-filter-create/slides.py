"""The slides of the "Write a Liquid Filter" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IServiceCollection", "TemplateOptionsBuilder", "StringValue", "FluidValue",
         "FilterArguments", "LiquidTemplateContext", "ILiquidFilter", "ISiteService", "WithSiteNameFilter",
         "ValueTask", "NilValue", "DateTimeOffset", "DateTimeStyles"}

SLUGIFY_TEMPLATE = """
{{ "This is some text" | slugify }}
{{ "This is some text" | slugify | append: ".html" }}
"""

SLUGIFY_OUTPUT = """
this-is-some-text
this-is-some-text.html
"""

STARTUP_DELEGATE = """
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptionsBuilder>(options =>
        {
            // A filter without dependencies.
            options.AddFilter("shout", (input, arguments, context) =>
                StringValue.Create(input.ToStringValue(context).ToUpperInvariant()));
        });
    }
}
"""

SHOUT_TEMPLATE = """
{{ "hello" | shout }}
"""

SHOUT_OUTPUT = """
HELLO
"""

FILTER_CLASS = """
public class WithSiteNameFilter : ILiquidFilter
{
    private readonly ISiteService _siteService;

    public WithSiteNameFilter(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        // The first argument is the separator, " | " when it is missing.
        var separator = arguments.At(0).IsNil() ? " | " : arguments.At(0).ToStringValue(context);

        return StringValue.Create(input.ToStringValue(context) + separator + site.SiteName);
    }
}
"""

REGISTRATION = """
public override void ConfigureServices(IServiceCollection services)
{
    services.Configure<TemplateOptionsBuilder>(options =>
    {
        options.AddFilter("shout", (input, arguments, context) =>
            StringValue.Create(input.ToStringValue(context).ToUpperInvariant()));
    });

    // Added as a scoped service, and resolved each time the filter runs.
    services.AddLiquidFilter<WithSiteNameFilter>("with_site_name");
}
"""

SITE_NAME_TEMPLATE = """
{{ "About" | with_site_name }}
{{ "About" | with_site_name: " - " }}
"""

SITE_NAME_OUTPUT = """
About | Orchard Core
About - Orchard Core
"""

ENCODED = """
// ...
return StringValue.Create(text);
"""

NOT_ENCODED = """
// ...
return StringValue.Create(html, encode: false);
"""

NIL = """
// ...
if (!DateTimeOffset.TryParse(stringValue, ctx.Options.CultureInfo, DateTimeStyles.AssumeUniversal, out value))
{
    return NilValue.Instance;
}
"""

WHERE = [
    ("1", "TemplateOptionsBuilder", "A module configures it in ConfigureServices."),
    ("2", "TemplateOptions", "Built once for the tenant, the first time it is needed, and immutable."),
    ("3", "Liquid templates", "Templates, workflows and queries of the tenant read the same options."),
]

ENCODING_NOTES = [
    ("1", "By default", "A string that a filter returns is HTML encoded when the template writes it."),
    ("2", "With encode: false", "The output is written as it is: use it for markup that you trust."),
]

OPTIONS_NOTES = [
    ("check", "Configure the builder", "services.Configure<TemplateOptionsBuilder>(...) or services.AddLiquidFilter<T>(\"name\")."),
    ("!", "Not the options", "A registration that configures TemplateOptions makes the tenant throw when they are first used."),
    ("i", "The message names the builder", "Configuring 'TemplateOptions' isn't supported as it is immutable, configure 'TemplateOptionsBuilder' instead."),
]

MORE = [
    ("1", "Global values", "WithGlobalValue adds a value that is available in all the templates."),
    ("2", "Value converters", "AddValueConverter is called when a CLR object is converted to a Liquid value."),
    ("3", "Member access", "ConfigureMemberAccess computes a member on demand, which doesn't have to exist on the type."),
    ("4", "Tags", "Registered on the LiquidViewParser, with LiquidViewOptions.LiquidViewParserConfiguration."),
]


def template_and_output(image, template, output, size=32):
    """The Liquid of a template, and what it writes, in two panels."""
    draw_code(image, (96, 240, W - 96, 240 + 74 + 46 * len(template.strip().splitlines()) + 20), "template.liquid", "html",
              template, size=size)
    top = 240 + 74 + 46 * len(template.strip().splitlines()) + 20 + 50
    draw_code(image, (96, top, W - 96, top + 74 + 46 * len(output.strip().splitlines()) + 20), "Output", "html",
              output, size=size)


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "How a filter is called")
        draw_code(image, (96, 240, 1190, 470), "template.liquid", "html", SLUGIFY_TEMPLATE, size=28)
        draw_code(image, (96, 510, 1190, 740), "Output", "html", SLUGIFY_OUTPUT, size=28)
        note_cards(image, [
            ("Input", "The value left of the pipe."),
            ("Arguments", "The values after a colon."),
            ("Output", "The value the filter returns."),
        ], (1230, 240, W - 96, 1030))
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "Where filters are registered")
        bullets(image, WHERE)
        return image

    if step_id == "delegate-1":
        image = slide_base(chapter_title, label, "A filter without dependencies")
        draw_code(image, (96, 240, W - 96, 980), "Startup.cs", "csharp", STARTUP_DELEGATE, types=TYPES, size=26,
                  highlight={10, 13})
        return image

    if step_id == "delegate-2":
        image = slide_base(chapter_title, label, "A filter without dependencies")
        draw_code(image, (96, 240, W - 96, 980), "Startup.cs", "csharp", STARTUP_DELEGATE, types=TYPES, size=26,
                  highlight={14})
        return image

    if step_id == "delegate-3":
        image = slide_base(chapter_title, label, "Calling shout")
        template_and_output(image, SHOUT_TEMPLATE, SHOUT_OUTPUT)
        return image

    if step_id == "class-1":
        image = slide_base(chapter_title, label, "A filter with dependencies")
        draw_code(image, (96, 240, W - 96, 990), "WithSiteNameFilter.cs", "csharp", FILTER_CLASS, types=TYPES, size=24,
                  highlight={1, 3, 5, 7, 10})
        return image

    if step_id == "class-2":
        image = slide_base(chapter_title, label, "A filter with dependencies")
        draw_code(image, (96, 240, W - 96, 990), "WithSiteNameFilter.cs", "csharp", FILTER_CLASS, types=TYPES, size=24,
                  highlight={10, 12, 14, 15})
        return image

    if step_id == "class-3":
        image = slide_base(chapter_title, label, "Registering the filter")
        draw_code(image, (96, 240, W - 96, 800), "Startup.cs", "csharp", REGISTRATION, types=TYPES, size=28,
                  highlight={9, 10})
        return image

    if step_id == "class-4":
        image = slide_base(chapter_title, label, "Calling with_site_name")
        template_and_output(image, SITE_NAME_TEMPLATE, SITE_NAME_OUTPUT)
        return image

    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "Encoding")
        draw_code(image, (96, 240, 940, 440), "Encoded", "csharp", ENCODED, types=TYPES, size=28)
        draw_code(image, (980, 240, W - 96, 440), "Not encoded", "csharp", NOT_ENCODED, types=TYPES, size=28)
        bullets(image, ENCODING_NOTES, top=520)
        return image

    if step_id == "rules-2":
        image = slide_base(chapter_title, label, "Nil")
        draw_code(image, (96, 240, W - 96, 560), "LocalTimeZoneFilter.cs", "csharp", NIL, types=TYPES, size=26)
        note_cards(image, [
            ("Return nil", "when the input can't be used."),
            ("A nil value", "writes nothing."),
        ], (96, 610, W - 96, 1030))
        return image

    if step_id == "rules-3":
        image = slide_base(chapter_title, label, "The options are immutable")
        bullets(image, OPTIONS_NOTES)
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "More from the same builder")
        bullets(image, MORE)
        return image

    raise SystemExit(f"No slide for {step_id}")
