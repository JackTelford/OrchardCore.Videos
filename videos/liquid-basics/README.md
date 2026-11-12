# Liquid: Objects, Filters and Tags in Orchard Core

**YouTube:** not published yet · **Length:** 6:25 · **Feature:** `OrchardCore.Liquid` ([Liquid](https://docs.orchardcore.net/en/latest/reference/modules/Liquid/)), with `OrchardCore.Liquid.Core`

A video for module and theme authors about the Liquid language of Orchard Core. It reads the Liquid templates of the
Blog theme, shows the blog post page that they draw, and goes through the syntax, the objects a template can read,
the filters, the tags and the parser options, with the feature that registers each one. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

Only one clip, `03-page`, is a recording: a public page of the Blog theme. The other chapters are slides drawn by
`slides.py`, which need no site. The video is recorded on a **fresh site** with fake data only, so it needs no mask
script. The full instructions, step by step, are in `recording-plan.md`; the person who records writes
`recorder/Scenes.cs` from it, one method for the clip `03-page`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). The Blog recipe enables the Templates module,
   which brings in the Liquid feature, so no feature needs to be enabled by hand.
2. Open `/` once and wait for the Blog home page, so that Auto Setup has finished. The clip is recorded signed out:
   do not sign the recorder in, or move `state.json` aside for the take.
3. Narrate and record:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/liquid-basics
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/liquid-basics/recorder
   export VIDEO_ROOT=<repo>/videos/liquid-basics VIDEO_BASE_URL=http://localhost:5010
   <bin>/Recorder record 03-page
   ```

   The clip changes nothing on the site, so it can be recorded again at any time without restoring a snapshot.
4. Before narrating, confirm the spoken spellings listed in `OPEN-QUESTIONS.md` with `scripts/pronounce.py`.
