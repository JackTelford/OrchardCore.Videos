# Open questions: Extend GraphQL

What was checked in the Orchard Core source (commit `92db4a83`, no running site), and what a person must confirm on a
running site before recording. Paths are relative to `src/` of `OrchardCMS/OrchardCore`.

## 1. The field of a part is named `autoroute`, not `autoroutePart`

The narration (`part-4`, `explorer-1`, `explorer-2`, `where-1`), the slide of `part-4` and the three queries of
`recording-plan.md` use `autoroute`, from the code:

- `OrchardCore/OrchardCore.ContentManagement.GraphQL/Extensions/StringExtensions.cs:10`: `ToFieldName()` removes a
  trailing `Part` and writes the rest in camel case;
- `OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/Types/TypedContentTypeBuilder.cs:44`: the field of a part on
  a content type, and its field in the `where` argument (`:134-139`), are named with `partName.ToFieldName()`.

So `AutoroutePart` gives `autoroute`. A comment in `OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:294`
speaks of "ones with the part suffix removed by the dsl", which agrees. No file of the source tree outside `docs/` (code,
views, scripts, recipes) uses `autoroutePart` or `aliasPart` as a GraphQL field name; the only `autoroutePart` in the
GraphQL code is the alias of the index, `Autoroute/GraphQL/AutoroutePartIndexAliasProvider.cs:12`, which is not a field.
The documentation says `autoroutePart` instead
(`docs/reference/modules/Contents/README.md:321` and `:444-453`, `docs/reference/modules/Alias/README.md:282-305`,
`docs/reference/modules/Apis.GraphQL.Abstractions/README.md:127-131`); the release notes of an early version mention
a fix about the `Part` suffix of part names in `where` statements (`docs/releases/1.0.0-beta3.md:88`). This is reported as a
documentation issue.

To confirm: in GraphiQL, run `{ blogPost { autoroute { path } } }` and
`{ blogPost(where: { autoroute: { path_contains: "harvest" } }) { displayText } }`. If the site instead accepts
`autoroutePart`, the narration of the four steps above, the card of `part-4`, and the queries of `03-explorer` and
`05-where` change, and the video is recorded after that.

## 2. GraphiQL: controls that belong to the library, and the layout

Orchard Core's own source labels only the **Explorer** button (`OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/js/App.tsx:118`)
and the menu entries Tools and GraphiQL. The rest of the page is the GraphiQL library (`graphiql` `^1.11.5`, from
`OrchardCore.Modules/OrchardCore.Apis.GraphQL/Assets/package.json`), whose files are not in the source tree, so these are
not verified:

- the play button is `button.execute-button`, titled "Execute Query (Ctrl-Enter)"; the editor is `.queryWrap` and the
  results pane `.resultWrap`;
- the Explorer (`.graphiql-explorer-root`, verified in `graphiql-orchard.css:5`) shows `blogPost` expanded, with
  `autoroute` checked, when the query comes from the `query` parameter of the address (`App.tsx:46`), and its root
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
- `autoroute`, `siteCultures` (spoken "site cultures") and `blogPost` (spoken "blog post");
- the two library names of `schema-4`, `OrchardCore.Apis.GraphQL.Abstractions` and
  `OrchardCore.ContentManagement.GraphQL`, spoken as words without dots, which make `schema-4` longer than its
  26 counted words suggest;
- "C#" in `part-1`, expected as "C sharp".

## 6. Length

The narration is 909 words, 6:04 at 150 words per minute. The identifiers spoken as
several words (see 5) make the voice slower than the count.
