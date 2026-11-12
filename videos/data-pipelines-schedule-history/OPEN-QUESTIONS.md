# Open questions: Data Pipelines: Schedule Runs and Read the History

What was checked in the source, and what a person must confirm on a running site before the video is recorded. The
narration states only what the source and the documentation support; the points below are the places where the
recorded screen has to be compared with it.

## Statements that rest on a schedule the recording cannot show

1. **`sched-3` and `sched-6`** say that the schedule entered (`0 2 * * *`) means two in the morning, every day, and that
   "the workflow queues a run every night". The source shows the field, its default (`*/5 * * * *`), the hint that the
   expression uses UTC unless **Use site time zone** is checked (`Views/Items/TimerEvent.Fields.Edit.cshtml`), and the
   once-a-minute "Timed Workflow Starter" task (`OrchardCore.Workflows/Timers/TimerBackgroundTask.cs`). The expression's
   meaning is the usual five-field cron one (`TimerEvent.cs` parses it with NCrontab), but no document of the repository
   spells it out. How the timer works, from the code: on the first minute after the workflow is published, the task
   starts an instance that waits on the Timer Event, and that instance continues with the Run Data Pipeline Task at the
   next time the expression names after it started waiting (`Timers/TimerEvent.cs`, `IsExpiredAsync`;
   `Services/WorkflowManager.cs`, `TriggerEventAsync`). So the first run comes at the next 02:00, not at once. Confirm on a
   site: publish a workflow with an expression for a minute a few minutes ahead, with and without the box, and see the
   run appear in **Run History** at that time in the right time zone.
2. **`hist-2`** says a run queued by a workflow reads **Workflow** with no user name. The code sets the trigger to
   `Workflow` and leaves `TriggeredBy` empty for a task (`Workflows/RunDataPipelineTask.cs`, `Views/Admin/Runs.cshtml:103`).
   Confirm the badge on the seeded runs.

## Recorder behavior

3. **`run-3`**: the page of a run refreshes itself every 3 seconds until the run ends (`Views/Admin/Run.cshtml:18`). The
   plan waits off camera for **Succeeded** and navigates again. Confirm that the capture continues after that navigation,
   and that the contact sheet shows a settled page.
4. **`sched-2` to `sched-5`**: the settings panel of an activity applies a change by itself 600 ms after a field changed
   and has an **Apply** button, which reads **Saved!** after it applied (the compiled designer,
   `wwwroot/Scripts/Workflows/designer/workflows-designer.js`, component `ServerFormHost`). The plan types the value,
   presses `Tab` and waits for **Saved!**. Confirm on screen that **Saved!** appears after each change and that the value
   is still there after the next click; if not, the plan needs a click on **Apply** after each field, and the narration of
   those steps stays valid.
5. **`sched-6`**: the **Run Data Pipeline Task** has the outcomes **Succeeded**, **Failed** and **Skipped**, none of them
   connected. Confirm whether **Publish** opens the **Publish the workflow?** dialog with warnings; the plan clicks
   **Publish anyway** if it does. The narration says only that Publish turns the draft into the published workflow.
   Confirm the message after publishing: the designer shows "The workflow was published as version 1." when the result
   carries a version, and "The workflow was published." otherwise.
6. **`sched-4`**: an outcome that leads nowhere shows its plus only while the pointer is on it (the stylesheet of the
   designer), so the narration says that pointing at it shows a plus and the plan hovers before it clicks. Confirm that a
   click without a drag opens the list titled "Add the next activity, after Done", and that the list holds **Run Data
   Pipeline Task** once the Workflows feature and Data Pipelines are enabled.
7. **`sched-2`, `sched-4`**: the title of a card on the canvas is the name of the activity as the library lists it (**Timer
   Event**, **Run Data Pipeline Task**), but the body of the card comes from the activity's design view, which has its own
   heading (**Timer**, **Run Data Pipeline**). Confirm what the card shows, and that the narration's names match the
   title and the list.

## Text on the run pages

8. **`hist-4`, `hist-5`**: the failed run's error is written from the source (`ReadMediaFileStep.cs`: "The media library
   has no file '{0}'."); the narration does not repeat it. The file is checked before the run starts a step, so the run
   fails with that error while all three steps keep their first status, **Pending**, with no error under them, and the
   **Log** holds the message twice, red, after the name of the step (`DataPipelineExecutor.cs`,
   `DataPipelineRunExecutor.cs:94-96,223-236`). The narration of `hist-5` says that the steps are **Pending** and that the
   **Log** repeats the error in red. Confirm the exact text of the **Error** row, the **Pending** badges, the two **Log**
   entries and their red color. If the screen shows a **Failed** step instead, change `hist-5` and the plan.
9. **`run-2`**, **`hist-3`**: the menu item of a pipeline reads **Run history** (lower case h) and the menu of the admin
   reads **Run History**; confirm both on screen. The narration says "Run history" for the Actions menu item and "Run
   History" for the page, as the source spells them (`Views/Admin/Index.cshtml:94`, `DataPipelines/AdminMenu.cs:25`).
10. **`keep-1`**: the narration says that **Cancel the run**, in the **Actions** menu of a run, stops a queued or a
    running run (`Views/Admin/Runs.cshtml:119-123`). It is shown on a slide only, since cancelling needs a run that has
    not ended. A person can confirm it by starting a run and opening the menu of its row.

## Pronunciation (the voice, to check with `scripts/pronounce.py` before narrating)

11. "CRON" (in "CRON expression" and the label **CRON Expression**): read as a word or letter by letter? No `spoken`
    entry was added. If the voice spells it, add one to the storyboard's "spoken" map.
12. "UTC", "Run data pipelines", "Blog posts export", "Subscriber list check".
