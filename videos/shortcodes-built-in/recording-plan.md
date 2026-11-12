# Recording plan: Shortcodes, Embed Images and Links in Your Text

Audience: admin (editors and site owners). The recordings show the admin editor of an article and its published page.
Slides: `why-1`, `why-2`, `why-3`, `which-1`, `where-1`, `where-2` (see the end of the plan). Recorded clips:
`02-image`, `03-options`, `04-list`.

In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Every
`file:line` is in the Orchard Core source at commit `92db4a83`; the plan assumes the same files in the checkout you
build. Read `OPEN-QUESTIONS.md` before the first take: the locators of the body editor come from Trumbowyg 2.31.0,
an npm package whose source is not in the tree (only its minified build is,
`OrchardCore.Resources/wwwroot/Scripts/trumbowyg/trumbowyg.js`), so check them with `Recorder probe` first.

## 1. The site it needs

| What | Value |
|---|---|
| Site | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | The environment variables below, so no setup or sign-in form is recorded |
| Recipe | `Blog` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles` |
| Tenant | `Default` only |
| Admin user | user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address; the video needs no external service |
| Browser | The recorder's defaults (1536x864, scale 1.25, light, en-US). Sign in once with `Recorder setup` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`) |

Auto Setup variables (names from `src/docs/reference/modules/AutoSetup/README.md`):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="<password>"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

### Features

Open **Tools, Features** (`OrchardCore.Features/AdminMenu.cs:26-27`, route `/Admin/Features`) and check, before
recording. Change nothing else.

| Feature (name on the page) | State | Why |
|---|---|---|
| Shortcodes | enabled (the Blog recipe enables Shortcode Templates, which depends on it: `blog.recipe.json:70`, `Manifest.cs`) | The shortcode engine and the Insert Shortcode dialog |
| Media | enabled (`blog.recipe.json:65`) | Provides the `image` and `asset_url` shortcodes (`OrchardCore.Media/Startup.cs:387-433`) and Insert Media |
| Localization | **disabled** | With it, the `locale` shortcode is added to Available Shortcodes (`OrchardCore.Shortcodes/Startup.cs:84-103`). The video lists the two Media shortcodes only, and the `locale` shortcode appears on a slide |

### The media library

**Media, Library** (`OrchardCore.Media/AdminMenu.cs:21`) holds exactly the three files that the Blog recipe adds to
the root folder (`blog.recipe.json`, step `media`): `about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`. Upload nothing.
The video uses `home-bg.jpg` for the image and `post-bg.jpg` for the address.

### The seed data (create it outside the capture)

One article, created in the admin with the keyboard, then published. All text is invented.

1. Sign in, open `/Admin/Contents/ContentItems`, click **New**, then **Article** (`ContentsAdminListCreate.cshtml:19-26`).
2. **Title**: `Spring Garden Tour`. Leave the permalink as it is generated (`/spring-garden-tour`, from the Article's
   autoroute pattern in `blog.recipe.json:158`).
3. **The body** (the rich text editor that comes after the Permalink; `Editor` is `Wysiwyg`, `blog.recipe.json:171`).
   Its label is built from the name of the part (`TypePartDefinition.DisplayName()`, `ContentPartSettingsExtensions.cs:54-62`,
   which removes `Part` from `HtmlBodyPart`), so read the exact label on screen; no step names it. Click in it and type three lines,
   pressing Enter between them:
   - `Join us for a walk through the spring garden. The tulips are in bloom, and our volunteers are glad to answer questions.`
   - `Photo from the entrance:`
   - `Address of the photo:`
4. **Subtitle**: `A walk with the volunteers`. Leave **Banner Image** empty: the page then shows the theme's default
   banner (`TheBlogTheme/Views/Content-Article.liquid`), which is expected.
5. Click **Publish** (`Content.PublishButton.cshtml`).
6. Open `/spring-garden-tour` and check that the three lines show, with no shortcode in them.

Check that the Article list (`/Admin/Contents/ContentItems/Article`) shows two rows: `About` (from the recipe) and
`Spring Garden Tour`.

### Snapshots

Stop the site and copy its `App_Data` folder (and `state.json`) aside at each of these points, then start it again:

| Snapshot | Taken | State |
|---|---|---|
| S0 | After the seed data | The article has three plain lines |
| S1 | After clip `02-image` | The second line ends with the image shortcode for `home-bg.jpg` |
| S2 | After clip `03-options` | The image shortcode has the width and alt options |
| S3 | After clip `04-list` | The third line ends with the `asset_url` shortcode for `post-bg.jpg` |

## 2. Order of the clips

`02-image`, then `03-options`, then `04-list`. They depend on each other through the article they publish: each one
starts from the snapshot the previous one left.

| Clip | Starts from | Changes the site |
|---|---|---|
| `02-image` | S0 | Yes: publishes the article with the image shortcode |
| `03-options` | S1 | Yes: publishes the article with the options |
| `04-list` | S2 | Yes: publishes the article with the `asset_url` shortcode |

To record one clip again, restore the snapshot it starts from (stop the site, restore `App_Data` and `state.json`,
start the site). To record everything again, restore S0.

## 3. Common editor actions

The article editor is `/Admin/Contents/ContentItems/<id>/Edit`; every clip reaches it by clicking **Edit** in the
Article list, so the plan needs no content item id.

- **Open the editor (before the capture in `03-options` and `04-list`, on camera in `02-image`)**: open
  `/Admin/Contents/ContentItems/Article`; in the row whose text contains `Spring Garden Tour`, click `a.btn.edit`, the
  button labeled **Edit** (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`). Narrow the row with
  `Filter(new() { HasText = "Spring Garden Tour" })` on the list item that `Recorder probe` shows. Wait for
  `.trumbowyg-editor` to be visible.
- **The body editor** is a Trumbowyg editor (`HtmlBodyPart-Wysiwyg.Edit.cshtml`): the editable area is
  `.trumbowyg-editor`, and each typed line is a `p` block inside it. Its toolbar buttons have a tooltip equal to
  their name (the build sets `title` from the language table and the class to `trumbowyg-<name>-button`): `button[title='Insert Shortcode']` (`OrchardCore.Resources/Assets/trumbowyg/js/trumbowyg.shortcodes.js:7`) and
  `button[title='Insert Media']` (`OrchardCore.Html/Assets/trumbowyg.media.tag.ts:80`). They are also
  `.trumbowyg-insertShortcode-button` and `.trumbowyg-insertImage-button`. The toolbar and the body must both be
  visible without scrolling at the end of the editor steps.
- **A line** is located with the smallest element that holds its text, never the editor itself: for example
  `Locator(".trumbowyg-editor").GetByText("Photo from the entrance")` (or `Locator(".trumbowyg-editor > p")` filtered
  with `HasText`). The build sets the paragraph separator to `p` and wraps the first typed text in a `p` too, so every
  seed line is a `p`; run `Recorder probe` first and check that the three seed lines are separate `p` elements inside
  `.trumbowyg-editor`. To put the caret at the end of a line: `ClickAsync` that element (a click in
  the middle of a full-width block puts the caret at the end of its text), then
  `scene.Page.Keyboard.PressAsync("End")`.
- **Typing inside the editor** uses `scene.Page.Keyboard.TypeAsync(text, new() { Delay = 60 })` and
  `scene.Page.Keyboard.PressAsync("ArrowLeft")`. Do not use `scene.TypeAsync` there: it clicks the middle of the
  element first and moves the caret.
- **Publish**: `ClickAndWaitForNavigationAsync` on `button.publish` (the **Publish** button,
  `Content.PublishButton.cshtml:19`). The browser returns to the Article list, which shows the notice
  `Your Article has been published.` (`OrchardCore.Contents/Controllers/AdminController.cs:967`, the edit path; `:473` is the same text for a new item).
- **The published page**: `NavigateAsync("/spring-garden-tour")`, then scroll (`ScrollAsync(0, 500)`) until the line
  `Photo from the entrance:` and the lines around it are in view.

## 4. Clip `02-image`

Page it starts on: `/Admin/Contents/ContentItems/Article` (the Article list, with the `Spring Garden Tour` row).
Nothing to prepare. Call `StartCaptureAsync` on that page.

- `image-1` (Article list, then the editor). Actions: `PauseAsync(1800)` on the list; move to **Edit** in the
  `Spring Garden Tour` row and click it with `ClickAndWaitForNavigationAsync`; wait for `.trumbowyg-editor`;
  `HighlightAsync(button[title='Insert Shortcode'], "Insert Shortcode")` and
  `HighlightAsync(button[title='Insert Media'], "Insert Media")`. Visible when the narration ends: the editor with
  the title `Spring Garden Tour`, the three body lines, and the two toolbar buttons ringed with their captions.
  Labels: **Edit** from `ContentsButtonEdit_SummaryAdmin.cshtml:13`; the button names from the files above.
- `image-2` (the media library dialog). Actions: `ClearHighlightsAsync()`; click the line `Photo from the
  entrance:` and press End; click `button[title='Insert Media']`; wait until `.mf-picker-shell` is visible and a
  `.file-item` row is shown (`OrchardCore.Media/Assets/media-picker/src/components/MediaPickerModal.vue`,
  `Assets/media-gallery/src/components/FileItemsComponent.vue:31`; the dialog first shows a loading line). Visible
  when the narration ends: the dialog with the rows `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg`. The picker opens
  in the table (list) view, which is the default (`media-gallery/src/services/LocalStorage.ts:9`); if it shows
  thumbnails, click the view toggle (its tooltip is the table view label) before the capture next time.
- `image-3` (select and confirm). Actions: click the checkbox of the `home-bg.jpg` row:
  `Locator(".file-item").Filter(new() { HasText = "home-bg.jpg" }).Locator("input[name='select-file']")`
  (`FileItemsComponent.vue:35`); the footer button `.mf-btn-primary` is enabled once a file is selected
  (`MediaPickerModal.vue:43-50`); click it; wait until `.mf-picker-shell` is gone and the editor text contains
  `[image]home-bg.jpg[/image]`. Visible when the narration ends: the dialog closed, and the second line reading
  `Photo from the entrance: [image]home-bg.jpg[/image]` (the plugin adds a space before the shortcode,
  `trumbowyg.media.tag.ts:99`). Do not name the confirm button in the capture; it is the dialog's own button.
- `image-4` (publish and look). Actions: click **Publish** (`button.publish`) and wait for the Article list with its
  notice; `NavigateAsync("/spring-garden-tour")`; wait for `img[src*='home-bg.jpg']`; scroll until the line and the
  picture are in view. Visible when the narration ends: the published page with the line `Photo from the entrance:`
  and the picture of `home-bg.jpg` under it (at the size of the file; do not resize it). Keep the actions shorter
  than the narration, about 8 seconds.

After the clip, take snapshot S1.

## 5. Clip `03-options`

Starts from S1. Before `StartCaptureAsync`: open the editor as in section 3 (not recorded) and check that the second
line reads `Photo from the entrance: [image]home-bg.jpg[/image]`. The capture starts on the editor, as it was at the
end of `image-3`.

- `options-1` (the width option). Actions: click the line `Photo from the entrance` and press End; press
  `ArrowLeft` 20 times (the length of `]home-bg.jpg[/image]`: 1 + 11 + 8), which puts the caret right after
  `[image`; type ` width="480"` (a space first) with `Keyboard.TypeAsync` and a 60 ms delay. Visible when the
  narration ends: `Photo from the entrance: [image width="480"]home-bg.jpg[/image]`. `HighlightAsync` on the block
  without a caption is optional.
- `options-2` (the alt option). Actions: the caret is right after `width="480"`; type ` alt="Spring tulips"` the same
  way. Visible when the narration ends: `[image width="480" alt="Spring tulips"]home-bg.jpg[/image]` on the second
  line.
- `options-3` (publish and look). Actions: `ClearHighlightsAsync()`; click **Publish**; wait for the Article list;
  `NavigateAsync("/spring-garden-tour")`; wait for `img[src*='home-bg.jpg']`; scroll as in section 3. Visible when
  the narration ends: the picture under the line is smaller than in `image-4`. The width is a resize request in the
  address of the image (`ImageShortcodeProvider.cs`, `mediaCommands.Width`): confirm that the source of the image
  holds `width=480`. If the image is as large as before, the width was not accepted (see `OPEN-QUESTIONS.md`).

After the clip, take snapshot S2.

## 6. Clip `04-list`

Starts from S2. Before `StartCaptureAsync`: open the editor as in section 3 (not recorded). The capture starts on the
editor with the options in place.

- `list-1` (Available Shortcodes). Actions: click the line `Address of the photo:` and press End; type one space
  (`Keyboard.TypeAsync(" ")`); click `button[title='Insert Shortcode']`; wait until `#shortcodeModal` is shown (it
  fades in; `OrchardCore.Shortcodes/Views/ShortcodeModal.cshtml:38`). The dialog focuses its Filter box by itself
  (`Assets/js/shortcodes.js:99-101`). `HighlightAsync("#shortcodeModal .nav-pills", "Categories")`. Visible when the
  narration ends: the dialog titled `Available Shortcodes` (`ShortcodeModal.cshtml:42`) with the box `Filter`
  (`:49`), the pills `All`, `HTML Content` and `Media` (`:53`, and the categories declared in
  `OrchardCore.Media/Startup.cs:411,429`), and two cards, `image` and `asset_url`, each with a hint, a usage note
  and an **Add** button; the categories ringed.
- `list-2` (filter, then Add). Actions: `ClearHighlightsAsync()`; `scene.TypeAsync(Locator("#shortcodeModal
  input[type='search']"), "asset", delay: 90)`; wait until only the `asset_url` card is visible (the other cards are
  hidden with a `display: none` style; the filter matches the start of the name, `shortcodes.js:75-80`); click the
  **Add** button of that card (`Locator("#shortcodeModal button:visible").Filter(new() { HasText = "Add" })`: the
  other cards are only hidden with a style, so their buttons are still in the page and the locator must say
  `:visible`; `ShortcodeDescriptor-Button-Actions.SummaryAdmin.cshtml:2`); wait until `#shortcodeModal` is hidden and the
  editor text contains `[asset_url] [/asset_url]`. Visible when the narration ends: the dialog closed, and the third
  line reading `Address of the photo: [asset_url] [/asset_url]`.
- `list-3` (the file name between the tags). Actions: click the line `Address of the photo` and press End; press
  `ArrowLeft` 12 times (the length of `[/asset_url]`), which puts the caret between the two tags, after the space;
  press `Backspace` once to delete that space; type `post-bg.jpg` with a 60 ms delay. Visible when the narration
  ends: the third line reading `Address of the photo: [asset_url]post-bg.jpg[/asset_url]`.
- `list-4` (publish and look). Actions: click **Publish**; wait for the Article list;
  `NavigateAsync("/spring-garden-tour")`; scroll until the second line with its smaller picture and the third line
  are in view. Visible when the narration ends: under the picture, a third line that reads `Address of the photo:`
  followed by the address of the file as plain text (expected: `/media/post-bg.jpg`), with no picture.

After the clip, take snapshot S3. Do not publish the final article anywhere else.

## 7. Slides

| Step | The slide shows |
|---|---|
| `why-1` | Heading "Text that depends on the site", three cards: you type a shortcode, the site replaces it, the editor keeps it |
| `why-2` | Heading "How a shortcode is written", four cards: a name, options, content, and an example with the image shortcode |
| `why-3` | Heading "The built-in shortcodes", three cards: image, asset_url, locale, with the module each one needs |
| `which-1` | Heading "Options of the image shortcode", four cards: alt and class, width and height, mode, format and quality |
| `where-1` | Heading "Where shortcodes work", the first two cards bright: HTML body and fields, Markdown body and fields |
| `where-2` | The same heading, the third card bright: the result shows on the displayed page, not in the editor |

## 8. Checks for private data

- The only people are the invented ones: user `admin` with `admin@example.com`, the article `Spring Garden Tour`,
  the subtitle `A walk with the volunteers`, the alt text `Spring tulips`. No real name, email address, phone number
  or token may appear in any frame: look at each contact sheet, then OCR the video as the skill says.
- The password never appears: it is typed outside the capture (`Recorder setup`, and Auto Setup), and the account menu
  must not be open in any frame.
- `state.json`, the `App_Data` snapshots and any mask script stay outside git.
- The three pictures are the ones the Blog recipe ships with Orchard Core; the video shows no other media.
