# Write a Recipe: Steps, Variables and Content

**YouTube:** not published yet · **Length:** 6:52 · **Feature:** `OrchardCore.Recipes` ([Recipes](https://docs.orchardcore.net/en/latest/reference/modules/Recipes/))

A developer video that writes one recipe for a module, step by step. It covers where a recipe file lives, its header,
its scripts and variables, the feature, themes and settings steps, a content type and a content item, and then runs
the recipe from Tools > Recipes. The title, description, tags and chapters YouTube shows are in `video.json`.

Chapters 01 to 05 and 07 are slides (`slides.py`). Only chapter 06 is a recording: the clip `06-run`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a recorder needs
is in [recording-plan.md](recording-plan.md): the site, the module that holds the recipe (with the full text of the
recipe), and every step of the clip. [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person must confirm on a running site.

1. Build `src/OrchardCore.Cms.Web` of a checkout of OrchardCore with the module `Bakery.Starter` added, as the plan
   describes, and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`).
2. Write the scene of `06-run` in `recorder/Scenes.cs` from the plan (one `StepAsync` per step), then sign the recorder
   in. The video needs no `PrepareAsync`: the module is the seed.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/recipes-write/recorder
   export VIDEO_ROOT=<repo>/videos/recipes-write VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside, with `state.json`: restore that copy before every take, because the
   clip saves settings, a content type and a content item.
3. Narrate and record: `scripts/narrate.py videos/recipes-write`, then `Recorder record 06-run`. The clip does not
   depend on another clip.
