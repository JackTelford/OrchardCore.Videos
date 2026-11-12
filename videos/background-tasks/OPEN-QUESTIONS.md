# Open questions: Background Tasks: See, Schedule and Run Them

Everything in the narration was read from the Orchard Core source (commit `92db4a83`) and the documentation page of
the module. The points below could not be settled from them, or depend on behaviour that only a running site shows.
A person should confirm each one on the recording site (see `recording-plan.md`) before the take counts.

1. **What the Schedule field accepts.** The narration says only "cron expression". The edit action validates the
   schedule with `CrontabSchedule.TryParse(schedule)` (`OrchardCore.Modules/OrchardCore.BackgroundTasks/Controllers/BackgroundTaskController.cs:403`),
   whose source (NCrontab) is not in the checkout. Every default schedule in the source has five fields, which is why the
   video types `*/10 * * * *`. Confirm that this value is accepted, that `every night` is rejected with "The schedule must
   be a valid cron expression.", and what happens with a six-field expression (seconds). The video does not claim either way.
2. **The schedule follows the site's time zone.** `schedule-2` says so. It is read from `BackgroundTaskScheduler.CanRun` and
   `GetNextStartTime` (`OrchardCore/OrchardCore/BackgroundTasks/BackgroundTaskScheduler.cs:49-53, 186-210`), which convert the
   clock to the time zone of the scheduler, and from `ModularBackgroundService.UpdateAsync` (line 322), which gives each scheduler the
   time zone of the site settings. The documentation page does not mention it. Confirm on the recording site (time zone
   `America/Los_Angeles`): on the details page of a task that has the default schedule `0 0 * * *`, the Next run date and time
   must read midnight in that time zone.
3. **The List and Grid buttons.** The code resolves the selector from the site setting "Let users choose the layout of a list",
   which is on by default (`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:49`, applied by
   `OrchardCore.Admin/Configuration/AdminListOptionsConfiguration.cs:40`). Confirm that the two buttons show in the toolbar of
   Tools, Background Tasks on a fresh Blog site, and that picking Grid shows the columns Title, Status, Last run, Next run and Actions.
4. **How soon the Next run badge follows a saved schedule.** The scheduler learns the new schedule when the background service next
   checks the tenant (by default the polling time is one minute, plus the minimum idle time). `schedule-4` relies on this and the plan
   waits off camera for the badge to change. Confirm the delay stays under the 150 seconds of the plan.
5. **Whether the row is drawn as Queued when a task is run on demand.** The controller queues the run and redirects. The background
   service ends its wait early when a run is requested, then starts the run one second later only if its wait began less than
   10 seconds before, and at once otherwise (`OrchardCore/OrchardCore/Modules/ModularBackgroundService.cs:374-392`). A task that takes a
   few milliseconds can therefore end before the redirected page is drawn, and the row then shows the result at once. `run-2` says
   the row follows the run without reloading, which the page script does (`background-task-state.ts`), but a take may not show it.
   The recording plan has a timing note (start `run-1` just after a check of the service). Confirm on the recording site that it
   gives a Queued or Running badge at the first frame of `run-2`, or accept a take where the badge already shows the result.
6. **The tasks run without the Background Tasks module.** `why-1` says so. The source shows that the background service is registered
   by the application, not by the module (`OrchardCore/OrchardCore.Application.Cms.Core.Targets/ServiceExtensions.cs:29`,
   `OrchardCore/OrchardCore/Modules/Extensions/OrchardCoreBuilderExtensions.cs:97-103`), and that it reads the settings provider as optional
   and falls back to the default settings of each task (`OrchardCore/OrchardCore/Modules/ModularBackgroundService.cs:313, 357`,
   `settings ??= task.GetDefaultSettings()`). It was never run. Confirm on a site where the module is disabled: the Scheduled Content
   Items Publisher keeps running (a content item scheduled with Publish Later is published).
7. **Running Content Version Pruning Background Task on demand** on the fresh site (`run-3`). The task prunes nothing when no content
   type is selected, which is the default (`OrchardCore.Modules/OrchardCore.Contents/VersionPruning/Services/ContentVersionPruningService.cs:38-39`,
   `ContentVersionPruningSettings.ContentTypes` is empty), and it catches its own errors. The run should therefore end as Succeeded.
   Confirm that the Last run badge shows a check mark and no error line.
8. **Redis Lock.** `schedule-5` names it as an example of a distributed lock feature, as the documentation does. The feature is called
   "Redis Lock" and has the id `OrchardCore.Redis.Lock` in the Redis module's manifest (`OrchardCore.Modules/OrchardCore.Redis/Manifest.cs:34-35`).
   The video does not enable it.
9. **Pronunciation.** The voice has to read "cron" and "Redis" correctly. Check both with `pronounce.py` before narrating; add a `spoken`
   entry to the storyboard only after a person confirms the spelling.
10. **A toast stays while the pointer is on it.** A success message is a Bootstrap toast that hides itself 5 seconds after the page has
    loaded (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `OrchardCore.Themes/TheAdmin/Views/Message.cshtml:19-20`).
    The plan keeps it on screen by resting the cursor on it, which relies on Bootstrap 5.3.8 pausing the hiding while the pointer is over
    a toast. Bootstrap is not in the checkout. Confirm in the first take that the toast of `enable-1`, `schedule-3`, `run-1`, `run-4`,
    `toggle-1` and `toggle-2` is still on screen when the narration ends. If it is not, click later in the step, so that the page loads
    less than 4 seconds before the narration ends.
