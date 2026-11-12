# Recording plan: Build a Contact Form From Widgets

How to record this video without having to decide anything. A selector written in backticks stands for
`scene.Page.Locator(...)`. Every label was read from the Orchard Core source (the commit named in
`catalog/SOURCE.md`); the `file:line` after each action is where the label or the element comes from, relative to
`src/OrchardCore.Modules/` in the Orchard Core checkout (a theme file is written with its full path under
`src/OrchardCore.Themes/`). The things nobody could run when this was written are in `OPEN-QUESTIONS.md`: check them
with `Recorder probe` before the first take.

## The site it needs

- **Orchard Core**: the checkout at the commit the catalog was read from, `src/OrchardCore.Cms.Web`, on
  `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (environment variables, no setup or login form is recorded), tenant `Default`. The web project
  registers the feature itself (`src/OrchardCore.Cms.Web/Program.cs:9`), so the variables are enough:

  | Setting | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Outbound calls**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. This video calls no external service and
  needs no local stand-in.
- **Features before recording**: exactly those the Blog recipe enables. **The Forms feature must be disabled** at the start:
  clip `02-create` enables it on camera. Do not enable it in a `prepare` command. The Widgets and Flows features, which
  Forms depends on, are enabled by the Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the
  `feature` step).
- **Settings**: none to save. **Seed data created outside the capture**: none. The pages of the Blog recipe stay as they
  are (fake content). There is no `PrepareAsync`.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (never committed). Copy `state.json` aside together with each snapshot below.
- **Browser**: the recorder's defaults (1536x864 CSS pixels, scale 1.25, light, en-US).

### The fake values typed on camera

| Where | Value |
|---|---|
| Page title | `Contact Us` |
| Form widget title | `Contact` |
| Input 1: HTML Name, Label Text, Placeholder Text | `Name`, `Name`, `Your full name` |
| Input 2: HTML Name, Label Text, Type, Placeholder Text | `Email`, `Email address`, `Email`, `name@example.com` |
| Select: HTML Name, Label Text | `Topic`, `What is this about` |
| Select options (Option Text / Value) | `General question` / `general`, `Support request` / `support`, `Sales` / `sales` |
| Text Area: HTML Name, Label Text, Placeholder Text, Rows | `Message`, `Message`, `How can we help`, `6` |
| Button: HTML Name, Text, Type | `Send`, `Send message`, `Submit` |
| Filled in on the site (clip `05-site`) | `Jamie Chen`, `jamie.chen@example.com`, `Support request`, `When is the office open?` |

## Order, dependencies and snapshots

The clips depend on each other and are recorded in the order they play: `02-create`, `03-text`, `04-rest`,
`05-site`. The slides of chapter 01 need no site.

| Snapshot | Taken | Site state |
|---|---|---|
| S0 | After Auto Setup and `Recorder setup`, with the site stopped | Blog site, Forms disabled, no Contact Us page |
| S1 | After `02-create` | Forms enabled; a draft Page titled Contact Us with an empty-bodied Form widget |
| S2 | After `03-text` | The draft also has the Name and Email Input widgets inside the form |
| S3 | After `04-rest` | The page is published, with all five widgets inside the form |

Every clip changes the site. Take S1 to S3 with the site stopped: stop it, copy `App_Data` and `state.json`, start it
again. To record one clip again, stop the site, restore the snapshot that precedes it (S0 for
`02-create`, S1 for `03-text`, S2 for `04-rest`, S3 for `05-site`) together with its `state.json`, and start the site.
`05-site` changes nothing (the sample values are typed in the page and never submitted).

### Reopening the Contact Us page off camera (clips `03-text` and `04-rest`)

Each clip begins in the editor of the page saved by the clip before it, so it is opened before `StartCaptureAsync`:

1. `scene.OpenAsync("/Admin/Contents/ContentItems?q=Contact%20Us")`.
2. Click the Edit button (`a.edit`) of the row titled Contact Us, which is the only row or the first one
   (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`); stop if the first row is another page. The link carries a `returnUrl`, so Save Draft
   and Publish return to the Content Items list.
3. Wait for `button[title="Add Widget"]` to be visible (the page-level one; the Form widget's own appears inside its card).
4. If the admin menu shows its labels, click `button.leftbar-compactor` (title "Collapse / expand menu",
   `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79`) once, so only the icons show and the editor has the width.
5. In clip `04-rest` only: click the collapse button (`.widget-editor-btn-collapse`) in the header of each of the two
   Input cards, so the Form widget's own Add Widget button is in view.

## Widget editor selectors (clips `02-create`, `03-text`, `04-rest`)

Names are the suffix of each input's `name` attribute (`OrchardCore.Forms/Assets/js/form-visibility.js:141-147` reads
them the same way).

| Name used below | Selector |
|---|---|
| `card(T)` | `.widget-editor[data-content-type="T"]` (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:29`) |
| Add Widget (page) | `button[title="Add Widget"]`, the last one on the page (`OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`) |
| Add Widget (form) | `card("Form")` then `button[title="Add Widget"]` |
| Widget type in the list | `a.add-widget[data-widget-type="T"]`, inside the same card as the button; `T` is `Form`, `Input`, `Select`, `TextArea` or `Button` |
| Title | `input[name$="TitlePart.Title"]` |
| Action / Method / EncType | `input[name$="FormPart.Action"]`, `select[name$="FormPart.Method"]`, `select[name$="FormPart.EncType"]` |
| Anti Forgery Token / Save Form Location | `input[name$="FormPart.EnableAntiForgeryToken"]`, `input[name$="FormPart.SaveFormLocation"]` |
| HTML Name | `input[name$="FormInputElementPart.Name"]` |
| ID | `input[name$="FormElementPart.Id"]` |
| Label option / Label Text | `select[name$="FormElementLabelPart.LabelOption"]`, `input[name$="FormElementLabelPart.Label"]` |
| Type (Input) / Placeholder Text / Default Value | `select[name$="InputPart.Type"]`, `input[name$="InputPart.Placeholder"]`, `input[name$="InputPart.DefaultValue"]` |
| Editor (Select) | `select[name$="SelectPart.Editor"]` |
| Text Area: Placeholder Text / Rows | `input[name$="TextAreaPart.Placeholder"]`, `input[name$="TextAreaPart.Rows"]` |
| Button: Text / Type | `input[name$="ButtonPart.Text"]`, `select[name$="ButtonPart.Type"]` |
| Save Draft / Publish | `button.draft` and `button.publish` (the main buttons of the split groups) |

Rules for the three editor clips:

- Scope every selector to the card it belongs to: `card("Form")` for the page's Form widget, then
  `card("Input").Nth(0)` or `.Nth(1)`, `card("Select")`, `card("TextArea")`, `card("Button")` inside it. The page also
  has its own Title and Permalink, and the cards have the same field names.
- Each Input, Select and Text Area card has a second tab, Visibility Settings. Stay on the first tab, the one that
  holds HTML Name.
- Label Text stays hidden until Label option is not None (`OrchardCore.Forms/Assets/js/formElementLabelManager.js:11`):
  choose Standard first, then type the label text.
- The header of every card also holds a plus button titled "Insert Widget"
  (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:51`). Never click it: the widgets are added with the button
  titled "Add Widget".
- Clear highlights (`ClearHighlightsAsync`) before scrolling.
- Type with a delay of 20 ms in `text-6`, `rest-3`, `rest-4` and `site-3`, where the actions are close to the length of the
  narration. An action must end before the narration of its step ends.

## Clip `02-create`

**Starts on** `/Admin`, signed in, the admin menu showing its labels. Before `StartCaptureAsync`, expand the Tools
group and the Content group of the admin menu (click the entry labeled Tools; if the entry is also a link, click its
chevron button `button.nav-toggle[aria-label="Toggle Tools"]`; the same for Content), so the links Features and Content
Items are visible. Forms is disabled, and no Contact Us page exists.

- `create-1`: ring the Features link in the Tools group (`a[href$="/Admin/Features"]`; menu labels `Tools` and
  `Features`, `OrchardCore.Features/AdminMenu.cs:26-27`), then `ClickAndWaitForNavigationAsync` on it. Click the search
  box (`#search-box`, placeholder "Search", `OrchardCore.Features/Views/Admin/Features.cshtml:27`) and type `Forms`. Move
  the cursor to the Enable button of the row named Forms (`a[href*="/Features/OrchardCore.Forms/Enable"]`, text "Enable",
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) and ring it. *Visible at the end:* the
  Features page, "Forms" in the search box, the matching rows, the Enable button of the Forms row ringed. Do not click yet.
- `create-2`: `ClickAndWaitForNavigationAsync` on that Enable button, then clear the ring. The page reloads with an empty search box
  (`OrchardCore.Features/Controllers/AdminController.cs:255`), so do not look for the Forms row. *Visible at the end:* the
  Features page with the notification "The feature Forms was enabled." at the top
  (`OrchardCore.Features/Controllers/AdminController.cs:317`).
- `create-3`: ring the Content Items link of the Content group (`a:text-is("Content Items")`, the first match; labels `Content`
  and `Content Items`, `OrchardCore.Contents/AdminMenu.cs:65,68`). Do not select it by an `href` ending in
  `/Admin/Contents/ContentItems`: the Blog recipe adds its own Content group with a Content Items link whose address ends in
  a slash (the `AdminMenu` step of `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), and
  `OrchardCore.Navigation.Core/NavigationManager.cs:74-152` merges the two groups by their names, so either address may
  be the one that stays. `ClickAndWaitForNavigationAsync` on it, then click the
  New button (`#new-dropdown`, text "New", `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`) to open its
  list. Ring the Page entry (`a.dropdown-item[href*="/Contents/ContentTypes/Page/Create"]`). *Visible at the end:* the
  Content Items page with the New list open and the Page entry ringed. Do not click yet.
- `create-4`: `ClickAndWaitForNavigationAsync` on the Page entry. Type `Contact Us` into the page's
  `input[name$="TitlePart.Title"]` (label "Title", `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`). Ring the Permalink
  field (`input[name$="AutoroutePart.Path"]`, label "Permalink", hint "Leave empty to auto-generate it",
  `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30,46`) and leave it empty. *Visible at the end:* the new Page
  editor, "Contact Us" in Title, the Permalink empty and ringed.
- `create-5`: clear the ring, scroll to the widget list. Click the page-level `button[title="Add Widget"]` (a plus icon;
  `OrchardCore.Flows/Views/FlowPart.Edit.cshtml:72`), then click `a.add-widget[data-widget-type="Form"]` (list entry
  "Form"). Wait until `card("Form")` is visible. *Visible at the end:* a Form card in the widget list, expanded, its
  Title field at the top.
- `create-6`: type `Contact` into `card("Form")` then `input[name$="TitlePart.Title"]` (label "Title"; an ID field
  follows it, `FormElementPart`, `OrchardCore.Forms/Migrations.cs:48-50`: leave it empty and do not ring it). The Form type
  does not render its title on the page: `OrchardCore.Forms/Migrations.cs:28` sets RenderTitle to false, and
  `OrchardCore.Title/Drivers/TitlePartDisplayDriver.cs:24` honors it. *Visible at the end:*
  the Form card's Title shows Contact.
- `create-7`: ring, one after the other, Action (`input[name$="FormPart.Action"]`, empty), Method
  (`select[name$="FormPart.Method"]`, shows POST) and EncType (`select[name$="FormPart.EncType"]`, shows
  application/x-www-form-urlencoded); labels "Action", "Method", "EncType",
  `OrchardCore.Forms/Views/Items/FormPart.Fields.Edit.cshtml:6,14,25`. Change nothing. *Visible at the end:* the three
  fields on screen, Action empty, Method POST.
- `create-8`: clear the rings, scroll down, ring the two checkboxes "Anti Forgery Token" and "Save Form Location"
  (`FormPart.Fields.Edit.cshtml:40,50`); both are checked. Change nothing. *Visible at the end:* both checkboxes checked and ringed.
- `create-9`: clear the ring, `ClickAndWaitForNavigationAsync` on `button.draft` (text "Save Draft",
  `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:19`). *Visible at the end:* the Content Items list with the
  notification "Your Page draft has been saved." (`OrchardCore.Contents/Controllers/AdminController.cs:439`) and the
  Contact Us row.

**Changes the site**: yes (Forms enabled, a draft page). Take snapshot S1 after it.

## Clip `03-text`

**Starts on** the Contact Us editor, opened off camera as described above, scrolled so the header of the Form card is
at the top and its own plus button (Add Widget, form) is in view. The Form card shows Title Contact and no widgets.

- `text-1`: click Add Widget (form), then `a.add-widget[data-widget-type="Input"]` (list entry "Input"). Wait until
  `card("Input").Nth(0)` is visible. *Visible at the end:* one Input card inside the Form card, expanded, HTML Name at the top.
- `text-2`: type `Name` into `input[name$="FormInputElementPart.Name"]` (label "HTML Name", required,
  `OrchardCore.Forms/Views/Items/FormInputElementPart.Fields.Edit.cshtml:5`). *Visible at the end:* HTML Name shows Name.
- `text-3`: ring the ID field (`input[name$="FormElementPart.Id"]`, label "ID",
  `OrchardCore.Forms/Views/Items/FormElementPart.Fields.Edit.cshtml:5`). Leave it empty. *Visible at the end:* the empty ID field, ringed.
- `text-4`: clear the ring. Ring Label option (`select[name$="FormElementLabelPart.LabelOption"]`, shows None;
  `OrchardCore.Forms/Views/Items/FormElementLabelPart.Fields.Edit.cshtml:10`), choose Standard with `SelectAsync`,
  then type `Name` into `input[name$="FormElementLabelPart.Label"]` (label "Label Text", which appears after the choice,
  line 22). *Visible at the end:* Label option Standard, Label Text Name.
- `text-5`: clear rings. Ring Type (`select[name$="InputPart.Type"]`, shows Text), type `Your full name` into
  `input[name$="InputPart.Placeholder"]` (label "Placeholder Text"), and ring Default Value
  (`input[name$="InputPart.DefaultValue"]`, label "Default Value", empty); labels at
  `OrchardCore.Forms/Views/Items/InputPart.Fields.Edit.cshtml:7,37,45`. *Visible at the end:* Type Text, Placeholder Text
  Your full name, Default Value empty.
- `text-6`: clear rings, click `card("Input").Nth(0)` then `.widget-editor-btn-collapse` to collapse the first Input.
  Click Add Widget (form), then `a.add-widget[data-widget-type="Input"]`; wait for `card("Input").Nth(1)`. Fill it in the
  order of the narration: HTML Name `Email`; Label option Standard; Label Text `Email address`; Type Email (`SelectAsync`,
  option "Email", `InputPart.Fields.Edit.cshtml:12`); Placeholder Text `name@example.com`. *Visible at the end:* the second
  Input card with HTML Name Email, Label option Standard, Label Text Email address, Type Email, Placeholder Text
  name@example.com; the first Input collapsed above it.
- `text-7`: `ClickAndWaitForNavigationAsync` on `button.draft`. *Visible at the end:* the Content Items list with the
  notification "Your Page draft has been saved.".

**Changes the site**: yes (two widgets saved in the draft). Take snapshot S2 after it.

## Clip `04-rest`

**Starts on** the Contact Us editor, opened off camera as described above, with both Input cards collapsed. The Form
card shows two collapsed cards, whose headers read "Name Input" and "Email Input", and its Add Widget (form) button.

- `rest-1`: click Add Widget (form), then `a.add-widget[data-widget-type="Select"]` (list entry "Select"); wait for
  `card("Select")`. Type `Topic` into HTML Name; choose Standard in Label option; type `What is this about` into Label Text.
  *Visible at the end:* the Select card with HTML Name Topic, Label option Standard, Label Text What is this about.
- `rest-2`: ring Editor (`select[name$="SelectPart.Editor"]`, label "Editor", shows Dropdown;
  `OrchardCore.Forms/Views/Items/SelectPart.Fields.Edit.cshtml:19`). Change nothing. *Visible at the end:* Editor Dropdown, ringed.
- `rest-3`: clear the ring. Three times, click "Add an option" (`a:has-text("Add an option")` inside the Select card,
  `OrchardCore.Forms/Services/FormsJSLocalizer.cs:23`), then in the new last row of `.select-options-table` type the
  option text into `input[placeholder="Enter the text"]` and the value into `input[placeholder="Enter a value"]`
  (`FormsJSLocalizer.cs:16-17`): `General question` / `general`, `Support request` / `support`, `Sales` / `sales`. Leave
  the Default? column empty. Table headings: "Option Text", "Value", "Default?" (`FormsJSLocalizer.cs:20-22`). *Visible at
  the end:* the table with the three rows and no default marked.
- `rest-4`: collapse the Select card (`.widget-editor-btn-collapse`). Click Add Widget (form), then
  `a.add-widget[data-widget-type="TextArea"]`; wait for `card("TextArea")`. Fill in: HTML Name `Message`; Label option
  Standard; Label Text `Message`; Placeholder Text `How can we help` (`input[name$="TextAreaPart.Placeholder"]`, label
  "Placeholder Text"); Rows `6` (`input[name$="TextAreaPart.Rows"]`, label "Rows"); labels at
  `OrchardCore.Forms/Views/Items/TextAreaPart.Fields.Edit.cshtml:5,21`. *Visible at the end:* the Text Area card with the
  five values.
- `rest-5`: collapse the Text Area card. Click Add Widget (form), then `a.add-widget[data-widget-type="Button"]`; wait
  for `card("Button")`. Type `Send` into HTML Name (required for a Button too, `OrchardCore.Forms/Drivers/FormInputElementPartDisplayDriver.cs:37`),
  `Send message` into `input[name$="ButtonPart.Text"]` (label "Text", shown with the required mark,
  `OrchardCore.Forms/Views/Items/ButtonPart.Fields.Edit.cshtml:5`; the code validates only the HTML Name, so the mark on Text
  is only a style), and ring Type (`select[name$="ButtonPart.Type"]`, shows
  Submit, line 13). Change nothing in Type. *Visible at the end:* the Button card with HTML Name Send, Text Send message,
  Type Submit.
- `rest-6`: clear the ring, `ClickAndWaitForNavigationAsync` on `button.publish` (text "Publish",
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`). *Visible at the end:* the Content Items list with the
  notification "Your Page has been published." (`OrchardCore.Contents/Controllers/AdminController.cs:966-967`).

**Changes the site**: yes (the page is published). Take snapshot S3 after it.

## Clip `05-site`

**Starts on** the Contact Us editor, opened off camera as described above (menu collapsed, the Permalink field in view).
Do not collapse any card; scroll the editor to the Permalink field.

- `site-1`: ring the Permalink field (`input[name$="AutoroutePart.Path"]`), which now holds `contact-us`. Then
  `NavigateAsync("/contact-us")`. *Visible at the end:* the front-end page with the form: a labeled Name
  field, an Email address field, a What is this about dropdown list, a Message text area and the Send message button.
- `site-2`: ring, one after the other, the Name label and field, the Email address field, and the Topic dropdown list
  (`select[name="Topic"]`). Scroll only if the Send message button is below the fold, after clearing the rings. *Visible
  at the end:* the whole form on screen, every label and placeholder readable.
- `site-3`: type `Jamie Chen` into `input[name="Name"]`, `jamie.chen@example.com` into `input[name="Email"]`, choose
  `Support request` in `select[name="Topic"]` (`SelectAsync`), and type `When is the office open?` into
  `textarea[name="Message"]`. *Visible at the end:* the four fields filled in, nothing submitted.
- `site-4`: ring the Send message button (`button[name="Send"]`, caption "Send message" is optional). **Do not click it.**
  *Visible at the end:* the filled form with the button ringed.

**Changes the site**: no.

## Slides (chapter 01, clip `01-idea`, drawn by `slides.py`, nothing to record)

- `idea-1`: three cards, "A page that holds widgets", "The Form widget", "Field widgets", all bright.
- `idea-2`: four cards, one per field widget: Input, Text Area, Select, Button.
- `idea-3`: three cards, "HTML Name", "Label option", "Placeholder Text"; the first two bright, the third dimmed.

## Checks for private data

- The only addresses on screen are `admin@example.com` (never shown in these clips), `name@example.com` and
  `jamie.chen@example.com`; the only person is "Jamie Chen". The Blog recipe's sample content has no real names.
- After assembling, read the contact sheets of `02-create` to `05-site`: no error banner, no password, no real host name.
- OCR the video (scene changes, then 5 frames per second) and search for `@`: every hit ends with `example.com`.
- Do not click Send message in `05-site`: nothing in this video submits the form.
