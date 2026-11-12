# Open questions: Content Items List: Search, Filter and Act on Many Items

What was checked in the Orchard Core source (commit 92db4a83), and what a person must confirm on a running site while
recording. Paths are relative to `src/OrchardCore.Modules/OrchardCore.Contents/` unless they name another module. None of
these blocks the narration: it claims only what the source shows. The point of each question is to catch a frame that
contradicts a narrated sentence.

## Search

1. **The search syntax.** Checked: the dialog "Available Filters" documents it with the examples `text:foo bar`,
   `text:foo AND bar`, `text:foo NOT bar`, `text:((foo OR bar) NOT baz)` and `text:"foo bar"`
   (`Views/ContentsAdminListSearch.cshtml:63-84`); a word typed without a term name is the default term "text"
   (`Services/DefaultContentsAdminListFilterProvider.cs:296`). The parser is YesSql's, which is not in this source tree; its
   rules were read from the decompiled `YesSql.Filters.Query` and `YesSql.Filters.Abstractions` 6.2.0 assemblies of the
   local NuGet cache (the version that this commit pins is not in the tree): a space between words is OR, `AND` and `NOT`
   (uppercase) are binary operators, parentheses group, and a quoted text is one value that is matched as a substring.
   The term name is optional for the default term. Confirm: typing `schedule` finds 3 items, and typing
   `schedule NOT winter` finds 2 (the plan's seed data). If the NOT form behaves differently, change the typed text in
   `03-search` and the narration of `search-4` and `search-5` accordingly.
2. **Case.** The search is a "contains" on the display text (`DefaultContentsAdminListFilterProvider.cs:296-300`); whether
   it ignores case depends on the database. The narration does not claim it either way. Checked: YesSql renders `Contains`
   as `like '%text%'`, and the SQLite provider declares its text columns `TEXT COLLATE NOCASE`, so the lowercase word
   `schedule` should find the titles that hold "Schedule" on the SQLite database of the recording. Confirm it. (Another
   database may behave differently; the video is recorded on SQLite.)
3. **What the search box shows after a search.** Checked: after the redirect it holds the parsed filter
   (`Controllers/AdminController.cs:226`, `options.SearchText = options.FilterResult.ToString()`). A default term is written back as it was
   typed (`DefaultTermNode.ToString` returns the operation). Confirm the exact text for `schedule NOT winter` and for the
   picker terms (`type:Article`, `status:Draft`, `sort:Title`), and that
   a second picker choice keeps the first one's term (the form posts the search text with the pickers,
   `Controllers/AdminController.cs:316-336`).

## Filters and the list

4. **"All versions".** Checked: the option "All versions" runs the same filter as "Latest"
   (`DefaultContentsAdminListFilterProvider.cs:41-44`), so it does not list every version. The narration does not name it.
   Confirm on the site; the documentation does not describe this option either.
5. **Page title when a type is selected.** Checked: the title is "Manage Content" (`Views/Admin/List.cshtml:17`), and the
   source suggests that choosing a type changes it to the type's display name with a "Manage Content" breadcrumb
   (`Views/Admin/List.cshtml:12-40`). The narration does not mention it; confirm so the contact sheet of `filter-1` is
   not a surprise.
6. **The type badge link.** Checked: the badge on a row is a link to the list with the selected type
   (`Views/ContentsType_SummaryAdmin.cshtml:6`), and it shows the type's name, not its display name. Confirm that
   clicking "Article" lists the 9 articles (`filter-6`).
7. **The first rows after the seed.** Checked: the default order is the date of the last modification, newest first
   (`DefaultContentsAdminListFilterProvider.cs:97,306`). Confirm the order of page 1 listed in `recording-plan.md`, that a
   published item with a newer draft shows both badges ("Published", "Draft"), and that the Edit, View and Preview Draft
   buttons and the Actions button show on the first row (`Views/ContentsButtonEdit_SummaryAdmin.cshtml`). The narration
   names Edit, View and Actions only.
8. **The layout buttons and the pager.** Checked: the List and Grid buttons appear when the setting **Let users choose the
   layout of a list** is on, which is its default (`OrchardCore.Admin/Configuration/AdminListOptionsConfiguration.cs:40`,
   `OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml`), and the Grid columns are Title, Type, Status, Last modified and
   Actions (`Views/ContentsAdminList.cshtml:35-39`). Confirm the buttons sit at the end of the row that holds the item count and the three pickers (`OrchardCore.Admin/Views/AdminList.cshtml`, the layout selector after the toolbar content), and that the pager shows
   with 15 items and a page size of 10.

## Acting on many items

9. **The Actions menu appears only with two or more ticked boxes.** Checked in the bundled script
   (`wwwroot/Scripts/contents-admin-list/contents-admin-list.js`; its source, the bulk select list component of
   `@orchardcore/bloom`, is not in this source tree): with one box ticked the filters stay, with two the pickers hide, the
   label "N selected" replaces the count and the Actions button shows. Confirm in `bulk-1`, where the narration says that
   one selected item shows no Actions menu: after the first tick the pickers must stay and the Actions button must stay
   hidden (only the select-all box changes, to a partial tick).
10. **The confirmation dialog.** Checked: title "Bulk Action", message "Are you sure you want to {0} these items?" with the
    lower-cased label of the command (`Views/ContentsAdminListBulkActions.cshtml:5`), and the buttons "Ok" and "Cancel"
    from the default of the admin layout (`TheAdmin/Views/Layout.cshtml:99`). Confirm the wording on screen, and that
    "Unpublish" and "Delete" give "Are you sure you want to unpublish these items?" and "... delete these items?".
11. **The messages and the reload.** Checked: "Content published successfully." and "Content removed successfully."
    (`Controllers/AdminController.cs:366,398`), then a redirect to the plain list (`:404`), so the filter is dropped.
    Confirm that the toast shows long enough for `bulk-4` and `bulk-7`. Checked: publishing does not change the
    modification date (`OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:80-92` sets only `PublishedUtc`,
    and `DefaultContentManager.PublishAsync` runs no update handler), so the two published schedules keep rows 2 and 3.
    Confirm it in `bulk-4`.
12. **Seeding.** Confirm that an Article and a Page can be saved with only a Title (no other required field), that
    "Save Draft" on an Article opened from the list (with a return address) saves a draft and keeps the published
    version, and that no extra entry appears in the Actions menus.
13. **The admin menu.** Checked: the Blog recipe adds its own admin menu (`Blog`, `Main Menu`, and under `Content` the entries
    `Content Items`, `Article`, `Page`, `Taxonomy`; `TheBlogTheme/Recipes/blog.recipe.json`, the `AdminMenu` step), and entries
    with the same caption are merged (`OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`). Confirm that
    "Content Items" appears once under "Content", that clicking "Content" only opens the section, and what address the click
    leads to (`/Admin/Contents/ContentItems`, possibly with a trailing slash). The narration names only "Content" and
    "Content Items".
14. **A type typed by hand.** Checked: the unfiltered list shows only listable types the role may view, but an explicit
    type (`type:BlogPost` typed in the search box, or a link with a type) lists that type even when it is not listable
    (`DefaultContentsAdminListFilterProvider.cs:145`). The narration of `list-2` describes the list as the video shows it and
    never types a type by hand. No action unless the narration is extended.
15. **Icon-only buttons.** Checked: the Filters button, and the List and Grid buttons, show an icon only; their names are
    tooltips and accessible labels (`Views/ContentsAdminListSearch.cshtml:16-18`,
    `OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:11-14`). The narration of `search-3` calls the Filters button
    "the funnel at its start", from its icon (`fa-filter`). Confirm that the icon looks like a funnel on the site.

## Voice

16. **Pronunciation.** No `spoken` entry is needed by the spelling of any word, but a person should listen to the first
    take of `narrate.py` for three words before the recording: "Ok" (`bulk-3`, `bulk-7`), which the voice might spell
    letter by letter, and the capitalised "AND" and "NOT" (`search-4`, `search-5`), which it should read as the words.
    Check them with `scripts/pronounce.py`; if "Ok" is spelled out, add `"Ok": "okay"` to `spoken` in `storyboard.json`.
