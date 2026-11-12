# Azure Communication Services Email

**YouTube:** not published yet · **Length:** 3:49 · **Feature:** `OrchardCore.Email.Azure` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Email.Azure/))

The video shows how a site sends its email through Azure Communication Services. It explains the two providers the
module adds and what you need to use the first one, then enables the feature, fills in the provider's settings with
fake values, and opens the Email Test page without sending a message. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Azure Communication Services
has no local emulator: the connection string is a fake value that is never used, and no message is sent.
`recording-plan.md` holds every action and the exact values.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore (the commit named in `recording-plan.md`) and run it with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by
   Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). Set
   `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so that nothing can reach a mail provider.
2. Sign the recorder in. This video has no `PrepareAsync`: the clips enable the features and save the settings
   themselves.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/email-azure-communication-services/recorder
   export VIDEO_ROOT=<repo>/videos/email-azure-communication-services VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as snapshot A before the first clip.
3. Narrate and record: `scripts/narrate.py videos/email-azure-communication-services`, then record the clips in the order
   they play, because each one starts from the state the previous one leaves:
   `02-enable` (enables the Email and Azure Communication Services Email features; keep snapshot B afterwards),
   `03-configure` (saves the settings; keep snapshot C afterwards), and `04-test` (changes nothing). Restore the snapshot
   a clip needs before each take of it.
