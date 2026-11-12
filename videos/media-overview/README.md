# Media: Manage the Files of Your Site

**YouTube:** not published yet · **Length:** 4:13 · **Feature:** [OrchardCore.Media](https://docs.orchardcore.net/en/latest/reference/modules/Media/)

An overview of the Media module for site owners and editors. It explains why a site keeps its files in one library, shows
where the module lives in the admin (the Media menu, the Settings entries and the optional features), tours the Media
Library (folders, the file table, the grid, the search box, uploads and the file and folder menus), and follows a file from
the library into a media field and onto a page of the site. The title, description, tags and chapters YouTube shows are in
the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The step-by-step actions, the
selectors and the source of every label are in recording-plan.md.

1. Run a fresh Orchard Core site on a local port, with its data folder outside both repositories, and let Auto Setup
   create it: the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin with the
   email admin@example.com. The password is chosen outside git and given to the recorder; it is never written in this
   repository. Block outbound calls so the site contacts no external service.
2. Seed nothing and enable no feature. The Blog recipe provides everything the video shows: the Media module, three
   images in the root folder of the library, the banner media profile, the Article content type with its Banner Image
   field, and the About page.
3. Sign the recorder in and load the Media Library once, so that the folder the site creates for the signed-in user
   exists. Then copy the site's data folder aside: that copy is the snapshot to restore, and it already holds that
   folder. Narrate the video.
4. Record the three clips: where it lives, the Media Library, and media in content. They are independent of each other
   and change nothing on the site (a clip uploads no file, creates no folder, enables no feature and saves no content
   item), so they can be recorded in any order and again at will. Restore the copy of the data folder only if a take
   went wrong.
5. Before assembling, check the three images and every frame for private data, as the plan describes.
