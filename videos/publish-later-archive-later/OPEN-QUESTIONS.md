# Open questions: Publish Later and Archive Later

Everything in the narration was read from the source (Orchard Core commit 92db4a83) and the two documentation pages. The
points below were read from the code only, or not checked at all, and a person must confirm them on a running site before
recording. None of them is stated in the narration as a fact beyond what the code shows.

## 1. The labels of the two parts on the "Add Parts" page

- **Checked:** `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:33` writes the display name of each part, and
  `OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19` takes it from
  `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`,
  which removes the ending "Part" from the part's name and does not add a space. The migrations
  (`OrchardCore.PublishLater/Migrations.cs:20-22`, `OrchardCore.ArchiveLater/Migrations.cs:21-23`) set no display name. By the
  code the labels read PublishLater and ArchiveLater. The documentation says "Publish Later Part" and "Archive Later Part".
  The Edit Content Type page writes the same text (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:152`), and the message after
  "Add Selected Parts" lists the names of the parts (`PublishLaterPart`, `ArchiveLaterPart`:
  `OrchardCore.ContentTypes/Controllers/AdminController.cs:372`), not the labels.
- **In the narration:** step `parts-2` writes the labels as the code does ("check PublishLater and ArchiveLater"), so the
  subtitles match the screen, and the storyboard's `spoken` map makes the voice say them with a space.
- **To confirm:** the exact text of the two check boxes on the Add Parts page and of the two rows under Parts on the Edit
  Content Type page. If the screen shows a space ("Publish Later", "Archive Later"), change the text of `parts-2` in
  `storyboard.json` and remove the two `spoken` entries.

## 2. The action bar of the editor

- **Checked:** the two groups (a date and time field with a button) are rendered in the Actions zone of the editor
  (`OrchardCore.PublishLater/Drivers/PublishLaterPartDisplayDriver.cs:41`, position 10;
  `OrchardCore.ArchiveLater/Drivers/ArchiveLaterPartDisplayDriver.cs:38`, position 10.5), as secondary actions of a bar
  fixed at the bottom (`src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss:14`). The Publish button is
  also at position 10, so the order of the two is not settled by the code.
- **To confirm:** that at 1536 x 864 pixels, with the admin's default actions layout (Buttons), the bar shows both groups
  inline and not in the "More actions" menu (which the bar uses on narrow windows or with the Menu layout), the order of the
  buttons, and that both date and time fields are readable. If the bar wraps or hides a group, the steps `publish-2` and
  `archive-1` need a different highlight.

## 3. The public address of the article

- **Checked:** the Article type of the Blog recipe builds its path from the title with a slug pattern
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the AutoroutePart of Article), and the path is generated
  when the item is created (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:139-143`), so the address of the
  article should be `/spring-sale-announcement`.
- **To confirm:** that address, what the Blog theme shows on it (step `publish-6` expects the article's title), and that it
  answers 404 before the first publication and after the archive (the two `OffCameraAsync` waits depend on it; the plan gives a
  fallback that polls the list).

## 4. Archive Later publishes the item

- **Checked:** the Archive Later and Cancel Archive Later buttons post `submit.Publish`
  (`OrchardCore.ArchiveLater/Views/ArchiveLaterPart.Edit.cshtml:12,19`), which the Contents module handles as
  "save and publish" (`OrchardCore.Contents/Controllers/AdminController.cs:546-551`, `:938-980`). The narration of
  `archive-2` and `cancel-3` says so. The Publish Later buttons post `submit.Save`, which only saves a draft.
- **To confirm:** the messages after each button ("Your Article draft has been saved." after Publish Later and Cancel Publish
  Later, "Your Article has been published." after Archive Later and Cancel Archive Later), and that the item is published
  with the scheduled archive time kept. The documentation does not mention this behavior (see the documentation issues).

## 5. How long the waits are

- **Checked:** both tasks use the schedule every minute
  (`OrchardCore.PublishLater/Services/ScheduledPublishingBackgroundTask.cs:14`,
  `OrchardCore.ArchiveLater/Services/ScheduledArchivingBackgroundTask.cs:15`), and the background service waits at least the
  polling time of one minute and the minimum idle time of ten seconds between runs
  (`src/OrchardCore/OrchardCore.Abstractions/Modules/BackgroundServiceOptions.cs:18,23`), and runs a tenant's tasks only after the
  tenant has served a request unless the shell warm-up option is on (same file, line 13).
- **To confirm:** the longest delay seen between the chosen time and the change (the plan assumes under 3 minutes after the
  time, and polls for up to 6 minutes), and that the first run after a restart of the site is not late.

## 6. Relative texts and tooltips

- **Checked:** the schedule badges show a relative text (for example "in 3 minutes") through the Timespan shape
  (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:34-113`) and the exact time in a tooltip
  (`OrchardCore.PublishLater/Views/PublishLaterPart.SummaryAdmin.cshtml:10`, "Scheduled to be published on {0}";
  `OrchardCore.ArchiveLater/Views/ArchiveLaterPart.SummaryAdmin.cshtml:10`, "Scheduled to be archived on {0}").
- **To confirm:** that the tooltip appears on a mouse move in the recorder's browser and stays visible until the end of the
  step, and that the narration's "a clock badge" and "a badge for the scheduled archive time" match what is drawn (a clock
  icon, and a box icon).

## 7. Not checked at all

- A content type that is not marked Draftable: the editor then has no Save Draft button, but the Publish Later button still
  posts `submit.Save`. Not read further, and not shown in the video (the Article type is Draftable).
- Both parts on one item with both times filled in: any button of the editor stores the two times that are in the fields
  (`OrchardCore.PublishLater/Drivers/PublishLaterPartDisplayDriver.cs:44-62`,
  `OrchardCore.ArchiveLater/Drivers/ArchiveLaterPartDisplayDriver.cs:44-58`), and an archive schedule is only indexed while the
  saved version is both published and the latest one (`OrchardCore.ArchiveLater/Indexes/ArchiveLaterPartIndexProvider.cs:50`).
  The effect of combining them was not run, and is not in the video.
- A Save Draft on a published item that has an Archive Later time: the archive time is only indexed while the saved version
  is both published and the latest one (`OrchardCore.ArchiveLater/Indexes/ArchiveLaterPartIndexProvider.cs:50`, and the task
  query, `OrchardCore.ArchiveLater/Services/ScheduledArchivingBackgroundTask.cs:32`), so by the code a newer draft pauses the
  archive until it is published. Not run, and not in the video.
- Behavior with the User Time Zone feature enabled, and around a daylight-saving change. Read in the code only: the local
  clock sorts the zones offered by the site (priority 0) and by the signed-in user (priority 100) and takes the first one
  that is not empty (`src/OrchardCore/OrchardCore/Modules/Services/LocalClock.cs:67-102`,
  `OrchardCore.Settings/Services/DefaultTimeZoneSelector.cs`, `OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:22-38`).
  Step `know-1` says only that a zone set for a user takes precedence. A skipped or repeated local time is refused
  (`LocalClock.cs:55`, `AtStrictly`), as the documentation warns. Neither case is shown in the video.
