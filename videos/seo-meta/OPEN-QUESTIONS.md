# Open questions: seo-meta

Everything in the narration was read from the Orchard Core source (commit `92db4a83`), because the content was written
without running a site. This list says what was checked in the source and what a person must confirm on a running site
while recording. Paths are under `src/`.

## 1. Labels that are computed or come from placement strings

| What the video says | What was checked | To confirm on screen |
|---|---|---|
| The part is listed as **SeoMeta** in Add Parts (`part-1`, `part-2`, and the slides) | The part definition has no display name, so the label is the part name without "Part": `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`, kept by `OrchardCore.Modules/OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19,27-30` | The row reads "SeoMeta" (no space), and the notifications read `The "SeoMeta" part has been added.` and `The "SeoMeta" part settings have been saved.` |
| The editor of an Article has a second tab named **SEO** (`page-1`) | Placement `Parts#SEO;50` (`OrchardCore.Modules/OrchardCore.Seo/Drivers/SeoMetaPartDisplayDriver.cs:39`); fields without a tab go to a tab named "Content" (`OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:14,62-68`) | Exactly two tabs, "Content" and "SEO" |
| The **Social**, **Open Graph** and **Twitter** cards (`defaults-4`, `page-6`, `page-8`) | Placement strings `OrchardCore.Seo/placement.json:18-28,30-40` and `SeoMetaPartDisplayDriver.cs:50,63,72`; the card title is the placement name (`GroupShapes.cs:89`); the cards are expanded by default (`GroupShapes.cs:108`, class "collapse show") | The card titles and their order (the code gives Social 1, Open Graph 20, Twitter 30, Google Schema 40); that the fields without a card are in a card titled "Content" (the code puts every shape without a card in a group named "Content", `OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:14,62-68` and the card variant of the same file, and a missing position sorts before the numbers, `FlatPositionComparer.cs`, so that card comes first); that the cards of the SEO tab are expanded. The narration never states an order of cards. The plan's scroll actions may need adjusting |
| The Social Meta Settings page has a **Social** card with the three pictures (`defaults-4`), below a **Content** card with the ten text fields | `placement.json:30-40`; the text fields have no placement, so they fall into the "Content" card, which sorts first | The arrangement of that page: **Content** card (**Default Meta Description** at the top), **Social** card last. The plan's scrolls depend on it: `defaults-3` scrolls through the **Content** card, and `defaults-4` scrolls on to the **Social** card |

## 2. The media field dialog

The Media field is drawn by a script of the Bloom package, whose source is not in the tree. Its labels were read from
the server strings (`OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:118-137`: "Add media", "Remove",
"Media text", "Enter media text", "OK", "Cancel", "No Files", "Select Media") and from the built script
(`OrchardCore.Modules/OrchardCore.Media/wwwroot/Scripts/media-field2.js`). To confirm: clicking a file in the **Select
Media** dialog selects it ("1 selected" in the footer, confirmed in the built script), **OK** inserts it, the **Media text** button is
drawn only while the picture is selected (in the script the button exists only when an item is selected), and its dialog uses the title "Enter media text" and a text area.
The field setting that allows the media text defaults to on (`OrchardCore/OrchardCore.Media.Core/Settings/MediaFieldSettings.cs:12`) and the SEO fields
do not change it, so the button should be there in both editors. The class `.mf-modal-content` used by the
plan for the dialog comes from the built script and may differ.

## 3. Menus and permissions

- **Settings > Social Meta Settings** is a Custom Settings entry named after the display name of the content type
  (`OrchardCore.Modules/OrchardCore.CustomSettings/AdminMenu.cs:44-45`). It needs the Custom Settings feature, which the Blog
  recipe enables, and it appears once the migration of the SEO module has created the type
  (`OrchardCore.Modules/OrchardCore.Seo/Migrations.cs:40`). To confirm: it is in the menu on the first page loaded after `enable-2`.
- The Administrator role "grants all permissions" (`docs/reference/modules/Roles/README.md:11`), which covers the
  permission of this settings group (`OrchardCore.Modules/OrchardCore.CustomSettings/Permissions.cs`). To confirm: the `admin` user opens the page.
- The notification of `enable-2` reads "The feature SEO was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`). It lists the features
  that were asked for, not the ones enabled with them (`OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs`, case `Enable`), so it names only SEO.
  The four features SEO depends on are already enabled by the Blog recipe, so the narration's sentence about dependencies is general and nothing else is enabled on camera.
  To confirm: the page reloads with an empty search box after the click.

## 4. Statements that cannot be seen on screen

The result of these settings lives in the head of the page, which is code, so an admin video cannot film it. These
narration statements come from the code and should be confirmed once, outside the capture, with the check at the end of
clip `05-page` in recording-plan.md:

- `why-1`: Social Meta Settings apply to every page that shows a content item, whatever its content type (also when the type has no SeoMeta part).
  Code: the handler `OrchardCore.Modules/OrchardCore.Seo/Handlers/SeoMetaSettingsHandler.cs` is a content handler
  (`Startup.cs:38`), and the driver `Drivers/SeoContentDriver.cs:44-65` (registered at `Startup.cs:34`) runs for every item shown with the Detail display type
  that is not a widget. To confirm: the visitor page of the **Blog** item (a type without the part) carries the default
  description `A demo blog built with Orchard Core.` and the default social picture (after `03-defaults`).
- `page-6`, `page-7`, `sources-3`: the page sets only its **Default social image** (with a media text), which feeds both the Open Graph and the
  Twitter picture (`Handlers/SeoMetaPartHandler.cs:85-101,122-139`). The picture and its alternative text each follow their own chain: a page
  that sets only an **Open graph image** without media text would keep the picture of that field for Open Graph, but get the Twitter picture and
  the alternative text from Social Meta Settings (`Handlers/SeoMetaSettingsHandler.cs:203-281`). That is why the video fills the page's Default social image and its media text.
- `page-2`: with **Render** cleared the module writes nothing for the page (`SeoContentDriver.cs:62-65`).
- `page-5`: **Meta Robots** is written as a meta tag named robots (`SeoContentDriver.cs:104-111`), which is why the narration calls it "the robots meta tag of this page". Its value is not explained, because neither the module's documentation nor its hint says what to type there.
- `page-3` and `sources-1`: the Page Title replaces the whole browser title, so the site name is not part of it. Code: `SeoContentDriver.cs:72-75`
  sets a fixed title, which `OrchardCore/OrchardCore.DisplayManagement/Title/PageTitleBuilder.cs:43-48` returns as it is. This holds
  for the default **Page title format** setting (`OrchardCore.Modules/OrchardCore.Settings/Services/SiteService.cs:57`;
  label `OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:25`). A site with another format may behave differently.
- `sources-1`, `sources-2`, `sources-3`: the order in which values are chosen follows `Handlers/SeoMetaPartHandler.cs:37-177` (the page)
  and then `Handlers/SeoMetaSettingsHandler.cs:37-291` (from line 166 on) (Social Meta Settings, then the automatic values). Two details that surprise:
  the Open Graph and Twitter descriptions fall back to the Meta Description of the page and then to **Default Open Graph
  Description** or **Default Twitter Description**, and not to **Default Meta Description**; the Open Graph and Twitter titles fall back to the Page Title and then
  to the title of the content item. `sources-2` and its slide name both default labels for that reason, and `defaults-3` names them as fields that stay empty.

## 5. A hint on screen that disagrees with the code

The hints under **Open Graph Title** and **Twitter Title** read "The Title is used by default, then the Page Title, or can
be overridden here." (`OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPartOpenGraph.Edit.cshtml:15`,
`SeoMetaPartTwitter.Edit.cshtml:7`). The code chooses the own field, then the Page Title, then the title of the content
item (`Handlers/SeoMetaPartHandler.cs:103-110,141-148` and `SeoMetaSettingsHandler.cs:179-182,233-236`). The narration
(`sources-2`) follows the code and does not quote the hint. The hint is user interface text, not a documentation page, so it is not
in the doc issues of the report.

## 6. Pronunciation (not confirmed)

`narrate.py` and `pronounce.py` were not run (they create audio). The `spoken` map has two entries. `"Add & Configure": "Add and Configure"`
lets the narration show the label as the screen writes it while the voice says it as words. `"SeoMeta": "SEO Meta"` is a new spelling:
a voice may read the camel-case word "SeoMeta" as the name "Seo" followed by "Meta", and "SEO" written in capitals is read
letter by letter. A person should confirm both with `scripts/pronounce.py`, and listen to "SEO", "Open Graph", "Canonical" and
"Social Meta Settings". If "SEO Meta" does not sound right, try `"SeoMeta": "S.E.O. Meta"`.

## 7. Left out on purpose

- **Meta Keywords**, **Custom Meta Tags** and **Google Schema**: their panels stay hidden in the video. The Custom Meta Tags editor has an
  "Edit Data" dialog that shows JSON, and Google Schema is JSON (`Drivers/SeoMetaPartDisplayDriver.cs:129-131`), so both belong to a developer video.
- The values accept shortcodes (`Drivers/SeoContentDriver.cs`, each value goes through the shortcode service), the SeoMeta part is available to
  GraphQL (`GraphQL/`) and its title, description and keywords are indexed (`Indexes/SeoMetaPartContentIndexHandler.cs`). None of this is in the video.
- The robots file (Settings > Search > Search Engine Optimization > Robots) is the subject of another video.
- No claim is made about what a particular search engine or social network shows. The video only says what the SEO module writes
  for them to read, and the introduction states in general terms that they read a title, a description and a picture; this follows the
  list of features in `docs/reference/modules/Seo/README.md` (meta description, canonical URL, Open Graph, Twitter Card).
