# Liquid: Display Content Items, Shapes and Zones

**YouTube:** not published yet · **Length:** 6:00 · **Feature:** `OrchardCore.Liquid` ([Liquid documentation](https://docs.orchardcore.net/en/latest/reference/modules/Liquid/))

A Liquid template receives a shape. The video shows how a template reads the shape of a content item and draws the
shapes of its parts and fields, how the `zone`, `render_section` and `render_body` tags place content in the zones of
the layout, how to load and render other content items, and how to create and change shapes. The examples come from The
Blog Theme. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** made with the Blog recipe, with no data of its own, so it needs no mask script.
The only recording is the front-end tour of chapter 2 (`02-page`); every other chapter is a slide chapter drawn by
`slides.py`, or a card. `recording-plan.md` has the exact actions of each step, and `OPEN-QUESTIONS.md` lists what a
person must confirm on a running site first.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`. The password is chosen
   outside git and passed in `VIDEO_PASSWORD`. No feature has to be enabled, and no record added.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/liquid-display-content/recorder
   export VIDEO_ROOT=<repo>/videos/liquid-display-content VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Narrate with `scripts/narrate.py videos/liquid-display-content`, then record `Recorder record 02-page`. The clip only
   reads the site, so it needs no snapshot and can be recorded again at any time. Check the slides in memory with
   Pillow and Pygments before assembling.
