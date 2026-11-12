# Rules: Conditions That Decide When a Layer Shows

**YouTube:** not published yet · **Length:** 5:36 · **Feature:** [OrchardCore.Rules](https://docs.orchardcore.net/en/latest/reference/modules/Rules/)

An administrator video about the rule of a layer. It explains that a rule is a list of conditions that must all be
true, tours the conditions a rule can hold and the operations that compare text, and then builds a rule on the edit page
of a layer: an Any group that holds a Homepage condition and a Url condition, checked on three pages of the site, and an
Is authenticated condition added to the rule itself. It ends with editing, reordering, the Content type limit and the
Manage layers permission. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything the person who
records needs, step by step, is in [recording-plan.md](recording-plan.md); the points that still need a check on a
running site are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. **Site.** Build the web project of Orchard Core and start it on port 5010 with an empty data folder outside both
   repositories, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user **admin**. The password is chosen outside git. Block outbound calls with a dead proxy
   address.
2. **Features.** Nothing to enable: the Blog recipe turns on Layers, Rules, Widgets and Deployment.
3. **Seed.** Outside the capture, import the small recipe given in the plan with **Tools**, **Deployments**,
   **JSON Import**. It creates the layer **Welcome**, which has no conditions, and the widget **Welcome note** in the
   **Content** zone. Sign the recorder in once, then copy the data folder and the saved sign-in aside as the starting
   snapshot.
4. **Narrate, then record.** Narrate the video first, then record the five recording chapters in this order, because
   each starts from the site the previous one left:
   03-open, 04-group, 05-conditions, 06-site, 07-visitor.
   04-group, 05-conditions and 07-visitor change the site. Copy the data folder aside after 04-group and after
   05-conditions, and restore the right copy before another take of a clip. The slide chapters and the cards need no
   recording.
5. **Check.** Look at every contact sheet, then scan the video for private data as the skill describes. The only
   account is **admin**, and no email address appears on screen.
