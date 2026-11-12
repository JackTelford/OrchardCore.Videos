# Forms: Validate What Visitors Type

**YouTube:** not published yet · **Length:** 4:38 · **Feature:** [OrchardCore.Forms](https://docs.orchardcore.net/en/latest/reference/modules/Forms/)

How the Forms module validates what visitors type. A workflow checks the submitted values and records an error for each
one that fails; the form displays the errors with a Validation Summary widget and the Validation option of its inputs;
and the workflow sends the visitor back to the form with the messages and the values already typed. The video is for
site owners and editors: it shows the admin screens and the front end, and no code. The title, description, tags and
chapters YouTube shows are in the metadata file of the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. recording-plan.md holds
everything a person needs: the site, the seed data, the order of the clips, and the actions of every step. The
recorder, the narration and the assembly work as the skill describes.

1. Run a fresh Orchard Core site on port 5010 with its own data folder outside both repositories, provisioned by Auto
   Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin, email
   admin@example.com.
2. Enable the **Forms**, **Workflows** and **HTTP Workflows Activities** features, then create the seed data of the
   plan, in the admin screens and outside the capture: a Thank You page, a Contact Form page with a Form widget (a Name
   input, an Email input, a Message text area and a Submit button), and a workflow named Contact Form Validation that
   holds only its HTTP Request start event, whose URL is the Action of the form.
3. Sign the recorder in, stop the site, and copy its data folder aside: restore that copy before every full take.
4. Narrate, then record the clips in this order: 02-messages (the Contact Form page), 03-rules (the workflow,
   ending with its publication) and 04-try (the front end). The third clip needs the other two, so retaking either
   of them means restoring the copy and recording both again before 04-try.
