# Import: Package and JSON Import

**YouTube:** not published yet · **Length:** 3:37 · **Feature:** OrchardCore.Deployment ([Deployment](https://docs.orchardcore.net/en/latest/reference/modules/Deployment/))

The video shows how to run a recipe on the site you are signed in to, with the two import pages of the Deployment module. It imports a zip package with Package Import and a pasted recipe with JSON Import, explains what travels with each, what happens when a step fails, and which permission controls the pages. The title, description, tags and chapters that YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build the web project of Orchard Core and run it with an empty data folder outside both repositories, on port 5010, set up by Auto Setup with the Blog recipe (site name **Orchard Core**, time zone America/Los_Angeles, user **admin**). The Blog recipe enables the Deployment feature, so nothing has to be enabled by hand.
2. Prepare two things outside the capture, both described in the recording plan: a small zip package that adds one article called **Spring Menu**, and the text of a recipe that renames the site to **Maple Street Bakery**. Neither is committed. Import both once on a throw-away copy of the site first, as the recording plan asks.
3. Sign the recorder in with the password chosen outside git, narrate, then record the two clips, **Package Import** and **JSON Import**. The other chapters are slides and need no recording.

Both clips change the site: the first adds the article, the second renames the site. Take a snapshot of the data folder and the sign-in before the first clip, and restore it before repeating a take. The two clips do not depend on each other, so they can be recorded in either order. The recording plan holds the exact actions of every step, the labels and where they come from, and the checks for private data.
