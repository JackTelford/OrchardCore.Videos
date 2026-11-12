# Content Definitions in Code: Migrations and Recipes

**YouTube:** not published yet · **Length:** 6:07 · **Feature:** `OrchardCore.ContentTypes` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/))

A video for developers about defining content types, parts and fields in code. It reads the migration and the recipe
of the SEO module as a worked example: the manifest, the registration of a migration, `AlterTypeDefinitionAsync` and
`AlterPartDefinitionAsync`, a recipe run by `IRecipeMigrator`, the versions each site catches up on, and a class for
the part. One chapter is a recording of the admin, where enabling the SEO module creates a part, a type and their
fields. The reference pages are [Content Types](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/)
and [Data Migrations](https://docs.orchardcore.net/en/latest/reference/modules/Migrations/). The title, description,
tags and chapters YouTube shows are in `video.json`; `recording-plan.md` says how to record it, and
`OPEN-QUESTIONS.md` lists what a person has to confirm on a running site.

## Recording it again

Only the chapter "A migration at work" (clip `07-seo`) is recorded; the other chapters are slides drawn by `slides.py`.

1. Build `src/OrchardCore.Cms.Web` of Orchard Core (the commit named in `recording-plan.md`) and run it with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by
   Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email
   `admin@example.com`). Do not enable the SEO module: the clip does it.
2. Sign the recorder in. There is no seed data and no `PrepareAsync`:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-definitions-in-code/recorder
   export VIDEO_ROOT=<repo>/videos/content-definitions-in-code VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside before the clip: the clip enables the SEO module, and
   that cannot be undone from the admin. Restore the copy before every take.
3. Narrate and record: `scripts/narrate.py videos/content-definitions-in-code`, then `Recorder record 07-seo`. It is
   the only clip, so there is no order to follow. Check the contact sheet of the clip: the frame at the end of each step
   must show what `recording-plan.md` lists, with no error message.
4. Assemble. The slides need nothing but the narration.
