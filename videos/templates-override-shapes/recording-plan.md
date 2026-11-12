# Recording plan: Templates: Override Shapes From the Admin With Liquid

How to record the four recordings of this video (`02-create`, `04-debug`, `05-shortcuts`, `06-manage`) without
deciding anything. The other chapters are slides (see "Slides" at the end). Narrate first (`narrate.py`), then record.

Conventions of this file:

- A selector such as `a.create` stands for `scene.Page.Locator("a.create")`. Scene methods are those of
  `references/recording.md`. The recorder (`Scenes.cs`) is written from this plan.
- "Source" gives `file:line` in the Orchard Core source (the commit the video is recorded on), where the label or the
  element of the step is defined. A label is typed or clicked exactly as written there.
- A step id in backticks is the id of the step in `storyboard.json`.
- Every step's actions are shorter than its narration (about 0.4 seconds per word).

## The site it needs

- **Site**: `src/OrchardCore.Cms.Web` of an Orchard Core checkout, on `http://localhost:5010`, with its own
  `ORCHARD_APP_DATA` folder outside both repositories. Outbound calls of the site are blocked with
  `HTTP_PROXY`/`HTTPS_PROXY` set to a dead address, as `references/recording.md` describes.
- **Auto Setup** (environment variables `OrchardCore__AutoSetup__Tenants__0__...`): `ShellName` Default, `SiteName`
  Orchard Core, `SiteTimeZone` America/Los_Angeles, `AdminUsername` admin, `AdminEmail` admin@example.com,
  `AdminPassword` `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`), `DatabaseProvider` Sqlite,
  `RecipeName` **Blog**. The Blog recipe sets TheBlogTheme as the site theme and TheAdmin as the admin theme.
- **Features to enable before recording**: none. The Blog recipe enables Templates, Content Types, Placements, Lists
  and Contents (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, step `feature`). Admin Templates stays
  disabled: the video is about Templates only, so the Design menu shows Templates and no Admin Templates.
- **Settings**: none to save beforehand. `Write shape debug information` must be **unchecked** at the start of
  `04-debug` (it is by default) and is checked by that clip.
- **External services**: none. The theme asks Google Fonts for its fonts from the browser. Record every clip with the
  same network state (online or offline for all of them), so the fonts do not change from one clip to the next.
- **Templates**: none at the start. The Templates list is empty before `02-create`.
- **Browser**: one new browser context for each take (the Save button remembers the last choice of its menu in the
  browser's local storage). Chromium settings are those of the specification. `state.json` is the sign-in of `admin`
  (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`).

### Seed data (create it outside the capture, once)

The Blog recipe creates one blog post, "Man must explore, and this is exploration at its greatest" (subtitle "Problems
look mighty small from 150 miles up"; tags Earth, Exploration and Space; category Travel), in the blog "Blog". Add two
blog posts, so that the blog page lists three:

1. In the admin, open **Content**, **Content Items** (`/Admin/Contents/ContentItems`). On the row of **Blog**, click
   **List Blog Post Items**, then **Create Blog Post**.
2. Fill **Title** with `Planning a market garden` and **Subtitle** with `What the first season taught us`. Leave every
   other field empty. Click **Publish**.
3. Do it again with **Title** `Composting in a small kitchen` and **Subtitle** `A year of scraps in one bin`.
4. Open `/blog`. The three posts are listed, the two new ones above the recipe's post, each as the theme's summary view
   shows it: the title, the subtitle, "Posted by admin on" and a date.

Source: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68` (Content, Content Items),
`OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10` (List Blog Post Items),
`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52` (Create Blog Post),
`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` (Title), `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`
(Publish). The label **Subtitle** is the display name of the Subtitle field in `blog.recipe.json`.

Then run `Recorder setup`, stop the site, and copy `App_Data` and `state.json` aside as snapshot **S0**.

## Order of the clips, and the state each one changes

Record in this order. Slides need no site.

| Order | Clip | Starts from | Changes the site | Leaves |
|---|---|---|---|---|
| 1 | `02-create` | S0 (no template) | Creates the template `Content_Summary__BlogPost` | Take snapshot **S1** |
| 2 | `05-shortcuts` | S1 (the template exists) | Nothing (the form is cancelled) | S1 |
| 3 | `06-manage` | S1 (the template exists) | Deletes the template | The Templates list is empty |
| 4 | `04-debug` | S1 (the template exists, debug information off) | Checks **Write shape debug information** | Nothing more is recorded |

To record a clip again, restore the snapshot in the "Starts from" column (stop the site, restore `App_Data` and
`state.json`, start the site). `05-shortcuts` and `06-manage` depend on `02-create`: the template must exist.
`04-debug` is recorded last on purpose: once the setting is on, every shape of every page is wrapped in HTML comments,
which would change the page source of the clips recorded after it. It must start with the setting unchecked, so a
second take of `04-debug` restores S1 first, since the click would otherwise uncheck the box.

## Clip `02-create`: create a template

Starts on `/blog` (`scene.OpenAsync("/blog")`, before the capture). Chapter 02.

The template text, used by `create-5`, is exactly (two spaces per level, line feeds, no line after the last one):

```
<div class="post-preview">
  <a href="{{ Model.ContentItem | display_url }}">
    <h2 class="post-title">{{ Model.ContentItem.DisplayText }}</h2>
    <h3 class="post-subtitle">
      {{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}
    </h3>
  </a>
  <p class="post-meta">
    Written by {{ Model.ContentItem.Author }}
  </p>
</div>
<hr class="my-4" />
```

- `create-1`
  - Actions: `HighlightAsync(.post-preview >> nth=0)` (a ring around the first post, no caption), hold 2 seconds,
    `ClearHighlightsAsync()`.
  - On screen at the end: the blog page, the header "Blog", three posts in the theme's format ("Posted by admin on" and
    a date under each title, badges under the post of the recipe).
  - Source: `src/OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.Summary.liquid:1` and `:13`.
- `create-2`
  - Actions: `NavigateAsync("/Admin/Templates")`. Wait until the text `There are no templates for the moment` is visible.
    `HighlightAsync(a.create)`, hold 1 second, `ClearHighlightsAsync()`, `ClickAndWaitForNavigationAsync(a.create)`
    (the button labeled **Add Template**).
  - On screen at the end: the **Create Template** page: the fields **Name**, **Description** and **Content**, all
    empty (the editor shows no text), the Save button at the bottom.
  - Source: `OrchardCore.Templates/Views/Template/Index.cshtml:20` (the empty message),
    `OrchardCore.Templates/Views/TemplateCreateButton.cshtml:6` (Add Template),
    `OrchardCore.Templates/Views/Template/Create.cshtml:8` (Create Template), `:18`, `:27`, `:35` (Name,
    Description, Content). The menu path **Design**, **Templates** is `OrchardCore.Templates/AdminMenu.cs:19-20`.
- `create-3`
  - Actions: `TypeAsync(#Name, "Content_Summary__BlogPost")`, then
    `TypeAsync(#Description, "Summary of a blog post in a list")`.
  - On screen at the end: both fields filled; the hints "The name of the template." and "The description of the
    template." under them; the editor still empty.
  - Source: `Create.cshtml:22` and `:30` (the hints).
- `create-4`
  - Actions: `ClickAndWaitForNavigationAsync(button.save)` without writing any content. The form is shown again.
  - On screen at the end: the **Create Template** page again; **Name** and **Description** keep their text; under
    the editor the error "The content is mandatory."; at the top a warning that starts with "If you left the content
    empty because you want to hide the shape, use" and links **Placements**. Wait for the text "The content is
    mandatory." to be visible before the step ends.
  - Source: `OrchardCore.Templates/Controllers/TemplateController.cs:418` (the warning) and `:420` (the error).
- `create-5`
  - Actions, in this order, none of them typed on the keyboard (the editor closes brackets and quotes by itself, and
    its suggestion list takes the Enter key, so typed Liquid comes out wrong):
    1. `scene.Page.WaitForSelectorAsync("#Content_editor .monaco-editor")`.
    2. `scene.Page.EvaluateAsync` with the script
       `async (text) => { const monaco = await window.__orchardCoreMonacoReady; monaco.editor.getModels()[0].setValue(text); }`
       and the template text above as its argument.
    3. Check with `scene.Page.EvaluateAsync` that
       `(await window.__orchardCoreMonacoReady).editor.getModels()[0].getValue()` equals the template text, and that
       `document.querySelector("#Content").value` equals it too (the page copies the editor into the hidden field).
       If either differs, stop: the take is not valid.
  - On screen at the end: the editor shows the twelve lines of the template, from `<div class="post-preview">`.
    The first lines are in view without scrolling (the editor starts about a third of the way down the page).
  - Source: `OrchardCore.Templates/Assets/ts/template-create.ts` (editor and language "liquid", the copy to the
    hidden field), `Create.cshtml:40` (the hint "The Liquid template.").
- `create-6`
  - Actions: `HighlightAsync(button.save)`, `HighlightAsync(a.action-bar-secondary)` (the link labeled **Preview**,
    which opens the preview page in a new tab: do not click it), hold 2 seconds, `ClearHighlightsAsync()`.
  - On screen at the end: the action bar at the bottom of the page with **Save** and **Preview** (rings cleared or
    visible), the editor with the template above.
  - Source: `Create.cshtml:53` (Save) and `:69` (Preview, a link to the `Index` action of the `Preview` controller).
- `create-7`
  - Actions: `ClearHighlightsAsync()`, `ClickAndWaitForNavigationAsync(button.save)`. Wait for the text
    `Content_Summary__BlogPost` to be visible in the list.
  - On screen at the end: the **Templates** list with one row: the link `Content_Summary__BlogPost`, and under it
    `Summary of a blog post in a list`, with **Edit** and **Delete** buttons; the message
    `The "Content_Summary__BlogPost" template has been created.` if it has not faded.
  - Source: `TemplateController.cs:204` (the message), `Views/TemplateEntry.Fields.SummaryAdmin.cshtml` (the name),
    `Views/TemplateEntry.Description.SummaryAdmin.cshtml:8` (the description),
    `Views/TemplateEntry.Buttons.SummaryAdmin.cshtml:11` and `:19` (Edit, Delete).
- `create-8`
  - Actions: `NavigateAsync("/blog")`. Wait for the text `Written by admin` to be visible.
  - On screen at the end: the blog page, the three posts in the format of the template: the title as a link, the
    subtitle under it, "Written by admin", a line between the posts; no date and no badges.
  - Source: the template text above (`Content_Summary__BlogPost` is the alternate of a blog post in a summary view:
    `OrchardCore/OrchardCore.ContentManagement.Display/ContentItemAlternatesFactory.cs:77`).

## Clip `04-debug`: shape debug information

Starts on `/Admin/Templates` (`scene.OpenAsync`, before the capture): the list holds the template. Chapter 04.
State from S1, recorded last.

- `debug-1`
  - Actions: `NavigateAsync("/Admin/Settings/debugging")`. Wait for the label `Write shape debug information` to be
    visible. `ClickAsync(scene.Page.GetByLabel("Write shape debug information"))`. `HighlightAsync` on that label,
    hold 1 second, `ClearHighlightsAsync()`. `ClickAndWaitForNavigationAsync(button.primaryAction.save)` (the button
    **Save**).
  - On screen at the end: the **Debugging** settings page, the box **Write shape debug information** checked with its
    hint "Wrap rendered shapes in HTML comments to help identify the template or binding that produced each
    fragment.", and the message "Site settings updated successfully.".
  - Source: `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` and `:46` (Settings, Debugging),
    `OrchardCore.Settings/Views/DebugSettings.Edit.cshtml:7` and `:9`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`
    (Save), `OrchardCore.Settings/Controllers/AdminController.cs:119` (the message).

## Clip `05-shortcuts`: the shortcuts of a content type

Starts on `/Admin/ContentTypes/List` (`scene.OpenAsync`, before the capture): the list of content types. Chapter 05.
State from S1.

- `shortcuts-1`
  - Actions: `ClickAndWaitForNavigationAsync(a[role='btn-edit-BlogPost'])` (the button **Edit** on the row of Blog
    Post). Wait for the heading `Edit Content Type - Blog Post`.
    `ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Edit templates" }))`.
  - On screen at the end: the **Edit Content Type - Blog Post** page with the **Edit templates** menu open, listing
    "Template for a Blog Post content item in detail views" and "Template for a Blog Post content item in summary
    views".
  - Source: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63` (the title),
    `OrchardCore.ContentTypes/AdminMenu.cs:23-25` (Design, Content Definition, Content Types),
    `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` (the Edit button of a row),
    `OrchardCore.Templates/Views/TemplateSettings.cshtml:5` (Edit templates),
    `OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:50` and `:57` (the two entries).
- `shortcuts-2`
  - Actions: `ClickAndWaitForNavigationAsync` on the link whose text is
    `Template for a Blog Post content item in detail views` (`a.dropdown-item`). The Edit action finds no template
    named `Content__BlogPost` and redirects to the Create form.
  - On screen at the end: the **Create Template** page with **Name** holding `Content__BlogPost`, **Description** and
    the editor empty.
  - Source: `TemplateController.cs:234` (the redirect to Create), `TemplateContentTypeDefinitionDriver.cs:49`
    (the key `Content__BlogPost`: the stereotype is "Content" for a type without one).
- `shortcuts-3`
  - Actions: `ClickAndWaitForNavigationAsync(a.cancel)` (the link **Cancel**, back on the content type).
    `ClickAsync` on the **Edit templates** button again. `ClickAndWaitForNavigationAsync` on the link whose text is
    `Template for a Blog Post content item in summary views`. Wait for `#Content_editor .monaco-editor` and for the
    text `post-preview` to be visible in the editor.
  - On screen at the end: the **Edit Template** page: **Name** `Content_Summary__BlogPost`, **Description**
    `Summary of a blog post in a list`, the template in the editor.
  - Source: `Create.cshtml:64` (Cancel), `OrchardCore.Templates/Views/Template/Edit.cshtml:8` (Edit Template),
    `TemplateController.cs:232-243` (the Edit action loads the existing template).
  - Leave the page without saving (the next take restores a snapshot anyway).

## Clip `06-manage`: search, rename, delete

Starts on `/Admin/Templates` (`scene.OpenAsync`, before the capture): the list holds the template
`Content_Summary__BlogPost`. Chapter 06. State from S1.

- `manage-1`
  - Actions: `TypeAsync(#search-box, "Summary")`, then
    `WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`. Then
    `ClickAndWaitForNavigationAsync(a.btn-primary.btn-sm)` (the button **Edit** of the row).
  - On screen at the end: the **Edit Template** page: **Name**, **Description** and **Content** of the template, the
    template in the editor. (Wait for the editor to show the text.)
  - Source: `OrchardCore.Admin/Views/AdminListSearch.cshtml:21` (the search box, placeholder "Search"),
    `TemplateController.cs:85` (`Index` filters the names that contain the text), `TemplateEntry.Buttons.SummaryAdmin.cshtml:11`.
- `manage-2`
  - Actions: `HighlightAsync(#Name)`, hold 3 seconds, `ClearHighlightsAsync()`. Do not change the text.
  - On screen at the end: the **Edit Template** page, with a ring around the **Name** field (cleared before the next
    step starts).
  - Source: `OrchardCore.Templates/Views/Template/Edit.cshtml:19` (Name). The behavior (the old name is removed, the
    template is stored under the new name) is `TemplateController.cs:277-283`.
- `manage-3`
  - Actions: `ClickAndWaitForNavigationAsync(a.cancel)` (back on the list, filtered by "Summary").
    `ClickAsync(a.btn-danger)` (the button **Delete** of the row). Wait for `#modalOkButton`; hold 1 second so the
    dialog can be read. `WaitForNavigationAsync(() => ClickAsync(#modalOkButton))` (the button **Ok**).
  - On screen at the end: the list with the message "Nothing here! There are no templates for the moment." and, if it
    has not faded, "Template deleted successfully.".
  - Source: `TemplateEntry.Buttons.SummaryAdmin.cshtml:19` (Delete), `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`
    (the dialog: title "Delete", message "Are you sure you want to remove this element?", buttons "Ok" and "Cancel"),
    `TemplateController.cs:322` (the message).
- `manage-4`
  - Actions: `NavigateAsync("/blog")`. Wait for the text `Posted by admin on` to be visible.
  - On screen at the end: the blog page in the theme's format again: "Posted by admin on" and a date under each title.
  - Source: `Content-BlogPost.Summary.liquid:13`.

## Slides

These steps are drawn by `slides.py`; nothing is recorded for them.

- `idea-1`: cards Shape, Alternates, View file, Template; the first two are bright.
- `idea-2`: the same cards; Alternates, View file and Template are bright.
- `idea-3`: four cards, how a shape is resolved: the most specific alternate first, a template, a view file, the next alternate.
- `idea-4`: three cards, where templates apply: site pages, not the admin screens, the site's database.
- `idea-5`: three cards, the permissions Manage templates and Manage Liquid templates, both required.
- `names-1`: the list of name patterns (code panel) and three cards.
- `inside-1`: the template of `02-create` in a code panel, and cards for Model, Model.ContentItem and the content.
- `inside-2`: three lines of Liquid (shape_remove_item twice, shape_render), and three cards.
- `inside-3`: three cards, the properties of a content item, a widget and a zone.
- `debug-2`: the three comment lines of the shape debug information in a code panel, and three cards.
- `debug-3`: three cards, the names Content_Summary__BlogPost, Content_Summary and Content__BlogPost and what each one covers.
- `move-1`: the Templates recipe step (code panel) and three cards.
- `move-2`: three cards, the All Templates step, Export templates as files, the Templates recipe step.

## Checks for private data

Before the clips go to assembly, look at every contact sheet and OCR the video (one frame per second and at every
scene change). The only names on screen may be: the user `admin`, the site name "Orchard Core", and the fake titles
and subtitles of the seed data above. The only address that may appear is `admin@example.com` (it is not shown by
these clips). No password may be typed or shown: the sign-in comes from `state.json`. Check that no real date of
birth, phone number, token or machine path appears in the page, in the address bar or in a notification.
