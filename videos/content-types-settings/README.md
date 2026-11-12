# Content Type Settings: Creatable, Listable, Draftable and More

**YouTube:** not published yet · **Length:** 4:57 · **Feature:** [OrchardCore.ContentTypes](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/)

The video covers the settings on the Edit Content Type page: the Creatable, Listable, Draftable, Versionable and
Securable options, and the Stereotype, Category and Thumbnail settings. Creatable, Listable, Draftable and Securable
are cleared in the admin, a draft of a published item shows what Versionable keeps, and a category and a thumbnail are
filled in. The effects appear in Manage Content, in the content editor, in the permissions of a role and in the
widget picker. The title, description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact seed data, the
selectors and the actions of every step are in recording-plan.md; questions that a person must settle on a running
site are in OPEN-QUESTIONS.md.

1. Build the web project of Orchard Core and run it with an empty data folder outside both repositories, on port 5010.
   Auto Setup provisions it with the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles
   and the user **admin** with the email admin@example.com. The password is chosen outside git and given to the
   recorder through its password variable. Point the proxy settings at a dead address, so the site never calls out.
2. Seed the site in the admin, outside the capture: the **Event** content type (display name Event, the Title part,
   a short description), three published Event items, and the **Blocks** editor on the FlowPart of the Page type. Do
   not enable any feature beyond the ones the Blog recipe enables.
3. Stop the site and copy its data folder aside as the baseline. Start the site again and sign the recorder in.
4. Narrate the video, then record the four clips: the visibility clip, the drafts clip, the securable clip and the
   pickers clip. Every clip changes the site and none depends on another, so stop the site and restore the baseline
   copy before every take.
