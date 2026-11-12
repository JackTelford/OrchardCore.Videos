# Themes: Change the Look of Your Site

**YouTube:** not published yet · **Length:** 4:24 · **Feature:** [OrchardCore.Themes](https://docs.orchardcore.net/en/latest/reference/modules/Themes/)

The video explains what a theme is, and how the Site theme and the Admin theme differ. It then walks through the Themes page
under Design: how a theme card reads, how Make Current changes the Site theme, how Enable and Disable switch a theme on
or off, what the Reset current themes box does, and which permission opens the page. The title, description, tags and
chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The page it shows lists themes,
not content, and the front end shows the content the Blog recipe creates, so there is no data to seed.

1. Build the web application of the Orchard Core source (the CMS web project) and run it with an empty data folder outside
   both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin, email admin@example.com. The password is chosen outside git and passed to the recorder
   in its password setting. Block the site's outbound calls, as the skill's recording reference describes. The recording
   machine itself needs internet access, because The Blog Theme loads its fonts from Google Fonts. Leave every setting at its
   default, including the theme toggler, which is on: the admin and The Default Theme show a "Toggle theme" button and follow
   the light color scheme of the recorder's browser.
2. Sign the recorder in once, then stop the site and copy its data folder and the sign-in file aside. This copy is the
   starting snapshot for every full take: The Blog Theme is the Site theme, The Admin Theme is the Admin theme, and The
   Default Theme, The Agency Theme and The Coming Soon Theme are disabled.
3. Two clips change the site. The clip that changes the Site theme enables The Default Theme and makes it current. The
   clip that follows starts from a second snapshot, the first one with that change made, and it makes The Blog Theme
   current again and disables The Default Theme, which brings the site back to the starting snapshot. Record the clips in
   the order of the storyboard, and restore the starting snapshot before every full take and before recording the first of
   the two clips again. The clip about the Reset current themes box opens a confirmation dialog and cancels it: it never
   confirms a reset.
4. Narrate first, then record. recording-plan.md lists the actions, the waits, the names to click and what must be visible
   at the end of each step. OPEN-QUESTIONS.md lists what a person must confirm on a running site.
