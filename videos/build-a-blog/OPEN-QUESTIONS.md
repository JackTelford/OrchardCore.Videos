# Open questions: Build a Blog

What was checked in the source of `OrchardCMS/OrchardCore` (commit `92db4a83`, read only, no running site), and what a
person must confirm on a running site before or while recording. Nothing below is stated in the narration as a fact
beyond what the source shows; each item says which sentence depends on it.

## Confirmed in the source

Labels, behaviors and defaults used by the narration were read in these places (the recording plan gives `file:line`):
the Blog recipe (`TheBlogTheme/Recipes/blog.recipe.json`: types, parts, seeded content, admin menu entries, layers and
the footer widget), the views of The Blog Theme, the Lists module (`ListPartNavigationAdmin`, `ContainerService`), the
Taxonomies module (`AdminController`, `TagController`, `TaxonomyField` editors, `TaxonomyTermsFilter`), the Menu module
(`AdminController`, `MenuPart.Edit`, the menu item views), the Autoroute module (`AutoroutePart.Edit`,
`AutoroutePartDisplayDriver`), the Contents module (publish buttons, messages) and the Layers module (`Index.cshtml`).

## To confirm on a running site

1. **Admin menu entries** (`tour-4`, `tour-6`, `category-1`, `tags-2`, `menu-1`). The entries **Blog**, **Main Menu** and
   **Content** with **Taxonomy** come from the Admin Menu step of the Blog recipe, merged with the **Content** menu of the
   Contents module by `NavigationManager.Merge`. The merge and the captions were read in the code, not seen. Confirm that
   **Content, Taxonomy**, **Design, Widgets**, **Blog** and **Main Menu** are all visible, and with which locators.
2. **Icon picker** (`category-4`). The field **Icon** uses a jQuery icon picker. The recording plan guesses `.icon-picker-toggle`
   for the caret and `.iconpicker-item` for the icons. Confirm that the picker has an icon `fa-camera`, how it is opened, and
   that **Publish** with an icon set is accepted. If there is no camera, pick another icon and keep the title Photography.
3. **Body editor** (`post-2`). The Markdown body uses EasyMDE, a CodeMirror editor, with the **Wysiwyg** option set by the recipe.
   The narration only says that the body is written in Markdown. Confirm that typing into the CodeMirror element works with
   `TypeAsync`, and look at what the editor shows above the text (a toolbar is expected: the default toolbar is defined in the
   front-end package of the editor, which is not in the source tree). The label of the body field is derived from the part name
   (probably **MarkdownBody**); the narration does not name it.
4. **Tags editor** (`post-4`, `post-5`). It is a vue-multiselect (`taxonomy-field-tags.js`). The narration says "Type to search the
   tags that exist, and select one" and "when no tag matches, the field offers Press enter to create a tag". The strings are from
   `TaxonomiesJSLocalizer.cs`; where the text is shown (on the typed option, or under the box) and whether Enter selects the
   highlighted existing tag must be seen. Confirm that Exploration can be chosen by a click on the option.
5. **Address of an inline tag** (`tags-1`). `TagController` creates the tag term and publishes the Tags taxonomy, and the
   Autoroute handler generates its path. Confirm that `/tags/lisbon` opens, with the heading "Lisbon" and the post listed. The
   narration says only "Each tag links to a page of its own".
6. **Address of the post** (`post-1`, `post-8`, `05-tags` start). The narration says the permalink "is built from the blog and the
   title". Read in the source: the Contained part driver sets the container id while the editor is updated
   (`OrchardCore.Lists/Drivers/ContainedPartDisplayDriver.cs:77`-`:87`), before the item is created, and the Autoroute handler then
   renders the pattern of the Blog Post type with the `container` filter (`AutoroutePartHandler.cs:139`, `ContainerFilter.cs:22`).
   Confirm on the site that the post is at `/blog/chasing-golden-hour-in-lisbon` (the seeded post has the path `blog/post-1`).
7. **Order of the posts** (`post-7`, `post-8`). The list is ordered by creation date (`ContainerService.cs:353`). Confirm that the
   new post is above the seeded one in the admin list and on the home page.
8. **Media dialog** (`post-3`). **Add media**, **Select Media** and **OK** are strings of `MediaJSLocalizer.cs`; the files
   `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` are put in the media library by the recipe. Confirm that the dialog lists them
   in its first folder and that a click on a file selects it (the button **OK** stays disabled until then).
9. **Menu cache** (`menu-6`, `menu-7`). The layout caches the menu for five minutes with the tag `alias:main-menu`, and the Alias
   handler removes that tag when the menu is published. Confirm that Photography shows on the home page right after the
   menu is published. If it does not, the video cannot say "the menu shows Photography" without a wait.
10. **Menu editing** (`menu-6`). The narration says "Dragging an item changes its order, and the Add button of an item adds an
    item inside it, which the theme shows as a drop-down". Read in the source: the sortable script of the menu editor
    (`OrchardCore.Menu/wwwroot/Scripts/menu/menu.js`) drags a row by its title bar and writes the new order and depth into the
    hidden hierarchy field that Publish sends; `MenuItem.Admin.cshtml:40` shows **Add** on each item, and
    `Menu/Controllers/AdminController.cs:75`-`:86` nests the new item under it; the theme's `MenuItem.liquid` shows a drop-down for
    an item that has items. None of it is exercised in the clip. Confirm it on the running site before recording.
11. **Homepage note** (`tour-5`). The note "This content item is the current homepage" depends on the home route matching the id
    of the Blog (`AutoroutePartDisplayDriver.cs:62`-`:67`). Confirm that it shows in the editor of the Blog after Auto Setup.
12. **Widgets and Layers** (`tour-6`). The Footer zone card should show the widget "Footer" and the badge **Always**. The cards
    come from the zones and widgets in the recipe; confirm the page after Auto Setup.
13. **Pronunciation.** The storyboard's `spoken` entries (Html as H.T.M.L., Url as U.R.L., main-menu as main menu) were not
    tried with `scripts/pronounce.py`. Have a person listen to a sample of each before narrating. Also listen to "The Blog
    Theme" and "Lisbon".
14. **Length of the actions.** Several steps combine two page loads with typing (`category-1`, `post-3`, `tags-2`). The narration
    of each is 13 to 15 seconds or more, which should be enough; if a step runs short, shorten the typed values in the plan,
    not the narration.
15. **Address of the category** (`category-3`, `menu-4`, `menu-7`). The narration has the Permalink typed (`photography`), because
    the source never generates the path of a term added from the taxonomy editor (`Taxonomies/Controllers/AdminController.cs:118`-`:163`
    saves the term without running the content handlers). With the path typed, the term is routed as the path of the Categories
    taxonomy plus the term path (`AutoroutePartIndex.cs`, `PopulateContainedContentItemIndexesAsync`), which is `/categories/photography`.
    Confirm that this page opens after the taxonomy is published, with the heading made of the icon and "Photography" and the post
    listed. A person may also leave the Permalink empty once, off camera, to see which address the category gets: the narration does not depend on it.

## Documentation that does not describe the video's main feature

The catalog maps the feature `TheBlogTheme` to the Resources and Widgets pages. Neither describes the theme or the Blog recipe;
the page that does is the starter recipes and themes page (`src/docs/getting-started/starter-recipes.md`). The README links the
feature to that page, `video.json` lists all the pages of the tracker entry in `embeddedIn` and in its links.
