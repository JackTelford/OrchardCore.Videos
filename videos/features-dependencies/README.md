# Features: Dependencies and Features That Enable Others

**YouTube:** not published yet · **Length:** 3:26 · **Feature:** [OrchardCore.Features](https://docs.orchardcore.net/en/latest/reference/modules/Features/)

The video explains how features depend on each other, with Users Registration and Email as the example. It shows the
dependency badges of the Features page, what happens to the dependencies when you enable a feature and to the dependents
when you disable one, and the features that have no working Enable or Disable button: on demand features and always
enabled features. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with no data of its own, so it needs no mask script. The step-by-step plan, with
the selectors and the source of every label, is in recording-plan.md.

1. Run a throwaway Orchard Core site (the CMS web application) on http://localhost:5010, with its own App_Data folder
   outside both repositories, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user admin. Configure **no email or SMTP setting** of any kind: enabling the Email feature would
   otherwise enable SMTP Email Provider as well, and the confirmation dialog of the disable clip would list two
   dependents.
2. Sign the recorder in with the admin user and the password chosen outside git. Enable nothing else: the Blog recipe
   already enables everything the clips need, and Users Registration and Email must start disabled.
3. Save a copy of the site and of the sign-in as snapshot **S0**. Narrate, then record the clips in this order:
   02-read, 05-demand, 03-enable. Save a second copy as snapshot **S1** (Users Registration and Email enabled),
   and record 04-disable from it.
4. 02-read and 05-demand only read the site. 03-enable and 04-disable change it: restore S0 before another take of
   03-enable, and S1 before another take of 04-disable.
