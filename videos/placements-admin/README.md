# Placements Module: Edit Placement From the Admin

**YouTube:** not published yet · **Length:** 4:26 · **Feature:** `OrchardCore.Placements` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Placements/))

This video is for module and theme authors. It shows how the Placements module keeps placement rules in the admin
instead of in a `placement.json` file: how to enable it, how to create a placement in Design, Placements, how the
settings of a content type open a placement with its shape type and differentiator filled in, where the rules are
stored (the database, or a file with Placements file storage), and how the Placements recipe step and deployment plans
move them between sites. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Three chapters are browser
clips (`02-enable`, `03-create`, `04-shortcuts`); the others are slides and cards. `recording-plan.md` has every action
of every clip.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`).
2. Before recording, disable the **Placements** feature in Tools, Features (the Blog recipe enables it, and the first
   clip enables it on camera). Leave **Placements file storage** disabled.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/placements-admin/recorder
   export VIDEO_ROOT=<repo>/videos/placements-admin VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot S0): restore that copy before a full take.
4. Narrate and record: `scripts/narrate.py videos/placements-admin`, then record the clips in this order, restoring the
   snapshot of the plan before each one, because every clip changes the site: `02-enable` (enables Placements),
   `03-create` (saves the placement `MediaField_Edit`), then `04-shortcuts` (saves the placement `TextField_Edit`).
