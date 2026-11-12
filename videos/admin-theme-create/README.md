# Create an Admin Theme: Restyle the Admin

**YouTube:** not published yet · **Length:** 5:13 · **Feature:** `guide:guides/create-admin-theme` ([Creating a custom admin theme](https://docs.orchardcore.net/en/latest/guides/create-admin-theme/))

The Orchard Core admin has a theme of its own, separate from the theme of the site. The video explains how the admin
gets its theme, then builds an admin theme on top of `TheAdmin`: a manifest with the admin tag and a base theme, an
`AdminBranding` template that loads a stylesheet, and a horizontal form layout written with the `ocat` classes. It
makes the theme current from **Design**, **Themes**, shows the result in the admin, and ends with the `Hint.Admin`
template and the `Admin` attribute for pages of your own. The title, description, tags and chapters YouTube shows are
in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only two chapters are
recordings (**Make it the admin theme** and **The restyled admin**); the other chapters are slides drawn by
`slides.py`. `recording-plan.md` holds every file of the sample theme, every action of every step, and the snapshots.

1. Build `src/OrchardCore.Cms.Web` of a checkout of OrchardCore with the sample theme `MyAdminTheme` added to it
   (the files are in `recording-plan.md`), and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both
   repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, e-mail `admin@example.com`).
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/admin-theme-create/recorder
   export VIDEO_ROOT=<repo>/videos/admin-theme-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is nothing to seed, so the video has no `prepare` step. Stop the site and copy its `App_Data` aside as
   snapshot A.
3. Narrate and record: `scripts/narrate.py videos/admin-theme-create`, then `Recorder record 05-enable`. The clip
   changes the site (it makes the theme current): copy `App_Data` aside again as snapshot B, then
   `Recorder record 06-result`. Restore snapshot A before another take of `05-enable`, and snapshot B before another
   take of `06-result`.
