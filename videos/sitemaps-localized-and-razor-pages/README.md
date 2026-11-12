# Sitemaps for Localized Content and Razor Pages

**YouTube:** not published yet · **Length:** 4:02 · **Feature:** [`OrchardCore.ContentLocalization.Sitemaps`](https://docs.orchardcore.net/en/latest/reference/modules/Sitemaps/#localized-sitemaps) and [`OrchardCore.Sitemaps.RazorPages`](https://docs.orchardcore.net/en/latest/reference/modules/Sitemaps/#decoupled-razor-pages)

A video for developers about two features that extend the Sitemaps module. Localized Content Item Sitemaps adds an
alternate link with `hreflang` for every localization of a content item. Sitemaps for Decoupled Razor Pages lists the
content types that your own Razor Pages display, with `SitemapsRazorPagesOptions`. The video also shows how a sitemap
finds the routes of content types, and how to write a provider of your own. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything the recorder needs
is in [`recording-plan.md`](recording-plan.md), and the points a person must confirm first are in
[`OPEN-QUESTIONS.md`](OPEN-QUESTIONS.md).

1. Build a throwaway host project named `OrchardSite` next to `src/OrchardCore.Cms.Web` of OrchardCore, outside the
   solution, with the `Program.cs` and the Razor Page of the plan. Run it with `ORCHARD_APP_DATA` pointing at an empty
   folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blank site recipe, site
   name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`).
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/sitemaps-localized-and-razor-pages/recorder
   export VIDEO_ROOT=<repo>/videos/sitemaps-localized-and-razor-pages VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Seed the site as the plan says, outside the capture: enable five features and import two recipes with Tools,
   Deployments, JSON Import, then create the sitemap Site Pages in Tools, Search Engine Optimization, Sitemaps. Stop the
   site and copy its `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/sitemaps-localized-and-razor-pages`, then `Recorder record all`. The
   four clips (`02-features`, `02-sitemap`, `03-types`, `03-sitemap`) do not depend on each other; record them in the
   order of the video. The other chapters are slides, drawn by `slides.py`.
