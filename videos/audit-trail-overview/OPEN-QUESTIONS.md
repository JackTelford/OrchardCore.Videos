# Open questions: audit-trail-overview

What was checked in the Orchard Core source (commit 92db4a83) and what a person must confirm on a running site before
the narration is recorded. Nothing below was run: the narration states only what the code and the documentation show.
Paths are relative to the root of the OrchardCore checkout. If an answer differs, change the narration of the step
named in brackets and narrate again.

## 1. Names and order of the tabs of Settings, Audit Trail  [`settings-1`, `settings-2`, `settings-3`]

Checked: the tabs come from the placement strings of the three settings drivers: "Content:1#Events"
(`src/OrchardCore.Modules/OrchardCore.AuditTrail/Drivers/AuditTrailSettingsDisplayDriver.cs:76`), "Content:10#Trimming;0"
(`.../AuditTrail/Drivers/AuditTrailTrimmingSettingsDisplayDriver.cs:39`) and "Content:10#Content;5"
(`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Drivers/ContentAuditTrailSettingsDisplayDriver.cs:41`). By the
grouping code (`src/OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:71-85`) the tabs should be Events,
Trimming and Content, in that order, with Events shown first.

Confirm: the three tab headers read exactly "Events", "Trimming" and "Content", the Events tab is the one open on arrival,
and the heading of the page (the view sets a title "Audit Trail" and a page title "Audit Trail Settings",
`.../AuditTrail/Views/AuditTrailSettings.Edit.cshtml:4-8`) looks as `settings-1` expects.

## 2. The events that creating, publishing and editing record  [`change-3`, `list-1` to `list-3`, `item-2`]

Checked: `AuditTrailContentHandler` records Created, Saved, Published, Unpublished, Removed, Cloned and Restored
(`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Handlers/AuditTrailContentHandler.cs:37-63`). Creating an Article with
the Publish button calls the content manager's create (a Created event) and then publish (a Published event)
(`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:769-808`,
`src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:604-660`). Editing a published item and clicking Publish
creates a second version and calls publish (`AdminController.cs:810-848`, `:938-975`).

Confirm: after the seed and the clip `04-change` the log holds exactly these rows, newest first: Published (Version 2,
admin), Published (Version 1, jamie.chen), Created (Version 1, jamie.chen), with no Saved event. The narration never counts
events or names a version number, but the plan (`list-3`, `item-1`) expects a Restore button on the second row and three
rows on the item's own log. If a Saved event appears, `list-3` should highlight the Restore button of the first row that
has one.

## 3. The sentence of a row and the links in it  [`list-2`]

Checked: the text is built from the template `.../Contents/Views/Items/AuditTrailContentEventData.SummaryAdmin.cshtml:41`
as "{Version N} of the {content type} {title} was {event}". The content type is the technical name of the type
(`contentItem.ContentType`), which is "Article" here. The version links to the read-only view of the item at that version
(`.../Contents/AuditTrail/Controllers/AuditTrailContentController.cs`, action Display, with the overlay
`.../Contents/Views/AuditTrailContent/Display.cshtml`), and the title links to the editor of the latest version
(`Orchard.EditForLinkAsync`). A Removed event shows the title without a link (`:30-37`).

Confirm: the sentence reads "Version 2 of the Article Pruning workshop for beginners was published" and both links exist.

## 4. The tab bar of the Details page  [`details-3`]

Checked: the Event Info and Content Item Info shapes have no tab, so they fall in the tab named "Content"; the
comparison has the tab "Diff" (`.../Contents/AuditTrail/Drivers/AuditTrailContentEventDisplayDriver.cs:108`). The grouping
code renders a tab bar only when there is more than one tab (`ZoneShapes.cs:70-85`). The documentation also names the
Diff tab (`src/docs/reference/modules/AuditTrail/README.md`).

Confirm: the tab bar shows the headers "Content" and "Diff", and the tab "Diff" is findable without scrolling. The
narration says "A Diff tab on this page compares the item with the way the previous event for that item recorded it" and does not open the tab, because
the tab shows the content of two versions as text, which the programme does not show in a video for administrators
(see the question for the specification in the result of this video).

## 5. Names in the user badge  [`list-1`, `list-5`]

Checked: the badge uses the `user-display-name` tag helper with the event's user name (`.../AuditTrail/Views/AuditTrailEventMeta.SummaryAdmin.cshtml:12`,
`src/OrchardCore.Modules/OrchardCore.Users/Views/UserDisplayName.SummaryAdmin.cshtml`), and the search box matches the
normalized user name by "contains" (`.../AuditTrail/Services/DefaultAuditTrailAdminListFilterProvider.cs:171-187`).

Confirm: the badges read "admin" and "jamie.chen", and searching for "jamie" keeps only the two rows of jamie.chen.

## 6. The Actions menu of a content item row  [`item-1`]

Checked: the entry "Audit Trail" is added to the ActionsMenu zone of a content item when the viewer holds the permission
View audit trail (`.../Contents/AuditTrail/Drivers/AuditTrailContentsDriver.cs:25-32`) and the default row layout shows
that zone under a button "Actions" (`src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`). The layout
of the actions is a site setting (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`, default Buttons).

Confirm: on the default site the row of the Article has a button "Actions" whose menu holds "Audit Trail", and the page it
opens lists only that item's events, with the lists Date, Events and Sort and without Category.

## 7. A daily task deletes old events  [`settings-2`]

Checked: `AuditTrailBackgroundTask` has the schedule "0 0 * * *" (every day at midnight) and calls
`TrimEventsAsync(RetentionDays)` unless trimming is disabled (`.../AuditTrail/Services/AuditTrailBackgroundTask.cs:13,19-41`);
the retention default is 10 days (`.../AuditTrail/Settings/AuditTrailTrimmingSettings.cs:5`); the manager deletes events
created at or before the threshold (`.../AuditTrail/Services/AuditTrailManager.cs`, `TrimEventsAsync`). The narration says "A
daily task deletes events older than that" and does not name a time of day or a time zone.

Confirm (optional, no recording needed): with the Background Tasks feature enabled, the task named "Audit Trail Events
Purger" is listed with this schedule.

## 8. Details that depend on the running site  [`enable-1`, `enable-2`, `list-4`, `list-5`]

- Features, search for "Audit Trail": the list should show "Audit Trail" and probably "Users Audit Trail" (its name
  contains the same words). The plan targets the Enable link by `a[href*="OrchardCore.AuditTrail"]`; confirm the link's
  address holds the feature id.
- Enabling a feature reloads the site's features, so the page after "Enable" may take several seconds
  (the plan allows `OffCameraAsync`).
- The three lists Date, Category and Sort are bootstrap-select widgets: confirm the button of each carries the title
  "Date", "Category" and "Sort" (`.../AuditTrail/Views/AuditTrailAdminListFilters.cshtml:6,9,15`) and that pressing Escape
  closes an open list without choosing an option.
- The menu of the Filters button (`#filter-dropdown`) lists the categories (here "Content") and "Filter syntax"
  (`.../AuditTrail/Views/AuditTrailAdminListSearch.cshtml:21-28`).
- Access Control, Users, Add User: confirm that Administrator is offered in Roles, and that the new user can sign in at
  `/Login` with the password that was typed (no confirmation of the email is needed).
- The Content Items search for "Pruning" finds the Article by its title (the default search term of the Contents list).

## 10. Permissions needed to open Settings, Audit Trail  [`notes-1`]

Checked: the page is served by the Settings module, whose action authorizes the permission "Manage settings" for the
group "AuditTrail" (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:53`). The module has no
permission registered for this group (only the General, Debugging and Email groups have one:
`src/OrchardCore.Modules/OrchardCore.Settings/Startup.cs:50-51`, `src/OrchardCore.Modules/OrchardCore.Email/Startup.cs:21`),
so the check falls back to "Manage settings" (`src/OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8,21`).
The three editors of the page also require "Manage audit trail settings" (`.../AuditTrail/Drivers/AuditTrailSettingsDisplayDriver.cs:37`,
`.../AuditTrail/Drivers/AuditTrailTrimmingSettingsDisplayDriver.cs:29`, `.../Contents/AuditTrail/Drivers/ContentAuditTrailSettingsDisplayDriver.cs:33`),
and the menu entry requires the same permission (`.../AuditTrail/Navigation/AuditTrailAdminMenu.cs:47`). The narration states
that both are needed, and that the Administrator role has all of them (the system role grants all permissions:
`src/OrchardCore/OrchardCore.Roles.Core/DefaultSystemRoleProvider.cs:22`).

Confirm (optional, no recording needed): sign in as a user whose role holds only "Manage audit trail settings": the entry
Settings, Audit Trail is listed but the page is refused; adding "Manage settings" to the role opens it. If the page opens
without "Manage settings", change `notes-1` and the slide in `slides.py`.

## 11. Requests of the Details page  [`details-1` to `details-3`]

Checked: the page of a Content event includes scripts for the Diff tab even while the tab is closed. One of them is a
public script CDN address
(`src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventDiff.DetailAdmin.cshtml:19-20`), and the
diff script of the module (`src/OrchardCore.Modules/OrchardCore.AuditTrail/wwwroot/Scripts/diffviewer.js`) carries a web-font
import. Nothing of it is on screen while the tab stays closed.

Confirm: the clip `06-details` records without a visible delay or an error banner on the machine that records, and the
network log of the browser shows no request that the plan does not expect.

## 9. Not used in the narration, on purpose

- The name under which the Audit Trail content part is listed in Add Parts. The part is `AuditTrailPart`, and its display
  name defaults to the part name without the word "Part" (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`),
  so the list may show "AuditTrail". The narration therefore says "a content part" and names only the label "Comment"
  (`.../Contents/Views/AuditTrailPart.Edit.cshtml:4`).
- The List and Grid layouts of the log. The module declares both (`.../AuditTrail/Views/AuditTrailAdminList.cshtml:29-37`),
  and whether a site shows the layout selector depends on an admin setting, so the video shows the default List layout
  and says nothing about the other.
