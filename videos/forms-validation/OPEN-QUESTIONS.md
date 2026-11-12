# Open questions: Forms: Validate What Visitors Type

What was read from the source (Orchard Core at commit 92db4a83) and what a person must confirm on a running site
before recording. The narration states only what the source shows; the points below are what it cannot show.

## To confirm on a running site, before recording

1. **The whole round trip.** The narration says that after a failed check the visitor comes back to the form with the
   error messages (and, with Bind Form Model State, the typed values). Read in the source: `HTTP Request` stores the
   form's location (`OrchardCore.Workflows/Http/Activities/HttpRequestEvent.cs:76-93`), `HTTP Redirect To Form
   Location` sets it as the redirect (`OrchardCore.Forms/Activities/HttpRedirectToFormLocationTask.cs:45-61`),
   `HttpWorkflowController` returns the redirect (`OrchardCore.Workflows/Http/Controllers/HttpWorkflowController.cs:279`,
   `GetWorkflowActionResult`), and the filters of the Forms module copy the model state into `TempData` on the
   redirect and merge it back when the page is displayed (`OrchardCore.Forms/Filters/ExportModelStateAttribute.cs`,
   `ImportModelStateAttribute.cs`). Nothing in the source shows it running: do the three submissions of `04-try` once
   before recording, and check that (a) the summary and the three messages appear, (b) the Name value survives,
   (c) a valid submission ends on `/thank-you`.
2. **Caching and leftovers.** The Blog recipe enables Dynamic Cache. Check that the form page shown after the redirect
   is not served from a cache that predates the errors, and that the errors do not show on a later visit to the page
   (they are read once from `TempData`). After the valid submission of `try-4`, open `/contact-form` again: the form
   must be empty and show no message. Bind Form Model State fills the model state with unvalidated entries, so the
   Forms filter may also copy it to `TempData` on the redirect to `/thank-you` even when no error exists
   (`ExportModelStateAttribute.cs:13` tests `ModelState.IsValid`); the Thank You page then reads and drops it. Confirm
   that nothing of the form shows on the Thank You page or on the next visit of the form.
3. **Appearance of the errors.** The Input and Text Area views add the classes `input-validation-error is-invalid` to
   a field that has an error (`OrchardCore.Forms/Views/Items/InputPart.cshtml:35`,
   `TextAreaPart.cshtml:20`), and the messages are rendered by `Html.ValidationMessage`. Whether TheBlogTheme
   styles those classes and the message text visibly (a red outline, red text) is not in the files read. The narration
   says only that the messages appear; it never names a color. If the theme shows nothing visible for the messages,
   `try-2` has nothing to ring: report it instead of recording around it.
4. **A leftover empty Key.** The Add Form Validation Error activity passes its **Key** to the model state as it is
   (`OrchardCore.Forms/Workflows/Activities/AddModelValidationErrorTask.cs:55`), and the editor posts an empty field as no value. The
   source does not show whether an empty Key is accepted, nor how the summary's **Show Model Only Error** option
   treats such an error. The narration does not use an empty Key, and says only that the option stays unchecked so
   that the summary lists every error (the hint on screen: "When checked, model level errors will be visible
   otherwise all errors will be visible.").
5. **Browser validation of the Email input.** The Input widget renders its **Type** as the type of the HTML input.
   A browser may check the format of an `email` input before it sends the form, and block the submission. The
   recording types only a valid address or nothing in that field. Confirm that an empty Email field is sent.
6. **The designer.** The selectors of `recording-plan.md` come from the Vue sources of the designer and from its built
   bundle (`OrchardCore.Workflows/wwwroot/Scripts/Workflows/designer/workflows-designer.js`, which holds the shared
   parts: `[data-cy="zoom-fit"]` for **Fit to content**, `[data-cy="panel-apply"]` for **Apply**). Check with
   `Recorder probe`: the quick-add menu, the closing button of the activity panel, the layout of eight cards at the
   zoom that **Fit to content** gives (the cards must stay readable; if they do not, record `rules-9` with the canvas
   zoomed in on the part the narration needs), and that the panel applies a field about 0.6 s after it loses focus
   (`autoApplyDelay` 600 in the bundle). The label **Fit to content** is the title and aria-label of an icon-only
   button: the viewer sees the icon, and a native tooltip may not appear in the recording. If the narration of
   `rules-9` then points at nothing readable, hover the button for a second before the click, or report it.
7. **Labels of the same activity differ between surfaces.** Bind Form Model State is "Bind Form to Model State" in the
   activities pane, "Bind Model State Task" in the quick-add list and the card header, and "Bind Form Model State" in
   the card body. The narration uses the card body's title, which is on screen when it is named. The same holds for
   the HTTP Request event: "Http Request Event" on the card header and in the start picker, "HTTP Request" in the
   activities pane, and "Handle Incoming HTTP Request" as the title in the card body
   (`HttpRequestEvent.Fields.Design.cshtml:4`). The narration says "HTTP Request event", the name of the activity in
   the activities pane; no card carries exactly that text, so confirm that the wording reads naturally against the
   card on screen in `rules-1`.
8. **Pronunciation.** The storyboard has no `spoken` entry. Check with `scripts/pronounce.py` how the voice reads
   "HTTP", "URL", "POST", "Antiforgery" (not narrated) and "thank-you" before narrating.

## Read in the source (no question)

- Validate Form Field reports an error only when the field is missing, empty or only spaces
  (`OrchardCore.Forms/Workflows/Activities/ValidateFormFieldTask.cs:59`), and always follows **Done**, plus **Valid**
  or **Invalid** (`:53`, `:69`); the workflow manager follows every outcome an activity returns
  (`OrchardCore.Workflows/Services/WorkflowManager.cs:789-810`). That is why the video chains the checks through
  **Done**.
- Validate Form follows **Valid** when the model state has no error, **Invalid** otherwise
  (`ValidateFormTask.cs:38-48`).
- The Validation option **Standard** only renders the message of the field after the widget
  (`Views/Items/FormElementValidationPart.cshtml`); it makes nothing required. The rule lives in the workflow.
  The Select widget has the same part (`OrchardCore.Forms/Migrations.cs:145`) and the same option, so
  `messages-2` is right to list Input, Text Area and Select; only Input and Text Area are shown.
- Only the published workflow answers the form: `HttpWorkflowController.Invoke` reads the workflow type, not the
  draft; the designer's documentation says a draft does not run.
- The first event added to a workflow becomes its start activity
  (`OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:175`).
- A new workflow is saved with a version 1 that has no activities (`WorkflowTypeStore.cs:87`), so the first publish
  of the video makes version 2. A form posted to the URL before that publish gets "not found"
  (`HttpWorkflowController.cs:110-127`): the published workflow has no start activity yet.
