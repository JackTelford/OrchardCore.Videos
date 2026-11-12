# Recording plan: GraphQL: Settings, Permissions and Hidden Fields

This plan is for the person who records the video. It needs no decision: every value, label and wait is written here.
A selector below stands for `scene.Page.Locator(...)`. Labels are quoted as the Orchard Core source spells them, with
the file and line they come from (paths are relative to the Orchard Core checkout the video was written from, commit
`92db4a83`). Clips are browser recordings; everything else is a slide, drawn by `slides.py`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, built and run on
  `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. No clip
  depends on the hosting environment (Development or Production).
- **Auto Setup** (no setup or sign-in form is recorded), through environment variables set before the first request:

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls an outside service.
- **Tenant**: `Default` only.
- **Features to enable before recording**, off camera, in **Tools**, **Features** (`OrchardCore.Features`,
  `AdminMenu.cs:26-27`): search for `GraphQL` and enable the feature **GraphQL** (`OrchardCore.Apis.GraphQL`). The
  Blog recipe does not enable it. The GraphQL options of the content type editor (Hide, Collapse, Prevent Field Name
  Collision) are only registered when this feature is enabled
  (`OrchardCore.ContentTypes/GraphQL/Startup.cs:8`), and the **Execute GraphQL** permissions only appear in the role
  editor then. Every other feature the clips use (Roles, Content Types, Contents, Autoroute, Markdown) is part of the
  Blog recipe.
- **Settings**: none to save. The GraphQL options keep their defaults.
- **External services**: none, and no local stand-in is needed.
- **Seed data**: none to create. The clips use the content types, parts and roles that the Blog recipe and the
  Roles module create: the content types **Article**, **Blog Post** and **Page** (technical names `Article`,
  `BlogPost`, `Page`), the part `MarkdownBodyPart` and the part `AutoroutePart` on **Blog Post**, and the roles
  **Anonymous**, **Authenticated** and **Administrator**. No clip shows a content item.
- **Sign-in**: `Recorder setup` signs in once as user `admin` with the password of `VIDEO_PASSWORD` and saves
  `state.json` (git-ignored). The sign-in form is never on camera.
- **Check before the snapshot** (open each page once, off camera, and stop if one differs):
  - `/Admin/Roles/Edit/Anonymous`: in the **GraphQL** group, the **Allow** boxes of "Execute GraphQL." and
    "Execute GraphQL Mutations." are not checked.
  - `/Admin/ContentTypes/Edit/Article`: the **Hide** box is not checked, and no content type is hidden.
  - `/Admin/GraphQL`: the Explorer on the left lists `article`, `blog`, `blogPost` and `page` among its fields.

## Snapshots, order and dependencies

Stop the site and copy its `App_Data` folder and `state.json` aside after the check above. This is snapshot A.

| Clip | Needs | Changes the site |
|---|---|---|
| `03-parts` | Snapshot A. | No: it ends on **Cancel**, and never saves. |
| `02-roles` | Snapshot A. | Yes: the **Anonymous** role is saved with "Execute GraphQL." allowed. |
| `03-type` | Snapshot A. | Yes: the **Article** content type is saved with **Hide** selected. |

The three clips do not depend on each other. Record them in this order: `03-parts`, `02-roles`, `03-type`. **Restore
snapshot A (stop the site, restore the copy, start it) before every new take of `02-roles` and `03-type`**, and
before recording the three clips together: a take of `03-type` needs `article` to be listed in the Explorer at its
start, and a take of `02-roles` needs the **Allow** box to be empty.

The admin menu starts collapsed. Where a step says "click the menu", the groups are opened on camera, in the order
the narration names them.

## Clips

### `02-roles`: allow Execute GraphQL on the Anonymous role

Starts on `/Admin` (the admin home), signed in as `admin`, with the sidebar menu collapsed. Before
`StartCaptureAsync`, wait for the page to finish loading. Chapter 02 of the storyboard.

- `roles-1`
  - Actions: click the sidebar entry **Access Control** (`OrchardCore.Roles/AdminMenu.cs:18`), then the entry
    **Roles** (`AdminMenu.cs:19`) with `ClickAndWaitForNavigationAsync`. On the **Roles** page
    (`Views/Admin/Index.cshtml:15`), click the **Edit** button of the Anonymous row:
    `a.edit[href$='/Roles/Edit/Anonymous']` (`Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`), with
    `ClickAndWaitForNavigationAsync`. On the edit page, click `#search-box` (`Views/Admin/_RolePermissions.cshtml:49`,
    placeholder "Search permissions") and `TypeAsync` the text `GraphQL`. Then `HighlightAsync` the legend of the
    **GraphQL** group.
  - Waits: after the navigation, wait until the heading text "Edit 'Anonymous' Role" is visible
    (`Views/Admin/Edit.cshtml:6`). After typing, wait until the row `tr[data-text='Execute GraphQL.']` is visible.
  - On screen when the narration ends: the page "Edit 'Anonymous' Role", the search box holding `GraphQL`, and only
    the **GraphQL** group, with the two rows "Execute GraphQL." and "Execute GraphQL Mutations." in the
    **Permission** column. Both **Allow** boxes are empty. The group legend reads "GraphQL" followed by the hint
    "(OrchardCore.Apis.GraphQL)" (`Controllers/AdminController.cs:412-427`).
  - Label sources: `OrchardCore.Apis.GraphQL.Abstractions/GraphQLPermissions.cs:10` and `:12` for the two permission
    descriptions; `_RolePermissions.cshtml:68-70` for the column titles **Permission**, **Allow**, **Effective**.
- `roles-2`
  - Actions: `ClickAsync` the **Allow** box of "Execute GraphQL.": `[id='Checkbox.ExecuteGraphQL']`
    (`_RolePermissions.cshtml:131`; the dot needs the attribute syntax). Then `ClickAndWaitForNavigationAsync` the
    **Save** button `button.save` (`Views/Admin/Edit.cshtml:33`).
  - Waits: after the click on the box, wait until `[id='Checkbox.ExecuteGraphQL']` is checked. After the save, wait
    until the **Roles** list is shown again.
  - On screen when the narration ends: the **Roles** page, with the message "Role updated successfully."
    (`Controllers/AdminController.cs:213`) visible.
- `roles-3`
  - Actions: `HighlightAsync` the message "Role updated successfully." (`GetByText`), and hold. Do not click anything.
  - On screen when the narration ends: the same **Roles** page with the highlighted message and the Anonymous row
    in the list.

### `03-type`: hide the Article content type

Starts on `/Admin/GraphQL` (the GraphiQL screen, `Controllers/AdminController.cs:9`), signed in as `admin`, with the
sidebar menu collapsed. Before `StartCaptureAsync`, wait until `.graphiql-explorer-root` contains the field `article`
(`.graphiql-explorer-article`), so the schema is loaded. Chapter 03 of the storyboard.

- `type-1`
  - Actions: `HighlightAsync` the Explorer node `.graphiql-explorer-root .graphiql-explorer-article` (the Explorer
    node of a field carries the class `graphiql-explorer-` plus the field name). Then `ClearHighlightsAsync`. Click the
    sidebar entry **Design** (`OrchardCore.ContentTypes/AdminMenu.cs:23`), then **Content Definition** (`:24`), then
    **Content Types** (`:25`), the last with `ClickAndWaitForNavigationAsync`. On the **Content Types** page
    (`Views/Admin/List.cshtml:6`), click the **Edit** button of the Article row:
    `a[href$='/ContentTypes/Edit/Article']` (`Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`), with
    `ClickAndWaitForNavigationAsync`.
  - Waits: after the last navigation, wait until the breadcrumb "Edit Content Type - Article" is visible
    (`Views/Admin/Edit.cshtml:63`).
  - On screen when the narration ends: the page "Edit Content Type - Article", showing the type's settings from the top.
- `type-2`
  - Actions: `ScrollAsync` until the checkbox labeled **Hide** and its hint "Check to hide type from the GraphQL
    schema." are in view (`OrchardCore.ContentTypes/Views/GraphQLContentTypeSettings.Edit.cshtml:13` and `:20`;
    the box is the `input[type=checkbox]` of the `form-check` that contains the hint). `ClickAsync` the box, then
    `ClickAndWaitForNavigationAsync` the **Save** button `button.save` (`Views/Admin/Edit.cshtml:215`).
  - Waits: after the click, wait until the box is checked. After the save, wait until the **Content Types** list is
    shown.
  - On screen when the narration ends: the **Content Types** list with the message "Content type updated
    successfully." (`Controllers/AdminController.cs:225`).
- `type-3`
  - Actions: `NavigateAsync("/Admin/GraphQL")`. Wait until `.graphiql-explorer-root` lists `blogPost`
    (`.graphiql-explorer-blogPost`) and has no `.graphiql-explorer-article`. `HighlightAsync` the Explorer panel
    (`.graphiql-explorer-root`).
  - On screen when the narration ends: the GraphiQL screen with the Explorer listing `blog`, `blogPost` and `page`
    (and the other types), and no `article`.

### `03-parts`: Collapse and Hide on a part

Starts on `/Admin/ContentTypes/Edit/BlogPost` (the page "Edit Content Type - Blog Post"), signed in as `admin`.
Before `StartCaptureAsync`, wait until the **Parts** list (`Views/Admin/Edit.cshtml:144`) is shown. Chapter 03 of the
storyboard. Do not use the part with the same name as the type (`BlogPost`): it has no GraphQL options.

- `parts-1`
  - Actions: `HighlightAsync` the row of the Markdown body part (the `li` that holds
    `input[name='OrderedPartNames'][value='MarkdownBodyPart']`; its text is read from the page, see the open
    questions), then `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` its **Edit** button:
    `a[href$='/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit']` (`Views/Admin/Edit.cshtml:163`). On the
    page "Edit Part - ..." (`Views/Admin/EditTypePart.cshtml:22`), `HighlightAsync` the labels **Collapse** and
    **Hide**, then `ClearHighlightsAsync` and `ClickAsync` the **Collapse** box
    (`input[id$='Settings_Collapse']`, `OrchardCore.ContentTypes/Views/GraphQLContentTypePartSettings.Edit.cshtml:19`).
  - Waits: after the click, wait until `input[id$='Settings_PreventFieldNameCollision']` is visible: the page hides
    the box and its label until **Collapse** is checked (`Assets/js/graphql-content-type-part-settings.ts:11-22`). The
    hint under it, "Check to prevent fields' names collisions in the GraphQL schema when they are collapsed."
    (`GraphQLContentTypePartSettings.Edit.cshtml:38`), is not hidden by that script, so it is on screen before the
    click, without a box above it.
  - On screen when the narration ends: the part page with **Collapse** checked and, under it, **Prevent Field Name
    Collision** (label from `GraphQLContentTypePartSettings.Edit.cshtml:36`) unchecked, and **Hide** (`:46`)
    unchecked below. The hints "Check to collapse fields in the GraphQL schema." (`:27`) and "Check to hide part from
    the GraphQL schema." (`:54`) are visible.
- `parts-2`
  - Actions: click **Cancel** (`a.cancel`, `Views/Admin/EditTypePart.cshtml:112`) with
    `ClickAndWaitForNavigationAsync`; this returns to "Edit Content Type - Blog Post" and discards the box that was
    checked. Never click **Save** in this clip. `ClickAndWaitForNavigationAsync` the **Edit** button of the Autoroute
    part: `a[href$='/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit']`. Then `HighlightAsync` the hint
    "NB. Setting is collapsed by default, and cannot be overwritten." (`GetByText`).
  - Waits: after the last navigation, wait until the hint is visible.
  - On screen when the narration ends: the part page of the Autoroute part with **Collapse** checked and disabled, its
    hint highlighted, **Prevent Field Name Collision** visible and **Hide** below
    (`GraphQLContentTypePartSettings.Edit.cshtml:12-23`, and `Autoroute/Startup.cs:94-96` for the code that collapses
    the part).

## Slides

Slides are drawn by `slides.py`; nothing is recorded for them. Each line says what the slide shows.

- `options-1`: the `OrchardCore`, `Apis`, `GraphQL` section of `appsettings.json` with its seven options, and three
  notes: under OrchardCore, shell configuration a tenant can override, every option optional.
- `options-2`: three cards on result limits, `DefaultNumberOfResults` and `MaxNumberOfResults` bright.
- `options-3`: the same cards, `MaxNumberOfResultsValidationMode` bright.
- `options-4`: four cards on protection, `MaxDepth`, `MaxComplexity` and `FieldImpact` bright.
- `options-5`: the same cards, `ExposeExceptions` bright.
- `levels-1`: four cards on where permissions apply, "Execute GraphQL" bright.
- `levels-2`: the code of `GraphQLPermissions.cs` with `ExecuteGraphQL` implied by `ExecuteGraphQLMutations`.
- `levels-3`: the same four cards, "View own content" bright.
- `levels-4`: the same four cards, "The contentItem query" bright.
- `field-1`: the code of `UserPickerFieldQueryObjectType.cs` with `RequirePermission`, and three notes.
- `code-1`: `IgnoreField<ContentItemType>(nameof(ContentItem.Owner))` in a `Startup` class, and two notes.
- `code-2`: `ConfigurePart<AutoroutePart>` and `ConfigureContentType("SiteLayers", ...)`, and two notes.
- `code-3`: `DiscoverableSterotypes.Add("ExampleStereotype")`, and two notes.
- `code-4`: three cards: Hide selected in the editor, `Hidden` set to false in code, the content type hidden.

## Checks for private data

- The site is new and holds only the Blog recipe's sample content and the fake user `admin` (`admin@example.com`).
  No clip shows a content item, a user's email or the password.
- The password never appears on camera: the sign-in is done by `Recorder setup`, off camera.
- Watch the contact sheets (`review.py`) and look for the admin user menu: it shows the user name `admin` only.
- Run OCR at one frame per second and at every scene change, as the skill asks, and search for `@` other than
  `example.com`, for a password, and for any host other than `localhost`.
