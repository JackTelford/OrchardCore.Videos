# Google Analytics and Tag Manager: Measure Your Visitors

**YouTube:** not published yet · **Length:** 3:53 · **Feature:** [OrchardCore.Google.Analytics](https://docs.orchardcore.net/en/latest/reference/modules/Google/) and [OrchardCore.Google.TagManager](https://docs.orchardcore.net/en/latest/reference/modules/Google/)

An admin video about the two Google features that measure the visitors of a site. It shows what each feature adds to
the public pages, how to enable them under Tools, Features, how to save a tracking ID under Settings, Integrations,
Google Analytics and a container ID under Settings, Integrations, Google Tag Manager, and where the scripts load on the
site. It ends with the cases in which a script is left out and how to stop one. The title, description, tags and
chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The two IDs typed on camera
are the placeholders of the documentation, and the site itself calls no outside service.

1. Start a site of the Orchard Core source named in recording-plan.md, provisioned by Auto Setup with the Blog recipe,
   the site name Orchard Core, the time zone America/Los_Angeles and the user admin, with an empty folder for its data
   outside both repositories. The password is chosen outside git and is never written in this folder. Both Google
   features must be disabled: the first clip enables them. There is no other seed data to create.
2. Sign the recorder in once, and copy the site's data folder aside as snapshot A.
3. Narrate the video, then record the clips in the order they play: the clip that enables the features, the Google
   Analytics clip, the Google Tag Manager clip, and the clip on the public site. The first three change the site, so
   copy the data folder aside after each one, and restore the snapshot a clip starts from before recording it again.
   The two slide chapters are drawn from the slides file and need no recording.
4. Run the checks at the end of recording-plan.md: the scripts are on the public pages and not in the admin, no real
   ID or personal data is on screen, and the contact sheet of every clip shows what its step describes.

recording-plan.md holds the exact actions, the values to type, what must be on screen when each step ends, and where
each label comes from in the Orchard Core source. OPEN-QUESTIONS.md lists what a person must confirm on a running site.
