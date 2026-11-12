# Open questions: Meta Social Plugins: Like, Share and Comments Widgets

The narration says only what was read in the source (commit `92db4a83`, paths relative to its `src` folder) and in
the two documentation pages. The points below could not be checked without a running site and Meta's servers. Each
one says what was checked, and what a person must confirm. Do not edit the narration to settle a point: report it.

## 1. What Meta's script draws on the recorded site

- **Checked in the source.** The Like, Share and Comments widgets write their markup into the page
  (`OrchardCore.Modules/OrchardCore.Facebook/Migrations/Widgets/migration.recipe.json:18`, `:52`, `:120`). The widget
  view adds the `fb` script to the page when Init on every page is off
  (`OrchardCore.Modules/OrchardCore.Facebook/Views/FacebookPluginPart.cshtml:14`). That script starts Meta's SDK with
  the identifier and the version of the settings (`Endpoints/GetSdkEndpoints.cs:61-65`) and loads the SDK file from a
  Meta server (`Endpoints/GetSdkEndpoints.cs:120`).
- **Not checked.** What Meta's servers do with a made-up numeric application identifier, with the default version of
  the SDK, and with a page on a local address. Whether the Like button, the Share button and the comments box are
  drawn at all, and what they show (a count, a sign-in prompt, an error).
- **To confirm.** Run the pre-flight check of `recording-plan.md` on a running site with internet access. The steps
  `check-1` and `check-2` say only that the footer holds the three widgets and that the page loads Meta's script,
  which is true even if Meta draws nothing, but a footer with three blank blocks makes a poor video. If the footer shows no frame from
  Meta after 15 seconds, or shows an error, report it: the maintainer decides how to record the last chapter. If
  the frames show a real person's name or account, do not use the take.

## 2. The default SDK version in the settings

- **Checked.** The settings page fills the field Javascript SDK Version with its default, and the hint of the field
  names that default (`OrchardCore.Modules/OrchardCore.Facebook/Views/FacebookSettings.Edit.cshtml:41`,
  `Settings/FacebookSettings.cs:27`). The narration says only that the other fields keep their defaults.
- **Not checked.** Whether Meta's SDK still serves plugins for that version. The narration does not claim it does.
- **To confirm.** Open the browser console during the pre-flight check and report any message about the version.

## 3. Whether visitors who are not signed in see the widgets

- **Checked.** The three widget types are securable (`Migrations/Widgets/migration.recipe.json`, key `Securable`). The
  layer filter shows a securable widget when the user may view the content item
  (`OrchardCore.Modules/OrchardCore.Layers/Services/LayerFilter.cs:132`). The handler of per-type permissions only
  adds grants (`OrchardCore.Modules/OrchardCore.Contents/Security/ContentTypeAuthorizationHandler.cs`), and the
  Anonymous role has the View content permission by default (`OrchardCore.Modules/OrchardCore.Contents/Permissions.cs:91-96`).
  So the widgets should show to everyone.
- **Not checked.** On a running site. The video is recorded signed in as admin, and the narration says the widget
  "shows on every page" because of the Always layer, and that the About page holds the same three widgets.
- **To confirm.** After `04-widgets`, open `/blog/post-1` in a private window and check that the footer holds the
  same three widgets. Report it if it does not.

## 4. Labels and screens that were read in the code only

Confirm on the running site that each reads as the narration says:

- the page title **Widgets and Layers** is the breadcrumb of the page opened by Design, Widgets
  (`OrchardCore.Modules/OrchardCore.Layers/Views/Admin/Index.cshtml:20`);
- the Add Widget list shows the display names Facebook Chat, Facebook Comments, Facebook Continue With, Facebook
  Like, Facebook Quote, Facebook Save and Facebook Share, sorted with the other widget types (`Index.cshtml:77`);
- **Render title** is unchecked by default (`OrchardCore.Modules/OrchardCore.Layers/Models/LayerMetadata.cs`, a
  `bool` with no default);
- the **Body** field is filled in when the editor opens, from the default of the widget type
  (`OrchardCore.Modules/OrchardCore.Facebook/Widgets/Drivers/FacebookPluginPartDisplayDriver.cs:67`), and for a
  widget type that is not draftable the editor offers **Publish** and no Save Draft button
  (`OrchardCore.Modules/OrchardCore.Contents/Drivers/ContentsDriver.cs:116`);
- on the Features page, the Enable link of the row is `btn-enable-OrchardCore_Facebook_Widgets`, the message after
  it reads `The feature Meta Social Plugins Widgets was enabled.` (the message names the requested feature, not its
  dependencies, `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`), and typing `Meta`
  in the search box leaves the row visible;
- the Secret Key field is a plain password field while the Secrets feature is disabled.
- the last chapter (`rights-1`, `rights-2`) is not recorded and shows no screen. It says what the code does: a content
  type that holds the Facebook plugin part makes the edit and publish permissions of content fail for a user without
  Manage Liquid templates (`OrchardCore.Modules/OrchardCore.Liquid/Security/LiquidContentAuthorizationHandler.cs:36`,
  `:73-75`, `:159-175`), so such a role is refused on these widgets, whatever else it may edit. A maintainer may confirm
  on a running site, with a role that has the content permissions and not Manage Liquid templates, that the Add Widget
  link leads to an access-denied page.

## 5. Widgets the video only names

- Facebook Chat, Facebook Continue With, Facebook Quote and Facebook Save are named on a slide and not shown.
  The default template of Facebook Chat (`Migrations/Widgets/migration.recipe.json:248`) holds a placeholder for a page identifier, and the hint of the field
  Javascript Sdk js says the chat plugin needs another SDK file (`Views/FacebookSettings.Edit.cshtml:50`). The
  default template of Facebook Save holds the fixed address of a third-party site instead of the address of the
  site (`Migrations/Widgets/migration.recipe.json:214`). None of this is in the narration. A maintainer may want a
  note in the documentation.
