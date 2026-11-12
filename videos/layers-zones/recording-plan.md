# Recording plan: layers-zones

How to record the video "Layers: Show Widgets on the Right Pages" without deciding anything. In this plan a selector
stands for `scene.Page.Locator(...)`. Source paths are relative to `src/` of the OrchardCore checkout the site runs
from; the labels were read from that source, and the markup the plan relies on is listed in `OPEN-QUESTIONS.md` where it
could not be checked without a running site.

Timing: a step is held for its narration, about 0.4 seconds per word. Keep the actions of a step shorter than that, and
let a click that loads a page happen where this plan says, so that the frame at the end of the step shows what the
narration describes.

## The site it needs

| Item | Value |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of the OrchardCore checkout, on `http://localhost:5010`, with its own `App_Data` outside both repositories |
| Setup | Auto Setup, recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, culture en-US |
| Administrator | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Themes | `TheBlogTheme` on the site, `TheAdmin` in the admin (both set by the Blog recipe) |
| Features | Nothing to enable: the Blog recipe enables Layers, Widgets, Rules and Contents (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:54`, `:62`, `:71` and `:75`); Scripting is a dependency of Layers (`OrchardCore.Modules/OrchardCore.Layers/Manifest.cs`) |
| Sign-in | One sign-in outside the capture, saved as `state.json` (git-ignored) with `Recorder setup` |
| External services | None. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, as `references/recording.md` says (the Blog theme links Google Fonts and Font Awesome from its layout) |

### Seed data

None. The video is recorded on the content the Blog recipe creates, with nothing added by hand. Before the first
capture, open `/Admin/Layers` (Design, Widgets) and check that the site holds exactly this, since the narration
describes it:

- Zones: `Content` (no widget) and `Footer` (one widget, titled `Footer`, with a badge `Always`). Source: `LayerSettings` at `blog.recipe.json:120`, the widget at `blog.recipe.json:1091`.
- Layers: `Always` (description "The widgets in this layer are displayed on any page of this site.") and `Homepage` (description "The widgets in this layer are only displayed on the homepage."). Source: `blog.recipe.json:1141` to `:1172`.
- The blog post at `/blog/post-1` (`blog.recipe.json:1018`), the About page at `/about` (`blog.recipe.json:1068`) and the blog at `/` (the home route).

If the site holds anything else (another layer, another widget, a different zone list), start again from a fresh
site.

## Order of the clips, and what they change

Record in the order the video plays: `02-page`, `03-zones`, `04-layer`, `05-widget`, `06-site`, `07-move`, `08-manage`.
`01-idea` is made of slides, `00-intro` and `99-outro` of cards; none of them is recorded.

| Clip | Needs | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|
| `02-page` | the fresh site | no | S0 |
| `03-zones` | the fresh site | yes: adds the zone `Header` to the settings | S0 |
| `04-layer` | `03-zones` | yes: adds the layer `Posts` with one condition | S3 |
| `05-widget` | `04-layer` | yes: adds the Paragraph widget `Follow the blog` | S4 |
| `06-site` | `05-widget` | no | S5 |
| `07-move` | `05-widget` | no in the end: the widget is moved to Footer, then Undo puts it back | S5 |
| `08-manage` | `05-widget` | no: Delete is refused because the layer has a widget | S5 |

Snapshots: stop the site and copy its `App_Data` folder and `state.json` aside. S0 is taken after setup and sign-in,
before `02-page`. S3 is taken after `03-zones`, S4 after `04-layer`, S5 after `05-widget`. To record one clip again,
stop the site, restore the snapshot of its row, and start the site again.

Viewport and style: the recorder's defaults (1536 by 864 CSS pixels, light color scheme, en-US).

## The admin menu

Several clips open a page from the admin menu. A first-level item with children and no page of its own is a
`button` with the class `nav-group-label` inside `li.nav-group` (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`, `NavigationItemLink-admin.cshtml`).
The plan writes the two menu steps as:

- Design, Widgets: click `button.nav-group-label:has-text("Design")`, then `ClickAndWaitForNavigationAsync(a[href$="/Admin/Layers"].First)`. Labels: `OrchardCore.Modules/OrchardCore.Layers/AdminMenu.cs:26` and `:27`.
- Settings, Zones: click `button.nav-group-label:has-text("Settings")`, then `ClickAndWaitForNavigationAsync(a[href$="/Admin/Settings/zones"])`. Labels: `OrchardCore.Modules/OrchardCore.Layers/AdminMenu.cs:33` and `:34`; the route is `Settings/{groupId}` (`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`) with the group id `zones` (`OrchardCore.Modules/OrchardCore.Layers/Drivers/LayerSiteSettingsDisplayDriver.cs:14`).

## 00-intro (card), 01-idea (slides), 99-outro (card)

- `intro`: the opening card. Nothing to record.
- `idea-1`: slide "Three parts of a page": three cards, Zone, Widget and Layer, each with one line.
- `idea-2`: slide "What happens for each page": three cards, a visitor opens a page, the rules of layers with widgets are checked, widgets appear in their zones.
- `idea-3`: slide "How a rule decides": three cards, every condition must be true, All and Any groups, no conditions and no widgets.
- `outro`: the closing card. Nothing to record.

## 02-page (Chapter 2, Widgets and Layers)

Start page: `/Admin` (opened before the capture; the admin menu is visible, with no group expanded). Read-only clip.

- `page-1`: click the Design group, then Widgets as described under "The admin menu". Wait until `h3:has-text("Zones")` and `h3:has-text("Layers")` are visible. Highlight the first heading, wait 1 second, clear, highlight the second. Visible at the end: the page "Widgets and Layers" with the Zones column on the left (cards Content and Footer) and the Layers column on the right. Labels: page title `OrchardCore.Modules/OrchardCore.Layers/Views/Admin/Index.cshtml:20`; headings `:34` and `:121`.
- `page-2`: highlight the Footer card `div.layer-zone[data-zone="Footer"]` and, inside it, the badge `span.badge:has-text("Always")`; then highlight the empty Content card `div.layer-zone[data-zone="Content"]`. Clear the highlights. Visible at the end: the Content card with only an Add Widget button, and the Footer card with the widget row `Footer` and its `Always` badge. Source of the badge: `Index.cshtml:57`.
- `page-3`: highlight the layer rows of the right column: the row of Always (checkbox `#Always`, its description) for 2 seconds, then the row of Homepage (checkbox `#Homepage`, its description). Visible at the end: both layers with their descriptions, each row with Edit and Delete buttons. Source of the rows: `Views/Layer.SummaryAdmin.cshtml`, `Layer.Fields.SummaryAdmin.cshtml`, `Layer.Description.SummaryAdmin.cshtml`, `Layer.Buttons.SummaryAdmin.cshtml` (the Edit label is at `:6`, Delete at `:8`); the default list layout is a list with buttons (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`).
- `page-4`: click the checkbox `#Always` (`input.layer-check`). Wait until `div.layer-zone[data-zone="Footer"] li.list-group-item-success` exists (the highlighted row). Pause 2 seconds. Click `#Always` again, and wait until no `li.list-group-item-success` is left. Visible at the end: the checkbox cleared and no highlighted row. Source: `Views/Layer.Checkbox.SummaryAdmin.cshtml:8`, `Assets/ts/layers-index.ts:75` to `:90`; the hint above the list reads "Select a layer to reveal its associated widgets." (`Index.cshtml:124`).

## 03-zones (Chapter 3, Zones)

Start page: `/Admin/Layers`, with no group of the admin menu expanded (reload the page if needed). Changes the site: record on S0.

- `zones-1`: open Settings, Zones as described under "The admin menu". Wait until the label "Available zones for Layers" is visible. Highlight the input `input[name$="Zones"]`. Visible at the end: the Settings page with the field "Available zones for Layers" holding `Content, Footer`. Labels: `OrchardCore.Modules/OrchardCore.Layers/Views/LayerSettings.Edit.cshtml:4`; the value is written by `Drivers/LayerSiteSettingsDisplayDriver.cs` (`string.Join(", ", settings.Zones)`).
- `zones-2`: highlight the hint under the field (`span.hint` of that field, the text begins "A comma separated list of values"), wait 2 seconds, clear. Then click the input, press End, and type `, Header` with `PressSequentiallyAsync` (delay 55). Wait until the input value is `Content, Footer, Header`. Visible at the end: the field reading `Content, Footer, Header`. Hint text: `LayerSettings.Edit.cshtml:9`. The Blog theme renders the section `Header` at `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:35`.
- `zones-3`: read the length of this step's narration from `durations.json` (key `zones-3`, about 8 seconds) and pause until 4.5 seconds before its end, so that the message, which stays for 5 seconds and appears about half a second after the click, is still on screen when the step ends. Then `ClickAndWaitForNavigationAsync(button.save)`. Wait until `.toast.message-success` shows "Site settings updated successfully." Visible at the end: the Settings page with the field reading `Content, Footer, Header` and the green message (it stays for 5 seconds, so it must appear no more than 4.5 seconds before the end of the step; if the pause is longer than the step allows, shorten it). Labels: Save `OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message `OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`; the 5 seconds are `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`.
- `zones-4`: open Design, Widgets as described under "The admin menu". Wait until `div.layer-zone[data-zone="Header"]` exists. Clear highlights, scroll down (`ScrollAsync(0, 400)`) so that the Header card is visible, highlight its Add Widget button `div.layer-zone[data-zone="Header"] button:has-text("Add Widget")`. Visible at the end: the cards Content, Footer and Header, the last one with an Add Widget button. Source: `Index.cshtml:85`.

## 04-layer (Chapter 4, Create a layer)

Start page: `/Admin/Layers` (opened before the capture). Needs `03-zones`; changes the site: record on S3.

- `layer-1`: `ClickAndWaitForNavigationAsync` on the Add button of the Layers column: `a.btn[href$="/Admin/Layers/Create"]` (the only link with that address on the page). Wait until the label "Name" is visible. Visible at the end: the Create Layer page with empty Name and Description fields and a Create button. Labels: `Index.cshtml:150` (Add), `Views/Admin/Create.cshtml:4` (Create Layer), `:11` (Name), `:19` (Description), `:27` (Create).
- `layer-2`: `TypeAsync(input#Name, "Posts")`, then `TypeAsync(textarea#Description, "Pages that show a blog post.")`, then `MoveToAsync` the Create button `input[type="submit"][value="Create"]`, without clicking. Visible at the end: the form filled with `Posts` and the description, the pointer on Create. Source of the name rule: the name is read-only on the next page (`Views/Admin/Edit.cshtml:24`), and a widget stores the layer by name (`Models/LayerMetadata.cs`, `Services/LayerFilter.cs`).
- `layer-3`: `ClickAndWaitForNavigationAsync(input[type="submit"][value="Create"])`. The page "Edit Layer - Posts" opens (`Views/Admin/Edit.cshtml:15`). Wait until the warning `p.alert-warning` with the text "No conditions found. Please add at least one condition." is visible, and highlight it. Visible at the end: the Edit Layer page, the read-only Name `Posts`, the Description, the heading "Rules" with its Add button, and the highlighted warning. Source: `OrchardCore.Modules/OrchardCore.Rules/Views/Items/ConditionGroup.Fields.Summary.cshtml:7`, `Items/Rule.Fields.Summary.cshtml:3`; the rule with no condition is never true: `OrchardCore/OrchardCore.Rules.Core/Services/RuleService.cs:23` to `:29`.
- `layer-4`: clear highlights; click the Add button of the Rules heading `button[data-bs-target="#modalConditions"]` (there is only one while the rule is empty). Wait until `#modalConditions.show` is visible (the window "Available Rules"), and keep every card on screen for 4 seconds, while the narration says that the window lists the kinds of condition. Then highlight `#search-box` (placeholder "Filter"), as the narration reaches the Filter box. `TypeAsync(#search-box, "url")`; wait until the card `#modalConditions .item[data-filter-value="UrlCondition"]` is visible (its key is the name of the condition's factory, the class name: `OrchardCore/OrchardCore.Rules.Abstractions/IConditionFactory.cs:11`, `OrchardCore.Modules/OrchardCore.Rules/Startup.cs:40`; the key is written by `Views/ConditionsModal.cshtml:21`). It shows "Url condition" and the sentence "A url condition evaluates the current url against a value." Visible at the end: the Available Rules window, the Filter box holding `url`, and the card Url condition with its Add button (other cards may stay visible if the filter keeps them; the card of Url condition must be visible). Labels: `OrchardCore.Modules/OrchardCore.Rules/Views/Rule.Summary.cshtml:12`, `Views/ConditionsModal.cshtml:10` and `:15`, `Views/Items/UrlCondition.Fields.Thumbnail.cshtml:3` and `:4`; the filter script is `Assets/ts/conditions-modal.ts`.
- `layer-5`: `ClickAndWaitForNavigationAsync(#modalConditions .item[data-filter-value="UrlCondition"] a.condition-thumbnail)` (it loads a new page). Wait until the heading "Url" and the label "Operation" are visible. `SelectAsync(select[name$="SelectedOperation"], "Starts with")`, then `TypeAsync(input[name$="Value"], "/blog/")`, press Tab. Visible at the end: the Create Rule page with Operation `Starts with` and Value `/blog/`, the Create button below. Labels: `Views/Items/UrlCondition.Fields.Edit.cshtml:5`, `:10`, `:19`; the option text `Services/ConditionOperatorConfigureOptions.cs:25`; Create `OrchardCore.Modules/OrchardCore.Layers/Views/LayerRule/Create.cshtml:19`.
- `layer-6`: `ClickAndWaitForNavigationAsync(button.create)`. The Edit Layer page returns, with the toast "Condition added successfully." (`Controllers/LayerRuleController.cs:125`). Wait until the condition row shows "Url" and "Starts with /blog/" (`Views/Items/UrlCondition.Fields.Summary.cshtml:10`), highlight it for 3 seconds, clear. Then `ClickAndWaitForNavigationAsync(input[type="submit"][value="Save"])` (`Views/Admin/Edit.cshtml:44`), which returns to Widgets and Layers. Visible at the end: the Widgets and Layers page with the layers Always, Homepage and Posts listed, Posts with its description. The click on Save happens about 5 seconds before the end of the step: read the length of the step's narration from `durations.json` (key `layer-6`) and pause until then.

## 05-widget (Chapter 5, Add a widget)

Start page: `/Admin/Layers`. Needs `04-layer`; changes the site: record on S4.

- `widget-1`: click `div.layer-zone[data-zone="Content"] button:has-text("Add Widget")` (`Index.cshtml:85`). The drop-down opens; highlight it `div.layer-zone[data-zone="Content"] ul.dropdown-menu`. `MoveToAsync` the item `div.layer-zone[data-zone="Content"] a.add-list-widget[data-widget-type="Paragraph"]`, without clicking (the scope is needed: the zones Content, Footer and Header each have such an item, and a locator that matches three elements fails). Visible at the end: the open list of widget types of the site (Blockquote, Container, Image, Paragraph, Raw Html), the pointer on Paragraph. Source: `Index.cshtml:87` to `:99`; the types are those of the Blog recipe, with the display names at `blog.recipe.json:321`, `:353`, `:376`, `:399` and `:422`.
- `widget-2`: `ClickAndWaitForNavigationAsync(div.layer-zone[data-zone="Content"] a.add-list-widget[data-widget-type="Paragraph"])`. Wait until `input[name="LayerMetadata.Title"]` is visible. `TypeAsync(input[name="LayerMetadata.Title"], "Follow the blog")`. Highlight the checkbox `input[type="checkbox"][name$="RenderTitle"]` with its label "Render title" and the hint "If checked, the title will be rendered with the widget.", without changing it. Visible at the end: the editor with the Title `Follow the blog`, and Render title unchecked. Labels: `OrchardCore.Modules/OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:4`, `:15`, `:16`.
- `widget-3`: `SelectAsync(select[name$=".Layer"], "Posts")` (the first option reads "Select a layer", `LayerMetadata.Edit.cshtml:25`). Highlight the hint "The layer controls the visibility of this widget." (`:32`) for 2 seconds, clear. Wait until `.trumbowyg-editor` is visible, then `TypeAsync(.trumbowyg-editor, "Read more stories on the blog home page.")` (delay 40). Visible at the end: The layer set to `Posts` and the field Content (the Paragraph widget's HTML field, label "Content" from `blog.recipe.json:767`) holding the sentence. The editor is Trumbowyg (`OrchardCore.Modules/OrchardCore.ContentFields/Assets/ts/html-field-wysiwyg.ts`).
- `widget-4`: scroll to the Publish button if needed (`ScrollAsync`), then `ClickAndWaitForNavigationAsync(button.publish[name="submit.Publish"])` (the first of the split button, not the "and continue" item; `OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`). The page `/Admin/Layers` loads again with the toast "Your Paragraph has been published." (`OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:473`). Highlight the new row in the Content card, `div.layer-zone[data-zone="Content"] li.list-group-item`, which holds the title `Follow the blog` and the badge `Posts`. Visible at the end: the Content card listing the widget with its Posts badge. Click Publish about 6 seconds before the end of the step: read the length of the step's narration from `durations.json` (key `widget-4`) and pause until then.

## 06-site (Chapter 6, See the result)

Start page: `/` (the blog home page, opened before the capture), signed in as admin. Needs `05-widget`; read-only. The layers apply to the site, not to the admin (`Services/LayerFilter.cs:65`), so these pages are the front end.

- `site-1`: `NavigateAsync("/blog/post-1")`. Wait for the page. Scroll (`ScrollAsync`) until the paragraph `article.widget-paragraph` (text "Read more stories on the blog home page.") is in view, then highlight it. Visible at the end: the blog post with the highlighted paragraph. The order of the paragraph and the post body is not read from the source: see `OPEN-QUESTIONS.md`. The rule passes because the request path `/blog/post-1` starts with `/blog/` (`OrchardCore.Modules/OrchardCore.Rules/Services/UrlConditionEvaluator.cs`, `StringOperatorComparer.cs`).
- `site-2`: `NavigateAsync("/about")`. Visible for 3 seconds: the top of the About page with no paragraph in the page. Then `ScrollAsync` to the bottom of the page. Visible at the end: the footer (social icons and the line "Copyright © Your Website 2021") and no paragraph "Read more stories on the blog home page." anywhere on the page. The footer is the widget on `Always` (`blog.recipe.json:1091`, `:1096`).
- `site-3`: `NavigateAsync("/")`. Scroll slowly through the page to its bottom (`ScrollAsync(0, 300)` twice, 1 second apart). Visible at the end: the blog home page with its footer, and no paragraph widget. The homepage request path is `/`, which does not start with `/blog/`.

## 07-move (Chapter 7, Move a widget)

Start page: `/Admin/Layers` (opened before the capture). Needs `05-widget`; read-only in the end. Record on S5.

- `move-1`: `DragToAsync(div.layer-zone[data-zone="Content"] li.list-group-item .properties, div.layer-zone[data-zone="Footer"] li.list-group-item .properties, 0, -10)`: the row `Follow the blog` is dropped on the upper half of the row `Footer`, so that it lands above it. Wait until `#layer-undo-message` is visible (the message "Widget successfully moved. Undo."). Visible at the end: the Footer card listing `Follow the blog` above `Footer`, the Content card without a widget, and the green message with its Undo link at the top of the page. If the viewport cuts the message off, scroll to the top of the page before the end of the step. Source: `Assets/ts/layers-index.ts:9` to `:62` (drag handle `.properties`, group `layers-widgets`, the move is saved by a request as soon as the row is dropped), `Views/Admin/Index.cshtml:24` and `:25`, `:61`.
- `move-2`: highlight the Undo link `#layer-undo-message a`, then `ClickAndWaitForNavigationAsync(#layer-undo-message a)`. The page loads again. Visible at the end: the widget `Follow the blog` back in the Content card with its Posts badge, the Footer card holding only `Footer`, and no message. Source: the Undo link submits the form `#undo-form` with the previous zone and position (`Index.cshtml:23`), handled by `Controllers/AdminController.cs:282`.

## 08-manage (Chapter 8, Edit and delete a layer)

Start page: `/Admin/Layers` (opened before the capture). Needs `05-widget`; changes nothing. Record on S5.

- `manage-1`: highlight the row of Posts in the right column (`li.list-group-item:has(#Posts)`). `ClickAndWaitForNavigationAsync` on its Edit button (`li.list-group-item:has(#Posts) a.btn:has-text("Edit")`). Wait until the page "Edit Layer - Posts" is shown, highlight the Name field `input[readonly]` with the caption "Read-only", then highlight the Description field and the Rules list with its condition. Visible at the end: the Edit Layer page of Posts with the read-only Name, the Description and the condition `Url Starts with /blog/`. Source: `Views/Layer.Buttons.SummaryAdmin.cshtml:6`, `Views/Admin/Edit.cshtml:22` to `:30`.
- `manage-2`: `NavigateAsync("/Admin/Layers")`. Click Delete in the row of Posts (`li.list-group-item:has(#Posts) a.btn-danger:has-text("Delete")`). The confirmation window appears (`#confirmRemoveModal`: title "Delete", text "Are you sure you want to remove this element?", buttons "Ok" and "Cancel"); wait 2 seconds so it can be read, then click `#modalOkButton`. The page loads again with the red message "The layer couldn't be deleted: you must remove any associated widgets first." Visible at the end: the message (an error message stays until it is closed) and Posts still listed, with its widget `Follow the blog` badge in the Content card. Source: `Views/Layer.Buttons.SummaryAdmin.cshtml:8`, `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`, `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:13` to `:62` (the dialog) and `:114` to `:184` (a link with `data-url-af="RemoveUrl UnsafeUrl"` opens the dialog, and the answer Ok submits a POST to the address of the link), `OrchardCore.Modules/OrchardCore.Layers/Controllers/AdminController.cs:263` to `:275`.

## Checks for private data

The site is fresh and holds no real data. Before the video is assembled, check each contact sheet and run the OCR scans the
skill describes for:

- the user name `admin` (the only account) and the address `admin@example.com`, the only address that may appear;
- the text of the Blog recipe (lorem ipsum, the sample post and the line "Copyright © Your Website 2021"), which names no person;
- the addresses in the browser frame: only `localhost` appears, never a real domain;
- no password, no token, and no path of this machine in any frame (the Settings page shows none).

The fake values typed on camera are `Posts`, `Pages that show a blog post.`, `/blog/`, `Follow the blog`, `Read more stories on the blog home page.` and the zone `Header`.
