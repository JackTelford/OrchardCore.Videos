# Auto Setup: Install Sites Without the Setup Screen

**YouTube:** not published yet · **Length:** 4:46 · **Feature:** `OrchardCore.AutoSetup` ([Auto Setup documentation](https://docs.orchardcore.net/en/latest/reference/modules/AutoSetup/))

The video shows how the Auto Setup module answers the questions of the Setup screen from configuration, so a site
installs itself on its first request. It registers the module in the host, configures a tenant, keeps the secrets in
environment variables and user secrets, records the first request and the sign-in, and covers more than one tenant, the
AutoSetupPath, validation, failures, and a distributed lock for several instances. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

Two chapters are recordings, and **neither starts from a site provisioned with Auto Setup**: the subject of the video is
the Setup screen and the installation itself. Every other chapter is a slide. The full steps, selectors and checks are in
`recording-plan.md`; in short:

1. Build `src/OrchardCore.Cms.Web` of OrchardCore, and run it in the Development environment with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, on `http://localhost:5010`, with outbound calls blocked
   (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address).
2. Record `01-setup-screen` first, on a site with **no** Auto Setup configuration (no `OrchardCore__AutoSetup__` variable, no
   user secret, no active section in `appsettings.json`). Nothing is typed and the form is never submitted.
3. Stop the site, empty its data folder, and start it again with the Auto Setup variables of `recording-plan.md` (site name
   **Lakeside Journal**, user `admin`, `admin@example.com`, the Sqlite provider, the Blog recipe, time zone
   `America/Los_Angeles`, and the password in `VIDEO_PASSWORD`, chosen outside git). Do not request the site, and do not run
   `Recorder setup`: the first request is the one the clip `05-first-request` records, and it signs in on camera.
4. Narrate and record: `scripts/narrate.py videos/auto-setup`, then `Recorder record 01-setup-screen`, restart as in step 3,
   and `Recorder record 05-first-request`. The second clip installs the site, so each take starts from an emptied data folder.
