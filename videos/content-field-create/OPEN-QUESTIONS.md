# Open questions: Write a Content Field With Its Editor and Settings

Everything in the narration and on the slides was read from the Orchard Core source at the commit of
`catalog/SOURCE.md` (the Content Fields module, the Content Types module, the display management and content management
libraries, and TheBlogTheme). No site was run, so a person must confirm the following on a running site before
recording.

1. **The Event seed.** The JSON of `recording-plan.md` was written after the Article type of the Blog recipe
   (`blog.recipe.json:128-185` for the type, `:676-705` for its part) and was not imported. Confirm that JSON Import
   accepts it, that Content Types then lists Event, and that "Edit Content Type - Event" shows the Title, Autoroute and
   Event parts with an empty Fields list and an "Add Field" button. The Add Field action loads the part named after the
   type, and accepts a type without such a part (`AdminController.cs:706` and `:1184-1201`), and adding the field creates
   that part (`ContentDefinitionService.cs:275-295`). The `ContentParts` entry of the seed is still needed for the Parts
   list: `Edit.cshtml:25-30` skips a part that has no definition, so without the entry Event would not be listed before
   the first field is added.
2. **The Parts list of `admin-1`.** The narration says the type "has the Title and Autoroute parts". The list shows
   `DisplayName()` of each part of the type (`Edit.cshtml:153`). For the part named like the type it is the display name
   of the type, "Event" (`ContentTypePartExtensions.cs:7-26`). For the others it is the name without the suffix "Part"
   (`ContentPartSettingsExtensions.cs:54-64`), unless a part definition of the site sets a display name, so "Title" and
   "Autoroute" are expected. Confirm on screen.
3. **The Field Type list of `admin-2`.** The radio buttons are the registered field names made friendly with
   `CamelFriendly()` (`AddFieldTo.cshtml:62`), so "Boolean Field" is expected. Confirm the label, and that the radio
   button is selectable by its label.
4. **The Technical Name of `admin-2`.** `autocomplete.ts` copies the display name into the technical name on key up and
   on blur, through `getTechnicalName`. Confirm that "Featured" gives the technical name `Featured` (the text is typed by
   `TypeAsync` and followed by Tab).
5. **The editor list of `admin-4`.** The entries are read from the shape table (`EditField.cshtml:17`). Confirm that the
   list shows exactly "Standard" and "Switch" for the Boolean field with the Blog recipe, and no entry from another
   module (no other template named `BooleanField*.Option.cshtml` exists in the source). The order of the two entries
   comes from the order in which the templates are found, which the source does not fix: confirm it, and if "Switch"
   comes first, the narration (which says "Standard and Switch") is still true, but the caption of the highlight must
   follow the order on screen. The native list cannot be drawn open by the headless browser, so the plan shows the two
   names in a highlight caption: confirm that the caption is legible.
6. **The display mode list of `admin-5`.** Same check: only "Standard" is expected (`BooleanField.DisplayOption.cshtml`).
7. **The badge of `admin-6`.** `Edit.cshtml:123` draws a badge with the editor name when the editor is not empty, so
   "Switch" is expected, and no display mode badge. Confirm.
8. **The default value of `admin-7`.** `BooleanFieldDisplayDriver.cs:28` uses the default value of the settings when
   `context.IsNew` is true, and `AdminController.cs:417` of the Contents module passes `true` for a new item. Confirm that
   the switch is ticked when the editor opens, and that its label is "Featured event" (`BooleanField-Switch.Edit.cshtml:15`).
9. **The front end of `admin-8`.** The display template prints `@Model.Value` in a `div` with the class
   `field-type-booleanfield`. The type Event has a part with its own name, for which no wrapper shape is created
   (`ContentItemDisplayCoordinator.cs:108`), so the field shape is placed in the Content zone of the item, which
   `TheBlogTheme/Views/Content.liquid:17` renders. No `placement.json` of the modules that the Blog recipe enables, and
   none of the theme, mentions the Boolean field. This was not run: confirm that "True" is visible on
   `/community-meetup`, and that the page has no error. If the theme hides it, record the page with another theme
   rather than change the narration.
10. **Pronunciation.** The narration spells identifiers as words (Update Async, Get Editor Shape Type, "I Content Part
    Field Definition Display Driver", "Boolean Field dot Edit"). Check them with `scripts/pronounce.py` before
    narrating, in particular how the voice reads "dot" and "Async". The label "Save & Configure" is written as such in
    the captions and spoken as "Save and Configure" through the `spoken` map of the storyboard.
11. **The pointer to another video.** `register-2` says "A separate video covers creating a module". The series holds
    such a video, but if it is dropped, remove the sentence (the step stays above 8 words).
