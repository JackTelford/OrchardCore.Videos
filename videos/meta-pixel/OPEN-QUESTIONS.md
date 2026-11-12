# Open questions: Meta Pixel

What was checked in the Orchard Core source (commit `92db4a83`) and the documentation, and what a person must confirm
on a running site before the video is recorded. The narration states only what the source shows.

## To confirm on a running site

1. **The settings page with only Meta Pixel enabled.** `FacebookPixelSettingsDisplayDriver` checks the permission
   `ManageFacebookApp` before it shows or saves the editor
   (`OrchardCore.Facebook/Drivers/FacebookPixelSettingsDisplayDriver.cs:41, 75`). That permission is registered by
   `Startup` of `OrchardCore.Facebook` (Meta Core Components), and the Meta Pixel feature does not depend on that
   feature. In the source, `SuperUserHandler` (`OrchardCore.Settings/Services/SuperUserHandler.cs:35`) grants every
   permission to a user in the Administrator role, registered or not, so `admin` should see the editor. Confirm that
   `/Admin/Settings/facebook-pixel` shows the `Pixel Identifier` field right after `Meta Pixel` is enabled, with
   `Meta Core Components` still disabled. If it does not, enable `Meta Social Plugins Widgets` (it depends on Meta
   Core Components) before `03-pixel`, off camera, and change nothing in the narration.
   The page is reachable for the `Administrator` role only: `StartupPixel` registers no permission for the settings
   group `facebook-pixel`, so for any other user `Settings/Controllers/AdminController.cs:53` needs `ManageSettings`
   (`OrchardCore.Settings/Services/SiteSettingsAuthorizationHandler.cs:28-34`), and the editor needs `ManageFacebookApp`,
   which no role can be given while Meta Core Components is disabled. The recordings sign in as `admin`, so they are not
   affected, and the narration says nothing about permissions.
2. **The pixel code in the page.** The source adds the Meta Pixel code to the head scripts of a front-end page when a
   Pixel Identifier is saved (`OrchardCore.Facebook/Filters/FacebookPixelFilter.cs:34-44`), and the Blog theme renders
   head scripts (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`). `recording-plan.md` section 7 has the check
   (`curl` of the home page). Also confirm that the same code is absent from an admin page.
3. **Tracking not allowed.** The narration of `page-2` says the pixel is left out for a visitor whose tracking is not
   allowed. The source reads `ITrackingConsentFeature.CanTrack` and treats a missing feature as allowed
   (`FacebookPixelFilter.cs:36`). Orchard Core has no admin setting for it, and with the default options of the
   ASP.NET Core cookie policy `CanTrack` is true, so a default site always adds the pixel. Confirm on a site that
   configures a consent requirement in code that the pixel is left out until the visitor consents. The video does not
   show that configuration.
4. **The menu and the Features page.** The locators of `recording-plan.md` (the `Tools`, `Settings` and
   `Integrations` buttons, the `Features` and `Meta Pixel` links, the id of the `Enable` link, the filtering of the
   search box) were written from `OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`,
   `NavigationItemText.cshtml`, `OrchardCore.Features/Views/Admin/Features.cshtml` and
   `OrchardCore.Features/Assets/ts/features-index.ts`; none was run. Check each with `Recorder probe`.
5. **The toasts.** `The feature Meta Pixel was enabled.` and `Site settings updated successfully.` are toasts that hide
   5 seconds after the page loads (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). The
   moment of the click in `enable-2` and `pixel-3` needs tuning on the first take, so the narration of the next step
   reads the notice while it shows.
6. **The password field.** The token field is a password field (`FacebookPixelSettings.Edit.cshtml:33`) whose value
   attribute holds the decrypted token. Confirm that the recording shows dots only.

## Not verified from the source

- **The empty token.** Saving the settings page with the token field empty (as `03-pixel` does) stores an encrypted
  empty text (`FacebookPixelSettingsDisplayDriver.cs:87-91`), which is not an empty value for the check
  `string.IsNullOrWhiteSpace(settings.ConversionsApiAccessToken)` of `MetaConversionsApiService.cs:61`. A workflow run
  after such a save sends the request with an empty token. The source does not show how Meta answers, so the narration
  of `wf-4` says only that the task ends in `Failed` when Meta does not accept the event, and gives a missing Pixel
  Identifier or a missing access token as examples. A person with a Meta account can confirm the empty-token case; the
  video does not run a workflow.
- **What the browser pixel reports.** The pixel code this feature adds reports a page view only
  (`OrchardCore.Facebook/Filters/FacebookPixelFilter.cs:19`) and sends no event id. The `Event ID` of the workflow task
  (`Views/Items/MetaConversionsApiEventTask.Fields.Edit.cshtml:48`) lets Meta deduplicate an event only when something
  else on the site also reports the same event, with the same id, from the browser. The narration of `wf-3` is worded
  that way. That Meta deduplicates on this id is stated by the hint of the form and by the documentation, not by the
  source.

- **What Meta shows.** The video reads only the hints of the Orchard Core form: the Pixel Identifier is found on the
  "Meta Data Sources portal", the token is generated in the "Events Manager's Conversions API settings", the Test
  Event Code is on the "Test Events tab", and events with a test code "show up in the test console". Whether those
  names are still the ones Meta uses, and the claim of the opening card that the feature lets you "track conversions
  from your Meta ads" (the purpose of the pixel), come from the form, the manifest description and the documentation,
  not from Meta. A person with a Meta account should confirm them.
- **The workflow editor.** The slides of `wf-1` to `wf-4` say that the Meta category of the workflow editor holds the
  Meta Conversions API Event task, that its settings are Event Name, Action Source, Event Source URL and Event ID, and
  that it ends with Done or Failed. The labels come from `MetaConversionsApiEventTask.Fields.Edit.cshtml`,
  `...Fields.Thumbnail.cshtml` and `MetaConversionsApiEventTask.cs`; the designer
  (`OrchardCore.Workflows/Assets/designer`) lists activities by category with the thumbnail of each. No workflow is
  built or run in this video, and a run would end in Failed without a Meta account. A person should open the editor
  once, with Workflows enabled, and confirm the `Meta` category and the card.
- **Where a code mentions a path.** The error messages of `MetaConversionsApiService` say "Settings > Meta Pixel",
  while the menu is Settings, Integrations, Meta Pixel (`AdminMenuPixel.cs:26-28`). The video uses the menu.
