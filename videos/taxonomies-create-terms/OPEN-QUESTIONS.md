# Open questions: Create a Taxonomy and Organize Its Terms

The narration was written from the Orchard Core source (commit `92db4a83`), with no running site. Everything below was
read in the code and not yet seen on screen. A person who has a running site must confirm each item before the
recording, and tell the author if the screen differs from the narration.

## What the narration says that comes from code only

- **The term editor shows a single Publish button.** `OrchardCore.Contents/Drivers/ContentsDriver.cs` adds Publish,
   Unpublish, Save Draft and Delete buttons to every content editor, and the Taxonomies editor for a term
   (`OrchardCore.Taxonomies/Views/Admin/Create.cshtml`, `Edit.cshtml`) shows that same editor. For the Topic type
   (not draftable, and the term has no database id) the code leads to Publish alone. `add-2` and `edit-1` say
   "select Publish". Confirm the button is there, its label is exactly "Publish", and that no Unpublish, Save Draft or Delete
   button shows on a term.
- **Terms go to a draft, and the list shows two badges.** `OrchardCore.Taxonomies/Controllers/AdminController.cs` loads the
   taxonomy with `VersionOptions.DraftRequired` on every add, edit and delete. `add-2` ("a draft of the taxonomy") and
   `why-3` rely on it, and `pub-1` says the Topics row shows a **Published** and a **Draft** badge
   (`OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml`). Confirm both badges after the first term is added,
   and that only **Published** shows after `pub-3`. Confirm also that the editor page itself shows no draft indicator
   (the narration does not mention one).
- **Deleting a term deletes the terms inside it** (`RemoveTaxonomyItem`, `AdminController.cs`). `edit-3` says it. Confirm
   on a scratch taxonomy, not in the take: delete a term that has a child, and check that the child is gone.
- **Drag and drop.** The list uses a mouse-simulated Sortable list (`forceFallback`, `OrchardCore.Taxonomies/wwwroot/Scripts/menu/menu.js`).
   `org-1` and `org-2` say that the children move with a parent, that a sideways movement of more than 40 pixels changes the
   level by one, and that the page warns before it is left. Confirm with the recorder's `DragToAsync`:
   - the offsets in `recording-plan.md` (`0, -14` and `70, -14`) give the depths 0, 1, 0, 1, 1, 0 and then 0, 1, 1, 0, 1, 1;
   - Herbs lands between Vegetables and Cooking and is indented like Vegetables, not under Vegetables;
   - the hidden field `TaxonomyPart.Hierarchy` is filled after each drop;
   - the leave-page warning does not appear in the take (the clip never leaves the page after a drag except by Save Draft).
   The wording of the leave-page warning is the browser's, not the page's: the narration only says "the page warns you".
- **Save Draft keeps the editor open.** `org-3` and `pub-1` assume the Topics editor was opened without a `returnUrl`, so
   Save Draft redisplays the editor (`OrchardCore.Contents/Controllers/AdminController.cs`, `EditInternalAsync`) with
   "Your Taxonomy draft has been saved." Confirm the message and that the saved order is the dragged order.
- **The first Publish returns to the list.** `create-4` and `pub-3` assume that a Publish pressed on an editor opened from
   the New button or from the list (which carry a `returnUrl`) returns to the Content Items list, with the message
   "Your Taxonomy has been published." Confirm, and confirm that the Topics row is the first row (sorted by last modified).
- **The Delete dialog.** `edit-2` says "A dialog asks you to confirm", and `edit-3` starts with "Select Ok". The text comes from the defaults in
   `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml` (title Delete, "Are you sure you want to remove this element?", Ok, Cancel).
   Confirm the dialog opens on the taxonomy editor and that the message after Ok is "Taxonomy item deleted successfully."
- **The fields of the Taxonomy editor.** `create-2` says the editor has a title, an alias and a permalink, and that the
   alias and permalink are generated from the title (patterns in `OrchardCore.Taxonomies/Migrations.cs`). Confirm the three
   fields show, in that order, the Permalink field is disabled with the hint "The url of the content item. It will be
   automatically generated.", and that an unchecked "Route contained items" box shows under the permalink (read in the code:
   `AutoroutePart.Edit.cshtml:94` needs a contained items accessor, which `TaxonomyPartHandler` adds, and the Taxonomy type allows
   contained item routing, `Migrations.cs:40`). The narration does not mention the box; leave it as it is.
- **The options of Term Content Type.** `create-3` says the list leaves out special-purpose content types that have a stereotype, such
   as widgets (`TaxonomyPart.Edit.cshtml` filters with `HasStereotype`). Confirm Topic is in the list, a widget type
   (for example a Html Widget) is not, and the required field message is "The Term Content Type field is required." when the
   field is left empty.
- **The Add Topic button** appears only when the taxonomy is saved and has a stored Term Content Type (`add-1`,
    `create-4`). Confirm it is absent on the New Taxonomy page and present on the editor after the first Publish, and that
    its label reads "Add Topic".
- **Terms are not content items of their own** (`why-3`): the code stores them inside the taxonomy. Confirm that no
    "Cooking" or "Gardening" item shows in the Content Items list.
- **Feature and permission** (`why-4`): Tools, Features lists "Taxonomies" (`Manifest.cs`), and the role editor lists the
    permission as "Manage taxonomies" (`Permissions.cs`), granted to Administrator by default. Confirm both labels on
    screen.

- **Success messages are toasts that hide after 5 seconds.** `SuccessAsync` gives every success message a 5000 ms delay
  (`NotifierExtensions.cs:71`), `Message.cshtml` passes it to a Bootstrap toast, and `NotifyMessages.ts` removes the toast when
  it hides. `edit-3` names the message in its second sentence (after "Select Ok.") and `pub-3` in its second, so the toast has to be on
  screen then: `edit-3` between about 1 and 3.5 seconds into the step, `pub-3` between about 1 and 4 seconds into the step. Confirm on the
  take that the toast is visible while the sentence is spoken, and that the ring `edit-3` draws around it is cleared before the
  toast goes. The messages of `create-4` and `org-3` are not narrated and may be gone by the end of the step.
- **The seed.** The plan creates the Topic type with a `PrepareAsync` method that sets the text of the CodeMirror editor
  of JSON Import (`import-json.ts` creates it with `CodeMirror.fromTextArea`, which leaves the instance on the `.CodeMirror`
  element). Confirm that the method imports the type, and that Design, Content Definition, Content Types lists Topic afterwards.

## Recording details to confirm

- The selectors in `recording-plan.md` (names such as `TitlePart.Title`, `TaxonomyPart.TermContentType`,
  `AutoroutePart.Path`, `AliasPart.Alias`, and `ol#menu > li.menu-item`) come from the views and the display-driver
  prefixes, not from a page. Check each with `Recorder probe`.
- The Auto Setup environment variable names come from `src/docs/reference/modules/AutoSetup/README.md`; the code also
  reads the older section name `OrchardCore_AutoSetup`.
- Whether the Content Items page is a list or a grid, and the exact place of the Status badges and of the New button, in
  the admin theme of the checkout used for the take.
- Whether the Playwright click on a row's Add, Edit or Delete button needs the row scrolled into view when the list of terms
  grows past the viewport (the last clips hold 6 to 7 rows).

## Speech to check with `scripts/pronounce.py`

No `spoken` entry was added to `storyboard.json`. A person should listen to how the voice reads the words a viewer must
recognize, and add an entry only if one is misread: "Ok" (`edit-3`), "Permalink" (`create-2`), "Taxonomy" and "Taxonomies"
(throughout), "Herbs" (`add-6`, `org-2`), and "Barbecue" (`edit-1`). The narration uses the labels as the screen spells them.

## Also confirm

- `org-3` says Save Draft keeps the order in the draft. Confirm that the editor reopens in the dragged order after Save Draft
  (the order `pub-2` shows) and that the Topics row then holds both a Published and a Draft badge.
- The pauses of `recording-plan.md` assume 150 words per minute. After `narrate.py`, compare them with `durations.json`.
