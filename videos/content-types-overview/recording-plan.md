# Recording plan: content-types-overview

How to record the clips of this video without deciding anything. A selector written here stands for
`scene.Page.Locator(...)`. Source references are paths in the Orchard Core source (the checkout the site is built from),
with the line of the label. The video is read-only: no clip saves anything on the site, only the seed below does.

## The site it needs

- **Build**: `src/OrchardCore.Cms.Web` of the Orchard Core checkout, on `http://localhost:5010`, with its own
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Block outbound calls with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address.
- **Auto Setup** (no setup or sign-in form is recorded), with the **Blog** recipe (`name` "Blog" in
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:2`; the types, parts, fields and the About article of
  the video come from that recipe, lines 128 to 800 and 1062):
  - site name `Orchard Core`, time zone `America/Los_Angeles`;
  - user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`);
  - the front end theme is TheBlogTheme and the admin theme is TheAdmin (`blog.recipe.json:83`).
- **Features to enable before recording**: none. The recipe enables Content Types, Content Fields, Media, Html,
  Markdown, Taxonomies, Title, Autoroute, Menu, Flows and Widgets (`blog.recipe.json:31` to `80`). Open
  Configuration > Features once and check that **Content Types** is enabled.
- **Settings**: leave every setting at its default. In particular leave Configuration > Settings > Admin as it is: the
  lists must use the List layout and the Buttons layout for the row actions, which are the defaults
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30` and `:36`). The step entries below describe
  buttons beside each row.
- **External services**: none for the site. The pages of the Blog theme ask `fonts.googleapis.com` for two fonts
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12` and `:13`). The recorder's browser makes those requests,
  not the site, so the proxy settings above do not block them: the fonts load when the recording machine is online, and
  the page uses its fallback fonts when it is not. Either way, wait for the page to be drawn before the narration of
  `editor-3` ends (see there).
- **Sign-in**: run `Recorder setup` once; `state.json` stays out of git.

### Seed data (create it outside the capture, once)

The Content Parts list shows one part of the viewer's own, **Contact Details**, with two fields. Create it by hand in the
admin (or in a `PrepareAsync`, with the same requests), signed in as `admin`:

1. Design > Content Definition > Content Parts, **Create new part** (`ListParts.cshtml:20`). In **Name**
   (`CreatePart.cshtml:13`) type `ContactDetails`, click **Create** (`CreatePart.cshtml:21`). The page "Edit Content Part -
   ContactDetails" opens.
2. In **Display Name** (`ContentPartSettings.Edit.cshtml:13`) type `Contact Details`. In **Description**
   (`ContentPartSettings.Edit.cshtml:21`) type `Phone number and website of a person or a company.`. Leave
   **Attachable** checked and **Reusable** unchecked (a part created in the admin is attachable:
   `Services/ContentDefinitionService.cs:224`). Click **Save** (`EditPart.cshtml`, the Save button of the action bar).
3. Click **Add Field** (`EditPart.cshtml:88`). **Display Name** `Phone`; the **Technical Name** box fills itself while you
   type: check that it reads `Phone`, and type it if it is empty. **Field Type**: choose **Text Field**. Click **Save**
   (`AddFieldTo.cshtml:38`, `:46`, `:54`, `:71`). The part page opens again.
4. Click **Add Field** again. **Display Name** `Website`, **Technical Name** `Website`, **Field Type** **Link Field**,
   **Save**.
5. Check that, under **Fields**, the part lists **Phone** with the hint "Text Field" and **Website** with the hint
   "Link Field" (`EditPart.cshtml:46`, `:75`).

Then stop the site and copy its `App_Data` folder and `state.json` aside: that is the snapshot.

## Order of the clips

All the clips start from the snapshot and none of them changes the site, so they can be recorded in any order and as
often as needed, without restoring the snapshot. The one dependency: `05-parts` needs the seed above. If a take ends with
something saved by mistake (a click on a Save button), stop the site, restore the snapshot and start it again.

| Order | Clip | Starts on | Changes the site |
|---|---|---|---|
| 1 | `02-types` | `/Admin` | no |
| 2 | `03-anatomy` | `/Admin/ContentTypes/List` | no |
| 3 | `04-editor` | `/Admin/ContentTypes/List` | no |
| 4 | `05-parts` | `/Admin` | no |

`01-model` is a slide clip: nothing to record.

**Timing.** Narrate first (`narrate.py`), so the recorder holds each step for its narration. A step lasts about
0.4 seconds per word of its text. Keep the actions of a step shorter than that: `types-1` (three menu clicks) and
`anatomy-8` (two page loads, then the field type list) are the tight ones; if they run long, drop the optional
second ring of the step, never a click.

## Chapter 01: The content model (slides)

Drawn by `slides.py`; four cards, the ones the narration talks about are bright and the others are dimmed.

- `idea-1`: the "Content item" card is bright.
- `idea-2`: the "Content type" card is bright.
- `idea-3`: the "Content part" card is bright.
- `idea-4`: the "Content field" card is bright.
- `idea-5`: the "Content part" and "Content field" cards are bright.

## Chapter 02: The Content Types page (`02-types`)

Open `/Admin` before the capture, with the admin menu collapsed. Start the capture when the dashboard is drawn.
In this clip the rows are `[data-type-name='<TechnicalName>']` (`Drivers/ContentTypeEntryDisplayDriver.cs:20`).

- `types-1`
  - **Actions**: click **Design** in the admin menu, then **Content Definition**, then **Content Types** (the last click
    with `ClickAndWaitForNavigationAsync`; page `/Admin/ContentTypes/List`). Then highlight the search box (placeholder
    "Search") and, 1.5 seconds later, the **Create new type** button (`a#btnCreate`). Do not click it.
  - **On screen when the narration ends**: the Content Types page, with the **Create new type** button ringed and the
    first rows of the list below the search box.
  - **Labels**: `AdminMenu.cs:23`, `:24`, `:25`; `Views/Admin/List.cshtml:15`, `:20`.
- `types-2`
  - **Actions**: clear the highlights. Scroll the page by 0 pixels or the least needed so the row `BlogPost` is on
    screen. Highlight the link "Blog Post" of that row, then the first badge of the row (it reads `BlogPost`), and hover
    the badge for one second so its tooltip "Technical Name" shows.
  - **On screen when the narration ends**: the Blog Post row, with the display name "Blog Post" and the badge `BlogPost`
    both ringed.
  - **Labels**: `Views/ContentTypeEntry.Fields.SummaryAdmin.cshtml` (the link); `Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:11`;
    the names come from `blog.recipe.json:188` and `:189`.
- `types-3`
  - **Actions**: clear the highlights. Move the cursor to the row `Container`; highlight its second badge (it reads
    `Widget`) and hover it for one second (tooltip "Stereotype"). Then do the same for the row `ContentMenuItem` (display
    name "Content Menu Item", second badge `MenuItem`). If a row is out of view, clear the highlights and scroll by
    the least needed first.
  - **On screen when the narration ends**: the "Content Menu Item" row with its `MenuItem` badge ringed, and the
    Container row with its `Widget` badge above it.
  - **Labels**: `Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:15`; `blog.recipe.json:320` to `327` (Container,
    Widget); `src/OrchardCore.Modules/OrchardCore.Menu/Migrations/menu.recipe.json:96` and `:99` (Content Menu Item,
    MenuItem).
- `types-4`
  - **Actions**: clear the highlights and scroll to the top. Highlight the buttons of the row `Article` in one ring:
    **Create New Article** (`a#btn-create-Article`), **List Items** (`a[role='btn-list-Article']`) and **Edit**
    (`a[role='btn-edit-Article']`). Hold two seconds. Then highlight the buttons of the row `BlogPost`: it has **List
    Items**, **Edit** and **Delete**, and no **Create New** button. Do not click any of them.
  - **On screen when the narration ends**: the Blog Post row with its buttons ringed and no "Create New Blog Post"
    button in it. The Article row is visible above it if the screen allows.
  - **Labels**: `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`, `:22`, `:24`, `:36`. Blog Post has no Creatable
    setting (`blog.recipe.json:193` to `196`); Article has it (`blog.recipe.json:134` to `139`).

## Chapter 03: Inside a content type (`03-anatomy`)

Open `/Admin/ContentTypes/List` before the capture. Start the capture when the list is drawn.

- `anatomy-1`
  - **Actions**: click **Edit** of the row `Article` (`a[role='btn-edit-Article']`, `ClickAndWaitForNavigationAsync`;
    page `/Admin/ContentTypes/Edit/Article`, titled "Edit Content Type - Article"). Highlight the **Technical Name** input
    (it is read only and holds `Article`), then the **Display Name** input (`Article`). Above the names the page draws an
    **Edit templates** button (the Templates feature, for an admin who may manage templates); it is not narrated: do not
    click it.
  - **On screen when the narration ends**: the top of the page "Edit Content Type - Article", with both name boxes
    ringed.
  - **Labels**: `Views/Admin/Edit.cshtml:63`; `Views/ContentType.Edit.cshtml:4`, `:11`; `OrchardCore.Templates/Views/TemplateSettings.cshtml:5`
    (the **Edit templates** button, placed above the names by `Views/Admin/Edit.cshtml:73` to `80`).
- `anatomy-2`
  - **Actions**: clear the highlights. Scroll down until the settings are on screen. Highlight the boxes
    **Creatable** and **Listable** in one ring, three seconds later **Draftable** alone (the page also holds
    **Versionable**, checked, and **Securable**, unchecked: ring neither), then, for the last sentence, the
    **Stereotype**, **Category** and **Thumbnail** boxes as one group. Do not change anything.
  - **On screen when the narration ends**: **Stereotype**, **Category** and **Thumbnail** visible and ringed, with the
    check boxes above them. (The Contents feature adds a **Full-text** section after these boxes,
    `OrchardCore.Contents/Views/FullTextAspectSettings.Edit.cshtml:15`; it is not narrated, so scroll past it.)
  - **Labels**: `Views/ContentTypeSettings.Edit.cshtml:17`, `:30`, `:43`, `:56`, `:69`, `:77`, `:91`, `:105`. Article has
    Creatable, Draftable, Versionable and Listable on and Securable off (`blog.recipe.json:134` to `139`).
- `anatomy-3`
  - **Actions**: clear the highlights. Scroll down until the **Fields** heading (`h3`) is in the upper third of the
    screen. Highlight the rows "Subtitle" (hint "Text Field") and "Banner Image" (hint "Media Field"), one ring each,
    one after the other.
  - **On screen when the narration ends**: the Fields list with both rows visible and ringed. The two fields have the
    same position in the recipe, so they can appear in either order: both orders are fine for the narration.
  - **Labels**: `Views/Admin/Edit.cshtml:88`, `:115`; the names come from `blog.recipe.json:677` to `700`.
- `anatomy-4`
  - **Actions**: clear the highlights. Scroll down until the **Parts** heading (`h3`) and its four rows are on
    screen. Move the cursor down the list, over the rows "Title", "Autoroute", "HtmlBody" and "Article", and ring the
    whole list (`ul#parts`) at the end.
  - **On screen when the narration ends**: the four rows in this order (Title, Autoroute, HtmlBody, Article) inside one
    ring. The HtmlBody row carries a badge "Wysiwyg" (tooltip "Editor").
  - **Labels**: `Views/Admin/Edit.cshtml:144`, `:189`; the order is the position of each part (`blog.recipe.json:145` to
    `183`: Title 0, Autoroute 1, HtmlBody 2, Article 3); the part names are the part's technical name without the "Part"
    suffix (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs`, `DisplayName`).
- `anatomy-5`
  - **Actions**: clear the highlights. Ring the row "Article" of the Parts list (grey background). Its text reads
    "Contains the fields for the current type" and it has no **Edit** and no **Remove** button on the right.
  - **On screen when the narration ends**: the Article row ringed, with the other three rows above it showing their
    **Edit** and **Remove** buttons.
  - **Labels**: `Views/Admin/Edit.cshtml:157` to `180` (the buttons are drawn only for the parts that are not the type's own
    part); `:180` (the hint).
- `anatomy-6`
  - **Actions**: clear the highlights. Ring the **Edit** and **Remove** buttons of the "Title" row, then of the
    "Autoroute" row, then of the "HtmlBody" row, one second each. Do not click them.
  - **On screen when the narration ends**: the buttons of the HtmlBody row ringed.
  - **Labels**: `Views/Admin/Edit.cshtml:163`, `:174`.
- `anatomy-7`
  - **Actions**: clear the highlights. Scroll so the buttons under the Parts list are on screen and click **Add Parts**
    (`ClickAndWaitForNavigationAsync`; page `/Admin/ContentTypes/AddPartsTo/Article`, titled "Add Parts", with the text
    "Choose the Parts to add to this Content Type."). Scroll the list slowly, one screen. Do not tick any box.
  - **On screen when the narration ends**: the Add Parts page with part names and their grey descriptions on screen,
    no box ticked, no **Add Selected Parts** button.
  - **Labels**: `Views/Admin/AddPartsTo.cshtml:10`, `:18`, `:34`. The list holds the attachable parts that the type does
    not have yet (`Controllers/AdminController.cs:270` to `277`).
- `anatomy-8`
  - **Actions**: click **Cancel** (`a.btn.cancel`, `ClickAndWaitForNavigationAsync`, back to "Edit Content Type -
    Article"). Scroll to the **Add Field** button under the Fields list (`Edit.cshtml:137`) and click it
    (`ClickAndWaitForNavigationAsync`; page `/Admin/ContentTypes/AddFieldsTo/Article`, titled `Add New Field To "Article"`).
    Highlight the **Display Name** and **Technical Name** boxes, then the **Field Type** heading. Scroll the field type
    list and ring, in turn, the radio buttons "Boolean Field", "Date Field", "Media Field", "Numeric Field" and
    "Text Field". Do not type and do not click **Save**.
  - **On screen when the narration ends**: the Field Type list, with "Text Field" ringed. The clip ends here.
  - **Labels**: `Views/Admin/AddFieldTo.cshtml:20`, `:38`, `:46`, `:54`, `:62`. The names come from the registered field types
    (`catalog/features.json`: BooleanField, DateField, MediaField, NumericField, TextField), shown with a space between
    the words.

## Chapter 04: From type to editor (`04-editor`)

Open `/Admin/ContentTypes/List` before the capture. Start the capture when the list is drawn.

- `editor-1`
  - **Actions**: click **Create New Article** (`a#btn-create-Article`, `ClickAndWaitForNavigationAsync`; page
    `/Admin/Contents/ContentTypes/Article/Create?returnUrl=...`, titled "New Article"). Wait until the body editor's
    toolbar is drawn and the **Title** box has the focus. Do not type.
  - **On screen when the narration ends**: the top of the form "New Article": **Title** and **Permalink**, empty.
  - **Labels**: `OrchardCore.Contents/Views/Admin/Create.cshtml:26`; `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`.
- `editor-2`
  - **Actions**: ring the **Title** label, then the **Permalink** label (`TitlePart.Edit.cshtml:12`,
    `AutoroutePart.Edit.cshtml:30`). Clear the highlights, scroll down, ring the label of the body editor (it reads
    "HtmlBody"), clear, scroll down, ring **Subtitle**, then **Banner Image**. Do not type or save.
  - **On screen when the narration ends**: **Subtitle** and **Banner Image** visible, **Banner Image** ringed.
  - **Labels**: `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22` (the label is the part's display name);
    `OrchardCore.ContentFields/Views/TextField.Edit.cshtml` and `OrchardCore.Media/Views/MediaField.Edit.cshtml:35` (the label is
    the field's display name from `blog.recipe.json:685` and `:695`).
- `editor-3`
  - **Actions**: clear the highlights. Click **Cancel** (`a.btn.cancel`, `ClickAndWaitForNavigationAsync`; it returns to
    the list), then `NavigateAsync("/about")`. Wait until the heading "About", the sub heading and the banner image
    behind them are drawn (the heading is the `h1` of the page header, the sub heading is `span.subheading`; the banner
    is the background of `header.masthead`). The page can take a few seconds to settle while the browser fetches the fonts; give it up to ten seconds.
  - **On screen when the narration ends**: the top of the About page: the heading "About", the line "This is what I do."
    below it, and the banner photo behind both.
  - **Labels**: `OrchardCore.Contents/Views/Content.Edit.cshtml:30` (Cancel); the page is `src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid:4`
    to `16` (heading, sub heading, banner); the data is `blog.recipe.json:1059` to `1087`.
- `editor-4`
  - **Actions**: scroll the About page down slowly until the first paragraph of the body is on screen, then hold.
  - **On screen when the narration ends**: the start of the body paragraphs (placeholder text), under the header.
  - **Labels**: `Content-Article.liquid:27` (the body is drawn after the header).

## Chapter 05: The Content Parts page (`05-parts`)

Needs the seed above. Open `/Admin` before the capture, with the admin menu collapsed. Start the capture when the
dashboard is drawn. In this clip the rows are `[data-type-name='<TechnicalName>']`
(`Drivers/ContentPartEntryDisplayDriver.cs:20`).

- `parts-1`
  - **Actions**: click **Design**, then **Content Definition**, then **Content Parts** (the last click with
    `ClickAndWaitForNavigationAsync`; page `/Admin/ContentTypes/ListParts`). Highlight the **Create new part**
    button (`a#btnCreate`). Do not click it.
  - **On screen when the narration ends**: the Content Parts page with the **Create new part** button ringed.
  - **Labels**: `AdminMenu.cs:23`, `:24`, `:30`; `Views/Admin/ListParts.cshtml:20`.
- `parts-2`
  - **Actions**: clear the highlights. Scroll the list slowly from the top to the bottom while the cursor follows the
    rows, then back up to the row `ContactDetails` and ring it (display name "Contact Details", and under it the
    description "Phone number and website of a person or a company."). The list also holds the parts that features
    add, such as "Autoroute" and "Title": let them pass on screen.
  - **On screen when the narration ends**: the "Contact Details" row ringed, with a few other parts above and below.
  - **Labels**: `Views/ContentPartEntry.Fields.SummaryAdmin.cshtml`, `Views/ContentPartEntry.Description.SummaryAdmin.cshtml`; the list
    holds the part definitions stored in the site, except the part of each type (`Controllers/AdminController.cs:508`,
    `:1162`); Title and Autoroute come from `OrchardCore.Title/Migrations.cs:31` and `OrchardCore.Autoroute/Migrations.cs:21`.
- `parts-3`
  - **Actions**: clear the highlights. Click **Edit** of the row `ContactDetails` (`#btn-edit-ContactDetails`,
    `ClickAndWaitForNavigationAsync`; page `/Admin/ContentParts/Edit/ContactDetails`, titled "Edit Content Part -
    Contact Details"). Ring the **Technical Name** (`ContactDetails`), **Display Name** (`Contact Details`) and
    **Description** boxes as one group, then the **Attachable** box (checked) alone, then the **Reusable** box
    (unchecked) alone, about two seconds each. Do not click **Save**. Above the names the page also draws the
    **Edit templates** button (as on the page of a type); it is not narrated: do not click it.
  - **On screen when the narration ends**: the settings of the part, with the **Reusable** box ringed and unchecked and
    the **Attachable** box checked above it.
  - **Labels**: `Views/Admin/EditPart.cshtml:23`; `Views/ContentPartSettings.Edit.cshtml:5`, `:13`, `:21`, `:33`, `:43`. The
    hints "Check to allow this part to be attached to other content types." (`:34`) and "Check to allow this part to be
    attached multiple times." (`:44`) are the source of the narration of the two boxes;
    `Views/ContentPartEntry.Buttons.SummaryAdmin.cshtml:6`.
- `parts-4`
  - **Actions**: clear the highlights. Scroll to the **Fields** heading (`EditPart.cshtml:46`) so that it is in the
    upper third of the screen, and ring the rows "Phone" (hint "Text Field") and "Website" (hint "Link Field"), one
    ring each, one after the other. Do not click **Save**.
  - **On screen when the narration ends**: the Fields list with Phone and Website ringed.
  - **Labels**: `Views/Admin/EditPart.cshtml:46`, `:75`.

## Checks for private data

- Every name on screen is fake: the user `admin` with `admin@example.com`, the recipe's About article (placeholder
  text), the Blog and its sample post, and the Contact Details part. No real person, company, address or token exists
  on this site.
- The admin bar and the user menu show only `admin`.
- Run `review.py` for each clip and look at every contact sheet: no error banner, no empty Fields or Parts list, no
  sign-in form, no browser dialog.
- OCR the master at one frame per second and at every scene change, then at five frames per second, and search for real
  names, email addresses other than `@example.com`, phone numbers and tokens. The password never appears on screen.
