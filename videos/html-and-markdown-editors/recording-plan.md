# Recording plan: Html and Markdown: Write the Body of Your Content

Read this file and `storyboard.json`; nothing else is needed to record the video. Seven slide steps are drawn by
`slides.py` and need no recording. Seventeen steps are recordings, in four clips: `02-add`, `03-html`, `04-standard`
and `05-markdown`. In this file a selector stands for `scene.Page.Locator(...)`, and "Orchard Core source" means the
`OrchardCMS/OrchardCore` checkout the site is built from (paths are given from its `src/OrchardCore.Modules` folder, except `OrchardCore.Html.Core` and
`OrchardCore.ContentManagement.Abstractions`, which are in `src/OrchardCore`, and `OrchardCore.Themes`, which is in `src`).

The Trumbowyg and EasyMDE editors are third-party libraries that are not in that source folder. Every selector for
a toolbar button or an editing area below is therefore a best expectation: confirm it with `Recorder probe` before
the first take, and write what you find in OPEN-QUESTIONS.md (items 1 to 4).

## The site it needs

| | |
|---|---|
| Host | `src/OrchardCore.Cms.Web` of an `OrchardCore` checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data | An empty `ORCHARD_APP_DATA` folder outside both repositories |
| Setup | **Auto Setup**, tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe **Blog**, user `admin`, email `admin@example.com`, database SQLite |
| Password | `<password>`: chosen outside git, set in the Auto Setup `AdminPassword` value and passed to the recorder in `VIDEO_PASSWORD` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls out |
| Browser | The recorder's own Chromium, 1536x864, light scheme, `en-US`. Sign in once with `Recorder setup` (writes `state.json`, which is git-ignored) |

The Auto Setup values are the `OrchardCore:AutoSetup:Tenants:0:*` configuration keys (`ShellName`, `SiteName`,
`SiteTimeZone`, `AdminUsername`, `AdminEmail`, `AdminPassword`, `DatabaseProvider`, `RecipeName`); as environment
variables they are `OrchardCore__AutoSetup__Tenants__0__ShellName` and so on
(`src/docs/reference/modules/AutoSetup/README.md`).

### Features

The **Blog** recipe enables everything the video needs, so nothing is enabled before recording: Html
(`OrchardCore.Html`), Markdown (`OrchardCore.Markdown`), Media, Shortcodes (a dependency of both), Contents, Content
Types, Title, Autoroute, Content Preview, Features, Deployment, Recipes and Placements
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, `feature` step). The recipe also gives:

- `Article`, with Title, Autoroute and **HtmlBody** (editor Wysiwyg);
- `Blog`, with **HtmlBody** (the Standard editor) and one item titled **Blog**, whose body reads "This is the description of your blog";
- `Blog Post`, with **MarkdownBody** (editor Wysiwyg);
- `Page`, which has no body part.

### Seed data, created outside the capture

Two content types are added by hand before the first take. Open **Tools, Deployments, JSON Import**
(`OrchardCore.Deployment/AdminMenu.cs:18-30`), paste the JSON below into the field labeled **JSON**
(`OrchardCore.Deployment/Views/Import/Json.cshtml:15`), and click **Import** (`:21`). The notice "Recipe imported
successfully!" (`OrchardCore.Deployment/Controllers/ImportController.cs:185`) confirms it. The types are
`Announcement` (Title and Autoroute, **no body part**) and `Help Topic` (Title, Autoroute and
MarkdownBody with the Wysiwyg editor). Both are creatable, listable, draftable and versionable, and their route is the
slug of the title. The JSON copies the shape of the `ContentDefinition` step of the Blog recipe.

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Announcement",
          "DisplayName": "Announcement",
          "Hidden": false,
          "Settings": {
            "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "Announcement", "Name": "Announcement", "Settings": { "ContentTypePartSettings": { "Position": "3" } } },
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "{{ Model.ContentItem | display_text | slugify }}",
                  "ShowHomepageOption": false
                },
                "ContentTypePartSettings": { "Position": "1" }
              }
            }
          ]
        },
        {
          "Name": "HelpTopic",
          "DisplayName": "Help Topic",
          "Hidden": false,
          "Settings": {
            "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "HelpTopic", "Name": "HelpTopic", "Settings": { "ContentTypePartSettings": { "Position": "3" } } },
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "{{ Model.ContentItem | display_text | slugify }}",
                  "ShowHomepageOption": false
                },
                "ContentTypePartSettings": { "Position": "1" }
              }
            },
            {
              "PartName": "MarkdownBodyPart",
              "Name": "MarkdownBodyPart",
              "Settings": { "ContentTypePartSettings": { "Editor": "Wysiwyg", "Position": "2" } }
            }
          ]
        }
      ],
      "ContentParts": [
        { "Name": "Announcement", "Settings": {}, "ContentPartFieldDefinitionRecords": [] },
        { "Name": "HelpTopic", "Settings": {}, "ContentPartFieldDefinitionRecords": [] }
      ]
    }
  ]
}
```

After the import, check three things on screen, outside the capture:

1. **Design, Content Definition, Content Types** lists **Announcement** and **Help Topic** (columns "Display name" and "Technical name").
2. Editing **Announcement** shows the parts **Title** and **Autoroute** and the type's own entry, and **no** HtmlBody or MarkdownBody.
3. Editing **Help Topic** shows **MarkdownBody** with a badge reading **Wysiwyg**.

No content items are seeded. The items the clips create are fake: "Garden Tour This Saturday" and "Borrow a Library Book".

### Snapshots

- **S0**: stop the site after the seed is imported and the three checks pass. Copy `App_Data` and `state.json` aside.
- **S1**: record `02-add`, stop the site, and copy `App_Data` and `state.json` aside again.

## Order of the clips, and what changes the site

| Order | Clip | Starts from | Changes the site | Before a new take |
|---|---|---|---|---|
| 1 | `02-add` | S0 | Adds HtmlBody to Announcement, with the Wysiwyg editor | Restore S0 |
| 2 | `03-html` | S1 (needs `02-add`) | Creates and publishes the item "Garden Tour This Saturday" | Restore S1 |
| 3 | `04-standard` | any | Nothing: it ends on Cancel | Nothing |
| 4 | `05-markdown` | S0 or S1 | Creates and publishes the item "Borrow a Library Book" | Restore the snapshot it started from |

`03-html` needs `02-add`, because the Announcement editor only exists after HtmlBody is attached. The other clips do
not depend on each other. Narrate before recording: each step is held for the length of its narration.

Timing: at about 150 words a minute a 20-word step is held for about 8 seconds, so keep the typing, clicks and waits of
a step shorter than that, and use the shortest typing delay (25 ms).

## Clip `02-add`

**Starts on** `/Admin` (the admin home page: the Blog recipe does not enable the Admin Dashboard feature), signed in as `admin`. The menu entries **Tools**, **Design**
and **Content Definition** are collapsed, because opening them is part of the story. Capture starts there.

`add-1` (Both modules are features.)
- Click the menu entry **Tools**, then **Features**, and wait for the Features page. Click the search field
  (`#search-box`, placeholder "Search"), type `html` (25 ms delay), wait 1.2 s, select all (`Control+A`), type
  `markdown`, wait 1.2 s.
- On screen when the narration ends: the Features page with the search text `markdown`, and the feature **Markdown**
  in the list with the badge **Enabled**. (**Html** was on screen 1.2 s earlier, also **Enabled**.)
- Labels: Tools, `OrchardCore.Features/AdminMenu.cs:26`; Features, `AdminMenu.cs:27`; Search,
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`; Enabled,
  `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`; Enable,
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. The feature names Html and Markdown are
  the `Name` of the module manifests (`OrchardCore.Html/Manifest.cs:4`, `OrchardCore.Markdown/Manifest.cs:4`).
- The search matches the name, the id and the description of a feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`),
  so `html` can list other features besides **Html**. The narration needs only the row **Html** (then **Markdown**) to be
  visible with its **Enabled** badge.
- The narration says that when a feature is off you select Enable beside it (the button is shown only for a disabled feature,
  `FeatureEntry.Buttons.SummaryAdmin.cshtml:38-44`). Both features are on, so do not click anything for that.

`add-2` (A body part belongs to a content type.)
- Click **Design**, then **Content Definition**, then **Content Types** (a navigation: use
  `ClickAndWaitForNavigationAsync` for the last click). On the list of content types, click **Edit** in the row of
  **Announcement** (`a[role="btn-edit-Announcement"]`, with `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the page "Edit Content Type - Announcement", with the **Parts** list showing
  **Title**, **Autoroute** and **Announcement** (the type's own entry), and the **Add Parts** button below the list.
- Labels: Design, Content Definition, Content Types, `OrchardCore.ContentTypes/AdminMenu.cs:23-25`; Edit,
  `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the page title and the Parts
  heading, `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63` and `:144` (the `<h3>`); Add Parts,
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201`.
- The step has three menu clicks, one navigation and a second navigation; if it runs longer than the narration
  (about 11 s), shorten the pauses, not the clicks.

`add-3` (Add Parts lists every part you can attach.)
- Click **Add Parts** (`ClickAndWaitForNavigationAsync`). On the page "Add Parts", scroll the list so that the row
  **HtmlBody** is in view, and highlight it (`HighlightAsync`, no caption, 0.8 s; the row is
  `scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "HtmlBody" })`, `AddPartsTo.cshtml:29`). Do the same for the row
  **MarkdownBody** (the same locator with `HasText = "MarkdownBody"`; the rows are in alphabetical order, so the two rows are not next to each other; clear the highlight
  before scrolling). Scroll back to the row **HtmlBody**, then click its **Add & Configure** button
  (`button[name="submit.AddAndConfigure"][value="HtmlBodyPart"]`, `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the page "Edit Part - HtmlBody" has loaded, with a green notice "The "HtmlBody"
  part has been added." above it. Before that click, the row HtmlBody showed the description "Provides an HTML Body for
  your content item." and the row MarkdownBody showed "Provides a Markdown formatted body for your content item.".
- Labels: Add Parts (title and instruction "Choose the Parts to add to this Content Type."),
  `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:10` and `:18`; Add & Configure, `AddPartsTo.cshtml:36`;
  the part names HtmlBody and MarkdownBody are the part names without the final "Part"
  (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`); the
  descriptions are `OrchardCore.Html/Migrations.cs:34` and `OrchardCore.Markdown/Migrations.cs:27`; the notice,
  `OrchardCore.ContentTypes/Controllers/AdminController.cs:355`.
- The narration is 23 words (about 9 s): keep the scrolling short, one highlight each.

`add-4` (The part's settings open.)
- The page "Edit Part - HtmlBody" is open. Highlight the label **What type of editor should be used?** and its drop-down
  (`select[data-type-part-editor-select]`). Select **Wysiwyg editor** with `SelectAsync` (the value is `Wysiwyg`).
  Do not select **Trumbowyg editor** or **Monaco editor**: their settings panels show editor options written as code.
- On screen when the narration ends: the drop-down reading **Wysiwyg editor**, and no settings panel below it.
- Labels: the question, `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:70` and the drop-down `:72`; the options, `OrchardCore.Html/Views/HtmlBodyPart.Option.cshtml:4`
  (Standard), `HtmlBodyPart-Wysiwyg.Option.cshtml:4` (Wysiwyg editor), `HtmlBodyPart-Trumbowyg.Option.cshtml:4`,
  `HtmlBodyPart-Monaco.Option.cshtml:4`. A headless browser draws no open list: the narration does not need one.

`add-5` (Sanitize Html is checked by default.)
- Highlight the check box **Sanitize Html** (`.form-check` with the text "Sanitize Html"), which is checked and has the
  hint "Whether the Html is sanitized, e.g. by removing script tags (and more)." Wait 1.5 s, clear the highlight, click
  **Save** (`button.save`, `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the page "Edit Content Type - Announcement" has loaded (see `add-6`).
- Labels: Sanitize Html and its hint, `OrchardCore.Html/Views/HtmlBodyPartSettings.Edit.cshtml:7-9`; Save,
  `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:111`. Default on: `OrchardCore.Html.Core/Settings/HtmlBodyPartSettings.cs:11`.

`add-6` (Back on Announcement.)
- No action; the page shows the green notice "The "HtmlBody" part settings have been saved." Highlight the badge next
  to **HtmlBody** in the Parts list (it reads **Wysiwyg**) and hold it until the narration ends.
- On screen when the narration ends: the Parts list with Title, Autoroute, Announcement and **HtmlBody** with a
  **Wysiwyg** badge.
- Labels: the badge shows the value of the editor, `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:187-189`; the
  notice, `OrchardCore.ContentTypes/Controllers/AdminController.cs:1084`. Do not click **Save** on this page: the part
  and its editor are already saved.

After this clip, stop the site and make snapshot S1.

## Clip `03-html`

**Starts on** `/Admin/Contents/ContentItems` (the page reached by **Content**, **Content Items**), signed in. The menu
entry **Content** is expanded. Capture starts there.

`html-1` (Let's create an announcement.)
- Click **New** (`#new-dropdown`). In the open menu click **Announcement** (`a.dropdown-item` with that text,
  `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the page "New Announcement" with the fields **Title** and the editor labeled
  **HtmlBody**.
- Labels: Content and Content Items, `OrchardCore.Contents/AdminMenu.cs:65` and `:68`; New,
  `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`; the page title "New {type}",
  `OrchardCore.Contents/Views/Admin/Create.cshtml:26`. The list holds six creatable types (Announcement, Article, Blog,
  Help Topic and Page from the recipe and the seed, and Taxonomy, `OrchardCore.Taxonomies/Migrations.cs:26`), so the button is
  a menu (`ContentsAdminListCreate.cshtml:18-22`); its entries are the type display names.

`html-2` (The editor labeled HtmlBody is the Wysiwyg editor we chose.)
- Highlight the label **HtmlBody** (0.8 s), clear the highlight. Type the title `Garden Tour This Saturday` into
  **Title** (`input.content-caption-text`). Click inside the editing area of the HtmlBody editor (the white area
  under the toolbar, `.html-body-part-wysiwyg-editor .trumbowyg-editor`, the `contenteditable` element Trumbowyg creates) and type `Join us for a guided walk through the community garden.`
- On screen when the narration ends: the Title filled in, and one line of text in the editor, with the editor toolbar above it.
- Labels: Title, `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; the editor label is the part's display name
  (`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22`, `Model.TypePartDefinition.DisplayName()`), which is
  HtmlBody (see above); the editor is the one chosen in `add-4`
  (`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml`, wrapper class `html-body-part-wysiwyg-editor`).

`html-3` (The toolbar above the text formats it.)
- In the editing area press `End`, then `Enter`. Click the toolbar button for **bold** (expected
  `.html-body-part-wysiwyg-editor .trumbowyg-strong-button`), type `Doors open at ten o'clock.`, click the same button
  again to switch bold off, press `Enter`. Click the toolbar button for an **unordered list** (expected
  `.trumbowyg-unorderedList-button`), type `Bring a sun hat`, press `Enter`, type `Wear closed shoes`.
- On screen when the narration ends: the editor shows the sentence, a bold sentence, and a two-item bulleted list.
- Labels: the buttons are Trumbowyg's own. The Wysiwyg editor starts the library with `extendDefaultButtons: true`
  (`OrchardCore.Html/Assets/ts/html-body-part-wysiwyg.ts:17`), which keeps the library's default buttons and inserts
  `insertShortcode` before `insertImage` (the code is bundled in
  `OrchardCore.Html/wwwroot/Scripts/html-body-part-editors/html-body-part-wysiwyg.js`). The default buttons of the vendored
  Trumbowyg 2.31.0 include `strong` (title "Strong") and `unorderedList` (title "Unordered list")
  (`OrchardCore.Resources/wwwroot/Scripts/trumbowyg/trumbowyg.js`, `defaultOptions.btns`), and each button gets the class
  `trumbowyg-<name>-button`. Confirm both on the running site (OPEN-QUESTIONS.md item 1). The narration speaks of bold
  words and a list only.
- This step has about 70 typed characters and four clicks for a 25-word narration (about 10 s): type with a 25 ms delay.

`html-4` (We click Publish, then View on the item in the list.)
- Click **Publish** (`button.publish`, `ClickAndWaitForNavigationAsync`; it returns to the Content Items list). In the row
  of **Garden Tour This Saturday** click **View**
  (`scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Garden Tour This Saturday" }).Locator("a.view")`;
  the rows of the Content Items list are `li.list-group-item`, `OrchardCore.Admin/Views/AdminList.cshtml`). It opens a second tab: take it with the
  context's page-created event, call `SwitchToAsync` with it, and wait for the page to load.
- On screen when the narration ends: the public page of the announcement, with the title, the paragraph, the bold
  sentence and the bulleted list, rendered as formatted text.
- Labels: Publish, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7` (the form has no return URL, so there is
  a single button) and `:19` (with a return URL, a button group whose first button is **Publish**); View,
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`. The front end shows the stored HTML as it is
  (`OrchardCore.Html/Views/HtmlBodyPart.cshtml`).
- The page address is the slug of the title, `/garden-tour-this-saturday`, and the page theme is TheBlogTheme. Check
  the page by eye, not by the narration: OPEN-QUESTIONS.md item 6.

## Clip `04-standard`

**Starts on** `/Admin/Contents/ContentItems/Blog` (the list of items of the type Blog, which holds one item titled
**Blog**), signed in. Capture starts there.

`std-1` (Let's edit the Blog item, which uses the Standard editor of HtmlBody.)
- Click **Edit** in the row of **Blog** (`a.edit`, `ClickAndWaitForNavigationAsync`). Highlight the editing area of the
  **HtmlBody** editor.
- On screen when the narration ends: the Blog editor page, the HtmlBody editor as a plain text box with a line number
  1 in the margin and the text "This is the description of your blog".
- Labels: Edit, `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; the Standard editor
  (`OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml`, a text area turned into a code editor with line numbers by
  `OrchardCore.Html/Assets/ts/html-body-part.ts:8`, `lineNumbers: true`). The Blog recipe gives the Blog type's
  HtmlBodyPart no `Editor`, which is the Standard editor (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).

`std-2` (Nothing is formatted for you here.)
- Click at the end of the text in the editor (`.CodeMirror`) and wait 2 s without typing. Then click **Cancel**
  (`a.cancel`, `ClickAndWaitForNavigationAsync`), which returns to the list without saving.
- On screen when the narration ends: the list of Blog items (or the editor, if the click on Cancel is still loading).
- Labels: Cancel, `OrchardCore.Contents/Views/Content.Edit.cshtml:30`.
- Do not type anything and do not click **Publish** or **Save Draft**: the clip must leave the site as it found it.

## Clip `05-markdown`

**Starts on** `/Admin/Contents/ContentItems`, signed in, with the menu entry **Content** expanded. Capture starts there.

`md-1` (Help Topic is a content type with a MarkdownBody part.)
- Click **New**, then **Help Topic** (`ClickAndWaitForNavigationAsync`). Type the title `Borrow a Library Book` into **Title**
  (`input.content-caption-text`).
- On screen when the narration ends: the page "New Help Topic" with the Title filled in and the editor labeled
  **MarkdownBody** with its toolbar and an empty text area.
- Labels: New, `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`; Title,
  `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; MarkdownBody, the part's display name
  (`OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml:20`); the hint "The markdown text of the content item."
  is `:22`.

`md-2` (Markdown is plain text with simple marks.)
- Click inside the editor text area (`.markdown-body-part-wysiwyg-editor .CodeMirror`). Type `## How borrowing works`,
  press `Enter` twice, type `Show your library card at the front desk to borrow a book.`
- On screen when the narration ends: two lines of text with the marks visible, the first starting with two hash signs.
- Labels: the editor, `OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml` (wrapper class
  `markdown-body-part-wysiwyg-editor`, initialized by `OrchardCore.Markdown/Assets/ts/markdown-body-part-wysiwyg.ts`).
- The text is typed by hand here on purpose: the marks must be seen in the text before the toolbar adds the next ones.

`md-3` (The toolbar adds marks for you.)
- Press `Home`, press `Control+Right` three times (the cursor sits after "library"), then `Control+Shift+Left` to select the
  word "library". Click the toolbar button **Bold** (`.editor-toolbar button[aria-label="Bold"]`): the word becomes
  `**library**`. Press `End`, `Enter` twice. Click the toolbar button **Generic List**
  (`.editor-toolbar button[aria-label="Generic List"]`), which puts `* ` at the start of the line (EasyMDE's default list mark is `*`, `unorderedListStyle` in `easymde.min.js`), and type
  `Loans last three weeks`. Press `Enter`: the editor continues the list, so the new line already starts with `* `. Type
  `You can renew a book twice`.
- On screen when the narration ends: the heading line, the sentence with `**library**`, and a two-item list.
- Labels: the button titles come from `OrchardCore.Markdown/Assets/Scripts/mde.mediatoolbar.ts`: Bold (line 112),
  Generic List (line 190). The toolbar is used when the part has no editor options, as in the Help Topic seed
  (`OrchardCore.Markdown/wwwroot/Scripts/markdown-editors/markdown-body-part-wysiwyg.js`). EasyMDE 2.18.0
  (`OrchardCore.Markdown/wwwroot/Scripts/easymde.min.js`) writes the title into `aria-label` as it is, but writes the `title`
  attribute (the tooltip) with the shortcut appended, such as "Bold (Ctrl-B)", so a selector on `title="Bold"` finds nothing:
  use `aria-label`. The buttons are icons without a visible label; highlight each button for 0.5 s (`HighlightAsync`, no
  caption) just before its click, so the viewer sees which one the narration names. The same library binds `Enter` to the list
  continuation (`newlineAndIndentContinueMarkdownList`). Confirm on the running site (OPEN-QUESTIONS.md item 2).
- 21 words (about 8 s) for one selection, two toolbar clicks and about 45 typed characters: use a 25 ms delay and no pause other than the two 0.5 s highlights.

`md-4` (Toggle Preview and Toggle Side by Side.)
- Click the toolbar button **Toggle Preview** (`.editor-toolbar button[aria-label="Toggle Preview"]`) and wait 2 s: the text
  area is replaced by the formatted result. Click it again. Click **Toggle Side by Side**
  (`.editor-toolbar button[aria-label="Toggle Side by Side"]`) and wait 2 s: the editor fills the page with the text on the left
  and the result on the right. Click the same button again to leave side by side, then press `Escape`.
- Why `Escape`: Toggle Side by Side puts the editor in full screen when it turns on, and turning it off does not leave full
  screen (`OrchardCore.Markdown/wwwroot/Scripts/easymde.min.js`, the side-by-side function only toggles full screen on the way
  in). In full screen the editor covers the page, so **Publish** of `md-5` cannot be clicked. `Escape` leaves full screen
  (the same file binds `Esc` and a document key handler for key code 27). The alternative is the toolbar button **Toggle
  Fullscreen** (`aria-label`, `mde.mediatoolbar.ts:228`). Wait until `button.publish` is visible before the step ends.
- On screen when the narration ends: the editor back in its normal layout, inside the page (or, if a click is late, the
  side-by-side layout). Record `md-5` only from the normal layout.
- Labels: Toggle Preview and Toggle Side by Side, `OrchardCore.Markdown/Assets/Scripts/mde.mediatoolbar.ts` (lines 216 and 222);
  the tooltip adds the shortcut ("Toggle Preview (Ctrl-P)", "Toggle Side by Side (F9)"), see `md-3`.
- The step has five clicks, two waits and a key press for a 22-word narration (about 9 s): use the waits of 2 s and no other pauses.

`md-5` (We click Publish, then View.)
- Click **Publish** (`button.publish`, `ClickAndWaitForNavigationAsync`; it returns to the Content Items list). In the row of
  **Borrow a Library Book** click **View** (the same locator as in `html-4`, with `HasText = "Borrow a Library Book"`); take the second tab as in `html-4` and `SwitchToAsync` to it.
- On screen when the narration ends: the public page with the heading "How borrowing works", the sentence with the bold
  word "library", and the bulleted list.
- Labels: as in `html-4`. The page address is `/borrow-a-library-book`.

## Slide steps

These are drawn by `slides.py`; nothing is recorded.

- `kinds-1`: "Two body parts": cards for HtmlBody and MarkdownBody (bright), and a dimmed card saying both are parts of a content type.
- `kinds-2`: "Editors": four cards listing the editors of HtmlBody and of MarkdownBody.
- `kinds-3`: "From the editor to the page": four numbered cards, from "You write" to "Sanitize Html".
- `good-1`: "Which body fits your writers": three cards pairing a kind of text with a body and an editor.
- `good-2`: "Sanitize Html": three cards, "On by default", "A setting of each body part", "Turn it off with care".
- `good-3`: "Text and shortcodes": two cards, HTML in a Markdown body and shortcodes.
- `good-4`: "The Markdown field": three cards, a field and not a part, the same editors, the same output.

## Checks for private data

After recording each clip, run `review.py` and look at every contact sheet.

- Everything on screen is fake: the site is called Orchard Core, the user is `admin` with `admin@example.com`, the titles
  are "Garden Tour This Saturday" and "Borrow a Library Book". The password is never typed on camera (the sign-in comes from `state.json`).
- The page header of the admin may show the signed-in user name `admin` and nothing else about a person. If a real
  name appears anywhere, stop and fix the site; do not mask it.
- The address bar is not part of the frame. The public pages are on `localhost`, which is not shown.
- OCR the final video at one frame per second and at every scene change (`select='gt(scene,0.02)'`), and search for any
  address other than `example.com`, any phone number, and any token. Nothing may be found.
- `state.json`, `App_Data`, audio and MP4 files are never committed.
