# Recording plan: Templates: Preview a Template on Any Page Before You Save It

This plan is for the person who records the video. It holds everything the recorder needs, so nothing is decided
while recording. In this plan a selector such as `.template-editor` stands for `scene.Page.Locator(".template-editor")`,
since the recorder's methods take locators. Source paths are relative to `src/OrchardCore.Modules/OrchardCore.Templates`
of the Orchard Core checkout, unless they start with `src/`.

The video has two recordings (`02-open` and `03-any-page`) and three sets of slides (`01-why`, `04-how`, `05-limits`). Only the recordings need a site.
Both recordings use two browser tabs of the same browser context: the editor tab (the Edit Template page) and the
preview tab (the page that the Preview button opens). The recorder captures one tab at a time, so every step below
says which tab is on screen when the step starts, and which one when it ends.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`. Run `src/OrchardCore.Cms.Web` on `http://localhost:5010`
  with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Block outbound calls from the site
  with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup** (no setup screen and no sign-in form is recorded). The environment variables:

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `VIDEO_PASSWORD` is chosen outside git. Write `<password>` wherever a password is needed in a document.
- **Tenant, recipe, time zone**: the `Default` tenant, the Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`),
  `America/Los_Angeles`, site name **Orchard Core**, user `admin`, email `admin@example.com`.
- **Features to enable**: none. The Blog recipe enables `OrchardCore.Templates` (blog.recipe.json:74), which brings
  `OrchardCore.Liquid`, and also `OrchardCore.Autoroute` (:50) and `OrchardCore.Deployment` (:57), which the seed and the
  preview use. Check after setup that **Design**, **Templates** is in the admin menu
  (`AdminMenu.cs:19-20`). The user `admin` has the Administrator role, which holds **Manage templates** and
  **Manage Liquid templates** by default (`Permissions.cs:18-31`, `src/OrchardCore/OrchardCore.Liquid.Abstractions/Permissions.cs:21-34`).
- **Settings**: none to save.
- **External services**: none. The Blog theme asks for Google Fonts (`Layout.liquid` of TheBlogTheme). So that a
  page never waits on the network, every recording starts with
  `await scene.Context.RouteAsync(new Regex(@"^https://fonts\.(googleapis|gstatic)\.com/"), route => route.AbortAsync());`
  right after `NewSceneAsync` (it needs `using System.Text.RegularExpressions;`). The pages then use their fallback fonts,
  the same in every take.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, once, outside the capture. The
  session is saved in `state.json`, which is never committed.

### The seed data (created outside the capture)

One template, `Content__BlogPost`, that replaces the detail page of a blog post. Its second line is a paragraph with
the words "Posted by", so that one edit in the editor changes the page visibly. The `padding-top` on the first line
keeps the paragraph clear of the theme's navigation bar, which sits over the top of the page.

Open **Tools**, **Deployments**, **JSON Import** (`/Admin/DeploymentPlan/Import/Json`), paste this recipe, and press
**Import**:

```json
{
  "steps": [
    {
      "name": "Templates",
      "Templates": {
        "Content__BlogPost": {
          "Description": "Detail view of a blog post",
          "Content": "<article style=\"padding-top: 7rem\">\n    <p class=\"lead\">Posted by</p>\n    <h1>{{ Model.ContentItem.DisplayText }}</h1>\n    {{ Model.Content | shape_render }}\n</article>"
        }
      }
    }
  ]
}
```

The JSON Import page edits the text with CodeMirror. With the recorder, set the value from a page script:
`await scene.Page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", recipe);`
then click `button.import` (`src/OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:21`). The recipe step is
the one in `src/docs/reference/modules/Recipes/README.md` (the `Templates` step), read by `Recipes/TemplateStep.cs`.

Check the seed:

1. `/Admin/Templates` lists exactly one template, `Content__BlogPost`, with the description "Detail view of a blog post".
2. `/blog/post-1` shows the title "Man must explore, and this is exploration at its greatest" under the words
   "Posted by", and the page has no large picture at the top. The theme's own `Content-BlogPost.liquid` draws a
   picture (a masthead), so its absence shows that the saved template is used instead of the template file.
3. `/` (the blog) looks like the home page of a Blog site: the post is listed with its title and subtitle, and the
   words "Posted by" appear only inside each summary of the theme, not above the list.

### Snapshot

After the seed and the checks, stop the site and copy its `App_Data` folder and `state.json` aside. Restore both
before every full take. The clip `03-any-page` saves the template, so the site must be restored before a clip is
recorded again.

## Order of the clips and what they change

| Order | Clip | Starts on | Changes the site? |
|---|---|---|---|
| 1 | `02-open` | the Templates list, `/Admin/Templates` | No. It opens the preview and leaves it open. |
| 2 | `03-any-page` | the Edit Template page of `Content__BlogPost`, with the preview tab already open | Yes. It saves the template: the content gets the words "the editorial team". |

The slide chapters (`01-why`, `04-how`, `05-limits`) are drawn by `slides.py` and need no site. The clips do not
depend on each other, because each one opens its own browser context from the snapshot. Record `02-open` first;
restore the snapshot before a new take of `03-any-page`.

## Locators used in both clips

| Name | Locator | Where the label or element comes from |
|---|---|---|
| Name field | `#Name` | `Views/Template/Edit.cshtml:19-21`, label "Name" |
| Editor | `.template-editor .monaco-editor` | `Views/Template/Edit.cshtml:38`, Monaco, language `liquid` |
| Editor lines | `.template-editor .view-lines` | Monaco's text area of lines |
| Save button | `button.btn.save` | `Views/Template/Edit.cshtml:54`, label "Save" |
| Preview button | `a.btn-info.action-bar-secondary` | `Views/Template/Edit.cshtml:70`, label "Preview", `target="_blank"` |
| Preview frame | `#iframe` | `Views/Preview/Index.cshtml:8` |
| Disconnected alert | `#notConnectedWarning` | `Views/Preview/Index.cshtml:12-14`, text "Preview Disconnected" |
| Error alert | `#serverErrorWarning` | `Views/Preview/Index.cshtml:17-19`, text "Preview Error" (not shown in the recordings) |

Inside the preview tab, the page of the site is in the frame: use `preview.FrameLocator("#iframe").Locator(...)`.

## Clip `02-open`

Chapter 02, "Open the preview". Four steps, all on the editor tab except the last.

Before `StartCaptureAsync` (not recorded):

```csharp
await using var scene = await NewSceneAsync(browser, directory, durations);
await scene.Context.RouteAsync(new Regex(@"^https://fonts\.(googleapis|gstatic)\.com/"), route => route.AbortAsync());
await scene.OpenAsync("/Admin/Templates");
// The sidebar shows Design expanded, with Templates as the active entry. If the Design menu is collapsed, expand it now.
await scene.StartCaptureAsync();
IPage preview = null;
```

- `open-1`: **The Templates list.** The narration names the menu first, then the template. Actions: highlight the
  **Templates** entry of the **Design** menu in the sidebar with
  `HighlightAsync(scene.Page.Locator("a[href$='/Admin/Templates']").First)`, which is the first match in the page and
  should be the sidebar entry (the link of the menu entry is `Action("Index", "Template", ...)`, `AdminMenu.cs:21`,
  and the `Index` action answers on `Templates`, `TemplateController.cs:65`). Pause 1500 ms, call `ClearHighlightsAsync()`,
  then `HighlightAsync(row link)` with `a:has-text("Content__BlogPost")`, no caption, and call `ClearHighlightsAsync()` at
  the end of the actions.
  On screen when the narration ends: the Templates page; the sidebar with **Design** expanded and **Templates**
  active; one row, `Content__BlogPost`, with the description "Detail view of a blog post"; the **Add Template** button
  (`Views/TemplateCreateButton.cshtml:6`). Labels: "Design" and "Templates" from `AdminMenu.cs:19-20`.
- `open-2`: **The editor.** Actions: `await scene.ClickAndWaitForNavigationAsync(row link)`, then
  `await scene.Page.Locator(".template-editor .monaco-editor").WaitForAsync();`, then
  `await scene.HighlightAsync(scene.Page.Locator("#Name"))`, a pause of 1500 ms, `ClearHighlightsAsync()`, a second
  `HighlightAsync(scene.Page.Locator(".template-editor"))`, and leave it. On screen when the narration ends: the Edit Template
  page (breadcrumb title "Edit Template", `Edit.cshtml:8`); the fields **Name** (`Content__BlogPost`), **Description**
  ("Detail view of a blog post") and **Content** with the five lines of Liquid, the second line reading
  `<p class="lead">Posted by</p>`; a highlight ring on the editor. Labels: `Edit.cshtml:19, 28, 36`.
- `open-3`: **The Preview button.** Actions: `ClearHighlightsAsync()`, then `HighlightAsync(preview button)`, which
  scrolls the button into view, so no explicit scroll is needed. **Save**, **Cancel** and **Preview** are in one action
  bar (`Edit.cshtml:45-72`); the editor was opened from the list, whose link carries a return address
  (`Views/TemplateEntry.Fields.SummaryAdmin.cshtml:11`), so the page shows **Cancel** (`Edit.cshtml:63-66`). Then
  `preview = await scene.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(previewButton));`, then
  `ClearHighlightsAsync()`. On screen when the narration ends: still the editor tab, with the **Save**, **Cancel** and
  **Preview** buttons visible and the click ripple on **Preview**. The new tab is open behind it.
  Label: "Preview", `Edit.cshtml:70`.
- `open-4`: **The preview tab.** Actions: `await scene.SwitchToAsync(preview);` then
  `await preview.FrameLocator("#iframe").Locator(".post-preview").First.WaitForAsync();`. On screen when the narration
  ends: the preview tab, filled by the blog home page of the site: the navigation bar with "Orchard Core", the
  blog title, and the post "Man must explore, and this is exploration at its greatest" as a summary, drawn by the
  theme's `Content-BlogPost.Summary.liquid` (the file is the shape `Content_Summary__BlogPost`, which is found before
  `Content__BlogPost`: see OPEN-QUESTIONS.md). No alert is shown, and the page does not contain the word "undefined".
  The page is the same one that `/` shows. The navigation bar is there because the `Render` view sets no layout, so the
  theme's layout wraps it (`src/OrchardCore/OrchardCore.DisplayManagement/Theming/ThemeLayout.cs`, the comment at the top).
  If the frame shows the word "undefined" or stays empty, call `await preview.ReloadAsync();` and wait again before
  `SwitchToAsync` (see OPEN-QUESTIONS.md).

## Clip `03-any-page`

Chapter 03, "Preview another page". Five steps that move between the two tabs. The editor tab is on screen when the
capture starts. Before `StartCaptureAsync` (not recorded):

```csharp
await using var scene = await NewSceneAsync(browser, directory, durations);
await scene.Context.RouteAsync(new Regex(@"^https://fonts\.(googleapis|gstatic)\.com/"), route => route.AbortAsync());
var editor = scene.Page;
await scene.OpenAsync("/Admin/Templates/Edit/Content__BlogPost");
await editor.Locator(".template-editor .monaco-editor").WaitForAsync();
var preview = await scene.Context.RunAndWaitForPageAsync(() => editor.Locator("a.btn-info.action-bar-secondary").ClickAsync());
await preview.WaitForLoadStateAsync(LoadState.NetworkIdle);
var frame = preview.FrameLocator("#iframe");
await frame.Locator(".post-preview").First.WaitForAsync();   // the preview shows the blog home page
await editor.BringToFrontAsync();
await editor.EvaluateAsync("window.scrollTo(0, 0)");   // the editor starts at the top of the page, whatever the click on Preview did
await scene.StartCaptureAsync();
```

- `any-1`: **Follow a link in the preview.** Starts on the editor tab. Actions: `await scene.SwitchToAsync(preview);`,
  `await scene.ClickAsync(frame.Locator(".post-preview a").First);`, then
  `await frame.Locator("article h1").WaitForAsync();`. On screen when the narration ends: the preview tab showing the
  page of the post, from the saved template: the words "Posted by" on their own line, under the navigation bar, then
  the title "Man must explore, and this is exploration at its greatest" and the body of the post. The words
  "the editorial team" are not there. No alert is shown. The cursor and the click ripple are visible on the link's
  place.
- `any-2`: **Finish the byline in the editor.** Starts on the preview tab. Actions:

  ```csharp
  await scene.SwitchToAsync(editor);
  await scene.ClickAsync(editor.Locator(".template-editor .view-lines"));   // gives the editor the focus
  await editor.Keyboard.PressAsync("Control+Home");
  await editor.Keyboard.PressAsync("ArrowDown");                              // line 2, the paragraph
  await editor.Keyboard.PressAsync("End");
  for (var i = 0; i < 4; i++) await editor.Keyboard.PressAsync("ArrowLeft");  // just before </p>
  await editor.Keyboard.TypeAsync(" the editorial team", new KeyboardTypeOptions { Delay = 70 });
  await editor.Keyboard.PressAsync("Escape");                                 // closes a suggestion list, if any
  ```

  Type no braces and no quotes: Monaco closes them by itself. On screen when the narration ends: the editor tab, with
  line 2 reading `<p class="lead">Posted by the editorial team</p>`, no suggestion list open, and the **Save** button
  not yet pressed.
- `any-3`: **The preview renders the post again.** Starts on the editor tab. Actions:

  ```csharp
  await frame.Locator("p.lead", new() { HasText = "Posted by the editorial team" }).WaitForAsync();   // in the other tab
  await scene.SwitchToAsync(preview);
  ```

  On screen when the narration ends: the preview tab, with the same post, now with the line "Posted by the editorial team"
  above the title. No alert is shown.
- `any-4`: **Save.** Starts on the preview tab. Actions: `await scene.SwitchToAsync(editor);` then
  `await scene.ClickAndWaitForNavigationAsync(editor.Locator("button.btn.save"));`. The page opens
  `/Admin/Templates` (the form was opened without a return address, so `Edit` redirects to the list:
  `Controllers/TemplateController.cs`, the `Edit` post action ending in `RedirectToReturnUrlOrIndex`). On screen when the
  narration ends: the Templates list, with the row `Content__BlogPost`, and no error message. Label: "Save", `Edit.cshtml:54`.
- `any-5`: **Preview Disconnected.** Starts on the Templates list. Actions:
  `await preview.Locator("#notConnectedWarning").WaitForAsync(new() { State = WaitForSelectorState.Visible });`, then
  `await scene.SwitchToAsync(preview);`, then `await scene.HighlightAsync(scene.Page.Locator("#notConnectedWarning"));`
  and `ClearHighlightsAsync()` before the end. On screen when the narration ends: the preview tab, with the alert
  reading "Preview Disconnected" at the top right (`Views/Preview/Index.cshtml:14`), over the post with the line
  "Posted by the editorial team". The preview page is drawn with its own bare layout (`Views/Preview/_Layout.cshtml`
  has no stylesheet), so expect the text and a close button without Bootstrap styling, not a blue box (see
  OPEN-QUESTIONS.md). The alert's source is the `unload` handler of the editor, `Assets/ts/template-edit.ts:59-62`,
  and the listener `Assets/ts/template-preview.ts:84-99`.

After the clip, `/blog/post-1` (in any tab of the same context) shows "Posted by the editorial team" above the title,
which confirms that the template was saved. Restore the snapshot before another take.

## Slide steps

- `why-1`: slide "Templates are matched by name": three cards (A name, Saved, Previewed); the first two are bright.
- `why-2`: the same slide with the third card, Previewed, bright.
- `how-1`: slide "1. The editor tells the preview tab": two code panels side by side, the editor's `sendFormData` that
  writes the name and content to local storage (`Assets/ts/template-edit.ts:32-43`) and the preview tab's `storage`
  listener (`Assets/ts/template-preview.ts:84-99`).
- `how-2`: slide "2. The Render action builds the page": the `Render` action of `Controllers/PreviewController.cs`
  (lines 53-106), trimmed.
- `how-3`: slide "3. The preview template is checked first": the lines of `Services/PreviewTemplatesProvider.cs` (:22-33)
  that add the template of the request, and `GetShapeBindingAsync` of `Services/TemplatesShapeBindingResolver.cs` (:36-53).
- `limits-1`: slide "What to know about the preview", first card, Permissions, bright (`Permissions.cs:8`,
  `Liquid.Abstractions/Permissions.cs:10`; the controller checks both at `PreviewController.cs:44-45` and `:56-57`).
- `limits-2`: the same slide, second card, Pages, bright (`PreviewController.cs:73-103`: the home route's `contentItemId`
  at :77-81, an Autoroute address through `IContentHandleManager` at :82-90, a 404 at :92-101; the page shows the status
  code in `Assets/ts/template-preview.ts:61-77`).
- `limits-3`: the same slide, third card, Shapes, bright (`TemplatesShapeBindingResolver.cs:47` matches the name of each
  alternate; `DefaultHtmlDisplay.cs`, `GetShapeBindingAsync`, tries the alternates from the last to the first, so a more
  specific alternate that has a template wins).
- `limits-4`: the same slide, fourth card, Admin templates, bright (`Views/Template/Edit.cshtml:68-71`).

## Checks for private data

- The only user is `admin` with `admin@example.com`; the post author in the summary is shown as `admin`. No other
  person, email or domain appears. Check the frames of the Templates list, the editor and the preview.
- No password appears: the site is set up by Auto Setup, and no sign-in form is recorded.
- The address bar is not part of the capture, so the port is not visible; the page text must not contain `localhost`.
- OCR the final video at one frame per second and at every scene change, as the skill asks, and search for names,
  email addresses and tokens. The text of the site comes from the Blog recipe and from the seed above only.
- `state.json`, the copy of `App_Data`, and any audio or video file stay out of git.
