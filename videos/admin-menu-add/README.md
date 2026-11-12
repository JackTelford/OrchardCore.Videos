# Add Admin Menu Entries to Your Module

**YouTube:** not published yet · **Length:** 4:30 · **Feature:** `guide:guides/add-admin-menu` ([Adding a Menu Item to the Admin Navigation from a Module](https://docs.orchardcore.net/en/latest/guides/add-admin-menu/))

The video shows how a module puts its own pages in the admin menu of an Orchard Core site. It writes a controller with two views, a navigation provider that describes a top-level item with two children, and the registration in `Startup`, then enables the module and uses the menu. It also covers how positions order the items, the base class that Orchard Core's own modules use, adding to an existing menu, and limiting an item to users with a permission. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Only one clip is a recording (`05-admin`); the other chapters are slides drawn by `slides.py`. The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds everything a recording needs, including the text of the module's files.

1. Make a scratch copy of the `OrchardCMS/OrchardCore` checkout. Add the module `MyModule` (the files are in `recording-plan.md`) under `src/OrchardCore.Modules`, and reference it from `src/OrchardCore.Cms.Web`. Run `src/OrchardCore.Cms.Web` with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in. The module stays **disabled**: the clip enables it.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/admin-menu-add/recorder
   export VIDEO_ROOT=<repo>/videos/admin-menu-add VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` folder and `state.json` aside. Restore that copy before every take: the clip enables `MyModule`, so a second take on the same data has no Enable button.
3. Narrate and record: `scripts/narrate.py videos/admin-menu-add`, then `Recorder record 05-admin`. There is nothing to seed and no clip depends on another. The slides need no recording.
