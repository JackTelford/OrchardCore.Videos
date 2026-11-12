# Dynamic Cache: Cache Parts of a Page

**YouTube:** not published yet · **Length:** 5:48 · **Feature:** `OrchardCore.DynamicCache` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/DynamicCache/))

A developer video about the Dynamic Cache module. It explains why Orchard Core caches sections of markup instead of
whole pages, shows the Cache Mode setting and a Liquid cache block at work on a page, and then covers caching a shape
(attributes, the Razor `dynamic-cache` tag helper, a display driver), invalidating entries by tag, adding a cache
context, and the storage and failover settings. The title, description, tags and chapters YouTube shows are in
`video.json`.

Two chapters are browser recordings (the Cache Mode setting, and a cached block on the About page); the other chapters
are slides drawn by `slides.py`. `recording-plan.md` lists everything a person needs to record them, and
`OPEN-QUESTIONS.md` lists what a person must confirm on a running site.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ASPNETCORE_ENVIRONMENT=Development`,
   `ASPNETCORE_URLS=http://localhost:5010` and `dotnet run --no-launch-profile`, with `ORCHARD_APP_DATA` pointing at an
   empty folder outside both repositories, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time
   zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The default Cache Mode, "From environment",
   caches in Production only, which the video relies on.
2. Import the Liquid template of the seed data (**Tools**, **Deployments**, **JSON Import**; the JSON is in
   `recording-plan.md`), then sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/dynamic-cache/recorder
   export VIDEO_ROOT=<repo>/videos/dynamic-cache VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot A).
3. Narrate and record: `scripts/narrate.py videos/dynamic-cache`, then `Recorder record 02-mode`. Copy `App_Data` aside
   again (snapshot B), then `Recorder record 04-watch`. `04-watch` needs the Cache Mode that `02-mode` saves, and it
   publishes the About page again: restore snapshot A before another take of `02-mode`, and snapshot B before another take of
   `04-watch`.
