# Open questions: Build a Company Website With the Agency Theme

Everything in the narration was read from the Orchard Core source at commit `92db4a83` and its documentation. The
points below could not be verified without a running site. A person confirms each one while recording, and tells the
author if the narration or the plan has to change.

1. **The admin menu groups.** The narration says "open Content, Content Items", "Tools, Features" and "Content, Menus".
   The labels are in the menu builders (`OrchardCore.Contents/AdminMenu.cs:65-68`, `OrchardCore.Features/AdminMenu.cs:26-27`,
   `OrchardCore.Menu/AdminMenu.cs:34`). How the redesigned admin menu opens a group (click, or hover) is decided by the
   admin theme's script (`TheAdmin/Views/NavigationItem-admin.cshtml` says only that nothing is expanded when the menu is
   rendered). Check that clicking the group name expands it, and that the clip shows both clicks.
2. **The button that saves a menu item (`menu-5`).** The editor of a Content Menu Item is rendered by the menu module's
   own pages (`OrchardCore.Menu/Views/Admin/Create.cshtml`), and the content module adds its buttons to every content
   editor (`OrchardCore.Contents/Drivers/ContentsDriver.cs:110-120`). By the code the editor shows one plain "Publish"
   button: the Add link has no return address (`MenuItem.Thumbnail.cshtml:16`, so `Content.PublishButton.cshtml:7`), the type is
   not draftable and the new item has no Id (so no Save Draft and no Delete). The narration ("Click Publish on the menu item") and the plan name that button: confirm on the first take that it is there, that it is the only button, and that it returns to the
   editor of Main Menu (`OrchardCore.Menu/Controllers/AdminController.cs:139`).
3. **Dragging a card in a Bag part (`edit-6`).** The cards have a handle and a sortable list
   (`OrchardCore.Flows/Assets/ts/bag-part.ts`, `flow-sortable-placeholder.ts`). That the recorder's `DragToAsync` moves the
   card in the headless browser is not known. If it does not, the sentence "We drag Brand Identity to the top of the
   Services part" has to be replaced by a recorded action that works, for example collapsing the cards and pointing at the
   handle without dragging, and the narration of `edit-6` changed with it.
4. **The permalink of the new page (`form-3`, `finish-1`).** The Page type generates the Permalink from the Title with the
   pattern `{{ Model.ContentItem | display_text | slugify }}` (`agency.recipe.json`, type `Page`, line 144), so the
   page should be at `/start-a-project`. Confirm it after publishing, because the plan opens that address.
5. **The content picker (`menu-3`).** The Selected Content Item field searches content items while typing
   (`OrchardCore.ContentFields/Controllers/ContentPickerAdminController.cs:38`). Confirm that typing `Start` lists the page
   `Start a project`, and that the option is clickable in the headless browser.
6. **The icon (`edit-5`).** The Icon Class hint names Font Awesome classes (`agency.recipe.json:587`); the theme loads Font
   Awesome (`TheAgencyTheme/Views/Layout.liquid:11`). Confirm that `fa-palette` draws an icon on the landing page. If it
   does not, use another class from the same family and change the plan (the narration says "the palette icon").
7. **What the form does when it is submitted.** The Action field is left empty (`form-4`), and the video never submits the form. The
   source shows how the form is written (`OrchardCore.Forms/Views/Form.Wrapper.cshtml`) but the behaviour of a submission
   without an Action was not checked, and the narration makes no claim about it.
8. **Sample content of the Agency recipe that stays on screen.** The recipe copies sample photos (team, about,
   portfolio) from the theme into the media library, and has project titles such as Threads and Southwest
   (`agency.recipe.json`, step `Content`). The plan renames the team members and removes the client logos, but the photos
   and the project titles remain. A person decides whether the photos show real people, and whether the project titles are
   acceptable as fake data; if not, replace them before the first clip.
9. **Pronunciation.** Check with `scripts/pronounce.py` how the voice says: `HTML Name`, `Permalink`, `Anti Forgery Token`,
   `POST`, `Font Awesome`, `HtmlBody`, `Html Menu Item`, `Alder Creek Studio` and `Brand Identity`. The storyboard's `spoken` map is empty until a person
   confirms a spelling.
10. **The order of the cards in the "Available Menu Items" window (`menu-2`).** The narration lists the Link Menu Item, the
    Content Menu Item and a third type. The order on screen follows the order of the content type definitions
    (`OrchardCore.Menu/Views/MenuPart.Edit.cshtml:65-73`), which was not checked. The sentence is true in any order; the highlights
    in the plan follow the order on screen.
11. **The label of the description field (`edit-5`).** The narration calls the field "HtmlBody". By the code it is the name of
    the part without "Part" (`HtmlBodyPart.Edit.cshtml:10` shows `TypePartDefinition.DisplayName()`; the Service type and the
    part define no display name: `agency.recipe.json`, type `Service`, and `OrchardCore.Html/Migrations.cs:32-34`). Confirm that
    the card shows "HtmlBody", and the hint "The body of the content item.". If it shows another label, change the narration of
    `edit-5` and the plan.
12. **The seed change of the Twitter hint.** The recipe's hint under the field Twitter names a real person
    (`agency.recipe.json:697`). The plan replaces it with "Twitter handle" before the first clip, on the page
    `/Admin/ContentParts/TeamMember/Fields/Twitter/Edit` (route from `OrchardCore.ContentTypes/Controllers/AdminController.cs:790`).
    Confirm that the page opens, that "Save" keeps the field, and that the Team cards show the new hint. If the list of
    cards still shows the old text, restart the site after saving.
13. **Fonts from the internet.** The layout of the theme links two Google Fonts style sheets
    (`TheAgencyTheme/Views/Layout.liquid:9-10`). The browser of the recorder loads them: the dead proxy of the plan only covers
    the site. A person decides whether to record online, or offline with the fallback fonts of the browser (the page then looks
    different from a published site, but shows no data).
14. **The Contact Us section of the landing page.** It is part of the template of the landing page, with a form that has no
    action (`landingpage.liquid:167-203`). The narration says only that the section and its form come from the template; it
    makes no claim about what the form does when it is submitted. Nobody submits it in the recording.
15. **The trash button of a card.** The narration calls it "a trash button". The code gives it no visible text and no
    `title`: "Delete" only titles the confirmation window (`ContentCard-BagPart.Edit.cshtml:37`, `flows.edit.ts`). Check
    on the running site that the button is drawn as a trash can.
