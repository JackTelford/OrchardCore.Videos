# Open questions: content-types-overview

The narration was written from the Orchard Core source (commit 92db4a83) and the Blog recipe, with no running site.
Every label in the narration was read in a view, a menu builder or a recipe; the file and line of each are in
`recording-plan.md`. What a person must confirm on the site while recording is listed here. None of these items is
stated in the narration as a fact that the source does not show, but each depends on how the page is drawn.

## Pronunciation (not tried: no audio was generated)

- `HtmlBody` is spoken as "HTML body" through the `spoken` entry of the storyboard. The label on screen is `HtmlBody`
  (the part's technical name without the "Part" suffix, with no space). Check the spoken form with
  `scripts/pronounce.py` before narrating, and change the entry if the voice does better another way.
- The voice has to say `BlogPost` (step `types-2`) and `MenuItem` (step `types-3`) the same way as the words "Blog Post"
  and "Menu Item", which are different on screen only in spacing. The sentence of `types-2` adds "written as one word"
  for that reason. Listen to both steps; if a sentence still sounds like a repetition, the wording can point at the
  screen instead.
- Also listen for "Autoroute", "Widget", "Banner Image" and "Create New Article".
- Listen for the settings words that end in "-able": "Creatable", "Listable" and "Draftable" (steps `types-4` and
  `anatomy-2`), and "Attachable" and "Reusable" (step `parts-3`). They are the labels of the check boxes, so they are
  spoken as written; confirm the stress with `scripts/pronounce.py`, and add `spoken` entries only if one is misread.

## Things to confirm on the running site

1. **Order of the two fields of Article.** Subtitle and Banner Image both have the position `0` in the recipe
   (`blog.recipe.json:686` and `:696`), so their order in the Fields list and in the editor follows the order stored in
   the definition. The narration names them in the recipe order and claims no order; either order is fine, but the
   plan's ring order follows what is on screen. (The lists are sorted with `OrderBy`, which keeps the stored order of equal
   positions, `Views/Admin/Edit.cshtml:52` and `:53`, so Subtitle is expected first; confirm it.)
2. **Rows of the Content Types list.** The list is ordered by display name. The plan expects Article, Blockquote, Blog,
   Blog Post, Category, Container, Content Menu Item, Html Menu Item and more, with the Widget badge on Container and the
   MenuItem badge on Content Menu Item. The three menu item types come from the Menu feature's own migration recipe
   (`OrchardCore.Menu/Migrations/menu.recipe.json:58`, `:96`, `:126`), not from the Blog recipe: confirm that they are
   in the list of a fresh Blog site, with the stereotype badge. If a row is below the fold, the plan says to scroll.
3. **Rows of the Content Parts list.** The page lists the part definitions stored in the site, except a part that has
   the name of a type (`AdminController.cs:508` and `:1162`). Title and Autoroute have stored definitions because their
   features' migrations create them (`OrchardCore.Title/Migrations.cs:31`, `OrchardCore.Autoroute/Migrations.cs:21`).
   Confirm that "Title", "Autoroute" and "Contact Details" appear, and that none of the type-named parts (Article,
   BlogPost, Blog, and so on) do. If "Title" or "Autoroute" is missing, step `parts-2` must name two parts that are listed.
4. **Add Parts list.** Step `anatomy-7` says the page lists the parts you can add to the type, each with a short
   description. Confirm that several parts are listed and that the descriptions show (the built-in ones have them:
   `OrchardCore.Title/Migrations.cs:33`).
5. **Add Field list.** Step `anatomy-8` names Boolean Field, Date Field, Media Field, Numeric Field and Text Field.
   Confirm that all five radio buttons are on the page (they come from the Content Fields and Media features, both
   enabled by the recipe).
6. **Admin menu.** The click path Design, Content Definition, Content Types (and Content Parts) is read from
   `AdminMenu.cs`; confirm how the menu opens its sections in the recorder's viewport, and whether Content Definition
   needs a second click. The Blog recipe also adds custom admin menu entries (Blog, Main Menu and Content,
   `blog.recipe.json:1188` and after); they sit beside Design and do not replace it.
7. **Row selectors.** The plan uses `[data-type-name='...']` on the rows, `a#btn-create-Article`,
   `a[role='btn-list-Article']`, `a[role='btn-edit-Article']` and `#btn-edit-ContactDetails`, read from the views and
   drivers. Confirm them with `Recorder probe` before the first take.
8. **Cancel on the New Article page.** Step `editor-3` clicks **Cancel**, which exists because the **Create New Article**
   button passes a return URL (`Content.Edit.cshtml:30`). Confirm that the button is drawn; if it is not, navigate straight
   to `/about`.
9. **About page.** Step `editor-3` says the title is the heading, the subtitle sits below it, the banner image fills the
   header and the body follows. That is what the theme's template does (`Content-Article.liquid:4` to `27`), and the recipe
   copies `about-bg.jpg` into the media library (`blog.recipe.json`, the `media` step). Confirm on screen that the banner
   photo is drawn behind the heading. The theme asks `fonts.googleapis.com` for fonts (`Layout.liquid:12`, `:13`). The
   recorder's browser makes those requests, not the site, so the site's proxy settings do not block them: confirm
   whether the fonts load on the recording machine, and that the page has settled before the narration ends.
10. **Body editor label.** The editor of the body uses the part's display name as its label (`HtmlBodyPart-Wysiwyg.Edit.cshtml:22`),
    so it reads "HtmlBody". Step `editor-2` says "the body comes from HtmlBody"; confirm the label on screen.
11. **Admin list layout.** The plan assumes the default List layout with the Buttons layout for the row actions
    (`AdminListOptions.cs:30`, `:36`). If the site shows a Menu of actions or a Grid, the steps `types-4` and `parts-3`
    must be recorded after setting the defaults back, and a layout selector may be drawn above the list; it is not narrated.
12. **Edit templates button.** The Edit Content Type page and the Edit Content Part page draw an **Edit templates**
    drop-down button above the names, because the Blog recipe enables the Templates feature and the admin may manage
    templates (`OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:59`,
    `TemplateContentPartDefinitionDriver.cs:55`, `Views/TemplateSettings.cshtml:5`). It is not narrated and nothing clicks
    it; confirm that it does not push the names out of view in the recorder's viewport.
13. **Tooltips.** The badges of the Content Types list carry the tooltips "Technical Name" and "Stereotype"
    (`ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:11`, `:15`), drawn by script on hover. Confirm that they show in the
    capture; the narration does not depend on them.
