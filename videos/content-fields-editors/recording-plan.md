# Recording plan: Field Editors and Display Modes

Three recordings (`02-editor`, `03-list`, `04-display`), two slide clips drawn by `slides.py` (`01-concept`,
`05-catalog`), and two cards. Nothing here needs a decision from the recorder: every click, every value to type, and
every frame to check is written down.

Conventions of this plan:

- A selector in backticks stands for `scene.Page.Locator(...)`.
- "OC" is a checkout of OrchardCMS/OrchardCore at commit 92db4a83 (the commit the labels were read at). File references
  are relative to it, as `path:line`. "CT" stands for `src/OrchardCore.Modules/OrchardCore.ContentTypes`, "CF" for
  `src/OrchardCore.Modules/OrchardCore.ContentFields`, "CO" for `src/OrchardCore.Modules/OrchardCore.Contents`.
- The password is never written in git: it is `<password>`, passed in `VIDEO_PASSWORD`.
- The admin menu is not part of this story. The clips open each page by address, as the narration allows.

## The site it needs

This video is not about the Setup screen, so the site is provisioned by Auto Setup and the recorder signs in once.

| Setting | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an OC checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Sign-in of the recorder | `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`, then `Recorder setup` once (it writes `state.json`, which stays out of git) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls nothing |
| Theme | TheBlogTheme, from the Blog recipe (its `Content.liquid` draws the Workshop page) |

Auto Setup variable names: `src/docs/reference/modules/AutoSetup/README.md`.

Features: the Blog recipe already enables Content Fields, Content Types, Contents, Title, Autoroute and Users
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:46-73`). There is nothing to enable. Do not enable Lucene
or Elasticsearch: they add editors to the Content Picker field, which this video does not use.

External services: none.

### Seed data, created outside the capture

The video needs one content type, **Workshop**, with three fields, and one published Workshop. Everything is invented.

1. Sign in as `admin` in a browser (not the capture).
2. Open **Tools**, **Deployments**, **JSON Import** (`/Admin/DeploymentPlan/Import/Json`;
   `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18-31`).
3. Paste the following recipe into the **JSON** box and click **Import**
   (`src/OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`).

```json
{
  "name": "",
  "displayName": "",
  "description": "",
  "author": "",
  "website": "",
  "version": "",
  "issetuprecipe": false,
  "categories": [],
  "tags": [],
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Workshop",
          "DisplayName": "Workshop",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Listable": true,
              "Draftable": true,
              "Versionable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "TitlePart",
              "Name": "TitlePart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "0"
                }
              }
            },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "1"
                },
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "{{ Model.ContentItem | display_text | slugify }}",
                  "ShowHomepageOption": false
                }
              }
            },
            {
              "PartName": "Workshop",
              "Name": "Workshop",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "2"
                }
              }
            }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "Workshop",
          "Settings": {},
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "TextField",
              "Name": "Headline",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Headline",
                  "Position": "0"
                }
              }
            },
            {
              "FieldName": "NumericField",
              "Name": "Seats",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Seats",
                  "Position": "1"
                },
                "NumericFieldSettings": {
                  "Scale": 0,
                  "Minimum": 0,
                  "Maximum": 40
                }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "SkillLevel",
              "Settings": {
                "ContentPartFieldSettings": {
                  "DisplayName": "Skill Level",
                  "Position": "2"
                },
                "TextFieldPredefinedListEditorSettings": {
                  "Editor": "Dropdown",
                  "Options": [
                    {
                      "name": "Beginner",
                      "value": "Beginner"
                    },
                    {
                      "name": "Intermediate",
                      "value": "Intermediate"
                    }
                  ]
                }
              }
            }
          ]
        }
      ]
    },
    {
      "name": "Content",
      "data": [
        {
          "ContentItemId": "4g9q0z3h6k2m8s5v1w7x4n9c3d",
          "ContentItemVersionId": "5a2b8c4d6e1f7g3h9j0k4m5n6p",
          "ContentType": "Workshop",
          "DisplayText": "Spring Pottery Evening",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": {
            "Title": "Spring Pottery Evening"
          },
          "AutoroutePart": {
            "Path": "spring-pottery-evening"
          },
          "Workshop": {
            "Headline": {
              "Text": "Hands-on clay for beginners"
            },
            "Seats": {
              "Value": 12
            },
            "SkillLevel": {
              "Text": "Beginner"
            }
          }
        }
      ]
    }
  ]
}
```

The recipe is modelled on `blog.recipe.json` (the content definition and content steps) and on the recipe in
`src/docs/guides/create-blazor-cms/README.md`. It has not been run: step 4 is the check, and `OPEN-QUESTIONS.md` records it.

About the Skill Level field in the seed: it stores a list of options and the list editor `Dropdown`
(`CF/Settings/TextFieldPredefinedListEditorSettings.cs:12-16`; the enum is read from its name by `JsonStringEnumConverter`,
`OrchardCore.Abstractions/Json/JOptions.cs:19`), but it does not choose the Predefined List editor itself, so Skill Level
is still a plain Text Field with no badge. The stored `Dropdown` is deliberate: when `list-2` shows the settings of the
Predefined List editor, the list editor reads **Drop down list**, and `list-3` changes it to **Radio buttons**, a change the
viewer can see. (Without a stored value the list editor starts as Radio buttons, the first member of `EditorOption`.) The
step names are matched without regard to case (`NamedRecipeStepHandler`), and the `Content` step reads its array from `data`
or `Data` (`ContentStepModel`, case-insensitive properties: `JOptions.cs:28`).

4. Check the result:
   - **Design**, **Content Definition**, **Content Types** lists **Workshop**. Its Edit page lists three fields under
     **Fields**, in this order: **Headline** (Text Field), **Seats** (Numeric Field), **Skill Level** (Text Field), and no
     badge next to any of them. The settings of Skill Level, with **Predefined List** chosen in the editor drop-down list (do
     not save), show the list editor **Drop down list** and the options Beginner and Intermediate.
   - `http://localhost:5010/spring-pottery-evening` shows the Workshop on the site: the title **Spring Pottery Evening** in the
     masthead, then the lines `Hands-on clay for beginners`, `12` and `Beginner`. The Headline is not a heading yet.
   - **Content**, **Content Items** lists **Spring Pottery Evening** as published.
5. Stop the site. Copy its `App_Data` folder (the one named by `ORCHARD_APP_DATA`) and `state.json` aside as the
   **seeded** snapshot. Start the site again.

Do not open `/spring-pottery-evening` again before `display-4` in a take: restoring a snapshot and restarting the site
clears the in-memory cache, and a page rendered before the display mode changed could be kept by it.

## Order of the clips, and what each one changes

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-editor` | the seeded snapshot | Seats gets the editor Slider |
| 2 | `03-list` | the seeded snapshot | Skill Level gets the editor Predefined List, with the list editor Radio buttons and the options Beginner, Intermediate, Advanced |
| 3 | `04-display` | the seeded snapshot | Headline gets the display mode Header, level H2 |

The three clips do not depend on each other. Each one changes the site, so **restore the seeded snapshot before every
take** (stop the site, restore `App_Data` and `state.json`, start the site). No clip saves a Workshop: the New Workshop
page is left without clicking **Publish** or **Save Draft**.

The slide clips (`01-concept`, `05-catalog`) and the cards (`00-intro`, `99-outro`) need no site.

Every step below keeps its actions shorter than its narration. The last frame of each step is the one described under
"Visible when the narration ends".

## Opening card

`intro`: the opening card, drawn by the assembler. It shows the logo, the title Field Editors and Display Modes, and the
subtitle How a field is entered, and how it is shown. No recording.

## Chapter 01: One field, many faces (slides)

Slides are drawn by `slides.py`; there is nothing to record. The three cards are the same on every step; the bright card
follows the narration.

- `concept-1`: three cards titled Field, Editor and Display mode under the heading "A field, an editor, a display mode". The
  Field card is bright ("Stores one kind of value: text, a number or a date."), the other two are dimmed.
- `concept-2`: the same slide with the Editor card bright ("How the value is entered on the edit screen of a content item.").
- `concept-3`: the same slide with the Display mode card bright ("How the value is shown to visitors on the site.").

## Chapter 02: Choose an editor (clip `02-editor`)

Start: restore the seeded snapshot, then `OpenAsync("/Admin/ContentTypes/List")`: the **Content Types** page, with the
**Search** box empty and the list showing every content type. Start the capture.

- `editor-1`
  - Actions: `TypeAsync` the text `Workshop` into `#search-box`. Wait until `a[role='btn-edit-Workshop']` is visible and it
    is the only visible `a.edit-button` (the list narrows as you type). `PauseAsync(800)`. Then
    `ClickAndWaitForNavigationAsync` on `a[role='btn-edit-Workshop']`.
  - Visible when the narration ends: the page titled **Edit Content Type - Workshop**, at its top (the type's own settings).
  - Labels: Design, Content Definition, Content Types: `CT/AdminMenu.cs:23-25`. Search: `CT/Views/Admin/List.cshtml:15`.
    Edit: `CT/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`. Title: `CT/Views/Admin/Edit.cshtml:63`. The search
    box filters the list on the page: `src/OrchardCore.Modules/OrchardCore.Resources/Assets/js/list-management.js`.
- `editor-2`
  - Actions: `MoveToAsync` on `#fields` so the **Fields** list scrolls into view. `HighlightAsync` on
    `#fields li:has-text("Seats")` with no caption. `PauseAsync(2500)`. `ClearHighlightsAsync`. Then
    `ClickAndWaitForNavigationAsync` on `a[href*="/ContentParts/Workshop/Fields/Seats/Edit"]`.
  - Visible when the narration ends: the page titled **"Seats" settings**, with **Display Name** `Seats` and **Technical Name** `Seats`.
  - Labels: Fields: `CT/Views/Admin/Edit.cshtml:88`. The field's name and its kind (Numeric Field): `Edit.cshtml:115`.
    Edit: `Edit.cshtml:100`. Title: `CT/Views/Admin/EditField.cshtml:30`.
- `editor-3`
  - Actions: `MoveToAsync` on `.ocat-wrapper:has(label:text-is("Minimum"))`, then `HighlightAsync` on it and on
    `.ocat-wrapper:has(label:text-is("Maximum"))` (two rings at the same time). `PauseAsync` until the end of the step.
  - Visible when the narration ends: the **Minimum** field holding `0` and the **Maximum** field holding `40`, both ringed.
  - Labels: `CF/Views/NumericFieldSettings.Edit.cshtml:46,54`. The Slider uses them: `CF/Views/NumericField-Slider.Edit.cshtml:11-12`.
- `editor-4`
  - Actions: `ClearHighlightsAsync`. `MoveToAsync` on `[data-field-editor-select]`, then `HighlightAsync` on it.
    `PauseAsync(4500)` (the narrator reads the question and says Number is selected). `SelectAsync` on
    `[data-field-editor-select]` with the value `Slider`, at the words "Let's choose Slider".
  - Visible when the narration ends: the question **What type of editor should be used?** with the drop-down list showing **Slider**.
    The headless browser does not draw the open list, and the narration does not describe it.
  - Labels: `CT/Views/Admin/EditField.cshtml:79,81`. Number: `CF/Views/NumericField.Option.cshtml:4`. Slider:
    `CF/Views/NumericField-Slider.Option.cshtml:4`.
- `editor-5`
  - Actions: `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on `button[name='submit.Save']` (the **Save** button at the
    bottom of the form). The page returns to **Edit Content Type - Workshop**. `MoveToAsync` on `#fields`, then
    `HighlightAsync` on `#fields li:has-text("Seats") .badge`.
  - Visible when the narration ends: the Fields list, with a ringed badge reading `Slider` next to Seats (a pencil icon
    before the word). The other two fields have no badge.
  - Labels: Save: `CT/Views/Admin/EditField.cshtml:120`. The badge shows the stored name of the editor, and its tooltip is
    Editor: `CT/Views/Admin/Edit.cshtml:123`. The message `The "Seats" field settings have been saved.` is
    `CT/Controllers/AdminController.cs:908`; it may have scrolled out of view.
- `editor-6`
  - Actions: `ClearHighlightsAsync`. `NavigateAsync("/Admin/Contents/ContentTypes/Workshop/Create")` (route:
    `CO/Controllers/AdminController.cs:407`). Wait for `.numeric-field-slider .noUi-handle` to be visible.
    `MoveToAsync` on `.numeric-field-slider` so the whole **Seats** row is on screen.
  - Visible when the narration ends: the page titled **New Workshop**, the **Seats** label, the number box (empty: the field
    has no default value, `CF/Drivers/NumericFieldDisplayDriver.cs:46-54`) with `0` and `40` beside it, and the slider track
    under it with `0` and `40` at its two ends, the handle at the left end.
  - Labels: `CO/Views/Admin/Create.cshtml:26`. The row: `CF/Views/NumericField-Slider.Edit.cshtml:25-47`.
- `editor-7`
  - Actions: `DragAsync` from `.numeric-field-slider-track .noUi-handle` to the point three quarters along
    `.numeric-field-slider-track` (its left edge plus 75 percent of its width, at its vertical center). `PauseAsync(1200)`.
    `ClickAsync` on `.numeric-field-slider-value`, press `Control+A` with `scene.Page.Keyboard`, then
    `PressSequentiallyAsync("10")` on the same box. Do not click Publish or Save Draft.
  - Visible when the narration ends: the number box showing `10` and the handle about a quarter of the way along the track.
    Right after the drag, before the typing, the box reads `30.00`, with two decimals although the field's Scale is 0:
    the slider library formats its value with `toFixed(2)` unless told otherwise, and the component passes no format
    (`OrchardCore.Resources/wwwroot/Scripts/nouislider/nouislider.js`, noUiSlider 15.7.0, its default `format`; the version is in `OrchardCore.Resources/Assets/package.json:26`).
    The narration does not quote the number, and the typing replaces it.
  - Behavior: the box takes the slider's value when a drag ends, and the handle follows the box on each key release:
    `CF/wwwroot/Scripts/numeric-field-slider/numeric-field-slider.js` (the `change` and `keyup` handlers). The handle's
    class is `noUi-handle`: the library's default `cssPrefix` is `noUi-` and the class name is `handle`.

## Chapter 03: An editor with its own settings (clip `03-list`)

Start: restore the seeded snapshot, then `OpenAsync("/Admin/ContentTypes/Edit/Workshop")` (route:
`CT/Controllers/AdminController.cs:167`). Scroll `#fields` into view before the capture, with
`scene.Page.Locator("#fields").ScrollIntoViewIfNeededAsync()`. Start the capture.

- `list-1`
  - Actions: `HighlightAsync` on `#fields li:has-text("Skill Level")`. `PauseAsync(3000)`. `ClearHighlightsAsync`. Then
    `ClickAndWaitForNavigationAsync` on `a[href*="/ContentParts/Workshop/Fields/SkillLevel/Edit"]`.
  - Visible when the narration ends: the page titled **"Skill Level" settings**.
  - Labels: `CT/Views/Admin/Edit.cshtml:100,115`; `CT/Views/Admin/EditField.cshtml:30`.
- `list-2`
  - Actions: `MoveToAsync` on `[data-field-editor-select]` (the page is long; the drop-down list is below the Text Field
    settings). `SelectAsync` on it with the value `PredefinedList`. Wait for `.field-editor-predefinedlist` to be visible.
    `ScrollAsync(0, 300)` so the list editor drop-down and the options table are both on screen.
  - Visible when the narration ends: the editor drop-down showing **Predefined List**; below it the question **What type of
    list editor should be used?** with the drop-down list showing **Drop down list** (the seed stores it), and the **Options**
    table with the columns **Option Label**, **Value** and **Default?**, holding the rows Beginner/Beginner and
    Intermediate/Intermediate, and an **Add an option** button under the table.
  - Labels: Predefined List: `CF/Views/TextField-PredefinedList.Option.cshtml:4`. The settings:
    `CF/Views/TextFieldPredefinedListEditorSettings.Edit.cshtml:16,26`. The column titles and the button:
    `CF/Services/ContentFieldsJSLocalizer.cs:16,18,20,21`. The settings appear when the editor is chosen:
    `src/OrchardCore.Modules/OrchardCore.ContentTypes/wwwroot/Scripts/content-type-editors/edit-field-selector.js`.
- `list-3`
  - Actions: `SelectAsync` on `.field-editor-predefinedlist select` with the value `Radio` (the **Radio buttons** choice; the
    drop-down list changes from **Drop down list**, which the seed stored). Then
    `ClickAsync` on `.field-editor-predefinedlist a:has-text("Add an option")`: a third, empty row appears. `TypeAsync` the
    text `Advanced` into `.field-editor-predefinedlist table.options-table tbody tr:last-child input[placeholder="Enter a name"]`.
    Wait until the same row's **Value** box also reads `Advanced`.
  - Visible when the narration ends: three rows, Beginner, Intermediate and Advanced, each with the same text in both
    boxes; the list editor drop-down showing **Radio buttons**.
  - Labels: `CF/Views/TextFieldPredefinedListEditorSettings.Edit.cshtml:19,20`; **Enter a name**:
    `CF/Services/ContentFieldsJSLocalizer.cs:17`. The new row and the copy of the label into the value:
    `CF/wwwroot/Scripts/options-editor-fields/options-editor-fields.js` (the `add` and `onColumnInput` methods).
- `list-4`
  - Actions: `HighlightAsync` on `.field-editor-predefinedlist table.options-table th:has-text("Default?")`. Click nothing.
  - Visible when the narration ends: the **Default?** title ringed, with a radio button in each row under it, none selected.
  - Labels: `CF/Services/ContentFieldsJSLocalizer.cs:20`. The radio per row and what selecting one does: the same script as `list-3`.
- `list-5`
  - Actions: `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on `button[name='submit.Save']`. `MoveToAsync` on `#fields`.
  - Visible when the narration ends: **Edit Content Type - Workshop**, scrolled to the Fields list, with a badge next to Skill Level.
    The narration does not read that badge, because it shows the stored name of the editor and not its label.
  - Labels: `CT/Views/Admin/EditField.cshtml:120`. The options are saved with the field:
    `CF/Settings/TextFieldPredefinedListEditorSettingsDriver.cs`.
- `list-6`
  - Actions: `NavigateAsync("/Admin/Contents/ContentTypes/Workshop/Create")`. `MoveToAsync` on
    `label.form-check-label:text-is("Intermediate")`, then `ClickAsync` on it. Do not click Publish or Save Draft.
  - Visible when the narration ends: the **Skill Level** row with three radio buttons labeled Beginner, Intermediate and
    Advanced, Intermediate selected.
  - Labels: `CO/Views/Admin/Create.cshtml:26`. The radio buttons: `CF/Views/TextField-PredefinedList.Edit.cshtml:35-45` (the
    `EditorOption.Radio` branch).

## Chapter 04: Choose a display mode (clip `04-display`)

Start: restore the seeded snapshot, then `OpenAsync("/Admin/ContentTypes/Edit/Workshop")`, with `#fields` scrolled into view
before the capture as in `03-list`. Start the capture. Do not open the site's `/spring-pottery-evening` page before `display-4`.

- `display-1`
  - Actions: `HighlightAsync` on `#fields li:has-text("Headline")`. `PauseAsync(2500)`. `ClearHighlightsAsync`.
    `ClickAndWaitForNavigationAsync` on `a[href*="/ContentParts/Workshop/Fields/Headline/Edit"]`. Then `MoveToAsync` on
    `[data-field-display-select]`.
  - Visible when the narration ends: the page titled **"Headline" settings**, scrolled so the display mode drop-down list is on
    screen, below the editor drop-down list.
  - Labels: `CT/Views/Admin/EditField.cshtml:30,79,100,102`; Edit: `CT/Views/Admin/Edit.cshtml:100`.
- `display-2`
  - Actions: `HighlightAsync` on `[data-field-display-select]`. `PauseAsync(4000)`. `SelectAsync` on it with the value `Header`,
    at the words "Let's choose Header".
  - Visible when the narration ends: the question **What type of display mode should be used?** with **Header** showing,
    and the level setting appearing below it.
  - Labels: `CT/Views/Admin/EditField.cshtml:100`; Standard: `CF/Views/TextField.DisplayOption.cshtml:4`; Header:
    `CF/Views/TextField-Header.DisplayOption.cshtml:4`.
- `display-3`
  - Actions: `ClearHighlightsAsync`. `SelectAsync` on `.field-display-header select` with the value `h2`. `PauseAsync(1500)`.
    `ClickAndWaitForNavigationAsync` on `button[name='submit.Save']`. `MoveToAsync` on `#fields`, then `HighlightAsync` on
    `#fields li:has-text("Headline") .badge`.
  - Visible when the narration ends: the Fields list shows a ringed badge reading `Header` next to Headline (a screen icon
    before the word), and no badge next to the other two fields. The level drop-down was set to **H2** before saving.
  - Labels: **Select the header level to use** and **H2**: `CF/Views/TextFieldHeaderDisplaySettings.Edit.cshtml:4,8`. Save:
    `CT/Views/Admin/EditField.cshtml:120`. The badge's tooltip is Display mode: `CT/Views/Admin/Edit.cshtml:119`. The level
    is kept only for the Header display mode: `CF/Settings/TextFieldHeaderDisplaySettingsDriver.cs`.
- `display-4`
  - Actions: `ClearHighlightsAsync`. `NavigateAsync("/spring-pottery-evening")`. Wait for `h2.field-type-textfield` to be visible,
    then `MoveToAsync` on it.
  - Visible when the narration ends: the Spring Pottery Evening page of the site, with `Hands-on clay for beginners` drawn
    as a heading (an `h2`), above the lines `12` and `Beginner`.
  - Labels: none on screen to read from the source. The heading tag comes from the level setting:
    `CF/Views/TextField-Header.Display.cshtml:9-15`. The plain line it replaces: `CF/Views/TextField.cshtml:8`. The theme draws
    the title and then the content: `src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:17`.

## Chapter 05: What each field offers (slides)

Slides drawn by `slides.py`; there is nothing to record.

- `catalog-1`: under the heading "Editors of a Text Field and a Numeric Field", two cards. Text Field lists Standard, Code Mirror,
  Color, Email, Icon picker, Monaco editor, Multi-line, Phone, Predefined List, Url. Numeric Field lists Number, Range, Select,
  Slider, Spinner. Labels: `CF/Views/TextField*.Option.cshtml`, `CF/Views/NumericField*.Option.cshtml`.
- `catalog-2`: under the heading "More fields and their editors", four cards. Boolean Field: Standard, Switch. Multi Text Field:
  Standard, Checkbox List, Picker. Date Field: Standard, Localized. Date Time Field and Time Field: Standard. Labels:
  `CF/Views/BooleanField*.Option.cshtml`, `MultiTextField*.Option.cshtml`, `DateField*.Option.cshtml`,
  `DateTimeField.Option.cshtml`, `TimeField.Option.cshtml`.
- `catalog-3`: under the heading "Display modes", three cards. Text Field: Standard, Header. User Picker Field: Standard, User
  Names. Every other field of the Content Fields module: Standard. Labels: `CF/Views/*.DisplayOption.cshtml`.

## Closing card

`outro`: the closing card, drawn by the assembler: the logo, the title, and the subtitle docs.orchardcore.net. No recording.

## Checks for private data

- The site is new and holds only the seed above: the user `admin` with `admin@example.com`, the Blog recipe's sample content,
  and the Workshop with its invented texts. No real person, company, address, token or password appears.
- The sign-in is in `state.json` only; the password is never typed on camera.
- After recording, look at the contact sheet of each clip (`review.py`), then OCR the frames and search for an email address
  other than `admin@example.com`, and for any host other than `localhost`. The browser address bar is not part of the capture.
- Never commit `state.json`, the copied `App_Data`, an MP4 or an audio file.
