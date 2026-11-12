# Recording plan: Title part

A person who reads only this file records the video without deciding anything. Selectors stand for
`scene.Page.Locator(...)`; "Orchard Core" paths are in the checkout of `OrchardCMS/OrchardCore` at commit `92db4a83`,
under `src/OrchardCore.Modules` unless a path says otherwise. The general method (recorder, scene API, pitfalls) is in
`.agents/skills/orchardcore-video-producer/references/recording.md`.

The video has three slides (chapter 01, drawn by `slides.py`, not recorded), four recordings (chapters 02 to 05) and
the two cards.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of the checkout above, run on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup (no setup screen is recorded, and the sign-in is made outside the capture) | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...SiteName=Orchard Core`, `...SiteTimeZone=America/Los_Angeles`, `...AdminUsername=admin`, `...AdminEmail=admin@example.com`, `...AdminPassword=$VIDEO_PASSWORD`, `...DatabaseProvider=Sqlite`, `...RecipeName=Blog` |
| Password | Chosen outside git and passed in `VIDEO_PASSWORD` (`<password>` in every file of the repository). The recorder signs in with `VIDEO_USER_NAME=admin` and saves the session in `state.json`, which is git-ignored |
| Recipe and theme | Blog: it makes TheBlogTheme the site theme (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:85`) |
| Features to enable before recording | None. The Blog recipe enables `OrchardCore.Title`, `OrchardCore.Contents`, `OrchardCore.ContentTypes` and `OrchardCore.ContentFields` (`blog.recipe.json:52-55,73`) |
| Admin settings | The defaults: lists in the List layout with Buttons for the row actions (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). Do not change them |
| External services | None for the site. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address anyway, so the site never calls out. Only the browser reaches out: the layout of TheBlogTheme links web fonts of a public font host (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:11-12`), which `gen-7` shows. Without a connection the page falls back to default fonts; that is harmless, but record every take under the same conditions |
| Browser | The recorder's Chromium: 1536x864 CSS pixels, light color scheme, `en-US` |

### Seed data (create it before the first take, outside the capture)

The video needs one content type, **Product**, with two Text fields and no Title part. Create it in the admin, signed in
as `admin`:

1. Open **Design, Content Definition, Content Types** (`/Admin/ContentTypes/List`) and click **Create new type**
   (`ContentTypes/Views/Admin/List.cshtml:20`).
2. Display Name: `Product`. Technical Name: `Product` (the page fills it in as you type; check it reads `Product`).
   Click **Create** (`ContentTypes/Views/Admin/Create.cshtml:14,22,30`). The **Add Parts** page opens: click
   **Cancel** (`AddPartsTo.cshtml:48`), which opens **Edit Content Type - Product**. Do not add any part.
3. On that page, under **Fields**, click **Add Field** (`Edit.cshtml:137`). Display Name: `Brand`. Technical Name:
   `Brand`. Field Type: select the radio **Text Field** (the first radio is **Boolean Field**; choose the right one).
   Click **Save** (`AddFieldTo.cshtml:38,46,54,71`). Repeat with Display Name `Name`, Technical Name `Name`, Field
   Type **Text Field**.
4. Check **Edit Content Type - Product**: the Fields list reads Brand then Name (if Name comes first, drag Brand above
   it and click **Save**), and the Parts list holds only **Product**. A new type is Creatable, Draftable, Versionable and
   Listable (`ContentTypes/Services/ContentDefinitionService.cs:85-89`), so **Create New Product** and **List Items**
   appear on its row of the Content Types list.
5. Check that no Product content item exists (Content, Content Items, filter by type, or the **List Items** button of the
   Product row).

All values are invented: the brand "Brightwater" and the products are not real.

### Snapshots

Every recording changes the site, so take a copy of the site after each stage: stop the site, copy `App_Data` and
`state.json` aside, start it again.

| Snapshot | Taken | Restore it before recording |
|---|---|---|
| S0 | after the seed data above | `02-attach` (and before a full take) |
| S2 | after `02-attach` | `03-typed` |
| S3 | after `03-typed` | `04-generate` |
| S4 | after `04-generate` | `05-hidden` |

## Order of the clips

`02-attach`, then `03-typed`, then `04-generate`, then `05-hidden`: the order they play in, and the only order that
works.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-attach` | S0 | Attaches the Title part to Product, with the option **Title is editable and required** and the placeholder `Product title` |
| `03-typed` | S2 | Creates the content item "Brightwater Camp Stove" (published) |
| `04-generate` | S3 | Changes the option to **Title is generated and input is disabled**, sets the pattern, clears the placeholder, creates "Brightwater Trail Backpack" |
| `05-hidden` | S4 | Changes the option to **Title is generated and input is hidden**, creates "Brightwater Water Bottle" |

## Notes for every clip

- Before `StartCaptureAsync`, open the start page with `OpenAsync` and scroll the Product row of the Content Types list
  into view (`ScrollIntoViewIfNeededAsync` on `a[role='btn-edit-Product']`): the list holds every type of the Blog
  recipe, and Product is far down it. Do the same after each step that returns to the list.
- Row buttons of the Content Types list (`ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml`):
  `#btn-create-Product` is **Create New Product** (line 19), `a[role='btn-list-Product']` is **List Items** (line 22),
  `a[role='btn-edit-Product']` is **Edit** (line 24).
- The ids of the fields in the part settings are prefixed by the display driver, so select them by the end of the id
  (`select[id$='Options']`, `input[id$='Placeholder']`, `textarea[id$='Pattern']`), as
  `OrchardCore.Title/Assets/ts/title-part-settings.ts:7-8` does for the Pattern and the Options (it never touches the
  Placeholder, which needs the same kind of selector). In the content editor, find fields by their label:
  `GetByLabel("Title", new() { Exact = true })`, `GetByLabel("Brand", new() { Exact = true })`,
  `GetByLabel("Name", new() { Exact = true })`.
- **Publish** is `button.publish` (`Contents/Views/Content.PublishButton.cshtml:7,19`). The editor is always opened from
  the Content Types list, so the page is in the split-button form and `button.publish` is its main button. Every
  submit and every link below loads a page: use `ClickAndWaitForNavigationAsync`.
- The Pattern field is a CodeMirror editor: the textarea is hidden. Click `#patternEditor .CodeMirror` and type with the
  keyboard (`TypeAsync` on that locator, or click it and use `scene.Page.Keyboard.TypeAsync`).
- Clear highlights (`ClearHighlightsAsync`) before every scroll or navigation.
- Keep each step's actions shorter than its narration; the steps with several page loads (`attach-2`, `gen-1`,
  `hidden-1`) are the tightest, so move the cursor directly and do not add pauses.

## Chapter 01: the slides (`01-why`, not recorded)

- `why-1`: three cards, "Content type", "Content item", "Display text".
- `why-2`: three cards, "Without a title", "The Title part", "Display text".
- `why-3`: four cards, the four labels of the Behavior option exactly as the settings page spells them.

## Chapter 02: Add the Title part (`02-attach`)

Start page: `/Admin/ContentTypes/List`, with the Product row in view. Starts from S0.

- `attach-1`: Highlight the Product row (`HighlightAsync` on the row holding `a[role='btn-edit-Product']`, caption
  "Product"), then `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(a[role='btn-edit-Product'])`.
  Visible at the end: **Edit Content Type - Product**, with the Fields list (Brand, Name) and the Parts list (only
  Product). Labels: menu path `ContentTypes/AdminMenu.cs:23-25`; **Edit** `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`;
  page title `ContentTypes/Views/Admin/Edit.cshtml:63`.
- `attach-2`: `ClickAndWaitForNavigationAsync(a[href$='/ContentTypes/AddPartsTo/Product'])` (the **Add Parts** button,
  `Edit.cshtml:201`). The page lists the attachable parts sorted by display name. Scroll the Title row into view
  (`li.list-group-item:has(label.form-check-label:text-is('Title'))`, the label is **Title**, its hint is "Provides a
  Title for your content item."), `HighlightAsync` it with the caption "Title", `ClearHighlightsAsync`, then
  `ClickAndWaitForNavigationAsync(button[name='submit.AddAndConfigure'][value='TitlePart'])` (**Add & Configure**).
  Visible at the end: the **Edit Part - Title** page. Labels: `ContentTypes/Views/Admin/AddPartsTo.cshtml:36`; the row
  reads **Title** because the part has no display name of its own and the page drops the suffix "Part"
  (`ContentTypes/ViewModels/EditPartViewModel.cs:27-31`, `ContentPartSettingsExtensions.cs` `DisplayName`); page title
  `EditTypePart.cshtml:22`.
- `attach-3`: `HighlightAsync(select[id$='Options'], "Behavior option")`, `PauseAsync(1200)`, `ClearHighlightsAsync`,
  `HighlightAsync(input[id$='RenderTitle'], "Render Title")`. Visible at the end: the select reads **Title is editable**,
  the **Render Title** box is checked, and the **Pattern** field is not shown. The page also still shows the message
  `The "Title" part has been added.` from the click on **Add & Configure** (`ContentTypes/Controllers/AdminController.cs:355`):
  leave it, it is true. Labels:
  `OrchardCore.Title/Views/TitlePartSettings.Edit.cshtml:15,18,29`. Defaults: `OrchardCore.Title/Models/TitlePartSettings.cs`
  (`Options` is `Editable`, `RenderTitle` is `true`). The editor and display mode drop-downs below the Title settings
  belong to the generic part page: do not scroll to them.
- `attach-4`: `ClearHighlightsAsync`, `TypeAsync(input[id$='Placeholder'], "Product title")`, then
  `SelectAsync(select[id$='Options'], "EditableRequired")` (label **Title is editable and required**). Visible at the
  end: the Placeholder box holds `Product title`, the select reads **Title is editable and required**, and the
  Pattern field is still hidden. Labels: `TitlePartSettings.Edit.cshtml:19,44`.
- `attach-5`: `ClickAndWaitForNavigationAsync(button.save[name='submit.Save'])` (**Save**, `EditTypePart.cshtml:111`).
  Visible at the end: **Edit Content Type - Product**, with the message `The "Title" part settings have been saved.`
  (`ContentTypes/Controllers/AdminController.cs:1084`) and **Title** in the Parts list. If an error shows instead, stop and
  fix the cause before taking again.

## Chapter 03: Typed titles (`03-typed`)

Start page: `/Admin/ContentTypes/List`, with the Product row in view. Starts from S2.

- `typed-1`: `ClickAndWaitForNavigationAsync(#btn-create-Product)` (**Create New Product**). Wait for the Title field,
  then `HighlightAsync(GetByLabel("Title"), "Title")`. Visible at the end: **New Product**, with an empty Title field that
  shows the placeholder `Product title`; by the code the Brand and Name fields are above it, not below. Labels:
  `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`; `Contents/Views/Admin/Create.cshtml:26`;
  `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12-14`. The editor orders the parts by the position stored on the type
  (`OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentItemDisplayCoordinator.cs:327`), and a part
  without a stored position counts as "before". The Product part has none, and attaching the Title part stored the
  position "1" (`ContentTypes/Services/ContentDefinitionService.cs:143-148,481-522`), so the fields of Product come
  first and the Title field last. The part's default position of 0 (`OrchardCore.Title/Migrations.cs:34`) only sorts the
  Parts list of the type page (`ContentTypes/Views/Admin/Edit.cshtml:32`), where Title is listed above Product. The
  highlight works wherever the field is; if the screen differs on the first take, see OPEN-QUESTIONS.md.
- `typed-2`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(button.publish)` with the Title field empty. The
  page loads again with errors. Scroll to the top so the validation summary is in view, and `HighlightAsync` the message.
  Visible at the end: the message **A value is required for Title.** (`OrchardCore.Title/Drivers/TitlePartDisplayDriver.cs:56`;
  the summary comes from `Contents/Views/Admin/Create.cshtml:34`) and no new item. The Title field has no browser-side
  `required` check, so the server answers.
- `typed-3`: `ClearHighlightsAsync`, `TypeAsync(GetByLabel("Title"), "Brightwater Camp Stove")`, then
  `ClickAndWaitForNavigationAsync(button.publish)`. Visible at the end: the Content Types list, with the message
  `Your Product has been published.` (`Contents/Controllers/AdminController.cs:473`); scroll the Product row into view.
  The editor returns to the list because it was opened from it (`AdminController.cs:795-798`).
- `typed-4`: `ClickAndWaitForNavigationAsync(a[role='btn-list-Product'])` (**List Items**). `HighlightAsync` the title link
  of the row. Visible at the end: the **Product** list with one row, the link **Brightwater Camp Stove**. Labels:
  `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:22`; the row title `Contents/Views/ContentsTitle_SummaryAdmin.cshtml`.

## Chapter 04: Generated titles (`04-generate`)

Start page: `/Admin/ContentTypes/List`, with the Product row in view. Starts from S3.

- `gen-1`: `ClickAndWaitForNavigationAsync(a[role='btn-edit-Product'])`, scroll the Title row of the Parts list into view,
  `ClickAndWaitForNavigationAsync(a[href$='/ContentTypes/Product/ContentParts/TitlePart/Edit'])` (**Edit**,
  `Edit.cshtml:163`), then `SelectAsync(select[id$='Options'], "GeneratedDisabled")` (label **Title is generated and
  input is disabled**). Visible at the end: **Edit Part - Title**, the select reads **Title is generated and input is
  disabled**, and the **Pattern** field is now shown, empty, with line numbers (the script
  `OrchardCore.Title/Assets/ts/title-part-settings.ts:15-23` shows it for the two generated options). The select keeps the
  stored value on the next visit, so the page opened with **Title is editable and required** selected.
- `gen-2`: `HighlightAsync(#patternEditor, "Pattern")`, `PauseAsync(1200)`, `ClearHighlightsAsync`, then
  `TypeAsync(#patternEditor .CodeMirror, ...)` with exactly this text on one line (one space between the two
  expressions):
  `{{ ContentItem.Content.Product.Brand.Text }} {{ ContentItem.Content.Product.Name.Text }}`.
  Visible at the end: the typed text in the Pattern editor. Check it with
  `scene.Page.EvalOnSelectorAsync<string>("#patternEditor .CodeMirror", "e => e.CodeMirror.getValue()")` before leaving the
  step: it must equal the line above. The expression reads the Text of the Brand and Name fields of the part named
  Product, which holds the fields of the Product type (`ContentDefinitionService.cs:82`; documented form in
  `src/docs/reference/modules/Title/README.md`, "TitlePart Settings"). Labels: `TitlePartSettings.Edit.cshtml:36-39`.
- `gen-3`: `MoveToAsync(input[id$='Placeholder'])` then `FillAsync("")` on it, so the box is empty. Then
  `ClickAndWaitForNavigationAsync(button.save[name='submit.Save'])`. Visible at the end: **Edit Content Type - Product**
  with the message `The "Title" part settings have been saved.`. If the page shows "A pattern is required when using the
  selected behavior option." or "The pattern doesn't contain a valid Liquid expression" (`Settings/TitlePartSettingsDisplayDriver.cs:55,59`),
  the pattern was typed wrongly: restore S3 and take the clip again.
- `gen-4`: `NavigateAsync("/Admin/ContentTypes/List")`, scroll the Product row into view,
  `ClickAndWaitForNavigationAsync(#btn-create-Product)`. In the editor, `HighlightAsync` the hint under the Title field
  ("The title will be auto-generated using the pattern.", `TitlePart.Edit.cshtml:18`) for about a second,
  `ClearHighlightsAsync`, then `TypeAsync(GetByLabel("Brand"), "Brightwater")`, `TypeAsync(GetByLabel("Name"),
  "Trail Backpack")`, `ClickAndWaitForNavigationAsync(button.publish)`. Visible while the editor is open: the Title field
  grayed out and empty, with the hint. Visible at the end: the Content Types list with `Your Product has been
  published.`, the Product row in view. The title field is disabled by `TitlePart.Edit.cshtml:14` (`disabled` when the
  option is `GeneratedDisabled`).
- `gen-5`: `ClickAndWaitForNavigationAsync(a[role='btn-list-Product'])`. `HighlightAsync` the title link
  **Brightwater Trail Backpack**. Visible at the end: the Product list with two rows, **Brightwater Trail Backpack** and
  **Brightwater Camp Stove**. The generated title is the display text set by `OrchardCore.Title/Handlers/TitlePartHandler.cs:100-110`
  (run on create, update, clone and publish: lines 33-52).
- `gen-6`: `ClearHighlightsAsync`, then click **Edit** in the row of Brightwater Trail Backpack and wait for the page:
  `ClickAndWaitForNavigationAsync(Locator("xpath=//a[normalize-space()='Brightwater Trail Backpack']/ancestor::li[1]//a[contains(@class,'edit')]"))`
  (if the list does not use `li` rows, use the `a.edit` button of the first row, which is the most recently modified
  item; check the row on the first take). Visible at the end: **Edit Product**, with the Title field disabled and holding
  `Brightwater Trail Backpack`, Brand `Brightwater`, Name `Trail Backpack`. Labels: `Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`;
  `Contents/Views/Admin/Edit.cshtml:39`.
- `gen-7`: `ClickAndWaitForNavigationAsync(a.cancel)` (**Cancel**, `Contents/Views/Content.Edit.cshtml:30`), which returns to
  the Product list. Then click the **View** button of the same row (`a.view`, `ContentsButtonEdit_SummaryAdmin.cshtml:18`;
  it opens a new tab): `var popup = await scene.Page.RunAndWaitForPopupAsync(() => scene.ClickAsync(viewButton));`
  `await scene.SwitchToAsync(popup);`, then wait for `popup.Locator("h1")` to contain `Brightwater Trail Backpack`.
  Visible at the end: the item on the site in TheBlogTheme, with the page heading `Brightwater Trail Backpack`. The
  theme prints the display text itself: `src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:9`.

## Chapter 05: Hidden Title field (`05-hidden`)

Start page: `/Admin/ContentTypes/List`, with the Product row in view. Starts from S4.

- `hidden-1`: `ClickAndWaitForNavigationAsync(a[role='btn-edit-Product'])`, scroll the Title row into view,
  `ClickAndWaitForNavigationAsync(a[href$='/ContentTypes/Product/ContentParts/TitlePart/Edit'])`,
  `SelectAsync(select[id$='Options'], "GeneratedHidden")` (label **Title is generated and input is hidden**),
  `HighlightAsync(#patternEditor, "Pattern")` for about a second (the pattern of the previous clip is still there),
  `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(button.save[name='submit.Save'])`. Visible at the end:
  **Edit Content Type - Product** with the message `The "Title" part settings have been saved.`. Labels:
  `TitlePartSettings.Edit.cshtml:21`.
- `hidden-2`: `NavigateAsync("/Admin/ContentTypes/List")`, scroll the Product row into view,
  `ClickAndWaitForNavigationAsync(#btn-create-Product)`, then `TypeAsync(GetByLabel("Brand"), "Brightwater")`,
  `TypeAsync(GetByLabel("Name"), "Water Bottle")`, `ClickAndWaitForNavigationAsync(button.publish)`. Visible while the
  editor is open: no Title field at all, only Brand and Name (`OrchardCore.Title/Views/ContentPart-TitlePart.Edit.cshtml:19-22`
  skips the row for `GeneratedHidden`). Visible at the end: the Content Types list with `Your Product has been published.`.
- `hidden-3`: `ClickAndWaitForNavigationAsync(a[role='btn-list-Product'])`, then `HighlightAsync` the title link
  **Brightwater Water Bottle**. Visible at the end: the Product list with three rows: Brightwater Water Bottle,
  Brightwater Trail Backpack, Brightwater Camp Stove.

## Chapters 00 and 99: the cards

Drawn by `assemble.py` from the storyboard: the opening card has the title and subtitle, the closing card ends with
"Thanks for watching." No recording.

## Checks for private data

- The only account is `admin` with `admin@example.com`; the Blog recipe adds sample content with no real people. Look at
  the user menu in every contact sheet.
- The password never appears: the site is signed in by `state.json` before the capture, and no clip types it.
- Product names, the brand "Brightwater" and every typed value are invented; the host is `localhost`.
- Scan the finished video by OCR at one frame per second and at every scene change (SKILL.md, section 6), looking for a
  real name, an email address other than `@example.com`, a path of the recording machine, or a token.
- Do not commit `state.json`, `App_Data`, audio, clips or the MP4.
