# Recording plan: seo-meta

How to record the clips of this video without deciding anything. In this file a selector stands for
`scene.Page.Locator(...)`, since the recorder's methods take locators. Source references (`path:line`) are in the
Orchard Core source of the commit the content was written against (`src/...`), and name where each label comes from.

The video has four recordings (`02-enable`, `03-defaults`, `04-part`, `05-page`). Its other parts are the opening card
(`intro`), the closing card (`outro`) and six slides, which are drawn by `slides.py` and need no recording.

## The site it needs

| Item | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` with its own empty `ORCHARD_APP_DATA` folder outside both repositories |
| Setup | Auto Setup, no setup form recorded: recipe **Blog**, tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles` |
| Sign-in | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`; never typed on camera, the session is saved in `state.json` by `Recorder setup`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls nothing |
| Features to enable before recording | none: the Blog recipe already enables Custom Settings, Media, Content Types and Contents, and the other two features SEO depends on come with them (Recipes Core Services with Recipes, which is always enabled; Shortcodes with Shortcode Templates). **The SEO feature must be disabled when recording starts**: clip `02-enable` enables it on camera |
| Settings | none beyond the recipe's |
| External services | none |
| Seed data | none to create. The video uses what the Blog recipe creates: the Article content item **About** (path `/about`, lorem ipsum text) and the pictures `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` in the Media Library (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, steps `content` and `media`) |

The recipe's sample blog post quotes real people: **never open it on camera**. The video only opens the About article.

### Preflight, outside the capture

1. Open `/Admin/Features`, search for SEO: the **SEO** feature (description "Provides SEO meta features") is listed with an **Enable** button, so it is disabled. The **Media Slugify** feature is listed too (its description contains "SEO-friendly"); leave it as it is.
2. Open the Content Items list (`/Admin/Contents/ContentItems`): the item **About** (type Article) is there.
3. Open the Media Library and look at `about-bg.jpg` and `home-bg.jpg`: both must exist, and neither may show a recognizable person. If one does, stop and report it.
4. Open `/Admin/ContentTypes/Edit/Article`: the **Parts** list has no SeoMeta entry.
5. Open `/about` in a visitor window: the page renders.

## Order, dependencies and snapshots

Record in this order, once each: `02-enable`, `03-defaults`, `04-part`, `05-page`.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-enable` | the start state above | Enables the SEO feature (and any feature it depends on that is still disabled). After a restart of the tenant, the content type **Social Meta Settings** exists and **Settings > Social Meta Settings** appears in the admin menu |
| `03-defaults` | `02-enable` | Saves Social Meta Settings: a default description, a default social image with its media text |
| `04-part` | `02-enable` | Adds the SeoMeta part to the Article type and saves its settings: **Display Open Graph** and **Display Twitter** checked |
| `05-page` | `02-enable` and `04-part` (`03-defaults` only makes the site realistic) | Publishes the About article with the SEO values below |

Take snapshots, always with the site stopped: copy `App_Data` and `state.json` aside.

| Snapshot | Taken | Restore it to take again |
|---|---|---|
| S0 | after Auto Setup and `Recorder setup`, before `02-enable` | `02-enable` |
| S1 | after `02-enable` | `03-defaults` |
| S2 | after `03-defaults` | `04-part` |
| S3 | after `04-part` | `05-page` |

To take the whole video again, restore S0 and start again from `02-enable`. Narrate first (`narrate.py`), since the
recorder holds each step for the length of its narration.

The values typed on camera (fake data only):

| Where | Field | Value |
|---|---|---|
| `03-defaults` | Default Meta Description | `A demo blog built with Orchard Core.` |
| `03-defaults` | Default social image | `home-bg.jpg`, media text `Banner picture of the demo blog` |
| `05-page` | Page Title | `About This Demo Blog` |
| `05-page` | Meta Description | `A short introduction to this demo blog and what it covers.` |
| `05-page` | Default social image | `about-bg.jpg`, media text `Banner picture of the About page` |

Nothing else is typed. No account names are entered (Twitter Site, Twitter Creator and Open Graph App Id stay empty).

## Clip 02-enable (chapter 2, Enable the module)

Starts on `/Admin` (open with `scene.OpenAsync("/Admin")`, not recorded), signed in as `admin`, SEO disabled. The
admin menu is collapsed.

### `enable-1`

Actions:

1. `ClickAsync` the **Tools** entry of the admin menu (`#left-nav`, link or button with the text "Tools"): the group opens.
2. `ClickAndWaitForNavigationAsync` the **Features** link of the same menu. Wait until the URL ends with `/Admin/Features`.
3. `TypeAsync` into `#search-box` the text `SEO` (delay 60). Wait until the **SEO** row is visible (its description "Provides SEO meta features" is shown). The search matches the name, the id and the description of a feature, so two rows stay in the list: **SEO** and **Media Slugify**, whose description contains "SEO-friendly". That is expected, not a failed search: the narration is about the **SEO** row only. Do not click **Media Slugify**.

On screen at the end of the narration: the Features page, `SEO` in the search box, the two rows **SEO** and **Media Slugify** (in this order) in the Content Management group, and the **Enable** button of the **SEO** row.

Source: Tools, Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the search box (placeholder
"Search") `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`; the text the search matches (name, id, description)
`src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`; the feature name "SEO" and its
description `src/OrchardCore.Modules/OrchardCore.Seo/Manifest.cs:4,8`; the description of Media Slugify `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:58`.

### `enable-2`

Actions:

1. `ClickAndWaitForNavigationAsync` the **Enable** link of the SEO row, not the one of **Media Slugify**:
   `scene.Page.Locator("[data-filter-value^='SEO OrchardCore.Seo ']").GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true })`
   (the row carries the attribute `data-filter-value` with the name, the id and the description of the feature).
   If it finds nothing, probe the page: the link has an id that starts with `btn-enable-` and ends with the feature id.
2. `OffCameraAsync(() => scene.Page.WaitForSelectorAsync("text=was enabled"))`: enabling restarts the tenant, so the wait is cut out of the clip.
3. `HighlightAsync` the notification.

On screen at the end of the narration: the Features page with the green notification "The feature SEO was enabled.". The
notification names only the feature that was enabled, never the features enabled with it, and on this site the four
features SEO depends on are already enabled by the Blog recipe, so nothing else is enabled. The page is loaded again after
the click, so its search box is empty and the list is complete: the SEO row is not in view. To show it with its
**Disable** button, `TypeAsync` `SEO` into `#search-box` again after the notification is highlighted (this is optional; the narration
only talks about the notification).

Source: the **Enable** link `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`
(the admin settings can draw the row actions as a menu instead of buttons: if the link is not visible, open the menu of the
row first); the notification `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`, whose list is the
features that were asked for (`OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs`, case `Enable`), here only SEO.

After this clip: stop the site and take snapshot S1.

## Clip 03-defaults (chapter 3, Site-wide defaults)

Starts on `/Admin`, SEO enabled (state S1).

### `defaults-1`

Actions:

1. `ClickAsync` the **Settings** entry of the admin menu (`#left-nav`).
2. `ClickAndWaitForNavigationAsync` the **Social Meta Settings** link of the same menu. Wait until the URL ends with `/Admin/Settings/SocialMetaSettings`.
   If the entry is missing, reload the page once (the tenant was restarted by the previous clip); if it is still missing, navigate with `NavigateAsync("/Admin/Settings/SocialMetaSettings")` and note it in the take report.

On screen at the end of the narration: the Social Meta Settings editor, with the field **Default Meta Description** visible.

Source: `src/OrchardCore.Modules/OrchardCore.CustomSettings/AdminMenu.cs:44-45` (the menu item is the display name of the
content type); the display name `src/OrchardCore.Modules/OrchardCore.Seo/Migrations/socialmetasettings.recipe.json:17`;
the route `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`.

### `defaults-2`

Actions:

1. The field is edited with a CodeMirror editor, so click the `.CodeMirror` element and type with the keyboard (do not use `TypeAsync` on the hidden text area, and do not fill it):
   `var wrapper = scene.Page.Locator("div.field-wrapper").Filter(new() { HasText = "Default Meta Description" });`
   `await scene.ClickAsync(wrapper.Locator(".CodeMirror"));`
   `await scene.Page.Keyboard.TypeAsync("A demo blog built with Orchard Core.", new() { Delay = 30 });`

On screen at the end of the narration: the label **Default Meta Description** and the typed sentence in its editor.

Source: label `src/OrchardCore.Modules/OrchardCore.Seo/Migrations/socialmetasettings.recipe.json:158`; the CodeMirror editor
`src/OrchardCore.Modules/OrchardCore.ContentFields/Views/TextField-CodeMirror.Edit.cshtml`.

### `defaults-3`

Actions:

1. `ScrollAsync` down from the top of the page in two or three moves, through the **Content** card, until the labels **Open Graph Type**, **Default Open Graph Description**, **Twitter Card** and **Default Twitter Description** have all been on screen, in that order (it is the order of the page). The fields are empty: do not type in them. The same card also holds **Open Graph Site Name**, **Open Graph App Id**, **Twitter Creator**, **Twitter Site** and **Google Schema**, which are empty too and are not named by the narration. Stop before the **Social** card comes into view.

On screen at the end of the narration: the lower fields of the **Content** card, empty (the last of the four labels above is visible).

Source: labels `src/OrchardCore.Modules/OrchardCore.Seo/Migrations/socialmetasettings.recipe.json:86,182,96,170`; the order of the fields is their `Position` in the same file.

### `defaults-4`

Actions:

1. Scroll on (`ScrollIntoViewIfNeededAsync` on the field below, or `ScrollAsync`) until the card titled **Social** and the field **Default social image** are on screen. The page is made of two cards: the fields without a placement (the text fields, **Default Meta Description** first) are in a card titled **Content**, and the **Social** card with the three picture fields is the last one, below them (see OPEN-QUESTIONS.md, section 1). The other two picture fields of the card, **Open graph image** and **Twitter Image**, stay empty: do not fill them.
2. `HighlightAsync` the card title "Social" (`h5` with that text).
3. `var image = scene.Page.Locator("div.field-wrapper").Filter(new() { HasText = "Default social image" });`
   `ClickAsync(image.GetByRole(AriaRole.Button, new() { Name = "Add media" }))`. Wait until the dialog titled **Select Media** shows the file `home-bg.jpg`.
4. `ClickAsync` the file `home-bg.jpg` in the dialog (`scene.Page.GetByText("home-bg.jpg")`). Wait until the dialog footer shows "1 selected".
5. `ClickAsync` the **OK** button of the dialog footer. Wait until the dialog is closed and the field shows the picture.

On screen at the end of the narration: the **Social** card with the **Default social image** field holding the thumbnail of `home-bg.jpg`, and the buttons **Add media**, **Remove**, **Media text** (the **Media text** button is drawn only while the picture is selected).

Source: card "Social" `src/OrchardCore.Modules/OrchardCore.Seo/placement.json:30-31`; field labels
`src/OrchardCore.Modules/OrchardCore.Seo/Migrations/socialmetasettings.recipe.json:47,60,73` ("Default social image", "Open graph image", "Twitter Image"); **Add media**, **Remove**, **Media text**,
**Select Media**, **OK** `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:119-121,126,137`.

### `defaults-5`

Actions:

1. If the **Media text** button is not shown, click the thumbnail in the **Default social image** field first (it selects the picture; the button is drawn only for a selected picture, `OrchardCore.Media/wwwroot/Scripts/media-field2.js`).
2. `ClickAsync` the **Media text** button of that field. A dialog titled **Enter media text** opens.
3. `TypeAsync` into the dialog's text area `Banner picture of the demo blog` (delay 30), then `ClickAsync` the **OK** button of that dialog (`scene.Page.Locator(".mf-modal-content").GetByRole(AriaRole.Button, new() { Name = "OK" })`).
4. `ClickAndWaitForNavigationAsync` the **Save** button (`button.save`, text "Save"). Wait for the page to reload.
5. `HighlightAsync` the notification "Site settings updated successfully.".

On screen at the end of the narration: the Social Meta Settings page after saving, with the green notification "Site settings updated successfully." ringed in green. The chapter ends on it.

Source: **Media text**, **Enter media text** `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:121-122`;
**Save** `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; notification `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.

After this clip: stop the site and take snapshot S2.

## Clip 04-part (chapter 4, Add the part to a content type)

Starts on `/Admin`, SEO enabled. Record it from snapshot S2.

### `part-1`

Actions:

1. `ClickAsync` **Design** in the admin menu, then **Content Definition**, then `ClickAndWaitForNavigationAsync` **Content Types**. Wait until the list of content types is shown.
2. `ClickAndWaitForNavigationAsync` the **Edit** button of the Article row: `scene.Page.Locator("[role='btn-edit-Article']")`. Wait until the page title contains "Edit Content Type - Article".

On screen at the end of the narration: the Article type editor, with its **Parts** list (the existing parts of the type, none of them SeoMeta; their display names are not read in the video) and the **Add Parts** button.

Source: menu `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`; the **Edit** button
`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; **Parts** and
**Add Parts** `src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144,201`.

### `part-2`

Actions:

1. `ClickAndWaitForNavigationAsync` the **Add Parts** button (a link with that text).
2. Wait for the text "Choose the Parts to add to this Content Type." Find the row of the part: `var row = scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "SeoMeta" });` and `HighlightAsync(row)` for about 2 s. The row reads **SeoMeta** and the description "Provides a part that allows SEO meta descriptions to be applied to a content item."
3. `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Button, new() { Name = "Add & Configure" }))`. The browser lands on the settings of the part (page title "Edit Part - SeoMeta") with the notification `The "SeoMeta" part has been added.`

On screen at the end of the narration: the Edit Part page for SeoMeta, with the five Display checkboxes and the notification above.

Source: the list text `src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18`; the button
**Add & Configure** (written `Add &amp; Configure` in the view) `.../AddPartsTo.cshtml:36`; the part name shown is "SeoMeta"
(the part name `SeoMetaPart` without the end "Part": `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`, used by
`src/OrchardCore.Modules/OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19,27-30`); the description
`src/OrchardCore.Modules/OrchardCore.Seo/Migrations.cs:25`; the notification `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:355`.

### `part-3`

Actions:

1. `HighlightAsync` each label in turn, about 1 s each (`PauseAsync(900)` between): **Display Keywords**, **Display Custom Meta Tags**, **Display Open Graph**, **Display Twitter**, **Display Google Schema** (`scene.Page.GetByText("Display Keywords")`, and so on).
2. `ClearHighlightsAsync`.

On screen at the end of the narration: the five checkboxes, all unchecked, with their hints ("Check to display the Keywords panel." and so on).

Source: `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPartSettings.Edit.cshtml:7,17,27,37,47`; the settings are shown in the part editor
by `src/OrchardCore.Modules/OrchardCore.Seo/Settings/SeoMetaPartSettingsDisplayDriver.cs:23`.

### `part-4`

Actions:

1. `ClickAsync` the checkbox **Display Open Graph** (`scene.Page.GetByLabel("Display Open Graph")`), then the checkbox **Display Twitter**. Leave the other three unchecked.
2. `ClickAndWaitForNavigationAsync` the **Save** button (`button.save`). The browser returns to the Article type editor.
3. `HighlightAsync` the notification `The "SeoMeta" part settings have been saved.`, and scroll so that the **Parts** list shows the **SeoMeta** row.

On screen at the end of the narration: the Article type editor with the notification and a **SeoMeta** row in the Parts list.

Source: labels `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPartSettings.Edit.cshtml:27,37`; **Save**
`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:111`; the notification
`src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:1084`.

After this clip: stop the site and take snapshot S3.

## Clip 05-page (chapter 5, Edit a page)

Starts on `/Admin`, state S3.

### `page-1`

Actions:

1. `ClickAsync` **Content** in the admin menu, then `ClickAndWaitForNavigationAsync` **Content Items**.
2. `ClickAndWaitForNavigationAsync` the title link **About** in the list (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true })`). Wait for the editor.
3. `ClickAsync` the tab **SEO** (`scene.Page.GetByRole(AriaRole.Tab, new() { Name = "SEO" })`). Wait until the field **Page Title** is visible.

On screen at the end of the narration: the editor of About with the tab strip (two tabs, **Content** and **SEO**), the SEO tab selected, and the **Render** checkbox and the **Page Title** field at the top.

Source: menu `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68`; the title link
`src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsTitle_SummaryAdmin.cshtml:10`; the tab "SEO" comes from
`Parts#SEO;50` in `src/OrchardCore.Modules/OrchardCore.Seo/Drivers/SeoMetaPartDisplayDriver.cs:39` and is written by
`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:56,59`.

### `page-2`

Actions:

1. `HighlightAsync` the **Render** checkbox and its hint "Whether to render metadata for this page.". It is checked: do not click it.

On screen at the end of the narration: the Render checkbox checked, ringed in green.

Source: `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPart.Edit.cshtml:9-10`.

### `page-3`

Actions:

1. `ClearHighlightsAsync`, then `TypeAsync(scene.Page.GetByLabel("Page Title"), "About This Demo Blog", 40)`.
2. `HighlightAsync` the hint under the field ("Override the default generated web browser page title (recommended to keep under 70 characters).").

On screen at the end of the narration: the Page Title field holding the typed title, with its hint ringed.

Source: `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPart.Edit.cshtml:16,19`.

### `page-4`

Actions:

1. `ClearHighlightsAsync`, then `TypeAsync(scene.Page.GetByLabel("Meta Description", new() { Exact = true }), "A short introduction to this demo blog and what it covers.", 30)`.
   If the label does not match, use `scene.Page.Locator("textarea[name$='MetaDescription']")`.

On screen at the end of the narration: the Meta Description text area with the sentence, below the Page Title field.

Source: `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPart.Edit.cshtml:24,27`.

### `page-5`

Actions:

1. `HighlightAsync` the **Canonical** field and its hint "Override the default canonical url.". It stays empty. Hold about 4 s.
2. `ClearHighlightsAsync`, then `HighlightAsync` the **Meta Robots** field and its hint "The robots meta to render for this page.". It stays empty.
   The Meta Keywords field is not on the page, because **Display Keywords** is unchecked.

On screen at the end of the narration: the Meta Robots field, empty and ringed, with the Canonical field above it.

Source: `src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPart.Edit.cshtml:43,46,51,54`.

### `page-6`

Actions:

1. `ClearHighlightsAsync`, then scroll until the card titled **Social** and the field **Default social image** are on screen.
2. `var image = scene.Page.Locator("div.field-wrapper").Filter(new() { HasText = "Default social image" });`
   `ClickAsync(image.GetByRole(AriaRole.Button, new() { Name = "Add media" }))`. Wait until the dialog **Select Media** lists `about-bg.jpg`.
3. `ClickAsync` the file `about-bg.jpg` (`scene.Page.GetByText("about-bg.jpg")`), wait for "1 selected", then `ClickAsync` **OK** in the dialog footer.

The fields **Open graph image** and **Twitter image** of the same card stay empty: do not fill them.

On screen at the end of the narration: the **Default social image** field with the thumbnail of `about-bg.jpg`; the fields **Open graph image** and **Twitter image** below it show "No Files".

Source: field labels `src/OrchardCore.Modules/OrchardCore.Seo/Migrations.cs:28,32,36` (on this page the labels are "Default social image",
"Open graph image" and "Twitter image", while Social Meta Settings spells "Twitter Image"); the card `src/OrchardCore.Modules/OrchardCore.Seo/placement.json:18-28`;
**Add media**, **OK**, **Select Media**, "No Files" `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:118-119,126,137`.

### `page-7`

Actions:

1. If the **Media text** button of the **Default social image** field is not shown, click the thumbnail first (it selects the picture; the button is drawn only for a selected picture).
2. `ClickAsync` the **Media text** button of that field. A dialog titled **Enter media text** opens.
3. `TypeAsync` into the dialog's text area `Banner picture of the About page` (delay 30), then `ClickAsync` the **OK** button of that dialog
   (`scene.Page.Locator(".mf-modal-content").GetByRole(AriaRole.Button, new() { Name = "OK" })`). Wait until the dialog is closed.

Without this text the alternative text of the page's pictures would come from the picture of Social Meta Settings (`Banner picture of the demo blog`): see the check after this clip.

On screen at the end of the narration: the Social card, with the thumbnail of `about-bg.jpg` and the dialog closed.

Source: **Media text**, **Enter media text**, **OK** `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:121-122,126`;
the media text of the page's Default social image is used as the alternative text of both the Open Graph and the Twitter image
`src/OrchardCore.Modules/OrchardCore.Seo/Handlers/SeoMetaPartHandler.cs:94-101,132-139`.

### `page-8`

Actions:

1. `ScrollAsync` down to the card titled **Open Graph**. `HighlightAsync` its title (`h5` with the text "Open Graph") and leave its fields empty: **Open Graph Type**, **Open Graph Title**, **Open Graph Description**.
2. About 5 s later, `ClearHighlightsAsync`, scroll to the card titled **Twitter**, and `HighlightAsync` its title. Its fields are empty: **Twitter Title**, **Twitter Description**, **Twitter Card**, **Twitter Creator**, **Twitter Site**.

On screen at the end of the narration: the Twitter card with its five empty fields (or as many as fit), its title ringed.

Source: cards `src/OrchardCore.Modules/OrchardCore.Seo/Drivers/SeoMetaPartDisplayDriver.cs:50,63` (the card titles are written by
`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:89`); labels `.../Views/SeoMetaPartOpenGraph.Edit.cshtml:4,12,20` and `.../Views/SeoMetaPartTwitter.Edit.cshtml:4,12,20,28,36`.
There is no **Google Schema** card, because **Display Google Schema** is unchecked.

### `page-9`

Actions:

1. `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`. The button is at the bottom of the editor; the split button's arrow is not used.
2. Wait for the notification "Your Article has been published." and `HighlightAsync` it.

On screen at the end of the narration: the Content Items list with the notification "Your Article has been published.".

Source: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`; the notification
`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:967`.

After this clip, outside the capture, check the result once (this is not filmed): open `/about` in a visitor window and run
`document.title` (expected: `About This Demo Blog`) and `document.head.innerHTML`. Expected in the page head: a description meta
tag with the Meta Description of the page; a canonical link to the absolute address of `/about`; the Open Graph and Twitter entries for the title
(`About This Demo Blog`) and the description (the Meta Description of the page); the Open Graph image and the Twitter image
both `about-bg.jpg` (the Default social image of the page), with the alternative text `Banner picture of the About page` for both;
and no `home-bg.jpg` in any Open Graph or Twitter tag. Also open the visitor page of the **Blog** item (a type without the part): it should carry the
description meta tag `A demo blog built with Orchard Core.` and `home-bg.jpg` as the Open Graph and Twitter picture, with the
alternative text `Banner picture of the demo blog`. It carries no Open Graph or Twitter description, because those two fall back to
**Default Open Graph Description** and **Default Twitter Description**, which stay empty, and not to **Default Meta Description**.
Report any difference in the take report: the narration of chapters 1 and 6 says what the code does.

## Slides and cards

No recording. `slides.py` draws them; each line says what the slide shows.

| Step | What it shows |
|---|---|
| `intro` | Opening card: title "The Orchard Core SEO module", subtitle "Titles, descriptions and social sharing cards" |
| `why-1` | Slide "Two places for the values": two cards, "Social Meta Settings" bright and "SeoMeta part" dimmed |
| `why-2` | The same slide with "SeoMeta part" bright and "Social Meta Settings" dimmed |
| `why-3` | Slide "Two formats for shared links": cards "Open Graph" and "Twitter Card" |
| `sources-1` | Slide "Title, description and address": cards "Page Title" (replaces the whole browser title, without the site name), "Meta Description" and "Canonical" with where each value comes from |
| `sources-2` | Slide "Titles and descriptions for sharing": two cards, for the Open Graph and Twitter title and for their description, with their order of fallbacks (the description ends with Default Open Graph Description or Default Twitter Description) |
| `sources-3` | Slide "Pictures, in the order they are used": four numbered cards, from the specific picture of the page to the Default social image of Social Meta Settings |
| `outro` | Closing card: title "The Orchard Core SEO module", subtitle "docs.orchardcore.net" |

## Checks for private data

Before the assembly, look at every contact sheet (`review.py`) and check by OCR at one frame per second and at every scene change:

- The admin shows only the user `admin`; no real person's name, email or phone number appears (the email in this video is `admin@example.com`, and only if a profile screen is opened, which this video never does).
- The password is never typed on camera, and no address bar or token appears (the recording shows only the page).
- The pictures `home-bg.jpg` and `about-bg.jpg` show no recognizable person.
- The fields Twitter Site, Twitter Creator and Open Graph App Id stay empty: no account name or application identifier appears.
- No content item other than **About** is opened. The recipe's sample blog post (its text quotes real people) may appear in the Content Items list as a row title only: its text is never shown.
- The host is `localhost`; no other domain or machine name appears in a notification.
