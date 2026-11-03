# <Video title>

**YouTube:** not published yet · **Length:** m:ss · **Feature:** `<OrchardCore.Feature.Id>` ([OrchardCore#NNNN](https://github.com/OrchardCMS/OrchardCore/pull/NNNN))

What the video covers, in two or three sentences. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore (a branch with the feature) and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the
   Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`).
2. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/<slug>/recorder
   export VIDEO_ROOT=<repo>/videos/<slug> VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # only when Scenes.cs has a PrepareAsync: enables features, creates the fake data
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every full take.
3. Narrate and record: `scripts/narrate.py videos/<slug>`, then `Recorder record all`. List here the clips that depend
   on each other, and the order to record them in.
