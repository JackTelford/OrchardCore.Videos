# Autoroute: Friendly URLs for Your Content Items

**YouTube:** not published yet · **Length:** 4:46 · **Feature:** [OrchardCore.Autoroute](https://docs.orchardcore.net/en/latest/reference/modules/Autoroute/)

This video is for site owners and editors. It shows why a content item needs a readable address, how to attach the
Autoroute part to a content type, what each Autoroute setting decides, and how an editor creates an item, keeps its
permalink when the title changes, refreshes it, or writes one by hand. The title, description, tags and chapters
YouTube shows are in video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The full, step by step plan is
in recording-plan.md, and the questions a person must confirm on a running site are in OPEN-QUESTIONS.md.

- **Site:** a local Orchard Core site provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the
  time zone America/Los_Angeles, and the user admin (admin@example.com). The password is chosen outside git; the
  recording plan says where the recorder reads it.
- **Features:** none to enable. The Blog recipe turns Autoroute on.
- **Seed:** one content type, Event, made of a title and nothing else (no Autoroute part), and no Event item. Save
  a copy of the site's data as the first snapshot.
- **Order:** the four recorded clips depend on each other and are recorded in the order they play: 02-add, then
  03-settings, then 04-create, then 05-change. Each one changes the site, so take a snapshot after each clip and
  restore the matching snapshot before recording a clip again.
- **Narrate first:** run the narration step of the skill before recording, since the recorder holds each step for the
  length of its narration.
