# Recording plan: Shape Alternates

This plan lets a person record the video without deciding anything. The audience is developers, so the narration
talks about files, and the recording only shows what a browser shows: a page before a template exists, the page
after, and the Debugging settings. The code of the templates is on slides; the recorder cannot create files, so a
person adds the two template files between takes, as described under "Clip order".

The plan was written from the Orchard Core source at commit `92db4a83`. Use a checkout of `OrchardCMS/OrchardCore`
at that commit or later. Where the plan says "(source)" it names the file and line to check a label or a behavior.

## The site it needs

- A throwaway checkout of `OrchardCMS/OrchardCore`, run from `src/OrchardCore.Cms.Web`, with its own `App_Data`
  folder outside both repositories (`ORCHARD_APP_DATA`). Nothing is copied from a real site.
- Provisioned by Auto Setup (`src/OrchardCore.Cms.Web/Program.cs:9` calls `AddSetupFeatures("OrchardCore.AutoSetup")`),
  so no setup screen is recorded. Recipe `Blog` (`blog.recipe.json`, `"name": "Blog"`), site name `Orchard Core`,
  tenant `Default`, time zone `America/Los_Angeles`, database `Sqlite`.
- Sign in as user `admin`, email `admin@example.com`, with the password in `VIDEO_PASSWORD` (chosen outside git;
  written here as `<password>`).
- The `Blog` recipe already enables everything the video needs: Alias, Autoroute, Contents, Content Types, Deployment
  (for the JSON Import page) and Templates, and it makes `TheBlogTheme` the site theme
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, steps `feature` and `themes`). Enable no other
  feature and change no other setting before recording.
- No external service. Block outbound calls so the site never calls out (the layout of the theme links Google Fonts,
  which then falls back to local fonts).

```bash
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
export OrchardCore__AutoSetup__AutoSetupPath=""
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
cd <OrchardCore checkout>/src/OrchardCore.Cms.Web && dotnet run --no-launch-profile
```

The first request to `http://localhost:5010/` runs Auto Setup. Wait for the Blog home page, then sign the recorder in
once (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which writes `state.json` (git-ignored).
The front end of the theme shows no user menu (`TheBlogTheme/Views/Layout.liquid`), so the same `state.json` serves
every clip.

## Seed data, created outside the capture

The video needs one content type that the theme has no template for, `Event`, with three events. Import this recipe
with **Tools** > **Deployments** > **JSON Import** (source: `OrchardCore.Deployment/AdminMenu.cs:18,19,30`): paste it into
the **JSON** editor and select **Import** (`OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`). The page answers
"Recipe imported successfully!" (`OrchardCore.Deployment/Controllers/ImportController.cs:185`).

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Event",
          "DisplayName": "Event",
          "Hidden": false,
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Draftable": true,
              "Versionable": true,
              "Listable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "TitlePart",
              "Name": "TitlePart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "0"
                }
              }
            },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "events/{{ Model.ContentItem | display_text | slugify }}"
                },
                "ContentTypePartSettings": {
                  "Position": "1"
                }
              }
            },
            {
              "PartName": "HtmlBodyPart",
              "Name": "HtmlBodyPart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Editor": "Wysiwyg",
                  "Position": "2"
                }
              }
            }
          ]
        }
      ],
      "ContentParts": []
    },
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4k7m2xq9w5t8r3b6n1v0c4zhsa",
          "ContentType": "Event",
          "DisplayText": "Harvest Festival",
          "Latest": true,
          "Published": true,
          "TitlePart": {
            "Title": "Harvest Festival"
          },
          "AutoroutePart": {
            "Path": "events/harvest-festival"
          },
          "HtmlBodyPart": {
            "Html": "<p>Join us on the village green for music, food stalls and a lantern parade.</p><p>Entry is free. Stalls open at ten in the morning.</p>"
          }
        },
        {
          "ContentItemId": "4d9f3p7y2e6j8u1g5a0l4xtbqo",
          "ContentType": "Event",
          "DisplayText": "Night Market",
          "Latest": true,
          "Published": true,
          "TitlePart": {
            "Title": "Night Market"
          },
          "AutoroutePart": {
            "Path": "events/night-market"
          },
          "HtmlBodyPart": {
            "Html": "<p>Street food, crafts and live music, with stalls open until midnight.</p><p>Entry is free after six in the evening.</p>"
          }
        },
        {
          "ContentItemId": "4h2s6c9n3v7z1k5m8w0r4eyjpd",
          "ContentType": "Event",
          "DisplayText": "Garden Tour",
          "Latest": true,
          "Published": true,
          "TitlePart": {
            "Title": "Garden Tour"
          },
          "AutoroutePart": {
            "Path": "events/garden-tour"
          },
          "HtmlBodyPart": {
            "Html": "<p>Walk through six private gardens of the old town with a local guide.</p><p>Each tour is limited to twenty people.</p>"
          }
        }
      ]
    }
  ]
}
```

All names, texts and numbers are invented. After the import, open each of `/events/harvest-festival`,
`/events/night-market` and `/events/garden-tour` in a private window and check that it answers with the title of the
event in a banner and the text below it.

Then stop the site and copy its `App_Data` folder aside. This is the snapshot **S0**: the site with the three events and
no template file of the video.

## The two template files

Keep these two files in a folder outside both repositories (for example `templates/`). They go into
`<OrchardCore checkout>/src/OrchardCore.Themes/TheBlogTheme/Views/`, one at a time, as the clip order says. They are
the same text as the slides `type-1`, `type-2`, `item-2`, so the video shows what is recorded.

`Content-Event.liquid`:

```liquid
{% zone "Header" %}
    <header class="masthead" style="background-image: url('{{ "~/TheBlogTheme/assets/img/home-bg.jpg" | href }}')">
        <div class="container position-relative px-4 px-lg-5">
            <div class="row gx-4 gx-lg-5 justify-content-center">
                <div class="col-md-10 col-lg-8 col-xl-7">
                    <div class="site-heading">
                        <h1>{{ Model.ContentItem.DisplayText }}</h1>
                        <span class="subheading">Event</span>
                    </div>
                </div>
            </div>
        </div>
    </header>
{% endzone %}

{{ Model.Content | shape_render }}
```

`Content-Slug-events-night-market.liquid`:

```liquid
{% zone "Header" %}
    <header class="masthead" style="background-image: url('{{ "~/TheBlogTheme/assets/img/contact-bg.jpg" | href }}')">
        <div class="container position-relative px-4 px-lg-5">
            <div class="row gx-4 gx-lg-5 justify-content-center">
                <div class="col-md-10 col-lg-8 col-xl-7">
                    <div class="site-heading">
                        <h1>{{ Model.ContentItem.DisplayText }}</h1>
                        <span class="subheading">Featured event</span>
                    </div>
                </div>
            </div>
        </div>
    </header>
{% endzone %}

{{ Model.Content | shape_render }}
```

The theme has no template for `Event`, so the page of an event is rendered by the theme's own `Content.liquid`
(`TheBlogTheme/Views/Content.liquid`): a banner with `post-bg.jpg` and the title, and the body below. (The theme does
have templates for other content types, such as `Content-BlogPost.liquid`; none of them applies to an `Event`.)
`Content-Event.liquid` is that file with three differences: the two comment lines at the top of the zone are left out
(the slides leave them out too), the banner image is `home-bg.jpg` instead of `post-bg.jpg`, and a `span` with the word
`Event` sits under the title. The narration calls the last two "two changes". The second file differs from the first by
the banner image (`contact-bg.jpg`) and by the words `Featured event`. All four images (`post-bg.jpg`, `home-bg.jpg`,
`contact-bg.jpg`, `about-bg.jpg`) are in `TheBlogTheme/wwwroot/assets/img/`.

The files a theme provides are listed when the project is built: the build embeds them and writes the list into the
assembly (`OrchardCore.Module.Targets/OrchardCore.Module.Targets.targets:52-85`), and the site reads that list
(`OrchardCore.Abstractions/Modules/Module.cs:32-35`, `ModuleEmbeddedFileProvider.cs:52-55`). A file copied into `Views/`
is therefore found only after the next build. Every "start the site" below means the same `dotnet run --no-launch-profile`
command as above, which builds first; never add `--no-build`, and never start a copy of an earlier build.

## Clip order, and the state each clip needs

Record in this order. A site state is the snapshot **S0** plus the template files listed.

| # | Clip | Site state | Before recording |
|---|------|------------|------------------|
| 1 | `03-before` | **S0**: no template file in `Views/` | Start the site on the S0 snapshot. Make sure neither file is in `Views/`. Request each event page once, outside the capture, so the shape table is built. |
| 2 | `03-after` | S0 + `Content-Event.liquid` | Stop the site, copy `Content-Event.liquid` into `Views/`, start the site with `dotnet run` (it builds the theme again), request the three event pages once outside the capture. |
| 3 | `04-after` | S0 + both files | Stop the site, copy `Content-Slug-events-night-market.liquid` into `Views/`, start the site with `dotnet run`, request the three event pages once. |
| 4 | `05-debug` | S0 + both files, then **changes the site** (the debugging setting is saved) | The site of clip 3 still running. |

Stop the site by its process id, never by image name. The two files are not in `App_Data`, so restoring S0 does not
remove them: to record again from clip 1, delete both files from `Views/` and restore S0. To record clip 4 again,
open `/Admin/Settings/debugging`, clear **Write shape debug information**, select **Save**, or restore S0 and put both
files back.

The clips depend on each other only through this order. Slides need nothing from the site. Run `scripts/narrate.py`
first, so the recorder knows the length of each step.

### Checks before the first take

With both files in place and the debugging setting on (clip 4's end state), the source of each page shows which template
rendered the `Content` shape. Fetch it outside the capture, for example
`curl -s http://localhost:5010/events/night-market | grep "shape-start type:Content "`, and check:

- `/events/harvest-festival` and `/events/garden-tour`: the binding is `content__event`, from `Content-Event.liquid`.
- `/events/night-market`: the binding is `content__slug__events__night__market`, from
  `Content-Slug-events-night-market.liquid`.
- In both comments the text after `=>` is the path of the file inside the site, which should start with
  `Areas/TheBlogTheme/Views/` (`OrchardCore.Abstractions/Modules/Application.cs:12`, `ShapeTemplateBindingStrategy.cs:75-78`)
  and end with `(liquid)`.

The slide `output-1` shows that comment. Compare its text with what the page really contains; the points to confirm are in
`OPEN-QUESTIONS.md`. Turn the setting off again before recording clips 1 to 3 again.

## Clips

### `03-before` (chapter 03, clip `03-before`)

Starts on `/events/harvest-festival`, opened with `OpenAsync` before the capture starts. Site state: S0.

- `before-1`. Actions: `HighlightAsync(scene.Page.Locator("header.masthead .site-heading"), "Title only")`, then
  `PauseAsync(1200)`. On screen when the narration ends: the Harvest Festival page, with the banner image `post-bg.jpg`
  and the title "Harvest Festival" in the banner, nothing under the title, a green ring around the heading, and the
  text of the event below the banner. Source of the markup: `OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:4-14`.
- `before-2`. Actions: `ClearHighlightsAsync()`, `NavigateAsync("/events/night-market")`, `PauseAsync(900)`,
  `NavigateAsync("/events/garden-tour")`. Keep the actions shorter than the narration. On screen when the narration
  ends: the Garden Tour page, with the same banner image `post-bg.jpg` and no text under the title (the same look as
  the first page).

### `03-after` (chapter 03, clip `03-after`)

Starts on `/` (the blog home page), opened with `OpenAsync` before the capture starts. Site state: S0 +
`Content-Event.liquid`.

- `type-3`. Actions: `NavigateAsync("/events/harvest-festival")`, `PauseAsync(800)`,
  `HighlightAsync(scene.Page.Locator("header.masthead .site-heading .subheading"), "Event")`. On screen when the
  narration ends: the Harvest Festival page, with the banner image `home-bg.jpg` (a different picture from `post-bg.jpg`
  in clip `03-before`), the title, the word "Event" under the title inside a green ring, and the text below.
- `type-4`. Actions: `ClearHighlightsAsync()`, `NavigateAsync("/events/night-market")`, `PauseAsync(900)`,
  `NavigateAsync("/events/garden-tour")`. On screen when the narration ends: the Garden Tour page with the banner image
  `home-bg.jpg` and the word "Event" under the title.

### `04-after` (chapter 04, clip `04-after`)

Starts on `/events/harvest-festival`, opened with `OpenAsync` before the capture starts. Site state: S0 + both files.

- `item-3`. Actions: `NavigateAsync("/events/night-market")`, `PauseAsync(800)`,
  `HighlightAsync(scene.Page.Locator("header.masthead .site-heading .subheading"), "Featured event")`. On screen when
  the narration ends: the Night Market page, with the banner image `contact-bg.jpg` (a third picture), the title, the
  words "Featured event" under it inside a green ring, and the text below.
- `item-4`. Actions: `ClearHighlightsAsync()`, `NavigateAsync("/events/harvest-festival")`. On screen when the narration
  ends: the Harvest Festival page with the banner image `home-bg.jpg` and the word "Event" under the title, as in
  clip `03-after`.

### `05-debug` (chapter 05, clip `05-debug`)

Starts on `/Admin/Settings/debugging`, opened with `OpenAsync` before the capture starts, signed in with `state.json`.
The admin menu is expanded on **Settings** > **Debugging** before the capture (menu: `OrchardCore.Settings/AdminMenu.cs:36,46`).
Site state: S0 + both files, debugging setting off. This clip changes the site.

- `debug-1`. Actions: `HighlightAsync(scene.Page.Locator("label", new() { HasText = "Write shape debug information" }), "Off by default")`,
  `PauseAsync(1200)`, `ClearHighlightsAsync()`, `ClickAsync` on the same label (it toggles the check box), then
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))`. The label is in
  `OrchardCore.Settings/Views/DebugSettings.Edit.cshtml:7`; the **Save** button in
  `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; the page answers "Site settings updated successfully."
  (`OrchardCore.Settings/Controllers/AdminController.cs:119`). On screen when the narration ends: the Debugging page
  after it reloaded, **Write shape debug information** checked.
- `debug-2`. Actions: `NavigateAsync("/events/night-market")`. On screen when the narration ends: the Night Market page,
  with the banner image `contact-bg.jpg` and the words "Featured event" under the title. The comments are in the HTML
  and invisible; the next slide shows them.

## Slides

One line each. The text of each slide is drawn by `slides.py`.

- `choose-1`: three cards: a page is made of shapes, each shape has a type, a template renders the type `Content`.
- `choose-2`: three cards: the last alternate added is tried first, the earlier ones next, the shape type last.
- `choose-3`: a table of file names and shape names (`Content-Event.liquid` and others), and three rules below it.
- `choose-4`: three cards: a module provides a default template, the active theme replaces it, a more specific name
  targets less.
- `names-1`: the shape names and file names for a content type (`Content__Event`, `Content_Summary__Event`).
- `names-2`: the shape names and file names for a path and an alias, and which part each one needs.
- `names-3`: three cards in order of priority: path and alias, content type, shape type.
- `type-1`: the code of `Content-Event.liquid`, all lines bright.
- `type-2`: the same code, with the two changed lines bright (the banner image and the word `Event`).
- `item-1`: the path `events/night-market`, its shape name and its file name.
- `item-2`: the code of `Content-Slug-events-night-market.liquid`, with the two changed lines bright.
- `output-1`: the comments of the page source, one line for `shape-start`, one for `shape-end`, and what each part says.
- `own-1`: the Liquid `shape` tag and the Razor `menu` tag helper with an `alternate` attribute, and the `metadata` element.
- `own-2`: three cards: create the shape, add the alternate, then Orchard Core picks the template.
- `own-3`: the shape table provider of the Alias module, trimmed.
- `own-4`: the registration of the provider, and three notes beside it.

## Checks for private data

- Everything on screen is invented: the three events, the site name `Orchard Core`, the user `admin` and the email
  `admin@example.com`. The password is never typed on camera and never written in a file of the repository.
- Do not commit `state.json`, the `templates/` folder, a copy of `App_Data` or any media.
- OCR the video at one frame per second and at every scene change, then at five frames per second, and search for
  email addresses (only `example.com` is allowed), host names (only `localhost`), tokens and personal names.
