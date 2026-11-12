# Open questions: Audit Trail: Compare and Restore Content Versions

What was checked in the Orchard Core source (commit `92db4a83`) and the documentation, and what a person has to confirm on a
running site before the first take. The narration states only what the source shows; the points below are what a source
reading cannot settle. If an answer differs, change the narration and the recording plan before narrating.

## Checked in the source

- Content events are recorded only for the types listed in the Content tab of Settings, Audit Trail, and that list is empty by
  default (`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Settings/ContentAuditTrailSettings.cs:5`,
  `.../Handlers/AuditTrailContentHandler.cs:74`). Narrated in `record-1` and `versions-3`.
- **Restore** is shown on Published events and on Removed events only, and not when the event's version is the latest version of the
  item (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:10,25`).
  Narrated in `versions-3`, `restore-1` and `deleted-3`.
- A restore adds a new draft version, leaves a published version in place, and records a Restored event
  (`src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:845-884`,
  `.../Contents/AuditTrail/Handlers/AuditTrailContentHandler.cs:62`). Narrated in `versions-2`, `restore-3`, `restore-4`, `deleted-4`.
- Viewing a version needs the permission to edit the item, restoring needs the permission to publish it, and the list needs View audit
  trail (`.../Contents/AuditTrail/Controllers/AuditTrailContentController.cs:70,103`,
  `src/OrchardCore.Modules/OrchardCore.AuditTrail/Controllers/AdminController.cs:57`). Narrated in `versions-4`.
- The retention period is ten days unless changed, and a daily task removes older events
  (`src/OrchardCore.Modules/OrchardCore.AuditTrail/Settings/AuditTrailTrimmingSettings.cs:5`,
  `.../Services/AuditTrailBackgroundTask.cs:12-13`). Narrated in `versions-3`.
- The Diff tab compares the event's stored copy with the copy stored by the previous Content event of the same item, whatever that
  event was (`.../Contents/AuditTrail/Drivers/AuditTrailContentEventDisplayDriver.cs:82-108`). Narrated in `compare-2` as "the previous
  recorded copy". The seed in `recording-plan.md` makes that copy the previous version.
- An edit followed by Publish records one Published event and no Saved event: the editor's Publish button runs the update and then the publish
  (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:548-554,938-958`), and only Save Draft calls `SaveDraftAsync`
  (`:525-534`). A new item records Created, then Published (`:769-790`, `:448-470`). The seed of `recording-plan.md` relies on this.

## To confirm on a running site

1. **Version numbers.** A line says "Version N", where N is the number of versions of the item in the database when the event is
   recorded (`.../AuditTrailContentHandler.cs:79-81`). Whether the count includes the version being saved depends on when the
   session writes its index rows. The narration names no number, but the plan expects the three Published lines to read Version 1 to 3
   with the Created line also Version 1, and the Restored line to read Version 4. Record what the list shows.
2. **Menus.** Settings, Audit Trail and Tools, Audit Trail are the menu paths in the source
   (`.../AuditTrail/Navigation/AuditTrailAdminMenu.cs:33-43`). Confirm how the admin theme shows them (collapsed groups, order) and that
   a click on each reaches the page; the plan lists the addresses as a fallback.
3. **Tabs.** The Settings page has tabs named Events, Trimming and Content, and the Details page has tabs named Content and Diff,
   from the placement strings in the drivers. Confirm the names, and that the Content tab of Settings can be reached by a click on
   the tab link, and whether the page keeps the tab after Save (the plan clicks it again).
4. **Row actions.** Confirm that the Content Items list shows the **Actions** button with **Audit Trail**, **Publish Draft** and
   **Delete** in its menu in the default actions layout, and the selectors of the plan.
5. **Diff colors.** The viewer is a bundled library; the colors are read from its minified default theme (light: added lines green,
   removed lines red). Confirm that the older copy is on the left and shaded red, the newer on the right and shaded green, that
   unchanged lines are collapsed, and that **Split** is selected first. The `compare-2` and `compare-3` narration depends on it.
6. **Internet access of the browser.** The Diff tab loads Prism from a public content delivery network
   (`.../Views/Items/AuditTrailContentEventDiff.DetailAdmin.cshtml:19-20`). Without it the Diff tab stays empty.
7. **Dialogs and messages.** Confirm the titles, texts and buttons of the two restore dialogs (Restore to Version, Restore content
   item; Yes and No), the delete dialog (Delete; Ok and Cancel), and the notification texts quoted in the plan. Confirm that a
   notification is still on screen when the narration of `restore-3` ends.
8. **Pronunciation.** Check with `pronounce.py` how the voice says "Diff" and "Ok", and add a spelling to `spoken` if needed. The
   narration uses the labels as written on screen.
9. **Restore validation.** A restore validates the copy before it creates the draft; if validation fails, the page shows "was not restored,
    the version is not valid" instead of the success message (`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Controllers/AuditTrailContentController.cs:108-123`,
    `src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:862-868`). The Autoroute path check excludes the item itself
    (`src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:119-133`), so the plan expects both restores to succeed.
    Confirm this for the second version of Harvest Festival and for the deleted Summer Parking Notice before the first take.
10. **Removed events of a discarded draft.** `DiscardDraftAsync` also raises the removed handlers (`DefaultContentManager.cs:967`), so discarding
    a draft records a Removed event. The seed never discards a draft; confirm the list holds no such line.
