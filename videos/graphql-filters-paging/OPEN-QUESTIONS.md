# Open questions: GraphQL: Filter, Sort and Page Content Items

The narration was written from the Orchard Core source at commit `92db4a83` (paths below are under `src/`), without
running a site. Each item says what was checked in the source and what a person must confirm on a running site
before recording. Nothing in the narration depends on an unconfirmed behavior except items 1 and 2.

## Needed before recording

1. **The seed.** The two recipes of `recording-plan.md` were written from `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`
   (the `BlogPost` type and the `content` step) and from `OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:1008-1074`
   (an import keeps `CreatedUtc`, `ModifiedUtc`, `PublishedUtc`, `Author`, and creates a draft when `Published` is false
   and `Latest` is true). Confirm with the check query of the plan: nine items, the order of the table, `Roadmap notes`
   unpublished, the paths as in the table. A different order means `CreatedUtc` was not kept: every result in the
   video would differ.
2. **Tools menu on screen (`where-1`).** The narration says "under Tools". The plan expects the expanded admin menu to
   show **Tools** open with **GraphiQL** selected on this page. Confirm it; if the section is closed, open it before the
   capture, and keep it open until the click that collapses the menu in `where-2`.

## Behaviors the video avoids, because the code and the documentation disagree or the code is unclear

3. **`MaxNumberOfResultsValidationMode` set to `Disabled`.** The documentation and `ConfigurationSchema.json` say
   that only the maximum number of results is returned. In `OrchardCore.Modules/OrchardCore.Apis.GraphQL/ValidationRules/MaxNumberOfResultsValidationRule.cs:68`
   the rule assigns a new `GraphQLArgument` to the parameter of its visitor, which does not change the parsed query, and
   `ContentItemsFieldType.PageQuery` (`:203-222`) takes `first` as it is. So `Disabled` seems to log the value and
   return as many items as asked. The narration says only "logs the value without rejecting the query". To confirm:
   run the site in Production with `...__MaxNumberOfResults=3` and `...__MaxNumberOfResultsValidationMode=Disabled`,
   run `blogPost(first: 5)`, and count the items (5 means no cap, 3 means a cap), and look for the information
   message in the log.
4. **`NOT` with several conditions.** The documentation says all the nested conditions must be false. The code adds the
   nested conditions to one conjunction and negates it (`ContentItemsFieldType.cs:330-334`), which is "not (a and b)".
   The narration and the slide only say that NOT "negates the condition it contains". To confirm, run
   `where: { NOT: [ { displayText_contains: "Liquid" }, { displayText_contains: "GraphQL" } ] }` on the seed: with
   "not (a and b)" all eight published posts are returned, with "all false" six.
5. **`orderBy` by `contentType`.** The input type accepts it (`Queries/Types/ContentItemOrderByInput.cs:14`), and the
   switch that builds the order has no case for it (`ContentItemsFieldType.cs:436-448`), so it has no effect. It is
   not shown. Sorting by several fields at once (`ThenBy`, lines 452-463) is not shown either, because the order of the
   keys inside the argument is not verified.
6. **Filters on content fields (`ops-3`, not recorded).** Read from `OrchardCore.Modules/OrchardCore.ContentFields/GraphQL/Startup.cs:27-34`
   (needs `OrchardCore.Apis.GraphQL` and `OrchardCore.ContentFields.Indexing.SQL`) and `GraphQL/Fields/ContentFieldsProvider.cs:17-96`
   (Boolean, Date, DateTime, Numeric, Text and Time fields have an index). Confirm on a type with a text field, after
   enabling **Content Fields Indexing (SQL)** and saving an item, that the Explorer shows the field with the suffixes of
   its type. The nesting of the field (under the part, or directly when the part has the name of the type) is not said
   in the video.
7. **Date values.** `createdUtc_gt` and the other date suffixes exist (`Apis.GraphQL.Abstractions/Queries/WhereInputObjectGraphType.cs:95-106`),
   but no recording uses them: how a date string is compared in each database was not verified, and the documentation
   examples use several formats.
8. **Case.** `_contains`, `_starts_with` and `_ends_with` become a SQL `like` (`Queries/Predicates/LikeExpression.cs`), so
   whether they ignore case depends on the database. The recorded queries only use words written the same way in the
   data.
9. **Variables with `first`.** `MaxNumberOfResultsValidationRule` reads `first` and `last` from variables in a special
    way (lines 43-49). The video passes `first` and `skip` as literals and does not talk about variables.

10. **A part filter next to other keys in one `where` object.** `BuildExpressionsInternal` (`ContentItemsFieldType.cs:268-316`)
    reassigns its `tableAlias` parameter when it meets the key of a collapsed part, such as `path_starts_with` (line 305),
    and keeps that value for the next keys of the same object, so a key that follows it, such as `displayText_contains`,
    seems to be resolved against the Autoroute index and not against the content item. The video keeps `path_starts_with`
    alone in `where` (Q3 and Q12), and `ops-2` only talks about conditions in general. To confirm, run
    `where: { path_starts_with: "guides/", displayText_contains: "Securing" }` and the same two keys in the other order.

## Recording details to confirm

11. **GraphiQL.** The play button (`button.execute-button`), `.result-window`, `.query-editor`, the title `Toggle Explorer`
    and the `query` parameter of the address were read from `OrchardCore.Apis.GraphQL/Assets/js/App.tsx` and from the
    bundle `wwwroot/Scripts/graphiql-orchard.js`, not from a running page. With the Explorer open, the result pane is
    about 40 lines tall: confirm that Q1 shows all eight titles (the narration says "all eight published posts"). If the
    last title is cut, report it: the fix is in the query, not in the narration.
12. **Pronunciation.** `narrate.py` could not be run to check these spellings in the `spoken` map of `storyboard.json`; a
    person must hear them (`scripts/pronounce.py`) before narrating: `GraphiQL` as "graphical", `orderBy` as "order by",
    `ASC` as "A.S.C.", `DESC` as "D.E.S.C.", and the three setting names as separate words. Also listen to `GraphQL`,
    `Autoroute`, `Liquid`, `SQL` and "1001".
13. **`first` of zero.** The narration says that a missing or zero `first` returns the default number of results; this is
    `ContentItemsFieldType.cs:205-210` (`first == 0`). There is no `last` argument on content type queries.
