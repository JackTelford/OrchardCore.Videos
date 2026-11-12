# Error Pages: Customize Not Found and Error Pages

**YouTube:** not published yet · **Length:** 4:00 · **Feature:** `OrchardCore.Diagnostics` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Diagnostics/))

How the Orchard Diagnostics feature turns an error status into a page, and how to replace the page of a status: with a
view in a theme (Razor or Liquid), or with a template written in the admin with the Templates feature. The video
also shows the order in which the names and the sources are tried, and the requests that get no error page. The
title, description, tags and chapters YouTube shows are in `video.json`.

It is a developer video: the theme views, the Liquid view, the recipe and the module code are slides. Two chapters are
recordings of a browser: the built-in pages, and the template written in the admin.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Read `recording-plan.md`
first: it lists the exact actions of every step. `OPEN-QUESTIONS.md` lists what was read in the source but not run.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the SaaS recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The recipe enables **Orchard Diagnostics**;
   in **Tools > Features**, enable **Templates** once. The Blog and Agency recipes are not used: their themes already
   replace the Not Found page.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/diagnostics-error-pages/recorder
   export VIDEO_ROOT=<repo>/videos/diagnostics-error-pages VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every full take.
3. Narrate and record: `scripts/narrate.py videos/diagnostics-error-pages`, then `Recorder record 02-builtin 04-admin`.
   Record `02-builtin` first: `04-admin` creates the template `HttpError__NotFound`, which changes the Not Found page
   of the site. To record `02-builtin` again afterwards, restore the copy of `App_Data`.
