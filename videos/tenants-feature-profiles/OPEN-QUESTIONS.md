# Open questions: Tenant Feature Profiles

What was read in the source (Orchard Core commit `92db4a83`) and what a person must confirm on a running site. The
narration says only what the source shows. These points are kept out of it, or are things the recorder meets.

Paths: `Tenants/` is `src/OrchardCore.Modules/OrchardCore.Tenants/`, `Modules/` is `src/OrchardCore.Modules/`, and `Core/`
is `src/OrchardCore/OrchardCore/`.

## To confirm while recording

1. **The Rules editor.** `Tenants/Views/FeatureProfiles/EditorTemplates/FeatureProfileViewModel.cshtml:22-37` renders
   a Monaco JSON editor, set up by the `@orchardcore/bloom` package, whose source is not in this checkout. Not
   verified: whether `window.monaco` exists (step `create-4`, method 1), whether typing the compact text keeps the
   brackets and quotes right (method 2: auto-closing, the suggestion list of the schema that offers `Exclude` and
   `Include`), whether `Shift+Alt+F` formats the document, and whether the editor starts empty on the Create page (the
   view model has no default value: `Tenants/ViewModels/FeatureProfileViewModel.cs`). Try the probe in `recording-plan.md`
   first, and change the plan if neither method gives the text shown there.
2. **The Feature profile field.** `Tenants/Views/Shared/_TenantFeatureProfile.cshtml:7` is a `select` bound to
   `string[] FeatureProfiles`, which the ASP.NET Core select tag helper renders as a list that allows several
   selections, not as a drop-down. The narration says "field" and "Select Basic sites" for this reason. Confirm how it
   looks, that `scene.SelectAsync` selects exactly one option, and that the saved tenant shows one badge.
3. **The admin menu.** The labels **Tools**, **Features**, **Multi-Tenancy**, **Tenants** and **Feature Profiles** come
   from the menu builders, but the markup of the admin theme's menu is not in the files read. Confirm that the clicks of
   `enable-1`, `enable-3` and `create-1` work, or use the fallback written in the plan.
4. **The tenant list layout.** The plan assumes the List layout and the Buttons actions layout (the defaults of
   `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). If the site is set to Grid or Menu, the rows and buttons
   of `assign-*` and `effect-*` look different: set the defaults back before recording.
5. **Auto Setup of the tenants.** The plan sets Bluebird and Maplewood up with Auto Setup on their first request. If a
   request ends in a setup screen instead, use Add Tenant and **Create & Setup** with the same names, prefixes and
   the Blog recipe, and keep the rest of the plan.
6. **The Edit Feature Profile page.** `FeatureProfilesController.cs:177` writes the rules back with
   `JConvert.SerializeObject(..., JOptions.Indented)`. Not verified: the letter case of the property names the editor
   then shows (the hint of the Create page and the documentation use `Rule` and `Expression`), and whether the editor
   shows the rules in the layout of `create-4`. The narration of `create-6` says only "the current rules".
7. **The effect on a tenant.** Saving a tenant reloads its shell (`Core/Shell/ShellHost.cs:154-159`,
   `Tenants/Controllers/AdminController.cs:578`), and the validation reads the profile on each request scope
   (`Core/Shell/FeatureProfilesValidationProvider.cs:31-62`), so the Features page of Bluebird should apply the rules
   at once. Confirm in `effect-2` and `effect-3` that no restart or reload of Bluebird is needed. If one is, add it to
   the seed of `05-assign` and tell the author, because the narration says the page follows the rules.
8. **Pronunciation.** The storyboard says `Orchard Core dot ` for `OrchardCore.` (so `OrchardCore.Workflows` is spoken
   "Orchard Core dot Workflows"), and the voice must read "IDs" as "I D s". Check both with `scripts/pronounce.py` before
   narrating.

## Read in the source, but not said in the video

- **Features that are already enabled.** The Features page lists only the features the profile allows
  (`Core/Shell/ShellFeaturesManager.cs:26`, `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:21-24`).
  `GetEnabledFeaturesAsync` does not apply the profile (`ShellFeaturesManager.cs:52`). So a feature a tenant
  has already enabled and a profile later excludes is probably still enabled, and probably no longer listed.
  Confirm on a running site before any video says what happens.
- **Changing or deleting a profile used by tenants.** A tenant stores the profile's key in its `FeatureProfile` setting.
  `FeatureProfilesValidationProvider` (`Core/Shell/FeatureProfilesValidationProvider.cs`) reads the profile on each
  request scope, and treats a name that no profile has as "no restriction". Confirm that an edit applies to a tenant
  without reloading it, and what a tenant shows after its profile is deleted.
- **Several profiles on one tenant.** The tenant field accepts several values and stores them comma separated
  (`Tenants/Controllers/AdminController.cs:422,564`), but the validation loop keeps only the last name of the list
  (`Core/Shell/FeatureProfilesValidationProvider.cs:50-60`): when that name matches no profile, the restriction is off,
  and the profiles named before it are ignored. The video shows one profile only.
- **The `?` wildcard.** The documentation names `*` and `?`. The rules use `FileSystemName.MatchesSimpleExpression`
  (`Core/Shell/ServiceCollectionExtensions.cs:75,85`), whose exact `?` behavior is in the .NET libraries, not in this
  source. The narration names only the asterisk.
- **Delete.** The **Delete** button of a profile (`Tenants/Views/FeatureProfileEntry.Buttons.SummaryAdmin.cshtml:8`) has no
  confirmation text of its own. The video does not click it. The success notice is **Feature profile deleted
  successfully.** (`FeatureProfilesController.cs:211`).
- **Auto Setup and recipes.** A profile can also be named in the `FeatureProfile` setting of an Auto Setup tenant
  (`Modules/OrchardCore.AutoSetup/Services/AutoSetupService.cs:75`) and created by the `FeatureProfiles` recipe step
  (`Tenants/Recipes/FeatureProfilesStep.cs`). Both use the profile's key, which for a profile created in the admin is an
  identifier the admin does not show. These belong to other videos.
