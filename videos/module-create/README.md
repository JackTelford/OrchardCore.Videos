# Create a Module: Your First Orchard Core Module

**YouTube:** not published yet · **Length:** 5:44 · **Feature:** `guide:guides/create-install-custom-module` ([Create and install a custom module](https://docs.orchardcore.net/en/latest/guides/create-install-custom-module/))

A developer video. It creates a module from the Orchard Core module template, describes it in its manifest,
references it from the web project, and enables it from Tools, Features. It then opens the page the module adds, shows
the recipe step that enables the same feature, and lists what to check when a module does not appear. The idea, the
code and the commands are slides; only enabling the module and opening its page are recordings. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The step-by-step plan,
with every action, selector and expected screen, is in `recording-plan.md`; what a person must still confirm on a
running site is in `OPEN-QUESTIONS.md`.

1. Generate the module with the Orchard Core module template (`dotnet new ocmodulecms -n Greetings.OrchardCore`),
   replace its `Manifest.cs`, `Startup.cs`, `Controllers/HomeController.cs` and `Views/Home/Index.cshtml` with the
   contents given in `recording-plan.md`, move it to `src/OrchardCore.Modules` of an `OrchardCMS/OrchardCore` checkout with the project
   file the plan gives, and reference it from `src/OrchardCore.Cms.Web` of that checkout.
2. Run the host with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`, email `admin@example.com`). The feature `Greetings.OrchardCore` stays
   disabled.
3. Sign the recorder in, with the password chosen outside git:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/module-create/recorder
   export VIDEO_ROOT=<repo>/videos/module-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the host and copy its `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/module-create`, then `Recorder record all`. Record `05-enable`
   first: it enables the feature. `06-run` depends on it and changes nothing. To record `05-enable` again, restore
   the copy of `App_Data` first.
