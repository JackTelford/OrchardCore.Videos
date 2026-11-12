# Recording plan: Admin Templates

How to record the clips of `storyboard.json` without having to decide anything. Source references are paths and lines
of `OrchardCMS/OrchardCore` at commit `92db4a83`. A selector stands for `scene.Page.Locator(...)`. The video has three
recordings (`03-enable`, `04-create`, `05-manage`) and ten slide steps; the cards need no plan.

## The site it needs

| Setting | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout that has the `OrchardCore.AdminTemplates` feature, on `http://localhost:5010` |
| App data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Setup | Auto Setup (environment variables), recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls out |
| Admin theme | `TheAdmin`, which the Blog recipe sets (`TheBlogTheme/Recipes/blog.recipe.json:83-85`) |
| Features | The Blog recipe enables `OrchardCore.Templates` (and with it `OrchardCore.Liquid`), `OrchardCore.Features`, `OrchardCore.Contents` and `OrchardCore.Admin`. It does not enable `OrchardCore.AdminTemplates`: clip `03-enable` does |
| External services | None, so no local stand-in is needed |
| Sign-in | `Recorder setup` once, outside the capture, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`; `state.json` stays out of git |
| Browser | The recorder's Chromium: 1536x864 CSS pixels, light color scheme, `en-US` |

No mask script is needed: the site is made from scratch. No `PrepareAsync` is needed either: the site starts as Auto
Setup leaves it, and the only data of the video is created on camera or by the steps below.

## Fake data

There is no seed data. The only values typed in the video are:

| Value | Text |
|---|---|
| Name | `VisitSiteNavbarItem` |
| Description | `Visit Site link with a text label` |
| Content, first version | the Liquid below |
| Content, second version | the same Liquid with `{{ 'Open Site' | t }}` in place of the visible `{{ 'Visit Site' | t }}` (line 9) |

First version of the Content:

```liquid
<li class="nav-item">
    <a href="{{ '~' | absolute_url }}"
       class="nav-link"
       data-bs-toggle="tooltip"
       data-bs-placement="bottom"
       title="{{ 'Visit Site' | t }}"
       role="button">
        <i class="fa-solid fa-width-auto fa-external-link me-1" aria-hidden="true"></i>
        {{ 'Visit Site' | t }}
    </a>
</li>
```

It is the markup of `src/OrchardCore.Modules/OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:1-5` written in
Liquid (the Liquid form of the `VisitSiteNavbarItem` shape is in the documentation, `Admin/README.md`), with `me-1` on
the icon and the visible text added after it.

**Entering the Content.** The Content field is a Monaco editor with the Liquid language
(`Assets/ts/template-create.ts`). It closes tags and quotes by itself and offers suggestions, so typing the text
would change it. Set the text of the editor instead, once it is shown (`WaitForSelectorAsync(".template-editor .monaco-editor")`):

```
await scene.Page.EvaluateAsync("async (text) => { const monaco = await window.__orchardCoreMonacoReady; monaco.editor.getEditors()[0].setValue(text); }", liquid);
```

`window.__orchardCoreMonacoReady` is set by `OrchardCore.Resources/Assets/monaco/monaco-loader.ts:69`. If
`getEditors()` is missing, use `monaco.editor.getModels()[0].setValue(text)`. On the Create page the editor copies its
text to the hidden field on every change (`Assets/ts/template-create.ts`); on the Edit page it copies it when the form
is submitted (`Assets/ts/template-edit.ts`). Either way Save posts the text. The text appears at once, like a paste.

**The top-bar link.** The link has no text in its original form, and Bootstrap's tooltip moves its `title` to
`data-bs-original-title`, so a selector on the words **Visit Site** finds the link only while a template adds them.
Every highlight below uses the same structural selector, which matches the link in both forms, and nothing else in the
top bar (`OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:3` is the only top-bar view with that icon):

```
.user-top-navbar li.nav-item:has(i.fa-external-link)
```

**The Save button.** The main button of the bar at the bottom reads **Save** until someone selects the item of its
arrow menu (**and continue**): that selection is stored in the admin preferences of the browser
(`TheAdmin/Assets/js/TheAdmin/actionGroup.ts`; `TheAdmin/Assets/js/adminPreferences.ts` stores it in the local storage
and in a cookie whose name ends in `-adminPreferences`), and the main button then reads **Save and continue** and
returns to the edit form instead of the list. Never open the arrow menu while recording. If the main button reads
**Save and continue** before a take, clear that local storage entry and cookie of the site, and reload.

## Order, dependencies and snapshots

The clips depend on each other and must be recorded in the order of the storyboard: `03-enable`, `04-create`,
`05-manage`. Every one of them changes the site.

| Snapshot | Taken | State | Needed by |
|---|---|---|---|
| S0 | After Auto Setup and `Recorder setup` | Fresh Blog site. `OrchardCore.AdminTemplates` is disabled | `03-enable` |
| S1 | After `03-enable` | `OrchardCore.AdminTemplates` is enabled. No admin template | `04-create` |
| S2 | After `04-create` | One admin template, `VisitSiteNavbarItem`, with the first version of the Content | `05-manage` |

Stop the site and copy `ORCHARD_APP_DATA` aside for each snapshot; restore the one a clip needs, together with
`state.json`, before every new take of that clip. After `05-manage` the site is back to S1.

To record `05-manage` without S2: restore S1, open `/Admin/Templates/Admin`, select **Add Template**, enter the Name,
the Description and the first version of the Content as in `04-create`, and select **Save**, outside the capture.

## Clip `03-enable`

Starts on `/Admin` (the dashboard), signed in, from S0. The sidebar is not expanded.

`enable-1`
- Click **Tools** in the sidebar menu (text `Tools`, `OrchardCore.Features/AdminMenu.cs:26`), then
  `ClickAndWaitForNavigationAsync` on **Features** (`AdminMenu.cs:27`). The page is `/Admin/Features`. If the menu
  does not open, `NavigateAsync("/Admin/Features")`.
- `TypeAsync` in `#search-box` (placeholder **Search**, `OrchardCore.Features/Views/Admin/Features.cshtml:27`) the text
  `admin templates`. The page filters its rows on the name, the id and the description of each feature
  (`Drivers/FeatureEntryDisplayDriver.cs:20`).
- Wait until the row whose `<code>` reads `OrchardCore.AdminTemplates` is shown.
- On screen at the end: the Features page, the search box holding `admin templates`, the **Development** heading
  (`Manifest.cs` of `OrchardCore.Templates`, line 23) and the row **Admin Templates** `OrchardCore.AdminTemplates` with
  its description "The Admin Templates module provides a way to write custom admin shape templates." (`Manifest.cs:20-21`).
  If another row is still shown, that is fine; the Admin Templates row must be visible without scrolling.

`enable-2`
- First sentence (about 4 seconds): `HighlightAsync` on the dependency badge **Templates** in the row
  (`.direct-dependencies .badge`, `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`), then `ClearHighlightsAsync`.
- Then `ClickAndWaitForNavigationAsync` on the **Enable** link of that row (`a.btn.btn-primary.btn-sm` with text
  `Enable`, `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). The page reloads.
- A green notification reads "The feature Admin Templates was enabled." (`Controllers/AdminController.cs:317`). It
  names only the feature that was selected: `Enable` passes that one feature to `NotifyAsync` (`AdminController.cs:247`),
  and the features it depends on are enabled without being named (`ShellDescriptorFeaturesManager.cs:163-181`; on this
  site Templates and Liquid are on already, since the Blog recipe enables Templates). It disappears after 5 seconds.
- Type `admin templates` in `#search-box` again, so the frame at the end shows the row.
- On screen at the end: the row **Admin Templates** with a red **Disable** button instead of **Enable**. The
  notification may be gone already.

`enable-3`
- Click **Design** in the sidebar (`OrchardCore.Templates/AdminTemplatesAdminMenu.cs:19`). The submenu opens with
  **Templates** and **Admin Templates** (`AdminTemplatesAdminMenu.cs:20`; `AdminMenu.cs:19-20`). `HighlightAsync` on both
  entries for about 2 seconds, then `ClearHighlightsAsync`.
- `ClickAndWaitForNavigationAsync` on **Admin Templates**. The page is `/Admin/Templates/Admin`.
- On screen at the end: the title **Admin Templates** (`Views/Template/Index.cshtml:6`), the search box, the button
  **Add Template**, and the message "Nothing here! There are no templates for the moment. See documentation"
  (`Index.cshtml:20`).

## Clip `04-create`

Starts on `/Admin/Templates/Admin`, from S1 (the list is empty). Collapse nothing: the sidebar stays as it is.

`create-1`
- `ClickAndWaitForNavigationAsync` on **Add Template** (`a.btn.btn-secondary.create`,
  `Views/TemplateCreateButton.cshtml:6`). The page is `/Admin/Templates/Create?admintemplates=True&returnUrl=...`
  with the title **Create Template** (`Views/Template/Create.cshtml:8`) under the breadcrumb **Admin Templates**.
- `HighlightAsync` on the bar at the bottom (`.action-bar-bottom`) for about 2 seconds: it holds the **Save**
  button with its arrow, and **Cancel**, which is there because the **Add Template** button passes a local `returnUrl`
  (`Create.cshtml:62-65`). The **Preview** button (`Create.cshtml:67-70`) is absent because the form is the one of the
  admin templates. `ClearHighlightsAsync`.
- On screen at the end: the empty form with **Name**, **Description** and **Content** (`Create.cshtml:18, 27, 35`),
  and the bottom bar with **Save** and **Cancel**, and without a **Preview** button.

`create-2`
- `TypeAsync` in `#Name` the text `VisitSiteNavbarItem`, then in `#Description` the text `Visit Site link with a text label`.
  The hints under the fields read "The name of the template." and "The description of the template." (`Create.cshtml:22, 30`).
- On screen at the end: both fields filled, the Content still empty.

`create-3`
- `ClickAsync` in the editor (`#Content_editor`), then set the text of the editor to the first version of the
  Content with the script of "Entering the Content" above.
- The hint under the editor reads "The Liquid template." (`Create.cshtml:40`).
- On screen at the end: the editor holds the eleven lines of the first version, colored as Liquid and HTML, with no
  suggestion popup and no red underline.

`create-4`
- `HighlightAsync` on the editor (`#Content_editor`) for the length of the step, no caption. Do not scroll.
- On screen at the end: the same editor, highlighted, with the line `{{ 'Visit Site' | t }}` after the `<i>` line visible.

`create-5`
- Wait one second, then `ClickAndWaitForNavigationAsync` on **Save** (`button.btn.btn-primary.save`,
  `Create.cshtml:53`). The form posts, and the page returns to `/Admin/Templates/Admin` (the `returnUrl` of the
  **Add Template** button, `Controllers/TemplateController.cs:212, 367-371`).
- A green notification reads `The "VisitSiteNavbarItem" template has been created.` (`TemplateController.cs:204`);
  it disappears after 5 seconds.
- On screen at the end: the list with one row: the name `VisitSiteNavbarItem` as a link, the description under it,
  and the buttons **Edit** and **Delete** (`Views/TemplateEntry.Buttons.SummaryAdmin.cshtml:11, 19`;
  `TemplateEntry.Description.SummaryAdmin.cshtml`). The top bar already shows the changed link.

`create-6`
- `HighlightAsync` on the Visit Site link of the top bar (the structural selector of "Entering the Content") for
  about 3 seconds, then `ClearHighlightsAsync` before navigating.
- Click **Content** in the sidebar, then `ClickAndWaitForNavigationAsync` on **Content Items**
  (`OrchardCore.Contents/AdminMenu.cs:65, 68`). If the menu does not open, `NavigateAsync("/Admin/Contents/ContentItems")`.
- `HighlightAsync` on the same link again.
- On screen at the end: the Content Items page, and in its top bar the link with the external-link icon followed by
  the words **Visit Site**.

## Clip `05-manage`

Starts on `/Admin/Templates/Admin`, from S2 (one template). The top bar shows the icon and **Visit Site**.

`manage-1`
- `ClickAndWaitForNavigationAsync` on **Edit** of the row (`a.btn.btn-primary.btn-sm`, `Views/TemplateEntry.Buttons.SummaryAdmin.cshtml:11`).
  The page is `/Admin/Templates/Edit/VisitSiteNavbarItem?admintemplates=True&returnUrl=...` with the title
  **Edit Admin Template** (`Views/Template/Edit.cshtml:8`).
- Wait until the editor shows the saved Liquid (`.template-editor .view-lines` contains `Visit Site`). Hold about
  2 seconds so the saved name, description and Liquid are seen.
- Set the text of the editor to the second version of the Content (script of "Entering the Content" above).
- `ClickAndWaitForNavigationAsync` on **Save**. The page returns to the list. An edit shows no notification
  (`TemplateController.cs:250-292`).
- On screen at the end: the list with the row, and in the top bar the link with the icon followed by **Open Site**.

`manage-2`
- `ClickAsync` on **Delete** of the row (`a.btn.btn-danger.btn-sm`, `TemplateEntry.Buttons.SummaryAdmin.cshtml:19`).
  A confirmation dialog opens, titled **Delete**, asking "Are you sure you want to remove this element?" with the
  buttons **Ok** and **Cancel** (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). Hold for about 2 seconds.
- `ClickAndWaitForNavigationAsync` on **Ok** (`#modalOkButton`, `TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:30`). The
  dialog is shown for the **Delete** link because of its `data-url-af="RemoveUrl UnsafeUrl"` (`TheAdmin.ts:74-85,
  115-182`); **Ok** submits a form that posts to the `Delete` action, which redirects to the `returnUrl`. The page returns
  to the list.
- A green notification reads "Template deleted successfully." (`TemplateController.cs:322`); it disappears after 5 seconds.
- On screen at the end: the empty list with "Nothing here! There are no templates for the moment." The toast may be gone.

`manage-3`
- `HighlightAsync` on the link of the top bar (the structural selector of "Entering the Content", since the link has
  no text now) for the rest of the step, no caption.
- On screen at the end: the top bar shows the external-link icon alone, with no text, and the empty list below.

## Slides

| Step | The slide shows |
|---|---|
| `why-1` | Three cards, "Changing the admin the usual way": shapes draw the admin, a view overrides a shape, a change is a code change |
| `why-2` | Three check cards, "Admin Templates": written in the admin, named after a shape, stored in the database |
| `why-3` | Four cards, "The admin and the front end stay apart": Admin Templates for admin pages, Templates for front end pages, a template wins over a view, delete it to go back |
| `shapes-1` | Three cards, "A template is named after a shape": the shape type, an alternate with its view file name, and `Model` |
| `shapes-2` | Four cards, "Admin shapes the documentation overrides": AdminBranding, VisitSiteNavbarItem, Message, NotifyMessages |
| `ship-1` | A code panel, "The AdminTemplates recipe step": a recipe whose `AdminTemplates` step holds `VisitSiteNavbarItem` with its Description and a Content that points to a file |
| `ship-2` | Three cards, "The All Admin Templates deployment step": the step, its option Export templates as files, and the recipe step it produces |
| `good-1` | Three cards, "What Save checks": checked on save (name and content filled in, no other template with the name), not checked on save (whether the Liquid parses), found when the shape is drawn (a parse error shown in place of the markup) |
| `good-2` | Three cards, "Two permissions": Manage admin templates, Manage Liquid templates, both security critical |
| `good-3` | A code panel with the `Exclude` rule for `OrchardCore.AdminTemplates`, and three cards: where, rules, applies to |

Sources of the slides: `OrchardCore.Templates/Services/AdminTemplatesShapeBindingResolver.cs:39-43, 73` (admin
requests only), `OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:349-411` (a template is
asked for before the view files of the same name), `Recipes/AdminTemplateStep.cs`, `Deployment/AllAdminTemplatesDeploymentSource.cs:24-48`,
`Views/Items/AllAdminTemplatesDeploymentStep.*.cshtml`, `Controllers/TemplateController.cs:380-392, 394-424` (what Save
checks), `OrchardCore.Liquid/Services/LiquidTemplateManager.cs:98-114` (a parse error), `AdminTemplatesPermissions.cs`,
`OrchardCore.Liquid.Abstractions/Permissions.cs`, and `docs/reference/modules/Tenants/README.md` (the rule).

## Checks for private data

- The frames hold only the fake values above, the user `admin`, `admin@example.com` and the site name **Orchard
  Core**. Open no page that shows the machine's user name, a local path, or the real password; the password is never typed on camera.
- The Content Items page of `create-6` shows the sample content of the Blog recipe: confirm by OCR that it holds no
  real name.
- OCR the video at one frame per second and at every scene change, search for real names, emails (anything that is not
  `@example.com`), phone numbers and tokens, and look at every contact sheet before assembling.
