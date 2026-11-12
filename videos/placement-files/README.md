# Placement: Decide Where Parts and Fields Appear

**YouTube:** not published yet · **Length:** 6:34 · **Feature:** `topic:Placement` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Placement/))

A video for module and theme authors. It shows how a display driver gives a shape its default location, and how a
`placement.json` file overrides it: the format of a rule, its filters and its placement information, how to target
parts, fields and editors with shape types and differentiators, and how a place names a zone, a position, tabs, cards
and columns. It ends with the Article editor changed by two rules, and with the order in which placement files win.
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Only two clips are
recordings (`05-before` and `05-after`); the other chapters are slides drawn by `slides.py`. Neither recording saves
anything, so the site needs no seed data and no feature has to be enabled.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Start it **without** a
   `placement.json` in `src/OrchardCore.Cms.Web`.
2. Sign the recorder in and record the first take:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/placement-files/recorder
   export VIDEO_ROOT=<repo>/videos/placement-files VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder record 05-before
   ```

3. Stop the site, create `src/OrchardCore.Cms.Web/placement.json` with the content given in `recording-plan.md`, start
   the site again with the same `ORCHARD_APP_DATA`, and record the second take: `<bin>/Recorder record 05-after`.
4. Narrate first (`scripts/narrate.py videos/placement-files`), since the recorder holds each step for the length of
   its narration. The clips do not depend on each other's data: they differ only by the placement file. The exact
   steps, selectors and what must be on screen are in `recording-plan.md`.
