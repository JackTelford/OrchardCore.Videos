"""The slides of the "Localize a Module" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"IStringLocalizer", "IHtmlLocalizer", "IViewLocalizer", "IStringLocalizerFactory", "IHtmlLocalizerFactory",
         "LocalizedString", "LocalizedHtmlString", "LocalizationSource", "LocalizationMetadata", "Permission",
         "MyService", "MyPermissions", "MyMessages", "MyModuleLocalizationSources", "SkipLocalizationExtraction",
         "SkipLocalizationExtractionAttribute"}

HOW = [
    ("1", "The text is the message ID", "The English text in your code is what the translations are looked up by."),
    ("2", "The context is the owner", "The type or the view that asks for the text."),
    ("3", "Without a translation", "The text is shown as written."),
    ("4", "Nothing to register", "Inject a localizer. Without the Localization feature, text is shown as written."),
]

SERVICE = """
using Microsoft.Extensions.Localization;

namespace MyNamespace;

public class MyService
{
    private readonly IStringLocalizer S;

    public MyService(IStringLocalizer<MyService> localizer)
    {
        S = localizer;
    }

    public string Greet(string name) => S["Hello {0}", name];
}
"""

LOCALIZERS = [
    ("S", "IStringLocalizer<T>", "Plain text: labels, messages, errors. Usually kept in a field named S."),
    ("H", "IHtmlLocalizer<T>", "Messages that carry HTML. Its format arguments are HTML encoded."),
    ("T", "IViewLocalizer", "In a Razor view, T. In a Liquid template, the t filter."),
]

VIEW = """
<label class="ocat-label ocat-label-required">@T["Define which cultures are supported by the site."]</label>
"""

SERVICE_CONTEXT = """
namespace MyNamespace;

public class MyService
{
    private readonly IStringLocalizer S;

    public MyService(IStringLocalizer<MyService> localizer)
    {
        S = localizer;
    }
}
"""

SERVICE_PO = """
msgctxt "MyNamespace.MyService"
msgid "Hello {0}"
msgstr "Bonjour {0}"
"""

VIEW_CODE = """
<h1>@T["Welcome"]</h1>
"""

VIEW_PO = """
msgctxt "MyModule.Views.Welcome"
msgid "Welcome"
msgstr "Bienvenue"
"""

SHARED = """
private readonly IStringLocalizer S;
private readonly IStringLocalizer _sharedLocalizer;

public MyService(IStringLocalizerFactory factory)
{
    S = factory.Create(typeof(MyService));
    _sharedLocalizer = factory.Create("MySharedContext", string.Empty);
}
"""

SHARED_PO = """
msgctxt "MySharedContext"
msgid "Hello"
msgstr "Salut"

# Used when no entry has the context.
msgid "Hello"
msgstr "Bonjour"
"""

PLURAL_VIEW = """
@T.Plural(count, "{0} book", "{0} books")
"""

PLURAL_FEATURES = """
await _notifier.SuccessAsync(H.Plural(
    features.Length,
    "The feature {1} was enabled.",
    "The following features were enabled: {1}.",
    string.Join(", ", features.Select(f => f.Name ?? f.Id))));
"""

PLURAL_NOTES = [
    "The count is always placeholder {0}.",
    "The arguments that follow start at {1}.",
]

PLURAL_HEADER = """
msgid ""
msgstr ""
"Language: cs\\n"
"Plural-Forms: nplurals=3; plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2;\\n"
"""

PLURAL_PO = """
msgctxt "MyModule.Views.Books"
msgid "{0} book"
msgid_plural "{0} books"
msgstr[0] "{0} kniha"
msgstr[1] "{0} knihy"
msgstr[2] "{0} knih"
"""

COUNTING = [
    ("1", "Placeholder zero is the count", "To show it, write {0} in the singular form and in the plural form."),
    ("2", "Languages count differently", "Czech has three forms. French reads zero as singular."),
    ("3", "The translation places the count", "Each language puts the number where it needs it."),
]

SOURCE = """
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

public static class MyPermissions
{
    public static readonly Permission ManageWidgets = new(
        "ManageWidgets",
        LocalizationSource.Create(
            "Manage widgets", typeof(MyPermissions)));
}
"""

SOURCE_NOTES = [
    ("The value", "The English text."),
    ("The type", "Supplies the context."),
    ("Nothing else", "No translation, no format."),
]

LOCALIZE = """
LocalizedString text = stringLocalizerFactory.Localize(description);
LocalizedHtmlString html = htmlLocalizerFactory.Localize(description);
"""

LOCALIZE_PLURAL = """
private static readonly LocalizationSource s_item =
    LocalizationSource.Create<MyMessages>("{0} item for {1}");

var result = stringLocalizerFactory.Plural(count, s_item, "{0} items for {1}", ownerName);
"""

LOCALIZE_NOTES = [
    "The factory creates the localizer of the source's type.",
    "Format arguments go to Localize and Plural, not to Create.",
]

MODULE_SOURCES = """
using OrchardCore.Localization;

namespace MyModule;

internal static class MyModuleLocalizationSources
{
    public static readonly LocalizationSource ExportMyData =
        LocalizationSource.Create("Export My Data", typeof(MyModuleLocalizationSources));
}
"""

SOURCES_PO = """
msgctxt "OrchardCore.Localization.CommonLocalizationSources"
msgid "Content Management"
msgstr "Gestion du contenu"

msgctxt "MyModule.MyModuleLocalizationSources"
msgid "Export My Data"
msgstr "Exporter mes données"
"""

NOTES_CODE = """
using static OrchardCore.Localization.LocalizationMetadata;

var greeting = S.Note("Greeting shown on the dashboard.")["Hello {0}", Arg("user name", user.Name)];

// TRANSLATORS: Button that saves the draft.
var save = S["Save draft"];
"""

NOTES_POT = """
#. Greeting shown on the dashboard.
#. Format arguments: {0}: user name
#: Services/Greeter.cs
#, csharp-format
msgctxt "MyModule.Services.Greeter"
msgid "Hello {0}"
msgstr ""

# ...
"""

RULES = [
    ("!", "OCLOC101 · Warning", "A constant LocalizationSource is declared without a context."),
    ("i", "OCLOC102 · Info", "The context is neither the declaring type nor a containing type."),
    ("!", "OCLOC103 · Warning", "The value of a LocalizationSource is not a constant."),
    ("!", "OCLOC104 · Warning", "A localizer key is not a constant."),
]

PROJECT = """
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <ItemGroup>
    <!-- Version: that of your Orchard Core packages. -->
    <PackageReference Include="OrchardCore.Localization.Build"
                      PrivateAssets="all" />
  </ItemGroup>

</Project>
"""

PROJECT_NOTES = [
    ("Each project", "whose strings are extracted."),
    ("Not inherited", "from the application project."),
]

UPDATE_ONE = """
dotnet msbuild src/MyModule/MyModule.csproj -restore -t:UpdateLocalizationCatalog
"""

UPDATE_ALL = """
dotnet build MySolution.slnx -p:UpdateLocalizationCatalogs=true
"""

UPDATE_NOTES = [
    ("dotnet build", "Embeds the checked-in template."),
    ("UpdateLocalizationCatalog", "Rewrites the template of one project."),
    ("UpdateLocalizationCatalogs", "Rewrites those of the projects built."),
]

TEMPLATE = """
msgid ""
msgstr ""
"Project-Id-Version: MyModule\\n"
"MIME-Version: 1.0\\n"
"Content-Type: text/plain; charset=UTF-8\\n"
"Content-Transfer-Encoding: 8bit\\n"
"X-OrchardCore-Catalog-Schema: 1\\n"

#. Greeting shown on the dashboard.
#. Format arguments: {0}: user name
#: Services/Greeter.cs
#, csharp-format
msgctxt "MyModule.Services.Greeter"
msgid "Hello {0}"
msgstr ""

# ...
"""

TEMPLATE_NOTES = [
    "Entries are sorted, with no timestamp and no line number.",
    "A build embeds the file as MyModule.Localization.pot.",
]

READS = [
    ("C#", "Localizer calls and sources", "Indexers, GetString, GetHtml, Plural and LocalizationSource declarations."),
    ("R", "Razor views and pages", "Each one with the context of its view."),
    ("L", "Liquid files", "Where the t filter gets a literal text."),
    ("!", "OCLOC diagnostics", "Unresolved calls are reported. LocalizationStrict makes warnings fail."),
]

SKIP = """
[SkipLocalizationExtraction]
public LocalizedString Forward(IStringLocalizer localizer, string name)
    => localizer[name];
"""

SKIP_NOTES = [
    ("A class or a method", "Its localizer calls are skipped."),
    ("Not inherited", "Derived classes are not skipped."),
    ("Callers", "Calls to it are still extracted."),
]

EXPORT = """
dotnet build MySite/MySite.csproj -p:LocalizationCatalogOutputPath="$PWD/artifacts/localization"
# Each template: artifacts/localization/<TargetFramework>/<AssemblyName>.pot
"""

PLATFORM = [
    ("1", "Upload the templates", "As source files, keeping contexts, plural forms and placeholders."),
    ("2", "Translate", "The platform exports a PO file for each culture."),
    ("3", "Deploy the PO files", "Place them in a location Orchard Core loads."),
]


def dots(image, lines, x, y, step=60):
    """Lines of text, each with a green dot."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((x, y - 7, x + 14, y + 7), fill=GREEN)
        draw.text((x + 36, y), line, font=font(30), fill=TEXT, anchor="lm")
        y += step


def slide(step_id, chapter_title, label):
    if step_id in ("how-1", "how-2"):
        image = slide_base(chapter_title, label, "How a string is found")
        bullets(image, HOW, active={0, 1, 2} if step_id == "how-1" else {3})
        return image

    if step_id == "loc-1":
        image = slide_base(chapter_title, label, "1. Inject IStringLocalizer<T>")
        draw_code(image, (96, 240, W - 96, 900), "MyService.cs", "csharp", SERVICE, types=TYPES, size=26)
        return image

    if step_id == "loc-2":
        image = slide_base(chapter_title, label, "Three localizers")
        bullets(image, LOCALIZERS, active={1, 2})
        draw_code(image, (96, 800, W - 96, 960), "LocalizationSettings.Edit.cshtml", "html", VIEW, size=24)
        return image

    if step_id == "ctx-1":
        image = slide_base(chapter_title, label, "Context of a service")
        draw_code(image, (96, 240, W - 96, 712), "MyService.cs", "csharp", SERVICE_CONTEXT, types=TYPES, size=26)
        draw_code(image, (96, 740, W - 96, 980), "fr.po", "bash", SERVICE_PO, size=30)
        return image

    if step_id == "ctx-2":
        image = slide_base(chapter_title, label, "Context of a view")
        draw_code(image, (96, 240, W - 96, 430), "MyModule/Views/Welcome.cshtml", "html", VIEW_CODE, size=30)
        draw_code(image, (96, 480, W - 96, 720), "fr.po", "bash", VIEW_PO, size=30)
        return image

    if step_id == "ctx-3":
        image = slide_base(chapter_title, label, "Shared contexts, and the fallback")
        draw_code(image, (96, 240, W - 96, 650), "MyService.cs", "csharp", SHARED, types=TYPES, size=26)
        draw_code(image, (96, 680, W - 96, 1030), "fr.po", "bash", SHARED_PO, size=26)
        return image

    if step_id == "plural-1":
        image = slide_base(chapter_title, label, "Plural: a count and two forms")
        draw_code(image, (96, 240, W - 96, 390), "Books.cshtml", "html", PLURAL_VIEW, size=28)
        draw_code(image, (96, 430, W - 96, 730), "AdminController.cs", "csharp", PLURAL_FEATURES, types=TYPES, size=26)
        dots(image, PLURAL_NOTES, 110, 810)
        return image

    if step_id == "plural-2":
        image = slide_base(chapter_title, label, "One translation for each form")
        draw_code(image, (96, 240, W - 96, 520), "cs.po", "bash", PLURAL_HEADER, size=26)
        draw_code(image, (96, 560, W - 96, 940), "cs.po", "bash", PLURAL_PO, size=28)
        return image

    if step_id == "plural-3":
        image = slide_base(chapter_title, label, "Keep the number out of the text")
        bullets(image, COUNTING)
        return image

    if step_id == "src-1":
        image = slide_base(chapter_title, label, "LocalizationSource")
        draw_code(image, (96, 240, 1160, 760), "MyPermissions.cs", "csharp", SOURCE, types=TYPES, size=24)
        note_cards(image, SOURCE_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "src-2":
        image = slide_base(chapter_title, label, "Translate where it is displayed")
        draw_code(image, (96, 240, W - 96, 430), "MyDisplay.cs", "csharp", LOCALIZE, types=TYPES, size=28)
        draw_code(image, (96, 470, W - 96, 760), "MyMessages.cs", "csharp", LOCALIZE_PLURAL, types=TYPES, size=26)
        dots(image, LOCALIZE_NOTES, 110, 840)
        return image

    if step_id == "src-3":
        image = slide_base(chapter_title, label, "Shared sources, and module sources")
        draw_code(image, (96, 240, W - 96, 646), "MyModuleLocalizationSources.cs", "csharp", MODULE_SOURCES, types=TYPES, size=26)
        draw_code(image, (96, 676, W - 96, 1020), "fr.po", "bash", SOURCES_PO, size=26)
        return image

    if step_id == "note-1":
        image = slide_base(chapter_title, label, "Notes for translators")
        draw_code(image, (96, 240, W - 96, 540), "Greeter.cs", "csharp", NOTES_CODE, types=TYPES, size=24)
        draw_code(image, (96, 580, W - 96, 1030), "MyModule.pot", "bash", NOTES_POT, size=26)
        return image

    if step_id == "note-2":
        image = slide_base(chapter_title, label, "Analyzers")
        bullets(image, RULES)
        return image

    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "1. Add the build package")
        draw_code(image, (96, 240, 1160, 640), "MyModule.csproj", "xml", PROJECT, size=22)
        note_cards(image, PROJECT_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "2. Update the template")
        draw_code(image, (96, 240, W - 96, 390), "One project", "bash", UPDATE_ONE, size=26)
        draw_code(image, (96, 430, W - 96, 580), "A solution", "bash", UPDATE_ALL, size=26)
        draw = ImageDraw.Draw(image)
        y = 680
        for title, text in UPDATE_NOTES:
            draw.ellipse((110, y - 7, 124, y + 7), fill=GREEN)
            draw.text((146, y), title, font=font(30, "Bold"), fill=TEXT, anchor="lm")
            draw.text((146, y + 44), text, font=font(28), fill=MUTED, anchor="lm")
            y += 104
        return image

    if step_id == "ext-3":
        image = slide_base(chapter_title, label, "3. Commit the template")
        draw_code(image, (96, 240, W - 96, 890), "MyModule.pot", "bash", TEMPLATE, size=22)
        dots(image, TEMPLATE_NOTES, 110, 940)
        return image

    if step_id == "ext-4":
        image = slide_base(chapter_title, label, "What extraction reads")
        bullets(image, READS)
        return image

    if step_id == "ext-5":
        image = slide_base(chapter_title, label, "Skip what only forwards keys")
        draw_code(image, (96, 240, 1160, 560), "KeyForwarder.cs", "csharp", SKIP, types=TYPES, size=24)
        note_cards(image, SKIP_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "ext-6":
        image = slide_base(chapter_title, label, "Export the templates for translators")
        draw_code(image, (96, 240, W - 96, 400), "Export", "bash", EXPORT, size=24)
        bullets(image, PLATFORM, top=440)
        return image

    raise SystemExit(f"No slide for {step_id}")
