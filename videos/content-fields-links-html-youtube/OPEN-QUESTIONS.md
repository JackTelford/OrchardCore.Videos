# Open questions: Content Fields: Links, Rich Text, Markdown and YouTube Videos

The narration states only what was read in the Orchard Core source (commit 92db4a83) and its documentation. The points
below were not verifiable without a running site, or depend on code that is not in that source (the editor libraries and
the shared scripts of `@orchardcore/bloom`). A person confirms each one on the recording site before the first take, and
writes the answer here or in the pull request.

## Pronunciation (confirm with `scripts/pronounce.py` before narrating)

The storyboard's `spoken` map holds two candidates that nobody has played yet (`Html` as `H.T.M.L.`, `Wysiwyg` as `wizzywig`); confirm or correct them. Words that the voice may misread:

- **Html** (in "Html Field" and "Sanitize Html", the labels as the code spells them). Candidate: `"Html": "H.T.M.L."`.
  Plain "HTML" in two steps (`html-3`, `safe-2`) is already read as letters.
- **Wysiwyg** (in "Wysiwyg editor"). Candidate: `"Wysiwyg": "wizzywig"`.
- **Youtube** (the label "Youtube Field"). Probably read like "YouTube"; confirm.
- **Markdown** (the label "Markdown Field"): confirm that it is read as one word.

## Facts to confirm on the running site

1. **Technical names.** The Technical Name is computed from the Display Name on key up by a helper that is not in this
   source (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts` calls `getTechnicalName`). Expected: `RegistrationLink`,
   `Schedule`, `WhatToBring`, `TeaserVideo`. Nothing in the narration depends on the exact value, and the selectors of the
   plan do not use it.
2. **Editor input names.** The plan selects the boxes of the content item editor by the end of their `name` attribute
   (`.Url`, `.Text`, `.Target`, `.Markdown`, `.RawAddress`, `TitlePart.Title`). Confirm with `Recorder probe` that the names
   end this way, and that each one matches a single element on the page.
3. **The Wysiwyg toolbar of the Html Field.** The editor is built by a library whose markup is not in this source
   (`OrchardCore.ContentFields/Assets/ts/html-field-wysiwyg.ts` only starts it). The plan guesses
   `.html-field-wysiwyg-editor .trumbowyg-editor` for the editable area and `.trumbowyg-strong-button` for the bold button.
   Confirm both, and that a click on the bold button before typing makes the next text bold. The narration says only
   "a toolbar" and "format them", and names no button. The toolbar also has a button that shows the HTML of the text:
   it must not be clicked on camera. The plan types the pieces of the Schedule with the keyboard after one click, so the
   bold state set by the toolbar button is not reset by another click of the box: confirm that `10:00 to 12:00` comes
   out bold and `Doors open at 9:30.` comes out as a second paragraph. If the cursor or the bold state is lost, say what
   happens here, and the plan will be changed.
4. **No code panel on camera.** In the source, choosing **Wysiwyg editor** for the Markdown Field shows a panel of editor
   options written as code (`OrchardCore.Markdown/Views/MarkdownFieldWysiwygEditorSettings.Edit.cshtml`, shown by the
   selector of `OrchardCore.ContentTypes/Assets/js/edit-field-selector.ts` for the class `field-editor-wysiwyg`), and so
   does **Trumbowyg editor** for the Html Field (`HtmlFieldTrumbowygEditorSettings.Edit.cshtml`) and **Monaco editor**
   (`HtmlFieldMonacoEditorSettings.Edit.cshtml`). The video chooses Standard for the Markdown Field and Wysiwyg editor
   for the Html Field, and no settings driver exists for the Html Field's Wysiwyg editor, so no panel is expected.
   Confirm on the screen of `html-3`, and again on the contact sheet of `markdown-2`.
5. **The editor and the front end.** In the content item editor, confirm that the Title box and the four fields are shown, and in which order
   (the narration of `fill-1` does not claim an order). Confirm in the theme of the Blog recipe that the item shows its Title as a heading (the Title part
   renders it by default: `OrchardCore.Title/Models/TitlePartSettings.cs:29-30`), that the four fields follow in the order
   they were added, and that a 560 pixel wide frame fits the content column.
6. **The Content Items list of a type**, opened with **List Items** from the Content Types list (`/Admin/Contents/ContentItems/Workshop`): confirm that the row of the item shows
   **Edit** and **View**, and that **View** opens a new tab.
7. **Content Types list.** The row buttons are drawn by the same shape in the List and the Grid layouts; confirm that
   `a[role="btn-edit-Workshop"]` and `a#btn-create-Workshop` are visible in the layout of the recording site.
8. **Narrated claims about Markdown and sanitizing**, none of them demonstrated on screen. Try them once on the site, on a
   throwaway item: HTML typed inside a Markdown Field is displayed as text (the default Markdown extensions are
   `nohtml+advanced`: `OrchardCore.Markdown/Startup.cs:26`; the documentation says HTML is then escaped, and the library
   that does it is not in this source); a script tag typed in an Html Field with **Sanitize Html** ticked is removed when
   the item is displayed; a Link Field rejects an address that the sanitizer changes, for example `javascript:alert(1)`
   (`LinkFieldDisplayDriver.cs:110-115`: the link `<a href="...">` is compared with its sanitized form, and the sanitizer
   drops an `href` whose scheme is not allowed). Do not use an address with a space in it as the example: the address is
   escaped before it is tested (`ToUriComponents` in `src/OrchardCore/OrchardCore.Abstractions/StringUriExtensions.cs:5-16`
   uses `UriFormat.UriEscaped`), so a space becomes `%20` and the address is probably accepted. Relative addresses were
   not checked at all (the same method calls `Uri.GetComponents`, which the .NET documentation says needs an absolute
   address); the video uses an absolute address only.
9. **Bold in the Schedule on the site.** The narration of `site-3` says that the Schedule keeps the formatting applied
   in the toolbar. The Html Field display runs the sanitizer by default (`HtmlFieldDisplayDriver.cs:34-37`,
   `Sanitize Html` ticked), and the sanitizer is the library's default list of tags, with `class` allowed and
   `form` removed (`OrchardCoreBuilderExtensions.cs:17-24`). The tag that the toolbar writes for bold comes from the editor library, not
   from this source. Confirm that the times are bold on the site; if they are not, change the formatting used in `fill-3`
   to one that survives, and the narration of `site-3` stays true.

## The Youtube Field and the allowed domains

- The programme allows `example.com` as the only real domain in what is written, but the Youtube Field exists to show a
  YouTube video. The plan therefore types `https://www.example.com/watch?v=pottery-teaser`, and answers the embedded
  address `https://www.example.com/embed/pottery-teaser` with a stand-in page (a dark panel with a green triangle). The
  question for the maintainer is in the author's report: may the recording use a public YouTube address instead?
  If yes, the stand-in route is removed and the playback must be checked on the contact sheet (an embedded player
  can refuse to play in a headless browser, and needs access to the internet).
- Only an address with a `v` value in its query string is converted correctly
  (`OrchardCore.ContentFields/Drivers/YoutubeFieldDisplayDriver.cs:64-71`). An address without a query, such as a short
  link, gets an embedded address with two slashes after `embed` (lines 77-81: the path already starts with a slash).
  An address that is not a valid URI makes `new Uri(...)` throw (line 62 and `Handlers/YoutubeFieldHandler.cs:30`). The
  narration claims none of this, and the video shows only the `v` form. Confirm on the site before anyone adds
  such a step.
- The display template falls back to a frame of 640 by 640 when the settings hold no width and height
  (`OrchardCore.ContentFields/Views/YoutubeField.cshtml:8-9`), while the settings page shows 560 by 315 as the defaults
  (`Settings/YoutubeFieldSettingsDriver.cs:19-20`). The plan saves the settings page, so 560 by 315 are stored, and the
  narration of `site-5` ("the width and height from the field settings") holds. Confirm the size on the site.

## Documentation and code disagree (reported in the author's result, not edited here)

- Link Field: the documentation says that only the http and https schemes are allowed without configuring the HTML
  Sanitizer; the code adds `mailto` and `tel` to the allowed schemes by default
  (`src/OrchardCore/OrchardCore.Infrastructure/Html/OrchardCoreBuilderExtensions.cs:22-23`), and the Link Field validates
  with that sanitizer (`Handlers/LinkFieldHandler.cs:74-79`). The video says nothing about these schemes. Confirm on the
  site that a `mailto:` address is accepted by the Link Field before the documentation is corrected.
- Link Field: the table of fields in the documentation lists the properties `string Url, string Text`; the field class
  also has `Target` (`src/OrchardCore/OrchardCore.ContentFields.Core/Fields/LinkField.cs`), and the video uses it.
- The Sanitize Html setting is written `Sanitize HTML` in the Markdown documentation (the "Sanitization" section); the
  label on the screen is `Sanitize Html` (`OrchardCore.Markdown/Views/MarkdownFieldSettings.Edit.cshtml:14`,
  `OrchardCore.ContentFields/Views/HtmlFieldSettings.Edit.cshtml:15`). The video uses the label on the screen.
- Markdown editors: the Markdown documentation names the default editor `Default`, and the shapes `Markdown_Option__{Name}`,
  `Markdown-{Name}.Option.cshtml`, `Markdown.Edit.cshtml` and `Markdown-Wysiwyg.Edit.cshtml`. The screen shows `Standard`
  (`OrchardCore.Markdown/Views/MarkdownField.Option.cshtml:4`), and the shapes are named after the part or the field type:
  `MarkdownField-Wysiwyg.Option.cshtml`, `MarkdownField-Wysiwyg.Edit.cshtml`, `MarkdownBodyPart-Wysiwyg.Edit.cshtml`
  (the lookup is `{type name}_Option__{Name}`: `OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:17` and
  `EditTypePart.cshtml:15`). The video uses the label on the screen.
