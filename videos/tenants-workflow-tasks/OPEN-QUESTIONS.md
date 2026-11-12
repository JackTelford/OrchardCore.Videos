# Open questions: tenants-workflow-tasks

What the author could not settle from the source and the documentation, and what a person must confirm on a running
site. Source paths are under `src/OrchardCore.Modules/` of the Orchard Core checkout the content was written from, and
`src/OrchardCore/` for the shared libraries.

## 1. Create Tenant Task saves Name and Description swapped (settle before recording `04-create`)

What was checked: the editor posts every field with the same names as its labels, and the server copies them to the activity in
`OrchardCore.Tenants/Workflows/Drivers/CreateTenantTaskDisplayDriver.cs:25-26`:

```
activity.TenantName = new WorkflowExpression<string>(model.DescriptionExpression);
activity.Description = new WorkflowExpression<string>(model.TenantNameExpression);
```

So the text typed in **Name** is stored as the description, and the text typed in **Description** is stored as the tenant name (the
edit side, lines 11-12, reads them the right way round, so the editor shows them swapped when it is opened again). The designer's
apply does not redraw the form (`OrchardCore.Workflows/Controllers/WorkflowDesignerController.cs:541-586` answers `valid: true`
without a form, and the built panel code in `OrchardCore.Workflows/wwwroot/Scripts/Workflows/designer/workflows-designer.js`,
function `N` of `ServerFormHost`, reloads the editor only for an invalid form or `reloadEditor`), so on camera the typed values stay where they
were typed, and the swap shows only when the editor is opened again or when the workflow runs.

What follows from the code, not yet seen on a running site:
- With **Description** empty, the tenant name is empty and Create Tenant Task takes **Failed** (`CreateTenantTask.cs:107-110`).
- With a description such as `Bluebird Books`, the tenant name is `Bluebird Books` and the validator rejects it
  ("Invalid tenant name. Must contain characters only and no spaces.", `OrchardCore.Tenants/Services/TenantValidator.cs:70`), so the task takes **Failed** and the
  validation errors become the last result.
- The hint under Description says "(Optional)": it is not, while the swap exists.

What the content does about it: the plan types the same text, `Bluebird`, in Name and in Description, so the chain works with and without
the swap; the narration never calls Description optional or empty; the plan never opens the Create Tenant Task editor again after the
apply. The narration of `create-2` ("We type Bluebird in Name, and again in Description, so the tenant's name and its description read the same") does not say which box becomes
the tenant name and which the description, so it is true with and without the swap. It must be reworded if the swap is fixed and the take uses different texts.

To confirm (rehearsal step 4 of `recording-plan.md`): type `Bluebird` in Name and `Bluebird Books` in Description, apply, select another
activity and select Create Tenant Task again, then publish and run. Swapped values in the editor, or a Failed outcome, confirm the code as read.
Recommendation: report it to the Orchard Core maintainers (the fix swaps the two right-hand sides) and decide whether to record
before or after it is fixed. If it is fixed before the take, Description can hold a different text, and `create-2` should then say so.

## 2. Url Prefix is not optional in practice

The hint under **Url Prefix** says "(Optional)". `TenantValidator.cs:83-90` rejects a tenant whose host and prefix are both the same as another
tenant's, and the Default tenant of an Auto Setup site has neither a prefix nor a host, so an empty prefix with an empty **Host name** gives
"A tenant with the same host and prefix already exists." and the task takes **Failed**. The narration says the prefix "gives the site an address of its own" and
never calls it optional. To confirm: in the rehearsal, leave Url Prefix empty once and see the Failed outcome. If the recording site's Default tenant has a
host or a prefix configured, the behavior differs.

## 3. A password typed on screen

**Admin password** of Setup Tenant Task is a plain text box (`SetupTenantTask.Fields.Edit.cshtml:41`, `type="text"`), and the value is stored in the workflow as
plain text (`SetupTenantTaskDisplayDriver.cs:29`). With **Record activity data** on, the evaluated value would also be kept in the journal of every instance
(`OrchardCore.Workflows/Controllers/WorkflowTypeController.cs:253` checks it by default; `OrchardCore.Workflows.Abstractions/Models/WorkflowExecutionContext.cs:81-99`
records the result of every evaluated expression without hiding any); so the video clears it (`workflow-2`) and says why.

To decide: the plan masks the typed characters with a style rule added by a JavaScript init script kept outside git, and uses a throwaway `<tenant-password>`. Is a masked box,
with a narration that says the workflow stores the password as plain text, acceptable, or must the password be absent from the frame and the video? To confirm: that
`<tenant-password>` passes the password rules of the new tenant (the administrator is created by the setup handlers of the new tenant, `SetupService.cs:271-276`, `OrchardCore.Users/Services/SetupEventHandler.cs:33`;
the task takes Failed if the setup cannot create the user, `SetupTenantTask.cs:214-224`). The rules are not read from source here: confirm them in the rehearsal.

## 4. The designer's form behavior (read from a built file, not from source)

The shared panel code that applies an activity's form is imported from `.scripts/bloom`, which is not in the checkout. The behavior was read from the built
`workflows-designer.js`: a form change (a field losing the focus) applies after 600 ms, the button **Apply** applies at once and then reads **Saved!** for 1.8 s. To confirm on a
running site: that these two labels are what the button shows, and that Tab after the last field is enough. Also to confirm: how tall the activity panel needs to be so that the plan's
fields are visible (the plan pins the panel and drags its top edge before the capture), and the `data-cy` names and menu structure used as selectors.

## 5. Names on screen

The narration uses "Create Tenant Task", "Setup Tenant Task", "Disable Tenant Task" and "Enable Tenant Task", which are the names in the quick-add list and in the header of the canvas card
(`DisplayName` in `OrchardCore.Tenants/Workflows/Activities/*.cs`). The toolbox card of the same activities reads "Create Tenant" and "Setup Tenant"
(`Views/Items/*.Fields.Thumbnail.cshtml:1`), and the canvas card of Create and Setup Tenant Task also shows the type name in italics (`CreateTenantTask.Fields.Design.cshtml:7`; it is
product text, the narration does not read it). For Started By Workflow, the toolbox card and the body of the canvas card read "Started By Workflow" (`StartedByWorkflowEvent.Fields.Thumbnail.cshtml:1`, `StartedByWorkflowEvent.Fields.Design.cshtml:5`),
while the header of the canvas card reads the activity's display name, "Started By Workflow Event" (`OrchardCore.Workflows/Events/StartedByWorkflowEvent.cs:27`), and so does the quick-add list.
The narration says "Started By Workflow", which is on screen in the toolbox card and in the card body. The field labels differ between the two tasks: **Name** and **Host name** in Create, **Tenant name** in Setup, **Hostname** in the admin form (not used).

## 6. Statements of the slides that are read from the code, and not recorded

To confirm in the rehearsal:
- `wrap-1`: an unconnected **Failed** outcome ends the workflow and the instance reads **Finished** (`OrchardCore.Workflows/Services/WorkflowManager.cs:817`: with no transition and no blocking activity the status is Finished).
- `wrap-2`: a second run takes Failed in Create Tenant Task because the tenant exists (`CreateTenantTask.cs:113`).
- `activities-3` and `activities-4`: Setup needs an Uninitialized tenant (`SetupTenantTask.cs:131-139`), Disable needs a Running tenant other than Default (`DisableTenantTask.cs:46-54`),
  Enable needs a Disabled tenant (`EnableTenantTask.cs:49-57`), and all four take Failed outside the Default tenant (the first check of each `ExecuteAsync`).
  These tasks are never run on camera except Create and Setup. A throwaway workflow in the rehearsal can try Disable Tenant Task and Enable Tenant Task on Bluebird (Disabled then Running again).

## 7. A quirk that does not show with plain values

Setup Tenant Task hands the setup service its `AdminEmail` setting itself (the expression object, whose text is the expression) instead of the evaluated, trimmed email
(`SetupTenantTask.cs:201`). With a plain value such as `owner@example.com` the result is the same, which is why the video types plain values only. It is not in the narration.

Also not in the narration: Setup Tenant Task reads the current HTTP request when it runs (`OrchardCore.Setup.Core/Services/SetupService.cs:152`), so a start that has no request, such as a timer,
is not claimed for it. The narration of `wrap-2` and its slide speak of Disable Tenant Task and Enable Tenant Task only, which use no request in their code. To confirm, if the claim is kept: start a throwaway
workflow with a Timer event and a Disable Tenant Task (needs the Timer Workflows Activities feature, which this video's site does not enable) and see the tenant get disabled.

## 8. Pronunciation to check with `scripts/pronounce.py`

No `spoken` entry was added. Check: "Url Prefix" (as a word, or letter by letter), "Sqlite", "Bluebird", "owner@example.com" (said as "owner at example dot com"), "Multi-Tenancy", and "Setup" in
"Setup Tenant Task" next to "sets it up".

## 9. Timing and waits that only a running site shows

- How long the Blog setup takes inside the Run request (`run-2` cuts the wait off camera) and how long the first request to Bluebird takes (`result-2`).
- The title of the Bluebird home page: expected to contain the site name, `Bluebird Books`.
- That `Finished` is the status shown for a run in which both tasks took Done (`run-2`), and that the instance page highlights the three activities (`run-3`).
- That the hint texts of Create Tenant Task are not readable in the frames (they show a sample address, product text).
