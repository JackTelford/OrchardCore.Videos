# Lists: Group Content Items in a Container

**YouTube:** not published yet · **Length:** 4:58 · **Feature:** [`OrchardCore.Lists`](https://docs.orchardcore.net/en/latest/reference/modules/Lists/)

A list is a content item that holds other content items, such as a blog and its posts or a help center and its
pages. The video shows the Blog as a list in the admin, creates a list type with the List part, creates a list and
adds pages to it, finds them by status and by title, and shows the list on the site. The title, description, tags
and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan
(recording-plan.md) holds every detail; in short:

1. Start a throwaway local site, provisioned by Auto Setup with the **Blog** recipe, the site name **Orchard Core**, the
   time zone America/Los_Angeles, and the user admin with the email admin@example.com. Its password is chosen
   outside git. The Blog recipe enables the Lists feature, so there is no feature to enable.
2. Sign the recorder in, stop the site, and keep a copy of its data (snapshot A).
3. Narrate, then record the clips in this order, restoring the snapshot the plan names before each take: the Blog
   page (02-blog), the list type (03-type), the list and its three pages (04-items), the search and filters
   (05-find), and the public page (06-site). The list type and the list clips change the site, and each clip depends
   on the data of the one before it.
4. Assemble the video, and check it for private data as the plan describes.
