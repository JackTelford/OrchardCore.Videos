# Open questions: Write a Background Task

What was checked in the Orchard Core source (commit `92db4a83`), and what a person must confirm on a running site before
or while recording. Nothing here was run: the site, the recorder and the voice were not used.

## To confirm on a running site (the recording)

1. **The task list.** A site made with the Blog recipe, with Background Tasks enabled, should list three tasks:
   Abandoned File Uploads Cleaner, Remote Media Cache Cleaner and Resized Media Cache Cleaner. Checked in
   `OrchardCore.Media/Startup.cs:77-78` and `:229` (`AddChunkFileUploadServices`) and in `blog.recipe.json`, which enables
   `OrchardCore.Media`. Every other registration of an `IBackgroundTask` in the source (Content Version Pruning, Indexing
   Worker, Publish Later, Archive Later, Audit Trail, Sitemaps, Workflows, Data Pipelines, OpenID, Demo) sits in a feature
   that the recipe does not enable and that none of its features lists as a dependency. Confirm the list on a running site.
2. **Time until the task is listed.** `recording-plan.md` waits up to 3 minutes after enabling the feature until the row of
   the task has a **Next run** badge. This follows from `ModularBackgroundService.cs` (a tenant that was released is
   updated on the next round, and the polling time is one minute), but the real delay was not measured. Confirm that
   3 minutes is enough, or give the recorder a longer wait.
3. **Run now on a fresh site.** `ResizedMediaCacheBackgroundTask` catches every exception of its cleanup, so its run is
   expected to end as **Succeeded** whatever the cache holds (it also returns at once when `MediaOptions.ResizedCacheMaxStale`
   has no value, which is not shown on the slide). Confirm that **Last run** shows **Succeeded** within
   20 seconds of the click, and that **Last run duration** is short (the text is `{0} ms` under one second).
4. **Selectors.** They were read from the views and not run: `a[href$='/Admin/BackgroundTasks']` for the menu item,
   `[data-task-name=...]` on the row (`BackgroundTaskEntryDisplayDriver.cs`, set on the row shape in every layout),
   `.admin-list-secondary` for the description, `[data-next-run]`, `button[data-task-actions]`, `a[data-run-task]`,
   `dd[data-last-run] [data-run-result='Succeeded']` on the details page, and the Enable link of the feature
   (`a[href*='OrchardCore.BackgroundTasks/Enable']`). In `TheAdmin`, an item with children and no address is rendered as a
   `<button type="button">` that expands them (`NavigationItemLink-admin.cshtml`), so **Tools** is a button and **Background
   Tasks** a link. To confirm on the site: that exactly one `a[href$='/Admin/BackgroundTasks']` exists on the page, so the
   highlight does not meet two elements. The links **Enable** and **Run now** post through the script of `TheAdmin`
   (`data-url-af="UnsafeUrl"`, `TheAdmin.ts`), which sends the antiforgery token and shows no confirmation dialog for them.
5. **The default layout of the list.** The video relies on the default admin list layout (List, with Buttons): see
   `AdminListOptions.cs` (`DefaultLayout`, `DefaultActionsLayout`). The recording uses the details page for the
   run request, so the layout of the row actions does not matter, but the badges **Last run never** and **Next run** are
   labeled in the List layout only (the Grid layout puts them in columns and drops the labels).

## To confirm before narrating

6. **Pronunciation.** The `spoken` map of `storyboard.json` splits identifiers into words ("I Background Task Monitor",
   "Resized Media Cache Background Task", "Do Work Async", "Request Run Async", "Already Queued", "Orchard Core" for
   `OrchardCore`). These are plain word splits, but check them with `scripts/pronounce.py`, together with "cron" and
   "URLs", which the voice may read as letters. `narrate.py` replaces the map's keys in the order of the map, so the
   longer identifiers come first.

## Claims in the narration that come from reading code, not from running it

7. **The schedule is read in the time zone of the site** (`attr-2`). `ModularBackgroundService.UpdateAsync` takes the time
   zone from the site settings (`Clock.GetTimeZone`, which falls back to the time zone of the server when none is set), and
   `BackgroundTaskScheduler.CanRun` converts both times to it before evaluating the cron expression. To confirm: save a
   schedule for the next minute in the time zone of the site on a task, and watch it run.
8. **The default schedule** (`attr-2`). It is every five minutes in the attribute (`BackgroundTaskAttribute.Schedule`,
   `*/5 * * * *`) and every minute for a task without the attribute (`BackgroundTaskSettings.Schedule`, `* * * * *`, used by
   `GetDefaultSettings`). Both are in the narration. To confirm: a task without the attribute is listed with the schedule
   `* * * * *` on its details page.
9. **Tasks and the lifetime of their services** (`write-2`). A task registered with `AddSingleton` is a singleton of the
   container of the tenant (it is resolved from `scope.ServiceProvider` in `ModularBackgroundService.RunAsync`), so the
   narration says "at least as long as the tenant". Whether a task registered as scoped also works was not checked, and is
   not claimed.
10. **A tenant's tasks run only once it has handled a request** (`cfg-1`). `ModularBackgroundService.GetRunningShells`
    keeps a tenant only when `ShellWarmup` is on or the tenant has a pipeline (`HasPipeline()`), as the documentation says
    ("once this tenant has handled a request on this server"). Not run: it is why the preparation of the recording
    reloads the page of the task list after enabling the feature.
11. **A handler of `IBackgroundTaskEventHandler` applies to the tenants where it is registered** (`hook-1`). The handlers
    are resolved from the scope of the tenant that runs the task (`ModularBackgroundService.cs:217`), so the narration says
    "every task of a tenant". An exception of a handler is logged and skipped (`InvokeExtensions`); it was not run.
12. **A lock that cannot be acquired** (`farm-2`). The run is not started and `ReferenceTime` is unchanged, so the task is
    still due on the next round; the narration says the service tries again on the next round. Not run, as it needs a
    distributed lock held by another process.

## Left out of the narration on purpose

- **What a failed run does to the changes of the session.** `ModularBackgroundService` calls `scope.HandleExceptionAsync`,
  which cancels the pending changes of the YesSql session (`OrchardCoreBuilderExtensions.cs`, `AddExceptionHandler`), and the
  session is committed when the scope ends otherwise. The behavior of `ISession.CancelAsync` is in YesSql, which is not in the
  source that was read, so the video says only that the run is recorded as failed.
- **A lock that cannot be acquired skips the rest of the round for the tenant.** In `ModularBackgroundService.RunAsync`
  (line 158), the `break` after a lock timeout leaves the loop over the tasks of the tenant, so the tasks that follow wait for
  the next round too. The documentation says only that the task tries again on its next polling. The narration does not
  say it, because a person has not seen it happen.
- **A run is `Canceled`, not `Failed`, when the application is stopping.** `ModularBackgroundService.cs:243-246` records
  `Canceled` for an `OperationCanceledException` raised while the application stops. The narration says "failed" for an
  exception thrown by `DoWorkAsync`, which is what happens in every other case.

## Code on the slides that is not from the source

- `TaskFailureHandler` (`hook-2`) is written for the video: Orchard Core has no implementation of
  `IBackgroundTaskEventHandler`, only the interface and the call in `ModularBackgroundService`. It was not compiled. Compile it
  in a module that references the `OrchardCore.Abstractions` project (it needs `using OrchardCore.BackgroundTasks;`) before
  recording the slide, or replace it.
- `ResizedMediaCacheBackgroundTask` (`write-1` to `write-3`) is trimmed: the attribute is left out (the next chapter shows it)
  and `// ...` stands for the early `return` when `MediaOptions.ResizedCacheMaxStale` has no value. The field `_logger` is
  shown. The panel has 24 lines and ends lower than the other panels (`y = 1062`), because 24 lines at size 22 do not fit
  above 1030.
- The configuration panel (`cfg-1`) adds `"ShellWarmup": false` to the example of the documentation: that is the default of
  `BackgroundServiceOptions.ShellWarmup`.
