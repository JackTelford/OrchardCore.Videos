# Open questions: Content Types, Create a Type and Add Parts and Fields

Everything in the narration was read from the Orchard Core source (commit 92db4a83). The points below are what the code
says but a person must confirm on a running site before recording, or what the source does not settle. Nothing here is
stated in the narration as more than the code says. For each, the file and line that was read.

## To confirm on the running site

1. **Technical Name filled in from "Event".** The page fills the Technical Name from the Display Name with a script
   (`Assets/js/autocomplete.ts`) on key up and on blur, until a key is pressed in the Technical Name field itself. The
   function it calls, `getTechnicalName`, is in the built bundle of the admin theme
   (`OrchardCore.Themes/TheAdmin/wwwroot/js/theadmin/TheAdmin.js`): it removes diacritics and keeps letters, and digits that
   are not the first character, so `Event` gives `Event`. Pressing Tab in the Display Name field fires its blur, not a key
   down in the Technical Name field, so the automatic fill is not switched off. Confirm on screen that the field reads
   `Event` while typing and after Tab.
2. **Label of the Title part on the Add Parts page.** `EditPartViewModel.DisplayName` is the part's display setting, or the
   part name without the `Part` suffix (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`),
   and the Title migration sets no display name (`OrchardCore.Title/Migrations.cs:30-35`). The label is therefore `Title`,
   with the description "Provides a Title for your content item."; the page lists a part only if it has a stored definition
   and is attachable (`Controllers/AdminController.cs:276`), which is the case for the part of the Title feature. Read from
   the code; a glance on screen is enough. The narration says "most with a short description" because a part made in the
   admin has none unless one is typed: every attachable part that the features of the Blog recipe create has one.
3. **When the part named Event is listed under Parts.** `AddTypeAsync` only adds the part to the type
   (`Services/ContentDefinitionService.cs:81-82`); it stores no definition for it. The definition is stored by the first
   `AlterPartDefinitionAsync` call, which is the first field added (`Services/ContentDefinitionService.cs:285-299`,
   `OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:26-40`, and `Acquire` in
   `OrchardCore.ContentManagement/ContentDefinitionManager.cs`). Building a part with no stored definition gives null
   (`ContentDefinitionManager.cs:384-393`), and the type editor skips a part whose definition is null
   (`Views/Admin/Edit.cshtml:25-30`). So the Parts list is expected to show only Title at `parts-4`, and Title and Event
   from `fields-3`, after the first field is saved; the narration says exactly that. Confirm both states on screen.
   Also confirm the order: the type's own part has no position, which the editor sorts as 5
   (`Views/Admin/Edit.cshtml`, the `defaultPosition` fallback), and the Title part gets position 1
   (`Services/ContentDefinitionService.cs:481-491`, `:507-520`), so the Parts list is expected to read Title, Event. The
   content editor places a part with no position "before" the others
   (`OrchardCore.ContentManagement.Display/ContentDisplay/ContentItemDisplayCoordinator.cs:232`), so before the type is
   saved from its editor the fields could appear above the Title. Saving the type (clip `05-order`) writes the positions
   (`Services/ContentDefinitionService.cs:376`, `AlterTypePartsOrderAsync`), and the narration of `item-2` describes the
   order after that Save. Confirm that the editor shows Title, Starts, Venue, Capacity after the Save.
4. **Radio buttons of Field Type.** The first radio button is selected when the page opens, and the list is sorted by
   the field class name (`Controllers/AdminController.cs:690`, `Views/Admin/AddFieldTo.cshtml:61`). Confirm that "Text
   Field", "Date Time Field" and "Numeric Field" appear with exactly these labels (the label is the class name with
   spaces added before each capital letter, `Views/Admin/AddFieldTo.cshtml:62`), and that `GetByLabel(..., Exact = true)`
   finds each of them once.
5. **Sections on the field settings page.** `Views/Admin/EditField.cshtml` renders the settings of the field type and,
   below, the lists of editors and display modes; other enabled features can add their own sections to this page. The
   plan stops at the Hint. Confirm that no section with code, Liquid, HTML or JSON is visible in the frame at the end of
   `fields-5`, and scroll it out of the frame if one is.
6. **Layout of the Content Types list.** The default is the List layout with the Buttons actions layout
   (`OrchardCore.Admin.Abstractions/AdminListOptions.cs`), where the four buttons of the row sit in the `Actions` zone
   (`Drivers/ContentTypeEntryDisplayDriver.cs`). Confirm that "Create New Event", "List Items", "Edit" and "Delete" are
   buttons on the row, not entries of an "Actions" menu. A List and Grid selector is expected on the page: the setting
   "Let users choose the layout of a list" is on by default (`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:49`,
   `OrchardCore.Admin/Configuration/AdminListOptionsConfiguration.cs:40`, `OrchardCore.Admin/AdminListShapeTableProvider.cs:80`).
   Confirm where it sits, that it does not hide the search box or the "Create new type" button in the 1536x864 frame, and
   that the recorder never clicks it (the choice is kept in a cookie, which `state.json` would keep).
7. **Drag and drop in `order-1`.** The lists use Sortable (`Assets/js/content-type-sortable.ts`), which relies on the
   browser's drag events. Confirm that `DragToAsync` reorders the rows in the recorder's headless Chromium, and which
   offset drops Starts above Venue.
8. **How the left menu opens.** `AdminMenu.cs` builds Design, Content Definition, Content Types. The menu is rendered
   fully collapsed, and the default behavior is `Focused` (`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:29`,
   `Models/AdminMenuBehavior.cs`; `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:157, 170`), so from `/Admin`
   three clicks reach Content Types, and nothing is remembered between takes. The entries with children and no page of their
   own are rendered as buttons, the others as links (`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`),
   which is why the plan uses `GetByRole(Button)` for Design and Content Definition and `GetByRole(Link)` for Content Types
   inside `#left-nav`. Read from the theme's source, not run: confirm that the three locators each match one element and
   that the accessible name is exactly the label.
9. **Asterisk and hint in the item editor.** The label of a required field gets the class `ocat-label-required`, drawn as
   a red asterisk by `OrchardCore.Themes/TheAdmin/Assets/scss/components/_ocat.scss:27-34`, and the hint is rendered
   under the input (`OrchardCore.ContentFields/Views/DateTimeField.Edit.cshtml:22`). The narration of `item-2` says
   "marked with an asterisk" and that the hint "appears under it". Confirm both on screen.
10. **Return pages.** After "Save" in the type editor the code redirects to the Content Types list, with the message
    "Content type updated successfully." (`Controllers/AdminController.cs:225-230`); after "Publish" with the return
    address of the row's button, to the Content Types list, with "Your Event has been published."
    (`OrchardCore.Contents/Controllers/AdminController.cs:473, 795-798`); "List Items" opens the content list filtered on
    the type. Confirm each page and message.
11. **Settings checkboxes of a created type.** `AddTypeAsync` turns on Creatable, Draftable, Versionable, Listable and
    Securable (`Services/ContentDefinitionService.cs:85-91`). Confirm that the five checkboxes are ticked on the Edit
    Content Type page after the type is created, since `parts-3` says they are turned on when a type is created.
12. **Pronunciation.** The storyboard maps "Add & Configure" to "Add and Configure" and "Save & Configure" to "Save and
    Configure" in `spoken`, so that the voice is not left to read the ampersand. These are ordinary words, but they are new
    entries: confirm them with `scripts/pronounce.py`, together with how the voice reads "Creatable" and
    "Listable" (`parts-3`, `create-2`), "Date Time Field" and "Technical Name".
13. **Name of the added part in the notification.** After "Add Selected Parts" the message names the part by its technical
    name, `TitlePart` (`Controllers/AdminController.cs:372` joins the selected part names), while the Add Parts page and
    the Parts list show `Title`. The narration does not read the message. Confirm the text, and keep the cursor away if it
    is distracting.
14. **Buttons of the other rows.** Only types that are Creatable have a "Create New" button
    (`Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:17-20`), and the "Delete" button of a system-defined type is shown
    disabled (`:26-33`). The narration of `create-2` says so about Creatable only. Confirm that the Article row has all
    four active buttons, as the plan expects, and that it is in view without scrolling.

## Not settled by the source

15. **Default thumbnail and category.** `Startup.cs:45` sets `DefaultThumbnailPath` to a placeholder image when the
    configuration has none, and `AddTypeAsync` stores it in the type. It is not shown in the video; the "Thumbnail" box
    of the Edit page may show that path. If it is visible when `parts-3` ends, keep the cursor away from it.
16. **What Remove does to existing items.** The narration says only that Remove takes a part off the type and a field
    off the type. What happens to the data of existing items was not read, and the video does not say.
17. **Deleting a type.** The Delete button of a type is not used by the video. `AdminController.Delete` calls
    `RemoveTypeAsync(id, true)`, but the service ignores its `deleteContent` argument
    (`Services/ContentDefinitionService.cs:104-138`); whether items of a deleted type remain was not checked.
