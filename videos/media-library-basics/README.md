# Media Library: Upload, Organize and Reuse Files

**YouTube:** not published yet · **Length:** 6:00 · **Feature:** [OrchardCore.Media](https://docs.orchardcore.net/en/latest/reference/modules/Media/MediaGallery/)

The video shows the media library of the admin: how to open it, upload files, organize them in folders, rename, move,
copy and delete them, and find them with the filter, the sort headings and the pager. It then picks a file from the
library in the Banner Image field of an Article, and ends with the optional features that extend the library. The
title, description, tags and chapters YouTube shows are kept in the video's metadata file.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run the web application of an Orchard Core checkout with its data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and
   the user admin with the email admin@example.com and a password chosen outside git. The Blog recipe enables the Media
   feature and puts three theme pictures in the library. Media Slugify, Media TUS Uploads, Media SignalR and Secure
   Media stay disabled.
2. Make the fake files and seed the library with the folders and files the recording plan lists: five files in the
   root folder, a folder with one file, and a folder with twelve photos. Then stop the site and copy its data folder
   aside. This is the first snapshot.
3. Sign the recorder in, narrate the video, and record the clips in the order they play: tour, upload, folders, files,
   find and reuse. The upload, folders and files clips change the site, so copy the data folder aside after each of
   them, and restore the matching snapshot before recording one of them again. The files clip needs the files
   uploaded by the upload clip and the folder created by the folders clip. The reuse clip needs the file moved by the
   files clip, and never saves the Article.

The recording plan holds everything else: the script that makes the fake files, the exact actions and the selectors of
every step, the state of the library before each clip, and the checks for private data.
