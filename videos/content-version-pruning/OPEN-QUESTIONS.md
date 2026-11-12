# Open questions: Content Version Pruning

What the author could not verify from the source or the documentation, and what a person must confirm on a running
site before the video is recorded or published. The narration claims only what the source shows; the items below are
checks on a site, not facts the video relies on without them.

## 1. The effect of a run is not visible in the admin

The video promises a successful run (the Last run badge), never a count of deleted versions.

- **What was checked in the source**: the rules in `VersionPruning/Services/ContentVersionPruningSelector.cs` and
  `ContentVersionPruningService.cs` (the published and the most recent version are never selected; the newest
  "Archived versions to keep per item" archived versions of an item stay; the rest are deleted when `ModifiedUtc` is
  before now minus the retention period, or empty), and `ContentVersionPruningBackgroundTask.cs` (the task returns
  at once when Disable is selected; the number of deleted versions is only logged at the Debug level).
- **Why the screen cannot show it**: no admin page lists archived versions. The status filter "All versions" of the
  Content Items list shows only the most recent version of each item
  (`OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs`, case `ContentsStatus.AllVersions`), so
  it cannot be used to show them. The Blog recipe imports seven content items, each both the most recent and the
  published version of its own content item (`blog.recipe.json`, the `content` step), so the fresh site has no archived
  version at all and a run deletes nothing, which is what the narration of `task-4` says.
- **To confirm on a throwaway site** (never the recording site): publish a Blog Post, then edit and publish it
  three times; with a database tool, check that the `Document` and `ContentItemIndex` rows hold one version that is
  both published and the most recent, and three archived ones (neither `Latest` nor `Published`);
  set Retention period (days) to 0 and Archived versions to keep per item to 1, select Blog Post, and run the task
  on demand; check that the published version and the newest archived version remain and the two older archived
  versions are gone. Put the settings back afterwards or discard the site.

## 2. Checks on the first take

- On the fresh Blog recipe site, neither Content Version Pruning nor Background Tasks is enabled, and there is no
  Background Tasks page (the recipe was read; the site was not run).
- The Features search `pruning` leaves one row, under the heading "Content Management", and `Background Tasks` shows
  its row under the heading "Tasks".
- After Content Version Pruning is enabled, Content Version Pruning appears in the Settings menu of the admin
  menu for `admin`.
- On the Background Tasks page the row of the task shows "Disabled" before Enable and "Enabled" after it, a Next run
  badge with a relative time such as "in 7 hours" (not "due"), and an enabled item "Run now" in the Actions menu once the background service loaded
  the task (it shows "Waiting for the background service to load this task" until then). The recording plan waits
  for the status Idle.
- The run triggered by "Run now" ends with the Last run badge in its success color, and not with an error under the
  description of the task.

## 3. The time zone of the schedule

The narration says the task runs every day at midnight, in the time zone of the site. The source evaluates the
schedule in the time zone of the site settings (`BackgroundTaskScheduler.cs`, `CanRun` and `GetNextStartTime`), and
the default schedule of the task is daily at midnight (`ContentVersionPruningBackgroundTask.cs`). A person confirms,
with the title (tooltip) of the Next run badge, that the next run is at midnight in `America/Los_Angeles`.

## 4. Behavior claimed from the code only

- A content type with Versionable selected keeps the version it replaces when a published item is changed and
  published; a type without it does not (`OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs`, the
  draft-required branch of `GetAsync`, and `BuildNewVersionAsync`). The Versionable check box is on the editor of
  the content type (Blog Post and Page have it selected in the Blog recipe).
- The age of an archived version counts from its last modification, not from the day it was replaced: archiving a
  version does not change its `ModifiedUtc`. The narration says "last modified".
- The permission "Manage Content Version Pruning settings" is granted to the Administrator role by default. A
  person may confirm it on Access Control, Roles.
- A version without a modification date (`ModifiedUtc` empty) is deleted whatever the retention period, once it is
  not among the archived versions kept (`ContentVersionPruningSelector.cs`). The narration of `settings-3` therefore
  says only that a version modified within the period is not deleted. Versions created through the editor always have
  a date (`UpdateContentsHandler.cs`), so this concerns imported content only.

## 5. The permissions needed by other roles

The narration of `settings-1` says that a role other than Administrator needs both "Manage settings" and "Manage
Content Version Pruning settings" to open the page.

- **What was checked in the source**: the settings page authorizes `ManageGroupSettings` for the group
  (`OrchardCore.Settings/Controllers/AdminController.cs:53`), which `ManageSettings` implies
  (`OrchardCore.Settings.Core/SettingsPermissions.cs`). A group permission can replace it only when the module registers
  one with `AddSiteSettingsPermission` (`SiteSettingsAuthorizationHandler.cs`), and `VersionPruning/Startup.cs` does not.
  The editor of the module then returns nothing without "Manage Content Version Pruning settings"
  (`ContentVersionPruningSettingsDisplayDriver.cs:31-36`), and the controller answers Not Found when the editor is
  empty (`AdminController.cs:67`).
- **To confirm on a throwaway site** (never the recording site): create a role with only "Manage Content Version
  Pruning settings", assign it to a test user, and check that the menu shows Settings, Content Version Pruning but the
  page is refused (access denied); add "Manage settings" and check that the page opens. The documentation page says that the
  first permission is enough (reported as a documentation issue). If the page opens with the first permission alone,
  put the original sentence back in `settings-1`.
