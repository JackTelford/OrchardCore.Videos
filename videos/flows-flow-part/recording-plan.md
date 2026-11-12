# Recording plan: Flow part

Read this file and nothing else to record the video. A selector below stands for `scene.Page.Locator(...)`. Every
label comes from the Orchard Core source at commit `92db4a83` (paths are under `src/OrchardCore.Modules`, except where
a theme or the docs are named), and was read there, not seen on a running site: where a step says "confirm", the
recorder checks that on the first take and writes the answer in `OPEN-QUESTIONS.md`.

## The site it needs

| | |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` whose Flows module has the Blocks editor (commit `92db4a83` or a later `main`). Run `src/OrchardCore.Cms.Web` on `http://localhost:5010`. |
| Data | `ORCHARD_APP_DATA` points to an empty folder outside both repositories. |
| Setup | Auto Setup, so no setup or sign-in form is recorded. Environment variables: `OrchardCore__AutoSetup__AutoSetupPath=` (empty), `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__RecipeName=Blog`, `...__DatabaseProvider=Sqlite`. |
| Recipe | **Blog**. It enables `OrchardCore.Flows` and `OrchardCore.Widgets` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:60` and `:75`), sets `TheBlogTheme` as the site theme and `TheAdmin` as the admin theme, and creates the content types this video uses: **Page** with Title, Autoroute and Flow parts (`:444-497`), and the widget types **Blockquote**, **Container**, **Image**, **Paragraph** and **Raw Html** (`:320-441`). The **Article** type has no Flow part (`:131-186`). |
| Features to enable | None. Do not enable or disable anything. |
| Settings to save | None. The setting **Display New menu** (Settings, Admin; `OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:38`) is off unless it is saved, and the Blog recipe does not set it, so the sidebar is expected to have no **New** menu: the page is created from the **Content Items** screen either way. |
| External services | None. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`) so the site never calls out. |
| Seed data | None. The only data of the video is the page **Hillside Bakery**, created on camera in `03-build`. |
| Sign-in | After the site has started and set itself up, run `Recorder setup` once with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` (the password is chosen outside git). It saves `state.json` (git-ignored). |
| Browser | The recorder's own: 1536 x 864 CSS pixels, light color scheme, `en-US`. The side menu of the admin is collapsed unless a step needs it. |

Fake values to type, exactly:

| Where | Text |
|---|---|
| Page title | `Hillside Bakery` |
| Paragraph widget | `Fresh bread and pastries, baked every morning.` |
| Blockquote widget | `I walk to the shop every Friday for a loaf.` |

## Commands

```bash
# 1. Run the site, from the OrchardCore checkout (Git Bash). The password is chosen outside git.
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
export OrchardCore__AutoSetup__AutoSetupPath=
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
dotnet run --project src/OrchardCore.Cms.Web -c Release --no-launch-profile

# 2. Build the recorder with this video's scenes, and sign in (once the site has set itself up).
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/flows-flow-part/recorder
export VIDEO_ROOT=<repo>/videos/flows-flow-part VIDEO_BASE_URL=http://localhost:5010
export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup

# 3. Narrate, then record (the order is in the next section), and review each contact sheet.
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/flows-flow-part
<bin>/Recorder record 02-part 03-build 04-site
# Stop the site, copy ORCHARD_APP_DATA aside (snapshot B, see the next section), and start the site again.
<bin>/Recorder record 05-blocks
python .agents/skills/orchardcore-video-producer/scripts/review.py videos/flows-flow-part 03-build
```

## Order of the clips, and the site between them

1. `Recorder prepare` is not needed: the video has no `PrepareAsync`.
2. Snapshot **A**: after Auto Setup has finished and `Recorder setup` has run, stop the site and copy its `ORCHARD_APP_DATA` folder aside. The page **Hillside Bakery** does not exist in it yet, so it is the state to restore before every take of `03-build`, and for that clip only.
3. `02-part` changes nothing (it never saves). Record it while the Flow part of the Page type is still on **Standard** (`sett-4` shows **Standard**), that is before `05-blocks` or on a restored snapshot.
4. `03-build` creates and publishes the page **Hillside Bakery**. It must run before `04-site` and `05-blocks`. To record it again, restore snapshot **A**: a take on a site that already holds the page would create a second page, with another address and a second row in the list.
5. `04-site` only reads the published page. It needs `03-build` done and the Page type's editor still on **Standard**.
6. Snapshot **B**: after `03-build` and `04-site`, stop the site and copy `ORCHARD_APP_DATA` aside again.
7. `05-blocks` changes the Page type (its Flow part gets the **Blocks** editor). Record it last. To record it again, or to record `02-part` or `04-site` again afterwards, restore snapshot **B**: the page exists in it and the editor is on **Standard**. Only `03-build` starts from snapshot **A**; after a new take of `03-build`, take snapshot **B** again.

Clips that change the site: `03-build` (creates a page) and `05-blocks` (changes a content type).

Tips for every clip with a form: use `ClickAndWaitForNavigationAsync` for anything that loads a page, clear highlights
before scrolling, and keep each step's actions shorter than its narration (the narration length of each step is in
`durations.json` after `narrate.py` has run).

## Chapter 01: Why the Flow part (slides, no recording)

`01-why` is made of slides drawn by `slides.py`; the recorder does nothing.

- `why-1`: three cards, "Mixed content", "One body field", "Editors in control", all bright.
- `why-2`: three cards, "A list of widgets", "Widgets are content items", "Saved with the page", all bright.
- `why-3`: three cards, "Size", "Alignment", "The theme lays them out", all bright.
- `need-1`: four cards, "The Flows feature" and "The Blog recipe" bright, the other two dimmed.
- `need-2`: the same four cards, "Widget types" and "The Flow part on a type" bright, the other two dimmed.

## Chapter 02: Add the Flow part to a type (clip `02-part`)

**Starts on** (`scene.OpenAsync`, not recorded): `/Admin`. Before the capture, expand the **Design** group of the side
menu and, inside it, **Content Definition**, so that **Content Types** is visible (the narration of `part-1` names this
path). Then `StartCaptureAsync()`. Nothing is saved in this clip.

### `part-1`

- Actions: wait 0.8 s. `ClickAndWaitForNavigationAsync` on the side menu entry **Content Types** (the link whose address ends in `/Admin/ContentTypes/List`; take it from the side menu, not from a breadcrumb). On the list, `ClickAndWaitForNavigationAsync` on the **Edit** button of the **Article** row (`a[href$='/Admin/ContentTypes/Edit/Article']`; the list is drawn as a list or as a grid, and both draw the same button).
- On screen when the narration ends: the **Edit Content Type - Article** screen, with its **Parts** list. Do not scroll yet.
- Labels from: `OrchardCore.ContentTypes/AdminMenu.cs:23-25` ("Design", "Content Definition", "Content Types"); `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` ("Edit"); `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63` ("Edit Content Type - {0}"). The Article type is `blog.recipe.json:131`; it has no Flow part, which is why it is used here (Page has one).
- Confirm: that the **Design** group and its **Content Definition** group open by a click in the sidebar of the Blog recipe, that exactly one side menu link leads to `/Admin/ContentTypes/List`, and that the **Edit** button of the Article row is reachable without scrolling.

### `part-2`

- Actions: `ScrollAsync` down until the **Add Parts** button, below the **Parts** list, is in view (skip when it already is). `ClickAndWaitForNavigationAsync` on **Add Parts** (`a[href$='/Admin/ContentTypes/AddPartsTo/Article']`). Wait 0.8 s. `HighlightAsync` the row of the part named **Flow**: the `li.list-group-item` that contains the label `Flow` (exact text, `label.form-check-label`). Hold.
- On screen when the narration ends: the Add Parts list with the **Flow** row highlighted, its description visible ("Provides a customizable body for your content item where you can build a content structure with widgets."), and the **Add & Configure** button of that row visible on the right. The page title reads "Add Parts" (breadcrumb "Content Types > Edit Content Type - Article > Add Parts").
- Labels from: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201` ("Add Parts"), `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:10` ("Add Parts"), `:34` (the description), `:36` ("Add & Configure"). The part is listed as "Flow" because a part's display name drops the word "Part" (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`). The description is `OrchardCore.Flows/Migrations.cs:21`. Only attachable parts that the type does not have are listed (`OrchardCore.ContentTypes/Controllers/AdminController.cs:255-282`).
- Confirm: that **Flow** is listed for Article, and the exact position of the row on a 864 px high viewport (scroll to it before the highlight if needed; clear highlights before scrolling).

### `part-3`

- Actions: `ClearHighlightsAsync()`. `HighlightAsync` the **Add & Configure** button of the Flow row; hold about 2 s; do not click it. `ClearHighlightsAsync()`. `NavigateAsync("/Admin/ContentTypes/Edit/Page")`. `HighlightAsync` the **Flow** row of the **Parts** list; hold 1 s. `ClearHighlightsAsync()`. `ClickAndWaitForNavigationAsync` on the **Edit** button of the Flow row (`a.btn` with text `Edit` inside the list item that contains `Flow`).
- On screen when the narration ends: the **Edit Part - Flow** screen, with the **Restrict Content Types** block and its checkboxes **Select all**, **Blockquote**, **Container**, **Image**, **Paragraph**, **Raw Html**, all unchecked. A green note about the Part Definition, and perhaps shortcut buttons of the Placements and Templates features, sit above that block; they are not mentioned.
- Labels from: `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:36` ("Add & Configure"); `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144` ("Parts") and `:163` ("Edit"); `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22` ("Edit Part - {0}"); `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml:10` ("Select all"). The part order of the Page type (Title, Autoroute, Flow, Page) comes from the positions in `blog.recipe.json:444-497`.
- Confirm: that the type list shows **Flow** (not "FlowPart") and that the **Edit** button of that row is reachable without scrolling; scroll first (before the capture, or in the step with `ScrollAsync`) if not.

### `sett-1`

- Actions: `HighlightAsync` the whole **Restrict Content Types** block (label, hint "The content types that this flow can contain." and the checkboxes). Do not check any box. Hold.
- On screen when the narration ends: the highlighted **Restrict Content Types** block, every box unchecked.
- Labels from: `OrchardCore.Flows/Views/FlowPartSettings.Edit.cshtml:6` ("Restrict Content Types") and `:8` (the hint). The behavior (no box checked means every Widget type) is `OrchardCore.Flows/Drivers/FlowPartDisplayDriver.cs`, method `GetContainedContentTypesAsync`.

### `sett-2`

- Actions: `ClearHighlightsAsync()`. `HighlightAsync` the **Collapse Contained Items** checkbox with its hint ("When checked, the contained items will be collapsed when the edit form is loaded."); hold. Do not change it.
- On screen when the narration ends: the highlighted **Collapse Contained Items** checkbox, unchecked, with its hint.
- Labels from: `OrchardCore.Flows/Views/FlowPartSettings.Edit.cshtml:17` ("Collapse Contained Items") and `:18` (the hint). That an editor can expand a collapsed widget again is shown in `build-7`.

### `sett-3`

- Actions: `ClearHighlightsAsync()`. `HighlightAsync` the list under **What default alignment should be used?**, which reads "Please choose an alignment". Do not change it. Hold.
- On screen when the narration ends: the highlighted alignment list showing "Please choose an alignment", with its label.
- Labels from: `OrchardCore.Flows/Views/FlowPartSettings.Edit.cshtml:24` ("What default alignment should be used?") and `:27` ("Please choose an alignment"). The default (Justify when nothing is chosen) is `OrchardCore.Flows/Controllers/AdminController.cs:136-141`, applied to a new widget at `:64`.

### `sett-4`

- Actions: `ClearHighlightsAsync()`. `ScrollAsync` down until **What type of editor should be used?** is in view if it is not. `HighlightAsync` that list; it shows **Standard**. Do not change it, and do not click **Save**.
- On screen when the narration ends: the highlighted editor list showing **Standard**, and the **Save** and **Cancel** buttons at the bottom of the screen.
- Labels from: `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:70` ("What type of editor should be used?"), `OrchardCore.Flows/Views/FlowPart.Option.cshtml:4` ("Standard") and `OrchardCore.Flows/Views/FlowPart-Blocks.Option.cshtml:4` ("Blocks"). The Flow part defines no display mode options, so no "What type of display mode should be used?" list follows (`EditTypePart.cshtml:14`, `:88`): the editor list is the last setting.

## Chapter 03: Build a page (clip `03-build`)

**Starts on** (`scene.OpenAsync`, not recorded): `/Admin`. Before the capture, expand the **Content** group of the side
menu so **Content Items** is visible. Then `StartCaptureAsync()`. This clip creates and publishes a page.

### `build-1`

- Actions: `ClickAndWaitForNavigationAsync` on the side menu entry **Content Items**. Then `ClickAsync` the **New** button (`#new-dropdown`) and hold 1 s so the list opens: it holds the creatable types, among them **Article**, **Blog** and **Page**. Then `ClickAndWaitForNavigationAsync` on the **Page** item of that list: the `a.dropdown-item` inside the menu that follows `#new-dropdown`, whose text is exactly `Page` (an unscoped `a.dropdown-item` also matches links of other menus on the screen, and a text match without `Exact` also matches any longer name).
- On screen when the narration ends: the page editor, titled **New Page**, with the **Title** field focused, and the **Flow** section below it.
- Labels from: `OrchardCore.Contents/AdminMenu.cs:65` ("Content") and `:68` ("Content Items"); `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-25` ("New", then the type names); `OrchardCore.Contents/Views/Admin/Create.cshtml:26` ("New {0}").
- Confirm: that the admin list of this version still has the **New** button (`#new-dropdown`) and not another control, and whether the **Content** menu group has to be clicked to open.

### `build-2`

- Actions: `HighlightAsync` the **Title** field for 0.8 s, then the **Permalink** label for 0.8 s, then the **Flow** label with its description under it for 1.2 s. `ClearHighlightsAsync()`. Then `TypeAsync` in the **Title** field (`GetByLabel("Title", Exact = true)`) the text `Hillside Bakery`. Do not touch the **Permalink** field.
- On screen when the narration ends: **Title** filled with "Hillside Bakery", the **Permalink** field below it empty, with its hint "The url of the content item. Leave empty to auto-generate it.", and on the left of the **Flow** area the label "Flow" with its description in small gray text; the plus button of the Flow editor (a small blue button with a plus icon, on a thin blue line) is in view. Scroll down by hand before the capture if the plus button is below the fold.
- Labels from: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` ("Title"); `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30` ("Permalink") and `:46` (the hint, shown because the Page type has a pattern and allows a custom path: `blog.recipe.json:473-474`); `OrchardCore.Contents/Views/ContentPart-FlowPart.Edit.cshtml:21-26` (the label "Flow", from the part's display name, and its description). The Permalink stays empty in the editor: the script of the Autoroute editor only handles the Disabled toggle (`OrchardCore.Autoroute/Assets/ts/autoroute-part.ts`), so the narration's "generated from the title" rests on the pattern `{{ Model.ContentItem | display_text | slugify }}`.
- Confirm: that the Permalink is empty after the title is typed, and that the page gets the address `hillside-bakery` once published (see `04-site`).

### `build-3`

- Actions: `ClearHighlightsAsync()`. `ClickAsync` the plus button under the Flow editor (`.btn-widget-add-below button.dropdown-toggle`; its tooltip "Add Widget" is a `title` only and is never drawn). Hold 1.5 s with the list open: **Blockquote**, **Container**, **Image**, **Paragraph**, **Raw Html**, in that order. `ClickAsync` the **Paragraph** item (`a.add-widget[data-widget-type='Paragraph']`). Wait until `.widget-editor[data-content-type='Paragraph']` is visible. `TypeAsync` (delay 40) in its rich text area (confirm the selector: `.widget-editor[data-content-type='Paragraph'] .trumbowyg-editor`) the text `Fresh bread and pastries, baked every morning.`
- On screen when the narration ends: one card headed "Paragraph", with the sentence in its text area, and the plus button below the card.
- Labels from: `OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72` ("Add Widget", the title of the plus button), `:82-96` (the items, ordered by display name; `data-widget-type` is the technical name of the type: `Blockquote`, `Container`, `Image`, `Paragraph`, `RawHtml`); `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:29-38` (the card and its header text); the Paragraph widget has one HTML field with the WYSIWYG editor (`blog.recipe.json:398-418`, `:759-772`).
- Confirm: the selector of the rich text area; that the header reads "Paragraph".

### `build-4`

- Actions: `ClickAsync` the plus button under the cards. `ClickAsync` the **Blockquote** item (`a.add-widget[data-widget-type='Blockquote']`). Wait until `.widget-editor[data-content-type='Blockquote']` is visible. `TypeAsync` (delay 40) in the text box of that card (`.widget-editor[data-content-type='Blockquote'] textarea`) the text `I walk to the shop every Friday for a loaf.`
- On screen when the narration ends: two stacked cards, "Paragraph" then "Blockquote", each full width, the quote typed in the second.
- Labels from: `OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72-96`; the Blockquote widget has one text field with a text area editor (`blog.recipe.json:352-372`, `:707-722`).

### `build-5`

- Actions: `MoveToAsync` the header of the Paragraph card (`.widget-editor[data-content-type='Paragraph'] .widget-editor-header`) and wait until the size button is visible: the controls sit above the card's top edge and only appear while the cursor is over the header (a gray button group with an alignment button, a size button reading **100%**, a button with two arrows, and a blue plus button). `ClickAsync` the size button (`.widget-editor[data-content-type='Paragraph'] .btn-widget-metadata button` with text `100%`), hold 1 s, `ClickAsync` the item **50%** of that card (`.widget-editor[data-content-type='Paragraph'] label.dropdown-item` with text `50%`; always scope the item to its card, because every card holds the same six labels and an unscoped locator matches several elements). Then the same for the Blockquote card: `MoveToAsync` its header, `ClickAsync` its size button, `ClickAsync` **50%** scoped to `.widget-editor[data-content-type='Blockquote']`.
- On screen when the narration ends: the Paragraph card and the Blockquote card side by side in one row, each half the width, the cursor over the Blockquote header with the size button reading **50%**.
- Labels from: `OrchardCore.Flows/Views/FlowMetadata.Edit.cshtml:35` (the size button shows the current size) and `:39-54` (items "25%", "33%", "50%", "66%", "75%", "100%"); the new widget's size is 100 (`src/OrchardCore/OrchardCore.Flows.Core/Models/FlowMetadata.cs`). The controls appear on hover because of `OrchardCore.Flows/Assets/scss/flows.edit.scss` (`.btn-widget-metadata` is collapsed until `.widget-editor-header:hover`). The row layout changes when the card width changes: `OrchardCore.Flows/Assets/ts/flows.edit.ts` (the `change` handler for the size radio buttons).
- Confirm: that hovering the header makes the controls clickable, and that the second card moves up beside the first once both are at 50%. The narration of this step ends with "they sit side by side": the frame at its end must show the two cards in one row.

### `build-6`

- Actions: `MoveToAsync` the header of the Blockquote card. `ClickAsync` its alignment button (the first button of the group, with the justify icon: `.widget-editor[data-content-type='Blockquote'] .btn-widget-metadata button.dropdown-toggle` that contains `i.fa-align-justify`). Hold 1.5 s with the list open: **Left**, **Center**, **Right**, **Justify**, **Inherit**. `ClickAsync` **Center** (`.widget-editor[data-content-type='Blockquote'] label.dropdown-item` with text `Center`, scoped to the card).
- On screen when the narration ends: the alignment button of the Blockquote card shows the center icon; the two cards are still side by side.
- Labels from: `OrchardCore.Flows/Views/FlowMetadata.Edit.cshtml:16-28`. A widget's default alignment is Justify (`OrchardCore.Flows/Controllers/AdminController.cs:136-141`, since no default is set on this type).

### `build-7`

- Actions: `ClickAsync` the collapse arrow in the header of the Paragraph card (`.widget-editor[data-content-type='Paragraph'] .widget-editor-btn-collapse`; a small outlined button with a down arrow). Hold 1.2 s: the card shows only its header. `ClickAsync` the expand arrow of the same card (`.widget-editor[data-content-type='Paragraph'] .widget-editor-btn-expand`; scoped, because every card has one, and only the card that is collapsed shows it).
- On screen when the narration ends: the Paragraph card expanded again, with its sentence visible.
- Labels from: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:36-37` (the two arrow buttons have no text); the toggle is `OrchardCore.Flows/Assets/ts/flows.edit.ts` (the `.widget-editor-btn-toggle` handler, which toggles the class `collapsed` on the card).
- Confirm: which of the two arrows is shown for each state.

### `build-8`

- Actions: `DragToAsync` from the handle of the Blockquote card (`.widget-editor[data-content-type='Blockquote'] .widget-editor-handle`, a four-arrow icon at the left of the header) to the Paragraph card's header, with a negative `offsetX` (about -150) so the drop lands on the left half. Wait 1 s.
- On screen when the narration ends: the Blockquote card on the left and the Paragraph card on the right, in one row.
- Labels from: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:34` (the handle); the sortable behavior is initialized by `OrchardCore.Flows/Assets/ts/flow-part.ts` and `flows.edit.ts` (`initFlowSortableWidgets`). The sortable library itself comes from the `@orchardcore/bloom` package, which is not in the source read for this plan.
- Confirm: that `DragToAsync` reorders the cards. If it does not, see `OPEN-QUESTIONS.md` (question 1) and decide before recording `build-8`.

### `build-9`

- Actions: `MoveToAsync` the header of the Blockquote card (now on the left). `ClickAsync` the blue plus button of its controls (`.widget-editor[data-content-type='Blockquote'] .btn-widget-metadata button[title='Insert Widget']`; the tooltip is not drawn). Hold 1.5 s with the list open. `ClickAsync` the item **Raw Html** of that card's menu (`.widget-editor[data-content-type='Blockquote'] a.insert-widget[data-widget-type='RawHtml']`; scoped, because every card has its own Insert Widget menu with the same items). Wait until `.widget-editor[data-content-type='RawHtml']` is visible.
- On screen when the narration ends: a card headed "Raw Html" at full width, above the row that holds the Blockquote and the Paragraph.
- Labels from: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:51-66` ("Insert Widget" is the title; the items are the display names). A widget is inserted before the card whose button was used: `OrchardCore.Flows/Assets/ts/flow-part.ts` and `flows.edit.ts` (the `.insert-widget` handler uses `beforebegin`).

### `build-10`

- Actions: `ClickAsync` the red trash button in the header of the Raw Html card (`.widget-editor[data-content-type='RawHtml'] .widget-delete`). Wait for the dialog (`#confirmRemoveModal`) and hold 1.2 s: title **Delete**, message "Are you sure you want to remove this item?", buttons **Ok** and **Cancel**. `ClickAsync` **Ok** (`#modalOkButton`). Wait until the Raw Html card is gone.
- On screen when the narration ends: only the Blockquote and Paragraph cards remain, side by side.
- Labels from: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:79` ("Delete", "Are you sure you want to remove this item?"); `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99` ("Ok", "Cancel").

### `build-11`

- Actions: `ClickAndWaitForNavigationAsync` on the **Publish** button (`button.publish`, `name="submit.Publish"`; do not use the arrow next to it). Wait for the list of content items.
- On screen when the narration ends: the list of content items, with a row **Hillside Bakery** of the type Page, published. (A green message "Your Page has been published." appears for a few seconds; the narration does not rely on it.)
- Labels from: `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7-25` ("Publish"); `OrchardCore.Contents/Controllers/AdminController.cs:472` (the message); the redirect to the list comes from the `returnUrl` that the **New** list item carries (`OrchardCore.Contents/Controllers/AdminController.cs`, `CreateInternalAsync`).
- Confirm: the page's address under **Permalink** is `hillside-bakery` (see `04-site`).

## Chapter 04: See the page (clip `04-site`)

**Starts on** (`scene.OpenAsync`, not recorded): `/hillside-bakery`, the published page on the site. If the address is
not that one, open the page with the **View** button of its row in the list of content items (it opens a new tab; use
`SwitchToAsync`). Then `StartCaptureAsync()`.

### `site-1`

- Actions: wait 1 s. `ScrollAsync(0, 400)` (adjust) until the two widgets are fully in view. Do not highlight.
- On screen when the narration ends: the Blockquote on the left and the Paragraph on the right, each taking half the content width, the quote's text centered; the theme's header with the title "Hillside Bakery" may be partly visible above.
- Source of the layout: `src/OrchardCore.Themes/TheBlogTheme/Assets/TheBlogTheme/src/scss/modules/_flow.scss` (the flow is a row) and `_widgets.scss` (size and alignment); `src/OrchardCore.Themes/TheBlogTheme/Views/Widget-Paragraph.liquid` and `Widget-Blockquote.liquid` write the widget's classes. The compiled style (`wwwroot/css/bootstrap-oc.css`) gives a widget of 50% half the width from 576 px wide.
- The address `hillside-bakery` comes from the Autoroute pattern of the Page type, `{{ Model.ContentItem | display_text | slugify }}` (`blog.recipe.json:474`).

## Chapter 05: The Blocks editor (clip `05-blocks`)

**Starts on** (`scene.OpenAsync`, not recorded): `/Admin/ContentTypes/Page/ContentParts/FlowPart/Edit`, the **Edit Part -
Flow** screen of the Page type, scrolled so **What type of editor should be used?** is in view. Then
`StartCaptureAsync()`. This clip saves a change to the Page type: record it last.

### `blocks-1`

- Actions: `SelectAsync` the list `select[data-type-part-editor-select]` with the value `Blocks`. Hold 1 s. `ClickAndWaitForNavigationAsync` on **Save** (`button.save`).
- On screen when the narration ends: the **Edit Content Type - Page** screen with, in the **Parts** list, the Flow row followed by a small badge reading **Blocks** (its tooltip is "Editor"). A green message "The "Flow" part settings have been saved." shows for a few seconds after the redirect; the narration does not rely on it.
- Labels from: `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:70` and `:111` ("Save"); `OrchardCore.Flows/Views/FlowPart-Blocks.Option.cshtml:4` ("Blocks"); `OrchardCore.ContentTypes/Controllers/AdminController.cs` (`EditTypePartPOST`, the message); `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:189` (the badge).

### `blocks-2`

- Actions: `NavigateAsync("/Admin/Contents/ContentItems")`. `ClickAndWaitForNavigationAsync` on the **Edit** button of the row **Hillside Bakery** (`a.btn.edit` in that row). This is the first sentence of the narration ("Let's edit the Hillside Bakery page again."), which lasts about 3 s: wrap the wait for the loaded editor in `OffCameraAsync` (see Confirm). Scroll to the Flow editor and `ClickAsync` the plus button under the cards (`button.flowpart-blocks-picker-button`) as the second sentence starts ("The plus button opens the Select Widget dialog"). Wait until the dialog (`#contentTypePickerModal`) is shown.
- On screen when the narration ends: the dialog titled **Select Widget**, with a **Search** box and a list of categories (**All**) on the left, and on the right one card for each of **Blockquote**, **Container**, **Image**, **Paragraph** and **Raw Html**, each with an **Add** button. The cards follow the order in which the content types were created (`ListWidgetTypeDefinitionsAsync` does not sort), which for the Blog recipe is Container, Blockquote, Image, Paragraph, Raw Html: do not expect an alphabetical order, and do not rely on a position.
- Labels from: `OrchardCore.Flows/Views/FlowPart-Blocks.Edit.cshtml:77` ("Add Widget", the title of the plus button), `:88` and `:120` ("Select Widget"); `OrchardCore.Flows/Views/ContentTypePicker.cshtml:14` ("Search") and `:43` ("Add"); `OrchardCore.Flows/Assets/ts/content-type-picker.ts:124` (the category "All"). The existing cards use the `FlowPartBlocks` card (`OrchardCore.Flows/Views/ContentCard-FlowPartBlocks.Edit.cshtml`).
- Confirm: the time the page editor takes to load (the rich text editor loads scripts); if the step is too short, split the wait with `OffCameraAsync` on a condition specific to the loaded editor (the plus button of the Blocks editor being visible).

### `blocks-3`

- Actions: `HighlightAsync` the list of categories on the left of the dialog (`#contentTypePickerApp ul.content-type-picker-categories`); hold about 3 s. `ClearHighlightsAsync()`. `HighlightAsync` the grid of cards (`#contentTypePickerApp .content-type-picker-grid`); hold. Do not click anything.
- On screen when the narration ends: the dialog still open, the card grid highlighted, the category list showing only **All**, and no picture on any card.
- Labels from: `OrchardCore.Flows/Views/ContentTypePicker.cshtml:17-26` (the category list), `:34-37` (the picture of a card, drawn only when the type has a thumbnail); the **Category** and **Thumbnail** settings of a content type are `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:91` and `:105`, on the Edit Content Type screen; the docs say the same (`src/docs/reference/modules/ContentTypes/README.md`, "Content Type Settings for Block Pickers"). The Blog recipe sets neither (see `OPEN-QUESTIONS.md`, question 9), so the dialog shows only **All** and no pictures: the narration says what a type can have ("can illustrate"), not what the dialog shows here.

### `blocks-4`

- Actions: `TypeAsync` (delay 120) in the **Search** box (`#contentTypePickerApp input[type=search]`) the text `quote`. The list narrows to the **Blockquote** card (its name contains "quote"). Hold 1.5 s. `ClickAsync` the **Add** button of that card (`.content-type-card` that contains `Blockquote`, button `Add`). Wait until the dialog is closed and a second `.widget-editor[data-content-type='Blockquote']` card appears at the end of the Flow editor (the page already holds one Blockquote card, so wait for two matches and use the last one, not the first).
- On screen when the narration ends: the page editor with the dialog closed and a third card, "Blockquote", at the end of the Flow editor. Do not save the page.
- Labels from: `OrchardCore.Flows/Views/ContentTypePicker.cshtml:14` and `:43`; the search matches the display name or the description (`OrchardCore.Flows/Assets/ts/content-type-picker.ts`, `filteredContentTypes`); a widget is inserted and the dialog closes after `contentTypePickerSelectContentType` (same file).

## Chapter 99: closing card

`99-outro` is a card. The recorder does nothing.

## Checks for private data

- The only names on screen are `admin` (the signed-in user), `Hillside Bakery`, and the text above. The side menu, the profile menu and the list of content items must show no other real name.
- The email address of the administrator is `admin@example.com`; it is never typed on camera.
- No password is typed or shown: `VIDEO_PASSWORD` is used by `Recorder setup` only, outside the capture.
- The Blog recipe's own sample content (a blog post, an article, tags) is fake text from the recipe. Look at every contact sheet (`review.py`) for anything else.
- OCR every frame of the result for names, email addresses and tokens (see `SKILL.md`, "Check for private data") before the video goes anywhere.
