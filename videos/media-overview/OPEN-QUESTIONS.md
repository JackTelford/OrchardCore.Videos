# Open questions: Media overview

Things the narration does not state as fact, or that a person must confirm on a running site before or during the
recording. The narration uses only what was read in the source of `OrchardCMS/OrchardCore` (commit 92db4a83) and its
documentation.

## To confirm on the running site

1. **The three images.** The Blog recipe imports `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` from the Blog theme
   (`src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img/`). They appear in the grid (`library-3`), in the picker
   (`content-3`) and in the header of the About page (`content-4`). What the source files show (looked at in the source
   copy of the theme, not on a running site): `home-bg.jpg` is a desk with a notebook, a phone, a laptop and glasses, with
   no person; `post-bg.jpg` is an astronaut in a spacesuit next to a solar panel, with the face hidden behind the visor;
   `about-bg.jpg` is a map with a camera, a backpack and a notebook, with no person, but it shows trademarks (a Nikon
   camera strap and lens, a Fjallraven backpack patch, Ordnance Survey map covers). A person decides whether the astronaut
   and the trademarks are acceptable on camera, and if not, replaces the image before recording (and records the
   replacement in the plan). The images are small in the grid and the picker; the About header shows only a strip of
   `about-bg.jpg`, 600 px high and centered (the profile crops, and the page passes no anchor), so the strip is what to look at there.
2. **The folder tree on a fresh site.** The source creates `_Users` and a folder named by the user's id when the Library
   loads (`GetDirectoryTreeEndpoint.cs:47-51`). Confirm that the tree shows them, that the id is not a real name, and that
   `library-1` ("the folders ... starting with the root folder, named Files") still reads true with that folder below the
   root. The root's label **Files** comes from `MediaJSLocalizer.cs:31`; confirm it on screen.
3. **The picker.** Confirm that the dialog opened by **Add media** shows its title (the source says **Select Media**,
   `MediaJSLocalizer.cs:137`) and the text of its confirm and cancel buttons (`OK` at `MediaJSLocalizer.cs:126`,
   `Cancel` at `:127`). The gallery inside the dialog calls `setTranslations` with its own group of strings when it
   starts (`media-gallery/src/App.vue`), and the dialog reads the shared store with `getTranslations()`. The
   implementation of both functions is in `@bloom/helpers/localizations` (the `.scripts/bloom` folder of the Orchard
   Core repository), which is not part of the source copy this video was written from, so it is not known whether the
   second call adds to the first set or replaces it. If it replaces it, a label of the dialog could render empty. The
   narration therefore names only **Add media** and says "confirm the choice"; confirm the title and the button text
   on screen. Confirm also that ticking the checkbox of a row is how a file is selected in the picker (and not a click
   on the row), and that the footer then reads "1 selected" (`MediaPickerModal.vue:39`, an English string that is not
   translated).
4. **The grid button.** The toolbar button that switches between the table and the grid has no text, only an icon; its
   tooltip is a fallback string ("Grid view" or "Table view", `App.vue:112`), and a native tooltip is probably not part of
   the capture. The narration therefore calls it "a button with a grid icon" in the toolbar above the table, and the
   recording rings it. Confirm that the icon on screen is a grid, that the four toolbar controls are, in order, Upload
   files, delete, download and grid, and that the icon selectors in the plan (`grip`, `list`) match.
5. **The Features search.** The search matches the name, the id and the description of a feature
   (`FeatureEntryDisplayDriver.cs:20`), so **Data Pipelines** also matches "media". Confirm which rows the search shows
   (the default filters hide on-demand and always-enabled features, and **Media Indexing** is enabled only by dependency),
   in which order the rows of each category are listed, and whether the two obsolete image cache features are listed. The
   narration claims none of this: it says only that the search narrows the list "to the features that mention it". Confirm
   that the rows named in `where-5` and `where-6` can be brought into view and ringed as the plan says, and that
   **Media Slugify**, **Secure Media**, **Azure Media Storage** and **Amazon Media Storage** show the status **Disabled**.
6. **The admin menu.** The Media menu and the Settings menu are groups with no page of their own, so they open when clicked
   (`NavigationItemLink-admin.cshtml:24-33`). The menu behavior of a site is **Focused** unless an administrator changes it
   (`AdminSettings.cs:29`, `AdminMenuBehavior.cs`; the Blog recipe does not set it): only the group that holds the current
   page is open when a page loads, and opening a group closes the others of the same level (`sidebar.ts:168`).
   Confirm that on screen: that **Content** is the group open on the Content Items page, that clicking **Media** closes it,
   and that clicking **Settings** after the Profiles page closes **Media**. Confirm the labels and that **Options** and **API**
   are visible under Settings, Media without scrolling the page (or that scrolling the menu is smooth enough to film).
7. **The About page.** The narration says the About page uses the same file as the one picked in `content-3`
   (`about-bg.jpg`) in its Banner Image field; the recipe stores that file in the field (`blog.recipe.json:1083`). Confirm
   on the edit form of the About article (opened outside the capture, and left without saving) that Banner Image holds
   `about-bg.jpg`. Confirm that `/about`
   shows the header image of `about-bg.jpg` and that the image URL carries the
   banner profile (the theme template asks for it, `Content-Article.liquid:9`). The narration says only that the header
   shows the image "cropped to a wide banner", and that the profile "crops an image to a wide banner". Confirm on the
   profile's edit page that **Resize Mode** reads **Crop**, **Width** 2048 and **Height** 600 (recipe values at
   `blog.recipe.json:1113-1120`; the labels at `Views/MediaProfiles/Edit.cshtml`). The narration does not claim the size of
   the result: `about-bg.jpg` is 1900 px wide and 1501 px high, and the engine shrinks but never enlarges an image
   (`Processing/VipsImageProcessingEngine.cs:111-123`, `Size.Down`), so the header image is cropped in height but may be
   narrower than 2048 px. Confirm on screen that the header looks cropped (a wide, short strip) and not stretched.
8. **Pronunciation.** "Slugify" (in the feature name Media Slugify), "Azure Blob Storage" and "Amazon S3" were not tested
   with the voice. Check them with `scripts/pronounce.py` and add a spelling to the `spoken` map of the storyboard only if
   the voice misreads them. A person confirms any added spelling.
9. **The Options page.** The video does not open Settings, Media, Options, because the page carries the notice "Media is
   configured with appsettings.json." (`Views/Admin/Options.cshtml:16`). The narration says what the page shows: the
   allowed file extensions and the maximum file size are on it (`Options.cshtml:37` and `:69`). Confirm on screen that those two values
   are listed.
10. **Leaving the form.** `content-4` navigates away from the unsaved Article form. No script of the content editor, of
    the admin theme or of the Contents module registers a `beforeunload` handler (searched in `src/OrchardCore.Modules`,
    `src/OrchardCore.Themes` and `src/OrchardCore`, vendor libraries aside), so no unsaved-changes prompt is expected.
    Confirm on the first take that the navigation completes without a browser prompt; if one appears, the recorder must
    accept it before the clip continues, and the plan must say so.
11. **The picker's gallery.** The picker mounts the library inside the dialog (`MediaPickerModal.vue`); the plan expects its
    first view to be the root folder as a table, from the cleared local storage. Confirm that the dialog opens on the
    root folder and shows the three files, and that selecting `about-bg.jpg` leaves **Add media** disabled afterwards.

## Choices made while writing

- The media field demo uses the empty Article form and never saves it, because the **Add media** button is disabled while a
  field for a single file already holds a file (`MediaFieldBasic.vue:187-189`); the **About** article already has one.
- The narration does not name the features Media TUS Uploads and Media SignalR (the protocol name and the library name are
  hard to say); it describes what they do and the recording rings their rows. It does name Azure Media Storage and Amazon
  Media Storage, so a viewer can find them in the list. The descriptions come from the feature manifests
  (`OrchardCore.Media/Manifest.cs`).
- The narration does not mention the feature Media Cache. The two storage features depend on it (`Manifest.cs` of the
  Azure and Amazon modules), and a separate video covers it.
