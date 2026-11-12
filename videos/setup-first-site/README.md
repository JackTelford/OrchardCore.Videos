# Setup: Install Your First Orchard Core Site

**YouTube:** not published yet · **Length:** 4:45 · **Feature:** `OrchardCore.Setup` ([Setup](https://docs.orchardcore.net/en/latest/reference/modules/Setup/))

An empty Orchard Core site shows the Setup screen. The video answers its questions in order: the name of the site, the
recipe, the default time zone and the database, then the super user. It explains what Finish Setup does and which
databases need a connection string, signs in to the admin of the site, and checks that the answers were saved as
settings. The title, description, tags and chapters YouTube shows are in `video.json`; the exact steps to record are in
`recording-plan.md`.

## Recording it again

The video is recorded on a **fresh site** that has never been set up, with fake data only, so it needs no mask script.
Unlike the other videos, the site is **not** provisioned by Auto Setup: the Setup screen is the subject.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it on `http://localhost:5010` with `ORCHARD_APP_DATA`
   pointing at an **empty** folder outside both repositories, and with the time zone of the machine set to UTC (`tzutil /s "UTC"` on Windows, and restore it afterwards; `TZ=UTC` on Linux and macOS).
   UTC is not an entry of the Default Time Zone list, so the list then shows its first entry, `(GMT+00) Africa/Abidjan (+00:00)`,
   and nothing about where the recorder lives. Set no Auto Setup, database or recipe preset for the Default tenant.
2. Write the scenes of the three clips in `recorder/Scenes.cs` of this video's folder, from `recording-plan.md`: one method
   per clip (`02-questions`, `03-finish`, `04-signin`), with one `StepAsync` per step id of the plan.
3. Choose the password outside git and export it, with the other recorder variables, in the shell that runs the
   recorder. Do not run `Recorder setup`, and delete `state.json` from the video's folder if there is one: the site has
   no user until `03-finish` creates it, and `04-signin` must start signed out.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/setup-first-site/recorder
   export VIDEO_ROOT=<repo>/videos/setup-first-site VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   ```

4. Narrate and record: `scripts/narrate.py videos/setup-first-site`, then record the clips in this order:
   `Recorder record 02-questions`, `Recorder record 03-finish`, `Recorder record 04-signin`.
   - `02-questions` changes nothing. It can be recorded again as long as the site is not set up.
   - `03-finish` **sets the site up**. Before another take: stop the site, empty `ORCHARD_APP_DATA`, start the site again.
   - `04-signin` starts from the site as `03-finish` left it. Copy `ORCHARD_APP_DATA` aside after a good take of
     `03-finish`, and restore that copy before another take of `04-signin`.

The seed data is three answers typed on the Setup screen (`Contoso Bakery`, the Blog recipe, the Los Angeles time zone)
and one user (`admin`, `admin@example.com`). Everything else comes from the Blog recipe.
