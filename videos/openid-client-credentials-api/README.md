# Call the Orchard Core API With Client Credentials

**YouTube:** not published yet · **Length:** 5:34 · **Feature:** `OrchardCore.OpenId.Server` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/))

A program that runs without a person, such as an import job or a build step, signs in to Orchard Core as an
application with the OpenID Connect client credentials flow. The video enables the authorization server and token
validation, creates a role and an application, requests an access token, and calls the Content API with it to create,
read, update and delete a content item. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The complete steps, the
clip order and the exact actions of every step are in `recording-plan.md`; what a person has to confirm on a running
site is in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). Enable no OpenID Connect feature: the first clip
   enables them.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/openid-client-credentials-api/recorder
   export VIDEO_ROOT=<repo>/videos/openid-client-credentials-api VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Copy the site's `App_Data` folder and `state.json` aside before the first take: restore that copy before every
   full take.
3. Narrate with `scripts/narrate.py videos/openid-client-credentials-api`, then record the four clips in this order
   (each one starts from the site the previous one left, and the last one needs an article that a client program
   creates between the clips): `02-site`, `03-role`, `04-app`, then the two calls described in `recording-plan.md`,
   then `07-result`. The four slide chapters (`01-flow`, `05-token`, `06-call`, `08-fail`) are drawn from
   `slides.py` and need no recording.
