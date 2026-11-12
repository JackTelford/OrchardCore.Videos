# Localize a Module: Strings, Plurals and Extraction

**YouTube:** not published yet · **Length:** 5:51 · **Feature:** `OrchardCore.Localization` ([Localization](https://docs.orchardcore.net/en/latest/reference/modules/Localize/)), with `OrchardCore.Localization.Extraction`

A video for module and theme authors. It shows how a module declares its translatable strings with `IStringLocalizer`,
`IHtmlLocalizer` and `IViewLocalizer`, how the context of a string is chosen, how `Plural` selects a form, and how
`LocalizationSource` declares text that has no localizer. It ends with the notes and analyzer rules that help
translators, and with the `OrchardCore.Localization.Build` package that extracts the strings of a project into a
template. One short recording shows `Plural` on the Content Items list of the admin; every other screen is a slide.
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video needs no module and no PO file: its slides are drawn from `slides.py`, and its one recording is made on a
**fresh site** with the content of the Blog recipe, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`). The exact
   variables are in `recording-plan.md`. Enable no other feature: the recording shows English text.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/localization-in-modules/recorder
   export VIDEO_ROOT=<repo>/videos/localization-in-modules VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is nothing to seed, so the recorder needs no `PrepareAsync`. Write `recorder/Scenes.cs` with one method,
   `04-plural-screen`, from the steps of `recording-plan.md`, and build the recorder with it.
3. Narrate with `scripts/narrate.py videos/localization-in-modules`, then record the only clip with
   `Recorder record 04-plural-screen`. The clip does not change the site, so it needs no snapshot, and it can be
   recorded again at any time.
4. Check the slides by rendering each one in memory from the video's folder, with the skill's `scripts` folder on
   `PYTHONPATH`; assemble with `scripts/assemble.py videos/localization-in-modules`.
