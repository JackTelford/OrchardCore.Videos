# JavaScript Localization: Translate Strings in Scripts

**YouTube:** not published yet · **Length:** 3:30 · **Feature:** `OrchardCore.Localization` ([JavaScript Localization](https://docs.orchardcore.net/en/latest/reference/modules/Localize/javascript-localization/))

A script that draws part of a page can't call the string localizer of C# code, but its labels and messages belong in the same PO files. The video, for module and theme authors, follows the CORS module of Orchard Core: a localizer that implements `IJSLocalizer` returns a dictionary for a group name, a Razor view writes it into the page with `Orchard.GetJSLocalizations`, and the script reads it with `setTranslations` and `getTranslations`. It then translates the strings with a PO file, shows the CORS settings page in French, and ends with requesting several groups and extracting a template for translators. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Only one clip, `05-admin`, is a recording; the other chapters are slides drawn by `slides.py`. `recording-plan.md` holds the exact steps, the French `fr.po` file, and the checks for private data.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Stop the site after its first start.
2. Create the tenant's `Localization/fr.po` file in that folder from `recording-plan.md`, then start the site again. The file must be there before the site serves a French page.
3. Write `recorder/Scenes.cs` for the clip `05-admin` from `recording-plan.md`, sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/localization-javascript/recorder
   export VIDEO_ROOT=<repo>/videos/localization-javascript VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   In a normal browser signed in as `admin`, enable the **Localization** and **CORS Configuration** features, add the `fr | French` culture in **Settings**, **Localization**, **Cultures**, and create the policies **Public site** (the default) and **Partner portal** on the **Cross-Origin Resource Sharing** page. Stop the site and copy its `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/localization-javascript`, then `Recorder record 05-admin`. The clip only reads the CORS page and opens a policy without saving, so it can be recorded again without restoring the copy. The voice speaks a few identifiers through the storyboard's `spoken` map; listen to them before assembling.
