# Shapes: How Orchard Core Renders a Page

**YouTube:** not published yet · **Length:** 6:30 · **Feature:** `topic:DisplayManagement` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/DisplayManagement/))

A developer video about the shape, the object Orchard Core builds every page from. It shows a blog page made of shapes,
creates and renders a shape from Razor, Liquid and C#, and explains the shape contract, the rendering pipeline, the
bindings and the shape table, the names of template files, layouts and zones, and display drivers. It closes with the
`Write shape debug information` setting of the admin, which shows which template rendered a fragment. The title,
description, tags and chapters YouTube shows are in `video.json`; the exact steps to record it are in
`recording-plan.md`.

## Recording it again

The video is recorded on a **fresh site** with the sample content of the Blog recipe, so it needs no mask script. Only
two of its clips are recordings (`02-page` and `09-debug`); the others are slides drawn by `slides.py`, with code
from the documentation and the Orchard Core source.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the `Blog` recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. The password is
   chosen outside git and passed in `VIDEO_PASSWORD`. The complete list of values is in `recording-plan.md`.
2. Sign the recorder in. The video needs no seed data and no features beyond the recipe's, so there is no `prepare`
   step:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/display-shapes/recorder
   export VIDEO_ROOT=<repo>/videos/display-shapes VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: this is the clean snapshot. Start the site again.
3. Narrate with `scripts/narrate.py videos/display-shapes`, then record the clips in this order: `02-page`, then
   `09-debug`. `02-page` only reads the site. `09-debug` saves the Debugging settings, so restore the clean snapshot
   before recording it again, and before a full take.
