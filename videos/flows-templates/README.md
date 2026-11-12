# Flow and Bag Parts: Templates and Alternates for Their Widgets

**YouTube:** not published yet · **Length:** 4:31 · **Feature:** `OrchardCore.Flows` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Flow/))

A video for module and theme authors. It shows the shapes that the Flow Part and the Bag Part render, the template file
names and alternates that target a part, a widget type or a display type, the way a Liquid template can read the items
of a named Bag Part directly, and the setting that writes the template behind each shape into the page source. The
code on the slides comes from the Flows module, The Blog Theme, The Default Theme and the Agency recipe. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with the Agency recipe, so it needs no mask script. Only two clips are
browser recordings: the Bag Part settings of the `Landing Page` content type (`03-display`) and the Debugging settings
page (`06-debug`). Everything else is drawn from `slides.py`. The step-by-step plan is in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Agency** recipe (it defines
   the `Landing Page` type with named Bag Parts), site name **Orchard Core**, time zone `America/Los_Angeles`, user
   `admin`, e-mail `admin@example.com`. Nothing is seeded and no feature has to be enabled.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/flows-templates/recorder
   export VIDEO_ROOT=<repo>/videos/flows-templates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before another take of `06-debug`, which saves the
   setting Write shape debug information.
3. Narrate and record: `scripts/narrate.py videos/flows-templates`, then `Recorder record 03-display 06-debug`. The two
   clips do not depend on each other. Record `03-display` first, because it only reads.
4. The Agency recipe seeds content items whose names are not fake data. The clips never open a page that lists content
   items, the front end or the dashboard. Keep it that way, and check the contact sheets.
