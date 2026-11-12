# Content Parts: Create Reusable Groups of Fields

**YouTube:** not published yet · **Length:** 4:34 · **Feature:** `OrchardCore.ContentTypes` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/))

A content part is a group of fields that you define once and attach to content types. The video creates a Postal
Address part on the Content Parts page, sets its display name, description, Attachable and Reusable options, and adds
three Text Fields. It then attaches the part twice to a Customer type with Add Named Part, as Billing Address and
Shipping Address, and fills in a customer to show that each name keeps its own values. The YouTube title,
description, tags and chapters are kept with the video; the exact steps to record are in the recording plan.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run the Orchard Core CMS web project on http://localhost:5010, with its data folder empty and outside both
   repositories, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles,
   user admin, email admin@example.com. The Blog recipe enables every feature the video needs, so none is enabled by
   hand.
2. Sign the recorder in as admin once, with the password chosen outside git. The session is saved in a git-ignored
   file.
3. Seed the site once, outside the capture: create the content type **Customer** with the **Title** part, as the
   recording plan describes. Stop the site and copy its data folder aside as the first snapshot.
4. Narrate the video, then record the clips 02-create, 03-settings, 04-fields, 05-attach and 06-content, in this
   order. Each clip starts from the state the previous one left, so take a snapshot after each clip, and restore the
   one before it to record a clip again.
5. The slides (01-idea) and the two cards are drawn when the video is assembled and need no recording.
