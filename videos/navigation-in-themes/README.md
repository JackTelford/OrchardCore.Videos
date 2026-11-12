# Navigation in Themes: Render Menus, Breadcrumbs and Pagers

**YouTube:** not published yet · **Length:** 5:35 · **Feature:** `OrchardCore.Navigation` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Navigation/)) and `OrchardCore.Menu` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Menu/))

A video for theme and module authors. It explains that menus, breadcrumb trails and pagers are shapes drawn by templates,
renders a menu by its alias, declares a breadcrumb trail with the tag helper, and creates a pager in a controller. It
then shows the templates and alternates that a theme overrides to change their markup. The code is on slides; four short
browser clips show the screens that the code produces. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds the
exact actions, labels and source lines of every clip; this section is the outline.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin` (`admin@example.com`) with the password passed in
   `VIDEO_PASSWORD`. The Blog recipe brings **The Blog Theme** (Liquid templates) and the **Main Menu**.
2. Before recording, in the admin:
   - enable the **Admin Dashboard** feature (Tools, Features), which adds the Dashboard link to the breadcrumb trails;
   - tick **Allow users to change the page size on listing pages** in Settings, General, and save;
   - add twelve fake blog posts with Tools, Deployments, **JSON Import**, using the recipe that `recording-plan.md` prints.
3. Sign the recorder in and stop the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/navigation-in-themes/recorder
   export VIDEO_ROOT=<repo>/videos/navigation-in-themes VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Copy the `App_Data` folder and `state.json` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/navigation-in-themes`, then `Recorder record all`. The four clips are
   `02-menu-site`, `04-trail-site`, `05-pager-blog` and `05-pager-admin` (which opens the list of Blog Post items by its
   address, because the posts are not in the unfiltered Content Items list). None needs another clip first. **`04-trail-site`
   changes the site** (it clears **Show breadcrumb** in Settings, Admin), so record it last and restore the copy of
   `App_Data` before another take.
