# Content Types: Model the Content of Your Site

**YouTube:** not published yet · **Length:** 5:12 · **Feature:** [OrchardCore.ContentTypes](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/)

An overview of the content model of Orchard Core, for site owners and editors. The video explains what a content item, a content type, a content part and a content field are, then tours the Content Types page, the inside of the Article type (its settings, fields and parts), the editor and the page that the type produces, and the Content Parts page (documented in [Content Parts](https://docs.orchardcore.net/en/latest/reference/modules/ContentParts/)). It changes nothing on the site. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script.

1. Build the Orchard Core web project of the CMS and run it on port 5010, with its own App_Data folder outside both repositories, provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles and the user admin (email admin@example.com). The password is chosen outside git and given to the recorder in the environment; it is not written in this folder. No feature needs to be enabled besides the ones the recipe enables.
2. Sign the recorder in. Then create the seed data by hand in the admin, outside the capture: one content part named Contact Details with two fields, Phone (a Text Field) and Website (a Link Field). The exact steps are in the recording plan. Stop the site and copy its App_Data folder and the saved sign-in aside as the snapshot.
3. Narrate, then record the four clips of the recording plan: the Content Types page, the inside of the Article type, the editor and the About page, and the Content Parts page. No clip saves anything, so they can be recorded in any order and as often as needed. The Content Parts clip is the only one that depends on the seed data.
4. The recording plan lists, for every step, the actions, what must be on screen when its narration ends, and the place in the Orchard Core source where each label comes from. Open questions that a person must confirm on a running site are in the questions file of this folder.
