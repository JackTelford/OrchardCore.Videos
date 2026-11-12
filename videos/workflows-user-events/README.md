# Workflows: React to Registrations, Sign-Ins and Role Changes

**YouTube:** not published yet · **Length:** 4:31 · **Feature:** [OrchardCore.Workflows](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/)

The video shows how a workflow starts from the user events of the Users module. It explains which events exist and
what starts each one, then builds three small workflows in the admin designer, one for a created account, one for a
role change and one for a sign-in. Each workflow has an event and a Notify Task, and the video ends on the workflow
instances that the three runs left. It is a video for site owners and administrators: it shows the admin and the
sign-in page, and no code. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, the
labels to click, the text to type and the files of the Orchard Core source each label comes from are in
the recording plan (recording-plan.md), and the points the author could not check without a running site are in the open questions (OPEN-QUESTIONS.md).
In short:

1. Run Orchard Core with its own empty data folder, provisioned by Auto Setup with the Blog recipe, the site name
   Orchard Core, the time zone America/Los_Angeles and the user admin (email admin@example.com, with a password chosen
   outside git). Enable the Workflows feature, sign the recorder in, and keep a copy of the site's data folder and of
   the saved sign-in as the first snapshot. The video needs no seed data: the users and workflows are made on
   camera.
2. Narrate the video, then record the clips with the recorder, as the skill's recording reference describes.
3. Record the clips in the order they play in: the build of the first workflow, the creation of the user jamie.chen, the
   role change, the sign-in, and the instances. Each clip needs what the one before it made, and the sign-in clip signs
   the administrator out and in again, so keep a snapshot after every clip and restore the one before a clip to record
   it again. The clip that ends the video only reads, so it can be recorded again without restoring anything.
