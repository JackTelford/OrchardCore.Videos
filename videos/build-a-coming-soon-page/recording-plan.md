# Recording plan: Launch a Coming Soon Page While You Build Your Site

For the person who records the video. Read this file only; nothing else has to be decided. The narration is in
`storyboard.json`; the steps below use its step ids. Source references are `file:line` in `OrchardCMS/OrchardCore`
(the checkout the site runs from), under `src/`. A selector below stands for `scene.Page.Locator(...)`, since the
recorder's methods take locators. Every selector and every on-screen label is read from the source; confirm each
selector once with `Recorder probe` before recording (see OPEN-QUESTIONS.md for the few labels that come from code
defaults).

This is an admin video: the viewer sees the Setup screen, the admin and the front end, never code. Do not open a
developer tool, a file or a terminal on camera.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout that holds `TheComingSoonTheme` and its `ComingSoon`
  setup recipe (`src/OrchardCore.Themes/TheComingSoonTheme/Recipes/comingsoon.recipe.json`).
- **Port and data**: `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an **empty**
  folder outside both repositories. Block outbound calls of the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead
  address, so it never calls a mail or reCAPTCHA provider.
- **No Auto Setup, no `state.json` for the first clip.** Clip `02-recipe` shows the Setup screen, so it is recorded on
  an uninstalled site: do not set the Auto Setup variables, and do not run `Recorder setup` before it (the recorder
  runs without a sign-in when `state.json` does not exist). The first request to the site opens the Setup screen
  (`src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml`).
- **Finish the setup by hand, off camera**, right after `02-recipe`, in an ordinary browser window, with these answers:

  | Setup label | Value |
  |---|---|
  | What is the name of your site? | `Example Garden Supply` |
  | Recipe | Coming Soon |
  | Default Time Zone | the entry whose value is `America/Los_Angeles` |
  | What type of database to use? | the default entry (keep it as it is) |
  | Table Prefix, Table Schema | empty |
  | User Name | `admin` |
  | Email | `admin@example.com` |
  | Password, Password Confirmation | `<password>`: the value of `VIDEO_PASSWORD`, chosen outside git, typed twice |

  Run the setup on a day of the month from 1 to 28: the recipe writes the target launch date of the message as the
  month after the day of setup (`comingsoon.recipe.json:17`), and a day past the end of the next month moves the date
  one month further.
- **Then sign in once**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which writes
  `state.json` (git-ignored). Stop the site, and copy the `App_Data` folder and `state.json` aside. This copy is the
  **snapshot after setup**.
- **Features to enable before recording**: none. The recipe enables everything the page uses (Forms, Workflows,
  ReCaptcha, Users Registration, Email, Templates, Flows, Widgets, Autoroute).
- **Settings to save before recording**: none. Do **not** enter reCAPTCHA keys, and do **not** configure an email
  provider: the notice "ReCaptcha settings are not configured." has to be on the front end for `visit-2`.
- **External services and stand-ins**: none. No clip submits the sign-up form, so nothing is sent anywhere. The browser
  loads two Google fonts for the front end (`layout.liquid:13-14`); with no network the page uses fallback fonts, which
  is acceptable.
- **Seed data**: none. All data comes from the recipe and from the values typed on camera, listed in each clip:
  - site name `Example Garden Supply`;
  - message `We are building an online shop for garden tools. Leave your email address and we will tell you when it opens.`;
  - button label `Keep Me Posted`;
  - social address `https://example.com/garden-supply`.
- `Recorder prepare` is not needed: the video has no `PrepareAsync`.

## Order of the clips, and what changes the site

Record in this order (it differs from the order they play in only where the table says so):

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-recipe` | the uninstalled site, no `state.json` | No: the form is never submitted. |
| (off camera) | finish the setup, `Recorder setup`, make the snapshot after setup | | Installs the site. |
| 2 | `03-visit` | the snapshot after setup | No. |
| 3 | `05-recaptcha` | the snapshot after setup | No: the fields stay empty. |
| 4 | `06-theme` | the snapshot after setup | No: Make Current is not clicked. |
| 5 | `04-edit` | the snapshot after setup | **Yes**: publishes a new message, button label and social address. |

`03-visit` must be recorded before `04-edit` (its narration describes the message the recipe writes). Before a new
take of `03-visit`, `05-recaptcha`, `06-theme` or `04-edit`, stop the site, restore the snapshot after setup (the
`App_Data` copy), start the site and sign in again with the saved `state.json`. To record `02-recipe` again, stop the
site, empty the data folder, delete `state.json`, and start the site.

Clip `06-theme` and the slide clips do not depend on `04-edit`.

## Cards and slides

- `intro` (card `00-intro`) and `outro` (card `99-outro`): the generated cards, nothing to record.
- `why-1`: slide "A site under construction", three cards: the site has an address, nothing useful to see, no way to
  follow up. All three are bright.
- `why-2`: slide "The Coming Soon recipe", four cards: a title and a message, a field for an email address, social
  icons, The Coming Soon Theme ("A background video behind the page", from `layout.liquid:24`). All four are bright.
- `signup-1`: slide "What a sign-up does", four numbered cards; the first one ("The visitor selects the button") is
  bright, the other three are dimmed.
- `signup-2`: the same slide with cards 2 to 4 bright (the workflow checks the sign-up, it creates a user, it sends a
  confirmation email), card 1 dimmed.
- `signup-4`: slide "Where to set it up", three cards: reCaptcha keys (Settings, Security, reCaptcha), Email
  provider (Tools, Features, then Settings, Communication, Email), Sign-ups (Access Control, Users). The paths come
  from `OrchardCore.ReCaptcha/AdminMenu.cs:26-28`, `OrchardCore.Features/AdminMenu.cs:26-27`,
  `OrchardCore.Email/AdminMenu.cs:38-40` and `OrchardCore.Users/AdminMenu.cs:27-30`. The recipe enables the Email
  feature but no provider feature, and the Email settings page then reads "No email providers are currently
  enabled." (`OrchardCore.Email/Views/EmailSettings.Edit.cshtml:6-14`); a provider such as "SMTP Email Provider"
  (`OrchardCore.Email.Smtp/Manifest.cs`) is enabled under Tools, Features, and its settings appear on the Email
  settings page (`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:35,88`).
- `launch-1`: slide "Make another page the home page", three numbered cards: open the content item (it needs a
  permalink), check Set as homepage (the option is under Permalink, and appears only when the content type's Autoroute settings have
  "Show homepage options" checked, `AutoroutePart.Edit.cshtml:56`, `AutoroutePartSettings.Edit.cshtml:45`), publish it. A homepage is set at publish only
  when the item's path is not empty (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:82`). The labels come from
  `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30` and `:72`.

## Clip `02-recipe` (chapter 02, Choose the recipe)

Starts on the root of the uninstalled site (`/`), which shows the Setup screen: the heading "Setup", the question
"What is the name of your site?" and the Recipe list. Open the page before `StartCaptureAsync`, and wait until
`#SiteName` is visible. Viewport 1536 x 864; do not scroll before the capture.

- `recipe-1` (about 13 s). Actions:
  1. `TypeAsync(#SiteName, "Example Garden Supply", 40)`.
  2. `ClickAsync(#recipeButton)`: the drop-down list opens and lists the recipes with a short description under each
     name. Wait until `a.dropdown-item[data-recipe-name="ComingSoon"]` is visible, then pause 700 ms.
  3. `ClickAsync(a.dropdown-item[data-recipe-name="ComingSoon"])`: the list closes and the button reads "Coming Soon".

  Visible when the narration ends: the site name filled in, and the Recipe button showing "Coming Soon", the list
  closed. Labels: "What is the name of your site?" `Index.cshtml:118`, "Recipe" `:130`, the button `:142`, the list
  entries `:148` (name "Coming Soon" and description "Coming Soon website." from `comingsoon.recipe.json:3-4`). The
  button takes its text from the chosen entry (`OrchardCore.Setup/Assets/ts/setup.ts:49-62`).
- `recipe-2` (about 11 s). Actions:
  1. `HighlightAsync(#recipeButton, "Coming Soon")`, hold 3 s.
  2. `ClearHighlightsAsync()`, then `ScrollAsync(0, 600)` until `#SubmitButton` is fully on screen (scroll more if the
     page is longer).
  3. `HighlightAsync(#SubmitButton, "Finish Setup")`. Do **not** click it.

  Visible when the narration ends: the Super User questions and the "Finish Setup" button with a green ring. Labels:
  "Super User" `Index.cshtml:218`, "Finish Setup" `:256`. The recipe facts spoken here (The Coming Soon Theme as the
  site theme, the Coming Soon page as the home page) come from `comingsoon.recipe.json:66-68` and `:96-102`.

After this clip finish the setup off camera, as described above.

## Clip `03-visit` (chapter 03, The page visitors see)

Starts on the front end, `/`, signed in or not (the page looks the same). Open it before `StartCaptureAsync` and wait
until the heading "Coming Soon" and the field with the placeholder "Enter email address..." are visible. The page
fills the window; the three social icons are in a column at the right edge (large screens,
`_social.scss:22-30`). The background video (`layout.liquid:24`) may not play in the recorder's browser: that is
accepted, the narration does not describe it.

- `visit-1` (about 14 s). Actions:
  1. `HighlightAsync(h1, "Coming Soon")` (the heading, `Content__ComingSoon.liquid:4`), hold 2 s.
  2. `ClearHighlightsAsync()`, then `HighlightAsync(.masthead-content p)`: the message, which ends with the target
     launch date in bold ("Our target launch date is" followed by a month and a year,
     `comingsoon.recipe.json:461`), hold 4 s.
  3. `ClearHighlightsAsync()`, then `HighlightAsync(input[type="email"])`, hold until the narration ends.

  Visible when the narration ends: the heading, the message with the month and year in bold, the email field with its
  placeholder "Enter email address..." (`comingsoon.recipe.json:451`) ringed. The month and year are the month after
  the setup day.
- `visit-2` (about 14 s). Actions:
  1. `ClearHighlightsAsync()`, then `HighlightAsync(#submit-button)`: the button (`Widget__Register.liquid:5`). Its
     text is "Notify Me!" (`comingsoon.recipe.json:454`); the theme draws it in capital letters
     (`_masthead.scss:57`), so the screen shows NOTIFY ME!. Hold 3 s.
  2. `ClearHighlightsAsync()`, then `HighlightAsync(.social-icons)`: the column of three round icons, hold 4 s.
  3. `ClearHighlightsAsync()`, then `HighlightAsync(.alert-warning)`: the notice below the form, "ReCaptcha settings
     are not configured." (`OrchardCore.ReCaptcha/Views/Items/ReCaptchaPart.cshtml:9`). Keep the ring until the
     narration ends.

  Visible when the narration ends: the whole page with the notice ringed. The icons come from Font Awesome served by
  the site (`layout.liquid:10`): confirm that three icons are drawn, not empty squares.

## Clip `04-edit` (chapter 04, Change the message)

Starts on the content list, `/Admin/Contents/ContentItems` (the page titled "Manage Content"), which lists one item,
"Coming Soon", with the buttons "Edit" and "View" (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`
and `:18`). Open it before `StartCaptureAsync`, with the left menu showing Content, Content Items as the active entry
(menu labels: `OrchardCore.Contents/AdminMenu.cs:65` and `:68`). This clip changes the site: record it last, from the
snapshot after setup.

- `edit-1` (about 8 s). Actions:
  1. `HighlightAsync(a[href$="/Admin/Contents/ContentItems"], "Content Items")` on the active left-menu entry, hold 2 s.
  2. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync(a.btn.edit)`: the "Edit" button of the only row.
     Wait until the field "Title" shows the value "Coming Soon".

  Visible when the narration ends: the editor of the Coming Soon item, top of the page, with the Title field.
- `edit-2` (about 15 s). Actions:
  1. `HighlightAsync(the "Title" field)`, hold 2 s (label "Title": `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`).
  2. `ClearHighlightsAsync()`, then `HighlightAsync(the blue notice "This content item is the current homepage")`, which
     sits under the "Permalink" field (`AutoroutePart.Edit.cshtml:30` and `:61`). The Permalink field is disabled and
     empty. Hold 3 s.
  3. `ClearHighlightsAsync()`, then `ScrollAsync(0, 350)` until the fields "ID", "Action", "Method", "EncType" and the
     checkboxes "Anti Forgery Token" and "Save Form Location" are on screen
     (`OrchardCore.Forms/Views/Items/FormElementPart.Fields.Edit.cshtml:5`, `FormPart.Fields.Edit.cshtml:6`, `:14`,
     `:25`, `:40`, `:50`). Then `HighlightAsync(the "Action" field)`. Its hint reads "The URL to submit the form to."
     (`FormPart.Fields.Edit.cshtml:9`). Do not type in any of these fields.

  Visible when the narration ends: the form fields, with a ring on Action.
- `edit-3` (about 12 s). Actions:
  1. `ClearHighlightsAsync()`, then `ScrollAsync(0, 500)` until the section labelled "Flow" and the card of the
     Register widget are on screen. The cards are expanded; the Register card holds a rich-text editor and the fields
     "Placeholder", "InputFieldName" and "Button" (`comingsoon.recipe.json:300`, `:326`, `:313`; the section label
     "Flow" comes from `OrchardCore.Contents/Views/ContentPart-FlowPart.Edit.cshtml:22`, see OPEN-QUESTIONS.md).
  2. In the rich-text editor of the Register card (`.trumbowyg-editor` inside that card): triple-click the message to
     select it, then `TypeAsync(..., "We are building an online shop for garden tools. Leave your email address and we
     will tell you when it opens.", 25)` (one line, no line break).
  3. `TypeAsync(the "Button" field of the Register card, "Keep Me Posted", 25)` after selecting its current text
     ("Notify Me!") with a triple-click; press Tab so the value is kept.
  4. `HighlightAsync(the "InputFieldName" field)`, leave it unchanged.

  Visible when the narration ends: the Register card with the new message in the editor, "Keep Me Posted" in the
  Button field, and a ring on InputFieldName, which still holds "Email".
- `edit-4` (about 11 s). Actions:
  1. `ClearHighlightsAsync()`, then `ScrollAsync(0, 700)` until the section "Social Icons" and its first card are on
     screen (label: `comingsoon.recipe.json:157`). The cards are headed "Twitter", "Facebook" and "Instagram", each
     with the small text "Social Icon" (`OrchardCore.Flows/Views/ContentCard-BagPart.Edit.cshtml`, the header line
     `@T["{0} <span class=\"hint dashed\">{1}</span>", ...]`). Each card has the fields "Icon Class" and "Social Site
     Url" (`comingsoon.recipe.json:266`, `:280`).
  2. `HighlightAsync(the "Icon Class" field of the first card)`, hold 2 s.
  3. `ClearHighlightsAsync()`, triple-click the "Social Site Url" field of the first card (it holds
     `https://twitter.com`), `TypeAsync(..., "https://example.com/garden-supply", 25)`, press Tab.

  Visible when the narration ends: the first card with "Social Site Url" showing
  `https://example.com/garden-supply`.
- `edit-5` (about 8 s). Actions:
  1. `ClearHighlightsAsync()`, then `ScrollAsync(0, 3000)` to the bottom of the page, and
     `ClickAsync(button "Publish")` (the green button, `Content.PublishButton.cshtml:19`, the main half of the split
     button). Use `ClickAndWaitForNavigationAsync`: the browser returns to the content list.
  2. `NavigateAsync("/")`. Wait until `.masthead-content` contains the typed message and `#submit-button` reads
     "Keep Me Posted" (shown in capital letters).

  Visible when the narration ends: the front end with the typed message, the button KEEP ME POSTED, the notice about
  reCAPTCHA, and the social icons. The whole step has to fit in the narration: if the navigation is slow, shorten the
  scrolling, not the final hold.

## Clip `05-recaptcha` (chapter 05, Sign-ups)

Starts on the admin home page, `/Admin`. Open it before `StartCaptureAsync`. The step goes to the settings page
directly: the narration names the menu path, the page it opens is `/Admin/Settings/recaptcha`
(`OrchardCore.Settings/Controllers/AdminController.cs:50` with the group id `recaptcha`).

- `signup-3` (about 12 s). Actions:
  1. `NavigateAsync("/Admin/Settings/recaptcha")`. Wait until the labels "Site Key" and "Secret Key" are visible
     (`OrchardCore.ReCaptcha/Views/ReCaptchaSettings.Edit.cshtml:4` and `:13`).
  2. `HighlightAsync(the "Site Key" field, "Site Key")`, hold 3 s.
  3. `ClearHighlightsAsync()`, then `HighlightAsync(the "Secret Key" field, "Secret Key")`, hold until the narration
     ends. Do **not** type anything: both fields stay empty, and the page is not saved.

  Visible when the narration ends: the two empty fields, the Secret Key one ringed, and the hint "Get your reCaptcha
  at" followed by the address of Google's reCaptcha page, which is part of the screen.

## Clip `06-theme` (chapter 06, Launch the site)

Starts on `/Admin/Themes` (the page titled "Themes", `OrchardCore.Themes/Views/Admin/Index.cshtml:12`). Open it before
`StartCaptureAsync`, and wait until the cards are drawn. The cards show the themes of the site: The Admin Theme and
The Coming Soon Theme first (the two current ones), then The Agency Theme, The Blog Theme, The Default Theme and Safe
Mode Theme. Their names and descriptions come from the theme manifests (`TheComingSoonTheme/Manifest.cs:5`).

- `launch-2` (about 10 s). Actions:
  1. `HighlightAsync(the card of "The Coming Soon Theme")`, or the text "This is the current Site theme" in it
     (`OrchardCore.Themes/Views/Items/ThemeEntry-Current.SummaryAdmin.cshtml:13`), hold 3 s.
  2. `ClearHighlightsAsync()`, then `MoveToAsync(the "Make Current" button of the card "The Default Theme")` and
     `HighlightAsync(that button, "Make Current")`
     (`ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`). Do **not** click it: the site keeps the Coming Soon
     theme.

  Visible when the narration ends: the Themes page with a ring on a "Make Current" button.

## Checks for private data

- Only fake data appears: the site name `Example Garden Supply`, the user `admin`, addresses at `example.com`. The
  email `admin@example.com` and the password are typed off camera only; check that no frame of `02-recipe` shows the
  Super User fields filled in.
- The social icons link to `https://twitter.com`, `https://facebook.com` and `https://instagram.com` (the recipe's own
  addresses) until `04-edit` replaces the first one; these are not personal data.
- OCR the video at one frame per second and at every scene change, as the skill says, and search for real names, email
  addresses, phone numbers and tokens. The expected texts are: the site name, `admin`, `example.com`, "Coming Soon",
  the recipe and theme names.
- No sign-up is submitted, so the Users list holds only `admin`; do not open it on camera.
