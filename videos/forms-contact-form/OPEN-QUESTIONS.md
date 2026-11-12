# Open questions: Build a Contact Form From Widgets

What was checked in the source, and what a person must confirm on a running site (Orchard Core at the commit of
`catalog/SOURCE.md`, Blog recipe, Forms enabled) before the first take. The narration names only labels that were read in
the code; these items are about how the screen looks and behaves, which the code does not settle.

1. **Display names in the Add Widget list.** The Forms migration creates the types Form, Input, TextArea, Select and
   Button without a display name; the content definition manager gives a new type `name.CamelFriendly()`
   (`OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:12`, `.../Utilities/StringExtensions.cs:30`),
   so the lists should read Form, Input, Text Area, Select and Button. The narration says "Text Area" in `idea-2`, `idea-3`
   and `rest-4`, and the slides do too. Confirm the entry reads "Text Area" (the documentation says "Textarea").
2. **Card headers.** `ContentCard-FlowPart.Edit.cshtml:38` writes the header as the content item's display text followed by the
   type's display name, and the Input driver sets the display text to the HTML Name (`FormInputElementPartDisplayDriver.cs:50`).
   The plan expects "Name Input" and "Email Input" in the collapsed cards of `04-rest`. The narration does not read them.
3. **Admin menu groups.** `NavigationItem-admin.cshtml` makes a group with no link of its own a button that toggles its
   children, and a group with a link gets a chevron button (`button.nav-toggle`). Find out, with `Recorder probe`, which
   one Tools and Content have, and use that to expand them before the capture of `02-create`. The Blog recipe also adds its
   own admin menu (the `AdminMenu` step of `blog.recipe.json`): a Content group with a Content Items link whose address ends in a
   slash, a node for the content types Article, Page and Taxonomy, and the entries Blog and Main Menu. `NavigationManager.cs:74-152` merges
   groups and items that have the same name, so confirm that the Content group shows one Content Items link, and which address it
   has. The plan selects it by its text, not by its address.
4. **Content Items list.** The plan filters the list with `?q=Contact%20Us` and clicks the first `a.edit`. Confirm that the
   filter finds the page by its title, and that Edit is in the same place in the List and Grid layouts.
5. **Features search.** Confirm that typing Forms in the search box leaves the Forms row visible with an Enable link whose
   address contains `OrchardCore.Forms/Enable` (the id `btn-enable-OrchardCore_Forms` is the likely one).
6. **After Save Draft and Publish.** The Edit link carries a return address (`ContentsButtonEdit_SummaryAdmin.cshtml:13`), so
   `OrchardCore.Contents/Controllers/AdminController.cs:858` (and `:797` for a new item) redirects to the list. Confirm it, and that the notifications read "Your Page draft has been
   saved." and "Your Page has been published.".
7. **Method before it is touched.** A new Form widget has no stored Method, and the select shows POST because it is the first
   option. Confirm the front-end form is written with the POST method after the page is saved without touching the select.
8. **Front end.** Confirm in the Blog theme that each label appears before its field, that Email address is an email input,
   that the Topic list shows the three options, and that no heading or style hides the form. The narration says only "the
   form shows its widgets in the order they were added" and "labels", "placeholders" and a "dropdown list".
9. **The permalink.** The Autoroute pattern of the Page type is a slug of the title
   (`blog.recipe.json`, the Page type's `AutoroutePartSettings`), so Contact Us should give `contact-us`. Confirm the
   Permalink field shows `contact-us` after publishing.
10. **Spoken forms.** The storyboard's `spoken` map has three entries to confirm with `scripts/pronounce.py` before narrating:
    `EncType` as "Encode Type", `contact-us` as "contact us", and `name@example.com` as "name at example dot com".
    "HTML" is left as the voice reads it; confirm it says the letters.
11. **Not shown, not claimed.** What happens when the form is submitted with an empty Action is not shown and not described:
    the video never submits. The Visibility Settings tab of Input, Select and Text Area, and the Validation option, are not
    covered by this video; their tabs are not named in the narration.
