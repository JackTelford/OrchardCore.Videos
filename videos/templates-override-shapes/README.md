# Templates: Override Shapes From the Admin With Liquid

**YouTube:** not published yet · **Length:** 5:48 · **Feature:** `OrchardCore.Templates` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Templates/))

The video shows how the Templates module replaces the way a shape renders with a Liquid template written in the admin:
what shapes and alternates are, why a template wins over a view file of the theme with the same name, how to create a
template that overrides how a blog post is listed, what the Liquid can use, how to find the name of a shape, the
shortcuts in Content Types, and how to manage templates and move them with recipes and deployment plans. The title,
description, tags and chapters YouTube shows are in `video.json`. `recording-plan.md` has every action of the four
recordings.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`. The Blog recipe
   enables the Templates feature, so no feature is enabled by hand.
2. Seed the site: create the two extra blog posts of the seed data in `recording-plan.md` in the admin, so the blog
   page lists three posts.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/templates-override-shapes/recorder
   export VIDEO_ROOT=<repo>/videos/templates-override-shapes VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside (snapshot S0): restore that copy before every full
   take.
4. Narrate and record: `scripts/narrate.py videos/templates-override-shapes`, then `Recorder record` for the clips in
   this order: `02-create`, `05-shortcuts`, `06-manage`, `04-debug`. `02-create` creates the template that
   `05-shortcuts` and `06-manage` need, and `06-manage` deletes it. `04-debug` is last, because it turns on the shape
   debug information, which wraps every shape of the pages recorded after it in comments; it starts from the snapshot
   taken after `02-create`. The table in `recording-plan.md` says which snapshot to restore before each clip. The
   other chapters are slides, drawn by `slides.py`.
