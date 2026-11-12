# Shells and Configuration: How Tenants Are Configured

**YouTube:** not published yet · **Length:** 6:09 · **Feature:** `topic:Shells` ([Shells Configuration Providers](https://docs.orchardcore.net/en/latest/reference/modules/Shells/))

A video for developers. It shows what a tenant is made of (its settings and its configuration, and the files that hold
them), the layers that the configuration of a tenant is built from and which one wins, how to prepare a tenant
before it exists and how to change one that exists, and how to give every tenant the same database. It ends with how a
module reads its configuration and how the Azure Blob Storage and Database providers keep the files in a shared store.
Almost everything is slides; one short recording shows a prepared tenant on the Tenants page and its Edit page. The
title, description, tags and chapters YouTube shows are in `video.json`, and the documentation pages it backs are
[Shells Configuration Providers](https://docs.orchardcore.net/en/latest/reference/modules/Shells/),
[Configuration](https://docs.orchardcore.net/en/latest/reference/modules/Configuration/) and the
[Configuration topic](https://docs.orchardcore.net/en/latest/topics/configuration/).

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The full steps, with the
selectors, the labels and the checks for private data, are in [recording-plan.md](recording-plan.md); points to check
on a running site are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup through environment variables (the Blog
   recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, e-mail `admin@example.com`, the
   password in `VIDEO_PASSWORD`). This is not a video about the Setup screen, so Auto Setup and `state.json` are used.
2. **Before the first start**, create `appsettings.json` in the `App_Data` folder with a section `Acme` under
   `OrchardCore` (State `Uninitialized`, Url Prefix `acme`, Category `Demo`, a Description and the database provider
   `Sqlite`): the exact content is in the recording plan. Start the site, open it once so Auto Setup sets up the Default
   tenant, and enable the **Tenants** feature in **Tools > Features**.
3. Sign the recorder in and check the Tenants page:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/shells-and-configuration/recorder
   export VIDEO_ROOT=<repo>/videos/shells-and-configuration VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/shells-and-configuration`, then `Recorder record 03-tenants`. It is the
   only clip, it depends on no other, and it changes nothing on the site as long as the Acme tenant is never set up.
   The slides need no recording.
