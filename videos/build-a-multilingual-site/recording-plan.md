# Recording plan: Build a Multilingual Site

How to record this video without deciding anything. Read `OPEN-QUESTIONS.md` first: it lists what was read in the
source but never seen on a running site, and what to confirm in the first take.

Source paths below are relative to `src/OrchardCore.Modules/` of the `OrchardCMS/OrchardCore` checkout, unless they
start with `src/`. A selector written as `#search-box` stands for `scene.Page.Locator("#search-box")`. "Menu" means
the left admin menu of TheAdmin; "Enter" means `scene.Page.Keyboard.PressAsync("Enter")`.

## The site it needs

| What | Value |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of the OrchardCore checkout, on `http://localhost:5010`, its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`), outbound calls blocked with a dead `HTTP_PROXY`/`HTTPS_PROXY` (see `.agents/skills/orchardcore-video-producer/references/recording.md`) |
| Provisioning | Auto Setup, **Blog** recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles` |
| Admin user | `admin`, email `admin@example.com`, password `<password>`: chosen outside git and passed in `VIDEO_PASSWORD`. Never write it in a file of this repository |
| Computer language | The **installed UI language of the operating system** must be English (United States). This is `CultureInfo.InstalledUICulture`, the language Windows was installed with, which can differ from the display language chosen in the Windows settings. Check it in a terminal: `[System.Globalization.CultureInfo]::InstalledUICulture.Name` must print `en-US`. The first row of the culture table is this culture (`OrchardCore.Localization/Models/LocalizationSettings.cs:97-108`), and `cultures-1` names it `en-US` |
| Theme at the start | The Blog Theme, as the Blog recipe sets it (`blog.recipe.json:85`) |
| Features at the start | Only those of the Blog recipe. `OrchardCore.Localization`, `OrchardCore.ContentLocalization` and `OrchardCore.ContentLocalization.ContentCulturePicker` are **disabled**: clip `02-enable` enables them on camera |
| Settings | None changed. The recorder signs in once with `Recorder setup` (user `admin`) and saves the sign-in as `state.json`; that file is copied aside with `App_Data` (see Snapshots) and is never committed |
| External services | None. Nothing leaves the machine |
| Browser | The recorder's Chromium at 1536x864, device scale 1.25, `en-US`, light color scheme (`references/specs.md`). Do not change the browser language: the culture picker lists cultures by their own names, and the visitor's browser language must not decide the culture of a page |

### Seed data (create it outside the capture, before the first snapshot)

The Blog recipe creates a page "About" of type Article with placeholder Latin text (`blog.recipe.json:1062-1081`).
Replace the text with fake English text, once, before the Localization features are enabled:

1. Sign in as `admin`. Open **Content, Content Items**, press **Edit** on the row **About**.
2. Click into the body editor, select all (Control+A), and type:
   `We run a small garden café. The menu follows the seasons, and every dish is made with local produce.`
3. Leave the title `About` and the permalink `about` as they are. Press **Publish**.

No other content is created or edited. Everything on screen is fake: the site "Orchard Core", the user `admin`, the
café text, and the recipe's own sample posts.

### Snapshots

Stop the site after the seed and copy its `App_Data` folder and `state.json` aside. Restore that copy before every
full take. To record one clip again, restore the copy taken **before** that clip (the table below) and start the site.

| Copy | Taken | Holds |
|---|---|---|
| `S0` | After the seed | Blog recipe, About text in English, no localization feature |
| `S1` | After `02-enable` | The three features enabled |
| `S2` | After `03-cultures` | `S1` plus the cultures en-US (default) and fr-FR |
| `S3` | After `04-part` | `S2` plus the Localization part on Article |
| `S4` | After `05-translate` | `S3` plus About published with a localization set, and the published French copy `À propos` |
| `S5` | After `06-picker` | `S4` plus The Default Theme as the site theme |

## Order of the clips

The clips depend on each other. Record them in this order, each one starting from the copy before it:

| Order | Clip | Starts from | Needs | What it changes on the site |
|---|---|---|---|---|
| 1 | `02-enable` | `S0` | nothing | Enables the three features |
| 2 | `03-cultures` | `S1` | `02-enable` | Saves the culture settings (en-US default, fr-FR added) |
| 3 | `04-part` | `S2` | `02-enable` | Adds the Localization part to the type Article |
| 4 | `05-translate` | `S3` | `03-cultures`, `04-part` | Publishes About again (it receives a localization set and the culture en-US), creates and publishes the French copy |
| 5 | `06-picker` | `S4` | `02-enable` | Makes The Default Theme the site theme; changes no setting of the picker |
| 6 | `07-try` | `S5` | `03-cultures`, `05-translate`, `06-picker` | Nothing on the site; it only writes a culture cookie in the recorder's own browser, which is discarded with the clip |

`03-cultures` and `04-part` do not depend on each other: either can come first. `01-idea` and `08-rules` are slides and
need no site. Run the whole sequence on one site and restart nothing in between except what a clip itself triggers
(enabling a feature and saving the culture settings each restart the tenant; wait for the page to come back).

## Clips

### `02-enable` (chapter 02, Enable the features)

Starts on `/Admin` (the dashboard). Open it before `StartCaptureAsync`.

- `enable-1`: click the menu entry **Tools**, then the entry **Features** (`ClickAndWaitForNavigationAsync`; the page
  is `/Admin/Features`). Click `#search-box` and type `localization`. Wait for this condition: at least one row is shown and
  every shown row matches the search, i.e. `(() => { const rows = [...document.querySelectorAll('[data-filter-value]:not(.d-none)')]; return rows.length > 0 && rows.every(r => r.dataset.filterValue.toLowerCase().includes('localization')); })()`.
  (A condition that only compares the number of shown rows with the total is already true when the page loads, because
  the filters **Hide on demand** and **Hide always enabled** are on by default: `features-index.ts` applies them at load.)
  On screen at the end: the page **Features**, the search box holding `localization`, and a short list that contains the
  rows **Localization**, **Content Localization** and **Content Culture Picker**, each marked Disabled.
  Labels: `OrchardCore.Features/AdminMenu.cs:26-27`; search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`;
  the search text matches name, id and description (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`).
- `enable-2`: highlight the row of Content Culture Picker (`[data-filter-value*='ContentCulturePicker']`), then move to
  its **Enable** link (`[data-filter-value*='ContentCulturePicker'] a.btn-primary` with the text `Enable`). The row shows
  its **direct** dependencies as small badges: **Autoroute** (already enabled by the Blog recipe) and **Content
  Localization** (`OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-18`,
  `OrchardCore.Features/Controllers/AdminController.cs:152`). Highlight the badge **Content Localization**. **Localization**
  is an indirect dependency, hidden behind the ellipsis button "Show indirect dependencies": do not click that button, and do
  not expect the word Localization on the row of Content Culture Picker. Content Localization lists Localization as a
  direct dependency (`OrchardCore.ContentLocalization/Manifest.cs:16`, and `:24` for Content Culture Picker).
  Click **Enable** after about 6 seconds of the step. The click posts, the tenant restarts and the page loads again: wrap
  the wait in `scene.OffCameraAsync(...)` with the condition "the page shows the text `was enabled`". On screen at the
  end: the Features page again, with the green message "The feature Content Culture Picker was enabled."
  Labels: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message
  `OrchardCore.Features/Controllers/AdminController.cs:317`. The dependencies are enabled by the same click:
  `OrchardCore.Features/Controllers/AdminController.cs:227-250` passes `force`, and
  `src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs` (`GetFeaturesToEnable`) adds the dependencies.
- `enable-3`: the search box is empty after the reload. Click `#search-box` and type `localization`; wait for the same
  condition as in `enable-1`. Highlight the **Enabled** badge of each of the three rows (`Localization`,
  `Content Localization`, `Content Culture Picker`). On screen at the end: the three rows, each with the badge
  **Enabled** and a **Disable** button. Label: `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

### `03-cultures` (chapter 03, Add the cultures)

Starts on `/Admin`.

- `cultures-1`: click the menu entry **Settings**, then **Localization**, then **Cultures** (`ClickAndWaitForNavigationAsync`
  on the last click; the page is `/Admin/Settings/localization`). Wait until `.localization-settings-wrapper table tbody tr`
  exists exactly once. Highlight the table. On screen at the end: the yellow warning "The website might be restarted upon
  saving the settings, potentially leading to temporary unresponsiveness during the process.", the label "Define which
  cultures are supported by the site.", a table with the columns **Culture** and **Default culture** and one row
  `en-US` whose radio button is selected, a drop-down list with a green **Add culture** button, and the check box
  "Fall back to parent culture(s)". Labels: `OrchardCore.Localization/AdminMenu.cs:34-45`;
  `OrchardCore.Localization/Views/LocalizationSettings.Edit.cshtml:31,42`;
  `OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`; warning `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`;
  the table and the buttons are drawn by the template inside `OrchardCore.Localization/wwwroot/Scripts/localization-settings/localization-settings.js`.
- `cultures-2`: select the option with the value `fr-FR` in `.localization-settings-wrapper select`
  (`SelectAsync(select, "fr-FR")`; its text is `fr-FR (French (France))`). Click the green link **Add culture**
  (`.localization-settings-wrapper a.btn-success`). Wait until the table has two rows. On screen at the end: rows `en-US`
  and `fr-FR`; the radio button of `en-US` is still selected.
- `cultures-3`: highlight the column header **Default culture** and the selected radio button of the `en-US` row
  (`#customRadio_0`). Do not click anything. On screen at the end: the two rows and the selected radio button of `en-US`.
- `cultures-4`: scroll to the end of the form, click the blue **Save** button (`button.save`,
  `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`) with `ClickAndWaitForNavigationAsync`. Wait until the
  text `Site settings updated successfully.` is on the page (wrap the wait in `scene.OffCameraAsync`). On screen at the
  end: the page reloaded, the two messages "Site settings updated successfully." and "The site has been restarted for
  the settings to take effect.", and the table with the rows `en-US` and `fr-FR`. Labels: `OrchardCore.Settings/Controllers/AdminController.cs:119`;
  `OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132`.

### `04-part` (chapter 04, Make a type translatable)

Starts on `/Admin`.

- `part-1`: click the menu entry **Design**, then **Content Definition**, then **Content Types**
  (`ClickAndWaitForNavigationAsync` on the last click; `/Admin/ContentTypes/List`). Click the **Edit** button of the row
  Article (`[role='btn-edit-Article']`, with `ClickAndWaitForNavigationAsync`). On screen at the end: the page "Edit
  Content Type - Article", with the section **Parts** (TitlePart, AutoroutePart and the others). Labels:
  `OrchardCore.ContentTypes/AdminMenu.cs:23-25`; `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`;
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml` (breadcrumb and heading **Parts**).
- `part-2`: scroll to the heading **Parts**, click the button **Add Parts** (`a` with the text `Add Parts`,
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201`) with `ClickAndWaitForNavigationAsync`. The page says "Choose
  the Parts to add to this Content Type." Click the check box of the entry **Localization** (the `label` with the text
  `Localization`; its hint reads "Provides a way to create localized version of content."). The button **Add Selected Parts**
  (`#add-selected-parts-button`) appears. Click it at the end of the step (`ClickAndWaitForNavigationAsync`). On screen
  at the end: the page "Edit Content Type - Article" again. Labels: `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18,33,47`;
  the entry is named from `LocalizationPart` without the word Part (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`);
  hint `OrchardCore.ContentLocalization/Migrations.cs:25-26`.
- `part-3`: highlight the green message "The "LocalizationPart" part has been added." and the new list entry
  **Localization** in the list **Parts** (`#parts`). Scroll to the end of the page and click the green **Save** button
  (`button.save`, with `ClickAndWaitForNavigationAsync`) near the end of the step. On screen at the end: the Content Types
  list with the green message "Content type updated successfully.". Labels: `OrchardCore.ContentTypes/Controllers/AdminController.cs:372`
  (the message names the part with its technical name `LocalizationPart`); `OrchardCore.ContentTypes/Controllers/AdminController.cs:225`;
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:212`.

### `05-translate` (chapter 05, Translate a page)

Starts on `/Admin`.

- `translate-1`: click the menu entry **Content**, then **Content Items** (`ClickAndWaitForNavigationAsync`;
  `/Admin/Contents/ContentItems`). Do not search: the seed holds fewer than ten items, so one page shows every row.
  Highlight the row **About**. On screen at the end: the list titled "Manage Content" with the row **About** (type Article)
  and its **Edit** button. Labels: `OrchardCore.Contents/AdminMenu.cs:65-68`; `OrchardCore.Contents/Views/Admin/List.cshtml:17`;
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`.
- `translate-2`: click the **Edit** button of the row About (`ClickAndWaitForNavigationAsync`). Scroll down until the
  collapsed panel `#contentLocalizationToggle` is on screen and highlight it. On screen at the end: the editor "Edit
  Article" scrolled to the panel whose header reads "Current Culture: English (United States) | en-US". Labels:
  `OrchardCore.ContentLocalization/Views/LocalizationPart.Edit.cshtml:12` (the text is built from `CultureInfo.DisplayName`
  and `Name`, so it depends on the machine: see `OPEN-QUESTIONS.md`).
- `translate-3`: click the panel header (`#contentLocalizationToggle button`) to open it. The text "Please save your
  content item before creating localizations" is shown. Scroll to the end of the editor and click the green **Publish**
  button (`button.publish`) near the end of the step, with `ClickAndWaitForNavigationAsync`. The page returns to the list
  because the editor was opened from it with a return address. On screen at the end: the list with the message "Your
  Article has been published." Labels: `OrchardCore.ContentLocalization/Views/LocalizationPart.Edit.cshtml:45`;
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`; `OrchardCore.Contents/Controllers/AdminController.cs:473`;
  the set is created when the item is saved (`OrchardCore.ContentLocalization/Drivers/LocalizationPartDisplayDriver.cs:58`).
- `translate-4`: the row About now has a badge with a globe and the text `en-US`, and a blue button **Localizations**.
  Highlight the badge (it is the one with the tooltip "Culture") and the button, then click **Localizations**
  (`button.localizations`). The menu lists one entry, "French (France) | fr-FR", with a plus sign. Highlight the plus sign
  of that entry. **Do not click the entry in this step.** On screen at the end: the open menu with the entry "French
  (France) | fr-FR". Labels: `OrchardCore.ContentLocalization/Views/LocalizationPart.SummaryAdmin.cshtml:5`;
  `OrchardCore.ContentLocalization/Views/LocalizationPart.SummaryAdminLinks.cshtml:7-9` (the plus sign marks a culture
  without a translation, line 21; the pencil marks one with a translation, line 17).
- `translate-5`: click the entry "French (France) | fr-FR" (`.dropdown-menu a.dropdown-item:has-text('fr-FR')`; select it by
  its text, not by its `title` attribute, which the view writes without quotes, so it may render as the single word
  `Create`: `LocalizationPart.SummaryAdminLinks.cshtml:21`) with `ClickAndWaitForNavigationAsync`. The link posts a form
  (`data-url-af="UnsafeUrl"`, handled by `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts`) and opens no
  confirmation dialog. The page opens the editor of the copy and shows the message "Localized version of the
  content created successfully." Scroll down to `#contentLocalizationToggle`, click it to open the panel, and highlight
  the list inside it. On screen at the end: the panel header "Current Culture: French (France) | fr-FR" and one entry
  below it, "English (United States) | en-US", with a pencil. Labels: `OrchardCore.ContentLocalization/Controllers/AdminController.cs:76`;
  `OrchardCore.ContentLocalization/Views/LocalizationPart.Edit.cshtml:12,34`; the copy is made by
  `OrchardCore.ContentLocalization/DefaultContentLocalizationManager.cs:94`.
- `translate-6`: scroll to the top of the editor. In the field **Title** (`TitlePart`, label
  `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`) select all and type `À propos`. In the field **Permalink**
  (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`) select all and type `a-propos`; the copy starts with a path
  that is a unique variant of `about` (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:158`). Click into the body
  editor (`.trumbowyg-editor`), select all (Control+A) and type, with a delay of 25 ms per character:
  `Nous tenons un petit café-jardin. Notre carte suit les saisons.` Do not change any other field. On screen at the end:
  the title `À propos`, the permalink `a-propos` and the French text in the body.
- `translate-7`: scroll to the end of the editor and click the green **Publish** button near the start of the step, with
  `ClickAndWaitForNavigationAsync`. On screen at the end: the list with the message "Your Article has been published.",
  and two Article rows: **À propos** with the badge `fr-FR` and **About** with the badge `en-US`, each with a
  **Localizations** button. Highlight the two badges. Labels as in `translate-3`.
- `translate-8`: click the filter button titled **Culture** in the toolbar above the list (a bootstrap-select drop-down:
  `.bootstrap-select.culture-selectpicker button.dropdown-toggle`, or `button[title='Culture']`). The drop-down shows the
  header "Filter by culture", a search box, and the entries "All cultures", `en-US` and `fr-FR`. Click the entry `fr-FR`
  inside `scene.WaitForNavigationAsync(...)`: choosing an entry submits the filter by itself, because
  `contents-admin-list.js` listens to `changed.bs.select` on every `.selectpicker` that does not have the class `nosubmit`
  and clicks the hidden `submit.Filter` button; the culture filter has no `nosubmit`. Do not click the magnifier button.
  On screen at the end: the list with the single row **À propos** (the row **About** is gone). Labels: `OrchardCore.ContentLocalization/Views/ContentsAdminList-LocalizationPartFilter.cshtml:4`;
  `OrchardCore.ContentLocalization/Drivers/LocalizationContentsAdminListDisplayDriver.cs:43`.

### `06-picker` (chapter 06, Add the culture picker)

Starts on `/Admin`.

- `picker-1`: click the menu entry **Design**, then **Themes** (`ClickAndWaitForNavigationAsync`; `/Admin/Themes`).
  Highlight the card **The Blog Theme** (it says "This is the current Site theme"), then the card **The Default Theme**.
  Click **Make Current** on the card The Default Theme (`.theme-card:has-text('The Default Theme') button:has-text('Make Current')`,
  with `ClickAndWaitForNavigationAsync`) near the end of the step. The card has two buttons, **Make Current** and **Enable**
  (the theme is not enabled yet, `ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:96-98`): a locator that ends in `button`
  alone matches both and fails. On screen at the end: the Themes page with the messages
  "The Default Theme was enabled" and "The Default Theme was set as the default Site theme", and the card The Default Theme
  marked "This is the current Site theme". Labels: `OrchardCore.Themes/AdminMenu.cs:18-21`;
  `OrchardCore.Themes/Views/Items/ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`;
  `OrchardCore.Themes/Controllers/AdminController.cs:148`; theme names `src/OrchardCore.Themes/TheTheme/Manifest.cs:5` and
  `src/OrchardCore.Themes/TheBlogTheme/Manifest.cs:5`. The reason: The Default Theme draws the navigation bar shape
  (`src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:13,54`) and The Blog Theme does not
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid` has no Navbar).
- `picker-2`: click the menu entry **Settings**, then **Localization**, then **Content Culture Picker**
  (`ClickAndWaitForNavigationAsync`; `/Admin/Settings/ContentCulturePicker`). Highlight the check box **Set Cookie** (checked)
  and the check box **Homepage redirect** (unchecked), one after the other. Do not change them. On screen at the end:
  both check boxes with their grey hints "Sets the localization cookie when switching cultures using the
  ContentCulturePicker" and "Redirects to the homepage of the target culture if the ContentItem of the target culture does
  not exist." Labels: `OrchardCore.ContentLocalization/AdminMenu.cs:41`;
  `OrchardCore.ContentLocalization/Views/ContentCulturePickerSettings.Edit.cshtml:8,10,18,20` (the label of the second check
  box ends with a space in the source); defaults `OrchardCore.ContentLocalization/Models/ContentCulturePickerSettings.cs`.
- `picker-3`: click the menu entry **Content Culture** (`ClickAndWaitForNavigationAsync`;
  `/Admin/Settings/ContentRequestCultureProvider`). Highlight the check box **Set Cookie** (unchecked). Do not change it.
  What it does (`OrchardCore.ContentLocalization/ContentRequestCultureProvider.cs:21-31`): a page with a translation
  always gets the culture of its translation for that request; with **Set Cookie** on, the provider also writes the culture
  cookie, so pages without a culture of their own then use that language. On screen at the end: the check box **Set Cookie** with the hint "Sets the localization cookie based on the
  LocalizationPart culture when accessing a page in the frontend." Labels: `OrchardCore.ContentLocalization/AdminMenu.cs:34`;
  `OrchardCore.ContentLocalization/Views/ContentRequestCultureProviderSettings.Edit.cshtml:8,10`; default
  `OrchardCore.ContentLocalization/Models/ContentRequestCultureProviderSettings.cs`.

### `07-try` (chapter 07, Switch languages on the site)

Starts on `/` (the home page, the Blog). The recorder is signed in, so the navigation bar also shows the user menu of
`admin` and a theme toggle: they are expected and hold no private data.

- `try-1`: click the link **About** in the main menu of the navigation bar (`ClickAndWaitForNavigationAsync`; the menu
  entry links to `~/about`, `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:843-846`). Highlight the
  drop-down toggle `#oc-culture-picker` at the right of the bar. On screen at the end: the About page with the English
  text, and the toggle labeled "English (United States)". Labels: the toggle text is the native name of the current
  culture (`OrchardCore.ContentLocalization/Views/ContentCulturePicker.cshtml:2`); the picker is drawn only when more than
  one culture is supported (`OrchardCore.ContentLocalization/Drivers/ContentCulturePickerNavbarDisplayDriver.cs:36-37`).
- `try-2`: click `#oc-culture-picker`. The menu shows one entry, "français (France)". Highlight it, then click it at the
  end of the step (`ClickAndWaitForNavigationAsync`). On screen at the end: the open menu with the entry "français
  (France)", or the French page if the click has completed. Label: `OrchardCore.ContentLocalization/Views/ContentCulturePicker.cshtml:16`.
- `try-3`: wait until the page shows the French text (`Notre carte suit les saisons.`). Highlight the toggle. On screen
  at the end: the French page with the title `À propos` and the French text, and the toggle labeled "français (France)".
  The redirect is made by `OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:43-58`.
- `try-4`: click `#oc-culture-picker`, then the entry "English (United States)" (`ClickAndWaitForNavigationAsync`). On
  screen at the end: the About page in English, with the toggle labeled "English (United States)".
- `try-5`: click the link **Home** in the main menu (`ClickAndWaitForNavigationAsync`; the Blog home page). Click
  `#oc-culture-picker`, then the entry "français (France)" (`ClickAndWaitForNavigationAsync`). The home page has no
  translation, so the browser lands on the same page. On screen at the end: the home page (the Blog), the same page as
  before the click, with the toggle now labeled "français (France)": the picker writes the culture cookie before it looks
  for a translation (`ContentCulturePickerController.cs:46-50`, **Set Cookie** is on), and the home page has no culture of
  its own, so the cookie decides. Rule: `OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:54` (the home
  page is tried only when **Homepage redirect** is on, and it is off).

## Slides

No recording is needed. `slides.py` draws them.

- `plan-1`: three cards with check marks: two cultures, one translated page, a language menu.
- `idea-1`: the three feature cards; the card **Localization** is bright.
- `idea-2`: the same cards; **Content Localization** is bright.
- `idea-3`: the same cards; **Content Culture Picker** is bright.
- `rules-1`: three rule cards; "The page has a translation" is bright.
- `rules-2`: the same cards; "No translation, Homepage redirect on" is bright.
- `rules-3`: the same cards; "Anything else" is bright.

## Checks for private data

- The site is new, made by the Blog recipe with the user `admin` and the address `admin@example.com`. No real person,
  company, address or password appears in the content, the settings or the sample posts.
- The seed text and the French text are invented (a garden café). They are the only text typed into content.
- Do not open the admin user menu on camera, and do not record the sign-in form: the password is never shown.
- Look at the contact sheet of every clip (`review.py`) for the machine name, a user profile path or a real email
  address in a browser message, a tooltip or an error page. None is expected.
- OCR the finished video once per second and at every scene change; the only names expected are `admin`, `About`,
  `À propos`, and the culture names.
