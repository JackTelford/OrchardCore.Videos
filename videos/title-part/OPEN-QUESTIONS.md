# Open questions: Title part

What was checked in the source (Orchard Core at commit `92db4a83`) and what a person must confirm on a running site
while recording. None of these changes a claim of the narration that was verified in the source; they are about what
the screen shows.

## Confirm on the first take

1. **The row label in Add Parts.** The narration says "Title is one of them". The page lists the parts by
   `EditPartViewModel.DisplayName` (`OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:27-31`), which for the part
   stored as `TitlePart` is the name without the suffix "Part" (`ContentPartSettingsExtensions.DisplayName`). Confirm the
   row reads **Title** (not **Title Part**), and that its hint reads "Provides a Title for your content item."
   (`OrchardCore.Title/Migrations.cs:33`). If the row reads differently, change the narration of `attach-2`, `attach-3`
   and `gen-1`.
2. **Position of the Title field.** The part's default position of 0 (`Migrations.cs:34`) is only used to sort the Parts
   list of the type page (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:32`). The editor orders the parts by the
   position stored on the type, and a part without one counts as "before"
   (`OrchardCore.ContentManagement.Display/ContentDisplay/ContentItemDisplayCoordinator.cs:327`). The Product part has no
   stored position, and attaching the Title part stores "1" (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:143-148,481-522`),
   so by the code the editor shows Brand and Name first and the Title field last, while the Parts list of the type page
   lists Title above Product. Confirm the order of the fields in the editor on the first take. The narration only says
   the editor has a Title field, so it holds either way, and the highlight in `typed-1` works wherever the field is.
3. **Where the required error shows.** `TitlePartDisplayDriver.UpdateAsync` adds the error "A value is required for Title."
   under the key of the Title field (`Drivers/TitlePartDisplayDriver.cs:56`), and the page shows a validation summary at
   the top (`OrchardCore.Contents/Views/Admin/Create.cshtml:34`) and `asp-validation-for="Title"` next to the field
   (`Views/TitlePart.Edit.cshtml:15`). Confirm the message is visible in the frame at the end of `typed-2`, and in which
   place, so the highlight is on the right one.
4. **The generated titles.** The pattern is the one in the documentation's form (`ContentItem.Content.Product.<Field>.Text`).
   The code renders it with the content item as `ContentItem` (`Handlers/TitlePartHandler.cs:100-110`). Confirm that the
   list shows exactly `Brightwater Trail Backpack` (one space, no trailing space) and `Brightwater Water Bottle`. If the
   output differs, the pattern typed in `gen-2` is wrong, not the narration.
5. **The disabled field.** On a new item the Title field is disabled and empty (`TitlePart.Edit.cshtml:14`), and the hint
   "The title will be auto-generated using the pattern." shows only when the pattern is not empty. Confirm both in `gen-4`.
   After clearing the placeholder in `gen-3`, the disabled field must show no placeholder text.
6. **The hidden field.** With **Title is generated and input is hidden** the part's editor row is not rendered at all
   (`Views/ContentPart-TitlePart.Edit.cshtml:19-22`). Confirm no Title row shows in `hidden-2`, and that the item is still
   saved with its generated title (the form sends no title; the handler builds it, `TitlePartHandler.cs:92-110`).
7. **The site page.** TheBlogTheme's template for a generic content item prints the display text in the page header
   itself (`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:9`) and renders `Model.Content`, not the item's
   `Header` zone, where the Title part's own heading goes. The narration of `gen-7` only claims that the heading of the
   page shows the title, which that template does. Confirm the page heading on the first take. Because of this template,
   the **Render Title** option makes no visible difference in this theme, so the video never toggles it. The narration
   of `attach-3` only repeats what the setting says (the hint "Check to render the title in display views." and
   `TitlePartDisplayDriver.cs:24-35`, which adds the title to the Header zone of the Detail and Summary display types when
   the box is checked and the title is not blank), and does not claim that the page of `gen-7` shows that heading.
8. **The content items list.** `Contents/Views/Admin/List.cshtml` renders its rows with the admin list shape; the row
   locators in `gen-6` and `gen-7` assume `li` rows. Confirm the structure and the order of the rows (the newest item
   first), and adjust the locators in `Scenes.cs`, not the narration.
9. **The View button opens a new tab** (`target="_blank"`, `ContentsButtonEdit_SummaryAdmin.cshtml:18`). Confirm the
   recorder keeps capturing it with `SwitchToAsync`.
10. **The messages after a redirect.** `The "Title" part settings have been saved.`, `Your Product has been published.` are
    notifications of the page that follows the redirect. Confirm they are still visible at the end of the steps that
    mention them (`attach-5`, `typed-3`).
11. **Timing.** `attach-2`, `gen-1`, `gen-4`, `hidden-1` and `hidden-2` hold several page loads. Narrate first, then check
    in the contact sheets that the actions end before the narration does.
12. **The pattern editor.** The Pattern field is a CodeMirror editor with line numbers and bracket matching
    (`OrchardCore.Title/wwwroot/Scripts/title-part-settings/title-part-settings.js`, the `liquid-pattern-editor` module):
    confirm that typing the line of `gen-2` produces exactly that text, with no automatic closing characters (the script
    sets `lineNumbers`, `styleActiveLine` and `matchBrackets`, not `autoCloseBrackets`). The recorder's `TypeAsync`
    clicks the locator and then presses the keys on it (`scripts/recorder/Scene.cs:443-448`); the `.CodeMirror` wrapper is
    not an input, so if the keys do not land in the editor, click it and use `scene.Page.Keyboard.TypeAsync` instead.
13. **Pronunciation.** `Add & Configure` is spoken as "Add and Configure" through the storyboard's `spoken` map; confirm
    it with `scripts/pronounce.py` before narrating. Also listen to "Brightwater".
