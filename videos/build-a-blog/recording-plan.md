# Recording plan: Build a Blog

How to record the clips of this video without deciding anything. Every label below was read in the source of
`OrchardCMS/OrchardCore` at commit `92db4a83`; the `file:line` after a label says where. Paths of the source are
relative to the root of that repository. In this file a selector such as `a.edit` stands for
`scene.Page.Locator("a.edit")`.

Selectors marked **(probe)** were derived from the views and scripts, not seen on a running site: check each one once
with `Recorder probe <path> "<script>"` before recording, and write down any change in the scenes, not in the
narration. `OPEN-QUESTIONS.md` lists what a person must confirm on the running site.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`, `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`
  with its own `ORCHARD_APP_DATA` folder outside both repositories. Block outbound calls from the server with
  `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup**, so no setup or sign-in form is recorded. The host registers the feature itself
  (`src/OrchardCore.Cms.Web/Program.cs:9`), so only the environment variables of the `Default` tenant are needed:

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `<password>` is chosen outside git and also passed to the recorder as `VIDEO_PASSWORD`. The recipe is **Blog**
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:3`, a setup recipe: `:8`).
- **Features to enable**: none. The Blog recipe enables everything the video shows: Autoroute `:50`, Layers `:62`, Lists
  `:63`, Markdown `:64`, Media `:65`, Menu `:66`, Taxonomies `:72`, Widgets `:75`, the Admin Menu `:49`, and it makes
  The Blog Theme the site theme (`:85`).
- **Settings**: none to change. The recipe sets the home page to the Blog (`:114`) and the widget zones to Content and
  Footer (`:121`).
- **External services**: none. The site calls no mail, SMS or payment provider. The layout of The Blog Theme links
  Google Fonts from the browser (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12`); with no internet access
  the page falls back to system fonts, which is acceptable.
- **Seed data**: the recipe is the seed, nothing is created outside the capture. After Auto Setup the site holds, all
  from `blog.recipe.json`:
  - the Blog "Blog" (`:965`), with the description "This is the description of your blog" (`:976`) and the banner image
    `home-bg.jpg` (`:985`), at the permalink `blog` (`:979`), and set as the home page (`:114`);
  - one Blog Post, "Man must explore, and this is exploration at its greatest" (`:1002`), with the subtitle "Problems look
    mighty small from 150 miles up", the category Travel and the tags Earth, Exploration and Space;
  - the taxonomy Categories (`:930`), with the one term Travel (`:940`), and the taxonomy Tags (`:864`), with the terms
    Earth, Exploration and Space (`:874`);
  - the menu Main Menu (`:827`) with the alias `main-menu` (`:852`) and two Link Menu Items, Home (`:836`) and About
    (`:845`);
  - the Article "About", the layer Always (`:1144`), and a Raw Html widget "Footer" (`:1091`) in the zone Footer
    (`:1098`) on the layer Always (`:1097`);
  - the media files `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` (`:1131`, `:1135`), and the custom admin menu entries
    **Blog** (`:1197`), **Main Menu** (`:1217`) and **Content** with **Content Items** and the content types Article,
    Page and Taxonomy (`:1237`, `:1253`, `:1271`).
- **The values typed on camera** (all invented): the category "Photography" with the permalink `photography` and a camera icon; the post "Chasing Golden
  Hour in Lisbon", subtitle "Light worth waiting for", body "The light over the river turns gold for twenty minutes
  every evening."; the tags Exploration (existing) and Lisbon (created while writing); the menu item "Photography"
  pointing to `~/categories/photography`. The author shown on posts is the user `admin`.
- **Sign-in**: `Recorder setup` once, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`. The sign-in file is never
  committed. Use the viewport of the specification (1536 by 864) so the menu of The Blog Theme is expanded.
- **No mask script** is needed: the site is new and holds no real names.

## Snapshots, and the order to record in

Clips are recorded in the order they play, because each one starts from what the previous one left:

| Clip | Starts from | Changes the site |
|---|---|---|
| `02-tour` | the site after Auto Setup | No: it only looks. It must be recorded **before** the others, since it shows one post and one category. |
| `03-category` | the same | Yes: adds the category Photography and publishes the Categories taxonomy |
| `04-post` | after `03-category` | Yes: creates and publishes the post, and creates the tag Lisbon (the Tags taxonomy is published as soon as the tag is created) |
| `05-tags` | after `04-post` | No |
| `06-menu` | after `04-post` | Yes: adds the menu item Photography and publishes the menu. Its last step opens the category page, which needs the category (`03-category`) and the post (`04-post`). |

Copy the `App_Data` folder aside at these points (stop the site first, start it again after copying), and restore the
right copy before every new take:

1. **A**: after Auto Setup and `Recorder setup`. Restore it before `02-tour`, and before a full take from the start.
2. **B**: after `03-category`. Restore it before a new take of `04-post`.
3. **C**: after `04-post`. Restore it before a new take of `05-tags` (not needed, it changes nothing) or `06-menu`.

A new take of `03-category` starts from **A**. After restoring, start the site and open `/Admin` once, so the first
request is not recorded.

## Common to every clip

- **Admin menu**: keep it expanded (labels visible), not reduced to icons, so the viewer can read the entries named in
  the narration. Open `/Admin` before the capture when the clip begins in the admin.
- **Admin entries, as labeled**: **Blog** and **Main Menu** are links built by the recipe (`blog.recipe.json:1197`,
  `:1217`). **Content** (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs`, `S["Content"]`) is a menu that
  the recipe's own **Content** entry is merged into (`NavigationManager.Merge` merges entries with the same caption,
  `src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74`), so it holds **Content Items**, **Menus**
  (`OrchardCore.Menu/AdminMenu.cs:34`), **Article**, **Page** and **Taxonomy**. **Design** then **Widgets** comes from
  `OrchardCore.Layers/AdminMenu.cs:26`, `:27`. **(probe)** how the entries are drawn: use the accessible name, such as
  `GetByRole(AriaRole.Link, new() { Name = "Taxonomy", Exact = true })`.
- **Publish buttons**: the button labeled **Publish** is `button[value='submit.Publish']`
  (`OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`). When the page has a `returnUrl`, **Publish** is the left
  half of a split button and **and continue** is in its drop-down (`:11`-`:24`): always click the left half, never
  `button[value='submit.PublishAndContinue']`. **Save Draft** (`Content.SaveDraftButton.cshtml:7`) is never clicked.
- **Messages**: a published item shows a message "Your {type} has been published." (`OrchardCore.Contents/Controllers/AdminController.cs:473`)
  in the admin. Wait for the message text before the narration of the step ends, so the frame shows it.
- **Typing**: use `TypeAsync` with a delay of 25 to 40 ms, so the actions stay shorter than the narration.
- **No clip leaves a form half filled**: when a clip ends on an editor, the next clip navigates away without saving. If
  the browser asks to confirm leaving the page, the take is wrong: record it again.

## Clip `02-tour`

**Starts on** `/` (the home page, signed in as admin, scrolled to the top). The capture starts there.

- `tour-1`: the home page of the site. At about 4 s, highlight the banner `header.masthead` (the title "Blog" and the
  description); at about 9 s clear the highlights and highlight the post preview `.post-preview`. **On screen when the
  narration ends**: the banner and the one post preview (title, subtitle "Problems look mighty small from 150 miles up",
  "Posted by admin on" and a date, and the badges Travel, Earth, Exploration and Space) in the page. Labels:
  `OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:14`-`:16`, `Content-BlogPost.Summary.liquid:3`, `:13`, `:15`.
- `tour-2`: clear the highlights, highlight the menu `#mainNav .navbar-nav` (`Layout.liquid:25`), then move the cursor over
  Home and About (`MoveToAsync`). **On screen when the narration ends**: the menu with Home and About highlighted at the top
  right, and the name of the site, "Orchard Core", at the top left (`Layout.liquid:27`).
- `tour-3`: click the post title, the first link `.post-preview a` (`.First`: the author and the badges are links too; `ClickAndWaitForNavigationAsync`). When the post page has
  loaded, scroll down until the body text and the badges `.term-badge` under it are in view. Wait for `.term-badge` to be
  visible. **On screen when the narration ends**: the end of the text and the badges (Travel, Earth, Exploration, Space)
  below it. Labels: `Content-BlogPost.liquid:16` (subtitle), `:30` (body), `:31`, `:32` (category and tags).
  Expected: under the badges the page prints the titles of the most recent posts as plain text, one title after the
  other (the query RecentBlogPosts, `Content-BlogPost.liquid:34`-`:37`). It is part of the theme, not a broken page; the same text is
  at the end of the post page in `post-8` and at the start of `05-tags`.
- `tour-4`: `NavigateAsync("/Admin")`, then click the entry **Blog** of the admin menu
  (`ClickAndWaitForNavigationAsync`). Wait for the list of posts. **On screen when the narration ends**: the admin page of the
  Blog, with the post in the list and the two buttons **Edit Blog** and **Create Blog Post** at the top. Labels:
  `OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:43` ("Edit {0}") and `:52` ("Create {0}"), with the content type
  "Blog" and the contained type "Blog Post". The button **List Blog Post Items** is not shown on this page (`:28`).
- `tour-5`: click **Edit Blog** (`ClickAndWaitForNavigationAsync`). In the editor, scroll to the field **Permalink**
  (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`), which holds `blog`, and the note under it "This content
  item is the current homepage" (`:61`). Highlight the note. **On screen when the narration ends**: the Permalink field and
  the note together. Do not change or save anything.
- `tour-6`: click the admin entry **Design**, then **Widgets** (`ClickAndWaitForNavigationAsync`). The page is titled
  "Widgets and Layers" (`OrchardCore.Layers/Views/Admin/Index.cshtml:20`). Highlight the card of the zone **Footer**, which holds
  the widget "Footer" with the badge **Always**. **On screen when the narration ends**: the card Footer with the widget and its
  badge Always highlighted. The zone cards are named after the zones set in `blog.recipe.json:121`-`:123`, the badge after the
  layer of the widget (`Index.cshtml:57`), and the widget after `blog.recipe.json:1091`.

## Clip `03-category`

**Starts on** `/Admin`, with the admin menu showing **Content** collapsed (the narration names the entry).

- `category-1`: click the admin entry **Content**, then **Taxonomy** (`ClickAndWaitForNavigationAsync`; the list at
  `/Admin/Contents/ContentItems/Taxonomy` shows Categories and Tags). Then click **Edit** (`a.edit`,
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`) in the row of **Categories**: the row is
  `scene.Page.Locator("li, tr").Filter(new() { HasText = "Categories" }).Locator("a.edit").First` **(probe)**, then
  `ClickAndWaitForNavigationAsync`. **On screen when the narration ends**: the editor of the Categories taxonomy has loaded.
- `category-2`: scroll so that the field **Term Content Type**, the button **Add Category** and the term list `#menu` are in view
  (`OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:26`, `:48`, `:58`). The list holds Travel with the buttons **Edit**,
  **Add** and **Delete** (`Content.TermAdmin.cshtml:27`, `:36`). At about 5 s highlight the list, at about 11 s highlight the
  button **Add Category** and then the button **Add** of Travel. **On screen when the narration ends**: the button Add
  Category and the row Travel with its three buttons.
- `category-3`: click **Add Category** (`a:has-text("Add Category")`, `ClickAndWaitForNavigationAsync`). The page "New Category" opens.
  Type `Photography` in **Title** (`GetByLabel("Title")`,
  `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`), then type `photography` in **Permalink** (`AutoroutePart.Edit.cshtml:30`).
  **Do not leave Permalink empty**, although its hint says "The url of the content item. Leave empty to auto-generate it."
  (`:46`): a category is added by `OrchardCore.Taxonomies/Controllers/AdminController.cs:118`-`:163`, which saves the term inside
  the taxonomy without running the content handlers, so the pattern of the Autoroute part is never evaluated for it
  (`AutoroutePartHandler.cs:139`-`:149` only generates a path for an item it creates or updates itself). With the field empty
  the category page is not at `/categories/photography`, and the menu item of `06-menu` would lead nowhere. **On screen when the
  narration ends**: the title "Photography" and the Permalink `photography` typed.
- `category-4`: click the caret `.icon-picker-toggle` of the field **Icon**
  (`OrchardCore.ContentFields/Views/TextField-IconPicker.Edit.cshtml:32`) **(probe)**. Pick a camera in the picker: the item
  `.iconpicker-item` that holds `.fa-camera` **(probe)**, using the search box of the picker if it has one. The button left of
  the caret now shows the camera. About 2 s before the narration ends, click **Publish**
  (`ClickAndWaitForNavigationAsync`). The field is required (`blog.recipe.json:806`). **On screen when the narration ends**: the
  Categories editor with the terms Travel and Photography in the list (the page the redirect lands on,
  `OrchardCore.Taxonomies/Controllers/AdminController.cs:165`).
- `category-5`: click **Publish** at the bottom of the Categories editor (`button[value='submit.Publish']`, the editor has no
  `returnUrl` here, so it is a single button). Wait for the message "Your Taxonomy has been published.". **On screen when the
  narration ends**: the message above the editor, with Travel and Photography listed. The taxonomy is saved as a draft until
  this click (`AdminController.cs:110`), and the front end reads the published version
  (`OrchardCore.Taxonomies/Liquid/TaxonomyTermsFilter.cs:47`).

**Changes the site**: restore snapshot **A** before a new take.

## Clip `04-post`

**Starts on** the admin page of the Blog: before the capture, open `/Admin`, click the entry **Blog**, and keep the page.
Read its address once (the address contains the content item id of the Blog) if a later take wants to open it directly.

- `post-1`: click **Create Blog Post** (`a:has-text("Create Blog Post")`, `ClickAndWaitForNavigationAsync`; the address carries a
  `returnUrl`, so **Publish** will be a split button). Type `Chasing Golden Hour in Lisbon` in **Title**
  (`GetByLabel("Title", new() { Exact = true })`: the label **Subtitle** also contains "Title"). Leave **Permalink** empty. **On screen when the narration ends**: the editor with the Title filled, the empty Permalink and the hint under it.
  The permalink is built later from the title of the Blog and the title of the post
  (`blog.recipe.json:216`), and the Permalink field accepts a custom path (`:215`).
- `post-2`: click in the body editor `.markdown-body-part-wysiwyg-editor .CodeMirror`
  (`OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml:19`) **(probe)** and type `The light over the river turns
  gold for twenty minutes every evening.` The editor is EasyMDE: its text box is hidden, so type into the CodeMirror element,
  not the `textarea`. The editor is the Wysiwyg one (`blog.recipe.json:235`). **On screen when the narration ends**: the body
  text in the editor (with the toolbar of the editor above it, if the editor shows one). Do not name the label of the body field in any caption: its text is derived
  from the part name.
- `post-3`: scroll to the fields of the post. Type `Light worth waiting for` in **Subtitle** (`GetByLabel("Subtitle")`,
  `blog.recipe.json:619`). Under **Banner Image** (`:629`), click **Add media** (`OrchardCore.Media/Services/MediaJSLocalizer.cs:119`).
  The dialog **Select Media** opens (`:137`) with the files `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg`. Click `post-bg.jpg`,
  then **OK** (`:126`; it is disabled until a file is selected). **(probe)** the file tile in the dialog. **On screen when the
  narration ends**: the dialog closed, the field Banner Image showing the thumbnail of `post-bg.jpg`, and the Subtitle filled.
- `post-4`: scroll to the field **Tags** (`blog.recipe.json:648`), whose box says "Type to search"
  (`OrchardCore.Taxonomies/Services/TaxonomiesJSLocalizer.cs:16`). Click the box and type `Explo`. Select **Exploration** in the
  list that opens: click the option `.multiselect__option` with the text Exploration **(probe)**. **On screen when the
  narration ends**: Exploration shown as a selected tag in the field.
- `post-5`: click the box of **Tags** again and type `Lisbon`. The list offers "Press enter to create a tag"
  (`TaxonomiesJSLocalizer.cs:20`). Hold the frame on that text for about 2 s, then press Enter. Wait for the selected tag
  `.multiselect__tag` with the text Lisbon **(probe)**. The tag is created at once in the Tags taxonomy, which is published
  (`OrchardCore.Taxonomies/Controllers/TagController.cs:105`), because the user has the permission "Manage taxonomies" and the
  editor is open (`TaxonomyFieldTagsEditorSettings.cs:14`, `TaxonomyField-Tags.Edit.cshtml:24`). **On screen when the
  narration ends**: Exploration and Lisbon as selected tags.
- `post-6`: scroll to the field **Category**, a list of radio buttons (`blog.recipe.json:664`, `:669`, `:670`;
  `OrchardCore.Taxonomies/Views/TaxonomyField.Edit.cshtml:72`) with Travel and Photography. Click the radio button of
  **Photography** (`label.form-check-label` with the text Photography). **On screen when the narration ends**: the Tags and
  the Category fields together, with Photography selected.
- `post-7`: click **Publish** (the left half of the split button; `button[value='submit.Publish']`), with
  `ClickAndWaitForNavigationAsync`. The page returns to the admin page of the Blog. Wait for the message "Your Blog Post has been
  published." (`AdminController.cs:473`). **On screen when the narration ends**: the message, and the list of posts with
  "Chasing Golden Hour in Lisbon" above "Man must explore, and this is exploration at its greatest" (the list is ordered by
  creation date, most recent first: `OrchardCore.Lists/Services/ContainerService.cs:353`).
- `post-8`: `NavigateAsync("/")`. Wait for the first `.post-preview` to show "Chasing Golden Hour in Lisbon" with the badges
  Photography, Exploration and Lisbon. Click the title (`ClickAndWaitForNavigationAsync`). Scroll slowly down the post: banner
  `post-bg.jpg`, the subtitle "Light worth waiting for", the text. **On screen when the narration ends**: the header of the post
  (banner, title, subtitle, "Posted by admin on" and a date) or the first lines of the text.
  The address of the post is `/blog/chasing-golden-hour-in-lisbon` **(probe)**: it is not visible in the recording, and nothing
  in the narration says it.

**Changes the site**: restore snapshot **B** before a new take.

## Clip `05-tags`

**Starts on** the post page `/blog/chasing-golden-hour-in-lisbon` **(probe the address)**, scrolled before the capture until the
badges `.term-badge` are in view.

- `tags-1`: click the badge **Lisbon** (`.term-badge a` with the text Lisbon, `ClickAndWaitForNavigationAsync`). The page of the
  tag opens at `/tags/lisbon` **(probe)**, with the heading "Lisbon" (`Content.liquid`) and the post in the list below
  (`Tag-TermPart.liquid:1`). **On screen when the narration ends**: the heading Lisbon and the post preview.
- `tags-2`: `NavigateAsync("/Admin")`, click **Content**, then **Taxonomy**, then **Edit** in the row of **Tags** (same locators
  as `category-1`, with the text Tags). Scroll to the term list `#menu`. **On screen when the narration ends**: the terms Earth,
  Exploration, Space and Lisbon in the list (Lisbon last).
- `tags-3`: highlight the button **Add Tag** (`a:has-text("Add Tag")`, `TaxonomyPart.Edit.cshtml:48`), with no caption. Do not
  click it. **On screen when the narration ends**: the button Add Tag highlighted above the term list.

**Changes the site**: no.

## Clip `06-menu`

**Starts on** `/Admin`.

- `menu-1`: click the admin entry **Main Menu** (`blog.recipe.json:1217`, `ClickAndWaitForNavigationAsync`). The editor of the
  menu opens (`/Admin/Contents/ContentItems/<id>/Edit`, no `returnUrl`). Highlight the field **Alias**
  (`GetByLabel("Alias")`, `OrchardCore.Alias/Views/AliasPart.Edit.cshtml`), which holds `main-menu`. **On screen when the
  narration ends**: the Alias field with its value, highlighted. The Layout asks for the menu by this alias
  (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`).
- `menu-2`: scroll down to the card with the button **Add Menu Item** (`OrchardCore.Menu/Views/MenuPart.Edit.cshtml:31`) and the
  list `#menu` with Home and About, each with **Edit**, **Add** and **Delete** (`MenuItem.Admin.cshtml:37`, `:40`). At about 4 s,
  click **Add Menu Item**. **On screen when the narration ends**: the dialog **Available Menu Items** (`MenuPart.Edit.cshtml:60`)
  open.
- `menu-3`: move the cursor over the three cards of the dialog, one after the other (`MoveToAsync`): **Link Menu Item**
  (`LinkMenuItemPart.Thumbnail.cshtml:1`), **Content Menu Item** (`ContentMenuItemPart.Thumbnail.cshtml:1`) and **Html Menu Item**
  (`HtmlMenuItemPart.Thumbnail.cshtml:1`). **On screen when the narration ends**: the dialog with the three cards, the last one
  under the cursor.
- `menu-4`: click **Add** in the card **Link Menu Item** (`.card` with that text, `a.thumbnail-link-create`,
  `MenuItem.Thumbnail.cshtml:16`; `ClickAndWaitForNavigationAsync`). Type `Photography` in **Name**
  (`OrchardCore.Menu/Views/LinkMenuItemPart.Edit.cshtml:4`) and `~/categories/photography` in **Url** (`:11`). Leave **Target**
  and **Permissions** empty. **On screen when the narration ends**: Name and Url filled.
- `menu-5`: click **Publish** (single button, `ClickAndWaitForNavigationAsync`). The page returns to the editor of Main Menu
  (`OrchardCore.Menu/Controllers/AdminController.cs:139`), where the menu was saved as a draft (`:137`). Wait for the list `#menu` to
  hold three items. **On screen when the narration ends**: the list with Home, About and Photography, each marked **Link**
  (`LinkMenuItemPart.Admin.cshtml`).
- `menu-6`: click **Publish** at the bottom of the editor (`button[value='submit.Publish']`) and wait for the message "Your Menu has
  been published.". Publishing the menu clears the cache of the alias `main-menu`
  (`OrchardCore.Alias/Handlers/AliasPartHandler.cs:61`). Then highlight the button **Add** of the item Photography. Do not
  click it, and do not drag. **On screen when the narration ends**: the message and the three items, the button Add of
  Photography highlighted.
- `menu-7`: `NavigateAsync("/")`. The menu shows Home, About and Photography. Click **Photography**
  (`#mainNav a` with that text, `ClickAndWaitForNavigationAsync`). The page `/categories/photography` opens, with a heading made of
  the camera icon and "Photography" (`Content-Category.liquid:10`) and the post below (`Category-TermPart.liquid:1`). **On screen
  when the narration ends**: the category page with the heading and the post preview.

**Changes the site**: restore snapshot **C** before a new take.

## Checks for private data

Before the clips are assembled, and again on the video (OCR at one frame per second and at every scene change):

- The only user name on screen is `admin`; the only email is `admin@example.com` (the user menu of the admin).
- The password is never typed on camera (the sign-in is saved in advance), and no password is visible anywhere in the recording.
- No real name in the posts, the tags or the menu: the seeded post and the values listed above are invented. The footer of the
  theme reads "Your Website" (`Recipes/Snippets/footer.html:31`): that text is acceptable.
- The address bar is not part of the recording, and no clip shows a path that holds a machine name or a user folder.
