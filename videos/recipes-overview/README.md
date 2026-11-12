# Recipes: Set Up and Update Sites From a File

**YouTube:** not published yet · **Length:** 4:25 · **Feature:** `OrchardCore.Recipes` ([Recipes](https://docs.orchardcore.net/en/latest/reference/modules/Recipes/))

An overview of recipes for the people who run an Orchard Core site. It explains what a recipe is, shows the Recipe list
of the Setup screen and the Recipes page under Tools, runs a recipe and checks the result, and ends with the Manage
Recipes permission. It shows the Setup screen and the admin only, never the file of a recipe. The recording plan says how
to record each clip, and the list of open questions says what a person must confirm on a running site.

## Recording it again

The video uses two throwaway sites built from the web application of OrchardCore, each with its own data folder outside
both repositories, and fake data only, so it needs no mask script.

1. **Main site** (used by the clips 03, 04 and 05): provisioned by Auto Setup with the Blog recipe, site name
   **Orchard Core**, time zone America/Los_Angeles, user admin, email admin@example.com, and a password chosen
   outside git. Enable the **Admin Dashboard** feature (Tools, Features) before recording; nothing else is seeded.
2. **Setup site** (used by the clip 02): the same build with an empty data folder, started without Auto Setup, so it shows
   the Setup screen. Nothing is ever submitted on it, and no sign-in is saved for it.
3. Sign the recorder in on the main site only, keep the saved sign-in out of git, and copy the data folder of the main
   site aside after the seed: restore that copy before every full take.
4. Narrate, then record in this order: 02-setup on the setup site, then 05-permission, 03-browse and 04-run on
   the main site. Only 04-run changes the site (it adds a dashboard widget), so restore the copy before another take.
   The slide chapters (01-what, 04-care) are drawn by the slides script and need no recording.

The commands, the exact actions of every step and the checks for private data are in the recording plan.
