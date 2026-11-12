# Open questions: shortcodes-built-in

Everything in the narration was read from the Orchard Core source (commit `92db4a83`) or its documentation. What
follows could not be checked without a running site, or is not in the source tree. A person confirms each point on
the recording site before the first take, and tells the author if the narration must change.

## 1. The body editors of the page

The body editor of an Article is Trumbowyg 2.31.0, an npm package (`OrchardCore.Resources/ResourceManagementOptionsConfiguration.cs:1053-1066`,
`OrchardCore.Resources/Assets/package.json`). Its source is not in the tree; its minified build is
(`OrchardCore.Resources/wwwroot/Scripts/trumbowyg/trumbowyg.js`, one line). Read there:

- a toolbar button is `<button class="trumbowyg-<name>-button" title="<text of the language table>">`, so the tooltips
  are `Insert Shortcode` (`trumbowyg.shortcodes.js:7`) and `Insert Media` (`trumbowyg.media.tag.ts:80`, which replaces
  the English text of `insertImage`), and the classes are `.trumbowyg-insertShortcode-button` and
  `.trumbowyg-insertImage-button`;
- the editable area has the class `trumbowyg-editor`; Enter makes `p` blocks (the paragraph separator is set to `p`,
  and the first typed text is wrapped in a `p`);
- a button acts on `mousedown` and keeps the selection in the editor, so a click from Playwright works;
- the page's editor (`html-body-part-wysiwyg.js`) adds `insertShortcode` at index 6 of the default buttons, so the
  Insert Shortcode button sits after Link and before the group of Insert Media.

Not known without a running site: that both buttons show in the toolbar of the Article body as icons with those
tooltips, and that the three seed lines are three `p` elements. Check with `Recorder probe`. The narration of `image-1`
assumes both buttons are there.

The Markdown editor of `where-1`: `MarkdownBodyPart-Wysiwyg.Edit.cshtml` loads the toolbar script
`mde.mediatoolbar.ts`, which defines the buttons titled `Insert Shortcode` and `Insert Image`, and the compiled
`markdown-body-part-wysiwyg.js` uses that whole toolbar when the editor has no options (the setting `Options` of
`MarkdownBodyPartWysiwygEditorSettings` is empty unless a person fills it; the Blog recipe sets none). A person only
confirms on a Blog Post, whose body is a Markdown body with this editor, that the toolbar shows both buttons. If a
site has custom toolbar options, the buttons can be missing there.

## 2. Spaces and quotes typed into the editor

`options-1` and `options-2` type ` width="480"` and ` alt="Spring tulips"` into a rich text editor. A browser can store
a typed space as a non-breaking space, and a shortcode with such a space inside its opening tag is not recognized. The
published page decides: the picture must be smaller, and the text `[image` must not show on the page. If the shortcode
shows as text, type the options another way (for example paste them, or insert them with the clipboard) and record
again. If no way of typing them works, tell the author.

## 3. The caret after Add, and the arrow key counts

`trumbowyg.shortcodes.js` inserts the default value `[asset_url] [/asset_url]` as a text node and does not place the
caret between the tags. The plan therefore clicks the end of the line, presses End, and presses `ArrowLeft` 12 times
(`[/asset_url]`) before `Backspace` deletes the space; in `03-options` it presses `ArrowLeft` 20 times
(`]home-bg.jpg[/image]`). Confirm on the contact sheet that the caret lands where the plan says. If a take shows the
text in the wrong place, adjust the count rather than the narration.

## 4. The media library dialog

`MediaPickerModal.vue` draws the dialog's title and buttons from translation strings that are set when the page
loads (`t.selectMedia`, `t.ok`, `t.cancel`). The server sends the group `media-gallery` with the text `Ok`
(`OrchardCore.Media/Services/MediaJSLocalizer.cs`), while the component's defaults read `OK` and `Select Media`. The
narration names neither, and only says "confirm". Read the real labels on screen. Also confirm:

- the dialog opens in the table (list) view, with the checkbox column (`media-gallery/src/services/LocalStorage.ts:9`
  sets the default to the table view);
- the root folder holds exactly `about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`;
- the confirm button is enabled once one checkbox is ticked.

## 5. The resized picture (`options-3`)

The narration says the page shows a smaller picture because the site served a copy resized to the requested width.
From the code: `ImageShortcodeProvider.cs` adds `width=480` to the file address, and `MediaCommandParser.cs` keeps a
width only when it is one of `SupportedSizes`, whose default is `16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048`
(`OrchardCore.Media/Services/MediaOptionsConfiguration.cs:10`). Confirm that the image address of the published page
holds `width=480` and that the picture is visibly smaller than in `image-4`. If the site is configured with other
sizes, 480 may be ignored.

The narration of `image-4` does not say how large the picture is. The Blog theme has no maximum width for images
(`TheBlogTheme/wwwroot/css/styles.css`), so the 1900 pixel file may extend past the text column. Look at the contact
sheet: if the page looks broken, tell the author.

## 6. Links

The title of the video says "Links". The `asset_url` shortcode returns an address and no tag
(`AssetUrlShortcodeProvider.cs`, the comment above its last statement), and the narration says exactly that, and
shows the address as plain text. Not verified: whether the editor's link dialog accepts a shortcode as the address of
a link, and whether the saved link is replaced when the page is displayed (the link dialog belongs to Trumbowyg, see
question 1). If a person confirms it on a running site, a clip that makes a link from `asset_url` can be added in a
later revision of the video.

## 7. The address on the page (`list-4`)

Read from the code: for the Default tenant, with no CDN and no path base, the address is `/media/post-bg.jpg`
(`DefaultMediaFileStore.cs:224-236`, with the base path built in `OrchardCore.Media/Startup.cs:142-144`; the
documentation shows `/my-tenant/media/my-image.jpg` for a tenant named `my-tenant`). The narration says "the address
of the file on the site". A person confirms what appears on the page of the recording site.

## 8. Pronunciation

`storyboard.json` has `"spoken": { "asset_url": "asset underscore U.R.L." }`. A person must confirm this spelling with
`scripts/pronounce.py` before narrating, as the skill requires for a new spelling. The same applies to the new entries for the file names `home-bg.jpg` and `post-bg.jpg`
("home dash B.G. dot J.P.G.", "post dash B.G. dot J.P.G."). Also check how the voice says
"alt" (in `which-1` and `options-2`) and "locale".

## 9. Text in the dialog

The hint of the `image` card in Available Shortcodes reads "Add a image from the media library." in the source
(`OrchardCore.Media/Startup.cs:398`). It is visible on screen in `list-1`; the narration does not quote it.

## 10. What the video does not show

The `locale` shortcode is described on a slide from `LocaleShortcodeProvider.cs` and `Startup.cs`, and is not
recorded: it needs the Localization feature and a culture setup. Shortcode Templates (Design, Shortcodes) belong to
another video and are not shown.
