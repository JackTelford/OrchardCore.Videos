# Recording plan: Data Sources: Expose Your Module's Data to Pipelines

A developer video. Only one clip is a browser recording, `02-consume`: it shows the consumer side of a data source,
the data set dialog and the preview of the Data Pipelines designer, with the **Users** data source that ships with
the Data Pipelines Core feature. Everything else is a card or a slide, drawn from `storyboard.json` and `slides.py`
(no recording). The code on the slides is not run, so no module of your own has to be built or installed.

In this plan a selector such as `[data-cy='toolbar-add-data-source']` stands for
`scene.Page.Locator("[data-cy='toolbar-add-data-source']")`. File references are to the Orchard Core source at the
commit the video is recorded from (`src/OrchardCore.Modules/OrchardCore.DataPipelines` is written `DP/`).

## The site it needs

- A local, throwaway site: `src/OrchardCore.Cms.Web` of an Orchard Core checkout that has the Data Pipelines module,
  on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). The designer of the Data Pipelines module is a client-side application built from
  `DP/Assets/designer`: the checkout must be built with its client assets, and the designer page must show a toolbar
  (see the first check below).
- Provisioned by **Auto Setup** (environment variables, see `references/autosetup.md` of the `orchardcore-tester`
  skill): recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email
  `admin@example.com`, password `<password>` (chosen outside git and passed in `VIDEO_PASSWORD`; never written in the
  repository). The setup screen is not part of this video.
- Outbound calls blocked with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video needs no external
  service and no local stand-in.
- The Blog recipe already enables Contents, Queries and Users (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`),
  whose data sources the dialog lists.

### Features to enable before recording (outside the capture)

1. Sign in as `admin`, open **Tools** > **Features**, type `Data Pipelines` in the search box, and select **Enable**
   on the row **Data Pipelines** (`OrchardCore.DataPipelines`). The **Data Pipelines Core** feature
   (`OrchardCore.DataPipelines.Core`) is enabled with it, because it is a dependency
   (`DP/Manifest.cs`, `EnabledByDependencyOnly`).
2. Check that **Tools** > **Data Pipelines** > **Pipelines** opens an empty list with an **Add Pipeline** button
   (`DP/AdminMenu.cs`, `DP/Views/Admin/Index.cshtml:20`).

### Seed data (create it outside the capture)

All names are invented; all emails end in `@example.com`.

1. Open **Access Control** > **Users** (`OrchardCore.Users/AdminMenu.cs:27-30`), select **Add User**
   (`OrchardCore.Users/Views/UsersAdminListCreate.cshtml:7`), and create two users, one after the other:

   | User name | Email |
   |---|---|
   | `jamie.chen` | `jamie.chen@example.com` |
   | `priya.shah` | `priya.shah@example.com` |

   Fill **User name**, **Email**, **Password** and **Password confirmation**, and select **Save**
   (`OrchardCore.Users/Views/UserName.Edit.cshtml:4`, `UserEmail.Edit.cshtml:4`, `UserFields.Edit.cshtml:12,21`,
   `UserSaveButtons.Edit.cshtml:2`). Use any password the site accepts, chosen when you create the users; never write
   it in the repository. Leave the roles empty. The list then holds `admin`, `jamie.chen` and `priya.shah`.
2. Open **Tools** > **Data Pipelines** > **Pipelines**, select **Add Pipeline**, type `User directory` in **Name**,
   and select **Create** (`DP/Views/Admin/Create.cshtml:13,20`). The designer opens on an empty canvas. Add no step.
   Select **Data Pipelines** in the toolbar to go back to the list (`DP/Assets/designer/src/App.vue:670`): the list
   holds one pipeline, **User directory**, with the badge **Draft**.
3. Stop the site, and copy its `App_Data` folder aside. Copy `state.json` aside as well after the sign-in below.

### The recorder

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/data-sources-developer/recorder
export VIDEO_ROOT=<repo>/videos/data-sources-developer VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup          # signs in once and saves state.json (git-ignored)
<bin>/Recorder record 02-consume
```

The video has no `PrepareAsync`: the seed above is done by hand. Narrate first (`narrate.py`), since the recorder holds
each step for the length of its narration.

## Order, dependencies, and what changes the site

- One clip to record: `02-consume`. It depends on the seed above and on nothing else.
- `02-consume` **changes the site**: it adds a **Read a data source** step to the draft of **User directory**, which
  the designer saves by itself. Before a new take, stop the site, restore the copy of `App_Data` made after the seed
  (and its `state.json`), and start the site again.
- Every other chapter is a card or slides: nothing to record.

## Clip `02-consume`

Starts on: the designer of the pipeline **User directory**, with an empty canvas. Before `StartCaptureAsync`:

1. `await scene.OpenAsync("/Admin/DataPipelines");`
2. Read the id of the pipeline: `var id = await scene.Page.Locator("li[data-pipeline-id]").First.GetAttributeAsync("data-pipeline-id");`
   (`DP/Views/Admin/Index.cshtml:38`).
3. `await scene.OpenAsync($"/Admin/DataPipelines/{id}/Edit");` (route in `DP/Controllers/AdminController.cs:126`).
4. Wait for `[data-cy='toolbar-add-data-source']` to be visible, and for the panel **Start with a source**
   (`[data-cy='start-picker']`, `DP/Assets/designer/src/canvas/StartPicker.vue:20`).
5. If the admin menu is open, collapse it so that the designer has the whole width. Then `await scene.StartCaptureAsync();`

On screen at the start: the designer toolbar, with **Data Pipelines** (back link), the name **User directory**, and the
button **Add a data source**; an empty canvas with the panel **Start with a source**.

Record the steps in this order. Keep each step's actions shorter than its narration.

- `consume-1`: select **Add a data source**: `ClickAsync([data-cy='toolbar-add-data-source'])`
  (label: `DP/Assets/designer/src/App.vue:707`). Wait for the dialog `[data-cy='data-set-picker']` and for the cards:
  `[data-cy='data-set-picker'] .card` visible, with no **Loading…** text left. At the end of the narration the dialog
  titled **Add a data source** (`DP/Assets/designer/src/dataSets/DataSetPicker.vue:70`) is open: a **Filter** box and,
  under it, a list of data sources starting with **All**, each with a count badge, on the left; the cards of the data
  sets on the right (the content types of the Blog recipe, **Users**, **User roles**, and the others the site has).
  Do not scroll.
- `consume-2`: first `ClickAsync([data-cy='data-set-category-Users'])`, the **Users** entry of the list on the left
  (`DataSetPicker.vue:91`; the entry is named after the data source,
  `OrchardCore.DataPipelines/DataSources/Users/UsersDataSource.cs:50`). Wait until exactly two cards are listed. Then
  `HighlightAsync([data-cy='data-set-Users::Users'])` on the **Users** card, with no caption, to point at its three parts
  (the label, the description, the data source badge at the bottom); hold about two seconds, then
  `ClearHighlightsAsync()`. Do this on the **Users** card: the cards of content types have a label, a group and a data
  source badge but no description (`OrchardCore.DataPipelines/DataSources/Contents/ContentItemsDataSource.cs:260-268`).
  At the end of the narration the **Users** entry is the highlighted one with the badge **2**, and exactly two cards
  are listed: **Users** with the description **One row per user.**, and **User roles** with the description
  **One row per role of each user.**, each with the badge **Users** (`UsersDataSource.cs:170,175`).
- `consume-3`: select **Add** on the card **Users**:
  `ClickAsync([data-cy='data-set-Users::Users'] [data-cy='data-set-choose'])` (`DataSetPicker.vue:107,122-123`).
  The dialog closes, a step is added to the canvas, and the step panel opens by itself on **Settings**
  (`App.vue:296-306`). Wait for `[data-cy='step-panel']` and for the text **Users · One row per user.** in it. If the
  panel does not open, select the sliders button of the step (`[data-cy='node-edit']`). At the end of the narration
  the canvas shows one step named **Read a data source** (`OrchardCore.DataPipelines.Core/Steps/Sources/DataSourceStep.cs:43`)
  whose summary reads **Users › Users** and **Every field** (`DP/Views/DataSource.Fields.Design.cshtml:9,12`), and
  the step panel is open on **Settings** with **Data set** showing **Users** and, under it, **Users · One row per
  user.** and a **Change** button (`DP/Views/DataSource.Fields.Edit.cshtml:25-30`).
- `consume-4`: select the tab **Fields**: `ClickAsync([data-cy='step-tab-fields'])` (`DP/Assets/designer/src/panel/StepPanel.vue:97`).
  Wait for `[data-cy='fields-outputs']` to show **User id**. At the end of the narration the tab lists, under
  **Produced**, one line per field with its label, its reference in brackets and its type, in this order:
  **User id** `[UserId]` Text, **User name** `[UserName]` Text, **Email** `[Email]` Text, **Email confirmed**
  `[EmailConfirmed]` Boolean, and the others down to **Failed sign-ins** `[AccessFailedCount]` Integer and **Roles**
  `[Roles]` Text (`UsersDataSource.cs:184-195`; tab layout `DP/Assets/designer/src/panel/FieldsTab.vue:66-94`;
  type labels `DP/Assets/designer/src/preview/previewTable.ts:23-30`). **Received** reads **This step has no input.**
- `consume-5`: select the tab **Data**: `ClickAsync([data-cy='step-tab-data'])`, then select **Preview**:
  `ClickAsync([data-cy='preview-run'])` (`DP/Assets/designer/src/panel/PreviewTab.vue:53-56`). Wait for a table in
  `[data-cy='preview-tab']` (`[data-cy='preview-tab'] table`) with its rows. At the end of the narration the table
  shows a header whose first columns read **User id** over **Text**, **User name** over **Text** and **Email** over
  **Text** (`PreviewTab.vue:96-97`), and three rows for `admin`, `jamie.chen` and `priya.shah`, with the emails
  `admin@example.com`, `jamie.chen@example.com` and `priya.shah@example.com`. A heading above the table reads
  **Rows** and **3 rows** (`PreviewTab.vue:80-87`; the output port is named **Rows**, `OrchardCore.DataPipelines.Abstractions/Steps/DataPipelineStepType.cs:34`). If the table is wider than the panel, leave it scrolled to the left, so that the
  first columns are visible.

After the last step the recorder holds the frame for the rest of the narration. Do not select **Publish** or
**Run**: the video never runs the pipeline.

## Slides and cards

These steps are drawn from `slides.py` and the storyboard; nothing is recorded.

- `intro`: the opening card, "Orchard Core Data Sources", with the subtitle "Expose your module's data to pipelines".
- `concept-1`: four cards, "Data source" bright; the others dimmed.
- `concept-2`: the same four cards, "Data set and schema" bright.
- `concept-3`: the same four cards, "Batches" and "Read for a user" bright.
- `build-1`: `Manifest.cs` with the `Dependencies` lines bright, and two cards: the feature id and "Enabled by dependency".
- `build-2`: `SupportDataSource.cs`: the `Ticket` record and the data source class.
- `build-3`: `TicketsDataSet.cs` with the class line, the descriptor and the `AddField` lines bright.
- `build-4`: the same file with the `IsIdentifier`, `IsKeyFilterable` and `References` lines bright.
- `build-5`: the `CanReadAsync` override, and two cards on who reads the data set.
- `build-6`: `LoadAsync` with the paging lines bright.
- `build-7`: the same method with the two `DataConditions` lines and their comment bright.
- `register-1`: `Startup.cs` with `AddDataSource<SupportDataSource>()`, and two cards: "Scoped service" and "Unique name".
- `register-2`: two cards: "Listed by every consumer" and "Reported when unavailable".
- `direct-1`: `WarehouseDataSource.cs` with the class line and the signatures of `GetDataSetsAsync` and `GetSchemaAsync` bright.
- `direct-2`: the same file with the whole `GetSchemaAsync` method bright.
- `direct-3`: `ReadAsync` of the same class, yielding a `DataBatch`.
- `more-1`: a `LoadAsync` that reads a queue and calls `OnSucceeded`, and two cards: `ConsumerId` and `OnSucceeded`.
- `more-2`: four cards: `IDataAggregateSource`, `IDataFileFormat`, "Formula functions" and "Documentation".
- `outro`: the closing card, with the subtitle "docs.orchardcore.net".

## Checks before assembling

Look at the contact sheet of `02-consume` (`review.py`): the frame at the end of each step must show what its narration
describes, with no error message, no empty list and no **The data sets could not be loaded.** text.

Private data: the only people on screen are `admin`, `jamie.chen` and `priya.shah`, and the only addresses are the three
`@example.com` addresses above. The **User id** column holds ids the site generated, which are not names. No password is
typed on camera, and the address bar is not part of the capture. OCR the clip at one frame per second and at every scene
change, and search the text for any other name, email address, phone number or token.
