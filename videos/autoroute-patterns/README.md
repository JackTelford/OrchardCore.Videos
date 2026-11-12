# Autoroute Patterns: Generate URLs With Liquid

**YouTube:** not published yet · **Length:** 4:56 · **Feature:** `OrchardCore.Autoroute` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Autoroute/))

The Autoroute part gives a content item a URL, and its Pattern setting, a Liquid expression, decides what that URL looks
like. The video is for module and theme authors: it explains when a pattern runs, what it can use (the content item,
the `display_text`, `slugify` and `container` filters, and field values), shows patterns generating paths in the admin,
sets a pattern from a data migration and from a recipe, and loads content by its path in Liquid and Razor. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds the
exact actions of every clip; this is the outline.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore (`main`) and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin` with `admin@example.com`. The password is chosen
   outside git and passed in `VIDEO_PASSWORD`.
2. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/autoroute-patterns/recorder
   export VIDEO_ROOT=<repo>/videos/autoroute-patterns VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # imports the Product content type (a Text field named Color) with the JSON Import page
   ```

   Stop the site and copy its `App_Data` aside as snapshot S0.
3. No Orchard Core feature has to be enabled: the Blog recipe enables the ones the video uses.
4. Narrate and record: `scripts/narrate.py videos/autoroute-patterns`, then `Recorder record 03-edit`,
   `Recorder record 04-result`, `Recorder record 05-container`, in that order. `04-result` needs the pattern saved by
   `03-edit`, so take snapshot S1 after a good take of `03-edit` and restore it before each further take of the other two.
   The chapters `01-idea`, `02-vocabulary`, `06-code` and `07-lookup` are slides and need no recording.
