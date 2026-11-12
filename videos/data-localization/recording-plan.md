# Recording plan: Dynamic Translations

This plan is for the person who records the video. Read it from top to bottom once, then follow the clips in the order
of "Recording order". Nothing here asks you to decide anything: every value to type, every label to click and every
condition to wait for is written down. In this file a selector such as `#culture-select` stands for
`scene.Page.Locator("#culture-select")`, because the recorder's methods take locators. Labels come from the Orchard
Core source; `file:line` references are relative to the `src` folder of the Orchard Core checkout.

The video has 8 chapters: `01-why`, `07-access` and `08-move` are slides (nothing to record); `02-enable`,
`03-editor`, `04-translate`, `05-switch` and `06-statistics` are recordings.

## The site it needs

- **Source:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout of the `main` branch, run on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both
  repositories.
- **Auto Setup**, so no setup or sign-in form is recorded (environment variables, one tenant):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, the same value as `VIDEO_PASSWORD`, chosen outside git |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  The Auto Setup feature itself must be enabled in the host project; `OrchardCore.Cms.Web` does it in `Program.cs:9`
  (see `docs/reference/modules/AutoSetup/README.md`).
  Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls an outside service. The video needs no
  external service and no local stand-in.
- **No PO files.** Do not install any localization file (no `Localization` folder in the web project, none under
  `App_Data/Sites/Default`). The narration of `switch-2` says that the interface stays in English because no PO files
  are installed; that is only true on a site without them.
- **Sign in once** outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`
  writes `state.json` (git-ignored). Sign in with the browser's language left at `en-US`; the admin culture cookie is
  only set in `05-switch`.
- **Fake data only.** The Blog recipe's sample content is the only content. The only user is `admin` with
  `admin@example.com`. Create no other users, contents or tenants.

### Features and settings to prepare (outside the capture)

The site starts with the features of the Blog recipe (Roles, Contents, Content Types, Admin Menu, Taxonomies, Media,
Deployment and the others it enables). Prepare these three things before any recording, with
**Tools, Deployments, JSON Import** (label `JSON Import`, field `JSON`, button `Import`:
`OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`, `Views/Import/Json.cshtml:15,21`). Paste one recipe at a
time and select Import; wait for the message `Recipe imported successfully!` before the next one
(`OrchardCore.Deployment/Controllers/ImportController.cs:185`).

1. Enables Localization and the Admin Culture Picker. Do **not** enable Data Localization: `02-enable` does that on
   camera.

   ```json
   {
     "steps": [
       { "name": "feature", "enable": ["OrchardCore.Localization", "OrchardCore.Localization.AdminCulturePicker"] }
     ]
   }
   ```

2. Sets the cultures. English (United States) is first, so it is the first culture of the editor (`editor-1`).

   ```json
   {
     "steps": [
       {
         "name": "settings",
         "LocalizationSettings": {
           "DefaultCulture": "en-US",
           "SupportedCultures": ["en-US", "fr-FR", "es-ES"]
         }
       }
     ]
   }
   ```

3. Makes the content type Blog Post creatable. The Blog recipe leaves `Creatable` unset for it
   (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the type `BlogPost`: only `Draftable` and
   `Versionable`), and the **New** menu of Manage Content lists only the creatable types
   (`OrchardCore.Contents/Controllers/AdminController.cs:174-181`), so `switch-3` needs this. The step must carry the
   `DisplayName`, because the recipe handler merges the settings only when it is present
   (`OrchardCore.ContentTypes/RecipeSteps/ContentDefinitionStep.cs:54-58`).

   ```json
   {
     "steps": [
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "BlogPost",
             "DisplayName": "Blog Post",
             "Settings": {
               "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true }
             }
           }
         ]
       }
     ]
   }
   ```

Check: **Settings, Localization, Cultures** lists English (United States), French (France) and Spanish (Spain) as
supported, with English (United States) as the default, and the top bar of the admin shows a culture picker button
labelled `English (United States)`. Under **Design, Content Definition, Content Types**, edit **Blog Post**: the
checkbox **Creatable** is checked (`OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:17`), and the type
still has its four parts. Close the editor without saving.

Stop the site and copy its `App_Data` folder and `state.json` aside as **snapshot A**.

### The translation seed (outside the capture, after `02-enable`)

The Data Localization feature must be enabled before its translations can be seeded, so the seed runs after the clip
`02-enable`. Open `/Admin/DataLocalization/Index`, open the browser console, paste the script and run it. It saves,
for French, a translation for every string except three (the ones translated on camera), and for Spanish three
translations. English (United States) stays empty. The translations of the strings that are not listed in the maps
repeat the original text, which counts as translated and changes nothing on screen.

```js
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

const frMap = {
  "Content Types|Blog": "Blog",
  "Content Types|Container": "Conteneur",
  "Content Types|Blockquote": "Citation",
  "Content Types|Paragraph": "Paragraphe",
  "Content Types|Raw Html": "HTML brut",
  "Content Types|Tag": "Étiquette",
  "Content Types|Category": "Catégorie",
  "Content Fields:BlogPost|Subtitle": "Sous-titre",
  "Content Fields:BlogPost|Banner Image": "Image de bannière",
  "Content Fields:BlogPost|Category": "Catégorie",
  "Content Fields:Article|Subtitle": "Sous-titre",
  "Content Fields:Article|Banner Image": "Image de bannière",
  "Content Fields:Blog|Banner Image": "Image de bannière",
};
const frSkip = [
  "Content Types|Blog Post",
  "Content Fields:BlogPost|Tags",
  "Admin Menus:Admin menus|Main Menu",
];
const esMap = {
  "Content Types|Blog Post": "Entrada de blog",
  "Content Types|Page": "Página",
  "Content Types|Article": "Artículo",
};

async function load(culture) {
  const response = await fetch(`/Admin/DataLocalization/GetStrings?culture=${culture}`);
  const data = await response.json();
  const all = [];
  for (const provider of data.providers) {
    all.push(...(provider.strings ?? []));
    for (const group of provider.subGroups ?? []) all.push(...group.strings);
  }
  return all;
}

async function save(culture, translations) {
  const response = await fetch("/Admin/DataLocalization/Save", {
    method: "POST",
    headers: { "Content-Type": "application/json", RequestVerificationToken: token },
    body: JSON.stringify({ culture, translations }),
  });
  console.log(culture, response.status, translations.length);
}

const french = (await load("fr-FR")).map((s) => {
  const key = `${s.context}|${s.key}`;
  return { context: s.context, key: s.key, value: frSkip.includes(key) ? "" : (frMap[key] ?? s.key) };
});
await save("fr-FR", french);

const spanish = (await load("es-ES"))
  .filter((s) => esMap[`${s.context}|${s.key}`])
  .map((s) => ({ context: s.context, key: s.key, value: esMap[`${s.context}|${s.key}`] }));
await save("es-ES", spanish);
```

The console must print `fr-FR 200 <count>` and `es-ES 200 3`. Then check, in the editor and on the statistics page,
before taking **snapshot B** (stop the site, copy `App_Data` and `state.json`):

- French, category Content Types, **Missing only** checked: the row `Blog Post` is listed (other types of the site,
  if any, translated too). Category Content Fields, search `Tags`: one row under the group `BlogPost`, empty.
  Category Admin Menus, search `Main Menu`: one row under the group `Admin menus`, empty.
- The categories of the editor are exactly Admin Menus, Content Fields, Content Types and Permissions (the narration
  of `editor-2` lists them). If another one appears, write it in `OPEN-QUESTIONS.md` instead of recording.
- Statistics: French is green (75 percent or more), Spanish and English (United States) are red.

### Recording order and snapshots

Clips run in this order. A clip that changes the site must be taken from the snapshot named in the table; restore it
(stop the site, restore `App_Data` and `state.json`, start the site) before every new take of that clip.

| Order | Clip | Chapter | Starts from | Changes the site? | After the clip |
|---|---|---|---|---|---|
| 1 | `02-enable` | 02 Enable the feature | snapshot A | Yes: enables Data Localization | Run the seed, take snapshot B |
| 2 | `03-editor` | 03 The translation editor | snapshot B | No (nothing is saved) | |
| 3 | `04-translate` | 04 Translate strings | snapshot B | Yes: saves three French translations | Take snapshot C |
| 4 | `05-switch` | 05 See the translations | snapshot C | No (the admin culture cookie lives only in the browser of the clip) | |
| 5 | `06-statistics` | 06 Track progress | snapshot C | No | |

`05-switch` and `06-statistics` depend on `04-translate`: they need the three translations it saves.
`03-editor` must be taken before `04-translate`, or from snapshot B, because `editor-4` ends on the missing row
`Blog Post`.

### Browser setup shared by the clips

The recorder's Chromium (1536x864, scale 1.25, light, `en-US`) is used as it is. Every clip starts signed in as `admin`
from `state.json`. Open each clip's start page with `scene.OpenAsync` (not recorded), prepare what the clip says
before `StartCaptureAsync`, then run the steps. "Wait for X" means a `WaitForAsync` on the locator; do not use a fixed
pause for it. A step's actions must end before its narration does.

---

## Clip `02-enable` (chapter 02, start page `/Admin`)

Before the capture: open `/Admin` and make sure every group of the left admin menu is collapsed (reload the page).

`enable-1` Click **Tools** in the left admin menu, then click **Features** under it, with
`ClickAndWaitForNavigationAsync`. On screen at the end: the Features page (title `Features`, the search box with the
placeholder `Search`, the features grouped by category). Labels: `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`,
`Views/Admin/Features.cshtml:15,27`.

`enable-2` Click the search box `#search-box` and type `Data Localization`. Wait until the list shows only one
feature row (the text `Showing 1 of` appears in `#list-summary`). Highlight the group title `Internationalization`
(`h3` with that text; `Views/Admin/Features.cshtml:88`, category from `OrchardCore.Modules/OrchardCore.DataLocalization/Manifest.cs:9`).
Then, about 3 seconds before the narration ends, click the `Enable` link of that row (`a.btn` with the text `Enable`
inside the visible row: `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) with `ClickAndWaitForNavigationAsync`.
The site reloads its shell, so the page can take a few seconds; wait for the notification
`The feature Data Localization was enabled.` (`OrchardCore.Features/Controllers/AdminController.cs:317`). On screen at
the end: that notification.

`enable-3` Click **Settings** in the left admin menu, then **Localization** under it, so that its two items are
visible: **Cultures** and **Dynamic Translations** (`OrchardCore.DataLocalization/AdminMenu.cs:30-32`,
`OrchardCore.Localization/AdminMenu.cs:34-38`). Do not click either item. Highlight **Dynamic Translations**. On
screen at the end: the open menu with both items, **Dynamic Translations** highlighted.

After this clip: run the seed ("The translation seed"), then take snapshot B.

---

## Clip `03-editor` (chapter 03, start page `/Admin`)

Before the capture: in the left admin menu, open **Settings**, then **Localization**, so **Dynamic Translations** is
visible. The browser stays in English.

`editor-1` Click **Dynamic Translations** with `ClickAndWaitForNavigationAsync`. Wait until `#translation-editor` is
visible and the spinner is gone. Highlight `#culture-select`. On screen at the end: the title `Dynamic Translations`,
the **Culture** drop-down showing `English (United States) (en-US)`, and the first category with the text
`strings` beside its name and no `translated` count. Labels: `OrchardCore.DataLocalization/Views/Admin/Index.cshtml:18,42-48,103`.
The option text is `{display name} ({culture name})` (`Index.cshtml:46`).

`editor-2` Clear the highlight. Select French in `#culture-select` with `SelectAsync(..., "fr-FR")`. Wait until the
category names have a `translated` count (the editor reloaded the strings). Then scroll the page so that the legend of
each category reaches the top of the editor in turn, about one every 1.5 seconds, in this order: `Admin Menus`,
`Content Fields`, `Content Types`, `Permissions` (the `legend` of each `fieldset.translations-list`: `Index.cshtml:100-104`).
On screen at the end: the **Permissions** legend near the top of the page.

`editor-3` Scroll back to the top of the page. Highlight the first category's legend (`Admin Menus`, with its
`(N strings, M translated)` hint), then highlight the first row's pair: the original text in the first column
(`Original`) and the input of the second (`Translation`) (`Index.cshtml:110-125`). On screen at the end: the top of
the editor with the culture `French (France) (fr-FR)`, the first category legend and its table header `Original` and
`Translation` highlighted.

`editor-4` Clear the highlights. Select `Content Types` in `#filter-select` (label **Category**; first option
`All Categories`: `Index.cshtml:52-60`). Click `#search-box` (label **Search**, placeholder `Search translations`:
`Index.cshtml:63-66`) and type `Blog`. Then check `#missing-only-toggle` (label **Missing only**: `Index.cshtml:73-74`).
On screen at the end: the category **Content Types** only, the search box containing `Blog`, **Missing only**
checked, and one row: `Blog Post` with an empty input (its placeholder repeats `Blog Post`). Do not type in the input.

This clip saves nothing.

---

## Clip `04-translate` (chapter 04, start page `/Admin/DataLocalization/Index?culture=fr-FR`)

Before the capture: open the start page. The Culture drop-down shows French (France); the category is `All
Categories`; the search box is empty; **Missing only** is unchecked; **Auto-save** is checked (it is on by default:
`OrchardCore.DataLocalization/Assets/ts/translation-editor.ts:89`).

`translate-1` Select `Content Types` in `#filter-select`. Click `#search-box` and type `Blog Post`. On screen at the
end: only the category **Content Types**, with one row `Blog Post` and an empty input (placeholder `Blog Post`).

`translate-2` Click the input of the `Blog Post` row (`tr[data-text="blog post"] input`) and type `Article de blog`,
timed so that the last key is typed about 3.5 seconds before the narration ends (the length of the narration is in
`durations.json`). Do not pause after the last key: click the **Save** button (`button.save`, label `Save`:
`Index.cshtml:169-172`) within 1 second of it, so about 2.5 seconds before the narration ends. The button is disabled
while nothing is unsaved (`:disabled="!isDirty || isSaving"`, `Index.cshtml:169`), and auto-save saves and clears the
unsaved state 2 seconds after the last key (`translation-editor.ts:162-170`), so a click after a longer pause waits for
a disabled button and fails. Wait for the message `Translations saved successfully.` (`.toast.show`; `Index.cshtml:9`,
shown for 3 seconds by `translation-editor.ts:269`). On screen at the end: the message in the top right corner, and the
row `Blog Post` with `Article de blog` in its input. The **Unsaved changes** badge (`Index.cshtml:87`) is gone.
Auto-save still fires 2 seconds after the last key (a manual save does not cancel it) and shows a second message:
that is expected.

`translate-3` Select `Content Fields` in `#filter-select`. Click `#search-box`, select its text (Ctrl+A) and type
`Tags`. On screen: the category **Content Fields**, the group heading `BlogPost` (`h6` in the group:
`Index.cshtml:131-135`) and one row `Tags` with an empty input. Click that input and type `Étiquettes`. If the
recorder cannot type the capital `É` with `TypeAsync`, insert it with `scene.Page.Keyboard.InsertTextAsync("É")` and
type the rest. Do not click **Save**: auto-save stores it 2 seconds after the last key, so type it at least 3 seconds
before the narration ends. On screen at the end: the row `Tags` with `Étiquettes`. Group heading source: `OrchardCore.Contents/Services/ContentFieldLocalizationDataProvider.cs`.

`translate-4` Select `Admin Menus` in `#filter-select`. Click `#search-box`, select its text and type `Main Menu`.
On screen: the category **Admin Menus**, its group heading `Admin menus` (the name of the admin menu that the Blog
recipe creates) and one row `Main Menu` with an empty input. Only the top-level items of an admin menu are listed
(`LinkAdminNodeDataLocalizationProvider.cs` reads `m.MenuItems`), which is why the narration says so.

Click that input and type `Menu principal`, timed so that the last key is typed about 3.5 seconds before the narration
ends. Do not click **Save**: the button is disabled once auto-save has stored the text, and a click after the 2 seconds
of auto-save would fail (see `translate-2`). Auto-save stores the text 2 seconds after the last key and shows the
message `Translations saved successfully.` (`.toast.show`, 3 seconds), which is still on screen when the narration
ends. On screen at the end: the row `Main Menu` with `Menu principal`, and the message. After the step, wait until the
**Unsaved changes** badge is not visible, so nothing is lost when the clip ends. Source of the strings:
`OrchardCore.AdminMenu/Services/LinkAdminNodeDataLocalizationProvider.cs`.

After this clip: reload the editor, French: `Blog Post`, `Tags` and `Main Menu` keep their translations. Then take
snapshot C.

---

## Clip `05-switch` (chapter 05, start page `/Admin/Contents/ContentItems`)

Before the capture: open the start page. The title is `Manage Content` and the culture picker button in the top bar
is labelled `English (United States)`. The left admin menu shows the items of the custom menu, among them
`Main Menu`. Select **New**: the list includes `Blog Post`, next to the other creatable types (for example `Article`,
`Blog`, `Page` and `Taxonomy`). The Blog recipe does not make `Blog Post` creatable, so it is there only if the third
preparation recipe was applied. Close the list without choosing an item.

`switch-1` Click the culture picker button `#oc-culture-picker`
(`OrchardCore.Localization/Views/AdminCulturePicker.cshtml:8`). In the list, click the item with the text
`français (France)` (`.language-menu-item[data-language-value="fr-FR"]`, line 25) using `WaitForNavigationAsync`: the
click sets a cookie and reloads the page (`Assets/ts/admin-culture-selector.ts`). Click the item about 4 seconds
before the narration ends. On screen at the end: the reloaded page, with the picker button labelled
`français (France)`.

`switch-2` Highlight the item `Menu principal` in the left admin menu (no caption). Other items of the menu, such as
`Content` and `Settings`, stay in English. On screen at the end: the highlight around `Menu principal`. The label is
translated by `OrchardCore.Themes/TheAdmin/Views/NavigationItemText.cshtml:40`.

`switch-3` Clear the highlight. Click the `New` button of the page (`#new-dropdown`: `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-20`).
Highlight the item `Article de blog` of the open list (line 25). The list holds the types that are set as Creatable,
sorted by their original display names, so `Article de blog` comes after `Blog`
(`OrchardCore.Contents/Controllers/AdminController.cs:861-883`). On screen at the end: the open list, with
`Article de blog` highlighted, and the page title `Manage Content` in English.

`switch-4` Clear the highlight. Click `Article de blog` with `ClickAndWaitForNavigationAsync`. The title of the page
reads `New Article de blog` (`OrchardCore.Contents/Views/Admin/Create.cshtml:26`; `New` is not translated). Wait about 2
seconds, then scroll down until the label `Étiquettes` (the field named Tags) is visible
(`OrchardCore.Taxonomies/Views/TaxonomyField-Tags.Edit.cshtml:28,37`: the Tags field of the Blog recipe uses the editor
`Tags`, so this view draws the label, not `TaxonomyField.Edit.cshtml`), and highlight it. On screen at the end: the label
`Étiquettes` highlighted. Do not save the new content item: the clip ends on this page.

This clip changes nothing on the site.

---

## Clip `06-statistics` (chapter 06, start page `/Admin/DataLocalization/Index?culture=fr-FR`)

Before the capture: open the start page, English interface (no admin culture cookie).

`stats-1` Click the **Statistics** button at the bottom of the editor (`a.btn` with the text `Statistics`:
`OrchardCore.DataLocalization/Views/Admin/Index.cshtml:173`) with `ClickAndWaitForNavigationAsync`. Highlight the bar
of **Overall Progress** (`Views/Admin/Statistics.cshtml:17-33`). On screen at the end: the title `Statistics` with the
breadcrumb `Dynamic Translations`, and the section **Overall Progress** with its bar and the count `x / y strings`.

`stats-2` Clear the highlight. Scroll down to **Progress by Culture** (`Statistics.cshtml:38-75`) and highlight, one
after the other, the bar of French (France) (green), then the bars of English (United States) and Spanish (Spain)
(red). On screen at the end: the table **Progress by Culture** with its three rows and the columns `Culture`,
`Progress` and `Completion`. Thresholds: `Assets/ts/translation-statistics.ts:59-68`.

`stats-3` Clear the highlights. Scroll down to **Progress by Category** (`Statistics.cshtml:78-126`). Select French in
the **Culture** drop-down `#selected-culture` with `SelectAsync(..., "fr-FR")`, and wait for the table to list the
categories. On screen at the end: **Progress by Category** with French (France) chosen and one row for each category
(Admin Menus, Content Fields, Content Types, Permissions), each with its bar and count.

This clip changes nothing on the site.

---

## Slides (nothing to record)

- `why-1`: four cards, "Names that depend on your site's data": content type names, content field names, custom admin
  menu items (the top-level items of the admin menus you build), permissions.
- `why-2`: four cards, "How a name is shown": the page asks, the culture decides, a translation exists, no translation.
- `why-3`: three cards, "Which tool translates what": PO files, Data Localization, Content Localization.
- `access-1`: three cards, "Permissions of Data Localization": Manage all dynamic translations, View dynamic
  translations and statistics, Manage French (France) translations. Sources: `OrchardCore/OrchardCore.Localization.Core/Data/DataLocalizationPermissions.cs:13,20,35`.
- `access-2`: three cards, "Who gets what by default": Administrator, Editor, and a user who can only view
  (`OrchardCore.DataLocalization/Permissions.cs:44-56`; the read-only message is `Views/Admin/Index.cshtml:24`).
- `move-1`: three cards, "Move translations to another site": the deployment step Translations
  (`Views/Items/TranslationsDeploymentStep_Fields_Thumbnail.cshtml`), its cultures and categories options
  (`Views/Items/TranslationsDeploymentStep_Fields_Edit.cshtml:11-13,31`), and the import on a site where Data
  Localization is enabled. The step Dynamic Data Translations is not on the slide: its export is not read back by the
  recipe step (see `OPEN-QUESTIONS.md`).

## Checks for private data

Before publishing, follow step 6 of the skill (OCR at one frame per second, at every scene change and at five frames
per second) and search the text for anything that is not one of these fake values: the user `admin`, the address
`admin@example.com`, the site name `Orchard Core`, and the sample content of the Blog recipe. Look for the machine name
in the address bar (the capture shows none), a real password (it is never typed on camera: the recorder is signed in
from `state.json`), and any real person's name in the Blog recipe's sample content. A site built from scratch with
Auto Setup needs no mask script. Never commit `state.json`, a copy of `App_Data`, or the snapshots.
