# Recording plan: content-types-settings

How to record this video without deciding anything. Every label below was read from the Orchard Core source
(`OrchardCMS/OrchardCore` at commit 92db4a83); the file and line after a label tell where. File references start
with an abbreviation of a folder of that checkout: CT is `src/OrchardCore.Modules/OrchardCore.ContentTypes`, CO is
`src/OrchardCore.Modules/OrchardCore.Contents`, RO is `src/OrchardCore.Modules/OrchardCore.Roles`, FL is
`src/OrchardCore.Modules/OrchardCore.Flows`, TI is `src/OrchardCore.Modules/OrchardCore.Title`, and CORE is
`src/OrchardCore`. In this plan a selector stands for `scene.Page.Locator(...)`. Text selectors use the Playwright
forms `:has-text('...')` (contains) and `:text-is('...')` (exact).

The video is an admin video: nothing in the video shows code, a command line or a configuration file. This plan is for
the person recording, so it may name selectors, scene calls and routes.

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data | An empty `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Setup | Auto Setup, so no setup form is recorded. Tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Environment | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` (the names and meaning are in `src/docs/reference/modules/AutoSetup/README.md`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls out |
| Features | Exactly the features the Blog recipe enables (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, step `feature`). It includes Content Types, Contents, Roles, Flows, Widgets, Templates and Title. Do not enable anything else; in particular do not enable `OrchardCore.Apis.GraphQL`, which would add a Hide checkbox to the Edit Content Type page that the narration does not mention |
| Admin settings | Left as Auto Setup creates them (no "Display New menu", default list layout with Buttons) |
| Theme | The recipe's admin theme `TheAdmin` and site theme `TheBlogTheme`, light color scheme |
| External services | None. There is no stand-in to run |

Sign in once outside the capture with `Recorder setup` (user `admin`, password from `VIDEO_PASSWORD`), which writes
`state.json` next to the video. `state.json` is git-ignored. This video is not about the Setup screen, so Auto Setup
and a signed-in session are right.

## 2. The seed data (create it once, outside the capture)

All names are invented. Use the browser or `PrepareAsync`; either way nothing of this is recorded.

### 2.1 The Event content type

1. Open `/Admin/ContentTypes/List`. Click **Create new type** (`CT/Views/Admin/List.cshtml:20`).
2. Display Name: `Event`. Technical Name: it fills in by itself; it must read `Event`. Click **Create**
   (`CT/Views/Admin/Create.cshtml:14,22,30`). The page "Add Parts" opens.
3. Tick the part whose description reads "Provides a Title for your content item." (`TI/Migrations.cs:33`)
   and click **Add Selected Parts** (`CT/Views/Admin/AddPartsTo.cshtml:47`). If the Edit Content Type page does not
   open by itself, open `/Admin/ContentTypes/Edit/Event`.
4. In **Description** type `Open days and community events at the library.` and click **Save**
   (`CT/Views/ContentTypeSettings.Edit.cshtml:4`, `CT/Views/Admin/Edit.cshtml:215`). Do not touch any other field.

A type created here starts with Creatable, Listable, Draftable, Versionable and Securable all checked, with Stereotype
and Category empty, and with Thumbnail holding `~/OrchardCore.ContentTypes/placeholder.png`
(`CT/Services/ContentDefinitionService.cs:85-91`, `CT/Startup.cs:38-48`).
Open the Edit Content Type page of Event once and check that the five boxes are checked. The clips depend on it.

### 2.2 Three published Event items

For each title, open `/Admin/Contents/ContentTypes/Event/Create`, type the title in **Title**
(`TI/Views/TitlePart.Edit.cshtml:12`) and click **Publish**
(`CO/Views/Content.PublishButton.cshtml:7`). Create them in this order, so
the list shows the newest first:

1. `Library Book Sale`
2. `Community Cleanup Day`
3. `Spring Open House`

### 2.3 The Blocks editor on Page

Open `/Admin/ContentTypes/Page/ContentParts/FlowPart/Edit` (route: `CT/Controllers/AdminController.cs:966`). In
**What type of editor should be used?** choose **Blocks** (`CT/Views/Admin/EditTypePart.cshtml:70`,
`FL/Views/FlowPart-Blocks.Option.cshtml:4`) and click **Save**
(`CT/Views/Admin/EditTypePart.cshtml:111`).

### 2.4 Check the starting state

- `/Admin/Contents/ContentItems` lists the three Event items first, ordered by the newest, among the items of the
  Blog recipe. If the Events are not on the first page, sort with **Sort** then **Recently created**.
- The **New** button (`#new-dropdown`) opens a list that includes Event.
- `/Admin/Roles/Edit/Editor`: type `Event` in **Search permissions**. Only the group
  **Event Content Type - Event** may remain (`RO/Views/Admin/_RolePermissions.cshtml:49,57`). If another group also
  matches, write down which one and report it (see OPEN-QUESTIONS.md) before recording the Securable clip.
- `/Admin/ContentTypes/Edit/Paragraph` shows Stereotype `Widget`, and empty Category and Thumbnail
  (`blog.recipe.json`, type `Paragraph`).

### 2.5 The baseline snapshot

Stop the site. Copy its `App_Data` folder to a folder named `baseline`, and keep the `state.json` with it. Start the
site again. This is the starting state of every clip.

## 3. Clip order and snapshots

| Clip | Starts from | Changes the site? |
|---|---|---|
| `02-visibility` | baseline | Yes: clears Creatable and Listable on Event |
| `03-drafts` | baseline | Yes: saves a draft of one Event and clears Draftable on Event |
| `04-securable` | baseline | Yes: clears Securable on Event |
| `05-pickers` | baseline | Yes: sets Category and Thumbnail on Paragraph. The Page form is never saved |

The four clips do not depend on each other, and none may start from the state another one left. Before every take,
including a second take of the same clip: stop the site, restore the `baseline` copy of `App_Data`, start the site,
and wait for `/Admin` to load. Record in this order: 02, 03, 04, 05. The chapter cards, the opening card and the
closing card need no recording.

## 4. The clips

Every clip uses `NewSceneAsync`, opens its first page with `scene.OpenAsync(...)` before `StartCaptureAsync()`, and
navigates later only with `NavigateAsync`, `ClickAndWaitForNavigationAsync` or `WaitForNavigationAsync`. Expand
nothing in the admin menu: the clips go to pages by address, and the narration only names the menu path. Keep the
actions of a step shorter than its narration.

Selectors used in several clips (`Edit Content Type` is the page at `/Admin/ContentTypes/Edit/<name>`):

| Name | Selector | Source |
|---|---|---|
| Option label | `label.form-check-label:text-is('Creatable')` (same with Listable, Draftable, Versionable, Securable) | `CT/Views/ContentTypeSettings.Edit.cshtml:17,30,43,56,69` |
| Text setting | `.ocat-limited-wrapper:has(label:text-is('Description')) input` (same with Technical Name, Display Name, Stereotype, Category, Thumbnail) | `CT/Views/ContentTypeSettings.Edit.cshtml:4,77,91,105`, `CT/Views/ContentType.Edit.cshtml:4,11` |
| Save a type | `button.btn.save[name='submit.Save'][value='Save']` | `CT/Views/Admin/Edit.cshtml:215` |
| Type row buttons | `#btn-create-Event`, `a[role='btn-list-Event']`, `a[role='btn-edit-Event']` | `CT/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19,22,24` |
| New button | `#new-dropdown` | `CO/Views/ContentsAdminListCreate.cshtml:19` |
| Content Type filter | `.contenttypes-selectpicker button.dropdown-toggle` (check it with `Recorder probe`) | `CO/Views/ContentsAdminListFilters.cshtml:8` |
| Publish, Save Draft | `button.publish`, `button.draft` | `CO/Views/Content.PublishButton.cshtml:7`, `CO/Views/Content.SaveDraftButton.cshtml:7` |
| Permission search | `#search-box` | `RO/Views/Admin/_RolePermissions.cshtml:49` |

After every **Save** of a type, the Content Types page shows the notification "Content type updated successfully."
(`CT/Controllers/AdminController.cs:225`). Wait for it with `scene.Page.Locator(".message-success, .alert-success")`
or the notification area of the admin theme before ending the step; do not click anything else until it shows.

### 4.1 Chapter 1, slides (`01-options`)

Slides only; nothing is recorded. `slides.py` draws them.

- `options-1`: three cards, "Design, Content Definition", "Edit" and "All five options on".
- `options-2`: four cards, Creatable, Listable, Draftable and Versionable.
- `options-3`: four cards, Securable, Stereotype, Category and Thumbnail.

### 4.2 Chapter 2, `02-visibility` (Creatable and Listable)

Starts on `/Admin/Contents/ContentItems` (title "Manage Content", `CO/Views/Admin/List.cshtml:17`), signed in, from the
baseline. Start the capture there.

- `visibility-1`
  - Actions: highlight the first Event row. Click `#new-dropdown` (label "New"); pause 1200 ms with the list open and
    highlight the entry `ul.dropdown-menu a.dropdown-item:has-text('Event')`; press Escape. Click the Content Type
    filter button (its text reads "Content Type"; the list header reads "Filter by content type"); pause 1200 ms.
    Leave the filter open.
  - On screen when the narration ends: the open Content Type list with Event in it, and the Event items behind it.
  - Labels: `CO/Views/ContentsAdminListCreate.cshtml:20`, `CO/Views/ContentsAdminListFilters.cshtml:8`.
- `visibility-2`
  - Actions: press Escape to close the filter. `NavigateAsync("/Admin/ContentTypes/List")`. Highlight
    `#btn-create-Event` (label "Create New Event"); if the Event row is below the visible area, scroll it into view first.
    `ClickAndWaitForNavigationAsync` on `a[role='btn-edit-Event']` (label "Edit").
  - On screen when the narration ends: the page "Edit Content Type - Event" (`CT/Views/Admin/Edit.cshtml:63`), with the
    top of the form visible.
  - Labels: `CT/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19,24`.
- `visibility-3`
  - Actions: highlight the Technical Name input (it is read-only), then the Display Name input, then the Description
    input (its value is "Open days and community events at the library."), about two seconds each. Do not type.
    Clear highlights.
  - On screen when the narration ends: the Description field highlighted, the checkboxes below it visible. If the
    five checkboxes are not all visible without scrolling at 1536x864, scroll inside this step with
    `ScrollAsync(0, 120)` after clearing the highlights.
  - Labels: `CT/Views/ContentType.Edit.cshtml:4,11`, `CT/Views/ContentTypeSettings.Edit.cshtml:4`. The Templates module
    puts a dropdown button labeled "Edit templates" above Technical Name for an administrator
    (`src/OrchardCore.Modules/OrchardCore.Templates/Views/TemplateSettings.cshtml:5`, placed by
    `src/OrchardCore.Modules/OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:59`). Leave it alone and do not open it; the narration says the form
    "begins with" Technical Name, which is the first field of the form.
- `visibility-4`
  - Actions: highlight the hint after the Creatable label ("Determines if an instance of this content type can be
    created through the UI."), then the hint after the Listable label ("Determines if an instance of this content
    type can be listed through the UI."), one second each. The hint is a text span that follows the label on the same
    line and starts with a dash (`CT/Views/ContentTypeSettings.Edit.cshtml:17-18`, style `.hint.dashed` in
    `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_forms.scss:4-14`); it may wrap to a second line on a
    narrow screen. Clear highlights. Click the Creatable label, then the Listable label. Both
    boxes are now empty.
  - On screen when the narration ends: Creatable and Listable unchecked, Draftable, Versionable and Securable still
    checked.
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:17,18,30,31`.
- `visibility-5`
  - Actions: click Save (`ClickAndWaitForNavigationAsync`). Wait for the notification.
  - On screen when the narration ends: the Content Types page with the notification "Content type updated
    successfully.", and the Event row with the buttons List Items, Edit and Delete only. Highlight the row's
    button area at the end.
  - Labels: `CT/Views/Admin/Edit.cshtml:215`, `CT/Controllers/AdminController.cs:225`,
    `CT/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:17-24`.
- `visibility-6`
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems")`. Click `#new-dropdown`, pause 1200 ms (no Event in the
    list), press Escape. Click the Content Type filter button, pause 1200 ms (no Event in the list), press Escape.
  - On screen when the narration ends: Manage Content with the New and Content Type lists closed, and no Event
    items in the list.
  - Labels: as in `visibility-1`. Source of the behavior: `CO/Views/ContentsAdminListCreate.cshtml` (creatable types
    only), `CO/Controllers/AdminController.cs:901` (listable types only in the filter), and
    `CO/Services/DefaultContentsAdminListFilterProvider.cs:182` (listable types only in the unfiltered list), all
    under `src/OrchardCore.Modules/OrchardCore.Contents/`.
- `visibility-7`
  - Actions: `NavigateAsync("/Admin/ContentTypes/List")`. `ClickAndWaitForNavigationAsync` on
    `a[role='btn-list-Event']` (label "List Items").
  - On screen when the narration ends: Manage Content filtered to Event (breadcrumb and title "Event"), listing
    Spring Open House, Community Cleanup Day and Library Book Sale.
  - Labels: `CT/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:22`. Source of the behavior: the comment "We display
    a specific type even if it's not listable" in `CO/Services/DefaultContentsAdminListFilterProvider.cs:145`.

### 4.3 Chapter 3, `03-drafts` (Draftable and Versionable)

Starts on `/Admin/Contents/ContentItems/Event` (Manage Content filtered to Event), from the baseline. Before the
capture, click the title link `a:text-is('Spring Open House')` and wait for the editor to load. The address carries a
return address, which makes **Save Draft** come back to this list. Start the capture on the editor.

- `drafts-1`
  - Actions: highlight `button.publish` (label "Publish"), then `button.draft` (label "Save Draft"), two seconds
    each. Clear highlights.
  - On screen when the narration ends: the editor of "Spring Open House" with the action bar showing Publish,
    Unpublish, Save Draft and Delete, then Preview (`src/OrchardCore.Modules/OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`)
    and Cancel (the page has a return address), and Save Draft highlighted. Publish and Save Draft are split buttons
    with a small arrow beside them because of the return address (`CO/Views/Content.PublishButton.cshtml:11-26`).
  - Labels: `CO/Views/Content.PublishButton.cshtml:7`,
    `CO/Views/Content.SaveDraftButton.cshtml:7`; the button is shown only for a Draftable type
    (`CO/Drivers/ContentsDriver.cs:116`).
- `drafts-2`
  - Actions: `ClickAsync` on the Title input (`input.content-caption-text`), press End with
    `scene.Page.Keyboard.PressAsync("End")`, then type ` Schedule` (a space first) with
    `scene.Page.Locator("input.content-caption-text").PressSequentiallyAsync(" Schedule", new() { Delay = 55 })`. Do not
    use `TypeAsync` here: it clicks the middle of the field and types where the click lands. The Title then reads
    "Spring Open House Schedule". Click `button.draft` with `ClickAndWaitForNavigationAsync`. Wait for the
    notification "Your Event draft has been saved." (`CO/Controllers/AdminController.cs:539`, the Edit action; line 439
    is the same text for a new item).
  - On screen when the narration ends: Manage Content filtered to Event; the row "Spring Open House Schedule" shows
    the badges Published and Draft. Highlight both badges.
  - Labels: `CO/Views/ContentsTags_SummaryAdmin.cshtml:12,19`.
- `drafts-3`
  - Actions: none that change the page. Keep the highlight on the two badges; add a third highlight on the row's
    title, then clear them at the end.
  - On screen when the narration ends: the same list with the row and its two badges.
  - Source of the behavior: `CORE/OrchardCore.ContentManagement/DefaultContentManager.cs:160-180`
    (a Versionable item gets a version of its own; the published one stays) and the comment "meaning we use only one
    version" at line 167 for a type that is not Versionable.
- `drafts-4`
  - Actions: `NavigateAsync("/Admin/ContentTypes/Edit/Event")` (the page is opened by its address, so the step does
    not pass through the list). Highlight the Draftable label for one second, then clear highlights. Click the
    Draftable label. Click Save with `ClickAndWaitForNavigationAsync`, and wait for the notification.
  - On screen when the narration ends: the Content Types page with the notification "Content type updated
    successfully." (if the actions are not done when the narration ends, the Edit page with Draftable unchecked is
    acceptable; the notification must be on screen before step `drafts-5` begins).
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:43`.
- `drafts-5`
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems/Event")`. `ClickAndWaitForNavigationAsync` on the title
    link `a:text-is('Community Cleanup Day')`. Highlight `button.publish`.
  - On screen when the narration ends: the editor of "Community Cleanup Day" with Publish, Unpublish, Delete, Preview
    and Cancel in the action bar and no Save Draft button.
  - Labels: `CO/Views/Content.PublishButton.cshtml:7`.

### 4.4 Chapter 4, `04-securable` (Securable)

Starts on `/Admin/Roles`, from the baseline. Start the capture there. The page title is "Roles"
(`RO/Views/Admin/Index.cshtml:15`).

- `securable-1`
  - Actions: highlight the row of the role Editor. `ClickAndWaitForNavigationAsync` on
    `a.edit[href*='/Roles/Edit/Editor']` (label "Edit").
  - On screen when the narration ends: the page "Edit 'Editor' Role" with the **Search permissions** box and the
    permission groups below it.
  - Labels: `RO/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`, `RO/Views/Admin/Edit.cshtml:6`.
- `securable-2`
  - Actions: `TypeAsync(#search-box, "Event")`. Wait until the groups that do not match have disappeared (the script
    gives a hidden group the class `d-none`, `src/OrchardCore.Modules/OrchardCore.Resources/Assets/js/list-management.js:323-331`). Highlight
    the group title `fieldset.permissions-list:not(.d-none) legend`; the plain selector also matches the hidden groups.
    The search compares the typed text with the description of each permission, ignoring case, not with the group
    title.
  - On screen when the narration ends: one group titled "Event Content Type - Event" with its table of permissions
    (descriptions such as "Publish or unpublish Event for others", "Edit Event", "Delete Event", "View Event by
    others", "Preview Event by others", "Clone Event by others", "List Event content items") and the Allow and
    Effective columns.
  - Labels: `RO/Views/Admin/_RolePermissions.cshtml:49,57,68`; the group title is built from the category template
    "{1} Content Type - {0}" and the descriptions from the templates in
    `CORE/OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:13-26,155`. Do not tick anything and do not
    save the role.
- `securable-3`
  - Actions: `NavigateAsync("/Admin/ContentTypes/Edit/Event")` (the page is opened by its address, so the step does
    not pass through the list). Highlight the Securable label for one second, then clear highlights. Click the
    Securable label. Click Save with `ClickAndWaitForNavigationAsync`, and wait for the notification.
  - On screen when the narration ends: the Content Types page with the notification, or the Edit page with Securable
    unchecked if the save is still loading (the notification must show before `securable-4` begins).
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:69`.
- `securable-4`
  - Actions: `NavigateAsync("/Admin/Roles/Edit/Editor")`. `TypeAsync(#search-box, "Event")`. Wait one second.
  - On screen when the narration ends: the Edit 'Editor' Role page with "Event" in the search box and no permission
    group below it.

### 4.5 Chapter 5, `05-pickers` (Stereotype, category and thumbnail)

Starts on `/Admin/ContentTypes/Edit/Paragraph` ("Edit Content Type - Paragraph"), from the baseline. Start the
capture there.

- `pickers-1`
  - Actions: highlight the Stereotype input (its value is "Widget"), then the Category and Thumbnail inputs (both
    empty) for one second each. Clear highlights.
  - On screen when the narration ends: the Stereotype box showing Widget, highlighted.
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:77`; the value comes from `blog.recipe.json`, type `Paragraph`.
- `pickers-2`
  - Actions: highlight the hint under the Stereotype box ("(Optional) The stereotype of the content type. e.g.,
    Widget, MenuItem, ...") for the whole step. Do not click the Stereotype box, do not type in it and do not change
    its value.
  - On screen when the narration ends: the Stereotype box with its hint highlighted.
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:77,86`; the rule about letters and digits, and the error
    "The stereotype should be alphanumeric." are in `CT/Editors/ContentTypeSettingsDisplayDriver.cs:64-66,131-143`.
- `pickers-3`
  - Actions: `TypeAsync(Category input, "Text")`, then `TypeAsync(Thumbnail input, "~/OrchardCore.ContentTypes/placeholder.png")`,
    then press Tab. Click Save and wait for the notification.
  - On screen when the narration ends: the Content Types page with the notification "Content type updated
    successfully." (or the Edit page with both values typed, if the save is still loading).
  - Labels: `CT/Views/ContentTypeSettings.Edit.cshtml:91,105`. The text boxes are labeled **Category** and **Thumbnail**,
    not "Thumbnail Path".
- `pickers-4`
  - Actions: `NavigateAsync("/Admin/Contents/ContentTypes/Page/Create")`. Wait for the editor to load. Click
    `button[title='Add Widget']` (a button with a plus icon). Wait for `#contentTypePickerModal` to be visible.
  - On screen when the narration ends: the modal titled "Select Widget" open, with the sidebar listing All and
    Text, and the grid of widget cards.
  - Labels: `FL/Views/FlowPart-Blocks.Edit.cshtml:77,120`.
- `pickers-5`
  - Actions: click `.content-type-picker-categories a.nav-link:text-is('Text')`. Highlight the one card that
    remains and its **Add** button. Do not click Add.
  - On screen when the narration ends: the sidebar with Text active, one card "Paragraph" with the placeholder
    thumbnail, and its Add button.
  - Labels: `FL/Views/ContentTypePicker.cshtml:43`; categories and the "All"
    entry come from `FL/Assets/ts/content-type-picker.ts:85-124`. After the take, close the modal and leave the page
    without saving; no Page is created.

### 4.6 Cards

`intro` and `outro` are the opening and closing cards; they have no recording.

## 5. Checks for private data

- The only names in the video are the invented event titles and the user name `admin`. The video never shows the
  email `admin@example.com` or the password; if the user menu opens by accident, record the clip again.
- OCR every contact sheet and the final video at one frame per second and at every scene change. Search for any email
  address other than `@example.com`, any name that is not in this plan, and the password.
- The Roles pages of this video list roles and permissions, not people. No user names appear on them.
- Nothing in this video needs a mask script. Do not copy `App_Data` or `state.json` into the repository.
