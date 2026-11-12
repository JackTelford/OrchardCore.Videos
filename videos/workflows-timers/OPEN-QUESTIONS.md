# Open questions: workflows-timers

What was checked in the source, and what a person must confirm on a running site while recording. The narration
states only what the source and the documentation show; the points below are left out of the narration or are
conditions of the recording.

## To confirm while recording

1. **The designer's activity forms.** The editor of an activity is loaded by `ServerFormHost`, a component of a shared
   package (`@bloom/designer`) that is not in the source used for this video. The documentation says the changes of an
   editor "are applied when you leave a field". Confirm that typing in **CRON Expression** and pressing Tab applies the
   value (the card on the canvas shows it), what shows the feedback (`AppliedFeedback`, **Saved!**,
   `Services/WorkflowsDesignerJSLocalizer.cs:134`) and whether the toolbar shows **All changes saved**
   (`WorkflowsDesignerJSLocalizer.cs:166`). If an **Apply** button (`WorkflowsDesignerJSLocalizer.cs`, key `Apply`) is
   needed, click it and add it to the steps `create-4`, `create-5` and `create-7`.
2. **The admin menu.** The menu entries are **Tools** / **Features** (`OrchardCore.Features/AdminMenu.cs:26-27`) and
   **Design** / **Workflows** (`OrchardCore.Workflows/AdminMenu.cs:18-19`), but the markup of the admin theme's menu is
   not read here beyond the markup of `TheAdmin` (`Views/NavigationItem-admin.cshtml`: a first-level entry with children
   and no page of its own is a button that opens its children). Confirm that a click on **Design** and then **Workflows**
   works as the plan describes.
3. **The search on the Features page.** The filter runs in the page (`features-index`) on the name, the id and the
   description of each feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), and no other manifest in
   the source mentions a timer, so `Timer` should leave only the row **Timer Workflows Activities**. Confirm it, and that
   the button label is **Enable** (the Buttons layout is the default, `OrchardCore.Admin.Abstractions/AdminListOptions.cs:36`).
4. **The buttons of the start picker.** `StartPicker.vue` lists up to six events: its suggested list has the Timer Event
   in fourth place, after the two content events and the HTTP request event, so it is among the six whatever other
   events are enabled (`StartPicker.vue:13-21`). Confirm that the button **Timer Event** is on screen once the
   feature is enabled.
5. **When the first instance and the first run appear.** From the code: the **Timed Workflow Starter** background task
   (`Timers/TimerBackgroundTask.cs`, schedule every minute) first resumes the instances that wait on the timer, and then
   starts a new instance for the workflows that start with a timer (`Services/WorkflowManager.cs`, `TriggerEventAsync`).
   The scope recursion count of the workflow type is back to zero when the first part ends (`ExecuteWorkflowAsync`, the
   `finally` block), and the exclusive check reads the instances by query, which YesSql is expected to flush before it runs (YesSql is outside this source); so the
   next instance should start in the same call. Confirm on the site that **Halted 1** shows within about a minute of
   publishing, that **Finished** grows by one for each minute, and that **Halted** never drops to 0 (otherwise wait for
   it before `run-1`).
6. **The status labels of the list.** `Halted` and `Finished` come from `GetLocalizedStatus`
   (`OrchardCore.Workflows.Abstractions/Helpers/ActivityExtensions.cs:48`); confirm the badge text on screen.
7. **The page of an instance.** The documentation says the activities and connections the instance ran are
   highlighted. From the code, `.wfd-node.is-executed` is set for every activity with a journal record, and the journal
   is on by default (`WorkflowJournalOptions.Enabled`). Confirm that both the Timer Event and the Log Task are highlighted
   for a Finished instance, and that the Timer Event card shows the badge **×2** (two records: waiting, then continuing).
   If the badge is not there, `run-3` is still correct.
8. **The version number of the first publish.** Creating a workflow saves its empty definition as version 1
    (`Services/WorkflowTypeStore.cs` `SaveAsync`, `WorkflowTypeVersionStore.cs:90-129`), so the first publish should give
    **version 2**, whereas the documentation numbers the versions from the first publish. The plan waits for the toast
    without a number; confirm the number on screen. The narration never says it.
9. **The five fields of a CRON expression.** The order (minute, hour, day of the month, month, day of the week) is the
    standard one of the NCrontab parser, which is outside the source used here. The source agrees with it: the default
    `*/5 * * * *`, the schedule `* * * * *` of the background task, and the Background Tasks documentation, which calls
    `0 0 31 2 *` "February 31" (`BackgroundTasks/README.md:19`). Confirm with the NCrontab documentation if in doubt.
10. **Pronunciation.** Two spellings are used in the storyboard's `spoken` map: `CRON` as `cron`, and `UTC` as `U.T.C.`.
   Check them with `scripts/pronounce.py` before narrating, as the skill asks.
11. **Admin menu or route of Background Tasks.** The video names the background task but does not open its page.
   If a later take shows it, the path is **Tools** / **Background Tasks** (`OrchardCore.BackgroundTasks/AdminMenu.cs`).

## Verified in the source, but not in the narration

- **The expression is not checked when saved.** The editor of the Timer Event requires a value
  (`Timers/TimerEventViewModel.cs:7`) and nothing else. Reading `Timers/TimerEvent.cs`, an expression that the CRON
  parser (NCrontab, outside this source) rejects fails when the timer is evaluated. What the site then shows (a
  faulted instance, an error of the background task, other timers affected) must be seen on a site before a video says
  anything about it. The Background Tasks editor does reject an invalid schedule; the Timer Event editor does not.
- **Disabling.** `WorkflowTypeStore.GetByStartActivityAsync` keeps only enabled workflows, so a disabled workflow starts
  no new instance (`Services/WorkflowTypeStore.cs:72-79`). The code does not check the setting for an instance that is
  already waiting on the timer (`WorkflowStore.ListByActivityNameAsync`, `WorkflowManager.ResumeWorkflowAsync`), so that
  instance may still run once after the workflow is disabled. The **Enabled** change applies at once, without
  publishing (`Services/WorkflowTypeDraftManager.cs`, `UpdateSettingsAsync`), although the message `RunDisabled`
  (`WorkflowsDesignerJSLocalizer.cs:292`) says "Enable it on the Workflow tab, and publish". The video says nothing
  about stopping a timer until the behaviour of the waiting instance is confirmed on a site.
- **Changing the schedule later.** A waiting instance keeps the expression it started with: the activity state, with the
  time the wait began, is saved with the instance (`WorkflowManager.cs` `PersistAsync`), and the instance resumes on the
  version it started on. Not in the narration.
- **Execution settings need a publish.** **Delete successfully completed workflows** is copied to the workflow only when
  the draft is published (`Helpers/WorkflowTypeDraftExtensions.cs` `ApplyTo`), and an instance finishes with the setting
  of the version it started on (`WorkflowManager.cs` `CompleteRunAsync`, `ResumeWorkflowAsync`). The narration of
  `know-3` says to publish. The instance that already waits when the setting is published keeps the old value, so it
  is not deleted when it finishes; the instances that start afterwards are.
- **Time zone.** With **Use site time zone**, the start time and the current time are converted to the time zone of the
  site settings before the expression is evaluated (`Timers/TimerEvent.cs:96-122`). The video does not show the setting
  of the time zone; the recorded site uses `America/Los_Angeles`.
- **Restarts.** "By default, after a restart they resume once the site has served its first request" follows
  `ModularBackgroundService.cs` (a tenant without a request pipeline is skipped unless `ShellWarmup` is on) and the
  Background Tasks documentation. It was not tried on a site.
- **Day of the week.** The fifth field is the day of the week; its numbering is not in the source or the
  documentation, so no slide or narration uses it.
- **Run… and timers.** The **Run…** button of the designer does not run a workflow that starts with a Timer Event
  (`WorkflowsDesignerJSLocalizer.cs`, key `RunUnavailable`), so the video does not use it.
