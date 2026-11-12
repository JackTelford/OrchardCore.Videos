# Open questions: Write a Content Part With a Display Driver

Everything the narration states was read from the Orchard Core source (commit `92db4a83`) and the documentation pages of the
tracker entry. Nothing was run: there is no site in this session. A person must confirm the following on a running site
before recording.

## What the video shows, and why

The code on the slides is the code of the Title part (`src/OrchardCore.Modules/OrchardCore.Title`), copied from the
source and trimmed with comments that read "...". No part was invented: a part of the video's own would be code that
nobody compiled, and the part in the module template (`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module`)
does not compile: its driver overrides `UpdateAsync(MyTestPart, IUpdateModel)` and calls `Edit(model)`, two methods that
`ContentPartDisplayDriver<TPart>` does not declare, and its migration awaits in a method that is not `async`. The
recorder therefore needs no module.

## To confirm on a running site

1. **The heading on the front end.** The Title part places its shape in the `Header` zone (`TitlePartDisplayDriver.cs`
   lines 34 and 35). `OrchardCore.Contents/Views/Content.cshtml` renders `Model.Header`, and "The Default Theme" has no
   `Content` template of its own, so the page of a Workshop should show `<h1>Pruning Apple Trees</h1>` inside
   `article.content-item`. The Blog theme does not: `TheBlogTheme/Views/Content.liquid` renders only `Model.Content`, so
   the video uses the Blank recipe and makes "The Default Theme" current. Confirm that `use-4` shows the heading, and
   that the page has no other `h1` that the highlight could pick.
2. **The selectors of the plan** were read from the views, not probed: `.oc-breadcrumb-title` (if the title is not on
   the page in the admin theme, highlight the entry **Content Types** of the admin menu, expanded before the capture, with
   the same caption), `li.list-group-item:has(label:text-is("Title"))`, `button[name="submit.AddAndConfigure"]`,
   `#TitlePart_Title` (the id follows from the prefix `TitlePart` and the property `Title`), `button.publish`,
   `span.field-validation-error`, `.toast.message-success`, `a.btn.view` and `a.btn.edit`. Probe each with
   `Recorder probe` before writing `Scenes.cs`.
3. **A single creatable type.** After `06-attach` the Content Items page must show one button, **New Workshop**
   (`ContentsAdminListCreate.cshtml` line 14). Among the features the Blank recipe enables, no migration calls
   `Creatable()` (only Taxonomies does, and it is not enabled), so no other type should be creatable. If the page shows a
   **New** drop-down instead, `use-1` has to open it and choose Workshop.
4. **Enable, then Make Current.** On the Themes page, the card **The Default Theme** shows **Enable** while the theme is
   disabled (`ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml`), and **Make Current** is the other button of the card.
   Confirm that both buttons are shown at once, or that **Make Current** appears after **Enable**.
5. **The list of parts.** The row is labeled **Title** because the display name of a part without one is its name
   without "Part" (`ContentPartSettingsExtensions.cs` line 60). Confirm that no other row is labeled **Title**, and that
   the description "Provides a Title for your content item." is shown under it.
6. **The toasts.** Success toasts fade after 5 s. Confirm on the contact sheets that the toast of `use-3` ("Your
   Workshop has been published.") is on screen while the end of its narration is spoken, and move the click earlier if not.
7. **The editor, after a failed validation.** `AdminController` (Contents) cancels the session and builds the editor again
   when the model state is invalid (lines 782 to 793), and the form keeps the return address (`Create.cshtml`,
   `asp-route-returnUrl`). Confirm that the message is displayed under the field **Title** as well as in the validation
   summary at the top of the form, and that the next **Publish** returns to Manage Content, where **View** is.
8. **The length of the actions.** The plan estimates the narration of each step at 2.5 words per second. After
   `narrate.py`, check in `durations.json` that the actions of every step are shorter than its narration.
9. **The layout of the list.** `ContentsAdminList.cshtml` declares a grid layout, and `AdminListShapeTableProvider.cs`
   defaults to the List layout (line 54). In the List layout the buttons **Edit** and **View**
   (`ContentsButtonEdit_SummaryAdmin.cshtml`) are expected on the row of the item. Confirm on the contact sheet of `use-3`
   that both are visible without opening a menu, and that the site does not remember the Grid layout.

## To confirm by ear

The `spoken` entries of `storyboard.json` are plain text replacements (`narrate.py` applies them in the order of the map, so a name that contains another one,
such as `TitlePartViewModel` and `TitlePart`, has to come first in the map, as it does).
Listen to each with `scripts/pronounce.py` before narrating: "Content Part Display Driver", "Title Part View Model",
"Title Part underscore Edit", "Alter Part Definition Async", "Try Update Model Async", "Get Display Shape Type",
"Get Editor Shape Type", "Use Display Driver", "Add Data Migration", "Add Content Part", "Update Async", "Add Handler",
"Bind Never", "Content Part" and "Title Part". Also listen to the step `attach-2`: the entry "Add & Configure" makes the voice say "Add and Configure".
