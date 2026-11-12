# Open questions: Remote Deployment

Everything in the narration was read from the Orchard Core source (`src/OrchardCore.Modules/OrchardCore.Deployment.Remote`,
`OrchardCore.Deployment`, `OrchardCore.Contents`, `OrchardCore.AutoSetup`) and from `src/docs/reference/modules/Deployment.Remote/README.md`.
Nothing was run. The points below were read but not seen working; a person must confirm them on a running site before
recording, and change the narration or the plan if the site does otherwise.

## Behavior to confirm on the running pair of tenants

1. **The request between the tenants goes through the proxy settings.** `recording.md` asks for a dead `HTTP_PROXY`/`HTTPS_PROXY`.
   The source posts the package with an `HttpClient` from `IHttpClientFactory`
   (`OrchardCore.Deployment.Remote/Controllers/ExportRemoteInstanceController.cs:106-109`). The plan sets `NO_PROXY=localhost,127.0.0.1`
   so that the call to `http://localhost:5010/staging/...` is not sent to the dead proxy. Confirm that the deployment reaches the
   destination with the proxy variables set; if it does not, say so in the plan and record without the proxy variables.
2. **Plain `http` is accepted for the Url.** The form only requires an absolute URL (`Controllers/RemoteInstanceController.cs`,
   `ValidateViewModel`); nothing in the code requires HTTPS. The narration says "A real destination uses an HTTPS address" and
   the documentation says to always use HTTPS. Confirm that the recording works with `http://localhost`.
3. **The second tenant installs itself.** Read in `OrchardCore.AutoSetup/AutoSetupMiddleware.cs:133-141`: when the Default tenant is
   installed, the other tenants of the list are created uninstalled and installed on their first request. Confirm that the first
   request to `/staging` installs `Staging Site` with the Blog recipe, the URL prefix `staging` and the admin user `admin`.
4. **One `state.json` signs in to both tenants.** The sign-in cookie is named per tenant (`OrchardCore.Users/Startup.cs:123`); it was
   not checked that the storage state of one browser context keeps both cookies, nor that `/staging/Login` accepts the same user
   name and password. Confirm with `Recorder probe` that `/staging/Admin` opens without a sign-in page.
5. **The two pages come first in the destination's list.** The list sorts by **Recently modified** by default
   (`OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:98`). The Content step keeps the creation and modification
   dates of the items (`OrchardCore.Contents/Deployment/ContentDeploymentSource.cs`, the dates are removed only for a setup recipe), but
   it was not checked what the import does with them. If the two pages are not at the top of **Manage Content**, scroll to them or
   filter the list by the type, and adjust `check-1`.
6. **The imported pages show the type `Page`** and the Page flow editor accepts the empty items exported by the plan. Confirm the
   two rows and their type badge after `04-send`.
7. **The Select button deployment.** **Select** is a link marked `data-url-af="UnsafeUrl"` (`OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:132`)
   whose action accepts POST only (`ExportRemoteInstanceController.Execute`). The script of the admin theme turns such a link into a POST
   (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:115-182`): it builds a form, copies the page's `__RequestVerificationToken`
   and the query string (`remoteInstanceId`) into it and submits it. It asks no confirmation, because the link has no `data-unsafe-url`,
   which supports the narration of `send-3`. Not verified: that the plan page contains the antiforgery input (its `<form asp-action=...>`
   should emit one). Confirm that one click returns to the plan page with the toast **Deployment executed successfully.**
8. **Toast timing.** Success toasts hide after 5 seconds (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`;
   `OrchardCore.Themes/TheAdmin/Views/Message.cshtml`). The narration of `client-5`, `inst-5` and `send-4` names a notification in its first
   sentences, and the plan has the click at the start of those steps. Confirm on a contact sheet that the toast is on screen while it is
   named; otherwise start the click earlier or later.
9. **Row actions are buttons.** The default actions layout is `Buttons` and the default list layout is `List`
   (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`), but both can be changed in the admin settings. The narration
   says the rows have **Edit** and **Delete** buttons. Confirm on the recorded site.
10. **The sidebar.** The plan expands **Tools** and **Deployments** before the capture in `03-instance` and `04-send`, and clicks them on
    camera in `02-client` and `check-1`. The exact markup of the menu was not read; find its links by text and confirm the menu stays
    open after a page loads.
11. **The Remote Client form has no title.** `Views/RemoteClient/Create.cshtml` has no title zone, unlike the Remote Instance form, so
    the page after **Add Remote Client** probably shows no heading. The narration does not name that page; confirm what the page shows.
12. **A failed import still answers 200.** Read in `OrchardCore.Deployment.Remote/Controllers/ImportRemoteInstanceController.cs:106-131`: a
    recipe error or an unexpected error is logged on the destination (the notification it adds goes into the response to the source's HTTP
    client, so no browser shows it), and the action still returns `Ok()`, so the source shows
    **Deployment executed successfully.** The narration of `send-4` and `check-2` says it. To see it, send a plan whose step needs a feature
    that the destination lacks (not part of the video), and confirm that the source still shows the success toast.

## Pronunciation to confirm with `scripts/pronounce.py`

13. `storyboard.json` has `"Api Key": "A.P.I. Key"` and `"Url": "U.R.L."` in `spoken`, because the labels are written **Api Key** and
    **Url** in the source (`Views/RemoteClient/Create.cshtml:15`, `Views/RemoteInstance/Create.cshtml:20`). Listen to both spellings
    and change them if the voice does not read the letters at a natural pace. Also listen to "HTTPS" and "Staging Site".

## Read in the source, to confirm on the running pair of tenants

14. **No upload handler rejects the package.** The destination passes the uploaded package through `FileCreationService`, which runs
    every registered `IFileEventHandler` (`OrchardCore.Deployment.Remote/Controllers/ImportRemoteInstanceController.cs:87-96`). The only
    implementation in the source is `ClamAvFileEventHandler`, registered by the feature **ClamAV Antivirus Scanner**
    (`OrchardCore.Antivirus/Startup.cs:32`), which the Blog recipe does not enable. The source sends the package without a content type
    (`ExportRemoteInstanceController.cs:99-103`). Confirm that the destination answers 200 for the package, and not 400.
15. **The destination always has the Recipes module.** The import is done by `RecipeDeploymentTargetHandler` of `OrchardCore.Recipes`
    (`ImportDeploymentPackageAsync` runs every `IDeploymentTargetHandler`), and the feature `OrchardCore.Recipes` is always enabled
    (`OrchardCore.Recipes/Manifest.cs:19`), so the narration need not name it as a prerequisite.
16. **A destination that cannot be reached.** `ExportRemoteInstanceController.Execute` catches nothing around `PostAsync` (line 109), so a
    refused connection, a name that does not resolve or a TLS error should end on an error page, not on a notification. The narration of
    `fix-1` only claims what happens when the destination answers with an error status (an error notification with the reason phrase and
    the status code, `ExportRemoteInstanceController.cs:118`). Confirm what the page shows, if it is ever recorded.
17. **The destination tenant restarts after an import.** `RecipeDeploymentTargetHandler` releases the shell of the destination after
    the recipe ran (`OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs:46`). Confirm that `/staging/Admin` still opens signed in
    right after `04-send`, before recording `05-check`.
