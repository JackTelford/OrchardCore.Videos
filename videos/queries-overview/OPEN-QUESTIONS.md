# Open questions: queries-overview

Everything in the narration and on the slides was read from the Orchard Core source (the recording plan lists the
`file:line` of each label and fact). The points below could not be checked without a running site or the voice, so a
person confirms them while recording. None of them changes the narration unless the answer is "no".

## Voice

1. **Pronunciation.** Check these with `scripts/pronounce.py` before narrating. The storyboard's `spoken` map holds the
   spellings: `IQuerySource` as "I Query Source", `AddQuerySource` as "Add Query Source", `ExecuteQueryAsync` as "Execute
   Query Async", `IQueryHandler` as "I Query Handler", `DisplayDriver` as "Display Driver", `RecentBlogPosts` as "Recent
   Blog Posts", `PublishedBlogPosts` as "Published Blog Posts", and "Execute Api - " (the start of the permission labels
   in `run-2`) as "Execute A.P.I., ", so the voice spells out the three letters of "Api" and pauses at the dash instead of
   reading "minus". Also listen to words the map does not touch: SQL, Lucene, Elasticsearch, GraphQL, Liquid and Razor.

## The admin menu (steps `screen-1`)

2. **How the groups open.** The recording plan clicks **Search**, then **Queries**, then **All Queries**. The labels, the
   nesting and the other items of the two groups (**Indexes**, **Run SQL Query**) are read from the menu builders. What is
   not in the source is the click behavior of a group: it comes from the admin script, and the menu can be `persistent` or
   `focused` (`src/OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml:24`). Confirm on the site that clicking the label of
   a group opens it, and that the click on **Queries** does not leave the page.

## The pages (steps `screen-2`, `screen-3`, `create-1` to `create-6`)

3. **Layout and badge.** The plan expects the **List** layout (the default of `AdminListOptions`), with the source badge in
   the row and the buttons **Run**, **Edit**, **Delete** in that order (zones `Actions:2` and `Actions:5`). Confirm the order
   on screen. If a different layout is the default of the site, the rows are table rows and the selectors
   `.list-group-item` and `.ta-badge` do not match.
4. **One card.** Confirm that the **Available Query Types** dialog of the Blog site shows only the **SQL** card (the Blog
   recipe enables no other source). The narration says so.
5. **Title of the create page.** The page title is built as `New {0} query` from the source name (`Sql`), so it should
   read **New Sql query**. The narration only says that the title names the source.
6. **Field locators.** The plan finds **Name** and **Return Documents** by their labels. Confirm that each label matches one
   element, and note the ids of the inputs (the editor prefix is not visible in the source).
7. **Typing into the Query editor.** The editor is CodeMirror (`codemirror-query-editor`); the component that builds it
   (`@orchardcore/bloom`) is not in the source, so the selector `.CodeMirror` of the plan is unverified. Confirm that the
   selector matches one element, that typed text arrives
   unchanged (no automatic closing of the quotes around `BlogPost`), and that **Save** accepts
   `select DocumentId from ContentItemIndex where Published = true and ContentType = 'BlogPost'` and shows **Query created
   successfully.** The query is the example of the module's documentation; the check is `SqlParser.Validate`
   (`src/OrchardCore.Modules/OrchardCore.Queries/Sql/SqlParser.cs:33`), which relies on a SQL parser package whose grammar is
   not in the source. If the parser reports an error, the message shows
   under the editor and the plan's step `create-4` needs another text.
8. **The message.** The message **Query created successfully.** must still be on screen when the narration of `create-5`
   ends (about 10 seconds after the page loads). If it disappears earlier, highlight the new row only and drop the mention
   of the message from the narration of `create-5`.
9. **The search.** The plan types `Published` into the search box and clicks the search button, which posts the form and
   redirects to the list with the text in the address (`AdminController.cs:111`). Confirm that the page then lists only
   **PublishedBlogPosts** and that the box still holds `Published`.

## Facts of the narration checked in the source only

10. **The Blog theme runs RecentBlogPosts by name.** `Content-BlogPost.liquid` of the Blog theme assigns
    `Queries.RecentBlogPosts | query` (line 34). The narration relies on the template, not on what a blog post page shows.
    Nothing is recorded on the front end.
