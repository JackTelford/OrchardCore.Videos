# Open questions: Content Items: Create, Edit and Publish

Everything in the narration comes from the Orchard Core source (commit `92db4a83`) and its documentation. The items
below could not be confirmed without running a site. A person who records the video confirms each one on the running
site and writes the answer under it.

## What the narration says without being able to see the screen

1. **The body editor's label.** The narration calls it "the body" and "the rich text editor", and never speaks its
   label. The editor's label is `TypePartDefinition.DisplayName()` (`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22`).
   The Blog recipe sets no display name for the body part of the Article type, and the part has none either
   (`OrchardCore.Html/Migrations.cs:32`), so the code falls back to the part name without "Part"
   (`OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs:7-24`, which calls
   `.../Metadata/Settings/ContentPartSettingsExtensions.cs:54-61`). Expected
   label: `HtmlBody`. Check what the editor shows. If it is a readable name, the narration of `create-3` could name it.
2. **The admin menu path.** `list-1` says "open Content, then Content Items". The Contents module adds
   **Content > Content Items** (`AdminMenu.cs:65,68`) and the Blog recipe adds its own **Content** group with
   **Content Items**, **Article**, **Page** and **Taxonomy** (`blog.recipe.json:1237-1301`); `NavigationManager.Merge`
   joins items with the same text. Confirm that the sidebar shows one **Content** group with one **Content Items** entry,
   and whether clicking **Content** opens its sub-menu or goes straight to the Content Items page (the recording plan
   accepts both).
3. **The list's rows at the start.** `list-1` and the end of `remove-4` assume five rows: `About`, `Blog`, `Tags`,
   `Categories` and `Main Menu`, and no others. The Menu type is listable (`OrchardCore.Menu/Migrations/menu.recipe.json:11`)
   while the Blog Post and the Footer widget are not. Confirm on the fresh site, and which of the five comes first (the
   narration does not depend on the order).
4. **The New menu's entries.** `list-4` expects **Article**, **Blog**, **Page** and **Taxonomy**. The narration only
   needs **Article** to be there.
5. **The address of the Article.** `view-2` says the address comes from the permalink generated from the title.
   Expected: `spring-garden-checklist` (pattern `blog.recipe.json:158`, the `slugify` filter). Confirm in the
   **Permalink** field when the editor opens in `edit-1`, and that the caption in `view-2` matches.
6. **The front page of an Article in the blog theme.** `view-2` says the page shows the title, the subtitle and the
   body (`TheBlogTheme/Views/Content-Article.liquid`). Confirm the three are visible without scrolling.
7. **The relative time on a row.** `list-2` says a row shows "how long ago the item changed". The badge renders a
   `Timespan` shape (`ContentsMeta_SummaryAdmin.cshtml:10-11`) whose wording is "N minutes ago", "1 hour ago",
   "a moment ago" and so on (`OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:43-112`). Confirm what a row shows
   right after the item was saved.
8. **The confirmation dialogs.** The texts and button labels come from `data-*` attributes (`Yes`/`No` for Unpublish,
   `Ok`/`Cancel` for Delete); the dialog itself is drawn by a script. Confirm the dialogs show them.
9. **The toasts.** Success messages disappear after 5 seconds and sit at the right edge of the window
   (`TheAdmin/Views/NotifyMessages.cshtml:3-12`; the code sets no `top`, so the vertical position depends on the layout).
   Confirm where they appear, and that each one is visible while the sentence that
   mentions it is spoken (`create-6`, `edit-2`, `remove-1`, `remove-4`).
10. **Unpublished items are not served.** `remove-2` says visitors can no longer open the page. The published routes
    are built from versions with `Published` set (`OrchardCore.Autoroute.Core/Services/AutorouteEntries.cs:181`) and
    the unpublish clears it (`DefaultContentManager.cs:488`). Not shown on screen; confirm by opening the address after
    `remove-1`, outside the capture.
11. **The order of Subtitle and Banner Image.** Both fields of the Article part have the position 0
    (`blog.recipe.json:680-700`), and the body (position 2) comes before the Article part (position 3). The narration of
    `create-3` says the body, then the subtitle, then Banner Image. Confirm the order on the editor.
12. **The controls around the list.** Besides the Search box, the Filters menu and **New**, the page shows selectors for
    the content type, the status and the sort, the bulk actions and a selector for the layout of the list. The narration
    names none of them. Confirm they do not hide or cover the controls the clips use.
13. **The "and continue" arrows.** `create-5` says the arrow opens one more choice, "and continue", and that choosing
    it keeps you in the editor. The label comes from `Content.PublishButton.cshtml:24` and `Content.SaveDraftButton.cshtml:24`, and the redirect to
    the editor from `AdminController.cs:800-807` with `ContentsHandler.cs:57-62`. The plan never chooses it, because the
    choice is stored in the admin preferences and changes the main button (`actionGroup.ts`). Confirm the menu opens
    with that single item.
14. **The Filters button.** `list-3` names the Filters menu, which is a button with a funnel icon and the tooltip
    **Filters** (`ContentsAdminListSearch.cshtml:17`). The plan rings it with the caption `Filters` and does not open it.
    Confirm that the ring and the caption leave the button recognizable at the size of the recording.

## Pronunciation to confirm before narrating

Check these with `scripts/pronounce.py` before running `narrate.py`; no `spoken` entry was added because a person has
to confirm each spelling: "Creatable", "Listable", "Draftable", "Permalink", "Unpublish", "unpublishing", "UI"
(`intro`), "Ok" (the button of the delete dialog, `remove-4`; the voice should say "okay"), and the phrase "and
continue" in `create-5` (the label is lower case and starts with a conjunction; the comma before it should give the
voice a pause).
