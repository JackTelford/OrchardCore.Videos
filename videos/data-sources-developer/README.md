# Data Sources: Expose Your Module's Data to Pipelines

**YouTube:** not published yet · **Length:** 5:46 · **Feature:** `OrchardCore.DataPipelines.Core` ([Data Sources](https://docs.orchardcore.net/en/latest/reference/modules/DataSources/))

A developer video for module authors. It explains what a data source, a data set, a schema and a batch are, shows how
the Data Pipelines designer lists the data sets of every registered data source, and builds a data source for a help
desk module: a record, a data set with its fields, who may read it, and how it loads. It registers the data source with
`AddDataSource`, then implements `IDataSource` directly for data that doesn't come as records. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

Only the chapter "What consumers see" is a browser recording. The other chapters are cards and slides that
`assemble.py` draws from `storyboard.json` and `slides.py`, and the code on the slides is not run. The video is
recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` has every step.

1. Build `src/OrchardCore.Cms.Web` of Orchard Core (a commit that has the Data Pipelines module, with its client
   assets) and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`).
2. Seed the site by hand, outside the capture: enable **Data Pipelines** in **Tools** > **Features**, create the users
   `jamie.chen` and `priya.shah` (`@example.com` addresses), and create an empty pipeline named **User directory**.
   Stop the site and copy its `App_Data` aside.
3. Sign the recorder in, narrate, and record the one clip:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/data-sources-developer/recorder
   export VIDEO_ROOT=<repo>/videos/data-sources-developer VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder record 02-consume
   ```

   The clip adds a step to the draft of **User directory**: restore the copy of `App_Data` before another take.

`OPEN-QUESTIONS.md` lists what a person confirms on the running site, and the spoken spellings of the identifiers.
