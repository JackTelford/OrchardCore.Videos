# Open questions: layers-zones

Everything the narration states was read in the Orchard Core source. These points could not be settled from the source
alone: a person confirms them on the running site before recording, and says so in the pull request.

## Confirm on the running site

1. **Where the paragraph sits in the Content zone of a blog post** (`site-1`). `LayerFilter` adds the widget to the
   zone `Content` before the post is rendered (`OrchardCore.Modules/OrchardCore.Layers/Services/LayerFilter.cs:157`),
   and the Blog theme draws that zone with `render_body` (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:40`),
   which renders `Zones["Content"]` (`OrchardCore/OrchardCore.DisplayManagement.Liquid/Tags/RenderBodyTag.cs:17`). The
   body of the page is added to that zone afterwards (`OrchardCore/OrchardCore.DisplayManagement/Theming/ThemeLayout.cs:25`),
   but the source does not settle whether the paragraph is drawn before or after the body of the post. The narration
   says only that it appears in the Content zone. If it sits where the recorder cannot show it with one scroll, say so in the pull
   request; the plan scrolls until `article.widget-paragraph` is in view.
2. **The drag in `move-1`.** The rows are sortable with Sortable.js
   (`OrchardCore.Modules/OrchardCore.Layers/Assets/ts/layers-index.ts:9`), which listens for native drag events. The
   recorder's `DragToAsync` presses the mouse, moves, and releases; the source says it works with HTML5 drag and drop
   but this has not been run on the Layers page. Check that the row lands above the Footer widget, that the message
   "Widget successfully moved. Undo." appears, and that Undo (`move-2`) puts the widget back in the Content zone (the Undo
   link is a `javascript:` link that submits the form `#undo-form`, `Views/Admin/Index.cshtml:25`; confirm that the click
   loads the page again and that nothing blocks the link). If the
   drag does not work, chapter 07 (steps `move-1` and `move-2`) cannot be recorded as written, and a person decides
   whether to drop the chapter; the rest of the video does not depend on it.
3. **The admin menu behaviour.** The markup is read from `NavigationItem-admin.cshtml` and
   `NavigationItemLink-admin.cshtml`: a first-level item with children and no page of its own is a `button` with the
   class `nav-group-label`, which the plan clicks (`button.nav-group-label:has-text("Design")` and
   `...("Settings")`). The menu was not run: confirm that opening a group makes the links
   `a[href$="/Admin/Layers"]` and `a[href$="/Admin/Settings/zones"]` visible, and whether it collapses a group that is
   already open. The admin prefix is assumed to be `Admin`, the default (`blog.recipe.json` reads
   `Admin:AdminUrlPrefix` with `Admin` as default).
4. **The editor of the Paragraph widget.** The field uses the WYSIWYG editor, which is Trumbowyg
   (`OrchardCore.Modules/OrchardCore.ContentFields/Assets/ts/html-field-wysiwyg.ts`). The class `.trumbowyg-editor` of
   the editable area comes from that library and was not read in this source: confirm the selector, and that typing in
   it with `PressSequentiallyAsync` is accepted.
5. **The names of the form controls.** The plan uses `input[name="LayerMetadata.Title"]`, `select[name$=".Layer"]`,
   `input[type="checkbox"][name$="RenderTitle"]`, `select[name$="SelectedOperation"]` and `input[name$="Value"]`. They
   are derived from the view models and from `LayerMetadataWelder` (prefix `LayerMetadata`); the rendered names were not
   inspected. If one differs, use the label text instead (Title, Render title, The layer, Operation, Value).
6. **The page the recording starts on (`/Admin`).** Nothing in the Blog recipe enables the admin dashboard feature, and
   `/Admin` renders the built-in welcome page (`OrchardCore.Modules/OrchardCore.Admin/Controllers/AdminController.cs`,
   `Views/Admin/Index.cshtml`, `Views/AdminDashboard.cshtml`: "Welcome to Orchard Core" and a Documentation link). Confirm
   that it shows the admin menu and no content that would be recorded by accident; if not, start `02-page` on
   `/Admin/Contents/ContentItems`.
7. **The Settings page around the field.** The view of the field has no heading of its own
   (`Views/LayerSettings.Edit.cshtml`), and the page title is "Settings"
   (`OrchardCore.Modules/OrchardCore.Settings/Views/Admin/Index.cshtml:4`). Confirm that no other group's content is
   shown on that page in the recording.
8. **The request path of a blog post.** `UrlConditionEvaluator` compares `Request.Path` with the value. The Blog
   recipe gives the post the path `blog/post-1`. Confirm that `/blog/post-1` is served at that address (not redirected
   to another address), since the narration says the address starts with `/blog/`.
9. **How long the toast stays.** Success messages stay for 5 seconds
   (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). The plan times the click on Save in
   `zones-3` so that the message is visible at the end of the step; confirm on the contact sheet.

## Pronunciation (a person confirms with `scripts/pronounce.py`)

`storyboard.json` has two entries in `spoken`: `Url` is said as "U.R.L." (the label on screen is "Url condition" and
"Url"), and `/blog/` is said as "slash blog slash". Neither has been checked with the voice. If "Url" is already read
correctly, remove the entry; if "U.R.L." is read with pauses, try another spelling.

## Not covered, on purpose

- The condition types other than Url, the JavaScript condition, and the All and Any groups are only named. A separate
  video covers them.
- The permission "Manage layers" (`OrchardCore.Modules/OrchardCore.Layers/Permissions.cs`) is not narrated. Administrators
  have it by default.
