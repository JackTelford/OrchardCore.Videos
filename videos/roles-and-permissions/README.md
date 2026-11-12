# Roles and Permissions: Control Who Can Do What

**YouTube:** not published yet · **Length:** 5:24 · **Feature:** [OrchardCore.Roles](https://docs.orchardcore.net/en/latest/reference/modules/Roles/)

This video explains how Orchard Core decides who can do what. It starts with permissions, roles, and the three system
roles, Administrator, Anonymous, and Authenticated. It then walks through the Roles page, creates a role, chooses its
permissions with the Allow and Effective columns, gives the role to a user, and deletes a role. It is an admin video: it
shows the admin screens only. A separate video covers permissions for each content type and for the owner of a content
item. The title, description, tags, and links YouTube shows are kept with the video.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan
(recording-plan.md) gives every action, wait, and selector; this is the outline.

1. **Site.** A fresh Orchard Core site provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user **admin** with the email admin@example.com and a password chosen outside git. The site runs
   on http://localhost:5010 with its own empty App_Data folder, and outbound calls are blocked.
2. **Features and settings.** No feature to enable: the Blog recipe turns on Roles and Users. The Admin settings stay at
   their defaults, so lists are shown in the List layout with buttons for the actions.
3. **Seed data.** One role, **Guest Blogger**, with the description "Writes posts for the blog as a guest.", created with
   the Create button. There is no user besides admin: the person who appears in the video, priya.raman, is created on
   camera.
4. **Snapshots and order.** Take a snapshot of the site after the seed data (A). Record the clips in this order: the
   Roles page, create a role, choose permissions, give a role to a user, delete a role. Create a role changes the site, so
   take a snapshot after it (B), and another after choose permissions (C). Restore the snapshot named in the plan before
   another take of a clip.
5. **Slides.** Seven steps are slides (the idea, the system roles, and the summary); they need no site.
6. **Narrate and record.** Generate the narration first, then record each clip, then assemble, as the video producer
   skill describes. Check every contact sheet and read the plan's checks for private data before assembling.
