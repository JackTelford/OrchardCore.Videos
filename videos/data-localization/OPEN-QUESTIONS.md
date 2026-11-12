# Open questions: Dynamic Translations

Everything in the narration was read from the Orchard Core source (commit 92db4a83) and its documentation. The points
below could not be confirmed without a running site. A person confirms each one on the recording site before the
narration is generated; if a point differs, the step named in brackets changes first.

## Names on screen

1. **Culture display names** (`enable-1`, `editor-1`, `editor-2`, `switch-1`, `access-1`). The editor lists cultures as
   "display name (culture name)", for example "French (France) (fr-FR)", and the culture permission is "Manage French
   (France) translations". The display name comes from .NET (`CultureInfo.DisplayName` in
   `OrchardCore.DataLocalization/Controllers/AdminController.cs`), so it depends on the language of the machine that runs
   the site. Confirm the English names on the recording machine. The admin culture picker lists the native names
   ("English (United States)", "français (France)"; `OrchardCore.Localization/Views/AdminCulturePicker.cshtml`).
2. **The four categories** (`editor-2`, `stats-3`). The narration names Admin Menus, Content Fields, Content Types and
   Permissions. A fifth category, Search, appears only when the Search settings hold a placeholder and the Search module is
   enabled; the recording site has neither. Confirm that exactly these four appear.
3. **The group named BlogPost** (`translate-3`). Fields are grouped by the name of the content part
   (`DataLocalizationContext.ContentField(part name)`), and the Blog recipe's part is named BlogPost. Confirm the heading on
   screen, and that Tags is listed once.

## Behavior to see work

4. **No restart** (`translate-4`). `TranslationsManager.UpdateTranslationAsync` clears the cached dictionary of the
   culture when it saves, so a saved translation should show on the next request. Confirm in `05-switch` that the
   three translations appear right after `04-translate`, without restarting the site.
5. **Admin menu item** (`switch-2`). The admin theme translates the text of a menu item with the context of its menu
   (`TheAdmin/Views/NavigationItemText.cshtml`), and the Blog recipe's admin menu is named "Admin menus". Confirm that Main
   Menu reads Menu principal in the left menu, and that Blog and Content, the built-in Content menu, stay as they are.
6. **Content type name on several pages** (`switch-3`). The source translates the name in the New menu, the page titles,
   and the content type lists. Confirm it in the New menu of Manage Content, in the title of the create page, and in
   Content Definition, Content Types. The New menu lists only the types set as Creatable
   (`OrchardCore.Contents/Controllers/AdminController.cs:861-883`) and the Blog recipe leaves Blog Post without that
   setting, so the recording plan makes it creatable in its third preparation recipe. Confirm that the recipe leaves the
   four parts and the Draftable and Versionable settings of Blog Post unchanged (the step merges the settings:
   `OrchardCore.ContentTypes/RecipeSteps/ContentDefinitionStep.cs:54-58`), and that `Blog Post` is in the New list
   before `05-switch` is recorded. The narration of `switch-3` says the menu lists "the content types that are set as
   Creatable", the label of the checkbox (`ContentTypeSettings.Edit.cshtml:17`).
7. **Tags label** (`switch-4`). Confirm that the create page of a Blog Post shows the label Étiquettes for the Tags field.
8. **Statistics colors** (`stats-2`). The bars are green from 75 percent, yellow from 25 percent and red below
   (`Assets/ts/translation-statistics.ts`). Confirm that, with the seed of the recording plan, French is green and English
   (United States) and Spanish are red.
9. **Default roles** (`access-2`). `OrchardCore.DataLocalization/Permissions.cs` gives the Administrator role Manage all
   dynamic translations and the Editor role View dynamic translations and statistics as default stereotypes. The Blog
   recipe creates the Editor role with an empty permission list. Confirm on Access Control, Roles, Edit the Editor role,
   under Data Localization, that the permission is checked after Data Localization is enabled. If it is not, change the
   second sentence of `access-2`.
10. **Culture permission label** (`access-1`). Confirm on the same page that a permission labelled "Manage French (France)
    translations" is listed under Data Localization, one for each supported culture.
11. **Deployment step** (`move-1`). The step name "Translations" comes from its thumbnail
    (`Views/Items/TranslationsDeploymentStep_Fields_Thumbnail.cshtml`). Confirm it is offered by Add Step in a deployment
    plan (Tools, Deployments, Plans), and that the file a plan exports can be imported with Package Import on a site where
    Data Localization is enabled: `TranslationsDeploymentSource` writes a step named "Translations" with a `translations`
    array of `culture`, `context`, `key` and `value`, which is the shape `Recipes/TranslationsStep.cs:47-67` reads, and the
    import merges the translations into the existing ones. The narration says nothing about the other step, "Dynamic Data
    Translations", on purpose: `Deployment/AllDataTranslationsDeploymentSource.cs:38-42` writes a step named
    "DynamicDataTranslations" with a `DynamicDataTranslations` array of `Key` and `Translations`, and
    `TranslationsStep.HandleLegacyFormatAsync` (`:108-126`) reads a `Translations` array of `Name` and `Translation`, so
    the export seems not to be read back. Confirm by exporting a plan with that step and importing it on a second site;
    if it imports, a later video can name it. The documentation calls this step "All Data Translations" and recommends it
    for moving translations between environments.

## Recording details

12. **Accented characters.** `translate-3` and `translate-4` type É and é in the translation inputs. Confirm that the
    recorder's typing method enters them (the recording plan gives a fallback).
13. **Save and auto-save** (`translate-2`, `translate-4`). The message "Translations saved successfully." is shown for
    3 seconds. The Save button is disabled when nothing is unsaved (`Index.cshtml:169`) and auto-save clears the unsaved
    state 2 seconds after the last key, so the plan clicks Save within 1 second of the last key in `translate-2`, about
    2.5 seconds before the end of the narration, and does not click it in `translate-4`, where auto-save shows the message.
    Confirm on the contact sheet that the message is on screen in the last frame of both steps, and that the click of
    `translate-2` did not wait for the button.
14. **Pronunciation.** Check with the pronunciation script: "P.O. files" (the storyboard's spoken form of "PO files"),
    "français", "Étiquettes", "Article de blog" and "Menu principal".
15. **Seed script.** The console script of the recording plan calls the editor's own endpoints (`GetStrings` and `Save` of
    `AdminController`). Confirm that it prints a 200 status for French and Spanish, and that the Statistics page then
    shows French complete except three strings.

## Behavior seen in the code, and what the narration keeps to

16. A user who has only a culture permission (Manage French (France) translations) and neither View dynamic translations and
    statistics nor Manage all dynamic translations: the editor opens, since `Index` accepts any culture permission, but
    `GetStrings` (used when the culture is changed) and the statistics page require the view permission. The documentation
    says every culture permission implies the view permission; the code gives the culture permission no such implication
    (`DataLocalizationPermissions.cs`). The video does not describe this case. A person confirms the behavior before any
    video explains per-culture translators in detail.
17. **Items of admin menus** (`translate-4`, `why-1`). The providers list the menu names and the top-level link and
    placeholder items of each menu, the content types of a top-level content types node and the lists of a top-level
    lists node (`OrchardCore.AdminMenu/Services/LinkAdminNodeDataLocalizationProvider.cs`,
    `PlaceholderAdminNodeDataLocalizationProvider.cs`, `OrchardCore.Contents/Services/ContentTypesAdminNodeDataLocalizationProvider.cs`);
    they do not read the children of a placeholder, so `Content Items` (inside the placeholder `Content` of the Blog
    recipe's menu) is not listed. The narration says "top-level items". Confirm that the Admin Menus category of the
    editor lists `Admin menus` (the menu), `Blog`, `Main Menu` and `Content` under the group `Admin menus`, and no
    `Content Items`.
18. **Categories of the Translations step** (`move-1`). The step lists, as categories, the full contexts of the strings
    (`TranslationsDeploymentStepDriver.GetCategoriesAsync`), so a content field appears as `Content Fields:BlogPost` and
    not as `Content Fields`, and the export keeps only the translations whose context equals a chosen category
    (`TranslationsDeploymentSource.cs:42-44`). The narration only says "the cultures and categories you choose". Confirm
    on the step's edit page before any video describes the categories in detail.
