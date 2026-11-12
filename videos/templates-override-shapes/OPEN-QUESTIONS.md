# Open questions: Templates: Override Shapes From the Admin With Liquid

The narration states only what the Orchard Core source and documentation show. Nothing below was run on a live site.
Each item says what was checked in the source and what a person must confirm before recording.

## Pronunciation (`spoken` in `storyboard.json`)

New spellings, to be checked with `scripts/pronounce.py` before narrating:

- `Content_Summary__BlogPost` is spoken "Content underscore Summary, double underscore, Blog Post".
- `Content_Summary` is spoken "Content underscore Summary".
- `Content__BlogPost` is spoken "Content, double underscore, Blog Post".
- `Model.ContentItem` is spoken "Model dot Content Item", `Model.Content` "Model dot Content".
- `shape_render` is spoken "shape render", `shape_remove_item` "shape remove item".
- `DisplayText` is spoken "Display Text", `ZoneName` "Zone Name".

The spoken forms are longer than the written ones, so check the length of `create-3`, `shortcuts-2`, `idea-2`,
`inside-1` and `inside-2` after narrating, since the recorder holds each step for the length of its audio.

Also check with `scripts/pronounce.py` that the voice reads the noun "alternates" (`idea-1`, `idea-2`, `idea-3`,
`create-3`) as "all-ter-nuts" and not as the verb "all-ter-nates", and add a `spoken` entry if it does not.

## Claims to confirm on a running site

1. **A template wins over a view file with the same name** (`idea-3`, `create-8`). Source:
   `OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs`, `GetShapeBindingAsync`: the alternates are
   walked from the last one added to the first, and for each one the shape binding resolvers (the Templates module)
   are asked before the shape table (the views of the theme and the modules). Confirm in `create-8` that the blog page
   shows the template's output, and with shape debug information on that the start comment names
   `Templates/Content_Summary__BlogPost.liquid`.
2. **Saving a template needs no build and the next request uses it** (`idea-4`, `create-8`). Source: the templates are
   a document of the site (`TemplatesManager`, `TemplatesDocument`), read through `IDocumentManager`. The Blog recipe
   enables Dynamic Cache. Confirm that `/blog` shows the new format on the first load after saving. If it does not,
   reword `idea-4` and `create-8`.
3. **The demo template** (`recording-plan.md`, slide `inside-1`) is made of expressions found in the documentation
   (`display_url`, `Model.ContentItem.DisplayText`, `Model.ContentItem.Author`) and in the Liquid views of
   TheBlogTheme (`Model.ContentItem.Content.BlogPost.Subtitle.Text`, the classes `post-preview`, `post-title`,
   `post-subtitle`, `post-meta`). Confirm that it renders "Written by admin", the linked title and the subtitle, and
   that the classes give a readable result. If the text changes, change the plan and `slides.py` together.
4. **Roles that have the two permissions by default** (`idea-5`). Source: the permission stereotypes of
   `OrchardCore.Templates/Permissions.cs` and `OrchardCore.Liquid.Abstractions/Permissions.cs` (Administrator and
   Editor), and the Liquid documentation. The Blog recipe creates the Editor role with an empty list of permissions;
   confirm on the Roles page of a Blog-recipe site that Editor has **Manage templates** and **Manage Liquid templates**.
5. **A template named like the shape type** (`debug-3`). Source: `DefaultHtmlDisplay.GetShapeBindingAsync` tries the
   shape type itself only when no alternate has a binding. Confirm with a content type that the theme has no summary
   view for (Page, for instance) if the claim is to be shown; the video only states it. The narration says "in
   summary views" on purpose: `Content_Summary` is the shape type of a content type without a stereotype
   (`ContentItemDisplayManager.BuildDisplayAsync`), so a widget in a summary view is `Widget_Summary` and is not
   answered by it.
6. **The debug comment** (slide `debug-2`). Source: `DefaultHtmlDisplay.CreateStartComment` and `CreateEndComment`
   (note the space before `-->` in the end comment; the DisplayManagement documentation shows none) and
   `TemplatesShapeBindingResolver.BuildShapeBinding` (`Templates/{shapeType}.liquid`). Confirm in the page source of
   `/blog` that the line of a post is exactly as the slide shows it, and look at what the line shows for a view file of
   the theme (the narration only says "it names that view"). `ShapeDebugInfoHtmlContent.WriteTo` writes the two comments
   directly against the HTML of the shape, with no line break, so the slide, which puts them on separate lines, is a
   layout choice; the binding name of a view file of the theme is written in lower case by
   `BasicShapeTemplateHarvester.Adjust`, and the narration does not quote it.
7. **The page after a failed save** (`create-4`): the error "The content is mandatory." and the warning about
   Placements are expected on the same page (a failed POST returns the view, and `NotifyFilter` writes the messages in
   a view result). Confirm both are visible, and that **Name** and **Description** keep their text.
8. **The labels of the menu path** (`create-2`, `shortcuts-1`, `debug-1`): "Design, Templates", "Design, Content
   Definition, Content Types" and "Settings, Debugging" come from the admin menu builders. The plan opens the pages
   by URL (and the Content Types list is the page `05-shortcuts` starts on). Confirm that the admin menu on screen
   shows those entries on those pages. The narration names the path; it does not ask the viewer to see a click.
9. **Seed labels** (`recording-plan.md`): "List Blog Post Items", "Create Blog Post", "Title", "Publish" come from the
   Lists, Title and Contents views; "Subtitle" is the display name of the field in the Blog recipe. Confirm that a post
   with an empty body can be published, and that the new posts are listed above the recipe's post.
10. **Selectors** (`#Name`, `#Description`, `#Content_editor`, `#Content`, `button.save`, `a.create`, `a.cancel`,
    `a.action-bar-secondary`, `#search-box`, `a.btn-danger`, `#modalOkButton`, `button.primaryAction.save`) were read
    from the Razor views and `TheAdmin.ts`, not run. Check each with `Recorder probe` once.
11. **The editor** (`create-5`): `window.__orchardCoreMonacoReady` and `monaco.editor.getModels()[0]` come from
    `OrchardCore.Resources/Assets/monaco/monaco-loader.ts` and `template-create.ts`. Confirm that setting the model's
    value fills the hidden field and that the editor shows the text. The narration says "a code editor" and does not
    claim syntax colors or suggestions: the source registers an HTML language service and a completion provider for
    the language "liquid" (`liquid-intellisense.ts`), but the colors come from the Monaco package, which was not read.
12. **The delete dialog** (`manage-3`): title "Delete", message "Are you sure you want to remove this element?" and the
    button "Ok" come from `TheAdmin/Views/Layout.cshtml:99`. Confirm on screen.
13. **Notifications fade**: `create-7` and `manage-3` may end after the message has gone; the list is the evidence.

14. **The Preview button of the Create form** (`create-6`). The narration only says that the button opens a page for
    previewing templates (`Create.cshtml:69`, `PreviewController.Index`). The source suggests the Create form may not
    send the typed template to that page: `template-create.ts` calls `initializeTemplatePreview(textArea, "change")`,
    and the compiled `template-preview-sync` sends `Name` as `textArea.name` (the string "Content", not the value of
    the Name field) when the hidden textarea fires `change`, which a Monaco edit does not trigger. `template-edit.ts`
    sends the Name field and the editor content on every edit. Confirm on a running site, for the video about
    previewing templates, whether Preview shows an unsaved template from Create and from Edit.

## Not covered on purpose

- The bulk delete of the list (`TemplateController.cs:112`, `:340-357`): the label of its menu was not read on a
  screen, so the narration does not name it.
- Templates for parts and fields in the content part and type-part editors have the same **Edit templates** button
  (`TemplateContentPartDefinitionDriver.cs`, `TemplateContentTypePartDefinitionDriver.cs`); they are not recorded.
