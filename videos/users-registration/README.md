# User Registration: Let Visitors Create Accounts

**YouTube:** not published yet · **Length:** 4:57 · **Feature:** [OrchardCore.Users.Registration](https://docs.orchardcore.net/en/latest/topics/security/registration/)

By default, only administrators create accounts. This video enables the Users Registration feature and shows its three
settings: email verification and administrator approval, which decide how a registration is checked, and the theme of
the Register page. It follows three visitors through the form, the confirmation message and the approval, and ends with
the registration options for Google, GitHub and Meta logins. The title, description, tags and chapters YouTube shows are in
video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The full list of actions, the
labels and where each comes from in the Orchard Core source are in recording-plan.md. In short:

1. **The site.** Run the Orchard Core web application with its own data folder, provisioned by Auto Setup (the Blog
   recipe, site name Orchard Core, time zone America/Los_Angeles, user admin with the email admin@example.com and the
   password chosen outside git). Block outbound web calls with a dead proxy address.
2. **The seed, before the first take.** Enable the SMTP Email Provider feature (it brings the Email feature), and save
   the SMTP settings so the site delivers to a local test inbox: an smtp4dev container in Docker. Leave Users
   Registration disabled; the first clip enables it. The visitors are not seeded: they register on camera. The commands
   and the exact values are in recording-plan.md.
3. **The sign-ins.** The admin clips use the saved admin sign-in. The three visitor clips must be anonymous, so they use
   a scene without that sign-in. The visitors' password is also chosen outside git.
4. **The order.** Record the clips in the order they play: enable the feature, look at the settings, register Jamie with
   every option off, select email verification and register Morgan (the inbox clip depends on it), clear it and select
   approval, register Riley, and enable Riley's account. Copy the data folder aside before each clip that changes the
   site, and restore it before recording a clip again.
5. **Narrate, then record.** Generate the narration first, then record the clips, then look at every contact sheet for
   errors and real data before assembling.
