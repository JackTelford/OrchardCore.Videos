# User Time Zone and Culture: Personal Preferences for Each User

**YouTube:** not published yet · **Length:** 3:41 · **Feature:** [OrchardCore.Users.TimeZone](https://docs.orchardcore.net/en/latest/reference/modules/Users/) and [OrchardCore.Users.Localization](https://docs.orchardcore.net/en/latest/reference/modules/Users/)

The User Time Zone and User Localization features of the Users module add a Default Time Zone field and a Default User
Culture field to every user. The video enables both features, checks the cultures the site supports, sets a time zone
and a culture for the signed-in administrator, and shows the same blog post with a different day and a different month
name. It ends with the order in which the user's choice, the site's settings and the server apply. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video was recorded on a fresh site with fake data only, so it needs no mask script. Everything the recorder needs
is in recording-plan.md: the site, the seeds, the order of the clips, and every action with the frame expected at the
end of each step.

1. Run the web project of Orchard Core on a fresh site, with its own data folder outside both repositories, on port
   5010, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone America/Los_Angeles, user admin
   with the email admin@example.com. The password is chosen outside git and given to the recorder when it signs in.
2. Sign the recorder in. Then seed the site with three imports under Tools, Deployments, JSON Import: enable the
   Localization feature, set the supported cultures (English as the default, and French), and create one fake blog post
   with a fixed creation time. Stop the site, and keep a copy of its data and of the sign-in aside as the first
   snapshot. Restore it before every full take.
3. Narrate, then record the clips in this order: the preparation clip, which enables the two features; the clip that
   edits the administrator, which saves the time zone and the culture; and, after signing the recorder in again so
   that the sign-in carries the culture just saved, the clip that shows the blog post. Each of the first two clips changes the
   site, so keep a copy of the data from before each of them, and restore the matching copy before a new take of that
   clip. The two clips made of slides need nothing from the site.
