# Features: Enable and Disable What Your Site Uses

**YouTube:** not published yet · **Length:** 4:08 · **Feature:** [OrchardCore.Features](https://docs.orchardcore.net/en/latest/reference/modules/Features/)

The video explains what a feature is and what an enabled feature adds to a site. It walks through the Features page under
Tools: how the list is grouped, how to search and filter it, how to enable and disable one feature, how to change several
features with Bulk Actions, and which permission opens the page. The title, description, tags and chapters YouTube shows
are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The page it shows lists
features, not content, so there is no data to seed.

1. Build the web application of the Orchard Core source (the CMS web project) and run it with an empty data folder outside
   both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin, email admin@example.com. The password is chosen outside git and passed to the recorder
   in its password setting. Block outbound calls, as the skill's recording reference describes.
2. Sign the recorder in once, then stop the site and copy its data folder and the sign-in file aside. This copy is the
   starting snapshot for every full take.
3. Three clips change the site: enabling Background Tasks, disabling it, and enabling Google Analytics and Google Tag
   Manager. Record the clips in the order of the storyboard. The clip that disables Background Tasks starts from a snapshot
   in which it is enabled, which the clip before it leaves behind. Restore the starting snapshot before every full take and
   before recording again the clip that enables Background Tasks or the clip that enables the Google features. To record
   the disabling clip again on its own, restore a second snapshot instead: the starting one with Background Tasks enabled.
   recording-plan.md describes both.
4. Narrate first, then record. recording-plan.md lists the actions, the waits, the names to click and what must be visible
   at the end of each step.
