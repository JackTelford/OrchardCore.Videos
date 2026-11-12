# Authentication Ticket Store: Keep Sign-In Tickets on the Server | Orchard Core

**YouTube:** not published yet · **Length:** 3:31 · **Feature:** `OrchardCore.Users.Authentication.CacheTicketStore` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Users/TicketStore/))

A video for module and site developers about the Users Authentication Ticket Store feature. It explains why the ticket of
a signed-in user can outgrow its cookie, shows where the feature is enabled and which recipe step enables it, walks through
the code of the session store (`CacheTicketStore`) and its registration, and ends with where the tickets are kept: the memory
cache of the tenant by default, or Redis when several instances of a site share them, with the Data Protection keys shared
as well. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with no data beyond the Blog recipe, so it needs no mask script. Only one clip
is a recording (`02-enable`, on the Features page); the other chapters are slides. [recording-plan.md](recording-plan.md)
holds the exact actions of each step, and [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) holds what a person must confirm on a
running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`). Enable no other feature: the
   **Users Authentication Ticket Store** must read Disabled.
2. Sign the recorder in. The video has no `PrepareAsync`, so there is nothing to seed:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/users-ticket-store/recorder
   export VIDEO_ROOT=<repo>/videos/users-ticket-store VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Run `Recorder setup` again after every restart of the site or restore of its `App_Data`.
3. Narrate and record: `scripts/narrate.py videos/users-ticket-store`, then `Recorder record 02-enable`. The clip never
   clicks Enable, so it does not change the site and can be recorded again as often as needed.
