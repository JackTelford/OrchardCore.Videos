# Open questions: HTML Sanitizer

Each item says what was checked in the Orchard Core source (commit `92db4a83`) and what a person must confirm on a
running site before or while recording. Paths are relative to the root of that checkout.

## 1. The names of the parts in the Parts list

Narration of `setting-1` and `setting-3` says "HtmlBody" and "MarkdownBody".

- Checked: the Parts list prints `partDefinition.DisplayName()` (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:146-163`).
  With no display name set, that is the part name without its "Part" suffix (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-61`).
  The Html migration sets only a description (`src/OrchardCore.Modules/OrchardCore.Html/Migrations.cs:32-34`), and the Blog
  recipe sets no display name for these parts (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:166-173`, `:230-238`).
- Confirm: the rows read exactly "HtmlBody" on the Article type and "MarkdownBody" on the Blog Post type. If not, change
  the narration, the `spoken` entries "HtmlBody" and "MarkdownBody", and `recording-plan.md`.

## 2. The admin menu on a direct visit

`recording-plan.md` opens `/Admin/ContentTypes/List` before the capture, and `setting-1` names the path Design, Content
Definition, Content Types.

- Checked: the menu entries and their labels (`src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`).
- Confirm: the admin menu shows that branch expanded and **Content Types** as the current page when the page is opened by
  address. If not, expand it before `StartCaptureAsync`, as the plan says.

## 3. Where **Sanitize Html** is on the Edit Part page, and its state

- Checked: the settings driver adds the check box at `Content:20` of the type part editor
  (`src/OrchardCore.Modules/OrchardCore.Html/Settings/HtmlBodyPartSettingsDisplayDriver.cs:12-19`), and the setting is on by
  default (`src/OrchardCore/OrchardCore.Html.Core/Settings/HtmlBodyPartSettings.cs:10-11`; the Markdown part has the same
  default in `src/OrchardCore.Modules/OrchardCore.Markdown/Settings/MarkdownBodyPartSettings.cs:7-8`).
- Confirm: on a new site the box is checked for both `Article` and `BlogPost`, and whether the row is visible without
  scrolling. The Article part uses the Wysiwyg editor, which adds editor settings to the same page and may push the row down. `recording-plan.md` measures it with `Recorder probe`; if the page scrolls inside a container
  instead of the window, write the container and the distance here.

## 4. The output of the example on the slide `idea-2`

The slide shows `<p>Welcome to the site.</p>` followed by a script element as the stored source, and `<p>Welcome to the
site.</p>` as the rendered page.

- Checked: the display path calls `IHtmlSanitizerService.Sanitize` when `SanitizeHtml` is on
  (`src/OrchardCore/OrchardCore.Html.Core/Services/HtmlDisplayService.cs:35-38`), the part view writes the result
  unchanged (`src/OrchardCore.Modules/OrchardCore.Html/Views/HtmlBodyPart.cshtml:2`), and the setting's hint says it removes script
  tags "(and more)". The sanitizing library itself is not in the source tree.
- Confirm: sanitize the two-line sample on a running site and compare the result with the slide, for example with an
  item whose body uses the Standard editor (the Wysiwyg editor would alter what is typed). The script element and its
  content must both disappear, and the paragraph must be unchanged.
- Not claimed by the video: that event handler attributes or script addresses are removed. The slide `idea-1` names
  them only as markup that runs in a browser. To say more, sanitize `<p onclick="...">` and `<a href="javascript:...">`
  on a running site first.

## 5. The schemes accepted by the link checks (`configure-3`)

Narration: the link checks use the same sanitizer, so they accept the schemes you allow in the configuration.

- Checked: `LinkField` (`src/OrchardCore.Modules/OrchardCore.ContentFields/Handlers/LinkFieldHandler.cs:63-80`),
  `LinkMenuItemPart` and `HtmlMenuItemPart` (`src/OrchardCore.Modules/OrchardCore.Menu/MenuShapes.cs:201-225`) build
  `<a href="..."></a>`, sanitize it, and reject the URL when the result differs.
- Confirm: with `AllowedSchemes.Add("ftp")` configured, a Link Field accepts a URL such as
  `ftp://files.example.com/notice.pdf`. The documentation says the same about other schemes in links
  (`src/docs/reference/modules/ContentFields/README.md:198`: other URIs need the sanitizer to be configured). The video
  does not say what happens without the configuration.

## 6. Authors who edit in the Trumbowyg editor

The video says that what the author wrote is stored unchanged.

- Checked: the display drivers store the posted text as it is (`src/OrchardCore.Modules/OrchardCore.Html/Drivers/HtmlBodyPartDisplayDriver.cs:40-48`,
  `src/OrchardCore.Modules/OrchardCore.Markdown/Drivers/MarkdownBodyPartDisplayDriver.cs:39-47`), and the documentation says the same.
- Not verified: `src/OrchardCore.Modules/OrchardCore.Html/Views/HtmlBodyPart-Trumbowyg.Edit.cshtml:26` fills the editor with the sanitized value, and the editor
  script restores the original value on submit only when the author did not change the content
  (`src/OrchardCore.Modules/OrchardCore.Html/wwwroot/Scripts/html-body-part-editors/html-body-part-trumbowyg-editor.js`). So an author who edits and saves in that
  editor posts what the editor holds. The video does not mention editors; a person should check this on a running site
  before any video claims more.

## 7. Pronunciation

The `spoken` map of `storyboard.json` has new spellings: "I HTML Sanitizer Service", "I HTML Content",
"Configure HTML Sanitizer", "Add Orchard CMS", "Orchard dot Sanitize HTML", "sanitize underscore HTML", "mail to" and
"F.T.P.". They were not played, because the checking script writes audio. A person confirms them with
`scripts/pronounce.py` before narrating, and adjusts the map. Names with no entry that the voice could misread are
"GraphQL", "tel", "Razor", "Liquid" and "markdownify"; check them in the same pass.
