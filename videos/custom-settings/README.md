# Custom Settings: Turn a Content Type Into Site Settings

**YouTube:** not published yet · **Length:** 4:26 · **Feature:** [`OrchardCore.CustomSettings`](https://docs.orchardcore.net/en/latest/reference/modules/CustomSettings/)

An admin video about the Custom Settings feature. It explains what a custom settings section is, then enables the
feature, creates a content type with two fields, clears its content options and gives it the CustomSettings stereotype,
edits the values under Settings, grants the section's permission to the Editor role, and adds the section to a
deployment plan. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every clip, the exact actions
of every step and the fake values are in [recording-plan.md](recording-plan.md). What a person must confirm on a running
site is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Run the Orchard Core web project on http://localhost:5010 with an empty data folder outside both repositories,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin
   with the email admin@example.com. The password is chosen outside git.
2. Sign the recorder in once, as the recording plan describes.
3. Prepare the site outside the capture. Disable the **Custom Settings** feature, which the Blog recipe enables and which
   the first clip enables on camera. Create a deployment plan named **Settings Export** without steps. Copy the data
   folder aside as snapshot A.
4. Narrate, then record the clips in the order they play in, because each one starts from what the one before it left:
   02-enable, 03-create, 04-edit, 05-permission, 06-deploy. The slides of chapter 01 are drawn by slides.py and are not
   recorded. Take a snapshot after each clip, and restore the one before a clip to record that clip again.
