# Recording plan: Build a Company Website With the Agency Theme

This plan is for the person who records the video: it needs no decision beyond what is written here. Narrate first
(`scripts/narrate.py`), because the recorder holds every step for the length of its narration.

Conventions of this file:

- A selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. `Filter(HasText = ...)`
  stands for `.Filter(new() { HasText = ... })`.
- Every label comes from the Orchard Core source at commit `92db4a83`, given as `file:line`. A path that starts with
  `OrchardCore.` is under `src/OrchardCore.Modules/`, one that starts with `TheAgencyTheme/` is under
  `src/OrchardCore.Themes/`, and `docs/` is `src/docs/`. The selectors come from the markup in those files: check each
  one with `Recorder probe` before the first take, because this plan could not run them.
- The video is an admin video: nothing in the narration or the slides is code. The code in this file (selectors, file
  names, commands) is for the recorder only.
- Fake data only. The site, the people and the texts below are invented; the only e-mail address is
  `admin@example.com`. The password is chosen outside git and passed in `VIDEO_PASSWORD`; it is written
  `<password>` here and is never typed on camera.

## 1. The site it needs

### The host

Run `src/OrchardCore.Cms.Web` of an Orchard Core checkout at commit `92db4a83` or a later `main` that still has
the labels listed below, on `http://localhost:5010`, with an empty `App_Data` folder outside both repositories.
Auto Setup is enabled by default in that project (`docs/reference/modules/AutoSetup/README.md`, "Enabling Auto Setup
Feature"), so no Setup screen is recorded.

```bash
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9      # a dead address: no outbound call is possible
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Alder Creek Studio"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Agency
dotnet run --project src/OrchardCore.Cms.Web -c Release
```

- **Recipe**: `Agency` (`TheAgencyTheme/Recipes/agency.recipe.json:2-4`, a setup recipe:
  `issetuprecipe` at line 8). Its `themes` step makes TheAgencyTheme the site theme and TheAdmin the admin theme.
- **Tenant**: `Default` only. **Site name**: `Alder Creek Studio` (an invented studio). **Time zone**:
  `America/Los_Angeles`. **User**: `admin`, e-mail `admin@example.com`.
- **Recorder environment**: `VIDEO_ROOT=<repo>/videos/build-a-company-website`, `VIDEO_BASE_URL=http://localhost:5010`,
  `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`, and `MSYS_NO_PATHCONV=1` in Git Bash. The site is made from
  scratch, so no `VIDEO_INIT_SCRIPT` is needed. Run `Recorder setup` once the site is running (it writes `state.json`,
  which is git-ignored).
- **External services**: none for the site. No mail server, no SMS, no payment provider. The form of this video is never
  submitted, so nothing needs a stand-in. The page layout of the theme links two style sheets of Google Fonts
  (`TheAgencyTheme/Views/Layout.liquid:9-10`); the browser of the recorder loads them, not the site, so the dead proxy above
  does not stop them. Record with network access, or accept the fallback fonts: see `OPEN-QUESTIONS.md`.

### Features

The Agency recipe enables everything the video needs, so no feature is enabled before recording. Flows, Widgets and
Menu are in the recipe's `feature` step (`TheAgencyTheme/Recipes/agency.recipe.json`, the ids "OrchardCore.Flows", "OrchardCore.Widgets",
"OrchardCore.Menu"). **The Forms feature must be disabled** when the recording starts: it is not in the recipe, and
clip `04-enable` enables it on camera.

### Seed data, outside the capture

The recipe's sample content is generic, but it holds company logos and made-up people. Make these changes once, after
the site is running and signed in as `admin`, and before the first clip. Nothing here is recorded.

1. Open Content, Content Items and click Edit on the row `Alder Creek Studio` (type `LandingPage`).
2. In the Team block, change the Title of the three cards: the first to `Jamie Chen`, the second to `Morgan Ellis`,
   the third to `Riley Okafor`. Leave the Occupations as they are (`Lead Designer`, `Lead Marketer`,
   `Lead Developer`): the first one is changed on camera in `edit-3`.
3. In the Clients block, delete the four Client cards that the recipe creates (their logos are those of real
   companies) with their trash buttons (`button.widget-delete`,
   `OrchardCore.Flows/Views/ContentCard-BagPart.Edit.cshtml:37`), confirming the dialog. The
   Clients block stays empty, and the landing page does not draw the Clients row when it has no client
   (`TheAgencyTheme/Recipes/Snippets/landingpage.liquid:150`, the `Clients.ContentItems.size > 0` test).
4. Click Publish.
5. Open `/Admin/ContentParts/TeamMember/Fields/Twitter/Edit` (the route is
   `OrchardCore.ContentTypes/Controllers/AdminController.cs:790`; Design, Content Definition, Content Parts leads to the
   same pages). Replace the text of the field "Hint" (`OrchardCore.ContentFields/Views/TextFieldSettings.Edit.cshtml:26`)
   with `Twitter handle` and click "Save" (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml`). The hint that the
   recipe sets names a real person (`TheAgencyTheme/Recipes/agency.recipe.json:697`), and every card of the Team block
   shows its hint under the field Twitter (`OrchardCore.ContentFields/Views/TextField.Edit.cshtml:29`,
   `TheAdmin/Views/Hint.Admin.cshtml`).
6. Open `/` and check that the Team section shows the three invented names and that no company logo appears.
7. Open Tools, Features, search for `Forms`, and check that the row of the Forms feature shows an Enable button (the
   feature is off).
8. Check that `/start-a-project` returns the Not Found page: the page of `04-form` must not exist yet.

The photos of the Team and the images of the Portfolio and About sections are the theme's own sample images, copied
into the media library by the recipe (`TheAgencyTheme/Recipes/agency.recipe.json`, step `media`): see `OPEN-QUESTIONS.md`.

Then stop the site and copy its `App_Data` folder aside, together with `state.json`. This copy is **snapshot A**.

## 2. Order of the clips, and the state each one leaves

Record the clips in the order they play. Each clip starts from the state the previous one left.

| Clip | Needs | Changes the site | Restore before a new take |
|---|---|---|---|
| `02-tour` | snapshot A | nothing | snapshot A (already the state) |
| `03-edit` | snapshot A | adds the service `Brand Identity` to the Services part, moves it first, changes the first Team Occupation to `Design Director`, publishes the landing page | snapshot A |
| `04-enable` | state after `03-edit` | enables the Forms feature | snapshot B (taken after `03-edit`) |
| `04-form` | Forms enabled, no page at `/start-a-project` | creates and publishes the page `Start a project` with its form | snapshot C (taken after `04-enable`) |
| `05-menu` | the page `Start a project` exists | adds a Content Menu Item to Main Menu and publishes the menu | snapshot D (taken after `04-form`) |
| `06-finish` | the menu item and the page exist | nothing | snapshot E (taken after `05-menu`) |

- The slide clips `01-why` and `07-recap` are drawn by `slides.py` and need no recording.
- Take a snapshot (stop the site, copy `App_Data` and `state.json`, start the site again) after every clip that
  changes the site, and name it by the clip it follows. To record one clip again, restore the snapshot taken just
  before it.
- `02-tour` must be recorded before `03-edit`: after it, the Services section shows `Brand Identity`, which the tour
  does not mention.
- `06-finish` and `02-tour` only read the site, so they can be recorded again from any later snapshot.
- `04-enable` and `04-form` are the two clips of one chapter, and the second needs the first: the Page editor lists the
  widgets of the Forms feature only after it is enabled.

## 3. The clips

### `01-why` (slides, drawn by `slides.py`)

- `why-1`: three cards. The Setup screen's Recipe list and the Agency choice, what Finish Setup creates (the Agency
  theme and the landing page as home page), and that the Main Menu links to Services, Portfolio, About, Team and Contact.
- `why-2`: three cards. One content item holding the items of its Bag parts, the five sections that are Bag parts of the
  Flows module, and one content type in each Bag part of this recipe.
- `why-3`: four cards. What each Bag part holds: Services, Portfolio, About, and Team and Clients, with the fields of
  their items.
- `why-4`: three cards. The three changes made in the video: edit the landing page, add a contact form, extend the menu.

### `02-tour` (front end, read only)

Starts on `/` (not recorded), scrolled to the top. The recorder is signed in, and the first frame must show no toolbar of
any kind. The viewport is 1536 x 864, so the menu bar of the theme is expanded (`navbar-expand-lg`, `TheAgencyTheme/Views/Layout.liquid:21`). Page anchors
change the scroll position only, so they do not stop the capture.

- `tour-1`: highlight, one after the other (clear each before the next), `header.masthead .masthead-heading` (the site
  name in capitals, `TheAgencyTheme/Recipes/Snippets/landingpage.liquid:6`), the link `a:text-is("Tell Me More")` (`landingpage.liquid:7`, same folder), and
  `#mainNav`. Do not click. Visible at the end of the narration: the top of the page, with the masthead, the button and
  the menu bar.
- `tour-2`: click the menu links `#mainNav a.nav-link` with the text `Services`, then `Portfolio`, then `About`, about
  a second and a half apart (`pauseAfter: 1500`); the theme scrolls smoothly (`TheAgencyTheme/Views/Layout.liquid:28`). Visible at the end:
  the About section with its timeline.
- `tour-3`: highlight the five links `#mainNav a.nav-link` in turn (Services, Portfolio, About, Team, Contact; the menu of the
  recipe has no entry for Clients), clear the highlights, then click `#mainNav a.nav-link:text-is("Team")`, wait 1.5 seconds,
  then `#mainNav a.nav-link:text-is("Contact")`. Visible at the end: the Contact Us section (`#contact`), the Contact entry
  of the menu bar active.
- `tour-4`: the page stays on the Contact Us section that `tour-3` ended on. While the first sentence plays, highlight
  `#contact .section-heading` and then the form `#contactForm` (`landingpage.liquid:168-174`: the heading, the placeholders
  "Your Name *", "Your Email *", "Your Phone *", "Your Message *" and the button "Send Message" are written in the template, not
  stored as content); clear the highlights. Do not type in the form or click its button. In the last three seconds, click
  `a.navbar-brand` (the site name at the left of the menu bar, `Layout.liquid:23`), which scrolls to the top. Visible at
  the end: the top of the page again.

### `03-edit` (admin: the landing page)

Starts on `/Admin` (not recorded; the page shows "Welcome to Orchard Core",
`OrchardCore.Admin/Views/AdminDashboard.cshtml:2`), with the admin menu groups collapsed.
Every step that opens a page uses `ClickAndWaitForNavigationAsync`.

- `edit-1`: click the admin menu group "Content" (`OrchardCore.Contents/AdminMenu.cs:65`), then the link "Content
  Items" under it (`AdminMenu.cs:68`). On the list, highlight the two rows (`Alder Creek Studio` and `Main Menu`), clear the
  highlights, then click `a.edit` ("Edit", `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`) in the
  row `Locator("li, tr").Filter(HasText = "Alder Creek Studio")`. Visible at the end: the landing page editor, the
  Title field showing `Alder Creek Studio`.
- `edit-2`: for each of the five blocks in turn, call `ScrollIntoViewIfNeededAsync()` on the block's label, highlight
  it for about a second, and clear the highlight: `Locator(".content-part-wrapper .ocat-label").Filter(HasText =
  "Services")`, then `"Portfolio"`, `"About"`, `"Team"`, `"Clients"` (the labels of the Bag parts,
  `OrchardCore.Contents/Views/ContentPart-BagPart.Edit.cshtml:22-23`, from the part names in `TheAgencyTheme/Recipes/agency.recipe.json`, lines 216 to
  290). The page opens on the Title (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`) and the Permalink
  (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`): hold there for the first second. Then scroll back to
  the Services block and click, on the first card of that block, `button.widget-editor-btn-collapse`, wait a
  second, and click `button.widget-editor-btn-expand` (`OrchardCore.Flows/Views/ContentCard-BagPart.Edit.cshtml:30-31`). Visible at the end:
  the Services block with its first card open.
- `edit-3`: scroll to the first card of the Team block. Highlight its fields (Title, Occupation, Picture, Twitter,
  Facebook, LinkedIn). The hint under Twitter must read "Twitter handle" (seed step 5): if it reads "e.g. @" followed by a
  name, seed step 5 was not applied before snapshot A: apply it, take snapshot A again, and record the clip again. Then change the Occupation: the input whose label reads "Occupation"
  (`TheAgencyTheme/Recipes/agency.recipe.json:666`) in the first card of the Team block is
  `Locator(".content-part-wrapper-team .widget-editor").First.GetByLabel("Occupation")`. Call `FillAsync("")` on it
  (it empties the field at once), then `TypeAsync` the text `Design Director`. Visible at the end: the first Team card with `Design Director` as its Occupation.
- `edit-4`: first collapse the three cards of the Services block (click `button.widget-editor-btn-collapse` on each, one
  second apart), so that the block stays short for the drag of `edit-6`. Then highlight the button "Add Service",
  `Locator(".content-part-wrapper-services a.add-widget")` (`OrchardCore.Flows/Views/BagPart.Edit.cshtml:73`), and click
  it. A fourth card opens (it is added by a request, so wait for `Locator(".content-part-wrapper-services
  .widget-editor")` to count four). Click the Title input of that card (label "Title") and type `Brand Identity`.
  Visible at the end: the new card open, below the three collapsed ones, with the Title `Brand Identity`.
- `edit-5`: in the new card, click the description editor (the field labelled "HtmlBody", below the Title, with the hint
  "The body of the content item.": a text area that the script turns into a `.CodeMirror` editor,
  `OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml:10-13`; the Service type sets no display name for its Html body part, so
  the label is the part name without "Part",
  `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`)
  and type `Logos, color palettes and style guides for growing
  companies.`; then click the input labelled "Icon Class" (`TheAgencyTheme/Recipes/agency.recipe.json:587`; its hint names the Font Awesome
  classes) and type `fa-palette`. If typing in the code editor does not take, set the text with `FillAsync` on the
  hidden text area of the card and keep the rest. Highlight the Icon Class hint at the end. Visible: the description and
  `fa-palette` in the new card.
- `edit-6`: highlight, one after the other, the handle of the new card (`.widget-editor-handle`,
  `OrchardCore.Flows/Views/ContentCard-BagPart.Edit.cshtml:28`) and its trash button (`button.widget-delete`, line 37: an icon without text, its "Delete" only titles the confirmation window
  that it opens; do not click it); clear the highlights
  (they are fixed-position), then `DragToAsync(<handle of the new card>, <handle of the first card of the block>,
  offsetY: -8)`. Scroll first so that the whole Services block is in view. Visible at the end: `Brand Identity` as the
  first card of the Services block, the three other cards below it. If the card does not move, restore the
  snapshot and try again: see `OPEN-QUESTIONS.md`.
- `edit-7`: highlight the buttons "Publish" and "Save Draft" at the bottom of the editor
  (the editor was opened from the list with a return address, so both are split buttons:
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`, `Content.SaveDraftButton.cshtml:19`; "Unpublish" sits
  between them, do not mention it). Click "Publish" (`button[name="submit.Publish"]`, the first one) with
  `ClickAndWaitForNavigationAsync`. The Content Items list returns with the message "Your Landing Page has been
  published." (`OrchardCore.Contents/Controllers/AdminController.cs:967`, the edit path); keep it on screen for two seconds, then
  `NavigateAsync("/#services")`. Visible at the end: the Services section of the site, `Brand Identity` first with a
  palette icon and its description.

### `04-enable` (admin: Features)

Starts on `/Admin` (not recorded), with the admin menu groups collapsed.

- `enable-1`: click the admin menu group "Tools" (`OrchardCore.Features/AdminMenu.cs:26`), then "Features" (line 27), with
  `ClickAndWaitForNavigationAsync`. Click the search box `#search-box` (placeholder "Search",
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`) and type `Forms`. The list filters as you type. Highlight the
  row `Locator(".list-group-item, tr").Filter(HasText = "OrchardCore.Forms")` (the name "Forms" with its id
  `OrchardCore.Forms`, `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`). Visible at the end: the row of the Forms feature
  with its Enable button.
- `enable-2`: click the "Enable" link of that row (`a.btn:text-is("Enable")` inside it,
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) with `ClickAndWaitForNavigationAsync`. Visible at the end: the message
  "The feature Forms was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`) at the top of the page.
  The reloaded list is not filtered; do not search again.

### `04-form` (admin: a page with a form)

Starts on `/Admin/Contents/ContentItems` (not recorded), the Content Items list with its two rows. Forms is enabled.

- `form-1`: highlight `#new-dropdown` (the "New" button, `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-20`), click it,
  and click the dropdown item whose text is exactly "Page" (`a.dropdown-item:text-is("Page")`, line 25; the other item is
  "Landing Page") with `ClickAndWaitForNavigationAsync`. Visible at the end: the editor of a Page with an empty Title.
- `form-2`: no click. Highlight the plus button `button[title="Add Widget"]` (`OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`),
  the only way to add content to the page. Visible: the empty Page editor, the Title, the Permalink and the plus button.
- `form-3`: click the Title input of the page (label "Title") and type `Start a project`. Highlight the Permalink input
  (label "Permalink") and its hint "The url of the content item. Leave empty to auto-generate it."
  (`AutoroutePart.Edit.cshtml:46`), and leave it empty. Click `button[title="Add Widget"]`; the list shows the widget types
  by display name (`OrchardCore.Flows/Views/FlowPart.Edit.cshtml:84-94`): click `a.add-widget:text-is("Form")`. The card of the Form widget opens
  (added by a request: wait for `Locator(".widget-editor[data-content-type='Form']")`). Visible at the end: the Form card.
- `form-4`: inside that card (`var form = Locator(".widget-editor[data-content-type='Form']")`), click the input labelled
  "Title" (`form.GetByLabel("Title", new() { Exact = true }).First`) and type `Project inquiry`. Highlight the input
  labelled "Action" and its hint "The URL to submit the form to."
  (`OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml:6-9`). Leave Action empty. Visible: the Title and the Action
  field of the Form card.
- `form-5`: highlight the select labelled "Method" (it shows POST, line 14) and the check box labelled "Anti Forgery Token"
  (checked, line 40). Change nothing. Visible: both highlights.
- `form-6`: inside the Form card, click its own plus button (`form.Locator("button[title='Add Widget']").Last`, the one of
  the form's Flow part), then `form.Locator("a.add-widget:text-is('Input')")`. Scope every widget choice of this clip to `form`: the Page has a
  dropdown of its own with the same item names, and an unscoped locator would match both. Wait for
  `form.Locator(".widget-editor[data-content-type='Input']")`.
  In that Input card: type `name` into the input labelled "HTML Name"
  (`OrchardCore.Forms/Views/Items/FormInputElementPart.Fields.Edit.cshtml:5`); `SelectAsync` the select labelled
  "Label option" to `Standard` (`FormElementLabelPart.Fields.Edit.cshtml:10-14`); the input labelled "Label Text" appears
  (line 22): type `Your name`. Leave Type on Text. Visible at the end: the Input card with the three values.
- `form-7`: highlight the select "Label option" of that card with the caption `None, Standard, Screen Readers` (a closed
  native list cannot be shown). Visible: the highlight on the select.
- `form-8`: click the form's plus button again and choose "Input" (`form.Locator("a.add-widget:text-is('Input')")`). In the second Input card
  (`form.Locator(".widget-editor[data-content-type='Input']").Nth(1)`): type `email` into "HTML Name"; `SelectAsync` the
  select labelled "Type" to `email` (shown as "Email", `InputPart.Fields.Edit.cshtml:12`); `SelectAsync` "Label option" to
  `Standard`; type `Your email` into "Label Text". Visible at the end: the second Input card with its values.
- `form-9`: click the form's plus button and choose "Text Area" (`form.Locator("a.add-widget:text-is('Text Area')")`; the type's display name is made from `TextArea`,
  `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:12`). In its card: `message` into
  "HTML Name", "Label option" `Standard`, `Project details` into "Label Text". Click the plus button again and choose
  "Button" (`form.Locator("a.add-widget:text-is('Button')")`). In its card: `send` into "HTML Name" (it is required for a Button too,
  `OrchardCore.Forms/Drivers/FormInputElementPartDisplayDriver.cs:37`), `Send` into "Text" (`ButtonPart.Fields.Edit.cshtml:5`), and
  `SelectAsync` "Type" to `submit`. Visible at the end: the Button card with `send`, `Send` and Submit.
- `form-10`: scroll to the bottom of the editor, highlight "Publish", and click it (`button[name="submit.Publish"]`, first one)
  with `ClickAndWaitForNavigationAsync`. Visible at the end: the Content Items list with the message "Your Page has been
  published." (`OrchardCore.Contents/Controllers/AdminController.cs:473`, the create path), and the new row `Start a project`. If a card shows a validation message, an HTML Name
  is missing: restore snapshot C and record the clip again.

### `05-menu` (admin: the main menu)

Starts on `/Admin` (not recorded), with the admin menu groups collapsed.

- `menu-1`: click the group "Content", then "Menus" (`OrchardCore.Menu/AdminMenu.cs:34`), with
  `ClickAndWaitForNavigationAsync`. On the list, click "Edit" (`a.edit`) in the row `Main Menu`. The menu editor opens
  (`OrchardCore.Menu/Views/MenuPart.Edit.cshtml`). Highlight the list `#menu`. Visible at the end: the list of the
  five menu items, each row with its name, its address and the buttons Edit, Add and Delete
  (`OrchardCore.Menu/Views/MenuItem.Admin.cshtml`).
- `menu-2`: click the button "Add Menu Item" (`button:text-is("Add Menu Item")`, `OrchardCore.Menu/Views/MenuPart.Edit.cshtml:31`). The window "Available
  Menu Items" opens (`#modalMenuItems`, line 60) with three cards. Highlight the three titles in turn ("Link Menu Item",
  "Content Menu Item" and "Html Menu Item", from `LinkMenuItemPart.Thumbnail.cshtml`, `ContentMenuItemPart.Thumbnail.cshtml`
  and `HtmlMenuItemPart.Thumbnail.cshtml`, with the lines "Links to a custom url.", "Links to a Content url." and "Links to a
  custom url with HTML."), then click "Add" in the card of the Content Menu Item:
  `Locator("#modalMenuItems .card").Filter(HasText = "Content Menu Item").Locator("a.thumbnail-link-create")`
  (`OrchardCore.Menu/Views/MenuItem.Thumbnail.cshtml:16`) with `ClickAndWaitForNavigationAsync`, as the last action. Visible at the end: the editor of a
  Content Menu Item opening.
- `menu-3`: click the input labelled "Name" (`OrchardCore.Menu/Views/ContentMenuItemPart.Edit.cshtml:4`) and type `Start a project`. Click
  the picker of the field "Selected Content Item" (`.vue-multiselect input`, placeholder "Type to search",
  `OrchardCore.ContentFields/Services/ContentFieldsJSLocalizer.cs:40`), type `Start`, wait for the option
  `.multiselect__option` with the text `Start a project`, and click it. Visible at the end: the field showing the selected page.
- `menu-4`: highlight the check box "Check content permissions" and its hint
  (`OrchardCore.Menu/Views/ContentMenuItemPart.Edit.cshtml:14`). Do not check it. Visible at the end: the editor of the Content Menu Item with the name, the selected page and the check box unchecked.
- `menu-5`: first, in the first sentence, click the one button at the bottom of the editor, "Publish" (`button[name="submit.Publish"]`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`: the
  Add link carries no return address, a Content Menu Item is not draftable and a new item has no Id, so the editor shows
  no Save Draft and no Delete, `OrchardCore.Contents/Drivers/ContentsDriver.cs:110-120`; confirm it on the first take, see
  `OPEN-QUESTIONS.md`) with `ClickAndWaitForNavigationAsync`. The menu editor returns
  (`OrchardCore.Menu/Controllers/AdminController.cs:137`, which saves a draft of the menu) and lists six items, `Start a project` last.
  Then highlight the last row of `#menu` (`Start a project`) and clear it, and click "Publish" at the bottom of the
menu editor (`button[name="submit.Publish"]`, no split button here because the page has no return address,
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`) with `ClickAndWaitForNavigationAsync`. Visible at the end: the message "Your Menu has
  been published." (`OrchardCore.Contents/Controllers/AdminController.cs:967`; the editor returns to itself because the
  redirect of the Menu module, `OrchardCore.Menu/Controllers/AdminController.cs:139`, carries no return address, so
  `EditInternalAsync` redirects to the editor, `OrchardCore.Contents/Controllers/AdminController.cs:841`) and, below it, the menu list with the six items; highlight `#menu` last.

### `06-finish` (front end)

Starts on `/` (not recorded), scrolled to the top.

- `finish-1`: highlight the last entry of the menu bar (`#mainNav a.nav-link:text-is("Start a project")`), clear it, and click it with
  `ClickAndWaitForNavigationAsync`. The menu is cached for five minutes but is cleared when the menu is published
  (`OrchardCore.Alias/Handlers/AliasPartHandler.cs:61`), so the entry is there. Visible at the end: the page `/start-a-project`
  opening, with the heading START A PROJECT.
- `finish-2`: scroll down about 500 pixels (`ScrollAsync(0, 500)`) so that the form is in view, then highlight the labels `Your
  name`, `Your email` and `Project details`, and the `Send` button, one after the other. Do not click the button and do not
  type in the fields. Visible at the end: the form with three labels, three fields and the button.

### `07-recap` (slide)

- `recap-1`: three cards. The Bag parts that hold the items of the sections, the contact form as a Form widget holding the Input, Text Area and Button widgets,
  and the menu item as a Content Menu Item that links the Main Menu to the page.

## 4. Checks for private data

Before assembling, and again on the assembled video (OCR at one frame per second and at every scene change):

- The only names on screen are `Alder Creek Studio`, `Jamie Chen`, `Morgan Ellis`, `Riley Okafor`, the invented texts typed
  in the clips, and the theme's own sample texts (Lorem ipsum, project and milestone titles).
- The hint under the field Twitter of a Team card reads "Twitter handle": the hint of the recipe names a real person (seed
  step 5). Search the OCR text for the handle that this hint gives as its example
  (`TheAgencyTheme/Recipes/agency.recipe.json:697`).
- No company logo (the Clients row is empty), no real e-mail address (`admin@example.com` is the only one, and it is not shown), no
  password, no token, no host name other than `localhost:5010` in the address bar if it is shown.
- The form of `06-finish` is never submitted.
- `state.json`, the `App_Data` snapshots and the password stay outside git.
