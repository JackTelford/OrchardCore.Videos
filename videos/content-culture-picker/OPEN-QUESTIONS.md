# Open questions

What was checked in the Orchard Core source, and what a person must confirm on a running site before or while
recording. None of it is stated in the narration as a fact beyond what the source shows.

## The seed has not been run

- **Checked in the source:** the step names and properties of both recipes (the feature, settings, themes, ContentDefinition and
  content steps), the property names of the localization settings and of the Localization part, and that the
  ContentDefinition step only adds the parts it lists (`OrchardCore.ContentTypes/RecipeSteps/ContentDefinitionStep.cs`).
  The JSON of both recipes parses.
- **To confirm:** both imports report "Recipe imported successfully!"; after the restart, the six addresses of
  section 2 of `recording-plan.md` show the titles and texts of its table; "Settings, Localization, Cultures" lists two
  supported cultures; the home page `/` is the Welcome article; the Article content type has the Localization part
  (Content Definition, Content Types, Article). If the one-step enabling of the features and the theme fails, enable
  Content Localization and The Default Theme in Tools, Features and Design, Themes, and import recipe 2 only.
- **To confirm:** the picker finds the translations without a restart (the plan restarts the site to be safe, because the
  list of translations is loaded when the site starts and the source does not show that a recipe import refreshes it).

## The names in the language menu

- **Checked:** the menu shows the `NativeName` of each supported culture (`ContentCulturePicker.cshtml:2,16`).
- **To confirm:** with the cultures `en` and `fr` the toggle reads "English" and the other entry reads "français" (the
  narration says "English" and "français"). If the machine's culture data gives other texts (for example "English (United
  States)"), the narration still reads correctly, and the plan's "On screen" lines must be updated.

## The redirect is permanent

- **Checked:** the picker's action ends with `LocalRedirect(url, true)` (`ContentCulturePickerController.cs:79`), which is a
  permanent redirect. A browser can keep a permanent redirect in its cache.
- **To confirm:** the status code in the network log of the browser (301). The plan never asks a clip to use the same
  picker link twice, and each clip runs in a new browser context, so a cached redirect cannot change a take; a person
  who tries the picker by hand should use a private window, or a cached redirect hides a change of the Homepage redirect option.

## Behavior of the picker, read from the source only

- On a page without a version in the chosen culture, with "Homepage redirect" cleared, the visitor stays on the page
  (`ContentCulturePickerController.cs:44-58`, then the redirect to the same address at line 79). To confirm on `/opening-hours`.
- With "Homepage redirect" selected, the same choice opens the French home article, "Bienvenue" (lines 54-58, with the
  home route's content item from `ContentCulturePickerService.cs:41-45`). To confirm on `/opening-hours`.
- The toggle shows the culture of the page itself, so the French article shows "français" without a cookie
  (`ContentRequestCultureProvider.cs:15-31`). To confirm on `/bienvenue`.
- The cookie: its lifetime is 14 days by default, it is HttpOnly, and "Set Cookie" is selected by default on the Content
  Culture Picker page and cleared by default on the Content Culture page (`CulturePickerOptions.cs:5`,
  `ContentCulturePickerService.cs:112-115`, `Models/ContentCulturePickerSettings.cs:6`,
  `Models/ContentRequestCultureProviderSettings.cs:5`). To confirm in the browser's cookie list after a choice in the menu
  (the standard ASP.NET Core culture cookie), and after opening `/bienvenue` with the Content Culture option selected.
- The picker is not drawn in the admin (the admin theme renders the `DetailAdmin` navigation bar, the picker is placed
  in `Detail`) and not in the Blog theme or the Agency theme (their layouts never render the navigation bar). To confirm
  by switching the site theme.

- The address after a choice: the picker builds the target with the content item's id, and the Autoroute address scheme
  turns that into the item's path (`ContentCulturePickerController.cs:70`, `OrchardCore.Autoroute/Routing/AutorouteValuesAddressScheme.cs:23-49`).
  That is why the toggle can read the culture of the page itself (the content culture provider looks the address up in
  the Autoroute entries). To confirm: after choosing français on `/`, the address bar shows `/bienvenue`, and the
  toggle reads "français" without relying on the cookie (clear the cookie, reload `/bienvenue`).
- The cookie is read by the standard culture cookie provider of ASP.NET Core, which the source never removes from the
  list of providers (a search of `src/` finds no `RequestCultureProviders.Clear`). On a page that has a culture of its
  own, the content culture provider is asked first (`Startup.cs:69` inserts it at the front), so the cookie decides the
  culture only for pages without one. The narration says only that the site reads the cookie on later requests.
  To confirm: choose français on `/opening-hours` (Homepage redirect cleared) and see that the page keeps the toggle
  "English", then open a page without the Localization part and see the culture of the cookie.

## The admin screens

- **Checked:** every label of the plan in the source, with its file and line. The two settings pages share the heading and
  breadcrumb "Settings" (`OrchardCore.Settings/Views/Admin/Index.cshtml:4`); the narration never names a heading.
- **To confirm:** the accessible names the plan uses for the menu ("Tools", "Settings", "Localization" as buttons;
  "Features", "Content Culture", "Content Culture Picker" as links); that the search "content culture" leaves the one
  row "Content Culture Picker"; that the dependency badges read "Autoroute" and "Content Localization" (sorted by name); that the toast
  messages stay long enough to be seen (they can close by themselves); that the front end, signed in as admin, draws
  the article titles as headings (`TitlePart.cshtml`) and the Article pages without errors, although the articles have
  no values for the fields of the Article part.
- **Checked:** the "Visit Site" icon of the admin top bar is an `a` element with `role="button"`, the title "Visit Site" and
  `data-bs-toggle="tooltip"`; the admin theme builds a Bootstrap tooltip on it, and Bootstrap 5.3.8 then removes the
  `title` and sets `aria-label` (`TheAdmin.ts:190-193`, `bootstrap.bundle.js` `_fixTitle`). The plan therefore selects it by
  role and name. **To confirm:** `GetByRole(Button, "Visit Site")` finds exactly one element after the page has loaded, and
  the hover tooltip reads "Visit Site".

## Who can open the two settings pages

- **Checked:** the narration says the pages are available only to users with the permission "Manage ContentCulturePicker
  settings", and that the Administrator role has it by default (`ContentLocalizationPermissions.cs:12`, `Permissions.cs:26-27`).
  The Editor role has the permission by default too (`Permissions.cs:31-32`), and both menu items ask for it
  (`AdminMenu.cs:38,45`). But the settings controller authorizes the group with the general settings permission before
  any driver runs (`OrchardCore.Settings/Controllers/AdminController.cs:53,85`; implied only by "Manage settings", which
  only the Administrator stereotype holds, `OrchardCore.Settings/Permissions.cs`), and the module does not register a
  permission for its two groups with `AddSiteSettingsPermission` (only the Email and Settings modules do). So the source
  suggests an Editor sees the menu items but is refused when opening them, which is why the narration does not name
  the Editor role.
- **To confirm:** on a site with a user in the Editor role and no other role, whether "Content Culture Picker" and
  "Content Culture" appear in the Settings, Localization menu, and whether opening them is refused. If the pages do open for
  an Editor, the narration may name both roles.
