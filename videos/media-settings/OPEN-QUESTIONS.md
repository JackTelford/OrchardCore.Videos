# Open questions: Media settings

What the author could not verify from the Orchard Core source (commit `92db4a83`) or the documentation, and what a
person must confirm on a running site, or with the voice, before the video is recorded. Nothing below is stated as a
fact in the narration unless it says so.

## Pronunciation (confirm with `scripts/pronounce.py` before narrating)

- `storyboard.json` has one `spoken` entry, `Cdn` as `C.D.N.`, for the on-screen label **Cdn base url**. It is the
  spelling the skill describes for an initialism, and it has **not** been tried with the voice.
- Words of the narration the voice may misread, with no entry: `OAuth2`, `OpenID` and `SVG`. Try each as written; add
  an entry only if the voice gets it wrong. (The narration does not say SignalR or TUS: they appear only as labels on
  screen.)
- The narration never says `PKCE`, on purpose: it appears on screen in the label of the Bearer choice and the name of
  the recipe.

## On a running site

1. **Menu clicks.** `lists-1`, `api-1` and `recipes-1` open pages through the admin menu. The locators in
   `recording-plan.md` come from reading the markup of the admin theme, not from running it. Confirm that Settings and
   the Media group inside it open as buttons, that **Options** and **API** are links, and that the two entries named
   **Media** (the top-level group and the one under Settings) can be told apart.
2. **The refusal toast in `upload-1`.** The code shows a toast with the summary **Validation error** when the upload
   library refuses a file (`UppyFileUpload.ts:77-86`), and closes it after 5000 ms
   (`Assets/media-gallery/src/components/NotificationToast.vue:74-75`). The recording plan therefore picks the file
   about 8 seconds into the step. The detail line under the summary is a text of the upload library, not of Orchard
   Core, and is not narrated. Confirm that the toast appears for a `.txt` file, that the timing in the plan keeps it
   on screen when the voice names it, and whether `upload-2` needs the close button.
3. **The restricted upload in `upload-2`.** The narration says the user holds the permission. The signed-in user is in
   the Administrator role, which is granted every permission by the handler of the site owner
   (`src/OrchardCore.Modules/OrchardCore.Settings/Services/SuperUserHandler.cs:35-37`). Confirm on the running site
   that `company-logo.svg` uploads and is listed.
4. **The Editor role.** `role-1` shows the permission row and its **Security Critical** badge. The Blog recipe creates the
   `Editor` role with an empty list of permissions, so whether the box is ticked is not known; the narration does not
   say. Confirm the row and the badge are shown on the **Edit 'Editor' Role** page, and that the search for
   `additional permission` leaves exactly one row.
5. **Refusal of a file above Max file size.** `values-2` says a larger file is refused before the upload starts. This
   comes from the documentation (`MediaGallery.md`, "File Size Validation") and from the restriction the page gives the
   upload library (`Views/Admin/Index.cshtml:22`, `UppyFileUpload.ts:139`). It is not shown on camera. Confirm it with a
   file larger than the value on the Options page.
6. **Saving the unchanged Cookie scheme.** The code reloads the site only when the scheme differs
   (`MediaApiSettingsDisplayDriver.cs:63-68`). Confirm that `api-4` shows "Site settings updated successfully." and does
   not log the recorder out. The notification hides itself after 5 seconds
   (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`); confirm that the click on Save
   about 3 seconds into the step leaves it on screen while the voice names it.
7. **The Recipes page.** Confirm that typing `Media API` leaves the three Media recipes under one heading, in which
   order they are listed (the narration says "the Bearer recipe" and does not rely on the order), and that their
   descriptions are readable at the recording size.
8. **Bearer without OpenID Connect (`api-3`).** The narration says the Media Library cannot sign in and shows a
   message that names OpenID Connect and the Cookie choice. This is read from the code (`MediaJSLocalizer.cs:93`,
   `App.vue:415-433`, `services/media-gallery-auth.ts:156-172`), where the message appears when no token can be
   acquired. It is not shown on camera and was not run. Confirm it once on a throwaway site, never on the recording
   site.
9. **Values shown.** The narration names no number. Confirm on the Options page that the defaults appear as in
   `MediaOptionsConfiguration.cs`, that **Temporary file lifetime** shows as a time of one hour, and that **Storage
   provider** reads **Local**, as `values-4` says.

## For a maintainer

- **The Bearer recipe and an administrator.** The recipe `Media API — Bearer/PKCE` registers its client for
  `https://localhost:5001` (`Recipes/media-api-pkce.recipe.json`), and its description and the documentation say to
  replace that address before running it. The Recipes page has a **Run** button and no edit screen. The narration says
  only that the description tells you to replace the local address with your own, and does not say how an
  administrator does that. Should the video say how (copy the recipe, deploy a changed copy), or is that a developer
  topic?
- **The permission of the API page.** The menu entry and the editor need **Manage Media API authentication settings**
  (`AdminMenu.cs:43`, `Drivers/MediaApiSettingsDisplayDriver.cs:47,54`), but the page itself is authorized with the
  general **Manage settings** permission, because the Media module does not register a permission for its settings
  group (`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:95`; the Settings documentation describes the
  registration and Email does it). A role that holds only the Media permission would see the API entry in the menu and
  be refused the page. The `pages-2` slide names both permissions, as the code has them. Is that the intended
  behavior, or should the Media module register the group permission? The Media permission is also missing from the
  permission table of the Media documentation (see the documentation issues returned with this video).
