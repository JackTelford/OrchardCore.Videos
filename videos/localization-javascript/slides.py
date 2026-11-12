"""The slides of the JavaScript Localization video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is the code of the CORS module of Orchard Core (OrchardCore.Cors), trimmed with "// ...", and the
localizations.ts helper that its script imports.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IJSLocalizer", "IStringLocalizer", "CorsJSLocalizer", "IDictionary", "Dictionary", "StringComparison",
         "IServiceCollection", "HTMLElement", "Record"}

LOCALIZER = """
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

public sealed class CorsJSLocalizer(IStringLocalizer<CorsJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "cors-admin", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "Search", S["Search"].Value },
                { "AddPolicy", S["Add a policy"].Value },
                { "DefaultPolicy", S["Default Policy"].Value },
                // ...
            };
        }

        return null;
    }
}
"""

STARTUP = """
public override void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddScoped<IJSLocalizer, CorsJSLocalizer>();
    // ...
}
"""

VIEW = """
@{
    var translations = Orchard.GetJSLocalizations("cors-admin");
    var translationsJson = Json.Serialize(translations).ToString();
}

@* ... *@
<div id="corsAdmin" data-cors-translations="@translationsJson"></div>
"""

SEED = """
import { setTranslations } from "@orchardcore/bloom/helpers/localizations";

const translationsElement = document.querySelector<HTMLElement>("[data-cors-translations]");

// ...
if (corsSettingsElement && corsFormElement && corsAdminElement && translationsElement) {
    const translations = JSON.parse(translationsElement.dataset.corsTranslations ?? "{}") as Record<string, string>;
    setTranslations(translations);
    // ...
}
"""

STORE = """
const translations: Record<string, string> = {};

export function getTranslations(): Record<string, string> {
    return translations;
}

export function setTranslations(t: Record<string, string>): void {
    Object.assign(translations, t);
}
"""

READ = """
import { getTranslations } from "@orchardcore/bloom/helpers/localizations";

const t = getTranslations();
console.log(t["AddPolicy"]);
"""

PO = """
msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Add a policy"
msgstr "Ajouter une politique"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Default Policy"
msgstr "Politique par défaut"
"""

GROUPS = """
@{
    var translations = Orchard.GetJSLocalizations("my-module", "shared-ui");
}
"""

PACKAGE = """
<PackageReference Include="OrchardCore.Localization.Build" PrivateAssets="all" />
"""

TARGET = """
dotnet msbuild src/MyModule/MyModule.csproj -restore -t:UpdateLocalizationCatalog
"""

WHY = [
    ("1", "C# code translates with PO files", "A string localizer reads the strings of the current culture."),
    ("2", "A script runs in the browser", "It can't call a string localizer."),
    ("3", "Its labels still need translating", "An editor drawn by a script shows text to the visitor."),
]

PATH = [
    ("1", "A localizer", "Returns a dictionary of strings for a group name."),
    ("2", "A view", "Writes the dictionary into the page."),
    ("3", "A script", "Reads the dictionary from the page."),
]

LOCALIZER_NOTES = [
    ("1", "Scoped", "One instance for each request."),
    ("2", "One per module", "Each one answers only the groups it owns."),
]

VIEW_ONE = [
    ("1", "Asks every registered localizer", "Each one returns its dictionary for the group, or null."),
    ("2", "Translated for the request", "The culture of the current request picks the translation."),
]

VIEW_TWO = [
    ("1", "A data attribute", "The element your script works on carries the JSON."),
    ("2", "Part of the page", "The strings arrive with the HTML of the page."),
]

PO_NOTES = [
    ("Context", "The localizer class, in full."),
    ("Message ID", "The English text of the call."),
    ("Translation", "The text in the culture of the file."),
]

LOCATIONS = [
    ("1", "In your module", "[ModuleLocation]/Localization/[CultureName].po"),
    ("2", "In the site", "/Localization/[CultureName].po"),
    ("3", "In a tenant", "/App_Data/Sites/[TenantName]/Localization/[CultureName].po"),
    ("4", "After an edit", "Restart the application for the change to take effect."),
]

RULES = [
    ("1", "Group by group", "For each group, in the order you pass them, every localizer is asked."),
    ("2", "Null is skipped", "A localizer that does not own the group returns null."),
    ("3", "The last one wins", "When two entries share a key, the later value replaces the earlier one."),
]

EXTRACTION = [
    ("1", "Not your script files", "Extraction reads the string localizer calls of your code, not JavaScript or TypeScript."),
    ("2", "Constant keys", "Pass each English text to the string localizer as a constant string."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A script can't call the string localizer")
        bullets(image, WHY)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The path of a string")
        bullets(image, PATH)
        return image

    if step_id == "localizer-1":
        image = slide_base(chapter_title, label, "A class that implements IJSLocalizer")
        draw_code(image, (96, 240, W - 96, 1032), "CorsJSLocalizer.cs", "csharp", LOCALIZER, types=TYPES,
                  highlight={1, 2, 4, 6}, size=24)
        return image

    if step_id == "localizer-2":
        image = slide_base(chapter_title, label, "A dictionary for each group you own")
        draw_code(image, (96, 240, W - 96, 1032), "CorsJSLocalizer.cs", "csharp", LOCALIZER, types=TYPES,
                  highlight={8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 19}, size=24)
        return image

    if step_id == "localizer-3":
        image = slide_base(chapter_title, label, "Register it in Startup")
        draw_code(image, (96, 240, W - 96, 560), "Startup.cs", "csharp", STARTUP, types=TYPES, size=28)
        bullets(image, LOCALIZER_NOTES, top=610)
        return image

    if step_id == "view-1":
        image = slide_base(chapter_title, label, "Ask for a group in the view")
        draw_code(image, (96, 240, W - 96, 620), "Index.cshtml", "cshtml", VIEW, types=TYPES, highlight={2}, size=28)
        bullets(image, VIEW_ONE, top=670)
        return image

    if step_id == "view-2":
        image = slide_base(chapter_title, label, "Write the dictionary into the page")
        draw_code(image, (96, 240, W - 96, 620), "Index.cshtml", "cshtml", VIEW, types=TYPES, highlight={3, 7}, size=28)
        bullets(image, VIEW_TWO, top=670)
        return image

    if step_id == "script-1":
        image = slide_base(chapter_title, label, "Read the attribute in the script")
        draw_code(image, (96, 240, W - 96, 760), "cors-admin-index.ts", "javascript", SEED, types=TYPES, size=24)
        return image

    if step_id == "script-2":
        image = slide_base(chapter_title, label, "One shared object, read anywhere")
        draw_code(image, (96, 240, W - 96, 660), "localizations.ts", "javascript", STORE, types=TYPES, size=26)
        draw_code(image, (96, 700, W - 96, 940), "any component or service", "javascript", READ, types=TYPES, size=26)
        return image

    if step_id == "po-1":
        image = slide_base(chapter_title, label, "A PO entry for each string")
        draw_code(image, (96, 240, 1140, 760), "fr.po", "bash", PO, types=TYPES, size=26)
        note_cards(image, PO_NOTES, (1180, 240, W - 96, 1000))
        return image

    if step_id == "po-2":
        image = slide_base(chapter_title, label, "Where Orchard Core looks for PO files")
        bullets(image, LOCATIONS)
        return image

    if step_id == "groups-1":
        image = slide_base(chapter_title, label, "Request several groups at once")
        draw_code(image, (96, 240, W - 96, 460), "Index.cshtml", "cshtml", GROUPS, types=TYPES, size=28)
        bullets(image, RULES, top=510)
        return image

    if step_id == "template-1":
        image = slide_base(chapter_title, label, "A template for translators")
        draw_code(image, (96, 240, W - 96, 370), "MyModule.csproj", "xml", PACKAGE, types=TYPES, size=28)
        draw_code(image, (96, 400, W - 96, 530), "Terminal", "bash", TARGET, types=TYPES, size=28)
        bullets(image, EXTRACTION, top=580)
        return image

    raise SystemExit(f"No slide for {step_id}")
