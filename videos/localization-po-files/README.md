# Translate the UI: Install and Write PO Files

**YouTube:** not published yet · **Length:** 5:14 · **Feature:** `OrchardCore.Localization` ([Localization module documentation](https://docs.orchardcore.net/en/latest/reference/modules/Localize/))

The video is for developers who want an Orchard Core site in another language. It installs the
community French files (the [installation guide](https://docs.orchardcore.net/en/latest/guides/install-localization-files/)),
enables the Localization feature and the culture, and tests the result with the culture query string. It then shows where
PO files are read from and in which order, and how to write entries of your own for a service, a view, a tenant, and
the texts that carry placeholders. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything the recorder
needs, down to the exact clicks, is in `recording-plan.md`. In short:

1. Run `src/OrchardCore.Cms.Web` of OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Localization
   feature is **not** enabled by the recipe: the first clip enables it.
2. Build the site, then, before it starts, empty the checkout's `src/OrchardCore.Cms.Web/Localization` folder if the build copied
   translation files into it, and put the stand-in French file of `recording-plan.md` in its `fr/` folder. It has the layout
   of the downloaded `fr.zip` and translates a few texts of the Features page. Start the site with `dotnet run --no-build`.
   Nothing of Crowdin is recorded.
3. Sign the recorder in, narrate, and record the two clips in this order, from the snapshots of `recording-plan.md`:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/localization-po-files/recorder
   export VIDEO_ROOT=<repo>/videos/localization-po-files VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder record 03-enable     # enables Localization and adds the culture: it changes the site
   <bin>/Recorder record 04-result     # needs 03-enable; it does not change the site
   ```

   Restore the first snapshot before each retake of `03-enable`, and the second one before each retake of `04-result`.
4. The slide chapters (`01-po`, `02-install`, `04-query`, `05-where`, `06-write`) are drawn by `slides.py`; there is nothing
   to record for them. Delete the stand-in file when the recording is over.
