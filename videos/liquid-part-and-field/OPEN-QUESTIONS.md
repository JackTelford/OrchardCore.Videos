# Open questions: Liquid Part and Liquid Field

The narration states only what the source of `OrchardCMS/OrchardCore` (commit `92db4a83`) and its documentation say.
These points were not run on a site, so a person confirms them on the recording site before the clips are recorded.
Nothing here is narrated as more than the sentence quoted. Source paths follow the convention at the top of
`recording-plan.md`.

## 1. The label and the description of the part in Add Parts (`part-2`)

Narration: "The part is listed as Liquid, with the description: Provides a Liquid formatted body for your content item."

Checked in the source: the part is defined by `OrchardCore.Liquid/Migrations.cs:18-20` with that description and no
display name, so the Add Parts list shows the name without "Part"
(`OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:27`, `OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54`).

To confirm on the site: open `/Admin/ContentTypes/AddPartsTo/Notice`; the row is labeled "Liquid" (not "Liquid Part")
and carries that sentence. If the row reads differently, change `part-1` and `part-2` of the storyboard.

## 2. The syntax error of `part-5`, and what the form holds afterwards

Narration: "Add an if tag with no end tag and choose Publish: the item is not saved, and the message begins, The Liquid
Body doesn't contain a valid Liquid expression."

Checked in the source: `LiquidPartDisplayDriver.UpdateAsync` parses the template with `ILiquidTemplateManager.Validate`
and adds that message to the model state when the parse fails (`OrchardCore.Liquid/Drivers/LiquidPartDisplayDriver.cs:65-67`);
the content controller then returns the form without saving (`OrchardCore.Contents/Controllers/AdminController.cs:789`),
and the form shows a validation summary (`OrchardCore.Contents/Views/Admin/Create.cshtml`).

Not checked: (a) that the Fluid parser rejects a template that ends with an open `{% if true %}` (it should, since the
block has no `endif`); if it accepts it, use another unclosed construct, for example an output tag without its closing
braces, and update `recording-plan.md` and nothing else; (b) the text after "Details:"; (c) whether the Body editor and
the Title field come back with what was typed. The driver puts the previous value in the view model, but the form
helpers normally show the posted value. `recording-plan.md` gives both ways for `part-6`.

## 3. The page of `part-6` and `field-4`

Narration (`part-6`): "The page shows the title in the header, and below it the output of the template, with the site
name and the date." Narration (`field-4`): "The output of the field is on the page too: a link to the content item."

Checked in the source: TheBlogTheme draws the title in its own header and renders the whole Content zone
(`OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:9,17`); the Liquid part is placed in that zone, and the field
shape in the zone of the part named Notice.

To confirm on the site: both outputs are visible on `/Contents/ContentItems/<id>`, in which order, and that
`date: "%B %d, %Y"` prints a long month name, the day and the year (for example "November 12"). If the date does not
render, replace the filter argument in the typed template and in `recording-plan.md`; the narration does not name the
format.

## 4. Names the pages generate

- The Technical Name `ClosingNote` that follows the Display Name `Closing Note` (`field-1`), produced by the script
  `OrchardCore.ContentTypes/Assets/js/autocomplete.ts`.
- The radio label "Liquid Field" on the Add Field page: it is the field type name split into words
  (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:62`).
- The words "Liquid Field" next to the field in the Fields list (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:115`).

## 5. Statements taken from the source and the documentation, not run

- The HTML body part, the HTML field, and the Markdown body part and field do not run Liquid
  (`docs/reference/modules/Liquid/README.md:15`; the migrations of `OrchardCore.Html` and `OrchardCore.Markdown` log a
  warning about it). To confirm: type `{{ Site.SiteName }}` in an HTML body, publish, and see the text unchanged.
- A user without the Manage Liquid templates permission cannot edit or publish an item that has a Liquid part or field,
  nested ones included (`OrchardCore.Liquid/Security/LiquidContentAuthorizationHandler.cs`). To confirm: sign in as a
  user whose role lacks the permission, and open "Open House".
- A feed reads the body aspect that the Liquid part provides (`OrchardCore.Contents/Feeds/CommonFeedItemBuilder.cs:25`).
- With GraphQL enabled, a Liquid field has a field named `liquid` that returns the rendered result
  (`OrchardCore.Liquid/GraphQL/LiquidFieldQueryObjectType.cs:23`).

## 6. Pronunciation

The storyboard's `spoken` entries (`LiquidPart_Summary`, `LiquidPart`, `LiquidField`, `field-type-liquidfield`,
`ContentItem`, `WithPart`, `OfType`, `Html`) have not been checked with the voice. A person confirms them with `scripts/pronounce.py`, and
listens to "GraphQL", "URL" and "Markdown" as the voice says them, before narrating the video. `narrate.py` applies the
entries as plain text replacements in the order of the file, so `LiquidPart_Summary` has to stay above `LiquidPart`.
