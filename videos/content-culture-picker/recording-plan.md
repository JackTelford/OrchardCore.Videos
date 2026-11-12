# Recording plan: The Content Culture Picker

Read this file from top to bottom once, seed the site (section 2), then record the clips in the order of section 4. In
this file a selector in quotes stands for `scene.Page.Locator("...")`, since the recorder's methods take locators.
Source paths are relative to the Orchard Core checkout (`OrchardCMS/OrchardCore`) at the commit the video was written
from; `Name:line` points at the line where a label or a behavior comes from.

## 1. The site

- **Source:** `src/OrchardCore.Cms.Web` of an Orchard Core checkout, run on `http://localhost:5010`, with its own empty
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Outbound calls are blocked with `HTTP_PROXY` and
  `HTTPS_PROXY` pointing at a dead address. See the `orchardcore-tester` skill of that repository for building and running it.
- **Setup:** Auto Setup with the **Blog** recipe, no setup screen and no login form on camera. Environment variables
  (the password comes from `VIDEO_PASSWORD`, chosen outside git; write nothing else in its place):

  ```
  OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
  OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- **Sign-in:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` once, outside the capture.
  It saves `state.json` (git-ignored). Every clip runs signed in as `admin`, so the front end also shows the user menu
  and the theme toggle in its navigation bar, next to the picker.
- **Theme:** the picker is drawn by the `Navbar` shape in the display type `Detail` (`OrchardCore.ContentLocalization/Drivers/ContentCulturePickerNavbarDisplayDriver.cs:36-37`).
  Of the themes of the repository, only The Default Theme (`TheTheme`) renders that shape on the front end
  (`OrchardCore.Themes/TheTheme/Views/Layout.cshtml:54`); the Blog theme of the Blog recipe does not, and the admin
  theme renders `DetailAdmin`, so the picker never appears in the admin
  (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:16`). The seed therefore makes The Default Theme the site theme.
  The theme's name is "The Default Theme" (`OrchardCore.Themes/TheTheme/Manifest.cs:5`).
- **Features before recording:** Content Localization (and Localization, which it needs), Autoroute (the Blog recipe
  enables it), The Default Theme. **The Content Culture Picker feature stays disabled**: clip `02-enable` enables it.
- **Browser:** the recorder's default (1536x864, `en-US`, light). Each clip runs in a new browser context
  (`Scene.cs`, `NewContextAsync`), so no cookie and no cached redirect carries from one clip to the next.

## 2. Seed the site (outside the capture)

The site has two cultures, `en` (default) and `fr`; the content is a small cafe site with invented text. Everything is
fake: the only address is `hello@example.com`.

1. Start the site, wait for Auto Setup to finish, then run `Recorder setup`.
2. Open **Tools, Deployments, JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:18-19,30-34`; the page has a field labeled
   "JSON" and a button "Import", `OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`). Paste **recipe 1** into the
   field and select **Import**. The message "Recipe imported successfully!" appears
   (`OrchardCore.Deployment/Controllers/ImportController.cs:185`). Enabling the features restarts the site's tenant: reload
   `/Admin/Features` until it answers, and check that "Content Localization" shows Enabled.

   ```json
   {
     "steps": [
       {
         "name": "feature",
         "enable": [
           "OrchardCore.Localization",
           "OrchardCore.ContentLocalization",
           "TheTheme"
         ]
       }
     ]
   }
   ```

3. On the same page, paste **recipe 2** and select **Import**. It stores the cultures and the home page, makes The
   Default Theme the site theme, adds the Localization part to the Article content type, and creates five articles
   (Welcome and Bienvenue, Contact us and Nous contacter, and Opening hours, which has no French version).

   ```json
   {
     "steps": [
       {
         "name": "settings",
         "LocalizationSettings": {
           "DefaultCulture": "en",
           "SupportedCultures": [
             "en",
             "fr"
           ],
           "FallBackToParentCulture": false
         },
         "HomeRoute": {
           "Action": "Display",
           "Controller": "Item",
           "Area": "OrchardCore.Contents",
           "ContentItemId": "39e41475c202fcff826c1e3eec"
         }
       },
       {
         "name": "themes",
         "site": "TheTheme"
       },
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "Article",
             "ContentTypePartDefinitionRecords": [
               {
                 "PartName": "LocalizationPart",
                 "Name": "LocalizationPart",
                 "Settings": {}
               }
             ]
           }
         ]
       },
       {
         "name": "content",
         "data": [
           {
             "ContentItemId": "39e41475c202fcff826c1e3eec",
             "ContentItemVersionId": "2d57d76fb3d6048bb12dd91c6d",
             "ContentType": "Article",
             "DisplayText": "Welcome",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": {
               "Title": "Welcome"
             },
             "AutoroutePart": {
               "Path": "welcome",
               "SetHomepage": false
             },
             "HtmlBodyPart": {
               "Html": "<p>Welcome to Maple Street Cafe. We serve breakfast and lunch every day.</p><p><a href=\"/contact-us\">Contact us</a> | <a href=\"/opening-hours\">Opening hours</a></p>"
             },
             "LocalizationPart": {
               "LocalizationSet": "e9b1702bb1466859be63943293",
               "Culture": "en"
             }
           },
           {
             "ContentItemId": "71d8b5cc6dfecbc785e88a7f53",
             "ContentItemVersionId": "1ed11b37332494561d160ac3f2",
             "ContentType": "Article",
             "DisplayText": "Bienvenue",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": {
               "Title": "Bienvenue"
             },
             "AutoroutePart": {
               "Path": "bienvenue",
               "SetHomepage": false
             },
             "HtmlBodyPart": {
               "Html": "<p>Bienvenue au Maple Street Cafe. Nous servons le petit-déjeuner et le déjeuner tous les jours.</p><p><a href=\"/nous-contacter\">Nous contacter</a> | <a href=\"/opening-hours\">Heures d'ouverture</a></p>"
             },
             "LocalizationPart": {
               "LocalizationSet": "e9b1702bb1466859be63943293",
               "Culture": "fr"
             }
           },
           {
             "ContentItemId": "da71c1433329308627f0f8ff7e",
             "ContentItemVersionId": "c1f920014dbd019e9531ab3dc7",
             "ContentType": "Article",
             "DisplayText": "Contact us",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": {
               "Title": "Contact us"
             },
             "AutoroutePart": {
               "Path": "contact-us",
               "SetHomepage": false
             },
             "HtmlBodyPart": {
               "Html": "<p>Write to us at hello@example.com, or visit us during our opening hours.</p><p><a href=\"/opening-hours\">Opening hours</a></p>"
             },
             "LocalizationPart": {
               "LocalizationSet": "a37ade4f6e5ab7db36a8b77cd2",
               "Culture": "en"
             }
           },
           {
             "ContentItemId": "b39b140c4fcf7b16f7af70a91e",
             "ContentItemVersionId": "bce4e1e9ac443ec1ef39160f1f",
             "ContentType": "Article",
             "DisplayText": "Nous contacter",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": {
               "Title": "Nous contacter"
             },
             "AutoroutePart": {
               "Path": "nous-contacter",
               "SetHomepage": false
             },
             "HtmlBodyPart": {
               "Html": "<p>Écrivez-nous à hello@example.com, ou passez nous voir pendant nos heures d'ouverture.</p><p><a href=\"/opening-hours\">Heures d'ouverture</a></p>"
             },
             "LocalizationPart": {
               "LocalizationSet": "a37ade4f6e5ab7db36a8b77cd2",
               "Culture": "fr"
             }
           },
           {
             "ContentItemId": "f5947e66147390029e4492b220",
             "ContentItemVersionId": "1659a945a34472ec9e9b737f69",
             "ContentType": "Article",
             "DisplayText": "Opening hours",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": {
               "Title": "Opening hours"
             },
             "AutoroutePart": {
               "Path": "opening-hours",
               "SetHomepage": false
             },
             "HtmlBodyPart": {
               "Html": "<p>We are open every day from 7:00 to 17:00.</p><p><a href=\"/\">Back to the home page</a></p>"
             },
             "LocalizationPart": {
               "LocalizationSet": "63a3ef9efb983076f5889aca45",
               "Culture": "en"
             }
           }
         ]
       }
     ]
   }
   ```

4. **Stop the site and start it again** (stop it by its process ID). The picker looks pages up in an in-memory list of
   translations that is loaded when the site starts; a restart makes sure that the imported articles are in it.
5. **Check the content** (the picker is still disabled, so no language menu is expected): open each address and look at
   the page title and the text.

   | Address | Title (h1) | Text starts with |
   |---|---|---|
   | `/` | Welcome | Welcome to Maple Street Cafe. |
   | `/welcome` | Welcome | Welcome to Maple Street Cafe. |
   | `/bienvenue` | Bienvenue | Bienvenue au Maple Street Cafe. |
   | `/contact-us` | Contact us | Write to us at hello@example.com |
   | `/nous-contacter` | Nous contacter | Écrivez-nous à hello@example.com |
   | `/opening-hours` | Opening hours | We are open every day from 7:00 to 17:00. |

   The navigation bar shows the Blog recipe's main menu, **Home** and **About**. Leave it as it is: it is not part of
   the story and the narration never mentions it. In **Settings, Localization, Cultures** two cultures are supported,
   English and French, and English is the default.
6. **Snapshot A.** Stop the site and copy its `App_Data` folder and `state.json` aside. This is the state `02-enable`
   starts from: the picker is **disabled**.

**Dry run of the picker (recommended, never for a take).** Enable the Content Culture Picker feature, then, in a
private window that is thrown away afterwards: on `/` choose the French entry in the language menu and expect the
page "Bienvenue"; on `/nous-contacter` choose the English entry and expect "Contact us"; on `/opening-hours` choose
the French entry and expect to stay on "Opening hours". Restore snapshot A afterwards. The picker redirects with a
**permanent** redirect (`OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:79`), which a
browser keeps in its cache. A private window that is closed after the dry run, and the new context of every clip,
keep that cache out of the takes. Within one clip never choose the same language twice on the same page; this plan
never does.

## 3. What the screen shows, in short

- The picker is an item of the front end's navigation bar: a toggle with the id `oc-culture-picker` whose text is the
  name of the current culture in its own language, and a drop-down list of the other supported cultures, each an item
  with the class `dropdown-item` (`OrchardCore.ContentLocalization/Views/ContentCulturePicker.cshtml:2-16`). The names come
  from the .NET culture data (`NativeName`): expect **English** and **français**. A person confirms the exact texts on
  the running site (see `OPEN-QUESTIONS.md`); the narration says "English" for the first and "français" for the second.
- The picker is hidden when the site supports one culture only (`ContentCulturePickerNavbarDisplayDriver.cs:36`).
- The culture of a translated page comes from the page itself (`OrchardCore.ContentLocalization/ContentRequestCultureProvider.cs:15-31`),
  so on `/bienvenue` the toggle reads français and on `/` it reads English.
- Define once, in the scene methods of the clips that use them:
  `picker = scene.Page.Locator("#oc-culture-picker")`,
  `pickerMenu = scene.Page.Locator(".dropdown-menu[aria-labelledby='oc-culture-picker']")`,
  `pickerItems = pickerMenu.Locator("a.dropdown-item")`.

## 4. Order of the clips, and what changes the site

| Clip | Starts from | Changes the site? |
|---|---|---|
| `02-enable` | snapshot A (picker disabled) | **Yes**: enables the Content Culture Picker feature. Take **snapshot B** after it: stop the site, copy `App_Data` and `state.json` aside. |
| `03-switch` | snapshot B | No. |
| `04-missing` | snapshot B | No. |
| `05-homepage` | snapshot B (Homepage redirect not selected) | **Yes**: selects and saves "Homepage redirect". Take **snapshot C** after it. |
| `06-cookie` | snapshot C | No. |

Record in the order 02, 03, 04, 05, 06. Clips 03 and 04 do not depend on each other. To record one clip again, restore
the snapshot in the second column first (stop the site, copy `App_Data` and `state.json` back, start the site). To
record everything again, restore snapshot A and start over at 02. The slide chapter 01 needs no recording.

## 5. Chapter 01: slides (not recorded)

One line per slide step; `slides.py` draws them, the recorder does nothing.

- `how-1`: "The language menu", four cards: a menu in the navigation bar, the button shows the current language, the menu lists the other languages, a language is a culture.
- `how-2`: "What the picker relies on", four cards: two or more supported cultures, translated pages, a path for each page, a theme with the navigation bar.
- `how-3`: "Where the visitor lands", three cards: the same page in that language, the homepage in that language, the page the visitor is on.

## 6. Clip `02-enable`

**Page it starts on:** `/Admin` (the admin home), opened with `OpenAsync` before `StartCaptureAsync`. The admin menu has no group open.
Define once: `row = scene.Page.Locator("[data-filter-value]").Filter(new() { HasText = "Content Culture Picker" })`.

- `enable-1` (narration: open Tools, Features, search for content culture).
  1. `ClickAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true }))`. The group opens.
  2. `ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true }))`. Wait until `#search-box` is visible.
  3. `TypeAsync(scene.Page.Locator("#search-box"), "content culture", 60)`. Wait until the heading "Internationalization" is visible and `row` is visible.
  - **On screen when the narration ends:** the Features page; the search field holds "content culture"; one group, "Internationalization", with one row, "Content Culture Picker", description "Provides a culture picker shape for the frontend." and a button "Enable".
  - **Labels:** Tools and Features `OrchardCore.Features/AdminMenu.cs:26-27`; search field `OrchardCore.Features/Views/Admin/Features.cshtml:27`; group heading `Features.cshtml:88`; feature name and description `OrchardCore.ContentLocalization/Manifest.cs:22-23`; category `Manifest.cs:25`; filter `OrchardCore.Features/Assets/ts/features-index.ts:97-101`.
- `enable-2` (narration: the row's description and dependencies).
  1. `HighlightAsync(row.Locator(".admin-list-secondary"))` for the description.
  2. After about 3 seconds `ClearHighlightsAsync()`, then `HighlightAsync(row.Locator(".direct-dependencies"))` for the dependency badges.
  - **On screen when the narration ends:** the same row, with a green ring around the two badges "Autoroute" and "Content Localization" (in that order: the page sorts the dependencies by name, `OrchardCore.Features/Controllers/AdminController.cs:134-139`). A small "..." button for the indirect dependencies sits after the badges and stays closed.
  - **Labels:** description `OrchardCore.Features/Views/FeatureEntry.Description.SummaryAdmin.cshtml:8`; badges `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-18` (the `direct-dependencies` block); dependency names `OrchardCore.ContentLocalization/Manifest.cs:24` and `OrchardCore.Autoroute/Manifest.cs:4`.
- `enable-3` (narration: select Enable, the message, the row shows Enabled). Do the click as the first action of the step, with no pause before it.
  1. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true }))`. The Features page reloads, with every feature listed.
  2. Wait until a toast with the text "The feature Content Culture Picker was enabled." is visible.
  3. `TypeAsync(scene.Page.Locator("#search-box"), "content culture", 60)` so that the row is on screen again. The row now has the badge "Enabled" and a button "Disable" in place of "Enable" (`FeatureEntry.Buttons.SummaryAdmin.cshtml:43-54`); do not click it.
  - **On screen when the narration ends:** the filtered list with the row "Content Culture Picker" marked "Enabled" (green check) and the toast with the message, if it has not closed yet. The toast can close by itself after a few seconds (`TheAdmin/Views/Message.cshtml`); the narration says "A message confirms", so the message must be visible for part of the narration. If it was never seen on camera, record the clip again.
  - **Labels:** button `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message `OrchardCore.Features/Controllers/AdminController.cs:317`; badge `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

After recording: stop the site and save **snapshot B**.

## 7. Clip `03-switch`

**Page it starts on:** `/Admin/Features`, opened with `OpenAsync` before the capture (signed in; the picker feature is enabled).

- `switch-1` (narration: select Visit Site; the home page in English; the menu shows the current language).
  1. `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Visit Site", Exact = true }))` (an icon in the admin's top bar, an `a` element with `role="button"` and the title "Visit Site", `OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`). Do not select it with `a[title='Visit Site']`: the admin theme turns the element into a Bootstrap tooltip when the page loads (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:190-193`), which removes the `title` attribute and sets `aria-label="Visit Site"` instead (`OrchardCore.Resources/wwwroot/Scripts/bootstrap.bundle.js`, `_fixTitle`), so the role and name stay the same before and after. The front end opens at `/`.
  2. Wait until `h1` has the text "Welcome" and `picker` is visible.
  3. `HighlightAsync(picker)`.
  - **On screen when the narration ends:** the page "Welcome" with its text and two links, "Contact us" and "Opening hours"; in the navigation bar a green ring around the toggle reading **English**.
- `switch-2` (narration: open the menu; the other language under its own name).
  1. `ClearHighlightsAsync()`, then `ClickAsync(picker)`.
  2. Wait until `pickerMenu` is visible.
  3. `HighlightAsync(pickerItems)`.
  - **On screen when the narration ends:** the open menu with one entry, **français**, in a green ring. The current culture is not in the list (`ContentCulturePicker.cshtml:6-9`).
- `switch-3` (narration: choose français; the French home page opens; the menu shows français).
  1. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync(pickerItems.Filter(new() { HasText = "français" }))`.
  2. Wait until `h1` has the text "Bienvenue" and `picker` has the text "français".
  - **On screen when the narration ends:** the page "Bienvenue", its French text, two links "Nous contacter" and "Heures d'ouverture", and the toggle reading **français**. The picker's action is `OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:30-80`.
- `switch-4` (narration: the same on every translated page; follow the link Nous contacter).
  1. `ClickAndWaitForNavigationAsync(scene.Page.Locator("main").GetByRole(AriaRole.Link, new() { Name = "Nous contacter", Exact = true }))`.
  2. Wait until `h1` has the text "Nous contacter".
  - **On screen when the narration ends:** the page "Nous contacter" with its French text; the toggle reads **français**.
- `switch-5` (narration: choose English; the English contact page opens).
  1. `ClickAsync(picker)`, wait for `pickerMenu`, `PauseAsync(600)`.
  2. `ClickAndWaitForNavigationAsync(pickerItems.Filter(new() { HasText = "English" }))`.
  3. Wait until `h1` has the text "Contact us".
  - **On screen when the narration ends:** the page "Contact us" and its English text, with the link "Opening hours"; the toggle reads **English**.

This clip picks français on `/` and English on `/nous-contacter`: two different links, so no cached redirect is replayed.

## 8. Clip `04-missing`

**Page it starts on:** `/contact-us`, opened with `OpenAsync` before the capture. The toggle reads English.

- `missing-1` (narration: some pages exist in one language only; follow the link to Opening hours).
  1. `ClickAndWaitForNavigationAsync(scene.Page.Locator("main").GetByRole(AriaRole.Link, new() { Name = "Opening hours", Exact = true }))`.
  2. Wait until `h1` has the text "Opening hours".
  - **On screen when the narration ends:** the page "Opening hours", the text "We are open every day from 7:00 to 17:00." and the link "Back to the home page"; the toggle reads **English**.
- `missing-2` (narration: choose français; no French version; the visitor stays on the page in English).
  1. `ClickAsync(picker)`, wait for `pickerMenu`, `PauseAsync(600)`.
  2. `ClickAndWaitForNavigationAsync(pickerItems.Filter(new() { HasText = "français" }))`.
  3. Wait until `h1` has the text "Opening hours", then `PauseAsync(1200)` so that the viewer sees that nothing changed.
  - **On screen when the narration ends:** still the page "Opening hours" in English; the toggle still reads **English**. "Homepage redirect" is not selected, so the picker has no other place to go (`ContentCulturePickerController.cs:54`).

## 9. Clip `05-homepage`

**Page it starts on:** `/Admin`, opened with `OpenAsync` before the capture. The admin menu has no group open.

- `homepage-1` (narration: open Settings, Localization, Content Culture Picker).
  1. `ClickAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "Settings", Exact = true }))`.
  2. `ClickAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "Localization", Exact = true }))`.
  3. `ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Content Culture Picker", Exact = true }))`.
  4. Wait until `scene.Page.GetByLabel("Homepage redirect")` is visible.
  - **On screen when the narration ends:** the page with the heading "Settings" (the heading of every settings page, `OrchardCore.Settings/Views/Admin/Index.cshtml:4`), two options, "Set Cookie" (selected) and "Homepage redirect" (not selected), each with its hint, and a "Save" button; in the menu "Settings" and "Localization" are open and "Content Culture Picker" is the current item.
  - **Labels:** menu `OrchardCore.ContentLocalization/AdminMenu.cs:32-41`; options `Views/ContentCulturePickerSettings.Edit.cshtml:8,18` (the label is written "Homepage redirect " with a trailing space; the screen shows "Homepage redirect"); hints `ContentCulturePickerSettings.Edit.cshtml:10,20`; defaults `Models/ContentCulturePickerSettings.cs:5-6`.
- `homepage-2` (narration: two options; Homepage redirect opens the homepage in the chosen language; select it).
  1. `HighlightAsync(scene.Page.GetByLabel("Homepage redirect"))` for about 2 seconds, then `ClearHighlightsAsync()`.
  2. `ClickAsync(scene.Page.GetByLabel("Homepage redirect"))`. Wait until it is checked.
  - **On screen when the narration ends:** "Homepage redirect" selected, with its hint "Redirects to the homepage of the target culture if the ContentItem of the target culture does not exist."
- `homepage-3` (narration: select Save; a message confirms).
  1. `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))`.
  2. Wait until a toast with the text "Site settings updated successfully." is visible.
  - **On screen when the narration ends:** the settings page again, "Homepage redirect" still selected, and the toast (it can close by itself after a few seconds; it must be seen on camera). **Labels:** `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message `OrchardCore.Settings/Controllers/AdminController.cs:119`.
- `homepage-4` (narration: visit the site again; follow the link to Opening hours).
  1. `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Visit Site", Exact = true }))` (the same element as in `switch-1`). Wait until `h1` has the text "Welcome".
  2. `ClickAndWaitForNavigationAsync(scene.Page.Locator("main").GetByRole(AriaRole.Link, new() { Name = "Opening hours", Exact = true }))`. Wait until `h1` has the text "Opening hours".
  - **On screen when the narration ends:** the page "Opening hours" in English; the toggle reads **English**.
- `homepage-5` (narration: choose français; the French homepage Bienvenue opens).
  1. `ClickAsync(picker)`, wait for `pickerMenu`, `PauseAsync(600)`.
  2. `ClickAndWaitForNavigationAsync(pickerItems.Filter(new() { HasText = "français" }))`.
  3. Wait until `h1` has the text "Bienvenue".
  - **On screen when the narration ends:** the page "Bienvenue" in French; the toggle reads **français**. This is the only time the clip picks français on "Opening hours"; the clip `04-missing` did it in another browser context.

After recording: stop the site and save **snapshot C**.

## 10. Clip `06-cookie`

**Page it starts on:** `/Admin/Settings/ContentCulturePicker`, opened with `OpenAsync` before the capture. "Set Cookie" and "Homepage redirect" are both selected (the state `05-homepage` left).

- `cookie-1` (narration: both pages have Set Cookie; here it is selected by default).
  1. `HighlightAsync(scene.Page.GetByLabel("Set Cookie"))`.
  - **On screen when the narration ends:** the Content Culture Picker settings with "Set Cookie" selected, in a green ring.
  - **Labels:** `OrchardCore.ContentLocalization/Views/ContentCulturePickerSettings.Edit.cshtml:8`; default `Models/ContentCulturePickerSettings.cs:6`.
- `cookie-2` (narration: the picker stores the choice in a cookie; 14 days; scripts cannot read it).
  1. `ClearHighlightsAsync()`, then `HighlightAsync(scene.Page.Locator(".hint").Filter(new() { HasText = "Sets the localization cookie when switching cultures" }))`, the hint under "Set Cookie".
  - **On screen when the narration ends:** the hint "Sets the localization cookie when switching cultures using the ContentCulturePicker" in a green ring.
  - **Behavior:** cookie lifetime `OrchardCore.ContentLocalization/CulturePickerOptions.cs:5` (14 days); `HttpOnly` `Services/ContentCulturePickerService.cs:114`.
- `cookie-3` (narration: open Settings, Localization, Content Culture; its Set Cookie is not selected by default).
  1. `ClearHighlightsAsync()`. The menu groups "Settings" and "Localization" are already open on this page: `ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Content Culture", Exact = true }))`. (If the groups are closed, click the buttons "Settings" and "Localization" first, as in `homepage-1`.)
  2. Wait until `scene.Page.GetByLabel("Set Cookie")` is visible.
  - **On screen when the narration ends:** the Content Culture settings with one option, "Set Cookie", **not** selected.
  - **Labels:** menu `OrchardCore.ContentLocalization/AdminMenu.cs:34`; option `Views/ContentRequestCultureProviderSettings.Edit.cshtml:8`; default `Models/ContentRequestCultureProviderSettings.cs:5`.
- `cookie-4` (narration: when selected, the site stores the language of any page with a Localization part).
  1. `HighlightAsync(scene.Page.Locator(".hint").Filter(new() { HasText = "Sets the localization cookie based on the LocalizationPart culture" }))`.
  - **On screen when the narration ends:** the hint "Sets the localization cookie based on the LocalizationPart culture when accessing a page in the frontend." in a green ring. Do **not** select the option, and do not save.
  - **Behavior:** `OrchardCore.ContentLocalization/ContentRequestCultureProvider.cs:15-31`. The cookie is set whenever the address of the request belongs to a content item that has a Localization part with a set and a culture (lines 21-29), so "Opening hours", which has no translation, counts too.
- `cookie-5` (narration: both pages are available only to users who have the permission Manage ContentCulturePicker settings; the Administrator role has it). No action: the step holds until the narration ends.
  1. `ClearHighlightsAsync()` at the start of the step.
  - **On screen when the narration ends:** the same page, no highlight.
  - **Permission text and roles:** `OrchardCore/OrchardCore.ContentLocalization.Abstractions/ContentLocalizationPermissions.cs:12`; defaults `OrchardCore.ContentLocalization/Permissions.cs:26-27` (Administrator) and `:31-32` (Editor). Both display drivers return nothing without the permission (`Drivers/ContentCulturePickerSettingsDriver.cs:33-36`, `Drivers/ContentRequestCultureProviderSettingsDriver.cs:33-36`), so the page is not available. The narration names the Administrator role only: the Editor role also holds this permission by default, but the settings controller asks for the general permission to manage settings first (`OrchardCore.Settings/Controllers/AdminController.cs:53`), and the module registers no permission of its own for these two settings groups. See `OPEN-QUESTIONS.md`.

## 11. Checks for private data

- Every name, text and address on screen is invented: "Maple Street Cafe", "hello@example.com" and the site name
  "Orchard Core". The user is `admin`; its email `admin@example.com` is never on screen in these clips.
- The password is typed by nobody on camera, written in no file of the repository, and passed in `VIDEO_PASSWORD`.
- `state.json`, the snapshots of `App_Data` and the recordings stay outside git.
- OCR the video once per second and at every scene change, as the skill describes, and search the text for anything that
  is not in the list above. The language menu, the page texts and the two settings pages are the only content.
- Look at every contact sheet: the frame at the end of each step must match the "On screen" line above, with no error
  message and no empty page.
