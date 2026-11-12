# Content Permissions: By Content Type and by Owner

**YouTube:** not published yet · **Length:** 5:08 · **Feature:** [OrchardCore.Contents](https://docs.orchardcore.net/en/latest/reference/modules/Contents/) (with OrchardCore.Roles, documented at [Roles](https://docs.orchardcore.net/en/latest/reference/modules/Roles/))

The video explains how content permissions work in Orchard Core: the Contents permissions that apply to every content type,
the permissions of a single Securable content type, and the two forms of each permission, one for the items a user owns and
one for the items that belong to others. It marks the Article type as Securable, grants a Writer role its permissions in the
role editor, reads the Allow and Effective columns, and then shows what a user in that role can and cannot edit. The title,
description, tags and chapters YouTube shows are kept with the video's other files.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full, step-by-step plan, with
the exact actions of every clip, is in the recording plan of this folder.

1. Build the Orchard Core web project and run it with its data folder in an empty location outside both repositories, on
   the recorder's address, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user **admin** with the email admin@example.com and a password chosen outside git. No feature needs
   to be enabled.
2. Sign the recorder in as admin and seed the site: publish an article titled Press Kit (the Blog recipe already provides an
   article titled About). Check, as the plan lists, that the Anonymous and Authenticated roles hold View all content and that
   the Editor role holds Edit content for others. Copy the site's data folder and the sign-in file aside as the first snapshot.
3. Record the clips in this order, taking a snapshot after each one that changes the site:
   1. Make a type securable (Article becomes Securable).
   2. Grant permissions to a role (the Writer role is created and receives three permissions).
   3. Permissions for all content (the Editor role is only read).
   4. Outside the capture, create the user jamie.chen with the Writer role and the same password as the administrator, and
      save a second sign-in for that user.
   5. See it as a writer (recorded signed in as jamie.chen; it creates the article Team Offsite Notes).

   The two slide chapters, How content permissions work and What to remember, need no recording. Restore the matching
   snapshot before recording a clip again.
4. Narrate first, then record, as the producer skill describes. Check every contact sheet: the frame at the end of each step
   must show what its narration describes, with no error message and no real data.
