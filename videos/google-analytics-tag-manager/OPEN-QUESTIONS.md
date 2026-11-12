# Open questions: Google Analytics and Tag Manager

What was checked in the source (OrchardCore commit `92db4a837d`) and what a person must confirm on a running site. The
narration states only what the source shows; the points below are kept out of it, or are checks on the recording.

## Left out of the narration because the source does not settle it

1. **Who can open the settings pages.** The menu entries need the permissions `ManageGoogleAnalytics` and
   `ManageGoogleTagManager` (`GoogleAnalyticsAdminMenu.cs:30`, `GoogleTagManagerAdminMenu.cs:31`), and the display
   drivers check them again. But the page itself, `OrchardCore.Settings/Controllers/AdminController.cs:53`, authorizes
   the permission `ManageResourceSettings` for the group id, and the only handler that maps a group to its own
   permission (`SiteSettingsAuthorizationHandler.cs`) reads the permissions registered with `AddSiteSettingsPermission`.
   Only the Email and Settings modules register one; the Google module does not
   (`GoogleAuthenticationStartup.cs`). The permission checked by the page, `ManageResourceSettings`, is implied by
   "Manage settings" (`OrchardCore.Settings.Core/SettingsPermissions.cs:21`), and the display drivers then ask for the
   Google permission again (`Analytics/Drivers/GoogleAnalyticsSettingsDisplayDriver.cs:31`). So a role that has only
   "Manage Google Analytics settings" may see the menu entry and then be refused the page, and the page seems to need
   both permissions. The video says only that the Administrator role has both permissions by default, and the slide
   says the permission is "needed" for the settings.
   **To confirm:** create a role with only that permission, sign in as a user of that role, and open
   Settings, Integrations, Google Analytics.
2. **Where the ID comes from in Google's own screens.** The documentation says "Admin->Tracking Info->Tracking Code",
   which describes Google's interface, not Orchard Core's, and its own recipe example uses an ID that starts with `G-`.
   The video repeats only the hint of the field ("The tracking ID defined in your Google Analytics account.") and
   never names a screen of Google, nor the words "Measurement ID", which are not in the source.
3. **What Google does with the placeholder IDs.** The video types `G-XXXXXXXXXX` and `GTM-XXXXXXX`. Nothing in the
   source says how Google treats a hit for such an ID, so the narration does not say that visits are reported; it says
   that the scripts load from Google (`www.googletagmanager.com` in both filters).
4. **The Tag Manager body fallback.** The code registers one head script and nothing else
   (`TagManager/GoogleTagManagerFilter.cs:44`); it adds no fallback for browsers without scripts. The narration does
   not mention it either way. A maintainer may want a sentence about it.
5. **Using both features together.** If a Tag Manager container already sends the visits to Google Analytics, enabling
   both features may count a visit twice. That is behavior of Google's products, not of the source, so the video gives
   no advice. A maintainer may want to add some.
6. **Tracking consent.** Both filters read `ITrackingConsentFeature.CanTrack` and use `true` when the feature is
   absent (`GoogleAnalyticsFilter.cs:40`, `GoogleTagManagerFilter.cs:36`). The default pipeline calls `UseCookiePolicy`
   without a consent check (`OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:471-480`), and the checked
   source has no module that shows a consent banner. The slide says only that the scripts are left out when the visitor's
   consent does not allow tracking. **To confirm:** how a host or a module turns the consent check on, if the maintainer
   wants a pointer to it.
7. **Themes.** The script is registered as a head script, so it appears where the theme renders head scripts
   (`resources type: "HeadScript"` in `TheBlogTheme/Views/Layout.liquid:19`, TheAgencyTheme and TheComingSoonTheme;
   `resources type="Header"` in TheTheme). A theme that renders neither would not output it: the SafeMode theme
   (`SafeMode/Views/Layout.cshtml`) renders neither. The narration says "the head of every public page", true for the
   bundled site themes. **To confirm on the running site:** the probe of section 5 of `recording-plan.md`.

8. **Where the tags of Tag Manager are chosen.** The narration (`why-2`) and the slide say that you choose the tags in your
   Google account and that the site only needs the container ID. The source shows the second half: the only setting is
   `ContainerID` and the filter writes only the loader of the container (`TagManager/GoogleTagManagerFilter.cs:18,44`).
   That the tags are defined in the Google Tag Manager account is a fact about Google's product; the field hint ("The
   container ID defined in your Google Tag Manager account.") and the documentation ("Copy the Container ID generated")
   agree with it. **To confirm:** a maintainer accepts the sentence, or has it reduced to the part the source shows.

## To check while recording

9. **Selectors.** Every selector in `recording-plan.md` was read from the views and scripts, not run: the admin menu
   (`#adminMenu`), `#search-box`, `#bulk-action-menu-button`, `a[data-action='Enable']`, the checkboxes, `button.save`,
   `input[id$='TrackingID']`, `input[id$='ContainerID']`, `#mainNav`. Confirm each with `Recorder probe`.
10. **Start page `/Admin`.** The Blog recipe does not enable the admin dashboard. Confirm that `/Admin` opens without an
   error and shows no real data. If it does not, start the clips on `/Admin/Features` and click from there.
11. **How long the tenant restart takes** after enabling the two features. Enabling features restarts the tenant;
    saving the Google Analytics or the Google Tag Manager settings does not (the two drivers never call
    `RequestRelease`, unlike the General settings driver, `DefaultSiteSettingsDisplayDriver.cs:140`, and the Google
    Authentication driver). The end frames of `enable-5`, `analytics-4` and `tagmanager-3` depend on the success
    toast, which hides after 5 seconds (`NotifierExtensions.cs:71`). If the toast is not on screen at the end of
    `enable-5`, `analytics-4` or `tagmanager-3`, record the clip again with the click later in its step.
12. **Requests to Google from the recording machine.** The browser of the recorder loads the head scripts from
    `www.googletagmanager.com` on the pages of `05-site`. Decide whether to block those requests on the machine (for
    example with a dead proxy for the browser, or a firewall rule) or to let them through: the IDs are placeholders, but
    the requests carry the machine's address. Nothing about this appears on screen.
13. **Pronunciation.** No `spoken` entries were added. Listen for "ID" and "IDs" (they should be said letter by letter),
    "Google Account", and "Tag Manager". If a word is misread, a person confirms a spelling with `pronounce.py` first.
14. **Product text on the Features page.** The Features page shows the feature id in code style next to each name
    (`FeatureEntry.Fields.SummaryAdmin.cshtml`) and the description "Integrate Google Analytics (gtag.js)"
    (`Manifest.cs:27`). That is product text of the real page, not read aloud, and not avoidable in the List or the
    Grid layout. A maintainer confirms it is acceptable in an admin video.
