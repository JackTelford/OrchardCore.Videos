# CORS: Allow Other Sites to Call Your APIs

**YouTube:** not published yet · **Length:** 4:26 · **Feature:** [OrchardCore.Cors](https://docs.orchardcore.net/en/latest/reference/modules/Cors/)

The video explains what an origin is and how a browser uses CORS headers to decide whether a page may use the response of
another site. It then enables the CORS Configuration feature, creates a policy with credentials, origins, headers and
methods, and shows the combination of any origin with credentials that the admin refuses to save. It ends with the
settings to keep narrow and the checks to make when a browser blocks a call. The title, description, tags and chapters
YouTube shows are in video.json; the instructions for the person who records are in recording-plan.md.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run a fresh Orchard Core site on port 5010 with its own data folder outside both repositories, provisioned by Auto Setup:
   the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin, email admin@example.com.
   No feature is enabled beforehand: the **CORS Configuration** feature is enabled on camera, and the video has no seed
   data.
2. Sign the recorder in as admin with the password chosen outside git. Stop the site and copy its data folder and the
   saved sign-in aside as snapshot 0.
3. Narrate, then record the clips in this order, because each one starts from the site that the previous one leaves:
   02-enable, then 03-create, then 04-unsafe. Copy the data folder aside after 02-enable (snapshot 1) and after
   03-create (snapshot 2), and restore the snapshot a clip starts from before recording it again. 04-unsafe saves
   nothing. The slide chapters and the cards need no recording.

The site, the exact values to type, the actions of every step and the commands are in recording-plan.md.
