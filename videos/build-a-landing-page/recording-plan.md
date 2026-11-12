# Recording plan: Build a Landing Page With Flows, Widgets and Layers

For the person who records this video. Everything is decided here: the site, the fake data, the order of the clips, and
what to do and what must be on screen in every step. A selector in this file stands for `scene.Page.Locator(...)`
(`scene.Page.GetByLabel(...)` where it says so). File references are paths in the Orchard Core checkout at the commit
named in `catalog/SOURCE.md`: `M/` is `src/OrchardCore.Modules/`, `T/` is `src/OrchardCore.Themes/`, `C/` is
`src/OrchardCore/`. Where a step says "label" the text is quoted from the file named after it. The audience is
administrators: show only the admin and the front end of the site, never a code editor or a file.

Narrate first (`scripts/narrate.py videos/build-a-landing-page`), then record: the recorder holds each step for the
length of its narration. Type with `delay: 40` so that a step's actions stay shorter than its narration.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of the checkout, on `http://localhost:5010`, with its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`) |
| Provisioning | Auto Setup, no setup screen, no login form on camera (environment variables `OrchardCore__AutoSetup__Tenants__0__...`, see `src/docs/reference/modules/AutoSetup/README.md`) |
| Tenant | `Default` |
| Recipe | `Blog` (`RecipeName`), theme The Blog Theme (`T/TheBlogTheme/Recipes/blog.recipe.json:2,83-86`) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Database | `Sqlite` |
| User | `admin`, `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`, never written in a file of the repository) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The theme asks for web fonts from the internet: when the proxy blocks them the page falls back to the system fonts, which is expected |
| Browser | The recorder's Chromium (1536x864 CSS pixels, scale 1.25, light color scheme, `en-US`) |

### Features and settings: nothing to change

The Blog recipe enables every feature the video uses. Check them once on `/Admin/Features` before recording: Flows,
Widgets, Layers, Rules, Autoroute, Title, Contents, Content Types, Menu (`blog.recipe.json:31-81`). The recipe
also creates what the video starts from. Do not change any of it:

- The settings **Zones**: `Content` and `Footer` (`blog.recipe.json:119-126`).
- The layers `Always` (a Boolean condition set to true) and `Homepage` (a Homepage condition set to true)
  (`blog.recipe.json:1141-1175`).
- The widget content types `Blockquote`, `Container`, `Image`, `Paragraph` and `Raw Html`, all with the Widget
  stereotype (`blog.recipe.json:315-440`; display names at lines 321, 353, 399, 422).
- The content type `Page` with the parts Title, Autoroute and Flow, created from the admin and listed under New
  (`blog.recipe.json:444-499`: `Creatable` is true, `FlowPart` has no restriction, so every widget type is offered).
- One widget already on the site: the `Raw Html` widget **Footer**, in the layer `Always`, zone `Footer`, position 10
  (`blog.recipe.json:1088-1108`).
- Admin display: leave the defaults (list layout List, row actions as Buttons: `C/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`).

### Seed data

None outside the capture. Every clip creates what it needs on camera, with these fake values:

| Where | Value |
|---|---|
| Page title | `Spring Workshop` |
| Permalink | `spring-workshop` |
| Paragraph, field Content | `Join our free two-hour workshop and learn to publish your first website.` |
| Blockquote, field Quote | `Clear, practical and full of examples. Maya Ortiz` |
| Layer name | `Workshop` (one word: the name is also an HTML id) |
| Layer description | `Widgets for the workshop page` |
| Url condition value | `/spring-workshop` |
| Footer widget, Title | `Questions about the workshop?` |
| Footer widget, Content | `Write to workshop@example.com and we will reply within one day.` |

### Admin menu

Before a capture starts, open the page named in the clip and expand the menu group it needs (click the group's label:
`Content`, `Design` or `Settings`). During the capture, click the link inside the group. The menu markup is
`li.nav-group` with a `ul.collapse` of links (`T/TheAdmin/Views/NavigationItem-admin.cshtml:90-93`); a link is found
by its address, for example `a[href$='/Contents/ContentItems']`.

## Order, dependencies and snapshots

Record in this order. Each clip starts from the site the previous changing clip left.

| Clip | Changes the site? | Needs |
|---|---|---|
| `02-page` | Yes: creates the page Spring Workshop | The clean site |
| `03-layer` | Yes: creates the layer Workshop and its Url condition | `S1` (the clip itself does not use the page, but it must be in the site that `06-result` needs) |
| `04-zones` | No (never select Save) | Nothing |
| `05-widget` | Yes: creates the Footer widget in the layer Workshop and moves it | `03-layer` |
| `06-result` | No | `02-page` and `05-widget` |

Snapshots: copy `App_Data` and `state.json` aside (site stopped) at four moments, and restore the right copy, then start
the site again, before every take.

1. `S0`: right after Auto Setup, with the sign-in saved by `Recorder setup`. Restore it before `02-page` and before a full take.
2. `S1`: after `02-page`. Restore it before `03-layer`.
3. `S2`: after `03-layer`. Restore it before `05-widget`.
4. `S3`: after `05-widget`. Restore it before `06-result`; `04-zones` can be recorded from any of them.

Slides need no site. Chapter cards need nothing.

## Opening and closing cards

- `intro` (card `00-intro`): the opening card, drawn by the assembler. Nothing to record.
- `outro` (card `99-outro`): the closing card, drawn by the assembler. Nothing to record.

## Chapter 01: Three building blocks (slides, `01-parts`)

Drawn by `slides.py`; nothing to record.

- `parts-1`: three cards, "Flow" bright: blocks stored inside one page.
- `parts-2`: the same three cards, "Widgets and zones" bright.
- `parts-3`: the same three cards, "Layers" bright.

## Chapter 02: Build the page with a Flow (`02-page`)

Starts on `/Admin` (the admin home page: the Blog recipe does not enable the dashboard), signed in, with the **Content** menu group expanded. The capture starts there.
Steps run on the page **New Page** (address `/Admin/Contents/ContentTypes/Page/Create?returnUrl=...`).

- `page-1`
  - Do: click the link `a[href$='/Contents/ContentItems']` (label **Content Items**, `M/OrchardCore.Contents/AdminMenu.cs:68`)
    and wait for the page **Manage Content** (`M/OrchardCore.Contents/Views/Admin/List.cshtml:17`). Click `#new-dropdown`
    (label **New**, `M/OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-20`). Click the item
    `a.dropdown-item` whose text is exactly `Page` (line 25 of the same file) and wait for the page **New Page**
    (`M/OrchardCore.Contents/Views/Admin/Create.cshtml:26`). Type `Spring Workshop` in `GetByLabel("Title", Exact)`
    (`M/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`). Type `spring-workshop` in `GetByLabel("Permalink")`
    (`M/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`), then press Tab.
  - On screen at the end: the New Page form with the Title `Spring Workshop` and the Permalink `spring-workshop`
    filled in, and the menu group Content expanded on the left.
  - If the Permalink field is disabled, stop: the Page type must allow a custom path (`AllowCustomPath` is true in the recipe, line 473).
- `page-2`
  - Do: scroll down (`ScrollAsync(0, 350)`) until the Flow editor is in view: the section whose label is **Flow**, with its
    description (`M/OrchardCore.Flows/Migrations.cs:19-20`; the label is the part name without "Part",
    `C/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`). Highlight
    `button[title='Add Widget']` (the plus button, `M/OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`), clear the
    highlight, and click it. The menu `div.dropdown-menu` opens.
  - On screen at the end: the open list with five entries in this order: Blockquote, Container, Image, Paragraph, Raw Html
    (sorted by display name, `FlowPart.Edit.cshtml:82`). Do not close it.
- `page-3`
  - Do: click `a.dropdown-item.add-widget[data-widget-type='Paragraph']`. Wait for the card
    `.widget-editor[data-content-type='Paragraph']` and for `.trumbowyg-editor` inside it. Click into
    `.field-wrapper-paragraph-content .trumbowyg-editor` (the field **Content**, a rich text editor, not the hidden
    textarea; `blog.recipe.json:769`) and type `Join our free two-hour workshop and learn to publish your first website.`
  - On screen at the end: one card with the typed text, its header showing the alignment button (an icon) and the size
    button (`100%`) (`M/OrchardCore.Flows/Views/FlowMetadata.Edit.cshtml:10,35`).
- `page-4`
  - Do: in the Paragraph card, click the size button, the `button.dropdown-toggle` whose text is exactly `100%`, then click
    the option `label.dropdown-item` whose text is `50%` (`FlowMetadata.Edit.cshtml:45`).
  - On screen at the end: the size button reads `50%` and the card fills half of the editor width (the script sets the
    class `col-md-6`: `M/OrchardCore.Flows/Assets/ts/flows.edit.ts:127-136`).
- `page-5`
  - Do: click `button[title='Add Widget']` (the plus button under the cards), click
    `a.dropdown-item.add-widget[data-widget-type='Blockquote']`, wait for the card
    `.widget-editor[data-content-type='Blockquote']`. Type `Clear, practical and full of examples. Maya Ortiz` in
    `.field-wrapper-blockquote-quote textarea` (the field **Quote**, `blog.recipe.json:716`). In this card click the size button
    `100%` and the option `50%`; then click the first `button.dropdown-toggle` of the card's `.btn-widget-metadata` (the
    alignment button, an icon) and click the option `label.dropdown-item` whose text is `Center` (`FlowMetadata.Edit.cshtml:19`).
  - On screen at the end: two cards side by side, Paragraph on the left and Blockquote on the right; the alignment menu of
    the Blockquote closed, with the alignment icon changed to the centered one.
- `page-6`
  - Do: drag the handle `.widget-editor[data-content-type='Blockquote'] .widget-editor-handle`
    (`M/OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:34`) onto the Paragraph card, to the left of its centre, with
    `scene.DragToAsync(from, to, offsetX: -40, offsetY: 0)`. The two cards are side by side (each is `col-md-6`), so the
    drop puts the Blockquote before the Paragraph, on its left, not above it. The Sortable list takes the card by its handle
    (`handle: ".widget-editor-handle"` in `M/OrchardCore.Flows/wwwroot/Scripts/flows/flows.edit.js`, built from `flows.edit.ts`).
  - On screen at the end: the Blockquote card on the left and the Paragraph card on the right.
  - If the order does not change, repeat once with `scene.DragAsync` in small moves. If it still does not change, do not
    record the clip: write it in `OPEN-QUESTIONS.md` and report the video as blocked, since `page-6` and `page-8`
    describe the new order.
- `page-7`
  - Do: click the first `button.publish` (label **Publish**, `M/OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`; the page was
    opened with a `returnUrl`, so Publish is the main part of a split button). Wait for the address to contain
    `/Contents/ContentItems`: a create form opened with a `returnUrl` redirects to it
    (`M/OrchardCore.Contents/Controllers/AdminController.cs:795-797`).
  - On screen at the end: the list **Manage Content** with the green notification "Your Page has been published."
    (`M/OrchardCore.Contents/Controllers/AdminController.cs:472-473`) and the row Spring Workshop.
- `page-8`
  - Do: `scene.NavigateAsync("/spring-workshop")`. Wait for `section.flow`.
  - On screen at the end: the front end of the site. The title Spring Workshop in the page header, and under it one row
    with two blocks, the centered quote on the left and the paragraph on the right (`T/TheBlogTheme/Assets/TheBlogTheme/src/scss/modules/_flow.scss`
    makes `.flow` a row, and `_widgets.scss` gives `widget-size-50` half of the row from the small breakpoint up).
    The page is reached without a 404: if it is a 404, the permalink was not saved, so restore `S0` and record again.

## Chapter 03: Create a layer (`03-layer`)

Starts on `/Admin` with the **Design** menu group expanded. Restore `S1` first: the layer and the page must end up in the same
`App_Data`, because `06-result` needs both.

- `layer-1`
  - Do: click the link `a[href$='/Layers']` (label **Widgets**, `M/OrchardCore.Layers/AdminMenu.cs:27`) and wait for the page
    **Widgets and Layers** (`M/OrchardCore.Layers/Views/Admin/Index.cshtml:20`). Highlight the heading `h3` **Zones**
    (line 34), then the heading `h3` **Layers** (line 121), then the two rows `Always` and `Homepage`.
  - On screen at the end: the left column with the zone cards `Content` (empty) and `Footer` (holding the widget Footer,
    tagged with the layer Always), the right column with the layers Always and Homepage.
- `layer-2`
  - Do: click the link **Edit** in the row of Always: `.list-group-item` that has `#Always`, then its `a` with text `Edit`
    (`M/OrchardCore.Layers/Views/Layer.Buttons.SummaryAdmin.cshtml:6`). Wait for the page **Edit Layer - Always**
    (`M/OrchardCore.Layers/Views/Admin/Edit.cshtml:15`). Highlight the rule row (`ol.condition-group li`).
  - On screen at the end: the page Edit Layer - Always, with the heading Rules and one row, "Boolean condition" followed by
    `True` (`M/OrchardCore.Rules/Views/Items/BooleanCondition.Fields.Summary.cshtml:3`).
- `layer-3`
  - Do: click the breadcrumb link **Widgets and Layers** and wait for the page. Click the link `a.btn.btn-primary.btn-sm`
    whose text is exactly `Add` under the layers (`Index.cshtml:150`; the zone buttons Add Widget are `button`
    elements, so they do not match). On the page **Create Layer** (`M/OrchardCore.Layers/Views/Admin/Create.cshtml:4`) type
    `Workshop` in `GetByLabel("Name", Exact)` (line 11) and `Widgets for the workshop page` in `GetByLabel("Description", Exact)` (line 19),
    then click `input[type='submit'][value='Create']` (line 27) and wait for the page **Edit Layer - Workshop**.
  - On screen at the end: the page Edit Layer - Workshop with the Name `Workshop` read-only and the Description filled in.
- `layer-4`
  - Do: highlight the yellow message "No conditions found. Please add at least one condition."
    (`M/OrchardCore.Rules/Views/Items/ConditionGroup.Fields.Summary.cshtml:7`). Clear the highlight. Click
    `button[data-bs-target='#modalConditions']` whose text is `Add` (`M/OrchardCore.Rules/Views/Rule.Summary.cshtml:11-12`)
    and wait for `#modalConditions.show`.
  - On screen at the end: the dialog **Available Rules** (`M/OrchardCore.Rules/Views/ConditionsModal.cshtml:10`) with its
    Filter box and the first condition cards.
- `layer-5`
  - Do: highlight `#search-box` (the placeholder and aria-label **Filter**, `ConditionsModal.cshtml:15`; it has no visible label). Scroll the dialog down (`ScrollAsync(0, 300)`)
    until the cards Role condition and Url condition are visible, then scroll back up.
  - On screen at the end: the dialog from the top, the Filter box highlighted, the cards listed in alphabetical order.
    The cards read: All condition group, Any condition, Boolean condition, Content type condition, Culture condition,
    Homepage condition, Is anonymous condition, Is authenticated condition, Javascript condition, Role condition,
    Url condition (the thumbnails in `M/OrchardCore.Rules/Views/Items/*.Fields.Thumbnail.cshtml`).
- `layer-6`
  - Do: type `Url` in `#search-box` (the cards filter as you type, on `keyup`: the script compares the typed text with the whole text of a card, its title and its
    description, `M/OrchardCore.Rules/Assets/ts/conditions-modal.ts:29-51`. Only the Url condition stays, since no other card's text
    contains "url"; if another card stays, correct this line). Click `.item:visible a.condition-thumbnail` (the card's **Add**
    button, `M/OrchardCore.Rules/Views/Condition.Thumbnail.cshtml:8`) and wait for the page **Create Rule**
    (`M/OrchardCore.Layers/Views/LayerRule/Create.cshtml:4`) with the heading **Url**
    (`M/OrchardCore.Rules/Views/Items/UrlCondition.Fields.Edit.cshtml:5`). Leave the **Operation** list on **Equals** (the first
    entry, `M/OrchardCore.Rules/Services/ConditionOperatorConfigureOptions.cs:13`). Type `/spring-workshop` in `GetByLabel("Value", Exact)`
    (line 19 of the Url edit view). Click `button.create` (label **Create**, `LayerRule/Create.cshtml:19`) and wait for the
    page **Edit Layer - Workshop**.
  - On screen at the end: the green notification "Condition added successfully."
    (`M/OrchardCore.Layers/Controllers/LayerRuleController.cs:125`) and, under Rules, the row "Url" followed by
    "Equals /spring-workshop" with the buttons Edit and Delete.
- `layer-7`
  - Do: highlight the row `ol.condition-group li`. Do not click Save: the condition is already stored
    (`LayerRuleController.cs:122-125`).
  - On screen at the end: the highlighted row Url, Equals /spring-workshop.

## Chapter 04: Check the zones (`04-zones`)

Starts on `/Admin` with the **Settings** menu group expanded. Read only: never select Save.

- `zones-1`
  - Do: click the link `a[href$='/Settings/zones']` (label **Zones**, `M/OrchardCore.Layers/AdminMenu.cs:34`; the page is
    titled **Settings**, `M/OrchardCore.Settings/Views/Admin/Index.cshtml:4`). Highlight `GetByLabel("Available zones for Layers")`
    (`M/OrchardCore.Layers/Views/LayerSettings.Edit.cshtml:4`).
  - On screen at the end: the field holding `Content, Footer`, highlighted.
- `zones-2`
  - Do: clear the highlight, then highlight the hint `span.hint` under the field.
  - On screen at the end: the hint "A comma separated list of values, representing the available zones for widgets. Note that
    you'll need to render these zones in the layout template, as they do not automatically render."
    (`LayerSettings.Edit.cshtml:9`), highlighted.

## Chapter 05: Add a widget to the layer (`05-widget`)

Starts on `/Admin` with the **Design** menu group expanded. Restore `S2` first.

- `widget-1`
  - Do: click the link `a[href$='/Layers']` (**Widgets**) and wait for **Widgets and Layers**. Highlight the card header
    `Footer` and the existing widget in it. In the Footer card (`.layer-zone[data-zone='Footer']`) click the button
    **Add Widget** (`Index.cshtml:85`; it shows because a layer and widget types exist, lines 75-79). The list opens with
    Blockquote, Container, Image, Paragraph and Raw Html. Click `a.add-list-widget[data-widget-type='Paragraph'][data-zone='Footer']`
    and wait for the page **New Paragraph**.
  - On screen at the end: the editor of the widget, titled New Paragraph, with the fields from the next step.
- `widget-2`
  - Do: type `Questions about the workshop?` in `GetByLabel("Title", Exact)` (label **Title**,
    `M/OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:4`). Check `GetByLabel("Render title")` (line 15). Pick `Workshop` in
    `GetByLabel("The layer")` with `scene.SelectAsync` (line 22; the first entry is "Select a layer").
  - On screen at the end: the Title filled, Render title checked, The layer showing Workshop, with the hint
    "The layer controls the visibility of this widget." (line 32) visible below the list.
- `widget-3`
  - Do: click into `.field-wrapper-paragraph-content .trumbowyg-editor` and type
    `Write to workshop@example.com and we will reply within one day.` Click the first `button.publish` (**Publish**) and wait for
    the address to be `/Admin/Layers` (the same redirect to the `returnUrl`).
  - On screen at the end: the page Widgets and Layers with the notification "Your Paragraph has been published." and the new
    widget listed in the Footer zone.
- `widget-4`
  - Do: highlight the new widget's row in the Footer card (the row whose text contains `Questions about the workshop?`; it
    carries a badge with the layer name, `Index.cshtml:57`). Clear the highlight. Click the checkbox `#Workshop`
    (`M/OrchardCore.Layers/Views/Layer.Checkbox.SummaryAdmin.cshtml:8`).
  - On screen at the end: the checkbox Workshop checked and the new widget's row highlighted in green
    (`list-group-item-success`, `layers-index.js`).
- `widget-5`
  - Do: drag the new widget's row, from its `.properties` area, above the row of the widget Footer in the same zone, with
    `scene.DragToAsync(from, to, offsetY: -10)`. Wait for `#layer-undo-message` to lose its `d-none` class.
  - On screen at the end: the new widget above the widget Footer in the Footer zone, and the green message
    "Widget successfully moved. Undo." (`Index.cshtml:25`) above the zones. Do not click Undo.
  - If the order does not change, repeat once in small moves with `scene.DragAsync`; if it still does not, report the video
    as blocked as in `page-6`.

## Chapter 06: See the result (`06-result`)

Starts on `/Admin/Layers`. Restore `S3` first (or record right after `05-widget`).

- `result-1`
  - Do: `scene.NavigateAsync("/spring-workshop")`. Scroll to the bottom of the page. Highlight the footer widget (the
    `footer` element, from the heading `h2` **Questions about the workshop?** to the message).
  - On screen at the end: the bottom of the landing page. The footer holds the widget (title in an `h2`, then the message)
    above the social icons of the footer widget, because the widget was moved above it
    (`M/OrchardCore.Layers/Services/LayerFilter.cs:157`, `T/TheBlogTheme/Views/Layout.liquid`, `footer`).
- `result-2`
  - Do: `scene.NavigateAsync("/")`. Scroll to the bottom of the page. Highlight the `footer` element.
  - On screen at the end: the bottom of the homepage with the social icons and the copyright line of the Always footer
    widget, and no "Questions about the workshop?".
- `result-3`
  - Do: `scene.NavigateAsync("/Admin/Layers")`, then click the checkbox `#Workshop`.
  - On screen at the end: the page Widgets and Layers, Workshop checked, the widget of the layer highlighted in the Footer zone.

## Chapter 07: Flow or layer (slides, `07-choose`)

Drawn by `slides.py`; nothing to record.

- `choose-1`: two cards, "A Flow" bright: content of one page, stored in the page and edited with it.
- `choose-2`: the same two cards, "A layer" bright: widgets shared by many pages, shown wherever the rule is true.

## Checks for private data

Before the clips are assembled, look at every contact sheet and check:

- Only the fake values of the table above appear; the only email address is `admin@example.com` (in the account menu, if
  it is shown) and `workshop@example.com`.
- The password is never visible: the clips start signed in, and no login form is recorded.
- The site is the fresh one from Auto Setup; no copy of a real site, so no mask script is needed.
- No error page, no yellow warning other than the one `layer-4` talks about, no browser extension, and no file path in
  an address bar (the recording shows only the page).
- After assembling, scan the video by OCR (one frame per second and at every scene change) for any `@` or `.com` that is
  not `example.com`.
