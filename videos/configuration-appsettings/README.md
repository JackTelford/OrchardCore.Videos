# Configuration: appsettings, Environment Variables and Tenants

**YouTube:** not published yet · **Length:** 6:07 · **Feature:** `topic:Configuration` ([Configuration](https://docs.orchardcore.net/en/latest/reference/modules/Configuration/))

A developer video about how an Orchard Core site and each of its tenants read their settings. It covers the
`OrchardCore` section and its nested sections, environment variables, the order in which the sources load, the
configuration of a tenant before and after it exists, one database shared by every tenant, and how a module of your
own reads a section. The title, description, tags and chapters YouTube shows are in `video.json`.

Almost every step is a slide. One clip, `04-list`, records a browser: the Tenants page of the admin and the setup
screen of a tenant that exists only in the configuration. `recording-plan.md` has every action, label and source
reference; `OPEN-QUESTIONS.md` lists what a person must confirm on a running site.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it on `http://localhost:5010`, with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories. Provision the Default tenant with Auto Setup through
   environment variables: the **SaaS** recipe (it enables the Tenants feature), site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`.
2. Before the site starts, set the four environment variables that pre-configure the tenant `MyTenant` (state
   `Uninitialized`, URL prefix `mytenant`, database provider `Sqlite`, recipe `SaaS`). The table with their names is
   in `recording-plan.md`. The site holds no other data.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/configuration-appsettings/recorder
   export VIDEO_ROOT=<repo>/videos/configuration-appsettings VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is no `PrepareAsync` to run: the tenant comes from the environment.
4. Narrate and record: `scripts/narrate.py videos/configuration-appsettings`, then `Recorder record 04-list`. It is the
   only clip, it depends on no other, and it leaves the site as it found it. The chapters made of slides are drawn
   by `assemble.py`.
