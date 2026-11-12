# Recording plan: Create a Liquid Widget Without Writing a Module

For the person who records the clips. Every `file:line` is in the Orchard Core source (`src/OrchardCore.Modules/...`
unless a path says otherwise) and tells you where a label on screen comes from. A selector below stands for
`scene.Page.Locator(...)`. The selectors are read from the views, not run: check each one with `Recorder probe` before
recording, and see `OPEN-QUESTIONS.md` for what is still unconfirmed.

The video has four recorded clips (`02-type`, `03-widget`, `04-place`, `06-template`) and eight slide steps. Nothing
in it needs an external service.

## The site it needs

- A local site: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
  its own empty `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- Provisioned by Auto Setup, so no setup or login form is recorded:
  - recipe: **Blog**;
  - site name: **Orchard Core**;
  - time zone: **America/Los_Angeles**;
  - user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in
    `VIDEO_PASSWORD`, never written anywhere in the repository).
- Block outbound calls of the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, as the skill says. The
  variables reach the site process only: the layout of the Blog theme links two stylesheets from `fonts.googleapis.com`
  (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`), and the recording browser loads them itself, so the fonts on
  screen depend on that browser's network access (`OPEN-QUESTIONS.md`). Either way is fine for the video.
- Features: the Blog recipe already enables everything the video uses: Content Types, Widgets, Layers, Templates
  (which enables Liquid) and Flows (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step).
  Enable nothing else.
- Settings: none. The recipe's `LayerSettings` define the zones **Content** and **Footer**, and its `layers` step
  creates the layers **Always** and **Homepage** (same file, the `settings` and `layers` steps).
- External services: none.
- Sign in once outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which saves
  `state.json` (git-ignored).
- The video has no `PrepareAsync`: there is no seed data to create. The Blog recipe's own content is the starting
  state, and it is all fake (Lorem ipsum text, a sample blog post, a footer saying "Your Website").

### What the site must look like before the first take

Check these outside the capture, on the fresh site:

1. **Design, Content Definition, Content Types** lists the types of the Blog recipe and no **Liquid Widget**.
2. **Design, Widgets** is titled **Widgets and Layers**. Its **Footer** zone already holds one widget named **Footer**
   (a raw HTML widget with the social icons and "Copyright © Your Website 2021"), tagged **Always**. The **Content**
   zone is empty.
3. **Design, Templates** says "Nothing here! There are no templates for the moment."
4. The home page (`/`) ends with that footer.

### Text to enter

The two texts below are entered exactly as written, with four spaces for each indent and a line feed between lines.
The Body of the widget (one line):

```
<p class="text-center">{{ Site.SiteName }} is open Monday to Friday, 9:00 to 17:00.</p>
```

The template (four lines):

```
<article class="{{ Model.Classes | join: " " }} text-center">
    <h3>{{ Model.ContentItem.DisplayText }}</h3>
    {{ Model.Content.LiquidPart | shape_render }}
</article>
```

### How to enter them

Both editors can close brackets and quotes or indent a line on their own when text is typed, so do not type these
texts. Paste them, with one method for both editors: send a paste event with the text to the editor's hidden text box.

```csharp
await scene.Page.EvaluateAsync(@"([selector, text]) => {
    const box = document.querySelector(selector);
    box.focus();
    const data = new DataTransfer();
    data.setData('text/plain', text);
    box.dispatchEvent(new ClipboardEvent('paste', { clipboardData: data, bubbles: true, cancelable: true }));
}", new object[] { selector, text });
```

- The Body of the widget (CodeMirror): click the editor first (`.ocat-wrapper:has(label:text-is("Body")) .CodeMirror`),
  then paste with the selector `.ocat-wrapper:has(label:text-is("Body")) .CodeMirror textarea`.
- The Content of the template (Monaco): click the editor first (`.template-editor`), then paste with the selector
  `.template-editor textarea.inputarea`.

Read the text back before going on, and stop if it differs from the text above:

- Body: `document.querySelector('.CodeMirror').CodeMirror.getValue()`.
- Template: `document.querySelector('.template-editor-value').value`.

If the read-back differs, grant the browser context the `clipboard-read` and `clipboard-write` permissions, write the
text with `navigator.clipboard.writeText(text)`, click the editor, and press `Control+V`; read it back again. If that
fails too, stop and write what you saw in `OPEN-QUESTIONS.md`: the narration and the slides assume this exact text.

## The order of the clips

Record in the order they play: `02-type`, `03-widget`, `04-place`, `06-template`. Each one starts from the site the
previous one left, and each one changes the site:

| Clip | What it changes |
|---|---|
| `02-type` | Creates the **Liquid Widget** content type, with the Liquid part |
| `03-widget` | Clears **Creatable** and **Listable** on it, and sets its **Stereotype** to `Widget` |
| `04-place` | Publishes a widget titled **Opening hours** in the **Footer** zone of the **Always** layer |
| `06-template` | Creates the template `Widget__LiquidWidget` |

Snapshots: stop the site, copy its `App_Data` folder and `state.json` aside after Auto Setup (`S0`), and again after
each clip (`S02`, `S03`, `S04`). To record a clip again, stop the site, restore the snapshot of the clip before it
(`S0` for `02-type`, `S02` for `03-widget`, `S03` for `04-place`, `S04` for `06-template`), start the site, and record.
Do not record `06-template` on a site that already has the template.

Every clip starts on a page opened before the capture (`scene.OpenAsync`), as written below.

## Clip `02-type`: the content type

Starts on `/Admin` (the dashboard). Before `StartCaptureAsync`, expand the **Design** group and then **Content
Definition** in the admin menu, so the clicks of `type-1` stay within the narration:

- **Design**: `button.nav-group-label:has-text("Design")`
- **Content Definition**: `button.item-label:has-text("Content Definition")`

The menu items come from `OrchardCore.ContentTypes/AdminMenu.cs:23-25`.

- `type-1`: Click the **Content Types** link (`a.item-label:text-is("Content Types")`) with
  `ClickAndWaitForNavigationAsync`, then the **Create new type** button (`a#btnCreate`) the same way. Visible at the
  end: the page **New Content Type** with an empty **Display Name** and **Technical Name**. Labels:
  `OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`, `Views/Admin/Create.cshtml:5-30`.
- `type-2`: Type `Liquid Widget` into **Display Name** (`input#DisplayName`) with `TypeAsync`, 60 ms per character.
  Press `Tab`. Read the **Technical Name** (`input#Name`): it must read `LiquidWidget`. If it does not, click it, select
  all, and type `LiquidWidget`. `PauseAsync(3000)`, then click **Create** (`button.create[type="submit"]`) with
  `ClickAndWaitForNavigationAsync`. Visible at the end: the **Add Parts** page, with the message "The "Liquid Widget"
  content type has been created." Labels: `Views/Admin/Create.cshtml:14,22,30`; the message,
  `Controllers/AdminController.cs:162`.
- `type-3`: Highlight the **Liquid** row (`li.list-group-item:has(label:text-is("Liquid"))`, caption `Liquid`), and click
  its check box (`li.list-group-item:has(label:text-is("Liquid")) input[type="checkbox"]`). The button **Add Selected
  Parts** (`#add-selected-parts-button`) appears. Clear the highlight, `PauseAsync(2500)`, and click the button with
  `ClickAndWaitForNavigationAsync`. Visible at the end: the **Edit Content Type - Liquid Widget** page. The row says
  "Provides a Liquid formatted body for your content item." (`OrchardCore.Liquid/Migrations.cs:20`); the label is the
  part name without "Part". Labels: `Views/Admin/AddPartsTo.cshtml:10,47`;
  `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`.
- `type-4`: Scroll the **Parts** heading (`h3:text-is("Parts")`) into view with `ScrollAsync`, then highlight the list
  (`ul#parts`, caption `Parts`). Visible at the end: the list with the part that is named after the type, which reads
  **Liquid Widget** (the display name of the type, not the technical name; hint "Contains the fields for the current
  type"), and **Liquid** (with its description), and the highlight ring around both. Labels:
  `Views/Admin/Edit.cshtml:144,153,180`; the name of the part that belongs to the type comes from
  `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs:7-25`.

## Clip `03-widget`: the Widget stereotype

Starts on `/Admin/ContentTypes/Edit/LiquidWidget`, scrolled so that the check boxes **Creatable** and **Listable** are
in view.

- `widget-1`: Highlight the **Creatable** check box (`.form-check:has(label:text-is("Creatable"))`, caption `Creatable`),
  click its label (`label:text-is("Creatable")`), clear the highlight, then do the same for **Listable**. Visible at
  the end: both boxes cleared; **Draftable**, **Versionable** and **Securable** still checked. Labels:
  `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:17,30`. The defaults are in
  `OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:84-91`.
- `widget-2`: Click the **Stereotype** field (`.ocat-limited-wrapper:has(label:text-is("Stereotype")) input`) and type
  `Widget`, 80 ms per character. `PauseAsync(1500)`, then click **Save**
  (`button[name="submit.Save"][value="Save"]`) with `ClickAndWaitForNavigationAsync`. Visible at the end: the Content
  Types list, with the message "Content type updated successfully." and the **Liquid Widget** row. Labels:
  `Views/ContentTypeSettings.Edit.cshtml:77`, `Views/Admin/Edit.cshtml:215`; the message,
  `Controllers/AdminController.cs:225`; the offered stereotype, `OrchardCore.Widgets/Services/WidgetStereotypesProvider.cs:18`.

## Clip `04-place`: the layer

Starts on `/Admin`. Before `StartCaptureAsync`, expand the **Design** group (as above).

- `place-1`: Click the **Widgets** link (`a.item-label:text-is("Widgets")`) with `ClickAndWaitForNavigationAsync`: the page
  is titled **Widgets and Layers**, with a card for each zone. In the **Footer** card, click
  **Add Widget** (`div.card:has(> .card-header:text-is("Footer")) button.dropdown-toggle`), then **Liquid Widget**
  (`a.add-list-widget[data-zone="Footer"][data-widget-type="LiquidWidget"]`) with `ClickAndWaitForNavigationAsync`. Every zone
  card has its own **Add Widget** menu with its own **Liquid Widget** entry (`Views/Admin/Index.cshtml:85,96`), so always
  scope the locators to the **Footer** card or to `data-zone="Footer"`.
  Visible at the end: the page **New Liquid Widget**, with the empty fields **Title**, **Render title**, **The layer**
  and **Body**. Labels: `OrchardCore.Layers/AdminMenu.cs:26-27`, `Views/Admin/Index.cshtml:20,34,85`; the list holds
  every type with the stereotype Widget (`OrchardCore/OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:128-133`).
- `place-2`: Type `Opening hours` into **Title** (`.ocat-wrapper:has(label:text-is("Title")) input`), 60 ms per character.
  `SelectAsync` on **The layer** (`.ocat-wrapper:has(label:text-is("The layer")) select`) with the value `Always`.
  Highlight the **Render title** check box (`.form-check:has(label:text-is("Render title"))`, caption `Cleared`). Visible
  at the end: the title, the layer **Always**, and the cleared box with its ring. Labels:
  `OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:4,15,22`; the box is cleared by default
  (`OrchardCore.Layers/Models/LayerMetadata.cs:7`, a `bool`).
- `place-3`: Clear the highlight. Click the **Body** editor, paste the Body text (see "How to enter them"), read it back.
  `PauseAsync(1500)`, then click **Publish** (`button.publish[name="submit.Publish"]`) with
  `ClickAndWaitForNavigationAsync`. Visible at the end: the page **Widgets and Layers** again (the form was opened with a
  return address). Labels: `OrchardCore.Liquid/Views/LiquidPart.Edit.cshtml:14`,
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`; only published widgets are shown,
  `OrchardCore.Layers/Services/LayerFilter.cs:88`.
- `place-4`: Highlight the new row in the **Footer** card (`div.card:has(> .card-header:text-is("Footer")) li.list-group-item:has-text("Opening hours")`,
  caption `Opening hours`). Visible at the end: the **Footer** card with two rows, **Footer** and **Opening hours**, each
  tagged **Always**. Source: `OrchardCore.Layers/Views/Admin/Index.cshtml:57`.
- `place-5`: `ClearHighlightsAsync`, then `NavigateAsync("/")`. Scroll to the bottom with `ScrollAsync(0, 1000)` repeated,
  `PauseAsync(400)` between the scrolls, until `footer` is fully in view. Highlight the paragraph
  (`footer p.text-center:has-text("is open Monday")`, caption `Liquid widget`). Visible at the end: the footer, with the
  social icons and the copyright line of the first widget, and under them the centered paragraph "Orchard Core is open
  Monday to Friday, 9:00 to 17:00." and no heading. Source: `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:44-46`
  (the Footer zone).

## Clip `06-template`: the template

Starts on `/Admin`. Before `StartCaptureAsync`, expand the **Design** group (as above).

- `template-1`: Click the **Templates** link (`a.item-label:text-is("Templates")`) with `ClickAndWaitForNavigationAsync`, then
  **Add Template** (`a.create:text-is("Add Template")`) the same way. On the **Create Template** page, type
  `Widget__LiquidWidget` into **Name** (`input#Name`), 30 ms per character. Visible at the end: the page **Create
  Template** with the name in the field. Labels: `OrchardCore.Templates/AdminMenu.cs:19-20`,
  `Views/TemplateCreateButton.cshtml:6`, `Views/Template/Create.cshtml:8,18`.
- `template-2`: Click the **Content** editor (`.template-editor`), paste the template text (see "How to enter them"), read it
  back. `PauseAsync(1500)`, then click **Save** (`button.save[type="submit"][value="Save"]`) with
  `ClickAndWaitForNavigationAsync`. Visible at the end: the **Templates** list with the row `Widget__LiquidWidget` and the
  message "The "Widget__LiquidWidget" template has been created." Labels: `Views/Template/Create.cshtml:35,53`;
  the message, `Controllers/TemplateController.cs:204`.
- `template-3`: `NavigateAsync("/")`, scroll to the footer as in `place-5`. Highlight the heading
  (`footer article.widget-liquid-widget h3`, caption `From the template`). Visible at the end: the footer, with the
  heading **Opening hours** centered above the paragraph. The class `widget-liquid-widget` comes from
  `OrchardCore.Layers/Services/LayerFilter.cs:140` and `OrchardCore/OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:15-33,77-113`
  (the type name `LiquidWidget` becomes `liquid-widget`: `CamelFriendly` inserts a space before each capital letter, `HtmlClassify`
  lowers the letters and joins the words with a hyphen).
- `template-4`: `ClearHighlightsAsync`, `NavigateAsync("/Admin/ContentTypes/Edit/LiquidWidget")`. Click **Edit templates**
  (`button.dropdown-toggle:text-is("Edit templates")`). Highlight the open menu (`.dropdown-menu.show`, caption `Edit templates`).
  Visible at the end: the menu with the two entries "Template for a Liquid Widget content item in detail views" and
  "Template for a Liquid Widget content item in summary views". Do not click either entry. Labels:
  `OrchardCore.Templates/Views/TemplateSettings.cshtml:5`,
  `OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:50,57`.

## The slides

Drawn by `slides.py`, no recording. One line each:

- `idea-1`: three cards, "A content type", "The Widget stereotype" and "A template"; the first two are bright.
- `idea-2`: two cards, "Stored exactly as typed" and "Guarded by a permission".
- `shape-1`: three cards, the alternates "Stereotype", "Widget" and "Widget__LiquidWidget".
- `shape-2`: the code of `Widget.cshtml` (`OrchardCore.Widgets/Views/Widget.cshtml`), trimmed, with two notes.
- `shape-3`: the code of `Widget.Wrapper.cshtml` (`OrchardCore.Layers/Views/Widget.Wrapper.cshtml`), trimmed, with two notes.
- `shape-4`: the template of the video, with a note for each of `Model.Classes`, `Model.ContentItem` and `Model.Content`.
- `more-1`: two cards, "In a layer" and "In a Flow part", with the classes each one adds
  (`OrchardCore.Layers/Services/LayerFilter.cs:139-140`, `OrchardCore.Flows/Views/FlowPart.cshtml:36-39`).
- `more-2`: the same override as two theme files, `Widget-LiquidWidget.liquid` and `Widget-LiquidWidget.cshtml`.

## Checks for private data

- Everything on screen is fake: the Blog recipe's sample content, `admin`, the site name **Orchard Core**, the title
  **Opening hours**, and the text of the Body. No clip shows the sign-in form, the password, the user menu with an
  email address, or a path of this machine.
- OCR the video at one frame per second and at every scene change, as the skill says, and search for the password, for
  real names and for email addresses other than `admin@example.com`.
- `state.json`, the `App_Data` snapshots and any script that holds the password stay outside git.
