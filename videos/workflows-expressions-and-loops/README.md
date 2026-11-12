# Workflows in Depth: Expressions, Variables, Branches and Loops

**YouTube:** not published yet · **Length:** 5:58 · **Feature:** `OrchardCore.Workflows` ([Workflows documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

A video for module developers on how a workflow computes, remembers, chooses and repeats. It covers the three expression
syntaxes (Literal, Liquid and JavaScript, which the Scripting module runs), typed variables, outcomes and transitions
with the If/Else and Script activities, and the For Each, For Loop and While Loop activities. It ends with the code that
adds a syntax, evaluates an expression in an activity, provides script functions, and limits scripts. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. One workflow is seeded, from the
JSON in `recording-plan.md`; the plan also lists, for every step, what to do and what must be on screen.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Enable the **Workflows** feature (Tools, Features), then import the workflow of `recording-plan.md` with Tools,
   Deployments, **JSON Import**. The seed is created by hand, outside the capture, so `Scenes.cs` needs no `PrepareAsync`.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/workflows-expressions-and-loops/recorder
   export VIDEO_ROOT=<repo>/videos/workflows-expressions-and-loops VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/workflows-expressions-and-loops`, then `Recorder record all`. Record the
   clips in this order: `01-designer`, `02-declare`, `03-canvas`, `04-run`. Only `04-run` changes the site (it runs the
   workflow once, which adds an instance), so restore the `App_Data` copy before taking it again. The slide clips need
   no site.
