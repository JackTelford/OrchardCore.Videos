# Open questions: html-and-markdown-editors

What the author could not verify from the Orchard Core source (commit `92db4a83`) or the documentation, and what a person
must confirm on a running site before the first take. The narration states only what the source shows; each item below
says which step depends on it and what to do if the site differs.

The two body editors are built on libraries that are not in the source folder: Trumbowyg (the Html "Wysiwyg editor"),
EasyMDE (the Markdown "Wysiwyg editor") and the shared components of the `@orchardcore/bloom` package that start them
(`trumbowyg-editor`, `easymde-editor`, `shape-option-selector`). Everything about their toolbars is therefore expected,
not verified.

## 1. The toolbar of the Html "Wysiwyg editor" (steps `html-2`, `html-3`, `good-3`)

- Source: `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml`, `OrchardCore.Html/Assets/ts/html-body-part-wysiwyg.ts`
  (starts Trumbowyg with `extendDefaultButtons: true`), `OrchardCore.Resources/Assets/trumbowyg/js/trumbowyg.shortcodes.js`
  (the `insertShortcode` button, title "Insert Shortcode"), `OrchardCore.Html/Assets/trumbowyg.media.tag.ts` (the `insertImage`
  button, title "Insert Media", added when the Media feature is enabled).
- Read in the source (second pass): the editor is started with `extendDefaultButtons: true`; the bundled component
  (`OrchardCore.Html/wwwroot/Scripts/html-body-part-editors/html-body-part-wysiwyg.js`) keeps the default `btns` of the
  vendored Trumbowyg 2.31.0 (`OrchardCore.Resources/wwwroot/Scripts/trumbowyg/trumbowyg.js`), which contain `strong` (title
  "Strong") and `unorderedList` (title "Unordered list"), and inserts `insertShortcode` before `insertImage`. Each button
  gets the class `trumbowyg-<name>-button`, and the editing area is a `contenteditable` element with the class
  `trumbowyg-editor`. So `html-3` (bold and a list) and the Html half of `good-3` (a button to insert shortcodes, title
  "Insert Shortcode" from `trumbowyg.shortcodes.js:7`) follow from the source.
- Still to confirm on the running site with `Recorder probe` on the page "New Announcement": that the toolbar draws these
  buttons in this order, that a click on a button keeps the typing position in the editing area, and what the bold
  button writes (the editor is started with `semantic: false`, so the markup differs from the library's default; the
  narration says only "bold"). If a button is missing, report it and change `html-3` before recording.
- The Markdown editor's button for shortcodes is titled "Insert Shortcode" (`OrchardCore.Markdown/Assets/Scripts/mde.mediatoolbar.ts:157`).

## 2. The toolbar of the Markdown "Wysiwyg editor" (steps `md-3`, `md-4`, `good-3`, `good-4`)

- Source: `OrchardCore.Markdown/Assets/Scripts/mde.mediatoolbar.ts` defines the toolbar with the titles Bold (line 112),
  Heading (131), Generic List (190), Toggle Preview (216), Toggle Side by Side (222) and Insert Shortcode (157), and
  `OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml` loads that script with the editor.
- Read in the source (second pass): when the part has no editor options (the Help Topic seed), the bundled component
  (`OrchardCore.Markdown/wwwroot/Scripts/markdown-editors/markdown-body-part-wysiwyg.js`) uses the whole `mdeToolbar`
  above, so the titles are those of the file. EasyMDE 2.18.0 (`OrchardCore.Markdown/wwwroot/Scripts/easymde.min.js`)
  copies the title into `aria-label` and writes the tooltip (`title`) with the shortcut appended ("Bold (Ctrl-B)"), so the
  plan selects the buttons by `aria-label`. It binds `Enter` to the list continuation, so `md-3` no longer branches.
  Toggle Side by Side puts the editor in full screen when it turns on and does not leave full screen when it turns off;
  `md-4` therefore ends with `Escape` (without it, `md-5` cannot reach **Publish**).
- Still to confirm on the running site: that the toolbar buttons have the `aria-label` values of the titles, that the
  split layout fills the page as described, and that `Escape` returns the editor to the page.
- Do not open the Edit Part page of a Markdown part in the capture: with the Wysiwyg editor selected it shows an options
  box written as code (`OrchardCore.Markdown/Views/MarkdownBodyPartWysiwygEditorSettings.Edit.cshtml`).

## 3. What the Standard editors look like (steps `std-1`, `std-2`, `kinds-2`)

- Source: `OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml` and `OrchardCore.Html/Assets/ts/html-body-part.ts` (CodeMirror in
  HTML mode with `lineNumbers: true`, closing tags on typing). `kinds-2` says "A text box with line numbers for the HTML
  source", and `std-1` says it shows "the HTML of the body as stored".
- Confirm on the Blog item that the line number 1 is visible in the margin and that the text is the plain sentence
  "This is the description of your blog" (`blog.recipe.json`, the Blog item). If that item were to show HTML tags, replace
  the clip's page: the admin video must not show markup.
- Source of the Markdown Standard editor: `OrchardCore.Markdown/Views/MarkdownBodyPart.Edit.cshtml` (a text area). `kinds-2`
  calls it "A plain text box"; it is not on screen in this video.

## 4. Selectors the plan expects but the source does not show

- The editing area of Trumbowyg (`.trumbowyg-editor`, from the vendored library) and of EasyMDE (`.CodeMirror`). Both come
  from the libraries' code, not from an Orchard view.
- The admin menu entries **Tools**, **Design**, **Content Definition**, **Content Types**, **Content** and **Content
  Items**: the labels come from the `AdminMenu.cs` files, the markup from the admin theme, which the author did not read.
  Confirm the menu can be expanded by clicking its entries, and that **Design**, **Content Definition** and **Tools** start
  collapsed in the recorded browser.
- The admin list rows (Content Types, Content Items, Features): the buttons carry the classes and `role` values used in
  the plan (`a[role="btn-edit-Announcement"]`, `a.edit`, `a.view`), taken from the `.cshtml` files.

## 5. Names and descriptions shown on screen

- The part names **HtmlBody** and **MarkdownBody** are not stored anywhere: they are the part name without the final "Part"
  (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`). Confirm them in the
  Add Parts list, in the Parts list of the type, in the title "Edit Part - HtmlBody" and as the label of the editor on the
  content item page. If the site shows another name (for example "Html Body"), change every narration that says HtmlBody
  or MarkdownBody.
- The descriptions in the Add Parts list ("Provides an HTML Body for your content item." and "Provides a Markdown formatted
  body for your content item.") are written when a site creates the part (`Migrations.cs` of each module), so they are
  present on a site set up from scratch.
- The Features page lists the features by the module name, **Html** and **Markdown**, each with the badge **Enabled** on a
  Blog-recipe site (`add-1`).
- The order of the options in the drop-down "What type of editor should be used?" comes from the shape table, not from a
  list in the source. The narration and the slide use the order Standard, Wysiwyg editor, Trumbowyg editor, Monaco editor,
  which is only an order of reading. The drop-down is not opened on camera.

## 6. The public pages (steps `html-4`, `md-5`)

- Announcement and Help Topic have no template of their own in TheBlogTheme: they use the generic content template
  (`OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`). The source shows that the body part writes its HTML into
  the page (`OrchardCore.Html/Views/HtmlBodyPart.cshtml`, `OrchardCore.Markdown/Views/MarkdownBodyPart.cshtml`), not how the theme lays
  it out. Look at both pages before recording. The narration says only that the page shows formatted text, a heading,
  bold words and a list. If the title appears twice, or the body is missing, report it and do not hide it in the edit.
- The layout of TheBlogTheme links Google Fonts (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), so the recorded
  browser asks `fonts.googleapis.com` for them on a public page. That is a request from the browser, not text on the screen:
  with no network the page falls back to other fonts. Look at the page before recording and decide whether that suits the take.
- The address of each item is the slug of its title (`/garden-tour-this-saturday`, `/borrow-a-library-book`), from the
  Autoroute pattern in the seed. The plan clicks **View**, so the address does not matter on screen.

## 7. Behaviour stated in the narration from the source only

Confirm each one on the site while preparing, so that no sentence is false on the day:

- `good-2` and `add-5`: Sanitize Html is checked by default on a part that was just added, and removes unsafe markup such as
  script tags (the hint text in `OrchardCore.Html/Views/HtmlBodyPartSettings.Edit.cshtml:9`).
- `good-3`: HTML typed into a Markdown body appears as text on the page by default (the Markdown pipeline defaults to
  `nohtml+advanced`, `OrchardCore.Markdown/Startup.cs:26`, and the documentation says HTML is turned into escaped entities).
  Try it with a throwaway item that is not in the video.
- `kinds-3` and the closing card: the stored text is what the editor sends, without a change by the site
  (`OrchardCore.Html/Drivers/HtmlBodyPartDisplayDriver.cs:40-48`, `OrchardCore.Markdown/Drivers/MarkdownBodyPartDisplayDriver.cs:39-47`),
  and Markdown is converted, shortcodes are replaced and the result is sanitized only when the page is shown
  (`OrchardCore.Html.Core/Services/HtmlDisplayService.cs`, `OrchardCore.Markdown/Services/MarkdownDisplayService.cs:28-35`).
  With a Wysiwyg editor the stored HTML is what the editor writes, so the narration says "without changing it", not "as you typed it".
- `good-4`: the Markdown field offers the editors Standard and Wysiwyg editor, and has the Sanitize Html setting
  (`OrchardCore.Markdown/Views/MarkdownField.Option.cshtml`, `MarkdownField-Wysiwyg.Option.cshtml`, `MarkdownFieldSettings.Edit.cshtml`).

## 8. Pronunciation

The storyboard's `spoken` map has five entries that a person must confirm with `scripts/pronounce.py` before narrating:
`HtmlBody` as "H.T.M.L. Body", `MarkdownBody` as "Markdown Body", `Html` as "H.T.M.L.", `Wysiwyg` as "Wizzywig" and
`Trumbowyg` as "Trumbo wig". The order of the entries matters: `narrate.py` replaces them one after the other, so `HtmlBody`
must stay before `Html`. Listen also to "Monaco", "Autoroute", "shortcodes" and "HTML" read as letters.

## 9. The seed JSON

The JSON in `recording-plan.md` copies the shape of the `ContentDefinition` step of the Blog recipe. It was not run. If JSON
Import refuses it, create the two types in the admin instead: Design, Content Definition, Content Types, Create new type
(Display Name "Announcement", then "Help Topic"), add the parts Title and Autoroute (pattern as in the plan), and for
Help Topic also MarkdownBody with the Wysiwyg editor. Mark the types creatable and listable in their settings.
