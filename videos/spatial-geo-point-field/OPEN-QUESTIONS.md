# Open questions: Geo Point Field: Add Locations to Content Items

The narration states only what the source shows. These points come from reading the source (commit `92db4a83` of
`OrchardCMS/OrchardCore`, paths under `src/`) and have not been run. A person with a running site confirms them
while recording, and corrects the narration or the plan when the site differs. Nothing below is stated as a fact in
the narration unless the source says so.

## To confirm on the running site

1. **The field on the front end of the Page type (step `check-2`).** The Blog theme renders a page with
   `OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`, which draws the whole content shape. The field's own template
   prints only the two numbers, formatted with six decimals (`OrchardCore.Modules/OrchardCore.Spatial/Views/GeoPointField.cshtml:10`).
   The Article type is not used because `Content-Article.liquid` of the same theme draws only the body, so a field of
   Article would not show on the site. Confirm that the Page shows `47.606200, -122.332100`, where on the page it
   appears, and the address of the page (the Permalink is built from the title, so probably `/find-us`). The number
   format follows the culture of the request: confirm it is the en-US format with a point and a minus sign.
   Nothing in the source hides the field on the Page: the Blog theme has no `placement.json`, the Blog recipe has no
   placement step, and no `placement.json` of a module names the field or the Page type
   (`OrchardCore.Spatial/Drivers/GeoPointFieldDisplayDriver.cs:31-32` places it in `Content` for the Detail display type).
2. **Where the field sits in the Page editor (step `enter-2`).** The Page part holds the field and has position 3,
   after the title, the permalink and the flow part (`TheBlogTheme/Recipes/blog.recipe.json`, type `Page`). Confirm
   nothing else follows it, and that "Scroll down below the page content" describes what the recorder sees.
3. **Row actions in the List layout (steps `add-1`, `check-1`, `check-2`).** The rows of Content Types and Content
   Items draw their buttons in the layout chosen in the admin settings. Confirm that "Edit" and "View" are buttons
   in the row, and not in a menu.
4. **The admin menu (steps `enable-1`, `add-1`, `enter-1`).** The groups "Tools", "Design" (with "Content Definition")
   and "Content" are buttons and open on a click (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`,
   `NavigationItemLink-admin.cshtml`). Confirm that the open state survives a page load, as the plan assumes.
5. **Toasts.** A message is drawn by `TheAdmin/Views/Message.cshtml` and hides itself only when the notifier gives a
   delay; the messages used here give none (`OrchardCore.DisplayManagement/Notify/Notifier.cs:30`). Confirm whether
   the toast stays on screen. If it stays, it can hide part of the page at the end of a step: close it before the
   narration of the next sentence, or record the step again.
6. **Ranges (step `idea-2`).** The narration says the Latitude box takes values from minus 90 to 90 and the Longitude box
   from minus 180 to 180. The source gives these limits as the `min` and `max` of the two number inputs
   (`GeoPointField-Leaflet.Edit.cshtml:15`, `:25`). The server checks only that a value is a number
   (`Drivers/GeoPointFieldDisplayDriver.cs`). Confirm what the browser does with a value outside the limits.
7. **The map (steps `enter-2`, `enter-3`, `enter-4`, `check-1`).** With empty boxes the map shows the world, centred on a
   fixed point, with a marker already on it. With saved values it opens at a street-level zoom, centred on them
   (`coordinate-leaflet-field.ts:40-53`). A click sets both boxes to six decimals and opens a popup; typing in a
   box moves the marker without moving the map. Confirm the tiles load from OpenStreetMap in the recorder's browser,
   that the click at an offset of -95 and -10 from the centre of the map lands on the map, and that the popup is
   readable. The zoom buttons and the attribution line are defaults of Leaflet, not set by the module: confirm
   they are drawn.
8. **The marker is draggable, but dragging does not update the boxes.** The marker is created as draggable
   (`coordinate-leaflet-field.ts:53`) and no handler reads its new position. The video does not drag the marker.
   Confirm it, and tell the maintainers if an editor can be misled by it.
9. **Required (step `add-4`).** The source adds the `required` attribute to both boxes, a red asterisk to both labels,
    and, on the server, the message "The {0} field is required." for each empty box (`GeoPointFieldDisplayDriver.cs`,
    `Handlers/GeoPointFieldHandler.cs`). The video only ticks the setting. Confirm the asterisks, and, if useful for a
    later video, what an editor sees when a box is empty (the message may appear once for each of the two checks).
10. **Where "Save" goes (step `add-5`).** "Save & Configure" passes the return address on, and "Save" on the settings
    page returns to the page of the content type
    (`OrchardCore.ContentTypes/Controllers/AdminController.cs:770-786`, `:911-925`). Confirm that the page
    "Edit Content Type - Page" is shown with the badge "Leaflet" beside the field.
11. **The Features search (step `enable-1`).** The search filters the rows in the browser, on the name, the id and the
    description of each feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`). Confirm that "Spatial"
    leaves only one row, and that Lucene is disabled at the start, so that the sentence about Lucene is true. The Blog
    recipe enables Indexing and Queries but not Lucene (`TheBlogTheme/Recipes/blog.recipe.json:31-80`). The row shows the
    direct dependencies as badges ("Content Types" and "Lucene") and keeps the indirect ones behind a "..." button
    (`FeatureEntry.DefaultTags.SummaryAdmin.cshtml:20-40`): confirm that the two badges are the ones on screen, and that the
    narration, which says "directly", matches.
12. **Indexing (step `after-1`).** The narration says that the location is indexed when the content type belongs to a Lucene
    index and the box "Include this element in the index" is checked in the field's settings. This is read from the
    source: the indexer skips a field whose box is unchecked (`OrchardCore.Contents/Indexing/ContentItemIndexCoordinator.cs:75-78`),
    the box is off by default (`OrchardCore.Lucene.Core/Models/LuceneContentIndexSettings.cs:10`), and the box is drawn on
    the field's settings page once Lucene is enabled and the user may manage Lucene indexes
    (`OrchardCore.Lucene/Settings/ContentPartFieldIndexSettingsDisplayDriver.cs:27-35`). The video creates no index and
    runs no search. A person confirms it by creating a Lucene index for the Page type, checking the box, saving a page,
    and running a `geo_distance` query from the Spatial documentation. The documentation does not mention the box.
13. **The "Page" links of the admin menu (step `enter-1`).** The Blog recipe adds a custom admin menu whose group
    "Content" lists the links "Article", "Page" and "Taxonomy" next to "Content Items"
    (`TheBlogTheme/Recipes/blog.recipe.json:1237-1284`). The plan scopes the "Page" item of the "New" list to its own
    drop-down list. Confirm that the group "Content" shows one link "Content Items" and not two (the admin menu merges items
    of the same name, `OrchardCore.Navigation.Core/NavigationManager.cs:74-150`).

## Not in the video

- The Standard editor is described but not recorded. Its template has no map and no `data-` attributes
  (`GeoPointField.Edit.cshtml`).
- The search queries of the documentation (`geo_bounding_box` and `geo_distance`) are named only as "search by area or
  by distance" on the last slide. The Lucene filters exist (`src/OrchardCore/OrchardCore.Lucene.Core/QueryProviders/Filters`).
  A location is searchable only when the content type belongs to an index and the box "Include this element in the
  index" is checked on the field (open question 12): the video does not create an index, so it does not claim that a
  search works on this site.
- The GraphQL type of the field (`OrchardCore.Spatial/GraphQL`) is left to a developer video.
