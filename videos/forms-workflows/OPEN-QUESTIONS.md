# Open questions: Forms and Workflows: Process Each Submission

Everything in the narration was read from the source of the Forms, Workflows and Email modules (and their designer
sources) of the commit the catalog was made from. No site was run to write this video. These are the points that a
person must confirm on a running site before recording, or while recording, and what was checked in the source.

## Before the first take

1. **The URL of the workflow.** The Http Request Event fills its `URL` box through the `Regenerate` script, and
   the box holds `Url.Action("Invoke", "HttpWorkflow", new { token })`
   (`OrchardCore.Workflows/Http/Controllers/HttpWorkflowController.cs`). `Http/Startup.cs` maps two routes to that
   action (`workflows/{action}` and `workflows/invoke/{token}`), so the shape of the address depends on which one
   the link generator picks. The narration never describes it. Confirm that the value pasted into `Action` is a path
   (without scheme, host and port), and that a post to it starts the workflow (the instance appears in the list).
2. **The admin menu.** `Design`, `Workflows` and `Content`, `Content Items` come from the menu builders, but the
   behaviour of the menu (a click or a hover opens a group, collapsed rails) was not checked. Adjust the clicks of
   `start-1` and `connect-1`, and say so in the recorder's code.
3. **The designer's selectors.** The `data-cy` and `data-outcome` attributes in `recording-plan.md` come from the
   Vue sources (`Assets/designer/src/canvas/OutcomePort.vue`, `QuickAddMenu.vue`, `StartPicker.vue`,
   `App.vue`). Confirm them in the built page, and confirm that the control that fits the canvas to its content
   exists and what it is labeled (the localizer has a `FitToContent` string). Two components of the designer come from
   a shared package that is not in the source of this commit: the form host of the activity panel
   (`@bloom/designer/panel/ServerFormHost.vue`) and the save indicator of the toolbar
   (`@bloom/designer/draft/SaveStatusIndicator.vue`). What the source shows is that closing the panel, selecting
   another activity and `Publish` apply the pending values (`ActivityPanel.vue`, `close` and `settle`; `App.vue`,
   `publish`), and that the toolbar strings are `All changes saved`, `Saving…` and `Unsaved changes`
   (`WorkflowsDesignerJSLocalizer.cs:166-168`). Whether a value is also applied when its field loses focus, and that
   the indicator shows those strings, must be confirmed in the built page; the plan presses Tab after every value
   and waits for `All changes saved` before a snapshot.
4. **The activity panel over the ports.** The panel opens over the bottom of the canvas when an activity with
   settings is added. Whether it hides the next port depends on the window height; the plan closes it before every
   port click. If the ports of the last activities are out of view, pin the panel, or zoom out, rather than
   changing the story.
5. **The SMTP provider.** `Settings`, `Communication`, `Email` is built from `OrchardCore.Email/AdminMenu.cs`; the
   fields come from `SmtpSettings.Edit.cshtml`, but how the page presents the providers (tabs, a `Default provider`
   list, the label of the save button) was not read. The ports of the Docker stand-in (`3000` for its web page,
   `2525` for SMTP) are the usual ones for smtp4dev and were not checked either. Any SMTP catcher on any port does.
6. **Default values of the widgets.** A Form widget saved without touching `Method` should post `POST` (the list
   shows `POST` first), and an `Input` widget's `Validation option` should start at `None`; the plan sets both
   explicitly. Confirm that the validation summary is the only place the errors show on screen in `try-1`.

## While recording

7. **The Email Task's `HTML Body` editor.** The editor is built by `Assets/ts/email-task.ts` (a Liquid code editor
   bound to the text area). Typing `{{` may close the braces by itself. When it does, set the text through the
   editor's API with an evaluation in the page, or type the body without braces and say nothing about it. The
   narration only needs the `Subject` to hold a Liquid expression.
8. **The timing of `start-1`.** The narration has 25 words (about 10 seconds) for five navigations and a typed
   name. Shorten the pauses, or move the typing before the capture and show only the click on `Create and open the
   designer`; do not lengthen the narration without validating it again.
9. **The first match of a quick-add search.** The list shows tasks first, sorted by name. `validate form task`
   and `http redirect` each also match a second activity, so the plan clicks the result by its `data-cy`.
10. **The order of the instances and their journal.** The default order of the list of instances is newest first
    (`WorkflowOrder.CreatedDesc`), and the journal of an instance lists the activities by their title. Confirm that
    the instance opened in `try-3` is the valid post and shows `Completed` records. The workflow panel of the
    instance page can be collapsed to a rail of its tabs, which the browser remembers (`WorkflowPanel.vue`,
    `COLLAPSED_KEY`); a fresh browser profile shows it open. Confirm that the `Journal` tab is reachable without
    opening the panel first.
11. **Temp data across the redirect.** The error carry-over in `try-1` relies on the temp data of the site (cookie or
    session based, per the site's configuration). The filters are registered for every controller
    (`OrchardCore.Forms/Startup.cs`), and the page that displays the item returns a view, so the summary should
    show both errors, and the message should stay in its box. Confirm it on the front end. The Blog recipe enables
    Dynamic Cache; nothing read in the Forms and Contents sources caches the shape of a page, so the page after the
    redirect should be built for each request, but only a take on the running site shows it.
    The filter exports when `ModelState.IsValid` is false (`ExportModelStateAttribute.cs`). Bind Model State Task adds
    its entries with `SetModelValue`, and ASP.NET Core treats such entries as not validated, which is why Validate Form
    Task tests `ErrorCount == 0`. If `IsValid` is false for them, the valid post also exports its model state when
    Http Redirect Task sets the 302, and the Thank You page merges it. That page has no validation summary and no
    input, so nothing should show. The narration states the filter's own condition (the model state isn't valid and
    the action redirects) and does not claim that only errors trigger it; confirm in the take of `try-2` that the
    Thank You page shows no error text.

## Pronunciation

12. **Names the voice may misread.** `spoken` is empty. Check with `scripts/pronounce.py` how the voice reads
    `Http Request Event`, `Http Redirect Task` and `Http Redirect To Form Location Task` (the exact labels of the
    designer), `HTTP`, `Liquid`, `antiforgery` and `POST`, and add a `spoken` entry if one is wrong. A person must
    confirm a new spelling before it is used.
