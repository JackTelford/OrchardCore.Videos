# Content Definition Stores: Keep Types in the Database or a File

**YouTube:** not published yet · **Length:** 3:52 · **Feature:** `OrchardCore.Contents.FileContentDefinition` ([Understanding Content Definition Stores](https://docs.orchardcore.net/en/latest/guides/content-definitions/))

A developer video about where a tenant's content definition is kept. It shows the content definition record and the
`IContentDefinitionStore` that reads and writes it, how the File Content Definition feature replaces the database store with a
`ContentDefinition.json` file for each tenant, and what a site sees when it starts with no file. It then follows the
documentation's procedure to move the definitions back to the database with a deployment plan, and ends with a feature profile that
keeps the feature out of other tenants. The title, description, tags and chapters YouTube shows are in `video.json`.

The code on the slides comes from the Orchard Core source; `recording-plan.md` names the file and lines of each slide
and of every label on screen. `OPEN-QUESTIONS.md` lists what a person must confirm on a running site.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Three clips are recorded, in
the order the story tells them; the nine slides are drawn from `slides.py` and need no recording.

1. Build `src/OrchardCore.Cms.Web` of Orchard Core and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-definition-stores/recorder
   export VIDEO_ROOT=<repo>/videos/content-definition-stores VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Seed the site as `recording-plan.md` describes: export the Blog definitions, enable the File Content Definition
   feature, import them into the file, and create the content type **Event**. Stop the site and copy its `App_Data` folder and
   `state.json` aside: restore that snapshot before every full take.
4. Narrate with `scripts/narrate.py videos/content-definition-stores`, then record `03-export`, `03-switch` and
   `03-import` in that order, without restoring the snapshot in between. `03-export` saves `ContentDefinitions.zip`, which
   `03-import` imports. To record one clip again, restore the snapshot first (`recording-plan.md` says what each clip needs).
