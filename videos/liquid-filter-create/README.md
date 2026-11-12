# Write a Liquid Filter

**YouTube:** not published yet · **Length:** 3:54 · **Feature:** `OrchardCore.Liquid` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Liquid/))

A developer video for module authors. It shows how a template calls a filter and where a module registers one, then
writes two filters: a delegate registered with `AddFilter`, and a class that implements `ILiquidFilter` and takes a
service. It covers encoding, nil and the immutable template options, and ends with a template of the admin that calls
both filters on the About page. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It shows one browser
screen, the Templates editor, and a front-end page. Everything else is slides. The full steps, with the code of the
module, the actions of every step and what must be visible, are in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the
   Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`).
2. Add the throwaway module **Video Liquid Filters** (the code is in `recording-plan.md`) to the checkout, reference it
   from `OrchardCore.Cms.Web`, and enable it under **Tools**, **Features**. Remove it from the checkout afterwards.
3. Write `recorder/Scenes.cs` from `recording-plan.md` (one method, `05-template`), sign the recorder in, and copy the
   site's `App_Data` and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/liquid-filter-create/recorder
   export VIDEO_ROOT=<repo>/videos/liquid-filter-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate and record: `scripts/narrate.py videos/liquid-filter-create`, then `Recorder record 05-template`. It is the
   only recording, and it saves the template `Content__Slug__about`: restore the copy of `App_Data` before another take.
