# Open questions: Lucene Query Syntax

What the author could not confirm on a running site, and what a person must check before or while recording.
Source paths are relative to the `src` folder of `OrchardCMS/OrchardCore` (the commit in `catalog/SOURCE.md`).

## 1. Parser behavior that comes from the Lucene.Net library, not from Orchard Core's source

Orchard Core's code only builds the parser and calls it (`OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:71, 75`,
with `StandardAnalyzer` registered at `OrchardCore.Modules/OrchardCore.Lucene/Startup.cs:47-48`). The rules below are the
Lucene.Net classic query parser's and the standard analyzer's. They were checked by running those two classes in a
throwaway console program (the Lucene.Net 4.8 beta packages, a `MultiFieldQueryParser` over the field
`Content.ContentItem.FullText`, the standard analyzer, an in-memory index of the seed posts and of the items the Blog
recipe creates, once with parser version `LUCENE_30`, which the recipe stores, and once with `LUCENE_48`). That
program is not part of the repository. Confirm each line on the running site, in the search box of `/search`:

| Claim in the narration | What to confirm |
| --- | --- |
| Only capital `AND`, `OR`, `NOT` are operators (`op-2`, `op-3`, `habits-1`) | `quince AND cider` lists one post; `quince and cider` and `quince not pruning` list what a query of plain words lists |
| The analyzer drops common English words such as and, or, not and the, and lowercases terms (`parse-2`, `op-3`) | `quince and cider` lists the same posts as `quince cider`; `QUINCE` lists the same posts as `quince` |
| A hyphen inside a word excludes nothing (`op-5`, `habits-1`) | `quince-pruning` does not behave like `quince -pruning`, which lists one post |
| A wildcard cannot start a term (`wild-3`, `habits-1`) | `*ince` and `?uince` list nothing, and the server log holds "Incorrect Lucene search query syntax provided in search." |
| `~` without a number allows two edits (`wild-4`) | `quinse~` and `quinse~2` list the same two posts |
| A regular expression must match a whole term (`wild-5`) | `/pr.*ing/` lists three posts, `/pr.*/` also lists the other items that contain a term starting with pr |
| Only the term after the colon belongs to the field; parentheses group terms (`fields-3`) | `Content.ContentItem.DisplayText.Analyzed:fruit trees` lists three posts, with parentheses it lists two |
| `[p TO r]` is a range, `p TO r` without brackets is not (`fields-4`, `habits-1`) | The range lists three posts; `p TO r` is read as the terms p and r and lists nothing in the harness. On the site the indexed full text may hold a `p` token from the HTML of the body (see 4), so this one check may list posts: do not record it, and do not change the narration unless the range with brackets differs |
| `*:*` matches every document of the index (`saved-2`) | `*:*` lists the posts, and also the other items the blog recipe creates |

A second check of the whole table of results in `recording-plan.md` (every query, with the parser version `LUCENE_30` and `LUCENE_48`, over the six seed posts and the items the Blog recipe creates) gave the posts the plan lists. Two details of that run matter when recording: `quince-pruning` lists posts that contain either word with `LUCENE_48`, and none with `LUCENE_30`, which reads it as the phrase "quince pruning"; and the full text holds the token `p` from the HTML of every post body, so a bracketless `p TO r` lists most posts. The video shows neither query.

## 2. Index settings that the search page does not seem to read

- `Allow Lucene queries` (`OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneQuerySettings.Edit.cshtml:31`): the property
  `AllowLuceneQueries` is written by the index form and by the recipe (`blog.lucene.search.recipe.json:53`, set to
  `false`), but no code path of the search page or of `LuceneSearchService` reads it. The narration does not mention it.
  Confirm that `quince AND cider` lists one post on the recorded site, where the box is unchecked.
- `Query analyzer` (`LuceneQuerySettings.Edit.cshtml:12`): `LuceneSearchService.cs:55` takes the analyzer from the
  **Analyzer** of the index (`LuceneIndexMetadata.AnalyzerName`), not from `QueryAnalyzerName`. The narration says "its
  analyzer" only.
- `Version`: `LuceneDefaultQueryViewModel.DefaultVersion` has an `internal` setter, so the index form cannot bind it
  (`OrchardCore.Modules/OrchardCore.Lucene/ViewModels/LuceneDefaultQueryViewModel.cs:13`); an index saved from the form
  probably stores `LUCENE_30`, the default of the enum, as the recipe does. Every query of the plan behaves the same with
  `LUCENE_30` and `LUCENE_48`. A hyphenated word is the exception (`quince-pruning` becomes a phrase with `LUCENE_30` and
  two alternative terms with `LUCENE_48`), and the video does not show one.

## 3. Pronunciation (check each with `scripts/pronounce.py` before narrating)

- Capital `AND`, `NOT`, `OR` and the word `TO` (`op-2`, `op-5`, `fields-3`, `fields-4`, `habits-1`): the voice must say the
  words, not spell them.
- The identifiers `Content.ContentItem.DisplayText.Analyzed`, `Content.ContentItem.DisplayText.Normalized` and
  `Content.ContentItem.FullText`, spoken as "Content dot Content Item dot Display Text dot Analyzed", and so on
  (`fields-1`, `fields-4`).
- `MultiFieldQueryParser` ("Multi Field Query Parser"), `QueryParser`, `query_string` ("query string") and `*:*`
  ("star colon star").
- The spelled letters "p r u n", "i n c e", "p r" and "i n g" (`wild-2`, `wild-3`, `wild-5`), and the misspelling "quinse".
  The captions keep the written text.
- The `spoken` map replaces text with `str.replace` in the order of the keys: `MultiFieldQueryParser` is listed before
  `QueryParser` on purpose.

## 4. The page and the index on a running site

- The search page has a tall header, so the results start low on the screen. The plan scrolls by 420 pixels after every
  search; confirm that the box and the longest result list (three posts) fit together, and adjust the offset.
- The body of a Markdown post is indexed as HTML (the `Sanitize` option of `FullTextContentIndexHandler.cs:36` is not read
  by `LuceneIndexManager`), so the index holds a `p` token. None of the queries of the plan is affected; the
  bracketless `p TO r` of item 1 could be.
- The seed posts are imported without a container and then indexed by the rebuild of the recipe. Confirm that the
  imported posts show in the summaries (title, subtitle, **Posted by admin**) without an error, and that the first
  `/search?terms=quince` lists two posts without waiting for a background worker.
- `state.json` is a signed-in session. Confirm that the public search page draws no toolbar for it; if it does, record
  the four clips without the sign-in.
- **The recipe group on Tools > Recipes.** The plan says the group header is **The Blog Theme**, because the page uses the
  name of the feature found for the recipe's folder (`OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:81`)
  and the theme's manifest names it that way (`OrchardCore.Themes/TheBlogTheme/Manifest.cs:5`). Confirm it; the recipe is also found
  by its name, **Blog - Lucene Search**.
- **The routes of the seed posts.** The posts carry their own `AutoroutePart` path (`blog/<slug>`), like the post of the Blog
  recipe, so the path pattern of the type, which reads the container (`blog.recipe.json:216`), is not evaluated. Confirm that the
  import succeeds and that no post shows an error or a broken link in the results.
- **The width of the search box.** The longest queries (`fields-2`, `fields-3`, `fields-4`) are up to 58 characters long and the
  box of the theme is a large Bootstrap input. If the start of the typed text scrolls out of the box while the end stays
  visible, that is accepted. If the text is cut at the end, or hidden by the button, report it: do not change the
  browser zoom or the query.
- Result order is not described by the narration, and the plan does not fix it.
