# Html and Markdown: Write the Body of Your Content

**YouTube:** not published yet · **Length:** 4:36 · **Feature:** OrchardCore.Html ([Html](https://docs.orchardcore.net/en/latest/reference/modules/Html/)) and OrchardCore.Markdown ([Markdown](https://docs.orchardcore.net/en/latest/reference/modules/Markdown/))

This video shows how a content type gets a body of text. It compares the HtmlBody part and the MarkdownBody part,
attaches HtmlBody to a content type and chooses its Wysiwyg editor, then writes and publishes an item with the Html
editor, looks at the Standard editor, and writes and publishes an item with the Markdown editor. It ends with the
Sanitize Html setting, how HTML in a Markdown body and shortcodes behave, and the Markdown field. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan holds
the exact actions, texts and labels of every step; this is the outline.

1. Run the Orchard Core site from an empty data folder outside both repositories, on port 5010, set up by Auto Setup:
   the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the user **admin** with the
   email admin@example.com. The password is chosen outside git. The Blog recipe already enables the Html and
   Markdown features.
2. Sign the recorder in. Then, outside the capture, import the two seed content types with **Tools, Deployments, JSON
   Import**: **Announcement**, which has a title and a route but no body part, and **Help Topic**, which has a title, a
   route and a MarkdownBody part that uses the Wysiwyg editor. The recording plan lists the text to paste. Stop the
   site and copy its data aside: this is the first snapshot.
3. Narrate the video, then record the clips in this order: **02-add**, **03-html**, **04-standard**, **05-markdown**.
   - 03-html needs 02-add, because Announcement has no body editor until 02-add attaches HtmlBody. Make a second
     snapshot after 02-add.
   - 02-add, 03-html and 05-markdown change the site (a part, and two published items). Restore the snapshot they
     start from before another take. 04-standard leaves the site as it found it.
4. Read every contact sheet, and check the video for private data before publishing, as the recording plan says.
