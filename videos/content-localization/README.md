# Content Localization: Translate Your Content Items

**YouTube:** not published yet · **Length:** 5:42 · **Feature:** [OrchardCore.ContentLocalization](https://docs.orchardcore.net/en/latest/reference/modules/ContentLocalization/)

The video shows how the Content Localization module keeps one page in several languages: each translation is a content
item of its own, linked to the others by the Localization part. It adds the part to the Article content type, saves an
existing article so it receives its culture, creates French and Spanish translations from the editor and from the
content items list, filters the list by culture, opens the translations on the site, and ends with the two permissions
that control who can translate. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every command, locator and
label is in recording-plan.md, which a person follows without making any choice.

1. Build the Orchard Core web project and run it with an empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user
   **admin**. The password is chosen outside git and given to the recorder through the environment.
2. Seed the site, outside the capture: enable the Localization feature (not Content Localization), then set the
   supported cultures to English (United States), French (France) and Spanish (Spain), with English (United States) as
   the default culture. Stop the site and copy its data folder and the saved sign-in aside as the starting snapshot.
3. Narrate, then record the clips in this order: 02-enable, 03-part, 04-original, 05-translate, 06-manage, 07-site,
   08-permissions. Each clip starts from the state the previous one left. The first five change the site, so restore
   the snapshot of the clip before when a clip is recorded again, and restore the starting snapshot for a full take.
   Chapter 01 is made of slides and needs no recording. The role is only read in the last clip: it is never saved.
