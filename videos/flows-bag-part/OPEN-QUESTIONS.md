# Open questions: Bag Part, Embed Content Items Inside a Content Item

Everything in the narration comes from the source of Orchard Core at commit `92db4a83` and from the documentation pages
of the tracker entry. No site was run for this video. The points below are the ones that only a running site can settle.
Each says what was checked in the source and what a person must confirm before recording or publishing. Paths are
relative to the Orchard Core checkout.

## 1. How the bag and its items look on the site

Checked: `src/OrchardCore.Modules/OrchardCore.Flows/Views/BagPart.cshtml` renders each item with the display type
Summary when the setting is empty (line 9). The shape of an item is `Content`, drawn by
`src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.Summary.cshtml`. The Title part draws
`<h2><a display-for=...>` in the Header zone (`OrchardCore.Title/Views/TitlePart.Summary.cshtml`), and a Text field
draws a `div` with its text (`OrchardCore.ContentFields/Views/TextField.cshtml`). The page itself uses
`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`, which puts the page title in the masthead and renders the
Content zone below it.

To confirm on a running site, at the end of `view-1`:

- The title of the page is the masthead heading and the two items appear below it, Tip first, each with its title and
  its text (the Answer or the Details).
- No masthead is repeated for the items. If one is, the setting **Display Type** holds a value (the Detail display type
  would trigger the theme's template for each item).
- The narration says only "the site shows the items of the bag in the order of the editor". It names no heading level,
  no link, and no style, so it holds whatever the theme draws.

## 2. The link on each item's title

`TitlePart.Summary.cshtml` renders the title as a link to the content item. An item inside a bag is part of its parent's
document and has no record of its own. Not verified: where that link leads (a page that shows the item, or an error).
The recording plan forbids clicking a title in the capture. A person should open one link once, outside the capture, and
write the result here. The narration says nothing about it.

## 3. The seed recipe

The two content types (**Question** and **Tip**) are loaded from the recipe in `recording-plan.md` with **JSON Import**.
It follows the shape of the content types in `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json` but was not
imported. If it fails, the plan lists the manual way to build the same two types, including the settings to untick
because a type made in the admin is Creatable, Listable and Securable and has a thumbnail
(`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs` lines 81 to 91). The narration depends on these facts
about the types: each has a **Title** and one Text field (**Answer** for Question, **Details** for Tip), each has a
description (shown on the cards of the Blocks dialog in `blocks-3`), and each has a display name equal to its technical
name (see item 4). To confirm: that the import succeeds, that the **New** list then holds four entries
(Article, Blog, Help Page, Page), and that neither type is in the Content Items list.

## 4. Dragging a card

Checked: the bag is a sortable list whose handle is `.widget-editor-handle`
(`src/OrchardCore.Modules/OrchardCore.Flows/wwwroot/Scripts/flows/flows.edit.js`, the `Sortable.create` call). The source
of the helper that draws it (`@orchardcore/bloom/components/sortable-widgets`) is an npm package that is not in the
checkout, so only the compiled call could be read. To confirm: that `DragToAsync` from the handle of the Tip card to the
header of the Question card reorders the cards in the capture, and that the order posted on **Publish** is the order of
the cards on screen (the driver builds the list from `Prefixes` in request order:
`OrchardCore.Flows/Drivers/BagPartDisplayDriver.cs` lines 107 to 188). If the drag does not work with the recorder, `manage-2`
and `view-1` cannot say "Tip first", and both narrations must change before recording.

Two points about the order, read in the source and not run:

- The driver collects the items in a dictionary in the order of the posted `Prefixes` and then takes its values
  (`BagPartDisplayDriver.cs` lines 103, 158 and 188). A comment there reads "TODO, some how here contentItems should be
  sorted by a defined order", and the documentation of `Dictionary` does not promise an enumeration order. In practice
  a dictionary that only receives additions lists its values in insertion order, which is what the video relies on. To
  confirm: open **Account Help** again after `manage-4` and check that the Tip is still first, in the editor and on the
  site.
- The Standard editor refuses a move whose card type is not in the list of the bag. That list is built by joining the
  type definitions (`OrchardCore.Flows/Views/BagPart.Edit.cshtml` line 14), which writes their display names, and the
  check compares it with the technical name of the card (`Assets/ts/flows.edit.ts` lines 238 to 242). The seeded types
  have the same display and technical name, so the move should be accepted. For a type whose display name differs
  from its technical name, such as **Blog Post** and `BlogPost`, the move would be refused. The Blocks editor joins
  the names (`BagPart-Blocks.Edit.cshtml` line 14). This looks like a defect of Orchard Core, to report to the team; the
  video does not depend on it as long as the seed keeps the names equal.

## 5. The Technical Name of the page type and of the bag

The name fields fill themselves from the display name when a key is released or the field loses focus
(`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`). Not verified: the exact names produced for `Help Page` and for
`Help Items`. Nothing in the narration depends on them. The recording plan reaches the type from the Content Types list.

## 6. The text of the green messages

The messages after **Add Selected Parts** and **Save & Configure** are built with `H.Plural` and with the technical part
name (`OrchardCore.ContentTypes/Controllers/AdminController.cs` lines 372 and 453), so they read `The "TitlePart" part has
been added.` and `The "BagPart" part has been added.` and not the display names. The narration does not read any of
them. If a take ends while one is on screen, that is expected.

## 7. Stereotype mode and Collapse Contained Items

Observation, not used in the narration. With the source set to **Stereotype**, saving the settings does not copy
**Collapse Contained Items** into the stored settings (`OrchardCore.Flows/Settings/BagPartSettingsDisplayDriver.cs`,
`SetStereoTypes`, lines 85 to 90 and 102 to 108: only the content-types branch sets `CollapseContainedItems`). A bag
configured by stereotype therefore always loads its items expanded. `add-5` mentions **Stereotype** only as a choice
that the video does not use, and `add-6` ticks **Collapse Contained Items** with the **Content Types** source. A person
should confirm the behavior on a site and, if it is unintended, report it to the Orchard Core team.

## 8. The header of a card

Checked: a card has a header with the item's title and the type name in grey, a move handle, an arrow button and a red
trash button (`OrchardCore.Flows/Views/ContentCard-BagPart.Edit.cshtml`). The script that is meant to copy the typed title
into the header (`flows.edit.ts`, the `keyup` handler) looks for classes that this card does not have, so the header is
not expected to change while typing. To confirm at `manage-1`: a saved item's collapsed header shows its title (the
display text stored by the Title part) and the type name. The narration does not describe the header text.

## 9. The Blocks dialog

Checked: the dialog is built by `OrchardCore.Flows/Views/ContentTypePicker.cshtml` and `Assets/ts/content-type-picker.ts`
(title **Select Block**, a **Search** box, a side list that starts with **All** and adds one entry per category, and one
card per content type with its name, its description and an **Add** button; it hides itself after an item is added). The
two seeded types have no category, so the side list should show only **All**. To confirm at `blocks-3`: the card layout,
that the new card appears at the end of the bag, and that the dialog closes. The narration says "a card for each content
type" and "Selecting Add on a card adds an item to the bag".

## 10. The Edit button of a row in the Content Items list

The list can draw its rows in a List or a Grid layout (the admin list). The selectors in the plan for the **Edit**
and **View** buttons of a row (`a.btn.edit`, `a.btn.view`, from
`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml`) must be checked in the layout the site uses.

## 11. The Stereotypes box in the bag settings

Checked: `OrchardCore.Flows/Views/BagPartSettings.Edit.cshtml` holds three blocks: the radio group **Contained Content
Types** (lines 5 to 21), the list of content types (lines 23 to 29) and a label **Stereotypes** with a text box
(lines 31 to 37). The script `Assets/ts/bag-part-settings.ts` shows one of the last two blocks according to the radio
button, but it finds the radio buttons with `document.getElementsByName("Source")`. The inputs of the view are named
with the prefix of the settings driver (`ContentTypePartDefinitionDisplayDriver.BuildPrefix`, which builds the type
name, the part name and the driver name, and `DisplayDriverBase.Factory`, which applies it), so the lookup may find
nothing, and then neither block is hidden. To confirm at `add-5`: whether the **Stereotypes** box is on screen when
**Content Types** is selected. The narration does not mention the box, and the plan says what to do if it is there. If
it is visible, report it to the Orchard Core team.

## 12. The Parts list of the new type

Checked: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml` (lines 25 to 30) leaves out a part for which
`GetPartDefinitionAsync` returns nothing, and `ContentDefinitionService.AddTypeAsync` (line 82) adds the part of the
type to the type without storing a part definition, so the type **Help Page**, which has no field, is expected to
list only **Title** after `add-2`, and **Title** then **Help Items** after `add-7`. The narration of `add-7` says
"lists Title and Help Items", which holds either way. To confirm at `add-2` and `add-7`: the rows of the **Parts** list
and their order.
