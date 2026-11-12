# Templates: Preview a Template on Any Page Before You Save It

**YouTube:** not published yet · **Length:** 4:12 · **Feature:** [`OrchardCore.Templates`](https://docs.orchardcore.net/en/latest/reference/modules/Templates/)

The Templates module has a Preview button in the editor of a template. This video shows it on the `Content__BlogPost`
template of a Blog site: how to open the preview, how to follow a link to preview the template on another page, and
how to save the template when the result is right. Slides then show how the editor, the Render action of the Preview
controller and the shape binding resolver work together, and the permissions and pages the preview needs. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only the two recordings
(`02-open` and `03-any-page`) need a site; the slides are drawn by `slides.py`. `recording-plan.md` holds every action,
selector and check.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. The Blog recipe enables
   the Templates, Autoroute and Deployment features, so there is nothing to enable by hand.
2. Sign the recorder in, then create the one template of the video outside the capture. In
   **Tools**, **Deployments**, **JSON Import**, import the recipe of `recording-plan.md` (a `Templates` step with
   `Content__BlogPost`, which has a paragraph with the words "Posted by"):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/templates-preview/recorder
   export VIDEO_ROOT=<repo>/videos/templates-preview VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Check that `/Admin/Templates` lists `Content__BlogPost` and that `/blog/post-1` starts with the words "Posted by".
   Then stop the site and copy its `App_Data` and `state.json` aside: restore that copy before every full take.
3. Narrate with `scripts/narrate.py videos/templates-preview`, then record. The clips do not depend on each other, but
   `03-any-page` saves the template, so restore the copy before recording it again:

   ```bash
   <bin>/Recorder record 02-open
   <bin>/Recorder record 03-any-page
   ```

Both clips use two browser tabs (the editor and the preview), as `recording-plan.md` describes. Things that could not be
checked without a running site are in `OPEN-QUESTIONS.md`.
