# Secure Your Site: A Checklist for Administrators

**YouTube:** not published yet · **Length:** 6:25 · **Feature:** [guide:topics/security](https://docs.orchardcore.net/en/latest/topics/security/)

A checklist of the admin pages that decide how secure an Orchard Core site is, for site owners and administrators. It asks
four questions: who can do what, how people sign in, how connections and requests are protected, and what is kept safe and
recorded. For each page it says where it is in the admin menu and what to look for. The title, description, tags and
chapters YouTube shows are in the YouTube metadata file of this folder.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. It is an admin video: no clip
saves a setting, so every clip can be recorded again from the same snapshot. The recording plan in this folder holds the
exact actions of every step, and the commands to build and run the recorder.

1. Run the Orchard Core web project on port 5010 over plain HTTP, with its own empty data folder outside both repositories,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin, email
   admin@example.com, and a password chosen outside git.
2. Enable the features the recording plan lists, with Tools, Deployments, JSON Import. Then create the three fake users
   (and confirm their email addresses) and the two fake secrets it lists. Create the users after the features, so that the
   Audit Trail holds their events.
3. Sign the recorder in, so that the sign-in is also in the Audit Trail. Then stop the site and copy its data folder and the
   saved sign-in aside: restore them before every full take.
4. Narrate the video, then record all the clips. They do not depend on each other. Record them in the order of the
   storyboard.
