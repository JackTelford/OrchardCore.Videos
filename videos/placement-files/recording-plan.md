# Recording plan: Placement: Decide Where Parts and Fields Appear

Audience: developer. Six chapters of slides and two browser recordings (`05-before` and `05-after`). A selector written
in this plan stands for `scene.Page.Locator(...)`. File references (`path:line`) point to the Orchard Core source this
content was written from (commit `92db4a83`), in the `OrchardCMS/OrchardCore` checkout the site runs from.

## What is recorded and what is a slide

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-places` | slide | `places-1`, `places-2`, `places-3` |
| 02 | `02-file` | slide | `file-1` to `file-5` |
| 03 | `03-target` | slide | `target-1` to `target-5` |
| 04 | `04-place` | slide | `place-1` to `place-5` |
| 05 | `05-before` | **recording** | `before-1`, `before-2` |
| 05 | `05-rules` | slide | `rules-1` |
| 05 | `05-after` | **recording** | `after-1`, `after-2` |
| 06 | `06-wins` | slide | `wins-1`, `wins-2` |
| 99 | `99-outro` | card | `outro` |

Only `05-before` and `05-after` need a site. Everything else is drawn by `slides.py`.

## The site it needs

A throwaway local site made from a checkout of `OrchardCMS/OrchardCore` (the `main` branch), never a production site.
Do not record the Setup screen: the site is provisioned by Auto Setup, so no setup or sign-in form is recorded.

- **Run**: `src/OrchardCore.Cms.Web` on `http://localhost:5010`, started from the project folder
  (`cd src/OrchardCore.Cms.Web`, then `dotnet run --urls http://localhost:5010`), so that its content root is the
  project folder: the startup project's `placement.json` is read from the content root
  (`ModuleEmbeddedFileProvider.cs:91-94`). Check that the file is found (the second take shows it), and if it is
  not, see the first question of `OPEN-QUESTIONS.md`.
- **App_Data**: an empty folder outside both repositories, set with the `ORCHARD_APP_DATA` environment variable.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`), so the
  site never calls an external service. The video needs no external service and no local stand-in.
- **Auto Setup** (the feature is enabled in `src/OrchardCore.Cms.Web/Program.cs:9`), through environment variables:

  | Variable (prefix `OrchardCore__AutoSetup__Tenants__0__`) | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git; never write it in the repository) |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

  The Blog recipe is `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json` (`"name": "Blog"`, line 2).
- **Tenant**: the `Default` tenant only.
- **Time zone**: `America/Los_Angeles` (above).
- **Features to enable before recording**: none. The Blog recipe enables Contents, Content Types, Title, Autoroute,
  Html, Content Fields, Media and Placements (`blog.recipe.json`, the `feature` step), and sets `TheAdmin` as the admin
  theme and `TheBlogTheme` as the site theme (the `themes` step, line 83).
- **Settings**: none to change.
- **Seed data**: none. Neither clip saves anything: the Article editor is opened on its Create page and left without
  saving, so no content item is created and no personal data exists. Never click Save Draft or Publish.
- **Check before recording** (outside the capture): open `/Admin/Contents/ContentTypes/Article/Create`. The Article
  type of the Blog recipe (`blog.recipe.json:131`) has the parts `TitlePart`, `AutoroutePart`, `HtmlBodyPart` and
  `Article`. The `Article` part has a text field named `Subtitle` (display name "Subtitle", lines 682-685) and a media
  field named `Image` (display name "Banner Image", lines 692-695). The editor must show the labels Title, Permalink, a
  body editor, Subtitle and Banner Image. If one is missing, the site was not made with the Blog recipe: start again
  from an empty App_Data folder.
- **No other placement source**: the Placements module (Design, Placements) must hold no rule, and there must be no
  `placement.json` in the active themes (`TheBlogTheme`, `TheAdmin`). The Blog recipe adds none. The modules carry
  `placement.json` files of their own (Title, Html, Contents, Markdown, Liquid, Menu, Admin Dashboard); none has a rule
  for `TextField_Edit`, `MediaField_Edit` or `ContentPart_Edit`, and the Seo module, whose file has `MediaField_Edit`
  rules for its own fields, is not enabled by the recipe.

### The placement file of the second take

Only the `05-after` take needs it. Create `src/OrchardCore.Cms.Web/placement.json` in the Orchard Core checkout (the
root of the startup project; it is read from the physical content root,
`src/OrchardCore/OrchardCore.Abstractions/Modules/ModuleEmbeddedFileProvider.cs:91-94`), with exactly this content:

```json
{
  "TextField_Edit": [
    {
      "contentType": "Article",
      "differentiator": "Article-Subtitle",
      "place": "Parts:0.5"
    }
  ],
  "MediaField_Edit": [
    {
      "contentType": "Article",
      "differentiator": "Article-Image",
      "place": "Parts:4#Media;1"
    }
  ]
}
```

It is the file the slide `rules-1` shows. The file lives in the Orchard Core checkout and is never committed to this
repository. The site reads the file when it starts and caches the result (`ShapePlacementParsingStrategy.cs:41-65`,
`DefaultShapeTableManager.cs`), so stop the site and start it again after adding or removing the file (the
assumption is listed in `OPEN-QUESTIONS.md`).

## The order of the recordings

The clips play in the order `05-before`, `05-rules`, `05-after`, and are recorded in that order too, with a restart
between the two recordings:

1. Start the site **without** `placement.json` (the file does not exist). Sign the recorder in
   (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), then record `05-before`.
2. Stop the site (by its process ID, never by image name). Create `placement.json` as above. Start the site again
   with the same `ORCHARD_APP_DATA` and the same environment variables. If the recorder's session is rejected
   after the restart, run `Recorder setup` again. Record `05-after`.

`05-after` depends on step 2 and `05-before` on the absence of the file. Neither clip depends on data created by the
other. No recording changes the site's data. The only difference between the two takes is the file, so there is no
snapshot to restore: to record `05-before` again, delete `placement.json` and restart the site; to record
`05-after` again, make sure the file exists and the site was restarted after it was created.

Run `narrate.py` before recording, since each step is held for the length of its narration (about 13 seconds for
`before-1` and `before-2`, 14 for `after-1`, 7 for `after-2`).

## Recorded clips

Both clips start on the same page, opened with `scene.OpenAsync` before `StartCaptureAsync`:
`/Admin/Contents/ContentTypes/Article/Create` (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:407`).
Use `MSYS_NO_PATHCONV=1` in Git Bash so the path is not rewritten.

Selectors used below (a label is matched exactly, with `:text-is`):

| Name | Selector |
|---|---|
| Title row | `.ocat-wrapper:has(> label.ocat-label:text-is("Title"))` |
| Permalink row | `.ocat-wrapper:has(> label.ocat-label:text-is("Permalink"))` |
| Body row | `.html-body-part-wysiwyg-editor` |
| Subtitle row | `.field-wrapper-article-subtitle` |
| Banner Image row | `.field-wrapper-article-image` |
| Tab bar | `.edit-item-parts > ul.nav-tabs` |
| Media tab | `.edit-item-parts > ul.nav-tabs a.nav-link:text-is("Media")` |

Where the labels come from: Title, `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`;
Permalink, `src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`; Subtitle and Banner
Image are the display names of the fields, shown by `OrchardCore.ContentFields/Views/TextField.Edit.cshtml:20` and
`OrchardCore.Media/Views/MediaField.Edit.cshtml:35` (display names in `blog.recipe.json:685` and `:695`); the two field
rows are selected by the class `field-wrapper-<part type>-<field name>` that the same views write
(`TextField.Edit.cshtml:19`, `MediaField.Edit.cshtml:34`), and the tab bar by its place in the edit page
(`OrchardCore.Contents/Views/Content.Edit.cshtml`, the `edit-item-parts` container, which holds the `Parts` zone); the body
editor is `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml` (its label is the display name of the part, not
named in the narration); the tab names Content and Media come from `src/OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:14` and from the `place` value, and
the tab bar is drawn by `src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:14-66`.

### `05-before`: the editor before the site has a placement file

Start page: the Create page of an Article (above), site started without `placement.json`.

Before `StartCaptureAsync` (not recorded): wait until the Banner Image row and `.trumbowyg-box` (the rich text
editor of the body) are both visible, then scroll to the top of the page (`window.scrollTo(0, 0)`) and wait 500 ms.
If `.trumbowyg-box` never appears, wait for the Banner Image row only.

- `before-1` (narration of about 13 seconds):
  1. `HighlightAsync` on the Title row, `PauseAsync(1500)`, `ClearHighlightsAsync()`.
  2. `HighlightAsync` on the Permalink row, `PauseAsync(1500)`, `ClearHighlightsAsync()`.
  3. `HighlightAsync` on the Body row, `PauseAsync(1500)`, `ClearHighlightsAsync()`.
  4. `ScrollAsync(0, 700)`, `PauseAsync(800)`, then `HighlightAsync` on the Subtitle row and on the Banner Image row.
     The distance 700 is a starting value: adjust it after a trial run until both rows are fully in the frame.

  Visible when the narration ends: the lower part of the editor with the Subtitle row first and the Banner Image
  row below it, both ringed in green. No tab bar anywhere on the page. No error message. Label source: above.
- `before-2` (narration of about 13 seconds):
  1. `ClearHighlightsAsync()`.
  2. `HighlightAsync(Subtitle row, "Article-Subtitle")`, `PauseAsync(3500)`, `ClearHighlightsAsync()`.
  3. `HighlightAsync(Banner Image row, "Article-Image")` and hold until the narration ends.

  Visible when the narration ends: the Banner Image row ringed with the caption "Article-Image", the Subtitle row
  above it without a ring. The label reads "Banner Image" while the caption reads "Article-Image": that contrast is
  what the narration points out. Label source: `blog.recipe.json:695` (display name) and `:692` (field name `Image`).

### `05-rules`: slide

- `rules-1`: the slide shows the `placement.json` of the startup project with its two rules, and two cards: "Subtitle:
  Parts zone, position 0.5" and "Banner Image: A tab named Media".

### `05-after`: the same editor with the placement file

Start page: the Create page of an Article (above), site restarted with `placement.json` in place.

Before `StartCaptureAsync` (not recorded): wait until the tab bar and the Subtitle row are visible, then scroll to
the top and wait 500 ms. The tab bar shows two tabs, Content and Media, Content selected.

- `after-1` (narration of about 14 seconds):
  1. `HighlightAsync(Tab bar)`, `PauseAsync(4000)`, `ClearHighlightsAsync()`.
  2. `HighlightAsync(Subtitle row)` and hold until the narration ends.

  Visible when the narration ends: the Content tab selected, the Title row and, directly below it, the Subtitle row
  ringed in green, with the Permalink row after it. The Banner Image row is not on this tab. Label source: above.
- `after-2` (narration of about 7 seconds):
  1. `ClearHighlightsAsync()`.
  2. `ClickAsync(Media tab, pauseAfter: 900)`.
  3. `HighlightAsync(Banner Image row)` and hold until the narration ends.

  Visible when the narration ends: the Media tab selected and the Banner Image row alone in the tab pane, ringed in
  green. Label source: `ZoneShapes.cs` and `GroupShapes.cs` (above).

Do not click Save Draft, Publish or Cancel, and leave the page without saving.

## Slides

Each slide is drawn by `slides.py` from the helpers of `brand.py`. One line each:

- `places-1`: code panel of `TextFieldDisplayDriver.cs` with the two `Location` calls bright and the rest dimmed.
- `places-2`: three cards: Move, Hide, Group.
- `places-3`: four cards: Shape type, Differentiator, Display type, Content item and request.
- `file-1`: the `placement.json` of the Title module, and three cards: Shape types, Rules, Location.
- `file-2`: a rule that uses every property, and two cards: Filters, Placement information.
- `file-3`: the four filter cards, the first two bright (displayType and differentiator, contentType).
- `file-4`: the same four cards, the last two bright (contentPart, path).
- `file-5`: four cards: place, alternates, wrappers, shape.
- `target-1`: rules for `TitlePart` and for the `Services` bag part, and two cards: TitlePart, Services.
- `target-2`: rules for `ContentPart` and `ContentPart_Summary` with the differentiator `GalleryPart`, under the
  heading "A dynamic part".
- `target-3`: a rule for `TextField` with `Article-Subtitle` and `Address-City`, and two cards.
- `target-4`: a rule for `ContentPart_Edit` with `LandingPage-Services`, and two cards: TextField_Edit, ContentPart_Edit.
- `target-5`: a rule for `TextField_Display` with the full differentiator, and two cards.
- `place-1`: four cards: a colon, a dot, a slash and a dash, each with an example place (`Parts.Article:1` for the dot).
- `place-2`: a row of five chips (start, before, the numbers, after, end) separated by "<", and three notes (the third
  says that no position counts as 0, or as the part's position in an editor).
- `place-3`: four cards: tab, card, column and group position, each with an example place.
- `place-4`: a rule file that puts a media field in a Media tab and an HTML field in a Content tab, and three cards.
- `place-5`: C# code panel: a part display driver whose `Edit` method places its shape with the fluent builder
  (zone, tab, card and column), and the equivalent string in a comment.
- `rules-1`: see `05-rules` above.
- `wins-1`: three cards: the startup project, the active theme, the modules (a module that depends on another wins
  over it).
- `wins-2`: three cards: the last matching rule wins, unknown properties are ignored, check the names first.

## Checks for private data

- The only people and addresses in the capture are the fake `admin` user and `admin@example.com` (if the admin bar
  shows them). No content item exists, so no title, author or email other than those appears.
- The media library behind the Banner Image row holds only the images of the Blog recipe. Do not open it.
- No password, token or connection string is typed or shown. `VIDEO_PASSWORD` stays outside git and the page never
  shows it.
- No file path of the machine is visible (the editor and the slides show none).
- Before publishing, OCR the video at one frame per second and at every scene change, as the skill asks, and search
  for real names, email addresses and tokens.
