# Admin Culture Picker and the Content-Language Header

**YouTube:** not published yet · **Length:** 3:44 · **Feature:** `OrchardCore.Localization.AdminCulturePicker` and `OrchardCore.Localization.ContentLanguageHeader` ([Localization module](https://docs.orchardcore.net/en/latest/reference/modules/Localize/))

The Admin Culture Picker adds a drop-down to the admin top bar, so you choose the culture of the admin pages in your
browser while the front end keeps the culture of the site. The Content Language Header adds a Content-Language header
to the responses of the site. The video enables both features, adds a second supported culture so the picker appears,
chooses the admin culture, and explains what the header carries. The title, description, tags and chapters YouTube
shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder is the complete script of the recording: the site, the seed, the order of the clips and, for each step, the
actions and what the screen must show.

1. Start from a fresh local site, provisioned by Auto Setup with the Blog recipe, the site name **Orchard Core**, the
   time zone America/Los_Angeles, and the user **admin** with the email admin@example.com. The language the operating
   system was installed with is English (United States). When Auto Setup has finished and the site is stopped, put the small French
   translation file of the recording plan in the Localization folder of the Default tenant's data folder: it gives the admin a few labels in
   French. Keep a copy of the site's data folder with the file in it, then start the site again.
2. Sign the recorder in. The recording needs no preparation step: the features are enabled on camera.
3. Narrate, then record the clips in the order **enable, cultures, pick**: each one changes the site or depends on the
   previous one. Keep a copy of the site's data folder before each clip, and restore the copy made before a clip to
   record it again. The slides are drawn by the assembly step and need no recording.
