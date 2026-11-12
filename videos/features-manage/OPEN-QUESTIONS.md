# Open questions: Features, enable and disable what your site uses

Every label, menu path, button, dialog, message and default in the narration was read from the Orchard Core source at
commit `92db4a83` (the Features module, the admin theme's `TheAdmin.ts` and `sidebar.ts`, the Roles module, the Blog
recipe, and the manifests of Background Tasks and Google). The content was written without a running site, so a
person must confirm the points below while recording. For each point: what was checked, and what to look at on screen.

1. **Search results of `background tasks` and `google`.** Checked: the rows match on `Name Id Description`
   (`Drivers/FeatureEntryDisplayDriver.cs:20`, `Assets/ts/features-index.ts:87-135`), and the descriptions come from the
   manifests (`OrchardCore.BackgroundTasks/Manifest.cs`, `OrchardCore.Google/Manifest.cs`). By that reading,
   `background tasks` leaves one row (Background Tasks, category Tasks) and `google` leaves three (Google Analytics,
   Google Authentication, Google Tag Manager, category Google). The narration of `find-2` says "One row is left" and the
   narration of `bulk-3` names Google Authentication. Every `Manifest.cs` under `src` was searched for both texts and only
   those features match. The other modules whose manifests mention "a background task" (Contents, Indexing, Lucene,
   Elasticsearch, Sitemaps) say it in the singular, so they do not match `background tasks`. The project files
   (`*.csproj`) hold NuGet package descriptions only; the names and descriptions on the page come from the manifests. Confirm the counts in the summary "Showing X of N features" (X is the number of rows shown, N the
   number of features of the site; X is lower than N from the start, because both Visibility options are selected and hide
   the on-demand and always enabled features, `Assets/ts/features-index.ts:87-134,218`); if a description of another
   feature contains the search text, change the text typed or the narration.

2. **Background Tasks, Google Analytics, Google Tag Manager and Google Authentication start Disabled** on a site made with
   the Blog recipe (they are not in the recipe's feature list, `blog.recipe.json:31-80`). Confirm on the Features page
   before the first take.

3. **The Tools entry appears and disappears.** Checked: `OrchardCore.BackgroundTasks/AdminMenu.cs:18-19` registers the
   entry under Tools with the permission Manage background tasks, and `OrchardCore.Roles/Services/RoleUpdater.cs` gives the
   Administrator role the permissions of a feature when it is installed or enabled. Confirm that, after Enable, the entry
   Background Tasks is in the left menu of the user `admin` on the page that follows the reload (`enable-3`), and that it
   is gone after the Disable and its reload (`disable-3`). The narration of `what-3` ("a feature that is disabled adds none
   of them") rests on the same point.

4. **The disable dialog shows the plain text.** Checked: nothing in the shipped manifests depends on
   `OrchardCore.BackgroundTasks`, so the dialog text is "Are you sure you want to disable the Background Tasks feature?
   Continue?" without a list of dependent features (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:45-53`). Confirm
   the dialog title is "Disable Feature" and the buttons are Yes and No.

5. **Timing of the success message.** Checked: a success message is a toast that hides after 5 seconds
   (`NotifierExtensions.cs:71`). The narration of `enable-2`, `disable-3` and `bulk-3` mentions the message in its first
   seconds. Confirm on the contact sheets that the reload of the site (the site reloads its configuration after a feature change) is quick
   enough for the message to be on screen while it is mentioned. If it is not, shorten the sentence, or accept a frame
   without the message: the narration also describes the page state that remains.

6. **Bulk actions ask for no confirmation.** Checked: the Bulk Actions items only set the hidden `BulkAction` field and click
   the hidden submit button (`Assets/ts/features-index.ts:156-168`), and the form sends `Force=true`
   (`Views/Admin/Features.cshtml:48-49`). The narration of `bulk-2` says neither Enable nor Disable asks for confirmation.
   Confirm that no dialog appears after the click on Enable.

7. **The tooltip on the dimmed Disable button** ("This feature is always enabled and cannot be manually disabled.") is a
   Bootstrap tooltip on a wrapper span (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:32-33`). Confirm that the cursor
   moved by `MoveToAsync` makes it appear in the capture, and that the Features row is in the group Infrastructure and
   hidden until "Hide always enabled" is cleared (`find-5`).

8. **Names in the narration of `what-1`.** "Media", "Workflows" and "Sitemaps" are the names of features in the catalog
   (`OrchardCore.Media`, `OrchardCore.Workflows`, `OrchardCore.Sitemaps`). Confirm that they appear with these names in
   the list of the site (Workflows and Sitemaps are Disabled on a Blog site).

9. **Not said in the video, on purpose.** What happens to the data of a feature when it is disabled, whether a disabled
   feature's content types stay, and what the cascade of dependents does are not narrated: the first two are not
   settled by the source read for this video, and the third belongs to the video about dependencies.

10. **Layout of the list and of its buttons.** Checked: without a stored choice the lists use the List layout and the
    Buttons actions layout (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`), and the `Admin`
    `List` section of `src/OrchardCore.Cms.Web/appsettings.json:19-25` is commented out. The narration of `page-4` ("a
    button at the end of a row") and every locator of the plan rely on it. Confirm on the first take that the rows are
    list items with the Enable or Disable button at their end, and that the List and Grid buttons (on by default,
    `AdminSettings.cs:49`) are on the page beside the filters and are not clicked.

11. **How the voice reads the names.** The `spoken` map of the storyboard is empty, because no name seemed to need a
    respelling. Before narrating, check with `scripts/pronounce.py` that the voice reads "Sitemaps" (`what-1`) and the
    lower-case "google" (`bulk-1`, `bulk-3`) as they are written.
