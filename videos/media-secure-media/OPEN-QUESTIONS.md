# Open questions: Secure Media

Everything in the narration was read from the Orchard Core source (commit 92db4a83) and the documentation. The list
below is what could not be run in this session and must be confirmed on a running site before or while recording.
Source paths are relative to the Orchard Core checkout.

## 1. How the header looks without its picture (`after-1`)

- Checked: the Blog home page draws its header picture from the Media Library root folder
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:4-9`, `Recipes/blog.recipe.json:985` and
  `:1124-1139`), and `SecureMediaMiddleware` answers an unauthorized request for `/media/...` with 404
  (`src/OrchardCore.Modules/OrchardCore.Media/Services/SecureMediaMiddleware.cs:50-57`), whatever the query string.
- To confirm: that the header without the picture is clearly different from the header with it, as a visitor. If it is
  not, replace the visitor clips with a visit to a file address (`/media/home-bg.jpg`) and change the narration of
  `before-1`, `after-1` and `visitor-1` to say what that page shows. Diagnostics skips its error page for a path with a
  known file extension (`src/OrchardCore.Modules/OrchardCore.Diagnostics/DiagnosticsStartupFilter.cs:29 and :42`), so the
  browser's own error page would show.

## 2. Both messages after enabling (`enable-3`)

- Checked: the success message is raised by the Features controller (`.../OrchardCore.Features/Controllers/AdminController.cs:317`)
  and the warning by `SecureMediaFeatureEventHandler.EnablingAsync` (`.../OrchardCore.Media/Events/SecureMediaFeatureEventHandler.cs:15-24`).
- To confirm: that both are shown on the page the enable link redirects to, and that the warning's links read
  "Roles page" and "secure media documentation". The narration says a message and a warning, without reading either
  aloud (the warning contains the banned word).

## 3. The reload after enabling (`enable-2`)

- To confirm: how long the tenant takes to answer after "Enable", and that
  `OffCameraAsync(() => scene.WaitForNavigationAsync(...))` cuts the wait. If the page is back in a second or two,
  use `ClickAndWaitForNavigationAsync` on camera instead.

## 4. The default permissions after enabling (`enable-4`)

- Checked: the permission provider gives the Authenticated role "View own media content" and Administrators everything
  (`.../OrchardCore.Media/SecureMediaPermissions.cs:60-82`), and `RoleUpdater` adds the default permissions when the
  feature is installed (`.../OrchardCore.Roles/Services/RoleUpdater.cs:43-94`).
- To confirm: after `02-enable`, Access Control, Roles, Authenticated has "View own media content" ticked in the Allow
  column, and no other role has a Secure Media permission ticked. If not, change the narration of `enable-4`.

## 5. The rows of the permission list (`anon-2`, `editor-2`)

- Checked: the list shows one "View media content in folder" row for each first-level folder of the media root except
  `_Users` and the attached-fields folder (`SecureMediaPermissions.cs:128-149`), rebuilt at least every five minutes and
  when a folder is created or deleted in the Library (`SecureMediaPermissions.cs:48-57`,
  `.../Events/SecureMediaFileStoreEventHandler.cs`). The roles page filters rows by a case-insensitive substring of
  their description (`.../OrchardCore.Roles/Assets/ts/roles-edit.ts`).
- To confirm: that searching "media content" leaves exactly the six rows of the plan, in that order, and no row of
  another group; and that `press` and `contracts` are both listed on the first visit (they exist before the feature is
  enabled).

## 6. Seeding the folders from the Library

- Checked in the source only: the folder menu of the Library (`FolderMenu.vue`), the item "Create a subfolder" and the
  box "Enter a folder name" that submits on Enter (`ModalFolderAction.vue`).
- To confirm: that this is how a folder is created in the running Library, and that an empty folder is kept (a folder
  that disappears when empty would have no permission).

## 7. The effective column after saving (`editor-5`)

- Checked: `GetEffectivePermissions` (`.../OrchardCore.Roles/Controllers/AdminController.cs:451-476`) builds a user with
  the role and asks the authorization service, so the `press` row is ticked in both columns and `contracts` in neither.
- To confirm: on the page. Also confirm that the root folder row is ticked in the Effective column because the
  Anonymous role has the permission (the narration only talks about `press` and `contracts`).

## 8. A folder permission also opens the root (`perm-3`)

- Checked: the root permission published by the provider is implied by every first-level folder permission
  (`SecureMediaPermissions.cs:118-123` and `:147`), and the authorization handler asks the provider for that instance
  (`.../Services/ViewMediaFolderAuthorizationHandler.cs:86-91` and `:130-146`). The Media documentation says only that
  "View media content in the root folder" is implied by "View media content in all folders".
- To confirm: on a site where the Anonymous role has no root permission, give a role only the `press` permission and
  check that the Effective column of "View media content in the root folder" is ticked for that role.

## 9. The Media Library lists only permitted folders (`know-1`)

- Checked in the source only: the folder endpoints keep a folder only when `ManageMediaFolder` is granted for its path,
  and that handler also requires the view permission when Secure Media is enabled
  (`.../Endpoints/Api/GetFoldersEndpoint.cs:62-68`, `.../Endpoints/Api/MediaEndpointHelpers.cs:74-82`,
  `.../Services/ManageMediaFolderAuthorizationHandler.cs:79-89`).
- To confirm (not recorded): sign in as a user with "Manage Media" and only one folder permission, and check that the
  Library lists that folder only.

## 10. Pronunciation

- "404 Not Found" appears in `why-2`, `after-1` and `outro`. Check how the voice says "404" with `scripts/pronounce.py`
  (for example "four hundred four" or "four oh four"). If it must change, add a `spoken` entry in `storyboard.json`
  after a person confirms the spelling.

## 11. The heading of the Secure Media rows (`anon-2`, `editor-2`)

- Checked: the roles page groups permissions by the last enabled feature the provider type belongs to
  (`.../OrchardCore.Roles/Controllers/AdminController.cs:295-300` and `:427-441`). The provider type is registered by
  the Secure Media startup (`.../OrchardCore.Media/Startup.cs:436-446`), which adds it to the Secure Media feature after
  the Media feature (`src/OrchardCore/OrchardCore/Shell/Builders/ShellContainerFactory.cs:191`,
  `src/OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:208-222`). The documentation lists the permissions under
  "Secure Media (OrchardCore.Media.Security)".
- To confirm: the group is headed "Secure Media" with the hint "(OrchardCore.Media.Security)", not "Media".

## 12. The order of the two messages (`enable-3`)

- Checked: the warning is raised while the feature is being enabled (`SecureMediaFeatureEventHandler.cs:15-24`), the
  success message after it (`.../OrchardCore.Features/Controllers/AdminController.cs:317`).
- To confirm: both are on the page after the redirect, and in which order. The narration names the success message
  first; if the warning is shown above it, swap the two rings of `enable-3`, which do not move the narration.

## 13. Where the Effective column gets its ticks (`editor-5`)

- Checked: `GetEffectivePermissions` asks the authorization service about a user who holds the role
  (`.../OrchardCore.Roles/Controllers/AdminController.cs:451-476`), and the handler of the Roles module adds the
  permissions of the Anonymous role, and of the Authenticated role for a signed-in user
  (`.../OrchardCore.Roles/Services/RolesPermissionsHandler.cs:36-52`). The tooltip of an Effective box names the role
  (`.../OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:177-190`). The Roles documentation describes the Anonymous
  role as representing non-authenticated users only; the narration of `anon-3` and `editor-5` follows the code.
- To confirm: in the Editor role, the Effective box of "View own media content" is ticked, and its tooltip reads
  "This permission is granted by role: Authenticated".
