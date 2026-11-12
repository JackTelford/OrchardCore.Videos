# Open questions: templates-preview

The narration says only what the source shows. These points come from reading the code, and need a person to confirm
them on a running site (the Blog recipe, the seed of `recording-plan.md`). Source paths are relative to
`src/OrchardCore.Modules/OrchardCore.Templates` of the Orchard Core checkout.

## Checks the recording depends on

1. **The blog home page does not use `Content__BlogPost` (`open-1`, `open-4`).**
   Checked: `Blog-ListPart.liquid` of TheBlogTheme builds each post with the display type `Summary`. The alternates of
   a content shape are, in order, `Content_[DisplayType]`, `Content__[ContentType]`, `Content__[Id]`,
   `Content_[DisplayType]__[ContentType]`, `Content_[DisplayType]__[Id]`
   (`src/OrchardCore.Modules/OrchardCore.Contents/ContentShapeAlternatesFactory.cs:45-52`), so `Content__BlogPost`
   applies to every display type of a blog post, not only `Detail`. The theme has `Content-BlogPost.Summary.liquid`, which
   `BasicShapeTemplateHarvester` names `content_summary__blogpost`; `GetShapeBindingAsync` of `DefaultHtmlDisplay.cs`
   tries the alternates from the last to the first, so that file is found before the saved `Content__BlogPost`.
   The narration therefore says "on this site" for the detail page and gives the theme's own summary template as the
   reason the home page does not change. To confirm: with the template saved, `/` looks the same as without it.
2. **A saved template is used instead of the theme's template file (`why-1`, seed check 2).**
   Checked: `DefaultHtmlDisplay.cs` (`GetShapeBindingAsync`, lines 349-412 of
   `src/OrchardCore/OrchardCore.DisplayManagement/Implementation`) asks every `IShapeBindingResolver` before the shape
   table for each alternate. To confirm: after the seed, `/blog/post-1` no longer shows the picture that the theme's
   `Content-BlogPost.liquid` draws at the top.
3. **Following a link in the preview selects the page of the next render (`any-1`, `any-3`).**
   Checked: `Assets/ts/template-preview.ts` sends `Handle: contentPreviewUrl` (line 34) and, in `iframe.onload`, copies
   `iframe.contentWindow.location.pathname` into `contentPreviewUrl` unless it equals the preview page's own address
   (lines 142-143). To confirm: after a click on the post link, the next change in the editor re-renders
   `/blog/post-1` and not the home page, and the preview does not jump back to the home page after the render.
4. **First render of the preview (`open-4`, `any-1` setup).**
   The comment at `Assets/ts/template-preview.ts:125-138` says the iframe may be written with the text "undefined"
   if its first load ends before the first render. To confirm: the preview tab shows the blog, never the word
   "undefined". If it does, reload the preview tab (the plan says how).
5. **"Preview Disconnected" after Save (`any-4`, `any-5`).**
   Checked: the editor sets and removes `OrchardCore.templates:not-connected` in an `unload` handler
   (`Assets/ts/template-edit.ts:59-62`), and the preview shows `#notConnectedWarning` on that event
   (`Assets/ts/template-preview.ts:84-88`). Browsers may skip `unload` for a page that goes into the back/forward
   cache. To confirm: the alert appears in the preview tab after **Save** in headless Chromium. If it does not, close
   the editor tab instead (`await editor.CloseAsync()`) in `any-4`, and say so in the narration of `any-4` and `any-5`.
6. **The edit at the start of the template (`any-2`).**
   The keys (`Control+Home`, `ArrowDown`, `End`, four `ArrowLeft`) and the click on `.template-editor .view-lines` are
   the plan's choice, not read from the source. To confirm: Monaco puts the caret just before `</p>` on line 2, the
   typed text appears without a suggestion list open at the end, and no character is added by auto-closing.
7. **Two cursors in the preview (`any-1`).**
   The recorder's cursor script is added to every frame (`scripts/recorder/Scene.cs`, `OverlayScript`), and the
   preview shows the page of the site in a frame. To confirm: one cursor and one click ripple on the post link. If the
   frame hides the cursor, highlight the link with `HighlightAsync` before the click.
8. **The words "Posted by" stay visible (seed).**
   TheBlogTheme's navigation bar can overlay the top of the content when the template has no masthead. The seed uses
   `padding-top: 7rem`. To confirm: the words "Posted by" are fully visible below the navigation bar, in the
   preview and on the site.
9. **The menu entry highlighted in `open-1`.**
   The plan highlights `a[href$='/Admin/Templates']` (first match). To confirm: the ring lands on the **Templates**
   entry of the **Design** menu and not on the breadcrumb. The label path comes from `AdminMenu.cs:19-20`.
10. **The spelling of names for the voice (`spoken` in `storyboard.json`).**
   The entries are new spellings, to confirm with `scripts/pronounce.py` and a listen before narrating: `Content__BlogPost`
   (spoken "Content double underscore Blog Post"), `PreviewTemplatesProvider` ("Preview Templates Provider"),
   `TemplatesShapeBindingResolver` ("Templates Shape Binding Resolver"), `Autoroute` ("Auto route") and `404` ("four oh four").
   The captions keep the written form.

## Behavior seen in the code that the video does not claim

- **The Add Template page.** The preview button is also on **Add Template** (`Views/Template/Create.cshtml`), but
  `Assets/ts/template-create.ts` calls `initializeTemplatePreview(textArea, "change")` of the shared package, whose
  built code (`wwwroot/Scripts/template-editors/template-create.js`) sends `Name: e.name`, which is the name attribute
  of the hidden Content text area (`Content`), not the Name field. The editor changes the hidden text area from code,
  which does not raise a `change` event. So the preview of a template that is not saved yet may not update while typing,
  and may use the name `Content`. To confirm: open **Add Template**, type a name and some Liquid, press **Preview**,
  and watch whether the page changes. The video uses **Edit Template** only; if the Add Template page is broken, it is
  a defect to report to the Orchard Core project, not a documentation issue.
- **Changing the Name field in the Edit Template page.** `Assets/ts/template-edit.ts` sends the name only when the
  content changes or when the preview tab asks (lines 32-56), so a change of the Name field alone does not refresh the
  preview. The narration does not say that the preview follows the Name field.
- **The home page when the home route is not a content item.** `PreviewController.cs:74-81` reads the content item id
  of the home route; a site whose home route is an action rather than a content item gets a 404 from the preview. The
  narration says only that the preview renders the home page or a published content item with an Autoroute address.

## Behavior seen in the code that the plan depends on

- **The look of "Preview Disconnected" (`any-5`).**
  Checked: `Views/Preview/Index.cshtml:12-21` gives the alerts Bootstrap classes, but `Views/Preview/_Layout.cshtml` has
  no stylesheet and `Index.cshtml` registers no style resource, so the alerts should be drawn without Bootstrap, as plain
  text at the top right (inline `position:fixed`). The narration only says that the preview shows Preview Disconnected.
  To confirm: what the alert looks like on screen, and that it is readable over the page of the post. If it is not,
  highlight it with `HighlightAsync` for the whole step.
- **Following a link in the preview frame (`any-1`).**
  Checked: the frame has no `sandbox` attribute (`Views/Preview/Index.cshtml:8`), and the source at this commit adds no
  `X-Frame-Options` header and no default `frame-ancestors` directive (the Security feature, which the Blog recipe
  enables, has an empty `ContentSecurityPolicy` by default: `SecurityHeaderDefaults.cs`). To confirm: the link opens
  `/blog/post-1` inside the frame, and no host setting blocks framing of the site.
- **The preview of a page that is not a content item.**
  The narration says that any other address shows Preview Error with a status code such as 404
  (`PreviewController.cs:92-101`, `Assets/ts/template-preview.ts:61-77`). To confirm: follow a link to a page of the site
  that has no Autoroute address (for example the login page, when the site is not signed in), edit the template, and see
  the alert. The video does not show this.
