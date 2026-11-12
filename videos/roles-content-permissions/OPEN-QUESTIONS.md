# Open questions: Content Permissions: By Content Type and by Owner

Everything in the narration was read from the Orchard Core source (commit `92db4a83`) and the documentation. The items
below could not be confirmed without a running site. Each one says what was checked in the source, what to confirm, and
which steps depend on it. If a check fails, change the narration of those steps before narrating.

## 1. Defaults of the Anonymous and Authenticated roles

- **Source:** `OrchardCore.Modules/OrchardCore.Contents/Permissions.cs:86-96` gives the stereotypes Authenticated and Anonymous
  the permission `ViewContent` ("View all content"); `OrchardCore.Modules/OrchardCore.Roles/Services/RoleUpdater.cs` adds the
  permissions of a stereotype to the role of the same name when the role is created or the feature is installed. The system
  roles Anonymous and Authenticated are created through the role manager by
  `OrchardCore.Modules/OrchardCore.Roles/Migrations/SystemRolesMigrations.cs:24-33`, which raises the same role-created
  event, so either order of installation (Roles before Contents or after) gives them the permission. Anonymous and
  Authenticated permissions are applied to every user by `Roles/Services/RolesPermissionsHandler.cs:43-52`.
- **Confirm:** on the Blog recipe site, the role editors of `Anonymous` and `Authenticated` show **Allow** ticked for **View all
  content**; and, after `role-7`, the `Writer` role shows **Effective** ticked for **View Article by others** and **View own
  Article**, with the tooltip "This permission is granted by role: Anonymous, Authenticated"
  (`Roles/Views/Admin/_RolePermissions.cshtml:113`).
- **Steps:** `role-8`, `writer-1`, `writer-5`, `recap-3`, and the second half of `recap-2`.

## 2. Permissions of the Editor role

- **Source:** `Contents/Permissions.cs:53-62` gives the Editor stereotype `PublishContent`, `EditContent`, `DeleteContent`,
  `PreviewContent`, `CloneContent` and `ListContent`. The Blog recipe creates the role `Editor` with an empty list of
  permissions (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:88-112`), and the role updater then adds the
  stereotype's permissions.
- **Confirm:** the `Editor` role has **Allow** ticked for **Edit content for others**, **Publish or unpublish content for others**
  and **Delete content for others**, and, once `Article` is Securable, its Article group has an empty **Allow** column and **Effective**
  ticked on every row but **Edit the owner of a Article content item**.
- **Steps:** `editor-2`, `editor-3`.

## 3. What the writer sees in the list

- **Source:** the list filter of `Contents/Services/DefaultContentsAdminListFilterProvider.cs:121-217` shows the items of a
  type to a user who can view it, so with **View all content** the writer sees every article. The title is a link only when
  the user may edit the item (`Contents/Views/ContentsTitle_SummaryAdmin.cshtml:8`), and the **Edit** button is shown only then
  (`ContentsButtonEdit_SummaryAdmin.cshtml:11`). The **New Article** button reads `New {type}` when exactly one type can be created
  (`ContentsAdminListCreate.cshtml:12-14`), and a type is offered only when the user passes the `EditContent` check for a
  new item they own (`Contents/Controllers/AdminController.cs:875`, and `:412` for the `Create` action); `writer-1` says the button
  is there because creating an article needs **Edit Article**, which publishing implies.
- **Confirm:** with the filtered list `/Admin/Contents/ContentItems/Article`, signed in as `jamie.chen`: **New Article** is a single
  button (not a drop-down), `About` and `Press Kit` show plain titles and no **Edit** button, and `Team Offsite Notes` shows a
  linked title and **Edit** after publishing. Also confirm that a green **View** button on the published rows does not distract
  from the story.
- **Also confirm:** the page opened by `/Admin/Contents/ContentItems/Article` is titled **Article** with the breadcrumb
  **Manage Content** (`Contents/Views/Admin/List.cshtml:17-25`), not **Content Items**; the narration of `writer-1` says
  "the list of articles" for that reason. Each row shows the author's name beside the title
  (`Contents/Views/ContentsMeta_SummaryAdmin.cshtml:15-18`): confirm that the names on screen are `admin` and `jamie.chen`
  only.
- **Steps:** `writer-1` to `writer-5`.

## 4. Publishing as the writer

- **Source:** `Contents/Controllers/AdminController.cs` (`Create`, `CreateAndPublishPOST`, `CreateInternalAsync`) authorizes the
  owner variation of the permissions for a new item; `PublishOwn_Article` implies `EditOwn_Article`
  (`OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:16`). The **Publish** button is drawn when `PublishContent` is
  granted for the item (`Contents/Drivers/ContentsDriver.cs:110`).
- **Confirm:** the editor opened from **New Article** shows **Publish** and **Save Draft**, publishing returns to the list, and
  the notice "Your Article has been published." is not a problem for the frame.
- **Steps:** `writer-2`, `writer-3`.

## 5. The Effective column after saving

- **Source:** the column is computed on the server by `Roles/Controllers/AdminController.cs:451-476` with a fake user holding the
  role's permissions; it is not updated by the browser when a box is ticked. **Edit Article** is implied by **Publish or
  unpublish Article** because the dynamic permission `EditOwn_Article` lists `PublishOwn_Article` among the permissions that
  imply it.
- **Confirm:** after **Save**, **and continue**, **Effective** is ticked on **Edit Article**, **List Article content items**,
  **Publish or unpublish Article**, and the two View rows of item 1; and the tooltip of **Edit Article** reads "This permission is
  implied by: Publish or unpublish Article". Confirm that the Bootstrap tooltips show on hover in the recorder's headless
  browser and stay long enough for `role-8`.
- **Steps:** `role-7`, `role-8`.

## 6. The group appears as soon as the type is Securable

- **Source:** `Contents/Security/ContentTypePermissions.cs:18-27` lists the dynamic permissions of the Securable types each time
  the role editor is built.
- **Confirm:** the group **Article Content Type - Article** is in the `Writer` role editor right after `02-securable`, with no
  restart. Also confirm the title of the group (Display Name, "Content Type -", technical name), which is built from
  `ContentTypePermissionsHelper.cs:155` and `PermissionTemplate.cs:36`.
- **Steps:** `role-5`, `editor-1`, `editor-2`.

## 7. Types created in the admin are Securable

- **Source:** `OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:89` calls `Securable()` when
  a type is created with **Create new type**.
- **Confirm:** create a type with **Create new type** on a spare site and check that **Securable** is ticked in its settings.
- **Step:** `securable-3`.

## 8. Signing in as the second user

- **Source:** users created in the admin are enabled and need no email confirmation unless the registration setting
  `UsersMustValidateEmail` is on (`Users/Handlers/EmailConfirmationLoginFormEvent.cs:31`; it is off by default).
- **Confirm:** `Recorder setup` with the user name `jamie.chen` signs in and writes a working sign-in file, and the first page
  of `05-writer` opens without a redirect to the sign-in page.
- **Steps:** all of `05-writer`.

## 9. Pronunciation (a person must listen)

Check with `scripts/pronounce.py` before narrating, and add a `spoken` entry to `storyboard.json` for any word that is misread.
`storyboard.json` already maps "Save & Configure" to "Save and Configure" and "Article Content Type - Article" to "Article
Content Type, Article" for the voice; listen to check these, and the other words:

- "Article Content Type - Article" in `role-5` (the label has a hyphen between spaces; the voice may read it as a pause, a
  dash or "minus"; if so, write the narration with a comma instead and say the label's parts);
- "Securable" (`concepts-1`, `securable-1` to `securable-3`, `recap` slides);
- "Save & Configure" in `role-2` (the ampersand should be read as "and");
- "Anonymous" and "Authenticated" in `role-8`, `writer-5` and `recap-2`, `recap-3`.

## 10. Layout of the admin lists

The plan's selectors for the Content Types and Roles lists assume the default **List** layout of the admin lists. In the **Grid**
layout, the same buttons sit in the **Actions** column of a table row (`tr` instead of `.list-group-item`). The clips keep the
site's default layout; confirm the default before recording.

## 11. The Save menu of the role editor

- **Source:** the **Save** button group of the role editor (`Roles/Views/Admin/Edit.cshtml:25-40`) has a menu entry
  **and continue**; the admin theme stores the last entry used in local storage and, on the next page load, swaps the main
  button to **Save and continue** (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts:13-24`, `:51-57`).
- **Confirm:** at `role-7` the main button reads **Save** and the menu entry beside it reads **and continue**; after the
  step, the page has the main button **Save and continue**, which is expected and not narrated.
- **Step:** `role-7`.

## Not narrated, not verified

- What the writer sees when opening the **Edit** address of an article that belongs to someone else (the controller returns
  Forbid, `Contents/Controllers/AdminController.cs:512-514`); the video does not show it.
- Changing the owner of an item: the owner editor needs the Common part with **Display owner editor** ticked and the
  permission **Edit the owner of a Article content item** (`Contents/Drivers/OwnerEditorDriver.cs:34-58`); the video only
  names the permission (`concepts-2`).
