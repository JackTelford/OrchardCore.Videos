# Media Uploads: Slugified Names, Resumable Uploads and Live Updates

**YouTube:** not published yet · **Length:** 5:09 · **Feature:** [OrchardCore.Media.Slugify](https://docs.orchardcore.net/en/latest/reference/modules/Media.Slugify/), with [OrchardCore.Media.Tus](https://docs.orchardcore.net/en/latest/reference/modules/Media/MediaGallery/) and [OrchardCore.Media.SignalR](https://docs.orchardcore.net/en/latest/reference/modules/Media/MediaGallery/)

This video shows three optional features of the Media module and the problem each one solves. Media Slugify gives the
folders you create and the files you upload clean names. Media TUS Uploads sends a large file in parts, so you can pause
it, resume it, and continue it after the page is reloaded. Media SignalR keeps the file list of every open Media Library up
to date. It ends with what to plan for when a site runs on several servers. The title, description, tags and chapters
YouTube shows are in the YouTube metadata file of this folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder lists every step of every clip, with what must be on screen when its narration ends, and the OPEN-QUESTIONS file
lists what a person must confirm on the running site.

1. Build the web project of Orchard Core from a checkout that holds the three features and run it with an empty data
   folder outside both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**,
   time zone America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git.
2. Sign the recorder in. No feature has to be enabled beforehand: the video enables Media Slugify, Media TUS Uploads and
   Media SignalR on camera. Make the fake files described in the plan (four pictures and two files of 25,000,000
   bytes, all outside the repository), and seed the folder **Team Photos** with its picture
   through the Media Library while Media Slugify is still disabled. Stop the site and copy its data folder aside (snapshot S0).
3. Narrate, then record in this order: the chapter that enables the features, the chapter that creates the folder and
   uploads the pictures, then the resumable uploads and the live updates. Restore the copy of the data folder before
   every full take. The live updates and the resumable uploads both start from the state the slugified names chapter leaves
   behind (snapshot S2) and do not depend on each other.
4. The resumable uploads need the speed of the upload to be limited, and the live updates need two browser windows on
   the same site. Both are described in the plan.
