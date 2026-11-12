# Recording plan: Write a Content Field With Its Editor and Settings

Audience: developer. Chapters 01 to 07 are slides drawn by `slides.py`: they need no site and no recording. Chapter 08
is the only recorded clip, `08-admin`: it shows in the admin what the code of the slides produces. A selector below
stands for `scene.Page.Locator(...)`. "Source" lines are paths in the `OrchardCMS/OrchardCore` checkout, relative to
its `src` folder, at the commit named in `catalog/SOURCE.md`.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore`. Run `OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`), with
  `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- Auto Setup, with these environment variables (the password is chosen outside git and read from `VIDEO_PASSWORD`;
  write nothing else anywhere):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"   # the placeholder is <password>
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  ```

- Tenant: Default. Recipe: Blog (it sets the site theme to TheBlogTheme). Time zone: `America/Los_Angeles`. Culture:
  the default, en-US.
- Features to enable before recording: none. The Blog recipe already enables the ones the clip uses: Content Fields
  (`OrchardCore.ContentFields`), Content Types (`OrchardCore.ContentTypes`), Contents (`OrchardCore.Contents`), Title
  (`OrchardCore.Title`), Autoroute (`OrchardCore.Autoroute`) and Deployment (`OrchardCore.Deployment`, for JSON Import).
  Source: `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:50-73`.
- Settings: none to change.
- External services: none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls out.
- Sign in once outside the capture as user `admin` with the password of `VIDEO_PASSWORD` (`Recorder setup`), which saves
  `state.json` (git-ignored). Never type the password on camera.

## The fake seed data (create it outside the capture)

The clip needs one content type, **Event**, with the Title and Autoroute parts and its own part, and no field. Create
it before the snapshot: in the admin, open Tools, Deployments, JSON Import (`OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18-30`),
paste this into the box labeled JSON, and select Import (`OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`):

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Event",
          "DisplayName": "Event",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Draftable": true,
              "Versionable": true,
              "Listable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "Event",
              "Name": "Event",
              "Settings": {
                "ContentTypePartSettings": { "Position": "2" }
              }
            },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "{{ Model.ContentItem | display_text | slugify }}"
                },
                "ContentTypePartSettings": { "Position": "1" }
              }
            },
            {
              "PartName": "TitlePart",
              "Name": "TitlePart",
              "Settings": {
                "ContentTypePartSettings": { "Position": "0" }
              }
            }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "Event",
          "Settings": {},
          "ContentPartFieldDefinitionRecords": []
        }
      ]
    }
  ]
}
```

The shape follows the Article type of the Blog recipe (`blog.recipe.json:128-185` for the type, `:676-705` for its part). Then check, before the
snapshot:

- `http://localhost:5010/Admin/ContentTypes/Edit/Event` shows the page title "Edit Content Type - Event", a "Fields"
  heading with no field under it, an "Add Field" button, and a "Parts" list with Title, Autoroute and Event.
- Design, Content Definition, Content Types lists Event (`OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`).
- No content item of type Event exists.

If the import is refused, create the type by hand instead: Design, Content Definition, Content Types, then add the
Title and Autoroute parts, with the Autoroute pattern above, and give it the technical name `Event`. A type created in the
admin gets a part with its own name (`OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:82`).
Whatever way you use, the type must end up with a part named Event that holds no field.

## Order, dependencies, snapshot

- Slides: no order, no site.
- `08-admin` is the only clip. It starts from the seeded site (the Event type without a field, no Event item).
- It changes the site: it adds the field Featured to the Event part, saves its settings (label, default value, editor)
  and creates and publishes the Event "Community meetup". Stop the site, copy `App_Data` and `state.json` aside after
  the seed, and restore that copy before every take.

## Clip `08-admin` (chapter 08, "The field in the admin")

Start page: `/Admin/ContentTypes/Edit/Event`, signed in through `state.json`. Before `StartCaptureAsync`: wait until the
text "Edit Content Type - Event" and the "Add Field" button are visible, and collapse the admin menu, as in the other
screens of the specification that need the whole width. Source of the route:
`OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:167`.

### `admin-1`

- Page: Edit Content Type - Event (`OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`).
- Actions: highlight the "Fields" heading, `h3:has-text("Fields")` (`Edit.cshtml:88`), for about two seconds, then the
  Parts list, `ul#parts` (`Edit.cshtml:146`), for about two seconds, then `ClearHighlightsAsync`. Near the end of the
  narration, `ClickAndWaitForNavigationAsync(a.btn:has-text("Add Field"))` (`Edit.cshtml:137`).
- Visible when the narration ends: the page titled `Add New Field To "Event"`
  (`OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:20`), with the empty Display Name input.
- Wait: the Display Name input is visible and focused.

### `admin-2`

- Page: Add New Field To "Event".
- Actions: `TypeAsync(GetByLabel("Display Name"), "Featured")` (`AddFieldTo.cshtml:38`); press Tab. Wait until the
  Technical Name input (`AddFieldTo.cshtml:46`) holds `Featured` (the page copies it from the display name:
  `OrchardCore.Modules/OrchardCore.ContentTypes/Assets/js/autocomplete.ts`). Highlight the Field Type list
  (`AddFieldTo.cshtml:54`, the radio list `ul.list-group`) for about two seconds. Click the radio labeled "Boolean Field"
  (`AddFieldTo.cshtml:62`: the label is the field type name made friendly, `BooleanField` becomes "Boolean Field"). Clear
  highlights. `ClickAndWaitForNavigationAsync(button:has-text("Save & Configure"))` (`AddFieldTo.cshtml:72`).
- Visible when the narration ends: the page titled `"Featured" settings`
  (`OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:30`). The notification `The field
  "Featured" has been added.` may still be on screen (`AdminController.cs:768`).
- Wait: the text `"Featured" settings` is visible.

### `admin-3`

- Page: `"Featured" settings`: Display Name, Technical Name, then the settings of the Boolean field.
- Actions: highlight the three settings one after the other for about a second each, then clear: Hint
  (`OrchardCore.Modules/OrchardCore.ContentFields/Views/BooleanFieldSettings.Edit.cshtml:4`), Label (`:12`) and the check
  box "Default value is true" (`:23`). `TypeAsync(GetByLabel("Label", Exact), "Featured event")`. Click the check
  box `GetByLabel("Default value is true")`.
- Visible when the narration ends: the Label input holds `Featured event` and "Default value is true" is ticked. The
  Hint is empty.
- Wait: after the click, `IsCheckedAsync` of the check box is true.

### `admin-4`

- Page: the same page, scrolled to the editor list.
- Actions: scroll until `select[data-field-editor-select]` is in view; this is the drop-down below the label "What type
  of editor should be used?" (`EditField.cshtml:79`). A headless browser draws no open list, so show the two entries with
  `HighlightAsync(select[data-field-editor-select], "Standard, Switch")`. The entries are the Option templates
  `OrchardCore.Modules/OrchardCore.ContentFields/Views/BooleanField.Option.cshtml:4` ("Standard") and
  `BooleanField-Switch.Option.cshtml:4` ("Switch"); the page lists them from the shapes named `BooleanField_Option*`
  (`EditField.cshtml:17`). The source does not fix the order in which the list shows the two entries (see
  `OPEN-QUESTIONS.md`, 5): the caption names them in the order of the narration, whatever the list shows. After about three seconds `SelectAsync(select[data-field-editor-select], "Switch")`, then clear
  highlights.
- Visible when the narration ends: the editor list shows "Switch".
- Wait: the selected option of the list is "Switch".

### `admin-5`

- Page: the same page, scrolled to the display mode list.
- Actions: scroll until `select[data-field-display-select]` is in view, below the label "What type of display mode should
  be used?" (`EditField.cshtml:100`). `HighlightAsync(select[data-field-display-select], "Standard")` for about two
  seconds (its only entry comes from `BooleanField.DisplayOption.cshtml:4`, "Standard"). Clear highlights, scroll to the
  bottom, `ClickAndWaitForNavigationAsync(button.save)` (the button labeled "Save", `EditField.cshtml:120`).
- Visible when the narration ends: the page titled "Edit Content Type - Event", redirected back to the type editor (the
  Add Field link carried the return address, `AdminController.cs:911-914`). The notification `The "Featured" field settings have been saved.`
  may still be on screen (`AdminController.cs:908`).
- Wait: the text "Edit Content Type - Event" is visible and `#fields li` holds one row.

### `admin-6`

- Page: Edit Content Type - Event, with the field in the Fields list.
- Actions: highlight the row `#fields li` (`Edit.cshtml:90`) for the whole step. Optionally, move the cursor over the badge
  of the row, `#fields li .badge` (`Edit.cshtml:123`).
- Visible when the narration ends: one row with the display name "Featured", the field type "Boolean Field" in a lighter
  hint (`Edit.cshtml:115`, the friendly name of `BooleanField`) and a badge with the text "Switch" (the editor, with the
  tooltip "Editor"). There is no display mode badge, since the display mode is Standard.

### `admin-7`

- Page: starts on Edit Content Type - Event, then goes to the content editor.
- Actions: `ClearHighlightsAsync`; `NavigateAsync("/Admin/Contents/ContentTypes/Event/Create")`
  (`OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:407`; the same page is reached by Content,
  Content Items, New, Event). Wait until the input labeled "Title" (`OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`)
  and the switch labeled "Featured event" are visible. `HighlightAsync(.form-switch)` for about two seconds (the switch is
  drawn by `BooleanField-Switch.Edit.cshtml:13-15`, and it is ticked because the item is new and the default value is
  true, `BooleanFieldDisplayDriver.cs:28`), then clear. `TypeAsync(GetByLabel("Title", Exact), "Community meetup")`.
  `ClickAndWaitForNavigationAsync(button.publish)` (the button labeled "Publish",
  `OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`).
- Visible when the narration ends: the editor of the published item "Community meetup" (the publish redirects to the
  item's Edit page), with the switch still on.
- Wait: before typing, `IsCheckedAsync` of `.form-switch input[type=checkbox]` is true; after the click, the title
  "Community meetup" is in the Title input of the page that loads.

### `admin-8`

- Page: the front end of the published item.
- Actions: `NavigateAsync("/community-meetup")` (the Autoroute pattern of the seed turns the title into this path). Wait
  until `div.field-type-booleanfield` is visible. `HighlightAsync(div.field-type-booleanfield)` for the rest of the
  step.
- Visible when the narration ends: the page of TheBlogTheme with the title "Community meetup" in its header, and the
  text `True` in the highlighted block (`OrchardCore.Modules/OrchardCore.ContentFields/Views/BooleanField.cshtml:9`; the
  theme renders the Content zone with `OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:17`, and the driver places
  the shape there for the Detail display type).
- Wait: the block `div.field-type-booleanfield` holds the text `True`.

## Slides

Each slide step is drawn by `slides.py` with code of the Boolean field of the Content Fields module
(`OrchardCore.Modules/OrchardCore.ContentFields`), trimmed with `// ...`.

- `idea-1`: four cards, the first two bright: a class that inherits from ContentField, saved as JSON in the content item.
- `idea-2`: the same cards, the last two bright: the part definition that lists the field, and the settings.
- `idea-3`: four cards, all bright: the class, the display driver, the views, the settings driver.
- `class-1`: the `BooleanField` class (`OrchardCore/OrchardCore.ContentFields.Core/Fields/BooleanField.cs`) bright, the settings class dimmed.
- `class-2`: the `BooleanFieldSettings` class (`OrchardCore/OrchardCore.ContentFields.Core/Settings/BooleanFieldSettings.cs`) bright, the field class dimmed.
- `driver-1`: the outline of `BooleanFieldDisplayDriver` (`Drivers/BooleanFieldDisplayDriver.cs`): `Display`, `Edit`, `UpdateAsync`.
- `driver-2`: the `Display` method, and a card with the shape type (`GetDisplayShapeType`, `OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentFieldDisplayDriver.cs:209-220`).
- `driver-3`: the `Edit` method, and a card with the shape type (`GetEditorShapeType`, `ContentFieldDisplayDriver.cs:186-206`).
- `driver-4`: the `UpdateAsync` method, and the lines of the base class that build the prefix (`:224`) and apply the field (`:146-156`).
- `views-1`: the two view models (`ViewModels/EditBooleanFieldViewModel.cs`, `ViewModels/DisplayBooleanFieldViewModel.cs`).
- `views-2`: the editor template `Views/BooleanField.Edit.cshtml`, trimmed.
- `views-3`: the display template `Views/BooleanField.cshtml`, and a card on how templates are found by name
  (`OrchardCore/OrchardCore.DisplayManagement/Descriptors/ShapeTemplateStrategy/BasicShapeTemplateHarvester.cs`).
- `settings-1`: the `Edit` method of `BooleanFieldSettingsDriver` (`Settings/BooleanFieldSettingsDriver.cs`).
- `settings-2`: its `UpdateAsync` method, and a card on where the settings are kept.
- `settings-3`: the settings template `Views/BooleanFieldSettings.Edit.cshtml`, trimmed.
- `register-1`: the lines of `Startup.cs:31-34` that register the Boolean field.
- `register-2`: the dependencies of the feature in `Manifest.cs:17`, and the line of the Content Types `AdminController.cs:59` that builds the list of field types.
- `editors-1`: `Views/BooleanField-Switch.Edit.cshtml` and `Views/BooleanField-Switch.Option.cshtml`.
- `editors-2`: `GetEditorShapeType` of the base class, and `UseDisplayDriver` (`ContentFieldServiceCollectionExtensions.cs:24-28`).
- `editors-3`: `Views/TextField-Header.DisplayOption.cshtml`, and the `Edit` method of `Settings/TextFieldHeaderDisplaySettingsDriver.cs`.

## Checks for private data

- The only people and places on screen are invented: the Event "Community meetup", the field "Featured", the user
  `admin`. The only address that may appear is `admin@example.com`.
- No password is typed or shown; no sign-in page is recorded; no token or connection string is on a page.
- The site is a fresh Auto Setup, so there is nothing to mask. After recording, OCR the frames once per second and at
  every scene change, and search for `@`, `http` hosts other than `localhost`, and any name that is not above.
