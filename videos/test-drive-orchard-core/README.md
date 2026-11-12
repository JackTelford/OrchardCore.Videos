# Test Drive Orchard Core: Run It From Source

**YouTube:** not published yet · **Length:** 4:26 · **Feature:** `guide:getting-started/test-drive-orchard-core` ([Test drive Orchard Core](https://docs.orchardcore.net/en/latest/getting-started/test-drive-orchard-core/))

The video compares the four ways the documentation lists to test drive Orchard Core (Try Orchard Core, DotNest, Docker
and the source code), then runs the source code: it clones the repository, starts `OrchardCore.Cms.Web` with
`dotnet run`, goes through the Setup screen with the Blog recipe, and looks at the home page and the admin. It ends with
the host project and how it registers Auto Setup, the layout of the repository, and the `App_Data` folder where the site keeps its data. It is a
developer video: the commands and `Program.cs` are slides, and only the Setup screen, the home page and the admin are
recorded. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

This video is recorded on an **uninstalled site**: its subject is the Setup screen, so it is recorded without Auto Setup
and without a `state.json`. `recording-plan.md` has every step, selector and label; the order and the fake data are:

1. Run `src/OrchardCore.Cms.Web` of the Orchard Core source (`main`) with `dotnet run`, on `http://localhost:5000`, with
   `ORCHARD_APP_DATA` pointing at an **empty folder** outside both repositories. Check that no Auto Setup configuration
   exists (the example in `appsettings.json` stays commented out, `dotnet user-secrets list` shows no
   `OrchardCore` secret, no environment variable has a name that contains `OrchardCore`), so the first request shows the Setup screen.
2. Narrate: `scripts/narrate.py videos/test-drive-orchard-core`.
3. Build the recorder with the video's scenes, then:

   ```bash
   export VIDEO_ROOT=<repo>/videos/test-drive-orchard-core VIDEO_BASE_URL=http://localhost:5000 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder record 03-setup
   ```

   Do not run `Recorder setup` or `Recorder prepare`: the site is not installed before `03-setup`, and nothing is seeded.
   The time zone `America/Los_Angeles` is selected before the capture starts, so that the time zone of the machine does not
   appear. The data typed on camera is the site name `Test Drive`, the Blog recipe, and the Super User `admin`,
   `admin@example.com` and the password; the Sqlite database is the default and is left as it is.
4. Stop the site (Ctrl+C, and check that no process is left) and copy the `ORCHARD_APP_DATA` folder aside (never commit it), start the site again, and record
   `04-explore`, which depends on `03-setup`. To record `03-setup` again, empty the folder first; to record `04-explore`
   again, restore the copy.

The other chapters are slides that `slides.py` draws; they need no capture.
