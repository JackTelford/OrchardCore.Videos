# Open questions: User Time Zone and Culture

The narration states only what the source and the documentation show. These points were read in the source, not seen
on a running site. A person who records the video should confirm each one on the recording site and report any
difference before the narration is recorded.

## What the narration relies on

1. **The date and the month change on the blog post.**
   - Checked in the source: the post header writes the creation time with the `DateTime` shape
     (`src/OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:17-20`). The shape converts the time with the
     local clock (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:121-123`), whose time zone is
     the one of the signed-in user when it is set (priority 100,
     `src/OrchardCore.Modules/OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:28`) and the site's otherwise
     (priority 0, `src/OrchardCore.Modules/OrchardCore.Settings/Services/DefaultTimeZoneSelector.cs:23`). The text is
     formatted with `CultureInfo.CurrentUICulture`.
   - To confirm: with the seed of `recording-plan.md`, the header reads `March 14, 2026` before the choice, and
     `mars 15, 2026` after it (`Europe/Paris`, `fr-FR`). Also confirm that no cache keeps the old text after the user is
     saved. Checked in the source: the Blog theme caches only the main menu
     (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`) and the header of the post uses no cache tag
     (`Content-BlogPost.liquid:14-22`); the time zone of a user is cached for an hour but the cache entry is removed
     when the user is saved (`src/OrchardCore.Modules/OrchardCore.Users/TimeZone/Handlers/UserEventHandler.cs:20-21`,
     called from `src/OrchardCore/OrchardCore.Users.Core/Services/UserStore.cs:235`). The narration of `prepare-1` and `result-1` states
     these two dates and the French month.
   - No French translation files ship with the source (a search for `*.po` under `src` finds none), so only the
     formatted month changes. If translation files are present on the recording site, the words "Posted by" and "on"
     change too, and so does every label of the admin after `edit-4`: then select the controls of `04-result` and later
     clips by selector, not by label, and reread the narration of `result-1`.

2. **The seed recipes import as written.** The content step must keep `CreatedUtc`
   (`src/OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:37` only sets it when it is
   missing), keep the explicit path `blog/market-walk` (the Blog Post type allows a custom path,
   `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:215`), and show the page at `/blog/market-walk`
   although the post has no container. The feature step of seed A and the `ReloadTenant` step of seed B
   (`src/OrchardCore.Modules/OrchardCore.Recipes/RecipeSteps/ReloadTenantStep.cs`) have not been run. If the Cultures
   page does not list `en-US` and `fr-FR` after seed B, restart the site, or add the culture with the Add culture
   button and select Save (which restarts the tenant by itself,
   `src/OrchardCore.Modules/OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:127`).

3. **The culture comes from the sign-in.** The culture is a claim added when the sign-in is created
   (`src/OrchardCore.Modules/OrchardCore.Users/Localization/Providers/UserLocalizationClaimsProvider.cs:23-25`). Saving
   one's own account refreshes the sign-in
   (`src/OrchardCore.Modules/OrchardCore.Users/Controllers/AdminController.cs:403`), which is why `edit-4` says the
   change applies right away. A browser context that loads the sign-in saved before `03-edit` has no culture claim, so
   `04-result` needs a new `Recorder setup` first. Confirm that the French month appears with the new sign-in, and does
   not with the old one. The narration does not say when the culture of another user takes effect, because that was not
   established (the sign-in of that user is refreshed later, at an interval that was not read).

4. **"Use site's culture leaves the choice to the site, as for a visitor who is not signed in"** (`order-2`), and
   "the site decides how dates and numbers are written" (`why-1`).
   - Checked in the source: when the culture of the user is "none" no culture claim is added
     (`src/OrchardCore.Modules/OrchardCore.Users/Localization/Providers/UserLocalizationClaimsProvider.cs:23`), and the
     provider returns no result for a request without a claim, exactly as it does for a visitor who is not signed in
     (`.../Localization/Providers/UserLocalizationRequestCultureProvider.cs:13-23`). The request then falls through to
     the other request culture providers of ASP.NET Core, as the documentation of the Users module says.
   - Not established, so not narrated: which culture those providers pick. They include the language sent by the
     browser, so the site's default culture is not necessarily the one used. The narration therefore says "the site
     decides" and never "the site's default culture" or "the localization settings".
   - To confirm: a browser set to French, a user left on Use site's culture, and French in the supported cultures.
     Record with the browser language left at `en-US`, as the plan says, and this does not arise.

5. **Visitors who are not signed in are never affected** (`order-3`). Both the time zone selector and the culture
   provider return nothing when the request is not authenticated
   (`src/OrchardCore.Modules/OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:24`,
   `.../Localization/Providers/UserLocalizationRequestCultureProvider.cs:13`). Confirm in a private window that the blog
   post shows March 14 and the English month.

6. **A user reaches the two fields from Profile with the permissions Edit own user information and Access admin
   panel** (`order-3` and the card of the users themselves in the slide).
   - Checked in the source: the Profile link is shown with Edit own user information
     (`src/OrchardCore.Modules/OrchardCore.Users/Views/UserMenuItems-Profile.cshtml:5`); the page it opens is
     `Edit` without an id in the Users controller (`.../Controllers/AdminController.cs:325-345`), which belongs to the
     admin area and so requires Access admin panel (`src/OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs:56-58`;
     the controller carries the admin attribute at `AdminController.cs:28`). The drivers of both fields check no
     permission of their own, so the same two fields are on that page. The default roles Editor, Moderator,
     Contributor and Author hold Edit own user information (`src/OrchardCore.Modules/OrchardCore.Users/Permissions.cs:33-58`).
   - To confirm: with a fake user (for example `jamie.chen@example.com`) in a role that holds both permissions,
     outside the capture, Profile shows the two fields; and that the user without Access admin panel gets a
     refusal. Do not sign in as that user on camera.

## What the recorder must confirm on the page

7. **The ids of the Enable buttons.** `#btn-enable-OrchardCore_Users_TimeZone` and
   `#btn-enable-OrchardCore_Users_Localization` assume that the id is built from the feature id with each dot replaced
   by an underscore (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
   The plan gives a fallback.
8. **The Features page layout.** The Dependencies badges are a column in the grid layout and tags under the name in the
   list layout. The narration of `prepare-3` ("listed beside it") holds for both, but check which one the recorder
   shows and ring the badges accordingly.
9. **The text of the options.** The time zone option reads `(GMT+01) Europe/Paris (+02:00)` or similar
   (`src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25`); the culture option reads the display name of the
   culture followed by the code, in the language of the page
   (`src/OrchardCore.Modules/OrchardCore.Users/Localization/Drivers/UserLocalizationDisplayDriver.cs:36-41`). The
   narration names only "Europe, Paris" and "French". Confirm that the select, once changed, shows an option that reads
   that way.
10. **The order of the two fields on the user page.** Both are placed at the same position (`Content:2`), after the
    roles. The narration names them as "Default Time Zone and Default User Culture"; if the page shows the culture
    first, swap the names in the narration of `edit-2` and record again.
11. **The messages after Enable and Save.** Success messages hide themselves five seconds after the page is shown
    (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, the delay is used by
    `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:12-20`). The plan clicks about three to four seconds before
    the end of the narration so the message is on screen during the last sentence. Confirm on the recording that the
    message is visible when the step ends; if not, click a little earlier or later.
12. **How long the site takes to answer after a feature is enabled.** The plan puts the wait off camera. If the first
    page after "Enable" takes more than a few seconds, the contact sheet shows a still frame: record again with the
    click earlier in the narration.
