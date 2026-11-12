# Recording plan: Extend GraphQL

Audience: developer. Most of the video is slides (`slides.py`); three short chapters are browser recordings of
GraphiQL. The recorder drives a browser only, so the code, the file names and the command lines of this video are
slides, never clips.

File references are to the Orchard Core source at commit `92db4a83` (`src/...` of `OrchardCMS/OrchardCore`). A selector
below stands for `scene.Page.Locator(...)`.

## What the clips show, and why no custom module is needed

The slides show real code of Orchard Core (the Autoroute and Localization modules, and the content management GraphQL
library). The clips show what that code produces, so they run on a stock Orchard Core site: the Autoroute module
registers an object type, an input type, an alias provider and an index property provider for `AutoroutePart`
(`src/OrchardCore.Modules/OrchardCore.Autoroute/GraphQL/Startup.cs:11-21`), and the Localization module registers the
`SiteCulturesQuery` schema builder
(`src/OrchardCore.Modules/OrchardCore.Localization/GraphQL/Startup.cs:10-19`). No extra module, project or host code is
built, and no code is typed on camera.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore` at the commit above, run from `src/OrchardCore.Cms.Web` on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`).
- Provisioned by Auto Setup through environment variables, so no setup form and no sign-in form is recorded
  (`src/docs/reference/modules/AutoSetup/README.md`). Tenant `Default`, recipe **Blog**
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, `"name": "Blog"`):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>      # the value of VIDEO_PASSWORD, chosen outside git
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- Outbound calls blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. This video calls no external service,
  so there is no stand-in to run.
- Sign the recorder in once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and
  `VIDEO_PASSWORD=<password>`. The result, `state.json`, is never committed.
- The admin menu stays in its default (expanded) state, so that Tools, GraphiQL is visible in the first clip. GraphiQL
  fills the page to the right of the menu (`src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Views/Admin/Index.cshtml:10`,
  `left: 260px`). If the Explorer, the editor and the results do not all fit at 1536x864 on the contact sheet, see
  `OPEN-QUESTIONS.md`.

## Features to enable before recording (outside the capture)

1. Open `/Admin/Features` (Tools, Features: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:27`).
2. Type `GraphQL` in the **Search** box, tick the feature **GraphQL** (id `OrchardCore.Apis.GraphQL`), clear the box,
   type `Localization`, tick **Localization** (id `OrchardCore.Localization`). The ticks are kept while the search
   changes; if they are not, do the two features one after the other.
3. Open **Bulk Actions** and choose **Enable** (`src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:34-38`).
4. Check on the page that both features are listed as enabled. The site restarts its tenant; reload the page once.

## Seed data (outside the capture, fake data only)

### Cultures

The `siteCultures` clip reads the supported cultures of the site, so they are set to known values: exactly `en-US`,
then `fr-FR`, with `en-US` as the default.

1. Open Settings, Localization, Cultures (`src/OrchardCore.Modules/OrchardCore.Localization/AdminMenu.cs:34-38`).
2. The editor ("Define which cultures are supported by the site.",
   `src/OrchardCore.Modules/OrchardCore.Localization/Views/LocalizationSettings.Edit.cshtml:31`) is a table with the
   columns **Culture** and **Default culture**. A new site does not start empty: the only supported culture, and the
   default, is the installed UI culture of the machine
   (`src/OrchardCore.Modules/OrchardCore.Localization/Models/LocalizationSettings.cs:25-26`). Look at the table first,
   then make it read `en-US`, `fr-FR`, from top to bottom, whatever it held (the rows keep the order in which they were
   added):
   - If the table does not hold `en-US`, pick `en-US` in the drop-down list under the table (the options read
     `en-US (English (United States))`; cultures already in the table are not offered) and press **Add culture**.
   - Remove every row that is not `en-US` with the trash button of its row (title "Remove culture"), `fr-FR`
     included if it was there.
   - Pick `fr-FR` (`fr-FR (French (France))`) and press **Add culture**; it goes below `en-US`.

   The labels **Add culture**, **Culture**, **Default culture** and the titles "Remove culture" and "Set as default"
   are in `src/OrchardCore.Modules/OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`; the editor is
   the bundled `culture-settings-editor` component, whose source is not in the tree.
3. Select the radio button of the `en-US` row, in the **Default culture** column. Its title, shown as a tooltip, is
   "Set as default"; it has no visible text. Leave "Fall back to parent culture(s)" as it is.
4. Press **Save** (`src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). The site
   shows "The site has been restarted for the settings to take effect."
   (`src/OrchardCore.Modules/OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132`). Reload once and
   check that the table holds `en-US` (default) and `fr-FR`, and nothing else.

### Three blog posts

Create these three posts, in this order (the GraphQL list shows the newest first):

| Order | Title | Expected path |
|---|---|---|
| 1 | Harvest Notes | `blog/harvest-notes` |
| 2 | Pruning in Winter | `blog/pruning-in-winter` |
| 3 | Orchard Tour Dates | `blog/orchard-tour-dates` |

For each one: Content, Content Items (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68`); on the row of
the item **Blog**, press the button **List BlogPost Items**: its label is built from the technical name of the
contained type, `BlogPost`, not from its display name
(`src/OrchardCore.Modules/OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10`, with no data translation the label is
the name itself, `src/OrchardCore/OrchardCore.Localization.Core/Data/DataLocalizer.cs`); press **Create Blog Post**
(`src/OrchardCore.Modules/OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`); type the title in **Title**
(`src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`); press **Publish**
(`src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`). Leave the body, the subtitle and
the image empty.

The path comes from the pattern of the Blog Post type, `container | display_text | slugify` followed by the slugified
title (`blog.recipe.json:216`), so each post gets `blog/` plus the slug of its title. Leave the recipe's own sample post
in place (its path is `blog/post-1`): `explorer-2` lists four posts, with that one last.

### Check the seed

Open `/Admin/GraphQL`, run `{ blogPost { displayText path } }` and `{ siteCultures { culture default } }`
(the queries of `03-explorer` and `07-run`), and compare with the end states described for `explorer-2` and `run-2`. Then stop the site, and copy its
`App_Data` folder aside together with `state.json`.

## Order of the clips, and what they change

All three clips are read-only: they only open GraphiQL and run queries, and none of them changes the site. They do not
depend on each other and can be recorded in any order, in any number of takes, from the one snapshot made after the
seed. Restore the snapshot only if a take showed something unexpected after a change by hand.

Order in the video: `03-explorer`, `05-where`, `07-run`.

## Common start of the three clips

Each clip loads GraphiQL with its query already in the editor, before `StartCaptureAsync()`. GraphiQL reads the
`query` parameter of the address and uses it as the editor's content
(`src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/js/App.tsx:46` and `:80-98`), so nothing is typed on camera
and the editor's automatic brackets cannot interfere.

1. `scene.OpenAsync("<address of the clip>")` (the addresses are in each clip below; the path is
   `/Admin/GraphQL`, from `src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Controllers/AdminController.cs:9`).
2. Wait until the schema has loaded: the Explorer, `.graphiql-explorer-root`
   (`src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/css/graphiql-orchard.css:5`), lists the root field named
   in the clip. This is the wait for the introspection request of the page; do not start the capture before it.
3. `scene.StartCaptureAsync()`.

The button that runs the query is a control of the GraphiQL library, not of Orchard Core's source: in the version the
module builds it is the play button of the toolbar, `button.execute-button`, titled "Execute Query (Ctrl-Enter)". Click
it with `scene.ClickAsync(...)`. If the selector does not match, run `Recorder probe` on the page and use the button
that carries that title; see `OPEN-QUESTIONS.md`.

The results pane is `.resultWrap`; wait for text in it with a concrete condition (below), never with a fixed pause.

## Clip `03-explorer` (chapter 03: Check it in GraphiQL)

Starts on:

```
/Admin/GraphQL?query=%7B%0A%20%20blogPost%20%7B%0A%20%20%20%20displayText%0A%20%20%20%20path%0A%20%20%7D%0A%7D
```

which puts this query in the editor:

```graphql
{
  blogPost {
    displayText
    path
  }
}
```

Wait for `.graphiql-explorer-root` to contain the text `blogPost`.

- `explorer-1`: actions: `HighlightAsync` on the **GraphiQL** entry under **Tools** in the admin menu (a link whose
  text is `GraphiQL`, the active item of the open **Tools** group; `AdminMenu.cs:18-19` of the GraphQL module), wait 2.5
  seconds, `ClearHighlightsAsync()`, then `HighlightAsync` on the query editor (`.queryWrap`). On screen when the
  narration ends: the editor with the query above, ringed in green, the Explorer to its left, and the results pane
  empty. Label sources: Tools and GraphiQL, `src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/AdminMenu.cs:18-19`.
- `explorer-2`: actions: `ClearHighlightsAsync()`, click the play button (`button.execute-button`), wait until the
  results pane contains the text `"path"`, wait 1.5 seconds, then `HighlightAsync` on the Explorer
  (`.graphiql-explorer-root`) and nothing else. On screen when the narration ends: the results pane showing
  `data.blogPost` with an entry per post, each with `displayText` and `path`; first
  `Orchard Tour Dates` with `blog/orchard-tour-dates`, then `Pruning in Winter` and `Harvest Notes`; and the Explorer
  ringed, with `blogPost` open and `path` checked under it. The label "Explorer" comes from
  `src/OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/js/App.tsx:118`.

## Clip `05-where` (chapter 05: Try the filter)

Starts on:

```
/Admin/GraphQL?query=%7B%0A%20%20blogPost%28where%3A%20%7B%20path_contains%3A%20%22harvest%22%20%7D%29%20%7B%0A%20%20%20%20displayText%0A%20%20%20%20path%0A%20%20%7D%0A%7D
```

which puts this query in the editor:

```graphql
{
  blogPost(where: { path_contains: "harvest" }) {
    displayText
    path
  }
}
```

Wait for `.graphiql-explorer-root` to contain the text `blogPost`.

- `where-1`: actions: `HighlightAsync` on the query editor (`.queryWrap`) with the caption `where`. On screen when
  the narration ends: the editor with the query above, ringed, and the results pane empty. The argument names `where` and
  `path_contains` come from the schema, not from a label: `where` from
  `src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:44-50`, `path` and the lack of
  an `autoroute` level from the collapse of the part (`src/OrchardCore.Modules/OrchardCore.Autoroute/Startup.cs:92-98`,
  `src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/Types/TypedContentTypeBuilder.cs:66-90` and `:122-130`),
  and `path_contains` from `src/OrchardCore/OrchardCore.Apis.GraphQL.Abstractions/Queries/WhereInputObjectGraphType.cs:57`.
- `where-2`: actions: `ClearHighlightsAsync()`, click the play button, wait until the results pane contains the text
  `harvest-notes`, wait 1.5 seconds. On screen when the narration ends: the results pane showing a single entry,
  `Harvest Notes` with `blog/harvest-notes`, and none of the other posts.

## Clip `07-run` (chapter 07: Run your query)

Starts on:

```
/Admin/GraphQL?query=%7B%0A%20%20siteCultures%20%7B%0A%20%20%20%20culture%0A%20%20%20%20default%0A%20%20%7D%0A%7D
```

which puts this query in the editor:

```graphql
{
  siteCultures {
    culture
    default
  }
}
```

Wait for `.graphiql-explorer-root` to contain the text `siteCultures`.

- `run-1`: actions: `HighlightAsync` on the query editor (`.queryWrap`). On screen when the narration ends: the editor
  with the query above, ringed, and the results pane empty. The field name `siteCultures` is the field `SiteCultures`
  of `src/OrchardCore.Modules/OrchardCore.Localization/GraphQL/SiteCulturesQuery.cs:48`, written in camel case by the
  schema; `culture` and `default` are in
  `src/OrchardCore.Modules/OrchardCore.Localization/GraphQL/CultureQueryObjectType.cs:18` and `:22`.
- `run-2`: actions: `ClearHighlightsAsync()`, click the play button, wait until the results pane contains the text
  `fr-FR`, wait 1.5 seconds, then `HighlightAsync` on the row `siteCultures` of the Explorer
  (`.graphiql-explorer-root` filtered by the text `siteCultures`; scroll the Explorer first with `scene.ScrollAsync` if
  the row is out of view, and clear the highlight before any scroll). On screen when the narration ends: the results
  pane showing `data.siteCultures` with `en-US` and `default: true`, then `fr-FR` and `default: false`, and the
  Explorer with the `siteCultures` row ringed, in the same list as the content type fields (`blog`, `blogPost` and the
  others).

## Slide steps

The slides are drawn by `slides.py`; they need no recording. One line each:

- `schema-1`: three cards: registered types, schema builders, and empty roots (the first two bright).
- `schema-2`: three cards: one schema per culture, an identifier per builder, and the content definitions.
- `schema-3`: four cards: the object type, the input type and index, the schema builder, and the GraphQLFilter.
- `schema-4`: three cards: reference the libraries, register in a Startup class, test in GraphiQL.
- `part-1`: the `AutoroutePart` class, trimmed with `// ...` before and after the two properties, with two note cards.
- `part-2`: `AutorouteQueryObjectType` with the class line, `Name`, `Description` and `Field` lines bright.
- `part-3`: the `Startup` class registering the object type, with the attribute line and the `AddObjectGraphType` line bright.
- `part-4`: three cards: matched by name (the part class and the part on the content type), one field per part, and every
  content type unless its GraphQL options hide or collapse the part.
- `filter-1`: `AutorouteInputObjectType`, with the class line, the constructor and the `AddScalarFilterFields` call bright.
- `filter-2`: `AutoroutePartIndexAliasProvider`, with the class line and the alias, index and index type lines bright.
- `filter-3`: the `Startup` class with its four registrations, the input type, alias provider and index lines bright.
- `query-1`: `SiteCulturesQuery.BuildAsync`, with the field and its name, description, type and resolver bright.
- `query-2`: the resolver line of `BuildAsync`, and `ResolveAsync` with the request services line bright.
- `query-3`: the `SiteCulture` class (from `SiteCulture.cs`) and `CultureQueryObjectType` with its two fields.
- `query-4`: the `Startup` class with the two registrations, and `GetIdentifierAsync` returning an empty string.
- `query-5`: the `IsHiddenByDefault` check, the `ConfigureContentType` option that hides the query, and the
  `RequirePermission` call.
- `hook-1`: the `ContentItemFilters` class with the two methods `PreQueryAsync` and `PostQueryAsync`.
- `hook-2`: two cards: what `PreQueryAsync` and `PostQueryAsync` do in `ContentItemFilters`.
- `hook-3`: the registration of a filter with `AddGraphQLFilterType`, and the loop that runs the filters.
- `source-1`: four cards: the modules to read (Autoroute and Alias, Localization and Layers, Media) and the content
  management GraphQL library, which holds `ContentItemFilters`.

Check the slides once: from `videos/graphql-extend`, with `.agents/skills/orchardcore-video-producer/scripts` on
`PYTHONPATH`, call `slides.slide(step_id, title, "Chapter 1")` for each slide step in memory; nothing is saved.

## Checks for private data

- The only people, companies and addresses on screen are the invented ones above: the three post titles, the paths
  `blog/...`, and the cultures `en-US` and `fr-FR`. The site is new and has no other records. The recipe's own sample
  post ("Man must explore, and this is exploration at its greatest") is part of the Blog recipe, not of a real site.
- Nothing shows a password, an email address, a token or the address of a real server. The browser's address bar is
  not part of the capture. The admin user is never displayed.
- OCR the video at one frame per second and at every scene change, as the skill describes, and search the text for any
  string that is not in the lists above.
