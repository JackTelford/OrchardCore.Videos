# Open questions: diagnostics-error-pages

Everything in the narration was read in the Orchard Core source at commit `92db4a83` (the module
`OrchardCore.Diagnostics`, the module `OrchardCore.Templates`, `OrchardCore.DisplayManagement` and the Users module).
Nothing was run: no site was started to write this video. The points below are what a person must confirm on a running
site before or while recording. Where a point fails, change the narration of the step named in it, not the plan.

## 1. The theme layout around the error pages (`builtin-1`, `builtin-2`, `builtin-3`, `admin-4`)

Narration: the Not Found page is "drawn inside the layout of the active theme".
Checked: `DiagnosticsController.Error` returns a normal MVC view (`Views/Diagnostics/Error.cshtml`), and
`OrchardCore.DisplayManagement/Razor/RazorViewActionFilter.cs` gives MVC views the theme layout.
To confirm: `/missing-page` shows the navigation bar of `TheTheme` (the site name `Orchard Core`) above the heading
`The page could not be found.`.
Why `TheTheme`: `TheBlogTheme` and `TheAgencyTheme` each have their own `Views/HttpError-NotFound.liquid`, which replaces
the module's page (it reads "Page Not Found" in a masthead). The SaaS recipe sets `TheTheme`, which has no view named
`HttpError*`. If a different theme is used, confirm first that it has no such view.

## 2. Opening the Error route directly (`builtin-2`, `builtin-3`)

Narration: opening the route with a code previews a page "and does not cause an error".
Checked: the route `Error/{status?}` (`Startup.cs:17-22`) maps to the action `Error(int? status)`, which draws the
`HttpError` shape for any number.
To confirm: `/Error/403` and `/Error/405` show the headings named in the plan, and the browser's network log shows
status 200 for them (they are not real errors), while `/missing-page` shows 404.

## 3. The status number does not replace the status name (`flow-2`, `rank-1`)

Narration: the names are tried "the name of the status, then the number, then HttpError", and "the views of the module
use the names, so name your own the same way".
Checked in code: `HttpErrorShapeViewModel` adds the alternates `HttpError__<number>` then `HttpError__<name>`;
`DefaultHtmlDisplay.GetShapeBindingAsync` (line 355) tries the alternates from the last to the first. The module's own
view `HttpError-NotFound.cshtml` is bound to `HttpError__NotFound`, so for 400, 401, 403, 404 and 429 a template named
with the number is never reached. The documentation says `HttpError__NotFound` or `HttpError__404` are equivalent.
To confirm: in **Design > Templates**, create `HttpError__404` with any content and request `/missing-page`. The built-in
page should not change. Delete it, create `HttpError__NotFound`, and the page changes. Also confirm that a template
named `HttpError__405` changes `/Error/405` (405 has no built-in view). Delete the templates afterwards.

## 4. Order of the sources for one name (`rank-2`)

Narration: for each name, a template written in the admin wins over a view in the theme, and a view in the theme wins
over the view of the module.
Checked in code: `DefaultHtmlDisplay.GetShapeBindingAsync` asks every `IShapeBindingResolver` (the admin templates,
`TemplatesShapeBindingResolver`) before it reads the shape table; `DefaultShapeTableManager` (lines 135-136) orders the
bindings by the enabled features, with the modules before the active theme.
To confirm: add `Views/HttpError-NotFound.cshtml` to a copy of `TheTheme`; `/missing-page` should show the theme's
heading; then create the template `HttpError__NotFound` in the admin; the page should show the template's heading.

## 5. Typing into the Content editor of a template (`admin-3`)

The Content field is a Monaco editor (`Views/Template/Create.cshtml:37`, `Assets/ts/template-create.ts`) whose value is
copied to the hidden field `#Content` on every change. Monaco may close brackets and quotes when keys are pressed one by
one, which would add characters to the text. The plan inserts the text in one piece (`Keyboard.InsertTextAsync`),
which should add nothing.
To confirm: after the insert, the value of `#Content` is exactly
`<h2 class="text-danger">{{ "We could not find that page." | t }}</h2>`. If it is not, and the key-by-key fallback of
the plan does not give that text either, set the value with the Monaco API of the page, outside the capture, and
record only the click on Save.

## 6. The admin menu entries (`admin-1`)

The labels "Design" and "Templates" are read in `AdminMenu.cs:19-20`, but the markup of the admin menu (a collapsed
section, a link, a button) is not in the module. The plan gives a fallback (`/Admin/Templates`).
To confirm: how to click "Design" and then "Templates" on camera, and whether the menu needs another click to open.

## 7. Pronunciation of the `spoken` entries

`storyboard.json` has a `spoken` map for `HttpError__NotFound`, `HttpError__404`, `HttpError__MethodNotAllowed`,
`HttpError-NotFound.cshtml`, `HttpError-NotFound.liquid`, `HttpErrorShapeViewModel`, `HttpStatusCode`, `HttpError`,
`BadRequest`, `TooManyRequests` and `NotFound`. `narrate.py` replaces them in the order of the map, as plain text, so
the longer names come first.
To confirm: listen to each with `scripts/pronounce.py` before narrating. In particular "underscore underscore",
"dash", "dot C.S.H.T.M.L." and "HTTP Error".

## 8. A Liquid view in a theme (`theme-2`)

Narration: a view named `HttpError-NotFound.liquid` takes the same place as the `.cshtml` view.
Checked: the documentation of the module names `HttpError-NotFound.liquid`; `LiquidShapeTemplateViewEngine.cs:49`
registers the extension `.liquid`; `ShapeTemplateBindingStrategy.cs` reads the files of the `Views` folder for every
registered extension; `BasicShapeTemplateHarvester.cs:42` turns the hyphen of the file name into two underscores. Not run.
The services of the `.liquid` views are registered by the always-enabled feature `OrchardCore.Liquid.Core`
(`OrchardCore.Liquid/Startup.cs:99`), but the filter `t` is registered by the feature **Liquid** (`Startup.cs:44`), so
the narration of `theme-2` says the Liquid feature is enabled.
To confirm: put the file in a copy of `TheTheme` (with no `.cshtml` file of the same name), and request `/missing-page`.

## 9. A path with a file extension (`limits-1`)

Narration: a path that ends in a known file extension "keeps its plain error status and gets no error page".
Checked: `DiagnosticsStartupFilter.cs:31-46` disables the status code pages when the path has a content type known to
`FileExtensionContentTypeProvider` and the response status is below 200 or from 400.
To confirm: request `/images/missing.png`; the answer is a 404 with an empty body, and no error page.

## 10. Unhandled exceptions (`limits-2`, `theme-3`)

Narration: outside Development, an unhandled exception goes to the Error route with no status code, and the `HttpError`
view draws the page; in Development, that handler is not registered; `Code` and `HttpStatusCode` are empty.
Checked: `DiagnosticsStartupFilter.cs:24-27` and the host `OrchardCore.Cms.Web/Program.cs:21-24` register
`UseExceptionHandler("/Error")` only when the environment is not Development; the route has an optional `status`.
To confirm: run a site in the `Production` environment with a module that throws, request its URL, and check that the
page reads "An error occurred while executing this request."

## 11. What the status code pages handle (`flow-1`)

Narration: "When a request ends with an error status ... runs the request again against the Error route".
Checked: `DiagnosticsStartupFilter.cs:29` registers `UseStatusCodePagesWithReExecute("/Error/{0}")`. What that ASP.NET
Core middleware accepts as an error response (a status from 400 to 599, and no body written) is framework behaviour, not
Orchard Core source, and was not read here.
To confirm: in the ASP.NET Core documentation of `UseStatusCodePagesWithReExecute`, which responses it replaces. The
video shows only a request for a path that no route handles, which has no body. If the middleware has another condition,
change "ends with an error status" in `flow-1`.

## 12. Enabling the Templates feature before the recording

The SaaS recipe does not enable Templates (`OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:12-37`).
Checked: `OrchardCore.Templates/Manifest.cs:9-15` (feature `Templates`, dependency `OrchardCore.Liquid`) and the button
`Enable` of `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
To confirm: after enabling it, **Design** lists **Templates**, `/Admin/Templates` opens, and the Liquid editor of the
Create page loads (the Monaco editor needs no network access: confirm with the outbound calls blocked).
