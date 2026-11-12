"""The slides of the Forms and Workflows video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from OrchardCore.Forms and OrchardCore.Workflows, trimmed with // ... to what the narration talks about.
"""
from brand import W, bullets, draw_code, slide_base

TYPES = {"ActionExecutedContext", "ExportModelStateAttribute", "ImportModelStateAttribute", "ModelStateTransferAttribute",
         "ImportModelStatePageFilter", "Controller", "MvcOptions", "IActionResult", "EmptyResult", "ViewResult",
         "PageResult", "WorkflowPayload", "WorkflowConstants", "WorkflowHttpResult", "IHtmlContent"}

PARTS = [
    ("1", "Form widget", "Renders the form. Its Action field holds a URL."),
    ("2", "Http Request Event", "Starts the workflow when a post reaches that URL."),
    ("3", "Validation tasks", "Check the fields and record errors in the model state."),
    ("4", "A redirect", "Back to the form with its errors, or to a thank-you page."),
]

OWN_ACTION = [
    ("1", "Post to your action", "Set the Action of the Form widget to a route of your controller."),
    ("2", "Add errors, then redirect", "Invalid model state and a redirect are stored in temp data."),
    ("3", "The page shows them", "The validation summary and the inputs read the model state."),
]

FORM_WRAPPER = """
<form id="@formId" action="@formPart.Action" method="@formPart.Method" ...>
    @if (formPart.EnableAntiForgeryToken)
    {
        @Html.AntiForgeryToken()
    }

    @if (formPart.SaveFormLocation)
    {
        <input type="hidden"
               name="@WorkflowConstants.FormLocationKeyInputName"
               value="@ViewContext.HttpContext.Request.Path" />
    }

    @await DisplayAsync((IHtmlContent)Model.Metadata.ChildContent)
</form>
"""

INVOKE = """
[IgnoreAntiforgeryToken]
[HttpGet, HttpPost, HttpPut, HttpPatch]
public async Task<IActionResult> Invoke(string token)
{
    if (!_securityTokenService.TryDecryptToken<WorkflowPayload>(token, out var payload))
    {
        // ...
        return NotFound();
    }
    // ...
    if (httpRequestActivity.ValidateAntiforgeryToken && (!await _antiforgery.IsRequestValidAsync(HttpContext)))
    {
        // ...
        return BadRequest();
    }
    // ...
    await _workflowManager.StartWorkflowAsync(workflowType, startActivity);
    // ...
    return GetWorkflowActionResult();
}
"""

RESULT = """
private IActionResult GetWorkflowActionResult()
{
    if (HttpContext.Items.ContainsKey(WorkflowHttpResult.Instance))
    {
        return new EmptyResult();
    }

    if (HttpContext.Items.TryGetValue(WorkflowConstants.FormOriginatedLocationItemsKey, out var value))
    {
        return Redirect(value.ToString().ToUriComponents());
    }

    return Accepted();
}
"""

LIQUID = """
Contact form submitted by {{ Request.Form.Name }}

<p>The following information was submitted:</p>
<p>Name: {{ Request.Form.Name }}</p>
<p>Email: {{ Request.Form.Email }}</p>
<p>Message: {{ Request.Form.Message }}</p>
"""

JAVASCRIPT = """
requestForm("Name")
"""

REGISTRATION = """
options.Filters.Add<ExportModelStateAttribute>();
options.Filters.Add<ImportModelStateAttribute>();
options.Filters.Add<ImportModelStatePageFilter>();
"""

EXPORT = """
public override void OnActionExecuted(ActionExecutedContext context)
{
    // Only export if ModelState is not valid.
    if (context.ModelState != null && !context.ModelState.IsValid && IsRedirect(context))
    {
        var controller = context.Controller as Controller;
        if (controller != null)
        {
            controller.TempData[Key] = ModelStateHelpers.SerializeModelState(context.ModelState);
        }
    }
    // ...
}
"""

IMPORT = """
public override void OnActionExecuted(ActionExecutedContext context)
{
    var controller = context.Controller as Controller;
    var serializedModelState = controller?.TempData[Key] as string;
    if (serializedModelState != null)
    {
        // Only Import if we are viewing.
        if (context.Result is ViewResult || context.Result is PageResult)
        {
            var modelState = ModelStateHelpers.DeserializeModelState(serializedModelState);
            context.ModelState.Merge(modelState);
        }
        else
        {
            // Otherwise remove it.
            controller.TempData.Remove(Key);
        }
    }
    // ...
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Two parts, linked by a URL")
        bullets(image, PARTS, active={0, 1})
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "The form element the widget renders")
        draw_code(image, (96, 240, W - 96, 900), "Form.Wrapper.cshtml", "cshtml", FORM_WRAPPER, types=TYPES, size=26)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "The controller that receives the post")
        draw_code(image, (96, 240, W - 96, 1030), "HttpWorkflowController.cs", "csharp", INVOKE, types=TYPES, size=24)
        return image

    if step_id == "how-4":
        image = slide_base(chapter_title, label, "What the controller answers")
        draw_code(image, (96, 240, W - 96, 860), "HttpWorkflowController.cs", "csharp", RESULT, types=TYPES, size=26)
        return image

    if step_id == "values-1":
        image = slide_base(chapter_title, label, "Reading the submitted values")
        draw_code(image, (96, 240, 1240, 740), "Liquid", "cshtml", LIQUID, types=TYPES, size=26)
        draw_code(image, (1280, 240, W - 96, 420), "JavaScript", "javascript", JAVASCRIPT, types=TYPES, size=26)
        return image

    if step_id == "state-1":
        image = slide_base(chapter_title, label, "Carry the errors across the redirect")
        draw_code(image, (96, 240, W - 96, 440), "Startup.cs", "csharp", REGISTRATION, types=TYPES, size=24)
        draw_code(image, (96, 470, W - 96, 1030), "ExportModelStateAttribute.cs", "csharp", EXPORT, types=TYPES, size=24)
        return image

    if step_id == "state-2":
        image = slide_base(chapter_title, label, "Merge them into the next page")
        draw_code(image, (96, 240, W - 96, 1030), "ImportModelStateAttribute.cs", "csharp", IMPORT, types=TYPES, size=24)
        return image

    if step_id == "state-3":
        image = slide_base(chapter_title, label, "The same works with your own action")
        bullets(image, OWN_ACTION)
        return image

    raise SystemExit(f"No slide for {step_id}")
