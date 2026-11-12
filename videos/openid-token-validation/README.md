# OpenID Connect Token Validation: Protect Your APIs

**YouTube:** not published yet · **Length:** 4:38 · **Feature:** `OrchardCore.OpenId.Validation` ([OpenID Connect Token Validation](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/#token-validation))

A video for developers about the resource-server side of OpenID Connect: how the API authentication scheme hands a
bearer token to the validation handler, how to choose who issues the tokens (an authorization server tenant or a remote
authority), and how to protect an endpoint of your own with a permission check. Two chapters are recordings of the admin
UI (enabling the feature and its settings page); the others are slides with code from the Orchard Core source. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Read `recording-plan.md`: it
lists every action, label and wait.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Enable **OpenID Connect Authorization Server** (Tools > Features) and leave **OpenID Connect Token Validation**
   disabled: the first recording enables it. Do not open or save the Token Validation settings page before the take.
3. Sign the recorder in and narrate:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/openid-token-validation/recorder
   export VIDEO_ROOT=<repo>/videos/openid-token-validation VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/openid-token-validation
   ```

   Then stop the site and copy its `App_Data` and `state.json` aside as snapshot A.
4. Record the clips in this order, because the second one needs the feature the first one enables:
   `03-enable` (from snapshot A), then take snapshot B, then `04-settings` (from snapshot B). Both clips change the
   site: restore the matching snapshot before every other take. The other chapters are slides and cards that
   `assemble.py` draws from `slides.py`.
