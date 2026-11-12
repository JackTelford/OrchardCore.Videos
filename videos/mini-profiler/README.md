# Mini Profiler: Find What Makes a Page Slow

**YouTube:** not published yet · **Length:** 4:21 · **Feature:** [`OrchardCore.MiniProfiler`](https://docs.orchardcore.net/en/latest/reference/modules/MiniProfiler/)

A developer video about the Mini Profiler module. It shows what the module records (requests, MVC filters and views,
shapes and YesSql commands), how to enable it and choose who sees its widget, and how to read a profile: the timing tree,
the shapes and the queries view with its duplicate and error badges. It ends with the options, the security of the
results, custom steps, and what to check when the widget or a result is missing. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with the Orchard Core Blog recipe and no seed data, so it needs no mask
for names: only an init script that hides the machine name of the profiler's popup. Four chapters are recordings (`02-enable`, `03-roles`, `04-profile`, `06-admin`); the other five are slides.
`recording-plan.md` has every action; `OPEN-QUESTIONS.md` lists what to confirm on the first take.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the Blog recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. Enable no feature by hand: the
   Mini Profiler feature must be disabled when the first clip starts.
2. Sign the recorder in (the password is passed in `VIDEO_PASSWORD` and never written in git), and set
   `VIDEO_INIT_SCRIPT` to a script that hides the machine name of the profiler's popup (the rule is in
   `recording-plan.md`):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/mini-profiler/recorder
   export VIDEO_ROOT=<repo>/videos/mini-profiler VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as **snapshot A** (feature disabled).
3. Narrate (`scripts/narrate.py videos/mini-profiler`), then record `02-enable` from snapshot A. It enables the feature:
   stop the site and copy `App_Data` aside again as **snapshot B**.
4. Record `03-roles`, `04-profile` and `06-admin` from snapshot B, in any order; none of them changes the site. A retake
   of `02-enable` starts from snapshot A, and a retake of any other clip from snapshot B.
