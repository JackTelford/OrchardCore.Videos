# Open questions: Drafts and Versions

Everything in the narration was read from the source at commit `92db4a83` of `OrchardCMS/OrchardCore` (paths below are
relative to its root) and from `src/docs/reference/modules/Contents/README.md`. What follows could not be seen on a
running site. A person confirms each point while recording the first take.

## To confirm on a running site

1. **Success messages last 5 seconds.** `NotifierExtensions.SuccessAsync` sets a dismissal of 5000 ms
   (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`; the toast is
   `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:19-20`). The steps that quote a message (`draft-3`,
   `publish-1`, `publish-4`, `unpublish-1`) say it right after the click. Check that each message is on screen while it
   is spoken, and move the click earlier if it is not.
2. **The Subtitle input name.** `input[name="Article.Subtitle.Text"]` comes from the prefix built in
   `src/OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentFieldDisplayDriver.cs:222-224` (type
   part name, field name) and the `Text` property of `src/OrchardCore.Modules/OrchardCore.ContentFields/Views/TextField.Edit.cshtml`.
   If the page names it differently, use the input labelled Subtitle.
3. **Empty Body and Banner Image.** The seeded articles Parking Information and Spring Event leave both empty. Nothing in
   the Article type marks them as required (`blog.recipe.json:128-186`), but confirm that Publish and Save Draft accept
   the form.
4. **The address `/about` after a draft and a publish.** The Article's Autoroute pattern builds the path from the title
   (`blog.recipe.json:156-158`) and the title never changes in the clips, so the path should stay `about`. Confirm that
   `/about` answers in `draft-4` and `publish-2`.
5. **Preview Draft opens a new tab** (`target="_blank"`, `ContentsButtonEdit_SummaryAdmin.cshtml:23`) that shows the
   draft with the site theme (`ItemController.Preview` loads the version flagged Latest, `ItemController.cs:44-67`). Confirm that
   the subtitle is visible in the page header of the Blog theme.
6. **Row order.** The list is sorted by Recently modified, so About should rise to the top after `draft-3`. Confirm that
   its row is easy to find before the edit, with the recipe's other items and the two seeded ones on the first page.
7. **The admin menu link** `#left-nav a[href$="/Contents/ContentItems"]` used to highlight Content Items in `draft-1`.
8. **Pronunciation.** Check "Draftable", "Versionable", "Unpublish" and "Unpublishing" with `scripts/pronounce.py`
   before narrating, and add a `spoken` entry to the storyboard if the voice misreads them.
9. **The replaced version.** Steps `versions-2` and `limits-2` say the replaced version "stays stored" and that older
   versions "stay in the database". The code saves the previous published version with `Published = false` and leaves it
   in place (`DefaultContentManager.cs:416-421`). The admin does not list it; only the database shows it. The narration
   does not call it "archived", because the Archive Later module uses that word for unpublishing.
10. **The Filters label.** The button shows only a funnel icon, and the word Filters is a plain title attribute of the
    icon (`ContentsAdminListSearch.cshtml:17`), which a screencast does not show. The narration of `find-2` therefore
    says "the Filters menu, the funnel button next to the search box", and the plan rings the button while those words
    are spoken. Confirm on the first take that the ring and the opened menu are enough for the viewer to find it.
11. **View opens a new tab.** Steps `draft-4` and `publish-2` click View, which opens the live page in a new tab
    (`target="_blank"`, `ContentsButtonEdit_SummaryAdmin.cshtml:18`). The plan hands the new page to `SwitchToAsync`, as
    `draft-5` does for Preview Draft. Confirm that the capture follows the new tab and that the front end page is fully
    drawn (header and subtitle) before the step ends.
12. **The search box after Only draft items.** The plan expects the box to show `status:Draft` after the click, since the
    link goes to `?q=status:Draft` (`ContentsAdminListSearch.cshtml:20`). The text is built from the filter result, so
    confirm it on the first take. If it shows something else, the step still holds, because the narration does not read it.
13. **The live page of an unpublished item.** `04-unpublish` never shows the front end after the unpublish. The page
    should answer with the theme's not found page, but that was not seen on a running site, so the narration only says
    that Unpublish takes the page out of public view. If a person confirms what `/about` shows, a step can be added to
    show it.

## Behaviors found in the code and left out of the narration

- **The Show list.** Its choices are Latest, Owned by me (only with the List content permission), Published, Unpublished
  and All versions (`AdminController.cs:187-195`). "All versions" runs the same query as the default
  (`x.Latest`, `DefaultContentsAdminListFilterProvider.cs:40-42`), so it does not list every version. "Unpublished" maps to
  the status Draft (`x.Latest && !x.Published`, `DefaultContentsAdminListFilterProvider.cs:29-31`), which also includes
  published items that have a pending draft. The video uses the Only draft items link of the Filters menu and describes
  it by that query.
- **Discard Draft permission.** The row menu shows Discard Draft when the user can edit the item
  (`ContentsButtonActions_SummaryAdmin.cshtml:28`), but the action requires the permission to delete content
  (`AdminController.cs:637`). A user with only the edit permission would see the entry and be refused. The narration of
  `limits-1` says that discarding a draft needs permission to delete content, and does not mention the entry that an edit-only
  user sees.
- **Discarding a draft of an item that was never published, or that was unpublished,** leaves no latest version
  (`DefaultContentManager.cs:949-974`), so the item disappears from the list, like a deleted item. The video discards a
  draft of a published item only. Confirm this on a site before a video ever shows it.
- **A content type that is not Versionable.** When such an item is edited, its single version is flagged as not
  published (`DefaultContentManager.cs:168-170`) instead of a separate draft version being created. The video says only that
  Versionable keeps the published version while you edit.
- **Audit Trail restores a recorded copy, not an archived version.** The event keeps its own copy of the content item
  (`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Models/AuditTrailContentEvent.cs:8`), recorded only while the
  Audit Trail feature is enabled and the content type is allowed in its settings
  (`.../AuditTrail/Handlers/AuditTrailContentHandler.cs:74`). `Restore` rebuilds a draft from that copy
  (`.../AuditTrail/Controllers/AuditTrailContentController.cs:88-125`, `DefaultContentManager.cs:845-885`). By default no
  content type is recorded (`.../AuditTrail/Settings/ContentAuditTrailSettings.cs:5`). The narration of `limits-2`
  therefore says "when the Audit Trail module is enabled and set to record a content type, it records that type's changes
  and can restore a recorded version as a draft", and never that it reads the older versions of the database.
- **Saving over an existing draft** updates that draft: a version is created when a draft is started on top of a
  published version (`DefaultContentManager.cs:159-181`). The narration never says that every save creates a version.
- **The Unpublish button of the editor** is drawn only when the item's version is published
  (`src/OrchardCore.Modules/OrchardCore.Contents/Drivers/ContentsDriver.cs:112-114`). The About article is published in
  `draft-2`, so the button is in the bottom bar of the editor on screen; the narration does not use it, and the clips unpublish
  from the list.
