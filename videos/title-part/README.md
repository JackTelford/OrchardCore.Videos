# Title Part: Titles, Generated Titles and Display Text

**YouTube:** not published yet · **Length:** 4:30 · **Feature:** `OrchardCore.Title` ([Title documentation](https://docs.orchardcore.net/en/latest/reference/modules/Title/))

The video shows what a display text is, how the Title part gives a content type a title, and the four settings of its
Behavior option: a title the editor types, a title the editor must type, a title generated from a pattern with the
Title field disabled, and a title generated from a pattern with the Title field hidden. It adds the part to a Product
content type, creates content items with each setting, and checks the result in the content items list and on the site.
The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan
(recording-plan.md) holds the exact actions of every step, the commands, the selectors and the label sources.

1. Run a local Orchard Core site with its own data folder outside both repositories, provisioned by Auto Setup with the
   **Blog** recipe, the site name **Orchard Core**, the time zone America/Los_Angeles and the user admin with the email
   admin@example.com. The password is chosen outside git. The Blog recipe enables the Title, Contents, Content Types and
   Content Fields features, so none has to be enabled.
2. Sign the recorder in as admin, as the recording plan describes.
3. Seed the site in the admin, outside the capture: a **Product** content type with two Text fields, **Brand** and
   **Name**, and no Title part. Stop the site and copy its data folder aside as the first snapshot.
4. Narrate and record the clips in this order, each from the snapshot the plan names, since every clip changes the
   site: 02-attach, 03-typed, 04-generate, 05-hidden. Chapter 01 is slides and needs no recording.
