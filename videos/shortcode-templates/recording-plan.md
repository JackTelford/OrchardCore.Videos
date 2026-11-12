# Recording plan: Shortcode Templates

This plan is for the person who records the video. Nothing in it needs a decision. In every entry, a selector stands for
`scene.Page.Locator(...)`. The sources are files of `OrchardCMS/OrchardCore` at the commit the catalog was read from
(`src/OrchardCore.Modules/...`). Selectors and behaviors marked "to confirm" are listed in `OPEN-QUESTIONS.md`: probe them
on the running site first (`Recorder probe <path> "<js>"`), and correct the recorder scene, not the narration.

The video has three recordings (`02-enable`, `03-create`, `04-use`) and four slide clips (`01-idea`, `03-code`, `05-more`,
`06-deploy`). The slides are drawn by `slides.py`; nothing is recorded for them.

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an Orchard Core checkout, run on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | Recipe **Blog**, tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, the password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. This video uses no external service and no local stand-in. |
| Sign-in | Once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`. It writes `state.json` (git-ignored). |
| Mask script | None. The site is new and holds fake data only. |

What the Blog recipe gives this video (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`):

- The **Article** content type, whose Html Body part uses the **Wysiwyg** editor (the Trumbowyg toolbar, with the
  Insert Shortcode button). The recipe creates one Article titled **About**, with the path `about`, and lorem ipsum text
  in its body. The video edits this item.
- The features Media, Liquid, Html, Templates and **Shortcode Templates** are enabled. Media registers the `image` and
  `asset_url` shortcodes in the category **HTML Content**, which the video selects.
- No shortcode template exists.

### Seed data, outside the capture

The video needs no records of its own. It needs one change of state, made once, before the first take:

1. Sign in as `admin`, open **Tools, Features** (`/Admin/Features`), type `shortcode` in the search box.
2. On the row of **Shortcode Templates** (`OrchardCore.Shortcodes.Templates`), select **Disable**, then **Yes** in the
   confirmation. Liquid and Shortcodes stay enabled.
3. Check that **Design** no longer has a **Shortcodes** entry and that `/Admin/Shortcodes` answers 404.

This is snapshot A below. Nothing else is created by hand: the template, the shortcode in the article and the published
page are all made by the clips.

## 2. The order of the clips, and the snapshots

The recorded clips depend on each other and must be recorded in the order they play in:

| Take | Clip | Needs the site in state | Changes the site | Snapshot to keep afterwards |
|---|---|---|---|---|
| 1 | `02-enable` | **A**: new Blog site, Shortcode Templates disabled | Enables Shortcode Templates | **B** |
| 2 | `03-create` | **B**: feature enabled, no template | Adds the template `primary` | **C** |
| 3 | `04-use` | **C**: the template `primary` exists, the About article is unchanged | Adds a paragraph to the body of About, and publishes it | **D** (not needed by another clip) |

To keep a snapshot: stop the site (by its process ID), copy the `ORCHARD_APP_DATA` folder and `state.json` aside, start the
site again. To record a clip again, stop the site, restore the snapshot the clip needs (A for `02-enable`, B for
`03-create`, C for `04-use`), and start the site. A retake of `04-use` on D would show a second inserted paragraph, so
always restore C first.

The slide clips change nothing and can be assembled in any order.

## 3. Slides

One line each: what `slides.py` draws while the step plays.

- `idea-1`: two code panels, "In the body" with the two forms of the shortcode (one with a `text` argument, one with inner
  content) and "On the page" with the HTML both produce, and cards "Name", "Argument", "Content".
- `idea-2`: three cards, "In code", "As a template", "One list for editors".
- `idea-3`: three cards for the values a template receives: Args, Content, Context.
- `template-1`: the `primary` template in a code panel named Content, the lines that choose between the `text` argument
  and the inner content bright, the others dimmed, and cards "Args.Text", "Content", "!= nil".
- `template-2`: the same panel, the lines that capture the markup and write it (`capture`, `endcapture`, and the last line
  with `sanitize_html` and `raw`) bright, and cards `{{ }}` (encodes its output), `sanitize_html` and `raw`.
- `more-1`: the `display_text` template, a body that uses it, and cards "Context.ContentItem", "DisplayText", "Set by".
- `more-2`: two cards, "Same name, same shortcode" and "Sanitize Html".
- `more-3`: two code panels, the `shortcode` filter in Liquid and the `ShortcodesToHtmlAsync` helper in Razor.
- `deploy-1`: the `ShortcodeTemplates` recipe step in a code panel, and cards "Recipe step", "Keyed by name",
  "Deployment step".
- `deploy-2`: three cards: the permission "Manage shortcode templates", "Security critical", "Default roles".

## 4. Clip `02-enable`

**Starts on:** `scene.OpenAsync("/Admin/Features")`, signed in, on snapshot A. Nothing typed. The page shows the full list of
features grouped by category.

**Chapter card:** the recorder shows it; nothing to do.

### `enable-1`

Actions, in order:

1. `TypeAsync("#search-box", "shortcode")`. The list filters while you type
   (`OrchardCore.Features/Views/Admin/Features.cshtml:27`, the search box). Wait until the row whose text contains
   `OrchardCore.Shortcodes.Templates` is visible (the id is written after the name,
   `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`).
2. `HighlightAsync` on the dependency badges of that row (`.direct-dependencies`,
   `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`), caption `Dependencies`. They read **Liquid** and **Shortcodes**.

At the end of the narration the screen must show: the search box with `shortcode`, the row **Shortcode Templates** with the
badges Liquid and Shortcodes ringed, and an **Enable** button on that row. The row of **Shortcodes** may also be listed.

Labels: Tools, Features `OrchardCore.Features/AdminMenu.cs:26-27`; Search `Features.cshtml:27`; Enable
`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.

### `enable-2`

Actions, in order:

1. `ClearHighlightsAsync()`.
2. `ClickAndWaitForNavigationAsync` on the **Enable** link of the Shortcode Templates row (the `<a>` with the text
   `Enable`, id starting `btn-enable-`, `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). The shell restarts: wait until the
   page is back and the text `was enabled` is visible (`AdminController.cs:317` of the Features module: "The feature
   Shortcode Templates was enabled.").
3. Click **Design** in the admin menu if it is collapsed, then `HighlightAsync` on its **Shortcodes** entry
   (`OrchardCore.Shortcodes/AdminMenu.cs:18-19`).

At the end of the narration the screen must show: the notification "The feature Shortcode Templates was enabled.", and the
admin menu with **Design** open and the **Shortcodes** entry ringed. If the actions run longer than the narration, shorten
the pauses; do not remove the ring.

## 5. Clip `03-code` (slides) and `03-create`

`03-code` is slides: see section 3 (`template-1`, `template-2`).

**`03-create` starts on:** `scene.OpenAsync("/Admin")` (the dashboard), signed in, on snapshot B. Before
`StartCaptureAsync`, expand **Design** in the admin menu. Then start the capture.

The Create page stacks every field at full width, in this order: **Name**, **Hint**, **Usage**, **Categories**,
**Default Shortcode Value**, a preview card, then **Content** (`OrchardCore.Shortcodes/Views/Admin/Create.cshtml:47-107`).
**Usage** and **Content** are CodeMirror editors made from the textareas `#Usage` and `#Content` (the editor is the
`.CodeMirror` element right after the textarea). They are not inputs, so do not type into the textareas; set their value
with the editor's own method:

```
scene.Page.EvaluateAsync("(text) => document.querySelector('#Usage + .CodeMirror').CodeMirror.setValue(text)", usage)
```

The same call, with `#Content`, fills Content. The editor fires its change event, which updates the preview card. **Name**
and **Hint** are ordinary inputs: type them with `TypeAsync`. If the page is taller than the viewport, scroll with
`ScrollAsync(0, 450)` (start with that value) to bring the next field into view, and clear highlights before scrolling.

### `create-1`

Actions: `ClickAndWaitForNavigationAsync` on **Shortcodes** in the open **Design** menu
(`OrchardCore.Shortcodes/AdminMenu.cs:19`), pause 600 ms on the empty list, then `ClickAndWaitForNavigationAsync` on the
button **Add Shortcode Template** (`a.create`, `Views/Admin/Index.cshtml:25`).

At the end of the narration the screen must show: the page **Create Shortcode** (`Create.cshtml:41`, the breadcrumb) with
the empty form, **Name** on top. The empty list ("Nothing here! There are no shortcode templates for the moment.",
`Index.cshtml:34`) is seen during the step, before the click.

### `create-2`

Actions: `TypeAsync("#Name", "primary")`, then `PauseAsync(600)`.

At the end of the narration the screen must show: **Name** holding `primary`, and the preview card (hidden until you type,
`Create.cshtml:91`) showing the title `[ primary ]`. If the card is below the fold, that is accepted for this step.

Source of the checks the narration lists: `OrchardCore.Shortcodes/Controllers/AdminController.cs:154-166`
("The name is mandatory.", "The name contains invalid characters.", "A template with the same name already exists.").

### `create-3`

Actions, in order:

1. `TypeAsync("#Hint", "Formats text in the primary color of the theme.")`.
2. `EvaluateAsync` as above on `#Usage`, with this text (two lines):

```
[primary text="Read this first."]
[primary]Read this first.[/primary]
```

3. `ScrollAsync(0, 450)` so that the preview card is in view.

At the end of the narration the screen must show: the preview card with the title `[ primary ]`, the hint, and the two
usage lines, as the picker will show them. Labels: Hint `Create.cshtml:58`, Usage `Create.cshtml:66`; the preview is
updated by `Assets/js/shortcode-templates.js` (name and hint on key up, usage on the editor change).

### `create-4`

Actions, in order:

1. Click inside the **Categories** control (`#shortcodeCategories .multiselect`, to confirm), then click the option
   `HTML Content` in the list that opens. If the option is not listed, type `HTML Content` and press Enter (the control
   says "Press enter to add a category", `Services/ShortcodesJSLocalizer.cs`).
2. `TypeAsync("#DefaultValue", "[primary]Read this first.[/primary]")`.

At the end of the narration the screen must show: the chip **HTML Content** in Categories and the **Default Shortcode
Value** field holding `[primary]Read this first.[/primary]`. Labels: Categories `Create.cshtml:75`, Default Shortcode Value
`Create.cshtml:83`. The chip must read exactly `HTML Content` (the category Media registers, `OrchardCore.Media/Startup.cs:411`).

### `create-5`

Actions, in order:

1. `ScrollAsync(0, 450)` until the **Content** editor is in view.
2. `EvaluateAsync` on `#Content` with exactly this text (eight lines, no indentation):

```
{% capture output %}
{% if Args.Text != nil %}
<span class="text-primary">{{ Args.Text }}</span>
{% else %}
<span class="text-primary">{{ Content }}</span>
{% endif %}
{% endcapture %}
{{ output | sanitize_html | raw }}
```

At the end of the narration the screen must show: the **Content** editor with these lines and line numbers, the hint
"The Shortcode Liquid template." under it (`Create.cshtml:106`), and the **Save** button in the action bar.
This is the `primary` template of the documentation, with the filter name the code registers (`sanitize_html`,
`OrchardCore.DisplayManagement.Liquid/Extensions/LiquidCoreServices.cs:61`).

### `create-6`

Actions: `ClickAndWaitForNavigationAsync` on the button **Save** (`button.save`, `Create.cshtml:119`; not the arrow next to
it). The page returns to the list (`/Admin/Shortcodes`).

At the end of the narration the screen must show: the list **Shortcodes** with one row, the name `primary` as a link, the
hint "Formats text in the primary color of the theme." and the category **HTML Content**. No error and no
"Nothing here!" message. Sources: `Views/ShortcodeTemplateEntry.Fields.SummaryAdmin.cshtml`,
`...Description.SummaryAdmin.cshtml`, `...DefaultTags.SummaryAdmin.cshtml`.

### `create-7`

Actions, in order (nothing is saved or deleted):

1. `HighlightAsync` on the search box (`#search-box`), caption `Search`.
2. `HighlightAsync` on the **Edit** button of the row (`Views/ShortcodeTemplateEntry.Buttons.SummaryAdmin.cshtml:6`) and on
   its **Delete** button (`:8`). If the admin settings put them in a menu, open that menu instead (to confirm).
3. Tick the checkbox of the row (`input[name="itemIds"]`, `Views/ShortcodeTemplateEntry.Checkbox.SummaryAdmin.cshtml`), so
   that the bulk action appears (**Delete**, `OrchardCore.Shortcodes/Controllers/AdminController.cs:110`), `PauseAsync(800)`, then untick it.
4. `ClearHighlightsAsync()`.

At the end of the narration the screen must show: the list with the row `primary` and, ringed or visible, the search box and
the row's buttons. The template must still exist: do not select **Delete** and do not confirm anything. Snapshot C is taken
after this step.

## 6. Clip `04-use`

**Starts on:** the edit page of the article **About**, on snapshot C. Before `StartCaptureAsync`:

1. `scene.OpenAsync("/Admin/Contents/ContentItems")`; select **Edit** on the row **About**
   (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`) and wait for the edit page (**Body** editor
   with the lorem ipsum paragraphs; `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml`).
2. Click in the last paragraph of the body, press `Control+End`, then `Enter`, so the caret is in a new empty paragraph
   (to confirm: the caret must be in the body when the picker opens).

### `use-1`

Actions, in order: `HighlightAsync` on the **Insert Shortcode** button of the editor toolbar
(`button.trumbowyg-insertShortcode-button`, to confirm; its title is `Insert Shortcode`,
`OrchardCore.Resources/Assets/trumbowyg/js/trumbowyg.shortcodes.js:7`), `PauseAsync(500)`, `ClearHighlightsAsync()`, then
`ClickAsync` on it. Wait until `#shortcodeModal` is shown.

At the end of the narration the screen must show: the dialog **Available Shortcodes** (`Views/ShortcodeModal.cshtml:42`) with
its **Filter** box and the list of categories on the left (All, HTML Content, Media) and the cards of the shortcodes,
including `[ primary ]`.

### `use-2`

Actions, in order:

1. `TypeAsync` in the **Filter** box of the dialog (`#shortcodeModal input[type="search"]`, `ShortcodeModal.cshtml:49`) the text
   `prim`, so that only the card of `primary` remains.
2. `HighlightAsync` on that card (`#shortcodeModal .card` that contains `primary`), then `PauseAsync(700)`,
   `ClearHighlightsAsync()`.
3. As the last action, `ClickAsync` on the button **Add** of that card
   (`Views/ShortcodeDescriptor-Button-Actions.SummaryAdmin.cshtml:2`).

At the end of the narration the screen must show: the dialog closing or closed and the text
`[primary]Read this first.[/primary]` in the new paragraph of the body. The card, before the click, shows the title
`[ primary ]`, the hint and the two usage lines (`Views/ShortcodeDescriptor.Fields.SummaryAdmin.cshtml`).

### `use-3`

Actions, in order: `HighlightAsync` on the inserted text, `PauseAsync(1000)`, `ClearHighlightsAsync()`, then
`ClickAndWaitForNavigationAsync` on the button **Publish** (`button.publish`,
`OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`: the page shows this split button because the Edit link of
the list passes a `returnUrl`). The browser returns to the Content Items list.

At the end of the narration the screen must show: the Content Items list, after the publish, with no error. (If the page
shows a notification that the item was published, it stays on screen.)

### `use-4`

Actions, in order:

1. `NavigateAsync("/about")`. Wait until `span.text-primary` is visible.
2. `ScrollAsync` to the end of the body text, and `HighlightAsync` on `span.text-primary`.

At the end of the narration the screen must show: the page **About** of the Blog theme, the last paragraph of the body, and
the words `Read this first.` in the primary color of the theme, ringed. The text `[primary]` must not appear on the page
(`OrchardCore.Html/Drivers/HtmlBodyPartDisplayDriver.cs` renders the shortcodes through `HtmlDisplayService`).

## 7. Checks for private data

Before the clips are assembled and again after:

- Every name on screen is fake: the user `admin`, the email `admin@example.com`, the site `Orchard Core`, the article
  **About** and its lorem ipsum text, and the shortcode `primary` with `Read this first.` Nothing else is typed.
- No password is shown: the sign-in is made by `Recorder setup`, outside the capture, and the capture never shows the
  sign-in page. `<password>` stays a placeholder in this repository.
- No tenant, machine, user-profile or folder name appears: the address bar is not recorded, and the pages show no file
  paths.
- No other content item is opened. The Content Items list (seen at the end of `use-3`) holds only the items of the Blog
  recipe; look at it for real names before assembling.
- OCR the assembled video at one frame per second and at every scene change, as the skill says, and search for real names,
  email addresses and tokens.
- `state.json`, the `ORCHARD_APP_DATA` folder and its snapshots stay out of git.
