# Open questions: Content Definitions in Code

Everything in the narration was read from the Orchard Core source at commit 92db4a83 and from the documentation. No
site was run and nothing was compiled, so these points need a person with a running site.

## On a running site (clip `07-seo`)

1. **The wait after Enable.** The code path is read: the Enable action updates the shell descriptor
   (`ShellDescriptorFeaturesManager.cs:112-114`), `ShellDescriptorManager.UpdateShellDescriptorAsync` calls the
   handlers (`ShellDescriptorManager.cs:133-136`), `ShellHost.ChangedAsync` releases the shell (`ShellHost.cs:174-175`),
   and `AutomaticDataMigrations.ActivatingAsync` (the only caller of `UpdateAllFeaturesAsync`) runs the migration when
   the next request builds the tenant. What is not known is how long that request takes. It happens inside the
   navigation that follows the click, so it is on camera, and `OffCameraAsync` after the navigation cannot remove it.
   Measure it: if it is long, a person must decide how to cut it (for example by wrapping the click and the navigation
   in one off-camera wait, if the recorder keeps capturing across the navigation, or by trimming the clip). Confirm
   that, after **Enable**, the Features page comes back with no error and Content Parts lists **SeoMeta**.
2. **The id of the button.** `btn-enable-OrchardCore_Seo` is built from `Html.GenerateIdFromName`, which replaces the
   dots by underscores by default (an ASP.NET Core setting that is not in this repository). Confirm the id on the page.
3. **The rows after the search.** From the source, typing `SEO` leaves two rows: **Media Slugify** (its description
   says "SEO-friendly", `OrchardCore.Media/Manifest.cs:58`) and **SEO**, because the search looks at the name, the id and
   the description. Confirm on the page, and that the click on the **Enable** button of **SEO** is not covered by the
   sticky bar at the top.
4. **The position of the rows.** The Content Parts list and the Content Types list are ordered by display name and hold
   many rows that other features create. The plan scrolls the row of **SeoMeta** and of **Social Meta Settings** into
   the upper third of the frame by its bounding box. Check on the contact sheet that the row, its description and the
   highlight are fully on screen at the end of `seo-2` and `seo-4`.
5. **The layout of the lists.** The default admin list layout is List with Buttons (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). Confirm
   that the description of **SeoMeta** is shown under its name in that layout, and that **Edit** and **Delete** are
   buttons, not a menu.
6. **The Edit page of the type.** `Views/Admin/Edit.cshtml` lists **Fields** before **Parts**, orders the fields by
   their `Position`, and marks the part named like the type with "Contains the fields for the current type"; the row
   of that part reads like the display name of the type (`ContentTypePartExtensions.cs:13-18`). Confirm both on **Edit
   Content Type - Social Meta Settings**, and that the part row and its hint are visible after the second scroll.
7. **Nullable field properties.** The narration says a field property of a part can be null, as the documentation of
   Content Types warns. The binding of content parts to classes (`ContentElementBinder`) was not read. On a running
   site, read `DefaultSocialImage` from a content item whose JSON has no such field and confirm it is null.

## Code on the slides

8. **Compilation.** The SEO excerpts (`Manifest.cs`, `Startup.cs`, `Migrations.cs`, `SeoMetaPart.cs`, the handler, the
   recipe) are copied from the source and trimmed; the excerpts of `Product`, `ProductReview` and `Price` follow the
   signatures of `ContentDefinitionManagerExtensions`, `ContentTypeDefinitionBuilder`, `ContentPartDefinitionBuilder`,
   `ContentTypeSettingsExtensions` and `ContentPartFieldSettingsExtensions`. None was compiled. A person should paste
   them in a module and build, in particular `type-1` to `type-3`, `part-2`, `version-3` and the handler of `cs-2`
   (an `async` lambda with no `await` in it only warns).
9. **The trimmed recipe.** The step on the `recipe-2` slide keeps the keys of the SEO recipe and replaces the settings
   it drops with `/* ... */`; it is an excerpt to read, not a file to import. To check the keys, import the full
   `socialmetasettings.recipe.json` (for example with a recipe migration of a test module) and check that the type and
   the part are created.
10. **A fresh tenant skips `UpdateFrom1Async`.** Read from the loop in `DataMigrationManager.UpdateAsync`
    (`DataMigrationManager.cs:219-243`): after `CreateAsync` returns 2, no method is registered for version 2, so
    nothing else runs. The log of the first start should hold no "Applying migration ... from version 1" line for the SEO
    module.

## Pronunciation

11. **Spellings added to the `spoken` map** of `storyboard.json`, to confirm with `scripts/pronounce.py` before narrating:
    `SeoMeta` as "S.E.O. Meta", `C#` as "C sharp", `UpdateFrom1Async` as "Update From 1 Async", and the identifiers
    spelled as separate words (`CreateAsync`, `AlterTypeDefinitionAsync`, `AddDataMigration`, `IRecipeMigrator`, ...).
    The spoken forms have more words than the written ones, so the audio is a little longer than the minutes counted
    in `README.md`.
