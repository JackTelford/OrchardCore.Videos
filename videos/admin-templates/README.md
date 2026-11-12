# Admin Templates: Customize Admin Screens With Liquid

**YouTube:** not published yet · **Length:** 4:42 · **Feature:** `OrchardCore.AdminTemplates` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Notifications/#admin-templates-ui))

The video is for developers. It explains how the Admin Templates feature replaces a shape of the admin with a Liquid
template written in the admin, then enables the feature, replaces the Visit Site link of the top bar, and edits and
deletes the template. Slides cover how a template is named after a shape, the `AdminTemplates` recipe step and the All
Admin Templates deployment step, what Save checks, the permissions, and a feature profile that keeps the feature away
from a tenant. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything the recorder
needs to decide is in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). The Blog recipe enables the Templates feature but
   not the Admin Templates feature.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/admin-templates/recorder
   export VIDEO_ROOT=<repo>/videos/admin-templates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is no seed data and no `prepare` command. Stop the site and copy its `App_Data` aside as snapshot S0.
3. Narrate and record: `scripts/narrate.py videos/admin-templates`, then record the three clips in this order, each
   one from the state the previous one left: `03-enable` (turns the feature on), `04-create` (creates the
   `VisitSiteNavbarItem` template), `05-manage` (edits it, then deletes it). Copy `App_Data` aside after each clip
   (S1 after `03-enable`, S2 after `04-create`) and restore the snapshot a clip needs before another take of it.
