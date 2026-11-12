# Open questions: placements-admin

What the narration, the slides and the recording plan state was read from the Orchard Core source at commit
`92db4a83` (paths relative to `src/OrchardCore.Modules/OrchardCore.Placements`, written `P/`). Nothing below was run on
a site. The narration keeps to what the source shows; a person confirms the following on a running site while
recording, and tells the author if one differs.

## Screens (checked when recording `03-create` and `04-shortcuts`)

1. **The template of an empty rule.** `P/Controllers/AdminController.cs:138-144` serializes `new PlacementNode[] { new() }`
   with `JOptions.Indented`, which skips null properties. The code editor of `Create Placement` should show
   `[` / `  {}` / `]`. The narration of `create-3` says "one empty rule".
2. **The rule a shortcut generates.** `P/Controllers/AdminController.cs:160-173` adds a rule with only the
   differentiator (and a `displayType` or `contentType`/`contentPart` filter when the shortcut has them). For the
   editor entry of the `Subtitle` field, the editor should show one rule with `"differentiator": "BlogPost-Subtitle"`,
   the `Shape type` field disabled and reading `TextField_Edit`. The narration of `shortcut-3` says the differentiator is
   "already filled in".
3. **The error message.** An empty rule is refused with `A valid placement must contain either place, shape, wrappers
   or alternates. Please correct the placements at positions: 1.` (`AdminController.cs:230`). Confirm it appears as a
   notification above the form and that the form keeps the shape type and the rule.
4. **The result in the blog post editor.** In `03-create`, only the placement `MediaField_Edit` exists: the editor of
   the seeded blog post should lack the `Banner Image` field (shape `MediaField_Edit`, differentiator `BlogPost-Image`)
   and still show `Subtitle`, followed by `Tags`. The narration of `create-6` states the first. After `04-shortcuts`
   (outside the capture), the editor should also lack the `Subtitle` field (shape `TextField_Edit`, differentiator
   `BlogPost-Subtitle`). The editor differentiator is built as `{type part name}-{field name}` in
   `src/OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentFieldDisplayDriver.cs:34`. Check that
   only these fields disappear.
4a. **The list of Blog Post items.** `create-6` opens `/Admin/Contents/ContentItems/BlogPost` because Blog Post is not
   listable in the Blog recipe (`blog.recipe.json:188-196`), so the unfiltered Content Items list skips it
   (`C/Services/DefaultContentsAdminListFilterProvider.cs:182`), while a type named in the route is listed anyway
   (`:145`). Confirm that the page holds the item `Man must explore, and this is exploration at its greatest` with an
   `Edit` button.
5. **The features search.** `enable-2` says "Two features match" for the search `placements`. The search compares the
   name, the id and the description of every feature (`F/Drivers/FeatureEntryDisplayDriver.cs:20`, with `F` for
   `OrchardCore.Features`). The manifests in the source contain only these two features with that word; confirm the
   counter reads `Showing 2 of ...` on the recording site.
6. **Where the shortcut Save returns.** `shortcut-4` and `shortcut-5` rely on the return URL of the shortcut link
   (`P/Views/PlacementSettings.cshtml`, `P/Controllers/AdminController.cs:263`): Save should return to the page of the
   `Subtitle` field, not to the list. If it returns to the list, the step `shortcut-5` can drop its navigation.

## Behavior of the module (not shown on screen)

7. **Precedence over a theme or a module.** `why-3` says that a location in a rule of the module replaces the location
   from a theme or a module, and that alternates and wrappers are added. This is read from
   `PlacementInfoExtensions.Combine` (`src/OrchardCore/OrchardCore.DisplayManagement/Descriptors/PlacementInfoExtensions.cs:14`,
   "second overrides first"), from `BaseDisplayManager.FindPlacementImpl` (which combines the providers in the order
   they are registered) and from `P/Startup.cs:31`, where the module's provider is registered after the core one.
   The registration order was not observed at runtime. To confirm: add a rule in the admin for a shape that a
   `placement.json` file of a module or a theme already moves (for example a shape of the Contents module), and check
   that the admin rule wins, and that an alternate in the admin rule is added to the alternates of the file.
   A related point to confirm with the same test: a `place` of `-` in an admin rule, for a shape that a file also
   places. `PlacementInfo.IsHidden` (`src/OrchardCore/OrchardCore.DisplayManagement/Descriptors/PlacementInfo.cs:274`)
   compares the private location field with `-`, and the constructor that `Combine` uses (`:162`) leaves that field
   unset, so a combined placement may not count as hidden. The video shows `-` only for shapes that no file places
   (`MediaField_Edit` with `BlogPost-Image`, `TextField_Edit` with `BlogPost-Subtitle`). If the combined case does not
   hide, `why-3` and `why-1` stay true for a location such as `Content:2`, and the combined `-` is a defect of the
   code, to report to the maintainer.
8. **Placements file storage.** `store-2` says the placements are saved in `PlacementsDocument.json`, in the folder of
   the tenant under `App_Data/Sites`. This is read from `FileDocumentStore`
   (`src/OrchardCore/OrchardCore.Data/Documents/FileDocumentStore.cs:22,89,117`: `ShellsApplicationDataPath`,
   `ShellsContainerName`, the tenant name, then the type name `PlacementsDocument` and `.json`; the defaults are
   `App_Data`, or the folder in `ORCHARD_APP_DATA`, and `Sites` in `src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:20-31`
   and `src/OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs`). The file
   content on the slide (an `Identifier` and the `Placements` object) comes from `PlacementsDocument` and `Document`
   (`Identifier`, set by the document manager). To confirm: enable Placements file storage on a scratch site, save a
   placement, and open `App_Data/Sites/Default/PlacementsDocument.json`.
9. **Several rules for one shape type.** `why-4` says that, within one shape type, every matching rule applies from the first to the last, that a later
   place replaces an earlier one, and that alternates and wrappers accumulate. This is read from
   `P/Services/PlacementProvider.cs` (the loop over `shapePlacements` at lines 68-133: `currentLocation` is overwritten by
   each matching rule that has a `place`, and the alternates and wrappers arrays are concatenated). To confirm: save two
   rules for one shape type that both match, one with `place` and an alternate, the other with another `place` and
   another alternate, and check where the shape appears and which alternates it has (for example by adding a template for the alternate).
   The narration says "In the Placements module" because a `placement.json` file behaves differently: each rule of a
   file wraps the previous one (`src/OrchardCore/OrchardCore.DisplayManagement/Descriptors/ShapeAlterationBuilder.cs:185-191`,
   `ShapePlacementParsingStrategy.cs:98-100`), so only the last rule that matches applies. Confirm this too if a
   video ever compares the two.
10. **Switching stores.** The source has no migration between the two stores, so placements saved in one store are not
   read from the other. The narration does not say this; confirm before anyone adds it.
11. **The deployment plan step.** `recipe-2` says a deployment plan can include the All Placements step when the
    Deployment module is enabled, and that it exports every placement as a Placements step. This is read from
    `P/Startup.cs:57-63`, `P/Deployment/PlacementsDeploymentSource.cs` and the views in `P/Views/Items` (the picker card
    reads `All Placements` and `Exports placements defined in Placements module.`; the step in the plan reads
    `Placements` and `Adds all placements to the plan.`). To confirm: in Tools, Deployments, Plans, add the step to a plan,
    run an export, and compare the JSON with the recipe on the slide `recipe-1`.
12. **The recipe step.** `recipe-1` is read from `P/Recipes/PlacementStep.cs`: the step name is `Placements`, its
    `Placements` property is an object of shape types, and each value replaces the rules of that shape type
    (`PlacementsManager.UpdateShapePlacementsAsync`). To confirm: run the recipe of the slide through Tools, Recipes or
    the JSON import, then open Design, Placements.
