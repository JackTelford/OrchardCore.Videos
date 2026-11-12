# Add Site Settings to Your Module

**YouTube:** not published yet · **Length:** 5:33 · **Feature:** `OrchardCore.Settings` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Settings/))

A developer video about the site settings of Orchard Core. It follows the Debugging group of the Settings module, from
its settings class and view model to its display driver and view, its registration, permission and menu entry, and
the code, Liquid templates and recipes that read it. One clip is recorded in the admin: the group is opened and saved.
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The code on the slides is
the code of the Settings module of Orchard Core, trimmed, so the site needs no module of its own and no feature to
enable. Every step, selector and source reference is in [recording-plan.md](recording-plan.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore (a branch that has the Debugging group) and run it with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned
   by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`,
   email `admin@example.com`).
2. Write the scene of the clip `05-try` in `recorder/Scenes.cs` from the recording plan, then sign the recorder in. There is
   nothing to seed, so there is no `prepare` step:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/site-settings-create/recorder
   export VIDEO_ROOT=<repo>/videos/site-settings-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` folder and `state.json` aside: restore them before every take.
3. Narrate and record: `scripts/narrate.py videos/site-settings-create`, then `Recorder record all`. Only the clip
   `05-try` is recorded; it changes the site (it ticks and saves the Debugging group), so restore the copy before another
   take. The other chapters are slides drawn by `slides.py`.
