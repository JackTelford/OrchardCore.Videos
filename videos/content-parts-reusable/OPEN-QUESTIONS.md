# Open questions: Content Parts: Create Reusable Groups of Fields

The narration states only what the Orchard Core source (commit 92db4a83) shows. These points depend on how the pages
look and behave on a running site, which nobody has confirmed yet. A person confirms each one while preparing the
recording (`Recorder probe` and a first take), and edits `recording-plan.md` or the narration if the answer differs.

1. **The admin menu at the start of `02-create`.** Checked in the source: `NavigationItem-admin.cshtml` renders every
   group collapsed and "the script restores what the user last left open", and `Navigation-admin.cshtml:24` sets a
   `data-menu-behavior` of `focused` or `persistent` from the admin settings. To confirm: that **Design** and **Content
   Definition** are collapsed when the clip starts (the session in `state.json` may remember an open group), that one
   click on each opens it, and that the selectors `#left-nav .item-label:has(> .title:text-is("Design"))` and the two after
   it match (the label sits in a child `span.title` of the button or link, `NavigationItemText.cshtml:40`, so a
   `:text-is()` on `.item-label` itself would match nothing). If the menu cannot be driven reliably, open `/Admin/ContentTypes/ListParts` with `NavigateAsync` instead and
   keep the narration, which names the menu path.
2. **The rows of the Content Parts list.** Checked in the source: the list holds every stored part definition except
   those named like a content type (`Controllers/AdminController.cs:508`, `:1133`), so it includes parts that modules
   store when they are enabled, not only parts made by hand. The narration says "the parts that you can configure" and
   names no row. To confirm: what the list shows on the Blog site, that no row holds private data, and that the
   sentence fits what is on screen.
3. **The Add Named Part list.** Checked in the source: it lists the parts that are both attachable and reusable
   (`Controllers/AdminController.cs:299-302`), sorted by display name, with the first one selected. To confirm: that on
   the Blog site it holds the Bag part next to Postal Address (Flows is enabled), that Postal Address shows its
   description, and that clicking its radio selects it. The narration says "we choose Postal Address" and names no other
   row.
4. **The order of the groups in the Customer editor.** The positions given to the Title part and the two named parts
   increase in the order they were added (`Services/ContentDefinitionService.cs:481-522`), so Title (position 1), Billing
   Address (2) and Shipping Address (3) should appear in that order, both in the Parts list of the type and in the
   editor. The narration says "Besides the Title, the editor has a group named
   Billing Address and another named Shipping Address" and does not depend on the order. To confirm: what the editor
   shows, and that both groups fit on screen (`content-2`).
5. **How long the messages stay on screen.** The messages are toasts. A success message is given 5000 milliseconds
   (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), and
   `OrchardCore.Themes/TheAdmin/Views/Message.cshtml` hides the toast after that delay (a notification without a delay
   does not hide itself, but all three messages here are success messages). Three steps
   mention a message (`create-3`, `settings-4`, `content-5`). To confirm: that each message is on screen while its
   sentence is spoken, and if not, move the click that causes it later in the previous step (or earlier in `content-5`).
6. **The page title.** `settings-4` says the page title shows the display name. The title is the breadcrumb title
   **Edit Content Part - Postal Address** (`Views/Admin/EditPart.cshtml:23`); `Breadcrumb.cshtml` of the admin theme
   renders it in a heading that can sit in a top bar. To confirm: that it is visible in the frame at the end of the step.
7. **Selectors that come from the markup, not from a run.** `a[role='btn-edit-Customer']` (the Edit button writes its
   id into a `role` attribute), `.form-check:has(label:text-is("Reusable")) input[type='checkbox']`, the field inputs of
   the editor named `<part name>.<field name>.Text`, and `.toast.message-success`. To confirm each with `Recorder probe`.
8. **The spelling for the voice.** `storyboard.json` maps `PostalAddress` to `Postal Address`, so the voice says
   "We type Postal Address, with the two words joined" in `create-2`, while the screen shows `PostalAddress`. To confirm with
   `pronounce.py` before narrating, and that the sentence still reads well. The words Attachable and Reusable are read as written.
9. **The seed.** The plan creates the Customer type by hand and attaches the Title part to it. To confirm that the
   Add Parts page labels the Title part **Title** (its display name is the part name without the suffix Part,
   `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`) and
   that the narration of `attach-1` ("a content type that already exists, with a Title part") matches the Parts list
   on screen.
10. **The Customer row in the Parts list.** `Views/Admin/Edit.cshtml:25-30` skips a part when
    `GetPartDefinitionAsync` returns null, and `ContentDefinitionManager.cs:384-399` returns null for a part that has no
    stored record. `ContentDefinitionService.AddTypeAsync` adds the type's own part to the type
    (`Services/ContentDefinitionService.cs:82`) but stores no part record, and `ContentDefinitionManager.Apply`
    (`:258-292`) does not create one. So the row **Customer** ("Contains the fields for the current type") is probably
    not in the Parts list while the Customer type has no fields. The narration does not mention it (`attach-2` says the
    section "lists what the type is made of"). To confirm: what the Parts list shows in `attach-2` and `attach-6`, and
    that `attach-1` ("a content type that already exists, with a Title part") matches it.
11. **The description of the Title row.** The Title row of the Parts list is expected to show the hint **Provides a
    Title for your content item.** (`ContentTypePartExtensions.cs:28-38`, `OrchardCore.Title/Migrations.cs:33`). To
    confirm on screen, since it is the part's own description and the type part has none.
12. **The Description of a part.** `settings-1` says the Description appears next to the part when you attach it to a
    type. Checked in the source: the Add Parts page (`Views/Admin/AddPartsTo.cshtml:34`) and the Add Named Part page
    (`Views/Admin/AddReusablePartTo.cshtml:55-58`) show it. To confirm: that the Postal Address row of `attach-3` shows
    **Street, city and postal code.**
