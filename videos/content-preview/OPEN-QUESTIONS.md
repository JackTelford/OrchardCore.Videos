# Open questions: Content Preview

What the author could not confirm from the Orchard Core source (commit 92db4a83) and the documentation alone. Each item
says what was checked and what a person must confirm on the running site while recording. If a check fails, the
narration step named in brackets must change before the clip is recorded.

## 1. What the preview window shows without the Preview part (`live-2`, `live-4`, `draft-2`)

- Checked: `PreviewController.Display` returns `View("Render", model)` for a type with no preview path
  (`OrchardCore.ContentPreview/Controllers/PreviewController.cs:180`). `Render.cshtml` sets no layout. The root view start
  `ThemeViewStart` gives every view the active theme's layout
  (`OrchardCore.DisplayManagement/Theming/ThemeViewStart.cs:24`), and the module's own `_Layout.cshtml` is used only by
  `Preview/Index.cshtml`, the window that holds the two iframes. The Article template of `TheBlogTheme` writes the title and
  the subtitle into the theme's "Header" zone, so they can appear only if the layout is the theme's.
- The documentation says the opposite: that the fallback is "a minimal HTML layout owned by the Content Preview module"
  that "doesn't load the active frontend theme's layout, resource zones, styles, or scripts" (reported in the doc issues).
- Confirm: in the preview window of `live-2`, the heading "Autumn Open Day" and the subtitle appear (the theme's masthead),
  and the page has the theme's header and footer. The narration says only that the window "shows the article", so it
  stays true either way, but the recording plan expects the heading and the subtitle on screen. If they do not appear,
  `live-2` and `live-4` have nothing to show and need another field (for example the body).

## 2. Which fields update the preview (`live-3`)

- Checked: the Title input (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:13`) and the text field input
  (`OrchardCore.ContentFields/Views/TextField.Edit.cshtml:22`) carry `content-preview-text`, and `contentpreview.edit.js`
  sends the form 500 ms after the last input event.
- Not narrated, so not asserted: the Wysiwyg body of the Article (`HtmlBodyPart`). `html-body-part.ts:32` dispatches the
  event, but no run of it was checked.
- Confirm: the preview shows the changed Title and Subtitle within a second or two of the last key, while the preview
  window is the background tab.

## 3. The "Preview Disconnected" message and reconnecting (`live-6`)

- Checked: the editor posts `disconnected` on `pagehide` (`contentpreview.edit.js:122`), so the alert appears when the
  editor page is left or closed, and its text is only "Preview Disconnected" (`preview-swap.ts:82`,
  `Preview/Index.cshtml:11`); the narration says the alert appears "because you left the editor". The reconnection is read
  from the messages: the editor posts `hello` when its page loads (`contentpreview.edit.js:52`), an open window answers
  `ready` (`preview-swap.ts:54-57`), the editor sends the form (`contentpreview.edit.js:43-44`), and the `token` message
  hides the alert (`preview-swap.ts:59-62`).
- Confirm: after **Cancel**, the alert is on screen in the preview window; and after opening Article 1 for editing again,
  without selecting **Preview**, the same window shows the page again and the alert disappears. The last sentence of
  `live-6` ("Opening the article in the editor again reconnects the window") depends on it; it was not run.

## 4. Preview Draft and View (`draft-2`, `draft-3`)

- Checked: **Preview Draft** loads the latest version (`ItemController.cs:44-69`, `VersionOptions.Latest`) and renders it
  with the item's display manager; **View** (`display-for`) opens the published item. **Preview Draft** appears only when
  `HasDraft()` is true (`ContentsButtonEdit_SummaryAdmin.cshtml:21`), which is also true for an item that was never
  published.
- Confirm: the draft tab shows the draft's Subtitle, the View tab shows the published Subtitle, and both open as new pages
  of the browser context (the recorder needs them to switch to).
- Checked, and narrated in `ways-2`: **Preview Draft** is not part of the Content Preview feature. The button is in
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:21-23`, the page in
  `OrchardCore.Contents/Controllers/ItemController.cs`, and neither refers to the feature. Confirm at S0 (feature
  disabled): the button is on Article 2 and opens its draft.

## 5. Enabling the feature (`enable-1`, `enable-2`)

- Checked: four setup recipes list the feature in their `feature` step (the Blog recipe, the agency recipe, the coming
  soon recipe and the blank recipe of `TheAdmin`), which is why "Some setup recipes, such as Blog" is narrated. No manifest
  depends on the feature, so it can be disabled and enabled alone.
- Confirm: the notification "The feature Content Preview was enabled." is still on screen at the end of `enable-2`, and
  the Features page lists the feature under "Content Management".

## 6. Permissions on a fresh Blog site (`who-1`)

- Checked: **Preview content** is implied by **Edit content for others** and **Publish or unpublish content for others**
  (`CommonPermissions.cs:33`), and the default permissions of the Administrator and Editor roles list it
  (`OrchardCore.Contents/Permissions.cs:44,59`). Whether the Editor role that the Blog recipe creates holds it was not
  checked: the recipe creates the role with an empty list of permissions (`blog.recipe.json`, the `Roles` step). The
  narration says "Roles that can Edit content for others, or Publish or unpublish content for others, include it", which
  follows from the implication and does not depend on the defaults.
- Not narrated, because it is unclear: the Author role has only **Preview own content** by default. The **Preview** button
  uses a resource check, which accepts the owner variation, but the draft and display endpoints check **Preview content**
  without a resource (`PreviewController.cs:58,137`). Confirm whether an Author sees the button but gets no live preview.
  If so, a sentence about it belongs in this video or in a video on roles.

## 7. The Preview part (`part-1`, `part-2`)

- Checked: the part is listed as "Preview" on the Add Parts page (the name without the suffix "Part",
  `ContentPartSettingsExtensions.cs:54-60`), and its only setting is **Pattern** (`PreviewPartSettings.Edit.cshtml:14`).
  The part's description on that page, set in `Migrations.cs:20`, says to use it "when running the frontend decoupled from
  the admin", while the code re-runs the request pipeline of the same tenant for the path (`PreviewStartupFilter.cs`), so
  it cannot reach another application (reported in the doc issues).
- The video describes the part in words and slides only: the setting is a Liquid expression, and an admin video shows no
  Liquid, so no clip configures it. Nothing in the programme shows how to write the Pattern.
- Confirm, on a site where Article has the Preview part and the pattern returns the item's own path: the preview window
  shows the full page, and an edit of the Title still shows (the path of an Autoroute item can be built from its title, so
  the changed path may not resolve to a route and the window could answer 404). If it does, the narration of `part-2` is
  still correct, but a limit belongs in the video.

## 8. The cache token of a preview (not narrated)

- Checked: the editor sends the field `PreviewToken` with the value of a variable that is undefined until the first answer
  (`contentpreview.edit.js:77`), the request body is built with `URLSearchParams`, which turns an undefined value into the
  text "undefined", and the controller keeps any non-empty token (`PreviewController.cs`, `existingToken`). So the first
  draft of every editor page seems to be cached under the token "undefined", one key for all editors of the tenant.
- Confirm, if wanted: the address of the frame inside the preview window ends in `token=undefined`. Nothing in the video
  depends on it, and one editor at a time works either way. It is also in the doc issues.

## 9. Not recorded, so not checked

- The five-minute expiry of a preview that nobody renews, the behavior behind several servers, and the validation errors of
  the editor (the window keeps the last page it could render) are in the documentation and match the code, but the video
  does not show them.
