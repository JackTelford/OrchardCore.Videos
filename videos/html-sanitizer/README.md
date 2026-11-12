# HTML Sanitizer: Keep Unsafe Markup Out of Content

**YouTube:** not published yet · **Length:** 3:54 · **Feature:** `topic:Sanitizer` ([HTML Sanitizer](https://docs.orchardcore.net/en/latest/reference/modules/Sanitizer/))

A video for module and theme authors. It shows that the HTML Sanitizer cleans the final rendered HTML each time content
is displayed, and stores the authored HTML or Markdown as written. It covers where the sanitizer applies by default, the
**Sanitize Html** setting of a part, calling the sanitizer from Razor, Liquid and a service, and configuring it in
code, including the rule that its cache imposes. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only the chapter "The
Sanitize Html setting" is a recording; the other chapters are slides and cards. The recording needs no seed data, and it
changes nothing on the site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: tenant `Default`, the Blog recipe, site
   name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. No feature has to be
   enabled: the Blog recipe already enables the Html and Markdown modules, and the sanitizer is always registered.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/html-sanitizer/recorder
   export VIDEO_ROOT=<repo>/videos/html-sanitizer VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is nothing to prepare, so `Recorder prepare` is not needed.
3. Narrate and record: `scripts/narrate.py videos/html-sanitizer`, then `Recorder record 02-setting`. The clip only opens
   pages, so it can be recorded again at any time. Its steps and selectors are in `recording-plan.md`, and the questions
   to settle on a running site are in `OPEN-QUESTIONS.md`.
4. The slides are drawn from `slides.py` when the video is assembled.
