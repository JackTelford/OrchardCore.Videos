# Secrets in Your Module: Offer Secret Inputs in Your Settings

**YouTube:** not published yet · **Length:** 5:16 · **Feature:** [`OrchardCore.Secrets`](https://docs.orchardcore.net/en/latest/reference/modules/Secrets/)

A video for module authors. It shows the Password field of the SMTP email settings with and without the Secrets
feature, then the code a module needs to offer the same secret input in its own settings: a settings class that keeps a
protected value and a secret name, an editor model, a display driver, a view with the secret-input tag helper, a call
that reads the value at runtime, and an optional driver for the Migrate Secrets screen. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

Only chapter 01 is a recording: two clips of the SMTP settings, `01-without` and `01-with`. Every other chapter
is a slide, drawn by `slides.py`. `recording-plan.md` describes the site, the seed data and every step of the two
clips; the scene methods go in `recorder/Scenes.cs` when the video is recorded.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore at the revision named in `recording-plan.md` and run it with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by
   Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email
   `admin@example.com`). The password is chosen outside git and passed in `VIDEO_PASSWORD`.
2. Sign the recorder in, once the site is up:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/secrets-in-your-module/recorder
   export VIDEO_ROOT=<repo>/videos/secrets-in-your-module VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Enable the features **Email** and **SMTP Email Provider**, and save the SMTP settings with the fake values of
   `recording-plan.md` (no password). Stop the site and copy its `App_Data` folder and `state.json` aside as snapshot
   S1. The Secrets feature stays disabled. Start the site again and record the first clip:

   ```bash
   <bin>/Recorder record 01-without
   ```

4. With the take of the first clip kept, enable the **Secrets** feature and create the text secret `Smtp.Password`.
   Stop the site and copy `App_Data` and `state.json` aside as snapshot S2. Start the site again and record the second
   clip: `<bin>/Recorder record 01-with`.

`01-with` depends on `01-without`: the first clip saves a protected password in the SMTP settings, which the second clip
replaces with a reference to the secret. Both clips change the site. To record `01-without` again, restore S1 and repeat
step 4 afterwards; to record `01-with` again, restore S2. Narrate with `scripts/narrate.py videos/secrets-in-your-module`
before recording; the pronunciation of the identifiers is set in `"spoken"` of `storyboard.json`, and a person confirms
each spelling with `scripts/pronounce.py` first.
