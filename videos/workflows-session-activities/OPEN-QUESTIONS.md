# Open questions: Workflows and the Database Session

What could not be verified from the source and the documentation alone. The narration says only what the source of
`OrchardCMS/OrchardCore` at commit `92db4a83` shows, but each point below was read, not run. A person who records the
video confirms them on a running site before the clips are used. Paths are in `src/OrchardCore.Modules` unless noted.

## To confirm while recording

1. **The order of the branches of a Fork.** `OrchardCore.Workflows/Services/WorkflowManager.cs:589` and `:807`: the
   outcomes of an activity are visited in order and each next activity is pushed on a stack, which pops the last pushed
   first. For the Fork `Billing, Shipping` the Shipping branch should therefore run first. The narration says only
   that "the branches run one after the other". Expected journal of the "Order handoff" run: #3 Prepare shipping before
   #5 Prepare billing (see the table of clip `03-run` in recording-plan.md). If the journal shows the other order, only
   the table needs fixing.
2. **The Join appears twice in the journal.** `OrchardCore.Workflows/Activities/JoinTask.cs`: in `WaitAll` mode the
   Join returns no outcome (`Noop()`) when the first branch arrives, and `Joined` when the last one does, and
   `WorkflowManager.cs:710` records each execution. Step `run-2` says "The Join appears twice: it waits for the
   first branch, and continues after the second." Confirm the journal has two Join records (#4 without outcomes, #6
   with `Joined`) and that the canvas shows `×2` on the Join card.
3. **The Run request commits and follows `Valid`.** Step `run-3` says Commit Transaction completes with `Done` and
   `Valid`. `WorkflowDesignerController` (`Controllers/WorkflowDesignerController.cs:31`) is an MVC `Controller`, so
   `ModelBinderAccessorFilter` gives the task that controller's model state (`OrchardCore/OrchardCore.DisplayManagement/ModelBinding/ModelBinderAccessorFilter.cs:20`),
   which nothing in the run adds an error to. Confirm record #7 shows `Done, Valid`.
4. **The seed import.** The "Order handoff" JSON of recording-plan.md follows `Recipes/workflows-variables-samples.recipe.json`
   and `Recipes/WorkflowTypeStep.cs`, but no sample in the source holds a Fork or a Join, so their properties were written
   from the activity classes (`Forks` is a list of names; the Join's `Mode` and the Log's `LogLevel` are left out to use
   their defaults, since the source has no example of how those enumerations are written). Confirm that the import
   succeeds, that the Issues tab shows no issue, that the Fork's editor shows `Billing, Shipping` and the Join's **Mode**
   shows `WaitAll`, and that no two cards overlap.
5. **The `Enable` link on the Features page.** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:51`
   is a link with `data-url-af="UnsafeUrl"` to a POST action (`Controllers/AdminController.cs:226`). Confirm that
   clicking it enables the feature without a confirmation dialog, that the notification reads "The feature Session
   Workflows Activities was enabled.", and that its id is as the plan says.
6. **The YesSql semantics.** Steps `session-2`, `session-3` and `commit-2` rely on `ISession.SaveAsync` recording a
   change without committing it and on `SaveChangesAsync` committing what the session holds. YesSql is a package, not
   part of the Orchard Core source that was read; what the source shows is that the scope commits the session at its
   end (`OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:160-181`, which commits only the session of the shell scope and `Documents/DocumentStore.cs:124`),
   that the workflow store calls `SaveAsync` (`Services/WorkflowStore.cs:134`) and that the task calls `SaveChangesAsync`
   (`Activities/CommitTransactionTask.cs:47`). The Orchard Core page `docs/topics/data/yessql.md` ("The session") says
   that `SaveAsync` and `Delete` are buffered in memory, that the SQL commands run in a single transaction when
   `SaveChangesAsync` is called, and that the session of a shell scope is committed when the scope ends; that supports
   these steps. Still to confirm, with the YesSql log at debug level on the "Order handoff" run: that the commit of
   the task is a transaction of its own, and that the commit of the scope after it is harmless. `WorkflowStore.SaveAsync`
   also calls `FlushAsync` (`Services/WorkflowStore.cs:148`), which the slide hides under `// ...`: confirm that it
   writes inside the open transaction without committing.
7. **Designer details of the recording.** The labels of the canvas controls (**Fit to content**, **Hide the
   activities**, **Collapse the panel**) come from `Services/WorkflowsDesignerJSLocalizer.cs`; the controls are drawn by
   the shared canvas package, whose source is not in the tree. Confirm the buttons and their titles with
   `Recorder probe`, and that opening an editor without a change leaves no draft. The workflow panel and the
   activities pane keep their collapsed state in the browser (`panel-collapsed`, `toolbox-collapsed`), and the instance
   page reads the same keys: on that page the Journal tab is reached through the rail button until the panel is expanded.
8. **The effect of the commit in the sample.** The Log tasks of "Order handoff" save nothing, and the instance and its
   journal records are saved after the run's loop (`WorkflowManager.cs:575`, `PersistAsync`), so the early commit of
   the sample has nothing visible to commit. The narration says the Log tasks stand in for real work and claims no
   visible effect. Confirm that this is acceptable for the recording.
9. **WaitAny with branches that don't wait.** `JoinTask.cs` never clears its `Branches`, so in `WaitAny` mode a later
   branch that arrives also satisfies `Any(...)`, and the Join returns `Joined` again. Step `fork-3` says only that
   `WaitAny` continues as soon as any inbound branch has arrived. Confirm the repeated continuation on a copy of the
   sample set to `WaitAny` if the narration is ever extended.
10. **Where the Invalid outcome comes from.** The Forms module's validation tasks (`OrchardCore.Forms/Workflows/Activities/AddModelValidationErrorTask.cs:55`,
   `ValidateFormFieldTask.cs:66`) add errors to the same `IUpdateModelAccessor` model state, which is a way to reach
   `Invalid`. The narration does not name them; the editor behaviour of step `where-2` is
   `OrchardCore.Contents/Controllers/AdminController.cs:789-791` and `:835-837`.

## To confirm with the voice

11. **Pronunciation.** The storyboard's `spoken` map has `ISession` ("I Session"), `SaveAsync` ("Save Async"),
   `SaveChangesAsync` ("Save Changes Async"), `WaitAll` ("Wait All") and `WaitAny` ("Wait Any"). None is confirmed with
   `scripts/pronounce.py` yet. Also listen to "Run…" (said as "Run"), "Started By Workflow Event" and "Orchard Core".
