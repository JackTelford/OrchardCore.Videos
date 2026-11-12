# GraphQL: Settings, Permissions and Hidden Fields

**YouTube:** not published yet · **Length:** 4:30 · **Feature:** `OrchardCore.Apis.GraphQL` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

The video is for developers who run or extend the GraphQL module. It reads the options of the `Apis`, `GraphQL`
configuration section, follows the permissions that guard the endpoint, mutations and content, grants one to a role,
hides content types and parts from the schema in the content type editor, and then hides fields, parts and content types
in code with the GraphQL content options. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Three clips are browser
recordings; the other steps are slides drawn by `slides.py`. `recording-plan.md` holds every action, wait and label.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Enable the **GraphQL** feature in **Tools**, **Features**. The Blog recipe does not enable it, and the GraphQL
   options of the content type editor only appear when it is enabled. No other feature, setting or content is seeded.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/graphql-settings-permissions/recorder
   export VIDEO_ROOT=<repo>/videos/graphql-settings-permissions VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside: restore that copy before every take of `02-roles`
   and `03-type`, which save a role and a content type.
4. Narrate and record: `scripts/narrate.py videos/graphql-settings-permissions`, then `Recorder record 03-parts`,
   `Recorder record 02-roles` and `Recorder record 03-type`. The three clips do not depend on each other.
