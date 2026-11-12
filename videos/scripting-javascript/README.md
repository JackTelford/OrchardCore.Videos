# Scripting: JavaScript in Workflows and Recipes

**YouTube:** not published yet · **Length:** 4:48 · **Feature:** `OrchardCore.Scripting` ([Scripting](https://docs.orchardcore.net/en/latest/reference/modules/Scripting/))

A video for module and theme authors. It shows how the Scripting module evaluates a script by its prefix, where scripts
run (recipes, workflows, layer rules and external logins), which methods a script can call, and how to add a method of
your own. It then shows how to evaluate a script from C# and how to limit what a script can do. The title,
description, tags and chapters YouTube shows are in `video.json`.

Most of the video is slides with code taken from the Orchard Core source. One clip is recorded in the browser: the Script
Task and the JavaScript syntax of a workflow, in the sample workflow that ships with Orchard Core.

## Recording it again

The video is recorded on a **fresh site** with the sample data that ships in the Orchard Core source, so it needs no
mask script. `recording-plan.md` has every action of the clip, and `OPEN-QUESTIONS.md` the points to check on a running
site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Seed the site: sign in, open **Tools**, **Recipes**, and run **Workflow Variables Samples**. It enables **Workflows**
   and **HTTP Workflows Activities** and creates the workflow **Sample: order total**.
3. Sign the recorder in, and copy the site's `App_Data` folder and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/scripting-javascript/recorder
   export VIDEO_ROOT=<repo>/videos/scripting-javascript VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate and record: `scripts/narrate.py videos/scripting-javascript`, then `Recorder record 03-workflow`. Only
   `03-workflow` is a recording; the other chapters are slides drawn by `slides.py`. The designer saves a draft as it
   goes, so restore the copy of `App_Data` before every new take.
