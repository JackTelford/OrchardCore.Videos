# Secure Media: Control Who Sees Each Media Folder

**YouTube:** not published yet · **Length:** 4:26 · **Feature:** [OrchardCore.Media.Security](https://docs.orchardcore.net/en/latest/reference/modules/Media/#secure-media)

This video is for site owners and administrators. It shows what the Secure Media feature does to a public Orchard Core
site: files in the Media Library stop being open to everyone, and view permissions decide who can open the root folder
and each folder under it. It enables the feature, opens the root folder to visitors with the Anonymous role, and allows
the Editor role to view one folder. The title, description, tags and chapters YouTube shows are in the video's
metadata file, and the exact steps for the recorder are in recording-plan.md.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run the web application of Orchard Core (src/OrchardCore.Cms.Web) with its own data folder outside both
   repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, the site name Orchard Core, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com and a password chosen outside git. The Blog
   recipe enables the Media and Roles features, creates the Editor role, and puts the pictures of its pages in the
   root folder of the Media Library. Secure Media must be disabled when recording starts.
2. Seed the site: create two empty folders, press and contracts, in the root folder of the Media Library (Media,
   Library). Sign the recorder in as admin, stop the site, and copy its data folder aside as snapshot A.
3. Narrate, then record the clips in the order of the storyboard: 02-visitor-before, 02-enable, 02-visitor-after,
   04-anonymous, 04-visitor, 05-editor. Three clips change the site (02-enable, 04-anonymous and 05-editor), so take a
   snapshot after the first two and restore the right one before each take. Three clips are recorded as a visitor
   who is not signed in. The recording plan names the snapshot every clip starts from, and the actions of every step.
