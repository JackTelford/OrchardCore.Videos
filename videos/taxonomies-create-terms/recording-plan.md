# Recording plan: Create a Taxonomy and Organize Its Terms

For the person who records `taxonomies-create-terms`. Read this file and `storyboard.json`; nothing else has to be decided.
Selectors below stand for `scene.Page.Locator(...)`. They were read from the Orchard Core source (commit `92db4a83`), not
from a running site, so run `Recorder probe <path> "<script>"` once to confirm any selector that does not match, and
write what you find in `OPEN-QUESTIONS.md`. Where a plan line says "label", the text is the label on screen and the
`file:line` after it is where the source defines it (paths are under `src/OrchardCore.Modules/` of the Orchard Core
source unless they start with `src/`).

The video has five recordings (`02-create`, `03-add`, `04-edit`, `05-organize`, `06-publish`), five slides, an opening
card and a closing card. The recordings change the site and depend on each other.

## The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that has the Taxonomies module, on `http://localhost:5010` |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup, so no setup or sign-in form is recorded (the Setup screen is not part of this video) |
| Recipe | `Blog` (it enables Taxonomies, Title, Alias, Autoroute, Contents, Content Types and Deployment, which this video needs) |
| Tenant | `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, SQLite |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`) |
| Network | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no outside service |
| External services | None |

Auto Setup environment variables (set them in the shell that starts the site; never write a real password in a file):

```bash
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export OrchardCore__AutoSetup__AutoSetupPath=""
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

The keys are those of `src/docs/reference/modules/AutoSetup/README.md`; the `orchardcore-tester` skill of the Orchard Core
repository (`references/autosetup.md`) shows how to run the site with them. The values are the ones above.

Settings: none. Leave Settings, Admin, "Display New menu" unchecked (the video uses the New button of the Content Items
page, not the New menu of the sidebar). Features to enable before recording: none beyond what the Blog recipe enables.
Check in Tools, Features that Taxonomies, Title, Alias, Autoroute and Deployment are enabled.

### The seed: the Topic content type (outside the capture)

The video chooses a content type named **Topic** as the Term Content Type. It is not part of any recipe, so create it
once, before the first take, with JSON Import:

1. Sign in once with `Recorder setup` (writes `state.json`, which stays out of git).
2. The page is `/Admin/DeploymentPlan/Import/Json` (Tools, Deployments, JSON Import; label from `OrchardCore.Deployment/AdminMenu.cs:30`).
   Its JSON field is a CodeMirror editor, so the text has to be set through the editor, not typed into the hidden text area.
3. Add this method to `Scenes.cs` of the copied recorder, then run `Recorder prepare` once (it runs outside any capture):

   ```csharp
   public static async Task PrepareAsync(IBrowser browser)
   {
       var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
       var page = await context.NewPageAsync();
       await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
       await page.EvaluateAsync("(t) => document.querySelector('.CodeMirror').CodeMirror.setValue(t)", RecipeText);
       await page.Locator("button.import").ClickAsync(); // Import, OrchardCore.Deployment/Views/Import/Json.cshtml:21
       await page.WaitForURLAsync(url => url.Contains("/Import/Json", StringComparison.OrdinalIgnoreCase), new() { Timeout = 30_000 });
       await Task.Delay(1500);
       await context.CloseAsync();
   }
   ```

   `RecipeText` is a string constant that holds exactly:

   ```json
   {
     "steps": [
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "Topic",
             "DisplayName": "Topic",
             "Settings": { "ContentTypeSettings": {} },
             "ContentTypePartDefinitionRecords": [
               {
                 "PartName": "TitlePart",
                 "Name": "TitlePart",
                 "Settings": { "ContentTypePartSettings": { "Position": "0" } }
               }
             ]
           }
         ],
         "ContentParts": []
       }
     ]
   }
   ```

4. The import shows the message "Recipe imported successfully!" for about 5 seconds (`OrchardCore.Deployment/Controllers/ImportController.cs:185`),
   so do not rely on seeing it. Check in Design, Content Definition, Content Types that **Topic** exists. It must stay without a stereotype, not draftable,
   not creatable and not listable (the recipe sets none of them), so its editor shows a title and a single Publish button.

No other data is created outside the capture. The Blog recipe already holds the taxonomies **Tags** and **Categories**
(`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:859`, `:925`); leave them alone. They appear in the Content
Items list next to **Topics**, which is fine: they are fake data from the recipe.

Take snapshot **S0** now (see below).

## Order, dependencies and snapshots

Record in this order; each recording starts from the state the previous one left:

| Clip | Needs | Changes the site | Leaves the site with |
|---|---|---|---|
| `02-create` | S0 | yes | the published taxonomy Topics (Term Content Type: Topic), with no terms |
| `03-add` | after `02-create` | yes | Topics with a draft that holds Cooking (Baking, Grilling, Roasting), Gardening (Vegetables) and Herbs |
| `04-edit` | after `03-add` | yes | Grilling renamed Barbecue, Roasting deleted |
| `05-organize` | after `04-edit` | yes | Gardening (Vegetables, Herbs) above Cooking (Baking, Barbecue), saved as a draft |
| `06-publish` | after `05-organize` | yes | Topics published, with no draft |

Snapshots: **stop the site by its process ID** (never by image name), copy the `ORCHARD_APP_DATA` folder and `state.json`
aside (`cp -r "$ORCHARD_APP_DATA" <snapshot folder outside git>`), and start the site again. Take **S0** after the seed,
**S1** after `02-create`, **S2** after `03-add`, **S3** after `04-edit`, **S4** after `05-organize`. To record a clip
again, stop the site, restore the snapshot of the state before it (clip N needs S(N-1)), start the site, and record.
To record everything again, restore S0.

The pauses in this plan assume the narration is spoken at 150 words per minute, 0.4 second per word. After `narrate.py`,
`durations.json` holds the real length of each step; if a step is longer or shorter than the plan expects, move the pause so
the action lands on the sentence it goes with, and keep every step's actions shorter than its narration.

Each clip starts a fresh browser context from `state.json`. Before the capture starts (`scene.OpenAsync`, not recorded)
every clip opens the page named under "Starts on".

Two helpers for the clips that start on the taxonomy editor. The editor is opened without a `returnUrl`, as it is after
a term is saved:

```csharp
// Opens the Topics editor without a returnUrl (not recorded).
async Task OpenTopicsEditorAsync(Scene scene)
{
    await scene.OpenAsync("/Admin/Contents/ContentItems");
    var href = await scene.Page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First.GetAttributeAsync("href");
    await scene.OpenAsync(href!.Split('?')[0]);
}

// The row of a term in the list of terms.
ILocator Row(Scene scene, string term) => scene.Page.Locator("ol#menu > li.menu-item")
    .Filter(new() { Has = scene.Page.Locator(".menu-item-title > span", new() { HasTextRegex = new Regex($"^{term}$") }) });
```

Selectors used in several clips (from `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml` and `Content.TermAdmin.cshtml`):

| What | Selector |
|---|---|
| The Add Topic button | `scene.Page.GetByRole(AriaRole.Link, new() { Name = "Add Topic", Exact = true })` |
| The Edit, Add and Delete buttons of a row | `Row(scene, "Cooking").Locator(".menu-item-title a.btn-success")`, `a.btn-primary`, `a.btn-danger` |
| The grip of a row, for dragging | `Row(scene, "Cooking").Locator(".menu-item-title")` (grab its empty middle, not a button) |
| The title field of a term or taxonomy | `input[name='TitlePart.Title']` |
| The Publish button | `button[name='submit.Publish']` (first match: the main button, never the "and continue" item) |
| The Save Draft button | `button[name='submit.Save']` (first match) |
| The confirm button of the Delete dialog | `#modalOkButton` (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:30`) |

The action buttons remember the last "and continue" choice in the admin preferences of the browser. This plan never uses
"and continue"; if a clip shows "Publish and continue" on the main button, the preferences have changed: use a fresh
`state.json` made by `Recorder setup`.

## Chapter 00, opening card

`intro`: the card of chapter 00 (made by `assemble.py`; nothing to record).

## Chapter 01, slides (`slides.py`; nothing to record)

- `why-1`: three cards, Taxonomy, Terms, Term content type.
- `why-2`: the Topics taxonomy as a tree: Topics, then Gardening (Vegetables, Herbs) and Cooking (Baking, Barbecue), with a note that every term is a Topic.
- `why-3`: three cards, terms live inside the taxonomy, changes go to a draft, publishing applies them.
- `why-4`: two cards, the Taxonomies feature in Tools, Features, and the Manage taxonomies permission.
- `why-5`: two cards, any content type without a stereotype, and Topic.

## Chapter 02, `02-create`: create a taxonomy

**Starts on:** `/Admin/Contents/ContentItems` (Content, Content Items; the page title is Manage Content). **Site state:** S0.
The list must hold no item called Topics.

**`create-1`**
- Actions: `ClickAsync(scene.Page.Locator("#new-dropdown"))`; `PauseAsync(1200)` so the open list is visible; then
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.dropdown-menu a.dropdown-item[href*='/ContentTypes/Taxonomy/Create']"))`.
  The list of types can scroll: Playwright scrolls Taxonomy into view when it clicks.
- Visible at the end: the page **New Taxonomy** (breadcrumb: Manage Content, New Taxonomy) with the empty editor.
- Labels: Content, Content Items `OrchardCore.Contents/AdminMenu.cs:65`, `:68`; New `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`;
  New Taxonomy `OrchardCore.Contents/Views/Admin/Create.cshtml:26`.

**`create-2`**
- Actions: `TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']"), "Topics")`; then
  `HighlightAsync(scene.Page.Locator("input[name='AliasPart.Alias']"), "Generated from the title")` and
  `HighlightAsync(scene.Page.Locator("input[name='AutoroutePart.Path']"))`. Leave Alias and Permalink empty. A "Route contained items" box (unchecked)
  shows under Permalink: leave it as it is (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:100`; it shows because the
  Taxonomy type allows contained item routing, `OrchardCore.Taxonomies/Migrations.cs:40`, and `OrchardCore.Taxonomies/Handlers/TaxonomyPartHandler.cs:10` adds the terms as contained items).
- Visible at the end: Title shows **Topics**; the Alias and Permalink fields are ringed. The Permalink field is disabled
  and its hint reads "The url of the content item. It will be automatically generated."
- Labels: Title `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; Alias `OrchardCore.Alias/Views/AliasPart.Edit.cshtml:10`;
  Permalink `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`, hint `:50`.

**`create-3`**
- Actions: `ClearHighlightsAsync()`; `ScrollAsync(0, 300)` only if Term Content Type is below the fold;
  `SelectAsync(scene.Page.Locator("select[name='TaxonomyPart.TermContentType']"), "Topic")`;
  `HighlightAsync(scene.Page.Locator("select[name='TaxonomyPart.TermContentType']"))`.
- Visible at the end: the **Term Content Type** list shows **Topic** (a required field, marked with the required asterisk).
- Labels: Term Content Type `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:26`; the list holds the content types
  without a stereotype, `:22`. The headless browser draws no open list, so the options are not visible; that is expected.

**`create-4`**
- Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Publish']").First)`.
- Visible at the end: the Content Items list with, at the top, the row **Topics** with the type Taxonomy (the list is sorted
  by last modified). Wait until the row is visible. The message **Your Taxonomy has been published.** is a toast that hides
  itself 5 seconds after the list loads, so it is gone before the narration ends; the narration does not mention it.
- Labels: Publish `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` (the page was opened from the New button, which carries a
  `returnUrl`, so the button sits in a group whose main button is Publish); message `OrchardCore.Contents/Controllers/AdminController.cs:473`,
  toast `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71` (5000 ms); Publish returns to the list: `AdminController.cs:795`.

Take snapshot **S1** after this clip.

## Chapter 03, `03-add`: add terms

**Starts on:** `/Admin/Contents/ContentItems`. **Site state:** S1. Take the whole clip in one session; the taxonomy
editor never shows a message after a term is saved.

**`add-1`**
- Actions: `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First)`;
  then scroll until the Add Topic button is in view (`ScrollAsync(0, 300)` if needed); `HighlightAsync` on the Add Topic button.
- Visible at the end: the editor **Edit Taxonomy** with Term Content Type **Topic**, a ringed **Add Topic** button, and an
  empty list of terms below it.
- Labels: Add Topic `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:48` (the button appears only for a saved
  taxonomy with a stored Term Content Type, `:21`, `:37`).

**`add-2`**
- Actions: `ClearHighlightsAsync()`; click the Add Topic button with `ClickAndWaitForNavigationAsync`; on the page **New Topic**,
  `TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']"), "Cooking")`; `ClickAndWaitForNavigationAsync` on the Publish button.
- Visible at the end: the taxonomy editor again, with one row **Cooking** and its three buttons Edit, Add and Delete.
- Labels: New Topic `OrchardCore.Taxonomies/Views/Admin/Create.cshtml:19`; Publish `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`
  (the term editor shows the Publish button alone, because Topic is not draftable). The term goes into a draft of the
  taxonomy: `OrchardCore.Taxonomies/Controllers/AdminController.cs:110`, redirect to the taxonomy editor `:165`.

**`add-3`**
- Actions: `HighlightAsync(Row(scene, "Cooking").Locator(".menu-item-title .float-end"), "Edit, Add and Delete")`; `PauseAsync(1200)`;
  `ClearHighlightsAsync()`; then the same creation as in `add-2` with the title **Gardening**.
- Visible at the end: two rows at the top level, **Cooking** and **Gardening**, one below the other.
- Labels: Edit, Add, Delete `OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:27`, `:36`, `:45`.

**`add-4`**
- Actions: `ClickAndWaitForNavigationAsync(Row(scene, "Cooking").Locator(".menu-item-title a.btn-primary"))` (the Add button of the
  Cooking row, not Add Topic); type **Baking**; Publish.
- Visible at the end: the order Cooking, Baking, Gardening, with **Baking** indented below **Cooking**.
- Label: Add `OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:36`.

**`add-5`**
- Actions: twice, the Add button of the Cooking row, then the title, then Publish: first **Grilling**, then **Roasting**.
- Visible at the end: Cooking, Baking, Grilling, Roasting (the three indented), then Gardening. Each term sits after the
  terms its parent already held.

**`add-6`**
- Actions: the Add button of the **Gardening** row, title **Vegetables**, Publish; then the Add Topic button, title **Herbs**,
  Publish. Scroll down if the Add Topic button left the screen.
- Visible at the end: Cooking, Baking, Grilling, Roasting, Gardening, Vegetables (indented), then **Herbs** at the top level
  (not indented).

Timing: each term takes about 6 seconds of actions, and `add-5` and `add-6` hold two terms each in about 13 seconds of
narration. If a step's actions run longer than its narration, type faster (`delay: 30` in `TypeAsync`).

Take snapshot **S2** after this clip. The taxonomy now has a draft; the published version still has no terms.

## Chapter 04, `04-edit`: edit and delete terms

**Starts on:** the Topics editor without a `returnUrl` (`OpenTopicsEditorAsync`). **Site state:** S2.

**`edit-1`**
- Actions: `ClickAndWaitForNavigationAsync` on the Edit button of the **Grilling** row (`a.btn-success`); on the page **Edit Topic**,
  `FillAsync("")` on the title field, then `TypeAsync(..., "Barbecue")`; Publish.
- Visible at the end: the editor again, with **Barbecue** in the place where Grilling was (third row of the Cooking group).
- Labels: Edit `OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:27`; Edit Topic `OrchardCore.Taxonomies/Views/Admin/Edit.cshtml:19`.

**`edit-2`**
- Actions: `PauseAsync(3500)`; click the Delete button of the **Roasting** row (`a.btn-danger`) with `ClickAsync`, which opens a dialog and does not
  navigate; do not select Ok in this step (`edit-3` starts with "Select Ok."). The click lands as the narration reaches "Let's delete
  Roasting", and the dialog is open while it says "A dialog asks you to confirm."
- Visible at the end: the dialog titled **Delete**, with the text "Are you sure you want to remove this element?" and the
  buttons **Ok** and **Cancel**, over the list.
- Labels: Delete `OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:45`; the dialog text `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`
  (title Delete, message, Ok, Cancel). The dialog needs the antiforgery token that the editor form holds.

**`edit-3`**
- Actions: `ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"))` as the step starts, while the narration says
  "Select Ok." (the Ok button: the page reloads); then at once
  `HighlightAsync(scene.Page.Locator(".oc-notify-toast-container .toast.message-success"))`, `PauseAsync(2500)`, `ClearHighlightsAsync()`.
  The message is a toast that hides itself 5 seconds after the page loads, and the narration reads it between about 1 and 3.5 seconds
  into the step. Clear the ring before the toast goes, since a highlight stays on the screen when its element is gone.
- Visible while the narration reads the message: the editor with the toast **Taxonomy item deleted successfully.**, ringed. Visible at the
  end: the editor without the toast, with no **Roasting** row; Cooking holds Baking and Barbecue.
- Label: message `OrchardCore.Taxonomies/Controllers/AdminController.cs:347`; toast `OrchardCore.Themes/TheAdmin/Views/Message.cshtml:15`,
  `NotifierExtensions.cs:71`.

Take snapshot **S3** after this clip.

## Chapter 05, `05-organize`: organize the hierarchy

**Starts on:** the Topics editor without a `returnUrl`. **Site state:** S3. The rows are, in order: Cooking, Baking,
Barbecue, Gardening, Vegetables (indented), Herbs. Never click a link or button other than Save Draft in this clip:
after a drag the page asks the browser to confirm before it is left.

How the drag works (`OrchardCore.Taxonomies/wwwroot/Scripts/menu/menu.js`): the list is one flat list of rows; a row is
dragged by its grip (`.menu-item-title`); the rows below it that are deeper than it move along with it; the new level
follows the sideways movement of the pointer, one level deeper when it moves more than 40 pixels to the right and one
level up when it moves more than 40 pixels to the left, never deeper than one level below the row above. Drag **upward**
only: a parent that is dragged downward loses its children from the page while it moves, so the row positions change
under the pointer.

**`org-1`**
- Actions: `await scene.DragToAsync(Row(scene, "Gardening").Locator(".menu-item-title"), Row(scene, "Cooking").Locator(".menu-item-title"), 0, -14);`
  (the pointer ends on the upper half of the Cooking grip, with no sideways movement). Wait about 5 seconds into the step
  (`PauseAsync(5000)`) before the drag, so it happens while the narration says "Let's drag Gardening above Cooking".
- Visible at the end: Gardening, Vegetables (indented), Cooking, Baking, Barbecue (both indented), Herbs. Vegetables moved
  with Gardening.
- Check before the next clip: `Page.Locator("ol#menu > li.menu-item").EvaluateAllAsync("els => els.map(e => e.dataset.depth)")`
  returns 0, 1, 0, 1, 1, 0. If a row landed in the wrong place, restore S3 and take it again.

**`org-2`**
- Actions: `PauseAsync(8200)`, so the drag starts as the narration says "Let's drag Herbs below Vegetables"; then `await scene.DragToAsync(Row(scene, "Herbs").Locator(".menu-item-title"), Row(scene, "Cooking").Locator(".menu-item-title"), 70, -14);`
  (the pointer ends on the upper half of the Cooking grip, 70 pixels to the right, so Herbs lands between Vegetables and
  Cooking and one level deeper).
- Visible at the end: Gardening, Vegetables (indented), **Herbs (indented like Vegetables)**, Cooking, Baking, Barbecue.
- Check: the depths are 0, 1, 1, 0, 1, 1, and the hidden field `input[name='TaxonomyPart.Hierarchy']` holds a value. If Herbs
  landed above Vegetables, or is not indented, or is indented under Vegetables, restore S3 and take both drags again.

**`org-3`**
- Actions: `PauseAsync(7000)`, so the click lands as the narration says "Select Save Draft"; `ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Save']").First)` (Save Draft).
  Scroll to the action bar first if the button is out of view (before the pause, not after it).
- Visible at the end: the editor reloaded with the rows in the order of `org-2`. The message **Your Taxonomy draft has been saved.**
  is a toast that shows for 5 seconds after the reload; the narration does not mention it.
- Labels: Save Draft `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:7`; message `OrchardCore.Contents/Controllers/AdminController.cs:539`;
  the warning before leaving, which is not recorded, `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:58`.

Take snapshot **S4** after this clip.

## Chapter 06, `06-publish`: publish the taxonomy

**Starts on:** the Topics editor without a `returnUrl`. **Site state:** S4.

**`pub-1`**
- Actions: `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Manage Content" }).First)`; wait until the
  Topics row is visible; then highlight the group of badges with
  `HighlightAsync(scene.Page.Locator("span.badge", new() { HasText = "Draft" }).First.Locator("xpath=.."))` (the parent of the Draft badge
  holds the Published badge too; only Topics has a draft, so the first Draft badge is on its row). If that parent is much wider than
  the two badges, highlight the Draft badge itself.
- Visible at the end: the Content Items list with **Topics** at the top (last modified) and two badges, **Published** and
  **Draft**.
- Labels: Manage Content `OrchardCore.Contents/Views/Admin/Edit.cshtml:40` (breadcrumb); Published, Draft
  `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`, `:19`. The Topics row is the most recently modified item, so it
  is the first row; if it is not in view, scroll the list to it.

**`pub-2`**
- Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First)`;
  scroll to the list of terms.
- Visible at the end: the editor with the terms in the saved order: Gardening (Vegetables, Herbs), Cooking (Baking, Barbecue).

**`pub-3`**
- Actions: click Publish at once, with no pause before it, so the toast shows while the narration reads it:
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Publish']").First)` (the editor was opened from the list, so
  Publish returns to it); wait until the Topics row is visible.
- Visible while the narration reads the message (about 1 to 4 seconds into the step): the toast **Your Taxonomy has been published.**
  over the Content Items list. The toast hides itself 5 seconds after the list loads. Visible at the end: **Topics** with the
  **Published** badge only (no Draft badge).
- Labels: Publish `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` (an editor opened with a `returnUrl`); message
  `OrchardCore.Contents/Controllers/AdminController.cs:967`.

## Chapter 99, closing card

`outro`: the card of chapter 99 (made by `assemble.py`; nothing to record).

## Checks for private data

- The site is new, with the Blog recipe and the fake data of this plan only: `admin`, `admin@example.com`, Topics and its
  terms, and the recipe's own sample content. No mask script (`VIDEO_INIT_SCRIPT`) is needed.
- The password is never typed on camera: the clips start from `state.json`. Do not commit `state.json` or a copy of
  `App_Data`; the snapshots stay outside the repository.
- Look at every contact sheet (`review.py`): no error page, no leave-page dialog, no empty list where terms are expected.
- Before publishing, OCR the video at one frame per second and at every scene change, and search for anything that is not
  the fake data above (no real names, other email addresses, host names other than `localhost`, or tokens).
