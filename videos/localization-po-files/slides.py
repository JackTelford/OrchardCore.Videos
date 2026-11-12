"""The slides of the "Translate the UI: Install and Write PO Files" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code panels
use the "bash" language for PO files and file trees: it colors the strings and the "#" comments.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IStringLocalizer", "IMyService", "MyService", "Console"}

PO_ENTRY = """
#: Views/Admin/Features.cshtml
msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Search"
msgstr "Rechercher"
"""

FILE_TREE = """
MySite/
  Localization/
    fr/
      OrchardCore.Admin.po
      OrchardCore.AdminMenu.po
      OrchardCore.Alias.po
      OrchardCore.Contents.po
      # ... one file for each module
"""

QUERY = """
/Admin/Features?culture=fr
"""

CMS_TARGET = """
<Target Name="PublishTranslationFiles"
        BeforeTargets="GetCopyToPublishDirectoryItems">
  <ItemGroup>
    <_WebProjectGeneratedContent
        Include="Localization\\**"
        Exclude="$(DefaultItemExcludes)" />
  </ItemGroup>
</Target>
"""

PROJECT_FILE = """
<ItemGroup>
  <Content Include="Localization\\**" >
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
"""

SERVICE = """
using Microsoft.Extensions.Localization;

namespace MyNamespace;

public class MyService : IMyService
{
    private readonly IStringLocalizer S;

    public MyService(IStringLocalizer<MyService> localizer)
    {
        S = localizer;
    }

    public void DoSomething()
    {
        Console.WriteLine(S["Hello"]);
    }
}
"""

SERVICE_ENTRY = """
msgctxt "MyNamespace.MyService"
msgid "Hello"
msgstr "Bonjour"
"""

VIEW = """
@* OrchardCore.Features/Views/Admin/Features.cshtml *@
<input id="search-box" class="form-control" placeholder="@T["Search"]" type="search" autofocus autocomplete="off" />
"""

VIEW_ENTRY = """
msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Search"
msgstr "Rechercher"
"""

NO_CONTEXT = """
# No context: used wherever a context has no entry of its own
msgid "Save"
msgstr "Enregistrer"

# Empty translation: the entry is ignored
msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Name"
msgstr ""
"""

TENANT_TREE = """
MySite/
  Localization/
    fr.po                 # every tenant
    fr/                   # every tenant, loaded last
  App_Data/
    Sites/
      Default/
        Localization/
          fr.po           # the Default tenant only
"""

PLACEHOLDERS = """
msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Showing {0} of {1} features"
msgstr "{0} sur {1} fonctionnalités affichées"
"""

LOCATIONS = [
    ("1", "Module or theme", "<module>/Localization/<culture>.po"),
    ("2", "Site", "Localization/<culture>.po"),
    ("3", "Tenant", "App_Data/Sites/<tenant>/Localization/<culture>.po"),
    ("4", "Split by module", "Localization/<culture>/<module id>.po and Localization/<module id>/<culture>.po"),
]


def slide(step_id, chapter_title, label):
    if step_id == "po-1":
        image = slide_base(chapter_title, label, "The anatomy of a PO entry")
        draw_code(image, (96, 240, 1170, 580), "fr.po", "bash", PO_ENTRY, size=30)
        note_cards(image, [
            ("msgctxt", "Where the text is used"),
            ("msgid", "The original text"),
            ("msgstr", "The translation"),
        ], (1210, 240, W - 96, 0))
        return image

    if step_id == "po-2":
        image = slide_base(chapter_title, label, "One set of files for each culture")
        bullets(image, [
            ("1", "Culture in the file or folder name", "fr.po or fr/<file>.po serves French, and fr-CA.po serves French in Canada."),
            ("2", "Loaded for the request", "Orchard Core reads the files of the culture of the request."),
            ("3", "No translation, no change", "A text without a translation is shown as it is written in the code."),
        ])
        return image

    if step_id == "install-1":
        image = slide_base(chapter_title, label, "Download French from Crowdin")
        bullets(image, [
            ("1", "Select French", "In the Orchard Core project, in the list of languages."),
            ("2", "Download or Upload", "The button at the top right of the page."),
            ("3", "Download translations", "The browser saves a zip file named fr.zip."),
        ])
        return image

    if step_id == "install-2":
        image = slide_base(chapter_title, label, "Extract it into Localization")
        draw_code(image, (96, 240, 1130, 720), "fr.zip, extracted", "bash", FILE_TREE, size=28)
        note_cards(image, [
            ("Localization", "In the root of your site"),
            ("fr", "One folder for each culture"),
            ("PO files", "One for each module"),
        ], (1170, 240, W - 96, 0))
        return image

    if step_id == "install-3":
        image = slide_base(chapter_title, label, "Publish the files")
        draw_code(image, (96, 240, 1250, 630), "OrchardCore.Application.Cms.Core.Targets.targets", "xml", CMS_TARGET, size=24)
        draw_code(image, (96, 660, 1250, 940), "MySite.csproj", "xml", PROJECT_FILE, size=24)
        note_cards(image, [
            ("CMS targets", "Publish Localization"),
            ("Other projects", "Include it as content"),
            ("Docker", "Not under App_Data"),
        ], (1290, 240, W - 96, 0))
        return image

    if step_id == "result-1":
        image = slide_base(chapter_title, label, "Test a culture")
        draw_code(image, (96, 240, 1250, 400), "Address of a page", "bash", QUERY, size=34)
        note_cards(image, [
            ("Query string", "Applies to one request"),
            ("Supported cultures", "fr is listed on Cultures"),
        ], (1290, 240, W - 96, 0))
        return image

    if step_id in ("where-1", "where-2", "where-3"):
        heading = "Later files replace earlier ones" if step_id == "where-3" else "Where Orchard Core looks"
        image = slide_base(chapter_title, label, heading)
        active = {"where-1": {0, 1, 2}, "where-2": {3}, "where-3": None}[step_id]
        bullets(image, LOCATIONS, active=active)
        return image

    if step_id == "where-4":
        image = slide_base(chapter_title, label, "Files and cultures")
        bullets(image, [
            ("1", "fr.po or fr/", "Serves the culture fr."),
            ("2", "fr-CA.po or fr-CA/", "Serves the culture fr-CA."),
            ("3", "Fall back to parent culture(s)", "A text missing from the fr-CA files is looked up in the fr files."),
        ])
        return image

    if step_id == "write-1":
        image = slide_base(chapter_title, label, "The context of a service")
        draw_code(image, (96, 240, 976, 960), "MyService.cs", "csharp", SERVICE, types=TYPES, size=22)
        draw_code(image, (1016, 240, W - 96, 520), "Localization/fr.po", "bash", SERVICE_ENTRY, size=26)
        note_cards(image, [("Context", "The full name of the type")], (1016, 560, W - 96, 0))
        return image

    if step_id == "write-2":
        image = slide_base(chapter_title, label, "The context of a view")
        draw_code(image, (96, 240, W - 96, 420), "Features.cshtml", "html", VIEW, size=22)
        draw_code(image, (96, 460, 1150, 800), "Localization/fr.po", "bash", VIEW_ENTRY, size=28)
        note_cards(image, [
            ("Path of the view", "Starts at the module folder"),
            ("Context", "Dots, no extension"),
        ], (1190, 460, W - 96, 0))
        return image

    if step_id == "write-3":
        image = slide_base(chapter_title, label, "Entries without a context")
        draw_code(image, (96, 240, 1250, 760), "Localization/fr.po", "bash", NO_CONTEXT, size=28)
        note_cards(image, [
            ("Lookup order", "Context first, then none"),
            ("Empty translation", "The entry is ignored"),
        ], (1290, 240, W - 96, 0))
        return image

    if step_id == "write-4":
        image = slide_base(chapter_title, label, "One tenant only")
        draw_code(image, (96, 240, 1130, 760), "Site folders", "bash", TENANT_TREE, size=28)
        note_cards(image, [
            ("Tenant folder", "Read by that tenant"),
            ("Site folder", "Read by every tenant"),
            ("Culture folder", "Loaded last, wins"),
        ], (1170, 240, W - 96, 0))
        return image

    if step_id == "write-5":
        image = slide_base(chapter_title, label, "Placeholders, and restarting")
        draw_code(image, (96, 240, 1250, 580), "Localization/fr.po", "bash", PLACEHOLDERS, size=30)
        note_cards(image, [
            ("Placeholders", "Keep {0} and {1}"),
            ("Restart", "After you edit a PO file"),
        ], (1290, 240, W - 96, 0))
        return image

    raise SystemExit(f"No slide for {step_id}")
