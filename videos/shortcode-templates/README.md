# Shortcode Templates: Write Your Own Shortcodes With Liquid

**YouTube:** not published yet · **Length:** 4:21 · **Feature:** `OrchardCore.Shortcodes.Templates` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Shortcodes/#shortcode-templates))

The video shows how to write a shortcode as a Liquid template from the admin. It explains the three values a template
receives (Args, Content and Context), creates the `primary` template under Design, Shortcodes, and uses it from the
shortcode picker of an article. It ends with the context of a template, the rules for names and sanitizing, the recipe
and deployment steps, and the permission that guards the feature. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The step-by-step plan, with every
click, every value to type and what must be on screen, is in [recording-plan.md](recording-plan.md). What a person must
confirm on a running site is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both
   repositories, on `http://localhost:5010`, provisioned by Auto Setup (the **Blog** recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Blog recipe enables Shortcode Templates and
   creates the article **About** that the video edits.
2. Write the scenes of the three recordings in `recorder/Scenes.cs` from the plan, sign the recorder in, then disable the
   feature once, outside the capture (Tools, Features, **Disable** on Shortcode Templates), so that the first clip can
   enable it:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/shortcode-templates/recorder
   export VIDEO_ROOT=<repo>/videos/shortcode-templates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as snapshot A: restore that copy before a full take.
3. Narrate and record: `scripts/narrate.py videos/shortcode-templates`, then record the clips in this order, because each
   one needs the site as the previous one leaves it:
   `02-enable` (starts on snapshot A, keep snapshot B), `03-create` (starts on B, keep snapshot C), `04-use` (starts on C).
   The other clips (`01-idea`, `03-code`, `05-more`, `06-deploy`) are slides and need no site. To record a clip again, restore
   the snapshot it starts on.
