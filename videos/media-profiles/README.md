# Media Profiles: Resize and Crop Images Consistently

**YouTube:** not published yet · **Length:** 4:59 · **Feature:** [OrchardCore.Media](https://docs.orchardcore.net/en/latest/reference/modules/Media/)

A media profile gives an image size a name: image settings such as a width, a height, a resize mode, a format and a
quality, saved once under Media, Profiles and asked for by name by a theme. The video explains why profiles exist, tours the Media Profiles page,
explains the resize modes Max, Crop, Pad and Stretch, and creates a thumbnail profile. It then changes the banner
profile that the Blog recipe creates, and shows the result in the header of the home page and of a blog post. It is
for site owners and administrators, and it shows the admin screens and the front end only. The title, description,
tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build the web project of Orchard Core and run it with its data folder in an empty place outside both repositories,
   on port 5010. Set the site up with Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   **America/Los_Angeles**, the user **admin**, the email **admin@example.com**, and a password chosen outside git and
   passed to the recorder in the environment. The Blog recipe is all the seed data the video needs: it enables the Media
   feature, creates the **banner** media profile, and adds the sample photos used by the home page, the blog post and
   the About page. Nothing is created by hand before the first clip.
2. Sign the recorder in once, then stop the site and copy its data folder and the saved sign-in aside. Restore that copy
   before every full take.
3. Narrate, then record the clips in this order, because each one starts from the state the one before it left: the
   Media Profiles page (02-list), creating a profile (04-create), the profile on the site (05-site), and search and
   delete (06-manage). Creating a profile adds **thumbnail**, the site clip changes **banner**, and the last clip
   deletes **thumbnail**, so a single clip is recorded again from the saved copy, with the earlier changes made by hand
   off camera.

The file recording-plan.md in this folder lists every action, every value to type, what must be on screen at the end
of each step, and where each label comes from in the Orchard Core source. OPEN-QUESTIONS.md lists what a person must
confirm on the running site before the narration is recorded.
