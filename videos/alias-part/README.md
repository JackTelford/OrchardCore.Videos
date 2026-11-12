# Alias Part: Give Content Items a Stable Name

**YouTube:** not published yet · **Length:** 4:20 · **Feature:** [OrchardCore.Alias](https://docs.orchardcore.net/en/latest/reference/modules/Alias/)

A walkthrough of the Alias part of Orchard Core on a site made with the Blog recipe: why a content item needs a name
that does not change from one site to another, how to check that the Alias feature is enabled, how to add the part to
the Article content type, how an alias is generated from the title, kept, and generated again, why each alias belongs
to one content item, the option that generates the alias and disables the field, the rules of an alias, and the alias
of the main menu at work. The title, description, tags and chapters YouTube shows are in the video's metadata file. The
narration is in the storyboard, and everything a person needs to record the video is in the recording plan of this
folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the content of the Blog recipe and one article made on
camera), so it needs no mask script.

1. Run the CMS web application of Orchard Core with its own empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the **Blog** recipe, site name **Orchard Core**, time zone America/Los_Angeles, user
   **admin** with the email admin@example.com, and a password chosen outside git. The Blog recipe already enables the
   Alias feature, so no feature is enabled or disabled before recording.
2. Sign the recorder in once, outside the capture, as the skill describes. Stop the site and copy its data folder aside,
   together with the saved sign-in (snapshot A). Record 02-feature and 03-add, then copy the data folder aside again
   (snapshot B): the clips 04-generate, 05-unique and 06-disabled start from it.
3. Narrate the video, then record the clips in the order 02-feature, 03-add, 04-generate, 05-unique, 06-disabled and
   08-use. The clips 03-add, 04-generate and 06-disabled change the site (the Alias part is added to the Article type,
   articles are created and published, and the option of the part is changed). Restore snapshot B before taking
   04-generate, 05-unique or 06-disabled again. The slide chapters (01-why and 07-rules) need no recording.
