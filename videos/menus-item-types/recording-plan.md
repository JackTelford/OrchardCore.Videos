# Recording plan: Menu items

Read only this file to record the video. Every source reference is `file:line` in `OrchardCMS/OrchardCore` at commit
`92db4a83`, under `src/OrchardCore.Modules/OrchardCore.Menu/` unless the path starts with `src/`. In this file a selector
such as "the Add button of the card Link Menu Item" stands for `scene.Page.Locator(...)` (or `GetByRole`, `GetByLabel`),
since the recorder's methods take locators. The audience is `admin`: the narration shows the admin UI and the front end,
and never code. The one place where markup appears is the text typed into the **Html** field (clip `05-html`).

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore` at the commit above (or a later `main`), running
  `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). Outbound calls are blocked with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Provisioning**: Auto Setup on the first request, with no setup screen and no sign-in on camera (the video is not
  about the Setup screen). The Auto Setup environment variables of the Default tenant:

  | Variable (prefix `OrchardCore__AutoSetup__Tenants__0__`) | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

  Source of the recipe: `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` (git-ignored).
  The clip `07-visitor` does not use it (see that clip).
- **Tenant**: Default only.
- **Features to enable**: none. The Blog recipe enables `OrchardCore.Menu` and what it needs (Contents, Title, Alias,
  Autoroute, Html, Widgets, DynamicCache, the Menu's recipes) and sets **TheBlogTheme** as the site theme
  (`blog.recipe.json:32-83`). Check on Features that Menu is enabled before recording.
- **Settings**: none to change.
- **External services**: none. The link "Editor Guide" points to `https://docs.orchardcore.net/en/latest/` and is never
  opened; no clip clicks a link of the front end.
- **What the recipe already creates** (`blog.recipe.json:815-855`): a published menu named **Main Menu** (alias
  `main-menu`) with two Link Menu Items, **Home** (`~/`) and **About** (`~/about`), and an Article **About**. TheBlogTheme
  renders the menu in its header with a 5-minute cache that varies by role (`TheBlogTheme/Views/Layout.liquid:32`).
  Publishing the menu clears that cache (`OrchardCore.Alias/Handlers/AliasPartHandler.cs:59-62`).

### Seed data (create it outside the capture, once)

One fake, published Article that the Content Menu Item points at. It is the only seed.

- In the admin: **Content, Content Items**, then **New**, **Article**. Title `Opening Hours`; in the body (any text) type
  `The shop is open every day from 9 a.m. to 5 p.m.`; leave the other fields; **Publish**. The Article type builds its
  path from the title (`blog.recipe.json:158`), so the item is at `/opening-hours`.
- Or import this with **Tools, Deployments, JSON Import** (paste it in the field labeled **JSON**, then **Import**;
  `OrchardCore.Deployment/AdminMenu.cs:30`, `Views/Import/Json.cshtml:15,21`):

  ```json
  {
    "steps": [
      {
        "name": "content",
        "data": [
          {
            "ContentItemId": "4openinghours0000000000a1b",
            "ContentType": "Article",
            "DisplayText": "Opening Hours",
            "Latest": true,
            "Published": true,
            "TitlePart": { "Title": "Opening Hours" },
            "AutoroutePart": { "Path": "opening-hours" },
            "HtmlBodyPart": { "Html": "<p>The shop is open every day from 9 a.m. to 5 p.m.</p>" }
          }
        ]
      }
    ]
  }
  ```

Check before recording: `http://localhost:5010/opening-hours` shows the article, and the front page header lists exactly
**Home** and **About**. The Main Menu has no other item.

### Reaching the menu editor (used by every clip)

The Main Menu editor is **Content, Menus**, then **Edit** on the row **Main Menu**. Its address contains the menu's
content item id, which differs on every site. Compute it once per take:

1. Open `/Admin/Contents/ContentItems/Menu` (the list that **Content, Menus** opens; `AdminMenu.cs:11-19,33-34`).
2. Read the `href` of the **Edit** button of the row **Main Menu**, and remove its query string (the `returnUrl`).
   The result, `MENU_EDIT_URL`, looks like `/Admin/Contents/ContentItems/<id>/Edit`.

Opened without `returnUrl`, the editor shows a plain **Publish** button (no "and continue" arrow), the same editor the
Add and Edit forms redirect to (`Controllers/AdminController.cs:139,239`). Each clip below that says "opens
`MENU_EDIT_URL`" does it before `StartCaptureAsync`, with `scene.OpenAsync`.

Locators used by several clips:

- **Add** of a card in the window **Available Menu Items**: a link `a.thumbnail-link-create` inside the card whose title
  is the type (`Views/MenuItem.Thumbnail.cshtml:16`). Its address is `/Admin/Menu/Create/<Type>?menuContentItemId=<id>`
  (`Controllers/AdminController.cs:17`, `Views/MenuItem.Thumbnail.cshtml:16`), so the address does not *end* with the type:
  find the card by its title (`#modalMenuItems .card` filtered by the text of its `h4`) and take the link in that card.
  The rows of the menu have their own **Add** buttons (`<button>`s, `Views/MenuItem.Admin.cshtml:40`); do not use them.
- **Publish** (of a menu item form and of the menu editor): `button[name="submit.Publish"]`
  (`Content.PublishButton.cshtml:7`). Match it exactly: the menu editor of a published menu without a draft also shows
  **Unpublish** (`Content.UnpublishButton.cshtml`), and a text match on "Publish" finds both. The buttons sit in a bar fixed
  to the bottom of the window (`src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss:14-20`), so no
  scrolling is needed to reach them.
- The notification after a publication is a toast that hides itself after 5 seconds (see `publish-2`).

## Order, and what each clip changes

The clips depend on each other and must be recorded in this order. Copy `App_Data` aside (with the site stopped) at each
snapshot, and restore the right one before a new take of a clip.

| Order | Clip | Starts from | Changes the site | Needs |
|---|---|---|---|---|
| 0 | (seed) | a fresh Blog site | creates the Article Opening Hours | take **snapshot A** now |
| 1 | `02-dialog` | snapshot A | no | |
| 2 | `03-link` | snapshot A | adds the item Editor Guide to the draft of the Main Menu | after it, take **snapshot B** |
| 3 | `04-content` | snapshot B | adds the item Hours | Editor Guide; take **snapshot C** |
| 4 | `05-html` | snapshot C | adds the item Contact us | Hours; take **snapshot D** |
| 5 | `06-permissions` | snapshot D | adds the permission to Editor Guide | the three items; take **snapshot E** |
| 6 | `07-publish` | snapshot E | publishes the Main Menu | the permission; take **snapshot F** |
| 7 | `07-visitor` | snapshot F | no | the published menu |

To record `02-dialog` again, no snapshot is needed. To record any other clip again, restore the snapshot it starts from
and start the site again.

## Chapter 00: opening card

Step `intro`: the opening card, "Menu items" over "Links, content items, HTML and permissions". Nothing to record.

## Chapter 01: three types of menu item (slides, nothing to record)

- `types-1`: the slide "Three types of menu item" with three cards, all bright: Link Menu Item, Content Menu Item, Html Menu Item.
- `types-2`: the slide "Who sees a menu item", first card (Permissions) bright.
- `types-3`: the same slide, second card (Check content permissions) bright.
- `types-4`: the same slide, third card (Sanitize Html) bright.

## Chapter 02: add a menu item, clip `02-dialog`

Starts on: `scene.OpenAsync("/Admin")`, the admin dashboard, signed in, the admin menu at its default state. The cursor is
idle. Starts the capture on the dashboard. This clip changes nothing.

**`dialog-1`**
- Actions: click **Content** in the admin menu (it opens its group); click-and-wait **Menus** (with
  `ClickAndWaitForNavigationAsync`; the list of menus opens); click-and-wait the **Edit** button of the row **Main Menu**
  (the Main Menu editor opens); wait until the **Add Menu Item** button and the rows **Home** and **About** are shown;
  `HighlightAsync` the **Add Menu Item** button, no caption. If the admin menu keeps **Content** open already, skip the first click.
- Visible when the narration ends: the Main Menu editor with the heading of the menu, the rows **Home** (`~/`) and
  **About** (`~/about`), each with the buttons **Edit**, **Add** and **Delete**, and a green ring around **Add Menu Item**.
- Labels from: **Content**, **Menus** `AdminMenu.cs:33-34`; **Add Menu Item** `Views/MenuPart.Edit.cshtml:31`;
  **Edit** of a list row `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`;
  row buttons `Views/MenuItem.Admin.cshtml:37,40,49`.

**`dialog-2`**
- Actions: `ClearHighlightsAsync`; click **Add Menu Item**; wait until the window `#modalMenuItems` is shown; move the
  cursor over the titles of the three cards one after the other (`MoveToAsync`, about one second each). Leave the window open.
- Visible when the narration ends: the window titled **Available Menu Items** with three cards, **Link Menu Item**
  ("Links to a custom url."), **Content Menu Item** ("Links to a Content url.") and **Html Menu Item** ("Links to a custom
  url with HTML."), each with an **Add** button, and **Cancel** in the footer.
- Labels from: `Views/MenuPart.Edit.cshtml:60,79`; `Views/LinkMenuItemPart.Thumbnail.cshtml:1-2`;
  `Views/ContentMenuItemPart.Thumbnail.cshtml:1-2`; `Views/HtmlMenuItemPart.Thumbnail.cshtml:1-2`;
  **Add** `Views/MenuItem.Thumbnail.cshtml:16`. Do not rely on the order of the cards (see OPEN-QUESTIONS.md).

## Chapter 03: Link Menu Item, clip `03-link`

Starts on: `scene.OpenAsync(MENU_EDIT_URL)` (snapshot A): the Main Menu editor with the rows **Home** and **About**.
Changes the site.

**`link-1`**
- Actions: click **Add Menu Item**; wait for the window; click-and-wait **Add** on the card **Link Menu Item** (the link
  `a.thumbnail-link-create` of that card, whose path is `/Admin/Menu/Create/LinkMenuItem`); on the form, `TypeAsync` the field
  **Name** with `Editor Guide`, then the field **Url** with `https://docs.orchardcore.net/en/latest/`.
- Visible when the narration ends: the form of a Link Menu Item, titled with "New Link Menu Item" in the page title
  (`Views/Admin/Create.cshtml:20`), with **Name** `Editor Guide` and **Url** `https://docs.orchardcore.net/en/latest/`,
  and **Target** still empty.
- Labels from: `Views/LinkMenuItemPart.Edit.cshtml:4` (Name), `:11` (Url), `:18` (Target).

**`link-2`**
- Actions: `TypeAsync` the field **Target** with `_blank`, then press Tab; `HighlightAsync` the field **Target** (no
  caption).
- Visible when the narration ends: **Target** holds `_blank`, with the hint "The target attribute of the A tag, see more:
  target" under it, and a ring around the field. (The list of suggestions of the field is a browser list that a headless
  browser does not draw; the narration does not depend on it.)
- Labels from: `Views/LinkMenuItemPart.Edit.cshtml:18-27` (the suggestion for `_blank` is described at `:23`).

**`link-3`**
- Actions: `ClearHighlightsAsync`; click-and-wait **Publish**; wait until the Main Menu editor shows a row **Editor Guide**; `HighlightAsync` that row.
- Visible when the narration ends: the Main Menu editor with the rows **Home**, **About** and **Editor Guide** (with
  `https://docs.orchardcore.net/en/latest/` and the badge "Link"), a ring around the last one.
- Labels from: **Publish** `src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`, added by
  `src/OrchardCore.Modules/OrchardCore.Contents/Drivers/ContentsDriver.cs:110`; the redirect to the editor
  `Controllers/AdminController.cs:139`; the row `Views/LinkMenuItemPart.Admin.cshtml:5`.

## Chapter 04: Content Menu Item, clip `04-content`

Starts on: `scene.OpenAsync(MENU_EDIT_URL)` (snapshot B): the rows **Home**, **About**, **Editor Guide**. Changes the site.

**`content-1`**
- Actions: click **Add Menu Item**; wait for the window; click-and-wait **Add** on the card **Content Menu Item** (the
  link of that card, whose path is `/Admin/Menu/Create/ContentMenuItem`).
- Visible when the narration ends: the form of a Content Menu Item with the fields **Name**, **Selected Content Item** and
  the checkbox **Check content permissions** (unchecked), whatever their order on the page.
- Labels from: `Views/ContentMenuItemPart.Edit.cshtml:4,14`; **Selected Content Item**
  `Migrations/menu.recipe.json:195`.

**`content-2`**
- Actions: `TypeAsync` the field **Name** with `Hours`; click the picker under **Selected Content Item** (placeholder
  "Type to search"); type `Opening`; wait until a result **Opening Hours** is listed; click it; wait until it is shown as
  the selected item.
- Visible when the narration ends: **Name** `Hours`, and **Selected Content Item** listing **Opening Hours**.
- Labels from: placeholder `src/OrchardCore.Modules/OrchardCore.ContentFields/Services/ContentFieldsJSLocalizer.cs:40`;
  results `src/OrchardCore.Modules/OrchardCore.ContentFields/Services/DefaultContentPickerResultProvider.cs:36-51`
  (the picker searches by display text or content type).

**`content-3`**
- Actions: `HighlightAsync` the field **Selected Content Item** (no caption) and hold.
- Visible when the narration ends: a ring around **Selected Content Item** showing **Opening Hours**; no field for an address.

**`content-4`**
- Actions: `ClearHighlightsAsync`; `HighlightAsync` the checkbox **Check content permissions** and its hint (leave it
  unchecked; do not click it); `PauseAsync` until about two seconds before the narration ends; `ClearHighlightsAsync`;
  click-and-wait **Publish**; wait until the editor shows a row **Hours**.
- Visible when the narration ends: the Main Menu editor with the rows **Home**, **About**, **Editor Guide**, **Hours**;
  the row **Hours** shows `/opening-hours` and the badge "ContentLink".
- Labels from: `Views/ContentMenuItemPart.Edit.cshtml:14-15` (the hint says to cache the menu when the option is on);
  the row `Views/ContentMenuItemPart.Admin.cshtml:10`.

## Chapter 05: Html Menu Item, clip `05-html`

Starts on: `scene.OpenAsync(MENU_EDIT_URL)` (snapshot C): the rows **Home**, **About**, **Editor Guide**, **Hours**. Changes the site.

**`html-1`**
- Actions: click **Add Menu Item**; wait for the window; click-and-wait **Add** on the card **Html Menu Item** (the link of
  that card, whose path is `/Admin/Menu/Create/HtmlMenuItem`); then `MoveToAsync` the fields **Name**, **Url**, **Target** and **Html** in turn (about
  0.6 s each), and `HighlightAsync` the field **Html** (no caption).
- Visible when the narration ends: the form of an Html Menu Item with **Name**, **Url**, **Target** and **Html**, all empty,
  a ring around **Html**.
- Labels from: `Views/HtmlMenuItemPart.Edit.cshtml:4,11,18,32`.

**`html-2`**
- Actions: `ClearHighlightsAsync`; `TypeAsync` **Name** with `Contact us`; `TypeAsync` **Url** with `mailto:hello@example.com`.
- Visible when the narration ends: **Name** `Contact us`, **Url** `mailto:hello@example.com`.
- The editor accepts this address: `src/OrchardCore/OrchardCore.Infrastructure/Html/OrchardCoreBuilderExtensions.cs:22` allows
  the `mailto` scheme and `Drivers/HtmlMenuItemPartDisplayDriver.cs:72-90` validates the address with it.

**`html-3`**
- Actions: `TypeAsync` the field **Html** with `<em>Contact</em> us` (this one short tag is the only markup of the
  video; the narration never reads it); `HighlightAsync` the field **Html**. The tag is `em` and not `strong`: the theme
  sets the weight of every header link to 800 (`TheBlogTheme/wwwroot/css/styles.css:11057-11062`) and loads Open Sans only
  up to 800 (`TheBlogTheme/Views/Layout.liquid:13`), so bold text would look the same as the rest of the link.
- Visible when the narration ends: the field **Html** holding `<em>Contact</em> us`, with the hint "This field will
  be rendered raw in place of the name; it will be enclosed in the anchor tag." under it.
- Labels from: `Views/HtmlMenuItemPart.Edit.cshtml:32-35`.

**`html-4`**
- Actions: `ClearHighlightsAsync`; `PauseAsync` until about two seconds before the narration ends; click-and-wait
  **Publish**; wait until the editor shows a row **Contact us**.
- Visible when the narration ends: the Main Menu editor with the rows **Home**, **About**, **Editor Guide**, **Hours**,
  **Contact us** (with `mailto:hello@example.com` and the badge "Link").
- Labels from: `Views/HtmlMenuItemPart.Admin.cshtml:5`; the setting named in the narration is
  `Views/HtmlMenuItemPartSettings.Edit.cshtml:7` (not shown in this clip).

## Chapter 06: who sees an item, clip `06-permissions`

Starts on: `scene.OpenAsync(MENU_EDIT_URL)` (snapshot D): five rows. Changes the site.

**`perm-1`**
- Actions: click-and-wait **Edit** on the row **Editor Guide** (the green button of that row); wait for the form;
  `ScrollAsync` until the field **Permissions** is shown; `HighlightAsync` the label and field **Permissions** (no caption).
- Visible when the narration ends: the form of the Link Menu Item **Editor Guide** with **Name**, **Url**, **Target** filled
  and the field **Permissions** (empty, with the picker "Type to search") ringed.
- Labels from: **Edit** `Views/MenuItem.Admin.cshtml:37`; **Permissions** `Views/MenuItemPermissionPart.Edit.cshtml:15`.
  Only the Link and Html item types have this field (`Migrations/menu.recipe.json:84-85,153-154`).

**`perm-2`**
- Actions: `ClearHighlightsAsync`; click the picker of **Permissions** ("Type to search"); `TypeAsync` `Access admin`;
  wait until the option **Access admin panel** is listed; click it; wait until it is shown in the list above the picker.
- Visible when the narration ends: the field **Permissions** with the entry **Access admin panel** (and its trash button)
  and the hint "The permissions required to display this menu item (optional)." under the picker.
- Labels from: placeholder `Services/MenuJSLocalizer.cs:16`; hint `Views/MenuItemPermissionPart.Edit.cshtml:17`; option
  `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminPermissions.cs:8` (the description of the permission).
  Behavior: every listed permission is required, and a hidden item hides its sub items: `MenuShapes.cs:258-308` and
  `:87-91,127-131`.

**`perm-3`**
- Actions: `PauseAsync` about 3 seconds; click-and-wait **Publish**; wait for the Main Menu editor; `HighlightAsync` the row
  **Editor Guide**.
- Visible when the narration ends: the Main Menu editor with the five rows, a ring around **Editor Guide** (the row does not
  show the permission).
- Labels from: `Controllers/AdminController.cs:239` (Edit saves the menu draft and redirects to the editor).

## Chapter 07: publish and check

### Clip `07-publish`

Starts on: `scene.OpenAsync(MENU_EDIT_URL)` (snapshot E): the five rows, the menu is a draft. Changes the site.

**`publish-1`**
- Actions: `MoveToAsync` the rows **Editor Guide**, **Hours** and **Contact us** in turn (about two seconds each);
  then `MoveToAsync` the **Publish** button of the menu (`button[name="submit.Publish"]`, in the bar fixed to the bottom of
  the window) without clicking it.
- Visible when the narration ends: the **Publish** button of the Main Menu editor under the pointer.
- Labels from: **Publish** `Content.PublishButton.cshtml:7`; the menu is saved as a draft by every Add and Edit:
  `Controllers/AdminController.cs:137,237`.

**`publish-2`**
- Actions: `PauseAsync` about 3 seconds (the first sentence is spoken); click-and-wait **Publish** (the menu's); wait until
  the toast `.toast.message-success` is shown; `HighlightAsync` it; `PauseAsync` about 3 seconds; `ClearHighlightsAsync`.
  The toast hides itself 5 seconds after the page loads (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`
  sets 5000 ms, `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml` turns it into `data-bs-autohide`), so the click must
  fall in the first half of the step, and the ring must be cleared before the toast goes, or it circles an empty place.
- Visible when the narration ends: the Main Menu editor (the page reloaded on the menu, not the list of menus). The
  notification "Your Menu has been published." is shown for about 5 seconds after the click, during the second sentence of
  the step, and may be gone by the end of the narration (check the real length of the step in `durations.json`).
- Labels from: `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:967`; no redirect to the list,
  because `MENU_EDIT_URL` has no return address (`:841-847`).

**`publish-3`**
- Actions: `ClearHighlightsAsync`; `NavigateAsync("/")` (the front end, signed in as the administrator); wait until the
  header menu `#navbarResponsive` lists **Contact**; `HighlightAsync` `#navbarResponsive` (no caption).
- Visible when the narration ends: the blog's front page with the header menu **Home**, **About**, **Editor Guide**,
  **Hours**, and **Contact** in italics followed by "us", with a ring around it. The theme writes every header link in
  capitals (`text-transform: uppercase`, `TheBlogTheme/wwwroot/css/styles.css:11057-11062`), so the screen reads HOME,
  ABOUT, EDITOR GUIDE, HOURS and CONTACT US: that is expected, not a wrong label. The viewport of the recorder (1536 x 864,
  `Scene.cs:14-15`) is wide enough for the header menu (the theme collapses it behind a **Menu** toggle below 992 px,
  `TheBlogTheme/Views/Layout.liquid:25,28`).
- Source: `TheBlogTheme/Views/Menu.liquid:1`, `MenuItemLink-HtmlMenuItem.liquid`; the Html is cleaned by
  `MenuShapes.cs:157-190,227-248` when Sanitize Html is on (its default, `Settings/HtmlMenuItemPartSettings.cs`).

### Clip `07-visitor` (a signed-out visit)

Start this scene without the sign-in: call `Scene.StartAsync` (what `NewSceneAsync` calls) with a storage state path of a file
that does not exist, so `Scene.cs:109` opens a context with no cookies. Then `scene.OpenAsync("/")` outside the capture, and
`StartCaptureAsync`. This clip changes nothing.

**`visitor-1`**
- Actions: `HighlightAsync` `#navbarResponsive` (no caption); hold.
- Visible when the narration ends: the same front page, signed out, with the header menu **Home**, **About**, **Hours**,
  **Contact** (in italics) us, and no **Editor Guide**, with a ring around the menu (in capitals, as in `publish-3`).
- Source: `MenuShapes.cs:258-308` (the permission check), cache varying by role `TheBlogTheme/Views/Layout.liquid:32`.

## Chapter 99: closing card

Step `outro`: the closing card. Nothing to record.

## Checks for private data

- Fake data only: the site name `Orchard Core`, the user `admin`, `admin@example.com`, `hello@example.com`, the Article
  Opening Hours, the item names above. The only external address is `docs.orchardcore.net`.
- No password is typed or shown: the sign-in is `state.json` (git-ignored); `<password>` is never replaced by a value in
  any file of this folder.
- Look at every contact sheet (`review.py`) for: an error message after Publish (an "invalid url" message means the address
  was rejected), an empty menu, a stale header menu on the front end (publish clears it; reload once if it is stale), the
  user's real name or email in the admin bar, and a browser notification.
- Before publishing, run the OCR scan of the skill (one frame per second and at every scene change) and search for
  addresses other than `example.com`, `orchardcore.net` and `docs.orchardcore.net`, and for names of real people.
