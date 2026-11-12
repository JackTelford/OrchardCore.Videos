# Open questions: azure-ai-search-queries

Written from the Orchard Core source at commit `92db4a83` and the documentation pages of the tracker entry. Nothing
was run against a running Orchard Core site. This file is for the person who records the video and for the
maintainer; the narration, the slides and `video.json` hold only what the source shows.

## 1. The planned video assumes a feature that is not in the source (a maintainer decides)

The tracker title is "Azure AI Search Queries: Search From a Saved Query", the counterpart of the Lucene and
Elasticsearch video. In the source there is no Azure AI Search query source:

- `AddQuerySource` is called in three places only: `OrchardCore.Lucene/Startup.cs:54`,
  `OrchardCore.Elasticsearch.Core/ServiceCollectionExtensions.cs:18` and `OrchardCore.Queries/Sql/Startup.cs:24`.
  The Add Query dialog lists `IQuerySource` services (`OrchardCore.Queries/Controllers/AdminController.cs:90`).
- `OrchardCore.AzureAI/Startup.cs` registers `AddSearchService<AzureAISearchService>` (class `SearchStartup`), which
  serves the site search page (`OrchardCore.Search/Controllers/SearchController.cs:115`), and no query source.
  The "Default Query Settings" of an index profile (`AzureAISearchQuerySettings.Edit.cshtml`) are the fields and the
  analyzer that this search service uses, not a saved query.
- The Blog theme has recipes for Lucene, Elasticsearch and SQL queries and searches
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.*.query.recipe.json`), and none for Azure AI Search.
- Neither documentation page of the entry describes an Azure AI Search query source.

What this video does about it: it says so (chapter 01), and teaches how to write the missing source, with a small
module built from the extension points that the source shows (`IQuerySource`, `AddQuerySource`, `QueryHandlerBase`,
`DisplayDriver<Query>`, `ISearchService`). The module is not part of Orchard Core. A maintainer should confirm that
this is the video wanted, or retitle or drop the entry. If Orchard Core gains such a source, the sentences of
`intro` and `queries-3` become false: set the video to `needs-update`.

## 2. The module was not compiled

The extract of the source has no `Directory.Packages.props` and no solution, so nothing could be built. Every type,
method and overload used in the module was read in the source (the sources are cited in `recording-plan.md`), but a
person must build it before recording and fix what the compiler reports, then check that the slides still show the
same code. Points most likely to need a change:

- `results.Items = query.ReturnContentItems ? ordered : ordered.Select(x => new { ... })` relies on the target-typed
  conditional expression (`List<ContentItem>` and `IEnumerable<anonymous>` to `IEnumerable<object>`).
- The list of `ProjectReference` lines of the module (the reference to `OrchardCore.Search.Abstractions`,
  `OrchardCore.Indexing.Abstractions` and `OrchardCore.Liquid.Abstractions` is needed for `ISearchService`,
  `IIndexProfileStore` and `ILiquidTemplateManager`).
- `[assembly: Module(... Dependencies = [...])]` with a collection expression; the module template uses
  `new[] { ... }` for the same property.
- The shape names: `Query-AzureAISearch.Link.cshtml` for the alternate `Query_Link__AzureAISearch`
  (`OrchardCore.Queries/Views/Admin/Index.cshtml:63-64`), and `AzureAISearchQuery.Edit.cshtml` for the shape
  `AzureAISearchQuery_Edit`, follow the Elasticsearch module's `Query-Elasticsearch.Link.cshtml` and
  `ElasticQuery.Edit.cshtml`.

## 3. The stand-in for the Azure AI Search service

Checked: a console app with the NuGet package `Azure.Search.Documents` 11.7.0 and the stand-in of
`recording-plan.md` (development certificate, `https://localhost:7443`): `GetIndexAsync` (404, then 200),
`CreateIndexAsync`, `MergeOrUploadDocumentsAsync`, `SearchAsync<SearchDocument>` with `Skip`, `Size`, `Select` and
`SearchFields`, `DeleteDocumentsAsync` and `DeleteIndexAsync` all succeeded, and the SDK refuses an endpoint that is
not `https` ("endpoint only supports https"). The calls that Orchard Core makes are those of
`AzureAISearchIndexManager.cs` and `AzureAISearchDocumentIndexManager.cs`.

To confirm on the running site:

- The version of `Azure.Search.Documents` that Orchard Core uses is set in `Directory.Packages.props`, which is not
  in the extract. A different version may send other calls (the site calls `DeleteIndexAsync(name, default(MatchConditions))`,
  an overload that 11.7.0 does not have). The stand-in console logs every request: each must be answered, none
  with 404 or 500, and `Search > Indexes` must show "BlogPostsAI" after the index is created.
- The stand-in matches a document when any value contains one of the words. Real Azure AI Search analyzes the text;
  the order and the number of results of the video come from the stand-in, not from a real service.
- `NO_PROXY=localhost,127.0.0.1` together with the dead `HTTP_PROXY` and `HTTPS_PROXY`: confirm that .NET on the
  recording machine does not send the stand-in calls to the dead proxy.
- The development certificate must already be trusted on the recording machine (`dotnet dev-certs https --check --trust`).
  The recording guidance forbids adding a certificate made for the video to a trust store. If the development
  certificate is not trusted, a person decides how the site may trust the stand-in; until then the video stays blocked.
- The settings come from the configuration (`OrchardCore__Search__AzureAISearch__Endpoint` and
  `__Credential__Key`), as the documentation shows for `appsettings.json`. Binding `Credential:Key` into an
  `AzureKeyCredential` was not run. The site logs a warning for a key kept in the configuration
  (`WarnIfLegacySecretConfigured`), which is harmless here. If the binding fails, use Settings > Search > Azure AI
  Search (tick "Use custom Azure Search AI settings", then the fields "Endpoint", "Authentication Type" = "API Key",
  "Api Key": `AzureAISearchDefaultSettings.Edit.cshtml:23,33,41,49`, `AzureAISearchDefaultSettingsDisplayDriver.cs:69`)
  before the recording, and do not record that page.
- The name of the index at the stand-in: the site sends the full name of the index, the tenant name in lowercase, a
  hyphen and the "Index Name" (`AzureAISearchIndexNameProvider.cs:76-81`), so the stand-in is expected to list
  `default-blogposts` at `/stats`. This was read in the source, not run: if `/stats` shows another key, the plan's
  wait condition (`{"default-blogposts":4}`) is the only text to change.

## 4. Screens and values to confirm on a running site

- The index form: the technical name generator of "Index Name" and "Name" (`technical-name-generator`, a script
  that is not in the extract) may fill "Name" in another way than the plan assumes. The check box list of "Content
  types" (`SelectContentTypes`, `Default.cshtml`) labels each content type with its display name, so "Blog Post" is
  the label to tick; that was read in the source. The plan says to check both fields before "Save".
- The JSON that the API returns: `AzureAISearchQueryResults` has the properties `Items` and `Count`, and each item
  without "Return Content Items" has `ContentItemId` and `DisplayText`. The casing of the JSON names and the way
  Chromium shows the response (a `pre` element, or its pretty-print view) were not seen. The narration names none
  of them; the contact sheet of `run-1` to `run-3` must show the display texts and nothing else of the posts.
- The anonymous call: the API scheme has no handler without OpenID Connect, so the request is anonymous
  (`OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs`), and the permission
  "Execute Api - BlogSearch" must be granted to the Anonymous role (the "Allow" check box of its row). The role page
  and the permission label were read in `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml` and
  `QueryPermissions.cs:23` only. The narration of `run-4` also says that "Manage queries" and "Execute Api - All
  queries" imply the permission: that follows `QueryPermissions.cs:19-25` and `DefaultPermissionGrantingService.cs:37-39`
  and was not tried with a token on a running site.
- The editor page is taller than the window: the base fields ("Name" and the Monaco "Schema" field) come before the
  module's three fields, so `create-3` scrolls. The scroll distance is a guess.
- The chapter 03 sentence "A query handler fills that class from the data of a recipe step" follows
  `QueryStep` calling `IQueryManager.NewAsync(source, data)` and `InitializingAsync` of the handlers
  (`OrchardCore.Queries/Recipes/QueryStep.cs`, `DefaultQueryManager.cs`); no recipe was run.
- The first page of the list in `create-5` shows no "source" badge for the new query, because the module has no
  `Display` method in its driver (the shipped sources add a badge). That is a choice of the sample module.
- Pronunciation: the label "Execute Api" (step `run-4`, as written in `QueryPermissions.cs:19,23`) is mapped to
  "Execute API" in the `spoken` map of `storyboard.json`. Confirm with `scripts/pronounce.py` how the voice says it,
  and "SQL". If "API" is not read letter by letter, a person picks the spelling (for example "Execute A.P.I.").
- The stand-in's dead-proxy rule and certificate rule come from `references/recording.md`, which asks to accept a
  self-signed certificate "only in that process". The Azure SDK clients of Orchard Core are created without options
  (`AzureAIClientFactory.cs`), so the site's process cannot be told to accept another certificate without code;
  the plan therefore relies on the trusted development certificate (see also the questions to the specification).
