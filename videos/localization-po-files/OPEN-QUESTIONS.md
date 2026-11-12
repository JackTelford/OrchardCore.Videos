# Open questions: Translate the UI: Install and Write PO Files

What was checked in the Orchard Core source (`92db4a83`) and the documentation, and what a person has to confirm on a
running site or on the outside services before the video is recorded. Each statement of the narration is either what the
documentation says, or follows from the code that is named; the items below are what only a running site or the outside
service can confirm.

## On the Crowdin site (not recorded, shown on slides)

1. **The steps and labels of Crowdin** (`install-1`): select **French**, then **Download or Upload**, then **Download translations**,
   and a zip file named `fr.zip` is saved. They come from the installation guide
   (`src/docs/guides/install-localization-files/README.md`) and its screenshots, not from the source of Orchard Core. The guide's text
   says "Download", while its screenshot (`assets/crowdin-download.jpg`) shows an icon button whose tooltip is "Download or Upload" and a
   menu item "Download translations"; the narration and the slide use the screenshot's label. Confirm the labels, and that the
   project is called "Orchard Core", on the project page.
2. **The layout of the zip** (`install-2`): the guide and its screenshot show a folder named `fr` holding one PO file for each
   module (`OrchardCore.Admin.po`, `OrchardCore.AdminMenu.po`, `OrchardCore.Alias.po`, `OrchardCore.Contents.po` are
   the names on the screenshot). Extract a current `fr.zip` and confirm that its root is the `fr` folder. The code
   reads every PO file of `Localization/<culture>/` (`OrchardCore.Localization/ModularPoFileLocationProvider.cs:80-86`), so
   the layout works only when the files end up in that folder.
3. **Anyone can add or improve a language** (`install-1`): from the guide, not from code.

## On a running site

4. **The culture query string** (`result-1`, `result-2`): Orchard Core has no code of its own for `?culture=fr`. It relies on the
   request localization of ASP.NET Core, which the guide documents. The Localization feature restricts the cultures to the
   supported ones (`OrchardCore.Localization/Startup.cs:56-60`). Confirm that `/Admin/Features?culture=fr` shows the
   French texts of the stand-in file, and that `?culture=de` (a culture that is not supported) shows English.
5. **A browser configured with French** (`result-4`): from the guide ("if a browser is configured with French as its default
   culture"), not testable in the recording. Confirm with a browser set to French, without the query string.
6. **The menu stays in English** (`result-2`, `result-3`): true of the stand-in file only. Its contexts
   (`OrchardCore.Features.AdminMenu`, `OrchardCore.Settings.AdminMenu`) have no entry in the stand-in, and the lookup falls back to an
   entry without a context only when there is one
   (`OrchardCore/OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizer.cs:221-229`). The community files do translate
   menu texts: the guide's screenshot `assets/localized-french.jpg` shows a French menu. So the narration says that the recording uses a
   small French file, and that "the file" has no entry for the menu, and never says it of the community files. Confirm on screen that no
   label of the menu is French, and that the title, the search box, **Bulk Actions**, the **Enable** buttons and the count are.
7. **The option text of the culture list** (`enable-4`): the editor writes the culture name and its display name in
   parentheses, `fr (French)`; the guide writes `fr | French`. The display name is `CultureInfo.DisplayName`, in the UI culture of the
   request. Confirm the text on a running site (the narration says "French" only).
8. **The first culture of the table** (`enable-3`): the Localization settings start with `CultureInfo.InstalledUICulture`, the language
   the operating system is installed with, as the only supported culture, or `en` when the runtime reports none
   (`OrchardCore.Localization/Models/LocalizationSettings.cs:15-27`). It is not the display language of the signed-in account. Confirm
   that the table of `03-enable` lists `en-US` as the default on the recording machine.
9. **The notice of `enable-5`**: the warning `The site has been restarted for the settings to take effect.` and the
   success notice `Site settings updated successfully.` come from
   `OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132` and
   `OrchardCore.Settings/Controllers/AdminController.cs:119`. Confirm that both show after the redirect, and that the
   warning stays on screen until the narration ends (it has no dismissal delay in the notifier, the success notice hides
   itself after five seconds).
10. **The load order** (`where-3`, `write-4`): derived from the order of the locations returned by
    `ModularPoFileLocationProvider.GetLocations` (module and theme files, `Localization/<culture>.po`, the tenant file, the files split by
    module, then every file of `Localization/<culture>/`) and from `CultureDictionary.MergeTranslations`, which replaces an entry
    that already exists (`OrchardCore.Localization.Abstractions/CultureDictionary.cs:96`). Confirm with files that
    define the same text differently (for example `Localization/fr.po`, `App_Data/Sites/Default/Localization/fr.po` and
    `Localization/fr/OrchardCore.Features.po`): the file of the culture folder should win over the other two.
11. **The regional culture** (`where-4`): follows from the file name being the culture name and from the loop over the parent
    cultures when **Fall back to parent culture(s)** is selected (`PortableObjectStringLocalizer.cs:173-186`). Confirm with
    the supported cultures `en-US` and `fr-CA`, a `fr.po` file only, and the box selected and cleared.
12. **Editing a PO file** (`write-5`): the documentation says to restart the application. The dictionary of a culture is
    cached (`OrchardCore.Localization.Core/LocalizationManager.cs:38`), and the narration says only "restart the
    application". Whether saving the cultures, which restarts the tenant, also loads the files again is not verified.
13. **The tenant folder** (`write-4`): `App_Data/Sites/<tenant>/Localization/<culture>.po` is built from the shell options and
    the tenant name (`ModularPoFileLocationProvider.cs:45` and `:65`). Confirm with the `Default` tenant, where the folder is
    `App_Data/Sites/Default/Localization`.
14. **Publishing the Localization folder** (`install-3`): the CMS targets add `Localization\**` to the files that publish copies
    (`OrchardCore.Application.Cms.Core.Targets.targets:46-52`, the `PublishTranslationFiles` target) and remove it from the `Content`,
    `Compile`, `EmbeddedResource` and `None` items (`:4-7`). The installation guide and the Localization documentation tell the reader to
    add `<Content Include="Localization\**">` to the project file instead. The narration follows the code for an application that uses the
    CMS targets, and the documentation for any other project. Confirm with `dotnet publish` of a CMS application that has
    `Localization/fr`, without the item and with it, and look for `Localization/fr` in the publish folder; confirm also what a project
    without the CMS targets needs.
15. **Translation files copied by the build** (`recording-plan.md`): `OrchardCore.Application.Cms.Targets.csproj:42` references the
    `OrchardCore.Translations.All` package, and `OrchardCore.Application.Cms.Core.Targets.targets:34-44` copies the translation files that
    a package declares into `Localization` after a build. The source of Orchard Core does not show what that package declares. Build
    `OrchardCore.Cms.Web` and list its `Localization` folder: if a `fr` folder is already there, the community French files come with
    the package, the plan empties the folder before it creates the stand-in, and the question for the maintainer is whether
    the guide should say that a CMS application already has the translation files that the package ships.
16. **The trail above the title** (`result-2`): the Features page declares no ancestor and the Admin Dashboard feature, the only provider of a
    Dashboard item, is not enabled by the Blog recipe, so the trail should hold the page name only, in French
    (`OrchardCore.Navigation.Core/BreadcrumbTagHelper.cs:104-135`, `OrchardCore.Themes/TheAdmin/Views/Breadcrumb.cshtml`). The narration does
    not mention it. Confirm on screen.

## Pronunciation (to confirm with `scripts/pronounce.py`; the slides and captions keep the written form)

The `spoken` map of `storyboard.json` asks for these spellings: `PO` as "P.O.", `fr.zip` as "F.R. dot zip", `fr-CA` as
"F.R. dash C.A.", `?culture=fr` as "question mark culture equals F.R.", `Localization/fr.po` as "Localization slash F.R.
dot P.O.", `msgctxt` as "message context", `msgid` as "message I.D.", `msgstr` as "message string", `App_Data` as "App Data",
`MyNamespace.MyService` as "My Namespace dot My Service", and `OrchardCore.Features.Views.Admin.Features` as "Orchard
Core dot Features dot Views dot Admin dot Features". The fr in "such as fr.", "named fr," and "the fr files" is spoken
"F.R." through phrase entries, since a plain `fr` entry would also change "from" and "French". Also check how the voice
says "Crowdin", which has no entry.
