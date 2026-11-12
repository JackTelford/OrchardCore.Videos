# Open questions: Create a Liquid Widget Without Writing a Module

What was checked in the source, and what a person must confirm on a running site (or with the voice) before the video
is recorded or published. Nothing here is stated as a fact in the narration unless it says "checked".

## On a running site

1. **Pasting into the two editors.** The Body of the widget is a CodeMirror editor (`OrchardCore.Liquid/Views/LiquidPart.Edit.cshtml`
   loads `codemirror`) and the Content of a template is a Monaco editor (`OrchardCore.Templates/Assets/ts/template-create.ts`).
   Their set-up code lives in the `@orchardcore/bloom` package (`content-preview-liquid-editor`, `helpers/monaco`), which is not
   in the source that was read. So these are unconfirmed:
   - that a synthetic `paste` event on `.CodeMirror textarea` and on `.template-editor textarea.inputarea` inserts the text
     verbatim, with no extra indent or closing characters (the method of "How to enter them" in `recording-plan.md`);
   - that `document.querySelector('.CodeMirror').CodeMirror.getValue()` returns the Body, and that
     `.template-editor-value` holds the template (`template-create.ts` copies the editor value into it on every change);
   - that the Body is saved with the text of the editor when **Publish** is clicked right after the paste.

   Typing is avoided on purpose: the Monaco language set-up of `OrchardCore.Liquid/Assets/monaco/liquid-intellisense.ts`
   opens a completion list after `{%` or `|` followed by a space, and Monaco may add the indent of the line above to a typed line.
2. **The Technical Name.** `autocomplete.ts` of the Content Types module fills **Technical Name** from **Display Name**
   with `getTechnicalName` (`@orchardcore/bloom/helpers/globals`, not in the source that was read). The video, the
   template name `Widget__LiquidWidget` and the plan assume the result is `LiquidWidget` for `Liquid Widget`. The plan
   checks the field before **Create** and types the value if it differs.
3. **The selectors of `recording-plan.md`.** They were written from the views and from `TheAdmin/Views/NavigationItem-admin.cshtml`
   and `NavigationItemLink-admin.cshtml`, not run. Confirm in particular: the admin menu groups (`button.nav-group-label`,
   `button.item-label`, `a.item-label`), the title, layer and body fields of the widget editor, the **Add Widget** menu in
   the **Footer** card, and `button.dropdown-toggle:text-is("Edit templates")`.
4. **What the front end looks like.** The Blog theme prints the **Footer** zone inside `<footer class="border-top">`
   (`TheBlogTheme/Views/Layout.liquid:44-46`). The new widget comes after the widget that the Blog recipe already puts in
   that zone, at position 11 (the recipe's widget is at position 10, and the **Add Widget** menu uses the highest position
   plus one: `OrchardCore.Layers/Views/Admin/Index.cshtml`). Confirm on the contact sheets that the centered paragraph,
   and later the centered heading, are visible, are not cut off by the footer, and that the page needs only a scroll to
   the bottom (the number of scrolls in `place-5` and `template-3` is a guess).
5. **The fonts on screen.** The layout of the Blog theme links two stylesheets from `fonts.googleapis.com`
   (`TheBlogTheme/Views/Layout.liquid:12-13`). The browser of the recorder loads them, not the site, so `HTTP_PROXY` and
   `HTTPS_PROXY` do not decide what the footer looks like. Confirm on the contact sheets that the footer is readable with or
   without those fonts, and that no request to another domain shows on screen.
6. **The "Add Selected Parts" button and the notifications.** The button is hidden until a part is selected
   (`Assets/js/add-parts-to.ts`, checked); the messages named in the plan come from the controllers (checked), but the
   exact moment they show on screen was not observed.

## With the voice

The `spoken` entries of `storyboard.json` are new spellings. `narrate.py` replaces them in the order they are listed, so
the longer names come first. A person must confirm each one with `scripts/pronounce.py` before narrating:

| Written | Spoken as |
|---|---|
| `Widget-LiquidWidget.liquid` | Widget, dash, Liquid Widget, dot liquid |
| `Widget-LiquidWidget.cshtml` | Widget, dash, Liquid Widget, dot C.S.H.T.M.L. |
| `Widget__LiquidWidget` | Widget, two underscores, Liquid Widget |
| `Widget.cshtml` | Widget dot C.S.H.T.M.L. |
| `Model.ContentItem` | Model dot Content Item |
| `Model.Classes` | Model dot Classes |
| `Model.Content` | Model dot Content |
| `shape_render` | shape underscore render |
| `DisplayAsync` | Display Async |

Also listen to "widget-body", "widget-container", "text-center" and "Bootstrap" in the narration: they are left as
written.

## Checked in the source

- Labels: every label in the narration and in `recording-plan.md` was read in the views, menus and controllers named in
  the plan. The part is listed as **Liquid** because the display name drops the "Part" ending
  (`ContentPartSettingsExtensions.cs:54-64`).
- A type created in the admin starts with **Creatable**, **Draftable**, **Versionable**, **Listable** and **Securable** set
  (`ContentDefinitionService.cs:84-91`).
- The alternates of a widget are `Stereotype`, `Widget` and `Widget__<technical name>`, and the last one that has a
  template wins (`ContentItemAlternatesFactory.cs`, `DefaultHtmlDisplay.cs:349-362`).
- A layer adds the classes `widget` and `widget-<type>`; only the Flow part adds `widget-align-*` and `widget-size-*`
  (`LayerFilter.cs:139-140`, `FlowPart.cshtml:36-39`).
- Editing the Liquid part, and the **Templates** pages, require `ManageLiquidTemplates`, labeled "Manage Liquid templates",
  which the Administrator and Editor roles have by default (`OrchardCore.Liquid.Abstractions/Permissions.cs`).
- The row order in the **Footer** card (`place-4`): the **Layers** page orders the widgets by `Position`
  (`OrchardCore.Layers/Controllers/AdminController.cs`, the `Index` action), and **Add Widget** gives the new widget the
  highest position of the zone plus one (`OrchardCore.Layers/Views/Admin/Index.cshtml:94`), so **Opening hours** (11) comes
  after the widget named **Footer** (10, from the Blog recipe).
- The part named after the type (`type-4`) reads **Liquid Widget**, the display name of the type
  (`OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs:7-25`).
- Publishing a widget refreshes the layer state that the front end checks (`OrchardCore.Layers/Handlers/LayerMetadataHandler.cs`),
  and the saved templates are read for each request (`OrchardCore.Templates/Services/TemplatesShapeBindingResolver.cs`), so
  neither the widget nor the template needs a restart; the templates are not applied in the admin, only on the front end.
- The widget editor is rendered through `Content_Edit`, which draws the **Publish** button
  (`OrchardCore.Widgets/Views/Widget.Edit.cshtml`, `OrchardCore.Contents/Drivers/ContentsDriver.cs`, `Content_PublishButton`).
- The **Edit templates** menu (`template-4`): the detail entry has the key `<stereotype>__<technical name>`, which is `Widget__LiquidWidget`
  for this type, so it opens the template of `template-2` (`OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:44-60`,
  `Views/TemplateSettings.cshtml`).
