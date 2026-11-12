# Open questions: Admin Dashboard

What the author checked in the Orchard Core source (commit `92db4a83`) and could not check on a running site. Each
item says what a person must confirm while recording. Nothing below is stated in the narration as a fact unless the
source shows it; the items are about the behavior of the browser, the voice, or a site that nobody ran.

## Pronunciation

- The narration says "Html Dashboard Widget" (steps `add-2`, and the subtitle of chapter 03) and "DashboardWidget"
  (step `types-1`, spoken as "Dashboard Widget" through the storyboard's `spoken` map). Check both with
  `scripts/pronounce.py`. If the voice misreads "Html", a person decides a spelling to add to `spoken` (for example
  "H.T.M.L." for "Html"); no unconfirmed spelling is in the storyboard.

## Permissions and roles (slides `who-1` to `who-3`, never shown on screen)

- `who-2` says the Administrator role has both permissions and that Editor, Moderator, Author and Contributor can
  access the dashboard "by default". Source: the default stereotypes of
  `OrchardCore.AdminDashboard/Permissions.cs:25-54`, applied to roles of those names when the feature is installed
  (`OrchardCore.Roles/Services/RoleUpdater.cs:55-94`). Confirm on the recorded site, after `02-enable`: Access
  Control, Roles, Edit on Editor, then type "dashboard" in "Search permissions"; "Access the Admin Dashboard" should
  be checked in the Allow column. The Blog recipe creates these four roles with no permissions
  (`blog.recipe.json`, step `Roles`).
- `who-2` also says the Administrator role has both permissions. The role updater skips the Administrator role
  (`OrchardCore.Roles/Services/RoleUpdater.cs:206-210`); the role passes every permission check as the system
  administrator role (`OrchardCore.Settings/Services/SuperUserHandler.cs:33-40`), and the Roles screen shows no
  permission grid for it (`OrchardCore.Roles/Controllers/AdminController.cs:535-540`). That is why the narration says
  the permissions of "these four roles" are changed on the Roles page. Confirm that the Edit page of the
  Administrator role lists no permissions, and that the admin user still sees Manage Dashboard.
- `who-3` says a user who cannot manage the dashboard also needs permission to view the widget's content, and gets
  the welcome home page when no widget is left to see. Source: `Controllers/DashboardController.cs:47-82` (the view
  check at `:67` is skipped for a user who can manage the dashboard) and `Views/Dashboard/Index.cshtml:33-56`. Confirm by signing in as an invented Editor user (an @example.com address) to a
  copy of the site after the seed: the three published widgets show, Holiday schedule does not, and the Manage
  Dashboard button is absent. This sign-in is not recorded.

## Browser behavior (recordings)

- `arrange-1`: the drag uses SortableJS (`Assets/ts/dashboard.ts:121`). Confirm that `DragToAsync` on the handle
  reorders the widgets in the capture, and that the new order is still there after a reload (it is saved by a POST to
  the Update action, `Controllers/DashboardController.cs:143`).
- `arrange-3`: the resize uses pointer events on 7 and 12 px handles (`Assets/scss/dashboard.scss:106-129`). Confirm
  that `DragAsync` on the corner handle resizes, and tune the two offsets (60 px right, 120 px down) until Style
  reminders is three columns wide and two rows high. The snapping uses a cell width that is only recalculated by a
  `resize` event (`Assets/ts/dashboard.ts:56-70`, `:237`): the plan dispatches one before the capture. Confirm that the
  columns are wider than about 80 px at 1536 px, which the offset needs.
- `add-3`: the body editor is a rich-text editor (`HtmlBodyPart-Wysiwyg.Edit.cshtml`). Confirm the editable area's
  selector, and that `TypeAsync` types into it. Confirm that the label "Title" selects one field only.
- `add-4`: Position, Width and Height hold numbers with decimals in the model (`Models/DashboardPart.cs`). Confirm
  whether their defaults show as 0, 1 and 1, or as 0.0, 1.0 and 1.0, and type the values that give the narrated result
  (position first, two columns, one row).
- `arrange-5` and `arrange-6`: the dashboard script moves an open Actions menu to the end of the page
  (`Assets/ts/dashboard.ts:208-235`). Confirm the menu is fully visible in the capture, lists the items in the order
  Edit, Delete, Unpublish (published widget) and Edit, Delete, Publish Draft, Discard Draft (draft), and closes with
  Escape.
- `enable-2`: the Features page filters rows by their text. Confirm the search "Admin Dashboard" leaves one row
  (the description of another feature might contain the same words) and that the row sits under the heading Content
  Management.
- `enable-3`: the Enable control is a button in the default layout of the list, and in a menu with another layout
  (`OrchardCore.Admin/Views/AdminListActions.cshtml`). Confirm where it is, and that the message reads "The feature
  Admin Dashboard was enabled." with no other feature in it.
- `add-5`: the success message "Your Html Dashboard Widget has been published." comes from
  `OrchardCore.Contents/Controllers/AdminController.cs:473`. Confirm that it is still on screen at the end of the
  step's narration; the narration does not quote it.

## Layout (slide `how-2`, step `add-4`)

- The narration says the grid has six columns "on a wide screen". Source: `Assets/scss/dashboard.scss:39-44` (from
  1400 px wide). Confirm six columns at the recorder's 1536 px width.
- The narration does not state a range for Width and Height. The documentation says 1 to 6; the code accepts any number
  (see the documentation issues reported with this video).

## Widget types (slides `types-1` and `types-2`, never shown on screen)

- The Stereotype field is on the Edit page of a content type, not in the creation form, which asks only for a Display
  Name and a Technical Name (`OrchardCore.ContentTypes/Views/Admin/Create.cshtml:13-27`,
  `Views/ContentTypeSettings.Edit.cshtml:77`). The slides say "create a content type ... and enter DashboardWidget as
  its Stereotype" without saying on which screen. Confirm the field is visible on the Edit page after Create, and
  that saving it is enough for the type to be listed.
- The slides say a content type whose Stereotype is DashboardWidget appears under Add Widget and receives a part for
  the position and size. Source: `Controllers/DashboardController.cs:99-112` and `:201-204`;
  `Services/DashboardPartContentTypeDefinitionHandler.cs`. Confirm by creating a content type (Design, Content
  Definition, Content Types, Create new type), entering DashboardWidget in its Stereotype field, adding the Title and
  Html Body parts, and checking that it is listed under Add Widget and that its title and body show on the dashboard
  (`OrchardCore.Title/placement.json`, `OrchardCore.Html/placement.json`).
