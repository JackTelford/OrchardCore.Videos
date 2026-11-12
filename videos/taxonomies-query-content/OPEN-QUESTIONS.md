# Open questions: Taxonomies in Code: Query Content Items by Term

Everything in the narration was read in the Orchard Core source (`src/OrchardCore.Modules/OrchardCore.Taxonomies`,
`OrchardCore.Lucene`, `OrchardCore.Queries`, `OrchardCore.Indexing`, and `src/OrchardCore/OrchardCore.Taxonomies.Core`,
`OrchardCore.Lucene.Core`), not run. **No site was started to write this video.** A person must confirm the items below on a running site
before recording, and fix the narration (and `slides.py`) if one of them turns out differently.

## 1. The Lucene query, end to end (chapters 03 and 04)

Checked in the source: the field handler writes `{type}.{field}.Ids` and `{type}.{field}.Inherited` as keyword and stored entries
(`TaxonomyFieldIndexHandler.cs:24,31,49`); `Content.ContentItem.ContentType` is a keyword entry without a `.keyword` copy
(`DefaultContentIndexHandler.cs:16`, `LuceneIndexManager.cs:377-383`); the `match` provider tokenizes its text with the index analyzer and joins the
tokens with `or` unless `"operator": "and"` (`MatchQueryProvider.cs`); the `bool` provider reads `must` as an array; the query text is rendered by Liquid
and then parsed as JSON (`LuceneQuerySource.cs`, `Lucene/Controllers/AdminController.cs:170-190`).

To confirm on a site:

- With `{ }` as parameters, the template returns the four blog posts ("Found 4 result(s)"); with `{"categories":"<politics-id>"}` it returns the two Politics
  posts. If the count is 0, check first that the `Search` index was rebuilt after the two fields were included, then the field names in the **Lucene Query**
  table (the stored columns are the exact names to use).
- A 26-character term id is kept as a single token by the `standardanalyzer` of the `Search` index. If it is not, replace the `match` clauses with
  `terms` clauses (`TermsQueryProvider.cs`), which compare exactly, and change the `template-1` and `template-2` slides and the plan with it.
- Several ids joined with a space in one parameter (the `razor-1` narration) return the posts of either term: try `"<politics-id> <travel-id>"`.
- `razor-1` passes `string.Join(' ', Model.TermContentItemIds)`. For a post without a category that is an empty string. In Liquid only nil and false are
  falsy, so an empty string probably makes `{% if categories %}` true; the `match` clause then has no tokens and the query returns no post instead of every post.
  The slide shows no guard because Fluid's rule for an empty string was not read: confirm it, and decide whether the slide needs one.
- A parameter that is left out makes `{% if categories %}` false. This is Liquid's behavior for an undefined name; the Liquid options of the site were not read.
- `/Admin/Lucene/query/Search?query=<base64>&parameters=<json>` runs the query as it opens (the `QueryIndex` action calls the POST overload), and the route
  resolves to `QueryIndex`, not to `Query`, because `{indexName}` is required while `{id?}` is optional. The **Run** button builds the same kind of address.
- **Save Query** is only shown when the result has documents, and the Create form takes the template from the address
  (`LuceneQueryDisplayDriver.EditAsync`). The saved query keeps the Liquid tags.
- **Rebuild**, in the Actions menu of **Search > Indexes**, probably posts at once, without the confirmation dialog. The link carries `data-title`, `data-message`,
  `data-ok-text` and `data-cancel-text` (`IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:25-32`), but its `data-url-af` is `UnsafeUrl` only. The admin script
  (`TheAdmin.ts:118-184`) opens the dialog only for `RemoveUrl` or for `data-unsafe-url`, and the page script (`indexing-admin-index`) only handles the bulk actions
  of selected rows. So `index-3` and `index-4` name no dialog. Confirm on the first take that the click on **Rebuild** reloads the list with the notification; if a dialog
  opens, the plan says to click **Yes**, and the narration of `index-4` ("Choose Rebuild.") stays true. Confirm too that this is the intended behavior of the list.
- On the **Lucene Query** page the **Parameters** text is deserialized into a dictionary of objects and wrapped with `FluidValue.Create` (`Lucene/Controllers/AdminController.cs`, `QueryIndex`/`Query`). That a JSON string value then renders in `{{ categories }}` as the plain id, and that `{% if categories %}` is false when the key is absent, was not verified in the Liquid options of the site: confirm on the first take that `run-2` returns the two Politics posts.
- The field keys: the field handler writes `{key}.{field}.Ids` for the type name and for the part name (`ContentFieldIndexHandler.cs:35-44`). On a Blog Post both are `BlogPost`, so `BlogPost.Category.Ids` is the one name the video shows.
- The template sets `"size": 10`, so the query returns at most ten posts. `query-1` says "up to ten"; the site has four posts, so the page shows "Found 4 result(s)".
- The result table of the **Lucene Query** page lists every stored field of the hits. Check that the `BlogPost.Category.Ids` column is readable on screen;
  if the table is too wide, the narration of `run-2` ("including the term ids") needs a highlight or a scroll in the plan.

## 2. Statements deliberately left out

Not verified in this source tree, so the video does not say them:

- That a query on `TaxonomyIndex` returns an item twice when its published version and a draft both match. The index has rows for both
  (`TaxonomyIndexProvider.cs:29`) and the helper adds no filter (`TaxonomyOrchardHelperExtensions.cs:59-67`), but how YesSql returns the documents was not read.
  The narration only says to add `Published` or `Latest`.
- Whether a term page lists draft items. `DefaultContentsTaxonomyListQueryService.cs:31-48` adds no condition on `Published`, and nothing else was found that does.
- Anything about the Elasticsearch module, and the behavior of an empty array as a Liquid parameter (the video passes strings).

## 3. Pronunciation (`spoken` in `storyboard.json`)

The map is applied with a plain text replacement, in the order written. Each spelling must be confirmed by a person with `scripts/pronounce.py` before narrating:

- `QueryCategorizedContentItemsAsync`, `ContentQueryAsync`, `IContentsTaxonomyListFilter`, `TaxonomyIndex`, `TaxonomyField`,
  `GetBlogsByFilter`, `TermPart`, `ISession`, `DisplayText`.
- `Ids` is spoken `I.D.s` (it appears in `keys-1`). The lower case "id" and "ids" in the other steps are left to the voice: check that it says "I D" and "I D s".
- The voice's reading of "Lucene", "Liquid", "Razor" and "JSON" (`run-1`).

The narration counts 692 words (4:37). Identifiers take longer to say than their word count suggests, so the narrated length is expected to be a little over 5 minutes.

## 4. The seed data and the recording plan

The plan's seed steps were written from the views, not run:

- **Content > Taxonomy** exists because the `Blog` recipe's admin menu step adds it (`blog.recipe.json:1283`).
- The term form buttons (**Publish**) and the **Category** and **Tags** editors of a post were not read in detail. The **Category** field is a single choice (`Unique` is
  checked by the recipe), and the **Tags** field takes typed tags.
- The selectors of the plan (`label:text-is(...)`, `button.dropdown-toggle.actions`, the CodeMirror boxes next to `textarea[name='DecodedQuery']` and
  `textarea[name='Parameters']`, the dialog button `button#modalOkButton` of the recipe run) come from the Razor views and the admin script, and may need a small
  change on the first take.
- The time the rebuild takes before `Found 2 result(s)` appears was not measured.

## 5. Code on the slides

The slides show source trimmed with `// ...`, except three snippets written for the video from public members read in the source: the one-line call on
`sql-1`, the `TermContentService` class on `sql-5`, and the Razor and Liquid calls on `razor-1` and `razor-2`. The `sql-2` and `sql-3` view is the documentation's
example with `index.Published &&` added. A person should compile them once:

- In a `TaxonomyField.cshtml` view, `@using YesSql.Services` and the lambda with `Published`, `IsIn` and `Model.Part.ContentItem.ContentItemId`.
- `ISession.Query<ContentItem, TaxonomyIndex>(predicate)` and `IContentManager.LoadAsync(IEnumerable<ContentItem>)`
  (`OrchardCore.ContentManagement.Abstractions/ContentManagerExtensions.cs:29`).
- The file name `TaxonomyField.cshtml` is the module's own view (`Views/TaxonomyField.cshtml`). That a theme replaces it by using the same file name was not checked here.
- The Liquid call uses `Queries.GetBlogsByFilter | query: categories: "..."`, as `QueryFilter.cs` and the Queries documentation have it.
