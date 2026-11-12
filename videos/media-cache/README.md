# Media Cache: Purge the Local Copy of Remote Media

**YouTube:** not published yet · **Length:** 3:31 · **Feature:** [OrchardCore.Media.Cache](https://docs.orchardcore.net/en/latest/reference/modules/Security/Permissions/)

When media files live in Azure Blob Storage or Amazon S3, the site keeps a local copy of each file it serves. The video
explains what that copy is used for, enables the Media Cache feature, empties the cache with Purge All on the Asset
Cache page, grants the Manage Asset Cache Folder permission to a role, and names the two background tasks that clean the
caches on a schedule. It is for site owners and administrators: it shows the admin and a front-end image, and no code.
The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person needs,
step by step, is in recording-plan.md: the site, the remote store stand-in, the exact seed, the order of the clips
and the actions of every step. In short:

1. **The site.** A local site made from the Orchard Core source, set up unattended with the Blog recipe, the site name
   **Orchard Core**, the time zone America/Los_Angeles, and the user admin (admin@example.com). The password is
   chosen outside git and passed to the recorder in VIDEO_PASSWORD.
2. **The remote store.** The Azurite emulator for Azure Blob Storage, in Docker, never a real Azure account. The host
   configuration of the site points to it, so Azure Media Storage works as soon as the feature is enabled. The plan
   lists the exact values.
3. **The starting state.** The Blog recipe's features only: Media Cache and Azure Media Storage stay disabled. Take the
   first snapshot of the site here.
4. **First clip, 02-enable.** It enables Media Cache and opens its page, which reports that no remote store is
   configured.
5. **The seed between the clips.** Enable Azure Media Storage, upload the three stock pictures of the Blog theme to the
   Media Library, and take the second snapshot (the site and the emulator's data).
6. **The other clips.** 03-purge and 04-permission both start from the second snapshot and do not depend on each
   other. The chapters 01-how and 05-clean are slides and need no site.

Every clip changes the site, so restore the snapshot it starts from before each take. Narrate before recording, so
the recorder holds each step for the length of its narration.
