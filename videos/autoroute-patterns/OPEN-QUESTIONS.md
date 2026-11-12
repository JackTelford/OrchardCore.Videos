# Open questions: Autoroute patterns

What the author read in the source and could not run. A person confirms each point on a running site before or while
recording, and changes the narration if the screen disagrees. Source paths are relative to the root of
`OrchardCMS/OrchardCore` at commit `92db4a837d`.

## To check on screen while recording

1. **`edit-4`: which input makes Save refuse the pattern.** The message text is in the source
   (`src/OrchardCore.Modules/OrchardCore.Autoroute/Settings/AutoroutePartSettingsDisplayDriver.cs` line 60: "Pattern
   doesn't contain a valid Liquid expression. Details: {0}"), and it comes from `ILiquidTemplateManager.Validate`. The
   source does not say whether Fluid refuses an output tag without its closing braces. The plan removes them; the fallback is an
   `if` tag that is never closed. The narration says only that a pattern with a syntax error is refused and that the
   message begins with the quoted words, so it holds for either input. Note which input worked.
2. **`edit-3`: a line break in the pattern.** `AutoroutePartHandler.GenerateContainerPathFromPatternAsync` removes line
   endings from the result (`ReplaceLineEndings(string.Empty)`, line 428), so the two-line pattern should give
   `trail-mug-forest-green` with no break. Confirm the Permalink of `result-2`.
3. **The Pattern editor.** `Assets/ts/autoroute-part-settings.ts` hands the textarea to `initLiquidPatternEditor`, and the
   built `wwwroot/Scripts/autoroute-editors/autoroute-part-settings.js` makes it a CodeMirror editor (line numbers, Liquid
   mode, no automatic closing of brackets). Confirm that clicking a line, `Control+End`, `Enter` and typing work as the plan
   says, and that the editor accepts the second line.
4. **`result-3`: the number appended to a taken path.** By the code (`AutoroutePartHandler.cs` lines 456 to 488) the
   first free version is 1, so the second product gets `trail-mug-forest-green-1`. Confirm the value on screen.
5. **`result-4`: Refresh the path.** The seed turns on Allow path updates, which shows the checkbox
   (`Views/AutoroutePart.Edit.cshtml` line 87). Confirm the Permalink becomes `trail-mug-slate-blue` after Publish.
6. **The name of the part in the Parts list.** The row is drawn with `ContentTypePartDefinition.DisplayName()`
   (`OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs` line 7), which falls back to the
   part definition's `DisplayName()` and strips the `Part` suffix when no display name is set
   (`.../Metadata/Settings/ContentPartSettingsExtensions.cs` line 54; the Autoroute migration sets only a description,
   `OrchardCore.Autoroute/Migrations.cs` line 21). The narration says "Autoroute part", and the success notification should
   read `The "Autoroute" part settings have been saved.` Confirm both labels.
7. **`container-1`: the pattern is wider than its editor.** Confirm that the line is readable enough with the ring, or
   scroll the editor a little. Do not save the Blog Post type.
8. **`container-2`: the path to the Blog page.** The admin menu entry is labeled `Blog` in the recipe
   (`blog.recipe.json` line 1197) and the button is `Create Blog Post` (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml`
   line 52). Confirm both on the site, and that the split Publish button offers `and continue`
   (`OrchardCore.Contents/Views/Content.PublishButton.cshtml` line 24).
9. **`container-3`: a post created outside the blog.** The sentence comes from `OrchardCore.Lists/Liquid/ContainerFilter.cs`
   (it returns the content item when there is no `ContainedPart`) and from the editor, which adds a `ContainedPart` only
   when the page is opened with a container in the query string (`OrchardCore.Lists/Drivers/ContainedPartDisplayDriver.cs`).
   It is not shown. To confirm it, open `/Admin/Contents/ContentTypes/BlogPost/Create` directly (the Blog Post type of the
   recipe is not Creatable, `blog.recipe.json` lines 188 to 195, so Content > Content Items > New does not list it), give
   it a title, publish, and read its Permalink: it should be the post title, a slash, and the post title again. If it is
   not, remove the sentence.
10. **`edit-5`: the editor after a refused pattern.** The Pattern textarea is bound with `asp-for="Pattern"`
    (`Views/AutoroutePartSettings.Edit.cshtml` line 16), so the page drawn again after `edit-4` should hold the posted text
    (the settings driver returns `Edit(...)` without storing it, `Settings/AutoroutePartSettingsDisplayDriver.cs` lines 58
    to 77). Confirm that the editor still shows the two lines. If it shows the stored line only, the plan types line 2 again.
11. **`container-2`: Publish and continue.** The item **and continue** of the split button
    (`Content.PublishButton.cshtml` line 24) keeps the editor open after the post is published; confirm that the Edit page of the
    post opens with the Permalink filled in.

## Not verified, and left out of the narration

- **A pattern that produces an invalid character.** The narration says a permalink cannot contain spaces, two slashes
  in a row, or characters such as `?`, `:` and `.` (`src/OrchardCore/OrchardCore.Autoroute.Core/Model/AutoroutePart.cs`
  line 9, `AutoroutePartExtensions.cs`). The editor validates the path an editor types
  (`Drivers/AutoroutePartDisplayDriver.cs` line 116), and `AutoroutePartHandler.ValidatingAsync` validates it when content
  is validated (the API, workflows and scripts call `IContentManager.ValidateAsync`). The admin content editor does not
  call it (`OrchardCore.Contents/Controllers/AdminController.cs` lines 769 to 807 and 819 to 850). What happens to a
  path that a pattern generates with such a character is therefore not claimed.
- **The order of the parts of a type.** Handlers run in the order of the parts of the type
  (`ContentPartHandlerCoordinator.InvokeHandlers`), and the order of that list was not traced. The editor sets the title
  before the content item is created (`OrchardCore.Title/Drivers/TitlePartDisplayDriver.cs` line 60), so a pattern that
  uses the display text works there. The video makes no claim about creating content through an API or an import.
- **Date-based patterns** (`CreatedUtc`, `PublishedUtc`): when those values are set relative to the Autoroute handler was
  not traced, so no such pattern is shown.
- **`transliterate`.** The filter exists (`OrchardCore.Localization/Startup.cs` line 41) but belongs to the Localization
  module, whose feature the Blog recipe does not enable. It is not in the video.

## New spellings for the voice

The `spoken` map of `storyboard.json` has new entries. `scripts/pronounce.py` was not run (it writes audio). A person
checks each one with it before narrating:

| Written | Spoken as |
|---|---|
| `ContentItem` | Content Item |
| `Model.ContentItem` | Model dot Content Item |
| `ContentItem.Content.Product.Color.Text` | Content Item dot Content dot Product dot Color dot Text |
| `Content.Slug` | Content dot Slug |
| `display_text` | display text |
| `AutoroutePartSettings` | Autoroute Part Settings |
| `AlterTypeDefinitionAsync` | Alter Type Definition Async |
| `GetContentItemBySlugAsync` | Get Content Item By Slug Async |
| `ContentDefinition` | Content Definition |
| `WithSettings` | With Settings |
| `trail-mug-forest-green-1` | trail mug forest green dash 1 |
| `blog/notes-from-the-orchard` | blog slash notes from the orchard |

Also check how the voice says `slugify` and `slugified` (no entry yet), and `Permalink`.
