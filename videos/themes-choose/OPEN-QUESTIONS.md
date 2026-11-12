# Open questions: Themes, change the look of your site

What was checked in the Orchard Core source (commit `92db4a83`), and what a person must confirm on a running site before
or while recording. The narration states only what the source shows; each item says where it stops.

## 1. The Blog recipe's home page in The Default Theme (`site-4`)

- **Checked**: The Default Theme's layout renders the body of any page, a navigation bar with the main menu, and the
  Header, Messages and Footer sections (`src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:45-72`). The Blog Theme's layout
  and templates are different (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`).
- **Narration says**: "Make Current changes no content. The layout and the styling come from The Default Theme." The first
  sentence is what the code does: Make Current only saves the Site theme and enables the theme's feature
  (`src/OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:107-150`); no content item is touched, and
  The Default Theme has no data migration or feature event handler (`src/OrchardCore.Themes/TheTheme/Startup.cs` registers
  only a display driver). The
  second is the layout of the theme (`TheTheme/Views/Layout.cshtml`). The narration does not say how the blog page looks.
- **Confirm**: on the running site, `/` in The Default Theme shows the blog and its post without an error or an empty
  block, and its look differs from The Blog Theme. If it does not, the step needs another wording, or another target
  theme. The Agency Theme and The Coming Soon Theme were not chosen: their layouts render a Content section and not the page body
  (`src/OrchardCore.Themes/TheAgencyTheme/Views/Layout.liquid`, `src/OrchardCore.Themes/TheComingSoonTheme/Views/layout.liquid`), and what a
  Blog page puts there was not verified.

## 2. The messages after Make Current, and the reload time (`site-2`, `toggle-1`, `toggle-2`)

- **Checked**: Make Current on a disabled theme sets the theme, enables it, and adds two success messages, "The Default Theme was
  enabled" and "The Default Theme was set as the default Site theme"; on an enabled theme it adds only the second
  (`src/OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:127-148`). Disable adds "The Default Theme was
  disabled." (`:203`).
- **Not checked**: how long the reload takes after enabling or disabling a theme (the tenant restarts,
  `AdminController.cs:138`). The plan cuts that wait out with `OffCameraAsync` around `ClickAndWaitForNavigationAsync`
  (`site-2`, `toggle-2`), as the recorder's `Scene.cs` allows; a wait placed after the click would not cut it, because the
  click already waits for the reload.
- **Confirm**: both messages show, in this order, as toasts at the top right; the cut lands where the plan says (the end of
  "when you click Make Current" in `site-2`, before "A message confirms it" in `toggle-2`); and the toasts, which hide 5 s
  after the page appears, are still on screen while the last sentence of `site-2` ("Two messages confirm...") is spoken. If
  not, move the click later in the step. Also confirm that a clip made with the click inside `OffCameraAsync` looks right:
  the click ripple is not recorded, and the video cuts from the cursor on the button to the reloaded page. If a take of
  `toggle-1` pauses on the reload, use the same pattern there.

## 3. The six cards and their thumbnails (`page-2` to `page-5`)

- **Checked**: the page lists every theme that is not always enabled, not enabled by dependency only, and not tagged hidden
  (`AdminController.cs:58-75`). None of the six themes of the Orchard Core CMS application is excluded, so The Blog Theme, The
  Admin Theme, The Agency Theme, The Coming Soon Theme, The Default Theme and Safe Mode Theme show. Each card's thumbnail is
  the file `Theme.png` of the theme's own `wwwroot` folder (`Views/Items/ThemeEntry-Thumbnail.SummaryAdmin.cshtml:6`); the one of
  Safe Mode Theme is an empty light-gray image.
  The static file provider serves `/{ThemeId}/...` for every module of the application, not only for the features enabled on
  the tenant (`src/OrchardCore/OrchardCore.Abstractions/Modules/ModuleEmbeddedStaticFileProvider.cs`, `GetFileInfo`), so
  the thumbnail of a disabled theme is served too.
- **Not checked**: how each thumbnail looks. The narration says every card shows a thumbnail, which is a file that exists
  for all six themes.
- **Confirm**: six cards, each with a thumbnail, on the starting snapshot. If a card shows no thumbnail, change
  the `page-3` sentence "Each card shows a thumbnail" to name only what is shown. Also confirm that The Blog Theme card, centered
  in the window, shows its thumbnail and its attributes list together (`page-3`); if the card is taller than the window, the
  plan keeps the list in view and lets the top of the card be cut.

## 4. Order of the cards (`page-2`, `page-5`, `site-3`)

- **Checked**: the current themes come first (`AdminController.cs:92`).
- **Not checked**: the order of the other cards, and of the two current ones. The narration claims no order, and the plan finds
  each card by its title.
- **Confirm**: where the cards sit on the starting snapshot, to decide how far `page-5` scrolls so that the cards it names
  and Safe Mode Theme's description are in view. If The Agency Theme, The Coming Soon Theme or The Default Theme cannot be seen
  at some moment of that step, say so in the take notes and re-time the scroll.

## 5. Safe Mode Theme (`page-5`)

- **Checked**: its manifest description is "A fail-safe theme when no other themes are available."
  (`src/OrchardCore.Themes/SafeMode/Manifest.cs:9`), its layout is a plain page headed "SAFE MODE"
  (`src/OrchardCore.Themes/SafeMode/Views/Layout.cshtml`), and it is enabled by the setup recipes of the Blog, Agency, Coming Soon
  and Software as a Service sites.
- **Not found**: any code that falls back to it. The theme manager returns no theme when none is valid
  (`src/OrchardCore/OrchardCore.DisplayManagement/Theming/ThemeManager.cs:40-74`).
- **Narration says**: only that the theme "describes itself as a fail-safe theme when no other themes are available", which is the
  text on its card. Nothing in the video claims an automatic fallback. Confirm with the Orchard Core team what the theme is
  for before a video says more.

## 6. The sign-in page and the Admin theme (`what-3`)

- **Checked**: on the account pages of the Users module, the Admin theme is used unless "Use site theme for login page" is
  checked, which it is not by default (`src/OrchardCore.Modules/OrchardCore.Users/Services/UsersThemeSelector.cs:43-47,86`;
  `src/OrchardCore/OrchardCore.Users.Core/Models/LoginSettings.cs:7`). The setting is under Settings, Security, User Login
  (`src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:40-45`).
  The sign-in page is `AccountController.Login` in the area `OrchardCore.Users`
  (`src/OrchardCore.Modules/OrchardCore.Users/Controllers/AccountController.cs:75`, `UserConstants.cs:11`), which the
  selector handles. With the box checked, the selector gives the Admin theme a lower priority than the Site theme
  (`UsersThemeSelector.cs:84-88`, `SiteThemeSelector.cs:29`), so the Site theme applies only if the site has one (the
  setting's hint says "Requires an active site theme.", `LoginSettings.Edit.cshtml:63`).
- **Confirm**: sign out and open `/Login`: the page uses the layout of The Admin Theme (`src/OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`),
  and checking the setting changes it to the Site theme. No clip shows this; the slide only states it.

## 7. Blank site and Headless site (`what-4`)

- **Checked**: both recipes set the Admin theme and an empty Site theme, and the Themes recipe step skips an empty value
  (`src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json:65-67`, `headless.recipe.json:107-109`,
  `src/OrchardCore.Modules/OrchardCore.Themes/Recipes/ThemesStep.cs:30-33`).
- **Confirm** (optional, on a second site): on a site made with either recipe, the Themes page marks no card with "This is the
  current Site theme".

## 8. Google Fonts (`site-1`, `site-4`)

- **Checked**: The Blog Theme asks Google Fonts for Lora and Open Sans (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`).
  The site's proxy variables block the server's own calls, not the recorder's browser.
- **Confirm**: the front end of The Blog Theme shows its fonts during the recording. If the machine is offline, the browser
  uses fallback fonts and the clip does not show the theme as it is.

## 9. The Reset dialog (`reset-2`)

- **Checked**: the Reset links open a confirmation dialog titled "Reset Site Theme" or "Reset Admin Theme" with the message
  "Are you sure you want to reset the Site theme?" (or "Admin"), and the buttons Ok and Cancel
  (`Views/Admin/Index.cshtml:33-34`, `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`,
  `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-66`).
- **Confirm**: the dialog shows these texts, and Cancel closes it without a reset (the card marked "This is the current Site theme"
  stays the same). Never click Ok.

## 10. The admin home page and the Visit Site icon (`page-1`, `site-1`, `site-4`)

- **Checked**: the admin home page shows the Customize theme card with a Manage themes button
  (`src/OrchardCore.Modules/OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4-6`, added to the dashboard by
  `AdminDashboardShapeTableProvider.cs:10-14`), and the top bar of the admin has an icon-only link to `/` whose tooltip is
  "Visit Site" (`src/OrchardCore.Modules/OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`).
- **Confirm**: on the starting snapshot the card is in view without scrolling, the tooltip shows on hover, and the link opens
  the front end in the same tab.

## 11. The theme toggler (every admin clip, `site-4`)

- **Checked**: "Enable theme toggler" is on by default (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:7-8`)
  and the Blog recipe does not change it. With it on, the top bar of the admin shows a "Toggle theme" button
  (`src/OrchardCore.Themes/TheAdmin/Drivers/ToggleThemeNavbarDisplayDriver.cs:20`). The navigation bar of The Default
  Theme shows one whenever it is the Site theme, whatever the setting says, since its driver does not read the setting
  (`src/OrchardCore.Themes/TheTheme/Drivers/ToggleThemeNavbarDisplayDriver.cs:21`). The setting does decide the color mode of
  both layouts: off gives "lite", on gives the cookie value or, with no cookie, "auto"
  (`ThemeTogglerService.cs:29-39`, `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:16`,
  `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:19`). The recorder's browser reports the light color
  scheme (this repository's `.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs`, `ColorScheme.Light`).
- **Not checked**: what "auto" looks like in the browser of the recorder.
- **Confirm**: the admin and The Default Theme are drawn in their light look in every clip, and the "Toggle theme" button
  is in the top bar and not in the way of the Visit Site icon. The narration does not mention the button.
