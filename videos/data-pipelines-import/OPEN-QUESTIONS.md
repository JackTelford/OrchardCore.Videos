# Open questions: data-pipelines-import

The narration was written from the source of Orchard Core at commit `92db4a83` (no site was run). Each item says what was
read in the source, and what a person must confirm on a running site while following `recording-plan.md`. If one of the
confirmations of 1 to 4 fails, the narration of the steps named there must change before the voice is generated.

1. **The seed recipes were not run.** The Product content type (`ContentDefinition` step) is written after the Blog recipe
   (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `Article` and `BlogPost` definitions) and the
   `NumericFieldSettings` of `src/OrchardCore/OrchardCore.ContentFields.Core/Settings/NumericFieldSettings.cs`. Confirm that
   both JSON Imports succeed, that **Design > Content Definition > Content Types** lists **Product**, that its Price is
   required, and that the Data Pipelines feature is enabled (**Tools > Data Pipelines** appears in the admin menu).
2. **The row without a price is rejected.** Source: a CSV cell that is empty is read as no value
   (`src/OrchardCore/OrchardCore.DataSources.Core/Files/CsvDataFileFormat.cs`, `row[index] = record[index].Length == 0 ? null : ...`),
   it is written to the item as an empty `Price` (`ContentDataColumns.WriteValue`), and `NumericFieldHandler.ValidatingAsync`
   (`src/OrchardCore.Modules/OrchardCore.ContentFields/Handlers/NumericFieldHandler.cs`) fails a required field without a
   value ("A value is required for Price."), so `SaveContentItemsStep` counts the row as rejected. Confirm that the first run
   delivers exactly "Created 4 and updated 0 'Product' content items; skipped 0 and rejected 1 rows." (steps `run-3`) and the
   second run "Created 0 and updated 4 'Product' content items; skipped 0 and rejected 1 rows." (steps `again-2`). Confirm
   that the run still ends with the status "Succeeded" and a warning in the log.
3. **Matching on the display text.** Source: with **Match on** = Display text, the key is the value of the **Name** field, an
   item is looked up by the display text of its latest version, and a created item gets that display text
   (`SaveContentItemsStep.SaveAsync`, `FindAsync`). Confirm in the Content Items list that the four items are named after
   their rows ("Reading lamp", "Standing desk", "Task chair", "Ceramic mug"), and that the second run creates no duplicate
   (steps `save-4`, `check-1`, `again-2`). The sentence of `save-4` "display text, which is how the item is named in lists"
   rests on this.
4. **The option texts of the drop-down lists.** They are computed, not written: the part label of `TitlePart` is "Title"
   (no display name is set on the part, `ContentPartSettingsExtensions.DisplayName` removes the "Part" suffix), the value
   is described as "Title" by `ContentDataSourceOptions`, so `ContentDataColumns.Build` labels it "Title"; the fields of the
   Product part take the display names of the seed. Expected options: "Title (TitlePart.Title)", "Price (Product.Price)",
   "Category (Product.Category)", and for the rows "Name (Text)", "Price (Text)", "Category (Text)". The narration says
   only "Title", "Price" and "Category". Confirm (steps `save-4`, `save-5`).
5. **The file in the media library.** The file is placed in `Sites/Default/Media/imports/` on disk, which the media library
   reads live (`src/docs/reference/modules/Media/README.md`, "stores files in `App_Data/Sites/{tenant}/Media`"). Confirm that
   **Media > Library** lists the folder `imports` and the file, and that the pipeline reads it with the path
   `imports/products.csv` (steps `read-4`, `read-5`).
6. **The preview of the file.** Source: `DPD/preview/previewTable.ts` shows an empty value as `null` and the type of each field
   as "Text". Confirm that the Data tab of **Read a media file** shows five rows, three columns, and `null` for the price of
   "Wall shelf" (step `read-5`), and that the five rows fit on screen once the panel is resized.
7. **The output dot opens the list of steps.** Source: `DPD/canvas/DesignerCanvas.vue` (`onPortClick`) opens the quick-add list
   for an output with no connection, and `DPD/canvas/StepPort.vue` starts a connection on `pointerdown`. Confirm that the
   scene's `ClickAsync` on the output dot opens the list titled "Add a step after Rows" instead of starting a drag (step
   `save-1`). If it does not, right-click the card and choose "Add a step after it..." in its menu, which opens the same list
   (`DPD/canvas/DesignerCanvas.vue:258-260`). Pressing `Enter` on the dot does not: it opens the dialog "Connect an output of..."
   (`@port-activate`, `DesignerCanvas.vue:402`).
8. **Names of the form controls.** The selectors of `save-2` to `save-5` assume the names `DataPipelineStep.ContentType`,
   `.Mode`, `.MatchBy`, `.KeyField` and `DataPipelineStep.Mappings[n].Target` / `.Source`
   (`DP/Views/Shared/DataPipelineContentMappingRow.cshtml:3-5` says the rows bind as `DataPipelineStep.Mappings[0]`).
   Confirm the names with `Recorder probe`, and that each change is applied by the server before the next one
   (the form reloads after the content type is chosen).
9. **Publishing.** No warning is expected, so no confirmation dialog should appear when **Publish** is clicked; the toast
   should read "Version 1 was published." and the toolbar badge "Version 1". The wording of the Issues tab when there is
   nothing to fix comes from the shared designer components, which are not in the source snapshot (`@bloom/designer`): the
   words were read in the compiled `data-pipelines-designer.js`. Confirm them on screen (steps `save-6`, `run-1`).
10. **The run.** Confirm that the **History** tab opens by itself after **Run**, that the run shows "Succeeded" within the
    narration of `run-2` (otherwise wrap the wait in `OffCameraAsync`), that **Open the run** is visible without scrolling
    the panel, and that the **Delivered** list is visible on the run page after scrolling (step `run-3`). The canvas badge of
    **Save content items** counts the rows of its Rejected output, so the narration does not read it.
11. **The editor of a product.** Confirm that the item "Task chair" shows the Title, the Price (189.5 or 189.50) and the Category
    without scrolling (step `check-2`).
12. **Run from the list.** Confirm the dialog opened by **Run now** in the **Actions** menu: title "Run now", message "Run the
    pipeline 'Import products' now?", button "Run" (step `again-1`). The narration describes the action without naming the
    label, because the programme bans the word in narration.
13. **Pronunciation.** `storyboard.json` has `"spoken": {"CSV": "C.S.V."}`, the spelling the skill gives as its example. A
    person has to confirm it with `scripts/pronounce.py` before the voice is generated, and also listen to "Excel
    workbook", "Rejected" and "Read a media file".
14. **The run has the access of the user who published.** Source: `DataPipelineRunManager.QueueAsync` stores
    `PublishedByUserId`, `DataPipelineUserResolver.GetPrincipalAsync` builds the principal in the background, and
    `ReadMediaFileStep` and `SaveContentItemsStep` check the media and content permissions of that principal. Confirm that
    the first run reaches "Succeeded" (not "Failed" with a message such as "The user the pipeline runs for may not read the
    media of the folder 'imports'"), as the user `admin` (steps `run-2`, `run-3`).
