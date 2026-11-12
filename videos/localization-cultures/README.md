# Cultures: Make Your Site Multilingual

**YouTube:** not published yet · **Length:** 4:46 · **Feature:** [OrchardCore.Localization](https://docs.orchardcore.net/en/latest/reference/modules/Localize/)

This video explains what a culture is, and what the supported cultures and the default culture of a site are. It enables
the Localization feature in Tools, Features, adds cultures on the Cultures page under Settings, chooses the default,
saves, and asks for French with the culture parameter to see a culture at work. It ends with the localization features
that build on the list of cultures. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan,
recording-plan.md, has every step; this is the outline.

1. Run the web project of an Orchard Core checkout with its own App_Data folder outside both repositories, set up by Auto
   Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin
   (admin@example.com, with a password chosen outside git). Use a machine whose operating system was installed in
   English (United States): a site starts with that language as its only culture.
2. Put the sample French translation file, whose text is in the recording plan, in the Localization folder of the default
   tenant's App_Data folder before starting the site. No feature needs to be enabled in advance: the video enables
   Localization on camera.
3. Sign the recorder in, narrate the video, and record the three clips in this order, restoring a copy of App_Data between
   takes: enable (changes the site), cultures (changes the site, and needs the previous clip), and french (changes
   nothing, and needs the cultures clip). The slides and the cards are made from the storyboard.

The commands are those of the video producer skill (SKILL.md and references/recording.md in the skill's folder).
