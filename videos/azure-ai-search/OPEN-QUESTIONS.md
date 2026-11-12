# Open questions: azure-ai-search

What could not be verified from the source and the documentation alone, with what was checked and what a person must
confirm on a running site. The narration says only what the source shows; each item below is either something the
narration avoids, or a claim the narration makes from the code that a take should confirm.

## 1. The stand-in for the search service

- **Verified (no longer open): the SDK refuses `http`.** In `Azure.Search.Documents` 11.7.0 and 12.0.0 (the packages in the
  NuGet cache; the extracted OrchardCore source has no `Directory.Packages.props`, and both versions behave the same) the
  constructors of `SearchIndexClient` and `SearchClient` that take a key credential, and the short ones that chain to them,
  call `AssertHttpsScheme`. Its IL compares `endpoint.Scheme` with `Uri.UriSchemeHttps` and throws an `ArgumentException`.
  `AzureAIClientFactory.cs:80-82` builds the client with the typed endpoint, and `AzureAISearchIndexManager.cs` (`GetAsync`,
  `CreateAsync`) catches the exception, so an `http` address ends on "Unable to create the index for the provider
  'AzureAISearch'." (`AdminController.cs:250` of the Indexing module). The stand-in therefore serves HTTPS and the Endpoint is
  `https://localhost:9201`; the first draft of the plan used `http://search.example.com:9201` and could not have worked.
- **Checked:** `AzureAISearchIndexManager.cs` shows the calls the site makes (get the index, create it, delete it) and
  `AzureAISearchDocumentIndexManager.cs` the document calls (upload, delete, search).
- **Not verified:** the REST paths, the `api-version` value and the response bodies the SDK expects were written from
  knowledge of the service, not read from the SDK. The TLS wrapper of the stand-in and the commands that export the
  development certificate (the file names `dotnet dev-certs https --format Pem` writes, and whether the certificate is trusted
  for the user that runs the site) were not run. Whether the `localhost` route or the `search.example.com` fallback is the one
  that works on the recording machine.
- **To confirm:** the dry run of section 4 of the plan. The log of the stand-in must show `GET /indexes('default-blog-posts')`,
  `POST /indexes` and `POST /indexes('default-blog-posts')/docs/search.index`; change the patterns of the stand-in to the
  paths it prints if they differ. If the site cannot connect over TLS (log of the site), the video stays blocked: record the
  tracker note instead of recording.

## 2. The spoken forms

New entries of `"spoken"` in the storyboard: "Api Key" (said as "API Key"), "standard.lucene" ("standard dot lucene"),
"AzureAISearch" ("Azure AI Search"), "blog-posts" and "default-blog-posts" (with "hyphen" between the words). Check each with
`scripts/pronounce.py` and let a person confirm them. Also listen to "Azure AI Search", "Obsolete" and "Content in Azure AI Search".

## 3. The Features page

- **Checked:** the manifest declares two features named "Azure AI Search" and "Azure AI Search (Obsolete)" in the category
  Search (`OrchardCore.AzureAI/Manifest.cs:12,23`), and `OrchardCore.Features` has no filter that hides obsolete features.
- **To confirm:** that both rows are listed after the search. If the obsolete row is not listed, drop the second and third
  sentences of `enable-1` and the highlight in the plan (the step must keep at least 8 words).

## 4. The admin menu

The markup of the menu was read in `OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml` and
`NavigationItemLink-admin.cshtml`, not run. From that markup: a leaf item keeps its id on the `li` (`li#azureaisearch`,
`li#indexes`); a group moves its id to the `figcaption` and shows a `button.item-label` that toggles it (`#settings` is set in
`OrchardCore.Settings/AdminMenu.cs`, `#search` in `OrchardCore.Indexing/AdminMenu.cs`); the group Search inside Settings, built
by `OrchardCore.AzureAI/AdminMenu.cs:40`, has no id, so the plan reaches it through `ul[data-title="Settings"]`. Not run: that the
menu restores what was left open, and that these locators resolve to one element each. If the clicks in `enable-2` are not
reliable, keep only the highlight of `li#azureaisearch a` with the groups expanded before the capture; the narration
stays true.

## 5. The create and edit forms

- **Not verified:** the order of "Content types" and "Analyzer" on the page (both drivers use `Content:5`); the exact list of
  fields under "Query field names" on the edit page. The code lists the searchable fields of the mapping, ordered by name
  (`AzureAISearchIndexProfileDisplayDriver.cs:55-66`), checks the ones in the stored default search fields, and makes the
  full-text field the default when the index is saved (`AzureAISearchContentIndexProfileHandler.cs:36-54`). The narration of
  `edit-2` says only that the list holds the searchable fields and that a search looks in the checked ones.
- **Read in the code (corrects the first draft):** "Default Query Settings" is on the create page as well as the edit page.
  `NewAsync` runs `AzureAISearchContentIndexProfileHandler.InitializingAsync`, which maps ContentItemId and
  ContentItemVersionId before the content types are known (`AzureAISearchContentFieldMapper.cs:37-49`), and
  `DefaultAzureAISearchFieldIndexEvents.cs:12-16` marks every Text entry searchable, so the view does not return at
  `AzureAISearchQuerySettings.Edit.cshtml:3-6`. On the create page the list holds those two fields, unchecked. Also read in
  the code: the content types are checkboxes labelled with their display names (`SelectContentTypes/Default.cshtml`), and
  Culture offers "Any culture" as its first option (`ContentIndexMetadata.Edit.cshtml:8-10`).
- **To confirm:** the names of the inputs used by the selectors (section 7 of the plan); that the Name field is filled in as
  Index Name is typed; that Culture shows "Any culture" before anything is chosen; that the page shows what is described
  above.

## 6. Claims the narration derives from the code

- `connect-3`: "Saving does not contact the service." (`AzureAISearchDefaultSettingsDisplayDriver.cs:83-165` only validates
  the address and protects the key.) The dry run must show an empty stand-in log after the save.
- `edit-3`: "Changes that affect the fields of the index reach the service when you rebuild it." Saving an edit
  (`Controllers/AdminController.cs:292-338`) updates the profile only; the Azure index is created from the stored mapping
  in `AzureAISearchIndexManager.cs` (`CreateAsync`, `RebuildAsync`). Confirm that no request reaches the stand-in on Save,
  and that a rebuild sends a `POST /indexes` whose definition holds the fields of the current mapping.
- `holds-2`: "Then rebuild the index." After "Include this element in the index" is checked on a part or a field, the
  mapping is recomputed when the profile is updated (`AzureAISearchContentIndexProfileHandler.cs:57`), which the
  rebuild action does first (`Controllers/AdminController.cs:450-487`). Confirm with the log of the stand-in on a running site.
- `holds-1`: the full-text field is built from the display text and the body by default
  (`FullTextAspectSettings.cs:20,26`, both `DefaultValue(true)`). Confirm that the Blog Post of the recipe is sent with a
  non-empty full-text field (the stand-in prints the number of documents; print a document to see its fields).
- `create-3`: the Name field is filled in from Index Name by `technical-name-generator.js`, only while Name has not been edited.
- `edit-2`: "a search looks in the checked ones" and "leaving every box clear searches all searchable fields" come from
  `AzureAISearchService.cs:62-83`: the checked fields become the search fields, and with none checked the searchable fields of
  the mapping are used. This applies when the Search module is enabled; the video does not enable it.
- `how-1` (slide): "Content items are sent to the indexes when they change." `IndexingContentHandler.cs` of
  `OrchardCore.Indexing.Core` indexes at the end of the request that creates, updates, publishes or removes an item, without the
  Indexing Worker feature (the Worker only adds the periodic background task). Confirm with a published Blog Post on a running site.
- `actions-2`: "Rebuild deletes the index in the service, creates it again, and indexes the content again." The Rebuild action
  resets the profile, updates it, calls `AzureAISearchIndexManager.RebuildAsync` (delete, then create) and then synchronizes
  (`Controllers/AdminController.cs:450-487`).

## 7. Seen in the code, not in the video

- **Index name rules.** A valid Index Name is lowercase letters, numbers and hyphens, at least two characters
  (`AzureAISearchIndexNamingHelper.cs:72-98`, compared in `AzureAISearchIndexProfileHandler.cs:35`). The message shown for a
  rejected name says "Only letters, numbers, and underscores are allowed" and "Must start with a letter" (line 37): an
  underscore or a capital letter is rejected, a leading digit is accepted. The narration of `create-3` follows the code.
  Confirm by typing `blog_posts` and `Blog-Posts` in the dry run.
- **Query analyzer.** The value of "Query analyzer" is stored and exported by the deployment step
  (`AzureAISearchIndexDeploymentSource.cs:67`), but the built-in search (`AzureAISearchService.cs`) reads only the default
  search fields. The narration names the setting and does not say what it does.
- **Permission.** `ManageAzureAISearchSettings` ("Manage Azure AI Search Settings") is checked by the settings page and by
  the "Include this element in the index" checkboxes, but no permission provider registers it, so it cannot be granted to a
  role in the roles pages. The user admin passes every check: members of the Administrator role succeed all permission
  requirements (`OrchardCore.Settings/Services/SuperUserHandler.cs:35-37`), and Setup makes the first user the site owner
  (`SetupEventHandler.cs:24`, `SuperUserHandler.cs:50-52`). Without that, the settings page would answer "not found"
  (`OrchardCore.Settings/Controllers/AdminController.cs`, `HasItemsInAnyZone`).
- **Row actions.** The plan assumes the default layout of admin lists (List) and of row actions (Buttons): Edit and Delete
  buttons and an Actions menu. With the layout Menu the actions sit in one menu, and `actions-1` would not match.
