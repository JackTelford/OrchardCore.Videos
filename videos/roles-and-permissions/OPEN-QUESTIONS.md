# Open questions: Roles and Permissions

What was checked in the source, and what a person must confirm on a running site before or while recording. Paths are
relative to `src/` of the Orchard Core checkout.

## 1. A fresh role already has View all content as effective (`perms-2`)

The narration of `perms-2` says "No permission is allowed for this role, but View all content is effective", and the recording
plan hovers the Effective box of that row to show "This permission is granted by role: Anonymous, Authenticated".

- Checked in the source: the Contents module gives `ViewContent` to the Anonymous and Authenticated roles as default
  permissions (`OrchardCore.Modules/OrchardCore.Contents/Permissions.cs:86-95`). The role updater adds the default
  permissions of every enabled feature to a role when the role is created
  (`OrchardCore.Modules/OrchardCore.Roles/Services/RoleUpdater.cs:133-168`). The editor shows a permission as effective
  when the role passes an authorization check, and names Anonymous and Authenticated as the source when no assigned
  permission implies it (`OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:100-115`).
- To confirm: on the site set up with the Blog recipe, open the Anonymous role and check that Allow is ticked for
  View all content, then open a role with no permissions (Proofreader, just created) and check that the Effective box
  of View all content is ticked. If the Effective box is cleared, the narration of `perms-2` is wrong: report it to the
  author instead of recording.

## 2. The reason shown after Edit content for others is allowed (`perms-7`)

- Checked in the source: `ViewContent` is implied by `EditContent` (`OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29`),
  and when an assigned permission implies the row, the tooltip names it
  (`Views/Admin/_RolePermissions.cshtml:91-98`), which takes precedence over the role source of question 1.
- To confirm: after saving, the tooltip of the Effective box of View all content reads "This permission is implied by:
  Edit content for others".

## 3. Tooltips in the capture (`perms-2`, `perms-5`, `perms-7`)

The tooltips are Bootstrap tooltips, opened by the cursor over the element. The scene API has no hover method, so the
plan uses `MoveToAsync`. To confirm: the tooltip appears in the headless capture and is still visible when the
narration of the step ends.

- The Effective box is a disabled check box inside a `div.form-check` that carries the tooltip
  (`OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:136-137`), and a browser may not pass
  mouse events from a disabled control to its parent. The plan moves the cursor to the center of the `div`, which is
  not on the check box; confirm the tooltip opens there.
- `perms-7` needs it: its narration says that hovering the Effective box shows the reason, so a capture without the
  tooltip cannot be used for this step.
- `perms-2` and `perms-5` do not depend on it: the box that is ticked and the Security Critical badge are on screen
  without any tooltip, and the plan only asks to wait up to 2 seconds for it.

## 4. Selectors that were read from the markup, not run

- The attribute `data-filter-value` (lower-case role name) and the class `item` on the row of a role in the List layout
  (`OrchardCore.Modules/OrchardCore.Roles/Drivers/RoleEntryDisplayDriver.cs:19-20`): confirm they are on the element that
  holds the System badge and the Edit and Delete buttons.
- The admin menu: the group Access Control is a button inside `figcaption#accessControl`, and its children are in
  `ul[data-title="Access Control"]`. The group has no page of its own (its href is "#",
  `OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:187`), so the theme draws it as a button that toggles
  the list (`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,76-86`); the id comes from the
  Users module (`OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:29`) and moves to the figcaption
  (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml:81-85,92`). The Roles module adds its item to the same
  group (`OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`).
- The labels "User name" and "Email" of the Create User page find their fields with `GetByLabel`.
- The Roles field of the Create User page lists Administrator, Author, Contributor, Editor, Guest Blogger, Moderator,
  and Proofreader, and not Anonymous or Authenticated (`OrchardCore/OrchardCore.Roles.Abstractions/RoleServiceExtensions.cs:16-32`).

## 5. Left out on purpose

- **Clone and Display.** The roles controller has a Clone action and a Display action, with views
  (`OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:223-242,322-389`). No view or menu in the source
  links to them, so the video does not mention them. To confirm: no button or menu item for them on the Roles page.
- **When a change to a role reaches users who are signed in.** Saving a role updates the security stamp of its users
  (`OrchardCore.Modules/OrchardCore.Users/Roles/Handlers/UserRoleUpdatedEventHandler.cs`), but how this affects a session
  that is open was not verified, so the narration says nothing about it.
- **The permissions the roles Author, Contributor, Editor, and Moderator start with.** Modules give default
  permissions to roles with these names (the stereotypes of each module's permission provider, applied by the role
  updater), and the narration says only that. The exact lists were not verified on a running site.
- **Content type permissions and the permissions for the owner of an item.** They belong to a separate video of the
  series.

## 6. Small things to look at while recording

- The user created in `05-assign` may show an "Unconfirmed" badge beside the name
  (`OrchardCore.Modules/OrchardCore.Users/Views/UserFields.cshtml:8`). The narration does not mention it.
- The order of the rows of the Roles list is the culture-aware order of the names
  (`OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:88`). The plan lists the order expected for
  `en-US`: Administrator, Anonymous, Authenticated, Author, Contributor, Editor, Guest Blogger, Moderator, Proofreader.
- The main button of the Edit role page (`button.save`) reads "Save" unless the item "and continue" of its drop-down was used
  in that browser, which makes the main button read "Save and continue"
  (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts:13-26,51-56`,
  `OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Edit.cshtml:25-40`). Never use that item; the narration of `perms-6`
  and the plan expect "Save", which returns to the list.

## 7. The address of the Roles page

The plan opens `/Admin/Roles/Index`, not `/Admin/Roles`. The roles controller declares the route template
`Roles/{action}/{id?}` (`OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:22`), and the admin route
mapper replaces `{action}` with the action name and adds no shorter route for Index
(`OrchardCore.Modules/OrchardCore.Admin/AdminAreaControllerRouteMapper.cs:36-52`). To confirm: `/Admin/Roles/Index` opens
the Roles page, and `/Admin/Roles/Edit/Proofreader` opens the permissions of the role once it exists.
