# Workflows for Tenants: Create and Set Up Tenants Automatically

**YouTube:** not published yet · **Length:** 4:36 · **Feature:** [OrchardCore.Tenants](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/) with [OrchardCore.Workflows](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/)

The Tenants module adds four activities to the workflow designer: Create Tenant Task, Setup Tenant Task, Disable Tenant
Task and Enable Tenant Task. The video explains what each one needs, turns on the Tenants and Workflows features,
builds a workflow that creates a tenant called Bluebird and sets it up, publishes and runs it from the designer, and
opens the tenant. It ends with what happens when a task takes its Failed outcome. The title, description, tags and
chapters YouTube shows are in the video's metadata file. The length is the narration's words divided by 150.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script for names. One thing is masked: the
Admin password box of Setup Tenant Task is a plain text box, so the recording plan hides what is typed in it.

1. Read OPEN-QUESTIONS.md first. Its first point, the Name and Description boxes of Create Tenant Task, decides how the
   create clip is typed and may block the recording until it is settled.
2. Run Orchard Core from a checkout of the web project, with its own data folder outside both repositories, provisioned by
   Auto Setup: site name Orchard Core, the Blog recipe, the time zone America/Los_Angeles, the user admin with the email
   admin@example.com, and a password chosen outside git and passed to the recorder. The Tenants and Workflows features must
   be disabled: the second chapter enables them on camera. Nothing else is seeded.
3. Sign the recorder in once, outside the capture. Write the init script that masks the Admin password box, outside the
   repository, and pass it to the recorder.
4. Do the rehearsal that recording-plan.md describes, off camera. It checks the chain, the password rules, and the claims of the
   last slides, and it needs a restored site afterwards.
5. Narrate, then record the clips in the order they play in, because each one starts from the state the previous one left:
   02-features, 03-workflow, 04-create, 05-setup, 06-run, 07-result. The slide chapters and the two cards are drawn from the
   storyboard and need no recording.
6. Stop the site and copy its data folder aside after every clip. Restore the copy before another take of the clip that follows.
   The snapshots are listed in recording-plan.md. Never commit a snapshot: it holds the workflow with its password.
