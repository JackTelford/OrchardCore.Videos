# Recording plan: Widgets overview

This plan is for the person who records the video. It needs no other source than the skill's `references/recording.md`
(how the recorder runs) and `references/storyboard.md`. A selector written here stands for `scene.Page.Locator(...)`.
Paths such as `src/OrchardCore.Modules/...:12` are in the Orchard Core checkout the plan was written from (commit 92db4a83);
they say where a label or a behavior comes from. Every selector below was read from that source and has not been
tried on a running site: check each one first with `Recorder probe` (see OPEN-QUESTIONS.md).

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with its own data folder outside both repositories |
| Auto Setup | tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), database provider Sqlite, recipe `Blog` |
| Setup screen | not recorded: the site is provisioned by Auto Setup before the first clip, and `state.json` is made with `Recorder setup` |
| Features to enable first | none. The Blog recipe enables `OrchardCore.Widgets`, `OrchardCore.Layers`, `OrchardCore.ContentTypes`, `OrchardCore.Contents`, `OrchardCore.Features`, `OrchardCore.Html`, `OrchardCore.Title`, `OrchardCore.Flows` and the others the clips use (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-81`) |
| Themes | site theme `TheBlogTheme`, admin theme `TheAdmin` (`blog.recipe.json:83-86`) |
| Settings to save | none |
| Admin list layout | leave the default, **List** (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`). The step `types-2` depends on it: the search of the Content Types list also reads the text of the row's tags, which holds the stereotype badge, only in the List layout (`OrchardCore.ContentTypes/Views/ContentTypeEntry.SummaryAdmin.cshtml:14`). In the Grid layout the badge is not searched (`OrchardCore.ContentTypes/Drivers/ContentTypeEntryDisplayDriver.cs:19,29-32`) and a search for `Widget` finds no row |
| External services | none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never makes an outbound call |
| Private data | none: a fresh site only holds the text of the Blog recipe (Lorem ipsum) and the fake values typed below. No mask script (`VIDEO_INIT_SCRIPT`) is needed |

### Seed data

Nothing is created outside the capture. The clips use what the Blog recipe creates:

- the content type `Article` (display name Article), with the parts Title, Autoroute, HtmlBody and Article (`blog.recipe.json:131-186`);
- the article titled `About`, published, at the path `about` (`blog.recipe.json:1060-1090`);
- the five widget types `Container`, `Blockquote`, `Image`, `Paragraph` and `RawHtml` (display name `Raw Html`), each with the stereotype `Widget` (`blog.recipe.json:320-440`).

### Text typed in the clips

| Where | Text |
|---|---|
| Features search box | `Widgets` |
| Content Types search box | `Widget`, then in another clip `Article` |
| Available widgets zones | `Content, Footer` |
| Paragraph widget, field Content | `Open every weekday from nine to five.` |
| Blockquote widget, field Quote | `Every garden begins with a single seed.` |

Type with the scene's `TypeAsync` (25 to 90 ms per character). No real name, address or number is typed anywhere.

## 2. Order of the clips and the state of the site

Record the narration first (`narrate.py`), then the clips in this order. A **snapshot** is a copy of the site's data folder
and `state.json`, made with the site stopped.

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 0 | | a freshly provisioned site | **Snapshot S0** (before any clip) |
| 1 | `02-feature` | S0 | no |
| 2 | `03-types` | S0 | no |
| 3 | `04-allow` | S0 | **yes**: the `Article` type gets the WidgetsList part, with the zones `Content, Footer`. Make **snapshot S1** afterwards |
| 4 | `05-add` | S1 | **yes**: the `About` article gets a Blockquote in `Content` and a Paragraph in `Footer`, published. Make **snapshot S2** afterwards |
| 5 | `06-manage` | S2 | no: nothing is published, so the saved article is unchanged |

To record a clip again, restore the snapshot it starts from (stop the site first, restore the data folder and `state.json`,
start the site). `05-add` and `06-manage` depend on `04-allow`; `06-manage` depends on `05-add`.

## 3. Slides (`01-idea`, drawn by `slides.py`, nothing to record)

- `idea-1`: three cards, "A content item", "A content type with a stereotype", "Ready-made types".
- `idea-2`: three cards, "Named zones", "A widget goes into a zone", "Names that match the theme".
- `idea-3`: three cards, "The WidgetsList part", "Layers" and "The Flow part".

## 4. Clips

Every clip uses `NewSceneAsync`, opens its first page with `OpenAsync` (not recorded), calls `StartCaptureAsync`, then one
`StepAsync` per step, in the order below. Keep the actions of a step shorter than its narration. Clear highlights
(`ClearHighlightsAsync`) before scrolling or navigating.

### Clip `02-feature` (chapter 02, short name `feature`)

Starts on `/Admin` (the admin home), with the admin menu as it opens.

- `feature-1`
  - Actions: click **Tools** in the admin menu, then click **Features** (`ClickAndWaitForNavigationAsync`). On the Features page, type `Widgets` in the search box `#search-box` (`TypeAsync`). Wait for `#list-summary` to have changed from its first text. Several rows stay in the list, not only **Widgets** (see below): find the row of the feature **Widgets** by its id `OrchardCore.Widgets`, shown in small type next to the name, and highlight only that row's description with `HighlightAsync`.
  - On screen when the narration ends: the Features page, the search box holding `Widgets`, and the row **Widgets** with its description "Provides a part allowing content items to render Widgets in theme zones." highlighted.
  - Why more than one row stays: the search reads the name, the id and the description of every feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), disabled features included, so the features whose description says "widgets" stay as well, for example Layers (`OrchardCore.Layers/Manifest.cs:13`), Flows (`OrchardCore.Flows/Manifest.cs:13`), Forms (`OrchardCore.Forms/Manifest.cs:13`), Admin Dashboard (`OrchardCore.AdminDashboard/Manifest.cs:8`) and the Meta widgets feature (`OrchardCore.Facebook/Manifest.cs:36`). The narration does not say how many rows stay.
  - Labels: Tools and Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`; description `OrchardCore.Widgets/Manifest.cs:13`; name and id of the row `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`. The search is done in the browser by `OrchardCore.Features/Views/Admin/Features.cshtml:109` (`features-index`).
- `feature-2`
  - Actions: clear the highlight; highlight the red **Disable** button at the end of the row of the feature **Widgets** (the button `#btn-disable-OrchardCore_Widgets`, the id is made from the feature id: confirm it with `Recorder probe`), with the caption `Disable` (`HighlightAsync`). Do not click it.
  - On screen when the narration ends: the row **Widgets** with a ring around its **Disable** button.
  - Labels: **Disable** and **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53,40`. The Blog recipe enables the feature: `blog.recipe.json:75`.

### Clip `03-types` (chapter 03, short name `types`)

Starts on `/Admin`.

- `types-1`
  - Actions: click **Design** in the admin menu, then **Content Definition**, then **Content Types** (the last click with `ClickAndWaitForNavigationAsync`). Use `pauseAfter: 500` on the first two clicks so the three clicks fit in the narration.
  - On screen when the narration ends: the **Content Types** page with the search box, the button **Create new type**, and the list of types.
  - Labels: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`; page title `OrchardCore.ContentTypes/Views/Admin/List.cshtml:6`.
- `types-2`
  - Actions: type `Widget` in the search box `#search-box`. Wait until only rows with a **Widget** badge remain (the list filters in the browser, on the text of the row's `.list-item-search-text` elements: `OrchardCore.Resources/Assets/js/list-management.js:228-240`). Highlight, in each of the rows Blockquote, Container, Image, Paragraph and Raw Html, the badge `.ta-badge` whose text is `Widget` (a text filter such as `HasText`); give the first one the caption `Stereotype`. Do not select the badge by its `title` attribute: the badge is a Bootstrap tooltip, and TheAdmin creates the tooltip when the page loads (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:190-193`), which moves `title` into `data-bs-original-title` and removes `title` (`src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Scripts/bootstrap.js:3555-3565`). Each row also has a badge with the technical name (for example `Paragraph`); its text does not contain `Widget`.
  - On screen when the narration ends: the five rows Blockquote, Container, Image, Paragraph and Raw Html, each with a badge reading **Widget**, and rings around the badges.
  - Labels: badge `OrchardCore.ContentTypes/Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:13-16` (its tooltip is `Stereotype`, line 15); the badge is inside the `.list-item-search-text` div of `ContentTypeEntry.SummaryAdmin.cshtml:14`; display names in `blog.recipe.json:320-440`.
- `types-3`
  - Actions: clear the highlights. In the row **Paragraph**, click the button **Edit** (`a.edit-button` inside the row, `ClickAndWaitForNavigationAsync`). On the page "Edit Content Type - Paragraph", scroll until the **Stereotype** field is in view, then highlight it with the caption `Stereotype`.
  - On screen when the narration ends: the page "Edit Content Type - Paragraph" with the field **Stereotype** holding `Widget`, highlighted, and its hint "(Optional) The stereotype of the content type. e.g., Widget, MenuItem, ...".
  - Labels: field `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:76-88` (label line 77, hint line 86); row button `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; page title `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`. The row buttons are in the List layout's Actions zone; if the admin settings show them in a menu instead, open the menu first (`Recorder probe`).
- `types-4`
  - Actions: clear the highlight; scroll down to the **Fields** heading; highlight the field row **Content** (it shows the small text "Html Field" and a badge **Wysiwyg**).
  - On screen when the narration ends: the **Fields** list of Paragraph with one row, **Content**, and the badge **Wysiwyg**, highlighted.
  - Labels: **Fields** `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:88`; the field and its editor `blog.recipe.json:759-775` (display name `Content`, editor `Wysiwyg`).
- `types-5`
  - Actions: clear the highlight; scroll to the end of the page and click **Cancel** (`a.cancel`, `ClickAndWaitForNavigationAsync`), which goes back to the Content Types list. Highlight the button **Create new type** with the caption `Create new type`. Do not click it.
  - On screen when the narration ends: the Content Types list with a ring around **Create new type**.
  - Labels: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:235` (Cancel), `OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`.

### Clip `04-allow` (chapter 04, short name `allow`) — changes the site, start from S0

Starts on `/Admin/ContentTypes/List` (the Content Types list, unfiltered).

- `allow-1`
  - Actions: type `Article` in the search box `#search-box`; in the one remaining row **Article**, click **Edit** (`a.edit-button`, `ClickAndWaitForNavigationAsync`); on the page "Edit Content Type - Article", scroll to the end of the **Parts** list and click **Add Parts** (`ClickAndWaitForNavigationAsync`).
  - On screen when the narration ends: the page **Add Parts** with the text "Choose the Parts to add to this Content Type." and the list of parts.
  - Labels: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201` (Add Parts); `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:10,18`. The About article is of the type Article: `blog.recipe.json:1060-1064`.
- `allow-2`
  - Actions: scroll the part list until the row **WidgetsList** is in view (it is near the end, the list is sorted by name); highlight the row (its label **WidgetsList** and the description "Provides a way to add widgets to Layout zones for your content item."); then click the button **Add & Configure** of that row (`button[name="submit.AddAndConfigure"]` inside the row, `ClickAndWaitForNavigationAsync`). Do not tick the checkbox and do not click **Add Selected Parts**.
  - On screen when the narration ends: the page "Edit Part - WidgetsList", with the field **Available widgets zones** empty.
  - Labels: the part name shown is `WidgetsList` (see OPEN-QUESTIONS.md, question 1): the part name minus its suffix `Part` is built in `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63` and used by `src/OrchardCore.Modules/OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19`; description `OrchardCore.Widgets/Migrations.cs:20`; button `AddPartsTo.cshtml:36`; the redirect to the part's settings `OrchardCore.ContentTypes/Controllers/AdminController.cs:355-357`; page title `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22`.
- `allow-3`
  - Actions: click the field **Available widgets zones** (`GetByLabel("Available widgets zones")`) and type `Content, Footer`; press Tab; click **Save** (`button.save`, `ClickAndWaitForNavigationAsync`). Highlight the field with the caption `Available widgets zones` while typing, and clear the highlight before **Save**.
  - On screen when the narration ends: the page "Edit Content Type - Article" with the green notice that the part settings were saved (or, if the notice already faded, the page itself).
  - Labels: field and hint ("A comma separated list of values, representing the available zones for widgets.") `OrchardCore.Widgets/Views/WidgetsPartSettings.Edit.cshtml:4,8`; **Save** `EditTypePart.cshtml:111`; the settings are stored by `OrchardCore.ContentTypes/Controllers/AdminController.cs:1061-1085`; the zones are split on commas and spaces: `OrchardCore.Widgets/Settings/WidgetsListPartSettingsDisplayDriver.cs:11,30`.
- `allow-4`
  - Actions: scroll to the **Parts** heading and highlight the row **WidgetsList** (with the description under its name and the buttons **Edit** and **Remove**).
  - On screen when the narration ends: the **Parts** list of Article ending with the row **WidgetsList**, highlighted.
  - Labels: **Parts** `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144`. The new part is placed last: `OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:481-491`.

Take **snapshot S1** now.

### Clip `05-add` (chapter 05, short name `add`) — changes the site, start from S1

Starts on `/Admin/Contents/ContentItems`, the page that the menu entry **Content Items** (group **Content**) opens. Its title is **Manage Content**. Reach it before the capture by clicking **Content**, then **Content Items** in the admin menu, so that the page is open and the menu shows where it came from.

- `add-1`
  - Actions: in the row **About** (the article), click **Edit** (`a.edit` inside the row, `ClickAndWaitForNavigationAsync`). On the edit page, scroll down until the section **WidgetsList** shows two cards, **Content** and **Footer** (the card headers show the zone names, in the order of the setting). Highlight the two headers.
  - On screen when the narration ends: the edit page of About, scrolled to the **WidgetsList** section, with the cards **Content** and **Footer**, both empty, each with a plus button at its bottom.
  - Labels: the menu entry **Content Items** in the group **Content** `OrchardCore.Contents/AdminMenu.cs:65-68` (the Blog recipe adds an admin menu entry of the same names, `blog.recipe.json:1237,1253`, and entries with the same caption are merged into one: `src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`); the page title **Manage Content** `OrchardCore.Contents/Views/Admin/List.cshtml:17` (the page is not titled "Content Items"); **Edit** `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; section label `OrchardCore.Contents/Views/ContentPart-WidgetsListPart.Edit.cshtml:23`; zone cards `OrchardCore.Widgets/Views/WidgetsListPart.Edit.cshtml:24-30` (the zones come from `Model.AvailableZones`).
- `add-2`
  - Actions: clear the highlights. In the **Footer** card (`.widget-list-zone` whose `.card-header` text is exactly `Footer`), highlight the plus button `button[title="Add Widget"]` with the caption `Add Widget`, then click it. The dropdown opens.
  - On screen when the narration ends: the open dropdown under the plus button of the Footer card, listing, in this order, **Blockquote**, **Container**, **Image**, **Paragraph**, **Raw Html**.
  - Labels: the title `Add Widget` is only a tooltip on an icon (`OrchardCore.Widgets/Views/WidgetsListPart.Edit.cshtml:82`), which a headless capture does not draw: the highlight caption puts the name on screen. The list is every type with the stereotype Widget, sorted by display name (`WidgetsListPart.Edit.cshtml:86`).
- `add-3`
  - Actions: click **Paragraph** in the dropdown (`.dropdown-item.add-list-widget` with the text `Paragraph`). Wait until a card appears in the Footer zone (`.widget-editor` inside that `.widget-list-zone`, loaded by a request to `Widgets/BuildEditor`). If the card shows only its header (the code adds the `collapsed` class to a card made this way), click its expand button (`.widget-editor-btn-expand`). Wait for the editor `.trumbowyg-editor` inside that card (the editor starts by itself on a card inserted into the page later: `src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/ts/html-field-wysiwyg.ts:5` registers it with `observeAndInit`, which watches the page for new fields), click it, and type `Open every weekday from nine to five.` Scope every locator to this card: the article body above also has a `.trumbowyg-editor`.
  - On screen when the narration ends: the Footer card titled **Paragraph**, open, with the field **Content** (a Wysiwyg editor) holding the typed sentence.
  - Labels: type name `blog.recipe.json:398-420` (display name Paragraph); the card is built collapsed because `OrchardCore.Widgets/Controllers/AdminController.cs:67` sets `Inline = true` and `OrchardCore.Widgets/Views/ContentCard-WidgetsListPart.Edit.cshtml:15` adds `collapsed` for it; field label `Content` `blog.recipe.json:759-775`. See OPEN-QUESTIONS.md, question 2.
- `add-4`
  - Actions: in the **Content** card, click the plus button `button[title="Add Widget"]`, click **Blockquote** in the dropdown, wait for the card, click its expand button if the card shows only its header, click the text area of the field **Quote** and type `Every garden begins with a single seed.` Use a typing delay of about 40 ms so the typing ends before the narration does.
  - On screen when the narration ends: the Content card holding a **Blockquote** card, open, with the field **Quote** (a text area) holding the typed sentence.
  - Labels: field `Quote`, editor text area: `blog.recipe.json:707-722` (line 716 is the editor); label view `OrchardCore.ContentFields/Views/TextField-TextArea.Edit.cshtml`.
- `add-5`
  - Actions: highlight the button **Publish** in the bottom action bar with the caption `Publish`; click it (`button.publish`, `ClickAndWaitForNavigationAsync`). The site returns to the Content Items list.
  - On screen when the narration ends: the Content Items list after publishing, with the row **About**.
  - Labels: **Publish** `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7,19`. The widgets are saved inside the part of the article: `OrchardCore.Widgets/Models/WidgetsListPart.cs` (a dictionary of zone to widgets) and `OrchardCore.Widgets/Drivers/WidgetsListPartDisplayDriver.cs:103-140`.
- `add-6`
  - Actions: `NavigateAsync("/about")`. Scroll slowly down the page so that the quote (`article.widget-blockquote`) and then the paragraph in the footer (`article.widget-paragraph`) are each in view for a moment; highlight each with `HighlightAsync` (clear before scrolling).
  - On screen when the narration ends: the front end of About, scrolled to the footer, with the paragraph "Open every weekday from nine to five." visible inside the footer and a ring around it; the quote was shown earlier in the step.
  - Labels: the widgets are put in the zones by `OrchardCore.Widgets/Drivers/WidgetsListPartDisplayDriver.cs:42-86` (front end only, in the detail view); the theme renders the zones in `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:35-47`; the classes come from `TheBlogTheme/Views/Widget-Blockquote.liquid` and `Widget-Paragraph.liquid`. Whether the quote comes before or after the article text is not decided by the plan: see OPEN-QUESTIONS.md, question 3.

Take **snapshot S2** now.

### Clip `06-manage` (chapter 06, short name `manage`) — no saved change, start from S2

Starts on the edit page of the **About** article, reached before the capture from the Content Items list (**Edit** in the row **About**) and scrolled so that the whole **WidgetsList** section is in view. Both saved cards are open (cards that were saved are drawn open: `ContentCard-WidgetsListPart.Edit.cshtml:15`, `WidgetsListPart.Edit.cshtml:56`).

- `manage-1`
  - Actions: highlight the handle (`.widget-editor-handle`, the four-arrows icon at the left of the header) of the **Blockquote** card; drag it (`DragToAsync`) onto the header of the **Paragraph** card in the Footer zone, and drop it there. Clear the highlight first.
  - On screen when the narration ends: the Footer zone holding two cards, **Paragraph** and **Blockquote** (in the order the drop gives; the next step removes the Blockquote card either way), and the Content zone empty.
  - Labels: handle `OrchardCore.Widgets/Views/ContentCard-WidgetsListPart.Edit.cshtml:19`; moving between zones `OrchardCore.Widgets/Assets/ts/widgetslist.edit.ts` (function `initWidgetsListSortable`, `onReparented`). The drag library is set up with the handle `.widget-editor-handle` and one group for all zones (compiled `OrchardCore.Widgets/wwwroot/Scripts/widgets/widgetslist.edit.js`, `Sortable.create`; its source package `@orchardcore/bloom` is not part of the checkout the plan was written from). See OPEN-QUESTIONS.md, question 5.
- `manage-2`
  - Actions: click the collapse button (`.widget-editor-btn-collapse`, a down arrow) of the **Paragraph** card, wait one second, then click its expand button (`.widget-editor-btn-expand`, a right arrow; it takes the place of the collapse button, so the second click lands where the first one did).
  - On screen when the narration ends: the **Paragraph** card open again, with its field **Content** visible.
  - Labels: the two buttons `ContentCard-WidgetsListPart.Edit.cshtml:21-22`, of which only one is drawn at a time (`OrchardCore.Widgets/Assets/scss/widgetslist.edit.scss`); the toggle is the click handler of `.widget-list-editor-btn-toggle` in `OrchardCore.Widgets/Assets/ts/widgetslist.edit.ts`.
- `manage-3`
  - Actions: move the mouse over the header of the **Paragraph** card (`MoveToAsync`) until the plus button of `.btn-widget-metadata` appears at the top of the header; highlight it with the caption `Insert Widget` (`HighlightAsync`). Do not click it.
  - On screen when the narration ends: the **Paragraph** card open, with a ring and the caption `Insert Widget` around the plus button on its header.
  - Labels: the plus button, title `Insert Widget`, `ContentCard-WidgetsListPart.Edit.cshtml:24-28`, shown on hover by `OrchardCore.Widgets/Assets/scss/widgetslist.edit.scss`; its dropdown lists the same widget types as the Add Widget button (`ContentCard-WidgetsListPart.Edit.cshtml:31-49`), and the widget is inserted before the card: `OrchardCore.Widgets/Assets/ts/widgetslist.edit.ts` (the click handler of `.insert-list-widget` inserts at `beforebegin` of the card's `.widget-template` frame, `OrchardCore.Widgets/Views/ContentCard.Frame.cshtml`).
- `manage-4`
  - Actions: clear the highlight. Click the red trash button (`.widget-list-delete`) of the **Blockquote** card. The dialog titled **Delete** with the text "Are you sure you want to remove this item?" opens; click **Ok** (`#modalOkButton`). The card disappears. Do not click **Publish** or any other save button.
  - On screen when the narration ends: the Footer zone holding only the **Paragraph** card, and the Content zone empty.
  - Labels: `ContentCard-WidgetsListPart.Edit.cshtml:57`; the dialog buttons **Ok** and **Cancel** `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`.
  - After the clip, leave the page without saving. The saved article is unchanged, so the next take of this clip can start again from S2 with no restore.

### Cards

`00-intro` and `99-outro` are cards drawn from the chapter titles and subtitles of `storyboard.json`; nothing to record.

## 5. Checks for private data

Before assembling, look at every contact sheet (`review.py`) and OCR the video as `SKILL.md` describes. Look for:

- the profile menu or any page that shows an email address other than `admin@example.com`;
- the password: it is never typed on camera (the sign-in is made by `Recorder setup`);
- the address bar or a path of this machine, a user name of this machine, a file name of a data folder;
- any text typed other than the values in the table of section 1;
- the footer of `add-6`: besides the new paragraph it shows the recipe's own footer widget, which holds social icons without a real link and the text "Copyright (c) Your Website 2021" (`src/OrchardCore.Themes/TheBlogTheme/Recipes/Snippets/footer.html`). That is expected and holds no real name;
- error messages, "Nothing here" notices, or a list that is empty by mistake.
