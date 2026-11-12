# Open questions: Site Search Settings

Everything in the narration was read from the Orchard Core source at commit `92db4a83` (paths under `src/`) or from
`docs/reference/modules/Search/README.md`. These points could not be confirmed without a running site. A person
confirms them on the site of `recording-plan.md`, before or while recording.

## 1. The Site Search form has two fields, not three

- **Checked in the source:** the documentation lists three settings (`DefaultIndexProfileName`, `PageTitle`,
  `Placeholder`). The form, `OrchardCore.Search/Views/SearchSettings.Edit.cshtml`, shows only **Default search index**
  and **Placeholder**; there is no page title field. `SearchSettingsDisplayDriver.cs:73` copies the posted page title
  (empty, since the form has no such field) into the settings on every save, so a page title set through a recipe or
  a deployment plan is cleared by the first save of this page.
- **Narration relying on it:** `overview-3` ("Two choices belong to the page itself"), `default-2` ("The page has two
  fields").
- **To confirm:** on **Settings, Search, Site Search** only those two fields appear, and no *Page title* field.
- Related: `TheBlogTheme/Views/Search-List.liquid` writes the title *Search* in the page header and never reads the
  page title setting; the module's own `Search-List.cshtml` writes it as the `h1`. The video does not talk about it.
- When no index exists at all, the form shows only the warning *No available indexes. Add at least one index to
  search.* (`SearchSettings.Edit.cshtml:28`) and hides both fields. The seed creates two indexes, so the clips never
  show this.

## 2. The menu path

- **Checked in the source:** `OrchardCore.Search/AdminMenu.cs:25-27` builds **Settings, Search, Site Search**. The
  documentation says **Search, Settings, Site Search**, which is wrong. The top-level **Search** group (with
  **Indexes**, `OrchardCore.Indexing/AdminMenu.cs`) is a different entry.
- **To confirm:** in the admin menu of the running site, **Settings** holds **General**, **Debugging** and **Search**,
  and **Search** holds **Site Search**. Check the selectors of `default-2` with `Recorder probe`.

## 3. What opens the Site Search page

- **Checked in the source:** `SettingsPermissions.ManageGroupSettings` is required by
  `OrchardCore.Settings/Controllers/AdminController.cs:53`, for the group `search`. The Search module does not
  register a permission for that group (it has no `AddSiteSettingsPermission` call in `Startup.cs`), so a role seems to
  need **Manage settings** as well as **Manage Search Settings** (required by the menu item,
  `AdminMenu.cs:31`, and by the editor, `SearchSettingsDisplayDriver.cs:41,62`).
- **Narration relying on it:** `permissions-1` says only that the Administrator role has **Manage Search Settings**
  and **Query any index** by default. It does not claim that **Manage Search Settings** alone opens the page.
- **To confirm:** with a role that has only **Manage Search Settings**, check whether **Site Search** opens. If it
  does not, the documentation should say so.

## 4. The search page with no default index

- **Checked in the source:** `SearchController.cs:93-99` adds the warning *No default search index has been
  configured.* and returns `View()` with no model; `Views/Search/Search.cshtml` renders `DisplayAsync(Model)`, and
  `ShapeExecuteAsync` returns empty content for a null shape (`DisplayHelper.cs:63-68`), so no `Search-List` shape is
  drawn: the page has the navigation bar and the warning, but no masthead and no search box. The warning reaches the
  `Messages` zone of the Blog layout (`NotifyFilter.cs:129-166`, `Layout.liquid:39`) as a `div.message.message-warning`
  (core `Message` shape, `CoreShapes.cs:122`; the Blog theme has no `Message` template).
- **Narration relying on it:** `default-1`.
- **To confirm:** `/search` in `TheBlogTheme` renders without an error, and the warning is visible in the page, styled
  as a yellow alert. Whether the dynamic call `DisplayAsync(Model)` with a null model picks the overload that accepts
  a null shape (and does not throw `ArgumentException`) is a runtime detail of the C# binder. If the page fails or is
  bare, reword `default-1` and adapt its entry in `recording-plan.md`.

## 5. The seed recipe

- **Checked in the source:** the two index definitions copy `OrchardCore.Themes/TheBlogTheme/Recipes/blog.lucene.search.recipe.json`;
  the content step copies the shape of the blog post in `blog.recipe.json`, without `Owner` and `ContainedPart`
  (the recipe parameters `AdminUserId` and `AdminUsername` do not exist in **JSON Import**).
- **To confirm:** the import runs without errors; the three posts open at `/blog/mountain-week`, `/blog/good-map` and
  `/blog/city-on-foot`; their summaries render in the results list (`Content-BlogPost.Summary.liquid` reads the
  category and tags, which these posts do not have; the `taxonomy_terms` filter returns nothing for a missing value);
  and `/search/Posts?terms=exploration` lists four posts. Indexing runs in the background: wait, or use the
  **Rebuild** bulk action of **Search, Indexes**.
- The default query version `LUCENE_30` is copied from the shipped recipe, while the index form offers `LUCENE_48`
  only (`LuceneIndexProfileDisplayDriver.cs`). If the import rejects it, create the two indexes on **Search, Indexes**
  instead (**Add index**, **Content in Lucene**, the content type, then **Save**) and rebuild them.

## 6. Visitors who are not signed in

- **Checked in the source:** `SearchController.cs:110-113` returns `ChallengeOrForbid()` when the user lacks
  **Query any index** for the index (directly, or through the per-index permission
  `IndexingAuthorizationHandler.cs`). `ControllerBaseExtensions.cs:14` challenges a user who is not signed in and
  forbids one who is.
- **Narration relying on it:** `permissions-2` ("is asked to sign in"), `permissions-3`.
- **To confirm (not recorded):** in a private browser window, `/search/Posts?terms=exploration` redirects to the
  sign-in page before `07-permissions`, and lists the posts after it.

## 7. Paging and highlights (slides only, not recorded)

- **Checked in the source:** the page size is the site's Page size (`SearchController.cs:145`, label **Page size** in
  `OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:54`); the pager is `PagerSlim` with previous and next links
  (`PagerShapesTableProvider.cs`). `Search-Results.cshtml:19-33` shows a linked title and the highlight fragments when
  the provider returns highlights, and the Summary display otherwise. Of the providers, `ElasticsearchQueryService.cs`
  fills the highlights; the Lucene search service does not.
- **Narration relying on it:** `results-2`, `results-3`.
- **To confirm:** on a scratch copy with Page size 2, `/search/Posts?terms=exploration` shows next and previous links
  and the second page lists the other posts. For highlights, the statement "some providers" is kept general on
  purpose.

## 8. The role editor rows

- **Checked in the source:** `IndexingPermissionsProvider.cs` adds one **Query '<name>' Index** permission per index,
  shown under the **Indexing** group; **Query any index** and **Manage Search Settings** are under **Search**. The row
  attribute is `data-text` and the check box id is `Checkbox.<permission name>` (`_RolePermissions.cshtml`).
- **To confirm:** after typing *Query* in the **Search permissions** box of the *Anonymous* role, the rows
  **Query any index**, **Query 'Pages' Index** and **Query 'Posts' Index** are listed, and the selectors of
  `permissions-2` and `permissions-3` find them.

## 9. Voice

Add to `spoken` only after hearing them with `scripts/pronounce.py`: `/search`, `/search/Pages` and `term(s)`
(the storyboard has first guesses: "slash search", "slash search slash Pages" and "terms"). Also listen to
*Lucene*, *Elasticsearch*, *Azure AI Search* and *Query 'Posts' Index*.

## 10. Toast timing

`default-4` and `permissions-4` mention a confirmation message that appears for a few seconds. Check on the contact
sheets that it is on screen while the narration reads it.

## 11. The placeholder on the Blog theme's search box

- **Checked in the source:** the controller passes the **Placeholder** setting to the search form
  (`SearchController.cs:139`). `TheBlogTheme/Views/Search-Form.liquid:11` writes it with the Liquid filter `d`, which
  only the Data Localization feature registers (`OrchardCore.DataLocalization/Startup.cs:26`). The Blog recipe does not
  enable that feature, and the seed does not either. The module's own `Search-Form.cshtml:11` uses a no-op data
  localizer by default (`ServiceCollectionExtensions.cs:148`), so it is not affected.
- **Narration relying on it:** `placeholder-2` ("The search box shows Search the blog"), `placeholder-1`.
- **To confirm:** with the Blog theme and Data Localization disabled, `/search` shows *Search the blog* in the empty
  box after the setting is saved, and does not fail with an error about the filter. If it fails or shows another text,
  enable **Data Localization** (it depends on **Localization**) in the seed and check again, or reword `placeholder-2`.

## 12. The Search Form widget

- **Checked in the source:** `SearchMigrations.cs:21-26` creates the content type **Search Form** with the *Widget*
  stereotype when **Contents** is enabled, so it can go in a layer zone (the Blog recipe enables **Layers**) or in the
  flow of a page (the Blog recipe's *Page* type has a *FlowPart*). **Placeholder** is required
  (`SearchPartViewModel.cs:7`, `SearchFormPart.Edit.cshtml:4,6`); **Index Name** is optional (`SearchFormPart.Edit.cshtml:12-16`).
- **Narration relying on it:** `address-2` (a slide only, not recorded).
- **To confirm:** on a scratch copy, add a **Search Form** widget to a layer and to a *Page*, leave **Index Name**
  empty, and check that its box submits to `/search` and uses the default index.
