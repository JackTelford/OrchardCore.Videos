# Rate Limits: Protect Your Site From Floods of Requests

**YouTube:** not published yet · **Length:** 6:10 · **Feature:** [OrchardCore.RateLimits](https://docs.orchardcore.net/en/latest/reference/modules/RateLimits/)

This video shows an administrator how to turn on the Rate Limits module, read the Default Global Policy it adds, and
create an Endpoint policy with a Fixed window limiter. It enables that policy, shows a request rejected with a 429 Too
Many Requests response, and manages policies: search, clone, disable and delete. It also covers the read-only limits that
features set, IP addresses and reverse proxies that make visitors share a budget, and moving policies between sites.
The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions, selectors
and screen checks of every clip are in recording-plan.md.

1. Run the Cms.Web project of an Orchard Core checkout that has the Rate Limits module, with its App_Data folder in an
   empty folder outside both repositories, on http://localhost:5010. Provision it with Auto Setup: the Blog recipe,
   site name **Orchard Core**, time zone America/Los_Angeles, user admin, email admin@example.com, and the password chosen
   outside git. Block outbound calls with a dead proxy address. Do not turn on Rate Limits: the first clip does it on camera.
2. Sign the recorder in as admin once, then stop the site and copy its App_Data folder and the sign-in file aside. This is
   snapshot A. Three more snapshots are taken while recording (after the first, third and fourth clips); the table in
   recording-plan.md says which clip starts from which.
3. Narrate the video, then record the clips in the order they depend on each other: 02-enable, 03-default, 06-create,
   07-test, 08-manage, 09-readonly. The clip 07-test sends four requests to the blog inside one 60 second window, and it
   enables a policy, which restarts the site: restore snapshot C before every take of it. 08-manage deletes the policy
   that 06-create made, so restore snapshot D before every take of that one.
4. The chapters 01, 04, 05, 10 and 11 are slides, drawn by slides.py; they need no recording.
