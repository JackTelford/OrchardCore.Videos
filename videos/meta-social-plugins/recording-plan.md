# Recording plan: Meta Social Plugins: Like, Share and Comments Widgets

For the person who records this video. Read this file, `storyboard.json` and `OPEN-QUESTIONS.md`; nothing else is
needed. In this file a selector stands for `scene.Page.Locator(...)`. File references are to the Orchard Core source
the content was written from (commit `92db4a83`), relative to its `src` folder, written `File:line`.

The video has four recordings (`02-enable`, `03-app`, `04-widgets`, `05-check`) and two slide chapters (`01-idea`,
`06-rights`) that need no recording.

## Before anything else

Read `OPEN-QUESTIONS.md`. The last recording shows plugins drawn by Meta's own script, and nobody has checked yet
what Meta draws for a made-up application identifier on a local address. Do the pre-flight check at the end of
"The site it needs" before recording anything; if it fails, stop and report it, and do not change the narration.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`, `src/OrchardCore.Cms.Web`, run on
  `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (the site is provisioned by Auto Setup, so no Setup screen is recorded), with these environment
  variables (section `AutoSetup`, `OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:26`):

  ```
  OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
  OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `<password>` is the value chosen outside git and passed in `VIDEO_PASSWORD`. The recorder signs in once with
  `Recorder setup` (user `admin`) and keeps the session in `state.json` (not committed).
- **Recipe**: Blog (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It supplies everything the video
  uses, so no seed data is created:
  - the Layers, Widgets and Rules features are enabled (`blog.recipe.json:31-82`);
  - the zones are `Content` and `Footer` (`blog.recipe.json:121-124`);
  - the layers are `Always` (a rule that is always true) and `Homepage` (`blog.recipe.json:1144`, `:1159`);
  - the blog post at `/blog/post-1` (`blog.recipe.json:1018`) and the menu links `Home` and `About`
    (`blog.recipe.json:834`, `:843`);
  - one widget of type Raw Html named `Footer`, in the `Footer` zone on the `Always` layer, with the three social
    icons and a copyright line (`blog.recipe.json:1090-1105`, `Recipes/Snippets/footer.html`).
- **Features before recording**: only those of the Blog recipe. The Meta features are disabled (the `02-enable`
  clip enables them on camera). Nothing else is enabled by hand, and the Secrets feature stays disabled, so that the
  Secret Key field is a plain password field.
- **No `PrepareAsync`**: the site needs no seed outside the capture.
- **Fake values typed on camera** (all made up):
  - Application Identifier: `123456789012345`
  - Secret Key: `example-secret-key` (it shows as dots)
  - Widget titles: `Like this post`, `Share this post`, `Comments`
- **External services**: none for the site. Its outbound calls can be blocked with `HTTP_PROXY` and `HTTPS_PROXY`
  set to a dead address for the site process; the feature makes no server-side call. The **browser** is not behind
  that proxy and must reach Meta's servers, because Meta's script and the plugin frames load in the page (the
  address of the script is built at `OrchardCore.Modules/OrchardCore.Facebook/Endpoints/GetSdkEndpoints.cs:120`). No Meta account is
  needed and none is signed in: use the recorder's fresh browser context.
- **Warm up** (outside the capture, on the fresh site): open `/Admin`, `/Admin/Features`, `/Admin/Layers` and
  `/blog/post-1` once, so the first requests of the take are not the slow ones. Do not enable anything while warming up.
- **Pre-flight check** (on a throwaway copy of the site, never on the take): enable Meta Social Plugins Widgets, save
  the Meta App settings with the values above, add a Facebook Like widget to the Footer zone on the Always layer, and
  open `/blog/post-1`. Wait up to 15 seconds. The footer must show a frame (an `iframe`) drawn by Meta for the
  widget. If it does not, stop: see question 1 of `OPEN-QUESTIONS.md`. Delete the throwaway copy.

## Order, snapshots and what each clip changes

Record in this order, because each clip starts from what the previous one left:

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | snapshot S0 (fresh site, warmed up) | enables Meta Social Plugins Widgets and Meta Core Components; the tenant restarts |
| 2 | `03-app` | snapshot S1 | saves the Meta App settings; the tenant restarts |
| 3 | `04-widgets` | snapshot S2 | publishes three widgets |
| 4 | `05-check` | snapshot S3 | nothing |

Snapshots: stop the site, copy the whole `ORCHARD_APP_DATA` folder (and `state.json`) aside, start the site again.
Take S0 right after the warm-up. After a good take of a clip, stop the site, copy `App_Data` as the next snapshot
(S1 after `02-enable`, S2 after `03-app`, S3 after `04-widgets`) and start it again. To record a clip again, stop
the site, restore the snapshot that precedes the clip, start the site, and record. To record everything again,
restore S0.

## Clip `02-enable` (chapter 02)

Starts on `/Admin` (the dashboard), signed in, with the admin menu groups collapsed. Open it with `scene.OpenAsync("/Admin")`,
then `StartCaptureAsync()`.

- `enable-1`
  1. `ClickAsync` the group **Tools** in the admin menu: `#left-nav` element with the exact text `Tools`
     (`OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`). Wait for the group to open (the item **Features** is visible).
  2. `ClickAndWaitForNavigationAsync` the link **Features** inside `#left-nav`
     (`OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:27`). Wait until `#search-box` is visible.
  3. `TypeAsync(#search-box, "Meta")` (`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`). The list filters as
     you type; wait until the heading `h3` with the exact text `Meta` is visible.
  4. `HighlightAsync` the row that contains the text `Meta Social Plugins Widgets` (name and description from
     `OrchardCore.Modules/OrchardCore.Facebook/Manifest.cs:34-37`).
  - On screen when the narration ends: the search box with `Meta`, the category heading **Meta**, and the row
    **Meta Social Plugins Widgets** with its description and its **Enable** button, ringed. Other rows that contain
    the letters "meta" in their text may be listed too; leave them. **Meta Core Components** is not listed (it is
    hidden by the default visibility filter).
- `enable-2`
  1. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` the **Enable** link of that row:
     `#btn-enable-OrchardCore_Facebook_Widgets` (the id is the feature id with the dots turned into underscores,
     `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). If that id is not found, use the
     **Enable** link inside the row that contains the text `Meta Social Plugins Widgets`.
  2. The tenant restarts when features are enabled, so the page can take several seconds. That wait is on camera;
     it is part of the story. Wait until the text `was enabled` is visible (the message is
     `The feature Meta Social Plugins Widgets was enabled.`, `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`).
  3. `TypeAsync(#search-box, "Meta")` again, so the list shows the row.
  - On screen when the narration ends: the green message at the top, and the row **Meta Social Plugins Widgets**
    with the badge **Enabled** and a **Disable** button.

After this clip: stop the site, save snapshot S1, start the site.

## Clip `03-app` (chapter 03)

Starts on `/Admin` (dashboard), signed in. Menu groups collapsed.

- `app-1`
  1. `ClickAsync` the group **Settings** in `#left-nav` (`OrchardCore.Modules/OrchardCore.Facebook/AdminMenu.cs:25`), then the group
     **Integrations** (`:26`), then `ClickAndWaitForNavigationAsync` the link **Meta App** (`:27`).
     Wait until the heading `h3` with the text `Meta Integration` is visible
     (`OrchardCore.Modules/OrchardCore.Facebook/Views/FacebookSettings.Edit.cshtml:15`).
  2. `HighlightAsync` the label `Application Identifier` (`FacebookSettings.Edit.cshtml:20`), then the label
     `Secret Key` (`:29`), one after the other.
  - On screen when the narration ends: the page titled **Settings** with the warning, the heading **Meta
    Integration**, the fields **Application Identifier**, **Secret Key**, **Javascript SDK Version**,
    **Javascript Sdk js**, the check box **Init on every page** and **Parameters for the FB.init() call**.
- `app-2`
  1. `ClearHighlightsAsync()`.
  2. `TypeAsync(scene.Page.GetByLabel("Application Identifier", new() { Exact = true }), "123456789012345")`.
  3. `TypeAsync(scene.Page.Locator("input[type=password]"), "example-secret-key")` (the only password field of the
     page; with the Secrets feature disabled it is a plain password input, `OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96`).
  - On screen when the narration ends: the identifier in clear digits and the secret as dots.
- `app-3`
  1. `HighlightAsync` the fields **Javascript SDK Version** and **Javascript Sdk js** (a ring around both inputs,
     for example one ring on each), then the check box **Init on every page**. Do not change any of them.
  - On screen when the narration ends: the default values (the version field shows its default, the file field shows
    `sdk.js`), and **Init on every page** unchecked, ringed. Defaults: `OrchardCore.Modules/OrchardCore.Facebook/Settings/FacebookSettings.cs:18-27`.
- `app-4`
  1. `ClearHighlightsAsync()`; `HighlightAsync` the warning (`.alert.alert-warning` at the top of the form,
     text from `FacebookSettings.Edit.cshtml:7`); then `ClearHighlightsAsync()`.
  2. `ClickAndWaitForNavigationAsync` the button `button.save` (label **Save**,
     `OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). The tenant is asked to restart, so the page can
     take a few seconds; that is on camera. Wait until the text `Site settings updated successfully.` is visible
     (`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`).
  - On screen when the narration ends: the green message `Site settings updated successfully.`, the identifier still
    in the field, and the secret field empty with a placeholder saying the value is securely stored.

After this clip: stop the site, save snapshot S2, start the site.

## Clip `04-widgets` (chapter 04)

Starts on `/Admin` (dashboard), signed in. The three widgets are created in this clip, one after the other. For
each widget the same actions apply (the widget type, the title and the narration step change).

Shared actions for adding a widget to the Footer zone:

1. Open the dropdown: `ClickAsync` the button **Add Widget** of the Footer zone:
   `.layer-zone[data-zone="Footer"] button` with the text `Add Widget`
   (`OrchardCore.Modules/OrchardCore.Layers/Views/Admin/Index.cshtml:85`). The Footer zone is the second card; the first is Content.
2. Choose the type: `ClickAndWaitForNavigationAsync` the item
   `.layer-zone[data-zone="Footer"] a.add-list-widget[data-widget-type="<TypeName>"]`, with `<TypeName>` one of
   `FacebookLike`, `FacebookShare`, `FacebookComments`. The dropdown scrolls inside (it lists every widget type,
   sorted by display name, with a maximum height); the scene scrolls the item into view before it clicks.
   (`Index.cshtml:77`, `:99`; display names from `OrchardCore.Modules/OrchardCore.Facebook/Migrations/Widgets/migration.recipe.json:18`, `:52`, `:120`.)
3. The editor opens with the breadcrumb `New <display name>` (`OrchardCore.Modules/OrchardCore.Contents/Views/Admin/Create.cshtml:26`).
   `TypeAsync` the field **Title** (`scene.Page.GetByLabel("Title", new() { Exact = true })`,
   `OrchardCore.Modules/OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:4`). Leave **Render title** unchecked.
4. `SelectAsync` the drop-down **The layer** (`select` labeled `The layer`, `LayerMetadata.Edit.cshtml:22`) with the
   label `Always`.
5. `ClickAndWaitForNavigationAsync` the button `button.publish[value="submit.Publish"]`, label **Publish**
   (`OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`). The page returns to the Widgets and Layers page
   with the message `Your <display name> has been published.` (`OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:473`).

Clip steps:

- `add-1`
  1. Open the menu: `ClickAsync` the group **Design** in `#left-nav` (`OrchardCore.Modules/OrchardCore.Layers/AdminMenu.cs:26`), then
     `ClickAndWaitForNavigationAsync` the link **Widgets** (`:27`). Wait until the heading `h3` `Zones` is visible
     (`Index.cshtml:34`).
  2. `HighlightAsync` the heading **Zones**, then the heading **Layers** (`Index.cshtml:121`).
  - On screen when the narration ends: the page titled **Widgets and Layers** (`Index.cshtml:20`) with the cards
    **Content** and **Footer** (the Footer card shows the existing widget **Footer** with the badge **Always**), and
    on the right the layers **Always** and **Homepage**.
- `add-2`
  1. `ClearHighlightsAsync()`. Open the dropdown of the Footer zone (shared action 1).
  2. `MoveToAsync` the item `FacebookLike`, which scrolls the list to it; `HighlightAsync` it with no caption.
     Click it with `ClickAndWaitForNavigationAsync` (shared action 2).
  - On screen when the narration ends: the editor **New Facebook Like**, with the fields **Title**, **Render
    title** and **The layer** and the **Body** field.
- `add-3`
  1. `TypeAsync` **Title** with `Like this post` (shared action 3).
  2. `HighlightAsync` the check box **Render title**, leave it unchecked, `ClearHighlightsAsync()`.
  3. `SelectAsync` **The layer** with `Always` (shared action 4).
  - On screen when the narration ends: the title typed, **Render title** unchecked, and `Always` selected in **The layer**.
- `add-4`
  1. `ScrollAsync(0, 400)` if needed so the **Body** field and the **Publish** button are both visible
     (`OrchardCore.Modules/OrchardCore.Facebook/Views/FacebookPluginPart.Edit.cshtml:12`; the field is filled from the widget type,
     `OrchardCore.Modules/OrchardCore.Facebook/Widgets/Drivers/FacebookPluginPartDisplayDriver.cs:67`). The field shows the plugin's
     markup; the narration does not read it. `HighlightAsync` the label **Body** only (not the text area).
  2. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` **Publish** (shared action 5).
  - On screen when the narration ends: the Widgets and Layers page, the message `Your Facebook Like has been
    published.`, and the Footer card listing the widget **Like this post** (badge **Always**) under the existing one.
- `add-5`
  1. `HighlightAsync` the new widget in the Footer card, `ClearHighlightsAsync()`.
  2. Add Facebook Share: shared actions 1 to 5 with type `FacebookShare` and title `Share this post`, layer `Always`.
     Move quickly (no highlights): the actions must be shorter than the narration.
  - On screen when the narration ends: the Widgets and Layers page with the message `Your Facebook Share has been
    published.` and the Footer card listing **Like this post** and **Share this post**.
- `add-6`
  1. Add Facebook Comments: shared actions 1 to 5 with type `FacebookComments` and title `Comments`, layer `Always`.
  - On screen when the narration ends: the message `Your Facebook Comments has been published.` and the Footer card
    listing, in this order, the existing **Footer**, then **Like this post**, **Share this post** and **Comments**,
    each with the badge **Always** (the list is ordered by position; each new widget gets the next position,
    `Index.cshtml:94`).

After this clip: stop the site, save snapshot S3, start the site.

## Clip `05-check` (chapter 05)

Starts on `/` (the home page of the Blog recipe, the list of blog posts), signed in as before. Open it with
`scene.OpenAsync("/")`. Meta's script and frames load in the page, so the capture needs the network.

- `check-1`
  1. `ClickAndWaitForNavigationAsync` the link of the post whose title is
     `Man must explore, and this is exploration at its greatest` (`blog.recipe.json:1002`; the page is `/blog/post-1`).
  2. `ScrollAsync(0, 6000)`: the page scrolls to its end (the scroll stops at the bottom of the page).
  3. Wait for Meta to draw: `OffCameraAsync(() => scene.Page.Locator("footer iframe").First.WaitForAsync(new() { Timeout = 15000 }))`
     (the footer is rendered from the `Footer` section of the theme, `TheBlogTheme/Views/Layout.liquid:45`).
  - On screen when the narration ends: the bottom of the post, the footer with the existing social icons and the
    copyright line, and below them the three widgets, in the order Like, Share, Comments, drawn by Meta's script.
    No sign-in prompt of a real account and no real name may be visible: see the checks below.
- `check-2`
  1. `ScrollAsync(0, -6000)` to the top, then `ClickAndWaitForNavigationAsync` the link **About** in the main menu
     (`#mainNav`, `blog.recipe.json:843`).
  2. `ScrollAsync(0, 6000)`, then the same off-camera wait for `footer iframe` as in `check-1`.
  - On screen when the narration ends: the footer of the **About** page with the same three widgets.

## Slides (nothing to record)

- `idea-1`: three cards, **Social plugin**, **Widget**, **Zone and layer**, with one line each.
- `idea-2`: the seven widget names in a grid; **Facebook Comments**, **Facebook Like** and **Facebook Share** are
  bright, and a line says the feature and its settings say Meta while the widgets say Facebook.
- `rights-1`: four cards, the first three bright: **Body field**, **Manage Liquid templates**, **Default roles**.
- `rights-2`: the same four cards, the fourth bright: **Without the permission**.

## Checks for private data

Before handing the clips over:

- The only identity on screen is the user `admin`; the only email, if one appears, ends in `@example.com`. The
  Application Identifier on screen is the made-up `123456789012345`; the secret is dots.
- Look at the frames of `05-check` closely: Meta's frames can show a sign-in prompt or the name of the account that
  is signed in to Meta in that browser. The recorder's context has no Meta sign-in, so a name must not appear; if
  one does, stop and report it. Do not blur by hand.
- No real site address: the address bar is not recorded, and the only host in the page is `localhost`.
- OCR the video (see the skill, "Check for private data") and search for names, emails, phone numbers and tokens
  before the clips are used.
- Do not commit `state.json`, `App_Data`, the snapshots, or any file holding `<password>`.
