# Recording plan: Content Parts: Create Reusable Groups of Fields

Read this file and nothing else to record the video. The selectors below stand for `scene.Page.Locator(...)`; every
label is copied from the Orchard Core source (commit 92db4a83), with the file and line it comes from. File paths are
relative to `src/OrchardCore.Modules/OrchardCore.ContentTypes/` unless they start with another module's name.
Confirm every selector once with `Recorder probe <path> "<script>"` on the running site before the first take, and
write down any difference in `OPEN-QUESTIONS.md`.

The video has two `card` clips (opening and closing), one `slide` clip (`01-idea`) and five `clip` recordings
(`02-create` to `06-content`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at an
  empty folder outside both repositories. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead
  address.
- **Auto Setup**: the **Blog** recipe, tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`,
  user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in `VIDEO_PASSWORD`;
  never written in a file of the repository). The site is not the subject, so Setup is not recorded and `Recorder
  setup` saves `state.json` (git-ignored) once.
- **Features**: nothing to enable. The Blog recipe enables Content Types, Content Fields, Title, Contents and Flows
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Content Fields must be enabled,
  or the Add Field button is missing (`Views/Admin/EditPart.cshtml:84-90`). Flows is enabled too, so the built-in
  Bag part (attachable and reusable, `OrchardCore.Flows/Migrations.cs:22-26`) also appears in the Add Named Part list.
- **Names that must stay free**: no content type and no content part named `PostalAddress`. A part with the name of a
  content type is left out of every list (`Controllers/AdminController.cs:1133` and `:1161`). The Blog recipe's types are
  Article, BlogPost, Blog, Container, Blockquote, Image, Paragraph, RawHtml, Page, Tag and Category.
- **Time zone and locale**: the recorder's `en-US`; nothing else is configured.
- **Admin lists**: left at their default layout, the List layout (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`);
  the Edit and Create New buttons of the Content Types list are written for it.

### Seed data (outside the capture, once, as signed-in `admin`)

The Customer type is the only seed. It is made by hand in a browser (or in a `PrepareAsync`), then the first snapshot
is taken.

1. Open `/Admin/ContentTypes/Create` (page title **New Content Type**, `Views/Admin/Create.cshtml:5`). Type
   `Customer` into the Display Name field, `input[data-displayname]`. The Technical Name fills itself with `Customer`
   (`Assets/js/autocomplete.ts`); never type into the Technical Name field. Select **Create**, `button.create`
   (`Views/Admin/Create.cshtml:30`).
2. The **Add Parts** page opens (`Controllers/AdminController.cs:164`). Tick the checkbox of the row labeled **Title**:
   `.list-group-item:has(label:text-is("Title")) input[type='checkbox']` (`Views/Admin/AddPartsTo.cshtml:32-33`). Wait
   until `#add-selected-parts-button` is visible (it is hidden until a box is ticked,
   `Views/Admin/AddPartsTo.cshtml:47`), then select **Add Selected Parts**.
3. The **Edit Content Type - Customer** page opens with the message `The "TitlePart" part has been added.`
   (`Controllers/AdminController.cs:372`). Change nothing else: a type created on this page is Creatable, Draftable,
   Versionable, Listable and Securable from the start (`Services/ContentDefinitionService.cs:85-91`), which is what
   the `06-content` clip needs.
4. Open `/Admin/ContentTypes/List` and check that the **Customer** row has the buttons **Create New Customer**, **List
   Items**, **Edit** and **Delete** (`Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19-24`).

Take **snapshot S0** now: stop the site (by process ID), copy `ORCHARD_APP_DATA` and `state.json` aside, start the
site again.

## Order, dependencies and snapshots

Record in this order; each clip starts from the state the previous one left, and every clip changes the site.

| Clip | Starts from | Changes the site by | Snapshot to take afterwards |
|---|---|---|---|
| `02-create` | S0 | creating the part `PostalAddress` (Attachable, nothing else set) | S2 |
| `03-settings` | S2 | giving the part a Display Name and a Description, and making it Reusable | S3 |
| `04-fields` | S3 | adding the Text Fields Street, City and PostalCode to the part | S4 |
| `05-attach` | S4 | attaching the part twice to Customer, as BillingAddress and ShippingAddress | S5 |
| `06-content` | S5 | publishing one Customer content item | none |

To record one clip again, stop the site, restore the snapshot it starts from (and its `state.json`), start the site,
and record that clip. `01-idea`, `00-intro` and `99-outro` need no recording.

## Slides and cards

- `intro`: the opening card (the logo, **Orchard Core Content Parts**, and its subtitle), drawn by the assembler.
- `idea-1`: slide **A type is made of parts**, with three cards: Fields, Parts, Content types.
- `idea-2`: slide **Define once, attach many times**, with three cards: One definition, Many types, Many times.
- `idea-3`: slide **Example: a postal address**, with three check cards: Postal Address, Billing Address, Shipping
  Address.
- `outro`: the closing card (**Orchard Core Content Parts**, `docs.orchardcore.net`), drawn by the assembler.

## Clip `02-create`

Starts on `/Admin` (the admin home), opened with `OpenAsync` before `StartCaptureAsync`. The admin menu is collapsed:
do not open it before the capture, since opening it is the first thing the narration says. The menu is the `nav`
element `#left-nav`; a group with children and no page of its own is a `button.item-label`, a link is an
`a.item-label` (`OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml:24`,
`NavigationItemLink-admin.cshtml:29,69`). The label is not the text of that element but of a child
`<span class="title">` (`NavigationItemText.cshtml:40`; the alternate
`OrchardCore.ContentTypes/Views/NavigationItemText-contentdefinition.Id.cshtml:4` has the same span). A `:text-is()` on `.item-label` itself matches nothing, because Playwright matches the
smallest element with that text and the child has the same text: the selectors below test the child, with `:has(> .title:...)`.
Keep each action shorter than its narration.

### `create-1`

- Actions: `ClickAsync` `#left-nav .item-label:has(> .title:text-is("Design"))` and wait until `#left-nav
  .item-label:has(> .title:text-is("Content Definition"))` is visible; `ClickAsync` that item and wait until `#left-nav
  a.item-label:has(> .title:text-is("Content Parts"))` is visible; `ClickAndWaitForNavigationAsync` the **Content Parts**
  link. `HighlightAsync` `a#btnCreate` (no caption),
  `PauseAsync(700)`, `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` `a#btnCreate` as the last action.
- On screen when the narration ends: the page **New Content Part** with an empty, focused Name field.
- Labels: Design, Content Definition, Content Parts: `AdminMenu.cs:23-30`. Create new part:
  `Views/Admin/ListParts.cshtml:20`. The list's page title **Content Parts**: `Views/Admin/ListParts.cshtml:6`.
- The rows the Content Parts list holds on this site are not known (see `OPEN-QUESTIONS.md`): look at them in the
  contact sheet.

### `create-2`

- Actions: `HighlightAsync` `.ocat-limited .hint` (the text **Use no spaces or special characters. For example,
  "MyCustomPart"**), `PauseAsync(900)`, `ClearHighlightsAsync`; `TypeAsync` `input[name='Name']` with `PostalAddress`
  (delay 45); `PauseAsync(500)`; as the last action `ClickAndWaitForNavigationAsync` `button.create[type='submit']`
  (the **Create** button). Clicking it late means the message of the next step is on screen when that step begins.
- On screen when the narration ends: the page **Edit Content Part - PostalAddress**, with the green message from the
  next step starting to show.
- Labels: Name, the hint and Create: `Views/Admin/CreatePart.cshtml:13,16,21`. The page title **New Content Part**:
  `Views/Admin/CreatePart.cshtml:4`.

### `create-3`

- Actions: none beyond `HoldAsync` to the end of the narration. `HighlightAsync` `.toast.message-success` for the last
  two seconds if the message is still there. A success message hides itself after 5000 milliseconds
  (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71` sets the delay, and
  `OrchardCore.Themes/TheAdmin/Views/Message.cshtml` turns it into `data-bs-autohide` and `data-bs-delay`), so it is
  visible only during the first seconds of this step.
- On screen: the message `The "PostalAddress" content part has been created.` and the editor of the part, with the
  title **Edit Content Part - PostalAddress**, the fields Technical Name, Display Name, Description, and the boxes
  Attachable and Reusable.
- Labels: the message `Controllers/AdminController.cs:579` (it shows the technical name); the title
  `Views/Admin/EditPart.cshtml:23`.

## Clip `03-settings`

Starts on `/Admin/ContentParts/Edit/PostalAddress`, opened with `OpenAsync` before the capture (no message on
screen). Fields are found by their label, since the input ids come from the driver's model name:
`.ocat-limited-wrapper:has(> label:text-is("Display Name")) input`, and for the boxes
`.form-check:has(label:text-is("Reusable")) input[type='checkbox']`.

### `settings-1`

- Actions: `HighlightAsync` the Technical Name input (`.ocat-limited-wrapper:has(> label:text-is("Technical Name"))
  input`, read-only, holds `PostalAddress`), `PauseAsync(1200)`, `ClearHighlightsAsync`; `TypeAsync` the Display Name
  input with `Postal Address` (delay 45); `TypeAsync` the Description input
  (`.ocat-limited-wrapper:has(> label:text-is("Description")) input`) with `Street, city and postal code.`
- On screen when the narration ends: Display Name `Postal Address` and Description `Street, city and postal code.`
  filled in, Technical Name unchanged. Before it is typed, the Display Name input shows the placeholder `PostalAddress`
  (`Views/ContentPartSettings.Edit.cshtml:15`).
- Labels: `Views/ContentPartSettings.Edit.cshtml:5,13,21`. The Display Name is stored in the part's settings by
  `Editors/ContentPartSettingsDisplayDriver.cs:34`.

### `settings-2`

- Actions: `HighlightAsync` `.form-check:has(label:text-is("Attachable"))`; `HoldAsync` to the end; leave the box as it
  is (it is already checked).
- On screen: the Attachable box checked, ringed, with the hint **Check to allow this part to be attached to other
  content types.**
- Labels: `Views/ContentPartSettings.Edit.cshtml:33-34`. A part made on the Create page is attachable from the start:
  `Services/ContentDefinitionService.cs:224`.

### `settings-3`

- Actions: `ClearHighlightsAsync`; `HighlightAsync` `.form-check:has(label:text-is("Reusable"))`, `PauseAsync(600)`;
  `ClickAsync` `.form-check:has(label:text-is("Reusable")) input[type='checkbox']`; `PauseAsync(500)`,
  `ClearHighlightsAsync`; as the last action `ClickAndWaitForNavigationAsync` `button.save[name='submit.Save']`
  (the **Save** button at the bottom of the form).
- On screen when the narration ends: the part's editor reloaded, with the green message of the next step starting to
  show.
- Labels: Reusable and its hint **Check to allow this part to be attached multiple times.**
  `Views/ContentPartSettings.Edit.cshtml:43-44`. Save: `Views/Admin/EditPart.cshtml:94`.

### `settings-4`

- Actions: `HoldAsync` to the end; `HighlightAsync` `.oc-breadcrumb-title` for the last two seconds.
- On screen: the message `The settings of "PostalAddress" have been saved.` (`Controllers/AdminController.cs:635`),
  the page title **Edit Content Part - Postal Address** (`Views/Admin/EditPart.cshtml:23`), and the Reusable box
  checked.

## Clip `04-fields`

Starts on `/Admin/ContentParts/Edit/PostalAddress`, opened with `OpenAsync` (no message on screen). Every Add Field
form preselects the first field type of the list, which is not Text Field: always click the Text Field radio,
`input[name='FieldTypeName'][value='TextField']` (`Views/Admin/AddFieldTo.cshtml:61`). Never type into Technical Name;
it fills itself from Display Name on key up and on blur (`Assets/js/autocomplete.ts`). The Save buttons are
`button[name='submit.Save'][value='Save']`; with the return address the part's page passes, Save returns to the part's
editor (`Controllers/AdminController.cs:780-783`).

### `fields-1`

- Actions: `HighlightAsync` `h3:text-is("Fields")`, `PauseAsync(1500)`, `ClearHighlightsAsync`; `HighlightAsync`
  `a.btn:text-is("Add Field")`, `PauseAsync(600)`, `ClearHighlightsAsync`; as the last action
  `ClickAndWaitForNavigationAsync` `a.btn:text-is("Add Field")`.
- On screen when the narration ends: the page **Add New Field To "Postal Address"**, with empty Display Name and
  Technical Name fields and the Field Type list.
- Labels: Fields `Views/Admin/EditPart.cshtml:46`; Add Field `Views/Admin/EditPart.cshtml:88`; the page title
  `Views/Admin/AddFieldTo.cshtml:20`.

### `fields-2`

- Actions: `TypeAsync` `input[data-displayname]` with `Street` (delay 45); `PauseAsync(500)` (the Technical Name field
  now holds `Street`); `ClickAsync` `input[name='FieldTypeName'][value='TextField']`; `PauseAsync(600)`; as the last
  action `ClickAndWaitForNavigationAsync` `button[name='submit.Save'][value='Save']`.
- On screen when the narration ends: the part's editor again, with Street in the Fields list and the message `The
  field "Street" has been added.` (`Controllers/AdminController.cs:768`).
- Labels: Display Name, Technical Name, Field Type, Save: `Views/Admin/AddFieldTo.cshtml:38,46,54,71`. The radio label
  **Text Field** is the field type name `TextField` made readable with `CamelFriendly()`
  (`Views/Admin/AddFieldTo.cshtml:62`).

### `fields-3`

- Actions: `HighlightAsync` the Street row of the Fields list (`ul#fields li:has-text("Street")`), `PauseAsync(1500)`,
  `ClearHighlightsAsync`. Then the same four actions as `fields-2` with `City`: `ClickAndWaitForNavigationAsync`
  `a.btn:text-is("Add Field")`, `TypeAsync` `input[data-displayname]` with `City` (delay 30), `ClickAsync` the Text Field
  radio, and as the last action Save. Keep the typing fast (delay 30): the actions must end before the narration.
- On screen when the narration ends: the part's editor with Street and City in the Fields list, each followed by the
  hint **Text Field** (`Views/Admin/EditPart.cshtml:75`, the field type name made readable).

### `fields-4`

- Actions: `ClickAndWaitForNavigationAsync` `a.btn:text-is("Add Field")`, `TypeAsync` `input[data-displayname]` with
  `Postal Code` (delay 30), `PauseAsync(400)` (the Technical Name field holds `PostalCode`), `ClickAsync` the Text Field
  radio, `ClickAndWaitForNavigationAsync` Save. Then `HighlightAsync` `ul#fields` for the rest of the narration.
- On screen when the narration ends: the part's editor with three fields in this order: Street, City, Postal Code,
  each with the hint **Text Field**, and the message `The field "Postal Code" has been added.`.

## Clip `05-attach`

Starts on `/Admin/ContentTypes/List` (page title **Content Types**, `Views/Admin/List.cshtml:6`), opened with
`OpenAsync` before the capture. The Add Named Part page preselects the first reusable part of the list (the Bag part
when Flows is enabled: the list is sorted by display name, so Bag comes before Postal Address,
`Controllers/AdminController.cs:1178-1181`), so the plan always clicks the Postal Address radio,
`input[name='SelectedPartName'][value='PostalAddress']` (`Views/Admin/AddReusablePartTo.cshtml:53`,
`Controllers/AdminController.cs:310`). Never type into Technical Name.

### `attach-1`

- Actions: `HighlightAsync` the Customer row's Edit button, `a[role='btn-edit-Customer']` (no caption),
  `PauseAsync(1500)`, `ClearHighlightsAsync`; as the last action `ClickAndWaitForNavigationAsync` that button. If the
  Customer row is below the visible part of the list, `ScrollAsync` first.
- On screen when the narration ends: the page **Edit Content Type - Customer**, showing the top of the form.
- Labels: Edit `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` (the markup puts the name in a `role`
  attribute, which is why the selector is written that way); the page title `Views/Admin/Edit.cshtml:63`.

### `attach-2`

- Actions: `ScrollAsync(0, 700)` so the **Parts** heading and both buttons are on screen (adjust the amount on the
  contact sheet); `HighlightAsync` `h3:text-is("Parts")`, `HoldAsync(3)`; `ClearHighlightsAsync`, `HighlightAsync`
  `a.btn:text-is("Add Parts")`, `HoldAsync(4)`; `ClearHighlightsAsync`, `HighlightAsync` `a.btn:text-is("Add Named
  Part")` and hold to the end.
- On screen when the narration ends: the Parts section with the row **Title** (its description **Provides a Title for
  your content item.**, with Edit and Remove buttons), the two buttons Add Parts and Add Named Part, and the ring on Add
  Named Part. The type's own row, **Customer** ("Contains the fields for the current type"), is probably not shown, since
  the Customer type has no fields yet (see `OPEN-QUESTIONS.md`, point 10); the narration does not depend on it.
- Labels: Parts `Views/Admin/Edit.cshtml:144`; Add Parts, Add Named Part `Views/Admin/Edit.cshtml:201-202`;
  "Contains the fields for the current type" `Views/Admin/Edit.cshtml:180`. The Title row: the type part has no
  description of its own, so the part's is shown (`ContentTypePartExtensions.cs:28-38`, set by
  `OrchardCore.Title/Migrations.cs:33`).

### `attach-3`

- Actions: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` `a.btn:text-is("Add Named Part")`;
  `PauseAsync(1200)`; `ClickAsync` `input[name='SelectedPartName'][value='PostalAddress']`.
- On screen when the narration ends: the page **Add Named Part**, with the Display Name, Technical Name and
  Description fields above a list of radio rows (the Bag row, then Postal Address with its description **Street, city
  and postal code.**), and the Postal Address radio selected.
- Labels: the title `Views/Admin/AddReusablePartTo.cshtml:11`; the list holds only parts that are both attachable and
  reusable (`Controllers/AdminController.cs:299-302`); the row label is the part's Display Name.

### `attach-4`

- Actions: `TypeAsync` `input[data-displayname]` with `Billing Address` (delay 40); `PauseAsync(400)` (the Technical
  Name field holds `BillingAddress`); `TypeAsync` the Description input
  (`.ocat-limited-wrapper:has(> label:text-is("Description")) input`) with `Where invoices are sent.`; as the last
  action `ClickAndWaitForNavigationAsync` `button[name='submit.Save'][value='Save']`.
- On screen when the narration ends: the page **Edit Content Type - Customer** again, with the message `The
  "PostalAddress" part has been added.` (`Controllers/AdminController.cs:453`; it shows the technical name of the
  part, not the name given to it).
- Labels: Display Name, Technical Name, Description, Save: `Views/Admin/AddReusablePartTo.cshtml:23,31,39,75`.

### `attach-5`

- Actions: `ScrollAsync(0, 700)` as in `attach-2` and `HighlightAsync` the **Billing Address** row of the Parts list
  (`ul#parts li:has-text("Billing Address")`), `PauseAsync(1500)`, `ClearHighlightsAsync`;
  `ClickAndWaitForNavigationAsync` `a.btn:text-is("Add Named Part")`; `ClickAsync`
  `input[name='SelectedPartName'][value='PostalAddress']`; `TypeAsync` `input[data-displayname]` with
  `Shipping Address` (delay 30); `TypeAsync` the Description input with `Where orders are delivered.` (delay 30). Do
  not select Save yet.
- On screen when the narration ends: the Add Named Part page with Display Name `Shipping Address`, Technical Name
  `ShippingAddress`, Description `Where orders are delivered.` and the Postal Address radio selected.
- Labels: the row label is the type part's display name (`Views/Admin/Edit.cshtml:153`).

### `attach-6`

- Actions: as the first action `ClickAndWaitForNavigationAsync` `button[name='submit.Save'][value='Save']`; then
  `ScrollAsync(0, 700)` if the Parts section is not on screen, and `HighlightAsync` the Billing Address row, then the
  Shipping Address row, about five seconds each.
- On screen when the narration ends: the Parts list with the rows Title, Billing Address (description **Where
  invoices are sent.**) and Shipping Address (description **Where orders are delivered.**), each with Edit and Remove
  buttons, in the order of their positions 1, 2 and 3 (`Services/ContentDefinitionService.cs:481-522`; the type's own
  Customer row, if the page shows it, has no position and sorts last, `Views/Admin/Edit.cshtml:32,38`).
- Labels: the type part's description is shown beside the name (`Views/Admin/Edit.cshtml:157-159`). The parts are
  saved as soon as they are added; the **Save** button at the bottom of the type is not selected.

## Clip `06-content`

Starts on `/Admin/ContentTypes/List`, opened with `OpenAsync` before the capture. Fields of the editor are found by the
names the drivers give them: `<part name on the type>.<field name>.Text`, for example
`input[name='BillingAddress.Street.Text']` (`OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentFieldDisplayDriver.cs:224`);
the Title input by its label: `.ocat-wrapper:has(> label:text-is("Title")) input`
(`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`). The fields are typed with delay 25 to leave time.

### `content-1`

- Actions: `HighlightAsync` `a#btn-create-Customer` (no caption), `PauseAsync(1500)`, `ClearHighlightsAsync`; as the last
  action `ClickAndWaitForNavigationAsync` that button. If the Customer row is below the visible part of the list,
  `ScrollAsync` first.
- On screen when the narration ends: the page **New Customer** (`OrchardCore.Contents/Views/Admin/Create.cshtml:26`), with the
  Title field focused.
- Labels: Create New Customer `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`.

### `content-2`

- Actions: `HighlightAsync` the Title row for two seconds; `ClearHighlightsAsync`; `HighlightAsync`
  `.content-part-wrapper:has(.ocat-label:has-text("Billing Address"))` for three seconds; `ClearHighlightsAsync`;
  `HighlightAsync` `.content-part-wrapper:has(.ocat-label:has-text("Shipping Address"))` and hold to the end. If both
  groups do not fit on screen, `ClearHighlightsAsync` and `ScrollAsync(0, 300)` before the Shipping Address ring.
- On screen when the narration ends: the editor with the Title field and two groups. The label of each group is its
  display name (Billing Address, Shipping Address) with its description in small text under it; each group holds the
  fields Street, City and Postal Code, all empty.
- Labels: only a named part gets a group label with the display name and the description
  (`OrchardCore.Contents/Views/ContentPart.Edit.cshtml:26-35`). The field labels are the fields' Display Names
  (`OrchardCore.ContentFields/Views/TextField.Edit.cshtml:20`).

### `content-3`

- Actions: `TypeAsync` the Title input with `Jamie Chen`; `TypeAsync` `input[name='BillingAddress.Street.Text']` with
  `22 Cider Mill Road`; `TypeAsync` `input[name='BillingAddress.City.Text']` with `Applewood`; `TypeAsync`
  `input[name='BillingAddress.PostalCode.Text']` with `AP1 4CM`.
- On screen when the narration ends: Title and the three Billing Address fields filled in; the Shipping Address fields
  empty.

### `content-4`

- Actions: `TypeAsync` `input[name='ShippingAddress.Street.Text']` with `8 Harbor Row`; `TypeAsync`
  `input[name='ShippingAddress.City.Text']` with `Fernbrook`; `TypeAsync` `input[name='ShippingAddress.PostalCode.Text']`
  with `FB2 8HR`. Then `HighlightAsync` each group once, if time remains.
- On screen when the narration ends: both groups filled in with different values.
- Labels: the values of each named part are kept under the name the part has on the type
  (`OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentItemDisplayCoordinator.cs:326`).

### `content-5`

- Actions: `PauseAsync(600)`; `ClickAndWaitForNavigationAsync` `button.publish[name='submit.Publish']` (the **Publish**
  button, `OrchardCore.Contents/Views/Content.PublishButton.cshtml`; because the editor was opened with a return
  address, the button is the first of a button group, `:5-27`, and `.publish` does not match the `publish-continue`
  item of its menu); `HoldAsync` to the end.
- On screen: the **Content Types** page (the return address of the editor: `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`
  passes it, and `OrchardCore.Contents/Controllers/AdminController.cs:795-797` redirects to it) with the green message
  `Your Customer has been published.` (`OrchardCore.Contents/Controllers/AdminController.cs:473`), visible during the
  first seconds of the step.

## Checks for private data

- The site is new and holds only the fake data above: the people and places are Jamie Chen, 22 Cider Mill Road,
  Applewood AP1 4CM, 8 Harbor Row and Fernbrook FB2 8HR; the only email is `admin@example.com`, and it is not shown.
- Look at every contact sheet (`review.py`): the Content Parts and Content Types lists must show no name other than the
  parts and types of the Blog recipe, Customer and PostalAddress.
- OCR the video at one frame per second and at every scene change, and search for `@`, `http`, and the password. The
  password is never typed on camera (the session comes from `state.json`).
- No mask script is needed; none is kept in git.
