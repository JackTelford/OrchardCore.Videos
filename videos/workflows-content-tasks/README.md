# Workflows: Create, Update and Publish Content Automatically

**YouTube:** not published yet · **Length:** 5:21 · **Feature:** `OrchardCore.Workflows` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

A developer video about the content activities that the Contents module adds to the Workflows module. It builds one
workflow in the designer: when an article is published, Create Content Task makes an announcement as a draft, Update
Content Task fills in its body, and Publish Content Task publishes it. Slides with code from the source explain the
inputs of a content event, Content Properties (Liquid, then JSON), the merge of Update Content Task, how a task finds
its content item, the guards against infinite loops, and the workflow as a recipe step. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with invented data only, so it needs no mask script. The whole procedure,
step by step, is in [`recording-plan.md`](recording-plan.md); what could not be checked without a running site is in
[`OPEN-QUESTIONS.md`](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Seed the site, outside the capture:
   - enable the **Workflows** feature (Tools, Features), and no other workflow feature;
   - import the `Announcement` content type (the recipe is in the recording plan) at Tools, Deployments, JSON Import.

   Then sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/workflows-content-tasks/recorder
   export VIDEO_ROOT=<repo>/videos/workflows-content-tasks VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as the first snapshot.
3. Narrate and record: `scripts/narrate.py videos/workflows-content-tasks`, then `Recorder record` for the clips in
   this order, restoring a snapshot before a clip is taken again: `02-event`, `03-create`, `04-update`, `05-publish`,
   `06-run`. Each of the five clips builds on the workflow the previous one left, so they cannot be recorded in
   another order. The six slide clips need no recording.
