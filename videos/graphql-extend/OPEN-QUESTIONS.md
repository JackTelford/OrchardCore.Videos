# Open questions: Extend GraphQL

What was checked in the Orchard Core source (commit `92db4a83`, no running site), and what a person must confirm on a
running site before recording. Paths are relative to `src/` of `OrchardCMS/OrchardCore`.

## 1. The Autoroute part is collapsed: `path`, not `autoroute { path }`

The Autoroute module configures its part as collapsed
(`OrchardCore.Modules/OrchardCore.Autoroute/Startup.cs:92-98`, `partOptions.Collapse = true`). For a collapsed part,
`TypedContentTypeBuilder.cs:66-90` adds the fields of the object type (`path`) directly to the content type, and `:122-130`
adds the input fields (`path`, `path_contains`, ...) directly to the `where` argument. So the queries of the clips use
`path` and `where: { path_contains: "harvest" }`. A part that is not collapsed gets a field named `partName.ToFieldName()`
(`StringExtensions.cs:10`: no trailing `Part`, camel case), which is what `part-4` says. The documentation shows
`autoroutePart` and nested queries (`docs/reference/modules/Contents/README.md:321` and `:444-453`,
`docs/reference/modules/Alias/README.md:282-305`, `docs/reference/modules/Apis.GraphQL.Abstractions/README.md:127-131`).

To confirm: in GraphiQL, run `{ blogPost { displayText path } }` and
`{ blogPost(where: { path_contains: "harvest" }) { displayText path } }`; the Explorer should list `path` under `blogPost`
and no `autoroute` field. Read from the source only, not run.

## 2. GraphiQL: controls that belong to the library, and the layout

Orchard Core's own source labels only the **Explorer** button (`OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/js/App.tsx:118`)
and the menu entries Tools and GraphiQL. The rest of the page is the GraphiQL library (`graphiql` `^1.11.5`, from
`OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/package.json`), whose files are not in the source tree, so these are
not verified:

- the play button is `button.execute-button`, titled "Execute Query (Ctrl-Enter)"; the editor is `.queryWrap` and the
  results pane `.resultWrap`;
- the Explorer (`.graphiql-explorer-root`, verified in `graphiql-orchard.css:5`) shows `blogPost` expanded, with
  `path` checked, when the query comes from the `query` parameter of the address (`App.tsx:46`), and its root
  fields are listed once the introspection request has answered (the wait of every clip);
- with the admin menu expanded, GraphiQL is 260 pixels narrower (`Views/Admin/Index.cshtml:10`); at 1536x864 the
  Explorer, the editor and the results must all stay readable. If they do not, record the three clips with the menu
  collapsed (`graphiql-orchard.css:9` moves GraphiQL to `left: 48px`), and drop the green ring on the menu entry in
  `explorer-1` (the narration still reads correctly: "under Tools").

## 3. Results the narration describes

- `explorer-1`: the narration says the field comes from the object type the Autoroute module registers, because the
  clips run on the stock modules and no custom module is built.
- `explorer-2`: the list holds four posts, the three seeded ones, newest first, then the recipe's sample post. The
  order comes from `ContentItemsFieldType.cs:472` (`OrderByDescending(x => x.CreatedUtc)` when there is no `orderBy`).
- `where-2`: only `Harvest Notes` is returned. The narration says the filter runs in the database and the other posts
  are never loaded; this is read from `ContentItemsFieldType.cs:190-192` (the `where` clause is added to the YesSql
  query), not measured.
- `run-2`: the order of the two cultures is the order saved by the culture editor (the order in which they were added),
  and the default is the one whose radio button is selected, titled "Set as default". If the editor lists them in
  another order, adjust the end state in the plan, not the narration. A site that never saved its cultures has one
  supported culture, the installed UI culture of the machine
  (`OrchardCore.Modules/OrchardCore.Localization/Models/LocalizationSettings.cs:25-26`,
  `OrchardCore.Modules/OrchardCore.Localization/Services/LocalizationService.cs:17-20`), which is why the seed sets the
  list to `en-US` and `fr-FR` whatever the machine's culture is. The narration says the Explorer lists `siteCultures`
  "among" the content type queries; the order of the root fields in the Explorer belongs to the GraphiQL library, so
  nothing says they are next to each other.
- `schema-2`: the schema being built again when a builder's identifier changes is read from `OrchardCore.Modules/OrchardCore.Apis.GraphQL/Services/SchemaService.cs:27-119`.
  To try it: with GraphiQL open, add a field to a content type in Content Definition, reload GraphiQL, and the Explorer
  lists it without a restart.

## 4. Seed steps

The labels of the seed steps (outside the capture) come from the views cited in `recording-plan.md`. Not run:

- the search in **Features** hides rows with a CSS class and does not clear their checkboxes
  (`OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts`, `applyFilters`), so the ticks of earlier
  searches are kept; **Enable** submits the form without a confirmation;
- the row of the **Blog** item has one button for its one contained type (`blog.recipe.json:311-313`). Its label is
  built in `OrchardCore.Modules/OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10` from the technical name of the
  type, passed through the data localizer, which returns the name itself when there is no translation
  (`OrchardCore/OrchardCore.Localization.Core/Data/DataLocalizer.cs`): the label should read **List BlogPost Items**,
  not "List Blog Post Items". Its page (`ListPartNavigationAdmin.cshtml:52`) uses the display name: **Create Blog Post**.
  Confirm both labels; if the first differs, use the button of that row whatever it reads;
- the generated paths are `blog/harvest-notes`, `blog/pruning-in-winter` and `blog/orchard-tour-dates`
  (pattern at `blog.recipe.json:216`);
- the cultures editor is a bundled component (`culture-settings-editor`, in the `@orchardcore/bloom` package), whose
  source is not in the tree. The plan relies on what its compiled form in
  `OrchardCore.Modules/OrchardCore.Localization/wwwroot/Scripts/localization-settings/localization-settings.js` does: a
  drop-down list of the cultures not yet supported, the link **Add culture**, a trash link titled "Remove culture", and
  a radio button titled "Set as default" in each row. Confirm that the table can be set to `en-US`, `fr-FR` as the plan
  says, on the machine that records.

## 5. Pronunciation

The `spoken` map of `storyboard.json` was written without hearing the voice. Check each entry with
`scripts/pronounce.py` before narrating, in particular:

- `GraphiQL` spoken as "Graphical" (the project's own pronunciation), `YesSql` as "Yes Sequel";
- `siteCultures` (spoken "site cultures") and `blogPost` (spoken "blog post");
- the two library names of `schema-4`, `OrchardCore.Apis.GraphQL.Abstractions` and
  `OrchardCore.ContentManagement.GraphQL`, spoken as words without dots, which make `schema-4` longer than its
  26 counted words suggest;
- "C#" in `part-1`, expected as "C sharp".

## 6. Length

The narration is 921 words, 6:08 at 150 words per minute. The identifiers spoken as
several words (see 5) make the voice slower than the count.
