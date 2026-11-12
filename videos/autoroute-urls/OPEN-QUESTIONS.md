# Open questions: Autoroute

The narration states only what was read in the source of Orchard Core (commit 92db4a83) and in the documentation. The
site was not run, so a person must confirm the points below on a running site before recording. For each one: what
was checked in the source, and what to confirm on screen.

## Behavior read from the source, not seen running

1. **A generated duplicate receives a dash and a number** (narration of `create-3`). Read:
   `Autoroute/Handlers/AutoroutePartHandler.cs:435-437` calls `GenerateUniqueAbsolutePathAsync` (`:455-488`), which
   starts at version 1. Confirm: publish a second Event titled "Spring Planting Workshop" on a spare copy of the site
   and check that its Permalink reads `spring-planting-workshop-1`. If the suffix differs, change `create-3`.
2. **A typed permalink used by another item is refused** (`change-5`). Read: `AutoroutePartDisplayDriver.cs:119-129`
   and `AutoroutePartHandler.cs:119-135`, message "Your permalink is already in use." Confirm on a spare copy.
3. **A permalink is generated only while the path is empty** (`change-2`), and "Refresh the path" empties it
   (`change-3`). Read: `AutoroutePartHandler.cs:397-403`, `AutoroutePartDisplayDriver.cs:98-107`. Confirm that after
   the title change and Publish the Permalink still reads `spring-planting-workshop`, and that after "Refresh the
   path" it reads `spring-planting-day`.
4. **Publish from a form opened with New returns to the Content Items list** (`create-3`, `change-2`, `change-3`).
   Read: the New menu passes a return address (`Contents/Views/ContentsAdminListCreate.cshtml:25`) and
   `Contents/Controllers/AdminController.cs:795-797` redirects to it. Confirm the confirmation reads "Your Event has
   been published." and that the Edit button of the row (`ContentsButtonEdit_SummaryAdmin.cshtml:13`) also returns to
   the list on Publish (the same return address logic: the edit form posts to its own address with the return address
   in the query string, `Contents/Views/Admin/Edit.cshtml:52`, so the page re-rendered after the permalink error of
   `change-5` still returns to the list when `change-6` publishes). If Publish stays on the editor instead, the steps
   that open the item again (`create-4`, `change-2`, `change-4`, `change-6`) need no click on Edit, and the waits for
   the list become waits for the Permalink value.
5. **"Add & Configure" attaches only the chosen part and opens its settings** (`add-4`). Read:
   `ContentTypes/Controllers/AdminController.cs:337-357`. Confirm the page "Edit Part - Autoroute" and the
   confirmation The "Autoroute" part has been added.
6. **The part is listed as "Autoroute", described "Provides a custom url for your content item."** (`add-4`). Read:
   `Autoroute/Migrations.cs:21-23` (no display name is set, so the name without its Part suffix is shown:
   `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-61`).
   Confirm the label and the description on the Add Parts page.
7. **The defaults of a new Autoroute part**: Pattern is the default pattern, every checkbox unchecked
   (`Autoroute/Models/AutoroutePartSettings.cs:5-47`, and `GetSettings` returns a new settings object when none is
   stored: `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentDefinition.cs:16-21`).
   Confirm what the Edit Part page shows right after Add & Configure, and that the pattern builds
   `spring-planting-workshop` from the title.

## Screens whose exact look or controls were not seen

8. **Menu clicks.** `add-2` navigates straight to `/Admin/ContentTypes/List` while the narration reads Design, Content
   Definition, Content Types, because the markup of the redesigned admin menu was not inspected. If the menu entries
   can be clicked one after the other on the running site, replace the navigation by those clicks (the labels are in
   `ContentTypes/AdminMenu.cs:23-25`); the timing of the step allows it.
9. **Row buttons of the Content Items and Content Types lists.** The locators `a[role='btn-edit-Event']` and
   `.list-group-item, tr` plus `a.edit` come from the Razor views
   (`ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`,
   `Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`), not from a running page. The lists can render as List
   or Grid (`Contents/Views/...`, admin list layouts); confirm the row locator for the layout that the site uses, and
   whether the new row is on the first page of the Content Items list.
10. **The front-end page of an Event** (`create-5`). With the Blog theme, the Event is drawn by
    `src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`, which prints the title in a masthead `h1` (`:9`) and
    renders only `Model.Content`. The Title part adds its own `h1` to the Detail display, location Header
    (`Title/Drivers/TitlePartDisplayDriver.cs:20-35`, `Title/Views/TitlePart.cshtml`), and the source does not show
    whether that one is drawn too. Confirm which headings the page shows (the clip rings the first `h1`), and that
    nothing private (the footer, the menu) appears.
11. **Where the Permalink message appears** (`change-5`). The message is bound to the Permalink field by the part
    driver (`AutoroutePartDisplayDriver.cs:116`), and the view prints it in the input group
    (`AutoroutePart.Edit.cshtml:42`). Confirm which element carries it (the `HighlightAsync` call of `change-5`
    uses a `.field-validation-error` element) and that the long list of characters fits on the screen.
12. **Field names.** The part's fields should be named `AutoroutePart.Path`, `AutoroutePart.UpdatePath` and so on, but
    the plan uses the labels (`GetByLabel`), which come from `asp-for` in `AutoroutePart.Edit.cshtml`. If a label does
    not match its input on the running page, use the input's name instead.
13. **A new tab for the published page** (`create-5`). The link in the Permalink field has `target="_blank"`
    (`AutoroutePart.Edit.cshtml:40`). Confirm that `SwitchToAsync` records the new tab with the caption.

## Wording

14. **Pronunciation.** Listen to "permalink", "Autoroute" and "URL" with the skill's pronunciation script before
    narrating. The storyboard's `spoken` entry makes the voice say "Add and Configure" for the label "Add & Configure"
    (the subtitles keep the written label); confirm that the voice reads the replacement as intended.
15. **Features page.** `add-1` says the feature is "listed under Tools, Features". Confirm that the search finds the
    Autoroute row alone, or with other features that mention it, and that the row is not hidden by the default
    visibility filters (`Features/Views/Admin/Features.cshtml:64-67`).
