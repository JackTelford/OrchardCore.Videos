# Recording plan: Content Fields: Links, Rich Text, Markdown and YouTube Videos

This plan lets a person record the video without deciding anything. Read `OPEN-QUESTIONS.md` first: it lists what the
author could not check without a running site (editor toolbar selectors, the technical names that the page generates,
and the pronunciation of a few labels).

A selector below stands for `scene.Page.Locator(...)`. Labels are spelled as the code spells them, and each entry gives
the file in the Orchard Core source where the label comes from. A path that starts with a module name, such as `OrchardCore.Contents/...`, is in `src/OrchardCore.Modules/`; the others start with `src/`. Text after "Type"
is typed with `scene.TypeAsync`, except where a step says otherwise. Text after "Press" is a key press.

Two things appear on every field settings page and are left alone: the button **Edit placements** above the settings
(`OrchardCore.Placements/Views/PlacementSettings.cshtml:5`, shown because the Blog recipe enables Placements), and, below the
editor question, the drop-down **What type of display mode should be used?** (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:100`),
which stays on "Standard". Do not click the button or change the drop-down.

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` |
| App_Data | Its own folder outside both repositories (`ORCHARD_APP_DATA`), empty before Auto Setup |
| Auto Setup | Tenant `Default`, recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (the one passed in `VIDEO_PASSWORD`, chosen outside git; never written in the repository). See `references/recording.md` and the `orchardcore-tester` skill for the environment variables |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` of the site process point to a dead address, so the site never calls a service. The browser is not affected |
| Features to enable | None. The Blog recipe enables **Content Fields**, **Content Types**, **Contents**, **Features**, **Markdown** and **Title** (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, lines 38-73, the `feature` step). Check them once under Tools, Features (`OrchardCore.Features/AdminMenu.cs:26-27`) |
| Settings | None changed |
| Sign-in | Run `Recorder setup` once, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, so `state.json` exists (git-ignored). No sign-in is recorded |
| Init script | Not needed: the site is fresh and holds only fake data |

### External services and their local stand-ins

The Youtube Field turns the address it is given into an embedded address on the same host: for
`https://www.example.com/watch?v=pottery-teaser` the embedded address is `https://www.example.com/embed/pottery-teaser`
(`OrchardCore.ContentFields/Drivers/YoutubeFieldDisplayDriver.cs:60-84`). The programme allows `example.com` as the only
real domain in the repository, so the video uses an `example.com` address and a stand-in for the player. The stand-in is
a route on the browser context, set in `07-site` before the capture starts (see that clip). It answers the embedded
address with a dark page and a green play triangle:

```csharp
const string StandIn = "<!doctype html><meta charset=\"utf-8\"><style>html,body{margin:0;height:100%;background:#181B1A}" +
    "body{display:grid;place-items:center}.play{width:0;height:0;border-style:solid;border-width:28px 0 28px 48px;" +
    "border-color:transparent transparent transparent #41B670}</style><body><div class=\"play\"></div></body>";
await scene.Page.Context.RouteAsync("https://www.example.com/embed/**",
    route => route.FulfillAsync(new RouteFulfillOptions { ContentType = "text/html", Body = StandIn }));
```

No other external address is loaded by the pages of this video. Do not click the Registration Link on the site: it opens
`example.com` in another tab.

### The seed data, created outside the capture

Create the **Workshop** content type with the Title part, with the browser signed in, before the first take:

1. Open `/Admin/ContentTypes/List` (menu: Design, Content Definition, Content Types: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`).
2. Select **Create new type** (`OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`). Display Name **Workshop**; the Technical
   Name fills in as `Workshop`; select **Create** (`Views/Admin/Create.cshtml:14,22,30`).
3. The "Add Parts" page opens (`Views/Admin/AddPartsTo.cshtml:10,18`). Tick **Title** (the part is named `TitlePart`; its display
   name drops the word Part: `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`),
   then select **Add Selected Parts** (`AddPartsTo.cshtml:47`).
4. Open `/Admin/ContentTypes/Edit/Workshop`. Under **Parts** there are **Title** and **Workshop**, and the **Fields** section is
   empty with the **Add Field** button below it. A new type is creatable, listable, draftable and versionable
   (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:85-91`).

Nothing else is seeded. The fake values typed during the clips are:

| Where | Value |
|---|---|
| Field Display Names | `Registration Link`, `Schedule`, `What to Bring`, `Teaser Video` |
| Hints | Schedule: `Days, times and room.` / What to Bring: `One item per line.` / Teaser Video: `Use the address of the video page.` |
| Youtube placeholder | `Paste the address of the video page` |
| Item Title | `Pottery Basics` |
| Registration Link | Url `https://example.com/workshops/pottery-basics`, Link text `Register for Pottery Basics`, Target `_blank` |
| Schedule | `Saturdays from `, then **10:00 to 12:00** in bold, then `.`, a new paragraph `Doors open at 9:30.` |
| What to Bring | Four lines: `## Bring with you`, `- An apron`, `- A towel`, `- A notebook` |
| Teaser Video | `https://www.example.com/watch?v=pottery-teaser` |

## 2. Order of the clips, and what changes the site

Record in this order. Every clip starts from the state the previous one left.

| Order | Clip | Starts after | Changes the site |
|---|---|---|---|
| 1 | `02-link` | the seed | Adds the field Registration Link to the Workshop type |
| 2 | `03-html` | `02-link` | Adds the field Schedule |
| 3 | `04-markdown` | `03-html` | Adds the field What to Bring |
| 4 | `05-youtube` | `04-markdown` | Adds the field Teaser Video |
| 5 | `06-fill` | `05-youtube` | Creates and publishes the content item Pottery Basics |
| 6 | `07-site` | `06-fill` | Nothing: it only reads |

The slide clips `01-fields` and `08-safe` and the two cards need no recording.

**Snapshots.** Stop the site and copy its `App_Data` folder (and keep `state.json`) after the seed, and again after each
clip, under the name of that clip. To record a clip again, stop the site, restore the copy taken before that clip, and
start the site again. To record everything again, restore the copy taken after the seed.

## 3. The clips

Every clip is opened on its starting page with `scene.OpenAsync` before `scene.StartCaptureAsync()`. Keep the actions of
a step shorter than its narration. After each click that loads a page, use `ClickAndWaitForNavigationAsync`.

### Clip `02-link` (chapter 02, Link Field)

Starts on `/Admin/ContentTypes/List`. The list shows a row **Workshop** with the buttons **Create New Workshop**, **List Items**,
**Edit** and **Delete** (`OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19,22,24`).

- `link-1`: Highlight the **Workshop** row for a moment, then clear the highlight. Click **Edit** of that row
  (`a[role="btn-edit-Workshop"]`). The page "Edit Content Type - Workshop" opens (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`).
  Highlight the **Fields** heading (`Edit.cshtml:88`), which is followed by an empty list, then clear the highlight and click
  **Add Field** (`a:has-text("Add Field")`, `Edit.cshtml:137`). *Visible when the narration ends:* the page "Add New Field To
  "Workshop"" with empty Display Name and Technical Name boxes (`Views/Admin/AddFieldTo.cshtml:38,46`).
- `link-2`: Type `Registration Link` into Display Name (`input[name="DisplayName"]`). Do not type the Technical Name: it
  fills in from the Display Name on key up (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`). Scroll down until the Field
  Type list shows **Link Field**, then click its radio button (`input[type="radio"][value="LinkField"]`): the first radio
  of the list is preselected, so the click is required (`AddFieldTo.cshtml:61`). The label of each radio is the field name
  split into words (`AddFieldTo.cshtml:62`). *Visible:* the **Field Type** list with **Link Field** selected.
- `link-3`: Click **Save & Configure** (`button[value="SaveAndConfigure"]`, `AddFieldTo.cshtml:72`) and wait for the page.
  *Visible:* the page `"Registration Link" settings` (`Views/Admin/EditField.cshtml:30`) with the Display Name, the read-only
  Technical Name, and the first settings of the Link Field.
- `link-4`: Click the **Required** check box (`input[type="checkbox"][name$="Required"]`,
  `OrchardCore.ContentFields/Views/LinkFieldSettings.Edit.cshtml:8`). Select **Text is required** in the **Text** drop-down with
  `scene.SelectAsync` (`select[name$="LinkTextMode"]`, option value `Required`, labels at `LinkFieldSettings.Edit.cshtml:14,18`; the other options are "Text is optional", "Use
  the default value" and "Use the url of the link"). *Visible:* Required ticked and the drop-down showing "Text is required".
- `link-5`: Scroll to **Default value of the Target** (`LinkFieldSettings.Edit.cshtml:77`). Type `_blank` into it
  (`input[name$="DefaultTarget"]`) and press Tab. Do not type anything into the two **Watermark** boxes: the editor then keeps its floating
  labels "Url" and "Link text" (`OrchardCore.ContentFields/Views/LinkField.Edit.cshtml:11`). Click **Save**
  (`button[name="submit.Save"][value="Save"]`, `EditField.cshtml:120`) and wait for the page. *Visible:* the page
  "Edit Content Type - Workshop" with the notification `The "Registration Link" field settings have been saved.` and the
  field **Registration Link** in the Fields list, followed by the words "Link Field".

### Clip `03-html` (chapter 03, Html Field)

Starts on `/Admin/ContentTypes/Edit/Workshop` (the Fields list holds Registration Link).

- `html-1`: Click **Add Field**. Type `Schedule` into Display Name. Scroll to the Field Type list and click the radio
  `input[type="radio"][value="HtmlField"]` (label "Html Field"). Click **Save & Configure** and wait. *Visible:* the page
  `"Schedule" settings`.
- `html-2`: Type `Days, times and room.` into **Hint** (`textarea[name$="Hint"]`,
  `OrchardCore.ContentFields/Views/HtmlFieldSettings.Edit.cshtml:4`). Highlight the **Sanitize Html** check box
  (`input[type="checkbox"][name$="SanitizeHtml"]`, `HtmlFieldSettings.Edit.cshtml:15`), which is ticked already:
  do not change it. *Visible:* the Hint filled and Sanitize Html ticked, with the highlight.
- `html-3`: Clear the highlight. Scroll to the question **What type of editor should be used?**
  (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:79`). Select **Wysiwyg editor** in the drop-down with
  `scene.SelectAsync` (`select[data-field-editor-select]`, value `Wysiwyg`; the options are "Standard", "Monaco editor", "Multiline", "Trumbowyg
  editor" and "Wysiwyg editor": `OrchardCore.ContentFields/Views/HtmlField.Option.cshtml:4`, `HtmlField-Wysiwyg.Option.cshtml:4`).
  Selecting Wysiwyg shows no extra panel for the Html Field; if a panel with code appears, stop and see `OPEN-QUESTIONS.md`.
  Click **Save** and wait. *Visible:* the page "Edit Content Type - Workshop", with Registration Link and Schedule in the
  Fields list; the Schedule line carries a badge with the word Wysiwyg.

### Clip `04-markdown` (chapter 04, Markdown Field)

Starts on `/Admin/ContentTypes/Edit/Workshop`.

- `markdown-1`: Click **Add Field**. Type `What to Bring` into Display Name. Click the radio
  `input[type="radio"][value="MarkdownField"]` (label "Markdown Field"). Click **Save & Configure** and wait. *Visible:* the
  page `"What to Bring" settings`.
- `markdown-2`: Type `One item per line.` into **Hint** (`textarea[name$="Hint"]`, `OrchardCore.Markdown/Views/MarkdownFieldSettings.Edit.cshtml:3`).
  Highlight **Sanitize Html** (`input[type="checkbox"][name$="SanitizeHtml"]`, `MarkdownFieldSettings.Edit.cshtml:14`), which
  is ticked already. Then highlight the editor drop-down (`select[data-field-editor-select]`) while the narration names
  the "Wysiwyg editor" option, **without choosing it**: the drop-down stays on "Standard" (`OrchardCore.Markdown/Views/MarkdownField.Option.cshtml:4`,
  `MarkdownField-Wysiwyg.Option.cshtml:4`). Choosing Wysiwyg for this field opens a panel of editor options written as code,
  which an admin video must not show. Clear the highlights and click **Save**, then wait. *Visible:* "Edit Content Type -
  Workshop" with three fields in the list.

### Clip `05-youtube` (chapter 05, Youtube Field)

Starts on `/Admin/ContentTypes/Edit/Workshop`.

- `youtube-1`: Click **Add Field**. Type `Teaser Video` into Display Name. Click the radio
  `input[type="radio"][value="YoutubeField"]` (the label reads "Youtube Field", with a lower-case t: it is the field name
  split into words). Click **Save & Configure** and wait. *Visible:* the page `"Teaser Video" settings`.
- `youtube-2`: Type `Paste the address of the video page` into **Placeholder** (`input[name$="Placeholder"]`,
  `OrchardCore.ContentFields/Views/YoutubeFieldSetting.Edit.cshtml:14`). Type `Use the address of the video page.` into **Hint**
  (`textarea[name$="Hint"]`, `YoutubeFieldSetting.Edit.cshtml:22`). Highlight **Width** and **Height**
  (`input[name$="Width"]`, `input[name$="Height"]`, `YoutubeFieldSetting.Edit.cshtml:30,38`); they already hold 560 and 315
  (`OrchardCore.ContentFields/Settings/YoutubeFieldSettingsDriver.cs:19-20`). Clear the highlights and click **Save**, then wait.
  The save stores the two sizes; a field that never went through this page displays at a different size (see `OPEN-QUESTIONS.md`).
  *Visible:* "Edit Content Type - Workshop" with four fields.
- `youtube-3`: Highlight the Fields list (`ul#fields`). *Visible:* the four fields, in order: Registration Link (Link
  Field), Schedule (Html Field), What to Bring (Markdown Field), Teaser Video (Youtube Field), each with the field type
  after its name (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:88`).

### Clip `06-fill` (chapter 06, Fill in a Workshop)

Starts on `/Admin/ContentTypes/List`.

- `fill-1`: Click **Create New Workshop** (`a#btn-create-Workshop`; the label is `Create New {type name}`:
  `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`) and wait. *Visible:* the page "New Workshop"
  (`OrchardCore.Contents/Views/Admin/Create.cshtml:26`) with the **Title** box (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`)
  and the four fields. The Title is expected first, then the fields in the order they were added; confirm it (see `OPEN-QUESTIONS.md`).
- `fill-2`: Type `Pottery Basics` into Title (`input[name="TitlePart.Title"]`). Scroll to the Registration Link. Type the
  Url into `input[name$=".Url"]` and the Link text into `input[name$=".Text"]` (the floating labels read "Url" and "Link
  text": `LinkField.Edit.cshtml:25,41`). Highlight the **Target** box (`input[name$=".Target"]`, `LinkField.Edit.cshtml:55`),
  which already holds `_blank` from the default of the field settings (`OrchardCore.ContentFields/Drivers/LinkFieldDisplayDriver.cs:60`).
  *Visible:* the three Link boxes filled, with the Target highlighted.
- `fill-3`: Clear the highlight and scroll to the Schedule. The Wysiwyg editor shows a toolbar above an editable area
  (`.html-field-wysiwyg-editor .trumbowyg-editor`). Click in the editable area with `scene.ClickAsync`. Then type every piece
  with `scene.Page.Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = 55 })`, never with `scene.TypeAsync`: that
  method clicks the box again before each text, which can move the cursor and drop the bold state. The pieces, in order:
  `Saturdays from `, then a click on the bold button of the toolbar (`.html-field-wysiwyg-editor .trumbowyg-strong-button`,
  with `scene.ClickAsync`), `10:00 to 12:00`, a click on the bold button again, `.`, a press of Enter
  (`scene.Page.Keyboard.PressAsync("Enter")`), `Doors open at 9:30.` Do not click the button that shows the HTML of the text.
  *Visible:* the toolbar and the two paragraphs, the times in bold. The toolbar selectors come from the editor library,
  not from the Orchard Core source: confirm them with `Recorder probe` (see `OPEN-QUESTIONS.md`).
- `fill-4`: Scroll to What to Bring. Click the text area (`textarea[name$=".Markdown"]`) with `scene.ClickAsync`, then type
  its four lines with `scene.Page.Keyboard.TypeAsync(line, new KeyboardTypeOptions { Delay = 55 })`, pressing Enter between
  the lines (`scene.Page.Keyboard.PressAsync("Enter")`). Do not use `scene.TypeAsync` for the lines after the first one: it
  clicks the middle of the box each time, which can put the cursor inside an earlier line. *Visible:* the four lines in the text area
  and the hint `One item per line.` under it.
- `fill-5`: Scroll to Teaser Video. Type `https://www.example.com/watch?v=pottery-teaser` into `input[name$=".RawAddress"]`.
  *Visible:* the box holding the address, with its hint under it.
- `fill-6`: Click **Publish** (`button.publish[name="submit.Publish"]`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`) and wait.
  The item is created and published, and the browser returns to the Content Types list. *Visible:* `/Admin/ContentTypes/List`
  with the notification `Your Workshop has been published.` (`OrchardCore.Contents/Controllers/AdminController.cs:471-473`).
  If the page stays on the editor with a red message, a required value is missing: fix it and record the clip again.

### Clip `07-site` (chapter 07, On the site)

Starts on `/Admin/ContentTypes/List`, where `06-fill` ended. The first action opens the items of the type, whose list shows the row
**Pottery Basics** with the buttons **Edit** and **View**.
Before `StartCaptureAsync()`, register the stand-in route of section 1 on `scene.Page.Context`, so the new tab uses it too.

- `site-1`: Highlight the **Workshop** row for a moment, then clear the highlight. Click **List Items** of that row (`a[role="btn-list-Workshop"]`,
  `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:22`) and wait for `/Admin/Contents/ContentItems/Workshop`. Then click **View** (`a.btn.view` in the row, `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`). The link
  opens in a new tab (`target="_blank"`): click inside `scene.Page.RunAndWaitForPopupAsync(...)` and call `scene.SwitchToAsync(popup)`.
  *Visible:* the published item on the site, in the theme of the Blog recipe, with its title, and the four fields below it.
- `site-2`: Highlight the link (`div.field-type-linkfield a`, `OrchardCore.ContentFields/Views/LinkField.cshtml:48-49`). *Visible:*
  the link with the words `Register for Pottery Basics`.
- `site-3`: Clear the highlight, scroll if needed, highlight the Schedule (`div.field-type-htmlfield`,
  `OrchardCore.ContentFields/Views/HtmlField.cshtml:8`). *Visible:* two paragraphs, the times in bold.
- `site-4`: Clear the highlight, scroll if needed, highlight What to Bring (`div.field-type-markdownfield`,
  `OrchardCore.Markdown/Views/MarkdownField.cshtml:8`). *Visible:* the heading `Bring with you` and the three list items.
- `site-5`: Clear the highlight, scroll if needed, highlight the player (`iframe[src*="/embed/pottery-teaser"]`;
  `OrchardCore.ContentFields/Views/YoutubeField.cshtml:16`). *Visible:* the frame at 560 by 315 showing the stand-in page.
  Make sure the frame is not empty white: if it is, the route was registered too late or on the wrong context.

### Slides and cards (drawn by `slides.py`, not recorded)

- `00-intro` (card): the opening card with the title and the subtitle of chapter 00.
- `fields-1`: four cards, all bright: Link Field, Html Field, Markdown Field and Youtube Field, each with the need of a workshop page it answers.
- `fields-2`: the same four cards; the Link Field and the Html Field are bright, the others dimmed.
- `fields-3`: the same four cards; the Markdown Field and the Youtube Field are bright, the others dimmed.
- `fields-4`: three cards: Content Fields (Link Field, Html Field and Youtube Field), Markdown (Markdown Field), and Tools, Features (where both are enabled).
- `safe-1`: three cards: Sanitize Html, Cleaned when displayed, and Stored as written.
- `safe-2`: two cards: HTML inside Markdown is shown as text by default, and the Link Field rejects an address that isn't valid.
- `99-outro` (card): the closing card, subtitle `docs.orchardcore.net`.

Where the slide statements come from: `OrchardCore.ContentFields/Views/HtmlFieldSettings.Edit.cshtml:15-17` and
`OrchardCore.Markdown/Views/MarkdownFieldSettings.Edit.cshtml:14-16` (the Sanitize Html setting and its hint);
`OrchardCore.ContentFields/Drivers/HtmlFieldDisplayDriver.cs:32-37` and `OrchardCore.Markdown/Services/MarkdownDisplayService.cs:23-37`
(the content is converted, then sanitized when displayed; the stored value is the one typed); `OrchardCore.Markdown/Startup.cs:26,61-67`
(the default Markdown extensions are `nohtml+advanced`, and the Markdown documentation says that HTML is then escaped);
`OrchardCore.ContentFields/Drivers/LinkFieldDisplayDriver.cs:97-117` (the check that runs when the editor form is submitted) and
`OrchardCore.ContentFields/Handlers/LinkFieldHandler.cs:66-80` (the same check when the item is validated): an address that isn't
well formed, or whose link the HTML sanitizer changes, is rejected with "{0} is an invalid url.".

## 4. Checks for private data

Before the clips are assembled, look at every contact sheet and check that:

- the site is fresh and holds only the fake values of this plan, with the user `admin` and the email `admin@example.com`;
- no password, token, or secret is visible, typed or in a notification (the sign-in is never recorded);
- no address other than `example.com` appears in a box, in a link, or in the address of the player;
- the field settings page of the Markdown Field and the Html Field shows no panel of editor options (code) while recorded;
- the toolbar button that shows the HTML of the Schedule is never clicked;
- the browser tab, the window and the page show no real name, machine name, or folder of this computer.

Then read the video at one frame per second and at every scene change for the same data, as the skill asks.
